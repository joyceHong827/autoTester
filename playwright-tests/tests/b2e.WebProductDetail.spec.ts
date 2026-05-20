import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';
import { assert } from 'console';

test('test', async ({ page }) => {
  const productId = '1214815'; // 將原本寫死的 ID 改為變數

  // 使用封裝好的登入 function
  await b2eLogin(page, 'role.mdmp', '123456');
  await page.goto("http://b2e.lab.etzone.net/web/product");

  // 輸入商品 ID 並查詢
  const searchInput = page.getByRole('textbox').first();
  await searchInput.click();
  await searchInput.fill(productId);
  await page.getByRole('button', { name: ' 查詢' }).click();

  // 點擊查詢結果中的連結
  // 原本是寫死的名稱 'Lingsun_邊界_49175'，
  // 建議改為根據商品 ID 點擊第一個出現的連結，通常較為穩健。
  //await page.locator(`a:has-text("${productId}")`).first().click();
// 確保第一列的第三欄已經出現在畫面上
const link = page.locator('#dataList_table [product-detail-page-link]').first();
console.log('找到的連結文本:', await link.textContent());
await link.click();
const haveCostColum = await validHasCostColumn(page);
const businessCost = await getBusinessCost(page);
console.log('是否有成本欄位:', haveCostColum);
console.log('業務成本:', businessCost);
//assert(haveCostColum, '表格中應該包含成本相關的欄位');
expect(businessCost).toEqual('1,305');
});

async function validHasCostColumn(page) {

    // // 定位包含「新成本」且裡面有 font 標籤的 th
    const newCostTh = page.locator('th').filter({ has: page.locator('font'), hasText: '新成本' });
    // 關鍵：這行會一直等，直到 UI 真的出現這個欄位為止 (預設等 30 秒)
    await newCostTh.waitFor({ state: 'visible' });    

    
    // const xpathSelectorForCost = '//th[contains(., "成本")]';
     const xpathSelectorForNewCost = '//th[contains(., "新成本")]';
    // const hasCost = await page.locator(xpathSelectorForCost);
     const hasNewCost =await page.locator(xpathSelectorForNewCost);  

    
    if (await hasNewCost.isVisible()) {
        return true
    } else {
        return false;
    }  
}

async function getBusinessCost(page) {
    const targetCellLink = page.locator('tbody tr >> span[ng-bind="obKitTotalBusinessCost | intCurrency"]').first();
    console.log(targetCellLink);
    return await targetCellLink.textContent();
}
