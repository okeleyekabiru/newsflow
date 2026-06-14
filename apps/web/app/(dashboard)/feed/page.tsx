'use client';

import { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import Topbar from '@/components/layout/Topbar';
import Panel from '@/components/ui/Panel';
import Badge from '@/components/ui/Badge';
import Link from 'next/link';
import { api, type FeedItem } from '@/lib/api';

const CATS = [
  { label: 'Politics',    icon: 'ti-world',              color: 'var(--accent2)', category: 'politics' },
  { label: 'Finance',     icon: 'ti-trending-up',        color: 'var(--accent)',  category: 'finance' },
  { label: 'Technology',  icon: 'ti-cpu',                color: 'var(--purple)',  category: 'technology' },
  { label: 'Sports',      icon: 'ti-ball-football',      color: 'var(--yellow)',  category: 'sports' },
  { label: 'Health',      icon: 'ti-heart-rate-monitor', color: '#22d3ee',        category: 'health' },
  { label: 'Weather',     icon: 'ti-cloud-storm',        color: 'var(--text3)',   category: 'weather' },
  { label: 'Science',     icon: 'ti-planet',             color: 'var(--text3)',   category: 'science' },
];

const CAT_ICON: Record<string, { icon: string; color: string; bg: string }> = {
  politics:   { icon: 'ti-world',          color: 'var(--accent2)', bg: 'rgba(0,136,255,.12)' },
  finance:    { icon: 'ti-trending-up',    color: 'var(--accent)',  bg: 'rgba(0,229,160,.10)' },
  technology: { icon: 'ti-cpu',            color: 'var(--purple)',  bg: 'rgba(168,85,247,.10)' },
  sports:     { icon: 'ti-ball-football',  color: 'var(--yellow)',  bg: 'rgba(255,183,0,.10)' },
  conflict:   { icon: 'ti-alert-triangle', color: 'var(--yellow)',  bg: 'rgba(255,183,0,.10)' },
  blocked:    { icon: 'ti-shield-x',       color: 'var(--red)',     bg: 'rgba(255,69,96,.10)' },
};

function itemIcon(item: FeedItem) {
  return CAT_ICON[item.category?.toLowerCase()] ?? { icon: 'ti-news', color: 'var(--text2)', bg: 'rgba(255,255,255,.05)' };
}

function ItemBadges({ item }: { item: FeedItem }) {
  const badges = [];
  const s = item.status?.toLowerCase();
  if (s === 'draft')     badges.push(<Badge key="d" variant="review" icon="ti-pencil">Draft</Badge>);
  if (s === 'published') badges.push(<Badge key="p" variant="auto" icon="ti-check">Published</Badge>);
  if (s === 'archived')  badges.push(<Badge key="a" variant="blocked" icon="ti-archive">Archived</Badge>);
  return <>{badges}</>;
}

function relativeTime(iso: string) {
  const diff = Date.now() - new Date(iso).getTime();
  const m = Math.floor(diff / 60000);
  if (m < 1) return 'just now';
  if (m < 60) return `${m}m ago`;
  return `${Math.floor(m / 60)}h ago`;
}

export default function FeedPage() {
  const router = useRouter();
  const [items, setItems]             = useState<FeedItem[]>([]);
  const [loading, setLoading]         = useState(true);
  const [error, setError]             = useState('');
  const [activeCategory, setActive]   = useState<string | null>(null);
  const [posting, setPosting]         = useState<Set<string>>(new Set());
  const [toast, setToast]             = useState('');

  const load = useCallback((category?: string) => {
    setLoading(true);
    setError('');
    api.getFeed(category ? { category } : undefined)
      .then(setItems)
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => { load(); }, [load]);

  function toggleCategory(cat: string) {
    const next = activeCategory === cat ? null : cat;
    setActive(next);
    load(next ?? undefined);
  }

  function showToast(msg: string) {
    setToast(msg);
    setTimeout(() => setToast(''), 3000);
  }

  async function postDraft(id: string) {
    setPosting((prev) => { const n = new Set(prev); n.add(id); return n; });
    try {
      await api.publishArticle(id, []);
      setItems((prev) => prev.map((i) => i.id === id ? { ...i, status: 'published' } : i));
      showToast('Article published successfully.');
    } catch (e) {
      showToast(e instanceof Error ? e.message : 'Post failed');
    } finally {
      setPosting((prev) => { const n = new Set(prev); n.delete(id); return n; });
    }
  }

  const hasReview = items.some((i) => i.status?.toLowerCase() === 'draft');

  const filterBar = (
    <div className="flex gap-[6px]">
      <button
        onClick={() => { setActive(null); load(); }}
        className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px]">
        <i className="ti ti-filter" /> Filter
      </button>
      <button
        onClick={() => load(activeCategory ?? undefined)}
        className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px]">
        <i className="ti ti-refresh" /> Refresh
      </button>
    </div>
  );

  return (
    <>
      {toast && (
        <div className="fixed bottom-4 right-4 z-50 px-4 py-2 rounded-[8px] text-[12px] font-[500]"
          style={{ background: 'var(--bg3)', border: '1px solid var(--border)', color: 'var(--text)' }}>
          {toast}
        </div>
      )}

      <Topbar title="Live news feed" live>
        {filterBar}
      </Topbar>

      <div className="flex-1 overflow-y-auto p-[18px_20px] flex flex-col gap-[14px]">
        <Panel title="Live news feed" icon="ti-rss" action={filterBar}>
          {/* Category chips */}
          <div className="flex flex-wrap gap-[6px] mb-[10px]">
            {CATS.map((c) => {
              const active = activeCategory === c.category;
              return (
                <button
                  key={c.label}
                  onClick={() => toggleCategory(c.category)}
                  className="flex items-center gap-[5px] px-[10px] py-[5px] rounded-btn border text-[11px] font-[500] cursor-pointer"
                  style={active
                    ? { borderColor: c.color, background: 'rgba(0,229,160,.07)', color: c.color }
                    : { borderColor: 'var(--border)', background: 'var(--bg3)', color: 'var(--text)' }}>
                  <i className={`ti ${c.icon}`} /> {c.label}
                </button>
              );
            })}
          </div>

          {loading && <div className="text-text3 text-[12px] font-mono py-4">Loading feed…</div>}
          {error && (
            <div className="text-[12px] p-[10px] rounded-[8px]"
              style={{ background: 'rgba(255,69,96,.1)', border: '1px solid rgba(255,69,96,.3)', color: 'var(--red)' }}>
              {error}
            </div>
          )}
          {!loading && !error && items.length === 0 && (
            <div className="text-text3 text-[12px] font-mono py-4">No feed items found.</div>
          )}

          <div className="flex flex-col divide-y divide-border">
            {items.map((item) => {
              const ic = itemIcon(item);
              const isDraft = item.status?.toLowerCase() === 'draft';
              const isBusy = posting.has(item.id);

              return (
                <div
                  key={item.id}
                  className="flex gap-[10px] py-[10px] first:pt-0 last:pb-0"
                >
                  <div className="w-8 h-8 rounded-[7px] flex items-center justify-center flex-shrink-0 mt-[2px]"
                    style={{ background: ic.bg }}>
                    <i className={`ti ${ic.icon} text-[14px]`} style={{ color: ic.color }} />
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="text-[12px] font-[500] leading-[1.4] mb-[3px] text-text">{item.title}</div>
                    <div className="flex items-center gap-[6px] text-[10px] text-text3 font-mono flex-wrap mb-[2px]">
                      <span>{item.sourceName ?? item.category}</span><span>·</span>
                      <span>{relativeTime(item.updatedAt)}</span>
                      <ItemBadges item={item} />
                    </div>

                    {/* Draft action buttons */}
                    {isDraft && (
                      <div className="flex gap-[5px] mt-[6px]">
                        <button
                          onClick={() => router.push(`/writeup?id=${item.id}`)}
                          className="flex items-center gap-[4px] text-[10px] px-[8px] py-[3px] rounded-[5px] cursor-pointer"
                          style={{ background: 'var(--bg2)', color: 'var(--text2)', border: '1px solid var(--border)' }}
                        >
                          <i className="ti ti-pencil text-[10px]" /> Edit
                        </button>
                        <button
                          onClick={() => postDraft(item.id)}
                          disabled={isBusy}
                          className="flex items-center gap-[4px] text-[10px] px-[8px] py-[3px] rounded-[5px] cursor-pointer disabled:opacity-50"
                          style={{ background: 'rgba(0,229,160,.12)', color: 'var(--accent)', border: '1px solid rgba(0,229,160,.3)' }}
                        >
                          <i className="ti ti-send text-[10px]" /> {isBusy ? 'Posting…' : 'Post directly'}
                        </button>
                      </div>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        </Panel>

        {hasReview && (
          <div className="flex gap-[10px] items-center p-[10px_14px] rounded-[9px] text-[11px] text-text2"
            style={{ background: 'rgba(255,183,0,.06)', border: '1px solid rgba(255,183,0,.2)' }}>
            <i className="ti ti-alert-triangle text-yellow text-[15px] flex-shrink-0" />
            Some articles require manual review.{' '}
            <Link href="/review" className="text-yellow cursor-pointer">Go to review queue →</Link>
          </div>
        )}
      </div>
    </>
  );
}
