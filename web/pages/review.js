window.render_review = function(container) {
  container.innerHTML = `
<div class="page active">
<div class="page-content">
  <div style="display:flex;align-items:center;justify-content:space-between;margin-bottom:4px">
    <div style="font-size:12px;color:var(--text2)">3 stories require manual review. Flagged by content safety filter.</div>
    <div style="display:flex;gap:6px;font-size:10px;font-family:'DM Mono',monospace">
      <span style="color:var(--accent);display:flex;align-items:center;gap:4px"><span style="width:8px;height:8px;border-radius:2px;background:rgba(0,229,160,.15);border:1px solid var(--accent);display:inline-block"></span>Sev 1–3 AutoPost</span>
      <span style="color:var(--yellow);display:flex;align-items:center;gap:4px"><span style="width:8px;height:8px;border-radius:2px;background:rgba(255,183,0,.12);border:1px solid var(--yellow);display:inline-block"></span>Sev 4–7 Review</span>
      <span style="color:var(--red);display:flex;align-items:center;gap:4px"><span style="width:8px;height:8px;border-radius:2px;background:rgba(255,69,96,.1);border:1px solid var(--red);display:inline-block"></span>Sev 8–10 Block</span>
    </div>
  </div>

  <div class="review-item">
    <div style="display:flex;align-items:center;justify-content:space-between;margin-bottom:4px">
      <div class="ri-title">Escalating tensions in disputed border region — casualties confirmed</div>
      <span class="severity-badge sev-mid">Severity 6/10</span>
    </div>
    <div class="ri-meta">AP News · 15m ago · Conflict &amp; War · Keywords: casualties, troops, frontline</div>
    <div style="font-size:11px;color:var(--text2);margin-bottom:8px;line-height:1.5">Reuters reports at least 12 casualties following escalating confrontations along a disputed border. Local authorities have declared a state of emergency.</div>
    <div class="ri-actions">
      <button class="ri-btn ri-approve" onclick="approveFlag(this)"><i class="ti ti-check"></i>Approve &amp; post</button>
      <button class="ri-btn ri-reject" onclick="rejectFlag(this)"><i class="ti ti-x"></i>Reject</button>
      <button class="ri-btn ri-edit" onclick="goto('writeup',null)"><i class="ti ti-pencil"></i>Edit in studio</button>
      <button class="ri-btn" style="background:rgba(168,85,247,.1);color:var(--purple);border:1px solid rgba(168,85,247,.2)" onclick="escalateFlag(this)"><i class="ti ti-arrow-up"></i>Escalate</button>
    </div>
  </div>

  <div class="review-item">
    <div style="display:flex;align-items:center;justify-content:space-between;margin-bottom:4px">
      <div class="ri-title">Extremist group claims responsibility for attack</div>
      <span class="severity-badge sev-high">Severity 9/10</span>
    </div>
    <div class="ri-meta">BBC · 31m ago · Terrorism · Keywords: extremist, terrorist, attack claimed, manifesto</div>
    <div style="font-size:11px;color:var(--text2);margin-bottom:8px;line-height:1.5">An extremist group has released a statement claiming responsibility. Contains propaganda content that requires senior editorial review before publication.</div>
    <div class="ri-actions">
      <button class="ri-btn ri-approve" onclick="approveFlag(this)"><i class="ti ti-check"></i>Approve &amp; post</button>
      <button class="ri-btn ri-reject" onclick="rejectFlag(this)"><i class="ti ti-x"></i>Reject</button>
      <button class="ri-btn ri-edit" onclick="goto('writeup',null)"><i class="ti ti-pencil"></i>Edit in studio</button>
      <button class="ri-btn" style="background:rgba(168,85,247,.1);color:var(--purple);border:1px solid rgba(168,85,247,.2)" onclick="escalateFlag(this)"><i class="ti ti-arrow-up"></i>Escalate</button>
    </div>
  </div>

  <div class="review-item">
    <div style="display:flex;align-items:center;justify-content:space-between;margin-bottom:4px">
      <div class="ri-title">Military operation launched — government confirms offensive</div>
      <span class="severity-badge sev-mid">Severity 5/10</span>
    </div>
    <div class="ri-meta">Reuters · 1h ago · Conflict &amp; War · Keywords: military, offensive, casualties</div>
    <div style="font-size:11px;color:var(--text2);margin-bottom:8px;line-height:1.5">Government forces have launched a military operation. Story includes unverified casualty figures and references civilian impact.</div>
    <div class="ri-actions">
      <button class="ri-btn ri-approve" onclick="approveFlag(this)"><i class="ti ti-check"></i>Approve &amp; post</button>
      <button class="ri-btn ri-reject" onclick="rejectFlag(this)"><i class="ti ti-x"></i>Reject</button>
      <button class="ri-btn ri-edit" onclick="goto('writeup',null)"><i class="ti ti-pencil"></i>Edit in studio</button>
      <button class="ri-btn" style="background:rgba(168,85,247,.1);color:var(--purple);border:1px solid rgba(168,85,247,.2)" onclick="escalateFlag(this)"><i class="ti ti-arrow-up"></i>Escalate</button>
    </div>
  </div>

  <div style="padding:10px 14px;background:rgba(255,183,0,.06);border:1px solid rgba(255,183,0,.2);border-radius:9px;display:flex;gap:10px;align-items:center">
    <i class="ti ti-settings-2" style="color:var(--yellow);font-size:15px;flex-shrink:0"></i>
    <div style="font-size:11px;color:var(--text2)">Flag rules are configurable per category. <span style="color:var(--yellow);cursor:pointer" onclick="goto('config',null)">Edit rules in Configuration →</span></div>
  </div>
</div>
</div>`;

  window.approveFlag = function(btn) {
    btn.closest('.review-item').style.opacity = '.4';
    btn.closest('.review-item').style.pointerEvents = 'none';
  };
  window.rejectFlag = function(btn) {
    btn.closest('.review-item').style.opacity = '.25';
    btn.closest('.review-item').style.pointerEvents = 'none';
  };
  window.escalateFlag = function(btn) {
    const item = btn.closest('.review-item');
    item.style.borderColor = 'var(--purple)';
    btn.textContent = '↑ Escalated';
    btn.style.opacity = '.6';
    btn.disabled = true;
  };
};
