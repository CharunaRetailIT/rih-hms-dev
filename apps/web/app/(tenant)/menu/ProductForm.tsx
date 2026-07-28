"use client";

import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import {
  ArrowLeft,
  ChefHat,
  DollarSign,
  ImagePlus,
  Layers,
  Package,
  Percent,
  Plus,
  Printer,
  RefreshCw,
  Save,
  Sparkles,
  Tag,
  Trash2,
  Truck,
  Wallet,
  X,
} from "lucide-react";
import { Topbar } from "@/components/app-shell/Topbar";
import { apiClient, lkr } from "@/lib/api-client";
import { generateEan13, barcodeLabelHtml } from "@/lib/barcode";

type LookupItem = {
  id: string;
  code: string;
  name: string;
  locationId?: string | null;
};
type Charge = {
  id: string;
  code: string;
  description: string;
  percentage: number | null;
  amount: number | null;
  chargeTypeName: string;
};
type KitchenStation = {
  id: string;
  code: string;
  name: string;
  locationId: string | null;
  printerName: string | null;
  printerTypeId: string;
};
type PriceLevel = {
  id: string;
  code: string;
  name: string;
  locationId: string | null;
  isDefault: boolean;
  appliesToOrderType: string | null;
};

type ProductLookups = {
  locations: LookupItem[];
  departments: LookupItem[];
  categories: (LookupItem & { parentId?: string | null })[];
  units: (LookupItem & { symbol: string | null })[];
  charges: Charge[];
  suppliers: LookupItem[];
  servingUnits: LookupItem[];
  priceLevels: PriceLevel[];
  kitchenStations: KitchenStation[];
};

type VariantRow = {
  id?: string;
  servingUnitId: string;
  code: string;
  name: string;
  servingQty: string;
  costPrice: string;
  price: string;
  deductStockOnRecipe: boolean;
  sortOrder: number;
  isActive: boolean;
};

type SupplierRow = {
  id?: string;
  supplierId: string;
  isPreferred: boolean;
  isActive: boolean;
};

type PriceRow = {
  id?: string;
  locationId: string;
  productVariantId: string;
  priceLevelId: string;
  costPrice: string;
  price: string;
  qty: string;
};

type KitchenStationRow = {
  id?: string;
  kitchenStationId: string;
  locationId: string;
};

type ChargeRow = {
  id?: string;
  chargeId: string;
};

type Form = {
  locationId: string;
  departmentId: string;
  categoryId: string;
  sku: string;
  barcode: string;
  name: string;
  nameOnInvoice: string;
  description: string;
  refCode01: string;
  refCode02: string;
  unitOfMeasureId: string;
  basePrice: string;
  costPrice: string;
  isTaxable: boolean;
  isTaxInclusive: boolean;
  taxClass: string;
  productType: string;
  isSold: boolean;
  isPurchased: boolean;
  isStocked: boolean;
  isScaleItem: boolean;
  isOpenItem: boolean;
  isLocked: boolean;
  tracksExpiry: boolean;
  allowBelowCost: boolean;
  allowDiscount: boolean;
  discountType: string;
  discountValue: string;
  maxDiscountAmount: string;
  maxDiscountPercentage: string;
  reorderLevel: string;
  reorderQuantity: string;
  parLevel: string;
  imageUrl: string;
  colorHex: string;
  sortOrder: string;
  isAvailableOnline: boolean;
  preferredSupplierId: string;
  isActive: boolean;
};

const emptyForm: Form = {
  locationId: "",
  departmentId: "",
  categoryId: "",
  sku: "",
  barcode: "",
  name: "",
  nameOnInvoice: "",
  description: "",
  refCode01: "",
  refCode02: "",
  unitOfMeasureId: "",
  basePrice: "",
  costPrice: "",
  isTaxable: true,
  isTaxInclusive: false,
  taxClass: "standard",
  productType: "finished",
  isSold: true,
  isPurchased: true,
  isStocked: true,
  isScaleItem: false,
  isOpenItem: false,
  isLocked: false,
  tracksExpiry: false,
  allowBelowCost: false,
  allowDiscount: true,
  discountType: "none",
  discountValue: "0",
  maxDiscountAmount: "",
  maxDiscountPercentage: "",
  reorderLevel: "",
  reorderQuantity: "",
  parLevel: "",
  imageUrl: "",
  colorHex: "",
  sortOrder: "0",
  isAvailableOnline: true,
  preferredSupplierId: "",
  isActive: true,
};

// Curated preset palette — kept short and legible against both light chips and dark text.
const PRESET_COLORS = [
  "#EF4444",
  "#F97316",
  "#F59E0B",
  "#84CC16",
  "#22C55E",
  "#10B981",
  "#14B8A6",
  "#06B6D4",
  "#3B82F6",
  "#6366F1",
  "#8B5CF6",
  "#D946EF",
  "#EC4899",
  "#64748B",
  "#0F172A",
];

// Explicit, self-contained field styling — does not depend on an external
// ".input" utility class existing/being correct, so fields are always visibly
// clickable (border + background + focus ring) even if the app's global
// input styles change or don't cover a given element type.
const INPUT_CLS =
  "input w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 shadow-sm outline-none transition placeholder:text-slate-400 focus:border-primary focus:ring-2 focus:ring-primary/15 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:text-slate-400";

// Tabbed layout (mirrors the old system's Product screen) so a long form
// doesn't turn into one giant scroll — each tab groups a coherent slice of
// the product master rather than every field living on one page.
type TabId =
  | "general"
  | "pricing"
  | "servingUnits"
  | "suppliers"
  | "printers"
  | "charges"
  | "priceLevels";

const TABS: { id: TabId; label: string; icon: React.ComponentType<{ className?: string }> }[] = [
  { id: "general", label: "General", icon: Package },
  { id: "pricing", label: "Pricing & Stock", icon: Wallet },
  { id: "servingUnits", label: "Serving Units", icon: Layers },
  { id: "suppliers", label: "Suppliers", icon: Truck },
  { id: "printers", label: "Printers", icon: Printer },
  { id: "charges", label: "Charges", icon: Percent },
  { id: "priceLevels", label: "Price Level", icon: DollarSign },
];

function num(value: string, fallback = 0) {
  if (!value.trim()) return fallback;
  const n = Number(value);
  return Number.isNaN(n) ? fallback : n;
}

function nullableGuid(value: string) {
  return value || null;
}

function nullableNum(value: string) {
  if (!value.trim()) return null;
  const n = Number(value);
  return Number.isNaN(n) ? null : n;
}

function extractError(e: unknown, fallback: string): string {
  const msg = (e as Error)?.message ?? "";
  const jsonStart = msg.indexOf("{");

  if (jsonStart !== -1) {
    try {
      const parsed = JSON.parse(msg.slice(jsonStart));
      if (typeof parsed?.error === "string") return parsed.error;
    } catch {
      //
    }
  }

  return msg || fallback;
}

function isHexColor(value: string) {
  return /^#[0-9a-fA-F]{6}$/.test(value);
}

/** Which tab a validation error key lives on, so a failed save can jump the user there. */
function tabForErrorKey(key: string): TabId {
  if (key === "unitOfMeasureId" || key === "basePrice" || key === "costPrice") return "pricing";
  if (key.startsWith("variant_")) return "servingUnits";
  return "general";
}

/** Open a print window with a barcode label for the given product. */
function printBarcodeLabel(code: string, name: string, basePrice: string) {
  const html = barcodeLabelHtml(code, name, basePrice ? `LKR ${lkr(Number(basePrice) || 0)}` : "");
  const w = window.open("", "_blank", "width=420,height=360");
  if (w) {
    w.document.open();
    w.document.write(html);
    w.document.close();
  } else {
    alert("Allow pop-ups to print the barcode label.");
  }
}

type ProductFormProps = {
  /** Pass a product id to switch the page into edit mode. Omit for create mode. */
  productId?: string;
};

