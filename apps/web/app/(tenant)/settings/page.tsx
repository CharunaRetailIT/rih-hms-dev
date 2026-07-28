'use client';

import { useEffect, useState } from 'react';
import { useSearchParams } from 'next/navigation';
import { Topbar } from '@/components/app-shell/Topbar';
import { Icon } from '@/components/ui/Icon';
import { apiClient, lkr } from '@/lib/api-client';
import { AggregatorSettings } from '@/components/delivery/AggregatorSettings';
import { PageLoader } from '@/components/ui/PageLoader';
import { confirmDialog } from '@/components/ui/confirm';
import { Field, Combobox } from '@/components/ui/form';
import { AddressFields } from '@/components/ui/AddressFields';
import { type Address, EMPTY_ADDRESS } from '@/lib/regions';
import { loadPayHere } from '@/lib/payhere';
import { cn } from '@/lib/utils';

type SectionId = 'business' | 'billing' | 'tax' | 'loyalty' | 'period' | 'outlets' | 'delivery' | 'kitchen' | 'numbering' | 'print' | 'approvals';
const SECTIONS: { id: SectionId; title: string; desc: string; icon: string; save: boolean }[] = [
  { id: 'business',  title: 'Business identity',     desc: 'Legal name, VAT & registration',  icon: 'storefront',    save: true },
  { id: 'billing',   title: 'Subscription & billing', desc: 'Plan, add-ons & licenses',        icon: 'credit_card',   save: false },
  { id: 'tax',       title: 'Tax & charges',         desc: 'Bill-level charge stack',          icon: 'percent',       save: false },
  { id: 'approvals', title: 'Procurement approvals', desc: 'PO & GRN maker-checker',           icon: 'fact_check',    save: true },
  { id: 'loyalty',   title: 'Loyalty',               desc: 'Points earn, redeem & tiers',      icon: 'loyalty',       save: true },
  { id: 'period',    title: 'Accounting period',     desc: 'Close & lock the books',           icon: 'lock_clock',    save: false },
  { id: 'outlets',   title: 'Outlets',               desc: 'Branches & locations',             icon: 'storefront',    save: false },
  { id: 'delivery',  title: 'Delivery integrations', desc: 'Uber Eats & PickMe',               icon: 'local_shipping',save: false },
  { id: 'kitchen',   title: 'Kitchen / KOT',         desc: 'Display & ticket printing',        icon: 'soup_kitchen',  save: true },
  { id: 'numbering', title: 'Document numbering',    desc: 'Prefixes for documents',           icon: 'tag',           save: true },
  { id: 'print',     title: 'Bill & invoice print',  desc: 'Headers & footers',                icon: 'receipt_long',  save: true },
];

type Settings = {
  legalName: string | null; vatRegistrationNumber: string | null; businessRegistrationNo: string | null;
  svatNumber: string | null; vatEnabled: boolean; vatFilingFrequency: string; taxLabel: string;
  invoicePrefix: string; orderPrefix: string; poPrefix: string; grnPrefix: string;
  supplierPrefix: string; transferPrefix: string; wastagePrefix: string; adjustmentPrefix: string; requestNotePrefix: string; productCodePrefix: string;
  allowDiscountBelowCost: boolean;
  billHeader: string | null; billFooter: string | null; taxInvoiceFooter: string | null; showVatOnBills: boolean;
  requireOpenShiftForBilling: boolean;
  requirePoApproval: boolean; poApprovalThreshold: number;
  requireGrnApproval: boolean; grnApprovalThreshold: number;
  requireStockAdjustmentApproval: boolean; stockAdjustmentApprovalThreshold: number;
  requireWastageApproval: boolean; wastageApprovalThreshold: number;
  requireRequestNoteApproval: boolean;
  requireTransferApproval: boolean; transferApprovalThreshold: number;
  loyaltyEnabled: boolean; loyaltyEarnRate: number; loyaltyRedeemValue: number; loyaltyExpiryDays: number;
  kdsEnabled: boolean; kotAutoPrint: boolean; booksLockedThrough: string | null;
  logoUrl: string | null; baseCurrency: string;
  tabDeviceLimit: number; guestQrEnabled: boolean;
};

const CURRENCIES = ['LKR', 'USD', 'EUR', 'GBP', 'INR', 'AUD', 'AED', 'SGD', 'MVR'].map(c => ({ value: c, label: c }));

