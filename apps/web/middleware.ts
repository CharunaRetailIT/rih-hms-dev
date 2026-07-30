import { NextRequest } from 'next/server';

// Manually proxies /api/* to the .NET API instead of relying on next.config.mjs's
// `rewrites()` — on Cloudflare Workers (OpenNext), that built-in rewrite-to-an-
// external-URL mechanism crashes ("Error in routingHandler ... at handleRewrites")
// on every request, regardless of @opennextjs/cloudflare version. Middleware runs
// before that code path, so it sidesteps the bug. Server-side fetches (lib/utils.ts's
// `api()` when `typeof window === 'undefined'`) already call API_BASE_URL directly
// and never hit this — only browser-issued fetch('/api/...') calls need this proxy.
const apiBase = process.env.API_BASE_URL ?? 'http://localhost:5000';

export async function middleware(req: NextRequest) {
  const target = new URL(req.nextUrl.pathname + req.nextUrl.search, apiBase);

  const headers = new Headers(req.headers);
  headers.delete('host');

  const hasBody = !['GET', 'HEAD'].includes(req.method);
  const init: RequestInit & { duplex?: 'half' } = {
    method: req.method,
    headers,
    body: hasBody ? req.body : undefined,
    redirect: 'manual',
  };
  if (hasBody) init.duplex = 'half';

  return fetch(target, init);
}

export const config = {
  matcher: '/api/:path*',
};
