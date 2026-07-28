"use client";

import { useEffect, useState } from "react";
import { Topbar } from "@/components/app-shell/Topbar";
import { apiClient } from "@/lib/api-client";
import { Pagination } from "@/components/ui/Pagination";
import { Percent, Plus, Search, X } from "lucide-react";

type Charge = {
  id: string;
  chargeTypeId: string;
  chargeTypeName: string;
  appliesPerProduct: boolean;
  code: string;
  description: string;
  percentage: number | null;
  amount: number | null;
  isActive: boolean;
};

type ChargeType = {
  id: string;
  code: string;
  name: string;
  appliesPerProduct: boolean;
};

type PagedResponse = {
  data: Charge[];
  pagination: {
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
  };
};

type Form = {
  id: string | null;
  chargeTypeId: string;
  code: string;
  description: string;
  mode: "percentage" | "amount";
  value: string;
  isActive: boolean;
};

const emptyForm: Form = {
  id: null,
  chargeTypeId: "",
  code: "",
  description: "",
  mode: "percentage",
  value: "0",
  isActive: true,
};

export default function ChargesPage() {
  const [items, setItems] = useState<Charge[]>([]);
  const [chargeTypes, setChargeTypes] = useState<ChargeType[]>([]);
  const [loading, setLoading] = useState(true);

  const [search, setSearch] = useState("");
  const [chargeTypeFilter, setChargeTypeFilter] = useState("");
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

  async function loadChargeTypes() {
    try {
      const res = await apiClient<{ data: ChargeType[] }>(
        "/api/v1/charge-types/paged?pageSize=100&isActive=true",
      );
      setChargeTypes(res.data);
    } catch {
      setChargeTypes([]);
    }
  }

  async function load() {
    setLoading(true);

    try {
      const params = new URLSearchParams();
      params.set("pageNumber", String(pageNumber));
      params.set("pageSize", String(pageSize));

      if (search.trim()) params.set("search", search.trim());
      if (chargeTypeFilter) params.set("chargeTypeId", chargeTypeFilter);
      if (statusFilter) params.set("isActive", statusFilter);

      const res = await apiClient<PagedResponse>(
        `/api/v1/charges/paged?${params.toString()}`,
      );

      setItems(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages || 1);
    } catch (e) {
      flash(extractError(e, "Could not load charges."));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadChargeTypes();
  }, []);

  useEffect(() => {
    void load();
  }, [pageNumber, pageSize, chargeTypeFilter, statusFilter]);

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setPageNumber(1);
      void load();
    }, 350);

    return () => window.clearTimeout(timer);
  }, [search]);

  function openNew() {
    setForm({ ...emptyForm, chargeTypeId: chargeTypes[0]?.id ?? "" });
    setOpen(true);
  }

  function openEdit(item: Charge) {
    setForm({
      id: item.id,
      chargeTypeId: item.chargeTypeId,
      code: item.code,
      description: item.description,
      mode: item.amount != null ? "amount" : "percentage",
      value: String(item.amount != null ? item.amount : (item.percentage ?? 0)),
      isActive: item.isActive,
    });

    setOpen(true);
  }

  async function save() {
    if (!form.chargeTypeId) {
      flash("Charge type is required.");
      return;
    }

    if (!form.code.trim()) {
      flash("Code is required.");
      return;
    }

    if (!form.description.trim()) {
      flash("Description is required.");
      return;
    }

    setSubmitting(true);

    try {
      const numericValue = Number(form.value) || 0;

      await apiClient("/api/v1/charges", {
        method: "PUT",
        body: JSON.stringify({
          id: form.id,
          chargeTypeId: form.chargeTypeId,
          code: form.code.trim(),
          description: form.description.trim(),
          percentage: form.mode === "percentage" ? numericValue : null,
          amount: form.mode === "amount" ? numericValue : null,
          isActive: form.isActive,
        }),
      });

      setOpen(false);
      flash(form.id ? "Charge updated." : "Charge created.");
      await load();
    } catch (e) {
      flash(extractError(e, "Could not save charge."));
    } finally {
      setSubmitting(false);
    }
  }

  async function remove(item: Charge) {
    setBusyId(item.id);

    try {
      await apiClient(`/api/v1/charges/${item.id}`, { method: "DELETE" });
      flash(`${item.description} removed.`);
      await load();
    } catch (e) {
      flash(extractError(e, "Could not remove charge."));
    } finally {
      setBusyId(null);
    }
  }

  const start = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const end = Math.min(pageNumber * pageSize, totalCount);

  return (
    <>
      <Topbar
        title="Charges"
        subtitle="Tax, service charge, levy and other configured charges"
      />

      <div className="p-6">
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Charges</h2>
            <p className="text-sm text-muted-foreground">
              Each charge is a % rate or a flat amount under a charge type.
              Charge types that assign to products (e.g. Tax) are picked per
              product on the menu form; everything else applies to every
              order.
            </p>
          </div>

          <button
            onClick={openNew}
            disabled={chargeTypes.length === 0}
            className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark disabled:opacity-50"
          >
            <Plus className="size-4" />
            New charge
          </button>
        </div>

        <div className="mb-4 flex flex-wrap items-center gap-3">
          <div className="relative">
            <Search className="absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />

            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search by code or description"
              className="rounded-lg border border-border bg-card py-1.5 pl-8 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
            />
          </div>

          <select
            value={chargeTypeFilter}
            onChange={(e) => {
              setChargeTypeFilter(e.target.value);
              setPageNumber(1);
            }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">All types</option>
            {chargeTypes.map((t) => (
              <option key={t.id} value={t.id}>
                {t.name}
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
                  <th className="px-4 py-2.5 font-medium">Charge</th>
                  <th className="px-4 py-2.5 font-medium">Type</th>
                  <th className="px-4 py-2.5 text-right font-medium">
                    Rate / Amount
                  </th>
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
                        <div className="flex size-8 items-center justify-center rounded-lg bg-primary/10 text-primary">
                          <Percent className="size-4" />
                        </div>

                        <div>
                          <div className="font-medium">
                            {item.description}
                          </div>
                          <div className="font-mono text-xs text-muted-foreground">
                            {item.code}
                          </div>
                        </div>
                      </div>
                    </td>

                    <td className="px-4 py-2.5">
                      <span className="pill pill-idle">
                        {item.chargeTypeName}
                      </span>
                    </td>

                    <td className="px-4 py-2.5 text-right tabular-nums">
                      {item.percentage != null
                        ? `${item.percentage}%`
                        : `LKR ${item.amount ?? 0}`}
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
                      No charges configured — sales won&rsquo;t be taxed. Add
                      VAT to get started.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={start} to={end} setPage={setPageNumber} noun="charges" />
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
                {form.id ? "Edit Charge" : "New Charge"}
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
                <label className="mb-1 block text-sm font-semibold">
                  Charge type
                </label>
                <select
                  value={form.chargeTypeId}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, chargeTypeId: e.target.value }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                >
                  <option value="">Select…</option>
                  {chargeTypes.map((t) => (
                    <option key={t.id} value={t.id}>
                      {t.name}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">
                  Code
                </label>
                <input
                  value={form.code}
                  disabled={!!form.id}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      code: e.target.value.toUpperCase(),
                    }))
                  }
                  placeholder="VAT"
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 font-mono uppercase focus:border-primary focus:ring-2 focus:ring-primary/20 disabled:opacity-60"
                />
              </div>

              <div className="col-span-2">
                <label className="mb-1 block text-sm font-semibold">
                  Description
                </label>
                <input
                  value={form.description}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, description: e.target.value }))
                  }
                  placeholder="VAT (18%)"
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">
                  Mode
                </label>
                <select
                  value={form.mode}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      mode: e.target.value as "percentage" | "amount",
                    }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                >
                  <option value="percentage">Percentage</option>
                  <option value="amount">Flat amount</option>
                </select>
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold">
                  {form.mode === "percentage" ? "Rate %" : "Amount (LKR)"}
                </label>
                <input
                  value={form.value}
                  inputMode="decimal"
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      value: e.target.value.replace(/[^0-9.]/g, ""),
                    }))
                  }
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 text-right tabular-nums focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
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
                    : "Create Charge"}
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
