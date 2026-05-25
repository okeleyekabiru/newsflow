import Topbar from '@/components/layout/Topbar';
import Panel from '@/components/ui/Panel';

export default function ProfilePage() {
  return (
    <>
      <Topbar title="Profile" />

      <div className="flex-1 overflow-y-auto p-[18px_20px] flex flex-col gap-[14px]">
        {/* Avatar + name */}
        <div className="flex items-center gap-[16px] p-[20px] rounded-card"
          style={{ background: 'var(--card)', border: '1px solid var(--border)' }}>
          <div
            className="w-[60px] h-[60px] rounded-[14px] flex items-center justify-center text-[22px] font-[700] text-black font-mono flex-shrink-0"
            style={{ background: 'linear-gradient(135deg, var(--accent), var(--accent2))' }}
          >
            KO
          </div>
          <div>
            <div className="font-display text-[20px] font-[800]">Kabiru Okeleye</div>
            <div className="text-[12px] text-text2 mt-[2px]">kabiotobiano@gmail.com</div>
            <div className="flex items-center gap-[6px] mt-[6px]">
              <span className="font-mono text-[10px] text-accent px-[8px] py-[2px] rounded-[20px]"
                style={{ background: 'rgba(0,229,160,.1)', border: '1px solid rgba(0,229,160,.3)' }}>
                Pro plan
              </span>
              <span className="font-mono text-[10px] text-text3">Member since Jan 2025</span>
            </div>
          </div>
          <div className="ml-auto">
            <button className="flex items-center gap-[5px] px-[11px] py-[6px] rounded-btn border border-border text-text2 text-[11px] font-[500]">
              <i className="ti ti-pencil text-[12px]" /> Edit profile
            </button>
          </div>
        </div>

        {/* Stats */}
        <div className="grid grid-cols-4 gap-[10px]">
          {[
            { label: 'Articles written', value: '1,204', icon: 'ti-file-text', color: 'var(--accent)' },
            { label: 'Posts published',  value: '4,891', icon: 'ti-send',      color: 'var(--accent2)' },
            { label: 'Videos generated', value: '23',    icon: 'ti-player-play', color: 'var(--purple)' },
            { label: 'Revenue earned',   value: '$4.2K', icon: 'ti-currency-dollar', color: 'var(--yellow)' },
          ].map((s) => (
            <div key={s.label} className="p-[14px] rounded-card flex items-center gap-[12px]"
              style={{ background: 'var(--card)', border: '1px solid var(--border)' }}>
              <div className="w-[36px] h-[36px] rounded-[8px] flex items-center justify-center flex-shrink-0"
                style={{ background: `${s.color}1a` }}>
                <i className={`ti ${s.icon} text-[16px]`} style={{ color: s.color }} />
              </div>
              <div>
                <div className="font-display text-[18px] font-[700]">{s.value}</div>
                <div className="text-[10px] text-text3 font-mono">{s.label}</div>
              </div>
            </div>
          ))}
        </div>

        {/* Account settings */}
        <Panel title="Account settings" icon="ti-user">
          <div className="flex flex-col divide-y divide-border">
            {[
              { label: 'Display name', value: 'Kabiru Okeleye' },
              { label: 'Email address', value: 'kabiotobiano@gmail.com' },
              { label: 'Plan', value: 'Pro — $49/mo' },
              { label: 'Timezone', value: 'UTC+1 (Lagos)' },
            ].map((r) => (
              <div key={r.label} className="flex items-center justify-between py-[9px]">
                <span className="text-[12px] text-text2">{r.label}</span>
                <div className="flex items-center gap-[8px]">
                  <span className="font-mono text-[11px] text-text">{r.value}</span>
                  <button className="text-[10px] text-text3 hover:text-text">Edit</button>
                </div>
              </div>
            ))}
          </div>
        </Panel>

        {/* Danger zone */}
        <Panel title="Danger zone" icon="ti-alert-triangle">
          <div className="flex items-center justify-between p-[12px] rounded-[8px]"
            style={{ background: 'rgba(255,69,96,.06)', border: '1px solid rgba(255,69,96,.2)' }}>
            <div>
              <div className="text-[12px] font-[500]">Delete account</div>
              <div className="text-[10px] text-text3 font-mono">Permanently delete all data. This cannot be undone.</div>
            </div>
            <button className="px-[12px] py-[6px] rounded-btn text-[11px] font-[500]"
              style={{ background: 'rgba(255,69,96,.15)', color: 'var(--red)', border: '1px solid rgba(255,69,96,.3)' }}>
              Delete
            </button>
          </div>
        </Panel>
      </div>
    </>
  );
}
