'use client';

import { useMemo, useState } from 'react';
import { Topbar } from '@/components/app-shell/Topbar';
import {
  Search, ShoppingCart, LayoutGrid, Users, Tag, Boxes, ChefHat, PartyPopper,
  BarChart3, UserCog, Clock, Truck, Settings, LifeBuoy, Keyboard, ChevronRight,
} from 'lucide-react';

type Section = {
  id: string;
  title: string;
  icon: typeof Search;
  blurb: string;
  steps: { q: string; a: string }[];
};

const GUIDE: Section[] = [
  {
    id: 'getting-started', title: 'Getting started', icon: LifeBuoy,
    blurb: 'The essentials to find your way around RIT HMS.',
    steps: [
      { q: 'Navigating the app', a: 'The left sidebar groups every module — POS, Floor, Inventory, Reports and so on. The top bar shows the current screen, a global search (⌘K / Ctrl-K), the notification bell, and this help guide.' },
      { q: 'Switching outlet / workspace', a: 'Open the account menu at the top of the sidebar to see who you are signed in as and your role. Most screens have an outlet selector when your business runs more than one location.' },
      { q: 'Signing out', a: 'Click the account menu (or the log-out icon at the bottom of the sidebar) and confirm. You will be returned to the login screen.' },
      { q: 'Global search', a: 'Press ⌘K (Mac) or Ctrl-K (Windows) anywhere to search menu items, customers and transactions, then jump straight to them.' },
    ],
  },
  {
    id: 'pos', title: 'POS & billing', icon: ShoppingCart,
    blurb: 'Take orders, send to the kitchen, take payment and recall bills.',
    steps: [
      { q: 'Taking an order', a: 'Pick a table or start a takeaway/delivery order, tap items to add them, adjust quantities and add modifiers. The running total, taxes and any active promotions are calculated live.' },
      { q: 'Sending to the kitchen (KOT)', a: 'Use “Send to KOT” to fire the order to the relevant kitchen station. New and changed lines are printed/displayed; already-sent lines are not duplicated.' },
      { q: 'Assigning a steward', a: 'Open the bill’s details to set the covers (number of guests) and the steward serving the table. Steward sales feed the steward performance report.' },
      { q: 'Taking payment', a: 'Choose a payment type — cash, card, credit (house account), loyalty points or a split across several. For split payments, add each tender; the balance updates until the bill is settled.' },
      { q: 'Redeeming loyalty points', a: 'With a customer attached, use the “Redeem points” panel to apply any amount up to the customer’s balance — a partial redemption (e.g. 500 of 1,000 points) is fine. Removing the customer clears any loyalty/credit tender automatically.' },
      { q: 'Multi-currency & tips', a: 'Foreign-currency tenders are converted at the configured rate; the bill is always settled in the base currency. A tip can be added on settlement.' },
      { q: 'Recalling a bill', a: 'Use Recall to find a recent bill and re-open or view it. Viewing opens the receipt without re-printing.' },
    ],
  },
  {
    id: 'floor', title: 'Tables & reservations', icon: LayoutGrid,
    blurb: 'Manage the floor plan, table status, reservations, split/merge and transfers.',
    steps: [
      { q: 'Table status', a: 'The floor view shows each table as free, occupied or reserved. Tap a table to open or resume its bill.' },
      { q: 'Taking a table off service', a: 'In Manage tables, use “Take off” to temporarily mark a table inactive (under repair, reserved long-term, etc.) and “Bring back” to restore it. A table that is currently occupied cannot be taken off.' },
      { q: 'Reservations', a: 'Create a reservation with the guest, party size and time. Reservation times cannot be set in the past. Today’s reservations also appear in the notification bell.' },
      { q: 'Split, merge & transfer', a: 'Split a bill by item or amount, merge two bills, or transfer a bill to another table from the floor view.' },
    ],
  },
  {
    id: 'customers', title: 'Customers & loyalty', icon: Users,
    blurb: 'The customer master, credit accounts, and the loyalty programme.',
    steps: [
      { q: 'Customer master', a: 'Maintain customers with categories, contact details and notes. Attach a customer to a bill to track their history and apply targeted pricing or promotions.' },
      { q: 'Credit / house accounts', a: 'Flag a customer as a credit customer to allow “on account” settlement. Outstanding balances are tracked as receivables and can be settled later.' },
      { q: 'Loyalty programme', a: 'Customers earn points on eligible spend (tiered rules, with expiry). Points are redeemed at the POS, in part or in full.' },
    ],
  },
  {
    id: 'promotions', title: 'Promotions', icon: Tag,
    blurb: 'Happy-hour, BOGO, bundles, bill-value and bank-card offers.',
    steps: [
      { q: 'Creating a promotion', a: 'Choose a promotion type, the products or categories it applies to, the value (percentage or amount) and the active window. End dates must be on or after the start date.' },
      { q: 'How promotions apply', a: 'Eligible promotions are applied automatically at the POS as items are added — happy-hour by time, BOGO/bundles by basket, bill-value thresholds at settlement, and bank-BIN offers by the card used.' },
    ],
  },
  {
    id: 'inventory', title: 'Inventory & purchasing', icon: Boxes,
    blurb: 'Stock, suppliers, purchase orders, GRN, transfers, wastage and counts.',
    steps: [
      { q: 'Stock & costing', a: 'Stock is held per product per location. Cost is a moving weighted average, recalculated on every goods receipt; issues do not move the average.' },
      { q: 'Low-stock alerts', a: 'Set a reorder level on a product to be alerted in the notification bell when on-hand falls to or below it.' },
      { q: 'Purchase orders & GRN', a: 'Raise a PO (with discounts, charges, currency and terms), send it for approval, then receive against it with a GRN — including free/bonus items and the supplier invoice number. Landed cost flows into the weighted-average cost.' },
      { q: 'Transfers, returns & wastage', a: 'Move stock between locations, return goods to suppliers (PRN), and record wastage or stock adjustments with reasons.' },
      { q: 'Stock counts', a: 'Run a physical count, enter counted quantities and post the variance to reconcile system stock to reality.' },
    ],
  },
  {
    id: 'production', title: 'Production & recipes', icon: ChefHat,
    blurb: 'Recipes / BOM, production orders and packing.',
    steps: [
      { q: 'Recipes', a: 'Define a recipe (bill of materials) for a made product, with ingredient quantities, units and a yield. Units convert automatically (e.g. 500 g drawn from kg stock).' },
      { q: 'Producing', a: 'Create a production order to consume ingredients and produce the output at cost. Ad-hoc production without a recipe and multi-product notes are supported, with draft → post and revert/void.' },
    ],
  },
  {
    id: 'catering', title: 'Catering & banquet', icon: PartyPopper,
    blurb: 'Event bookings, halls, per-head packages and off-site catering.',
    steps: [
      { q: 'Booking an event', a: 'Create a booking with the customer, hall, package, guest count (pax) and date/time. The bill is pax × package price plus any extras, less a discount. A hall cannot be double-booked for an overlapping time, and dates cannot be back-dated.' },
      { q: 'Deposits & balance', a: 'Record deposits and part-payments against the event; the outstanding balance updates as you go.' },
      { q: 'Producing from inventory', a: 'When a package is linked to a recipe, use “Produce” on the event to consume ingredients from stock and capture the food cost and margin.' },
      { q: 'Off-site catering', a: 'Tick “off-site” on a booking to capture the delivery address and vehicle, then track the dispatch status (pending → dispatched → delivered).' },
    ],
  },
  {
    id: 'reports', title: 'Reports & accounting', icon: BarChart3,
    blurb: 'Sales, costing, budgets, the transaction explorer and GL export.',
    steps: [
      { q: 'Sales & costing reports', a: 'Sales register, daily sales, item usage, food costing, bin card and stock balance are all available, filterable by date and outlet.' },
      { q: 'Transaction explorer', a: 'A gateway-style log of every bill — settled, void or open — with tender methods, cashier/steward, filters (including split-payment combinations) and a drill-down to items and individual tenders. Exportable to CSV.' },
      { q: 'Budget vs sales', a: 'Set a budget per outlet, or a company-wide (all-outlets) budget, and compare against actual sales.' },
      { q: 'GL / accounting export', a: 'Journal postings, accounts payable, payment terms and expenses are available for export to your accounting system.' },
    ],
  },
  {
    id: 'team', title: 'Team & permissions', icon: UserCog,
    blurb: 'Users, roles, stewards and approval limits.',
    steps: [
      { q: 'Users & roles', a: 'Add team members and assign a role — Owner, Manager, Cashier, Kitchen or Accountant — which controls what they can see and do.' },
      { q: 'Stewards', a: 'Mark a team member as a steward (server) so they can be assigned to bills. A steward need not be a cashier — a steward-only record without login is allowed.' },
      { q: 'Discount & approval limits', a: 'Set per-role maximum discount limits and approval workflows so larger discounts or sensitive actions require a manager’s sign-off.' },
    ],
  },
  {
    id: 'shifts', title: 'Shifts & day-end', icon: Clock,
    blurb: 'Open/close shifts, cash-up and the day-end / month-end close.',
    steps: [
      { q: 'Shifts & cash-up', a: 'Open a shift at the start of service and close it with a cash-up Z-report reconciling expected vs counted cash.' },
      { q: 'Day-end / month-end close', a: 'Run the close to lock a period, after the physical stock count, with a full audit log of changes.' },
    ],
  },
  {
    id: 'delivery', title: 'Delivery & aggregators', icon: Truck,
    blurb: 'Uber Eats / PickMe incoming orders and the 86 list.',
    steps: [
      { q: 'Incoming orders', a: 'Aggregator orders arrive in the delivery queue; new ones awaiting acceptance also surface in the notification bell. Accept, mark preparing/ready and complete through the lifecycle.' },
      { q: '86 / item availability', a: 'Mark items as unavailable (“86”) to stop them being ordered on connected aggregator menus.' },
    ],
  },
  {
    id: 'settings', title: 'Settings', icon: Settings,
    blurb: 'Tax, bill branding, prefixes and organisation details.',
    steps: [
      { q: 'Tax & charges', a: 'Configure VAT/tax classes and service charges to match Sri Lankan compliance, including compound and per-product tax.' },
      { q: 'Bill branding & prefixes', a: 'Set your logo, address, tax certificates, and document prefixes (invoices, POs, GRNs) under Settings.' },
    ],
  },
];

