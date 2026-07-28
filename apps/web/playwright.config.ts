import { defineConfig, devices } from '@playwright/test';

/**
 * E2E config. Assumes the API (:5000) and web (:3000) dev servers are running
 * (locally: `make dev`; in CI: started by the workflow before this runs).
 */
export default defineConfig({
  testDir: './e2e',
  timeout: 30_000,
  expect: { timeout: 10_000 },
  fullyParallel: false,
  // Serial — these E2E tests share one tenant's data + the dev servers, so
  // parallel workers race on login/state. One worker is reliable for this suite.
  workers: 1,
  retries: process.env.CI ? 2 : 1,
  reporter: process.env.CI ? 'github' : 'list',
  use: {
    baseURL: 'http://localhost:3000',
    trace: 'on-first-retry',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  ],
});
