import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: 0,
  workers: 1,

  reporter: [
    ['json', { outputFile: 'playwright-report/results.json' }],
    ['html',  { open: 'never' }],
    ['line']
  ],

  use: {
    headless: false,              // 有頭模式，可以看到瀏覽器操作過程
    viewport: { width: 1280, height: 720 },
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    trace: 'on-first-retry',
  },

  // ✅ 只使用 Chromium（Chrome），不安裝 Firefox / WebKit
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
