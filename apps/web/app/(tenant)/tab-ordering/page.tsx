'use client';

import Link from 'next/link';
import { Topbar } from '@/components/app-shell/Topbar';
import { Icon } from '@/components/ui/Icon';

export default function TabOrderingPage() {
  return (
    <>
      <Topbar title="Tab Ordering" subtitle="Let guests order from their phones, right at the table" />

      <main className="mx-auto max-w-[1280px] p-6 md:p-8">
        {/* Hero Section */}
        <section className="mb-20 grid grid-cols-1 items-center gap-12 lg:grid-cols-2">
          <div className="space-y-6">
            <div className="inline-flex items-center gap-2 rounded-full bg-secondary-tint px-3 py-1 text-xs font-bold uppercase tracking-wider text-secondary">
              <Icon name="new_releases" className="text-[14px]" />
              Module Update
            </div>
            <h1 className="font-heading text-5xl font-extrabold leading-tight tracking-tighter text-on-surface md:text-6xl">
              Revolutionize your table service with{' '}
              <span className="text-primary">Tab Ordering</span>.
            </h1>
            <p className="max-w-xl text-lg leading-relaxed text-muted-foreground">
              Empower your staff and delight your guests. RIT HMS Tab Ordering bridges the gap between
              the dining floor and the kitchen with real-time synchronization on any handheld device.
            </p>
            <div className="flex flex-col gap-4 pt-4 sm:flex-row">
              <Link
                href="/pos"
                className="flex items-center justify-center gap-2 rounded-xl bg-primary px-8 py-4 font-bold text-white transition-all hover:bg-primary-dark active:scale-95"
              >
                Add to Plan
                <Icon name="arrow_forward" />
              </Link>
              <div className="flex items-center gap-3 px-6 py-4">
                <span className="font-heading text-2xl font-black text-on-surface">LKR 2,500</span>
                <span className="font-medium text-muted-foreground">/user/month</span>
              </div>
            </div>
          </div>
          <div className="group relative">
            <div className="absolute -inset-4 rounded-full bg-primary-container opacity-20 blur-3xl transition-opacity group-hover:opacity-30"></div>
            <div className="relative overflow-hidden rounded-2xl border border-border shadow-2xl">
              <img
                className="h-auto w-full object-cover"
                alt="A professional waiter in a high-end restaurant using a sleek handheld tablet to take an order."
                src="https://lh3.googleusercontent.com/aida-public/AB6AXuDrKwmkoFNL1GZWJcglgj45TRNz1Mn_zPIQSlOxeX95fZ0IZma48qkp-dQTZVDHluitOALEoUeuttwbKuYVFz6_6iSlJ94QZGLyHOqKJscu3TxQ6cPJRa6c7_iZyAMEFpTdtK9rGXXasJu_6EK8Jo2GT_2dZHTC2_ldl-6ZofsekcmD5BA3D1Jp2uxtXondv1h6WZmXPcfSfqqqYoPJE0XYUZR_KCv4S-4bIJqZ5JIryzXzixmhX31r_um6aQVhlLm3qleLqe5nna8"
              />
              <div className="absolute inset-0 bg-gradient-to-t from-black/40 to-transparent"></div>
              <div className="absolute bottom-6 left-6 right-6 rounded-lg border border-white/30 bg-white/70 p-4 backdrop-blur">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary text-white">
                      <Icon name="tablet_android" />
                    </div>
                    <div>
                      <p className="text-xs font-bold text-on-surface">Staff Terminal v4.2</p>
                      <p className="text-[10px] text-muted-foreground">Live Sync Active</p>
                    </div>
                  </div>
                  <span className="rounded bg-primary-tint px-2 py-1 text-[10px] font-bold text-primary-dark">
                    OPERATIONAL
                  </span>
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* Bento Grid Benefits */}
        <section className="mb-24 space-y-12">
          <div className="space-y-2 text-center">
            <h2 className="font-heading text-3xl font-bold tracking-tight">Key Operational Benefits</h2>
            <p className="text-muted-foreground">Designed for the speed of modern hospitality.</p>
          </div>
          <div className="grid grid-cols-12 gap-6">
            {/* Benefit 1 */}
            <div className="group col-span-12 rounded-2xl border border-border bg-card p-8 transition-all duration-300 hover:border-primary md:col-span-8">
              <div className="flex h-full flex-col justify-between gap-8">
                <div className="space-y-4">
                  <Icon name="contactless" className="text-4xl text-primary" />
                  <h3 className="font-heading text-2xl font-bold">Contactless ordering</h3>
                  <p className="text-lg text-muted-foreground">
                    Minimize physical touchpoints and reduce order errors. Guests view digital menus
                    while staff transmit orders directly from the table to the kitchen via encrypted
                    handheld connections.
                  </p>
                </div>
                <div className="grid grid-cols-3 gap-4">
                  <div className="h-2 rounded-full bg-primary-container"></div>
                  <div className="h-2 rounded-full bg-primary-container"></div>
                  <div className="h-2 rounded-full bg-border"></div>
                </div>
              </div>
            </div>
            {/* Benefit 2 */}
            <div className="group col-span-12 rounded-2xl bg-accent p-8 text-white transition-transform duration-300 hover:scale-[1.02] md:col-span-4">
              <div className="space-y-6">
                <Icon name="speed" className="text-4xl" />
                <h3 className="font-heading text-2xl font-bold leading-tight">Faster turnarounds</h3>
                <p className="opacity-90">
                  Reduce average table wait times by 15 minutes. Orders reach the kitchen instantly,
                  cutting down on staff transit time and accelerating service cycles.
                </p>
                <div className="border-t border-white/20 pt-4">
                  <div className="flex items-center justify-between text-sm font-bold">
                    <span>Avg. Table Cycle</span>
                    <span>-22%</span>
                  </div>
                </div>
              </div>
            </div>
            {/* Benefit 3 */}
            <div className="col-span-12 rounded-2xl border border-border bg-surface p-8 transition-colors hover:bg-muted md:col-span-5">
              <div className="space-y-4">
                <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-slate-900 text-slate-100">
                  <Icon name="restaurant" />
                </div>
                <h3 className="font-heading text-xl font-bold">Kitchen integration</h3>
                <p className="text-muted-foreground">
                  Direct KOT (Kitchen Order Ticket) generation. No more manual entry from paper slips.
                  Automated routing to specific stations based on item category.
                </p>
              </div>
            </div>
            {/* Benefit 4 */}
            <div className="col-span-12 flex flex-col items-center gap-6 rounded-2xl border border-border bg-surface p-8 md:col-span-7 md:flex-row">
              <div className="flex-1 space-y-4">
                <h3 className="font-heading text-xl font-bold">Real-time Stock Guard</h3>
                <p className="text-muted-foreground">
                  Menu items are automatically marked as &ldquo;86&apos;d&rdquo; or out-of-stock across
                  all tablets the moment the inventory hits zero.
                </p>
                <ul className="space-y-2">
                  <li className="flex items-center gap-2 text-sm font-medium text-on-surface">
                    <Icon name="check_circle" className="text-sm text-primary" />
                    Auto-gray out items
                  </li>
                  <li className="flex items-center gap-2 text-sm font-medium text-on-surface">
                    <Icon name="check_circle" className="text-sm text-primary" />
                    Low-stock badges
                  </li>
                </ul>
              </div>
              <div className="w-full overflow-hidden rounded-xl shadow-sm md:w-1/2">
                <img
                  className="h-32 w-full object-cover"
                  alt="A close-up of a digital kitchen display system showing an organized grid of incoming orders."
                  src="https://lh3.googleusercontent.com/aida-public/AB6AXuDFAUuiGjQQ3nx3QV58FXQBTMwxoAq9QP8yi7VWvUuQV2Yq-ruwntvJ7QmJT0sYLliNy_h22Z_TYgJ8aYH4smEcHiagUax4XIp137MqlgTZUFuMrXXSgtOpYsHt7UhiIkn-5YW2RXCeKukfLUe1rGVfhqJjn84Ju4C6T9tE75-yQLeY80qhoEdRPyBTVxDyUkZmsgKmRrPgAu7bXRKCbwMyBkLTqBSLttztp7drO7CmLcGJ_CF1LNnv-vRQNzbY8k1u0kWuY2aUoGs"
                />
              </div>
            </div>
          </div>
        </section>

        {/* Pricing / CTA Section */}
        <section className="relative overflow-hidden rounded-[2rem] bg-slate-900 p-12 text-slate-100">
          <div className="pointer-events-none absolute right-0 top-0 h-full w-1/2 bg-gradient-to-l from-primary/10 to-transparent"></div>
          <div className="relative z-10 grid grid-cols-1 items-center gap-12 md:grid-cols-2">
            <div className="space-y-6">
              <h2 className="font-heading text-4xl font-bold text-white">
                Ready to upgrade your floor service?
              </h2>
              <p className="text-lg text-white/70">
                Add the Tab Ordering Module to your current RIT HMS subscription and start serving
                smarter today.
              </p>
              <div className="flex items-center gap-4">
                <div className="rounded-lg bg-white/10 px-4 py-2 backdrop-blur">
                  <p className="text-[10px] font-black uppercase tracking-widest text-white/50">
                    Current Promo
                  </p>
                  <p className="font-bold text-white">Free Setup Assistance</p>
                </div>
                <div className="rounded-lg bg-white/10 px-4 py-2 backdrop-blur">
                  <p className="text-[10px] font-black uppercase tracking-widest text-white/50">
                    Training
                  </p>
                  <p className="font-bold text-white">On-site Support</p>
                </div>
              </div>
            </div>
            <div className="rounded-2xl border border-outline bg-card p-8 text-on-surface">
              <div className="mb-6 flex items-start justify-between">
                <div>
                  <h3 className="font-heading text-xl font-bold">Tab Ordering</h3>
                  <p className="text-sm text-muted-foreground">Unlimited orders, synchronized floor.</p>
                </div>
                <div className="text-right">
                  <p className="font-heading text-3xl font-black text-primary">LKR 2,500</p>
                  <p className="text-xs font-medium text-muted-foreground">per user / mo</p>
                </div>
              </div>
              <div className="mb-8 space-y-4">
                <div className="flex items-center gap-3 text-sm">
                  <Icon name="check_circle" className="text-primary" />
                  <span>Full KOT synchronization</span>
                </div>
                <div className="flex items-center gap-3 text-sm">
                  <Icon name="check_circle" className="text-primary" />
                  <span>Live inventory stock-guard</span>
                </div>
                <div className="flex items-center gap-3 text-sm">
                  <Icon name="check_circle" className="text-primary" />
                  <span>Multi-device floor management</span>
                </div>
              </div>
              <Link
                href="/pos"
                className="block w-full rounded-xl bg-primary py-4 text-center font-bold text-white shadow-lg transition-all hover:bg-primary-dark active:scale-95"
              >
                Add to Plan
              </Link>
            </div>
          </div>
        </section>
      </main>
    </>
  );
}
