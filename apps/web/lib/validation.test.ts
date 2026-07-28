import { describe, it, expect } from 'vitest';
import { required, email, slug, phoneLK, positiveNumber, vatNumber, validate } from './validation';

describe('required', () => {
  it('fails on empty/whitespace, passes on value', () => {
    expect(required('Name')('')).toMatch(/required/);
    expect(required('Name')('   ')).toMatch(/required/);
    expect(required('Name')('Asela')).toBeNull();
  });
});

describe('email', () => {
  it.each(['a@b.com', 'owner@demo.local', 'x.y@sub.domain.lk'])('accepts %s', (v) =>
    expect(email(v)).toBeNull());
  it.each(['', 'nope', 'a@b', 'a b@c.com'])('rejects %s', (v) =>
    expect(email(v)).not.toBeNull());
});

describe('slug', () => {
  it.each(['demo', 'spice-garden', 'a1b2'])('accepts %s', (v) =>
    expect(slug(v)).toBeNull());
  it.each(['', 'ab', 'BadSlug', 'has space', '-leading', 'trailing-', 'a'])('rejects %s', (v) =>
    expect(slug(v)).not.toBeNull());
});

describe('phoneLK', () => {
  it('is optional (empty passes)', () => expect(phoneLK('')).toBeNull());
  it.each(['0771234567', '+94771234567', '0112345678'])('accepts %s', (v) =>
    expect(phoneLK(v)).toBeNull());
  it.each(['123', '07712'])('rejects %s', (v) => expect(phoneLK(v)).not.toBeNull());
});

describe('positiveNumber', () => {
  it('accepts > 0', () => expect(positiveNumber('Price')('100')).toBeNull());
  it('rejects 0, negatives, non-numbers', () => {
    expect(positiveNumber('Price')('0')).not.toBeNull();
    expect(positiveNumber('Price')('-5')).not.toBeNull();
    expect(positiveNumber('Price')('abc')).not.toBeNull();
  });
});

describe('vatNumber', () => {
  it('optional + format', () => {
    expect(vatNumber('')).toBeNull();
    expect(vatNumber('134567890-7000')).toBeNull();
    expect(vatNumber('1234567897000')).toBeNull();
    expect(vatNumber('bad')).not.toBeNull();
  });
});

describe('validate()', () => {
  it('returns only failing fields, first error per field', () => {
    const errs = validate(
      { workspace: 'BAD', email: 'nope' },
      { workspace: [slug], email: [email] },
    );
    expect(Object.keys(errs)).toEqual(['workspace', 'email']);
  });
  it('returns empty object when all valid', () => {
    const errs = validate({ workspace: 'demo', email: 'a@b.com' }, { workspace: [slug], email: [email] });
    expect(errs).toEqual({});
  });
});
