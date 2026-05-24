window.render_feed = function(container) {
  container.innerHTML = `<div class="page active"><div class="page-content">
  <div class="panel">
    <div class="panel-hdr"><div class="panel-title"><i class="ti ti-rss"></i>Live news feed</div><div style="display:flex;gap:6px"><button class="btn btn-ghost" style="padding:4px 9px;font-size:10px"><i class="ti ti-filter"></i>Filter</button><button class="btn btn-ghost" style="padding:4px 9px;font-size:10px"><i class="ti ti-refresh"></i>Refresh</button></div></div>
    <div class="panel-body">
      <div class="cats" style="margin-bottom:10px">
        <div class="cat on" style="--cc:var(--accent2)"><i class="ti ti-world"></i>Politics</div>
        <div class="cat on" style="--cc:var(--accent)"><i class="ti ti-trending-up"></i>Finance</div>
        <div class="cat on" style="--cc:var(--purple)"><i class="ti ti-cpu"></i>Technology</div>
        <div class="cat on" style="--cc:var(--yellow)"><i class="ti ti-ball-football"></i>Sports</div>
        <div class="cat on" style="--cc:#22d3ee"><i class="ti ti-heart-rate-monitor"></i>Health</div>
        <div class="cat"><i class="ti ti-cloud-storm"></i>Weather</div>
        <div class="cat"><i class="ti ti-planet"></i>Science</div>
      </div>
      <div class="feed-item"><div class="feed-icon" style="background:rgba(0,136,255,.12)"><i class="ti ti-world" style="color:var(--accent2)"></i></div><div><div class="feed-title">G7 Summit reaches historic agreement on digital trade regulations</div><div class="feed-meta"><span>Reuters</span><span>·</span><span>4m ago</span><span class="fbadge s-auto"><i class="ti ti-bolt" style="font-size:9px"></i>Auto-post</span></div></div></div>
      <div class="feed-item"><div class="feed-icon" style="background:rgba(0,229,160,.1)"><i class="ti ti-trending-up" style="color:var(--accent)"></i></div><div><div class="feed-title">NASDAQ surges 2.3% on strong tech earnings</div><div class="feed-meta"><span>Bloomberg</span><span>·</span><span>9m ago</span><span class="fbadge s-auto"><i class="ti ti-bolt" style="font-size:9px"></i>Auto-post</span><span class="fbadge s-video"><i class="ti ti-player-play" style="font-size:9px"></i>Video</span></div></div></div>
      <div class="feed-item"><div class="feed-icon" style="background:rgba(255,183,0,.1)"><i class="ti ti-alert-triangle" style="color:var(--yellow)"></i></div><div><div class="feed-title">Tensions escalate in disputed border region — casualties confirmed</div><div class="feed-meta"><span>AP News</span><span>·</span><span>15m ago</span><span class="fbadge s-review"><i class="ti ti-eye" style="font-size:9px"></i>Review required</span><span class="severity-badge sev-mid">Sev 6/10</span></div></div></div>
      <div class="feed-item"><div class="feed-icon" style="background:rgba(168,85,247,.1)"><i class="ti ti-cpu" style="color:var(--purple)"></i></div><div><div class="feed-title">AI breakthrough: new model achieves human-level reasoning</div><div class="feed-meta"><span>TechCrunch</span><span>·</span><span>22m ago</span><span class="fbadge s-auto"><i class="ti ti-bolt" style="font-size:9px"></i>Auto-post</span></div></div></div>
      <div class="feed-item"><div class="feed-icon" style="background:rgba(255,69,96,.1)"><i class="ti ti-shield-x" style="color:var(--red)"></i></div><div><div class="feed-title">Extremist group claims responsibility — full details withheld</div><div class="feed-meta"><span>BBC</span><span>·</span><span>31m ago</span><span class="fbadge s-blocked"><i class="ti ti-lock" style="font-size:9px"></i>Blocked</span><span class="severity-badge sev-high">Sev 9/10</span></div></div></div>
    </div>
  </div>
  <div style="padding:10px 14px;background:rgba(255,183,0,.06);border:1px solid rgba(255,183,0,.2);border-radius:9px;display:flex;gap:10px;align-items:center">
    <i class="ti ti-alert-triangle" style="color:var(--yellow);font-size:15px;flex-shrink:0"></i>
    <div style="font-size:11px;color:var(--text2)">Conflict &amp; Geopolitics requires manual review. <span style="color:var(--yellow);cursor:pointer" onclick="goto('review',null)">Go to review queue →</span></div>
  </div>
</div></div>`;
};

