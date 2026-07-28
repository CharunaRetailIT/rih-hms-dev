'use client';

import { useCallback, useEffect, useState } from 'react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient, money } from '@/lib/api-client';
import { confirmDialog } from '@/components/ui/confirm';
import { Field, Combobox } from '@/components/ui/form';
import { AddressFields } from '@/components/ui/AddressFields';
import { type Address, EMPTY_ADDRESS } from '@/lib/regions';
import { Plus, X, Truck, Pencil, CalendarPlus, Printer } from 'lucide-react';

type Hall = { id: string; code: string; name: string; capacity: number; notes: string | null; isActive: boolean };
type Pkg = { id: string; code: string; name: string; pricePerHead: number; description: string | null; isActive: boolean; recipeProductId: string | null };
type RecipeOpt = { productId: string; productName: string };
type EventRow = {
  id: string; eventNo: string; title: string | null; status: string; pax: number; startsAt: string; endsAt: string | null;
  hallId: string | null; hall: string | null; packageId: string | null; customerName: string | null; customerPhone: string | null;
  pricePerHead: number; packageTotal: number; extrasTotal: number; discountAmount: number; totalAmount: number; paidAmount: number; balance: number;
  isOffsite: boolean; deliveryAddress: string | null; vehicle: string | null; driver: string | null; dispatchStatus: string | null; notes: string | null;
  countryCode: string | null; province: string | null; district: string | null; postalCode: string | null;
};
type Detail = EventRow & {
  package: string | null; foodCost: number; produced: boolean; productionOrderId: string | null; margin: number;
  items: { id: string; description: string; quantity: number; unitPrice: number; lineTotal: number }[];
  payments: { id: string; amount: number; payType: string; kind: string; reference: string | null; paidAt: string }[];
};
const cls = 'w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20';
const fmt = (ts: string) => new Date(ts).toLocaleString('en-LK', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
const STATUS_PILL: Record<string, string> = { enquiry: 'pill-idle', quote: 'pill-pending', confirmed: 'pill-idle', running: 'pill-pending', completed: 'pill-paid', cancelled: 'pill-void' };
const CATERING_STATUSES = ['enquiry', 'quote', 'confirmed', 'running', 'completed', 'cancelled'];

const escH = (s: string | null | undefined) => (s ?? '').replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]!));

// Printable / downloadable customer quotation for a catering booking.
function quoteHtml(d: Detail, business: string): string {
  const m = (n: number) => `${money(n)}`;
  const rows = [
    d.packageId || d.package
      ? `<tr><td>${escH(d.package || 'Package')} — ${d.pax} pax @ ${m(d.pricePerHead)}/head</td><td class="r">${m(d.packageTotal)}</td></tr>`
      : '',
    ...d.items.map(i => `<tr><td>${escH(i.description)} <span class="muted">×${i.quantity}</span></td><td class="r">${m(i.lineTotal)}</td></tr>`),
  ].filter(Boolean).join('');
  const line = (label: string, val: string, strong = false) => `<tr class="${strong ? 'tot' : ''}"><td>${label}</td><td class="r">${val}</td></tr>`;
  return `<!doctype html><html><head><meta charset="utf-8"><title>Quotation ${escH(d.eventNo)}</title>
<style>
  *{font-family:system-ui,Arial,sans-serif} body{max-width:720px;margin:24px auto;padding:0 24px;color:#111}
  h1{font-size:22px;margin:0} .muted{color:#666;font-size:12px} .head{display:flex;justify-content:space-between;align-items:flex-start;border-bottom:2px solid #15803d;padding-bottom:10px}
  .badge{color:#15803d;font-weight:800;letter-spacing:1px} table{width:100%;border-collapse:collapse;margin-top:14px}
  td{padding:6px 0;border-bottom:1px solid #eee;font-size:14px} .r{text-align:right} tr.tot td{border-top:2px solid #111;border-bottom:none;font-weight:800;font-size:16px;padding-top:8px}
  .meta{margin-top:12px;font-size:13px;line-height:1.6} .foot{margin-top:24px;font-size:12px;color:#666}
</style></head><body onload="window.focus()">
  <div class="head"><div><h1>${escH(business)}</h1><div class="muted">Catering Quotation</div></div><div class="badge">QUOTATION</div></div>
  <div class="meta">
    <strong>Quote no:</strong> ${escH(d.eventNo)}<br>
    <strong>Event:</strong> ${escH(d.title || d.eventNo)} · ${fmt(d.startsAt)}${d.hall ? ` · ${escH(d.hall)}` : ''}<br>
    <strong>Guests:</strong> ${d.pax} pax${d.customerName ? `<br><strong>Customer:</strong> ${escH(d.customerName)}${d.customerPhone ? ` (${escH(d.customerPhone)})` : ''}` : ''}
    ${d.isOffsite ? `<br><strong>Delivery:</strong> ${escH([d.deliveryAddress, d.district, d.countryCode].filter(Boolean).join(', '))}` : ''}
  </div>
  <table><tbody>${rows}</tbody><tbody>
    ${line('Subtotal', m(d.packageTotal + d.extrasTotal))}
    ${d.discountAmount > 0 ? line('Discount', `- ${m(d.discountAmount)}`) : ''}
    ${(d as { serviceCharge?: number }).serviceCharge ? line('Service charge', m((d as { serviceCharge?: number }).serviceCharge ?? 0)) : ''}
    ${(d as { taxAmount?: number }).taxAmount ? line('Tax', m((d as { taxAmount?: number }).taxAmount ?? 0)) : ''}
    ${line('TOTAL', m(d.totalAmount), true)}
  </tbody></table>
  <div class="foot">This quotation is an estimate and valid for 14 days. Prices are subject to final confirmation. Thank you for considering ${escH(business)}.</div>
</body></html>`;
}
function printDoc(html: string) { const w = window.open('', '_blank', 'width=760,height=920'); if (!w) return; w.document.open(); w.document.write(html); w.document.close(); setTimeout(() => w.print(), 250); }
type Tab = 'bookings' | 'halls' | 'packages';

