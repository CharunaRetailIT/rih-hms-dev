import { Mail, Phone } from 'lucide-react';

/** Retail IT wordmark — green "RETAIL", brand-yellow "IT". */
export function RetailItMark({ className = '' }: { className?: string }) {
  return (
    <span className={`font-heading font-extrabold tracking-tight ${className}`}>
      <span className="text-primary">RETAIL</span>{' '}
      <span className="text-[#ffc329]">IT</span>
    </span>
  );
}

// NOTE: support email + phone are placeholders — confirm the real Retail IT support details.
const SUPPORT_EMAIL = 'support@retailit.lk';
const SUPPORT_PHONE_DISPLAY = '+94 11 522 4400';
const SUPPORT_PHONE_TEL = '+94115224400';

/** App-wide footer: brand mark, support contact, copyright. */
export function AppFooter() {
  const year = new Date().getFullYear();
  return (
    <footer className="mt-10 border-t border-border bg-card">
      <div className="mx-auto flex max-w-7xl flex-col items-center justify-between gap-3 px-6 py-5 text-xs text-muted-foreground md:flex-row">
        <div className="flex items-center gap-2">
          <RetailItMark className="text-sm" />
          <span className="hidden text-muted-foreground/80 sm:inline">· we provide more than a solution</span>
        </div>

        <div className="flex flex-wrap items-center justify-center gap-x-5 gap-y-1">
          <span className="font-medium text-on-surface">Need help?</span>
          <a href={`mailto:${SUPPORT_EMAIL}`} className="flex items-center gap-1.5 transition-colors hover:text-primary">
            <Mail className="size-3.5" /> {SUPPORT_EMAIL}
          </a>
          <a href={`tel:${SUPPORT_PHONE_TEL}`} className="flex items-center gap-1.5 transition-colors hover:text-primary">
            <Phone className="size-3.5" /> {SUPPORT_PHONE_DISPLAY}
          </a>
        </div>

        <div className="text-center md:text-right">
          © {year} Retail IT (Pvt) Ltd. All rights reserved.
          <span className="ml-1">Powered by <RetailItMark className="text-[11px]" />.</span>
        </div>
      </div>
    </footer>
  );
}
