'use client';

import { useState } from 'react';
import Topbar from '@/components/layout/Topbar';

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

export default function WriteupPage() {
  const [content, setContent] = useState(INITIAL_MD);
  const [activeTemplate, setActiveTemplate] = useState('analysis');
  const [wordCount] = useState(Math.round(INITIAL_MD.split(/\s+/).length));

  return (
    <>
      <Topbar title="Write-up studio">
        <button className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px]">
          <i className="ti ti-sparkles text-purple" /> AI assist
        </button>
        <button className="flex items-center gap-[5px] px-[9px] py-[4px] rounded-btn border border-border text-text2 text-[10px]">
          <i className="ti ti-device-floppy" /> Save draft
        </button>
        <button
          className="flex items-center gap-[5px] px-[11px] py-[6px] rounded-btn text-[11px] font-[500] text-black"
          style={{ background: 'var(--accent)' }}
        >
          <i className="ti ti-send" /> Publish
        </button>
      </Topbar>

      <div className="flex-1 overflow-hidden flex gap-0">
        {/* Left: editor */}
        <div className="flex-1 flex flex-col overflow-hidden" style={{ borderRight: '1px solid var(--border)' }}>
          {/* Toolbar */}
          <div className="flex items-center gap-[4px] px-4 py-[8px] flex-shrink-0" style={{ borderBottom: '1px solid var(--border)', background: 'var(--bg2)' }}>
            {['B', 'I', 'H1', 'H2', '"', '—'].map((t) => (
              <button key={t} className="px-[8px] py-[4px] rounded-[5px] text-[11px] font-mono text-text2 hover:text-text hover:bg-white/5">
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

          {/* Textarea */}
          <textarea
            className="flex-1 p-6 font-mono text-[13px] text-text2 leading-[1.8] resize-none outline-none border-none"
            style={{ background: 'var(--bg)', caretColor: 'var(--accent)' }}
            value={content}
            onChange={(e) => setContent(e.target.value)}
            spellCheck={false}
          />
        </div>

        {/* Right: AI panel */}
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
                    : { background: 'var(--bg3)', border: '1px solid var(--border)', color: 'var(--text2)' }}
                >
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
              {[
                { icon: 'ti-wand', label: 'Rewrite headline', color: 'var(--accent)' },
                { icon: 'ti-sparkles', label: 'Expand section', color: 'var(--purple)' },
                { icon: 'ti-scissors', label: 'Summarise', color: 'var(--accent2)' },
                { icon: 'ti-player-play', label: 'Generate video script', color: 'var(--yellow)' },
              ].map((tool) => (
                <button
                  key={tool.label}
                  className="flex items-center gap-[8px] px-[10px] py-[8px] rounded-[7px] text-[11px] text-text2 hover:text-text transition-all"
                  style={{ background: 'var(--bg3)', border: '1px solid var(--border)' }}
                >
                  <i className={`ti ${tool.icon} text-[13px]`} style={{ color: tool.color }} />
                  {tool.label}
                </button>
              ))}
            </div>
          </div>

          {/* Publish targets */}
          <div className="p-[14px]">
            <div className="text-[11px] font-[600] mb-[8px]">Publish to</div>
            {[
              { icon: 'ti-brand-tiktok', style: 'plat-tiktok', handle: '@worldnewsnow', checked: true },
              { icon: 'ti-brand-twitter', style: 'plat-twitter', handle: '@breakingnews_x', checked: true },
              { icon: 'ti-brand-instagram', style: 'plat-instagram', handle: '@reelsnews', checked: false },
            ].map((a) => (
              <label key={a.handle} className="flex items-center gap-[8px] py-[6px] cursor-pointer">
                <div className={`w-[24px] h-[24px] rounded-[5px] flex items-center justify-center ${a.style}`}>
                  <i className={`ti ${a.icon} text-white text-[11px]`} />
                </div>
                <span className="text-[11px] text-text2 flex-1">{a.handle}</span>
                <input type="checkbox" defaultChecked={a.checked} className="accent-[var(--accent)]" />
              </label>
            ))}
          </div>
        </div>
      </div>
    </>
  );
}
