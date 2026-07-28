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

async function openNewSupplierModal(page: Page) {
  await page.getByRole('button', { name: /^new supplier$/i }).click();
  await expect(page.getByRole('heading', { name: 'New supplier' })).toBeVisible();
}

// Every field in this page's modals is the shared <Input>/<select> component:
// a plain <label> immediately followed by its <input>/<select> sibling (no
// htmlFor/id), so getByLabel() won't associate them.
function fieldControl(page: Page, labelText: string) {
  return page.locator(
    `label:text-is("${labelText}") + input, label:text-is("${labelText}") + select`
  ).first();
}

test.describe('Suppliers — CRUD', () => {
  // Serial + no retries: later tests depend on the row created earlier. A
  // retry re-imports the file in a fresh worker, regenerating `stamp` and
  // silently pointing later tests at a row that retry never created.
  test.describe.configure({ mode: 'serial', retries: 0 });

  const stamp = Date.now().toString().slice(-6);
  const code = `E2E${stamp}`;
  const name = `E2E Test Supplier ${stamp}`;
  const updatedName = `E2E Test Supplier ${stamp} (Updated)`;

  test('page loads with heading, filters, and table', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');

    await expect(page.getByRole('heading', { level: 1, name: 'Master Data' })).toBeVisible();
    await expect(page.getByRole('heading', { level: 2, name: 'Suppliers' })).toBeVisible();
    await expect(page.getByRole('button', { name: /^new supplier$/i })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Supplier Groups' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Supplier Types' })).toBeVisible();
    await expect(page.getByPlaceholder('Search suppliers')).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Supplier' })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Group / Type' })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'VAT' })).toBeVisible();
  });

  test('rejects an empty required field', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');
    await openNewSupplierModal(page);

    await page.getByRole('button', { name: /^create$/i }).click();
    await expect(page.getByText('Supplier code is required.')).toBeVisible();
    // Modal must stay open — nothing was submitted.
    await expect(page.getByRole('heading', { name: 'New supplier' })).toBeVisible();
  });

  test('reveals the VAT number field only when VAT registered is checked', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');
    await openNewSupplierModal(page);

    await expect(page.locator('label:text-is("VAT registration number")')).toHaveCount(0);
    await page.getByLabel('VAT registered').check();
    await expect(page.locator('label:text-is("VAT registration number")')).toBeVisible();
    await page.getByLabel('VAT registered').uncheck();
    await expect(page.locator('label:text-is("VAT registration number")')).toHaveCount(0);
  });

  test('creates a new supplier', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');
    await openNewSupplierModal(page);

    await fieldControl(page, 'Code').fill(code);
    await fieldControl(page, 'Company Name').fill(name);
    await fieldControl(page, 'Contact').fill('Jane Contact');
    await fieldControl(page, 'Phone').fill('+94771234567');
    await fieldControl(page, 'Email').fill('jane@e2e-supplier.test');
    await fieldControl(page, 'Payment terms days').fill('30');
    await fieldControl(page, 'Address').fill('789 Supplier Rd');
    // Country is a dropdown (like the Locations form) — pick a non-default value.
    await fieldControl(page, 'Country').selectOption('AE');
    await fieldControl(page, 'District').fill('Colombo');

    await page.getByLabel('VAT registered').check();
    await fieldControl(page, 'VAT registration number').fill('VAT-E2E-001');

    await page.getByRole('button', { name: /^create$/i }).click();

    await expect(page.getByText('Supplier created.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New supplier' })).not.toBeVisible();

    await page.getByPlaceholder('Search suppliers').fill(code);
    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await expect(row.getByText(code)).toBeVisible();
    await expect(row.getByText('Jane Contact')).toBeVisible();
    await expect(row.getByText('+94771234567')).toBeVisible();
    await expect(row.getByText('VAT-E2E-001')).toBeVisible();
    await expect(row.getByText('30 days')).toBeVisible();
    await expect(row.getByText('Active')).toBeVisible();

    // Country isn't shown on the row — reopen the edit modal to confirm it persisted.
    await row.getByRole('button', { name: 'Edit' }).click();
    await expect(page.getByRole('heading', { name: 'Edit supplier' })).toBeVisible();
    await expect(fieldControl(page, 'Country')).toHaveValue('AE');
    await page.getByRole('button', { name: /^cancel$/i }).click();
    await expect(page.getByRole('heading', { name: 'Edit supplier' })).not.toBeVisible();
  });

  test('rejects a duplicate supplier code', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');
    await openNewSupplierModal(page);

    await fieldControl(page, 'Code').fill(code); // same code as the previous test
    await fieldControl(page, 'Company Name').fill(`${name} Duplicate`);

    await page.getByRole('button', { name: /^create$/i }).click();
    await expect(page.getByText('Supplier code already exists.')).toBeVisible();
  });

  test('updates the supplier', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');
    await page.getByPlaceholder('Search suppliers').fill(code);

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Edit' }).click();

    await expect(page.getByRole('heading', { name: 'Edit supplier' })).toBeVisible();
    await fieldControl(page, 'Company Name').fill(updatedName);
    await fieldControl(page, 'Contact').fill('John Updated');
    await page.getByLabel('VAT registered').uncheck();
    await page.getByLabel('Active').uncheck();

    await page.getByRole('button', { name: /save changes/i }).click();
    await expect(page.getByText('Supplier updated.')).toBeVisible();

    await page.getByPlaceholder('Search suppliers').fill(code);
    const updatedRow = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(updatedRow).toBeVisible();
    await expect(updatedRow.getByText('John Updated')).toBeVisible();
    await expect(updatedRow.getByText('No VAT')).toBeVisible();
    await expect(updatedRow.getByText('Inactive')).toBeVisible();
  });

  test('filters by search text', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');

    await page.getByPlaceholder('Search suppliers').fill(code);
    await expect(page.getByText(updatedName, { exact: true })).toBeVisible();

    await page.getByPlaceholder('Search suppliers').fill('zzz-no-such-supplier-zzz');
    await expect(page.getByText('No suppliers found.')).toBeVisible();
  });

  test('deletes (soft-removes) the supplier', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');
    await page.getByPlaceholder('Search suppliers').fill(code);

    const row = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Remove' }).click();

    await expect(page.getByText(`${updatedName} removed.`)).toBeVisible();
    await expect(page.getByText(updatedName, { exact: true })).not.toBeVisible();
    await expect(page.getByText('No suppliers found.')).toBeVisible();
  });
});

