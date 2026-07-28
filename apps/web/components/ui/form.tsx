'use client';

import { useEffect, useRef, useState, type ReactNode } from 'react';
import { ChevronDown, Search, Check } from 'lucide-react';
import { cn } from '@/lib/utils';
import { DateTimePicker, type DTMode } from '@/components/ui/DateTimePicker';

const DATE_TYPES: Record<string, DTMode> = {
  date: 'date', 'datetime-local': 'datetime', time: 'time', month: 'month',
};

const shell = 'rounded-lg border border-border bg-surface px-3 py-1.5 transition-colors focus-within:border-primary focus-within:ring-2 focus-within:ring-primary/20';

/**
 * Text/number field with the label INSIDE the box (stacked above the input) —
 * clean, always-readable, no separate label row. Optional top-right `hint`
 * (e.g. a live margin %) and a `helper` line below.
 */
export function Field({ label, value, onChange, mono, inputMode, type = 'text', placeholder, error, hint, helper, disabled, className, multiline, rows, up, min }: {
  label: string;
  value: string;
  onChange: (v: string) => void;
  mono?: boolean;
  inputMode?: 'decimal' | 'numeric' | 'text';
  /** Native input type — 'text' | 'date' | 'time' | 'month' | 'number' | 'email' | 'tel' | 'password' … */
  type?: string;
  placeholder?: string;
  error?: string;
  hint?: ReactNode;
  helper?: ReactNode;
  disabled?: boolean;
  className?: string;
  multiline?: boolean;
  rows?: number;
  up?: boolean;
  min?: string;
}) {
  // Date / time types use the branded picker (the native popup can't be themed).
  if (DATE_TYPES[type]) {
    return <DateTimePicker label={label} value={value} onChange={onChange} mode={DATE_TYPES[type]}
      placeholder={placeholder} error={error} helper={helper} up={up} className={className} disabled={disabled} min={min} />;
  }
  const inputCls = cn('w-full bg-transparent py-0.5 text-sm text-on-surface outline-none placeholder:text-muted-foreground/60', mono && 'font-mono');
  return (
    <div className={className}>
      <div className={cn(shell, error && 'border-status-error focus-within:border-status-error focus-within:ring-status-error/20')}>
        <div className="flex items-center justify-between">
          <label className="block text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">{label}</label>
          {hint}
        </div>
        {multiline ? (
          <textarea value={value} onChange={e => onChange(e.target.value)} placeholder={placeholder} disabled={disabled} rows={rows ?? 3}
            autoComplete="off" autoCorrect="off" autoCapitalize="off" spellCheck={false} data-1p-ignore="true" data-lpignore="true" data-form-type="other"
            className={cn(inputCls, 'resize-none')} />
        ) : (
          <input value={value} onChange={e => onChange(e.target.value)} type={type} placeholder={placeholder} inputMode={inputMode} disabled={disabled}
            autoComplete="off" autoCorrect="off" autoCapitalize="off" spellCheck={false} data-1p-ignore="true" data-lpignore="true" data-form-type="other"
            className={inputCls} />
        )}
      </div>
      {error ? <p className="mt-1 px-3 text-xs text-status-error">{error}</p> : helper ? <p className="mt-1 px-3 text-xs text-muted-foreground">{helper}</p> : null}
    </div>
  );
}

type Opt = { value: string; label: string };

/**
 * Searchable dropdown (combobox). Replaces native <select> — the trigger has a
 * properly-padded chevron, the menu has a search box that filters options, and
 * keyboard support (Esc closes, Enter picks the top match). With `label` it
 * renders the in-field stacked-label style; without it, a plain toolbar control.
 */
