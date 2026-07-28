'use client';

import { useEffect, useMemo, useRef, useState, useCallback } from 'react';
import { apiClient } from '@/lib/api-client';
import { Icon } from '@/components/ui/Icon';

type Ticket = {
  id: string; ticketNumber: string; station: string; orderLabel: string;
  orderSource: string; status: string; itemsJson: string; createdAt: string; readyAt: string | null;
  orderStatus?: string | null; orderNumber?: string | null;
};
type TicketItem = { Quantity: number; ProductName: string; Notes: string | null };
type Station = { id: string; code: string; name: string };

const BAR_COLOR = '#6d89fa';      // tertiary
const KITCHEN_COLOR = '#006b63';  // primary

// Current-status pill styling for the dark KDS theme.
const KDS_STATUS: Record<string, { label: string; cls: string }> = {
  new:       { label: 'Queued',    cls: 'bg-slate-500/20 text-slate-300' },
  preparing: { label: 'Preparing', cls: 'bg-amber-500/20 text-amber-300' },
  ready:     { label: 'Ready',     cls: 'bg-green-500/20 text-green-400' },
  served:    { label: 'Served',    cls: 'bg-slate-500/20 text-slate-400' },
};
const iconFor = (code: string) => {
  const c = code.toLowerCase();
  if (c.includes('bar')) return 'local_bar';
  if (c.includes('pizza')) return 'local_pizza';
  if (c.includes('coffee') || c.includes('bev')) return 'coffee';
  if (c.includes('dessert')) return 'icecream';
  return 'restaurant';
};

