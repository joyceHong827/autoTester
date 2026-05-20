import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

// b2e 競網商品比價查詢測試腳本
test('Test', async ({ page }) => {
  await b2eLogin(page);
  await page.getByRole('link', { name: ' 商品管理' }).click();
  await page.getByRole('link', { name: '維護商品資料', exact: true }).click();  

  //step 1 - 進入維護商品資料頁面，選擇網路商品，點擊維護商品，查詢後關閉視窗，productype = 1  await page.locator('span').filter({ hasText: '--未選擇--網路商品電視商品型錄商品OB商品直播商品' }).getByRole('combobox').selectOption('1');//網路商品 已提報成功有銷編的商品
  await page.getByRole('button', { name: '維護商品' }).click();
  await page.getByText('查', { exact: true }).first().click();
  await page.getByRole('button', { name: '   關閉' }).click();

  // step 2 - 進入維護商品資料頁面，選擇電視商品，點擊維護商品，查詢後關閉視窗，productype = 2 網路商品待審核的商品的商品
  await page.getByRole('button', { name: '商開審核新品' }).click();
  await page.getByText('查', { exact: true }).first().click();
  await page.getByRole('button', { name: '   關閉' }).click();

  // step 3 - 進入維護商品資料頁面，選擇電視商品，點擊商開審核，查詢後關閉視窗，productype = 3 電視商品待審核的商品
  await page.getByRole('link', { name: '電視商品管理' }).click();
  await page.getByRole('link', { name: '商品開發&審核' }).click();
  await page.getByRole('button', { name: '商開審核' }).click();
  await page.getByRole('button', { name: '維護商品' }).click();
  await page.getByText('查', { exact: true }).first().click();
  await page.getByRole('button', { name: '   關閉' }).click();

//step 4 - 進入維護商品資料頁面，選擇電視商品，點擊銷售商品查詢，查詢後關閉視窗，productype = 4 電視商品已提報成功有銷編的商品
  await page.getByRole('link', { name: '電視商品管理' }).click();
  await page.getByRole('link', { name: '銷售商品查詢' }).click();
  await page.getByRole('button', { name: ' 查詢' }).click();
  await page.getByText('查', { exact: true }).first().click();
  await page.getByRole('button', { name: '   關閉' }).click(); 
});

test('Test2', async ({ page }) => {
  await b2eLogin(page);
  await page.getByRole('link', { name: ' 商品管理' }).click();
  await page.getByRole('link', { name: '競網商品比價查詢' }).click();
  await page.getByRole('textbox', { name: '請輸入銷編，每行一筆，最多10筆' }).click();
  await page.getByRole('textbox', { name: '請輸入銷編，每行一筆，最多10筆' }).fill('1214368');
  await page.getByRole('button', { name: '查詢' }).click();
});