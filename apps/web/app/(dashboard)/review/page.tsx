import Topbar from '@/components/layout/Topbar';
import Badge from '@/components/ui/Badge';

const QUEUE = [
  {
    title: 'Tensions escalate in disputed border region — 12 casualties confirmed',
    source: 'AP News', time: '15m ago', category: 'Conflict',
    severity: 6, sevVariant: 'sev-mid' as const,
    reason: 'Contains graphic conflict details. Manual review required by policy.',
  },
  {
    title: "Government announces emergency powers amid civil unrest",
    source: 'Reuters', time: '28m ago', category: 'Politics',
    severity: 7, sevVariant: 'sev-mid' as const,
    reason: 'Sensitive political content. Verify sources before publishing.',
  },
  {
    title: 'Protest turns violent — police disperse crowd with force',
    source: 'BBC', time: '41m ago', category: 'Conflict',
    severity: 8, sevVariant: 'sev-high' as const,
    reason: 'Requires additional context and editorial judgement.',
  },
];

export default function ReviewPage() {
  return (
    <>
      <Topbar title="Review queue">
        <span className="font-mono text-[10px] text-yellow bg-[rgba(255,183,0,0.10)] border border-[rgba(255,183,0,0.3)] px-[8px] py-[3px] rounded-[20px]">
          {QUEUE.length} items pending
        </span>
      </Topbar>

      <div className="flex-1 overflow-y-auto p-[18px_20px] flex flex-col gap-[10px]">
        {/* Notice */}
        <div className="flex gap-[10px] items-start p-[12px_14px] rounded-[9px]"
          style={{ background: 'rgba(255,183,0,.06)', border: '1px solid rgba(255,183,0,.2)' }}>
          <i className="ti ti-alert-triangle text-yellow text-[15px] flex-shrink-0 mt-[1px]" />
          <div className="text-[11px] text-text2">
            <strong className="text-text">Conflict &amp; Geopolitics policy</strong> — articles in this category require
            manual approval before auto-posting. Assess severity, verify sources, and approve or reject.
          </div>
        </div>

        {/* Queue items */}
        {QUEUE.map((item, i) => (
          <div key={i} className="rounded-[9px] p-[12px]" style={{ background: 'var(--bg3)', border: '1px solid var(--border)' }}>
            <div className="flex items-start justify-between gap-[8px] mb-[4px]">
              <div className="text-[12px] font-[500] text-text leading-[1.4]">{item.title}</div>
              <Badge variant={item.sevVariant}>Sev {item.severity}/10</Badge>
            </div>
            <div className="text-[10px] text-text3 font-mono mb-[8px]">
              {item.source} · {item.time} · {item.category}
            </div>
            <div className="text-[11px] text-text2 mb-[10px] p-[8px] rounded-[7px]"
              style={{ background: 'var(--bg2)', border: '1px solid var(--border)' }}>
              <i className="ti ti-info-circle text-text3 mr-[5px]" />
              {item.reason}
            </div>
            <div className="flex gap-[6px]">
              <button className="text-[10px] px-[10px] py-[4px] rounded-[6px] font-[500] cursor-pointer"
                style={{ background: 'rgba(0,229,160,.15)', color: 'var(--accent)', border: '1px solid rgba(0,229,160,.3)' }}>
                <i className="ti ti-check mr-[4px]" /> Approve &amp; post
              </button>
              <button className="text-[10px] px-[10px] py-[4px] rounded-[6px] font-[500] cursor-pointer"
                style={{ background: 'rgba(255,69,96,.1)', color: 'var(--red)', border: '1px solid rgba(255,69,96,.2)' }}>
                <i className="ti ti-x mr-[4px]" /> Reject
              </button>
              <button className="text-[10px] px-[10px] py-[4px] rounded-[6px] font-[500] cursor-pointer"
                style={{ background: 'var(--bg2)', color: 'var(--text2)', border: '1px solid var(--border)' }}>
                <i className="ti ti-pencil mr-[4px]" /> Edit article
              </button>
              <button className="text-[10px] px-[10px] py-[4px] rounded-[6px] font-[500] cursor-pointer ml-auto"
                style={{ color: 'var(--text3)' }}>
                View source →
              </button>
            </div>
          </div>
        ))}
      </div>
    </>
  );
}
