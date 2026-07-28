'use client';

import { use, useEffect, useState } from 'react';

type Info = {
  docType: string; docNumber: string; amount: number; docSummary: string; requestedByName: string | null;
  requestStatus: string; actionStatus: string; level: number; decidable: boolean; expired: boolean;
};

const DOC_LABEL: Record<string, string> = {
  purchase_order: 'Purchase Order', grn: 'Goods Receipt', stock_transfer: 'Stock Transfer',
  wastage_note: 'Wastage Note', request_note: 'Request Note', stock_adjustment: 'Stock Adjustment',
};

export default function ApprovePage({ params }: { params: Promise<{ token: string }> }) {
  const { token } = use(params);
  const [info, setInfo] = useState<Info | null>(null);
  const [err, setErr] = useState<string | null>(null);
  const [remark, setRemark] = useState('');
  const [busy, setBusy] = useState(false);
  const [done, setDone] = useState<string | null>(null);

  useEffect(() => {
    let alive = true;
    (async () => {
      try {
        const res = await fetch(`/api/v1/approvals/link/${token}`, { cache: 'no-store' });
        const j = await res.json().catch(() => ({}));
        if (!res.ok) throw new Error(j.error ?? 'This approval link is invalid or has expired.');
        if (alive) setInfo(j as Info);
      } catch (e) { if (alive) setErr((e as Error).message); }
    })();
    return () => { alive = false; };
  }, [token]);

  async function decide(action: 'approve' | 'reject' | 'hold') {
    if ((action === 'reject' || action === 'hold') && !remark.trim()) {
      setErr('Please add a note to reject or hold.'); return;
    }
    setBusy(true); setErr(null);
    try {
      const res = await fetch(`/api/v1/approvals/link/${token}`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ action, remark: remark.trim() || null }),
      });
      const j = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(j.error ?? 'Could not record your decision.');
      setDone(action);
    } catch (e) { setErr((e as Error).message); } finally { setBusy(false); }
  }

  const shell = (body: React.ReactNode) => (
    <div style={{ minHeight: '100vh', background: '#f1f5f9', display: 'flex', alignItems: 'flex-start', justifyContent: 'center', padding: '32px 12px', fontFamily: 'system-ui,-apple-system,Segoe UI,Roboto,sans-serif' }}>
      <div style={{ width: '100%', maxWidth: 520, background: '#fff', borderRadius: 16, border: '1px solid #e2e8f0', overflow: 'hidden' }}>
        <div style={{ background: '#15803d', color: '#fff', padding: '18px 28px', fontSize: 18, fontWeight: 800 }}>RIT&nbsp;HMS · Approval</div>
        <div style={{ padding: 28, color: '#0f172a' }}>{body}</div>
      </div>
    </div>
  );

  if (err && !info) return shell(<p style={{ color: '#b91c1c', fontSize: 15 }}>{err}</p>);
  if (!info) return shell(<p style={{ color: '#64748b' }}>Loading…</p>);

  if (done) {
    const map = { approve: ['Approved ✓', '#15803d'], reject: ['Rejected', '#b91c1c'], hold: ['Put on hold', '#b45309'] } as const;
    const [label, color] = map[done as keyof typeof map];
    return shell(<div style={{ textAlign: 'center', padding: '12px 0' }}>
      <div style={{ fontSize: 26, fontWeight: 800, color }}>{label}</div>
      <p style={{ color: '#64748b', marginTop: 8 }}>Your decision on {DOC_LABEL[info.docType] ?? 'the document'} <b>{info.docNumber}</b> has been recorded. Thank you.</p>
    </div>);
  }

  if (!info.decidable) return shell(<div>
    <h2 style={{ margin: '0 0 6px', fontSize: 20 }}>{DOC_LABEL[info.docType] ?? 'Document'} {info.docNumber}</h2>
    <p style={{ color: '#64748b' }}>This request is <b>{info.requestStatus}</b>{info.expired ? ' (link expired)' : info.actionStatus !== 'pending' ? ` — you already responded (${info.actionStatus})` : ''}. No action is needed.</p>
  </div>);

  return shell(<div>
    <h2 style={{ margin: '0 0 4px', fontSize: 20, fontWeight: 800 }}>Approval needed</h2>
    <p style={{ color: '#64748b', margin: '0 0 16px' }}>{DOC_LABEL[info.docType] ?? 'Document'} · Level {info.level}</p>
    <div style={{ background: '#f8fafc', borderRadius: 10, padding: '12px 16px', marginBottom: 18, fontSize: 15 }}>
      <div><span style={{ color: '#64748b' }}>Reference</span> &nbsp; <b>{info.docNumber}</b></div>
      <div style={{ marginTop: 4 }}><span style={{ color: '#64748b' }}>Amount</span> &nbsp; <b>{info.amount.toLocaleString(undefined, { minimumFractionDigits: 2 })}</b></div>
      {info.requestedByName && <div style={{ marginTop: 4 }}><span style={{ color: '#64748b' }}>Requested by</span> &nbsp; {info.requestedByName}</div>}
    </div>
    <label style={{ fontSize: 13, fontWeight: 600, color: '#334155' }}>Note <span style={{ color: '#94a3b8', fontWeight: 400 }}>(required to reject or hold)</span></label>
    <textarea value={remark} onChange={e => setRemark(e.target.value)} rows={3} placeholder="Add a note…"
      style={{ width: '100%', marginTop: 6, marginBottom: 14, borderRadius: 10, border: '1px solid #cbd5e1', padding: 10, fontSize: 14, fontFamily: 'inherit', boxSizing: 'border-box' }} />
    {err && <p style={{ color: '#b91c1c', fontSize: 13, margin: '0 0 12px' }}>{err}</p>}
    <div style={{ display: 'flex', gap: 10 }}>
      <button disabled={busy} onClick={() => decide('approve')} style={{ flex: 1, padding: '12px', borderRadius: 10, border: 'none', background: '#15803d', color: '#fff', fontWeight: 700, fontSize: 15, cursor: 'pointer' }}>Approve</button>
      <button disabled={busy} onClick={() => decide('hold')} style={{ padding: '12px 16px', borderRadius: 10, border: '1px solid #cbd5e1', background: '#fff', color: '#b45309', fontWeight: 700, fontSize: 15, cursor: 'pointer' }}>Hold</button>
      <button disabled={busy} onClick={() => decide('reject')} style={{ padding: '12px 16px', borderRadius: 10, border: '1px solid #cbd5e1', background: '#fff', color: '#b91c1c', fontWeight: 700, fontSize: 15, cursor: 'pointer' }}>Reject</button>
    </div>
  </div>);
}