window.render_analytics = function(container) {
  container.innerHTML = `<div class="page active"><div class="page-content">
  <div class="stats-row">
    <div class="stat-card" style="--c:var(--accent)"><div class="stat-label">Total views</div><div class="stat-value">4.2M</div><div class="stat-sub"><span>+18%</span> vs last month</div></div>
    <div class="stat-card" style="--c:var(--accent2)"><div class="stat-label">Engagement rate</div><div class="stat-value">6.4%</div><div class="stat-sub">Above <span>industry avg</span></div></div>
    <div class="stat-card" style="--c:var(--yellow)"><div class="stat-label">Revenue (month)</div><div class="stat-value">$847</div><div class="stat-sub"><span>+$120</span> vs last month</div></div>
    <div class="stat-card" style="--c:var(--purple)"><div class="stat-label">Top platform</div><div class="stat-value">TikTok</div><div class="stat-sub"><span>847K</span> followers</div></div>
  </div>
  <div class="two-col">
    <div class="panel">
      <div class="panel-hdr"><div class="panel-title"><i class="ti ti-chart-line"></i>Views over time (30 days)</div></div>
      <div class="panel-body">
        <div class="big-chart">
          <div class="bc-bar" style="height:45%;background:rgba(0,229,160,.25);border-top:2px solid var(--accent)"></div>
          <div class="bc-bar" style="height:52%;background:rgba(0,229,160,.25);border-top:2px solid var(--accent)"></div>
          <div class="bc-bar" style="height:38%;background:rgba(0,229,160,.25);border-top:2px solid var(--accent)"></div>
          <div class="bc-bar" style="height:65%;background:rgba(0,229,160,.25);border-top:2px solid var(--accent)"></div>
          <div class="bc-bar" style="height:55%;background:rgba(0,229,160,.25);border-top:2px solid var(--accent)"></div>
          <div class="bc-bar" style="height:80%;background:rgba(0,229,160,.3);border-top:2px solid var(--accent)"></div>
          <div class="bc-bar" style="height:72%;background:rgba(0,229,160,.25);border-top:2px solid var(--accent)"></div>
          <div class="bc-bar" style="height:90%;background:rgba(0,136,255,.25);border-top:2px solid var(--accent2)"></div>
          <div class="bc-bar" style="height:75%;background:rgba(0,229,160,.25);border-top:2px solid var(--accent)"></div>
          <div class="bc-bar" style="height:95%;background:rgba(0,229,160,.35);border-top:2px solid var(--accent)"></div>
        </div>
      </div>
    </div>
    <div class="panel">
      <div class="panel-hdr"><div class="panel-title"><i class="ti ti-users"></i>Revenue by platform</div></div>
      <div class="panel-body" style="padding-top:6px">
        <div class="account-card"><div class="plat-icon" style="background:#000;border:1px solid #333"><i class="ti ti-brand-tiktok" style="color:#fff;font-size:13px"></i></div><div style="flex:1"><div style="font-size:11px;font-weight:500">TikTok</div><div style="font-size:9px;color:var(--text3);font-family:'DM Mono',monospace">@worldnewsnow</div></div><div style="font-size:13px;font-weight:700;font-family:'Syne',sans-serif;color:var(--accent)">$312</div></div>
        <div class="account-card"><div class="plat-icon" style="background:linear-gradient(135deg,#833ab4,#fd1d1d,#fcb045);border:1px solid #444"><i class="ti ti-brand-instagram" style="color:#fff;font-size:13px"></i></div><div style="flex:1"><div style="font-size:11px;font-weight:500">Instagram</div><div style="font-size:9px;color:var(--text3);font-family:'DM Mono',monospace">@reelsnews</div></div><div style="font-size:13px;font-weight:700;font-family:'Syne',sans-serif;color:var(--accent)">$225</div></div>
        <div class="account-card"><div class="plat-icon" style="background:#000;border:1px solid #333"><i class="ti ti-brand-twitter" style="color:#fff;font-size:13px"></i></div><div style="flex:1"><div style="font-size:11px;font-weight:500">X / Twitter</div><div style="font-size:9px;color:var(--text3);font-family:'DM Mono',monospace">@breakingnews_x</div></div><div style="font-size:13px;font-weight:700;font-family:'Syne',sans-serif;color:var(--accent)">$198</div></div>
        <div class="account-card"><div class="plat-icon" style="background:#ff0000;border:1px solid #cc0000"><i class="ti ti-brand-youtube" style="color:#fff;font-size:13px"></i></div><div style="flex:1"><div style="font-size:11px;font-weight:500">YouTube</div><div style="font-size:9px;color:var(--text3);font-family:'DM Mono',monospace">@YTNewsHub</div></div><div style="font-size:13px;font-weight:700;font-family:'Syne',sans-serif;color:var(--accent)">$112</div></div>
      </div>
    </div>
  </div>
</div></div>`;
};

