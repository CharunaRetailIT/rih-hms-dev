'use client';

import { useEffect, useState } from 'react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient } from '@/lib/api-client';
import { confirmDialog } from '@/components/ui/confirm';
import { Pagination } from '@/components/ui/Pagination';
import { Modal } from '@/components/ui/Modal';
import { Plus, X, Trash2, ShieldCheck, ChevronUp, ChevronDown } from 'lucide-react';

type Step = { level: number; approverType: string; approverUserId?: string | null; approverRole?: number | null; approverEmail?: string | null; approverLabel: string };
type Rule = { id?: string; docType: string; name: string; minAmount: number; locationId?: string | null; isActive: boolean; steps: Step[] };
type Member = { id: string; displayName: string; role: number; email?: string | null };
type ApprovalItem = {
  id: string; docType: string; docNumber: string; amount: number; requestedByName: string | null;
  currentLevel: number; createdAt: string; status: string; decidedAt: string | null; holdReason?: string | null;
};
type PageMeta = { totalCount: number; pageNumber: number; pageSize: number; totalPages: number };

const DOC = [
  { v: 'purchase_order', label: 'Purchase Order' },
  { v: 'grn', label: 'Goods Receipt (GRN)' },
  { v: 'stock_transfer', label: 'Stock Transfer' },
  { v: 'wastage_note', label: 'Wastage Note' },
  { v: 'request_note', label: 'Request Note' },
  { v: 'stock_adjustment', label: 'Stock Adjustment' },
];
const ROLES = [{ v: 0, label: 'Any Owner' }, { v: 1, label: 'Any Manager' }, { v: 4, label: 'Any Accountant' }];
const DOC_LABEL: Record<string, string> = Object.fromEntries(DOC.map(d => [d.v, d.label]));

