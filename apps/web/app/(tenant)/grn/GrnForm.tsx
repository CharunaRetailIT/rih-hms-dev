'use client';

import { useEffect, useMemo, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import {
  ArrowLeft,
  ClipboardList,
  Plus,
  Receipt,
  Save,
  Send,
  Trash2,
  Wallet,
} from 'lucide-react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient, lkr } from '@/lib/api-client';
import { SearchableSelect } from '@/components/ui/SearchableSelect';

type Location = { id: string; code: string; name: string; isActive: boolean };
type Uom = { id: string; code: string; name: string; symbol: string | null };
type LocationTax = { rate: number; flat: number };

type PurchaseOrder = {
  id: string;
  poNumber: string;
  supplierId: string;
  locationId: string;
  status: string;
};
type PoLine = {
  productId: string;
  sku: string;
  productName: string;
  quantityOrdered: number;
  quantityReceived: number;
  unitCost: number;
  unitId: string | null;
  unitSymbol: string | null;
};
type PoDetail = PurchaseOrder & { lines: PoLine[] };

// Tax is header-level now (the GRN's location's tax-type Charges, see fetchLocationTax
// below) — a product only decides whether it's taxable at all (isTaxable + standard
// tax class) and whether its listed cost already includes that tax.
type ProductMeta = {
  taxable: boolean;
  isTaxInclusive: boolean;
  tracksExpiry: boolean;
  unitId: string;
  costPrice: number;
};

type DraftLine = {
  productId: string;
  productLabel: string;
  poQty: number | null; // remaining PO qty, only set in PO-based mode
  unitId: string;
  quantity: string;
  freeQuantity: string;
  discountMode: 'amount' | 'percent';
  discountValue: string;
  unitCost: string;
  taxable: boolean;
  tracksExpiry: boolean;
  batchNo: string;
  expiryDate: string;
};

// Matches the Purchasing page's own RECEIVABLE set — a PO only accepts
// goods once it's cleared approval (or is already partly received).
const PO_RECEIVABLE = new Set(['approved', 'partially_received']);

const PAYMENT_METHODS = [
  { value: '', label: 'Not set' },
  { value: 'cash', label: 'Cash' },
  { value: 'bank_transfer', label: 'Bank transfer' },
  { value: 'cheque', label: 'Cheque' },
  { value: 'credit', label: 'Credit' },
];

const CURRENCIES = ['LKR', 'USD', 'EUR', 'GBP', 'INR', 'AUD', 'AED', 'SGD', 'MVR'];

const INPUT =
  'w-full rounded-lg border border-border bg-card px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary disabled:opacity-60';
const INPUT_RIGHT = `${INPUT} text-right tabular-nums`;

type PagedApiResult<T> = { data: T[]; pagination: { totalPages: number } };

async function fetchPage<T>(
  url: string,
  { page, pageSize, search }: { page: number; pageSize: number; search: string },
): Promise<PagedApiResult<T>> {
  const qs = new URLSearchParams({ pageNumber: String(page), pageSize: String(pageSize), isActive: 'true' });
  if (search) qs.set('search', search);
  return apiClient<PagedApiResult<T>>(`${url}?${qs}`);
}

async function fetchSupplierOptions(args: { page: number; pageSize: number; search: string }) {
  const res = await fetchPage<{ id: string; name: string; paymentTermsDays: number; isVatRegistered: boolean }>(
    '/api/v1/suppliers/paged',
    args,
  );
  return {
    items: res.data.map(s => ({
      id: s.id,
      label: s.name,
      data: { paymentTermsDays: s.paymentTermsDays, isVatRegistered: s.isVatRegistered },
    })),
    hasMore: args.page < res.pagination.totalPages,
  };
}

async function fetchLocationOptions(args: { page: number; pageSize: number; search: string }) {
  const res = await fetchPage<{ id: string; code: string; name: string }>('/api/v1/locations/paged', args);
  return {
    items: res.data.map(l => ({ id: l.id, label: `${l.code} — ${l.name}` })),
    hasMore: args.page < res.pagination.totalPages,
  };
}

async function fetchProductOptions(args: { page: number; pageSize: number; search: string }) {
  const res = await fetchPage<{ id: string; sku: string; name: string; costPrice: number }>(
    '/api/v1/products/paged',
    args,
  );
  return {
    items: res.data.map(p => ({ id: p.id, label: `${p.name} (${p.sku})`, data: { name: p.name, sku: p.sku } })),
    hasMore: args.page < res.pagination.totalPages,
  };
}

async function fetchProductMeta(productId: string): Promise<ProductMeta> {
  const p = await apiClient<{
    isTaxable: boolean;
    taxClass: string;
    isTaxInclusive: boolean;
    tracksExpiry: boolean;
    unitOfMeasureId: string;
    purchaseUnitOfMeasureId: string | null;
    costPrice: number;
  }>(`/api/v1/products/${productId}`);
  return {
    taxable: p.isTaxable && p.taxClass === 'standard',
    isTaxInclusive: Boolean(p.isTaxInclusive),
    tracksExpiry: p.tracksExpiry,
    unitId: p.purchaseUnitOfMeasureId ?? p.unitOfMeasureId,
    costPrice: p.costPrice,
  };
}

async function fetchLocationTax(locationId: string): Promise<LocationTax> {
  if (!locationId) return { rate: 0, flat: 0 };
  return apiClient<LocationTax>(`/api/v1/purchase-orders/tax-rate?locationId=${locationId}`);
}

// Product's listed cost price already includes tax — back the rate out so the
// cost price shown/entered on the line is ex-tax, matching what the header tax
// gets computed off of.
function exTaxCost(costPrice: number, meta: ProductMeta, tax: LocationTax): number {
  if (!meta.isTaxInclusive || tax.rate <= 0) return costPrice;
  return Math.round((costPrice / (1 + tax.rate / 100)) * 10000) / 10000;
}

const emptyLine = (): DraftLine => ({
  productId: '', productLabel: '', poQty: null, unitId: '',
  quantity: '', freeQuantity: '', discountMode: 'amount', discountValue: '',
  unitCost: '', taxable: false, tracksExpiry: false,
  batchNo: '', expiryDate: '',
});

export default function GrnForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const presetPoId = searchParams.get('poId');

  const [allLocations, setAllLocations] = useState<Location[]>([]);
  const [allUoms, setAllUoms] = useState<Uom[]>([]);
  const [allPos, setAllPos] = useState<PurchaseOrder[]>([]);
  // The GRN's location determines which tax-type Charges apply — a single shared
  // rate/flat for every line, refetched whenever the location changes.
  const [locationTax, setLocationTax] = useState<LocationTax>({ rate: 0, flat: 0 });

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [mode, setMode] = useState<'manual' | 'po'>('po');
  const [supplierId, setSupplierId] = useState('');
  const [supplierLabel, setSupplierLabel] = useState('');
  const [supplierVatRegistered, setSupplierVatRegistered] = useState(false);
  const [poId, setPoId] = useState('');
  const [poLoading, setPoLoading] = useState(false);
  const [supplierInvoiceNo, setSupplierInvoiceNo] = useState('');
  const [locationId, setLocationId] = useState('');
  const [grnDate, setGrnDate] = useState(() => new Date().toISOString().slice(0, 10));
  const [referenceNo, setReferenceNo] = useState('');
  const [notes, setNotes] = useState('');
  const [currencyCode, setCurrencyCode] = useState('LKR');
  const [paymentTermsDays, setPaymentTermsDays] = useState('');
  const [paymentMethod, setPaymentMethod] = useState('');
  const [headerDiscountAmount, setHeaderDiscountAmount] = useState('0');
  const [headerDiscountPercent, setHeaderDiscountPercent] = useState('0');
  const [otherCharges, setOtherCharges] = useState('0');
  const [lines, setLines] = useState<DraftLine[]>([emptyLine()]);

  useEffect(() => {
    (async () => {
      try {
        const [locations, uoms, pos] = await Promise.all([
          // `all=true` so a location assigned before it was deactivated still resolves a label.
          apiClient<Location[]>('/api/v1/locations?all=true'),
          apiClient<Uom[]>('/api/v1/units-of-measure'),
          apiClient<PurchaseOrder[]>('/api/v1/purchase-orders'),
        ]);
        setAllLocations(locations);
        setAllUoms(uoms);
        setAllPos(pos.filter(p => PO_RECEIVABLE.has(p.status)));
        const activeLocations = locations.filter(x => x.isActive);
        setLocationId(activeLocations.find(x => x.code === 'MAIN')?.id ?? activeLocations[0]?.id ?? '');
      } catch (e) {
        setError((e as Error).message);
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  // Tax is sourced from the GRN's location, not per-product — refetch the combined
  // rate/flat whenever the location changes (including the initial default above).
  useEffect(() => {
    if (!locationId) { setLocationTax({ rate: 0, flat: 0 }); return; }
    fetchLocationTax(locationId).then(setLocationTax).catch(() => setLocationTax({ rate: 0, flat: 0 }));
  }, [locationId]);

  // Arriving from "Receive" on a PO's row (Purchasing page) — deep-link
  // straight into PO Based mode with that PO preselected, once the initial
  // load above has resolved.
  useEffect(() => {
    if (loading || !presetPoId || poId) return;
    setMode('po');
    setPoId(presetPoId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, presetPoId]);

  // Loading a PO prefills supplier/location + lines from its remaining quantities.
  useEffect(() => {
    if (mode !== 'po' || !poId) return;
    (async () => {
      setPoLoading(true);
      try {
        const po = await apiClient<PoDetail>(`/api/v1/purchase-orders/${poId}`);
        // Deep-linked straight to a PO (no prior manual supplier pick) — the
        // supplier fields are still blank, so resolve them here too.
        if (po.supplierId !== supplierId) {
          setSupplierId(po.supplierId);
          apiClient<{ name: string; isVatRegistered: boolean }>(`/api/v1/suppliers/${po.supplierId}`)
            .then(s => {
              setSupplierLabel(s.name);
              setSupplierVatRegistered(s.isVatRegistered);
            })
            .catch(() => {});
        }
        setLocationId(po.locationId);
        const metas = await Promise.all(po.lines.map(l => fetchProductMeta(l.productId)));
        setLines(
          po.lines.map((l, i) => {
            const remaining = Math.max(0, l.quantityOrdered - l.quantityReceived);
            const meta = metas[i];
            return {
              productId: l.productId, productLabel: `${l.productName} (${l.sku})`,
              poQty: remaining, unitId: l.unitId ?? meta.unitId,
              quantity: String(remaining), freeQuantity: '', discountMode: 'amount', discountValue: '',
              // The PO line's unitCost was already stored ex-tax by the PO form/backend.
              unitCost: String(l.unitCost),
              taxable: meta.taxable,
              tracksExpiry: meta.tracksExpiry, batchNo: '', expiryDate: '',
            };
          }),
        );
      } catch (e) {
        setError((e as Error).message);
      } finally {
        setPoLoading(false);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [mode, poId]);

  const locationLabel = (id: string) => {
    const loc = allLocations.find(l => l.id === id);
    return loc ? `${loc.code} — ${loc.name}` : '';
  };
  const poOptions = useMemo(
    () => allPos.filter(p => !supplierId || p.supplierId === supplierId).map(p => ({ id: p.id, label: p.poNumber })),
    [allPos, supplierId],
  );

  const setLine = (idx: number, patch: Partial<DraftLine>) =>
    setLines(prev => prev.map((l, i) => (i === idx ? { ...l, ...patch } : l)));

  const addLine = () => setLines(prev => [...prev, emptyLine()]);
  const removeLine = (idx: number) =>
    setLines(prev => (prev.length === 1 ? prev : prev.filter((_, i) => i !== idx)));

  function lineGrossOf(l: DraftLine): number {
    const q = (Number(l.quantity) || 0);
    const c = Number(l.unitCost) || 0;
    return q * c;
  }
  function lineDiscountOf(l: DraftLine): number {
    const gross = lineGrossOf(l);
    const v = Number(l.discountValue) || 0;
    return l.discountMode === 'percent' ? (gross * v) / 100 : v;
  }
  function lineCostValueOf(l: DraftLine): number {
    return Math.max(0, lineGrossOf(l) - lineDiscountOf(l));
  }
  function lineTaxOf(l: DraftLine): number {
    if (!supplierVatRegistered || !l.taxable) return 0;
    const cost = lineCostValueOf(l);
    return Math.round(cost * (locationTax.rate / 100) * 10000) / 10000;
  }

  const totalCostValue = useMemo(() => lines.reduce((s, l) => s + lineCostValueOf(l), 0), [lines]);
  const totalDiscounts = useMemo(() => lines.reduce((s, l) => s + lineDiscountOf(l), 0), [lines]);
  // Flat tax-type charges (a fixed levy, say) apply once for the whole document,
  // not per line — added on top of the summed per-line percentage tax.
  const totalTaxes = useMemo(() => {
    const perLine = lines.reduce((s, l) => s + lineTaxOf(l), 0);
    const anyTaxed = supplierVatRegistered && lines.some(l => l.taxable);
    return perLine + (anyTaxed ? locationTax.flat : 0);
  }, [lines, supplierVatRegistered, locationTax]);
  const grossAmount = totalCostValue;
  const netAmount =
    grossAmount - (Number(headerDiscountAmount) || 0) + (Number(otherCharges) || 0) + totalTaxes;

  function syncHeaderDiscountFromAmount(amount: string) {
    setHeaderDiscountAmount(amount);
    const a = Number(amount) || 0;
    setHeaderDiscountPercent(grossAmount > 0 ? String(Math.round((a / grossAmount) * 10000) / 100) : '0');
  }
  function syncHeaderDiscountFromPercent(percent: string) {
    setHeaderDiscountPercent(percent);
    const p = Number(percent) || 0;
    setHeaderDiscountAmount(String(Math.round(((grossAmount * p) / 100) * 100) / 100));
  }

  function validate(): string | null {
    if (!supplierId) return 'Choose a supplier.';
    if (!locationId) return 'Choose a location.';
    if (mode === 'po' && !poId) return 'Choose a purchase order, or switch to Manual mode.';
    const filled = lines.filter(l => l.productId);
    if (filled.length === 0) return 'Add at least one line.';
    for (const l of filled) {
      const q = Number(l.quantity);
      if (!Number.isFinite(q) || q <= 0) return 'Each line needs a GRN quantity greater than zero.';
      const c = Number(l.unitCost);
      if (!Number.isFinite(c) || c < 0) return 'Cost price cannot be negative.';
    }
    return null;
  }

  function resetForm() {
    setMode('po');
    setSupplierId(''); setSupplierLabel(''); setSupplierVatRegistered(false);
    setPoId(''); setSupplierInvoiceNo('');
    setLocationId(allLocations.find(x => x.code === 'MAIN')?.id ?? allLocations[0]?.id ?? '');
    setGrnDate(new Date().toISOString().slice(0, 10));
    setReferenceNo(''); setNotes('');
    setCurrencyCode('LKR');
    setPaymentTermsDays(''); setPaymentMethod('');
    setHeaderDiscountAmount('0'); setHeaderDiscountPercent('0'); setOtherCharges('0');
    setLines([emptyLine()]);
    setError(null);
  }

  async function submit(thenSubmitForApproval: boolean) {
    const err = validate();
    if (err) {
      setError(err);
      return;
    }
    setError(null);
    setSaving(true);
    try {
      const payload = {
        locationId,
        supplierId,
        purchaseOrderId: mode === 'po' ? poId : null,
        supplierInvoiceNo: supplierInvoiceNo || null,
        receivedDate: grnDate || null,
        currencyCode,
        paymentTermsDays: paymentTermsDays ? Number(paymentTermsDays) : null,
        paymentMethod: paymentMethod || null,
        discountAmount: Number(headerDiscountAmount) || 0,
        otherCharges: Number(otherCharges) || 0,
        referenceNo: referenceNo || null,
        notes: notes || null,
        lines: lines
          .filter(l => l.productId)
          .map(l => ({
            productId: l.productId,
            quantity: Number(l.quantity),
            unitCost: Number(l.unitCost),
            batchNo: l.batchNo || null,
            expiryDate: l.expiryDate || null,
            unitId: l.unitId || null,
            freeQuantity: Number(l.freeQuantity) || 0,
            discountAmount: lineDiscountOf(l),
          })),
      };
      const created = await apiClient<{ id: string; grnNumber?: string }>('/api/v1/grn', {
        method: 'POST',
        body: JSON.stringify(payload),
      });
      const qs = new URLSearchParams();
      if (created?.grnNumber) qs.set('justCreated', created.grnNumber);
      if (thenSubmitForApproval) {
        const submitted = await apiClient<{ status?: string }>(`/api/v1/grn/${created.id}/submit`, { method: 'POST' });
        if (submitted?.status) qs.set('status', submitted.status);
      } else {
        qs.set('status', 'draft');
      }
      router.push(qs.size ? `/grn?${qs}` : '/grn');
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return (
      <>
        <Topbar title="New GRN" subtitle="Create a goods received note" />
        <div className="p-6 space-y-5">
          {Array.from({ length: 3 }).map((_, section) => (
            <div key={section} className="card space-y-4 p-6">
              <div className="h-4 w-40 animate-pulse rounded bg-muted" />
              <div className="grid grid-cols-4 gap-3">
                {Array.from({ length: 4 }).map((_, i) => (
                  <div key={i} className="h-10 animate-pulse rounded-lg bg-muted" />
                ))}
              </div>
            </div>
          ))}
        </div>
      </>
    );
  }

  return (
    <>
      <Topbar title="New GRN" subtitle="Create a goods received note" />

      <div className="p-6 pb-28">
        <div className="sticky top-0 z-30 -mx-6 mb-6 border-b border-border/70 bg-background/85 px-6 py-4 backdrop-blur supports-[backdrop-filter]:bg-background/70">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <button
                onClick={() => router.push('/grn')}
                className="mb-1 flex items-center gap-1 text-sm font-medium text-muted-foreground transition hover:text-foreground"
              >
                <ArrowLeft className="size-4" />
                Back to GRNs
              </button>
              <h2 className="font-heading text-xl font-bold">Create Goods Received Note</h2>
              <p className="text-sm text-muted-foreground">
                Save as a draft to keep editing, or submit it straight away — submitting is what adds stock and updates weighted-average cost.
              </p>
            </div>

            <div className="flex items-center gap-3">
              <button
                onClick={resetForm}
                className="rounded-lg border border-border bg-card px-4 py-2.5 text-sm font-semibold transition hover:bg-muted"
              >
                Clear
              </button>
              <button
                disabled={saving}
                onClick={() => submit(false)}
                className="flex items-center gap-1.5 rounded-lg border border-border bg-card px-5 py-2.5 text-sm font-bold shadow-sm transition hover:bg-muted disabled:opacity-50"
              >
                <Save className="size-4" />
                {saving ? 'Saving…' : 'Save as Draft'}
              </button>
              <button
                disabled={saving}
                onClick={() => submit(true)}
                className="flex items-center gap-1.5 rounded-lg bg-primary px-5 py-2.5 text-sm font-bold text-primary-foreground shadow-sm transition hover:bg-primary-dark hover:shadow disabled:opacity-50"
              >
                <Send className="size-4" />
                {saving ? 'Submitting…' : 'Submit'}
              </button>
            </div>
          </div>
        </div>

        {error && (
          <div className="mb-5 rounded-lg bg-red-50 px-4 py-3 text-sm text-status-error">{error}</div>
        )}

        <div className="grid grid-cols-12 gap-5">
          <SectionCard className="col-span-12" icon={<ClipboardList className="size-4" />} title="GRN Details">
            <div className="grid grid-cols-4 gap-3">
              <Field label="Mode">
                <div className="flex h-10 items-center gap-4">
                  <label className="flex items-center gap-1.5 text-sm">
                    <input
                      type="radio"
                      checked={mode === 'manual'}
                      onChange={() => {
                        setMode('manual');
                        setPoId('');
                        setLines([emptyLine()]);
                      }}
                    />
                    Manual
                  </label>
                  <label className="flex items-center gap-1.5 text-sm">
                    <input
                      type="radio"
                      checked={mode === 'po'}
                      onChange={() => {
                        setMode('po');
                        setLines([emptyLine()]);
                      }}
                    />
                    PO Based
                  </label>
                </div>
              </Field>
              <Field label="Supplier">
                <SearchableSelect
                  value={supplierId}
                  selectedLabel={supplierLabel}
                  placeholder="Search suppliers…"
                  fetchPage={fetchSupplierOptions}
                  onChange={(id, option) => {
                    setSupplierId(id);
                    setSupplierLabel(option?.label ?? '');
                    const data = option?.data as
                      | { paymentTermsDays?: number; isVatRegistered?: boolean }
                      | undefined;
                    setSupplierVatRegistered(Boolean(data?.isVatRegistered));
                    if (!paymentTermsDays && data?.paymentTermsDays != null) {
                      setPaymentTermsDays(String(data.paymentTermsDays));
                    }
                    setPoId('');
                  }}
                />
              </Field>
              {mode === 'po' && (
                <Field label="PO #">
                  <SearchableSelect
                    value={poId}
                    onChange={id => setPoId(id)}
                    options={poOptions}
                    placeholder={supplierId ? 'Select a PO…' : 'Choose a supplier first'}
                    disabled={!supplierId}
                  />
                </Field>
              )}
              <Field label="Supplier invoice #">
                <input
                  value={supplierInvoiceNo}
                  onChange={e => setSupplierInvoiceNo(e.target.value)}
                  placeholder="Invoice / delivery note #"
                  className={INPUT}
                />
              </Field>
              <Field label="Location">
                <SearchableSelect
                  value={locationId}
                  selectedLabel={locationLabel(locationId)}
                  placeholder="Search locations…"
                  fetchPage={fetchLocationOptions}
                  onChange={id => setLocationId(id)}
                />
              </Field>
              <Field label="GRN date">
                <input
                  type="date"
                  value={grnDate}
                  onChange={e => setGrnDate(e.target.value)}
                  className={INPUT}
                />
              </Field>
              <Field label="Reference no.">
                <input
                  value={referenceNo}
                  onChange={e => setReferenceNo(e.target.value)}
                  placeholder="Delivery note / ref #"
                  className={INPUT}
                />
              </Field>
              <Field label="Remark" className="col-span-2">
                <input
                  value={notes}
                  onChange={e => setNotes(e.target.value)}
                  placeholder="Internal remark"
                  className={INPUT}
                />
              </Field>
            </div>
          </SectionCard>

          <SectionCard className="col-span-12" icon={<Wallet className="size-4" />} title="Currency & Payment">
            <div className="grid grid-cols-3 gap-3">
              <Field label="Currency">
                <select value={currencyCode} onChange={e => setCurrencyCode(e.target.value)} className={INPUT}>
                  {CURRENCIES.map(c => (
                    <option key={c} value={c}>{c}</option>
                  ))}
                </select>
              </Field>
              <Field label="Payment method">
                <select value={paymentMethod} onChange={e => setPaymentMethod(e.target.value)} className={INPUT}>
                  {PAYMENT_METHODS.map(m => (
                    <option key={m.value} value={m.value}>{m.label}</option>
                  ))}
                </select>
              </Field>
              <Field label="Payment term (days)">
                <input
                  type="number" min="0"
                  value={paymentTermsDays}
                  onChange={e => setPaymentTermsDays(e.target.value)}
                  placeholder="Defaults from supplier"
                  className={INPUT_RIGHT}
                />
              </Field>
            </div>
          </SectionCard>

          <SectionCard
            className="col-span-12"
            icon={<Receipt className="size-4" />}
            title="GRN Items"
            count={lines.filter(l => l.productId).length}
            action={
              mode === 'manual' && (
                <button
                  onClick={addLine}
                  className="inline-flex items-center gap-1 text-xs font-medium text-primary hover:text-primary-dark"
                >
                  <Plus className="size-3.5" /> Add line
                </button>
              )
            }
          >
            {poLoading ? (
              <div className="space-y-2">
                {Array.from({ length: 3 }).map((_, i) => (
                  <div key={i} className="h-9 animate-pulse rounded bg-muted" />
                ))}
              </div>
            ) : (
              <div className="overflow-visible rounded-lg border border-border">
                <table className="w-full text-sm">
                  <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                    <tr>
                      <th className="px-3 py-2 font-medium">Item</th>
                      {mode === 'po' && <th className="w-20 px-3 py-2 text-right font-medium">PO Qty</th>}
                      <th className="w-24 px-3 py-2 font-medium">Unit</th>
                      <th className="w-24 px-3 py-2 text-right font-medium">GRN Qty</th>
                      <th className="w-20 px-3 py-2 text-right font-medium">Free Qty</th>
                      <th className="w-36 px-3 py-2 text-right font-medium">Discount</th>
                      <th className="w-28 px-3 py-2 text-right font-medium">Cost Price</th>
                      <th className="w-28 px-3 py-2 text-right font-medium">Cost Value</th>
                      <th className="w-24 px-3 py-2 text-right font-medium">Tax</th>
                      <th className="w-32 px-3 py-2 font-medium">Batch No</th>
                      <th className="w-36 px-3 py-2 font-medium">Expiry Date</th>
                      {mode === 'manual' && <th className="w-10 px-3 py-2" />}
                    </tr>
                  </thead>
                  <tbody>
                    {lines.map((l, idx) => {
                      const costValue = lineCostValueOf(l);
                      const tax = lineTaxOf(l);
                      return (
                        <tr key={idx} className="border-b border-border last:border-0">
                          <td className="px-3 py-2">
                            {mode === 'manual' ? (
                              <SearchableSelect
                                value={l.productId}
                                selectedLabel={l.productLabel}
                                placeholder="Search products…"
                                fetchPage={fetchProductOptions}
                                onChange={async (id, option) => {
                                  setLine(idx, { productId: id, productLabel: option?.label ?? '' });
                                  const meta = await fetchProductMeta(id).catch(
                                    (): ProductMeta => ({
                                      taxable: false, isTaxInclusive: false,
                                      tracksExpiry: false, unitId: '', costPrice: 0,
                                    }),
                                  );
                                  setLine(idx, {
                                    unitId: meta.unitId,
                                    unitCost: l.unitCost || String(exTaxCost(meta.costPrice, meta, locationTax)),
                                    taxable: meta.taxable,
                                    tracksExpiry: meta.tracksExpiry,
                                  });
                                }}
                              />
                            ) : (
                              <span className="font-medium">{l.productLabel}</span>
                            )}
                          </td>
                          {mode === 'po' && (
                            <td className="px-3 py-2 text-right tabular-nums text-muted-foreground">
                              {l.poQty ?? '—'}
                            </td>
                          )}
                          <td className="px-3 py-2">
                            <select
                              value={l.unitId}
                              onChange={e => setLine(idx, { unitId: e.target.value })}
                              className={INPUT}
                            >
                              <option value="">Select unit…</option>
                              {allUoms.map(u => (
                                <option key={u.id} value={u.id}>{u.symbol || u.code}</option>
                              ))}
                            </select>
                          </td>
                          <td className="px-3 py-2">
                            <input
                              type="number" min="0" step="any"
                              value={l.quantity}
                              onChange={e => setLine(idx, { quantity: e.target.value })}
                              placeholder="0"
                              className={INPUT_RIGHT}
                            />
                          </td>
                          <td className="px-3 py-2">
                            <input
                              type="number" min="0" step="any"
                              value={l.freeQuantity}
                              onChange={e => setLine(idx, { freeQuantity: e.target.value })}
                              placeholder="0"
                              className={INPUT_RIGHT}
                            />
                          </td>
                          <td className="px-3 py-2">
                            <div className="flex items-center gap-1.5">
                              <select
                                value={l.discountMode}
                                onChange={e =>
                                  setLine(idx, { discountMode: e.target.value as 'amount' | 'percent' })
                                }
                                className="rounded-lg border border-border bg-card px-1.5 py-2 text-xs"
                              >
                                <option value="amount">Amt</option>
                                <option value="percent">%</option>
                              </select>
                              <input
                                type="number" min="0" step="any"
                                value={l.discountValue}
                                onChange={e => setLine(idx, { discountValue: e.target.value })}
                                placeholder="0"
                                className={INPUT_RIGHT}
                              />
                            </div>
                          </td>
                          <td className="px-3 py-2">
                            <input
                              type="number" min="0" step="any"
                              value={l.unitCost}
                              onChange={e => setLine(idx, { unitCost: e.target.value })}
                              placeholder="0.00"
                              className={INPUT_RIGHT}
                            />
                          </td>
                          <td className="px-3 py-2 text-right font-medium tabular-nums">{lkr(costValue)}</td>
                          <td
                            className="px-3 py-2 text-right tabular-nums text-muted-foreground"
                            title={l.taxable && locationTax.rate > 0 ? `${locationTax.rate}%` : undefined}
                          >
                            {lkr(tax)}
                          </td>
                          <td className="px-3 py-2">
                            <input
                              value={l.batchNo}
                              onChange={e => setLine(idx, { batchNo: e.target.value })}
                              placeholder="Batch #"
                              className={INPUT}
                            />
                          </td>
                          <td className="px-3 py-2">
                            <input
                              type="date"
                              value={l.expiryDate}
                              onChange={e => setLine(idx, { expiryDate: e.target.value })}
                              disabled={!!l.productId && !l.tracksExpiry}
                              className={INPUT}
                            />
                          </td>
                          {mode === 'manual' && (
                            <td className="px-3 py-2 text-right">
                              <button
                                onClick={() => removeLine(idx)}
                                disabled={lines.length === 1}
                                className="rounded p-1 text-muted-foreground hover:bg-muted disabled:opacity-30"
                                title="Remove line"
                              >
                                <Trash2 className="size-4" />
                              </button>
                            </td>
                          )}
                        </tr>
                      );
                    })}
                    {lines.length === 0 && (
                      <tr>
                        <td colSpan={11} className="px-3 py-8 text-center text-muted-foreground">
                          {mode === 'po' ? 'Choose a PO with lines to receive.' : 'Add a line to get started.'}
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            )}

            <div className="mt-4 grid grid-cols-3 gap-3">
              <Field label="Total cost value">
                <input readOnly value={lkr(totalCostValue)} className={`${INPUT_RIGHT} bg-muted/40`} />
              </Field>
              <Field label="Total discounts">
                <input readOnly value={lkr(totalDiscounts)} className={`${INPUT_RIGHT} bg-muted/40`} />
              </Field>
              <Field label="Total taxes">
                <input readOnly value={lkr(totalTaxes)} className={`${INPUT_RIGHT} bg-muted/40`} />
              </Field>
            </div>
          </SectionCard>

          <SectionCard className="col-span-12" icon={<Wallet className="size-4" />} title="Charges & Totals">
            <div className="grid grid-cols-4 gap-3">
              <Field label="Gross amount">
                <input readOnly value={lkr(grossAmount)} className={`${INPUT_RIGHT} bg-muted/40`} />
              </Field>
              <Field label="Discount amount">
                <input
                  type="number" min="0" step="any"
                  value={headerDiscountAmount}
                  onChange={e => syncHeaderDiscountFromAmount(e.target.value)}
                  className={INPUT_RIGHT}
                />
              </Field>
              <Field label="Discount percentage">
                <input
                  type="number" min="0" step="any"
                  value={headerDiscountPercent}
                  onChange={e => syncHeaderDiscountFromPercent(e.target.value)}
                  className={INPUT_RIGHT}
                />
              </Field>
              <Field label="Other charges">
                <input
                  type="number" min="0" step="any"
                  value={otherCharges}
                  onChange={e => setOtherCharges(e.target.value)}
                  className={INPUT_RIGHT}
                />
              </Field>
              <Field label="Total tax amount">
                <input readOnly value={lkr(totalTaxes)} className={`${INPUT_RIGHT} bg-muted/40`} />
              </Field>
              <Field label="Net amount" className="col-span-2">
                <input readOnly value={lkr(netAmount)} className={`${INPUT_RIGHT} bg-muted/40 font-bold`} />
              </Field>
            </div>
          </SectionCard>
        </div>
      </div>
    </>
  );
}

function SectionCard({
  icon,
  title,
  count,
  action,
  children,
  className = '',
}: {
  icon: React.ReactNode;
  title: string;
  count?: number;
  action?: React.ReactNode;
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <section className={`card overflow-visible p-0 ${className}`}>
      <div className="flex items-center justify-between gap-3 border-b border-border/70 bg-muted/20 px-5 py-3.5">
        <div className="flex items-center gap-2.5">
          <span className="flex size-8 items-center justify-center rounded-lg bg-primary/10 text-primary">
            {icon}
          </span>
          <div className="flex items-center gap-2">
            <h3 className="font-heading text-base font-bold leading-none">{title}</h3>
            {typeof count === 'number' && (
              <span className="rounded-full bg-muted px-2 py-0.5 text-xs font-semibold text-muted-foreground">
                {count}
              </span>
            )}
          </div>
        </div>
        {action}
      </div>
      <div className="p-5">{children}</div>
    </section>
  );
}

function Field({
  label,
  children,
  className = '',
}: {
  label: string;
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <div className={className}>
      <label className="mb-1 block text-sm font-semibold text-slate-700">{label}</label>
      {children}
    </div>
  );
}
