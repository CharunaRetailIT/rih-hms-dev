import { test, expect, Page, APIRequestContext } from '@playwright/test';

async function loginAs(page: Page, request: APIRequestContext, email: string) {
  const r = await request.post('http://localhost:5000/api/v1/auth/magic-link', {
    data: { tenantSlug: 'demo', email },
    headers: { 'Content-Type': 'application/json' },
  });
  const { devLink } = await r.json();
  await page.goto(devLink);
  await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 });
}

test('owner sees Team and can list staff', async ({ page, request }) => {
  await loginAs(page, request, 'owner@demo.local');
  const nav = page.locator('aside');   // the sidebar
  // "Team" and "Reports" live in separate collapsed-by-default accordion sections.
  await nav.getByRole('button', { name: 'Settings' }).click();
  await expect(nav.getByRole('link', { name: 'Team' })).toBeVisible();
  await nav.getByRole('button', { name: 'Reports' }).click();
  await expect(nav.getByRole('link', { name: 'Reports' })).toBeVisible();
  await page.goto('/team');
  await expect(page.getByRole('heading', { level: 1, name: 'Team' })).toBeVisible();
  await expect(page.getByText('Maya Manager')).toBeVisible();
  await expect(page.getByText('cashier@demo.local')).toBeVisible();
});

test('cashier nav is role-gated and team is denied', async ({ page, request }) => {
  await loginAs(page, request, 'cashier@demo.local');
  const nav = page.locator('aside');   // the sidebar
  // gated-away items are absent from the sidebar
  await expect(nav.getByRole('link', { name: 'POS' })).toBeVisible();   // sidebar has hydrated
  await expect(nav.getByRole('link', { name: 'Team' })).toHaveCount(0);
  await expect(nav.getByRole('link', { name: 'Reports' })).toHaveCount(0);
  await expect(nav.getByRole('link', { name: 'Suppliers' })).toHaveCount(0);
  // direct navigation to a forbidden page is denied by the API
  await page.goto('/team');
  await expect(page.getByText(/only an owner/i)).toBeVisible();
});
