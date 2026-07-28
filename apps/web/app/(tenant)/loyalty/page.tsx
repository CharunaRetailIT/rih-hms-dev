'use client';

// NOTE: Loyalty backend is deferred (v1 scope). This screen uses MOCK data and is
// NOT wired to a live API yet. Enrollment + redemption are local-state only.

import { useState } from 'react';
import { Topbar } from '@/components/app-shell/Topbar';
import { Icon } from '@/components/ui/Icon';
import { Field, Combobox } from '@/components/ui/form';
import { lkr } from '@/lib/api-client';
import { validate, required, email as emailRule, phoneLK, positiveNumber } from '@/lib/validation';

type Tab = 'enrollment' | 'redemption';

// ---- Mock redemption data (loyalty backend deferred) ----
const MOCK_POINTS = 4500;
const POINTS_PER_LKR = 10; // 10 points = 1 LKR
const MOCK_MAX_REDEEM = MOCK_POINTS / POINTS_PER_LKR; // 450.00 LKR
const MOCK_ORDER_TOTAL = 5966.5;

export default function LoyaltyPage() {
  const [tab, setTab] = useState<Tab>('enrollment');

  return (
    <>
      <Topbar title="Loyalty" subtitle="Turn first-time guests into regulars who keep coming back" />
      <div className="p-6 md:p-8">
        {/* Tab switcher */}
        <div className="mb-6 inline-flex rounded-xl border border-border bg-card p-1">
          <button
            onClick={() => setTab('enrollment')}
            className={`flex items-center gap-2 rounded-lg px-5 py-2 text-sm font-bold transition-all ${
              tab === 'enrollment'
                ? 'bg-primary text-white shadow-sm'
                : 'text-muted-foreground hover:bg-muted'
            }`}
          >
            <Icon name="how_to_reg" className="text-lg" />
            Enrollment
          </button>
          <button
            onClick={() => setTab('redemption')}
            className={`flex items-center gap-2 rounded-lg px-5 py-2 text-sm font-bold transition-all ${
              tab === 'redemption'
                ? 'bg-primary text-white shadow-sm'
                : 'text-muted-foreground hover:bg-muted'
            }`}
          >
            <Icon name="redeem" className="text-lg" />
            Redemption
          </button>
        </div>

        {tab === 'enrollment' ? <EnrollmentTab /> : <RedemptionTab />}
      </div>
    </>
  );
}

