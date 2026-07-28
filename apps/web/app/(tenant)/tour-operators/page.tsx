"use client";

import { useEffect, useState } from "react";
import { Topbar } from "@/components/app-shell/Topbar";
import { apiClient } from "@/lib/api-client";
import { confirmDialog } from "@/components/ui/confirm";
import { Plus, Search, Briefcase, Plane, X } from "lucide-react";
import { Pagination } from "@/components/ui/Pagination";
import { COUNTRIES } from "@/lib/regions";

type Company = {
  id: string; code: string; name: string;
  address1: string | null; address2: string | null; countryCode: string | null; mobile: string | null; telephone: string | null;
  faxNo: string | null; email: string | null; webAddress: string | null; contactPerson: string | null;
  commissionPercent: number; commissionAmount: number; isActive: boolean;
};

type Agent = {
  id: string; code: string; name: string; commissionPercent: number; kind: string; isActive: boolean;
  companyId: string | null; title: string | null; nic: string | null;
  address1: string | null; address2: string | null; address3: string | null; countryCode: string | null;
  mobile: string | null; email: string | null; amount: number; remarks: string | null;
};

type CommissionBasis = "percent" | "amount";

type CompanyForm = {
  id: string | null; code: string; name: string; address1: string; address2: string; countryCode: string; mobile: string; telephone: string;
  faxNo: string; email: string; webAddress: string; contactPerson: string;
  commissionBasis: CommissionBasis; commissionPercent: string; commissionAmount: string; isActive: boolean;
};

type AgentForm = {
  id: string | null; code: string; name: string; isActive: boolean;
  companyId: string; title: string; nic: string; address1: string; address2: string; address3: string; countryCode: string;
  mobile: string; email: string; commissionBasis: CommissionBasis; commissionPercent: string; amount: string; remarks: string;
};

const emptyCompanyForm: CompanyForm = {
  id: null, code: "", name: "", address1: "", address2: "", countryCode: "LK", mobile: "", telephone: "",
  faxNo: "", email: "", webAddress: "", contactPerson: "", commissionBasis: "amount", commissionPercent: "0", commissionAmount: "0", isActive: true,
};

const emptyAgentForm: AgentForm = {
  id: null, code: "", name: "", isActive: true,
  companyId: "", title: "", nic: "", address1: "", address2: "", address3: "", countryCode: "LK",
  mobile: "", email: "", commissionBasis: "percent", commissionPercent: "0", amount: "0", remarks: "",
};

const TITLES = ["Mr.", "Mrs.", "Miss", "Dr.", "Rev."];

export default function TourOperatorsPage() {
  const [tab, setTab] = useState<"agents" | "companies">("agents");
  // Full, unpaginated lookup of companies — used for the agent's company dropdown
  // and to resolve a company name on the Agents tab. The Companies tab paginates
  // its own table separately (see CompaniesTab) but refreshes this lookup too.
  const [companies, setCompanies] = useState<Company[]>([]);
  const [toast, setToast] = useState<string | null>(null);
  const flash = (m: string) => { setToast(m); window.setTimeout(() => setToast(null), 3000); };

  async function loadCompanyLookup() {
    try { setCompanies(await apiClient<Company[]>("/api/v1/tour-operator-companies?all=true")); }
    catch (e) { flash(extractError(e, "Could not load tour agent companies.")); }
  }
  useEffect(() => { void loadCompanyLookup(); }, []);

  return (
    <>
      <Topbar title="Master Data" subtitle="Tour Agents" />

      <div className="p-6">
        <div className="mb-5">
          <h2 className="font-heading text-xl font-bold">Tour agents</h2>
          <p className="text-sm text-muted-foreground">
            Travel agents, guides and the agencies they belong to. Pick one on a bill at the till and their commission is worked out automatically when the bill is settled.
          </p>
        </div>

        <div className="mb-5 flex gap-1 border-b border-border">
          <button
            onClick={() => setTab("agents")}
            className={`flex items-center gap-1.5 border-b-2 px-4 py-2 text-sm font-semibold ${tab === "agents" ? "border-primary text-primary" : "border-transparent text-muted-foreground hover:text-foreground"}`}
          >
            <Plane className="size-4" /> Agents
          </button>
          <button
            onClick={() => setTab("companies")}
            className={`flex items-center gap-1.5 border-b-2 px-4 py-2 text-sm font-semibold ${tab === "companies" ? "border-primary text-primary" : "border-transparent text-muted-foreground hover:text-foreground"}`}
          >
            <Briefcase className="size-4" /> Companies
          </button>
        </div>

        {tab === "agents" ? (
          <AgentsTab companies={companies} flash={flash} />
        ) : (
          <CompaniesTab flash={flash} reloadLookup={loadCompanyLookup} />
        )}
      </div>

      {toast && (
        <div className="fixed bottom-12 left-1/2 z-[70] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">
          {toast}
        </div>
      )}
    </>
  );
}

