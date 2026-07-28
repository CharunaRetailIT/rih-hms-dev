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

async function openNewLevelModal(page: Page) {
  await page.getByRole('button', { name: /^new price level$/i }).click();
  await expect(page.getByRole('heading', { name: 'New price level' })).toBeVisible();
}

// Labels here are plain <label> tags immediately followed by their
// <input>/<select> sibling (no htmlFor/id) — target the adjacent-sibling
// relationship directly instead of getByLabel().
function fieldControl(page: Page, labelText: string) {
  return page.locator(
    `label:text-is("${labelText}") + input, label:text-is("${labelText}") + select`
  ).first();
}

async function search(page: Page, text: string) {
  await page.getByPlaceholder('Search code, name or type').fill(text);
  await page.getByRole('button', { name: /^search$/i }).click();
}

test.describe('Price Levels — CRUD', () => {
  // Serial + no retries: later tests depend on the row created earlier. A
  // retry re-imports the file in a fresh worker, regenerating `stamp` and
  // silently pointing later tests at a row that retry never created.
  test.describe.configure({ mode: 'serial', retries: 0 });

  const stamp = Date.now().toString().slice(-6);
  const code = `E2E${stamp}`;
  const name = `E2E Test Level ${stamp}`;
  const updatedName = `E2E Test Level ${stamp} (Updated)`;

  test('page loads with heading, filters, and table', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/price-levels');

    // The page's own <h2> repeats the Topbar's <h1> text ("Price Levels"),
    // so heading level disambiguates rather than name alone.
    await expect(page.getByRole('heading', { level: 1, name: 'Price Levels' })).toBeVisible();
    await expect(page.getByRole('heading', { level: 2, name: 'Price Levels' })).toBeVisible();
    await expect(page.getByRole('button', { name: /^new price level$/i })).toBeVisible();
    await expect(page.getByPlaceholder('Search code, name or type')).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Order Type' })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Default' })).toBeVisible();
  });

  test('rejects empty required fields inline (no toast, modal stays open)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/price-levels');
    await openNewLevelModal(page);

    await page.getByRole('button', { name: /save price level/i }).click();

    await expect(page.getByText('Code is required.')).toBeVisible();
    await expect(page.getByText('Name is required.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New price level' })).toBeVisible();
  });

  test('creates a new price level (global — no location)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/price-levels');
    await openNewLevelModal(page);

    await fieldControl(page, 'Code').fill(code);
    await fieldControl(page, 'Sort order').fill('5');
    await fieldControl(page, 'Name').fill(name);
    await fieldControl(page, 'Order type').selectOption({ label: 'Takeaway' });
    // Location left as "Select location" and "Default price level" left unchecked.

    await page.getByRole('button', { name: /save price level/i }).click();

    await expect(page.getByText('Price level created.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New price level' })).not.toBeVisible();

    await search(page, code);
    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await expect(row.getByText(code)).toBeVisible();
    await expect(row.getByText('Takeaway')).toBeVisible();
    await expect(row.getByText('Global')).toBeVisible();
    await expect(row.getByText('5', { exact: true })).toBeVisible();
    await expect(row.getByText('No', { exact: true })).toBeVisible();
    await expect(row.getByText('Active')).toBeVisible();
  });

  test('rejects a duplicate code for the same (global) location', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/price-levels');
    await openNewLevelModal(page);

    await fieldControl(page, 'Code').fill(code); // same code, still global → collides
    await fieldControl(page, 'Name').fill(`${name} Duplicate`);
    await page.getByRole('button', { name: /save price level/i }).click();

    await expect(page.getByText('Price level code already exists for this location.')).toBeVisible();
  });

  test('marking it default disables Remove (server enforces single default)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/price-levels');
    await search(page, code);

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await row.getByRole('button', { name: 'Edit' }).click();
    await expect(page.getByRole('heading', { name: 'Edit price level' })).toBeVisible();

    await page.getByLabel('Default price level').check();
    await page.getByRole('button', { name: /save price level/i }).click();
    await expect(page.getByText('Price level updated.')).toBeVisible();

    await search(page, code);
    const defaultRow = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(defaultRow.getByText('Default', { exact: true })).toBeVisible();
    await expect(defaultRow.getByRole('button', { name: 'Remove' })).toBeDisabled();
  });

  test('unmarking default re-enables Remove, and other fields still update', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/price-levels');
    await search(page, code);

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await row.getByRole('button', { name: 'Edit' }).click();
    await expect(page.getByRole('heading', { name: 'Edit price level' })).toBeVisible();

    await fieldControl(page, 'Name').fill(updatedName);
    await page.getByLabel('Default price level').uncheck();
    await page.getByLabel('Active').uncheck();
    await page.getByRole('button', { name: /save price level/i }).click();
    await expect(page.getByText('Price level updated.')).toBeVisible();

    await search(page, code);
    const updatedRow = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(updatedRow).toBeVisible();
    await expect(updatedRow.getByText('No', { exact: true })).toBeVisible();
    await expect(updatedRow.getByText('Inactive')).toBeVisible();
    await expect(updatedRow.getByRole('button', { name: 'Remove' })).toBeEnabled();
  });

  test('filters by search text', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/price-levels');

    await search(page, code);
    await expect(page.getByText(updatedName, { exact: true })).toBeVisible();

    await search(page, 'zzz-no-such-price-level-zzz');
    await expect(page.getByText('No price levels found.')).toBeVisible();
  });

  test('deletes (soft-removes) the price level', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/price-levels');
    await search(page, code);

    const row = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Remove' }).click();

    await expect(page.getByText(`${updatedName} removed.`)).toBeVisible();
    await expect(page.getByText(updatedName, { exact: true })).not.toBeVisible();
    await expect(page.getByText('No price levels found.')).toBeVisible();
  });
});
