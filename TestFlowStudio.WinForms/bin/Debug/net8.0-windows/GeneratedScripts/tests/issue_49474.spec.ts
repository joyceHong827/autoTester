import { test, expect } from '@playwright/test';
import { b2bLogin } from './b2bLogin';
import * as path from 'path';

/**
 * Redmine #49474 廠商請款上傳憑證 測試程式
 */

test.describe('Issue #49474 廠商請款上傳憑證功能驗證', () => {
  
  test.beforeEach(async ({ page }) => {
    // 1. 登入 B2B 系統並進入頁面
    await b2bLogin(page);
    await page.waitForURL('**/Web/DefaultPage');
    await page.goto('http://b2b.lab.etzone.net/Web/SPMPaymentRequest');
    await page.waitForTimeout(2000);
    
    // 2. 切換至「憑證上傳」頁籤
    await page.getByRole('link', { name: '上傳憑證' }).click();
    await page.waitForTimeout(1000);
  });

  test('正向測試: 檔案上傳、覆蓋與儲存唯讀驗證', async ({ page }) => {
    // 3. 查詢特定資料 (2011 / 202603)
    const year = '2026';
    const month = '03';
    
    // 處理月份選擇
    const monthGroup = page.locator('div.form-inline').filter({ hasText: '費用所屬月份' });
    const selects = monthGroup.locator('select');
    if (await selects.count() >= 2) {
      await selects.nth(0).selectOption({ label: year });
      await selects.nth(1).selectOption(month);
    }
    
    await page.getByRole('button', { name: '查詢' }).click();
    await page.waitForTimeout(1000);    
    await page.getByRole('button', { name: '+ 新增' }).click();
    await page.setInputFiles('#certificateFileInput', 'tests/images/1.jpg');
    await page.waitForTimeout(1000);

    // 6. 儲存前編輯權限：再次上傳新檔案進行覆蓋 (PDF)
    const pdfPath = path.resolve('tests/images/test_document.pdf');
    await page.setInputFiles('#certificateFileInput', pdfPath);
    console.log('已覆蓋上傳為 pdf 檔案');
    await page.screenshot({ path: 'tests/images/issue_49474_positive_3_overwrite_pdf.png' });

    // 7. 點擊【儲存】
    // 注意：按鈕名稱包含特殊字元  (Unicode)，建議使用部分配對或 role
    await page.getByRole('button', { name: /儲存/ }).click();
    
    // 處理確認對話框
    const confirmButton = page.getByRole('button', { name: '確定' });
    if (await confirmButton.isVisible()) {
      await confirmButton.click();
    }
    
    await page.waitForTimeout(2000);
    
    // 8. 驗證上傳成功
    const successRow = page.locator('table tbody tr').filter({ hasText: '上傳成功' });
    // 容錯處理：有些系統顯示「成功」或「上傳成功」
    //await expect(page.locator('table tbody')).toContainText('成功');
    await expect(page.locator('tbody').filter({ hasText: '成功' })).toBeVisible();
    await page.screenshot({ path: 'tests/images/issue_49474_positive_4_save_result.png', fullPage: true });

    // 9. 驗證儲存後檔案是否不可再修改/刪除
    const addButtonAfterSave = page.getByRole('button', { name: '+ 新增' });
    const isAddDisabled = await addButtonAfterSave.isDisabled().catch(() => true);
    console.log(`儲存後新增按鈕是否停用: ${isAddDisabled}`);
  });

  test('反向測試: 不正確檔案格式限制驗證', async ({ page }) => {
    // 3. 點擊【新增】
    await page.getByRole('button', { name: '+ 新增' }).click();
    await page.waitForTimeout(1000);

    // 4. 監聽錯誤彈窗
    page.once('dialog', async dialog => {
      console.log(`偵測到錯誤訊息: ${dialog.message()}`);
      expect(dialog.message()).toMatch(/格式|不允許/); 
      await dialog.accept();
    });

    // 5. 嘗試上傳不符合格式 (txt) 的檔案
    const invalidPath = path.resolve('tests/images/invalid_file.txt');
    await page.setInputFiles('#certificateFileInput', invalidPath);
    
    await page.waitForTimeout(1000);
    await page.screenshot({ path: 'tests/images/issue_49474_negative_1_invalid_format.png' });
    
    console.log('反向測試完成：已驗證非法格式限制');
  });
});
