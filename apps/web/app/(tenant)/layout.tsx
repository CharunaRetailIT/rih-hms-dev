'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { apiClient, setMoneyCurrency } from '@/lib/api-client';
import { Sidebar } from '@/components/app-shell/Sidebar';
import { PageLoader } from '@/components/ui/PageLoader';
import { AppFooter } from '@/components/app-shell/AppFooter';
import { TrialBanner } from '@/components/app-shell/TrialBanner';

export default function TenantLayout({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const [ready, setReady] = useState(false);

  useEffect(() => {
    if (!localStorage.getItem('hms.token')) {
      router.replace('/login');
      return;
    }
    // Set the base currency once so money() prefixes the right code everywhere (LKR, AED…).
    apiClient<{ baseCurrency?: string }>('/api/v1/settings')
      .then(s => setMoneyCurrency(s.baseCurrency))
      .catch(() => { /* keep default */ });
    setReady(true);
  }, [router]);

  if (!ready) {
    return (
      <div className="flex h-screen items-center justify-center">
        <PageLoader />
      </div>
    );
  }

  return (
    <div className="flex h-screen overflow-hidden">
      <Sidebar />
      <div className="flex flex-1 flex-col overflow-y-auto bg-background">
        <TrialBanner />
        <div className="flex-1">{children}</div>
        <AppFooter />
      </div>
    </div>
  );
}
