"use client";

import { useEffect, useState } from "react";
import { Topbar } from "@/components/app-shell/Topbar";
import { apiClient } from "@/lib/api-client";
import { Pagination } from "@/components/ui/Pagination";
import { Plus, Search, X } from "lucide-react";

type PriceLevel = {
  id: string;
  locationId: string | null;
  code: string;
  name: string;
  isDefault: boolean;
  appliesToOrderType: string | null;
  sortOrder: number;
  isActive: boolean;
};

type Location = {
  id: string;
  code: string;
  name: string;
  city: string | null;
  locationType: string;
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
  locationId: string;
  code: string;
  name: string;
  appliesToOrderType: string;
  sortOrder: string;
  isDefault: boolean;
  isActive: boolean;
};

const emptyForm: Form = {
  id: null,
  locationId: "",
  code: "",
  name: "",
  appliesToOrderType: "",
  sortOrder: "0",
  isDefault: false,
  isActive: true,
};

const orderTypes = [
  { value: "", label: "None" },
  { value: "dine_in", label: "Dine In" },
  { value: "takeaway", label: "Takeaway" },
  { value: "delivery", label: "Delivery" },
  { value: "online", label: "Online" },
  { value: "third_party", label: "Third Party" },
  { value: "wholesale", label: "Wholesale" },
];

function extractError(e: unknown, fallback: string) {
  return e instanceof Error ? e.message : fallback;
}

