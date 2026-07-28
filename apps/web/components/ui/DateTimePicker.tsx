'use client';

import { useEffect, useMemo, useRef, useState, type ReactNode } from 'react';
import { Calendar, Clock, ChevronLeft, ChevronRight } from 'lucide-react';
import { cn } from '@/lib/utils';

const shell = 'rounded-lg border border-border bg-surface px-3 py-1.5 transition-colors focus-within:border-primary focus-within:ring-2 focus-within:ring-primary/20';
const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
const DOW = ['Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa', 'Su'];

export type DTMode = 'date' | 'datetime' | 'time' | 'month';
type Parts = { y: number; mo: number; d: number; hh: number; mm: number };

const pad = (n: number) => String(n).padStart(2, '0');

function parseValue(value: string, mode: DTMode): Parts | null {
  if (!value) return null;
  if (mode === 'time') { const [h, m] = value.split(':').map(Number); return { y: 0, mo: 0, d: 1, hh: h || 0, mm: m || 0 }; }
  if (mode === 'month') { const [y, mo] = value.split('-').map(Number); return { y: y || 0, mo: (mo || 1) - 1, d: 1, hh: 0, mm: 0 }; }
  const [datePart, timePart] = value.split('T');
  const [y, mo, d] = datePart.split('-').map(Number);
  const [hh, mm] = (timePart ?? '00:00').split(':').map(Number);
  return { y: y || 0, mo: (mo || 1) - 1, d: d || 1, hh: hh || 0, mm: mm || 0 };
}

function toValue(p: Parts, mode: DTMode): string {
  if (mode === 'time') return `${pad(p.hh)}:${pad(p.mm)}`;
  if (mode === 'month') return `${p.y}-${pad(p.mo + 1)}`;
  const date = `${p.y}-${pad(p.mo + 1)}-${pad(p.d)}`;
  return mode === 'datetime' ? `${date}T${pad(p.hh)}:${pad(p.mm)}` : date;
}

function display(p: Parts | null, mode: DTMode): string {
  if (!p) return '';
  const t12 = () => { const h = (p.hh % 12) || 12; return `${pad(h)}:${pad(p.mm)} ${p.hh < 12 ? 'AM' : 'PM'}`; };
  if (mode === 'time') return t12();
  if (mode === 'month') return `${MONTHS[p.mo]} ${p.y}`;
  const ds = `${pad(p.d)} ${MONTHS[p.mo]} ${p.y}`;
  return mode === 'datetime' ? `${ds}, ${t12()}` : ds;
}