window.render_video = function(container) {
  container.innerHTML = `<div class="page active"><div class="page-content">
  <div class="stats-row">
    <div class="stat-card" style="--c:var(--purple)"><div class="stat-label">Videos generated</div><div class="stat-value">23</div><div class="stat-sub"><span>3</span> processing now</div></div>
    <div class="stat-card" style="--c:var(--accent)"><div class="stat-label">Total video views</div><div class="stat-value">1.8M</div><div class="stat-sub"><span>+22%</span> this week</div></div>
    <div class="stat-card" style="--c:var(--accent2)"><div class="stat-label">Avg watch time</div><div class="stat-value">38s</div><div class="stat-sub">TikTok benchmark</div></div>
    <div class="stat-card" style="--c:var(--yellow)"><div class="stat-label">Video revenue</div><div class="stat-value">$415</div><div class="stat-sub">This month</div></div>
  </div>
  <div class="two-col">
    <div class="panel">
      <div class="panel-hdr"><div class="panel-title"><i class="ti ti-player-play"></i>Video queue</div><button class="btn btn-ghost" style="padding:4px 10px;font-size:10px;background:rgba(168,85,247,.15);color:var(--purple);border:1px solid rgba(168,85,247,.3)"><i class="ti ti-sparkles"></i>Generate new</button></div>
      <div class="panel-body">
        <div class="video-card"><div class="vid-thumb"><i class="ti ti-player-play" style="color:var(--red);font-size:16px"></i></div><div style="flex:1"><div class="vid-title">NASDAQ Surge — Financial Breakdown</div><div class="vid-meta"><span>9:16 TikTok</span><span>·</span><span>58s</span><span class="vbadge vb-proc">Processing</span></div></div></div>
        <div class="video-card"><div class="vid-thumb"><i class="ti ti-player-play" style="color:var(--red);font-size:16px"></i></div><div style="flex:1"><div class="vid-title">AI Reasoning Breakthrough — Full Story</div><div class="vid-meta"><span>9:16 Reels</span><span>·</span><span>45s</span><span class="vbadge vb-ready">Ready</span></div></div></div>
        <div class="video-card"><div class="vid-thumb"><i class="ti ti-player-play" style="color:var(--red);font-size:16px"></i></div><div style="flex:1"><div class="vid-title">G7 Digital Trade Summit — Explainer</div><div class="vid-meta"><span>16:9 YouTube</span><span>·</span><span>3m 12s</span><span class="vbadge vb-ready">Ready</span></div></div></div>
      </div>
    </div>
    <div style="display:flex;flex-direction:column;gap:12px">
      <div class="panel">
        <div class="panel-hdr"><div class="panel-title"><i class="ti ti-settings-2"></i>Video AI config</div></div>
        <div class="panel-body" style="padding-top:6px;padding-bottom:6px">
          <div class="config-row"><div><div class="config-label"><i class="ti ti-microphone"></i>AI voiceover</div><div class="config-sub">ElevenLabs — Nigerian English</div></div><div class="toggle"></div></div>
          <div class="config-row"><div><div class="config-label"><i class="ti ti-photo"></i>Stock B-roll</div><div class="config-sub">Pexels + Pixabay auto-match</div></div><div class="toggle"></div></div>
          <div class="config-row"><div><div class="config-label"><i class="ti ti-subtask"></i>Auto captions</div><div class="config-sub">Burn-in subtitles</div></div><div class="toggle"></div></div>
          <div class="config-row"><div><div class="config-label"><i class="ti ti-brand-tiktok"></i>9:16 crop</div><div class="config-sub">TikTok / Reels format</div></div><div class="toggle"></div></div>
        </div>
      </div>
    </div>
  </div>
</div></div>`;
};