export default function CateringPage() {
  const [tab, setTab] = useState<Tab>('bookings');
  const [toast, setToast] = useState<string | null>(null);
  const flash = (m: string) => { setToast(m); window.setTimeout(() => setToast(null), 3000); };
  const err = (e: unknown) => { const m = (e as Error)?.message ?? ''; const i = m.indexOf('{'); if (i >= 0) { try { return JSON.parse(m.slice(i)).error ?? m; } catch { /* */ } } return m; };

  return (
    <>
      <Topbar title="Catering" subtitle="Win events and manage every booking from enquiry to celebration" />
      <div className="p-6 md:p-8">
        <div className="mb-4 flex gap-1 border-b border-border">
          {(['bookings', 'halls', 'packages'] as Tab[]).map(t => (
            <button key={t} onClick={() => setTab(t)} className={`px-4 py-2 text-sm font-semibold capitalize ${tab === t ? 'border-b-2 border-primary text-primary' : 'text-muted-foreground hover:text-on-surface'}`}>{t}</button>
          ))}
        </div>
        {tab === 'bookings' && <Bookings flash={flash} err={err} />}
        {tab === 'halls' && <Halls flash={flash} err={err} />}
        {tab === 'packages' && <Packages flash={flash} err={err} />}
      </div>
      {toast && <div className="fixed bottom-12 left-1/2 z-[80] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">{toast}</div>}
    </>
  );
}

function Bookings({ flash, err }: { flash: (m: string) => void; err: (e: unknown) => string }) {
  const today = () => new Date().toISOString().slice(0, 10);
  const [from, setFrom] = useState(today());
  const [to, setTo] = useState(new Date(Date.now() + 90 * 86400000).toISOString().slice(0, 10));
  const [status, setStatus] = useState('');
  const [rows, setRows] = useState<EventRow[]>([]);
  const [halls, setHalls] = useState<Hall[]>([]);
  const [pkgs, setPkgs] = useState<Pkg[]>([]);
  const [form, setForm] = useState(false);
  const [detail, setDetail] = useState<Detail | null>(null);

  const load = useCallback(async () => {
    try {
      const qs = new URLSearchParams({ from, to: new Date(new Date(to).getTime() + 86400000).toISOString().slice(0, 10) });
      if (status) qs.set('status', status);
      setRows(await apiClient<EventRow[]>(`/api/v1/catering/events?${qs}`));
    } catch (e) { flash(err(e)); }
  }, [from, to, status]);
  useEffect(() => { void load(); }, []);
  useEffect(() => { apiClient<Hall[]>('/api/v1/catering/halls').then(setHalls).catch(() => {}); apiClient<Pkg[]>('/api/v1/catering/packages').then(setPkgs).catch(() => {}); }, []);

  async function openDetail(id: string) { try { setDetail(await apiClient<Detail>(`/api/v1/catering/events/${id}`)); } catch (e) { flash(err(e)); } }
  const refreshDetail = async (id: string) => { setDetail(await apiClient<Detail>(`/api/v1/catering/events/${id}`)); void load(); };

  return (
    <>
      <div className="mb-4 flex flex-wrap items-end gap-3">
        <Field className="w-40" label="From" type="date" value={from} onChange={setFrom} />
        <Field className="w-40" label="To" type="date" value={to} onChange={setTo} />
        <Combobox className="w-44" label="Status" value={status} onChange={setStatus}
          placeholder="All" searchPlaceholder="Search status…"
          options={[{ value: '', label: 'All' }, ...CATERING_STATUSES.map(s => ({ value: s, label: s }))]} />
        <button onClick={() => void load()} className="rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground hover:bg-primary-dark">Apply</button>
        <button onClick={() => setForm(true)} className="ml-auto flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-bold text-primary-foreground hover:bg-primary-dark"><Plus className="size-4" /> New Booking</button>
      </div>

      <div className="card overflow-hidden">
        <table className="w-full text-sm">
          <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
            <tr><th className="px-3 py-3">Event</th><th className="px-3 py-3">When</th><th className="px-3 py-3">Hall</th><th className="px-3 py-3">Pax</th><th className="px-3 py-3">Status</th><th className="px-3 py-3 text-right">Total</th><th className="px-3 py-3 text-right">Balance</th></tr>
          </thead>
          <tbody>
            {rows.map((r, i) => (
              <tr key={r.id} onClick={() => openDetail(r.id)} className={`cursor-pointer hover:bg-primary-tint/40 ${i % 2 ? 'bg-muted/20' : ''}`}>
                <td className="px-3 py-2.5"><span className="font-semibold">{r.title || r.eventNo}</span> {r.isOffsite && <Truck className="ml-1 inline size-3.5 text-muted-foreground" />}<div className="text-xs text-muted-foreground">{r.eventNo}{r.customerName ? ` · ${r.customerName}` : ''}</div></td>
                <td className="px-3 py-2.5 whitespace-nowrap text-muted-foreground">{fmt(r.startsAt)}</td>
                <td className="px-3 py-2.5">{r.hall ?? (r.isOffsite ? 'Off-site' : '—')}</td>
                <td className="px-3 py-2.5 tabular-nums">{r.pax}</td>
                <td className="px-3 py-2.5"><span className={`pill ${STATUS_PILL[r.status] ?? 'pill-idle'}`}>{r.status}</span></td>
                <td className="px-3 py-2.5 text-right tabular-nums">{money(r.totalAmount)}</td>
                <td className="px-3 py-2.5 text-right font-bold tabular-nums">{r.balance > 0 ? <span className="text-status-pending">{money(r.balance)}</span> : <span className="text-status-paid">0.00</span>}</td>
              </tr>
            ))}
            {rows.length === 0 && <tr><td colSpan={7} className="px-4 py-10 text-center text-muted-foreground">No bookings in this range.</td></tr>}
          </tbody>
        </table>
      </div>

      {form && <BookingForm halls={halls} pkgs={pkgs} onClose={() => setForm(false)} onSaved={() => { setForm(false); void load(); }} flash={flash} err={err} />}
      {detail && <BookingDetail d={detail} halls={halls} pkgs={pkgs} onClose={() => setDetail(null)} refresh={() => refreshDetail(detail.id)} flash={flash} err={err} />}
    </>
  );
}

