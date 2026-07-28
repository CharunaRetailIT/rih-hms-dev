import { PageLoader } from '@/components/ui/PageLoader';

/** App-wide route fallback (e.g. /login, top-level segments). */
export default function Loading() {
  return <PageLoader className="min-h-screen" />;
}
