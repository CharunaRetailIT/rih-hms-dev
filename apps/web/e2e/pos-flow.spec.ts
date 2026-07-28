import { test, expect } from '@playwright/test';

/**
 * FLOW AUDIT — drives the core money path end-to-end in the browser and reports
 * any console error / failed network call that fires during the flow. Opens a
 * shift first (billing is gated on an open shift), then new order via the table
 * picker → add item → KOT → settle.
 */
test('POS money path: shift → new order → add item → KOT → settle', async ({ page, request }) => {
  test.setTimeout(90_000);
  const errors: string[] = [];
  page.on('pageerror', e => errors.push(`pageerror: ${e.message}`));
  page.on('console', m => { if (m.type() === 'error' && !m.text().includes('favicon')) errors.push(`console: ${m.text().slice(0, 200)}`); });
  page.on('response', r => { if (r.status() >= 400 && !r.url().includes('favicon')) errors.push(`HTTP ${r.status()} ${r.request().method()} ${r.url().replace('http://localhost:3000', '')}`); });

  const magic = async () => (await (await request.post('http://localhost:5000/api/v1/auth/magic-link', {
    data: { tenantSlug: 'demo', email: 'owner@demo.local' }, headers: { 'Content-Type': 'application/json' },
  })).json()).devLink as string;

  // API JWT (consumes one link) to ensure an open shift exists; a fresh link for the browser.
  const jwt = (await (await request.post('http://localhost:5000/api/v1/auth/exchange', {
    data: { token: new URL(await magic()).searchParams.get('token') }, headers: { 'Content-Type': 'application/json' },
  })).json()).accessToken;
  const auth = { Authorization: `Bearer ${jwt}`, 'Content-Type': 'application/json' };
  const locs = await (await request.get('http://localhost:5000/api/v1/locations', { headers: auth })).json();
  const loc = locs.find((l: { code: string }) => l.code === 'MAIN') ?? locs[0];
  await request.post('http://localhost:5000/api/v1/shifts/open', { headers: auth, data: { locationId: loc.id, openingFloat: 1000 } }); // ignore if already open
  const devLink = await magic();

  await page.goto(devLink);
  await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 });
  // /pos keeps an SSE stream open, so 'networkidle' never fires.
  await page.goto('/pos', { waitUntil: 'load' });
  await page.waitForTimeout(500);

  // 1) new order → table picker → takeaway (no table dependency)
  await page.locator('button[title*="New order"]').click();
  await page.getByRole('button', { name: /Takeaway/ }).click();
  await page.waitForTimeout(500);

  // 2) add an item (no variants/modifiers to avoid the picker)
  await page.getByRole('button').filter({ hasText: 'Pol Sambol Rice' }).first().click();
  const addBtn = page.getByRole('button', { name: 'Add to order' });
  if (await addBtn.isVisible().catch(() => false)) await addBtn.click();
  await page.waitForTimeout(600);
  await expect(page.getByText('Pol Sambol Rice').first()).toBeVisible();

  // 3) send to KOT (button now reads "Send to Kitchen")
  await page.getByRole('button', { name: /send to kitchen/i }).first().click();
  await page.waitForTimeout(800);

  // 4) settle (Pay — auto-tenders full cash)
  await page.getByRole('button', { name: /^Pay · LKR/ }).first().click();
  await page.waitForTimeout(1000);

  const settled = await page.getByText(/Settled/).isVisible().catch(() => false);
  console.log(errors.length ? `ERRORS: ${errors.join(' | ')}` : 'flow clean ✅');
  expect(errors, `flow errors:\n${errors.join('\n')}`).toEqual([]);
  expect(settled, 'order should settle and show a Settled toast').toBeTruthy();
});