window.render_scheduler = function(container) {
  container.innerHTML = `<div class="page active"><div class="page-content">
  <div class="panel">
    <div class="panel-hdr"><div class="panel-title"><i class="ti ti-clock"></i>Post schedule — today</div></div>
    <div class="panel-body">
      <div class="schedule">
        <div class="sch">0</div><div class="sch">2</div><div class="sch on">4</div><div class="sch on">6</div>
        <div class="sch peak">8</div><div class="sch peak">10</div><div class="sch on">12</div><div class="sch on">14</div>
        <div class="sch peak">16</div><div class="sch on">18</div><div class="sch on">20</div><div class="sch">22</div>
      </div>
      <div style="display:flex;gap:10px;margin-top:8px;font-size:9px;font-family:'DM Mono',monospace">
        <span style="color:var(--accent);display:flex;align-items:center;gap:3px"><span style="width:8px;height:8px;border-radius:2px;background:rgba(0,229,160,.15);border:1px solid var(--accent);display:inline-block"></span>Scheduled</span>
        <span style="color:var(--accent2);display:flex;align-items:center;gap:3px"><span style="width:8px;height:8px;border-radius:2px;background:rgba(0,136,255,.12);border:1px solid var(--accent2);display:inline-block"></span>Peak hours</span>
        <span style="color:var(--text3);display:flex;align-items:center;gap:3px"><span style="width:8px;height:8px;border-radius:2px;background:var(--bg3);border:1px solid var(--border);display:inline-block"></span>Off</span>
      </div>
    </div>
  </div>
  <div class="panel">
    <div class="panel-hdr"><div class="panel-title"><i class="ti ti-calendar"></i>Upcoming posts</div></div>
    <div class="panel-body">
      <div class="feed-item"><div class="feed-icon" style="background:rgba(0,0,0,.3)"><i class="ti ti-brand-tiktok" style="color:#fff"></i></div><div><div class="feed-title">G7 Digital Trade — TikTok caption</div><div class="feed-meta"><span>Today · 18:00 UTC</span><span class="fbadge s-auto">Scheduled</span></div></div></div>
      <div class="feed-item"><div class="feed-icon" style="background:#000"><i class="ti ti-brand-twitter" style="color:#fff"></i></div><div><div class="feed-title">NASDAQ surge thread — X/Twitter</div><div class="feed-meta"><span>Today · 20:00 UTC</span><span class="fbadge s-auto">Scheduled</span></div></div></div>
      <div class="feed-item"><div class="feed-icon" style="background:#ff0000"><i class="ti ti-brand-youtube" style="color:#fff"></i></div><div><div class="feed-title">G7 Explainer video — YouTube</div><div class="feed-meta"><span>Tomorrow · 06:00 UTC</span><span class="fbadge s-video">Video ready</span></div></div></div>
    </div>
  </div>
</div></div>`;
};