export function Combobox({ label, value, onChange, options, placeholder = 'Select…', searchPlaceholder = 'Search…', className, error, helper }: {
  label?: string;
  value: string;
  onChange: (v: string) => void;
  options: Opt[];
  placeholder?: string;
  searchPlaceholder?: string;
  className?: string;
  error?: string;
  helper?: ReactNode;
  up?: boolean;
}) {
  const [open, setOpen] = useState(false);
  const [q, setQ] = useState('');
  const ref = useRef<HTMLDivElement>(null);
  const triggerRef = useRef<HTMLDivElement>(null);
  const popRef = useRef<HTMLDivElement>(null);
  const [coords, setCoords] = useState<{ top: number; left: number; width: number; up: boolean } | null>(null);
  const sel = options.find(o => o.value === value);
  const ql = q.trim().toLowerCase();
  const filtered = ql ? options.filter(o => o.label.toLowerCase().includes(ql)) : options;

  // Fixed-positioned menu so it escapes modal `overflow` clipping; flips up when low.
  function place() {
    const el = triggerRef.current; if (!el) return;
    const r = el.getBoundingClientRect();
    const popH = 320;
    const flipUp = r.bottom + popH > window.innerHeight - 8 && r.top > popH;
    setCoords({ top: flipUp ? r.top - 6 : r.bottom + 6, left: r.left, width: r.width, up: flipUp });
  }

  useEffect(() => {
    if (!open) return;
    place();
    const onScroll = () => place();
    const h = (e: MouseEvent) => {
      const t = e.target as Node;
      if (ref.current && !ref.current.contains(t) && popRef.current && !popRef.current.contains(t)) setOpen(false);
    };
    document.addEventListener('mousedown', h);
    window.addEventListener('scroll', onScroll, true); window.addEventListener('resize', onScroll);
    return () => {
      document.removeEventListener('mousedown', h);
      window.removeEventListener('scroll', onScroll, true); window.removeEventListener('resize', onScroll);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  const trigger = (
    <button type="button" onClick={() => { setOpen(o => !o); setQ(''); }}
      className="flex w-full items-center justify-between gap-2 text-left text-sm text-on-surface outline-none">
      <span className={cn('truncate', !sel && 'text-muted-foreground')}>{sel?.label ?? placeholder}</span>
      <ChevronDown className={cn('size-4 shrink-0 text-muted-foreground transition-transform', open && 'rotate-180')} />
    </button>
  );

  return (
    <div className={className}>
      <div ref={ref} className="relative">
        {label ? (
          <div ref={triggerRef} className={cn(shell, open && 'border-primary ring-2 ring-primary/20', error && 'border-status-error')}>
            <span className="block text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">{label}</span>
            <div className="py-0.5">{trigger}</div>
          </div>
        ) : (
          <div ref={triggerRef} className={cn('rounded-lg border border-border bg-card px-3 py-2.5 transition-colors', open && 'border-primary ring-2 ring-primary/20')}>{trigger}</div>
        )}

        {open && coords && (
          <div ref={popRef} className="fixed z-[90] overflow-hidden rounded-lg border border-border bg-card shadow-2xl"
            style={{ top: coords.top, left: coords.left, minWidth: Math.max(coords.width, 240), maxWidth: 'min(22rem, 92vw)', transform: coords.up ? 'translateY(-100%)' : undefined }}>
            <div className="flex items-center gap-2 border-b border-border px-3 py-2">
              <Search className="size-4 shrink-0 text-muted-foreground" />
              <input autoFocus value={q} onChange={e => setQ(e.target.value)} placeholder={searchPlaceholder}
                autoComplete="off" autoCorrect="off" autoCapitalize="off" spellCheck={false} data-1p-ignore="true" data-lpignore="true" data-form-type="other"
                onKeyDown={e => { if (e.key === 'Escape') setOpen(false); if (e.key === 'Enter' && filtered[0]) { onChange(filtered[0].value); setOpen(false); } }}
                className="w-full bg-transparent text-sm outline-none placeholder:text-muted-foreground" />
            </div>
            <div className="max-h-60 overflow-y-auto py-1">
              {filtered.map(o => (
                <button key={o.value} type="button" onClick={() => { onChange(o.value); setOpen(false); }}
                  className={cn('flex w-full items-center justify-between gap-2 px-3 py-2 text-left text-sm hover:bg-muted', o.value === value ? 'font-semibold text-primary' : 'text-on-surface')}>
                  <span className="truncate">{o.label}</span>
                  {o.value === value && <Check className="size-4 shrink-0" />}
                </button>
              ))}
              {filtered.length === 0 && <p className="px-3 py-6 text-center text-xs text-muted-foreground">No matches for “{q.trim()}”.</p>}
            </div>
          </div>
        )}
      </div>
      {error ? <p className="mt-1 px-3 text-xs text-status-error">{error}</p> : helper ? <p className="mt-1 px-3 text-xs text-muted-foreground">{helper}</p> : null}
    </div>
  );
}
