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
} from "lucide-react";
import { Topbar } from "@/components/app-shell/Topbar";
import { apiClient, lkr } from "@/lib/api-client";
import { SearchableSelect } from "@/components/ui/SearchableSelect";

type Location = { id: string; code: string; name: string; isActive: boolean };

type TransferLineDetail = {
  productId: string;
  sku: string;
  productName: string;
  quantity: number;
  unitCost: number;
  lineTotal: number;
};
type TransferDetail = {
  id: string;
  transferNumber: string;
  fromLocationId: string;
  toLocationId: string;
  isReturn: boolean;
  transferDate: string;
  referenceNo: string | null;
  notes: string | null;
  status: string;
  totalCost: number;
  lines: TransferLineDetail[];
};

type GrnSummary = {
  id: string;
  grnNumber: string;
  locationId: string;
  status: string;
};
type GrnDetail = {
  id: string;
  grnNumber: string;
  locationId: string;
  status: string;
  lines: {
    productId: string; sku: string; productName: string; quantityReceived: number;
    requestNoteNumber: string | null; requestNoteDate: string | null;
  }[];
};

type StockRow = { productId: string; quantityOnHand: number; averageCost: number };

type RequestNoteDetail = {
  id: string;
  requestNumber: string;
  mode: string;
  fromLocationId: string;
  toLocationId: string;
  status: string;
  lines: { productId: string; sku: string; productName: string; quantity: number }[];
};

type DraftLine = {
  productId: string;
  productLabel: string;
  quantity: string;
  sourceGrnId?: string;
  sourceGrnNumber?: string;
  requestNoteNumber?: string | null;
  requestNoteDate?: string | null;
};

const INPUT =
  "w-full rounded-lg border border-border bg-card px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary disabled:opacity-60";
const INPUT_RIGHT = `${INPUT} text-right tabular-nums`;

type PagedApiResult<T> = { data: T[]; pagination: { totalPages: number } };

async function fetchPage<T>(
  url: string,
  { page, pageSize, search }: { page: number; pageSize: number; search: string },
): Promise<PagedApiResult<T>> {
  const qs = new URLSearchParams({ pageNumber: String(page), pageSize: String(pageSize), isActive: "true" });
  if (search) qs.set("search", search);
  return apiClient<PagedApiResult<T>>(`${url}?${qs}`);
}

async function fetchLocationOptions(args: { page: number; pageSize: number; search: string }) {
  const res = await fetchPage<{ id: string; code: string; name: string }>("/api/v1/locations/paged", args);
  return {
    items: res.data.map((l) => ({ id: l.id, label: `${l.code} — ${l.name}` })),
    hasMore: args.page < res.pagination.totalPages,
  };
}

async function fetchProductOptions(args: { page: number; pageSize: number; search: string }) {
  const res = await fetchPage<{ id: string; sku: string; name: string }>("/api/v1/products/paged", args);
  return {
    items: res.data.map((p) => ({ id: p.id, label: `${p.name} (${p.sku})` })),
    hasMore: args.page < res.pagination.totalPages,
  };
}

async function fetchEligibleGrns(locationId: string): Promise<GrnSummary[]> {
  const qs = new URLSearchParams({ pageNumber: "1", pageSize: "100", status: "approved", locationId });
  const res = await apiClient<PagedApiResult<GrnSummary> & { data: GrnSummary[] }>(`/api/v1/grn/paged?${qs}`);
  return res.data;
}

function todayIso(): string {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
}
function fmtDate(iso: string): string {
  const d = new Date(iso);
  return `${String(d.getMonth() + 1).padStart(2, "0")}/${String(d.getDate()).padStart(2, "0")}/${d.getFullYear()}`;
}

const emptyLine = (): DraftLine => ({ productId: "", productLabel: "", quantity: "" });