window.render_accounts = function(container) {
  container.innerHTML = `<div class="page active"><div class="page-content">
  <div class="stats-row">
    <div class="stat-card" style="--c:var(--accent)"><div class="stat-label">Total followers</div><div class="stat-value">1.76M</div><div class="stat-sub">Across 4 platforms</div></div>
    <div class="stat-card" style="--c:var(--yellow)"><div class="stat-label">Monthly revenue</div><div class="stat-value">$847</div><div class="stat-sub">All platforms combined</div></div>
    <div class="stat-card" style="--c:var(--accent2)"><div class="stat-label">Posts today</div><div class="stat-value">84</div><div class="stat-sub">Across all accounts</div></div>
    <div class="stat-card" style="--c:var(--purple)"><div class="stat-label">Accounts active</div><div class="stat-value">4/4</div><div class="stat-sub">All connected</div></div>
  </div>
  <div class="panel">
    <div class="panel-hdr"><div class="panel-title"><i class="ti ti-users"></i>Connected accounts</div><button class="btn btn-primary" style="padding:4px 10px;font-size:10px"><i class="ti ti-plus"></i>Add account</button></div>
    <div class="panel-body" style="padding-top:6px;padding-bottom:6px">
      <div class="account-card"><div class="plat-icon" style="background:#000;border:1px solid #333"><i class="ti ti-brand-tiktok" style="color:#fff;font-size:14px"></i></div><div style="flex:1"><div class="acc-followers">847K</div><div class="acc-posts">@worldnewsnow · 34 posts/day</div><div class="acc-rev">$312/mo</div></div><div class="toggle"></div></div>
      <div class="account-card"><div class="plat-icon" style="background:#000;border:1px solid #333"><i class="ti ti-brand-twitter" style="color:#fff;font-size:14px"></i></div><div style="flex:1"><div class="acc-followers">512K</div><div class="acc-posts">@breakingnews_x · 48 posts/day</div><div class="acc-rev">$198/mo</div></div><div class="toggle"></div></div>
      <div class="account-card"><div class="plat-icon" style="background:linear-gradient(135deg,#833ab4,#fd1d1d,#fcb045);border:1px solid #444"><i class="ti ti-brand-instagram" style="color:#fff;font-size:14px"></i></div><div style="flex:1"><div class="acc-followers">290K</div><div class="acc-posts">@reelsnews · 12 posts/day</div><div class="acc-rev">$225/mo</div></div><div class="toggle"></div></div>
      <div class="account-card"><div class="plat-icon" style="background:#ff0000;border:1px solid #cc0000"><i class="ti ti-brand-youtube" style="color:#fff;font-size:14px"></i></div><div style="flex:1"><div class="acc-followers">112K</div><div class="acc-posts">@YTNewsHub · 4 vids/day</div><div class="acc-rev">$112/mo</div></div><div class="toggle"></div></div>
    </div>
  </div>
</div></div>`;
};

window.render_config = function(container) {
  container.innerHTML = `<div class="page active"><div class="page-content">
  <div class="two-col">
    <div style="display:flex;flex-direction:column;gap:12px">
      <div class="panel">
        <div class="panel-hdr"><div class="panel-title"><i class="ti ti-robot"></i>AI settings</div></div>
        <div class="panel-body" style="padding-top:6px;padding-bottom:6px">
          <div class="config-row"><div><div class="config-label"><i class="ti ti-sparkles"></i>AI headline rewrite</div><div class="config-sub">Claude rewrites for engagement</div></div><div class="toggle"></div></div>
          <div class="config-row"><div><div class="config-label"><i class="ti ti-player-play"></i>Auto-generate video</div><div class="config-sub">For trending stories</div></div><div class="toggle"></div></div>
          <div class="config-row"><div><div class="config-label"><i class="ti ti-pencil"></i>AI write-up assist</div><div class="config-sub">Context-aware article help</div></div><div class="toggle"></div></div>
        </div>
      </div>
      <div class="panel">
        <div class="panel-hdr"><div class="panel-title"><i class="ti ti-shield-check"></i>Flag rules — conflict &amp; war</div></div>
        <div class="panel-body" style="padding-top:6px;padding-bottom:6px">
          <div class="config-row"><div><div class="config-label"><i class="ti ti-shield-check"></i>Safe content filter</div><div class="config-sub">Block policy violations</div></div><div class="toggle"></div></div>
          <div class="config-row"><div><div class="config-label"><i class="ti ti-eye"></i>Conflict review gate</div><div class="config-sub">Manual approval for sensitive</div></div><div class="toggle"></div></div>
          <div class="config-row"><div><div class="config-label"><i class="ti ti-adjustments"></i>Severity threshold</div><div class="config-sub">Flag when score ≥ 4/10</div></div><span style="font-size:12px;font-family:'DM Mono',monospace;color:var(--accent)">4</span></div>
          <div class="config-row"><div><div class="config-label"><i class="ti ti-arrow-up"></i>Auto-escalate at</div><div class="config-sub">Send email when score ≥ 8</div></div><span style="font-size:12px;font-family:'DM Mono',monospace;color:var(--red)">8</span></div>
        </div>
      </div>
    </div>
    <div style="display:flex;flex-direction:column;gap:12px">
      <div class="panel">
        <div class="panel-hdr"><div class="panel-title"><i class="ti ti-rss"></i>Trusted news sources</div></div>
        <div class="panel-body" style="display:flex;flex-direction:column;gap:6px">
          <div class="mono-chip green"><i class="ti ti-check" style="font-size:11px"></i>Reuters</div>
          <div class="mono-chip green"><i class="ti ti-check" style="font-size:11px"></i>AP News</div>
          <div class="mono-chip green"><i class="ti ti-check" style="font-size:11px"></i>Bloomberg</div>
          <div class="mono-chip green"><i class="ti ti-check" style="font-size:11px"></i>BBC News</div>
          <div class="mono-chip green"><i class="ti ti-check" style="font-size:11px"></i>TechCrunch</div>
          <button class="btn btn-ghost" style="font-size:10px;padding:5px 10px;margin-top:4px"><i class="ti ti-plus"></i>Add source</button>
        </div>
      </div>
    </div>
  </div>
</div></div>`;
};

