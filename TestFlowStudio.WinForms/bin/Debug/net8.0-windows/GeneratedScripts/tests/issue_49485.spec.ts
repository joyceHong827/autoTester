import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

/**
 * Redmine #49485: 大批維護新增OB專用欄位
 * 測試範圍：商品管理 > 維護大批商品 > 商品欄位
 */
test('Redmine #49485 - 驗證維護大批商品之OB專用欄位', async ({ page }) => {
  // 1. 系統登入
  await b2eLogin(page);

  // 2. 導航至「商品管理 > 維護大批商品」
  await page.getByRole('link', { name: ' 商品管理' }).click();
  await page.getByRole('link', { name: '維護大批商品' }).click();

  // 3. 展開「OB專用欄位」區塊
  await page.getByRole('link', { name: '  OB專用欄位' }).click();

  // 4. 操作「主要功能」欄位
  await page.getByText('主要功能').click();
  await page.getByRole('textbox', { name: '輸入文字快速查詢' }).first().click();
  await page.getByRole('textbox', { name: '輸入文字快速查詢' }).first().click();
  await page.getByRole('textbox', { name: '輸入文字快速查詢' }).first().fill('寵');
  await page.getByText('寵物-腸胃保健').click();

  // 5. 操作「商品回購小分類」欄位
  await page.getByText('商品回購小分類').click();
  await page.getByRole('textbox', { name: '輸入文字快速查詢' }).nth(1).click();
  await page.getByRole('textbox', { name: '輸入文字快速查詢' }).nth(1).fill('寵');
  await page.getByText('寵物雲-寵物毛視優').click();

  // 6. 操作「劑型」欄位
  await page.getByText('劑型').click();
  await page.locator('div:nth-child(5) > .status').selectOption('1003');

  // 7. 操作「OB買斷」欄位
  await page.getByText('OB買斷').click();
  await page.locator('div:nth-child(7) > .status').selectOption('1');

  // 8. 操作「自然美商品」欄位
  await page.getByText('自然美商品').click();
  await page.locator('div:nth-child(9) > .status').selectOption('1');

  // 9. 操作「直消商品」欄位
  await page.getByText('直消商品').click();
  await page.locator('div:nth-child(11) > .status').selectOption('1');

  // 10. 操作「專員顯示」欄位
  await page.getByText('專員顯示').click();
  await page.locator('div > div:nth-child(13) > .status').selectOption('1');

  // 11. 截圖存證
  await page.screenshot({ path: 'tests/images/issue_49485_ob_fields_result.png', fullPage: true });
});
