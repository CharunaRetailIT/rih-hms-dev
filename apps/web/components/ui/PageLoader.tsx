import { cn } from '@/lib/utils';

/**
 * Branded full-section preloader — a spinning ring around the RIT HMS mark.
 * Use for page/data loading states so the whole app shows one consistent
 * loading experience instead of bare "Loading…" text.
 */
export function PageLoader({ label = 'Loading…', className }: { label?: string; className?: string }) {
  return (
    <div className={cn('flex min-h-[55vh] w-full flex-col items-center justify-center gap-4', className)}>
      <div className="relative flex size-14 items-center justify-center">
        <span className="absolute inset-0 animate-spin rounded-full border-4 border-primary/15 border-t-primary" />
        <span className="font-heading text-xl font-extrabold text-primary">R</span>
      </div>
      <p className="text-sm font-medium text-muted-foreground">{label}</p>
    </div>
  );
}
