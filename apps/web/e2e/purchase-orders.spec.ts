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

// SearchableSelect (Supplier / PO location / Delivery location) renders as
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

// The product line's SearchableSelect has no <label> (bare cell in the Lines
// table) — scope to its own "relative" root div within the row's first cell.
async function pickProductInRow(row: Locator, query: string) {
  const cell = row.locator('td').nth(0).locator('div.relative');
  await cell.locator('input').click();
  await cell.locator('input').fill(query);
  await cell.getByRole('button', { name: new RegExp(query) }).first().click();
}

function section(page: Page, title: string) {
  return page.locator('section', { has: page.getByRole('heading', { level: 3, name: title }) });
}

function firstLineRow(page: Page) {
  return section(page, 'Lines').locator('tbody tr').first();
}

// Submit/Approve/Reject/Remove share one confirm modal — scope to its own
// title so e.g. the row's "Remove" button and the modal's "Remove" button
// (same text) don't collide.
function confirmModal(page: Page, title: string) {
  return page.locator('div.card', { has: page.getByRole('heading', { name: title }) });
}

async function poRow(page: Page, supplierName: string) {
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
async function fetchRequirePoApproval(page: Page): Promise<boolean> {
  return page.evaluate(async () => {
    const token = localStorage.getItem('hms.token');
    const r = await fetch('/api/v1/settings', { headers: { Authorization: `Bearer ${token}` } });
    return (await r.json()).requirePoApproval;
  });
}

async function setRequirePoApproval(page: Page, checked: boolean) {
  // Toggling this checkbox and saving is intermittently flaky — occasionally
  // the persisted value doesn't match what the checkbox showed right before
  // Save was clicked (root cause not pinned down; not reproducible via a
  // plain, unhurried mouse click either). Verify against the server after
  // each attempt and retry rather than trust the UI's own "Settings saved"
  // toast.
  for (let attempt = 0; attempt < 3; attempt++) {
    if ((await fetchRequirePoApproval(page)) === checked) return;

    await page.goto('/settings');
    await page.getByRole('button', { name: 'Procurement approvals' }).click();
    const checkbox = page.getByLabel('Require approval on every purchase order');
    if ((await checkbox.isChecked()) !== checked) {
      await checkbox.click();
      await expect(checkbox).toBeChecked({ checked });
    }
    await page.getByRole('button', { name: /save settings/i }).click();
    await expect(page.getByText('Settings saved')).toBeVisible();
  }

  expect(await fetchRequirePoApproval(page)).toBe(checked);
}

test.describe('Purchase Orders — CRUD + workflow', () => {
  // Serial + no retries: later tests depend on suppliers/products/POs created
  // earlier. A retry re-imports the file in a fresh worker, regenerating
  // `stamp` and silently pointing later tests at data that retry never created.
  test.describe.configure({ mode: 'serial', retries: 0 });

  const stamp = Date.now().toString().slice(-6);
  const supCode = `E2ESUP${stamp}`;
  const supName = `E2E PO Supplier ${stamp}`;
  const sup2Code = `E2ESUP2${stamp}`;
  const sup2Name = `E2E PO Supplier Approve ${stamp}`;
  const sup3Code = `E2ESUP3${stamp}`;
  const sup3Name = `E2E PO Supplier Reject ${stamp}`;
  const prodSku = `E2EPO${stamp}`;
  const prodName = `E2E PO Product ${stamp}`;

  test('sets up throwaway suppliers and a product for PO testing', async ({ page, request }) => {
    await login(page, request);

    // Three non-VAT suppliers (no tax to worry about) — one for the main
    // create→edit→submit→receive lifecycle, two for the separate
    // approve/reject branches, each locatable later by its unique name.
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

  test('page loads with heading and table', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/purchasing');

    await expect(page.getByRole('heading', { level: 1, name: 'Purchasing' })).toBeVisible();
    await expect(page.getByRole('heading', { level: 2, name: 'Purchase orders' })).toBeVisible();
    await expect(page.getByRole('button', { name: /^new po$/i })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'PO number' })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Supplier' })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Status' })).toBeVisible();
  });

  test('create form: validation chain (supplier, lines, quantity, unit cost)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/purchasing/create');
    await expect(page.getByRole('heading', { level: 2, name: 'Create Purchase Order' })).toBeVisible();

    await page.getByRole('button', { name: /^save as draft$/i }).click();
    await expect(page.getByText('Choose a supplier.')).toBeVisible();

    await pickSearchable(page, 'Supplier', supName);
    await page.getByRole('button', { name: /^save as draft$/i }).click();
    await expect(page.getByText('Add at least one line.')).toBeVisible();

    const row = firstLineRow(page);
    await pickProductInRow(row, prodSku);
    // Quantity left blank (0) — filled.length is now 1, so validation moves
    // past "Add at least one line." to the per-line quantity check.
    await page.getByRole('button', { name: /^save as draft$/i }).click();
    await expect(page.getByText('Each line needs a quantity greater than zero.')).toBeVisible();

    await row.locator('td').nth(1).locator('input').fill('5');
    await row.locator('td').nth(2).locator('input').fill('-10');
    await page.getByRole('button', { name: /^save as draft$/i }).click();
    await expect(page.getByText('Unit cost cannot be negative.')).toBeVisible();

    // Still on the create form — nothing was submitted through the whole chain.
    await expect(page).toHaveURL(/\/purchasing\/create/);
  });

  test('create form: rejects a past expected delivery date', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/purchasing/create');

    await pickSearchable(page, 'Supplier', supName);
    const yesterday = new Date(Date.now() - 86_400_000).toISOString().slice(0, 10);
    await page.locator('label:text-is("Expected delivery date") + input').fill(yesterday);
    await page.getByRole('button', { name: /^save as draft$/i }).click();

    await expect(page.getByText("Expected delivery date can't be in the past.")).toBeVisible();
  });

  test('creates a draft PO (Save as Draft)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/purchasing/create');

    await pickSearchable(page, 'Supplier', supName);
    const row = firstLineRow(page);
    await pickProductInRow(row, prodSku);
    await row.locator('td').nth(1).locator('input').fill('5');
    // Unit cost auto-fills from the product's cost price (100) on selection —
    // leave it as-is: line total should be 5 * 100 = 500.

    await page.getByRole('button', { name: /^save as draft$/i }).click();
    await expect(page).toHaveURL(/\/purchasing$/, { timeout: 10_000 });

    const r = await poRow(page, supName);
    await expect(r.getByText('Draft')).toBeVisible();
    await expect(r.getByText('500.00')).toBeVisible();
  });

  test('edits the draft PO (quantity + header discount)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/purchasing');

    const r = await poRow(page, supName);
    await r.getByRole('button', { name: 'Edit' }).click();
    await expect(page.getByRole('heading', { level: 2, name: /Edit /i })).toBeVisible();

    const row = firstLineRow(page);
    await row.locator('td').nth(1).locator('input').fill('8'); // 8 * 100 = 800
    await page.locator('label:text-is("Header discount") + input').fill('50');

    await expect(page.getByText('750.00').last()).toBeVisible(); // 800 - 50 = 750, shown live in the totals box

    await page.getByRole('button', { name: /^save changes$/i }).first().click();
    await expect(page).toHaveURL(/\/purchasing$/, { timeout: 10_000 });

    const updated = await poRow(page, supName);
    await expect(updated.getByText('750.00')).toBeVisible();
  });

  test('submitting a draft PO lands on Approved directly (demo tenant requires no approval)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/purchasing');

    const r = await poRow(page, supName);
    await r.getByRole('button', { name: /^submit$/i }).click();
    await confirmModal(page, 'Submit purchase order').getByRole('button', { name: /^submit$/i }).click();

    await expect(page.getByText('Purchase order submitted')).toBeVisible();
    const updated = await poRow(page, supName);
    await expect(updated.getByText('Approved')).toBeVisible();
    // Edit/Submit disappear once it's no longer a draft.
    await expect(updated.getByRole('button', { name: 'Edit' })).not.toBeVisible();
  });

  test('Receive navigates to the GRN create page, pre-filled from the PO', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/purchasing');

    const r = await poRow(page, supName);
    const poNumber = (await r.locator('td').first().innerText()).trim();
    await r.getByRole('button', { name: /^receive$/i }).click();

    await expect(page).toHaveURL(/\/grn\/create\?poId=/, { timeout: 10_000 });
    await expect(page.getByRole('heading', { level: 2, name: 'Create Goods Received Note' })).toBeVisible();

    // Deep-linked straight into PO Based mode with supplier/PO#/location/lines
    // all resolved from the PO — no manual re-entry needed.
    await expect(page.locator('input[placeholder="Search suppliers…"]')).toHaveValue(supName);
    await expect(page.locator('input[placeholder="Select a PO…"]')).toHaveValue(poNumber);
    await expect(page.locator('input[placeholder="Search locations…"]')).toHaveValue(/MAIN/);
    const row = page.locator('table').filter({ has: page.getByRole('columnheader', { name: 'PO Qty' }) }).locator('tbody tr').first();
    await expect(row).toContainText(prodName);
    await expect(row).toContainText('8'); // PO qty, from the earlier edit
    await expect(row.locator('td').nth(3).locator('input')).toHaveValue('8'); // GRN qty pre-fills to the full remaining amount

    await page.getByRole('button', { name: /^submit$/i }).click();

    await expect(page).toHaveURL(/\/grn$/, { timeout: 10_000 });
    await expect(page.getByText(/^GRN .+ posted$/)).toBeVisible();

    await page.goto('/purchasing');
    const updated = await poRow(page, supName);
    await expect(updated.getByText('Received')).toBeVisible();
    // Fully received — Receive and Remove both drop off the row's actions.
    await expect(updated.getByRole('button', { name: /^receive$/i })).not.toBeVisible();
    await expect(updated.getByRole('button', { name: 'Remove' })).not.toBeVisible();
  });

  test('enabling PO approval routes a new submission to Pending, then Approve clears it', async ({ page, request }) => {
    await login(page, request);
    await setRequirePoApproval(page, true);

    await page.goto('/purchasing/create');
    await pickSearchable(page, 'Supplier', sup2Name);
    const row = firstLineRow(page);
    await pickProductInRow(row, prodSku);
    await row.locator('td').nth(1).locator('input').fill('2');
    await page.getByRole('button', { name: /^save as draft$/i }).click();
    await expect(page).toHaveURL(/\/purchasing$/, { timeout: 10_000 });

    let r = await poRow(page, sup2Name);
    await r.getByRole('button', { name: /^submit$/i }).click();
    await confirmModal(page, 'Submit purchase order').getByRole('button', { name: /^submit$/i }).click();
    await expect(page.getByText('Purchase order submitted')).toBeVisible();

    r = await poRow(page, sup2Name);
    await expect(r.getByText('Pending approval')).toBeVisible();

    await r.getByRole('button', { name: /^approve$/i }).click();
    await confirmModal(page, 'Approve purchase order').getByRole('button', { name: /^approve$/i }).click();
    await expect(page.getByText('Purchase order approved')).toBeVisible();

    r = await poRow(page, sup2Name);
    await expect(r.getByText('Approved')).toBeVisible();
  });

  test('rejecting a pending PO with a reason marks it Rejected', async ({ page, request }) => {
    await login(page, request);
    // Approval is still enabled from the previous test.
    await page.goto('/purchasing/create');
    await pickSearchable(page, 'Supplier', sup3Name);
    const row = firstLineRow(page);
    await pickProductInRow(row, prodSku);
    await row.locator('td').nth(1).locator('input').fill('3');
    await page.getByRole('button', { name: /^save as draft$/i }).click();
    await expect(page).toHaveURL(/\/purchasing$/, { timeout: 10_000 });

    let r = await poRow(page, sup3Name);
    await r.getByRole('button', { name: /^submit$/i }).click();
    await confirmModal(page, 'Submit purchase order').getByRole('button', { name: /^submit$/i }).click();
    await expect(page.getByText('Purchase order submitted')).toBeVisible();

    r = await poRow(page, sup3Name);
    await expect(r.getByText('Pending approval')).toBeVisible();

    await r.getByRole('button', { name: /^reject$/i }).click();
    const modal = confirmModal(page, 'Reject purchase order');
    await modal.locator('textarea').fill('E2E test rejection reason');
    await modal.getByRole('button', { name: /^reject order$/i }).click();
    await expect(page.getByText('Purchase order rejected')).toBeVisible();

    r = await poRow(page, sup3Name);
    await expect(r.getByText('Rejected')).toBeVisible();

    // Restore the tenant's default (no-approval) setting for other suites.
    await setRequirePoApproval(page, false);
  });

  test('remove is available for draft/pending/approved/rejected POs but not a fully-received one', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/purchasing');

    // The Rejected PO (sup3) can be removed.
    let r = await poRow(page, sup3Name);
    await r.getByRole('button', { name: 'Remove' }).click();
    await confirmModal(page, 'Remove purchase order').getByRole('button', { name: /^remove$/i }).click();
    await expect(page.getByText('Purchase order removed')).toBeVisible();
    await expect(page.locator('tr', { has: page.getByText(sup3Name, { exact: true }) })).not.toBeVisible();

    // The Approved-but-not-received PO (sup2) can also be removed.
    r = await poRow(page, sup2Name);
    await r.getByRole('button', { name: 'Remove' }).click();
    await confirmModal(page, 'Remove purchase order').getByRole('button', { name: /^remove$/i }).click();
    await expect(page.getByText('Purchase order removed')).toBeVisible();
    await expect(page.locator('tr', { has: page.getByText(sup2Name, { exact: true }) })).not.toBeVisible();

    // The main PO (supName) is now fully Received — no Remove action at all.
    r = await poRow(page, supName);
    await expect(r.getByRole('button', { name: 'Remove' })).not.toBeVisible();
  });

  test('voiding the GRN rolls the PO back to draft, then it can be removed', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/grn');

    const row = page.locator('tr', { has: page.getByText(supName, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: /^void$/i }).click();
    await page.locator('div.card', { has: page.getByRole('heading', { name: 'Void GRN' }) })
      .getByRole('button', { name: /^void grn$/i }).click();
    await expect(page.getByText(/^GRN .+ voided$/)).toBeVisible();

    await page.goto('/purchasing');
    let r = await poRow(page, supName);
    await expect(r.getByText('Draft')).toBeVisible();

    await r.getByRole('button', { name: 'Remove' }).click();
    await confirmModal(page, 'Remove purchase order').getByRole('button', { name: /^remove$/i }).click();
    await expect(page.getByText('Purchase order removed')).toBeVisible();
    await expect(page.locator('tr', { has: page.getByText(supName, { exact: true }) })).not.toBeVisible();
  });

  test('cleans up the throwaway product and suppliers', async ({ page, request }) => {
    await login(page, request);

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
