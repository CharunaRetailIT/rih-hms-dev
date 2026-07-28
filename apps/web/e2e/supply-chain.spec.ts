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

test('inventory page shows stock on hand', async ({ page, request }) => {
  await login(page, request);
  await page.goto('/inventory');
  await expect(page.getByText(/stock on hand/i).first()).toBeVisible();
  // Search for it explicitly — other e2e specs create throwaway products that
  // can push it past the first page of the unfiltered, client-paginated list.
  // Match by name pattern rather than one exact variety — which Kottu product
  // is seeded (e.g. "Chicken Kottu" vs "Cheese Kottu") can vary by seed run.
  await page.getByPlaceholder('Search by name or Product Code').fill('Kottu');
  await expect(page.getByText(/kottu/i).first()).toBeVisible();
});

test('suppliers page renders', async ({ page, request }) => {
  await login(page, request);
  await page.goto('/suppliers');
  await expect(page.getByRole('heading', { name: /suppliers/i }).first()).toBeVisible();
  await expect(page.getByRole('button', { name: /new supplier/i }).first()).toBeVisible();
});

test('purchasing page renders', async ({ page, request }) => {
  await login(page, request);
  await page.goto('/purchasing');
  await expect(page.getByText(/purchase order|new po/i).first()).toBeVisible();
});

test('transfers page renders', async ({ page, request }) => {
  await login(page, request);
  await page.goto('/transfers');
  await expect(page.getByText(/transfer/i).first()).toBeVisible();
});
