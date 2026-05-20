import { test, expect, Page } from '@playwright/test';
import { b2bLogin } from './b2bLogin';

/**
 * Redmine #49422 - 問題回報的訂單調整判斷SP
 * 驗證不同類型的訂單（廠送、庫送、特約）是否都能正確在問題回報功能中使用。
 */
test.describe('Issue #49422 - 問題回報訂單判斷邏輯測試', () => {

  test.beforeEach(async ({ page }) => {
    // 1. 執行登入
    await b2bLogin(page);
    await page.waitForURL('**/Web/DefaultPage');
    
    // 2. 進入問題回報頁面
    await page.goto('http://b2b.lab.etzone.net/Web/IssueManagement');
    await page.waitForTimeout(1000);
  });

  // 測試案例 1：廠送訂單
  test('驗證廠送訂單可以正常提交問題回報', async ({ page }) => {
    const testData = {
      orderId: '100307266',
      itemId: '1',
      subject: '49422-廠送訂單測試',
      content: '測試廠送訂單判斷邏輯',
      caseName: 'factory_delivery'
    };
    await runIssueSubmitTest(page, testData);
  });

  // 測試案例 2：庫送訂單
  test('驗證庫送訂單可以正常提交問題回報', async ({ page }) => {
    const testData = {
      orderId: '103231506',
      itemId: '2',
      subject: '49422-庫送訂單測試',
      content: '測試庫送訂單判斷邏輯',
      caseName: 'warehouse_delivery'
    };
    await runIssueSubmitTest(page, testData);
  });

  // 測試案例 3：特約訂單
  test('驗證特約訂單可以正常提交問題回報', async ({ page }) => {
    const testData = {
      orderId: '100213039',
      itemId: '1',
      subject: '49422-特約訂單測試',
      content: '測試特約訂單判斷邏輯',
      caseName: 'special_order'
    };
    await runIssueSubmitTest(page, testData);
  });
});

/**
 * 執行問題回報提交與驗證的共用邏輯
 */
async function runIssueSubmitTest(page: Page, data: { orderId: string, itemId: string, subject: string, content: string, caseName: string }) {
  // 點擊新增
  await page.getByText('新增', { exact: true }).click();
  
  // 選擇問題回報類型：1 訂單/銷退相關
  await page.locator('#issueForm').getByRole('combobox').selectOption('1');
  
  // 填寫訂單資訊
  await page.getByRole('textbox', { name: '請輸入訂單編號' }).fill(data.orderId);
  await page.getByRole('textbox', { name: '請輸入項次' }).fill(data.itemId);
  await page.getByRole('textbox', { name: '請輸入主旨' }).fill(data.subject);
  await page.getByRole('textbox', { name: '請輸入內容' }).fill(data.content);

  // 提交前截圖
  await page.screenshot({ path: `tests/images/issue_49422_${data.caseName}_input.png` });

  // 提交
  await page.getByRole('button', { name: '確定' }).click();
  
  // 處理可能的成功彈窗
  const okButton = page.getByRole('button', { name: 'OK' });
  await expect(okButton).toBeVisible({ timeout: 5000 });
  await okButton.click();

  // 驗證：查詢剛建立的問題
  const orderItemId = await queryIssue(page, data.orderId, data.itemId);
  
  // 查詢結果截圖
  await page.screenshot({ path: `tests/images/issue_49422_${data.caseName}_result.png` });

  expect(orderItemId).toBe(`${data.orderId}-${data.itemId}`);
}

/**
 * 查詢問題回報
 */
async function queryIssue(page: Page, orderId: string, itemId: string) {
    await page.waitForTimeout(2000);
    // 選擇查詢條件
    await page.getByRole('combobox').nth(1).selectOption('2'); // 假設 2 是「處理中」或特定狀態
    await page.getByRole('textbox', { name: '請輸入訂單編號' }).fill(orderId);    
    await page.getByRole('textbox', { name: '請輸入項次' }).fill(itemId);
    await page.getByText('搜尋', { exact: true }).click();

    // 取得結果清單中第一筆資料的編號欄位 (根據參考檔案，編號在第 5 個 td)
    const orderItemId = await page.getByRole('row').nth(1).locator('td').nth(4).textContent();
    console.log(`查詢到的訂單項次編號: ${orderItemId}`);

    return orderItemId ? orderItemId.trim() : null;
}
