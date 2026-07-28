import { test, expect } from '@playwright/test';

/** The KOT clock rendered Date.now() during SSR → hydration mismatch. Guard. */
test('KOT page hydrates without a mismatch error', async ({ page, request }) => {
  const hydration: string[] = [];
  page.on('console', m => {
    const t = m.text();
    if (/hydrat|didn't match|did not match/i.test(t)) hydration.push(t.slice(0, 160));
  });
  page.on('pageerror', e => { if (/hydrat/i.test(e.message)) hydration.push(`pageerror: ${e.message.slice(0, 160)}`); });

  const { devLink } = await (await request.post('http://localhost:5000/api/v1/auth/magic-link', {
    data: { tenantSlug: 'demo', email: 'owner@demo.local' }, headers: { 'Content-Type': 'application/json' },
  })).json();
  await page.goto(devLink);
  await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 });
  // /kot keeps an SSE stream open, so 'networkidle' never fires.
  await page.goto('/kot', { waitUntil: 'load' });
  await page.waitForTimeout(1500);

  expect(hydration, `hydration issues:\n${hydration.join('\n')}`).toEqual([]);
});
