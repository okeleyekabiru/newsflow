import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './specs',
  timeout: 30_000,
  retries: 0,
  reporter: [['list'], ['html', { open: 'never' }]],

  projects: [
    {
      name: 'api',
      testMatch: '**/*.api.spec.ts',
    },
    {
      name: 'chromium',
      use: { browserName: 'chromium', baseURL: 'http://localhost:3000' },
      testMatch: '**/*.ui.spec.ts',
    },
  ],
});
