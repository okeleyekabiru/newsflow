'use client';

import { useState, useEffect } from 'react';
import Topbar from '@/components/layout/Topbar';
import Badge from '@/components/ui/Badge';
import { api, type Flag, type FlagDetail } from '@/lib/api';

function relativeTime(iso: string) {
  const m = Math.floor((Date.now() - new Date(iso).getTime()) / 60000);
  if (m < 1) return 'just now';
  if (m < 60) return `${m}m ago`;
  if (m < 1440) return `${Math.floor(m / 60)}h ago`;
  return `${Math.floor(m / 1440)}d ago`;
}

function DetailDrawer({
  flagId,
  onClose,
  onAction,
}: {
  flagId: string;
  onClose: () => void;
  onAction: (id: string, action: 'approve' | 'reject' | 'escalate') => Promise<void>;
}) {
  const [detail, setDetail] = useState<FlagDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [acting, setActing] = useState<string | null>(null);

  useEffect(() => {
    setLoading(true);
    setDetail(null);
    api.getFlag(flagId)
      .then(setDetail)
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [flagId]);

  async function act(action: 'approve' | 'reject' | 'escalate') {
    setActing(action);
    try {
      await onAction(flagId, action);
      onClose();
    } finally {
      setActing(null);
    }
  }

  const busy = acting !== null;

  return (
    <div
      className="fixed inset-0 z-50 flex justify-end"
      style={{ background: 'rgba(0,0,0,0.55)' }}
      onClick={onClose}
    >
      <div
        className="h-full flex flex-col overflow-hidden"
        style={{
          width: 'min(680px, 90vw)',
          background: 'var(--bg2)',
          borderLeft: '1px solid var(--border)',
        }}
        onClick={(e) => e.stopPropagation()}
      >
        {/* Drawer header */}
        <div
          className="flex items-center justify-between flex-shrink-0 px-[18px] py-[12px]"
          style={{ borderBottom: '1px solid var(--border)' }}
        >
          <div className="text-[12px] font-[600] text-text">Article detail</div>
          <button
            onClick={onClose}
            className="w-[24px] h-[24px] flex items-center justify-center rounded-[5px] text-text3 hover:text-text hover:bg-white/5 text-[16px]"
          >
            <i className="ti ti-x" />
          </button>
        </div>

        {/* Drawer body */}
        <div className="flex-1 overflow-y-auto p-[18px] flex flex-col gap-[14px]">
          {loading && (
            <div className="text-text3 text-[12px] font-mono py-4">Loading…</div>
          )}

          {!loading && detail && (
            <>
              {/* Title + severity */}
              <div className="flex items-start gap-[10px]">
                <div className="flex-1">
                  <div className="text-[14px] font-[600] text-text leading-[1.4] mb-[4px]">
                    {detail.articleTitle}
                  </div>
                  <div className="text-[10px] font-mono text-text3">
                    {detail.sourceName} · {detail.category}
                  </div>
                </div>
                <Badge variant={detail.severityScore >= 8 ? 'sev-high' : 'sev-mid'}>
                  Sev {detail.severityScore}/10
                </Badge>
              </div>

              {/* Flag reason */}
              <div
                className="text-[11px] text-text2 p-[10px] rounded-[7px]"
                style={{ background: 'var(--bg3)', border: '1px solid var(--border)' }}
              >
                <div className="text-[10px] font-mono text-text3 mb-[4px] uppercase tracking-wider">
                  Flag reason
                </div>
                <i className="ti ti-info-circle text-text3 mr-[5px]" />
                {detail.flagReason}
              </div>

              {/* Trigger keywords */}
              {detail.triggerKeywords?.length > 0 && (
                <div>
                  <div className="text-[10px] font-mono text-text3 mb-[6px] uppercase tracking-wider">
                    Trigger keywords
                  </div>
                  <div className="flex flex-wrap gap-[4px]">
                    {detail.triggerKeywords.map((kw) => (
                      <span
                        key={kw}
                        className="text-[9px] font-mono px-[6px] py-[2px] rounded-[4px]"
                        style={{
                          background: 'rgba(255,183,0,.1)',
                          color: 'var(--yellow)',
                          border: '1px solid rgba(255,183,0,.25)',
                        }}
                      >
                        {kw}
                      </span>
                    ))}
                  </div>
                </div>
              )}

              {/* Article content */}
              <div>
                <div className="text-[10px] font-mono text-text3 mb-[6px] uppercase tracking-wider">
                  Article content
                </div>
                <div
                  className="text-[12px] text-text2 leading-[1.8] p-[14px] rounded-[7px] font-mono whitespace-pre-wrap"
                  style={{ background: 'var(--bg)', border: '1px solid var(--border)' }}
                >
                  {detail.contentMd}
                </div>
              </div>

              {/* Audit log */}
              {detail.auditLogs?.length > 0 && (
                <div>
                  <div className="text-[10px] font-mono text-text3 mb-[6px] uppercase tracking-wider">
                    Audit log
                  </div>
                  <div className="flex flex-col gap-[4px]">
                    {detail.auditLogs.map((log, i) => (
                      <div
                        key={i}
                        className="text-[10px] font-mono text-text3 px-[10px] py-[6px] rounded-[5px]"
                        style={{ background: 'var(--bg3)', border: '1px solid var(--border)' }}
                      >
                        <span className="text-text2 font-[500]">{log.action}</span>
                        {log.notes && <span className="ml-[8px] text-text3">— {log.notes}</span>}
                        <span className="ml-[8px]">{relativeTime(log.timestamp)}</span>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </>
          )}
        </div>

        {/* Drawer actions */}
        <div
          className="flex-shrink-0 flex gap-[6px] px-[18px] py-[12px]"
          style={{ borderTop: '1px solid var(--border)', background: 'var(--bg3)' }}
        >
          <button
            onClick={() => act('approve')}
            disabled={busy}
            className="text-[10px] px-[12px] py-[6px] rounded-[6px] font-[500] cursor-pointer disabled:opacity-50"
            style={{
              background: 'rgba(0,229,160,.15)',
              color: 'var(--accent)',
              border: '1px solid rgba(0,229,160,.3)',
            }}
          >
            <i className="ti ti-check mr-[4px]" />
            {acting === 'approve' ? 'Approving…' : 'Approve & post'}
          </button>
          <button
            onClick={() => act('reject')}
            disabled={busy}
            className="text-[10px] px-[12px] py-[6px] rounded-[6px] font-[500] cursor-pointer disabled:opacity-50"
            style={{
              background: 'rgba(255,69,96,.1)',
              color: 'var(--red)',
              border: '1px solid rgba(255,69,96,.2)',
            }}
          >
            <i className="ti ti-x mr-[4px]" />
            {acting === 'reject' ? 'Rejecting…' : 'Reject'}
          </button>
          <button
            onClick={() => act('escalate')}
            disabled={busy}
            className="text-[10px] px-[12px] py-[6px] rounded-[6px] font-[500] cursor-pointer disabled:opacity-50"
            style={{
              background: 'var(--bg2)',
              color: 'var(--text2)',
              border: '1px solid var(--border)',
            }}
          >
            <i className="ti ti-arrow-up mr-[4px]" />
            {acting === 'escalate' ? 'Escalating…' : 'Escalate'}
          </button>
          <button
            onClick={onClose}
            disabled={busy}
            className="ml-auto text-[10px] px-[10px] py-[6px] rounded-[6px] text-text3 cursor-pointer disabled:opacity-50"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  );
}

export default function ReviewPage() {
  const [queue, setQueue] = useState<Flag[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [acting, setActing] = useState<string | null>(null);
  const [openDetailId, setOpenDetailId] = useState<string | null>(null);

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
      {openDetailId && (
        <DetailDrawer
          flagId={openDetailId}
          onClose={() => setOpenDetailId(null)}
          onAction={act}
        />
      )}

      <Topbar title="Review queue">
        <span className="font-mono text-[10px] text-yellow bg-[rgba(255,183,0,0.10)] border border-[rgba(255,183,0,0.3)] px-[8px] py-[3px] rounded-[20px]">
          {queue.length} pending
        </span>
      </Topbar>

      <div className="flex-1 overflow-y-auto p-[18px_20px] flex flex-col gap-[10px]">
        <div
          className="flex gap-[10px] items-start p-[12px_14px] rounded-[9px]"
          style={{ background: 'rgba(255,183,0,.06)', border: '1px solid rgba(255,183,0,.2)' }}
        >
          <i className="ti ti-alert-triangle text-yellow text-[15px] flex-shrink-0 mt-[1px]" />
          <div className="text-[11px] text-text2">
            <strong className="text-text">Conflict &amp; Geopolitics policy</strong> — articles in this
            category require manual approval before auto-posting. Assess severity, verify sources, and
            approve or reject.
          </div>
        </div>

        {loading && (
          <div className="text-text3 text-[12px] font-mono py-4">Loading queue…</div>
        )}
        {error && (
          <div
            className="text-[12px] p-[10px] rounded-[8px]"
            style={{ background: 'rgba(255,69,96,.1)', border: '1px solid rgba(255,69,96,.3)', color: 'var(--red)' }}
          >
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
            <div
              key={item.id}
              className="rounded-[9px] p-[12px]"
              style={{ background: 'var(--bg3)', border: '1px solid var(--border)' }}
            >
              {/* Header row */}
              <div className="flex items-start justify-between gap-[8px] mb-[4px]">
                <div className="text-[12px] font-[500] text-text leading-[1.4]">{item.title}</div>
                <Badge variant={sevVariant as 'sev-high' | 'sev-mid'}>
                  Sev {item.severity}/10
                </Badge>
              </div>

              {/* Meta row */}
              <div className="text-[10px] text-text3 font-mono mb-[8px]">
                {item.source} · {relativeTime(item.time)} · {item.category}
              </div>

              {/* Reason */}
              <div
                className="text-[11px] text-text2 mb-[6px] p-[8px] rounded-[7px]"
                style={{ background: 'var(--bg2)', border: '1px solid var(--border)' }}
              >
                <i className="ti ti-info-circle text-text3 mr-[5px]" />
                {item.reason}
              </div>

              {/* Trigger keywords */}
              {item.triggerKeywords?.length > 0 && (
                <div className="flex flex-wrap gap-[4px] mb-[10px]">
                  {item.triggerKeywords.map((kw) => (
                    <span
                      key={kw}
                      className="text-[9px] font-mono px-[6px] py-[2px] rounded-[4px]"
                      style={{
                        background: 'rgba(255,183,0,.1)',
                        color: 'var(--yellow)',
                        border: '1px solid rgba(255,183,0,.25)',
                      }}
                    >
                      {kw}
                    </span>
                  ))}
                </div>
              )}

              {/* Actions */}
              <div className="flex gap-[6px] flex-wrap">
                <button
                  onClick={() => setOpenDetailId(item.id)}
                  className="text-[10px] px-[10px] py-[4px] rounded-[6px] font-[500] cursor-pointer"
                  style={{ background: 'var(--bg2)', color: 'var(--text2)', border: '1px solid var(--border)' }}
                >
                  <i className="ti ti-eye mr-[4px]" />View full article
                </button>
                <button
                  onClick={() => act(item.id, 'approve')}
                  disabled={busy}
                  className="text-[10px] px-[10px] py-[4px] rounded-[6px] font-[500] cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                  style={{ background: 'rgba(0,229,160,.15)', color: 'var(--accent)', border: '1px solid rgba(0,229,160,.3)' }}
                >
                  <i className="ti ti-check mr-[4px]" />{busy ? 'Working…' : 'Approve & post'}
                </button>
                <button
                  onClick={() => act(item.id, 'reject')}
                  disabled={busy}
                  className="text-[10px] px-[10px] py-[4px] rounded-[6px] font-[500] cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                  style={{ background: 'rgba(255,69,96,.1)', color: 'var(--red)', border: '1px solid rgba(255,69,96,.2)' }}
                >
                  <i className="ti ti-x mr-[4px]" />Reject
                </button>
                <button
                  onClick={() => act(item.id, 'escalate')}
                  disabled={busy}
                  className="text-[10px] px-[10px] py-[4px] rounded-[6px] font-[500] cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                  style={{ background: 'var(--bg2)', color: 'var(--text2)', border: '1px solid var(--border)' }}
                >
                  <i className="ti ti-arrow-up mr-[4px]" />Escalate
                </button>
                {item.sourceUrl && (
                  <a
                    href={item.sourceUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-[10px] px-[10px] py-[4px] rounded-[6px] font-[500] cursor-pointer ml-auto"
                    style={{ color: 'var(--text3)' }}
                  >
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
