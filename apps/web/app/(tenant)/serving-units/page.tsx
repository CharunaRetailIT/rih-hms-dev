"use client";

import { useEffect, useState } from "react";
import { Topbar } from "@/components/app-shell/Topbar";
import { apiClient } from "@/lib/api-client";
import { Pagination } from "@/components/ui/Pagination";
import { Plus, Search, X } from "lucide-react";

type ServingUnit = {
  id: string;
  code: string;
  name: string;
  sortOrder: number;
  isActive: boolean;
};

type PagedResult<T> = {
  data: T[];
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
  sortOrder: string;
  isActive: boolean;
};

const emptyForm: Form = {
  id: null,
  code: "",
  name: "",
  sortOrder: "0",
  isActive: true,
};

function extractError(e: unknown, fallback: string) {
  return e instanceof Error ? e.message : fallback;
}

export default function ServingUnitsPage() {
  const [items, setItems] = useState<ServingUnit[]>([]);

  const [loading, setLoading] = useState(true);
  const [toast, setToast] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("");

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const [open, setOpen] = useState(false);
  const [form, setForm] = useState<Form>(emptyForm);
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);
  const [busyId, setBusyId] = useState<string | null>(null);

  function flash(message: string) {
    setToast(message);
    window.setTimeout(() => setToast(null), 3500);
  }

  async function load() {
    setLoading(true);
    setError(null);

    try {
      const params = new URLSearchParams();
      params.set("pageNumber", String(pageNumber));
      params.set("pageSize", String(pageSize));

      if (search.trim()) params.set("search", search.trim());
      if (status) params.set("isActive", status);

      const result = await apiClient<PagedResult<ServingUnit>>(
        `/api/v1/serving-units/paged?${params.toString()}`,
      );

      setItems(result.data);
      setTotalCount(result.pagination.totalCount);
      setTotalPages(result.pagination.totalPages);
    } catch (e) {
      setError(extractError(e, "Could not load serving units."));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, [pageNumber, pageSize, status]);

  function applySearch() {
    setPageNumber(1);
    void load();
  }

  function openNew() {
    setForm(emptyForm);
    setFormErrors({});
    setOpen(true);
  }

  function openEdit(item: ServingUnit) {
    setForm({
      id: item.id,
      code: item.code,
      name: item.name,
      sortOrder: String(item.sortOrder),
      isActive: item.isActive,
    });

    setFormErrors({});
    setOpen(true);
  }

  function validate() {
    const e: Record<string, string> = {};

    if (!form.code.trim()) e.code = "Code is required.";
    if (!form.name.trim()) e.name = "Name is required.";

    const sort = Number(form.sortOrder);
    if (Number.isNaN(sort) || sort < 0) e.sortOrder = "Enter valid sort order.";

    setFormErrors(e);
    return Object.keys(e).length === 0;
  }

  async function submit() {
    if (!validate()) return;

    setSubmitting(true);

    const payload = {
      id: form.id,
      code: form.code.trim(),
      name: form.name.trim(),
      sortOrder: Number(form.sortOrder) || 0,
      isActive: form.isActive,
    };

    try {
      await apiClient("/api/v1/serving-units", {
        method: "PUT",
        body: JSON.stringify(payload),
      });

      setOpen(false);
      flash(form.id ? "Serving unit updated." : "Serving unit created.");
      await load();
    } catch (e) {
      flash(extractError(e, "Could not save serving unit."));
    } finally {
      setSubmitting(false);
    }
  }

  async function remove(item: ServingUnit) {
    setBusyId(item.id);

    try {
      await apiClient(`/api/v1/serving-units/${item.id}`, {
        method: "DELETE",
      });

      flash(`${item.name} removed.`);
      await load();
    } catch (e) {
      flash(extractError(e, "Could not remove serving unit."));
    } finally {
      setBusyId(null);
    }
  }

  const from = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to = Math.min(pageNumber * pageSize, totalCount);

  return (
    <>
      <Topbar title="Serving Units" subtitle="Inventory master files" />

      {toast && (
        <div className="fixed right-5 top-5 z-[80] rounded-lg bg-slate-900 px-4 py-2 text-sm font-medium text-white shadow-xl">
          {toast}
        </div>
      )}

      <div className="p-6">
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Serving Units</h2>
            <p className="text-sm text-muted-foreground">
              {totalCount} serving units configured for products and variants
            </p>
          </div>

          <button
            onClick={openNew}
            className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark"
          >
            <Plus className="size-4" />
            New serving unit
          </button>
        </div>

        <div className="mb-4 flex flex-wrap items-center gap-2">
          <div className="relative">
            <Search className="absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") applySearch();
              }}
              placeholder="Search code or name"
              className="w-72 rounded-lg border border-border bg-card py-2 pl-8 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
            />
          </div>

          <button
            onClick={applySearch}
            className="rounded-lg border border-border bg-card px-4 py-2 text-sm font-medium hover:bg-muted"
          >
            Search
          </button>

          <select
            value={status}
            onChange={(e) => {
              setStatus(e.target.value);
              setPageNumber(1);
            }}
            className="rounded-lg border border-border bg-card px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">All statuses</option>
            <option value="true">Active</option>
            <option value="false">Inactive</option>
          </select>

          <select
            value={pageSize}
            onChange={(e) => {
              setPageSize(Number(e.target.value));
              setPageNumber(1);
            }}
            className="ml-auto rounded-lg border border-border bg-card px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value={10}>10 rows</option>
            <option value={25}>25 rows</option>
            <option value={50}>50 rows</option>
            <option value={100}>100 rows</option>
          </select>
        </div>

        <div className="card overflow-hidden">
          {loading ? (
            <div className="space-y-2 p-4">
              {Array.from({ length: 8 }).map((_, i) => (
                <div key={i} className="h-9 animate-pulse rounded bg-muted" />
              ))}
            </div>
          ) : error ? (
            <div className="p-6 text-sm text-status-error">{error}</div>
          ) : (
            <table className="w-full text-sm">
              <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th className="px-4 py-2.5 font-medium">Code</th>
                  <th className="px-4 py-2.5 font-medium">Name</th>
                  <th className="px-4 py-2.5 text-right font-medium">Sort</th>
                  <th className="px-4 py-2.5 font-medium">Status</th>
                  <th className="px-4 py-2.5 text-right font-medium">
                    Actions
                  </th>
                </tr>
              </thead>

              <tbody>
                {items.map((item, i) => (
                  <tr key={item.id} className={i % 2 ? "bg-muted/20" : ""}>
                    <td className="px-4 py-2.5 font-mono text-xs text-muted-foreground">
                      {item.code}
                    </td>

                    <td className="px-4 py-2.5 font-medium">{item.name}</td>

                    <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">
                      {item.sortOrder}
                    </td>

                    <td className="px-4 py-2.5">
                      <span
                        className={`pill ${item.isActive ? "pill-paid" : "pill-void"}`}
                      >
                        {item.isActive ? "Active" : "Inactive"}
                      </span>
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
                      colSpan={5}
                      className="px-4 py-10 text-center text-muted-foreground"
                    >
                      No serving units found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from} to={to} setPage={setPageNumber} noun="serving units" />
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
                  {form.id ? "Edit serving unit" : "New serving unit"}
                </h3>
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
                <label className="mb-1 block text-sm font-semibold text-slate-700">
                  Code
                </label>
                <input
                  value={form.code}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      code: e.target.value.toUpperCase(),
                    }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 font-mono text-sm focus:border-primary focus:ring-2 focus:ring-primary/20"
                  placeholder="FULL"
                />
                {formErrors.code && (
                  <p className="mt-1 text-xs text-status-error">
                    {formErrors.code}
                  </p>
                )}
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold text-slate-700">
                  Sort order
                </label>
                <input
                  value={form.sortOrder}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      sortOrder: e.target.value.replace(/[^0-9]/g, ""),
                    }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 tabular-nums focus:border-primary focus:ring-2 focus:ring-primary/20"
                  placeholder="0"
                />
                {formErrors.sortOrder && (
                  <p className="mt-1 text-xs text-status-error">
                    {formErrors.sortOrder}
                  </p>
                )}
              </div>

              <div className="col-span-2">
                <label className="mb-1 block text-sm font-semibold text-slate-700">
                  Name
                </label>
                <input
                  value={form.name}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, name: e.target.value }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                  placeholder="Full Portion"
                />
                {formErrors.name && (
                  <p className="mt-1 text-xs text-status-error">
                    {formErrors.name}
                  </p>
                )}
              </div>

              <label className="col-span-2 mt-1 flex items-center gap-2 text-sm font-medium">
                <input
                  type="checkbox"
                  checked={form.isActive}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, isActive: e.target.checked }))
                  }
                  className="size-4 rounded border-border"
                />
                Active
              </label>
            </div>

            <div className="mt-5 flex justify-end gap-2">
              <button
                disabled={submitting}
                onClick={() => setOpen(false)}
                className="rounded-lg border border-border bg-card px-4 py-2 text-sm font-medium hover:bg-muted disabled:opacity-50"
              >
                Cancel
              </button>

              <button
                disabled={submitting}
                onClick={submit}
                className="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark disabled:opacity-50"
              >
                {submitting ? "Saving..." : "Save serving unit"}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
