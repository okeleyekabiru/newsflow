'use client';

import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import { useState } from 'react';

interface NavItem {
  href: string;
  icon: string;
  label: string;
  badge?: { text: string; color?: 'green' | 'red' | 'purple' };
}

const NAV: { section: string; items: NavItem[] }[] = [
  {
    section: 'Automation',
    items: [
      { href: '/',         icon: 'ti-dashboard',      label: 'Dashboard' },
      { href: '/feed',     icon: 'ti-rss',            label: 'Live feed',  badge: { text: '12', color: 'green' } },
      { href: '/review',   icon: 'ti-shield-check',   label: 'Review queue', badge: { text: '3', color: 'red' } },
      { href: '/scheduler',icon: 'ti-calendar-event', label: 'Scheduler' },
    ],
  },
  {
    section: 'Content',
    items: [
      { href: '/writeup',  icon: 'ti-pencil',         label: 'Write-up studio' },
      { href: '/video',    icon: 'ti-player-play',    label: 'Video engine',  badge: { text: 'AI', color: 'purple' } },
    ],
  },
  {
    section: 'Distribution',
    items: [
      { href: '/accounts',  icon: 'ti-users',         label: 'Accounts' },
      { href: '/analytics', icon: 'ti-chart-bar',     label: 'Analytics' },
      { href: '/monetize',  icon: 'ti-currency-dollar', label: 'Monetise' },
    ],
  },
  {
    section: 'System',
    items: [
      { href: '/settings',  icon: 'ti-settings',      label: 'Settings' },
      { href: '/profile',   icon: 'ti-user-circle',   label: 'Profile' },
    ],
  },
];

const BADGE_COLORS: Record<string, string> = {
  green:  'bg-accent text-black',
  red:    'bg-red text-white',
  purple: 'bg-purple text-white',
};

export default function Sidebar() {
  const pathname  = usePathname();
  const router    = useRouter();
  const [loggingOut, setLoggingOut] = useState(false);

  async function handleLogout() {
    setLoggingOut(true);
    try {
      const refresh = localStorage.getItem('refresh_token');
      if (refresh) {
        await fetch('/api/auth/logout', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ refreshToken: refresh }),
        }).catch(() => {});
      }
    } finally {
      localStorage.removeItem('access_token');
      localStorage.removeItem('refresh_token');
      router.replace('/login');
    }
  }

  return (
    <aside
      className="flex flex-col"
      style={{
        width: 210,
        minWidth: 210,
        background: 'var(--bg2)',
        borderRight: '1px solid var(--border)',
      }}
    >
      {/* Logo */}
      <div className="px-[18px] py-[16px] pb-[14px]" style={{ borderBottom: '1px solid var(--border)' }}>
        <div className="font-display text-[17px] font-[800] tracking-[-0.5px]">
          News<span className="text-accent">Flow</span>
        </div>
        <div className="font-mono text-[9px] text-text3 tracking-[2px] uppercase mt-[2px]">
          Media Automation
        </div>
      </div>

      {/* Nav */}
      <nav className="flex-1 py-[10px] overflow-y-auto">
        {NAV.map(({ section, items }) => (
          <div key={section}>
            <div
              className="font-mono text-[9px] text-text3 tracking-[2px] uppercase px-4 pt-[6px] pb-[3px] mt-[6px]"
            >
              {section}
            </div>
            {items.map(({ href, icon, label, badge }) => {
              const active = pathname === href;
              return (
                <Link
                  key={href}
                  href={href}
                  className={`flex items-center gap-[9px] px-[18px] py-[8px] text-[12px] relative transition-all duration-[120ms] ${
                    active
                      ? 'text-accent'
                      : 'text-text2 hover:text-text hover:bg-white/[0.03]'
                  }`}
                  style={active ? { background: 'rgba(0,229,160,0.07)' } : {}}
                >
                  {active && <span className="nav-active-bar" />}
                  <i className={`ti ${icon} w-[15px] text-center text-[14px]`} />
                  <span className="flex-1">{label}</span>
                  {badge && (
                    <span
                      className={`font-mono text-[9px] px-[5px] py-[1px] rounded-[10px] font-[600] ${BADGE_COLORS[badge.color ?? 'green']}`}
                    >
                      {badge.text}
                    </span>
                  )}
                </Link>
              );
            })}
          </div>
        ))}
      </nav>

      {/* Account pill */}
      <div className="p-3" style={{ borderTop: '1px solid var(--border)' }}>
        <div
          className="flex items-center gap-[9px] rounded-[9px] p-[9px] pr-[8px]"
          style={{ background: 'var(--bg3)', border: '1px solid var(--border)' }}
        >
          <div
            className="w-[26px] h-[26px] rounded-[7px] flex items-center justify-center text-[10px] font-[700] text-black font-mono flex-shrink-0"
            style={{ background: 'linear-gradient(135deg, var(--accent), var(--accent2))' }}
          >
            KO
          </div>
          <div className="flex-1 min-w-0">
            <div className="text-[12px] font-[500] truncate">Kabiru Okeleye</div>
            <div className="text-[9px] text-accent font-mono">Pro plan</div>
          </div>
          <button
            onClick={handleLogout}
            disabled={loggingOut}
            title="Log out"
            className="flex items-center justify-center w-[26px] h-[26px] rounded-[6px] text-text3 hover:text-red hover:bg-red/10 transition-colors flex-shrink-0 disabled:opacity-50"
          >
            <i className={`ti ${loggingOut ? 'ti-loader-2 animate-spin' : 'ti-logout'} text-[14px]`} />
          </button>
        </div>
      </div>
    </aside>
  );
}
