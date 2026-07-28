'use client';

import { useEffect, useState } from 'react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient, money } from '@/lib/api-client';
import { confirmDialog } from '@/components/ui/confirm';
import { Modal } from '@/components/ui/Modal';
import { Field, Combobox } from '@/components/ui/form';
import { AddressFields } from '@/components/ui/AddressFields';
import { type Address, EMPTY_ADDRESS } from '@/lib/regions';
import { Plus, X, Search, Wallet, History, User, Pencil, Tags, Power, Trash2 } from 'lucide-react';
import { HeaderStat, Num } from '@/components/ui/HeaderStat';
import { Pagination } from '@/components/ui/Pagination';

type Category = { id: string; code: string; name: string; discountPercent: number; isActive: boolean; notes?: string | null; isVat: boolean };
type Customer = {
  id: string; code: string; name: string; categoryId: string | null; categoryName: string | null;
  phone: string | null; email: string | null; address: string | null; taxNo: string | null; loyaltyCardNo: string | null; dateOfBirth: string | null;
  countryCode: string | null; province: string | null; district: string | null; postalCode: string | null;
  discountPercent: number | null; isCreditCustomer: boolean; creditLimit: number; currentBalance: number;
  creditAvailable: number; advanceBalance: number; isActive: boolean; notes: string | null;
};
type Ledger = {
  customer: Customer; balance: number; loyaltyPoints: number;
  orders: { id: string; orderNumber: string; invoiceNumber: string | null; settledAt: string | null; totalAmount: number; onCredit: boolean }[];
  receipts: { id: string; amount: number; payType: string; reference: string | null; receivedAt: string; notes: string | null }[];
  loyalty: { id: string; txnType: string; points: number; balanceAfter: number; note: string | null; createdAt: string }[];
};

type Form = {
  id?: string; code: string; name: string; categoryId: string; phone: string; email: string;
  address: string; addr: Address; taxNo: string; loyaltyCardNo: string; dateOfBirth: string; discountPercent: string; isCreditCustomer: boolean; creditLimit: string; isActive: boolean; notes: string;
};
const emptyForm: Form = {
  code: '', name: '', categoryId: '', phone: '', email: '', address: '', addr: EMPTY_ADDRESS, taxNo: '', loyaltyCardNo: '', dateOfBirth: '',
  discountPercent: '', isCreditCustomer: false, creditLimit: '', isActive: true, notes: '',
};

