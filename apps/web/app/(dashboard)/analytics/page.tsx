import Topbar from '@/components/layout/Topbar';
import StatCard from '@/components/ui/StatCard';
import Panel from '@/components/ui/Panel';

const revPlatforms = [
  { icon: 'ti-brand-tiktok',    style: 'plat-tiktok',    name: 'TikTok',    handle: '@worldnewsnow',   rev: '$312' },
  { icon: 'ti-brand-instagram', style: 'plat-instagram', name: 'Instagram', handle: '@reelsnews',      rev: '$225' },
  { icon: 'ti-brand-twitter',   style: 'plat-twitter',   name: 'X / Twitter', handle: '@breakingnews_x', rev: '$198' },
  { icon: 'ti-brand-youtube',   style: 'plat-youtube',   name: 'YouTube',   handle: '@YTNewsHub',      rev: '$112' },
];

const chartBars = [45, 52, 38, 65, 55, 80, 72, 90, 75, 95];

export default function AnalyticsPage() {
  return (
    <>
      <Topbar title="Analytics">
        <button className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px]">
          <i className="ti ti-calendar" /> Last 30 days
        </button>
        <button className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px]">
          <i className="ti ti-download" /> Export
        </button>
      </Topbar>

      <div className="flex-1 overflow-y-auto p-[18px_20px] flex flex-col gap-[14px]">
        <div className="grid grid-cols-4 gap-[10px]">
          <StatCard label="Total views"     value="4.2M" subHighlight="+18%" sub="vs last month"  color="var(--accent)" />
          <StatCard label="Engagement rate" value="6.4%" sub="Above industry avg"                  color="var(--accent2)" />
          <StatCard label="Revenue (month)" value="$847" subHighlight="+$120" sub="vs last month" color="var(--yellow)" />
          <StatCard label="Top platform"    value="TikTok" subHighlight="847K" sub="followers"    color="var(--purple)" />
        </div>

        <div className="grid gap-[14px]" style={{ gridTemplateColumns: '1fr 300px' }}>
          {/* Chart panel */}
          <Panel title="Views over time (30 days)" icon="ti-chart-line">
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
          </Panel>

          {/* Revenue by platform */}
          <Panel title="Revenue by platform" icon="ti-users">
            <div className="flex flex-col divide-y divide-border -mt-[6px] -mb-[6px]">
              {revPlatforms.map((p) => (
                <div key={p.name} className="flex items-center gap-[10px] py-[10px]">
                  <div className={`w-[30px] h-[30px] rounded-[7px] flex items-center justify-center flex-shrink-0 ${p.style}`}>
                    <i className={`ti ${p.icon} text-white text-[13px]`} />
                  </div>
                  <div className="flex-1">
                    <div className="text-[11px] font-[500]">{p.name}</div>
                    <div className="text-[9px] text-text3 font-mono">{p.handle}</div>
                  </div>
                  <div className="font-display text-[13px] font-[700] text-accent">{p.rev}</div>
                </div>
              ))}
            </div>
          </Panel>
        </div>
      </div>
    </>
  );
}
