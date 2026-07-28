'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { Search } from 'lucide-react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient } from '@/lib/api-client';
import { Icon } from '@/components/ui/Icon';
import { Pagination } from '@/components/ui/Pagination';

type Location = { id: string; code: string; name: string };

type RequestNoteRow = {
  id: string;
  requestNumber: string;
  mode: string;
  fromLocationId: string;
  fromLocationCode: string;
  fromLocationName: string;
  toLocationId: string;
  toLocationCode: string;
  toLocationName: string;
  expectedDeliveryDate: string;
  status: string;
  commonRemark: string | null;
  submittedAt: string | null;
  approvedAt: string | null;
  rejectedAt: string | null;
  rejectReason: string | null;
  purchaseOrderId: string | null;
  transferId: string | null;
  createdAt: string;
};

type PagedResponse = {
  data: RequestNoteRow[];
  pagination: { totalCount: number; pageNumber: number; pageSize: number; totalPages: number };
};

const STATUS_PILL: Record<string, string> = {
  draft: 'pill-idle',
  pending: 'pill-pending',
  approved: 'pill-paid',
  rejected: 'pill-void',
  fulfilled: 'pill-paid',
};
const STATUS_LABEL: Record<string, string> = {
  draft: 'Draft',
  pending: 'Pending approval',
  approved: 'Approved',
  rejected: 'Rejected',
  fulfilled: 'Fulfilled',
};
const MODE_LABEL: Record<string, string> = {
  po: 'PO',
  transfer: 'Transfer',
};

const SUBMITTABLE = new Set(['draft']);
const PENDING = new Set(['pending']);
const REMOVABLE = new Set(['draft', 'pending', 'rejected']);

type ActionKind = 'submit' | 'approve' | 'reject' | 'remove';

const ACTION_CONFIG: Record<ActionKind, {
  endpoint: (id: string) => string;
  method: 'POST' | 'DELETE';
  title: string;
  description: (r: RequestNoteRow) => string;
  confirmLabel: string;
  submittingLabel: string;
  closeLabel: string;
  danger: boolean;
  showReason: boolean;
  successMessage: (r: RequestNoteRow) => string;
}> = {
  submit: {
    endpoint: id => `/api/v1/request-notes/${id}/submit`, method: 'POST',
    title: 'Submit request note',
    description: r =>
      `This submits ${r.requestNumber} for processing. Depending on your approval settings it either resolves straight to Approved, or waits as Pending until someone approves it.`,
    confirmLabel: 'Submit', submittingLabel: 'Submitting…', closeLabel: 'Not yet',
    danger: false, showReason: false, successMessage: r => `${r.requestNumber} submitted`,
  },
  approve: {
    endpoint: id => `/api/v1/request-notes/${id}/approve`, method: 'POST',
    title: 'Approve request note',
    description: r => `This approves ${r.requestNumber}, authorising it to be picked up by a purchase order or transfer.`,
    confirmLabel: 'Approve', submittingLabel: 'Approving…', closeLabel: 'Not now',
    danger: false, showReason: false, successMessage: r => `${r.requestNumber} approved`,
  },
  reject: {
    endpoint: id => `/api/v1/request-notes/${id}/reject`, method: 'POST',
    title: 'Reject request note',
    description: r => `This rejects ${r.requestNumber}.`,
    confirmLabel: 'Reject', submittingLabel: 'Rejecting…', closeLabel: "Don't reject",
    danger: true, showReason: true, successMessage: r => `${r.requestNumber} rejected`,
  },
  remove: {
    endpoint: id => `/api/v1/request-notes/${id}`, method: 'DELETE',
    title: 'Remove request note',
    description: r => `This removes ${r.requestNumber} from the list. Only possible before it's approved.`,
    confirmLabel: 'Remove', submittingLabel: 'Removing…', closeLabel: 'Keep it',
    danger: true, showReason: false, successMessage: r => `${r.requestNumber} removed`,
  },
};

