'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { Topbar } from '@/components/app-shell/Topbar';
import { Icon } from '@/components/ui/Icon';
import { apiClient, money } from '@/lib/api-client';
import { useTaxLabel } from '@/lib/use-tax-label';

type Order = {
  id: string;
  orderNumber: string;
  status: string;
  tableLabel: string | null;
  covers: number | null;
  totalAmount: number;
};
type Notif = { type: string };
type NotifResp = { count: number; items: Notif[] };
type Summary = { grossSales: number; orderCount: number };
type Activity = { id: string; at: string; actorName: string | null; actorRole: string | null; action: string; summary: string | null };

const ymdLocal = () => {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
};
const fmtTime = (iso: string) => new Date(iso).toLocaleTimeString('en-LK', { hour: '2-digit', minute: '2-digit' });

// Status → design-system pill class for live order rows.
const ORDER_PILL: Record<string, string> = {
  open: 'pill-idle', confirmed: 'pill-pending', preparing: 'pill-pending',
  ready: 'pill-paid', settled: 'pill-paid', void: 'pill-void',
};
// Coloured icon-chip per metric.
const ACCENT: Record<string, string> = {
  green: 'bg-primary/10 text-primary',
  blue: 'bg-blue-50 text-blue-700',
  amber: 'bg-amber-50 text-amber-700',
  indigo: 'bg-indigo-50 text-indigo-700',
};

