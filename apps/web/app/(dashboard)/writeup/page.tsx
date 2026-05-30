'use client';

import { useState } from 'react';
import Topbar from '@/components/layout/Topbar';
import { api } from '@/lib/api';

const TEMPLATES = [
  { id: 'breaking', label: '⚡ Breaking', desc: 'Fast, punchy, single-source' },
  { id: 'analysis', label: '🔬 Analysis', desc: 'Deep dive, multi-source' },
  { id: 'thread',   label: '🧵 Thread',   desc: 'Twitter-optimised sequence' },
  { id: 'script',   label: '🎬 Script',   desc: 'Video narration script' },
];

const INITIAL_MD = `# G7 Digital Trade Summit — Full Analysis

The G7 summit concluded with a landmark agreement on digital trade regulations that will reshape cross-border data flows for decades...

## Key Outcomes

- Unified framework for cross-border data transfers
- New AI governance standards agreed by all members
- Digital tax harmonisation scheduled for 2026

## Market Reaction

*NASDAQ responded positively*, surging 2.3% in pre-market trading as investors welcomed the regulatory clarity.

---

*Sources: Reuters, Bloomberg, AP News*
`;

const PUBLISH_ACCOUNTS = [
  { id: 'tiktok-1',    icon: 'ti-brand-tiktok',    style: 'plat-tiktok',    handle: '@worldnewsnow',   defaultChecked: true },
  { id: 'twitter-1',  icon: 'ti-brand-twitter',   style: 'plat-twitter',   handle: '@breakingnews_x', defaultChecked: true },
  { id: 'instagram-1',icon: 'ti-brand-instagram', style: 'plat-instagram', handle: '@reelsnews',      defaultChecked: false },
];

