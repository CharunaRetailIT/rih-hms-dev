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

// Labels here are plain <label> tags immediately followed by their
// <input>/<select> sibling (no htmlFor/id) — target the adjacent-sibling
// relationship directly instead of getByLabel().
function fieldControl(page: Page, labelText: string) {
  return page.locator(
    `label:text-is("${labelText}") + input, label:text-is("${labelText}") + select`
  ).first();
}

async function search(page: Page, text: string) {
  await page.getByPlaceholder(/^search /i).fill(text);
  await page.getByRole('button', { name: /^search$/i }).click();
}

async function openPrinterTypesModal(page: Page) {
  await page.getByRole('button', { name: /^printer types$/i }).click();
  await expect(page.getByRole('heading', { name: 'Printer Types' })).toBeVisible();
}

// The modal overlay sits on top of the main page, but the underlying
// Kitchen Stations table stays mounted behind it — both tables share column
// names ("Code" etc.), so scope every modal query to its wrapper. "max-w-2xl"
// is this modal's own wrapper class and isn't reused elsewhere on this page.
function printerTypesModal(page: Page) {
  return page.locator('div.max-w-2xl');
}

test.describe('Printer Types — CRUD', () => {
  // Serial + no retries: later tests depend on the row created earlier. A
  // retry re-imports the file in a fresh worker, regenerating `stamp` and
  // silently pointing later tests at a row that retry never created.
  test.describe.configure({ mode: 'serial', retries: 0 });

  const stamp = Date.now().toString().slice(-6);
  const code = `E2EPT${stamp}`;
  const name = `E2E Test Printer ${stamp}`;
  const updatedName = `E2E Test Printer ${stamp} (Updated)`;

  test('modal opens with the printer types table', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/kitchen-stations');
    await openPrinterTypesModal(page);
    await expect(printerTypesModal(page).getByRole('columnheader', { name: 'Code' })).toBeVisible();
    await expect(page.getByRole('button', { name: /^save$/i })).toBeVisible();
  });

  test('rejects empty required fields inline', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/kitchen-stations');
    await openPrinterTypesModal(page);

    await page.getByRole('button', { name: /^save$/i }).click();
    await expect(page.getByText('Code is required.')).toBeVisible();
    await expect(page.getByText('Name is required.')).toBeVisible();
    // Modal must stay open — nothing was submitted.
    await expect(page.getByRole('heading', { name: 'Printer Types' })).toBeVisible();
  });

  test('creates a new printer type', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/kitchen-stations');
    await openPrinterTypesModal(page);

    await fieldControl(page, 'Code').fill(code);
    await fieldControl(page, 'Name').fill(name);
    await fieldControl(page, 'Sort').fill('3');
    await page.getByRole('button', { name: /^save$/i }).click();

    // Unlike Suppliers' Groups/Types, this modal closes on a successful
    // save (create or edit) — reopen it to see the row in the refreshed list.
    await expect(page.getByText('Printer type created.')).toBeVisible();
    await openPrinterTypesModal(page);

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await expect(row.getByText(code)).toBeVisible();
    await expect(row.getByText('Active')).toBeVisible();
  });

  test('rejects a duplicate printer type code', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/kitchen-stations');
    await openPrinterTypesModal(page);

    await fieldControl(page, 'Code').fill(code);
    await fieldControl(page, 'Name').fill(`${name} Duplicate`);
    await page.getByRole('button', { name: /^save$/i }).click();

    await expect(page.getByText('Printer type code already exists.')).toBeVisible();
  });

  test('updates the printer type (staying active)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/kitchen-stations');
    await openPrinterTypesModal(page);

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await row.getByRole('button', { name: 'Edit' }).click();

    await fieldControl(page, 'Name').fill(updatedName);
    await fieldControl(page, 'Sort').fill('9');
    // Deliberately left Active checked — see the next test for what happens
    // when a printer type is deactivated (this list has no "show inactive"
    // toggle, unlike the paged Kitchen Stations/Serving Units/etc. tables).
    await page.getByRole('button', { name: /^save$/i }).click();
    await expect(page.getByText('Printer type updated.')).toBeVisible();
    await openPrinterTypesModal(page);

    const updatedRow = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(updatedRow).toBeVisible();
    await expect(updatedRow.getByText('Active')).toBeVisible();
  });

  test('deactivating a printer type hides it from this list entirely', async ({ page, request }) => {
    // GET /api/v1/printer-types (used to populate this modal) omits `all=true`,
    // so PrinterTypesService.ListPrinterTypesAsync defaults to active-only —
    // and this modal has no status filter to reveal inactive rows again. Using
    // a disposable throwaway record here so it doesn't affect the delete test.
    await login(page, request);
    await page.goto('/kitchen-stations');
    await openPrinterTypesModal(page);

    const throwawayCode = `${code}D`;
    const throwawayName = `${name} Throwaway`;
    await fieldControl(page, 'Code').fill(throwawayCode);
    await fieldControl(page, 'Name').fill(throwawayName);
    await page.getByRole('button', { name: /^save$/i }).click();
    await expect(page.getByText('Printer type created.')).toBeVisible();
    await openPrinterTypesModal(page);

    const row = page.locator('tr', { has: page.getByText(throwawayName, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Edit' }).click();
    await page.getByLabel('Active').uncheck();
    await page.getByRole('button', { name: /^save$/i }).click();
    await expect(page.getByText('Printer type updated.')).toBeVisible();
    await openPrinterTypesModal(page);

    await expect(page.getByText(throwawayName, { exact: true })).not.toBeVisible();
  });

  test('deletes (soft-removes) the printer type', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/kitchen-stations');
    await openPrinterTypesModal(page);

    const row = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Remove' }).click();

    await expect(page.getByText(`${updatedName} removed.`)).toBeVisible();
    await expect(page.getByText(updatedName, { exact: true })).not.toBeVisible();
  });
});

