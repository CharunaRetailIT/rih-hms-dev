'use client';

import { useEffect, useState, useCallback, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { apiClient, lkr } from '@/lib/api-client';
import { Icon } from '@/components/ui/Icon';
import { Modal } from '@/components/ui/Modal';
import { confirmDialog } from '@/components/ui/confirm';
import { DateTimePicker } from '@/components/ui/DateTimePicker';

type Product = { id: string; sku: string; name: string; barcode: string | null; categoryId: string | null; basePrice: number; colorHex: string | null; variantCount?: number };
type Category = { id: string; name: string; parentId: string | null; colorHex: string | null };
type Variant = { id: string; code: string; name: string; price: number };
type OrderItem = { id: string; productName: string; variantName?: string | null; quantity: number; unitPrice: number; lineTotal: number; discountAmount?: number; station: string; notes: string | null; kotStatus: string; modifiers?: { name: string; priceDelta: number }[] };
type ModItem = { id: string; name: string; priceDelta: number; isDefault: boolean };
type ModGroup = { id: string; name: string; minSelect: number; maxSelect: number; isRequired: boolean; items: ModItem[] };
type Order = {
  id: string; orderNumber: string; status: string; tableLabel: string | null; tableId?: string | null; covers: number | null;
  subtotalAmount: number; discountAmount: number; promotionDiscountAmount?: number; serviceChargeAmount: number; taxAmount: number; tipAmount?: number; totalAmount: number;
  invoiceNumber: string | null; customerId?: string | null; customerName?: string | null; items: OrderItem[];
  stewardId?: string | null; tourOperatorId?: string | null; tourCommissionAmount?: number;
};
type Steward = { id: string; name: string };   // a user flagged is_server (#76)
type TourOp = { id: string; code: string; name: string; commissionPercent: number };
type Ccy = { id: string; code: string; name: string; symbol: string | null; rateToBase: number; isBase: boolean };
type Cust = {
  id: string; code: string; name: string; phone: string | null; isCreditCustomer: boolean;
  discountPercent: number | null; creditLimit: number; currentBalance: number; creditAvailable: number;
  advanceBalance: number; loyaltyPoints: number; loyaltyLifetimePoints: number;
};
type PosCfg = { enabled: boolean; earnRate: number; redeemValue: number; kdsEnabled: boolean; kotAutoPrint: boolean; baseCurrency?: string; taxLabel?: string; ereceipt?: { channels: string; quota: number; used: number } };

type Invoice = {
  invoiceNumber: string | null; isTaxInvoice: boolean; orderNumber: string; settledAt: string | null;
  supplier: { legalName: string | null; vatNo: string | null; brNo: string | null; svatNo: string | null;
    footer: string | null; billHeader: string | null; billFooter: string | null; showVatOnBills: boolean };
  customer: { name: string | null; vatNo: string | null };
  tableLabel?: string | null; covers?: number | null; reprintCount?: number;
  discountAmount?: number; promotionDiscountAmount?: number;
  lines: { productName: string; quantity: number; unitPrice: number; lineSubtotal: number }[];
  subtotalAmount: number; serviceChargeAmount: number; taxAmount: number; totalAmount: number;
  charges: { name: string; ratePercent: number; baseAmount: number; chargeAmount: number }[];
};

type PayType = 'cash' | 'card' | 'ubereats_prepaid' | 'pickme_prepaid' | 'credit' | 'loyalty' | 'advance';
const PAY_TABS: { key: PayType; label: string; dot?: string }[] = [
  { key: 'cash', label: 'Cash' }, { key: 'card', label: 'Card' },
  { key: 'ubereats_prepaid', label: 'UberEats', dot: 'bg-[#1e2329]' },
  { key: 'pickme_prepaid', label: 'PickMe', dot: 'bg-accent' },
];

type ShiftView = {
  id: string; shiftNumber: string; status: string; openingFloat: number;
  totalSales: number; cashSales: number; cardSales: number; otherSales: number;
  orderCount: number; expectedCash: number; declaredCash: number | null; cashVariance: number | null;
  openedByName: string | null;
};
type Session = { id: string; email: string; displayName: string; role: number };
const ROLE_LABEL: Record<number, string> = { 0: 'Owner', 1: 'Manager', 2: 'Cashier', 3: 'Kitchen', 4: 'Accountant' };

// Major world currencies for the POS "view in" live-rate selector (display only).
const VIEW_CURRENCIES = [
  'USD', 'EUR', 'GBP', 'AED', 'SAR', 'QAR', 'KWD', 'INR', 'LKR', 'MVR', 'PKR', 'BDT',
  'AUD', 'CAD', 'CHF', 'JPY', 'CNY', 'SGD', 'MYR', 'THB', 'NZD', 'ZAR', 'HKD', 'NPR',
];

// ── operating hours (soft warning only) ──────────────────────────────────────
// operating_hours is a JSON string keyed by weekday; each day { open, close } in
// "HH:mm" (a missing/null day = closed). We compare to the venue's local time
// (the browser, which runs at the till) and never block — just surface a notice.
const HRS_DAYS = ['sun', 'mon', 'tue', 'wed', 'thu', 'fri', 'sat'] as const;
function hoursWarning(json: string | null | undefined): string | null {
  if (!json) return null;                         // not configured → no warning
  let h: Record<string, { open?: string; close?: string } | null>;
  try { h = JSON.parse(json); } catch { return null; }
  if (!h || typeof h !== 'object') return null;
  const now = new Date();
  const today = h[HRS_DAYS[now.getDay()]];
  if (!today || !today.open || !today.close) return 'Closed today';
  const cur = now.getHours() * 60 + now.getMinutes();
  const [oh, om] = today.open.split(':').map(Number);
  const [ch, cm] = today.close.split(':').map(Number);
  const o = oh * 60 + om, c = ch * 60 + cm;
  const within = c > o ? (cur >= o && cur < c) : (cur >= o || cur < c);   // handle overnight close
  return within ? null : `Outside hours · ${today.open}–${today.close}`;
}

// ── receipt printing ────────────────────────────────────────────────────────
const esc = (s: string | null | undefined) =>
  (s ?? '').replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]!));

/** Build an 80mm thermal-receipt HTML doc from the invoice + live order. reprintCopy>0 marks a duplicate. */
function receiptHtml(inv: Invoice, o: Order, reprintCopy = 0, ccy = 'LKR', rate = 1, taxLabel = 'VAT'): string {
  const sup = inv.supplier;
  const settled = !!inv.invoiceNumber;
  const title = settled ? (inv.isTaxInvoice ? 'TAX INVOICE' : 'RECEIPT') : 'BILL';
  const reprintBanner = reprintCopy > 0 ? `<div class="ctr" style="font-weight:bold;border:1px solid #000;margin:4px 0;padding:2px">*** REPRINT · COPY ${reprintCopy} ***</div>` : '';
  const when = inv.settledAt ? new Date(inv.settledAt).toLocaleString('en-LK') : new Date().toLocaleString('en-LK');
  const discount = (o.discountAmount || 0) + (o.promotionDiscountAmount || 0);
  const m = (n: number) => lkr(n / (rate || 1));   // base → outlet display currency

  const lines = (inv.lines.length ? inv.lines : o.items.map(i => ({
    productName: i.variantName ? `${i.productName} (${i.variantName})` : i.productName,
    quantity: i.quantity, unitPrice: i.unitPrice, lineSubtotal: i.lineTotal,
  }))).map(l => `<tr><td>${esc(l.productName)}</td><td class="c">${l.quantity}</td>
      <td class="r">${m(l.unitPrice)}</td><td class="r">${m(l.lineSubtotal)}</td></tr>`).join('');

  const tot = (label: string, val: string, strong = false) =>
    `<tr class="${strong ? 'tot' : ''}"><td colspan="3" class="r">${esc(label)}</td><td class="r">${val}</td></tr>`;
  const charges = inv.charges.map(c => tot(`${esc(c.name)} (${c.ratePercent}%)`, m(c.chargeAmount))).join('');

  return `<!doctype html><html><head><meta charset="utf-8"><title>${esc(title)} ${esc(inv.invoiceNumber ?? inv.orderNumber)}</title>
<style>
  @page { size: 80mm auto; margin: 4mm; }
  * { font-family: 'Courier New', monospace; }
  body { width: 72mm; margin: 0 auto; color: #000; font-size: 12px; }
  h1 { font-size: 14px; text-align: center; margin: 0 0 2px; }
  .ctr { text-align: center; }
  .meta { font-size: 11px; margin: 6px 0; }
  hr { border: none; border-top: 1px dashed #000; margin: 6px 0; }
  table { width: 100%; border-collapse: collapse; }
  th, td { padding: 1px 0; vertical-align: top; }
  th { text-align: left; border-bottom: 1px solid #000; font-size: 11px; }
  .r { text-align: right; } .c { text-align: center; }
  tr.tot td { border-top: 1px solid #000; font-weight: bold; font-size: 13px; padding-top: 3px; }
  .ftr { text-align: center; font-size: 11px; margin-top: 8px; white-space: pre-line; }
</style></head><body onload="window.focus();window.print();">
  <h1>${esc(sup.legalName || 'RIT HMS')}</h1>
  ${sup.billHeader ? `<div class="ctr meta">${esc(sup.billHeader)}</div>` : ''}
  ${sup.showVatOnBills && sup.vatNo ? `<div class="ctr meta">${esc(taxLabel)} No: ${esc(sup.vatNo)}${sup.brNo ? ` · BR: ${esc(sup.brNo)}` : ''}</div>` : ''}
  <hr><div class="ctr"><strong>${esc(title)}</strong></div>
  ${reprintBanner}
  <div class="meta">
    No: ${esc(inv.invoiceNumber ?? inv.orderNumber)}<br>
    ${o.tableLabel ? `Table: ${esc(o.tableLabel)}${o.covers ? ` · Covers: ${o.covers}` : ''}<br>` : ''}
    Date: ${esc(when)}${settled ? '' : ' (not settled)'}
    ${inv.customer.name ? `<br>Customer: ${esc(inv.customer.name)}${inv.customer.vatNo ? ` · ${esc(taxLabel)} ${esc(inv.customer.vatNo)}` : ''}` : ''}
  </div>
  <table>
    <thead><tr><th>Item</th><th class="c">Qty</th><th class="r">Price</th><th class="r">Amt</th></tr></thead>
    <tbody>${lines}</tbody>
    <tbody>
      ${tot('Subtotal', m(inv.subtotalAmount))}
      ${discount > 0.009 ? tot('Discount', `-${m(discount)}`) : ''}
      ${charges}
      ${tot(`TOTAL (${ccy})`, m(inv.totalAmount), true)}
    </tbody>
  </table>
  <div class="ftr">${esc(sup.billFooter || sup.footer || 'Thank you!')}</div>
</body></html>`;
}

/**
 * Kitchen ticket (KOT) — the chit the kitchen/bar works off. No prices: just
 * what to cook, grouped by station, with the table/order and time. Used for
 * venues running without a KDS screen (print to a KOT printer, or print at the
 * POS and hand it over). `only` limits the ticket to a set of item ids (e.g. the
 * lines just sent); omit it to print the whole order.
 */
function kitchenTicketHtml(o: Order, only?: Set<string>): string {
  const items = o.items.filter(i => !only || only.has(i.id));
  const byStation = new Map<string, OrderItem[]>();
  for (const i of items) {
    const k = (i.station || 'kitchen').toUpperCase();
    (byStation.get(k) ?? byStation.set(k, []).get(k)!).push(i);
  }
  const when = new Date().toLocaleString('en-LK');
  const where = o.tableLabel ? `TABLE ${esc(o.tableLabel)}` : esc(o.orderNumber);
  const sections = [...byStation.entries()].map(([station, lines]) => `
    <div class="stn">${esc(station)}</div>
    <table>${lines.map(l => `
      <tr><td class="q">${l.quantity}×</td><td>${esc(l.variantName ? `${l.productName} (${l.variantName})` : l.productName)}${
        (l.modifiers && l.modifiers.length) ? `<div class="mod">${l.modifiers.map(m => esc(m.name)).join(', ')}</div>` : ''
      }${l.notes ? `<div class="mod">» ${esc(l.notes)}</div>` : ''}</td></tr>`).join('')}
    </table>`).join('<hr>');

  return `<!doctype html><html><head><meta charset="utf-8"><title>KOT ${esc(o.orderNumber)}</title>
<style>
  @page { size: 80mm auto; margin: 4mm; }
  * { font-family: 'Courier New', monospace; }
  body { width: 72mm; margin: 0 auto; color: #000; font-size: 13px; }
  h1 { font-size: 18px; text-align: center; margin: 0; }
  .meta { font-size: 12px; text-align: center; margin: 4px 0 6px; }
  hr { border: none; border-top: 1px dashed #000; margin: 6px 0; }
  .stn { font-weight: bold; font-size: 13px; text-transform: uppercase; margin: 4px 0 2px; border-bottom: 1px solid #000; }
  table { width: 100%; border-collapse: collapse; }
  td { padding: 2px 0; vertical-align: top; font-size: 14px; }
  td.q { width: 36px; font-weight: bold; }
  .mod { font-size: 11px; padding-left: 2px; }
</style></head><body onload="window.focus();window.print();">
  <h1>${where}</h1>
  <div class="meta">KOT · ${esc(o.orderNumber)}${o.covers ? ` · ${o.covers} covers` : ''}<br>${esc(when)}</div>
  <hr>${sections || '<div class="meta">(no items)</div>'}
</body></html>`;
}

/** Render a receipt HTML doc into a hidden iframe and open the print dialog. */
function printReceipt(html: string) {
  if (typeof document === 'undefined') return;
  const frame = document.createElement('iframe');
  frame.style.cssText = 'position:fixed;right:0;bottom:0;width:0;height:0;border:0;';
  document.body.appendChild(frame);
  const doc = frame.contentWindow?.document;
  if (!doc) { frame.remove(); return; }
  doc.open(); doc.write(html); doc.close();
  // The doc's onload triggers print(); clean the iframe up shortly after.
  setTimeout(() => frame.remove(), 3000);
}

