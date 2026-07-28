'use client';

import { useEffect, useMemo, useState } from 'react';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient, money } from '@/lib/api-client';
import { Icon } from '@/components/ui/Icon';
import { Field, Combobox } from '@/components/ui/form';
import { HeaderStat, Num } from '@/components/ui/HeaderStat';
import { useTaxLabel } from '@/lib/use-tax-label';
import { Pagination } from '@/components/ui/Pagination';

type Location = { id: string; code: string; name: string; city: string; currency: string };

type SalesByDay = { day: string; orderCount: number; total: number };
type SalesBySource = { source: string; orderCount: number; total: number };
type SalesTopItem = { productId: string; sku: string | null; productName: string; quantity: number; revenue: number };

type OutletRow = { locationId: string; code: string; name: string; orderCount: number; grossSales: number; netSales: number; tax: number; stockValue: number };
type OutletReport = { outlets: OutletRow[]; totalOrders: number; totalGross: number; totalNet: number; totalTax: number; totalStockValue: number };

type DailySalesRow = { date: string; locationId: string; locationCode: string; locationName: string; receipts: number; gross: number; discount: number; serviceCharge: number; tax: number; net: number };
type DailySalesSummary = { periodFrom: string; periodTo: string; rows: DailySalesRow[]; totals: { receipts: number; gross: number; discount: number; serviceCharge: number; tax: number; net: number } };

type DailySalesDetailRow = {
  id: string; date: string; settledAt: string; locationId: string; locationCode: string; locationName: string;
  number: string; orderType: string; tableLabel: string | null; customerName: string | null;
  gross: number; discount: number; serviceCharge: number; tax: number; net: number;
};
type DailySalesDetail = { periodFrom: string; periodTo: string; rows: DailySalesDetailRow[]; totals: { receipts: number; gross: number; discount: number; serviceCharge: number; tax: number; net: number }; pagination: { totalCount: number; pageNumber: number; pageSize: number; totalPages: number } };

type SalesSummary = {
  periodFrom: string;
  periodTo: string;
  orderCount: number;
  grossSales: number;
  netSales: number;
  discount: number;
  serviceCharge: number;
  tax: number;
  byDay: SalesByDay[];
  bySource: SalesBySource[];
  topItems: SalesTopItem[];
};

type VatCharge = {
  code: string;
  name: string;
  chargeType: string;
  ratePercent: number;
  baseAmount: number;
  chargeAmount: number;
};

type VatSummary = {
  periodFrom: string;
  periodTo: string;
  invoiceCount: number;
  netSales: number;
  serviceCharge: number;
  outputVat: number;
  grnCount: number;
  purchasesExVat: number;
  inputVat: number;
  netVatPayable: number;
  charges: VatCharge[];
};

// ── #72 reports library ──
type RegisterRow = { id: string; settledAt: string; number: string; orderType: string; orderSource: string; tableLabel: string | null; customerName: string | null; subtotalAmount: number; discountAmount: number; promotionDiscountAmount: number; serviceChargeAmount: number; taxAmount: number; totalAmount: number };
type TenderRow = { payType: string; amount: number };
type PageMeta = { totalCount: number; pageNumber: number; pageSize: number; totalPages: number };
type SalesRegister = { periodFrom: string; periodTo: string; count: number; totals: { subtotal: number; discount: number; serviceCharge: number; tax: number; total: number }; tenders: TenderRow[]; orders: RegisterRow[]; pagination: PageMeta };
type ItemRow = { productId: string; sku: string | null; productName: string; quantity: number; revenue: number; tax: number };
type ItemSales = { items: ItemRow[]; totalQty: number; totalRevenue: number };
type StockRow = { productId: string; sku: string; name: string; location: string; onHand: number; avgCost: number; value: number };
type StockBalance = { asAt: string; lines: StockRow[]; totalValue: number; pagination: PageMeta };
type ShiftRow = { shiftNumber: string; openedByName: string | null; openedAt: string; closedAt: string | null; status: string; totalSales: number; cashSales: number; cardSales: number; otherSales: number; orderCount: number; expectedCash: number | null; declaredCash: number | null; cashVariance: number | null };
type ShiftSettlement = { shifts: ShiftRow[]; totalSales: number; totalVariance: number; pagination: { totalCount: number; pageNumber: number; pageSize: number; totalPages: number } };
type PromoRow = { code: string; name: string; times: number; discount: number };
type PromotionUsage = { promotions: PromoRow[]; totalDiscount: number; pagination: PageMeta };
type StewardRow = { stewardId: string | null; name: string; orderCount: number; covers: number; grossSales: number; tips: number };
type TourCommissionRow = { tourOperatorId: string; code: string; name: string; orderCount: number; grossSales: number; commission: number };
type DiscountRow = { stewardId: string | null; stewardName: string; billCount: number; grossSales: number; discountTotal: number; discountPercent: number };
type DiscountReport = { periodFrom: string; periodTo: string; rows: DiscountRow[]; totals: { billCount: number; grossSales: number; discountTotal: number }; pagination: PageMeta };
type TableTurnoverRow = { tableLabel: string; billCount: number; totalCovers: number; avgDurationMinutes: number; grossSales: number };
type TableTurnoverReport = { periodFrom: string; periodTo: string; rows: TableTurnoverRow[]; totals: { billCount: number; totalCovers: number; avgDurationMinutes: number; grossSales: number }; pagination: PageMeta };
type CategorySalesRow = { categoryId: string | null; categoryCode: string; categoryName: string; quantity: number; revenue: number; tax: number };
type CategorySalesReport = { periodFrom: string; periodTo: string; rows: CategorySalesRow[]; totalQty: number; totalRevenue: number; pagination: PageMeta };
type PagedListResult<T> = { data: T[]; pagination: PageMeta };

const PAY_LABEL: Record<string, string> = { cash: 'Cash', card: 'Card', credit: 'Credit (A/C)', ubereats_prepaid: 'Uber Eats', pickme_prepaid: 'PickMe' };

const SOURCE_LABEL: Record<string, string> = {
  pos: 'POS',
  ubereats: 'Uber Eats',
  pickme: 'PickMe',
};

function sourceLabel(s: string): string {
  return SOURCE_LABEL[s] ?? s;
}

/** Build a CSV from a header row + body rows and trigger a browser download. */
function downloadCsv(filename: string, head: string[], body: (string | number)[][]) {
  const esc = (v: string) => `"${v.replace(/"/g, '""')}"`;
  const csv = [...(head.length ? [head] : []), ...body].map(row => row.map(x => esc(String(x))).join(',')).join('\n');
  const url = URL.createObjectURL(new Blob([csv], { type: 'text/csv' }));
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  a.click();
  URL.revokeObjectURL(url);
}

type PdfSection = {
  heading?: string;
  head?: string[];
  rows?: (string | number)[][];
  kpis?: { label: string; value: string }[];
  boldLastRow?: boolean;
};

/** RIT HMS brand palette for print output — mirrors tailwind.config.ts (primary green #15803d). */
const PDF_BRAND = {
  primary: [21, 128, 61] as [number, number, number],
  primaryDark: [0, 101, 44] as [number, number, number],
  ink: [18, 28, 42] as [number, number, number],
  muted: [100, 116, 139] as [number, number, number],
  border: [226, 232, 240] as [number, number, number],
  zebra: [246, 249, 247] as [number, number, number],
  kpiFill: [248, 250, 255] as [number, number, number],
};

/** Best-effort tenant name for the print letterhead — same source Sidebar.tsx uses for the workspace label. */
function getTenantName(): string {
  try {
    const raw = localStorage.getItem('hms.tenant');
    if (raw) {
      const t = JSON.parse(raw);
      if (t?.displayName) return t.displayName as string;
    }
  } catch { /* localStorage unavailable / malformed — fall back below */ }
  return 'RIT HMS';
}

function isNumericCell(v: string | number): boolean {
  if (typeof v === 'number') return true;
  const t = v.trim();
  if (t === '') return false;
  return /^-?[\d,]+(\.\d+)?%?$/.test(t);
}

/** Draw a grid of stat cards (KPI tiles) at (x, y) with the given max width; returns the height consumed. */
function drawKpiGrid(doc: jsPDF, kpis: { label: string; value: string }[], x: number, y: number, width: number): number {
  const perRow = 3;
  const gap = 4;
  const boxW = (width - gap * (perRow - 1)) / perRow;
  const boxH = 15;
  kpis.forEach((k, i) => {
    const col = i % perRow;
    const row = Math.floor(i / perRow);
    const bx = x + col * (boxW + gap);
    const by = y + row * (boxH + gap);
    doc.setFillColor(...PDF_BRAND.kpiFill);
    doc.setDrawColor(...PDF_BRAND.border);
    doc.setLineWidth(0.3);
    doc.roundedRect(bx, by, boxW, boxH, 1.4, 1.4, 'FD');
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(6.8);
    doc.setTextColor(...PDF_BRAND.muted);
    doc.text(k.label.toUpperCase(), bx + 3.5, by + 5.5);
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(10.5);
    doc.setTextColor(...PDF_BRAND.ink);
    doc.text(k.value, bx + 3.5, by + 11.7);
  });
  const rows = Math.ceil(kpis.length / perRow);
  return rows * (boxH + gap);
}

function drawPdfFooter(doc: jsPDF, tenantName: string, page: number, totalPages: number) {
  const pageWidth = doc.internal.pageSize.getWidth();
  const pageHeight = doc.internal.pageSize.getHeight();
  doc.setDrawColor(...PDF_BRAND.border);
  doc.setLineWidth(0.3);
  doc.line(14, pageHeight - 13, pageWidth - 14, pageHeight - 13);
  doc.setFont('helvetica', 'normal');
  doc.setFontSize(7.5);
  doc.setTextColor(...PDF_BRAND.muted);
  doc.text(`${tenantName} - Generated by RIT HMS`, 14, pageHeight - 8);
  doc.text(`Page ${page} of ${totalPages}`, pageWidth - 14, pageHeight - 8, { align: 'right' });
}

/** Build a branded, letterhead-style report PDF (title + subtitle + KPI cards / tables) and trigger a download. */
function downloadPdf(filename: string, title: string, subtitle: string, sections: PdfSection[]) {
  const doc = new jsPDF();
  const pageWidth = doc.internal.pageSize.getWidth();
  const pageHeight = doc.internal.pageSize.getHeight();
  const marginX = 14;
  const contentWidth = pageWidth - marginX * 2;
  const tenantName = getTenantName();

  doc.setFillColor(...PDF_BRAND.primary);
  doc.rect(0, 0, pageWidth, 2.6, 'F');

  doc.setFont('helvetica', 'bold');
  doc.setFontSize(12.5);
  doc.setTextColor(...PDF_BRAND.ink);
  doc.text(tenantName, marginX, 15);

  doc.setFont('helvetica', 'normal');
  doc.setFontSize(8);
  doc.setTextColor(...PDF_BRAND.muted);
  doc.text('RIT HMS - Hospitality Management', marginX, 20);

  const generated = new Date().toLocaleString('en-LK', { dateStyle: 'medium', timeStyle: 'short' });
  doc.setFontSize(8);
  doc.text(`Generated ${generated}`, pageWidth - marginX, 15, { align: 'right' });

  doc.setDrawColor(...PDF_BRAND.border);
  doc.setLineWidth(0.5);
  doc.line(marginX, 24, pageWidth - marginX, 24);

  doc.setFont('helvetica', 'bold');
  doc.setFontSize(16);
  doc.setTextColor(...PDF_BRAND.ink);
  doc.text(title, marginX, 33);

  doc.setFont('helvetica', 'normal');
  doc.setFontSize(9.5);
  doc.setTextColor(...PDF_BRAND.muted);
  doc.text(subtitle, marginX, 39.5);

  let y = 47;

  for (const sec of sections) {
    if (sec.kpis && sec.kpis.length > 0) {
      if (y > pageHeight - 40) { doc.addPage(); y = 16; }
      y += drawKpiGrid(doc, sec.kpis, marginX, y, contentWidth) + 6;
      continue;
    }
    if (!sec.rows || sec.rows.length === 0) continue;
    if (sec.heading) {
      if (y > pageHeight - 30) { doc.addPage(); y = 16; }
      doc.setFillColor(...PDF_BRAND.primary);
      doc.rect(marginX, y - 3.3, 2, 4, 'F');
      doc.setFont('helvetica', 'bold');
      doc.setFontSize(9.5);
      doc.setTextColor(...PDF_BRAND.ink);
      doc.text(sec.heading.toUpperCase(), marginX + 4.5, y);
      y += 5;
    }
    const colCount = sec.head ? sec.head.length : sec.rows[0].length;
    const columnStyles: Record<number, { halign: 'right' }> = {};
    for (let c = 0; c < colCount; c++) {
      if (sec.rows.every(r => isNumericCell(r[c] ?? ''))) columnStyles[c] = { halign: 'right' };
    }
    const lastRowIndex = sec.rows.length - 1;
    autoTable(doc, {
      startY: y,
      head: sec.head ? [sec.head] : undefined,
      body: sec.rows,
      margin: { left: marginX, right: marginX, bottom: 18 },
      styles: { fontSize: 8, cellPadding: 2.3, textColor: PDF_BRAND.ink, lineColor: PDF_BRAND.border, lineWidth: 0.1 },
      headStyles: sec.head ? { fillColor: PDF_BRAND.primary, textColor: 255, fontStyle: 'bold', fontSize: 8 } : undefined,
      alternateRowStyles: { fillColor: PDF_BRAND.zebra },
      columnStyles,
      theme: sec.head ? 'striped' : 'plain',
      didParseCell: data => {
        if (sec.boldLastRow && data.section === 'body' && data.row.index === lastRowIndex) {
          data.cell.styles.fontStyle = 'bold';
          data.cell.styles.fillColor = [241, 245, 249];
        }
      },
    });
    y = (doc as unknown as { lastAutoTable: { finalY: number } }).lastAutoTable.finalY + 9;
  }

  const totalPages = doc.internal.pages.length - 1;
  for (let p = 1; p <= totalPages; p++) {
    doc.setPage(p);
    drawPdfFooter(doc, tenantName, p, totalPages);
  }

  doc.save(filename);
}

function ExportButton({ onCsv, onPdf, disabled }: { onCsv: () => void; onPdf: () => void; disabled?: boolean }) {
  return (
    <div className="flex items-center gap-1.5">
      <button
        onClick={onCsv}
        disabled={disabled}
        className="flex items-center gap-1.5 rounded-lg border border-border bg-card px-3 py-1.5 text-sm font-medium hover:bg-muted disabled:opacity-50"
      >
        <Icon name="download" className="text-base" /> CSV
      </button>
      <button
        onClick={onPdf}
        disabled={disabled}
        className="flex items-center gap-1.5 rounded-lg border border-border bg-card px-3 py-1.5 text-sm font-medium hover:bg-muted disabled:opacity-50"
      >
        <Icon name="picture_as_pdf" className="text-base" /> PDF
      </button>
    </div>
  );
}

/** Format a Date as YYYY-MM-DD in local time (avoids UTC offset drift). */
function fmtDate(d: Date): string {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
}

/** Format a duration in minutes as "1h 37m" (or "42m" under an hour). */
function fmtDuration(minutes: number): string {
  const h = Math.floor(minutes / 60);
  const m = Math.round(minutes % 60);
  return h > 0 ? `${h}h ${m}m` : `${m}m`;
}

