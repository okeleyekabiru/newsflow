'use client';

import { useState, useEffect } from 'react';
import Topbar from '@/components/layout/Topbar';
import Panel from '@/components/ui/Panel';
import { api, type Account } from '@/lib/api';

const PLATFORM_META: Record<string, { icon: string; style: string; label: string }> = {
  tiktok:    { icon: 'ti-brand-tiktok',    style: 'plat-tiktok',    label: 'TikTok' },
  twitter:   { icon: 'ti-brand-twitter',   style: 'plat-twitter',   label: 'Twitter / X' },
  instagram: { icon: 'ti-brand-instagram', style: 'plat-instagram', label: 'Instagram' },
  youtube:   { icon: 'ti-brand-youtube',   style: 'plat-youtube',   label: 'YouTube' },
};

const CONNECT_PLATFORMS = ['tiktok', 'twitter', 'instagram', 'youtube'] as const;

export default function AccountsPage() {
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading]   = useState(true);
  const [error, setError]       = useState('');
  const [removing, setRemoving] = useState<string | null>(null);

  useEffect(() => {
    api.getAccounts()
      .then(setAccounts)
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, []);

  async function disconnect(id: string) {
    if (!confirm('Disconnect this account?')) return;
    setRemoving(id);
    try {
      await api.disconnectAccount(id);
      setAccounts((prev) => prev.filter((a) => a.id !== id));
    } catch (e) {
      alert(e instanceof Error ? e.message : 'Failed to disconnect');
    } finally {
      setRemoving(null);
    }
  }

  function toggleActive(id: string) {
    setAccounts((prev) =>
      prev.map((a) => (a.id === id ? { ...a, active: !a.active } : a)),
    );
  }

  function connectPlatform(platform: string) {
    alert(`OAuth flow for ${PLATFORM_META[platform]?.label ?? platform} is not yet configured. Add credentials in Settings.`);
  }

  return (
    <>
      <Topbar title="Connected accounts">
        <button
          onClick={() => alert('To connect a new account, use one of the platform buttons below.')}
          className="flex items-center gap-[5px] px-[11px] py-[6px] rounded-btn text-[11px] font-[500] text-black"
          style={{ background: 'var(--accent)' }}>
          <i className="ti ti-plus" /> Connect account
        </button>
      </Topbar>

      <div className="flex-1 overflow-y-auto p-[18px_20px] flex flex-col gap-[14px]">
        <Panel title="Social accounts" icon="ti-users">
          {loading && <div className="text-text3 text-[12px] font-mono py-2">Loading accounts…</div>}
          {error && (
            <div className="text-[12px] p-[10px] rounded-[8px]"
              style={{ background: 'rgba(255,69,96,.1)', border: '1px solid rgba(255,69,96,.3)', color: 'var(--red)' }}>
              {error}
            </div>
          )}
          {!loading && !error && accounts.length === 0 && (
            <div className="text-text3 text-[12px] font-mono py-2">No accounts connected yet.</div>
          )}

          <div className="flex flex-col divide-y divide-border">
            {accounts.map((a) => {
              const meta = PLATFORM_META[a.platform] ?? { icon: 'ti-world', style: '', label: a.platform };
              return (
                <div key={a.id} className="flex items-center gap-[10px] py-[12px] first:pt-0 last:pb-0">
                  <div className={`w-[36px] h-[36px] rounded-[8px] flex items-center justify-center flex-shrink-0 ${meta.style}`}>
                    <i className={`ti ${meta.icon} text-white text-[16px]`} />
                  </div>
                  <div className="flex-1">
                    <div className="text-[13px] font-[500]">{a.handle}</div>
                    <div className="flex items-center gap-[10px] mt-[3px] text-[10px] text-text3 font-mono">
                      <span>{a.followers} followers</span>
                      <span>{a.posts} posts</span>
                      <span className="text-accent">{a.estimatedRevenue}</span>
                    </div>
                  </div>

                  {/* Toggle */}
                  <button
                    onClick={() => toggleActive(a.id)}
                    title={a.active ? 'Disable' : 'Enable'}
                    style={{
                      width: 28, height: 16,
                      background: a.active ? 'var(--accent)' : 'var(--border2)',
                      borderRadius: 8,
                      position: 'relative',
                      flexShrink: 0,
                      border: 'none',
                      cursor: 'pointer',
                    }}>
                    <div style={{
                      position: 'absolute',
                      width: 10, height: 10,
                      borderRadius: '50%',
                      background: a.active ? '#000' : 'var(--bg)',
                      top: 3,
                      [a.active ? 'right' : 'left']: 3,
                    }} />
                  </button>

                  {/* Disconnect */}
                  <button
                    onClick={() => disconnect(a.id)}
                    disabled={removing === a.id}
                    className="ml-2 text-text3 hover:text-red transition-colors disabled:opacity-50"
                    title="Disconnect">
                    <i className={`ti ${removing === a.id ? 'ti-loader-2' : 'ti-trash'} text-[14px]`} />
                  </button>
                </div>
              );
            })}
          </div>
        </Panel>

        {/* Platform connect buttons */}
        <div className="grid grid-cols-4 gap-[10px]">
          {CONNECT_PLATFORMS.map((platform) => {
            const meta = PLATFORM_META[platform];
            return (
              <button
                key={platform}
                onClick={() => connectPlatform(platform)}
                className="flex flex-col items-center gap-[8px] p-[16px] rounded-card cursor-pointer hover:border-border2 transition-all text-[12px] font-[500]"
                style={{ background: 'var(--card)', border: '1px solid var(--border)' }}>
                <div className={`w-10 h-10 rounded-[9px] flex items-center justify-center ${meta.style}`}>
                  <i className={`ti ${meta.icon} text-white text-[18px]`} />
                </div>
                Connect {meta.label}
              </button>
            );
          })}
        </div>
      </div>
    </>
  );
}