export default function TransferForm({ transferId }: { transferId?: string }) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const isEdit = Boolean(transferId);
  const requestNoteIdsParam = searchParams.get("requestNoteIds");

  const [bundledNotes, setBundledNotes] = useState<RequestNoteDetail[]>([]);
  const [bundleError, setBundleError] = useState<string | null>(null);
  const [bundleLoading, setBundleLoading] = useState(Boolean(requestNoteIdsParam));

  const [allLocations, setAllLocations] = useState<Location[]>([]);
  const [stockByProduct, setStockByProduct] = useState<Map<string, StockRow>>(new Map());

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [status, setStatus] = useState("draft");
  const [transferNumber, setTransferNumber] = useState("");

  const [lineMode, setLineMode] = useState<"manual" | "grn">("manual");
  const [fromLocationId, setFromLocationId] = useState("");
  const [toLocationId, setToLocationId] = useState("");
  const [isReturn, setIsReturn] = useState(false);
  const [transferDate, setTransferDate] = useState(todayIso());
  const [referenceNo, setReferenceNo] = useState("");
  const [notes, setNotes] = useState("");
  const [lines, setLines] = useState<DraftLine[]>([emptyLine()]);

  const [addedGrnIds, setAddedGrnIds] = useState<string[]>([]);
  const [eligibleGrns, setEligibleGrns] = useState<GrnSummary[]>([]);
  const [grnToAdd, setGrnToAdd] = useState("");
  const [grnError, setGrnError] = useState<string | null>(null);
  const [grnLoading, setGrnLoading] = useState(false);

  useEffect(() => {
    (async () => {
      try {
        const locations = await apiClient<Location[]>("/api/v1/locations?all=true");
        setAllLocations(locations);

        if (transferId) {
          const t = await apiClient<TransferDetail>(`/api/v1/transfers/${transferId}`);
          setTransferNumber(t.transferNumber);
          setStatus(t.status);
          setFromLocationId(t.fromLocationId);
          setToLocationId(t.toLocationId);
          setIsReturn(t.isReturn);
          setTransferDate(t.transferDate.slice(0, 10));
          setReferenceNo(t.referenceNo ?? "");
          setNotes(t.notes ?? "");
          setLines(
            t.lines.length
              ? t.lines.map((l) => ({
                  productId: l.productId,
                  productLabel: `${l.productName} (${l.sku})`,
                  quantity: String(l.quantity),
                }))
              : [emptyLine()],
          );
        } else {
          const activeLocations = locations.filter((x) => x.isActive);
          setFromLocationId(activeLocations.find((x) => x.code === "MAIN")?.id ?? activeLocations[0]?.id ?? "");
        }
      } catch (e) {
        setError((e as Error).message);
      } finally {
        setLoading(false);
      }
    })();
  }, [transferId]);

  // Bundling: when arriving from the Request Notes list with one or more selected
  // "transfer"-mode notes, pull them in, merge same-product lines, and pin the
  // From/To locations to what they share — request-note fulfillment is set
  // server-side when the transfer is created, so this only needs to run once.
  useEffect(() => {
    if (isEdit || !requestNoteIdsParam || loading) return;
    const ids = requestNoteIdsParam.split(",").filter(Boolean);
    if (ids.length === 0) return;
    (async () => {
      setBundleLoading(true);
      setBundleError(null);
      try {
        const noteList = await Promise.all(
          ids.map((id) => apiClient<RequestNoteDetail>(`/api/v1/request-notes/${id}`)),
        );
        const bad = noteList.find((n) => n.mode !== "transfer" || n.status !== "approved");
        if (bad) throw new Error(`${bad.requestNumber} is not an approved Transfer-mode request note.`);
        const from = noteList[0].fromLocationId;
        const to = noteList[0].toLocationId;
        if (noteList.some((n) => n.fromLocationId !== from || n.toLocationId !== to))
          throw new Error("Selected request notes must share the same From and To locations.");

        setBundledNotes(noteList);
        setFromLocationId(from);
        setToLocationId(to);

        // Merge same-product lines across notes by summing quantity.
        const merged = new Map<string, { productId: string; label: string; quantity: number }>();
        for (const n of noteList) {
          for (const l of n.lines) {
            const existing = merged.get(l.productId);
            if (existing) existing.quantity += l.quantity;
            else merged.set(l.productId, { productId: l.productId, label: `${l.productName} (${l.sku})`, quantity: l.quantity });
          }
        }
        const mergedLines: DraftLine[] = [...merged.values()].map((m) => ({
          productId: m.productId,
          productLabel: m.label,
          quantity: String(m.quantity),
        }));
        setLines(mergedLines.length ? mergedLines : [emptyLine()]);
      } catch (e) {
        setBundleError((e as Error).message);
      } finally {
        setBundleLoading(false);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isEdit, requestNoteIdsParam, loading]);

  // GRN based mode: the picker's options are approved GRNs received at the From
  // location — their lines get pulled in as-is. Each line also carries whichever
  // request note specifically asked for that product, traced through GRN -> PO ->
  // request note, purely for reference (a TOG never fulfills a request note itself).
  useEffect(() => {
    if (lineMode !== "grn" || !fromLocationId) {
      setEligibleGrns([]);
      return;
    }
    fetchEligibleGrns(fromLocationId)
      .then((grns) => setEligibleGrns(grns.filter((g) => !addedGrnIds.includes(g.id))))
      .catch(() => setEligibleGrns([]));
  }, [lineMode, fromLocationId, addedGrnIds]);

  // Reload the From location's live stock-on-hand whenever it changes, so the
  // Current stock / Unit cost / SIH preview columns reflect reality.
  useEffect(() => {
    if (!fromLocationId) {
      setStockByProduct(new Map());
      return;
    }
    apiClient<{ productId: string; quantityOnHand: number; averageCost: number }[]>(
      `/api/v1/inventory/stock?locationId=${fromLocationId}`,
    )
      .then((rows) => setStockByProduct(new Map(rows.map((r) => [r.productId, r]))))
      .catch(() => setStockByProduct(new Map()));
  }, [fromLocationId]);

  const locationLabel = (id: string) => {
    const loc = allLocations.find((l) => l.id === id);
    return loc ? `${loc.code} — ${loc.name}` : "";
  };

  function switchLineMode(next: "manual" | "grn") {
    if (next === lineMode) return;
    setLineMode(next);
    setLines([emptyLine()]);
    setAddedGrnIds([]);
    setGrnError(null);
  }

  function onLocationChange(which: "from" | "to", id: string) {
    if (which === "from") setFromLocationId(id);
    else setToLocationId(id);
    // Changing the From location invalidates whichever GRNs are already pulled in
    // (they were received at the previous From location).
    if (which === "from" && addedGrnIds.length) {
      setAddedGrnIds([]);
      setLines([emptyLine()]);
    }
  }

  const setLine = (idx: number, patch: Partial<DraftLine>) =>
    setLines((prev) => prev.map((l, i) => (i === idx ? { ...l, ...patch } : l)));

  const addLine = () => setLines((prev) => [...prev, emptyLine()]);
  const removeLine = (idx: number) =>
    setLines((prev) => (prev.length === 1 ? prev : prev.filter((_, i) => i !== idx)));

  // Removing any line sourced from a GRN drops that GRN's whole contribution,
  // freeing it back up in the picker.
  function removeGrnLines(grnId: string) {
    setLines((prev) => {
      const next = prev.filter((l) => l.sourceGrnId !== grnId);
      return next.length ? next : [emptyLine()];
    });
    setAddedGrnIds((prev) => prev.filter((id) => id !== grnId));
  }

  async function addGrn() {
    if (!grnToAdd) return;
    setGrnError(null);
    setGrnLoading(true);
    try {
      const grn = await apiClient<GrnDetail>(`/api/v1/grn/${grnToAdd}`);
      const newLines: DraftLine[] = grn.lines.map((l) => ({
        productId: l.productId,
        productLabel: `${l.productName} (${l.sku})`,
        quantity: String(l.quantityReceived),
        sourceGrnId: grn.id,
        sourceGrnNumber: grn.grnNumber,
        requestNoteNumber: l.requestNoteNumber,
        requestNoteDate: l.requestNoteDate,
      }));
      setLines((prev) => {
        const withoutBlank = prev.filter((l) => l.productId || l.sourceGrnId);
        return [...withoutBlank, ...newLines];
      });
      setAddedGrnIds((prev) => [...prev, grn.id]);
      setGrnToAdd("");
    } catch (e) {
      setGrnError((e as Error).message);
    } finally {
      setGrnLoading(false);
    }
  }

  function currentStockOf(productId: string): number {
    return stockByProduct.get(productId)?.quantityOnHand ?? 0;
  }
  function unitCostOf(productId: string): number {
    return stockByProduct.get(productId)?.averageCost ?? 0;
  }
  function lineTotalOf(l: DraftLine): number {
    return (Number(l.quantity) || 0) * unitCostOf(l.productId);
  }

  const totalCost = useMemo(
    () => lines.filter((l) => l.productId).reduce((sum, l) => sum + lineTotalOf(l), 0),
    [lines, stockByProduct],
  );

  function validate(): string | null {
    if (!fromLocationId) return "Choose a From location.";
    if (!toLocationId) return "Choose a To location.";
    if (fromLocationId === toLocationId) return "From and To locations must differ.";
    const filled = lines.filter((l) => l.productId);
    if (filled.length === 0) return "Add at least one line.";
    for (const l of filled) {
      const q = Number(l.quantity);
      if (!Number.isFinite(q) || q <= 0) return "Each line needs a quantity greater than zero.";
    }
    return null;
  }

  async function submit(thenDispatch: boolean) {
    const err = validate();
    if (err) {
      setError(err);
      return;
    }
    setError(null);
    setSaving(true);
    try {
      const payload = {
        fromLocationId,
        toLocationId,
        isReturn,
        transferDate,
        referenceNo: referenceNo || null,
        notes: notes || null,
        lines: lines
          .filter((l) => l.productId)
          .map((l) => ({ productId: l.productId, quantity: Number(l.quantity) })),
        requestNoteIds: !isEdit && bundledNotes.length ? bundledNotes.map((n) => n.id) : undefined,
      };
      let id = transferId;
      if (isEdit) {
        await apiClient(`/api/v1/transfers/${transferId}`, {
          method: "PUT",
          body: JSON.stringify(payload),
        });
      } else {
        const created = await apiClient<{ id: string }>("/api/v1/transfers", {
          method: "POST",
          body: JSON.stringify(payload),
        });
        id = created.id;
      }
      if (thenDispatch && id) {
        await apiClient(`/api/v1/transfers/${id}/submit`, { method: "POST" });
      }
      router.push("/transfers");
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
          title={isEdit ? "Edit Transfer of Goods" : "New Transfer of Goods"}
          subtitle={isEdit ? "Loading transfer…" : "Create a transfer of goods"}
        />
        <div className="p-6 space-y-5">
          {Array.from({ length: 2 }).map((_, section) => (
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

  const locked = isEdit && status !== "draft";

  return (
    <>
      <Topbar
        title={isEdit ? "Edit Transfer of Goods" : "New Transfer of Goods"}
        subtitle={isEdit ? `Editing ${transferNumber}` : "Create a transfer of goods"}
      />

      <div className="p-6 pb-28">
        <div className="sticky top-0 z-30 -mx-6 mb-6 border-b border-border/70 bg-background/85 px-6 py-4 backdrop-blur supports-[backdrop-filter]:bg-background/70">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <button
                onClick={() => router.push("/transfers")}
                className="mb-1 flex items-center gap-1 text-sm font-medium text-muted-foreground transition hover:text-foreground"
              >
                <ArrowLeft className="size-4" />
                Back to transfers
              </button>
              <h2 className="font-heading text-xl font-bold">
                {isEdit ? `Edit ${transferNumber}` : "Create Transfer of Goods"}
              </h2>
              <p className="text-sm text-muted-foreground">
                {locked
                  ? `This transfer is ${status} and can no longer be edited.`
                  : isEdit
                    ? "Locations, lines and reference."
                    : "Save as a draft to keep editing, or submit it for processing — it dispatches straight away, or waits for approval depending on your settings."}
              </p>
            </div>

            <div className="flex items-center gap-3">
              <button
                onClick={() => router.push("/transfers")}
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
          <div className="mb-5 rounded-lg bg-red-50 px-4 py-3 text-sm text-status-error">{error}</div>
        )}

        {bundleLoading && (
          <div className="mb-5 rounded-lg bg-blue-50 px-4 py-3 text-sm text-blue-800">
            Loading the selected request notes…
          </div>
        )}
        {bundleError && (
          <div className="mb-5 rounded-lg bg-red-50 px-4 py-3 text-sm text-status-error">{bundleError}</div>
        )}
        {!bundleLoading && bundledNotes.length > 0 && (
          <div className="mb-5 rounded-lg bg-blue-50 px-4 py-3 text-sm text-blue-800">
            Bundling {bundledNotes.length} request note{bundledNotes.length === 1 ? "" : "s"}:{" "}
            {bundledNotes.map((n) => n.requestNumber).join(", ")}. They&apos;ll be marked fulfilled once this transfer is created.
          </div>
        )}

        <div className="grid grid-cols-12 gap-5">
          <SectionCard className="col-span-12" icon={<ClipboardList className="size-4" />} title="Transfer Details">
            <div className="mb-3 flex flex-wrap items-center gap-x-6 gap-y-2">
              <div className="flex items-center gap-4">
                <span className="text-sm font-semibold text-slate-700">Mode</span>
                {!isEdit ? (
                  <>
                    <label className="flex items-center gap-1.5 text-sm">
                      <input
                        type="radio"
                        name="lineMode"
                        checked={lineMode === "manual"}
                        onChange={() => switchLineMode("manual")}
                        className="size-4 text-primary focus:ring-primary"
                      />
                      Manual
                    </label>
                    <label className="flex items-center gap-1.5 text-sm">
                      <input
                        type="radio"
                        name="lineMode"
                        checked={lineMode === "grn"}
                        onChange={() => switchLineMode("grn")}
                        className="size-4 text-primary focus:ring-primary"
                      />
                      GRN based
                    </label>
                  </>
                ) : (
                  <span className="text-sm text-muted-foreground">
                    {lines.some((l) => l.sourceGrnId) ? "GRN based" : "Manual"}
                  </span>
                )}
              </div>
              <div className="flex items-center gap-4">
                <span className="text-sm font-semibold text-slate-700">Stock</span>
                <label className="flex items-center gap-1.5 text-sm">
                  <input
                    type="radio"
                    name="isReturn"
                    checked={!isReturn}
                    disabled={locked}
                    onChange={() => setIsReturn(false)}
                    className="size-4 text-primary focus:ring-primary"
                  />
                  Issue
                </label>
                <label className="flex items-center gap-1.5 text-sm">
                  <input
                    type="radio"
                    name="isReturn"
                    checked={isReturn}
                    disabled={locked}
                    onChange={() => setIsReturn(true)}
                    className="size-4 text-primary focus:ring-primary"
                  />
                  Return
                </label>
              </div>
            </div>
            <div className="grid grid-cols-4 gap-3">
              <Field label="From Location">
                <SearchableSelect
                  value={fromLocationId}
                  selectedLabel={locationLabel(fromLocationId)}
                  disabled={locked}
                  placeholder="Search locations…"
                  fetchPage={fetchLocationOptions}
                  onChange={(id) => onLocationChange("from", id)}
                />
              </Field>
              <Field label="To Location">
                <SearchableSelect
                  value={toLocationId}
                  selectedLabel={locationLabel(toLocationId)}
                  disabled={locked}
                  placeholder="Search locations…"
                  fetchPage={fetchLocationOptions}
                  onChange={(id) => onLocationChange("to", id)}
                />
              </Field>
              <Field label="TOG Date">
                <input
                  type="date"
                  value={transferDate}
                  disabled={locked}
                  onChange={(e) => setTransferDate(e.target.value)}
                  className={INPUT}
                />
              </Field>
              <Field label="Ref No.">
                <input
                  value={referenceNo}
                  disabled={locked}
                  onChange={(e) => setReferenceNo(e.target.value)}
                  placeholder="Optional reference"
                  className={INPUT}
                />
              </Field>
              <Field label="Remark" className="col-span-2">
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

          {lineMode === "grn" && !locked && (
            <SectionCard className="col-span-12" icon={<ClipboardList className="size-4" />} title="GRN">
              {!fromLocationId ? (
                <p className="text-sm text-muted-foreground">Choose a From location first.</p>
              ) : (
                <div className="flex items-center gap-2">
                  <select
                    value={grnToAdd}
                    onChange={(e) => setGrnToAdd(e.target.value)}
                    className={INPUT}
                  >
                    <option value="">
                      {eligibleGrns.length === 0 ? "No approved GRNs at this location" : "-- Select a GRN --"}
                    </option>
                    {eligibleGrns.map((g) => (
                      <option key={g.id} value={g.id}>{g.grnNumber}</option>
                    ))}
                  </select>
                  <button
                    onClick={addGrn}
                    disabled={!grnToAdd || grnLoading}
                    className="flex size-9 shrink-0 items-center justify-center rounded-full bg-primary text-primary-foreground hover:bg-primary-dark disabled:opacity-50"
                    title="Add GRN"
                  >
                    <Plus className="size-4" />
                  </button>
                </div>
              )}
              {grnError && <p className="mt-2 text-sm text-status-error">{grnError}</p>}
            </SectionCard>
          )}

          <SectionCard
            className="col-span-12"
            icon={<Receipt className="size-4" />}
            title={lineMode === "grn" ? "Transfer of Goods" : "Lines"}
            count={lines.filter((l) => l.productId).length}
            action={
              lineMode === "manual" &&
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
                  {lineMode === "grn" ? (
                    <tr>
                      <th className="px-3 py-2 font-medium">Request Note Date</th>
                      <th className="px-3 py-2 font-medium">Request Note No</th>
                      <th className="px-3 py-2 font-medium">Item</th>
                      <th className="w-20 px-3 py-2 text-right font-medium">SIH</th>
                      <th className="w-24 px-3 py-2 text-right font-medium">Quantity</th>
                      <th className="w-10 px-3 py-2" />
                    </tr>
                  ) : (
                    <tr>
                      <th className="px-3 py-2 font-medium">Product</th>
                      <th className="w-24 px-3 py-2 text-right font-medium">Current stock</th>
                      <th className="w-24 px-3 py-2 text-right font-medium">Quantity</th>
                      <th className="w-24 px-3 py-2 text-right font-medium">Unit cost</th>
                      <th className="w-28 px-3 py-2 text-right font-medium">Line value</th>
                      <th className="w-10 px-3 py-2" />
                    </tr>
                  )}
                </thead>
                <tbody>
                  {lineMode === "grn"
                    ? lines
                        .filter((l) => l.productId)
                        .map((l, idx) => (
                          <tr key={idx} className="border-b border-border last:border-0">
                            <td className="px-3 py-2 text-muted-foreground">
                              {l.requestNoteDate ? fmtDate(l.requestNoteDate) : "—"}
                            </td>
                            <td className="px-3 py-2 font-mono text-xs text-muted-foreground">{l.requestNoteNumber ?? "—"}</td>
                            <td className="px-3 py-2">{l.productLabel}</td>
                            <td className="px-3 py-2 text-right tabular-nums text-muted-foreground">
                              {currentStockOf(l.productId)}
                            </td>
                            <td className="px-3 py-2 text-right tabular-nums">{l.quantity}</td>
                            <td className="px-3 py-2 text-right">
                              <button
                                onClick={() => l.sourceGrnId && removeGrnLines(l.sourceGrnId)}
                                className="rounded p-1 text-muted-foreground hover:bg-muted"
                                title={`Remove ${l.sourceGrnNumber}`}
                              >
                                <Trash2 className="size-4" />
                              </button>
                            </td>
                          </tr>
                        ))
                    : lines.map((l, idx) => (
                        <tr key={idx} className="border-b border-border last:border-0">
                          <td className="px-3 py-2">
                            <SearchableSelect
                              value={l.productId}
                              selectedLabel={l.productLabel}
                              disabled={locked}
                              placeholder="Search products…"
                              fetchPage={fetchProductOptions}
                              onChange={(id, option) =>
                                setLine(idx, { productId: id, productLabel: option?.label ?? "" })
                              }
                            />
                          </td>
                          <td className="px-3 py-2 text-right tabular-nums text-muted-foreground">
                            {l.productId ? currentStockOf(l.productId) : "—"}
                          </td>
                          <td className="px-3 py-2">
                            <input
                              type="number"
                              min="0"
                              step="any"
                              value={l.quantity}
                              disabled={locked}
                              onChange={(e) => setLine(idx, { quantity: e.target.value })}
                              placeholder="0"
                              className={INPUT_RIGHT}
                            />
                          </td>
                          <td className="px-3 py-2 text-right tabular-nums text-muted-foreground">
                            {l.productId ? lkr(unitCostOf(l.productId)) : "—"}
                          </td>
                          <td className="px-3 py-2 text-right font-medium tabular-nums">
                            {l.productId && l.quantity ? lkr(lineTotalOf(l)) : "—"}
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
                      ))}
                  {lineMode === "grn" && lines.filter((l) => l.productId).length === 0 && (
                    <tr>
                      <td colSpan={6} className="px-3 py-8 text-center text-muted-foreground">
                        Add a GRN above to pull in its items.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>

            {lineMode === "manual" && (
              <div className="mt-4 flex justify-end">
                <div className="flex w-64 flex-col gap-1 rounded-lg bg-muted/30 px-4 py-2">
                  <div className="flex w-full justify-between border-t border-border pt-1 text-base font-bold">
                    <span>Total value</span>
                    <span className="tabular-nums">{lkr(totalCost)}</span>
                  </div>
                </div>
              </div>
            )}
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
            <h3 className="font-heading text-base font-bold leading-none">{title}</h3>
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
      <label className="mb-1 block text-sm font-semibold text-slate-700">{label}</label>
      {children}
    </div>
  );
}
