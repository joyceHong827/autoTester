import { test, expect, Page } from '@playwright/test';
import { b2bLogin } from './b2bLogin';
import { time } from 'console';

test('問題回報功能測試', async ({ page }) => {
  // 定義變數
  const orderId = '100213039';
  const subject = '測試特殊訂單1';
  const content = '測試特殊訂單';
  const itemId = '1';

  // 1. 呼叫 b2bLogin function 進行登入
  await b2bLogin(page);

  // 2. 進入問題回報頁面並新增
  await page.waitForURL('http://b2b.lab.etzone.net/Web/DefaultPage');
  await page.goto('http://b2b.lab.etzone.net/Web/IssueManagement');  
  await page.waitForTimeout(2000);
  
  await page.getByText('新增', { exact: true }).click();
  
  // 選擇問題回報類型  
  // 1	訂單/銷退相關
  // 2	廠商資料維護
  // 3	貨款/費用相關
  // 4	商品相關
  // 5	合約相關
  await page.locator('#issueForm').getByRole('combobox').selectOption('1'); //問題回報 1訂單/銷退相關 2系統相關 3其他
  

  // 3. 使用變數填寫資料
  await page.getByRole('textbox', { name: '請輸入訂單編號' }).fill(orderId);
  await page.getByRole('textbox', { name: '請輸入項次' }).fill(itemId);
  await page.getByRole('textbox', { name: '請輸入主旨' }).fill(subject);
  await page.getByRole('textbox', { name: '請輸入內容' }).fill(content);

  await page.getByRole('button', { name: '確定' }).click();
  await page.getByRole('button', { name: 'OK' }).click();

  // 4. 使用 await 呼叫自定義查詢函式，並傳入對應變數
  const orderItemId = await queryIssue(page, orderId, itemId);
  
  // 驗證查詢結果
  expect(orderItemId).toBe(`${orderId}-${itemId}`);
});

/**
 * 查詢問題回報
 */
async function queryIssue(page: Page, orderId: string, itemId: string) {
    await page.waitForTimeout(2000);
    // 選擇查詢條件 (假設 nth(1) 是狀態或類型下拉選單)
    await page.getByRole('combobox').nth(1).selectOption('2');        
    await page.getByRole('textbox', { name: '請輸入訂單編號' }).fill(orderId);    
    await page.getByRole('textbox', { name: '請輸入項次' }).fill(itemId);
    await page.getByText('搜尋', { exact: true }).click();

    // 取得第一筆資料列的欄位名稱【編號】的值，並回傳
   // const orderItemId = await page.getByRole('row').nth(1).getByRole('cell', { name: '編號' }).textContent();
    const orderItemId = await page.getByRole('row').nth(1).locator('td').nth(4).textContent();
    console.log(`查詢到的問題回報編號: ${orderItemId}`);

    if (orderItemId === null) {
        return null;
    }
    return orderItemId.trim();
}
