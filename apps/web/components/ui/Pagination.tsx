'use client';

import { useEffect, useMemo, useState } from 'react';
import { ChevronLeft, ChevronRight } from 'lucide-react';

/** Default rows-per-page across every data grid in the app. */
export const PAGE_SIZE = 25;

/**
 * Client-side pagination for a list. Shows `pageSize` rows (default 25) and
 * exposes the current slice + controls. `resetKey` (e.g. a search string or
 * filter) snaps back to page 1 whenever it changes.
 */
export function usePagination<T>(items: T[], pageSize: number = PAGE_SIZE, resetKey?: unknown) {
  const [page, setPage] = useState(1);
  const total = items.length;
  const totalPages = Math.max(1, Math.ceil(total / pageSize));
  const pageSafe = Math.min(page, totalPages);

  // Snap to page 1 when the underlying filter/search changes.
  useEffect(() => { setPage(1); }, [resetKey]);
  // Keep the page in range if the list shrinks.
  useEffect(() => { if (page > totalPages) setPage(totalPages); }, [page, totalPages]);

  const pageItems = useMemo(
    () => items.slice((pageSafe - 1) * pageSize, pageSafe * pageSize),
    [items, pageSafe, pageSize],
  );

  return {
    page: pageSafe,
    setPage,
    pageItems,
    totalPages,
    total,
    pageSize,
    from: total === 0 ? 0 : (pageSafe - 1) * pageSize + 1,
    to: Math.min(pageSafe * pageSize, total),
  };
}

/** Standard "1–25 of 140 · Page 1 / 6" footer with prev/next. Always shows the Page/Prev/Next controls (disabled at the ends), even for a single page — matches the reference list-page layout across the app. */
export function Pagination({
  page,
  totalPages,
  total,
  from,
  to,
  setPage,
  noun = 'rows',
  className = '',
}: {
  page: number;
  totalPages: number;
  total: number;
  from: number;
  to: number;
  setPage: (updater: number | ((p: number) => number)) => void;
  noun?: string;
  className?: string;
}) {
  if (total === 0) return null;
  return (
    <div className={`mt-3 flex flex-wrap items-center justify-between gap-3 text-xs text-muted-foreground ${className}`}>
      <span className="tabular-nums">
        {from}–{to} of {total} {noun}
      </span>
      <div className="flex items-center gap-1">
        <button
          disabled={page <= 1}
          onClick={() => setPage(p => Math.max(1, (typeof p === 'number' ? p : page) - 1))}
          className="rounded-md border border-border p-1 hover:bg-muted disabled:opacity-40"
          aria-label="Previous page"
        >
          <ChevronLeft className="size-4" />
        </button>
        <span className="px-2 py-1 tabular-nums">Page {page} / {totalPages}</span>
        <button
          disabled={page >= totalPages}
          onClick={() => setPage(p => Math.min(totalPages, (typeof p === 'number' ? p : page) + 1))}
          className="rounded-md border border-border p-1 hover:bg-muted disabled:opacity-40"
          aria-label="Next page"
        >
          <ChevronRight className="size-4" />
        </button>
      </div>
    </div>
  );
}