function defaultFrom(): string {
  const now = new Date();
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-01`;
}

function defaultTo(): string {
  return fmtDate(new Date());
}

// ── Reports catalog: Section → named reports within it ──
type ReportKey =
  | 'mdProducts' | 'mdSuppliers' | 'mdCustomers' | 'mdLocations'
  | 'salesOverview' | 'salesDaily' | 'salesDailyDetail' | 'salesRegister' | 'salesItems' | 'salesByCategory' | 'salesOutlets' | 'salesPromotions' | 'salesDiscounts' | 'salesTableTurnover' | 'salesStewards' | 'salesTourCommission' | 'salesVoids'
  | 'taxReturn'
  | 'invStock' | 'invWastage' | 'invBinCard' | 'invPurchasesBySupplier' | 'invLowStock' | 'invSlowMoving'
  | 'opsShifts'
  | 'costFood' | 'costBudget';

type SectionKey = 'masterData' | 'sales' | 'tax' | 'inventory' | 'operations' | 'costing';

export default function ReportsPage() {
  const [locations, setLocations] = useState<Location[]>([]);
  const [sales, setSales] = useState<SalesSummary | null>(null);
  const [vat, setVat] = useState<VatSummary | null>(null);
  const [outlets, setOutlets] = useState<OutletReport | null>(null);
  const [dailySummary, setDailySummary] = useState<DailySalesSummary | null>(null);
  const [items, setItems] = useState<ItemSales | null>(null);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [toast, setToast] = useState<string | null>(null);

  // filter state
  const [from, setFrom] = useState<string>(defaultFrom());
  const [to, setTo] = useState<string>(defaultTo());
  const [locationId, setLocationId] = useState<string>('');
  const [filterError, setFilterError] = useState<string | null>(null);

  const taxLabel = useTaxLabel();

  // Section → reports catalog (closes over taxLabel so the Tax report name matches the tenant's tax terminology).
  const SECTIONS: { key: SectionKey; label: string; reports: { key: ReportKey; label: string }[] }[] = [
    {
      key: 'masterData', label: 'Master Data', reports: [
        { key: 'mdProducts', label: 'Products' },
        { key: 'mdSuppliers', label: 'Suppliers' },
        { key: 'mdCustomers', label: 'Customers' },
        { key: 'mdLocations', label: 'Locations' },
      ],
    },
    {
      key: 'sales', label: 'Sales', reports: [
        { key: 'salesOverview', label: 'Overview' },
        { key: 'salesDaily', label: 'Daily Summary' },
        { key: 'salesDailyDetail', label: 'Daily Summary Detail' },
        { key: 'salesRegister', label: 'Register' },
        { key: 'salesItems', label: 'Item Sales' },
        { key: 'salesByCategory', label: 'Sales by Category' },
        { key: 'salesOutlets', label: 'By Outlet' },
        { key: 'salesPromotions', label: 'Promotions' },
        { key: 'salesDiscounts', label: 'Discount & Complimentary' },
        { key: 'salesTableTurnover', label: 'Table Turnover' },
        { key: 'salesStewards', label: 'Steward Sales' },
        { key: 'salesTourCommission', label: 'Tour Commission' },
        { key: 'salesVoids', label: 'Void / Cancellation' },
      ],
    },
    { key: 'tax', label: 'Tax', reports: [{ key: 'taxReturn', label: `${taxLabel} Return` }] },
    {
      key: 'inventory', label: 'Inventory', reports: [
        { key: 'invStock', label: 'Stock Balance' },
        { key: 'invWastage', label: 'Wastage' },
        { key: 'invBinCard', label: 'Bin Card' },
        { key: 'invPurchasesBySupplier', label: 'Purchases by Supplier' },
        { key: 'invLowStock', label: 'Low Stock / Reorder' },
        { key: 'invSlowMoving', label: 'Slow-Moving Stock' },
      ],
    },
    { key: 'operations', label: 'Operations', reports: [{ key: 'opsShifts', label: 'Shift Settlement' }] },
    {
      key: 'costing', label: 'Costing', reports: [
        { key: 'costFood', label: 'Food Costing' },
        { key: 'costBudget', label: 'Budget vs Sales' },
      ],
    },
  ];

  const [section, setSection] = useState<SectionKey>('sales');
  const [report, setReport] = useState<ReportKey | null>('salesOverview');
  const activeSection = SECTIONS.find(s => s.key === section)!;

  function selectSection(key: SectionKey) {
    setSection(key);
    const sec = SECTIONS.find(s => s.key === key)!;
    setReport(sec.reports[0]?.key ?? null);
  }

  function flash(msg: string) {
    setToast(msg);
    window.setTimeout(() => setToast(null), 3500);
  }

  async function run(f: string, t: string, loc: string) {
    setLoading(true);
    setError(null);
    try {
      const salesQuery = `?from=${f}&to=${t}${loc ? `&locationId=${loc}` : ''}`;
      const vatQuery = `?from=${f}&to=${t}`;
      const [s, v, o, ds, it] = await Promise.all([
        apiClient<SalesSummary>(`/api/v1/reports/sales/summary${salesQuery}`),
        apiClient<VatSummary>(`/api/v1/reports/vat/summary${vatQuery}`),
        apiClient<OutletReport>(`/api/v1/reports/outlets/summary${vatQuery}`),
        apiClient<DailySalesSummary>(`/api/v1/reports/sales/daily-summary${salesQuery}`),
        apiClient<ItemSales>(`/api/v1/reports/sales/items${salesQuery}`),
      ]);
      setSales(s);
      setVat(v);
      setOutlets(o);
      setDailySummary(ds);
      setItems(it);
    } catch (e) {
      setError(extractError(e, 'Could not load reports.'));
    } finally {
      setLoading(false);
    }
  }

  // initial load: locations + both summaries for the default period
  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const l = await apiClient<Location[]>('/api/v1/locations');
        if (!cancelled) setLocations(l);
      } catch {
        /* locations are optional for the report; ignore */
      }
    })();
    void run(defaultFrom(), defaultTo(), '');
    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  function onRun() {
    if (!from || !to) {
      setFilterError('Both From and To dates are required.');
      return;
    }
    if (from > to) {
      setFilterError('From date must be on or before To date.');
      return;
    }
    setFilterError(null);
    flash('Running report…');
    void run(from, to, locationId);
  }

  function exportOverviewCsv() {
    if (!sales) return;
    const rows: (string | number)[][] = [
      ['Sales Overview'], ['Orders', sales.orderCount], ['Gross Sales', sales.grossSales.toFixed(2)],
      ['Net Sales', sales.netSales.toFixed(2)], ['Discount', sales.discount.toFixed(2)],
      ['Service Charge', sales.serviceCharge.toFixed(2)], ['Tax', sales.tax.toFixed(2)], [],
      ['By Source'], ['Source', 'Orders', 'Total'],
      ...sales.bySource.map(s => [sourceLabel(s.source), s.orderCount, s.total.toFixed(2)]), [],
      ['By Day'], ['Day', 'Orders', 'Total'],
      ...sales.byDay.map(d => [d.day.slice(0, 10), d.orderCount, d.total.toFixed(2)]), [],
      ['Top Items'], ['Product Code', 'Item', 'Qty', 'Revenue'],
      ...sales.topItems.map(it => [it.sku ?? '', it.productName, it.quantity, it.revenue.toFixed(2)]),
    ];
    downloadCsv(`sales_overview_${from}_to_${to}.csv`, [], rows);
  }

  function exportOverviewPdf() {
    if (!sales) return;
    downloadPdf(`sales_overview_${from}_to_${to}.pdf`, 'Sales Overview', `Period ${from} to ${to} - amounts in LKR`, [
      {
        kpis: [
          { label: 'Orders', value: String(sales.orderCount) }, { label: 'Gross Sales', value: money(sales.grossSales) },
          { label: 'Net Sales', value: money(sales.netSales) }, { label: 'Discount', value: money(sales.discount) },
          { label: 'Service Charge', value: money(sales.serviceCharge) }, { label: 'Tax', value: money(sales.tax) },
        ],
      },
      { heading: 'By Source', head: ['Source', 'Orders', 'Total'], rows: sales.bySource.map(s => [sourceLabel(s.source), s.orderCount, s.total.toFixed(2)]) },
      { heading: 'By Day', head: ['Day', 'Orders', 'Total'], rows: sales.byDay.map(d => [d.day.slice(0, 10), d.orderCount, d.total.toFixed(2)]) },
      { heading: 'Top Items', head: ['Product Code', 'Item', 'Qty', 'Revenue'], rows: sales.topItems.map(it => [it.sku ?? '', it.productName, it.quantity, it.revenue.toFixed(2)]) },
    ]);
  }

  function exportDailySummaryCsv() {
    if (!dailySummary || dailySummary.rows.length === 0) return;
    downloadCsv(
      `daily_sales_summary_${from}_to_${to}.csv`,
      ['Date', 'Location', 'Receipts', 'Gross', 'Discount', 'Service Charge', 'Tax', 'Net'],
      dailySummary.rows.map(r => [
        r.date.slice(0, 10), `${r.locationCode} — ${r.locationName}`, r.receipts,
        r.gross.toFixed(2), r.discount.toFixed(2), r.serviceCharge.toFixed(2), r.tax.toFixed(2), r.net.toFixed(2),
      ]),
    );
  }

  function exportDailySummaryPdf() {
    if (!dailySummary || dailySummary.rows.length === 0) return;
    downloadPdf(`daily_sales_summary_${from}_to_${to}.pdf`, 'Daily Sales Summary', `Period ${from} to ${to} - amounts in LKR`, [{
      head: ['Date', 'Location', 'Receipts', 'Gross', 'Discount', 'Svc Chg', 'Tax', 'Net'],
      rows: dailySummary.rows.map(r => [
        r.date.slice(0, 10), `${r.locationCode} - ${r.locationName}`, r.receipts,
        r.gross.toFixed(2), r.discount.toFixed(2), r.serviceCharge.toFixed(2), r.tax.toFixed(2), r.net.toFixed(2),
      ]),
    }]);
  }

  function exportItemSalesCsv() {
    if (!items || items.items.length === 0) return;
    downloadCsv(
      `item_sales_${from}_to_${to}.csv`,
      ['Product Code', 'Item', 'Qty', 'Revenue'],
      items.items.map(it => [it.sku ?? '', it.productName, it.quantity, it.revenue.toFixed(2)]),
    );
  }

  function exportItemSalesPdf() {
    if (!items || items.items.length === 0) return;
    downloadPdf(`item_sales_${from}_to_${to}.pdf`, 'Item Sales', `Period ${from} to ${to} - amounts in LKR`, [{
      head: ['Product Code', 'Item', 'Qty', 'Revenue'],
      rows: items.items.map(it => [it.sku ?? '', it.productName, it.quantity, it.revenue.toFixed(2)]),
    }]);
  }

  function exportOutletsCsv() {
    if (!outlets || outlets.outlets.length === 0) return;
    downloadCsv(
      `sales_by_outlet_${from}_to_${to}.csv`,
      ['Outlet Code', 'Outlet Name', 'Orders', 'Gross Sales', 'Net Sales', 'Tax', 'Stock Value'],
      [
        ...outlets.outlets.map(o => [o.code, o.name, o.orderCount, o.grossSales.toFixed(2), o.netSales.toFixed(2), o.tax.toFixed(2), o.stockValue.toFixed(2)]),
        ['Group total', '', outlets.totalOrders, outlets.totalGross.toFixed(2), outlets.totalNet.toFixed(2), outlets.totalTax.toFixed(2), outlets.totalStockValue.toFixed(2)],
      ],
    );
  }

  function exportOutletsPdf() {
    if (!outlets || outlets.outlets.length === 0) return;
    downloadPdf(`sales_by_outlet_${from}_to_${to}.pdf`, 'Sales by Outlet', `Period ${from} to ${to} - amounts in LKR`, [{
      head: ['Outlet Code', 'Outlet Name', 'Orders', 'Gross Sales', 'Net Sales', 'Tax', 'Stock Value'],
      rows: [
        ...outlets.outlets.map(o => [o.code, o.name, o.orderCount, o.grossSales.toFixed(2), o.netSales.toFixed(2), o.tax.toFixed(2), o.stockValue.toFixed(2)]),
        ['Group total', '', outlets.totalOrders, outlets.totalGross.toFixed(2), outlets.totalNet.toFixed(2), outlets.totalTax.toFixed(2), outlets.totalStockValue.toFixed(2)],
      ],
      boldLastRow: true,
    }]);
  }

  function exportTaxReturnCsv() {
    if (!vat) return;
    downloadCsv(`${taxLabel.toLowerCase()}_return_${from}_to_${to}.csv`, [], [
      [`Output ${taxLabel}`, vat.outputVat.toFixed(2)], [`Input ${taxLabel}`, vat.inputVat.toFixed(2)],
      [`Net ${taxLabel} Payable`, vat.netVatPayable.toFixed(2)],
      ['Tax Invoices', vat.invoiceCount], ['Net Sales', vat.netSales.toFixed(2)],
      ['GRNs', vat.grnCount], [`Purchases ex-${taxLabel}`, vat.purchasesExVat.toFixed(2)], [],
      ['Code', 'Name', 'Type', 'Rate %', 'Base', 'Charge'],
      ...vat.charges.map(c => [c.code, c.name, c.chargeType, c.ratePercent.toFixed(2), c.baseAmount.toFixed(2), c.chargeAmount.toFixed(2)]),
    ]);
  }

  function exportTaxReturnPdf() {
    if (!vat) return;
    downloadPdf(`${taxLabel.toLowerCase()}_return_${from}_to_${to}.pdf`, `${taxLabel} Return`, `Period ${from} to ${to} - amounts in LKR`, [
      {
        kpis: [
          { label: `Output ${taxLabel}`, value: money(vat.outputVat) }, { label: `Input ${taxLabel}`, value: money(vat.inputVat) },
          { label: `Net ${taxLabel} Payable`, value: money(vat.netVatPayable) },
          { label: 'Tax Invoices', value: String(vat.invoiceCount) }, { label: 'Net Sales', value: money(vat.netSales) },
          { label: 'GRNs', value: String(vat.grnCount) }, { label: `Purchases ex-${taxLabel}`, value: money(vat.purchasesExVat) },
        ],
      },
      { head: ['Code', 'Name', 'Type', 'Rate %', 'Base', 'Charge'], rows: vat.charges.map(c => [c.code, c.name, c.chargeType, c.ratePercent.toFixed(2), c.baseAmount.toFixed(2), c.chargeAmount.toFixed(2)]) },
    ]);
  }

  const maxDayTotal = useMemo(() => {
    if (!sales || sales.byDay.length === 0) return 0;
    return Math.max(...sales.byDay.map(d => d.total));
  }, [sales]);

  const dayFmt = (iso: string) =>
    new Date(iso).toLocaleDateString('en-LK', { month: 'short', day: 'numeric' });

  const vatReclaimable = (vat?.netVatPayable ?? 0) < 0;

  return (
    <>
      <Topbar title="Reports" subtitle="Understand your business at a glance — sales, tax, stock and shifts" />

      <div className="p-6 md:p-8">
        {/* Filter bar */}
        <div className="card mb-6 p-4">
          <div className="flex flex-wrap items-end gap-3">
            <Field label="From" type="date" value={from} onChange={setFrom} />
            <Field label="To" type="date" value={to} onChange={setTo} />
            <Combobox
              className="w-56"
              label="Location"
              value={locationId}
              onChange={setLocationId}
              placeholder="All locations"
              options={[
                { value: '', label: 'All locations' },
                ...locations.map(l => ({ value: l.id, label: `${l.code} — ${l.name}` })),
              ]}
            />
            <button
              onClick={onRun}
              disabled={loading}
              className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark disabled:opacity-50"
            >
              <Icon name="play_arrow" className="text-base" /> Run
            </button>
          </div>
          {filterError && <p className="mt-2 text-xs text-status-error">{filterError}</p>}
        </div>

        {/* Level 1 — sections */}
        <div className="mb-4 flex flex-wrap gap-2">
          {SECTIONS.map(s => (
            <button
              key={s.key}
              onClick={() => selectSection(s.key)}
              className={`rounded-lg px-4 py-2 text-sm font-semibold transition-colors ${
                section === s.key ? 'bg-primary text-primary-foreground' : 'border border-border bg-card text-muted-foreground hover:text-on-surface'
              }`}
            >
              {s.label}
            </button>
          ))}
        </div>

        {/* Level 2 — reports within the section */}
        {activeSection.reports.length > 0 && (
          <div className="mb-6 flex flex-wrap gap-1 border-b border-border">
            {activeSection.reports.map(r => (
              <button
                key={r.key}
                onClick={() => setReport(r.key)}
                className={`px-4 py-2 text-sm ${
                  report === r.key
                    ? 'border-b-2 border-primary text-primary font-semibold'
                    : 'border-b-2 border-transparent text-muted-foreground hover:text-on-surface'
                }`}
              >
                {r.label}
              </button>
            ))}
          </div>
        )}

        {error ? (
          <div className="card p-6 text-sm text-status-error">{error}</div>
        ) : loading ? (
          <div className="space-y-6">
            <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6">
              {Array.from({ length: 6 }).map((_, i) => (
                <div key={i} className="h-20 animate-pulse rounded-lg bg-muted" />
              ))}
            </div>
            <div className="card space-y-2 p-4">
              {Array.from({ length: 6 }).map((_, i) => (
                <div key={i} className="h-9 animate-pulse rounded bg-muted" />
              ))}
            </div>
          </div>
        ) : (
          <div className="space-y-8">
            {/* ───────────────── SALES: OVERVIEW ───────────────── */}
            {report === 'salesOverview' && (
              <section>
                <div className="mb-4 flex items-end justify-between">
                  <h2 className="font-heading text-xl font-bold">Sales overview</h2>
                  <ExportButton onCsv={exportOverviewCsv} onPdf={exportOverviewPdf} disabled={!sales} />
                </div>

                {/* KPI tiles */}
                <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6">
                  <Kpi label="Orders" value={String(sales?.orderCount ?? 0)} />
                  <Kpi label="Gross sales" value={money(sales?.grossSales ?? 0)} />
                  <Kpi label="Net sales" value={money(sales?.netSales ?? 0)} />
                  <Kpi label="Discount" value={money(sales?.discount ?? 0)} />
                  <Kpi label="Service charge" value={money(sales?.serviceCharge ?? 0)} />
                  <Kpi label="Tax" value={money(sales?.tax ?? 0)} />
                </div>

                {/* By source + By day */}
                <div className="mt-4 grid gap-4 lg:grid-cols-2">
                  {/* By source */}
                  <div className="card overflow-hidden">
                    <div className="border-b border-border px-4 py-3">
                      <h3 className="text-sm font-semibold">By source</h3>
                    </div>
                    <table className="w-full text-sm">
                      <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                        <tr>
                          <th className="px-4 py-3 font-medium">Source</th>
                          <th className="px-4 py-3 text-right font-medium">Orders</th>
                          <th className="px-4 py-3 text-right font-medium">Total</th>
                        </tr>
                      </thead>
                      <tbody>
                        {(sales?.bySource ?? []).map((s, i) => (
                          <tr key={s.source} className={i % 2 ? 'bg-muted/20' : ''}>
                            <td className="px-4 py-3 font-medium">{sourceLabel(s.source)}</td>
                            <td className="px-4 py-3 text-right tabular-nums">{s.orderCount}</td>
                            <td className="px-4 py-3 text-right font-semibold tabular-nums">
                              {money(s.total)}
                            </td>
                          </tr>
                        ))}
                        {(!sales || sales.bySource.length === 0) && (
                          <tr>
                            <td colSpan={3} className="px-4 py-10 text-center text-muted-foreground">
                              No sales in this period.
                            </td>
                          </tr>
                        )}
                      </tbody>
                    </table>
                  </div>

                  {/* By day */}
                  <div className="card overflow-hidden">
                    <div className="border-b border-border px-4 py-3">
                      <h3 className="text-sm font-semibold">By day</h3>
                    </div>
                    <table className="w-full text-sm">
                      <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                        <tr>
                          <th className="px-4 py-3 font-medium">Day</th>
                          <th className="px-4 py-3 text-right font-medium">Orders</th>
                          <th className="px-4 py-3 text-right font-medium">Total</th>
                        </tr>
                      </thead>
                      <tbody>
                        {(sales?.byDay ?? []).map((d, i) => (
                          <tr key={d.day} className={i % 2 ? 'bg-muted/20' : ''}>
                            <td className="px-4 py-3">
                              <div className="font-medium text-muted-foreground">{dayFmt(d.day)}</div>
                              <div className="mt-1 h-1.5 w-full overflow-hidden rounded-full bg-muted">
                                <div
                                  className="h-full rounded-full bg-primary"
                                  style={{
                                    width: `${maxDayTotal > 0 ? (d.total / maxDayTotal) * 100 : 0}%`,
                                  }}
                                />
                              </div>
                            </td>
                            <td className="px-4 py-3 text-right tabular-nums">{d.orderCount}</td>
                            <td className="px-4 py-3 text-right font-semibold tabular-nums">
                              {money(d.total)}
                            </td>
                          </tr>
                        ))}
                        {(!sales || sales.byDay.length === 0) && (
                          <tr>
                            <td colSpan={3} className="px-4 py-10 text-center text-muted-foreground">
                              No sales in this period.
                            </td>
                          </tr>
                        )}
                      </tbody>
                    </table>
                  </div>
                </div>

                {/* Top items */}
                <div className="card mt-4 overflow-hidden">
                  <div className="border-b border-border px-4 py-3">
                    <h3 className="text-sm font-semibold">Top items</h3>
                  </div>
                  <table className="w-full text-sm">
                    <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                      <tr>
                        <th className="px-4 py-3 font-medium">#</th>
                        <th className="px-4 py-3 font-medium">Product Code</th>
                        <th className="px-4 py-3 font-medium">Item</th>
                        <th className="px-4 py-3 text-right font-medium">Qty</th>
                        <th className="px-4 py-3 text-right font-medium">Revenue</th>
                      </tr>
                    </thead>
                    <tbody>
                      {(sales?.topItems ?? []).map((it, i) => (
                        <tr key={it.productId} className={i % 2 ? 'bg-muted/20' : ''}>
                          <td className="px-4 py-3 text-muted-foreground tabular-nums">{i + 1}</td>
                          <td className="px-4 py-3 font-mono text-xs text-muted-foreground">{it.sku ?? '—'}</td>
                          <td className="px-4 py-3 font-medium">{it.productName}</td>
                          <td className="px-4 py-3 text-right tabular-nums">{it.quantity}</td>
                          <td className="px-4 py-3 text-right font-semibold tabular-nums">
                            {money(it.revenue)}
                          </td>
                        </tr>
                      ))}
                      {(!sales || sales.topItems.length === 0) && (
                        <tr>
                          <td colSpan={5} className="px-4 py-10 text-center text-muted-foreground">
                            No item sales in this period.
                          </td>
                        </tr>
                      )}
                    </tbody>
                  </table>
                </div>
              </section>
            )}

            {/* ───────────────── SALES: DAILY SUMMARY ───────────────── */}
            {report === 'salesDaily' && (
              <section>
                <div className="mb-4 flex items-end justify-between">
                  <h2 className="font-heading text-xl font-bold">Daily sales summary</h2>
                  <ExportButton onCsv={exportDailySummaryCsv} onPdf={exportDailySummaryPdf} disabled={!dailySummary || dailySummary.rows.length === 0} />
                </div>
                <div className="card overflow-hidden overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                      <tr>
                        <th className="px-4 py-3 font-medium">Date</th>
                        <th className="px-4 py-3 font-medium">Location</th>
                        <th className="px-4 py-3 text-right font-medium">Receipts</th>
                        <th className="px-4 py-3 text-right font-medium">Gross</th>
                        <th className="px-4 py-3 text-right font-medium">Discount</th>
                        <th className="px-4 py-3 text-right font-medium">Service charge</th>
                        <th className="px-4 py-3 text-right font-medium">Tax</th>
                        <th className="px-4 py-3 text-right font-medium">Net</th>
                      </tr>
                    </thead>
                    <tbody>
                      {(dailySummary?.rows ?? []).map((r, i) => (
                        <tr key={`${r.date}-${r.locationId}`} className={i % 2 ? 'bg-muted/20' : ''}>
                          <td className="whitespace-nowrap px-4 py-2.5 text-muted-foreground">{new Date(r.date).toLocaleDateString('en-LK', { year: 'numeric', month: 'short', day: 'numeric' })}</td>
                          <td className="px-4 py-2.5"><span className="font-medium">{r.locationName}</span> <span className="font-mono text-xs text-muted-foreground">{r.locationCode}</span></td>
                          <td className="px-4 py-2.5 text-right tabular-nums">{r.receipts}</td>
                          <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{money(r.gross)}</td>
                          <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{money(r.discount)}</td>
                          <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{money(r.serviceCharge)}</td>
                          <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{money(r.tax)}</td>
                          <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{money(r.net)}</td>
                        </tr>
                      ))}
                      {(!dailySummary || dailySummary.rows.length === 0) && (
                        <tr>
                          <td colSpan={8} className="px-4 py-10 text-center text-muted-foreground">No sales in this period.</td>
                        </tr>
                      )}
                    </tbody>
                    {dailySummary && dailySummary.rows.length > 0 && (
                      <tfoot className="border-t border-border bg-muted/30 font-bold">
                        <tr>
                          <td className="px-4 py-3" colSpan={2}>Total</td>
                          <td className="px-4 py-3 text-right tabular-nums">{dailySummary.totals.receipts}</td>
                          <td className="px-4 py-3 text-right tabular-nums">{money(dailySummary.totals.gross)}</td>
                          <td className="px-4 py-3 text-right tabular-nums">{money(dailySummary.totals.discount)}</td>
                          <td className="px-4 py-3 text-right tabular-nums">{money(dailySummary.totals.serviceCharge)}</td>
                          <td className="px-4 py-3 text-right tabular-nums">{money(dailySummary.totals.tax)}</td>
                          <td className="px-4 py-3 text-right tabular-nums">{money(dailySummary.totals.net)}</td>
                        </tr>
                      </tfoot>
                    )}
                  </table>
                </div>
              </section>
            )}

            {/* ───────────────── SALES: DAILY SUMMARY DETAIL ───────────────── */}
            {report === 'salesDailyDetail' && <DailySummaryDetailReport from={from} to={to} locationId={locationId} />}

            {/* ───────────────── SALES: REGISTER ───────────────── */}
            {report === 'salesRegister' && <SalesRegisterReport from={from} to={to} locationId={locationId} />}

            {/* ───────────────── SALES: ITEM SALES ───────────────── */}
            {report === 'salesItems' && (
              <section>
                <div className="mb-4 flex items-end justify-between">
                  <h2 className="font-heading text-xl font-bold">Item sales</h2>
                  <div className="flex items-center gap-3">
                    <HeaderStat><Num>{items?.totalQty ?? 0}</Num> units · <Num>{money(items?.totalRevenue ?? 0)}</Num></HeaderStat>
                    <ExportButton onCsv={exportItemSalesCsv} onPdf={exportItemSalesPdf} disabled={!items || items.items.length === 0} />
                  </div>
                </div>
                <div className="card overflow-hidden">
                  <table className="w-full text-sm">
                    <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                      <tr><th className="px-4 py-3 font-medium">Product Code</th><th className="px-4 py-3 font-medium">Item</th><th className="px-4 py-3 text-right font-medium">Qty</th><th className="px-4 py-3 text-right font-medium">Revenue</th></tr>
                    </thead>
                    <tbody>
                      {(items?.items ?? []).map((it, i) => (
                        <tr key={it.productId} className={i % 2 ? 'bg-muted/20' : ''}>
                          <td className="px-4 py-2.5 font-mono text-xs text-muted-foreground">{it.sku ?? '—'}</td>
                          <td className="px-4 py-2.5 font-medium">{it.productName}</td>
                          <td className="px-4 py-2.5 text-right tabular-nums">{it.quantity}</td>
                          <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{money(it.revenue)}</td>
                        </tr>
                      ))}
                      {(!items || items.items.length === 0) && (
                        <tr><td colSpan={4} className="px-4 py-10 text-center text-muted-foreground">No items sold in this period.</td></tr>
                      )}
                    </tbody>
                  </table>
                </div>
              </section>
            )}

            {/* ───────────────── SALES: BY CATEGORY ───────────────── */}
            {report === 'salesByCategory' && <CategorySalesReportView from={from} to={to} locationId={locationId} />}

            {/* ───────────────── SALES: BY OUTLET (HQ) ───────────────── */}
            {report === 'salesOutlets' && (
              <section>
                <div className="mb-4 flex items-end justify-between">
                  <h2 className="font-heading text-xl font-bold">By outlet</h2>
                  <ExportButton onCsv={exportOutletsCsv} onPdf={exportOutletsPdf} disabled={!outlets || outlets.outlets.length === 0} />
                </div>
                <div className="card overflow-hidden">
                  <table className="w-full text-sm">
                    <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                      <tr>
                        <th className="px-4 py-3 font-medium">Outlet</th>
                        <th className="px-4 py-3 text-right font-medium">Orders</th>
                        <th className="px-4 py-3 text-right font-medium">Gross sales</th>
                        <th className="px-4 py-3 text-right font-medium">Net sales</th>
                        <th className="px-4 py-3 text-right font-medium">Tax</th>
                        <th className="px-4 py-3 text-right font-medium">Stock value</th>
                      </tr>
                    </thead>
                    <tbody>
                      {(outlets?.outlets ?? []).map((o, i) => (
                        <tr key={o.locationId} className={i % 2 ? 'bg-muted/20' : ''}>
                          <td className="px-4 py-3"><span className="font-medium">{o.name}</span> <span className="font-mono text-xs text-muted-foreground">{o.code}</span></td>
                          <td className="px-4 py-3 text-right tabular-nums">{o.orderCount}</td>
                          <td className="px-4 py-3 text-right font-semibold tabular-nums">{money(o.grossSales)}</td>
                          <td className="px-4 py-3 text-right tabular-nums text-muted-foreground">{money(o.netSales)}</td>
                          <td className="px-4 py-3 text-right tabular-nums text-muted-foreground">{money(o.tax)}</td>
                          <td className="px-4 py-3 text-right tabular-nums">{money(o.stockValue)}</td>
                        </tr>
                      ))}
                      {(!outlets || outlets.outlets.length === 0) && (
                        <tr><td colSpan={6} className="px-4 py-10 text-center text-muted-foreground">No outlets configured.</td></tr>
                      )}
                    </tbody>
                    {outlets && outlets.outlets.length > 0 && (
                      <tfoot className="border-t border-border bg-muted/30 font-bold">
                        <tr>
                          <td className="px-4 py-3">Group total</td>
                          <td className="px-4 py-3 text-right tabular-nums">{outlets.totalOrders}</td>
                          <td className="px-4 py-3 text-right tabular-nums">{money(outlets.totalGross)}</td>
                          <td className="px-4 py-3 text-right tabular-nums">{money(outlets.totalNet)}</td>
                          <td className="px-4 py-3 text-right tabular-nums">{money(outlets.totalTax)}</td>
                          <td className="px-4 py-3 text-right tabular-nums">{money(outlets.totalStockValue)}</td>
                        </tr>
                      </tfoot>
                    )}
                  </table>
                </div>
              </section>
            )}

            {/* ───────────────── SALES: PROMOTION USAGE ───────────────── */}
            {report === 'salesPromotions' && <PromotionsUsageReport from={from} to={to} locationId={locationId} />}

            {/* ───────────────── SALES: DISCOUNT & COMPLIMENTARY (lite) ───────────────── */}
            {report === 'salesDiscounts' && <DiscountsReportView from={from} to={to} locationId={locationId} />}

            {/* ───────────────── SALES: TABLE TURNOVER ───────────────── */}
            {report === 'salesTableTurnover' && <TableTurnoverReportView from={from} to={to} locationId={locationId} />}

            {/* ───────────────── SALES: STEWARD SALES ───────────────── */}
            {report === 'salesStewards' && <StewardSalesReportView from={from} to={to} locationId={locationId} />}

            {/* ───────────────── SALES: TOUR COMMISSION ───────────────── */}
            {report === 'salesTourCommission' && <TourCommissionReportView from={from} to={to} locationId={locationId} />}

            {/* ───────────────── SALES: VOID / CANCELLATION ───────────────── */}
            {report === 'salesVoids' && <VoidOrdersReport from={from} to={to} locationId={locationId} />}

            {/* ───────────────── TAX RETURN ───────────────── */}
            {report === 'taxReturn' && (
              <section>
                <div className="mb-4 flex items-end justify-between">
                  <h2 className="font-heading text-xl font-bold">{taxLabel} return</h2>
                  <ExportButton onCsv={exportTaxReturnCsv} onPdf={exportTaxReturnPdf} disabled={!vat} />
                </div>

                <div className="card p-6">
                  {/* Headline */}
                  <div className="grid gap-4 sm:grid-cols-3">
                    <div>
                      <div className="text-xs uppercase tracking-wide text-muted-foreground">
                        Output {taxLabel}
                      </div>
                      <div className="mt-1 text-2xl font-bold tabular-nums">
                        {money(vat?.outputVat ?? 0)}
                      </div>
                    </div>
                    <div>
                      <div className="text-xs uppercase tracking-wide text-muted-foreground">
                        − Input {taxLabel}
                      </div>
                      <div className="mt-1 text-2xl font-bold tabular-nums">
                        {money(vat?.inputVat ?? 0)}
                      </div>
                    </div>
                    <div className="rounded-lg bg-muted/40 p-3">
                      <div className="text-xs uppercase tracking-wide text-muted-foreground">
                        {vatReclaimable ? `= Net ${taxLabel} reclaimable` : `= Net ${taxLabel} payable`}
                      </div>
                      <div
                        className={`mt-1 text-3xl font-extrabold tabular-nums ${
                          vatReclaimable ? 'text-status-success' : 'text-primary'
                        }`}
                      >
                        {money(Math.abs(vat?.netVatPayable ?? 0))}
                      </div>
                      {vatReclaimable && (
                        <span className="pill pill-paid mt-1 inline-block">Reclaimable</span>
                      )}
                    </div>
                  </div>

                  {/* Secondary stats */}
                  <div className="mt-6 grid grid-cols-2 gap-3 sm:grid-cols-4">
                    <SecondaryStat label="Tax invoices" value={String(vat?.invoiceCount ?? 0)} />
                    <SecondaryStat label="Net sales" value={money(vat?.netSales ?? 0)} />
                    <SecondaryStat label="GRNs" value={String(vat?.grnCount ?? 0)} />
                    <SecondaryStat
                      label={`Purchases ex-${taxLabel}`}
                      value={money(vat?.purchasesExVat ?? 0)}
                    />
                  </div>
                </div>

                {/* Charge breakdown */}
                <div className="card mt-4 overflow-hidden">
                  <div className="border-b border-border px-4 py-3">
                    <h3 className="text-sm font-semibold">Charge breakdown</h3>
                  </div>
                  <table className="w-full text-sm">
                    <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                      <tr>
                        <th className="px-4 py-3 font-medium">Code</th>
                        <th className="px-4 py-3 font-medium">Name</th>
                        <th className="px-4 py-3 font-medium">Type</th>
                        <th className="px-4 py-3 text-right font-medium">Rate %</th>
                        <th className="px-4 py-3 text-right font-medium">Base</th>
                        <th className="px-4 py-3 text-right font-medium">Charge</th>
                      </tr>
                    </thead>
                    <tbody>
                      {(vat?.charges ?? []).map((c, i) => (
                        <tr key={c.code} className={i % 2 ? 'bg-muted/20' : ''}>
                          <td className="px-4 py-3 font-mono text-xs">{c.code}</td>
                          <td className="px-4 py-3 font-medium">{c.name}</td>
                          <td className="px-4 py-3 text-muted-foreground">{c.chargeType}</td>
                          <td className="px-4 py-3 text-right tabular-nums">
                            {c.ratePercent.toFixed(2)}
                          </td>
                          <td className="px-4 py-3 text-right tabular-nums">{money(c.baseAmount)}</td>
                          <td className="px-4 py-3 text-right font-semibold tabular-nums">
                            {money(c.chargeAmount)}
                          </td>
                        </tr>
                      ))}
                      {(!vat || vat.charges.length === 0) && (
                        <tr>
                          <td colSpan={6} className="px-4 py-10 text-center text-muted-foreground">
                            No charges in this period.
                          </td>
                        </tr>
                      )}
                    </tbody>
                  </table>
                </div>
              </section>
            )}

            {/* ───────────────── INVENTORY: STOCK BALANCE ───────────────── */}
            {report === 'invStock' && <StockBalanceReport locationId={locationId} />}

            {/* ───────────────── INVENTORY: WASTAGE ───────────────── */}
            {report === 'invWastage' && <WastageReport from={from} to={to} locationId={locationId} />}

            {/* ───────────────── INVENTORY: BIN CARD ───────────────── */}
            {report === 'invBinCard' && <BinCardReport locations={locations} />}

            {/* ───────────────── INVENTORY: PURCHASES BY SUPPLIER ───────────────── */}
            {report === 'invPurchasesBySupplier' && <PurchasesBySupplierReport from={from} to={to} locationId={locationId} />}

            {/* ───────────────── INVENTORY: LOW STOCK / REORDER ───────────────── */}
            {report === 'invLowStock' && <LowStockReport locationId={locationId} />}

            {/* ───────────────── INVENTORY: SLOW-MOVING STOCK ───────────────── */}
            {report === 'invSlowMoving' && <SlowMovingReport locationId={locationId} />}

            {/* ───────────────── OPERATIONS: SHIFT SETTLEMENT ───────────────── */}
            {report === 'opsShifts' && <ShiftsReport from={from} to={to} locationId={locationId} />}

            {/* ───────────────── COSTING: FOOD COSTING ───────────────── */}
            {report === 'costFood' && <FoodCostingReport locations={locations} />}

            {/* ───────────────── COSTING: BUDGET VS SALES ───────────────── */}
            {report === 'costBudget' && <BudgetVsSalesReport locations={locations} />}

            {/* ───────────────── MASTER DATA: PRODUCTS ───────────────── */}
            {report === 'mdProducts' && <ProductsMasterReport />}

            {/* ───────────────── MASTER DATA: SUPPLIERS ───────────────── */}
            {report === 'mdSuppliers' && <SuppliersMasterReport />}

            {/* ───────────────── MASTER DATA: CUSTOMERS ───────────────── */}
            {report === 'mdCustomers' && <CustomersMasterReport />}

            {/* ───────────────── MASTER DATA: LOCATIONS ───────────────── */}
            {report === 'mdLocations' && <LocationsMasterReport />}
          </div>
        )}

        {sales && !loading && !error && (
          <p className="mt-3 text-xs text-muted-foreground">
            Period {sales.periodFrom.slice(0, 10)} → {sales.periodTo.slice(0, 10)} · amounts in LKR
          </p>
        )}
      </div>

      {toast && (
        <div className="fixed bottom-12 left-1/2 z-[70] -translate-x-1/2 rounded-lg bg-on-surface px-4 py-2.5 text-sm text-white shadow-lg">
          {toast}
        </div>
      )}
    </>
  );
}

type CostRow = { productId: string; name: string; sku: string; dishCost: number; sellPrice: number; grossProfit: number; gpPercent: number; foodCostPercent: number; costSource: string };
type BinCard = { opening: number; closing: number; totalIn: number; totalOut: number; lines: { date: string; type: string; doc: string; qty: number; balance: number }[] };
type BvsRow = { month: string; budget: number; actual: number; variance: number; pct: number | null };

function FoodCostingReport({ locations }: { locations: Location[] }) {
  const [loc, setLoc] = useState('');
  const [costing, setCosting] = useState<CostRow[] | null>(null);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [busy, setBusy] = useState(false);
  const [exporting, setExporting] = useState(false);

  async function loadCosting(page: number) {
    setBusy(true);
    try {
      const params = new URLSearchParams();
      params.set('pageNumber', String(page)); params.set('pageSize', String(pageSize));
      if (loc) params.set('locationId', loc);
      const res = await apiClient<{ items: CostRow[]; pagination: { totalCount: number; totalPages: number } }>(`/api/v1/reports/food-costing?${params.toString()}`);
      setCosting(res.items);
      setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages || 1);
    } catch { /* */ } finally { setBusy(false); }
  }

  useEffect(() => { if (costing !== null) void loadCosting(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);

  function runReport() { setPageNumber(1); void loadCosting(1); }

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: CostRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        if (loc) params.set('locationId', loc);
        const res = await apiClient<{ items: CostRow[] }>(`/api/v1/reports/food-costing?${params.toString()}`);
        all.push(...res.items);
      }
      downloadCsv(
        `food_costing_${fmtDate(new Date())}.csv`,
        ['Item', 'SKU', 'Dish Cost', 'Sell', 'GP', 'GP %', 'Food Cost %', 'Source'],
        all.map(r => [r.name, r.sku, r.dishCost.toFixed(2), r.sellPrice.toFixed(2), r.grossProfit.toFixed(2), r.gpPercent, r.foodCostPercent, r.costSource === 'production' ? 'actual' : 'estimate']),
      );
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: CostRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        if (loc) params.set('locationId', loc);
        const res = await apiClient<{ items: CostRow[] }>(`/api/v1/reports/food-costing?${params.toString()}`);
        all.push(...res.items);
      }
      downloadPdf(`food_costing_${fmtDate(new Date())}.pdf`, 'Food Costing', `As at ${fmtDate(new Date())} - amounts in LKR`, [{
        head: ['Item', 'SKU', 'Dish Cost', 'Sell', 'GP', 'GP %', 'Food Cost %', 'Source'],
        rows: all.map(r => [r.name, r.sku, r.dishCost.toFixed(2), r.sellPrice.toFixed(2), r.grossProfit.toFixed(2), r.gpPercent, r.foodCostPercent, r.costSource === 'production' ? 'actual' : 'estimate']),
      }]);
    } finally { setExporting(false); }
  }

  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);

  return (
    <section>
      <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
        <div><h2 className="font-heading text-xl font-bold">Food costing</h2><p className="text-sm text-muted-foreground">Dish cost vs sell price · gross-profit %</p></div>
        <div className="flex items-end gap-2">
          <Combobox
            className="w-56"
            label="Outlet"
            value={loc}
            onChange={setLoc}
            placeholder="All outlets"
            options={[{ value: '', label: 'All outlets' }, ...locations.map(l => ({ value: l.id, label: `${l.code} — ${l.name}` }))]}
          />
          <button onClick={runReport} disabled={busy} className="rounded-lg bg-primary px-4 py-2 text-sm font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{busy ? 'Loading…' : 'Run'}</button>
          <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
        </div>
      </div>
      <div className="card p-5">
        {costing ? (
          <table className="w-full text-sm">
            <thead className="border-b border-border text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr><th className="py-2">Item</th><th className="py-2 text-right">Dish cost</th><th className="py-2 text-right">Sell</th><th className="py-2 text-right">GP</th><th className="py-2 text-right">GP %</th><th className="py-2 text-right">Food cost %</th><th className="py-2 text-right">Source</th></tr>
            </thead>
            <tbody>
              {costing.map(r => (
                <tr key={r.productId} className="border-b border-border/40">
                  <td className="py-2">{r.name} <span className="font-mono text-xs text-muted-foreground">{r.sku}</span></td>
                  <td className="py-2 text-right tabular-nums">{money(r.dishCost)}</td>
                  <td className="py-2 text-right tabular-nums">{money(r.sellPrice)}</td>
                  <td className="py-2 text-right tabular-nums">{money(r.grossProfit)}</td>
                  <td className={`py-2 text-right tabular-nums font-semibold ${r.gpPercent < 60 ? 'text-status-error' : 'text-primary'}`}>{r.gpPercent}%</td>
                  <td className="py-2 text-right tabular-nums">{r.foodCostPercent}%</td>
                  <td className="py-2 text-right text-xs text-muted-foreground">{r.costSource === 'production' ? 'actual' : 'estimate'}</td>
                </tr>
              ))}
              {costing.length === 0 && <tr><td colSpan={7} className="py-6 text-center text-muted-foreground">No recipes configured.</td></tr>}
            </tbody>
          </table>
        ) : (
          <p className="py-6 text-center text-sm text-muted-foreground">Click Run to load food costing.</p>
        )}
      </div>
      {costing !== null && totalCount > 0 && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm"
          >
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
            <option value={100}>100 / page</option>
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun="recipes" className="mt-0 flex-1" />
        </div>
      )}
    </section>
  );
}

type WastageRow = {
  wastageNumber: string; approvedAt: string; reason: string;
  locationCode: string; locationName: string;
  sku: string; productName: string; quantity: number; unitCost: number; lineTotal: number;
};

const WASTAGE_REASON_LABEL: Record<string, string> = {
  spoilage: 'Spoilage', breakage: 'Breakage', expiry: 'Expiry', theft: 'Theft', other: 'Other',
};

function WastageReport({ from, to, locationId }: { from: string; to: string; locationId: string }) {
  const [rows, setRows] = useState<WastageRow[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCost, setTotalCost] = useState(0);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [reason, setReason] = useState('');
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  async function load(page: number) {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set('pageNumber', String(page));
      params.set('pageSize', String(pageSize));
      params.set('from', from); params.set('to', to);
      if (locationId) params.set('locationId', locationId);
      if (reason) params.set('reason', reason);
      const res = await apiClient<{ data: WastageRow[]; pagination: { totalCount: number; totalPages: number }; totals: { cost: number } }>(`/api/v1/reports/wastage?${params.toString()}`);
      setRows(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages || 1);
      setTotalCost(res.totals.cost);
    } catch { /* */ }
    finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { setPageNumber(1); void load(1); /* eslint-disable-next-line */ }, [from, to, locationId, reason]);

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: WastageRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        if (reason) params.set('reason', reason);
        const res = await apiClient<{ data: WastageRow[] }>(`/api/v1/reports/wastage?${params.toString()}`);
        all.push(...res.data);
      }
      downloadCsv(
        `wastage_${from}_to_${to}.csv`,
        ['Date', 'Wastage No', 'Location', 'Product Code', 'Product', 'Qty', 'Unit Cost', 'Line Total', 'Reason'],
        all.map(r => [
          new Date(r.approvedAt).toLocaleDateString('en-LK'), r.wastageNumber, `${r.locationCode} — ${r.locationName}`,
          r.sku, r.productName, r.quantity, r.unitCost.toFixed(2), r.lineTotal.toFixed(2), WASTAGE_REASON_LABEL[r.reason] ?? r.reason,
        ]),
      );
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: WastageRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        if (reason) params.set('reason', reason);
        const res = await apiClient<{ data: WastageRow[] }>(`/api/v1/reports/wastage?${params.toString()}`);
        all.push(...res.data);
      }
      downloadPdf(`wastage_${from}_to_${to}.pdf`, 'Wastage', `Period ${from} to ${to} - total cost ${money(totalCost)}`, [{
        head: ['Date', 'Wastage No', 'Location', 'Product Code', 'Product', 'Qty', 'Unit Cost', 'Line Total', 'Reason'],
        rows: all.map(r => [
          new Date(r.approvedAt).toLocaleDateString('en-LK'), r.wastageNumber, `${r.locationCode} - ${r.locationName}`,
          r.sku, r.productName, r.quantity, r.unitCost.toFixed(2), r.lineTotal.toFixed(2), WASTAGE_REASON_LABEL[r.reason] ?? r.reason,
        ]),
      }]);
    } finally { setExporting(false); }
  }

  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);

  return (
    <section>
      <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
        <div>
          <h2 className="font-heading text-xl font-bold">Wastage</h2>
          <p className="text-sm text-muted-foreground">Posted wastage lines · total cost <span className="font-semibold text-foreground">{money(totalCost)}</span></p>
        </div>
        <div className="flex items-end gap-2">
          <select value={reason} onChange={e => setReason(e.target.value)} className="h-9 rounded-lg border border-border bg-card px-3 text-sm">
            <option value="">All reasons</option>
            {Object.entries(WASTAGE_REASON_LABEL).map(([v, l]) => <option key={v} value={v}>{l}</option>)}
          </select>
          <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
        </div>
      </div>
      <div className="card overflow-hidden">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-3 font-medium">Date</th>
                <th className="px-4 py-3 font-medium">Wastage No</th>
                <th className="px-4 py-3 font-medium">Location</th>
                <th className="px-4 py-3 font-medium">Product</th>
                <th className="px-4 py-3 text-right font-medium">Qty</th>
                <th className="px-4 py-3 text-right font-medium">Unit Cost</th>
                <th className="px-4 py-3 text-right font-medium">Line Total</th>
                <th className="px-4 py-3 font-medium">Reason</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r, i) => (
                <tr key={`${r.wastageNumber}-${r.sku}-${i}`} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="whitespace-nowrap px-4 py-2.5 text-muted-foreground">{new Date(r.approvedAt).toLocaleDateString('en-LK', { year: 'numeric', month: 'short', day: 'numeric' })}</td>
                  <td className="px-4 py-2.5 font-mono text-xs">{r.wastageNumber}</td>
                  <td className="px-4 py-2.5"><span className="font-medium">{r.locationName}</span> <span className="font-mono text-xs text-muted-foreground">{r.locationCode}</span></td>
                  <td className="px-4 py-2.5"><span className="font-medium">{r.productName}</span> <span className="font-mono text-xs text-muted-foreground">{r.sku}</span></td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{r.quantity}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{money(r.unitCost)}</td>
                  <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{money(r.lineTotal)}</td>
                  <td className="px-4 py-2.5"><span className="pill pill-idle">{WASTAGE_REASON_LABEL[r.reason] ?? r.reason}</span></td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr><td colSpan={8} className="px-4 py-10 text-center text-muted-foreground">No posted wastage in this period.</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {!loading && totalCount > 0 && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm"
          >
            <option value={5}>5 / page</option>
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun="lines" className="mt-0 flex-1" />
        </div>
      )}
    </section>
  );
}

function SalesRegisterReport({ from, to, locationId }: { from: string; to: string; locationId: string }) {
  const [orders, setOrders] = useState<RegisterRow[]>([]);
  const [tenders, setTenders] = useState<TenderRow[]>([]);
  const [totals, setTotals] = useState({ subtotal: 0, discount: 0, serviceCharge: 0, tax: 0, total: 0 });
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  async function load(page: number) {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set('pageNumber', String(page)); params.set('pageSize', String(pageSize));
      params.set('from', from); params.set('to', to);
      if (locationId) params.set('locationId', locationId);
      const res = await apiClient<SalesRegister>(`/api/v1/reports/sales/register?${params.toString()}`);
      setOrders(res.orders); setTenders(res.tenders); setTotals(res.totals);
      setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages || 1);
    } catch { /* */ }
    finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { setPageNumber(1); void load(1); /* eslint-disable-next-line */ }, [from, to, locationId]);

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: RegisterRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<SalesRegister>(`/api/v1/reports/sales/register?${params.toString()}`);
        all.push(...res.orders);
      }
      downloadCsv(
        `sales_register_${from}_to_${to}.csv`,
        ['Time', 'Bill', 'Type', 'Table / Customer', 'Subtotal', 'Discount', 'Service Charge', 'Tax', 'Total'],
        all.map(o => [
          new Date(o.settledAt).toLocaleString('en-LK'), o.number, o.orderType.replace('_', '-'),
          `${o.tableLabel ? `Table ${o.tableLabel}` : ''}${o.customerName ? ` ${o.customerName}` : ''}`.trim(),
          o.subtotalAmount.toFixed(2), (o.discountAmount + o.promotionDiscountAmount).toFixed(2),
          o.serviceChargeAmount.toFixed(2), o.taxAmount.toFixed(2), o.totalAmount.toFixed(2),
        ]),
      );
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: RegisterRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<SalesRegister>(`/api/v1/reports/sales/register?${params.toString()}`);
        all.push(...res.orders);
      }
      downloadPdf(`sales_register_${from}_to_${to}.pdf`, 'Sales Register', `Period ${from} to ${to} - amounts in LKR`, [{
        head: ['Time', 'Bill', 'Type', 'Table / Customer', 'Subtotal', 'Discount', 'Svc Chg', 'Tax', 'Total'],
        rows: all.map(o => [
          new Date(o.settledAt).toLocaleString('en-LK'), o.number, o.orderType.replace('_', '-'),
          `${o.tableLabel ? `Table ${o.tableLabel}` : ''}${o.customerName ? ` ${o.customerName}` : ''}`.trim(),
          o.subtotalAmount.toFixed(2), (o.discountAmount + o.promotionDiscountAmount).toFixed(2),
          o.serviceChargeAmount.toFixed(2), o.taxAmount.toFixed(2), o.totalAmount.toFixed(2),
        ]),
      }]);
    } finally { setExporting(false); }
  }

  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);

  return (
    <section>
      <div className="mb-4 flex items-end justify-between">
        <h2 className="font-heading text-xl font-bold">Sales register</h2>
        <div className="flex items-center gap-3">
          <HeaderStat><Num>{totalCount}</Num> bills · <Num>{money(totals.total)}</Num></HeaderStat>
          <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
        </div>
      </div>
      {tenders.length > 0 && (
        <div className="mb-3 flex flex-wrap gap-2">
          {tenders.map(t => (
            <span key={t.payType} className="rounded-lg border border-border bg-surface px-3 py-1.5 text-sm">
              <span className="text-muted-foreground">{PAY_LABEL[t.payType] ?? t.payType}:</span> <span className="font-semibold tabular-nums">{money(t.amount)}</span>
            </span>
          ))}
        </div>
      )}
      <div className="card overflow-hidden overflow-x-auto">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-3 font-medium">Time</th><th className="px-4 py-3 font-medium">Bill</th>
                <th className="px-4 py-3 font-medium">Type</th><th className="px-4 py-3 font-medium">Table / customer</th>
                <th className="px-4 py-3 text-right font-medium">Subtotal</th><th className="px-4 py-3 text-right font-medium">Disc</th>
                <th className="px-4 py-3 text-right font-medium">Svc</th><th className="px-4 py-3 text-right font-medium">Tax</th>
                <th className="px-4 py-3 text-right font-medium">Total</th>
              </tr>
            </thead>
            <tbody>
              {orders.map((o, i) => (
                <tr key={o.id} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="whitespace-nowrap px-4 py-2.5 text-xs text-muted-foreground">{new Date(o.settledAt).toLocaleString('en-LK', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })}</td>
                  <td className="px-4 py-2.5 font-mono text-xs">{o.number}</td>
                  <td className="px-4 py-2.5 text-muted-foreground">{o.orderType.replace('_', '-')}</td>
                  <td className="px-4 py-2.5">{o.tableLabel ? `Table ${o.tableLabel}` : ''}{o.customerName ? ` · ${o.customerName}` : ''}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{money(o.subtotalAmount)}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{money(o.discountAmount + o.promotionDiscountAmount)}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{money(o.serviceChargeAmount)}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{money(o.taxAmount)}</td>
                  <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{money(o.totalAmount)}</td>
                </tr>
              ))}
              {orders.length === 0 && (
                <tr><td colSpan={9} className="px-4 py-10 text-center text-muted-foreground">No settled bills in this period.</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>
      {!loading && totalCount > 0 && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm"
          >
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
            <option value={100}>100 / page</option>
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun="bills" className="mt-0 flex-1" />
        </div>
      )}
    </section>
  );
}

function DailySummaryDetailReport({ from, to, locationId }: { from: string; to: string; locationId: string }) {
  const [rows, setRows] = useState<DailySalesDetailRow[]>([]);
  const [totals, setTotals] = useState({ receipts: 0, gross: 0, discount: 0, serviceCharge: 0, tax: 0, net: 0 });
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  async function load(page: number) {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set('pageNumber', String(page)); params.set('pageSize', String(pageSize));
      params.set('from', from); params.set('to', to);
      if (locationId) params.set('locationId', locationId);
      const res = await apiClient<DailySalesDetail>(`/api/v1/reports/sales/daily-summary/detail?${params.toString()}`);
      setRows(res.rows); setTotals(res.totals);
      setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages || 1);
    } catch { /* */ }
    finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { setPageNumber(1); void load(1); /* eslint-disable-next-line */ }, [from, to, locationId]);

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: DailySalesDetailRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<DailySalesDetail>(`/api/v1/reports/sales/daily-summary/detail?${params.toString()}`);
        all.push(...res.rows);
      }
      downloadCsv(
        `daily_sales_summary_detail_${from}_to_${to}.csv`,
        ['Date', 'Time', 'Location', 'Bill No', 'Type', 'Table / Customer', 'Gross', 'Discount', 'Service Charge', 'Tax', 'Net'],
        all.map(r => [
          r.date.slice(0, 10), new Date(r.settledAt).toLocaleTimeString('en-LK', { hour: '2-digit', minute: '2-digit' }),
          `${r.locationCode} — ${r.locationName}`, r.number, r.orderType.replace('_', '-'),
          r.tableLabel ? `Table ${r.tableLabel}` : (r.customerName ?? ''),
          r.gross.toFixed(2), r.discount.toFixed(2), r.serviceCharge.toFixed(2), r.tax.toFixed(2), r.net.toFixed(2),
        ]),
      );
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: DailySalesDetailRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<DailySalesDetail>(`/api/v1/reports/sales/daily-summary/detail?${params.toString()}`);
        all.push(...res.rows);
      }
      downloadPdf(`daily_sales_summary_detail_${from}_to_${to}.pdf`, 'Daily Sales Summary - Detail', `Period ${from} to ${to} - amounts in LKR`, [{
        head: ['Date', 'Time', 'Location', 'Bill No', 'Type', 'Table / Customer', 'Gross', 'Discount', 'Svc Chg', 'Tax', 'Net'],
        rows: all.map(r => [
          r.date.slice(0, 10), new Date(r.settledAt).toLocaleTimeString('en-LK', { hour: '2-digit', minute: '2-digit' }),
          `${r.locationCode} - ${r.locationName}`, r.number, r.orderType.replace('_', '-'),
          r.tableLabel ? `Table ${r.tableLabel}` : (r.customerName ?? ''),
          r.gross.toFixed(2), r.discount.toFixed(2), r.serviceCharge.toFixed(2), r.tax.toFixed(2), r.net.toFixed(2),
        ]),
      }]);
    } finally { setExporting(false); }
  }

  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);

  return (
    <section>
      <div className="mb-4 flex items-end justify-between">
        <div>
          <h2 className="font-heading text-xl font-bold">Daily sales summary detail</h2>
          <p className="text-sm text-muted-foreground">Every receipt behind the Daily Summary — one row per settled bill.</p>
        </div>
        <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
      </div>
      <div className="card overflow-hidden overflow-x-auto">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-3 font-medium">Date</th>
                <th className="px-4 py-3 font-medium">Time</th>
                <th className="px-4 py-3 font-medium">Location</th>
                <th className="px-4 py-3 font-medium">Bill No</th>
                <th className="px-4 py-3 font-medium">Type</th>
                <th className="px-4 py-3 font-medium">Table / Customer</th>
                <th className="px-4 py-3 text-right font-medium">Gross</th>
                <th className="px-4 py-3 text-right font-medium">Discount</th>
                <th className="px-4 py-3 text-right font-medium">Service charge</th>
                <th className="px-4 py-3 text-right font-medium">Tax</th>
                <th className="px-4 py-3 text-right font-medium">Net</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r, i) => (
                <tr key={r.id} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="whitespace-nowrap px-4 py-2.5 text-muted-foreground">{new Date(r.date).toLocaleDateString('en-LK', { year: 'numeric', month: 'short', day: 'numeric' })}</td>
                  <td className="whitespace-nowrap px-4 py-2.5 text-xs text-muted-foreground">{new Date(r.settledAt).toLocaleTimeString('en-LK', { hour: '2-digit', minute: '2-digit' })}</td>
                  <td className="px-4 py-2.5"><span className="font-medium">{r.locationName}</span> <span className="font-mono text-xs text-muted-foreground">{r.locationCode}</span></td>
                  <td className="px-4 py-2.5 font-mono text-xs">{r.number}</td>
                  <td className="px-4 py-2.5 text-muted-foreground">{r.orderType.replace('_', '-')}</td>
                  <td className="px-4 py-2.5">{r.tableLabel ? `Table ${r.tableLabel}` : (r.customerName ?? '—')}</td>
                  <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{money(r.gross)}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{money(r.discount)}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{money(r.serviceCharge)}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{money(r.tax)}</td>
                  <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{money(r.net)}</td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr><td colSpan={11} className="px-4 py-10 text-center text-muted-foreground">No sales in this period.</td></tr>
              )}
            </tbody>
            {rows.length > 0 && (
              <tfoot className="border-t border-border bg-muted/30 font-bold">
                <tr>
                  <td className="px-4 py-3" colSpan={6}>Total · {totals.receipts} receipts</td>
                  <td className="px-4 py-3 text-right tabular-nums">{money(totals.gross)}</td>
                  <td className="px-4 py-3 text-right tabular-nums">{money(totals.discount)}</td>
                  <td className="px-4 py-3 text-right tabular-nums">{money(totals.serviceCharge)}</td>
                  <td className="px-4 py-3 text-right tabular-nums">{money(totals.tax)}</td>
                  <td className="px-4 py-3 text-right tabular-nums">{money(totals.net)}</td>
                </tr>
              </tfoot>
            )}
          </table>
        )}
      </div>
      {!loading && totalCount > 0 && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm"
          >
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
            <option value={100}>100 / page</option>
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun="receipts" className="mt-0 flex-1" />
        </div>
      )}
    </section>
  );
}

function StockBalanceReport({ locationId }: { locationId: string }) {
  const [lines, setLines] = useState<StockRow[]>([]);
  const [totalValue, setTotalValue] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  async function load(page: number) {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set('pageNumber', String(page)); params.set('pageSize', String(pageSize));
      if (locationId) params.set('locationId', locationId);
      const res = await apiClient<StockBalance>(`/api/v1/reports/stock/balance?${params.toString()}`);
      setLines(res.lines); setTotalValue(res.totalValue);
      setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages || 1);
    } catch { /* */ }
    finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { setPageNumber(1); void load(1); /* eslint-disable-next-line */ }, [locationId]);

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: StockRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<StockBalance>(`/api/v1/reports/stock/balance?${params.toString()}`);
        all.push(...res.lines);
      }
      downloadCsv(
        `stock_balance_${fmtDate(new Date())}.csv`,
        ['Product Code', 'Product', 'Outlet', 'On Hand', 'Avg Cost', 'Value'],
        all.map(s => [s.sku, s.name, s.location, s.onHand, s.avgCost.toFixed(2), s.value.toFixed(2)]),
      );
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: StockRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<StockBalance>(`/api/v1/reports/stock/balance?${params.toString()}`);
        all.push(...res.lines);
      }
      downloadPdf(`stock_balance_${fmtDate(new Date())}.pdf`, 'Stock Balance', `As at ${fmtDate(new Date())} - amounts in LKR`, [{
        head: ['Product Code', 'Product', 'Outlet', 'On Hand', 'Avg Cost', 'Value'],
        rows: all.map(s => [s.sku, s.name, s.location, s.onHand, s.avgCost.toFixed(2), s.value.toFixed(2)]),
      }]);
    } finally { setExporting(false); }
  }

  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);

  return (
    <section>
      <div className="mb-4 flex items-end justify-between">
        <h2 className="font-heading text-xl font-bold">Stock balance</h2>
        <div className="flex items-center gap-3">
          <HeaderStat>Value on hand: <Num>{money(totalValue)}</Num></HeaderStat>
          <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
        </div>
      </div>
      <div className="card overflow-hidden">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr><th className="px-4 py-3 font-medium">Product Code</th><th className="px-4 py-3 font-medium">Product</th><th className="px-4 py-3 font-medium">Outlet</th><th className="px-4 py-3 text-right font-medium">On hand</th><th className="px-4 py-3 text-right font-medium">Avg cost</th><th className="px-4 py-3 text-right font-medium">Value</th></tr>
            </thead>
            <tbody>
              {lines.map((s, i) => (
                <tr key={`${s.productId}-${s.location}`} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="px-4 py-2.5 font-mono text-xs">{s.sku}</td>
                  <td className="px-4 py-2.5 font-medium">{s.name}</td>
                  <td className="px-4 py-2.5 text-muted-foreground">{s.location}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{s.onHand}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{money(s.avgCost)}</td>
                  <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{money(s.value)}</td>
                </tr>
              ))}
              {lines.length === 0 && (
                <tr><td colSpan={6} className="px-4 py-10 text-center text-muted-foreground">No stock on hand.</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>
      {!loading && totalCount > 0 && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm"
          >
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
            <option value={100}>100 / page</option>
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun="lines" className="mt-0 flex-1" />
        </div>
      )}
    </section>
  );
}

function ShiftsReport({ from, to, locationId }: { from: string; to: string; locationId: string }) {
  const [shifts, setShifts] = useState<ShiftRow[]>([]);
  const [totalVariance, setTotalVariance] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  async function load(page: number) {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set('pageNumber', String(page)); params.set('pageSize', String(pageSize));
      params.set('from', from); params.set('to', to);
      if (locationId) params.set('locationId', locationId);
      const res = await apiClient<ShiftSettlement>(`/api/v1/reports/shifts?${params.toString()}`);
      setShifts(res.shifts); setTotalVariance(res.totalVariance);
      setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages || 1);
    } catch { /* */ }
    finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { setPageNumber(1); void load(1); /* eslint-disable-next-line */ }, [from, to, locationId]);

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: ShiftRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<ShiftSettlement>(`/api/v1/reports/shifts?${params.toString()}`);
        all.push(...res.shifts);
      }
      downloadCsv(
        `shift_settlement_${from}_to_${to}.csv`,
        ['Shift', 'By', 'Opened', 'Status', 'Sales', 'Expected', 'Declared', 'Variance'],
        all.map(s => [
          s.shiftNumber, s.openedByName ?? '', new Date(s.openedAt).toLocaleString('en-LK'), s.status,
          s.totalSales.toFixed(2), s.expectedCash == null ? '' : s.expectedCash.toFixed(2),
          s.declaredCash == null ? '' : s.declaredCash.toFixed(2), s.cashVariance == null ? '' : s.cashVariance.toFixed(2),
        ]),
      );
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: ShiftRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<ShiftSettlement>(`/api/v1/reports/shifts?${params.toString()}`);
        all.push(...res.shifts);
      }
      downloadPdf(`shift_settlement_${from}_to_${to}.pdf`, 'Shift Settlement', `Period ${from} to ${to} - amounts in LKR`, [{
        head: ['Shift', 'By', 'Opened', 'Status', 'Sales', 'Expected', 'Declared', 'Variance'],
        rows: all.map(s => [
          s.shiftNumber, s.openedByName ?? '', new Date(s.openedAt).toLocaleString('en-LK'), s.status,
          s.totalSales.toFixed(2), s.expectedCash == null ? '' : s.expectedCash.toFixed(2),
          s.declaredCash == null ? '' : s.declaredCash.toFixed(2), s.cashVariance == null ? '' : s.cashVariance.toFixed(2),
        ]),
      }]);
    } finally { setExporting(false); }
  }

  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);

  return (
    <section>
      <div className="mb-4 flex items-end justify-between">
        <h2 className="font-heading text-xl font-bold">Shift settlement</h2>
        <div className="flex items-center gap-3">
          <HeaderStat>Variance: <Num>{money(totalVariance)}</Num></HeaderStat>
          <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
        </div>
      </div>
      <div className="card overflow-hidden overflow-x-auto">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr><th className="px-4 py-3 font-medium">Shift</th><th className="px-4 py-3 font-medium">By</th><th className="px-4 py-3 font-medium">Opened</th><th className="px-4 py-3 font-medium">Status</th><th className="px-4 py-3 text-right font-medium">Sales</th><th className="px-4 py-3 text-right font-medium">Expected</th><th className="px-4 py-3 text-right font-medium">Declared</th><th className="px-4 py-3 text-right font-medium">Variance</th></tr>
            </thead>
            <tbody>
              {shifts.map((s, i) => (
                <tr key={s.shiftNumber} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="px-4 py-2.5 font-mono text-xs">{s.shiftNumber}</td>
                  <td className="px-4 py-2.5">{s.openedByName ?? '—'}</td>
                  <td className="whitespace-nowrap px-4 py-2.5 text-xs text-muted-foreground">{new Date(s.openedAt).toLocaleString('en-LK', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })}</td>
                  <td className="px-4 py-2.5"><span className={`pill ${s.status === 'closed' ? 'pill-idle' : 'pill-paid'}`}>{s.status}</span></td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{money(s.totalSales)}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{s.expectedCash == null ? '—' : money(s.expectedCash)}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{s.declaredCash == null ? '—' : money(s.declaredCash)}</td>
                  <td className={`px-4 py-2.5 text-right font-semibold tabular-nums ${(s.cashVariance ?? 0) < 0 ? 'text-status-error' : ''}`}>{s.cashVariance == null ? '—' : money(s.cashVariance)}</td>
                </tr>
              ))}
              {shifts.length === 0 && (
                <tr><td colSpan={8} className="px-4 py-10 text-center text-muted-foreground">No shifts in this period.</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>
      {!loading && totalCount > 0 && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm"
          >
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
            <option value={100}>100 / page</option>
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun="shifts" className="mt-0 flex-1" />
        </div>
      )}
    </section>
  );
}

function CategorySalesReportView({ from, to, locationId }: { from: string; to: string; locationId: string }) {
  const [rows, setRows] = useState<CategorySalesRow[]>([]);
  const [totalQty, setTotalQty] = useState(0);
  const [totalRevenue, setTotalRevenue] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  async function load(page: number) {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set('pageNumber', String(page)); params.set('pageSize', String(pageSize));
      params.set('from', from); params.set('to', to);
      if (locationId) params.set('locationId', locationId);
      const res = await apiClient<CategorySalesReport>(`/api/v1/reports/sales/by-category?${params.toString()}`);
      setRows(res.rows); setTotalQty(res.totalQty); setTotalRevenue(res.totalRevenue);
      setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages || 1);
    } catch { /* */ }
    finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { setPageNumber(1); void load(1); /* eslint-disable-next-line */ }, [from, to, locationId]);

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: CategorySalesRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<CategorySalesReport>(`/api/v1/reports/sales/by-category?${params.toString()}`);
        all.push(...res.rows);
      }
      downloadCsv(
        `sales_by_category_${from}_to_${to}.csv`,
        ['Category Code', 'Category', 'Qty', 'Revenue', 'Tax'],
        all.map(r => [r.categoryCode, r.categoryName, r.quantity, r.revenue.toFixed(2), r.tax.toFixed(2)]),
      );
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: CategorySalesRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<CategorySalesReport>(`/api/v1/reports/sales/by-category?${params.toString()}`);
        all.push(...res.rows);
      }
      downloadPdf(`sales_by_category_${from}_to_${to}.pdf`, 'Sales by Category', `Period ${from} to ${to} - amounts in LKR`, [{
        head: ['Category Code', 'Category', 'Qty', 'Revenue', 'Tax'],
        rows: all.map(r => [r.categoryCode, r.categoryName, r.quantity, r.revenue.toFixed(2), r.tax.toFixed(2)]),
      }]);
    } finally { setExporting(false); }
  }

  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);

  return (
    <section>
      <div className="mb-4 flex items-end justify-between">
        <h2 className="font-heading text-xl font-bold">Sales by category</h2>
        <div className="flex items-center gap-3">
          <HeaderStat><Num>{totalQty}</Num> units · <Num>{money(totalRevenue)}</Num></HeaderStat>
          <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
        </div>
      </div>
      <div className="card overflow-hidden">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr><th className="px-4 py-3 font-medium">Category Code</th><th className="px-4 py-3 font-medium">Category</th><th className="px-4 py-3 text-right font-medium">Qty</th><th className="px-4 py-3 text-right font-medium">Revenue</th><th className="px-4 py-3 text-right font-medium">Tax</th></tr>
            </thead>
            <tbody>
              {rows.map((r, i) => (
                <tr key={r.categoryId ?? 'uncategorized'} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="px-4 py-2.5 font-mono text-xs text-muted-foreground">{r.categoryCode}</td>
                  <td className="px-4 py-2.5 font-medium">{r.categoryName}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{r.quantity}</td>
                  <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{money(r.revenue)}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{money(r.tax)}</td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr><td colSpan={5} className="px-4 py-10 text-center text-muted-foreground">No items sold in this period.</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>
      {!loading && totalCount > 0 && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm"
          >
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
            <option value={100}>100 / page</option>
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun="categories" className="mt-0 flex-1" />
        </div>
      )}
    </section>
  );
}

function PromotionsUsageReport({ from, to, locationId }: { from: string; to: string; locationId: string }) {
  const [promotions, setPromotions] = useState<PromoRow[]>([]);
  const [totalDiscount, setTotalDiscount] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  async function load(page: number) {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set('pageNumber', String(page)); params.set('pageSize', String(pageSize));
      params.set('from', from); params.set('to', to);
      if (locationId) params.set('locationId', locationId);
      const res = await apiClient<PromotionUsage>(`/api/v1/reports/promotions?${params.toString()}`);
      setPromotions(res.promotions); setTotalDiscount(res.totalDiscount);
      setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages || 1);
    } catch { /* */ }
    finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { setPageNumber(1); void load(1); /* eslint-disable-next-line */ }, [from, to, locationId]);

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: PromoRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<PromotionUsage>(`/api/v1/reports/promotions?${params.toString()}`);
        all.push(...res.promotions);
      }
      downloadCsv(
        `promotion_usage_${from}_to_${to}.csv`,
        ['Code', 'Promotion', 'Times', 'Discount'],
        all.map(p => [p.code, p.name, p.times, p.discount.toFixed(2)]),
      );
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: PromoRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<PromotionUsage>(`/api/v1/reports/promotions?${params.toString()}`);
        all.push(...res.promotions);
      }
      downloadPdf(`promotion_usage_${from}_to_${to}.pdf`, 'Promotion Usage', `Period ${from} to ${to} - amounts in LKR`, [{
        head: ['Code', 'Promotion', 'Times', 'Discount'],
        rows: all.map(p => [p.code, p.name, p.times, p.discount.toFixed(2)]),
      }]);
    } finally { setExporting(false); }
  }

  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);

  return (
    <section>
      <div className="mb-4 flex items-end justify-between">
        <h2 className="font-heading text-xl font-bold">Promotion usage</h2>
        <div className="flex items-center gap-3">
          <HeaderStat>Total given: <Num>{money(totalDiscount)}</Num></HeaderStat>
          <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
        </div>
      </div>
      <div className="card overflow-hidden">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr><th className="px-4 py-3 font-medium">Code</th><th className="px-4 py-3 font-medium">Promotion</th><th className="px-4 py-3 text-right font-medium">Times</th><th className="px-4 py-3 text-right font-medium">Discount</th></tr>
            </thead>
            <tbody>
              {promotions.map((p, i) => (
                <tr key={p.code} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="px-4 py-2.5 font-mono text-xs">{p.code}</td>
                  <td className="px-4 py-2.5 font-medium">{p.name}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{p.times}</td>
                  <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{money(p.discount)}</td>
                </tr>
              ))}
              {promotions.length === 0 && (
                <tr><td colSpan={4} className="px-4 py-10 text-center text-muted-foreground">No promotions applied in this period.</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>
      {!loading && totalCount > 0 && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm"
          >
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
            <option value={100}>100 / page</option>
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun="promotions" className="mt-0 flex-1" />
        </div>
      )}
    </section>
  );
}

function DiscountsReportView({ from, to, locationId }: { from: string; to: string; locationId: string }) {
  const [rows, setRows] = useState<DiscountRow[]>([]);
  const [totals, setTotals] = useState({ billCount: 0, grossSales: 0, discountTotal: 0 });
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  async function load(page: number) {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set('pageNumber', String(page)); params.set('pageSize', String(pageSize));
      params.set('from', from); params.set('to', to);
      if (locationId) params.set('locationId', locationId);
      const res = await apiClient<DiscountReport>(`/api/v1/reports/discounts?${params.toString()}`);
      setRows(res.rows); setTotals(res.totals);
      setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages || 1);
    } catch { /* */ }
    finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { setPageNumber(1); void load(1); /* eslint-disable-next-line */ }, [from, to, locationId]);

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: DiscountRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<DiscountReport>(`/api/v1/reports/discounts?${params.toString()}`);
        all.push(...res.rows);
      }
      downloadCsv(
        `discounts_${from}_to_${to}.csv`,
        ['Steward', 'Bills', 'Gross Sales', 'Discount', 'Discount %'],
        all.map(r => [r.stewardName, r.billCount, r.grossSales.toFixed(2), r.discountTotal.toFixed(2), `${r.discountPercent}%`]),
      );
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: DiscountRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<DiscountReport>(`/api/v1/reports/discounts?${params.toString()}`);
        all.push(...res.rows);
      }
      downloadPdf(`discounts_${from}_to_${to}.pdf`, 'Discount & Complimentary', `Period ${from} to ${to} - amounts in LKR (lite: manual discount amounts only, no reason/approver/comp tracking yet)`, [{
        head: ['Steward', 'Bills', 'Gross Sales', 'Discount', 'Discount %'],
        rows: all.map(r => [r.stewardName, r.billCount, r.grossSales.toFixed(2), r.discountTotal.toFixed(2), `${r.discountPercent}%`]),
      }]);
    } finally { setExporting(false); }
  }

  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);

  return (
    <section>
      <div className="mb-4 flex items-end justify-between">
        <div>
          <h2 className="font-heading text-xl font-bold">Discount & complimentary</h2>
          <p className="text-sm text-muted-foreground">Manual discount totals per steward — reason/approver and complimentary tracking aren&rsquo;t captured yet, so this shows amounts only.</p>
        </div>
        <div className="flex items-center gap-3">
          <HeaderStat>Total given: <Num>{money(totals.discountTotal)}</Num></HeaderStat>
          <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
        </div>
      </div>
      <div className="card overflow-hidden">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-3 font-medium">Steward</th>
                <th className="px-4 py-3 text-right font-medium">Bills</th>
                <th className="px-4 py-3 text-right font-medium">Gross sales</th>
                <th className="px-4 py-3 text-right font-medium">Discount</th>
                <th className="px-4 py-3 text-right font-medium">Discount %</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r, i) => (
                <tr key={r.stewardId ?? 'unassigned'} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="px-4 py-2.5 font-medium">{r.stewardName}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{r.billCount}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{money(r.grossSales)}</td>
                  <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{money(r.discountTotal)}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{r.discountPercent}%</td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr><td colSpan={5} className="px-4 py-10 text-center text-muted-foreground">No manual discounts given in this period.</td></tr>
              )}
            </tbody>
            {rows.length > 0 && (
              <tfoot className="border-t border-border bg-muted/30 font-bold">
                <tr>
                  <td className="px-4 py-3">Total</td>
                  <td className="px-4 py-3 text-right tabular-nums">{totals.billCount}</td>
                  <td className="px-4 py-3 text-right tabular-nums">{money(totals.grossSales)}</td>
                  <td className="px-4 py-3 text-right tabular-nums">{money(totals.discountTotal)}</td>
                  <td className="px-4 py-3 text-right tabular-nums">{totals.grossSales > 0 ? `${Math.round(totals.discountTotal / totals.grossSales * 1000) / 10}%` : '—'}</td>
                </tr>
              </tfoot>
            )}
          </table>
        )}
      </div>
      {!loading && totalCount > 0 && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm"
          >
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
            <option value={100}>100 / page</option>
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun="stewards" className="mt-0 flex-1" />
        </div>
      )}
    </section>
  );
}

function TableTurnoverReportView({ from, to, locationId }: { from: string; to: string; locationId: string }) {
  const [rows, setRows] = useState<TableTurnoverRow[]>([]);
  const [totals, setTotals] = useState({ billCount: 0, totalCovers: 0, avgDurationMinutes: 0, grossSales: 0 });
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  async function load(page: number) {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set('pageNumber', String(page)); params.set('pageSize', String(pageSize));
      params.set('from', from); params.set('to', to);
      if (locationId) params.set('locationId', locationId);
      const res = await apiClient<TableTurnoverReport>(`/api/v1/reports/table-turnover?${params.toString()}`);
      setRows(res.rows); setTotals(res.totals);
      setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages || 1);
    } catch { /* */ }
    finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { setPageNumber(1); void load(1); /* eslint-disable-next-line */ }, [from, to, locationId]);

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: TableTurnoverRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<TableTurnoverReport>(`/api/v1/reports/table-turnover?${params.toString()}`);
        all.push(...res.rows);
      }
      downloadCsv(
        `table_turnover_${from}_to_${to}.csv`,
        ['Table', 'Bills', 'Covers', 'Avg Duration (min)', 'Gross Sales'],
        all.map(r => [r.tableLabel, r.billCount, r.totalCovers, r.avgDurationMinutes, r.grossSales.toFixed(2)]),
      );
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: TableTurnoverRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<TableTurnoverReport>(`/api/v1/reports/table-turnover?${params.toString()}`);
        all.push(...res.rows);
      }
      downloadPdf(`table_turnover_${from}_to_${to}.pdf`, 'Table Turnover', `Period ${from} to ${to} - amounts in LKR`, [{
        head: ['Table', 'Bills', 'Covers', 'Avg Duration (min)', 'Gross Sales'],
        rows: all.map(r => [r.tableLabel, r.billCount, r.totalCovers, r.avgDurationMinutes, r.grossSales.toFixed(2)]),
      }]);
    } finally { setExporting(false); }
  }

  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);

  return (
    <section>
      <div className="mb-4 flex items-end justify-between">
        <div>
          <h2 className="font-heading text-xl font-bold">Table turnover</h2>
          <p className="text-sm text-muted-foreground">Bills, covers and average occupancy time per dine-in table — how quickly tables turn over.</p>
        </div>
        <div className="flex items-center gap-3">
          <HeaderStat>Avg duration: <Num>{fmtDuration(totals.avgDurationMinutes)}</Num></HeaderStat>
          <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
        </div>
      </div>
      <div className="card overflow-hidden">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-3 font-medium">Table</th>
                <th className="px-4 py-3 text-right font-medium">Bills</th>
                <th className="px-4 py-3 text-right font-medium">Covers</th>
                <th className="px-4 py-3 text-right font-medium">Avg Duration</th>
                <th className="px-4 py-3 text-right font-medium">Gross Sales</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r, i) => (
                <tr key={r.tableLabel} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="px-4 py-2.5 font-medium">Table {r.tableLabel}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{r.billCount}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{r.totalCovers}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{fmtDuration(r.avgDurationMinutes)}</td>
                  <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{money(r.grossSales)}</td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr><td colSpan={5} className="px-4 py-10 text-center text-muted-foreground">No dine-in bills with a table assigned in this period.</td></tr>
              )}
            </tbody>
            {rows.length > 0 && (
              <tfoot className="border-t border-border bg-muted/30 font-bold">
                <tr>
                  <td className="px-4 py-3">Total</td>
                  <td className="px-4 py-3 text-right tabular-nums">{totals.billCount}</td>
                  <td className="px-4 py-3 text-right tabular-nums">{totals.totalCovers}</td>
                  <td className="px-4 py-3 text-right tabular-nums">{fmtDuration(totals.avgDurationMinutes)}</td>
                  <td className="px-4 py-3 text-right tabular-nums">{money(totals.grossSales)}</td>
                </tr>
              </tfoot>
            )}
          </table>
        )}
      </div>
      {!loading && totalCount > 0 && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm"
          >
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
            <option value={100}>100 / page</option>
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun="tables" className="mt-0 flex-1" />
        </div>
      )}
    </section>
  );
}

function StewardSalesReportView({ from, to, locationId }: { from: string; to: string; locationId: string }) {
  const [rows, setRows] = useState<StewardRow[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  async function load(page: number) {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set('pageNumber', String(page)); params.set('pageSize', String(pageSize));
      params.set('from', from); params.set('to', to);
      if (locationId) params.set('locationId', locationId);
      const res = await apiClient<PagedListResult<StewardRow>>(`/api/v1/reports/steward-sales?${params.toString()}`);
      setRows(res.data);
      setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages || 1);
    } catch { /* */ }
    finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { setPageNumber(1); void load(1); /* eslint-disable-next-line */ }, [from, to, locationId]);

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: StewardRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<PagedListResult<StewardRow>>(`/api/v1/reports/steward-sales?${params.toString()}`);
        all.push(...res.data);
      }
      downloadCsv(
        `steward_sales_${from}_to_${to}.csv`,
        ['Steward', 'Bills', 'Covers', 'Gross Sales', 'Tips'],
        all.map(s => [s.name, s.orderCount, s.covers, s.grossSales.toFixed(2), s.tips.toFixed(2)]),
      );
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: StewardRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<PagedListResult<StewardRow>>(`/api/v1/reports/steward-sales?${params.toString()}`);
        all.push(...res.data);
      }
      downloadPdf(`steward_sales_${from}_to_${to}.pdf`, 'Steward Sales', `Period ${from} to ${to} - amounts in LKR`, [{
        head: ['Steward', 'Bills', 'Covers', 'Gross Sales', 'Tips'],
        rows: all.map(s => [s.name, s.orderCount, s.covers, s.grossSales.toFixed(2), s.tips.toFixed(2)]),
      }]);
    } finally { setExporting(false); }
  }

  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);

  return (
    <section>
      <div className="mb-4 flex items-end justify-between">
        <div>
          <h2 className="font-heading text-xl font-bold">Steward sales</h2>
          <p className="text-sm text-muted-foreground">Bills, covers, gross sales and tips per waiter — the basis for a tip payout.</p>
        </div>
        <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
      </div>
      <div className="card overflow-hidden">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-3 font-medium">Steward</th>
                <th className="px-4 py-3 text-right font-medium">Bills</th>
                <th className="px-4 py-3 text-right font-medium">Covers</th>
                <th className="px-4 py-3 text-right font-medium">Gross sales</th>
                <th className="px-4 py-3 text-right font-medium">Tips</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((s, i) => (
                <tr key={s.stewardId ?? 'unassigned'} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="px-4 py-2.5 font-medium">{s.name}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{s.orderCount}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{s.covers}</td>
                  <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{money(s.grossSales)}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{money(s.tips)}</td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr><td colSpan={5} className="px-4 py-10 text-center text-muted-foreground">No settled bills in this period.</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>
      {!loading && totalCount > 0 && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm"
          >
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
            <option value={100}>100 / page</option>
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun="stewards" className="mt-0 flex-1" />
        </div>
      )}
    </section>
  );
}

function TourCommissionReportView({ from, to, locationId }: { from: string; to: string; locationId: string }) {
  const [rows, setRows] = useState<TourCommissionRow[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  async function load(page: number) {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set('pageNumber', String(page)); params.set('pageSize', String(pageSize));
      params.set('from', from); params.set('to', to);
      if (locationId) params.set('locationId', locationId);
      const res = await apiClient<PagedListResult<TourCommissionRow>>(`/api/v1/reports/tour-commission?${params.toString()}`);
      setRows(res.data);
      setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages || 1);
    } catch { /* */ }
    finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { setPageNumber(1); void load(1); /* eslint-disable-next-line */ }, [from, to, locationId]);

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: TourCommissionRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<PagedListResult<TourCommissionRow>>(`/api/v1/reports/tour-commission?${params.toString()}`);
        all.push(...res.data);
      }
      downloadCsv(
        `tour_commission_${from}_to_${to}.csv`,
        ['Code', 'Tour Operator', 'Bills', 'Gross Sales', 'Commission'],
        all.map(t => [t.code, t.name, t.orderCount, t.grossSales.toFixed(2), t.commission.toFixed(2)]),
      );
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: TourCommissionRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<PagedListResult<TourCommissionRow>>(`/api/v1/reports/tour-commission?${params.toString()}`);
        all.push(...res.data);
      }
      downloadPdf(`tour_commission_${from}_to_${to}.pdf`, 'Tour Commission', `Period ${from} to ${to} - amounts in LKR`, [{
        head: ['Code', 'Tour Operator', 'Bills', 'Gross Sales', 'Commission'],
        rows: all.map(t => [t.code, t.name, t.orderCount, t.grossSales.toFixed(2), t.commission.toFixed(2)]),
      }]);
    } finally { setExporting(false); }
  }

  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);

  return (
    <section>
      <div className="mb-4 flex items-end justify-between">
        <div>
          <h2 className="font-heading text-xl font-bold">Tour commission</h2>
          <p className="text-sm text-muted-foreground">Bills, gross sales and commission booked per tour operator — what the venue owes each one.</p>
        </div>
        <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
      </div>
      <div className="card overflow-hidden">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-3 font-medium">Tour operator</th>
                <th className="px-4 py-3 text-right font-medium">Bills</th>
                <th className="px-4 py-3 text-right font-medium">Gross sales</th>
                <th className="px-4 py-3 text-right font-medium">Commission</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((t, i) => (
                <tr key={t.tourOperatorId} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="px-4 py-2.5"><span className="font-medium">{t.name}</span> <span className="font-mono text-xs text-muted-foreground">{t.code}</span></td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{t.orderCount}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{money(t.grossSales)}</td>
                  <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{money(t.commission)}</td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr><td colSpan={4} className="px-4 py-10 text-center text-muted-foreground">No tour-operator bills in this period.</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>
      {!loading && totalCount > 0 && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm"
          >
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
            <option value={100}>100 / page</option>
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun="operators" className="mt-0 flex-1" />
        </div>
      )}
    </section>
  );
}

type SupplierPurchaseRow = {
  supplierId: string; supplierCode: string; supplierName: string;
  grnCount: number; purchases: number; tax: number; discount: number; otherCharges: number; total: number;
};

function PurchasesBySupplierReport({ from, to, locationId }: { from: string; to: string; locationId: string }) {
  const [rows, setRows] = useState<SupplierPurchaseRow[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [totals, setTotals] = useState({ grnCount: 0, purchases: 0, tax: 0, discount: 0, otherCharges: 0, total: 0 });
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  async function load(page: number) {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set('pageNumber', String(page));
      params.set('pageSize', String(pageSize));
      params.set('from', from); params.set('to', to);
      if (locationId) params.set('locationId', locationId);
      const res = await apiClient<{ data: SupplierPurchaseRow[]; pagination: { totalCount: number; totalPages: number }; totals: typeof totals }>(`/api/v1/reports/purchases/by-supplier?${params.toString()}`);
      setRows(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages || 1);
      setTotals(res.totals);
    } catch { /* */ }
    finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { setPageNumber(1); void load(1); /* eslint-disable-next-line */ }, [from, to, locationId]);

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: SupplierPurchaseRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<{ data: SupplierPurchaseRow[] }>(`/api/v1/reports/purchases/by-supplier?${params.toString()}`);
        all.push(...res.data);
      }
      downloadCsv(
        `purchases_by_supplier_${from}_to_${to}.csv`,
        ['Supplier Code', 'Supplier', 'GRNs', 'Purchases', 'Tax', 'Discount', 'Other Charges', 'Total'],
        all.map(r => [r.supplierCode, r.supplierName, r.grnCount, r.purchases.toFixed(2), r.tax.toFixed(2), r.discount.toFixed(2), r.otherCharges.toFixed(2), r.total.toFixed(2)]),
      );
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: SupplierPurchaseRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<{ data: SupplierPurchaseRow[] }>(`/api/v1/reports/purchases/by-supplier?${params.toString()}`);
        all.push(...res.data);
      }
      downloadPdf(`purchases_by_supplier_${from}_to_${to}.pdf`, 'Purchases by Supplier', `Period ${from} to ${to} - total ${money(totals.total)} across ${totals.grnCount} GRNs`, [{
        head: ['Supplier Code', 'Supplier', 'GRNs', 'Purchases', 'Tax', 'Discount', 'Other Charges', 'Total'],
        rows: all.map(r => [r.supplierCode, r.supplierName, r.grnCount, r.purchases.toFixed(2), r.tax.toFixed(2), r.discount.toFixed(2), r.otherCharges.toFixed(2), r.total.toFixed(2)]),
      }]);
    } finally { setExporting(false); }
  }

  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);

  return (
    <section>
      <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
        <div>
          <h2 className="font-heading text-xl font-bold">Purchases by supplier</h2>
          <p className="text-sm text-muted-foreground">Approved GRNs, rolled up per supplier · total <span className="font-semibold text-foreground">{money(totals.total)}</span> across {totals.grnCount} GRNs</p>
        </div>
        <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
      </div>
      <div className="card overflow-hidden">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-3 font-medium">Supplier</th>
                <th className="px-4 py-3 text-right font-medium">GRNs</th>
                <th className="px-4 py-3 text-right font-medium">Purchases</th>
                <th className="px-4 py-3 text-right font-medium">Tax</th>
                <th className="px-4 py-3 text-right font-medium">Discount</th>
                <th className="px-4 py-3 text-right font-medium">Other charges</th>
                <th className="px-4 py-3 text-right font-medium">Total</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r, i) => (
                <tr key={r.supplierId} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="px-4 py-2.5"><span className="font-medium">{r.supplierName}</span> <span className="font-mono text-xs text-muted-foreground">{r.supplierCode}</span></td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{r.grnCount}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{money(r.purchases)}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{money(r.tax)}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{money(r.discount)}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{money(r.otherCharges)}</td>
                  <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{money(r.total)}</td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr><td colSpan={7} className="px-4 py-10 text-center text-muted-foreground">No approved GRNs in this period.</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {!loading && totalCount > 0 && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm"
          >
            <option value={5}>5 / page</option>
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun="suppliers" className="mt-0 flex-1" />
        </div>
      )}
    </section>
  );
}

type VoidOrderRow = {
  orderNumber: string; voidedAt: string;
  locationCode: string; locationName: string;
  orderType: string; tableLabel: string | null; customerName: string | null;
  subtotalAmount: number; discountAmount: number; serviceChargeAmount: number; taxAmount: number; totalAmount: number;
  voidReason: string | null; voidedBy: string;
};

function VoidOrdersReport({ from, to, locationId }: { from: string; to: string; locationId: string }) {
  const [rows, setRows] = useState<VoidOrderRow[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [totalAmount, setTotalAmount] = useState(0);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  async function load(page: number) {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set('pageNumber', String(page));
      params.set('pageSize', String(pageSize));
      params.set('from', from); params.set('to', to);
      if (locationId) params.set('locationId', locationId);
      const res = await apiClient<{ data: VoidOrderRow[]; pagination: { totalCount: number; totalPages: number }; totals: { count: number; amount: number } }>(`/api/v1/reports/void-orders?${params.toString()}`);
      setRows(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages || 1);
      setTotalAmount(res.totals.amount);
    } catch { /* */ }
    finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { setPageNumber(1); void load(1); /* eslint-disable-next-line */ }, [from, to, locationId]);

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: VoidOrderRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<{ data: VoidOrderRow[] }>(`/api/v1/reports/void-orders?${params.toString()}`);
        all.push(...res.data);
      }
      downloadCsv(
        `void_orders_${from}_to_${to}.csv`,
        ['Voided At', 'Bill No', 'Location', 'Type', 'Table / Customer', 'Subtotal', 'Discount', 'Service Charge', 'Tax', 'Total', 'Voided By', 'Reason'],
        all.map(r => [
          new Date(r.voidedAt).toLocaleString('en-LK'), r.orderNumber, `${r.locationCode} - ${r.locationName}`,
          r.orderType.replace('_', '-'), r.tableLabel ? `Table ${r.tableLabel}` : (r.customerName ?? ''),
          r.subtotalAmount.toFixed(2), r.discountAmount.toFixed(2), r.serviceChargeAmount.toFixed(2), r.taxAmount.toFixed(2), r.totalAmount.toFixed(2),
          r.voidedBy, r.voidReason ?? '',
        ]),
      );
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: VoidOrderRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        params.set('from', from); params.set('to', to);
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<{ data: VoidOrderRow[] }>(`/api/v1/reports/void-orders?${params.toString()}`);
        all.push(...res.data);
      }
      downloadPdf(`void_orders_${from}_to_${to}.pdf`, 'Void / Cancellation', `Period ${from} to ${to} - ${totalCount} voided bills, ${money(totalAmount)} total`, [{
        head: ['Voided At', 'Bill No', 'Location', 'Type', 'Table / Customer', 'Total', 'Voided By', 'Reason'],
        rows: all.map(r => [
          new Date(r.voidedAt).toLocaleString('en-LK'), r.orderNumber, `${r.locationCode} - ${r.locationName}`,
          r.orderType.replace('_', '-'), r.tableLabel ? `Table ${r.tableLabel}` : (r.customerName ?? ''),
          r.totalAmount.toFixed(2), r.voidedBy, r.voidReason ?? '',
        ]),
      }]);
    } finally { setExporting(false); }
  }

  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);

  return (
    <section>
      <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
        <div>
          <h2 className="font-heading text-xl font-bold">Void / cancellation</h2>
          <p className="text-sm text-muted-foreground">Voided bills · total <span className="font-semibold text-foreground">{money(totalAmount)}</span> across {totalCount} bills</p>
        </div>
        <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
      </div>
      <div className="card overflow-hidden">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-3 font-medium">Voided At</th>
                <th className="px-4 py-3 font-medium">Bill No</th>
                <th className="px-4 py-3 font-medium">Location</th>
                <th className="px-4 py-3 font-medium">Table / Customer</th>
                <th className="px-4 py-3 text-right font-medium">Total</th>
                <th className="px-4 py-3 font-medium">Voided By</th>
                <th className="px-4 py-3 font-medium">Reason</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r, i) => (
                <tr key={`${r.orderNumber}-${i}`} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="whitespace-nowrap px-4 py-2.5 text-muted-foreground">{new Date(r.voidedAt).toLocaleString('en-LK', { dateStyle: 'medium', timeStyle: 'short' })}</td>
                  <td className="px-4 py-2.5 font-mono text-xs">{r.orderNumber}</td>
                  <td className="px-4 py-2.5"><span className="font-medium">{r.locationName}</span> <span className="font-mono text-xs text-muted-foreground">{r.locationCode}</span></td>
                  <td className="px-4 py-2.5">{r.tableLabel ? `Table ${r.tableLabel}` : (r.customerName ?? '—')}</td>
                  <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{money(r.totalAmount)}</td>
                  <td className="px-4 py-2.5">{r.voidedBy}</td>
                  <td className="px-4 py-2.5 text-muted-foreground">{r.voidReason ?? '—'}</td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr><td colSpan={7} className="px-4 py-10 text-center text-muted-foreground">No voided bills in this period.</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {!loading && totalCount > 0 && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm"
          >
            <option value={5}>5 / page</option>
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun="voided bills" className="mt-0 flex-1" />
        </div>
      )}
    </section>
  );
}

type LowStockRow = {
  locationCode: string; locationName: string;
  sku: string; productName: string;
  onHand: number; reorderLevel: number; parLevel: number; needQty: number;
  unitCost: number; poValue: number; supplierName: string | null;
};

function LowStockReport({ locationId }: { locationId: string }) {
  const [rows, setRows] = useState<LowStockRow[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [poValue, setPoValue] = useState(0);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  async function load(page: number) {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set('pageNumber', String(page));
      params.set('pageSize', String(pageSize));
      if (locationId) params.set('locationId', locationId);
      const res = await apiClient<{ data: LowStockRow[]; pagination: { totalCount: number; totalPages: number }; totals: { itemCount: number; poValue: number } }>(`/api/v1/reports/low-stock?${params.toString()}`);
      setRows(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages || 1);
      setPoValue(res.totals.poValue);
    } catch { /* */ }
    finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { setPageNumber(1); void load(1); /* eslint-disable-next-line */ }, [locationId]);

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: LowStockRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<{ data: LowStockRow[] }>(`/api/v1/reports/low-stock?${params.toString()}`);
        all.push(...res.data);
      }
      downloadCsv(
        `low_stock_${fmtDate(new Date())}.csv`,
        ['Location', 'Product Code', 'Product', 'On Hand', 'Reorder Level', 'Par Level', 'Need Qty', 'Unit Cost', 'PO Value', 'Preferred Supplier'],
        all.map(r => [
          `${r.locationCode} - ${r.locationName}`, r.sku, r.productName,
          r.onHand, r.reorderLevel, r.parLevel, r.needQty, r.unitCost.toFixed(2), r.poValue.toFixed(2), r.supplierName ?? '',
        ]),
      );
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: LowStockRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<{ data: LowStockRow[] }>(`/api/v1/reports/low-stock?${params.toString()}`);
        all.push(...res.data);
      }
      downloadPdf(`low_stock_${fmtDate(new Date())}.pdf`, 'Low Stock / Reorder', `As at ${fmtDate(new Date())} - ${totalCount} items, ${money(poValue)} to reorder`, [{
        head: ['Location', 'Product Code', 'Product', 'On Hand', 'Reorder', 'Par', 'Need', 'Supplier'],
        rows: all.map(r => [
          `${r.locationCode} - ${r.locationName}`, r.sku, r.productName,
          r.onHand, r.reorderLevel, r.parLevel, r.needQty, r.supplierName ?? '—',
        ]),
      }]);
    } finally { setExporting(false); }
  }

  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);

  return (
    <section>
      <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
        <div>
          <h2 className="font-heading text-xl font-bold">Low stock / reorder</h2>
          <p className="text-sm text-muted-foreground">Products at/below their reorder level · <span className="font-semibold text-foreground">{money(poValue)}</span> to reorder across {totalCount} items</p>
        </div>
        <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
      </div>
      <div className="card overflow-hidden">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-3 font-medium">Location</th>
                <th className="px-4 py-3 font-medium">Product</th>
                <th className="px-4 py-3 text-right font-medium">On Hand</th>
                <th className="px-4 py-3 text-right font-medium">Reorder</th>
                <th className="px-4 py-3 text-right font-medium">Par</th>
                <th className="px-4 py-3 text-right font-medium">Need</th>
                <th className="px-4 py-3 font-medium">Supplier</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r, i) => (
                <tr key={`${r.locationCode}-${r.sku}-${i}`} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="px-4 py-2.5"><span className="font-medium">{r.locationName}</span> <span className="font-mono text-xs text-muted-foreground">{r.locationCode}</span></td>
                  <td className="px-4 py-2.5"><span className="font-medium">{r.productName}</span> <span className="font-mono text-xs text-muted-foreground">{r.sku}</span></td>
                  <td className={`px-4 py-2.5 text-right tabular-nums ${r.onHand <= 0 ? 'font-semibold text-status-error' : ''}`}>{r.onHand}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{r.reorderLevel}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{r.parLevel}</td>
                  <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{r.needQty}</td>
                  <td className="px-4 py-2.5">{r.supplierName ?? <span className="text-status-error">No supplier</span>}</td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr><td colSpan={7} className="px-4 py-10 text-center text-muted-foreground">Nothing is below its reorder level right now.</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {!loading && totalCount > 0 && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm"
          >
            <option value={5}>5 / page</option>
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun="items" className="mt-0 flex-1" />
        </div>
      )}
    </section>
  );
}

type SlowMovingRow = {
  locationCode: string; locationName: string;
  sku: string; productName: string;
  onHand: number; unitCost: number; value: number;
  lastSaleAt: string | null; daysSinceLastSale: number | null;
};

function SlowMovingReport({ locationId }: { locationId: string }) {
  const [rows, setRows] = useState<SlowMovingRow[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [totalValue, setTotalValue] = useState(0);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [minDays, setMinDays] = useState(30);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  async function load(page: number) {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      params.set('pageNumber', String(page));
      params.set('pageSize', String(pageSize));
      params.set('minDays', String(minDays));
      if (locationId) params.set('locationId', locationId);
      const res = await apiClient<{ data: SlowMovingRow[]; pagination: { totalCount: number; totalPages: number }; totals: { itemCount: number; value: number } }>(`/api/v1/reports/slow-moving?${params.toString()}`);
      setRows(res.data);
      setTotalCount(res.pagination.totalCount);
      setTotalPages(res.pagination.totalPages || 1);
      setTotalValue(res.totals.value);
    } catch { /* */ }
    finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { setPageNumber(1); void load(1); /* eslint-disable-next-line */ }, [locationId, minDays]);

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: SlowMovingRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap)); params.set('minDays', String(minDays));
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<{ data: SlowMovingRow[] }>(`/api/v1/reports/slow-moving?${params.toString()}`);
        all.push(...res.data);
      }
      downloadCsv(
        `slow_moving_stock_${fmtDate(new Date())}.csv`,
        ['Location', 'Product Code', 'Product', 'On Hand', 'Unit Cost', 'Value', 'Last Sale', 'Days Since'],
        all.map(r => [
          `${r.locationCode} - ${r.locationName}`, r.sku, r.productName,
          r.onHand, r.unitCost.toFixed(2), r.value.toFixed(2),
          r.lastSaleAt ? new Date(r.lastSaleAt).toLocaleDateString('en-LK') : 'Never sold',
          r.daysSinceLastSale ?? '',
        ]),
      );
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: SlowMovingRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap)); params.set('minDays', String(minDays));
        if (locationId) params.set('locationId', locationId);
        const res = await apiClient<{ data: SlowMovingRow[] }>(`/api/v1/reports/slow-moving?${params.toString()}`);
        all.push(...res.data);
      }
      downloadPdf(`slow_moving_stock_${fmtDate(new Date())}.pdf`, 'Slow-Moving Stock', `As at ${fmtDate(new Date())} - ${totalCount} items, ${money(totalValue)} at risk (no sale in ${minDays}+ days)`, [{
        head: ['Location', 'Product Code', 'Product', 'On Hand', 'Value', 'Last Sale', 'Days Since'],
        rows: all.map(r => [
          `${r.locationCode} - ${r.locationName}`, r.sku, r.productName, r.onHand, r.value.toFixed(2),
          r.lastSaleAt ? new Date(r.lastSaleAt).toLocaleDateString('en-LK') : 'Never sold', r.daysSinceLastSale ?? '—',
        ]),
      }]);
    } finally { setExporting(false); }
  }

  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);

  return (
    <section>
      <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
        <div>
          <h2 className="font-heading text-xl font-bold">Slow-moving stock</h2>
          <p className="text-sm text-muted-foreground">On-hand stock with no sale in {minDays}+ days (or never sold) · <span className="font-semibold text-foreground">{money(totalValue)}</span> tied up across {totalCount} items</p>
        </div>
        <div className="flex items-end gap-2">
          <select value={minDays} onChange={e => setMinDays(Number(e.target.value))} className="h-9 rounded-lg border border-border bg-card px-3 text-sm">
            <option value={14}>14+ days idle</option>
            <option value={30}>30+ days idle</option>
            <option value={60}>60+ days idle</option>
            <option value={90}>90+ days idle</option>
          </select>
          <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
        </div>
      </div>
      <div className="card overflow-hidden">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-3 font-medium">Location</th>
                <th className="px-4 py-3 font-medium">Product</th>
                <th className="px-4 py-3 text-right font-medium">On Hand</th>
                <th className="px-4 py-3 text-right font-medium">Value</th>
                <th className="px-4 py-3 font-medium">Last Sale</th>
                <th className="px-4 py-3 text-right font-medium">Days Since</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r, i) => (
                <tr key={`${r.locationCode}-${r.sku}-${i}`} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="px-4 py-2.5"><span className="font-medium">{r.locationName}</span> <span className="font-mono text-xs text-muted-foreground">{r.locationCode}</span></td>
                  <td className="px-4 py-2.5"><span className="font-medium">{r.productName}</span> <span className="font-mono text-xs text-muted-foreground">{r.sku}</span></td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{r.onHand}</td>
                  <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{money(r.value)}</td>
                  <td className="px-4 py-2.5 text-muted-foreground">{r.lastSaleAt ? new Date(r.lastSaleAt).toLocaleDateString('en-LK') : <span className="pill pill-idle">Never sold</span>}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-status-error">{r.daysSinceLastSale ?? '—'}</td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr><td colSpan={6} className="px-4 py-10 text-center text-muted-foreground">Nothing has been sitting idle this long.</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {!loading && totalCount > 0 && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm"
          >
            <option value={5}>5 / page</option>
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun="items" className="mt-0 flex-1" />
        </div>
      )}
    </section>
  );
}

function BinCardReport({ locations }: { locations: Location[] }) {
  const [products, setProducts] = useState<{ id: string; name: string }[]>([]);
  const [loc, setLoc] = useState('');
  const [pid, setPid] = useState('');
  const [bin, setBin] = useState<BinCard | null>(null);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    apiClient<{ id: string; name: string }[]>('/api/v1/products').then(setProducts).catch(() => {});
  }, []);

  async function loadBin() { if (!pid) return; setBusy(true); try { setBin(await apiClient<BinCard>(`/api/v1/reports/bin-card?productId=${pid}${loc ? `&locationId=${loc}` : ''}`)); } catch { /* */ } finally { setBusy(false); } }

  function exportCsv() {
    if (!bin) return;
    downloadCsv(
      `bin_card_${fmtDate(new Date())}.csv`,
      ['Date', 'Type', 'Doc', 'In', 'Out', 'Balance'],
      [
        ['', 'Opening balance', '', '', '', bin.opening],
        ...bin.lines.map(l => [new Date(l.date).toLocaleDateString(), l.type, l.doc, l.qty > 0 ? l.qty : '', l.qty < 0 ? -l.qty : '', l.balance]),
        ['', `Closing (in ${bin.totalIn} / out ${bin.totalOut})`, '', '', '', bin.closing],
      ],
    );
  }

  function exportPdf() {
    if (!bin) return;
    downloadPdf(`bin_card_${fmtDate(new Date())}.pdf`, 'Bin Card', `As at ${fmtDate(new Date())}`, [{
      head: ['Date', 'Type', 'Doc', 'In', 'Out', 'Balance'],
      rows: [
        ['', 'Opening balance', '', '', '', bin.opening],
        ...bin.lines.map(l => [new Date(l.date).toLocaleDateString(), l.type, l.doc, l.qty > 0 ? l.qty : '', l.qty < 0 ? -l.qty : '', l.balance]),
        ['', `Closing (in ${bin.totalIn} / out ${bin.totalOut})`, '', '', '', bin.closing],
      ],
      boldLastRow: true,
    }]);
  }

  return (
    <section>
      <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
        <div><h2 className="font-heading text-xl font-bold">Bin card</h2><p className="text-sm text-muted-foreground">Stock movements for a product (last 30 days)</p></div>
        <div className="flex flex-wrap items-end gap-2">
          <Combobox
            className="w-56"
            label="Outlet"
            value={loc}
            onChange={setLoc}
            placeholder="All outlets"
            options={[{ value: '', label: 'All outlets' }, ...locations.map(l => ({ value: l.id, label: `${l.code} — ${l.name}` }))]}
          />
          <Combobox
            className="w-56"
            label="Product"
            value={pid}
            onChange={setPid}
            placeholder="— product —"
            options={products.map(p => ({ value: p.id, label: p.name }))}
          />
          <button onClick={loadBin} disabled={!pid || busy} className="rounded-lg bg-primary px-4 py-2 text-sm font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{busy ? 'Loading…' : 'Run'}</button>
          <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={!bin} />
        </div>
      </div>
      <div className="card p-5">
        {bin ? (
          <table className="w-full text-sm">
            <thead className="border-b border-border text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr><th className="py-2">Date</th><th className="py-2">Type</th><th className="py-2">Doc</th><th className="py-2 text-right">In</th><th className="py-2 text-right">Out</th><th className="py-2 text-right">Balance</th></tr>
            </thead>
            <tbody>
              <tr className="border-b border-border/40 text-muted-foreground"><td className="py-2" colSpan={5}>Opening balance</td><td className="py-2 text-right font-semibold tabular-nums">{bin.opening}</td></tr>
              {bin.lines.map((l, i) => (
                <tr key={i} className="border-b border-border/40">
                  <td className="py-2 text-muted-foreground">{new Date(l.date).toLocaleDateString()}</td>
                  <td className="py-2">{l.type}</td>
                  <td className="py-2 font-mono text-xs text-muted-foreground">{l.doc}</td>
                  <td className="py-2 text-right tabular-nums text-primary">{l.qty > 0 ? l.qty : ''}</td>
                  <td className="py-2 text-right tabular-nums text-status-error">{l.qty < 0 ? -l.qty : ''}</td>
                  <td className="py-2 text-right font-medium tabular-nums">{l.balance}</td>
                </tr>
              ))}
              <tr className="font-bold"><td className="py-2" colSpan={3}>Closing · in {bin.totalIn} / out {bin.totalOut}</td><td colSpan={2}></td><td className="py-2 text-right tabular-nums">{bin.closing}</td></tr>
            </tbody>
          </table>
        ) : (
          <p className="py-6 text-center text-sm text-muted-foreground">Pick a product and click Run.</p>
        )}
      </div>
    </section>
  );
}

function BudgetVsSalesReport({ locations }: { locations: Location[] }) {
  const [loc, setLoc] = useState('');
  const [bvs, setBvs] = useState<BvsRow[] | null>(null);
  const [budMonth, setBudMonth] = useState('');
  const [budAmount, setBudAmount] = useState('');
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [busy, setBusy] = useState(false);
  const [exporting, setExporting] = useState(false);

  async function loadBvs(page: number) {
    setBusy(true);
    try {
      const params = new URLSearchParams();
      params.set('pageNumber', String(page)); params.set('pageSize', String(pageSize));
      if (loc) params.set('locationId', loc);
      const res = await apiClient<{ rows: BvsRow[]; pagination: { totalCount: number; totalPages: number } }>(`/api/v1/reports/budget-vs-sales?${params.toString()}`);
      setBvs(res.rows);
      setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages || 1);
    } catch { /* */ } finally { setBusy(false); }
  }
  useEffect(() => { if (bvs !== null) void loadBvs(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);

  function runReport() { setPageNumber(1); void loadBvs(1); }

  async function saveBudget() {
    if (!budMonth || !budAmount) return;   // outlet optional: blank ⇒ company-wide target
    try { await apiClient('/api/v1/budgets', { method: 'PUT', body: JSON.stringify({ locationId: loc || null, month: `${budMonth}-01`, amount: Number(budAmount) }) }); setBudAmount(''); void loadBvs(pageNumber); }
    catch { /* */ }
  }

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: BvsRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        if (loc) params.set('locationId', loc);
        const res = await apiClient<{ rows: BvsRow[] }>(`/api/v1/reports/budget-vs-sales?${params.toString()}`);
        all.push(...res.rows);
      }
      downloadCsv(
        `budget_vs_sales_${fmtDate(new Date())}.csv`,
        ['Month', 'Budget', 'Actual', 'Variance', '% of Target'],
        all.map(r => [r.month.slice(0, 7), r.budget.toFixed(2), r.actual.toFixed(2), r.variance.toFixed(2), r.pct != null ? `${r.pct}%` : '']),
      );
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all: BvsRow[] = [];
      const cap = 100;
      const pages = Math.ceil(totalCount / cap);
      for (let p = 1; p <= pages; p++) {
        const params = new URLSearchParams();
        params.set('pageNumber', String(p)); params.set('pageSize', String(cap));
        if (loc) params.set('locationId', loc);
        const res = await apiClient<{ rows: BvsRow[] }>(`/api/v1/reports/budget-vs-sales?${params.toString()}`);
        all.push(...res.rows);
      }
      downloadPdf(`budget_vs_sales_${fmtDate(new Date())}.pdf`, 'Budget vs Sales', `As at ${fmtDate(new Date())} - amounts in LKR`, [{
        head: ['Month', 'Budget', 'Actual', 'Variance', '% of Target'],
        rows: all.map(r => [r.month.slice(0, 7), r.budget.toFixed(2), r.actual.toFixed(2), r.variance.toFixed(2), r.pct != null ? `${r.pct}%` : '']),
      }]);
    } finally { setExporting(false); }
  }

  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);

  return (
    <section>
      <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
        <div><h2 className="font-heading text-xl font-bold">Budget vs sales</h2><p className="text-sm text-muted-foreground">Monthly sales target vs actual {loc ? '(selected outlet)' : '(all outlets)'}</p></div>
        <div className="flex flex-wrap items-end gap-2">
          <Combobox
            className="w-56"
            label="Outlet"
            value={loc}
            onChange={setLoc}
            placeholder="All outlets"
            options={[{ value: '', label: 'All outlets' }, ...locations.map(l => ({ value: l.id, label: `${l.code} — ${l.name}` }))]}
          />
          <Field label="Month" type="month" value={budMonth} onChange={setBudMonth} />
          <Field className="w-32" label="Budget LKR" inputMode="decimal" value={budAmount} onChange={v => setBudAmount(v.replace(/[^0-9.]/g, ''))} placeholder="budget LKR" />
          <button onClick={saveBudget} disabled={!budMonth || !budAmount} title={loc ? 'Set this outlet’s target' : 'Set a company-wide (all-outlets) target'} className="rounded-lg border border-border bg-card px-3 py-2 text-sm font-medium hover:bg-muted disabled:opacity-50">Set Budget</button>
          <button onClick={runReport} disabled={busy} className="rounded-lg bg-primary px-4 py-2 text-sm font-bold text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{busy ? 'Loading…' : 'Run'}</button>
          <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
        </div>
      </div>
      <div className="card p-5">
        {bvs ? (
          <table className="w-full text-sm">
            <thead className="border-b border-border text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr><th className="py-2">Month</th><th className="py-2 text-right">Budget</th><th className="py-2 text-right">Actual</th><th className="py-2 text-right">Variance</th><th className="py-2 text-right">% of target</th></tr>
            </thead>
            <tbody>
              {bvs.map(r => (
                <tr key={r.month} className="border-b border-border/40">
                  <td className="py-2">{r.month.slice(0, 7)}</td>
                  <td className="py-2 text-right tabular-nums">{money(r.budget)}</td>
                  <td className="py-2 text-right tabular-nums">{money(r.actual)}</td>
                  <td className={`py-2 text-right tabular-nums ${r.variance < 0 ? 'text-status-error' : 'text-primary'}`}>{r.variance >= 0 ? '+' : ''}{money(r.variance)}</td>
                  <td className="py-2 text-right tabular-nums font-semibold">{r.pct != null ? `${r.pct}%` : '—'}</td>
                </tr>
              ))}
              {bvs.length === 0 && <tr><td colSpan={5} className="py-6 text-center text-muted-foreground">No budgets or sales in range. Set a budget above.</td></tr>}
            </tbody>
          </table>
        ) : (
          <p className="py-6 text-center text-sm text-muted-foreground">Click Run to load budget vs sales.</p>
        )}
      </div>
      {bvs !== null && totalCount > 0 && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
          <select
            value={pageSize}
            onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm"
          >
            <option value={10}>10 / page</option>
            <option value={25}>25 / page</option>
            <option value={50}>50 / page</option>
            <option value={100}>100 / page</option>
          </select>
          <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun="months" className="mt-0 flex-1" />
        </div>
      )}
    </section>
  );
}

type ProductMasterRow = {
  id: string; sku: string; barcode: string | null; name: string; productType: string;
  categoryName: string | null; departmentName: string | null; unitOfMeasure: string | null; basePrice: number; costPrice: number;
  taxClass: string; isTaxable: boolean; isSold: boolean; isPurchased: boolean; isStocked: boolean; isActive: boolean;
  reorderLevel: number | null; parLevel: number | null; preferredSupplierName: string | null;
};

/** Shared active-state filter used by every master data report. */
function ActiveFilter({ value, onChange }: { value: string; onChange: (v: string) => void }) {
  return (
    <select value={value} onChange={e => onChange(e.target.value)} className="rounded-lg border border-border bg-card px-3 py-2 text-sm">
      <option value="">All</option>
      <option value="true">Active only</option>
      <option value="false">Inactive only</option>
    </select>
  );
}

/** Shared free-text search box used by every master data report. */
function SearchBox({ value, onChange, placeholder }: { value: string; onChange: (v: string) => void; placeholder: string }) {
  return (
    <input
      value={value}
      onChange={e => onChange(e.target.value)}
      placeholder={placeholder}
      className="w-64 rounded-lg border border-border bg-card px-3 py-2 text-sm"
    />
  );
}

function MasterDataPager({ pageNumber, pageSize, totalCount, totalPages, setPageNumber, setPageSize, noun }: {
  pageNumber: number; pageSize: number; totalCount: number; totalPages: number;
  setPageNumber: (updater: number | ((p: number) => number)) => void; setPageSize: (n: number) => void; noun: string;
}) {
  const from_ = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to_ = Math.min(pageNumber * pageSize, totalCount);
  if (totalCount === 0) return null;
  return (
    <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
      <select
        value={pageSize}
        onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
        className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm"
      >
        <option value={25}>25 / page</option>
        <option value={50}>50 / page</option>
        <option value={100}>100 / page</option>
        <option value={200}>200 / page</option>
      </select>
      <Pagination page={pageNumber} totalPages={totalPages} total={totalCount} from={from_} to={to_} setPage={setPageNumber} noun={noun} className="mt-0 flex-1" />
    </div>
  );
}

function ProductsMasterReport() {
  const [rows, setRows] = useState<ProductMasterRow[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [search, setSearch] = useState('');
  const [activeFilter, setActiveFilter] = useState('');
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  function query(page: number, size: number): string {
    const params = new URLSearchParams();
    params.set('pageNumber', String(page)); params.set('pageSize', String(size));
    if (search.trim()) params.set('search', search.trim());
    if (activeFilter) params.set('isActive', activeFilter);
    return params.toString();
  }

  async function load(page: number) {
    setLoading(true);
    try {
      const res = await apiClient<PagedListResult<ProductMasterRow>>(`/api/v1/reports/master-data/products?${query(page, pageSize)}`);
      setRows(res.data); setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages || 1);
    } catch { /* */ } finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { const t = setTimeout(() => { setPageNumber(1); void load(1); }, 300); return () => clearTimeout(t); /* eslint-disable-next-line */ }, [search, activeFilter]);

  async function fetchAll(): Promise<ProductMasterRow[]> {
    const all: ProductMasterRow[] = [];
    const cap = 200;
    const pages = Math.ceil(totalCount / cap);
    for (let p = 1; p <= pages; p++) {
      const res = await apiClient<PagedListResult<ProductMasterRow>>(`/api/v1/reports/master-data/products?${query(p, cap)}`);
      all.push(...res.data);
    }
    return all;
  }

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all = await fetchAll();
      downloadCsv('products_master.csv',
        ['SKU', 'Barcode', 'Name', 'Category', 'Department', 'UOM', 'Product Type', 'Base Price', 'Cost Price', 'Tax Class', 'Sold', 'Purchased', 'Stocked', 'Reorder', 'Par', 'Preferred Supplier'],
        all.map(r => [r.sku, r.barcode ?? '', r.name, r.categoryName ?? '', r.departmentName ?? '', r.unitOfMeasure ?? '', r.productType, r.basePrice.toFixed(2), r.costPrice.toFixed(2), r.taxClass,
          r.isSold ? 'Yes' : 'No', r.isPurchased ? 'Yes' : 'No', r.isStocked ? 'Yes' : 'No', r.reorderLevel ?? '', r.parLevel ?? '', r.preferredSupplierName ?? '']));
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all = await fetchAll();
      downloadPdf('products_master.pdf', 'Product Master', `${totalCount} products`, [{
        head: ['SKU', 'Name', 'Category', 'Department', 'UOM', 'Product Type', 'Base Price', 'Cost Price'],
        rows: all.map(r => [r.sku, r.name, r.categoryName ?? '—', r.departmentName ?? '—', r.unitOfMeasure ?? '—', r.productType, r.basePrice.toFixed(2), r.costPrice.toFixed(2)]),
      }]);
    } finally { setExporting(false); }
  }

  return (
    <section>
      <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
        <div>
          <h2 className="font-heading text-xl font-bold">Products</h2>
          <p className="text-sm text-muted-foreground">{totalCount} product{totalCount === 1 ? '' : 's'} in the catalog</p>
        </div>
        <div className="flex flex-wrap items-end gap-2">
          <SearchBox value={search} onChange={setSearch} placeholder="Search SKU, name, barcode…" />
          <ActiveFilter value={activeFilter} onChange={setActiveFilter} />
          <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
        </div>
      </div>
      <div className="card overflow-hidden overflow-x-auto">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-3 font-medium">Product</th>
                <th className="px-4 py-3 font-medium">Category</th>
                <th className="px-4 py-3 font-medium">Department</th>
                <th className="px-4 py-3 font-medium">UOM</th>
                <th className="px-4 py-3 font-medium">Product Type</th>
                <th className="px-4 py-3 text-right font-medium">Base Price</th>
                <th className="px-4 py-3 text-right font-medium">Cost Price</th>
                <th className="px-4 py-3 font-medium">Tax Class</th>
                <th className="px-4 py-3 font-medium">Supplier</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r, i) => (
                <tr key={r.id} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="px-4 py-2.5"><span className="font-medium">{r.name}</span> <span className="font-mono text-xs text-muted-foreground">{r.sku}</span></td>
                  <td className="px-4 py-2.5">{r.categoryName ?? '—'}</td>
                  <td className="px-4 py-2.5">{r.departmentName ?? '—'}</td>
                  <td className="px-4 py-2.5">{r.unitOfMeasure ?? '—'}</td>
                  <td className="px-4 py-2.5">{r.productType}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{money(r.basePrice)}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums text-muted-foreground">{money(r.costPrice)}</td>
                  <td className="px-4 py-2.5">{r.taxClass}</td>
                  <td className="px-4 py-2.5">{r.preferredSupplierName ?? '—'}</td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr><td colSpan={9} className="px-4 py-10 text-center text-muted-foreground">No products match this filter.</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>
      {!loading && <MasterDataPager pageNumber={pageNumber} pageSize={pageSize} totalCount={totalCount} totalPages={totalPages} setPageNumber={setPageNumber} setPageSize={setPageSize} noun="products" />}
    </section>
  );
}

type SupplierMasterRow = {
  id: string; code: string; name: string; contactName: string | null; phone: string | null; email: string | null; address: string | null;
  paymentTermsDays: number; isVatRegistered: boolean; vatRegistrationNumber: string | null; isActive: boolean;
  groupName: string | null; typeName: string | null;
};

function SuppliersMasterReport() {
  const [rows, setRows] = useState<SupplierMasterRow[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [search, setSearch] = useState('');
  const [activeFilter, setActiveFilter] = useState('');
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  function query(page: number, size: number): string {
    const params = new URLSearchParams();
    params.set('pageNumber', String(page)); params.set('pageSize', String(size));
    if (search.trim()) params.set('search', search.trim());
    if (activeFilter) params.set('isActive', activeFilter);
    return params.toString();
  }

  async function load(page: number) {
    setLoading(true);
    try {
      const res = await apiClient<PagedListResult<SupplierMasterRow>>(`/api/v1/reports/master-data/suppliers?${query(page, pageSize)}`);
      setRows(res.data); setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages || 1);
    } catch { /* */ } finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { const t = setTimeout(() => { setPageNumber(1); void load(1); }, 300); return () => clearTimeout(t); /* eslint-disable-next-line */ }, [search, activeFilter]);

  async function fetchAll(): Promise<SupplierMasterRow[]> {
    const all: SupplierMasterRow[] = [];
    const cap = 200;
    const pages = Math.ceil(totalCount / cap);
    for (let p = 1; p <= pages; p++) {
      const res = await apiClient<PagedListResult<SupplierMasterRow>>(`/api/v1/reports/master-data/suppliers?${query(p, cap)}`);
      all.push(...res.data);
    }
    return all;
  }

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all = await fetchAll();
      downloadCsv('suppliers_master.csv',
        ['Code', 'Name', 'Contact', 'Phone', 'Email', 'Group', 'Type', 'Payment Terms (days)', 'VAT Registered', 'VAT No', 'Active'],
        all.map(r => [r.code, r.name, r.contactName ?? '', r.phone ?? '', r.email ?? '', r.groupName ?? '', r.typeName ?? '', r.paymentTermsDays,
          r.isVatRegistered ? 'Yes' : 'No', r.vatRegistrationNumber ?? '', r.isActive ? 'Yes' : 'No']));
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all = await fetchAll();
      downloadPdf('suppliers_master.pdf', 'Supplier Master', `${totalCount} suppliers`, [{
        head: ['Code', 'Name', 'Contact', 'Phone', 'Group', 'Type', 'Active'],
        rows: all.map(r => [r.code, r.name, r.contactName ?? '—', r.phone ?? '—', r.groupName ?? '—', r.typeName ?? '—', r.isActive ? 'Yes' : 'No']),
      }]);
    } finally { setExporting(false); }
  }

  return (
    <section>
      <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
        <div>
          <h2 className="font-heading text-xl font-bold">Suppliers</h2>
          <p className="text-sm text-muted-foreground">{totalCount} supplier{totalCount === 1 ? '' : 's'} on file</p>
        </div>
        <div className="flex flex-wrap items-end gap-2">
          <SearchBox value={search} onChange={setSearch} placeholder="Search code, name, phone, email…" />
          <ActiveFilter value={activeFilter} onChange={setActiveFilter} />
          <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
        </div>
      </div>
      <div className="card overflow-hidden overflow-x-auto">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-3 font-medium">Supplier</th>
                <th className="px-4 py-3 font-medium">Contact</th>
                <th className="px-4 py-3 font-medium">Group</th>
                <th className="px-4 py-3 font-medium">Type</th>
                <th className="px-4 py-3 text-right font-medium">Terms</th>
                <th className="px-4 py-3 font-medium">VAT No</th>
                <th className="px-4 py-3 font-medium">Status</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r, i) => (
                <tr key={r.id} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="px-4 py-2.5"><span className="font-medium">{r.name}</span> <span className="font-mono text-xs text-muted-foreground">{r.code}</span></td>
                  <td className="px-4 py-2.5">{r.contactName ?? '—'}<div className="text-xs text-muted-foreground">{[r.phone, r.email].filter(Boolean).join(' · ') || '—'}</div></td>
                  <td className="px-4 py-2.5">{r.groupName ?? '—'}</td>
                  <td className="px-4 py-2.5">{r.typeName ?? '—'}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{r.paymentTermsDays}d</td>
                  <td className="px-4 py-2.5">{r.isVatRegistered ? (r.vatRegistrationNumber ?? 'Yes') : '—'}</td>
                  <td className="px-4 py-2.5">{r.isActive ? <span className="text-primary">Active</span> : <span className="text-muted-foreground">Inactive</span>}</td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr><td colSpan={7} className="px-4 py-10 text-center text-muted-foreground">No suppliers match this filter.</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>
      {!loading && <MasterDataPager pageNumber={pageNumber} pageSize={pageSize} totalCount={totalCount} totalPages={totalPages} setPageNumber={setPageNumber} setPageSize={setPageSize} noun="suppliers" />}
    </section>
  );
}

type CustomerMasterRow = {
  id: string; code: string; name: string; phone: string | null; email: string | null; address: string | null; taxNo: string | null;
  categoryName: string | null; discountPercent: number; isCreditCustomer: boolean; creditLimit: number; currentBalance: number; isActive: boolean;
};

function CustomersMasterReport() {
  const [rows, setRows] = useState<CustomerMasterRow[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [search, setSearch] = useState('');
  const [activeFilter, setActiveFilter] = useState('');
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  function query(page: number, size: number): string {
    const params = new URLSearchParams();
    params.set('pageNumber', String(page)); params.set('pageSize', String(size));
    if (search.trim()) params.set('search', search.trim());
    if (activeFilter) params.set('isActive', activeFilter);
    return params.toString();
  }

  async function load(page: number) {
    setLoading(true);
    try {
      const res = await apiClient<PagedListResult<CustomerMasterRow>>(`/api/v1/reports/master-data/customers?${query(page, pageSize)}`);
      setRows(res.data); setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages || 1);
    } catch { /* */ } finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { const t = setTimeout(() => { setPageNumber(1); void load(1); }, 300); return () => clearTimeout(t); /* eslint-disable-next-line */ }, [search, activeFilter]);

  async function fetchAll(): Promise<CustomerMasterRow[]> {
    const all: CustomerMasterRow[] = [];
    const cap = 200;
    const pages = Math.ceil(totalCount / cap);
    for (let p = 1; p <= pages; p++) {
      const res = await apiClient<PagedListResult<CustomerMasterRow>>(`/api/v1/reports/master-data/customers?${query(p, cap)}`);
      all.push(...res.data);
    }
    return all;
  }

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all = await fetchAll();
      downloadCsv('customers_master.csv',
        ['Code', 'Name', 'Phone', 'Email', 'Category', 'Discount %', 'Credit Customer', 'Credit Limit', 'Balance', 'Active'],
        all.map(r => [r.code, r.name, r.phone ?? '', r.email ?? '', r.categoryName ?? '', r.discountPercent.toFixed(2),
          r.isCreditCustomer ? 'Yes' : 'No', r.creditLimit.toFixed(2), r.currentBalance.toFixed(2), r.isActive ? 'Yes' : 'No']));
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all = await fetchAll();
      downloadPdf('customers_master.pdf', 'Customer Master', `${totalCount} customers`, [{
        head: ['Code', 'Name', 'Phone', 'Category', 'Credit Limit', 'Balance', 'Active'],
        rows: all.map(r => [r.code, r.name, r.phone ?? '—', r.categoryName ?? '—', r.creditLimit.toFixed(2), r.currentBalance.toFixed(2), r.isActive ? 'Yes' : 'No']),
      }]);
    } finally { setExporting(false); }
  }

  return (
    <section>
      <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
        <div>
          <h2 className="font-heading text-xl font-bold">Customers</h2>
          <p className="text-sm text-muted-foreground">{totalCount} customer{totalCount === 1 ? '' : 's'} on file</p>
        </div>
        <div className="flex flex-wrap items-end gap-2">
          <SearchBox value={search} onChange={setSearch} placeholder="Search code, name, phone, email…" />
          <ActiveFilter value={activeFilter} onChange={setActiveFilter} />
          <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
        </div>
      </div>
      <div className="card overflow-hidden overflow-x-auto">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-3 font-medium">Customer</th>
                <th className="px-4 py-3 font-medium">Contact</th>
                <th className="px-4 py-3 font-medium">Category</th>
                <th className="px-4 py-3 text-right font-medium">Discount</th>
                <th className="px-4 py-3 text-right font-medium">Credit Limit</th>
                <th className="px-4 py-3 text-right font-medium">Balance</th>
                <th className="px-4 py-3 font-medium">Status</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r, i) => (
                <tr key={r.id} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="px-4 py-2.5"><span className="font-medium">{r.name}</span> <span className="font-mono text-xs text-muted-foreground">{r.code}</span></td>
                  <td className="px-4 py-2.5">{[r.phone, r.email].filter(Boolean).join(' · ') || '—'}</td>
                  <td className="px-4 py-2.5">{r.categoryName ?? '—'}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{r.discountPercent > 0 ? `${r.discountPercent}%` : '—'}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{r.isCreditCustomer ? money(r.creditLimit) : '—'}</td>
                  <td className={`px-4 py-2.5 text-right tabular-nums ${r.currentBalance > 0 ? 'font-semibold text-status-error' : ''}`}>{r.isCreditCustomer ? money(r.currentBalance) : '—'}</td>
                  <td className="px-4 py-2.5">{r.isActive ? <span className="text-primary">Active</span> : <span className="text-muted-foreground">Inactive</span>}</td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr><td colSpan={7} className="px-4 py-10 text-center text-muted-foreground">No customers match this filter.</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>
      {!loading && <MasterDataPager pageNumber={pageNumber} pageSize={pageSize} totalCount={totalCount} totalPages={totalPages} setPageNumber={setPageNumber} setPageSize={setPageSize} noun="customers" />}
    </section>
  );
}

type LocationMasterRow = {
  id: string; code: string; name: string; city: string; locationType: string; currency: string;
  vatExempt: boolean; canSell: boolean; canProduce: boolean; canStock: boolean; isActive: boolean;
};

const LOCATION_TYPE_LABEL: Record<string, string> = {
  head_office: 'Head Office', central_kitchen: 'Central Kitchen', warehouse: 'Warehouse', outlet: 'Outlet',
};

function LocationsMasterReport() {
  const [rows, setRows] = useState<LocationMasterRow[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [search, setSearch] = useState('');
  const [activeFilter, setActiveFilter] = useState('');
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  function query(page: number, size: number): string {
    const params = new URLSearchParams();
    params.set('pageNumber', String(page)); params.set('pageSize', String(size));
    if (search.trim()) params.set('search', search.trim());
    if (activeFilter) params.set('isActive', activeFilter);
    return params.toString();
  }

  async function load(page: number) {
    setLoading(true);
    try {
      const res = await apiClient<PagedListResult<LocationMasterRow>>(`/api/v1/reports/master-data/locations?${query(page, pageSize)}`);
      setRows(res.data); setTotalCount(res.pagination.totalCount); setTotalPages(res.pagination.totalPages || 1);
    } catch { /* */ } finally { setLoading(false); }
  }

  useEffect(() => { void load(pageNumber); /* eslint-disable-next-line */ }, [pageNumber, pageSize]);
  useEffect(() => { const t = setTimeout(() => { setPageNumber(1); void load(1); }, 300); return () => clearTimeout(t); /* eslint-disable-next-line */ }, [search, activeFilter]);

  async function fetchAll(): Promise<LocationMasterRow[]> {
    const all: LocationMasterRow[] = [];
    const cap = 200;
    const pages = Math.ceil(totalCount / cap);
    for (let p = 1; p <= pages; p++) {
      const res = await apiClient<PagedListResult<LocationMasterRow>>(`/api/v1/reports/master-data/locations?${query(p, cap)}`);
      all.push(...res.data);
    }
    return all;
  }

  async function exportCsv() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all = await fetchAll();
      downloadCsv('locations_master.csv',
        ['Code', 'Name', 'City', 'Type', 'Currency', 'VAT Exempt', 'Can Sell', 'Can Produce', 'Can Stock', 'Active'],
        all.map(r => [r.code, r.name, r.city, LOCATION_TYPE_LABEL[r.locationType] ?? r.locationType, r.currency,
          r.vatExempt ? 'Yes' : 'No', r.canSell ? 'Yes' : 'No', r.canProduce ? 'Yes' : 'No', r.canStock ? 'Yes' : 'No', r.isActive ? 'Yes' : 'No']));
    } finally { setExporting(false); }
  }

  async function exportPdf() {
    if (totalCount === 0) return;
    setExporting(true);
    try {
      const all = await fetchAll();
      downloadPdf('locations_master.pdf', 'Location Master', `${totalCount} locations`, [{
        head: ['Code', 'Name', 'City', 'Type', 'Currency', 'Active'],
        rows: all.map(r => [r.code, r.name, r.city, LOCATION_TYPE_LABEL[r.locationType] ?? r.locationType, r.currency, r.isActive ? 'Yes' : 'No']),
      }]);
    } finally { setExporting(false); }
  }

  return (
    <section>
      <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
        <div>
          <h2 className="font-heading text-xl font-bold">Locations</h2>
          <p className="text-sm text-muted-foreground">{totalCount} location{totalCount === 1 ? '' : 's'} configured</p>
        </div>
        <div className="flex flex-wrap items-end gap-2">
          <SearchBox value={search} onChange={setSearch} placeholder="Search code, name, city…" />
          <ActiveFilter value={activeFilter} onChange={setActiveFilter} />
          <ExportButton onCsv={exportCsv} onPdf={exportPdf} disabled={totalCount === 0 || exporting} />
        </div>
      </div>
      <div className="card overflow-hidden overflow-x-auto">
        {loading ? (
          <div className="space-y-2 p-4">{Array.from({ length: 5 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-3 font-medium">Location</th>
                <th className="px-4 py-3 font-medium">City</th>
                <th className="px-4 py-3 font-medium">Type</th>
                <th className="px-4 py-3 font-medium">Currency</th>
                <th className="px-4 py-3 font-medium">Capabilities</th>
                <th className="px-4 py-3 font-medium">VAT</th>
                <th className="px-4 py-3 font-medium">Status</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r, i) => (
                <tr key={r.id} className={i % 2 ? 'bg-muted/20' : ''}>
                  <td className="px-4 py-2.5"><span className="font-medium">{r.name}</span> <span className="font-mono text-xs text-muted-foreground">{r.code}</span></td>
                  <td className="px-4 py-2.5">{r.city}</td>
                  <td className="px-4 py-2.5">{LOCATION_TYPE_LABEL[r.locationType] ?? r.locationType}</td>
                  <td className="px-4 py-2.5">{r.currency}</td>
                  <td className="px-4 py-2.5 text-xs text-muted-foreground">{[r.canSell && 'Sell', r.canProduce && 'Produce', r.canStock && 'Stock'].filter(Boolean).join(' · ') || '—'}</td>
                  <td className="px-4 py-2.5">{r.vatExempt ? 'Exempt' : 'Standard'}</td>
                  <td className="px-4 py-2.5">{r.isActive ? <span className="text-primary">Active</span> : <span className="text-muted-foreground">Inactive</span>}</td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr><td colSpan={7} className="px-4 py-10 text-center text-muted-foreground">No locations match this filter.</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>
      {!loading && <MasterDataPager pageNumber={pageNumber} pageSize={pageSize} totalCount={totalCount} totalPages={totalPages} setPageNumber={setPageNumber} setPageSize={setPageSize} noun="locations" />}
    </section>
  );
}

function Kpi({ label, value }: { label: string; value: string }) {
  return (
    <div className="card p-4">
      <div className="text-xs uppercase tracking-wide text-muted-foreground">{label}</div>
      <div className="mt-1 text-xl font-bold tabular-nums">{value}</div>
    </div>
  );
}

function SecondaryStat({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-border bg-surface p-3">
      <div className="text-xs uppercase tracking-wide text-muted-foreground">{label}</div>
      <div className="mt-1 text-lg font-semibold tabular-nums">{value}</div>
    </div>
  );
}

/** Pull a friendly {error} string out of an apiClient error like `API 400: {"error":"..."}`. */
function extractError(e: unknown, fallback: string): string {
  const msg = (e as Error)?.message ?? '';
  const jsonStart = msg.indexOf('{');
  if (jsonStart !== -1) {
    try {
      const parsed = JSON.parse(msg.slice(jsonStart));
      if (typeof parsed?.error === 'string') return parsed.error;
      if (typeof parsed?.message === 'string') return parsed.message;
    } catch {
      /* fall through */
    }
  }
  return msg || fallback;
}
