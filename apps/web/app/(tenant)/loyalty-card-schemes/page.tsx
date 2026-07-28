"use client";

import { useEffect, useState } from "react";
import { Topbar } from "@/components/app-shell/Topbar";
import { apiClient } from "@/lib/api-client";
import { confirmDialog } from "@/components/ui/confirm";
import { Pagination } from "@/components/ui/Pagination";
import { Plus, Search, CreditCard, X, Trash2 } from "lucide-react";

type Tier = { id?: string; billFromValue: number; billToValue: number; increment: number; points: number; sortOrder: number };

type Scheme = {
  id: string; code: string; name: string; description: string | null; type: string;
  discountPercent: number; promotionId: string | null; promotionName: string | null; isActive: boolean;
  tiers: Tier[];
};

type Promotion = { id: string; code: string; name: string };

type TierForm = { billFromValue: string; billToValue: string; increment: string; points: string };

type SchemeForm = {
  id: string | null; code: string; name: string; description: string; type: string;
  discountPercent: string; promotionId: string; isActive: boolean; tiers: TierForm[];
};

const emptyTier = (): TierForm => ({ billFromValue: "0", billToValue: "0", increment: "0", points: "0" });

const emptyForm: SchemeForm = {
  id: null, code: "", name: "", description: "", type: "points",
  discountPercent: "0", promotionId: "", isActive: true, tiers: [emptyTier()],
};

const TYPES: [string, string][] = [["discount", "Discount"], ["points", "Points"], ["promotion", "Promotion"]];

