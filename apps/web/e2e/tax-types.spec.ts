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

async function openNewTypeModal(page: Page) {
  await page.getByRole('button', { name: /^new charge type$/i }).click();
  await expect(page.getByRole('heading', { name: 'New Charge Type' })).toBeVisible();
}

// Labels here are plain <label> tags immediately followed by their
// <input>/<select> sibling (no htmlFor/id) — target the adjacent-sibling
// relationship directly instead of getByLabel().
function fieldControl(page: Page, labelText: string) {
  return page.locator(
    `label:text-is("${labelText}") + input, label:text-is("${labelText}") + select`
  ).first();
}

test.describe('Charge Types — CRUD', () => {
  // Serial + no retries: later tests depend on the row created earlier. A
  // retry re-imports the file in a fresh worker, regenerating `stamp` and
  // silently pointing later tests at a row that retry never created.
  test.describe.configure({ mode: 'serial', retries: 0 });

  const stamp = Date.now().toString().slice(-6);
  const code = `E2E${stamp}`;
  const name = `E2E Test Charge Type ${stamp}`;
  const updatedName = `E2E Test Charge Type ${stamp} (Updated)`;

  test('page loads with heading, filters, and table', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tax-types');

    await expect(page.getByRole('heading', { level: 1, name: 'Charge Types' })).toBeVisible();
    await expect(page.getByRole('heading', { level: 2, name: 'Charge Types' })).toBeVisible();
    await expect(page.getByRole('button', { name: /^new charge type$/i })).toBeVisible();
    await expect(page.getByPlaceholder('Search by code or name')).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Assignment' })).toBeVisible();
  });

  test('rejects empty required fields', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tax-types');
    await openNewTypeModal(page);

    await page.getByRole('button', { name: /create charge type/i }).click();
    await expect(page.getByText('Code is required.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New Charge Type' })).toBeVisible();
  });

  test('creates a new charge type (assign to products)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tax-types');
    await openNewTypeModal(page);

    await fieldControl(page, 'Code').fill(code);
    await fieldControl(page, 'Name').fill(name);
    await fieldControl(page, 'Sort order').fill('7');
    await page.getByLabel(/Assign to specific products/).check();

    await page.getByRole('button', { name: /create charge type/i }).click();

    await expect(page.getByText('Charge type created.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New Charge Type' })).not.toBeVisible();

    await page.getByPlaceholder('Search by code or name').fill(code);
    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await expect(row.getByText(code)).toBeVisible();
    await expect(row.getByText('Assign to products')).toBeVisible();
    await expect(row.getByText('7', { exact: true })).toBeVisible();
    await expect(row.getByText('Active')).toBeVisible();
  });

  test('rejects a duplicate charge type code', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tax-types');
    await openNewTypeModal(page);

    await fieldControl(page, 'Code').fill(code); // same code as the previous test
    await fieldControl(page, 'Name').fill(`${name} Duplicate`);
    await page.getByRole('button', { name: /create charge type/i }).click();

    await expect(page.getByText('Charge type code already exists.')).toBeVisible();
  });

  test('code field is locked once editing an existing charge type', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tax-types');
    await page.getByPlaceholder('Search by code or name').fill(code);

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await row.getByRole('button', { name: 'Edit' }).click();
    await expect(page.getByRole('heading', { name: 'Edit Charge Type' })).toBeVisible();

    await expect(fieldControl(page, 'Code')).toBeDisabled();
    await expect(fieldControl(page, 'Code')).toHaveValue(code);
  });

  test('updates a charge type (name, unassign from products, deactivate)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tax-types');
    await page.getByPlaceholder('Search by code or name').fill(code);

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await row.getByRole('button', { name: 'Edit' }).click();
    await expect(page.getByRole('heading', { name: 'Edit Charge Type' })).toBeVisible();

    await fieldControl(page, 'Name').fill(updatedName);
    await page.getByLabel(/Assign to specific products/).uncheck();
    await page.getByLabel('Active', { exact: true }).uncheck();

    await page.getByRole('button', { name: /save changes/i }).click();
    await expect(page.getByText('Charge type updated.')).toBeVisible();

    await page.getByPlaceholder('Search by code or name').fill(code);
    const updatedRow = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(updatedRow).toBeVisible();
    await expect(updatedRow.getByText('Tenant-wide')).toBeVisible();
    await expect(updatedRow.getByText('Inactive')).toBeVisible();
  });

  test('filters by search text', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tax-types');

    await page.getByPlaceholder('Search by code or name').fill(code);
    await expect(page.getByText(updatedName, { exact: true })).toBeVisible();

    await page.getByPlaceholder('Search by code or name').fill('zzz-no-such-charge-type-zzz');
    await expect(page.getByText('No charge types found.')).toBeVisible();
  });

  test('blocks deleting a charge type that a charge still references', async ({ page, request }) => {
    await login(page, request);

    // Re-activate + re-assign so it's usable as a real charge type for a new charge.
    await page.goto('/tax-types');
    await page.getByPlaceholder('Search by code or name').fill(code);
    let row = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await row.getByRole('button', { name: 'Edit' }).click();
    await page.getByLabel('Active', { exact: true }).check();
    await page.getByRole('button', { name: /save changes/i }).click();
    await expect(page.getByText('Charge type updated.')).toBeVisible();

    // Create a throwaway charge under this charge type.
    await page.goto('/tax-service-charge');
    await page.getByRole('button', { name: /^new charge$/i }).click();
    await expect(page.getByRole('heading', { name: 'New Charge' })).toBeVisible();
    await fieldControl(page, 'Charge type').selectOption({ label: updatedName });
    await fieldControl(page, 'Code').fill(`${code}C`);
    await fieldControl(page, 'Description').fill(`E2E Charge For Type ${stamp}`);
    await fieldControl(page, 'Rate %').fill('5');
    await page.getByRole('button', { name: /create charge/i }).click();
    await expect(page.getByText('Charge created.')).toBeVisible();

    // Now try to delete the charge type — should be rejected.
    await page.goto('/tax-types');
    await page.getByPlaceholder('Search by code or name').fill(code);
    row = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await row.getByRole('button', { name: 'Remove' }).click();
    await expect(page.getByText('Cannot delete this charge type because charges are linked to it.')).toBeVisible();
    await expect(row).toBeVisible();

    // Clean up the throwaway charge so the charge type is free to delete below.
    await page.goto('/tax-service-charge');
    await page.getByPlaceholder('Search by code or description').fill(`${code}C`);
    const chargeRow = page.locator('tr', { has: page.getByText(`E2E Charge For Type ${stamp}`, { exact: true }) });
    await chargeRow.getByRole('button', { name: 'Remove' }).click();
    await expect(page.getByText(`E2E Charge For Type ${stamp} removed.`)).toBeVisible();
  });

  test('deletes (soft-removes) the charge type', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tax-types');
    await page.getByPlaceholder('Search by code or name').fill(code);

    const row = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Remove' }).click();

    await expect(page.getByText(`${updatedName} removed.`)).toBeVisible();
    await expect(page.getByText(updatedName, { exact: true })).not.toBeVisible();
    await expect(page.getByText('No charge types found.')).toBeVisible();
  });
});