export default function ProductForm({ productId }: ProductFormProps) {
  const router = useRouter();
  const isEdit = Boolean(productId);

  const [lookups, setLookups] = useState<ProductLookups | null>(null);
  const [form, setForm] = useState<Form>(emptyForm);
  const [variants, setVariants] = useState<VariantRow[]>([]);
  const [suppliers, setSuppliers] = useState<SupplierRow[]>([]);
  const [prices, setPrices] = useState<PriceRow[]>([]);
  const [kitchenStations, setKitchenStations] = useState<KitchenStationRow[]>(
    [],
  );
  const [charges, setCharges] = useState<ChargeRow[]>([]);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [toast, setToast] = useState<{
    message: string;
    tone: "success" | "error";
  } | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [activeTab, setActiveTab] = useState<TabId>("general");

  // Charges tab is hidden for non-taxable products — bounce off it if it was
  // active when Taxable got switched off, so the tab strip and content stay in sync.
  useEffect(() => {
    if (!form.isTaxable && activeTab === "charges") setActiveTab("general");
  }, [form.isTaxable, activeTab]);

  function flash(message: string, tone: "success" | "error" = "error") {
    setToast({ message, tone });
    window.setTimeout(() => setToast(null), 3500);
  }

  async function load() {
    setLoading(true);

    try {
      const lookupData = await apiClient<ProductLookups>(
        "/api/v1/products/lookups",
      );
      setLookups(lookupData);

      if (productId) {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const product = await apiClient<any>(`/api/v1/products/${productId}`);

        setForm({
          locationId: product.locationId ?? "",
          departmentId: product.departmentId ?? "",
          categoryId: product.categoryId ?? "",
          sku: product.sku ?? "",
          barcode: product.barcode ?? "",
          name: product.name ?? "",
          nameOnInvoice: product.nameOnInvoice ?? "",
          description: product.description ?? "",
          refCode01: product.refCode01 ?? "",
          refCode02: product.refCode02 ?? "",
          unitOfMeasureId:
            product.unitOfMeasureId ?? lookupData.units[0]?.id ?? "",
          basePrice: product.basePrice != null ? String(product.basePrice) : "",
          costPrice: product.costPrice != null ? String(product.costPrice) : "",
          isTaxable: product.isTaxable ?? true,
          isTaxInclusive: product.isTaxInclusive ?? false,
          taxClass: product.taxClass ?? "standard",
          productType: product.productType ?? "finished",
          isSold: product.isSold ?? true,
          isPurchased: product.isPurchased ?? true,
          isStocked: product.isStocked ?? true,
          isScaleItem: product.isScaleItem ?? false,
          isOpenItem: product.isOpenItem ?? false,
          isLocked: product.isLocked ?? false,
          tracksExpiry: product.tracksExpiry ?? false,
          allowBelowCost: product.allowBelowCost ?? false,
          allowDiscount: product.allowDiscount ?? true,
          discountType: product.discountType ?? "none",
          discountValue:
            product.discountValue != null ? String(product.discountValue) : "0",
          maxDiscountAmount:
            product.maxDiscountAmount != null
              ? String(product.maxDiscountAmount)
              : "",
          maxDiscountPercentage:
            product.maxDiscountPercentage != null
              ? String(product.maxDiscountPercentage)
              : "",
          reorderLevel:
            product.reorderLevel != null ? String(product.reorderLevel) : "",
          reorderQuantity:
            product.reorderQuantity != null
              ? String(product.reorderQuantity)
              : "",
          parLevel: product.parLevel != null ? String(product.parLevel) : "",
          imageUrl: product.imageUrl ?? "",
          colorHex: product.colorHex ?? "",
          sortOrder:
            product.sortOrder != null ? String(product.sortOrder) : "0",
          isAvailableOnline: product.isAvailableOnline ?? true,
          preferredSupplierId: product.preferredSupplierId ?? "",
          isActive: product.isActive ?? true,
        });

        setVariants(
          (product.variants ?? []).map(
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            (v: any): VariantRow => ({
              id: v.id,
              servingUnitId: v.servingUnitId ?? "",
              code: v.code ?? "",
              name: v.name ?? "",
              servingQty: v.servingQty != null ? String(v.servingQty) : "1",
              costPrice: v.costPrice != null ? String(v.costPrice) : "0",
              price: v.price != null ? String(v.price) : "0",
              deductStockOnRecipe: v.deductStockOnRecipe ?? true,
              sortOrder: v.sortOrder ?? 0,
              isActive: v.isActive ?? true,
            }),
          ),
        );

        setSuppliers(
          (product.suppliers ?? []).map(
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            (s: any): SupplierRow => ({
              id: s.id,
              supplierId: s.supplierId,
              isPreferred: s.isPreferred ?? false,
              isActive: s.isActive ?? true,
            }),
          ),
        );

        setPrices(
          (product.prices ?? []).map(
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            (p: any): PriceRow => ({
              id: p.id,
              locationId: p.locationId ?? "",
              productVariantId: p.productVariantId ?? "",
              priceLevelId: p.priceLevelId,
              costPrice: p.costPrice != null ? String(p.costPrice) : "0",
              price: p.price != null ? String(p.price) : "0",
              qty: p.qty != null ? String(p.qty) : "1",
            }),
          ),
        );

        setKitchenStations(
          (product.kitchenStations ?? []).map(
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            (k: any): KitchenStationRow => ({
              id: k.id,
              kitchenStationId: k.kitchenStationId ?? "",
              locationId: k.locationId ?? "",
            }),
          ),
        );

        setCharges(
          (product.charges ?? []).map(
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            (c: any): ChargeRow => ({
              id: c.id,
              chargeId: c.chargeId ?? "",
            }),
          ),
        );
      } else {
        setForm((f) => ({
          ...f,
          locationId: lookupData.locations[0]?.id ?? "",
          unitOfMeasureId: lookupData.units[0]?.id ?? "",
          departmentId: lookupData.departments[0]?.id ?? "",
          categoryId: lookupData.categories[0]?.id ?? "",
        }));
      }
    } catch (e) {
      flash(extractError(e, "Could not load product data."));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [productId]);

  const filteredDepartments = useMemo(() => {
    if (!lookups) return [];
    if (!form.locationId) return lookups.departments;
    return lookups.departments.filter(
      (x) => !x.locationId || x.locationId === form.locationId,
    );
  }, [lookups, form.locationId]);

  const filteredKitchenStations = useMemo(() => {
    if (!lookups) return [];
    if (!form.locationId) return lookups.kitchenStations;
    return lookups.kitchenStations.filter(
      (x) => !x.locationId || x.locationId === form.locationId,
    );
  }, [lookups, form.locationId]);

  const filteredPriceLevels = useMemo(() => {
    if (!lookups) return [];
    if (!form.locationId) return lookups.priceLevels;
    return lookups.priceLevels.filter(
      (x) => !x.locationId || x.locationId === form.locationId,
    );
  }, [lookups, form.locationId]);

  const margin = useMemo(() => {
    const cost = num(form.costPrice, 0);
    const price = num(form.basePrice, 0);
    if (price <= 0) return null;
    return ((price - cost) / price) * 100;
  }, [form.costPrice, form.basePrice]);

  // Populated by validate() with messages for errors that live on a tab other
  // than the one it switches to — those tabs never mount, so submit() flashes
  // this alongside its own toast instead of leaving them silently invisible.
  let hiddenErrorMessages: string[] = [];

  function validate() {
    const e: Record<string, string> = {};

    if (!form.sku.trim()) e.sku = "Product code is required.";
    if (!form.name.trim()) e.name = "Product name is required.";
    if (!form.unitOfMeasureId) e.unitOfMeasureId = "Unit is required.";
    if (form.basePrice.trim() === "" || Number.isNaN(Number(form.basePrice)))
      e.basePrice = "Valid selling price is required.";
    if (form.costPrice.trim() === "" || Number.isNaN(Number(form.costPrice)))
      e.costPrice = "Valid cost price is required.";
    if (form.colorHex.trim() && !isHexColor(form.colorHex.trim()))
      e.colorHex = "Use a 6-digit hex code, e.g. #16A34A.";

    const validVariants = variants.filter(
      (x) => x.code.trim() || x.name.trim(),
    );

    for (const [index, row] of validVariants.entries()) {
      if (!row.code.trim()) e[`variant_${index}_code`] = "Code required.";
      if (!row.name.trim()) e[`variant_${index}_name`] = "Name required.";
    }

    setErrors(e);

    const errorKeys = Object.keys(e);
    const firstKey = errorKeys[0];
    const primaryTab = firstKey ? tabForErrorKey(firstKey) : activeTab;
    if (firstKey) setActiveTab(primaryTab);

    // Errors on a tab other than the one we just switched to would otherwise be
    // silently invisible (that tab's content isn't mounted) — the caller flashes
    // these alongside the generic "fix the highlighted fields" toast.
    hiddenErrorMessages = errorKeys
      .filter((k) => tabForErrorKey(k) !== primaryTab)
      .map((k) => e[k]);

    return errorKeys.length === 0;
  }

  function addVariant() {
    setVariants((rows) => [
      ...rows,
      {
        servingUnitId: lookups?.servingUnits[0]?.id ?? "",
        code: "",
        name: "",
        servingQty: "1",
        costPrice: form.costPrice || "0",
        price: form.basePrice || "0",
        deductStockOnRecipe: true,
        sortOrder: rows.length,
        isActive: true,
      },
    ]);
  }

  function addSupplier() {
    if (!lookups?.suppliers.length) {
      flash("No suppliers available.");
      return;
    }

    setSuppliers((rows) => [
      ...rows,
      {
        supplierId: lookups.suppliers[0].id,
        isPreferred: rows.length === 0,
        isActive: true,
      },
    ]);
  }

  function addPrice() {
    if (!filteredPriceLevels.length) {
      flash("No price levels available.");
      return;
    }

    setPrices((rows) => [
      ...rows,
      {
        locationId: form.locationId,
        productVariantId: "",
        priceLevelId: filteredPriceLevels[0].id,
        costPrice: form.costPrice || "0",
        price: form.basePrice || "0",
        qty: "1",
      },
    ]);
  }

  function addKitchenStation() {
    if (!lookups?.kitchenStations.length) {
      flash("No kitchen stations available.");
      return;
    }

    setKitchenStations((rows) => [
      ...rows,
      {
        kitchenStationId: filteredKitchenStations[0]?.id ?? lookups.kitchenStations[0].id,
        locationId: form.locationId,
      },
    ]);
  }

  function addCharge() {
    if (!lookups?.charges.length) {
      flash("No charges available. Add one under Charges first.");
      return;
    }

    const alreadyUsed = new Set(charges.map((c) => c.chargeId));
    const next = lookups.charges.find((c) => !alreadyUsed.has(c.id));

    if (!next) {
      flash("All available charges are already added to this product.");
      return;
    }

    setCharges((rows) => [...rows, { chargeId: next.id }]);
  }

  async function submit() {
    if (!validate()) {
      const suffix = hiddenErrorMessages.length
        ? ` Also: ${hiddenErrorMessages.join(" ")}`
        : "";
      flash(`Please fix the highlighted fields before saving.${suffix}`);
      return;
    }

    setSaving(true);

    const cleanedSuppliers = suppliers
      .filter((x) => x.supplierId)
      .map((x, index) => ({
        id: x.id,
        supplierId: x.supplierId,
        isPreferred: x.isPreferred || index === 0,
        isActive: x.isActive,
      }));

    const preferredSupplierId =
      cleanedSuppliers.find((x) => x.isPreferred)?.supplierId ??
      cleanedSuppliers[0]?.supplierId ??
      null;

    const payload = {
      id: productId ?? null,
      locationId: nullableGuid(form.locationId),
      departmentId: nullableGuid(form.departmentId),
      categoryId: nullableGuid(form.categoryId),
      sku: form.sku.trim(),
      name: form.name.trim(),
      barcode: form.barcode.trim() || null,
      nameOnInvoice: form.nameOnInvoice.trim() || null,
      description: form.description.trim() || null,
      refCode01: form.refCode01.trim() || null,
      refCode02: form.refCode02.trim() || null,
      unitOfMeasureId: form.unitOfMeasureId,
      basePrice: num(form.basePrice),
      costPrice: num(form.costPrice),
      isTaxable: form.isTaxable,
      isTaxInclusive: form.isTaxInclusive,
      taxClass: form.taxClass,
      productType: form.productType,
      isSold: form.isSold,
      isPurchased: form.isPurchased,
      isStocked: form.isStocked,
      isScaleItem: form.isScaleItem,
      isOpenItem: form.isOpenItem,
      isLocked: form.isLocked,
      tracksExpiry: form.tracksExpiry,
      allowBelowCost: form.allowBelowCost,
      allowDiscount: form.allowDiscount,
      discountType: form.discountType,
      discountValue: num(form.discountValue),
      maxDiscountAmount: nullableNum(form.maxDiscountAmount),
      maxDiscountPercentage: nullableNum(form.maxDiscountPercentage),
      reorderLevel: nullableNum(form.reorderLevel),
      reorderQuantity: nullableNum(form.reorderQuantity),
      parLevel: nullableNum(form.parLevel),
      colorHex: form.colorHex.trim() || null,
      imageUrl: form.imageUrl.trim() || null,
      sortOrder: num(form.sortOrder),
      isAvailableOnline: form.isAvailableOnline,
      preferredSupplierId,
      isActive: form.isActive,

      variants: variants
        .filter((x) => x.code.trim() && x.name.trim())
        .map((x) => ({
          id: x.id,
          servingUnitId: nullableGuid(x.servingUnitId),
          code: x.code.trim(),
          name: x.name.trim(),
          servingQty: num(x.servingQty, 1),
          costPrice: num(x.costPrice),
          price: num(x.price),
          deductStockOnRecipe: x.deductStockOnRecipe,
          sortOrder: x.sortOrder,
          isActive: x.isActive,
        })),

      suppliers: cleanedSuppliers,

      prices: prices
        .filter((x) => x.priceLevelId && x.price.trim() !== "")
        .map((x) => {
          // A variant added in this same save has no id yet — its price row
          // selector value is its code instead, so resolve which one this is
          // and send the field the backend can actually use.
          const variant = variants.find(
            (v) =>
              (v.id && v.id === x.productVariantId) ||
              (!v.id && v.code === x.productVariantId),
          );

          return {
            id: x.id,
            locationId: nullableGuid(x.locationId),
            productVariantId: variant?.id ?? null,
            variantCode: variant && !variant.id ? variant.code : null,
            priceLevelId: x.priceLevelId,
            costPrice: num(x.costPrice),
            price: num(x.price),
            qty: num(x.qty, 1),
          };
        }),

      kitchenStations: kitchenStations
        .filter((x) => x.kitchenStationId)
        .map((x) => ({
          id: x.id,
          kitchenStationId: x.kitchenStationId,
          locationId: nullableGuid(x.locationId),
        })),

      charges: charges
        .filter((x) => x.chargeId)
        .map((x) => ({
          id: x.id,
          chargeId: x.chargeId,
        })),
    };

    try {
      await apiClient("/api/v1/products", {
        method: "PUT",
        body: JSON.stringify(payload),
      });

      flash(isEdit ? "Product updated." : "Product created.", "success");
      router.push("/menu");
    } catch (e) {
      flash(
        extractError(
          e,
          isEdit ? "Could not update product." : "Could not create product.",
        ),
      );
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete() {
    if (!productId) return;
    if (
      !window.confirm(
        `Delete "${form.name || form.sku}"? This can't be undone.`,
      )
    )
      return;

    setDeleting(true);

    try {
      await apiClient(`/api/v1/products/${productId}`, { method: "DELETE" });
      flash("Product deleted.", "success");
      router.push("/menu");
    } catch (e) {
      flash(extractError(e, "Could not delete product."));
    } finally {
      setDeleting(false);
    }
  }

  async function genCode() {
    try {
      const { code } = await apiClient<{ code: string }>("/api/v1/products/next-code");
      setForm((f) => ({ ...f, sku: code }));
    } catch (e) {
      flash(extractError(e, "Could not generate a product code."));
    }
  }

  if (loading) {
    return (
      <>
        <Topbar
          title={isEdit ? "Edit Product" : "New Product"}
          subtitle={isEdit ? "Loading product…" : "Create product"}
        />
        <div className="p-6">
          <div className="space-y-5">
            {Array.from({ length: 3 }).map((_, section) => (
              <div key={section} className="card space-y-4 p-6">
                <div className="h-4 w-40 animate-pulse rounded bg-muted" />
                <div className="grid grid-cols-4 gap-3">
                  {Array.from({ length: 4 }).map((_, i) => (
                    <div
                      key={i}
                      className="h-10 animate-pulse rounded-lg bg-muted"
                    />
                  ))}
                </div>
              </div>
            ))}
          </div>
        </div>
      </>
    );
  }

  return (
    <>
      <Topbar
        title={isEdit ? "Edit Product" : "New Product"}
        subtitle={
          isEdit
            ? `Editing ${form.sku || "product"}`
            : "Create menu/product master"
        }
      />

      {toast && (
        <div
          className={`fixed bottom-12 left-1/2 z-[80] -translate-x-1/2 rounded-lg px-4 py-2.5 text-sm font-medium text-white shadow-lg ${
            toast.tone === "success" ? "bg-emerald-600" : "bg-on-surface"
          }`}
        >
          {toast.message}
        </div>
      )}

      <div className="p-6 pb-28">
        {/* Sticky action header */}
        <div className="sticky top-0 z-30 -mx-6 mb-6 border-b border-border/70 bg-background/85 px-6 py-4 backdrop-blur supports-[backdrop-filter]:bg-background/70">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <button
                onClick={() => router.push("/menu")}
                className="mb-1 flex items-center gap-1 text-sm font-medium text-muted-foreground transition hover:text-foreground"
              >
                <ArrowLeft className="size-4" />
                Back to products
              </button>

              <div className="flex items-center gap-2.5">
                <span
                  className="size-2.5 shrink-0 rounded-full ring-2 ring-white"
                  style={{
                    backgroundColor: isHexColor(form.colorHex)
                      ? form.colorHex
                      : "#94a3b8",
                  }}
                />
                <h2 className="font-heading text-xl font-bold">
                  {isEdit ? "Edit Product" : "Create Product"}
                </h2>
                {!form.isActive && (
                  <span className="rounded-full bg-status-error/10 px-2 py-0.5 text-xs font-semibold text-status-error">
                    Inactive
                  </span>
                )}
              </div>
              <p className="text-sm text-muted-foreground">
                Full product setup with pricing, serving units, suppliers and
                kitchen station.
              </p>
            </div>

            <div className="flex items-center gap-2">
              {isEdit && (
                <button
                  disabled={deleting}
                  onClick={handleDelete}
                  className="flex items-center gap-1.5 rounded-lg border border-status-error/30 bg-status-error/5 px-4 py-2.5 text-sm font-semibold text-status-error transition hover:bg-status-error/10 disabled:opacity-50"
                >
                  <Trash2 className="size-4" />
                  {deleting ? "Deleting…" : "Delete"}
                </button>
              )}

              <button
                onClick={() => router.push("/menu")}
                className="rounded-lg border border-border bg-card px-4 py-2.5 text-sm font-semibold transition hover:bg-muted"
              >
                Cancel
              </button>

              <button
                disabled={saving}
                onClick={submit}
                className="flex items-center gap-1.5 rounded-lg bg-primary px-5 py-2.5 text-sm font-bold text-primary-foreground shadow-sm transition hover:bg-primary-dark hover:shadow disabled:opacity-50"
              >
                <Save className="size-4" />
                {saving ? "Saving…" : isEdit ? "Save Changes" : "Save Product"}
              </button>
            </div>
          </div>
        </div>

        {/* Tab strip — mirrors the old system's Product screen instead of one long scroll.
            Charges only applies to taxable products, so it's hidden otherwise. */}
        <div className="mb-5 flex flex-wrap gap-1 border-b border-border">
          {TABS.filter((t) => t.id !== "charges" || form.isTaxable).map((t) => {
            const on = activeTab === t.id;
            const count =
              t.id === "servingUnits"
                ? variants.length
                : t.id === "suppliers"
                  ? suppliers.length
                  : t.id === "printers"
                    ? kitchenStations.length
                    : t.id === "charges"
                      ? charges.length
                      : t.id === "priceLevels"
                        ? prices.length
                        : undefined;
            return (
              <button
                key={t.id}
                type="button"
                onClick={() => setActiveTab(t.id)}
                className={`flex items-center gap-1.5 border-b-2 px-4 py-2.5 text-sm font-semibold transition ${
                  on
                    ? "border-primary text-primary"
                    : "border-transparent text-muted-foreground hover:text-foreground"
                }`}
              >
                <t.icon className="size-4" />
                {t.label}
                {typeof count === "number" && count > 0 && (
                  <span
                    className={`rounded-full px-1.5 py-0.5 text-xs font-semibold ${
                      on ? "bg-primary/10 text-primary" : "bg-muted text-muted-foreground"
                    }`}
                  >
                    {count}
                  </span>
                )}
              </button>
            );
          })}
        </div>

        <div className="grid grid-cols-12 gap-5">
          {activeTab === "general" && (
          <>
          <SectionCard
            className="col-span-12"
            icon={<Package className="size-4" />}
            title="General Details"
          >
            <div className="grid grid-cols-4 gap-3">
              <Field label="Location">
                <select
                  value={form.locationId}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, locationId: e.target.value }))
                  }
                  className={INPUT_CLS}
                >
                  <option value="">Global</option>
                  {lookups?.locations.map((x) => (
                    <option key={x.id} value={x.id}>
                      {x.name}
                    </option>
                  ))}
                </select>
              </Field>

              <Field label="Department">
                <select
                  value={form.departmentId}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, departmentId: e.target.value }))
                  }
                  className={INPUT_CLS}
                >
                  <option value="">— none —</option>
                  {filteredDepartments.map((x) => (
                    <option key={x.id} value={x.id}>
                      {x.name}
                    </option>
                  ))}
                </select>
              </Field>

              <Field label="Category">
                <select
                  value={form.categoryId}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, categoryId: e.target.value }))
                  }
                  className={INPUT_CLS}
                >
                  <option value="">— none —</option>
                  {lookups?.categories.map((x) => (
                    <option key={x.id} value={x.id}>
                      {x.name}
                    </option>
                  ))}
                </select>
              </Field>

              <Field label="Product Type">
                <select
                  value={form.productType}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, productType: e.target.value }))
                  }
                  className={INPUT_CLS}
                >
                  <option value="finished">Finished Item</option>
                  <option value="raw_material">Raw Material</option>
                  <option value="addon">Add-on</option>
                  <option value="pack">Pack Item</option>
                  <option value="bundle">Bundle</option>
                </select>
              </Field>

              <Field label="Product Code" error={errors.sku}>
                <div className="flex items-center gap-2">
                  <input
                    value={form.sku}
                    onChange={(e) =>
                      setForm((f) => ({
                        ...f,
                        sku: e.target.value.toUpperCase(),
                      }))
                    }
                    className={`${INPUT_CLS} font-mono`}
                  />
                  <button
                    type="button"
                    onClick={genCode}
                    className="inline-flex shrink-0 items-center gap-1.5 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-semibold text-slate-700 transition hover:bg-slate-50"
                  >
                    <Sparkles className="size-4" /> Auto-generate
                  </button>
                </div>
              </Field>

              <Field label="Barcode">
                <div className="space-y-2">
                  <input
                    value={form.barcode}
                    onChange={(e) =>
                      setForm((f) => ({ ...f, barcode: e.target.value }))
                    }
                    placeholder="Scan, type, or generate"
                    className={`${INPUT_CLS} font-mono`}
                  />
                  <p className="text-xs text-muted-foreground">
                    Generate creates a unique in-store EAN-13 you can print & stick on the item.
                  </p>
                  <div className="flex items-center gap-2">
                    <button
                      type="button"
                      onClick={() =>
                        setForm((f) => ({ ...f, barcode: generateEan13() }))
                      }
                      className="inline-flex items-center gap-1.5 rounded-lg bg-primary px-3 py-2 text-sm font-semibold text-primary-foreground transition-colors hover:bg-primary-dark"
                    >
                      <RefreshCw className="size-4" /> Generate
                    </button>
                    <button
                      type="button"
                      disabled={!form.barcode.trim()}
                      onClick={() =>
                        printBarcodeLabel(form.barcode.trim(), form.name, form.basePrice)
                      }
                      className="inline-flex items-center gap-1.5 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-semibold text-slate-700 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      <Printer className="size-4" /> Print label
                    </button>
                  </div>
                </div>
              </Field>

              <Field
                label="Product Name"
                error={errors.name}
                className="col-span-2"
              >
                <input
                  value={form.name}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, name: e.target.value }))
                  }
                  className={INPUT_CLS}
                />
              </Field>

              <Field label="Name on Invoice" className="col-span-2">
                <input
                  value={form.nameOnInvoice}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, nameOnInvoice: e.target.value }))
                  }
                  className={INPUT_CLS}
                />
              </Field>

              <Field label="Reference Code 01">
                <input
                  value={form.refCode01}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, refCode01: e.target.value }))
                  }
                  className={INPUT_CLS}
                />
              </Field>

              <Field label="Reference Code 02">
                <input
                  value={form.refCode02}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, refCode02: e.target.value }))
                  }
                  className={INPUT_CLS}
                />
              </Field>

              <Field label="Description" className="col-span-4">
                <textarea
                  value={form.description}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, description: e.target.value }))
                  }
                  className={`${INPUT_CLS} min-h-20`}
                />
              </Field>
            </div>
          </SectionCard>

          <SectionCard
            className="col-span-12"
            icon={<Sparkles className="size-4" />}
            title="Media & Branding"
            description="How this product appears on menus, tiles and receipts."
          >
            <div className="grid grid-cols-2 gap-6">
              <Field label="Product Image">
                <ImagePicker
                  value={form.imageUrl}
                  onChange={(v) => setForm((f) => ({ ...f, imageUrl: v }))}
                  fallbackLabel={form.name || form.sku}
                  accentColor={
                    isHexColor(form.colorHex) ? form.colorHex : undefined
                  }
                />
              </Field>

              <Field label="Color Tag" error={errors.colorHex}>
                <ColorPicker
                  value={form.colorHex}
                  onChange={(v) => setForm((f) => ({ ...f, colorHex: v }))}
                />
              </Field>
            </div>
          </SectionCard>
          </>
          )}

          {activeTab === "pricing" && (
          <>
          <SectionCard
            className="col-span-12"
            icon={<Wallet className="size-4" />}
            title="Pricing & Stock"
            description={
              margin !== null ? (
                <span>
                  Margin at current prices:{" "}
                  <span
                    className={
                      margin >= 0
                        ? "font-semibold text-emerald-600"
                        : "font-semibold text-status-error"
                    }
                  >
                    {margin.toFixed(1)}%
                  </span>
                </span>
              ) : undefined
            }
          >
            <div className="grid grid-cols-4 gap-3">
              <Field label="Unit" error={errors.unitOfMeasureId}>
                <select
                  value={form.unitOfMeasureId}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, unitOfMeasureId: e.target.value }))
                  }
                  className={INPUT_CLS}
                >
                  <option value="">Select...</option>
                  {lookups?.units.map((x) => (
                    <option key={x.id} value={x.id}>
                      {x.code} — {x.name}
                    </option>
                  ))}
                </select>
              </Field>

              <Field label="Cost Price" error={errors.costPrice}>
                <Money
                  value={form.costPrice}
                  onChange={(v) => setForm((f) => ({ ...f, costPrice: v }))}
                />
              </Field>

              <Field label="Selling Price" error={errors.basePrice}>
                <Money
                  value={form.basePrice}
                  onChange={(v) => setForm((f) => ({ ...f, basePrice: v }))}
                />
              </Field>

              <Field label="Reorder Level">
                <Money
                  value={form.reorderLevel}
                  onChange={(v) => setForm((f) => ({ ...f, reorderLevel: v }))}
                />
              </Field>

              <Field label="Reorder Quantity">
                <Money
                  value={form.reorderQuantity}
                  onChange={(v) =>
                    setForm((f) => ({ ...f, reorderQuantity: v }))
                  }
                />
              </Field>

              <Field label="Par Level">
                <Money
                  value={form.parLevel}
                  onChange={(v) => setForm((f) => ({ ...f, parLevel: v }))}
                />
              </Field>

              <Field label="Sort Order">
                <input
                  value={form.sortOrder}
                  onChange={(e) =>
                    setForm((f) => ({
                      ...f,
                      sortOrder: e.target.value.replace(/[^0-9]/g, ""),
                    }))
                  }
                  className={INPUT_CLS}
                />
              </Field>
            </div>
          </SectionCard>

          <SectionCard
            className="col-span-12"
            icon={<Tag className="size-4" />}
            title="Tax & Discount"
          >
            <div className="grid grid-cols-4 gap-3">
              <Field label="Tax Class">
                <select
                  value={form.taxClass}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, taxClass: e.target.value }))
                  }
                  className={INPUT_CLS}
                >
                  <option value="standard">Standard</option>
                  <option value="zero_rated">Zero Rated</option>
                  <option value="exempt">Exempt</option>
                </select>
              </Field>

              <Field label="Discount Type">
                <select
                  value={form.discountType}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, discountType: e.target.value }))
                  }
                  className={INPUT_CLS}
                >
                  <option value="none">None</option>
                  <option value="percentage">Percentage</option>
                  <option value="fixed">Fixed Amount</option>
                </select>
              </Field>

              <Field label="Discount Value">
                <Money
                  value={form.discountValue}
                  onChange={(v) => setForm((f) => ({ ...f, discountValue: v }))}
                />
              </Field>

              <Field label="Max Discount Amount">
                <Money
                  value={form.maxDiscountAmount}
                  onChange={(v) =>
                    setForm((f) => ({ ...f, maxDiscountAmount: v }))
                  }
                />
              </Field>

              <Field label="Max Discount %">
                <Money
                  value={form.maxDiscountPercentage}
                  onChange={(v) =>
                    setForm((f) => ({ ...f, maxDiscountPercentage: v }))
                  }
                />
              </Field>

              <div className="col-span-4 grid grid-cols-4 gap-3 pt-2">
                <Check
                  label="Taxable"
                  checked={form.isTaxable}
                  onChange={(v) => setForm((f) => ({ ...f, isTaxable: v }))}
                />
                <Check
                  label="Tax Inclusive"
                  checked={form.isTaxInclusive}
                  onChange={(v) =>
                    setForm((f) => ({ ...f, isTaxInclusive: v }))
                  }
                />
                <Check
                  label="Allow Discount"
                  checked={form.allowDiscount}
                  onChange={(v) => setForm((f) => ({ ...f, allowDiscount: v }))}
                />
                <Check
                  label="Allow Below Cost"
                  checked={form.allowBelowCost}
                  onChange={(v) =>
                    setForm((f) => ({ ...f, allowBelowCost: v }))
                  }
                />
              </div>
            </div>
          </SectionCard>

          <SectionCard
            className="col-span-12"
            icon={<Sparkles className="size-4" />}
            title="Product Flags"
          >
            <div className="grid grid-cols-4 gap-3">
              <Check
                label="Sold on POS"
                checked={form.isSold}
                onChange={(v) => setForm((f) => ({ ...f, isSold: v }))}
              />
              <Check
                label="Purchased"
                checked={form.isPurchased}
                onChange={(v) => setForm((f) => ({ ...f, isPurchased: v }))}
              />
              <Check
                label="Stock Tracked"
                checked={form.isStocked}
                onChange={(v) => setForm((f) => ({ ...f, isStocked: v }))}
              />
              <Check
                label="Available Online"
                checked={form.isAvailableOnline}
                onChange={(v) =>
                  setForm((f) => ({ ...f, isAvailableOnline: v }))
                }
              />
              <Check
                label="Scale Item"
                checked={form.isScaleItem}
                onChange={(v) => setForm((f) => ({ ...f, isScaleItem: v }))}
              />
              <Check
                label="Open Item"
                checked={form.isOpenItem}
                onChange={(v) => setForm((f) => ({ ...f, isOpenItem: v }))}
              />
              <Check
                label="Locked Item"
                checked={form.isLocked}
                onChange={(v) => setForm((f) => ({ ...f, isLocked: v }))}
              />
              <Check
                label="Track Expiry"
                checked={form.tracksExpiry}
                onChange={(v) => setForm((f) => ({ ...f, tracksExpiry: v }))}
              />
              <Check
                label="Active"
                checked={form.isActive}
                onChange={(v) => setForm((f) => ({ ...f, isActive: v }))}
              />
            </div>
          </SectionCard>
          </>
          )}

          {activeTab === "servingUnits" && (
          <SectionCard
            className="col-span-12"
            icon={<Package className="size-4" />}
            title="Serving Units / Variants"
            count={variants.length}
            action={
              <button
                onClick={addVariant}
                className="flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-2 text-sm font-medium transition hover:border-primary/40 hover:bg-primary/5"
              >
                <Plus className="size-4" />
                Add Variant
              </button>
            }
          >
            <div className="space-y-2">
              {variants.length > 0 && (
                <div className="hidden grid-cols-12 gap-2 px-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground sm:grid">
                  <span className="col-span-2">Serving Unit</span>
                  <span className="col-span-1">Code</span>
                  <span className="col-span-2">Name</span>
                  <span className="col-span-1">Qty</span>
                  <span className="col-span-2">Cost Price</span>
                  <span className="col-span-2">Sell Price</span>
                  <span className="col-span-1">Deduct</span>
                  <span className="col-span-1" />
                </div>
              )}

              {variants.map((row, idx) => (
                <div
                  key={idx}
                  className="grid grid-cols-12 gap-2 rounded-lg border border-border bg-muted/20 p-3 transition hover:border-primary/30"
                >
                  <select
                    value={row.servingUnitId}
                    onChange={(e) =>
                      setVariants((vs) =>
                        vs.map((x, i) =>
                          i === idx
                            ? { ...x, servingUnitId: e.target.value }
                            : x,
                        ),
                      )
                    }
                    className={`${INPUT_CLS} col-span-2`}
                  >
                    <option value="">Serving unit</option>
                    {(lookups?.servingUnits ?? []).map((x) => (
                      <option key={x.id} value={x.id}>
                        {x.name}
                      </option>
                    ))}
                  </select>

                  <input
                    value={row.code}
                    onChange={(e) =>
                      setVariants((vs) =>
                        vs.map((x, i) =>
                          i === idx
                            ? { ...x, code: e.target.value.toUpperCase() }
                            : x,
                        ),
                      )
                    }
                    placeholder="CODE"
                    className={`${INPUT_CLS} col-span-1 font-mono`}
                  />
                  <input
                    value={row.name}
                    onChange={(e) =>
                      setVariants((vs) =>
                        vs.map((x, i) =>
                          i === idx ? { ...x, name: e.target.value } : x,
                        ),
                      )
                    }
                    placeholder="Name"
                    className={`${INPUT_CLS} col-span-2`}
                  />
                  <Money
                    value={row.servingQty}
                    onChange={(v) =>
                      setVariants((vs) =>
                        vs.map((x, i) =>
                          i === idx ? { ...x, servingQty: v } : x,
                        ),
                      )
                    }
                    className="col-span-1"
                  />
                  <Money
                    value={row.costPrice}
                    onChange={(v) =>
                      setVariants((vs) =>
                        vs.map((x, i) =>
                          i === idx ? { ...x, costPrice: v } : x,
                        ),
                      )
                    }
                    className="col-span-2"
                  />
                  <Money
                    value={row.price}
                    onChange={(v) =>
                      setVariants((vs) =>
                        vs.map((x, i) => (i === idx ? { ...x, price: v } : x)),
                      )
                    }
                    className="col-span-2"
                  />

                  <label className="col-span-1 flex items-center gap-1 text-xs">
                    <input
                      type="checkbox"
                      checked={row.deductStockOnRecipe}
                      onChange={(e) =>
                        setVariants((vs) =>
                          vs.map((x, i) =>
                            i === idx
                              ? { ...x, deductStockOnRecipe: e.target.checked }
                              : x,
                          ),
                        )
                      }
                    />
                    Deduct
                  </label>

                  <button
                    onClick={() =>
                      setVariants((vs) => vs.filter((_, i) => i !== idx))
                    }
                    className="col-span-1 rounded-lg p-2 text-status-error transition hover:bg-status-error/10"
                  >
                    <X className="size-4" />
                  </button>
                </div>
              ))}

              {variants.length === 0 && (
                <EmptyState text="No variants yet. Product will be sold using the base price." />
              )}
            </div>
          </SectionCard>
          )}

          {activeTab === "suppliers" && (
          <SectionCard
            className="col-span-12"
            icon={<Truck className="size-4" />}
            title="Suppliers"
            count={suppliers.length}
            action={
              <button
                onClick={addSupplier}
                className="flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-2 text-sm font-medium transition hover:border-primary/40 hover:bg-primary/5"
              >
                <Plus className="size-4" />
                Add Supplier
              </button>
            }
          >
            <div className="space-y-2">
              {suppliers.length > 0 && (
                <div className="hidden grid-cols-12 gap-2 px-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground sm:grid">
                  <span className="col-span-7">Supplier</span>
                  <span className="col-span-2">Preferred</span>
                  <span className="col-span-2">Active</span>
                  <span className="col-span-1" />
                </div>
              )}

              {suppliers.map((row, idx) => (
                <div
                  key={idx}
                  className="grid grid-cols-12 gap-2 rounded-lg border border-border bg-muted/20 p-3 transition hover:border-primary/30"
                >
                  <select
                    value={row.supplierId}
                    onChange={(e) =>
                      setSuppliers((vs) =>
                        vs.map((x, i) =>
                          i === idx ? { ...x, supplierId: e.target.value } : x,
                        ),
                      )
                    }
                    className={`${INPUT_CLS} col-span-7`}
                  >
                    {lookups?.suppliers.map((x) => (
                      <option key={x.id} value={x.id}>
                        {x.code} — {x.name}
                      </option>
                    ))}
                  </select>

                  <label className="col-span-2 flex items-center gap-2 text-sm">
                    <input
                      type="checkbox"
                      checked={row.isPreferred}
                      onChange={(e) => {
                        const checked = e.target.checked;
                        setSuppliers((vs) =>
                          vs.map((x, i) => ({
                            ...x,
                            isPreferred: i === idx ? checked : false,
                          })),
                        );
                      }}
                    />
                    Preferred
                  </label>

                  <label className="col-span-2 flex items-center gap-2 text-sm">
                    <input
                      type="checkbox"
                      checked={row.isActive}
                      onChange={(e) =>
                        setSuppliers((vs) =>
                          vs.map((x, i) =>
                            i === idx
                              ? { ...x, isActive: e.target.checked }
                              : x,
                          ),
                        )
                      }
                    />
                    Active
                  </label>

                  <button
                    onClick={() =>
                      setSuppliers((vs) => vs.filter((_, i) => i !== idx))
                    }
                    className="col-span-1 rounded-lg p-2 text-status-error transition hover:bg-status-error/10"
                  >
                    <X className="size-4" />
                  </button>
                </div>
              ))}

              {suppliers.length === 0 && (
                <EmptyState text="No suppliers linked to this product yet." />
              )}
            </div>
          </SectionCard>
          )}

          {activeTab === "printers" && (
          <SectionCard
            className="col-span-12"
            icon={<ChefHat className="size-4" />}
            title="Kitchen Stations"
            count={kitchenStations.length}
            description="Which stations this product's KOTs print/route to. Add more than one for a combo that fires to both Grill and Dessert, for example."
            action={
              <button
                onClick={addKitchenStation}
                className="flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-2 text-sm font-medium transition hover:border-primary/40 hover:bg-primary/5"
              >
                <Plus className="size-4" />
                Add Station
              </button>
            }
          >
            <div className="space-y-2">
              {kitchenStations.length > 0 && (
                <div className="hidden grid-cols-12 gap-2 px-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground sm:grid">
                  <span className="col-span-7">Kitchen Station</span>
                  <span className="col-span-4">Location</span>
                  <span className="col-span-1" />
                </div>
              )}

              {kitchenStations.map((row, idx) => (
                <div
                  key={idx}
                  className="grid grid-cols-12 gap-2 rounded-lg border border-border bg-muted/20 p-3 transition hover:border-primary/30"
                >
                  <select
                    value={row.kitchenStationId}
                    onChange={(e) =>
                      setKitchenStations((vs) =>
                        vs.map((x, i) =>
                          i === idx
                            ? { ...x, kitchenStationId: e.target.value }
                            : x,
                        ),
                      )
                    }
                    className={`${INPUT_CLS} col-span-7`}
                  >
                    <option value="">Select station</option>
                    {filteredKitchenStations.map((x) => (
                      <option key={x.id} value={x.id}>
                        {x.code} — {x.name}
                        {x.printerName ? ` · ${x.printerName}` : ""}
                      </option>
                    ))}
                  </select>

                  <select
                    value={row.locationId}
                    onChange={(e) =>
                      setKitchenStations((vs) =>
                        vs.map((x, i) =>
                          i === idx ? { ...x, locationId: e.target.value } : x,
                        ),
                      )
                    }
                    className={`${INPUT_CLS} col-span-4`}
                  >
                    <option value="">All locations</option>
                    {lookups?.locations.map((x) => (
                      <option key={x.id} value={x.id}>
                        {x.name}
                      </option>
                    ))}
                  </select>

                  <button
                    onClick={() =>
                      setKitchenStations((vs) => vs.filter((_, i) => i !== idx))
                    }
                    className="col-span-1 rounded-lg p-2 text-status-error transition hover:bg-status-error/10"
                  >
                    <X className="size-4" />
                  </button>
                </div>
              ))}

              {kitchenStations.length === 0 && (
                <EmptyState text="No station linked yet — this product's KOTs won't route anywhere until you add one." />
              )}
            </div>
          </SectionCard>
          )}

          {activeTab === "charges" && (
          <SectionCard
            className="col-span-12"
            icon={<Percent className="size-4" />}
            title="Charges"
            count={charges.length}
            description="Tax and other per-product charges. A product can carry several (e.g. VAT + a luxury tax)."
            action={
              <button
                onClick={addCharge}
                className="flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-2 text-sm font-medium transition hover:border-primary/40 hover:bg-primary/5"
              >
                <Plus className="size-4" />
                Add Charge
              </button>
            }
          >
            <div className="space-y-2">
              {charges.length > 0 && (
                <div className="hidden grid-cols-12 gap-2 px-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground sm:grid">
                  <span className="col-span-11">Charge</span>
                  <span className="col-span-1" />
                </div>
              )}

              {charges.map((row, idx) => (
                <div
                  key={idx}
                  className="grid grid-cols-12 gap-2 rounded-lg border border-border bg-muted/20 p-3 transition hover:border-primary/30"
                >
                  <select
                    value={row.chargeId}
                    onChange={(e) =>
                      setCharges((vs) =>
                        vs.map((x, i) =>
                          i === idx ? { ...x, chargeId: e.target.value } : x,
                        ),
                      )
                    }
                    className={`${INPUT_CLS} col-span-11`}
                  >
                    <option value="">Select charge</option>
                    {lookups?.charges.map((x) => (
                      <option key={x.id} value={x.id}>
                        {x.chargeTypeName}: {x.description}
                        {x.percentage != null
                          ? ` (${x.percentage}%)`
                          : x.amount != null
                            ? ` (LKR ${x.amount})`
                            : ""}
                      </option>
                    ))}
                  </select>

                  <button
                    onClick={() =>
                      setCharges((vs) => vs.filter((_, i) => i !== idx))
                    }
                    className="col-span-1 rounded-lg p-2 text-status-error transition hover:bg-status-error/10"
                  >
                    <X className="size-4" />
                  </button>
                </div>
              ))}

              {charges.length === 0 && (
                <EmptyState text="No charges linked to this product. Add VAT or another tax to charge it on this line." />
              )}
            </div>
          </SectionCard>
          )}

          {activeTab === "priceLevels" && (
          <SectionCard
            className="col-span-12"
            icon={<Wallet className="size-4" />}
            title="Price Levels"
            count={prices.length}
            action={
              <button
                onClick={addPrice}
                className="flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-2 text-sm font-medium transition hover:border-primary/40 hover:bg-primary/5"
              >
                <Plus className="size-4" />
                Add Price
              </button>
            }
          >
            <div className="space-y-2">
              {prices.length > 0 && (
                <div className="hidden grid-cols-12 gap-2 px-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground sm:grid">
                  <span className="col-span-2">Location</span>
                  <span className="col-span-2">Applies To</span>
                  <span className="col-span-3">Price Level</span>
                  <span className="col-span-2">Cost Price</span>
                  <span className="col-span-2">Sell Price</span>
                  <span className="col-span-1" />
                </div>
              )}

              {prices.map((row, idx) => (
                <div
                  key={idx}
                  className="grid grid-cols-12 gap-2 rounded-lg border border-border bg-muted/20 p-3 transition hover:border-primary/30"
                >
                  <select
                    value={row.locationId}
                    onChange={(e) =>
                      setPrices((vs) =>
                        vs.map((x, i) =>
                          i === idx ? { ...x, locationId: e.target.value } : x,
                        ),
                      )
                    }
                    className={`${INPUT_CLS} col-span-2`}
                  >
                    <option value="">Global</option>
                    {lookups?.locations.map((x) => (
                      <option key={x.id} value={x.id}>
                        {x.name}
                      </option>
                    ))}
                  </select>

                  <select
                    value={row.productVariantId}
                    onChange={(e) =>
                      setPrices((vs) =>
                        vs.map((x, i) =>
                          i === idx
                            ? { ...x, productVariantId: e.target.value }
                            : x,
                        ),
                      )
                    }
                    className={`${INPUT_CLS} col-span-2`}
                  >
                    <option value="">Base product</option>
                    {variants.map((x, i) => (
                      <option key={i} value={x.id ?? x.code}>
                        {x.name || x.code || `Variant ${i + 1}`}
                      </option>
                    ))}
                  </select>

                  <select
                    value={row.priceLevelId}
                    onChange={(e) =>
                      setPrices((vs) =>
                        vs.map((x, i) =>
                          i === idx
                            ? { ...x, priceLevelId: e.target.value }
                            : x,
                        ),
                      )
                    }
                    className={`${INPUT_CLS} col-span-3`}
                  >
                    {filteredPriceLevels.map((x) => (
                      <option key={x.id} value={x.id}>
                        {x.code} — {x.name}
                      </option>
                    ))}
                  </select>

                  <Money
                    value={row.costPrice}
                    onChange={(v) =>
                      setPrices((vs) =>
                        vs.map((x, i) =>
                          i === idx ? { ...x, costPrice: v } : x,
                        ),
                      )
                    }
                    className="col-span-2"
                  />
                  <Money
                    value={row.price}
                    onChange={(v) =>
                      setPrices((vs) =>
                        vs.map((x, i) => (i === idx ? { ...x, price: v } : x)),
                      )
                    }
                    className="col-span-2"
                  />

                  <button
                    onClick={() =>
                      setPrices((vs) => vs.filter((_, i) => i !== idx))
                    }
                    className="col-span-1 rounded-lg p-2 text-status-error transition hover:bg-status-error/10"
                  >
                    <X className="size-4" />
                  </button>
                </div>
              ))}

              {prices.length === 0 && (
                <EmptyState text="No price-level overrides. The base price will be used." />
              )}
            </div>
          </SectionCard>
          )}
        </div>
      </div>

      {/* Bottom action bar mirrors the sticky header for long forms */}
      <div className="sticky bottom-0 z-30 border-t border-border bg-background/95 px-6 py-3 backdrop-blur">
        <div className="flex justify-end gap-2">
          <button
            onClick={() => router.push("/menu")}
            className="rounded-lg border border-border bg-card px-5 py-2.5 text-sm font-semibold transition hover:bg-muted"
          >
            Cancel
          </button>

          <button
            disabled={saving}
            onClick={submit}
            className="flex items-center gap-1.5 rounded-lg bg-primary px-5 py-2.5 text-sm font-bold text-primary-foreground shadow-sm transition hover:bg-primary-dark disabled:opacity-50"
          >
            <Save className="size-4" />
            {saving ? "Saving…" : isEdit ? "Save Changes" : "Create Product"}
          </button>
        </div>
      </div>
    </>
  );
}

