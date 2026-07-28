'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { Search } from 'lucide-react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient, lkr } from '@/lib/api-client';
import { Icon } from '@/components/ui/Icon';
import { Pagination } from '@/components/ui/Pagination';

type Location = { id: string; code: string; name: string };

type TransferRow = {
  id: string;
  transferNumber: string;
  fromLocationId: string;
  fromLocationCode: string;
  fromLocationName: string;
  toLocationId: string;
  toLocationCode: string;
  toLocationName: string;
  isReturn: boolean;
  transferDate: string;
  referenceNo: string | null;
  status: string;
  totalCost: number;
  dispatchedAt: string | null;
  receivedAt: string | null;
  createdAt: string;
};

type PagedResponse = {
  data: TransferRow[];
  pagination: { totalCount: number; pageNumber: number; pageSize: number; totalPages: number };
};

const STATUS_PILL: Record<string, string> = {
  draft: 'pill-idle',
  pending: 'pill-pending',
  dispatched: 'pill-pending',
  received: 'pill-paid',
  rejected: 'pill-void',
  cancelled: 'pill-void',
};
const STATUS_LABEL: Record<string, string> = {
  draft: 'Draft',
  pending: 'Pending approval',
  dispatched: 'Dispatched',
  received: 'Received',
  rejected: 'Rejected',
  cancelled: 'Cancelled',
};

const EDITABLE = new Set(['draft']);
const SUBMITTABLE = new Set(['draft']);
const PENDING = new Set(['pending']);
const REMOVABLE = new Set(['draft', 'pending', 'rejected']);

// ---- Submit / Approve / Reject / Remove — one confirm modal, four configs --

type ActionKind = 'submit' | 'approve' | 'reject' | 'remove';

const ACTION_CONFIG: Record<ActionKind, {
  endpoint: (id: string) => string;
  method: 'POST' | 'DELETE';
  title: string;
  description: (t: TransferRow) => string;
  confirmLabel: string;
  submittingLabel: string;
  closeLabel: string;
  danger: boolean;
  showReason: boolean;
  successMessage: string;
}> = {
  submit: {
    endpoint: id => `/api/v1/transfers/${id}/submit`, method: 'POST',
    title: 'Submit transfer',
    description: t =>
      `This submits ${t.transferNumber} for processing. Depending on your approval settings it either posts straight through as Dispatched, or waits as Pending until someone approves it.`,
    confirmLabel: 'Submit', submittingLabel: 'Submitting…', closeLabel: 'Not yet',
    danger: false, showReason: false, successMessage: 'Transfer submitted',
  },
  approve: {
    endpoint: id => `/api/v1/transfers/${id}/approve`, method: 'POST',
    title: 'Approve transfer',
    description: t => `This approves ${t.transferNumber} and dispatches it, decrementing stock at ${t.fromLocationCode}.`,
    confirmLabel: 'Approve', submittingLabel: 'Approving…', closeLabel: 'Not now',
    danger: false, showReason: false, successMessage: 'Transfer approved',
  },
  reject: {
    endpoint: id => `/api/v1/transfers/${id}/reject`, method: 'POST',
    title: 'Reject transfer',
    description: t => `This rejects ${t.transferNumber}. It's final — create a new transfer if it needs to be resubmitted.`,
    confirmLabel: 'Reject transfer', submittingLabel: 'Rejecting…', closeLabel: "Don't reject",
    danger: true, showReason: true, successMessage: 'Transfer rejected',
  },
  remove: {
    endpoint: id => `/api/v1/transfers/${id}`, method: 'DELETE',
    title: 'Remove transfer',
    description: t => `This removes ${t.transferNumber} from the list. It hasn't dispatched, so no stock has moved.`,
    confirmLabel: 'Remove', submittingLabel: 'Removing…', closeLabel: 'Keep it',
    danger: true, showReason: false, successMessage: 'Transfer removed',
  },
};