function BookingForm({ halls, pkgs, initial, onClose, onSaved, flash, err }: { halls: Hall[]; pkgs: Pkg[]; initial?: Detail | null; onClose: () => void; onSaved: () => void; flash: (m: string) => void; err: (e: unknown) => string }) {
  const editing = !!initial;
  // ISO (UTC) → local "YYYY-MM-DDTHH:mm" for datetime-local inputs.
  const toLocal = (iso?: string | null) => iso ? new Date(new Date(iso).getTime() - new Date(iso).getTimezoneOffset() * 60000).toISOString().slice(0, 16) : '';
  const [f, setF] = useState({
    title: initial?.title ?? '', customerName: initial?.customerName ?? '', customerPhone: initial?.customerPhone ?? '',
    hallId: initial?.hallId ?? '', packageId: initial?.packageId ?? '',
    pax: initial ? String(initial.pax) : '', startsAt: toLocal(initial?.startsAt), endsAt: toLocal(initial?.endsAt),
    discountAmount: initial?.discountAmount ? String(initial.discountAmount) : '',
    isOffsite: initial?.isOffsite ?? false, deliveryAddress: initial?.deliveryAddress ?? '', vehicle: initial?.vehicle ?? '', driver: initial?.driver ?? '', notes: initial?.notes ?? '',
  });
  const [addr, setAddr] = useState<Address>({ countryCode: initial?.countryCode ?? '', province: initial?.province ?? '', district: initial?.district ?? '', postalCode: initial?.postalCode ?? '' });
  const [busy, setBusy] = useState(false);
  // datetime-local min = now (no back-dating a NEW booking; an existing one may
  // already be in the past, so don't constrain it on edit).
  const nowLocal = new Date(Date.now() - new Date().getTimezoneOffset() * 60000).toISOString().slice(0, 16);
  const set = (k: keyof typeof f, v: string | boolean) => setF(s => ({ ...s, [k]: v }));
  async function save() {
    if (!f.title.trim() && !f.customerName.trim()) { flash('Add an event title or a customer name'); return; }
    if (!(Number(f.pax) >= 1)) { flash('Set the head-count (pax) — at least 1 guest'); return; }
    if (!f.startsAt) { flash('Pick a start date/time'); return; }
    if (!editing && new Date(f.startsAt).getTime() < Date.now() - 60000) { flash('A booking can’t start in the past'); return; }
    if (f.endsAt && new Date(f.endsAt) <= new Date(f.startsAt)) { flash('End must be after the start'); return; }
    if (f.isOffsite) {
      if (!f.deliveryAddress.trim()) { flash('An off-site booking needs a delivery street address'); return; }
      if (!addr.countryCode) { flash('Select the delivery country for an off-site booking'); return; }
      if (!addr.district.trim()) { flash('Enter the delivery city/district for an off-site booking'); return; }
    }
    setBusy(true);
    try {
      await apiClient('/api/v1/catering/events', { method: 'POST', body: JSON.stringify({
        id: initial?.id ?? null,   // present → update the existing booking
        title: f.title || null, customerName: f.customerName || null, customerPhone: f.customerPhone || null,
        hallId: f.hallId || null, packageId: f.packageId || null, pax: Number(f.pax),
        startsAt: f.startsAt, endsAt: f.endsAt || null, discountAmount: Number(f.discountAmount) || 0,
        isOffsite: f.isOffsite, deliveryAddress: f.deliveryAddress || null, vehicle: f.vehicle || null, driver: f.driver || null, notes: f.notes || null,
        countryCode: f.isOffsite ? (addr.countryCode || null) : null, province: f.isOffsite ? (addr.province.trim() || null) : null,
        district: f.isOffsite ? (addr.district.trim() || null) : null, postalCode: f.isOffsite ? (addr.postalCode.trim() || null) : null,
      }) });
      flash(editing ? 'Booking updated.' : 'Booking saved.'); onSaved();
    } catch (e) { flash(err(e)); } finally { setBusy(false); }
  }
  return (
    <Modal title={editing ? `Edit ${initial?.eventNo ?? 'booking'}` : 'New Booking'} icon={<CalendarPlus className="size-4" />} onClose={onClose}>
      <div className="grid grid-cols-2 gap-3">
        <Field className="col-span-2" label="Event title" value={f.title} onChange={v => set('title', v)} placeholder="Perera Wedding" helper="Title or customer required" />
        <Field label="Customer" value={f.customerName} onChange={v => set('customerName', v)} />
        <Field label="Phone" value={f.customerPhone} onChange={v => set('customerPhone', v)} />
        <Combobox up label="Hall" value={f.hallId} onChange={v => set('hallId', v)}
          placeholder="—" searchPlaceholder="Search halls…"
          options={[{ value: '', label: '—' }, ...halls.map(h => ({ value: h.id, label: `${h.name} (${h.capacity}p)` }))]} />
        <Combobox up label="Package" value={f.packageId} onChange={v => set('packageId', v)}
          placeholder="—" searchPlaceholder="Search packages…"
          options={[{ value: '', label: '—' }, ...pkgs.map(p => ({ value: p.id, label: `${p.name} · ${money(p.pricePerHead)}/head` }))]} />
        <Field label="Pax *" value={f.pax} onChange={v => set('pax', v.replace(/[^0-9]/g, ''))} inputMode="numeric" placeholder="e.g. 100" />
        <Field label="Discount (LKR)" value={f.discountAmount} onChange={v => set('discountAmount', v.replace(/[^0-9.]/g, ''))} inputMode="decimal" />
        <Field label="Starts *" type="datetime-local" value={f.startsAt} onChange={v => set('startsAt', v)} min={editing ? undefined : nowLocal} />
        <Field label="Ends" type="datetime-local" value={f.endsAt} onChange={v => set('endsAt', v)} min={f.startsAt || nowLocal} />
        <label className="col-span-2 flex items-center gap-2 text-sm"><input type="checkbox" checked={f.isOffsite} onChange={e => set('isOffsite', e.target.checked)} className="size-4 rounded" /> Off-site (own-fleet delivery)</label>
        {f.isOffsite && <>
          <Field className="col-span-2" label="Delivery street address *" value={f.deliveryAddress} onChange={v => set('deliveryAddress', v)} multiline />
          <div className="col-span-2"><AddressFields value={addr} onChange={setAddr} required /></div>
          <p className="col-span-2 text-xs text-muted-foreground">Vehicle &amp; driver are assigned later, at the delivery stage.</p>
        </>}
      </div>
      <div className="mt-5 flex gap-2">
        <button onClick={onClose} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted">Cancel</button>
        <button onClick={save} disabled={busy} className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{busy ? 'Saving…' : 'Save Booking'}</button>
      </div>
    </Modal>
  );
}

