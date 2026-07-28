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

test('POS: open order → add item → send to KOT', async ({ page, request }) => {
  await login(page, request);
  await page.goto('/pos');

  // "New order" opens a table-picker modal — menu cards stay disabled (no
  // active order yet) until a table (or takeaway) is picked.
  await page.getByRole('button', { name: /new order/i }).first().click();
  await page.getByRole('button', { name: /takeaway/i }).click();

  // Tap the first in-stock menu card (out-of-stock/86'd cards render disabled
  // and can't be clicked — skip over them instead of grabbing .first()).
  await page.locator('button:has-text("LKR"):not([disabled])').first().click();
  // The order ticket should now show a total > 0.
  await expect(page.getByText(/TOTAL/)).toBeVisible();
  // Send to KOT should be enabled (button now reads "Send to Kitchen").
  await page.getByRole('button', { name: /send to kitchen/i }).first().click();
});
