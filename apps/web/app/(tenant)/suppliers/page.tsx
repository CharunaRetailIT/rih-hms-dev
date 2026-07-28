"use client";

import { useEffect, useState } from "react";
import { Topbar } from "@/components/app-shell/Topbar";
import { apiClient } from "@/lib/api-client";
import { Pagination } from "@/components/ui/Pagination";
import { Plus, Search, Truck, X, Building2, Tag } from "lucide-react";
import { COUNTRIES } from "@/lib/regions";

type Supplier = {
  id: string;
  code: string;
  name: string;
  contactName: string | null;
  phone: string | null;
  email: string | null;
  address: string | null;
  paymentTermsDays: number;
  isActive: boolean;
  isVatRegistered: boolean;
  vatRegistrationNumber: string | null;
  countryCode: string | null;
  province: string | null;
  district: string | null;
  postalCode: string | null;
  supplierGroupId: string | null;
  supplierGroupName: string | null;
  supplierTypeId: string | null;
  supplierTypeName: string | null;
};

type SupplierGroup = {
  id: string;
  code: string;
  name: string;
  remark: string | null;
  isActive: boolean;
};

type SupplierType = {
  id: string;
  code: string;
  name: string;
  remark: string | null;
  isActive: boolean;
};

type PaginationMeta = {
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
};

type PagedResponse = {
  data: Supplier[];
  pagination: PaginationMeta;
};

type PagedGroupsResponse = { data: SupplierGroup[]; pagination: PaginationMeta };
type PagedTypesResponse = { data: SupplierType[]; pagination: PaginationMeta };

type SupplierForm = {
  id: string | null;
  code: string;
  name: string;
  contactName: string;
  phone: string;
  email: string;
  address: string;
  paymentTermsDays: string;
  isActive: boolean;
  isVatRegistered: boolean;
  vatRegistrationNumber: string;
  countryCode: string;
  province: string;
  district: string;
  postalCode: string;
  supplierGroupId: string;
  supplierTypeId: string;
};

type SimpleMasterForm = {
  id: string | null;
  code: string;
  name: string;
  remark: string;
  isActive: boolean;
};

const emptySupplierForm: SupplierForm = {
  id: null,
  code: "",
  name: "",
  contactName: "",
  phone: "",
  email: "",
  address: "",
  paymentTermsDays: "0",
  isActive: true,
  isVatRegistered: false,
  vatRegistrationNumber: "",
  countryCode: "LK",
  province: "",
  district: "",
  postalCode: "",
  supplierGroupId: "",
  supplierTypeId: "",
};

const emptyMasterForm: SimpleMasterForm = {
  id: null,
  code: "",
  name: "",
  remark: "",
  isActive: true,
};

