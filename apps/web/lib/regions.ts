// Countries + region pick-lists for the shared <AddressFields> component.
// Smart inputs: Sri Lanka → district + auto-province dropdowns; UAE → emirate
// dropdown; every other country → free text. Postal code is always free text.

export type Address = { countryCode: string; province: string; district: string; postalCode: string };
export const EMPTY_ADDRESS: Address = { countryCode: '', province: '', district: '', postalCode: '' };

export type Opt = { value: string; label: string };

// Country dropdown, sorted alphabetically by country name. value = ISO-3166 alpha-2.
export const COUNTRIES: Opt[] = [
  ['LK', '🇱🇰 Sri Lanka'], ['AE', '🇦🇪 United Arab Emirates'], ['IN', '🇮🇳 India'],
  ['MV', '🇲🇻 Maldives'], ['PK', '🇵🇰 Pakistan'], ['BD', '🇧🇩 Bangladesh'], ['NP', '🇳🇵 Nepal'],
  ['QA', '🇶🇦 Qatar'], ['SA', '🇸🇦 Saudi Arabia'], ['KW', '🇰🇼 Kuwait'], ['OM', '🇴🇲 Oman'], ['BH', '🇧🇭 Bahrain'],
  ['SG', '🇸🇬 Singapore'], ['MY', '🇲🇾 Malaysia'], ['TH', '🇹🇭 Thailand'], ['ID', '🇮🇩 Indonesia'], ['PH', '🇵🇭 Philippines'],
  ['GB', '🇬🇧 United Kingdom'], ['US', '🇺🇸 United States'], ['CA', '🇨🇦 Canada'],
  ['AU', '🇦🇺 Australia'], ['NZ', '🇳🇿 New Zealand'], ['ZA', '🇿🇦 South Africa'],
  ['DE', '🇩🇪 Germany'], ['FR', '🇫🇷 France'], ['NL', '🇳🇱 Netherlands'], ['IE', '🇮🇪 Ireland'],
]
  .map(([value, label]) => ({ value, label }))
  .sort((a, b) => a.label.replace(/^\S+\s/, '').localeCompare(b.label.replace(/^\S+\s/, '')));

// Common IANA time zones for the markets above, sorted alphabetically.
export const TIME_ZONES: string[] = [
  'Asia/Colombo', 'Asia/Dubai', 'Asia/Kolkata', 'Asia/Male', 'Asia/Karachi', 'Asia/Dhaka', 'Asia/Kathmandu',
  'Asia/Qatar', 'Asia/Riyadh', 'Asia/Kuwait', 'Asia/Muscat', 'Asia/Bahrain',
  'Asia/Singapore', 'Asia/Kuala_Lumpur', 'Asia/Bangkok', 'Asia/Jakarta', 'Asia/Manila',
  'Europe/London', 'America/New_York', 'America/Los_Angeles', 'America/Chicago', 'America/Denver', 'America/Toronto',
  'Australia/Sydney', 'Australia/Melbourne', 'Australia/Perth', 'Pacific/Auckland', 'Africa/Johannesburg',
  'Europe/Berlin', 'Europe/Paris', 'Europe/Amsterdam', 'Europe/Dublin', 'UTC',
].sort();

// Sri Lanka — 25 districts grouped by their province (selecting a district auto-fills the province).
const LK_DISTRICTS_BY_PROVINCE: Record<string, string[]> = {
  'Western': ['Colombo', 'Gampaha', 'Kalutara'],
  'Central': ['Kandy', 'Matale', 'Nuwara Eliya'],
  'Southern': ['Galle', 'Matara', 'Hambantota'],
  'Northern': ['Jaffna', 'Kilinochchi', 'Mannar', 'Vavuniya', 'Mullaitivu'],
  'Eastern': ['Batticaloa', 'Ampara', 'Trincomalee'],
  'North Western': ['Kurunegala', 'Puttalam'],
  'North Central': ['Anuradhapura', 'Polonnaruwa'],
  'Uva': ['Badulla', 'Monaragala'],
  'Sabaragamuwa': ['Ratnapura', 'Kegalle'],
};
export const LK_PROVINCES: string[] = Object.keys(LK_DISTRICTS_BY_PROVINCE);
export const LK_DISTRICTS: string[] = Object.values(LK_DISTRICTS_BY_PROVINCE).flat().sort();
export const LK_DISTRICT_TO_PROVINCE: Record<string, string> = Object.fromEntries(
  Object.entries(LK_DISTRICTS_BY_PROVINCE).flatMap(([prov, ds]) => ds.map(d => [d, prov])),
);

// UAE — the 7 emirates serve as the province/state level.
export const AE_EMIRATES: string[] = ['Abu Dhabi', 'Dubai', 'Sharjah', 'Ajman', 'Umm Al Quwain', 'Ras Al Khaimah', 'Fujairah'];

type RegionConfig = {
  provinceLabel: string;
  districtLabel: string;
  provinceOptions: string[] | null;   // null → free-text field
  districtOptions: string[] | null;
  districtToProvince: Record<string, string> | null;
};

// Drives the field labels + whether each region field is a dropdown or free text.
export function regionConfig(countryCode: string): RegionConfig {
  switch (countryCode) {
    case 'LK':
      return { provinceLabel: 'Province', districtLabel: 'District',
        provinceOptions: LK_PROVINCES, districtOptions: LK_DISTRICTS, districtToProvince: LK_DISTRICT_TO_PROVINCE };
    case 'AE':
      return { provinceLabel: 'Emirate', districtLabel: 'City / Area',
        provinceOptions: AE_EMIRATES, districtOptions: null, districtToProvince: null };
    default:
      return { provinceLabel: 'State / Province', districtLabel: 'City / District',
        provinceOptions: null, districtOptions: null, districtToProvince: null };
  }
}

export const countryName = (code: string | null | undefined): string =>
  COUNTRIES.find(c => c.value === code)?.label.replace(/^\S+\s/, '') ?? code ?? '';
