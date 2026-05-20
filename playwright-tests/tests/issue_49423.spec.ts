import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

const testOrders = [
  { type: '廠送 (Factory Delivery)', orderItemId: '100307266-1', screenshotName: 'issue_49423_factory_delivery.png' },
  { type: '庫送 (Warehouse Delivery)', orderItemId: '103231506-2', screenshotName: 'issue_49423_warehouse_delivery.png' },
  { type: '特約 (Special Order)', orderItemId: '100213039-1', screenshotName: 'issue_49423_special_order.png' }
];

for (const order of testOrders) {
  test(`Verify ${order.type} order item ID: ${order.orderItemId}`, async ({ page }) => {
    // 1. 登入 B2E
    await b2eLogin(page);

    // 2. 直接進入廠商問題管理頁面
    await page.goto('http://b2e.lab.etzone.net/Web/IssueManagement');
    await page.waitForLoadState('networkidle');
    
    // 3. 設定查詢條件並搜尋
    // 清除可能的使用者名稱篩選 (參考範例)
    const userSearchBox = page.getByRole('searchbox', { name: '輸入使用者名稱' });
    if (await userSearchBox.count() > 1) {
       await userSearchBox.nth(1).fill('');
    }
    
    // 選擇查詢類型 (2: 銷編項次?)
    await page.getByRole('combobox').nth(1).selectOption('2');  
    
    // 輸入訂單項次
    await page.locator('form input[type="text"]').fill(order.orderItemId);
    
    // 點擊查詢
    await page.getByRole('button', { name: '  查詢' }).click();

    // 4. 驗證結果
    // 等候表格載入
    await page.waitForTimeout(2000);
    
    const secondRow = page.getByRole('row').nth(1);
    
    try {
      await expect(secondRow).toBeVisible({ timeout: 15000 });
      
      // 取得商行資訊 (根據 b2e.issueManagement.spec.ts 應為第 8 或 9 欄)
      const col8 = await secondRow.locator('td').nth(7).innerText();
      const col9 = await secondRow.locator('td').nth(8).innerText();

      console.log(`Order Type: ${order.type}`);
      console.log(`Order Item ID: ${order.orderItemId}`);
      console.log(`Column 8: ${col8}`);
      console.log(`Column 9: ${col9}`);

      expect(col8.trim()).not.toBe('');
      expect(col9.trim()).not.toBe('');
    } catch (error) {
      console.error(`Failed to find results for ${order.orderItemId}`);
      await page.screenshot({ path: `tests/images/fail_${order.screenshotName}` });
      throw error;
    }

    // 5. 截圖
    await page.screenshot({ path: `tests/images/${order.screenshotName}` });
  });
}
