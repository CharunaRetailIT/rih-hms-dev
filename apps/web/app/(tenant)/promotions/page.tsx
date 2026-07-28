'use client';

import { useEffect, useState } from 'react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient, lkr } from '@/lib/api-client';
import { Modal } from '@/components/ui/Modal';
import { confirmDialog } from '@/components/ui/confirm';
import { Field, Combobox } from '@/components/ui/form';
import { Plus, X, Percent, Search } from 'lucide-react';
import { HeaderStat, Num } from '@/components/ui/HeaderStat';
import { Pagination } from '@/components/ui/Pagination';

type Product = { id: string; name: string };
type Line = {
  productId?: string | null; categoryId?: string | null; minQty?: number; billFrom?: number; billTo?: number | null;
  getProductId?: string | null; getQty?: number; bundlePrice?: number | null;
  discountPercent?: number; discountAmount?: number;
};
type Promo = {
  id: string; code: string; name: string; promoType: string; isActive: boolean; autoApply: boolean;
  priority: number; startsOn: string | null; endsOn: string | null; daysMask: number;
  startTime: string | null; endTime: string | null; appliesToOrderType: string | null;
  displayMessage: string | null; appliesToCategoryId: string | null; lines: Line[];
};
type CustCategory = { id: string; name: string };
type ProdCategory = { id: string; name: string; parentId: string | null };

const TYPE_LABEL: Record<string, string> = {
  product_discount: 'Product discount', time_based: 'Time-based', bill_value: 'Spend & save', buy_x_get_y: 'Buy X get Y', bundle: 'Bundle', lowest_price: 'Lowest-price (3-for-2)',
};
const DAYS = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

type FormLine = {
  productId: string; categoryId: string; targetType: string; minQty: string; billFrom: string; billTo: string;
  getProductId: string; getQty: string; discountPercent: string; discountAmount: string;
};
type Form = {
  id?: string; code: string; name: string; promoType: string; isActive: boolean; autoApply: boolean;
  priority: string; startsOn: string; endsOn: string; daysMask: number; startTime: string; endTime: string;
  appliesToOrderType: string; appliesToCategoryId: string; displayMessage: string; bundlePrice: string;
  lines: FormLine[];
};
const emptyForm: Form = {
  code: '', name: '', promoType: '', isActive: false, autoApply: false, priority: '',
  startsOn: '', endsOn: '', daysMask: 0, startTime: '', endTime: '', appliesToOrderType: '', appliesToCategoryId: '', displayMessage: '',
  bundlePrice: '', lines: [],
};

type PaginationMeta = { totalCount: number; pageNumber: number; pageSize: number; totalPages: number };
type PagedResponse = { data: Promo[]; pagination: PaginationMeta };

