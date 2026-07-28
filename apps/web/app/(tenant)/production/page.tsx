'use client';

import { useEffect, useMemo, useState } from 'react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient, fetchAllProducts, lkr } from '@/lib/api-client';
import { Icon } from '@/components/ui/Icon';
import { SearchableSelect } from '@/components/ui/SearchableSelect';
import { Plus, X } from 'lucide-react';

type Location = { id: string; code: string; name: string; city: string; currency: string };

type Product = {
  id: string;
  sku: string;
  name: string;
  categoryId: string | null;
  basePrice: number;
  costPrice: number;
  isActive: boolean;
  isSold: boolean;
  isStocked: boolean;
  unitOfMeasureId: string | null;
};

type Unit = { id: string; code: string; name: string; symbol: string | null; dimension: string; factorToBase: number };

type RecipeSummary = {
  id: string;
  productId: string;
  productName: string;
  sku: string;
  yieldQuantity: number;
  lineCount: number;
  isActive: boolean;
  locationId: string | null;
  locationCode: string | null;
};

type RecipeLine = {
  ingredientProductId: string;
  sku: string;
  productName: string;
  quantity: number;
  unitId: string | null;
  unitSymbol: string | null;
  costPrice: number;
  lineCost: number;
};

type RecipeDetail = {
  id: string;
  productId: string;
  locationId: string | null;
  yieldQuantity: number;
  notes: string | null;
  isActive: boolean;
  totalCost: number;
  lines: RecipeLine[];
};

type Consumption = {
  ingredientProductId: string;
  sku: string;
  productName: string;
  quantityConsumed: number;
  unitCost: number;
  lineTotal: number;
};

type ProdOrder = {
  id: string;
  productionNumber: string;
  locationId: string;
  productId: string;
  productName: string;
  quantity: number;
  status: string;
  isCustom: boolean;
  documentNo: string | null;
  totalInputCost: number;
  unitCost: number;
  completedAt: string | null;
  consumptions: Consumption[];
};

/** Editable line used by the recipe builder. */
type Line = { ingredientProductId: string; quantity: string; unitId: string };

const STATUS_PILL: Record<string, string> = {
  draft: 'pill-idle',
  pending: 'pill-pending',
  in_progress: 'pill-progress',
  completed: 'pill-paid',
  cancelled: 'pill-void',
};

