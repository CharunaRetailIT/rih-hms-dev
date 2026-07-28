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

// Field labels are a plain <label> with no htmlFor/id (so getByLabel() won't
// associate them) whose control lives in the same wrapper div — usually as a
// direct sibling, but "Product Code" wraps its <input> in an extra flex div
// alongside the auto-generate button, so scope to the shared parent instead
// of requiring an immediate-sibling match.
function fieldControl(page: Page, labelText: string) {
  return page
    .locator(`label:text-is("${labelText}")`)
    .locator('xpath=..')
    .locator('input, select, textarea')
    .first();
}

// "Check" toggles aren't real checkboxes — a <label> wraps the text and a
// styled <span> whose onClick actually flips the value. Scope to a specific
// SectionCard so e.g. "Active" doesn't also match a supplier row's real
// checkbox labeled "Active" once sub-collection rows exist.
function toggle(container: Locator, labelText: string) {
  return container.locator('label').filter({ hasText: labelText }).locator('> span').first();
}

// Each SectionCard renders as a <section> containing a unique <h3> title —
// sections don't nest, so this reliably scopes queries to one card.
function section(page: Page, title: string) {
  return page.locator('section', { has: page.getByRole('heading', { level: 3, name: title }) });
}

function sectionCount(page: Page, title: string) {
  return page.locator('h3', { hasText: title }).locator('xpath=following-sibling::span[1]');
}

// The form is split into tabs (General / Pricing & Stock / Serving Units /
// Suppliers / Printers / Charges / Price Level) — only the active tab's
// fields are mounted, so every cross-tab interaction switches first. Once a
// tab's sub-collection is non-empty its button's accessible name gains a
// trailing count badge (e.g. "Serving Units 1") — match the label as a
// prefix, not exact, so the switch still finds it.
async function switchTab(page: Page, label: string) {
  await page.getByRole('button', { name: new RegExp(`^${label}( \\d+)?$`) }).click();
}

async function fillRequiredBasics(page: Page, opts: { sku: string; name: string; cost?: string; price?: string }) {
  await fieldControl(page, 'Product Code').fill(opts.sku);
  await fieldControl(page, 'Product Name').fill(opts.name);
  // Unit / Cost Price / Selling Price live on the "Pricing & Stock" tab.
  await switchTab(page, 'Pricing & Stock');
  await fieldControl(page, 'Unit').selectOption({ index: 1 });
  await fieldControl(page, 'Cost Price').fill(opts.cost ?? '100');
  await fieldControl(page, 'Selling Price').fill(opts.price ?? '250');
}

test.describe('Products — list page', () => {
  test('page loads with heading, actions, and table', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/menu');

    await expect(page.getByRole('heading', { level: 1, name: 'Menu' })).toBeVisible();
    await expect(page.getByRole('heading', { level: 2, name: 'Products' })).toBeVisible();
    await expect(page.getByRole('button', { name: /^new product$/i })).toBeVisible();
    // "Price levels" / "Kitchen stations" quick-add buttons were removed from
    // this page — the dedicated /price-levels and /kitchen-stations pages are
    // the only way to manage them now (see products.spec.ts's rich-combo test).
    await expect(page.getByRole('columnheader', { name: 'SKU' })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Category' })).toBeVisible();
  });

  test('search filters the product list', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/menu');
    await expect(page.getByRole('columnheader', { name: 'SKU' })).toBeVisible();

    await page.getByPlaceholder('Search by name or SKU').fill('zzz-no-such-product-zzz');
    await expect(page.getByText('No products match your filter.')).toBeVisible();
  });
});

