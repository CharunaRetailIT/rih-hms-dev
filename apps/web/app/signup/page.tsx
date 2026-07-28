'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { Icon } from '@/components/ui/Icon';
import { validate, required, email as emailRule, slug, phoneLK } from '@/lib/validation';

const STEPS = ['Workspace details', 'Choose plan', 'Configure Network', 'Payment'];

export default function SignupPage() {
  const router = useRouter();
  const [v, setV] = useState({
    businessName: '', workspace: '', country: 'LK', currency: 'LKR',
    timezone: 'Asia/Colombo', fullName: '', email: '', mobile: '',
  });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [slugState, setSlugState] = useState<'idle' | 'checking' | 'available' | 'taken'>('idle');
  const [submitting, setSubmitting] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);

  const set = (k: string) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    setV(s => ({ ...s, [k]: e.target.value }));
    if (errors[k]) setErrors(er => { const n = { ...er }; delete n[k]; return n; });
  };

  // derive a slug from the business name if the user hasn't typed one
  function onBusinessName(e: React.ChangeEvent<HTMLInputElement>) {
    const name = e.target.value;
    setV(s => ({ ...s, businessName: name, workspace: s.workspace || autoSlug(name) }));
    if (errors.businessName) setErrors(er => { const n = { ...er }; delete n.businessName; return n; });
  }

  async function checkSlug(value: string) {
    if (!/^[a-z0-9][a-z0-9-]{1,58}[a-z0-9]$/.test(value)) { setSlugState('idle'); return; }
    setSlugState('checking');
    try {
      const res = await fetch(`/api/v1/tenants/by-slug/${value}`);
      setSlugState(res.ok ? 'taken' : 'available');
    } catch { setSlugState('idle'); }
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setApiError(null);
    const errs = validate(v, {
      businessName: [required('Business name')],
      workspace: [slug],
      fullName: [required('Your full name')],
      email: [emailRule],
      mobile: [phoneLK],
    });
    setErrors(errs);
    if (Object.keys(errs).length) return;
    if (slugState === 'taken') { setErrors(er => ({ ...er, workspace: 'That workspace is taken.' })); return; }

    // Carry the workspace details forward; the tenant + subscription are created at
    // the final Payment step (after plan + add-ons are chosen). #109
    try { localStorage.setItem('hms.signup', JSON.stringify({ ...v, workspace: v.workspace.trim() })); } catch { /* ignore */ }
    router.push('/signup/plan');
  }

  return (
    <main className="min-h-screen bg-slate-50 text-on-surface">
      {/* top nav */}
      <nav className="flex items-center justify-between border-b border-slate-200 bg-white px-6 py-3">
        <span className="font-headline text-lg font-extrabold">RIT HMS</span>
        <div className="flex items-center gap-5 text-sm text-slate-600">
          <Link href="#" className="hover:text-on-surface">Pricing</Link>
          <Link href="#" className="hover:text-on-surface">Demo</Link>
          <Link href="/login" className="font-medium text-primary hover:text-primary-dark">Sign in</Link>
        </div>
      </nav>

      <div className="mx-auto max-w-[720px] px-4 py-10">
        {/* hero */}
        <p className="text-xs font-semibold uppercase tracking-widest text-primary">Get started</p>
        <h1 className="mt-2 font-headline text-3xl font-bold md:text-4xl">Set up your workspace in 60 seconds.</h1>
        <p className="mt-3 text-slate-500">
          Your own database, your own login URL, your own everything. Pay nothing for 14 days.
        </p>

        {/* stepper */}
        <div className="mt-8 flex items-center gap-2 text-sm">
          {STEPS.map((s, i) => (
            <div key={s} className="flex items-center gap-2">
              <span className={`flex size-6 items-center justify-center rounded-full text-xs font-bold ${i === 0 ? 'bg-primary text-white' : 'bg-slate-200 text-slate-500'}`}>{i + 1}</span>
              <span className={i === 0 ? 'font-medium text-on-surface' : 'text-slate-400'}>{s}</span>
              {i < STEPS.length - 1 && <Icon name="chevron_right" className="text-slate-300" />}
            </div>
          ))}
        </div>

        {/* form card */}
        <form onSubmit={onSubmit} noValidate className="mt-6 rounded-xl border border-slate-200 bg-white p-6 md:p-8">
          <h2 className="mb-5 font-headline text-lg font-bold">Workspace details</h2>

          <Field label="Business name" id="businessName" value={v.businessName} onChange={onBusinessName}
            placeholder="e.g. Spice Garden Restaurant" error={errors.businessName}
            hint="Shown on receipts and the app header." />

          {/* workspace URL with availability */}
          <div className="mt-4 space-y-1.5">
            <label className="block text-sm font-semibold text-slate-700">Workspace URL</label>
            <div className="flex items-stretch">
              <span className="inline-flex items-center rounded-l-lg border border-r-0 border-slate-200 bg-slate-100 px-3 text-sm text-slate-500">rithms.lk/</span>
              <input value={v.workspace}
                onChange={e => { set('workspace')(e); setSlugState('idle'); }}
                onBlur={e => checkSlug(e.target.value.trim())}
                placeholder="spice-garden"
                className={`w-full border px-4 py-3 transition-all focus:ring-2 ${errors.workspace ? 'border-error focus:ring-error/20' : 'border-slate-200 focus:border-primary focus:ring-primary/20'}`} />
              <span className="inline-flex items-center rounded-r-lg border border-l-0 border-slate-200 bg-white px-3">
                {slugState === 'checking' && <Icon name="progress_activity" className="animate-spin text-slate-400" />}
                {slugState === 'available' && <Icon name="check_circle" className="text-primary" />}
                {slugState === 'taken' && <Icon name="cancel" className="text-error" />}
              </span>
            </div>
            {errors.workspace
              ? <p className="flex items-center gap-1 text-xs text-error"><Icon name="error" className="text-[14px]" /> {errors.workspace}</p>
              : slugState === 'available'
                ? <p className="flex items-center gap-1 text-xs text-primary"><Icon name="check" className="text-[14px]" /> Available</p>
                : <p className="text-xs text-slate-400">The short name your team will type to sign in.</p>}
          </div>

          {/* country / currency / tz */}
          <div className="mt-4 grid grid-cols-1 gap-4 md:grid-cols-3">
            <SelectField label="Country" id="country" value={v.country} onChange={set('country')}
              options={[['LK', '🇱🇰 Sri Lanka'], ['IN', '🇮🇳 India'], ['AE', '🇦🇪 UAE']]} />
            <SelectField label="Default currency" id="currency" value={v.currency} onChange={set('currency')}
              options={[['LKR', 'LKR — Sri Lankan Rupee'], ['USD', 'USD — US Dollar'], ['AED', 'AED — UAE Dirham']]} />
            <SelectField label="Time zone" id="timezone" value={v.timezone} onChange={set('timezone')}
              options={[['Asia/Colombo', 'Asia/Colombo (UTC+05:30)'], ['Asia/Dubai', 'Asia/Dubai (UTC+04:00)']]} />
          </div>

          <h2 className="mb-4 mt-8 font-headline text-lg font-bold">Owner identity</h2>
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            <Field label="Your full name" id="fullName" value={v.fullName} onChange={set('fullName')} placeholder="Asela Perera" error={errors.fullName} />
            <Field label="Your email" id="email" type="email" value={v.email} onChange={set('email')} placeholder="owner@spicegarden.lk" error={errors.email} hint="We'll send a magic link to confirm." />
          </div>
          <div className="mt-4">
            <Field label="Mobile (optional)" id="mobile" value={v.mobile} onChange={set('mobile')} placeholder="+94 77 123 4567" error={errors.mobile} />
          </div>

          {apiError && <p className="mt-4 flex items-center gap-1.5 text-sm text-error"><Icon name="error" /> {apiError}</p>}

          <button type="submit" disabled={submitting}
            className="mt-6 flex w-full items-center justify-center gap-2 rounded-lg bg-primary px-6 py-3.5 font-headline font-bold text-white hover:bg-primary-dark active:scale-[0.99] disabled:opacity-60">
            {submitting ? 'Creating…' : 'Continue to plan'} <Icon name="arrow_forward" />
          </button>
          <p className="mt-3 text-center text-sm text-slate-500">
            Already have a workspace? <Link href="/login" className="font-medium text-primary">Sign in →</Link>
          </p>
        </form>

        {/* value props */}
        <div className="mt-8 grid grid-cols-1 gap-4 md:grid-cols-3">
          <Prop icon="database" title="DB-per-tenant" body="Your data lives in its own Postgres database. Strong isolation by default." />
          <Prop icon="bolt" title="Auto-provisioned" body="We create your database, run migrations, and seed reference data while you onboard." />
          <Prop icon="public" title="Built for Sri Lanka" body="GST, service charge, LKR-first, Sinhala/Tamil support in v2." />
        </div>

        <footer className="mt-12 border-t border-slate-200 pt-6 text-center text-xs text-slate-400">
          © 2026 Retail IT (Pvt) Ltd · Privacy · Terms · System Status
        </footer>
      </div>
    </main>
  );
}

