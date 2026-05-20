import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

test('test', async ({ page }) => {
  const orderItemId = '100307266-1';

  // 1. 登入 B2E
  await b2eLogin(page);

  // 2. 進入廠商問題管理頁面
  await page.getByRole('link', { name: ' 供應商管理' }).click();
  await page.getByRole('link', { name: '廠商問題管理' }).click();
  // 或者直接導向
  // await page.goto('http://b2e.lab.etzone.net/Web/IssueManagement');
  
  await page.waitForTimeout(1000);
  
  // 3. 設定查詢條件並搜尋
  await page.getByRole('searchbox', { name: '輸入使用者名稱' }).nth(1).fill('');
  await page.getByRole('combobox').nth(1).selectOption('2');  
  await page.locator('form input[type="text"]').fill(orderItemId);
  await page.getByRole('button', { name: '  查詢' }).click();

  // 4. 驗證結果
  // 取得查詢結果資料列的第 2 列 (nth(1))
  const secondRow = page.getByRole('row').nth(1);
  await expect(secondRow).toBeVisible();

  // 驗證第 8 欄 (nth(7)) 與 第 9 欄 (nth(8)) 不為空值
  const col8 = await secondRow.locator('td').nth(7).innerText();
  const col9 = await secondRow.locator('td').nth(8).innerText();

  console.log(`第 8 欄內容: ${col8}`);
  console.log(`第 9 欄內容: ${col9}`);

  expect(col8.trim()).not.toBe('');
  expect(col9.trim()).not.toBe('');

  // 5. 截圖
  await page.screenshot({ path: 'tests/images/issue_management_result.png' });
});