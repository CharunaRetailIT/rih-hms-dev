'use client';

import { useEffect, useState } from 'react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient } from '@/lib/api-client';
import { Modal } from '@/components/ui/Modal';
import { confirmDialog } from '@/components/ui/confirm';
import { Field, Combobox } from '@/components/ui/form';
import { HeaderStat, Num } from '@/components/ui/HeaderStat';
import { Pagination } from '@/components/ui/Pagination';
import { Plus, Tablet, Ban, Copy, Check, Pencil, Trash2 } from 'lucide-react';

type Device = {
  id: string; name: string; lastSeenAt: string | null; isActive: boolean; locationId: string | null; createdAt: string;
};
type Loc = { id: string; code: string; name: string };
type PaginationMeta = { totalCount: number; pageNumber: number; pageSize: number; totalPages: number };
type ListResponse = {
  limit: number; used: number; available: number; guestQrEnabled: boolean;
  data: Device[]; pagination: PaginationMeta;
};

export default function TabDevicesPage() {
  const [devices, setDevices] = useState<Device[]>([]);
  const [locations, setLocations] = useState<Loc[]>([]);
  const [limit, setLimit] = useState(0);
  const [used, setUsed] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [toast, setToast] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<'' | 'active' | 'inactive'>('active');
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const [open, setOpen] = useState(false);
  const [name, setName] = useState('');
  const [locationId, setLocationId] = useState('');
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);

  // One-time pairing token, shown right after a successful registration.
  const [issued, setIssued] = useState<{ name: string; token: string } | null>(null);
  const [copied, setCopied] = useState(false);

  // Edit an existing device's name / outlet.
  const [editId, setEditId] = useState<string | null>(null);
  const [eName, setEName] = useState('');
  const [eLocationId, setELocationId] = useState('');
  const [eErrors, setEErrors] = useState<Record<string, string>>({});
  const [eSaving, setESaving] = useState(false);

  function flash(m: string) { setToast(m); window.setTimeout(() => setToast(null), 3500); }

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ pageNumber: String(pageNumber), pageSize: String(pageSize) });
      if (search.trim()) params.set('search', search.trim());
      if (statusFilter) params.set('isActive', String(statusFilter === 'active'));
      const res = await apiClient<ListResponse>(`/api/v1/tab-devices?${params.toString()}`);
      setDevices(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages);
      setLimit(res.limit);
      setUsed(res.used);
    } catch (e) { setError(extractError(e, 'Could not load tab devices.')); }
    finally { setLoading(false); }
  }
  useEffect(() => {
    apiClient<Loc[]>('/api/v1/locations?all=true').then(setLocations).catch(() => {});
  }, []);
  useEffect(() => { void load(); }, [pageNumber, pageSize, statusFilter]);
  useEffect(() => {
    const t = window.setTimeout(() => { setPageNumber(1); void load(); }, 350);
    return () => window.clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search]);

  function openModal() { setName(''); setLocationId(''); setFormErrors({}); setOpen(true); }

  function validate(): boolean {
    const errs: Record<string, string> = {};
    if (!name.trim()) errs.name = 'Give this handheld a name (e.g. Waiter 1).';
    setFormErrors(errs);
    return Object.keys(errs).length === 0;
  }

  async function submit() {
    if (!validate()) return;
    setSubmitting(true);
    try {
      const res = await apiClient<{ id: string; name: string; token: string; seatsUsed: number; limit: number }>(
        '/api/v1/tab-devices/register',
        { method: 'POST', body: JSON.stringify({ name: name.trim(), locationId: locationId || null }) },
      );
      setOpen(false);
      setIssued({ name: res.name, token: res.token });
      setCopied(false);
      await load();
    } catch (e) { flash(extractError(e, 'Could not register the device.')); }
    finally { setSubmitting(false); }
  }

  async function revoke(d: Device) {
    if (!(await confirmDialog({
      title: `Revoke ${d.name}?`,
      body: 'The handheld will be signed out and its seat freed. It can be re-paired later with a new device code.',
      confirmLabel: 'Revoke',
      danger: true,
    }))) return;
    setBusyId(d.id);
    try {
      await apiClient(`/api/v1/tab-devices/${d.id}/revoke`, { method: 'POST' });
      flash(`${d.name} revoked.`);
      await load();
    } catch (e) { flash(extractError(e, 'Could not revoke the device.')); }
    finally { setBusyId(null); }
  }

  function openEdit(d: Device) {
    setEditId(d.id); setEName(d.name); setELocationId(d.locationId ?? ''); setEErrors({});
  }
  async function saveEdit() {
    const errs: Record<string, string> = {};
    if (!eName.trim()) errs.eName = 'Name is required.';
    setEErrors(errs); if (Object.keys(errs).length) return;
    setESaving(true);
    try {
      await apiClient(`/api/v1/tab-devices/${editId}`, { method: 'PUT', body: JSON.stringify({ name: eName.trim(), locationId: eLocationId || null }) });
      setEditId(null); flash('Device updated.'); await load();
    } catch (e) { flash(extractError(e, 'Could not update the device.')); }
    finally { setESaving(false); }
  }

  async function remove(d: Device) {
    if (!(await confirmDialog({
      title: `Remove ${d.name}?`,
      body: 'This permanently removes the device record (not just revokes it). This cannot be undone.',
      confirmLabel: 'Remove',
      danger: true,
    }))) return;
    setBusyId(d.id);
    try {
      await apiClient(`/api/v1/tab-devices/${d.id}`, { method: 'DELETE' });
      flash(`${d.name} removed.`);
      await load();
    } catch (e) { flash(extractError(e, 'Could not remove the device.')); }
    finally { setBusyId(null); }
  }

  async function copyToken(token: string) {
    try { await navigator.clipboard.writeText(token); setCopied(true); window.setTimeout(() => setCopied(false), 2000); }
    catch { /* clipboard unavailable — the code is still visible to copy by hand */ }
  }

  return (
    <>
      <Topbar title="Tab Devices" subtitle="Register and manage waiter handhelds for Tab Ordering" />
      <div className="p-6 md:p-8">
        <div className="mb-4 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Tab Devices</h2>
            <HeaderStat><Num>{used}</Num> / {limit} seat{limit === 1 ? '' : 's'} used</HeaderStat>
          </div>
          <button onClick={openModal} disabled={limit > 0 && used >= limit}
            title={limit > 0 && used >= limit ? `All ${limit} device licence(s) are in use.` : undefined}
            className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark disabled:cursor-not-allowed disabled:opacity-50">
            <Plus className="size-4" /> Register Device
          </button>
        </div>

        <div className="mb-3 flex flex-wrap items-center gap-2">
          <input
            value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder="Search device name…"
            className="w-full max-w-xs rounded-lg border border-border bg-surface px-3 py-2 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20"
          />
          <div className="flex gap-1 rounded-lg border border-border bg-surface p-0.5 text-xs">
            {([['active', 'Active'], ['inactive', 'Revoked'], ['', 'All']] as const).map(([k, label]) => (
              <button key={k} onClick={() => { setStatusFilter(k); setPageNumber(1); }}
                className={`rounded-md px-2.5 py-1.5 font-semibold transition-colors ${statusFilter === k ? 'bg-primary text-primary-foreground' : 'text-muted-foreground hover:text-on-surface'}`}>
                {label}
              </button>
            ))}
          </div>
        </div>

        <div className="card overflow-x-auto">
          {loading ? (
            <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
          ) : error ? (
            <div className="p-6 text-sm text-status-error">{error}</div>
          ) : (
            <table className="w-full min-w-[680px] text-sm">
              <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th className="px-4 py-3 font-medium">Name</th>
                  <th className="px-4 py-3 font-medium">Outlet</th>
                  <th className="px-4 py-3 font-medium">Status</th>
                  <th className="px-4 py-3 font-medium">Last seen</th>
                  <th className="px-4 py-3 text-right font-medium">Actions</th>
                </tr>
              </thead>
              <tbody>
                {devices.map((d, i) => (
                  <tr key={d.id} className={i % 2 ? 'bg-muted/20' : ''}>
                    <td className="px-4 py-3 font-medium">{d.name}</td>
                    <td className="px-4 py-3 text-muted-foreground">
                      {d.locationId ? (locations.find(l => l.id === d.locationId)?.name ?? '—') : <span className="text-muted-foreground/60">Any outlet</span>}
                    </td>
                    <td className="px-4 py-3">
                      <span className={`pill ${d.isActive ? 'pill-paid' : 'pill-void'}`}>{d.isActive ? 'Active' : 'Revoked'}</span>
                    </td>
                    <td className="px-4 py-3 text-muted-foreground">
                      {d.lastSeenAt ? new Date(d.lastSeenAt).toLocaleDateString('en-LK', { year: 'numeric', month: 'short', day: 'numeric' }) : 'never'}
                    </td>
                    <td className="whitespace-nowrap px-4 py-3 text-right">
                      <div className="flex justify-end gap-1.5">
                        <IconBtn title="Edit" disabled={busyId === d.id} onClick={() => openEdit(d)}><Pencil className="size-4" /></IconBtn>
                        {d.isActive && (
                          <IconBtn title="Revoke" danger disabled={busyId === d.id} onClick={() => revoke(d)}><Ban className="size-4" /></IconBtn>
                        )}
                        <IconBtn title="Remove" danger disabled={busyId === d.id} onClick={() => remove(d)}><Trash2 className="size-4" /></IconBtn>
                      </div>
                    </td>
                  </tr>
                ))}
                {devices.length === 0 && (
                  <tr><td colSpan={5} className="px-4 py-10 text-center text-muted-foreground">No devices yet.</td></tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-surface px-2 py-1.5 text-xs"
          >
            {[10, 25, 50, 100].map(n => <option key={n} value={n}>{n} / page</option>)}
          </select>
          <Pagination
            page={pageNumber}
            totalPages={totalPages}
            total={totalCount}
            from={totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1}
            to={Math.min(pageNumber * pageSize, totalCount)}
            setPage={setPageNumber}
            noun="devices"
            className="mt-0 flex-1"
          />
        </div>

        <p className="mt-3 text-xs text-muted-foreground">Each active device consumes one licensed Tab Ordering seat. Pin a device to an outlet, or leave it unset so a waiter can use it at any outlet.</p>
      </div>

      {open && (
        <Modal
          title="Register Device"
          icon={<Tablet className="size-4" />}
          onClose={() => !submitting && setOpen(false)}
          size="md"
          footer={
            <div className="flex gap-2">
              <button onClick={() => setOpen(false)} disabled={submitting} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50">Cancel</button>
              <button onClick={submit} disabled={submitting} className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{submitting ? 'Registering…' : 'Register Device'}</button>
            </div>
          }
        >
          <p className="mb-4 text-sm text-muted-foreground">This consumes one Tab Ordering seat and generates a one-time device code — you&apos;ll enter it on the handheld to pair it.</p>
          <div className="space-y-4">
            <Field label="Device name" value={name} onChange={setName} placeholder="e.g. Waiter 1" error={formErrors.name} />
            <Combobox label="Outlet (optional)" value={locationId} onChange={setLocationId}
              placeholder="Any outlet…"
              options={locations.map(l => ({ value: l.id, label: `${l.code} — ${l.name}` }))} />
          </div>
        </Modal>
      )}

      {editId && (
        <Modal
          title="Edit Device"
          icon={<Pencil className="size-4" />}
          onClose={() => !eSaving && setEditId(null)}
          size="md"
          footer={
            <div className="flex gap-2">
              <button onClick={() => setEditId(null)} disabled={eSaving} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50">Cancel</button>
              <button onClick={saveEdit} disabled={eSaving} className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{eSaving ? 'Saving…' : 'Save changes'}</button>
            </div>
          }
        >
          <div className="space-y-4">
            <Field label="Device name" value={eName} onChange={setEName} placeholder="e.g. Waiter 1" error={eErrors.eName} />
            <Combobox label="Outlet (optional)" value={eLocationId} onChange={setELocationId}
              placeholder="Any outlet…"
              options={locations.map(l => ({ value: l.id, label: `${l.code} — ${l.name}` }))} />
          </div>
          <p className="mt-3 text-xs text-muted-foreground">Pairing token &amp; active status are managed from the table row.</p>
        </Modal>
      )}

      {issued && (
        <Modal
          title="Device Registered"
          icon={<Tablet className="size-4" />}
          onClose={() => setIssued(null)}
          size="md"
          footer={<button onClick={() => setIssued(null)} className="h-11 w-full rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark">Done</button>}
        >
          <p className="mb-3 text-sm text-muted-foreground">
            <strong>{issued.name}</strong> is registered. Enter this workspace and code on the handheld&apos;s
            &ldquo;Activate this handheld&rdquo; screen. <strong>This code is shown only once</strong> — copy it now.
          </p>
          <div className="flex items-center gap-2 rounded-lg border border-border bg-surface px-3 py-2.5">
            <code className="flex-1 select-all break-all font-mono text-sm">{issued.token}</code>
            <button onClick={() => copyToken(issued.token)} title="Copy code"
              className="grid size-8 shrink-0 place-items-center rounded-lg border border-border bg-card text-muted-foreground hover:bg-muted hover:text-on-surface">
              {copied ? <Check className="size-4 text-status-success" /> : <Copy className="size-4" />}
            </button>
          </div>
        </Modal>
      )}

      {toast && <div className="fixed bottom-12 left-1/2 z-[70] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">{toast}</div>}
    </>
  );
}

function IconBtn({ title, onClick, disabled, danger, children }: { title: string; onClick: () => void; disabled?: boolean; danger?: boolean; children: React.ReactNode }) {
  return (
    <button type="button" title={title} aria-label={title} onClick={onClick} disabled={disabled}
      className={`grid size-8 shrink-0 place-items-center rounded-lg border border-border bg-card transition-colors disabled:opacity-50 ${danger ? 'text-status-error hover:bg-status-error/10' : 'text-muted-foreground hover:bg-muted hover:text-on-surface'}`}>
      {children}
    </button>
  );
}

function extractError(e: unknown, fallback: string): string {
  if ((e as { status?: number })?.status === 403) return 'Only an Owner, Admin, Manager or Accountant can manage tab devices.';
  const msg = (e as Error)?.message ?? '';
  const jsonStart = msg.indexOf('{');
  if (jsonStart !== -1) {
    try {
      const parsed = JSON.parse(msg.slice(jsonStart));
      if (typeof parsed?.error === 'string') return parsed.error;
    } catch { /* fall through */ }
  }
  return msg || fallback;
}
