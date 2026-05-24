window.render_writeup = function(container) {
  container.style.overflow = 'hidden';
  container.style.display = 'flex';
  container.style.flexDirection = 'column';
  container.style.flex = '1';
  container.style.minHeight = '0';

  const TEMPLATES = {
    breaking: `# [BREAKING] G7 Digital Trade Summit\n\n**By:** Reporter Name | **Source:** Reuters | **${new Date().toLocaleDateString()}**\n\n---\n\n## What happened\n\nLead paragraph — the most important facts first.\n\n## Key details\n\n- Detail one\n- Detail two\n- Detail three\n\n## Official response\n\n> "Quote from official here." — Name, Title\n\n## What happens next\n\nFollow-up context.\n\n---\n*Developing story. Updates to follow.*`,
    analysis: `# Analysis: G7 Digital Trade Summit\n\n**By:** Analyst | **${new Date().toLocaleDateString()}**\n\n---\n\n## Overview\n\nBrief summary.\n\n## Background\n\nContext and history.\n\n## Key findings\n\n- Finding one\n- Finding two\n\n## Conclusion\n\nSummary and takeaways.`,
    opinion: `# Opinion: [Headline]\n\n**By:** Author | **${new Date().toLocaleDateString()}**\n\n---\n\n## The argument\n\nYour position.\n\n## Why this matters\n\nStakes and context.\n\n## Conclusion\n\nRestate your position.`,
    explainer: `# Explainer: What is [Topic]?\n\n**${new Date().toLocaleDateString()}** | 5 min read\n\n---\n\n## The short answer\n\nOne paragraph summary.\n\n## How it works\n\nStep-by-step.\n\n## FAQ\n\n**Q: Question?**\nA: Answer.`,
    social: `# Social caption draft\n\n---\n\n## TikTok / Instagram\n\n[Hook line]\n\n[2-3 lines of context]\n\n[Call to action]\n\n#hashtag1 #hashtag2 #hashtag3`,
    interview: `# Interview: Name, Title\n\n**${new Date().toLocaleDateString()}**\n\n---\n\n**Q: Opening question?**\n\n**A:** Answer.\n\n---\n\n**Q: Follow-up?**\n\n**A:** Answer.`
  };

  let versions = [];
  let wSaveTimer;
  let wIsGen = false;

  container.innerHTML = `
<style>
.writer-topbar{display:flex;align-items:center;gap:7px;padding:8px 14px;background:var(--bg2);border-bottom:1px solid var(--border);flex-shrink:0}
.doc-input{flex:1;background:transparent;border:none;color:var(--text);font-size:13px;font-weight:500;outline:none;font-family:'DM Sans',sans-serif;min-width:0}
.doc-input::placeholder{color:var(--text3)}
.wbadge{font-size:9px;font-family:'DM Mono',monospace;padding:2px 6px;border-radius:20px;font-weight:500}
.bmd{background:rgba(0,136,255,.15);color:#60a5fa;border:1px solid rgba(0,136,255,.25)}
.bsaved{background:rgba(0,229,160,.1);color:var(--accent);border:1px solid rgba(0,229,160,.2);display:flex;align-items:center;gap:3px}
.writer-body{flex:1;display:grid;grid-template-columns:180px 1fr 1fr 250px;overflow:hidden;min-height:0}
.wpane{display:flex;flex-direction:column;border-right:1px solid var(--border);overflow:hidden}
.wpane:last-child{border-right:none}
.wpane-hdr{display:flex;align-items:center;gap:6px;padding:6px 11px;background:var(--bg3);border-bottom:1px solid var(--border);flex-shrink:0;font-size:9px;font-family:'DM Mono',monospace;color:var(--text3);text-transform:uppercase;letter-spacing:1.5px}
.wpane-body{flex:1;overflow-y:auto;min-height:0}
.wpane-body::-webkit-scrollbar{width:3px}
.wpane-body::-webkit-scrollbar-thumb{background:var(--border2);border-radius:3px}
.tmpl-card{background:var(--bg3);border:1px solid var(--border);border-radius:7px;padding:9px;margin:6px;cursor:pointer;transition:all .15s}
.tmpl-card:hover{border-color:var(--accent)}
.tmpl-card.active{border-color:var(--accent);background:rgba(0,229,160,.06)}
.tc-name{font-size:11px;font-weight:500;color:var(--text);margin-top:3px}
.tc-desc{font-size:9px;color:var(--text3);font-family:'DM Mono',monospace;margin-top:2px;line-height:1.3}
.editor-area{width:100%;height:100%;background:var(--bg);border:none;color:var(--text);font-family:'DM Mono',monospace;font-size:11px;line-height:1.8;padding:12px;resize:none;outline:none}
.editor-area::placeholder{color:var(--text3)}
.preview-body{padding:14px;font-size:11px;line-height:1.8;color:var(--text2);overflow-y:auto;height:100%}
.preview-body::-webkit-scrollbar{width:3px}
.preview-body::-webkit-scrollbar-thumb{background:var(--border2);border-radius:3px}
.preview-body h1{font-family:'Syne',sans-serif;font-size:17px;font-weight:800;color:var(--text);margin:0 0 8px;padding-bottom:6px;border-bottom:1px solid var(--border)}
.preview-body h2{font-family:'Syne',sans-serif;font-size:14px;font-weight:700;color:var(--text);margin:14px 0 5px}
.preview-body h3{font-size:12px;font-weight:600;color:var(--accent);margin:10px 0 4px}
.preview-body p{margin:0 0 7px;font-size:11px}
.preview-body ul{margin:0 0 7px 14px}
.preview-body li{margin-bottom:2px;font-size:11px}
.preview-body strong{color:var(--text);font-weight:600}
.preview-body em{font-style:italic}
.preview-body blockquote{border-left:3px solid var(--accent);padding:5px 10px;background:rgba(0,229,160,.05);border-radius:0 6px 6px 0;margin:8px 0;font-style:italic}
.preview-body code{font-family:'DM Mono',monospace;font-size:10px;background:var(--bg3);border:1px solid var(--border);border-radius:3px;padding:1px 4px;color:var(--purple)}
.preview-body hr{border:none;border-top:1px solid var(--border);margin:10px 0}
.chat-panel{background:var(--bg2);display:flex;flex-direction:column;overflow:hidden}
.rp-tabs{display:flex;border-bottom:1px solid var(--border);flex-shrink:0}
.rp-tab{flex:1;text-align:center;padding:7px 4px;font-size:9px;font-family:'DM Mono',monospace;color:var(--text3);cursor:pointer;border-bottom:2px solid transparent;transition:all .15s;letter-spacing:.5px;text-transform:uppercase}
.rp-tab.active{color:var(--accent);border-bottom-color:var(--accent)}
.rp-panel{flex:1;overflow:hidden;display:none;flex-direction:column}
.rp-panel.show{display:flex}
.chat-msgs{flex:1;overflow-y:auto;padding:8px;display:flex;flex-direction:column;gap:7px}
.chat-msgs::-webkit-scrollbar{width:3px}
.chat-msgs::-webkit-scrollbar-thumb{background:var(--border2);border-radius:3px}
.wmsg{display:flex;gap:6px;align-items:flex-start}
.wmav{width:22px;height:22px;border-radius:6px;display:flex;align-items:center;justify-content:center;font-size:9px;font-weight:700;flex-shrink:0;font-family:'DM Mono',monospace}
.wmai .wmav{background:rgba(0,229,160,.15);color:var(--accent);border:1px solid rgba(0,229,160,.2)}
.wmuser .wmav{background:rgba(0,136,255,.15);color:#60a5fa;border:1px solid rgba(0,136,255,.2)}
.wmbubble{font-size:10px;line-height:1.6;color:var(--text2);flex:1;min-width:0}
.wmuser .wmbubble{color:var(--text)}
.wmbubble strong{color:var(--accent);font-weight:500}
.wmcode{background:var(--bg3);border:1px solid var(--border);border-radius:6px;padding:6px 8px;font-size:9px;font-family:'DM Mono',monospace;color:var(--text2);white-space:pre-wrap;margin-top:5px;max-height:90px;overflow-y:auto;line-height:1.5}
.wmaction{display:inline-flex;align-items:center;gap:3px;font-size:9px;font-family:'DM Mono',monospace;background:rgba(0,229,160,.08);color:var(--accent);border:1px solid rgba(0,229,160,.2);border-radius:5px;padding:2px 6px;margin-top:4px;cursor:pointer}
.wmaction:hover{background:rgba(0,229,160,.16)}
.wthinking{display:flex;align-items:center;gap:5px;padding:6px 8px;background:rgba(0,229,160,.04);border:1px solid rgba(0,229,160,.1);border-radius:6px;font-size:9px;color:var(--accent);font-family:'DM Mono',monospace}
.wdots span{display:inline-block;width:3px;height:3px;border-radius:50%;background:var(--accent);margin:0 1px;animation:bounce 1.2s infinite}
.wdots span:nth-child(2){animation-delay:.2s}
.wdots span:nth-child(3){animation-delay:.4s}
.chat-foot{padding:7px;border-top:1px solid var(--border);flex-shrink:0}
.wchips{display:flex;flex-wrap:wrap;gap:3px;margin-bottom:5px}
.wchip{font-size:9px;padding:2px 6px;border-radius:20px;background:var(--bg3);border:1px solid var(--border);color:var(--text3);cursor:pointer;font-family:'DM Mono',monospace;transition:all .15s}
.wchip:hover{border-color:var(--accent);color:var(--accent)}
.winp-row{display:flex;gap:4px;align-items:flex-end}
.wcinput{flex:1;background:var(--bg3);border:1px solid var(--border);border-radius:6px;color:var(--text);font-family:'DM Sans',sans-serif;font-size:10px;padding:6px 8px;outline:none;resize:none;max-height:60px;line-height:1.5}
.wcinput:focus{border-color:var(--border2)}
.wcinput::placeholder{color:var(--text3)}
.wsbtn{width:28px;height:28px;border-radius:6px;background:var(--accent);border:none;cursor:pointer;display:flex;align-items:center;justify-content:center;flex-shrink:0}
.wsbtn:hover{background:#00ffb2}
.wsbtn i{color:#000;font-size:13px}
.ver-item{background:var(--bg3);border:1px solid var(--border);border-radius:7px;padding:9px;margin-bottom:5px;cursor:pointer}
.ver-item:hover{border-color:var(--border2)}
.ver-item.aver{border-color:var(--accent)}
.ver-label{font-size:11px;font-weight:500;color:var(--text)}
.ver-time{font-size:9px;font-family:'DM Mono',monospace;color:var(--text3)}
.ver-diff-row{display:flex;gap:6px;font-size:9px;font-family:'DM Mono',monospace;margin-top:3px}
.vdiff-a{color:var(--accent)}
.vdiff-d{color:var(--red)}
.ver-preview{font-size:9px;color:var(--text3);margin-top:4px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.ver-btn-sm{font-size:9px;padding:2px 7px;border-radius:4px;border:1px solid var(--border);background:transparent;color:var(--text2);cursor:pointer;font-family:'DM Mono',monospace;margin-top:5px}
.ver-btn-sm:hover{border-color:var(--accent);color:var(--accent)}
.plat-bar{display:flex;gap:5px;padding:5px 10px;background:var(--bg2);border-top:1px solid var(--border);flex-shrink:0;align-items:center;flex-wrap:wrap}
.pchip{display:flex;align-items:center;gap:4px;padding:3px 8px;border-radius:20px;font-size:9px;font-family:'DM Mono',monospace;border:1px solid var(--border);background:var(--bg3);white-space:nowrap}
.pchip.ok{border-color:var(--accent);color:var(--accent);background:rgba(0,229,160,.07)}
.pchip.warn{border-color:var(--yellow);color:var(--yellow);background:rgba(255,183,0,.07)}
.pchip.over{border-color:var(--red);color:var(--red);background:rgba(255,69,96,.07)}
.pbar-wrap{width:40px;height:3px;background:var(--border);border-radius:2px}
.pbar{height:100%;border-radius:2px;transition:width .3s}
</style>

<div class="writer-topbar">
  <i class="ti ti-pencil" style="color:var(--text3);font-size:13px"></i>
  <input class="doc-input" id="wDocTitle" value="G7 Digital Trade Summit — Full Analysis" placeholder="Untitled article...">
  <span class="wbadge bmd">MD</span>
  <span class="wbadge bsaved" id="wSavedBadge"><i class="ti ti-check" style="font-size:9px"></i>Saved</span>
  <button class="btn btn-ghost" onclick="wExportMD()" style="font-size:10px;padding:4px 9px"><i class="ti ti-download"></i>Export .md</button>
  <button class="btn btn-ghost" style="font-size:10px;padding:4px 9px"><i class="ti ti-brand-tiktok"></i>Post</button>
  <button class="btn btn-primary" style="font-size:10px;padding:4px 9px"><i class="ti ti-send"></i>Publish</button>
</div>

<div class="writer-body">
  <div class="wpane">
    <div class="wpane-hdr"><i class="ti ti-template"></i>Templates</div>
    <div class="wpane-body" style="padding:6px" id="wTmplList"></div>
  </div>
  <div class="wpane" style="min-width:0">
    <div class="wpane-hdr" style="justify-content:space-between">
      <span><i class="ti ti-code"></i>Markdown editor</span>
      <span id="wCursorPos" style="font-size:9px">Ln 1</span>
    </div>
    <div style="display:flex;gap:2px;padding:4px 8px;background:var(--bg2);border-bottom:1px solid var(--border);flex-wrap:wrap">
      <button class="ver-btn-sm" onclick="wIns('## ','')">H2</button>
      <button class="ver-btn-sm" onclick="wIns('**','**')"><b>B</b></button>
      <button class="ver-btn-sm" onclick="wIns('*','*')"><i>I</i></button>
      <button class="ver-btn-sm" onclick="wIns('> ','')">&#10077;</button>
      <button class="ver-btn-sm" onclick="wIns('- ','')">&#8226;</button>
      <button class="ver-btn-sm" onclick="wIns('\`','\`')">&lt;/&gt;</button>
      <button class="ver-btn-sm" onclick="wIns('\n---\n','')">HR</button>
      <span style="flex:1"></span>
      <span id="wWordCount" style="font-size:9px;font-family:'DM Mono',monospace;color:var(--text3);padding:2px 4px">0 words</span>
    </div>
    <div style="flex:1;overflow:hidden;min-height:0">
      <textarea class="editor-area" id="wEditor" oninput="wOnEdit()" onclick="wCursor()" onkeyup="wCursor()" placeholder="Start writing or use the AI assistant..."></textarea>
    </div>
  </div>
  <div class="wpane" style="min-width:0">
    <div class="wpane-hdr" style="justify-content:space-between">
      <span><i class="ti ti-eye"></i>Live preview</span>
      <span id="wReadTime" style="font-size:9px;color:var(--text3)">0 min read</span>
    </div>
    <div class="preview-body" id="wPreview"></div>
  </div>
  <div class="wpane chat-panel">
    <div class="rp-tabs">
      <div class="rp-tab active" onclick="wSwitchTab('chat',this)">AI chat</div>
      <div class="rp-tab" onclick="wSwitchTab('versions',this)">Versions</div>
      <div class="rp-tab" onclick="wSwitchTab('diff',this)">Diff</div>
    </div>
    <div id="wRpChat" class="rp-panel show">
      <div class="chat-msgs" id="wChatMsgs"></div>
      <div class="chat-foot">
        <div class="wchips">
          <div class="wchip" onclick="wTp('Write a compelling introduction')">Intro</div>
          <div class="wchip" onclick="wTp('Add key findings as bullet points')">Key findings</div>
          <div class="wchip" onclick="wTp('Write a strong conclusion')">Conclusion</div>
          <div class="wchip" onclick="wTp('Add quotes from officials')">Add quotes</div>
          <div class="wchip" onclick="wTp('Improve writing tone and clarity')">Improve tone</div>
          <div class="wchip" onclick="wTp('Write a social media caption')">Social caption</div>
        </div>
        <div class="winp-row">
          <textarea class="wcinput" id="wChatInput" rows="2" placeholder="Ask AI to write, improve, restructure..." onkeydown="wHandleKey(event)"></textarea>
          <button class="wsbtn" onclick="wSendMsg()"><i class="ti ti-send"></i></button>
        </div>
      </div>
    </div>
    <div id="wRpVersions" class="rp-panel">
      <div style="flex:1;overflow-y:auto;padding:6px" id="wVerList"></div>
      <div style="padding:6px;border-top:1px solid var(--border)"><button class="btn btn-ghost" style="width:100%;justify-content:center;font-size:10px" onclick="wSaveVer()"><i class="ti ti-device-floppy"></i>Save version now</button></div>
    </div>
    <div id="wRpDiff" class="rp-panel">
      <div style="padding:6px;border-bottom:1px solid var(--border);font-size:9px;font-family:'DM Mono',monospace;display:flex;gap:8px;align-items:center"><span style="color:var(--accent)">+added</span><span style="color:var(--red)">-removed</span><span style="flex:1"></span><button class="ver-btn-sm" onclick="wBuildDiff()"><i class="ti ti-refresh"></i></button></div>
      <div style="flex:1;overflow-y:auto;padding:8px;font-size:9px;font-family:'DM Mono',monospace;line-height:1.6" id="wDiffView"></div>
    </div>
  </div>
</div>
<div class="plat-bar">
  <span style="font-size:9px;color:var(--text3);font-family:'DM Mono',monospace">Char limits:</span>
  <div class="pchip ok" id="wpX"><i class="ti ti-brand-twitter" style="font-size:11px"></i><span id="wpcX">0/280</span><div class="pbar-wrap"><div class="pbar" id="wpbX" style="width:0%;background:var(--accent)"></div></div></div>
  <div class="pchip ok" id="wpTT"><i class="ti ti-brand-tiktok" style="font-size:11px"></i><span id="wpcTT">0/2200</span><div class="pbar-wrap"><div class="pbar" id="wpbTT" style="width:0%;background:var(--accent)"></div></div></div>
  <div class="pchip ok" id="wpIG"><i class="ti ti-brand-instagram" style="font-size:11px"></i><span id="wpcIG">0/2200</span><div class="pbar-wrap"><div class="pbar" id="wpbIG" style="width:0%;background:var(--accent)"></div></div></div>
  <div class="pchip ok" id="wpYT"><i class="ti ti-brand-youtube" style="font-size:11px"></i><span id="wpcYT">0/5K</span><div class="pbar-wrap"><div class="pbar" id="wpbYT" style="width:0%;background:var(--accent)"></div></div></div>
</div>`;

  const tmplDefs = [
    {key:'breaking',icon:'ti-bolt',color:'var(--red)',name:'Breaking news',desc:'Fast, factual, lead first'},
    {key:'analysis',icon:'ti-chart-line',color:'var(--accent2)',name:'Analysis',desc:'Deep dive with context'},
    {key:'opinion',icon:'ti-message-circle',color:'var(--purple)',name:'Opinion',desc:'Editorial stance'},
    {key:'explainer',icon:'ti-bulb',color:'var(--yellow)',name:'Explainer',desc:'Background & FAQs'},
    {key:'social',icon:'ti-brand-tiktok',color:'var(--accent)',name:'Social caption',desc:'Short-form + hashtags'},
    {key:'interview',icon:'ti-microphone',color:'#ff6b35',name:'Interview / Q&A',desc:'Formatted dialogue'}
  ];

  const tmplList = document.getElementById('wTmplList');
  tmplDefs.forEach(t => {
    const d = document.createElement('div');
    d.className = 'tmpl-card' + (t.key === 'breaking' ? ' active' : '');
    d.innerHTML = `<div><i class="ti ${t.icon}" style="color:${t.color};font-size:13px"></i></div><div class="tc-name">${t.name}</div><div class="tc-desc">${t.desc}</div>`;
    d.onclick = () => wApplyTmpl(t.key, d);
    tmplList.appendChild(d);
  });

  document.getElementById('wEditor').value = TEMPLATES.breaking;
  wOnEdit(); wSaveVer();
  wAppendMsg('ai', 'Hello! I can write sections, improve your prose, add quotes, or reformat for any platform. Use the quick chips or type a prompt.<br><br><span class="wmaction" onclick="wTp(\'Write a compelling introduction\')"><i class="ti ti-arrow-right"></i>Write intro for me</span>');

  function wOnEdit() { wSync(); wUpdatePlatCounts(); wMarkUnsaved(); wCursor(); }
  window.wOnEdit = wOnEdit;

  function wMarkUnsaved() {
    const b = document.getElementById('wSavedBadge');
    b.innerHTML = '<i class="ti ti-point-filled" style="font-size:9px"></i>Unsaved';
    b.style.cssText = 'background:rgba(255,183,0,.1);color:#ffb700;border-color:rgba(255,183,0,.2)';
    clearTimeout(wSaveTimer);
    wSaveTimer = setTimeout(() => {
      b.innerHTML = '<i class="ti ti-check" style="font-size:9px"></i>Saved';
      b.style.cssText = 'background:rgba(0,229,160,.1);color:var(--accent);border-color:rgba(0,229,160,.2)';
    }, 2500);
  }

  function wCursor() {
    const ed = document.getElementById('wEditor');
    if (!ed) return;
    const ln = ed.value.substring(0, ed.selectionStart).split('\n').length;
    document.getElementById('wCursorPos').textContent = 'Ln ' + ln;
  }
  window.wCursor = wCursor;

  function wSync() {
    const md = document.getElementById('wEditor')?.value || '';
    const words = md.trim().split(/\s+/).filter(w => w).length;
    const wc = document.getElementById('wWordCount');
    const rt = document.getElementById('wReadTime');
    const pv = document.getElementById('wPreview');
    if (wc) wc.textContent = words + ' words';
    if (rt) rt.textContent = Math.max(1, Math.round(words / 200)) + ' min read';
    if (pv) pv.innerHTML = wRenderMD(md);
  }

  function wRenderMD(md) {
    if (!md.trim()) return '<p style="color:var(--text3);text-align:center;padding:30px;font-size:11px;font-family:\'DM Mono\',monospace">Preview here</p>';
    return md.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;')
      .replace(/^---$/gm,'<hr>').replace(/^### (.+)$/gm,'<h3>$1</h3>')
      .replace(/^## (.+)$/gm,'<h2>$1</h2>').replace(/^# (.+)$/gm,'<h1>$1</h1>')
      .replace(/^\> (.+)$/gm,'<blockquote>$1</blockquote>')
      .replace(/^[\-\*] (.+)$/gm,'<li>$1</li>').replace(/(<li>[\s\S]*?<\/li>)/g,'<ul>$1</ul>')
      .replace(/`([^`]+)`/g,'<code>$1</code>').replace(/\*\*([^*]+)\*\*/g,'<strong>$1</strong>')
      .replace(/\*([^*\n]+)\*/g,'<em>$1</em>').replace(/\n\n/g,'</p><p>');
  }

  function wUpdatePlatCounts() {
    const txt = (document.getElementById('wEditor')?.value || '').replace(/[#*`>\-_~\[\]]/g, '');
    const len = txt.length;
    [{id:'X',lim:280},{id:'TT',lim:2200},{id:'IG',lim:2200},{id:'YT',lim:5000}].forEach(({id,lim}) => {
      const pct = Math.min(100, Math.round(len / lim * 100));
      const chip = document.getElementById('wp' + id);
      const cnt = document.getElementById('wpc' + id);
      const bar = document.getElementById('wpb' + id);
      if (!chip) return;
      cnt.textContent = len + '/' + (lim > 999 ? Math.round(lim/1000)+'K' : lim);
      bar.style.width = pct + '%';
      chip.className = 'pchip ' + (pct >= 100 ? 'over' : pct >= 80 ? 'warn' : 'ok');
      bar.style.background = pct >= 100 ? 'var(--red)' : pct >= 80 ? 'var(--yellow)' : 'var(--accent)';
    });
  }

  function wIns(b, a) {
    const ed = document.getElementById('wEditor');
    const s = ed.selectionStart, e = ed.selectionEnd;
    ed.setRangeText(b + (ed.value.substring(s,e)||'text') + a, s, e, 'end');
    ed.focus(); wOnEdit();
  }
  window.wIns = wIns;

  function wApplyTmpl(key, el) {
    document.querySelectorAll('.tmpl-card').forEach(c => c.classList.remove('active'));
    el.classList.add('active');
    if (confirm('Replace with this template?')) {
      document.getElementById('wEditor').value = TEMPLATES[key];
      wOnEdit(); wSaveVer();
    }
  }
  window.wApplyTmpl = wApplyTmpl;

  function wSaveVer() {
    const content = document.getElementById('wEditor')?.value || '';
    const words = content.trim().split(/\s+/).filter(w=>w).length;
    const prev = versions.length > 0 ? versions[versions.length-1].content : '';
    const added = content.split('\n').filter(l=>!prev.split('\n').includes(l)&&l.trim()).length;
    const del = prev.split('\n').filter(l=>!content.split('\n').includes(l)&&l.trim()).length;
    versions.push({content, label:'v'+(versions.length+1)+'.0', time:new Date().toLocaleTimeString([],{hour:'2-digit',minute:'2-digit'}), words, added, del, preview:content.replace(/[#*`>\-_~]/g,'').substring(0,55)+'...'});
    wRenderVers(); wBuildDiff();
  }
  window.wSaveVer = wSaveVer;

  function wRenderVers() {
    const vl = document.getElementById('wVerList');
    if (!vl) return;
    if (!versions.length) { vl.innerHTML='<p style="color:var(--text3);font-size:10px;font-family:\'DM Mono\',monospace;padding:16px;text-align:center">No versions yet.</p>'; return; }
    vl.innerHTML = versions.slice().reverse().map((v,ri) => {
      const i = versions.length-1-ri;
      const isLast = i===versions.length-1;
      return `<div class="ver-item${isLast?' aver':''}"><div style="display:flex;justify-content:space-between"><span class="ver-label">${v.label}</span><span class="ver-time">${v.time}</span></div><div class="ver-diff-row"><span class="vdiff-a">+${v.added}</span><span class="vdiff-d">-${v.del}</span><span style="color:var(--text3)">${v.words}w</span></div><div class="ver-preview">${v.preview}</div>${!isLast?`<button class="ver-btn-sm" onclick="wRestoreVer(${i})"><i class="ti ti-history"></i> Restore</button>`:'<span style="font-size:9px;font-family:\'DM Mono\',monospace;color:var(--accent);margin-top:4px;display:block">&#10003; Current</span>'}</div>`;
    }).join('');
  }

  window.wRestoreVer = function(i) {
    if (confirm('Restore this version?')) {
      wSaveVer();
      document.getElementById('wEditor').value = versions[i].content;
      wOnEdit(); wRenderVers(); wBuildDiff();
    }
  };

  function wBuildDiff() {
    const dv = document.getElementById('wDiffView');
    if (!dv) return;
    if (versions.length < 2) { dv.innerHTML='<p style="color:var(--text3);font-size:10px;padding:12px;text-align:center">Need 2+ versions.</p>'; return; }
    const prev = versions[versions.length-2].content.split('\n');
    const curr = versions[versions.length-1].content.split('\n');
    let html = '';
    curr.forEach(line => { if (!prev.includes(line)&&line.trim()) html+=`<span style="background:rgba(0,229,160,.1);color:var(--accent);display:block;padding:1px 4px;border-radius:2px">+ ${line.replace(/&/g,'&amp;').replace(/</g,'&lt;')}</span>`; else if(line.trim()) html+=`<span style="color:var(--text3);display:block;padding:1px 4px">&nbsp; ${line.substring(0,50).replace(/&/g,'&amp;').replace(/</g,'&lt;')}</span>`; });
    prev.forEach(line => { if (!curr.includes(line)&&line.trim()) html+=`<span style="background:rgba(255,69,96,.1);color:var(--red);display:block;padding:1px 4px;border-radius:2px;text-decoration:line-through">- ${line.replace(/&/g,'&amp;').replace(/</g,'&lt;')}</span>`; });
    dv.innerHTML = html || '<p style="color:var(--text3);font-size:10px;padding:12px;text-align:center">No differences.</p>';
  }
  window.wBuildDiff = wBuildDiff;

  window.wSwitchTab = function(tab, el) {
    document.querySelectorAll('.rp-tab').forEach(t=>t.classList.remove('active'));
    document.querySelectorAll('.rp-panel').forEach(p=>p.classList.remove('show'));
    el.classList.add('active');
    const ids = {chat:'wRpChat',versions:'wRpVersions',diff:'wRpDiff'};
    document.getElementById(ids[tab]).classList.add('show');
    if (tab==='versions') wRenderVers();
    if (tab==='diff') wBuildDiff();
  };

  function wAppendMsg(role, html, action=null) {
    const isAI = role==='ai';
    const div = document.createElement('div');
    div.className = 'wmsg wm' + role;
    const act = action ? `<br><span class="wmaction" onclick="(${action})()"><i class="ti ti-file-plus"></i>Insert into doc</span>` : '';
    div.innerHTML = `<div class="wmav">${isAI?'AI':'ME'}</div><div class="wmbubble">${html}${act}</div>`;
    document.getElementById('wChatMsgs')?.appendChild(div);
    const msgs = document.getElementById('wChatMsgs');
    if (msgs) msgs.scrollTop = 99999;
  }

  function wShowThinking() {
    const d = document.createElement('div');
    d.className = 'wthinking'; d.id = 'wThinking';
    d.innerHTML = '<i class="ti ti-sparkles" style="font-size:11px"></i> Writing<div class="wdots"><span></span><span></span><span></span></div>';
    document.getElementById('wChatMsgs')?.appendChild(d);
    const msgs = document.getElementById('wChatMsgs');
    if (msgs) msgs.scrollTop = 99999;
  }
  function wHideThinking() { document.getElementById('wThinking')?.remove(); }

  window.wHandleKey = function(e) { if(e.key==='Enter'&&!e.shiftKey){e.preventDefault();wSendMsg();} };
  window.wTp = function(t) { const inp=document.getElementById('wChatInput'); if(inp){inp.value=t;} wSendMsg(); };

  window.wInsertDoc = function(text) {
    const ed = document.getElementById('wEditor');
    if (!ed) return;
    ed.value = ed.value.trimEnd() + '\n\n' + text + '\n';
    wOnEdit(); wSaveVer();
    ed.scrollTop = ed.scrollHeight;
  };

  window.wSendMsg = async function() {
    if (wIsGen) return;
    const input = document.getElementById('wChatInput');
    const text = input?.value?.trim();
    if (!text) return;
    if (input) input.value = '';
    wIsGen = true;
    wAppendMsg('user', text.replace(/&/g,'&amp;').replace(/</g,'&lt;'));
    wShowThinking();
    const sys = `You are an AI writing assistant in NewsFlow. Help write news articles in Markdown.\nArticle: "${document.getElementById('wDocTitle')?.value}"\nCurrent content:\n---\n${document.getElementById('wEditor')?.value}\n---\nRespond with 1-2 sentences then Markdown in a \`\`\`markdown code block.`;
    try {
      const res = await fetch('https://api.anthropic.com/v1/messages',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({model:'claude-sonnet-4-20250514',max_tokens:1000,system:sys,messages:[{role:'user',content:text}]})});
      const data = await res.json();
      wHideThinking();
      const reply = data.content?.find(b=>b.type==='text')?.text || 'Could not generate.';
      const mdMatch = reply.match(/```(?:markdown|md)?\n?([\s\S]+?)```/);
      const mdContent = mdMatch ? mdMatch[1].trim() : null;
      const prose = reply.replace(/```[\s\S]*?```/g,'').trim();
      let displayHtml = prose.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/\n/g,'<br>');
      if (mdContent) displayHtml += `<div class="wmcode">${mdContent.replace(/&/g,'&amp;').replace(/</g,'&lt;')}</div>`;
      const cap = mdContent;
      wAppendMsg('ai', displayHtml, cap ? `function(){wInsertDoc(${JSON.stringify(cap)})}` : null);
    } catch(e) { wHideThinking(); wAppendMsg('ai','Connection error — check API key in appsettings.'); }
    wIsGen = false;
  };

  window.wExportMD = function() {
    const ed = document.getElementById('wEditor');
    const title = document.getElementById('wDocTitle')?.value || 'article';
    if (!ed) return;
    const slug = title.toLowerCase().replace(/[^a-z0-9]+/g,'-').replace(/(^-|-$)/g,'');
    const blob = new Blob([ed.value],{type:'text/markdown'});
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = slug + '.md';
    a.click();
  };
};
