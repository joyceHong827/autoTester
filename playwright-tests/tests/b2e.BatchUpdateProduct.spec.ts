import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';
// b2e 大批修改商品測試腳本，針對員購商品進行修改
test('test', async ({ page }) => {
  await b2eLogin(page);
  await page.getByRole('link', { name: ' 商品管理' }).click();
  await page.getByRole('link', { name: '維護商品資料', exact: true }).click();
  
  //下拉式選單選項  
  //   { value: "0", text: "--未選擇--" },
  //   { value: "1", text: "網路商品" },
  //   { value: "2", text: "電視商品" },
  //   { value: "4", text: "型錄商品" },
  //   { value: "5", text: "OB商品" },
  //   { value: "6", text: "直播商品" },
  //   { value: "7", text: "ECTV商品" },
  //   { value: "8", text: "IB商品" },
  //   { value: "9", text: "藥妝店" },
  //   { value: "10", text: "電子票券" }
  await page.locator('span').filter({ hasText: '--未選擇--網路商品電視商品型錄商品OB商品直播商品' }).getByRole('combobox').selectOption('8');
  await page.getByRole('button', { name: ' 查詢' }).click();

  // 取得表格中「第一行」的所有儲存格，並挑選包含銷編的那一個 (假設是第 2 個 td)
  const row = page.locator('#dataList_table tbody tr').first();
  const saleId = await row.locator('td').nth(1).locator('span').textContent();

  await page.getByRole('link', { name: ' 商品管理' }).click();
  await page.getByRole('link', { name: '維護大批商品' }).click();
  await page.getByRole('radio', { name: '員購' }).check();
  await page.locator('div:nth-child(20) > .status').selectOption('1'); //員購 1:是 2:否
  await page.locator('#txtareaCodeList').click();
  await page.locator('#txtareaCodeList').fill(saleId?.toString() || '');
  await page.getByRole('button', { name: ' 開始修改' }).click();
});