export default function RequestNotesListPage() {
  const router = useRouter();

  const [rows, setRows] = useState<RequestNoteRow[]>([]);
  const [locations, setLocations] = useState<Location[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [search, setSearch] = useState('');
  const [fromLocationId, setFromLocationId] = useState('');
  const [toLocationId, setToLocationId] = useState('');
  const [mode, setMode] = useState('');
  const [status, setStatus] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const [actionFor, setActionFor] = useState<{ r: RequestNoteRow; kind: ActionKind } | null>(null);
  const [toast, setToast] = useState<string | null>(null);
  const [selected, setSelected] = useState<Set<string>>(new Set());

  const flash = (m: string) => {
    setToast(m);
    setTimeout(() => setToast(null), 3000);
  };

  useEffect(() => {
    apiClient<Location[]>('/api/v1/locations?all=true').then(setLocations).catch(() => {});
  }, []);

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const qs = new URLSearchParams({ pageNumber: String(pageNumber), pageSize: String(pageSize) });
      if (search) qs.set('search', search);
      if (fromLocationId) qs.set('fromLocationId', fromLocationId);
      if (toLocationId) qs.set('toLocationId', toLocationId);
      if (mode) qs.set('mode', mode);
      if (status) qs.set('status', status);
      if (dateFrom) qs.set('dateFrom', dateFrom);
      if (dateTo) qs.set('dateTo', dateTo);
      const res = await apiClient<PagedResponse>(`/api/v1/request-notes/paged?${qs}`);
      setRows(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(Math.max(1, res.pagination.totalPages));
      setSelected(new Set());
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  }

  // "po"-mode notes bundle into a Purchase Order; "transfer"-mode notes bundle
  // into a Transfer of Goods — both from this same selection list.
  function isBundleable(r: RequestNoteRow) {
    return (r.mode === 'po' || r.mode === 'transfer') && r.status === 'approved';
  }

  function toggleSelected(id: string) {
    setSelected(prev => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  const selectedRows = rows.filter(r => selected.has(r.id));
  const bundleModeMismatch =
    selectedRows.length > 1 && selectedRows.some(r => r.mode !== selectedRows[0].mode);
  // A PO only cares about the From (buying) location; a Transfer moves stock between
  // an exact From/To pair, so both locations have to match across the selection.
  const bundleLocationMismatch =
    selectedRows.length > 1 &&
    selectedRows.some(r =>
      r.fromLocationId !== selectedRows[0].fromLocationId ||
      (selectedRows[0].mode === 'transfer' && r.toLocationId !== selectedRows[0].toLocationId));
  const bundleBlocked = bundleModeMismatch || bundleLocationMismatch;

  function createFromSelected() {
    if (selectedRows.length === 0 || bundleBlocked) return;
    const path = selectedRows[0].mode === 'transfer' ? '/transfers/create' : '/purchasing/create';
    router.push(`${path}?requestNoteIds=${selectedRows.map(r => r.id).join(',')}`);
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pageNumber, pageSize, fromLocationId, toLocationId, mode, status, dateFrom, dateTo]);

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
      <Topbar title="Request Notes" subtitle="Requisitions fulfilled by a purchase order or a transfer" />

      <div className="p-6">
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Request notes</h2>
            <p className="text-sm text-muted-foreground">{totalCount} note{totalCount === 1 ? '' : 's'}</p>
          </div>
          <button
            onClick={() => router.push('/request-notes/create')}
            className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark"
          >
            <Icon name="add" className="text-base" /> New Request
          </button>
        </div>

        <div className="card mb-4 grid grid-cols-2 gap-3 p-4 md:grid-cols-8">
          <div className="relative col-span-2">
            <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
            <input
              value={search}
              onChange={e => setSearch(e.target.value)}
              placeholder="Search doc #, remark…"
              className="w-full rounded-lg border border-border bg-card py-2 pl-9 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
            />
          </div>
          <select
            value={fromLocationId}
            onChange={e => { setFromLocationId(e.target.value); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">From: any</option>
            {locations.map(l => (
              <option key={l.id} value={l.id}>{l.code} — {l.name}</option>
            ))}
          </select>
          <select
            value={toLocationId}
            onChange={e => { setToLocationId(e.target.value); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">To: any</option>
            {locations.map(l => (
              <option key={l.id} value={l.id}>{l.code} — {l.name}</option>
            ))}
          </select>
          <select
            value={mode}
            onChange={e => { setMode(e.target.value); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">All modes</option>
            {Object.entries(MODE_LABEL).map(([v, l]) => (
              <option key={v} value={v}>{l}</option>
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
            <option value="fulfilled">Fulfilled</option>
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

        {selected.size > 0 && (
          <div className="card mb-4 flex flex-wrap items-center justify-between gap-3 p-3">
            <div className="text-sm">
              <span className="font-medium">{selected.size} selected</span>
              {bundleModeMismatch && (
                <span className="ml-2 text-status-error">Selected notes must all be the same mode (PO or Transfer).</span>
              )}
              {!bundleModeMismatch && bundleLocationMismatch && (
                <span className="ml-2 text-status-error">
                  {selectedRows[0]?.mode === 'transfer'
                    ? 'Selected notes must share the same From and To locations.'
                    : 'Selected notes must share the same From location.'}
                </span>
              )}
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setSelected(new Set())}
                className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted"
              >
                Clear
              </button>
              <button
                onClick={createFromSelected}
                disabled={bundleBlocked}
                className="inline-flex items-center gap-1 rounded-lg bg-primary px-3 py-1.5 text-xs font-medium text-primary-foreground hover:bg-primary-dark disabled:opacity-50"
              >
                <Icon name="add" className="text-sm" />
                Create {selectedRows[0]?.mode === 'transfer' ? 'Transfer' : 'PO'} from selected
              </button>
            </div>
          </div>
        )}

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
                  <th className="w-8 px-4 py-2.5" />
                  <th className="px-4 py-2.5 font-medium">Doc #</th>
                  <th className="px-4 py-2.5 font-medium">Mode</th>
                  <th className="px-4 py-2.5 font-medium">From</th>
                  <th className="px-4 py-2.5 font-medium">To</th>
                  <th className="px-4 py-2.5 font-medium">Status</th>
                  <th className="px-4 py-2.5 font-medium">Expected</th>
                  <th className="px-4 py-2.5 font-medium">Date</th>
                  <th className="px-4 py-2.5 text-right font-medium">Actions</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((r, i) => {
                  const editable = r.status === 'draft';
                  const submittable = SUBMITTABLE.has(r.status);
                  const pendingApproval = PENDING.has(r.status);
                  const removable = REMOVABLE.has(r.status);
                  const hasAction = editable || submittable || pendingApproval || removable;
                  return (
                    <tr key={r.id} className={i % 2 ? 'bg-muted/20' : ''}>
                      <td className="px-4 py-2.5">
                        {isBundleable(r) && (
                          <input
                            type="checkbox"
                            checked={selected.has(r.id)}
                            onChange={() => toggleSelected(r.id)}
                            className="size-4 rounded border-border text-primary focus:ring-primary"
                            title="Select for bundling into a PO or Transfer"
                          />
                        )}
                      </td>
                      <td className="px-4 py-2.5 font-mono text-xs text-muted-foreground">{r.requestNumber}</td>
                      <td className="px-4 py-2.5">
                        <span className="pill pill-idle">{MODE_LABEL[r.mode] ?? r.mode}</span>
                      </td>
                      <td className="px-4 py-2.5">
                        <span className="pill pill-idle">{r.fromLocationCode}</span>
                      </td>
                      <td className="px-4 py-2.5">
                        <span className="pill pill-idle">{r.toLocationCode}</span>
                      </td>
                      <td className="px-4 py-2.5">
                        <span className={`pill ${STATUS_PILL[r.status] ?? 'pill-idle'}`}>
                          {STATUS_LABEL[r.status] ?? r.status}
                        </span>
                      </td>
                      <td className="px-4 py-2.5 text-muted-foreground">
                        {r.expectedDeliveryDate ? new Date(r.expectedDeliveryDate).toLocaleDateString('en-LK') : '—'}
                      </td>
                      <td className="px-4 py-2.5 text-muted-foreground">
                        {r.createdAt ? new Date(r.createdAt).toLocaleDateString('en-LK') : '—'}
                      </td>
                      <td className="px-4 py-2.5 text-right">
                        <div className="flex flex-wrap items-center justify-end gap-2">
                          {editable && (
                            <button
                              onClick={() => router.push(`/request-notes/${r.id}/edit`)}
                              className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted"
                            >
                              <Icon name="edit" className="text-sm" /> Edit
                            </button>
                          )}
                          {submittable && (
                            <button
                              onClick={() => setActionFor({ r, kind: 'submit' })}
                              className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted"
                            >
                              <Icon name="send" className="text-sm" /> Submit
                            </button>
                          )}
                          {pendingApproval && (
                            <>
                              <button
                                onClick={() => setActionFor({ r, kind: 'approve' })}
                                className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-primary hover:bg-muted"
                              >
                                <Icon name="check_circle" className="text-sm" /> Approve
                              </button>
                              <button
                                onClick={() => setActionFor({ r, kind: 'reject' })}
                                className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-status-error hover:bg-muted"
                              >
                                <Icon name="cancel" className="text-sm" /> Reject
                              </button>
                            </>
                          )}
                          {removable && (
                            <button
                              onClick={() => setActionFor({ r, kind: 'remove' })}
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
                      No request notes match these filters.
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
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={start} to={end} setPage={setPageNumber} noun="request notes" className="mt-0 flex-1" />
        </div>
      </div>

      {actionFor && (
        <ConfirmActionModal
          r={actionFor.r}
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
// Confirm action modal — Submit / Approve / Reject / Remove (see ACTION_CONFIG)
// ---------------------------------------------------------------------------

function ConfirmActionModal({
  r,
  kind,
  onClose,
  onDone,
}: {
  r: RequestNoteRow;
  kind: ActionKind;
  onClose: () => void;
  onDone: (message: string) => void | Promise<void>;
}) {
  const cfg = ACTION_CONFIG[kind];
  const [reasonText, setReasonText] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setError(null);
    setSubmitting(true);
    try {
      const init: RequestInit = { method: cfg.method };
      if (cfg.showReason) init.body = JSON.stringify({ reason: reasonText || null });
      await apiClient(cfg.endpoint(r.id), init);
      await onDone(cfg.successMessage(r));
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
          <p className="text-sm text-muted-foreground">{cfg.description(r)}</p>
          {cfg.showReason && (
            <div>
              <label className="mb-1 block text-sm font-semibold text-slate-700">Reason (optional)</label>
              <textarea
                value={reasonText}
                onChange={e => setReasonText(e.target.value)}
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
