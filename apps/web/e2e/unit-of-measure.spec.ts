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

async function openNewUnitModal(page: Page) {
  await page.getByRole('button', { name: /^new unit$/i }).click();
  await expect(page.getByRole('heading', { name: 'New unit' })).toBeVisible();
}

// Labels here are plain <label> tags immediately followed by their
// <input>/<select> sibling (no htmlFor/id) — target the adjacent-sibling
// relationship directly instead of getByLabel().
function fieldControl(page: Page, labelText: string) {
  return page.locator(
    `label:text-is("${labelText}") + input, label:text-is("${labelText}") + select`
  ).first();
}

test.describe('Unit of Measure — CRUD', () => {
  // Serial + no retries: later tests depend on the row created earlier. A
  // retry re-imports the file in a fresh worker, regenerating `stamp` and
  // silently pointing later tests at a row that retry never created.
  test.describe.configure({ mode: 'serial', retries: 0 });

  const stamp = Date.now().toString().slice(-6);
  const code = `E2E${stamp}`;
  const name = `E2E Test Unit ${stamp}`;
  const updatedName = `E2E Test Unit ${stamp} (Updated)`;
  const code2 = `E2E2${stamp}`;
  const name2 = `E2E Test Unit 2 ${stamp}`;

  test('page loads with heading, dimension pills, and table', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/unit-of-measure');

    await expect(page.getByRole('heading', { level: 1, name: 'Master Data' })).toBeVisible();
    await expect(page.getByRole('heading', { level: 2, name: 'Unit of Measure' })).toBeVisible();
    await expect(page.getByRole('button', { name: /^new unit$/i })).toBeVisible();
    await expect(page.getByPlaceholder('Search code or name')).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Dimension' })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Factor to Base' })).toBeVisible();
    // Seeded units should already show up.
    await expect(page.getByRole('table').getByText('Kilogram')).toBeVisible();
  });

  test('rejects empty required fields (toast, modal stays open)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/unit-of-measure');
    await openNewUnitModal(page);

    await page.getByRole('button', { name: /create unit/i }).click();
    await expect(page.getByText('Code is required.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New unit' })).toBeVisible();
  });

  test('creates a new unit (count dimension, non-base)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/unit-of-measure');
    await openNewUnitModal(page);

    await fieldControl(page, 'Code').fill(code);
    await fieldControl(page, 'Symbol').fill('e2e');
    await fieldControl(page, 'Name').fill(name);
    await fieldControl(page, 'Dimension').selectOption({ label: 'Count' });
    await fieldControl(page, 'Factor to Base').fill('5');

    await page.getByRole('button', { name: /create unit/i }).click();

    await expect(page.getByText('Unit created.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New unit' })).not.toBeVisible();

    await page.getByPlaceholder('Search code or name').fill(code);
    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await expect(row.getByText(code)).toBeVisible();
    await expect(row.getByText('e2e', { exact: true })).toBeVisible();
    await expect(row.getByText('count')).toBeVisible();
    await expect(row.getByText('5', { exact: true })).toBeVisible();
    await expect(row.getByText('Conversion unit')).toBeVisible();
  });

  test('rejects a duplicate unit code', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/unit-of-measure');
    await openNewUnitModal(page);

    await fieldControl(page, 'Code').fill(code); // same code as the previous test
    await fieldControl(page, 'Name').fill(`${name} Duplicate`);
    await page.getByRole('button', { name: /create unit/i }).click();

    await expect(page.getByText('UOM code already exists.')).toBeVisible();
  });

  test('rejects an invalid (zero) factor to base', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/unit-of-measure');
    await openNewUnitModal(page);

    await fieldControl(page, 'Code').fill(`${code}Z`);
    await fieldControl(page, 'Name').fill(`${name} Zero Factor`);
    await fieldControl(page, 'Factor to Base').fill('0');
    await page.getByRole('button', { name: /create unit/i }).click();

    await expect(page.getByText('Factor to base must be greater than zero.')).toBeVisible();
  });

  test('marking a unit as base disables the factor field and auto-sets it to 1', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/unit-of-measure');
    await page.getByPlaceholder('Search code or name').fill(code);

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await row.getByRole('button', { name: 'Edit' }).click();
    await expect(page.getByRole('heading', { name: 'Edit unit' })).toBeVisible();

    await page.getByLabel('This is the base unit for this dimension').check();
    await expect(fieldControl(page, 'Factor to Base')).toHaveValue('1');
    await expect(fieldControl(page, 'Factor to Base')).toBeDisabled();

    await page.getByRole('button', { name: /save changes/i }).click();
    await expect(page.getByText('Unit updated.')).toBeVisible();

    await page.getByPlaceholder('Search code or name').fill(code);
    const baseRow = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(baseRow.getByText('Base unit')).toBeVisible();
    await expect(baseRow.getByText('1', { exact: true })).toBeVisible();
  });

  test('a second base unit in the same dimension un-bases the first one', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/unit-of-measure');
    await openNewUnitModal(page);

    await fieldControl(page, 'Code').fill(code2);
    await fieldControl(page, 'Name').fill(name2);
    await fieldControl(page, 'Dimension').selectOption({ label: 'Count' }); // same dimension as `code`
    await page.getByLabel('This is the base unit for this dimension').check();
    await page.getByRole('button', { name: /create unit/i }).click();
    await expect(page.getByText('Unit created.')).toBeVisible();

    await page.getByPlaceholder('Search code or name').fill(code2);
    await expect(page.locator('tr', { has: page.getByText(name2, { exact: true }) }).getByText('Base unit')).toBeVisible();

    // The server-enforced single-base-per-dimension rule should have flipped the first one back.
    await page.getByPlaceholder('Search code or name').fill(code);
    await expect(page.locator('tr', { has: page.getByText(name, { exact: true }) }).getByText('Conversion unit')).toBeVisible();
  });

  test('updates a unit (name, symbol, unset base)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/unit-of-measure');
    await page.getByPlaceholder('Search code or name').fill(code2);

    const row = page.locator('tr', { has: page.getByText(name2, { exact: true }) });
    await row.getByRole('button', { name: 'Edit' }).click();
    await expect(page.getByRole('heading', { name: 'Edit unit' })).toBeVisible();

    await fieldControl(page, 'Name').fill(updatedName);
    await fieldControl(page, 'Symbol').fill('upd');
    await page.getByLabel('This is the base unit for this dimension').uncheck();
    await fieldControl(page, 'Factor to Base').fill('3');

    await page.getByRole('button', { name: /save changes/i }).click();
    await expect(page.getByText('Unit updated.')).toBeVisible();

    await page.getByPlaceholder('Search code or name').fill(code2);
    const updatedRow = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(updatedRow).toBeVisible();
    await expect(updatedRow.getByText('upd', { exact: true })).toBeVisible();
    await expect(updatedRow.getByText('3', { exact: true })).toBeVisible();
    await expect(updatedRow.getByText('Conversion unit')).toBeVisible();
  });

  test('filters by search text', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/unit-of-measure');

    await page.getByPlaceholder('Search code or name').fill(code2);
    await expect(page.getByText(updatedName, { exact: true })).toBeVisible();

    await page.getByPlaceholder('Search code or name').fill('zzz-no-such-unit-zzz');
    await expect(page.getByText('No units found.')).toBeVisible();
  });

  test('deletes (soft-removes) both test units', async ({ page, request }) => {
    await login(page, request);

    for (const [c, n] of [[code, name], [code2, updatedName]]) {
      await page.goto('/unit-of-measure');
      await page.getByPlaceholder('Search code or name').fill(c);
      const row = page.locator('tr', { has: page.getByText(n, { exact: true }) });
      await expect(row).toBeVisible();
      await row.getByRole('button', { name: 'Remove' }).click();
      await expect(page.getByText(`${c} removed.`)).toBeVisible();
    }

    await page.goto('/unit-of-measure');
    await page.getByPlaceholder('Search code or name').fill(code);
    await expect(page.getByText('No units found.')).toBeVisible();
  });
});
