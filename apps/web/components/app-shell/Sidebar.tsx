"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { cn } from "@/lib/utils";
import { apiClient } from "@/lib/api-client";
import { confirmDialog } from "@/components/ui/confirm";
import { Modal } from "@/components/ui/Modal";

type Outlet = {
  id: string;
  code: string;
  name: string;
  locationType?: string;
  canSell?: boolean;
  operatingHours?: string | null;
};

type Session = {
  tenant: { id: string; slug: string; displayName: string };
  user: {
    email: string;
    displayName: string;
    role: number;
    homeLocationId?: string | null;
    isServer?: boolean;
  };
};

// Head-office-only screens — hidden from a branch-pinned user.
const HQ_ONLY = new Set([
  "/settings",
  "/team",
  "/accounting",
  "/audit",
  "/transactions",
  "/approvals",
  "/data-import",
]);

const ROLE_LABEL: Record<number, string> = {
  0: "Owner",
  1: "Manager",
  2: "Cashier",
  3: "Kitchen",
  4: "Accountant",
  5: "Admin",
};

// Material Symbols names rendered as simple inline glyphs via the icon font
// would need the font loaded; for now use lucide-react.
import {
  LayoutDashboard,
  ShoppingCart,
  UtensilsCrossed,
  Boxes,
  BarChart3,
  Settings,
  ChevronDown,
  ChevronRight,
  MapPin,
  LogOut,
  X,
  Search,
  Check,
  Truck,
  ClipboardList,
  ArrowLeftRight,
  Bike,
  Factory,
  ChefHat,
  Users,
  ListPlus,
  Tag,
  Contact,
  ScrollText,
  ClipboardCheck,
  ExternalLink,
  Landmark,
  Receipt,
  PartyPopper,
  ConciergeBell,
  Lock,
  Plane,
  ShieldCheck,
  Upload,
  PackageSearch,
  ClipboardEdit,
  Trash2,
  FileText,
  Building2,
  Layers,
  Ruler,
  Scale,
  Repeat,
  ReceiptText,
  Percent,
  BadgePercent,
  IdCard,
  CreditCard,
  Tablet,
  FolderTree,
} from "lucide-react";

// #6 Pro/Lite gating: nav href → the plan feature it needs. A Lite tenant sees these
// locked (🔒) and clicking opens an upgrade prompt instead of the module.
const FEATURE_OF: Record<string, string> = {
  "/accounting": "accounting",
  "/production": "production",
  "/promotions": "promotions",
  "/catering": "catering",
};

