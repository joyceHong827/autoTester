import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

/**
 * Redmine #49319: 修改 OB KIT 商品不寫富購之驗證
 * 
 * 驗證項目:
 * 1. 自動登入 B2E 系統
 * 2. 模擬 OB KIT 商品修改流程
 * 3. 驗證儲存動作未觸發富購同步機制 (不應出現欄位不存在或同步失敗報錯)
 */

const PRODUCT_CODE = '1214841'; // 參考 b2e.ModifyProduct.spec.ts 使用之銷編

test('issue_49319: Modify OB KIT Product and Verify No Fugo Sync Error', async ({ page }) => {
  // 1. 自動登入 B2E 系統
  await b2eLogin(page);

  // 2. 模擬 OB KIT 商品修改流程
  // 進入商品管理與維護商品資料
  await page.getByRole('link', { name: ' 商品管理' }).click();
  await page.getByRole('link', { name: '維護商品資料', exact: true }).click();

  // 輸入銷編
  const productInput = page.getByRole('textbox').first();
  await productInput.click();
  await productInput.fill(PRODUCT_CODE);

  // 點擊維護商品
  await page.getByRole('button', { name: '維護商品' }).click();

  // 點擊 playwrightKit 提報 (進入 KIT 商品編輯頁面)
  await page.getByRole('link', { name: 'playwrightKit提報' }).click();

  // 處理可能出現的對話框
  page.once('dialog', dialog => {
    console.log(`Dialog message: ${dialog.message()}`);
    dialog.accept().catch(() => {});
  });

  // 修改欄位資訊
  const textarea = page.locator('textarea[ng-model="data.sma.prdDesPlanner"]');
  const originalText = await textarea.inputValue();
  const timestamp = new Date().getTime();
  const newText = `${originalText.split('_mod_')[0]}_mod_${timestamp}`;
  
  console.log(`修改前內容: ${originalText}`);
  await textarea.fill(newText);
  console.log(`修改後內容: ${newText}`);

  // 3. 驗證儲存動作未觸發富購同步機制
  await page.getByRole('button', { name: '儲存' }).click();

   // 處理可能出現的對話框
   page.once('dialog', dialog => {
    console.log(`Dialog message: ${dialog.message()}`);
    dialog.accept().catch(() => {});
  });

  // 等待 SweetAlert 彈窗出現
  const swalTextElement = page.locator('.swal2-content');
  await swalTextElement.waitFor({ state: 'visible', timeout: 15000 });

  const fullText = await swalTextElement.textContent();  
  console.log('儲存結果彈窗內容:', fullText);
  expect(fullText).toBe("已完成修改。"); 

  // 截圖存檔
  const screenshotPath = `tests/images/issue_49319_result.png`;
  await page.screenshot({ path: screenshotPath, fullPage: true });
  console.log(`測試截圖已存至: ${screenshotPath}`);

  console.log('issue_49319 測試腳本執行完成：已驗證 OB KIT 修改流程且未觸發同步錯誤。');
});
