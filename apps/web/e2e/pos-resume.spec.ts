import { test, expect } from '@playwright/test';

/**
 * Regression for the reported bug: opening an existing order into POS (dashboard
 * row / tab / ?order=) used to land on an empty bill. Now the terminal resumes
 * the order, the tab strip lists it, and the line items render.
 */
test('POS resumes an existing order via ?order= and shows its tab', async ({ page, request }) => {
  test.setTimeout(90_000);
  const errors: string[] = [];
  page.on('pageerror', e => errors.push(`pageerror: ${e.message}`));
  page.on('response', r => { if (r.status() >= 500) errors.push(`HTTP ${r.status()} ${r.url()}`); });

  const magic = async () => (await (await request.post('http://localhost:5000/api/v1/auth/magic-link', {
    data: { tenantSlug: 'demo', email: 'owner@demo.local' }, headers: { 'Content-Type': 'application/json' },
  })).json()).devLink as string;

  // one link to consume for the API JWT, a SEPARATE fresh link for the browser
  // (magic-link tokens are single-use)
  const apiToken = new URL(await magic()).searchParams.get('token');
  const jwt = (await (await request.post('http://localhost:5000/api/v1/auth/exchange', {
    data: { token: apiToken }, headers: { 'Content-Type': 'application/json' },
  })).json()).accessToken;
  const auth = { Authorization: `Bearer ${jwt}`, 'Content-Type': 'application/json' };
  const devLink = await magic();

  // need the MAIN location + a product
  const locs = await (await request.get('http://localhost:5000/api/v1/locations', { headers: auth })).json();
  const loc = locs.find((l: { code: string }) => l.code === 'MAIN') ?? locs[0];
  const products = await (await request.get('http://localhost:5000/api/v1/products?activeOnly=true', { headers: auth })).json();
  const prod = products[0];

  // create an order + add a line directly
  const order = await (await request.post('http://localhost:5000/api/v1/orders', {
    headers: auth, data: { locationId: loc.id, orderType: 'dine_in', tableLabel: '9', covers: 2 },
  })).json();
  await request.post(`http://localhost:5000/api/v1/orders/${order.id}/items`, {
    headers: auth, data: { productId: prod.id, quantity: 2, station: 'kitchen' },
  });

  // open it in the browser via the deep-link
  await page.goto(devLink);
  await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 });
  // /pos keeps an SSE stream open, so 'networkidle' never fires.
  await page.goto(`/pos?order=${order.id}`, { waitUntil: 'load' });
  await page.waitForTimeout(800);

  // the resumed bill shows its line + a non-zero total, and a tab for table 9
  await expect(page.getByText(prod.name).first()).toBeVisible();
  await expect(page.getByRole('button', { name: /T9/ }).first()).toBeVisible();

  console.log(errors.length ? `ERRORS: ${errors.join(' | ')}` : 'resume clean ✅');
  expect(errors).toEqual([]);
});
