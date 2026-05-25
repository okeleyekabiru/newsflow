interface StatCardProps {
  label: string;
  value: string;
  sub?: string;
  subHighlight?: string; // text before subHighlight
  color?: string;        // CSS color value for the top bar, defaults to --accent
}

export default function StatCard({ label, value, sub, subHighlight, color }: StatCardProps) {
  return (
    <div
      className="stat-card-bar rounded-card p-[14px]"
      style={{
        background: 'var(--card)',
        border: '1px solid var(--border)',
        '--bar-color': color ?? 'var(--accent)',
      } as React.CSSProperties}
    >
      <div className="font-mono text-[10px] text-text3 uppercase tracking-[1px] mb-1">{label}</div>
      <div className="font-display text-[22px] font-[700]">{value}</div>
      {(sub || subHighlight) && (
        <div className="text-[10px] text-text3 mt-[3px]">
          {subHighlight && <span className="text-accent">{subHighlight}</span>}
          {sub && ` ${sub}`}
        </div>
      )}
    </div>
  );
}
