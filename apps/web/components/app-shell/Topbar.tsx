'use client';

import { useCallback, useEffect, useRef, useState } from 'react';
import { useRouter } from 'next/navigation';
import {
  Search, Bell, HelpCircle, X, Package, User, Receipt, Truck, AlertTriangle,
  CalendarClock, PartyPopper, Loader2,
} from 'lucide-react';
import { apiClient, lkr } from '@/lib/api-client';
import { ensureNotifyPermission, showNotification, notifyPermission } from '@/lib/webnotify';

type NotifItem = { type: string; severity: string; title: string; detail: string; href: string; at: string };
type NotifResp = { count: number; items: NotifItem[] };
type SearchHit = { kind: 'product' | 'customer' | 'transaction'; id: string; title: string; sub: string; href: string };

const NOTIF_ICON: Record<string, typeof Bell> = {
  aggregator: Truck, low_stock: AlertTriangle, reservation: CalendarClock, catering: PartyPopper,
};

export function Topbar({ title, subtitle }: { title: string; subtitle?: string }) {
  const router = useRouter();
  const [searchOpen, setSearchOpen] = useState(false);
  const [notifOpen, setNotifOpen] = useState(false);
  const [notif, setNotif] = useState<NotifResp>({ count: 0, items: [] });
  const [perm, setPerm] = useState<'granted' | 'denied' | 'default' | 'unsupported'>('default');
  const [tip, setTip] = useState<string | null>(null);
  const notifRef = useRef<HTMLDivElement>(null);
  const lastTopRef = useRef<string | null>(null);   // signature of the newest item we've already alerted on

  useEffect(() => { setPerm(notifyPermission()); }, []);

  // --- Notifications: load on mount + poll every 60s (safety net). When a NEW
  // top item appears (and desktop alerts are granted), also fire a browser
  // notification so staff get pinged even if they're on another tab.
  const loadNotif = useCallback(async () => {
    try {
      const data = await apiClient<NotifResp>('/api/v1/notifications');
      setNotif(data);
      const top = data.items[0];
      const sig = top ? `${top.title}|${top.at}` : '';
      if (lastTopRef.current !== null && sig && sig !== lastTopRef.current) {
        showNotification(top.title, top.detail, top.href);
      }
      lastTopRef.current = sig;
    } catch { /* unauthenticated or transient — leave as-is */ }
  }, []);

  function flashTip(m: string) { setTip(m); window.setTimeout(() => setTip(null), 6000); }
  async function sendTestAlert() {
    const ok = await ensureNotifyPermission();
    setPerm(notifyPermission());
    if (!ok) {
      flashTip('Desktop alerts are blocked. Click the lock icon in the address bar → Notifications → Allow, then try again.');
      return;
    }
    const shown = showNotification('RIT HMS — test alert', 'Desktop notifications are working ✅ You’ll be pinged on new orders & low stock.', '/dashboard');
    flashTip(shown
      ? 'Test alert sent ✅ Check your desktop — if nothing appeared, your OS “Do Not Disturb” may be on.'
      : 'Alerts are enabled, but this browser couldn’t show a desktop notification here.');
  }
  useEffect(() => {
    loadNotif();
    const t = setInterval(loadNotif, 60_000);
    return () => clearInterval(t);
  }, [loadNotif]);

  // --- Real-time: subscribe to the app-wide SSE stream so the bell updates
  // instantly on a new delivery order / settle / void instead of waiting for
  // the poll. fetch (not EventSource) keeps the JWT on the Authorization
  // header; auto-reconnects with backoff; refetch is debounced + via a ref so
  // the stream never reconnects on a re-render. The 60s poll above stays as a
  // backstop if the stream drops.
  const loadNotifRef = useRef(loadNotif); loadNotifRef.current = loadNotif;
  useEffect(() => {
    let stop = false; let ctrl: AbortController | null = null; let backoff = 1000;
    let debounce: ReturnType<typeof setTimeout> | null = null;
    const LIVE = new Set(['notifications', 'orders', 'aggregator']);
    const bump = () => { if (debounce) clearTimeout(debounce); debounce = setTimeout(() => loadNotifRef.current(), 400); };
    async function connect() {
      while (!stop) {
        ctrl = new AbortController();
        try {
          const token = typeof window !== 'undefined' ? localStorage.getItem('hms.token') : null;
          const res = await fetch('/api/v1/events/stream', {
            headers: token ? { Authorization: `Bearer ${token}` } : {}, signal: ctrl.signal, cache: 'no-store',
          });
          if (!res.ok || !res.body) throw new Error(`stream ${res.status}`);
          backoff = 1000;
          const reader = res.body.getReader(); const dec = new TextDecoder(); let buf = '';
          for (;;) {
            const { value, done } = await reader.read();
            if (done) break;
            buf += dec.decode(value, { stream: true });
            let nl: number;
            while ((nl = buf.indexOf('\n')) >= 0) {
              const line = buf.slice(0, nl).trim(); buf = buf.slice(nl + 1);
              if (line.startsWith('data:')) { const topic = line.slice(5).trim(); if (LIVE.has(topic)) bump(); }
            }
          }
        } catch { /* fall through to reconnect */ }
        if (stop) break;
        await new Promise(r => setTimeout(r, backoff));
        backoff = Math.min(backoff * 2, 15000);
      }
    }
    void connect();
    return () => { stop = true; ctrl?.abort(); if (debounce) clearTimeout(debounce); };
  }, []);

  // Close the notifications dropdown on outside-click.
  useEffect(() => {
    if (!notifOpen) return;
    const onClick = (e: MouseEvent) => {
      if (notifRef.current && !notifRef.current.contains(e.target as Node)) setNotifOpen(false);
    };
    document.addEventListener('mousedown', onClick);
    return () => document.removeEventListener('mousedown', onClick);
  }, [notifOpen]);

  // Cmd/Ctrl+K opens search; Esc closes overlays.
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 'k') { e.preventDefault(); setSearchOpen(true); }
      if (e.key === 'Escape') { setSearchOpen(false); setNotifOpen(false); }
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, []);

  const go = (href: string) => { setNotifOpen(false); setSearchOpen(false); router.push(href); };

  return (
    <header className="flex items-center justify-between border-b border-border bg-card px-6 py-4 md:px-8">
      <div>
        <h1 className="font-heading text-2xl font-bold leading-tight tracking-tight">{title}</h1>
        {subtitle && <p className="mt-0.5 text-sm text-muted-foreground">{subtitle}</p>}
      </div>
      <div className="flex items-center gap-1">
        <button onClick={() => setSearchOpen(true)}
          className="flex items-center gap-2 rounded-lg px-2.5 py-2 text-muted-foreground hover:bg-muted" title="Search (⌘K)">
          <Search className="size-5" />
          <kbd className="hidden rounded border border-border bg-muted px-1.5 text-[10px] font-medium text-muted-foreground md:inline">⌘K</kbd>
        </button>

        {/* Notifications */}
        <div className="relative" ref={notifRef}>
          <button onClick={() => { setNotifOpen(o => !o); if (!notifOpen) loadNotif(); }}
            className="relative rounded-lg p-2 text-muted-foreground hover:bg-muted" title="Notifications">
            <Bell className="size-5" />
            {notif.count > 0 && (
              <span className="absolute right-0.5 top-0.5 flex min-w-4 items-center justify-center rounded-full bg-secondary-container px-1 text-[10px] font-bold text-on-surface ring-2 ring-card">
                {notif.count > 99 ? '99+' : notif.count}
              </span>
            )}
          </button>
          {notifOpen && (
            <div className="absolute right-0 z-50 mt-2 w-96 overflow-hidden rounded-xl border border-border bg-card shadow-2xl">
              <div className="flex items-center justify-between border-b border-border px-4 py-3">
                <span className="font-heading text-sm font-bold">Notifications</span>
                <div className="flex items-center gap-2">
                  <span className="text-xs text-muted-foreground">{notif.count} active</span>
                  {perm !== 'unsupported' && (
                    <button onClick={sendTestAlert}
                      title={perm === 'granted' ? 'Send a test desktop notification' : 'Enable desktop notifications & send a test'}
                      className="rounded-md border border-border px-2 py-0.5 text-[11px] font-semibold text-primary hover:bg-muted">
                      {perm === 'granted' ? 'Test alert' : 'Enable alerts'}
                    </button>
                  )}
                </div>
              </div>
              <div className="max-h-[60vh] overflow-y-auto">
                {notif.items.length === 0 && (
                  <div className="px-4 py-10 text-center text-sm text-muted-foreground">
                    You&apos;re all caught up — nothing needs attention.
                  </div>
                )}
                {notif.items.map((n, i) => {
                  const I = NOTIF_ICON[n.type] ?? Bell;
                  const tone = n.severity === 'critical' ? 'text-destructive bg-destructive/10'
                    : n.severity === 'warning' ? 'text-amber-600 bg-amber-100'
                    : 'text-primary bg-primary/10';
                  return (
                    <button key={i} onClick={() => go(n.href)}
                      className="flex w-full items-start gap-3 border-b border-border/50 px-4 py-3 text-left last:border-0 hover:bg-muted">
                      <span className={`mt-0.5 flex size-8 shrink-0 items-center justify-center rounded-full ${tone}`}>
                        <I className="size-4" />
                      </span>
                      <span className="min-w-0 flex-1">
                        <span className="block truncate text-sm font-medium">{n.title}</span>
                        <span className="block truncate text-xs text-muted-foreground">{n.detail}</span>
                      </span>
                    </button>
                  );
                })}
              </div>
            </div>
          )}
        </div>

        <button onClick={() => go('/help')}
          className="rounded-lg p-2 text-muted-foreground hover:bg-muted" title="Help & support">
          <HelpCircle className="size-5" />
        </button>
      </div>

      {searchOpen && <SearchModal onClose={() => setSearchOpen(false)} onGo={go} />}

      {tip && (
        <div className="fixed right-4 top-20 z-[120] max-w-xs rounded-xl border border-border bg-card px-4 py-3 text-sm shadow-2xl">
          {tip}
        </div>
      )}
    </header>
  );
}

