import { describe, it, expect } from 'vitest';
import { lkr } from './api-client';

describe('lkr', () => {
  it('formats zero with two decimals', () => {
    expect(lkr(0)).toBe('0.00');
  });

  it('adds thousand separators', () => {
    expect(lkr(1000)).toBe('1,000.00');
  });

  it('formats large values with grouping and trailing zero', () => {
    expect(lkr(1234567.5)).toBe('1,234,567.50');
  });

  it('preserves an already two-decimal value', () => {
    expect(lkr(11974.05)).toBe('11,974.05');
  });

  it('formats negative values with a leading minus', () => {
    expect(lkr(-1234.5)).toBe('-1,234.50');
  });

  it('rounds to two decimal places', () => {
    // Use values that are exactly representable / unambiguous in IEEE-754,
    // avoiding float half-way traps (e.g. 2.675 is really 2.67499…).
    expect(lkr(1.006)).toBe('1.01');
    expect(lkr(1.004)).toBe('1.00');
    expect(lkr(2.349)).toBe('2.35');
    expect(lkr(-1.006)).toBe('-1.01');
  });

  it('pads single-decimal values to two places', () => {
    expect(lkr(0.5)).toBe('0.50');
    expect(lkr(99.9)).toBe('99.90');
  });
});