const SHORTCUTS = [
  ['⌘K / Ctrl-K', 'Open global search'],
  ['Esc', 'Close search or panels'],
  ['Tap table', 'Open / resume its bill'],
];

export default function HelpPage() {
  const [query, setQuery] = useState('');

  const sections = useMemo(() => {
    const term = query.trim().toLowerCase();
    if (!term) return GUIDE;
    return GUIDE
      .map(s => ({
        ...s,
        steps: s.steps.filter(st =>
          st.q.toLowerCase().includes(term) || st.a.toLowerCase().includes(term) || s.title.toLowerCase().includes(term)),
      }))
      .filter(s => s.steps.length > 0 || s.title.toLowerCase().includes(term));
  }, [query]);

  return (
    <div>
      <Topbar title="Help & support" subtitle="Friendly guides and quick answers for every part of the app" />
      <div className="mx-auto max-w-6xl px-6 py-6 md:p-8">
        {/* Search */}
        <div className="mb-6 flex items-center gap-3 rounded-xl border border-border bg-card px-4 py-3">
          <Search className="size-5 shrink-0 text-muted-foreground" />
          <input value={query} onChange={e => setQuery(e.target.value)}
            placeholder="Search the guide — e.g. “loyalty”, “GRN”, “split bill”…"
            className="flex-1 bg-transparent text-sm outline-none placeholder:text-muted-foreground" />
          {query && <button onClick={() => setQuery('')} className="text-xs text-muted-foreground hover:underline">Clear</button>}
        </div>

        <div className="grid grid-cols-1 gap-6 lg:grid-cols-[220px_1fr]">
          {/* TOC */}
          <nav className="hidden lg:block">
            <div className="sticky top-6 space-y-1">
              <p className="px-3 pb-2 text-xs font-semibold uppercase tracking-wide text-muted-foreground">Contents</p>
              {GUIDE.map(s => (
                <a key={s.id} href={`#${s.id}`}
                  className="flex items-center gap-2 rounded-lg px-3 py-2 text-sm text-muted-foreground hover:bg-muted hover:text-foreground">
                  <s.icon className="size-4 shrink-0" /> {s.title}
                </a>
              ))}
              <a href="#shortcuts" className="flex items-center gap-2 rounded-lg px-3 py-2 text-sm text-muted-foreground hover:bg-muted hover:text-foreground">
                <Keyboard className="size-4 shrink-0" /> Keyboard shortcuts
              </a>
            </div>
          </nav>

          {/* Content */}
          <div className="space-y-8">
            {sections.length === 0 && (
              <div className="rounded-xl border border-border bg-card px-6 py-12 text-center text-sm text-muted-foreground">
                No help topics match &ldquo;{query.trim()}&rdquo;. Try a different word, or clear the search.
              </div>
            )}
            {sections.map(s => (
              <section key={s.id} id={s.id} className="scroll-mt-6 rounded-xl border border-border bg-card p-6">
                <div className="mb-1 flex items-center gap-3">
                  <span className="flex size-9 items-center justify-center rounded-lg bg-primary/10 text-primary"><s.icon className="size-5" /></span>
                  <h2 className="font-heading text-lg font-bold">{s.title}</h2>
                </div>
                <p className="mb-4 text-sm text-muted-foreground">{s.blurb}</p>
                <dl className="space-y-4">
                  {s.steps.map((st, i) => (
                    <div key={i} className="border-l-2 border-border pl-4">
                      <dt className="flex items-center gap-1.5 text-sm font-semibold">
                        <ChevronRight className="size-3.5 text-primary" /> {st.q}
                      </dt>
                      <dd className="mt-1 text-sm leading-relaxed text-muted-foreground">{st.a}</dd>
                    </div>
                  ))}
                </dl>
              </section>
            ))}

            {/* Shortcuts */}
            <section id="shortcuts" className="scroll-mt-6 rounded-xl border border-border bg-card p-6">
              <div className="mb-4 flex items-center gap-3">
                <span className="flex size-9 items-center justify-center rounded-lg bg-primary/10 text-primary"><Keyboard className="size-5" /></span>
                <h2 className="font-heading text-lg font-bold">Keyboard shortcuts</h2>
              </div>
              <dl className="divide-y divide-border">
                {SHORTCUTS.map(([k, v]) => (
                  <div key={k} className="flex items-center justify-between py-2 text-sm">
                    <dd className="text-muted-foreground">{v}</dd>
                    <dt><kbd className="rounded border border-border bg-muted px-2 py-0.5 text-xs font-medium">{k}</kbd></dt>
                  </div>
                ))}
              </dl>
            </section>

            <div className="rounded-xl border border-dashed border-border bg-card/50 px-6 py-5 text-sm text-muted-foreground">
              Still stuck? Contact your administrator or the RIT HMS support team. Include the screen you were on and what you expected to happen.
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