function SectionCard({
  icon,
  title,
  description,
  count,
  action,
  children,
  className = "",
}: {
  icon: React.ReactNode;
  title: string;
  description?: React.ReactNode;
  count?: number;
  action?: React.ReactNode;
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <section className={`card overflow-hidden p-0 ${className}`}>
      <div className="flex items-center justify-between gap-3 border-b border-border/70 bg-muted/20 px-5 py-3.5">
        <div className="flex items-center gap-2.5">
          <span className="flex size-8 items-center justify-center rounded-lg bg-primary/10 text-primary">
            {icon}
          </span>
          <div>
            <div className="flex items-center gap-2">
              <h3 className="font-heading text-base font-bold leading-none">
                {title}
              </h3>
              {typeof count === "number" && (
                <span className="rounded-full bg-muted px-2 py-0.5 text-xs font-semibold text-muted-foreground">
                  {count}
                </span>
              )}
            </div>
            {description && (
              <p className="mt-1 text-xs text-muted-foreground">
                {description}
              </p>
            )}
          </div>
        </div>
        {action}
      </div>
      <div className="p-5">{children}</div>
    </section>
  );
}

function EmptyState({ text }: { text: string }) {
  return (
    <p className="rounded-lg border border-dashed border-border bg-muted/10 p-5 text-center text-sm text-muted-foreground">
      {text}
    </p>
  );
}

