import Topbar from '@/components/layout/Topbar';
import Panel from '@/components/ui/Panel';
import Badge from '@/components/ui/Badge';
import Link from 'next/link';

const CATS = [
  { icon: 'ti-world',             label: 'Politics',   color: 'var(--accent2)', active: true },
  { icon: 'ti-trending-up',       label: 'Finance',    color: 'var(--accent)',  active: true },
  { icon: 'ti-cpu',               label: 'Technology', color: 'var(--purple)',  active: true },
  { icon: 'ti-ball-football',     label: 'Sports',     color: 'var(--yellow)',  active: true },
  { icon: 'ti-heart-rate-monitor',label: 'Health',     color: '#22d3ee',        active: true },
  { icon: 'ti-cloud-storm',       label: 'Weather',    active: false },
  { icon: 'ti-planet',            label: 'Science',    active: false },
];

const ITEMS = [
  { icon: 'ti-world',          iconColor: 'var(--accent2)', bg: 'rgba(0,136,255,.12)', title: 'G7 Summit reaches historic agreement on digital trade regulations', source: 'Reuters', time: '4m ago',  badges: [<Badge key="a" variant="auto" icon="ti-bolt">Auto-post</Badge>] },
  { icon: 'ti-trending-up',    iconColor: 'var(--accent)',  bg: 'rgba(0,229,160,.10)', title: 'NASDAQ surges 2.3% on strong tech earnings',                         source: 'Bloomberg', time: '9m ago', badges: [<Badge key="a" variant="auto" icon="ti-bolt">Auto-post</Badge>, <Badge key="v" variant="video" icon="ti-player-play">Video</Badge>] },
  { icon: 'ti-alert-triangle', iconColor: 'var(--yellow)',  bg: 'rgba(255,183,0,.10)', title: 'Tensions escalate in disputed border region — casualties confirmed',   source: 'AP News', time: '15m ago', badges: [<Badge key="r" variant="review" icon="ti-eye">Review required</Badge>, <Badge key="s" variant="sev-mid">Sev 6/10</Badge>] },
  { icon: 'ti-cpu',            iconColor: 'var(--purple)',  bg: 'rgba(168,85,247,.10)', title: 'AI breakthrough: new model achieves human-level reasoning',            source: 'TechCrunch', time: '22m ago', badges: [<Badge key="a" variant="auto" icon="ti-bolt">Auto-post</Badge>] },
  { icon: 'ti-shield-x',       iconColor: 'var(--red)',     bg: 'rgba(255,69,96,.10)', title: 'Extremist group claims responsibility — full details withheld',         source: 'BBC', time: '31m ago',  badges: [<Badge key="b" variant="blocked" icon="ti-lock">Blocked</Badge>, <Badge key="s" variant="sev-high">Sev 9/10</Badge>] },
];

export default function FeedPage() {
  return (
    <>
      <Topbar title="Live news feed" live>
        <button className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px] font-[500]">
          <i className="ti ti-filter" /> Filter
        </button>
        <button className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px] font-[500]">
          <i className="ti ti-refresh" /> Refresh
        </button>
      </Topbar>

      <div className="flex-1 overflow-y-auto p-[18px_20px] flex flex-col gap-[14px]">
        <Panel title="Live news feed" icon="ti-rss" action={
          <div className="flex gap-[6px]">
            <button className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px]"><i className="ti ti-filter" /> Filter</button>
            <button className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px]"><i className="ti ti-refresh" /> Refresh</button>
          </div>
        }>
          {/* Category chips */}
          <div className="flex flex-wrap gap-[6px] mb-[10px]">
            {CATS.map((c) => (
              <div
                key={c.label}
                className="flex items-center gap-[5px] px-[10px] py-[5px] rounded-btn border text-[11px] font-[500] cursor-pointer"
                style={c.active
                  ? { borderColor: c.color, background: 'rgba(0,229,160,.07)', color: c.color }
                  : { borderColor: 'var(--border)', background: 'var(--bg3)', color: 'var(--text)' }}
              >
                <i className={`ti ${c.icon}`} /> {c.label}
              </div>
            ))}
          </div>

          {/* Feed items */}
          <div className="flex flex-col divide-y divide-border">
            {ITEMS.map((item, i) => (
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

        {/* Review notice */}
        <div className="flex gap-[10px] items-center p-[10px_14px] rounded-[9px] text-[11px] text-text2"
          style={{ background: 'rgba(255,183,0,.06)', border: '1px solid rgba(255,183,0,.2)' }}>
          <i className="ti ti-alert-triangle text-yellow text-[15px] flex-shrink-0" />
          Conflict &amp; Geopolitics requires manual review.{' '}
          <Link href="/review" className="text-yellow cursor-pointer">Go to review queue →</Link>
        </div>
      </div>
    </>
  );
}
