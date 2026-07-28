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

test('settings page shows numbering + branding config', async ({ page, request }) => {
  await login(page, request);
  await page.goto('/settings');
  await expect(page.getByText(/document numbering/i)).toBeVisible();
  await expect(page.getByRole('heading', { name: /business identity/i })).toBeVisible();
  // "Bill & invoice print" is a separate accordion section (only one section's
  // content is rendered at a time) — switch to it first.
  await page.getByRole('button', { name: /bill & invoice print/i }).click();
  await expect(page.getByText(/print VAT registration/i)).toBeVisible();
  await expect(page.getByRole('button', { name: /save settings/i })).toBeVisible();
});