export default function PosPage() {
  const router = useRouter();
  const [products, setProducts] = useState<Product[]>([]);
  const [avail, setAvail] = useState<Record<string, { available: boolean; reason: string }>>({});   // #112 per-outlet 86
  const [categories, setCategories] = useState<Category[]>([]);
  const [activeCat, setActiveCat] = useState<string | null>(null);
  const [locationId, setLocationId] = useState<string | null>(null);
  const [outletCurrency, setOutletCurrency] = useState('LKR');   // outlet's default "view in" currency (billing stays base)
  const [outletName, setOutletName] = useState('');              // active branch name (header)
  const [outletHours, setOutletHours] = useState<string | null>(null);   // operating hours JSON (soft POS warning)
  const [tenantName, setTenantName] = useState('');              // business name (header)
  const [viewCcy, setViewCcy] = useState('');                    // live-rate viewer currency ('' = off)
  const [viewRate, setViewRate] = useState<number | null>(null); // units of viewCcy per 1 base, from the FX API
  const [order, setOrder] = useState<Order | null>(null);
  const [busy, setBusy] = useState(false);
  const [toast, setToast] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [clock, setClock] = useState('');

  // payment / keypad state
  const [payType, setPayType] = useState<PayType>('cash');
  const [keypad, setKeypad] = useState('');
  const [loyaltyInput, setLoyaltyInput] = useState('');   // #66 points to redeem (LKR), partial allowed
  const [tenders, setTenders] = useState<{ payType: PayType; amount: number; currencyCode?: string }[]>([]);
  const [openItem, setOpenItem] = useState<{ name: string; price: string; qty: string } | null>(null);
  const [modPick, setModPick] = useState<{ product: Product; station: string; groups: ModGroup[]; selected: Record<string, string[]>; variantId: string | null } | null>(null);
  const [sizePick, setSizePick] = useState<{ product: Product; station: string; variants: Variant[] } | null>(null);

  // shift / cash-up state
  const [session, setSession] = useState<Session | null>(null);
  const [shift, setShift] = useState<ShiftView | null>(null);
  const [shiftModal, setShiftModal] = useState<'start' | 'end' | null>(null);
  const [floatInput, setFloatInput] = useState('');
  const [declaredInput, setDeclaredInput] = useState('');
  const [shiftBusy, setShiftBusy] = useState(false);
  const [zReport, setZReport] = useState<ShiftView | null>(null);
  const [openOrders, setOpenOrders] = useState<Order[]>([]);
  const [tableModal, setTableModal] = useState<null | 'new' | 'move' | 'merge'>(null);
  const [discountModal, setDiscountModal] = useState(false);
  const [discountVal, setDiscountVal] = useState('');
  const [discountMode, setDiscountMode] = useState<'amount' | 'percent'>('amount');
  const [floorTables, setFloorTables] = useState<{ id: string; code: string; seats: number; area: string | null; occupied: boolean; orderId: string | null }[]>([]);
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [openBillConflict, setOpenBillConflict] = useState<string | null>(null);
  const [carryOpenBills, setCarryOpenBills] = useState(false);
  const [custModal, setCustModal] = useState(false);
  const [custSearch, setCustSearch] = useState('');
  const [custResults, setCustResults] = useState<Cust[]>([]);
  const [attachedCust, setAttachedCust] = useState<Cust | null>(null);
  const [newCust, setNewCust] = useState<{ name: string; phone: string; dob: string; address: string; taxNo: string; email: string } | null>(null);
  const [newCustErr, setNewCustErr] = useState<string | null>(null);
  const [newCustDup, setNewCustDup] = useState<{ id: string; name: string } | null>(null);
  const [loyaltyCfg, setLoyaltyCfg] = useState<PosCfg>({ enabled: false, earnRate: 0, redeemValue: 1, kdsEnabled: true, kotAutoPrint: false });
  // POS depth (#76): steward/covers/tour-operator + tip + multi-currency tender.
  const [stewards, setStewards] = useState<Steward[]>([]);
  const [tourOps, setTourOps] = useState<TourOp[]>([]);
  const [currencies, setCurrencies] = useState<Ccy[]>([]);
  const [tenderCcy, setTenderCcy] = useState('LKR');
  const [detailsModal, setDetailsModal] = useState(false);
  const [tipVal, setTipVal] = useState('');
  const [splitModal, setSplitModal] = useState(false);
  const [splitMoves, setSplitMoves] = useState<Record<string, number>>({});
  const [recallOpen, setRecallOpen] = useState(false);
  const [recallSearch, setRecallSearch] = useState('');
  // E-receipt (#79): email/SMS a settled bill to the guest.
  const [sendFor, setSendFor] = useState<{ id: string; label: string } | null>(null);
  const [sendChannel, setSendChannel] = useState<'email' | 'sms'>('email');
  const [sendTo, setSendTo] = useState('');
  const [sendBusy, setSendBusy] = useState(false);
  const [recallRows, setRecallRows] = useState<{ id: string; orderNumber: string; invoiceNumber: string | null; tableLabel: string | null; customerName: string | null; totalAmount: number; settledAt: string | null; reprintCount: number }[]>([]);

  useEffect(() => { apiClient<PosCfg>('/api/v1/pos/config').then(setLoyaltyCfg).catch(() => {}); }, []);
  // POS-depth masters (#76): waiters, tour operators, accepted tender currencies.
  useEffect(() => {
    apiClient<Steward[]>('/api/v1/servers').then(setStewards).catch(() => {});
    apiClient<TourOp[]>('/api/v1/tour-operators').then(setTourOps).catch(() => {});
    apiClient<Ccy[]>('/api/v1/currencies').then(c => {
      setCurrencies(c);
      const base = c.find(x => x.isBase)?.code; if (base) setTenderCcy(base);
    }).catch(() => {});
  }, []);

  // Default the live-rate "view in" currency to the outlet's configured display
  // currency. Billing always stays in base — this is a customer-facing reference only.
  useEffect(() => {
    const base = loyaltyCfg.baseCurrency || currencies.find(c => c.isBase)?.code || 'LKR';
    if (outletCurrency && outletCurrency !== base) setViewCcy(outletCurrency);
  }, [outletCurrency, currencies, loyaltyCfg.baseCurrency]);

  // Pull a real-time rate (base → chosen currency) from the FX API for the viewer.
  useEffect(() => {
    const base = loyaltyCfg.baseCurrency || currencies.find(c => c.isBase)?.code || 'LKR';
    if (!viewCcy || viewCcy === base) { setViewRate(null); return; }
    let cancelled = false;
    apiClient<{ rate: number }>(`/api/v1/fx/rate?from=${encodeURIComponent(base)}&to=${encodeURIComponent(viewCcy)}`)
      .then(r => { if (!cancelled) setViewRate(r.rate); })
      .catch(() => { if (!cancelled) setViewRate(null); });
    return () => { cancelled = true; };
  }, [viewCcy, currencies, loyaltyCfg.baseCurrency]);

  // Fullscreen toggle (POS only). Track real state via the fullscreenchange event
  // so the icon stays correct even when the user presses Esc.
  useEffect(() => {
    const sync = () => setIsFullscreen(typeof document !== 'undefined' && !!document.fullscreenElement);
    document.addEventListener('fullscreenchange', sync);
    return () => document.removeEventListener('fullscreenchange', sync);
  }, []);
  const toggleFullscreen = () => {
    if (typeof document === 'undefined') return;
    if (document.fullscreenElement) void document.exitFullscreen?.();
    else void document.documentElement.requestFullscreen?.().catch(() => flash('Fullscreen blocked by the browser'));
  };

  // Keep the attached-customer detail (credit limit etc.) in sync with the order.
  useEffect(() => {
    const cid = order?.customerId;
    if (!cid) { setAttachedCust(null); return; }
    let cancelled = false;
    apiClient<Cust>(`/api/v1/customers/${cid}`).then(c => { if (!cancelled) setAttachedCust(c); }).catch(() => {});
    return () => { cancelled = true; };
  }, [order?.customerId]);

  useEffect(() => {
    if (typeof window !== 'undefined' && !localStorage.getItem('hms.token')) { router.replace('/login'); return; }
    (async () => {
      const [p, c, locs] = await Promise.all([
        apiClient<Product[]>('/api/v1/products?activeOnly=true'),
        apiClient<Category[]>('/api/v1/categories'),
        apiClient<{ id: string; code: string; name: string; currency?: string; operatingHours?: string | null }[]>('/api/v1/locations').catch(() => []),
      ]);
      setProducts(p); setCategories(c);
      // Business name for the header (from the session tenant).
      try { const t = JSON.parse(localStorage.getItem('hms.tenant') || '{}'); setTenantName(t?.name || t?.slug || ''); } catch { /* ignore */ }
      // Use the admin-selected branch (hms.location) if it's still valid, else MAIN.
      let active: { id: string } | null = null;
      try { const s = localStorage.getItem('hms.location'); if (s) active = JSON.parse(s); } catch { /* ignore */ }
      const main = (active && locs.find(l => l.id === active!.id)) ?? locs.find(l => l.code === 'MAIN') ?? locs[0];
      if (main) { setLocationId(main.id); setOutletCurrency(main.currency || 'LKR'); setOutletName(main.name || main.code || ''); setOutletHours(main.operatingHours ?? null); }
    })();
  }, [router]);

  // Refresh the open-tab strip for this outlet.
  const refreshOpen = useCallback(async () => {
    if (!locationId) return;
    try { setOpenOrders(await apiClient<Order[]>(`/api/v1/orders/open?locationId=${locationId}`)); }
    catch { /* keep last */ }
  }, [locationId]);

  // #112 — live per-outlet availability (auto-86 from stock/recipe + manual overrides).
  const loadAvail = useCallback(async () => {
    if (!locationId) return;
    try {
      const rows = await apiClient<{ productId: string; available: boolean; reason: string }[]>(`/api/v1/availability?locationId=${locationId}`);
      setAvail(Object.fromEntries(rows.map(r => [r.productId, { available: r.available, reason: r.reason }])));
    } catch { /* keep last */ }
  }, [locationId]);
  useEffect(() => { void loadAvail(); }, [loadAvail]);

  // Subscribe to the app SSE stream; refetch availability when stock/86 changes (fetch keeps the JWT header).
  const loadAvailRef = useRef(loadAvail); loadAvailRef.current = loadAvail;
  useEffect(() => {
    let stop = false; let ctrl: AbortController | null = null; let backoff = 1000;
    let debounce: ReturnType<typeof setTimeout> | null = null;
    const bump = () => { if (debounce) clearTimeout(debounce); debounce = setTimeout(() => loadAvailRef.current(), 400); };
    async function connect() {
      while (!stop) {
        ctrl = new AbortController();
        try {
          const token = typeof window !== 'undefined' ? localStorage.getItem('hms.token') : null;
          const res = await fetch('/api/v1/events/stream', { headers: token ? { Authorization: `Bearer ${token}` } : {}, signal: ctrl.signal, cache: 'no-store' });
          if (!res.ok || !res.body) throw new Error(`stream ${res.status}`);
          backoff = 1000;
          const reader = res.body.getReader(); const dec = new TextDecoder(); let buf = '';
          for (;;) {
            const { value, done } = await reader.read(); if (done) break;
            buf += dec.decode(value, { stream: true }); let nl: number;
            while ((nl = buf.indexOf('\n')) >= 0) {
              const line = buf.slice(0, nl).trim(); buf = buf.slice(nl + 1);
              if (line.startsWith('data:')) { const topic = line.slice(5).trim(); if (topic === 'availability' || topic === 'orders') bump(); }
            }
          }
        } catch { /* reconnect */ }
        if (stop) break;
        await new Promise(r => setTimeout(r, backoff)); backoff = Math.min(backoff * 2, 15000);
      }
    }
    void connect();
    return () => { stop = true; ctrl?.abort(); if (debounce) clearTimeout(debounce); };
  }, []);

  // Load (resume) an existing order into the terminal + reflect it in the URL.
  const loadOrder = useCallback(async (id: string) => {
    setBusy(true);
    try {
      const o = await apiClient<Order>(`/api/v1/orders/${id}`);
      setOrder(o); setTenders([]); setKeypad('');
      if (typeof window !== 'undefined') window.history.replaceState(null, '', `/pos?order=${id}`);
    } catch { flash('Could not open that bill — it may be settled or voided.'); }
    finally { setBusy(false); }
  }, []);

  // Open a brand-new order bound to a floor table (deep-link from the Floor screen).
  const openTable = useCallback(async (tableId: string) => {
    if (!locationId) return;
    if (!shift) { flash('Start a shift before taking orders'); setFloatInput(''); setShiftModal('start'); return; }
    setBusy(true);
    try {
      const tables = await apiClient<{ id: string; code: string }[]>(`/api/v1/tables?locationId=${locationId}`).catch(() => []);
      const label = tables.find(t => t.id === tableId)?.code ?? null;
      const o = await apiClient<Order>('/api/v1/orders', {
        method: 'POST', body: JSON.stringify({ locationId, orderType: 'dine_in', tableLabel: label, covers: 2, tableId }),
      });
      setOrder(o); setTenders([]); setKeypad('');
      if (typeof window !== 'undefined') window.history.replaceState(null, '', `/pos?order=${o.id}`);
      void refreshOpen();
    } catch { flash('Could not open that table.'); }
    finally { setBusy(false); }
  }, [locationId, refreshOpen, shift]);

  // Once the outlet is known: fetch open tabs and deep-link to ?order=<id> or ?table=<id>.
  useEffect(() => {
    if (!locationId) return;
    void refreshOpen();
    const params = typeof window !== 'undefined' ? new URLSearchParams(window.location.search) : null;
    const id = params?.get('order');
    const table = params?.get('table');
    if (id) void loadOrder(id);
    else if (table) void openTable(table);
  }, [locationId, refreshOpen, loadOrder, openTable]);

  useEffect(() => {
    const tick = () => {
      const now = new Date();
      let h = now.getHours();
      const m = String(now.getMinutes()).padStart(2, '0');
      const ampm = h >= 12 ? 'PM' : 'AM';
      h = h % 12; h = h || 12;
      setClock(`${h}:${m} ${ampm}`);
    };
    tick();
    const id = setInterval(tick, 10000);
    return () => clearInterval(id);
  }, []);

  // who's logged in (for the cashier chip + shift attribution)
  useEffect(() => {
    const u = typeof window !== 'undefined' ? localStorage.getItem('hms.user') : null;
    if (u) setSession(JSON.parse(u));
  }, []);

  // load the open shift for this outlet
  useEffect(() => {
    if (!locationId) return;
    (async () => {
      try {
        const s = await apiClient<ShiftView | undefined>(`/api/v1/shifts/current?locationId=${locationId}`);
        setShift(s ?? null);
      } catch { /* leave as null */ }
    })();
  }, [locationId]);

  const flash = (m: string) => { setToast(m); setTimeout(() => setToast(null), 2500); };
  function signOut() {
    ['hms.token', 'hms.refresh', 'hms.tenant', 'hms.user', 'hms.location'].forEach(k => localStorage.removeItem(k));
    router.replace('/login');
  }

  async function startShift() {
    if (!locationId) { flash('No outlet configured'); return; }
    const f = Number(floatInput || '0');
    if (Number.isNaN(f) || f < 0) { flash('Enter a valid opening float'); return; }
    setShiftBusy(true);
    try {
      const s = await apiClient<ShiftView>('/api/v1/shifts/open', {
        method: 'POST', body: JSON.stringify({ locationId, openingFloat: f }),
      });
      setShift(s); setShiftModal(null); setFloatInput(''); flash(`Shift ${s.shiftNumber} started`);
    } catch (e) { flash((e as Error).message); } finally { setShiftBusy(false); }
  }

  // End shift. A shift must NEVER be closeable over a live bill, so we confront any
  // open bill UP FRONT — before the cash-up — and make the cashier decide (settle /
  // void / carry over). Only then do we open the drawer count.
  async function openEndShift() {
    if (!locationId) return;
    setCarryOpenBills(false);
    try {
      const open = await apiClient<Order[]>(`/api/v1/orders/open?locationId=${locationId}`);
      const live = open.filter(o => (o.items?.length ?? 0) > 0);
      if (live.length > 0) {
        setOpenBillConflict(
          `${live.length} open bill(s) still on this shift: `
          + live.map(o => `${o.tableLabel ? `Table ${o.tableLabel}` : o.orderNumber} · LKR ${lkr(o.totalAmount)}`).join(', '));
        return;   // stop here — no cash-up until the bill is dealt with
      }
    } catch { /* if the pre-check fails, fall through — the server still hard-blocks */ }
    await proceedToCashUp();
  }

  // Refresh live Z-report totals, then open the drawer-count modal.
  async function proceedToCashUp() {
    try {
      const s = await apiClient<ShiftView | undefined>(`/api/v1/shifts/current?locationId=${locationId}`);
      if (s) setShift(s);
    } catch { /* keep current */ }
    setDeclaredInput(''); setShiftModal('end');
  }

  async function closeShift() {
    if (!shift) return;
    const d = Number(declaredInput || '0');
    if (Number.isNaN(d) || d < 0) { flash('Enter the counted cash'); return; }
    setShiftBusy(true);
    try {
      const r = await apiClient<ShiftView>(`/api/v1/shifts/${shift.id}/close`, {
        method: 'POST', body: JSON.stringify({ declaredCash: d, notes: null, allowOpenBills: carryOpenBills }),
      });
      setZReport(r); setShift(null); setShiftModal(null); setDeclaredInput('');
      setOpenBillConflict(null); setCarryOpenBills(false); void refreshOpen();
    } catch (e) {
      const msg = (e as Error).message;
      // apiClient throws `API <status>: <body>`; pull the human message out of the JSON body.
      let nice = msg;
      const j = msg.match(/\{[\s\S]*\}$/);
      if (j) { try { nice = JSON.parse(j[0]).error ?? msg; } catch { /* keep raw */ } }
      // Safety net: a bill was opened (e.g. another terminal) during cash-up — re-confront it.
      if (/open bill/i.test(nice)) { setShiftModal(null); setOpenBillConflict(nice); }
      else flash(nice);
    } finally { setShiftBusy(false); }
  }

  // "Void the bill(s)" choice: void every open order at this outlet, then cash-up.
  async function voidAllOpenBills() {
    if (!locationId) return;
    setShiftBusy(true);
    try {
      const open = await apiClient<{ id: string }[]>(`/api/v1/orders/open?locationId=${locationId}`);
      for (const o of open)
        await apiClient(`/api/v1/orders/${o.id}/void`, { method: 'POST', body: JSON.stringify({ reason: 'Voided at shift close' }) }).catch(() => {});
      setOrder(null); void refreshOpen();
    } catch (e) { flash((e as Error).message); } finally { setShiftBusy(false); }
    setOpenBillConflict(null);
    await proceedToCashUp();
  }

  // "Keep open" choice: carry the bills to the next shift, then cash-up.
  async function carryOverAndCashUp() {
    setCarryOpenBills(true); setOpenBillConflict(null);
    await proceedToCashUp();
  }

  // A no-table bill (takeaway). Dine-in bills are opened against a table via the picker.
  const newOrder = useCallback(async () => {
    if (!locationId) { flash('No outlet configured'); return; }
    if (!shift) { flash('Start a shift before taking orders'); setFloatInput(''); setShiftModal('start'); return; }
    setBusy(true);
    try {
      const o = await apiClient<Order>('/api/v1/orders', {
        method: 'POST',
        body: JSON.stringify({ locationId, orderType: 'takeaway', covers: 1 }),
      });
      setOrder(o); setTenders([]); setKeypad('');
      if (typeof window !== 'undefined') window.history.replaceState(null, '', `/pos?order=${o.id}`);
      void refreshOpen();
    } finally { setBusy(false); }
  }, [locationId, refreshOpen, shift]);

  // top-level categories only for the tab strip (children inherit)
  const topCats = categories.filter(c => !c.parentId);
  const descendantIds = (catId: string): string[] => {
    const kids = categories.filter(c => c.parentId === catId).map(c => c.id);
    return [catId, ...kids.flatMap(descendantIds)];
  };

  function stationFor(p: Product) {
    const cat = categories.find(c => c.id === p.categoryId);
    const root = cat?.parentId ? categories.find(c => c.id === cat.parentId) : cat;
    return (root?.name === 'Beverages' || cat?.name === 'Beverages') ? 'bar' : 'kitchen';
  }

  async function addItem(p: Product) {
    if (!order) { flash('Open an order first'); return; }
    const av = avail[p.id];
    if (av && !av.available) { flash(`${p.name} is 86’d at this outlet${av.reason === 'ingredient_out' ? ' (out of an ingredient)' : av.reason === 'out_of_stock' ? ' (out of stock)' : ''}.`); return; }
    const station = stationFor(p);
    // If the product is sold in serving sizes, pick one first.
    if ((p.variantCount ?? 0) > 0) {
      const variants = await apiClient<Variant[]>(`/api/v1/products/${p.id}/variants`).catch(() => [] as Variant[]);
      if (variants.length > 0) { setSizePick({ product: p, station, variants }); return; }
    }
    await proceedAdd(p, station, null);
  }

  // After any size choice: collect modifier choices (if the product has groups) then post.
  async function proceedAdd(p: Product, station: string, variantId: string | null) {
    const groups = await apiClient<ModGroup[]>(`/api/v1/products/${p.id}/modifiers`).catch(() => [] as ModGroup[]);
    if (groups.length > 0) {
      const sel: Record<string, string[]> = {};
      for (const g of groups) sel[g.id] = g.items.filter(i => i.isDefault).map(i => i.id);
      setModPick({ product: p, station, groups, selected: sel, variantId });
      return;
    }
    await postItem(p.id, station, [], variantId);
  }

  async function postItem(productId: string, station: string, modifierItemIds: string[], variantId: string | null) {
    if (!order) return;
    setOrder(await apiClient<Order>(`/api/v1/orders/${order.id}/items`, {
      method: 'POST', body: JSON.stringify({ productId, quantity: 1, station, modifierItemIds, variantId }),
    }));
  }

  function toggleMod(groupId: string, itemId: string, single: boolean) {
    setModPick(mp => {
      if (!mp) return mp;
      const cur = mp.selected[groupId] ?? [];
      const next = single
        ? (cur.includes(itemId) ? [] : [itemId])
        : (cur.includes(itemId) ? cur.filter(x => x !== itemId) : [...cur, itemId]);
      return { ...mp, selected: { ...mp.selected, [groupId]: next } };
    });
  }

  async function confirmMods() {
    if (!modPick) return;
    // client-side required/min/max check (backend re-validates)
    for (const g of modPick.groups) {
      const n = (modPick.selected[g.id] ?? []).length;
      if (g.isRequired && n === 0) { flash(`Choose ${g.name}`); return; }
      if (n < g.minSelect) { flash(`${g.name}: pick at least ${g.minSelect}`); return; }
      if (g.maxSelect > 0 && n > g.maxSelect) { flash(`${g.name}: at most ${g.maxSelect}`); return; }
    }
    const ids = modPick.groups.flatMap(g => modPick.selected[g.id] ?? []);
    const { product, station, variantId } = modPick;
    setModPick(null);
    try { await postItem(product.id, station, ids, variantId); }
    catch (e) { flash((e as Error).message); }
  }
  async function setQty(item: OrderItem, q: number, managerPin?: string) {
    if (!order) return;
    try {
      setOrder(await apiClient<Order>(`/api/v1/orders/${order.id}/items/${item.id}`, {
        method: 'PUT', body: JSON.stringify({ quantity: q, ...(managerPin ? { managerPin } : {}) }),
      }));
    } catch (e) {
      const msg = (e as Error).message;
      // Voiding a line already fired to the kitchen needs manager approval (#71b) — prompt + retry.
      if (!managerPin && /manager approval/i.test(msg)) {
        const pin = typeof window !== 'undefined' ? window.prompt('Manager approval — this item was already sent to the kitchen. Enter a manager PIN to void it:') : null;
        if (pin) return setQty(item, q, pin);
      }
      flash(msg.replace(/^API \d+:\s*/, '').replace(/^\{.*"error":"?|"?(,.*)?\}$/g, ''));
    }
  }
  // Apply a bill discount as a fixed amount or a percentage (0 clears it).
  async function applyDiscount(value: number, mode: 'amount' | 'percent', managerPin?: string) {
    if (!order) return;
    setBusy(true);
    try {
      const body: Record<string, unknown> = mode === 'percent' ? { percent: value } : { amount: value };
      if (managerPin) body.managerPin = managerPin;
      setOrder(await apiClient<Order>(`/api/v1/orders/${order.id}/discount`, { method: 'POST', body: JSON.stringify(body) }));
      flash(value <= 0 ? 'Discount cleared' : mode === 'percent' ? `${value}% discount applied` : `Discount LKR ${lkr(value)} applied`);
      setDiscountModal(false); setDiscountVal('');
    } catch (e) {
      const msg = (e as Error).message;
      if (!managerPin && /manager approval/i.test(msg)) {
        const pin = typeof window !== 'undefined' ? window.prompt('Manager approval — enter a manager PIN to authorise this discount:') : null;
        if (pin) { setBusy(false); return applyDiscount(value, mode, pin); }
      }
      flash(msg.replace(/^API \d+:\s*/, '').replace(/^\{.*"error":"?|"?(,.*)?\}$/g, ''));
    } finally { setBusy(false); }
  }
  async function confirm() {
    if (!order) return; setBusy(true);
    try {
      const sent = await apiClient<Order>(`/api/v1/orders/${order.id}/confirm`, { method: 'POST' });
      setOrder(sent);
      // KOT routing: venues with a KOT printer (or printing-and-handing-over) get
      // the chit fired automatically on send; KDS venues just see it on the board.
      if (loyaltyCfg.kotAutoPrint) printReceipt(kitchenTicketHtml(sent));
      flash(loyaltyCfg.kotAutoPrint ? 'Sent to kitchen · KOT printing' : 'Sent to kitchen');
      void refreshOpen();
    }
    catch (e) { flash((e as Error).message); } finally { setBusy(false); }
  }

  // Print the kitchen ticket on demand (for venues with no KDS — print and hand over).
  function printKot() {
    if (!order || !hasItems) { flash('Add items before printing the KOT'); return; }
    printReceipt(kitchenTicketHtml(order));
  }
  const PAY_LABEL: Record<PayType, string> = {
    cash: 'Cash', card: 'Card', ubereats_prepaid: 'Uber Eats', pickme_prepaid: 'PickMe', credit: 'Credit (A/C)', loyalty: 'Points', advance: 'Advance',
  };
  // One tenant = one currency: billing, payment and the books are ALWAYS in the
  // business base currency (from org settings). A foreign-country branch is a separate tenant.
  const baseCode = loyaltyCfg.baseCurrency || currencies.find(c => c.isBase)?.code || 'LKR';
  const rateOf = (code?: string) => currencies.find(c => c.code === (code || baseCode))?.rateToBase ?? 1;
  // E-receipts add-on (#79): which channels the tenant has bought, ∩ what's actually built (email, SMS).
  const erEntitled = (loyaltyCfg.ereceipt?.channels ?? '').split(',').map(s => s.trim()).filter(Boolean);
  const erSendable = (['email', 'sms'] as const).filter(c => erEntitled.includes(c));
  const erQuota = loyaltyCfg.ereceipt?.quota ?? 0;
  const erUsed = loyaltyCfg.ereceipt?.used ?? 0;
  const outletRate = 1;                                   // billing never converts (kept for SummaryRow compat)
  // Format a base-currency amount for the bill.
  const cur = (n: number) => `${baseCode} ${lkr(n)}`;
  // Format an amount already in `code` currency (e.g. a foreign-cash tender).
  const curIn = (n: number, code: string) => `${code} ${lkr(n)}`;
  const isAcctTender = payType === 'credit' || payType === 'loyalty' || payType === 'advance';
  const selCcy = isAcctTender ? baseCode : tenderCcy;
  const selRate = rateOf(selCcy);
  const due = order?.totalAmount ?? 0;
  const paid = tenders.reduce((s, t) => s + t.amount * rateOf(t.currencyCode), 0);   // in base
  const remaining = Math.max(0, due - paid);
  // How much of each account tender is ALREADY applied in the cart — so we never let
  // the cashier stack more points/advance/credit than the customer actually holds
  // (the #66 over-redeem bug that produced "X points needed" > available).
  const tenderedOf = (pt: PayType) => tenders.filter(t => t.payType === pt).reduce((s, t) => s + t.amount, 0);
  // Account-tender ceilings — points/advance/credit can only tender what's available
  // (so redemption can be partial; the balance is taken on another tender). #66/#70
  const maxLoyalty = loyaltyCfg.enabled ? Math.floor((attachedCust?.loyaltyPoints ?? 0) * loyaltyCfg.redeemValue * 100) / 100 : 0;
  const maxAdvance = attachedCust?.advanceBalance ?? 0;
  const maxCredit = attachedCust?.creditAvailable ?? 0;
  // Remaining headroom for a tender = its ceiling minus what's already on the bill.
  const capFor = (pt: PayType) => pt === 'loyalty' ? Math.max(0, maxLoyalty - tenderedOf('loyalty'))
    : pt === 'advance' ? Math.max(0, maxAdvance - tenderedOf('advance'))
    : pt === 'credit' ? Math.max(0, maxCredit - tenderedOf('credit')) : Infinity;
  const tendered = keypad ? Number(keypad) : 0;          // in the selected currency
  const tenderedBase = tendered * selRate;
  // change applies once we'd be fully paid (cash overpayment) — in base currency
  const change = Math.max(0, paid + (tenderedBase || remaining) - due);

  // Add a split/partial payment of the keypad amount (or the remaining) for the selected type.
  function addTender() {
    if (!order) return;
    // amount is in the selected currency; default to the remaining base converted into it.
    let amt = tendered > 0 ? tendered : Math.ceil((remaining / selRate) * 100) / 100;
    // Account tenders (points/advance/credit): never exceed what's available — this
    // is what makes redemption partial. Take the rest on another tender (cash/card).
    const cap = capFor(payType);
    if (Number.isFinite(cap)) amt = Math.floor(Math.min(amt, cap, remaining) * 100) / 100;
    if (amt <= 0) {
      flash(payType === 'loyalty' ? 'No points available to redeem' : isAcctTender ? 'Nothing available on that tender' : 'Enter an amount');
      return;
    }
    const ccy = selCcy === baseCode ? undefined : selCcy;
    setTenders(t => [...t, { payType, amount: amt, currencyCode: ccy }]);
    setKeypad('');
    // After applying an account tender, fall back to cash so the big "Pay" button
    // settles the remaining balance as cash (not more points/credit). #66
    if (isAcctTender) setPayType('cash');
  }
  function removeTender(i: number) { setTenders(t => t.filter((_, idx) => idx !== i)); }

  // #66: redeem a chosen number of points (any partial amount up to the balance,
  // capped at what the customer holds). Blank = redeem the max that fits the bill.
  function applyPoints() {
    if (!order) return;
    const cap = capFor('loyalty');   // headroom = max redeemable minus points already applied
    const want = loyaltyInput ? Number(loyaltyInput) : Math.min(remaining, cap);
    const amt = Math.floor(Math.min(want, cap, remaining) * 100) / 100;
    if (amt <= 0) { flash(maxLoyalty <= 0 ? 'No points available' : cap <= 0 ? 'All available points already applied to this bill' : 'Enter an amount to redeem'); return; }
    if (loyaltyInput && Number(loyaltyInput) > cap + 0.009) flash(`Capped to LKR ${lkr(amt)} — the most these points can redeem now`);
    setTenders(t => [...t, { payType: 'loyalty', amount: amt }]);
    setLoyaltyInput(''); setKeypad('');
    setPayType('cash');   // points applied → take the balance as cash/card
  }

  // #76: set covers / steward / tour operator on the bill.
  async function saveMeta(patch: { covers?: number; stewardId?: string | null; tourOperatorId?: string | null }) {
    if (!order) return;
    try {
      setOrder(await apiClient<Order>(`/api/v1/orders/${order.id}/meta`, {
        method: 'POST',
        body: JSON.stringify({
          covers: patch.covers ?? null,
          // empty string clears (server treats Guid.Empty as clear)
          stewardId: patch.stewardId === undefined ? null : (patch.stewardId ?? '00000000-0000-0000-0000-000000000000'),
          tourOperatorId: patch.tourOperatorId === undefined ? null : (patch.tourOperatorId ?? '00000000-0000-0000-0000-000000000000'),
        }),
      }));
    } catch (e) { flash((e as Error).message); }
  }
  // #76: set the tip on the bill (added on top, untaxed).
  async function saveTip(amount: number) {
    if (!order) return;
    try { setOrder(await apiClient<Order>(`/api/v1/orders/${order.id}/tip`, { method: 'POST', body: JSON.stringify({ amount }) })); }
    catch (e) { flash((e as Error).message); }
  }

  async function addCustomItem() {
    if (!order || !openItem) return;
    const price = Number(openItem.price);
    if (!openItem.name.trim() || Number.isNaN(price) || price < 0) { flash('Enter a name and a valid price'); return; }
    const qty = Number(openItem.qty) || 1;
    const cat = null;
    setOrder(await apiClient<Order>(`/api/v1/orders/${order.id}/custom-item`, {
      method: 'POST',
      body: JSON.stringify({ name: openItem.name.trim(), unitPrice: price, quantity: qty, station: 'kitchen' }),
    }));
    setOpenItem(null);
  }

  async function settle() {
    if (!order) return;
    // Cash control: can't take payment without an open shift (server enforces too).
    if (!shift) { flash('Open a shift before taking payment'); setFloatInput(''); setShiftModal('start'); return; }
    // Build the final tender list: any added splits, plus a tender covering the
    // remaining (using the keypad amount if the cashier typed what they received).
    // Belt-and-suspenders: if no customer is attached, a points/advance/credit tender
    // can't be honoured — drop it so settle never 400s with "attach the customer…".
    const finalTenders = (order.customerId
      ? [...tenders]
      : tenders.filter(t => t.payType !== 'loyalty' && t.payType !== 'advance' && t.payType !== 'credit'));
    const baseOf = (t: { amount: number; currencyCode?: string }) => t.amount * rateOf(t.currencyCode);
    const remNow = due - finalTenders.reduce((s, t) => s + baseOf(t), 0);   // in base
    if (remNow > 0.009) {
      let amt = tendered > 0 ? tendered : Math.ceil((remNow / selRate) * 100) / 100;
      // Don't auto-tender more points/advance/credit than is available.
      const cap = capFor(payType);
      if (Number.isFinite(cap)) amt = Math.floor(Math.min(amt, cap, remNow) * 100) / 100;
      if (amt > 0) finalTenders.push({ payType, amount: amt, currencyCode: selCcy === baseCode ? undefined : selCcy });
    }
    const paidFinal = finalTenders.reduce((s, t) => s + baseOf(t), 0);   // in base
    if (paidFinal + 0.009 < due) { flash(`LKR ${lkr(due - paidFinal)} still due`); return; }

    setBusy(true);
    try {
      const o = await apiClient<Order>(`/api/v1/orders/${order.id}/settle`, {
        method: 'POST',
        body: JSON.stringify({ payments: finalTenders.map(t => ({ payType: t.payType, amount: t.amount, currencyCode: t.currencyCode })) }),
      });
      const chg = paidFinal - due;
      flash(`Settled · ${o.invoiceNumber ?? o.orderNumber}${chg > 0.009 ? ` · Change ${cur(chg)}` : ''}`);
      closeTerminal();
    } catch (e) { flash((e as Error).message); } finally { setBusy(false); }
  }
  async function voidOrder(managerPin?: string) {
    if (!order) return;
    if (!managerPin && !(await confirmDialog({ title: 'Void this order?', body: 'Voiding cancels the bill and cannot be undone.', confirmLabel: 'Void order', danger: true }))) return;
    setBusy(true);
    try {
      await apiClient<Order>(`/api/v1/orders/${order.id}/void`, {
        method: 'POST', body: JSON.stringify({ reason: 'Voided at POS', ...(managerPin ? { managerPin } : {}) }),
      });
      flash('Order voided'); closeTerminal();
    } catch (e) {
      const msg = (e as Error).message;
      if (!managerPin && /manager approval/i.test(msg)) {
        const pin = typeof window !== 'undefined' ? window.prompt('Manager approval — enter a manager PIN to authorise this void:') : null;
        if (pin) { setBusy(false); return voidOrder(pin); }
      }
      flash(msg);
    } finally { setBusy(false); }
  }
  function hold() { closeTerminal(); flash('Order held'); }

  // Print the bill/tax-invoice. Refuses an empty bill (nothing to print), then
  // pulls the compliant invoice (legal header + charge breakdown) and renders an
  // 80mm thermal receipt into a hidden iframe for the browser print dialog.
  async function printInvoice() {
    if (!order || !hasItems) { flash('Add items first — nothing to print'); return; }
    try {
      const inv = await apiClient<Invoice>(`/api/v1/orders/${order.id}/invoice`);
      printReceipt(receiptHtml(inv, order, 0, baseCode, 1, loyaltyCfg.taxLabel || 'VAT'));
    } catch (e) { flash((e as Error).message); }
  }

  // Recall a settled bill + reprint a duplicate (#78). Marks REPRINT + copy no.
  async function loadRecall() {
    try { setRecallRows(await apiClient(`/api/v1/orders/settled?search=${encodeURIComponent(recallSearch.trim())}&limit=30`)); }
    catch (e) { flash((e as Error).message); }
  }
  async function reprintBill(id: string) {
    try {
      const { reprintCount } = await apiClient<{ reprintCount: number }>(`/api/v1/orders/${id}/reprint`, { method: 'POST' });
      const inv = await apiClient<Invoice>(`/api/v1/orders/${id}/invoice`);
      const fake = { tableLabel: inv.tableLabel ?? null, covers: inv.covers ?? null, items: [],
        discountAmount: inv.discountAmount ?? 0, promotionDiscountAmount: inv.promotionDiscountAmount ?? 0 } as unknown as Order;
      printReceipt(receiptHtml(inv, fake, reprintCount, baseCode, 1, loyaltyCfg.taxLabel || 'VAT'));
      flash(`Reprinted ${inv.invoiceNumber ?? inv.orderNumber} (copy ${reprintCount})`);
      void loadRecall();
    } catch (e) { flash((e as Error).message); }
  }
  // View a settled bill on screen (no reprint, no print dialog) — just look it up.
  async function viewBill(id: string) {
    try {
      const inv = await apiClient<Invoice>(`/api/v1/orders/${id}/invoice`);
      const fake = { tableLabel: inv.tableLabel ?? null, covers: inv.covers ?? null, items: [],
        discountAmount: inv.discountAmount ?? 0, promotionDiscountAmount: inv.promotionDiscountAmount ?? 0 } as unknown as Order;
      const html = receiptHtml(inv, fake, 0, baseCode, 1, loyaltyCfg.taxLabel || 'VAT').replace('window.focus();window.print();', 'window.focus();');
      const w = window.open('', '_blank', 'width=420,height=720');
      if (w) { w.document.open(); w.document.write(html); w.document.close(); }
      else flash('Allow pop-ups to view the bill');
    } catch (e) { flash((e as Error).message); }
  }

  // Email / SMS a receipt for a settled bill (#79). Blank `to` → the customer on file.
  async function sendReceipt() {
    if (!sendFor) return;
    setSendBusy(true);
    try {
      const res = await apiClient<{ to: string }>(`/api/v1/orders/${sendFor.id}/send-receipt`, {
        method: 'POST',
        body: JSON.stringify({ channel: sendChannel, to: sendTo.trim() || undefined }),
      });
      flash(`Receipt sent by ${sendChannel === 'email' ? 'email' : 'SMS'} to ${res.to}`);
      setSendFor(null); setSendTo('');
    } catch (e) { flash((e as Error).message); }
    finally { setSendBusy(false); }
  }

  // Clear the terminal back to "no active order" and refresh the open-tab strip.
  function closeTerminal() {
    setOrder(null); setKeypad(''); setTenders([]);
    if (typeof window !== 'undefined') window.history.replaceState(null, '', '/pos');
    void refreshOpen();
  }

  // Open the floor-table picker (for a new bill, a move, or a merge).
  async function openTableModal(mode: 'new' | 'move' | 'merge') {
    if (!locationId) return;
    if (mode === 'new' && !shift) { flash('Start a shift before taking orders'); setFloatInput(''); setShiftModal('start'); return; }
    if (mode === 'merge') { setTableModal('merge'); return; }   // merge uses open tabs, not the floor
    try { setFloorTables(await apiClient(`/api/v1/tables/status?locationId=${locationId}`)); } catch { setFloorTables([]); }
    setTableModal(mode);
  }
  async function transferTo(tableId: string) {
    if (!order) return; setBusy(true);
    try { setOrder(await apiClient<Order>(`/api/v1/orders/${order.id}/transfer`, { method: 'POST', body: JSON.stringify({ tableId }) })); setTableModal(null); flash('Table moved'); void refreshOpen(); }
    catch (e) { flash((e as Error).message); } finally { setBusy(false); }
  }
  async function mergeFrom(sourceOrderId: string) {
    if (!order) return; setBusy(true);
    try { setOrder(await apiClient<Order>(`/api/v1/orders/${order.id}/merge`, { method: 'POST', body: JSON.stringify({ sourceOrderId }) })); setTableModal(null); flash('Bills merged'); void refreshOpen(); }
    catch (e) { flash((e as Error).message); } finally { setBusy(false); }
  }

  // Split selected item quantities onto a new bill (#69).
  async function doSplit() {
    if (!order) return;
    const lines = Object.entries(splitMoves).filter(([, q]) => q > 0).map(([itemId, q]) => ({ itemId, quantity: q }));
    if (lines.length === 0) { flash('Pick at least one item to move'); return; }
    setBusy(true);
    try {
      const dest = await apiClient<Order>(`/api/v1/orders/${order.id}/split`, { method: 'POST', body: JSON.stringify({ lines }) });
      setSplitModal(false); setSplitMoves({}); flash(`Split to ${dest.tableLabel ? `Table ${dest.tableLabel}` : dest.orderNumber}`);
      setOrder(dest); void refreshOpen();
    } catch (e) { flash((e as Error).message); } finally { setBusy(false); }
  }

  // ── CRM customer on the bill (#70) ────────────────────────────────────────
  function openCustomerModal() {
    if (!order) { flash('Open a bill first'); return; }
    setCustSearch(''); setNewCust(null); setCustModal(true); void searchCustomers('');
  }
  async function searchCustomers(q: string) {
    setCustSearch(q);
    try { setCustResults(await apiClient<Cust[]>(`/api/v1/customers?activeOnly=true${q ? `&search=${encodeURIComponent(q)}` : ''}`)); }
    catch { setCustResults([]); }
  }
  async function attachCustomer(id: string) {
    if (!order) return; setBusy(true);
    try { setOrder(await apiClient<Order>(`/api/v1/orders/${order.id}/customer`, { method: 'POST', body: JSON.stringify({ customerId: id }) })); setCustModal(false); setNewCust(null); flash('Customer attached'); }
    catch (e) { flash((e as Error).message); } finally { setBusy(false); }
  }
  async function detachCustomer() {
    if (!order) return; setBusy(true);
    try {
      setOrder(await apiClient<Order>(`/api/v1/orders/${order.id}/customer`, { method: 'DELETE' }));
      // Drop any customer-only tenders (points/credit/advance) — otherwise settle
      // fails with "attach the customer…" because the tender outlived the customer.
      setTenders(t => t.filter(x => x.payType !== 'credit' && x.payType !== 'loyalty' && x.payType !== 'advance'));
      if (isAcctTender) setPayType('cash');
      flash('Customer removed');
    }
    catch (e) { flash((e as Error).message); } finally { setBusy(false); }
  }
  async function quickAddCustomer() {
    if (!newCust) return;
    if (!newCust.name.trim()) { setNewCustErr('Customer name is required.'); return; }
    if (!newCust.phone.trim()) { setNewCustErr('Phone number is required.'); return; }
    setNewCustErr(null); setNewCustDup(null); setBusy(true);
    try {
      const c = await apiClient<Cust>('/api/v1/customers', { method: 'PUT', body: JSON.stringify({
        name: newCust.name.trim(), phone: newCust.phone.trim(),
        dateOfBirth: newCust.dob || null, address: newCust.address.trim() || null,
        taxNo: newCust.taxNo.trim() || null, email: newCust.email.trim() || null,
        isCreditCustomer: false, creditLimit: 0, isActive: true,
      }) });
      await attachCustomer(c.id);
    } catch (e) {
      const msg = (e as Error).message ?? '';
      const j = msg.indexOf('{');
      if (j >= 0) { try { const p = JSON.parse(msg.slice(j)); if (p.existingId) { setNewCustDup({ id: String(p.existingId), name: String(p.existingName ?? 'this customer') }); return; } } catch { /* not a dup conflict */ } }
      setNewCustErr(msg);
    } finally { setBusy(false); }
  }

  const visible = (activeCat ? products.filter(p => descendantIds(activeCat).includes(p.categoryId ?? '')) : products)
    .filter(p => {
      const q = search.trim().toLowerCase();
      if (!q) return true;
      return p.name.toLowerCase().includes(q) || p.sku.toLowerCase().includes(q) || (p.barcode ?? '').toLowerCase().includes(q);
    });

  // Barcode scanner support: a scan types the code then hits Enter. On Enter,
  // add the exact barcode/SKU match (or the only visible item) and clear search.
  function onSearchEnter(e: React.KeyboardEvent<HTMLInputElement>) {
    if (e.key !== 'Enter') return;
    const q = search.trim().toLowerCase();
    if (!q) return;
    const exact = products.find(p => (p.barcode ?? '').toLowerCase() === q || p.sku.toLowerCase() === q);
    const pick = exact ?? (visible.length === 1 ? visible[0] : null);
    if (!pick) return;
    e.preventDefault();
    if (!order) { flash('Open an order first'); return; }
    addItem(pick);
    setSearch('');
  }

  const hasItems = (order?.items.length ?? 0) > 0;
  const hrsWarn = hoursWarning(outletHours);   // recomputes each render; the 10s clock tick keeps it fresh

  return (
    <div className="flex h-screen flex-col overflow-hidden bg-surface text-on-surface">
      {/* ─── Top Bar: outlet · clock · open tabs · cashier · end shift ─── */}
      <header className="z-50 flex h-[56px] w-full items-center justify-between border-b border-border bg-card px-4">
        <div className="flex items-center gap-3">
          <span className="font-heading text-base font-bold text-on-surface">
            {[tenantName, outletName].filter(Boolean).join(' · ') || 'Point of Sale'}
          </span>
          <span className="text-sm font-medium text-muted-foreground">{clock}</span>
          {hrsWarn && (
            <span title="Operating hours are set for this outlet — billing isn't blocked, this is just a heads-up"
              className="flex items-center gap-1 rounded-full bg-amber-50 px-2 py-0.5 text-[11px] font-semibold text-amber-700 ring-1 ring-amber-200">
              <Icon name="schedule" className="text-[13px]" /> {hrsWarn}
            </span>
          )}
        </div>
        <nav className="flex max-w-[55vw] items-center gap-1 overflow-x-auto">
          {openOrders.map(t => {
            const active = order?.id === t.id;
            return (
              <button key={t.id} onClick={() => loadOrder(t.id)} disabled={busy} title={`${t.orderNumber} · LKR ${lkr(t.totalAmount)}`}
                className={`flex h-10 shrink-0 items-center gap-2 rounded-lg px-4 transition-colors disabled:opacity-50 ${active ? 'bg-primary font-bold text-primary-foreground shadow-sm' : 'text-muted-foreground hover:bg-muted'}`}>
                {t.tableLabel ? `T${t.tableLabel}` : t.orderNumber}{t.covers ? ` (${t.covers})` : ''}
                {t.status === 'confirmed' && <span className={`size-1.5 rounded-full ${active ? 'bg-white' : 'bg-status-pending'}`} />}
              </button>
            );
          })}
          {openOrders.length === 0 && <span className="px-2 text-xs text-muted-foreground">No open tabs</span>}
          <button onClick={() => openTableModal('new')} disabled={busy} title="New order — pick a table"
            className="flex size-10 shrink-0 items-center justify-center rounded-lg text-primary hover:bg-primary-tint disabled:opacity-50">
            <Icon name="add" />
          </button>
        </nav>
        <div className="flex items-center gap-3">
          <button onClick={() => { setRecallOpen(true); setRecallSearch(''); setRecallRows([]); void loadRecall(); }} title="Recall a settled bill to reprint"
            className="flex h-10 items-center gap-1.5 rounded-lg border border-border px-3 text-sm font-semibold text-muted-foreground hover:bg-muted hover:text-on-surface">
            <Icon name="receipt_long" className="text-base" /> Recall
          </button>
          <button onClick={toggleFullscreen} title={isFullscreen ? 'Exit full screen' : 'Full screen'}
            className="flex size-10 items-center justify-center rounded-lg border border-border text-muted-foreground hover:bg-muted hover:text-on-surface">
            <Icon name={isFullscreen ? 'fullscreen_exit' : 'fullscreen'} />
          </button>
          <div className="flex items-center gap-2 rounded-full border border-border bg-surface py-1 pl-1 pr-3">
            <div className="flex size-8 items-center justify-center rounded-full bg-primary-tint text-primary">
              <Icon name="person" className="text-xl" />
            </div>
            <div className="flex flex-col leading-none">
              <span className="text-xs font-bold text-on-surface">{session?.displayName ?? '—'}</span>
              <span className="text-[10px] uppercase tracking-tighter text-muted-foreground">
                {ROLE_LABEL[session?.role ?? 2]}
              </span>
            </div>
          </div>
          <button onClick={signOut} title="Sign out"
            className="flex size-10 items-center justify-center rounded-lg border border-border text-muted-foreground hover:bg-error/10 hover:text-error">
            <Icon name="logout" />
          </button>
          {shift ? (
            <div className="flex items-center gap-2">
              <span className="flex items-center gap-1 rounded-full bg-primary-tint px-2.5 py-1 text-[10px] font-bold uppercase tracking-wide text-primary">
                <span className="size-1.5 animate-pulse rounded-full bg-primary" /> {shift.shiftNumber}
              </span>
              <button onClick={openEndShift}
                className="h-10 rounded-lg bg-error px-4 text-sm font-bold text-white transition-transform active:scale-95">
                End shift
              </button>
            </div>
          ) : (
            <button onClick={() => { setFloatInput(''); setShiftModal('start'); }}
              className="flex h-10 items-center gap-1.5 rounded-lg bg-primary px-4 text-sm font-bold text-primary-foreground transition-transform active:scale-95">
              <Icon name="play_circle" className="text-base" /> Start shift
            </button>
          )}
        </div>
      </header>

      {/* ─── 3-column grid ─── */}
      <main className="grid flex-1 overflow-hidden" style={{ gridTemplateColumns: '340px 1fr 280px' }}>
        {/* ═══ LEFT: Active order ═══ */}
        <section className="flex h-full flex-col overflow-hidden border-r border-border bg-card">
          {order ? (
            <>
              <div className="border-b border-border bg-surface p-4">
                <div className="mb-1 flex items-start justify-between">
                  <h2 className="font-heading text-xl font-bold text-on-surface">
                    {order.tableLabel ? `Table ${order.tableLabel}` : order.orderNumber}{order.covers ? ` · ${order.covers} covers` : ''}
                  </h2>
                  <span className={`flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-bold ${order.status === 'open' ? 'pill-pending' : 'pill-progress'}`}>
                    <Icon name="schedule" className="text-sm" /> {order.status}
                  </span>
                </div>
                <p className="text-sm font-medium text-muted-foreground">#{order.orderNumber}</p>
                {/* Bill actions — full-width 3-up grid so Split never gets clipped on a narrow column */}
                <div className="mt-2 grid grid-cols-3 gap-1.5">
                  <button onClick={() => openTableModal('move')} disabled={busy} title="Move this bill to another table" className="flex h-10 items-center justify-center gap-1 rounded-lg border border-border bg-card text-sm font-semibold hover:bg-muted active:scale-95 disabled:opacity-50"><Icon name="swap_horiz" className="text-base" /> Move</button>
                  <button onClick={() => openTableModal('merge')} disabled={busy || openOrders.filter(o => o.id !== order.id).length === 0}
                    title={openOrders.filter(o => o.id !== order.id).length === 0 ? 'Open a second tab first, then merge it into this one' : 'Combine another open tab into this bill'}
                    className="flex h-10 items-center justify-center gap-1 rounded-lg border border-border bg-card text-sm font-semibold hover:bg-muted active:scale-95 disabled:opacity-50"><Icon name="merge" className="text-base" /> Merge</button>
                  <button onClick={() => { setSplitMoves({}); setSplitModal(true); }} disabled={busy || !hasItems} title="Split items onto a new bill"
                    className="flex h-10 items-center justify-center gap-1 rounded-lg border border-border bg-card text-sm font-semibold hover:bg-muted active:scale-95 disabled:opacity-50"><Icon name="call_split" className="text-base" /> Split</button>
                </div>
                {/* Customer (#70) */}
                <div className="mt-2 flex items-center justify-between">
                  {order.customerName ? (
                    <span className="flex items-center gap-1.5 text-sm">
                      <Icon name="person" className="text-base text-primary" />
                      <span className="font-medium text-on-surface">{order.customerName}</span>
                      {attachedCust?.isCreditCustomer && (
                        <span className="rounded bg-amber-500/15 px-1.5 text-[10px] font-bold uppercase text-amber-600">
                          Credit · {lkr(attachedCust.creditAvailable)} left
                        </span>
                      )}
                      {loyaltyCfg.enabled && (attachedCust?.loyaltyPoints ?? 0) > 0 && (
                        <span className="rounded bg-primary-tint px-1.5 text-[10px] font-bold uppercase text-primary">
                          {attachedCust!.loyaltyPoints} pts
                        </span>
                      )}
                      <button onClick={detachCustomer} disabled={busy} title="Remove customer" className="ml-0.5 text-muted-foreground hover:text-error disabled:opacity-50"><Icon name="close" className="text-sm" /></button>
                    </span>
                  ) : (
                    <button onClick={openCustomerModal} disabled={busy} className="flex items-center gap-1 text-sm font-semibold text-primary hover:underline disabled:opacity-50">
                      <Icon name="person_add" className="text-base" /> Add customer
                    </button>
                  )}
                </div>
              </div>

              {/* line items */}
              <div className="flex-1 space-y-2 overflow-y-auto p-2">
                {order.items.length === 0 && (
                  <p className="px-4 py-10 text-center text-sm text-muted-foreground">Tap menu items to add them.</p>
                )}
                {order.items.map(it => (
                  <div key={it.id}
                    className={`flex flex-col gap-1 rounded-lg border border-border bg-card p-3 ${it.station === 'bar' ? 'border-l-4 border-l-accent' : ''}`}>
                    <div className="flex items-start justify-between">
                      <div className="flex-1">
                        <div className="flex items-center gap-2">
                          <span className="font-bold text-on-surface">{Number(it.quantity)}× {it.productName}{it.variantName ? ` (${it.variantName})` : ''}</span>
                          {it.station === 'bar' && <span className="rounded bg-accent/15 px-1.5 text-[10px] font-black text-accent">BOT</span>}
                        </div>
                        {it.modifiers && it.modifiers.length > 0 && (
                          <span className="block text-xs text-muted-foreground">
                            {it.modifiers.map(m => m.name + (m.priceDelta ? ` (+${lkr(m.priceDelta)})` : '')).join(', ')}
                          </span>
                        )}
                        {it.notes && <span className="block text-sm italic text-muted-foreground">({it.notes})</span>}
                      </div>
                      {(it.discountAmount ?? 0) > 0.009 ? (
                        <span className="text-right tabular-nums">
                          <span className="mr-1.5 text-xs font-medium text-muted-foreground line-through">{cur(it.lineTotal + (it.discountAmount ?? 0))}</span>
                          <span className="font-bold text-primary">{cur(it.lineTotal)}</span>
                        </span>
                      ) : (
                        <span className="font-bold text-on-surface tabular-nums">{cur(it.lineTotal)}</span>
                      )}
                    </div>
                    <div className="mt-2 flex items-center justify-between">
                      <div className="flex items-center overflow-hidden rounded-lg border border-border">
                        <button onClick={() => setQty(it, Number(it.quantity) - 1)} className="flex size-10 items-center justify-center hover:bg-muted">
                          <Icon name="remove" className="text-sm" />
                        </button>
                        <span className="w-10 text-center font-bold tabular-nums">{Number(it.quantity)}</span>
                        <button onClick={() => setQty(it, Number(it.quantity) + 1)} className="flex size-10 items-center justify-center hover:bg-muted">
                          <Icon name="add" className="text-sm" />
                        </button>
                      </div>
                      <button onClick={() => setQty(it, 0)} className="text-error hover:opacity-80"><Icon name="delete" /></button>
                    </div>
                  </div>
                ))}
              </div>

              {/* summary */}
              <div className="space-y-1 border-t border-border bg-surface p-4">
                <SummaryRow label="Subtotal" value={order.subtotalAmount} ccy={baseCode} rate={outletRate} />
                {order.discountAmount > 0 && <SummaryRow label="Discount" value={-order.discountAmount} ccy={baseCode} rate={outletRate} />}
                {(order.promotionDiscountAmount ?? 0) > 0 && <SummaryRow label="Promotions" value={-(order.promotionDiscountAmount ?? 0)} ccy={baseCode} rate={outletRate} />}
                <SummaryRow label="Service charge" value={order.serviceChargeAmount} ccy={baseCode} rate={outletRate} />
                <SummaryRow label={`Tax (${loyaltyCfg.taxLabel || 'VAT'})`} value={order.taxAmount} className={(order.tipAmount ?? 0) > 0 ? '' : 'pb-2'} ccy={baseCode} rate={outletRate} />
                {(order.tipAmount ?? 0) > 0 && <SummaryRow label="Tip" value={order.tipAmount ?? 0} className="pb-2" ccy={baseCode} rate={outletRate} />}
                <div className="flex justify-between border-t border-border pt-2 text-2xl font-black text-on-surface">
                  <span>TOTAL</span>
                  <span className="tabular-nums">{cur(order.totalAmount)}</span>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-2 bg-surface p-3">
                <button onClick={hold} className="flex h-14 items-center justify-center rounded-lg border-2 border-primary font-bold text-primary transition-transform active:scale-95">
                  Hold
                </button>
                {/* Venues with the KOT printer off skip kitchen routing entirely — bill straight
                    from the till instead (#kot-auto-print). */}
                {loyaltyCfg.kotAutoPrint ? (
                  <button onClick={confirm} disabled={busy || !hasItems}
                    className="flex h-14 items-center justify-center gap-2 rounded-lg bg-primary font-bold text-primary-foreground transition-transform active:scale-95 disabled:opacity-50">
                    <Icon name="restaurant" /> Send to Kitchen
                  </button>
                ) : (
                  <button onClick={printInvoice} disabled={busy || !hasItems}
                    className="flex h-14 items-center justify-center gap-2 rounded-lg bg-primary font-bold text-primary-foreground transition-transform active:scale-95 disabled:opacity-50">
                    <Icon name="print" /> Print Bill
                  </button>
                )}
              </div>
            </>
          ) : (
            <div className="flex flex-1 flex-col items-center justify-center gap-4 p-6 text-center">
              <Icon name="receipt_long" className="text-5xl text-muted-foreground" />
              <p className="text-sm text-muted-foreground">No active order.</p>
              <button onClick={() => openTableModal('new')} disabled={busy}
                className="flex h-12 items-center gap-2 rounded-lg bg-primary px-5 font-bold text-primary-foreground transition-transform active:scale-95 disabled:opacity-50">
                <Icon name="add" /> New order
              </button>
              <p className="text-xs text-muted-foreground">Pick a table to start a bill, or tap an open tab above to resume.</p>
            </div>
          )}
        </section>

        {/* ═══ CENTER: categories + search + menu grid ═══ */}
        <section className="flex h-full flex-col overflow-hidden bg-surface">
          <div className="border-b border-border bg-card p-3 shadow-sm">
            <div className="scrollbar-hide mb-3 flex items-center gap-2 overflow-x-auto pb-2">
              <button onClick={() => setActiveCat(null)}
                className={`whitespace-nowrap rounded-full px-5 py-2 text-sm ${!activeCat ? 'border border-outline bg-primary font-bold text-primary-foreground shadow-md' : 'border border-transparent bg-surface font-semibold text-on-surface hover:bg-muted'}`}>
                All
              </button>
              {topCats.map(c => (
                <button key={c.id} onClick={() => setActiveCat(c.id)}
                  className={`whitespace-nowrap rounded-full px-5 py-2 text-sm ${activeCat === c.id ? 'bg-primary font-bold text-primary-foreground shadow-md' : 'border border-transparent bg-surface font-semibold text-on-surface hover:bg-muted'}`}>
                  {c.name}
                </button>
              ))}
            </div>
            <div className="flex gap-2">
              <div className="relative flex-1">
                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground"><Icon name="search" /></span>
                <input value={search} onChange={e => setSearch(e.target.value)} onKeyDown={onSearchEnter}
                  className="h-12 w-full rounded-lg border border-border bg-surface pl-10 pr-4 text-lg focus:border-primary focus:ring-primary"
                  placeholder="Search or scan barcode…" type="search" inputMode="search" enterKeyHint="search" autoComplete="off" />
              </div>
              <button onClick={() => order ? setOpenItem({ name: '', price: keypad || '', qty: '1' }) : flash('Open an order first')}
                className="flex h-12 items-center gap-1.5 whitespace-nowrap rounded-lg border border-dashed border-primary px-4 font-bold text-primary hover:bg-primary-tint">
                <Icon name="edit" className="text-base" /> Open item
              </button>
            </div>
          </div>

          {/* menu grid */}
          <div className="grid flex-1 auto-rows-[160px] grid-cols-2 gap-4 overflow-y-auto p-4 sm:grid-cols-3 lg:grid-cols-4">
            {visible.map(p => {
              const av = avail[p.id]; const off = av ? !av.available : false;
              return (
              <button key={p.id} onClick={() => addItem(p)} disabled={!order || off}
                title={off ? (av?.reason === 'manual_86' ? 'Manually 86’d at this outlet' : av?.reason === 'ingredient_out' ? 'Out of an ingredient' : 'Out of stock') : undefined}
                className={`group relative flex flex-col overflow-hidden rounded-lg border text-left transition-transform active:scale-95 disabled:active:scale-100 ${off ? 'border-border bg-muted opacity-60' : 'border-border bg-card disabled:opacity-40'}`}>
                <div className="flex h-24 w-full items-center justify-center" style={{ backgroundColor: (p.colorHex ?? '#d3ffd5') + '33' }}>
                  <Icon name="restaurant" className={`scale-150 ${off ? 'text-muted-foreground' : 'text-primary'}`} />
                </div>
                {off && <span className="absolute right-2 top-2 rounded-md bg-status-error px-1.5 py-0.5 text-[10px] font-black uppercase tracking-wide text-white shadow">86</span>}
                <div className="p-2">
                  <h3 className={`text-sm font-bold leading-tight ${off ? 'text-muted-foreground line-through' : 'text-on-surface'}`}>{p.name}</h3>
                  <p className={`mt-1 text-xs font-black tabular-nums ${off ? 'text-muted-foreground' : 'text-primary'}`}>{cur(p.basePrice)}</p>
                </div>
              </button>
            ); })}
            {visible.length === 0 && (
              <p className="col-span-full py-10 text-center text-sm text-muted-foreground">No items match.</p>
            )}
          </div>
        </section>

        {/* ═══ RIGHT: actions + payment tabs + keypad + footer ═══ */}
        <section className="flex h-full flex-col overflow-hidden border-l border-border bg-card">
          <div className="space-y-2 border-b border-border p-3">
            <button onClick={settle} disabled={busy || !order || !hasItems}
              className="h-16 w-full rounded-lg bg-status-paid text-xl font-black text-white shadow-sm transition-transform active:scale-95 disabled:opacity-50">
              {paid > 0 && remaining > 0.009 ? `Pay remaining · ${cur(remaining)}` : `Pay · ${cur(due)}`}
            </button>
            {/* Live-rate viewer (view-only — the bill is always charged in {baseCode}). */}
            <div className="flex items-center justify-between gap-2 rounded-lg bg-surface px-2 py-1.5 text-xs">
              <span className="flex items-center gap-1 text-muted-foreground">
                <Icon name="currency_exchange" className="text-sm" /> View in
                <select value={viewCcy} onChange={e => setViewCcy(e.target.value)}
                  className="rounded border border-border bg-card px-1 py-0.5 font-semibold focus:outline-none">
                  <option value="">—</option>
                  {VIEW_CURRENCIES.filter(c => c !== baseCode).map(c => <option key={c} value={c}>{c}</option>)}
                </select>
              </span>
              {viewCcy && viewCcy !== baseCode && (
                <span className="font-heading font-bold tabular-nums text-on-surface">
                  {viewRate ? `≈ ${viewCcy} ${lkr(due * viewRate)}` : '…'}
                </span>
              )}
            </div>
          </div>

          {/* payment method tabs */}
          <div className="p-3">
            <div className="mb-3 grid grid-cols-2 gap-1">
              {PAY_TABS.map(t => (
                <button key={t.key} onClick={() => setPayType(t.key)}
                  className={`flex h-10 items-center justify-center gap-1 rounded text-xs font-bold ${payType === t.key ? 'border border-outline bg-primary-tint text-primary-dark' : 'border border-border bg-surface hover:bg-muted'}`}>
                  {t.dot && <span className={`size-2 rounded-full ${t.dot}`} />}{t.label}
                </button>
              ))}
              {/* Credit (charge to account) — only for an attached credit customer */}
              {attachedCust?.isCreditCustomer && (
                <button onClick={() => setPayType('credit')}
                  className={`col-span-2 flex h-10 items-center justify-center gap-1 rounded text-xs font-bold ${payType === 'credit' ? 'border border-amber-500 bg-amber-500/15 text-amber-700' : 'border border-amber-500/40 bg-surface text-amber-700 hover:bg-amber-500/10'}`}>
                  <Icon name="account_balance_wallet" className="text-sm" /> Credit (A/C) · {lkr(attachedCust.creditAvailable)} available
                </button>
              )}
              {/* Loyalty points — redeem when the attached customer has a balance */}
              {loyaltyCfg.enabled && (attachedCust?.loyaltyPoints ?? 0) > 0 && (
                <button onClick={() => setPayType('loyalty')}
                  className={`col-span-2 flex h-10 items-center justify-center gap-1 rounded text-xs font-bold ${payType === 'loyalty' ? 'border border-primary bg-primary-tint text-primary-dark' : 'border border-primary/40 bg-surface text-primary hover:bg-primary-tint'}`}>
                  <Icon name="loyalty" className="text-sm" /> Points · {attachedCust!.loyaltyPoints} pts (LKR {lkr(attachedCust!.loyaltyPoints * loyaltyCfg.redeemValue)})
                </button>
              )}
              {/* Advance / deposit — draw down when the customer has a prepaid balance */}
              {(attachedCust?.advanceBalance ?? 0) > 0 && (
                <button onClick={() => setPayType('advance')}
                  className={`col-span-2 flex h-10 items-center justify-center gap-1 rounded text-xs font-bold ${payType === 'advance' ? 'border border-primary bg-primary-tint text-primary-dark' : 'border border-primary/40 bg-surface text-primary hover:bg-primary-tint'}`}>
                  <Icon name="savings" className="text-sm" /> Advance · LKR {lkr(attachedCust!.advanceBalance)} available
                </button>
              )}
            </div>

            {/* tenders already added (split payments) */}
            {tenders.length > 0 && (
              <div className="mb-2 space-y-1">
                {tenders.map((t, i) => (
                  <div key={i} className="flex items-center justify-between rounded bg-surface px-2 py-1 text-xs">
                    <span className="font-semibold">{PAY_LABEL[t.payType]}{t.currencyCode ? ` · ${t.currencyCode}` : ''}</span>
                    <span className="flex items-center gap-2 tabular-nums">
                      {t.currencyCode ? `${t.currencyCode} ${lkr(t.amount)} (LKR ${lkr(t.amount * rateOf(t.currencyCode))})` : `LKR ${lkr(t.amount)}`}
                      <button onClick={() => removeTender(i)} className="text-error"><Icon name="close" className="text-sm" /></button>
                    </span>
                  </div>
                ))}
              </div>
            )}

            {/* amount due / tendered / change */}
            <div className="mb-2 grid grid-cols-2 gap-2">
              <div className="rounded-lg bg-surface p-2">
                <div className="text-[10px] uppercase tracking-tight text-muted-foreground">{paid > 0 ? 'Remaining' : 'Amount due'}</div>
                <div className="font-heading text-base font-bold tabular-nums">{cur(paid > 0 ? remaining : due)}</div>
              </div>
              <div className="rounded-lg bg-surface p-2">
                <div className="text-[10px] uppercase tracking-tight text-muted-foreground">Tendered ({PAY_LABEL[payType]})</div>
                <div className="font-heading text-base font-bold tabular-nums">{curIn(tendered, selCcy)}</div>
                {change > 0.009 &&
                  <div className="text-[10px] font-bold text-status-paid">Change: {cur(change)}</div>}
              </div>
            </div>

            {/* Billing is single-currency (base). Foreign currencies are view-only via
                the "View in" selector above the keypad — the bill is always charged in {baseCode}. */}

            {/* Redeem points (#66) — type ANY amount (e.g. 10, 20, 500) up to the
                balance, capped at what the customer holds; or Max. Then take the
                rest as cash/card. */}
            {payType === 'loyalty' && (attachedCust?.loyaltyPoints ?? 0) > 0 && (
              <div className="mb-2 rounded-lg border border-primary/40 bg-primary-tint/50 p-2">
                <div className="mb-1.5 flex items-center justify-between text-xs">
                  <span className="font-bold text-primary-dark">Redeem points</span>
                  <span className="text-muted-foreground">{attachedCust!.loyaltyPoints} pts · up to LKR {lkr(capFor('loyalty'))}</span>
                </div>
                <div className="flex gap-2">
                  <input value={loyaltyInput} onChange={e => setLoyaltyInput(e.target.value.replace(/[^0-9.]/g, ''))} inputMode="decimal"
                    placeholder="LKR to redeem (partial OK)" className="h-9 flex-1 rounded-lg border border-border bg-surface px-2 text-right text-sm tabular-nums focus:border-primary focus:ring-primary" />
                  <button onClick={() => setLoyaltyInput(String(Math.floor(Math.min(remaining, capFor('loyalty')) * 100) / 100))}
                    className="h-9 rounded-lg border border-border bg-card px-3 text-xs font-semibold hover:bg-muted">Max</button>
                  <button onClick={applyPoints} className="h-9 rounded-lg bg-primary px-3 text-xs font-bold text-primary-foreground hover:bg-primary-dark">Apply</button>
                </div>
                {capFor('loyalty') < remaining - 0.009 && (
                  <p className="mt-1.5 text-[11px] text-muted-foreground">Points cover up to <b>LKR {lkr(capFor('loyalty'))}</b> of the <b>LKR {lkr(remaining)}</b> due — take the rest on cash/card after applying.</p>
                )}
              </div>
            )}

            {/* add split payment — for advance/credit the amount is capped at
                what's available, so the label shows what will actually be applied.
                (Points use the dedicated Redeem panel above.) */}
            {payType !== 'loyalty' && (() => {
              const cap = capFor(payType);
              const want = tendered > 0 ? tendered : remaining;
              const willAdd = Number.isFinite(cap) ? Math.floor(Math.min(want, cap, remaining) * 100) / 100 : (tendered > 0 ? tendered : remaining);
              const label = isAcctTender
                ? `Apply ${PAY_LABEL[payType]} (LKR ${lkr(willAdd)})`
                : `Add ${PAY_LABEL[payType]} payment ${tendered > 0 ? `(${selCcy} ${lkr(tendered)})` : '(remaining)'}`;
              return (
                <button onClick={addTender} disabled={!order || (isAcctTender && willAdd <= 0)}
                  className="mb-3 flex h-9 w-full items-center justify-center gap-1 rounded-lg border border-primary/30 bg-primary-tint text-xs font-bold text-primary-dark disabled:opacity-50">
                  <Icon name="add" className="text-sm" /> {label}
                </button>
              );
            })()}

            {/* numeric keypad — inert until a bill is active. Decimal key lets the
                cashier enter cents (e.g. 2886.60); only one '.' allowed. */}
            <div className="grid grid-cols-3 gap-2">
              {['1','2','3','4','5','6','7','8','9'].map(k => (
                <button key={k} disabled={!order} onClick={() => setKeypad(s => s + k)}
                  className="h-14 rounded-lg border border-border bg-surface text-xl font-bold active:bg-muted disabled:opacity-40">{k}</button>
              ))}
              <button disabled={!order} onClick={() => setKeypad(s => (s.includes('.') ? s : (s || '0') + '.'))}
                className="h-14 rounded-lg border border-border bg-surface text-xl font-bold active:bg-muted disabled:opacity-40">.</button>
              <button disabled={!order} onClick={() => setKeypad(s => s + '0')}
                className="h-14 rounded-lg border border-border bg-surface text-xl font-bold active:bg-muted disabled:opacity-40">0</button>
              <button disabled={!order} onClick={() => setKeypad(s => s.slice(0, -1))}
                className="flex h-14 items-center justify-center rounded-lg border border-error/20 bg-error/10 text-error">
                <Icon name="backspace" />
              </button>
            </div>
          </div>

          {/* footer actions */}
          <div className="mt-auto space-y-2 border-t border-border bg-surface p-3">
            <div className="grid grid-cols-2 gap-2">
              <button onClick={() => { if (!order || !hasItems) { flash('Add items first'); return; } setDiscountVal(''); setDiscountMode('amount'); setDiscountModal(true); }}
                className="flex h-12 items-center justify-center gap-1 rounded border border-border bg-card text-xs font-semibold text-on-surface hover:bg-muted">
                <Icon name="percent" className="text-sm" /> Discount
              </button>
              <button onClick={() => { if (!order) { flash('Open an order first'); return; } setTipVal(order.tipAmount ? String(order.tipAmount) : ''); setDetailsModal(true); }}
                className="flex h-12 items-center justify-center gap-1 rounded border border-border bg-card text-xs font-semibold text-on-surface hover:bg-muted">
                <Icon name="groups" className="text-sm" /> Details
              </button>
              <button onClick={() => setKeypad('')}
                className="flex h-12 items-center justify-center gap-1 rounded border border-border bg-card text-xs font-semibold text-on-surface hover:bg-muted">
                <Icon name="call_split" className="text-sm" /> Clear
              </button>
              <button onClick={printInvoice} disabled={busy || !order || !hasItems}
                className="flex h-12 items-center justify-center gap-1 rounded border border-border bg-card text-xs font-semibold text-on-surface hover:bg-muted disabled:opacity-50">
                <Icon name="receipt_long" className="text-sm" /> Print bill
              </button>
              <button onClick={printKot} disabled={busy || !order || !hasItems} title="Print the kitchen ticket (for venues without a kitchen display)"
                className="flex h-12 items-center justify-center gap-1 rounded border border-border bg-card text-xs font-semibold text-on-surface hover:bg-muted disabled:opacity-50">
                <Icon name="print" className="text-sm" /> Print KOT
              </button>
            </div>
            <button onClick={() => voidOrder()} disabled={busy || !order}
              className="flex h-12 w-full items-center justify-center gap-1 rounded border border-error/20 bg-card text-xs font-semibold text-error hover:bg-error/10 disabled:opacity-50">
              <Icon name="cancel" className="text-sm" /> Void
            </button>
          </div>
        </section>
      </main>

      {/* ─── Bottom status strip ─── */}
      <footer className="z-50 flex h-[40px] w-full items-center justify-between bg-on-surface px-4 text-white">
        <div className="flex items-center gap-2">
          <span className="size-2 animate-pulse rounded-full bg-primary-tint" />
          <span className="text-[10px] uppercase tracking-wider text-primary-tint">Connected</span>
        </div>
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-2 text-[10px] uppercase tracking-wider text-white/60">
            <Icon name="print" className="text-xs" /> 1 ticket queued for printer
          </div>
          <div className="text-[10px] uppercase tracking-wider text-white/60">V 2.4.0-PRO</div>
        </div>
      </footer>

      {/* Open / custom-price item modal */}
      {openItem && (
        <Modal title="Open item" size="sm" onClose={() => setOpenItem(null)}
          footer={
            <div className="flex gap-2">
              <button onClick={() => setOpenItem(null)} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted">Cancel</button>
              <button onClick={addCustomItem} className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark">Add to order</button>
            </div>
          }>
            <p className="mb-4 text-sm text-muted-foreground">A custom line at the price the customer pays.</p>
            <div className="space-y-3">
              <div>
                <label className="mb-1 block text-sm font-semibold text-slate-700">Item name</label>
                <input autoFocus value={openItem.name} onChange={e => setOpenItem(o => o && { ...o, name: e.target.value })}
                  placeholder="e.g. Special platter" className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20" />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="mb-1 block text-sm font-semibold text-slate-700">Price ({outletCurrency})</label>
                  <input value={openItem.price} onChange={e => setOpenItem(o => o && { ...o, price: e.target.value.replace(/[^0-9.]/g, '') })}
                    inputMode="decimal" placeholder="0.00" className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 tabular-nums focus:border-primary focus:ring-2 focus:ring-primary/20" />
                </div>
                <div>
                  <label className="mb-1 block text-sm font-semibold text-slate-700">Qty</label>
                  <input value={openItem.qty} onChange={e => setOpenItem(o => o && { ...o, qty: e.target.value.replace(/[^0-9]/g, '') })}
                    inputMode="numeric" placeholder="1" className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 tabular-nums focus:border-primary focus:ring-2 focus:ring-primary/20" />
                </div>
              </div>
            </div>
        </Modal>
      )}

      {/* Start shift modal */}
      {shiftModal === 'start' && (
        <Modal title="Start shift" size="sm" onClose={() => !shiftBusy && setShiftModal(null)}
          footer={
            <div className="flex gap-2">
              <button onClick={() => setShiftModal(null)} disabled={shiftBusy} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50">Cancel</button>
              <button onClick={startShift} disabled={shiftBusy} className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{shiftBusy ? 'Starting…' : 'Start shift'}</button>
            </div>
          }>
            <p className="mb-4 text-sm text-muted-foreground">Count the cash in the drawer to open with.</p>
            <label className="mb-1 block text-sm font-semibold text-slate-700">Opening float ({outletCurrency})</label>
            <input autoFocus value={floatInput} onChange={e => setFloatInput(e.target.value.replace(/[^0-9.]/g, ''))}
              inputMode="decimal" placeholder="0.00"
              className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 tabular-nums focus:border-primary focus:ring-2 focus:ring-primary/20" />
        </Modal>
      )}

      {/* End shift / cash-up modal */}
      {shiftModal === 'end' && shift && (
        <Modal title={`End shift · ${shift.shiftNumber}`} size="md" onClose={() => !shiftBusy && setShiftModal(null)}
          footer={
            <div className="flex gap-2">
              <button onClick={() => setShiftModal(null)} disabled={shiftBusy} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50">Cancel</button>
              <button onClick={() => closeShift()} disabled={shiftBusy} className="h-11 flex-1 rounded-lg bg-error font-bold text-white hover:opacity-90 disabled:opacity-50">{shiftBusy ? 'Closing…' : 'Close shift'}</button>
            </div>
          }>
            <p className="mb-4 text-sm text-muted-foreground">Cash-up (Z-report). Count the drawer and enter the total.</p>
            <div className="space-y-1.5 rounded-lg border border-border bg-surface p-3">
              <SummaryRow label="Opening float" value={shift.openingFloat} ccy={baseCode} rate={outletRate} />
              <SummaryRow label={`Sales (${shift.orderCount} orders)`} value={shift.totalSales} ccy={baseCode} rate={outletRate} />
              <SummaryRow label="Cash takings" value={shift.cashSales} ccy={baseCode} rate={outletRate} />
              <SummaryRow label="Card" value={shift.cardSales} ccy={baseCode} rate={outletRate} />
              {shift.otherSales > 0 && <SummaryRow label="Other / prepaid" value={shift.otherSales} ccy={baseCode} rate={outletRate} />}
              <div className="mt-1 flex justify-between border-t border-border pt-2 text-sm font-bold text-on-surface">
                <span>Expected in drawer</span><span className="tabular-nums">{cur(shift.expectedCash)}</span>
              </div>
            </div>
            <label className="mb-1 mt-4 block text-sm font-semibold text-slate-700">Counted cash ({outletCurrency})</label>
            <input autoFocus value={declaredInput} onChange={e => setDeclaredInput(e.target.value.replace(/[^0-9.]/g, ''))}
              inputMode="decimal" placeholder="0.00"
              className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 tabular-nums focus:border-primary focus:ring-2 focus:ring-primary/20" />
            {declaredInput !== '' && (
              <p className={`mt-1.5 text-sm font-semibold ${Number(declaredInput) - shift.expectedCash === 0 ? 'text-muted-foreground' : Number(declaredInput) - shift.expectedCash < 0 ? 'text-status-error' : 'text-primary'}`}>
                Variance: {cur(Number(declaredInput) - shift.expectedCash)} {Number(declaredInput) - shift.expectedCash < 0 ? '(short)' : Number(declaredInput) - shift.expectedCash > 0 ? '(over)' : ''}
              </p>
            )}
        </Modal>
      )}

      {/* Open-bill gate — shown the instant you tap End shift with a live bill, BEFORE
          the cash-up. The shift can't be closed over an open bill: you settle it,
          void it, or explicitly carry it to the next shift. */}
      {openBillConflict && (
        <Modal title="Can't end the shift yet — open bill" size="md" onClose={() => !shiftBusy && setOpenBillConflict(null)}>
            <p className="mb-1 text-sm font-semibold text-on-surface">{openBillConflict}</p>
            <p className="mb-5 text-sm text-muted-foreground">
              You can&apos;t cash up over a live bill. Go back and settle it, or choose what happens to it before counting the drawer.
            </p>
            <div className="flex flex-col gap-2">
              <button onClick={() => setOpenBillConflict(null)} disabled={shiftBusy}
                className="flex h-11 items-center justify-center gap-2 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">
                <Icon name="point_of_sale" className="text-base" /> Go back &amp; settle the bill
              </button>
              <button onClick={carryOverAndCashUp} disabled={shiftBusy}
                className="flex h-11 items-center justify-center gap-2 rounded-lg border border-border font-semibold text-on-surface hover:bg-muted disabled:opacity-50">
                <Icon name="schedule" className="text-base" /> Keep bill open for the next shift
              </button>
              <button onClick={voidAllOpenBills} disabled={shiftBusy}
                className="flex h-11 items-center justify-center gap-2 rounded-lg border border-error/30 font-semibold text-error hover:bg-error/10 disabled:opacity-50">
                <Icon name="cancel" className="text-base" /> Void the bill(s) &amp; continue
              </button>
            </div>
        </Modal>
      )}

      {/* Z-report result */}
      {zReport && (
        <Modal title={`Z-report · ${zReport.shiftNumber}`} size="md" onClose={() => setZReport(null)}
          footer={<button onClick={() => setZReport(null)} className="h-11 w-full rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark">Done</button>}>
            <div className="space-y-1.5 rounded-lg border border-border bg-surface p-3">
              <SummaryRow label="Opening float" value={zReport.openingFloat} ccy={baseCode} rate={outletRate} />
              <SummaryRow label={`Sales (${zReport.orderCount} orders)`} value={zReport.totalSales} ccy={baseCode} rate={outletRate} />
              <SummaryRow label="Cash takings" value={zReport.cashSales} ccy={baseCode} rate={outletRate} />
              <SummaryRow label="Card" value={zReport.cardSales} ccy={baseCode} rate={outletRate} />
              {zReport.otherSales > 0 && <SummaryRow label="Other / prepaid" value={zReport.otherSales} ccy={baseCode} rate={outletRate} />}
              <SummaryRow label="Expected in drawer" value={zReport.expectedCash} className="font-semibold text-on-surface" ccy={baseCode} rate={outletRate} />
              <SummaryRow label="Counted" value={zReport.declaredCash ?? 0} className="font-semibold text-on-surface" ccy={baseCode} rate={outletRate} />
              <div className={`mt-1 flex justify-between border-t border-border pt-2 text-sm font-bold ${(zReport.cashVariance ?? 0) < 0 ? 'text-status-error' : 'text-primary'}`}>
                <span>Variance {(zReport.cashVariance ?? 0) < 0 ? '(short)' : (zReport.cashVariance ?? 0) > 0 ? '(over)' : ''}</span>
                <span className="tabular-nums">{cur(zReport.cashVariance ?? 0)}</span>
              </div>
            </div>
        </Modal>
      )}

      {/* Serving-size picker */}
      {sizePick && (
        <Modal title={sizePick.product.name} size="md" onClose={() => setSizePick(null)}
          footer={<button onClick={() => setSizePick(null)} className="h-11 w-full rounded-lg border border-border font-semibold hover:bg-muted">Cancel</button>}>
            <p className="mb-3 text-sm text-muted-foreground">Choose a size</p>
            <div className="flex-1 space-y-2 overflow-y-auto">
              {sizePick.variants.map(v => (
                <button key={v.id}
                  onClick={() => { const sp = sizePick; setSizePick(null); void proceedAdd(sp.product, sp.station, v.id).catch(e => flash((e as Error).message)); }}
                  className="flex w-full items-center justify-between rounded-lg border border-border px-4 py-3 text-left hover:border-primary hover:bg-primary-tint">
                  <span className="font-semibold">{v.name}</span>
                  <span className="tabular-nums text-muted-foreground">{cur(v.price)}</span>
                </button>
              ))}
            </div>
        </Modal>
      )}

      {/* Modifier / add-on picker */}
      {modPick && (
        <Modal title={modPick.product.name} size="md" onClose={() => setModPick(null)}
          footer={
            <div className="flex gap-2">
              <button onClick={() => setModPick(null)} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted">Cancel</button>
              <button onClick={confirmMods} className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark">Add to order</button>
            </div>
          }>
            <p className="mb-3 text-sm text-muted-foreground">Choose options</p>
            <div className="flex-1 space-y-4 overflow-y-auto">
              {modPick.groups.map(g => {
                const single = g.maxSelect === 1;
                const chosen = modPick.selected[g.id] ?? [];
                return (
                  <div key={g.id}>
                    <div className="mb-1 flex items-center gap-2 text-sm font-semibold text-slate-700">
                      {g.name}
                      {g.isRequired && <span className="rounded bg-error/15 px-1.5 text-[10px] font-bold text-error">REQUIRED</span>}
                      {g.maxSelect > 0 && <span className="text-[10px] text-muted-foreground">max {g.maxSelect}</span>}
                    </div>
                    <div className="space-y-1.5">
                      {g.items.map(it => {
                        const on = chosen.includes(it.id);
                        return (
                          <button key={it.id} onClick={() => toggleMod(g.id, it.id, single)}
                            className={`flex w-full items-center justify-between rounded-lg border px-3 py-2 text-left text-sm ${on ? 'border-primary bg-primary-tint text-primary' : 'border-border hover:bg-muted'}`}>
                            <span className="flex items-center gap-2">
                              <span className={`flex size-4 items-center justify-center border ${single ? 'rounded-full' : 'rounded-md'} ${on ? 'border-primary bg-primary text-white' : 'border-border'}`}>
                                {on && <Icon name="check" className="text-[11px]" />}
                              </span>
                              {it.name}
                            </span>
                            {it.priceDelta !== 0 && <span className="tabular-nums text-muted-foreground">+{cur(it.priceDelta)}</span>}
                          </button>
                        );
                      })}
                    </div>
                  </div>
                );
              })}
            </div>
        </Modal>
      )}

      {/* Bill details — covers / steward / tour operator / tip (#76) */}
      {detailsModal && order && (
        <Modal title="Bill details" size="sm" onClose={() => setDetailsModal(false)}
          footer={<button onClick={() => setDetailsModal(false)} className="h-11 w-full rounded-lg border border-border font-semibold hover:bg-muted">Done</button>}>
            <div className="space-y-4">
              <div>
                <label className="mb-1 block text-xs font-semibold text-muted-foreground">Covers (guests)</label>
                <input type="number" min={0} defaultValue={order.covers ?? ''} inputMode="numeric"
                  onBlur={e => { const v = Number(e.target.value); if ((order.covers ?? 0) !== v) void saveMeta({ covers: v > 0 ? v : undefined }); }}
                  className="h-11 w-full rounded-lg border border-border bg-surface px-3 focus:border-primary focus:ring-primary" />
              </div>
              <div>
                <label className="mb-1 block text-xs font-semibold text-muted-foreground">Steward / waiter</label>
                <select value={order.stewardId ?? ''} onChange={e => void saveMeta({ stewardId: e.target.value || null })}
                  className="h-11 w-full rounded-lg border border-border bg-surface px-3 focus:border-primary focus:ring-primary">
                  <option value="">— none —</option>
                  {stewards.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                </select>
              </div>
              {tourOps.length > 0 && (
                <div>
                  <label className="mb-1 block text-xs font-semibold text-muted-foreground">Tour operator</label>
                  <select value={order.tourOperatorId ?? ''} onChange={e => void saveMeta({ tourOperatorId: e.target.value || null })}
                    className="h-11 w-full rounded-lg border border-border bg-surface px-3 focus:border-primary focus:ring-primary">
                    <option value="">— none —</option>
                    {tourOps.map(t => <option key={t.id} value={t.id}>{t.name} ({lkr(t.commissionPercent)}%)</option>)}
                  </select>
                </div>
              )}
              <div>
                <label className="mb-1 block text-xs font-semibold text-muted-foreground">Tip (added to the bill, untaxed)</label>
                <div className="flex gap-2">
                  <input value={tipVal} onChange={e => setTipVal(e.target.value.replace(/[^0-9.]/g, ''))} inputMode="decimal" placeholder="0.00"
                    className="h-11 flex-1 rounded-lg border border-border bg-surface px-3 focus:border-primary focus:ring-primary" />
                  <button onClick={() => void saveTip(Number(tipVal) || 0)}
                    className="h-11 rounded-lg bg-primary px-4 font-bold text-primary-foreground hover:bg-primary-dark">Set</button>
                </div>
              </div>
            </div>
        </Modal>
      )}

      {/* Discount */}
      {discountModal && order && (
        <Modal title="Discount" size="sm" onClose={() => setDiscountModal(false)}
          footer={
            <div className="flex gap-2">
              {order.discountAmount > 0 && <button disabled={busy} onClick={() => applyDiscount(0, 'amount')} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50">Clear</button>}
              <button onClick={() => setDiscountModal(false)} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted">Cancel</button>
              <button disabled={busy || !discountVal} onClick={() => applyDiscount(Number(discountVal), discountMode)}
                className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">Apply</button>
            </div>
          }>
            <p className="mb-3 text-sm text-muted-foreground">On {order.tableLabel ? `Table ${order.tableLabel}` : order.orderNumber} · subtotal {cur(order.subtotalAmount)}</p>
            <div className="mb-3 grid grid-cols-2 gap-1">
              {(['amount', 'percent'] as const).map(m => (
                <button key={m} onClick={() => setDiscountMode(m)}
                  className={`h-10 rounded-lg text-sm font-bold ${discountMode === m ? 'bg-primary text-primary-foreground' : 'border border-border bg-surface hover:bg-muted'}`}>
                  {m === 'amount' ? `${outletCurrency} amount` : '% percent'}
                </button>
              ))}
            </div>
            <input value={discountVal} onChange={e => setDiscountVal(e.target.value.replace(/[^0-9.]/g, ''))} inputMode="decimal" autoFocus
              placeholder={discountMode === 'percent' ? 'e.g. 10' : 'e.g. 500'}
              className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 text-right text-lg tabular-nums focus:border-primary focus:ring-2 focus:ring-primary/20" />
            {order.discountAmount > 0 && <p className="mt-2 text-xs text-muted-foreground">Current discount: {cur(order.discountAmount)}</p>}
        </Modal>
      )}

      {/* Recall a settled bill → reprint a duplicate (#78) */}
      {recallOpen && (
        <Modal title="Recall a bill" size="lg" onClose={() => setRecallOpen(false)}>
            <div className="mb-3 flex gap-2">
              <input value={recallSearch} onChange={e => setRecallSearch(e.target.value)} onKeyDown={e => { if (e.key === 'Enter') void loadRecall(); }}
                placeholder="Search invoice / order no / customer…" className="h-10 flex-1 rounded-lg border border-border bg-surface px-3 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20" />
              <button onClick={() => void loadRecall()} className="rounded-lg bg-primary px-4 text-sm font-bold text-primary-foreground hover:bg-primary-dark">Search</button>
            </div>
            <div className="flex-1 space-y-1 overflow-y-auto">
              {recallRows.length === 0 && <p className="py-6 text-center text-sm text-muted-foreground">No settled bills found.</p>}
              {recallRows.map(r => (
                <div key={r.id} className="flex items-center justify-between rounded-lg border border-border px-3 py-2 text-sm">
                  <div>
                    <div className="font-semibold">{r.invoiceNumber ?? r.orderNumber}{r.tableLabel ? ` · T${r.tableLabel}` : ''}</div>
                    <div className="text-xs text-muted-foreground">{r.settledAt ? new Date(r.settledAt).toLocaleString('en-LK') : ''}{r.customerName ? ` · ${r.customerName}` : ''}{r.reprintCount > 0 ? ` · reprinted ${r.reprintCount}×` : ''}</div>
                  </div>
                  <div className="flex items-center gap-3">
                    <span className="font-semibold tabular-nums">{cur(r.totalAmount)}</span>
                    <button onClick={() => viewBill(r.id)} className="flex items-center gap-1 rounded-lg border border-border px-3 py-1.5 text-xs font-semibold hover:bg-muted"><Icon name="visibility" className="text-sm" /> View</button>
                    <button onClick={() => reprintBill(r.id)} className="flex items-center gap-1 rounded-lg border border-border px-3 py-1.5 text-xs font-semibold hover:bg-muted"><Icon name="print" className="text-sm" /> Reprint</button>
                    <button onClick={() => { setSendFor({ id: r.id, label: r.invoiceNumber ?? r.orderNumber }); setSendChannel(erSendable[0] ?? 'email'); setSendTo(''); }} className="flex items-center gap-1 rounded-lg border border-border px-3 py-1.5 text-xs font-semibold hover:bg-muted"><Icon name="send" className="text-sm" /> Send</button>
                  </div>
                </div>
              ))}
            </div>
        </Modal>
      )}

      {/* E-receipt (#79) — email or SMS a settled bill to the guest (gated by the E-Receipts add-on) */}
      {sendFor && (
        <Modal title="Send receipt" size="sm" onClose={() => !sendBusy && setSendFor(null)}
          footer={erSendable.length > 0 ? (
            <button disabled={sendBusy} onClick={sendReceipt}
              className="w-full rounded-lg bg-primary py-2.5 text-sm font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">
              {sendBusy ? 'Sending…' : `Send by ${sendChannel === 'email' ? 'email' : 'SMS'}`}
            </button>
          ) : (
            <a href="/settings?billing=1" className="block w-full rounded-lg bg-primary py-2.5 text-center text-sm font-bold text-primary-foreground hover:bg-primary-dark">See plans</a>
          )}>
          {erSendable.length === 0 ? (
            <div className="space-y-2 text-sm">
              <div className="text-muted-foreground">Bill <span className="font-semibold text-on-surface">{sendFor.label}</span></div>
              <p className="rounded-lg border border-dashed border-border bg-muted/40 px-3 py-3 text-sm">E-receipts aren’t on your plan yet. Add the <b>E-Receipts</b> add-on in <b>Settings → Billing</b> to email or text bills to your guests.</p>
            </div>
          ) : (
            <div className="space-y-3 text-sm">
              <div className="text-muted-foreground">Bill <span className="font-semibold text-on-surface">{sendFor.label}</span></div>
              <div className={`grid gap-2 ${erSendable.length > 1 ? 'grid-cols-2' : 'grid-cols-1'}`}>
                {erSendable.map(ch => (
                  <button key={ch} onClick={() => setSendChannel(ch)}
                    className={`flex h-10 items-center justify-center gap-1.5 rounded-lg border text-sm font-semibold ${sendChannel === ch ? 'border-primary bg-primary-tint text-primary-dark' : 'border-border bg-surface hover:bg-muted'}`}>
                    <Icon name={ch === 'email' ? 'mail' : 'sms'} className="text-base" /> {ch === 'email' ? 'Email' : 'SMS'}
                  </button>
                ))}
              </div>
              <input value={sendTo} onChange={e => setSendTo(e.target.value)} inputMode={sendChannel === 'email' ? 'email' : 'tel'}
                placeholder={sendChannel === 'email' ? 'name@email.com' : '07X XXX XXXX'}
                className="h-10 w-full rounded-lg border border-border bg-surface px-3 text-sm focus:border-primary focus:ring-primary" />
              <p className="text-xs text-muted-foreground">Leave blank to use the contact saved on the bill’s customer.{erQuota > 0 ? ` · ${Math.max(0, erQuota - erUsed)} of ${erQuota} left this month.` : ''}</p>
            </div>
          )}
        </Modal>
      )}

      {/* Split bill (#69) — choose how many of each line to move to a new bill */}
      {splitModal && order && (
        <Modal title="Split bill" size="md" onClose={() => setSplitModal(false)}
          footer={
            <div className="flex gap-2">
              <button onClick={() => setSplitModal(false)} disabled={busy} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50">Cancel</button>
              <button onClick={doSplit} disabled={busy || Object.values(splitMoves).every(q => !q)}
                className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">Move to new bill</button>
            </div>
          }>
            <p className="mb-3 text-sm text-muted-foreground">Pick how many of each item to move to a new bill at this table.</p>
            <div className="flex-1 space-y-2 overflow-y-auto">
              {order.items.map(it => {
                const moving = splitMoves[it.id] ?? 0;
                const set = (n: number) => setSplitMoves(m => ({ ...m, [it.id]: Math.max(0, Math.min(Number(it.quantity), n)) }));
                return (
                  <div key={it.id} className="flex items-center justify-between gap-2 rounded-lg border border-border p-2">
                    <div className="min-w-0 flex-1">
                      <div className="truncate text-sm font-medium">{it.productName}{it.variantName ? ` (${it.variantName})` : ''}</div>
                      <div className="text-xs text-muted-foreground">{Number(it.quantity)} on bill · {cur(it.unitPrice)}</div>
                    </div>
                    <div className="flex items-center overflow-hidden rounded-lg border border-border">
                      <button onClick={() => set(moving - 1)} className="flex size-9 items-center justify-center hover:bg-muted"><Icon name="remove" className="text-sm" /></button>
                      <span className="w-8 text-center font-bold tabular-nums">{moving}</span>
                      <button onClick={() => set(moving + 1)} className="flex size-9 items-center justify-center hover:bg-muted"><Icon name="add" className="text-sm" /></button>
                    </div>
                  </div>
                );
              })}
            </div>
        </Modal>
      )}

      {/* Customer picker / quick-add (#70) */}
      {custModal && (
        <Modal title={newCust ? 'New customer' : 'Attach customer'} size="md" onClose={() => setCustModal(false)}>
            {newCust ? (
              <div className="space-y-3">
                {([
                  ['name', 'Customer name', 'text', true],
                  ['phone', 'Phone number', 'tel', true],
                  ['dob', 'Birthday', 'date', false],
                  ['address', 'Address', 'text', false],
                  ['taxNo', 'NIC / Tax ID', 'text', false],
                  ['email', 'Email', 'email', false],
                ] as const).map(([key, label, type, required]) => (
                  type === 'date' ? (
                    <label key={key} className="block">
                      <span className="mb-1 block text-xs font-semibold text-muted-foreground">{label}<span className="font-normal"> (optional)</span></span>
                      <DateTimePicker mode="date" value={newCust[key]} placeholder="Pick a date"
                        onChange={v => setNewCust(c => c && { ...c, [key]: v })} />
                    </label>
                  ) : (
                    <label key={key} className="block">
                      <span className="mb-1 block text-xs font-semibold text-muted-foreground">{label}{required ? <span className="text-status-error"> *</span> : <span className="font-normal"> (optional)</span>}</span>
                      <input
                        autoFocus={key === 'name'}
                        type={type === 'tel' || type === 'text' ? 'text' : type}
                        inputMode={type === 'tel' ? 'tel' : undefined}
                        value={newCust[key]}
                        onChange={e => { const v = e.target.value; setNewCust(c => c && { ...c, [key]: v }); if (newCustErr) setNewCustErr(null); if (newCustDup) setNewCustDup(null); }}
                        className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 outline-none focus:border-primary focus:ring-2 focus:ring-primary/20" />
                    </label>
                  )
                ))}
                {newCustDup && (
                  <div className="rounded-lg border border-amber-300 bg-amber-50 p-3">
                    <p className="text-sm font-semibold text-amber-900">This number is already registered to {newCustDup.name}.</p>
                    <p className="mt-0.5 text-xs text-amber-800">Attach the existing customer instead of creating a duplicate?</p>
                    <div className="mt-2 flex gap-2">
                      <button onClick={() => { const id = newCustDup.id; setNewCustDup(null); void attachCustomer(id); }}
                        className="rounded-lg bg-primary px-3 py-1.5 text-sm font-bold text-primary-foreground hover:bg-primary-dark">Attach {newCustDup.name}</button>
                      <button onClick={() => setNewCustDup(null)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm font-semibold hover:bg-muted">Use a different number</button>
                    </div>
                  </div>
                )}
                {newCustErr && <p className="text-sm font-medium text-status-error">{newCustErr}</p>}
                <div className="flex gap-2">
                  <button onClick={() => { setNewCust(null); setNewCustErr(null); setNewCustDup(null); }} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted">Back</button>
                  <button onClick={quickAddCustomer} disabled={busy} className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">Add &amp; attach</button>
                </div>
                <p className="text-xs text-muted-foreground">Set up credit limits &amp; discounts later from the Customers screen.</p>
              </div>
            ) : (
              <>
                <input autoFocus type="search" value={custSearch} onChange={e => searchCustomers(e.target.value)} placeholder="Search name / phone…" enterKeyHint="search"
                  className="mb-3 w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20" />
                <div className="max-h-72 space-y-1 overflow-y-auto">
                  {custResults.map(c => (
                    <button key={c.id} onClick={() => attachCustomer(c.id)} disabled={busy}
                      className="flex w-full items-center justify-between rounded-lg border border-border px-3 py-2 text-left text-sm hover:bg-muted disabled:opacity-50">
                      <span><span className="font-medium">{c.name}</span>{c.phone ? <span className="ml-1 text-xs text-muted-foreground">{c.phone}</span> : null}</span>
                      {c.isCreditCustomer && <span className="rounded bg-amber-500/15 px-1.5 text-[10px] font-bold uppercase text-amber-600">Credit</span>}
                    </button>
                  ))}
                  {custResults.length === 0 && <p className="px-1 py-4 text-center text-sm text-muted-foreground">No matches.</p>}
                </div>
                <button onClick={() => { setNewCust({ name: custSearch, phone: '', dob: '', address: '', taxNo: '', email: '' }); setNewCustErr(null); setNewCustDup(null); }}
                  className="mt-3 flex w-full items-center justify-center gap-1 rounded-lg border border-dashed border-primary/40 py-2.5 text-sm font-semibold text-primary hover:bg-primary-tint">
                  <Icon name="person_add" className="text-base" /> New customer
                </button>
              </>
            )}
        </Modal>
      )}

      {/* Table picker / merge */}
      {tableModal && (
        <Modal size="lg" onClose={() => setTableModal(null)}
          title={tableModal === 'new' ? 'Pick a table' : tableModal === 'move' ? 'Move to table' : 'Merge tables'}
          footer={
            <div className="flex gap-2">
              {tableModal === 'new' && <button onClick={() => { setTableModal(null); void newOrder(); }} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted">Takeaway / no table</button>}
              <button onClick={() => setTableModal(null)} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted">Cancel</button>
            </div>
          }>
            {tableModal === 'merge' ? (
              <p className="mb-3 text-sm text-muted-foreground">
                Pick the tab to combine <strong>into {order?.tableLabel ? `Table ${order.tableLabel}` : order?.orderNumber}</strong>. Its items move onto this bill and that tab is closed.
              </p>
            ) : (
              <p className="mb-3 text-sm text-muted-foreground">
                {tableModal === 'new' ? 'Green = free (start a bill) · amber = occupied (resume).' : 'Choose the table to move this bill to. Amber tables are taken.'}
              </p>
            )}
            <div className="flex-1 overflow-y-auto">
              {tableModal === 'merge' ? (
                <div className="space-y-2">
                  {openOrders.filter(o => o.id !== order?.id).map(o => (
                    <button key={o.id} onClick={() => mergeFrom(o.id)} disabled={busy}
                      className="flex w-full items-center justify-between rounded-lg border border-border px-4 py-3 text-left hover:border-primary hover:bg-primary-tint disabled:opacity-50">
                      <span className="font-semibold">{o.tableLabel ? `Table ${o.tableLabel}` : o.orderNumber}</span>
                      <span className="tabular-nums text-muted-foreground">{cur(o.totalAmount)}</span>
                    </button>
                  ))}
                  {openOrders.filter(o => o.id !== order?.id).length === 0 && <p className="py-6 text-center text-sm text-muted-foreground">No other open tabs.</p>}
                </div>
              ) : (
                <div className="grid grid-cols-3 gap-2 sm:grid-cols-4">
                  {floorTables.map(t => {
                    const isCurrent = order?.tableId === t.id;
                    const disabled = busy || isCurrent || (tableModal === 'move' && t.occupied);
                    const onClickTable = () => {
                      if (tableModal === 'new') { setTableModal(null); t.occupied && t.orderId ? void loadOrder(t.orderId) : void openTable(t.id); }
                      else void transferTo(t.id);
                    };
                    return (
                      <button key={t.id} onClick={onClickTable} disabled={disabled}
                        className={`flex h-20 flex-col items-start justify-between rounded-lg border-2 p-2 text-left disabled:opacity-40 ${t.occupied ? 'border-status-pending/40 bg-status-pending/10' : 'border-primary/30 bg-primary-tint hover:border-primary'}`}>
                        <span className="font-heading font-bold">{t.code}</span>
                        <span className="text-[10px] text-muted-foreground">{t.occupied ? 'occupied' : `${t.seats}p`}</span>
                      </button>
                    );
                  })}
                  {floorTables.length === 0 && <p className="col-span-full py-6 text-center text-sm text-muted-foreground">No tables — add them on the Floor screen.</p>}
                </div>
              )}
            </div>
        </Modal>
      )}

      {toast && <div className="fixed bottom-12 left-1/2 z-50 -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">{toast}</div>}
    </div>
  );
}

function SummaryRow({ label, value, className = '', ccy = 'LKR', rate = 1 }: { label: string; value: number; className?: string; ccy?: string; rate?: number }) {
  return (
    <div className={`flex justify-between text-sm text-muted-foreground ${className}`}>
      <span>{label}</span>
      <span className="tabular-nums">{ccy} {lkr(value / (rate || 1))}</span>
    </div>
  );
}
