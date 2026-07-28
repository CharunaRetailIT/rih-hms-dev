'use client';

import { useEffect, useState } from 'react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient } from '@/lib/api-client';
import { Plus, X } from 'lucide-react';

type Category = {
  id: string;
  parentId: string | null;
  code: string;
  name: string;
  description: string | null;
  colorHex: string | null;
  iconName: string | null;
  sortOrder: number;
  isActive: boolean;
};

type Form = {
  id: string | null;
  code: string;
  name: string;
  parentId: string;
  description: string;
  colorHex: string;
  iconName: string;
  sortOrder: string;
};
const emptyForm: Form = {
  id: null, code: '', name: '', parentId: '', description: '', colorHex: '#0F766E', iconName: '', sortOrder: '0',
};

export default function ProductCategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [toast, setToast] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  // modal
  const [open, setOpen] = useState(false);
  const [form, setForm] = useState<Form>(emptyForm);
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);

  function flash(m: string) { setToast(m); window.setTimeout(() => setToast(null), 3500); }

  async function load() {
    try {
      const c = await apiClient<Category[]>('/api/v1/categories?all=true');
      setCategories(c);
    } catch (e) { setError((e as Error).message); }
    finally { setLoading(false); }
  }
  useEffect(() => { void load(); }, []);

  const parents = categories.filter(c => !c.parentId);
  const childrenOf = (id: string) => categories.filter(c => c.parentId === id);
  const eligibleParents = (selfId: string | null) =>
    parents.filter(c => c.isActive && c.id !== selfId);

  function openNew() {
    setForm(emptyForm);
    setFormErrors({});
    setOpen(true);
  }
  function openEdit(c: Category) {
    setForm({
      id: c.id, code: c.code, name: c.name, parentId: c.parentId ?? '',
      description: c.description ?? '', colorHex: c.colorHex ?? '#0F766E',
      iconName: c.iconName ?? '', sortOrder: String(c.sortOrder ?? 0),
    });
    setFormErrors({});
    setOpen(true);
  }

  function validate(): boolean {
    const e: Record<string, string> = {};
    if (!form.code.trim()) e.code = 'Code is required.';
    if (!form.name.trim()) e.name = 'Name is required.';
    setFormErrors(e);
    return Object.keys(e).length === 0;
  }

  async function submit() {
    if (!validate()) return;
    setSubmitting(true);
    const payload = {
      id: form.id,
      code: form.code.trim(),
      name: form.name.trim(),
      parentId: form.parentId || null,
      description: form.description.trim() || null,
      colorHex: form.colorHex || null,
      iconName: form.iconName.trim() || null,
      sortOrder: Number(form.sortOrder) || 0,
    };
    try {
      await apiClient('/api/v1/categories', { method: 'PUT', body: JSON.stringify(payload) });
      setOpen(false); flash(form.id ? 'Category updated.' : 'Category created.'); await load();
    } catch (e) { flash(extractError(e, 'Could not save the category.')); }
    finally { setSubmitting(false); }
  }

  async function toggleActive(c: Category) {
    setBusyId(c.id);
    try {
      await apiClient(`/api/v1/categories/${c.id}/${c.isActive ? 'deactivate' : 'activate'}`, { method: 'PUT' });
      flash(c.isActive ? `${c.name} deactivated.` : `${c.name} activated.`);
      await load();
    } catch (e) { flash(extractError(e, 'Could not update the category.')); }
    finally { setBusyId(null); }
  }

  return (
    <>
      <Topbar title="Menu" subtitle="Categories" />

      <div className="p-6">
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h2 className="font-heading text-xl font-bold">Categories</h2>
            <p className="text-sm text-muted-foreground">{categories.length} categories · one level of grouping</p>
          </div>
          <button onClick={openNew} className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark">
            <Plus className="size-4" /> New category
          </button>
        </div>

        <div className="card overflow-hidden">
          {loading ? (
            <div className="space-y-2 p-4">{Array.from({ length: 6 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
          ) : error ? (
            <div className="p-6 text-sm text-status-error">{error}</div>
          ) : parents.length === 0 ? (
            <div className="px-4 py-10 text-center text-muted-foreground">No categories yet — add the first one.</div>
          ) : (
            <table className="w-full text-sm">
              <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th className="px-4 py-2.5 font-medium">Category</th>
                  <th className="px-4 py-2.5 font-medium">Code</th>
                  <th className="px-4 py-2.5 font-medium">Description</th>
                  <th className="px-4 py-2.5 font-medium">Status</th>
                  <th className="px-4 py-2.5 text-right font-medium">Actions</th>
                </tr>
              </thead>
              <tbody>
                {parents.map((p, i) => (
                  <CategoryRows
                    key={p.id}
                    category={p}
                    depth={0}
                    striped={i % 2 === 1}
                    busyId={busyId}
                    onEdit={openEdit}
                    onToggle={toggleActive}
                  />
                ))}
                {parents.flatMap(p => childrenOf(p.id)).map((c, i) => (
                  <CategoryRows
                    key={c.id}
                    category={c}
                    depth={1}
                    striped={i % 2 === 1}
                    busyId={busyId}
                    onEdit={openEdit}
                    onToggle={toggleActive}
                  />
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>

      {open && (
        <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 p-4" onClick={() => !submitting && setOpen(false)}>
          <div className="w-full max-w-lg rounded-xl bg-card p-6 shadow-2xl" onClick={e => e.stopPropagation()}>
            <div className="mb-4 flex items-start justify-between">
              <h3 className="font-heading text-lg font-bold">{form.id ? 'Edit category' : 'New category'}</h3>
              <button onClick={() => !submitting && setOpen(false)} className="rounded-lg p-1.5 text-muted-foreground hover:bg-muted"><X className="size-5" /></button>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="mb-1 block text-sm font-semibold text-slate-700">Code</label>
                <input value={form.code} onChange={e => setForm(f => ({ ...f, code: e.target.value.toUpperCase() }))}
                  placeholder="MAINS"
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 font-mono text-sm uppercase focus:border-primary focus:ring-2 focus:ring-primary/20" />
                {formErrors.code && <p className="mt-1 text-xs text-status-error">{formErrors.code}</p>}
              </div>
              <div>
                <label className="mb-1 block text-sm font-semibold text-slate-700">Name</label>
                <input value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
                  placeholder="Mains"
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20" />
                {formErrors.name && <p className="mt-1 text-xs text-status-error">{formErrors.name}</p>}
              </div>
              <div className="col-span-2">
                <label className="mb-1 block text-sm font-semibold text-slate-700">Parent category</label>
                <select value={form.parentId} onChange={e => setForm(f => ({ ...f, parentId: e.target.value }))}
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20">
                  <option value="">— top level —</option>
                  {eligibleParents(form.id).map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
                </select>
                <p className="mt-1 text-xs text-muted-foreground">Sub-categories sit one level deep, under a top-level one.</p>
              </div>
              <div className="col-span-2">
                <label className="mb-1 block text-sm font-semibold text-slate-700">Description</label>
                <textarea value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))}
                  rows={2}
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 focus:border-primary focus:ring-2 focus:ring-primary/20" />
              </div>
              <div>
                <label className="mb-1 block text-sm font-semibold text-slate-700">POS button colour</label>
                <div className="flex items-center gap-2">
                  <input type="color" value={form.colorHex} onChange={e => setForm(f => ({ ...f, colorHex: e.target.value }))}
                    className="h-10 w-10 shrink-0 cursor-pointer rounded-lg border border-border p-0.5" />
                  <input value={form.colorHex} onChange={e => setForm(f => ({ ...f, colorHex: e.target.value }))}
                    className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 font-mono text-sm focus:border-primary focus:ring-2 focus:ring-primary/20" />
                </div>
              </div>
              <div>
                <label className="mb-1 block text-sm font-semibold text-slate-700">Sort order</label>
                <input value={form.sortOrder} onChange={e => setForm(f => ({ ...f, sortOrder: e.target.value.replace(/[^0-9]/g, '') }))}
                  inputMode="numeric"
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 tabular-nums focus:border-primary focus:ring-2 focus:ring-primary/20" />
              </div>
              <div className="col-span-2">
                <label className="mb-1 block text-sm font-semibold text-slate-700">Icon name</label>
                <input value={form.iconName} onChange={e => setForm(f => ({ ...f, iconName: e.target.value }))}
                  placeholder="lunch_dining"
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2.5 font-mono text-sm focus:border-primary focus:ring-2 focus:ring-primary/20" />
                <p className="mt-1 text-xs text-muted-foreground">Material Symbols name, e.g. lunch_dining.</p>
              </div>
            </div>
            <div className="mt-6 flex gap-2">
              <button onClick={() => setOpen(false)} disabled={submitting} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50">Cancel</button>
              <button onClick={submit} disabled={submitting} className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{submitting ? 'Saving…' : form.id ? 'Save changes' : 'Create category'}</button>
            </div>
          </div>
        </div>
      )}

      {toast && <div className="fixed bottom-12 left-1/2 z-[70] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">{toast}</div>}
    </>
  );
}

function CategoryRows({ category, depth, striped, busyId, onEdit, onToggle }: {
  category: Category; depth: number; striped: boolean; busyId: string | null;
  onEdit: (c: Category) => void; onToggle: (c: Category) => void;
}) {
  return (
    <tr className={striped ? 'bg-muted/20' : ''}>
      <td className="px-4 py-2.5">
        <div className="flex items-center gap-2.5" style={{ paddingLeft: depth * 20 }}>
          <span className="size-3.5 shrink-0 rounded" style={{ background: category.colorHex ?? '#94A3B8' }} />
          <span className="font-medium">{category.name}</span>
        </div>
      </td>
      <td className="px-4 py-2.5 font-mono text-xs text-muted-foreground">{category.code}</td>
      <td className="px-4 py-2.5 text-xs text-muted-foreground">{category.description ?? '—'}</td>
      <td className="px-4 py-2.5"><span className={`pill ${category.isActive ? 'pill-paid' : 'pill-void'}`}>{category.isActive ? 'Active' : 'Inactive'}</span></td>
      <td className="px-4 py-2.5 text-right">
        <div className="flex justify-end gap-2">
          <button onClick={() => onEdit(category)} className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted">Edit</button>
          <button
            disabled={busyId === category.id}
            onClick={() => onToggle(category)}
            className={`rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted disabled:opacity-50 ${category.isActive ? 'text-status-error' : 'text-primary'}`}
          >
            {category.isActive ? 'Deactivate' : 'Activate'}
          </button>
        </div>
      </td>
    </tr>
  );
}

function extractError(e: unknown, fallback: string): string {
  const msg = (e as Error)?.message ?? '';
  const jsonStart = msg.indexOf('{');
  if (jsonStart !== -1) {
    try { const parsed = JSON.parse(msg.slice(jsonStart)); if (typeof parsed?.error === 'string') return parsed.error; } catch { /* ignore */ }
  }
  if (msg.includes('403')) return 'Only an owner or manager can manage categories.';
  return msg || fallback;
}
