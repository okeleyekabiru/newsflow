import { ReactNode } from 'react';

interface PanelProps {
  title: string;
  icon?: string;   // Tabler icon class e.g. "ti-rss"
  action?: ReactNode;
  children: ReactNode;
  className?: string;
}

export default function Panel({ title, icon, action, children, className }: PanelProps) {
  return (
    <div
      className={`rounded-card overflow-hidden ${className ?? ''}`}
      style={{ background: 'var(--card)', border: '1px solid var(--border)' }}
    >
      <div
        className="flex items-center justify-between px-4 py-3"
        style={{ borderBottom: '1px solid var(--border)' }}
      >
        <div className="flex items-center gap-[7px] text-[12px] font-[600]">
          {icon && <i className={`ti ${icon} text-accent text-[14px]`} />}
          {title}
        </div>
        {action}
      </div>
      <div className="p-4">{children}</div>
    </div>
  );
}