function Field({
  label,
  children,
  error,
  className = "",
}: {
  label: string;
  children: React.ReactNode;
  error?: string;
  className?: string;
}) {
  return (
    <div className={className}>
      <label className="mb-1 block text-sm font-semibold text-slate-700">
        {label}
      </label>
      {children}
      {error && <p className="mt-1 text-xs text-status-error">{error}</p>}
    </div>
  );
}

function Money({
  value,
  onChange,
  className = "",
}: {
  value: string;
  onChange: (value: string) => void;
  className?: string;
}) {
  return (
    <input
      value={value}
      onChange={(e) => onChange(e.target.value.replace(/[^0-9.]/g, ""))}
      inputMode="decimal"
      className={`${INPUT_CLS} text-right tabular-nums ${className}`}
    />
  );
}

function Check({
  label,
  checked,
  onChange,
}: {
  label: string;
  checked: boolean;
  onChange: (value: boolean) => void;
}) {
  return (
    <label
      className={`flex cursor-pointer items-center justify-between gap-2 rounded-lg border px-3 py-2 text-sm font-medium transition ${
        checked
          ? "border-primary/40 bg-primary/5 text-foreground"
          : "border-border bg-surface text-muted-foreground"
      }`}
    >
      {label}
      <span
        onClick={(e) => {
          e.preventDefault();
          onChange(!checked);
        }}
        className={`relative inline-flex h-5 w-9 shrink-0 items-center rounded-full transition ${
          checked ? "bg-primary" : "bg-muted"
        }`}
      >
        <span
          className={`inline-block size-3.5 transform rounded-full bg-white shadow transition ${
            checked ? "translate-x-[18px]" : "translate-x-1"
          }`}
        />
      </span>
    </label>
  );
}

