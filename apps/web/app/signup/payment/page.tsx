'use client';

// Onboarding — Step 4 of 4. Signup is a 14-day FREE TRIAL: no card is required or charged here.
// The owner adds a real payment method later in Settings → Subscription & billing (PayHere
// preapproval) before the trial ends — so this step just confirms the plan and creates the
// trialing workspace. (No fake card form: it was unvalidated and misleading.)

import { useEffect, useMemo, useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { Icon } from '@/components/ui/Icon';
import { lkr } from '@/lib/api-client';
import { loadPayHere } from '@/lib/payhere';

const STEPS = ['Workspace details', 'Choose plan', 'Configure Network', 'Payment'] as const;
const ACTIVE_STEP = 3;

// Catalog shapes (from /api/v1/billing/catalog — nothing hardcoded; tax is country-aware).
type CatPlan = { code: string; name: string; monthlyPrice: number; currency: string; includedLocations: number; includedUsers: number; features: string[] };
type CatAddon = { code: string; name: string; unit: string; unitPrice: number; currency: string };
type CatTax = { code: string; name: string; ratePercent: number };

export default function PaymentPage() {
  const router = useRouter();
  const [status, setStatus] = useState<'idle' | 'processing' | 'success'>('idle');
  const [apiError, setApiError] = useState<string | null>(null);

  // Real order summary from the signup draft + catalog (country-aware tax) — no hardcoded prices.
  const [plans, setPlans] = useState<CatPlan[]>([]);
  const [addons, setAddons] = useState<CatAddon[]>([]);
  const [taxes, setTaxes] = useState<CatTax[]>([]);
  const [planCode, setPlanCode] = useState('');
  const [addonQty, setAddonQty] = useState<Record<string, number>>({});
  const [country, setCountry] = useState('LK');
  const [requireCard, setRequireCard] = useState(false);

  useEffect(() => {
    let draft: Record<string, unknown> = {};
    try { draft = JSON.parse(localStorage.getItem('hms.signup') || '{}'); } catch { /* */ }
    const c = (draft.country as string) || 'LK';
    setCountry(c); setPlanCode((draft.plan as string) || ''); setAddonQty((draft.addons as Record<string, number>) || {});
    fetch(`/api/v1/billing/catalog?country=${encodeURIComponent(c)}`).then(r => r.json())
      .then((d: { plans: CatPlan[]; addons: CatAddon[]; taxes: CatTax[]; requireCardAtSignup?: boolean }) => {
        setPlans(d.plans || []); setAddons(d.addons || []); setTaxes(d.taxes || []); setRequireCard(!!d.requireCardAtSignup);
      })
      .catch(() => {});
  }, []);

  const plan = useMemo(() => plans.find(p => p.code === planCode) ?? plans[0], [plans, planCode]);
  const cur = plan?.currency ?? 'LKR';
  const subtotal = useMemo(() => { let t = plan?.monthlyPrice ?? 0; for (const a of addons) { const q = addonQty[a.code] ?? 0; if (q <= 0) continue; t += a.unit === 'flat_month' ? a.unitPrice : a.unitPrice * q; } return t; }, [plan, addons, addonQty]);
  const taxLines = useMemo(() => taxes.map(t => ({ ...t, amount: Math.round(subtotal * t.ratePercent) / 100 })), [taxes, subtotal]);
  const taxTotal = taxLines.reduce((s, t) => s + t.amount, 0);
  const total = subtotal + taxTotal;

  async function onSubmit() {
    setApiError(null);
    let draft: Record<string, unknown> = {};
    try { draft = JSON.parse(localStorage.getItem('hms.signup') || '{}'); } catch { /* ignore */ }
    if (!draft.workspace || !draft.businessName) { setApiError('Your signup session expired — please start again.'); return; }

    setStatus('processing');
    try {
      const res = await fetch('/api/v1/tenants', {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          slug: draft.workspace, displayName: draft.businessName, ownerEmail: draft.email,
          countryCode: draft.country, defaultCurrency: draft.currency, timeZone: draft.timezone,
          plan: draft.plan || undefined, addons: draft.addons || undefined,
        }),
      });
      if (!res.ok) {
        const t = await res.text();
        throw new Error(res.status === 409 ? 'That workspace is already taken.' : `Sign-up failed (${res.status}). ${t}`);
      }
      const created = await res.json().catch(() => ({} as { id?: string }));
      localStorage.removeItem('hms.signup');
      const goLogin = () => { setStatus('success'); setTimeout(() => router.push(`/login?workspace=${draft.workspace}`), 1200); };

      // Card-at-signup (when RIT enabled it + the gateway is live): capture a card for the new
      // workspace via PayHere's on-domain popup. Best-effort — whatever happens, the trial proceeds.
      if (requireCard && created?.id) {
        try {
          const payhere = await loadPayHere();
          const pre = await fetch('/api/v1/billing/payhere/preapproval/signup', {
            method: 'POST', headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ tenantId: created.id, firstName: String(draft.businessName || ''), email: String(draft.email || '') }),
          }).then(r => r.json());
          if (pre?.fields) {
            payhere.onCompleted = goLogin; payhere.onDismissed = goLogin; payhere.onError = goLogin;
            payhere.startPayment({ ...pre.fields, amount: Number(pre.fields.amount), sandbox: pre.sandbox, preapprove: true });
            return;
          }
        } catch { /* fall through to login */ }
      }
      goLogin();
    } catch (err) {
      setStatus('idle');
      setApiError((err as Error).message);
    }
  }

  return (
    <main className="min-h-screen bg-surface font-sans text-on-surface">
      <header className="fixed left-0 top-0 z-50 flex w-full items-center justify-between border-b border-border bg-card px-6 py-4">
        <div className="flex items-center gap-3">
          <span className="font-heading text-xl font-black tracking-tight text-primary">RIT HMS</span>
          <div className="mx-2 h-4 w-px bg-border" />
          <span className="text-sm font-medium text-muted-foreground">Step 4 of 4: Confirm</span>
        </div>
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-2">
            {STEPS.map((_, i) =>
              i < ACTIVE_STEP ? (
                <div key={i} className="flex size-6 items-center justify-center rounded-full bg-primary text-white"><Icon name="check" className="text-[14px]" /></div>
              ) : (
                <div key={i} className="flex size-6 items-center justify-center rounded-full border-2 border-primary text-[10px] font-bold text-primary">{i + 1}</div>
              ),
            )}
          </div>
          <Link href="/signup" className="text-muted-foreground transition-colors hover:text-primary" aria-label="Close"><Icon name="close" /></Link>
        </div>
      </header>

      <div className="mx-auto grid max-w-7xl grid-cols-1 gap-8 px-6 pb-12 pt-24 lg:grid-cols-12">
        {/* Left column: trial confirmation (no card needed) */}
        <section className="space-y-6 lg:col-span-7">
          <div className="space-y-2">
            <h1 className="font-heading text-2xl font-bold text-on-surface">Start your 14-day free trial</h1>
            <p className="text-sm text-muted-foreground">
              {requireCard
                ? 'Add your card to start — securely saved (tokenized), not charged until day 14. Cancel anytime before then.'
                : 'No credit card required today. You won’t be charged to get started.'}
            </p>
          </div>

          <div className="space-y-4 rounded-xl border border-primary bg-primary-tint/30 p-6">
            <div className="flex items-center gap-3">
              <Icon name="verified" className="text-3xl text-primary" />
              <div>
                <div className="font-bold text-on-surface">14 days free, full access</div>
                <div className="text-sm text-muted-foreground">Set up your outlets, team and menu — nothing is billed during the trial.</div>
              </div>
            </div>
            <ul className="space-y-2 text-sm text-on-surface">
              {[
                'Full access to every feature in your plan',
                'Invite your team and configure all your outlets',
                'Add a payment method any time in Settings → Subscription & billing',
                'Cancel before day 14 and you won’t be billed',
              ].map(t => (
                <li key={t} className="flex items-center gap-2"><Icon name="check_circle" className="text-lg text-primary" /> {t}</li>
              ))}
            </ul>
          </div>

          <div className="flex items-start gap-3 rounded-xl border border-border bg-card p-5">
            <Icon name="schedule" className="text-[22px] text-primary" />
            <div>
              <div className="font-bold text-on-surface">After your trial</div>
              <p className="text-sm text-muted-foreground">
                From day 15 your subscription is <strong>{cur} {lkr(total)}/month{taxTotal > 0 ? ' (incl. tax)' : ''}</strong>. We&rsquo;ll remind you before it ends — add your card in Settings whenever you&rsquo;re ready.
              </p>
            </div>
          </div>

          <Link href="/signup/network" className="inline-flex items-center gap-2 text-sm font-medium text-muted-foreground transition-colors hover:text-primary">
            <Icon name="arrow_back" className="text-[18px]" /> Back
          </Link>
        </section>

        {/* Right column: order summary */}
        <aside className="lg:col-span-5">
          <div className="sticky top-24 overflow-hidden rounded-xl border border-border bg-card">
            <div className="border-b border-border bg-surface px-6 py-4">
              <h3 className="font-heading font-bold text-on-surface">Order Summary</h3>
            </div>
            <div className="space-y-6 p-6">
              <div className="flex items-start gap-4">
                <div className="flex size-12 items-center justify-center rounded-lg bg-primary-container"><Icon name="restaurant" className="text-primary-dark" /></div>
                <div className="flex-1">
                  <div className="flex justify-between">
                    <span className="font-bold text-on-surface">{plan?.name ?? 'Plan'}</span>
                    <span className="font-bold text-on-surface">{cur} {lkr(plan?.monthlyPrice ?? 0)}</span>
                  </div>
                  <p className="mt-1 text-xs text-muted-foreground">Includes {plan?.includedLocations ?? 0} outlet{(plan?.includedLocations ?? 0) === 1 ? '' : 's'} · {plan?.includedUsers ?? 0} users.</p>
                </div>
              </div>

              {addons.some(a => (addonQty[a.code] ?? 0) > 0) && (
                <div className="space-y-3">
                  <span className="text-[10px] font-bold uppercase tracking-widest text-outline">Selected Add-ons</span>
                  {addons.map(a => { const q = addonQty[a.code] ?? 0; if (q <= 0) return null;
                    const flat = a.unit === 'flat_month'; const amt = flat ? a.unitPrice : a.unitPrice * q;
                    return (
                      <div key={a.code} className="flex items-center justify-between border-b border-border py-2">
                        <span className="text-sm text-on-surface">{flat ? a.name : `${a.name} × ${q}`}</span>
                        <span className="text-sm font-medium text-on-surface">+ {cur} {lkr(amt)}</span>
                      </div>
                    ); })}
                </div>
              )}

              <div className="space-y-3 pt-2">
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">Subtotal</span>
                  <span className="font-medium text-on-surface">{cur} {lkr(subtotal)}</span>
                </div>
                {taxLines.map(t => (
                  <div key={t.code} className="flex justify-between text-sm">
                    <span className="text-muted-foreground">{t.name} ({t.ratePercent}%)</span>
                    <span className="font-medium text-on-surface">{cur} {lkr(t.amount)}</span>
                  </div>
                ))}
                {taxLines.length === 0 && (
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Tax</span>
                    <span className="font-medium text-on-surface">{country === 'LK' ? `${cur} 0.00` : 'Export — zero-rated'}</span>
                  </div>
                )}
                <div className="mt-2 flex items-end justify-between border-t-2 border-dashed border-border pt-4">
                  <div>
                    <p className="text-[10px] font-bold uppercase text-outline">Due today</p>
                    <p className="font-heading text-2xl font-black text-primary">{cur} {lkr(0)}</p>
                    <p className="mt-0.5 text-[11px] text-muted-foreground">14-day free trial — then {cur} {lkr(total)}/month{taxTotal > 0 ? ' incl. tax' : ''}.</p>
                  </div>
                  <span className="pb-1 text-xs text-muted-foreground">Billed monthly</span>
                </div>
              </div>

              {apiError && (
                <p className="mb-3 flex items-center gap-1.5 rounded-lg bg-error/10 px-3 py-2 text-sm text-error"><Icon name="error" className="text-[16px]" /> {apiError}</p>
              )}

              {status === 'success' ? (
                <div className="flex w-full items-center justify-center gap-2 rounded-lg bg-primary-tint py-4 font-bold text-primary-dark"><Icon name="check_circle" /> Workspace created — redirecting…</div>
              ) : (
                <button type="button" disabled={status === 'processing'} onClick={onSubmit}
                  className="group flex w-full items-center justify-center gap-2 rounded-lg bg-primary py-4 font-bold text-white transition-all hover:bg-primary-dark active:scale-[0.98] disabled:cursor-not-allowed disabled:opacity-70">
                  {status === 'processing' ? (<><Icon name="progress_activity" className="animate-spin" /> Creating your workspace…</>)
                    : (<><span>{requireCard ? 'Save card & start trial' : 'Start free trial'}</span><Icon name="arrow_forward" className="transition-transform group-hover:translate-x-1" /></>)}
                </button>
              )}

              <p className="px-4 text-center text-[11px] leading-relaxed text-muted-foreground">
                By clicking &ldquo;Start free trial&rdquo;, you agree to RIT HMS{' '}
                <Link href="#" className="text-primary underline">Terms of Service</Link> and{' '}
                <Link href="#" className="text-primary underline">Privacy Policy</Link>.
              </p>
            </div>
          </div>
        </aside>
      </div>
    </main>
  );
}
