import type { ReactNode } from 'react';
import { cn } from '@/lib/utils';

/**
 * The standard summary line that sits under a page's main title — rendered as a
 * white chip with a subtle border: bold-black text, green emphasised numbers
 * (wrap counts in <Num>). Keeps every page's header stat consistent.
 */
export function HeaderStat({ children, className }: { children: ReactNode; className?: string }) {
  return (
    <div className={cn('mt-1.5 inline-flex flex-wrap items-center gap-x-1 rounded-lg border border-border bg-card px-3 py-1.5 text-sm font-bold text-on-surface shadow-sm', className)}>
      {children}
    </div>
  );
}

/** Green, emphasised number for use inside <HeaderStat>. */
export function Num({ children }: { children: ReactNode }) {
  return <span className="font-bold text-primary">{children}</span>;
}
