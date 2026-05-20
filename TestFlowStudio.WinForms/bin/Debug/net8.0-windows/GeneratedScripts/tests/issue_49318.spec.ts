import { test, expect } from '@playwright/test';
import { b2bLogin } from './b2bLogin';

test('Issue #49318: Brand selection "無" should not jump back to "尚未選擇"', async ({ page }) => {
  // 1. 登入 B2B 供應商後台
  await b2bLogin(page);
  await page.waitForURL('http://b2b.lab.etzone.net/Web/DefaultPage');

  // 2. 導航至商品上架頁面 (泛電視/OB商品 e=2)
  await page.goto('http://b2b.lab.etzone.net/Web/ProductManager?e=2');

  // 3. 定位品牌選擇按鈕
  // 初始狀態應顯示「尚未選擇」
  const brandButton = page.getByRole('button', { name: '尚未選擇' });
  await expect(brandButton).toBeVisible();
  await brandButton.click();

  // 4. 在品牌選單中選擇「無」
  // 參考 b2b.tvSubmit.spec.ts 的寫法，品牌選項應為按鈕格式
  const noneOption = page.getByRole('button', { name: '無', exact: true });
  await expect(noneOption).toBeVisible();
  await noneOption.click();

  // 5. 驗證結果
  // 選擇「無」之後，原本的品牌按鈕文字應該變為「無」
  const selectedBrand = page.getByRole('button', { name: '無', exact: true });
  await expect(selectedBrand).toBeVisible();

  // 額外等待一段時間，確認不會跳回「尚未選擇」
  await page.waitForTimeout(2000);
  
  // 再次確認文字是否仍為「無」
  const brandText = await selectedBrand.innerText();
  expect(brandText.trim()).toBe('無');
  
  // 確保「尚未選擇」按鈕不存在 (或已變更為「無」)
  await expect(page.getByRole('button', { name: '尚未選擇' })).not.toBeVisible();
});
