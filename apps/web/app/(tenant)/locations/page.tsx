"use client";

import { useEffect, useState } from "react";
import { Topbar } from "@/components/app-shell/Topbar";
import { apiClient } from "@/lib/api-client";
import { Pagination } from "@/components/ui/Pagination";
import { MapPin, Plus, Search, X } from "lucide-react";
import { COUNTRIES, TIME_ZONES } from "@/lib/regions";

type Location = {
  id: string;
  code: string;
  name: string;
  addressLine1: string;
  addressLine2: string | null;
  city: string;
  countryCode: string;
  timeZone: string;
  currency: string;
  phoneE164: string | null;
  isActive: boolean;
  locationType: string;
  canSell: boolean;
  canProduce: boolean;
  canStock: boolean;
  vatRegistrationNumber: string | null;
  defaultPrepMinutes: number;
};

type PagedResponse = {
  data: Location[];
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
  addressLine1: string;
  addressLine2: string;
  city: string;
  countryCode: string;
  timeZone: string;
  currency: string;
  phoneE164: string;
  isActive: boolean;
  locationType: string;
  canSell: boolean;
  canProduce: boolean;
  canStock: boolean;
  vatRegistrationNumber: string;
  defaultPrepMinutes: string;
};

const emptyForm: Form = {
  id: null,
  code: "",
  name: "",
  addressLine1: "",
  addressLine2: "",
  city: "",
  countryCode: "LK",
  timeZone: "Asia/Colombo",
  currency: "LKR",
  phoneE164: "",
  isActive: true,
  locationType: "outlet",
  canSell: true,
  canProduce: false,
  canStock: true,
  vatRegistrationNumber: "",
  defaultPrepMinutes: "20",
};

const LOCATION_TYPES = [
  { value: "outlet", label: "Outlet" },
  { value: "head_office", label: "Head Office" },
  { value: "central_kitchen", label: "Central Kitchen" },
  { value: "warehouse", label: "Warehouse" },
];

const CURRENCIES = ["LKR", "USD", "EUR", "GBP", "INR", "AUD", "AED", "SGD", "MVR"];

