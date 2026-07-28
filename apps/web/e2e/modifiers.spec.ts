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

test('create an add-on group and see it listed', async ({ page, request }) => {
  await login(page, request);
  await page.goto('/modifiers');
  await expect(page.getByRole('heading', { level: 1, name: 'Add-ons' })).toBeVisible();

  await page.getByRole('button', { name: /new group/i }).click();
  const unique = 'E2E Add-ons ' + Date.now().toString().slice(-5);
  await page.getByPlaceholder('e.g. Add-ons, Size, Spice level').fill(unique);
  await page.getByPlaceholder('Option name').first().fill('Extra cheese');
  await page.getByPlaceholder('+ price').first().fill('150');
  await page.getByRole('button', { name: /^create group$/i }).click();

  await expect(page.getByText(unique)).toBeVisible();
  await expect(page.getByText(/Extra cheese/).first()).toBeVisible();
});

test('owner sidebar has the Add-ons link', async ({ page, request }) => {
  await login(page, request);
  await expect(page.locator('aside').getByRole('link', { name: 'Add-ons' })).toBeVisible();
});