function BookingDetail({ d, halls, pkgs, onClose, refresh, flash, err }: { d: Detail; halls: Hall[]; pkgs: Pkg[]; onClose: () => void; refresh: () => Promise<void> | void; flash: (m: string) => void; err: (e: unknown) => string }) {
  const [busy, setBusy] = useState(false);
  const [edit, setEdit] = useState(false);
  const [it, setIt] = useState({ description: '', quantity: '1', unitPrice: '' });
  const [pay, setPay] = useState({ amount: '', kind: 'deposit', payType: 'cash' });
  const [fleet, setFleet] = useState({ vehicle: d.vehicle ?? '', driver: d.driver ?? '' });
  const act = async (fn: () => Promise<unknown>) => { setBusy(true); try { await fn(); await refresh(); } catch (e) { flash(err(e)); } finally { setBusy(false); } };
  const setStatus = async (s: string) => {
    if (s === 'cancelled' && !(await confirmDialog({ title: `Cancel ${d.title || d.eventNo}?`, body: 'The booking will be marked cancelled and freed from its hall. This frees the slot for other bookings.', confirmLabel: 'Cancel booking', danger: true }))) return;
    act(() => apiClient(`/api/v1/catering/events/${d.id}/status`, { method: 'POST', body: JSON.stringify({ status: s }) }));
  };
  const setDispatch = (s: string) => act(() => apiClient(`/api/v1/catering/events/${d.id}/dispatch`, { method: 'POST', body: JSON.stringify({ status: s, vehicle: fleet.vehicle.trim() || null, driver: fleet.driver.trim() || null }) }));
  const addItem = () => {
    if (!it.description.trim()) { flash('Name the extra'); return; }
    if (!(Number(it.unitPrice) > 0)) { flash('Enter a price (greater than 0) for the extra'); return; }
    act(() => apiClient(`/api/v1/catering/events/${d.id}/items`, { method: 'POST', body: JSON.stringify({ description: it.description, quantity: Number(it.quantity) || 1, unitPrice: Number(it.unitPrice) || 0 }) })).then(() => setIt({ description: '', quantity: '1', unitPrice: '' }));
  };
  const addPay = () => { if (!Number(pay.amount)) { flash('Amount?'); return; } act(() => apiClient(`/api/v1/catering/events/${d.id}/payments`, { method: 'POST', body: JSON.stringify({ amount: Number(pay.amount), payType: pay.payType, kind: pay.kind }) })).then(() => setPay({ amount: '', kind: 'deposit', payType: 'cash' })); };

  return (
    <Modal title={d.title || d.eventNo} icon={<CalendarPlus className="size-4" />} onClose={onClose} wide>
      <div className="mb-3 flex flex-wrap items-center gap-2 text-sm text-muted-foreground">
        <span className={`pill ${STATUS_PILL[d.status] ?? 'pill-idle'}`}>{d.status}</span>
        <span>{d.eventNo} · {fmt(d.startsAt)}{d.hall ? ` · ${d.hall}` : ''}{d.package ? ` · ${d.package}` : ''} · {d.pax} pax</span>
        {d.customerName && <span>· {d.customerName}{d.customerPhone ? ` (${d.customerPhone})` : ''}</span>}
        <button onClick={() => { let t = 'RIT HMS'; try { const o = JSON.parse(localStorage.getItem('hms.tenant') || '{}'); t = o.displayName || o.name || t; } catch { /* */ } printDoc(quoteHtml(d, t)); }}
          title="Print / download a customer quotation" className="ml-auto flex items-center gap-1 rounded-lg border border-border px-2.5 py-1 text-xs font-semibold text-foreground hover:bg-muted"><Printer className="size-3.5" /> Quote</button>
        <button onClick={() => setEdit(true)} disabled={d.produced} title={d.produced ? 'Already produced — details are locked' : 'Edit booking details'}
          className="flex items-center gap-1 rounded-lg border border-border px-2.5 py-1 text-xs font-semibold text-foreground hover:bg-muted disabled:opacity-40"><Pencil className="size-3.5" /> Edit</button>
      </div>

      {/* lifecycle */}
      <div className="mb-4 flex flex-wrap gap-2">
        {CATERING_STATUSES.map(s => (
          <button key={s} disabled={busy || d.status === s} onClick={() => setStatus(s)}
            className={`rounded-lg border px-3 py-1 text-xs font-semibold capitalize disabled:opacity-40 ${d.status === s ? 'border-primary bg-primary-tint text-primary-dark' : 'border-border hover:bg-muted'}`}>{s}</button>
        ))}
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        {/* extras + bill */}
        <div>
          <h4 className="mb-1 text-xs font-bold uppercase text-muted-foreground">Extras</h4>
          <div className="divide-y divide-border rounded-lg border border-border text-sm">
            {d.items.length === 0 && <div className="px-3 py-2 text-muted-foreground">No extras.</div>}
            {d.items.map(i => (
              <div key={i.id} className="flex items-center justify-between px-3 py-1.5">
                <span>{i.description} <span className="text-xs text-muted-foreground">×{i.quantity}</span></span>
                <span className="flex items-center gap-2 tabular-nums">{money(i.lineTotal)}
                  <button disabled={busy} onClick={async () => { if (!(await confirmDialog({ title: `Remove “${i.description}”?`, body: 'This extra will be removed from the booking.', confirmLabel: 'Remove', danger: true }))) return; act(() => apiClient(`/api/v1/catering/events/${d.id}/items/${i.id}`, { method: 'DELETE' })); }} className="text-error"><X className="size-3.5" /></button></span>
              </div>
            ))}
          </div>
          <div className="mt-2 grid grid-cols-[1fr_3.5rem_6rem_2.5rem] gap-1">
            <input value={it.description} onChange={e => setIt(s => ({ ...s, description: e.target.value }))} placeholder="Extra (e.g. décor)" className={cls} />
            <input value={it.quantity} onChange={e => setIt(s => ({ ...s, quantity: e.target.value.replace(/[^0-9.]/g, '') }))} inputMode="decimal" className={`${cls} text-center`} />
            <input value={it.unitPrice} onChange={e => setIt(s => ({ ...s, unitPrice: e.target.value.replace(/[^0-9.]/g, '') }))} inputMode="decimal" placeholder="LKR" className={`${cls} text-right`} />
            <button disabled={busy} onClick={addItem} className="rounded-lg bg-primary text-sm font-bold text-primary-foreground">+</button>
          </div>
          <div className="mt-3 space-y-1 text-sm">
            {d.packageId
              ? <Row l={`Package (${d.pax} × ${money(d.pricePerHead)})`} v={d.packageTotal} />
              : <div className="flex justify-between text-muted-foreground"><span>Package</span><span>— none attached —</span></div>}
            {d.extrasTotal > 0 && <Row l="Extras" v={d.extrasTotal} />}
            {d.discountAmount > 0 && <Row l="Discount" v={-d.discountAmount} />}
            <div className="flex justify-between border-t border-border pt-1 text-base font-black"><span>Total</span><span className="tabular-nums">{money(d.totalAmount)}</span></div>
            <Row l="Paid" v={d.paidAmount} />
            <div className="flex justify-between font-bold"><span>Balance</span><span className={`tabular-nums ${d.balance > 0 ? 'text-status-pending' : 'text-status-paid'}`}>{money(d.balance)}</span></div>
          </div>

          {/* Production / inventory tie-in */}
          <div className="mt-3 rounded-lg border border-border p-2 text-sm">
            {d.produced ? (
              <>
                <div className="flex justify-between"><span className="text-muted-foreground">Food cost (consumed)</span><span className="tabular-nums">{money(d.foodCost)}</span></div>
                <div className="flex justify-between font-semibold"><span>Gross margin</span><span className={`tabular-nums ${d.margin >= 0 ? 'text-status-paid' : 'text-status-error'}`}>{money(d.margin)}</span></div>
                <p className="mt-1 text-xs text-status-paid">✓ Produced — ingredients consumed from stock.</p>
              </>
            ) : (
              <>
                <button disabled={busy || !d.packageId} title={d.packageId ? 'Consume the package recipe from stock' : 'Attach a package first'}
                  onClick={() => act(() => apiClient(`/api/v1/catering/events/${d.id}/produce`, { method: 'POST' }))}
                  className="w-full rounded-lg bg-primary py-2 text-sm font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">
                  Produce — consume stock
                </button>
                {!d.packageId && <p className="mt-1 text-xs text-muted-foreground">Attach a package (via <span className="font-semibold">Edit</span>) with a linked recipe to produce from stock.</p>}
              </>
            )}
          </div>
        </div>

        {/* payments + dispatch */}
        <div>
          <h4 className="mb-1 text-xs font-bold uppercase text-muted-foreground">Payments</h4>
          <div className="divide-y divide-border rounded-lg border border-border text-sm">
            {d.payments.length === 0 && <div className="px-3 py-2 text-muted-foreground">No payments.</div>}
            {d.payments.map(p => (
              <div key={p.id} className="flex items-center justify-between px-3 py-1.5">
                <span className="capitalize">{p.kind} <span className="text-xs text-muted-foreground">· {p.payType}</span></span>
                <span className="tabular-nums">{money(p.amount)}</span>
              </div>
            ))}
          </div>
          <div className="mt-2 grid grid-cols-[6rem_5rem_1fr_2.5rem] gap-1">
            <select value={pay.kind} onChange={e => setPay(s => ({ ...s, kind: e.target.value }))} className={cls}>{['deposit', 'advance', 'balance'].map(k => <option key={k} value={k}>{k}</option>)}</select>
            <select value={pay.payType} onChange={e => setPay(s => ({ ...s, payType: e.target.value }))} className={cls}>{['cash', 'card', 'bank'].map(k => <option key={k} value={k}>{k}</option>)}</select>
            <input value={pay.amount} onChange={e => setPay(s => ({ ...s, amount: e.target.value.replace(/[^0-9.]/g, '') }))} inputMode="decimal" placeholder="LKR amount" className={`${cls} text-right`} />
            <button disabled={busy} onClick={addPay} className="rounded-lg bg-primary text-sm font-bold text-primary-foreground">+</button>
          </div>

          {d.isOffsite && (
            <div className="mt-4">
              <h4 className="mb-1 text-xs font-bold uppercase text-muted-foreground">Own-fleet delivery</h4>
              <p className="text-sm text-muted-foreground">{[d.deliveryAddress, d.district, d.countryCode].filter(Boolean).join(', ')}</p>
              {/* Vehicle + driver are assigned here, at the delivery stage — not at enquiry. */}
              <div className="mt-2 grid grid-cols-2 gap-2">
                <input value={fleet.vehicle} onChange={e => setFleet(s => ({ ...s, vehicle: e.target.value }))} placeholder="Vehicle (e.g. van CAB-1234)" className={cls} />
                <input value={fleet.driver} onChange={e => setFleet(s => ({ ...s, driver: e.target.value }))} placeholder="Driver name" className={cls} />
              </div>
              <div className="mt-2 flex items-center gap-2">
                {['pending', 'dispatched', 'delivered'].map(s => (
                  <button key={s} disabled={busy || d.dispatchStatus === s} onClick={() => setDispatch(s)}
                    className={`rounded-lg border px-3 py-1 text-xs font-semibold capitalize disabled:opacity-40 ${d.dispatchStatus === s ? 'border-primary bg-primary-tint text-primary-dark' : 'border-border hover:bg-muted'}`}>{s}</button>
                ))}
                <span className="text-[11px] text-muted-foreground">Assign a driver before dispatching.</span>
              </div>
            </div>
          )}
        </div>
      </div>

      {edit && <BookingForm halls={halls} pkgs={pkgs} initial={d} onClose={() => setEdit(false)} onSaved={() => { setEdit(false); void refresh(); }} flash={flash} err={err} />}
    </Modal>
  );
}

