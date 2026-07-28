'use client';

import { useEffect, useMemo, useRef, useState } from 'react';
import { apiClient } from '@/lib/api-client';
import { Download, Upload, FileSpreadsheet, CheckCircle2, AlertTriangle } from 'lucide-react';

// ── master-data import hub (#master-data-import) ──────────────────────────────
// One screen, four datasets. Each declares its CSV columns; the parser maps CSV
// header names to JSON fields (column order doesn't matter), converts numbers,
// previews the rows, then POSTs to the matching bulk-import endpoint. The server
// upserts and returns a per-row error report so good rows land and bad rows show.

type Col = { header: string; field: string; type?: 'number'; hint?: string };
type Dataset = {
  key: string;
  label: string;
  endpoint: string;
  /** opening stock wraps rows as { locationId, lines } and needs a location picked */
  wrap?: 'opening-stock';
  cols: Col[];
  sample: string[][];
  blurb: string;
};

const DATASETS: Dataset[] = [
  {
    key: 'products', label: 'Products', endpoint: '/api/v1/products/import',
    blurb: 'Upserts by Product Code. New categories & units are created automatically by name. Type = "raw" for ingredients, otherwise sold.',
    cols: [
      { header: 'Product Code', field: 'sku' },
      { header: 'Name', field: 'name' },
      { header: 'Barcode', field: 'barcode' },
      { header: 'Category', field: 'category' },
      { header: 'Unit', field: 'unit' },
      { header: 'Sell Price', field: 'sellPrice', type: 'number' },
      { header: 'Cost', field: 'cost', type: 'number' },
      { header: 'Type', field: 'type', hint: 'raw / finished' },
      { header: 'Station', field: 'station', hint: 'kitchen / bar …' },
      { header: 'Tax Class', field: 'taxClass', hint: 'standard / exempt / zero' },
    ],
    sample: [
      ['FOOD-001', 'Chicken Kottu', '', 'Mains', 'Plate', '1450', '620', 'finished', 'Hot Kitchen', 'standard'],
      ['ING-RICE', 'Basmati Rice', '', 'Dry Goods', 'kg', '', '320', 'raw', '', 'standard'],
    ],
  },
  {
    key: 'suppliers', label: 'Suppliers', endpoint: '/api/v1/suppliers/import',
    blurb: 'Upserts by Code (or by Name when Code is blank). A code is generated from your supplier prefix when left empty.',
    cols: [
      { header: 'Code', field: 'code', hint: 'blank = auto' },
      { header: 'Name', field: 'name' },
      { header: 'Contact Name', field: 'contactName' },
      { header: 'Phone', field: 'phone' },
      { header: 'Email', field: 'email' },
      { header: 'Address', field: 'address' },
      { header: 'Country', field: 'countryCode', hint: 'LK / AE …' },
      { header: 'Payment Terms (days)', field: 'paymentTermsDays', type: 'number' },
      { header: 'VAT No', field: 'vatNo' },
    ],
    sample: [
      ['', 'Fresh Farms Lanka', 'Nimal P.', '0771234567', 'sales@freshfarms.lk', 'Negombo Rd', 'LK', '30', ''],
    ],
  },
  {
    key: 'customers', label: 'Customers', endpoint: '/api/v1/customers/import',
    blurb: 'Upserts by Code, then Phone, then Name. Categories are created by name. A credit limit > 0 marks the customer as a credit (AR) account.',
    cols: [
      { header: 'Code', field: 'code', hint: 'blank = auto' },
      { header: 'Name', field: 'name' },
      { header: 'Phone', field: 'phone' },
      { header: 'Email', field: 'email' },
      { header: 'Address', field: 'address' },
      { header: 'Tax/NIC No', field: 'taxNo' },
      { header: 'Category', field: 'category' },
      { header: 'Discount %', field: 'discountPercent', type: 'number' },
      { header: 'Credit Limit', field: 'creditLimit', type: 'number' },
      { header: 'Loyalty Card No', field: 'loyaltyCardNo' },
      { header: 'Country', field: 'countryCode' },
    ],
    sample: [
      ['', 'Acme Corporate', '0112555666', 'ap@acme.lk', 'Colombo 03', '123456789V', 'Corporate', '10', '500000', '', 'LK'],
    ],
  },
  {
    key: 'opening-stock', label: 'Opening Stock', endpoint: '/api/v1/inventory/opening-stock/import', wrap: 'opening-stock',
    blurb: 'SETS on-hand for each product at the chosen location (re-running the same file is safe). Cost is optional and stamps the opening average cost.',
    cols: [
      { header: 'Product Code', field: 'sku' },
      { header: 'Quantity', field: 'quantity', type: 'number' },
      { header: 'Cost', field: 'cost', type: 'number', hint: 'optional' },
    ],
    sample: [
      ['ING-RICE', '120', '320'],
      ['FOOD-001', '0', ''],
    ],
  },
];

