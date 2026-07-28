'use client';

import { useEffect, useState } from 'react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient } from '@/lib/api-client';
import { HeaderStat, Num } from '@/components/ui/HeaderStat';
import { Combobox } from '@/components/ui/form';
import { Search } from 'lucide-react';
import { Pagination } from '@/components/ui/Pagination';

type Entry = {
  id: string; at: string; actorName: string | null; actorRole: string | null;
  action: string; entityType: string | null; entityId: string | null; summary: string | null;
};
type PaginationMeta = { totalCount: number; pageNumber: number; pageSize: number; totalPages: number };
type PagedResponse = { data: Entry[]; pagination: PaginationMeta };

const ACTIONS = [
  { value: '', label: 'All actions' },
  { value: 'order.settle', label: 'Settlements' },
  { value: 'order.void', label: 'Voids' },
  { value: 'order.discount', label: 'Discounts' },
  { value: 'shift.open', label: 'Shift opened' },
  { value: 'shift.close', label: 'Shift closed' },
  { value: 'permissions.update', label: 'Permission changes' },
];

const ACTION_STYLE: Record<string, string> = {
  'order.settle': 'pill-paid', 'order.void': 'pill-void', 'order.discount': 'pill-idle',
  'shift.open': 'pill-progress', 'shift.close': 'pill-progress', 'permissions.update': 'pill-idle',
};

export default function AuditPage() {
  const [entries, setEntries] = useState<Entry[]>([]);
  const [action, setAction] = useState('');
  const [search, setSearch] = useState('');
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setLoading(true); setError(null);
    try {
      const from = new Date(Date.now() - 30 * 86400000).toISOString();
      const qs = new URLSearchParams({ from, pageNumber: String(pageNumber), pageSize: String(pageSize) });
      if (action) qs.set('action', action);
      if (search.trim()) qs.set('search', search.trim());
      const res = await apiClient<PagedResponse>(`/api/v1/audit/paged?${qs}`);
      setEntries(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages);
    } catch (e) { setError((e as Error).message); }
    finally { setLoading(false); }
  }
  useEffect(() => { void load(); }, [pageNumber, pageSize]);   // eslint-disable-line react-hooks/exhaustive-deps
  useEffect(() => { setPageNumber(1); void load(); }, [action]);   // eslint-disable-line react-hooks/exhaustive-deps
  useEffect(() => {
    const t = window.setTimeout(() => { setPageNumber(1); void load(); }, 350);
    return () => window.clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search]);

  const pageItems = entries;

  return (
    <>
      <Topbar title="Activity log" subtitle="A clear, trustworthy trail of who did what, and when" />
      <div className="p-6 md:p-8">
        <div className="mb-5 flex flex-wrap items-end justify-between gap-3">
          <div>
            <h2 className="font-heading text-xl font-bold">Activity log</h2>
            <HeaderStat><Num>{totalCount}</Num> actions · last 30 days</HeaderStat>
          </div>
          <div className="flex flex-wrap items-center gap-2">
            <div className="relative">
              <Search className="absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
              <input value={search} onChange={e => setSearch(e.target.value)} placeholder="Search who, action, detail…"
                className="h-10 w-64 rounded-lg border border-border bg-surface py-2 pl-8 pr-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20" />
            </div>
            <Combobox className="w-52" value={action} onChange={setAction}
              placeholder="All actions" searchPlaceholder="Filter actions…" options={ACTIONS} />
          </div>
        </div>

        <div className="card overflow-hidden">
          {loading ? (
            <div className="space-y-2 p-4">{Array.from({ length: 8 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
          ) : error ? (
            <div className="p-6 text-sm text-status-error">{error}</div>
          ) : pageItems.length === 0 ? (
            <div className="p-10 text-center text-sm text-muted-foreground">{search || action ? 'No activity matches your filter.' : 'No activity recorded in the last 30 days.'}</div>
          ) : (
            <div className="overflow-x-auto">
            <table className="w-full min-w-[640px] text-sm">
              <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th className="px-4 py-3 font-medium">When</th>
                  <th className="px-4 py-3 font-medium">Who</th>
                  <th className="px-4 py-3 font-medium">Action</th>
                  <th className="px-4 py-3 font-medium">Detail</th>
                </tr>
              </thead>
              <tbody>
                {pageItems.map((e, i) => (
                  <tr key={e.id} className={i % 2 ? 'bg-muted/20' : ''}>
                    <td className="whitespace-nowrap px-4 py-3 text-xs text-muted-foreground">{new Date(e.at).toLocaleString('en-LK', { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })}</td>
                    <td className="px-4 py-3">{e.actorName ?? '—'}{e.actorRole ? <span className="ml-1 text-xs text-muted-foreground">({e.actorRole})</span> : null}</td>
                    <td className="px-4 py-3"><span className={`pill ${ACTION_STYLE[e.action] ?? 'pill-idle'}`}>{e.action}</span></td>
                    <td className="px-4 py-3 text-muted-foreground">{e.summary}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            </div>
          )}
        </div>

        {!loading && !error && totalCount > 0 && (
          <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
            <select
              value={pageSize}
              onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
              className="rounded-lg border border-border bg-surface px-2 py-1.5 text-xs"
            >
              {[25, 50, 100].map(n => <option key={n} value={n}>{n} / page</option>)}
            </select>
            <Pagination
              page={pageNumber}
              totalPages={totalPages}
              total={totalCount}
              from={totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1}
              to={Math.min(pageNumber * pageSize, totalCount)}
              setPage={setPageNumber}
              noun="actions"
              className="mt-0 flex-1"
            />
          </div>
        )}
      </div>
    </>
  );
}
