'use client';

import { useCallback, useEffect, useRef, useState } from 'react';
import { useRouter } from 'next/navigation';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient, money } from '@/lib/api-client';
import { Modal } from '@/components/ui/Modal';
import { confirmDialog, promptDialog } from '@/components/ui/confirm';
import { Field, Combobox } from '@/components/ui/form';
import { HeaderStat, Num } from '@/components/ui/HeaderStat';
import { LayoutGrid, CalendarClock, QrCode, Printer, Copy, Download, Bell, BellOff } from 'lucide-react';
import QR from 'qrcode';
import { pushSupported, currentSubscription, enablePush, disablePush } from '@/lib/push';

type Loc = { id: string; code: string; name: string };
type FloorRow = { id: string; name: string };
type TableStatus = {
  id: string; code: string; name: string | null; seats: number; area: string | null; floorId: string | null; posX: number; posY: number;
  occupied: boolean; orderId: string | null; orderNumber: string | null; total: number; orderStatus: string | null;
  pendingAcceptance: boolean;
};
type Reservation = {
  id: string; tableId: string | null; customerName: string; phone: string | null; partySize: number;
  reservedAt: string; durationMinutes: number; status: string; notes: string | null;
};

export default function FloorPage() {
  const router = useRouter();
  const [locs, setLocs] = useState<Loc[]>([]);
  const [locId, setLocId] = useState<string>('');
  // A location can have several floors (#68) — this filter narrows the Plan/List
  // views to one, since tables now carry a floorId rather than just a free-text area.
  const [floors, setFloors] = useState<FloorRow[]>([]);
  const [floorFilter, setFloorFilter] = useState<string>('');
  const [tables, setTables] = useState<TableStatus[]>([]);
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [loading, setLoading] = useState(true);
  const [toast, setToast] = useState<string | null>(null);
  const [manage, setManage] = useState(false);
  const [reserve, setReserve] = useState(false);
  const [mergeDest, setMergeDest] = useState<TableStatus | null | undefined>(undefined); // undefined=closed, null=picking dest, set=picking source
  const [busy, setBusy] = useState(false);

  // Push opt-in (#floor-push) — "new order" alerts even when this tab isn't open.
  const [pushOn, setPushOn] = useState(false);
  const [pushBusy, setPushBusy] = useState(false);
  useEffect(() => { currentSubscription().then(s => setPushOn(!!s)).catch(() => {}); }, []);
  async function togglePush() {
    setPushBusy(true);
    try {
      if (pushOn) { await disablePush(); setPushOn(false); flash('Notifications turned off on this device.'); }
      else { await enablePush(); setPushOn(true); flash('Notifications on — you’ll be alerted here for new orders on your floor.'); }
    } catch (e) { flash((e as Error).message || 'Could not update notifications.'); }
    finally { setPushBusy(false); }
  }

  // Visual floor plan (#68)
  const [plan, setPlan] = useState(true);
  const [editLayout, setEditLayout] = useState(false);
  const [pos, setPos] = useState<Record<string, { x: number; y: number }>>({});
  const [dirty, setDirty] = useState(false);
  const canvasRef = useRef<HTMLDivElement>(null);
  const dragId = useRef<string | null>(null);

  function flash(m: string) { setToast(m); window.setTimeout(() => setToast(null), 3000); }

  // Seed positions from saved x/y; auto-grid any unplaced (0/0) table. Keep live
  // drag positions across the 10s status refresh so tiles don't snap back.
  useEffect(() => {
    setPos(prev => {
      const next: Record<string, { x: number; y: number }> = {};
      const CW = 132, CH = 112, OX = 16, OY = 16, COLS = 6;
      const cell = (x: number, y: number) => `${Math.round((x - OX) / CW)},${Math.round((y - OY) / CH)}`;
      // Reserve every grid cell a placed (saved x/y) or already-dragged table sits in,
      // so an unplaced/new table (posX/posY = 0) never lands on top of one.
      const taken = new Set<string>();
      tables.forEach(t => {
        const p = prev[t.id] ?? (((t.posX ?? 0) !== 0 || (t.posY ?? 0) !== 0) ? { x: t.posX, y: t.posY } : null);
        if (p) taken.add(cell(p.x, p.y));
      });
      let slot = 0;
      tables.forEach(t => {
        const placed = (t.posX ?? 0) !== 0 || (t.posY ?? 0) !== 0;
        if (prev[t.id]) { next[t.id] = prev[t.id]; return; }
        if (placed) { next[t.id] = { x: t.posX, y: t.posY }; return; }
        while (taken.has(`${slot % COLS},${Math.floor(slot / COLS)}`)) slot++;
        const col = slot % COLS, row = Math.floor(slot / COLS);
        taken.add(`${col},${row}`); slot++;
        next[t.id] = { x: OX + col * CW, y: OY + row * CH };
      });
      return next;
    });
  }, [tables]);

  function onTileDown(e: React.PointerEvent, id: string) {
    if (!editLayout) return;
    dragId.current = id;
    (e.currentTarget as Element).setPointerCapture?.(e.pointerId);
  }
  function onCanvasMove(e: React.PointerEvent) {
    if (!dragId.current || !canvasRef.current) return;
    const r = canvasRef.current.getBoundingClientRect();
    const x = Math.max(0, Math.round(e.clientX - r.left - 56));
    const y = Math.max(0, Math.round(e.clientY - r.top - 36));
    const id = dragId.current;
    setPos(p => ({ ...p, [id]: { x, y } }));
    setDirty(true);
  }
  function onTileUp() { dragId.current = null; }

  async function saveLayout() {
    setBusy(true);
    try {
      await apiClient('/api/v1/tables/layout', {
        method: 'PUT',
        body: JSON.stringify({ items: tables.map(t => ({ id: t.id, posX: Math.round(pos[t.id]?.x ?? 0), posY: Math.round(pos[t.id]?.y ?? 0) })) }),
      });
      setDirty(false); setEditLayout(false); flash('Floor plan saved.');
    } catch (e) { flash(err(e, 'Could not save the layout.')); } finally { setBusy(false); }
  }

  async function doMerge(source: TableStatus) {
    if (!mergeDest || !mergeDest.orderId || !source.orderId) return;
    if (!(await confirmDialog({
      title: `Merge ${source.code} into ${mergeDest.code}?`,
      body: `All items on ${source.code} move to ${mergeDest.code}, and ${source.code} closes. This cannot be undone.`,
      confirmLabel: 'Merge',
      danger: true,
    }))) return;
    setBusy(true);
    try {
      await apiClient(`/api/v1/orders/${mergeDest.orderId}/merge`, { method: 'POST', body: JSON.stringify({ sourceOrderId: source.orderId }) });
      flash(`Merged ${source.code} into ${mergeDest.code}`); setMergeDest(undefined); await loadFloor(locId);
    } catch (e) { flash((e as Error).message); } finally { setBusy(false); }
  }

  const loadFloor = useCallback(async (lid: string) => {
    if (!lid) return;
    try {
      const today = new Date().toISOString().slice(0, 10);
      const [t, r] = await Promise.all([
        apiClient<TableStatus[]>(`/api/v1/tables/status?locationId=${lid}`),
        apiClient<Reservation[]>(`/api/v1/reservations?locationId=${lid}&from=${today}&to=${today}T23:59:59`).catch(() => []),
      ]);
      setTables(t); setReservations(r);
    } catch { /* keep last */ }
  }, []);

  useEffect(() => {
    (async () => {
      const l = await apiClient<Loc[]>('/api/v1/locations').catch(() => []);
      setLocs(l);
      const main = l.find(x => x.code === 'MAIN') ?? l[0];
      if (main) setLocId(main.id);
      setLoading(false);
    })();
  }, []);

  useEffect(() => {
    if (!locId) return;
    void loadFloor(locId);
    const t = setInterval(() => loadFloor(locId), 10000);
    return () => clearInterval(t);
  }, [locId, loadFloor]);

  useEffect(() => {
    if (!locId) { setFloors([]); return; }
    setFloorFilter(''); // switching location — the old floor selection no longer applies
    apiClient<FloorRow[]>(`/api/v1/floors?locationId=${locId}`).then(setFloors).catch(() => setFloors([]));
  }, [locId]);

  // Live "orders changed" push (#floor-push) — a new guest order lands within ~1s
  // instead of waiting for the 10s poll above. Scoped server-side: a steward only
  // gets this signal for their assigned floor(s) (see RealtimeBus.PublishToUsers).
  // Mirrors the delivery board / KOT board's exact SSE reconnect pattern.
  const locIdRef = useRef(locId); locIdRef.current = locId;
  const loadFloorRef = useRef(loadFloor); loadFloorRef.current = loadFloor;
  useEffect(() => {
    let stop = false; let ctrl: AbortController | null = null; let backoff = 1000;
    let debounce: ReturnType<typeof setTimeout> | null = null;
    const LIVE = new Set(['orders']);
    const bump = () => { if (debounce) clearTimeout(debounce); debounce = setTimeout(() => { if (locIdRef.current) void loadFloorRef.current(locIdRef.current); }, 400); };
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

  // Plan/List views show only the selected floor (merge, reservations, and Add/Manage
  // Tables still work across every floor — narrowing those would just get in the way).
  const visibleTables = floorFilter ? tables.filter(t => t.floorId === floorFilter) : tables;
  const areas = Array.from(new Set(visibleTables.map(t => t.area ?? 'Floor')));

  function tapTable(t: TableStatus) {
    if (t.pendingAcceptance) { void acceptOrder(t); return; }
    if (t.occupied && t.orderId) router.push(`/pos?order=${t.orderId}`);
    else router.push(`/pos?table=${t.id}`);
  }

  // Guest QR orders (#108) land awaiting acceptance — not sent to the kitchen until a
  // steward reviews and accepts. Accepting reuses Confirm under the hood (fires the KOT).
  async function acceptOrder(t: TableStatus) {
    if (!t.orderId) return;
    if (!(await confirmDialog({
      title: `Accept order at ${t.code}?`,
      body: `A guest placed order ${t.orderNumber} — LKR ${t.total.toFixed(2)}. Accepting sends it straight to the kitchen.`,
      confirmLabel: 'Accept',
    }))) return;
    setBusy(true);
    try {
      await apiClient(`/api/v1/orders/${t.orderId}/accept`, { method: 'POST' });
      flash(`Order at ${t.code} accepted — sent to kitchen.`); await loadFloor(locId);
    } catch (e) { flash(err(e, 'Could not accept the order.')); } finally { setBusy(false); }
  }

  return (
    <>
      <Topbar title="Floor" subtitle="See your whole dining room and manage tables and reservations" />
      <div className="p-6 md:p-8">
        <div className="mb-5 flex flex-wrap items-center justify-between gap-3">
          <div className="flex items-center gap-3">
            <h2 className="font-heading text-xl font-bold">Floor</h2>
            {locs.length > 1 && (
              <select value={locId} onChange={e => setLocId(e.target.value)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm">
                {locs.map(l => <option key={l.id} value={l.id}>{l.name}</option>)}
              </select>
            )}
            {floors.length > 0 && (
              <select value={floorFilter} onChange={e => setFloorFilter(e.target.value)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm">
                <option value="">All floors</option>
                {floors.map(f => <option key={f.id} value={f.id}>{f.name}</option>)}
              </select>
            )}
            <HeaderStat><Num>{visibleTables.filter(t => t.occupied).length}</Num>/<Num>{visibleTables.length}</Num> occupied</HeaderStat>
          </div>
          <div className="flex flex-wrap gap-2">
            <div className="flex overflow-hidden rounded-lg border border-border">
              <button onClick={() => { setPlan(true); }} className={`px-3 py-2 text-sm font-medium ${plan ? 'bg-primary text-primary-foreground' : 'bg-card hover:bg-muted'}`}>Plan</button>
              <button onClick={() => { setPlan(false); setEditLayout(false); }} className={`px-3 py-2 text-sm font-medium ${!plan ? 'bg-primary text-primary-foreground' : 'bg-card hover:bg-muted'}`}>List</button>
            </div>
            {plan && !editLayout && <button onClick={() => setEditLayout(true)} className="rounded-lg border border-border bg-card px-4 py-2 text-sm font-medium hover:bg-muted">Edit layout</button>}
            {plan && editLayout && <button onClick={saveLayout} disabled={busy} className="rounded-lg bg-primary px-4 py-2 text-sm font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{dirty ? 'Save layout' : 'Done'}</button>}
            <button onClick={() => setMergeDest(tables.filter(t => t.occupied).length < 2 ? (flash('Need two occupied tables to merge'), undefined) : null)}
              disabled={tables.filter(t => t.occupied).length < 2}
              className="rounded-lg border border-border bg-card px-4 py-2 text-sm font-medium hover:bg-muted disabled:opacity-40">Merge tables</button>
            <button onClick={() => setReserve(true)} className="rounded-lg border border-border bg-card px-4 py-2 text-sm font-medium hover:bg-muted">Reservations</button>
            <button onClick={() => setManage(true)} className="rounded-lg border border-border bg-card px-4 py-2 text-sm font-medium hover:bg-muted">Add or Manage Tables</button>
            {pushSupported() && (
              <button onClick={togglePush} disabled={pushBusy}
                title={pushOn ? 'Turn off new-order alerts on this device' : 'Get alerted here when a new order lands on your floor'}
                className={`flex items-center gap-1.5 rounded-lg border px-4 py-2 text-sm font-medium disabled:opacity-50 ${pushOn ? 'border-primary bg-primary-tint text-primary' : 'border-border bg-card hover:bg-muted'}`}>
                {pushOn ? <Bell className="size-4" /> : <BellOff className="size-4" />}
                {pushOn ? 'Notifications on' : 'Enable notifications'}
              </button>
            )}
          </div>
        </div>

        {loading ? (
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-4 lg:grid-cols-6">{Array.from({ length: 8 }).map((_, i) => <div key={i} className="h-24 animate-pulse rounded-xl bg-muted" />)}</div>
        ) : tables.length === 0 ? (
          <div className="card p-10 text-center text-sm text-muted-foreground">No tables yet. Use <strong>Add or Manage Tables</strong> to add some.</div>
        ) : visibleTables.length === 0 ? (
          <div className="card p-10 text-center text-sm text-muted-foreground">No tables on this floor.</div>
        ) : plan ? (
          <div>
            {editLayout && <p className="mb-2 text-xs text-muted-foreground">Drag tables to arrange the room, then <strong>Save layout</strong>. Tapping is disabled while editing.</p>}
            <div ref={canvasRef} onPointerMove={onCanvasMove} onPointerUp={onTileUp} onPointerLeave={onTileUp}
              className="relative h-[68vh] w-full overflow-auto rounded-xl border border-border bg-muted/20"
              style={{ backgroundImage: 'radial-gradient(circle at 1px 1px, rgba(0,0,0,0.06) 1px, transparent 0)', backgroundSize: '24px 24px' }}>
              {visibleTables.map(t => {
                const p = pos[t.id] ?? { x: 0, y: 0 };
                return (
                  <div key={t.id}
                    onPointerDown={e => onTileDown(e, t.id)}
                    onClick={() => { if (!editLayout) tapTable(t); }}
                    style={{ left: p.x, top: p.y, touchAction: editLayout ? 'none' : undefined }}
                    className={`absolute flex h-[72px] w-[112px] flex-col justify-between rounded-xl border-2 p-2 ${editLayout ? 'cursor-move' : 'cursor-pointer active:scale-95'} ${t.pendingAcceptance ? 'animate-pulse border-status-error bg-status-error/10' : t.occupied ? 'border-status-pending/50 bg-status-pending/10' : 'border-primary/40 bg-primary-tint hover:border-primary'}`}>
                    <div className="flex items-center justify-between">
                      <span className="font-heading text-base font-bold">{t.code}</span>
                      <span className="text-[10px] text-muted-foreground">{t.seats}p</span>
                    </div>
                    {t.pendingAcceptance
                      ? <span className="text-[11px] font-bold text-status-error">New order — Accept</span>
                      : t.occupied
                      ? <div className="text-[11px] leading-tight"><span className="font-semibold text-status-pending">{money(t.total)}</span></div>
                      : <span className="text-[11px] font-semibold text-primary">Open</span>}
                  </div>
                );
              })}
            </div>
          </div>
        ) : (
          areas.map(area => (
            <div key={area} className="mb-6">
              <h3 className="mb-2 text-xs font-bold uppercase tracking-wide text-muted-foreground">{area}</h3>
              <div className="grid grid-cols-2 gap-3 sm:grid-cols-4 lg:grid-cols-6">
                {visibleTables.filter(t => (t.area ?? 'Floor') === area).map(t => (
                  <button key={t.id} onClick={() => tapTable(t)}
                    className={`flex h-24 flex-col items-start justify-between rounded-xl border-2 p-3 text-left transition-transform active:scale-95 ${t.pendingAcceptance ? 'animate-pulse border-status-error bg-status-error/10' : t.occupied ? 'border-status-pending/40 bg-status-pending/10' : 'border-primary/30 bg-primary-tint hover:border-primary'}`}>
                    <div className="flex w-full items-center justify-between">
                      <span className="font-heading text-lg font-bold">{t.code}</span>
                      <span className="text-xs text-muted-foreground">{t.seats}p</span>
                    </div>
                    {t.pendingAcceptance
                      ? <span className="text-xs font-bold text-status-error">New order — Accept</span>
                      : t.occupied
                      ? <div className="text-xs"><span className="font-semibold text-status-pending">{money(t.total)}</span><div className="text-muted-foreground">{t.orderNumber}</div></div>
                      : <span className="text-xs font-semibold text-primary">Open</span>}
                  </button>
                ))}
              </div>
            </div>
          ))
        )}
      </div>

      {manage && <ManageTables locId={locId} tables={tables} onClose={() => setManage(false)} onChanged={() => loadFloor(locId)} flash={flash} />}
      {reserve && <Reservations locId={locId} tables={tables} reservations={reservations} onClose={() => setReserve(false)} onChanged={() => loadFloor(locId)} flash={flash} />}

      {mergeDest !== undefined && (
        <Modal title="Merge Tables" onClose={() => setMergeDest(undefined)}>
          <p className="mb-3 text-sm text-muted-foreground">
            {mergeDest === null
              ? 'Step 1 — pick the table to KEEP (the bill everything merges into).'
              : <>Step 2 — pick the table to merge <strong>into {mergeDest.code}</strong>. Its items move to {mergeDest.code} and it closes.</>}
          </p>
          <div className="grid grid-cols-3 gap-2 sm:grid-cols-4">
            {tables.filter(t => t.occupied && (mergeDest === null || t.id !== mergeDest.id)).map(t => (
              <button key={t.id} disabled={busy}
                onClick={() => (mergeDest === null ? setMergeDest(t) : doMerge(t))}
                className="flex h-20 flex-col items-start justify-between rounded-lg border-2 border-status-pending/40 bg-status-pending/10 p-2 text-left hover:border-primary disabled:opacity-50">
                <span className="font-heading font-bold">{t.code}</span>
                <span className="text-[10px] text-muted-foreground">{money(t.total)}</span>
              </button>
            ))}
          </div>
          {mergeDest && <button onClick={() => setMergeDest(null)} className="mt-4 text-sm text-primary hover:underline">← change the table to keep</button>}
        </Modal>
      )}
      {toast && <div className="fixed bottom-12 left-1/2 z-[70] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">{toast}</div>}
    </>
  );
}

function ManageTables({ locId, tables, onClose, onChanged, flash }: {
  locId: string; tables: TableStatus[]; onClose: () => void; onChanged: () => void; flash: (m: string) => void;
}) {
  type Row = { id: string; code: string; name: string | null; seats: number; area: string | null; floorId: string | null; sortOrder?: number; isActive: boolean };
  const [editId, setEditId] = useState<string | null>(null);
  const [code, setCode] = useState(''); const [seats, setSeats] = useState(''); const [area, setArea] = useState(''); const [floorId, setFloorId] = useState(''); const [busy, setBusy] = useState(false);
  const [rows, setRows] = useState<Row[]>([]);
  const [floors, setFloors] = useState<FloorRow[]>([]);
  const [qr, setQr] = useState<{ code: string; url: string; dataUrl: string } | null>(null);
  const [guestQrOn, setGuestQrOn] = useState<boolean | null>(null);
  const occupiedIds = new Set(tables.filter(t => t.occupied).map(t => t.id));
  const load = useCallback(async () => {
    try { setRows(await apiClient<Row[]>(`/api/v1/tables?locationId=${locId}&all=true`)); } catch { /* */ }
  }, [locId]);
  const loadFloors = useCallback(async () => {
    try { setFloors(await apiClient<FloorRow[]>(`/api/v1/floors?locationId=${locId}`)); } catch { /* */ }
  }, [locId]);
  useEffect(() => { void load(); void loadFloors(); }, [load, loadFloors]);
  useEffect(() => { apiClient<{ guestQrEnabled: boolean }>('/api/v1/tab-devices').then(d => setGuestQrOn(d.guestQrEnabled)).catch(() => setGuestQrOn(null)); }, []);

  async function addFloor() {
    const name = await promptDialog({ title: 'New Floor', body: 'e.g. Ground Floor, Rooftop, Garden', placeholder: 'Floor name', confirmLabel: 'Add' });
    if (!name || !name.trim()) return;
    setBusy(true);
    try {
      const row = await apiClient<{ id: string }>('/api/v1/floors', { method: 'PUT', body: JSON.stringify({ locationId: locId, name: name.trim() }) });
      await loadFloors(); setFloorId(row.id); flash('Floor added.');
    } catch (e) { flash(err(e, 'Could not add the floor.')); } finally { setBusy(false); }
  }

  async function showQr(t: Row) {
    setBusy(true);
    try {
      const { url } = await apiClient<{ token: string; url: string; table: string }>(`/api/v1/tables/${t.id}/guest-token`);
      const dataUrl = await QR.toDataURL(url, { width: 320, margin: 2, errorCorrectionLevel: 'M' });
      setQr({ code: t.code, url, dataUrl });
    } catch (e) { flash(err(e, 'Could not generate the QR code.')); } finally { setBusy(false); }
  }
  function printQr() {
    if (!qr) return;
    const w = window.open('', '_blank', 'width=420,height=560');
    if (!w) return;
    w.document.write(`<html><head><title>Table ${qr.code} — Scan to order</title></head>
      <body style="font-family:system-ui,sans-serif;text-align:center;padding:32px;color:#0f172a">
        <div style="font-size:13px;letter-spacing:.08em;text-transform:uppercase;color:#15803d;font-weight:700">Scan to order</div>
        <div style="font-size:34px;font-weight:800;margin:4px 0 16px">Table ${qr.code}</div>
        <img src="${qr.dataUrl}" style="width:300px;height:300px" />
        <div style="font-size:12px;color:#64748b;margin-top:16px">Point your phone camera here to see the menu and order. A server settles your bill at the table.</div>
      </body></html>`);
    w.document.close(); w.focus(); setTimeout(() => w.print(), 250);
  }
  function downloadQr() {
    if (!qr) return;
    const a = document.createElement('a');
    a.href = qr.dataUrl; a.download = `table-${qr.code}-qr.png`;
    document.body.appendChild(a); a.click(); a.remove();
  }
  const refresh = () => { void load(); onChanged(); };

  function reset() { setEditId(null); setCode(''); setSeats(''); setArea(''); setFloorId(''); }
  function startEdit(t: Row) { setEditId(t.id); setCode(t.code); setSeats(String(t.seats)); setArea(t.area ?? ''); setFloorId(t.floorId ?? ''); }

  async function save() {
    if (!code.trim()) { flash('Table code is required.'); return; }
    setBusy(true);
    try {
      const editing = rows.find(r => r.id === editId);
      await apiClient('/api/v1/tables', { method: 'PUT', body: JSON.stringify({ id: editId, locationId: locId, code: code.trim(), seats: Number(seats) || 2, area: area.trim() || null, floorId: floorId || null, sortOrder: editing?.sortOrder ?? 0 }) });
      reset(); flash('Table saved.'); refresh();
    } catch (e) { flash(err(e, 'Could not save the table.')); } finally { setBusy(false); }
  }
  async function remove(id: string, c: string) {
    if (!(await confirmDialog({ title: `Remove ${c}?`, body: 'This permanently removes the table. This cannot be undone.', confirmLabel: 'Remove', danger: true }))) return;
    setBusy(true); try { await apiClient(`/api/v1/tables/${id}`, { method: 'DELETE' }); flash(`${c} removed.`); if (editId === id) reset(); refresh(); } catch (e) { flash(err(e, 'Could not remove.')); } finally { setBusy(false); }
  }
  async function toggleActive(t: Row) {
    if (t.isActive && !(await confirmDialog({ title: `Take ${t.code} off service?`, body: 'The table will be hidden from the floor until you bring it back.', confirmLabel: 'Take off', danger: true }))) return;
    setBusy(true);
    try { await apiClient(`/api/v1/tables/${t.id}/active`, { method: 'POST', body: JSON.stringify({ isActive: !t.isActive }) }); flash(`${t.code} ${t.isActive ? 'taken off service' : 'back in service'}.`); refresh(); }
    catch (e) { flash(err(e, 'Could not update the table.')); } finally { setBusy(false); }
  }
  return (
    <>
    <Modal title="Add or Manage Tables" icon={<LayoutGrid className="size-4" />} onClose={() => !busy && onClose()} footer={
      <div className="flex gap-2">
        {editId && <button disabled={busy} onClick={reset} className="rounded-lg border border-border px-4 py-2.5 text-sm font-semibold hover:bg-muted disabled:opacity-50">Cancel Edit</button>}
        <button disabled={busy} onClick={save} className="flex-1 rounded-lg bg-primary py-2.5 text-sm font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{editId ? 'Save Table' : 'Add Table'}</button>
      </div>
    }>
      <div className="mb-4 max-h-64 divide-y divide-border overflow-y-auto rounded-lg border border-border">
        {rows.length === 0 && <div className="px-3 py-4 text-center text-sm text-muted-foreground">No tables.</div>}
        {rows.map(t => {
          const occupied = occupiedIds.has(t.id);
          return (
            <div key={t.id} className={`flex items-center justify-between px-3 py-2 text-sm ${t.isActive ? '' : 'opacity-60'}`}>
              <span>
                <span className="font-medium">{t.code}</span>
                <span className="text-muted-foreground"> · {t.seats}p{floors.find(f => f.id === t.floorId)?.name ? ` · ${floors.find(f => f.id === t.floorId)?.name}` : t.area ? ` · ${t.area}` : ''}</span>
                {!t.isActive && <span className="ml-2 rounded bg-muted px-1.5 py-0.5 text-[10px] font-semibold uppercase text-muted-foreground">Off</span>}
              </span>
              <span className="flex gap-2">
                <button disabled={busy} onClick={() => showQr(t)} title="Guest-order QR code"
                  className="grid place-items-center rounded-lg border border-border px-2 py-1 text-xs font-medium hover:bg-muted disabled:opacity-40"><QrCode className="size-3.5" /></button>
                <button disabled={busy} onClick={() => startEdit(t)} title="Edit name / seats / area"
                  className="rounded-lg border border-border px-3 py-1 text-xs font-medium hover:bg-muted disabled:opacity-40">Edit</button>
                <button disabled={busy || (t.isActive && occupied)} onClick={() => toggleActive(t)}
                  title={t.isActive ? (occupied ? 'Occupied — settle first' : 'Temporarily take out of service') : 'Bring back into service'}
                  className="rounded-lg border border-border px-3 py-1 text-xs font-medium hover:bg-muted disabled:opacity-40">
                  {t.isActive ? 'Take off' : 'Bring back'}
                </button>
                <button disabled={busy || occupied} onClick={() => remove(t.id, t.code)} className="rounded-lg border border-border px-3 py-1 text-xs font-medium text-status-error hover:bg-muted disabled:opacity-40" title={occupied ? 'Occupied — settle first' : 'Permanently remove'}>Remove</button>
              </span>
            </div>
          );
        })}
      </div>
      <div className="mb-1 text-sm font-semibold">{editId ? 'Edit Table' : 'New Table'}</div>
      <div className="grid grid-cols-3 gap-2">
        <Field label="Code" mono value={code} onChange={v => setCode(v.toUpperCase())} placeholder="Code (T1)" />
        <Field label="Seats" value={seats} onChange={v => setSeats(v.replace(/[^0-9]/g, ''))} inputMode="numeric" placeholder="Seats" />
        <Field label="Area" value={area} onChange={setArea} placeholder="Area (optional)" />
      </div>
      <div className="mt-2 flex items-end gap-2">
        <Combobox label="Floor (optional)" className="flex-1" value={floorId} onChange={setFloorId}
          placeholder="No floor…" options={floors.map(f => ({ value: f.id, label: f.name }))} />
        <button type="button" disabled={busy} onClick={addFloor}
          className="h-[46px] shrink-0 rounded-lg border border-border px-3 text-xs font-semibold hover:bg-muted disabled:opacity-50">+ New floor</button>
      </div>
      <p className="mt-1.5 text-xs text-muted-foreground">A table&apos;s floor drives which steward gets notified for guest orders placed there.</p>
    </Modal>

    {qr && (
      <Modal title={`Table ${qr.code} — Guest QR`} icon={<QrCode className="size-4" />} onClose={() => setQr(null)} footer={
        <div className="flex gap-2">
          <button onClick={() => { void navigator.clipboard?.writeText(qr.url); flash('Link copied.'); }}
            className="flex items-center justify-center gap-1.5 rounded-lg border border-border px-3 py-2.5 text-sm font-semibold hover:bg-muted"><Copy className="size-4" /> Copy</button>
          <button onClick={downloadQr} className="flex items-center justify-center gap-1.5 rounded-lg border border-border px-3 py-2.5 text-sm font-semibold hover:bg-muted"><Download className="size-4" /> Download</button>
          <button onClick={printQr} className="flex flex-1 items-center justify-center gap-1.5 rounded-lg bg-primary py-2.5 text-sm font-bold text-primary-foreground hover:bg-primary-dark"><Printer className="size-4" /> Print</button>
        </div>
      }>
        {guestQrOn === false && (
          <div className="mb-3 rounded-lg border border-amber-300 bg-amber-50 px-3 py-2 text-xs text-amber-800">
            Guest QR ordering is currently <strong>OFF</strong> for your account — these codes won&apos;t open until you enable the add-on in <strong>Settings</strong>.
          </div>
        )}
        <div className="flex flex-col items-center">
          {/* eslint-disable-next-line @next/next/no-img-element */}
          <img src={qr.dataUrl} alt={`QR for table ${qr.code}`} className="size-56 rounded-lg border border-border" />
          <p className="mt-3 text-center text-xs text-muted-foreground">Print this and place it on the table. Guests scan it to see the live menu and send orders straight to the kitchen — a server settles the bill at the table.</p>
          <code className="mt-2 max-w-full truncate rounded bg-muted px-2 py-1 text-[11px] text-muted-foreground">{qr.url}</code>
        </div>
      </Modal>
    )}
    </>
  );
}

function Reservations({ locId, tables, reservations, onClose, onChanged, flash }: {
  locId: string; tables: TableStatus[]; reservations: Reservation[]; onClose: () => void; onChanged: () => void; flash: (m: string) => void;
}) {
  const [editId, setEditId] = useState<string | null>(null);
  const [name, setName] = useState(''); const [phone, setPhone] = useState(''); const [party, setParty] = useState('2');
  const [time, setTime] = useState(''); const [tableId, setTableId] = useState(''); const [busy, setBusy] = useState(false);
  const nowLocal = new Date(Date.now() - new Date().getTimezoneOffset() * 60000).toISOString().slice(0, 16);
  // Convert a stored UTC ISO time to the value a datetime-local input expects (local wall-clock).
  const toLocalInput = (iso: string) => new Date(new Date(iso).getTime() - new Date().getTimezoneOffset() * 60000).toISOString().slice(0, 16);

  function reset() { setEditId(null); setName(''); setPhone(''); setTime(''); setTableId(''); setParty('2'); }
  function startEdit(r: Reservation) {
    setEditId(r.id); setName(r.customerName); setPhone(r.phone ?? ''); setParty(String(r.partySize));
    setTime(toLocalInput(r.reservedAt)); setTableId(r.tableId ?? '');
  }

  async function add() {
    if (!name.trim() || !time) { flash('Name and time are required.'); return; }
    // Past-time guard only applies to new bookings; editing an existing (possibly already-past) one is allowed.
    if (!editId && new Date(time).getTime() < Date.now() - 60000) { flash('Reservation time can’t be in the past.'); return; }
    setBusy(true);
    try {
      await apiClient('/api/v1/reservations', { method: 'PUT', body: JSON.stringify({ id: editId, locationId: locId, customerName: name.trim(), phone: phone.trim() || null, partySize: Number(party) || 2, reservedAt: new Date(time).toISOString(), tableId: tableId || null }) });
      reset(); flash('Reservation saved.'); onChanged();
    } catch (e) { flash(err(e, 'Could not save reservation.')); } finally { setBusy(false); }
  }
  async function setStatus(id: string, status: string) {
    if ((status === 'cancelled' || status === 'no_show') && !(await confirmDialog({
      title: status === 'cancelled' ? 'Cancel this reservation?' : 'Mark as no-show?',
      body: status === 'cancelled' ? 'The reservation will be cancelled.' : 'The reservation will be marked as a no-show.',
      confirmLabel: status === 'cancelled' ? 'Cancel reservation' : 'Mark no-show',
      danger: true,
    }))) return;
    setBusy(true); try { await apiClient(`/api/v1/reservations/${id}/status`, { method: 'POST', body: JSON.stringify({ status }) }); if (editId === id) reset(); flash(`Marked ${status}.`); onChanged(); } catch (e) { flash(err(e, 'Could not update.')); } finally { setBusy(false); }
  }
  const fmt = (iso: string) => new Date(iso).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
  return (
    <Modal title="Today's Reservations" icon={<CalendarClock className="size-4" />} onClose={() => !busy && onClose()} footer={
      <div className="flex gap-2">
        {editId && <button disabled={busy} onClick={reset} className="rounded-lg border border-border px-4 py-2.5 text-sm font-semibold hover:bg-muted disabled:opacity-50">Cancel Edit</button>}
        <button disabled={busy} onClick={add} className="flex-1 rounded-lg bg-primary py-2.5 text-sm font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{editId ? 'Save Reservation' : 'Add Reservation'}</button>
      </div>
    }>
      <div className="mb-4 max-h-[28vh] divide-y divide-border overflow-y-auto rounded-lg border border-border">
        {reservations.length === 0 && <div className="px-3 py-4 text-center text-sm text-muted-foreground">No reservations today.</div>}
        {reservations.map(r => (
          <div key={r.id} className={`flex items-center justify-between px-3 py-2 text-sm ${editId === r.id ? 'bg-primary-tint/40' : ''}`}>
            <div>
              <span className="font-medium">{fmt(r.reservedAt)} · {r.customerName}</span>
              <span className="ml-2 text-xs text-muted-foreground">{r.partySize}p{r.phone ? ` · ${r.phone}` : ''} · <span className="capitalize">{r.status.replace('_', ' ')}</span></span>
            </div>
            <div className="flex gap-1">
              {r.status !== 'cancelled' && r.status !== 'completed' && <button disabled={busy} onClick={() => startEdit(r)} className="rounded border border-border px-2 py-1 text-xs hover:bg-muted">Edit</button>}
              {r.status === 'booked' && <button disabled={busy} onClick={() => setStatus(r.id, 'seated')} className="rounded border border-border px-2 py-1 text-xs hover:bg-muted">Seat</button>}
              {r.status === 'booked' && <button disabled={busy} onClick={() => setStatus(r.id, 'no_show')} className="rounded border border-border px-2 py-1 text-xs text-status-error hover:bg-muted">No-show</button>}
              {r.status !== 'cancelled' && r.status !== 'completed' && <button disabled={busy} onClick={() => setStatus(r.id, 'cancelled')} className="rounded border border-border px-2 py-1 text-xs hover:bg-muted">Cancel</button>}
            </div>
          </div>
        ))}
      </div>
      <div className="mb-1 text-sm font-semibold">{editId ? 'Edit Reservation' : 'New Reservation'}</div>
      <div className="grid grid-cols-2 gap-2">
        <Field label="Customer name" value={name} onChange={setName} placeholder="Customer name" />
        <Field label="Phone" value={phone} onChange={setPhone} placeholder="Phone" />
        <Field label="Time" type="datetime-local" value={time} onChange={setTime} min={editId ? undefined : nowLocal} />
        <Field label="Party size" value={party} onChange={v => setParty(v.replace(/[^0-9]/g, ''))} inputMode="numeric" placeholder="Party size" />
        <Combobox up className="col-span-2" label="Table" value={tableId} onChange={setTableId}
          placeholder="No table assigned" searchPlaceholder="Search tables…"
          options={[{ value: '', label: 'No table assigned' }, ...tables.map(t => ({ value: t.id, label: `${t.code} (${t.seats}p)` }))]} />
      </div>
    </Modal>
  );
}

function err(e: unknown, fallback: string): string {
  const m = (e as Error)?.message ?? '';
  const j = m.indexOf('{'); if (j !== -1) { try { const p = JSON.parse(m.slice(j)); if (typeof p?.error === 'string') return p.error; } catch { /* */ } }
  if (m.includes('403')) return 'Only an owner or manager can manage tables.';
  return m || fallback;
}
