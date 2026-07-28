'use client';

import { useEffect, useMemo, useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { Icon } from '@/components/ui/Icon';
import { lkr } from '@/lib/api-client';

const STEPS = [
  { label: 'Workspace', state: 'done' as const },
  { label: 'Choose plan', state: 'active' as const },
  { label: 'Configure Network', state: 'todo' as const },
  { label: 'Payment', state: 'todo' as const },
];

// Catalog shapes (from /api/v1/billing/catalog — RIT-configurable, nothing hardcoded here).
type CatPlan = { code: string; name: string; monthlyPrice: number; currency: string; includedLocations: number; includedUsers: number; features: string[] };
type CatAddon = { code: string; name: string; unit: string; unitPrice: number; currency: string };

const isFlat = (unit: string) => unit === 'flat_month';

export default function ChoosePlanPage() {
  const router = useRouter();
  const [plans, setPlans] = useState<CatPlan[]>([]);
  const [addons, setAddons] = useState<CatAddon[]>([]);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<string>('');
  const [qty, setQty] = useState<Record<string, number>>({});   // addon code → quantity (flat = 0/1)

  useEffect(() => {
    fetch('/api/v1/billing/catalog')
      .then(r => r.json())
      .then((d: { plans: CatPlan[]; addons: CatAddon[] }) => {
        setPlans(d.plans || []);
        setAddons(d.addons || []);
        // restore prior choice from the signup draft, else default to the middle/most-common plan
        try {
          const draft = JSON.parse(localStorage.getItem('hms.signup') || '{}');
          setSelected(draft.plan || (d.plans?.[Math.min(1, (d.plans?.length || 1) - 1)]?.code ?? d.plans?.[0]?.code ?? ''));
          if (draft.addons) setQty(draft.addons);
        } catch { setSelected(d.plans?.[0]?.code ?? ''); }
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  const setQ = (code: string, n: number) => setQty(s => ({ ...s, [code]: Math.max(0, n) }));
  const toggle = (code: string) => setQty(s => ({ ...s, [code]: s[code] ? 0 : 1 }));

  const total = useMemo(() => {
    const plan = plans.find(p => p.code === selected);
    let t = plan?.monthlyPrice ?? 0;
    for (const a of addons) {
      const q = qty[a.code] ?? 0;
      if (q <= 0) continue;
      t += isFlat(a.unit) ? a.unitPrice : a.unitPrice * q;
    }
    return t;
  }, [plans, addons, selected, qty]);

  function next() {
    try {
      const draft = JSON.parse(localStorage.getItem('hms.signup') || '{}');
      const cleanAddons: Record<string, number> = {};
      for (const a of addons) { const q = qty[a.code] ?? 0; if (q > 0) cleanAddons[a.code] = isFlat(a.unit) ? 1 : q; }
      localStorage.setItem('hms.signup', JSON.stringify({ ...draft, plan: selected, addons: cleanAddons }));
    } catch { /* ignore */ }
    router.push('/signup/network');
  }

  return (
    <div className="flex min-h-screen flex-col bg-surface text-on-surface">
      <header className="fixed left-0 top-0 z-50 flex w-full items-center justify-between border-b border-border bg-card px-6 py-3">
        <div className="font-heading text-xl font-black tracking-tight text-primary">RIT HMS</div>
        <Link href="/login" className="text-sm font-medium text-muted-foreground hover:text-primary">Sign In</Link>
      </header>

      <main className="mx-auto w-full max-w-6xl flex-grow px-4 pb-32 pt-24 md:px-8">
        <div className="mb-12">
          <div className="mb-4 flex items-center justify-center gap-4">
            {STEPS.map((step, i) => (
              <div key={step.label} className="flex items-center gap-2">
                <span className={['flex h-8 w-8 items-center justify-center rounded-full text-xs font-bold',
                  step.state === 'todo' ? 'bg-slate-200 text-muted-foreground' : 'bg-primary text-white',
                  step.state === 'active' ? 'ring-4 ring-primary-tint' : ''].join(' ')}>
                  {step.state === 'done' ? <Icon name="check" className="text-sm" /> : i + 1}
                </span>
                <span className={['hidden text-xs font-bold sm:block', step.state === 'todo' ? 'text-muted-foreground' : 'text-primary'].join(' ')}>{step.label}</span>
                {i < STEPS.length - 1 && <div className={`h-px w-8 ${step.state === 'done' ? 'bg-primary' : 'bg-border'}`} />}
              </div>
            ))}
          </div>
          <h1 className="text-center font-heading text-3xl font-extrabold tracking-tight text-on-surface md:text-4xl">Select your operating model</h1>
          <p className="mx-auto mt-2 max-w-lg text-center text-muted-foreground">Scalable solutions tailored for Sri Lankan hospitality businesses of all sizes.</p>
        </div>

        {loading ? (
          <div className="py-20 text-center text-muted-foreground"><Icon name="progress_activity" className="animate-spin text-3xl" /></div>
        ) : (
          <>
            <div className="mb-16 grid grid-cols-1 gap-6 md:grid-cols-3">
              {plans.map(plan => {
                const isSel = selected === plan.code;
                return (
                  <div key={plan.code} role="button" tabIndex={0}
                    onClick={() => setSelected(plan.code)}
                    onKeyDown={e => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); setSelected(plan.code); } }}
                    className={['relative flex cursor-pointer flex-col rounded-xl border bg-card p-6 transition-all duration-300 hover:-translate-y-1',
                      isSel ? 'border-2 border-primary bg-primary-tint shadow-lg' : 'border-border hover:border-primary'].join(' ')}>
                    <div className="mb-6">
                      <span className="rounded bg-primary-tint px-2 py-1 text-xs font-bold uppercase tracking-widest text-primary">{plan.name}</span>
                      <div className="mt-4 flex items-baseline gap-1">
                        <span className="font-heading text-4xl font-black text-on-surface">LKR {plan.monthlyPrice.toLocaleString()}</span>
                        <span className="text-sm text-muted-foreground">/month</span>
                      </div>
                      <p className="mt-2 text-xs text-muted-foreground">Includes {plan.includedLocations} outlet{plan.includedLocations === 1 ? '' : 's'} · {plan.includedUsers} users</p>
                    </div>
                    <ul className="mb-8 flex-grow space-y-3">
                      {plan.features.map(f => (
                        <li key={f} className="flex items-center gap-3 text-sm text-on-surface">
                          <Icon name="check_circle" className="text-lg text-primary" /> {f}
                        </li>
                      ))}
                    </ul>
                    <button type="button" onClick={e => { e.stopPropagation(); setSelected(plan.code); }}
                      className={['w-full rounded-lg py-3 font-bold transition-colors',
                        isSel ? 'bg-primary text-white hover:bg-primary-dark' : 'border-2 border-primary text-primary hover:bg-primary hover:text-white'].join(' ')}>
                      {isSel ? 'Current Selection' : `Select ${plan.name}`}
                    </button>
                  </div>
                );
              })}
            </div>

            {addons.length > 0 && (
              <div className="rounded-2xl border border-border bg-card p-8">
                <div className="mb-8 flex items-center gap-3">
                  <Icon name="add_box" className="text-3xl text-primary" />
                  <h2 className="font-heading text-2xl font-bold text-on-surface">Operational Add-ons</h2>
                </div>
                <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
                  {addons.map(a => {
                    const q = qty[a.code] ?? 0;
                    const flat = isFlat(a.unit);
                    const on = q > 0;
                    return (
                      <div key={a.code} className={['flex flex-col justify-between rounded-xl border p-5 transition-all',
                        on ? 'border-primary bg-primary-tint' : 'border-border bg-card'].join(' ')}>
                        <div>
                          <div className="mb-2 flex items-start justify-between gap-2">
                            <h3 className="font-bold text-on-surface">{a.name}</h3>
                            {flat
                              ? <input type="checkbox" checked={on} onChange={() => toggle(a.code)} className="h-5 w-5 rounded border-border text-primary focus:ring-primary" />
                              : null}
                          </div>
                          <p className="text-sm text-muted-foreground">
                            LKR {a.unitPrice.toLocaleString()}{flat ? ' /month' : a.unit === 'per_device_month' ? ' /device/mo' : ' /outlet/mo'}
                          </p>
                        </div>
                        {!flat && (
                          <div className="mt-4 flex items-center gap-2">
                            <button type="button" onClick={() => setQ(a.code, q - 1)} className="size-8 rounded-lg border border-border font-bold hover:bg-muted">−</button>
                            <span className="w-8 text-center font-heading font-bold">{q}</span>
                            <button type="button" onClick={() => setQ(a.code, q + 1)} className="size-8 rounded-lg border border-border font-bold hover:bg-muted">+</button>
                            <span className="ml-2 text-xs text-muted-foreground">{a.unit === 'per_device_month' ? 'devices' : 'outlets'}</span>
                          </div>
                        )}
                      </div>
                    );
                  })}
                </div>
              </div>
            )}
          </>
        )}
      </main>

      <footer className="fixed bottom-0 left-0 z-50 flex w-full items-center justify-between border-t border-border bg-card px-8 py-4">
        <Link href="/signup" className="flex items-center rounded-lg px-6 py-2 font-medium text-secondary hover:bg-slate-100"><Icon name="arrow_back" className="mr-1" /> Back</Link>
        <div className="flex items-center gap-6">
          <div className="hidden flex-col items-end lg:flex">
            <span className="text-[10px] font-bold uppercase tracking-wider text-muted-foreground">Estimated Total</span>
            <span className="font-heading text-lg font-black text-primary">LKR {lkr(total)}<span className="ml-1 text-xs font-normal text-muted-foreground">/mo</span></span>
          </div>
          <button type="button" disabled={!selected} onClick={next}
            className="flex items-center gap-2 rounded-lg bg-primary px-8 py-3 font-bold text-white shadow-md hover:bg-primary-dark active:scale-95 disabled:opacity-50">
            Continue to Network <Icon name="arrow_forward" />
          </button>
        </div>
      </footer>
    </div>
  );
}