test.describe('Kitchen Stations — CRUD', () => {
  test.describe.configure({ mode: 'serial', retries: 0 });

  const stamp = Date.now().toString().slice(-6);
  const ptCode = `E2EHPT${stamp}`;
  const ptName = `E2E Helper Printer ${stamp}`;
  const code = `E2EKS${stamp}`;
  const name = `E2E Test Station ${stamp}`;
  const updatedName = `E2E Test Station ${stamp} (Updated)`;

  test('page loads with heading, filters, and table', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/kitchen-stations');

    // The page's own <h2> repeats the Topbar's <h1> text ("Kitchen Stations"),
    // so heading level disambiguates rather than name alone.
    await expect(page.getByRole('heading', { level: 1, name: 'Kitchen Stations' })).toBeVisible();
    await expect(page.getByRole('heading', { level: 2, name: 'Kitchen Stations' })).toBeVisible();
    await expect(page.getByRole('button', { name: /^new station$/i })).toBeVisible();
    await expect(page.getByRole('button', { name: /^printer types$/i })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Printer Type' })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Printer Name' })).toBeVisible();
  });

  test('sets up a helper printer type for the station to reference', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/kitchen-stations');
    await openPrinterTypesModal(page);

    await fieldControl(page, 'Code').fill(ptCode);
    await fieldControl(page, 'Name').fill(ptName);
    await page.getByRole('button', { name: /^save$/i }).click();
    await expect(page.getByText('Printer type created.')).toBeVisible();
  });

  test('rejects empty required fields inline', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/kitchen-stations');
    await page.getByRole('button', { name: /^new station$/i }).click();
    await expect(page.getByRole('heading', { name: 'New kitchen station' })).toBeVisible();

    // The form defaults "Printer type" to the first available option (a
    // printer type already exists from the previous test), so clear it back
    // to the blank placeholder to actually exercise this validation rule.
    await fieldControl(page, 'Printer type').selectOption('');

    await page.getByRole('button', { name: /save station/i }).click();
    await expect(page.getByText('Code is required.')).toBeVisible();
    await expect(page.getByText('Name is required.')).toBeVisible();
    await expect(page.getByText('Printer type is required.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New kitchen station' })).toBeVisible();
  });

  test('creates a new kitchen station (global — no location)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/kitchen-stations');
    await page.getByRole('button', { name: /^new station$/i }).click();

    await fieldControl(page, 'Code').fill(code);
    await fieldControl(page, 'Sort order').fill('4');
    await fieldControl(page, 'Name').fill(name);
    await fieldControl(page, 'Printer type').selectOption({ label: `${ptCode} — ${ptName}` });
    await fieldControl(page, 'Printer name').fill('Epson TM-88');

    await page.getByRole('button', { name: /save station/i }).click();

    await expect(page.getByText('Kitchen station created.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New kitchen station' })).not.toBeVisible();

    await search(page, code);
    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await expect(row.getByText(code)).toBeVisible();
    await expect(row.getByText(ptCode)).toBeVisible();
    await expect(row.getByText('Epson TM-88')).toBeVisible();
    await expect(row.getByText('Global')).toBeVisible();
    await expect(row.getByText('4', { exact: true })).toBeVisible();
    await expect(row.getByText('Active')).toBeVisible();
  });

  test('rejects a duplicate code for the same (global) location', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/kitchen-stations');
    await page.getByRole('button', { name: /^new station$/i }).click();

    await fieldControl(page, 'Code').fill(code); // same code, still global → collides
    await fieldControl(page, 'Name').fill(`${name} Duplicate`);
    await fieldControl(page, 'Printer type').selectOption({ label: `${ptCode} — ${ptName}` });
    await page.getByRole('button', { name: /save station/i }).click();

    await expect(page.getByText('Kitchen station code already exists for this location.')).toBeVisible();
  });

  test('updates the kitchen station', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/kitchen-stations');
    await search(page, code);

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Edit' }).click();

    await expect(page.getByRole('heading', { name: 'Edit kitchen station' })).toBeVisible();
    await fieldControl(page, 'Name').fill(updatedName);
    await fieldControl(page, 'Printer name').fill('Star TSP-143');
    await page.getByLabel('Active').uncheck();

    await page.getByRole('button', { name: /save station/i }).click();
    await expect(page.getByText('Kitchen station updated.')).toBeVisible();

    await search(page, code);
    const updatedRow = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(updatedRow).toBeVisible();
    await expect(updatedRow.getByText('Star TSP-143')).toBeVisible();
    await expect(updatedRow.getByText('Inactive')).toBeVisible();
  });

  test('filters by search text', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/kitchen-stations');

    await search(page, code);
    await expect(page.getByText(updatedName, { exact: true })).toBeVisible();

    await search(page, 'zzz-no-such-kitchen-station-zzz');
    await expect(page.getByText('No kitchen stations found.')).toBeVisible();
  });

  test('blocks deleting the printer type while the station still uses it', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/kitchen-stations');
    await openPrinterTypesModal(page);

    const row = page.locator('tr', { has: page.getByText(ptName, { exact: true }) });
    await row.getByRole('button', { name: 'Remove' }).click();

    await expect(page.getByText('Cannot delete this printer type because kitchen stations are using it.')).toBeVisible();
    // Still there — the delete was rejected, not silently ignored.
    await expect(row).toBeVisible();
  });

  test('deletes (soft-removes) the kitchen station', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/kitchen-stations');
    await search(page, code);

    const row = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Remove' }).click();

    await expect(page.getByText(`${updatedName} removed.`)).toBeVisible();
    await expect(page.getByText(updatedName, { exact: true })).not.toBeVisible();
    await expect(page.getByText('No kitchen stations found.')).toBeVisible();
  });

  test('now the helper printer type can be deleted (no station uses it anymore)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/kitchen-stations');
    await openPrinterTypesModal(page);

    const row = page.locator('tr', { has: page.getByText(ptName, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Remove' }).click();

    await expect(page.getByText(`${ptName} removed.`)).toBeVisible();
    await expect(page.getByText(ptName, { exact: true })).not.toBeVisible();
  });
});
