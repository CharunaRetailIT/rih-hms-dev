'use client';

import { useCallback, useEffect, useState } from 'react';
import { Topbar } from '@/components/app-shell/Topbar';
import { apiClient, lkr, money } from '@/lib/api-client';
import { Download, Search, Receipt } from 'lucide-react';
import { Modal } from '@/components/ui/Modal';
import { Combobox, Field } from '@/components/ui/form';
import { HeaderStat, Num } from '@/components/ui/HeaderStat';
import { Pagination } from '@/components/ui/Pagination';

type Row = {
  id: string; number: string; orderNumber: string; invoiceNumber: string | null;
  txnAt: string; status: string; orderType: string; orderSource: string;
  location: string | null; customer: string | null; cashier: string | null; steward: string | null;
  covers: number | null; totalAmount: number; tipAmount: number; voidReason: string | null;
  tenderMethods: string[];
};
type PaginationMeta = { totalCount: number; pageNumber: number; pageSize: number; totalPages: number };
type ListResp = { from: string; to: string; count: number; total: number; rows: Row[]; pagination: PaginationMeta };
type Detail = {
  id: string; number: string; orderNumber: string; invoiceNumber: string | null; isTaxInvoice: boolean;
  status: string; orderType: string; orderSource: string; tableLabel: string | null; covers: number | null;
  txnAt: string; voidReason: string | null; customer: string | null; customerVatNo: string | null;
  cashier: string | null; steward: string | null;
  subtotalAmount: number; discountAmount: number; promotionDiscountAmount: number; serviceChargeAmount: number;
  taxAmount: number; tipAmount: number; totalAmount: number; tourCommissionAmount: number;
  items: { productName: string; variantName: string | null; quantity: number; unitPrice: number; lineTotal: number; station: string; modifiers: { name: string; priceDelta: number }[] }[];
  charges: { name: string; ratePercent: number; baseAmount: number; chargeAmount: number }[];
  payments: { payType: string; amount: number; currencyCode: string | null; fxRate: number; baseAmount: number; reference: string | null; createdAt: string }[];
};
const today = () => new Date().toISOString().slice(0, 10);
const PAY_LABEL: Record<string, string> = {
  cash: 'Cash', card: 'Card', credit: 'Credit', loyalty: 'Points', advance: 'Advance',
  ubereats_prepaid: 'Uber Eats', pickme_prepaid: 'PickMe',
};
const payLabel = (p: string) => PAY_LABEL[p] ?? p;
const STATUSES = ['', 'settled', 'void', 'open', 'confirmed'];
// Always-listed single tenders (so credit/loyalty are findable even with no data yet).
const BASE_TENDERS = ['cash', 'card', 'credit', 'loyalty', 'advance', 'ubereats_prepaid', 'pickme_prepaid'];

