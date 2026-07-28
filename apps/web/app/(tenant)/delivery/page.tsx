'use client';

import { useEffect, useMemo, useRef, useState } from 'react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient, money } from '@/lib/api-client';
import { Icon } from '@/components/ui/Icon';
import { confirmDialog } from '@/components/ui/confirm';

/* ----------------------------- Shared types ----------------------------- */

type Location = { id: string; code: string; name: string; city: string; currency: string };

type Product = {
  id: string;
  sku: string;
  name: string;
  categoryId: string;
  basePrice: number;
  costPrice: number;
  isActive: boolean;
  isSold: boolean;
  isStocked: boolean;
};

type OrderSource = 'ubereats' | 'pickme';

type IncomingItem = { productName: string; quantity: number; notes?: string | null };

type IncomingOrder = {
  id: string;
  orderNumber: string;
  orderSource: OrderSource;
  externalOrderId: string;
  customerName: string;
  deliveryAddress: string;
  deliveryPhone: string;
  deliveryNotes?: string | null;
  totalAmount: number;
  openedAt: string;
  items: IncomingItem[];
};

type ActiveOrder = {
  id: string;
  orderNumber: string;
  orderSource: OrderSource;
  externalOrderId: string;
  status: string;
  aggregatorStatus: string;
  prepMinutes: number | null;
  promisedTime: string | null;
  customerName: string;
  deliveryAddress: string;
  totalAmount: number;
  openedAt: string;
  invoiceNumber: string | null;
};

type PickMeMenuItem = {
  id: number;
  name: string;
  refId: string | null;
  price: number | null;
  availability: string;
  category: string;
};

type OutboxRow = {
  id: string;
  aggregator: OrderSource;
  externalOrderId: string;
  operation: string;
  status: string;
  attempts: number;
  createdAt: string;
  sentAt: string | null;
};

type Tab = 'orders' | 'menu' | 'pickme' | 'outbox';

const TABS: { id: Tab; label: string; icon: string }[] = [
  { id: 'orders', label: 'Orders', icon: 'receipt_long' },
  { id: 'menu', label: 'Menu (86)', icon: 'block' },
  { id: 'pickme', label: 'PickMe sync', icon: 'sync' },
  { id: 'outbox', label: 'Outbox', icon: 'outbox' },
];

const SOURCE_LABEL: Record<OrderSource, string> = {
  ubereats: 'UBER EATS',
  pickme: 'PICKME',
};

const SOURCE_PILL: Record<OrderSource, string> = {
  ubereats: 'bg-primary-tint text-primary-dark ring-primary/20',
  pickme: 'bg-[#fff3cf] text-[#7a5b00] ring-[#caa53f]/40',
};

/** aggregatorStatus -> pill class. */
const AGG_PILL: Record<string, string> = {
  pending: 'pill-pending',
  accepted: 'pill-progress',
  preparing: 'pill-progress',
  ready: 'pill-paid',
  picked_up: 'pill-idle',
  rejected: 'pill-void',
  cancelled: 'pill-void',
};

function SourceBadge({ source }: { source: OrderSource }) {
  return (
    <span className={`pill ${SOURCE_PILL[source]} font-bold uppercase tracking-wide`}>
      {SOURCE_LABEL[source]}
    </span>
  );
}