export default function ApprovalsPage() {
  const [rules, setRules] = useState<Rule[]>([]);
  const [members, setMembers] = useState<Member[]>([]);
  const [edit, setEdit] = useState<Rule | null>(null);
  const [loading, setLoading] = useState(true);
  const [toast, setToast] = useState<string | null>(null);
  const flash = (m: string) => { setToast(m); window.setTimeout(() => setToast(null), 3000); };

  async function load() {
    try {
      const [r, m] = await Promise.all([
        apiClient<Rule[]>('/api/v1/approval-rules'),
        apiClient<Member[]>('/api/v1/users').catch(() => []),
      ]);
      setRules(r); setMembers(m);
    } catch (e) { flash((e as Error).message); } finally { setLoading(false); }
  }
  useEffect(() => { void load(); }, []);

  const blank = (): Rule => ({ docType: 'purchase_order', name: '', minAmount: 0, isActive: true, steps: [{ level: 1, approverType: 'role', approverRole: 1, approverLabel: 'Any Manager' }] });

  function setStep(idx: number, patch: Partial<Step>) {
    setEdit(e => e ? { ...e, steps: e.steps.map((s, i) => i === idx ? { ...s, ...patch } : s) } : e);
  }
  function moveStep(i: number, dir: -1 | 1) {
    setEdit(e => {
      if (!e) return e;
      const j = i + dir;
      if (j < 0 || j >= e.steps.length) return e;
      const steps = [...e.steps];
      [steps[i], steps[j]] = [steps[j], steps[i]];   // order = the level hierarchy (save re-numbers by position)
      return { ...e, steps };
    });
  }
  function labelFor(s: Step): string {
    if (s.approverType === 'role') return ROLES.find(r => r.v === s.approverRole)?.label ?? 'Approver';
    if (s.approverType === 'user') return members.find(m => m.id === s.approverUserId)?.displayName ?? 'Approver';
    return s.approverEmail || 'External approver';
  }

  async function save() {
    if (!edit) return;
    if (!edit.name.trim()) { flash('Give the rule a name.'); return; }
    if (edit.steps.length === 0) { flash('Add at least one approval level.'); return; }
    const steps = edit.steps.map((s, i) => ({ ...s, level: i + 1, approverLabel: labelFor(s),
      approverUserId: s.approverType === 'user' ? s.approverUserId : null,
      approverRole: s.approverType === 'role' ? s.approverRole : null,
      approverEmail: s.approverType === 'email' ? (s.approverEmail || '').trim() : null }));
    try {
      await apiClient('/api/v1/approval-rules', { method: 'PUT', body: JSON.stringify({ ...edit, steps }) });
      flash('Rule saved.'); setEdit(null); await load();
    } catch (e) { flash((e as Error).message); }
  }
  async function remove(r: Rule) {
    if (!r.id || !(await confirmDialog({ title: `Delete "${r.name}"?`, body: 'This approval rule will be removed. In-flight approvals are unaffected.', confirmLabel: 'Delete', danger: true }))) return;
    try { await apiClient(`/api/v1/approval-rules/${r.id}`, { method: 'DELETE' }); flash('Rule deleted.'); await load(); }
    catch (e) { flash((e as Error).message); }
  }

  const fld = 'rounded-lg border border-border bg-surface px-2.5 py-1.5 text-sm';

  return (
    <div>
      <Topbar title="Approvals" subtitle="Decide what needs sign-off, and who signs it — approvers get an email link (no login needed)." />
      <div className="p-6">
        <ApprovalInboxTabs flash={flash} />

        <div className="mb-4 flex items-center justify-between">
          <h2 className="font-heading text-xl font-bold">Approval rules</h2>
          <button onClick={() => setEdit(blank())} className="inline-flex items-center gap-1.5 rounded-lg bg-primary px-3 py-2 text-sm font-semibold text-primary-foreground hover:bg-primary-dark"><Plus className="size-4" /> New rule</button>
        </div>

        {loading ? <p className="text-sm text-muted-foreground">Loading…</p> : rules.length === 0 ? (
          <div className="rounded-lg border border-dashed border-border bg-muted/30 px-4 py-10 text-center text-sm text-muted-foreground">
            No approval rules yet. Add one to require sign-off on Purchase Orders, GRNs or Transfers — optionally only above a value, with one or more approval levels.
          </div>
        ) : (
          <div className="space-y-2">
            {rules.map(r => (
              <div key={r.id} className="flex flex-wrap items-center gap-3 rounded-lg border border-border bg-card px-4 py-3">
                <ShieldCheck className={`size-5 ${r.isActive ? 'text-primary' : 'text-muted-foreground'}`} />
                <div className="min-w-0 flex-1">
                  <div className="font-semibold">{r.name} {!r.isActive && <span className="ml-1 text-xs text-muted-foreground">(off)</span>}</div>
                  <div className="text-xs text-muted-foreground">{DOC_LABEL[r.docType] ?? r.docType}{r.minAmount > 0 ? ` · ≥ ${r.minAmount.toLocaleString()}` : ''} · {r.steps.length} level{r.steps.length === 1 ? '' : 's'}: {r.steps.map(labelFor).join(' → ')}</div>
                </div>
                <button onClick={() => setEdit(JSON.parse(JSON.stringify(r)))} className="rounded-lg border border-border px-3 py-1.5 text-sm font-semibold hover:bg-muted">Edit</button>
                <button onClick={() => remove(r)} className="rounded-lg border border-border px-3 py-1.5 text-sm font-semibold text-status-error hover:bg-muted">Delete</button>
              </div>
            ))}
          </div>
        )}

        {edit && (
          <div className="mt-6 rounded-xl border border-primary/40 bg-card p-5 ring-1 ring-primary/20">
            <div className="mb-3 flex items-center justify-between">
              <h3 className="font-heading text-lg font-bold">{edit.id ? 'Edit rule' : 'New rule'}</h3>
              <button onClick={() => setEdit(null)} className="rounded-lg p-1 text-muted-foreground hover:bg-muted"><X className="size-5" /></button>
            </div>
            <div className="grid gap-3 sm:grid-cols-2">
              <label className="text-sm">Document type
                <select value={edit.docType} onChange={e => setEdit({ ...edit, docType: e.target.value })} className={`${fld} mt-1 w-full`}>
                  {DOC.map(d => <option key={d.v} value={d.v}>{d.label}</option>)}
                </select>
              </label>
              <label className="text-sm">Rule name
                <input value={edit.name} onChange={e => setEdit({ ...edit, name: e.target.value })} placeholder="e.g. POs over LKR 50,000" className={`${fld} mt-1 w-full`} />
              </label>
              <label className="text-sm">Only when amount ≥ <span className="text-muted-foreground">(0 = always)</span>
                <input value={String(edit.minAmount)} inputMode="decimal" onChange={e => setEdit({ ...edit, minAmount: Number(e.target.value.replace(/[^0-9.]/g, '')) || 0 })} className={`${fld} mt-1 w-full text-right`} />
              </label>
              <label className="flex items-end gap-2 text-sm"><input type="checkbox" checked={edit.isActive} onChange={e => setEdit({ ...edit, isActive: e.target.checked })} className="mb-2 size-4 rounded" /> Active</label>
            </div>

            <div className="mt-4">
              <div className="mb-1.5 flex items-center justify-between">
                <label className="text-sm font-semibold">Approval levels <span className="font-normal text-muted-foreground">— signed off top→bottom; use ↑↓ to set the order</span></label>
                <button onClick={() => setEdit({ ...edit, steps: [...edit.steps, { level: edit.steps.length + 1, approverType: 'role', approverRole: 0, approverLabel: 'Any Owner' }] })} className="text-xs font-medium text-primary hover:underline">+ Add level</button>
              </div>
              <div className="space-y-2">
                {edit.steps.map((s, i) => (
                  <div key={i} className="flex flex-wrap items-center gap-2 rounded-lg border border-border bg-surface px-3 py-2">
                    <span className="grid size-6 shrink-0 place-items-center rounded-full bg-primary/10 text-xs font-bold text-primary">{i + 1}</span>
                    <select value={s.approverType} onChange={e => setStep(i, { approverType: e.target.value })} className={`${fld} w-32`}>
                      <option value="role">A group</option>
                      <option value="user">A person</option>
                      <option value="email">An email</option>
                    </select>
                    {s.approverType === 'role' && (
                      <select value={s.approverRole ?? 0} onChange={e => setStep(i, { approverRole: Number(e.target.value) })} className={`${fld} flex-1`}>
                        {ROLES.map(r => <option key={r.v} value={r.v}>{r.label}</option>)}
                      </select>
                    )}
                    {s.approverType === 'user' && (
                      <select value={s.approverUserId ?? ''} onChange={e => setStep(i, { approverUserId: e.target.value })} className={`${fld} flex-1`}>
                        <option value="">— pick a person —</option>
                        {members.map(m => <option key={m.id} value={m.id}>{m.displayName}</option>)}
                      </select>
                    )}
                    {s.approverType === 'email' && (
                      <input value={s.approverEmail ?? ''} onChange={e => setStep(i, { approverEmail: e.target.value })} placeholder="approver@email.com" className={`${fld} flex-1`} />
                    )}
                    <div className="flex items-center">
                      <button title="Move up" disabled={i === 0} onClick={() => moveStep(i, -1)} className="rounded-lg p-1.5 text-muted-foreground hover:bg-muted disabled:opacity-30"><ChevronUp className="size-4" /></button>
                      <button title="Move down" disabled={i === edit.steps.length - 1} onClick={() => moveStep(i, 1)} className="rounded-lg p-1.5 text-muted-foreground hover:bg-muted disabled:opacity-30"><ChevronDown className="size-4" /></button>
                    </div>
                    <button title="Remove level" onClick={() => setEdit({ ...edit, steps: edit.steps.filter((_, idx) => idx !== i) })} className="rounded-lg p-1.5 text-muted-foreground hover:bg-muted hover:text-status-error"><Trash2 className="size-4" /></button>
                  </div>
                ))}
              </div>
            </div>

            <div className="mt-4 flex gap-2">
              <button onClick={save} className="flex-1 rounded-lg bg-primary py-2.5 text-sm font-bold text-primary-foreground hover:bg-primary-dark">Save rule</button>
              <button onClick={() => setEdit(null)} className="rounded-lg border border-border px-4 py-2.5 text-sm font-semibold hover:bg-muted">Cancel</button>
            </div>
          </div>
        )}
      </div>
      {toast && <div className="fixed bottom-6 left-1/2 -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2 text-sm font-medium text-white shadow-lg">{toast}</div>}
    </div>
  );
}