export default function ProductionPage() {
  const [recipes, setRecipes] = useState<RecipeSummary[]>([]);
  const [orders, setOrders] = useState<ProdOrder[]>([]);
  const [locations, setLocations] = useState<Location[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [units, setUnits] = useState<Unit[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [toast, setToast] = useState<string | null>(null);

  // recipe modal state
  const [recipeOpen, setRecipeOpen] = useState(false);
  const [recipeProductId, setRecipeProductId] = useState('');
  const [recipeProductLocked, setRecipeProductLocked] = useState(false);
  const [recipeLocationId, setRecipeLocationId] = useState('');
  const [yieldQty, setYieldQty] = useState('1');
  const [notes, setNotes] = useState('');
  const [lines, setLines] = useState<Line[]>([{ ingredientProductId: '', quantity: '', unitId: '' }]);
  const [recipeErrors, setRecipeErrors] = useState<Record<string, string>>({});
  const [recipeSubmitting, setRecipeSubmitting] = useState(false);
  const [recipeLoading, setRecipeLoading] = useState(false);

  // produce modal state
  const [produceOpen, setProduceOpen] = useState(false);
  const [produceLoc, setProduceLoc] = useState('');
  const [produceProductId, setProduceProductId] = useState('');
  const [produceQty, setProduceQty] = useState('1');
  const [produceNotes, setProduceNotes] = useState('');
  const [produceErrors, setProduceErrors] = useState<Record<string, string>>({});
  const [producing, setProducing] = useState(false);

  // detail modal state
  const [detail, setDetail] = useState<ProdOrder | null>(null);
  const [busyOrderId, setBusyOrderId] = useState<string | null>(null);

  function flash(msg: string) {
    setToast(msg);
    window.setTimeout(() => setToast(null), 3500);
  }

  async function load() {
    try {
      const [r, o, l, p, u] = await Promise.all([
        apiClient<RecipeSummary[]>('/api/v1/recipes'),
        apiClient<ProdOrder[]>('/api/v1/production'),
        apiClient<Location[]>('/api/v1/locations'),
        fetchAllProducts(),
        apiClient<Unit[]>('/api/v1/units-of-measure'),
      ]);
      setRecipes(r);
      setOrders(o);
      setLocations(l);
      setProducts(p);
      setUnits(u);
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  const productById = useMemo(() => {
    const m = new Map<string, Product>();
    for (const p of products) m.set(p.id, p);
    return m;
  }, [products]);

  /** Products that already have a recipe (eligible finished goods to produce). */
  const producibleProducts = useMemo(() => {
    const ids = new Set(recipes.map(r => r.productId));
    const matched = products.filter(p => ids.has(p.id));
    return matched.length > 0 ? matched : products;
  }, [recipes, products]);

  // ── Recipe modal helpers ────────────────────────────────────────────────
  function openNewRecipe() {
    setRecipeProductId('');
    setRecipeProductLocked(false);
    setRecipeLocationId('');
    setYieldQty('1');
    setNotes('');
    setLines([{ ingredientProductId: '', quantity: '', unitId: '' }]);
    setRecipeErrors({});
    setRecipeOpen(true);
  }

  async function openEditRecipe(productId: string, locationId: string | null) {
    setRecipeProductId(productId);
    setRecipeProductLocked(true);
    setRecipeLocationId(locationId ?? '');
    setYieldQty('1');
    setNotes('');
    setLines([{ ingredientProductId: '', quantity: '', unitId: '' }]);
    setRecipeErrors({});
    setRecipeOpen(true);
    setRecipeLoading(true);
    try {
      const qs = locationId ? `?locationId=${locationId}` : '';
      const d = await apiClient<RecipeDetail>(`/api/v1/recipes/${productId}${qs}`);
      setRecipeLocationId(d.locationId ?? '');
      setYieldQty(String(d.yieldQuantity));
      setNotes(d.notes ?? '');
      setLines(
        d.lines.length
          ? d.lines.map(l => ({
              ingredientProductId: l.ingredientProductId,
              quantity: String(l.quantity),
              unitId: l.unitId ?? '',
            }))
          : [{ ingredientProductId: '', quantity: '', unitId: '' }],
      );
    } catch (e) {
      flash(extractError(e, 'Could not load recipe.'));
      setRecipeOpen(false);
    } finally {
      setRecipeLoading(false);
    }
  }

  function addLine() {
    setLines(ls => [...ls, { ingredientProductId: '', quantity: '', unitId: '' }]);
  }

  const unitById = useMemo(() => {
    const m = new Map<string, Unit>();
    for (const u of units) m.set(u.id, u);
    return m;
  }, [units]);

  /** Units a recipe line can use for an ingredient: its own stock unit + any
   * unit in the same mass/volume dimension. */
  function unitsForIngredient(ingredientProductId: string): Unit[] {
    const stockUnitId = productById.get(ingredientProductId)?.unitOfMeasureId ?? null;
    const stockUnit = stockUnitId ? unitById.get(stockUnitId) : undefined;
    if (!stockUnit) return units;
    if (stockUnit.dimension === 'count') return [stockUnit];
    return units.filter(u => u.dimension === stockUnit.dimension);
  }

  /** Same conversion the backend applies: a line quantity in its chosen unit,
   * expressed in the ingredient's stock unit (cost price is per stock unit). */
  function conversionFactor(ingredientProductId: string, lineUnitId: string): number {
    const stockUnitId = productById.get(ingredientProductId)?.unitOfMeasureId ?? null;
    const stockUnit = stockUnitId ? unitById.get(stockUnitId) : undefined;
    const lineUnit = (lineUnitId && unitById.get(lineUnitId)) || stockUnit;
    if (!lineUnit || !stockUnit || !stockUnit.factorToBase) return 1;
    return lineUnit.factorToBase / stockUnit.factorToBase;
  }

  /** Estimated cost for one recipe line — a preview of what the backend will
   * compute and store (it may use a location's average stock cost instead of
   * the product's master cost price if one exists). */
  function lineCostPreview(line: Line): number {
    const ing = productById.get(line.ingredientProductId);
    if (!ing) return 0;
    const qty = Number(line.quantity) || 0;
    const unitId = line.unitId || ing.unitOfMeasureId || '';
    return qty * conversionFactor(line.ingredientProductId, unitId) * ing.costPrice;
  }

  const totalCostPreview = useMemo(
    () => lines.reduce((sum, l) => sum + (l.ingredientProductId && l.quantity.trim() ? lineCostPreview(l) : 0), 0),
    [lines, products, units],
  );

  function removeLine(idx: number) {
    setLines(ls => (ls.length === 1 ? ls : ls.filter((_, i) => i !== idx)));
  }

  function updateLine(idx: number, patch: Partial<Line>) {
    setLines(ls => ls.map((l, i) => (i === idx ? { ...l, ...patch } : l)));
  }

  function validateRecipe(): boolean {
    const errs: Record<string, string> = {};
    if (!recipeProductId) errs.product = 'Finished product is required.';

    const y = Number(yieldQty);
    if (yieldQty.trim() === '' || Number.isNaN(y) || y <= 0)
      errs.yield = 'Yield must be greater than zero.';

    const cleaned = lines.filter(l => l.ingredientProductId || l.quantity.trim());
    if (cleaned.length === 0) {
      errs.lines = 'Add at least one ingredient line.';
    } else {
      const seen = new Set<string>();
      for (const l of cleaned) {
        if (!l.ingredientProductId) {
          errs.lines = 'Every line needs an ingredient.';
          break;
        }
        if (l.ingredientProductId === recipeProductId) {
          errs.lines = 'An ingredient cannot be the finished product.';
          break;
        }
        if (seen.has(l.ingredientProductId)) {
          errs.lines = 'Duplicate ingredient lines are not allowed.';
          break;
        }
        seen.add(l.ingredientProductId);
        const q = Number(l.quantity);
        if (l.quantity.trim() === '' || Number.isNaN(q) || q <= 0) {
          errs.lines = 'Quantity must be greater than zero.';
          break;
        }
      }
    }
    setRecipeErrors(errs);
    return Object.keys(errs).length === 0;
  }

  async function submitRecipe() {
    if (!validateRecipe()) return;
    setRecipeSubmitting(true);
    try {
      const payload = {
        productId: recipeProductId,
        locationId: recipeLocationId || null,
        yieldQuantity: Number(yieldQty),
        notes: notes.trim() ? notes.trim() : null,
        lines: lines
          .filter(l => l.ingredientProductId && l.quantity.trim())
          .map(l => ({
            ingredientProductId: l.ingredientProductId,
            quantity: Number(l.quantity),
            unitId: l.unitId || null,
          })),
      };
      await apiClient<RecipeDetail>('/api/v1/recipes', {
        method: 'PUT',
        body: JSON.stringify(payload),
      });
      setRecipeOpen(false);
      flash('Recipe saved.');
      await load();
    } catch (e) {
      flash(extractError(e, 'Could not save recipe.'));
    } finally {
      setRecipeSubmitting(false);
    }
  }

  // ── Produce modal helpers ───────────────────────────────────────────────
  function openProduce() {
    const main = locations.find(l => l.code === 'MAIN') ?? locations[0];
    setProduceLoc(main?.id ?? '');
    setProduceProductId('');
    setProduceQty('1');
    setProduceNotes('');
    setProduceErrors({});
    setProduceOpen(true);
  }

  function validateProduce(): boolean {
    const errs: Record<string, string> = {};
    if (!produceLoc) errs.location = 'Location is required.';
    if (!produceProductId) errs.product = 'Product is required.';
    const q = Number(produceQty);
    if (produceQty.trim() === '' || Number.isNaN(q) || q <= 0)
      errs.qty = 'Quantity must be greater than zero.';
    setProduceErrors(errs);
    return Object.keys(errs).length === 0;
  }

  async function submitProduce(post = true) {
    if (!validateProduce()) return;
    setProducing(true);
    try {
      const payload = {
        locationId: produceLoc,
        productId: produceProductId,
        quantity: Number(produceQty),
        notes: produceNotes.trim() ? produceNotes.trim() : null,
        post,
      };
      const order = await apiClient<ProdOrder>('/api/v1/production', {
        method: 'POST',
        body: JSON.stringify(payload),
      });
      setProduceOpen(false);
      flash(post
        ? `Produced ${order.productionNumber}: ${order.quantity} × ${order.productName}`
        : `Draft ${order.productionNumber} saved — post it when ready`);
      await load();
    } catch (e) {
      flash(extractError(e, 'Could not run production order.'));
    } finally {
      setProducing(false);
    }
  }

  async function postOrder(id: string) {
    setBusyOrderId(id);
    try { await apiClient(`/api/v1/production/${id}/post`, { method: 'POST' }); flash('Production posted.'); await load(); }
    catch (e) { flash(extractError(e, 'Could not post.')); }
    finally { setBusyOrderId(null); }
  }

  async function voidOrder(id: string) {
    setBusyOrderId(id);
    try {
      await apiClient(`/api/v1/production/${id}/void`, { method: 'POST', body: JSON.stringify({ reason: 'Voided from production screen' }) });
      flash('Production voided — stock reversed.'); await load();
    } catch (e) { flash(extractError(e, 'Could not void.')); }
    finally { setBusyOrderId(null); }
  }

  const fmtDate = (s: string | null) =>
    s
      ? new Date(s).toLocaleDateString('en-LK', {
          year: 'numeric',
          month: 'short',
          day: 'numeric',
        })
      : '—';

  return (
    <>
      <Topbar title="Production" subtitle="Recipes & central-kitchen production" />

      <div className="p-6">
        {/* Stats strip */}
        <div className="mb-5 grid grid-cols-2 gap-3 sm:max-w-md">
          <div className="card flex items-center gap-3 p-4">
            <span className="flex size-10 items-center justify-center rounded-lg bg-primary/10 text-primary">
              <Icon name="receipt_long" className="text-xl" />
            </span>
            <div>
              <p className="text-2xl font-bold leading-none tabular-nums">{recipes.length}</p>
              <p className="text-xs text-muted-foreground">Recipes</p>
            </div>
          </div>
          <div className="card flex items-center gap-3 p-4">
            <span className="flex size-10 items-center justify-center rounded-lg bg-secondary/10 text-secondary">
              <Icon name="factory" className="text-xl" />
            </span>
            <div>
              <p className="text-2xl font-bold leading-none tabular-nums">{orders.length}</p>
              <p className="text-xs text-muted-foreground">Production orders</p>
            </div>
          </div>
        </div>

        {/* ── Recipes ──────────────────────────────────────────────────── */}
        <div className="mb-3 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Recipes</h2>
            <p className="text-sm text-muted-foreground">
              Bill of materials for finished goods
            </p>
          </div>
          <button
            onClick={openNewRecipe}
            disabled={loading || !!error}
            className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark disabled:opacity-50"
          >
            <Plus className="size-4" /> New recipe
          </button>
        </div>

        <div className="card mb-8 overflow-hidden">
          {loading ? (
            <div className="space-y-2 p-4">
              {Array.from({ length: 5 }).map((_, i) => (
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
                  <th className="px-4 py-2.5 font-medium">Location</th>
                  <th className="px-4 py-2.5 text-right font-medium">Yield</th>
                  <th className="px-4 py-2.5 text-right font-medium">Ingredients</th>
                  <th className="px-4 py-2.5 font-medium">Status</th>
                  <th className="px-4 py-2.5 text-right font-medium">Action</th>
                </tr>
              </thead>
              <tbody>
                {recipes.map((r, i) => (
                  <tr key={r.id} className={i % 2 ? 'bg-muted/20' : ''}>
                    <td className="px-4 py-2.5 font-medium">{r.productName}</td>
                    <td className="px-4 py-2.5 font-mono text-xs text-muted-foreground">{r.sku}</td>
                    <td className="px-4 py-2.5 text-xs text-muted-foreground">{r.locationCode ?? 'All locations'}</td>
                    <td className="px-4 py-2.5 text-right tabular-nums">{r.yieldQuantity}</td>
                    <td className="px-4 py-2.5 text-right tabular-nums">{r.lineCount}</td>
                    <td className="px-4 py-2.5">
                      {r.isActive ? (
                        <span className="pill pill-paid">Active</span>
                      ) : (
                        <span className="pill pill-idle">Inactive</span>
                      )}
                    </td>
                    <td className="px-4 py-2.5 text-right">
                      <button
                        onClick={() => void openEditRecipe(r.productId, r.locationId)}
                        className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted"
                      >
                        <Icon name="edit" className="text-sm" /> Edit
                      </button>
                    </td>
                  </tr>
                ))}
                {recipes.length === 0 && (
                  <tr>
                    <td colSpan={7} className="px-4 py-10 text-center text-muted-foreground">
                      No recipes yet. Create one to define a bill of materials.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        {/* ── Production orders ────────────────────────────────────────── */}
        <div className="mb-3 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Production orders</h2>
            <p className="text-sm text-muted-foreground">
              Consume ingredients at average cost to yield finished stock
            </p>
          </div>
          <button
            onClick={openProduce}
            disabled={loading || !!error}
            className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark disabled:opacity-50"
          >
            <Plus className="size-4" /> Produce
          </button>
        </div>

        <div className="card overflow-hidden">
          {loading ? (
            <div className="space-y-2 p-4">
              {Array.from({ length: 6 }).map((_, i) => (
                <div key={i} className="h-9 animate-pulse rounded bg-muted" />
              ))}
            </div>
          ) : error ? (
            <div className="p-6 text-sm text-status-error">{error}</div>
          ) : (
            <table className="w-full text-sm">
              <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th className="px-4 py-2.5 font-medium">Production #</th>
                  <th className="px-4 py-2.5 font-medium">Product</th>
                  <th className="px-4 py-2.5 text-right font-medium">Qty</th>
                  <th className="px-4 py-2.5 text-right font-medium">Unit cost</th>
                  <th className="px-4 py-2.5 text-right font-medium">Total input cost</th>
                  <th className="px-4 py-2.5 font-medium">Completed</th>
                  <th className="px-4 py-2.5 font-medium">Status</th>
                  <th className="px-4 py-2.5 text-right font-medium">Actions</th>
                </tr>
              </thead>
              <tbody>
                {orders.map((o, i) => (
                  <tr
                    key={o.id}
                    onClick={() => setDetail(o)}
                    className={`cursor-pointer hover:bg-muted/40 ${i % 2 ? 'bg-muted/20' : ''}`}
                  >
                    <td className="px-4 py-2.5 font-mono text-xs">{o.productionNumber}</td>
                    <td className="px-4 py-2.5 font-medium">{o.productName}</td>
                    <td className="px-4 py-2.5 text-right tabular-nums">{o.quantity}</td>
                    <td className="px-4 py-2.5 text-right tabular-nums">{lkr(o.unitCost)}</td>
                    <td className="px-4 py-2.5 text-right font-semibold tabular-nums">
                      {lkr(o.totalInputCost)}
                    </td>
                    <td className="px-4 py-2.5 text-muted-foreground">{fmtDate(o.completedAt)}</td>
                    <td className="px-4 py-2.5">
                      <span className={`pill ${STATUS_PILL[o.status] ?? 'pill-idle'}`}>
                        {o.status}{o.isCustom ? ' · custom' : ''}
                      </span>
                    </td>
                    <td className="px-4 py-2.5 text-right" onClick={e => e.stopPropagation()}>
                      {o.status === 'draft' && (
                        <button disabled={busyOrderId === o.id} onClick={() => postOrder(o.id)}
                          className="rounded-lg bg-primary px-3 py-1.5 text-xs font-medium text-primary-foreground hover:bg-primary-dark disabled:opacity-50">
                          Post
                        </button>
                      )}
                      {o.status === 'posted' && (
                        <button disabled={busyOrderId === o.id} onClick={() => voidOrder(o.id)}
                          className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-status-error hover:bg-muted disabled:opacity-50">
                          Void
                        </button>
                      )}
                      {o.status !== 'draft' && o.status !== 'posted' && <span className="text-muted-foreground">—</span>}
                    </td>
                  </tr>
                ))}
                {orders.length === 0 && (
                  <tr>
                    <td colSpan={8} className="px-4 py-10 text-center text-muted-foreground">
                      No production orders yet. Run one to turn ingredients into finished stock.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        <p className="mt-3 text-xs text-muted-foreground">
          Showing {orders.length} production orders · costs in LKR at average cost
        </p>
      </div>

      {/* ── Recipe modal ───────────────────────────────────────────────── */}
      {recipeOpen && (
        <div
          className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 p-4"
          onClick={() => !recipeSubmitting && setRecipeOpen(false)}
        >
          <div
            className="w-full max-w-lg rounded-xl bg-card p-6 shadow-2xl"
            onClick={e => e.stopPropagation()}
          >
            <div className="mb-4 flex items-start justify-between">
              <div>
                <h3 className="font-heading text-lg font-bold">
                  {recipeProductLocked ? 'Edit recipe' : 'New recipe'}
                </h3>
                <p className="text-sm text-muted-foreground">
                  Define the bill of materials for a finished good.
                </p>
              </div>
              <button
                onClick={() => !recipeSubmitting && setRecipeOpen(false)}
                className="rounded-lg p-1.5 text-muted-foreground hover:bg-muted"
              >
                <X className="size-5" />
              </button>
            </div>

            {recipeLoading ? (
              <div className="space-y-2">
                {Array.from({ length: 5 }).map((_, i) => (
                  <div key={i} className="h-10 animate-pulse rounded bg-muted" />
                ))}
              </div>
            ) : (
              <>
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="mb-1 block text-sm font-semibold text-slate-700">
                        Finished product
                      </label>
                      <SearchableSelect
                        value={recipeProductId}
                        onChange={setRecipeProductId}
                        disabled={recipeProductLocked}
                        placeholder="Select…"
                        options={products.map(p => ({ id: p.id, label: `${p.name} (${p.sku})` }))}
                      />
                      {recipeErrors.product && (
                        <p className="mt-1 text-xs text-status-error">{recipeErrors.product}</p>
                      )}
                    </div>
                    <div>
                      <label className="mb-1 block text-sm font-semibold text-slate-700">
                        Yield quantity
                      </label>
                      <input
                        value={yieldQty}
                        onChange={e => setYieldQty(e.target.value.replace(/[^0-9.]/g, ''))}
                        inputMode="decimal"
                        placeholder="1"
                        className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 tabular-nums focus:border-primary focus:ring-2 focus:ring-primary/20"
                      />
                      {recipeErrors.yield && (
                        <p className="mt-1 text-xs text-status-error">{recipeErrors.yield}</p>
                      )}
                    </div>
                    <div>
                      <label className="mb-1 block text-sm font-semibold text-slate-700">
                        Location
                      </label>
                      <select
                        value={recipeLocationId}
                        onChange={e => setRecipeLocationId(e.target.value)}
                        disabled={recipeProductLocked}
                        className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20 disabled:opacity-60"
                      >
                        <option value="">All locations</option>
                        {locations.map(l => (
                          <option key={l.id} value={l.id}>
                            {l.code} — {l.name}
                          </option>
                        ))}
                      </select>
                    </div>
                  </div>

                  <div>
                    <label className="mb-1 block text-sm font-semibold text-slate-700">Notes</label>
                    <input
                      value={notes}
                      onChange={e => setNotes(e.target.value)}
                      placeholder="Optional"
                      className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                    />
                  </div>

                  {/* Ingredient line builder */}
                  <div>
                    <div className="mb-2 flex items-center justify-between">
                      <label className="block text-sm font-semibold text-slate-700">
                        Ingredients
                      </label>
                      <button
                        onClick={addLine}
                        className="inline-flex items-center gap-1 text-xs font-medium text-primary hover:text-primary-dark"
                      >
                        <Plus className="size-3.5" /> Add line
                      </button>
                    </div>
                    <div className="space-y-2">
                      {lines.map((line, idx) => (
                        <div key={idx} className="rounded-lg border border-border/60 p-2">
                          <div className="flex items-center gap-2">
                            <SearchableSelect
                              className="flex-1"
                              value={line.ingredientProductId}
                              onChange={id =>
                                updateLine(idx, {
                                  ingredientProductId: id,
                                  unitId: productById.get(id)?.unitOfMeasureId ?? '',
                                })
                              }
                              placeholder="Select ingredient…"
                              options={products
                                .filter(p => p.id !== recipeProductId)
                                .map(p => ({ id: p.id, label: `${p.name} (${p.sku})` }))}
                            />
                            <input
                              value={line.quantity}
                              onChange={e =>
                                updateLine(idx, {
                                  quantity: e.target.value.replace(/[^0-9.]/g, ''),
                                })
                              }
                              inputMode="decimal"
                              placeholder="Qty"
                              className="w-20 rounded-lg border border-border bg-surface px-3 py-2 text-sm tabular-nums focus:border-primary focus:ring-2 focus:ring-primary/20"
                            />
                            <select
                              value={line.unitId || (productById.get(line.ingredientProductId)?.unitOfMeasureId ?? '')}
                              onChange={e => updateLine(idx, { unitId: e.target.value })}
                              disabled={!line.ingredientProductId}
                              title="Unit"
                              className="w-24 rounded-lg border border-border bg-surface px-2 py-2 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20 disabled:opacity-50"
                            >
                              {unitsForIngredient(line.ingredientProductId).map(u => (
                                <option key={u.id} value={u.id}>{u.symbol ?? u.code}</option>
                              ))}
                            </select>
                            <button
                              onClick={() => removeLine(idx)}
                              disabled={lines.length === 1}
                              className="rounded-lg p-2 text-muted-foreground hover:bg-muted disabled:opacity-30"
                              title="Remove line"
                            >
                              <X className="size-4" />
                            </button>
                          </div>
                          {line.ingredientProductId && (
                            <p className="mt-1 text-right text-xs text-muted-foreground">
                              Cost price: {lkr(productById.get(line.ingredientProductId)?.costPrice ?? 0)} · Line cost: {lkr(lineCostPreview(line))}
                            </p>
                          )}
                        </div>
                      ))}
                    </div>
                    {recipeErrors.lines && (
                      <p className="mt-1 text-xs text-status-error">{recipeErrors.lines}</p>
                    )}
                  </div>

                  <div className="flex items-center justify-between rounded-lg bg-muted/40 px-4 py-3">
                    <div>
                      <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                        Total cost price
                      </p>
                      <p className="text-lg font-bold tabular-nums">{lkr(totalCostPreview)}</p>
                    </div>
                    <div className="text-right">
                      <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                        Cost per output unit
                      </p>
                      <p className="text-lg font-bold tabular-nums">
                        {lkr((Number(yieldQty) || 1) > 0 ? totalCostPreview / (Number(yieldQty) || 1) : 0)}
                      </p>
                    </div>
                  </div>
                </div>

                <div className="mt-6 flex gap-2">
                  <button
                    onClick={() => setRecipeOpen(false)}
                    disabled={recipeSubmitting}
                    className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50"
                  >
                    Cancel
                  </button>
                  <button
                    onClick={submitRecipe}
                    disabled={recipeSubmitting}
                    className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50"
                  >
                    {recipeSubmitting ? 'Saving…' : 'Save recipe'}
                  </button>
                </div>
              </>
            )}
          </div>
        </div>
      )}

      {/* ── Produce modal ──────────────────────────────────────────────── */}
      {produceOpen && (
        <div
          className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 p-4"
          onClick={() => !producing && setProduceOpen(false)}
        >
          <div
            className="w-full max-w-lg rounded-xl bg-card p-6 shadow-2xl"
            onClick={e => e.stopPropagation()}
          >
            <div className="mb-4 flex items-start justify-between">
              <div>
                <h3 className="font-heading text-lg font-bold">Produce</h3>
                <p className="text-sm text-muted-foreground">
                  Run a production order against a recipe.
                </p>
              </div>
              <button
                onClick={() => !producing && setProduceOpen(false)}
                className="rounded-lg p-1.5 text-muted-foreground hover:bg-muted"
              >
                <X className="size-5" />
              </button>
            </div>

            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="mb-1 block text-sm font-semibold text-slate-700">
                    Location
                  </label>
                  <select
                    value={produceLoc}
                    onChange={e => setProduceLoc(e.target.value)}
                    className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                  >
                    <option value="">Select…</option>
                    {locations.map(l => (
                      <option key={l.id} value={l.id}>
                        {l.code} — {l.name}
                      </option>
                    ))}
                  </select>
                  {produceErrors.location && (
                    <p className="mt-1 text-xs text-status-error">{produceErrors.location}</p>
                  )}
                </div>
                <div>
                  <label className="mb-1 block text-sm font-semibold text-slate-700">
                    Quantity
                  </label>
                  <input
                    value={produceQty}
                    onChange={e => setProduceQty(e.target.value.replace(/[^0-9.]/g, ''))}
                    inputMode="decimal"
                    placeholder="1"
                    className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 tabular-nums focus:border-primary focus:ring-2 focus:ring-primary/20"
                  />
                  {produceErrors.qty && (
                    <p className="mt-1 text-xs text-status-error">{produceErrors.qty}</p>
                  )}
                </div>
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold text-slate-700">Product</label>
                <SearchableSelect
                  value={produceProductId}
                  onChange={setProduceProductId}
                  placeholder="Select…"
                  options={producibleProducts.map(p => ({ id: p.id, label: `${p.name} (${p.sku})` }))}
                />
                {produceErrors.product && (
                  <p className="mt-1 text-xs text-status-error">{produceErrors.product}</p>
                )}
              </div>

              <div>
                <label className="mb-1 block text-sm font-semibold text-slate-700">Notes</label>
                <input
                  value={produceNotes}
                  onChange={e => setProduceNotes(e.target.value)}
                  placeholder="Optional"
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>
            </div>

            <div className="mt-6 flex gap-2">
              <button
                onClick={() => setProduceOpen(false)}
                disabled={producing}
                className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                onClick={() => submitProduce(false)}
                disabled={producing}
                className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50"
              >
                Save draft
              </button>
              <button
                onClick={() => submitProduce(true)}
                disabled={producing}
                className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50"
              >
                {producing ? 'Producing…' : 'Produce'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ── Production detail modal (read-only) ────────────────────────── */}
      {detail && (
        <div
          className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 p-4"
          onClick={() => setDetail(null)}
        >
          <div
            className="w-full max-w-2xl rounded-xl bg-card p-6 shadow-2xl"
            onClick={e => e.stopPropagation()}
          >
            <div className="mb-4 flex items-start justify-between">
              <div>
                <h3 className="font-heading text-lg font-bold">
                  Production {detail.productionNumber}
                </h3>
                <p className="text-sm text-muted-foreground">
                  {detail.quantity} × {detail.productName}
                  {productById.get(detail.productId)
                    ? ` (${productById.get(detail.productId)!.sku})`
                    : ''}
                </p>
              </div>
              <button
                onClick={() => setDetail(null)}
                className="rounded-lg p-1.5 text-muted-foreground hover:bg-muted"
              >
                <X className="size-5" />
              </button>
            </div>

            <div className="mb-4 flex items-center gap-2">
              <span className={`pill ${STATUS_PILL[detail.status] ?? 'pill-idle'}`}>
                {detail.status}
              </span>
              <span className="text-sm text-muted-foreground">
                Completed {fmtDate(detail.completedAt)}
              </span>
            </div>

            <div className="card overflow-hidden">
              <table className="w-full text-sm">
                <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                  <tr>
                    <th className="px-4 py-2.5 font-medium">Ingredient</th>
                    <th className="px-4 py-2.5 font-medium">SKU</th>
                    <th className="px-4 py-2.5 text-right font-medium">Qty consumed</th>
                    <th className="px-4 py-2.5 text-right font-medium">Unit cost</th>
                    <th className="px-4 py-2.5 text-right font-medium">Line total</th>
                  </tr>
                </thead>
                <tbody>
                  {detail.consumptions.map((c, i) => (
                    <tr key={`${c.ingredientProductId}-${i}`} className={i % 2 ? 'bg-muted/20' : ''}>
                      <td className="px-4 py-2.5 font-medium">{c.productName}</td>
                      <td className="px-4 py-2.5 font-mono text-xs text-muted-foreground">
                        {c.sku}
                      </td>
                      <td className="px-4 py-2.5 text-right tabular-nums">{c.quantityConsumed}</td>
                      <td className="px-4 py-2.5 text-right tabular-nums">{lkr(c.unitCost)}</td>
                      <td className="px-4 py-2.5 text-right font-semibold tabular-nums">
                        {lkr(c.lineTotal)}
                      </td>
                    </tr>
                  ))}
                  {detail.consumptions.length === 0 && (
                    <tr>
                      <td colSpan={5} className="px-4 py-8 text-center text-muted-foreground">
                        No consumption lines recorded.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>

            <div className="mt-4 flex items-center justify-end gap-6 text-sm">
              <div className="text-right">
                <p className="text-xs text-muted-foreground">Unit cost</p>
                <p className="font-semibold tabular-nums">{lkr(detail.unitCost)}</p>
              </div>
              <div className="text-right">
                <p className="text-xs text-muted-foreground">Total input cost</p>
                <p className="font-bold tabular-nums">{lkr(detail.totalInputCost)}</p>
              </div>
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

/** Pull a friendly {error} string out of an apiClient error like `API 400: {"error":"..."}`. */
function extractError(e: unknown, fallback: string): string {
  const msg = (e as Error)?.message ?? '';
  const jsonStart = msg.indexOf('{');
  if (jsonStart !== -1) {
    try {
      const parsed = JSON.parse(msg.slice(jsonStart));
      if (typeof parsed?.error === 'string') return parsed.error;
      if (typeof parsed?.message === 'string') return parsed.message;
    } catch {
      /* fall through */
    }
  }
  return msg || fallback;
}