window.render_monetize = function(container) {
  container.innerHTML = `<div class="page active"><div class="page-content">
  <div class="stats-row">
    <div class="stat-card" style="--c:var(--yellow)"><div class="stat-label">Total revenue</div><div class="stat-value">$847</div><div class="stat-sub">This month</div></div>
    <div class="stat-card" style="--c:var(--accent)"><div class="stat-label">Ad revenue share</div><div class="stat-value">$512</div><div class="stat-sub">TikTok + YouTube</div></div>
    <div class="stat-card" style="--c:var(--accent2)"><div class="stat-label">Sponsorships</div><div class="stat-value">$235</div><div class="stat-sub">2 active deals</div></div>
    <div class="stat-card" style="--c:var(--purple)"><div class="stat-label">SaaS subscriptions</div><div class="stat-value">$100</div><div class="stat-sub">2 external users</div></div>
  </div>
  <div class="panel">
    <div class="panel-hdr"><div class="panel-title"><i class="ti ti-currency-dollar"></i>Monetization streams</div></div>
    <div class="panel-body">
      <div class="config-row"><div><div class="config-label"><i class="ti ti-brand-tiktok"></i>TikTok Creator Fund</div><div class="config-sub">Enabled · Per-view payouts</div></div><span style="font-size:12px;font-family:'DM Mono',monospace;color:var(--accent)">$180/mo</span></div>
      <div class="config-row"><div><div class="config-label"><i class="ti ti-brand-youtube"></i>YouTube AdSense</div><div class="config-sub">Enabled · In-stream ads</div></div><span style="font-size:12px;font-family:'DM Mono',monospace;color:var(--accent)">$112/mo</span></div>
      <div class="config-row"><div><div class="config-label"><i class="ti ti-brand-twitter"></i>X Ad Revenue Share</div><div class="config-sub">Enabled · Verified creator</div></div><span style="font-size:12px;font-family:'DM Mono',monospace;color:var(--accent)">$198/mo</span></div>
      <div class="config-row"><div><div class="config-label"><i class="ti ti-brand-instagram"></i>Instagram Bonuses</div><div class="config-sub">Reels play bonus program</div></div><span style="font-size:12px;font-family:'DM Mono',monospace;color:var(--accent)">$22/mo</span></div>
      <div class="config-row"><div><div class="config-label"><i class="ti ti-briefcase"></i>Brand sponsorships</div><div class="config-sub">2 active · Manual invoicing</div></div><span style="font-size:12px;font-family:'DM Mono',monospace;color:var(--accent)">$235/mo</span></div>
      <div class="config-row"><div><div class="config-label"><i class="ti ti-server"></i>NewsFlow SaaS</div><div class="config-sub">Sell platform access to others</div></div><span style="font-size:12px;font-family:'DM Mono',monospace;color:var(--accent)">$100/mo</span></div>
    </div>
  </div>
</div></div>`;
};
