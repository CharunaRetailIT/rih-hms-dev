'use client';

import { useEffect, useMemo, useState } from 'react';
import { apiClient, money } from '@/lib/api-client';
import { PackageSearch, Truck, ClipboardList, Save, Sparkles, AlertTriangle, CheckCircle2 } from 'lucide-react';

// ── inventory replenishment (#replenishment) ─────────────────────────────────
// Worksheet: items at/below their effective reorder level for a location, with the
// suggested top-up split into a transfer (from the location's hub) + a PO (preferred
// supplier) for the rest. Generate creates DRAFTS only. Levels tab: per-location
// reorder/par overrides on top of the product default.

type Loc = {
  id: string; code: string; name: string; city?: string; addressLine1?: string;
  currency?: string; vatExempt?: boolean; locationType?: string;
  canSell?: boolean; canProduce?: boolean; canStock?: boolean; isActive?: boolean;
  operatingHours?: string | null; countryCode?: string; province?: string; district?: string; postalCode?: string;
  replenishSourceLocationId?: string | null;
};
type Supplier = { id: string; name: string };
type WsRow = { productId: string; sku: string; name: string; unitCost: number; onHand: number; reorder: number; par: number; need: number; transferQty: number; poQty: number; hubOnHand: number; supplierId: string | null; supplierName: string | null; hasSupplier: boolean };
type Worksheet = { location: { id: string; name: string }; hub: { id: string; name: string } | null; rows: WsRow[]; summary: { items: number; transferLines: number; poLines: number; poValue: number; missingSupplier: number; hasHub: boolean } };
type LevelRow = { id: string; sku: string; name: string; onHand: number; productReorder: number | null; productPar: number | null; overrideReorder: number | null; overridePar: number | null; overrideSupplierId: string | null; effectiveReorder: number; effectivePar: number };
type GenResult = { transfers: string[]; purchaseOrders: { supplier: string; number: string }[]; skipped: { sku: string; name: string; reason: string }[] };

const num = (v: string | number | null | undefined) => { if (v === '' || v == null) return null; const n = Number(v); return Number.isNaN(n) ? null : n; };

