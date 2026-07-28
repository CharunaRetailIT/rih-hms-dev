'use client';

import { useEffect, useState } from 'react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient } from '@/lib/api-client';
import { Modal } from '@/components/ui/Modal';
import { confirmDialog, promptDialog } from '@/components/ui/confirm';
import { Field, Combobox } from '@/components/ui/form';
import { HeaderStat, Num } from '@/components/ui/HeaderStat';
import { Plus, UserPlus, Pencil, KeyRound, UserX, UserCheck, Trash2, Layers } from 'lucide-react';
import { Pagination } from '@/components/ui/Pagination';

type Member = {
  id: string; email: string | null; username: string | null; displayName: string; role: number;
  homeLocationId: string | null;
  isActive: boolean; isServer?: boolean; lastLoginAt: string | null; phoneE164: string | null; hasPin?: boolean;
};
type Loc = { id: string; code: string; name: string };
type PaginationMeta = { totalCount: number; pageNumber: number; pageSize: number; totalPages: number };
type PagedResponse = { data: Member[]; pagination: PaginationMeta };

const ROLES = [
  { id: 0, label: 'Owner', hint: 'Full access incl. team & settings' },
  { id: 5, label: 'Admin', hint: 'Owner-level — except removing the owner & editing business info' },
  { id: 1, label: 'Manager', hint: 'Operations, supply chain, reports' },
  { id: 2, label: 'Cashier', hint: 'POS, orders, shifts, delivery' },
  { id: 3, label: 'Kitchen', hint: 'Kitchen display (KOT) only' },
  { id: 4, label: 'Accountant', hint: 'Reports & settings — no POS' },
];
const ROLE_LABEL = (r: number) => ROLES.find(x => x.id === r)?.label ?? '—';

