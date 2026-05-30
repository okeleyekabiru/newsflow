/**
 * Playwright API tests for the feed endpoint.
 * Runs against the live .NET API on http://localhost:5000 — start it before running.
 *
 * npm run test:api
 */
import { test, expect, APIRequestContext } from '@playwright/test';

const API = 'http://localhost:5000';

// ── Helpers ────────────────────────────────────────────────────────────────────

async function register(request: APIRequestContext): Promise<{ token: string; email: string }> {
  const email = `test_${Date.now()}@newsflow.dev`;
  const res = await request.post(`${API}/api/auth/register`, {
    data: { name: 'Test User', email, password: 'password1' },
  });
  expect(res.ok(), `Register failed: ${await res.text()}`).toBeTruthy();
  const { accessToken } = await res.json();
  return { token: accessToken, email };
}

function authHeaders(token: string) {
  return { Authorization: `Bearer ${token}` };
}

// ── Feed endpoint tests ────────────────────────────────────────────────────────

test.describe('GET /api/feed', () => {
  test('returns 401 without token', async ({ request }) => {
    const res = await request.get(`${API}/api/feed`);
    expect(res.status()).toBe(401);
  });

  test('returns paged wrapper with items array', async ({ request }) => {
    const { token } = await register(request);
    const res = await request.get(`${API}/api/feed`, { headers: authHeaders(token) });

    expect(res.ok(), `Feed failed: ${await res.text()}`).toBeTruthy();

    const body = await res.json();
    console.log('Feed response:', JSON.stringify(body, null, 2));

    // Shape checks
    expect(body).toHaveProperty('items');
    expect(body).toHaveProperty('total');
    expect(body).toHaveProperty('page');
    expect(body).toHaveProperty('perPage');
    expect(Array.isArray(body.items)).toBe(true);
  });

  test('fresh user has empty feed', async ({ request }) => {
    const { token } = await register(request);
    const res = await request.get(`${API}/api/feed`, { headers: authHeaders(token) });
    const body = await res.json();

    expect(body.total).toBe(0);
    expect(body.items).toHaveLength(0);
  });

  test('article created by user appears in feed', async ({ request }) => {
    const { token } = await register(request);
    const headers = authHeaders(token);

    // Create an article
    const createRes = await request.post(`${API}/api/articles`, {
      headers,
      data: {
        title: 'Test Article',
        contentMd: '# Hello World\nSome content here.',
        category: 'Technology',
        template: 'BreakingNews',
      },
    });
    expect(createRes.ok(), `Create article failed: ${await createRes.text()}`).toBeTruthy();
    const article = await createRes.json();
    console.log('Created article:', JSON.stringify(article, null, 2));

    // Feed should now have 1 item
    const feedRes = await request.get(`${API}/api/feed`, { headers });
    const feed = await feedRes.json();
    console.log('Feed after create:', JSON.stringify(feed, null, 2));

    expect(feed.total).toBe(1);
    expect(feed.items[0].title).toBe('Test Article');
  });

  test('category filter works', async ({ request }) => {
    const { token } = await register(request);
    const headers = authHeaders(token);

    const content = '# Test\nSome content for the article body.';

    // Create two articles in different categories
    const r1 = await request.post(`${API}/api/articles`, {
      headers,
      data: { title: 'Tech Article', contentMd: content, category: 'Technology', template: 'BreakingNews' },
    });
    expect(r1.ok(), `Tech create failed: ${await r1.text()}`).toBeTruthy();

    const r2 = await request.post(`${API}/api/articles`, {
      headers,
      data: { title: 'Finance Article', contentMd: content, category: 'Finance', template: 'BreakingNews' },
    });
    expect(r2.ok(), `Finance create failed: ${await r2.text()}`).toBeTruthy();

    // Verify unfiltered shows 2
    const allFeed = await (await request.get(`${API}/api/feed`, { headers })).json();
    console.log('All articles:', JSON.stringify(allFeed, null, 2));
    expect(allFeed.total).toBe(2);

    // Test filtered feeds
    const techFeed = await (await request.get(`${API}/api/feed?category=Technology`, { headers })).json();
    const finFeed  = await (await request.get(`${API}/api/feed?category=Finance`, { headers })).json();

    console.log('Tech feed:', JSON.stringify(techFeed, null, 2));
    console.log('Finance feed:', JSON.stringify(finFeed, null, 2));

    expect(techFeed.items.length).toBe(1);
    expect(techFeed.items[0].title).toBe('Tech Article');
    expect(finFeed.items.length).toBe(1);
    expect(finFeed.items[0].title).toBe('Finance Article');
  });
});

// ── Flags endpoint — find the "12 messages" ────────────────────────────────────

test.describe('GET /api/flags — count check', () => {
  test('reports current pending flag count', async ({ request }) => {
    const { token } = await register(request);
    const res = await request.get(`${API}/api/flags`, { headers: authHeaders(token) });

    expect(res.ok(), `Flags failed: ${await res.text()}`).toBeTruthy();
    const flags = await res.json();
    console.log(`Pending flags count: ${Array.isArray(flags) ? flags.length : JSON.stringify(flags)}`);
    console.log('Flags response:', JSON.stringify(flags, null, 2));
  });
});
