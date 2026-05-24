# NewsFlow UI

## How to use

### Option A — Open directly in browser
Double-click `NewsFlowApp.html` to open in any browser. All pages work immediately with no server needed.

### Option B — Paste into Next.js
The UI is structured so each page maps directly to a Next.js route:

```
NewsFlowApp.html          →  app/layout.tsx  (sidebar + topbar shell)
pages/dashboard.js        →  app/dashboard/page.tsx
pages/writeup.js          →  app/writeup/page.tsx
pages/review.js           →  app/review/page.tsx
pages/other-pages.js      →  app/*/page.tsx  (feed, analytics, video, etc.)
```

### Option C — Paste into Claude Code
Open your NewsFlow project in VS Code, open Claude Code in the terminal, 
and say:

> "I have a UI design in HTML/JS. Convert each page into a Next.js 14 
> App Router page component, using Tailwind for styling and connecting 
> each data point to the real .NET API endpoints."

Then paste the contents of NewsFlowApp.html when prompted.

## File structure

```
NewsFlowApp.html          Main shell — sidebar, topbar, navigation
pages/
  dashboard.js            Dashboard with stats, feed, accounts
  writeup.js              Full Write-up Studio with AI chat + MD editor
  review.js               Review queue with severity badges + flag actions  
  other-pages.js          Feed, Analytics, Video, Scheduler, Accounts, Config, Monetize
```

## API endpoints to wire up

| UI action | API endpoint |
|---|---|
| Load articles | GET /api/articles |
| Create article | POST /api/articles |
| Publish article | POST /api/articles/{id}/publish |
| Load review queue | GET /api/flags |
| Approve flag | PATCH /api/flags/{id}/approve |
| Reject flag | PATCH /api/flags/{id}/reject |
| Escalate flag | PATCH /api/flags/{id}/escalate |
| Update flag rules | PUT /api/flags/rules |
| AI chat in studio | POST /api/ai/generate |
| Load accounts | GET /api/accounts |
| Analytics | GET /api/analytics/overview |
| Live feed | GET /api/feed |

## Design tokens

All colors use CSS variables defined in `NewsFlowApp.html`:
- `--accent` #00e5a0  (green — primary CTA, active states)
- `--accent2` #0088ff (blue — secondary actions, info)
- `--red` #ff4560    (errors, blocks, high severity)
- `--yellow` #ffb700 (warnings, mid severity, review)
- `--purple` #a855f7 (AI features, video, escalation)
- `--bg` #0a0b0e     (page background)
- `--card` #1c2130   (card surfaces)
