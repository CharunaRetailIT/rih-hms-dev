"use client";

import { useEffect, useState } from "react";
import { Topbar } from "@/components/app-shell/Topbar";
import { apiClient } from "@/lib/api-client";
import { Pagination } from "@/components/ui/Pagination";
import { Building2, Plus, Search, X } from "lucide-react";

type Department = {
  id: string;
  code: string;
  name: string;
  remark: string | null;
  isActive: boolean;
  locationId: string | null;
  locationName: string | null;
  dashboardColor: string | null;
};

type Location = {
  id: string;
  code: string;
  name: string;
};

type PagedResponse = {
  data: Department[];
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
  remark: string;
  locationId: string;
  dashboardColor: string;
  isActive: boolean;
};

const emptyForm: Form = {
  id: null,
  code: "",
  name: "",
  remark: "",
  locationId: "",
  dashboardColor: "#2563eb",
  isActive: true,
};

export default function DepartmentsPage() {
  const [items, setItems] = useState<Department[]>([]);
  const [locations, setLocations] = useState<Location[]>([]);
  const [loading, setLoading] = useState(true);

  const [search, setSearch] = useState("");
  const [locationIdFilter, setLocationIdFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
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

  async function loadLocations() {
    try {
      const data = await apiClient<Location[]>("/api/v1/locations");
      setLocations(data);
    } catch {
      setLocations([]);
    }
  }

  async function load() {
    setLoading(true);

    try {
      const params = new URLSearchParams();
      params.set("pageNumber", String(pageNumber));
      params.set("pageSize", String(pageSize));

      if (search.trim()) params.set("search", search.trim());
      if (locationIdFilter) params.set("locationId", locationIdFilter);
      if (statusFilter) params.set("isActive", statusFilter);

      const res = await apiClient<PagedResponse>(
        `/api/v1/departments/paged?${params.toString()}`,
      );

      setItems(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages || 1);
    } catch (e) {
      flash(extractError(e, "Could not load departments."));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadLocations();
  }, []);

  useEffect(() => {
    void load();
  }, [pageNumber, pageSize, locationIdFilter, statusFilter]);

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setPageNumber(1);
      void load();
    }, 350);

    return () => window.clearTimeout(timer);
  }, [search]);

  function openNew() {
    setForm(emptyForm);
    setOpen(true);
  }

  function openEdit(item: Department) {
    setForm({
      id: item.id,
      code: item.code,
      name: item.name,
      remark: item.remark ?? "",
      locationId: item.locationId ?? "",
      dashboardColor: item.dashboardColor ?? "#2563eb",
      isActive: item.isActive,
    });

    setOpen(true);
  }

  async function save() {
    if (!form.code.trim()) {
      flash("Department code is required.");
      return;
    }

    if (!form.name.trim()) {
      flash("Department name is required.");
      return;
    }

    setSubmitting(true);

    try {
      await apiClient("/api/v1/departments", {
        method: "PUT",
        body: JSON.stringify({
          id: form.id,
          code: form.code.trim(),
          name: form.name.trim(),
          remark: form.remark.trim() || null,
          locationId: form.locationId || null,
          dashboardColor: form.dashboardColor || null,
          isActive: form.isActive,
        }),
      });

      setOpen(false);
      flash(form.id ? "Department updated." : "Department created.");
      await load();
    } catch (e) {
      flash(extractError(e, "Could not save department."));
    } finally {
      setSubmitting(false);
    }
  }

  async function remove(item: Department) {
    setBusyId(item.id);

    try {
      await apiClient(`/api/v1/departments/${item.id}`, {
        method: "DELETE",
      });

      flash(`${item.name} removed.`);
      await load();
    } catch (e) {
      flash(extractError(e, "Could not remove department."));
    } finally {
      setBusyId(null);
    }
  }

  const start = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const end = Math.min(pageNumber * pageSize, totalCount);

  return (
    <>
      <Topbar title="Master Data" subtitle="Departments" />

      <div className="p-6">
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Departments</h2>
            <p className="text-sm text-muted-foreground">
              Manage business departments by location.
            </p>
          </div>

          <button
            onClick={openNew}
            className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark"
          >
            <Plus className="size-4" />
            New department
          </button>
        </div>

        <div className="mb-4 flex flex-wrap items-center gap-3">
          <div className="relative">
            <Search className="absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />

            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search departments"
              className="rounded-lg border border-border bg-card py-1.5 pl-8 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
            />
          </div>

          <select
            value={locationIdFilter}
            onChange={(e) => {
              setLocationIdFilter(e.target.value);
              setPageNumber(1);
            }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">All locations</option>
            {locations.map((location) => (
              <option key={location.id} value={location.id}>
                {location.code} — {location.name}
              </option>
            ))}
          </select>

          <select
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(e.target.value);
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
                  <th className="px-4 py-2.5 font-medium">Department</th>
                  <th className="px-4 py-2.5 font-medium">Location</th>
                  <th className="px-4 py-2.5 font-medium">Remark</th>
                  <th className="px-4 py-2.5 font-medium">Color</th>
                  <th className="px-4 py-2.5 font-medium">Status</th>
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
                        <div
                          className="flex size-8 items-center justify-center rounded-lg text-white"
                          style={{
                            backgroundColor: item.dashboardColor ?? "#2563eb",
                          }}
                        >
                          <Building2 className="size-4" />
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
                      {item.locationName ? (
                        <span className="pill pill-idle">
                          {item.locationName}
                        </span>
                      ) : (
                        <span className="text-muted-foreground">
                          Tenant wide
                        </span>
                      )}
                    </td>

                    <td className="px-4 py-2.5 text-muted-foreground">
                      {item.remark || "—"}
                    </td>

                    <td className="px-4 py-2.5">
                      <div className="flex items-center gap-2">
                        <span
                          className="size-4 rounded border border-border"
                          style={{
                            backgroundColor: item.dashboardColor ?? "#2563eb",
                          }}
                        />
                        <span className="font-mono text-xs text-muted-foreground">
                          {item.dashboardColor ?? "—"}
                        </span>
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
                      No departments found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={start} to={end} setPage={setPageNumber} noun="departments" />
      </div>

      {open && (
        <div
          className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 p-4"
          onClick={() => !submitting && setOpen(false)}
        >
          <div
            className="w-full max-w-xl rounded-xl bg-card p-6 shadow-2xl"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="mb-4 flex items-start justify-between">
              <h3 className="font-heading text-lg font-bold">
                {form.id ? "Edit Department" : "New Department"}
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
                  placeholder="REST"
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
                  placeholder="Restaurant"
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>

              <div className="col-span-2">
                <label className="mb-1 block text-sm font-semibold">
                  Remark
                </label>
                <textarea
                  value={form.remark}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, remark: e.target.value }))
                  }
                  rows={3}
                  placeholder="Optional note"
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">
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

                  {locations.map((location) => (
                    <option key={location.id} value={location.id}>
                      {location.code} — {location.name}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">
                  Dashboard color
                </label>

                <div className="flex gap-2">
                  <input
                    type="color"
                    value={form.dashboardColor}
                    onChange={(e) =>
                      setForm((f) => ({
                        ...f,
                        dashboardColor: e.target.value,
                      }))
                    }
                    className="h-11 w-14 rounded-lg border border-border bg-surface p-1"
                  />

                  <input
                    value={form.dashboardColor}
                    onChange={(e) =>
                      setForm((f) => ({
                        ...f,
                        dashboardColor: e.target.value,
                      }))
                    }
                    className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 font-mono text-sm focus:border-primary focus:ring-2 focus:ring-primary/20"
                  />
                </div>
              </div>

              <label className="col-span-2 flex items-center gap-2 pt-1 text-sm">
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
                    : "Create Department"}
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
