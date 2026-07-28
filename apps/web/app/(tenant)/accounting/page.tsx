'use client';

import { useCallback, useEffect, useState } from 'react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient, money } from '@/lib/api-client';
import { Plus, Download, BookText, Receipt, Wallet, Landmark } from 'lucide-react';
import { Modal } from '@/components/ui/Modal';
import { Field, Combobox } from '@/components/ui/form';
import { confirmDialog } from '@/components/ui/confirm';

type Account = { id: string; code: string; name: string; accountType: string; isSystem: boolean; isActive: boolean };
type JLine = { accountCode: string; accountName: string; debit: number; credit: number; lineMemo: string | null };
type Journal = { id: string; entryNo: string; entryDate: string; memo: string | null; source: string; sourceRef: string | null; status: string; debit: number; credit: number; lines: JLine[] };
type TrialRow = { code: string; name: string; debit: number; credit: number; balance: number };
type TrialBalance = { from: string | null; to: string; rows: TrialRow[]; totalDebit: number; totalCredit: number };
type ApRow = { supplierId: string; code: string; name: string; termsDays: number; billed: number; paid: number; balance: number };
type Expense = { id: string; expenseNo: string; expenseDate: string; accountId: string; amount: number; payee: string | null; paymentMethod: string | null; memo: string | null };
const today = () => new Date().toISOString().slice(0, 10);
const monthStart = () => { const d = new Date(); return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-01`; };
type Tab = 'journals' | 'trial' | 'ap' | 'expenses' | 'chart';
const TABS: { id: Tab; label: string }[] = [
  { id: 'journals', label: 'Journals' }, { id: 'trial', label: 'Trial balance' },
  { id: 'ap', label: 'Accounts payable' }, { id: 'expenses', label: 'Expenses' }, { id: 'chart', label: 'Chart of accounts' },
];

export default function AccountingPage() {
  const [tab, setTab] = useState<Tab>('journals');
  const [from, setFrom] = useState(monthStart());
  const [to, setTo] = useState(today());
  const [busy, setBusy] = useState(false);
  const [toast, setToast] = useState<string | null>(null);

  const [accounts, setAccounts] = useState<Account[]>([]);
  const [journals, setJournals] = useState<Journal[]>([]);
  const [trial, setTrial] = useState<TrialBalance | null>(null);
  const [ap, setAp] = useState<ApRow[]>([]);
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [expanded, setExpanded] = useState<string | null>(null);

  const [jrnModal, setJrnModal] = useState(false);
  const [expModal, setExpModal] = useState(false);
  const [payModal, setPayModal] = useState<ApRow | null>(null);
  const [acctModal, setAcctModal] = useState(false);
  const [acctEdit, setAcctEdit] = useState<Account | null>(null);

  const flash = (m: string) => { setToast(m); window.setTimeout(() => setToast(null), 3500); };
  const err = (e: unknown) => { const m = (e as Error).message ?? ''; const j = m.indexOf('{'); if (j >= 0) { try { const p = JSON.parse(m.slice(j)); if (p.error) return flash(p.error); } catch { /* */ } } flash(m || 'Something went wrong'); };

  const qs = useCallback(() => `?from=${from}&to=${to}`, [from, to]);
  const loadAccounts = useCallback(async () => { try { setAccounts(await apiClient<Account[]>('/api/v1/accounting/accounts?includeInactive=true')); } catch (e) { err(e); } }, []);
  const loadTab = useCallback(async () => {
    try {
      if (tab === 'journals') setJournals(await apiClient<Journal[]>(`/api/v1/accounting/journals${qs()}`));
      else if (tab === 'trial') setTrial(await apiClient<TrialBalance>(`/api/v1/accounting/trial-balance${qs()}`));
      else if (tab === 'ap') setAp(await apiClient<ApRow[]>('/api/v1/accounting/ap-aging'));
      else if (tab === 'expenses') setExpenses(await apiClient<Expense[]>(`/api/v1/accounting/expenses${qs()}`));
    } catch (e) { err(e); }
  }, [tab, qs]);

  useEffect(() => { void loadAccounts(); }, [loadAccounts]);
  useEffect(() => { void loadTab(); }, [loadTab]);

  async function post(kind: 'sales' | 'purchases') {
    if (!(await confirmDialog({ title: `Post ${kind} journals?`, body: `Generate and post ${kind} journal entries for ${from} → ${to}. This writes to the general ledger.`, confirmLabel: 'Post' }))) return;
    setBusy(true);
    try {
      const r = await apiClient<{ posted: number }>(`/api/v1/accounting/post/${kind}${qs()}`, { method: 'POST' });
      flash(`Posted ${r.posted} ${kind} journal${r.posted === 1 ? '' : 's'}`);
      await loadTab();
    } catch (e) { err(e); } finally { setBusy(false); }
  }

  async function exportCsv() {
    try {
      const token = localStorage.getItem('hms.token');
      const res = await fetch(`/api/v1/accounting/export${qs()}`, { headers: token ? { Authorization: `Bearer ${token}` } : {}, cache: 'no-store' });
      if (!res.ok) throw new Error(`API ${res.status}`);
      const blob = await res.blob();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a'); a.href = url; a.download = `journals_${from}_${to}.csv`; a.click();
      URL.revokeObjectURL(url);
    } catch (e) { err(e); }
  }

  const acctName = (id: string) => accounts.find(a => a.id === id)?.name ?? '';

  async function toggleAccount(a: Account) {
    if (a.isActive && !(await confirmDialog({ title: `Deactivate ${a.name}?`, body: 'It will be hidden from new journal/expense pickers. Posted history is kept and you can reactivate it any time.', confirmLabel: 'Deactivate', danger: true }))) return;
    try { await apiClient('/api/v1/accounting/accounts', { method: 'POST', body: JSON.stringify({ id: a.id, code: a.code, name: a.name, accountType: a.accountType, isActive: !a.isActive }) }); await loadAccounts(); }
    catch (e) { err(e); }
  }
  async function deleteAccount(a: Account) {
    if (!(await confirmDialog({ title: `Delete ${a.name}?`, body: 'This removes the account from the chart. Accounts used in posted journals are kept; deactivate instead if in doubt.', confirmLabel: 'Delete', danger: true }))) return;
    try { await apiClient(`/api/v1/accounting/accounts/${a.id}`, { method: 'DELETE' }); flash('Account removed'); await loadAccounts(); }
    catch (e) { err(e); }
  }

  return (
    <>
      <Topbar title="Accounting" subtitle="Keep your books effortlessly tidy — ledgers, bills and expenses" />
      <div className="p-6 md:p-8">
        <div className="mb-4 flex flex-wrap items-center gap-2">
          <div className="flex gap-1 rounded-lg border border-border bg-card p-1">
            {TABS.map(t => (
              <button key={t.id} onClick={() => setTab(t.id)}
                className={`rounded-md px-3 py-1.5 text-sm font-semibold ${tab === t.id ? 'bg-primary text-primary-foreground' : 'text-muted-foreground hover:bg-muted'}`}>
                {t.label}
              </button>
            ))}
          </div>
          <div className="ml-auto flex items-end gap-2 text-sm">
            <Field label="From" type="date" value={from} onChange={setFrom} className="w-40" />
            <span className="pb-2 text-muted-foreground">→</span>
            <Field label="To" type="date" value={to} onChange={setTo} className="w-40" />
          </div>
        </div>

        {/* ── Journals ── */}
        {tab === 'journals' && (
          <div className="space-y-3">
            <div className="flex flex-wrap gap-2">
              <button disabled={busy} onClick={() => post('sales')} className="rounded-lg bg-primary px-3 py-2 text-sm font-semibold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">Post Sales</button>
              <button disabled={busy} onClick={() => post('purchases')} className="rounded-lg bg-primary px-3 py-2 text-sm font-semibold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">Post Purchases</button>
              <button onClick={() => setJrnModal(true)} className="flex items-center gap-1.5 rounded-lg border border-border bg-card px-3 py-2 text-sm font-semibold hover:bg-muted"><Plus className="size-4" /> Manual Entry</button>
              <button onClick={exportCsv} className="flex items-center gap-1.5 rounded-lg border border-border bg-card px-3 py-2 text-sm font-semibold hover:bg-muted"><Download className="size-4" /> Export CSV</button>
            </div>
            <div className="card overflow-hidden">
              <table className="w-full text-sm">
                <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                  <tr><th className="px-4 py-3">Entry</th><th className="px-4 py-3">Date</th><th className="px-4 py-3">Source</th><th className="px-4 py-3">Memo</th><th className="px-4 py-3 text-right">Debit</th><th className="px-4 py-3 text-right">Credit</th></tr>
                </thead>
                <tbody>
                  {journals.map(j => (
                    <>
                      <tr key={j.id} onClick={() => setExpanded(expanded === j.id ? null : j.id)} className="cursor-pointer border-b border-border/40 hover:bg-muted/30">
                        <td className="px-4 py-2.5 font-mono text-xs">{j.entryNo}</td>
                        <td className="px-4 py-2.5">{j.entryDate}</td>
                        <td className="px-4 py-2.5"><span className="pill pill-idle">{j.source}</span></td>
                        <td className="px-4 py-2.5 text-muted-foreground">{j.memo}</td>
                        <td className="px-4 py-2.5 text-right tabular-nums">{money(j.debit)}</td>
                        <td className="px-4 py-2.5 text-right tabular-nums">{money(j.credit)}</td>
                      </tr>
                      {expanded === j.id && j.lines.map((l, i) => (
                        <tr key={j.id + i} className="bg-muted/20 text-xs">
                          <td className="px-4 py-1"></td>
                          <td className="px-4 py-1 font-mono">{l.accountCode}</td>
                          <td className="px-4 py-1" colSpan={2}>{l.accountName}{l.lineMemo ? ` · ${l.lineMemo}` : ''}</td>
                          <td className="px-4 py-1 text-right tabular-nums">{l.debit ? money(l.debit) : ''}</td>
                          <td className="px-4 py-1 text-right tabular-nums">{l.credit ? money(l.credit) : ''}</td>
                        </tr>
                      ))}
                    </>
                  ))}
                  {journals.length === 0 && <tr><td colSpan={6} className="px-4 py-10 text-center text-muted-foreground">No journals in this period. Use “Post sales / purchases” to generate them.</td></tr>}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {/* ── Trial balance ── */}
        {tab === 'trial' && trial && (
          <div className="card overflow-hidden">
            <table className="w-full text-sm">
              <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr><th className="px-4 py-3">Code</th><th className="px-4 py-3">Account</th><th className="px-4 py-3 text-right">Debit</th><th className="px-4 py-3 text-right">Credit</th><th className="px-4 py-3 text-right">Balance</th></tr>
              </thead>
              <tbody>
                {trial.rows.map(r => (
                  <tr key={r.code} className="border-b border-border/40">
                    <td className="px-4 py-2.5 font-mono text-xs">{r.code}</td><td className="px-4 py-2.5">{r.name}</td>
                    <td className="px-4 py-2.5 text-right tabular-nums">{r.debit ? money(r.debit) : ''}</td>
                    <td className="px-4 py-2.5 text-right tabular-nums">{r.credit ? money(r.credit) : ''}</td>
                    <td className="px-4 py-2.5 text-right tabular-nums font-semibold">{money(r.balance)}</td>
                  </tr>
                ))}
                <tr className="bg-muted/40 font-bold"><td className="px-4 py-2.5" colSpan={2}>Total</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{money(trial.totalDebit)}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{money(trial.totalCredit)}</td>
                  <td className="px-4 py-2.5 text-right">{Math.abs(trial.totalDebit - trial.totalCredit) < 0.01 ? <span className="pill pill-paid">balanced</span> : <span className="pill pill-void">off</span>}</td>
                </tr>
                {trial.rows.length === 0 && <tr><td colSpan={5} className="px-4 py-10 text-center text-muted-foreground">No posted entries yet.</td></tr>}
              </tbody>
            </table>
          </div>
        )}

        {/* ── Accounts payable ── */}
        {tab === 'ap' && (
          <div className="card overflow-hidden">
            <table className="w-full text-sm">
              <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr><th className="px-4 py-3">Supplier</th><th className="px-4 py-3">Terms</th><th className="px-4 py-3 text-right">Billed</th><th className="px-4 py-3 text-right">Paid</th><th className="px-4 py-3 text-right">Balance</th><th className="px-4 py-3"></th></tr>
              </thead>
              <tbody>
                {ap.map(r => (
                  <tr key={r.supplierId} className="border-b border-border/40">
                    <td className="px-4 py-2.5 font-medium">{r.name}</td>
                    <td className="px-4 py-2.5 text-muted-foreground">{r.termsDays}d</td>
                    <td className="px-4 py-2.5 text-right tabular-nums">{money(r.billed)}</td>
                    <td className="px-4 py-2.5 text-right tabular-nums">{money(r.paid)}</td>
                    <td className="px-4 py-2.5 text-right tabular-nums font-semibold">{money(r.balance)}</td>
                    <td className="px-4 py-2.5 text-right"><button onClick={() => setPayModal(r)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-semibold hover:bg-muted">Pay</button></td>
                  </tr>
                ))}
                {ap.length === 0 && <tr><td colSpan={6} className="px-4 py-10 text-center text-muted-foreground">No outstanding payables. (Post purchases to generate AP from GRNs.)</td></tr>}
              </tbody>
            </table>
          </div>
        )}

        {/* ── Expenses ── */}
        {tab === 'expenses' && (
          <div className="space-y-3">
            <button onClick={() => setExpModal(true)} className="flex items-center gap-1.5 rounded-lg bg-primary px-3 py-2 text-sm font-semibold text-primary-foreground hover:bg-primary-dark"><Plus className="size-4" /> Record Expense</button>
            <div className="card overflow-hidden">
              <table className="w-full text-sm">
                <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                  <tr><th className="px-4 py-3">No.</th><th className="px-4 py-3">Date</th><th className="px-4 py-3">Account</th><th className="px-4 py-3">Payee</th><th className="px-4 py-3">Memo</th><th className="px-4 py-3 text-right">Amount</th></tr>
                </thead>
                <tbody>
                  {expenses.map(x => (
                    <tr key={x.id} className="border-b border-border/40">
                      <td className="px-4 py-2.5 font-mono text-xs">{x.expenseNo}</td><td className="px-4 py-2.5">{x.expenseDate}</td>
                      <td className="px-4 py-2.5">{acctName(x.accountId)}</td><td className="px-4 py-2.5">{x.payee}</td>
                      <td className="px-4 py-2.5 text-muted-foreground">{x.memo}</td>
                      <td className="px-4 py-2.5 text-right tabular-nums font-semibold">{money(x.amount)}</td>
                    </tr>
                  ))}
                  {expenses.length === 0 && <tr><td colSpan={6} className="px-4 py-10 text-center text-muted-foreground">No expenses in this period.</td></tr>}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {/* ── Chart of accounts ── */}
        {tab === 'chart' && (
          <div className="space-y-3">
            <button onClick={() => setAcctModal(true)} className="flex items-center gap-1.5 rounded-lg bg-primary px-3 py-2 text-sm font-semibold text-primary-foreground hover:bg-primary-dark"><Plus className="size-4" /> Add Account</button>
            <div className="card overflow-hidden">
              <table className="w-full text-sm">
                <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                  <tr><th className="px-4 py-3">Code</th><th className="px-4 py-3">Name</th><th className="px-4 py-3">Type</th><th className="px-4 py-3">System</th><th className="px-4 py-3">Active</th><th className="px-4 py-3 text-right">Actions</th></tr>
                </thead>
                <tbody>
                  {accounts.map(a => (
                    <tr key={a.id} className="border-b border-border/40">
                      <td className="px-4 py-2.5 font-mono text-xs">{a.code}</td><td className="px-4 py-2.5">{a.name}</td>
                      <td className="px-4 py-2.5 capitalize">{a.accountType}</td>
                      <td className="px-4 py-2.5">{a.isSystem ? <span className="pill pill-idle">system</span> : ''}</td>
                      <td className="px-4 py-2.5"><span className={`pill ${a.isActive ? 'pill-paid' : 'pill-void'}`}>{a.isActive ? 'Active' : 'Inactive'}</span></td>
                      <td className="px-4 py-2.5 text-right">
                        {a.isSystem ? (
                          <span className="text-xs text-muted-foreground">—</span>
                        ) : (
                          <span className="flex justify-end gap-2">
                            <button onClick={() => setAcctEdit(a)} className="text-xs font-medium text-primary hover:underline">Edit</button>
                            <button onClick={() => toggleAccount(a)} className="text-xs font-medium text-muted-foreground hover:underline">{a.isActive ? 'Deactivate' : 'Activate'}</button>
                            <button onClick={() => deleteAccount(a)} className="text-xs font-medium text-status-error hover:underline">Delete</button>
                          </span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>

      {jrnModal && <ManualJournalModal accounts={accounts} onClose={() => setJrnModal(false)} onSaved={() => { setJrnModal(false); flash('Journal posted'); void loadTab(); }} onError={err} />}
      {expModal && <ExpenseModal accounts={accounts} onClose={() => setExpModal(false)} onSaved={() => { setExpModal(false); flash('Expense recorded'); void loadTab(); }} onError={err} />}
      {payModal && <PayModal row={payModal} accounts={accounts} onClose={() => setPayModal(null)} onSaved={() => { setPayModal(null); flash('Payment recorded'); void loadTab(); }} onError={err} />}
      {acctModal && <AccountModal onClose={() => setAcctModal(false)} onSaved={() => { setAcctModal(false); flash('Account saved'); void loadAccounts(); }} onError={err} />}
      {acctEdit && <AccountModal initial={acctEdit} onClose={() => setAcctEdit(null)} onSaved={() => { setAcctEdit(null); flash('Account saved'); void loadAccounts(); }} onError={err} />}

      {toast && <div className="fixed bottom-12 left-1/2 z-[70] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">{toast}</div>}
    </>
  );
}

function ManualJournalModal({ accounts, onClose, onSaved, onError }: { accounts: Account[]; onClose: () => void; onSaved: () => void; onError: (e: unknown) => void }) {
  const [date, setDate] = useState(today());
  const [memo, setMemo] = useState('');
  const [lines, setLines] = useState([{ accountCode: '', debit: '', credit: '' }, { accountCode: '', debit: '', credit: '' }]);
  const [busy, setBusy] = useState(false);
  const dr = lines.reduce((s, l) => s + (Number(l.debit) || 0), 0);
  const cr = lines.reduce((s, l) => s + (Number(l.credit) || 0), 0);
  const balanced = Math.abs(dr - cr) < 0.01 && dr > 0;
  async function save() {
    if (!balanced) { onError(new Error('Debits must equal credits and be greater than zero')); return; }
    setBusy(true);
    try {
      await apiClient('/api/v1/accounting/journals', { method: 'POST', body: JSON.stringify({ entryDate: date, memo: memo || null, lines: lines.filter(l => l.accountCode && (Number(l.debit) || Number(l.credit))).map(l => ({ accountCode: l.accountCode, debit: Number(l.debit) || 0, credit: Number(l.credit) || 0, memo: null })) }) });
      onSaved();
    } catch (e) { onError(e); } finally { setBusy(false); }
  }
  const acctOpts = accounts.filter(a => a.isActive).map(a => ({ value: a.code, label: `${a.code} ${a.name}` }));
  return (
    <Modal title="Manual Journal Entry" icon={<BookText className="size-4" />} onClose={onClose}
      footer={<button disabled={busy || !balanced} onClick={save} className="h-11 w-full rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{busy ? 'Posting…' : 'Post Entry'}</button>}>
      <div className="space-y-3">
        <div className="flex items-end gap-2"><Field label="Date" type="date" value={date} onChange={setDate} className="w-44" />
          <input value={memo} onChange={e => setMemo(e.target.value)} placeholder="Memo" className="h-10 flex-1 rounded-lg border border-border bg-surface px-3" /></div>
        {lines.map((l, i) => (
          <div key={i} className="flex gap-2">
            <Combobox value={l.accountCode} onChange={v => setLines(ls => ls.map((x, j) => j === i ? { ...x, accountCode: v } : x))} options={acctOpts} placeholder="— account —" className="flex-1" />
            <input value={l.debit} onChange={e => setLines(ls => ls.map((x, j) => j === i ? { ...x, debit: e.target.value.replace(/[^0-9.]/g, ''), credit: '' } : x))} inputMode="decimal" placeholder="Debit" className="h-10 w-24 rounded-lg border border-border bg-surface px-2 text-right tabular-nums" />
            <input value={l.credit} onChange={e => setLines(ls => ls.map((x, j) => j === i ? { ...x, credit: e.target.value.replace(/[^0-9.]/g, ''), debit: '' } : x))} inputMode="decimal" placeholder="Credit" className="h-10 w-24 rounded-lg border border-border bg-surface px-2 text-right tabular-nums" />
          </div>
        ))}
        <button onClick={() => setLines(ls => [...ls, { accountCode: '', debit: '', credit: '' }])} className="text-sm font-semibold text-primary">+ Add line</button>
        <div className="flex justify-between text-sm font-semibold"><span>Dr {money(dr)} · Cr {money(cr)}</span><span className={balanced ? 'text-status-paid' : 'text-status-error'}>{balanced ? 'balanced' : 'must balance'}</span></div>
      </div>
    </Modal>
  );
}

function ExpenseModal({ accounts, onClose, onSaved, onError }: { accounts: Account[]; onClose: () => void; onSaved: () => void; onError: (e: unknown) => void }) {
  const exp = accounts.filter(a => a.accountType === 'expense' && a.isActive);
  const pay = accounts.filter(a => a.accountType === 'asset' && a.isActive);
  const [date, setDate] = useState(today());
  const [accountId, setAccountId] = useState(exp[0]?.id ?? '');
  const [paymentAccountId, setPaymentAccountId] = useState(pay.find(a => a.code === '1000')?.id ?? pay[0]?.id ?? '');
  const [amount, setAmount] = useState(''); const [payee, setPayee] = useState(''); const [memo, setMemo] = useState('');
  const [busy, setBusy] = useState(false);
  async function save() {
    if (!(Number(amount) > 0)) { onError(new Error('Enter an amount greater than zero')); return; }
    if (!accountId || !paymentAccountId) { onError(new Error('Pick an expense account and a paid-from account')); return; }
    setBusy(true);
    try { await apiClient('/api/v1/accounting/expenses', { method: 'POST', body: JSON.stringify({ expenseDate: date, accountId, amount: Number(amount) || 0, payee: payee || null, paymentAccountId, paymentMethod: null, memo: memo || null }) }); onSaved(); }
    catch (e) { onError(e); } finally { setBusy(false); }
  }
  const expOpts = exp.map(a => ({ value: a.id, label: `${a.code} ${a.name}` }));
  const payOpts = pay.map(a => ({ value: a.id, label: `${a.code} ${a.name}` }));
  return (
    <Modal title="Record Expense" icon={<Receipt className="size-4" />} onClose={onClose}
      footer={<button disabled={busy || !(Number(amount) > 0) || !accountId || !paymentAccountId} onClick={save} className="h-11 w-full rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{busy ? 'Saving…' : 'Record (Dr expense / Cr cash)'}</button>}>
      <div className="space-y-3">
        <Field label="Date" type="date" value={date} onChange={setDate} />
        <Combobox label="Expense account" value={accountId} onChange={setAccountId} options={expOpts} />
        <Field label="Amount" value={amount} onChange={v => setAmount(v.replace(/[^0-9.]/g, ''))} inputMode="decimal" placeholder="0.00" mono />
        <Field label="Payee" value={payee} onChange={setPayee} placeholder="Payee" />
        <Combobox label="Paid from" value={paymentAccountId} onChange={setPaymentAccountId} options={payOpts} />
        <Field label="Memo" value={memo} onChange={setMemo} placeholder="Memo" />
      </div>
    </Modal>
  );
}

function PayModal({ row, accounts, onClose, onSaved, onError }: { row: ApRow; accounts: Account[]; onClose: () => void; onSaved: () => void; onError: (e: unknown) => void }) {
  const pay = accounts.filter(a => a.accountType === 'asset' && a.isActive);
  const [date, setDate] = useState(today());
  const [amount, setAmount] = useState(row.balance.toFixed(2));
  const [paymentAccountId, setPaymentAccountId] = useState(pay.find(a => a.code === '1000')?.id ?? pay[0]?.id ?? '');
  const [reference, setReference] = useState(''); const [busy, setBusy] = useState(false);
  async function save() {
    if (!(Number(amount) > 0)) { onError(new Error('Enter an amount greater than zero')); return; }
    if (!paymentAccountId) { onError(new Error('Pick a paid-from account')); return; }
    setBusy(true);
    try { await apiClient('/api/v1/accounting/ap-payments', { method: 'POST', body: JSON.stringify({ paymentDate: date, supplierId: row.supplierId, amount: Number(amount) || 0, paymentAccountId, reference: reference || null, memo: null }) }); onSaved(); }
    catch (e) { onError(e); } finally { setBusy(false); }
  }
  const payOpts = pay.map(a => ({ value: a.id, label: `${a.code} ${a.name}` }));
  return (
    <Modal title={`Pay ${row.name}`} icon={<Wallet className="size-4" />} onClose={onClose}
      footer={<button disabled={busy || !(Number(amount) > 0) || !paymentAccountId} onClick={save} className="h-11 w-full rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{busy ? 'Saving…' : 'Record payment (Dr AP / Cr cash)'}</button>}>
      <div className="space-y-3">
        <p className="text-sm text-muted-foreground">Outstanding: <span className="font-semibold text-on-surface">{money(row.balance)}</span></p>
        <Field label="Date" type="date" value={date} onChange={setDate} />
        <Field label="Amount" value={amount} onChange={v => setAmount(v.replace(/[^0-9.]/g, ''))} inputMode="decimal" placeholder="0.00" mono />
        <Combobox label="Paid from" value={paymentAccountId} onChange={setPaymentAccountId} options={payOpts} />
        <Field label="Reference" value={reference} onChange={setReference} placeholder="Reference (cheque #, etc.)" />
      </div>
    </Modal>
  );
}

function AccountModal({ initial, onClose, onSaved, onError }: { initial?: Account | null; onClose: () => void; onSaved: () => void; onError: (e: unknown) => void }) {
  const [code, setCode] = useState(initial?.code ?? ''); const [name, setName] = useState(initial?.name ?? '');
  const [type, setType] = useState(initial?.accountType ?? 'expense'); const [busy, setBusy] = useState(false);
  const editing = !!initial;
  async function save() {
    if (!code.trim() || !name.trim()) { onError(new Error('Code and name are required')); return; }
    setBusy(true);
    // POST with an id edits; without one it creates. Preserve the account's active state on edit.
    try { await apiClient('/api/v1/accounting/accounts', { method: 'POST', body: JSON.stringify({ id: initial?.id ?? null, code, name, accountType: type, isActive: initial?.isActive ?? true }) }); onSaved(); }
    catch (e) { onError(e); } finally { setBusy(false); }
  }
  const typeOpts = ['asset', 'liability', 'equity', 'income', 'expense'].map(t => ({ value: t, label: t.charAt(0).toUpperCase() + t.slice(1) }));
  return (
    <Modal title={editing ? 'Edit Account' : 'Add Account'} icon={<Landmark className="size-4" />} onClose={onClose}
      footer={<button disabled={busy || !code || !name} onClick={save} className="h-11 w-full rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{busy ? 'Saving…' : editing ? 'Save Account' : 'Add Account'}</button>}>
      <div className="space-y-3">
        <Field label="Code" value={code} onChange={setCode} placeholder="Code (e.g. 6200)" mono />
        <Field label="Name" value={name} onChange={setName} placeholder="Name (e.g. Utilities)" />
        <Combobox label="Type" value={type} onChange={setType} options={typeOpts} />
      </div>
    </Modal>
  );
}
