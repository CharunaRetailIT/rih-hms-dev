'use client';

import { useEffect, useState } from 'react';
import { apiClient } from '@/lib/api-client';

/**
 * Uber Eats / PickMe credentials + per-outlet store mapping. Lives on the main
 * Settings page (moved out of the Delivery screen, which stays operational —
 * incoming orders, 86 menu, PickMe sync, outbox). Secrets are write-only:
 * stored encrypted server-side, never returned; leave a field blank to keep it.
 */

type OrderSource = 'ubereats' | 'pickme';

type Store = {
  locationId: string;
  code: string;
  name: string;
  externalStoreId: string | null;
  isEnabled: boolean;
  hasApiKey: boolean;            // PickMe per-outlet X-API-KEY is set (never returned)
  lastPolledAt: string | null;
};

type Credential = {
  aggregator: OrderSource;
  isEnabled: boolean;
  environment: string;
  clientId: string | null;
  hasClientSecret: boolean;
  clientSecretHint: string | null;
  hasWebhookSecret: boolean;
  baseUrl: string | null;
  stores: Store[];
};

const SOURCE_LABEL: Record<OrderSource, string> = { ubereats: 'UBER EATS', pickme: 'PICKME' };
const SOURCE_PILL: Record<OrderSource, string> = { ubereats: 'pill-progress', pickme: 'pill-pending' };

function SourceBadge({ source }: { source: OrderSource }) {
  return <span className={`pill ${SOURCE_PILL[source]} font-bold uppercase tracking-wide`}>{SOURCE_LABEL[source]}</span>;
}

