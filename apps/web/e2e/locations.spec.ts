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

async function openNewLocationModal(page: Page) {
  await page.getByRole('button', { name: /new location/i }).click();
  await expect(page.getByRole('heading', { name: 'New location' })).toBeVisible();
}

// Labels in the location modal are plain <label> tags immediately followed by
// their <input>/<select> sibling (no htmlFor/id), so getByLabel() won't
// associate them — target the adjacent-sibling relationship directly instead.
function fieldInput(page: Page, labelText: string) {
  return page.locator(`label:text-is("${labelText}") + input, label:text-is("${labelText}") + select`).first();
}

test.describe('Locations — CRUD', () => {
  // Serial + no retries: later tests depend on state the earlier ones create
  // (the row from "creates a new location" is edited, then deleted). A retry
  // re-imports the file in a fresh worker, which would regenerate `stamp` and
  // silently point later tests at a row that was never created in that retry.
  test.describe.configure({ mode: 'serial', retries: 0 });

  const stamp = Date.now().toString().slice(-6);
  const code = `E2E${stamp}`;
  const name = `E2E Test Outlet ${stamp}`;
  const updatedName = `E2E Test Outlet ${stamp} (Updated)`;

  test('page loads with heading, filters, and table', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/locations');

    await expect(page.getByRole('heading', { level: 1, name: 'Master Data' })).toBeVisible();
    await expect(page.getByRole('heading', { level: 2, name: 'Locations' })).toBeVisible();
    await expect(page.getByRole('button', { name: /new location/i })).toBeVisible();
    await expect(page.getByPlaceholder('Search locations')).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Location' })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Capabilities' })).toBeVisible();
    // Seeded demo location should already be present (scope to the table —
    // the sidebar's active-outlet indicator also renders "Main Outlet").
    await expect(page.getByRole('table').getByText('Main Outlet')).toBeVisible();
  });

  test('rejects an empty required field', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/locations');
    await openNewLocationModal(page);

    // Leave Code blank, fill nothing else, try to save.
    await page.getByRole('button', { name: /create location/i }).click();
    await expect(page.getByText('Code is required.')).toBeVisible();
    // Modal must stay open — nothing was submitted.
    await expect(page.getByRole('heading', { name: 'New location' })).toBeVisible();
  });

  test('creates a new location', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/locations');
    await openNewLocationModal(page);

    await fieldInput(page, 'Code').fill(code);
    await fieldInput(page, 'Name').fill(name);
    await fieldInput(page, 'Address line 1').fill('123 Test Lane');
    await fieldInput(page, 'City').fill('Colombo');
    await fieldInput(page, 'Location type').selectOption('warehouse');

    // Country, Currency and Time zone are dropdowns — pick non-default values
    // so the row assertions below can confirm the selection round-trips.
    await fieldInput(page, 'Country').selectOption('AE');
    await fieldInput(page, 'Currency').selectOption('USD');
    await fieldInput(page, 'Time zone').selectOption('Asia/Dubai');

    // Defaults are canSell=true, canProduce=false, canStock=true — flip canProduce
    // on and canSell off so the row's capability pills are distinctive.
    await page.getByLabel('Can sell').uncheck();
    await page.getByLabel('Can produce').check();

    await page.getByRole('button', { name: /create location/i }).click();

    await expect(page.getByText('Location created.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New location' })).not.toBeVisible();

    // Search narrows the list to just the new row, avoiding pagination flakiness.
    await page.getByPlaceholder('Search locations').fill(code);
    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await expect(row.getByText(code)).toBeVisible();
    await expect(row.getByText('warehouse', { exact: false })).toBeVisible();
    await expect(row.getByText('Produce')).toBeVisible();
    await expect(row.getByText('Sell')).not.toBeVisible();
    await expect(row.getByText('Active')).toBeVisible();
    await expect(row.getByText('USD · AE')).toBeVisible();

    // Re-open the row's edit modal to confirm the dropdowns persisted the selection.
    await row.getByRole('button', { name: 'Edit' }).click();
    await expect(page.getByRole('heading', { name: 'Edit location' })).toBeVisible();
    await expect(fieldInput(page, 'Country')).toHaveValue('AE');
    await expect(fieldInput(page, 'Currency')).toHaveValue('USD');
    await expect(fieldInput(page, 'Time zone')).toHaveValue('Asia/Dubai');
    await page.getByRole('button', { name: 'Cancel' }).click();
    await expect(page.getByRole('heading', { name: 'Edit location' })).not.toBeVisible();
  });

  test('rejects a duplicate location code', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/locations');
    await openNewLocationModal(page);

    await fieldInput(page, 'Code').fill(code); // same code as the previous test
    await fieldInput(page, 'Name').fill(`${name} Duplicate`);
    await fieldInput(page, 'Address line 1').fill('456 Other Road');
    await fieldInput(page, 'City').fill('Kandy');

    await page.getByRole('button', { name: /create location/i }).click();
    await expect(page.getByText('Location code already exists.')).toBeVisible();
  });

  test('updates the location', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/locations');
    await page.getByPlaceholder('Search locations').fill(code);

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Edit' }).click();

    await expect(page.getByRole('heading', { name: 'Edit location' })).toBeVisible();
    await fieldInput(page, 'Name').fill(updatedName);
    await fieldInput(page, 'City').fill('Galle');
    await page.getByLabel('Active').uncheck();

    await page.getByRole('button', { name: /save changes/i }).click();
    await expect(page.getByText('Location updated.')).toBeVisible();

    // Re-search since the row's label text just changed.
    await page.getByPlaceholder('Search locations').fill(code);
    const updatedRow = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(updatedRow).toBeVisible();
    await expect(updatedRow.getByText('Galle')).toBeVisible();
    await expect(updatedRow.getByText('Inactive')).toBeVisible();
  });

  test('filters by search text', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/locations');

    await page.getByPlaceholder('Search locations').fill(code);
    await expect(page.getByText(updatedName, { exact: true })).toBeVisible();
    // Nothing else should be showing in the table while the filter is this specific.
    await expect(page.getByRole('table').getByText('Main Outlet')).not.toBeVisible();
  });

  test('deletes (soft-removes) the location', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/locations');
    await page.getByPlaceholder('Search locations').fill(code);

    const row = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Remove' }).click();

    await expect(page.getByText(`${updatedName} removed.`)).toBeVisible();
    await expect(page.getByText(updatedName, { exact: true })).not.toBeVisible();
    await expect(page.getByText('No locations found.')).toBeVisible();
  });
});
