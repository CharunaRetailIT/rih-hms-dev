"use client";

import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
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
import { apiClient } from "@/lib/api-client";
import { SearchableSelect } from "@/components/ui/SearchableSelect";

type Location = { id: string; code: string; name: string; isActive: boolean };

type RequestNoteLineDetail = {
  productId: string;
  sku: string;
  productName: string;
  sih: number;
  quantity: number;
  remark: string | null;
};
type RequestNoteDetail = {
  id: string;
  requestNumber: string;
  mode: string;
  fromLocationId: string;
  toLocationId: string;
  expectedDeliveryDate: string;
  status: string;
  commonRemark: string | null;
  lines: RequestNoteLineDetail[];
};

type DraftLine = {
  productId: string;
  productLabel: string;
  quantity: string;
  remark: string;
};

const MODES = [
  { value: "po", label: "PO" },
  { value: "transfer", label: "Transfer" },
];

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

function todayIso(): string {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
}

const emptyLine = (): DraftLine => ({ productId: "", productLabel: "", quantity: "", remark: "" });

export default function RequestNoteForm({ requestId }: { requestId?: string }) {
  const router = useRouter();
  const isEdit = Boolean(requestId);

  const [allLocations, setAllLocations] = useState<Location[]>([]);
  const [sihByProduct, setSihByProduct] = useState<Map<string, number>>(new Map());

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [status, setStatus] = useState("draft");
  const [requestNumber, setRequestNumber] = useState("");

  const [mode, setMode] = useState("po");
  const [fromLocationId, setFromLocationId] = useState("");
  const [toLocationId, setToLocationId] = useState("");
  const [expectedDeliveryDate, setExpectedDeliveryDate] = useState(todayIso());
  const [commonRemark, setCommonRemark] = useState("");
  const [lines, setLines] = useState<DraftLine[]>([emptyLine()]);

  useEffect(() => {
    (async () => {
      try {
        const locations = await apiClient<Location[]>("/api/v1/locations?all=true");
        setAllLocations(locations);

        if (requestId) {
          const r = await apiClient<RequestNoteDetail>(`/api/v1/request-notes/${requestId}`);
          setRequestNumber(r.requestNumber);
          setStatus(r.status);
          setMode(r.mode);
          setFromLocationId(r.fromLocationId);
          setToLocationId(r.toLocationId);
          setExpectedDeliveryDate(r.expectedDeliveryDate.slice(0, 10));
          setCommonRemark(r.commonRemark ?? "");
          setLines(
            r.lines.length
              ? r.lines.map((l) => ({
                  productId: l.productId,
                  productLabel: `${l.productName} (${l.sku})`,
                  quantity: String(l.quantity),
                  remark: l.remark ?? "",
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
  }, [requestId]);

  // Reload the From location's live stock-on-hand whenever it changes, so the
  // SIH preview column reflects reality rather than a stale draft.
  useEffect(() => {
    if (!fromLocationId) {
      setSihByProduct(new Map());
      return;
    }
    apiClient<{ productId: string; quantityOnHand: number }[]>(
      `/api/v1/inventory/stock?locationId=${fromLocationId}`,
    )
      .then((rows) => setSihByProduct(new Map(rows.map((r) => [r.productId, r.quantityOnHand]))))
      .catch(() => setSihByProduct(new Map()));
  }, [fromLocationId]);

  const locationLabel = (id: string) => {
    const loc = allLocations.find((l) => l.id === id);
    return loc ? `${loc.code} — ${loc.name}` : "";
  };

  const setLine = (idx: number, patch: Partial<DraftLine>) =>
    setLines((prev) => prev.map((l, i) => (i === idx ? { ...l, ...patch } : l)));

  const addLine = () => setLines((prev) => [...prev, emptyLine()]);
  const removeLine = (idx: number) =>
    setLines((prev) => (prev.length === 1 ? prev : prev.filter((_, i) => i !== idx)));

  function sihOf(productId: string): number {
    return sihByProduct.get(productId) ?? 0;
  }

  const lineCount = useMemo(() => lines.filter((l) => l.productId).length, [lines]);

  function validate(): string | null {
    if (!fromLocationId) return "Choose a From location.";
    if (!toLocationId) return "Choose a To location.";
    if (fromLocationId === toLocationId) return "From and To locations must differ.";
    if (!expectedDeliveryDate) return "Choose an expected delivery date.";
    if (expectedDeliveryDate < todayIso()) return "Expected delivery date can't be in the past.";
    const filled = lines.filter((l) => l.productId);
    if (filled.length === 0) return "Add at least one line.";
    for (const l of filled) {
      const q = Number(l.quantity);
      if (!Number.isFinite(q) || q <= 0) return "Each line needs a quantity greater than zero.";
    }
    return null;
  }

  async function submit(thenSubmit: boolean) {
    const err = validate();
    if (err) {
      setError(err);
      return;
    }
    setError(null);
    setSaving(true);
    try {
      const payload = {
        mode,
        fromLocationId,
        toLocationId,
        expectedDeliveryDate,
        commonRemark: commonRemark || null,
        lines: lines
          .filter((l) => l.productId)
          .map((l) => ({ productId: l.productId, quantity: Number(l.quantity), remark: l.remark || null })),
      };
      let id = requestId;
      if (isEdit) {
        await apiClient(`/api/v1/request-notes/${requestId}`, {
          method: "PUT",
          body: JSON.stringify(payload),
        });
      } else {
        const created = await apiClient<{ id: string }>("/api/v1/request-notes", {
          method: "POST",
          body: JSON.stringify(payload),
        });
        id = created.id;
      }
      if (thenSubmit && id) {
        await apiClient(`/api/v1/request-notes/${id}/submit`, { method: "POST" });
      }
      router.push("/request-notes");
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
          title={isEdit ? "Edit Request Note" : "New Request Note"}
          subtitle={isEdit ? "Loading request note…" : "Create a request note"}
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
        title={isEdit ? "Edit Request Note" : "New Request Note"}
        subtitle={isEdit ? `Editing ${requestNumber}` : "Create a request note"}
      />

      <div className="p-6 pb-28">
        <div className="sticky top-0 z-30 -mx-6 mb-6 border-b border-border/70 bg-background/85 px-6 py-4 backdrop-blur supports-[backdrop-filter]:bg-background/70">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <button
                onClick={() => router.push("/request-notes")}
                className="mb-1 flex items-center gap-1 text-sm font-medium text-muted-foreground transition hover:text-foreground"
              >
                <ArrowLeft className="size-4" />
                Back to request notes
              </button>
              <h2 className="font-heading text-xl font-bold">
                {isEdit ? `Edit ${requestNumber}` : "Create Request Note"}
              </h2>
              <p className="text-sm text-muted-foreground">
                {locked
                  ? `This request note is ${status} and can no longer be edited.`
                  : isEdit
                    ? "Mode, locations, date and lines."
                    : "Save as a draft to keep editing, or submit it straight away."}
              </p>
            </div>

            <div className="flex items-center gap-3">
              <button
                onClick={() => router.push("/request-notes")}
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

        <div className="grid grid-cols-12 gap-5">
          <SectionCard className="col-span-12" icon={<ClipboardList className="size-4" />} title="Request Details">
            <div className="mb-3 flex items-center gap-4">
              <span className="text-sm font-semibold text-slate-700">Mode</span>
              {MODES.map((m) => (
                <label key={m.value} className="flex items-center gap-1.5 text-sm">
                  <input
                    type="radio"
                    name="mode"
                    checked={mode === m.value}
                    disabled={locked}
                    onChange={() => setMode(m.value)}
                    className="size-4 text-primary focus:ring-primary"
                  />
                  {m.label}
                </label>
              ))}
            </div>
            <div className="grid grid-cols-4 gap-3">
              <Field label="From Location">
                <SearchableSelect
                  value={fromLocationId}
                  selectedLabel={locationLabel(fromLocationId)}
                  disabled={locked}
                  placeholder="Search locations…"
                  fetchPage={fetchLocationOptions}
                  onChange={(id) => setFromLocationId(id)}
                />
              </Field>
              <Field label="To Location">
                <SearchableSelect
                  value={toLocationId}
                  selectedLabel={locationLabel(toLocationId)}
                  disabled={locked}
                  placeholder="Search locations…"
                  fetchPage={fetchLocationOptions}
                  onChange={(id) => setToLocationId(id)}
                />
              </Field>
              <Field label="Expected Delivery Date">
                <input
                  type="date"
                  value={expectedDeliveryDate}
                  min={todayIso()}
                  disabled={locked}
                  onChange={(e) => setExpectedDeliveryDate(e.target.value)}
                  className={INPUT}
                />
              </Field>
              <Field label="Common Remark">
                <input
                  value={commonRemark}
                  disabled={locked}
                  onChange={(e) => setCommonRemark(e.target.value)}
                  placeholder="Internal remark"
                  className={INPUT}
                />
              </Field>
            </div>
          </SectionCard>

          <SectionCard
            className="col-span-12"
            icon={<Receipt className="size-4" />}
            title="Products"
            count={lineCount}
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
                    <th className="w-20 px-3 py-2 text-right font-medium">SIH</th>
                    <th className="w-24 px-3 py-2 text-right font-medium">Quantity</th>
                    <th className="px-3 py-2 font-medium">Remark</th>
                    <th className="w-10 px-3 py-2" />
                  </tr>
                </thead>
                <tbody>
                  {lines.map((l, idx) => (
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
                        {l.productId ? sihOf(l.productId) : "—"}
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
                      <td className="px-3 py-2">
                        <input
                          value={l.remark}
                          disabled={locked}
                          onChange={(e) => setLine(idx, { remark: e.target.value })}
                          placeholder="Optional"
                          className={INPUT}
                        />
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
                </tbody>
              </table>
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
