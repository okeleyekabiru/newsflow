window.render_dashboard = function(container) {
  container.innerHTML = `
<div class="page active">
<div class="page-content">
  <div class="stats-row">
    <div class="stat-card" style="--c:var(--accent)"><div class="stat-label">Posts today</div><div class="stat-value">84</div><div class="stat-sub">Across <span>6 accounts</span></div></div>
    <div class="stat-card" style="--c:var(--accent2)"><div class="stat-label">Total followers</div><div class="stat-value">2.4M</div><div class="stat-sub"><span>+12k</span> this week</div></div>
    <div class="stat-card" style="--c:var(--yellow)"><div class="stat-label">Est. revenue</div><div class="stat-value">$847</div><div class="stat-sub">This month so far</div></div>
    <div class="stat-card" style="--c:var(--purple)"><div class="stat-label">AI videos made</div><div class="stat-value">23</div><div class="stat-sub"><span>3</span> processing</div></div>
  </div>
  <div class="two-col">
    <div style="display:flex;flex-direction:column;gap:12px">
      <div class="panel">
        <div class="panel-hdr"><div class="panel-title"><i class="ti ti-rss"></i>Live news feed</div><button class="btn btn-ghost" style="padding:4px 9px;font-size:10px" onclick="goto('feed',null)"><i class="ti ti-arrow-right"></i>View all</button></div>
        <div class="panel-body">
          <div class="feed-item"><div class="feed-icon" style="background:rgba(0,136,255,.12)"><i class="ti ti-world" style="color:var(--accent2)"></i></div><div><div class="feed-title">G7 Summit reaches historic agreement on digital trade</div><div class="feed-meta"><span>Reuters</span><span>·</span><span>4m ago</span><span class="fbadge s-auto"><i class="ti ti-bolt" style="font-size:9px"></i>Auto-post</span></div></div></div>
          <div class="feed-item"><div class="feed-icon" style="background:rgba(0,229,160,.1)"><i class="ti ti-trending-up" style="color:var(--accent)"></i></div><div><div class="feed-title">NASDAQ surges 2.3% on strong tech earnings reports</div><div class="feed-meta"><span>Bloomberg</span><span>·</span><span>9m ago</span><span class="fbadge s-auto"><i class="ti ti-bolt" style="font-size:9px"></i>Auto-post</span><span class="fbadge s-video"><i class="ti ti-player-play" style="font-size:9px"></i>Video</span></div></div></div>
          <div class="feed-item"><div class="feed-icon" style="background:rgba(255,183,0,.1)"><i class="ti ti-alert-triangle" style="color:var(--yellow)"></i></div><div><div class="feed-title">Tensions escalate in disputed border region</div><div class="feed-meta"><span>AP News</span><span>·</span><span>15m ago</span><span class="fbadge s-review"><i class="ti ti-eye" style="font-size:9px"></i>Review required</span><span class="severity-badge sev-mid">Sev 6/10</span></div></div></div>
          <div class="feed-item"><div class="feed-icon" style="background:rgba(168,85,247,.1)"><i class="ti ti-cpu" style="color:var(--purple)"></i></div><div><div class="feed-title">AI breakthrough: new model achieves human-level reasoning</div><div class="feed-meta"><span>TechCrunch</span><span>·</span><span>22m ago</span><span class="fbadge s-auto"><i class="ti ti-bolt" style="font-size:9px"></i>Auto-post</span></div></div></div>
        </div>
      </div>
      <div class="panel">
        <div class="panel-hdr"><div class="panel-title"><i class="ti ti-pencil"></i>Recent write-ups</div><button class="btn btn-ghost" style="padding:4px 9px;font-size:10px" onclick="goto('writeup',null)"><i class="ti ti-plus"></i>New article</button></div>
        <div class="panel-body">
          <div class="feed-item" onclick="goto('writeup',null)" style="cursor:pointer"><div class="feed-icon" style="background:rgba(0,229,160,.08)"><i class="ti ti-file-text" style="color:var(--accent)"></i></div><div style="flex:1"><div class="feed-title">G7 Digital Trade Summit — Full Analysis</div><div class="feed-meta"><span>847 words</span><span>·</span><span>Saved 2m ago</span><span class="fbadge s-auto">MD</span></div></div></div>
          <div class="feed-item" onclick="goto('writeup',null)" style="cursor:pointer"><div class="feed-icon" style="background:rgba(0,136,255,.08)"><i class="ti ti-file-text" style="color:var(--accent2)"></i></div><div style="flex:1"><div class="feed-title">NASDAQ Surge — Breaking Analysis</div><div class="feed-meta"><span>512 words</span><span>·</span><span>Saved 1h ago</span><span class="fbadge s-video">Published</span></div></div></div>
        </div>
      </div>
    </div>
    <div style="display:flex;flex-direction:column;gap:12px">
      <div class="panel">
        <div class="panel-hdr"><div class="panel-title"><i class="ti ti-users"></i>Connected accounts</div></div>
        <div class="panel-body" style="padding-top:6px;padding-bottom:6px">
          <div class="account-card"><div class="plat-icon" style="background:#000;border:1px solid #333"><i class="ti ti-brand-tiktok" style="color:#fff;font-size:14px"></i></div><div style="flex:1"><div class="acc-followers">847K</div><div class="acc-posts">@worldnewsnow</div><div class="acc-rev">$312/mo</div></div><div class="toggle"></div></div>
          <div class="account-card"><div class="plat-icon" style="background:#000;border:1px solid #333"><i class="ti ti-brand-twitter" style="color:#fff;font-size:14px"></i></div><div style="flex:1"><div class="acc-followers">512K</div><div class="acc-posts">@breakingnews_x</div><div class="acc-rev">$198/mo</div></div><div class="toggle"></div></div>
          <div class="account-card"><div class="plat-icon" style="background:linear-gradient(135deg,#833ab4,#fd1d1d,#fcb045);border:1px solid #444"><i class="ti ti-brand-instagram" style="color:#fff;font-size:14px"></i></div><div style="flex:1"><div class="acc-followers">290K</div><div class="acc-posts">@reelsnews</div><div class="acc-rev">$225/mo</div></div><div class="toggle"></div></div>
          <div class="account-card"><div class="plat-icon" style="background:#ff0000;border:1px solid #cc0000"><i class="ti ti-brand-youtube" style="color:#fff;font-size:14px"></i></div><div style="flex:1"><div class="acc-followers">112K</div><div class="acc-posts">@YTNewsHub</div><div class="acc-rev">$112/mo</div></div><div class="toggle"></div></div>
        </div>
      </div>
      <div class="panel">
        <div class="panel-hdr"><div class="panel-title"><i class="ti ti-chart-bar"></i>Posts this week</div></div>
        <div class="panel-body">
          <div class="mini-bars">
            <div class="bar" style="height:42%;background:rgba(0,229,160,.3);border-top:2px solid var(--accent)"></div>
            <div class="bar" style="height:58%;background:rgba(0,229,160,.3);border-top:2px solid var(--accent)"></div>
            <div class="bar" style="height:50%;background:rgba(0,229,160,.3);border-top:2px solid var(--accent)"></div>
            <div class="bar" style="height:78%;background:rgba(0,229,160,.3);border-top:2px solid var(--accent)"></div>
            <div class="bar" style="height:65%;background:rgba(0,229,160,.3);border-top:2px solid var(--accent)"></div>
            <div class="bar" style="height:82%;background:rgba(0,136,255,.25);border-top:2px solid var(--accent2)"></div>
            <div class="bar" style="height:52%;background:rgba(0,136,255,.12);border-top:2px dashed var(--accent2)"></div>
          </div>
          <div style="display:flex;justify-content:space-between;margin-top:3px;font-size:9px;color:var(--text3);font-family:'DM Mono',monospace"><span>Mon</span><span>Tue</span><span>Wed</span><span>Thu</span><span>Fri</span><span>Sat</span><span style="color:var(--accent2)">Sun</span></div>
        </div>
      </div>
    </div>
  </div>
</div>
</div>`;
};
