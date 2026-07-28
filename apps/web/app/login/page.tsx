'use client';

import { useState } from 'react';
import Link from 'next/link';
import { Icon } from '@/components/ui/Icon';
import { validate, slug, email as emailRule } from '@/lib/validation';

export default function LoginPage() {
  const [mode, setMode] = useState<'link' | 'pin'>('link');
  const [values, setValues] = useState({ workspace: 'demo', email: 'owner@demo.local' });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);
  const [sentLink, setSentLink] = useState<string | null>(null);
  const [sent, setSent] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);

  // PIN mode — username + PIN (no public staff roster)
  const [username, setUsername] = useState('');
  const [pin, setPin] = useState('');

  const set = (k: string) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setValues(v => ({ ...v, [k]: e.target.value }));
    if (errors[k]) setErrors(er => { const n = { ...er }; delete n[k]; return n; });
  };

  function storeSession(data: { accessToken: string; refreshToken?: string; tenant: unknown; user: { role: number } }) {
    localStorage.setItem('hms.token', data.accessToken);
    if (data.refreshToken) localStorage.setItem('hms.refresh', data.refreshToken);
    localStorage.setItem('hms.tenant', JSON.stringify(data.tenant));
    localStorage.setItem('hms.user', JSON.stringify(data.user));
    const r = data.user.role;
    window.location.href = r === 3 ? '/kot' : r === 2 ? '/pos' : '/dashboard';
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setApiError(null); setSentLink(null); setSent(false);
    const errs = validate(values, { workspace: [slug], email: [emailRule] });
    setErrors(errs);
    if (Object.keys(errs).length) return;
    setSubmitting(true);
    try {
      const res = await fetch('/api/v1/auth/magic-link', {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ tenantSlug: values.workspace.trim(), email: values.email.trim() }),
      });
      if (!res.ok) {
        const t = await res.text();
        throw new Error(res.status === 404 ? 'Workspace or email not found.' : `${res.status} ${t}`);
      }
      const j = await res.json().catch(() => ({} as { devLink?: string }));
      setSentLink(j.devLink ?? null); setSent(true);
    } catch (err) {
      setApiError((err as Error).message);
    } finally { setSubmitting(false); }
  }

  async function pinLogin() {
    if (!values.workspace.trim() || !username.trim() || pin.length < 4) {
      setApiError('Enter workspace, username and a 4–8 digit PIN.'); return;
    }
    setApiError(null); setSubmitting(true);
    try {
      const res = await fetch('/api/v1/auth/pin', {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ tenantSlug: values.workspace.trim(), username: username.trim(), pin }),
      });
      if (res.status === 423) throw new Error('Too many attempts — locked. Try again shortly.');
      if (!res.ok) throw new Error('Wrong username or PIN.');
      storeSession(await res.json());
    } catch (err) { setApiError((err as Error).message); setPin(''); }
    finally { setSubmitting(false); }
  }

  return (
    <main className="flex min-h-screen flex-col bg-surface text-on-surface antialiased">
      <div className="flex flex-grow items-center justify-center p-4 md:p-8">
        <div className="flex min-h-[720px] w-full max-w-[1280px] flex-col overflow-hidden rounded-xl bg-white shadow-2xl md:flex-row">
          {/* LEFT — brand panel */}
          <section className="relative flex w-full flex-col justify-between overflow-hidden p-8 text-white md:w-1/2 md:p-16"
            style={{ background: 'linear-gradient(135deg, #15803d 0%, #00652c 100%)' }}>
            <div className="pointer-events-none absolute inset-0 opacity-5"
              style={{ backgroundImage: 'radial-gradient(circle at 2px 2px, white 1px, transparent 0)', backgroundSize: '40px 40px' }} />
            <div className="z-10 font-headline text-2xl font-extrabold tracking-tight">RIT HMS</div>
            <div className="z-10 max-w-lg">
              <h1 className="mb-6 font-headline text-4xl font-bold leading-tight md:text-5xl">One platform for every outlet</h1>
              <p className="font-body text-lg leading-relaxed text-white/90 md:text-xl">
                POS, KOT, inventory, loyalty, delivery aggregators — built for Sri Lankan hospitality.
              </p>
            </div>
            <div className="z-10 mt-12 flex flex-wrap gap-3">
              {[{ icon: 'hub', label: 'Multi-tenant' }, { icon: 'delivery_dining', label: 'Uber Eats + PickMe ready' }, { icon: 'location_on', label: 'Made in Sri Lanka' }].map(b => (
                <div key={b.label} className="flex items-center gap-2 rounded-lg border border-white/20 bg-white/10 px-4 py-2 backdrop-blur-md">
                  <Icon name={b.icon} className="text-sm" />
                  <span className="text-xs font-semibold uppercase tracking-wide">{b.label}</span>
                </div>
              ))}
            </div>
          </section>

          {/* RIGHT — form */}
          <section className="flex w-full flex-col items-center justify-center bg-white p-8 md:w-1/2 md:p-16">
            <div className="w-full max-w-md">
              <div className="mb-6 text-center md:text-left">
                <h2 className="mb-3 font-headline text-3xl font-bold text-on-surface">Sign in</h2>
              </div>

              {/* Mode toggle */}
              <div className="mb-6 grid grid-cols-2 gap-1 rounded-lg bg-slate-100 p-1">
                <button onClick={() => { setMode('link'); setApiError(null); setSent(false); }}
                  className={`rounded-md py-2 text-sm font-semibold ${mode === 'link' ? 'bg-white text-primary shadow' : 'text-slate-500'}`}>Magic link</button>
                <button onClick={() => { setMode('pin'); setApiError(null); setSent(false); }}
                  className={`rounded-md py-2 text-sm font-semibold ${mode === 'pin' ? 'bg-white text-primary shadow' : 'text-slate-500'}`}>Staff PIN</button>
              </div>

              {mode === 'link' ? (
                <>
                  <p className="mb-6 font-body text-sm text-slate-500">We&apos;ll email you a magic link — no password to remember.</p>
                  <form className="space-y-6" onSubmit={onSubmit} noValidate>
                    <Field label="Workspace" id="workspace" value={values.workspace} onChange={set('workspace')} placeholder="your-tenant-slug" error={errors.workspace} hint="The short name your team uses." />
                    <Field label="Email" id="email" type="email" value={values.email} onChange={set('email')} placeholder="you@example.com" error={errors.email} />
                    <button type="submit" disabled={submitting}
                      className="flex w-full items-center justify-center gap-2 rounded-lg bg-primary px-6 py-3.5 font-headline font-bold text-white shadow-lg shadow-primary/10 transition-all hover:bg-primary-dark active:scale-[0.98] disabled:opacity-60">
                      <span>{submitting ? 'Sending…' : 'Send magic link'}</span><Icon name="auto_fix_high" className="text-xl" />
                    </button>
                  </form>
                  {sent && (
                    <div className="mt-6 rounded-lg border border-primary/30 bg-primary-container/40 p-4 text-sm">
                      <p className="flex items-center gap-2 font-semibold text-primary-dark"><Icon name="mark_email_read" className="text-lg" /> Magic link sent</p>
                      <p className="mt-1 text-slate-600">Check <strong>{values.email.trim()}</strong> (and your spam folder) for the sign-in link. It expires in 15 minutes.</p>
                      {sentLink && <a href={sentLink} className="mt-2 block break-all text-primary underline">{sentLink}</a>}
                    </div>
                  )}
                </>
              ) : (
                <>
                  <p className="mb-6 font-body text-sm text-slate-500">For staff without email — sign in with your username and PIN.</p>
                  <div className="space-y-4">
                    <Field label="Workspace" id="workspace" value={values.workspace} onChange={set('workspace')} placeholder="your-tenant-slug" error={errors.workspace} />
                    <div className="space-y-1.5">
                      <label htmlFor="username" className="block text-sm font-semibold text-slate-700">Username</label>
                      <input id="username" autoComplete="username" value={username} onChange={e => setUsername(e.target.value)}
                        placeholder="e.g. asela"
                        className="w-full rounded-lg border border-slate-200 bg-slate-50 px-4 py-3 transition-all placeholder:text-slate-400 focus:border-primary focus:ring-2 focus:ring-primary/20" />
                    </div>
                    <div className="space-y-1.5">
                      <label htmlFor="pin" className="block text-sm font-semibold text-slate-700">PIN</label>
                      <input id="pin" type="password" inputMode="numeric" value={pin} maxLength={8} autoComplete="off"
                        onChange={e => setPin(e.target.value.replace(/\D/g, ''))}
                        onKeyDown={e => { if (e.key === 'Enter') void pinLogin(); }}
                        placeholder="••••"
                        className="w-full rounded-lg border border-slate-200 bg-slate-50 px-4 py-3.5 text-center text-2xl tracking-[0.4em] focus:border-primary focus:ring-2 focus:ring-primary/20" />
                    </div>
                    <button onClick={pinLogin} disabled={submitting || pin.length < 4 || !username.trim()}
                      className="flex w-full items-center justify-center gap-2 rounded-lg bg-primary px-6 py-3.5 font-headline font-bold text-white hover:bg-primary-dark active:scale-[0.98] disabled:opacity-60">
                      {submitting ? 'Signing in…' : 'Sign in'} <Icon name="login" className="text-lg" />
                    </button>
                  </div>
                </>
              )}

              {apiError && (
                <p className="mt-4 flex items-center gap-1.5 text-sm text-error"><Icon name="error" className="text-base" /> {apiError}</p>
              )}

              <div className="mt-10 border-t border-slate-100 pt-8 text-center">
                <Link href="/signup" className="group flex items-center justify-center gap-1 font-medium text-primary transition-colors hover:text-primary-dark">
                  First time here? Create a workspace <Icon name="arrow_forward" className="text-lg transition-transform group-hover:translate-x-1" />
                </Link>
              </div>
            </div>
          </section>
        </div>
      </div>
      <footer className="w-full py-8 text-center">
        <p className="font-body text-xs tracking-wide text-slate-400">© 2026 Retail IT (Pvt) Ltd <span className="mx-2 opacity-50">·</span> Privacy <span className="mx-2 opacity-50">·</span> Terms</p>
      </footer>
    </main>
  );
}

function Field({ label, id, value, onChange, placeholder, error, hint, type = 'text' }: {
  label: string; id: string; value: string; placeholder?: string; error?: string; hint?: string; type?: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}) {
  return (
    <div className="space-y-1.5">
      <label htmlFor={id} className="block text-sm font-semibold text-slate-700">{label}</label>
      <input id={id} name={id} type={type} value={value} onChange={onChange} placeholder={placeholder} aria-invalid={!!error}
        className={`w-full rounded-lg border bg-slate-50 px-4 py-3 transition-all placeholder:text-slate-400 focus:ring-2 ${error ? 'border-error focus:border-error focus:ring-error/20' : 'border-slate-200 focus:border-primary focus:ring-primary/20'}`} />
      {error ? (
        <p className="mt-1 flex items-center gap-1 text-xs text-error"><Icon name="error" className="text-[14px]" /> {error}</p>
      ) : hint ? (
        <p className="mt-1 flex items-center gap-1 text-xs text-slate-400"><Icon name="info" className="text-[14px]" /> {hint}</p>
      ) : null}
    </div>
  );
}
