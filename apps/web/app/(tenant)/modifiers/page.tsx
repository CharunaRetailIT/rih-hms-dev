'use client';

import { useEffect, useState } from 'react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient, lkr } from '@/lib/api-client';
import { Modal } from '@/components/ui/Modal';
import { confirmDialog } from '@/components/ui/confirm';
import { Field } from '@/components/ui/form';
import { Plus, X, ListPlus } from 'lucide-react';
import { HeaderStat, Num } from '@/components/ui/HeaderStat';
import { Pagination } from '@/components/ui/Pagination';

type Item = { id?: string; name: string; priceDelta: number; isDefault: boolean };
type Group = {
  id: string; name: string; minSelect: number; maxSelect: number; isRequired: boolean;
  sortOrder: number; isActive: boolean; items: Item[];
};
type ItemRow = { name: string; price: string; isDefault: boolean };
type PaginationMeta = { totalCount: number; pageNumber: number; pageSize: number; totalPages: number };
type PagedResponse = { data: Group[]; pagination: PaginationMeta };

export default function ModifiersPage() {
  const [groups, setGroups] = useState<Group[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [toast, setToast] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  const [search, setSearch] = useState('');
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const [open, setOpen] = useState(false);
  const [editId, setEditId] = useState<string | null>(null);
  const [name, setName] = useState('');
  const [required, setRequired] = useState(false);
  const [minSelect, setMinSelect] = useState('0');
  const [maxSelect, setMaxSelect] = useState('0');
  const [rows, setRows] = useState<ItemRow[]>([{ name: '', price: '', isDefault: false }]);
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);

  function flash(m: string) { setToast(m); window.setTimeout(() => setToast(null), 3500); }

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ pageNumber: String(pageNumber), pageSize: String(pageSize) });
      if (search.trim()) params.set('search', search.trim());
      const res = await apiClient<PagedResponse>(`/api/v1/modifier-groups/paged?${params.toString()}`);
      setGroups(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages);
    }
    catch (e) { setError(extractError(e, 'Could not load add-on groups.')); }
    finally { setLoading(false); }
  }
  useEffect(() => { void load(); }, [pageNumber, pageSize]);
  useEffect(() => {
    const t = window.setTimeout(() => { setPageNumber(1); void load(); }, 350);
    return () => window.clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search]);

  function openNew() {
    setEditId(null); setName(''); setRequired(false); setMinSelect('0'); setMaxSelect('0');
    setRows([{ name: '', price: '', isDefault: false }]); setFormErrors({}); setOpen(true);
  }
  function openEdit(g: Group) {
    setEditId(g.id); setName(g.name); setRequired(g.isRequired);
    setMinSelect(String(g.minSelect)); setMaxSelect(String(g.maxSelect));
    setRows(g.items.length ? g.items.map(i => ({ name: i.name, price: String(i.priceDelta), isDefault: i.isDefault }))
      : [{ name: '', price: '', isDefault: false }]);
    setFormErrors({}); setOpen(true);
  }
  function addRow() { setRows(r => [...r, { name: '', price: '', isDefault: false }]); }
  function removeRow(idx: number) { setRows(r => (r.length === 1 ? r : r.filter((_, i) => i !== idx))); }
  function updateRow(idx: number, patch: Partial<ItemRow>) { setRows(r => r.map((x, i) => (i === idx ? { ...x, ...patch } : x))); }

  function validate(): boolean {
    const e: Record<string, string> = {};
    if (!name.trim()) e.name = 'Name is required.';
    const min = Number(minSelect), max = Number(maxSelect);
    if (Number.isNaN(min) || min < 0) e.min = 'Min must be ≥ 0.';
    if (Number.isNaN(max) || max < 0) e.max = 'Max must be ≥ 0.';
    if (max > 0 && min > max) e.max = 'Min cannot exceed max.';
    const clean = rows.filter(r => r.name.trim());
    if (clean.length === 0) e.items = 'Add at least one option.';
    else for (const r of clean) if (r.price.trim() !== '' && Number.isNaN(Number(r.price))) { e.items = 'Prices must be numbers.'; break; }
    setFormErrors(e);
    return Object.keys(e).length === 0;
  }

  async function submit() {
    if (!validate()) return;
    setSubmitting(true);
    const payload = {
      id: editId,
      name: name.trim(),
      minSelect: Number(minSelect),
      maxSelect: Number(maxSelect),
      isRequired: required,
      sortOrder: 0,
      items: rows.filter(r => r.name.trim()).map((r, i) => ({
        name: r.name.trim(), priceDelta: r.price.trim() === '' ? 0 : Number(r.price), isDefault: r.isDefault, sortOrder: i,
      })),
    };
    try {
      await apiClient('/api/v1/modifier-groups', { method: 'PUT', body: JSON.stringify(payload) });
      setOpen(false); flash(editId ? 'Group updated.' : 'Group created.'); await load();
    } catch (e) { flash(extractError(e, 'Could not save the group.')); }
    finally { setSubmitting(false); }
  }

  async function remove(g: Group) {
    if (!(await confirmDialog({ title: `Remove ${g.name}?`, body: 'This add-on group and its options will be deleted. This cannot be undone.', confirmLabel: 'Remove', danger: true }))) return;
    setBusyId(g.id);
    try { await apiClient(`/api/v1/modifier-groups/${g.id}`, { method: 'DELETE' }); flash(`${g.name} removed.`); await load(); }
    catch (e) { flash(extractError(e, 'Could not remove the group.')); }
    finally { setBusyId(null); }
  }

  const rule = (g: Group) => g.isRequired ? 'Required' : g.maxSelect === 1 ? 'Pick one' : g.maxSelect > 0 ? `Up to ${g.maxSelect}` : 'Any';

  return (
    <>
      <Topbar title="Add-ons" subtitle="Offer the extras and add-ons your customers love" />
      <div className="p-6 md:p-8">
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Add-on Groups</h2>
            <HeaderStat><Num>{totalCount}</Num> groups · attach them to products on the Menu screen</HeaderStat>
          </div>
          <button onClick={openNew} className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark">
            <Plus className="size-4" /> New Group
          </button>
        </div>

        <div className="mb-3 flex items-center gap-2">
          <input
            value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder="Search add-on groups…"
            className="w-full max-w-xs rounded-lg border border-border bg-surface px-3 py-2 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20"
          />
        </div>

        <div className="card overflow-hidden">
          {loading ? (
            <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
          ) : error ? (
            <div className="p-6 text-sm text-status-error">{error}</div>
          ) : (
            <table className="w-full text-sm">
              <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th className="px-4 py-3 font-medium">Group</th>
                  <th className="px-4 py-3 font-medium">Rule</th>
                  <th className="px-4 py-3 font-medium">Options</th>
                  <th className="px-4 py-3 text-right font-medium">Actions</th>
                </tr>
              </thead>
              <tbody>
                {groups.map((g, i) => (
                  <tr key={g.id} className={i % 2 ? 'bg-muted/20' : ''}>
                    <td className="px-4 py-3 font-medium">{g.name}</td>
                    <td className="px-4 py-3"><span className="pill pill-idle">{rule(g)}</span></td>
                    <td className="px-4 py-3 text-muted-foreground">
                      {g.items.map(it => it.name + (it.priceDelta ? ` (+${lkr(it.priceDelta)})` : '')).join(', ')}
                    </td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex justify-end gap-2">
                        <button onClick={() => openEdit(g)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted">Edit</button>
                        <button disabled={busyId === g.id} onClick={() => remove(g)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium text-status-error hover:bg-muted disabled:opacity-50">Remove</button>
                      </div>
                    </td>
                  </tr>
                ))}
                {groups.length === 0 && (
                  <tr><td colSpan={4} className="px-4 py-10 text-center text-muted-foreground">No add-on groups yet. Create one, then attach it to products.</td></tr>
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
            noun="groups"
            className="mt-0 flex-1"
          />
        </div>
      </div>

      {open && (
        <Modal
          title={editId ? 'Edit Add-on Group' : 'New Add-on Group'}
          icon={<ListPlus className="size-4" />}
          onClose={() => !submitting && setOpen(false)}
          size="lg"
          footer={
            <div className="flex gap-2">
              <button onClick={() => setOpen(false)} disabled={submitting} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50">Cancel</button>
              <button onClick={submit} disabled={submitting} className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{submitting ? 'Saving…' : editId ? 'Save Changes' : 'Create Group'}</button>
            </div>
          }
        >
            <div className="space-y-4">
              <Field label="Group name" value={name} onChange={setName} placeholder="e.g. Add-ons, Size, Spice level" error={formErrors.name} />
              <div className="grid grid-cols-3 gap-3">
                <label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={required} onChange={e => setRequired(e.target.checked)} className="size-4 rounded border-border text-primary" /> Required</label>
                <Field label="Min select" inputMode="numeric" value={minSelect} onChange={v => setMinSelect(v.replace(/[^0-9]/g, ''))} />
                <Field label="Max (0=∞)" inputMode="numeric" value={maxSelect} onChange={v => setMaxSelect(v.replace(/[^0-9]/g, ''))} error={formErrors.max} />
              </div>
              <div>
                <div className="mb-2 flex items-center justify-between">
                  <label className="text-sm font-semibold text-slate-700">Options</label>
                  <button onClick={addRow} className="inline-flex items-center gap-1 text-xs font-medium text-primary hover:text-primary-dark"><Plus className="size-3.5" /> Add option</button>
                </div>
                <div className="space-y-2">
                  {rows.map((r, idx) => (
                    <div key={idx} className="flex items-center gap-2">
                      <input value={r.name} onChange={e => updateRow(idx, { name: e.target.value })} placeholder="Option name"
                        className="flex-1 rounded-lg border border-border bg-surface px-3 py-2 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20" />
                      <input value={r.price} onChange={e => updateRow(idx, { price: e.target.value.replace(/[^0-9.]/g, '') })} inputMode="decimal" placeholder="+ price"
                        className="w-24 rounded-lg border border-border bg-surface px-3 py-2 text-sm tabular-nums focus:border-primary focus:ring-2 focus:ring-primary/20" />
                      <label className="flex items-center gap-1 text-xs text-muted-foreground" title="Selected by default">
                        <input type="checkbox" checked={r.isDefault} onChange={e => updateRow(idx, { isDefault: e.target.checked })} className="size-4 rounded border-border text-primary" /> def
                      </label>
                      <button onClick={() => removeRow(idx)} disabled={rows.length === 1} className="rounded-lg p-2 text-muted-foreground hover:bg-muted disabled:opacity-30"><X className="size-4" /></button>
                    </div>
                  ))}
                </div>
                {formErrors.items && <p className="mt-1 text-xs text-status-error">{formErrors.items}</p>}
              </div>
            </div>
        </Modal>
      )}

      {toast && <div className="fixed bottom-12 left-1/2 z-[70] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">{toast}</div>}
    </>
  );
}

function extractError(e: unknown, fallback: string): string {
  const msg = (e as Error)?.message ?? '';
  const jsonStart = msg.indexOf('{');
  if (jsonStart !== -1) {
    try { const parsed = JSON.parse(msg.slice(jsonStart)); if (typeof parsed?.error === 'string') return parsed.error; } catch { /* ignore */ }
  }
  if (msg.includes('403')) return 'Only an owner or manager can manage add-ons.';
  return msg || fallback;
}
