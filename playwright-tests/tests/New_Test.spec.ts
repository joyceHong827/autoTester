import { test, expect } from '@playwright/test';

test.describe('B2B 商品資料查詢流程', () => {
  test('完整登入並進入商品資料查詢', async ({ page }) => {
    // 設定較長的 timeout
    test.setTimeout(60000);

    // 1. 導航至登入頁面
    await page.goto('http://b2b.lab.etzone.net/Web/B2B_B2ELogin');

    // 2. 輸入統一編號
    await page.getByRole('textbox', { name: '統一編號' }).fill('12718372');

    // 3. 輸入密碼
    await page.getByRole('textbox', { name: '密碼' }).fill('123456');

    // 4. 點擊登入按鈕
    await page.getByRole('button', { name: ' 登入' }).click();

    // 5. 等待頁面載入
    await page.waitForLoadState('networkidle');

    // 6. 處理登入後的提示彈窗（如果有的話）
    try {
      await page.getByRole('button', { name: '下次再說' }).click({ timeout: 3000 });
      await page.waitForLoadState('networkidle');
    } catch {
      // 如果沒有彈窗就忽略
    }

    // 7. 驗證登入成功
    await expect(page).toHaveURL(/Web\/DefaultPage/);
    console.log('✅ 登入成功');

    // 8. 直接導航到商品資料查詢頁面（繞過可能隱藏的選單）
    await page.goto('http://b2b.lab.etzone.net/Web/ProductQuery');
    await page.waitForLoadState('networkidle');

    // 9. 驗證已進入商品資料查詢頁面
    await expect(page).toHaveURL(/Web\/ProductQuery/);
    await expect(page.locator('body')).toBeVisible();

    console.log('✅ 成功進入商品資料查詢頁面');

    // 10. 可選：截圖保存
    await page.screenshot({ path: 'test-results/product-query-page.png', fullPage: true });
  });
});




