import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

// 情境1 & 情境4：商品開發&審核 - 點擊「商開審核」後，明細頁 ESV 價欄位顯示且可修改
test('B2E TVProduct 商開審核 明細頁顯示 ESV 價欄位且可修改 (#49829)', async ({ page }) => {
  await b2eLogin(page);

  await page.goto('http://b2e.lab.etzone.net/Web/TVProduct');
  await expect(page).toHaveURL(/TVProduct/);

  // 點擊「商開審核」按鈕載入商品列表
  await page.getByRole('button', { name: '商開審核' }).click();
  await page.waitForSelector('tbody tr td a', { timeout: 10000 });

  // 點擊第一筆商品連結進入明細頁
  await page.locator('tbody tr td a').first().click();
  await expect(page).toHaveURL(/TVProductDetail/);
  await page.waitForSelector('input[ng-model="prod.Product.ProductCommon.ESVPrice"]', { timeout: 15000 });

  // 【情境1】驗證 ESV 價欄位可見
  const esvInput = page.locator('input[ng-model="prod.Product.ProductCommon.ESVPrice"]');
  await expect(esvInput).toBeVisible();

  // 【情境4】驗證欄位可修改（商開審核進入時 ESV 價開放編輯）
  await expect(esvInput).toBeEditable();

  // 驗證可實際輸入數值
  await esvInput.fill('9900');
  await expect(esvInput).toHaveValue('9900');
});

// 情境2：商審作業 - 商品明細頁顯示 ESV 價欄位
test('B2E TVProductAuditOperation 商審作業 明細頁顯示 ESV 價欄位 (#49829)', async ({ page, context }) => {
  await b2eLogin(page);

  await page.goto('http://b2e.lab.etzone.net/Web/TVProductAuditOperation');
  await expect(page).toHaveURL(/TVProductAuditOperation/);

  // 點擊查詢載入商品列表
  await page.locator('button.btn-primary', { hasText: '查詢' }).first().click();
  await page.waitForTimeout(2000);

  // 商品連結開新分頁，等待新頁開啟
  const [newPage] = await Promise.all([
    context.waitForEvent('page'),
    page.locator('tbody tr td a').first().click(),
  ]);
  await newPage.waitForLoadState('domcontentloaded');
  await newPage.waitForSelector('input[ng-model="prod.Product.ProductCommon.ESVPrice"]', { timeout: 15000 });

  // 【情境2】驗證 ESV 價欄位可見
  const esvInput = newPage.locator('input[ng-model="prod.Product.ProductCommon.ESVPrice"]');
  await expect(esvInput).toBeVisible();
  await newPage.close();
});

// 情境3：銷售商品查詢 - 商品明細頁顯示 ESV 價欄位
test('B2E TvProductSearchPage 銷售商品查詢 明細頁顯示 ESV 價欄位 (#49829)', async ({ page, context }) => {
  await b2eLogin(page);

  await page.goto('http://b2e.lab.etzone.net/web/product/TvProductSearchPage');
  await expect(page).toHaveURL(/TvProductSearchPage/);

  // 點擊查詢按鈕，等待商品列表載入
  await page.locator('button.btn-primary', { hasText: '查詢' }).first().click();
  await page.waitForSelector('table tbody tr td a', { timeout: 15000 });

  // 商品連結會開新分頁
  const [newPage] = await Promise.all([
    context.waitForEvent('page'),
    page.locator('table tbody tr td a').first().click(),
  ]);
  await newPage.waitForLoadState('domcontentloaded');
  await newPage.waitForTimeout(2000);

  // 【情境3】驗證 ESV 價欄位存在（WebProductDetail 以 <th> 顯示）
  const esvTh = newPage.locator('th[ng-if="data.sma.eType === 2"]');
  await expect(esvTh).toBeVisible();
  await expect(esvTh).toHaveText('ESV價');
  await newPage.close();
});

// 情境6,7：匯出表單 - 會前會/商審/書審包含 ESV，特審隱藏
test('B2E TVProduct 商品查詢 匯出類型包含會前會/商審/書審/評分表，不含特審 (#49829)', async ({ page }) => {
  await b2eLogin(page);

  await page.goto('http://b2e.lab.etzone.net/Web/TVProduct');
  await expect(page).toHaveURL(/TVProduct/);

  // 點擊商品查詢按鈕展開匯出區塊
  await page.getByRole('button', { name: ' 商品查詢' }).click();
  await page.waitForTimeout(1000);

  // 【情境6】驗證匯出類型 select 包含 會前會/商審/書審/評分表
  const importTypeSelect = page.locator('select[ng-model="query.importType"]');
  await expect(importTypeSelect).toBeVisible();

  await expect(importTypeSelect.locator('option[value="1"]')).toHaveText('會前會');
  await expect(importTypeSelect.locator('option[value="2"]')).toHaveText('商審');
  await expect(importTypeSelect.locator('option[value="4"]')).toHaveText('書審');
  await expect(importTypeSelect.locator('option[value="5"]')).toHaveText('評分表');

  // 【情境7】驗證匯出類型中不包含特審
  const allOptions = await importTypeSelect.locator('option').allTextContents();
  expect(allOptions).not.toContain('特審');

  // 驗證匯出按鈕存在
  await expect(page.getByRole('button', { name: '匯出' })).toBeVisible();
});
