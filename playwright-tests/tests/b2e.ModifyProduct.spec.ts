import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';
import { assert } from 'console';

/**
 * 銷編 (Product Code) 
 * 可以從環境變數或外部設定檔取得，此處先設為常數
 */
const PRODUCT_CODE = '1214841';
// 測試名稱：B2E 修改商品資料測試腳本
test('Modify Product', async ({ page }) => {
  // 呼叫 B2E 登入功能
  await b2eLogin(page);

  // 進入商品管理與維護商品資料
  await page.getByRole('link', { name: ' 商品管理' }).click();
  await page.getByRole('link', { name: '維護商品資料', exact: true }).click();

  // 輸入銷編
  const productInput = page.getByRole('textbox').first();
  await productInput.click();
  await productInput.fill(PRODUCT_CODE);

  // 點擊維護商品 (原本程式碼點了兩次，保留邏輯)
  await page.getByRole('button', { name: '維護商品' }).click();
  await page.getByRole('button', { name: '維護商品' }).click();

  // 點擊 playwrightKit 提報
  await page.getByRole('link', { name: 'playwrightKit提報' }).click();

  // 處理對話框
  page.once('dialog', dialog => {
    console.log(`Dialog message: ${dialog.message()}`);
    dialog.dismiss().catch(() => {});
  });


// 2. 觸發按鈕 (確保這裡的 selector 是正確的)
// 根據你的截圖，這應該是觸發「系統提醒：OB商品...」那個視窗的按鈕
await page.getByRole('button', { name: '儲存' }).click();

  // 等待 SweetAlert 彈窗出現
  const swalTextElement = page.locator('.swal2-content');
  await swalTextElement.waitFor({ state: 'visible', timeout: 15000 });

  const fullText = await swalTextElement.textContent();  
  console.log('儲存結果彈窗內容:', fullText);
  expect(fullText).toBe("已完成修改。"); 
});
