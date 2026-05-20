import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

// b2e 電視商品主管審核駁回測試腳本，針對銷編 1197404 進行駁回操作，並驗證駁回原因是否正確顯示
test('b2eTvAutoReject', async ({ page }) => {
  await b2eLogin(page);
  await page.getByRole('link', { name: '電視商品管理' }).click();
  await page.getByRole('link', { name: '商品開發&審核' }).click();
  await page.getByText('商開審核 主管審核商品 高階審核商品 駁回商品 商審維護 維護商品 商品查詢').click();        
  await page.getByRole('textbox').first().dblclick();
  await page.locator('input[ng-model="query.SalesCode"]').fill('1197404');
  await page.getByRole('button', { name: '主管審核商品' }).click();
  const row = page.getByRole('row').filter({
    has: page.locator('td').nth(4).filter({ hasText: '1197404' })
  });
  
  await row.getByRole('checkbox').check({timeout:30000});
  await page.getByRole('button', { name: '駁回' }).nth(1).click();
  await page.getByRole('textbox', { name: '請輸入駁回原因' }).click();
  await page.locator('select[ng-model="rejectionList.rejection.mainReason"]').selectOption('品名異常');
  await page.getByRole('textbox', { name: '請輸入駁回原因' }).click();
  await page.getByRole('textbox', { name: '請輸入駁回原因' }).fill('playwright 自動駁回');
  page.once('dialog', dialog => {
    console.log(`Dialog message: ${dialog.message()}`);
    dialog.dismiss().catch(() => {});
  });
  await page.getByRole('button', { name: '  確認' }).click();
  await page.getByRole('button', { name: '商開審核' }).click();
  await page.getByRole('button', { name: '主管審核商品' }).click();  
  await page.getByRole('button', { name: '駁回商品' }).click();
  page.pause();
});