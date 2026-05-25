import Topbar from '@/components/layout/Topbar';
import Panel from '@/components/ui/Panel';

const SCHEDULE = [
  { time: '09:00', platform: 'TikTok',    handle: '@worldnewsnow',   title: 'G7 Summit — Full Breakdown', type: 'Video', status: 'scheduled' },
  { time: '10:30', platform: 'Twitter',   handle: '@breakingnews_x', title: 'NASDAQ surge analysis thread', type: 'Thread', status: 'scheduled' },
  { time: '12:00', platform: 'Instagram', handle: '@reelsnews',      title: 'Finance weekly reel',          type: 'Reel',   status: 'processing' },
  { time: '14:00', platform: 'TikTok',    handle: '@worldnewsnow',   title: 'AI news daily brief',          type: 'Video', status: 'scheduled' },
  { time: '16:00', platform: 'YouTube',   handle: '@YTNewsHub',      title: 'Evening news compilation',     type: 'Video', status: 'draft' },
];

const STATUS_STYLE: Record<string, string> = {
  scheduled:  'text-accent  bg-[rgba(0,229,160,.10)]  border-[rgba(0,229,160,.3)]',
  processing: 'text-yellow  bg-[rgba(255,183,0,.10)]  border-[rgba(255,183,0,.3)]',
  draft:      'text-text3   bg-[rgba(74,84,112,.10)]  border-[rgba(74,84,112,.3)]',
};

const PLAT_STYLE: Record<string, string> = {
  TikTok: 'plat-tiktok', Twitter: 'plat-twitter', Instagram: 'plat-instagram', YouTube: 'plat-youtube',
};
const PLAT_ICON: Record<string, string> = {
  TikTok: 'ti-brand-tiktok', Twitter: 'ti-brand-twitter', Instagram: 'ti-brand-instagram', YouTube: 'ti-brand-youtube',
};

export default function SchedulerPage() {
  return (
    <>
      <Topbar title="Post scheduler">
        <button
          className="flex items-center gap-[5px] px-[11px] py-[6px] rounded-btn text-[11px] font-[500] text-black"
          style={{ background: 'var(--accent)' }}
        >
          <i className="ti ti-plus" /> Schedule post
        </button>
      </Topbar>

      <div className="flex-1 overflow-y-auto p-[18px_20px] flex flex-col gap-[14px]">
        {/* Today header */}
        <div className="flex items-center gap-[10px]">
          <div className="font-display text-[13px] font-[700]">Today</div>
          <div className="flex-1 h-[1px]" style={{ background: 'var(--border)' }} />
          <div className="font-mono text-[10px] text-text3">
            {new Date().toLocaleDateString('en-GB', { weekday: 'long', day: 'numeric', month: 'long' })}
          </div>
        </div>

        <Panel title="Scheduled posts" icon="ti-calendar-event">
          <div className="flex flex-col divide-y divide-border">
            {SCHEDULE.map((s, i) => (
              <div key={i} className="flex items-center gap-[12px] py-[12px] first:pt-0 last:pb-0">
                <div className="font-mono text-[12px] text-text2 w-[40px] flex-shrink-0">{s.time}</div>
                <div className={`w-[30px] h-[30px] rounded-[7px] flex items-center justify-center flex-shrink-0 ${PLAT_STYLE[s.platform]}`}>
                  <i className={`ti ${PLAT_ICON[s.platform]} text-white text-[13px]`} />
                </div>
                <div className="flex-1">
                  <div className="text-[12px] font-[500]">{s.title}</div>
                  <div className="text-[10px] text-text3 font-mono">{s.handle} · {s.type}</div>
                </div>
                <span className={`font-mono text-[9px] px-[8px] py-[3px] rounded-[20px] font-[500] border capitalize ${STATUS_STYLE[s.status]}`}>
                  {s.status}
                </span>
                <button className="text-text3 hover:text-text transition-colors">
                  <i className="ti ti-dots-vertical text-[14px]" />
                </button>
              </div>
            ))}
          </div>
        </Panel>

        {/* Recurrence settings */}
        <Panel title="Auto-scheduling rules" icon="ti-settings">
          <div className="flex flex-col divide-y divide-border">
            {[
              { label: 'Post frequency', value: '6× per day', icon: 'ti-clock' },
              { label: 'Peak hours', value: '8am – 10pm', icon: 'ti-sun' },
              { label: 'TikTok cadence', value: 'Every 4h', icon: 'ti-brand-tiktok' },
              { label: 'Twitter cadence', value: 'Every 2h', icon: 'ti-brand-twitter' },
            ].map((r) => (
              <div key={r.label} className="flex items-center justify-between py-[9px]">
                <div className="flex items-center gap-[7px] text-[12px]">
                  <i className={`ti ${r.icon} text-text3 text-[13px]`} /> {r.label}
                </div>
                <span className="font-mono text-[11px] text-accent">{r.value}</span>
              </div>
            ))}
          </div>
        </Panel>
      </div>
    </>
  );
}