// "Supplier Groups" and "Supplier Types" are tabs on the same /suppliers page
// (not always-visible sections) — only one tab's table is in the DOM at a
// time, so switching tabs is enough to scope "Edit"/"Remove"/row locators
// without any extra wrapper.
async function goToTab(page: Page, tab: 'Suppliers' | 'Supplier Groups' | 'Supplier Types') {
  await page.getByRole('button', { name: tab, exact: true }).click();
}

test.describe('Supplier Groups — CRUD', () => {
  test.describe.configure({ mode: 'serial', retries: 0 });

  const stamp = Date.now().toString().slice(-6);
  const code = `E2EG${stamp}`;
  const name = `E2E Test Group ${stamp}`;
  const updatedName = `E2E Test Group ${stamp} (Updated)`;

  test('section renders with an Add button', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');
    await goToTab(page, 'Supplier Groups');
    await expect(page.getByRole('button', { name: /^new group$/i })).toBeVisible();
  });

  test('rejects missing code/name', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');
    await goToTab(page, 'Supplier Groups');
    await page.getByRole('button', { name: /^new group$/i }).click();
    await expect(page.getByRole('heading', { name: 'New supplier group' })).toBeVisible();

    await page.getByRole('button', { name: /^create$/i }).click();
    await expect(page.getByText('Group code and name are required.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New supplier group' })).toBeVisible();
  });

  test('creates a new supplier group', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');
    await goToTab(page, 'Supplier Groups');
    await page.getByRole('button', { name: /^new group$/i }).click();

    await fieldControl(page, 'Code').fill(code);
    await fieldControl(page, 'Name').fill(name);
    await fieldControl(page, 'Remark').fill('Created by suppliers.spec e2e run');
    await page.getByRole('button', { name: /^create$/i }).click();

    await expect(page.getByText('Supplier group created.')).toBeVisible();

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await expect(row.getByText(code)).toBeVisible();
    await expect(row.getByText('Created by suppliers.spec e2e run')).toBeVisible();
    await expect(row.getByText('Active')).toBeVisible();
  });

  test('rejects a duplicate group code', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');
    await goToTab(page, 'Supplier Groups');
    await page.getByRole('button', { name: /^new group$/i }).click();

    await fieldControl(page, 'Code').fill(code);
    await fieldControl(page, 'Name').fill(`${name} Duplicate`);
    await page.getByRole('button', { name: /^create$/i }).click();

    await expect(page.getByText('Supplier group code already exists.')).toBeVisible();
  });

  test('blocks deleting a group that a supplier is using', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');

    // Assign the group to a throwaway supplier via the main "New supplier" flow.
    await openNewSupplierModal(page);
    const supplierCode = `E2ES${stamp}`;
    await fieldControl(page, 'Code').fill(supplierCode);
    await fieldControl(page, 'Company Name').fill(`E2E Supplier For Group ${stamp}`);
    await fieldControl(page, 'Supplier group').selectOption({ label: `${code} — ${name}` });
    await page.getByRole('button', { name: /^create$/i }).click();
    await expect(page.getByText('Supplier created.')).toBeVisible();

    await goToTab(page, 'Supplier Groups');
    const groupRow = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await groupRow.getByRole('button', { name: 'Remove' }).click();
    await expect(page.getByText('Cannot delete supplier group because suppliers are using it.')).toBeVisible();
    // Still there — the delete was rejected, not silently ignored.
    await expect(groupRow).toBeVisible();

    // Clean up the throwaway supplier so the group is free to delete below.
    await goToTab(page, 'Suppliers');
    await page.getByPlaceholder('Search suppliers').fill(supplierCode);
    const supplierRow = page.locator('tr', { has: page.getByText(`E2E Supplier For Group ${stamp}`, { exact: true }) });
    await supplierRow.getByRole('button', { name: 'Remove' }).click();
    await expect(page.getByText(`E2E Supplier For Group ${stamp} removed.`)).toBeVisible();
  });

  test('updates the supplier group', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');
    await goToTab(page, 'Supplier Groups');

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await row.getByRole('button', { name: 'Edit' }).click();

    await expect(page.getByRole('heading', { name: 'Edit supplier group' })).toBeVisible();
    await fieldControl(page, 'Name').fill(updatedName);
    await page.getByLabel('Active').uncheck();
    await page.getByRole('button', { name: /save changes/i }).click();

    await expect(page.getByText('Supplier group updated.')).toBeVisible();

    const updatedRow = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(updatedRow).toBeVisible();
    await expect(updatedRow.getByText('Inactive')).toBeVisible();
  });

  test('deletes (soft-removes) the supplier group', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');
    await goToTab(page, 'Supplier Groups');

    const row = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Remove' }).click();

    await expect(page.getByText(`${updatedName} removed.`)).toBeVisible();
    await expect(page.getByText(updatedName, { exact: true })).not.toBeVisible();
  });
});

