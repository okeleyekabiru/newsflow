'use client';

import { useState, useEffect, useCallback } from 'react';
import Topbar from '@/components/layout/Topbar';
import StatCard from '@/components/ui/StatCard';
import Panel from '@/components/ui/Panel';
import { api, type OverviewStats, type Account } from '@/lib/api';

const PLAT_META: Record<string, { icon: string; style: string; label: string }> = {
  tiktok:    { icon: 'ti-brand-tiktok',    style: 'plat-tiktok',    label: 'TikTok' },
  twitter:   { icon: 'ti-brand-twitter',   style: 'plat-twitter',   label: 'X / Twitter' },
  instagram: { icon: 'ti-brand-instagram', style: 'plat-instagram', label: 'Instagram' },
  youtube:   { icon: 'ti-brand-youtube',   style: 'plat-youtube',   label: 'YouTube' },
};

function isoRange(days: number) {
  const to = new Date();
  const from = new Date(Date.now() - days * 86_400_000);
  return { from: from.toISOString().slice(0, 10), to: to.toISOString().slice(0, 10) };
}

export default function AnalyticsPage() {
  const [stats, setStats]       = useState<OverviewStats | null>(null);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading]   = useState(true);
  const [range, setRange]       = useState(30);

  const load = useCallback(() => {
    setLoading(true);
    const { from, to } = isoRange(range);
    Promise.all([
      api.getOverview().catch(() => null),
      api.getAccounts().catch(() => [] as Account[]),
      api.getRevenue(from, to).catch(() => null),
    ]).then(([s, a]) => {
      if (s) setStats(s);
      setAccounts(a as Account[]);
    }).finally(() => setLoading(false));
  }, [range]);

  useEffect(() => { load(); }, [load]);

  const chartBars = [45, 52, 38, 65, 55, 80, 72, 90, 75, 95];

  return (
    <>
      <Topbar title="Analytics">
        <button
          onClick={() => setRange((r) => (r === 30 ? 7 : r === 7 ? 90 : 30))}
          className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px]">
          <i className="ti ti-calendar" /> Last {range} days
        </button>
        <button
          onClick={() => alert('CSV export coming soon.')}
          className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px]">
          <i className="ti ti-download" /> Export
        </button>
      </Topbar>

      <div className="flex-1 overflow-y-auto p-[18px_20px] flex flex-col gap-[14px]">
        <div className="grid grid-cols-4 gap-[10px]">
          <StatCard
            label="Total views"
            value={loading ? '—' : stats ? `${(stats.totalViews / 1000).toFixed(0)}K` : '—'}
            sub="Across all platforms"
            color="var(--accent)"
          />
          <StatCard
            label="Engagement rate"
            value="—"
            sub="Coming soon"
            color="var(--accent2)"
          />
          <StatCard
            label="Revenue (period)"
            value={loading ? '—' : stats ? `$${(stats.totalRevenue ?? 0).toLocaleString()}` : '—'}
            sub={`Last ${range} days`}
            color="var(--yellow)"
          />
          <StatCard
            label="Top platform"
            value={accounts[0] ? PLAT_META[accounts[0].platform]?.label ?? accounts[0].platform : '—'}
            subHighlight={accounts[0]?.followers}
            sub="followers"
            color="var(--purple)"
          />
        </div>

        <div className="grid gap-[14px]" style={{ gridTemplateColumns: '1fr 300px' }}>
          {/* Chart */}
          <Panel title={`Views over time (${range} days)`} icon="ti-chart-line">
            {loading
              ? <div className="text-text3 text-[12px] font-mono py-4">Loading…</div>
              : (
                <div className="flex items-end gap-[4px] h-[120px]">
                  {chartBars.map((h, i) => (
                    <div
                      key={i}
                      className="flex-1 rounded-t-[3px]"
                      style={{
                        height: `${h}%`,
                        background: i === 7 ? 'rgba(0,136,255,.25)' : 'rgba(0,229,160,.25)',
                        borderTop: i === 7 ? '2px solid var(--accent2)' : '2px solid var(--accent)',
                      }}
                    />
                  ))}
                </div>
              )}
          </Panel>

          {/* Revenue by platform */}
          <Panel title="Revenue by platform" icon="ti-users">
            {loading && <div className="text-text3 text-[12px] font-mono py-2">Loading…</div>}
            <div className="flex flex-col divide-y divide-border -mt-[6px] -mb-[6px]">
              {accounts.map((a) => {
                const meta = PLAT_META[a.platform] ?? { icon: 'ti-world', style: '', label: a.platform };
                return (
                  <div key={a.id} className="flex items-center gap-[10px] py-[10px]">
                    <div className={`w-[30px] h-[30px] rounded-[7px] flex items-center justify-center flex-shrink-0 ${meta.style}`}>
                      <i className={`ti ${meta.icon} text-white text-[13px]`} />
                    </div>
                    <div className="flex-1">
                      <div className="text-[11px] font-[500]">{meta.label}</div>
                      <div className="text-[9px] text-text3 font-mono">{a.handle}</div>
                    </div>
                    <div className="font-display text-[13px] font-[700] text-accent">{a.estimatedRevenue}</div>
                  </div>
                );
              })}
              {!loading && accounts.length === 0 && (
                <div className="text-text3 text-[11px] font-mono py-2">No accounts connected.</div>
              )}
            </div>
          </Panel>
        </div>
      </div>
    </>
  );
}
