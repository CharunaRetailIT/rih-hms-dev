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

test('POS shift control opens a modal (no longer a dead button) + real cashier', async ({ page, request }) => {
  await login(page, request);
  await page.goto('/pos');

  // The hardcoded placeholder cashier is gone.
  await expect(page.getByText('Asela')).toHaveCount(0);

  // The top-bar shift control is the first matching button (header renders before modals).
  const control = page.getByRole('button', { name: /start shift|end shift/i }).first();
  await expect(control).toBeVisible();

  const label = (await control.textContent())?.toLowerCase() ?? '';
  await control.click();

  // Clicking now DOES something — a modal appears (start float or cash-up).
  if (label.includes('end')) {
    // Other e2e specs share this demo shift and may leave a live bill open —
    // ending the shift then shows the open-bill gate first, not the cash-up
    // summary directly. Carry the bill over so the flow can proceed either way.
    const openBillGate = page.getByText(/can't end the shift yet/i);
    const expectedInDrawer = page.getByText(/expected in drawer/i);
    await expect(openBillGate.or(expectedInDrawer)).toBeVisible();
    if (await openBillGate.isVisible()) {
      await page.getByRole('button', { name: /keep bill open for the next shift/i }).click();
    }
    await expect(expectedInDrawer).toBeVisible();
  } else {
    await expect(page.getByText(/opening float/i)).toBeVisible();
  }

  // Dismiss.
  await page.getByRole('button', { name: /^cancel$/i }).click();
});