function Halls({ flash, err }: { flash: (m: string) => void; err: (e: unknown) => string }) {
  const [rows, setRows] = useState<Hall[]>([]);
  const [editId, setEditId] = useState<string | null>(null);
  const [f, setF] = useState({ code: '', name: '', capacity: '' });
  const load = useCallback(() => { apiClient<Hall[]>('/api/v1/catering/halls?all=true').then(setRows).catch(() => {}); }, []);
  useEffect(() => { load(); }, [load]);

  function reset() { setEditId(null); setF({ code: '', name: '', capacity: '' }); }
  function startEdit(h: Hall) { setEditId(h.id); setF({ code: h.code, name: h.name, capacity: String(h.capacity) }); }

  async function save() {
    if (!f.code || !f.name) { flash('Code + name required'); return; }
    const editing = rows.find(r => r.id === editId);
    try {
      await apiClient('/api/v1/catering/halls', { method: 'POST', body: JSON.stringify({ id: editId, code: f.code, name: f.name, capacity: Number(f.capacity) || 0, notes: editing?.notes ?? null, isActive: editing?.isActive ?? true }) });
      reset(); flash('Hall saved.'); load();
    } catch (e) { flash(err(e)); }
  }
  async function toggleActive(h: Hall) {
    if (h.isActive && !(await confirmDialog({ title: `Deactivate ${h.name}?`, body: 'It will be hidden when creating new bookings. Existing bookings keep their record and you can reactivate it any time.', confirmLabel: 'Deactivate', danger: true }))) return;
    try { await apiClient('/api/v1/catering/halls', { method: 'POST', body: JSON.stringify({ id: h.id, code: h.code, name: h.name, capacity: h.capacity, notes: h.notes, isActive: !h.isActive }) }); load(); } catch (e) { flash(err(e)); }
  }
  async function del(id: string) { if (!(await confirmDialog({ title: 'Remove this hall?', body: 'This banquet hall will be deleted. Existing bookings keep their record.', confirmLabel: 'Remove', danger: true }))) return; try { await apiClient(`/api/v1/catering/halls/${id}`, { method: 'DELETE' }); if (editId === id) reset(); load(); } catch (e) { flash(err(e)); } }
  return (
    <div className="card max-w-2xl divide-y divide-border">
      {rows.map(h => (
        <div key={h.id} className={`flex items-center justify-between px-4 py-2.5 text-sm ${h.isActive ? '' : 'opacity-60'}`}>
          <span><span className="font-semibold">{h.code}</span> · {h.name} <span className="text-muted-foreground">· {h.capacity}p</span>{!h.isActive && <span className="ml-2 rounded bg-muted px-1.5 text-[10px] uppercase">off</span>}</span>
          <span className="flex shrink-0 items-center gap-2">
            <button onClick={() => startEdit(h)} className="text-xs font-medium text-primary hover:underline">Edit</button>
            <button onClick={() => toggleActive(h)} className="text-xs font-medium text-muted-foreground hover:underline">{h.isActive ? 'Deactivate' : 'Activate'}</button>
            <button onClick={() => del(h.id)} className="text-xs font-medium text-status-error hover:underline">Remove</button>
          </span>
        </div>
      ))}
      <div className="p-3">
        <div className="mb-1 text-xs font-semibold">{editId ? 'Edit Hall' : 'New Hall'}</div>
        <div className="grid grid-cols-[7rem_1fr_5rem_auto] items-end gap-2">
          <Field label="Code" mono value={f.code} onChange={v => setF(s => ({ ...s, code: v.toUpperCase() }))} placeholder="CODE" />
          <Field label="Hall name" value={f.name} onChange={v => setF(s => ({ ...s, name: v }))} placeholder="Hall name" />
          <Field label="Capacity" value={f.capacity} onChange={v => setF(s => ({ ...s, capacity: v.replace(/[^0-9]/g, '') }))} inputMode="numeric" placeholder="Cap" />
          <div className="flex gap-2">
            {editId && <button onClick={reset} className="h-[58px] rounded-lg border border-border px-3 text-sm font-semibold hover:bg-muted">Cancel</button>}
            <button onClick={save} className="h-[58px] rounded-lg bg-primary px-4 text-sm font-bold text-primary-foreground">{editId ? 'Save' : 'Add'}</button>
          </div>
        </div>
      </div>
    </div>
  );
}

