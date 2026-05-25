import Topbar from '@/components/layout/Topbar';
import StatCard from '@/components/ui/StatCard';
import Panel from '@/components/ui/Panel';

const VIDEOS = [
  { thumb: 'ti-player-play', title: 'NASDAQ Surge — Financial Breakdown', meta: '9:16 TikTok · 58s', status: 'processing', statusStyle: 'text-yellow bg-[rgba(255,183,0,.1)] border-[rgba(255,183,0,.3)]' },
  { thumb: 'ti-player-play', title: 'G7 Summit Analysis — Full Report',   meta: '9:16 TikTok · 72s', status: 'done',       statusStyle: 'text-accent  bg-[rgba(0,229,160,.1)] border-[rgba(0,229,160,.3)]' },
  { thumb: 'ti-player-play', title: 'Weekly AI Digest — Top Stories',     meta: '16:9 YouTube · 3m', status: 'done',       statusStyle: 'text-accent  bg-[rgba(0,229,160,.1)] border-[rgba(0,229,160,.3)]' },
  { thumb: 'ti-player-play', title: 'Instagram Reels: Finance Roundup',   meta: '9:16 Reels · 30s',  status: 'queued',     statusStyle: 'text-text3   bg-[rgba(74,84,112,.1)] border-[rgba(74,84,112,.3)]' },
];

export default function VideoPage() {
  return (
    <>
      <Topbar title="Video engine">
        <button
          className="flex items-center gap-[5px] px-[11px] py-[6px] rounded-btn text-[11px] font-[500]"
          style={{ background: 'rgba(168,85,247,.15)', color: 'var(--purple)', border: '1px solid rgba(168,85,247,.3)' }}
        >
          <i className="ti ti-sparkles" /> Generate new
        </button>
      </Topbar>

      <div className="flex-1 overflow-y-auto p-[18px_20px] flex flex-col gap-[14px]">
        <div className="grid grid-cols-4 gap-[10px]">
          <StatCard label="Videos generated" value="23"   subHighlight="3" sub="processing now"  color="var(--purple)" />
          <StatCard label="Total video views" value="1.8M" subHighlight="+22%" sub="this week"    color="var(--accent)" />
          <StatCard label="Avg watch time"    value="38s"  sub="TikTok benchmark"                 color="var(--accent2)" />
          <StatCard label="Video revenue"     value="$415" sub="This month"                       color="var(--yellow)" />
        </div>

        <Panel title="Video queue" icon="ti-player-play" action={
          <button className="flex items-center gap-[5px] px-[10px] py-[4px] rounded-btn text-[10px]"
            style={{ background: 'rgba(168,85,247,.15)', color: 'var(--purple)', border: '1px solid rgba(168,85,247,.3)' }}>
            <i className="ti ti-sparkles" /> Generate new
          </button>
        }>
          <div className="flex flex-col divide-y divide-border">
            {VIDEOS.map((v, i) => (
              <div key={i} className="flex items-center gap-[12px] py-[12px] first:pt-0 last:pb-0">
                <div
                  className="w-[52px] h-[36px] rounded-[6px] flex items-center justify-center flex-shrink-0"
                  style={{ background: 'var(--bg3)', border: '1px solid var(--border)' }}
                >
                  <i className={`ti ${v.thumb} text-[16px] text-red`} />
                </div>
                <div className="flex-1">
                  <div className="text-[12px] font-[500]">{v.title}</div>
                  <div className="text-[10px] text-text3 font-mono mt-[2px]">{v.meta}</div>
                </div>
                <span className={`font-mono text-[9px] px-[8px] py-[3px] rounded-[20px] font-[500] border capitalize ${v.statusStyle}`}>
                  {v.status}
                </span>
                <button className="text-text3 hover:text-text transition-colors">
                  <i className="ti ti-download text-[14px]" />
                </button>
              </div>
            ))}
          </div>
        </Panel>

        {/* Generation settings */}
        <Panel title="Video generation settings" icon="ti-settings">
          <div className="grid grid-cols-2 gap-[14px]">
            {[
              { label: 'Default format', value: '9:16 (TikTok / Reels)', icon: 'ti-device-mobile' },
              { label: 'Voice model', value: 'ElevenLabs — Rachel', icon: 'ti-microphone' },
              { label: 'Stock footage', value: 'Pexels HD library', icon: 'ti-video' },
              { label: 'Auto-generate', value: 'On — 5× per day', icon: 'ti-bolt' },
            ].map((s) => (
              <div key={s.label} className="flex items-center gap-[8px] p-[10px] rounded-[8px]"
                style={{ background: 'var(--bg3)', border: '1px solid var(--border)' }}>
                <i className={`ti ${s.icon} text-text3 text-[16px]`} />
                <div>
                  <div className="text-[10px] text-text3 font-mono">{s.label}</div>
                  <div className="text-[12px] font-[500]">{s.value}</div>
                </div>
              </div>
            ))}
          </div>
        </Panel>
      </div>
    </>
  );
}