const DOC_SHORT: Record<string, string> = { purchase_order: 'PO', grn: 'GRN', stock_transfer: 'Transfer' };

type TabId = 'pending' | 'on_hold' | 'approved' | 'rejected';
const TABS: { id: TabId; label: string; actionable: boolean }[] = [
  { id: 'pending', label: 'Awaiting', actionable: true },
  { id: 'on_hold', label: 'On Hold', actionable: true },
  { id: 'approved', label: 'Approved', actionable: false },
  { id: 'rejected', label: 'Rejected', actionable: false },
];

/** Tabbed, server-paginated view of the requests relevant to the logged-in user: what's
 * awaiting their sign-off, what they've put on hold, and (read-only history) what's already
 * been approved/rejected tenant-wide. */
function ApprovalInboxTabs({ flash }: { flash: (m: string) => void }) {
  const [tab, setTab] = useState<TabId>('pending');
  const [items, setItems] = useState<ApprovalItem[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [decideFor, setDecideFor] = useState<{ item: ApprovalItem; action: 'reject' | 'hold' } | null>(null);
  const [remark, setRemark] = useState('');
  const [remarkError, setRemarkError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function load(t: TabId, page: number, size: number) {
    setLoading(true);
    try {
      const params = new URLSearchParams({ status: t, pageNumber: String(page), pageSize: String(size) });
      const res = await apiClient<{ data: ApprovalItem[]; pagination: PageMeta }>(`/api/v1/approvals?${params.toString()}`);
      setItems(res.data); setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages || 1);
    } catch { /* */ }
    finally { setLoading(false); }
  }

  useEffect(() => { void load(tab, pageNumber, pageSize); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { setPageNumber(1); void load(tab, 1, pageSize); /* eslint-disable-next-line */ }, [tab]);

  async function submitDecision(p: ApprovalItem, action: 'approve' | 'reject' | 'hold', remarkText: string | null) {
    setBusyId(p.id);
    try {
      await apiClient(`/api/v1/approvals/${p.id}/decide`, { method: 'POST', body: JSON.stringify({ action, remark: remarkText }) });
      flash(action === 'approve' ? 'Approved.' : action === 'reject' ? 'Rejected.' : 'Put on hold.');
      await load(tab, pageNumber, pageSize);
    } catch (e) { flash((e as Error).message); }
    finally { setBusyId(null); }
  }

  function decide(p: ApprovalItem, action: 'approve' | 'reject' | 'hold') {
    if (action === 'approve') { void submitDecision(p, 'approve', null); return; }
    setRemark(''); setRemarkError(null); setDecideFor({ item: p, action });
  }

  async function confirmDecideModal() {
    if (!decideFor) return;
    if (!remark.trim()) { setRemarkError('A reason is required.'); return; }
    setSubmitting(true);
    try {
      await submitDecision(decideFor.item, decideFor.action, remark.trim());
      setDecideFor(null);
    } finally { setSubmitting(false); }
  }

  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);
  const activeTab = TABS.find(x => x.id === tab)!;

  return (
    <div className="mb-8">
      <div className="mb-3 flex gap-1 border-b border-border">
        {TABS.map(t => (
          <button key={t.id} onClick={() => setTab(t.id)}
            className={`-mb-px border-b-2 px-4 py-2 text-sm font-semibold transition-colors ${tab === t.id ? 'border-primary text-primary' : 'border-transparent text-muted-foreground hover:text-on-surface'}`}>
            {t.label}{tab === t.id && totalCount > 0 ? ` (${totalCount})` : ''}
          </button>
        ))}
      </div>

      {loading ? (
        <div className="space-y-2">{Array.from({ length: 3 }).map((_, i) => <div key={i} className="h-14 animate-pulse rounded-lg bg-muted" />)}</div>
      ) : items.length === 0 ? (
        <div className="rounded-lg border border-dashed border-border bg-muted/30 px-4 py-8 text-center text-sm text-muted-foreground">
          {tab === 'pending' && 'Nothing awaiting your approval right now.'}
          {tab === 'on_hold' && "You haven't put anything on hold."}
          {tab === 'approved' && 'No approved requests yet.'}
          {tab === 'rejected' && 'No rejected requests yet.'}
        </div>
      ) : (
        <div className="space-y-2">
          {items.map(p => (
            <div key={p.id} className={`flex flex-wrap items-center gap-3 rounded-lg border px-4 py-3 ${activeTab.actionable ? 'border-primary/40 bg-primary-tint/30' : 'border-border bg-card'}`}>
              <div className="min-w-0 flex-1">
                <div className="font-semibold">{DOC_SHORT[p.docType] ?? p.docType} {p.docNumber} <span className="font-normal text-muted-foreground">· Level {p.currentLevel}</span></div>
                <div className="text-xs text-muted-foreground">
                  Amount {p.amount.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                  {p.requestedByName ? ` · Requested by ${p.requestedByName}` : ''}
                  {!activeTab.actionable && p.decidedAt ? ` · ${new Date(p.decidedAt).toLocaleDateString('en-LK', { year: 'numeric', month: 'short', day: 'numeric' })}` : ''}
                </div>
                {tab === 'on_hold' && p.holdReason && (
                  <div className="mt-1 text-xs italic text-amber-700">Hold reason: {p.holdReason}</div>
                )}
              </div>
              {activeTab.actionable && (
                <>
                  <button disabled={busyId === p.id} onClick={() => decide(p, 'approve')} className="rounded-lg bg-primary px-3 py-1.5 text-sm font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">Approve</button>
                  {tab !== 'on_hold' && (
                    <button disabled={busyId === p.id} onClick={() => decide(p, 'hold')} className="rounded-lg border border-border px-3 py-1.5 text-sm font-semibold text-amber-700 hover:bg-muted disabled:opacity-50">Hold</button>
                  )}
                  <button disabled={busyId === p.id} onClick={() => decide(p, 'reject')} className="rounded-lg border border-border px-3 py-1.5 text-sm font-semibold text-status-error hover:bg-muted disabled:opacity-50">Reject</button>
                </>
              )}
            </div>
          ))}
        </div>
      )}

      {!loading && totalCount > 0 && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-surface px-2 py-1.5 text-xs"
          >
            {[10, 25, 50, 100].map(n => <option key={n} value={n}>{n} / page</option>)}
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun="requests" className="mt-0 flex-1" />
        </div>
      )}

      {decideFor && (
        <Modal
          title={decideFor.action === 'reject' ? 'Reject request' : 'Put on hold'}
          onClose={() => !submitting && setDecideFor(null)}
          size="sm"
          tone={decideFor.action === 'reject' ? 'danger' : 'primary'}
          footer={
            <div className="flex gap-2">
              <button onClick={() => setDecideFor(null)} disabled={submitting} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50">
                {decideFor.action === 'reject' ? "Don't reject" : 'Cancel'}
              </button>
              <button onClick={confirmDecideModal} disabled={submitting}
                className={`h-11 flex-1 rounded-lg font-bold text-white disabled:opacity-50 ${decideFor.action === 'reject' ? 'bg-status-error hover:opacity-90' : 'bg-primary hover:bg-primary-dark'}`}>
                {submitting ? 'Saving…' : decideFor.action === 'reject' ? 'Reject' : 'Put on hold'}
              </button>
            </div>
          }
        >
          <p className="mb-4 text-sm text-muted-foreground">
            This {decideFor.action === 'reject' ? 'rejects' : 'puts on hold'} {decideFor.item.docNumber}.
            {decideFor.action === 'reject' ? ' It stays rejected — resubmit a new request if it needs to go through again.' : ' You (or another approver at this level) can come back and decide it later.'}
          </p>
          <label className="mb-1 block text-sm font-semibold text-slate-700">Reason</label>
          <textarea
            autoFocus
            value={remark}
            onChange={e => { setRemark(e.target.value); if (remarkError) setRemarkError(null); }}
            rows={3}
            placeholder="Add a note…"
            className={`w-full rounded-lg border bg-card px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary ${remarkError ? 'border-status-error' : 'border-border'}`}
          />
          {remarkError && <p className="mt-1 text-xs text-status-error">{remarkError}</p>}
        </Modal>
      )}
    </div>
  );
}
