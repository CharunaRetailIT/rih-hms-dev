"use client";

import { useEffect, useState } from "react";
import { Topbar } from "@/components/app-shell/Topbar";
import { apiClient } from "@/lib/api-client";
import { Pagination } from "@/components/ui/Pagination";
import { ArrowLeftRight, Plus, Search, X } from "lucide-react";

type Unit = {
  id: string;
  code: string;
  name: string;
  symbol: string | null;
  isBaseUnit: boolean;
  dimension: "mass" | "volume" | "count";
  factorToBase: number;
};

type UnitConversion = {
  id: string;
  unitOfMeasureId: string;
  unitCode: string;
  unitName: string;
  subUnitOfMeasureId: string;
  subUnitValue: number;
  baseUnitValue: number;
};

type PagedResponse = {
  data: UnitConversion[];
  pagination: {
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
  };
};

type ConversionResult = {
  fromUnitId: string;
  fromCode: string;
  toUnitId: string;
  toCode: string;
  quantity: number;
  convertedQuantity: number;
  dimension: string;
};

type Form = {
  id: string | null;
  unitOfMeasureId: string;
  subUnitOfMeasureId: string;
  subUnitValue: string;
  baseUnitValue: string;
};

const emptyForm: Form = {
  id: null,
  unitOfMeasureId: "",
  subUnitOfMeasureId: "",
  subUnitValue: "1",
  baseUnitValue: "",
};

