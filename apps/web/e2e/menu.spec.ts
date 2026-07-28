import { test, expect } from '@playwright/test';

/** Seed a browser session (the app reads the JWT from localStorage). */
async function login(page: import('@playwright/test').Page, request: import('@playwright/test').APIRequestContext) {
  const link = await request.post('http://localhost:5000/api/v1/auth/magic-link', {
    data: { tenantSlug: 'demo', email: 'owner@demo.local' },
    headers: { 'Content-Type': 'application/json' },
  });
  const { devLink } = await link.json();
  await page.goto(devLink);
  await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 });
}

test('menu page lists seeded products', async ({ page, request }) => {
  await login(page, request);
  await page.goto('/menu');

  // The menu page should render its Products heading/label.
  await expect(page.getByText('Products', { exact: false }).first()).toBeVisible();

  // A known seeded product should be listed — search for it explicitly since
  // other e2e specs create many throwaway products that can push it off the
  // first page of the default (unfiltered) listing. Match by SKU prefix/name
  // rather than one exact variety name — which Kottu product is seeded (e.g.
  // "Chicken Kottu" vs "Cheese Kottu") can vary by seed run.
  await page.getByPlaceholder('Search by name or SKU').fill('Kottu');
  await expect(page.getByText(/kottu/i).first()).toBeVisible();
  await page.getByPlaceholder('Search by name or SKU').fill('');

  // The category filter chips should include Beverages.
  await expect(page.getByText('Beverages', { exact: false }).first()).toBeVisible();
});

test('dashboard shows KPI tiles', async ({ page, request }) => {
  await login(page, request);

  // After login we land on /dashboard; KPI tiles should be visible.
  await expect(page.getByText('Revenue today', { exact: false }).first()).toBeVisible();
  await expect(page.getByText('Open orders', { exact: false }).first()).toBeVisible();
});
