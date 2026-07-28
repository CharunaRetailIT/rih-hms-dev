'use client';

import { Suspense, useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';

type State =
  | { kind: 'pending' }
  | { kind: 'success'; tenantSlug: string; userEmail: string; redirectAt: number }
  | { kind: 'error'; message: string };

function AuthCallbackInner() {
  const router = useRouter();
  const params = useSearchParams();
  const token = params.get('token');
  const [state, setState] = useState<State>({ kind: 'pending' });

  useEffect(() => {
    if (!token) {
      setState({ kind: 'error', message: 'Missing token in URL.' });
      return;
    }

    let cancelled = false;
    (async () => {
      try {
        const res = await fetch('/api/v1/auth/exchange', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ token }),
        });

        if (!res.ok) {
          const body = await res.text();
          throw new Error(
            res.status === 401
              ? 'This magic link is expired or already used.'
              : `${res.status}: ${body || 'unknown error'}`
          );
        }

        const data = await res.json();
        if (cancelled) return;

        // v1: store the JWT in localStorage. v2 will move to httpOnly cookie
        // set by the API for CSRF resistance.
        localStorage.setItem('hms.token', data.accessToken);
        if (data.refreshToken) localStorage.setItem('hms.refresh', data.refreshToken);
        localStorage.setItem('hms.tenant', JSON.stringify(data.tenant));
        localStorage.setItem('hms.user', JSON.stringify(data.user));

        setState({
          kind: 'success',
          tenantSlug: data.tenant.slug,
          userEmail: data.user.email,
          redirectAt: Date.now() + 1500,
        });

        setTimeout(() => router.push('/dashboard'), 1500);
      } catch (e) {
        if (!cancelled) setState({ kind: 'error', message: (e as Error).message });
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [token, router]);

  return (
    <main className="mx-auto max-w-md px-6 py-16">
      {state.kind === 'pending' && (
        <div className="rounded-lg border border-border bg-card p-6 text-center">
          <div className="mx-auto mb-3 size-8 animate-spin rounded-full border-2 border-border border-t-primary" />
          <h1 className="text-lg font-semibold">Signing you in…</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Exchanging your magic link for a session.
          </p>
        </div>
      )}

      {state.kind === 'success' && (
        <div className="rounded-lg border border-border bg-card p-6 text-center">
          <div className="mx-auto mb-3 flex size-10 items-center justify-center rounded-full bg-primary/10 text-primary">
            ✓
          </div>
          <h1 className="text-lg font-semibold">Welcome back</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Signed in as <span className="font-mono">{state.userEmail}</span>
          </p>
          <p className="mt-1 text-sm text-muted-foreground">
            Tenant: <span className="font-mono">{state.tenantSlug}</span>
          </p>
          <p className="mt-4 text-xs text-muted-foreground">Redirecting to dashboard…</p>
        </div>
      )}

      {state.kind === 'error' && (
        <div className="rounded-lg border border-destructive/50 bg-card p-6">
          <h1 className="text-lg font-semibold text-destructive">Sign-in failed</h1>
          <p className="mt-2 text-sm">{state.message}</p>
          <Link
            href="/login"
            className="mt-4 inline-block text-sm text-primary underline underline-offset-4"
          >
            ← Back to login
          </Link>
        </div>
      )}
    </main>
  );
}

export default function AuthCallbackPage() {
  // useSearchParams() must sit under a Suspense boundary (Next 15 CSR bailout).
  return (
    <Suspense
      fallback={
        <main className="mx-auto max-w-md px-6 py-16">
          <div className="rounded-lg border border-border bg-card p-6 text-center">
            <div className="mx-auto mb-3 size-8 animate-spin rounded-full border-2 border-border border-t-primary" />
            <h1 className="text-lg font-semibold">Signing you in…</h1>
          </div>
        </main>
      }
    >
      <AuthCallbackInner />
    </Suspense>
  );
}