// =================================================================
// ENROLLMENT
// =================================================================
function EnrollmentTab() {
  const [digits, setDigits] = useState('');
  const [eligible, setEligible] = useState(false);
  const [checking, setChecking] = useState(false);
  const [fullName, setFullName] = useState('');
  const [emailVal, setEmailVal] = useState('');
  const [birthDate, setBirthDate] = useState('');
  const [language, setLanguage] = useState('English');
  const [marketing, setMarketing] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [enrolled, setEnrolled] = useState(false);

  const display = digits.match(/.{1,3}/g)?.join(' ') ?? '';

  function appendNum(n: string) {
    if (digits.length < 9) setDigits((d) => d + n);
    setEnrolled(false);
  }
  function backspace() {
    setDigits((d) => d.slice(0, -1));
  }

  function checkEligibility() {
    setErrors({});
    const errs = validate(
      { mobile: digits },
      { mobile: [required('Mobile number'), phoneLK] },
    );
    if (Object.keys(errs).length) {
      setErrors(errs);
      return;
    }
    setChecking(true);
    setTimeout(() => {
      setChecking(false);
      setEligible(true);
    }, 600);
  }

  function enroll() {
    const errs = validate(
      { fullName, email: emailVal },
      { fullName: [required('Full name')], email: [emailRule] },
    );
    setErrors(errs);
    if (Object.keys(errs).length) return;
    // Mock: no API call (loyalty backend deferred).
    setEnrolled(true);
  }

  return (
    <div className="mx-auto grid max-w-6xl grid-cols-1 gap-8 lg:grid-cols-12">
      {/* Left: keypad + mobile entry (Step 1) */}
      <div className="flex flex-col gap-6 lg:col-span-5">
        <div className="flex h-full flex-col rounded-xl border border-border bg-card p-6">
          <div className="mb-6">
            <span className="mb-1 inline-flex items-center gap-1 text-[10px] font-bold uppercase tracking-widest text-primary">
              <Icon name="footprint" className="text-sm" /> Step 1
            </span>
            <h3 className="font-heading text-xl font-bold text-foreground">Mobile Number</h3>
            <p className="text-sm text-muted-foreground">Enter mobile to check loyalty eligibility.</p>
          </div>

          <div className="relative mb-2">
            <div className="flex h-20 items-center rounded-xl border-2 border-primary bg-surface px-6">
              <span className="mr-4 text-2xl font-bold text-muted-foreground">+94</span>
              <input
                className="w-full border-none bg-transparent font-heading text-3xl font-bold tracking-widest text-foreground focus:ring-0"
                value={display}
                placeholder="000 000 000"
                readOnly
              />
            </div>
          </div>
          {errors.mobile && (
            <p className="mb-3 flex items-center gap-1 text-sm text-error">
              <Icon name="error" className="text-[14px]" />
              {errors.mobile}
            </p>
          )}

          <div className="mt-4 grid flex-1 grid-cols-3 gap-3">
            {['1', '2', '3', '4', '5', '6', '7', '8', '9'].map((n) => (
              <button
                key={n}
                onClick={() => appendNum(n)}
                className="flex h-16 items-center justify-center rounded-lg border border-border bg-card text-2xl font-semibold text-foreground transition-all active:scale-95 active:bg-primary-tint"
              >
                {n}
              </button>
            ))}
            <button
              onClick={backspace}
              className="flex h-16 items-center justify-center rounded-lg border border-border bg-card text-error transition-all active:scale-95"
            >
              <Icon name="backspace" />
            </button>
            <button
              onClick={() => appendNum('0')}
              className="flex h-16 items-center justify-center rounded-lg border border-border bg-card text-2xl font-semibold text-foreground transition-all active:scale-95 active:bg-primary-tint"
            >
              0
            </button>
            <button
              onClick={checkEligibility}
              disabled={checking}
              className={`flex h-16 items-center justify-center rounded-lg font-bold text-white shadow-lg transition-all active:scale-95 disabled:opacity-70 ${
                eligible ? 'bg-accent' : 'bg-primary hover:bg-primary-dark'
              }`}
            >
              {checking ? (
                <span className="size-5 animate-spin rounded-full border-2 border-white border-t-transparent" />
              ) : (
                <Icon name={eligible ? 'check' : 'search'} className="text-2xl" />
              )}
            </button>
          </div>
        </div>
      </div>

      {/* Right: customer details (Step 2) */}
      <div
        className={`flex flex-col gap-6 transition-all duration-500 lg:col-span-7 ${
          eligible ? '' : 'pointer-events-none opacity-40 grayscale'
        }`}
      >
        <div className="relative flex flex-col gap-8 overflow-hidden rounded-xl border border-border bg-card p-8">
          <div className="absolute -right-24 -top-24 -z-10 size-64 rounded-full bg-primary-tint/40 blur-3xl" />
          <div>
            <span className="mb-1 inline-flex items-center gap-1 text-[10px] font-bold uppercase tracking-widest text-primary">
              <Icon name="footprint" className="text-sm" /> Step 2
            </span>
            <h3 className="font-heading text-xl font-bold text-foreground">Customer Details</h3>
            <p className="text-sm text-muted-foreground">Complete the profile to activate loyalty rewards.</p>
          </div>

          <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
            <Field
              label="Full Name"
              placeholder="e.g. Priyantha Silva"
              value={fullName}
              onChange={setFullName}
              error={errors.fullName}
            />

            <Field
              label="Email Address"
              type="email"
              placeholder="name@example.com"
              value={emailVal}
              onChange={setEmailVal}
              error={errors.email}
            />

            <Field
              label="Birth Date"
              type="date"
              value={birthDate}
              onChange={setBirthDate}
            />

            <Combobox
              label="Preferred Language"
              value={language}
              onChange={setLanguage}
              options={[
                { value: 'English', label: 'English' },
                { value: 'Sinhala', label: 'Sinhala' },
                { value: 'Tamil', label: 'Tamil' },
              ]}
            />
          </div>

          {/* Loyalty benefits card */}
          <div className="rounded-xl border border-border bg-surface p-5">
            <h4 className="mb-3 flex items-center gap-2 text-sm font-bold text-primary">
              <Icon name="star" className="text-lg" />
              Tier: Silver Rewards
            </h4>
            <div className="flex items-center justify-between text-xs text-muted-foreground">
              <div className="flex items-center gap-2">
                <Icon name="percent" className="text-base" />
                <span>5% instant cashback</span>
              </div>
              <div className="flex items-center gap-2">
                <Icon name="redeem" className="text-base" />
                <span>Welcome Bonus LKR 500</span>
              </div>
            </div>
          </div>

          <div className="mt-4 flex items-center gap-4">
            <button
              onClick={enroll}
              className={`flex h-14 flex-1 items-center justify-center gap-2 rounded-xl font-bold text-white shadow-lg transition-all active:scale-[0.98] ${
                enrolled ? 'bg-accent' : 'bg-primary hover:bg-primary-dark'
              }`}
            >
              <span>{enrolled ? 'Customer Enrolled' : 'Enroll Customer'}</span>
              <Icon name={enrolled ? 'check_circle' : 'how_to_reg'} />
            </button>
            <button
              onClick={() => {
                setFullName('');
                setEmailVal('');
                setBirthDate('');
                setMarketing(false);
                setErrors({});
                setEnrolled(false);
              }}
              className="h-14 rounded-xl border border-border bg-muted px-6 font-semibold text-foreground transition-all hover:bg-slate-200"
            >
              Cancel
            </button>
          </div>

          {/* Marketing opt-in */}
          <div className="flex items-center gap-3 px-2">
            <input
              id="marketing"
              type="checkbox"
              checked={marketing}
              onChange={(e) => setMarketing(e.target.checked)}
              className="size-5 rounded border-outline text-primary focus:ring-primary"
            />
            <label htmlFor="marketing" className="text-sm leading-tight text-muted-foreground">
              Subscribe customer to SMS promotions and monthly loyalty statements.
            </label>
          </div>

          {enrolled && (
            <div className="flex items-center gap-3 rounded-full bg-foreground px-6 py-3 text-white shadow-2xl">
              <Icon name="check_circle" className="text-primary-tint" />
              <span className="font-medium">Customer enrolled successfully. LKR 500 bonus applied.</span>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

// =================================================================
// REDEMPTION
// =================================================================
function RedemptionTab() {
  const [mobile, setMobile] = useState('');
  const [lookedUp, setLookedUp] = useState(false);
  const [lookupError, setLookupError] = useState<string | null>(null);
  const [redeem, setRedeem] = useState('0');
  const [redeemError, setRedeemError] = useState<string | null>(null);
  const [applied, setApplied] = useState(false);

  function lookup() {
    setLookupError(null);
    const errs = validate({ mobile }, { mobile: [required('Mobile number'), phoneLK] });
    if (errs.mobile) {
      setLookupError(errs.mobile);
      setLookedUp(false);
      return;
    }
    // Mock: no API call (loyalty backend deferred).
    setLookedUp(true);
    setApplied(false);
  }

  function clampRedeem(v: string) {
    setApplied(false);
    let n = parseFloat(v);
    if (Number.isNaN(n)) {
      setRedeem(v);
      return;
    }
    if (n > MOCK_MAX_REDEEM) n = MOCK_MAX_REDEEM;
    if (n < 0) n = 0;
    setRedeem(String(n));
  }

  function applyPoints() {
    setRedeemError(null);
    const errs = validate({ redeem }, { redeem: [positiveNumber('Redeem amount')] });
    if (errs.redeem) {
      setRedeemError(errs.redeem);
      return;
    }
    setApplied(true);
  }

  const redeemNum = parseFloat(redeem) || 0;
  const pointsUsed = redeemNum * POINTS_PER_LKR;
  const remaining = MOCK_ORDER_TOTAL - redeemNum;

  return (
    <div className="mx-auto max-w-[560px] overflow-hidden rounded-xl border border-border bg-card">
      {/* Header */}
      <div className="flex items-center justify-between border-b border-border bg-surface px-6 py-4">
        <div className="flex items-center gap-3">
          <div className="rounded-lg bg-primary-tint p-2">
            <Icon name="loyalty" className="text-primary" />
          </div>
          <div>
            <h2 className="font-heading text-xl font-extrabold leading-tight">Loyalty Redemption</h2>
            <p className="text-xs text-muted-foreground">Customer Profile: AS-8821</p>
          </div>
        </div>
      </div>

      <div className="space-y-6 p-6">
        {/* Mobile lookup */}
        <div className="flex items-end gap-2">
          <Field
            className="flex-1"
            label="Customer Mobile (+94)"
            type="tel"
            inputMode="numeric"
            placeholder="77 123 4560"
            value={mobile}
            onChange={setMobile}
            error={lookupError ?? undefined}
          />
          <button
            onClick={lookup}
            className="flex h-[42px] items-center gap-2 rounded-lg bg-primary px-5 font-bold text-white transition-all hover:bg-primary-dark active:scale-95"
          >
            <Icon name="search" />
            Lookup
          </button>
        </div>

        {lookedUp && (
          <>
            {/* Customer card */}
            <div className="flex items-center justify-between rounded-lg border border-border bg-surface p-4">
              <div className="flex items-center gap-4">
                <div className="flex size-12 items-center justify-center rounded-full border border-border bg-secondary-tint">
                  <span className="font-heading text-lg font-bold text-foreground">AP</span>
                </div>
                <div>
                  <div className="text-lg font-bold leading-none">Asela Perera</div>
                  <div className="mt-1 flex items-center gap-2">
                    <span className="rounded-full bg-secondary px-2 py-0.5 text-[10px] font-black uppercase tracking-wider text-white">
                      Gold Tier
                    </span>
                    <span className="text-xs text-muted-foreground">Member since 2021</span>
                  </div>
                </div>
              </div>
              <div className="text-right">
                <div className="text-xs font-medium text-muted-foreground">Verified Phone</div>
                <div className="text-sm font-bold text-foreground">+94 77 **** 450</div>
              </div>
            </div>

            {/* Points balance */}
            <div className="grid grid-cols-2 gap-4">
              <div className="rounded-lg border border-primary/20 bg-primary-tint/40 p-4 text-center">
                <div className="text-xs font-bold uppercase tracking-tight text-primary-dark">Available Balance</div>
                <div className="mt-1 text-3xl font-black text-primary">
                  {MOCK_POINTS.toLocaleString()} <span className="text-sm font-medium">pts</span>
                </div>
              </div>
              <div className="rounded-lg border border-border bg-surface p-4 text-center">
                <div className="text-xs font-bold uppercase tracking-tight text-muted-foreground">LKR Equivalent</div>
                <div className="mt-1 text-3xl font-black text-foreground">{lkr(MOCK_MAX_REDEEM)}</div>
              </div>
            </div>

            {/* Redemption control */}
            <div className="space-y-4">
              <div className="flex items-end justify-between">
                <label className="text-sm font-bold text-foreground">Amount to Redeem</label>
                <div className="flex items-baseline gap-1">
                  <span className="text-2xl font-black text-primary">{lkr(redeemNum)}</span>
                  <span className="text-xs text-muted-foreground">({pointsUsed.toLocaleString()} Points)</span>
                </div>
              </div>

              <div className="relative pb-2 pt-6">
                <input
                  type="range"
                  min={0}
                  max={MOCK_MAX_REDEEM}
                  step={50}
                  value={redeemNum}
                  onChange={(e) => clampRedeem(e.target.value)}
                  className="h-2 w-full cursor-pointer appearance-none rounded-lg bg-slate-200 accent-primary"
                />
                <div className="mt-2 flex justify-between text-[10px] font-bold uppercase tracking-widest text-muted-foreground">
                  <span>0 LKR</span>
                  <span>{MOCK_MAX_REDEEM / 2} LKR</span>
                  <span>{MOCK_MAX_REDEEM} LKR</span>
                </div>
              </div>

              <div className="relative">
                <span className="absolute left-4 top-1/2 -translate-y-1/2 font-bold text-muted-foreground">LKR</span>
                <input
                  type="number"
                  className="w-full rounded-lg border border-border bg-surface py-3 pl-14 pr-4 text-lg font-bold transition-all focus:border-primary focus:ring-1 focus:ring-primary"
                  placeholder="0.00"
                  value={redeem}
                  onChange={(e) => clampRedeem(e.target.value)}
                />
              </div>
              {redeemError && (
                <p className="flex items-center gap-1 text-sm text-error">
                  <Icon name="error" className="text-[14px]" />
                  {redeemError}
                </p>
              )}
            </div>

            {/* Calculation summary */}
            <div className="space-y-3 rounded-lg border border-border bg-surface p-4">
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Order Total</span>
                <span className="font-medium">{lkr(MOCK_ORDER_TOTAL)}</span>
              </div>
              <div className="flex justify-between text-sm text-primary">
                <span className="font-bold italic">Points applied</span>
                <span className="font-bold">- {lkr(redeemNum)}</span>
              </div>
              <div className="h-px bg-border" />
              <div className="flex items-center justify-between pt-1">
                <span className="font-heading font-bold text-foreground">Remaining Balance</span>
                <span className="font-heading text-xl font-black text-foreground">{lkr(remaining)}</span>
              </div>
            </div>
          </>
        )}
      </div>

      {/* Footer actions */}
      <div className="flex gap-3 border-t border-border bg-surface px-6 py-5">
        <button
          onClick={() => {
            setRedeem('0');
            setRedeemError(null);
            setApplied(false);
          }}
          className="flex-1 rounded-lg border border-outline py-4 font-bold text-foreground transition-all duration-150 hover:bg-muted active:scale-95"
        >
          Cancel
        </button>
        <button
          onClick={applyPoints}
          disabled={!lookedUp}
          className="flex flex-[2] items-center justify-center gap-2 rounded-lg bg-primary py-4 font-bold text-white shadow-lg transition-all hover:bg-primary-dark active:scale-95 disabled:opacity-50"
        >
          <span style={{ fontVariationSettings: "'FILL' 1" }}>
            <Icon name="check_circle" />
          </span>
          {applied ? 'Points Applied' : 'Apply Points to Order'}
        </button>
      </div>
    </div>
  );
}
