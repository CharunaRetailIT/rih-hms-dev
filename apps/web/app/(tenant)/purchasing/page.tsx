'use client';

import { useEffect, useMemo, useState } from 'react';
import { useRouter } from 'next/navigation';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient, lkr } from '@/lib/api-client';
import { Icon } from '@/components/ui/Icon';
import { Pagination } from '@/components/ui/Pagination';

type PurchaseOrder = {
  id: string;
  poNumber: string;
  supplierId: string;
  supplierName?: string;
  locationId: string;
  locationCode?: string;
  // draft|pending|approved|rejected|partially_received|received
  status: string;
  orderDate: string;
  totalAmount: number;
};

type Supplier = { id: string; name: string };
type Location = { id: string; code: string; name: string };
type PaginationMeta = { totalCount: number; pageNumber: number; pageSize: number; totalPages: number };
type PagedResponse = { data: PurchaseOrder[]; pagination: PaginationMeta };

// ---- Status helpers -------------------------------------------------------

const STATUS_PILL: Record<string, string> = {
  draft: 'pill-idle',
  pending: 'pill-pending',
  approved: 'pill-paid',
  rejected: 'pill-void',
  partially_received: 'pill-pending',
  received: 'pill-paid',
  // legacy values, no longer created — kept so old rows still render sensibly
  sent: 'pill-progress',
  cancelled: 'pill-void',
};

const STATUS_LABEL: Record<string, string> = {
  draft: 'Draft',
  pending: 'Pending approval',
  approved: 'Approved',
  rejected: 'Rejected',
  partially_received: 'Partially received',
  received: 'Received',
  sent: 'Sent',
  cancelled: 'Cancelled',
};

function statusPill(status: string) {
  return STATUS_PILL[status] ?? 'pill-idle';
}
function statusLabel(status: string) {
  return STATUS_LABEL[status] ?? status;
}

const EDITABLE = new Set(['draft']);
const SUBMITTABLE = new Set(['draft']);
const PENDING = new Set(['pending']);
const RECEIVABLE = new Set(['approved', 'partially_received']);
const REMOVABLE = new Set(['draft', 'pending', 'approved', 'rejected']);

// ---- Submit / Approve / Reject / Remove — one confirm modal, four configs --

type ActionKind = 'submit' | 'approve' | 'reject' | 'remove';

const ACTION_CONFIG: Record<ActionKind, {
  endpoint: (id: string) => string;
  method: 'POST' | 'DELETE';
  title: string;
  description: (po: PurchaseOrder) => string;
  confirmLabel: string;
  submittingLabel: string;
  closeLabel: string;
  danger: boolean;
  showReason: boolean;
  successMessage: string;
}> = {
  submit: {
    endpoint: id => `/api/v1/purchase-orders/${id}/submit`, method: 'POST',
    title: 'Submit purchase order',
    description: po =>
      `This submits ${po.poNumber} for processing. Depending on your approval settings it either posts straight through as Approved, or waits as Pending until someone approves it.`,
    confirmLabel: 'Submit', submittingLabel: 'Submitting…', closeLabel: 'Not yet',
    danger: false, showReason: false, successMessage: 'Purchase order submitted',
  },
  approve: {
    endpoint: id => `/api/v1/purchase-orders/${id}/approve`, method: 'POST',
    title: 'Approve purchase order',
    description: po => `This approves ${po.poNumber} and clears it for receiving.`,
    confirmLabel: 'Approve', submittingLabel: 'Approving…', closeLabel: 'Not now',
    danger: false, showReason: false, successMessage: 'Purchase order approved',
  },
  reject: {
    endpoint: id => `/api/v1/purchase-orders/${id}/reject`, method: 'POST',
    title: 'Reject purchase order',
    description: po => `This rejects ${po.poNumber}. It's final — create a new PO if it needs to be resubmitted.`,
    confirmLabel: 'Reject order', submittingLabel: 'Rejecting…', closeLabel: "Don't reject",
    danger: true, showReason: true, successMessage: 'Purchase order rejected',
  },
  remove: {
    endpoint: id => `/api/v1/purchase-orders/${id}`, method: 'DELETE',
    title: 'Remove purchase order',
    description: po => `This removes ${po.poNumber} from the list. Only possible before anything's been received.`,
    confirmLabel: 'Remove', submittingLabel: 'Removing…', closeLabel: 'Keep it',
    danger: true, showReason: false, successMessage: 'Purchase order removed',
  },
};

