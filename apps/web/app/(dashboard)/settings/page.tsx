'use client';

import { useEffect, useMemo, useState } from 'react';
import Topbar from '@/components/layout/Topbar';
import Panel from '@/components/ui/Panel';
import {
  api,
  ARTICLE_CATEGORIES,
  type ArticleCategory,
  type ContentDecision,
  type FlagRule,
} from '@/lib/api';

// ── Static, display-only sections (no backend yet) ────────────────────────────
const STATIC_SECTIONS = [
  {
    section: 'AI & Content',
    rows: [
      { icon: 'ti-brain', label: 'AI model', sub: 'Primary model for rewriting', value: 'Claude Sonnet 4.6' },
      { icon: 'ti-language', label: 'Output language', sub: 'Caption and article language', value: 'English (US)' },
      { icon: 'ti-refresh', label: 'Ingest frequency', sub: 'How often to pull RSS feeds', value: 'Every 5 min' },
    ],
  },
  {
    section: 'Video generation',
    rows: [
      { icon: 'ti-microphone', label: 'Voice (ElevenLabs)', sub: 'TTS voice model', value: 'Rachel — Calm' },
      { icon: 'ti-video', label: 'Stock footage', sub: 'Footage provider', value: 'Pexels HD' },
      { icon: 'ti-device-mobile', label: 'Default aspect ratio', sub: 'Target platform format', value: '9:16 (Vertical)' },
    ],
  },
  {
    section: 'Notifications',
    rows: [
      { icon: 'ti-mail', label: 'Email alerts', sub: 'Receive digest emails', value: 'Daily' },
      { icon: 'ti-bell', label: 'Review alerts', sub: 'Notify when items need review', value: 'Instant' },
    ],
  },
];

const DECISIONS: ContentDecision[] = ['AutoPost', 'FlagForReview', 'Block'];

const DECISION_LABEL: Record<ContentDecision, string> = {
  AutoPost: 'Auto-post',
  FlagForReview: 'Review required',
  Block: 'Auto-block',
};

const CATEGORY_LABEL: Record<ArticleCategory, string> = {
  Politics: 'Politics',
  Finance: 'Finance',
  Technology: 'Technology',
  Sports: 'Sports',
  Health: 'Health',
  Entertainment: 'Entertainment',
  Weather: 'Weather',
  Science: 'Science',
  ConflictAndWar: 'Conflict & War',
  Terrorism: 'Terrorism',
  General: 'General',
};

interface RuleForm {
  category: ArticleCategory;
  defaultDecision: ContentDecision;
  trustedSources: string;
  blockedKeywords: string;
  severityThreshold: number;
  escalationEmail: string;
  autoPostTrustedSources: boolean;
}

function blankForm(category: ArticleCategory): RuleForm {
  return {
    category,
    defaultDecision: 'FlagForReview',
    trustedSources: '',
    blockedKeywords: '',
    severityThreshold: 5,
    escalationEmail: '',
    autoPostTrustedSources: false,
  };
}

function formFromRule(rule: FlagRule): RuleForm {
  return {
    category: rule.category,
    defaultDecision: rule.defaultDecision,
    trustedSources: rule.trustedSources.join(', '),
    blockedKeywords: rule.blockedKeywords.join(', '),
    severityThreshold: rule.severityThreshold,
    escalationEmail: rule.escalationEmail ?? '',
    autoPostTrustedSources: rule.autoPostTrustedSources,
  };
}

function splitList(value: string): string[] {
  return value
    .split(',')
    .map((s) => s.trim())
    .filter(Boolean);
}

