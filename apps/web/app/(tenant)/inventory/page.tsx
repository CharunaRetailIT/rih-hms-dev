'use client';

import { useEffect, useMemo, useState } from 'react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient, money } from '@/lib/api-client';
import { Search } from 'lucide-react';
import { HeaderStat, Num } from '@/components/ui/HeaderStat';
import { usePagination, Pagination } from '@/components/ui/Pagination';

type StockRow = {
  productId: string;
  sku: string;
  name: string;
  colorHex: string | null;
  reorderLevel: number;
  locationId: string;
  locationCode: string;
  locationName: string;
  quantityOnHand: number;
  averageCost: number;
  belowReorder: boolean;
  stockValue: number;
};

type Location = { id: string; code: string; name: string };

type ExpiringRow = {
  id: string; sku: string; productName: string; batchNo: string | null;
  expiryDate: string; daysToExpiry: number; quantityReceived: number;
  stockQuantity: number; unitSymbol: string | null;
  locationCode: string; locationName: string; grnNumber: string; receivedDate: string;
};

const fmtDay = (d: string | null) =>
  !d ? '—' : new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });

export default function InventoryPage() {
  const [rows, setRows] = useState<StockRow[]>([]);
  const [locations, setLocations] = useState<Location[]>([]);
  const [locationId, setLocationId] = useState<string | null>(null); // null = All
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');

  const [view, setView] = useState<'stock' | 'expiring'>('stock');
  const [expDays, setExpDays] = useState(30);
  const [expRows, setExpRows] = useState<ExpiringRow[]>([]);
  const [expLoading, setExpLoading] = useState(false);

  useEffect(() => {
    (async () => {
      try {
        const locs = await apiClient<Location[]>('/api/v1/locations');
        setLocations(locs);
      } catch (e) {
        setError((e as Error).message);
      }
    })();
  }, []);

  async function loadStock() {
    setLoading(true);
    setError(null);
    try {
      const qs = locationId ? `?locationId=${encodeURIComponent(locationId)}` : '';
      const data = await apiClient<StockRow[]>(`/api/v1/inventory/stock${qs}`);
      setRows(data);
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadStock();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [locationId]);

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    return rows
      .filter(r => (q ? `${r.name} ${r.sku}`.toLowerCase().includes(q) : true))
      .sort((a, b) => a.name.localeCompare(b.name));
  }, [rows, search]);

  const itemsTracked = filtered.length;
  const belowReorderCount = filtered.filter(r => r.belowReorder).length;
  const totalValue = filtered.reduce((sum, r) => sum + r.stockValue, 0);

  const pager = usePagination(filtered, undefined, `${locationId ?? ''}|${search}`);

  // Near-expiry batches (from posted GRN lines that carry an expiry).
  useEffect(() => {
    if (view !== 'expiring') return;
    (async () => {
      setExpLoading(true);
      try {
        const qs = new URLSearchParams({ days: String(expDays) });
        if (locationId) qs.set('locationId', locationId);
        setExpRows(await apiClient<ExpiringRow[]>(`/api/v1/inventory/expiring?${qs.toString()}`));
      } catch (e) {
        setError((e as Error).message);
      } finally {
        setExpLoading(false);
      }
    })();
  }, [view, expDays, locationId]);

  const expPager = usePagination(expRows, undefined, `${locationId ?? ''}|${expDays}`);

  return (
    <>
      <Topbar title="Inventory" subtitle="Always know exactly what's in stock and what it's worth" />

      <div className="p-6 md:p-8">
        {/* Header row */}
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Stock on Hand</h2>
            <HeaderStat>
              <Num>{rows.length}</Num> stock records{locationId ? ' at this location' : ' across all locations'}
            </HeaderStat>
          </div>
        </div>

        {/* View toggle: stock vs near-expiry */}
        <div className="mb-4 flex gap-1 border-b border-border">
          {([['stock', 'Stock on hand'], ['expiring', 'Expiring soon']] as const).map(([key, label]) => (
            <button
              key={key}
              onClick={() => setView(key)}
              className={`-mb-px border-b-2 px-3 py-2 text-sm font-medium transition-colors ${
                view === key ? 'border-primary text-primary' : 'border-transparent text-muted-foreground hover:text-foreground'
              }`}
            >
              {label}
            </button>
          ))}
        </div>

        {/* KPI tiles */}
        {view === 'stock' && (
        <div className="mb-5 grid grid-cols-1 gap-3 sm:grid-cols-3">
          <div className="card p-5">
            <p className="text-xs uppercase tracking-wide text-muted-foreground">Items tracked</p>
            <p className="mt-1 font-heading text-2xl font-bold tabular-nums">{itemsTracked}</p>
          </div>
          <div className="card p-5">
            <p className="text-xs uppercase tracking-wide text-muted-foreground">Below reorder</p>
            <p className="mt-1 font-heading text-2xl font-bold tabular-nums text-amber-600">
              {belowReorderCount}
            </p>
          </div>
          <div className="card p-5">
            <p className="text-xs uppercase tracking-wide text-muted-foreground">Total stock value</p>
            <p className="mt-1 font-heading text-2xl font-bold tabular-nums">{money(totalValue)}</p>
          </div>
        </div>
        )}

        {/* Filter bar */}
        <div className="mb-4 flex flex-wrap items-center gap-2">
          <button
            onClick={() => setLocationId(null)}
            className={`pill ${!locationId ? 'bg-primary text-primary-foreground' : 'pill-idle'}`}
          >
            All
          </button>
          {locations.map(l => (
            <button
              key={l.id}
              onClick={() => setLocationId(l.id)}
              className={`pill ${locationId === l.id ? 'bg-primary text-primary-foreground' : 'pill-idle'}`}
            >
              {l.code}
            </button>
          ))}
          {view === 'expiring' && (
            <label className="ml-auto flex items-center gap-1.5 text-sm text-muted-foreground">
              Within
              <select
                value={expDays}
                onChange={e => setExpDays(Number(e.target.value))}
                className="rounded-lg border border-border bg-card px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
              >
                {[7, 14, 30, 60, 90].map(d => <option key={d} value={d}>{d} days</option>)}
              </select>
            </label>
          )}
          {view === 'stock' && (
            <div className="relative ml-auto">
              <Search className="absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
              <input
                value={search}
                onChange={e => setSearch(e.target.value)}
                placeholder="Search by name or Product Code"
                className="rounded-lg border border-border bg-card py-1.5 pl-8 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
              />
            </div>
          )}
        </div>

        {/* Stock table */}
        {view === 'stock' && (<>
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
              <thead className="sticky top-0 border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th className="px-4 py-3 font-medium">Product</th>
                  <th className="px-4 py-3 font-medium">Product Code</th>
                  <th className="px-4 py-3 font-medium">Location</th>
                  <th className="px-4 py-3 text-right font-medium">On hand</th>
                  <th className="px-4 py-3 text-right font-medium">Reorder level</th>
                  <th className="px-4 py-3 text-right font-medium">Avg cost</th>
                  <th className="px-4 py-3 text-right font-medium">Stock value</th>
                </tr>
              </thead>
              <tbody>
                {pager.pageItems.map((r, i) => (
                  <tr key={`${r.productId}-${r.locationId}`} className={i % 2 ? 'bg-muted/20' : ''}>
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-2.5">
                        <span
                          className="size-3.5 shrink-0 rounded"
                          style={{ background: r.colorHex ?? '#94A3B8' }}
                        />
                        <span className="font-medium">{r.name}</span>
                      </div>
                    </td>
                    <td className="px-4 py-3 font-mono text-xs text-muted-foreground">{r.sku}</td>
                    <td className="px-4 py-3">
                      <span className="pill pill-idle">{r.locationCode}</span>
                    </td>
                    <td
                      className={`px-4 py-3 text-right font-semibold tabular-nums ${
                        r.belowReorder ? 'text-amber-600' : ''
                      }`}
                    >
                      {r.quantityOnHand}
                    </td>
                    <td className="px-4 py-3 text-right tabular-nums text-muted-foreground">
                      {r.reorderLevel}
                    </td>
                    <td className="px-4 py-3 text-right tabular-nums text-muted-foreground">
                      {money(r.averageCost)}
                    </td>
                    <td className="px-4 py-3 text-right font-semibold tabular-nums">
                      {money(r.stockValue)}
                    </td>
                  </tr>
                ))}
                {filtered.length === 0 && (
                  <tr>
                    <td colSpan={7} className="px-4 py-10 text-center text-muted-foreground">
                      No stock records match your filter.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>
        {!loading && !error && <Pagination {...pager} noun="stock records" />}

        <p className="mt-3 text-xs text-muted-foreground">
          Showing {filtered.length} of {rows.length} · amounts in LKR · on-hand shown amber when below reorder level
        </p>
        </>)}

        {/* Near-expiry batches */}
        {view === 'expiring' && (<>
        <div className="card overflow-hidden">
          {expLoading ? (
            <div className="space-y-2 p-4">
              {Array.from({ length: 8 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}
            </div>
          ) : (
            <table className="w-full text-sm">
              <thead className="sticky top-0 border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th className="px-4 py-3 font-medium">Product</th>
                  <th className="px-4 py-3 font-medium">Batch #</th>
                  <th className="px-4 py-3 font-medium">Location</th>
                  <th className="px-4 py-3 font-medium">Expiry</th>
                  <th className="px-4 py-3 text-right font-medium">Days left</th>
                  <th className="px-4 py-3 text-right font-medium">Received qty</th>
                  <th className="px-4 py-3 font-medium">GRN</th>
                </tr>
              </thead>
              <tbody>
                {expPager.pageItems.map((r, i) => {
                  const expired = r.daysToExpiry < 0;
                  const soon = r.daysToExpiry >= 0 && r.daysToExpiry <= 7;
                  return (
                    <tr key={r.id} className={i % 2 ? 'bg-muted/20' : ''}>
                      <td className="px-4 py-3 font-medium">{r.productName} <span className="font-mono text-xs text-muted-foreground">{r.sku}</span></td>
                      <td className="px-4 py-3 font-mono text-xs text-muted-foreground">{r.batchNo || '—'}</td>
                      <td className="px-4 py-3"><span className="pill pill-idle">{r.locationCode}</span></td>
                      <td className="px-4 py-3 tabular-nums">{fmtDay(r.expiryDate)}</td>
                      <td className="px-4 py-3 text-right">
                        <span className={`pill ${expired ? 'pill-void' : soon ? 'pill-pending' : 'pill-paid'}`}>
                          {expired ? `Expired ${-r.daysToExpiry}d ago` : `${r.daysToExpiry}d`}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-right tabular-nums">{r.quantityReceived}{r.unitSymbol ? ` ${r.unitSymbol}` : ''}</td>
                      <td className="px-4 py-3 font-mono text-xs text-muted-foreground">{r.grnNumber}</td>
                    </tr>
                  );
                })}
                {expRows.length === 0 && (
                  <tr>
                    <td colSpan={7} className="px-4 py-10 text-center text-muted-foreground">
                      No batches expiring within {expDays} days{locationId ? ' at this location' : ''}.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>
        {!expLoading && <Pagination {...expPager} noun="batches" />}
        <p className="mt-3 text-xs text-muted-foreground">
          Batch-level view from goods receipts that carry an expiry date (received quantity, not net on-hand).
        </p>
        </>)}
      </div>
    </>
  );
}
