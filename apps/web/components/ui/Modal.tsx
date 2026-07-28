'use client';

import { X } from 'lucide-react';
import type { ReactNode } from 'react';
import { cn } from '@/lib/utils';

const SIZES: Record<string, string> = {
  sm: 'max-w-sm', md: 'max-w-md', lg: 'max-w-lg', xl: 'max-w-xl', '2xl': 'max-w-2xl', '3xl': 'max-w-3xl',
};

const TONES: Record<string, string> = {
  primary: 'bg-primary text-primary-foreground',
  danger: 'bg-status-error text-white',
};

/**
 * Standard app modal: dimmed/blurred backdrop, a full-width colored header strip
 * (white title + close X inside the strip, end to end), and a scrollable body.
 * Use everywhere for consistency.
 */
export function Modal({ title, onClose, children, size = 'lg', footer, icon, tone = 'primary' }: {
  title: string;
  onClose: () => void;
  children: React.ReactNode;
  size?: keyof typeof SIZES;
  footer?: React.ReactNode;
  icon?: ReactNode;
  tone?: keyof typeof TONES;
}) {
  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm" onClick={onClose}>
      <div className={cn('flex max-h-[90vh] w-full flex-col overflow-hidden rounded-xl bg-card shadow-2xl', SIZES[size])} onClick={e => e.stopPropagation()}>
        <div className={cn('flex shrink-0 items-center justify-between px-5 py-3', TONES[tone])}>
          <h3 className="flex min-w-0 items-center gap-2 truncate font-heading text-base font-bold">
            {icon && <span className="grid size-7 shrink-0 place-items-center rounded-lg bg-white/15">{icon}</span>}
            <span className="truncate">{title}</span>
          </h3>
          <button onClick={onClose} className="ml-3 shrink-0 rounded-lg p-1 text-primary-foreground/80 transition-colors hover:bg-white/15 hover:text-white" aria-label="Close">
            <X className="size-5" />
          </button>
        </div>
        <div className="overflow-auto overscroll-contain p-6">{children}</div>
        {footer && <div className="shrink-0 border-t border-border bg-card p-4">{footer}</div>}
      </div>
    </div>
  );
}
