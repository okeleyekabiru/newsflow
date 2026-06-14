'use client';

import { useState, useEffect, useCallback } from 'react';
import Topbar from '@/components/layout/Topbar';
import Panel from '@/components/ui/Panel';
import { api, type Source } from '@/lib/api';

function relativeTime(iso: string | null) {
  if (!iso) return 'Never';
  const m = Math.floor((Date.now() - new Date(iso).getTime()) / 60000);
  if (m < 1) return 'Just now';
  if (m < 60) return `${m}m ago`;
  if (m < 1440) return `${Math.floor(m / 60)}h ago`;
  return `${Math.floor(m / 1440)}d ago`;
}

const TYPE_OPTS = ['RSS', 'Atom', 'JSON Feed'];

export default function SourcesPage() {
  const [sources,  setSources]  = useState<Source[]>([]);
  const [loading,  setLoading]  = useState(true);
  const [error,    setError]    = useState('');
  const [busy,     setBusy]     = useState<string | null>(null);

  // Add-source form state
  const [showForm, setShowForm] = useState(false);
  const [name,     setName]     = useState('');
  const [url,      setUrl]      = useState('');
  const [type,     setType]     = useState('RSS');
  const [adding,   setAdding]   = useState(false);
  const [addError, setAddError] = useState('');

  const load = useCallback(() => {
    setLoading(true);
    setError('');
    api.getSources()
      .then(setSources)
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => { load(); }, [load]);

  async function handleToggle(id: string) {
    setBusy(id);
    try {
      const { isActive } = await api.toggleSource(id);
      setSources((s) => s.map((src) => src.id === id ? { ...src, isActive } : src));
    } catch (e) {
      alert(e instanceof Error ? e.message : 'Toggle failed');
    } finally {
      setBusy(null);
    }
  }

  async function handleRemove(id: string) {
    if (!confirm('Remove this source? It will no longer be used for ingestion.')) return;
    setBusy(id);
    try {
      await api.removeSource(id);
      setSources((s) => s.filter((src) => src.id !== id));
    } catch (e) {
      alert(e instanceof Error ? e.message : 'Remove failed');
    } finally {
      setBusy(null);
    }
  }

  async function handleTrust(id: string) {
    setBusy(id);
    try {
      await api.trustSource(id);
      setSources((s) => s.map((src) => src.id === id ? { ...src, isTrusted: true } : src));
    } catch (e) {
      alert(e instanceof Error ? e.message : 'Trust failed');
    } finally {
      setBusy(null);
    }
  }

  async function handleAdd(e: React.FormEvent) {
    e.preventDefault();
    setAdding(true);
    setAddError('');
    try {
      await api.addSource(name.trim(), url.trim(), type);
      setName(''); setUrl(''); setType('RSS');
      setShowForm(false);
      load();
    } catch (err) {
      setAddError(err instanceof Error ? err.message : 'Failed to add source');
    } finally {
      setAdding(false);
    }
  }

  const active   = sources.filter((s) => s.isActive);
  const inactive = sources.filter((s) => !s.isActive);

  return (
    <>
      <Topbar title="Feed sources">
        <button
          onClick={() => setShowForm((v) => !v)}
          className="flex items-center gap-[5px] px-[10px] py-[5px] rounded-btn text-[11px] font-[500] cursor-pointer"
          style={{ background: 'rgba(0,229,160,.15)', color: 'var(--accent)', border: '1px solid rgba(0,229,160,.3)' }}>
          <i className={`ti ${showForm ? 'ti-x' : 'ti-plus'}`} />
          {showForm ? 'Cancel' : 'Add source'}
        </button>
      </Topbar>

      <div className="flex-1 overflow-y-auto p-[18px_20px] flex flex-col gap-[14px]">

        {/* Add-source form */}
        {showForm && (
          <Panel title="Add new feed source" icon="ti-rss">
            <form onSubmit={handleAdd} className="flex flex-col gap-[10px]">
              <div className="grid grid-cols-2 gap-[10px]">
                <div className="flex flex-col gap-[4px]">
                  <label className="text-[10px] text-text3 font-mono uppercase tracking-[1px]">Source name</label>
                  <input
                    required
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    placeholder="e.g. BBC News"
                    className="text-[12px] px-[10px] py-[7px] rounded-[7px] outline-none focus:ring-1"
                    style={{ background: 'var(--bg)', border: '1px solid var(--border)', color: 'var(--text)' }}
                  />
                </div>
                <div className="flex flex-col gap-[4px]">
                  <label className="text-[10px] text-text3 font-mono uppercase tracking-[1px]">Feed type</label>
                  <select
                    value={type}
                    onChange={(e) => setType(e.target.value)}
                    className="text-[12px] px-[10px] py-[7px] rounded-[7px] outline-none cursor-pointer"
                    style={{ background: 'var(--bg)', border: '1px solid var(--border)', color: 'var(--text)' }}>
                    {TYPE_OPTS.map((t) => <option key={t}>{t}</option>)}
                  </select>
                </div>
              </div>
              <div className="flex flex-col gap-[4px]">
                <label className="text-[10px] text-text3 font-mono uppercase tracking-[1px]">Feed URL</label>
                <input
                  required
                  type="url"
                  value={url}
                  onChange={(e) => setUrl(e.target.value)}
                  placeholder="https://feeds.example.com/rss.xml"
                  className="text-[12px] px-[10px] py-[7px] rounded-[7px] outline-none focus:ring-1 w-full"
                  style={{ background: 'var(--bg)', border: '1px solid var(--border)', color: 'var(--text)' }}
                />
              </div>
              {addError && (
                <div className="text-[11px] p-[8px] rounded-[7px]"
                  style={{ background: 'rgba(255,69,96,.1)', border: '1px solid rgba(255,69,96,.3)', color: 'var(--red)' }}>
                  {addError}
                </div>
              )}
              <button
                type="submit"
                disabled={adding}
                className="self-start text-[11px] px-[14px] py-[6px] rounded-btn font-[500] cursor-pointer disabled:opacity-50"
                style={{ background: 'rgba(0,229,160,.15)', color: 'var(--accent)', border: '1px solid rgba(0,229,160,.3)' }}>
                <i className={`ti ${adding ? 'ti-loader-2 animate-spin' : 'ti-plus'} mr-[5px]`} />
                {adding ? 'Adding…' : 'Add feed'}
              </button>
            </form>
          </Panel>
        )}

        {/* Summary */}
        <div className="grid grid-cols-3 gap-[10px]">
          {[
            { label: 'Total sources',   value: sources.length, icon: 'ti-rss',           color: 'var(--text)' },
            { label: 'Active',          value: active.length,  icon: 'ti-circle-check',  color: 'var(--accent)' },
            { label: 'Paused',          value: inactive.length,icon: 'ti-circle-pause',  color: 'var(--text3)' },
          ].map(({ label, value, icon, color }) => (
            <div key={label} className="rounded-[9px] p-[12px]"
              style={{ background: 'var(--card)', border: '1px solid var(--border)' }}>
              <div className="flex items-center gap-[6px] text-[10px] text-text3 font-mono mb-[4px]">
                <i className={`ti ${icon}`} style={{ color }} /> {label}
              </div>
              <div className="text-[22px] font-[700] font-display" style={{ color }}>{value}</div>
            </div>
          ))}
        </div>

        {loading && <div className="text-text3 text-[12px] font-mono py-4">Loading sources…</div>}
        {error && (
          <div className="text-[12px] p-[10px] rounded-[8px]"
            style={{ background: 'rgba(255,69,96,.1)', border: '1px solid rgba(255,69,96,.3)', color: 'var(--red)' }}>
            {error}
          </div>
        )}

        {/* Source list */}
        {!loading && !error && (
          <Panel title="Configured sources" icon="ti-list">
            {sources.length === 0 && (
              <div className="text-text3 text-[12px] font-mono py-4">No sources yet. Add one above.</div>
            )}
            <div className="flex flex-col divide-y divide-border">
              {sources.map((src) => {
                const isBusy = busy === src.id;
                return (
                  <div key={src.id}
                    className="flex items-center gap-[10px] py-[10px] first:pt-0 last:pb-0">
                    {/* Toggle switch */}
                    <button
                      onClick={() => handleToggle(src.id)}
                      disabled={isBusy}
                      title={src.isActive ? 'Pause feed' : 'Enable feed'}
                      className="relative flex-shrink-0 w-[32px] h-[18px] rounded-[9px] cursor-pointer transition-colors disabled:opacity-50"
                      style={{ background: src.isActive ? 'var(--accent)' : 'var(--border2)' }}>
                      <span
                        className="absolute top-[2px] w-[14px] h-[14px] rounded-full bg-white transition-all"
                        style={{ left: src.isActive ? '16px' : '2px' }}
                      />
                    </button>

                    {/* Source info */}
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-[6px] mb-[2px]">
                        <span className="text-[12px] font-[500] text-text truncate">{src.name}</span>
                        <span className="text-[9px] font-mono px-[5px] py-[1px] rounded-[4px]"
                          style={{ background: 'var(--bg3)', color: 'var(--text3)', border: '1px solid var(--border)' }}>
                          {src.type}
                        </span>
                        {src.isTrusted && (
                          <span className="text-[9px] font-mono px-[5px] py-[1px] rounded-[4px]"
                            style={{ background: 'rgba(0,229,160,.1)', color: 'var(--accent)', border: '1px solid rgba(0,229,160,.3)' }}>
                            <i className="ti ti-shield-check mr-[2px]" />Trusted
                          </span>
                        )}
                        {!src.isActive && (
                          <span className="text-[9px] font-mono px-[5px] py-[1px] rounded-[4px]"
                            style={{ background: 'rgba(255,255,255,.04)', color: 'var(--text3)', border: '1px solid var(--border)' }}>
                            Paused
                          </span>
                        )}
                      </div>
                      <div className="text-[10px] text-text3 font-mono truncate">{src.url}</div>
                    </div>

                    {/* Last fetched */}
                    <div className="text-[10px] text-text3 font-mono flex-shrink-0 hidden sm:block">
                      {relativeTime(src.lastFetchedAt)}
                    </div>

                    {/* Actions */}
                    <div className="flex gap-[4px] flex-shrink-0">
                      {!src.isTrusted && (
                        <button
                          onClick={() => handleTrust(src.id)}
                          disabled={isBusy}
                          title="Mark as trusted (skip safety filter)"
                          className="w-[26px] h-[26px] rounded-[6px] flex items-center justify-center text-text3 hover:text-accent hover:bg-accent/10 transition-colors disabled:opacity-50 cursor-pointer">
                          <i className="ti ti-shield-check text-[13px]" />
                        </button>
                      )}
                      <button
                        onClick={() => handleRemove(src.id)}
                        disabled={isBusy}
                        title="Remove source"
                        className="w-[26px] h-[26px] rounded-[6px] flex items-center justify-center text-text3 hover:text-red hover:bg-red/10 transition-colors disabled:opacity-50 cursor-pointer">
                        <i className={`ti ${isBusy ? 'ti-loader-2 animate-spin' : 'ti-trash'} text-[13px]`} />
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          </Panel>
        )}

        {/* Info box */}
        <div className="flex gap-[10px] items-start p-[10px_14px] rounded-[9px] text-[11px] text-text2"
          style={{ background: 'rgba(0,136,255,.06)', border: '1px solid rgba(0,136,255,.2)' }}>
          <i className="ti ti-info-circle text-accent2 text-[15px] flex-shrink-0 mt-[1px]" />
          <div>
            <strong className="text-text">How ingest works</strong> — the worker polls every 5 minutes. Each active
            source fetches up to 20 items published in the last 24 hours. Trusted sources bypass the safety filter
            and are auto-posted; untrusted sources follow your content filter rules.
          </div>
        </div>
      </div>
    </>
  );
}
