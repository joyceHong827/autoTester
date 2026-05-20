
import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

test('b2elogin', async ({ page }) => {
  await b2eLogin(page);

  // Navigate to the Issue Management page
  await page.goto('http://b2e.lab.etzone.net/Web/IssueManagement');

});

test('b2eIssueManagement', async ({ page }) => { 
  await b2eLogin(page);

  // Navigate to the Issue Management page
  await page.goto('http://b2e.lab.etzone.net/Web/IssueManagement');

  await page.waitForURL(
    'http://b2e.lab.etzone.net/Web/IssueManagement',
    { timeout: 15000 }
  );
  // Fill in the issue title
  await page.getByPlaceholder('輸入使用者名稱').nth(1).clear();
  await page.getByRole('button', { name: '查詢' }).click();
  await page.getByText('Lingsun_49058_處理人Email格式錯誤', { exact: true }).click(); 
  
  // 跳出子視窗
  const editor = page.locator('.reply-area .ql-editor');

  await editor.click();
  await editor.fill('測試回覆內容');  
  await expect(editor).toContainText('測試回覆內容');  

});
