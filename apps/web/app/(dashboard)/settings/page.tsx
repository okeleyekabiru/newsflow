import Topbar from '@/components/layout/Topbar';
import Panel from '@/components/ui/Panel';

const CONFIG_ROWS = [
  {
    section: 'AI & Content',
    rows: [
      { icon: 'ti-brain', label: 'AI model', sub: 'Primary model for rewriting', value: 'Claude Sonnet 4.6' },
      { icon: 'ti-language', label: 'Output language', sub: 'Caption and article language', value: 'English (US)' },
      { icon: 'ti-refresh', label: 'Ingest frequency', sub: 'How often to pull RSS feeds', value: 'Every 5 min' },
    ],
  },
  {
    section: 'Content filters',
    rows: [
      { icon: 'ti-shield-check', label: 'Conflict filter', sub: 'Block / review violent content', value: 'Review required', accent: true },
      { icon: 'ti-shield-off', label: 'Terrorism filter', sub: 'Auto-block extremist content', value: 'Auto-block', red: true },
      { icon: 'ti-filter', label: 'Sensitivity threshold', sub: 'Severity score that triggers review', value: '5 / 10' },
    ],
  },
  {
    section: 'Video generation',
    rows: [
      { icon: 'ti-microphone', label: 'Voice (ElevenLabs)', sub: 'TTS voice model', value: 'Rachel — Calm' },
      { icon: 'ti-video', label: 'Stock footage', sub: 'Footage provider', value: 'Pexels HD' },
      { icon: 'ti-device-mobile', label: 'Default aspect ratio', sub: 'Target platform format', value: '9:16 (Vertical)' },
    ],
  },
  {
    section: 'Notifications',
    rows: [
      { icon: 'ti-mail', label: 'Email alerts', sub: 'Receive digest emails', value: 'Daily' },
      { icon: 'ti-bell', label: 'Review alerts', sub: 'Notify when items need review', value: 'Instant' },
    ],
  },
];

export default function SettingsPage() {
  return (
    <>
      <Topbar title="Settings">
        <button
          className="flex items-center gap-[5px] px-[11px] py-[6px] rounded-btn text-[11px] font-[500] text-black"
          style={{ background: 'var(--accent)' }}
        >
          <i className="ti ti-device-floppy" /> Save changes
        </button>
      </Topbar>

      <div className="flex-1 overflow-y-auto p-[18px_20px] flex flex-col gap-[14px]">
        {CONFIG_ROWS.map(({ section, rows }) => (
          <Panel key={section} title={section} icon="ti-settings">
            <div className="flex flex-col divide-y divide-border">
              {rows.map((r) => (
                <div key={r.label} className="flex items-center justify-between py-[9px] first:pt-0 last:pb-0">
                  <div className="flex items-center gap-[7px]">
                    <i className={`ti ${r.icon} text-text3 text-[13px]`} />
                    <div>
                      <div className="text-[12px]">{r.label}</div>
                      {r.sub && <div className="text-[10px] text-text3 font-mono">{r.sub}</div>}
                    </div>
                  </div>
                  <div
                    className="flex items-center gap-[5px] font-mono text-[10px] px-[10px] py-[4px] rounded-[20px]"
                    style={{
                      background: 'var(--bg3)',
                      border: '1px solid var(--border)',
                      color: r.red ? 'var(--red)' : r.accent ? 'var(--yellow)' : 'var(--text2)',
                    }}
                  >
                    {r.value}
                    <i className="ti ti-chevron-down text-[10px]" />
                  </div>
                </div>
              ))}
            </div>
          </Panel>
        ))}

        {/* API keys section */}
        <Panel title="API keys" icon="ti-key">
          <div className="flex flex-col gap-[8px]">
            {[
              { label: 'Anthropic (Claude)', key: 'sk-ant-••••••••••••••••••••••1a4f', connected: true },
              { label: 'ElevenLabs', key: 'el_••••••••••••••••••••••3b2c', connected: true },
              { label: 'Pexels', key: 'Not configured', connected: false },
              { label: 'NewsAPI.org', key: 'Not configured', connected: false },
            ].map((k) => (
              <div key={k.label} className="flex items-center gap-[10px] p-[10px] rounded-[8px]"
                style={{ background: 'var(--bg3)', border: '1px solid var(--border)' }}>
                <div className="flex-1">
                  <div className="text-[11px] font-[500]">{k.label}</div>
                  <div className="text-[10px] font-mono text-text3 mt-[1px]">{k.key}</div>
                </div>
                <span className={`font-mono text-[9px] px-[7px] py-[2px] rounded-[20px] ${
                  k.connected ? 'text-accent bg-[rgba(0,229,160,.1)] border border-[rgba(0,229,160,.3)]'
                              : 'text-text3 bg-bg2 border border-border'}`}>
                  {k.connected ? 'Connected' : 'Not set'}
                </span>
                <button className="text-[10px] font-mono text-text2 hover:text-text">Edit</button>
              </div>
            ))}
          </div>
        </Panel>
      </div>
    </>
  );
}
