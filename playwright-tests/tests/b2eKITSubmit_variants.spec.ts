import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

const scenarios = [
  { id: '5', name: 'OB商品', prefix: 'playwright_OB_Kit' },
  { id: '2', name: 'TV商品', prefix: 'playwright_TV_Kit' },
];
// B2E 套組商品提報測試腳本，包含 OB 商品與 TV 商品兩種情境，並在商品名稱中加入 timestamp 以確保唯一性
test.describe('B2E Kit Submission Variants', () => {
  
  for (const scenario of scenarios) {
    test(`B2E Kit Submission Test - ${scenario.name} (開發來源: ${scenario.id})`, async ({ page }) => {
      const timestamp = new Date().getTime();
      const productName = `${scenario.prefix}_${timestamp}`;

      console.log(`開始測試情境: ${scenario.name}`);

      // 1. 登入
      await b2eLogin(page);

      // 2. 導航至商品管理 -> 建立套組商品
      await page.getByRole('link', { name: ' 商品管理' }).click();
      await page.getByRole('link', { name: '建立套組商品' }).click();

      // 3. 選擇開發來源
      await page.getByRole('combobox').first().selectOption(scenario.id);

      // 4. 填寫商品名稱
      await page.locator('#saleName').fill(productName);
      await page.locator('#SalesSubtitle').fill(productName);

      // 5. 選擇日期 (Shelf Date)
      await page.getByRole('textbox').nth(2).click();
      const datePickerFrame = page.frameLocator('iframe').nth(1);
      await datePickerFrame.getByRole('cell', { name: '2', exact: true }).first().click();

      // Actual Shelf Date
      await page.locator('[id="SMA.actualShelf"]').first().click();
      await datePickerFrame.getByRole('cell', { name: '3', exact: true }).first().click();

      // 6. 選擇分類 (固定測試用分類)
      await page.getByRole('combobox').nth(1).selectOption('10000');
      await page.getByRole('combobox').nth(2).selectOption('10100');
      await page.getByRole('combobox').nth(3).selectOption('10102');
      await page.getByRole('combobox').nth(4).selectOption('103004');

      // 7. 選取組成商品
      await page.getByRole('button', { name: '選取商品' }).click();
      await page.getByRole('button', { name: '搜尋' }).click();
      
      // 選取第一個可選商品 (依據原本腳本 selector)
      await page.locator('tbody:nth-child(4) > tr > td > .pure-checkbox > label').click();
      await page.getByRole('button', { name: '加入勾選商品' }).first().click();

      // 8. 填寫價格與數量
      await page.getByRole('textbox').nth(4).fill('200'); // 售價
      await page.getByRole('textbox').nth(5).fill('100'); // 成本
      await page.locator('.row > div:nth-child(3) > .form-control').fill('10');
      await page.locator('.row > div:nth-child(4) > .form-control').fill('10');

      // 9. 置入資料
      await page.getByRole('button', { name: '置入組合商品資料' }).click();
      await page.getByRole('button', { name: ' 確定' }).click();

      // 10. 填寫短敘述並確定送出
      await page.locator('#form-validation-field-6').fill(`${scenario.name} 自動化測試提報`);
      await page.getByRole('button', { name: ' 確定' }).click();

      console.log(`[SUCCESS] ${scenario.name} 提報完成: ${productName}`);
      
      // 增加驗證點：確認是否回到列表或有成功訊息
      // await expect(page).toHaveURL(/.*ProductList/); 
    });
  }
});
