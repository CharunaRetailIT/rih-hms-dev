import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { api } from '@/lib/utils';

type HealthResponse = { status: string };

async function pingApi(): Promise<{ ok: boolean; status?: string; error?: string }> {
  try {
    const data = await api<HealthResponse>('/health');
    return { ok: true, status: data.status };
  } catch (e) {
    return { ok: false, error: (e as Error).message };
  }
}

export default async function HomePage() {
  const health = await pingApi();

  return (
    <main className="mx-auto max-w-4xl px-6 py-16">
      <header className="mb-12">
        <p className="text-sm uppercase tracking-wider text-muted-foreground">
          Retail IT · Cloud Platform
        </p>
        <h1 className="mt-2 text-4xl font-bold tracking-tight">RIT HMS</h1>
        <p className="mt-3 max-w-2xl text-lg text-muted-foreground">
          Multi-tenant hospitality management — POS, KOT/BOT, inventory, and
          delivery aggregators. Built in the open at{' '}
          <Link
            href="https://github.com/mubs62/rit-hms"
            className="text-primary underline-offset-4 hover:underline"
          >
            github.com/mubs62/rit-hms
          </Link>
          .
        </p>
      </header>

      <section className="mb-12 rounded-lg border border-border bg-card p-6">
        <h2 className="mb-3 text-lg font-semibold">API status</h2>
        {health.ok ? (
          <p className="text-sm">
            <span className="inline-block size-2 rounded-full bg-green-500 align-middle" />{' '}
            <span className="ml-2 align-middle font-medium">Healthy</span>
            <span className="ml-2 align-middle text-muted-foreground">
              ({health.status})
            </span>
          </p>
        ) : (
          <div>
            <p className="text-sm">
              <span className="inline-block size-2 rounded-full bg-destructive align-middle" />{' '}
              <span className="ml-2 align-middle font-medium">Unreachable</span>
            </p>
            <p className="mt-2 text-xs text-muted-foreground">
              Start the API with <code className="rounded bg-muted px-1 py-0.5">make api</code> in
              a separate terminal.
            </p>
            {health.error && (
              <pre className="mt-2 overflow-auto rounded bg-muted p-3 text-xs">{health.error}</pre>
            )}
          </div>
        )}
      </section>

      <section className="grid gap-4 sm:grid-cols-2">
        <Link
          href="/login"
          className="rounded-lg border border-border bg-card p-6 transition-colors hover:bg-muted"
        >
          <h3 className="text-lg font-semibold">Sign in</h3>
          <p className="mt-1 text-sm text-muted-foreground">
            Magic-link login for the demo tenant.
          </p>
        </Link>
        <Link
          href="http://localhost:5000/swagger"
          className="rounded-lg border border-border bg-card p-6 transition-colors hover:bg-muted"
        >
          <h3 className="text-lg font-semibold">API docs</h3>
          <p className="mt-1 text-sm text-muted-foreground">
            Interactive Swagger UI for the backend.
          </p>
        </Link>
      </section>

      <footer className="mt-16 border-t border-border pt-6 text-xs text-muted-foreground">
        <p>Sprint 1 · foundation only. No POS yet. See <code>docs/v2-v1-scope.md</code>.</p>
      </footer>
    </main>
  );
}
