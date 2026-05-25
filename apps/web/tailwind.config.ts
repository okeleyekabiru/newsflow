import type { Config } from 'tailwindcss';

const config: Config = {
  content: [
    './app/**/*.{js,ts,jsx,tsx,mdx}',
    './components/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {
      colors: {
        // Map every CSS variable so Tailwind classes like bg-card, text-accent work
        bg:       'var(--bg)',
        bg2:      'var(--bg2)',
        bg3:      'var(--bg3)',
        card:     'var(--card)',
        border:   'var(--border)',
        border2:  'var(--border2)',
        accent:   'var(--accent)',
        accent2:  'var(--accent2)',
        accent3:  'var(--accent3)',
        text:     'var(--text)',
        text2:    'var(--text2)',
        text3:    'var(--text3)',
        red:      'var(--red)',
        yellow:   'var(--yellow)',
        purple:   'var(--purple)',
      },
      fontFamily: {
        sans:  ['DM Sans', 'sans-serif'],
        mono:  ['DM Mono', 'monospace'],
        display: ['Syne', 'sans-serif'],
      },
      borderRadius: {
        card: '12px',
        btn:  '7px',
      },
    },
  },
  plugins: [],
};

export default config;