export default function SuppliersPage() {
  const [tab, setTab] = useState<"suppliers" | "groups" | "types">("suppliers");
  const [items, setItems] = useState<Supplier[]>([]);
  const [groups, setGroups] = useState<SupplierGroup[]>([]);
  const [types, setTypes] = useState<SupplierType[]>([]);
  const [loading, setLoading] = useState(true);

  const [search, setSearch] = useState("");
  const [groupFilter, setGroupFilter] = useState("");
  const [typeFilter, setTypeFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const [supplierOpen, setSupplierOpen] = useState(false);
  const [supplierForm, setSupplierForm] =
    useState<SupplierForm>(emptySupplierForm);
  const [submitting, setSubmitting] = useState(false);
  const [busyId, setBusyId] = useState<string | null>(null);

  const [groupOpen, setGroupOpen] = useState(false);
  const [groupForm, setGroupForm] = useState<SimpleMasterForm>(emptyMasterForm);
  const [groupsData, setGroupsData] = useState<SupplierGroup[]>([]);
  const [groupSearch, setGroupSearch] = useState("");
  const [groupStatusFilter, setGroupStatusFilter] = useState("");
  const [groupPageNumber, setGroupPageNumber] = useState(1);
  const [groupPageSize, setGroupPageSize] = useState(10);
  const [groupTotalCount, setGroupTotalCount] = useState(0);
  const [groupTotalPages, setGroupTotalPages] = useState(1);
  const [groupsLoading, setGroupsLoading] = useState(true);

  const [typeOpen, setTypeOpen] = useState(false);
  const [typeForm, setTypeForm] = useState<SimpleMasterForm>(emptyMasterForm);
  const [typesData, setTypesData] = useState<SupplierType[]>([]);
  const [typeSearch, setTypeSearch] = useState("");
  const [typeStatusFilter, setTypeStatusFilter] = useState("");
  const [typePageNumber, setTypePageNumber] = useState(1);
  const [typePageSize, setTypePageSize] = useState(10);
  const [typeTotalCount, setTypeTotalCount] = useState(0);
  const [typeTotalPages, setTypeTotalPages] = useState(1);
  const [typesLoading, setTypesLoading] = useState(true);

  const [toast, setToast] = useState<string | null>(null);

  function flash(message: string) {
    setToast(message);
    window.setTimeout(() => setToast(null), 3000);
  }

  async function loadMasters() {
    try {
      const [g, t] = await Promise.all([
        apiClient<SupplierGroup[]>("/api/v1/suppliers/groups"),
        apiClient<SupplierType[]>("/api/v1/suppliers/types"),
      ]);

      setGroups(g);
      setTypes(t);
    } catch (e) {
      flash(extractError(e, "Could not load supplier groups/types."));
    }
  }

  async function load() {
    setLoading(true);

    try {
      const params = new URLSearchParams();
      params.set("pageNumber", String(pageNumber));
      params.set("pageSize", String(pageSize));

      if (search.trim()) params.set("search", search.trim());
      if (groupFilter) params.set("supplierGroupId", groupFilter);
      if (typeFilter) params.set("supplierTypeId", typeFilter);
      if (statusFilter) params.set("isActive", statusFilter);

      const res = await apiClient<PagedResponse>(
        `/api/v1/suppliers/paged?${params.toString()}`,
      );

      setItems(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages || 1);
    } catch (e) {
      flash(extractError(e, "Could not load suppliers."));
    } finally {
      setLoading(false);
    }
  }

  async function loadGroups() {
    setGroupsLoading(true);

    try {
      const params = new URLSearchParams();
      params.set("pageNumber", String(groupPageNumber));
      params.set("pageSize", String(groupPageSize));
      if (groupSearch.trim()) params.set("search", groupSearch.trim());
      if (groupStatusFilter) params.set("isActive", groupStatusFilter);

      const res = await apiClient<PagedGroupsResponse>(
        `/api/v1/suppliers/groups/paged?${params.toString()}`,
      );

      setGroupsData(res.data);
      setGroupTotalCount(res.pagination.totalCount);
      setGroupTotalPages(res.pagination.totalPages || 1);
    } catch (e) {
      flash(extractError(e, "Could not load supplier groups."));
    } finally {
      setGroupsLoading(false);
    }
  }

  async function loadTypes() {
    setTypesLoading(true);

    try {
      const params = new URLSearchParams();
      params.set("pageNumber", String(typePageNumber));
      params.set("pageSize", String(typePageSize));
      if (typeSearch.trim()) params.set("search", typeSearch.trim());
      if (typeStatusFilter) params.set("isActive", typeStatusFilter);

      const res = await apiClient<PagedTypesResponse>(
        `/api/v1/suppliers/types/paged?${params.toString()}`,
      );

      setTypesData(res.data);
      setTypeTotalCount(res.pagination.totalCount);
      setTypeTotalPages(res.pagination.totalPages || 1);
    } catch (e) {
      flash(extractError(e, "Could not load supplier types."));
    } finally {
      setTypesLoading(false);
    }
  }

  useEffect(() => {
    void loadMasters();
  }, []);

  useEffect(() => {
    void loadGroups();
  }, [groupPageNumber, groupPageSize, groupStatusFilter]);

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setGroupPageNumber(1);
      void loadGroups();
    }, 350);

    return () => window.clearTimeout(timer);
  }, [groupSearch]);

  useEffect(() => {
    void loadTypes();
  }, [typePageNumber, typePageSize, typeStatusFilter]);

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setTypePageNumber(1);
      void loadTypes();
    }, 350);

    return () => window.clearTimeout(timer);
  }, [typeSearch]);

  useEffect(() => {
    void load();
  }, [pageNumber, pageSize, groupFilter, typeFilter, statusFilter]);

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setPageNumber(1);
      void load();
    }, 350);

    return () => window.clearTimeout(timer);
  }, [search]);

  function openNewSupplier() {
    setSupplierForm(emptySupplierForm);
    setSupplierOpen(true);
  }

  function openEditSupplier(item: Supplier) {
    setSupplierForm({
      id: item.id,
      code: item.code,
      name: item.name,
      contactName: item.contactName ?? "",
      phone: item.phone ?? "",
      email: item.email ?? "",
      address: item.address ?? "",
      paymentTermsDays: String(item.paymentTermsDays),
      isActive: item.isActive,
      isVatRegistered: item.isVatRegistered,
      vatRegistrationNumber: item.vatRegistrationNumber ?? "",
      countryCode: item.countryCode ?? "LK",
      province: item.province ?? "",
      district: item.district ?? "",
      postalCode: item.postalCode ?? "",
      supplierGroupId: item.supplierGroupId ?? "",
      supplierTypeId: item.supplierTypeId ?? "",
    });

    setSupplierOpen(true);
  }

  async function saveSupplier() {
    if (!supplierForm.code.trim()) {
      flash("Supplier code is required.");
      return;
    }

    if (!supplierForm.name.trim()) {
      flash("Supplier name is required.");
      return;
    }

    const paymentTermsDays = Number(supplierForm.paymentTermsDays || "0");

    if (Number.isNaN(paymentTermsDays) || paymentTermsDays < 0) {
      flash("Payment terms must be valid.");
      return;
    }

    setSubmitting(true);

    try {
      await apiClient("/api/v1/suppliers", {
        method: "PUT",
        body: JSON.stringify({
          id: supplierForm.id,
          code: supplierForm.code.trim(),
          name: supplierForm.name.trim(),
          contactName: supplierForm.contactName.trim() || null,
          phone: supplierForm.phone.trim() || null,
          email: supplierForm.email.trim() || null,
          address: supplierForm.address.trim() || null,
          paymentTermsDays,
          isActive: supplierForm.isActive,
          isVatRegistered: supplierForm.isVatRegistered,
          vatRegistrationNumber:
            supplierForm.vatRegistrationNumber.trim() || null,
          countryCode: supplierForm.countryCode.trim() || null,
          province: supplierForm.province.trim() || null,
          district: supplierForm.district.trim() || null,
          postalCode: supplierForm.postalCode.trim() || null,
          supplierGroupId: supplierForm.supplierGroupId || null,
          supplierTypeId: supplierForm.supplierTypeId || null,
        }),
      });

      setSupplierOpen(false);
      flash(supplierForm.id ? "Supplier updated." : "Supplier created.");
      await load();
    } catch (e) {
      flash(extractError(e, "Could not save supplier."));
    } finally {
      setSubmitting(false);
    }
  }

  async function removeSupplier(item: Supplier) {
    setBusyId(item.id);

    try {
      await apiClient(`/api/v1/suppliers/${item.id}`, {
        method: "DELETE",
      });

      flash(`${item.name} removed.`);
      await load();
    } catch (e) {
      flash(extractError(e, "Could not remove supplier."));
    } finally {
      setBusyId(null);
    }
  }

  function openNewGroup() {
    setGroupForm(emptyMasterForm);
    setGroupOpen(true);
  }

  function openEditGroup(item: SupplierGroup) {
    setGroupForm({
      id: item.id,
      code: item.code,
      name: item.name,
      remark: item.remark ?? "",
      isActive: item.isActive,
    });

    setGroupOpen(true);
  }

  async function saveGroup() {
    if (!groupForm.code.trim() || !groupForm.name.trim()) {
      flash("Group code and name are required.");
      return;
    }

    setSubmitting(true);

    try {
      await apiClient("/api/v1/suppliers/groups", {
        method: "PUT",
        body: JSON.stringify({
          id: groupForm.id,
          code: groupForm.code.trim(),
          name: groupForm.name.trim(),
          remark: groupForm.remark.trim() || null,
          isActive: groupForm.isActive,
        }),
      });

      setGroupOpen(false);
      flash(
        groupForm.id ? "Supplier group updated." : "Supplier group created.",
      );
      await Promise.all([loadGroups(), loadMasters()]);
    } catch (e) {
      flash(extractError(e, "Could not save supplier group."));
    } finally {
      setSubmitting(false);
    }
  }

  async function removeGroup(item: SupplierGroup) {
    setBusyId(item.id);

    try {
      await apiClient(`/api/v1/suppliers/groups/${item.id}`, {
        method: "DELETE",
      });

      flash(`${item.name} removed.`);
      await Promise.all([loadGroups(), loadMasters()]);
    } catch (e) {
      flash(extractError(e, "Could not remove supplier group."));
    } finally {
      setBusyId(null);
    }
  }

  function openNewType() {
    setTypeForm(emptyMasterForm);
    setTypeOpen(true);
  }

  function openEditType(item: SupplierType) {
    setTypeForm({
      id: item.id,
      code: item.code,
      name: item.name,
      remark: item.remark ?? "",
      isActive: item.isActive,
    });

    setTypeOpen(true);
  }

  async function saveType() {
    if (!typeForm.code.trim() || !typeForm.name.trim()) {
      flash("Type code and name are required.");
      return;
    }

    setSubmitting(true);

    try {
      await apiClient("/api/v1/suppliers/types", {
        method: "PUT",
        body: JSON.stringify({
          id: typeForm.id,
          code: typeForm.code.trim(),
          name: typeForm.name.trim(),
          remark: typeForm.remark.trim() || null,
          isActive: typeForm.isActive,
        }),
      });

      setTypeOpen(false);
      flash(typeForm.id ? "Supplier type updated." : "Supplier type created.");
      await Promise.all([loadTypes(), loadMasters()]);
    } catch (e) {
      flash(extractError(e, "Could not save supplier type."));
    } finally {
      setSubmitting(false);
    }
  }

  async function removeType(item: SupplierType) {
    setBusyId(item.id);

    try {
      await apiClient(`/api/v1/suppliers/types/${item.id}`, {
        method: "DELETE",
      });

      flash(`${item.name} removed.`);
      await Promise.all([loadTypes(), loadMasters()]);
    } catch (e) {
      flash(extractError(e, "Could not remove supplier type."));
    } finally {
      setBusyId(null);
    }
  }

  const start = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const end = Math.min(pageNumber * pageSize, totalCount);
  const groupStart = groupTotalCount === 0 ? 0 : (groupPageNumber - 1) * groupPageSize + 1;
  const groupEnd = Math.min(groupPageNumber * groupPageSize, groupTotalCount);
  const typeStart = typeTotalCount === 0 ? 0 : (typePageNumber - 1) * typePageSize + 1;
  const typeEnd = Math.min(typePageNumber * typePageSize, typeTotalCount);

  return (
    <>
      <Topbar title="Master Data" subtitle="Suppliers" />

      <div className="p-6">
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Suppliers</h2>
            <p className="text-sm text-muted-foreground">
              Manage vendors, supplier groups, supplier types, VAT and payment
              terms.
            </p>
          </div>

          <div className="flex gap-2">
            {tab === "suppliers" && (
              <button
                onClick={openNewSupplier}
                className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark"
              >
                <Plus className="size-4" />
                New supplier
              </button>
            )}

            {tab === "groups" && (
              <button
                onClick={openNewGroup}
                className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark"
              >
                <Plus className="size-4" />
                New group
              </button>
            )}

            {tab === "types" && (
              <button
                onClick={openNewType}
                className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark"
              >
                <Plus className="size-4" />
                New type
              </button>
            )}
          </div>
        </div>

        <div className="mb-5 flex gap-1 border-b border-border">
          <button
            onClick={() => setTab("suppliers")}
            className={`flex items-center gap-1.5 border-b-2 px-4 py-2 text-sm font-semibold ${tab === "suppliers" ? "border-primary text-primary" : "border-transparent text-muted-foreground hover:text-foreground"}`}
          >
            <Truck className="size-4" /> Suppliers
          </button>
          <button
            onClick={() => setTab("groups")}
            className={`flex items-center gap-1.5 border-b-2 px-4 py-2 text-sm font-semibold ${tab === "groups" ? "border-primary text-primary" : "border-transparent text-muted-foreground hover:text-foreground"}`}
          >
            <Building2 className="size-4" /> Supplier Groups
          </button>
          <button
            onClick={() => setTab("types")}
            className={`flex items-center gap-1.5 border-b-2 px-4 py-2 text-sm font-semibold ${tab === "types" ? "border-primary text-primary" : "border-transparent text-muted-foreground hover:text-foreground"}`}
          >
            <Tag className="size-4" /> Supplier Types
          </button>
        </div>

        {tab === "suppliers" && (<>
        <div className="mb-4 flex flex-wrap items-center gap-3">
          <div className="relative">
            <Search className="absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />

            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search suppliers"
              className="rounded-lg border border-border bg-card py-1.5 pl-8 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
            />
          </div>

          <select
            value={groupFilter}
            onChange={(e) => {
              setGroupFilter(e.target.value);
              setPageNumber(1);
            }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">All groups</option>
            {groups.map((g) => (
              <option key={g.id} value={g.id}>
                {g.code} — {g.name}
              </option>
            ))}
          </select>

          <select
            value={typeFilter}
            onChange={(e) => {
              setTypeFilter(e.target.value);
              setPageNumber(1);
            }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">All types</option>
            {types.map((t) => (
              <option key={t.id} value={t.id}>
                {t.code} — {t.name}
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
                  <th className="px-4 py-2.5 font-medium">Supplier</th>
                  <th className="px-4 py-2.5 font-medium">Contact</th>
                  <th className="px-4 py-2.5 font-medium">Group / Type</th>
                  <th className="px-4 py-2.5 font-medium">VAT</th>
                  <th className="px-4 py-2.5 text-right font-medium">Terms</th>
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
                        <div className="flex size-8 items-center justify-center rounded-lg bg-primary-tint text-primary">
                          <Truck className="size-4" />
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
                      <div>{item.contactName || "—"}</div>
                      <div className="text-xs text-muted-foreground">
                        {item.phone || item.email || "—"}
                      </div>
                    </td>

                    <td className="px-4 py-2.5">
                      <div>{item.supplierGroupName || "—"}</div>
                      <div className="text-xs text-muted-foreground">
                        {item.supplierTypeName || "—"}
                      </div>
                    </td>

                    <td className="px-4 py-2.5">
                      {item.isVatRegistered ? (
                        <div>
                          <span className="pill pill-paid">VAT</span>
                          <div className="mt-1 text-xs text-muted-foreground">
                            {item.vatRegistrationNumber || "No VAT number"}
                          </div>
                        </div>
                      ) : (
                        <span className="pill pill-idle">No VAT</span>
                      )}
                    </td>

                    <td className="px-4 py-2.5 text-right tabular-nums">
                      {item.paymentTermsDays} days
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
                          onClick={() => openEditSupplier(item)}
                          className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted"
                        >
                          Edit
                        </button>

                        <button
                          disabled={busyId === item.id}
                          onClick={() => removeSupplier(item)}
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
                      No suppliers found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={start} to={end} setPage={setPageNumber} noun="suppliers" />
        </>)}

        {tab === "groups" && (
          <MasterList
            items={groupsData}
            loading={groupsLoading}
            busyId={busyId}
            search={groupSearch}
            setSearch={setGroupSearch}
            statusFilter={groupStatusFilter}
            setStatusFilter={(v) => { setGroupStatusFilter(v); setGroupPageNumber(1); }}
            pageSize={groupPageSize}
            setPageSize={(n) => { setGroupPageSize(n); setGroupPageNumber(1); }}
            page={groupPageNumber}
            totalPages={groupTotalPages}
            total={groupTotalCount}
            from={groupStart}
            to={groupEnd}
            setPage={setGroupPageNumber}
            noun="groups"
            emptyLabel="No supplier groups found."
            onEdit={openEditGroup}
            onRemove={removeGroup}
          />
        )}

        {tab === "types" && (
          <MasterList
            items={typesData}
            loading={typesLoading}
            busyId={busyId}
            search={typeSearch}
            setSearch={setTypeSearch}
            statusFilter={typeStatusFilter}
            setStatusFilter={(v) => { setTypeStatusFilter(v); setTypePageNumber(1); }}
            pageSize={typePageSize}
            setPageSize={(n) => { setTypePageSize(n); setTypePageNumber(1); }}
            page={typePageNumber}
            totalPages={typeTotalPages}
            total={typeTotalCount}
            from={typeStart}
            to={typeEnd}
            setPage={setTypePageNumber}
            noun="types"
            emptyLabel="No supplier types found."
            onEdit={openEditType}
            onRemove={removeType}
          />
        )}
      </div>

      {supplierOpen && (
        <SupplierModal
          form={supplierForm}
          groups={groups}
          types={types}
          submitting={submitting}
          setForm={setSupplierForm}
          onClose={() => !submitting && setSupplierOpen(false)}
          onSave={saveSupplier}
        />
      )}

      {groupOpen && (
        <MasterModal
          title={groupForm.id ? "Edit supplier group" : "New supplier group"}
          form={groupForm}
          submitting={submitting}
          setForm={setGroupForm}
          onClose={() => !submitting && setGroupOpen(false)}
          onSave={saveGroup}
        />
      )}

      {typeOpen && (
        <MasterModal
          title={typeForm.id ? "Edit supplier type" : "New supplier type"}
          form={typeForm}
          submitting={submitting}
          setForm={setTypeForm}
          onClose={() => !submitting && setTypeOpen(false)}
          onSave={saveType}
        />
      )}

      {toast && (
        <div className="fixed bottom-12 left-1/2 z-[70] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">
          {toast}
        </div>
      )}
    </>
  );
}

function SupplierModal({
  form,
  groups,
  types,
  submitting,
  setForm,
  onClose,
  onSave,
}: {
  form: SupplierForm;
  groups: SupplierGroup[];
  types: SupplierType[];
  submitting: boolean;
  setForm: React.Dispatch<React.SetStateAction<SupplierForm>>;
  onClose: () => void;
  onSave: () => void;
}) {
  return (
    <div
      className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 p-4"
      onClick={onClose}
    >
      <div
        className="max-h-[90vh] w-full max-w-3xl overflow-y-auto rounded-xl bg-card p-6 shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        <ModalHeader
          title={form.id ? "Edit supplier" : "New supplier"}
          onClose={onClose}
        />

        <div className="grid grid-cols-2 gap-3">
          <Input
            label="Code"
            value={form.code}
            onChange={(v) => setForm((f) => ({ ...f, code: v.toUpperCase() }))}
          />
          <Input
            label="Company Name"
            value={form.name}
            onChange={(v) => setForm((f) => ({ ...f, name: v }))}
          />

          <Input
            label="Contact"
            value={form.contactName}
            onChange={(v) => setForm((f) => ({ ...f, contactName: v }))}
          />
          <Input
            label="Phone"
            value={form.phone}
            onChange={(v) => setForm((f) => ({ ...f, phone: v }))}
          />

          <Input
            label="Email"
            value={form.email}
            onChange={(v) => setForm((f) => ({ ...f, email: v }))}
          />
          <Input
            label="Payment terms days"
            value={form.paymentTermsDays}
            onChange={(v) =>
              setForm((f) => ({
                ...f,
                paymentTermsDays: v.replace(/[^0-9]/g, ""),
              }))
            }
          />

          <div>
            <label className="mb-1 block text-sm font-semibold">
              Supplier group
            </label>
            <select
              value={form.supplierGroupId}
              onChange={(e) =>
                setForm((f) => ({ ...f, supplierGroupId: e.target.value }))
              }
              className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
            >
              <option value="">No group</option>
              {groups.map((g) => (
                <option key={g.id} value={g.id}>
                  {g.code} — {g.name}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="mb-1 block text-sm font-semibold">
              Supplier type
            </label>
            <select
              value={form.supplierTypeId}
              onChange={(e) =>
                setForm((f) => ({ ...f, supplierTypeId: e.target.value }))
              }
              className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
            >
              <option value="">No type</option>
              {types.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.code} — {t.name}
                </option>
              ))}
            </select>
          </div>

          <div className="col-span-2">
            <Input
              label="Address"
              value={form.address}
              onChange={(v) => setForm((f) => ({ ...f, address: v }))}
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-semibold">
              Country
            </label>
            <select
              value={form.countryCode}
              onChange={(e) =>
                setForm((f) => ({ ...f, countryCode: e.target.value }))
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
          <Input
            label="Province"
            value={form.province}
            onChange={(v) => setForm((f) => ({ ...f, province: v }))}
          />

          <Input
            label="District"
            value={form.district}
            onChange={(v) => setForm((f) => ({ ...f, district: v }))}
          />
          <Input
            label="Postal code"
            value={form.postalCode}
            onChange={(v) => setForm((f) => ({ ...f, postalCode: v }))}
          />

          <label className="col-span-2 flex items-center gap-2 text-sm">
            <input
              type="checkbox"
              checked={form.isVatRegistered}
              onChange={(e) =>
                setForm((f) => ({ ...f, isVatRegistered: e.target.checked }))
              }
              className="size-4 rounded border-border text-primary"
            />
            VAT registered
          </label>

          {form.isVatRegistered && (
            <div className="col-span-2">
              <Input
                label="VAT registration number"
                value={form.vatRegistrationNumber}
                onChange={(v) =>
                  setForm((f) => ({ ...f, vatRegistrationNumber: v }))
                }
              />
            </div>
          )}

          <label className="col-span-2 flex items-center gap-2 text-sm">
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

        <ModalActions
          submitting={submitting}
          isEdit={!!form.id}
          onClose={onClose}
          onSave={onSave}
        />
      </div>
    </div>
  );
}

function MasterList({
  items,
  loading,
  busyId,
  search,
  setSearch,
  statusFilter,
  setStatusFilter,
  pageSize,
  setPageSize,
  page,
  totalPages,
  total,
  from,
  to,
  setPage,
  noun,
  emptyLabel,
  onEdit,
  onRemove,
}: {
  items: (SupplierGroup | SupplierType)[];
  loading: boolean;
  busyId: string | null;
  search: string;
  setSearch: (v: string) => void;
  statusFilter: string;
  setStatusFilter: (v: string) => void;
  pageSize: number;
  setPageSize: (n: number) => void;
  page: number;
  totalPages: number;
  total: number;
  from: number;
  to: number;
  setPage: (updater: number | ((p: number) => number)) => void;
  noun: string;
  emptyLabel: string;
  onEdit: (item: any) => void;
  onRemove: (item: any) => void;
}) {
  return (
    <>
      <div className="mb-4 flex flex-wrap items-center gap-3">
        <div className="relative">
          <Search className="absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder={`Search ${noun}`}
            className="rounded-lg border border-border bg-card py-1.5 pl-8 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          />
        </div>

        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
        >
          <option value="">All status</option>
          <option value="true">Active</option>
          <option value="false">Inactive</option>
        </select>

        <select
          value={pageSize}
          onChange={(e) => setPageSize(Number(e.target.value))}
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
            {Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className="h-9 animate-pulse rounded bg-muted" />
            ))}
          </div>
        ) : (
          <table className="w-full text-sm">
            <tbody>
              {items.map((item) => (
                <tr key={item.id} className="border-b border-border last:border-0">
                  <td className="px-4 py-2.5">
                    <div className="font-medium">{item.name}</div>
                    <div className="font-mono text-xs text-muted-foreground">
                      {item.code}
                    </div>
                  </td>

                  <td className="px-4 py-2.5 text-muted-foreground">
                    {item.remark || "—"}
                  </td>

                  <td className="px-4 py-2.5">
                    <span
                      className={`pill ${item.isActive ? "pill-paid" : "pill-void"}`}
                    >
                      {item.isActive ? "Active" : "Inactive"}
                    </span>
                  </td>

                  <td className="px-4 py-2.5 text-right">
                    <button
                      onClick={() => onEdit(item)}
                      className="mr-2 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted"
                    >
                      Edit
                    </button>

                    <button
                      disabled={busyId === item.id}
                      onClick={() => onRemove(item)}
                      className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-status-error hover:bg-muted disabled:opacity-50"
                    >
                      Remove
                    </button>
                  </td>
                </tr>
              ))}

              {items.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-4 py-10 text-center text-muted-foreground">
                    {emptyLabel}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      <Pagination page={page} totalPages={totalPages} total={total} from={from} to={to} setPage={setPage} noun={noun} />
    </>
  );
}

function MasterModal({
  title,
  form,
  submitting,
  setForm,
  onClose,
  onSave,
}: {
  title: string;
  form: SimpleMasterForm;
  submitting: boolean;
  setForm: React.Dispatch<React.SetStateAction<SimpleMasterForm>>;
  onClose: () => void;
  onSave: () => void;
}) {
  return (
    <div
      className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 p-4"
      onClick={onClose}
    >
      <div
        className="w-full max-w-lg rounded-xl bg-card p-6 shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        <ModalHeader title={title} onClose={onClose} />

        <div className="grid grid-cols-2 gap-3">
          <Input
            label="Code"
            value={form.code}
            onChange={(v) => setForm((f) => ({ ...f, code: v.toUpperCase() }))}
          />
          <Input
            label="Name"
            value={form.name}
            onChange={(v) => setForm((f) => ({ ...f, name: v }))}
          />

          <div className="col-span-2">
            <Input
              label="Remark"
              value={form.remark}
              onChange={(v) => setForm((f) => ({ ...f, remark: v }))}
            />
          </div>

          <label className="col-span-2 flex items-center gap-2 text-sm">
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

        <ModalActions
          submitting={submitting}
          isEdit={!!form.id}
          onClose={onClose}
          onSave={onSave}
        />
      </div>
    </div>
  );
}

function ModalHeader({
  title,
  onClose,
}: {
  title: string;
  onClose: () => void;
}) {
  return (
    <div className="mb-4 flex items-start justify-between">
      <h3 className="font-heading text-lg font-bold">{title}</h3>
      <button
        onClick={onClose}
        className="rounded-lg p-1.5 text-muted-foreground hover:bg-muted"
      >
        <X className="size-5" />
      </button>
    </div>
  );
}

function ModalActions({
  submitting,
  isEdit,
  onClose,
  onSave,
}: {
  submitting: boolean;
  isEdit: boolean;
  onClose: () => void;
  onSave: () => void;
}) {
  return (
    <div className="mt-6 flex gap-2">
      <button
        onClick={onClose}
        disabled={submitting}
        className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50"
      >
        Cancel
      </button>

      <button
        onClick={onSave}
        disabled={submitting}
        className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50"
      >
        {submitting ? "Saving…" : isEdit ? "Save changes" : "Create"}
      </button>
    </div>
  );
}

function Input({
  label,
  value,
  onChange,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
}) {
  return (
    <div>
      <label className="mb-1 block text-sm font-semibold">{label}</label>
      <input
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
      />
    </div>
  );
}

function extractError(error: unknown, fallback: string) {
  if (error instanceof Error) return error.message || fallback;
  return fallback;
}
