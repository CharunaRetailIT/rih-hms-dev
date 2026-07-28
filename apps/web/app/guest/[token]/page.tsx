'use client';

import { use, useEffect, useMemo, useState } from 'react';
import { Icon } from '@/components/ui/Icon';

type Product = { id: string; name: string; basePrice: number; categoryId: string | null; colorHex: string | null };
type Category = { id: string; name: string; parentId: string | null };
type Menu = {
  guestToken: string;
  tenant: { displayName: string };
  tableLabel: string | null;
  currency: string;
  products: Product[];
  categories: Category[];
  availability: { productId: string; available: boolean }[];
};

export default function GuestOrderPage({ params }: { params: Promise<{ token: string }> }) {
  const { token } = use(params);
  const [menu, setMenu] = useState<Menu | null>(null);
  const [loadErr, setLoadErr] = useState<string | null>(null);
  const [cart, setCart] = useState<Record<string, number>>({}); // productId → qty
  const [cat, setCat] = useState<string>('all');
  const [q, setQ] = useState('');
  const [sheet, setSheet] = useState(false);
  const [placing, setPlacing] = useState(false);
  const [done, setDone] = useState<string | null>(null);

  useEffect(() => {
    let alive = true;
    (async () => {
      try {
        const res = await fetch(`/api/v1/guest/${token}`, { cache: 'no-store' });
        const j = await res.json().catch(() => ({}));
        if (!res.ok) throw new Error(j.error ?? 'This QR code could not be opened.');
        if (alive) setMenu(j as Menu);
      } catch (e) {
        if (alive) setLoadErr((e as Error).message);
      }
    })();
    return () => { alive = false; };
  }, [token]);

  const avail = useMemo(() => {
    const m = new Map<string, boolean>();
    menu?.availability.forEach(a => m.set(a.productId, a.available));
    return m;
  }, [menu]);

  const money = (n: number) => `${menu?.currency ?? ''} ${n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  const byId = useMemo(() => new Map(menu?.products.map(p => [p.id, p]) ?? []), [menu]);
  const cartLines = Object.entries(cart).filter(([, qty]) => qty > 0);
  const cartCount = cartLines.reduce((s, [, q]) => s + q, 0);
  const cartTotal = cartLines.reduce((s, [id, q]) => s + (byId.get(id)?.basePrice ?? 0) * q, 0);

  const visible = useMemo(() => {
    if (!menu) return [];
    return menu.products.filter(p =>
      (cat === 'all' || p.categoryId === cat) &&
      (q === '' || p.name.toLowerCase().includes(q.toLowerCase()))
    );
  }, [menu, cat, q]);

  const setQty = (id: string, delta: number) =>
    setCart(c => { const n = Math.max(0, (c[id] ?? 0) + delta); return { ...c, [id]: n }; });

  async function placeOrder() {
    if (!menu || cartLines.length === 0) return;
    setPlacing(true);
    try {
      const res = await fetch('/api/v1/guest/order', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${menu.guestToken}` },
        body: JSON.stringify({ items: cartLines.map(([productId, quantity]) => ({ productId, quantity })) }),
      });
      const j = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(j.error ?? 'Could not send your order. Please call a server.');
      setDone(j.orderNumber ?? 'sent');
      setCart({}); setSheet(false);
    } catch (e) {
      alert((e as Error).message);
    } finally { setPlacing(false); }
  }

  // ─── error / loading states ───
  if (loadErr) return <Splash icon="error" title="Can't open the menu" body={loadErr} />;
  if (!menu) return <Splash icon="restaurant" title="Loading the menu…" spinner />;

  // ─── order-placed confirmation ───
  if (done) return (
    <Splash icon="check_circle" title="Order placed!" tone="success"
      body={`Your order ${done !== 'sent' ? `(${done}) ` : ''}has been sent${menu.tableLabel ? ` for Table ${menu.tableLabel}` : ''}. A server will confirm it shortly, then bring it over and settle the bill at your table.`}>
      <button onClick={() => setDone(null)}
        className="mt-6 inline-flex items-center gap-2 rounded-lg bg-primary px-6 py-3 font-bold text-white hover:bg-primary-dark active:scale-[0.98]">
        <Icon name="add" className="text-lg" /> Order more
      </button>
    </Splash>
  );

  const topCats = menu.categories.filter(c => menu.products.some(p => p.categoryId === c.id));

  return (
    <main className="mx-auto flex min-h-screen max-w-xl flex-col bg-slate-50 pb-28">
      {/* Header */}
      <header className="sticky top-0 z-20 bg-primary px-4 py-3 text-white shadow-md"
        style={{ background: 'linear-gradient(135deg, #15803d 0%, #00652c 100%)' }}>
        <div className="flex items-center justify-between">
          <div>
            <p className="text-[11px] font-semibold uppercase tracking-wide text-white/70">{menu.tenant.displayName}</p>
            <h1 className="font-headline text-lg font-extrabold leading-tight">
              {menu.tableLabel ? `Table ${menu.tableLabel}` : 'Order here'}
            </h1>
          </div>
          <span className="rounded-full bg-white/15 px-3 py-1 text-xs font-semibold backdrop-blur">Self-order</span>
        </div>
        <div className="mt-3 flex items-center gap-2 rounded-lg bg-white/15 px-3 py-2 backdrop-blur">
          <Icon name="search" className="text-base text-white/80" />
          <input value={q} onChange={e => setQ(e.target.value)} placeholder="Search the menu…"
            className="w-full bg-transparent text-sm text-white placeholder:text-white/60 focus:outline-none" />
        </div>
      </header>

      {/* Category chips */}
      {topCats.length > 0 && (
        <div className="sticky top-[104px] z-10 flex gap-2 overflow-x-auto border-b border-slate-200 bg-white px-4 py-2.5">
          <Chip active={cat === 'all'} onClick={() => setCat('all')}>All</Chip>
          {topCats.map(c => <Chip key={c.id} active={cat === c.id} onClick={() => setCat(c.id)}>{c.name}</Chip>)}
        </div>
      )}

      {/* Menu */}
      <div className="flex-1 divide-y divide-slate-100">
        {visible.length === 0 && <p className="px-4 py-16 text-center text-sm text-slate-400">No items here.</p>}
        {visible.map(p => {
          const off = avail.get(p.id) === false;
          const qty = cart[p.id] ?? 0;
          return (
            <div key={p.id} className={`flex items-center gap-3 px-4 py-3 ${off ? 'opacity-60' : ''}`}>
              <div className="min-w-0 flex-1">
                <p className={`truncate font-semibold ${off ? 'text-slate-400 line-through' : 'text-slate-800'}`}>{p.name}</p>
                <p className={`text-sm font-bold ${off ? 'text-slate-400' : 'text-primary'}`}>{money(p.basePrice)}</p>
              </div>
              {off ? (
                <span className="rounded-md bg-red-100 px-2 py-1 text-[11px] font-extrabold text-red-600">SOLD OUT</span>
              ) : qty > 0 ? (
                <div className="flex items-center gap-2.5">
                  <button onClick={() => setQty(p.id, -1)} className="grid h-8 w-8 place-items-center rounded-full border border-slate-300 text-slate-600 active:scale-95"><Icon name="remove" className="text-lg" /></button>
                  <span className="w-5 text-center font-bold">{qty}</span>
                  <button onClick={() => setQty(p.id, +1)} className="grid h-8 w-8 place-items-center rounded-full bg-primary text-white active:scale-95"><Icon name="add" className="text-lg" /></button>
                </div>
              ) : (
                <button onClick={() => setQty(p.id, +1)} className="rounded-lg border border-primary px-4 py-1.5 text-sm font-bold text-primary active:scale-95">Add</button>
              )}
            </div>
          );
        })}
      </div>

      {/* Sticky cart bar */}
      {cartCount > 0 && (
        <button onClick={() => setSheet(true)}
          className="fixed inset-x-0 bottom-0 z-30 mx-auto flex max-w-xl items-center justify-between bg-primary px-5 py-4 font-bold text-white shadow-2xl active:bg-primary-dark">
          <span className="flex items-center gap-2"><span className="grid h-6 w-6 place-items-center rounded-full bg-white/25 text-xs">{cartCount}</span> View order</span>
          <span>{money(cartTotal)}</span>
        </button>
      )}

      {/* Cart sheet */}
      {sheet && (
        <div className="fixed inset-0 z-40 flex flex-col justify-end bg-black/40" onClick={() => setSheet(false)}>
          <div className="mx-auto w-full max-w-xl rounded-t-2xl bg-white p-5" onClick={e => e.stopPropagation()}>
            <div className="mb-3 flex items-center justify-between">
              <h2 className="font-headline text-lg font-bold">Your order{menu.tableLabel ? ` · Table ${menu.tableLabel}` : ''}</h2>
              <button onClick={() => setSheet(false)} className="text-slate-400"><Icon name="close" /></button>
            </div>
            <div className="max-h-[45vh] space-y-2 overflow-y-auto">
              {cartLines.map(([id, qty]) => {
                const p = byId.get(id); if (!p) return null;
                return (
                  <div key={id} className="flex items-center gap-3">
                    <div className="flex items-center gap-2">
                      <button onClick={() => setQty(id, -1)} className="grid h-7 w-7 place-items-center rounded-full border border-slate-300 active:scale-95"><Icon name="remove" className="text-base" /></button>
                      <span className="w-5 text-center font-bold">{qty}</span>
                      <button onClick={() => setQty(id, +1)} className="grid h-7 w-7 place-items-center rounded-full bg-primary text-white active:scale-95"><Icon name="add" className="text-base" /></button>
                    </div>
                    <p className="min-w-0 flex-1 truncate text-sm font-medium text-slate-700">{p.name}</p>
                    <p className="text-sm font-bold text-slate-800">{money(p.basePrice * qty)}</p>
                  </div>
                );
              })}
            </div>
            <div className="mt-4 flex items-center justify-between border-t border-slate-100 pt-3">
              <span className="font-semibold text-slate-500">Total</span>
              <span className="font-headline text-xl font-extrabold text-primary">{money(cartTotal)}</span>
            </div>
            <button onClick={placeOrder} disabled={placing}
              className="mt-4 flex w-full items-center justify-center gap-2 rounded-lg bg-primary px-6 py-3.5 font-bold text-white hover:bg-primary-dark active:scale-[0.98] disabled:opacity-60">
              <Icon name="restaurant_menu" className="text-lg" /> {placing ? 'Sending…' : 'Send to kitchen'}
            </button>
            <p className="mt-2 text-center text-xs text-slate-400">A server settles the bill at your table — no payment needed here.</p>
          </div>
        </div>
      )}
    </main>
  );
}

function Chip({ active, onClick, children }: { active: boolean; onClick: () => void; children: React.ReactNode }) {
  return (
    <button onClick={onClick}
      className={`whitespace-nowrap rounded-full px-4 py-1.5 text-sm font-semibold transition-colors ${active ? 'bg-primary text-white' : 'bg-slate-100 text-slate-600'}`}>
      {children}
    </button>
  );
}

function Splash({ icon, title, body, tone, spinner, children }: {
  icon: string; title: string; body?: string; tone?: 'success'; spinner?: boolean; children?: React.ReactNode;
}) {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center bg-slate-50 px-8 text-center">
      <div className={`grid h-16 w-16 place-items-center rounded-full ${tone === 'success' ? 'bg-primary/10 text-primary' : 'bg-slate-200 text-slate-500'}`}>
        {spinner
          ? <span className="h-7 w-7 animate-spin rounded-full border-2 border-slate-300 border-t-primary" />
          : <Icon name={icon} className="text-3xl" />}
      </div>
      <h1 className="mt-5 font-headline text-xl font-bold text-slate-800">{title}</h1>
      {body && <p className="mt-2 max-w-sm text-sm text-slate-500">{body}</p>}
      {children}
    </main>
  );
}
