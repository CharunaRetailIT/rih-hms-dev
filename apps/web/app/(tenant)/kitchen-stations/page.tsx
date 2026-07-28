"use client";

import { useEffect, useState } from "react";
import { Topbar } from "@/components/app-shell/Topbar";
import { apiClient } from "@/lib/api-client";
import { Pagination } from "@/components/ui/Pagination";
import { Plus, Search, X } from "lucide-react";

type Location = {
  id: string;
  code: string;
  name: string;
  city: string | null;
};

type PrinterType = {
  id: string;
  code: string;
  name: string;
  sortOrder: number;
  isActive: boolean;
};

type KitchenStation = {
  id: string;
  locationId: string | null;
  printerTypeId: string;
  printerTypeCode: string;
  printerTypeName: string;
  code: string;
  name: string;
  printerName: string | null;
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

type StationForm = {
  id: string | null;
  locationId: string;
  printerTypeId: string;
  code: string;
  name: string;
  printerName: string;
  sortOrder: string;
  isActive: boolean;
};

type PrinterTypeForm = {
  id: string | null;
  code: string;
  name: string;
  sortOrder: string;
  isActive: boolean;
};

const emptyStationForm: StationForm = {
  id: null,
  locationId: "",
  printerTypeId: "",
  code: "",
  name: "",
  printerName: "",
  sortOrder: "0",
  isActive: true,
};

const emptyPrinterTypeForm: PrinterTypeForm = {
  id: null,
  code: "",
  name: "",
  sortOrder: "0",
  isActive: true,
};

function extractError(e: unknown, fallback: string) {
  return e instanceof Error ? e.message : fallback;
}

export default function KitchenStationsPage() {
  const [items, setItems] = useState<KitchenStation[]>([]);
  const [printerTypes, setPrinterTypes] = useState<PrinterType[]>([]);
  const [locations, setLocations] = useState<Location[]>([]);

  const [loading, setLoading] = useState(true);
  const [toast, setToast] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const [search, setSearch] = useState("");
  const [locationId, setLocationId] = useState("");
  const [printerTypeId, setPrinterTypeId] = useState("");
  const [status, setStatus] = useState("");

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const [stationOpen, setStationOpen] = useState(false);
  const [stationForm, setStationForm] = useState<StationForm>(emptyStationForm);
  const [stationErrors, setStationErrors] = useState<Record<string, string>>(
    {},
  );

  const [typeOpen, setTypeOpen] = useState(false);
  const [typeForm, setTypeForm] =
    useState<PrinterTypeForm>(emptyPrinterTypeForm);
  const [typeErrors, setTypeErrors] = useState<Record<string, string>>({});

  const [submitting, setSubmitting] = useState(false);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [typeBusyId, setTypeBusyId] = useState<string | null>(null);

  function flash(message: string) {
    setToast(message);
    window.setTimeout(() => setToast(null), 3500);
  }

  function locationName(id: string | null) {
    if (!id) return "Global";
    return locations.find((x) => x.id === id)?.name ?? "Unknown";
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
      if (printerTypeId) params.set("printerTypeId", printerTypeId);
      if (status) params.set("isActive", status);

      const [result, types, locs] = await Promise.all([
        apiClient<PagedResult<KitchenStation>>(
          `/api/v1/kitchen-stations/paged?${params.toString()}`,
        ),
        apiClient<PrinterType[]>("/api/v1/printer-types").catch(() => []),
        apiClient<Location[]>("/api/v1/locations").catch(() => []),
      ]);

      setItems(result.data);
      setTotalCount(result.pagination.totalCount);
      setTotalPages(result.pagination.totalPages);
      setPrinterTypes(types);
      setLocations(locs);
    } catch (e) {
      setError(extractError(e, "Could not load kitchen stations."));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, [pageNumber, pageSize, locationId, printerTypeId, status]);

  function applySearch() {
    setPageNumber(1);
    void load();
  }

  function openNewStation() {
    setStationForm({
      ...emptyStationForm,
      printerTypeId: printerTypes[0]?.id ?? "",
    });
    setStationErrors({});
    setStationOpen(true);
  }

  function openEditStation(item: KitchenStation) {
    setStationForm({
      id: item.id,
      locationId: item.locationId ?? "",
      printerTypeId: item.printerTypeId,
      code: item.code,
      name: item.name,
      printerName: item.printerName ?? "",
      sortOrder: String(item.sortOrder),
      isActive: item.isActive,
    });

    setStationErrors({});
    setStationOpen(true);
  }

  function validateStation() {
    const e: Record<string, string> = {};

    if (!stationForm.code.trim()) e.code = "Code is required.";
    if (!stationForm.name.trim()) e.name = "Name is required.";
    if (!stationForm.printerTypeId)
      e.printerTypeId = "Printer type is required.";

    const sort = Number(stationForm.sortOrder);
    if (Number.isNaN(sort) || sort < 0) e.sortOrder = "Enter valid sort order.";

    setStationErrors(e);
    return Object.keys(e).length === 0;
  }

  async function submitStation() {
    if (!validateStation()) return;

    setSubmitting(true);

    const payload = {
      id: stationForm.id,
      locationId: stationForm.locationId || null,
      printerTypeId: stationForm.printerTypeId,
      code: stationForm.code.trim(),
      name: stationForm.name.trim(),
      printerName: stationForm.printerName.trim() || null,
      sortOrder: Number(stationForm.sortOrder) || 0,
      isActive: stationForm.isActive,
    };

    try {
      await apiClient("/api/v1/kitchen-stations", {
        method: "PUT",
        body: JSON.stringify(payload),
      });

      setStationOpen(false);
      flash(
        stationForm.id
          ? "Kitchen station updated."
          : "Kitchen station created.",
      );
      await load();
    } catch (e) {
      flash(extractError(e, "Could not save kitchen station."));
    } finally {
      setSubmitting(false);
    }
  }

  async function removeStation(item: KitchenStation) {
    setBusyId(item.id);

    try {
      await apiClient(`/api/v1/kitchen-stations/${item.id}`, {
        method: "DELETE",
      });

      flash(`${item.name} removed.`);
      await load();
    } catch (e) {
      flash(extractError(e, "Could not remove kitchen station."));
    } finally {
      setBusyId(null);
    }
  }

  function openNewPrinterType() {
    setTypeForm(emptyPrinterTypeForm);
    setTypeErrors({});
    setTypeOpen(true);
  }

  function openEditPrinterType(item: PrinterType) {
    setTypeForm({
      id: item.id,
      code: item.code,
      name: item.name,
      sortOrder: String(item.sortOrder),
      isActive: item.isActive,
    });

    setTypeErrors({});
    setTypeOpen(true);
  }

  function validatePrinterType() {
    const e: Record<string, string> = {};

    if (!typeForm.code.trim()) e.code = "Code is required.";
    if (!typeForm.name.trim()) e.name = "Name is required.";

    const sort = Number(typeForm.sortOrder);
    if (Number.isNaN(sort) || sort < 0) e.sortOrder = "Enter valid sort order.";

    setTypeErrors(e);
    return Object.keys(e).length === 0;
  }

  async function submitPrinterType() {
    if (!validatePrinterType()) return;

    setSubmitting(true);

    const payload = {
      id: typeForm.id,
      code: typeForm.code.trim(),
      name: typeForm.name.trim(),
      sortOrder: Number(typeForm.sortOrder) || 0,
      isActive: typeForm.isActive,
    };

    try {
      await apiClient("/api/v1/printer-types", {
        method: "PUT",
        body: JSON.stringify(payload),
      });

      setTypeOpen(false);
      flash(typeForm.id ? "Printer type updated." : "Printer type created.");
      await load();
    } catch (e) {
      flash(extractError(e, "Could not save printer type."));
    } finally {
      setSubmitting(false);
    }
  }

  async function removePrinterType(item: PrinterType) {
    setTypeBusyId(item.id);

    try {
      await apiClient(`/api/v1/printer-types/${item.id}`, {
        method: "DELETE",
      });

      flash(`${item.name} removed.`);
      await load();
    } catch (e) {
      flash(extractError(e, "Could not remove printer type."));
    } finally {
      setTypeBusyId(null);
    }
  }

  const from = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to = Math.min(pageNumber * pageSize, totalCount);

  return (
    <>
      <Topbar title="Kitchen Stations" subtitle="KOT/BOT printer routing" />

      {toast && (
        <div className="fixed right-5 top-5 z-[80] rounded-lg bg-slate-900 px-4 py-2 text-sm font-medium text-white shadow-xl">
          {toast}
        </div>
      )}

      <div className="p-6">
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Kitchen Stations</h2>
            <p className="text-sm text-muted-foreground">
              {totalCount} stations configured for KOT, BOT and printer routing
            </p>
          </div>

          <div className="flex items-center gap-2">
            <button
              onClick={openNewPrinterType}
              className="rounded-lg border border-border bg-card px-4 py-2 text-sm font-medium hover:bg-muted"
            >
              Printer types
            </button>

            <button
              onClick={openNewStation}
              className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark"
            >
              <Plus className="size-4" />
              New station
            </button>
          </div>
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
              placeholder="Search code, name or printer"
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
            value={printerTypeId}
            onChange={(e) => {
              setPrinterTypeId(e.target.value);
              setPageNumber(1);
            }}
            className="rounded-lg border border-border bg-card px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">All printer types</option>
            {printerTypes.map((t) => (
              <option key={t.id} value={t.id}>
                {t.code} — {t.name}
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
                  <th className="px-4 py-2.5 font-medium">Station</th>
                  <th className="px-4 py-2.5 font-medium">Printer Type</th>
                  <th className="px-4 py-2.5 font-medium">Printer Name</th>
                  <th className="px-4 py-2.5 font-medium">Location</th>
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
                    <td className="px-4 py-2.5">
                      <span className="pill pill-idle">
                        {item.printerTypeCode}
                      </span>
                    </td>
                    <td className="px-4 py-2.5 text-muted-foreground">
                      {item.printerName || "—"}
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
                        className={`pill ${item.isActive ? "pill-paid" : "pill-void"}`}
                      >
                        {item.isActive ? "Active" : "Inactive"}
                      </span>
                    </td>
                    <td className="px-4 py-2.5 text-right">
                      <div className="flex justify-end gap-2">
                        <button
                          onClick={() => openEditStation(item)}
                          className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted"
                        >
                          Edit
                        </button>
                        <button
                          disabled={busyId === item.id}
                          onClick={() => removeStation(item)}
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
                      No kitchen stations found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from} to={to} setPage={setPageNumber} noun="kitchen stations" />
      </div>

      {stationOpen && (
        <div
          className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 p-4"
          onClick={() => !submitting && setStationOpen(false)}
        >
          <div
            className="w-full max-w-lg rounded-xl bg-card p-6 shadow-2xl"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="mb-4 flex items-start justify-between">
              <div>
                <h3 className="font-heading text-lg font-bold">
                  {stationForm.id
                    ? "Edit kitchen station"
                    : "New kitchen station"}
                </h3>
                <p className="text-sm text-muted-foreground">
                  Route products to KOT, BOT or no printer.
                </p>
              </div>

              <button
                onClick={() => !submitting && setStationOpen(false)}
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
                  value={stationForm.code}
                  onChange={(e) =>
                    setStationForm((f) => ({
                      ...f,
                      code: e.target.value.toUpperCase(),
                    }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 font-mono text-sm focus:border-primary focus:ring-2 focus:ring-primary/20"
                  placeholder="KOT"
                />
                {stationErrors.code && (
                  <p className="mt-1 text-xs text-status-error">
                    {stationErrors.code}
                  </p>
                )}
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold text-slate-700">
                  Sort order
                </label>
                <input
                  value={stationForm.sortOrder}
                  onChange={(e) =>
                    setStationForm((f) => ({
                      ...f,
                      sortOrder: e.target.value.replace(/[^0-9]/g, ""),
                    }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 tabular-nums focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
                {stationErrors.sortOrder && (
                  <p className="mt-1 text-xs text-status-error">
                    {stationErrors.sortOrder}
                  </p>
                )}
              </div>

              <div className="col-span-2">
                <label className="mb-1 block text-sm font-semibold text-slate-700">
                  Name
                </label>
                <input
                  value={stationForm.name}
                  onChange={(e) =>
                    setStationForm((f) => ({ ...f, name: e.target.value }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                  placeholder="Kitchen"
                />
                {stationErrors.name && (
                  <p className="mt-1 text-xs text-status-error">
                    {stationErrors.name}
                  </p>
                )}
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold text-slate-700">
                  Printer type
                </label>
                <select
                  value={stationForm.printerTypeId}
                  onChange={(e) =>
                    setStationForm((f) => ({
                      ...f,
                      printerTypeId: e.target.value,
                    }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                >
                  <option value="">Select...</option>
                  {printerTypes.map((t) => (
                    <option key={t.id} value={t.id}>
                      {t.code} — {t.name}
                    </option>
                  ))}
                </select>
                {stationErrors.printerTypeId && (
                  <p className="mt-1 text-xs text-status-error">
                    {stationErrors.printerTypeId}
                  </p>
                )}
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold text-slate-700">
                  Location
                </label>
                <select
                  value={stationForm.locationId}
                  onChange={(e) =>
                    setStationForm((f) => ({
                      ...f,
                      locationId: e.target.value,
                    }))
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

              <div className="col-span-2">
                <label className="mb-1 block text-sm font-semibold text-slate-700">
                  Printer name
                </label>
                <input
                  value={stationForm.printerName}
                  onChange={(e) =>
                    setStationForm((f) => ({
                      ...f,
                      printerName: e.target.value,
                    }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                  placeholder="Kitchen printer name"
                />
              </div>

              <label className="col-span-2 mt-1 flex items-center gap-2 text-sm font-medium">
                <input
                  type="checkbox"
                  checked={stationForm.isActive}
                  onChange={(e) =>
                    setStationForm((f) => ({
                      ...f,
                      isActive: e.target.checked,
                    }))
                  }
                  className="size-4 rounded border-border"
                />
                Active
              </label>
            </div>

            <div className="mt-5 flex justify-end gap-2">
              <button
                disabled={submitting}
                onClick={() => setStationOpen(false)}
                className="rounded-lg border border-border bg-card px-4 py-2 text-sm font-medium hover:bg-muted disabled:opacity-50"
              >
                Cancel
              </button>

              <button
                disabled={submitting}
                onClick={submitStation}
                className="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark disabled:opacity-50"
              >
                {submitting ? "Saving..." : "Save station"}
              </button>
            </div>
          </div>
        </div>
      )}

      {typeOpen && (
        <div
          className="fixed inset-0 z-[70] flex items-center justify-center bg-black/40 p-4"
          onClick={() => !submitting && setTypeOpen(false)}
        >
          <div
            className="w-full max-w-2xl rounded-xl bg-card p-6 shadow-2xl"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="mb-4 flex items-start justify-between">
              <div>
                <h3 className="font-heading text-lg font-bold">
                  Printer Types
                </h3>
                <p className="text-sm text-muted-foreground">
                  Manage KOT, BOT and NONE printer type masters.
                </p>
              </div>

              <button
                onClick={() => !submitting && setTypeOpen(false)}
                className="rounded-lg p-1.5 text-muted-foreground hover:bg-muted"
              >
                <X className="size-5" />
              </button>
            </div>

            <div className="mb-4 grid grid-cols-5 gap-3 rounded-xl border border-border bg-muted/30 p-3">
              <div>
                <label className="mb-1 block text-xs font-semibold text-slate-700">
                  Code
                </label>
                <input
                  value={typeForm.code}
                  onChange={(e) =>
                    setTypeForm((f) => ({
                      ...f,
                      code: e.target.value.toUpperCase(),
                    }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm font-mono"
                  placeholder="KOT"
                />
                {typeErrors.code && (
                  <p className="mt-1 text-xs text-status-error">
                    {typeErrors.code}
                  </p>
                )}
              </div>

              <div className="col-span-2">
                <label className="mb-1 block text-xs font-semibold text-slate-700">
                  Name
                </label>
                <input
                  value={typeForm.name}
                  onChange={(e) =>
                    setTypeForm((f) => ({ ...f, name: e.target.value }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm"
                  placeholder="Kitchen Order Ticket"
                />
                {typeErrors.name && (
                  <p className="mt-1 text-xs text-status-error">
                    {typeErrors.name}
                  </p>
                )}
              </div>

              <div>
                <label className="mb-1 block text-xs font-semibold text-slate-700">
                  Sort
                </label>
                <input
                  value={typeForm.sortOrder}
                  onChange={(e) =>
                    setTypeForm((f) => ({
                      ...f,
                      sortOrder: e.target.value.replace(/[^0-9]/g, ""),
                    }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm tabular-nums"
                />
              </div>

              <div className="flex items-end gap-2">
                <button
                  disabled={submitting}
                  onClick={submitPrinterType}
                  className="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50"
                >
                  Save
                </button>
              </div>

              <label className="col-span-5 flex items-center gap-2 text-sm font-medium">
                <input
                  type="checkbox"
                  checked={typeForm.isActive}
                  onChange={(e) =>
                    setTypeForm((f) => ({ ...f, isActive: e.target.checked }))
                  }
                  className="size-4 rounded border-border"
                />
                Active
              </label>
            </div>

            <div className="max-h-80 overflow-auto rounded-xl border border-border">
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
                  {printerTypes.map((t) => (
                    <tr key={t.id}>
                      <td className="px-4 py-2.5 font-mono text-xs text-muted-foreground">
                        {t.code}
                      </td>
                      <td className="px-4 py-2.5 font-medium">{t.name}</td>
                      <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">
                        {t.sortOrder}
                      </td>
                      <td className="px-4 py-2.5">
                        <span
                          className={`pill ${t.isActive ? "pill-paid" : "pill-void"}`}
                        >
                          {t.isActive ? "Active" : "Inactive"}
                        </span>
                      </td>
                      <td className="px-4 py-2.5 text-right">
                        <div className="flex justify-end gap-2">
                          <button
                            onClick={() => openEditPrinterType(t)}
                            className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted"
                          >
                            Edit
                          </button>
                          <button
                            disabled={typeBusyId === t.id}
                            onClick={() => removePrinterType(t)}
                            className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-status-error hover:bg-muted disabled:opacity-50"
                          >
                            Remove
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}

                  {printerTypes.length === 0 && (
                    <tr>
                      <td
                        colSpan={5}
                        className="px-4 py-10 text-center text-muted-foreground"
                      >
                        No printer types found.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>

            <div className="mt-5 flex justify-end">
              <button
                onClick={() => setTypeOpen(false)}
                className="rounded-lg border border-border bg-card px-4 py-2 text-sm font-medium hover:bg-muted"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
