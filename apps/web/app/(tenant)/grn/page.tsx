'use client';

import { useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { Search } from 'lucide-react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient, lkr } from '@/lib/api-client';
import { Icon } from '@/components/ui/Icon';
import { SearchableSelect } from '@/components/ui/SearchableSelect';
import { Pagination } from '@/components/ui/Pagination';

type Location = { id: string; code: string; name: string };

type GrnRow = {
  id: string;
  grnNumber: string;
  supplierId: string;
  supplierName: string;
  locationId: string;
  locationCode: string;
  locationName: string;
  purchaseOrderId: string | null;
  poNumber: string | null;
  status: string;
  receivedDate: string;
  totalCost: number;
  taxAmount: number;
  discountAmount: number;
  otherCharges: number;
  netAmount: number;
  supplierInvoiceNo: string | null;
  referenceNo: string | null;
  postedAt: string | null;
  approvedAt: string | null;
  rejectedAt: string | null;
  rejectReason: string | null;
  fulfilledRequestNoteNumbers: string[];
};

type PagedResponse = {
  data: GrnRow[];
  pagination: { totalCount: number; pageNumber: number; pageSize: number; totalPages: number };
};

const STATUS_PILL: Record<string, string> = {
  draft: 'pill-idle',
  pending: 'pill-pending',
  approved: 'pill-paid',
  rejected: 'pill-void',
  void: 'pill-void',
  // legacy value, no longer created — kept so old rows still render sensibly
  posted: 'pill-paid',
};
const STATUS_LABEL: Record<string, string> = {
  draft: 'Draft',
  pending: 'Pending approval',
  approved: 'Approved',
  rejected: 'Rejected',
  void: 'Void',
  posted: 'Posted',
};

const SUBMITTABLE = new Set(['draft']);
const PENDING = new Set(['pending']);
const VOIDABLE = new Set(['approved']);
const REMOVABLE = new Set(['draft', 'pending', 'rejected']);

// ---- Submit / Approve / Reject / Void / Remove — one confirm modal --------

type ActionKind = 'submit' | 'approve' | 'reject' | 'void' | 'remove';

const ACTION_CONFIG: Record<ActionKind, {
  endpoint: (id: string) => string;
  method: 'POST' | 'DELETE';
  title: string;
  description: (g: GrnRow) => string;
  confirmLabel: string;
  submittingLabel: string;
  closeLabel: string;
  danger: boolean;
  showReason: boolean;
  successMessage: (g: GrnRow) => string;
}> = {
  submit: {
    endpoint: id => `/api/v1/grn/${id}/submit`, method: 'POST',
    title: 'Submit GRN',
    description: g =>
      `This submits ${g.grnNumber} for processing. Depending on your approval settings it either posts straight through (adds stock, recomputes weighted-average cost), or waits as Pending until someone approves it.`,
    confirmLabel: 'Submit', submittingLabel: 'Submitting…', closeLabel: 'Not yet',
    danger: false, showReason: false, successMessage: g => `GRN ${g.grnNumber} submitted`,
  },
  approve: {
    endpoint: id => `/api/v1/grn/${id}/approve`, method: 'POST',
    title: 'Approve GRN',
    description: g => `This posts ${g.grnNumber} — adds stock and recomputes weighted-average cost.`,
    confirmLabel: 'Approve', submittingLabel: 'Approving…', closeLabel: 'Not now',
    danger: false, showReason: false, successMessage: g => `GRN ${g.grnNumber} approved and posted`,
  },
  reject: {
    endpoint: id => `/api/v1/grn/${id}/reject`, method: 'POST',
    title: 'Reject GRN',
    description: g => `This rejects ${g.grnNumber}. It never touched stock, so there's nothing to reverse.`,
    confirmLabel: 'Reject', submittingLabel: 'Rejecting…', closeLabel: "Don't reject",
    danger: true, showReason: true, successMessage: g => `GRN ${g.grnNumber} rejected`,
  },
  void: {
    endpoint: id => `/api/v1/grn/${id}/void`, method: 'POST',
    title: 'Void GRN',
    description: g => `This reverses the stock added by ${g.grnNumber} and rolls back its linked PO's received quantities. This cannot be undone.`,
    confirmLabel: 'Void GRN', submittingLabel: 'Voiding…', closeLabel: 'Keep GRN',
    danger: true, showReason: true, successMessage: g => `GRN ${g.grnNumber} voided`,
  },
  remove: {
    endpoint: id => `/api/v1/grn/${id}`, method: 'DELETE',
    title: 'Remove GRN',
    description: g => `This removes ${g.grnNumber} from the list. Only possible before it's ever posted to stock.`,
    confirmLabel: 'Remove', submittingLabel: 'Removing…', closeLabel: 'Keep it',
    danger: true, showReason: false, successMessage: g => `GRN ${g.grnNumber} removed`,
  },
};

async function fetchSupplierOptions(args: { page: number; pageSize: number; search: string }) {
  const qs = new URLSearchParams({
    pageNumber: String(args.page), pageSize: String(args.pageSize), isActive: 'true',
  });
  if (args.search) qs.set('search', args.search);
  const res = await apiClient<{ data: { id: string; name: string }[]; pagination: { totalPages: number } }>(
    `/api/v1/suppliers/paged?${qs}`,
  );
  return {
    items: res.data.map(s => ({ id: s.id, label: s.name })),
    hasMore: args.page < res.pagination.totalPages,
  };
}

export default function GrnListPage() {
  const router = useRouter();
  const searchParams = useSearchParams();

  const [rows, setRows] = useState<GrnRow[]>([]);
  const [locations, setLocations] = useState<Location[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [search, setSearch] = useState('');
  const [supplierId, setSupplierId] = useState('');
  const [supplierLabel, setSupplierLabel] = useState('');
  const [locationId, setLocationId] = useState('');
  const [status, setStatus] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const [actionFor, setActionFor] = useState<{ grn: GrnRow; kind: ActionKind } | null>(null);
  const [toast, setToast] = useState<string | null>(null);

  const flash = (m: string) => {
    setToast(m);
    setTimeout(() => setToast(null), 3000);
  };

  useEffect(() => {
    apiClient<Location[]>('/api/v1/locations?all=true').then(setLocations).catch(() => {});
  }, []);

  // A newly-created GRN redirects here with its number + resulting status in the URL.
  useEffect(() => {
    const createdGrnNumber = searchParams.get('justCreated');
    if (!createdGrnNumber) return;
    const createdStatus = searchParams.get('status');
    const message =
      createdStatus === 'pending'
        ? `GRN ${createdGrnNumber} saved — awaiting approval before it posts to stock`
        : createdStatus === 'approved'
          ? `GRN ${createdGrnNumber} posted`
          : `GRN ${createdGrnNumber} saved as draft`;
    flash(message);
    router.replace('/grn');
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchParams]);

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const qs = new URLSearchParams({ pageNumber: String(pageNumber), pageSize: String(pageSize) });
      if (search) qs.set('search', search);
      if (supplierId) qs.set('supplierId', supplierId);
      if (locationId) qs.set('locationId', locationId);
      if (status) qs.set('status', status);
      if (dateFrom) qs.set('dateFrom', dateFrom);
      if (dateTo) qs.set('dateTo', dateTo);
      const res = await apiClient<PagedResponse>(`/api/v1/grn/paged?${qs}`);
      setRows(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(Math.max(1, res.pagination.totalPages));
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pageNumber, pageSize, supplierId, locationId, status, dateFrom, dateTo]);

  useEffect(() => {
    const t = setTimeout(() => {
      setPageNumber(1);
      load();
    }, 350);
    return () => clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search]);

  const start = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const end = Math.min(pageNumber * pageSize, totalCount);

  return (
    <>
      <Topbar title="Goods Received Notes" subtitle="Every GRN posted across locations" />

      <div className="p-6">
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Goods received notes</h2>
            <p className="text-sm text-muted-foreground">{totalCount} GRN{totalCount === 1 ? '' : 's'}</p>
          </div>
          <button
            onClick={() => router.push('/grn/create')}
            className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark"
          >
            <Icon name="add" className="text-base" /> New GRN
          </button>
        </div>

        <div className="card mb-4 grid grid-cols-2 gap-3 p-4 md:grid-cols-7">
          <div className="relative col-span-2">
            <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
            <input
              value={search}
              onChange={e => setSearch(e.target.value)}
              placeholder="Search GRN #, invoice #, ref #, supplier…"
              className="w-full rounded-lg border border-border bg-card py-2 pl-9 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
            />
          </div>
          <SearchableSelect
            value={supplierId}
            selectedLabel={supplierLabel}
            placeholder="All suppliers"
            fetchPage={fetchSupplierOptions}
            onChange={(id, option) => {
              setSupplierId(id);
              setSupplierLabel(option?.label ?? '');
              setPageNumber(1);
            }}
          />
          <select
            value={locationId}
            onChange={e => { setLocationId(e.target.value); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">All locations</option>
            {locations.map(l => (
              <option key={l.id} value={l.id}>{l.code} — {l.name}</option>
            ))}
          </select>
          <select
            value={status}
            onChange={e => { setStatus(e.target.value); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">All statuses</option>
            <option value="draft">Draft</option>
            <option value="pending">Pending approval</option>
            <option value="approved">Approved</option>
            <option value="rejected">Rejected</option>
            <option value="void">Void</option>
          </select>
          <div className="col-span-2 flex min-w-0 items-center gap-2">
            <input
              type="date"
              value={dateFrom}
              onChange={e => { setDateFrom(e.target.value); setPageNumber(1); }}
              className="w-full min-w-0 rounded-lg border border-border bg-card px-2 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
              title="From date"
            />
            <input
              type="date"
              value={dateTo}
              onChange={e => { setDateTo(e.target.value); setPageNumber(1); }}
              className="w-full min-w-0 rounded-lg border border-border bg-card px-2 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
              title="To date"
            />
          </div>
        </div>

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
                  <th className="px-4 py-2.5 font-medium">GRN #</th>
                  <th className="px-4 py-2.5 font-medium">Supplier</th>
                  <th className="px-4 py-2.5 font-medium">Location</th>
                  <th className="px-4 py-2.5 font-medium">PO #</th>
                  <th className="px-4 py-2.5 font-medium">Status</th>
                  <th className="px-4 py-2.5 font-medium">Received</th>
                  <th className="px-4 py-2.5 text-right font-medium">Cost value</th>
                  <th className="px-4 py-2.5 text-right font-medium">Net amount</th>
                  <th className="px-4 py-2.5 text-right font-medium">Actions</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((g, i) => {
                  const submittable = SUBMITTABLE.has(g.status);
                  const pendingApproval = PENDING.has(g.status);
                  const voidable = VOIDABLE.has(g.status);
                  const removable = REMOVABLE.has(g.status);
                  const hasAction = submittable || pendingApproval || voidable || removable;
                  return (
                    <tr key={g.id} className={i % 2 ? 'bg-muted/20' : ''}>
                      <td className="px-4 py-2.5 font-mono text-xs text-muted-foreground">{g.grnNumber}</td>
                      <td className="px-4 py-2.5 font-medium">{g.supplierName}</td>
                      <td className="px-4 py-2.5">
                        <span className="pill pill-idle">{g.locationCode}</span>
                      </td>
                      <td className="px-4 py-2.5 font-mono text-xs text-muted-foreground">
                        {g.poNumber ?? '—'}
                        {g.fulfilledRequestNoteNumbers.length > 0 && (
                          <span
                            className="ml-1.5 rounded-full bg-blue-50 px-1.5 py-0.5 font-sans text-[10px] font-medium text-blue-800"
                            title={`Fulfilled request note${g.fulfilledRequestNoteNumbers.length === 1 ? '' : 's'}: ${g.fulfilledRequestNoteNumbers.join(', ')}`}
                          >
                            ← {g.fulfilledRequestNoteNumbers.join(', ')}
                          </span>
                        )}
                      </td>
                      <td className="px-4 py-2.5">
                        <span className={`pill ${STATUS_PILL[g.status] ?? 'pill-idle'}`}>
                          {STATUS_LABEL[g.status] ?? g.status}
                        </span>
                      </td>
                      <td className="px-4 py-2.5 text-muted-foreground">
                        {g.receivedDate ? new Date(g.receivedDate).toLocaleDateString('en-LK') : '—'}
                      </td>
                      <td className="px-4 py-2.5 text-right tabular-nums">{lkr(g.totalCost)}</td>
                      <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{lkr(g.netAmount)}</td>
                      <td className="px-4 py-2.5 text-right">
                        <div className="flex flex-wrap items-center justify-end gap-2">
                          {submittable && (
                            <button
                              onClick={() => setActionFor({ grn: g, kind: 'submit' })}
                              className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted"
                            >
                              <Icon name="send" className="text-sm" /> Submit
                            </button>
                          )}
                          {pendingApproval && (
                            <>
                              <button
                                onClick={() => setActionFor({ grn: g, kind: 'approve' })}
                                className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-primary hover:bg-muted"
                              >
                                <Icon name="check_circle" className="text-sm" /> Approve
                              </button>
                              <button
                                onClick={() => setActionFor({ grn: g, kind: 'reject' })}
                                className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-status-error hover:bg-muted"
                              >
                                <Icon name="cancel" className="text-sm" /> Reject
                              </button>
                            </>
                          )}
                          {voidable && (
                            <button
                              onClick={() => setActionFor({ grn: g, kind: 'void' })}
                              className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-status-error hover:bg-muted"
                            >
                              <Icon name="cancel" className="text-sm" /> Void
                            </button>
                          )}
                          {removable && (
                            <button
                              onClick={() => setActionFor({ grn: g, kind: 'remove' })}
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
                {rows.length === 0 && (
                  <tr>
                    <td colSpan={9} className="px-4 py-10 text-center text-muted-foreground">
                      No GRNs match these filters.
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
            className="rounded-lg border border-border bg-card px-2 py-1.5 text-xs"
          >
            {[10, 25, 50, 100].map(n => (
              <option key={n} value={n}>{n} / page</option>
            ))}
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={start} to={end} setPage={setPageNumber} noun="GRNs" className="mt-0 flex-1" />
        </div>
      </div>

      {actionFor && (
        <ConfirmActionModal
          grn={actionFor.grn}
          kind={actionFor.kind}
          onClose={() => setActionFor(null)}
          onDone={async message => {
            setActionFor(null);
            await load();
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
// Confirm action modal — Submit / Approve / Reject / Void / Remove (see ACTION_CONFIG)
// ---------------------------------------------------------------------------

function ConfirmActionModal({
  grn,
  kind,
  onClose,
  onDone,
}: {
  grn: GrnRow;
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
      await apiClient(cfg.endpoint(grn.id), init);
      await onDone(cfg.successMessage(grn));
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
          <p className="text-sm text-muted-foreground">{cfg.description(grn)}</p>
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
