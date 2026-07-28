import Link from 'next/link';

/** Branded 404 — shown for any unmatched route, app-wide. */
export default function NotFound() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-background px-6 text-center">
      <div className="mb-6 flex size-14 items-center justify-center rounded-xl bg-primary font-heading text-2xl font-extrabold text-primary-foreground shadow-lg shadow-primary/20">
        R
      </div>
      <p className="font-heading text-7xl font-black leading-none text-primary">404</p>
      <h1 className="mt-3 font-heading text-2xl font-bold">Page not found</h1>
      <p className="mt-2 max-w-md text-sm text-muted-foreground">
        The page you’re looking for doesn’t exist or may have moved. Check the address, or head back to your dashboard.
      </p>
      <div className="mt-6 flex flex-wrap items-center justify-center gap-3">
        <Link href="/dashboard" className="rounded-lg bg-primary px-5 py-2.5 font-bold text-primary-foreground hover:bg-primary-dark">
          Back to dashboard
        </Link>
        <Link href="/login" className="rounded-lg border border-border px-5 py-2.5 font-semibold hover:bg-muted">
          Sign in
        </Link>
      </div>
      <p className="mt-12 text-xs font-semibold uppercase tracking-wider text-muted-foreground">RIT HMS</p>
    </div>
  );
}
