import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

/**
 * Redmine #49604
 * 修正OB商品管理，進入商品明細頁，目前位置顯示
 */
test('Redmine 49604: Verify OB Product Detail Page Breadcrumb', async ({ page }) => {
  // 1. 登入 B2E 系統
  await b2eLogin(page);

  // 2. 於主選單中導覽至「OB商品管理 > OB商品審核&維護」
  await test.step('導航至 OB商品審核&維護', async () => {
    await page.getByRole('link', { name: 'OB商品管理' }).click();
    await page.getByRole('link', { name: 'OB商品審核&維護' }).click();
  });

  // 3. 在商品清單中點擊進入商品明細頁
  await test.step('進入商品明細頁', async () => {
    // 根據錄製程式碼，先點擊「商開審核」按鈕
    await page.getByRole('button', { name: '商開審核' }).click();
    
    // 點擊指定商品名稱進入明細頁
    await page.getByRole('link', { name: '作價測試商品-不勾選作價品-OB' }).click();
    
    await page.waitForLoadState('networkidle');
  });

  // 4. 驗證目前位置顯示
  await test.step('驗證目前位置顯示是否正確', async () => {
    // 取得「目前位置」區塊並驗證路徑文字
    const breadcrumb = page.locator('div:has-text("目前位置")').first();
    await expect(breadcrumb).toContainText('OB商品管理');
    await expect(breadcrumb).toContainText('OB商品審核&維護');
    
    // 負向驗證：目前位置路徑中不應包含錯誤的電視商品字樣
    await expect(breadcrumb).not.toContainText('電視商品管理');
    await expect(breadcrumb).not.toContainText('商品開發&審核');
    
    // 截圖存證
    await page.screenshot({ path: 'tests/images/issue_49604_result.png', fullPage: true });
  });
});
