/**
 * Lightweight, dependency-free validation used across all forms.
 * Each validator returns an error string or null (valid).
 */
export type Validator = (value: string) => string | null;

export const required =
  (label = 'This field'): Validator =>
  (v) => (v.trim() ? null : `${label} is required.`);

export const email: Validator = (v) =>
  !v.trim() ? 'Email is required.'
  : /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v.trim()) ? null
  : 'Enter a valid email address.';

export const slug: Validator = (v) =>
  !v.trim() ? 'Workspace is required.'
  : /^[a-z0-9][a-z0-9-]{1,58}[a-z0-9]$/.test(v.trim())
    ? null
    : 'Use 3–60 lowercase letters, numbers, or hyphens.';

export const minLen =
  (n: number, label = 'This field'): Validator =>
  (v) => (v.trim().length >= n ? null : `${label} must be at least ${n} characters.`);

export const phoneLK: Validator = (v) =>
  !v.trim() ? null // optional
  : /^(\+94|0)?[1-9][0-9]{8}$/.test(v.replace(/\s/g, '')) ? null
  : 'Enter a valid Sri Lankan phone number.';

export const positiveNumber =
  (label = 'Value'): Validator =>
  (v) => {
    const n = Number(v);
    if (v.trim() === '' || Number.isNaN(n)) return `${label} must be a number.`;
    return n > 0 ? null : `${label} must be greater than zero.`;
  };

export const vatNumber: Validator = (v) =>
  !v.trim() ? null // optional
  : /^[0-9]{9}-?[0-9]{4}$/.test(v.trim()) ? null
  : 'VAT number format: 123456789-7000.';

/**
 * Run a set of field→validators; returns a map of field→error (only failing
 * fields). `values` may carry non-string fields (e.g. booleans) — only the
 * fields named in `rules` are read, and each is coerced to a string for the
 * (string) validators.
 */
export function validate(
  values: Record<string, unknown>,
  rules: Record<string, Validator[]>,
): Record<string, string> {
  const errors: Record<string, string> = {};
  for (const [field, validators] of Object.entries(rules)) {
    const raw = values[field];
    const str = typeof raw === 'string' ? raw : raw == null ? '' : String(raw);
    for (const fn of validators) {
      const err = fn(str);
      if (err) { errors[field] = err; break; }
    }
  }
  return errors;
}