function Packages({ flash, err }: { flash: (m: string) => void; err: (e: unknown) => string }) {
  const [rows, setRows] = useState<Pkg[]>([]);
  const [recipes, setRecipes] = useState<RecipeOpt[]>([]);
  const [editId, setEditId] = useState<string | null>(null);
  const [f, setF] = useState({ code: '', name: '', pricePerHead: '' });
  const load = useCallback(() => { apiClient<Pkg[]>('/api/v1/catering/packages?all=true').then(setRows).catch(() => {}); }, []);
  useEffect(() => { load(); apiClient<RecipeOpt[]>('/api/v1/recipes').then(setRecipes).catch(() => {}); }, [load]);

  function reset() { setEditId(null); setF({ code: '', name: '', pricePerHead: '' }); }
  function startEdit(p: Pkg) { setEditId(p.id); setF({ code: p.code, name: p.name, pricePerHead: String(p.pricePerHead) }); }

  async function save() {
    if (!f.code || !f.name) { flash('Code + name required'); return; }
    const editing = rows.find(r => r.id === editId);
    try {
      await apiClient('/api/v1/catering/packages', { method: 'POST', body: JSON.stringify({ id: editId, code: f.code, name: f.name, pricePerHead: Number(f.pricePerHead) || 0, description: editing?.description ?? null, isActive: editing?.isActive ?? true, recipeProductId: editing?.recipeProductId ?? null }) });
      reset(); flash('Package saved.'); load();
    } catch (e) { flash(err(e)); }
  }
  async function toggleActive(p: Pkg) {
    if (p.isActive && !(await confirmDialog({ title: `Deactivate ${p.name}?`, body: 'It will be hidden when creating new bookings. You can reactivate it any time.', confirmLabel: 'Deactivate', danger: true }))) return;
    try { await apiClient('/api/v1/catering/packages', { method: 'POST', body: JSON.stringify({ id: p.id, code: p.code, name: p.name, pricePerHead: p.pricePerHead, description: p.description, isActive: !p.isActive, recipeProductId: p.recipeProductId }) }); load(); } catch (e) { flash(err(e)); }
  }
  async function del(id: string) { if (!(await confirmDialog({ title: 'Remove this package?', body: 'This catering package will be deleted. This cannot be undone.', confirmLabel: 'Remove', danger: true }))) return; try { await apiClient(`/api/v1/catering/packages/${id}`, { method: 'DELETE' }); if (editId === id) reset(); load(); } catch (e) { flash(err(e)); } }
  async function setRecipe(p: Pkg, recipeProductId: string) { try { await apiClient('/api/v1/catering/packages', { method: 'POST', body: JSON.stringify({ id: p.id, code: p.code, name: p.name, pricePerHead: p.pricePerHead, description: p.description, isActive: p.isActive, recipeProductId: recipeProductId || null }) }); flash('Recipe linked.'); load(); } catch (e) { flash(err(e)); } }
  return (
    <div className="card max-w-3xl divide-y divide-border">
      {rows.map(p => (
        <div key={p.id} className={`flex items-center justify-between gap-3 px-4 py-2.5 text-sm ${p.isActive ? '' : 'opacity-60'}`}>
          <span className="min-w-0 flex-1 truncate"><span className="font-semibold">{p.code}</span> · {p.name} <span className="text-muted-foreground">· {money(p.pricePerHead)}/head</span>{!p.isActive && <span className="ml-2 rounded bg-muted px-1.5 text-[10px] uppercase">off</span>}</span>
          <Combobox className="w-44" value={p.recipeProductId ?? ''} onChange={v => setRecipe(p, v)}
            placeholder="— no recipe —" searchPlaceholder="Search recipes…"
            options={[{ value: '', label: '— no recipe —' }, ...recipes.map(r => ({ value: r.productId, label: r.productName }))]} />
          <span className="flex shrink-0 items-center gap-2">
            <button onClick={() => startEdit(p)} className="text-xs font-medium text-primary hover:underline">Edit</button>
            <button onClick={() => toggleActive(p)} className="text-xs font-medium text-muted-foreground hover:underline">{p.isActive ? 'Deactivate' : 'Activate'}</button>
            <button onClick={() => del(p.id)} className="text-xs font-medium text-status-error hover:underline">Remove</button>
          </span>
        </div>
      ))}
      <div className="p-3">
        <div className="mb-1 text-xs font-semibold">{editId ? 'Edit Package' : 'New Package'}</div>
        <div className="grid grid-cols-[7rem_1fr_7rem_auto] items-end gap-2">
          <Field label="Code" mono value={f.code} onChange={v => setF(s => ({ ...s, code: v.toUpperCase() }))} placeholder="CODE" />
          <Field label="Package name" value={f.name} onChange={v => setF(s => ({ ...s, name: v }))} placeholder="Package name" />
          <Field label="LKR/head" value={f.pricePerHead} onChange={v => setF(s => ({ ...s, pricePerHead: v.replace(/[^0-9.]/g, '') }))} inputMode="decimal" placeholder="LKR/head" />
          <div className="flex gap-2">
            {editId && <button onClick={reset} className="h-[58px] rounded-lg border border-border px-3 text-sm font-semibold hover:bg-muted">Cancel</button>}
            <button onClick={save} className="h-[58px] rounded-lg bg-primary px-4 text-sm font-bold text-primary-foreground">{editId ? 'Save' : 'Add'}</button>
          </div>
        </div>
      </div>
      {recipes.length === 0 && <p className="px-4 py-2 text-xs text-muted-foreground">No recipes yet — create them under Production to enable stock consumption.</p>}
    </div>
  );
}

