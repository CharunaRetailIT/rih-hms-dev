"use client";

import { useEffect, useState } from "react";
import { Topbar } from "@/components/app-shell/Topbar";
import { apiClient } from "@/lib/api-client";
import { confirmDialog } from "@/components/ui/confirm";
import { Pagination } from "@/components/ui/Pagination";
import { Plus, Search, Contact, X } from "lucide-react";

type Scheme = { id: string; code: string; name: string };

type LoyaltyCustomer = {
  id: string; code: string; name: string; phone: string | null;
  loyaltyCardNo: string | null; loyaltyCardSchemeId: string | null; schemeName: string | null;
  loyaltyPoints: number; loyaltyLifetimePoints: number; isActive: boolean;
};

type CustomerLookup = { id: string; code: string; name: string; phone: string | null };

export default function LoyaltyCustomersPage() {
  const [rows, setRows] = useState<LoyaltyCustomer[]>([]);
  const [schemes, setSchemes] = useState<Scheme[]>([]);
  const [loading, setLoading] = useState(true);
  const [toast, setToast] = useState<string | null>(null);

  const [search, setSearch] = useState("");
  const [schemeFilter, setSchemeFilter] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(25);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<LoyaltyCustomer | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [busyId, setBusyId] = useState<string | null>(null);

  const flash = (m: string) => { setToast(m); window.setTimeout(() => setToast(null), 3000); };

  async function loadSchemes() {
    try { setSchemes(await apiClient<Scheme[]>("/api/v1/loyalty/card-schemes?all=true")); }
    catch (e) { flash(extractError(e, "Could not load loyalty card schemes.")); }
  }

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set("pageNumber", String(pageNumber));
      params.set("pageSize", String(pageSize));
      if (search.trim()) params.set("search", search.trim());
      if (schemeFilter) params.set("schemeId", schemeFilter);

      const res = await apiClient<{ data: LoyaltyCustomer[]; pagination: { totalCount: number; totalPages: number } }>(
        `/api/v1/loyalty/customers/paged?${params.toString()}`,
      );
      setRows(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages || 1);
    } catch (e) { flash(extractError(e, "Could not load loyalty customers.")); }
    finally { setLoading(false); }
  }

  useEffect(() => { void loadSchemes(); /* eslint-disable-next-line */ }, []);
  useEffect(() => { void load(); /* eslint-disable-next-line */ }, [pageNumber, schemeFilter]);
  useEffect(() => {
    const t = setTimeout(() => { setPageNumber(1); void load(); }, 300);
    return () => clearTimeout(t);
    /* eslint-disable-next-line */
  }, [search]);

  function openEnroll() { setEditing(null); setOpen(true); }
  function openEdit(r: LoyaltyCustomer) { setEditing(r); setOpen(true); }

  async function unenroll(r: LoyaltyCustomer) {
    if (!(await confirmDialog({ title: `Remove ${r.name} from loyalty?`, body: "Their points balance is kept, but they'll no longer show in this list until re-enrolled.", confirmLabel: "Remove", danger: true }))) return;
    setBusyId(r.id);
    try {
      await apiClient(`/api/v1/loyalty/customers/${r.id}`, { method: "PUT", body: JSON.stringify({ schemeId: null, loyaltyCardNo: null }) });
      flash("Removed from loyalty."); await load();
    } catch (e) { flash(extractError(e, "Could not update the customer.")); }
    finally { setBusyId(null); }
  }

  return (
    <>
      <Topbar title="Customer Master" subtitle="Loyalty Customers" />

      <div className="p-6 md:p-8">
        <div className="mb-5 flex items-center justify-between gap-3">
          <div>
            <h2 className="font-heading text-xl font-bold">Loyalty customers</h2>
            <p className="text-sm text-muted-foreground">{totalCount} enrolled · card scheme, points balance and card number</p>
          </div>
          <button onClick={openEnroll} className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark">
            <Plus className="size-4" /> Enroll Customer
          </button>
        </div>

        <div className="mb-4 flex flex-wrap items-center gap-3">
          <div className="relative">
            <Search className="pointer-events-none absolute left-2.5 top-2.5 size-4 text-muted-foreground" />
            <input
              value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Search name / code / card no"
              className="h-9 w-64 rounded-lg border border-border bg-surface pl-8 pr-3 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20"
            />
          </div>
          <select value={schemeFilter} onChange={(e) => { setSchemeFilter(e.target.value); setPageNumber(1); }} className="rounded-lg border border-border bg-card px-3 py-2 text-sm">
            <option value="">All schemes</option>
            {schemes.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
          </select>
        </div>

        <div className="card overflow-hidden">
          {loading ? (
            <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
          ) : (
            <table className="w-full text-sm">
              <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th className="px-4 py-3 font-medium">Customer</th>
                  <th className="px-4 py-3 font-medium">Card No</th>
                  <th className="px-4 py-3 font-medium">Scheme</th>
                  <th className="px-4 py-3 text-right font-medium">Points</th>
                  <th className="px-4 py-3 text-right font-medium">Lifetime</th>
                  <th className="px-4 py-3 text-right font-medium">Actions</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((r, i) => (
                  <tr key={r.id} className={i % 2 ? "bg-muted/20" : ""}>
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-2.5">
                        <div className="flex size-8 items-center justify-center rounded-lg bg-primary-tint text-primary"><Contact className="size-4" /></div>
                        <div>
                          <div className="font-medium">{r.name}</div>
                          <div className="font-mono text-xs text-muted-foreground">{r.code}</div>
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-3 font-mono text-xs text-muted-foreground">{r.loyaltyCardNo ?? "—"}</td>
                    <td className="px-4 py-3 text-muted-foreground">{r.schemeName ?? "—"}</td>
                    <td className="px-4 py-3 text-right tabular-nums">{r.loyaltyPoints.toLocaleString()}</td>
                    <td className="px-4 py-3 text-right tabular-nums text-muted-foreground">{r.loyaltyLifetimePoints.toLocaleString()}</td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex justify-end gap-2">
                        <button onClick={() => openEdit(r)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted">Edit</button>
                        <button disabled={busyId === r.id} onClick={() => unenroll(r)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-status-error hover:bg-muted disabled:opacity-50">Remove</button>
                      </div>
                    </td>
                  </tr>
                ))}
                {rows.length === 0 && (
                  <tr><td colSpan={6} className="px-4 py-10 text-center text-muted-foreground">No loyalty customers yet. Enroll your first one above.</td></tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        {!loading && totalCount > 0 && (
          <Pagination
            page={pageNumber} totalPages={totalPages} total={totalCount}
            from={(pageNumber - 1) * pageSize + 1} to={Math.min(pageNumber * pageSize, totalCount)}
            setPage={setPageNumber} noun="loyalty customers"
          />
        )}
      </div>

      {open && (
        <EnrollModal
          editing={editing} schemes={schemes}
          onClose={() => !submitting && setOpen(false)}
          onSaved={async () => { setOpen(false); flash(editing ? "Loyalty customer updated." : "Customer enrolled."); await load(); }}
          submitting={submitting} setSubmitting={setSubmitting}
          flash={flash}
        />
      )}

      {toast && (
        <div className="fixed bottom-12 left-1/2 z-[70] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">{toast}</div>
      )}
    </>
  );
}

function EnrollModal({
  editing, schemes, onClose, onSaved, submitting, setSubmitting, flash,
}: {
  editing: LoyaltyCustomer | null; schemes: Scheme[]; onClose: () => void; onSaved: () => Promise<void>;
  submitting: boolean; setSubmitting: (v: boolean) => void; flash: (m: string) => void;
}) {
  const [customer, setCustomer] = useState<CustomerLookup | null>(editing ? { id: editing.id, code: editing.code, name: editing.name, phone: null } : null);
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<CustomerLookup[]>([]);
  const [searching, setSearching] = useState(false);
  const [schemeId, setSchemeId] = useState(editing?.loyaltyCardSchemeId ?? "");
  const [cardNo, setCardNo] = useState(editing?.loyaltyCardNo ?? "");

  useEffect(() => {
    if (editing || query.trim().length < 2) { setResults([]); return; }
    setSearching(true);
    const t = setTimeout(async () => {
      try { setResults(await apiClient<CustomerLookup[]>(`/api/v1/customers?search=${encodeURIComponent(query.trim())}`)); }
      catch { setResults([]); }
      finally { setSearching(false); }
    }, 300);
    return () => clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [query, editing]);

  async function save() {
    if (!customer) { flash("Pick a customer to enroll."); return; }
    setSubmitting(true);
    try {
      await apiClient(`/api/v1/loyalty/customers/${customer.id}`, {
        method: "PUT",
        body: JSON.stringify({ schemeId: schemeId || null, loyaltyCardNo: cardNo.trim() || null }),
      });
      await onSaved();
    } catch (e) { flash(extractError(e, "Could not save.")); }
    finally { setSubmitting(false); }
  }

  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div className="w-full max-w-lg rounded-xl bg-card p-6 shadow-2xl" onClick={(e) => e.stopPropagation()}>
        <div className="mb-4 flex items-start justify-between">
          <h3 className="font-heading text-lg font-bold">{editing ? "Edit loyalty enrollment" : "Enroll customer"}</h3>
          <button onClick={onClose} className="rounded-lg p-1.5 text-muted-foreground hover:bg-muted"><X className="size-5" /></button>
        </div>

        <div className="space-y-4">
          <div>
            <label className="mb-1 block text-sm font-semibold">Customer</label>
            {editing ? (
              <div className="rounded-lg border border-border bg-muted/40 px-3 py-2.5 text-sm">
                <span className="font-medium">{customer?.name}</span>{" "}
                <span className="font-mono text-xs text-muted-foreground">{customer?.code}</span>
              </div>
            ) : customer ? (
              <div className="flex items-center justify-between rounded-lg border border-border bg-muted/40 px-3 py-2.5 text-sm">
                <span><span className="font-medium">{customer.name}</span> <span className="font-mono text-xs text-muted-foreground">{customer.code}</span></span>
                <button onClick={() => { setCustomer(null); setQuery(""); }} className="text-xs font-medium text-primary hover:underline">Change</button>
              </div>
            ) : (
              <div>
                <input
                  value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Search customer by name / phone / code"
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
                {searching && <p className="mt-1 text-xs text-muted-foreground">Searching…</p>}
                {results.length > 0 && (
                  <div className="mt-1 max-h-40 overflow-y-auto rounded-lg border border-border">
                    {results.map((c) => (
                      <button
                        key={c.id} onClick={() => { setCustomer(c); setResults([]); }}
                        className="flex w-full items-center justify-between px-3 py-2 text-left text-sm hover:bg-muted"
                      >
                        <span>{c.name}</span>
                        <span className="font-mono text-xs text-muted-foreground">{c.code}</span>
                      </button>
                    ))}
                  </div>
                )}
              </div>
            )}
          </div>

          <div>
            <label className="mb-1 block text-sm font-semibold">Loyalty Card Scheme</label>
            <select value={schemeId} onChange={(e) => setSchemeId(e.target.value)} className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20">
              <option value="">No scheme</option>
              {schemes.map((s) => <option key={s.id} value={s.id}>{s.code} — {s.name}</option>)}
            </select>
          </div>

          <div>
            <label className="mb-1 block text-sm font-semibold">Card No</label>
            <input value={cardNo} onChange={(e) => setCardNo(e.target.value)} placeholder="Scannable card / membership number" className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20" />
          </div>
        </div>

        <div className="mt-6 flex gap-2">
          <button onClick={onClose} disabled={submitting} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50">Cancel</button>
          <button onClick={save} disabled={submitting} className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">
            {submitting ? "Saving…" : editing ? "Save changes" : "Enroll"}
          </button>
        </div>
      </div>
    </div>
  );
}

function extractError(error: unknown, fallback: string) {
  if (error instanceof Error) return error.message || fallback;
  return fallback;
}