export default function LoyaltyCardSchemesPage() {
  const [rows, setRows] = useState<Scheme[]>([]);
  const [promotions, setPromotions] = useState<Promotion[]>([]);
  const [loading, setLoading] = useState(true);
  const [toast, setToast] = useState<string | null>(null);

  const [search, setSearch] = useState("");
  const [typeFilter, setTypeFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const [open, setOpen] = useState(false);
  const [form, setForm] = useState<SchemeForm>(emptyForm);
  const [submitting, setSubmitting] = useState(false);
  const [busyId, setBusyId] = useState<string | null>(null);

  const flash = (m: string) => { setToast(m); window.setTimeout(() => setToast(null), 3000); };

  async function loadPromotions() {
    try { setPromotions(await apiClient<Promotion[]>("/api/v1/promotions")); }
    catch { setPromotions([]); /* promotions module may be off for this tenant's plan */ }
  }

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set("pageNumber", String(pageNumber));
      params.set("pageSize", String(pageSize));
      if (search.trim()) params.set("search", search.trim());
      if (typeFilter) params.set("type", typeFilter);
      if (statusFilter) params.set("isActive", statusFilter === "active" ? "true" : "false");

      const res = await apiClient<{ data: Scheme[]; pagination: { totalCount: number; totalPages: number } }>(
        `/api/v1/loyalty/card-schemes/paged?${params.toString()}`,
      );
      setRows(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages || 1);
    } catch (e) { flash(extractError(e, "Could not load loyalty card schemes.")); }
    finally { setLoading(false); }
  }

  useEffect(() => { void loadPromotions(); /* eslint-disable-next-line */ }, []);
  useEffect(() => { void load(); /* eslint-disable-next-line */ }, [pageNumber, typeFilter, statusFilter]);
  useEffect(() => {
    const t = setTimeout(() => { setPageNumber(1); void load(); }, 300);
    return () => clearTimeout(t);
    /* eslint-disable-next-line */
  }, [search]);

  function openNew() { setForm(emptyForm); setOpen(true); }
  function openEdit(s: Scheme) {
    setForm({
      id: s.id, code: s.code, name: s.name, description: s.description ?? "", type: s.type,
      discountPercent: String(s.discountPercent ?? 0), promotionId: s.promotionId ?? "", isActive: s.isActive,
      tiers: s.tiers.length > 0
        ? s.tiers.map((t) => ({ billFromValue: String(t.billFromValue), billToValue: String(t.billToValue), increment: String(t.increment), points: String(t.points) }))
        : [emptyTier()],
    });
    setOpen(true);
  }

  async function save() {
    if (!form.code.trim() || !form.name.trim()) { flash("Code and name are required."); return; }
    if (form.type === "promotion" && !form.promotionId) { flash("Select a promotion."); return; }
    setSubmitting(true);
    try {
      await apiClient("/api/v1/loyalty/card-schemes", {
        method: "POST",
        body: JSON.stringify({
          id: form.id, code: form.code.trim(), name: form.name.trim(), description: form.description.trim() || null,
          type: form.type, discountPercent: Number(form.discountPercent) || 0,
          promotionId: form.type === "promotion" ? form.promotionId : null,
          isActive: form.isActive,
          tiers: form.type === "points"
            ? form.tiers.map((t, i) => ({
                billFromValue: Number(t.billFromValue) || 0, billToValue: Number(t.billToValue) || 0,
                increment: Number(t.increment) || 0, points: Number(t.points) || 0, sortOrder: i,
              }))
            : [],
        }),
      });
      setOpen(false);
      flash(form.id ? "Loyalty card scheme updated." : "Loyalty card scheme created.");
      await load();
    } catch (e) { flash(extractError(e, "Could not save the loyalty card scheme.")); }
    finally { setSubmitting(false); }
  }

  async function remove(s: Scheme) {
    if (!(await confirmDialog({ title: `Remove ${s.name}?`, body: "Customers enrolled in this scheme must be moved to another scheme first.", confirmLabel: "Remove", danger: true }))) return;
    setBusyId(s.id);
    try { await apiClient(`/api/v1/loyalty/card-schemes/${s.id}`, { method: "DELETE" }); flash(`${s.name} removed.`); await load(); }
    catch (e) { flash(extractError(e, "Could not remove the loyalty card scheme.")); }
    finally { setBusyId(null); }
  }

  function typeLabel(t: string) { return TYPES.find(([v]) => v === t)?.[1] ?? t; }
  function detail(s: Scheme) {
    if (s.type === "discount") return `${s.discountPercent}% discount`;
    if (s.type === "promotion") return s.promotionName ?? "—";
    return `${s.tiers.length} tier${s.tiers.length === 1 ? "" : "s"}`;
  }

  return (
    <>
      <Topbar title="Customer Master" subtitle="Loyalty Card Schemes" />

      <div className="p-6 md:p-8">
        <div className="mb-5 flex items-center justify-between gap-3">
          <div>
            <h2 className="font-heading text-xl font-bold">Loyalty card schemes</h2>
            <p className="text-sm text-muted-foreground">{totalCount} schemes · Discount, Points and Promotion card types</p>
          </div>
          <button onClick={openNew} className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark">
            <Plus className="size-4" /> New Scheme
          </button>
        </div>

        <div className="mb-4 flex flex-wrap items-center gap-3">
          <div className="relative">
            <Search className="pointer-events-none absolute left-2.5 top-2.5 size-4 text-muted-foreground" />
            <input
              value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Search schemes"
              className="h-9 w-64 rounded-lg border border-border bg-surface pl-8 pr-3 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20"
            />
          </div>
          <select value={typeFilter} onChange={(e) => { setTypeFilter(e.target.value); setPageNumber(1); }} className="rounded-lg border border-border bg-card px-3 py-2 text-sm">
            <option value="">All types</option>
            {TYPES.map(([v, l]) => <option key={v} value={v}>{l}</option>)}
          </select>
          <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setPageNumber(1); }} className="rounded-lg border border-border bg-card px-3 py-2 text-sm">
            <option value="">All status</option>
            <option value="active">Active</option>
            <option value="inactive">Inactive</option>
          </select>
        </div>

        <div className="card overflow-hidden">
          {loading ? (
            <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
          ) : (
            <table className="w-full text-sm">
              <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th className="px-4 py-3 font-medium">Scheme</th>
                  <th className="px-4 py-3 font-medium">Type</th>
                  <th className="px-4 py-3 font-medium">Detail</th>
                  <th className="px-4 py-3 font-medium">Status</th>
                  <th className="px-4 py-3 text-right font-medium">Actions</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((s, i) => (
                  <tr key={s.id} className={i % 2 ? "bg-muted/20" : ""}>
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-2.5">
                        <div className="flex size-8 items-center justify-center rounded-lg bg-primary-tint text-primary"><CreditCard className="size-4" /></div>
                        <div>
                          <div className="font-medium">{s.name}</div>
                          <div className="font-mono text-xs text-muted-foreground">{s.code}</div>
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-3"><span className="pill pill-idle">{typeLabel(s.type)}</span></td>
                    <td className="px-4 py-3 text-muted-foreground">{detail(s)}</td>
                    <td className="px-4 py-3"><span className={`pill ${s.isActive ? "pill-paid" : "pill-void"}`}>{s.isActive ? "Active" : "Inactive"}</span></td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex justify-end gap-2">
                        <button onClick={() => openEdit(s)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted">Edit</button>
                        <button disabled={busyId === s.id} onClick={() => remove(s)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-status-error hover:bg-muted disabled:opacity-50">Remove</button>
                      </div>
                    </td>
                  </tr>
                ))}
                {rows.length === 0 && (
                  <tr><td colSpan={5} className="px-4 py-10 text-center text-muted-foreground">No loyalty card schemes yet. Create your first one above.</td></tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        {!loading && totalCount > 0 && (
          <Pagination
            page={pageNumber} totalPages={totalPages} total={totalCount}
            from={(pageNumber - 1) * pageSize + 1} to={Math.min(pageNumber * pageSize, totalCount)}
            setPage={setPageNumber} noun="schemes"
          />
        )}
      </div>

      {open && (
        <SchemeModal form={form} setForm={setForm} promotions={promotions} submitting={submitting} onClose={() => !submitting && setOpen(false)} onSave={save} />
      )}

      {toast && (
        <div className="fixed bottom-12 left-1/2 z-[70] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">{toast}</div>
      )}
    </>
  );
}

function SchemeModal({
  form, setForm, promotions, submitting, onClose, onSave,
}: {
  form: SchemeForm; setForm: React.Dispatch<React.SetStateAction<SchemeForm>>; promotions: Promotion[];
  submitting: boolean; onClose: () => void; onSave: () => void;
}) {
  function patchTier(i: number, p: Partial<TierForm>) {
    setForm((f) => ({ ...f, tiers: f.tiers.map((t, idx) => (idx === i ? { ...t, ...p } : t)) }));
  }
  function addTier() { setForm((f) => ({ ...f, tiers: [...f.tiers, emptyTier()] })); }
  function removeTier(i: number) { setForm((f) => ({ ...f, tiers: f.tiers.filter((_, idx) => idx !== i) })); }

  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div className="max-h-[90vh] w-full max-w-3xl overflow-y-auto rounded-xl bg-card p-6 shadow-2xl" onClick={(e) => e.stopPropagation()}>
        <div className="mb-4 flex items-start justify-between">
          <h3 className="font-heading text-lg font-bold">{form.id ? "Edit Card Scheme" : "New Card Scheme"}</h3>
          <button onClick={onClose} className="rounded-lg p-1.5 text-muted-foreground hover:bg-muted"><X className="size-5" /></button>
        </div>

        <div className="space-y-4">
          <div>
            <label className="mb-1 block text-sm font-semibold">Type</label>
            <select value={form.type} onChange={(e) => setForm((f) => ({ ...f, type: e.target.value }))} className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20">
              {TYPES.map(([v, l]) => <option key={v} value={v}>{l}</option>)}
            </select>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <Input label="Code" value={form.code} onChange={(v) => setForm((f) => ({ ...f, code: v.toUpperCase() }))} disabled={!!form.id} />
            <Input label="Name" value={form.name} onChange={(v) => setForm((f) => ({ ...f, name: v }))} />
          </div>

          <Input label="Description" value={form.description} onChange={(v) => setForm((f) => ({ ...f, description: v }))} />

          {form.type === "discount" && (
            <Input label="Discount %" value={form.discountPercent} onChange={(v) => setForm((f) => ({ ...f, discountPercent: v.replace(/[^0-9.]/g, "") }))} />
          )}

          {form.type === "promotion" && (
            <div>
              <label className="mb-1 block text-sm font-semibold">Promotion</label>
              <select value={form.promotionId} onChange={(e) => setForm((f) => ({ ...f, promotionId: e.target.value }))} className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20">
                <option value="">— Select a Promotion —</option>
                {promotions.map((p) => <option key={p.id} value={p.id}>{p.code} — {p.name}</option>)}
              </select>
              {promotions.length === 0 && <p className="mt-1 text-xs text-muted-foreground">No promotions available — create one under Promotions first.</p>}
            </div>
          )}

          {form.type === "points" && (
            <div>
              <div className="mb-2 text-center text-sm font-semibold text-muted-foreground">Point Schema</div>
              <div className="overflow-x-auto rounded-lg border border-border">
                <table className="w-full min-w-[560px] border-collapse text-sm">
                  <thead>
                    <tr className="bg-primary text-xs font-semibold text-primary-foreground">
                      <th className="border-r border-primary-dark/40 px-3 py-2 text-left">Bill Value From</th>
                      <th className="border-r border-primary-dark/40 px-3 py-2 text-left">Bill Value To</th>
                      <th className="border-r border-primary-dark/40 px-3 py-2 text-left">Increment</th>
                      <th className="border-r border-primary-dark/40 px-3 py-2 text-left">Points</th>
                      <th className="w-10 px-2 py-2" />
                    </tr>
                  </thead>
                  <tbody>
                    {form.tiers.map((t, i) => (
                      <tr key={i} className="border-t border-border">
                        <td className="border-r border-border p-0">
                          <input value={t.billFromValue} inputMode="decimal" onChange={(e) => patchTier(i, { billFromValue: e.target.value.replace(/[^0-9.]/g, "") })} className="w-full bg-surface px-3 py-2 text-right text-sm" />
                        </td>
                        <td className="border-r border-border p-0">
                          <input value={t.billToValue} inputMode="decimal" onChange={(e) => patchTier(i, { billToValue: e.target.value.replace(/[^0-9.]/g, "") })} className="w-full bg-surface px-3 py-2 text-right text-sm" />
                        </td>
                        <td className="border-r border-border p-0">
                          <input value={t.increment} inputMode="decimal" onChange={(e) => patchTier(i, { increment: e.target.value.replace(/[^0-9.]/g, "") })} className="w-full bg-surface px-3 py-2 text-right text-sm" />
                        </td>
                        <td className="border-r border-border p-0">
                          <input value={t.points} inputMode="decimal" onChange={(e) => patchTier(i, { points: e.target.value.replace(/[^0-9.]/g, "") })} className="w-full bg-surface px-3 py-2 text-right text-sm" />
                        </td>
                        <td className="p-0 text-center">
                          <button
                            onClick={() => removeTier(i)} disabled={form.tiers.length === 1}
                            className="flex w-full items-center justify-center bg-surface px-2 py-2 text-status-error hover:bg-muted disabled:opacity-30"
                            title="Remove tier"
                          >
                            <Trash2 className="size-4" />
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              <button onClick={addTier} className="mt-2 flex items-center gap-1 text-sm font-medium text-primary hover:underline">
                <Plus className="size-4" /> Add tier
              </button>
            </div>
          )}

          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" checked={form.isActive} onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))} className="size-4 rounded border-border text-primary" />
            IsActive
          </label>
        </div>

        <div className="mt-6 flex gap-2">
          <button onClick={onClose} disabled={submitting} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50">Cancel</button>
          <button onClick={onSave} disabled={submitting} className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">
            {submitting ? "Saving…" : form.id ? "Save changes" : "Submit Card Type"}
          </button>
        </div>
      </div>
    </div>
  );
}

function Input({ label, value, onChange, disabled }: { label: string; value: string; onChange: (value: string) => void; disabled?: boolean }) {
  return (
    <div>
      <label className="mb-1 block text-sm font-semibold">{label}</label>
      <input
        value={value} onChange={(e) => onChange(e.target.value)} disabled={disabled}
        className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20 disabled:opacity-60"
      />
    </div>
  );
}

function extractError(error: unknown, fallback: string) {
  if (error instanceof Error) return error.message || fallback;
  return fallback;
}
