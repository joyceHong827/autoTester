import { test, expect } from '@playwright/test';

/**
 * 此檔案為 TestFlow Studio 自動生成的範例測試腳本。
 * 實際腳本由 AI 根據 Redmine Issue 與 Playwright Codegen 錄製結果產生。
 *
 * redmine_issue_id: 0
 * title: 範例 - 首頁載入測試
 */
test.describe('範例 - 首頁載入測試', () => {

  test('TC-0 驗證首頁正確載入', async ({ page }) => {
    // 步驟 1：開啟目標網站
    await page.goto('https://example.com');

    // 預期結果 1：頁面標題包含 "Example"
    await expect(page).toHaveTitle(/Example/);

    // 步驟 2：確認主要標題可見
    const heading = page.locator('h1');

    // 預期結果 2：h1 標題存在且可見
    await expect(heading).toBeVisible();

    // 預期結果 3：h1 文字為 "Example Domain"
    await expect(heading).toHaveText('Example Domain');
  });

});
