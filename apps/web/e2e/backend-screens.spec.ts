import { test, expect, Page, APIRequestContext } from '@playwright/test';

async function login(page: Page, request: APIRequestContext) {
  const r = await request.post('http://localhost:5000/api/v1/auth/magic-link', {
    data: { tenantSlug: 'demo', email: 'owner@demo.local' },
    headers: { 'Content-Type': 'application/json' },
  });
  const { devLink } = await r.json();
  await page.goto(devLink);
  await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 });
}

test('production page renders recipes + production', async ({ page, request }) => {
  await login(page, request);
  await page.goto('/production');
  await expect(page.getByRole('heading', { level: 1, name: 'Production' })).toBeVisible();
  await expect(page.getByText(/recipe/i).first()).toBeVisible();
  await expect(page.getByRole('button', { name: /produce/i }).first()).toBeVisible();
});

test('reports page renders sales + VAT', async ({ page, request }) => {
  await login(page, request);
  await page.goto('/reports');
  await expect(page.getByRole('heading', { level: 1, name: 'Reports' })).toBeVisible();
  await expect(page.getByText(/tax/i).first()).toBeVisible();
  await expect(page.getByRole('button', { name: /run/i }).first()).toBeVisible();
});

test('delivery page renders aggregator console', async ({ page, request }) => {
  await login(page, request);
  await page.goto('/delivery');
  await expect(page.getByRole('heading', { level: 1, name: 'Delivery' })).toBeVisible();
  // tab strip
  await expect(page.getByText(/orders/i).first()).toBeVisible();
});

test('sidebar links to the new screens', async ({ page, request }) => {
  await login(page, request);
  await page.goto('/dashboard');
  const nav = page.locator('aside');
  await expect(nav.getByRole('link', { name: 'Production' })).toBeVisible();
  await expect(nav.getByRole('link', { name: 'Delivery' })).toBeVisible();
  // "Reports" is a collapsed accordion section by default — expand it first.
  await nav.getByRole('button', { name: 'Reports' }).click();
  await expect(nav.getByRole('link', { name: 'Reports' })).toBeVisible();
});