function ColorPicker({
  value,
  onChange,
}: {
  value: string;
  onChange: (value: string) => void;
}) {
  const normalized = value.trim().toUpperCase();
  const previewColor = isHexColor(normalized) ? normalized : "#94A3B8";

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap gap-2">
        {PRESET_COLORS.map((c) => (
          <button
            key={c}
            type="button"
            aria-label={c}
            onClick={() => onChange(c)}
            className={`size-7 rounded-full transition ${
              normalized === c
                ? "ring-2 ring-offset-2 ring-primary scale-110"
                : "ring-1 ring-black/5 hover:scale-110"
            }`}
            style={{ backgroundColor: c }}
          />
        ))}
      </div>

      <div className="flex items-center gap-2">
        <input
          type="color"
          value={previewColor}
          onChange={(e) => onChange(e.target.value.toUpperCase())}
          className="h-10 w-10 cursor-pointer rounded-lg border border-border bg-transparent p-0.5"
          aria-label="Custom color"
        />
        <input
          value={value}
          onChange={(e) => onChange(e.target.value)}
          placeholder="#16A34A"
          maxLength={7}
          className={`${INPUT_CLS} flex-1 font-mono uppercase`}
        />
        {value && (
          <button
            type="button"
            onClick={() => onChange("")}
            className="rounded-lg p-2 text-muted-foreground transition hover:bg-muted hover:text-foreground"
            aria-label="Clear color"
          >
            <X className="size-4" />
          </button>
        )}
      </div>
    </div>
  );
}