export default function LocationsPage() {
  const [items, setItems] = useState<Location[]>([]);
  const [loading, setLoading] = useState(true);

  const [search, setSearch] = useState("");
  const [locationType, setLocationType] = useState("");
  const [isActive, setIsActive] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const [open, setOpen] = useState(false);
  const [form, setForm] = useState<Form>(emptyForm);
  const [submitting, setSubmitting] = useState(false);
  const [busyId, setBusyId] = useState<string | null>(null);

  const [toast, setToast] = useState<string | null>(null);

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
      if (locationType) params.set("locationType", locationType);
      if (isActive) params.set("isActive", isActive);

      const res = await apiClient<PagedResponse>(
        `/api/v1/locations/paged?${params.toString()}`,
      );

      setItems(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages || 1);
    } catch (e) {
      flash(extractError(e, "Could not load locations."));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, [pageNumber, pageSize, locationType, isActive]);

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

  function openEdit(item: Location) {
    setForm({
      id: item.id,
      code: item.code,
      name: item.name,
      addressLine1: item.addressLine1,
      addressLine2: item.addressLine2 ?? "",
      city: item.city,
      countryCode: item.countryCode,
      timeZone: item.timeZone,
      currency: item.currency,
      phoneE164: item.phoneE164 ?? "",
      isActive: item.isActive,
      locationType: item.locationType,
      canSell: item.canSell,
      canProduce: item.canProduce,
      canStock: item.canStock,
      vatRegistrationNumber: item.vatRegistrationNumber ?? "",
      defaultPrepMinutes: String(item.defaultPrepMinutes),
    });

    setOpen(true);
  }

  async function save() {
    if (!form.code.trim()) {
      flash("Code is required.");
      return;
    }

    if (!form.name.trim()) {
      flash("Name is required.");
      return;
    }

    if (!form.addressLine1.trim()) {
      flash("Address line 1 is required.");
      return;
    }

    if (!form.city.trim()) {
      flash("City is required.");
      return;
    }

    const defaultPrepMinutes = Number(form.defaultPrepMinutes);

    if (Number.isNaN(defaultPrepMinutes) || defaultPrepMinutes < 0) {
      flash("Default prep minutes must be valid.");
      return;
    }

    setSubmitting(true);

    try {
      await apiClient("/api/v1/locations", {
        method: "PUT",
        body: JSON.stringify({
          id: form.id,
          code: form.code.trim(),
          name: form.name.trim(),
          addressLine1: form.addressLine1.trim(),
          addressLine2: form.addressLine2.trim() || null,
          city: form.city.trim(),
          countryCode: form.countryCode.trim() || "LK",
          timeZone: form.timeZone.trim() || "Asia/Colombo",
          currency: form.currency.trim() || "LKR",
          phoneE164: form.phoneE164.trim() || null,
          isActive: form.isActive,
          locationType: form.locationType,
          canSell: form.canSell,
          canProduce: form.canProduce,
          canStock: form.canStock,
          vatRegistrationNumber: form.vatRegistrationNumber.trim() || null,
          defaultPrepMinutes,
        }),
      });

      setOpen(false);
      flash(form.id ? "Location updated." : "Location created.");
      await load();
    } catch (e) {
      flash(extractError(e, "Could not save location."));
    } finally {
      setSubmitting(false);
    }
  }

  async function remove(item: Location) {
    setBusyId(item.id);

    try {
      await apiClient(`/api/v1/locations/${item.id}`, {
        method: "DELETE",
      });

      flash(`${item.name} removed.`);
      await load();
    } catch (e) {
      flash(extractError(e, "Could not remove location."));
    } finally {
      setBusyId(null);
    }
  }

  const from = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to = Math.min(pageNumber * pageSize, totalCount);

  return (
    <>
      <Topbar title="Master Data" subtitle="Locations" />

      <div className="p-6">
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Locations</h2>
            <p className="text-sm text-muted-foreground">
              Manage outlets, warehouses, central kitchens, and head office
              locations.
            </p>
          </div>

          <button
            onClick={openNew}
            className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark"
          >
            <Plus className="size-4" />
            New location
          </button>
        </div>

        <div className="mb-4 flex flex-wrap items-center gap-3">
          <div className="relative">
            <Search className="absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />

            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search locations"
              className="rounded-lg border border-border bg-card py-1.5 pl-8 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
            />
          </div>

          <select
            value={locationType}
            onChange={(e) => {
              setLocationType(e.target.value);
              setPageNumber(1);
            }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">All types</option>
            {LOCATION_TYPES.map((t) => (
              <option key={t.value} value={t.value}>
                {t.label}
              </option>
            ))}
          </select>

          <select
            value={isActive}
            onChange={(e) => {
              setIsActive(e.target.value);
              setPageNumber(1);
            }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">All status</option>
            <option value="true">Active</option>
            <option value="false">Inactive</option>
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
                  <th className="px-4 py-2.5 font-medium">Location</th>
                  <th className="px-4 py-2.5 font-medium">Type</th>
                  <th className="px-4 py-2.5 font-medium">City</th>
                  <th className="px-4 py-2.5 font-medium">Capabilities</th>
                  <th className="px-4 py-2.5 font-medium">Status</th>
                  <th className="px-4 py-2.5 text-right font-medium">
                    Actions
                  </th>
                </tr>
              </thead>

              <tbody>
                {items.map((item, i) => (
                  <tr key={item.id} className={i % 2 ? "bg-muted/20" : ""}>
                    <td className="px-4 py-2.5">
                      <div className="flex items-center gap-2.5">
                        <div className="flex size-8 items-center justify-center rounded-lg bg-primary-tint text-primary">
                          <MapPin className="size-4" />
                        </div>

                        <div>
                          <div className="font-medium">{item.name}</div>
                          <div className="font-mono text-xs text-muted-foreground">
                            {item.code}
                          </div>
                        </div>
                      </div>
                    </td>

                    <td className="px-4 py-2.5">
                      <span className="pill pill-idle">
                        {item.locationType.replace("_", " ")}
                      </span>
                    </td>

                    <td className="px-4 py-2.5">
                      <div>{item.city}</div>
                      <div className="text-xs text-muted-foreground">
                        {item.currency} · {item.countryCode}
                      </div>
                    </td>

                    <td className="px-4 py-2.5">
                      <div className="flex flex-wrap gap-1">
                        {item.canSell && (
                          <span className="pill pill-paid">Sell</span>
                        )}
                        {item.canProduce && (
                          <span className="pill pill-idle">Produce</span>
                        )}
                        {item.canStock && (
                          <span className="pill pill-idle">Stock</span>
                        )}
                      </div>
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
                      colSpan={6}
                      className="px-4 py-10 text-center text-muted-foreground"
                    >
                      No locations found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from} to={to} setPage={setPageNumber} noun="locations" />
      </div>

      {open && (
        <div
          className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 p-4"
          onClick={() => !submitting && setOpen(false)}
        >
          <div
            className="max-h-[90vh] w-full max-w-3xl overflow-y-auto rounded-xl bg-card p-6 shadow-2xl"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="mb-4 flex items-start justify-between">
              <h3 className="font-heading text-lg font-bold">
                {form.id ? "Edit location" : "New location"}
              </h3>

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
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 font-mono uppercase focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">Name</label>
                <input
                  value={form.name}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, name: e.target.value }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>

              <div className="col-span-2">
                <label className="mb-1 block text-sm font-semibold">
                  Address line 1
                </label>
                <input
                  value={form.addressLine1}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, addressLine1: e.target.value }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>

              <div className="col-span-2">
                <label className="mb-1 block text-sm font-semibold">
                  Address line 2
                </label>
                <input
                  value={form.addressLine2}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, addressLine2: e.target.value }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">City</label>
                <input
                  value={form.city}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, city: e.target.value }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">
                  Location type
                </label>
                <select
                  value={form.locationType}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, locationType: e.target.value }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                >
                  {LOCATION_TYPES.map((t) => (
                    <option key={t.value} value={t.value}>
                      {t.label}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">
                  Phone
                </label>
                <input
                  value={form.phoneE164}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, phoneE164: e.target.value }))
                  }
                  placeholder="+94771234567"
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">
                  VAT Reg No
                </label>
                <input
                  value={form.vatRegistrationNumber}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      vatRegistrationNumber: e.target.value,
                    }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">
                  Country
                </label>
                <select
                  value={form.countryCode}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      countryCode: e.target.value,
                    }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                >
                  <option value="">Select country…</option>
                  {COUNTRIES.map((c) => (
                    <option key={c.value} value={c.value}>
                      {c.label}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">
                  Currency
                </label>
                <select
                  value={form.currency}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      currency: e.target.value,
                    }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                >
                  <option value="">Select currency…</option>
                  {CURRENCIES.map((c) => (
                    <option key={c} value={c}>
                      {c}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">
                  Time zone
                </label>
                <select
                  value={form.timeZone}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, timeZone: e.target.value }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                >
                  <option value="">Select time zone…</option>
                  {TIME_ZONES.map((tz) => (
                    <option key={tz} value={tz}>
                      {tz}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">
                  Prep minutes
                </label>
                <input
                  value={form.defaultPrepMinutes}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      defaultPrepMinutes: e.target.value.replace(/[^0-9]/g, ""),
                    }))
                  }
                  inputMode="numeric"
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 tabular-nums focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>

              <div className="col-span-2 flex flex-wrap gap-5 pt-2">
                <label className="flex items-center gap-2 text-sm">
                  <input
                    type="checkbox"
                    checked={form.canSell}
                    onChange={(e) =>
                      setForm((f) => ({ ...f, canSell: e.target.checked }))
                    }
                    className="size-4 rounded border-border text-primary"
                  />
                  Can sell
                </label>

                <label className="flex items-center gap-2 text-sm">
                  <input
                    type="checkbox"
                    checked={form.canProduce}
                    onChange={(e) =>
                      setForm((f) => ({ ...f, canProduce: e.target.checked }))
                    }
                    className="size-4 rounded border-border text-primary"
                  />
                  Can produce
                </label>

                <label className="flex items-center gap-2 text-sm">
                  <input
                    type="checkbox"
                    checked={form.canStock}
                    onChange={(e) =>
                      setForm((f) => ({ ...f, canStock: e.target.checked }))
                    }
                    className="size-4 rounded border-border text-primary"
                  />
                  Can stock
                </label>

                <label className="flex items-center gap-2 text-sm">
                  <input
                    type="checkbox"
                    checked={form.isActive}
                    onChange={(e) =>
                      setForm((f) => ({ ...f, isActive: e.target.checked }))
                    }
                    className="size-4 rounded border-border text-primary"
                  />
                  Active
                </label>
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
                    : "Create location"}
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
