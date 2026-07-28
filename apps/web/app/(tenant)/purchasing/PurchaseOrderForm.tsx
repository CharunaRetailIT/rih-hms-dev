"use client";

import { useEffect, useMemo, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import {
  ArrowLeft,
  ClipboardList,
  Plus,
  Receipt,
  Save,
  Send,
  Trash2,
  Truck,
  Wallet,
} from "lucide-react";
import { Topbar } from "@/components/app-shell/Topbar";
import { apiClient, lkr } from "@/lib/api-client";
import { SearchableSelect } from "@/components/ui/SearchableSelect";

type Location = { id: string; code: string; name: string; isActive: boolean };
type LocationTax = { rate: number; flat: number };
type ProductMeta = { taxable: boolean; costPrice: number; isTaxInclusive: boolean };

type PoLine = {
  productId: string;
  sku: string;
  productName: string;
  quantityOrdered: number;
  unitCost: number;
  discountAmount: number;
  lineTotal: number;
  taxRate: number;
  taxAmount: number;
};
type PoDetail = {
  id: string;
  poNumber: string;
  supplierId: string;
  locationId: string;
  status: string;
  expectedDate: string | null;
  discountAmount: number;
  deductions: number;
  otherCharges: number;
  taxAmount: number;
  excludeTax: boolean;
  subtotalAmount: number;
  totalAmount: number;
  currencyCode: string;
  deliveryLocationId: string | null;
  deliveryAddress: string | null;
  referenceNo: string | null;
  notes: string | null;
  paymentTermsDays: number | null;
  paymentMethod: string | null;
  lines: PoLine[];
};

type DraftLine = {
  productId: string;
  productLabel: string;
  quantity: string;
  unitCost: string;
  discountAmount: string;
  taxable: boolean;
};

type RequestNoteDetail = {
  id: string;
  requestNumber: string;
  mode: string;
  fromLocationId: string;
  toLocationId: string;
  status: string;
  lines: { productId: string; sku: string; productName: string; quantity: number }[];
};

type PagedApiResult<T> = { data: T[]; pagination: { totalPages: number } };

async function fetchPage<T>(
  url: string,
  {
    page,
    pageSize,
    search,
  }: { page: number; pageSize: number; search: string },
): Promise<PagedApiResult<T>> {
  const qs = new URLSearchParams({
    pageNumber: String(page),
    pageSize: String(pageSize),
    isActive: "true",
  });
  if (search) qs.set("search", search);
  return apiClient<PagedApiResult<T>>(`${url}?${qs}`);
}

async function fetchSupplierOptions(args: {
  page: number;
  pageSize: number;
  search: string;
}) {
  const res = await fetchPage<{
    id: string;
    name: string;
    paymentTermsDays: number;
    isVatRegistered: boolean;
  }>("/api/v1/suppliers/paged", args);
  return {
    items: res.data.map((s) => ({
      id: s.id,
      label: s.name,
      data: {
        paymentTermsDays: s.paymentTermsDays,
        isVatRegistered: s.isVatRegistered,
      },
    })),
    hasMore: args.page < res.pagination.totalPages,
  };
}

async function fetchLocationOptions(args: {
  page: number;
  pageSize: number;
  search: string;
}) {
  const res = await fetchPage<{ id: string; code: string; name: string }>(
    "/api/v1/locations/paged",
    args,
  );
  return {
    items: res.data.map((l) => ({ id: l.id, label: `${l.code} — ${l.name}` })),
    hasMore: args.page < res.pagination.totalPages,
  };
}

async function fetchProductOptions(args: {
  page: number;
  pageSize: number;
  search: string;
}) {
  const res = await fetchPage<{
    id: string;
    sku: string;
    name: string;
    costPrice: number;
  }>("/api/v1/products/paged", args);
  return {
    items: res.data.map((p) => ({
      id: p.id,
      label: `${p.name} (${p.sku})`,
      data: { costPrice: p.costPrice, sku: p.sku, name: p.name },
    })),
    hasMore: args.page < res.pagination.totalPages,
  };
}

// Tax is header-level now (the PO's location's tax-type Charges, see fetchLocationTax
// below) — a product only decides whether it's taxable at all (isTaxable + standard
// tax class) and whether its listed cost already includes that tax.
async function fetchProductMeta(productId: string): Promise<ProductMeta> {
  const detail = await apiClient<{
    costPrice: number;
    isTaxable: boolean;
    taxClass: string;
    isTaxInclusive: boolean;
  }>(`/api/v1/products/${productId}`);
  return {
    taxable: detail.isTaxable && detail.taxClass === "standard",
    costPrice: detail.costPrice ?? 0,
    isTaxInclusive: Boolean(detail.isTaxInclusive),
  };
}

async function fetchLocationTax(locationId: string): Promise<LocationTax> {
  if (!locationId) return { rate: 0, flat: 0 };
  return apiClient<LocationTax>(
    `/api/v1/purchase-orders/tax-rate?locationId=${locationId}`,
  );
}

// Product's listed cost price already includes tax — back the rate out so the
// cost price shown/entered on the line is ex-tax, matching what the header tax
// gets computed off of.
function exTaxCost(costPrice: number, meta: ProductMeta, tax: LocationTax): number {
  if (!meta.isTaxInclusive || tax.rate <= 0) return costPrice;
  return Math.round((costPrice / (1 + tax.rate / 100)) * 10000) / 10000;
}

const INPUT =
  "w-full rounded-lg border border-border bg-card px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary disabled:opacity-60";
const INPUT_RIGHT = `${INPUT} text-right tabular-nums`;

const PAYMENT_METHODS = [
  { value: "", label: "Not set" },
  { value: "cash", label: "Cash" },
  { value: "bank_transfer", label: "Bank transfer" },
  { value: "cheque", label: "Cheque" },
  { value: "credit", label: "Credit" },
];

const CURRENCIES = ["LKR", "USD", "EUR", "GBP", "INR", "AUD", "AED", "SGD", "MVR"];

function todayIso(): string {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
}

const emptyLine = (): DraftLine => ({
  productId: "",
  productLabel: "",
  quantity: "",
  unitCost: "",
  discountAmount: "",
  taxable: false,
});

export default function PurchaseOrderForm({ poId }: { poId?: string }) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const isEdit = Boolean(poId);
  const requestNoteIdsParam = searchParams.get("requestNoteIds");

  const [bundledNotes, setBundledNotes] = useState<RequestNoteDetail[]>([]);
  const [bundleError, setBundleError] = useState<string | null>(null);
  const [bundleLoading, setBundleLoading] = useState(Boolean(requestNoteIdsParam));

  // Locations are a small, bounded list (a handful of outlets/warehouses) —
  // safe to preload in full, used both to default the PO location on create
  // and to resolve the selected label without waiting for the dropdown to open.
  const [allLocations, setAllLocations] = useState<Location[]>([]);
  const [supplierLabel, setSupplierLabel] = useState("");
  const [supplierVatRegistered, setSupplierVatRegistered] = useState(false);
  // The PO's location determines which tax-type Charges apply — a single shared
  // rate/flat for every line, refetched whenever the location changes.
  const [locationTax, setLocationTax] = useState<LocationTax>({ rate: 0, flat: 0 });

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [status, setStatus] = useState("draft");
  const [poNumber, setPoNumber] = useState("");

  const [supplierId, setSupplierId] = useState("");
  const [locationId, setLocationId] = useState("");
  const [deliveryLocationId, setDeliveryLocationId] = useState("");
  const [deliveryAddress, setDeliveryAddress] = useState("");
  const [expectedDate, setExpectedDate] = useState("");
  const [referenceNo, setReferenceNo] = useState("");
  const [notes, setNotes] = useState("");
  const [currencyCode, setCurrencyCode] = useState("LKR");
  const [paymentTermsDays, setPaymentTermsDays] = useState("");
  const [paymentMethod, setPaymentMethod] = useState("");
  const [excludeTax, setExcludeTax] = useState(false);
  const [discountAmount, setDiscountAmount] = useState("0");
  const [deductions, setDeductions] = useState("0");
  const [otherCharges, setOtherCharges] = useState("0");
  const [lines, setLines] = useState<DraftLine[]>([emptyLine()]);

  useEffect(() => {
    (async () => {
      try {
        // `all=true` so a location assigned before it was deactivated still
        // resolves a label here — the pick-list itself (SearchableSelect,
        // fetchLocationOptions) is separately restricted to active locations.
        const locations = await apiClient<Location[]>("/api/v1/locations?all=true");
        setAllLocations(locations);

        if (poId) {
          const po = await apiClient<PoDetail>(
            `/api/v1/purchase-orders/${poId}`,
          );

          apiClient<{ name: string; isVatRegistered: boolean }>(
            `/api/v1/suppliers/${po.supplierId}`,
          )
            .then((s) => {
              setSupplierLabel(s.name);
              setSupplierVatRegistered(s.isVatRegistered);
            })
            .catch(() => {});
          setPoNumber(po.poNumber);
          setStatus(po.status);
          setSupplierId(po.supplierId);
          setLocationId(po.locationId);
          setDeliveryLocationId(po.deliveryLocationId ?? "");
          setDeliveryAddress(po.deliveryAddress ?? "");
          setExpectedDate(po.expectedDate ? po.expectedDate.slice(0, 10) : "");
          setReferenceNo(po.referenceNo ?? "");
          setNotes(po.notes ?? "");
          setCurrencyCode(po.currencyCode || "LKR");
          setPaymentTermsDays(
            po.paymentTermsDays != null ? String(po.paymentTermsDays) : "",
          );
          setPaymentMethod(po.paymentMethod ?? "");
          setExcludeTax(Boolean(po.excludeTax));
          setDiscountAmount(String(po.discountAmount ?? 0));
          setDeductions(String(po.deductions ?? 0));
          setOtherCharges(String(po.otherCharges ?? 0));

          // Whether a line was actually taxed when last saved — the rate itself now
          // comes live from the PO's location (see the locationTax effect below).
          const linesWithTax: DraftLine[] = po.lines.map((l) => ({
            productId: l.productId,
            productLabel: `${l.productName} (${l.sku})`,
            quantity: String(l.quantityOrdered),
            unitCost: String(l.unitCost),
            discountAmount: String(l.discountAmount || ""),
            taxable: l.taxAmount > 0 || l.taxRate > 0,
          }));
          setLines(linesWithTax.length ? linesWithTax : [emptyLine()]);
        } else {
          const activeLocations = locations.filter((x) => x.isActive);
          setLocationId(
            activeLocations.find((x) => x.code === "MAIN")?.id ??
              activeLocations[0]?.id ??
              "",
          );
        }
      } catch (e) {
        setError((e as Error).message);
      } finally {
        setLoading(false);
      }
    })();
  }, [poId]);

  // Tax is sourced from the PO's location, not per-product — refetch the combined
  // rate/flat whenever the location changes (including the initial default above).
  useEffect(() => {
    if (!locationId) { setLocationTax({ rate: 0, flat: 0 }); return; }
    fetchLocationTax(locationId).then(setLocationTax).catch(() => setLocationTax({ rate: 0, flat: 0 }));
  }, [locationId]);

  // Bundling: when arriving from the Request Notes list with one or more selected
  // "po"-mode notes, pull them in, merge same-product lines, and pin the PO's
  // location to what they share — request-note fulfillment is set server-side
  // when the PO is created, so this only needs to run once, on create.
  useEffect(() => {
    if (isEdit || !requestNoteIdsParam || loading) return;
    const ids = requestNoteIdsParam.split(",").filter(Boolean);
    if (ids.length === 0) return;
    (async () => {
      setBundleLoading(true);
      setBundleError(null);
      try {
        const notes = await Promise.all(
          ids.map((id) => apiClient<RequestNoteDetail>(`/api/v1/request-notes/${id}`)),
        );
        const bad = notes.find((n) => n.mode !== "po" || n.status !== "approved");
        if (bad) throw new Error(`${bad.requestNumber} is not an approved PO-mode request note.`);
        const fromLocationId = notes[0].fromLocationId;
        if (notes.some((n) => n.fromLocationId !== fromLocationId))
          throw new Error("Selected request notes must share the same From location.");

        setBundledNotes(notes);
        setLocationId(fromLocationId);

        // If every bundled note is requesting the same outlet, ship straight there
        // instead of to the buying location — otherwise leave it unset (defaults to
        // the PO location) since there's no single destination to prefill.
        const toLocationId = notes[0].toLocationId;
        setDeliveryLocationId(
          notes.every((n) => n.toLocationId === toLocationId) && toLocationId !== fromLocationId
            ? toLocationId
            : "",
        );

        // Merge same-product lines across notes by summing quantity.
        const merged = new Map<string, { productId: string; label: string; quantity: number }>();
        for (const n of notes) {
          for (const l of n.lines) {
            const existing = merged.get(l.productId);
            if (existing) existing.quantity += l.quantity;
            else merged.set(l.productId, { productId: l.productId, label: `${l.productName} (${l.sku})`, quantity: l.quantity });
          }
        }
        // Resolve the bundled location's tax rate directly rather than waiting on the
        // locationTax effect (which won't have fired yet for this freshly-set location).
        const bundleTax = await fetchLocationTax(fromLocationId).catch((): LocationTax => ({ rate: 0, flat: 0 }));
        const mergedLines = await Promise.all(
          [...merged.values()].map(async (m) => {
            const meta = await fetchProductMeta(m.productId).catch(
              (): ProductMeta => ({ taxable: false, costPrice: 0, isTaxInclusive: false }),
            );
            const line: DraftLine = {
              productId: m.productId,
              productLabel: m.label,
              quantity: String(m.quantity),
              unitCost: String(exTaxCost(meta.costPrice, meta, bundleTax)),
              discountAmount: "",
              taxable: meta.taxable,
            };
            return line;
          }),
        );
        setLines(mergedLines.length ? mergedLines : [emptyLine()]);
      } catch (e) {
        setBundleError((e as Error).message);
      } finally {
        setBundleLoading(false);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isEdit, requestNoteIdsParam, loading]);

  const locationLabel = (id: string) => {
    const loc = allLocations.find((l) => l.id === id);
    return loc ? `${loc.code} — ${loc.name}` : "";
  };

  const setLine = (idx: number, patch: Partial<DraftLine>) =>
    setLines((prev) =>
      prev.map((l, i) => (i === idx ? { ...l, ...patch } : l)),
    );

  const addLine = () => setLines((prev) => [...prev, emptyLine()]);

  const removeLine = (idx: number) =>
    setLines((prev) =>
      prev.length === 1 ? prev : prev.filter((_, i) => i !== idx),
    );

  function lineSubtotalOf(l: DraftLine): number {
    const q = Number(l.quantity) || 0;
    const c = Number(l.unitCost) || 0;
    const d = Number(l.discountAmount) || 0;
    return Math.max(0, q * c - d);
  }

  function lineTaxOf(l: DraftLine): number {
    if (excludeTax || !supplierVatRegistered || !l.taxable) return 0;
    const sub = lineSubtotalOf(l);
    return Math.round(sub * (locationTax.rate / 100) * 10000) / 10000;
  }

  const subtotal = useMemo(
    () => lines.reduce((sum, l) => sum + lineSubtotalOf(l), 0),
    [lines],
  );
  // Flat tax-type charges (a fixed levy, say) apply once for the whole document,
  // not per line — added on top of the summed per-line percentage tax.
  const taxTotal = useMemo(() => {
    const perLine = lines.reduce((sum, l) => sum + lineTaxOf(l), 0);
    const anyTaxed = !excludeTax && supplierVatRegistered && lines.some((l) => l.taxable);
    return perLine + (anyTaxed ? locationTax.flat : 0);
  }, [lines, excludeTax, supplierVatRegistered, locationTax]);
  const total = useMemo(
    () =>
      subtotal -
      (Number(discountAmount) || 0) -
      (Number(deductions) || 0) +
      (Number(otherCharges) || 0) +
      taxTotal,
    [subtotal, discountAmount, deductions, otherCharges, taxTotal],
  );

  function validate(): string | null {
    if (!supplierId) return "Choose a supplier.";
    if (!locationId) return "Choose a PO location.";
    if (expectedDate && expectedDate < todayIso())
      return "Expected delivery date can't be in the past.";
    const filled = lines.filter((l) => l.productId);
    if (filled.length === 0) return "Add at least one line.";
    for (const l of filled) {
      const q = Number(l.quantity);
      if (!Number.isFinite(q) || q <= 0)
        return "Each line needs a quantity greater than zero.";
      const c = Number(l.unitCost);
      if (!Number.isFinite(c) || c < 0) return "Unit cost cannot be negative.";
    }
    return null;
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
        expectedDate: expectedDate || null,
        notes: notes || null,
        discountAmount: Number(discountAmount) || 0,
        deductions: Number(deductions) || 0,
        otherCharges: Number(otherCharges) || 0,
        excludeTax,
        currencyCode,
        deliveryLocationId: deliveryLocationId || null,
        deliveryAddress: deliveryAddress || null,
        referenceNo: referenceNo || null,
        paymentTermsDays: paymentTermsDays ? Number(paymentTermsDays) : null,
        paymentMethod: paymentMethod || null,
        lines: lines
          .filter((l) => l.productId)
          .map((l) => ({
            productId: l.productId,
            quantity: Number(l.quantity),
            unitCost: Number(l.unitCost),
            discountAmount: Number(l.discountAmount) || 0,
          })),
        requestNoteIds: !isEdit && bundledNotes.length ? bundledNotes.map((n) => n.id) : undefined,
      };
      if (isEdit) {
        await apiClient(`/api/v1/purchase-orders/${poId}`, {
          method: "PUT",
          body: JSON.stringify(payload),
        });
      } else {
        const created = await apiClient<{ id: string }>("/api/v1/purchase-orders", {
          method: "POST",
          body: JSON.stringify(payload),
        });
        if (thenSubmitForApproval) {
          await apiClient(`/api/v1/purchase-orders/${created.id}/submit`, { method: "POST" });
        }
      }
      router.push("/purchasing");
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return (
      <>
        <Topbar
          title={isEdit ? "Edit Purchase Order" : "New Purchase Order"}
          subtitle={isEdit ? "Loading order…" : "Create a purchase order"}
        />
        <div className="p-6 space-y-5">
          {Array.from({ length: 3 }).map((_, section) => (
            <div key={section} className="card space-y-4 p-6">
              <div className="h-4 w-40 animate-pulse rounded bg-muted" />
              <div className="grid grid-cols-4 gap-3">
                {Array.from({ length: 4 }).map((_, i) => (
                  <div
                    key={i}
                    className="h-10 animate-pulse rounded-lg bg-muted"
                  />
                ))}
              </div>
            </div>
          ))}
        </div>
      </>
    );
  }

  const locked = isEdit && status !== "draft";

  return (
    <>
      <Topbar
        title={isEdit ? "Edit Purchase Order" : "New Purchase Order"}
        subtitle={isEdit ? `Editing ${poNumber}` : "Create a purchase order"}
      />

      <div className="p-6 pb-28">
        <div className="sticky top-0 z-30 -mx-6 mb-6 border-b border-border/70 bg-background/85 px-6 py-4 backdrop-blur supports-[backdrop-filter]:bg-background/70">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <button
                onClick={() => router.push("/purchasing")}
                className="mb-1 flex items-center gap-1 text-sm font-medium text-muted-foreground transition hover:text-foreground"
              >
                <ArrowLeft className="size-4" />
                Back to purchase orders
              </button>
              <h2 className="font-heading text-xl font-bold">
                {isEdit ? `Edit ${poNumber}` : "Create Purchase Order"}
              </h2>
              <p className="text-sm text-muted-foreground">
                {locked
                  ? `This order is ${status} and can no longer be edited.`
                  : isEdit
                    ? "Header, delivery, payment terms and lines."
                    : "Save as a draft to keep editing, or submit it straight away."}
              </p>
            </div>

            <div className="flex items-center gap-3">
              <label className="flex items-center gap-2 rounded-lg border border-border bg-card px-3 py-2 text-sm font-medium">
                <input
                  type="checkbox"
                  checked={excludeTax}
                  disabled={locked}
                  onChange={(e) => setExcludeTax(e.target.checked)}
                  className="size-4"
                />
                Exclude tax for this order
              </label>
              <button
                onClick={() => router.push("/purchasing")}
                className="rounded-lg border border-border bg-card px-4 py-2.5 text-sm font-semibold transition hover:bg-muted"
              >
                Cancel
              </button>
              <button
                disabled={saving || locked}
                onClick={() => submit(false)}
                className="flex items-center gap-1.5 rounded-lg border border-border bg-card px-5 py-2.5 text-sm font-bold shadow-sm transition hover:bg-muted disabled:opacity-50"
              >
                <Save className="size-4" />
                {saving ? "Saving…" : isEdit ? "Save Changes" : "Save as Draft"}
              </button>
              {!isEdit && (
                <button
                  disabled={saving || locked}
                  onClick={() => submit(true)}
                  className="flex items-center gap-1.5 rounded-lg bg-primary px-5 py-2.5 text-sm font-bold text-primary-foreground shadow-sm transition hover:bg-primary-dark hover:shadow disabled:opacity-50"
                >
                  <Send className="size-4" />
                  {saving ? "Submitting…" : "Submit"}
                </button>
              )}
            </div>
          </div>
        </div>

        {error && (
          <div className="mb-5 rounded-lg bg-red-50 px-4 py-3 text-sm text-status-error">
            {error}
          </div>
        )}

        {bundleLoading && (
          <div className="mb-5 rounded-lg bg-blue-50 px-4 py-3 text-sm text-blue-800">
            Loading the selected request notes…
          </div>
        )}
        {bundleError && (
          <div className="mb-5 rounded-lg bg-red-50 px-4 py-3 text-sm text-status-error">
            {bundleError}
          </div>
        )}
        {!bundleLoading && bundledNotes.length > 0 && (
          <div className="mb-5 rounded-lg bg-blue-50 px-4 py-3 text-sm text-blue-800">
            Bundling {bundledNotes.length} request note{bundledNotes.length === 1 ? "" : "s"}:{" "}
            {bundledNotes.map((n) => n.requestNumber).join(", ")}. They&apos;ll be marked fulfilled once this order is created.
          </div>
        )}

        <div className="grid grid-cols-12 gap-5">
          <SectionCard
            className="col-span-12"
            icon={<ClipboardList className="size-4" />}
            title="Order Details"
          >
            <div className="grid grid-cols-4 gap-3">
              <Field label="Supplier">
                <SearchableSelect
                  value={supplierId}
                  selectedLabel={supplierLabel}
                  disabled={locked}
                  placeholder="Search suppliers…"
                  fetchPage={fetchSupplierOptions}
                  onChange={(id, option) => {
                    setSupplierId(id);
                    setSupplierLabel(option?.label ?? "");
                    const data = option?.data as
                      | { paymentTermsDays?: number; isVatRegistered?: boolean }
                      | undefined;
                    setSupplierVatRegistered(Boolean(data?.isVatRegistered));
                    if (!paymentTermsDays && data?.paymentTermsDays != null) {
                      setPaymentTermsDays(String(data.paymentTermsDays));
                    }
                  }}
                />
              </Field>
              <Field label="PO location">
                <SearchableSelect
                  value={locationId}
                  selectedLabel={locationLabel(locationId)}
                  disabled={locked}
                  placeholder="Search locations…"
                  fetchPage={fetchLocationOptions}
                  onChange={(id) => {
                    setLocationId(id);
                    // Changing away from the bundled notes' shared location breaks the
                    // link they need — drop back to a plain manual PO instead of erroring.
                    if (bundledNotes.length && id !== bundledNotes[0].fromLocationId) setBundledNotes([]);
                  }}
                />
              </Field>
              <Field label="Expected delivery date">
                <input
                  type="date"
                  value={expectedDate}
                  min={todayIso()}
                  disabled={locked}
                  onChange={(e) => setExpectedDate(e.target.value)}
                  className={INPUT}
                />
              </Field>
              <Field label="Reference no.">
                <input
                  value={referenceNo}
                  disabled={locked}
                  onChange={(e) => setReferenceNo(e.target.value)}
                  placeholder="Supplier quote / ref #"
                  className={INPUT}
                />
              </Field>
              <Field label="Currency">
                <select
                  value={currencyCode}
                  disabled={locked}
                  onChange={(e) => setCurrencyCode(e.target.value)}
                  className={INPUT}
                >
                  {CURRENCIES.map((c) => (
                    <option key={c} value={c}>
                      {c}
                    </option>
                  ))}
                </select>
              </Field>
              <Field label="Notes" className="col-span-2">
                <input
                  value={notes}
                  disabled={locked}
                  onChange={(e) => setNotes(e.target.value)}
                  placeholder="Internal remark"
                  className={INPUT}
                />
              </Field>
            </div>
          </SectionCard>

          <SectionCard
            className="col-span-6"
            icon={<Truck className="size-4" />}
            title="Delivery"
          >
            <div className="grid grid-cols-2 gap-3">
              <Field label="Delivery location" className="col-span-2">
                <SearchableSelect
                  value={deliveryLocationId}
                  selectedLabel={
                    deliveryLocationId ? locationLabel(deliveryLocationId) : ""
                  }
                  disabled={locked}
                  placeholder="Same as PO location"
                  fetchPage={fetchLocationOptions}
                  onChange={(id) => setDeliveryLocationId(id)}
                />
              </Field>
              <Field label="Delivery address" className="col-span-2">
                <input
                  value={deliveryAddress}
                  disabled={locked}
                  onChange={(e) => setDeliveryAddress(e.target.value)}
                  placeholder="Ship-to address, if different"
                  className={INPUT}
                />
              </Field>
            </div>
          </SectionCard>

          <SectionCard
            className="col-span-6"
            icon={<Wallet className="size-4" />}
            title="Payment"
          >
            <div className="grid grid-cols-2 gap-3">
              <Field label="Payment terms (days)">
                <input
                  type="number"
                  min="0"
                  value={paymentTermsDays}
                  disabled={locked}
                  onChange={(e) => setPaymentTermsDays(e.target.value)}
                  placeholder="Defaults from supplier"
                  className={INPUT_RIGHT}
                />
              </Field>
              <Field label="Payment method">
                <select
                  value={paymentMethod}
                  disabled={locked}
                  onChange={(e) => setPaymentMethod(e.target.value)}
                  className={INPUT}
                >
                  {PAYMENT_METHODS.map((m) => (
                    <option key={m.value} value={m.value}>
                      {m.label}
                    </option>
                  ))}
                </select>
              </Field>
            </div>
          </SectionCard>

          <SectionCard
            className="col-span-12"
            icon={<Receipt className="size-4" />}
            title="Lines"
            count={lines.filter((l) => l.productId).length}
            action={
              !locked && (
                <button
                  onClick={addLine}
                  className="inline-flex items-center gap-1 text-xs font-medium text-primary hover:text-primary-dark"
                >
                  <Plus className="size-3.5" /> Add line
                </button>
              )
            }
          >
            <div className="overflow-visible rounded-lg border border-border">
              <table className="w-full text-sm">
                <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                  <tr>
                    <th className="px-3 py-2 font-medium">Product</th>
                    <th className="w-24 px-3 py-2 text-right font-medium">
                      Qty
                    </th>
                    <th className="w-32 px-3 py-2 text-right font-medium">
                      Unit cost
                    </th>
                    <th className="w-28 px-3 py-2 text-right font-medium">
                      Discount
                    </th>
                    <th className="w-28 px-3 py-2 text-right font-medium">
                      Tax
                    </th>
                    <th className="w-28 px-3 py-2 text-right font-medium">
                      Line
                    </th>
                    <th className="w-10 px-3 py-2" />
                  </tr>
                </thead>
                <tbody>
                  {lines.map((l, idx) => {
                    const lineSub = lineSubtotalOf(l);
                    const lineTax = lineTaxOf(l);
                    return (
                      <tr
                        key={idx}
                        className="border-b border-border last:border-0"
                      >
                        <td className="px-3 py-2">
                          <SearchableSelect
                            value={l.productId}
                            selectedLabel={l.productLabel}
                            disabled={locked}
                            placeholder="Search products…"
                            fetchPage={fetchProductOptions}
                            onChange={async (id, option) => {
                              setLine(idx, {
                                productId: id,
                                productLabel: option?.label ?? "",
                              });
                              const meta = await fetchProductMeta(id).catch(
                                (): ProductMeta => ({
                                  taxable: false,
                                  costPrice: 0,
                                  isTaxInclusive: false,
                                }),
                              );
                              setLine(idx, {
                                taxable: meta.taxable,
                                unitCost:
                                  l.unitCost ||
                                  String(exTaxCost(meta.costPrice, meta, locationTax)),
                              });
                            }}
                          />
                        </td>
                        <td className="px-3 py-2">
                          <input
                            type="number"
                            min="0"
                            step="any"
                            value={l.quantity}
                            disabled={locked}
                            onChange={(e) =>
                              setLine(idx, { quantity: e.target.value })
                            }
                            placeholder="0"
                            className={INPUT_RIGHT}
                          />
                        </td>
                        <td className="px-3 py-2">
                          <input
                            type="number"
                            min="0"
                            step="any"
                            value={l.unitCost}
                            disabled={locked}
                            onChange={(e) =>
                              setLine(idx, { unitCost: e.target.value })
                            }
                            placeholder="0.00"
                            className={INPUT_RIGHT}
                          />
                        </td>
                        <td className="px-3 py-2">
                          <input
                            type="number"
                            min="0"
                            step="any"
                            value={l.discountAmount}
                            disabled={locked}
                            onChange={(e) =>
                              setLine(idx, { discountAmount: e.target.value })
                            }
                            placeholder="0.00"
                            className={INPUT_RIGHT}
                          />
                        </td>
                        <td
                          className="px-3 py-2 text-right tabular-nums text-muted-foreground"
                          title={
                            l.taxable && locationTax.rate > 0
                              ? `${locationTax.rate}%`
                              : undefined
                          }
                        >
                          {lkr(lineTax)}
                        </td>
                        <td className="px-3 py-2 text-right font-medium tabular-nums">
                          {lkr(lineSub)}
                        </td>
                        <td className="px-3 py-2 text-right">
                          <button
                            onClick={() => removeLine(idx)}
                            disabled={locked || lines.length === 1}
                            className="rounded p-1 text-muted-foreground hover:bg-muted disabled:opacity-30"
                            title="Remove line"
                          >
                            <Trash2 className="size-4" />
                          </button>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </SectionCard>

          <SectionCard
            className="col-span-12"
            icon={<Wallet className="size-4" />}
            title="Charges & Totals"
          >
            <div className="grid grid-cols-4 gap-3">
              <Field label="Header discount">
                <input
                  type="number"
                  min="0"
                  step="any"
                  value={discountAmount}
                  disabled={locked}
                  onChange={(e) => setDiscountAmount(e.target.value)}
                  className={INPUT_RIGHT}
                />
              </Field>
              <Field label="Deductions">
                <input
                  type="number"
                  min="0"
                  step="any"
                  value={deductions}
                  disabled={locked}
                  onChange={(e) => setDeductions(e.target.value)}
                  className={INPUT_RIGHT}
                />
              </Field>
              <Field label="Other charges (freight/handling)">
                <input
                  type="number"
                  min="0"
                  step="any"
                  value={otherCharges}
                  disabled={locked}
                  onChange={(e) => setOtherCharges(e.target.value)}
                  className={INPUT_RIGHT}
                />
              </Field>
              <div className="flex flex-col items-end justify-center gap-1 rounded-lg bg-muted/30 px-4 py-2">
                <div className="flex w-full justify-between text-sm">
                  <span className="text-muted-foreground">Subtotal</span>
                  <span className="tabular-nums">{lkr(subtotal)}</span>
                </div>
                <div className="flex w-full justify-between text-sm">
                  <span className="text-muted-foreground">Tax</span>
                  <span className="tabular-nums">{lkr(taxTotal)}</span>
                </div>
                <div className="flex w-full justify-between border-t border-border pt-1 text-base font-bold">
                  <span>Total</span>
                  <span className="tabular-nums">{lkr(total)}</span>
                </div>
              </div>
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
  className = "",
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
            <h3 className="font-heading text-base font-bold leading-none">
              {title}
            </h3>
            {typeof count === "number" && (
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
  className = "",
}: {
  label: string;
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <div className={className}>
      <label className="mb-1 block text-sm font-semibold text-slate-700">
        {label}
      </label>
      {children}
    </div>
  );
}