export default function PriceLevelsPage() {
  const [items, setItems] = useState<PriceLevel[]>([]);
  const [locations, setLocations] = useState<Location[]>([]);

  const [loading, setLoading] = useState(true);
  const [toast, setToast] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const [search, setSearch] = useState("");
  const [locationId, setLocationId] = useState("");
  const [status, setStatus] = useState("");
  const [appliesToOrderType, setAppliesToOrderType] = useState("");

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

  function locationName(id: string | null) {
    if (!id) return "Global";
    return locations.find((x) => x.id === id)?.name ?? "Unknown";
  }

  function orderTypeLabel(value: string | null) {
    if (!value) return "None";
    return orderTypes.find((x) => x.value === value)?.label ?? value;
  }

  async function load() {
    setLoading(true);
    setError(null);

    try {
      const params = new URLSearchParams();
      params.set("pageNumber", String(pageNumber));
      params.set("pageSize", String(pageSize));

      if (search.trim()) params.set("search", search.trim());
      if (locationId) params.set("locationId", locationId);
      if (status) params.set("isActive", status);
      if (appliesToOrderType)
        params.set("appliesToOrderType", appliesToOrderType);

      const [result, locs] = await Promise.all([
        apiClient<PagedResult<PriceLevel>>(
          `/api/v1/price-levels/paged?${params.toString()}`,
        ),
        apiClient<Location[]>("/api/v1/locations").catch(() => []),
      ]);

      setItems(result.data);
      setTotalCount(result.pagination.totalCount);
      setTotalPages(result.pagination.totalPages);
      setLocations(locs);
    } catch (e) {
      setError(extractError(e, "Could not load price levels."));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, [pageNumber, pageSize, locationId, status, appliesToOrderType]);

  function applySearch() {
    setPageNumber(1);
    void load();
  }

  function openNew() {
    setForm(emptyForm);
    setFormErrors({});
    setOpen(true);
  }

  function openEdit(item: PriceLevel) {
    setForm({
      id: item.id,
      locationId: item.locationId ?? "",
      code: item.code,
      name: item.name,
      appliesToOrderType: item.appliesToOrderType ?? "",
      sortOrder: String(item.sortOrder),
      isDefault: item.isDefault,
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
    if (Number.isNaN(sort) || sort < 0) {
      e.sortOrder = "Enter valid sort order.";
    }

    setFormErrors(e);
    return Object.keys(e).length === 0;
  }

  async function submit() {
    if (!validate()) return;

    setSubmitting(true);

    const payload = {
      id: form.id,
      locationId: form.locationId || null,
      code: form.code.trim(),
      name: form.name.trim(),
      isDefault: form.isDefault,
      appliesToOrderType: form.appliesToOrderType || null,
      sortOrder: Number(form.sortOrder) || 0,
      isActive: form.isActive,
    };

    try {
      await apiClient("/api/v1/price-levels", {
        method: "PUT",
        body: JSON.stringify(payload),
      });

      setOpen(false);
      flash(form.id ? "Price level updated." : "Price level created.");
      await load();
    } catch (e) {
      flash(extractError(e, "Could not save price level."));
    } finally {
      setSubmitting(false);
    }
  }

  async function remove(item: PriceLevel) {
    setBusyId(item.id);

    try {
      await apiClient(`/api/v1/price-levels/${item.id}`, {
        method: "DELETE",
      });

      flash(`${item.name} removed.`);
      await load();
    } catch (e) {
      flash(extractError(e, "Could not remove price level."));
    } finally {
      setBusyId(null);
    }
  }

  const from = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to = Math.min(pageNumber * pageSize, totalCount);

  return (
    <>
      <Topbar title="Price Levels" subtitle="Pricing master files" />

      {toast && (
        <div className="fixed right-5 top-5 z-[80] rounded-lg bg-slate-900 px-4 py-2 text-sm font-medium text-white shadow-xl">
          {toast}
        </div>
      )}

      <div className="p-6">
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Price Levels</h2>
            <p className="text-sm text-muted-foreground">
              {totalCount} price levels configured for dine-in, takeaway,
              delivery and branch pricing
            </p>
          </div>

          <button
            onClick={openNew}
            className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark"
          >
            <Plus className="size-4" />
            New price level
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
              placeholder="Search code, name or type"
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
            value={locationId}
            onChange={(e) => {
              setLocationId(e.target.value);
              setPageNumber(1);
            }}
            className="rounded-lg border border-border bg-card px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">All locations</option>
            {locations.map((l) => (
              <option key={l.id} value={l.id}>
                {l.name}
              </option>
            ))}
          </select>

          <select
            value={appliesToOrderType}
            onChange={(e) => {
              setAppliesToOrderType(e.target.value);
              setPageNumber(1);
            }}
            className="rounded-lg border border-border bg-card px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">All order types</option>
            {orderTypes
              .filter((x) => x.value)
              .map((x) => (
                <option key={x.value} value={x.value}>
                  {x.label}
                </option>
              ))}
          </select>

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
                  <th className="px-4 py-2.5 font-medium">Order Type</th>
                  <th className="px-4 py-2.5 font-medium">Location</th>
                  <th className="px-4 py-2.5 text-right font-medium">Sort</th>
                  <th className="px-4 py-2.5 font-medium">Default</th>
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

                    <td className="px-4 py-2.5">
                      <span className="pill pill-idle">
                        {orderTypeLabel(item.appliesToOrderType)}
                      </span>
                    </td>

                    <td className="px-4 py-2.5">
                      <span className="pill pill-idle">
                        {locationName(item.locationId)}
                      </span>
                    </td>

                    <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">
                      {item.sortOrder}
                    </td>

                    <td className="px-4 py-2.5">
                      <span
                        className={`pill ${item.isDefault ? "pill-paid" : "pill-idle"}`}
                      >
                        {item.isDefault ? "Default" : "No"}
                      </span>
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
                          disabled={busyId === item.id || item.isDefault}
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
                      colSpan={8}
                      className="px-4 py-10 text-center text-muted-foreground"
                    >
                      No price levels found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from} to={to} setPage={setPageNumber} noun="price levels" />
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
                  {form.id ? "Edit price level" : "New price level"}
                </h3>
                <p className="text-sm text-muted-foreground">
                  Use price levels for dine-in, takeaway, delivery, Uber, PickMe
                  or branch pricing.
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
                  placeholder="DINEIN"
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
                  placeholder="Dine In"
                />
                {formErrors.name && (
                  <p className="mt-1 text-xs text-status-error">
                    {formErrors.name}
                  </p>
                )}
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold text-slate-700">
                  Order type
                </label>
                <select
                  value={form.appliesToOrderType}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      appliesToOrderType: e.target.value,
                    }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                >
                  {orderTypes.map((x) => (
                    <option key={x.value} value={x.value}>
                      {x.label}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold text-slate-700">
                  Location
                </label>
                <select
                  value={form.locationId}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, locationId: e.target.value }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                >
                  <option value="">Select location</option>
                  {locations.map((l) => (
                    <option key={l.id} value={l.id}>
                      {l.name}
                    </option>
                  ))}
                </select>
              </div>

              <label className="col-span-2 mt-1 flex items-center gap-2 text-sm font-medium">
                <input
                  type="checkbox"
                  checked={form.isDefault}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, isDefault: e.target.checked }))
                  }
                  className="size-4 rounded border-border"
                />
                Default price level
              </label>

              <label className="col-span-2 flex items-center gap-2 text-sm font-medium">
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
                {submitting ? "Saving..." : "Save price level"}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