export default function SettingsPage() {
  const [s, setS] = useState<Settings | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [toast, setToast] = useState<string | null>(null);

  useEffect(() => {
    apiClient<Settings>('/api/v1/settings').then(setS).catch(() => setToast('Could not load settings'))
      .finally(() => setLoading(false));
  }, []);

  const set = <K extends keyof Settings>(k: K, v: Settings[K]) => setS(p => (p ? { ...p, [k]: v } : p));
  const flash = (m: string) => { setToast(m); setTimeout(() => setToast(null), 2500); };

  const [lockDate, setLockDate] = useState('');
  // Business identity (legal name, tax regs, base currency) is OWNER-only — Admins can't edit it.
  const [isOwner, setIsOwner] = useState(false);
  useEffect(() => { try { setIsOwner(JSON.parse(localStorage.getItem('hms.user') || '{}').role === 0); } catch { /* */ } }, []);
  const [active, setActive] = useState<SectionId>(() =>
    (typeof window !== 'undefined' && new URLSearchParams(window.location.search).has('billing')) ? 'billing' : 'business');
  // Jump to the Subscription & billing tab whenever we arrive with ?billing — even if the
  // Settings page is already open (e.g. the "See plans" upgrade prompt from the sidebar).
  const searchParams = useSearchParams();
  useEffect(() => { if (searchParams.has('billing')) setActive('billing'); }, [searchParams]);
  async function closePeriod() {
    if (!lockDate) { flash('Pick a close-through date.'); return; }
    if (!(await confirmDialog({
      title: 'Close the books?',
      body: `Postings and reversals dated on or before ${lockDate} will be blocked. Only an owner can reopen a closed period.`,
      confirmLabel: 'Close period',
    }))) return;
    try { await apiClient('/api/v1/settings/period/close', { method: 'POST', body: JSON.stringify({ through: lockDate }) }); set('booksLockedThrough', lockDate); flash(`Books closed through ${lockDate}.`); }
    catch (e) { flash((e as Error).message.includes('{') ? JSON.parse((e as Error).message.slice((e as Error).message.indexOf('{'))).error : (e as Error).message); }
  }
  async function reopenPeriod() {
    if (!(await confirmDialog({
      title: 'Reopen the period?',
      body: 'This unlocks the closed books and allows postings in the previously frozen period again.',
      confirmLabel: 'Reopen',
    }))) return;
    try { await apiClient('/api/v1/settings/period/reopen', { method: 'POST', body: JSON.stringify({ through: null }) }); set('booksLockedThrough', null); flash('Books reopened.'); }
    catch (e) { flash((e as Error).message.includes('403') ? 'Only an owner can reopen a closed period.' : (e as Error).message); }
  }

  async function save() {
    if (!s) return;
    setSaving(true);
    try {
      await apiClient('/api/v1/settings', { method: 'PUT', body: JSON.stringify(s) });
      window.dispatchEvent(new Event('hms:settings-updated'));
      flash('Settings saved');
    } catch (e) { flash((e as Error).message); } finally { setSaving(false); }
  }

  if (loading || !s) {
    return (<><Topbar title="Settings" subtitle="Set the system up to run your business, your way" />
      <PageLoader label="Loading settings…" /></>);
  }

  const showSave = SECTIONS.find(x => x.id === active)?.save ?? false;
  return (
    <>
      <Topbar title="Settings" subtitle="Set the system up to run your business, your way" />
      <div className="mx-auto flex max-w-6xl flex-col gap-6 p-6 md:p-8 lg:flex-row lg:items-start">
        {/* Sub-menu: title + mini description + icon; loads one section at a time */}
        <nav className="shrink-0 space-y-1 lg:sticky lg:top-6 lg:w-72">
          {SECTIONS.map(item => {
            const on = active === item.id;
            return (
              <button key={item.id} onClick={() => setActive(item.id)}
                className={cn('flex w-full items-start gap-3 rounded-lg border px-3 py-2.5 text-left transition-colors',
                  on ? 'border-primary/40 bg-primary-tint' : 'border-transparent hover:bg-muted')}>
                <span className={cn('mt-0.5 flex size-8 shrink-0 items-center justify-center rounded-lg',
                  on ? 'bg-primary text-primary-foreground' : 'bg-muted text-muted-foreground')}>
                  <Icon name={item.icon} className="text-[18px]" />
                </span>
                <span className="min-w-0">
                  <span className={cn('block text-sm font-semibold', on ? 'text-primary-dark' : 'text-foreground')}>{item.title}</span>
                  <span className="block truncate text-xs text-muted-foreground">{item.desc}</span>
                </span>
              </button>
            );
          })}
        </nav>

        {/* Active section */}
        <div className="min-w-0 flex-1 space-y-4">
          {active === 'business' && (
            <Section title="Business Identity" icon="storefront"
              desc="Shown on tax invoices and (optionally) on bills.">
              {!isOwner && (
                <p className="mb-3 flex items-center gap-1.5 rounded-lg bg-muted/50 px-3 py-2 text-xs text-muted-foreground">
                  <Icon name="lock" className="text-[15px]" /> Legal name, tax registrations &amp; base currency can only be changed by the owner.
                </p>
              )}
              <Grid>
                <Field label="Legal name" value={s.legalName ?? ''} onChange={v => set('legalName', v)} placeholder="Demo Restaurant (Pvt) Ltd" disabled={!isOwner} />
                <Field label="VAT registration no." value={s.vatRegistrationNumber ?? ''} onChange={v => set('vatRegistrationNumber', v)} placeholder="134567890-7000" disabled={!isOwner} />
                <Field label="Business registration no." value={s.businessRegistrationNo ?? ''} onChange={v => set('businessRegistrationNo', v)} placeholder="PV 00123456" disabled={!isOwner} />
                <Field label="SVAT no. (if registered)" value={s.svatNumber ?? ''} onChange={v => set('svatNumber', v)} placeholder="SVAT…" disabled={!isOwner} />
                {isOwner ? (
                  <Combobox label="Base currency" value={s.baseCurrency ?? 'LKR'} onChange={v => set('baseCurrency', v)}
                    options={CURRENCIES} searchPlaceholder="Search currency…" helper="The single currency your books, bills and reports are kept in. The POS can show prices in any currency live, but always charges in this one." />
                ) : (
                  <Field label="Base currency" value={s.baseCurrency ?? 'LKR'} onChange={() => {}} disabled helper="Only the owner can change the base currency." />
                )}
                <Field label="Tax name" value={s.taxLabel ?? 'VAT'} onChange={v => set('taxLabel', v)} placeholder="VAT"
                  helper="What your country calls the consumption tax (VAT, GST, Sales Tax). Set from your signup country; shown across the app and on bills." />
              </Grid>
              <div className="mt-4">
                <label className="mb-1 block text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">Logo</label>
                <div className="flex items-center gap-3">
                  {s.logoUrl
                    ? <img src={s.logoUrl} alt="Logo" className="h-16 w-16 rounded-lg border border-border bg-card object-contain p-1" />
                    : <div className="grid h-16 w-16 place-items-center rounded-lg border border-dashed border-border text-[11px] text-muted-foreground">None</div>}
                  <div className="flex flex-col items-start gap-1">
                    <label className="cursor-pointer rounded-lg border border-border bg-card px-3 py-1.5 text-sm font-medium hover:bg-muted">
                      {s.logoUrl ? 'Replace logo' : 'Upload logo'}
                      <input type="file" accept="image/png,image/jpeg,image/svg+xml,image/webp" className="hidden"
                        onChange={e => {
                          const file = e.target.files?.[0]; if (!file) return;
                          if (file.size > 256 * 1024) { flash('Logo must be under 256 KB.'); return; }
                          const r = new FileReader(); r.onload = () => set('logoUrl', String(r.result)); r.readAsDataURL(file);
                        }} />
                    </label>
                    {s.logoUrl && <button type="button" onClick={() => set('logoUrl', null)} className="text-xs font-semibold text-status-error hover:underline">Remove</button>}
                    <span className="text-xs text-muted-foreground">PNG / JPG / SVG · up to 256&nbsp;KB. Shown in the sidebar &amp; on bills.</span>
                  </div>
                </div>
              </div>
              <label className="mt-3 flex items-center gap-2 text-sm">
                <input type="checkbox" checked={s.vatEnabled} onChange={e => set('vatEnabled', e.target.checked)} className="size-4 rounded border-border text-primary focus:ring-primary" />
                {s.taxLabel || 'Tax'} enabled (issue tax invoices with sequential numbers)
              </label>
              <label className="mt-2 flex items-center gap-2 text-sm">
                <input type="checkbox" checked={s.requireOpenShiftForBilling} onChange={e => set('requireOpenShiftForBilling', e.target.checked)} className="size-4 rounded border-border text-primary focus:ring-primary" />
                Require an open shift before billing (cashier must start a shift to take payment)
              </label>

            </Section>
          )}

          {active === 'billing' && <BillingSection flash={flash} />}

          {active === 'tax' && <TaxChargesSection />}

          {active === 'approvals' && (
            <Section title="Procurement approvals" icon="fact_check"
              desc="Maker-checker: route purchase orders, goods-received notes, stock adjustments and wastage through an approval step before they can be sent / posted to stock.">
              <p className="mb-3 rounded-lg bg-muted/50 px-3 py-2 text-xs text-muted-foreground">
                For Purchase Orders, GRNs and Transfers specifically: if a matching rule exists under <b>Approvals</b> (multi-level, named approvers), it takes priority over the simple toggle/threshold below. These only apply when no rule matches.
              </p>
              <label className="flex items-center gap-2 text-sm">
                <input type="checkbox" checked={s.requirePoApproval} onChange={e => set('requirePoApproval', e.target.checked)} className="size-4 rounded border-border text-primary focus:ring-primary" />
                Require approval on every purchase order
              </label>
              <Field label="Or require approval when a PO's total is at least (0 = no threshold)" value={String(s.poApprovalThreshold)}
                onChange={v => set('poApprovalThreshold', Number(v.replace(/[^0-9.]/g, '')) || 0)} />

              <label className="mt-4 flex items-center gap-2 text-sm">
                <input type="checkbox" checked={s.requireGrnApproval} onChange={e => set('requireGrnApproval', e.target.checked)} className="size-4 rounded border-border text-primary focus:ring-primary" />
                Require approval on every goods-received note
              </label>
              <Field label="Or require approval when a GRN's net amount is at least (0 = no threshold)" value={String(s.grnApprovalThreshold)}
                onChange={v => set('grnApprovalThreshold', Number(v.replace(/[^0-9.]/g, '')) || 0)} />
              <p className="mt-1 text-xs text-muted-foreground">A GRN awaiting approval has not touched stock yet — approving it is what posts it.</p>

              <label className="mt-4 flex items-center gap-2 text-sm">
                <input type="checkbox" checked={s.requireStockAdjustmentApproval} onChange={e => set('requireStockAdjustmentApproval', e.target.checked)} className="size-4 rounded border-border text-primary focus:ring-primary" />
                Require approval on every stock adjustment note
              </label>
              <Field label="Or require approval when an adjustment's absolute value is at least (0 = no threshold)" value={String(s.stockAdjustmentApprovalThreshold)}
                onChange={v => set('stockAdjustmentApprovalThreshold', Number(v.replace(/[^0-9.]/g, '')) || 0)} />

              <label className="mt-4 flex items-center gap-2 text-sm">
                <input type="checkbox" checked={s.requireWastageApproval} onChange={e => set('requireWastageApproval', e.target.checked)} className="size-4 rounded border-border text-primary focus:ring-primary" />
                Require approval on every wastage note
              </label>
              <Field label="Or require approval when a wastage note's cost is at least (0 = no threshold)" value={String(s.wastageApprovalThreshold)}
                onChange={v => set('wastageApprovalThreshold', Number(v.replace(/[^0-9.]/g, '')) || 0)} />
              <p className="mt-1 text-xs text-muted-foreground">A stock adjustment or wastage note awaiting approval has not touched stock yet — approving it is what posts it.</p>

              <label className="mt-4 flex items-center gap-2 text-sm">
                <input type="checkbox" checked={s.requireRequestNoteApproval} onChange={e => set('requireRequestNoteApproval', e.target.checked)} className="size-4 rounded border-border text-primary focus:ring-primary" />
                Require approval on every request note
              </label>
              <p className="mt-1 text-xs text-muted-foreground">A request note never touches stock either way — approving it only authorises it to be picked up by a purchase order or transfer.</p>

              <label className="mt-4 flex items-center gap-2 text-sm">
                <input type="checkbox" checked={s.requireTransferApproval} onChange={e => set('requireTransferApproval', e.target.checked)} className="size-4 rounded border-border text-primary focus:ring-primary" />
                Require approval on every transfer of goods
              </label>
              <Field label="Or require approval when a transfer's total cost is at least (0 = no threshold)" value={String(s.transferApprovalThreshold)}
                onChange={v => set('transferApprovalThreshold', Number(v.replace(/[^0-9.]/g, '')) || 0)} />
              <p className="mt-1 text-xs text-muted-foreground">A transfer awaiting approval has not touched stock yet — approving it is what dispatches it (decrements the sender's stock).</p>
            </Section>
          )}

          {active === 'loyalty' && (
            <Section title="Loyalty" icon="loyalty"
              desc="Customers earn points on settled bills and redeem them at the till as a 'Points' tender.">
              <label className="flex items-center gap-2 text-sm">
                <input type="checkbox" checked={s.loyaltyEnabled} onChange={e => set('loyaltyEnabled', e.target.checked)} className="size-4 rounded border-border text-primary focus:ring-primary" />
                Enable loyalty points
              </label>
              {s.loyaltyEnabled && (
                <Grid>
                  <Field label="Points earned per LKR 1 spent" inputMode="decimal" value={String(s.loyaltyEarnRate)}
                    onChange={v => set('loyaltyEarnRate', Number(v.replace(/[^0-9.]/g, '')) || 0)}
                    helper="e.g. 0.01 = 1 point per LKR 100" />
                  <Field label="LKR value of 1 point (on redemption)" inputMode="decimal" value={String(s.loyaltyRedeemValue)}
                    onChange={v => set('loyaltyRedeemValue', Number(v.replace(/[^0-9.]/g, '')) || 0)}
                    helper="e.g. 1 = each point is worth LKR 1" />
                  <Field label="Points expire after (days of inactivity)" inputMode="numeric" value={String(s.loyaltyExpiryDays)}
                    onChange={v => set('loyaltyExpiryDays', Number(v.replace(/[^0-9]/g, '')) || 0)}
                    helper="0 = never expire. Stale balances lapse on next earn/redeem (or a nightly sweep)." />
                </Grid>
              )}
              {s.loyaltyEnabled && <LoyaltyTiers flash={flash} />}
            </Section>
          )}

          {active === 'period' && (
            <Section title="Accounting Period" icon="lock_clock"
              desc="Close the books through a date to freeze that month — postings and reversals dated on/before it are blocked.">
              <div className="flex flex-wrap items-center gap-3">
                <div className="text-sm">
                  Currently closed through:{' '}
                  <span className="font-semibold">{s.booksLockedThrough ? s.booksLockedThrough : 'open (nothing locked)'}</span>
                </div>
                {s.booksLockedThrough && (
                  <button onClick={reopenPeriod} className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-semibold text-error hover:bg-muted">Reopen (owner)</button>
                )}
              </div>
              <div className="mt-3 flex flex-wrap items-end gap-2">
                <Field className="w-56" label="Close books through" type="date" value={lockDate} onChange={v => setLockDate(v)} />
                <button onClick={closePeriod} className="h-[38px] rounded-lg bg-primary px-4 text-sm font-bold text-primary-foreground hover:bg-primary-dark">Close Period</button>
              </div>
            </Section>
          )}

          {active === 'outlets' && (
            <Section title="Outlets" icon="storefront"
              desc="Branches & back-of-house locations. Type + capabilities drive selling, production and stock.">
              <OutletsManager flash={flash} />
            </Section>
          )}

          {active === 'delivery' && (
            <Section title="Delivery Integrations" icon="local_shipping"
              desc="Uber Eats & PickMe credentials and per-outlet store mapping. Secrets are stored encrypted and never shown — leave a field blank to keep the existing value.">
              <AggregatorSettings flash={flash} />
            </Section>
          )}

          {active === 'kitchen' && (
            <Section title="Kitchen / KOT" icon="soup_kitchen"
              desc="How kitchen tickets reach the kitchen. Run a display screen, a KOT printer, both, or neither (print from the POS and hand it over).">
              <label className="flex items-center gap-2 text-sm">
                <input type="checkbox" checked={s.kdsEnabled} onChange={e => set('kdsEnabled', e.target.checked)} className="size-4 rounded border-border text-primary focus:ring-primary" />
                Use the Kitchen Display (KDS) board
              </label>
              <p className="ml-6 text-xs text-muted-foreground">Off = no kitchen screen; the “Kitchen (KOT)” menu is hidden and staff work off printed tickets.</p>
              <label className="mt-3 flex items-center gap-2 text-sm">
                <input type="checkbox" checked={s.kotAutoPrint} onChange={e => set('kotAutoPrint', e.target.checked)} className="size-4 rounded border-border text-primary focus:ring-primary" />
                Print the KOT automatically when an order is sent to the kitchen
              </label>
              <p className="ml-6 text-xs text-muted-foreground">For venues with a KOT printer at the POS. You can also print a ticket any time with “Print KOT” on the till.</p>
            </Section>
          )}

          {active === 'numbering' && (
            <Section title="Document Numbering" icon="tag"
              desc="Prefixes for auto-generated document numbers. Sequences continue; only the prefix changes.">
              <Grid>
                <Field label="Bill / Order" value={s.orderPrefix} onChange={v => set('orderPrefix', v)} placeholder="ORD" />
                <Field label="Tax invoice" value={s.invoicePrefix} onChange={v => set('invoicePrefix', v)} placeholder="INV" />
                <Field label="Purchase order" value={s.poPrefix} onChange={v => set('poPrefix', v)} placeholder="PO" />
                <Field label="Goods received" value={s.grnPrefix} onChange={v => set('grnPrefix', v)} placeholder="GRN" />
                <Field label="Supplier code" value={s.supplierPrefix} onChange={v => set('supplierPrefix', v)} placeholder="SUP" />
                <Field label="Transfer" value={s.transferPrefix} onChange={v => set('transferPrefix', v)} placeholder="TRF" />
                <Field label="Wastage" value={s.wastagePrefix} onChange={v => set('wastagePrefix', v)} placeholder="WST" />
                <Field label="Adjustment" value={s.adjustmentPrefix} onChange={v => set('adjustmentPrefix', v)} placeholder="ADJ" />
                <Field label="Request note" value={s.requestNotePrefix} onChange={v => set('requestNotePrefix', v)} placeholder="REQ" />
                <Field label="Product code" value={s.productCodePrefix} onChange={v => set('productCodePrefix', v)} placeholder="ITM" />
              </Grid>
              <label className="mt-4 flex cursor-pointer items-start gap-2.5 rounded-lg border border-border bg-surface px-3 py-2.5 text-sm">
                <input type="checkbox" checked={!!s.allowDiscountBelowCost} onChange={e => set('allowDiscountBelowCost', e.target.checked)} className="mt-0.5 size-4 rounded" />
                <span><span className="font-medium">Allow item discounts below cost price</span><br /><span className="text-xs text-muted-foreground">Off (recommended): a per-product discount never drops the price below the item&apos;s cost. On: discounts may go below cost.</span></span>
              </label>
            </Section>
          )}

          {active === 'print' && (
            <Section title="Bill & Invoice Print" icon="receipt_long"
              desc="Header and footer printed on POS bills; tax-invoice footer shown on VAT invoices.">
              <Field multiline rows={2} label="Bill header" value={s.billHeader ?? ''} onChange={v => set('billHeader', v)} placeholder="Demo Restaurant · No.1 Galle Rd, Colombo · 011-2345678" />
              <Field multiline rows={2} label="Bill footer" value={s.billFooter ?? ''} onChange={v => set('billFooter', v)} placeholder="Thank you! Come again." />
              <Field multiline rows={2} label="Tax-invoice footer" value={s.taxInvoiceFooter ?? ''} onChange={v => set('taxInvoiceFooter', v)} placeholder="This is a VAT tax invoice." />
              <label className="flex items-center gap-2 text-sm">
                <input type="checkbox" checked={s.showVatOnBills} onChange={e => set('showVatOnBills', e.target.checked)} className="size-4 rounded border-border text-primary focus:ring-primary" />
                Print VAT registration no. on bills
              </label>
            </Section>
          )}

          {showSave && (
            <div className="flex justify-end">
              <button onClick={save} disabled={saving}
                className="flex items-center gap-2 rounded-lg bg-primary px-6 py-2.5 font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-60">
                <Icon name="save" className="text-base" /> {saving ? 'Saving…' : 'Save Settings'}
              </button>
            </div>
          )}
        </div>
      </div>

      {toast && <div className="fixed bottom-6 left-1/2 -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">{toast}</div>}
    </>
  );
}

function TaxChargesSection() {
  return (
    <Section title="Tax & Service Charges" icon="percent"
      desc="Tax types and the charges built from them now have their own dedicated pages, so a charge can be scoped to specific products (per-product tax) or applied tenant-wide (service charge, levy, ...).">
      <div className="flex flex-wrap gap-3">
        <a href="/tax-types" className="flex items-center gap-2 rounded-lg border border-border bg-card px-4 py-2.5 text-sm font-semibold hover:bg-muted">
          <Icon name="category" className="text-base" /> Manage tax types
        </a>
        <a href="/tax-service-charge" className="flex items-center gap-2 rounded-lg border border-border bg-card px-4 py-2.5 text-sm font-semibold hover:bg-muted">
          <Icon name="percent" className="text-base" /> Manage charges
        </a>
      </div>
    </Section>
  );
}


type Outlet = { id: string; code: string; name: string; locationType: string; currency: string; vatExempt: boolean; canSell: boolean; canProduce: boolean; canStock: boolean; isActive: boolean; operatingHours?: string | null; addressLine1?: string | null; countryCode?: string | null; province?: string | null; district?: string | null; postalCode?: string | null };
const LOC_TYPES = ['outlet', 'central_kitchen', 'warehouse', 'head_office'];
const HOURS_DAYS: { key: string; label: string }[] = [
  { key: 'mon', label: 'Mon' }, { key: 'tue', label: 'Tue' }, { key: 'wed', label: 'Wed' },
  { key: 'thu', label: 'Thu' }, { key: 'fri', label: 'Fri' }, { key: 'sat', label: 'Sat' }, { key: 'sun', label: 'Sun' },
];

function OutletsManager({ flash }: { flash: (m: string) => void }) {
  const [rows, setRows] = useState<Outlet[]>([]);
  const [d, setD] = useState({ code: '', name: '', locationType: '', currency: 'LKR', vatExempt: false, canSell: true, canProduce: false, canStock: true, addressLine1: '', addr: EMPTY_ADDRESS as Address });
  const [hoursFor, setHoursFor] = useState<Outlet | null>(null);
  const [addrFor, setAddrFor] = useState<Outlet | null>(null);

  async function load() { try { setRows(await apiClient<Outlet[]>('/api/v1/locations?all=true')); } catch { /* */ } }
  useEffect(() => { void load(); }, []);

  async function save(body: Partial<Outlet> & { code: string; name: string }) {
    try { await apiClient('/api/v1/locations', { method: 'PUT', body: JSON.stringify(body) }); await load(); flash('Outlet saved.'); }
    catch (e) { flash((e as Error).message.includes('403') ? 'Only an owner/manager can manage outlets.' : (e as Error).message); }
  }
  async function setActive(o: Outlet, isActive: boolean) {
    if (!isActive && !(await confirmDialog({
      title: `Deactivate ${o.name}?`,
      body: 'This outlet will be hidden from selling, production and stock operations. You can reactivate it later.',
      confirmLabel: 'Deactivate',
      danger: true,
    }))) return;
    await save({ ...o, isActive });
  }
  const cls = 'rounded-lg border border-border bg-surface px-2 py-1.5 text-sm';
  return (
    <div className="space-y-3">
      <div className="space-y-1">
        {rows.map(o => (
          <div key={o.id} className="flex flex-wrap items-center gap-2 rounded border border-border px-3 py-2 text-sm">
            <span className="font-mono text-xs text-muted-foreground">{o.code}</span>
            <span className="font-medium">{o.name}</span>
            <select value={o.locationType} onChange={e => save({ ...o, locationType: e.target.value })} className={`${cls} ml-auto`}>
              {LOC_TYPES.map(t => <option key={t} value={t}>{t.replace('_', ' ')}</option>)}
            </select>
            {(['canSell', 'canProduce', 'canStock'] as const).map(k => (
              <label key={k} className="flex items-center gap-1 text-xs text-muted-foreground">
                <input type="checkbox" checked={o[k]} onChange={e => save({ ...o, [k]: e.target.checked })} className="size-3.5 rounded border-border text-primary" />{k.replace('can', '')}
              </label>
            ))}
            <label className="flex items-center gap-1 text-xs text-muted-foreground" title="Tax-exempt outlet (e.g. duty-free) — no tax on its bills; managed here by the owner/admin">
              <input type="checkbox" checked={o.vatExempt} onChange={e => save({ ...o, vatExempt: e.target.checked })} className="size-3.5 rounded border-border text-primary" />Tax-exempt
            </label>
            <button onClick={() => setHoursFor(o)} title="Set operating hours (POS shows a soft 'outside hours' notice)"
              className="flex items-center gap-1 rounded-md border border-border px-2 py-1 text-xs font-semibold text-muted-foreground hover:bg-muted">
              <Icon name="schedule" className="text-[13px]" />{o.operatingHours ? 'Hours ✓' : 'Hours'}
            </button>
            <button onClick={() => setAddrFor(o)} title="Edit this outlet's address"
              className="flex items-center gap-1 rounded-md border border-border px-2 py-1 text-xs font-semibold text-muted-foreground hover:bg-muted">
              <Icon name="location_on" className="text-[13px]" />{o.countryCode || o.district ? 'Address ✓' : 'Address'}
            </button>
            <label className="flex items-center gap-1 text-xs text-muted-foreground">
              <input type="checkbox" checked={o.isActive} onChange={e => setActive(o, e.target.checked)} className="size-3.5 rounded border-border text-primary" />active
            </label>
          </div>
        ))}
      </div>
      {hoursFor && (
        <HoursEditor outlet={hoursFor} onClose={() => setHoursFor(null)}
          onSave={async json => { await save({ ...hoursFor, operatingHours: json }); setHoursFor(null); }} />
      )}
      {addrFor && (
        <OutletAddressEditor outlet={addrFor} onClose={() => setAddrFor(null)}
          onSave={async patch => { await save({ ...addrFor, ...patch }); setAddrFor(null); }} />
      )}
      <div className="flex flex-wrap items-center gap-2 border-t border-border pt-3">
        <input value={d.code} onChange={e => setD(s => ({ ...s, code: e.target.value.toUpperCase() }))} placeholder="code" className={`w-24 font-mono ${cls}`} />
        <input value={d.name} onChange={e => setD(s => ({ ...s, name: e.target.value }))} placeholder="name" className={`flex-1 ${cls}`} />
        <select value={d.locationType} onChange={e => setD(s => ({ ...s, locationType: e.target.value }))} className={cls}>
          <option value="">type…</option>
          {LOC_TYPES.map(t => <option key={t} value={t}>{t.replace('_', ' ')}</option>)}
        </select>
        <label className="flex items-center gap-1 text-xs text-muted-foreground" title="Duty-free / tax-exempt outlet">
          <input type="checkbox" checked={d.vatExempt} onChange={e => setD(s => ({ ...s, vatExempt: e.target.checked }))} className="size-3.5 rounded border-border text-primary" />Tax-exempt
        </label>
        <div className="w-full space-y-2 rounded-lg border border-dashed border-border p-2.5">
          <p className="text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">Address (optional)</p>
          <Field label="Street address" value={d.addressLine1} onChange={v => setD(s => ({ ...s, addressLine1: v }))} />
          <AddressFields value={d.addr} onChange={a => setD(s => ({ ...s, addr: a }))} />
        </div>
        <button onClick={() => {
            if (!d.code.trim() || !d.name.trim()) { flash('Code and name required.'); return; }
            if (!d.locationType) { flash('Choose an outlet type.'); return; }
            save({ code: d.code, name: d.name, locationType: d.locationType, currency: d.currency, vatExempt: d.vatExempt,
              canSell: d.canSell, canProduce: d.canProduce, canStock: d.canStock,
              addressLine1: d.addressLine1.trim() || undefined, countryCode: d.addr.countryCode || undefined,
              province: d.addr.province.trim() || undefined, district: d.addr.district.trim() || undefined, postalCode: d.addr.postalCode.trim() || undefined });
            setD({ code: '', name: '', locationType: '', currency: 'LKR', vatExempt: false, canSell: true, canProduce: false, canStock: true, addressLine1: '', addr: EMPTY_ADDRESS });
          }}
          className="rounded-lg bg-primary px-3 py-1.5 text-sm font-bold text-primary-foreground hover:bg-primary-dark">Add Outlet</button>
      </div>
    </div>
  );
}

// Per-outlet operating-hours editor. Saves a JSON string keyed by weekday (mon..sun);
// a day left "closed" is omitted. The POS reads this and shows a soft "outside hours"
// notice (it never blocks billing). "Clear" removes hours entirely (no notice shown).
function HoursEditor({ outlet, onClose, onSave }: { outlet: Outlet; onClose: () => void; onSave: (json: string) => void | Promise<void> }) {
  type Day = { open: string; close: string } | null;
  const parse = (): Record<string, Day> => {
    const base: Record<string, Day> = {};
    let src: Record<string, Day> = {};
    try { if (outlet.operatingHours) src = JSON.parse(outlet.operatingHours); } catch { /* ignore */ }
    for (const { key } of HOURS_DAYS) {
      const v = src[key];
      base[key] = v && v.open && v.close ? { open: v.open, close: v.close } : null;
    }
    // First-time setup with nothing saved → seed a sensible 09:00–22:00 for every day.
    if (!outlet.operatingHours) for (const { key } of HOURS_DAYS) base[key] = { open: '09:00', close: '22:00' };
    return base;
  };
  const [days, setDays] = useState<Record<string, Day>>(parse);
  const cls = 'rounded-lg border border-border bg-surface px-2 py-1.5 text-sm';
  const setDay = (k: string, v: Day) => setDays(s => ({ ...s, [k]: v }));
  function commit() {
    const out: Record<string, Day> = {};
    for (const { key } of HOURS_DAYS) if (days[key]) out[key] = days[key];
    void onSave(JSON.stringify(out));
  }
  return (
    <div className="fixed inset-0 z-[80] flex items-start justify-center bg-black/60 p-4 pt-[10vh]" onClick={onClose}>
      <div className="w-full max-w-md overflow-hidden rounded-xl bg-card shadow-2xl" onClick={e => e.stopPropagation()}>
        <div className="flex items-center justify-between bg-primary px-4 py-3 text-primary-foreground">
          <h3 className="font-heading text-base font-bold">Operating hours · {outlet.name}</h3>
          <button onClick={onClose} className="rounded-lg p-1 text-primary-foreground/80 hover:bg-white/15"><Icon name="close" /></button>
        </div>
        <div className="space-y-1.5 p-4">
          <p className="mb-2 text-xs text-muted-foreground">POS shows a soft “outside hours” notice — it never blocks billing.</p>
          {HOURS_DAYS.map(({ key, label }) => {
            const d = days[key];
            return (
              <div key={key} className="flex items-center gap-2 text-sm">
                <label className="flex w-20 items-center gap-1.5 font-medium">
                  <input type="checkbox" checked={!!d} className="size-4 rounded border-border text-primary"
                    onChange={e => setDay(key, e.target.checked ? { open: '09:00', close: '22:00' } : null)} />
                  {label}
                </label>
                {d ? (
                  <>
                    <input type="time" value={d.open} onChange={e => setDay(key, { ...d, open: e.target.value })} className={cls} />
                    <span className="text-muted-foreground">–</span>
                    <input type="time" value={d.close} onChange={e => setDay(key, { ...d, close: e.target.value })} className={cls} />
                  </>
                ) : <span className="text-xs text-muted-foreground">Closed</span>}
              </div>
            );
          })}
        </div>
        <div className="flex items-center justify-between gap-2 border-t border-border px-4 py-3">
          <button onClick={() => void onSave('')} className="text-sm font-semibold text-muted-foreground hover:text-on-surface">Clear (always open)</button>
          <div className="flex gap-2">
            <button onClick={onClose} className="rounded-lg border border-border px-3 py-1.5 text-sm font-semibold hover:bg-muted">Cancel</button>
            <button onClick={commit} className="rounded-lg bg-primary px-3 py-1.5 text-sm font-bold text-primary-foreground hover:bg-primary-dark">Save hours</button>
          </div>
        </div>
      </div>
    </div>
  );
}

// Edit an existing outlet's structured address (street + country/region + postal).
function OutletAddressEditor({ outlet, onClose, onSave }: {
  outlet: Outlet;
  onClose: () => void;
  onSave: (patch: { addressLine1: string | null; countryCode: string | null; province: string | null; district: string | null; postalCode: string | null }) => void | Promise<void>;
}) {
  const [street, setStreet] = useState(outlet.addressLine1 ?? '');
  const [addr, setAddr] = useState<Address>({ countryCode: outlet.countryCode ?? '', province: outlet.province ?? '', district: outlet.district ?? '', postalCode: outlet.postalCode ?? '' });
  return (
    <div className="fixed inset-0 z-[80] flex items-start justify-center bg-black/60 p-4 pt-[10vh]" onClick={onClose}>
      <div className="w-full max-w-lg overflow-hidden rounded-xl bg-card shadow-2xl" onClick={e => e.stopPropagation()}>
        <div className="flex items-center justify-between bg-primary px-4 py-3 text-primary-foreground">
          <h3 className="font-heading text-base font-bold">Address · {outlet.name}</h3>
          <button onClick={onClose} className="rounded-lg p-1 text-primary-foreground/80 hover:bg-white/15"><Icon name="close" /></button>
        </div>
        <div className="space-y-3 p-4">
          <Field label="Street address" value={street} onChange={setStreet} placeholder="123 Main St" />
          <AddressFields value={addr} onChange={setAddr} />
        </div>
        <div className="flex justify-end gap-2 border-t border-border px-4 py-3">
          <button onClick={onClose} className="rounded-lg border border-border px-3 py-1.5 text-sm font-semibold hover:bg-muted">Cancel</button>
          <button onClick={() => onSave({ addressLine1: street.trim() || null, countryCode: addr.countryCode || null, province: addr.province.trim() || null, district: addr.district.trim() || null, postalCode: addr.postalCode.trim() || null })}
            className="rounded-lg bg-primary px-3 py-1.5 text-sm font-bold text-primary-foreground hover:bg-primary-dark">Save address</button>
        </div>
      </div>
    </div>
  );
}


// Owner self-serve subscription + add-ons (#109/#111). Catalog-driven; charges via the
// payment seam (manual stub until PayHere #110). Owner-only changes (PUT is Owners-gated).
type SubView = { plan: string; status: string; provider: string; monthlyTotal: number; currency: string; items: { itemType: string; itemCode: string; quantity: number; unitPrice: number }[]; taxes?: { code: string; name: string; ratePercent: number; amount: number }[]; taxTotal?: number; grandTotal?: number; gatewayReady?: boolean; hasPaymentMethod?: boolean; cardBrand?: string | null; cardLast4?: string | null };
type BPlan = { code: string; name: string; monthlyPrice: number; currency: string; includedLocations: number; includedUsers: number; maxLocations: number; features: string[] };
type BAddon = { code: string; name: string; unit: string; unitPrice: number; currency: string };
const flatUnit = (u: string) => u === 'flat_month';
const EXTRA_LOCATION = 'extra_location';

function BillingSection({ flash }: { flash: (m: string) => void }) {
  const [sub, setSub] = useState<SubView | null>(null);
  const [plans, setPlans] = useState<BPlan[]>([]);
  const [addons, setAddons] = useState<BAddon[]>([]);
  const [plan, setPlan] = useState('');
  const [qty, setQty] = useState<Record<string, number>>({});
  const [outletCount, setOutletCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [cardBusy, setCardBusy] = useState(false);
  const [showBreakdown, setShowBreakdown] = useState(false);

  async function load() {
    try {
      const [s, c, locs] = await Promise.all([
        apiClient<SubView>('/api/v1/billing/subscription'),
        apiClient<{ plans: BPlan[]; addons: BAddon[] }>('/api/v1/billing/catalog'),
        apiClient<{ isActive?: boolean; locationType?: string }[]>('/api/v1/locations?all=true').catch(() => [] as { isActive?: boolean; locationType?: string }[]),
      ]);
      setSub(s); setPlans(c.plans); setAddons(c.addons); setPlan(s.plan);
      // Only SALES outlets are billed/capped. Head office, central kitchen & warehouses
      // are free supporting locations — they never count toward the outlet charge or cap.
      setOutletCount((locs || []).filter(l => l.isActive !== false && l.locationType === 'outlet').length);
      const q: Record<string, number> = {};
      for (const it of s.items) if (it.itemType === 'addon') q[it.itemCode] = it.quantity;
      setQty(q);
    } catch (e) { flash((e as Error).message); } finally { setLoading(false); }
  }
  useEffect(() => { void load(); }, []);

  // Returned from a PayHere card-capture redirect (?billing=card_added|card_cancelled).
  useEffect(() => {
    const p = new URLSearchParams(window.location.search).get('billing');
    if (p === 'card_added') flash('Card saved — recurring charges will use it.');
    else if (p === 'card_cancelled') flash('Card setup was cancelled.');
    if (p) window.history.replaceState({}, '', window.location.pathname);
  }, []);

  // Add/replace the saved card via PayHere's on-domain JS SDK popup (preapproval = tokenize, no charge).
  // The server signs the request; the card is entered in PayHere's iframe; the token arrives at our
  // notify webhook server-side. onCompleted just means the dialog finished — we reload to show the card.
  async function addCard() {
    setCardBusy(true);
    try {
      const payhere = await loadPayHere();
      const r = await apiClient<{ fields: Record<string, string>; sandbox: boolean }>(
        '/api/v1/billing/payhere/preapproval', { method: 'POST', body: JSON.stringify({}) });
      payhere.onCompleted = () => { flash('Card submitted — saving…'); setTimeout(() => { setCardBusy(false); void load(); }, 2500); };
      payhere.onDismissed = () => setCardBusy(false);
      payhere.onError = (err: string) => { flash(`Card setup failed: ${err}`); setCardBusy(false); };
      payhere.startPayment({ ...r.fields, amount: Number(r.fields.amount), sandbox: r.sandbox, preapprove: true });
    } catch (e) { flash((e as Error).message); setCardBusy(false); }
  }

  // Outlets you operate must be paid for: included (covered by the base plan) + the rest as add-on outlets.
  const includedNow = plans.find(p => p.code === plan)?.includedLocations ?? 0;
  const requiredExtra = Math.max(0, outletCount - includedNow);
  // Never let the additional-outlet count sit below what you actually run.
  useEffect(() => {
    if (!sub) return;
    setQty(s => ((s[EXTRA_LOCATION] ?? 0) < requiredExtra ? { ...s, [EXTRA_LOCATION]: requiredExtra } : s));
  }, [requiredExtra, sub]);

  const newTotal = (() => { const p = plans.find(x => x.code === plan); let t = p?.monthlyPrice ?? 0; for (const a of addons) { const n = qty[a.code] ?? 0; if (n <= 0) continue; t += flatUnit(a.unit) ? a.unitPrice : a.unitPrice * n; } return t; })();
  // Apply the same RIT billing-tax rates the server computed for this tenant's country.
  const taxRate = (sub?.taxes ?? []).reduce((s, t) => s + t.ratePercent, 0);
  const newTax = Math.round(newTotal * taxRate) / 100;
  const newGrand = newTotal + newTax;
  const changed = sub && (plan !== sub.plan || Math.round(newTotal) !== Math.round(sub.monthlyTotal));

  async function save() {
    setSaving(true);
    try {
      const clean: Record<string, number> = {};
      for (const a of addons) { const n = qty[a.code] ?? 0; if (n > 0) clean[a.code] = flatUnit(a.unit) ? 1 : n; }
      await apiClient('/api/v1/billing/subscription', { method: 'PUT', body: JSON.stringify({ plan, addons: clean }) });
      await load();
      // The plan drives feature locks across the whole shell (sidebar nav, POS, e-receipts, add-ons).
      // Those gates are fetched on mount, so reload once after a successful change to re-read the
      // new entitlement immediately — otherwise locks/unlocks only appear after a manual refresh.
      flash('Plan updated — applying changes…');
      setTimeout(() => window.location.reload(), 800);
      return;
    } catch (e) {
      const m = (e as Error).message;
      if (m.includes('NO_PAYMENT_METHOD')) flash('Add a card first — this change needs a payment method on file.');
      else if (m.includes('403')) flash('Only the owner can change the plan.');
      else flash(m);
    } finally { setSaving(false); }
  }

  if (loading) return <Section title="Subscription & billing" icon="credit_card" desc="Your plan, add-ons and licenses."><p className="text-sm text-muted-foreground">Loading…</p></Section>;

  const cls = 'rounded-lg border border-border bg-surface px-2 py-1.5 text-sm';
  return (
    <Section title="Subscription & billing" icon="credit_card" desc="Self-serve — change your plan or add-ons any time. Billed monthly; a 14-day trial applies to new workspaces.">
      <div className="mb-5 flex flex-wrap items-center gap-3 rounded-lg border border-border bg-muted/30 p-4">
        <div>
          <div className="text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">Current</div>
          <div className="font-heading text-lg font-bold">{plans.find(p => p.code === sub?.plan)?.name ?? sub?.plan}</div>
        </div>
        <span className={`pill ${sub?.status === 'trialing' ? 'pill-pending' : sub?.status === 'active' ? 'pill-paid' : 'pill-idle'}`}>{sub?.status}</span>
        <div className="ml-auto text-right">
          <div className="text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">Monthly</div>
          <div className="font-heading text-lg font-black text-primary">{sub?.currency} {lkr(sub?.grandTotal ?? sub?.monthlyTotal ?? 0)}</div>
          {(sub?.taxTotal ?? 0) > 0 && <div className="text-[11px] text-muted-foreground">incl. {sub?.currency} {lkr(sub?.taxTotal ?? 0)} {sub?.taxes?.map(t => t.name).join(' + ')}</div>}
        </div>
      </div>

      <div className="mb-5 flex flex-wrap items-center gap-3 rounded-lg border border-border p-4">
        <Icon name="credit_card" className="text-2xl text-primary" />
        <div className="min-w-0">
          <div className="text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">Payment method</div>
          {sub?.hasPaymentMethod
            ? <div className="font-medium">{sub.cardBrand || 'Card'} ···· {sub.cardLast4}</div>
            : <div className="text-sm text-muted-foreground">No card on file — add one for automatic renewal after your trial.</div>}
          {!sub?.gatewayReady && <div className="mt-0.5 text-xs text-muted-foreground">Saved cards activate once PayHere is connected.</div>}
        </div>
        <button type="button" onClick={addCard} disabled={cardBusy || !sub?.gatewayReady} title={sub?.gatewayReady ? '' : 'Connect PayHere to add a card'}
          className="ml-auto rounded-lg border-2 border-primary px-4 py-2 text-sm font-bold text-primary hover:bg-primary hover:text-white disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:bg-transparent disabled:hover:text-primary">
          {cardBusy ? 'Redirecting…' : sub?.hasPaymentMethod ? 'Update card' : 'Add payment method'}
        </button>
      </div>

      <label className="mb-2 block text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">Plan</label>
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
        {plans.map(p => (
          <button key={p.code} type="button" onClick={() => setPlan(p.code)}
            className={`rounded-xl border p-4 text-left transition-all ${plan === p.code ? 'border-2 border-primary bg-primary-tint' : 'border-border hover:border-primary'}`}>
            <div className="font-heading font-bold">{p.name}</div>
            <div className="mt-1 font-heading text-xl font-black text-primary">{p.currency} {p.monthlyPrice.toLocaleString()}<span className="text-xs font-normal text-muted-foreground">/mo</span></div>
            <div className="mt-1 text-xs text-muted-foreground">{p.includedLocations} outlet{p.includedLocations === 1 ? '' : 's'} included{p.maxLocations > 0 ? ` · up to ${p.maxLocations}` : ' · unlimited'} · {p.includedUsers} users</div>
            <ul className="mt-2 space-y-1">{p.features.map(f => <li key={f} className="flex items-center gap-1.5 text-xs text-muted-foreground"><Icon name="check" className="text-[13px] text-primary" />{f}</li>)}</ul>
          </button>
        ))}
      </div>

      {addons.length > 0 && (
        <>
          <label className="mb-2 mt-6 block text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">Add-ons</label>
          <div className="space-y-2">
            {addons.map(a => {
              // E-receipt tiers are mutually exclusive: the all-channels add-on includes email,
              // so hide the email-only option whenever the all-channels one is enabled (#79).
              if (a.code === 'ereceipt_email' && (qty['ereceipt_all'] ?? 0) > 0) return null;
              const n = qty[a.code] ?? 0; const flat = flatUnit(a.unit);
              return (
                <div key={a.code} className="flex flex-wrap items-center gap-3 rounded-lg border border-border px-3 py-2 text-sm">
                  <div className="min-w-0 flex-1">
                    <div className="font-medium">{a.name}</div>
                    <div className="text-xs text-muted-foreground">{a.currency} {a.unitPrice.toLocaleString()}{flat ? '/month' : a.unit === 'per_device_month' ? '/device/mo' : '/outlet/mo'}</div>
                    {a.code === EXTRA_LOCATION && <div className="mt-0.5 text-xs text-primary">You operate {outletCount} sales outlet{outletCount === 1 ? '' : 's'} · {includedNow} included → {requiredExtra} billed{requiredExtra > 0 ? '' : ' (all covered)'} <span className="text-muted-foreground">· head office & kitchens are free</span></div>}
                    {a.code === 'tab_device' && (
                      <div className="mt-0.5 text-xs text-muted-foreground">
                        One seat per waiter handheld. <a href="/tab-devices" className="font-semibold text-primary hover:underline">Manage devices →</a>
                      </div>
                    )}
                    {a.code === 'guest_qr' && <div className="mt-0.5 text-xs text-muted-foreground">Lets guests scan a per-table QR and order from their own phones.</div>}
                    {a.code === 'ereceipt_email' && <div className="mt-0.5 text-xs text-muted-foreground">Email a clean receipt to the guest from the till. Includes a monthly email allowance.</div>}
                    {a.code === 'ereceipt_all' && <div className="mt-0.5 text-xs text-muted-foreground">Send receipts by email, SMS or WhatsApp. Includes a monthly message allowance (WhatsApp coming soon).</div>}
                  </div>
                  {flat ? (
                    <label className="flex items-center gap-1.5 text-xs"><input type="checkbox" checked={n > 0} onChange={e => setQty(s => {
                      const next = { ...s, [a.code]: e.target.checked ? 1 : 0 };
                      // Turning on the all-channels tier supersedes (and clears) the email-only tier — never bill both.
                      if (a.code === 'ereceipt_all' && e.target.checked) next['ereceipt_email'] = 0;
                      return next;
                    })} className="size-4 rounded border-border text-primary" />Enabled</label>
                  ) : (
                    <div className="flex items-center gap-1.5">
                      <button type="button" onClick={() => setQty(s => ({ ...s, [a.code]: Math.max(a.code === EXTRA_LOCATION ? requiredExtra : 0, n - 1) }))} className="size-7 rounded border border-border font-bold hover:bg-muted">−</button>
                      <span className="w-7 text-center font-bold">{n}</span>
                      <button type="button" onClick={() => setQty(s => ({ ...s, [a.code]: n + 1 }))} className="size-7 rounded border border-border font-bold hover:bg-muted">+</button>
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </>
      )}

      {/* Line-item breakdown — collapsible, collapsed by default to keep the screen clean */}
      <div className="mt-6 rounded-lg border border-border bg-muted/30 text-sm">
        <button type="button" onClick={() => setShowBreakdown(v => !v)}
          className="flex w-full items-center justify-between px-3 py-2.5 transition-colors hover:bg-muted/40">
          <span className="flex items-center gap-1 text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">
            <Icon name={showBreakdown ? 'expand_less' : 'expand_more'} className="text-base" /> Breakdown
          </span>
          <span className="font-heading text-sm font-bold text-on-surface">{sub?.currency} {lkr(newTotal)}</span>
        </button>
        {showBreakdown && (
          <div className="space-y-2.5 border-t border-border px-3 pb-3 pt-3">
            {(() => { const p = plans.find(x => x.code === plan); return (
              <div className="flex justify-between gap-4"><span>{p?.name ?? plan} plan</span><span className="whitespace-nowrap">{sub?.currency} {lkr(p?.monthlyPrice ?? 0)}</span></div>
            ); })()}
            {addons.map(a => { const n = qty[a.code] ?? 0; if (n <= 0) return null;
              const flat = flatUnit(a.unit); const amt = flat ? a.unitPrice : a.unitPrice * n;
              return <div key={a.code} className="flex justify-between gap-4"><span>{flat ? a.name : `${a.name} × ${n}`}</span><span className="whitespace-nowrap">{sub?.currency} {lkr(amt)}</span></div>; })}
            <div className="flex justify-between gap-4 border-t border-border pt-2.5 font-semibold"><span>Subtotal</span><span className="whitespace-nowrap">{sub?.currency} {lkr(newTotal)}</span></div>
            {(sub?.taxes ?? []).map(t => <div key={t.code} className="flex justify-between gap-4 text-muted-foreground"><span>{t.name} ({t.ratePercent}%)</span><span className="whitespace-nowrap">{sub?.currency} {lkr(Math.round(newTotal * t.ratePercent) / 100)}</span></div>)}
          </div>
        )}
      </div>

      <div className="mt-4 flex flex-wrap items-center justify-between gap-3 border-t border-border pt-4">
        <div>
          <span className="text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">New monthly total</span>
          <div className="font-heading text-xl font-black text-primary">{sub?.currency} {lkr(newGrand)}</div>
          {newTax > 0 && <div className="text-[11px] text-muted-foreground">{sub?.currency} {lkr(newTotal)} + {sub?.currency} {lkr(newTax)} {sub?.taxes?.map(t => t.name).join(' + ')}</div>}
        </div>
        <button onClick={save} disabled={saving || !changed}
          className="rounded-lg bg-primary px-5 py-2.5 font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">
          {saving ? 'Updating…' : changed ? 'Update subscription' : 'No changes'}
        </button>
      </div>
      <p className="mt-3 text-xs text-muted-foreground">Changes apply immediately and re-scope your limits. {sub?.provider === 'manual' ? 'Card billing runs in test mode until PayHere is connected.' : ''} Only the owner can change the plan.</p>
    </Section>
  );
}

type Tier = { id?: string; name: string; minLifetimePoints: number; earnMultiplier: number; discountPercent: number; isActive: boolean };

const EMPTY_TIER: Tier = { name: '', minLifetimePoints: 0, earnMultiplier: 1, discountPercent: 0, isActive: true };

function LoyaltyTiers({ flash }: { flash: (m: string) => void }) {
  const [tiers, setTiers] = useState<Tier[]>([]);
  const [draft, setDraft] = useState<Tier>(EMPTY_TIER);

  async function load() { try { setTiers(await apiClient<Tier[]>('/api/v1/loyalty/tiers')); } catch { /* */ } }
  useEffect(() => { void load(); }, []);

  function reset() { setDraft(EMPTY_TIER); }
  function startEdit(t: Tier) { setDraft({ ...t }); }

  async function save() {
    if (!draft.name.trim()) { flash('Tier name is required.'); return; }
    // PUT with an id edits; without one it creates.
    try { await apiClient('/api/v1/loyalty/tiers', { method: 'PUT', body: JSON.stringify(draft) }); reset(); await load(); flash('Tier saved.'); }
    catch (e) { flash((e as Error).message); }
  }
  async function remove(id?: string) {
    if (!id) return;
    if (!(await confirmDialog({
      title: 'Delete this loyalty tier?',
      body: 'Customers will no longer reach this tier. This cannot be undone.',
      confirmLabel: 'Delete',
      danger: true,
    }))) return;
    try { await apiClient(`/api/v1/loyalty/tiers/${id}`, { method: 'DELETE' }); if (draft.id === id) reset(); await load(); } catch (e) { flash((e as Error).message); }
  }

  const cls = 'rounded-lg border border-border bg-surface px-2 py-1.5 text-sm';
  return (
    <div className="mt-4 border-t border-border pt-4">
      <div className="mb-2 text-sm font-semibold text-slate-700">Tiers <span className="font-normal text-muted-foreground">— reached by lifetime points; grant an earn multiplier + optional discount</span></div>
      {tiers.length > 0 && (
        <div className="mb-2 space-y-1">
          {tiers.map(t => (
            <div key={t.id} className="flex items-center justify-between rounded border border-border px-3 py-2 text-sm">
              <span><b>{t.name}</b> · ≥ {t.minLifetimePoints} pts · ×{t.earnMultiplier} earn{t.discountPercent > 0 ? ` · ${t.discountPercent}% off` : ''}</span>
              <span className="flex shrink-0 items-center gap-0.5">
                <button onClick={() => startEdit(t)} title="Edit" className="rounded-lg p-1 text-muted-foreground hover:bg-muted hover:text-foreground"><Icon name="edit" className="text-base" /></button>
                <button onClick={() => remove(t.id)} title="Delete" className="rounded-lg p-1 text-muted-foreground hover:bg-muted hover:text-error"><Icon name="close" className="text-base" /></button>
              </span>
            </div>
          ))}
        </div>
      )}
      <div className="rounded-lg border border-border bg-muted/30 p-3">
        <div className="mb-2 text-xs font-semibold text-slate-700">{draft.id ? 'Edit tier' : 'New tier'}</div>
        <div className="flex flex-wrap items-center gap-2">
          <input value={draft.name} onChange={e => setDraft(d => ({ ...d, name: e.target.value }))} placeholder="tier name" className={`flex-1 ${cls}`} />
          <input type="number" value={draft.minLifetimePoints} onChange={e => setDraft(d => ({ ...d, minLifetimePoints: Number(e.target.value) || 0 }))} placeholder="min pts" className={`w-24 text-right ${cls}`} />
          <input type="number" step="0.1" value={draft.earnMultiplier} onChange={e => setDraft(d => ({ ...d, earnMultiplier: Number(e.target.value) || 1 }))} placeholder="×earn" className={`w-20 text-right ${cls}`} />
          <input type="number" value={draft.discountPercent} onChange={e => setDraft(d => ({ ...d, discountPercent: Number(e.target.value) || 0 }))} placeholder="% off" className={`w-20 text-right ${cls}`} />
          {draft.id && <button onClick={reset} className="rounded-lg border border-border px-3 py-1.5 text-sm font-semibold hover:bg-muted">Cancel</button>}
          <button onClick={save} className="rounded-lg bg-primary px-3 py-1.5 text-sm font-bold text-primary-foreground hover:bg-primary-dark">{draft.id ? 'Save' : 'Add'}</button>
        </div>
      </div>
    </div>
  );
}



function Section({ title, icon, desc, children }: { title: string; icon: string; desc: string; children: React.ReactNode }) {
  return (
    <div className="card p-6">
      <div className="mb-1 flex items-center gap-2">
        <Icon name={icon} className="text-primary" />
        <h2 className="font-heading text-lg font-bold">{title}</h2>
      </div>
      <p className="mb-4 text-sm text-muted-foreground">{desc}</p>
      <div className="space-y-3">{children}</div>
    </div>
  );
}
const Grid = ({ children }: { children: React.ReactNode }) => <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">{children}</div>;
const cls = 'w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20';
