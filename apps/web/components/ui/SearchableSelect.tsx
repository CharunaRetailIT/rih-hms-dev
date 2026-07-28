'use client';

import { useEffect, useRef, useState } from 'react';

export type SearchableOption = { id: string; label: string; data?: unknown };

type FetchPageResult = { items: SearchableOption[]; hasMore: boolean };
type FetchPage = (args: { page: number; pageSize: number; search: string }) => Promise<FetchPageResult>;

type Props = {
  value: string;
  onChange: (id: string, option?: SearchableOption) => void;
  placeholder?: string;
  disabled?: boolean;
  className?: string;
} & (
  | { options: SearchableOption[]; fetchPage?: undefined; selectedLabel?: undefined; pageSize?: undefined }
  | { options?: undefined; fetchPage: FetchPage; selectedLabel?: string; pageSize?: number }
);

/**
 * A text-input-driven dropdown for long option lists (products, suppliers,
 * locations). Native <select> truncates/overflows badly with long labels and
 * has no search — this filters as you type.
 *
 * Two modes:
 *  - `options`: a preloaded array, filtered client-side (small lists).
 *  - `fetchPage`: server-driven — loads a page on open, debounced-reloads on
 *    search, and loads the next page on scroll-to-bottom (for large catalogs).
 */
export function SearchableSelect(props: Props) {
  const { value, onChange, placeholder = 'Select…', disabled = false, className = '' } = props;
  const isServer = 'fetchPage' in props && !!props.fetchPage;
  const pageSize = (isServer && props.pageSize) || 20;

  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState('');
  const rootRef = useRef<HTMLDivElement>(null);
  const listRef = useRef<HTMLDivElement>(null);

  const [items, setItems] = useState<SearchableOption[]>([]);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(false);
  const [loading, setLoading] = useState(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    function onDocClick(e: MouseEvent) {
      if (rootRef.current && !rootRef.current.contains(e.target as Node)) {
        setOpen(false);
        setQuery('');
      }
    }
    document.addEventListener('mousedown', onDocClick);
    return () => document.removeEventListener('mousedown', onDocClick);
  }, []);

  async function loadServerPage(nextPage: number, search: string, append: boolean) {
    if (!isServer) return;
    setLoading(true);
    try {
      const res = await (props as Extract<Props, { fetchPage: FetchPage }>).fetchPage({
        page: nextPage,
        pageSize,
        search,
      });
      setItems(prev => (append ? [...prev, ...res.items] : res.items));
      setHasMore(res.hasMore);
      setPage(nextPage);
    } finally {
      setLoading(false);
    }
  }

  // First open (or a fresh search) loads page 1; typing while open is debounced.
  useEffect(() => {
    if (!isServer || !open) return;
    const firstLoad = items.length === 0 && query === '';
    if (debounceRef.current) clearTimeout(debounceRef.current);
    if (firstLoad) {
      loadServerPage(1, '', false);
    } else {
      debounceRef.current = setTimeout(() => loadServerPage(1, query, false), 300);
    }
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, query]);

  function onScroll() {
    if (!isServer || !hasMore || loading) return;
    const el = listRef.current;
    if (!el) return;
    if (el.scrollHeight - el.scrollTop - el.clientHeight < 40) {
      loadServerPage(page + 1, query, true);
    }
  }

  const staticOptions = !isServer ? props.options : [];
  const filtered = isServer
    ? items
    : query.trim()
      ? staticOptions.filter(o => o.label.toLowerCase().includes(query.trim().toLowerCase()))
      : staticOptions;

  const selected = (isServer ? items : staticOptions).find(o => o.id === value);
  const displayLabel = selected?.label ?? (isServer ? props.selectedLabel : undefined) ?? '';

  return (
    <div ref={rootRef} className={`relative ${className}`}>
      <input
        value={open ? query : displayLabel}
        onChange={e => {
          setQuery(e.target.value);
          if (!open) setOpen(true);
        }}
        onFocus={() => {
          setOpen(true);
          setQuery('');
        }}
        disabled={disabled}
        placeholder={placeholder}
        className="w-full truncate rounded-lg border border-border bg-surface px-3 py-2.5 text-sm focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20 disabled:opacity-60"
      />
      {open && !disabled && (
        <div
          ref={listRef}
          onScroll={onScroll}
          className="absolute z-50 mt-1 max-h-64 w-full overflow-auto rounded-lg border border-border bg-card shadow-lg"
        >
          {filtered.length === 0 && !loading && (
            <div className="px-3 py-2 text-sm text-muted-foreground">No matches.</div>
          )}
          {filtered.map(o => (
            <button
              key={o.id}
              type="button"
              onMouseDown={e => e.preventDefault()}
              onClick={() => {
                onChange(o.id, o);
                setOpen(false);
                setQuery('');
              }}
              title={o.label}
              className={`block w-full truncate px-3 py-2 text-left text-sm hover:bg-muted ${
                o.id === value ? 'bg-primary/5 font-medium' : ''
              }`}
            >
              {o.label}
            </button>
          ))}
          {loading && (
            <div className="px-3 py-2 text-center text-xs text-muted-foreground">Loading…</div>
          )}
        </div>
      )}
    </div>
  );
}