/** Brand-green date / time / datetime / month picker — replaces the native (blue) control. */
export function DateTimePicker({ label, value, onChange, mode = 'date', placeholder = 'Select…', error, helper, className, disabled, min }: {
  label?: string;
  value: string;
  onChange: (v: string) => void;
  mode?: DTMode;
  placeholder?: string;
  error?: string;
  helper?: ReactNode;
  up?: boolean;
  className?: string;
  disabled?: boolean;
  /** Earliest allowed value (same string format as `value`); earlier days are disabled. */
  min?: string;
}) {
  const minP = min ? parseValue(min, mode === 'time' ? 'datetime' : mode) : null;
  const minNum = minP ? minP.y * 10000 + minP.mo * 100 + minP.d : null;
  const dayNum = (d: Date) => d.getFullYear() * 10000 + d.getMonth() * 100 + d.getDate();
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);
  const triggerRef = useRef<HTMLDivElement>(null);
  const popRef = useRef<HTMLDivElement>(null);
  const [coords, setCoords] = useState<{ top: number; left: number; width: number } | null>(null);
  const parts = parseValue(value, mode);
  const now = new Date();
  const [viewY, setViewY] = useState(parts?.y || now.getFullYear());
  const [viewM, setViewM] = useState(parts?.mo ?? now.getMonth());

  // Fixed-positioned popover so it escapes modal `overflow` clipping; flips above
  // the field when there isn't room below.
  function place() {
    const el = triggerRef.current; if (!el) return;
    const r = el.getBoundingClientRect();
    const popH = mode === 'time' ? 130 : (mode === 'date' || mode === 'month') ? 340 : 400;
    const flipUp = r.bottom + popH > window.innerHeight - 8 && r.top > popH;
    setCoords({ top: flipUp ? Math.max(8, r.top - popH - 6) : r.bottom + 6, left: r.left, width: r.width });
  }

  useEffect(() => {
    if (!open) return;
    const p = parseValue(value, mode);
    if (p && mode !== 'time') { setViewY(p.y); setViewM(p.mo); }
    place();
    const onScroll = () => place();
    const h = (e: MouseEvent) => {
      const t = e.target as Node;
      if (ref.current && !ref.current.contains(t) && popRef.current && !popRef.current.contains(t)) setOpen(false);
    };
    const k = (e: KeyboardEvent) => { if (e.key === 'Escape') setOpen(false); };
    document.addEventListener('mousedown', h); document.addEventListener('keydown', k);
    window.addEventListener('scroll', onScroll, true); window.addEventListener('resize', onScroll);
    return () => {
      document.removeEventListener('mousedown', h); document.removeEventListener('keydown', k);
      window.removeEventListener('scroll', onScroll, true); window.removeEventListener('resize', onScroll);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  // Calendar grid (Mon-first) for the viewed month.
  const grid = useMemo(() => {
    const first = new Date(viewY, viewM, 1);
    const startDow = (first.getDay() + 6) % 7;                 // 0=Mon
    const start = new Date(viewY, viewM, 1 - startDow);
    return Array.from({ length: 42 }, (_, i) => { const d = new Date(start); d.setDate(start.getDate() + i); return d; });
  }, [viewY, viewM]);

  const emit = (next: Parts) => onChange(toValue(next, mode));
  const base = (): Parts => parts ?? { y: now.getFullYear(), mo: now.getMonth(), d: now.getDate(), hh: now.getHours(), mm: now.getMinutes() };

  function pickDay(d: Date) {
    const next = { ...base(), y: d.getFullYear(), mo: d.getMonth(), d: d.getDate() };
    emit(next);
    if (mode === 'date') setOpen(false);
  }
  function setHM(hh: number, mm: number) { emit({ ...base(), hh, mm }); }

  const h12 = parts ? ((parts.hh % 12) || 12) : 12;
  const ampm = parts ? (parts.hh < 12 ? 'AM' : 'PM') : 'AM';
  const setH12 = (h: number) => { const hh = ampm === 'AM' ? (h % 12) : (h % 12) + 12; setHM(hh, parts?.mm ?? 0); };
  const setAmpm = (ap: 'AM' | 'PM') => { const h = h12 % 12; setHM(ap === 'AM' ? h : h + 12, parts?.mm ?? 0); };

  const selCls = 'rounded-lg border border-border bg-surface px-2 py-1.5 text-sm outline-none focus:border-primary';

  return (
    <div className={className}>
      <div ref={ref} className="relative">
        <div ref={triggerRef} className={cn(shell, open && 'border-primary ring-2 ring-primary/20', error && 'border-status-error', disabled && 'opacity-60')}>
          {label && <span className="block text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">{label}</span>}
          <button type="button" disabled={disabled} onClick={() => setOpen(o => !o)}
            className="flex w-full items-center justify-between gap-2 py-0.5 text-left text-sm text-on-surface outline-none">
            <span className={cn('truncate', !parts && 'text-muted-foreground/60')}>{parts ? display(parts, mode) : placeholder}</span>
            {mode === 'time' ? <Clock className="size-4 shrink-0 text-muted-foreground" /> : <Calendar className="size-4 shrink-0 text-muted-foreground" />}
          </button>
        </div>

        {open && coords && (
          <div ref={popRef} className="fixed z-[90] rounded-xl border border-border bg-card p-3 shadow-2xl"
            style={{ top: coords.top, left: coords.left, minWidth: Math.max(coords.width, 272) }}>
            {/* Month / day calendar */}
            {(mode === 'date' || mode === 'datetime' || mode === 'month') && (
              <div className="mb-1 flex items-center justify-between px-1">
                <button type="button" onClick={() => { if (mode === 'month') setViewY(y => y - 1); else { const m = viewM - 1; if (m < 0) { setViewM(11); setViewY(y => y - 1); } else setViewM(m); } }}
                  className="rounded-lg p-1 text-muted-foreground hover:bg-muted"><ChevronLeft className="size-4" /></button>
                <span className="font-heading text-sm font-bold">{mode === 'month' ? viewY : `${MONTHS[viewM]} ${viewY}`}</span>
                <button type="button" onClick={() => { if (mode === 'month') setViewY(y => y + 1); else { const m = viewM + 1; if (m > 11) { setViewM(0); setViewY(y => y + 1); } else setViewM(m); } }}
                  className="rounded-lg p-1 text-muted-foreground hover:bg-muted"><ChevronRight className="size-4" /></button>
              </div>
            )}

            {mode === 'month' ? (
              <div className="grid grid-cols-3 gap-1">
                {MONTHS.map((m, i) => {
                  const on = parts && parts.y === viewY && parts.mo === i;
                  return (
                    <button key={m} type="button" onClick={() => { emit({ ...base(), y: viewY, mo: i }); setOpen(false); }}
                      className={cn('rounded-lg px-2 py-2 text-sm font-medium', on ? 'bg-primary text-white' : 'hover:bg-muted')}>{m}</button>
                  );
                })}
              </div>
            ) : mode !== 'time' && (
              <>
                <div className="grid grid-cols-7 gap-0.5 px-1 text-center text-[11px] font-semibold text-muted-foreground">
                  {DOW.map((d, i) => <span key={i} className="py-1">{d}</span>)}
                </div>
                <div className="grid grid-cols-7 gap-0.5">
                  {grid.map((d, i) => {
                    const inMonth = d.getMonth() === viewM;
                    const isSel = parts && d.getFullYear() === parts.y && d.getMonth() === parts.mo && d.getDate() === parts.d;
                    const isToday = d.toDateString() === now.toDateString();
                    const blocked = minNum != null && dayNum(d) < minNum;
                    return (
                      <button key={i} type="button" disabled={blocked} onClick={() => !blocked && pickDay(d)}
                        className={cn('flex size-8 items-center justify-center rounded-lg text-sm',
                          blocked ? 'cursor-not-allowed text-muted-foreground/30'
                            : isSel ? 'bg-primary font-bold text-white' : inMonth ? 'text-on-surface hover:bg-muted' : 'text-muted-foreground/40 hover:bg-muted',
                          !isSel && !blocked && isToday && 'ring-1 ring-primary')}>
                        {d.getDate()}
                      </button>
                    );
                  })}
                </div>
              </>
            )}

            {/* Time row */}
            {(mode === 'datetime' || mode === 'time') && (
              <div className={cn('flex items-center gap-2', mode === 'datetime' && 'mt-3 border-t border-border pt-3')}>
                <Clock className="size-4 shrink-0 text-muted-foreground" />
                <select className={selCls} value={h12} onChange={e => setH12(Number(e.target.value))}>
                  {Array.from({ length: 12 }, (_, i) => i + 1).map(h => <option key={h} value={h}>{pad(h)}</option>)}
                </select>
                <span className="font-bold text-muted-foreground">:</span>
                <select className={selCls} value={parts?.mm ?? 0} onChange={e => setHM(parts?.hh ?? 0, Number(e.target.value))}>
                  {Array.from({ length: 60 }, (_, i) => i).map(m => <option key={m} value={m}>{pad(m)}</option>)}
                </select>
                <div className="ml-1 flex overflow-hidden rounded-lg border border-border">
                  {(['AM', 'PM'] as const).map(ap => (
                    <button key={ap} type="button" onClick={() => setAmpm(ap)}
                      className={cn('px-2.5 py-1.5 text-xs font-bold', ampm === ap ? 'bg-primary text-white' : 'text-muted-foreground hover:bg-muted')}>{ap}</button>
                  ))}
                </div>
              </div>
            )}

            <div className="mt-2 flex items-center justify-between border-t border-border pt-2 text-xs font-semibold text-primary">
              <button type="button" onClick={() => { onChange(''); setOpen(false); }} className="rounded px-1.5 py-0.5 hover:bg-muted">Clear</button>
              <button type="button" disabled={minNum != null && dayNum(new Date()) < minNum} onClick={() => {
                const t = new Date();
                const next: Parts = mode === 'time'
                  ? { ...base(), hh: t.getHours(), mm: t.getMinutes() }
                  : { y: t.getFullYear(), mo: t.getMonth(), d: t.getDate(), hh: t.getHours(), mm: t.getMinutes() };
                emit(next); setViewY(t.getFullYear()); setViewM(t.getMonth());
                if (mode === 'date' || mode === 'month') setOpen(false);
              }} className="rounded px-1.5 py-0.5 hover:bg-muted disabled:opacity-40">{mode === 'time' ? 'Now' : 'Today'}</button>
            </div>
          </div>
        )}
      </div>
      {error ? <p className="mt-1 px-3 text-xs text-status-error">{error}</p> : helper ? <p className="mt-1 px-3 text-xs text-muted-foreground">{helper}</p> : null}
    </div>
  );
}
