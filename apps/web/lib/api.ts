/**
 * Lightweight fetch wrapper for the NewsFlow .NET API.
 * Reads the access token from localStorage and injects it as a Bearer header.
 * On 401, attempts a token refresh then retries once.
 */

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? '';

function getToken() {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem('access_token');
}

function getRefreshToken() {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem('refresh_token');
}

async function refreshTokens() {
  const refreshToken = getRefreshToken();
  if (!refreshToken) throw new Error('No refresh token');

  const res = await fetch(`${API_BASE}/api/auth/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken }),
  });

  if (!res.ok) throw new Error('Token refresh failed');

  const { accessToken, refreshToken: newRefresh } = await res.json();
  localStorage.setItem('access_token',  accessToken);
  localStorage.setItem('refresh_token', newRefresh);
  return accessToken;
}

export async function apiFetch<T>(
  path: string,
  init: RequestInit = {},
): Promise<T> {
  const token = getToken();
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(init.headers as Record<string, string> ?? {}),
  };
  if (token) headers['Authorization'] = `Bearer ${token}`;

  let res = await fetch(`${API_BASE}${path}`, { ...init, headers });

  // Attempt silent token refresh on 401
  if (res.status === 401) {
    try {
      const newToken = await refreshTokens();
      headers['Authorization'] = `Bearer ${newToken}`;
      res = await fetch(`${API_BASE}${path}`, { ...init, headers });
    } catch {
      // Redirect to login
      if (typeof window !== 'undefined') window.location.href = '/login';
      throw new Error('Unauthenticated');
    }
  }

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error(body?.error ?? `HTTP ${res.status}`);
  }

  return res.json() as Promise<T>;
}

// ── Typed API helpers ─────────────────────────────────────────────────────────

export const api = {
  // Auth
  login:    (email: string, password: string) =>
    apiFetch<{ accessToken: string; refreshToken: string }>('/api/auth/login', {
      method: 'POST', body: JSON.stringify({ email, password }),
    }),
  register: (name: string, email: string, password: string) =>
    apiFetch<{ accessToken: string; refreshToken: string }>('/api/auth/register', {
      method: 'POST', body: JSON.stringify({ name, email, password }),
    }),
  logout: (refreshToken: string) =>
    apiFetch<void>('/api/auth/logout', { method: 'POST', body: JSON.stringify({ refreshToken }) }),

  // Feed
  getFeed: (params?: { status?: string; category?: string; page?: number; perPage?: number }) => {
    const qs = new URLSearchParams(params as Record<string, string>).toString();
    return apiFetch<unknown>(`/api/feed${qs ? `?${qs}` : ''}`);
  },

  // AI
  rewriteHeadline:  (headline: string)                => apiFetch<{ headline: string }>('/api/ai/rewrite',  { method: 'POST', body: JSON.stringify({ headline }) }),
  generateCaption:  (content: string, platform: string) => apiFetch<{ caption: string }>('/api/ai/caption', { method: 'POST', body: JSON.stringify({ content, platform }) }),
  generateArticle:  (topic: string, category: string)   => apiFetch<{ contentMd: string }>('/api/ai/generate', { method: 'POST', body: JSON.stringify({ topic, category }) }),
  generateScript:   (articleId: string)               => apiFetch<{ script: string }>('/api/ai/script',   { method: 'POST', body: JSON.stringify({ articleId }) }),

  // Accounts
  getAccounts:   ()                                   => apiFetch<unknown[]>('/api/accounts'),
  connectAccount:(data: object)                       => apiFetch<{ id: string }>('/api/accounts', { method: 'POST', body: JSON.stringify(data) }),
  disconnectAccount:(id: string)                      => apiFetch<void>(`/api/accounts/${id}`, { method: 'DELETE' }),

  // Analytics
  getOverview: () => apiFetch<unknown>('/api/analytics/overview'),
  getPosts:    (postId?: string) => apiFetch<unknown[]>(`/api/analytics/posts${postId ? `?postId=${postId}` : ''}`),
  getRevenue:  (from: string, to: string) => apiFetch<unknown>(`/api/analytics/revenue?from=${from}&to=${to}`),

  // Sources
  getSources:  ()                => apiFetch<unknown[]>('/api/sources'),
  addSource:   (data: object)    => apiFetch<{ id: string }>('/api/sources', { method: 'POST', body: JSON.stringify(data) }),
  removeSource:(id: string)      => apiFetch<void>(`/api/sources/${id}`, { method: 'DELETE' }),
  trustSource: (id: string)      => apiFetch<unknown>(`/api/sources/${id}/trust`, { method: 'PATCH' }),
};
