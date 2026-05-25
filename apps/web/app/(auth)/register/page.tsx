'use client';

import { useState, FormEvent } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';

export default function RegisterPage() {
  const router = useRouter();
  const [name, setName]         = useState('');
  const [email, setEmail]       = useState('');
  const [password, setPassword] = useState('');
  const [error, setError]       = useState('');
  const [loading, setLoading]   = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const res = await fetch('/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, email, password }),
      });
      if (!res.ok) {
        const body = await res.json().catch(() => ({}));
        throw new Error(body?.error ?? `HTTP ${res.status}`);
      }
      const { accessToken, refreshToken } = await res.json();
      localStorage.setItem('access_token',  accessToken);
      localStorage.setItem('refresh_token', refreshToken);
      router.push('/');
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Registration failed');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center" style={{ background: 'var(--bg)' }}>
      <div className="w-full max-w-[380px] px-4">
        {/* Logo */}
        <div className="text-center mb-8">
          <div className="font-display text-[28px] font-[800] tracking-[-0.5px]">
            News<span className="text-accent">Flow</span>
          </div>
          <div className="font-mono text-[10px] text-text3 tracking-[2px] uppercase mt-1">
            Media Automation Platform
          </div>
        </div>

        <div className="rounded-card p-6" style={{ background: 'var(--card)', border: '1px solid var(--border)' }}>
          <h2 className="font-display text-[18px] font-[700] mb-1">Create account</h2>
          <p className="text-[12px] text-text2 mb-5">Start automating your media empire.</p>

          {error && (
            <div className="flex items-center gap-[8px] p-[10px] rounded-[8px] mb-4 text-[12px]"
              style={{ background: 'rgba(255,69,96,.1)', border: '1px solid rgba(255,69,96,.3)', color: 'var(--red)' }}>
              <i className="ti ti-alert-circle text-[14px]" /> {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="flex flex-col gap-[12px]">
            <div>
              <label className="block font-mono text-[10px] text-text3 uppercase tracking-[1px] mb-[6px]">Full name</label>
              <input
                type="text" required value={name} onChange={(e) => setName(e.target.value)}
                className="w-full px-3 py-[9px] rounded-[8px] text-[13px] outline-none"
                style={{ background: 'var(--bg3)', border: '1px solid var(--border)', color: 'var(--text)' }}
                placeholder="Jane Smith"
              />
            </div>
            <div>
              <label className="block font-mono text-[10px] text-text3 uppercase tracking-[1px] mb-[6px]">Email</label>
              <input
                type="email" required value={email} onChange={(e) => setEmail(e.target.value)}
                className="w-full px-3 py-[9px] rounded-[8px] text-[13px] outline-none"
                style={{ background: 'var(--bg3)', border: '1px solid var(--border)', color: 'var(--text)' }}
                placeholder="you@example.com"
              />
            </div>
            <div>
              <label className="block font-mono text-[10px] text-text3 uppercase tracking-[1px] mb-[6px]">
                Password <span className="normal-case text-[9px]">(min 8 characters)</span>
              </label>
              <input
                type="password" required minLength={8} value={password} onChange={(e) => setPassword(e.target.value)}
                className="w-full px-3 py-[9px] rounded-[8px] text-[13px] outline-none"
                style={{ background: 'var(--bg3)', border: '1px solid var(--border)', color: 'var(--text)' }}
                placeholder="••••••••"
              />
            </div>
            <button
              type="submit" disabled={loading}
              className="w-full py-[10px] rounded-btn font-[600] text-[13px] text-black transition-all mt-1 disabled:opacity-50"
              style={{ background: 'var(--accent)' }}
            >
              {loading ? 'Creating account…' : 'Create account'}
            </button>
          </form>

          <p className="text-center text-[12px] text-text3 mt-4">
            Already have an account?{' '}
            <Link href="/login" className="text-accent hover:underline">Sign in</Link>
          </p>
        </div>
      </div>
    </div>
  );
}
