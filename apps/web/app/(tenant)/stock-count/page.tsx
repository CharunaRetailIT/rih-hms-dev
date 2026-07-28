'use client';

import { useEffect, useState } from 'react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient } from '@/lib/api-client';
import { confirmDialog } from '@/components/ui/confirm';
import { Combobox } from '@/components/ui/form';
import { HeaderStat, Num } from '@/components/ui/HeaderStat';
import { Plus, X } from 'lucide-react';
import { Pagination } from '@/components/ui/Pagination';

type Location = { id: string; code: string; name: string };
type CountRow = { id: string; countNumber: string | null; locationId: string; status: string; notes: string | null; createdAt: string; postedAt: string | null };
type Line = { id: string; productId: string; sku: string | null; name: string | null; systemQty: number; countedQty: number; variance: number };
type CountDetail = CountRow & { lines: Line[] };
type PaginationMeta = { totalCount: number; pageNumber: number; pageSize: number; totalPages: number };
type PagedResponse = { data: CountRow[]; pagination: PaginationMeta };

export default function StockCountPage() {
  const [locations, setLocations] = useState<Location[]>([]);
  const [counts, setCounts] = useState<CountRow[]>([]);
  const [active, setActive] = useState<CountDetail | null>(null);
  const [loc, setLoc] = useState('');
  const [busy, setBusy] = useState(false);
  const [toast, setToast] = useState<string | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  function flash(m: string) { setToast(m); window.setTimeout(() => setToast(null), 3000); }

  async function loadList() {
    try {
      const res = await apiClient<PagedResponse>(`/api/v1/stock-counts/paged?pageNumber=${pageNumber}&pageSize=${pageSize}`);
      setCounts(res.data); setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages);
    } catch { /* ignore */ }
  }
  useEffect(() => { void loadList(); }, [pageNumber]);
  useEffect(() => {
    (async () => {
      try { const l = await apiClient<Location[]>('/api/v1/locations'); setLocations(l); setLoc(l.find(x => x.code === 'MAIN')?.id ?? l[0]?.id ?? ''); } catch { /* ignore */ }
    })();
  }, []);

  async function openCount(id: string) {
    try { setActive(await apiClient<CountDetail>(`/api/v1/stock-counts/${id}`)); }
    catch (e) { flash(extractError(e, 'Could not open the count.')); }
  }
  async function newCount() {
    if (!loc) { flash('Pick an outlet.'); return; }
    setBusy(true);
    try {
      const c = await apiClient<{ id: string }>('/api/v1/stock-counts', { method: 'POST', body: JSON.stringify({ locationId: loc, notes: null }) });
      await loadList(); await openCount(c.id); flash('Count opened — enter the physical quantities.');
    } catch (e) { flash(extractError(e, 'Could not start the count.')); }
    finally { setBusy(false); }
  }
  function setCounted(productId: string, v: string) {
    setActive(a => a && { ...a, lines: a.lines.map(l => l.productId === productId ? { ...l, countedQty: Number(v.replace(/[^0-9.\-]/g, '')) || 0 } : l) });
  }
  async function save() {
    if (!active) return; setBusy(true);
    try {
      await apiClient(`/api/v1/stock-counts/${active.id}/lines`, { method: 'PUT', body: JSON.stringify({ lines: active.lines.map(l => ({ productId: l.productId, countedQty: l.countedQty })) }) });
      flash('Counts saved.');
    } catch (e) { flash(extractError(e, 'Could not save.')); }
    finally { setBusy(false); }
  }
  async function discard() {
    if (!active) return;
    if (!(await confirmDialog({ title: `Discard ${active.countNumber ?? 'this count'}?`, body: 'This draft count will be removed. It never touched your stock, so nothing is adjusted.', confirmLabel: 'Discard draft', danger: true }))) return;
    setBusy(true);
    try {
      await apiClient(`/api/v1/stock-counts/${active.id}`, { method: 'DELETE' });
      flash('Draft discarded.'); setActive(null); await loadList();
    } catch (e) { flash(extractError(e, 'Could not discard the count.')); }
    finally { setBusy(false); }
  }
  async function post() {
    if (!active) return;
    if (!(await confirmDialog({ title: 'Post this count?', body: 'Counted quantities become the new on-hand stock and variances are adjusted. This cannot be undone.', confirmLabel: 'Post count' }))) return;
    setBusy(true);
    try {
      await save();
      const r = await apiClient<{ varianceLines: number }>(`/api/v1/stock-counts/${active.id}/post`, { method: 'POST', body: JSON.stringify({}) });
      flash(`Count posted · ${r.varianceLines} line(s) adjusted.`);
      await loadList(); await openCount(active.id);
    } catch (e) { flash(extractError(e, 'Could not post the count.')); }
    finally { setBusy(false); }
  }

  const draft = active?.status === 'draft';
  const variances = active ? active.lines.filter(l => l.countedQty !== l.systemQty).length : 0;

  return (
    <>
      <Topbar title="Stock Count" subtitle="Count your shelves and reconcile stock in a few taps" />
      <div className="grid grid-cols-[260px_1fr] gap-6 p-6 md:p-8">
        {/* Left: recent counts + new */}
        <div>
          <div className="mb-3 flex items-center gap-2">
            <Combobox className="flex-1" value={loc} onChange={setLoc} options={locations.map(l => ({ value: l.id, label: l.code }))} placeholder="Outlet" />
            <button onClick={newCount} disabled={busy} className="flex items-center gap-1 rounded-lg bg-primary px-3 py-2 text-sm font-semibold text-primary-foreground hover:bg-primary-dark disabled:opacity-50"><Plus className="size-4" /> New</button>
          </div>
          <div className="space-y-1">
            {counts.map(c => (
              <button key={c.id} onClick={() => openCount(c.id)}
                className={`flex w-full items-center justify-between rounded-lg border px-3 py-2 text-left text-sm ${active?.id === c.id ? 'border-primary bg-primary-tint' : 'border-border hover:bg-muted'}`}>
                <span className="font-mono text-xs">{c.countNumber ?? c.id.slice(0, 8)}</span>
                <span className={`pill ${c.status === 'posted' ? 'pill-paid' : c.status === 'void' ? 'pill-void' : 'pill-pending'}`}>{c.status}</span>
              </button>
            ))}
            {counts.length === 0 && <p className="px-1 py-4 text-center text-sm text-muted-foreground">No counts yet.</p>}
          </div>
          <Pagination
            page={pageNumber}
            totalPages={totalPages}
            total={totalCount}
            from={totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1}
            to={Math.min(pageNumber * pageSize, totalCount)}
            setPage={setPageNumber}
            noun="counts"
          />
        </div>

        {/* Right: count sheet */}
        {!active ? (
          <div className="card flex items-center justify-center p-10 text-sm text-muted-foreground">Start a new count or pick one on the left.</div>
        ) : (
          <div>
            <div className="mb-4 flex items-center justify-between">
              <div>
                <h2 className="font-heading text-xl font-bold">{active.countNumber ?? 'Count'} <span className={`pill ${active.status === 'posted' ? 'pill-paid' : 'pill-pending'}`}>{active.status}</span></h2>
                <HeaderStat><Num>{active.lines.length}</Num> items · <Num>{variances}</Num> with a variance</HeaderStat>
              </div>
              {draft && (
                <div className="flex gap-2">
                  <button onClick={discard} disabled={busy} className="rounded-lg border border-border px-4 py-2 text-sm font-semibold text-status-error hover:bg-muted disabled:opacity-50">Discard</button>
                  <button onClick={save} disabled={busy} className="rounded-lg border border-border px-4 py-2 text-sm font-semibold hover:bg-muted disabled:opacity-50">Save</button>
                  <button onClick={post} disabled={busy} className="rounded-lg bg-primary px-4 py-2 text-sm font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">Post Count</button>
                </div>
              )}
            </div>
            <div className="card overflow-hidden">
              <table className="w-full text-sm">
                <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                  <tr><th className="px-4 py-3 font-medium">Product Code</th><th className="px-4 py-3 font-medium">Product</th><th className="px-4 py-3 text-right font-medium">System</th><th className="px-4 py-3 text-right font-medium">Counted</th><th className="px-4 py-3 text-right font-medium">Variance</th></tr>
                </thead>
                <tbody>
                  {active.lines.map((l, i) => {
                    const v = (draft ? l.countedQty : l.countedQty) - l.systemQty;
                    return (
                      <tr key={l.id} className={i % 2 ? 'bg-muted/20' : ''}>
                        <td className="px-4 py-2.5 font-mono text-xs">{l.sku}</td>
                        <td className="px-4 py-2.5 font-medium">{l.name}</td>
                        <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{l.systemQty}</td>
                        <td className="px-4 py-2 text-right">
                          {draft
                            ? <input value={l.countedQty} onChange={e => setCounted(l.productId, e.target.value)} inputMode="decimal" className="w-24 rounded border border-border bg-surface px-2 py-1 text-right tabular-nums" />
                            : <span className="tabular-nums">{l.countedQty}</span>}
                        </td>
                        <td className={`px-4 py-2.5 text-right font-semibold tabular-nums ${v < 0 ? 'text-status-error' : v > 0 ? 'text-primary' : 'text-muted-foreground'}`}>{v === 0 ? '—' : v > 0 ? `+${v}` : v}</td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>
      {toast && <div className="fixed bottom-6 left-1/2 z-[80] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2 text-sm text-white shadow-lg">{toast}</div>}
    </>
  );
}

function extractError(e: unknown, fallback: string): string {
  const m = (e as Error)?.message ?? '';
  const j = m.match(/\{[\s\S]*\}$/);
  if (j) { try { return JSON.parse(j[0]).error ?? fallback; } catch { /* noop */ } }
  return m || fallback;
}
