import { test, expect } from '@playwright/test';

/**
 * No-crash guard: log in once, visit every authenticated screen, and FAIL if any
 * screen triggers a server error (HTTP ≥ 500) or an uncaught page error. This is
 * the test that would have caught the Reports 500 (timestamptz / Kind=Unspecified).
 * Shallow "renders" checks live in the per-screen specs; this is breadth.
 */
const ROUTES = [
  '/dashboard', '/pos', '/kot', '/menu', '/modifiers', '/inventory',
  '/suppliers', '/purchasing', '/transfers', '/production', '/reports',
  '/delivery', '/team', '/settings',
];

test('every screen loads without a 5xx or uncaught error', async ({ page, request }) => {
  test.setTimeout(120_000);
  const fatal: string[] = [];
  let current = '(login)';
  page.on('pageerror', e => fatal.push(`[${current}] pageerror: ${e.message}`));
  page.on('response', r => { if (r.status() >= 500) fatal.push(`[${current}] HTTP ${r.status()} ${r.request().method()} ${r.url().replace('http://localhost:3000', '')}`); });

  const res = await request.post('http://localhost:5000/api/v1/auth/magic-link', {
    data: { tenantSlug: 'demo', email: 'owner@demo.local' }, headers: { 'Content-Type': 'application/json' },
  });
  await page.goto((await res.json()).devLink);
  await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 });

  for (const route of ROUTES) {
    current = route;
    // /pos and /kot keep an SSE stream open, so 'networkidle' never fires for them.
    await page.goto(route, { waitUntil: 'load', timeout: 20_000 }).catch(e => fatal.push(`[${route}] nav: ${e.message}`));
    await page.waitForTimeout(600);
  }

  expect(fatal, `screens with server errors / crashes:\n${fatal.join('\n')}`).toEqual([]);
});