function fmtDate(iso: string | null): string {
  if (!iso) return '—';
  return new Date(iso).toLocaleString('en-LK', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

/* ============================== Page shell ============================== */

export default function DeliveryPage() {
  const [tab, setTab] = useState<Tab>('orders');
  const [toast, setToast] = useState<string | null>(null);

  function flash(msg: string) {
    setToast(msg);
    window.setTimeout(() => setToast(null), 3500);
  }

  return (
    <>
      <Topbar title="Delivery" subtitle="Never miss a beat on your Uber Eats and PickMe orders" />

      <div className="p-6 md:p-8">
        {/* Tab strip */}
        <div className="mb-5 flex gap-1 border-b border-border">
          {TABS.map(t => (
            <button
              key={t.id}
              onClick={() => setTab(t.id)}
              className={`-mb-px flex items-center gap-1.5 border-b-2 px-4 py-2.5 text-sm font-medium transition-colors ${
                tab === t.id
                  ? 'border-primary text-primary'
                  : 'border-transparent text-muted-foreground hover:text-on-surface'
              }`}
            >
              <Icon name={t.icon} className="text-base" />
              {t.label}
            </button>
          ))}
        </div>

        {tab === 'orders' && <OrdersTab flash={flash} />}
        {tab === 'menu' && <MenuTab flash={flash} />}
        {tab === 'pickme' && <PickMeTab flash={flash} />}
        {tab === 'outbox' && <OutboxTab flash={flash} />}
      </div>

      {toast && (
        <div className="fixed bottom-12 left-1/2 z-[70] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">
          {toast}
        </div>
      )}
    </>
  );
}

/* =============================== Orders tab =============================== */

function OrdersTab({ flash }: { flash: (msg: string) => void }) {
  const [incoming, setIncoming] = useState<IncomingOrder[]>([]);
  const [active, setActive] = useState<ActiveOrder[]>([]);
  const [locations, setLocations] = useState<Location[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  // Simulate-order dev control.
  const [simSource, setSimSource] = useState<OrderSource>('ubereats');
  const [simLoc, setSimLoc] = useState('');
  const [simBusy, setSimBusy] = useState(false);

  // Per-incoming-order local form state.
  const [prepById, setPrepById] = useState<Record<string, string>>({});
  const [rejectOpenId, setRejectOpenId] = useState<string | null>(null);
  const [rejectReason, setRejectReason] = useState('');
  const [rejectError, setRejectError] = useState<string | null>(null);

  async function load() {
    try {
      const [inc, act, loc] = await Promise.all([
        apiClient<IncomingOrder[]>('/api/v1/aggregator/incoming'),
        apiClient<ActiveOrder[]>('/api/v1/aggregator/orders'),
        apiClient<Location[]>('/api/v1/locations'),
      ]);
      setIncoming(inc);
      setActive(act);
      setLocations(loc);
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  // Real-time: a new aggregator order (or a settle/void) pushes an "orders"
  // signal on the app-wide SSE stream — reload the queue instantly instead of
  // making the cashier refresh. fetch keeps the JWT on the header; reconnects
  // with backoff. (Mirrors the KOT board + notification bell.)
  const loadRef = useRef(load); loadRef.current = load;
  useEffect(() => {
    let stop = false; let ctrl: AbortController | null = null; let backoff = 1000;
    let debounce: ReturnType<typeof setTimeout> | null = null;
    const LIVE = new Set(['orders', 'aggregator', 'notifications']);
    const bump = () => { if (debounce) clearTimeout(debounce); debounce = setTimeout(() => loadRef.current(), 400); };
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
        } catch { /* reconnect */ }
        if (stop) break;
        await new Promise(r => setTimeout(r, backoff));
        backoff = Math.min(backoff * 2, 15000);
      }
    }
    void connect();
    return () => { stop = true; ctrl?.abort(); if (debounce) clearTimeout(debounce); };
  }, []);

  const prepFor = (id: string) => prepById[id] ?? '20';

  async function simulate() {
    setSimBusy(true);
    try {
      const body: { locationId?: string; seed?: number } = {};
      if (simLoc) body.locationId = simLoc;
      await apiClient(`/api/v1/aggregator/${simSource}/simulate`, {
        method: 'POST',
        body: JSON.stringify(body),
      });
      flash(`Injected a test ${SOURCE_LABEL[simSource]} order.`);
      await load();
    } catch (e) {
      flash(extractError(e, 'Could not inject test order.'));
    } finally {
      setSimBusy(false);
    }
  }

  async function accept(o: IncomingOrder) {
    const raw = prepFor(o.id).trim();
    const prep = Number(raw);
    if (raw === '' || Number.isNaN(prep) || prep <= 0) {
      flash('Prep time must be greater than zero.');
      return;
    }
    setBusyId(o.id);
    try {
      await apiClient(`/api/v1/aggregator/orders/${o.id}/accept`, {
        method: 'POST',
        body: JSON.stringify({ prepMinutes: prep }),
      });
      flash(`Accepted ${o.orderNumber} — ${prep} min prep.`);
      await load();
    } catch (e) {
      flash(extractError(e, 'Could not accept order.'));
    } finally {
      setBusyId(null);
    }
  }

  async function reject(o: IncomingOrder) {
    const reason = rejectReason.trim();
    if (!reason) {
      setRejectError('A reason is required to reject.');
      return;
    }
    if (!(await confirmDialog({
      title: `Reject ${o.orderNumber}?`,
      body: 'The customer’s order will be rejected on the aggregator. This cannot be undone.',
      confirmLabel: 'Reject order',
      danger: true,
    }))) return;
    setBusyId(o.id);
    try {
      await apiClient(`/api/v1/aggregator/orders/${o.id}/reject`, {
        method: 'POST',
        body: JSON.stringify({ reason }),
      });
      flash(`Rejected ${o.orderNumber}.`);
      setRejectOpenId(null);
      setRejectReason('');
      setRejectError(null);
      await load();
    } catch (e) {
      flash(extractError(e, 'Could not reject order.'));
    } finally {
      setBusyId(null);
    }
  }

  async function advance(o: ActiveOrder, action: 'ready' | 'pickup') {
    setBusyId(o.id);
    try {
      await apiClient(`/api/v1/aggregator/orders/${o.id}/${action}`, { method: 'POST' });
      flash(action === 'ready' ? `${o.orderNumber} marked ready.` : `${o.orderNumber} picked up.`);
      await load();
    } catch (e) {
      flash(extractError(e, `Could not mark order ${action}.`));
    } finally {
      setBusyId(null);
    }
  }

  if (loading) {
    return (
      <div className="space-y-2">
        {Array.from({ length: 6 }).map((_, i) => (
          <div key={i} className="h-16 animate-pulse rounded bg-muted" />
        ))}
      </div>
    );
  }

  if (error) {
    return <div className="card p-6 text-sm text-status-error">{error}</div>;
  }

  return (
    <div className="space-y-8">
      {/* Incoming */}
      <section>
        <div className="mb-4 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Incoming</h2>
            <p className="text-sm text-muted-foreground">
              {incoming.length} order{incoming.length === 1 ? '' : 's'} pending accept / reject
            </p>
          </div>
        </div>

        {/* Simulate dev control */}
        <div className="card mb-4 flex flex-wrap items-end gap-3 p-4">
          <div>
            <label className="mb-1 block text-xs font-semibold text-slate-700">Aggregator</label>
            <select
              value={simSource}
              onChange={e => setSimSource(e.target.value as OrderSource)}
              className="rounded-lg border border-border bg-surface px-3 py-2 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20"
            >
              <option value="ubereats">Uber Eats</option>
              <option value="pickme">PickMe</option>
            </select>
          </div>
          <div>
            <label className="mb-1 block text-xs font-semibold text-slate-700">
              Location (optional)
            </label>
            <select
              value={simLoc}
              onChange={e => setSimLoc(e.target.value)}
              className="rounded-lg border border-border bg-surface px-3 py-2 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20"
            >
              <option value="">Any location</option>
              {locations.map(l => (
                <option key={l.id} value={l.id}>
                  {l.code} — {l.name}
                </option>
              ))}
            </select>
          </div>
          <button
            onClick={simulate}
            disabled={simBusy}
            className="inline-flex items-center gap-1.5 rounded-lg border border-border bg-card px-3 py-2 text-xs font-medium hover:bg-muted disabled:opacity-50"
          >
            <Icon name="science" className="text-sm" />
            {simBusy ? 'Injecting…' : 'Inject test order'}
          </button>
          <span className="ml-auto text-xs text-muted-foreground">
            Dev only — injects a synthetic order into the queue.
          </span>
        </div>

        {incoming.length === 0 ? (
          <div className="card p-10 text-center text-sm text-muted-foreground">
            No pending orders. New Uber Eats &amp; PickMe orders land here.
          </div>
        ) : (
          <div className="grid gap-4 lg:grid-cols-2">
            {incoming.map(o => (
              <div key={o.id} className="card overflow-hidden border-l-4 border-l-primary p-5">
                <div className="mb-3 flex items-start justify-between gap-2">
                  <div className="flex items-center gap-2">
                    <SourceBadge source={o.orderSource} />
                    <span className="font-mono text-sm font-semibold">{o.orderNumber}</span>
                  </div>
                  <span className="rounded-lg bg-secondary-container/40 px-2.5 py-1 text-right text-sm font-bold tabular-nums text-on-surface">
                    {money(o.totalAmount)}
                  </span>
                </div>

                <div className="mb-3 space-y-0.5 text-sm">
                  <p className="font-medium">{o.customerName}</p>
                  <p className="text-muted-foreground">{o.deliveryAddress}</p>
                  <p className="text-muted-foreground">{o.deliveryPhone}</p>
                  {o.deliveryNotes && (
                    <p className="text-xs italic text-muted-foreground">“{o.deliveryNotes}”</p>
                  )}
                </div>

                <ul className="mb-3 space-y-1 rounded-lg bg-muted/30 p-3 text-sm">
                  {o.items.map((it, i) => (
                    <li key={i} className="flex gap-2">
                      <span className="font-semibold tabular-nums">{it.quantity}×</span>
                      <span>
                        {it.productName}
                        {it.notes && (
                          <span className="text-muted-foreground"> ({it.notes})</span>
                        )}
                      </span>
                    </li>
                  ))}
                </ul>

                {/* Actions */}
                <div className="flex flex-wrap items-end gap-2">
                  <div>
                    <label className="mb-1 block text-xs font-semibold text-slate-700">
                      Prep (min)
                    </label>
                    <input
                      value={prepFor(o.id)}
                      onChange={e =>
                        setPrepById(m => ({
                          ...m,
                          [o.id]: e.target.value.replace(/[^0-9]/g, ''),
                        }))
                      }
                      inputMode="numeric"
                      className="w-20 rounded-lg border border-border bg-surface px-3 py-2 text-sm tabular-nums focus:border-primary focus:ring-2 focus:ring-primary/20"
                    />
                  </div>
                  <button
                    onClick={() => accept(o)}
                    disabled={busyId === o.id}
                    className="inline-flex items-center gap-1.5 rounded-lg bg-[#ffc329] px-5 py-2 text-sm font-bold text-on-surface shadow-sm transition-[filter] hover:brightness-95 disabled:opacity-50"
                  >
                    <Icon name="check" className="text-sm" /> Accept
                  </button>
                  <button
                    onClick={() => {
                      setRejectOpenId(rejectOpenId === o.id ? null : o.id);
                      setRejectReason('');
                      setRejectError(null);
                    }}
                    disabled={busyId === o.id}
                    className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted disabled:opacity-50"
                  >
                    Reject
                  </button>
                </div>

                {rejectOpenId === o.id && (
                  <div className="mt-3 border-t border-border pt-3">
                    <label className="mb-1 block text-xs font-semibold text-slate-700">
                      Reject reason
                    </label>
                    <div className="flex gap-2">
                      <input
                        value={rejectReason}
                        onChange={e => {
                          setRejectReason(e.target.value);
                          if (rejectError) setRejectError(null);
                        }}
                        placeholder="e.g. Out of stock"
                        className="flex-1 rounded-lg border border-border bg-surface px-3 py-2 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20"
                      />
                      <button
                        onClick={() => reject(o)}
                        disabled={busyId === o.id}
                        className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted disabled:opacity-50"
                      >
                        Confirm reject
                      </button>
                    </div>
                    {rejectError && (
                      <p className="mt-1 text-xs text-status-error">{rejectError}</p>
                    )}
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </section>

      {/* Active orders */}
      <section>
        <div className="mb-4">
          <h2 className="font-heading text-xl font-bold">Active orders</h2>
          <p className="text-sm text-muted-foreground">
            {active.length} accepted &amp; in-flight order{active.length === 1 ? '' : 's'}
          </p>
        </div>

        <div className="card overflow-hidden">
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-3 font-medium">Order #</th>
                <th className="px-4 py-3 font-medium">Source</th>
                <th className="px-4 py-3 font-medium">Customer</th>
                <th className="px-4 py-3 font-medium">Status</th>
                <th className="px-4 py-3 text-right font-medium">Prep (min)</th>
                <th className="px-4 py-3 text-right font-medium">Total</th>
                <th className="px-4 py-3 text-right font-medium">Action</th>
              </tr>
            </thead>
            <tbody>
              {active.map((o, i) => (
                <tr key={o.id} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="px-4 py-3 font-mono text-xs">{o.orderNumber}</td>
                  <td className="px-4 py-3">
                    <SourceBadge source={o.orderSource} />
                  </td>
                  <td className="px-4 py-3">{o.customerName}</td>
                  <td className="px-4 py-3">
                    <span className={`pill ${AGG_PILL[o.aggregatorStatus] ?? 'pill-idle'}`}>
                      {o.aggregatorStatus}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-right tabular-nums text-muted-foreground">
                    {o.prepMinutes ?? '—'}
                  </td>
                  <td className="px-4 py-3 text-right font-semibold tabular-nums">
                    {money(o.totalAmount)}
                  </td>
                  <td className="px-4 py-3 text-right">
                    {(o.aggregatorStatus === 'preparing' ||
                      o.aggregatorStatus === 'accepted') && (
                      <button
                        disabled={busyId === o.id}
                        onClick={() => advance(o, 'ready')}
                        className="inline-flex items-center gap-1 rounded-lg bg-primary px-3 py-1.5 text-xs font-medium text-primary-foreground hover:bg-primary-dark disabled:opacity-50"
                      >
                        <Icon name="restaurant" className="text-sm" /> Mark ready
                      </button>
                    )}
                    {o.aggregatorStatus === 'ready' && (
                      <button
                        disabled={busyId === o.id}
                        onClick={() => advance(o, 'pickup')}
                        className="inline-flex items-center gap-1 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted disabled:opacity-50"
                      >
                        <Icon name="moped" className="text-sm" /> Mark picked up
                      </button>
                    )}
                    {o.aggregatorStatus !== 'preparing' &&
                      o.aggregatorStatus !== 'accepted' &&
                      o.aggregatorStatus !== 'ready' && (
                        <span className="text-muted-foreground">—</span>
                      )}
                  </td>
                </tr>
              ))}
              {active.length === 0 && (
                <tr>
                  <td colSpan={7} className="px-4 py-10 text-center text-muted-foreground">
                    No active orders. Accept an incoming order to start prep.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  );
}

/* ============================== Menu (86) tab ============================== */

function MenuTab({ flash }: { flash: (msg: string) => void }) {
  const [products, setProducts] = useState<Product[]>([]);
  const [available, setAvailable] = useState<Record<string, boolean>>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  async function load() {
    try {
      const p = await apiClient<Product[]>('/api/v1/products');
      setProducts(p);
      // Local boolean per product initialized to true (available).
      setAvailable(Object.fromEntries(p.map(prod => [prod.id, true])));
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  async function toggle(prod: Product) {
    const next = !(available[prod.id] ?? true);
    // Optimistic.
    setAvailable(m => ({ ...m, [prod.id]: next }));
    setBusyId(prod.id);
    try {
      await apiClient<{ ok: true }>('/api/v1/aggregator/availability', {
        method: 'POST',
        body: JSON.stringify({ productId: prod.id, available: next }),
      });
      flash(
        `Marked ${prod.name} ${next ? 'available' : 'unavailable (86)'} — syncing to aggregators`,
      );
    } catch (e) {
      // Roll back optimistic change on failure.
      setAvailable(m => ({ ...m, [prod.id]: !next }));
      flash(extractError(e, 'Could not update availability.'));
    } finally {
      setBusyId(null);
    }
  }

  if (loading) {
    return (
      <div className="space-y-2">
        {Array.from({ length: 8 }).map((_, i) => (
          <div key={i} className="h-9 animate-pulse rounded bg-muted" />
        ))}
      </div>
    );
  }

  if (error) {
    return <div className="card p-6 text-sm text-status-error">{error}</div>;
  }

  return (
    <div>
      <div className="mb-4">
        <h2 className="font-heading text-xl font-bold">Menu availability (86)</h2>
        <p className="text-sm text-muted-foreground">
          Flip an item off to “86” it — this pushes to your Uber Eats &amp; PickMe menus.
        </p>
      </div>

      <div className="card overflow-hidden">
        <table className="w-full text-sm">
          <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
            <tr>
              <th className="px-4 py-3 font-medium">Name</th>
              <th className="px-4 py-3 font-medium">Product Code</th>
              <th className="px-4 py-3 text-right font-medium">Available on aggregators</th>
            </tr>
          </thead>
          <tbody>
            {products.map((p, i) => {
              const isOn = available[p.id] ?? true;
              return (
                <tr key={p.id} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="px-4 py-3 font-medium">{p.name}</td>
                  <td className="px-4 py-3 font-mono text-xs text-muted-foreground">{p.sku}</td>
                  <td className="px-4 py-3 text-right">
                    <label className="inline-flex cursor-pointer items-center gap-2">
                      <span
                        className={`text-xs font-medium ${
                          isOn ? 'text-muted-foreground' : 'text-status-error'
                        }`}
                      >
                        {isOn ? 'Available' : 'Unavailable (86)'}
                      </span>
                      <span
                        className={`relative inline-flex h-5 w-9 items-center rounded-full transition-colors ${
                          isOn ? 'bg-primary' : 'bg-muted'
                        } ${busyId === p.id ? 'opacity-50' : ''}`}
                      >
                        <input
                          type="checkbox"
                          checked={isOn}
                          disabled={busyId === p.id}
                          onChange={() => toggle(p)}
                          className="sr-only"
                        />
                        <span
                          className={`inline-block size-4 transform rounded-full bg-white shadow transition-transform ${
                            isOn ? 'translate-x-4' : 'translate-x-1'
                          }`}
                        />
                      </span>
                    </label>
                  </td>
                </tr>
              );
            })}
            {products.length === 0 && (
              <tr>
                <td colSpan={3} className="px-4 py-10 text-center text-muted-foreground">
                  No products to publish yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      <p className="mt-3 text-xs text-muted-foreground">
        Toggling an item updates its availability on Uber Eats &amp; PickMe in near real time.
      </p>
    </div>
  );
}

/* ============================== PickMe sync tab ============================== */

function PickMeTab({ flash }: { flash: (msg: string) => void }) {
  const [locations, setLocations] = useState<Location[]>([]);
  const [loc, setLoc] = useState('');
  const [menu, setMenu] = useState<PickMeMenuItem[] | null>(null);
  const [outletName, setOutletName] = useState<string | null>(null);
  const [skus, setSkus] = useState<Map<string, string>>(new Map()); // sku -> our product name
  const [loading, setLoading] = useState(false);
  const [polling, setPolling] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    apiClient<Location[]>('/api/v1/locations').then(setLocations).catch(() => {});
    apiClient<Product[]>('/api/v1/products')
      .then(ps => setSkus(new Map(ps.map(p => [p.sku, p.name]))))
      .catch(() => {});
  }, []);

  async function loadMenu() {
    setLoading(true);
    setError(null);
    try {
      const m = await apiClient<{ params: { outletName?: string }; data: PickMeMenuItem[] }>(
        `/api/v1/aggregator/pickme/menu${loc ? `?locationId=${loc}` : ''}`,
      );
      setMenu(m.data ?? []);
      setOutletName(m.params?.outletName ?? null);
    } catch (e) {
      setError(extractError(e, 'Could not fetch the PickMe menu.'));
      setMenu(null);
    } finally {
      setLoading(false);
    }
  }

  async function pollNow() {
    setPolling(true);
    try {
      const r = await apiClient<{ ingested: number }>('/api/v1/aggregator/pickme/poll', { method: 'POST' });
      flash(`Polled PickMe — ${r.ingested} new order${r.ingested === 1 ? '' : 's'} ingested.`);
    } catch (e) {
      flash(extractError(e, 'Poll failed.'));
    } finally {
      setPolling(false);
    }
  }

  const matched = menu?.filter(m => m.refId && skus.has(m.refId)).length ?? 0;

  return (
    <div className="space-y-6">
      <div>
        <h2 className="font-heading text-xl font-bold">PickMe sync</h2>
        <p className="text-sm text-muted-foreground">
          Pull orders on demand and check that each PickMe item&apos;s <span className="font-mono">Ref ID</span> matches a POS Product Code
          (that&apos;s how incoming orders map to your products). Set credentials &amp; per-outlet keys under Settings → Delivery integrations.
        </p>
      </div>

      <div className="card flex flex-wrap items-end gap-3 p-4">
        <div>
          <label className="mb-1 block text-xs font-semibold text-slate-700">Outlet</label>
          <select
            value={loc}
            onChange={e => setLoc(e.target.value)}
            className="rounded-lg border border-border bg-surface px-3 py-2 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20"
          >
            <option value="">First keyed outlet</option>
            {locations.map(l => (
              <option key={l.id} value={l.id}>
                {l.code} — {l.name}
              </option>
            ))}
          </select>
        </div>
        <button
          onClick={loadMenu}
          disabled={loading}
          className="inline-flex items-center gap-1.5 rounded-lg border border-border bg-card px-3 py-2 text-sm font-medium hover:bg-muted disabled:opacity-50"
        >
          <Icon name="restaurant_menu" className="text-sm" />
          {loading ? 'Loading…' : 'Load PickMe menu'}
        </button>
        <button
          onClick={pollNow}
          disabled={polling}
          className="inline-flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark disabled:opacity-50"
        >
          <Icon name="sync" className="text-sm" />
          {polling ? 'Polling…' : 'Poll orders now'}
        </button>
        <span className="ml-auto text-xs text-muted-foreground">
          Orders normally poll automatically in the background.
        </span>
      </div>

      {error && <div className="card p-4 text-sm text-status-error">{error}</div>}

      {menu && (
        <div>
          <div className="mb-2 flex items-center justify-between">
            <h3 className="font-heading text-base font-semibold">
              {outletName ?? 'PickMe menu'} — {menu.length} item{menu.length === 1 ? '' : 's'}
            </h3>
            <span className="text-xs text-muted-foreground">
              {matched}/{menu.length} mapped to a POS Product Code
            </span>
          </div>
          <div className="card overflow-hidden">
            <table className="w-full text-sm">
              <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th className="px-4 py-3 font-medium">PickMe item</th>
                  <th className="px-4 py-3 font-medium">Ref ID</th>
                  <th className="px-4 py-3 text-right font-medium">Price</th>
                  <th className="px-4 py-3 font-medium">PickMe status</th>
                  <th className="px-4 py-3 font-medium">Maps to POS</th>
                </tr>
              </thead>
              <tbody>
                {menu.map((m, i) => {
                  const ourName = m.refId ? skus.get(m.refId) : undefined;
                  return (
                    <tr key={m.id} className={i % 2 ? 'bg-muted/20' : ''}>
                      <td className="px-4 py-3 font-medium">{m.name}</td>
                      <td className="px-4 py-3 font-mono text-xs text-muted-foreground">{m.refId || '—'}</td>
                      <td className="px-4 py-3 text-right tabular-nums">{m.price != null ? money(m.price) : '—'}</td>
                      <td className="px-4 py-3">
                        <span className="text-xs text-muted-foreground">{m.availability}</span>
                      </td>
                      <td className="px-4 py-3">
                        {ourName ? (
                          <span className="inline-flex items-center gap-1 text-xs font-medium text-primary">
                            <Icon name="check_circle" className="text-sm" /> {ourName}
                          </span>
                        ) : (
                          <span className="inline-flex items-center gap-1 text-xs text-status-error">
                            <Icon name="error" className="text-sm" />
                            {m.refId ? `No Product Code "${m.refId}"` : 'No Ref ID set'}
                          </span>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
          <p className="mt-3 text-xs text-muted-foreground">
            Unmapped items still come through as custom lines — but set the PickMe Ref ID to the matching POS Product Code so totals
            and stock track correctly.
          </p>
        </div>
      )}
    </div>
  );
}

/* =============================== Outbox tab =============================== */

function OutboxTab({ flash }: { flash: (msg: string) => void }) {
  const [rows, setRows] = useState<OutboxRow[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [processing, setProcessing] = useState(false);

  const OUTBOX_PILL: Record<string, string> = useMemo(
    () => ({ sent: 'pill-paid', pending: 'pill-pending', failed: 'pill-void' }),
    [],
  );

  async function load() {
    try {
      const o = await apiClient<OutboxRow[]>('/api/v1/aggregator/outbox');
      setRows(o);
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  async function process() {
    setProcessing(true);
    try {
      const res = await apiClient<{ sent: number }>('/api/v1/aggregator/outbox/process', {
        method: 'POST',
      });
      flash(`Sent ${res.sent}.`);
      await load();
    } catch (e) {
      flash(extractError(e, 'Could not process outbox.'));
    } finally {
      setProcessing(false);
    }
  }

  if (loading) {
    return (
      <div className="space-y-2">
        {Array.from({ length: 8 }).map((_, i) => (
          <div key={i} className="h-9 animate-pulse rounded bg-muted" />
        ))}
      </div>
    );
  }

  if (error) {
    return <div className="card p-6 text-sm text-status-error">{error}</div>;
  }

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <div>
          <h2 className="font-heading text-xl font-bold">Outbox</h2>
          <p className="text-sm text-muted-foreground">
            Outbound messages queued to Uber Eats &amp; PickMe.
          </p>
        </div>
        <button
          onClick={process}
          disabled={processing}
          className="inline-flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark disabled:opacity-50"
        >
          <Icon name="send" className="text-sm" />
          {processing ? 'Processing…' : 'Process pending'}
        </button>
      </div>

      <div className="card overflow-hidden">
        <table className="w-full text-sm">
          <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
            <tr>
              <th className="px-4 py-3 font-medium">Created</th>
              <th className="px-4 py-3 font-medium">Aggregator</th>
              <th className="px-4 py-3 font-medium">External order id</th>
              <th className="px-4 py-3 font-medium">Operation</th>
              <th className="px-4 py-3 text-right font-medium">Attempts</th>
              <th className="px-4 py-3 font-medium">Status</th>
              <th className="px-4 py-3 font-medium">Sent at</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((r, i) => (
              <tr key={r.id} className={i % 2 ? 'bg-muted/20' : ''}>
                <td className="px-4 py-3 text-muted-foreground">{fmtDate(r.createdAt)}</td>
                <td className="px-4 py-3">
                  <SourceBadge source={r.aggregator} />
                </td>
                <td className="px-4 py-3 font-mono text-xs">{r.externalOrderId}</td>
                <td className="px-4 py-3">{r.operation}</td>
                <td className="px-4 py-3 text-right tabular-nums">{r.attempts}</td>
                <td className="px-4 py-3">
                  <span className={`pill ${OUTBOX_PILL[r.status] ?? 'pill-idle'}`}>
                    {r.status}
                  </span>
                </td>
                <td className="px-4 py-3 text-muted-foreground">{fmtDate(r.sentAt)}</td>
              </tr>
            ))}
            {rows.length === 0 && (
              <tr>
                <td colSpan={7} className="px-4 py-10 text-center text-muted-foreground">
                  Outbox is empty. Order events queue here before delivery to aggregators.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
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
