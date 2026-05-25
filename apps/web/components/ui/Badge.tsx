import { ReactNode } from 'react';

type Variant = 'auto' | 'review' | 'blocked' | 'video' | 'sev-low' | 'sev-mid' | 'sev-high' | 'mono';

interface BadgeProps {
  variant: Variant;
  icon?: string;
  children: ReactNode;
}

const VARIANT_CLASS: Record<Variant, string> = {
  'auto':     'badge-auto',
  'review':   'badge-review',
  'blocked':  'badge-blocked',
  'video':    'badge-video',
  'sev-low':  'sev-low',
  'sev-mid':  'sev-mid',
  'sev-high': 'sev-high',
  'mono':     'bg-bg3 border border-border text-text2',
};

export default function Badge({ variant, icon, children }: BadgeProps) {
  return (
    <span
      className={`inline-flex items-center gap-[3px] text-[9px] px-[6px] py-[2px] rounded-[20px] font-mono font-[500] ${VARIANT_CLASS[variant]}`}
    >
      {icon && <i className={`ti ${icon} text-[9px]`} />}
      {children}
    </span>
  );
}
