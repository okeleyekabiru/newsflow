import Topbar from '@/components/layout/Topbar';
import Panel from '@/components/ui/Panel';

const ACCOUNTS = [
  { icon: 'ti-brand-tiktok',    style: 'plat-tiktok',    handle: '@worldnewsnow',   followers: '847K', posts: '1,204', rev: '$312/mo', active: true },
  { icon: 'ti-brand-twitter',   style: 'plat-twitter',   handle: '@breakingnews_x', followers: '512K', posts: '2,891', rev: '$198/mo', active: true },
  { icon: 'ti-brand-instagram', style: 'plat-instagram', handle: '@reelsnews',      followers: '290K', posts: '612',   rev: '$225/mo', active: true },
  { icon: 'ti-brand-youtube',   style: 'plat-youtube',   handle: '@YTNewsHub',      followers: '112K', posts: '89',    rev: '$112/mo', active: false },
];

export default function AccountsPage() {
  return (
    <>
      <Topbar title="Connected accounts">
        <button
          className="flex items-center gap-[5px] px-[11px] py-[6px] rounded-btn text-[11px] font-[500] text-black"
          style={{ background: 'var(--accent)' }}
        >
          <i className="ti ti-plus" /> Connect account
        </button>
      </Topbar>

      <div className="flex-1 overflow-y-auto p-[18px_20px] flex flex-col gap-[14px]">
        <Panel title="Social accounts" icon="ti-users">
          <div className="flex flex-col divide-y divide-border">
            {ACCOUNTS.map((a) => (
              <div key={a.handle} className="flex items-center gap-[10px] py-[12px] first:pt-0 last:pb-0">
                <div className={`w-[36px] h-[36px] rounded-[8px] flex items-center justify-center flex-shrink-0 ${a.style}`}>
                  <i className={`ti ${a.icon} text-white text-[16px]`} />
                </div>
                <div className="flex-1">
                  <div className="text-[13px] font-[500]">{a.handle}</div>
                  <div className="flex items-center gap-[10px] mt-[3px] text-[10px] text-text3 font-mono">
                    <span>{a.followers} followers</span>
                    <span>{a.posts} posts</span>
                    <span className="text-accent">{a.rev}</span>
                  </div>
                </div>
                <div
                  className="cursor-pointer flex-shrink-0"
                  style={{
                    width: 28, height: 16,
                    background: a.active ? 'var(--accent)' : 'var(--border2)',
                    borderRadius: 8,
                    position: 'relative',
                  }}
                >
                  <div style={{
                    position: 'absolute',
                    width: 10, height: 10,
                    borderRadius: '50%',
                    background: a.active ? '#000' : 'var(--bg)',
                    top: 3,
                    [a.active ? 'right' : 'left']: 3,
                  }} />
                </div>
                <button className="ml-2 text-text3 hover:text-red transition-colors">
                  <i className="ti ti-trash text-[14px]" />
                </button>
              </div>
            ))}
          </div>
        </Panel>

        {/* OAuth connect buttons */}
        <div className="grid grid-cols-4 gap-[10px]">
          {[
            { icon: 'ti-brand-tiktok',    label: 'TikTok',    style: 'plat-tiktok' },
            { icon: 'ti-brand-twitter',   label: 'Twitter / X', style: 'plat-twitter' },
            { icon: 'ti-brand-instagram', label: 'Instagram', style: 'plat-instagram' },
            { icon: 'ti-brand-youtube',   label: 'YouTube',   style: 'plat-youtube' },
          ].map((p) => (
            <button
              key={p.label}
              className="flex flex-col items-center gap-[8px] p-[16px] rounded-card cursor-pointer hover:border-border2 transition-all text-[12px] font-[500]"
              style={{ background: 'var(--card)', border: '1px solid var(--border)' }}
            >
              <div className={`w-10 h-10 rounded-[9px] flex items-center justify-center ${p.style}`}>
                <i className={`ti ${p.icon} text-white text-[18px]`} />
              </div>
              Connect {p.label}
            </button>
          ))}
        </div>
      </div>
    </>
  );
}
