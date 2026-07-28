'use client';

import { Field, Combobox } from '@/components/ui/form';
import { type Address, COUNTRIES, regionConfig } from '@/lib/regions';

const toOpts = (xs: string[]) => xs.map(x => ({ value: x, label: x }));

// Shared structured-address block: Country (dropdown) + Province/State/Emirate +
// District/City + Postal code. For Sri Lanka the province/district are dropdowns
// (and picking a district auto-fills its province); for the UAE the emirate is a
// dropdown; every other country falls back to free-text. Pair it with the form's
// existing free-text street line — this component owns only the structured fields.
export function AddressFields({ value, onChange, className, required }: {
  value: Address;
  onChange: (a: Address) => void;
  className?: string;
  required?: boolean;   // marks Country + District/City with "*" (used where an address is mandatory, e.g. off-site catering)
}) {
  const cfg = regionConfig(value.countryCode);
  const set = (patch: Partial<Address>) => onChange({ ...value, ...patch });
  const req = required ? ' *' : '';

  return (
    <div className={`grid grid-cols-1 gap-3 sm:grid-cols-2 ${className ?? ''}`}>
      <Combobox label={`Country${req}`} value={value.countryCode} options={COUNTRIES}
        placeholder="Select country…" searchPlaceholder="Search country…"
        onChange={code => set({ countryCode: code, province: '', district: '' })} />

      {cfg.provinceOptions
        ? <Combobox label={cfg.provinceLabel} value={value.province} options={toOpts(cfg.provinceOptions)}
            placeholder={`Select ${cfg.provinceLabel.toLowerCase()}…`} onChange={p => set({ province: p })} />
        : <Field label={cfg.provinceLabel} value={value.province} onChange={p => set({ province: p })} />}

      {cfg.districtOptions
        ? <Combobox label={`${cfg.districtLabel}${req}`} value={value.district} options={toOpts(cfg.districtOptions)}
            placeholder={`Select ${cfg.districtLabel.toLowerCase()}…`}
            onChange={d => set({ district: d, province: cfg.districtToProvince?.[d] ?? value.province })} />
        : <Field label={`${cfg.districtLabel}${req}`} value={value.district} onChange={d => set({ district: d })} />}

      <Field label="Postal code" value={value.postalCode} onChange={pc => set({ postalCode: pc })} placeholder="e.g. 00100" />
    </div>
  );
}