export default function WriteupPage() {
  const [content, setContent]             = useState(INITIAL_MD);
  const [activeTemplate, setActiveTemplate] = useState('analysis');
  const [articleId, setArticleId]         = useState<string | null>(null);
  const [selectedAccounts, setSelectedAccounts] = useState<Set<string>>(
    new Set(PUBLISH_ACCOUNTS.filter((a) => a.defaultChecked).map((a) => a.id)),
  );
  const [saving, setSaving]   = useState(false);
  const [publishing, setPublishing] = useState(false);
  const [aiWorking, setAiWorking] = useState<string | null>(null);
  const [toast, setToast]     = useState('');

  const wordCount = content.trim().split(/\s+/).filter(Boolean).length;

  function showToast(msg: string) {
    setToast(msg);
    setTimeout(() => setToast(''), 3000);
  }

  function extractTitle(md: string) {
    const match = md.match(/^#\s+(.+)$/m);
    return match ? match[1].trim() : 'Untitled';
  }

  function extractCategory() {
    return activeTemplate === 'breaking' ? 'Breaking' : 'Analysis';
  }

  async function saveDraft() {
    setSaving(true);
    try {
      const title = extractTitle(content);
      if (articleId) {
        await api.updateArticle(articleId, { title, contentMd: content, template: activeTemplate });
        showToast('Draft saved.');
      } else {
        const article = await api.createArticle({ title, contentMd: content, category: extractCategory(), template: activeTemplate });
        setArticleId(article.id);
        showToast('Draft saved.');
      }
    } catch (e) {
      showToast(e instanceof Error ? e.message : 'Save failed');
    } finally {
      setSaving(false);
    }
  }

  async function publish() {
    setPublishing(true);
    try {
      let id = articleId;
      if (!id) {
        const article = await api.createArticle({
          title: extractTitle(content),
          contentMd: content,
          category: extractCategory(),
          template: activeTemplate,
        });
        id = article.id;
        setArticleId(id);
      } else {
        await api.updateArticle(id, { title: extractTitle(content), contentMd: content });
      }
      await api.publishArticle(id, Array.from(selectedAccounts));
      showToast('Published successfully!');
    } catch (e) {
      showToast(e instanceof Error ? e.message : 'Publish failed');
    } finally {
      setPublishing(false);
    }
  }

  async function rewriteHeadline() {
    setAiWorking('rewrite');
    try {
      const title = extractTitle(content);
      const { headline } = await api.rewriteHeadline(title);
      setContent((c) => c.replace(/^#\s+.+$/m, `# ${headline}`));
      showToast('Headline rewritten.');
    } catch (e) {
      showToast(e instanceof Error ? e.message : 'AI request failed');
    } finally {
      setAiWorking(null);
    }
  }

  async function generateScript() {
    setAiWorking('script');
    try {
      let id = articleId;
      if (!id) {
        const article = await api.createArticle({
          title: extractTitle(content),
          contentMd: content,
          category: extractCategory(),
          template: activeTemplate,
        });
        id = article.id;
        setArticleId(id);
      }
      const { script } = await api.generateScript(id);
      setContent((c) => `${c}\n\n---\n## Video Script\n\n${script}`);
      showToast('Video script appended.');
    } catch (e) {
      showToast(e instanceof Error ? e.message : 'AI request failed');
    } finally {
      setAiWorking(null);
    }
  }

  function toggleAccount(id: string) {
    setSelectedAccounts((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  }

  const aiTools = [
    { key: 'rewrite', icon: 'ti-wand',        label: 'Rewrite headline',     color: 'var(--accent)',   action: rewriteHeadline },
    { key: 'script',  icon: 'ti-player-play',  label: 'Generate video script', color: 'var(--yellow)',  action: generateScript },
  ];

  return (
    <>
      {toast && (
        <div className="fixed bottom-4 right-4 z-50 px-4 py-2 rounded-[8px] text-[12px] font-[500]"
          style={{ background: 'var(--bg3)', border: '1px solid var(--border)', color: 'var(--text)' }}>
          {toast}
        </div>
      )}

      <Topbar title="Write-up studio">
        <button
          onClick={() => showToast('AI assist chat coming soon.')}
          className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px]">
          <i className="ti ti-sparkles text-purple" /> AI assist
        </button>
        <button
          onClick={saveDraft}
          disabled={saving}
          className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px] disabled:opacity-50">
          <i className="ti ti-device-floppy" /> {saving ? 'Saving…' : 'Save draft'}
        </button>
        <button
          onClick={publish}
          disabled={publishing || saving}
          className="flex items-center gap-[5px] px-[11px] py-[6px] rounded-btn text-[11px] font-[500] text-black disabled:opacity-50"
          style={{ background: 'var(--accent)' }}>
          <i className="ti ti-send" /> {publishing ? 'Publishing…' : 'Publish'}
        </button>
      </Topbar>

      <div className="flex-1 overflow-hidden flex gap-0">
        {/* Editor */}
        <div className="flex-1 flex flex-col overflow-hidden" style={{ borderRight: '1px solid var(--border)' }}>
          <div className="flex items-center gap-[4px] px-4 py-[8px] flex-shrink-0"
            style={{ borderBottom: '1px solid var(--border)', background: 'var(--bg2)' }}>
            {['B', 'I', 'H1', 'H2', '"', '—'].map((t) => (
              <button key={t}
                onClick={() => {
                  const ta = document.querySelector('textarea');
                  if (!ta) return;
                  const { selectionStart: s, selectionEnd: e, value: v } = ta;
                  const sel = v.slice(s, e);
                  const wrap: Record<string, string> = { B: '**', I: '_', '"': '> ', H1: '# ', H2: '## ', '—': '— ' };
                  const w = wrap[t] ?? '';
                  const newVal = v.slice(0, s) + w + sel + (t === 'B' || t === 'I' ? w : '') + v.slice(e);
                  setContent(newVal);
                }}
                className="px-[8px] py-[4px] rounded-[5px] text-[11px] font-mono text-text2 hover:text-text hover:bg-white/5">
                {t}
              </button>
            ))}
            <div className="w-[1px] h-[16px] bg-border mx-[4px]" />
            {['ti-list', 'ti-list-numbers', 'ti-link', 'ti-photo'].map((ic) => (
              <button key={ic} className="p-[5px] rounded-[5px] text-text3 hover:text-text hover:bg-white/5">
                <i className={`ti ${ic} text-[13px]`} />
              </button>
            ))}
            <div className="flex-1" />
            <span className="font-mono text-[10px] text-text3">{wordCount} words</span>
          </div>

          <textarea
            className="flex-1 p-6 font-mono text-[13px] text-text2 leading-[1.8] resize-none outline-none border-none"
            style={{ background: 'var(--bg)', caretColor: 'var(--accent)' }}
            value={content}
            onChange={(e) => setContent(e.target.value)}
            spellCheck={false}
          />
        </div>

        {/* Right panel */}
        <div className="w-[280px] flex flex-col flex-shrink-0 overflow-y-auto">
          {/* Template selector */}
          <div className="p-[14px]" style={{ borderBottom: '1px solid var(--border)' }}>
            <div className="text-[11px] font-[600] mb-[8px]">Template</div>
            <div className="flex flex-col gap-[6px]">
              {TEMPLATES.map((t) => (
                <div
                  key={t.id}
                  onClick={() => setActiveTemplate(t.id)}
                  className="px-[10px] py-[8px] rounded-[7px] cursor-pointer text-[11px] flex items-center gap-[8px]"
                  style={activeTemplate === t.id
                    ? { background: 'rgba(0,229,160,.08)', border: '1px solid rgba(0,229,160,.3)', color: 'var(--accent)' }
                    : { background: 'var(--bg3)', border: '1px solid var(--border)', color: 'var(--text2)' }}>
                  <span className="flex-1">{t.label}</span>
                  {activeTemplate === t.id && <i className="ti ti-check text-[12px]" />}
                </div>
              ))}
            </div>
          </div>

          {/* AI tools */}
          <div className="p-[14px]" style={{ borderBottom: '1px solid var(--border)' }}>
            <div className="text-[11px] font-[600] mb-[8px]">AI tools</div>
            <div className="flex flex-col gap-[6px]">
              {aiTools.map((tool) => (
                <button
                  key={tool.key}
                  onClick={tool.action}
                  disabled={aiWorking !== null}
                  className="flex items-center gap-[8px] px-[10px] py-[8px] rounded-[7px] text-[11px] text-text2 hover:text-text transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                  style={{ background: 'var(--bg3)', border: '1px solid var(--border)' }}>
                  <i className={`ti ${tool.icon} text-[13px]`} style={{ color: tool.color }} />
                  {aiWorking === tool.key ? 'Working…' : tool.label}
                </button>
              ))}
              {/* Non-wired tools */}
              {[
                { icon: 'ti-sparkles', label: 'Expand section', color: 'var(--purple)' },
                { icon: 'ti-scissors', label: 'Summarise',       color: 'var(--accent2)' },
              ].map((tool) => (
                <button
                  key={tool.label}
                  onClick={() => showToast(`${tool.label} coming soon.`)}
                  className="flex items-center gap-[8px] px-[10px] py-[8px] rounded-[7px] text-[11px] text-text2 hover:text-text transition-all"
                  style={{ background: 'var(--bg3)', border: '1px solid var(--border)' }}>
                  <i className={`ti ${tool.icon} text-[13px]`} style={{ color: tool.color }} />
                  {tool.label}
                </button>
              ))}
            </div>
          </div>

          {/* Publish targets */}
          <div className="p-[14px]">
            <div className="text-[11px] font-[600] mb-[8px]">Publish to</div>
            {PUBLISH_ACCOUNTS.map((a) => (
              <label key={a.id} className="flex items-center gap-[8px] py-[6px] cursor-pointer">
                <div className={`w-[24px] h-[24px] rounded-[5px] flex items-center justify-center ${a.style}`}>
                  <i className={`ti ${a.icon} text-white text-[11px]`} />
                </div>
                <span className="text-[11px] text-text2 flex-1">{a.handle}</span>
                <input
                  type="checkbox"
                  checked={selectedAccounts.has(a.id)}
                  onChange={() => toggleAccount(a.id)}
                  className="accent-[var(--accent)]"
                />
              </label>
            ))}
          </div>
        </div>
      </div>
    </>
  );
}