export default function PurchasingPage() {
  const router = useRouter();
  const [orders, setOrders] = useState<PurchaseOrder[]>([]);
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);
  const [locations, setLocations] = useState<Location[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [toast, setToast] = useState<string | null>(null);

  const [actionFor, setActionFor] = useState<{ po: PurchaseOrder; kind: ActionKind } | null>(null);

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const flash = (m: string) => {
    setToast(m);
    setTimeout(() => setToast(null), 3000);
  };

  async function loadOrders() {
    try {
      const params = new URLSearchParams({ pageNumber: String(pageNumber), pageSize: String(pageSize) });
      const res = await apiClient<PagedResponse>(`/api/v1/purchase-orders/paged?${params.toString()}`);
      setOrders(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages);
    } catch (e) {
      setError((e as Error).message);
    }
  }

  useEffect(() => {
    (async () => {
      try {
        const [, s, l] = await Promise.all([
          loadOrders(),
          apiClient<Supplier[]>('/api/v1/suppliers'),
          apiClient<Location[]>('/api/v1/locations?all=true'),
        ]);
        setSuppliers(s);
        setLocations(l);
      } catch (e) {
        setError((e as Error).message);
      } finally {
        setLoading(false);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);
  useEffect(() => { if (!loading) void loadOrders(); }, [pageNumber, pageSize]);   // eslint-disable-line react-hooks/exhaustive-deps

  const supplierName = (po: PurchaseOrder) => po.supplierName ?? suppliers.find(s => s.id === po.supplierId)?.name ?? '—';
  const locationCode = (po: PurchaseOrder) => po.locationCode ?? locations.find(l => l.id === po.locationId)?.code ?? '—';

  return (
    <>
      <Topbar title="Purchasing" subtitle="Purchase orders & receiving" />

      <div className="p-6">
        {/* Header row */}
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Purchase orders</h2>
            <p className="text-sm text-muted-foreground">
              {totalCount} order{totalCount === 1 ? '' : 's'} ·{' '}
              {suppliers.length} supplier{suppliers.length === 1 ? '' : 's'}
            </p>
          </div>
          <button
            onClick={() => router.push('/purchasing/create')}
            className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark"
          >
            <Icon name="add" className="text-base" /> New PO
          </button>
        </div>

        {/* Table */}
        <div className="card overflow-hidden">
          {loading ? (
            <div className="space-y-2 p-4">
              {Array.from({ length: 8 }).map((_, i) => (
                <div key={i} className="h-9 animate-pulse rounded bg-muted" />
              ))}
            </div>
          ) : error ? (
            <div className="p-6 text-sm text-status-error">{error}</div>
          ) : (
            <table className="w-full text-sm">
              <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th className="px-4 py-2.5 font-medium">PO number</th>
                  <th className="px-4 py-2.5 font-medium">Supplier</th>
                  <th className="px-4 py-2.5 font-medium">Location</th>
                  <th className="px-4 py-2.5 font-medium">Status</th>
                  <th className="px-4 py-2.5 text-right font-medium">Total</th>
                  <th className="px-4 py-2.5 font-medium">Date</th>
                  <th className="px-4 py-2.5 text-right font-medium">Actions</th>
                </tr>
              </thead>
              <tbody>
                {orders.map((po, i) => {
                  const editable = EDITABLE.has(po.status);
                  const submittable = SUBMITTABLE.has(po.status);
                  const pendingApproval = PENDING.has(po.status);
                  const receivable = RECEIVABLE.has(po.status);
                  const removable = REMOVABLE.has(po.status);
                  const hasAction = editable || submittable || pendingApproval || receivable || removable;
                  return (
                    <tr key={po.id} className={i % 2 ? 'bg-muted/20' : ''}>
                      <td className="px-4 py-2.5 font-mono text-xs text-muted-foreground">
                        {po.poNumber}
                      </td>
                      <td className="px-4 py-2.5 font-medium">{supplierName(po)}</td>
                      <td className="px-4 py-2.5">
                        <span className="pill pill-idle">{locationCode(po)}</span>
                      </td>
                      <td className="px-4 py-2.5">
                        <span className={`pill ${statusPill(po.status)}`}>
                          {statusLabel(po.status)}
                        </span>
                      </td>
                      <td className="px-4 py-2.5 text-right font-semibold tabular-nums">
                        {lkr(po.totalAmount)}
                      </td>
                      <td className="px-4 py-2.5 text-muted-foreground">
                        {po.orderDate ? new Date(po.orderDate).toLocaleDateString('en-LK') : '—'}
                      </td>
                      <td className="px-4 py-2.5 text-right">
                        <div className="flex flex-wrap items-center justify-end gap-2">
                          {editable && (
                            <button
                              onClick={() => router.push(`/purchasing/${po.id}/edit`)}
                              className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted"
                            >
                              <Icon name="edit" className="text-sm" /> Edit
                            </button>
                          )}
                          {submittable && (
                            <button
                              onClick={() => setActionFor({ po, kind: 'submit' })}
                              className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted"
                            >
                              <Icon name="send" className="text-sm" /> Submit
                            </button>
                          )}
                          {pendingApproval && (
                            <>
                              <button
                                onClick={() => setActionFor({ po, kind: 'approve' })}
                                className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-primary hover:bg-muted"
                              >
                                <Icon name="check_circle" className="text-sm" /> Approve
                              </button>
                              <button
                                onClick={() => setActionFor({ po, kind: 'reject' })}
                                className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-status-error hover:bg-muted"
                              >
                                <Icon name="cancel" className="text-sm" /> Reject
                              </button>
                            </>
                          )}
                          {receivable && (
                            <button
                              onClick={() => router.push(`/grn/create?poId=${po.id}`)}
                              className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted"
                            >
                              <Icon name="inventory_2" className="text-sm" /> Receive
                            </button>
                          )}
                          {removable && (
                            <button
                              onClick={() => setActionFor({ po, kind: 'remove' })}
                              className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-status-error hover:bg-muted"
                            >
                              <Icon name="delete" className="text-sm" /> Remove
                            </button>
                          )}
                          {!hasAction && <span className="text-xs text-muted-foreground">—</span>}
                        </div>
                      </td>
                    </tr>
                  );
                })}
                {orders.length === 0 && (
                  <tr>
                    <td colSpan={7} className="px-4 py-10 text-center text-muted-foreground">
                      No purchase orders yet. Create one with “+ New PO”.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-surface px-2 py-1.5 text-xs"
          >
            {[10, 25, 50, 100].map(n => <option key={n} value={n}>{n} / page</option>)}
          </select>
          <Pagination
            page={pageNumber}
            totalPages={totalPages}
            total={totalCount}
            from={totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1}
            to={Math.min(pageNumber * pageSize, totalCount)}
            setPage={setPageNumber}
            noun="orders"
            className="mt-0 flex-1"
          />
        </div>

        <p className="mt-3 text-xs text-muted-foreground">
          Totals in LKR. Receiving a PO posts a GRN and updates weighted-average cost.
        </p>
      </div>

      {actionFor && (
        <ConfirmActionModal
          po={actionFor.po}
          kind={actionFor.kind}
          onClose={() => setActionFor(null)}
          onDone={async message => {
            setActionFor(null);
            await loadOrders();
            flash(message);
          }}
        />
      )}

      {toast && (
        <div className="fixed bottom-12 left-1/2 z-[70] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">
          {toast}
        </div>
      )}
    </>
  );
}

// ---------------------------------------------------------------------------
// Confirm action modal — Submit / Approve / Reject / Remove (see ACTION_CONFIG)
// ---------------------------------------------------------------------------

function ConfirmActionModal({
  po,
  kind,
  onClose,
  onDone,
}: {
  po: PurchaseOrder;
  kind: ActionKind;
  onClose: () => void;
  onDone: (message: string) => void | Promise<void>;
}) {
  const cfg = ACTION_CONFIG[kind];
  const [reason, setReason] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setError(null);
    setSubmitting(true);
    try {
      const init: RequestInit = { method: cfg.method };
      if (cfg.showReason) init.body = JSON.stringify({ reason: reason || null });
      await apiClient(cfg.endpoint(po.id), init);
      await onDone(cfg.successMessage);
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div
      className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 p-4"
      onClick={onClose}
    >
      <div
        className="card w-full max-w-md overflow-hidden bg-card"
        onClick={e => e.stopPropagation()}
      >
        <div className="flex items-center justify-between border-b border-border px-5 py-3.5">
          <h3 className="font-heading text-lg font-bold">{cfg.title}</h3>
          <button
            onClick={onClose}
            className="rounded-lg p-1.5 text-muted-foreground hover:bg-muted"
            title="Close"
          >
            <Icon name="close" className="text-xl" />
          </button>
        </div>

        <div className="space-y-3 px-5 py-4">
          <p className="text-sm text-muted-foreground">{cfg.description(po)}</p>
          {cfg.showReason && (
            <div>
              <label className="mb-1 block text-sm font-semibold text-slate-700">Reason (optional)</label>
              <textarea
                value={reason}
                onChange={e => setReason(e.target.value)}
                rows={3}
                placeholder="Add a note…"
                className="w-full rounded-lg border border-border bg-card px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
              />
            </div>
          )}
          {error && (
            <div className="rounded-lg bg-red-50 px-3 py-2 text-sm text-status-error">{error}</div>
          )}
        </div>

        <div className="flex items-center justify-end gap-2 border-t border-border px-5 py-3.5">
          <button
            onClick={onClose}
            className="rounded-lg border border-border bg-card px-4 py-2 text-sm font-medium hover:bg-muted"
          >
            {cfg.closeLabel}
          </button>
          <button
            onClick={submit}
            disabled={submitting}
            className={`rounded-lg px-4 py-2 text-sm font-medium text-white hover:opacity-90 disabled:opacity-60 ${
              cfg.danger ? 'bg-status-error' : 'bg-primary'
            }`}
          >
            {submitting ? cfg.submittingLabel : cfg.confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}