export default function PromotionsPage() {
  const [promos, setPromos] = useState<Promo[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [custCategories, setCustCategories] = useState<CustCategory[]>([]);
  const [prodCategories, setProdCategories] = useState<ProdCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [toast, setToast] = useState<string | null>(null);
  const [open, setOpen] = useState(false);
  const [form, setForm] = useState<Form>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [query, setQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [typeFilter, setTypeFilter] = useState('');

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  function flash(m: string) { setToast(m); window.setTimeout(() => setToast(null), 3000); }

  async function loadPromos() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ pageNumber: String(pageNumber), pageSize: String(pageSize) });
      if (query.trim()) params.set('search', query.trim());
      if (statusFilter) params.set('status', statusFilter);
      if (typeFilter) params.set('promoType', typeFilter);
      const res = await apiClient<PagedResponse>(`/api/v1/promotions/paged?${params.toString()}`);
      setPromos(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages);
    } catch (e) { setError((e as Error).message); }
    finally { setLoading(false); }
  }

  async function loadLookups() {
    try {
      const [prods, cats, prodCats] = await Promise.all([
        apiClient<Product[]>('/api/v1/products'),
        apiClient<CustCategory[]>('/api/v1/customer-categories').catch(() => []),
        apiClient<ProdCategory[]>('/api/v1/categories').catch(() => []),
      ]);
      setProducts(prods); setCustCategories(cats); setProdCategories(prodCats);
    } catch (e) { setError((e as Error).message); }
  }

  async function load() { await Promise.all([loadPromos(), loadLookups()]); }

  useEffect(() => { void loadLookups(); }, []);
  useEffect(() => { void loadPromos(); }, [pageNumber, pageSize, statusFilter, typeFilter]);
  useEffect(() => {
    const t = window.setTimeout(() => { setPageNumber(1); void loadPromos(); }, 350);
    return () => window.clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [query]);

  // Local 'YYYY-MM-DD' today — a new promotion can't start before today.
  const todayStr = new Date(Date.now() - new Date().getTimezoneOffset() * 60000).toISOString().slice(0, 10);

  function openNew() { setForm({ ...emptyForm, lines: [] }); setOpen(true); }
  function openEdit(p: Promo) {
    setForm({
      id: p.id, code: p.code, name: p.name, promoType: p.promoType, isActive: p.isActive, autoApply: p.autoApply,
      priority: String(p.priority), startsOn: p.startsOn ?? '', endsOn: p.endsOn ?? '', daysMask: p.daysMask,
      startTime: (p.startTime ?? '').slice(0, 5), endTime: (p.endTime ?? '').slice(0, 5),
      appliesToOrderType: p.appliesToOrderType ?? '', appliesToCategoryId: p.appliesToCategoryId ?? '', displayMessage: p.displayMessage ?? '',
      bundlePrice: String(p.lines?.find(l => l.bundlePrice != null)?.bundlePrice ?? ''),
      lines: (p.lines ?? []).map(l => ({
        productId: l.productId ?? '', categoryId: l.categoryId ?? '', targetType: l.categoryId ? 'category' : 'product',
        minQty: String(l.minQty ?? 0), billFrom: String(l.billFrom ?? 0),
        billTo: l.billTo == null ? '' : String(l.billTo),
        getProductId: l.getProductId ?? '', getQty: String(l.getQty ?? 0),
        discountPercent: String(l.discountPercent ?? 0), discountAmount: String(l.discountAmount ?? 0),
      })),
    });
    setOpen(true);
  }
  const toggleDay = (i: number) => setForm(f => ({ ...f, daysMask: f.daysMask ^ (1 << i) }));
  const addLine = () => setForm(f => ({ ...f, lines: [...f.lines, { productId: '', categoryId: '', targetType: 'product', minQty: '1', billFrom: '0', billTo: '', getProductId: '', getQty: '1', discountPercent: '', discountAmount: '' }] }));
  const setLine = (idx: number, patch: Partial<FormLine>) => setForm(f => ({ ...f, lines: f.lines.map((x, i) => i === idx ? { ...x, ...patch } : x) }));
  const num = (s: string) => s.replace(/[^0-9.]/g, '');
  const RULES_LABEL: Record<string, string> = {
    product_discount: 'Product discounts', time_based: 'Happy-hour discount', bill_value: 'Spend tiers', buy_x_get_y: 'Buy / get rules', bundle: 'Bundle components', lowest_price: 'Lowest-price rule',
  };
  const productOptions = products.map(p => <option key={p.id} value={p.id}>{p.name}</option>);
  const categoryOptions = prodCategories.map(c => <option key={c.id} value={c.id}>{c.parentId ? '— ' : ''}{c.name}</option>);

  async function save() {
    if (!form.code.trim() || !form.name.trim()) { flash('Code and name are required.'); return; }
    if (!form.promoType) { flash('Choose a promotion type.'); return; }
    if (form.daysMask === 0) { flash('Select at least one day the promotion runs (or tap “Every day”).'); return; }
    if (!form.id && form.startsOn && form.startsOn < todayStr) { flash('A new promotion can’t start in the past.'); return; }
    if (form.startsOn && form.endsOn && form.endsOn < form.startsOn) { flash('End date must be on or after the start date.'); return; }
    // Type-specific guards (mirror the API) so saving shows a clear message instead of a raw 400.
    const t = form.promoType;
    if (t !== 'bill_value' && t !== 'time_based' && form.lines.length === 0) { flash('Add at least one product line for this promotion.'); return; }
    if (t === 'bill_value' && form.lines.length === 0) { flash('Add at least one spend tier.'); return; }
    if (t === 'time_based') {
      if (!form.startTime || !form.endTime) { flash('Set the “Active from / to (time)” fields — a happy-hour promo runs inside a time window.'); return; }
      if (!form.lines.some(l => Number(l.discountPercent) > 0 || Number(l.discountAmount) > 0)) { flash('Enter a happy-hour discount — a % off or an LKR amount.'); return; }
    }
    if (t === 'product_discount' && form.lines.some(l => l.targetType === 'category' ? !l.categoryId : !l.productId)) { flash('Each item-discount line needs a product or a category selected.'); return; }
    if (t === 'buy_x_get_y' && form.lines.some(l => !l.productId || !l.getProductId)) { flash('Each Buy-X-Get-Y rule needs both a “buy” and a “get” product.'); return; }
    if (t === 'bundle' && (!(Number(form.bundlePrice) > 0) || form.lines.some(l => !l.productId))) { flash('A bundle needs component products and a bundle price.'); return; }
    if ((form.startTime && !form.endTime) || (!form.startTime && form.endTime)) { flash('Set both a start and end time for the time window — or leave both blank.'); return; }
    setSaving(true);
    try {
      const payload = {
        id: form.id, code: form.code.trim(), name: form.name.trim(), promoType: form.promoType,
        isActive: form.isActive, autoApply: form.autoApply, priority: Number(form.priority) || 0,
        startsOn: form.startsOn || null, endsOn: form.endsOn || null, daysMask: form.daysMask,
        startTime: form.startTime || null, endTime: form.endTime || null,
        appliesToOrderType: form.appliesToOrderType || null, appliesToCategoryId: form.appliesToCategoryId || null, displayMessage: form.displayMessage || null,
        lines: form.lines.map(l => ({
          // product_discount lines can target a category instead of a single product.
          productId: form.promoType === 'bill_value' ? null
            : (form.promoType === 'product_discount' && l.targetType === 'category') ? null
            : (l.productId || null),
          categoryId: (form.promoType === 'product_discount' && l.targetType === 'category') ? (l.categoryId || null) : null,
          minQty: Number(l.minQty) || 0, billFrom: Number(l.billFrom) || 0,
          billTo: l.billTo.trim() === '' ? null : Number(l.billTo),
          getProductId: form.promoType === 'buy_x_get_y' ? (l.getProductId || null) : null,
          getQty: form.promoType === 'buy_x_get_y' ? (Number(l.getQty) || 0) : 0,
          bundlePrice: form.promoType === 'bundle' ? (Number(form.bundlePrice) || 0) : null,
          discountPercent: Number(l.discountPercent) || 0, discountAmount: Number(l.discountAmount) || 0,
        })),
      };
      await apiClient('/api/v1/promotions', { method: 'PUT', body: JSON.stringify(payload) });
      setOpen(false); flash('Promotion saved.'); await load();
    } catch (e) { flash(extractError(e, 'Could not save the promotion.')); }
    finally { setSaving(false); }
  }
  async function remove(p: Promo) {
    if (!(await confirmDialog({ title: `Remove ${p.name}?`, body: 'This promotion will be deleted permanently. This cannot be undone.', confirmLabel: 'Remove', danger: true }))) return;
    try { await apiClient(`/api/v1/promotions/${p.id}`, { method: 'DELETE' }); flash(`${p.name} removed.`); await load(); }
    catch (e) { flash(extractError(e, 'Could not remove the promotion.')); }
  }

  function schedule(p: Promo) {
    const days = p.daysMask === 127 ? 'every day' : DAYS.filter((_, i) => p.daysMask & (1 << i)).join(' ');
    const win = p.startTime && p.endTime ? ` · ${p.startTime.slice(0, 5)}–${p.endTime.slice(0, 5)}` : '';
    return `${days}${win}`;
  }

  return (
    <>
      <Topbar title="Promotions" subtitle="Create deals that fill tables and grow every bill" />
      <div className="p-6 md:p-8">
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Promotions</h2>
            <HeaderStat><Num>{totalCount}</Num> configured · auto-applied at the till</HeaderStat>
          </div>
          <button onClick={openNew} className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark">
            <Plus className="size-4" /> New Promotion
          </button>
        </div>

        {!loading && !error && (
          <div className="mb-4 flex flex-wrap items-center gap-2">
            <div className="relative">
              <Search className="absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
              <input
                value={query}
                onChange={e => setQuery(e.target.value)}
                placeholder="Search code / name"
                className="rounded-lg border border-border bg-card py-2 pl-8 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30"
              />
            </div>
            <select
              value={statusFilter}
              onChange={e => setStatusFilter(e.target.value)}
              className="rounded-lg border border-border bg-card px-3 py-2 text-sm"
            >
              <option value="">All statuses</option>
              <option value="active">Active</option>
              <option value="inactive">Inactive</option>
            </select>
            <select
              value={typeFilter}
              onChange={e => setTypeFilter(e.target.value)}
              className="rounded-lg border border-border bg-card px-3 py-2 text-sm"
            >
              <option value="">All types</option>
              {Object.entries(TYPE_LABEL).map(([value, label]) => (
                <option key={value} value={value}>{label}</option>
              ))}
            </select>
          </div>
        )}

        <div className="card overflow-hidden">
          {loading ? (
            <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
          ) : error ? (
            <div className="p-6 text-sm text-status-error">{error}</div>
          ) : (
            <table className="w-full text-sm">
              <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th className="px-4 py-3 font-medium">Promotion</th>
                  <th className="px-4 py-3 font-medium">Type</th>
                  <th className="px-4 py-3 font-medium">When</th>
                  <th className="px-4 py-3 font-medium">Status</th>
                  <th className="px-4 py-3 text-right font-medium">Actions</th>
                </tr>
              </thead>
              <tbody>
                {promos.map((p, i) => (
                  <tr key={p.id} className={i % 2 ? 'bg-muted/20' : ''}>
                    <td className="px-4 py-3"><span className="font-medium">{p.name}</span> <span className="ml-1 font-mono text-xs text-muted-foreground">{p.code}</span></td>
                    <td className="px-4 py-3"><span className="pill pill-idle">{TYPE_LABEL[p.promoType] ?? p.promoType}</span></td>
                    <td className="px-4 py-3 text-xs text-muted-foreground">{schedule(p)}{p.appliesToOrderType ? ` · ${p.appliesToOrderType.replace('_', '-')}` : ''}</td>
                    <td className="px-4 py-3"><span className={`pill ${p.isActive ? 'pill-paid' : 'pill-void'}`}>{p.isActive ? 'Active' : 'Off'}</span></td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex justify-end gap-2">
                        <button onClick={() => openEdit(p)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted">Edit</button>
                        <button onClick={() => remove(p)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-status-error hover:bg-muted">Remove</button>
                      </div>
                    </td>
                  </tr>
                ))}
                {promos.length === 0 && <tr><td colSpan={5} className="px-4 py-10 text-center text-muted-foreground">{totalCount === 0 ? 'No promotions yet. Create a time-based or spend-and-save deal.' : 'No promotions match your filters.'}</td></tr>}
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
            noun="promotions"
            className="mt-0 flex-1"
          />
        </div>
      </div>

      {open && (
        <Modal
          title={form.id ? 'Edit Promotion' : 'New Promotion'}
          icon={<Percent className="size-4" />}
          onClose={() => !saving && setOpen(false)}
          size="2xl"
          footer={
            <div className="flex gap-2">
              <button onClick={() => setOpen(false)} disabled={saving} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50">Cancel</button>
              <button onClick={save} disabled={saving} className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{saving ? 'Saving…' : 'Save promotion'}</button>
            </div>
          }
        >
            <div className="grid grid-cols-2 gap-3">
              <Field label="Code" mono value={form.code} onChange={v => setForm(f => ({ ...f, code: v.toUpperCase() }))} />
              <Field label="Name" value={form.name} onChange={v => setForm(f => ({ ...f, name: v }))} />
              <Combobox label="Type" value={form.promoType} placeholder="Select a type…" onChange={v => setForm(f => ({ ...f, promoType: v, lines: v === 'time_based' && f.lines.length === 0 ? [{ productId: '', categoryId: '', targetType: 'product', minQty: '1', billFrom: '0', billTo: '', getProductId: '', getQty: '1', discountPercent: '', discountAmount: '' }] : f.lines }))}
                options={[
                  { value: 'product_discount', label: 'Product discount' },
                  { value: 'time_based', label: 'Time-based (happy hour)' },
                  { value: 'bill_value', label: 'Spend & save (bill value)' },
                  { value: 'buy_x_get_y', label: 'Buy X get Y (BOGO)' },
                  { value: 'bundle', label: 'Bundle / combo' },
                  { value: 'lowest_price', label: 'Lowest-price (3-for-2)' },
                ]} />
              <Combobox label="Applies to" value={form.appliesToOrderType} onChange={v => setForm(f => ({ ...f, appliesToOrderType: v }))}
                options={[
                  { value: '', label: 'Any order type' },
                  { value: 'dine_in', label: 'Dine-in' },
                  { value: 'takeaway', label: 'Takeaway' },
                  { value: 'delivery', label: 'Delivery' },
                ]} />
              <Combobox label="Customer segment" value={form.appliesToCategoryId} onChange={v => setForm(f => ({ ...f, appliesToCategoryId: v }))}
                options={[{ value: '', label: 'Any customer' }, ...custCategories.map(c => ({ value: c.id, label: c.name }))]} />
              <Field label="Starts" type="date" value={form.startsOn} onChange={v => setForm(f => ({ ...f, startsOn: v }))} min={form.id ? undefined : todayStr} />
              <Field label="Ends" type="date" value={form.endsOn} onChange={v => setForm(f => ({ ...f, endsOn: v }))} min={form.startsOn || (form.id ? undefined : todayStr)} />
              <Field label="Active from (time)" type="time" value={form.startTime} onChange={v => setForm(f => ({ ...f, startTime: v }))} />
              <Field label="Active to (time)" type="time" value={form.endTime} onChange={v => setForm(f => ({ ...f, endTime: v }))} />
            </div>

            <div className="mt-3">
              <label className="mb-1 block text-sm font-semibold text-slate-700">Days <span className="font-normal text-muted-foreground">— pick the days this runs</span></label>
              <div className="flex flex-wrap items-center gap-1.5">
                <button type="button" onClick={() => setForm(f => ({ ...f, daysMask: f.daysMask === 127 ? 0 : 127 }))} className={`pill ${form.daysMask === 127 ? 'bg-primary text-primary-foreground' : 'pill-idle'}`}>Every day</button>
                <span className="text-muted-foreground">·</span>
                {DAYS.map((d, i) => {
                  const on = (form.daysMask & (1 << i)) !== 0;
                  return <button key={d} type="button" onClick={() => toggleDay(i)} className={`pill ${on ? 'bg-primary text-primary-foreground' : 'pill-idle'}`}>{d}</button>;
                })}
              </div>
            </div>

            <div className="mt-4">
              <div className="mb-1 flex items-center justify-between">
                <label className="block text-sm font-semibold text-slate-700">{RULES_LABEL[form.promoType] ?? 'Rules'}</label>
                {form.promoType && <button type="button" onClick={addLine} className="text-xs font-medium text-primary hover:underline">+ Add row</button>}
              </div>

              {!form.promoType && <p className="rounded-lg border border-dashed border-border px-3 py-2 text-xs text-muted-foreground">Choose a promotion type above to add its rules.</p>}

              {form.promoType === 'time_based' && <p className="mb-1.5 text-xs text-muted-foreground">A flat discount off the <b>whole bill</b>, live only during the time window you set below (and on the days you pick). The classic “happy hour”. Set the <b>Active from / to (time)</b> fields below.</p>}
              {form.promoType === 'buy_x_get_y' && <p className="mb-1.5 text-xs text-muted-foreground">For every <b>buy qty</b> of the buy product, the <b>get qty</b> of the get product is discounted (leave % / LKR blank = free). Same product on both sides = classic “buy 1 get 1”.</p>}
              {form.promoType === 'bundle' && (
                <div className="mb-2 flex items-center gap-2">
                  <span className="text-xs text-muted-foreground">Bundle price (LKR) for all the components together:</span>
                  <input value={form.bundlePrice} onChange={e => setForm(f => ({ ...f, bundlePrice: num(e.target.value) }))} placeholder="e.g. 1200" className="w-28 rounded-lg border border-border bg-surface px-2 py-1.5 text-right text-sm" />
                </div>
              )}
              {form.lines.length === 0 && <p className="text-xs text-muted-foreground">Add at least one row.</p>}

              <div className="space-y-2">
                {form.lines.map((l, idx) => {
                  const selCls = 'flex-1 rounded-lg border border-border bg-surface px-2 py-1.5 text-sm';
                  const numCls = 'rounded-lg border border-border bg-surface px-2 py-1.5 text-right text-sm';
                  return (
                    <div key={idx} className="flex flex-wrap items-center gap-2">
                      {form.promoType === 'product_discount' && (
                        <>
                          <select value={l.targetType} onChange={e => setLine(idx, { targetType: e.target.value, productId: '', categoryId: '' })} className={`w-28 ${selCls} flex-none`}>
                            <option value="product">Item</option>
                            <option value="category">Category</option>
                          </select>
                          {l.targetType === 'category' ? (
                            <select value={l.categoryId} onChange={e => setLine(idx, { categoryId: e.target.value })} className={selCls}>
                              <option value="">— category —</option>{categoryOptions}
                            </select>
                          ) : (
                            <select value={l.productId} onChange={e => setLine(idx, { productId: e.target.value })} className={selCls}>
                              <option value="">— product —</option>{productOptions}
                            </select>
                          )}
                          <input value={l.minQty} onChange={e => setLine(idx, { minQty: num(e.target.value) })} placeholder="min qty" className={`w-20 ${numCls}`} />
                          <input value={l.discountPercent} onChange={e => setLine(idx, { discountPercent: num(e.target.value) })} placeholder="% off" className={`w-20 ${numCls}`} />
                          <input value={l.discountAmount} onChange={e => setLine(idx, { discountAmount: num(e.target.value) })} placeholder="or LKR" className={`w-24 ${numCls}`} />
                        </>
                      )}
                      {form.promoType === 'time_based' && (
                        <>
                          <span className="rounded-lg border border-border bg-muted/40 px-3 py-1.5 text-sm text-muted-foreground">Whole bill</span>
                          <input value={l.discountPercent} onChange={e => setLine(idx, { discountPercent: num(e.target.value) })} placeholder="% off" className={`w-20 ${numCls}`} />
                          <input value={l.discountAmount} onChange={e => setLine(idx, { discountAmount: num(e.target.value) })} placeholder="or LKR" className={`w-24 ${numCls}`} />
                        </>
                      )}
                      {form.promoType === 'bill_value' && (
                        <>
                          <input value={l.billFrom} onChange={e => setLine(idx, { billFrom: num(e.target.value) })} placeholder="spend ≥" className={`w-28 ${numCls}`} />
                          <input value={l.billTo} onChange={e => setLine(idx, { billTo: num(e.target.value) })} placeholder="up to (∞)" className={`w-28 ${numCls}`} />
                          <input value={l.discountPercent} onChange={e => setLine(idx, { discountPercent: num(e.target.value) })} placeholder="% off" className={`w-20 ${numCls}`} />
                          <input value={l.discountAmount} onChange={e => setLine(idx, { discountAmount: num(e.target.value) })} placeholder="or LKR" className={`w-24 ${numCls}`} />
                        </>
                      )}
                      {form.promoType === 'buy_x_get_y' && (
                        <>
                          <select value={l.productId} onChange={e => setLine(idx, { productId: e.target.value })} className={selCls}>
                            <option value="">— buy product —</option>{productOptions}
                          </select>
                          <input value={l.minQty} onChange={e => setLine(idx, { minQty: num(e.target.value) })} placeholder="buy qty" className={`w-20 ${numCls}`} />
                          <select value={l.getProductId} onChange={e => setLine(idx, { getProductId: e.target.value })} className={selCls}>
                            <option value="">— get product —</option>{productOptions}
                          </select>
                          <input value={l.getQty} onChange={e => setLine(idx, { getQty: num(e.target.value) })} placeholder="get qty" className={`w-20 ${numCls}`} />
                          <input value={l.discountPercent} onChange={e => setLine(idx, { discountPercent: num(e.target.value) })} placeholder="% off (blank=free)" className={`w-28 ${numCls}`} />
                        </>
                      )}
                      {form.promoType === 'bundle' && (
                        <>
                          <select value={l.productId} onChange={e => setLine(idx, { productId: e.target.value })} className={selCls}>
                            <option value="">— component —</option>{productOptions}
                          </select>
                          <input value={l.minQty} onChange={e => setLine(idx, { minQty: num(e.target.value) })} placeholder="qty" className={`w-20 ${numCls}`} />
                        </>
                      )}
                      {form.promoType === 'lowest_price' && (
                        <>
                          <select value={l.productId} onChange={e => setLine(idx, { productId: e.target.value })} className={selCls}>
                            <option value="">any item</option>{productOptions}
                          </select>
                          <input value={l.minQty} onChange={e => setLine(idx, { minQty: num(e.target.value) })} placeholder="group of" className={`w-24 ${numCls}`} />
                          <input value={l.getQty} onChange={e => setLine(idx, { getQty: num(e.target.value) })} placeholder="free / grp" className={`w-24 ${numCls}`} />
                          <input value={l.discountPercent} onChange={e => setLine(idx, { discountPercent: num(e.target.value) })} placeholder="% off (blank=free)" className={`w-28 ${numCls}`} />
                        </>
                      )}
                      <button type="button" onClick={() => setForm(f => ({ ...f, lines: f.lines.filter((_, i) => i !== idx) }))} className="rounded-lg p-1.5 text-muted-foreground hover:bg-muted hover:text-status-error"><X className="size-4" /></button>
                    </div>
                  );
                })}
              </div>
            </div>

            <div className="mt-3 flex flex-wrap gap-6">
              <label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={form.isActive} onChange={e => setForm(f => ({ ...f, isActive: e.target.checked }))} className="size-4 rounded border-border text-primary" /> Active</label>
              <label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={form.autoApply} onChange={e => setForm(f => ({ ...f, autoApply: e.target.checked }))} className="size-4 rounded border-border text-primary" /> Auto-apply at till</label>
            </div>
        </Modal>
      )}

      {toast && <div className="fixed bottom-12 left-1/2 z-[70] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">{toast}</div>}
    </>
  );
}

function extractError(e: unknown, fallback: string): string {
  const msg = (e as Error)?.message ?? '';
  const j = msg.indexOf('{');
  if (j !== -1) { try { const p = JSON.parse(msg.slice(j)); if (typeof p?.error === 'string') return p.error; } catch { /* */ } }
  if (msg.includes('403')) return 'Only an owner or manager can manage promotions.';
  return msg || fallback;
}
