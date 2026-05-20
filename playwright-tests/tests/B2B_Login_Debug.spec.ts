import { test, expect } from '@playwright/test';

test.describe('B2B 登入測試', () => {
  test('基本登入流程', async ({ page }) => {
    // 設定較長的 timeout
    test.setTimeout(60000);

    // 導航至登入頁面
    console.log('1. 導航至登入頁面...');
    await page.goto('http://b2b.lab.etzone.net/Web/B2B_B2ELogin');

    // 輸入統一編號
    console.log('2. 輸入統一編號...');
    await page.getByRole('textbox', { name: '統一編號' }).fill('12718372');

    // 輸入密碼
    console.log('3. 輸入密碼...');
    await page.getByRole('textbox', { name: '密碼' }).fill('123456');

    // 點擊登入按鈕
    console.log('4. 點擊登入按鈕...');
    await page.getByRole('button', { name: ' 登入' }).click();

    // 等待導航
    console.log('5. 等待頁面載入...');
    await page.waitForLoadState('networkidle');

    // 嘗試處理彈窗（如果有的話）
    try {
      console.log('6. 嘗試關閉彈窗...');
      const closeButton = page.getByRole('button', { name: '下次再說' });
      await closeButton.click({ timeout: 3000 });
    } catch {
      console.log('   沒有找到「下次再說」按鈕，繼續...');
    }

    // 再次等待
    await page.waitForLoadState('networkidle');

    // 驗證當前 URL
    const currentUrl = page.url();
    console.log(`7. 當前 URL: ${currentUrl}`);

    // 驗證登入成功（檢查 URL 是否已改變）
    expect(currentUrl).toContain('b2b.lab.etzone.net/Web/');
    expect(currentUrl).not.toContain('B2B_B2ELogin');

    // 截圖以便檢查
    await page.screenshot({ path: 'test-results/login-success.png', fullPage: true });
    console.log('8. 已儲存截圖到 test-results/login-success.png');

    // 列出頁面上所有的連結
    console.log('9. 頁面上的所有連結:');
    const links = await page.locator('a').all();
    for (let i = 0; i < Math.min(links.length, 20); i++) {
      const text = await links[i].textContent();
      const href = await links[i].getAttribute('href');
      console.log(`   - ${text?.trim()} (${href})`);
    }
  });
});