export default function KotPage() {
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [stations, setStations] = useState<Station[]>([]);
  const [station, setStation] = useState<string>('all');
  // null until mounted — rendering Date.now() during SSR mismatches the client
  // clock by ~1s and throws a hydration error. Set it only on the client.
  const [now, setNow] = useState<number | null>(null);
  const [recallMode, setRecallMode] = useState(false);
  const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'ready' | 'overdue'>('all');

  // Always fetch ALL active (or recalled) tickets; filter + count client-side so
  // the per-station tabs stay live and case-insensitive (codes are upper-case).
  const load = useCallback(async () => {
    try {
      const q = recallMode ? '?recall=true' : '';
      setTickets(await apiClient<Ticket[]>(`/api/v1/kitchen/tickets${q}`));
    } catch { /* keep last */ }
  }, [recallMode]);

  useEffect(() => { apiClient<Station[]>('/api/v1/kitchen-stations').then(setStations).catch(() => setStations([])); }, []);

  const sameStation = (a: string | null | undefined, b: string) => (a ?? '').toLowerCase() === b.toLowerCase();
  const minsSince = (iso: string) => now == null ? 0 : (now - new Date(iso).getTime()) / 60000;
  const byStation = station === 'all' ? tickets : tickets.filter(t => sameStation(t.station, station));
  const shown = byStation.filter(t =>
    statusFilter === 'all' ? true
    : statusFilter === 'ready' ? t.status === 'ready'
    : statusFilter === 'active' ? t.status !== 'ready'
    : /* overdue */ t.status !== 'ready' && minsSince(t.createdAt) > 10);
  const countFor = (code: string) => tickets.filter(t => sameStation(t.station, code)).length;

  // All-day: total quantity of each item across every live ticket (the line's
  // running "how many of X do I still have to make" view).
  const allDay = useMemo(() => {
    const m = new Map<string, number>();
    for (const t of tickets) {
      let items: TicketItem[] = [];
      try { items = t.itemsJson ? JSON.parse(t.itemsJson) : []; } catch { items = []; }
      for (const it of items) m.set(it.ProductName, (m.get(it.ProductName) ?? 0) + Number(it.Quantity || 0));
    }
    return [...m.entries()].sort((a, b) => b[1] - a[1]);
  }, [tickets]);
  const navStations = [{ code: 'all', name: 'All Stations' }, ...stations.map(s => ({ code: s.code, name: s.name }))];

  async function recallTicket(id: string) {
    await apiClient(`/api/v1/kitchen/tickets/${id}/status`, { method: 'POST', body: JSON.stringify({ status: 'preparing' }) });
    setRecallMode(false);   // back to the live board, where the ticket now reappears
  }

  // Initial load + a slow safety-net poll (the SSE stream below does the real-time
  // work; this just covers a dropped stream or a missed event).
  useEffect(() => { load(); const t = setInterval(load, 15000); return () => clearInterval(t); }, [load]);
  useEffect(() => { setNow(Date.now()); const t = setInterval(() => setNow(Date.now()), 1000); return () => clearInterval(t); }, []);

  // Real-time board: subscribe to the kitchen SSE stream so a new ticket / bump /
  // settle / void shows up instantly instead of on the next poll. Consumed with
  // fetch (not EventSource) so the JWT stays on the Authorization header — no
  // token in the URL. Auto-reconnects with backoff; refetches the latest `load`
  // via a ref so it never reconnects just because the recall filter toggled.
  const loadRef = useRef(load); loadRef.current = load;
  useEffect(() => {
    let stop = false; let ctrl: AbortController | null = null; let backoff = 1000;
    async function connect() {
      while (!stop) {
        ctrl = new AbortController();
        try {
          const token = typeof window !== 'undefined' ? localStorage.getItem('hms.token') : null;
          const res = await fetch('/api/v1/kitchen/stream', {
            headers: token ? { Authorization: `Bearer ${token}` } : {}, signal: ctrl.signal, cache: 'no-store',
          });
          if (!res.ok || !res.body) throw new Error(`stream ${res.status}`);
          backoff = 1000;                                  // connected — reset backoff
          const reader = res.body.getReader(); const dec = new TextDecoder(); let buf = '';
          for (;;) {
            const { value, done } = await reader.read();
            if (done) break;
            buf += dec.decode(value, { stream: true });
            let nl: number;
            while ((nl = buf.indexOf('\n')) >= 0) {
              const line = buf.slice(0, nl).trim(); buf = buf.slice(nl + 1);
              if (line.startsWith('data:')) void loadRef.current();   // board changed → refetch
            }
          }
        } catch { /* fall through to reconnect */ }
        if (stop) break;
        await new Promise(r => setTimeout(r, backoff));
        backoff = Math.min(backoff * 2, 15000);
      }
    }
    void connect();
    return () => { stop = true; ctrl?.abort(); };
  }, []);

  async function setStatus(id: string, status: string) {
    await apiClient(`/api/v1/kitchen/tickets/${id}/status`, {
      method: 'POST', body: JSON.stringify({ status }),
    });
    load();
  }

  const minutesOf = (iso: string) => now == null ? 0 : (now - new Date(iso).getTime()) / 60000;
  // Once ready/served the make-time is frozen at readyAt (the ticker stops);
  // otherwise it counts up live from when the ticket fired.
  const isDone = (t: Ticket) => t.status === 'ready' || t.status === 'served';
  const endMsFor = (t: Ticket) => isDone(t) && t.readyAt ? new Date(t.readyAt).getTime() : now;
  const timeFor = (t: Ticket) => {
    const end = endMsFor(t);
    if (end == null) return '00:00';
    const secs = Math.max(0, Math.floor((end - new Date(t.createdAt).getTime()) / 1000));
    const m = Math.floor(secs / 60), s = secs % 60;
    return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
  };
  const urgencyFor = (t: Ticket) => {
    if (isDone(t)) return { color: 'text-green-500', glow: 'timer-glow-green' as const };
    const mins = minutesOf(t.createdAt);
    if (mins > 10) return { color: 'text-error', glow: 'timer-glow-red' as const };
    if (mins > 5) return { color: 'text-yellow-500', glow: 'timer-glow-amber' as const };
    return { color: 'text-green-500', glow: 'timer-glow-green' as const };
  };

  const initials = (label: string) =>
    label.replace(/[^A-Za-z ]/g, '').split(' ').filter(Boolean).map(w => w[0]).join('').slice(0, 2).toUpperCase() || '--';

  const active = tickets.filter(t => t.status !== 'ready').length;
  const ready = tickets.filter(t => t.status === 'ready').length;
  const overdue = tickets.filter(t => t.status !== 'ready' && minutesOf(t.createdAt) > 10).length;
  const clock = useMemo(
    () => now == null ? '' : new Date(now).toLocaleTimeString('en-US', { hour12: true, hour: '2-digit', minute: '2-digit', second: '2-digit' }).toUpperCase(),
    [now],
  );

  return (
    <div className="flex h-screen flex-col overflow-hidden bg-[#020617] font-sans text-slate-50">
      <style>{`
        .ready-pulse { animation: kds-pulse-border 2s cubic-bezier(0.4,0,0.6,1) infinite; }
        @keyframes kds-pulse-border {
          0%,100% { border-color: rgba(34,197,94,0.3); }
          50% { border-color: rgba(34,197,94,1); }
        }
        .timer-glow-red { text-shadow: 0 0 10px rgba(168,56,54,0.5); }
        .timer-glow-amber { text-shadow: 0 0 10px rgba(234,179,8,0.5); }
        .timer-glow-green { text-shadow: 0 0 10px rgba(34,197,94,0.5); }
      `}</style>

      {/* Top App Bar */}
      <header className="z-50 flex h-14 items-center justify-between border-b border-white/10 bg-[#020617] px-8">
        <div className="flex items-center gap-6">
          <span className="font-heading text-xl font-black uppercase tracking-tighter text-primary">DEMO RESTAURANT · KITCHEN</span>
          <div className="font-mono text-lg text-slate-400 tabular-nums">{clock}</div>
        </div>
        <div className="flex items-center gap-3">
          {/* Clickable status filters — tap to filter the board, tap again to clear. */}
          {([
            { key: 'active' as const, label: 'ACTIVE', count: active, dot: '#8ee4d9', text: '#8ee4d9' },
            { key: 'ready' as const, label: 'READY', count: ready, dot: '#22c55e', text: '#22c55e' },
            { key: 'overdue' as const, label: 'OVERDUE', count: overdue, dot: '#a83836', text: '#f87171' },
          ]).map(c => {
            const on = statusFilter === c.key;
            return (
              <button key={c.key} onClick={() => setStatusFilter(f => f === c.key ? 'all' : c.key)}
                title={on ? 'Showing this filter — tap to clear' : `Filter to ${c.label.toLowerCase()}`}
                className={`flex items-center gap-2 rounded-lg px-3 py-1.5 transition-colors ${on ? 'bg-white/10 ring-1 ring-white/20' : 'hover:bg-white/5'}`}>
                <span className={`h-3 w-3 rounded-full ${c.key === 'active' ? 'animate-pulse' : ''}`} style={{ backgroundColor: c.dot }} />
                <span className="text-lg font-bold" style={{ color: c.text }}>{c.count} {c.label}</span>
              </button>
            );
          })}
          <button
            onClick={() => { ['hms.token', 'hms.refresh', 'hms.tenant', 'hms.user', 'hms.location'].forEach(k => localStorage.removeItem(k)); window.location.href = '/login'; }}
            title="Sign out"
            className="flex items-center gap-1.5 rounded-lg border border-white/15 px-3 py-1.5 text-sm text-slate-300 hover:border-error hover:text-error"
          >
            <Icon name="logout" className="text-base" /> Sign out
          </button>
        </div>
      </header>

      <div className="flex flex-1 overflow-hidden">
        {/* Left station strip */}
        <aside className="flex w-64 flex-col border-r border-white/10 bg-[#0f172a]">
          <nav className="flex-1 space-y-2 overflow-y-auto p-4">
            {navStations.map(s => {
              const activeFilter = station === s.code;
              const count = s.code === 'all' ? tickets.length : countFor(s.code);
              return (
                <button
                  key={s.code}
                  onClick={() => setStation(s.code)}
                  className={`flex w-full items-center gap-3 rounded-lg p-4 text-left font-bold transition-colors ${
                    activeFilter ? 'bg-primary text-white' : 'font-semibold text-slate-400 hover:bg-slate-700'
                  }`}
                >
                  <Icon name={s.code === 'all' ? 'grid_view' : iconFor(s.code)} className="text-xl" />
                  <span className="flex-1 truncate">{s.name}</span>
                  <span className={`min-w-6 rounded-full px-2 py-0.5 text-center text-xs font-bold ${activeFilter ? 'bg-white/20 text-white' : 'bg-slate-700 text-slate-300'}`}>{count}</span>
                </button>
              );
            })}
          </nav>
          {!recallMode && allDay.length > 0 && (
            <div className="border-t border-white/10 p-4">
              <div className="mb-2 text-[10px] font-bold uppercase tracking-wider text-slate-500">All-day totals</div>
              <div className="max-h-48 space-y-1 overflow-y-auto">
                {allDay.map(([name, qty]) => (
                  <div key={name} className="flex items-center justify-between text-sm">
                    <span className="truncate text-slate-300">{name}</span>
                    <span className="ml-2 min-w-6 rounded bg-slate-700 px-1.5 text-center text-xs font-bold text-white">{qty}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
          <button
            onClick={() => setRecallMode(m => !m)}
            className={`m-4 flex items-center justify-center gap-2 rounded border p-4 transition-all ${recallMode ? 'border-primary bg-primary/20 text-white' : 'border-white/20 text-slate-400 hover:bg-slate-700'}`}
          >
            <Icon name="history" className="text-xl" />
            {recallMode ? 'Back to board' : 'Recall last 10'}
          </button>
        </aside>

        {/* KDS ticket grid */}
        <main className="grid flex-1 auto-rows-min grid-cols-1 gap-6 overflow-y-auto bg-[#020617] p-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {shown.length === 0 && (
            <p className="col-span-full mt-20 text-center text-slate-500">
              {recallMode ? 'No recently served tickets to recall.'
                : station !== 'all' ? 'No active tickets at this station.'
                : 'No active tickets. New orders appear here as they’re sent to the kitchen.'}
            </p>
          )}
          {shown.map(t => {
            let items: TicketItem[] = [];
            try { items = t.itemsJson ? JSON.parse(t.itemsJson) : []; } catch { items = []; }
            const isBar = (t.station ?? '').toLowerCase().includes('bar');
            const u = urgencyFor(t);
            const isReady = t.status === 'ready';
            const isOverdue = !isReady && minutesOf(t.createdAt) > 10;
            const ring = isReady
              ? 'ready-pulse border-2'
              : isOverdue
                ? 'ring-2 ring-error/50'
                : 'ring-1 ring-white/10';
            return (
              <article
                key={t.id}
                className={`flex flex-col overflow-hidden rounded-lg bg-[#1e293b] shadow-xl ${ring}`}
                style={{ borderLeft: `4px solid ${isBar ? BAR_COLOR : KITCHEN_COLOR}` }}
              >
                <header className="flex items-start justify-between bg-[#334155] p-4">
                  <div>
                    <div className="flex items-center gap-2">
                      <div className="text-xs font-bold uppercase tracking-widest text-slate-400">{t.orderLabel}{t.orderNumber ? ` · ${t.orderNumber}` : ''}</div>
                      {t.orderStatus === 'settled' && (
                        <span className="flex items-center gap-0.5 rounded bg-green-500/15 px-1.5 text-[10px] font-bold uppercase tracking-wide text-green-400">
                          <Icon name="paid" className="text-[12px]" /> Paid
                        </span>
                      )}
                    </div>
                    <div className="text-2xl font-black">
                      {t.ticketNumber} <span className="text-sm font-normal text-slate-400">{initials(t.orderLabel)}</span>
                    </div>
                  </div>
                  <div className="text-right">
                    <div className={`font-mono text-2xl font-bold tabular-nums ${u.color} ${u.glow}`}>
                      {timeFor(t)}
                    </div>
                    {isOverdue && (
                      <div className="flex items-center gap-1 rounded-full bg-error/10 px-2 text-[10px] font-bold uppercase text-error">
                        <Icon name="priority_high" className="text-[12px]" /> Overdue
                      </div>
                    )}
                    {isReady && (
                      <div className="flex items-center justify-end gap-1 text-[10px] font-bold uppercase text-green-500">
                        <Icon name="check_circle" className="text-[12px]" /> Ready
                      </div>
                    )}
                  </div>
                </header>

                <div className={`flex-1 space-y-4 overflow-y-auto p-4 ${isReady ? 'opacity-50' : ''}`}>
                  {items.map((it, i) => (
                    <div key={i} className="flex items-start gap-3">
                      <span
                        className={`mt-1 flex h-6 w-6 shrink-0 items-center justify-center rounded text-sm font-bold ${
                          isReady ? 'bg-green-500 text-white' : 'bg-[#0a121e] text-slate-300 ring-1 ring-white/10'
                        }`}
                      >
                        {Number(it.Quantity)}
                      </span>
                      <div>
                        <div className={`text-xl font-bold leading-tight ${isReady ? 'line-through' : ''}`}>
                          {Number(it.Quantity)}× {it.ProductName}
                        </div>
                        {it.Notes && <div className="text-sm italic text-slate-400">{it.Notes}</div>}
                      </div>
                    </div>
                  ))}
                </div>

                <footer className="flex flex-col gap-2 bg-[#0f172a] p-2">
                  <div className="flex items-center justify-between px-2">
                    <span
                      className={`rounded px-2 py-0.5 text-[10px] font-bold uppercase ${
                        isBar ? 'bg-[#6d89fa]/20 text-[#9cc0ff]' : 'bg-white/10 text-slate-400'
                      }`}
                    >
                      {t.station}
                    </span>
                    <span className={`flex items-center gap-1 rounded-full px-2 py-0.5 text-[10px] font-bold uppercase tracking-wide ${KDS_STATUS[t.status]?.cls ?? 'bg-white/10 text-slate-400'}`}>
                      <span className="size-1.5 rounded-full bg-current opacity-80" />
                      {KDS_STATUS[t.status]?.label ?? t.status}
                    </span>
                  </div>
                  {recallMode || t.status === 'served' ? (
                    <button
                      onClick={() => recallTicket(t.id)}
                      className="h-14 w-full rounded-md border border-amber-400/40 bg-amber-500/20 font-black uppercase tracking-tight text-amber-200 transition-all hover:bg-amber-500/30 active:scale-[0.98]"
                    >
                      Bring back
                    </button>
                  ) : (<>
                  {t.status !== 'preparing' && t.status !== 'ready' && (
                    <button
                      onClick={() => setStatus(t.id, 'preparing')}
                      className="h-14 w-full rounded-md border border-white/20 bg-[#334155] font-black uppercase tracking-tight text-primary-fixed-dim transition-all hover:bg-primary/20 active:scale-[0.98]"
                      style={{ color: '#8ee4d9' }}
                    >
                      Start preparing
                    </button>
                  )}
                  {t.status === 'preparing' && (
                    <button
                      onClick={() => setStatus(t.id, 'ready')}
                      className="h-14 w-full rounded-md border border-white/20 bg-[#334155] font-black uppercase tracking-tight transition-all hover:bg-primary/20 active:scale-[0.98]"
                      style={{ color: '#8ee4d9' }}
                    >
                      Mark all ready
                    </button>
                  )}
                  {t.status === 'ready' && (
                    <button
                      onClick={() => setStatus(t.id, 'served')}
                      className="h-14 w-full rounded-md bg-green-600 font-black uppercase tracking-tight text-white transition-all hover:bg-green-500 active:scale-[0.98]"
                    >
                      Clear Ticket
                    </button>
                  )}
                  </>)}
                </footer>
              </article>
            );
          })}

          {tickets.length === 0 && (
            <div className="col-span-full flex flex-col items-center justify-center py-24 text-center text-slate-500">
              <Icon name="skillet" className="mb-3 text-5xl opacity-40" />
              <p className="font-bold uppercase tracking-widest">No active tickets. The kitchen is quiet.</p>
            </div>
          )}
        </main>
      </div>

      {/* Bottom status bar */}
      <footer className="flex h-10 items-center justify-between border-t border-white/10 bg-[#020617] px-6 text-xs">
        <div className="flex items-center gap-4 font-medium text-slate-400">
          <span className="flex items-center gap-1"><span className="h-2 w-2 rounded-full" style={{ backgroundColor: '#8ee4d9' }} /> {active} active</span>
          <span className="text-slate-600">|</span>
          <span className="flex items-center gap-1"><span className="h-2 w-2 rounded-full bg-green-500" /> {ready} ready</span>
          <span className="text-slate-600">|</span>
          <span className="flex items-center gap-1"><span className="h-2 w-2 rounded-full bg-error" /> {overdue} overdue</span>
        </div>
        <div className="flex items-center gap-6">
          <div className="flex items-center gap-2">
            <span className="text-slate-400">Aggregator:</span>
            <span className="flex items-center gap-1 font-bold text-green-500">
              <span className="h-2 w-2 rounded-full bg-green-500" /> connected
            </span>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-slate-400">Printer station 1:</span>
            <span className="font-bold text-green-500">online</span>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-slate-400">Printer station 2:</span>
            <span className="font-bold text-error">offline</span>
          </div>
        </div>
      </footer>
    </div>
  );
}
