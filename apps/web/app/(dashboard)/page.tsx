import Topbar from '@/components/layout/Topbar';
import StatCard from '@/components/ui/StatCard';
import Panel from '@/components/ui/Panel';
import Badge from '@/components/ui/Badge';
import Link from 'next/link';

const feedItems = [
  { icon: 'ti-world',         iconColor: 'var(--accent2)', bg: 'rgba(0,136,255,.12)', title: 'G7 Summit reaches historic agreement on digital trade', source: 'Reuters', time: '4m ago', badges: [<Badge key="a" variant="auto" icon="ti-bolt">Auto-post</Badge>] },
  { icon: 'ti-trending-up',   iconColor: 'var(--accent)',  bg: 'rgba(0,229,160,.10)', title: 'NASDAQ surges 2.3% on strong tech earnings reports',       source: 'Bloomberg', time: '9m ago', badges: [<Badge key="a" variant="auto" icon="ti-bolt">Auto-post</Badge>, <Badge key="v" variant="video" icon="ti-player-play">Video</Badge>] },
  { icon: 'ti-alert-triangle',iconColor: 'var(--yellow)',  bg: 'rgba(255,183,0,.10)', title: 'Tensions escalate in disputed border region',               source: 'AP News', time: '15m ago', badges: [<Badge key="r" variant="review" icon="ti-eye">Review required</Badge>, <Badge key="s" variant="sev-mid">Sev 6/10</Badge>] },
  { icon: 'ti-cpu',           iconColor: 'var(--purple)',  bg: 'rgba(168,85,247,.10)', title: 'AI breakthrough: new model achieves human-level reasoning',  source: 'TechCrunch', time: '22m ago', badges: [<Badge key="a" variant="auto" icon="ti-bolt">Auto-post</Badge>] },
];

const accounts = [
  { icon: 'ti-brand-tiktok',    iconStyle: 'plat-tiktok',    handle: '@worldnewsnow',   followers: '847K', rev: '$312/mo' },
  { icon: 'ti-brand-twitter',   iconStyle: 'plat-twitter',   handle: '@breakingnews_x', followers: '512K', rev: '$198/mo' },
  { icon: 'ti-brand-instagram', iconStyle: 'plat-instagram', handle: '@reelsnews',      followers: '290K', rev: '$225/mo' },
  { icon: 'ti-brand-youtube',   iconStyle: 'plat-youtube',   handle: '@YTNewsHub',      followers: '112K', rev: '$112/mo' },
];

const weekData = [42, 58, 50, 78, 65, 82, 52];
const weekDays = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

export default function DashboardPage() {
  return (
    <>
      <Topbar title="Dashboard" live>
        <button className="btn-ghost flex items-center gap-[5px] px-[11px] py-[6px] rounded-btn border border-border text-text2 text-[11px] font-[500]">
          <i className="ti ti-refresh text-[12px]" /> Refresh
        </button>
      </Topbar>

      <div className="flex-1 overflow-y-auto p-[18px_20px] flex flex-col gap-[14px]">
        {/* Stats row */}
        <div className="grid grid-cols-4 gap-[10px]">
          <StatCard label="Posts today"     value="84"   subHighlight="6 accounts" sub="across" color="var(--accent)" />
          <StatCard label="Total followers" value="2.4M" subHighlight="+12k"       sub="this week" color="var(--accent2)" />
          <StatCard label="Est. revenue"    value="$847" sub="This month so far"   color="var(--yellow)" />
          <StatCard label="AI videos made"  value="23"   subHighlight="3"          sub="processing" color="var(--purple)" />
        </div>

        {/* Two-column body */}
        <div className="grid gap-[14px]" style={{ gridTemplateColumns: '1fr 300px' }}>
          <div className="flex flex-col gap-[14px]">
            {/* Live news feed */}
            <Panel
              title="Live news feed"
              icon="ti-rss"
              action={
                <Link href="/feed" className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px] font-[500]">
                  <i className="ti ti-arrow-right" /> View all
                </Link>
              }
            >
              <div className="flex flex-col divide-y divide-border">
                {feedItems.map((item, i) => (
                  <div key={i} className="flex gap-[10px] py-[10px] first:pt-0 last:pb-0">
                    <div className="w-8 h-8 rounded-[7px] flex items-center justify-center flex-shrink-0" style={{ background: item.bg }}>
                      <i className={`ti ${item.icon} text-[14px]`} style={{ color: item.iconColor }} />
                    </div>
                    <div>
                      <div className="text-[12px] font-[500] leading-[1.4] mb-[3px]">{item.title}</div>
                      <div className="flex items-center gap-[6px] text-[10px] text-text3 font-mono flex-wrap">
                        <span>{item.source}</span><span>·</span><span>{item.time}</span>
                        {item.badges}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </Panel>

            {/* Recent write-ups */}
            <Panel title="Recent write-ups" icon="ti-pencil" action={
              <Link href="/writeup" className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px] font-[500]">
                <i className="ti ti-plus" /> New article
              </Link>
            }>
              <div className="flex flex-col divide-y divide-border">
                {[
                  { title: 'G7 Digital Trade Summit — Full Analysis', meta: '847 words · Saved 2m ago', color: 'var(--accent)', bg: 'rgba(0,229,160,.08)', badge: <Badge key="m" variant="auto">MD</Badge> },
                  { title: 'NASDAQ Surge — Breaking Analysis', meta: '512 words · Saved 1h ago', color: 'var(--accent2)', bg: 'rgba(0,136,255,.08)', badge: <Badge key="p" variant="video">Published</Badge> },
                ].map((a, i) => (
                  <Link href="/writeup" key={i} className="flex gap-[10px] py-[10px] first:pt-0 last:pb-0 cursor-pointer">
                    <div className="w-8 h-8 rounded-[7px] flex items-center justify-center flex-shrink-0" style={{ background: a.bg }}>
                      <i className="ti ti-file-text text-[14px]" style={{ color: a.color }} />
                    </div>
                    <div className="flex-1">
                      <div className="text-[12px] font-[500] leading-[1.4] mb-[3px]">{a.title}</div>
                      <div className="flex items-center gap-[6px] text-[10px] text-text3 font-mono">
                        <span>{a.meta}</span>{a.badge}
                      </div>
                    </div>
                  </Link>
                ))}
              </div>
            </Panel>
          </div>

          {/* Right column */}
          <div className="flex flex-col gap-[14px]">
            {/* Connected accounts */}
            <Panel title="Connected accounts" icon="ti-users">
              <div className="flex flex-col divide-y divide-border -mt-[6px] -mb-[6px]">
                {accounts.map((a, i) => (
                  <div key={i} className="flex items-center gap-[10px] py-[10px]">
                    <div className={`w-[30px] h-[30px] rounded-[7px] flex items-center justify-center flex-shrink-0 ${a.iconStyle}`}>
                      <i className={`ti ${a.icon} text-white text-[14px]`} />
                    </div>
                    <div className="flex-1">
                      <div className="font-display text-[16px] font-[700]">{a.followers}</div>
                      <div className="text-[10px] text-text3 font-mono">{a.handle}</div>
                      <div className="text-[11px] font-[600] text-accent font-mono">{a.rev}</div>
                    </div>
                    <div className="w-7 h-4 rounded-lg cursor-pointer flex-shrink-0" style={{ background: 'var(--accent)', position: 'relative' }}>
                      <div className="absolute w-[10px] h-[10px] rounded-full bg-black top-[3px] right-[3px]" />
                    </div>
                  </div>
                ))}
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