function Row({ l, v }: { l: string; v: number }) {
  return <div className="flex justify-between text-muted-foreground"><span>{l}</span><span className="tabular-nums">{money(v)}</span></div>;
}

function Modal({ title, onClose, children, wide, icon }: { title: string; onClose: () => void; children: React.ReactNode; wide?: boolean; icon?: React.ReactNode }) {
  return (
    <div className="fixed inset-0 z-[70] flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm" onClick={onClose}>
      <div className={`flex max-h-[90vh] w-full ${wide ? 'max-w-3xl' : 'max-w-lg'} flex-col overflow-hidden rounded-xl bg-card shadow-2xl`} onClick={e => e.stopPropagation()}>
        <div className="flex shrink-0 items-center justify-between bg-primary px-5 py-3 text-primary-foreground">
          <h3 className="flex min-w-0 items-center gap-2 truncate font-heading text-base font-bold">
            {icon && <span className="grid size-7 shrink-0 place-items-center rounded-lg bg-white/15">{icon}</span>}
            <span className="truncate">{title}</span>
          </h3>
          <button onClick={onClose} className="ml-3 shrink-0 rounded-lg p-1 text-primary-foreground/80 transition-colors hover:bg-white/15 hover:text-white" aria-label="Close"><X className="size-5" /></button>
        </div>
        <div className="overflow-y-auto overscroll-contain p-6">{children}</div>
      </div>
    </div>
  );
}