export default function CustomersPage() {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [search, setSearch] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [toast, setToast] = useState<string | null>(null);
  const [open, setOpen] = useState(false);
  const [form, setForm] = useState<Form>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [catModal, setCatModal] = useState(false);
  const [ledger, setLedger] = useState<Ledger | null>(null);
  const [payAmt, setPayAmt] = useState('');
  const [advAmt, setAdvAmt] = useState('');

  function flash(m: string) { setToast(m); window.setTimeout(() => setToast(null), 3000); }

  async function loadCategories() {
    try { setCategories(await apiClient<Category[]>('/api/v1/customer-categories')); }
    catch (e) { flash(extractError(e, 'Could not load categories.')); }
  }

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set('pageNumber', String(pageNumber));
      params.set('pageSize', String(pageSize));
      if (search.trim()) params.set('search', search.trim());
      if (categoryFilter) params.set('categoryId', categoryFilter);
      if (statusFilter) params.set('isActive', statusFilter === 'active' ? 'true' : 'false');

      const res = await apiClient<{ data: Customer[]; pagination: { totalCount: number; pageNumber: number; pageSize: number; totalPages: number } }>(
        `/api/v1/customers/paged?${params.toString()}`,
      );
      setCustomers(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages || 1);
    } catch (e) { setError((e as Error).message); }
    finally { setLoading(false); }
  }
  useEffect(() => { void loadCategories(); /* eslint-disable-next-line */ }, []);
  useEffect(() => { void load(); /* eslint-disable-next-line */ }, [pageNumber, pageSize, categoryFilter, statusFilter]);
  useEffect(() => {
    const t = setTimeout(() => { setPageNumber(1); void load(); }, 300);
    return () => clearTimeout(t);
    /* eslint-disable-next-line */
  }, [search]);

  function openNew() { setForm(emptyForm); setOpen(true); }
  function openEdit(c: Customer) {
    setForm({
      id: c.id, code: c.code, name: c.name, categoryId: c.categoryId ?? '', phone: c.phone ?? '', email: c.email ?? '',
      address: c.address ?? '', addr: { countryCode: c.countryCode ?? '', province: c.province ?? '', district: c.district ?? '', postalCode: c.postalCode ?? '' },
      taxNo: c.taxNo ?? '', loyaltyCardNo: c.loyaltyCardNo ?? '', dateOfBirth: c.dateOfBirth ?? '', discountPercent: c.discountPercent ? String(c.discountPercent) : '',
      isCreditCustomer: c.isCreditCustomer, creditLimit: String(c.creditLimit), isActive: c.isActive, notes: c.notes ?? '',
    });
    setOpen(true);
  }

  async function save() {
    if (!form.name.trim()) { flash('Customer name is required.'); return; }
    setSaving(true);
    try {
      await apiClient('/api/v1/customers', {
        method: 'PUT',
        body: JSON.stringify({
          id: form.id, code: form.code.trim() || null, name: form.name.trim(), categoryId: form.categoryId || null,
          phone: form.phone.trim() || null, email: form.email.trim() || null, address: form.address.trim() || null,
          countryCode: form.addr.countryCode || null, province: form.addr.province.trim() || null, district: form.addr.district.trim() || null, postalCode: form.addr.postalCode.trim() || null,
          taxNo: form.taxNo.trim() || null, loyaltyCardNo: form.loyaltyCardNo.trim() || null, dateOfBirth: form.dateOfBirth || null, discountPercent: Number(form.discountPercent) || null,
          isCreditCustomer: form.isCreditCustomer, creditLimit: Number(form.creditLimit) || 0,
          isActive: form.isActive, notes: form.notes.trim() || null,
        }),
      });
      setOpen(false); flash('Customer saved.'); await load();
    } catch (e) { flash(extractError(e, 'Could not save the customer.')); }
    finally { setSaving(false); }
  }
  async function remove(c: Customer) {
    if (!(await confirmDialog({ title: `Remove ${c.name}?`, body: 'Customers with order or payment history are deactivated instead of deleted.', confirmLabel: 'Remove', danger: true }))) return;
    try { await apiClient(`/api/v1/customers/${c.id}`, { method: 'DELETE' }); flash(`${c.name} removed.`); await load(); }
    catch (e) { flash(extractError(e, 'Could not remove the customer.')); }
  }

  async function openLedger(c: Customer) {
    try { setLedger(await apiClient<Ledger>(`/api/v1/customers/${c.id}/ledger`)); setPayAmt(''); }
    catch (e) { flash(extractError(e, 'Could not load the ledger.')); }
  }
  async function recordPayment() {
    if (!ledger) return;
    const amt = Number(payAmt);
    if (!amt || amt <= 0) { flash('Enter an amount.'); return; }
    try {
      await apiClient(`/api/v1/customers/${ledger.customer.id}/payments`, { method: 'POST', body: JSON.stringify({ amount: amt }) });
      flash('Payment recorded.'); await openLedger(ledger.customer); await load();
    } catch (e) { flash(extractError(e, 'Could not record the payment.')); }
  }
  async function recordAdvance() {
    if (!ledger) return;
    const amt = Number(advAmt);
    if (!amt || amt <= 0) { flash('Enter an amount.'); return; }
    try {
      await apiClient(`/api/v1/customers/${ledger.customer.id}/advance`, { method: 'POST', body: JSON.stringify({ amount: amt }) });
      setAdvAmt(''); flash('Advance recorded.'); await openLedger(ledger.customer); await load();
    } catch (e) { flash(extractError(e, 'Could not record the advance.')); }
  }

  return (
    <>
      <Topbar title="Customers" subtitle="Build lasting relationships with credit accounts and loyalty" />
      <div className="p-6 md:p-8">
        <div className="mb-5 flex items-center justify-between gap-3">
          <div>
            <h2 className="font-heading text-xl font-bold">Customers</h2>
            <HeaderStat><Num>{totalCount}</Num> total · per-customer discounts &amp; credit (A/R)</HeaderStat>
          </div>
          <div className="flex items-center gap-2">
            <div className="relative">
              <Search className="pointer-events-none absolute left-2.5 top-2.5 size-4 text-muted-foreground" />
              <input value={search} onChange={e => setSearch(e.target.value)} placeholder="Search name / phone / code"
                className="h-9 w-64 rounded-lg border border-border bg-surface pl-8 pr-3 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20" />
            </div>
            <select value={categoryFilter} onChange={e => { setCategoryFilter(e.target.value); setPageNumber(1); }} className="rounded-lg border border-border bg-card px-3 py-2 text-sm">
              <option value="">All categories</option>
              {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
            <select value={statusFilter} onChange={e => { setStatusFilter(e.target.value); setPageNumber(1); }} className="rounded-lg border border-border bg-card px-3 py-2 text-sm">
              <option value="">All statuses</option>
              <option value="active">Active</option>
              <option value="inactive">Inactive</option>
            </select>
            <button onClick={() => setCatModal(true)} className="rounded-lg border border-border px-3 py-2 text-sm font-medium hover:bg-muted">Categories</button>
            <button onClick={openNew} className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark">
              <Plus className="size-4" /> New Customer
            </button>
          </div>
        </div>

        <div className="card overflow-hidden">
          {loading ? (
            <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
          ) : error ? (
            <div className="p-6 text-sm text-status-error">{error}</div>
          ) : customers.length === 0 ? (
            <div className="p-10 text-center text-sm text-muted-foreground">No customers yet. Add your first regular or credit customer.</div>
          ) : (
            <table className="w-full text-sm">
              <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th className="px-4 py-3 font-medium">Customer</th>
                  <th className="px-4 py-3 font-medium">Category</th>
                  <th className="px-4 py-3 font-medium">Contact</th>
                  <th className="px-4 py-3 text-right font-medium">Discount</th>
                  <th className="px-4 py-3 text-right font-medium">Balance / Limit</th>
                  <th className="px-4 py-3 text-right font-medium">Actions</th>
                </tr>
              </thead>
              <tbody>
                {customers.map((c, i) => (
                  <tr key={c.id} className={i % 2 ? 'bg-muted/20' : ''}>
                    <td className="px-4 py-3">
                      <button onClick={() => openLedger(c)} className="text-left font-medium hover:text-primary">{c.name}</button>
                      <span className="ml-1 font-mono text-xs text-muted-foreground">{c.code}</span>
                      {!c.isActive && <span className="pill pill-void ml-2">Inactive</span>}
                    </td>
                    <td className="px-4 py-3 text-muted-foreground">{c.categoryName ?? '—'}</td>
                    <td className="px-4 py-3 text-xs text-muted-foreground">{c.phone ?? c.email ?? '—'}</td>
                    <td className="px-4 py-3 text-right tabular-nums">{c.discountPercent ? `${c.discountPercent}%` : '—'}</td>
                    <td className="px-4 py-3 text-right tabular-nums">
                      {c.isCreditCustomer
                        ? <span className={c.currentBalance > 0 ? 'font-semibold text-status-error' : ''}>{money(c.currentBalance)} <span className="text-muted-foreground">/ {money(c.creditLimit)}</span></span>
                        : <span className="text-muted-foreground">cash</span>}
                    </td>
                    <td className="px-4 py-3 text-right">
                      <button onClick={() => openLedger(c)} title="Ledger" className="rounded p-1.5 text-muted-foreground hover:bg-muted hover:text-foreground"><History className="size-4" /></button>
                      <button onClick={() => openEdit(c)} className="rounded px-2 py-1 text-xs font-medium text-primary hover:bg-primary-tint">Edit</button>
                      <button onClick={() => remove(c)} className="rounded px-2 py-1 text-xs font-medium text-status-error hover:bg-status-error/10">Delete</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
        {!loading && !error && totalCount > 0 && (
          <Pagination
            page={pageNumber}
            totalPages={totalPages}
            total={totalCount}
            from={(pageNumber - 1) * pageSize + 1}
            to={Math.min(pageNumber * pageSize, totalCount)}
            setPage={setPageNumber}
            noun="customers"
          />
        )}
      </div>

      {/* New / edit customer */}
      {open && (
        <Modal
          title={form.id ? 'Edit Customer' : 'New Customer'}
          icon={form.id ? <Pencil className="size-4" /> : <User className="size-4" />}
          onClose={() => !saving && setOpen(false)}
          footer={
            <div className="flex gap-2">
              <button onClick={() => setOpen(false)} disabled={saving} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50">Cancel</button>
              <button onClick={save} disabled={saving} className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{saving ? 'Saving…' : 'Save'}</button>
            </div>
          }
        >
          <div className="grid grid-cols-2 gap-3">
            <Field label="Name *" className="col-span-2" value={form.name} onChange={v => setForm(f => ({ ...f, name: v }))} />
            <Field label="Code" mono value={form.code} onChange={v => setForm(f => ({ ...f, code: v }))} placeholder="auto" />
            <Combobox
              label="Category"
              value={form.categoryId}
              onChange={v => setForm(f => ({ ...f, categoryId: v }))}
              placeholder="— none —"
              options={[{ value: '', label: '— none —' }, ...categories.map(c => ({ value: c.id, label: `${c.name}${c.discountPercent ? ` (${c.discountPercent}%)` : ''}` }))]}
            />
            <Field label="Phone" value={form.phone} onChange={v => setForm(f => ({ ...f, phone: v }))} />
            <Field label="Email" value={form.email} onChange={v => setForm(f => ({ ...f, email: v }))} />
            <Field label="Street address" className="col-span-2" value={form.address} onChange={v => setForm(f => ({ ...f, address: v }))} />
            <div className="col-span-2"><AddressFields value={form.addr} onChange={a => setForm(f => ({ ...f, addr: a }))} /></div>
            <Field label="Tax / NIC / VAT no." mono value={form.taxNo} onChange={v => setForm(f => ({ ...f, taxNo: v }))} />
            <Field label="Loyalty card no." mono value={form.loyaltyCardNo} onChange={v => setForm(f => ({ ...f, loyaltyCardNo: v }))} placeholder="scan / enter card" />
            <Field label="Birthday" type="date" value={form.dateOfBirth} onChange={v => setForm(f => ({ ...f, dateOfBirth: v }))} helper="For birthday greetings & offers." />
            <Field label="Default discount %" inputMode="decimal" placeholder="0" value={form.discountPercent} onChange={v => setForm(f => ({ ...f, discountPercent: v.replace(/[^0-9.]/g, '') }))} />
            <div className="col-span-2 mt-1 rounded-lg border border-border bg-surface p-3">
              <label className="flex items-center gap-2 text-sm font-medium">
                <input type="checkbox" checked={form.isCreditCustomer} onChange={e => setForm(f => ({ ...f, isCreditCustomer: e.target.checked }))} className="size-4 rounded" />
                Credit customer (charge to account)
              </label>
              {form.isCreditCustomer && (
                <Field label="Credit limit (LKR)" className="mt-2" inputMode="decimal" value={form.creditLimit} onChange={v => setForm(f => ({ ...f, creditLimit: v.replace(/[^0-9.]/g, '') }))} />
              )}
            </div>
            <Field label="Notes" className="col-span-2" value={form.notes} onChange={v => setForm(f => ({ ...f, notes: v }))} />
            <label className="col-span-2 flex items-center gap-2 text-sm">
              <input type="checkbox" checked={form.isActive} onChange={e => setForm(f => ({ ...f, isActive: e.target.checked }))} className="size-4 rounded" /> Active
            </label>
          </div>
        </Modal>
      )}

      {/* Ledger drawer */}
      {ledger && (
        <Modal title={`Ledger — ${ledger.customer.name}`} icon={<Wallet className="size-4" />} onClose={() => setLedger(null)} size="md">
            <p className="-mt-2 mb-4 text-xs text-muted-foreground">{ledger.customer.code}{ledger.customer.phone ? ` · ${ledger.customer.phone}` : ''}</p>

            {ledger.customer.isCreditCustomer && (
              <div className="mb-4 rounded-lg border border-border bg-surface p-3">
                <div className="flex items-center justify-between text-sm">
                  <span className="text-muted-foreground">Outstanding balance</span>
                  <span className={`font-bold tabular-nums ${ledger.balance > 0 ? 'text-status-error' : ''}`}>{money(ledger.balance)}</span>
                </div>
                <div className="mt-1 flex items-center justify-between text-xs text-muted-foreground">
                  <span>Credit limit</span><span className="tabular-nums">{money(ledger.customer.creditLimit)}</span>
                </div>
                {ledger.balance > 0 && (
                  <div className="mt-3 flex gap-2">
                    <input value={payAmt} onChange={e => setPayAmt(e.target.value.replace(/[^0-9.]/g, ''))} inputMode="decimal" placeholder="Amount received"
                      className="h-10 flex-1 rounded-lg border border-border bg-card px-3 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20" />
                    <button onClick={recordPayment} className="flex items-center gap-1 rounded-lg bg-primary px-3 text-sm font-bold text-primary-foreground hover:bg-primary-dark">
                      <Wallet className="size-4" /> Receive
                    </button>
                  </div>
                )}
              </div>
            )}

            <div className="mb-4 rounded-lg border border-border bg-surface p-3">
              <div className="flex items-center justify-between text-sm">
                <span className="text-muted-foreground">Advance / deposit</span>
                <span className="font-bold tabular-nums text-primary">{money(ledger.customer.advanceBalance)}</span>
              </div>
              <div className="mt-3 flex gap-2">
                <input value={advAmt} onChange={e => setAdvAmt(e.target.value.replace(/[^0-9.]/g, ''))} inputMode="decimal" placeholder="Deposit amount"
                  className="h-10 flex-1 rounded-lg border border-border bg-card px-3 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20" />
                <button onClick={recordAdvance} className="flex items-center gap-1 rounded-lg border border-border bg-card px-3 text-sm font-bold hover:bg-muted">
                  <Wallet className="size-4" /> Take advance
                </button>
              </div>
            </div>

            <h4 className="mb-2 text-xs font-semibold uppercase tracking-wide text-muted-foreground">Visit history</h4>
            {ledger.orders.length === 0 ? <p className="mb-4 text-sm text-muted-foreground">No settled orders yet.</p> : (
              <div className="mb-4 space-y-1">
                {ledger.orders.map(o => (
                  <div key={o.id} className="flex items-center justify-between rounded border border-border px-3 py-2 text-sm">
                    <span>{o.invoiceNumber ?? o.orderNumber} {o.onCredit && <span className="pill pill-idle ml-1">credit</span>}</span>
                    <span className="tabular-nums">{money(o.totalAmount)}</span>
                  </div>
                ))}
              </div>
            )}

            {ledger.receipts.length > 0 && (
              <>
                <h4 className="mb-2 text-xs font-semibold uppercase tracking-wide text-muted-foreground">AR receipts</h4>
                <div className="space-y-1">
                  {ledger.receipts.map(r => (
                    <div key={r.id} className="flex items-center justify-between rounded border border-border px-3 py-2 text-sm">
                      <span className="text-muted-foreground">{new Date(r.receivedAt).toLocaleDateString()} · {r.payType}</span>
                      <span className="tabular-nums text-primary">− {money(r.amount)}</span>
                    </div>
                  ))}
                </div>
              </>
            )}

            {(ledger.loyaltyPoints > 0 || ledger.loyalty.length > 0) && (
              <>
                <h4 className="mb-2 mt-4 flex items-center justify-between text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                  <span>Loyalty points</span>
                  <span className="rounded bg-primary-tint px-1.5 text-primary">{ledger.loyaltyPoints} pts</span>
                </h4>
                <div className="space-y-1">
                  {ledger.loyalty.map(t => (
                    <div key={t.id} className="flex items-center justify-between rounded border border-border px-3 py-2 text-sm">
                      <span className="text-muted-foreground">{new Date(t.createdAt).toLocaleDateString()} · {t.txnType}</span>
                      <span className={`tabular-nums ${t.points >= 0 ? 'text-primary' : 'text-status-error'}`}>{t.points >= 0 ? '+' : ''}{t.points} pts</span>
                    </div>
                  ))}
                </div>
              </>
            )}

            <ContractPrices customerId={ledger.customer.id} flash={flash} />
            <StatementSection customerId={ledger.customer.id} />
        </Modal>
      )}

      {/* Categories manager */}
      {catModal && <CategoriesModal categories={categories} onClose={() => setCatModal(false)} onChanged={load} flash={flash} />}

      {toast && <div className="fixed bottom-6 left-1/2 z-[80] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2 text-sm text-white shadow-lg">{toast}</div>}
    </>
  );
}

type PriceRow = { productId: string; name: string; sku: string; basePrice: number; price: number };
type ProductLite = { id: string; name: string; basePrice: number };

function ContractPrices({ customerId, flash }: { customerId: string; flash: (m: string) => void }) {
  const [products, setProducts] = useState<ProductLite[]>([]);
  const [rows, setRows] = useState<PriceRow[]>([]);
  const [pid, setPid] = useState('');
  const [price, setPrice] = useState('');

  async function load() { try { setRows(await apiClient<PriceRow[]>(`/api/v1/customers/${customerId}/prices`)); } catch { /* */ } }
  useEffect(() => { apiClient<ProductLite[]>('/api/v1/products').then(setProducts).catch(() => {}); void load(); /* eslint-disable-next-line react-hooks/exhaustive-deps */ }, [customerId]);

  async function add() {
    if (!pid || price.trim() === '') { flash('Pick a product and a price.'); return; }
    try { await apiClient(`/api/v1/customers/${customerId}/prices`, { method: 'PUT', body: JSON.stringify({ productId: pid, price: Number(price) }) }); setPid(''); setPrice(''); await load(); flash('Contract price saved.'); }
    catch (e) { flash(extractError(e, 'Could not save the price.')); }
  }
  async function remove(productId: string) {
    try { await apiClient(`/api/v1/customers/${customerId}/prices/${productId}`, { method: 'DELETE' }); await load(); }
    catch (e) { flash(extractError(e, 'Could not remove the price.')); }
  }

  return (
    <>
      <h4 className="mb-2 mt-5 text-xs font-semibold uppercase tracking-wide text-muted-foreground">Contract prices</h4>
      {rows.length > 0 && (
        <div className="mb-2 space-y-1">
          {rows.map(r => (
            <div key={r.productId} className="flex items-center justify-between rounded border border-border px-3 py-2 text-sm">
              <span>{r.name} <span className="font-mono text-xs text-muted-foreground">{r.sku}</span></span>
              <span className="flex items-center gap-2">
                <span className="text-xs text-muted-foreground line-through">{money(r.basePrice)}</span>
                <span className="font-semibold tabular-nums">{money(r.price)}</span>
                <button onClick={() => remove(r.productId)} className="text-muted-foreground hover:text-status-error"><X className="size-4" /></button>
              </span>
            </div>
          ))}
        </div>
      )}
      <div className="flex items-start gap-2">
        <Combobox
          className="flex-1"
          value={pid}
          onChange={setPid}
          placeholder="— product —"
          up
          options={products.map(p => ({ value: p.id, label: `${p.name} (base ${money(p.basePrice)})` }))}
        />
        <input value={price} onChange={e => setPrice(e.target.value.replace(/[^0-9.]/g, ''))} inputMode="decimal" placeholder="price" className="h-10 w-24 rounded-lg border border-border bg-card px-3 text-right text-sm focus:border-primary focus:ring-2 focus:ring-primary/20" />
        <button onClick={add} className="flex h-10 items-center gap-1 rounded-lg bg-primary px-3 text-sm font-bold text-primary-foreground hover:bg-primary-dark"><Plus className="size-4" /></button>
      </div>
    </>
  );
}

type Statement = {
  periodFrom: string; periodTo: string; opening: number; closing: number; totalCharges: number; totalReceipts: number;
  lines: { date: string; kind: string; reference: string | null; debit: number; credit: number; balance: number }[];
};
function StatementSection({ customerId }: { customerId: string }) {
  const [st, setSt] = useState<Statement | null>(null);
  const [loading, setLoading] = useState(false);
  async function load() { setLoading(true); try { setSt(await apiClient<Statement>(`/api/v1/customers/${customerId}/statement`)); } catch { /* */ } finally { setLoading(false); } }
  return (
    <div className="mt-5">
      <h4 className="mb-2 flex items-center justify-between text-xs font-semibold uppercase tracking-wide text-muted-foreground">
        <span>Statement (this month)</span>
        <button onClick={load} className="font-medium text-primary hover:underline">{loading ? '…' : st ? 'Refresh' : 'Load'}</button>
      </h4>
      {st && (
        <div className="rounded-lg border border-border bg-surface p-3 text-sm">
          <div className="flex justify-between text-xs text-muted-foreground"><span>Opening</span><span className="tabular-nums">{money(st.opening)}</span></div>
          {st.lines.map((l, i) => (
            <div key={i} className="mt-1 flex items-center justify-between border-t border-border/50 pt-1 text-xs">
              <span className="text-muted-foreground">{new Date(l.date).toLocaleDateString()} · {l.kind === 'charge' ? 'Charge' : 'Receipt'} {l.reference ? `· ${l.reference}` : ''}</span>
              <span className="flex gap-2 tabular-nums">
                <span className={l.debit ? 'text-status-error' : 'text-primary'}>{l.debit ? `+${money(l.debit)}` : `−${money(l.credit)}`}</span>
                <span className="w-20 text-right font-medium">{money(l.balance)}</span>
              </span>
            </div>
          ))}
          <div className="mt-2 flex justify-between border-t border-border pt-2 font-bold"><span>Closing</span><span className="tabular-nums">{money(st.closing)}</span></div>
          {st.lines.length === 0 && <p className="mt-1 text-xs text-muted-foreground">No activity this month.</p>}
        </div>
      )}
    </div>
  );
}

function CategoriesModal({ categories, onClose, onChanged, flash }: {
  categories: Category[]; onClose: () => void; onChanged: () => Promise<void>; flash: (m: string) => void;
}) {
  const [editId, setEditId] = useState<string | null>(null);
  const [code, setCode] = useState('');
  const [name, setName] = useState('');
  const [disc, setDisc] = useState('');
  const [remark, setRemark] = useState('');
  const [isVat, setIsVat] = useState(false);
  const [busy, setBusy] = useState(false);

  const editing = categories.find(c => c.id === editId) ?? null;

  function reset() { setEditId(null); setCode(''); setName(''); setDisc(''); setRemark(''); setIsVat(false); }
  function startEdit(c: Category) {
    setEditId(c.id); setCode(c.code); setName(c.name);
    setDisc(c.discountPercent ? String(c.discountPercent) : ''); setRemark(c.notes ?? ''); setIsVat(c.isVat);
  }

  async function save() {
    if (!name.trim()) { flash('Category name is required.'); return; }
    setBusy(true);
    try {
      await apiClient('/api/v1/customer-categories', {
        method: 'PUT',
        // Blank code auto-derives from the name; editing keeps the active state.
        body: JSON.stringify({ id: editId, code: code.trim() || null, name: name.trim(), discountPercent: Number(disc) || 0, isActive: editing?.isActive ?? true, notes: remark.trim() || null, isVat }),
      });
      reset(); flash('Category saved.'); await onChanged();
    } catch (e) { flash(extractError(e, 'Could not save the category.')); }
    finally { setBusy(false); }
  }
  async function toggleActive(c: Category) {
    if (c.isActive && !(await confirmDialog({ title: `Deactivate ${c.name}?`, body: 'It will be hidden when assigning customers. Existing customers keep the category and you can reactivate it any time.', confirmLabel: 'Deactivate', danger: true }))) return;
    setBusy(true);
    try {
      await apiClient('/api/v1/customer-categories', {
        method: 'PUT',
        body: JSON.stringify({ id: c.id, code: c.code, name: c.name, discountPercent: c.discountPercent, isActive: !c.isActive, notes: c.notes ?? null, isVat: c.isVat }),
      });
      await onChanged();
    } catch (e) { flash(extractError(e, 'Could not update the category.')); }
    finally { setBusy(false); }
  }
  async function del(c: Category) {
    if (!(await confirmDialog({ title: `Delete ${c.name}?`, body: 'Customers in this category will be left uncategorised.', confirmLabel: 'Delete', danger: true }))) return;
    setBusy(true);
    try { await apiClient(`/api/v1/customer-categories/${c.id}`, { method: 'DELETE' }); if (editId === c.id) reset(); await onChanged(); }
    catch (e) { flash(extractError(e, 'Could not delete.')); }
    finally { setBusy(false); }
  }
  return (
    <Modal title="Customer Categories" icon={<Tags className="size-4" />} onClose={() => !busy && onClose()} size="md">
      <div className="mb-3 space-y-1">
        {categories.map(c => (
          <div key={c.id} className="flex items-center justify-between rounded border border-border px-3 py-2 text-sm">
            <span>
              <span className="text-muted-foreground">{c.code}</span> · {c.name} {c.discountPercent ? <span className="text-muted-foreground">· {c.discountPercent}%</span> : null}
              {c.isVat && <span className="pill pill-info ml-2">VAT</span>}
              {!c.isActive && <span className="pill pill-void ml-2">Inactive</span>}
            </span>
            <span className="flex shrink-0 items-center gap-0.5">
              <button disabled={busy} onClick={() => startEdit(c)} title="Edit" className="rounded-lg p-1.5 text-muted-foreground hover:bg-muted hover:text-foreground disabled:opacity-50"><Pencil className="size-4" /></button>
              <button disabled={busy} onClick={() => toggleActive(c)} title={c.isActive ? 'Deactivate' : 'Reactivate'} className={`rounded-lg p-1.5 hover:bg-muted disabled:opacity-50 ${c.isActive ? 'text-muted-foreground hover:text-status-error' : 'text-status-success'}`}><Power className="size-4" /></button>
              <button disabled={busy} onClick={() => del(c)} title="Delete" className="rounded-lg p-1.5 text-status-error hover:bg-muted disabled:opacity-50"><Trash2 className="size-4" /></button>
            </span>
          </div>
        ))}
        {categories.length === 0 && <p className="text-sm text-muted-foreground">No categories yet.</p>}
      </div>
      <div className="rounded-lg border border-border bg-muted/30 p-3">
        <div className="mb-2 text-sm font-semibold">{editId ? 'Edit Category' : 'New Category'}</div>
        <div className="flex gap-2">
          <input value={code} onChange={e => setCode(e.target.value)} placeholder="Code (auto)" className="h-10 w-24 min-w-0 rounded-lg border border-border bg-surface px-3 text-sm" />
          <input value={name} onChange={e => setName(e.target.value)} placeholder="Name, e.g. Staff" className="h-10 min-w-0 flex-1 rounded-lg border border-border bg-surface px-3 text-sm" />
          <input value={disc} onChange={e => setDisc(e.target.value.replace(/[^0-9.]/g, ''))} inputMode="decimal" placeholder="%" className="h-10 w-14 min-w-0 shrink-0 rounded-lg border border-border bg-surface px-3 text-sm" />
        </div>
        <input value={remark} onChange={e => setRemark(e.target.value)} placeholder="Remark (optional)" className="mt-2 h-10 w-full rounded-lg border border-border bg-surface px-3 text-sm" />
        <label className="mt-2 flex items-center gap-2 text-sm">
          <input type="checkbox" checked={isVat} onChange={e => setIsVat(e.target.checked)} className="size-4 rounded border-border" />
          VAT registered by default
        </label>
        <div className="mt-3 flex gap-2">
          {editId && <button onClick={reset} disabled={busy} className="rounded-lg border border-border px-3 py-2 text-sm font-semibold hover:bg-muted disabled:opacity-50">Cancel Edit</button>}
          <button onClick={save} disabled={busy} className="flex-1 rounded-lg bg-primary py-2 text-sm font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{editId ? 'Save Category' : 'Add Category'}</button>
        </div>
      </div>
    </Modal>
  );
}

function extractError(e: unknown, fallback: string): string {
  const m = (e as Error)?.message ?? '';
  const j = m.match(/\{[\s\S]*\}$/);
  if (j) { try { return JSON.parse(j[0]).error ?? fallback; } catch { /* noop */ } }
  return m || fallback;
}