export default function TransfersListPage() {
  const router = useRouter();

  const [rows, setRows] = useState<TransferRow[]>([]);
  const [locations, setLocations] = useState<Location[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [search, setSearch] = useState('');
  const [fromLocationId, setFromLocationId] = useState('');
  const [toLocationId, setToLocationId] = useState('');
  const [status, setStatus] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const [busyId, setBusyId] = useState<string | null>(null);
  const [toast, setToast] = useState<string | null>(null);
  const [actionFor, setActionFor] = useState<{ t: TransferRow; kind: ActionKind } | null>(null);

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
      if (status) qs.set('status', status);
      if (dateFrom) qs.set('dateFrom', dateFrom);
      if (dateTo) qs.set('dateTo', dateTo);
      const res = await apiClient<PagedResponse>(`/api/v1/transfers/paged?${qs}`);
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
  }, [pageNumber, pageSize, fromLocationId, toLocationId, status, dateFrom, dateTo]);

  useEffect(() => {
    const t = setTimeout(() => {
      setPageNumber(1);
      load();
    }, 350);
    return () => clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search]);

  async function act(id: string, action: 'receive') {
    setBusyId(id);
    try {
      await apiClient(`/api/v1/transfers/${id}/${action}`, { method: 'POST' });
      flash('Transfer received.');
      await load();
    } catch (e) {
      flash(extractError(e, `Could not ${action} transfer.`));
    } finally {
      setBusyId(null);
    }
  }

  const start = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const end = Math.min(pageNumber * pageSize, totalCount);

  return (
    <>
      <Topbar title="Transfer of Goods" subtitle="Move stock between your outlets, quickly and accurately" />

      <div className="p-6">
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Transfers</h2>
            <p className="text-sm text-muted-foreground">{totalCount} transfer{totalCount === 1 ? '' : 's'}</p>
          </div>
          <button
            onClick={() => router.push('/transfers/create')}
            className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark"
          >
            <Icon name="add" className="text-base" /> New Transfer
          </button>
        </div>

        <div className="card mb-4 grid grid-cols-2 gap-3 p-4 md:grid-cols-7">
          <div className="relative col-span-2">
            <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
            <input
              value={search}
              onChange={e => setSearch(e.target.value)}
              placeholder="Search doc #, ref #, remark…"
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
            value={status}
            onChange={e => { setStatus(e.target.value); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">All statuses</option>
            <option value="draft">Draft</option>
            <option value="pending">Pending approval</option>
            <option value="dispatched">Dispatched</option>
            <option value="received">Received</option>
            <option value="rejected">Rejected</option>
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
                  <th className="px-4 py-2.5 font-medium">Doc #</th>
                  <th className="px-4 py-2.5 font-medium">From</th>
                  <th className="px-4 py-2.5 font-medium">To</th>
                  <th className="px-4 py-2.5 font-medium">Type</th>
                  <th className="px-4 py-2.5 font-medium">Status</th>
                  <th className="px-4 py-2.5 text-right font-medium">Total</th>
                  <th className="px-4 py-2.5 font-medium">Date</th>
                  <th className="px-4 py-2.5 text-right font-medium">Actions</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((t, i) => {
                  const editable = EDITABLE.has(t.status);
                  const submittable = SUBMITTABLE.has(t.status);
                  const pendingApproval = PENDING.has(t.status);
                  const receivable = t.status === 'dispatched';
                  const removable = REMOVABLE.has(t.status);
                  const hasAction = editable || submittable || pendingApproval || receivable || removable;
                  return (
                    <tr key={t.id} className={i % 2 ? 'bg-muted/20' : ''}>
                      <td className="px-4 py-2.5 font-mono text-xs text-muted-foreground">{t.transferNumber}</td>
                      <td className="px-4 py-2.5">
                        <span className="pill pill-idle">{t.fromLocationCode}</span>
                      </td>
                      <td className="px-4 py-2.5">
                        <span className="pill pill-idle">{t.toLocationCode}</span>
                      </td>
                      <td className="px-4 py-2.5 text-muted-foreground">{t.isReturn ? 'Return' : 'Issue'}</td>
                      <td className="px-4 py-2.5">
                        <span className={`pill ${STATUS_PILL[t.status] ?? 'pill-idle'}`}>
                          {STATUS_LABEL[t.status] ?? t.status}
                        </span>
                      </td>
                      <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{lkr(t.totalCost)}</td>
                      <td className="px-4 py-2.5 text-muted-foreground">
                        {t.transferDate ? new Date(t.transferDate).toLocaleDateString('en-LK') : '—'}
                      </td>
                      <td className="px-4 py-2.5 text-right">
                        <div className="flex flex-wrap items-center justify-end gap-2">
                          {editable && (
                            <button
                              onClick={() => router.push(`/transfers/${t.id}/edit`)}
                              className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted"
                            >
                              <Icon name="edit" className="text-sm" /> Edit
                            </button>
                          )}
                          {submittable && (
                            <button
                              onClick={() => setActionFor({ t, kind: 'submit' })}
                              className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted"
                            >
                              <Icon name="send" className="text-sm" /> Submit
                            </button>
                          )}
                          {pendingApproval && (
                            <>
                              <button
                                onClick={() => setActionFor({ t, kind: 'approve' })}
                                className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-primary hover:bg-muted"
                              >
                                <Icon name="check_circle" className="text-sm" /> Approve
                              </button>
                              <button
                                onClick={() => setActionFor({ t, kind: 'reject' })}
                                className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-status-error hover:bg-muted"
                              >
                                <Icon name="cancel" className="text-sm" /> Reject
                              </button>
                            </>
                          )}
                          {receivable && (
                            <button
                              disabled={busyId === t.id}
                              onClick={() => act(t.id, 'receive')}
                              className="inline-flex items-center gap-1 rounded-lg bg-primary px-3 py-1.5 text-xs font-medium text-primary-foreground hover:bg-primary-dark disabled:opacity-50"
                            >
                              <Icon name="inventory_2" className="text-sm" /> Receive
                            </button>
                          )}
                          {removable && (
                            <button
                              onClick={() => setActionFor({ t, kind: 'remove' })}
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
                    <td colSpan={8} className="px-4 py-10 text-center text-muted-foreground">
                      No transfers match these filters.
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
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={start} to={end} setPage={setPageNumber} noun="transfers" className="mt-0 flex-1" />
        </div>
      </div>

      {actionFor && (
        <ConfirmActionModal
          t={actionFor.t}
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
  t,
  kind,
  onClose,
  onDone,
}: {
  t: TransferRow;
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
      await apiClient(cfg.endpoint(t.id), init);
      await onDone(cfg.successMessage);
    } catch (e) {
      setError(extractError(e, 'Something went wrong.'));
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
          <p className="text-sm text-muted-foreground">{cfg.description(t)}</p>
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

/** Pull a friendly {error} string out of an apiClient error like `API 400: {"error":"..."}`. */
function extractError(e: unknown, fallback: string): string {
  const msg = (e as Error)?.message ?? '';
  const jsonStart = msg.indexOf('{');
  if (jsonStart !== -1) {
    try {
      const parsed = JSON.parse(msg.slice(jsonStart));
      if (typeof parsed?.error === 'string') return parsed.error;
      if (typeof parsed?.message === 'string') return parsed.message;
    } catch {
      /* fall through */
    }
  }
  return msg || fallback;
}
