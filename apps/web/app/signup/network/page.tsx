'use client';

import { useEffect, useMemo, useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { Icon } from '@/components/ui/Icon';
import { lkr } from '@/lib/api-client';
import { validate, required } from '@/lib/validation';

const STEPS = [
  { label: 'Workspace details', state: 'done' as const },
  { label: 'Choose plan', state: 'done' as const },
  { label: 'Configure Network', state: 'active' as const },
  { label: 'Payment', state: 'todo' as const },
];

const OUTLET_TYPES = ['Restaurant', 'Cafe', 'Bar', 'Kiosk', 'Cloud Kitchen', 'Warehouse'];
const EXTRA_LOCATION = 'extra_location';

// Catalog shapes (from /api/v1/billing/catalog — RIT-configurable, nothing hardcoded here).
type CatPlan = { code: string; name: string; monthlyPrice: number; currency: string; includedLocations: number; includedUsers: number; features: string[] };
type CatAddon = { code: string; name: string; unit: string; unitPrice: number; currency: string };
type CatTax = { code: string; name: string; ratePercent: number };
const isFlat = (u: string) => u === 'flat_month';

type Outlet = { id: number; name: string; city: string; type: string };
let nextId = 100;

export default function ConfigureNetworkPage() {
  const router = useRouter();

  const [plans, setPlans] = useState<CatPlan[]>([]);
  const [addons, setAddons] = useState<CatAddon[]>([]);
  const [taxes, setTaxes] = useState<CatTax[]>([]);
  const [planCode, setPlanCode] = useState('');
  const [draftAddons, setDraftAddons] = useState<Record<string, number>>({});
  const [country, setCountry] = useState('LK');
  const [loading, setLoading] = useState(true);

  const [hq, setHq] = useState({ name: '', contact: '', address: '', taxNo: '' });
  const [outlets, setOutlets] = useState<Outlet[]>([{ id: 1, name: '', city: '', type: 'Restaurant' }]);
  const [activeId, setActiveId] = useState<number>(1);
  const [errors, setErrors] = useState<Record<string, string>>({});

  // Load the signup draft + the catalog priced for the chosen country (so we can show VAT).
  useEffect(() => {
    let draft: Record<string, unknown> = {};
    try { draft = JSON.parse(localStorage.getItem('hms.signup') || '{}'); } catch { /* */ }
    const c = (draft.country as string) || 'LK';
    const draftOutlets = Array.isArray(draft.outlets) ? (draft.outlets as string[]) : null;
    const draftExtra = Number((draft.addons as Record<string, number> | undefined)?.extra_location) || 0;
    const planFromDraft = (draft.plan as string) || '';
    setCountry(c);
    setPlanCode(planFromDraft);
    setDraftAddons((draft.addons as Record<string, number>) || {});
    setHq(h => ({ ...h, name: (draft.businessName as string) || (draft.workspace as string) || '' }));
    fetch(`/api/v1/billing/catalog?country=${encodeURIComponent(c)}`)
      .then(r => r.json())
      .then((d: { plans: CatPlan[]; addons: CatAddon[]; taxes: CatTax[] }) => {
        setPlans(d.plans || []); setAddons(d.addons || []); setTaxes(d.taxes || []);
        const code = planFromDraft || d.plans?.[Math.min(1, (d.plans?.length || 1) - 1)]?.code || '';
        if (!planFromDraft) setPlanCode(code);
        const included = d.plans?.find(p => p.code === code)?.includedLocations ?? 1;
        if (draftOutlets && draftOutlets.length) {
          // Returning to this step — restore what was entered.
          setOutlets(draftOutlets.map((name, i) => ({ id: i + 1, name, city: '', type: 'Restaurant' })));
          nextId = draftOutlets.length + 1;
        } else {
          // First visit — pre-fill one row per outlet the buyer is paying for (included + additional bought
          // on the plan step). The owner can remove any they don't need; the add-on count follows.
          const seed = Math.max(1, included + draftExtra);
          setOutlets(Array.from({ length: seed }, (_, i) => ({ id: i + 1, name: '', city: '', type: 'Restaurant' })));
          nextId = seed + 1;
        }
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  const plan = useMemo(() => plans.find(p => p.code === planCode) ?? plans[0], [plans, planCode]);
  const includedLocations = plan?.includedLocations ?? 1;
  const extraLocations = Math.max(0, outlets.length - includedLocations);
  const hasExtraLocationAddon = addons.some(a => a.code === EXTRA_LOCATION);

  // Effective add-ons = the basket chosen at the plan step, but the outlet count here drives extra_location.
  const effectiveAddons = useMemo(() => {
    const eff: Record<string, number> = { ...draftAddons };
    if (hasExtraLocationAddon) { if (extraLocations > 0) eff[EXTRA_LOCATION] = extraLocations; else delete eff[EXTRA_LOCATION]; }
    return eff;
  }, [draftAddons, extraLocations, hasExtraLocationAddon]);

  const subtotal = useMemo(() => {
    let t = plan?.monthlyPrice ?? 0;
    for (const a of addons) { const q = effectiveAddons[a.code] ?? 0; if (q <= 0) continue; t += isFlat(a.unit) ? a.unitPrice : a.unitPrice * q; }
    return t;
  }, [plan, addons, effectiveAddons]);

  const taxLines = useMemo(() => taxes.map(t => ({ ...t, amount: Math.round(subtotal * t.ratePercent) / 100 })), [taxes, subtotal]);
  const taxTotal = taxLines.reduce((s, t) => s + t.amount, 0);
  const grandTotal = subtotal + taxTotal;
  const cur = plan?.currency ?? 'LKR';

  const setHqField = (k: keyof typeof hq) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setHq((s) => ({ ...s, [k]: e.target.value }));
    if (errors[k]) setErrors((er) => { const n = { ...er }; delete n[k]; return n; });
  };
  function setOutletField(id: number, k: keyof Outlet, value: string) {
    setOutlets((list) => list.map((o) => (o.id === id ? { ...o, [k]: value } : o)));
    const key = `outlet_${id}_${k}`;
    if (errors[key]) setErrors((er) => { const n = { ...er }; delete n[key]; return n; });
  }
  function addOutlet() { const id = nextId++; setOutlets((l) => [...l, { id, name: '', city: '', type: OUTLET_TYPES[0] }]); setActiveId(id); }
  function removeOutlet(id: number) {
    setOutlets((list) => { const next = list.filter((o) => o.id !== id); if (id === activeId && next.length) setActiveId(next[0].id); return next; });
    setErrors((er) => { const n = { ...er }; delete n[`outlet_${id}_name`]; return n; });
  }

  function onContinue() {
    const values: Record<string, string> = { hqName: hq.name };
    const rules: Record<string, ReturnType<typeof required>[]> = { hqName: [required('HQ name')] };
    for (const o of outlets) { values[`outlet_${o.id}_name`] = o.name; rules[`outlet_${o.id}_name`] = [required('Outlet name')]; }
    const errs = validate(values, rules);
    if (errs.hqName) { errs.name = errs.hqName; delete errs.hqName; }
    setErrors(errs);
    if (Object.keys(errs).length) { const f = outlets.find((o) => errs[`outlet_${o.id}_name`]); if (f) setActiveId(f.id); return; }
    try {
      const draft = JSON.parse(localStorage.getItem('hms.signup') || '{}');
      // Persist the outlet count → extra_location reconciliation so payment + provisioning bill correctly.
      localStorage.setItem('hms.signup', JSON.stringify({ ...draft, plan: planCode, addons: effectiveAddons, hqName: hq.name, outlets: outlets.map(o => o.name).filter(Boolean) }));
    } catch { /* ignore */ }
    router.push('/signup/payment');
  }

  const active = outlets.find((o) => o.id === activeId) ?? null;

  return (
    <main className="flex min-h-screen flex-col bg-surface text-on-surface">
      <header className="sticky top-0 z-50 flex w-full items-center justify-between border-b border-border bg-surface px-6 py-4">
        <div className="flex items-center gap-8">
          <span className="font-heading text-lg font-black text-primary">RIT HMS</span>
          <nav className="hidden items-center gap-6 md:flex">
            <span className="cursor-pointer text-sm font-medium tracking-wide text-muted-foreground opacity-70 transition-colors hover:text-primary">Workspace</span>
            <span className="cursor-pointer text-sm font-medium tracking-wide text-muted-foreground opacity-70 transition-colors hover:text-primary">Choose Plan</span>
            <span className="cursor-pointer border-b-2 border-primary pb-2 text-sm font-medium tracking-wide text-primary">Configure Outlets</span>
            <span className="cursor-pointer text-sm font-medium tracking-wide text-muted-foreground opacity-70 transition-colors hover:text-primary">Payment</span>
          </nav>
        </div>
        <div className="flex items-center gap-4">
          <button type="button" className="text-muted-foreground transition-colors hover:text-primary"><Icon name="help" /></button>
          <button type="button" className="text-muted-foreground transition-colors hover:text-primary"><Icon name="settings" /></button>
          <div className="flex size-8 items-center justify-center rounded-full bg-primary-tint text-xs font-bold text-primary-dark">JD</div>
        </div>
      </header>

      <div className="mx-auto flex w-full max-w-7xl flex-grow flex-col gap-8 px-6 py-8 pb-28">
        {/* Progress Stepper */}
        <section className="flex flex-col gap-4 rounded-xl border border-border bg-card p-4 sm:flex-row sm:items-center sm:justify-between">
          <div className="flex flex-wrap items-center gap-3">
            {STEPS.map((s, i) => (
              <div key={s.label} className="flex items-center gap-3">
                <div className={`flex items-center gap-2 ${s.state === 'todo' ? 'text-muted-foreground opacity-50' : 'text-primary'} ${s.state === 'done' ? 'opacity-60' : ''}`}>
                  <span className={`flex size-8 items-center justify-center rounded-full text-xs font-bold ${s.state === 'todo' ? 'border border-border' : 'bg-primary text-white'}`}>
                    {s.state === 'done' ? <Icon name="check" className="text-sm" /> : `0${i + 1}`}
                  </span>
                  <span className="text-sm font-semibold">{s.label}</span>
                </div>
                {i < STEPS.length - 1 && <div className="h-px w-6 bg-border opacity-60" />}
              </div>
            ))}
          </div>
          <div className="text-right">
            <p className="text-xs text-muted-foreground">Selected Plan</p>
            <p className="font-bold text-primary">{plan?.name ?? '—'}</p>
          </div>
        </section>

        {/* Head Office Details */}
        <section className="w-full rounded-xl border border-border bg-card p-6">
          <h2 className="mb-6 flex items-center gap-2 font-heading text-lg font-bold text-on-surface">
            <Icon name="corporate_fare" className="text-primary" /> Head Office Details
          </h2>
          <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-4">
            <Field label="HQ Name" value={hq.name} onChange={setHqField('name')} placeholder="Business Name" error={errors.name} />
            <Field label="HQ Contact Person" value={hq.contact} onChange={setHqField('contact')} placeholder="Full Name" />
            <Field label="HQ Address" value={hq.address} onChange={setHqField('address')} placeholder="Full Address" />
            <Field label="Tax Registration Number (BRN/VAT)" value={hq.taxNo} onChange={setHqField('taxNo')} placeholder="Tax ID" />
          </div>
        </section>

        <div className="grid grid-cols-1 items-start gap-8 md:grid-cols-12">
          {/* Left: Outlet List */}
          <aside className="flex flex-col gap-4 md:col-span-4">
            <div className="mb-2 flex items-end justify-between">
              <div>
                <h2 className="font-heading text-lg font-bold text-on-surface">Registered Outlets</h2>
                <p className="text-sm text-muted-foreground">{outlets.length} {outlets.length === 1 ? 'Outlet' : 'Outlets'} · {includedLocations} included{extraLocations > 0 ? `, ${extraLocations} extra` : ''}</p>
              </div>
              <button type="button" onClick={addOutlet} className="flex items-center gap-2 rounded-lg bg-primary px-3 py-2 text-sm font-bold text-white transition-all hover:opacity-90 active:scale-95">
                <Icon name="add" className="text-sm" /> Add New
              </button>
            </div>
            <div className="custom-scrollbar flex max-h-[500px] flex-col gap-3 overflow-y-auto pr-2">
              {outlets.map((o) => {
                const isActive = o.id === activeId; const rowErr = errors[`outlet_${o.id}_name`];
                return (
                  <div key={o.id} onClick={() => setActiveId(o.id)}
                    className={`group flex cursor-pointer items-center justify-between rounded-xl p-4 transition-colors ${isActive ? 'border-2 border-primary bg-primary-tint shadow-sm' : rowErr ? 'border border-error bg-card hover:bg-surface' : 'border border-border bg-card hover:bg-surface'}`}>
                    <div className="flex items-center gap-3">
                      <div className={`flex size-10 items-center justify-center rounded-lg transition-colors ${isActive ? 'bg-primary text-white' : 'bg-slate-100 text-muted-foreground group-hover:bg-primary-tint group-hover:text-primary'}`}>
                        <Icon name="storefront" />
                      </div>
                      <div>
                        <h3 className={`text-sm font-bold ${isActive ? 'text-primary-dark' : 'text-on-surface'}`}>{o.name || 'Untitled outlet'}</h3>
                        <p className="text-xs text-muted-foreground">{o.city || o.type}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-1">
                      {outlets.length > 1 && (
                        <button type="button" aria-label="Remove outlet" onClick={(e) => { e.stopPropagation(); removeOutlet(o.id); }}
                          className="rounded-md p-1 text-muted-foreground opacity-0 transition-all hover:bg-error/10 hover:text-error group-hover:opacity-100">
                          <Icon name="delete" className="text-lg" />
                        </button>
                      )}
                      <Icon name="chevron_right" className={isActive ? 'text-primary' : 'text-border group-hover:text-primary'} />
                    </div>
                  </div>
                );
              })}
              <button type="button" onClick={addOutlet} className="flex flex-col items-center justify-center rounded-xl border-2 border-dashed border-border p-8 text-center opacity-70 transition-opacity hover:opacity-100">
                <Icon name="add_business" className="mb-2 text-4xl text-muted-foreground" />
                <p className="text-sm font-medium text-muted-foreground">Add another location to your network.</p>
              </button>
            </div>
          </aside>

          {/* Right: Outlet Details Form + Summary */}
          <section className="flex flex-col gap-6 md:col-span-8">
            {active ? (
              <div className="rounded-xl border border-border bg-card p-6">
                <h2 className="mb-6 flex items-center gap-2 font-heading text-lg font-bold text-on-surface">
                  <Icon name="edit_square" className="text-primary" /> Outlet Details: {active.name || 'New Outlet'}
                </h2>
                <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                  <Field label="Outlet Name" value={active.name} onChange={(e) => setOutletField(active.id, 'name', e.target.value)} placeholder="e.g. City Mall Branch" error={errors[`outlet_${active.id}_name`]} />
                  <Field label="City" value={active.city} onChange={(e) => setOutletField(active.id, 'city', e.target.value)} placeholder="e.g. Fort, Colombo" />
                  <div className="flex flex-col gap-2 md:col-span-2">
                    <label className="text-sm font-bold text-muted-foreground">Outlet Type</label>
                    <select value={active.type} onChange={(e) => setOutletField(active.id, 'type', e.target.value)}
                      className="rounded-lg border border-border bg-surface px-4 py-3 outline-none transition-all focus:border-primary focus:ring-2 focus:ring-primary/20">
                      {OUTLET_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
                    </select>
                  </div>
                </div>
              </div>
            ) : (
              <div className="flex flex-col items-center justify-center rounded-xl border-2 border-dashed border-border p-12 text-center text-muted-foreground">
                <Icon name="store" className="mb-3 text-5xl" />
                <p className="font-medium">No outlets yet. Add your first location to continue.</p>
              </div>
            )}

            {/* Price breakdown — catalog-driven, country-aware tax */}
            <div className="rounded-xl border border-border bg-card p-6">
              <h3 className="mb-4 flex items-center gap-2 font-heading text-base font-bold text-on-surface">
                <Icon name="receipt_long" className="text-primary" /> Monthly subscription
              </h3>
              {loading ? (
                <p className="text-sm text-muted-foreground">Loading pricing…</p>
              ) : (
                <div className="space-y-2 text-sm">
                  <Row label={`${plan?.name ?? 'Plan'} (incl. ${includedLocations} outlet${includedLocations === 1 ? '' : 's'}, ${plan?.includedUsers ?? 0} users)`} value={`${cur} ${lkr(plan?.monthlyPrice ?? 0)}`} />
                  {addons.map(a => { const q = effectiveAddons[a.code] ?? 0; if (q <= 0) return null;
                    const amt = isFlat(a.unit) ? a.unitPrice : a.unitPrice * q;
                    return <Row key={a.code} label={isFlat(a.unit) ? a.name : `${a.name} × ${q}`} value={`${cur} ${lkr(amt)}`} />; })}
                  <div className="flex justify-between border-t border-border pt-2 font-semibold"><span>Subtotal</span><span>{cur} {lkr(subtotal)}</span></div>
                  {taxLines.map(t => <Row key={t.code} muted label={`${t.name} (${t.ratePercent}%)`} value={`${cur} ${lkr(t.amount)}`} />)}
                  {taxLines.length === 0 && <p className="text-xs text-muted-foreground">No tax applies to {country === 'LK' ? 'this plan' : `${country} (export of services — zero-rated)`}.</p>}
                  <div className="flex justify-between border-t border-border pt-2 font-heading text-lg font-black text-primary"><span>Total / month</span><span>{cur} {lkr(grandTotal)}</span></div>
                </div>
              )}
            </div>
          </section>
        </div>
      </div>

      {/* Footer Navigation */}
      <footer className="fixed bottom-0 left-0 z-50 flex w-full items-center justify-between border-t border-border bg-surface px-8 py-4">
        <div className="flex items-center gap-4">
          <Link href="/signup/plan" className="flex items-center gap-2 rounded-lg border border-border px-6 py-2 font-bold text-muted-foreground transition-colors hover:bg-card">
            <Icon name="arrow_back" className="text-sm" /> Back
          </Link>
        </div>
        <div className="flex items-center gap-6">
          <div className="hidden text-right sm:block">
            <span className="text-sm text-muted-foreground">Final Monthly Estimated</span>
            <p className="text-base font-black text-on-surface">{cur} {lkr(grandTotal)}{taxTotal > 0 ? ' (incl. tax)' : ''}</p>
          </div>
          <button type="button" onClick={onContinue} className="flex items-center gap-3 rounded-lg bg-primary px-8 py-3 text-base font-bold text-white transition-all hover:shadow-lg active:scale-95">
            Continue to Payment <Icon name="payments" />
          </button>
        </div>
      </footer>
    </main>
  );
}

function Row({ label, value, muted }: { label: string; value: string; muted?: boolean }) {
  return <div className={`flex justify-between ${muted ? 'text-muted-foreground' : ''}`}><span>{label}</span><span>{value}</span></div>;
}

function Field({ label, value, onChange, placeholder, error, type = 'text' }: {
  label: string; value: string; placeholder?: string; error?: string; type?: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}) {
  return (
    <div className="flex flex-col gap-2">
      <label className="text-sm font-bold text-muted-foreground">{label}</label>
      <input type={type} value={value} onChange={onChange} placeholder={placeholder} aria-invalid={!!error}
        className={`rounded-lg border bg-surface px-4 py-3 outline-none transition-all focus:ring-2 ${error ? 'border-error focus:ring-error/20' : 'border-border focus:border-primary focus:ring-primary/20'}`} />
      {error && <p className="flex items-center gap-1 text-xs text-error"><Icon name="error" className="text-[14px]" /> {error}</p>}
    </div>
  );
}
