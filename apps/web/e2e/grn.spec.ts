import { test, expect, Page, APIRequestContext, Locator } from '@playwright/test';

async function login(page: Page, request: APIRequestContext) {
  const r = await request.post('http://localhost:5000/api/v1/auth/magic-link', {
    data: { tenantSlug: 'demo', email: 'owner@demo.local' },
    headers: { 'Content-Type': 'application/json' },
  });
  const { devLink } = await r.json();
  await page.goto(devLink);
  await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 });
}

// Plain <label> immediately followed by its <input>/<select> sibling — no htmlFor/id.
function fieldControl(page: Page, labelText: string) {
  return page.locator(
    `label:text-is("${labelText}") + input, label:text-is("${labelText}") + select`
  ).first();
}

// SearchableSelect (Supplier / Location) renders as
// <label>...</label><div class="relative">...<input/>...</div> — the div
// sibling holds both the text input and (once open) its option buttons.
function searchableField(page: Page, labelText: string) {
  return page.locator(`label:text-is("${labelText}") + div`).first();
}

async function pickSearchable(page: Page, labelText: string, query: string) {
  const field = searchableField(page, labelText);
  await field.locator('input').click();
  await field.locator('input').fill(query);
  await field.getByRole('button', { name: new RegExp(query) }).first().click();
}

// The manual-mode line's product SearchableSelect has no <label> (bare cell
// in the Items table) — scope to its own "relative" root div in cell 0.
async function pickProductInRow(row: Locator, query: string) {
  const cell = row.locator('td').nth(0).locator('div.relative');
  await cell.locator('input').click();
  await cell.locator('input').fill(query);
  await cell.getByRole('button', { name: new RegExp(query) }).first().click();
}

function section(page: Page, title: string) {
  return page.locator('section', { has: page.getByRole('heading', { level: 3, name: title }) });
}

function firstItemRow(page: Page) {
  return section(page, 'GRN Items').locator('tbody tr').first();
}

// Submit/Approve/Reject/Void/Remove share one confirm modal — scope to its
// own title so e.g. the row's "Remove" button and the modal's "Remove"
// button (same text) don't collide.
function confirmModal(page: Page, title: string) {
  return page.locator('div.card', { has: page.getByRole('heading', { name: title }) });
}

async function grnRow(page: Page, supplierName: string) {
  const row = page.locator('tr', { has: page.getByText(supplierName, { exact: true }) });
  await expect(row).toBeVisible();
  return row;
}

async function createDraftSupplier(page: Page, code: string, name: string) {
  await page.goto('/suppliers');
  await page.getByRole('button', { name: /^new supplier$/i }).click();
  await expect(page.getByRole('heading', { name: 'New supplier' })).toBeVisible();
  await fieldControl(page, 'Code').fill(code);
  await fieldControl(page, 'Company Name').fill(name);
  await page.getByRole('button', { name: /^create$/i }).click();
  await expect(page.getByText('Supplier created.')).toBeVisible();
}

async function removeSupplier(page: Page, code: string, name: string) {
  await page.goto('/suppliers');
  await page.getByPlaceholder('Search suppliers').fill(code);
  const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
  await expect(row).toBeVisible();
  await row.getByRole('button', { name: 'Remove' }).click();
  await expect(page.getByText(`${name} removed.`)).toBeVisible();
}

// Reads the org settings straight from the server (not the form's local
// state) — the only way to know a save actually landed.
async function fetchRequireGrnApproval(page: Page): Promise<boolean> {
  return page.evaluate(async () => {
    const token = localStorage.getItem('hms.token');
    const r = await fetch('/api/v1/settings', { headers: { Authorization: `Bearer ${token}` } });
    return (await r.json()).requireGrnApproval;
  });
}

async function setRequireGrnApproval(page: Page, checked: boolean) {
  // Toggling this checkbox and saving is intermittently flaky — occasionally
  // the persisted value doesn't match what the checkbox showed right before
  // Save was clicked (root cause not pinned down; not reproducible via a
  // plain, unhurried mouse click either). Verify against the server after
  // each attempt and retry rather than trust the UI's own "Settings saved"
  // toast.
  for (let attempt = 0; attempt < 3; attempt++) {
    if ((await fetchRequireGrnApproval(page)) === checked) return;

    await page.goto('/settings');
    await page.getByRole('button', { name: 'Procurement approvals' }).click();
    const checkbox = page.getByLabel('Require approval on every goods-received note');
    if ((await checkbox.isChecked()) !== checked) {
      await checkbox.click();
      await expect(checkbox).toBeChecked({ checked });
    }
    await page.getByRole('button', { name: /save settings/i }).click();
    await expect(page.getByText('Settings saved')).toBeVisible();
  }

  expect(await fetchRequireGrnApproval(page)).toBe(checked);
}