function fmtDate(iso: string | null): string {
  if (!iso) return '—';
  return new Date(iso).toLocaleString('en-LK', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
}

function extractError(e: unknown, fallback: string): string {
  const msg = (e as Error)?.message ?? '';
  const jsonStart = msg.indexOf('{');
  if (jsonStart !== -1) {
    try {
      const parsed = JSON.parse(msg.slice(jsonStart));
      if (typeof parsed?.error === 'string') return parsed.error;
      if (typeof parsed?.message === 'string') return parsed.message;
    } catch { /* fall through */ }
  }
  return msg || fallback;
}

export function AggregatorSettings({ flash }: { flash: (msg: string) => void }) {
  const [creds, setCreds] = useState<Credential[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    try { setCreds(await apiClient<Credential[]>('/api/v1/aggregator/credentials')); }
    catch (e) { setError((e as Error).message); }
    finally { setLoading(false); }
  }
  useEffect(() => { void load(); }, []);

  if (loading) {
    return <div className="space-y-2">{Array.from({ length: 2 }).map((_, i) => <div key={i} className="h-16 animate-pulse rounded bg-muted" />)}</div>;
  }
  if (error) return <div className="text-sm text-status-error">{error}</div>;

  return (
    <div className="space-y-5">
      {creds.map(c => <CredentialCard key={c.aggregator} cred={c} flash={flash} reload={load} />)}
      {creds.length === 0 && <div className="rounded-lg border border-dashed border-border p-8 text-center text-sm text-muted-foreground">No aggregator credentials configured.</div>}
    </div>
  );
}

function CredentialCard({ cred, flash, reload }: { cred: Credential; flash: (msg: string) => void; reload: () => Promise<void> }) {
  const [isEnabled, setIsEnabled] = useState<boolean>(cred.isEnabled);
  const [environment, setEnvironment] = useState<string>(cred.environment);
  const [clientId, setClientId] = useState<string>(cred.clientId ?? '');
  const [clientSecret, setClientSecret] = useState<string>(''); // write-only
  const [webhookSecret, setWebhookSecret] = useState<string>(''); // write-only
  const [baseUrl, setBaseUrl] = useState<string>(cred.baseUrl ?? '');
  const [saving, setSaving] = useState(false);

  async function save() {
    setSaving(true);
    try {
      const body: { clientId?: string; clientSecret?: string; webhookSecret?: string; environment?: string; baseUrl?: string; isEnabled?: boolean } = {};
      if (clientId !== (cred.clientId ?? '')) body.clientId = clientId;
      if (clientSecret.trim()) body.clientSecret = clientSecret;
      if (webhookSecret.trim()) body.webhookSecret = webhookSecret;
      if (environment !== cred.environment) body.environment = environment;
      if (baseUrl !== (cred.baseUrl ?? '')) body.baseUrl = baseUrl;
      if (isEnabled !== cred.isEnabled) body.isEnabled = isEnabled;
      await apiClient(`/api/v1/aggregator/credentials/${cred.aggregator}`, { method: 'PUT', body: JSON.stringify(body) });
      flash(`Saved ${SOURCE_LABEL[cred.aggregator]} credentials.`);
      setClientSecret(''); setWebhookSecret('');
      await reload();
    } catch (e) { flash(extractError(e, 'Could not save credentials.')); }
    finally { setSaving(false); }
  }

  const fieldCls = 'w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20';

  return (
    <div className="rounded-lg border border-border p-5">
      <div className="mb-4 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <SourceBadge source={cred.aggregator} />
          <span className={`pill ${cred.isEnabled ? 'pill-paid' : 'pill-idle'}`}>{cred.isEnabled ? 'enabled' : 'disabled'}</span>
        </div>
        <label className="flex items-center gap-2 text-sm">
          <input type="checkbox" checked={isEnabled} onChange={e => setIsEnabled(e.target.checked)} className="size-4 rounded border-border text-primary focus:ring-primary/20" />
          <span className="font-medium">Enabled</span>
        </label>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <div>
          <label className="mb-1 block text-sm font-semibold text-slate-700">Environment</label>
          <select value={environment} onChange={e => setEnvironment(e.target.value)} className={fieldCls}>
            <option value="sandbox">sandbox</option>
            <option value="production">production</option>
          </select>
        </div>
        <div>
          <label className="mb-1 block text-sm font-semibold text-slate-700">Base URL</label>
          <input value={baseUrl} onChange={e => setBaseUrl(e.target.value)} placeholder="https://api.example.com" className={fieldCls} />
        </div>
        <div>
          <label className="mb-1 block text-sm font-semibold text-slate-700">Client ID</label>
          <input value={clientId} onChange={e => setClientId(e.target.value)} className={fieldCls} />
        </div>
        <div>
          <label className="mb-1 block text-sm font-semibold text-slate-700">Client secret</label>
          <input type="password" value={clientSecret} onChange={e => setClientSecret(e.target.value)}
            placeholder={cred.hasClientSecret ? `${cred.clientSecretHint ?? '••••'} — leave blank to keep` : 'Not set'} className={fieldCls} />
        </div>
        <div>
          <label className="mb-1 block text-sm font-semibold text-slate-700">Webhook secret</label>
          <input type="password" value={webhookSecret} onChange={e => setWebhookSecret(e.target.value)}
            placeholder={cred.hasWebhookSecret ? '•••• — leave blank to keep' : 'Not set'} className={fieldCls} />
        </div>
      </div>

      <div className="mt-4 flex justify-end">
        <button onClick={save} disabled={saving} className="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary-dark disabled:opacity-50">{saving ? 'Saving…' : 'Save'}</button>
      </div>

      {/* Store mapping */}
      <div className="mt-6">
        <h4 className="mb-2 text-sm font-semibold text-slate-700">Store mapping</h4>
        {cred.aggregator === 'pickme' && (
          <p className="mb-2 text-xs text-muted-foreground">
            PickMe issues one <span className="font-mono">X-API-KEY</span> per outlet. Paste each outlet&apos;s key here; it&apos;s encrypted at rest and never shown again.
          </p>
        )}
        <div className="overflow-hidden rounded-lg border border-border">
          <table className="w-full text-sm">
            <thead className="border-b border-border bg-muted/50 text-left text-xs uppercase tracking-wide text-muted-foreground">
              <tr>
                <th className="px-4 py-2.5 font-medium">Location</th>
                <th className="px-4 py-2.5 font-medium">External store id</th>
                {cred.aggregator === 'pickme' && <th className="px-4 py-2.5 font-medium">PickMe X-API-KEY</th>}
                <th className="px-4 py-2.5 font-medium">Enabled</th>
                {cred.aggregator === 'pickme' && <th className="px-4 py-2.5 font-medium">Last polled</th>}
                <th className="px-4 py-2.5 text-right font-medium">Action</th>
              </tr>
            </thead>
            <tbody>
              {cred.stores.map((s, i) => <StoreRow key={s.locationId} aggregator={cred.aggregator} store={s} zebra={i % 2 === 1} flash={flash} reload={reload} />)}
              {cred.stores.length === 0 && (
                <tr><td colSpan={cred.aggregator === 'pickme' ? 6 : 4} className="px-4 py-8 text-center text-muted-foreground">No locations to map.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

function StoreRow({ aggregator, store, zebra, flash, reload }: { aggregator: OrderSource; store: Store; zebra: boolean; flash: (msg: string) => void; reload: () => Promise<void> }) {
  const isPickMe = aggregator === 'pickme';
  const [externalStoreId, setExternalStoreId] = useState<string>(store.externalStoreId ?? '');
  const [isEnabled, setIsEnabled] = useState<boolean>(store.isEnabled);
  const [apiKey, setApiKey] = useState<string>(''); // write-only
  const [saving, setSaving] = useState(false);

  async function save() {
    setSaving(true);
    try {
      const body: { externalStoreId?: string; isEnabled?: boolean; apiKey?: string } = {};
      if (externalStoreId !== (store.externalStoreId ?? '')) body.externalStoreId = externalStoreId;
      if (isEnabled !== store.isEnabled) body.isEnabled = isEnabled;
      if (apiKey.trim()) body.apiKey = apiKey.trim();
      await apiClient(`/api/v1/aggregator/credentials/${aggregator}/stores/${store.locationId}`, { method: 'PUT', body: JSON.stringify(body) });
      flash(`Saved ${store.code} store mapping.`);
      setApiKey('');
      await reload();
    } catch (e) { flash(extractError(e, 'Could not save store mapping.')); }
    finally { setSaving(false); }
  }

  const cellCls = 'w-full rounded-lg border border-border bg-surface px-3 py-1.5 text-sm focus:border-primary focus:ring-2 focus:ring-primary/20';

  return (
    <tr className={zebra ? 'bg-muted/20' : ''}>
      <td className="px-4 py-2.5"><span className="font-mono text-xs text-muted-foreground">{store.code}</span> <span>— {store.name}</span></td>
      <td className="px-4 py-2.5"><input value={externalStoreId} onChange={e => setExternalStoreId(e.target.value)} placeholder="store_xxx" className={cellCls} /></td>
      {isPickMe && (
        <td className="px-4 py-2.5">
          <input type="password" value={apiKey} onChange={e => setApiKey(e.target.value)}
            placeholder={store.hasApiKey ? '•••• set — leave blank to keep' : 'paste outlet key'}
            className={`${cellCls} font-mono text-xs`} />
        </td>
      )}
      <td className="px-4 py-2.5">
        <label className="flex items-center gap-2 text-sm">
          <input type="checkbox" checked={isEnabled} onChange={e => setIsEnabled(e.target.checked)} className="size-4 rounded border-border text-primary focus:ring-primary/20" />
          <span className="text-muted-foreground">{isEnabled ? 'On' : 'Off'}</span>
        </label>
      </td>
      {isPickMe && <td className="px-4 py-2.5 text-xs text-muted-foreground">{fmtDate(store.lastPolledAt)}</td>}
      <td className="px-4 py-2.5 text-right">
        <button onClick={save} disabled={saving} className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-medium hover:bg-muted disabled:opacity-50">{saving ? 'Saving…' : 'Save'}</button>
      </td>
    </tr>
  );
}
