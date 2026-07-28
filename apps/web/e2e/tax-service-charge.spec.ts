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

async function openNewChargeModal(page: Page) {
  await page.getByRole('button', { name: /^new charge$/i }).click();
  await expect(page.getByRole('heading', { name: 'New Charge' })).toBeVisible();
}

test.describe('Charges (Tax & Service Charge) — CRUD', () => {
  // Serial + no retries: later tests depend on state built up earlier. A
  // retry re-imports the file in a fresh worker, regenerating `stamp` and
  // silently pointing later tests at data that retry never created.
  test.describe.configure({ mode: 'serial', retries: 0 });

  const stamp = Date.now().toString().slice(-6);
  const typeCode = `E2ECT${stamp}`;
  const typeName = `E2E Charge Type For Charges ${stamp}`;
  const code = `E2E${stamp}`;
  const desc = `E2E Test Charge ${stamp}`;
  const updatedDesc = `E2E Test Charge ${stamp} (Updated)`;
  const amtCode = `E2EA${stamp}`;
  const amtDesc = `E2E Amount Charge ${stamp}`;

  test('sets up a charge type to attach charges to', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tax-types');
    await page.getByRole('button', { name: /^new charge type$/i }).click();
    await fieldControl(page, 'Code').fill(typeCode);
    await fieldControl(page, 'Name').fill(typeName);
    await page.getByLabel(/Assign to specific products/).check();
    await page.getByRole('button', { name: /create charge type/i }).click();
    await expect(page.getByText('Charge type created.')).toBeVisible();
  });

  test('page loads with heading, filters, and table', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tax-service-charge');

    await expect(page.getByRole('heading', { level: 1, name: 'Charges' })).toBeVisible();
    await expect(page.getByRole('heading', { level: 2, name: 'Charges' })).toBeVisible();
    await expect(page.getByRole('button', { name: /^new charge$/i })).toBeVisible();
    await expect(page.getByPlaceholder('Search by code or description')).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Rate / Amount' })).toBeVisible();
  });

  test('rejects empty required fields', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tax-service-charge');
    await openNewChargeModal(page);

    // "Charge type" defaults to the first available option when the modal
    // opens, so "Code is required." is the first reachable message.
    await page.getByRole('button', { name: /create charge/i }).click();
    await expect(page.getByText('Code is required.')).toBeVisible();

    await fieldControl(page, 'Code').fill(code);
    await page.getByRole('button', { name: /create charge/i }).click();
    await expect(page.getByText('Description is required.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New Charge' })).toBeVisible();
  });

  test('rejects a charge with neither a percentage nor an amount', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tax-service-charge');
    await openNewChargeModal(page);

    await fieldControl(page, 'Charge type').selectOption({ label: typeName });
    await fieldControl(page, 'Code').fill(code);
    await fieldControl(page, 'Description').fill(desc);
    await fieldControl(page, 'Rate %').fill('0'); // mode defaults to percentage, value defaults to "0"
    await page.getByRole('button', { name: /create charge/i }).click();

    await expect(page.getByText('Set either a percentage or a flat amount, not both.')).toBeVisible();
  });

  test('creates a percentage-mode charge', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tax-service-charge');
    await openNewChargeModal(page);

    await fieldControl(page, 'Charge type').selectOption({ label: typeName });
    await fieldControl(page, 'Code').fill(code);
    await fieldControl(page, 'Description').fill(desc);
    await fieldControl(page, 'Rate %').fill('18');
    await page.getByRole('button', { name: /create charge/i }).click();

    await expect(page.getByText('Charge created.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New Charge' })).not.toBeVisible();

    await page.getByPlaceholder('Search by code or description').fill(code);
    const row = page.locator('tr', { has: page.getByText(desc, { exact: true }) });
    await expect(row).toBeVisible();
    await expect(row.getByText(code)).toBeVisible();
    await expect(row.getByText(typeName)).toBeVisible();
    await expect(row.getByText('18%')).toBeVisible();
    await expect(row.getByText('Active')).toBeVisible();
  });

  test('rejects a duplicate charge code', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tax-service-charge');
    await openNewChargeModal(page);

    await fieldControl(page, 'Charge type').selectOption({ label: typeName });
    await fieldControl(page, 'Code').fill(code); // same code as the previous test
    await fieldControl(page, 'Description').fill(`${desc} Duplicate`);
    await fieldControl(page, 'Rate %').fill('5');
    await page.getByRole('button', { name: /create charge/i }).click();

    await expect(page.getByText('Charge code already exists.')).toBeVisible();
  });

  test('creates a flat-amount-mode charge', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tax-service-charge');
    await openNewChargeModal(page);

    await fieldControl(page, 'Charge type').selectOption({ label: typeName });
    await fieldControl(page, 'Code').fill(amtCode);
    await fieldControl(page, 'Description').fill(amtDesc);
    await fieldControl(page, 'Mode').selectOption({ label: 'Flat amount' });
    await fieldControl(page, 'Amount (LKR)').fill('250');
    await page.getByRole('button', { name: /create charge/i }).click();

    await expect(page.getByText('Charge created.')).toBeVisible();

    await page.getByPlaceholder('Search by code or description').fill(amtCode);
    const row = page.locator('tr', { has: page.getByText(amtDesc, { exact: true }) });
    await expect(row).toBeVisible();
    await expect(row.getByText('LKR 250')).toBeVisible();
  });

  test('code field is locked once editing an existing charge', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tax-service-charge');
    await page.getByPlaceholder('Search by code or description').fill(code);

    const row = page.locator('tr', { has: page.getByText(desc, { exact: true }) });
    await row.getByRole('button', { name: 'Edit' }).click();
    await expect(page.getByRole('heading', { name: 'Edit Charge' })).toBeVisible();

    await expect(fieldControl(page, 'Code')).toBeDisabled();
    await expect(fieldControl(page, 'Code')).toHaveValue(code);
  });

  test('updates a charge (description, switch mode, deactivate)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tax-service-charge');
    await page.getByPlaceholder('Search by code or description').fill(code);

    const row = page.locator('tr', { has: page.getByText(desc, { exact: true }) });
    await row.getByRole('button', { name: 'Edit' }).click();
    await expect(page.getByRole('heading', { name: 'Edit Charge' })).toBeVisible();

    await fieldControl(page, 'Description').fill(updatedDesc);
    await fieldControl(page, 'Mode').selectOption({ label: 'Flat amount' });
    await fieldControl(page, 'Amount (LKR)').fill('42');
    await page.getByLabel('Active', { exact: true }).uncheck();

    await page.getByRole('button', { name: /save changes/i }).click();
    await expect(page.getByText('Charge updated.')).toBeVisible();

    await page.getByPlaceholder('Search by code or description').fill(code);
    const updatedRow = page.locator('tr', { has: page.getByText(updatedDesc, { exact: true }) });
    await expect(updatedRow).toBeVisible();
    await expect(updatedRow.getByText('LKR 42')).toBeVisible();
    await expect(updatedRow.getByText('Inactive')).toBeVisible();
  });

  test('filters by search text and by charge type', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tax-service-charge');

    await page.getByPlaceholder('Search by code or description').fill(amtCode);
    await expect(page.getByText(amtDesc, { exact: true })).toBeVisible();

    await page.getByPlaceholder('Search by code or description').fill('');
    await page.locator('select').filter({ has: page.locator(`option[value=""]:text-is("All types")`) }).selectOption({ label: typeName });
    await expect(page.getByText(amtDesc, { exact: true })).toBeVisible();
    await expect(page.getByText(updatedDesc, { exact: true })).toBeVisible();

    await page.getByPlaceholder('Search by code or description').fill('zzz-no-such-charge-zzz');
    await expect(page.getByText('No charges configured', { exact: false })).toBeVisible();
  });

  test('blocks deleting a charge that a product still references', async ({ page, request }) => {
    await login(page, request);

    // Re-activate so it behaves like a normal, in-use charge.
    await page.goto('/tax-service-charge');
    await page.getByPlaceholder('Search by code or description').fill(code);
    let row = page.locator('tr', { has: page.getByText(updatedDesc, { exact: true }) });
    await row.getByRole('button', { name: 'Edit' }).click();
    await page.getByLabel('Active', { exact: true }).check();
    await page.getByRole('button', { name: /save changes/i }).click();
    await expect(page.getByText('Charge updated.')).toBeVisible();

    // Create a throwaway product and attach this charge to it via the
    // Products form's own "Charges" sub-collection.
    const prodSku = `E2ECHG${stamp}`;
    await page.goto('/menu/create');
    await page.locator('label:text-is("Product Code")').locator('xpath=..').locator('input').fill(prodSku);
    await page.locator('label:text-is("Product Name") + input').fill(`E2E Product For Charge ${stamp}`);
    // Unit / Cost Price / Selling Price live on the "Pricing & Stock" tab.
    await page.getByRole('button', { name: 'Pricing & Stock' }).click();
    await page.locator('label:text-is("Unit") + select').selectOption({ index: 1 });
    await page.locator('label:text-is("Cost Price") + input').fill('10');
    await page.locator('label:text-is("Selling Price") + input').fill('20');
    // The "Charges" sub-collection lives on its own tab.
    await page.getByRole('button', { name: 'Charges', exact: true }).click();
    const chargesSection = page.locator('section', { has: page.getByRole('heading', { level: 3, name: 'Charges' }) });
    await chargesSection.getByRole('button', { name: /add charge/i }).click();
    await chargesSection.locator('select').last().selectOption({ label: `${typeName}: ${updatedDesc} (LKR 42)` });
    await page.getByRole('button', { name: /save product/i }).first().click();
    await expect(page).toHaveURL(/\/menu$/, { timeout: 10_000 });

    // Now try to delete the charge — should be rejected.
    await page.goto('/tax-service-charge');
    await page.getByPlaceholder('Search by code or description').fill(code);
    row = page.locator('tr', { has: page.getByText(updatedDesc, { exact: true }) });
    await row.getByRole('button', { name: 'Remove' }).click();
    await expect(page.getByText('Cannot delete this charge because products are linked to it.')).toBeVisible();
    await expect(row).toBeVisible();

    // Clean up the throwaway product so the charge is free to delete below.
    // Unassign the charge via the edit form first — deleting the product
    // outright only soft-deletes the product row, not its ProductCharges
    // join row (confirmed via ProductsService.DeleteAsync, which sets
    // IsDeleted/IsActive on the product entity only), which would leave an
    // orphaned, never-deleted ProductCharges row and permanently block this
    // charge's delete guard even after the referencing product is gone.
    await page.goto('/menu');
    await page.getByPlaceholder('Search by name or SKU').fill(prodSku);
    const prodRow = page.locator('tr', { has: page.getByText(prodSku, { exact: true }) });
    await expect(prodRow).toBeVisible();
    await prodRow.getByRole('button', { name: 'Edit' }).click();
    await expect(page.getByRole('heading', { level: 2, name: 'Edit Product' })).toBeVisible();

    // The edit form always opens on the "General" tab regardless of where the
    // product's data actually lives — the "Charges" sub-collection is on its
    // own tab and isn't mounted until switched to. Once it holds a charge its
    // tab button's accessible name gains a trailing count badge ("Charges 1"),
    // so match as a prefix rather than exact.
    await page.getByRole('button', { name: /^Charges( \d+)?$/ }).click();

    // Scope to the charge row itself (not the section's own "Add Charge"
    // header button, which also renders an <svg> icon and sits earlier in
    // the DOM — a bare `section button:has(svg)` selector matches that one
    // first and adds a stray extra charge row instead of removing this one).
    const editChargesSection = page.locator('section', { has: page.getByRole('heading', { level: 3, name: 'Charges' }) });
    await editChargesSection.locator('div.grid-cols-12').last().locator('button:has(svg)').click();
    await page.getByRole('button', { name: /save changes/i }).first().click();
    await expect(page).toHaveURL(/\/menu$/, { timeout: 10_000 });

    await page.getByPlaceholder('Search by name or SKU').fill(prodSku);
    await expect(prodRow).toBeVisible();
    await prodRow.getByRole('button', { name: 'Remove' }).click();
    await expect(page.getByText(/removed\.$/)).toBeVisible();
  });

  test('deletes both charges, then cleans up the charge type', async ({ page, request }) => {
    await login(page, request);

    for (const [c, d] of [[code, updatedDesc], [amtCode, amtDesc]]) {
      await page.goto('/tax-service-charge');
      await page.getByPlaceholder('Search by code or description').fill(c);
      const row = page.locator('tr', { has: page.getByText(d, { exact: true }) });
      await expect(row).toBeVisible();
      await row.getByRole('button', { name: 'Remove' }).click();
      await expect(page.getByText(`${d} removed.`)).toBeVisible();
    }

    await page.goto('/tax-types');
    await page.getByPlaceholder('Search by code or name').fill(typeCode);
    const typeRow = page.locator('tr', { has: page.getByText(typeName, { exact: true }) });
    await expect(typeRow).toBeVisible();
    await typeRow.getByRole('button', { name: 'Remove' }).click();
    await expect(page.getByText(`${typeName} removed.`)).toBeVisible();
  });
});
