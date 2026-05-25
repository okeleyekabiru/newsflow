import Topbar from '@/components/layout/Topbar';
import StatCard from '@/components/ui/StatCard';
import Panel from '@/components/ui/Panel';

const EARNINGS = [
  { platform: 'TikTok',    icon: 'ti-brand-tiktok',    style: 'plat-tiktok',    handle: '@worldnewsnow',   current: '$312', growth: '+8%',  projected: '$337' },
  { platform: 'Instagram', icon: 'ti-brand-instagram', style: 'plat-instagram', handle: '@reelsnews',      current: '$225', growth: '+14%', projected: '$257' },
  { platform: 'Twitter',   icon: 'ti-brand-twitter',   style: 'plat-twitter',   handle: '@breakingnews_x', current: '$198', growth: '+4%',  projected: '$206' },
  { platform: 'YouTube',   icon: 'ti-brand-youtube',   style: 'plat-youtube',   handle: '@YTNewsHub',      current: '$112', growth: '+22%', projected: '$137' },
];

export default function MonetizePage() {
  return (
    <>
      <Topbar title="Monetisation">
        <button className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px]">
          <i className="ti ti-download" /> Export report
        </button>
      </Topbar>

      <div className="flex-1 overflow-y-auto p-[18px_20px] flex flex-col gap-[14px]">
        <div className="grid grid-cols-4 gap-[10px]">
          <StatCard label="Total revenue"  value="$847"  subHighlight="+$120" sub="vs last month"   color="var(--accent)" />
          <StatCard label="Projected (month)" value="$937" sub="Based on current trajectory"         color="var(--accent2)" />
          <StatCard label="Revenue / 1K views" value="$0.20" sub="Blended RPM"                      color="var(--yellow)" />
          <StatCard label="Best performer" value="TikTok" subHighlight="$312" sub="this month"      color="var(--purple)" />
        </div>

        <Panel title="Platform earnings" icon="ti-currency-dollar">
          <div className="flex flex-col divide-y divide-border">
            {EARNINGS.map((e) => (
              <div key={e.platform} className="flex items-center gap-[12px] py-[12px] first:pt-0 last:pb-0">
                <div className={`w-[34px] h-[34px] rounded-[8px] flex items-center justify-center flex-shrink-0 ${e.style}`}>
                  <i className={`ti ${e.icon} text-white text-[15px]`} />
                </div>
                <div className="flex-1">
                  <div className="text-[12px] font-[500]">{e.platform}</div>
                  <div className="text-[10px] text-text3 font-mono">{e.handle}</div>
                </div>
                <div className="text-right">
                  <div className="font-display text-[16px] font-[700] text-accent">{e.current}</div>
                  <div className="text-[10px] text-text3 font-mono">this month</div>
                </div>
                <div
                  className="text-[10px] font-[600] font-mono px-[8px] py-[3px] rounded-[20px] text-accent"
                  style={{ background: 'rgba(0,229,160,.1)', border: '1px solid rgba(0,229,160,.2)' }}
                >
                  {e.growth}
                </div>
                <div className="text-right">
                  <div className="text-[12px] font-[500]">{e.projected}</div>
                  <div className="text-[10px] text-text3 font-mono">projected</div>
                </div>
              </div>
            ))}
          </div>
        </Panel>

        {/* Monetisation tips */}
        <Panel title="Growth opportunities" icon="ti-bulb">
          <div className="flex flex-col gap-[8px]">
            {[
              { icon: 'ti-player-play', color: 'var(--purple)', tip: 'Increase TikTok posting to 8× per day to hit the Creator Fund bonus threshold.' },
              { icon: 'ti-brand-youtube', color: 'var(--red)', tip: 'Enable YouTube Super Thanks — your channel qualifies with 112K subscribers.' },
              { icon: 'ti-chart-arrows-vertical', color: 'var(--accent2)', tip: 'Twitter Amplify is available for news publishers with 500K+ monthly views.' },
            ].map((tip, i) => (
              <div key={i} className="flex gap-[10px] items-start p-[10px] rounded-[8px]"
                style={{ background: 'var(--bg3)', border: '1px solid var(--border)' }}>
                <i className={`ti ${tip.icon} text-[16px] flex-shrink-0 mt-[1px]`} style={{ color: tip.color }} />
                <p className="text-[11px] text-text2 leading-[1.5]">{tip.tip}</p>
              </div>
            ))}
          </div>
        </Panel>
      </div>
    </>
  );
}