export default function SettingsPage() {
  const [rules, setRules] = useState<FlagRule[]>([]);
  const [category, setCategory] = useState<ArticleCategory>('ConflictAndWar');
  const [form, setForm] = useState<RuleForm>(blankForm('ConflictAndWar'));
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [saved, setSaved] = useState(false);

  // Load existing rules once.
  useEffect(() => {
    api
      .getFlagRules()
      .then((data) => {
        setRules(data);
        const first = data[0]?.category ?? 'ConflictAndWar';
        setCategory(first);
        const match = data.find((r) => r.category === first);
        setForm(match ? formFromRule(match) : blankForm(first));
      })
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, []);

  // Repopulate the form whenever the selected category changes.
  function selectCategory(next: ArticleCategory) {
    setCategory(next);
    setSaved(false);
    const match = rules.find((r) => r.category === next);
    setForm(match ? formFromRule(match) : blankForm(next));
  }

  const configuredCategories = useMemo(
    () => new Set(rules.map((r) => r.category)),
    [rules],
  );

  async function save() {
    setSaving(true);
    setError('');
    setSaved(false);
    try {
      await api.updateFlagRule({
        category: form.category,
        defaultDecision: form.defaultDecision,
        trustedSources: splitList(form.trustedSources),
        blockedKeywords: splitList(form.blockedKeywords),
        severityThreshold: form.severityThreshold,
        escalationEmail: form.escalationEmail.trim() || null,
        autoPostTrustedSources: form.autoPostTrustedSources,
      });
      // Refresh so the "configured" markers stay accurate.
      const fresh = await api.getFlagRules();
      setRules(fresh);
      setSaved(true);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Save failed');
    } finally {
      setSaving(false);
    }
  }

  const inputStyle = {
    background: 'var(--bg2)',
    border: '1px solid var(--border)',
    color: 'var(--text)',
  } as const;

  return (
    <>
      <Topbar title="Settings">
        <button
          onClick={save}
          disabled={saving || loading}
          className="flex items-center gap-[5px] px-[11px] py-[6px] rounded-btn text-[11px] font-[500] text-black disabled:opacity-50 disabled:cursor-not-allowed"
          style={{ background: 'var(--accent)' }}
        >
          <i className="ti ti-device-floppy" /> {saving ? 'Saving…' : 'Save changes'}
        </button>
      </Topbar>

      <div className="flex-1 overflow-y-auto p-[18px_20px] flex flex-col gap-[14px]">
        {/* ── Content filters (live) ──────────────────────────────────────── */}
        <Panel title="Content filters" icon="ti-shield-check" className="shrink-0">
          {loading ? (
            <div className="text-text3 text-[12px] font-mono py-4">Loading rules…</div>
          ) : (
            <div className="flex flex-col gap-[12px]">
              <p className="text-[10px] text-text3 font-mono">
                Per-category rules drive the safety pipeline. Pick a category, adjust its rule, then save.
              </p>

              {/* Category selector */}
              <div className="flex flex-wrap gap-[5px]">
                {ARTICLE_CATEGORIES.map((c) => {
                  const active = c === category;
                  const configured = configuredCategories.has(c);
                  return (
                    <button
                      key={c}
                      onClick={() => selectCategory(c)}
                      className="flex items-center gap-[5px] font-mono text-[10px] px-[9px] py-[4px] rounded-[20px]"
                      style={{
                        background: active ? 'rgba(0,229,160,.12)' : 'var(--bg3)',
                        border: `1px solid ${active ? 'rgba(0,229,160,.4)' : 'var(--border)'}`,
                        color: active ? 'var(--accent)' : 'var(--text2)',
                      }}
                    >
                      {CATEGORY_LABEL[c]}
                      {configured && (
                        <i
                          className="ti ti-point-filled text-[10px]"
                          style={{ color: active ? 'var(--accent)' : 'var(--text3)' }}
                          title="Custom rule configured"
                        />
                      )}
                    </button>
                  );
                })}
              </div>

              {/* Editable rule form */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-[12px]">
                <label className="flex flex-col gap-[4px]">
                  <span className="text-[10px] text-text3 font-mono">Default decision</span>
                  <select
                    value={form.defaultDecision}
                    onChange={(e) =>
                      setForm({ ...form, defaultDecision: e.target.value as ContentDecision })
                    }
                    className="text-[11px] px-[10px] py-[6px] rounded-[7px] font-mono"
                    style={inputStyle}
                  >
                    {DECISIONS.map((d) => (
                      <option key={d} value={d}>
                        {DECISION_LABEL[d]}
                      </option>
                    ))}
                  </select>
                </label>

                <label className="flex flex-col gap-[4px]">
                  <span className="text-[10px] text-text3 font-mono">
                    Sensitivity threshold ({form.severityThreshold}/10)
                  </span>
                  <input
                    type="range"
                    min={1}
                    max={10}
                    value={form.severityThreshold}
                    onChange={(e) =>
                      setForm({ ...form, severityThreshold: Number(e.target.value) })
                    }
                    className="accent-accent h-[32px]"
                  />
                </label>

                <label className="flex flex-col gap-[4px] md:col-span-2">
                  <span className="text-[10px] text-text3 font-mono">Blocked keywords (comma-separated)</span>
                  <input
                    type="text"
                    value={form.blockedKeywords}
                    onChange={(e) => setForm({ ...form, blockedKeywords: e.target.value })}
                    placeholder="e.g. attack, casualties, militant"
                    className="text-[11px] px-[10px] py-[6px] rounded-[7px] font-mono"
                    style={inputStyle}
                  />
                </label>

                <label className="flex flex-col gap-[4px] md:col-span-2">
                  <span className="text-[10px] text-text3 font-mono">Trusted sources (comma-separated)</span>
                  <input
                    type="text"
                    value={form.trustedSources}
                    onChange={(e) => setForm({ ...form, trustedSources: e.target.value })}
                    placeholder="e.g. Reuters, AP, BBC"
                    className="text-[11px] px-[10px] py-[6px] rounded-[7px] font-mono"
                    style={inputStyle}
                  />
                </label>

                <label className="flex flex-col gap-[4px]">
                  <span className="text-[10px] text-text3 font-mono">Escalation email</span>
                  <input
                    type="email"
                    value={form.escalationEmail}
                    onChange={(e) => setForm({ ...form, escalationEmail: e.target.value })}
                    placeholder="editor@newsflow.io"
                    className="text-[11px] px-[10px] py-[6px] rounded-[7px] font-mono"
                    style={inputStyle}
                  />
                </label>

                <label className="flex items-center gap-[8px] mt-[20px]">
                  <input
                    type="checkbox"
                    checked={form.autoPostTrustedSources}
                    onChange={(e) =>
                      setForm({ ...form, autoPostTrustedSources: e.target.checked })
                    }
                    className="accent-accent w-[14px] h-[14px]"
                  />
                  <span className="text-[11px] text-text2">Auto-post from trusted sources</span>
                </label>
              </div>

              {error && (
                <div
                  className="text-[11px] p-[8px] rounded-[7px]"
                  style={{
                    background: 'rgba(255,69,96,.1)',
                    border: '1px solid rgba(255,69,96,.3)',
                    color: 'var(--red)',
                  }}
                >
                  {error}
                </div>
              )}
              {saved && (
                <div
                  className="text-[11px] p-[8px] rounded-[7px] flex items-center gap-[6px]"
                  style={{
                    background: 'rgba(0,229,160,.1)',
                    border: '1px solid rgba(0,229,160,.3)',
                    color: 'var(--accent)',
                  }}
                >
                  <i className="ti ti-check" /> Rule for {CATEGORY_LABEL[category]} saved.
                </div>
              )}
            </div>
          )}
        </Panel>

        {/* ── Static display-only sections ────────────────────────────────── */}
        {STATIC_SECTIONS.map(({ section, rows }) => (
          <Panel key={section} title={section} icon="ti-settings" className="shrink-0">
            <div className="flex flex-col divide-y divide-border">
              {rows.map((r) => (
                <div
                  key={r.label}
                  className="flex items-center justify-between py-[9px] first:pt-0 last:pb-0"
                >
                  <div className="flex items-center gap-[7px]">
                    <i className={`ti ${r.icon} text-text3 text-[13px]`} />
                    <div>
                      <div className="text-[12px]">{r.label}</div>
                      {r.sub && <div className="text-[10px] text-text3 font-mono">{r.sub}</div>}
                    </div>
                  </div>
                  <div
                    className="flex items-center gap-[5px] font-mono text-[10px] px-[10px] py-[4px] rounded-[20px]"
                    style={{ background: 'var(--bg3)', border: '1px solid var(--border)', color: 'var(--text2)' }}
                  >
                    {r.value}
                  </div>
                </div>
              ))}
            </div>
          </Panel>
        ))}

        {/* API keys (display only) */}
        <Panel title="API keys" icon="ti-key" className="shrink-0">
          <div className="flex flex-col gap-[8px]">
            {[
              { label: 'Anthropic (Claude)', key: 'sk-ant-••••••••••••••••••••••1a4f', connected: true },
              { label: 'ElevenLabs', key: 'el_••••••••••••••••••••••3b2c', connected: true },
              { label: 'Pexels', key: 'Not configured', connected: false },
              { label: 'NewsAPI.org', key: 'Not configured', connected: false },
            ].map((k) => (
              <div
                key={k.label}
                className="flex items-center gap-[10px] p-[10px] rounded-[8px]"
                style={{ background: 'var(--bg3)', border: '1px solid var(--border)' }}
              >
                <div className="flex-1">
                  <div className="text-[11px] font-[500]">{k.label}</div>
                  <div className="text-[10px] font-mono text-text3 mt-[1px]">{k.key}</div>
                </div>
                <span
                  className={`font-mono text-[9px] px-[7px] py-[2px] rounded-[20px] ${
                    k.connected
                      ? 'text-accent bg-[rgba(0,229,160,.1)] border border-[rgba(0,229,160,.3)]'
                      : 'text-text3 bg-bg2 border border-border'
                  }`}
                >
                  {k.connected ? 'Connected' : 'Not set'}
                </span>
              </div>
            ))}          </div>
        </Panel>
      </div>
    </>
  );
}