// Role ids: 0 Owner · 1 Manager · 2 Cashier · 3 Kitchen · 4 Accountant.
// `roles` mirrors the API authorization policies so the nav only shows what a
// user can actually reach.
// `newTab`: roles for whom this opens in a new browser tab. Owners/Managers run
// back-office in one tab and want POS + the kitchen display on their own screens;
// a cashier/kitchen user keeps them in-place (it's their primary screen).
type NavItem = {
  href: string;
  label: string;
  icon: typeof LayoutDashboard;
  section: string;
  roles: number[];
  newTab?: number[];
};
// Section taxonomy follows charuna_dev's scheme (Master Data / Unit of Measure /
// Tax & Service Charge / Transactions / Customer Master / Promotion / Reports /
// Settings). Pages that only exist in full-app have no charuna_dev section to
// inherit, so each is slotted into the existing section closest to its topic
// rather than kept in ad-hoc groups of its own.
const NAV: NavItem[] = [
  {
    href: "/dashboard",
    label: "Dashboard",
    icon: LayoutDashboard,
    section: "Dashboard",
    roles: [0, 1, 2, 3, 4],
  },

  {
    href: "/locations",
    label: "Locations",
    icon: MapPin,
    section: "Master Data",
    roles: [0, 1],
  },
  {
    href: "/departments",
    label: "Departments",
    icon: Building2,
    section: "Master Data",
    roles: [0, 1],
  },
  {
    href: "/suppliers",
    label: "Suppliers",
    icon: Truck,
    section: "Master Data",
    roles: [0, 1],
  },
  {
    href: "/tour-operators",
    label: "Tour Agents",
    icon: Plane,
    section: "Master Data",
    roles: [0, 1],
  },
  {
    href: "/menu",
    label: "Products",
    icon: UtensilsCrossed,
    section: "Master Data",
    roles: [0, 1],
  },
  {
    href: "/product-categories",
    label: "Categories",
    icon: FolderTree,
    section: "Master Data",
    roles: [0, 1],
  },
  {
    href: "/serving-units",
    label: "Serving Units",
    icon: Scale,
    section: "Master Data",
    roles: [0, 1],
  },
  {
    href: "/price-levels",
    label: "Price Levels",
    icon: Layers,
    section: "Master Data",
    roles: [0, 1],
  },
  {
    href: "/kitchen-stations",
    label: "Kitchen Stations",
    icon: ChefHat,
    section: "Master Data",
    roles: [0, 1],
  },
  {
    href: "/modifiers",
    label: "Add-ons",
    icon: ListPlus,
    section: "Master Data",
    roles: [0, 1],
  },
  {
    href: "/pos",
    label: "POS",
    icon: ShoppingCart,
    section: "Master Data",
    roles: [0, 1, 2],
    newTab: [0, 1],
  },
  {
    href: "/kot",
    label: "KOT",
    icon: ChefHat,
    section: "Master Data",
    roles: [0, 1, 2, 3],
    newTab: [0, 1],
  },
  {
    href: "/production",
    label: "Production",
    icon: Factory,
    section: "Master Data",
    roles: [0, 1],
  },
  {
    href: "/floor",
    label: "Floor",
    icon: MapPin,
    section: "Master Data",
    roles: [0, 1, 2],
  },
  {
    href: "/open-tabs",
    label: "Open tabs",
    icon: ConciergeBell,
    section: "Master Data",
    roles: [0, 1],
  },
  {
    href: "/delivery",
    label: "Delivery",
    icon: Bike,
    section: "Master Data",
    roles: [0, 1, 2],
  },

  {
    href: "/unit-of-measure",
    label: "UOM",
    icon: Ruler,
    section: "Unit of Measure",
    roles: [0, 1],
  },
  {
    href: "/uom-conversions",
    label: "Conversions",
    icon: Repeat,
    section: "Unit of Measure",
    roles: [0, 1],
  },

  {
    href: "/tax-types",
    label: "Charge Types",
    icon: Percent,
    section: "Tax & Service Charge",
    roles: [0, 1, 4],
  },
  {
    href: "/tax-service-charge",
    label: "Tax & Service Charge",
    icon: ReceiptText,
    section: "Tax & Service Charge",
    roles: [0, 1, 4],
  },

  {
    href: "/purchasing",
    label: "Purchase Order",
    icon: ClipboardList,
    section: "Transactions",
    roles: [0, 1],
  },
  {
    href: "/grn",
    label: "GRN",
    icon: ShoppingCart,
    section: "Transactions",
    roles: [0, 1],
  },
  {
    href: "/request-notes",
    label: "Request Notes",
    icon: FileText,
    section: "Transactions",
    roles: [0, 1],
  },
  {
    href: "/inventory",
    label: "Stock",
    icon: Boxes,
    section: "Transactions",
    roles: [0, 1],
  },
  {
    href: "/stock-adjustments",
    label: "Stock Adjustment",
    icon: ClipboardEdit,
    section: "Transactions",
    roles: [0, 1],
  },
  {
    href: "/wastage",
    label: "Wastage",
    icon: Trash2,
    section: "Transactions",
    roles: [0, 1],
  },
  {
    href: "/replenishment",
    label: "Replenishment",
    icon: PackageSearch,
    section: "Transactions",
    roles: [0, 1],
  },
  {
    href: "/transfers",
    label: "Transfers",
    icon: ArrowLeftRight,
    section: "Transactions",
    roles: [0, 1],
  },
  {
    href: "/stock-count",
    label: "Stock count",
    icon: ClipboardCheck,
    section: "Transactions",
    roles: [0, 1],
  },
  {
    href: "/approvals",
    label: "Approvals",
    icon: ShieldCheck,
    section: "Transactions",
    roles: [0, 1, 4],
  },

  {
    href: "/customers",
    label: "Customers",
    icon: Contact,
    section: "Customer Master",
    roles: [0, 1, 4],
  },
  {
    href: "/loyalty",
    label: "Loyalty",
    icon: BadgePercent,
    section: "Customer Master",
    roles: [0, 1, 4],
  },
  {
    href: "/loyalty-customers",
    label: "Loyalty Customers",
    icon: IdCard,
    section: "Customer Master",
    roles: [0, 1, 4],
  },
  {
    href: "/loyalty-card-schemes",
    label: "Loyalty Card Schemes",
    icon: CreditCard,
    section: "Customer Master",
    roles: [0, 1],
  },
  {
    href: "/catering",
    label: "Catering",
    icon: PartyPopper,
    section: "Customer Master",
    roles: [0, 1],
  },

  {
    href: "/promotions",
    label: "Promotions",
    icon: Tag,
    section: "Promotion",
    roles: [0, 1, 4],
  },

  {
    href: "/reports",
    label: "Reports",
    icon: BarChart3,
    section: "Reports",
    roles: [0, 1, 4],
  },
  {
    href: "/transactions",
    label: "Transactions",
    icon: Receipt,
    section: "Reports",
    roles: [0, 1, 4],
  },
  {
    href: "/accounting",
    label: "Accounting",
    icon: Landmark,
    section: "Reports",
    roles: [0, 1, 4],
  },

  {
    href: "/settings",
    label: "Settings",
    icon: Settings,
    section: "Settings",
    roles: [0, 1, 4],
  },
  {
    href: "/team",
    label: "Team",
    icon: Users,
    section: "Settings",
    roles: [0],
  },
  {
    href: "/tab-devices",
    label: "Tab Devices",
    icon: Tablet,
    section: "Settings",
    roles: [0, 1, 4, 5],
  },
  {
    href: "/audit",
    label: "Activity log",
    icon: ScrollText,
    section: "Settings",
    roles: [0, 1, 4],
  },
  {
    href: "/data-import",
    label: "Data import",
    icon: Upload,
    section: "Settings",
    roles: [0, 1],
  },
];