// ---------------------------------------------------------------------------
// Global search modal — products, customers, transactions. Debounced; each
// source is queried independently so a 403 (role-gated) one degrades quietly.
// ---------------------------------------------------------------------------
function SearchModal({ onClose, onGo }: { onClose: () => void; onGo: (href: string) => void }) {
  const [q, setQ] = useState('');
  const [hits, setHits] = useState<SearchHit[]>([]);
  const [busy, setBusy] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => { inputRef.current?.focus(); }, []);

  useEffect(() => {
    const term = q.trim();
    if (term.length < 2) { setHits([]); setBusy(false); return; }
    setBusy(true);
    const t = setTimeout(async () => {
      const enc = encodeURIComponent(term);
      const [products, customers, txns] = await Promise.all([
        apiClient<Array<{ id: string; name: string; sku: string; basePrice: number }>>(`/api/v1/products?activeOnly=true&search=${enc}`).catch(() => []),
        apiClient<Array<{ id: string; name: string; code: string; phone: string | null }>>(`/api/v1/customers?search=${enc}`).catch(() => []),
        apiClient<{ rows: Array<{ id: string; orderNumber: string; customer: string | null; totalAmount: number }> }>(`/api/v1/transactions?search=${enc}&limit=8`).catch(() => ({ rows: [] })),
      ]);
      const out: SearchHit[] = [
        ...products.slice(0, 6).map(p => ({ kind: 'product' as const, id: p.id, title: p.name, sub: `${p.sku} · LKR ${lkr(p.basePrice)}`, href: '/menu' })),
        ...customers.slice(0, 6).map(c => ({ kind: 'customer' as const, id: c.id, title: c.name, sub: [c.code, c.phone].filter(Boolean).join(' · '), href: '/customers' })),
        ...(txns.rows ?? []).slice(0, 6).map(o => ({ kind: 'transaction' as const, id: o.id, title: o.orderNumber, sub: `${o.customer ?? 'Walk-in'} · LKR ${lkr(o.totalAmount)}`, href: '/transactions' })),
      ];
      setHits(out);
      setBusy(false);
    }, 250);
    return () => clearTimeout(t);
  }, [q]);

  const KIND_ICON = { product: Package, customer: User, transaction: Receipt };
  const KIND_LABEL = { product: 'Menu item', customer: 'Customer', transaction: 'Transaction' };

  return (
    <div className="fixed inset-0 z-[100] flex items-start justify-center bg-black/60 backdrop-blur-sm p-4 pt-[12vh]" onClick={onClose}>
      <div className="w-full max-w-xl overflow-hidden rounded-xl border border-border bg-card shadow-2xl" onClick={e => e.stopPropagation()}>
        <div className="flex items-center gap-3 border-b border-border px-4 py-3">
          <Search className="size-5 shrink-0 text-muted-foreground" />
          <input ref={inputRef} value={q} onChange={e => setQ(e.target.value)}
            placeholder="Search menu items, customers, transactions…"
            className="flex-1 bg-transparent text-sm outline-none placeholder:text-muted-foreground" />
          {busy && <Loader2 className="size-4 animate-spin text-muted-foreground" />}
          <button onClick={onClose} className="rounded-md p-1 text-muted-foreground hover:bg-muted"><X className="size-4" /></button>
        </div>
        <div className="max-h-[55vh] overflow-y-auto">
          {q.trim().length < 2 && (
            <div className="px-4 py-10 text-center text-sm text-muted-foreground">Type at least 2 characters to search.</div>
          )}
          {q.trim().length >= 2 && !busy && hits.length === 0 && (
            <div className="px-4 py-10 text-center text-sm text-muted-foreground">No matches for &ldquo;{q.trim()}&rdquo;.</div>
          )}
          {hits.map(h => {
            const I = KIND_ICON[h.kind];
            return (
              <button key={`${h.kind}-${h.id}`} onClick={() => onGo(h.href)}
                className="flex w-full items-center gap-3 border-b border-border/50 px-4 py-3 text-left last:border-0 hover:bg-muted">
                <span className="flex size-8 shrink-0 items-center justify-center rounded-full bg-muted text-muted-foreground"><I className="size-4" /></span>
                <span className="min-w-0 flex-1">
                  <span className="block truncate text-sm font-medium">{h.title}</span>
                  <span className="block truncate text-xs text-muted-foreground">{h.sub}</span>
                </span>
                <span className="shrink-0 rounded-full bg-muted px-2 py-0.5 text-[10px] font-medium text-muted-foreground">{KIND_LABEL[h.kind]}</span>
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
}