function autoSlug(name: string) {
  return name.toLowerCase().trim().replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '').slice(0, 60);
}

function Field({ label, id, value, onChange, placeholder, error, hint, type = 'text' }: {
  label: string; id: string; value: string; placeholder?: string; error?: string; hint?: string; type?: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}) {
  return (
    <div className="space-y-1.5">
      <label htmlFor={id} className="block text-sm font-semibold text-slate-700">{label}</label>
      <input id={id} type={type} value={value} onChange={onChange} placeholder={placeholder} aria-invalid={!!error}
        className={`w-full rounded-lg border bg-slate-50 px-4 py-3 transition-all placeholder:text-slate-400 focus:ring-2 ${error ? 'border-error focus:ring-error/20' : 'border-slate-200 focus:border-primary focus:ring-primary/20'}`} />
      {error ? <p className="flex items-center gap-1 text-xs text-error"><Icon name="error" className="text-[14px]" /> {error}</p>
        : hint ? <p className="text-xs text-slate-400">{hint}</p> : null}
    </div>
  );
}

function SelectField({ label, id, value, onChange, options }: {
  label: string; id: string; value: string; options: [string, string][];
  onChange: (e: React.ChangeEvent<HTMLSelectElement>) => void;
}) {
  return (
    <div className="space-y-1.5">
      <label htmlFor={id} className="block text-sm font-semibold text-slate-700">{label}</label>
      <select id={id} value={value} onChange={onChange}
        className="w-full rounded-lg border border-slate-200 bg-slate-50 px-4 py-3 focus:border-primary focus:ring-2 focus:ring-primary/20">
        {options.map(([val, lbl]) => <option key={val} value={val}>{lbl}</option>)}
      </select>
    </div>
  );
}

function Prop({ icon, title, body }: { icon: string; title: string; body: string }) {
  return (
    <div className="rounded-lg border border-slate-200 bg-white p-4">
      <Icon name={icon} className="text-primary" />
      <h3 className="mt-1 font-semibold">{title}</h3>
      <p className="mt-1 text-sm text-slate-500">{body}</p>
    </div>
  );
}
