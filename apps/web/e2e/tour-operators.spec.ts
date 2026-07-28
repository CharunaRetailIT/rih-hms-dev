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

async function goToTab(page: Page, tab: 'Agents' | 'Companies') {
  await page.getByRole('button', { name: tab, exact: true }).click();
}

async function openNewAgentModal(page: Page) {
  await page.getByRole('button', { name: /new agent/i }).click();
  await expect(page.getByRole('heading', { name: 'New tour agent' })).toBeVisible();
}

async function openNewCompanyModal(page: Page) {
  await goToTab(page, 'Companies');
  await page.getByRole('button', { name: /new company/i }).click();
  await expect(page.getByRole('heading', { name: 'New tour agent company' })).toBeVisible();
}

// Labels are plain <label> tags immediately followed by their <input>/<select>
// sibling (no htmlFor/id), so getByLabel() won't associate them — target the
// adjacent-sibling relationship directly instead.
function fieldControl(page: Page, labelText: string) {
  return page.locator(
    `label:text-is("${labelText}") + input, label:text-is("${labelText}") + select`
  ).first();
}

test.describe('Tour Agents — CRUD', () => {
  // Serial + no retries: later tests depend on the row created earlier. A
  // retry re-imports the file in a fresh worker, regenerating `stamp` and
  // silently pointing later tests at a row that retry never created.
  test.describe.configure({ mode: 'serial', retries: 0 });

  const stamp = Date.now().toString().slice(-6);
  const code = `E2E${stamp}`;
  const name = `E2E Test Agent ${stamp}`;
  const updatedName = `E2E Test Agent ${stamp} (Updated)`;

  test('page loads with heading, tabs, and table', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tour-operators');

    await expect(page.getByRole('heading', { level: 2, name: 'Tour agents' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Agents', exact: true })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Companies', exact: true })).toBeVisible();
    await expect(page.getByRole('button', { name: /new agent/i })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Agent' })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Company' })).toBeVisible();
  });

  test('rejects an empty required field', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tour-operators');
    await openNewAgentModal(page);

    await page.getByRole('button', { name: /^create$/i }).click();
    await expect(page.getByText('Code and name are required.')).toBeVisible();
    // Modal must stay open — nothing was submitted.
    await expect(page.getByRole('heading', { name: 'New tour agent' })).toBeVisible();
  });

  test('shows meaningful address fields and a "Select a Company" dropdown', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tour-operators');
    await openNewAgentModal(page);

    // Address 01/02/03 were renamed to their actual meaning.
    await expect(page.locator('label:text-is("Street Address")')).toBeVisible();
    await expect(page.locator('label:text-is("City")')).toBeVisible();
    await expect(page.locator('label:text-is("Postal Code")')).toBeVisible();
    await expect(page.locator('label:text-is("Address 01")')).toHaveCount(0);
    await expect(page.locator('label:text-is("Address 02")')).toHaveCount(0);
    await expect(page.locator('label:text-is("Address 03")')).toHaveCount(0);

    // The company picker's placeholder option now reads "Select a Company", not "Select a Group".
    const companySelect = fieldControl(page, 'Tour Agent Company');
    await expect(companySelect.locator('option').first()).toHaveText('— Select a Company —');

    // A Country dropdown sits alongside the address fields, defaulting to Sri Lanka.
    await expect(fieldControl(page, 'Country')).toHaveValue('LK');
  });

  test('creates a new tour agent', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tour-operators');
    await openNewAgentModal(page);

    await fieldControl(page, 'Tour Agent Code').fill(code);
    await fieldControl(page, 'Name').fill(name);
    await fieldControl(page, 'Mobile').fill('+94771234567');
    await fieldControl(page, 'Street Address').fill('123 Test Lane');
    await fieldControl(page, 'City').fill('Colombo');
    await fieldControl(page, 'Postal Code').fill('00100');
    await fieldControl(page, 'Country').selectOption('AE');

    await page.getByRole('button', { name: /^create$/i }).click();

    await expect(page.getByText('Tour agent created.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New tour agent' })).not.toBeVisible();

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await expect(row.getByText(code)).toBeVisible();
    await expect(row.getByText('Company', { exact: true })).toBeVisible();
    await expect(row.getByText('Active')).toBeVisible();
  });

  test('updates the tour agent', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tour-operators');

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Edit' }).click();

    await expect(page.getByRole('heading', { name: 'Edit tour agent' })).toBeVisible();
    // Renamed address fields carry the previously saved values through to edit.
    await expect(fieldControl(page, 'Street Address')).toHaveValue('123 Test Lane');
    await expect(fieldControl(page, 'City')).toHaveValue('Colombo');
    await expect(fieldControl(page, 'Postal Code')).toHaveValue('00100');
    await expect(fieldControl(page, 'Country')).toHaveValue('AE');

    await fieldControl(page, 'Name').fill(updatedName);
    await page.getByLabel('IsActive').uncheck();

    await page.getByRole('button', { name: /save changes/i }).click();
    await expect(page.getByText('Tour agent updated.')).toBeVisible();

    const updatedRow = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(updatedRow).toBeVisible();
    await expect(updatedRow.getByText('Inactive')).toBeVisible();
  });

  test('deletes (soft-removes) the tour agent', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tour-operators');

    const row = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Remove' }).click();
    // Removing is gated behind a confirm dialog — its own "Remove" button is autofocused.
    await page.keyboard.press('Enter');

    await expect(page.getByText('Tour agent removed.')).toBeVisible();
    await expect(page.getByText(updatedName, { exact: true })).not.toBeVisible();
  });
});