export default function TeamPage() {
  const [members, setMembers] = useState<Member[]>([]);
  const [locations, setLocations] = useState<Loc[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [toast, setToast] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [tab, setTab] = useState<'members' | 'roles' | 'access'>('members');
  const [search, setSearch] = useState('');
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const [open, setOpen] = useState(false);
  const [email, setEmail] = useState('');
  const [name, setName] = useState('');
  const [role, setRole] = useState<number | ''>('');
  const [pin, setPin] = useState('');
  const [username, setUsername] = useState('');
  const [isServer, setIsServer] = useState(false);
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);

  // Edit an existing member's details (name / email / username / phone).
  const [editId, setEditId] = useState<string | null>(null);
  const [eName, setEName] = useState(''); const [eEmail, setEEmail] = useState('');
  const [eUsername, setEUsername] = useState(''); const [ePhone, setEPhone] = useState('');
  const [eErrors, setEErrors] = useState<Record<string, string>>({});
  const [eSaving, setESaving] = useState(false);

  function openEdit(m: Member) {
    setEditId(m.id); setEName(m.displayName); setEEmail(m.email ?? ''); setEUsername(m.username ?? ''); setEPhone(m.phoneE164 ?? ''); setEErrors({});
  }

  // Which floors a steward covers — only meaningful once they're pinned to one outlet
  // (a floor belongs to a location). Drives floor-scoped notification routing (#floor-push).
  const [floorsForId, setFloorsForId] = useState<string | null>(null);
  const [floorOptions, setFloorOptions] = useState<{ id: string; name: string }[]>([]);
  const [selectedFloorIds, setSelectedFloorIds] = useState<Set<string>>(new Set());
  const [floorsSaving, setFloorsSaving] = useState(false);

  async function openFloors(m: Member) {
    if (!m.homeLocationId) return;
    setFloorsForId(m.id);
    try {
      const [opts, assigned] = await Promise.all([
        apiClient<{ id: string; name: string }[]>(`/api/v1/floors?locationId=${m.homeLocationId}`),
        apiClient<string[]>(`/api/v1/users/${m.id}/floors`),
      ]);
      setFloorOptions(opts); setSelectedFloorIds(new Set(assigned));
    } catch (e) { flash(extractError(e, 'Could not load floors.')); setFloorsForId(null); }
  }
  async function saveFloors() {
    if (!floorsForId) return;
    setFloorsSaving(true);
    try {
      await apiClient(`/api/v1/users/${floorsForId}/floors`, { method: 'PUT', body: JSON.stringify({ floorIds: Array.from(selectedFloorIds) }) });
      setFloorsForId(null); flash('Floor coverage updated.');
    } catch (e) { flash(extractError(e, 'Could not save floor coverage.')); }
    finally { setFloorsSaving(false); }
  }
  async function saveEdit() {
    const errs: Record<string, string> = {};
    if (!eName.trim()) errs.eName = 'Name is required.';
    if (eEmail.trim() && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(eEmail.trim())) errs.eEmail = 'Enter a valid email address.';
    if (eUsername.trim() && !/^[a-z0-9._-]{2,}$/i.test(eUsername.trim())) errs.eUsername = 'Letters, numbers, . _ - only.';
    setEErrors(errs); if (Object.keys(errs).length) return;
    setESaving(true);
    try {
      await apiClient(`/api/v1/users/${editId}`, { method: 'PUT', body: JSON.stringify({ displayName: eName.trim(), email: eEmail.trim() || null, username: eUsername.trim() || null, phoneE164: ePhone.trim() || null }) });
      setEditId(null); flash('Member updated.'); await load();
    } catch (e) { flash(extractError(e, 'Could not update the member.')); }
    finally { setESaving(false); }
  }

  function flash(m: string) { setToast(m); window.setTimeout(() => setToast(null), 3500); }

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ pageNumber: String(pageNumber), pageSize: String(pageSize) });
      if (search.trim()) params.set('search', search.trim());
      const res = await apiClient<PagedResponse>(`/api/v1/users/paged?${params.toString()}`);
      setMembers(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages);
    }
    catch (e) { setError(extractError(e, 'Could not load the team.')); }
    finally { setLoading(false); }
  }
  useEffect(() => {
    apiClient<Loc[]>('/api/v1/locations?all=true').then(setLocations).catch(() => {});
  }, []);
  useEffect(() => { void load(); }, [pageNumber, pageSize]);
  useEffect(() => {
    const t = window.setTimeout(() => { setPageNumber(1); void load(); }, 350);
    return () => window.clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search]);

  // Pin a member to an outlet (or Head office = all access). null = HQ.
  async function setHomeLocation(m: Member, locationId: string) {
    setBusyId(m.id);
    try {
      await apiClient(`/api/v1/users/${m.id}/home-location`, { method: 'PUT', body: JSON.stringify({ locationId: locationId || null }) });
      flash(locationId ? `${m.displayName} pinned to ${locations.find(l => l.id === locationId)?.code ?? 'outlet'}.` : `${m.displayName} now has head-office (all) access.`);
      await load();
    } catch (e) { flash(extractError(e, 'Could not set the outlet.')); await load(); }
    finally { setBusyId(null); }
  }

  function openModal() { setEmail(''); setName(''); setRole(''); setPin(''); setUsername(''); setIsServer(false); setFormErrors({}); setOpen(true); }

  function validate(): boolean {
    const errs: Record<string, string> = {};
    if (!name.trim()) errs.name = 'Name is required.';
    if (role === '') errs.role = 'Choose a role.';
    // Email is optional (POS staff may have none) — but they need a way in: email
    // OR username+PIN. Exception: a server-only record (a waiter who never logs in).
    if (email.trim() && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) errs.email = 'Enter a valid email address.';
    if (!email.trim() && !pin.trim() && !isServer) errs.email = 'Give them an email or a username + PIN — or mark them server-only.';
    if (pin.trim() && !/^\d{4,8}$/.test(pin.trim())) errs.pin = 'PIN must be 4–8 digits.';
    if (pin.trim() && !username.trim()) errs.username = 'A username is required for PIN sign-in.';
    if (username.trim() && !/^[a-z0-9._-]{2,}$/i.test(username.trim())) errs.username = 'Letters, numbers, . _ - only.';
    setFormErrors(errs);
    return Object.keys(errs).length === 0;
  }

  async function submit() {
    if (!validate()) return;
    setSubmitting(true);
    try {
      await apiClient('/api/v1/users', { method: 'POST', body: JSON.stringify({ email: email.trim() || null, username: username.trim() || null, displayName: name.trim(), role: Number(role), pin: pin.trim() || null, isServer }) });
      setOpen(false); flash('Team member added.'); await load();
    } catch (e) { flash(extractError(e, 'Could not add the member.')); }
    finally { setSubmitting(false); }
  }

  async function setMemberPin(m: Member) {
    const entered = await promptDialog({
      title: 'Set POS PIN', body: `Set a POS login PIN for ${m.displayName} (4–8 digits). Leave blank to clear it.`,
      placeholder: '••••', inputMode: 'numeric', maxLength: 8, password: true, confirmLabel: 'Save PIN',
    });
    if (entered === null) return;
    const p = entered.trim();
    if (p && !/^\d{4,8}$/.test(p)) { flash('PIN must be 4–8 digits.'); return; }
    setBusyId(m.id);
    try { await apiClient(`/api/v1/users/${m.id}/pin`, { method: 'PUT', body: JSON.stringify({ pin: p || null }) }); flash(p ? `PIN set for ${m.displayName}.` : `PIN cleared for ${m.displayName}.`); await load(); }
    catch (e) { flash(extractError(e, 'Could not set the PIN.')); }
    finally { setBusyId(null); }
  }

  async function changeRole(m: Member, newRole: number) {
    setBusyId(m.id);
    try { await apiClient(`/api/v1/users/${m.id}`, { method: 'PUT', body: JSON.stringify({ role: newRole }) }); flash(`${m.displayName} is now ${ROLE_LABEL(newRole)}.`); await load(); }
    catch (e) { flash(extractError(e, 'Could not change the role.')); await load(); }
    finally { setBusyId(null); }
  }

  async function toggleActive(m: Member) {
    if (m.isActive && !(await confirmDialog({
      title: `Deactivate ${m.displayName}?`,
      body: 'They will no longer be able to sign in or be assigned to bills. You can reactivate them later.',
      confirmLabel: 'Deactivate',
      danger: true,
    }))) return;
    setBusyId(m.id);
    try { await apiClient(`/api/v1/users/${m.id}`, { method: 'PUT', body: JSON.stringify({ isActive: !m.isActive }) }); flash(`${m.displayName} ${m.isActive ? 'deactivated' : 'reactivated'}.`); await load(); }
    catch (e) { flash(extractError(e, 'Could not update the member.')); }
    finally { setBusyId(null); }
  }

  // #76: mark a user as assignable to a bill as the steward/server (POS dropdown).
  async function toggleServer(m: Member) {
    setBusyId(m.id);
    try { await apiClient(`/api/v1/users/${m.id}`, { method: 'PUT', body: JSON.stringify({ isServer: !m.isServer }) }); flash(`${m.displayName} ${m.isServer ? 'is no longer a steward' : 'can now be assigned as a steward'}.`); await load(); }
    catch (e) { flash(extractError(e, 'Could not update the member.')); }
    finally { setBusyId(null); }
  }

  async function remove(m: Member) {
    if (!(await confirmDialog({
      title: `Remove ${m.displayName}?`,
      body: 'This removes the team member and their access. This cannot be undone.',
      confirmLabel: 'Remove',
      danger: true,
    }))) return;
    setBusyId(m.id);
    try { await apiClient(`/api/v1/users/${m.id}`, { method: 'DELETE' }); flash(`${m.displayName} removed.`); await load(); }
    catch (e) { flash(extractError(e, 'Could not remove the member.')); }
    finally { setBusyId(null); }
  }

  return (
    <>
      <Topbar title="Team" subtitle="Bring your people on board and control what each can do" />
      <div className="p-6 md:p-8">
        <div className="mb-4 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Team</h2>
            <HeaderStat><Num>{totalCount}</Num> users · roles control what each person can access</HeaderStat>
          </div>
          {tab === 'members' && (
            <button onClick={openModal} className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark">
              <Plus className="size-4" /> Add Member
            </button>
          )}
        </div>

        <div className="mb-5 flex gap-1 border-b border-border">
          {([['members', 'Members'], ['roles', 'Roles & Permissions'], ['access', 'Screen Access']] as const).map(([k, label]) => (
            <button key={k} onClick={() => setTab(k)}
              className={`-mb-px border-b-2 px-4 py-2 text-sm font-semibold transition-colors ${tab === k ? 'border-primary text-primary' : 'border-transparent text-muted-foreground hover:text-on-surface'}`}>
              {label}
            </button>
          ))}
        </div>

        {tab === 'members' && (<>
        <div className="mb-3 flex items-center gap-2">
          <input
            value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder="Search name, email, or username…"
            className="w-full max-w-xs rounded-lg border border-border bg-surface px-3 py-2 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20"
          />
        </div>
        <div className="card overflow-x-auto">
          {loading ? (
            <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
          ) : error ? (
            <div className="p-6 text-sm text-status-error">{error}</div>
          ) : (
            <table className="w-full min-w-[920px] text-sm">
              <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th className="px-4 py-3 font-medium">Name</th>
                  <th className="px-4 py-3 font-medium">Email</th>
                  <th className="px-4 py-3 font-medium">Role</th>
                  <th className="px-4 py-3 font-medium" title="Pin to one outlet (branch staff only see their outlet). Head office = all access.">Outlet</th>
                  <th className="px-4 py-3 text-center font-medium" title="Can be assigned as the steward (server) on a bill">Steward</th>
                  <th className="px-4 py-3 font-medium">Status</th>
                  <th className="px-4 py-3 font-medium">Last login</th>
                  <th className="px-4 py-3 text-right font-medium">Actions</th>
                </tr>
              </thead>
              <tbody>
                {members.map((m, i) => (
                  <tr key={m.id} className={i % 2 ? 'bg-muted/20' : ''}>
                    <td className="px-4 py-3 font-medium">{m.displayName}</td>
                    <td className="px-4 py-3 font-mono text-xs text-muted-foreground">
                      {m.email ?? (m.username ? <span>@{m.username}</span> : <span className="not-italic text-muted-foreground/60">— no email —</span>)}
                      {m.hasPin && <span className="pill pill-idle ml-2">PIN</span>}
                    </td>
                    <td className="px-4 py-2.5">
                      {/* A login-less steward record (no email/username/PIN) has no
                          meaningful permission role — show a Steward pill, not a dropdown. */}
                      {(!m.email && !m.username && !m.hasPin && m.isServer) ? (
                        <span className="pill pill-idle" title="Steward-only record — no login, no permissions">Steward</span>
                      ) : (
                        <select
                          value={m.role}
                          disabled={busyId === m.id}
                          onChange={e => changeRole(m, Number(e.target.value))}
                          className={`rounded-lg border border-border bg-surface px-2 py-1 text-xs focus:border-primary focus:ring-2 focus:ring-primary/20`}
                        >
                          {ROLES.map(r => <option key={r.id} value={r.id}>{r.label}</option>)}
                        </select>
                      )}
                    </td>
                    <td className="px-4 py-2.5">
                      {(!m.email && !m.username && !m.hasPin && m.isServer) ? (
                        <span className="text-xs text-muted-foreground/60">—</span>
                      ) : m.role === 0 ? (
                        <span className="text-xs text-muted-foreground" title="Owners always have all-outlet access">All (HQ)</span>
                      ) : (
                        <select value={m.homeLocationId ?? ''} disabled={busyId === m.id}
                          onChange={e => setHomeLocation(m, e.target.value)}
                          className="rounded-lg border border-border bg-surface px-2 py-1 text-xs focus:border-primary focus:ring-2 focus:ring-primary/20"
                          title="Head office = sees all outlets; pick an outlet to lock this user to it">
                          <option value="">Head office (all)</option>
                          {locations.map(l => <option key={l.id} value={l.id}>{l.code} — {l.name}</option>)}
                        </select>
                      )}
                    </td>
                    <td className="px-4 py-2.5 text-center">
                      <input type="checkbox" checked={!!m.isServer} disabled={busyId === m.id}
                        onChange={() => toggleServer(m)} title="Can be assigned as the steward (server) on a bill"
                        className="size-4 rounded border-border text-primary focus:ring-primary" />
                    </td>
                    <td className="px-4 py-3">
                      <span className={`pill ${m.isActive ? 'pill-paid' : 'pill-void'}`}>{m.isActive ? 'Active' : 'Inactive'}</span>
                    </td>
                    <td className="px-4 py-3 text-muted-foreground">
                      {m.lastLoginAt ? new Date(m.lastLoginAt).toLocaleDateString('en-LK', { year: 'numeric', month: 'short', day: 'numeric' }) : 'never'}
                    </td>
                    <td className="whitespace-nowrap px-4 py-3 text-right">
                      <div className="flex justify-end gap-1.5">
                        <IconBtn title="Edit details" disabled={busyId === m.id} onClick={() => openEdit(m)}><Pencil className="size-4" /></IconBtn>
                        {m.isServer && (
                          <IconBtn title={m.homeLocationId ? 'Floor coverage' : 'Pin to an outlet first to assign floors'} disabled={busyId === m.id || !m.homeLocationId} onClick={() => openFloors(m)}><Layers className="size-4" /></IconBtn>
                        )}
                        <IconBtn title={m.hasPin ? 'Reset PIN' : 'Set PIN'} disabled={busyId === m.id} onClick={() => setMemberPin(m)}><KeyRound className="size-4" /></IconBtn>
                        <IconBtn title={m.isActive ? 'Deactivate' : 'Reactivate'} disabled={busyId === m.id} onClick={() => toggleActive(m)}>{m.isActive ? <UserX className="size-4" /> : <UserCheck className="size-4" />}</IconBtn>
                        <IconBtn title="Remove" danger disabled={busyId === m.id} onClick={() => remove(m)}><Trash2 className="size-4" /></IconBtn>
                      </div>
                    </td>
                  </tr>
                ))}
                {members.length === 0 && (
                  <tr><td colSpan={8} className="px-4 py-10 text-center text-muted-foreground">No team members yet.</td></tr>
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
            noun="users"
            className="mt-0 flex-1"
          />
        </div>

        <p className="mt-3 text-xs text-muted-foreground">Roles map to access: Owner (all) · Manager (ops/supply/reports) · Cashier (POS) · Kitchen (KOT) · Accountant (reports/settings).</p>
        </>)}

        {tab === 'roles' && <RolePermissionsCard flash={flash} />}
        {tab === 'access' && <ScreenAccessCard flash={flash} />}
      </div>

      {open && (
        <Modal
          title="Add Team Member"
          icon={<UserPlus className="size-4" />}
          onClose={() => !submitting && setOpen(false)}
          size="md"
          footer={
            <div className="flex gap-2">
              <button onClick={() => setOpen(false)} disabled={submitting} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50">Cancel</button>
              <button onClick={submit} disabled={submitting} className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{submitting ? 'Adding…' : 'Add Member'}</button>
            </div>
          }
        >
            <p className="mb-4 text-sm text-muted-foreground">Email → magic-link sign-in. No email → set a PIN for POS login.</p>
            <div className="space-y-4">
              <Field label="Full name" value={name} onChange={v => { setName(v); if (!username.trim() || username === name.toLowerCase().replace(/[^a-z0-9]/g, '')) setUsername(v.toLowerCase().replace(/[^a-z0-9]/g, '')); }} placeholder="e.g. Asela Perera" error={formErrors.name} />
              <Field label="Email (optional)" value={email} onChange={v => setEmail(v)} placeholder="name@restaurant.lk" error={formErrors.email} />
              <Field label="Username (for PIN sign-in)" value={username} onChange={v => setUsername(v)} placeholder="e.g. asela" error={formErrors.username} />
              <Field label="POS PIN (optional, 4–8 digits)" value={pin} onChange={v => setPin(v.replace(/\D/g, ''))} inputMode="numeric" placeholder="e.g. 2468" error={formErrors.pin} />
              <Combobox up label="Role" value={role === '' ? '' : String(role)} onChange={v => setRole(v === '' ? '' : Number(v))}
                placeholder="Select a role…" error={formErrors.role}
                options={ROLES.map(r => ({ value: String(r.id), label: `${r.label} — ${r.hint}` }))} />
              <label className="flex items-start gap-2.5 rounded-lg border border-border bg-surface px-3 py-2.5">
                <input type="checkbox" checked={isServer} onChange={e => setIsServer(e.target.checked)} className="mt-0.5 size-4 rounded border-border text-primary focus:ring-primary" />
                <span className="text-sm">
                  <span className="font-semibold text-slate-700">Can be assigned as a steward</span>
                  <span className="block text-xs text-muted-foreground">Appears in the POS &ldquo;steward / waiter&rdquo; list for tips &amp; sales attribution. A waiter who never signs in can be a steward-only record (no email/PIN needed).</span>
                </span>
              </label>
            </div>
        </Modal>
      )}

      {editId && (
        <Modal
          title="Edit Team Member"
          icon={<UserPlus className="size-4" />}
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
            <Field label="Full name" value={eName} onChange={v => setEName(v)} placeholder="e.g. Asela Perera" error={eErrors.eName} />
            <Field label="Email" value={eEmail} onChange={v => setEEmail(v)} placeholder="name@restaurant.lk" error={eErrors.eEmail} />
            <Field label="Username (for PIN sign-in)" value={eUsername} onChange={v => setEUsername(v)} placeholder="e.g. asela" error={eErrors.eUsername} />
            <Field label="Phone" value={ePhone} onChange={v => setEPhone(v)} placeholder="+94 7X XXX XXXX" />
          </div>
          <p className="mt-3 text-xs text-muted-foreground">Role, outlet, PIN, steward &amp; status are managed from the table row.</p>
        </Modal>
      )}

      {floorsForId && (
        <Modal
          title="Floor Coverage"
          icon={<Layers className="size-4" />}
          onClose={() => !floorsSaving && setFloorsForId(null)}
          size="sm"
          footer={
            <div className="flex gap-2">
              <button onClick={() => setFloorsForId(null)} disabled={floorsSaving} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50">Cancel</button>
              <button onClick={saveFloors} disabled={floorsSaving} className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{floorsSaving ? 'Saving…' : 'Save'}</button>
            </div>
          }
        >
          <p className="mb-3 text-sm text-muted-foreground">Which floors is {members.find(m => m.id === floorsForId)?.displayName} responsible for? Leave all unchecked to cover every floor.</p>
          {floorOptions.length === 0 ? (
            <p className="text-sm text-muted-foreground">No floors set up for this outlet yet — add one from Floor → Manage Tables.</p>
          ) : (
            <div className="space-y-1.5">
              {floorOptions.map(f => (
                <label key={f.id} className="flex items-center gap-2.5 rounded-lg border border-border px-3 py-2 text-sm">
                  <input type="checkbox" checked={selectedFloorIds.has(f.id)}
                    onChange={e => setSelectedFloorIds(prev => { const n = new Set(prev); if (e.target.checked) n.add(f.id); else n.delete(f.id); return n; })}
                    className="size-4 rounded border-border text-primary focus:ring-primary" />
                  {f.name}
                </label>
              ))}
            </div>
          )}
        </Modal>
      )}

      {toast && <div className="fixed bottom-12 left-1/2 z-[70] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">{toast}</div>}
    </>
  );
}

type Perm = { role: number; roleName: string; maxDiscountPercent: number; canApplyDiscount: boolean; canVoid: boolean; canComp: boolean };

function RolePermissionsCard({ flash }: { flash: (m: string) => void }) {
  const [perms, setPerms] = useState<Perm[]>([]);
  const [loading, setLoading] = useState(true);
  const [savingRole, setSavingRole] = useState<number | null>(null);

  useEffect(() => {
    apiClient<Perm[]>('/api/v1/permissions').then(setPerms).catch(() => {}).finally(() => setLoading(false));
  }, []);
  const patch = (role: number, p: Partial<Perm>) => setPerms(ps => ps.map(x => (x.role === role ? { ...x, ...p } : x)));
  async function save(p: Perm) {
    setSavingRole(p.role);
    try {
      await apiClient('/api/v1/permissions', {
        method: 'PUT',
        body: JSON.stringify({
          role: p.role, maxDiscountPercent: Number(p.maxDiscountPercent) || 0,
          canApplyDiscount: p.canApplyDiscount, canVoid: p.canVoid, canComp: p.canComp,
        }),
      });
      flash(`${p.roleName} limits saved.`);
    } catch (e) { flash(extractError(e, 'Could not save permissions.')); }
    finally { setSavingRole(null); }
  }

  return (
    <div>
      <h2 className="font-heading text-xl font-bold">Role Permissions</h2>
      <p className="mb-4 text-sm text-muted-foreground">POS limits per role. Owner always has full access; over-limit discounts &amp; voids are blocked and need a manager.</p>
      <div className="card overflow-hidden">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 4 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-3 font-medium">Role</th>
                <th className="px-4 py-3 font-medium">Max discount %</th>
                <th className="px-4 py-3 text-center font-medium">Discount</th>
                <th className="px-4 py-3 text-center font-medium">Void</th>
                <th className="px-4 py-3 text-center font-medium">Comp / no-sale</th>
                <th className="px-4 py-3 text-right font-medium"></th>
              </tr>
            </thead>
            <tbody>
              {perms.map((p, i) => (
                <tr key={p.role} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="px-4 py-3 font-medium">{p.roleName}</td>
                  <td className="px-4 py-2.5">
                    <input value={p.maxDiscountPercent}
                      onChange={e => patch(p.role, { maxDiscountPercent: Number(e.target.value.replace(/[^0-9.]/g, '')) || 0 })}
                      inputMode="decimal" className="w-20 rounded border border-border bg-surface px-2 py-1 tabular-nums" /> %
                  </td>
                  <td className="px-4 py-2.5 text-center"><input type="checkbox" checked={p.canApplyDiscount} onChange={e => patch(p.role, { canApplyDiscount: e.target.checked })} className="size-4 rounded" /></td>
                  <td className="px-4 py-2.5 text-center"><input type="checkbox" checked={p.canVoid} onChange={e => patch(p.role, { canVoid: e.target.checked })} className="size-4 rounded" /></td>
                  <td className="px-4 py-2.5 text-center"><input type="checkbox" checked={p.canComp} onChange={e => patch(p.role, { canComp: e.target.checked })} className="size-4 rounded" /></td>
                  <td className="px-4 py-2.5 text-right">
                    <button onClick={() => save(p)} disabled={savingRole === p.role}
                      className="rounded-lg bg-primary px-3 py-1.5 text-xs font-semibold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">
                      {savingRole === p.role ? 'Saving…' : 'Save'}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}

const SCREENS: { href: string; label: string }[] = [
  { href: '/dashboard', label: 'Dashboard' }, { href: '/floor', label: 'Floor' }, { href: '/delivery', label: 'Delivery' },
  { href: '/menu', label: 'Menu' }, { href: '/modifiers', label: 'Add-ons' }, { href: '/customers', label: 'Customers' },
  { href: '/promotions', label: 'Promotions' }, { href: '/catering', label: 'Catering' }, { href: '/inventory', label: 'Inventory' }, { href: '/suppliers', label: 'Suppliers' },
  { href: '/purchasing', label: 'Purchasing' }, { href: '/production', label: 'Production' }, { href: '/transfers', label: 'Transfers' },
  { href: '/stock-count', label: 'Stock count' }, { href: '/reports', label: 'Reports' }, { href: '/transactions', label: 'Transactions' }, { href: '/accounting', label: 'Accounting' }, { href: '/audit', label: 'Activity log' },
  { href: '/settings', label: 'Settings' },
];
const SCREEN_ROLES: { id: number; name: string }[] = [
  { id: 1, name: 'Manager' }, { id: 2, name: 'Cashier' }, { id: 3, name: 'Kitchen' }, { id: 4, name: 'Accountant' },
];

function ScreenAccessCard({ flash }: { flash: (m: string) => void }) {
  const [denied, setDenied] = useState<Set<string>>(new Set());   // "role:href" keys that are hidden
  const [loading, setLoading] = useState(true);

  async function load() {
    try {
      const rows = await apiClient<{ role: number; screen: string; allowed: boolean }[]>('/api/v1/permissions/screens');
      setDenied(new Set(rows.filter(r => !r.allowed).map(r => `${r.role}:${r.screen}`)));
    } catch { /* */ } finally { setLoading(false); }
  }
  useEffect(() => { void load(); }, []);

  async function toggle(role: number, href: string, allow: boolean) {
    const key = `${role}:${href}`;
    setDenied(prev => { const n = new Set(prev); if (allow) n.delete(key); else n.add(key); return n; });
    try { await apiClient('/api/v1/permissions/screens', { method: 'PUT', body: JSON.stringify({ role, screen: href, allowed: allow }) }); }
    catch (e) { flash(extractError(e, 'Could not save screen access.')); void load(); }
  }

  return (
    <div className="card p-5">
      <h2 className="font-heading text-xl font-bold">Screen Access</h2>
      <p className="mb-4 text-sm text-muted-foreground">Hide whole screens from a role (on top of their built-in access). Owner always sees everything.</p>
      {loading ? <div className="h-32 animate-pulse rounded bg-muted" /> : (
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="border-b border-border text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr><th className="py-2 pr-4">Screen</th>{SCREEN_ROLES.map(r => <th key={r.id} className="px-2 py-2 text-center">{r.name}</th>)}</tr>
            </thead>
            <tbody>
              {SCREENS.map(s => (
                <tr key={s.href} className="border-b border-border/40">
                  <td className="py-1.5 pr-4">{s.label}</td>
                  {SCREEN_ROLES.map(r => {
                    const allowed = !denied.has(`${r.id}:${s.href}`);
                    return (
                      <td key={r.id} className="px-2 py-1.5 text-center">
                        <input type="checkbox" checked={allowed} onChange={e => toggle(r.id, s.href, e.target.checked)} className="size-4 rounded border-border text-primary focus:ring-primary" />
                      </td>
                    );
                  })}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
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
  // apiClient sets .status on the thrown Error; a substring check on the
  // message text never matches since 403s get a generic friendly message
  // ("You don't have permission to do that.") with no literal "403" in it.
  if ((e as { status?: number })?.status === 403) return 'Only an Owner can manage the team.';
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
