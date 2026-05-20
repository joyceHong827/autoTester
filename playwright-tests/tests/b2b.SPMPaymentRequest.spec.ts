import { test, expect } from '@playwright/test';
import { b2bLogin } from './b2bLogin';

test('SPM付款申請上傳憑證測試', async ({ page }) => {
  // 使用共用的登入函式
  await b2bLogin(page);
  await page.waitForURL('http://b2b.lab.etzone.net/Web/DefaultPage');
  await page.goto('http://b2b.lab.etzone.net/Web/SPMPaymentRequest');  
  await page.waitForTimeout(2000);
    
  await page.getByRole('link', { name: '上傳憑證' }).click();
  await page.getByRole('button', { name: '+ 新增' }).click();  
  
  // 假設 1.jpg 位於 tests/images/ 目錄下，或者直接使用檔名（取決於執行環境）
  
  await page.setInputFiles('#certificateFileInput', 'tests/images/1.jpg');
  //await page.getByRole('button', { name: '  上傳憑證' }).setInputFiles('tests/images/1.jpg');
  await page.getByRole('button', { name: '  儲存' }).click();
  await page.getByRole('button', { name: '確定' }).click();


  // 定位到包含 "成功" 文本的那一行 (row)
  const successRow = page.locator('table tbody tr').filter({ hasText: '成功' });

  // 抓取第一個欄位 (檔名) 與第三個欄位 (訊息)
  const fileName = await successRow.locator('td').nth(0).textContent();
  const message = await successRow.locator('td').nth(2).textContent();
  console.log(`檔名: ${fileName?.trim()}, 訊息: ${message?.trim()}`);

  // 斷言驗證
  await expect(successRow).toBeVisible();
  await expect(successRow).toContainText('上傳成功');
});