function statusPill(s: string) {
  if (s === 'settled') return 'pill-paid';
  if (s === 'void') return 'pill-void';
  return 'pill-idle';
}
function fmt(ts: string) { return new Date(ts).toLocaleString('en-LK', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' }); }

export default function TransactionsPage() {
  const [from, setFrom] = useState(today());
  const [to, setTo] = useState(today());
  const [status, setStatus] = useState('');
  const [payFilter, setPayFilter] = useState('');   // '', 'split', 'single:cash', 'combo:cash+card'
  const [search, setSearch] = useState('');
  const [data, setData] = useState<ListResp | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [detail, setDetail] = useState<Detail | null>(null);
  const [detailBusy, setDetailBusy] = useState(false);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);

  const load = useCallback(async () => {
    setLoading(true); setError(null);
    try {
      const qs = new URLSearchParams();
      qs.set('from', from); qs.set('to', new Date(new Date(to).getTime() + 86400000).toISOString().slice(0, 10));
      if (status) qs.set('status', status);
      if (search.trim()) qs.set('search', search.trim());
      qs.set('pageNumber', String(pageNumber)); qs.set('pageSize', String(pageSize));
      // A single-tender filter (single:cash etc) is applied server-side via payType;
      // "split" / "combo:x+y" only inspect the current page (they need the full tender
      // breakdown per bill, which only the loaded page has).
      if (payFilter.startsWith('single:')) qs.set('payType', payFilter.slice(7));
      setData(await apiClient<ListResp>(`/api/v1/transactions?${qs.toString()}`));
    } catch (e) { setError((e as Error).message); } finally { setLoading(false); }
  }, [from, to, status, search, payFilter, pageNumber, pageSize]);

  useEffect(() => { void load(); }, [pageNumber, pageSize]);   // includes the initial load (today)
  useEffect(() => { setPageNumber(1); void load(); }, [payFilter]);   // eslint-disable-line react-hooks/exhaustive-deps

  async function openDetail(id: string) {
    setDetailBusy(true);
    try { setDetail(await apiClient<Detail>(`/api/v1/transactions/${id}`)); }
    catch (e) { setError((e as Error).message); } finally { setDetailBusy(false); }
  }

  // ── Payment filter (client-side so it can match split combinations) ──
  const allRows = data?.rows ?? [];
  const comboKey = (r: Row) => [...r.tenderMethods].sort().join('+');
  const presentSingles = new Set(allRows.flatMap(r => r.tenderMethods));
  // Always list the standard tenders (so credit/loyalty are selectable even with
  // none in range), plus any non-standard tender that appears in the data.
  const singleOpts = BASE_TENDERS.concat([...presentSingles].filter(t => !BASE_TENDERS.includes(t)));
  const comboOpts = Array.from(new Set(allRows.filter(r => r.tenderMethods.length >= 2).map(comboKey))).sort();
  const payFilterOpts = [
    { value: '', label: 'All payments' },
    { value: 'split', label: 'Split payment (2+ tenders)' },
    ...singleOpts.map(t => ({ value: `single:${t}`, label: payLabel(t) })),
    ...comboOpts.map(c => ({ value: `combo:${c}`, label: c.split('+').map(payLabel).join(' + ') })),
  ];
  const statusOpts = STATUSES.map(s => ({ value: s, label: s === '' ? 'All' : s }));
  const filteredRows = allRows.filter(r => {
    if (!payFilter) return true;
    if (payFilter === 'split') return r.tenderMethods.length >= 2;
    if (payFilter.startsWith('single:')) return r.tenderMethods.includes(payFilter.slice(7));
    if (payFilter.startsWith('combo:')) return comboKey(r) === payFilter.slice(6);
    return true;
  });
  const shownSettledTotal = filteredRows.filter(r => r.status === 'settled').reduce((s, r) => s + r.totalAmount, 0);

  function exportCsv() {
    if (!data) return;
    const head = ['Date', 'Number', 'Status', 'Type', 'Source', 'Location', 'Customer', 'Cashier', 'Steward', 'Tenders', 'Total'];
    const lines = filteredRows.map(r => [
      new Date(r.txnAt).toISOString(), r.number, r.status, r.orderType, r.orderSource,
      r.location ?? '', r.customer ?? '', r.cashier ?? '', r.steward ?? '',
      r.tenderMethods.map(payLabel).join('+'), r.totalAmount.toFixed(2),
    ].map(c => `"${String(c).replace(/"/g, '""')}"`).join(','));
    const blob = new Blob([[head.join(','), ...lines].join('\n')], { type: 'text/csv' });
    const a = document.createElement('a'); a.href = URL.createObjectURL(blob);
    a.download = `transactions_${from}_${to}.csv`; a.click(); URL.revokeObjectURL(a.href);
  }

  return (
    <>
      <Topbar title="Transactions" subtitle="Look back at every sale, with the full payment story" />
      <div className="p-6 md:p-8">
        {/* Filters */}
        <div className="card mb-4 flex flex-wrap items-end gap-3 p-4">
          <Field label="From" type="date" value={from} onChange={setFrom} className="w-40" />
          <Field label="To" type="date" value={to} onChange={setTo} className="w-40" />
          <Combobox label="Status" value={status} onChange={setStatus} options={statusOpts} className="w-40" />
          <Combobox label="Payment" value={payFilter} onChange={setPayFilter} options={payFilterOpts} className="w-52" searchPlaceholder="Filter tenders…" />
          <label className="flex-1 text-sm">Search<div className="relative mt-1">
            <Search className="absolute left-2 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
            <input value={search} onChange={e => setSearch(e.target.value)} onKeyDown={e => { if (e.key === 'Enter') { setPageNumber(1); void load(); } }}
              placeholder="bill / invoice / customer" className="w-full rounded-lg border border-border bg-surface py-1.5 pl-8 pr-2 text-sm" /></div></label>
          <button onClick={() => { setPageNumber(1); void load(); }} className="rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground hover:bg-primary-dark">Apply</button>
          <button onClick={exportCsv} disabled={!data?.rows.length} className="flex items-center gap-1.5 rounded-lg border border-border px-3 py-2 text-sm font-semibold hover:bg-muted disabled:opacity-50"><Download className="size-4" /> CSV</button>
        </div>

        {data && (
          <HeaderStat className="mb-3"><Num>{data.pagination.totalCount}</Num> transaction(s) in range · page settled total <Num>{money(shownSettledTotal)}</Num></HeaderStat>
        )}

        <div className="card overflow-hidden">
          {loading ? (
            <div className="space-y-2 p-4">{Array.from({ length: 8 }).map((_, i) => <div key={i} className="h-9 animate-pulse rounded bg-muted" />)}</div>
          ) : error ? <div className="p-6 text-sm text-status-error">{error}</div> : (
            <table className="w-full text-sm">
              <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th className="px-3 py-3 font-medium">Date</th>
                  <th className="px-3 py-3 font-medium">Number</th>
                  <th className="px-3 py-3 font-medium">Type</th>
                  <th className="px-3 py-3 font-medium">Status</th>
                  <th className="px-3 py-3 font-medium">Customer</th>
                  <th className="px-3 py-3 font-medium">Cashier</th>
                  <th className="px-3 py-3 font-medium">Tenders</th>
                  <th className="px-3 py-3 text-right font-medium">Total</th>
                </tr>
              </thead>
              <tbody>
                {filteredRows.map((r, i) => (
                  <tr key={r.id} onClick={() => openDetail(r.id)} className={`cursor-pointer hover:bg-primary-tint/40 ${i % 2 ? 'bg-muted/20' : ''}`}>
                    <td className="px-3 py-2.5 whitespace-nowrap text-muted-foreground">{fmt(r.txnAt)}</td>
                    <td className="px-3 py-2.5 font-semibold">{r.number}{r.location ? <span className="ml-1 text-xs text-muted-foreground">{r.location}</span> : ''}</td>
                    <td className="px-3 py-2.5 capitalize">{r.orderType.replace('_', ' ')}</td>
                    <td className="px-3 py-2.5"><span className={`pill ${statusPill(r.status)}`}>{r.status}</span></td>
                    <td className="px-3 py-2.5">{r.customer ?? <span className="text-muted-foreground">—</span>}</td>
                    <td className="px-3 py-2.5 text-muted-foreground">{r.cashier ?? '—'}</td>
                    <td className="px-3 py-2.5">{r.tenderMethods.length ? r.tenderMethods.map(m => <span key={m} className="mr-1 inline-block rounded bg-muted px-1.5 py-0.5 text-xs">{payLabel(m)}</span>) : <span className="text-muted-foreground">—</span>}</td>
                    <td className="px-3 py-2.5 text-right font-bold tabular-nums">{money(r.totalAmount)}</td>
                  </tr>
                ))}
                {filteredRows.length === 0 && <tr><td colSpan={8} className="px-4 py-10 text-center text-muted-foreground">No transactions{payFilter ? ' for that payment filter' : ' in this range'}.</td></tr>}
              </tbody>
            </table>
          )}
        </div>

        {data && (
          <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
            <select
              value={pageSize}
              onChange={e => { setPageSize(Number(e.target.value)); setPageNumber(1); }}
              className="rounded-lg border border-border bg-surface px-2 py-1.5 text-xs"
            >
              {[25, 50, 100].map(n => <option key={n} value={n}>{n} / page</option>)}
            </select>
            <Pagination
              page={data.pagination.pageNumber}
              totalPages={data.pagination.totalPages}
              total={data.pagination.totalCount}
              from={data.pagination.totalCount === 0 ? 0 : (data.pagination.pageNumber - 1) * data.pagination.pageSize + 1}
              to={Math.min(data.pagination.pageNumber * data.pagination.pageSize, data.pagination.totalCount)}
              setPage={setPageNumber}
              noun="transactions"
              className="mt-0 flex-1"
            />
          </div>
        )}
      </div>

      {/* Drill-down */}
      {(detail || detailBusy) && (
        <Modal title={detail?.number ?? 'Transaction'} icon={<Receipt className="size-4" />} onClose={() => setDetail(null)} size="2xl">
          {detailBusy && !detail ? <div className="p-10 text-center text-muted-foreground">Loading…</div> : detail && (
            <>
              <div className="mb-4">
                <p className="flex items-center gap-2 text-base font-bold">{detail.number}<span className={`pill ${statusPill(detail.status)} align-middle`}>{detail.status}</span></p>
                <p className="text-sm text-muted-foreground">{fmt(detail.txnAt)} · {detail.orderType.replace('_', ' ')} · {detail.orderSource}{detail.tableLabel ? ` · ${detail.tableLabel}` : ''}{detail.covers ? ` · ${detail.covers} covers` : ''}</p>
                <p className="mt-0.5 text-xs text-muted-foreground">
                  {detail.customer ? `Customer: ${detail.customer}` : ''}{detail.cashier ? ` · Cashier: ${detail.cashier}` : ''}{detail.steward ? ` · Steward: ${detail.steward}` : ''}
                </p>
                {detail.status === 'void' && detail.voidReason && <p className="mt-1 text-xs font-semibold text-status-error">Void: {detail.voidReason}</p>}
              </div>
              <div>
                  <table className="w-full text-sm">
                    <thead className="text-left text-xs uppercase text-muted-foreground"><tr><th className="py-1">Item</th><th className="py-1 text-center">Qty</th><th className="py-1 text-right">Price</th><th className="py-1 text-right">Total</th></tr></thead>
                    <tbody>
                      {detail.items.map((it, i) => (
                        <tr key={i} className="border-t border-border/50">
                          <td className="py-1.5">{it.productName}{it.variantName ? ` (${it.variantName})` : ''}{it.modifiers.length ? <span className="block text-xs text-muted-foreground">{it.modifiers.map(m => m.name).join(', ')}</span> : ''}</td>
                          <td className="py-1.5 text-center tabular-nums">{it.quantity}</td>
                          <td className="py-1.5 text-right tabular-nums">{money(it.unitPrice)}</td>
                          <td className="py-1.5 text-right tabular-nums">{money(it.lineTotal)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                  <div className="mt-4 ml-auto w-full max-w-xs space-y-1 text-sm">
                    <Row2 label="Subtotal" v={detail.subtotalAmount} />
                    {detail.discountAmount > 0 && <Row2 label="Discount" v={-detail.discountAmount} />}
                    {detail.promotionDiscountAmount > 0 && <Row2 label="Promotions" v={-detail.promotionDiscountAmount} />}
                    {detail.serviceChargeAmount > 0 && <Row2 label="Service charge" v={detail.serviceChargeAmount} />}
                    {detail.charges.filter(c => c.chargeAmount > 0).map((c, i) => <Row2 key={i} label={`${c.name} (${c.ratePercent}%)`} v={c.chargeAmount} muted />)}
                    {detail.tipAmount > 0 && <Row2 label="Tip" v={detail.tipAmount} />}
                    <div className="flex justify-between border-t border-border pt-1 text-base font-black"><span>Total</span><span className="tabular-nums">{money(detail.totalAmount)}</span></div>
                  </div>
                  {detail.payments.length > 0 && (
                    <div className="mt-4">
                      <h4 className="mb-1 text-xs font-bold uppercase text-muted-foreground">Tenders</h4>
                      {detail.payments.map((p, i) => (
                        <div key={i} className="flex justify-between border-t border-border/50 py-1 text-sm">
                          <span>{payLabel(p.payType)}{p.currencyCode && p.currencyCode !== 'LKR' ? ` · ${p.currencyCode} ${lkr(p.amount)} @ ${p.fxRate}` : ''}{p.reference ? ` · ${p.reference}` : ''}</span>
                          <span className="tabular-nums">{money(p.baseAmount || p.amount)}</span>
                        </div>
                      ))}
                    </div>
                  )}
                  {detail.tourCommissionAmount > 0 && <p className="mt-3 text-xs text-muted-foreground">Tour-operator commission booked: {money(detail.tourCommissionAmount)}</p>}
                </div>
              </>
            )}
        </Modal>
      )}
    </>
  );
}

function Row2({ label, v, muted }: { label: string; v: number; muted?: boolean }) {
  return <div className={`flex justify-between ${muted ? 'text-muted-foreground' : ''}`}><span>{label}</span><span className="tabular-nums">LKR {(v || 0).toLocaleString('en-LK', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</span></div>;
}