type Outlet = { id: string; name: string; canStock?: boolean };

// Minimal RFC-4180-ish parser: handles quoted fields, embedded commas, "" escapes, CRLF.
function parseCsv(text: string): string[][] {
  const rows: string[][] = [];
  let row: string[] = [], cell = '', inQ = false;
  for (let i = 0; i < text.length; i++) {
    const c = text[i];
    if (inQ) {
      if (c === '"') { if (text[i + 1] === '"') { cell += '"'; i++; } else inQ = false; }
      else cell += c;
    } else if (c === '"') inQ = true;
    else if (c === ',') { row.push(cell); cell = ''; }
    else if (c === '\n' || c === '\r') {
      if (c === '\r' && text[i + 1] === '\n') i++;
      row.push(cell); cell = '';
      if (row.some(v => v.trim() !== '')) rows.push(row);
      row = [];
    } else cell += c;
  }
  if (cell !== '' || row.length) { row.push(cell); if (row.some(v => v.trim() !== '')) rows.push(row); }
  return rows;
}

function csvCell(v: string): string {
  return /[",\n\r]/.test(v) ? `"${v.replace(/"/g, '""')}"` : v;
}

export default function DataImportPage() {
  const [activeKey, setActiveKey] = useState(DATASETS[0].key);
  const ds = useMemo(() => DATASETS.find(d => d.key === activeKey)!, [activeKey]);
  const [grid, setGrid] = useState<string[][]>([]);       // raw parsed rows incl. header
  const [fileName, setFileName] = useState('');
  const [locationId, setLocationId] = useState('');
  const [outlets, setOutlets] = useState<Outlet[]>([]);
  const [busy, setBusy] = useState(false);
  const [result, setResult] = useState<{ created?: number; updated?: number; applied?: number; adjustment?: string | null; errors: { row: number; sku?: string; name?: string; error: string }[] } | null>(null);
  const [toast, setToast] = useState<string | null>(null);
  const fileRef = useRef<HTMLInputElement>(null);
  const flash = (m: string) => { setToast(m); window.setTimeout(() => setToast(null), 3500); };

  useEffect(() => {
    apiClient<Outlet[]>('/api/v1/locations?all=true')
      .then(ls => { const s = ls.filter(o => o.canStock !== false); setOutlets(s); if (s[0]) setLocationId(p => p || s[0].id); })
      .catch(() => {});
  }, []);

  // Switching dataset clears the staged file (columns differ).
  useEffect(() => { setGrid([]); setFileName(''); setResult(null); }, [activeKey]);

  function downloadTemplate() {
    const lines = [ds.cols.map(c => csvCell(c.header)).join(','), ...ds.sample.map(r => r.map(csvCell).join(','))];
    const blob = new Blob([lines.join('\r\n')], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = `${ds.key}-template.csv`; a.click();
    URL.revokeObjectURL(url);
  }

  function ingest(text: string, name: string) {
    const parsed = parseCsv(text);
    if (parsed.length < 1) { flash('That file looks empty.'); return; }
    setGrid(parsed); setFileName(name); setResult(null);
  }

  function onFile(e: React.ChangeEvent<HTMLInputElement>) {
    const f = e.target.files?.[0]; if (!f) return;
    const reader = new FileReader();
    reader.onload = () => ingest(String(reader.result || ''), f.name);
    reader.readAsText(f);
    e.target.value = '';
  }

  // Map CSV header names → field, build typed row objects from the data rows.
  const { objects, headerMap, dataRows } = useMemo(() => {
    if (grid.length < 2) return { objects: [] as Record<string, unknown>[], headerMap: {} as Record<number, Col | undefined>, dataRows: [] as string[][] };
    const headers = grid[0].map(h => h.trim().toLowerCase());
    const hm: Record<number, Col | undefined> = {};
    headers.forEach((h, idx) => { hm[idx] = ds.cols.find(c => c.header.toLowerCase() === h); });
    const rows = grid.slice(1);
    const objs = rows.map(r => {
      const o: Record<string, unknown> = {};
      r.forEach((val, idx) => {
        const col = hm[idx]; if (!col) return;
        const v = (val ?? '').trim();
        if (v === '') return;
        if (col.type === 'number') { const n = Number(v.replace(/,/g, '')); if (!Number.isNaN(n)) o[col.field] = n; }
        else o[col.field] = v;
      });
      return o;
    });
    return { objects: objs, headerMap: hm, dataRows: rows };
  }, [grid, ds]);

  const unmatched = useMemo(() => {
    if (grid.length < 1) return [];
    return grid[0].map(h => h.trim()).filter((h, idx) => h !== '' && !headerMap[idx]);
  }, [grid, headerMap]);

  async function runImport() {
    if (objects.length === 0) { flash('Nothing to import — add a file with data rows.'); return; }
    if (ds.wrap === 'opening-stock' && !locationId) { flash('Pick a location for opening stock.'); return; }
    setBusy(true); setResult(null);
    try {
      const body = ds.wrap === 'opening-stock'
        ? JSON.stringify({ locationId, lines: objects })
        : JSON.stringify(objects);
      const res = await apiClient<typeof result>(ds.endpoint, { method: 'POST', body: body as string });
      setResult(res);
      const ok = (res?.created ?? 0) + (res?.updated ?? 0) + (res?.applied ?? 0);
      flash(`${ok} row(s) imported${res?.errors?.length ? `, ${res.errors.length} skipped` : ''}.`);
    } catch (e) {
      flash(e instanceof Error ? e.message : 'Import failed.');
    } finally { setBusy(false); }
  }

  return (
    <div className="mx-auto max-w-5xl p-4 sm:p-6">
      <header className="mb-5">
        <h1 className="text-xl font-semibold text-fg flex items-center gap-2"><FileSpreadsheet className="size-5 text-primary" /> Data Import</h1>
        <p className="mt-1 text-sm text-muted">Bring your master data in from a spreadsheet. Download a template, fill it, upload — we upsert and report any rows that need attention.</p>
      </header>

      {/* dataset tabs */}
      <div className="mb-5 flex flex-wrap gap-2">
        {DATASETS.map(d => (
          <button key={d.key} onClick={() => setActiveKey(d.key)}
            className={`rounded-lg px-3 py-1.5 text-sm font-medium border ${activeKey === d.key ? 'bg-primary text-white border-primary' : 'bg-surface text-fg border-border hover:bg-bg'}`}>
            {d.label}
          </button>
        ))}
      </div>

      <div className="rounded-xl border border-border bg-surface p-4 sm:p-5">
        <p className="text-sm text-muted">{ds.blurb}</p>

        {/* actions */}
        <div className="mt-4 flex flex-wrap items-center gap-3">
          <button onClick={downloadTemplate} className="inline-flex items-center gap-2 rounded-lg border border-border bg-bg px-3 py-2 text-sm font-medium text-fg hover:bg-surface">
            <Download className="size-4" /> Download template
          </button>
          <button onClick={() => fileRef.current?.click()} className="inline-flex items-center gap-2 rounded-lg border border-border bg-bg px-3 py-2 text-sm font-medium text-fg hover:bg-surface">
            <Upload className="size-4" /> Choose CSV
          </button>
          <input ref={fileRef} type="file" accept=".csv,text/csv" onChange={onFile} className="hidden" />
          {fileName && <span className="text-sm text-muted truncate">{fileName} — {dataRows.length} row(s)</span>}

          {ds.wrap === 'opening-stock' && (
            <label className="ml-auto flex items-center gap-2 text-sm text-fg">
              Location
              <select value={locationId} onChange={e => setLocationId(e.target.value)} className="rounded-lg border border-border bg-surface px-2.5 py-1.5 text-sm">
                {outlets.map(o => <option key={o.id} value={o.id}>{o.name}</option>)}
              </select>
            </label>
          )}
        </div>

        {/* unmatched-header warning */}
        {unmatched.length > 0 && (
          <div className="mt-3 rounded-lg border border-amber-300 bg-amber-50 px-3 py-2 text-sm text-amber-800">
            Ignored unknown column(s): {unmatched.join(', ')}. Match these headers: {ds.cols.map(c => c.header).join(', ')}.
          </div>
        )}

        {/* preview */}
        {dataRows.length > 0 && (
          <div className="mt-4 overflow-x-auto rounded-lg border border-border">
            <table className="w-full text-sm">
              <thead className="bg-bg text-left text-muted">
                <tr>{ds.cols.map(c => <th key={c.field} className="px-3 py-2 font-medium whitespace-nowrap">{c.header}{c.hint && <span className="ml-1 text-[10px] text-muted/70">({c.hint})</span>}</th>)}</tr>
              </thead>
              <tbody>
                {objects.slice(0, 50).map((o, i) => (
                  <tr key={i} className="border-t border-border">
                    {ds.cols.map(c => <td key={c.field} className="px-3 py-1.5 whitespace-nowrap text-fg">{o[c.field] != null ? String(o[c.field]) : <span className="text-muted/50">—</span>}</td>)}
                  </tr>
                ))}
              </tbody>
            </table>
            {objects.length > 50 && <div className="px-3 py-2 text-xs text-muted">Showing first 50 of {objects.length} rows.</div>}
          </div>
        )}

        {/* import */}
        {dataRows.length > 0 && (
          <div className="mt-4 flex justify-end">
            <button onClick={runImport} disabled={busy}
              className="inline-flex items-center gap-2 rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-white hover:opacity-90 disabled:opacity-50">
              {busy ? 'Importing…' : `Import ${objects.length} ${ds.label}`}
            </button>
          </div>
        )}

        {/* result */}
        {result && (
          <div className="mt-4 space-y-3">
            <div className="flex flex-wrap items-center gap-4 rounded-lg border border-border bg-bg px-4 py-3 text-sm">
              <CheckCircle2 className="size-5 text-emerald-600" />
              {result.created != null && <span><b className="text-fg">{result.created}</b> created</span>}
              {result.updated != null && <span><b className="text-fg">{result.updated}</b> updated</span>}
              {result.applied != null && <span><b className="text-fg">{result.applied}</b> applied</span>}
              {result.adjustment && <span className="text-muted">Adjustment {result.adjustment}</span>}
              {result.errors?.length ? <span className="text-amber-700">{result.errors.length} skipped</span> : <span className="text-emerald-700">no errors</span>}
            </div>
            {result.errors?.length > 0 && (
              <div className="overflow-x-auto rounded-lg border border-amber-300">
                <table className="w-full text-sm">
                  <thead className="bg-amber-50 text-left text-amber-800"><tr><th className="px-3 py-2 font-medium">Row</th><th className="px-3 py-2 font-medium">Reference</th><th className="px-3 py-2 font-medium">Problem</th></tr></thead>
                  <tbody>
                    {result.errors.map((e, i) => (
                      <tr key={i} className="border-t border-amber-200">
                        <td className="px-3 py-1.5 text-fg">{e.row}</td>
                        <td className="px-3 py-1.5 text-fg">{e.sku || e.name || '—'}</td>
                        <td className="px-3 py-1.5 text-amber-800 flex items-center gap-1.5"><AlertTriangle className="size-3.5" /> {e.error}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}
      </div>

      {toast && <div className="fixed bottom-5 left-1/2 -translate-x-1/2 rounded-lg bg-fg px-4 py-2 text-sm text-bg shadow-lg">{toast}</div>}
    </div>
  );
}
