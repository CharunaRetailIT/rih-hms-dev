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

async function createUnit(page: Page, opts: { code: string; name: string; dimension: string; isBase?: boolean; factor?: string }) {
  await page.goto('/unit-of-measure');
  await page.getByRole('button', { name: /^new unit$/i }).click();
  await fieldControl(page, 'Code').fill(opts.code);
  await fieldControl(page, 'Name').fill(opts.name);
  await fieldControl(page, 'Dimension').selectOption({ label: opts.dimension });
  if (opts.isBase) {
    await page.getByLabel('This is the base unit for this dimension').check();
  } else if (opts.factor) {
    await fieldControl(page, 'Factor to Base').fill(opts.factor);
  }
  await page.getByRole('button', { name: /create unit/i }).click();
  await expect(page.getByText('Unit created.')).toBeVisible();
}

async function openNewConversionModal(page: Page) {
  await page.getByRole('button', { name: /^new conversion$/i }).click();
  await expect(page.getByRole('heading', { name: 'New conversion' })).toBeVisible();
}

test.describe('UOM Conversions — CRUD + calculator', () => {
  // Serial + no retries: later tests depend on units/rows created earlier. A
  // retry re-imports the file in a fresh worker, regenerating `stamp` and
  // silently pointing later tests at data that retry never created.
  test.describe.configure({ mode: 'serial', retries: 0 });

  const stamp = Date.now().toString().slice(-6);
  const baseCode = `E2EB${stamp}`;
  const baseName = `E2E Base Unit ${stamp}`;
  const subCode = `E2ES${stamp}`;
  const subName = `E2E Sub Unit ${stamp}`;
  const otherDimCode = `E2ED${stamp}`;
  const otherDimName = `E2E Other Dim ${stamp}`;

  test('sets up two count-dimension units and one mass-dimension unit', async ({ page, request }) => {
    await login(page, request);
    // Base unit for "count" — factor locked to 1.
    await createUnit(page, { code: baseCode, name: baseName, dimension: 'Count', isBase: true });
    // A second "count" unit where 1 sub-unit = 10 base units.
    await createUnit(page, { code: subCode, name: subName, dimension: 'Count', factor: '10' });
    // A unit in a different dimension, to test the cross-dimension rejection.
    await createUnit(page, { code: otherDimCode, name: otherDimName, dimension: 'Mass', isBase: true });
  });

  test('page loads with heading, calculator, and table', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/uom-conversions');

    await expect(page.getByRole('heading', { level: 1, name: 'Master Data' })).toBeVisible();
    await expect(page.getByRole('heading', { level: 2, name: 'Unit Conversions' })).toBeVisible();
    await expect(page.getByRole('button', { name: /^new conversion$/i })).toBeVisible();
    await expect(page.locator('label:text-is("From unit")')).toBeVisible();
    await expect(page.locator('label:text-is("To unit")')).toBeVisible();
    await expect(page.getByRole('button', { name: /^convert$/i })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Base Unit' })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Meaning' })).toBeVisible();
  });

  test('calculator converts using each unit\'s own factor-to-base (independent of any conversion row)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/uom-conversions');

    // From = sub unit (factor 10), To = base unit (factor 1), qty 2 → 2 * 10 / 1 = 20.
    // Positional, not label-scoped: these are literally the first two <select>
    // elements on the page (before any modal/filter selects exist in the DOM).
    const selects = page.locator('select');
    await selects.nth(0).selectOption({ label: `${subCode} — ${subName}` }); // From unit
    // Wait for React to actually commit this controlled-select's value before
    // touching the next select — without this checkpoint the "From unit"
    // selection intermittently gets lost (confirmed as a test-timing issue,
    // not an app bug, via direct framework-level interaction outside Playwright).
    await expect(selects.nth(0).locator('option:checked')).toHaveText(`${subCode} — ${subName}`);
    await selects.nth(1).selectOption({ label: `${baseCode} — ${baseName}` }); // To unit
    await expect(selects.nth(1).locator('option:checked')).toHaveText(`${baseCode} — ${baseName}`);
    await page.locator('label:text-is("Quantity") + input').fill('2');
    await page.getByRole('button', { name: /^convert$/i }).click();

    await expect(page.getByText('Conversion result')).toBeVisible();
    await expect(page.getByText(`2 ${subCode}`)).toBeVisible();
    await expect(page.getByText(`20 ${baseCode}`)).toBeVisible();
  });

  test('rejects an empty sub-unit selection (toast, modal stays open)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/uom-conversions');
    await openNewConversionModal(page);

    // "Base unit" defaults to the first available unit when the modal opens
    // (openNew() pre-fills it), so "Sub unit is required." is the first
    // reachable validation message via a plain empty-form submit.
    await page.getByRole('button', { name: /create conversion/i }).click();
    await expect(page.getByText('Sub unit is required.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New conversion' })).toBeVisible();
  });

  test('rejects a cross-dimension conversion', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/uom-conversions');
    await openNewConversionModal(page);

    await fieldControl(page, 'Base unit').selectOption({ label: `${baseCode} — ${baseName}` }); // count
    await fieldControl(page, 'Sub unit').selectOption({ label: `${otherDimCode} — ${otherDimName}` }); // mass
    await page.locator('label:text-is("Sub unit value") + input').fill('1');
    await page.locator('label:text-is("Base unit value") + input').fill('1');
    await page.getByRole('button', { name: /create conversion/i }).click();

    await expect(page.getByText('Base unit and sub unit must be in same dimension.')).toBeVisible();
  });

  test('creates a new conversion rule', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/uom-conversions');
    await openNewConversionModal(page);

    await fieldControl(page, 'Base unit').selectOption({ label: `${baseCode} — ${baseName}` });
    await fieldControl(page, 'Sub unit').selectOption({ label: `${subCode} — ${subName}` });
    await page.locator('label:text-is("Sub unit value") + input').fill('1');
    await page.locator('label:text-is("Base unit value") + input').fill('10');
    await page.getByRole('button', { name: /create conversion/i }).click();

    await expect(page.getByText('Conversion created.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'New conversion' })).not.toBeVisible();

    await page.getByPlaceholder('Search conversion').fill(baseCode);
    const row = page.locator('tr', { has: page.getByText(baseName, { exact: true }) });
    await expect(row).toBeVisible();
    await expect(row.getByText(subName, { exact: true })).toBeVisible();
    await expect(row.getByText(`1 ${subCode} = 10 ${baseCode}`)).toBeVisible();
  });

  test('rejects a duplicate conversion pair', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/uom-conversions');
    await openNewConversionModal(page);

    await fieldControl(page, 'Base unit').selectOption({ label: `${baseCode} — ${baseName}` });
    await fieldControl(page, 'Sub unit').selectOption({ label: `${subCode} — ${subName}` });
    await page.locator('label:text-is("Sub unit value") + input').fill('1');
    await page.locator('label:text-is("Base unit value") + input').fill('99');
    await page.getByRole('button', { name: /create conversion/i }).click();

    await expect(page.getByText('This conversion already exists.')).toBeVisible();
  });

  test('updates the conversion', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/uom-conversions');
    await page.getByPlaceholder('Search conversion').fill(baseCode);

    const row = page.locator('tr', { has: page.getByText(baseName, { exact: true }) });
    await row.getByRole('button', { name: 'Edit' }).click();
    await expect(page.getByRole('heading', { name: 'Edit conversion' })).toBeVisible();

    await page.locator('label:text-is("Base unit value") + input').fill('25');
    await page.getByRole('button', { name: /save changes/i }).click();
    await expect(page.getByText('Conversion updated.')).toBeVisible();

    await page.getByPlaceholder('Search conversion').fill(baseCode);
    const updatedRow = page.locator('tr', { has: page.getByText(baseName, { exact: true }) });
    await expect(updatedRow.getByText(`1 ${subCode} = 25 ${baseCode}`)).toBeVisible();
  });

  test('filters by search text', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/uom-conversions');

    await page.getByPlaceholder('Search conversion').fill(baseCode);
    await expect(page.getByText(baseName, { exact: true })).toBeVisible();

    await page.getByPlaceholder('Search conversion').fill('zzz-no-such-conversion-zzz');
    await expect(page.getByText('No conversion records found.')).toBeVisible();
  });

  test('deletes the conversion, then cleans up the helper units', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/uom-conversions');
    await page.getByPlaceholder('Search conversion').fill(baseCode);

    const row = page.locator('tr', { has: page.getByText(baseName, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Remove' }).click();
    await expect(page.getByText(`${subName} removed.`)).toBeVisible();

    await page.getByPlaceholder('Search conversion').fill(baseCode);
    await expect(page.getByText('No conversion records found.')).toBeVisible();

    // Clean up the three helper units.
    for (const [c, n] of [[baseCode, baseName], [subCode, subName], [otherDimCode, otherDimName]]) {
      await page.goto('/unit-of-measure');
      await page.getByPlaceholder('Search code or name').fill(c);
      const unitRow = page.locator('tr', { has: page.getByText(n, { exact: true }) });
      await expect(unitRow).toBeVisible();
      await unitRow.getByRole('button', { name: 'Remove' }).click();
      await expect(page.getByText(`${c} removed.`)).toBeVisible();
    }
  });
});