const DEFAULT_OPEN_SECTIONS: Record<string, boolean> = {
  Dashboard: true,
  "Master Data": true,
  "Unit of Measure": false,
  "Tax & Service Charge": false,
  Transactions: false,
  "Customer Master": false,
  Promotion: false,
  Reports: false,
  Settings: false,
};

export function Sidebar() {
  const pathname = usePathname();
  const router = useRouter();
  const [session, setSession] = useState<Session | null>(null);
  const [outlets, setOutlets] = useState<Outlet[]>([]);
  const [activeLoc, setActiveLoc] = useState<Outlet | null>(null);
  const [kdsEnabled, setKdsEnabled] = useState(true); // hide the KOT board if a venue opts out
  const [features, setFeatures] = useState<Record<string, boolean> | null>(
    null,
  ); // #6 plan feature map
  const [planCode, setPlanCode] = useState<string | null>(null);
  const [denied, setDenied] = useState<Set<string>>(new Set()); // per-role screens an owner hid (#71)
  const [acctOpen, setAcctOpen] = useState(false); // account menu on the workspace dropdown
  const [myFloorsOpen, setMyFloorsOpen] = useState(false); // self-service floor coverage (#floor-push)
  const [outletModal, setOutletModal] = useState(false); // searchable branch switcher (scales to many outlets)
  const [outletQuery, setOutletQuery] = useState("");
  const [logoUrl, setLogoUrl] = useState<string | null>(null);
  const [openSections, setOpenSections] = useState<Record<string, boolean>>(
    DEFAULT_OPEN_SECTIONS,
  );

  function toggleSection(section: string) {
    setOpenSections((prev) => ({ ...prev, [section]: !prev[section] }));
  }

  // Pulls the projected POS config (KDS on/off, logo, feature flags, plan) — split out
  // so it can be re-run on mount AND whenever Settings broadcasts a change, without a
  // full page reload (#kds-live-refresh).
  function loadPosConfig() {
    apiClient<{
      kdsEnabled: boolean;
      logoUrl: string | null;
      taxLabel?: string;
      features?: Record<string, boolean>;
      planCode?: string | null;
    }>("/api/v1/pos/config")
      .then((c) => {
        setKdsEnabled(c.kdsEnabled !== false);
        setLogoUrl(c.logoUrl ?? null);
        setFeatures(c.features ?? null);
        setPlanCode(c.planCode ?? null);
        if (c.taxLabel) {
          try {
            localStorage.setItem("hms.taxLabel", c.taxLabel);
          } catch {
            /* ignore */
          }
        }
      })
      .catch(() => {});
  }

  useEffect(() => {
    const t = localStorage.getItem("hms.tenant");
    const u = localStorage.getItem("hms.user");
    if (t && u) setSession({ tenant: JSON.parse(t), user: JSON.parse(u) });
    loadPosConfig();
    apiClient<{ denied: string[] }>("/api/v1/permissions/screens/me")
      .then((r) => setDenied(new Set(r.denied)))
      .catch(() => {});
    // Branch list + the currently-active outlet (persisted in hms.location).
    apiClient<Outlet[]>("/api/v1/locations")
      .then((ls) => {
        setOutlets(ls);
        // A branch-pinned user (non-Owner with a home outlet) is locked to that outlet.
        const usr = u
          ? (JSON.parse(u) as { role: number; homeLocationId?: string | null })
          : null;
        const pinned =
          usr && usr.role !== 0 && usr.homeLocationId
            ? ls.find((l) => l.id === usr.homeLocationId)
            : null;
        let cur: Outlet | null = null;
        try {
          const s = localStorage.getItem("hms.location");
          if (s) cur = JSON.parse(s);
        } catch {
          /* ignore */
        }
        const pick =
          pinned ??
          (cur && ls.find((l) => l.id === cur!.id)) ??
          ls.find((l) => l.code === "MAIN") ??
          ls[0] ??
          null;
        if (pick) {
          setActiveLoc(pick);
          localStorage.setItem(
            "hms.location",
            JSON.stringify({
              id: pick.id,
              code: pick.code,
              name: pick.name,
              locationType: pick.locationType,
              canSell: pick.canSell,
              operatingHours: pick.operatingHours ?? null,
            }),
          );
        }
      })
      .catch(() => {});
  }, []);

  // Settings saves broadcast this event (same tab) so the sidebar reflects a KDS
  // on/off flip (or logo/plan change) immediately instead of needing a manual refresh.
  useEffect(() => {
    window.addEventListener("hms:settings-updated", loadPosConfig);
    return () => window.removeEventListener("hms:settings-updated", loadPosConfig);
  }, []);

  // Whether the signed-in user is locked to a single outlet (no branch switch, HQ screens hidden).
  const isPinned =
    !!session &&
    session.user.role !== 0 &&
    session.user.role !== 5 &&
    !!session.user.homeLocationId;

  // Owners/Managers can switch the active branch; it drives POS + defaults elsewhere.
  function switchBranch(id: string) {
    const l = outlets.find((o) => o.id === id);
    if (!l) return;
    localStorage.setItem(
      "hms.location",
      JSON.stringify({
        id: l.id,
        code: l.code,
        name: l.name,
        locationType: l.locationType,
        canSell: l.canSell,
        operatingHours: l.operatingHours ?? null,
      }),
    );
    window.location.reload(); // simplest reliable way to re-scope every open screen
  }

  function signOut() {
    [
      "hms.token",
      "hms.refresh",
      "hms.tenant",
      "hms.user",
      "hms.location",
    ].forEach((k) => localStorage.removeItem(k));
    router.push("/login");
  }
  async function signOutConfirm() {
    setAcctOpen(false);
    if (
      !(await confirmDialog({
        title: "Sign out?",
        body: "You’ll be returned to the login screen and will need to sign in again.",
        confirmLabel: "Sign out",
        danger: true,
      }))
    )
      return;
    signOut();
  }

  const tenantName = session?.tenant.displayName ?? "Loading…";
  const initials = (session?.user.displayName ?? "U")
    .split(" ")
    .map((w) => w[0])
    .slice(0, 2)
    .join("")
    .toUpperCase();

  return (
    <aside className="flex h-screen w-[260px] flex-col border-r border-border bg-card">
      {/* Workspace / account menu */}
      <div className="relative border-b border-border">
        <button
          onClick={() => setAcctOpen((o) => !o)}
          className="flex w-full items-center gap-3 px-4 py-4 text-left hover:bg-muted"
        >
          {logoUrl ? (
            <img
              src={logoUrl}
              alt=""
              className="size-9 shrink-0 rounded-lg border border-border bg-white object-contain p-0.5"
            />
          ) : (
            <div className="flex size-9 items-center justify-center rounded-lg bg-primary text-sm font-bold text-primary-foreground">
              {tenantName.slice(0, 1)}
            </div>
          )}
          <div className="min-w-0 flex-1">
            <div className="truncate font-heading text-sm font-semibold">
              {tenantName}
            </div>
            <div className="text-xs text-muted-foreground">RIT HMS</div>
          </div>
          <ChevronDown
            className={cn(
              "size-4 text-muted-foreground transition-transform",
              acctOpen && "rotate-180",
            )}
          />
        </button>
        {acctOpen && (
          <>
            <div
              className="fixed inset-0 z-10"
              onClick={() => setAcctOpen(false)}
            />
            <div className="absolute left-3 right-3 top-[68px] z-20 rounded-lg border border-border bg-card py-1 shadow-lg">
              <div className="px-3 py-2 text-xs text-muted-foreground">
                Signed in as{" "}
                <span className="font-semibold text-foreground">
                  {session?.user.displayName ?? "—"}
                </span>{" "}
                · {ROLE_LABEL[session?.user.role ?? 2]}
              </div>
              <Link
                href="/settings"
                onClick={() => setAcctOpen(false)}
                className="flex items-center gap-2 px-3 py-2 text-sm hover:bg-muted"
              >
                <Settings className="size-4 text-muted-foreground" /> Settings
              </Link>
              {session?.user.isServer && (
                <button
                  onClick={() => { setAcctOpen(false); setMyFloorsOpen(true); }}
                  className="flex w-full items-center gap-2 px-3 py-2 text-left text-sm hover:bg-muted"
                >
                  <Layers className="size-4 text-muted-foreground" /> My Floor Coverage
                </button>
              )}
              <button
                onClick={() => void signOutConfirm()}
                className="flex w-full items-center gap-2 px-3 py-2 text-left text-sm text-status-error hover:bg-muted"
              >
                <LogOut className="size-4" /> Sign out
              </button>
            </div>
          </>
        )}
      </div>

      {myFloorsOpen && (
        <MyFloorsModal homeLocationId={session?.user.homeLocationId ?? null} onClose={() => setMyFloorsOpen(false)} />
      )}

      {/* Active outlet — Owners/Managers tap "Change" to open a searchable branch
          picker (scales to 100s of branches, unlike a native dropdown). */}
      <div className="flex items-center gap-2 border-b border-border px-4 py-2.5 text-sm">
        <MapPin className="size-4 shrink-0 text-muted-foreground" />
        <span className="min-w-0 flex-1 truncate font-medium text-foreground">
          {activeLoc?.name ?? "Main Outlet"}
        </span>
        {(session?.user.role === 0 ||
          session?.user.role === 1 ||
          session?.user.role === 5) &&
          outlets.length > 1 &&
          !isPinned && (
            <button
              onClick={() => {
                setOutletQuery("");
                setOutletModal(true);
              }}
              className="shrink-0 rounded-md px-2 py-0.5 text-xs font-semibold text-primary hover:bg-muted"
            >
              Change
            </button>
          )}
        {isPinned && (
          <span
            className="shrink-0 rounded-md bg-muted px-2 py-0.5 text-[10px] font-semibold text-muted-foreground"
            title="You're assigned to this outlet"
          >
            Your outlet
          </span>
        )}
      </div>

      {/* Nav — filtered to the user's role, grouped into collapsible sections */}
      <nav className="flex-1 space-y-1 overflow-y-auto p-3">
        {(() => {
          const role = session?.user.role ?? 2;
          const navRole = role === 5 ? 0 : role; // Admin sees the owner's full nav
          // Floor = dine-in table layout; only real outlets have one (a central kitchen / warehouse / HQ sells at a counter or not at all).
          const noDineIn =
            !!activeLoc?.locationType && activeLoc.locationType !== "outlet";
          const items = NAV.filter(
            (n) =>
              n.roles.includes(navRole) &&
              (kdsEnabled || n.href !== "/kot") &&
              !denied.has(n.href) &&
              !(isPinned && HQ_ONLY.has(n.href)) &&
              !(noDineIn && n.href === "/floor"),
          );

          const groupedItems = items.reduce<Record<string, NavItem[]>>(
            (groups, item) => {
              (groups[item.section] ??= []).push(item);
              return groups;
            },
            {},
          );

          function renderItem({ href, label, icon: Icon, newTab }: NavItem) {
            const active = pathname === href || pathname.startsWith(href + "/");
            const openNew = newTab?.includes(navRole) ?? false;
            const feat = FEATURE_OF[href];
            const locked =
              !!feat && features != null && features[feat] === false; // #6 Pro-only on Lite

            if (locked) {
              return (
                <button
                  key={href}
                  type="button"
                  onClick={async () => {
                    const planName = planCode
                      ? planCode.charAt(0).toUpperCase() + planCode.slice(1)
                      : "current";
                    const biz = session?.tenant.displayName ?? "your account";
                    if (
                      await confirmDialog({
                        title: `${label} isn’t part of your plan`,
                        body: (
                          <>
                            <p className="text-on-surface">
                              You’re on the{" "}
                              <span className="font-semibold">{planName}</span>{" "}
                              plan, which doesn’t include {label}.
                            </p>
                            <hr className="my-3 border-border" />
                            <p className="text-xs">
                              Move to a plan that has it whenever you like —
                              everything in {biz} stays exactly as it is.
                            </p>
                          </>
                        ),
                        confirmLabel: "See plans",
                        cancelLabel: "Close",
                      })
                    )
                      router.push("/settings?billing=1");
                  }}
                  className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-muted-foreground transition-colors hover:bg-muted"
                >
                  <Icon className="size-[18px]" strokeWidth={1.75} />
                  <span className="flex-1 text-left">{label}</span>
                  <span className="rounded bg-amber-100 px-1.5 py-0.5 text-[10px] font-bold uppercase text-amber-700">
                    Pro
                  </span>
                  <Lock className="size-3.5" />
                </button>
              );
            }
            return (
              <Link
                key={href}
                href={href}
                target={openNew ? "_blank" : undefined}
                rel={openNew ? "noopener noreferrer" : undefined}
                className={cn(
                  "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                  active
                    ? "bg-primary-tint text-primary"
                    : "text-foreground hover:bg-muted",
                )}
              >
                <Icon
                  className="size-[18px]"
                  strokeWidth={active ? 2.25 : 1.75}
                />
                <span className="flex-1">{label}</span>
                {openNew && (
                  <ExternalLink className="size-3.5 text-muted-foreground" />
                )}
              </Link>
            );
          }

          return Object.entries(groupedItems).map(([section, sectionItems]) => {
            // Dashboard is a single top-level link, not a collapsible group.
            if (section === "Dashboard") return renderItem(sectionItems[0]);

            const sectionHasActiveItem = sectionItems.some(
              (n) => pathname === n.href || pathname.startsWith(n.href + "/"),
            );
            const isOpen = openSections[section] ?? sectionHasActiveItem;

            return (
              <div key={section} className="space-y-0.5">
                <button
                  type="button"
                  onClick={() => toggleSection(section)}
                  className={cn(
                    "flex w-full items-center gap-2 rounded-md px-3 pb-1 pt-3 text-left text-[11px] font-bold uppercase tracking-wider transition-colors first:pt-0",
                    sectionHasActiveItem
                      ? "text-primary"
                      : "text-muted-foreground hover:text-foreground",
                  )}
                >
                  <span className="flex-1">{section}</span>
                  {isOpen ? (
                    <ChevronDown className="size-3.5" />
                  ) : (
                    <ChevronRight className="size-3.5" />
                  )}
                </button>
                {isOpen && (
                  <div className="space-y-0.5">
                    {sectionItems.map(renderItem)}
                  </div>
                )}
              </div>
            );
          });
        })()}
      </nav>

      {/* Account chip */}
      <div className="flex items-center gap-3 border-t border-border px-4 py-3">
        <div className="flex size-8 items-center justify-center rounded-full bg-muted text-xs font-semibold text-muted-foreground">
          {initials}
        </div>
        <div className="min-w-0 flex-1">
          <div className="truncate text-sm font-medium">
            {session?.user.displayName ?? "—"}
          </div>
          <span className="pill pill-idle">
            {ROLE_LABEL[session?.user.role ?? 2]}
          </span>
        </div>
        <button
          onClick={() => void signOutConfirm()}
          title="Sign out"
          className="text-muted-foreground hover:text-status-error"
        >
          <LogOut className="size-4" />
        </button>
      </div>

      {/* Branch switcher — searchable list; scales past a dropdown for chains with many outlets */}
      {outletModal && (
        <div
          className="fixed inset-0 z-[80] flex items-start justify-center bg-black/60 backdrop-blur-sm p-4 pt-[12vh]"
          onClick={() => setOutletModal(false)}
        >
          <div
            className="flex max-h-[70vh] w-full max-w-md flex-col overflow-hidden rounded-xl bg-card shadow-2xl"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-center justify-between bg-primary px-4 py-3 text-primary-foreground">
              <h3 className="font-heading text-base font-bold">
                Switch branch
              </h3>
              <button
                onClick={() => setOutletModal(false)}
                className="rounded-lg p-1 text-primary-foreground/80 transition-colors hover:bg-white/15 hover:text-white"
              >
                <X className="size-5" />
              </button>
            </div>
            <div className="flex items-center gap-2 border-b border-border px-4 py-2.5">
              <Search className="size-4 shrink-0 text-muted-foreground" />
              <input
                autoFocus
                value={outletQuery}
                onChange={(e) => setOutletQuery(e.target.value)}
                placeholder="Filter by name or code…"
                className="w-full bg-transparent text-sm outline-none placeholder:text-muted-foreground"
              />
            </div>
            <div className="overflow-y-auto overscroll-contain p-2">
              {outlets
                .filter((o) => {
                  const q = outletQuery.trim().toLowerCase();
                  return (
                    !q ||
                    o.name.toLowerCase().includes(q) ||
                    o.code.toLowerCase().includes(q)
                  );
                })
                .map((o) => {
                  const current = o.id === activeLoc?.id;
                  return (
                    <button
                      key={o.id}
                      disabled={current}
                      onClick={() => switchBranch(o.id)}
                      className={cn(
                        "flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-left",
                        current ? "bg-primary-tint" : "hover:bg-muted",
                      )}
                    >
                      <MapPin
                        className={cn(
                          "size-4 shrink-0",
                          current ? "text-primary" : "text-muted-foreground",
                        )}
                      />
                      <span className="min-w-0 flex-1">
                        <span
                          className={cn(
                            "block truncate text-sm font-semibold",
                            current && "text-primary-dark",
                          )}
                        >
                          {o.name}
                        </span>
                        <span className="block truncate text-xs text-muted-foreground">
                          {o.code}
                        </span>
                      </span>
                      {current ? (
                        <span className="flex items-center gap-1 text-xs font-semibold text-primary">
                          <Check className="size-3.5" /> Current
                        </span>
                      ) : (
                        <span className="text-xs font-semibold text-primary">
                          Switch
                        </span>
                      )}
                    </button>
                  );
                })}
              {outlets.filter((o) => {
                const q = outletQuery.trim().toLowerCase();
                return (
                  !q ||
                  o.name.toLowerCase().includes(q) ||
                  o.code.toLowerCase().includes(q)
                );
              }).length === 0 && (
                <p className="px-3 py-8 text-center text-sm text-muted-foreground">
                  No branches match “{outletQuery.trim()}”.
                </p>
              )}
            </div>
          </div>
        </div>
      )}
    </aside>
  );
}

