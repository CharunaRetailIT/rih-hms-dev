"use client";

import { useEffect, useState } from "react";
import { Topbar } from "@/components/app-shell/Topbar";
import { apiClient, lkr } from "@/lib/api-client";
import { Pagination } from "@/components/ui/Pagination";
import { Download, Plus, Search } from "lucide-react";
import { useRouter } from "next/navigation";

type Product = {
  id: string;
  sku: string;
  name: string;
  barcode: string | null;
  categoryId: string | null;
  unitOfMeasureId: string | null;
  basePrice: number;
  costPrice: number;
  isActive: boolean;
  isSold: boolean;
  isStocked: boolean;
  colorHex: string | null;
  sortOrder: number;
  kitchenStationCode: string | null;
  taxClass?: string;
  productType?: string;
};

type Category = {
  id: string;
  name: string;
  code: string;
  colorHex: string | null;
};
type Unit = { id: string; code: string; name: string; symbol: string | null };
type Station = {
  id: string;
  code: string;
  name: string;
  printerName: string | null;
  sortOrder: number;
};
//paginated produts
type PaginationMeta = {
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
};

type PagedProductResult = {
  data: Product[];
  pagination: PaginationMeta;
};

export default function MenuPage() {
  const router = useRouter();

  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [units, setUnits] = useState<Unit[]>([]);
  const [stations, setStations] = useState<Station[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeCat, setActiveCat] = useState<string | null>(null);
  const [search, setSearch] = useState("");
  const [toast, setToast] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  //pagination
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(10);

  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  function flash(m: string) {
    setToast(m);
    window.setTimeout(() => setToast(null), 3500);
  }

  async function load() {
    try {
      const [p, c, u, st] = await Promise.all([
        apiClient<PagedProductResult>(
          `/api/v1/products/paged?pageNumber=${pageNumber}&pageSize=${pageSize}&search=${encodeURIComponent(search)}`,
        ),
        apiClient<Category[]>("/api/v1/categories"),
        apiClient<Unit[]>("/api/v1/units-of-measure"),
        apiClient<Station[]>("/api/v1/kitchen-stations").catch(() => []),
      ]);

      setProducts(p.data);
      setTotalCount(p.pagination.totalCount);
      setTotalPages(p.pagination.totalPages);
      setCategories(c);
      setUnits(u);
      setStations(st);
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  }
  useEffect(() => {
    void load();
  }, [pageNumber, search]);

  const catName = (id: string | null) =>
    categories.find((c) => c.id === id)?.name ?? "—";

  const filtered = products.filter((p) => {
    if (activeCat && p.categoryId !== activeCat) return false;
    if (
      search &&
      !`${p.name} ${p.sku}`.toLowerCase().includes(search.toLowerCase())
    )
      return false;
    return true;
  });

  function handleNewProduct() {
    router.push("/menu/create");
  }

  function handleEditProduct(product: Product) {
    router.push(`/menu/${product.id}/edit`);
  }

  async function remove(p: Product) {
    setBusyId(p.id);
    try {
      await apiClient(`/api/v1/products/${p.id}`, { method: "DELETE" });
      flash(`${p.name} removed.`);
      await load();
    } catch (e) {
      flash(extractError(e, "Could not remove the product."));
    } finally {
      setBusyId(null);
    }
  }

  // Exports the FULL catalog (not just the current page) with the same columns the
  // Data Import hub's Products template expects, so an export → edit → re-import
  // round-trip just works.
  async function exportCsv() {
    try {
      const all = await apiClient<Product[]>("/api/v1/products");
      const head = [
        "Product Code",
        "Name",
        "Barcode",
        "Category",
        "Unit",
        "Sell Price",
        "Cost",
        "Type",
        "Station",
        "Tax Class",
      ];
      const esc = (v: string) => `"${v.replace(/"/g, '""')}"`;
      const body = all.map((p) =>
        [
          p.sku,
          p.name,
          p.barcode ?? "",
          categories.find((c) => c.id === p.categoryId)?.name ?? "",
          units.find((u) => u.id === p.unitOfMeasureId)?.name ?? "",
          p.basePrice,
          p.costPrice,
          p.productType ?? "",
          p.kitchenStationCode
            ? (stations.find((s) => s.code === p.kitchenStationCode)?.name ??
              p.kitchenStationCode)
            : "",
          p.taxClass ?? "",
        ]
          .map((x) => esc(String(x)))
          .join(","),
      );
      const csv = [head.join(","), ...body].join("\n");
      const url = URL.createObjectURL(new Blob([csv], { type: "text/csv" }));
      const a = document.createElement("a");
      a.href = url;
      a.download = `products_${new Date().toISOString().slice(0, 10)}.csv`;
      a.click();
      URL.revokeObjectURL(url);
    } catch (e) {
      flash(extractError(e, "Could not export products."));
    }
  }

  const from = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to = Math.min(pageNumber * pageSize, totalCount);

  return (
    <>
      <Topbar title="Menu" subtitle="Products" />

      <div className="p-6">
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Products</h2>
            <p className="text-sm text-muted-foreground">
              {products.length} items across {categories.length} categories
            </p>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={() => router.push("/data-import")}
              className="flex items-center gap-1.5 rounded-lg border border-border bg-card px-4 py-2 text-sm font-medium hover:bg-muted"
            >
              Import
            </button>
            <button
              onClick={exportCsv}
              className="flex items-center gap-1.5 rounded-lg border border-border bg-card px-4 py-2 text-sm font-medium hover:bg-muted"
            >
              <Download className="size-4" /> Export CSV
            </button>
            <button
              onClick={handleNewProduct}
              className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark"
            >
              <Plus className="size-4" /> New product
            </button>
          </div>
        </div>

        {/* Filter bar */}
        <div className="mb-4 flex flex-wrap items-center gap-2">
          <button
            onClick={() => setActiveCat(null)}
            className={`pill ${!activeCat ? "bg-primary text-primary-foreground" : "pill-idle"}`}
          >
            All
          </button>
          {categories.map((c) => (
            <button
              key={c.id}
              onClick={() => setActiveCat(c.id)}
              className={`pill ${activeCat === c.id ? "bg-primary text-primary-foreground" : "pill-idle"}`}
            >
              {c.name}
            </button>
          ))}
          <div className="relative ml-auto">
            <Search className="absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
            <input
              value={search}
              onChange={(e) => {
                setSearch(e.target.value);
                setPageNumber(1);
              }}
              placeholder="Search by name or SKU"
              className="rounded-lg border border-border bg-card py-1.5 pl-8 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
            />
          </div>
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
                  <th className="px-4 py-2.5 font-medium">Product</th>
                  <th className="px-4 py-2.5 font-medium">SKU</th>
                  <th className="px-4 py-2.5 font-medium">Category</th>
                  <th className="px-4 py-2.5 font-medium">Station</th>
                  <th className="px-4 py-2.5 text-right font-medium">Cost</th>
                  <th className="px-4 py-2.5 text-right font-medium">Price</th>
                  <th className="px-4 py-2.5 font-medium">Status</th>
                  <th className="px-4 py-2.5 text-right font-medium">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((p, i) => (
                  <tr key={p.id} className={i % 2 ? "bg-muted/20" : ""}>
                    <td className="px-4 py-2.5">
                      <div className="flex items-center gap-2.5">
                        <span
                          className="size-3.5 shrink-0 rounded"
                          style={{ background: p.colorHex ?? "#94A3B8" }}
                          title={p.colorHex ?? "No color tag set"}
                        />
                        <span className="font-medium">{p.name}</span>
                        {p.colorHex && (
                          <span className="font-mono text-[10px] uppercase text-muted-foreground">
                            {p.colorHex}
                          </span>
                        )}
                      </div>
                    </td>
                    <td className="px-4 py-2.5 font-mono text-xs text-muted-foreground">
                      {p.sku}
                    </td>
                    <td className="px-4 py-2.5">
                      <span className="pill pill-idle">
                        {catName(p.categoryId)}
                      </span>
                    </td>
                    <td className="px-4 py-2.5 text-xs text-muted-foreground">
                      {p.kitchenStationCode
                        ? (stations.find((s) => s.code === p.kitchenStationCode)
                            ?.name ?? p.kitchenStationCode)
                        : "—"}
                    </td>
                    <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">
                      {lkr(p.costPrice)}
                    </td>
                    <td className="px-4 py-2.5 text-right font-semibold tabular-nums">
                      {lkr(p.basePrice)}
                    </td>
                    <td className="px-4 py-2.5">
                      <span
                        className={`pill ${p.isActive ? "pill-paid" : "pill-void"}`}
                      >
                        {p.isActive ? "Active" : "Inactive"}
                      </span>
                    </td>
                    <td className="px-4 py-2.5 text-right">
                      <div className="flex justify-end gap-2">
                        <button
                          onClick={() => handleEditProduct(p)}
                          className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted"
                        >
                          Edit
                        </button>
                        <button
                          disabled={busyId === p.id}
                          onClick={() => remove(p)}
                          className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-status-error hover:bg-muted disabled:opacity-50"
                        >
                          Remove
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
                {filtered.length === 0 && (
                  <tr>
                    <td
                      colSpan={8}
                      className="px-4 py-10 text-center text-muted-foreground"
                    >
                      No products match your filter.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        <div className="mt-3 flex flex-wrap items-center justify-between gap-2">
          <p className="text-xs text-muted-foreground">prices in LKR, VAT 18% applies at sale</p>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from} to={to} setPage={setPageNumber} noun="products" className="mt-0" />
        </div>
      </div>

      {toast && (
        <div className="fixed bottom-12 left-1/2 z-[70] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">
          {toast}
        </div>
      )}
    </>
  );
}

function extractError(e: unknown, fallback: string): string {
  const msg = (e as Error)?.message ?? "";
  const jsonStart = msg.indexOf("{");
  if (jsonStart !== -1) {
    try {
      const parsed = JSON.parse(msg.slice(jsonStart));
      if (typeof parsed?.error === "string") return parsed.error;
    } catch {
      /* ignore */
    }
  }
  if (msg.includes("403"))
    return "Only an owner or manager can manage products.";
  return msg || fallback;
}