// ── Agents tab ──────────────────────────────────────────────────────────────

function AgentsTab({
  companies, flash,
}: { companies: Company[]; flash: (m: string) => void }) {
  const [agents, setAgents] = useState<Agent[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [kindFilter, setKindFilter] = useState("");
  const [open, setOpen] = useState(false);
  const [form, setForm] = useState<AgentForm>(emptyAgentForm);
  const [submitting, setSubmitting] = useState(false);
  const [busyId, setBusyId] = useState<string | null>(null);

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  async function reload() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ pageNumber: String(pageNumber), pageSize: String(pageSize) });
      if (search.trim()) params.set("search", search.trim());
      if (statusFilter) params.set("isActive", statusFilter);
      if (kindFilter) params.set("kind", kindFilter);
      const res = await apiClient<{ data: Agent[]; pagination: { totalCount: number; totalPages: number } }>(`/api/v1/tour-operators/paged?${params.toString()}`);
      setAgents(res.data); setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages);
    } catch (e) { flash(extractError(e, "Could not load tour agents.")); }
    finally { setLoading(false); }
  }
  useEffect(() => { void reload(); }, [pageNumber, pageSize, statusFilter, kindFilter]);
  useEffect(() => {
    const t = window.setTimeout(() => { setPageNumber(1); void reload(); }, 350);
    return () => window.clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search]);

  const visible = agents;
  const companyName = (id: string | null) => companies.find((c) => c.id === id)?.name ?? null;

  function openNew() { setForm(emptyAgentForm); setOpen(true); }
  function openEdit(a: Agent) {
    setForm({
      id: a.id, code: a.code, name: a.name, isActive: a.isActive,
      companyId: a.companyId ?? "", title: a.title ?? "", nic: a.nic ?? "",
      address1: a.address1 ?? "", address2: a.address2 ?? "", address3: a.address3 ?? "", countryCode: a.countryCode ?? "LK",
      mobile: a.mobile ?? "", email: a.email ?? "", remarks: a.remarks ?? "",
      commissionBasis: a.amount > 0 && a.commissionPercent <= 0 ? "amount" : "percent",
      commissionPercent: String(a.commissionPercent), amount: String(a.amount ?? 0),
    });
    setOpen(true);
  }

  async function save() {
    if (!form.code.trim() || !form.name.trim()) { flash("Code and name are required."); return; }
    setSubmitting(true);
    try {
      // Only one of Percentage / Amount is sent; the other is zeroed based on the chosen basis.
      const commissionPercent = form.commissionBasis === "percent" ? Number(form.commissionPercent) || 0 : 0;
      const amount = form.commissionBasis === "amount" ? Number(form.amount) || 0 : 0;
      await apiClient("/api/v1/tour-operators", {
        method: "POST",
        body: JSON.stringify({
          id: form.id, code: form.code.trim(), name: form.name.trim(),
          // Kind is no longer user-chosen: an agent linked to a company is "individual"
          // (a person acting for that agency); one with no company link is a standalone "company".
          commissionPercent, kind: form.companyId ? "individual" : "company", isActive: form.isActive,
          companyId: form.companyId || null, title: form.title || null, nic: form.nic.trim() || null,
          address1: form.address1.trim() || null, address2: form.address2.trim() || null, address3: form.address3.trim() || null,
          countryCode: form.countryCode || null,
          mobile: form.mobile.trim() || null, email: form.email.trim() || null,
          amount, remarks: form.remarks.trim() || null,
        }),
      });
      setOpen(false);
      flash(form.id ? "Tour agent updated." : "Tour agent created.");
      await reload();
    } catch (e) { flash(extractError(e, "Could not save the tour agent.")); }
    finally { setSubmitting(false); }
  }

  async function remove(a: Agent) {
    if (!(await confirmDialog({ title: `Remove ${a.name}?`, body: "It will no longer appear at the till. Past bills keep their commission.", confirmLabel: "Remove", danger: true }))) return;
    setBusyId(a.id);
    try { await apiClient(`/api/v1/tour-operators/${a.id}`, { method: "DELETE" }); flash("Tour agent removed."); await reload(); }
    catch (e) { flash(extractError(e, "Could not remove the tour agent.")); }
    finally { setBusyId(null); }
  }

  return (
    <>
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-wrap items-center gap-3">
          <div className="relative">
            <Search className="absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
            <input
              value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Search agents"
              className="rounded-lg border border-border bg-card py-1.5 pl-8 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
            />
          </div>
          <select value={kindFilter} onChange={(e) => setKindFilter(e.target.value)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary">
            <option value="">All types</option>
            <option value="company">Company</option>
            <option value="individual">Individual</option>
          </select>
          <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary">
            <option value="">All status</option>
            <option value="true">Active</option>
            <option value="false">Inactive</option>
          </select>
        </div>
        <button onClick={openNew} className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark">
          <Plus className="size-4" /> New agent
        </button>
      </div>

      <div className="card overflow-hidden">
        {loading ? (
          <div className="space-y-2 p-4">
            {Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}
          </div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-2.5 font-medium">Agent</th>
                <th className="px-4 py-2.5 font-medium">Type</th>
                <th className="px-4 py-2.5 font-medium">Company</th>
                <th className="px-4 py-2.5 text-right font-medium">Commission</th>
                <th className="px-4 py-2.5 font-medium">Status</th>
                <th className="px-4 py-2.5 text-right font-medium">Actions</th>
              </tr>
            </thead>
            <tbody>
              {visible.map((a, index) => (
                <tr key={a.id} className={index % 2 ? "bg-muted/20" : ""}>
                  <td className="px-4 py-2.5">
                    <div className="flex items-center gap-2.5">
                      <div className="flex size-8 items-center justify-center rounded-lg bg-primary-tint text-primary">
                        <Plane className="size-4" />
                      </div>
                      <div>
                        <div className="font-medium">{a.title ? `${a.title} ` : ""}{a.name}</div>
                        <div className="font-mono text-xs text-muted-foreground">{a.code}</div>
                      </div>
                    </div>
                  </td>
                  <td className="px-4 py-2.5">
                    <span className="pill pill-idle capitalize">{a.kind === "individual" ? "Individual" : "Company"}</span>
                  </td>
                  <td className="px-4 py-2.5 text-muted-foreground">{companyName(a.companyId) || "—"}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">
                    {a.commissionPercent > 0 ? `${a.commissionPercent}%` : a.amount > 0 ? a.amount.toLocaleString() : "—"}
                  </td>
                  <td className="px-4 py-2.5">
                    <span className={`pill ${a.isActive ? "pill-paid" : "pill-void"}`}>{a.isActive ? "Active" : "Inactive"}</span>
                  </td>
                  <td className="px-4 py-2.5 text-right">
                    <div className="flex justify-end gap-2">
                      <button onClick={() => openEdit(a)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted">Edit</button>
                      <button disabled={busyId === a.id} onClick={() => remove(a)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-status-error hover:bg-muted disabled:opacity-50">Remove</button>
                    </div>
                  </td>
                </tr>
              ))}
              {visible.length === 0 && (
                <tr><td colSpan={6} className="px-4 py-10 text-center text-muted-foreground">
                  {totalCount === 0 ? "No tour agents yet." : "No agents match your filters."}
                </td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
        <select
          value={pageSize}
          onChange={(e) => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
          className="rounded-lg border border-border bg-surface px-2 py-1.5 text-xs"
        >
          {[10, 25, 50, 100].map((n) => <option key={n} value={n}>{n} / page</option>)}
        </select>
        <Pagination
          page={pageNumber}
          totalPages={totalPages}
          total={totalCount}
          from={totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1}
          to={Math.min(pageNumber * pageSize, totalCount)}
          setPage={setPageNumber}
          noun="agents"
          className="mt-0 flex-1"
        />
      </div>

      {open && (
        <AgentModal form={form} companies={companies} submitting={submitting} setForm={setForm} onClose={() => !submitting && setOpen(false)} onSave={save} />
      )}
    </>
  );
}

function AgentModal({
  form, companies, submitting, setForm, onClose, onSave,
}: {
  form: AgentForm; companies: Company[]; submitting: boolean;
  setForm: React.Dispatch<React.SetStateAction<AgentForm>>; onClose: () => void; onSave: () => void;
}) {
  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div className="max-h-[90vh] w-full max-w-4xl overflow-y-auto rounded-xl bg-card p-6 shadow-2xl" onClick={(e) => e.stopPropagation()}>
        <div className="mb-4 flex items-start justify-between">
          <h3 className="font-heading text-lg font-bold">{form.id ? "Edit tour agent" : "New tour agent"}</h3>
          <button onClick={onClose} className="rounded-lg p-1.5 text-muted-foreground hover:bg-muted"><X className="size-5" /></button>
        </div>

        <div className="grid grid-cols-1 gap-x-8 gap-y-3 sm:grid-cols-2">
          <div className="space-y-3">
            <Input label="Tour Agent Code" value={form.code} onChange={(v) => setForm((f) => ({ ...f, code: v.toUpperCase() }))} disabled={!!form.id} />
            <Select label="Title" value={form.title} onChange={(v) => setForm((f) => ({ ...f, title: v }))} options={[["", "—"], ...TITLES.map((t): [string, string] => [t, t])]} />
            <Input label="Mobile" value={form.mobile} onChange={(v) => setForm((f) => ({ ...f, mobile: v }))} />
            <Input label="Street Address" value={form.address1} onChange={(v) => setForm((f) => ({ ...f, address1: v }))} />
            <Input label="City" value={form.address2} onChange={(v) => setForm((f) => ({ ...f, address2: v }))} />
            <Input label="Postal Code" value={form.address3} onChange={(v) => setForm((f) => ({ ...f, address3: v }))} />
            <Select
              label="Country" value={form.countryCode} onChange={(v) => setForm((f) => ({ ...f, countryCode: v }))}
              options={[["", "Select country…"], ...COUNTRIES.map((c): [string, string] => [c.value, c.label])]}
            />
            <Select
              label="Tour Agent Company" value={form.companyId} onChange={(v) => setForm((f) => ({ ...f, companyId: v }))}
              options={[["", "— Select a Company —"], ...companies.map((c): [string, string] => [c.id, `${c.code} — ${c.name}`])]}
            />
          </div>

          <div className="space-y-3">
            <Input label="NIC" value={form.nic} onChange={(v) => setForm((f) => ({ ...f, nic: v }))} />
            <Input label="Name" value={form.name} onChange={(v) => setForm((f) => ({ ...f, name: v }))} />
            <Input label="E Mail" value={form.email} onChange={(v) => setForm((f) => ({ ...f, email: v }))} />
            <CommissionInput
              basis={form.commissionBasis} onBasisChange={(b) => setForm((f) => ({ ...f, commissionBasis: b }))}
              percentValue={form.commissionPercent} onPercentChange={(v) => setForm((f) => ({ ...f, commissionPercent: v }))}
              amountValue={form.amount} onAmountChange={(v) => setForm((f) => ({ ...f, amount: v }))}
            />
            <Input label="Remarks" value={form.remarks} onChange={(v) => setForm((f) => ({ ...f, remarks: v }))} />
            <label className="flex items-center gap-2 pt-2.5 text-sm">
              <input type="checkbox" checked={form.isActive} onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))} className="size-4 rounded border-border text-primary" />
              IsActive
            </label>
          </div>
        </div>

        <div className="mt-6 flex gap-2">
          <button onClick={onClose} disabled={submitting} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50">Cancel</button>
          <button onClick={onSave} disabled={submitting} className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">
            {submitting ? "Saving…" : form.id ? "Save changes" : "Create"}
          </button>
        </div>
      </div>
    </div>
  );
}

// ── Companies tab ────────────────────────────────────────────────────────────

function CompaniesTab({
  flash, reloadLookup,
}: { flash: (m: string) => void; reloadLookup: () => Promise<void> }) {
  const [companies, setCompanies] = useState<Company[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [open, setOpen] = useState(false);
  const [form, setForm] = useState<CompanyForm>(emptyCompanyForm);
  const [submitting, setSubmitting] = useState(false);
  const [busyId, setBusyId] = useState<string | null>(null);

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  async function reload() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ pageNumber: String(pageNumber), pageSize: String(pageSize) });
      if (search.trim()) params.set("search", search.trim());
      if (statusFilter) params.set("isActive", statusFilter);
      const res = await apiClient<{ data: Company[]; pagination: { totalCount: number; totalPages: number } }>(`/api/v1/tour-operator-companies/paged?${params.toString()}`);
      setCompanies(res.data); setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages);
    } catch (e) { flash(extractError(e, "Could not load tour agent companies.")); }
    finally { setLoading(false); }
  }
  useEffect(() => { void reload(); }, [pageNumber, pageSize, statusFilter]);
  useEffect(() => {
    const t = window.setTimeout(() => { setPageNumber(1); void reload(); }, 350);
    return () => window.clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search]);

  const visible = companies;

  function openNew() { setForm(emptyCompanyForm); setOpen(true); }
  function openEdit(c: Company) {
    setForm({
      id: c.id, code: c.code, name: c.name, address1: c.address1 ?? "", address2: c.address2 ?? "", countryCode: c.countryCode ?? "LK",
      mobile: c.mobile ?? "", telephone: c.telephone ?? "", faxNo: c.faxNo ?? "", email: c.email ?? "",
      webAddress: c.webAddress ?? "", contactPerson: c.contactPerson ?? "", isActive: c.isActive,
      commissionBasis: c.commissionPercent > 0 ? "percent" : "amount",
      commissionPercent: String(c.commissionPercent ?? 0), commissionAmount: String(c.commissionAmount ?? 0),
    });
    setOpen(true);
  }

  async function save() {
    if (!form.code.trim() || !form.name.trim()) { flash("Code and name are required."); return; }
    setSubmitting(true);
    try {
      // Only one of Percentage / Amount is sent; the other is zeroed based on the chosen basis.
      const commissionPercent = form.commissionBasis === "percent" ? Number(form.commissionPercent) || 0 : 0;
      const commissionAmount = form.commissionBasis === "amount" ? Number(form.commissionAmount) || 0 : 0;
      await apiClient("/api/v1/tour-operator-companies", {
        method: "POST",
        body: JSON.stringify({
          id: form.id, code: form.code.trim(), name: form.name.trim(),
          address1: form.address1.trim() || null, address2: form.address2.trim() || null,
          countryCode: form.countryCode || null,
          mobile: form.mobile.trim() || null, telephone: form.telephone.trim() || null,
          faxNo: form.faxNo.trim() || null, email: form.email.trim() || null,
          webAddress: form.webAddress.trim() || null, contactPerson: form.contactPerson.trim() || null,
          commissionPercent, commissionAmount, isActive: form.isActive,
        }),
      });
      setOpen(false);
      flash(form.id ? "Tour agent company updated." : "Tour agent company created.");
      await Promise.all([reload(), reloadLookup()]);
    } catch (e) { flash(extractError(e, "Could not save the tour agent company.")); }
    finally { setSubmitting(false); }
  }

  async function remove(c: Company) {
    if (!(await confirmDialog({ title: `Remove ${c.name}?`, body: "Tour agents linked to this company must be reassigned first.", confirmLabel: "Remove", danger: true }))) return;
    setBusyId(c.id);
    try { await apiClient(`/api/v1/tour-operator-companies/${c.id}`, { method: "DELETE" }); flash(`${c.name} removed.`); await Promise.all([reload(), reloadLookup()]); }
    catch (e) { flash(extractError(e, "Could not remove the tour agent company.")); }
    finally { setBusyId(null); }
  }

  return (
    <>
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-wrap items-center gap-3">
          <div className="relative">
            <Search className="absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
            <input
              value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Search companies"
              className="rounded-lg border border-border bg-card py-1.5 pl-8 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
            />
          </div>
          <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary">
            <option value="">All status</option>
            <option value="true">Active</option>
            <option value="false">Inactive</option>
          </select>
        </div>
        <button onClick={openNew} className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark">
          <Plus className="size-4" /> New company
        </button>
      </div>

      <div className="card overflow-hidden">
        {loading ? (
          <div className="space-y-2 p-4">
            {Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}
          </div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-2.5 font-medium">Company</th>
                <th className="px-4 py-2.5 font-medium">Contact</th>
                <th className="px-4 py-2.5 font-medium">Address</th>
                <th className="px-4 py-2.5 text-right font-medium">Commission</th>
                <th className="px-4 py-2.5 font-medium">Status</th>
                <th className="px-4 py-2.5 text-right font-medium">Actions</th>
              </tr>
            </thead>
            <tbody>
              {visible.map((c, index) => (
                <tr key={c.id} className={index % 2 ? "bg-muted/20" : ""}>
                  <td className="px-4 py-2.5">
                    <div className="flex items-center gap-2.5">
                      <div className="flex size-8 items-center justify-center rounded-lg bg-primary-tint text-primary">
                        <Briefcase className="size-4" />
                      </div>
                      <div>
                        <div className="font-medium">{c.name}</div>
                        <div className="font-mono text-xs text-muted-foreground">{c.code}</div>
                      </div>
                    </div>
                  </td>
                  <td className="px-4 py-2.5">
                    <div>{c.contactPerson || "—"}</div>
                    <div className="text-xs text-muted-foreground">{c.mobile || c.telephone || c.email || "—"}</div>
                  </td>
                  <td className="px-4 py-2.5 text-muted-foreground">{c.address1 || "—"}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">
                    {c.commissionPercent > 0 ? `${c.commissionPercent}%` : c.commissionAmount > 0 ? c.commissionAmount.toLocaleString() : "—"}
                  </td>
                  <td className="px-4 py-2.5">
                    <span className={`pill ${c.isActive ? "pill-paid" : "pill-void"}`}>{c.isActive ? "Active" : "Inactive"}</span>
                  </td>
                  <td className="px-4 py-2.5 text-right">
                    <div className="flex justify-end gap-2">
                      <button onClick={() => openEdit(c)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted">Edit</button>
                      <button disabled={busyId === c.id} onClick={() => remove(c)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-status-error hover:bg-muted disabled:opacity-50">Remove</button>
                    </div>
                  </td>
                </tr>
              ))}
              {visible.length === 0 && (
                <tr><td colSpan={6} className="px-4 py-10 text-center text-muted-foreground">
                  {totalCount === 0 ? "No tour agent companies yet." : "No companies match your filters."}
                </td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
        <select
          value={pageSize}
          onChange={(e) => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
          className="rounded-lg border border-border bg-surface px-2 py-1.5 text-xs"
        >
          {[10, 25, 50, 100].map((n) => <option key={n} value={n}>{n} / page</option>)}
        </select>
        <Pagination
          page={pageNumber}
          totalPages={totalPages}
          total={totalCount}
          from={totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1}
          to={Math.min(pageNumber * pageSize, totalCount)}
          setPage={setPageNumber}
          noun="companies"
          className="mt-0 flex-1"
        />
      </div>

      {open && (
        <CompanyModal form={form} submitting={submitting} setForm={setForm} onClose={() => !submitting && setOpen(false)} onSave={save} />
      )}
    </>
  );
}

function CompanyModal({
  form, submitting, setForm, onClose, onSave,
}: {
  form: CompanyForm; submitting: boolean; setForm: React.Dispatch<React.SetStateAction<CompanyForm>>; onClose: () => void; onSave: () => void;
}) {
  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div className="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-xl bg-card p-6 shadow-2xl" onClick={(e) => e.stopPropagation()}>
        <div className="mb-4 flex items-start justify-between">
          <h3 className="font-heading text-lg font-bold">{form.id ? "Edit tour agent company" : "New tour agent company"}</h3>
          <button onClick={onClose} className="rounded-lg p-1.5 text-muted-foreground hover:bg-muted"><X className="size-5" /></button>
        </div>

        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <Input label="Code" value={form.code} onChange={(v) => setForm((f) => ({ ...f, code: v.toUpperCase() }))} disabled={!!form.id} />
            <Input label="Name" value={form.name} onChange={(v) => setForm((f) => ({ ...f, name: v }))} />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <Input label="Street Address" value={form.address1} onChange={(v) => setForm((f) => ({ ...f, address1: v }))} />
            <Input label="City" value={form.address2} onChange={(v) => setForm((f) => ({ ...f, address2: v }))} />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <Select
              label="Country" value={form.countryCode} onChange={(v) => setForm((f) => ({ ...f, countryCode: v }))}
              options={[["", "Select country…"], ...COUNTRIES.map((c): [string, string] => [c.value, c.label])]}
            />
            <Input label="Mobile" value={form.mobile} onChange={(v) => setForm((f) => ({ ...f, mobile: v }))} />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <Input label="Telephone" value={form.telephone} onChange={(v) => setForm((f) => ({ ...f, telephone: v }))} />
            <Input label="Fax No" value={form.faxNo} onChange={(v) => setForm((f) => ({ ...f, faxNo: v }))} />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <Input label="E Mail" value={form.email} onChange={(v) => setForm((f) => ({ ...f, email: v }))} />
            <Input label="Web Address" value={form.webAddress} onChange={(v) => setForm((f) => ({ ...f, webAddress: v }))} />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <Input label="Contact Person" value={form.contactPerson} onChange={(v) => setForm((f) => ({ ...f, contactPerson: v }))} />
            <CommissionInput
              basis={form.commissionBasis} onBasisChange={(b) => setForm((f) => ({ ...f, commissionBasis: b }))}
              percentValue={form.commissionPercent} onPercentChange={(v) => setForm((f) => ({ ...f, commissionPercent: v }))}
              amountValue={form.commissionAmount} onAmountChange={(v) => setForm((f) => ({ ...f, commissionAmount: v }))}
            />
          </div>

          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" checked={form.isActive} onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))} className="size-4 rounded border-border text-primary" />
            IsActive
          </label>
        </div>

        <div className="mt-6 flex gap-2">
          <button onClick={onClose} disabled={submitting} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50">Cancel</button>
          <button onClick={onSave} disabled={submitting} className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">
            {submitting ? "Saving…" : form.id ? "Save changes" : "Create"}
          </button>
        </div>
      </div>
    </div>
  );
}

// ── Shared bits ──────────────────────────────────────────────────────────────

function Input({ label, value, onChange, disabled }: { label: string; value: string; onChange: (value: string) => void; disabled?: boolean }) {
  return (
    <div>
      <label className="mb-1 block text-sm font-semibold">{label}</label>
      <input
        value={value} onChange={(e) => onChange(e.target.value)} disabled={disabled}
        className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20 disabled:opacity-60"
      />
    </div>
  );
}

function Select({ label, value, onChange, options }: { label: string; value: string; onChange: (value: string) => void; options: [string, string][] }) {
  return (
    <div>
      <label className="mb-1 block text-sm font-semibold">{label}</label>
      <select
        value={value} onChange={(e) => onChange(e.target.value)}
        className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
      >
        {options.map(([v, l]) => <option key={v} value={v}>{l}</option>)}
      </select>
    </div>
  );
}

// Only one commission basis applies at a time — switching clears the other so a stray
// leftover value can't silently combine with the newly chosen one on save.
function CommissionInput({
  basis, onBasisChange, percentValue, onPercentChange, amountValue, onAmountChange,
}: {
  basis: CommissionBasis; onBasisChange: (b: CommissionBasis) => void;
  percentValue: string; onPercentChange: (v: string) => void;
  amountValue: string; onAmountChange: (v: string) => void;
}) {
  return (
    <div>
      <div className="mb-1 flex items-center justify-between">
        <label className="text-sm font-semibold">Commission</label>
        <div className="flex overflow-hidden rounded-lg border border-border text-xs font-medium">
          <button
            type="button"
            onClick={() => { onBasisChange("percent"); onAmountChange("0"); }}
            className={`px-2.5 py-1 ${basis === "percent" ? "bg-primary text-primary-foreground" : "bg-surface text-muted-foreground hover:bg-muted"}`}
          >
            Percentage
          </button>
          <button
            type="button"
            onClick={() => { onBasisChange("amount"); onPercentChange("0"); }}
            className={`px-2.5 py-1 ${basis === "amount" ? "bg-primary text-primary-foreground" : "bg-surface text-muted-foreground hover:bg-muted"}`}
          >
            Amount
          </button>
        </div>
      </div>
      {basis === "percent" ? (
        <input
          value={percentValue} inputMode="decimal" placeholder="Percentage %"
          onChange={(e) => onPercentChange(e.target.value.replace(/[^0-9.]/g, ""))}
          className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
        />
      ) : (
        <input
          value={amountValue} inputMode="decimal" placeholder="Amount"
          onChange={(e) => onAmountChange(e.target.value.replace(/[^0-9.]/g, ""))}
          className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
        />
      )}
    </div>
  );
}

function extractError(error: unknown, fallback: string) {
  if (error instanceof Error) return error.message || fallback;
  return fallback;
}
