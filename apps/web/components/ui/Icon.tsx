import { cn } from '@/lib/utils';

/** Material Symbols Outlined glyph — matches the Stitch designs. */
export function Icon({ name, className }: { name: string; className?: string }) {
  return (
    <span className={cn('material-symbols-outlined leading-none', className)} aria-hidden>
      {name}
    </span>
  );
}
