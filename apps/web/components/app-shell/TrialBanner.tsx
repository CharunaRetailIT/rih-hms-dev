'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { apiClient } from '@/lib/api-client';
import { Icon } from '@/components/ui/Icon';

type Trial = { status: string; trialEndsAt: string | null; daysRemaining: number | null };

/** Persistent banner shown across the app while the workspace is on its free trial. */
export function TrialBanner() {
  const [t, setT] = useState<Trial | null>(null);
  useEffect(() => { apiClient<Trial>('/api/v1/billing/trial').then(setT).catch(() => {}); }, []);

  if (!t || t.status !== 'trialing') return null;
  const days = t.daysRemaining ?? 0;
  const urgent = days <= 3;

  return (
    <div className={`flex flex-wrap items-center justify-center gap-x-2 gap-y-1 px-4 py-2 text-sm font-medium ${urgent ? 'bg-status-error/10 text-status-error' : 'bg-primary-tint text-primary-dark'}`}>
      <Icon name="schedule" className="text-[18px]" />
      <span>Trial mode — <strong>{days === 0 ? 'ends today' : `${days} day${days === 1 ? '' : 's'} left`}</strong>.</span>
      <Link href="/settings?billing=1" className="font-bold underline underline-offset-2 hover:opacity-80">Add a payment method</Link>
      <span className="hidden text-muted-foreground sm:inline">to keep your service after the trial.</span>
    </div>
  );
}
