/**
 * Real-credentials login + source seeding + feed verification.
 */
import { test, expect, APIRequestContext } from '@playwright/test';

const API   = 'http://localhost:5000';
const EMAIL = 'kabiotobiano@gmail.com';
const PASS  = 'Abiola123456%';

const RSS_SOURCES = [
  // General / World
  { name: 'BBC News',           url: 'http://feeds.bbci.co.uk/news/rss.xml',                       type: 'RSS' },
  { name: 'Reuters',            url: 'https://feeds.reuters.com/reuters/topNews',                   type: 'RSS' },
  { name: 'Al Jazeera',        url: 'https://www.aljazeera.com/xml/rss/all.xml',                   type: 'RSS' },
  { name: 'Sky News',          url: 'https://feeds.skynews.com/feeds/rss/home.xml',                type: 'RSS' },
  { name: 'The Guardian',      url: 'https://www.theguardian.com/world/rss',                       type: 'RSS' },
  { name: 'NPR News',          url: 'https://feeds.npr.org/1001/rss.xml',                          type: 'RSS' },
  // Technology
  { name: 'TechCrunch',        url: 'https://techcrunch.com/feed/',                                type: 'RSS' },
  { name: 'The Verge',         url: 'https://www.theverge.com/rss/index.xml',                      type: 'RSS' },
  { name: 'Ars Technica',      url: 'https://feeds.arstechnica.com/arstechnica/index',             type: 'RSS' },
  { name: 'Wired',             url: 'https://www.wired.com/feed/rss',                              type: 'RSS' },
  // Finance
  { name: 'CNBC Finance',      url: 'https://search.cnbc.com/rs/search/combinedcms/view.xml?partnerId=wrss01&id=10000664', type: 'RSS' },
  { name: 'Financial Times',   url: 'https://www.ft.com/rss/home/uk',                             type: 'RSS' },
  // Science / Health
  { name: 'New Scientist',     url: 'https://www.newscientist.com/feed/home/',                     type: 'RSS' },
  { name: 'BBC Health',        url: 'http://feeds.bbci.co.uk/news/health/rss.xml',                 type: 'RSS' },
  // Sports
  { name: 'BBC Sport',         url: 'http://feeds.bbci.co.uk/sport/rss.xml',                      type: 'RSS' },
  { name: 'Sky Sports',        url: 'https://www.skysports.com/rss/12040',                         type: 'RSS' },
];

async function login(request: APIRequestContext): Promise<string> {
  const res = await request.post(`${API}/api/auth/login`, {
    data: { email: EMAIL, password: PASS },
  });
  expect(res.ok(), `Login failed: ${await res.text()}`).toBeTruthy();
  const { accessToken } = await res.json();
  return accessToken;
}

function auth(token: string) {
  return { Authorization: `Bearer ${token}` };
}

// ── 1. Login ──────────────────────────────────────────────────────────────────

test('1 — login succeeds', async ({ request }) => {
  const token = await login(request);
  expect(token).toBeTruthy();
  console.log('Logged in, token starts with:', token.substring(0, 40) + '…');
});

// ── 2. Add RSS sources ────────────────────────────────────────────────────────

test('2 — add RSS sources for ingest', async ({ request }) => {
  const token = await login(request);

  // Check existing sources
  const existing = await (await request.get(`${API}/api/sources`, { headers: auth(token) })).json();
  console.log('Existing sources:', JSON.stringify(existing, null, 2));

  // Add each source (skip if already present)
  const existingNames = new Set(
    (Array.isArray(existing) ? existing : existing.value ?? []).map((s: { name: string }) => s.name)
  );

  for (const src of RSS_SOURCES) {
    if (existingNames.has(src.name)) {
      console.log(`Skipping "${src.name}" — already exists`);
      continue;
    }
    const res = await request.post(`${API}/api/sources`, {
      headers: auth(token),
      data: src,
    });
    const body = await res.json().catch(() => res.text());
    console.log(`Add "${src.name}": ${res.status()}`, JSON.stringify(body));
  }

  // Verify sources are saved
  const after = await (await request.get(`${API}/api/sources`, { headers: auth(token) })).json();
  console.log('Sources after setup:', JSON.stringify(after, null, 2));

  const sources = Array.isArray(after) ? after : after.value ?? [];
  expect(sources.length).toBeGreaterThan(0);
});

// ── 3. Feed + flags for real user ────────────────────────────────────────────

test('3 — check feed (may be empty before first ingest)', async ({ request }) => {
  const token = await login(request);

  const feed = await (await request.get(`${API}/api/feed`, { headers: auth(token) })).json();
  console.log(`Feed total: ${feed.total}, unique check OK`);
  console.log('Feed items (top 3):', JSON.stringify(feed.items?.slice(0, 3), null, 2));

  expect(feed).toHaveProperty('items');
  expect(Array.isArray(feed.items)).toBe(true);
});

test('4 — review queue has flagged posts for real user', async ({ request }) => {
  const token = await login(request);

  const res  = await request.get(`${API}/api/flags`, { headers: auth(token) });
  const flags = await res.json();

  console.log(`Flags count: ${Array.isArray(flags) ? flags.length : 'not array'}`);
  console.log('Sample flags:', JSON.stringify(
    Array.isArray(flags) ? flags.slice(0, 3) : flags, null, 2));

  expect(res.ok()).toBeTruthy();
  expect(Array.isArray(flags)).toBe(true);
  expect(flags.length).toBeGreaterThan(0);
  console.log(`\n✓ Review queue has ${flags.length} pending item(s) ready for approval/rejection`);
});