export default function UomConversionsPage() {
  const [units, setUnits] = useState<Unit[]>([]);
  const [items, setItems] = useState<UnitConversion[]>([]);
  const [loading, setLoading] = useState(true);

  const [search, setSearch] = useState("");
  const [unitFilter, setUnitFilter] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const [open, setOpen] = useState(false);
  const [form, setForm] = useState<Form>(emptyForm);
  const [submitting, setSubmitting] = useState(false);
  const [busyId, setBusyId] = useState<string | null>(null);

  const [fromUnitId, setFromUnitId] = useState("");
  const [toUnitId, setToUnitId] = useState("");
  const [quantity, setQuantity] = useState("1");
  const [result, setResult] = useState<ConversionResult | null>(null);

  const [toast, setToast] = useState<string | null>(null);

  function flash(message: string) {
    setToast(message);
    window.setTimeout(() => setToast(null), 3000);
  }

  function getUnitName(id: string) {
    return units.find((u) => u.id === id)?.name ?? "—";
  }

  function getUnitCode(id: string) {
    return units.find((u) => u.id === id)?.code ?? "—";
  }

  async function loadUnits() {
    const data = await apiClient<Unit[]>("/api/v1/units-of-measure");
    setUnits(data);

    const first = data[0];
    const second = data.find(
      (x) => x.dimension === first?.dimension && x.id !== first?.id,
    );

    if (first && !fromUnitId) setFromUnitId(first.id);
    if (second && !toUnitId) setToUnitId(second.id);

    if (first && !form.unitOfMeasureId) {
      setForm((f) => ({ ...f, unitOfMeasureId: first.id }));
    }
  }

  async function loadConversions() {
    setLoading(true);

    try {
      const params = new URLSearchParams();
      params.set("pageNumber", String(pageNumber));
      params.set("pageSize", String(pageSize));

      if (search.trim()) params.set("search", search.trim());
      if (unitFilter) params.set("unitOfMeasureId", unitFilter);

      const res = await apiClient<PagedResponse>(
        `/api/v1/uom-conversions/paged?${params.toString()}`,
      );

      setItems(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages || 1);
    } catch (e) {
      flash(extractError(e, "Could not load conversions."));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadUnits();
  }, []);

  useEffect(() => {
    void loadConversions();
  }, [pageNumber, pageSize, unitFilter]);

  useEffect(() => {
    const t = window.setTimeout(() => {
      setPageNumber(1);
      void loadConversions();
    }, 350);

    return () => window.clearTimeout(t);
  }, [search]);

  const fromUnit = units.find((x) => x.id === fromUnitId);
  const toUnit = units.find((x) => x.id === toUnitId);

  const compatibleToUnits = fromUnit
    ? units.filter((x) => x.dimension === fromUnit.dimension)
    : units;

  function openNew() {
    setForm({
      ...emptyForm,
      unitOfMeasureId: units[0]?.id ?? "",
      subUnitOfMeasureId: "",
    });

    setOpen(true);
  }

  function openEdit(item: UnitConversion) {
    setForm({
      id: item.id,
      unitOfMeasureId: item.unitOfMeasureId,
      subUnitOfMeasureId: item.subUnitOfMeasureId,
      subUnitValue: String(item.subUnitValue),
      baseUnitValue: String(item.baseUnitValue),
    });

    setOpen(true);
  }

  async function save() {
    if (!form.unitOfMeasureId) {
      flash("Base unit is required.");
      return;
    }

    if (!form.subUnitOfMeasureId) {
      flash("Sub unit is required.");
      return;
    }

    if (form.unitOfMeasureId === form.subUnitOfMeasureId) {
      flash("Base unit and sub unit cannot be same.");
      return;
    }

    const subUnitValue = Number(form.subUnitValue);
    const baseUnitValue = Number(form.baseUnitValue);

    if (Number.isNaN(subUnitValue) || subUnitValue <= 0) {
      flash("Sub unit value must be greater than zero.");
      return;
    }

    if (Number.isNaN(baseUnitValue) || baseUnitValue <= 0) {
      flash("Base unit value must be greater than zero.");
      return;
    }

    setSubmitting(true);

    try {
      await apiClient("/api/v1/uom-conversions", {
        method: "PUT",
        body: JSON.stringify({
          id: form.id,
          unitOfMeasureId: form.unitOfMeasureId,
          subUnitOfMeasureId: form.subUnitOfMeasureId,
          subUnitValue,
          baseUnitValue,
        }),
      });

      setOpen(false);
      flash(form.id ? "Conversion updated." : "Conversion created.");
      await loadConversions();
    } catch (e) {
      flash(extractError(e, "Could not save conversion."));
    } finally {
      setSubmitting(false);
    }
  }

  async function remove(item: UnitConversion) {
    setBusyId(item.id);

    try {
      await apiClient(`/api/v1/uom-conversions/${item.id}`, {
        method: "DELETE",
      });

      flash(`${getUnitName(item.subUnitOfMeasureId)} removed.`);
      await loadConversions();
    } catch (e) {
      flash(extractError(e, "Could not remove conversion."));
    } finally {
      setBusyId(null);
    }
  }

  async function convert() {
    if (!fromUnitId || !toUnitId) {
      flash("Select both units.");
      return;
    }

    const qty = Number(quantity);

    if (Number.isNaN(qty) || qty < 0) {
      flash("Enter valid quantity.");
      return;
    }

    try {
      const data = await apiClient<ConversionResult>(
        "/api/v1/uom-conversions/convert",
        {
          method: "POST",
          body: JSON.stringify({
            fromUnitId,
            toUnitId,
            quantity: qty,
          }),
        },
      );

      setResult(data);
    } catch (e) {
      flash(extractError(e, "Could not convert units."));
    }
  }

  function swapUnits() {
    setFromUnitId(toUnitId);
    setToUnitId(fromUnitId);
    setResult(null);
  }

  const start = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const end = Math.min(pageNumber * pageSize, totalCount);

  return (
    <>
      <Topbar title="Master Data" subtitle="UOM Conversions" />

      <div className="p-6">
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Unit Conversions</h2>
            <p className="text-sm text-muted-foreground">
              Manage conversion rules and calculate quantities between
              compatible units.
            </p>
          </div>

          <button
            onClick={openNew}
            className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark"
          >
            <Plus className="size-4" />
            New conversion
          </button>
        </div>

        <div className="grid max-w-4xl grid-cols-1 gap-4 lg:grid-cols-[1fr_auto_1fr]">
          <div className="card p-5">
            <label className="mb-1 block text-sm font-semibold">
              From unit
            </label>

            <select
              value={fromUnitId}
              onChange={(e) => {
                const id = e.target.value;
                const unit = units.find((x) => x.id === id);

                setFromUnitId(id);
                setResult(null);

                if (unit && toUnit?.dimension !== unit.dimension) {
                  const next = units.find(
                    (x) => x.dimension === unit.dimension && x.id !== unit.id,
                  );

                  setToUnitId(next?.id ?? "");
                }
              }}
              className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
            >
              {units.map((u) => (
                <option key={u.id} value={u.id}>
                  {u.code} — {u.name}
                </option>
              ))}
            </select>

            <label className="mb-1 mt-4 block text-sm font-semibold">
              Quantity
            </label>

            <input
              value={quantity}
              onChange={(e) => {
                setQuantity(e.target.value.replace(/[^0-9.]/g, ""));
                setResult(null);
              }}
              inputMode="decimal"
              className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 tabular-nums focus:border-primary focus:ring-2 focus:ring-primary/20"
            />

            {fromUnit && (
              <p className="mt-3 text-xs text-muted-foreground">
                Dimension: {fromUnit.dimension} · Factor to base:{" "}
                {fromUnit.factorToBase}
              </p>
            )}
          </div>

          <div className="flex items-center justify-center">
            <button
              onClick={swapUnits}
              className="flex size-11 items-center justify-center rounded-full border border-border bg-card text-muted-foreground hover:bg-muted hover:text-foreground"
              title="Swap units"
            >
              <ArrowLeftRight className="size-5" />
            </button>
          </div>

          <div className="card p-5">
            <label className="mb-1 block text-sm font-semibold">To unit</label>

            <select
              value={toUnitId}
              onChange={(e) => {
                setToUnitId(e.target.value);
                setResult(null);
              }}
              className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
            >
              {compatibleToUnits.map((u) => (
                <option key={u.id} value={u.id}>
                  {u.code} — {u.name}
                </option>
              ))}
            </select>

            {toUnit && (
              <p className="mt-3 text-xs text-muted-foreground">
                Dimension: {toUnit.dimension} · Factor to base:{" "}
                {toUnit.factorToBase}
              </p>
            )}

            <button
              onClick={convert}
              className="mt-6 w-full rounded-lg bg-primary py-2.5 text-sm font-bold text-primary-foreground hover:bg-primary-dark"
            >
              Convert
            </button>
          </div>
        </div>

        {result && (
          <div className="card mt-5 max-w-4xl p-6">
            <p className="text-sm text-muted-foreground">Conversion result</p>

            <div className="mt-2 text-3xl font-bold tabular-nums">
              {result.quantity} {result.fromCode}
              <span className="mx-3 text-muted-foreground">=</span>
              {Number(result.convertedQuantity.toFixed(6))} {result.toCode}
            </div>
          </div>
        )}

        <div className="mt-6 mb-4 flex flex-wrap items-center gap-3">
          <div className="relative">
            <Search className="absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />

            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search conversion"
              className="rounded-lg border border-border bg-card py-1.5 pl-8 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
            />
          </div>

          <select
            value={unitFilter}
            onChange={(e) => {
              setUnitFilter(e.target.value);
              setPageNumber(1);
            }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">All base units</option>

            {units.map((u) => (
              <option key={u.id} value={u.id}>
                {u.code} — {u.name}
              </option>
            ))}
          </select>

          <select
            value={pageSize}
            onChange={(e) => {
              setPageSize(Number(e.target.value));
              setPageNumber(1);
            }}
            className="ml-auto rounded-lg border border-border bg-card px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value={5}>5 / page</option>
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
          </select>
        </div>

        <div className="card overflow-hidden">
          {loading ? (
            <div className="space-y-2 p-4">
              {Array.from({ length: 8 }).map((_, i) => (
                <div key={i} className="h-9 animate-pulse rounded bg-muted" />
              ))}
            </div>
          ) : (
            <table className="w-full text-sm">
              <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th className="px-4 py-2.5 font-medium">Base Unit</th>
                  <th className="px-4 py-2.5 font-medium">Sub Unit</th>
                  <th className="px-4 py-2.5 text-right font-medium">
                    Sub Value
                  </th>
                  <th className="px-4 py-2.5 text-right font-medium">
                    Base Value
                  </th>
                  <th className="px-4 py-2.5 font-medium">Meaning</th>
                  <th className="px-4 py-2.5 text-right font-medium">
                    Actions
                  </th>
                </tr>
              </thead>

              <tbody>
                {items.map((item, i) => (
                  <tr key={item.id} className={i % 2 ? "bg-muted/20" : ""}>
                    <td className="px-4 py-2.5">
                      <div className="font-medium">{item.unitName}</div>
                      <div className="font-mono text-xs text-muted-foreground">
                        {item.unitCode}
                      </div>
                    </td>

                    <td className="px-4 py-2.5">
                      <div className="font-medium">
                        {getUnitName(item.subUnitOfMeasureId)}
                      </div>
                      <div className="font-mono text-xs text-muted-foreground">
                        {getUnitCode(item.subUnitOfMeasureId)}
                      </div>
                    </td>

                    <td className="px-4 py-2.5 text-right tabular-nums">
                      {item.subUnitValue}
                    </td>

                    <td className="px-4 py-2.5 text-right tabular-nums">
                      {item.baseUnitValue}
                    </td>

                    <td className="px-4 py-2.5 text-muted-foreground">
                      {item.subUnitValue} {getUnitCode(item.subUnitOfMeasureId)}{" "}
                      = {item.baseUnitValue} {item.unitCode}
                    </td>

                    <td className="px-4 py-2.5 text-right">
                      <div className="flex justify-end gap-2">
                        <button
                          onClick={() => openEdit(item)}
                          className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted"
                        >
                          Edit
                        </button>

                        <button
                          disabled={busyId === item.id}
                          onClick={() => remove(item)}
                          className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-status-error hover:bg-muted disabled:opacity-50"
                        >
                          Remove
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}

                {items.length === 0 && (
                  <tr>
                    <td
                      colSpan={6}
                      className="px-4 py-10 text-center text-muted-foreground"
                    >
                      No conversion records found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={start} to={end} setPage={setPageNumber} noun="conversions" />
      </div>

      {open && (
        <div
          className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 p-4"
          onClick={() => !submitting && setOpen(false)}
        >
          <div
            className="w-full max-w-lg rounded-xl bg-card p-6 shadow-2xl"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="mb-4 flex items-start justify-between">
              <h3 className="font-heading text-lg font-bold">
                {form.id ? "Edit conversion" : "New conversion"}
              </h3>

              <button
                onClick={() => !submitting && setOpen(false)}
                className="rounded-lg p-1.5 text-muted-foreground hover:bg-muted"
              >
                <X className="size-5" />
              </button>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div className="col-span-2">
                <label className="mb-1 block text-sm font-semibold">
                  Base unit
                </label>

                <select
                  value={form.unitOfMeasureId}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      unitOfMeasureId: e.target.value,
                      subUnitOfMeasureId:
                        f.subUnitOfMeasureId === e.target.value
                          ? ""
                          : f.subUnitOfMeasureId,
                    }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                >
                  <option value="">Select base unit</option>

                  {units.map((u) => (
                    <option key={u.id} value={u.id}>
                      {u.code} — {u.name}
                    </option>
                  ))}
                </select>
              </div>

              <div className="col-span-2">
                <label className="mb-1 block text-sm font-semibold">
                  Sub unit
                </label>

                <select
                  value={form.subUnitOfMeasureId}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      subUnitOfMeasureId: e.target.value,
                    }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                >
                  <option value="">Select sub unit</option>

                  {units
                    .filter((u) => u.id !== form.unitOfMeasureId)
                    .map((u) => (
                      <option key={u.id} value={u.id}>
                        {u.code} — {u.name}
                      </option>
                    ))}
                </select>
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">
                  Sub unit value
                </label>

                <input
                  value={form.subUnitValue}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      subUnitValue: e.target.value.replace(/[^0-9.]/g, ""),
                    }))
                  }
                  inputMode="decimal"
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 tabular-nums focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">
                  Base unit value
                </label>

                <input
                  value={form.baseUnitValue}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      baseUnitValue: e.target.value.replace(/[^0-9.]/g, ""),
                    }))
                  }
                  inputMode="decimal"
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 tabular-nums focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>

              <div className="col-span-2 rounded-lg bg-muted/40 p-3 text-xs text-muted-foreground">
                Example: Base unit = G, Sub unit = KG, Sub value = 1, Base value
                = 1000. Meaning: 1 KG = 1000 G.
              </div>
            </div>

            <div className="mt-6 flex gap-2">
              <button
                onClick={() => setOpen(false)}
                disabled={submitting}
                className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50"
              >
                Cancel
              </button>

              <button
                onClick={save}
                disabled={submitting}
                className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50"
              >
                {submitting
                  ? "Saving…"
                  : form.id
                    ? "Save changes"
                    : "Create conversion"}
              </button>
            </div>
          </div>
        </div>
      )}

      {toast && (
        <div className="fixed bottom-12 left-1/2 z-[70] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">
          {toast}
        </div>
      )}
    </>
  );
}

function extractError(error: unknown, fallback: string) {
  if (error instanceof Error) return error.message || fallback;
  return fallback;
}
