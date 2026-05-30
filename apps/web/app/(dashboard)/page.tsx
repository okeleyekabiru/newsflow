'use client';

import { useState, useEffect, useCallback } from 'react';
import Topbar from '@/components/layout/Topbar';
import StatCard from '@/components/ui/StatCard';
import Panel from '@/components/ui/Panel';
import Badge from '@/components/ui/Badge';
import Link from 'next/link';
import { api, type OverviewStats, type FeedItem, type Account } from '@/lib/api';

function relativeTime(iso: string) {
  const diff = Date.now() - new Date(iso).getTime();
  const m = Math.floor(diff / 60000);
  if (m < 1) return 'just now';
  if (m < 60) return `${m}m ago`;
  return `${Math.floor(m / 60)}h ago`;
}

const PLAT_META: Record<string, { icon: string; style: string }> = {
  tiktok:    { icon: 'ti-brand-tiktok',    style: 'plat-tiktok' },
  twitter:   { icon: 'ti-brand-twitter',   style: 'plat-twitter' },
  instagram: { icon: 'ti-brand-instagram', style: 'plat-instagram' },
  youtube:   { icon: 'ti-brand-youtube',   style: 'plat-youtube' },
};

const CAT_META: Record<string, { icon: string; color: string; bg: string }> = {
  politics:   { icon: 'ti-world',          color: 'var(--accent2)', bg: 'rgba(0,136,255,.12)' },
  finance:    { icon: 'ti-trending-up',    color: 'var(--accent)',  bg: 'rgba(0,229,160,.10)' },
  technology: { icon: 'ti-cpu',            color: 'var(--purple)',  bg: 'rgba(168,85,247,.10)' },
  conflict:   { icon: 'ti-alert-triangle', color: 'var(--yellow)',  bg: 'rgba(255,183,0,.10)' },
};

function feedIcon(item: FeedItem) {
  return CAT_META[item.category.toLowerCase()] ?? { icon: 'ti-news', color: 'var(--text2)', bg: 'rgba(255,255,255,.05)' };
}

function fmtNum(n: number) {
  if (n >= 1_000_000) return `${(n / 1_000_000).toFixed(1)}M`;
  if (n >= 1_000)     return `${(n / 1_000).toFixed(0)}K`;
  return String(n);
}