export default function ReplenishmentPage() {
  const [locations, setLocations] = useState<Loc[]>([]);
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);
  const [locationId, setLocationId] = useState('');
  const [tab, setTab] = useState<'worksheet' | 'levels'>('worksheet');
  const [ws, setWs] = useState<Worksheet | null>(null);
  const [levels, setLevels] = useState<LevelRow[]>([]);
  const [draft, setDraft] = useState<Record<string, { reorder: string; par: string; supplier: string }>>({});
  const [busy, setBusy] = useState(false);
  const [result, setResult] = useState<GenResult | null>(null);
  const [toast, setToast] = useState<string | null>(null);
  const flash = (m: string) => { setToast(m); window.setTimeout(() => setToast(null), 3500); };

  const loc = useMemo(() => locations.find(l => l.id === locationId), [locations, locationId]);
  const stockable = useMemo(() => locations.filter(l => l.canStock !== false), [locations]);
  const hubOptions = useMemo(() => stockable.filter(l => l.id !== locationId), [stockable, locationId]);
  const supName = (id: string | null) => (id ? suppliers.find(s => s.id === id)?.name ?? '—' : '—');

  useEffect(() => {
    apiClient<Loc[]>('/api/v1/locations?all=true').then(ls => {
      const s = ls.filter(o => o.canStock !== false);
      setLocations(ls);
      setLocationId(p => p || (s.find(o => o.locationType === 'outlet')?.id ?? s[0]?.id ?? ''));
    }).catch(() => {});
    apiClient<Supplier[]>('/api/v1/suppliers').then(setSuppliers).catch(() => {});
  }, []);

  async function loadWorksheet() {
    if (!locationId) return;
    setResult(null);
    try { setWs(await apiClient<Worksheet>(`/api/v1/replenishment?locationId=${locationId}`)); }
    catch (e) { flash(e instanceof Error ? e.message : 'Could not load worksheet.'); }
  }
  async function loadLevels() {
    if (!locationId) return;
    try {
      const r = await apiClient<{ products: LevelRow[] }>(`/api/v1/replenishment/levels?locationId=${locationId}`);
      setLevels(r.products);
      const d: Record<string, { reorder: string; par: string; supplier: string }> = {};
      r.products.forEach(p => { d[p.id] = { reorder: p.overrideReorder?.toString() ?? '', par: p.overridePar?.toString() ?? '', supplier: p.overrideSupplierId ?? '' }; });
      setDraft(d);
    } catch (e) { flash(e instanceof Error ? e.message : 'Could not load levels.'); }
  }
  useEffect(() => { if (tab === 'worksheet') void loadWorksheet(); else void loadLevels(); /* eslint-disable-next-line */ }, [locationId, tab]);

  async function setHub(hubId: string) {
    if (!loc) return;
    try {
      await apiClient('/api/v1/locations', { method: 'PUT', body: JSON.stringify({
        id: loc.id, code: loc.code, name: loc.name, city: loc.city, addressLine1: loc.addressLine1,
        locationType: loc.locationType, canSell: loc.canSell, canProduce: loc.canProduce, canStock: loc.canStock,
        isActive: loc.isActive, currency: loc.currency, vatExempt: loc.vatExempt, operatingHours: loc.operatingHours,
        countryCode: loc.countryCode, province: loc.province, district: loc.district, postalCode: loc.postalCode,
        replenishSourceLocationId: hubId || null,
      }) });
      setLocations(ls => ls.map(l => l.id === loc.id ? { ...l, replenishSourceLocationId: hubId || null } : l));
      flash(hubId ? 'Replenishment hub set.' : 'Hub cleared — this location buys externally.');
      void loadWorksheet();
    } catch (e) { flash(e instanceof Error ? e.message : 'Could not set hub.'); }
  }

  async function generate(transfers: boolean, pos: boolean) {
    setBusy(true); setResult(null);
    try {
      const r = await apiClient<GenResult>('/api/v1/replenishment/generate', { method: 'POST', body: JSON.stringify({ locationId, transfers, pos }) });
      setResult(r);
      flash(`${r.transfers.length} transfer(s), ${r.purchaseOrders.length} PO(s) drafted${r.skipped.length ? `, ${r.skipped.length} skipped` : ''}.`);
      void loadWorksheet();
    } catch (e) { flash(e instanceof Error ? e.message : 'Generate failed.'); }
    finally { setBusy(false); }
  }

  async function saveLevels() {
    // only rows whose override differs from what was loaded
    const rows = levels.filter(p => {
      const d = draft[p.id]; if (!d) return false;
      return num(d.reorder) !== p.overrideReorder || num(d.par) !== p.overridePar || (d.supplier || null) !== p.overrideSupplierId;
    }).map(p => ({ productId: p.id, reorderLevel: num(draft[p.id].reorder), parLevel: num(draft[p.id].par), preferredSupplierId: draft[p.id].supplier || null }));
    if (rows.length === 0) { flash('No level changes to save.'); return; }
    setBusy(true);
    try { await apiClient('/api/v1/replenishment/levels', { method: 'PUT', body: JSON.stringify({ locationId, rows }) }); flash(`Saved ${rows.length} level override(s).`); void loadLevels(); }
    catch (e) { flash(e instanceof Error ? e.message : 'Save failed.'); }
    finally { setBusy(false); }
  }

  const ccy = loc?.currency || 'LKR';

  return (
    <div className="mx-auto max-w-6xl p-4 sm:p-6">
      <header className="mb-4">
        <h1 className="text-xl font-semibold text-fg flex items-center gap-2"><PackageSearch className="size-5 text-primary" /> Replenishment</h1>
        <p className="mt-1 text-sm text-muted">Items at or below reorder level for a location. Top up from your hub by transfer, buy the rest from the preferred supplier — drafted for your review.</p>
      </header>

      {/* location + hub + tabs */}
      <div className="mb-4 flex flex-wrap items-end gap-3">
        <label className="text-sm text-fg">Location
          <select value={locationId} onChange={e => setLocationId(e.target.value)} className="ml-2 rounded-lg border border-border bg-surface px-2.5 py-1.5 text-sm">
            {stockable.map(l => <option key={l.id} value={l.id}>{l.name}</option>)}
          </select>
        </label>
        <label className="text-sm text-fg">Replenished from
          <select value={loc?.replenishSourceLocationId ?? ''} onChange={e => setHub(e.target.value)} className="ml-2 rounded-lg border border-border bg-surface px-2.5 py-1.5 text-sm">
            <option value="">— External supplier only —</option>
            {hubOptions.map(l => <option key={l.id} value={l.id}>{l.name}</option>)}
          </select>
        </label>
        <div className="ml-auto flex gap-2">
          {(['worksheet', 'levels'] as const).map(t => (
            <button key={t} onClick={() => setTab(t)} className={`rounded-lg px-3 py-1.5 text-sm font-medium border ${tab === t ? 'bg-primary text-white border-primary' : 'bg-surface text-fg border-border hover:bg-bg'}`}>
              {t === 'worksheet' ? 'Worksheet' : 'Set levels'}
            </button>
          ))}
        </div>
      </div>

      {tab === 'worksheet' && ws && (
        <>
          {/* summary */}
          <div className="mb-4 grid grid-cols-2 gap-3 sm:grid-cols-4">
            {[
              { label: 'Below par', value: ws.summary.items },
              { label: 'From hub', value: ws.summary.transferLines },
              { label: 'To purchase', value: ws.summary.poLines },
              { label: `PO value (${ccy})`, value: money(ws.summary.poValue) },
            ].map(c => (
              <div key={c.label} className="rounded-xl border border-border bg-surface px-4 py-3">
                <p className="text-xs uppercase tracking-wide text-muted">{c.label}</p>
                <p className="mt-1 text-lg font-semibold text-fg">{c.value}</p>
              </div>
            ))}
          </div>

          {ws.summary.missingSupplier > 0 && (
            <div className="mb-3 flex items-center gap-2 rounded-lg border border-amber-300 bg-amber-50 px-3 py-2 text-sm text-amber-800">
              <AlertTriangle className="size-4" /> {ws.summary.missingSupplier} item(s) have no preferred supplier — set one (Set levels, or on the product) to include them in a PO.
            </div>
          )}

          {ws.rows.length === 0 ? (
            <div className="rounded-xl border border-border bg-surface px-4 py-10 text-center text-sm text-muted">
              Nothing below par at {ws.location.name}. {ws.summary.hasHub ? '' : 'Tip: set a "Replenished from" hub to enable transfer suggestions.'}
            </div>
          ) : (
            <>
              <div className="mb-3 flex flex-wrap gap-2">
                <button onClick={() => generate(true, false)} disabled={busy || !ws.summary.hasHub || ws.summary.transferLines === 0}
                  className="inline-flex items-center gap-2 rounded-lg border border-border bg-surface px-3 py-2 text-sm font-medium text-fg hover:bg-bg disabled:opacity-40">
                  <Truck className="size-4" /> Draft transfers
                </button>
                <button onClick={() => generate(false, true)} disabled={busy || ws.summary.poLines === 0}
                  className="inline-flex items-center gap-2 rounded-lg border border-border bg-surface px-3 py-2 text-sm font-medium text-fg hover:bg-bg disabled:opacity-40">
                  <ClipboardList className="size-4" /> Draft POs
                </button>
                <button onClick={() => generate(true, true)} disabled={busy || ws.rows.length === 0}
                  className="inline-flex items-center gap-2 rounded-lg bg-primary px-3 py-2 text-sm font-semibold text-white hover:opacity-90 disabled:opacity-50">
                  <Sparkles className="size-4" /> Draft everything
                </button>
              </div>

              <div className="overflow-x-auto rounded-xl border border-border">
                <table className="w-full text-sm">
                  <thead className="bg-bg text-left text-muted">
                    <tr>
                      <th className="px-3 py-2 font-medium">Product Code</th>
                      <th className="px-3 py-2 font-medium">Name</th>
                      <th className="px-3 py-2 text-right font-medium">On hand</th>
                      <th className="px-3 py-2 text-right font-medium">Reorder</th>
                      <th className="px-3 py-2 text-right font-medium">Par</th>
                      <th className="px-3 py-2 text-right font-medium">Need</th>
                      <th className="px-3 py-2 text-right font-medium">{ws.hub ? `From ${ws.hub.name}` : 'From hub'}</th>
                      <th className="px-3 py-2 text-right font-medium">Buy</th>
                      <th className="px-3 py-2 font-medium">Supplier</th>
                    </tr>
                  </thead>
                  <tbody>
                    {ws.rows.map(r => (
                      <tr key={r.productId} className="border-t border-border">
                        <td className="px-3 py-1.5 text-fg">{r.sku}</td>
                        <td className="px-3 py-1.5 text-fg">{r.name}</td>
                        <td className="px-3 py-1.5 text-right text-amber-600">{r.onHand}</td>
                        <td className="px-3 py-1.5 text-right text-muted">{r.reorder}</td>
                        <td className="px-3 py-1.5 text-right text-muted">{r.par}</td>
                        <td className="px-3 py-1.5 text-right font-medium text-fg">{r.need}</td>
                        <td className="px-3 py-1.5 text-right text-emerald-700">{r.transferQty > 0 ? r.transferQty : '—'}</td>
                        <td className="px-3 py-1.5 text-right text-fg">{r.poQty > 0 ? r.poQty : '—'}</td>
                        <td className="px-3 py-1.5">{r.poQty > 0 ? (r.hasSupplier ? r.supplierName : <span className="text-amber-600">none set</span>) : <span className="text-muted/50">—</span>}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </>
          )}

          {result && (
            <div className="mt-4 space-y-2 rounded-xl border border-border bg-surface p-4 text-sm">
              <div className="flex items-center gap-2 font-medium text-fg"><CheckCircle2 className="size-5 text-emerald-600" /> Drafts created</div>
              {result.transfers.length > 0 && <p>Transfers: <b>{result.transfers.join(', ')}</b> — review under Transfers.</p>}
              {result.purchaseOrders.length > 0 && <p>Purchase orders: {result.purchaseOrders.map(p => `${p.number} (${p.supplier})`).join(', ')} — review under Purchasing.</p>}
              {result.transfers.length === 0 && result.purchaseOrders.length === 0 && <p className="text-muted">Nothing drafted.</p>}
              {result.skipped.length > 0 && (
                <p className="text-amber-700">Skipped {result.skipped.length}: {result.skipped.map(s => `${s.sku} (${s.reason})`).join('; ')}</p>
              )}
            </div>
          )}
        </>
      )}

      {tab === 'levels' && (
        <>
          <div className="mb-3 flex items-center justify-between">
            <p className="text-sm text-muted">Blank override = inherit the product default. Set a reorder (min) and par (order-up-to) per location.</p>
            <button onClick={saveLevels} disabled={busy} className="inline-flex items-center gap-2 rounded-lg bg-primary px-3 py-2 text-sm font-semibold text-white hover:opacity-90 disabled:opacity-50">
              <Save className="size-4" /> Save levels
            </button>
          </div>
          <div className="overflow-x-auto rounded-xl border border-border">
            <table className="w-full text-sm">
              <thead className="bg-bg text-left text-muted">
                <tr>
                  <th className="px-3 py-2 font-medium">Product</th>
                  <th className="px-3 py-2 text-right font-medium">On hand</th>
                  <th className="px-3 py-2 text-right font-medium">Default</th>
                  <th className="px-3 py-2 text-right font-medium">Reorder (min)</th>
                  <th className="px-3 py-2 text-right font-medium">Par (max)</th>
                  <th className="px-3 py-2 font-medium">Preferred supplier</th>
                </tr>
              </thead>
              <tbody>
                {levels.map(p => {
                  const d = draft[p.id] ?? { reorder: '', par: '', supplier: '' };
                  return (
                    <tr key={p.id} className="border-t border-border">
                      <td className="px-3 py-1.5"><span className="text-fg">{p.name}</span> <span className="text-xs text-muted">{p.sku}</span></td>
                      <td className="px-3 py-1.5 text-right text-muted">{p.onHand}</td>
                      <td className="px-3 py-1.5 text-right text-xs text-muted">{p.productReorder ?? '—'} / {p.productPar ?? '—'}</td>
                      <td className="px-3 py-1.5 text-right">
                        <input value={d.reorder} onChange={e => setDraft(s => ({ ...s, [p.id]: { ...d, reorder: e.target.value } }))}
                          placeholder={p.productReorder?.toString() ?? '—'} inputMode="decimal" className="w-20 rounded border border-border bg-surface px-2 py-1 text-right" />
                      </td>
                      <td className="px-3 py-1.5 text-right">
                        <input value={d.par} onChange={e => setDraft(s => ({ ...s, [p.id]: { ...d, par: e.target.value } }))}
                          placeholder={p.productPar?.toString() ?? '—'} inputMode="decimal" className="w-20 rounded border border-border bg-surface px-2 py-1 text-right" />
                      </td>
                      <td className="px-3 py-1.5">
                        <select value={d.supplier} onChange={e => setDraft(s => ({ ...s, [p.id]: { ...d, supplier: e.target.value } }))} className="rounded border border-border bg-surface px-2 py-1 text-sm">
                          <option value="">— default —</option>
                          {suppliers.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                        </select>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </>
      )}

      {toast && <div className="fixed bottom-5 left-1/2 -translate-x-1/2 rounded-lg bg-fg px-4 py-2 text-sm text-bg shadow-lg">{toast}</div>}
    </div>
  );
}