function ImagePicker({
  value,
  onChange,
  fallbackLabel,
  accentColor,
}: {
  value: string;
  onChange: (value: string) => void;
  fallbackLabel: string;
  accentColor?: string;
}) {
  const [dragOver, setDragOver] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState("");
  const initials = fallbackLabel.trim().slice(0, 2).toUpperCase() || "?";

  async function handleFile(file?: File | null) {
    if (!file) return;

    if (!file.type.startsWith("image/")) {
      setError("Please choose an image file.");
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      setError("Image must be 5MB or smaller.");
      return;
    }

    setError("");
    setUploading(true);
    try {
      const formData = new FormData();
      formData.append("file", file);
      const result = await apiClient<{ url: string }>(
        "/api/v1/products/images",
        { method: "POST", body: formData },
      );
      onChange(result.url);
    } catch (e) {
      setError(extractError(e, "Failed to upload image."));
    } finally {
      setUploading(false);
    }
  }

  return (
    <div className="flex gap-4">
      <div
        onDragOver={(e) => {
          e.preventDefault();
          setDragOver(true);
        }}
        onDragLeave={() => setDragOver(false)}
        onDrop={(e) => {
          e.preventDefault();
          setDragOver(false);
          void handleFile(e.dataTransfer.files?.[0]);
        }}
        className={`relative flex size-24 shrink-0 items-center justify-center overflow-hidden rounded-xl border-2 border-dashed transition ${
          dragOver ? "border-primary bg-primary/5" : "border-border bg-muted/30"
        }`}
      >
        {value ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img src={value} alt="Product" className="size-full object-cover" />
        ) : (
          <span
            className="flex size-full items-center justify-center text-lg font-bold text-white"
            style={{ backgroundColor: accentColor ?? "#94A3B8" }}
          >
            {initials}
          </span>
        )}

        {uploading && (
          <div className="absolute inset-0 flex items-center justify-center bg-black/40 text-xs font-semibold text-white">
            Uploading...
          </div>
        )}
      </div>

      <div className="flex-1 space-y-2">
        <input
          value={value}
          onChange={(e) => onChange(e.target.value)}
          placeholder="https://... image URL"
          className={INPUT_CLS}
        />

        <div className="flex flex-wrap items-center gap-2">
          <label className="flex cursor-pointer items-center gap-1.5 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-semibold transition hover:border-primary/40 hover:bg-primary/5">
            <ImagePlus className="size-3.5" />
            Upload image
            <input
              type="file"
              accept="image/*"
              className="hidden"
              disabled={uploading}
              onChange={(e) => void handleFile(e.target.files?.[0])}
            />
          </label>

          {value && (
            <button
              type="button"
              onClick={() => onChange("")}
              className="text-xs font-semibold text-status-error hover:underline"
            >
              Remove image
            </button>
          )}
        </div>

        <p
          className={`text-xs ${error ? "text-status-error" : "text-muted-foreground"}`}
        >
          {error ||
            "PNG, JPG, WEBP, or GIF up to 5MB — drag & drop, upload, or paste a URL."}
        </p>
      </div>
    </div>
  );
}