export default function DashboardPage() {
  const [stats, setStats]       = useState<OverviewStats | null>(null);
  const [feed, setFeed]         = useState<FeedItem[]>([]);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading]   = useState(true);

  const load = useCallback(() => {
    setLoading(true);
    Promise.all([
      api.getOverview().catch(() => null),
      api.getFeed({ perPage: 4 }).catch(() => [] as FeedItem[]),
      api.getAccounts().catch(() => [] as Account[]),
    ]).then(([s, f, a]) => {
      if (s) setStats(s);
      setFeed(f as FeedItem[]);
      setAccounts(a as Account[]);
    }).finally(() => setLoading(false));
  }, []);

  useEffect(() => { load(); }, [load]);

  const weekData = [42, 58, 50, 78, 65, 82, 52];
  const weekDays = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

  return (
    <>
      <Topbar title="Dashboard" live>
        <button
          onClick={load}
          disabled={loading}
          className="btn-ghost flex items-center gap-[5px] px-[11px] py-[6px] rounded-btn border border-border text-text2 text-[11px] font-[500] disabled:opacity-50">
          <i className={`ti ${loading ? 'ti-loader-2' : 'ti-refresh'} text-[12px]`} /> Refresh
        </button>
      </Topbar>

      <div className="flex-1 overflow-y-auto p-[18px_20px] flex flex-col gap-[14px]">
        {/* Stats row */}
        <div className="grid grid-cols-4 gap-[10px]">
          <StatCard
            label="Posts today"
            value={stats ? String(stats.postsToday) : '—'}
            subHighlight={accounts.length ? `${accounts.length} accounts` : undefined}
            sub="across"
            color="var(--accent)"
          />
          <StatCard
            label="Total followers"
            value={stats ? fmtNum(stats.totalFollowers) : '—'}
            subHighlight={stats ? `+${fmtNum(stats.followersGainedThisWeek)}` : undefined}
            sub="this week"
            color="var(--accent2)"
          />
          <StatCard
            label="Est. revenue"
            value={stats ? `$${stats.estimatedRevenue.toLocaleString()}` : '—'}
            sub="This month so far"
            color="var(--yellow)"
          />
          <StatCard
            label="AI videos made"
            value={stats ? String(stats.aiVideosMade) : '—'}
            subHighlight={stats?.aiVideosProcessing ? String(stats.aiVideosProcessing) : undefined}
            sub="processing"
            color="var(--purple)"
          />
        </div>

        {/* Two-column body */}
        <div className="grid gap-[14px]" style={{ gridTemplateColumns: '1fr 300px' }}>
          <div className="flex flex-col gap-[14px]">
            {/* Live feed preview */}
            <Panel
              title="Live news feed"
              icon="ti-rss"
              action={
                <Link href="/feed" className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px] font-[500]">
                  <i className="ti ti-arrow-right" /> View all
                </Link>
              }>
              {loading && <div className="text-text3 text-[12px] font-mono py-2">Loading…</div>}
              <div className="flex flex-col divide-y divide-border">
                {feed.map((item) => {
                  const ic = feedIcon(item);
                  return (
                    <div key={item.id} className="flex gap-[10px] py-[10px] first:pt-0 last:pb-0">
                      <div className="w-8 h-8 rounded-[7px] flex items-center justify-center flex-shrink-0"
                        style={{ background: ic.bg }}>
                        <i className={`ti ${ic.icon} text-[14px]`} style={{ color: ic.color }} />
                      </div>
                      <div>
                        <div className="text-[12px] font-[500] leading-[1.4] mb-[3px]">{item.title}</div>
                        <div className="flex items-center gap-[6px] text-[10px] text-text3 font-mono flex-wrap">
                          <span>{item.source}</span><span>·</span>
                          <span>{relativeTime(item.publishedAt)}</span>
                          {item.status === 'auto' && <Badge variant="auto" icon="ti-bolt">Auto-post</Badge>}
                          {item.status === 'review' && <Badge variant="review" icon="ti-eye">Review required</Badge>}
                          {item.hasVideo && <Badge variant="video" icon="ti-player-play">Video</Badge>}
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            </Panel>

            {/* Recent write-ups */}
            <Panel title="Recent write-ups" icon="ti-pencil" action={
              <Link href="/writeup" className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px] font-[500]">
                <i className="ti ti-plus" /> New article
              </Link>
            }>
              <div className="text-text3 text-[12px] font-mono py-1">
                <Link href="/writeup" className="text-accent hover:underline">Open write-up studio →</Link>
              </div>
            </Panel>
          </div>

          {/* Right column */}
          <div className="flex flex-col gap-[14px]">
            {/* Connected accounts */}
            <Panel title="Connected accounts" icon="ti-users">
              {loading && <div className="text-text3 text-[12px] font-mono py-2">Loading…</div>}
              <div className="flex flex-col divide-y divide-border -mt-[6px] -mb-[6px]">
                {accounts.map((a) => {
                  const meta = PLAT_META[a.platform] ?? { icon: 'ti-world', style: '' };
                  return (
                    <div key={a.id} className="flex items-center gap-[10px] py-[10px]">
                      <div className={`w-[30px] h-[30px] rounded-[7px] flex items-center justify-center flex-shrink-0 ${meta.style}`}>
                        <i className={`ti ${meta.icon} text-white text-[14px]`} />
                      </div>
                      <div className="flex-1">
                        <div className="font-display text-[16px] font-[700]">{a.followers}</div>
                        <div className="text-[10px] text-text3 font-mono">{a.handle}</div>
                        <div className="text-[11px] font-[600] text-accent font-mono">{a.estimatedRevenue}</div>
                      </div>
                      <div className="w-7 h-4 rounded-lg flex-shrink-0"
                        style={{ background: a.active ? 'var(--accent)' : 'var(--border2)', position: 'relative' }}>
                        <div className="absolute w-[10px] h-[10px] rounded-full bg-black top-[3px] right-[3px]" />
                      </div>
                    </div>
                  );
                })}
                {!loading && accounts.length === 0 && (
                  <div className="text-text3 text-[11px] font-mono py-2">
                    <Link href="/accounts" className="text-accent hover:underline">Connect your first account →</Link>
                  </div>
                )}
              </div>
            </Panel>

            {/* Posts this week */}
            <Panel title="Posts this week" icon="ti-chart-bar">
              <div className="flex items-end gap-[3px] h-[50px]">
                {weekData.map((h, i) => (
                  <div
                    key={i}
                    className="flex-1 rounded-t-[3px]"
                    style={{
                      height: `${h}%`,
                      background: i < 6 ? 'rgba(0,229,160,.3)' : 'rgba(0,136,255,.12)',
                      borderTop: i < 6 ? '2px solid var(--accent)' : '2px dashed var(--accent2)',
                    }}
                  />
                ))}
              </div>
              <div className="flex justify-between mt-[3px] text-[9px] text-text3 font-mono">
                {weekDays.map((d, i) => (
                  <span key={d} style={i === 6 ? { color: 'var(--accent2)' } : {}}>{d}</span>
                ))}
              </div>
            </Panel>
          </div>
        </div>
      </div>
    </>
  );
}
