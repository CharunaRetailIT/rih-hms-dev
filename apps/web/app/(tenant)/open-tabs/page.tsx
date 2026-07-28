'use client';

import { useCallback, useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient, money } from '@/lib/api-client';

type Tab = {
  id: string; orderNumber: string; orderType: string; orderSource: string | null;
  tableLabel: string | null; covers: number; status: string; totalAmount: number;
  openedAt: string; locationId: string; locationCode: string | null;
  itemCount: number; kitchenStatus: string;
};

const SOURCE: Record<string, string> = { guest_qr: 'Guest QR', handheld: 'Handheld', pos: 'Counter', ubereats: 'Uber Eats', pickme: 'PickMe' };
function sourceLabel(s: string | null) { return (s && SOURCE[s]) || 'Counter'; }

function statusChip(k: string) {
  const map: Record<string, [string, string]> = {
    ready: ['Ready', 'bg-green-100 text-green-700'],
    preparing: ['In kitchen', 'bg-amber-100 text-amber-700'],
    served: ['Served', 'bg-blue-100 text-blue-700'],
    building: ['New', 'bg-slate-100 text-slate-600'],
  };
  const [label, cls] = map[k] ?? map.building;
  return <span className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${cls}`}>{label}</span>;
}

export default function OpenTabsPage() {
  const router = useRouter();
  const [tabs, setTabs] = useState<Tab[]>([]);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState<string | null>(null);

  const load = useCallback(async () => {
    try { setTabs(await apiClient<Tab[]>('/api/v1/orders/open-tabs')); setErr(null); }
    catch (e) { setErr((e as Error).message); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => {
    load();
    const t = setInterval(load, 15000);   // live-ish refresh every 15s
    return () => clearInterval(t);
  }, [load]);
  const total = tabs.reduce((s, t) => s + (t.totalAmount || 0), 0);

  return (
    <div>
      <Topbar title="Open tabs" subtitle="Every open order across your outlets — counter, handheld and guest QR, live" />
      <div className="p-6">
        <div className="mb-4 flex flex-wrap items-center gap-3 text-sm text-muted-foreground">
          <span><strong className="text-foreground">{tabs.length}</strong> open tab{tabs.length === 1 ? '' : 's'}</span>
          <span>·</span>
          <span>Total <strong className="text-primary">{money(total)}</strong></span>
          <button onClick={load} className="ml-auto rounded-lg border border-border px-3 py-1.5 text-xs font-medium hover:bg-muted">Refresh</button>
        </div>

        <div className="card overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full min-w-[760px] text-sm">
              <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th className="px-4 py-3 font-medium">Table / Order</th>
                  <th className="px-4 py-3 font-medium">Outlet</th>
                  <th className="px-4 py-3 font-medium">Source</th>
                  <th className="px-4 py-3 text-right font-medium">Items</th>
                  <th className="px-4 py-3 text-right font-medium">Total</th>
                  <th className="px-4 py-3 font-medium">Kitchen</th>
                </tr>
              </thead>
              <tbody>
                {loading && <tr><td colSpan={6} className="px-4 py-10 text-center text-muted-foreground">Loading…</td></tr>}
                {err && !loading && <tr><td colSpan={6} className="px-4 py-10 text-center text-status-error">{err}</td></tr>}
                {!loading && !err && tabs.length === 0 && (
                  <tr><td colSpan={6} className="px-4 py-12 text-center text-muted-foreground">No open tabs right now.</td></tr>
                )}
                {tabs.map((t, i) => (
                  <tr key={t.id} className={`cursor-pointer hover:bg-muted/40 ${i % 2 ? 'bg-muted/20' : ''}`}
                    onClick={() => router.push(`/pos?order=${t.id}`)}>
                    <td className="px-4 py-3">
                      <div className="font-semibold">{t.tableLabel ? `Table ${t.tableLabel}` : 'Takeaway'}</div>
                      <div className="font-mono text-[11px] text-muted-foreground">{t.orderNumber}</div>
                    </td>
                    <td className="px-4 py-3">{t.locationCode ?? '—'}</td>
                    <td className="px-4 py-3">{sourceLabel(t.orderSource)}</td>
                    <td className="px-4 py-3 text-right tabular-nums">{t.itemCount}</td>
                    <td className="px-4 py-3 text-right font-semibold tabular-nums">{money(t.totalAmount)}</td>
                    <td className="px-4 py-3">{statusChip(t.kitchenStatus)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
        <p className="mt-3 text-xs text-muted-foreground">Tap a row to open it on the POS. Refreshes automatically every 15 seconds.</p>
      </div>
    </div>
  );
}