/** Self-service floor coverage (#floor-push): a steward picks which floor(s) of THEIR
 * outlet they're covering right now, so guest-order push/notifications reach them for
 * the right tables. Same underlying data as the Team screen's per-user picker, but this
 * one operates on "me" — no admin role needed. */
function MyFloorsModal({ homeLocationId, onClose }: { homeLocationId: string | null; onClose: () => void }) {
  const [floors, setFloors] = useState<{ id: string; name: string }[]>([]);
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!homeLocationId) { setLoading(false); return; }
    Promise.all([
      apiClient<{ id: string; name: string }[]>(`/api/v1/floors?locationId=${homeLocationId}`),
      apiClient<string[]>('/api/v1/me/floors'),
    ]).then(([opts, mine]) => { setFloors(opts); setSelected(new Set(mine)); })
      .catch(() => setError('Could not load floors.'))
      .finally(() => setLoading(false));
  }, [homeLocationId]);

  async function save() {
    setSaving(true);
    try {
      await apiClient('/api/v1/me/floors', { method: 'PUT', body: JSON.stringify({ floorIds: Array.from(selected) }) });
      onClose();
    } catch { setError('Could not save your floor coverage.'); }
    finally { setSaving(false); }
  }

  return (
    <Modal
      title="My Floor Coverage"
      icon={<Layers className="size-4" />}
      onClose={() => !saving && onClose()}
      size="sm"
      footer={
        <div className="flex gap-2">
          <button onClick={onClose} disabled={saving} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted disabled:opacity-50">Cancel</button>
          <button onClick={save} disabled={saving || !homeLocationId} className="h-11 flex-1 rounded-lg bg-primary font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{saving ? 'Saving…' : 'Save'}</button>
        </div>
      }
    >
      {!homeLocationId ? (
        <p className="text-sm text-muted-foreground">Ask your manager to pin you to an outlet first — floors belong to a specific outlet.</p>
      ) : loading ? (
        <div className="h-24 animate-pulse rounded bg-muted" />
      ) : error ? (
        <p className="text-sm text-status-error">{error}</p>
      ) : (
        <>
          <p className="mb-3 text-sm text-muted-foreground">Which floors are you covering? Leave all unchecked to be notified for every floor.</p>
          {floors.length === 0 ? (
            <p className="text-sm text-muted-foreground">No floors set up for your outlet yet — ask your manager to add some from Floor → Manage Tables.</p>
          ) : (
            <div className="space-y-1.5">
              {floors.map(f => (
                <label key={f.id} className="flex items-center gap-2.5 rounded-lg border border-border px-3 py-2 text-sm">
                  <input type="checkbox" checked={selected.has(f.id)}
                    onChange={e => setSelected(prev => { const n = new Set(prev); if (e.target.checked) n.add(f.id); else n.delete(f.id); return n; })}
                    className="size-4 rounded border-border text-primary focus:ring-primary" />
                  {f.name}
                </label>
              ))}
            </div>
          )}
        </>
      )}
    </Modal>
  );
}
