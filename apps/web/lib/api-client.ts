'use client';

/**
 * Browser-side API client. Attaches the tenant JWT (and, in dev, the
 * X-Tenant-Id header the API also accepts) to every request. Requests are
 * proxied to the .NET API via the Next.js rewrite in next.config.mjs.
 */
export async function apiClient<T = unknown>(path: string, init?: RequestInit, _retried = false): Promise<T> {
  const token = typeof window !== 'undefined' ? localStorage.getItem('hms.token') : null;
  const tenantRaw = typeof window !== 'undefined' ? localStorage.getItem('hms.tenant') : null;
  const tenantId = tenantRaw ? JSON.parse(tenantRaw).id : null;

  // FormData bodies (file uploads) need the browser to set their own
  // multipart boundary — a hardcoded application/json header would break it.
  const isFormData = typeof FormData !== 'undefined' && init?.body instanceof FormData;

  const headers: Record<string, string> = {
    ...(isFormData ? {} : { 'Content-Type': 'application/json' }),
    ...(init?.headers as Record<string, string> | undefined),
  };
  if (token) headers['Authorization'] = `Bearer ${token}`;
  if (tenantId) headers['X-Tenant-Id'] = tenantId; // dev convenience; JWT is source of truth

  const res = await fetch(path, { ...init, headers, cache: 'no-store' });

  // Access token expired? Silently refresh once, then replay the request.
  if (res.status === 401 && !_retried && path !== '/api/v1/auth/refresh' && typeof window !== 'undefined') {
    if (await tryRefresh()) return apiClient<T>(path, init, true);
  }

  if (!res.ok) {
    const body = await res.text();
    // Prefer the server's own message (usually meaningful, e.g. "PIN must be 4–8 digits"),
    // else a plain-English message per status — users never see raw 400/401/500 codes.
    let serverMsg = '';
    try { const j = JSON.parse(body); if (typeof j?.error === 'string') serverMsg = j.error; } catch { /* not JSON */ }
    const err = new Error(serverMsg || friendlyStatus(res.status)) as Error & { status?: number; body?: string };
    err.status = res.status; err.body = body;
    throw err;
  }
  return res.status === 204 ? (undefined as T) : ((await res.json()) as T);
}

/** Plain-English fallback for an HTTP status when the server didn't supply a message. */
function friendlyStatus(status: number): string {
  switch (status) {
    case 400: return 'Some details weren’t quite right — please check and try again.';
    case 401: return 'Your session has expired. Please sign in again.';
    case 403: return 'You don’t have permission to do that.';
    case 404: return 'We couldn’t find what you were looking for.';
    case 409: return 'That conflicts with something that already exists.';
    case 422: return 'Some details weren’t valid — please review and try again.';
    case 423: return 'Locked — too many attempts. Please wait a moment and try again.';
    case 429: return 'Too many requests — please slow down and try again shortly.';
    default:  return status >= 500
      ? 'Something went wrong on our side. Please try again in a moment.'
      : 'Sorry, that didn’t work. Please try again.';
  }
}

let refreshInFlight: Promise<boolean> | null = null;

/** Exchange the stored refresh token for a fresh access token. De-duped across concurrent 401s. */
async function tryRefresh(): Promise<boolean> {
  const refresh = localStorage.getItem('hms.refresh');
  if (!refresh) { redirectToLogin(); return false; }
  refreshInFlight ??= (async () => {
    try {
      const res = await fetch('/api/v1/auth/refresh', {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken: refresh }), cache: 'no-store',
      });
      if (!res.ok) { clearSession(); redirectToLogin(); return false; }
      const data = await res.json();
      localStorage.setItem('hms.token', data.accessToken);
      if (data.refreshToken) localStorage.setItem('hms.refresh', data.refreshToken);
      return true;
    } catch { return false; }
  })();
  try { return await refreshInFlight; } finally { refreshInFlight = null; }
}

function clearSession() {
  ['hms.token', 'hms.refresh', 'hms.tenant', 'hms.user'].forEach(k => localStorage.removeItem(k));
}
function redirectToLogin() {
  if (typeof window !== 'undefined' && !window.location.pathname.startsWith('/login')) {
    window.location.href = '/login';
  }
}

/** Bare number — thousand separators + 2 decimals, no currency code. */
export function lkr(amount: number): string {
  return new Intl.NumberFormat('en-LK', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount || 0);
}

/** The tenant's base-currency code for money display. Set once on app load from
 *  /api/v1/settings (defaults to LKR until then). */
let _moneyCcy = 'LKR';
export function setMoneyCurrency(code?: string | null) { if (code && code.trim()) _moneyCcy = code.trim().toUpperCase(); }
export function moneyCurrency(): string { return _moneyCcy; }

/** Money formatter for data grids/labels — base currency code + thousands + 2
 *  decimals, e.g. "LKR 1,000.00". Use this anywhere a base-currency amount is shown
 *  to the user. (POS keeps using lkr() because it manages its own multi-currency display.) */
export function money(amount: number): string {
  return `${_moneyCcy} ${lkr(amount)}`;
}

export type ProductListItem = {
  id: string;
  sku: string;
  name: string;
  barcode: string | null;
  departmentId: string | null;
  categoryId: string | null;
  unitOfMeasureId: string;
  basePrice: number;
  costPrice: number;
  isActive: boolean;
  isSold: boolean;
  isPurchased: boolean;
  isStocked: boolean;
  productType: string;
  kitchenStationCode: string | null;
  variantCount: number;
};

type PagedResult<T> = { data: T[]; pagination: { totalPages: number } };

/**
 * The catalog dropdowns/pickers across the app need the full product list, but
 * the API only exposes it paginated (max pageSize 100) — this pages through
 * every page and flattens the result.
 */
export async function fetchAllProducts(): Promise<ProductListItem[]> {
  const pageSize = 100;
  const first = await apiClient<PagedResult<ProductListItem>>(
    `/api/v1/products/paged?pageNumber=1&pageSize=${pageSize}`,
  );
  const items = [...first.data];
  for (let page = 2; page <= first.pagination.totalPages; page++) {
    const next = await apiClient<PagedResult<ProductListItem>>(
      `/api/v1/products/paged?pageNumber=${page}&pageSize=${pageSize}`,
    );
    items.push(...next.data);
  }
  return items;
}
