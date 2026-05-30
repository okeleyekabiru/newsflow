'use client';

import { useState, useEffect } from 'react';
import Topbar from '@/components/layout/Topbar';
import Badge from '@/components/ui/Badge';
import { api, type Flag } from '@/lib/api';

export default function ReviewPage() {
  const [queue, setQueue] = useState<Flag[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [acting, setActing] = useState<string | null>(null);

  useEffect(() => {
    api.getFlags()
      .then(setQueue)
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, []);

  async function act(id: string, action: 'approve' | 'reject' | 'escalate') {
    setActing(id);
    try {
      if (action === 'approve') await api.approveFlag(id);
      else if (action === 'reject') await api.rejectFlag(id);
      else await api.escalateFlag(id);
      setQueue((q) => q.filter((f) => f.id !== id));
    } catch (e) {
      alert(e instanceof Error ? e.message : 'Action failed');
    } finally {
      setActing(null);
    }
  }

  return (
    <>
      <Topbar title="Review queue">
        <span className="font-mono text-[10px] text-yellow bg-[rgba(255,183,0,0.10)] border border-[rgba(255,183,0,0.3)] px-[8px] py-[3px] rounded-[20px]">
          {queue.length} items pending
        </span>
      </Topbar>

      <div className="flex-1 overflow-y-auto p-[18px_20px] flex flex-col gap-[10px]">
        <div className="flex gap-[10px] items-start p-[12px_14px] rounded-[9px]"
          style={{ background: 'rgba(255,183,0,.06)', border: '1px solid rgba(255,183,0,.2)' }}>
          <i className="ti ti-alert-triangle text-yellow text-[15px] flex-shrink-0 mt-[1px]" />
          <div className="text-[11px] text-text2">
            <strong className="text-text">Conflict &amp; Geopolitics policy</strong> — articles in this category
            require manual approval before auto-posting. Assess severity, verify sources, and approve or reject.
          </div>
        </div>

        {loading && (
          <div className="text-text3 text-[12px] font-mono py-4">Loading queue…</div>
        )}
        {error && (
          <div className="text-[12px] p-[10px] rounded-[8px]"
            style={{ background: 'rgba(255,69,96,.1)', border: '1px solid rgba(255,69,96,.3)', color: 'var(--red)' }}>
            {error}
          </div>
        )}
        {!loading && !error && queue.length === 0 && (
          <div className="text-text3 text-[12px] font-mono text-center py-8">
            No items pending review.
          </div>
        )}

        {queue.map((item) => {
          const sevVariant = item.severity >= 8 ? 'sev-high' : 'sev-mid';
          const busy = acting === item.id;
          return (
            <div key={item.id} className="rounded-[9px] p-[12px]"
              style={{ background: 'var(--bg3)', border: '1px solid var(--border)' }}>
              <div className="flex items-start justify-between gap-[8px] mb-[4px]">
                <div className="text-[12px] font-[500] text-text leading-[1.4]">{item.title}</div>
                <Badge variant={sevVariant as 'sev-high' | 'sev-mid'}>Sev {item.severity}/10</Badge>
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
                <button
                  onClick={() => act(item.id, 'approve')}
                  disabled={busy}
                  className="text-[10px] px-[10px] py-[4px] rounded-[6px] font-[500] cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                  style={{ background: 'rgba(0,229,160,.15)', color: 'var(--accent)', border: '1px solid rgba(0,229,160,.3)' }}>
                  <i className="ti ti-check mr-[4px]" />{busy && acting === item.id ? 'Working…' : 'Approve & post'}
                </button>
                <button
                  onClick={() => act(item.id, 'reject')}
                  disabled={busy}
                  className="text-[10px] px-[10px] py-[4px] rounded-[6px] font-[500] cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                  style={{ background: 'rgba(255,69,96,.1)', color: 'var(--red)', border: '1px solid rgba(255,69,96,.2)' }}>
                  <i className="ti ti-x mr-[4px]" />Reject
                </button>
                <button
                  onClick={() => act(item.id, 'escalate')}
                  disabled={busy}
                  className="text-[10px] px-[10px] py-[4px] rounded-[6px] font-[500] cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                  style={{ background: 'var(--bg2)', color: 'var(--text2)', border: '1px solid var(--border)' }}>
                  <i className="ti ti-arrow-up mr-[4px]" />Escalate
                </button>
                {item.sourceUrl && (
                  <a
                    href={item.sourceUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-[10px] px-[10px] py-[4px] rounded-[6px] font-[500] cursor-pointer ml-auto"
                    style={{ color: 'var(--text3)' }}>
                    View source →
                  </a>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </>
  );
}
