import { test, expect } from '@playwright/test';

/**
 * Login (magic-link dev flow) → dashboard. This catches the class of bug the
 * user found manually (the /auth/callback 404): a route that compiles but
 * breaks at runtime in the browser.
 */
test('magic-link login reaches the dashboard', async ({ page, request }) => {
  // Request a magic link for the seeded demo tenant; dev returns the link.
  const res = await request.post('http://localhost:5000/api/v1/auth/magic-link', {
    data: { tenantSlug: 'demo', email: 'owner@demo.local' },
    headers: { 'Content-Type': 'application/json' },
  });
  expect(res.ok()).toBeTruthy();
  const { devLink } = await res.json();
  expect(devLink).toContain('/auth/callback?token=');

  // Follow the callback → it should exchange the token and land on /dashboard.
  await page.goto(devLink);
  await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 });
});

test('login page renders the brand panel + form', async ({ page }) => {
  await page.goto('/login');
  await expect(page.getByText('One platform for every outlet')).toBeVisible();
  await expect(page.getByRole('button', { name: /send magic link/i })).toBeVisible();
});

test('login validates a bad workspace slug', async ({ page }) => {
  await page.goto('/login');
  await page.getByLabel('Workspace').fill('BAD SLUG!');
  await page.getByRole('button', { name: /send magic link/i }).click();
  await expect(page.getByText(/lowercase letters/i)).toBeVisible();
});
