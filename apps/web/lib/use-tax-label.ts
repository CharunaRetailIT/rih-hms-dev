'use client';

import { useEffect, useState } from 'react';

// The consumption tax goes by different names per country (VAT / GST / Sales Tax).
// The Sidebar caches the tenant's label in localStorage when it loads /pos/config,
// so any client page can read it synchronously without its own fetch. Falls back to
// the neutral "Tax" until the cached value is available.
export function useTaxLabel(): string {
  const [label, setLabel] = useState('Tax');
  useEffect(() => {
    try {
      const l = localStorage.getItem('hms.taxLabel');
      if (l) setLabel(l);
    } catch { /* ignore */ }
  }, []);
  return label;
}
