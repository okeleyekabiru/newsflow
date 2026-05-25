import { ReactNode } from 'react';

interface TopbarProps {
  title: string;
  children?: ReactNode; // action buttons / controls
  live?: boolean;       // show pulsing live dot
}

export default function Topbar({ title, children, live }: TopbarProps) {
  return (
    <header
      className="flex items-center gap-2 px-5 py-[10px] flex-shrink-0"
      style={{ background: 'var(--bg2)', borderBottom: '1px solid var(--border)' }}
    >
      {live && <span className="status-dot" />}
      <h1 className="font-display text-[15px] font-[800] flex-1">{title}</h1>
      {children}
    </header>
  );
}