test.describe('Products — create/edit form', () => {
  test.describe.configure({ mode: 'serial', retries: 0 });

  const stamp = Date.now().toString().slice(-6);
  const sku = `E2E${stamp}`;
  const name = `E2E Test Product ${stamp}`;
  const updatedName = `E2E Test Product ${stamp} (Updated)`;

  test('rejects empty required fields inline', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/menu/create');
    await expect(page.getByRole('heading', { level: 2, name: 'Create Product' })).toBeVisible();

    await page.getByRole('button', { name: /save product/i }).first().click();
    await expect(page.getByText('Product code is required.')).toBeVisible();
    await expect(page.getByText('Product name is required.')).toBeVisible();
    await expect(page.getByText('Valid selling price is required.')).toBeVisible();
    await expect(page.getByText('Valid cost price is required.')).toBeVisible();
    // Still on the create form — nothing was submitted.
    await expect(page).toHaveURL(/\/menu\/create/);
  });

  test('creates a minimal finished product', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/menu/create');

    await fillRequiredBasics(page, { sku, name, cost: '100', price: '250' });
    await page.getByRole('button', { name: /save product/i }).first().click();

    // No toast assertion here on purpose: submit() calls flash() then
    // immediately router.push('/menu') — ProductForm unmounts before the
    // toast's setTimeout ever renders it, so it's never actually visible.
    // The URL change + the row appearing in the list are the real signals.
    await expect(page).toHaveURL(/\/menu$/, { timeout: 10_000 });

    await page.getByPlaceholder('Search by name or SKU').fill(sku);
    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await expect(row.getByText(sku)).toBeVisible();
    await expect(row.getByText('100.00', { exact: true })).toBeVisible();
    await expect(row.getByText('250.00', { exact: true })).toBeVisible();
    await expect(row.getByText('Active')).toBeVisible();
  });

  test('rejects a duplicate product code', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/menu/create');

    await fillRequiredBasics(page, { sku, name: `${name} Duplicate` }); // same sku
    await page.getByRole('button', { name: /save product/i }).first().click();

    await expect(page.getByText('Product code already exists.')).toBeVisible();
    await expect(page).toHaveURL(/\/menu\/create/);
  });

  test('updates the product (name, price, deactivate)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/menu');
    await page.getByPlaceholder('Search by name or SKU').fill(sku);

    const row = page.locator('tr', { has: page.getByText(name, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Edit' }).click();

    await expect(page.getByRole('heading', { level: 2, name: 'Edit Product' })).toBeVisible();
    await fieldControl(page, 'Product Name').fill(updatedName);
    await switchTab(page, 'Pricing & Stock');
    await fieldControl(page, 'Selling Price').fill('300');
    await toggle(section(page, 'Product Flags'), 'Active').click();

    await page.getByRole('button', { name: /save changes/i }).first().click();
    await expect(page).toHaveURL(/\/menu$/, { timeout: 10_000 });

    await page.getByPlaceholder('Search by name or SKU').fill(sku);
    const updatedRow = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(updatedRow).toBeVisible();
    await expect(updatedRow.getByText('300.00', { exact: true })).toBeVisible();
    await expect(updatedRow.getByText('Inactive')).toBeVisible();
  });

  test('product type + flag combination: raw material, not sold, no stock tracking', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/menu/create');

    const rmSku = `${sku}RM`;
    const rmName = `${name} Raw Material`;
    await fillRequiredBasics(page, { sku: rmSku, name: rmName });
    await switchTab(page, 'General');
    await fieldControl(page, 'Product Type').selectOption({ label: 'Raw Material' });

    await switchTab(page, 'Pricing & Stock');
    const flags = section(page, 'Product Flags');
    await toggle(flags, 'Sold on POS').click();      // finished-goods default is on; raw material isn't sold
    await toggle(flags, 'Stock Tracked').click();     // turn off stock tracking
    await toggle(flags, 'Track Expiry').click();      // turn on expiry tracking

    await page.getByRole('button', { name: /save product/i }).first().click();
    await expect(page).toHaveURL(/\/menu$/, { timeout: 10_000 });

    // Re-open to confirm the combination actually persisted, not just the toast.
    await page.getByPlaceholder('Search by name or SKU').fill(rmSku);
    await page.locator('tr', { has: page.getByText(rmName, { exact: true }) })
      .getByRole('button', { name: 'Edit' }).click();
    await expect(page.getByRole('heading', { level: 2, name: 'Edit Product' })).toBeVisible();

    await expect(fieldControl(page, 'Product Type')).toHaveValue('raw_material');
    await switchTab(page, 'Pricing & Stock');
    const flags2 = section(page, 'Product Flags');
    await expect(toggle(flags2, 'Sold on POS').locator('..')).not.toHaveClass(/border-primary/);
    await expect(toggle(flags2, 'Stock Tracked').locator('..')).not.toHaveClass(/border-primary/);
    await expect(toggle(flags2, 'Track Expiry').locator('..')).toHaveClass(/border-primary/);
  });

  test('tax + discount combination: non-taxable with a percentage discount', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/menu/create');

    const taxSku = `${sku}TX`;
    const taxName = `${name} Tax Combo`;
    await fillRequiredBasics(page, { sku: taxSku, name: taxName });

    await toggle(section(page, 'Tax & Discount'), 'Taxable').click(); // turn off
    await fieldControl(page, 'Discount Type').selectOption({ label: 'Percentage' });
    await fieldControl(page, 'Discount Value').fill('15');
    await fieldControl(page, 'Tax Class').selectOption({ label: 'Exempt' });

    await page.getByRole('button', { name: /save product/i }).first().click();
    await expect(page).toHaveURL(/\/menu$/, { timeout: 10_000 });

    await page.getByPlaceholder('Search by name or SKU').fill(taxSku);
    await page.locator('tr', { has: page.getByText(taxName, { exact: true }) })
      .getByRole('button', { name: 'Edit' }).click();
    await expect(page.getByRole('heading', { level: 2, name: 'Edit Product' })).toBeVisible();
    await switchTab(page, 'Pricing & Stock');

    await expect(toggle(section(page, 'Tax & Discount'), 'Taxable').locator('..')).not.toHaveClass(/border-primary/);
    await expect(fieldControl(page, 'Discount Type')).toHaveValue('percentage');
    await expect(fieldControl(page, 'Discount Value')).toHaveValue('15');
    await expect(fieldControl(page, 'Tax Class')).toHaveValue('exempt');
  });

  test('one product exercising every sub-collection at once (variant, supplier, kitchen station, price level)', async ({ page, request }) => {
    await login(page, request);

    // The demo tenant's lookups start with zero kitchen stations and zero
    // price levels (unlike suppliers, which has one seeded) — "Add Station"
    // / "Add Price" silently no-op when their lookup list is empty. Provision
    // one of each first. (Charges needs its own two-step ChargeType → Charge
    // setup, so it's out of scope here — covered separately once Tax &
    // Service Charge gets its own spec.)
    const rcStamp = Date.now().toString().slice(-6);

    // NOTE: the "Kitchen stations" quick-add modal on THIS page (/menu) never
    // collects a Printer Type, yet KitchenStation.PrinterTypeId is a required
    // (non-nullable) FK that the backend validates — every save through that
    // modal fails silently (no toast, no row). Using the dedicated
    // /kitchen-stations page instead, which has the printer-type selector.
    await page.goto('/kitchen-stations');
    await page.getByRole('button', { name: /^printer types$/i }).click();
    await expect(page.getByRole('heading', { name: 'Printer Types' })).toBeVisible();
    await page.locator('label:text-is("Code") + input').fill(`E2ERC${rcStamp}`);
    await page.locator('label:text-is("Name") + input').fill(`E2E RC Printer ${rcStamp}`);
    await page.getByRole('button', { name: /^save$/i }).click();
    await expect(page.getByText('Printer type created.')).toBeVisible();

    await page.goto('/kitchen-stations');
    await page.getByRole('button', { name: /^new station$/i }).click();
    await page.locator('label:text-is("Code") + input').fill(`E2ERC${rcStamp}`);
    await page.locator('label:text-is("Name") + input').fill(`E2E RC Station ${rcStamp}`);
    await page.locator('label:text-is("Printer type") + select').selectOption({ label: `E2ERC${rcStamp} — E2E RC Printer ${rcStamp}` });
    await page.getByRole('button', { name: /save station/i }).click();
    await expect(page.getByText('Kitchen station created.')).toBeVisible();

    // Same story for price levels — the quick-add modal on /menu was removed
    // (see menu/page.tsx); use the dedicated /price-levels page instead.
    await page.goto('/price-levels');
    await page.getByRole('button', { name: /^new price level$/i }).click();
    await page.locator('label:text-is("Code") + input').fill(`E2ERC${rcStamp}`);
    await page.locator('label:text-is("Name") + input').fill(`E2E RC Level ${rcStamp}`);
    await page.getByRole('button', { name: /save price level/i }).click();
    await expect(page.getByText('Price level created.')).toBeVisible();

    await page.goto('/menu/create');
    const richSku = `${sku}RC`;
    const richName = `${name} Full Combo`;
    await fillRequiredBasics(page, { sku: richSku, name: richName });

    // Variant — code/name are the only fields the backend requires for a row to count.
    await switchTab(page, 'Serving Units');
    const variants = section(page, 'Serving Units / Variants');
    await variants.getByRole('button', { name: /add variant/i }).click();
    const variantRow = variants.locator('div.grid-cols-12').last();
    await variantRow.locator('input[placeholder="CODE"]').fill('LRG');
    await variantRow.locator('input[placeholder="Name"]').fill('Large');

    // Supplier — defaults to the first available supplier and preferred=true (first row).
    await switchTab(page, 'Suppliers');
    const suppliers = section(page, 'Suppliers');
    await suppliers.getByRole('button', { name: /add supplier/i }).click();

    // Kitchen station routing — defaults to the first (only) available station, the one just created.
    await switchTab(page, 'Printers');
    const stations = section(page, 'Kitchen Stations');
    await stations.getByRole('button', { name: /add station/i }).click();

    // Price level override — defaults to the first (only) available price level, the one just created.
    await switchTab(page, 'Price Level');
    const prices = section(page, 'Price Levels');
    await prices.getByRole('button', { name: /add price/i }).click();

    await page.getByRole('button', { name: /save product/i }).first().click();
    await expect(page).toHaveURL(/\/menu$/, { timeout: 10_000 });

    await page.getByPlaceholder('Search by name or SKU').fill(richSku);
    await page.locator('tr', { has: page.getByText(richName, { exact: true }) })
      .getByRole('button', { name: 'Edit' }).click();
    await expect(page.getByRole('heading', { level: 2, name: 'Edit Product' })).toBeVisible();

    // All four sub-collections round-tripped through a save + reload with count 1.
    // Each lives on its own tab, so the count badge only mounts once that tab is active.
    await switchTab(page, 'Serving Units');
    await expect(sectionCount(page, 'Serving Units / Variants')).toHaveText('1');
    await switchTab(page, 'Suppliers');
    await expect(sectionCount(page, 'Suppliers')).toHaveText('1');
    await switchTab(page, 'Printers');
    await expect(sectionCount(page, 'Kitchen Stations')).toHaveText('1');
    await switchTab(page, 'Price Level');
    await expect(sectionCount(page, 'Price Levels')).toHaveText('1');

    await switchTab(page, 'Serving Units');
    const reopenedVariantRow = section(page, 'Serving Units / Variants').locator('div.grid-cols-12').last();
    await expect(reopenedVariantRow.locator('input[placeholder="CODE"]')).toHaveValue('LRG');
    await expect(reopenedVariantRow.locator('input[placeholder="Name"]')).toHaveValue('Large');

    // Remove the variant and re-save — the count should drop back to 0.
    await reopenedVariantRow.locator('button:has(svg)').click();
    await page.getByRole('button', { name: /save changes/i }).first().click();
    await expect(page).toHaveURL(/\/menu$/, { timeout: 10_000 });

    await page.getByPlaceholder('Search by name or SKU').fill(richSku);
    await page.locator('tr', { has: page.getByText(richName, { exact: true }) })
      .getByRole('button', { name: 'Edit' }).click();
    await switchTab(page, 'Serving Units');
    await expect(sectionCount(page, 'Serving Units / Variants')).toHaveText('0');
    await switchTab(page, 'Suppliers');
    await expect(sectionCount(page, 'Suppliers')).toHaveText('1'); // untouched
  });

  test('deletes a product from the edit page (native confirm dialog)', async ({ page, request }) => {
    await login(page, request);
    await page.goto('/menu');
    await page.getByPlaceholder('Search by name or SKU').fill(updatedName);

    const row = page.locator('tr', { has: page.getByText(updatedName, { exact: true }) });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Edit' }).click();
    await expect(page.getByRole('heading', { level: 2, name: 'Edit Product' })).toBeVisible();

    page.once('dialog', (dialog) => dialog.accept());
    await page.getByRole('button', { name: /^delete$/i }).click();

    await expect(page).toHaveURL(/\/menu$/, { timeout: 10_000 });
    await page.getByPlaceholder('Search by name or SKU').fill(updatedName);
    await expect(page.getByText(updatedName, { exact: true })).not.toBeVisible();
  });

  test('deletes the remaining test products from the list page (no confirm dialog)', async ({ page, request }) => {
    await login(page, request);
    for (const s of [`${sku}RM`, `${sku}TX`, `${sku}RC`]) {
      await page.goto('/menu');
      await page.getByPlaceholder('Search by name or SKU').fill(s);
      const row = page.locator('tr', { has: page.getByText(s, { exact: false }) }).first();
      await expect(row).toBeVisible(); // waits out the search's own reload instead of racing it
      await row.getByRole('button', { name: 'Remove' }).click();
      await expect(page.getByText(/removed\.$/)).toBeVisible();
    }
  });
});