async function fillManualLine(page: Page, prodSku: string, qty: string) {
  await page.getByLabel('Manual', { exact: true }).check();
  const row = firstItemRow(page);
  await pickProductInRow(row, prodSku);
  await row.locator('td').nth(2).locator('input').fill(qty); // Item, Unit, GRN Qty
  return row;
}

test.describe('GRN (Goods Received Notes) — CRUD + workflow', () => {
  // Serial + no retries: later tests depend on suppliers/products/GRNs
  // created earlier. A retry re-imports the file in a fresh worker,
  // regenerating `stamp` and silently pointing later tests at data that
  // retry never created.
  test.describe.configure({ mode: 'serial', retries: 0 });

  const stamp = Date.now().toString().slice(-6);
  const supCode = `E2ESUP${stamp}`;
  const supName = `E2E GRN Supplier ${stamp}`;
  const sup2Code = `E2ESUP2${stamp}`;
  const sup2Name = `E2E GRN Supplier Approve ${stamp}`;
  const sup3Code = `E2ESUP3${stamp}`;
  const sup3Name = `E2E GRN Supplier Reject ${stamp}`;
  const prodSku = `E2EGRN${stamp}`;
  const prodName = `E2E GRN Product ${stamp}`;

  test('sets up throwaway suppliers and a product for GRN testing', async ({ page, request }) => {
    await login(page, request);

    await createDraftSupplier(page, supCode, supName);
    await createDraftSupplier(page, sup2Code, sup2Name);
    await createDraftSupplier(page, sup3Code, sup3Name);

    await page.goto('/menu/create');
    await page.locator('label:text-is("Product Code")').locator('xpath=..').locator('input').fill(prodSku);
    await page.locator('label:text-is("Product Name") + input').fill(prodName);
    // Unit / Cost Price / Selling Price live on the "Pricing & Stock" tab.
    await page.getByRole('button', { name: 'Pricing & Stock' }).click();
    await page.locator('label:text-is("Unit") + select').selectOption({ index: 1 });
    await page.locator('label:text-is("Cost Price") + input').fill('100');
    await page.locator('label:text-is("Selling Price") + input').fill('250');
    await page.getByRole('button', { name: /save product/i }).first().click();
    await expect(page).toHaveURL(/\/menu$/, { timeout: 10_000 });
  });

  test('page loads with heading, filters, and table', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/grn');

    await expect(page.getByRole('heading', { level: 1, name: 'Goods Received Notes' })).toBeVisible();
    await expect(page.getByRole('heading', { level: 2, name: 'Goods received notes' })).toBeVisible();
    await expect(page.getByRole('button', { name: /^new grn$/i })).toBeVisible();
    await expect(page.getByPlaceholder(/Search GRN #/)).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'GRN #' })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'PO #' })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Net amount' })).toBeVisible();
  });

  test('create form: validation chain (supplier, PO/manual, lines, quantity, cost)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/grn/create');
    await expect(page.getByRole('heading', { level: 2, name: 'Create Goods Received Note' })).toBeVisible();

    await page.getByRole('button', { name: /^save as draft$/i }).click();
    await expect(page.getByText('Choose a supplier.')).toBeVisible();

    // Mode defaults to "PO Based" with no PO chosen yet.
    await pickSearchable(page, 'Supplier', supName);
    await page.getByRole('button', { name: /^save as draft$/i }).click();
    await expect(page.getByText('Choose a purchase order, or switch to Manual mode.')).toBeVisible();

    await page.getByLabel('Manual', { exact: true }).check();
    await page.getByRole('button', { name: /^save as draft$/i }).click();
    await expect(page.getByText('Add at least one line.')).toBeVisible();

    const row = firstItemRow(page);
    await pickProductInRow(row, prodSku);
    // GRN Qty left blank (0) — filled.length is now 1, so validation moves
    // past "Add at least one line." to the per-line quantity check.
    await page.getByRole('button', { name: /^save as draft$/i }).click();
    await expect(page.getByText('Each line needs a GRN quantity greater than zero.')).toBeVisible();

    await row.locator('td').nth(2).locator('input').fill('5');
    await row.locator('td').nth(5).locator('input').fill('-10'); // Cost Price
    await page.getByRole('button', { name: /^save as draft$/i }).click();
    await expect(page.getByText('Cost price cannot be negative.')).toBeVisible();

    // Still on the create form — nothing was submitted through the whole chain.
    await expect(page).toHaveURL(/\/grn\/create/);
  });

  test('creates a manual-mode draft GRN with a header discount and other charges', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/grn/create');

    await pickSearchable(page, 'Supplier', supName);
    const row = await fillManualLine(page, prodSku, '5'); // 5 * 100 = 500 cost value
    await expect(row.locator('td').nth(5).locator('input')).toHaveValue('100'); // cost price auto-fills from the product

    await page.locator('label:text-is("Discount amount") + input').fill('20');
    await page.locator('label:text-is("Other charges") + input').fill('10');
    // Net = 500 - 20 + 10 + 0 (no VAT-registered supplier) = 490.
    await expect(page.locator('label:text-is("Net amount") + input')).toHaveValue('490.00');

    await page.getByRole('button', { name: /^save as draft$/i }).click();
    await expect(page).toHaveURL(/\/grn$/, { timeout: 10_000 });
    await expect(page.getByText(/saved as draft$/)).toBeVisible();

    const r = await grnRow(page, supName);
    await expect(r.getByText('Draft')).toBeVisible();
    await expect(r.getByText('500.00')).toBeVisible(); // cost value column
    await expect(r.getByText('490.00')).toBeVisible(); // net amount column
  });

  test('submitting the draft GRN posts it straight to Approved (demo tenant requires no approval)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/grn');

    const r = await grnRow(page, supName);
    await r.getByRole('button', { name: /^submit$/i }).click();
    await confirmModal(page, 'Submit GRN').getByRole('button', { name: /^submit$/i }).click();
    await expect(page.getByText(/submitted$/)).toBeVisible();

    const updated = await grnRow(page, supName);
    await expect(updated.getByText('Approved')).toBeVisible();
    // Once approved, only Void is offered — no Submit or Remove.
    await expect(updated.getByRole('button', { name: /^submit$/i })).not.toBeVisible();
    await expect(updated.getByRole('button', { name: 'Remove' })).not.toBeVisible();
    await expect(updated.getByRole('button', { name: /^void$/i })).toBeVisible();
  });

  test('voiding the approved GRN reverses it and leaves no further actions', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/grn');

    const r = await grnRow(page, supName);
    await r.getByRole('button', { name: /^void$/i }).click();
    const modal = confirmModal(page, 'Void GRN');
    await modal.locator('textarea').fill('E2E test void reason');
    await modal.getByRole('button', { name: /^void grn$/i }).click();
    await expect(page.getByText(/voided$/)).toBeVisible();

    const updated = await grnRow(page, supName);
    await expect(updated.getByText('Void')).toBeVisible();
    // No actions left at all — the Actions cell (last column) is just "—".
    // (The PO # column also renders "—" for this PO-less GRN, hence scoping.)
    await expect(updated.locator('td').last()).toHaveText('—');
  });

  test('enabling GRN approval routes a new submission to Pending, then Approve posts it', async ({ page, request }) => {
    await login(page, request);
    await setRequireGrnApproval(page, true);

    await page.goto('/grn/create');
    await pickSearchable(page, 'Supplier', sup2Name);
    await fillManualLine(page, prodSku, '2');
    await page.getByRole('button', { name: /^save as draft$/i }).click();
    await expect(page).toHaveURL(/\/grn$/, { timeout: 10_000 });

    let r = await grnRow(page, sup2Name);
    await r.getByRole('button', { name: /^submit$/i }).click();
    await confirmModal(page, 'Submit GRN').getByRole('button', { name: /^submit$/i }).click();
    await expect(page.getByText(/submitted$/)).toBeVisible();

    r = await grnRow(page, sup2Name);
    await expect(r.getByText('Pending approval')).toBeVisible();
    // Stock hasn't posted yet while pending — only Approve/Reject are offered.
    await expect(r.getByRole('button', { name: /^void$/i })).not.toBeVisible();

    await r.getByRole('button', { name: /^approve$/i }).click();
    await confirmModal(page, 'Approve GRN').getByRole('button', { name: /^approve$/i }).click();
    await expect(page.getByText(/approved and posted$/)).toBeVisible();

    r = await grnRow(page, sup2Name);
    await expect(r.getByText('Approved')).toBeVisible();
  });

  test('rejecting a pending GRN with a reason marks it Rejected', async ({ page, request }) => {
    await login(page, request);
    // Approval is still enabled from the previous test.
    await page.goto('/grn/create');
    await pickSearchable(page, 'Supplier', sup3Name);
    await fillManualLine(page, prodSku, '3');
    await page.getByRole('button', { name: /^save as draft$/i }).click();
    await expect(page).toHaveURL(/\/grn$/, { timeout: 10_000 });

    let r = await grnRow(page, sup3Name);
    await r.getByRole('button', { name: /^submit$/i }).click();
    await confirmModal(page, 'Submit GRN').getByRole('button', { name: /^submit$/i }).click();
    await expect(page.getByText(/submitted$/)).toBeVisible();

    r = await grnRow(page, sup3Name);
    await expect(r.getByText('Pending approval')).toBeVisible();

    await r.getByRole('button', { name: /^reject$/i }).click();
    const modal = confirmModal(page, 'Reject GRN');
    await modal.locator('textarea').fill('E2E test rejection reason');
    await modal.getByRole('button', { name: /^reject$/i }).click();
    await expect(page.getByText(/rejected$/)).toBeVisible();

    r = await grnRow(page, sup3Name);
    await expect(r.getByText('Rejected')).toBeVisible();
    // Rejected never touched stock, so it's removable again (no Void).
    await expect(r.getByRole('button', { name: /^void$/i })).not.toBeVisible();
    await expect(r.getByRole('button', { name: 'Remove' })).toBeVisible();

    // Restore the tenant's default (no-approval) setting for other suites.
    await setRequireGrnApproval(page, false);
  });

  test('filters by search text, supplier, and status', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/grn');

    // Search matches supplier name (also GRN #, invoice #, ref #).
    await page.getByPlaceholder(/Search GRN #/).fill(supName);
    await expect(page.getByText(supName, { exact: true })).toBeVisible();
    await expect(page.getByText(sup2Name, { exact: true })).not.toBeVisible();

    await page.getByPlaceholder(/Search GRN #/).fill('zzz-no-such-grn-zzz');
    await expect(page.getByText('No GRNs match these filters.')).toBeVisible();
    await page.getByPlaceholder(/Search GRN #/).fill('');

    // Supplier filter (SearchableSelect with "All suppliers" placeholder).
    const supplierFilter = page.locator('input[placeholder="All suppliers"]');
    await supplierFilter.click();
    await supplierFilter.fill(sup2Name);
    await page.getByRole('button', { name: new RegExp(sup2Name) }).first().click();
    await expect(page.getByText(sup2Name, { exact: true })).toBeVisible();
    await expect(page.getByText(supName, { exact: true })).not.toBeVisible();

    // Status filter — sup2's GRN is Approved.
    await page.locator('select').filter({ has: page.locator('option[value=""]:text-is("All statuses")') }).selectOption('rejected');
    await expect(page.getByText('No GRNs match these filters.')).toBeVisible();
  });

  test('cleans up the remaining GRNs, throwaway product, and suppliers', async ({ page, request }) => {
    await login(page, request);

    // sup3's GRN is Rejected — removable directly. sup2's is Approved, which
    // can only be voided (not removed) — void it first so its supplier is
    // free of any live GRNs before deactivating the supplier below.
    await page.goto('/grn');
    let r = await grnRow(page, sup3Name);
    await r.getByRole('button', { name: 'Remove' }).click();
    await confirmModal(page, 'Remove GRN').getByRole('button', { name: /^remove$/i }).click();
    await expect(page.getByText(/removed$/)).toBeVisible();

    await page.goto('/grn');
    r = await grnRow(page, sup2Name);
    await r.getByRole('button', { name: /^void$/i }).click();
    const modal = confirmModal(page, 'Void GRN');
    await modal.getByRole('button', { name: /^void grn$/i }).click();
    await expect(page.getByText(/voided$/)).toBeVisible();

    await page.goto('/menu');
    await page.getByPlaceholder('Search by name or SKU').fill(prodSku);
    const prodRow = page.locator('tr', { has: page.getByText(prodSku, { exact: true }) });
    await expect(prodRow).toBeVisible();
    await prodRow.getByRole('button', { name: 'Remove' }).click();
    await expect(page.getByText(/removed\.$/)).toBeVisible();

    await removeSupplier(page, supCode, supName);
    await removeSupplier(page, sup2Code, sup2Name);
    await removeSupplier(page, sup3Code, sup3Name);
  });
});
