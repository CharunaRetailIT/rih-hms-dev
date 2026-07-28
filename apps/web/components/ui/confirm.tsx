'use client';

import { useEffect, useState, type CSSProperties, type ReactNode } from 'react';
import { X, AlertTriangle } from 'lucide-react';

type Opts = { title: string; body?: ReactNode; confirmLabel?: string; cancelLabel?: string; danger?: boolean };
type Req = Opts & { resolve: (v: boolean) => void };

let emit: ((r: Req | null) => void) | null = null;

/**
 * App-wide confirmation dialog. Call `await confirmDialog({ ... })` from anywhere
 * — destructive/irreversible actions (delete, void, deactivate, post) should be
 * gated through it. Returns true if confirmed. Falls back to window.confirm if
 * the host isn't mounted (e.g. SSR).
 */
export function confirmDialog(opts: Opts): Promise<boolean> {
  return new Promise(resolve => {
    if (emit) emit({ ...opts, resolve });
    else resolve(typeof window !== 'undefined' ? window.confirm(opts.title) : false);
  });
}

// ─────────────────────────── Prompt (text input) dialog ───────────────────────────
type POpts = { title: string; body?: string; placeholder?: string; defaultValue?: string; confirmLabel?: string; inputMode?: 'text' | 'numeric'; maxLength?: number; password?: boolean };
type PReq = POpts & { resolve: (v: string | null) => void };
let pEmit: ((r: PReq | null) => void) | null = null;

/** App-wide text-input dialog — a styled replacement for window.prompt. Resolves to the
 * entered string (may be empty) or null if cancelled. */
export function promptDialog(opts: POpts): Promise<string | null> {
  return new Promise(resolve => {
    if (pEmit) pEmit({ ...opts, resolve });
    else resolve(typeof window !== 'undefined' ? window.prompt(opts.title, opts.defaultValue ?? '') : null);
  });
}

/** Mount once near the app root. Renders the active prompt dialog. */
export function PromptHost() {
  const [req, setReq] = useState<PReq | null>(null);
  const [val, setVal] = useState('');
  useEffect(() => { pEmit = (r) => { setReq(r); setVal(r?.defaultValue ?? ''); }; return () => { pEmit = null; }; }, []);
  if (!req) return null;
  const close = (v: string | null) => { req.resolve(v); setReq(null); };
  return (
    <div className="fixed inset-0 z-[120] flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm" onClick={() => close(null)}>
      <div className="w-full max-w-sm overflow-hidden rounded-xl bg-card shadow-2xl" onClick={e => e.stopPropagation()}>
        <div className="flex items-center justify-between bg-primary px-5 py-3 text-white">
          <h3 className="font-heading text-base font-bold">{req.title}</h3>
          <button onClick={() => close(null)} className="rounded-lg p-1 text-white/80 hover:bg-white/15" aria-label="Close"><X className="size-5" /></button>
        </div>
        <div className="p-5">
          {req.body && <p className="mb-3 text-sm leading-relaxed text-muted-foreground">{req.body}</p>}
          {/* type=text (not password) + CSS mask + autofill-off hints so password managers
              (iCloud Passwords / 1Password / LastPass) don't pop up over a PIN field. */}
          <input autoFocus type="text" inputMode={req.inputMode} maxLength={req.maxLength}
            name="rit-prompt-field" autoComplete="off" autoCorrect="off" autoCapitalize="off" spellCheck={false}
            data-1p-ignore data-lpignore="true"
            style={req.password ? ({ WebkitTextSecurity: 'disc' } as unknown as CSSProperties) : undefined}
            value={val} onChange={e => setVal(e.target.value)} placeholder={req.placeholder}
            onKeyDown={e => { if (e.key === 'Enter') close(val); if (e.key === 'Escape') close(null); }}
            className="w-full rounded-lg border border-border bg-background px-3 py-2.5 outline-none focus:border-primary focus:ring-2 focus:ring-primary/20" />
          <div className="mt-5 flex gap-2">
            <button onClick={() => close(null)} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted">Cancel</button>
            <button onClick={() => close(val)} className="h-11 flex-1 rounded-lg bg-primary font-bold text-white hover:bg-primary-dark">{req.confirmLabel ?? 'OK'}</button>
          </div>
        </div>
      </div>
    </div>
  );
}

/** Mount once near the app root (tenant layout). Renders the active confirm dialog. */
export function ConfirmHost() {
  const [req, setReq] = useState<Req | null>(null);
  useEffect(() => { emit = setReq; return () => { emit = null; }; }, []);
  useEffect(() => {
    if (!req) return;
    const onKey = (e: KeyboardEvent) => { if (e.key === 'Escape') close(false); };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [req]);
  if (!req) return null;
  const close = (v: boolean) => { req.resolve(v); setReq(null); };
  const danger = req.danger;
  return (
    <div className="fixed inset-0 z-[120] flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm" onClick={() => close(false)}>
      <div className="w-full max-w-sm overflow-hidden rounded-xl bg-card shadow-2xl" onClick={e => e.stopPropagation()}>
        <div className={`flex items-center justify-between px-5 py-3 text-white ${danger ? 'bg-status-error' : 'bg-primary'}`}>
          <h3 className="flex items-center gap-2 font-heading text-base font-bold">
            {danger && <AlertTriangle className="size-4" />}{req.title}
          </h3>
          <button onClick={() => close(false)} className="rounded-lg p-1 text-white/80 hover:bg-white/15" aria-label="Close"><X className="size-5" /></button>
        </div>
        <div className="p-5">
          {req.body && <div className="mb-5 text-sm leading-relaxed text-muted-foreground">{req.body}</div>}
          <div className="flex gap-2">
            <button onClick={() => close(false)} className="h-11 flex-1 rounded-lg border border-border font-semibold hover:bg-muted">{req.cancelLabel ?? 'Cancel'}</button>
            <button autoFocus onClick={() => close(true)}
              className={`h-11 flex-1 rounded-lg font-bold text-white ${danger ? 'bg-status-error hover:opacity-90' : 'bg-primary hover:bg-primary-dark'}`}>
              {req.confirmLabel ?? 'Confirm'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