test.describe('Tour Agent Companies — CRUD', () => {
  test.describe.configure({ mode: 'serial', retries: 0 });

  const stamp = Date.now().toString().slice(-6);
  const code = `E2EC${stamp}`;
  const name = `E2E Test Company ${stamp}`;
  const updatedName = `E2E Test Company ${stamp} (Updated)`;

  test('shows meaningful address fields on the company form', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tour-operators');
    await openNewCompanyModal(page);

    await expect(page.locator('label:text-is("Street Address")')).toBeVisible();
    await expect(page.locator('label:text-is("City")')).toBeVisible();
    await expect(page.locator('label:text-is("Address 01")')).toHaveCount(0);
    await expect(page.locator('label:text-is("Address 02")')).toHaveCount(0);

    // A Country dropdown sits alongside the address fields, defaulting to Sri Lanka.
    await expect(fieldControl(page, 'Country')).toHaveValue('LK');
  });

  test('creates a new tour agent company', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tour-operators');
    await openNewCompanyModal(page);

    await fieldControl(page, 'Code').fill(code);
    await fieldControl(page, 'Name').fill(name);
    await fieldControl(page, 'Street Address').fill('456 Company Road');
    await fieldControl(page, 'City').fill('Kandy');
    await fieldControl(page, 'Country').selectOption('AE');
    await fieldControl(page, 'Contact Person').fill('Jane Contact');

    await page.getByRole('button', { name: /^create$/i }).click();
    await expect(page.getByText('Tour agent company created.')).toBeVisible();

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await expect(row.getByText(code)).toBeVisible();
    await expect(row.getByText('Jane Contact')).toBeVisible();
    await expect(row.getByText('456 Company Road')).toBeVisible();
    await expect(row.getByText('Active')).toBeVisible();
  });

  test('updates the tour agent company', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tour-operators');
    await goToTab(page, 'Companies');

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Edit' }).click();

    await expect(page.getByRole('heading', { name: 'Edit tour agent company' })).toBeVisible();
    await expect(fieldControl(page, 'Street Address')).toHaveValue('456 Company Road');
    await expect(fieldControl(page, 'City')).toHaveValue('Kandy');
    await expect(fieldControl(page, 'Country')).toHaveValue('AE');

    await fieldControl(page, 'Name').fill(updatedName);
    await page.getByLabel('IsActive').uncheck();

    await page.getByRole('button', { name: /save changes/i }).click();
    await expect(page.getByText('Tour agent company updated.')).toBeVisible();

    const updatedRow = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(updatedRow).toBeVisible();
    await expect(updatedRow.getByText('Inactive')).toBeVisible();
  });

  test('the updated company appears in the agent\'s "Select a Company" dropdown', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tour-operators');
    await openNewAgentModal(page);

    const companySelect = fieldControl(page, 'Tour Agent Company');
    await expect(companySelect.locator('option', { hasText: updatedName })).toHaveCount(1);
  });

  test('deletes (soft-removes) the tour agent company', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/tour-operators');
    await goToTab(page, 'Companies');

    const row = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Remove' }).click();
    // Removing is gated behind a confirm dialog — its own "Remove" button is autofocused.
    await page.keyboard.press('Enter');

    await expect(page.getByText(`${updatedName} removed.`)).toBeVisible();
    await expect(page.getByText(updatedName, { exact: true })).not.toBeVisible();
  });
});
