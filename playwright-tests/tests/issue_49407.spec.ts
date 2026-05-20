import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

/**
 * # Redmine #49407 測試報告
 * 
 * ## 測試目的
 * 驗證「競網商品比價查詢」功能在輸入非數字銷售編號時的防呆機制，確保系統不會因 NullReferenceException 而崩潰。
 * 
 * ## 測試步驟
 * 1. 自動登入 B2E 系統。
 * 2. 進入「商品管理」->「競網商品比價查詢」頁面。
 * 3. 在銷售編號輸入框中輸入非數字文字（例如：「女」）。
 * 4. 點擊「查詢」按鈕。
 * 5. 觀察系統反應（攔截 Dialog 訊息或頁面提示）。
 * 
 * ## 測試資料
 * - 銷售編號：女、1214368
 * 
 * ## 預期結果
 * 1. 前端應針對銷售編號（尤其是 productType 1、4）加入數字格式驗證。
 * 2. 系統應攔截非數字輸入，顯示「請輸入數字格式」或類似的防呆提示。
 * 3. 在混合輸入（一筆正確、一筆錯誤）時，正確的銷編應能正常查詢，錯誤的應被攔截或優雅報錯，且後端不應產生 NullReferenceException。
 * 
 * ## 驗證項目
 * - [ ] 銷售編號欄位是否限制僅能輸入數字（前端驗證）。
 * - [ ] 輸入非數字點擊查詢時，是否彈出格式錯誤提示。
 * - [ ] 混合輸入測試：有效銷編正常顯示，無效銷編不影響其他結果。
 * - [ ] 系統是否穩定執行，無 500 錯誤或 NullReferenceException。
 */

test('issue_49407: 競網商品比價查詢_加上防呆機制驗證', async ({ page }) => {
  // 1. 自動登入 B2E 系統
  await b2eLogin(page);

  // 2. 進入「競網商品比價查詢」頁面
  await page.getByRole('link', { name: ' 商品管理' }).click();
  await page.getByRole('link', { name: '競網商品比價查詢' }).click();

  // 3. 輸入非數字的銷售編號（例如：女）
  const salesIdInput = page.getByRole('textbox', { name: '請輸入銷編，每行一筆，最多10筆' });
  await salesIdInput.click();
  await salesIdInput.fill('女');
  
  console.log('輸入非數字銷售編號: 女');

  // 5. 驗證防呆機制
  // 監聽對話框 (Dialog)
  page.on('dialog', async dialog => {
    console.log(`攔截到對話框訊息: ${dialog.message()}`);
    if (dialog.message().includes('數字') || dialog.message().includes('格式')) {
      console.log('驗證成功：系統正確彈出格式錯誤提示。');
    }
    await dialog.accept();
  });

  // 觸發查詢動作 (查詢)
  await page.getByRole('button', { name: '查詢' }).click();
  await page.waitForTimeout(1000);

  // 截圖存檔
  await page.screenshot({ path: `tests/images/issue_49407_non_numeric.png`, fullPage: true });
});

test('issue_49407: 競網商品比價查詢_正常數字輸入驗證', async ({ page }) => {
  await b2eLogin(page);
  await page.getByRole('link', { name: ' 商品管理' }).click();
  await page.getByRole('link', { name: '競網商品比價查詢' }).click();

  const salesIdInput = page.getByRole('textbox', { name: '請輸入銷編，每行一筆，最多10筆' });
  await salesIdInput.click();
  await salesIdInput.fill('1214368');
  await page.getByRole('button', { name: '查詢' }).click();

  
  console.log('issue_49407 正常輸入測試完成：已填寫銷編 1214368 並點擊查詢。');
});

test('issue_49407: 競網商品比價查詢_混合輸入驗證', async ({ page }) => {
  await b2eLogin(page);
  await page.getByRole('link', { name: ' 商品管理' }).click();
  await page.getByRole('link', { name: '競網商品比價查詢' }).click();

  const salesIdInput = page.getByRole('textbox', { name: '請輸入銷編，每行一筆，最多10筆' });
  await salesIdInput.click();
  // 第一行正確，第二行錯誤
  await salesIdInput.fill('1214368\n女');
  
  console.log('輸入混合銷售編號: 1214368 與 女');

  // 監聽對話框 (Dialog)
  page.on('dialog', async dialog => {
    console.log(`攔截到對話框訊息: ${dialog.message()}`);
    await dialog.accept();
  });

  await page.getByRole('button', { name: '查詢' }).click();
  await page.waitForTimeout(2000);

  // 截圖存檔
  await page.screenshot({ path: `tests/images/issue_49407_mixed.png`, fullPage: true });
  console.log('issue_49407 混合輸入測試完成。');
});
