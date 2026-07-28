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
  await page.getByRole('button', { name: /^new serving unit$/i }).click();
  await expect(page.getByRole('heading', { name: 'New serving unit' })).toBeVisible();
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
  await page.getByPlaceholder('Search code or name').fill(text);
  await page.getByRole('button', { name: /^search$/i }).click();
}

test.describe('Serving Units — CRUD', () => {
  // Serial + no retries: later tests depend on the row created earlier. A
  // retry re-imports the file in a fresh worker, regenerating `stamp` and
  // silently pointing later tests at a row that retry never created.
  test.describe.configure({ mode: 'serial', retries: 0 });

  const stamp = Date.now().toString().slice(-6);
  const code = `E2E${stamp}`;
  const name = `E2E Test Unit ${stamp}`;
  const updatedName = `E2E Test Unit ${stamp} (Updated)`;

  test('page loads with heading, filters, and table', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/serving-units');

    // The page's own <h2> repeats the Topbar's <h1> text ("Serving Units"),
    // so heading level disambiguates rather than name alone.
    await expect(page.getByRole('heading', { level: 1, name: 'Serving Units' })).toBeVisible();
    await expect(page.getByRole('heading', { level: 2, name: 'Serving Units' })).toBeVisible();
    await expect(page.getByRole('button', { name: /^new serving unit$/i })).toBeVisible();
    await expect(page.getByPlaceholder('Search code or name')).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Code' })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Sort' })).toBeVisible();
    // Location was removed entirely from this screen — no filter, column, or form field.
    await expect(page.getByRole('columnheader', { name: 'Location' })).toHaveCount(0);
  });

  test('rejects empty required fields inline (no toast, modal stays open)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/serving-units');
    await openNewUnitModal(page);

    await page.getByRole('button', { name: /save serving unit/i }).click();

    await expect(page.getByText('Code is required.')).toBeVisible();
    await expect(page.getByText('Name is required.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New serving unit' })).toBeVisible();
  });

  test('creates a new serving unit', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/serving-units');
    await openNewUnitModal(page);

    await fieldControl(page, 'Code').fill(code);
    await fieldControl(page, 'Sort order').fill('7');
    await fieldControl(page, 'Name').fill(name);

    await page.getByRole('button', { name: /save serving unit/i }).click();

    await expect(page.getByText('Serving unit created.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New serving unit' })).not.toBeVisible();

    await search(page, code);
    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await expect(row.getByText(code)).toBeVisible();
    // exact: true — the generated E2E code can itself contain "7" as a substring.
    await expect(row.getByText('7', { exact: true })).toBeVisible();
    await expect(row.getByText('Active')).toBeVisible();
  });

  test('rejects a duplicate code', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/serving-units');
    await openNewUnitModal(page);

    await fieldControl(page, 'Code').fill(code); // same code → collides
    await fieldControl(page, 'Name').fill(`${name} Duplicate`);
    await page.getByRole('button', { name: /save serving unit/i }).click();

    await expect(page.getByText('Serving unit code already exists.')).toBeVisible();
  });

  test('updates the serving unit', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/serving-units');
    await search(page, code);

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Edit' }).click();

    await expect(page.getByRole('heading', { name: 'Edit serving unit' })).toBeVisible();
    await fieldControl(page, 'Name').fill(updatedName);
    await fieldControl(page, 'Sort order').fill('99');
    await page.getByLabel('Active').uncheck();

    await page.getByRole('button', { name: /save serving unit/i }).click();
    await expect(page.getByText('Serving unit updated.')).toBeVisible();

    await search(page, code);
    const updatedRow = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(updatedRow).toBeVisible();
    // exact: true — the generated E2E code can itself contain "99" as a substring.
    await expect(updatedRow.getByText('99', { exact: true })).toBeVisible();
    await expect(updatedRow.getByText('Inactive')).toBeVisible();
  });

  test('filters by search text', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/serving-units');

    await search(page, code);
    await expect(page.getByText(updatedName, { exact: true })).toBeVisible();

    await search(page, 'zzz-no-such-serving-unit-zzz');
    await expect(page.getByText('No serving units found.')).toBeVisible();
  });

  test('deletes (soft-removes) the serving unit', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/serving-units');
    await search(page, code);

    const row = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Remove' }).click();

    await expect(page.getByText(`${updatedName} removed.`)).toBeVisible();
    await expect(page.getByText(updatedName, { exact: true })).not.toBeVisible();
    await expect(page.getByText('No serving units found.')).toBeVisible();
  });
});