test.describe('Supplier Types — CRUD', () => {
  test.describe.configure({ mode: 'serial', retries: 0 });

  const stamp = Date.now().toString().slice(-6);
  const code = `E2ET${stamp}`;
  const name = `E2E Test Type ${stamp}`;
  const updatedName = `E2E Test Type ${stamp} (Updated)`;

  test('section renders with an Add button', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');
    await goToTab(page, 'Supplier Types');
    await expect(page.getByRole('button', { name: /^new type$/i })).toBeVisible();
  });

  test('rejects missing code/name', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');
    await goToTab(page, 'Supplier Types');
    await page.getByRole('button', { name: /^new type$/i }).click();
    await expect(page.getByRole('heading', { name: 'New supplier type' })).toBeVisible();

    await page.getByRole('button', { name: /^create$/i }).click();
    await expect(page.getByText('Type code and name are required.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New supplier type' })).toBeVisible();
  });

  test('creates a new supplier type', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');
    await goToTab(page, 'Supplier Types');
    await page.getByRole('button', { name: /^new type$/i }).click();

    await fieldControl(page, 'Code').fill(code);
    await fieldControl(page, 'Name').fill(name);
    await fieldControl(page, 'Remark').fill('Created by suppliers.spec e2e run');
    await page.getByRole('button', { name: /^create$/i }).click();

    await expect(page.getByText('Supplier type created.')).toBeVisible();

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await expect(row.getByText(code)).toBeVisible();
    await expect(row.getByText('Created by suppliers.spec e2e run')).toBeVisible();
    await expect(row.getByText('Active')).toBeVisible();
  });

  test('rejects a duplicate type code', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');
    await goToTab(page, 'Supplier Types');
    await page.getByRole('button', { name: /^new type$/i }).click();

    await fieldControl(page, 'Code').fill(code);
    await fieldControl(page, 'Name').fill(`${name} Duplicate`);
    await page.getByRole('button', { name: /^create$/i }).click();

    await expect(page.getByText('Supplier type code already exists.')).toBeVisible();
  });

  test('updates the supplier type', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');
    await goToTab(page, 'Supplier Types');

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await row.getByRole('button', { name: 'Edit' }).click();

    await expect(page.getByRole('heading', { name: 'Edit supplier type' })).toBeVisible();
    await fieldControl(page, 'Name').fill(updatedName);
    await page.getByLabel('Active').uncheck();
    await page.getByRole('button', { name: /save changes/i }).click();

    await expect(page.getByText('Supplier type updated.')).toBeVisible();

    const updatedRow = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(updatedRow).toBeVisible();
    await expect(updatedRow.getByText('Inactive')).toBeVisible();
  });

  test('deletes (soft-removes) the supplier type', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/suppliers');
    await goToTab(page, 'Supplier Types');

    const row = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Remove' }).click();

    await expect(page.getByText(`${updatedName} removed.`)).toBeVisible();
    await expect(page.getByText(updatedName, { exact: true })).not.toBeVisible();
  });
});
