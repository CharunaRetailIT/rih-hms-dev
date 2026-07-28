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

async function openNewDepartmentModal(page: Page) {
  await page.getByRole('button', { name: /new department/i }).click();
  await expect(page.getByRole('heading', { name: 'New Department' })).toBeVisible();
}

// Labels here are plain <label> tags immediately followed by their
// <input>/<select>/<textarea> sibling (no htmlFor/id) — target the
// adjacent-sibling relationship directly instead of getByLabel().
function fieldControl(page: Page, labelText: string) {
  return page.locator(
    `label:text-is("${labelText}") + input, label:text-is("${labelText}") + select, label:text-is("${labelText}") + textarea`
  ).first();
}

test.describe('Departments — CRUD', () => {
  // Serial + no retries: later tests depend on the row created earlier. A
  // retry re-imports the file in a fresh worker, regenerating `stamp` and
  // silently pointing later tests at a row that retry never created (same
  // pitfall hit in locations.spec.ts).
  test.describe.configure({ mode: 'serial', retries: 0 });

  const stamp = Date.now().toString().slice(-6);
  const code = `E2E${stamp}`;
  const name = `E2E Test Dept ${stamp}`;
  const updatedName = `E2E Test Dept ${stamp} (Updated)`;

  test('page loads with heading, filters, and table', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/departments');

    await expect(page.getByRole('heading', { level: 1, name: 'Master Data' })).toBeVisible();
    await expect(page.getByRole('heading', { level: 2, name: 'Departments' })).toBeVisible();
    await expect(page.getByRole('button', { name: /new department/i })).toBeVisible();
    await expect(page.getByPlaceholder('Search departments')).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Department' })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Location' })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Color' })).toBeVisible();
  });

  test('rejects an empty required field', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/departments');
    await openNewDepartmentModal(page);

    await page.getByRole('button', { name: /^create department$/i }).click();
    await expect(page.getByText('Department code is required.')).toBeVisible();
    // Modal must stay open — nothing was submitted.
    await expect(page.getByRole('heading', { name: 'New Department' })).toBeVisible();
  });

  test('creates a new department', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/departments');
    await openNewDepartmentModal(page);

    await fieldControl(page, 'Code').fill(code);
    await fieldControl(page, 'Name').fill(name);
    await fieldControl(page, 'Remark').fill('Created by locations.spec e2e run');
    // First real option after the "Select location" placeholder — deterministic
    // since the dropdown is alphabetically sorted by the backend.
    await fieldControl(page, 'Location').selectOption({ index: 1 });

    // "Dashboard color" has a wrapper <div> with two inputs (a native color
    // picker, then a plain text mirror) — not a single sibling input, so
    // fieldControl()'s adjacent-sibling match doesn't apply here.
    const colorHex = page.locator('label:text-is("Dashboard color") + div input').nth(1);
    await colorHex.fill('#ab12cd');

    await page.getByRole('button', { name: /^create department$/i }).click();

    await expect(page.getByText('Department created.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New Department' })).not.toBeVisible();

    await page.getByPlaceholder('Search departments').fill(code);
    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await expect(row.getByText(code)).toBeVisible();
    await expect(row.getByText('Created by locations.spec e2e run')).toBeVisible();
    await expect(row.getByText('#ab12cd', { exact: false })).toBeVisible();
    await expect(row.getByText('Active')).toBeVisible();
    // Location select's index-1 choice was actually applied — row should show
    // a location pill, not the "Tenant wide" fallback for an unset location.
    await expect(row.getByText('Tenant wide')).not.toBeVisible();
  });

  test('rejects a duplicate department code', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/departments');
    await openNewDepartmentModal(page);

    await fieldControl(page, 'Code').fill(code); // same code as the previous test
    await fieldControl(page, 'Name').fill(`${name} Duplicate`);

    await page.getByRole('button', { name: /^create department$/i }).click();
    await expect(page.getByText('Department code already exists.')).toBeVisible();
  });

  test('updates the department', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/departments');
    await page.getByPlaceholder('Search departments').fill(code);

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Edit' }).click();

    await expect(page.getByRole('heading', { name: 'Edit Department' })).toBeVisible();
    await fieldControl(page, 'Name').fill(updatedName);
    await fieldControl(page, 'Remark').fill('Updated by e2e run');
    // Clear the location back to tenant-wide.
    await fieldControl(page, 'Location').selectOption({ index: 0 });
    await page.getByLabel('Active').uncheck();

    await page.getByRole('button', { name: /save changes/i }).click();
    await expect(page.getByText('Department updated.')).toBeVisible();

    await page.getByPlaceholder('Search departments').fill(code);
    const updatedRow = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(updatedRow).toBeVisible();
    await expect(updatedRow.getByText('Updated by e2e run')).toBeVisible();
    await expect(updatedRow.getByText('Tenant wide')).toBeVisible();
    await expect(updatedRow.getByText('Inactive')).toBeVisible();
  });

  test('filters by search text', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/departments');

    await page.getByPlaceholder('Search departments').fill(code);
    await expect(page.getByText(updatedName, { exact: true })).toBeVisible();
    await expect(page.getByRole('table').getByText('No departments found.')).not.toBeVisible();

    await page.getByPlaceholder('Search departments').fill('zzz-no-such-department-zzz');
    await expect(page.getByText('No departments found.')).toBeVisible();
  });

  test('deletes (soft-removes) the department', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/departments');
    await page.getByPlaceholder('Search departments').fill(code);

    const row = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Remove' }).click();

    await expect(page.getByText(`${updatedName} removed.`)).toBeVisible();
    await expect(page.getByText(updatedName, { exact: true })).not.toBeVisible();
    await expect(page.getByText('No departments found.')).toBeVisible();
  });
});