export default function DashboardPage() {
  const [orders, setOrders] = useState<Order[] | null>(null);
  const [lowStock, setLowStock] = useState<number | null>(null);
  const [delivery, setDelivery] = useState<number>(0);
  const [revenue, setRevenue] = useState<Summary | null>(null);
  const [yRevenue, setYRevenue] = useState<number | null>(null);
  const [activity, setActivity] = useState<Activity[] | null>(null);
  const [greeting, setGreeting] = useState('Welcome');
  const [firstName, setFirstName] = useState('');
  const [today, setToday] = useState('');
  const [hasFloor, setHasFloor] = useState(true);   // dine-in floor only exists at real outlets
  const taxLabel = useTaxLabel();

  useEffect(() => {
    try { const l = localStorage.getItem('hms.location'); if (l) { const lt = JSON.parse(l).locationType; if (lt) setHasFloor(lt === 'outlet'); } } catch { /* ignore */ }
    const h = new Date().getHours();
    setGreeting(h < 12 ? 'Good morning' : h < 17 ? 'Good afternoon' : 'Good evening');
    setToday(new Date().toLocaleDateString('en-LK', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }));
    try { const u = localStorage.getItem('hms.user'); if (u) setFirstName((JSON.parse(u).displayName ?? '').split(' ')[0] ?? ''); } catch { /* ignore */ }

    apiClient<Order[]>('/api/v1/orders/open').then(setOrders).catch(() => setOrders([]));
    apiClient<NotifResp>('/api/v1/notifications').then(n => {
      setLowStock(n.items.filter(i => i.type === 'low_stock').length);
      setDelivery(n.items.filter(i => i.type === 'aggregator').length);
    }).catch(() => setLowStock(null));
    const d = ymdLocal();
    apiClient<Summary>(`/api/v1/reports/sales/summary?from=${d}&to=${d}`).then(setRevenue).catch(() => setRevenue(null));
    const y = new Date(Date.now() - 86400000);
    const yd = `${y.getFullYear()}-${String(y.getMonth() + 1).padStart(2, '0')}-${String(y.getDate()).padStart(2, '0')}`;
    apiClient<Summary>(`/api/v1/reports/sales/summary?from=${yd}&to=${yd}`).then(s => setYRevenue(s.grossSales)).catch(() => setYRevenue(null));
    apiClient<Activity[]>('/api/v1/audit?take=6').then(setActivity).catch(() => setActivity([]));
  }, []);

  const openCount = orders?.length ?? 0;
  const revTrend = revenue && yRevenue != null && yRevenue > 0 ? Math.round(((revenue.grossSales - yRevenue) / yRevenue) * 100) : null;

  return (
    <>
      <Topbar title="Dashboard" subtitle="A clear view of today's sales, orders and anything that needs you" />

      <main className="mx-auto max-w-[1280px] p-6 md:p-8">
        {/* Greeting hero */}
        <div className="mb-7 flex flex-wrap items-center justify-between gap-4">
          <div>
            <h1 className="font-heading text-2xl font-extrabold text-on-surface">
              {greeting}{firstName ? `, ${firstName}` : ''} 👋
            </h1>
            <div className="mt-1.5 flex flex-wrap items-center gap-x-3 gap-y-1.5 text-sm">
              <span className="text-muted-foreground">{today}</span>
              {orders != null && (
                <span className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-xs font-semibold ${openCount > 0 ? 'bg-primary/10 text-primary' : 'bg-muted text-muted-foreground'}`}>
                  <span className={`size-1.5 rounded-full ${openCount > 0 ? 'bg-primary' : 'bg-slate-400'}`} />
                  {openCount > 0 ? `${openCount} open order${openCount === 1 ? '' : 's'}` : 'No open orders'}
                </span>
              )}
            </div>
          </div>
          <Link href="/pos"
            className="flex items-center gap-2 rounded-lg bg-primary px-5 py-2.5 font-bold text-primary-foreground shadow-sm transition-all hover:bg-primary-dark active:scale-95">
            <Icon name="point_of_sale" className="text-base" /> New order
          </Link>
        </div>

        {/* KPI row */}
        <div className="mb-8 grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
          <Stat accent="green" icon="payments" label="Revenue today" href="/reports" trend={revTrend}
            value={revenue ? `${money(revenue.grossSales)}` : '—'}
            sub={revenue ? `${revenue.orderCount} bill${revenue.orderCount === 1 ? '' : 's'} settled${revTrend != null ? ' · vs yesterday' : ''}` : 'Updates as bills settle'} />

          <Stat accent="blue" icon="receipt_long" label="Open orders" href={hasFloor ? '/floor' : '/pos'}
            value={orders == null ? '—' : String(openCount)}
            sub={hasFloor ? 'Dine-in & takeaway in progress' : 'Counter orders in progress'} />

          <Stat accent="amber" icon="inventory_2" label="Low stock" href="/inventory" alert={(lowStock ?? 0) > 0}
            value={lowStock == null ? '—' : lowStock > 0 ? `${lowStock} item${lowStock === 1 ? '' : 's'}` : '0 items'}
            sub={lowStock == null ? 'Checking levels…' : lowStock > 0 ? 'At or below reorder level' : 'All above reorder level'} />

          <Stat accent="indigo" icon="delivery_dining" label="Delivery orders" href="/delivery"
            value={delivery > 0 ? String(delivery) : '—'}
            sub={delivery > 0 ? 'Awaiting acceptance' : 'Connect Uber Eats / PickMe'} />
        </div>

        {/* Main grid */}
        <div className="mb-8 grid grid-cols-1 gap-6 lg:grid-cols-3">
          {/* Live orders */}
          <section className="flex min-h-[420px] flex-col overflow-hidden rounded-xl border border-border bg-card lg:col-span-2">
            <header className="flex items-center justify-between border-b border-border px-5 py-4">
              <div className="flex items-center gap-2">
                <span className="flex size-7 items-center justify-center rounded-md bg-blue-50 text-blue-700"><Icon name="receipt_long" className="text-base" /></span>
                <h2 className="font-heading font-bold text-on-surface">Live Orders</h2>
                {openCount > 0 && <span className="pill pill-progress">{openCount}</span>}
              </div>
              <Link href={hasFloor ? '/floor' : '/pos'} className="text-xs font-semibold text-primary hover:underline">{hasFloor ? 'View floor →' : 'Open POS →'}</Link>
            </header>

            {orders == null ? (
              <div className="flex-1 divide-y divide-border">
                {[0, 1, 2].map(i => <div key={i} className="h-[68px] animate-pulse bg-muted/40" />)}
              </div>
            ) : openCount === 0 ? (
              <div className="flex flex-1 flex-col items-center justify-center p-10 text-center">
                <span className="mb-4 flex size-16 items-center justify-center rounded-full bg-muted text-muted-foreground"><Icon name="ramen_dining" className="!text-3xl" /></span>
                <h3 className="font-heading text-base font-bold">No open orders</h3>
                <p className="mb-6 mt-1 max-w-xs text-sm text-muted-foreground">Nothing in progress right now. Open the POS to take your first order.</p>
                <Link href="/pos" className="flex items-center gap-2 rounded-lg bg-primary px-5 py-2.5 font-bold text-primary-foreground transition-all hover:bg-primary-dark active:scale-95">
                  Open POS <Icon name="arrow_forward" className="text-sm" />
                </Link>
              </div>
            ) : (
              <div className="divide-y divide-border">
                {orders.map(o => (
                  <Link key={o.id} href={`/pos?order=${o.id}`} className="flex items-center gap-3 px-5 py-3 transition-colors hover:bg-muted/40">
                    <span className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-primary/10 font-heading text-sm font-bold text-primary">
                      {o.tableLabel ? o.tableLabel.replace(/[^0-9A-Za-z]/g, '').slice(0, 3) || 'T' : '#'}
                    </span>
                    <div className="min-w-0 flex-1">
                      <p className="truncate font-heading font-semibold text-on-surface">{o.tableLabel ? `Table ${o.tableLabel}` : o.orderNumber}</p>
                      <p className="truncate text-xs text-muted-foreground">{o.orderNumber} · {o.covers ?? 0} cover{o.covers === 1 ? '' : 's'}</p>
                    </div>
                    <span className="font-heading font-bold text-on-surface tabular-nums">{money(o.totalAmount)}</span>
                    <span className={`pill ${ORDER_PILL[o.status] ?? 'pill-idle'} capitalize`}>{o.status}</span>
                  </Link>
                ))}
              </div>
            )}
          </section>

          {/* Recent activity */}
          <section className="flex flex-col overflow-hidden rounded-xl border border-border bg-card">
            <header className="flex items-center justify-between border-b border-border px-5 py-4">
              <div className="flex items-center gap-2">
                <span className="flex size-7 items-center justify-center rounded-md bg-muted text-muted-foreground"><Icon name="history" className="text-base" /></span>
                <h2 className="font-heading font-bold text-on-surface">Recent Activity</h2>
              </div>
              {activity != null && activity.length > 0 && <Link href="/audit" className="text-xs font-semibold text-primary hover:underline">All →</Link>}
            </header>
            <div className="flex-1">
              {activity != null && activity.length > 0 ? (
                <ol className="divide-y divide-border">
                  {activity.map(a => (
                    <li key={a.id} className="flex gap-3 px-5 py-3">
                      <span className="mt-1.5 size-2 shrink-0 rounded-full bg-primary/60" />
                      <div className="min-w-0 flex-1">
                        <p className="text-sm font-medium leading-snug text-on-surface">{a.summary || a.action}</p>
                        <p className="mt-0.5 text-xs text-muted-foreground">
                          {fmtTime(a.at)}{a.actorName ? ` · ${a.actorName}` : ''}
                        </p>
                      </div>
                    </li>
                  ))}
                </ol>
              ) : (
                <div className="flex h-full flex-col items-center justify-center p-8 text-center">
                  <span className="mb-3 flex size-12 items-center justify-center rounded-full bg-muted text-muted-foreground"><Icon name="history" className="!text-2xl" /></span>
                  <p className="text-sm font-semibold text-on-surface">No activity yet</p>
                  <p className="mt-1 max-w-[15rem] text-xs text-muted-foreground">Sign-ins, settled bills, voids and stock changes will show up here.</p>
                </div>
              )}
            </div>
          </section>
        </div>

        {/* Quick actions */}
        <h2 className="mb-4 font-heading text-sm font-bold uppercase tracking-wider text-muted-foreground">Quick Actions</h2>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <QuickLink href="/pos" icon="point_of_sale" accent="green" label="Take an order" sub="Open the POS terminal" />
          <QuickLink href="/menu" icon="add_box" accent="blue" label="Add a product" sub="Create a menu item" />
          <QuickLink href="/reports" icon="summarize" accent="amber" label="View sales report" sub={`Today's takings & ${taxLabel}`} />
          <QuickLink href="/team" icon="badge" accent="indigo" label="Manage team" sub="Staff, roles & PINs" />
        </div>
      </main>
    </>
  );
}

function Stat({ icon, label, value, sub, accent, href, alert, trend }: {
  icon: string; label: string; value: string; sub: string; accent: string; href?: string; alert?: boolean; trend?: number | null;
}) {
  const body = (
    <>
      <div className="mb-3 flex items-start justify-between">
        <span className={`flex size-10 items-center justify-center rounded-lg ${ACCENT[accent]}`}><Icon name={icon} /></span>
        {trend != null ? (
          <span className={`inline-flex items-center gap-0.5 rounded-full px-1.5 py-0.5 text-[11px] font-bold ${trend >= 0 ? 'bg-green-50 text-green-700' : 'bg-red-50 text-red-700'}`} title="vs yesterday">
            <Icon name={trend >= 0 ? 'trending_up' : 'trending_down'} className="text-[13px]" /> {Math.abs(trend)}%
          </span>
        ) : alert ? <Icon name="warning" className="text-secondary" /> : href ? <Icon name="chevron_right" className="text-muted-foreground/50 transition-transform group-hover:translate-x-0.5" /> : null}
      </div>
      <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">{label}</p>
      <p className={`mt-0.5 font-heading text-3xl font-extrabold tabular-nums ${value === '—' ? 'text-muted-foreground' : 'text-on-surface'}`}>{value}</p>
      <p className="mt-1 text-xs text-muted-foreground">{sub}</p>
    </>
  );
  const cls = 'group rounded-xl border border-border bg-card p-5 transition-all hover:-translate-y-0.5 hover:shadow-md';
  return href ? <Link href={href} className={cls}>{body}</Link> : <div className={cls}>{body}</div>;
}

function QuickLink({ href, icon, label, sub, accent }: { href: string; icon: string; label: string; sub: string; accent: string }) {
  return (
    <Link href={href} className="group flex items-center gap-3 rounded-xl border border-border bg-card p-4 transition-all hover:-translate-y-0.5 hover:border-primary/40 hover:shadow-md">
      <span className={`flex size-11 shrink-0 items-center justify-center rounded-lg ${ACCENT[accent]}`}><Icon name={icon} /></span>
      <span className="min-w-0 flex-1">
        <span className="block truncate font-heading text-sm font-bold text-on-surface">{label}</span>
        <span className="block truncate text-xs text-muted-foreground">{sub}</span>
      </span>
      <Icon name="arrow_forward" className="text-muted-foreground/40 transition-all group-hover:translate-x-0.5 group-hover:text-primary" />
    </Link>
  );
}
