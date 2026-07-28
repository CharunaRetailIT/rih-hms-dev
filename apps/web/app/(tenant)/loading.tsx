import { PageLoader } from '@/components/ui/PageLoader';

/** Route-level fallback shown while a tenant page segment loads (navigation / code-split). */
export default function Loading() {
  return <PageLoader className="min-h-[80vh]" />;
}
