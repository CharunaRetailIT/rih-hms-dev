"use client";

import { useEffect, useState } from "react";
import { Topbar } from "@/components/app-shell/Topbar";
import { apiClient } from "@/lib/api-client";
import { Pagination } from "@/components/ui/Pagination";
import { Plus, Search, X, Ruler } from "lucide-react";

type UnitOfMeasure = {
  id: string;
  code: string;
  name: string;
  symbol: string | null;
  isBaseUnit: boolean;
  dimension: "mass" | "volume" | "count";
  factorToBase: number;
};

type PagedResponse = {
  data: UnitOfMeasure[];
  pagination: {
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
  };
};

type Form = {
  id: string | null;
  code: string;
  name: string;
  symbol: string;
  dimension: "mass" | "volume" | "count";
  isBaseUnit: boolean;
  factorToBase: string;
};

const emptyForm: Form = {
  id: null,
  code: "",
  name: "",
  symbol: "",
  dimension: "count",
  isBaseUnit: false,
  factorToBase: "1",
};

export default function UnitOfMeasurePage() {
  const [items, setItems] = useState<UnitOfMeasure[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [dimension, setDimension] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const [open, setOpen] = useState(false);
  const [form, setForm] = useState<Form>(emptyForm);
  const [toast, setToast] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  function flash(message: string) {
    setToast(message);
    window.setTimeout(() => setToast(null), 3000);
  }

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set("pageNumber", String(pageNumber));
      params.set("pageSize", String(pageSize));
      if (search.trim()) params.set("search", search.trim());
      if (dimension) params.set("dimension", dimension);

      const res = await apiClient<PagedResponse>(`/api/v1/units-of-measure/paged?${params.toString()}`);
      setItems(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages || 1);
    } catch (e) {
      flash(extractError(e, "Could not load units."));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, [pageNumber, pageSize, dimension]);

  useEffect(() => {
    const t = window.setTimeout(() => {
      setPageNumber(1);
      void load();
    }, 350);
    return () => window.clearTimeout(t);
  }, [search]);

  function openNew() {
    setForm(emptyForm);
    setOpen(true);
  }

  function openEdit(item: UnitOfMeasure) {
    setForm({
      id: item.id,
      code: item.code,
      name: item.name,
      symbol: item.symbol ?? "",
      dimension: item.dimension,
      isBaseUnit: item.isBaseUnit,
      factorToBase: String(item.factorToBase),
    });

    setOpen(true);
  }

  async function submit() {
    if (!form.code.trim()) {
      flash("Code is required.");
      return;
    }

    if (!form.name.trim()) {
      flash("Name is required.");
      return;
    }

    const factor = Number(form.factorToBase);

    if (Number.isNaN(factor) || factor <= 0) {
      flash("Factor to base must be greater than zero.");
      return;
    }

    setSubmitting(true);

    try {
      await apiClient("/api/v1/units-of-measure", {
        method: "PUT",
        body: JSON.stringify({
          id: form.id,
          code: form.code.trim(),
          name: form.name.trim(),
          symbol: form.symbol.trim() || null,
          dimension: form.dimension,
          isBaseUnit: form.isBaseUnit,
          factorToBase: form.isBaseUnit ? 1 : factor,
        }),
      });

      setOpen(false);
      flash(form.id ? "Unit updated." : "Unit created.");
      await load();
    } catch (e) {
      flash(extractError(e, "Could not save unit."));
    } finally {
      setSubmitting(false);
    }
  }

  async function remove(item: UnitOfMeasure) {
    setBusyId(item.id);

    try {
      await apiClient(`/api/v1/units-of-measure/${item.id}`, {
        method: "DELETE",
      });

      flash(`${item.code} removed.`);
      await load();
    } catch (e) {
      flash(extractError(e, "Could not remove unit."));
    } finally {
      setBusyId(null);
    }
  }

  const from = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to = Math.min(pageNumber * pageSize, totalCount);

  return (
    <>
      <Topbar title="Master Data" subtitle="Unit of Measure" />

      <div className="p-6">
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Unit of Measure</h2>
            <p className="text-sm text-muted-foreground">
              Manage stock, purchase, and recipe quantity units.
            </p>
          </div>

          <button
            onClick={openNew}
            className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark"
          >
            <Plus className="size-4" />
            New unit
          </button>
        </div>

        <div className="mb-4 flex flex-wrap items-center gap-3">
          <select
            value={dimension}
            onChange={(e) => {
              setDimension(e.target.value);
              setPageNumber(1);
            }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm capitalize focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">All dimensions</option>
            <option value="mass">Mass</option>
            <option value="volume">Volume</option>
            <option value="count">Count</option>
          </select>

          <div className="relative">
            <Search className="absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search code or name"
              className="rounded-lg border border-border bg-card py-1.5 pl-8 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
            />
          </div>

          <select
            value={pageSize}
            onChange={(e) => {
              setPageSize(Number(e.target.value));
              setPageNumber(1);
            }}
            className="ml-auto rounded-lg border border-border bg-card px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
            <option value={100}>100 / page</option>
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
                  <th className="px-4 py-2.5 font-medium">Unit</th>
                  <th className="px-4 py-2.5 font-medium">Code</th>
                  <th className="px-4 py-2.5 font-medium">Symbol</th>
                  <th className="px-4 py-2.5 font-medium">Dimension</th>
                  <th className="px-4 py-2.5 text-right font-medium">
                    Factor to Base
                  </th>
                  <th className="px-4 py-2.5 font-medium">Type</th>
                  <th className="px-4 py-2.5 text-right font-medium">
                    Actions
                  </th>
                </tr>
              </thead>

              <tbody>
                {items.map((item, index) => (
                  <tr key={item.id} className={index % 2 ? "bg-muted/20" : ""}>
                    <td className="px-4 py-2.5">
                      <div className="flex items-center gap-2.5">
                        <div className="flex size-8 items-center justify-center rounded-lg bg-primary-tint text-primary">
                          <Ruler className="size-4" />
                        </div>
                        <span className="font-medium">{item.name}</span>
                      </div>
                    </td>

                    <td className="px-4 py-2.5 font-mono text-xs text-muted-foreground">
                      {item.code}
                    </td>

                    <td className="px-4 py-2.5">{item.symbol || "—"}</td>

                    <td className="px-4 py-2.5">
                      <span className="pill pill-idle capitalize">
                        {item.dimension}
                      </span>
                    </td>

                    <td className="px-4 py-2.5 text-right tabular-nums">
                      {item.factorToBase}
                    </td>

                    <td className="px-4 py-2.5">
                      {item.isBaseUnit ? (
                        <span className="pill bg-primary text-primary-foreground">
                          Base unit
                        </span>
                      ) : (
                        <span className="pill pill-idle">Conversion unit</span>
                      )}
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
                      colSpan={7}
                      className="px-4 py-10 text-center text-muted-foreground"
                    >
                      No units found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from} to={to} setPage={setPageNumber} noun="units" />
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
              <div>
                <h3 className="font-heading text-lg font-bold">
                  {form.id ? "Edit unit" : "New unit"}
                </h3>
                <p className="text-sm text-muted-foreground">
                  Example: KG, G, L, ML, EA, PCS.
                </p>
              </div>

              <button
                onClick={() => !submitting && setOpen(false)}
                className="rounded-lg p-1.5 text-muted-foreground hover:bg-muted"
              >
                <X className="size-5" />
              </button>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="mb-1 block text-sm font-semibold">Code</label>
                <input
                  value={form.code}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      code: e.target.value.toUpperCase(),
                    }))
                  }
                  placeholder="KG"
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 font-mono uppercase focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">
                  Symbol
                </label>
                <input
                  value={form.symbol}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, symbol: e.target.value }))
                  }
                  placeholder="kg"
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>

              <div className="col-span-2">
                <label className="mb-1 block text-sm font-semibold">Name</label>
                <input
                  value={form.name}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, name: e.target.value }))
                  }
                  placeholder="Kilogram"
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">
                  Dimension
                </label>
                <select
                  value={form.dimension}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      dimension: e.target.value as Form["dimension"],
                    }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                >
                  <option value="count">Count</option>
                  <option value="mass">Mass</option>
                  <option value="volume">Volume</option>
                </select>
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">
                  Factor to Base
                </label>
                <input
                  value={form.factorToBase}
                  disabled={form.isBaseUnit}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      factorToBase: e.target.value.replace(/[^0-9.]/g, ""),
                    }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 tabular-nums disabled:bg-muted focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>

              <div className="col-span-2 rounded-lg bg-muted/40 p-3 text-xs text-muted-foreground">
                <p>
                  Mass base unit can be gram. Volume base unit can be
                  milliliter. Count base unit can be each.
                </p>
                <p className="mt-1">
                  Example: 1 KG = 1000 base units if base is gram.
                </p>
              </div>

              <label className="col-span-2 flex items-center gap-2 text-sm">
                <input
                  type="checkbox"
                  checked={form.isBaseUnit}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      isBaseUnit: e.target.checked,
                      factorToBase: e.target.checked ? "1" : f.factorToBase,
                    }))
                  }
                  className="size-4 rounded border-border text-primary"
                />
                This is the base unit for this dimension
              </label>
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
                onClick={submit}
                disabled={submitting}
                className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50"
              >
                {submitting
                  ? "Saving…"
                  : form.id
                    ? "Save changes"
                    : "Create unit"}
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
