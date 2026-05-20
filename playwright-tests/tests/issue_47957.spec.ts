import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

const products = {
  sdProduct: '1214815', // SD 負責的商品 (OB KIT)
  otherProduct: '1214845' // 非 SD 負責的商品
};

const users = [
  { role: 'SD', username: 'role.mdmp', password: '123456' },
  { role: 'Other', username: 'role.md', password: '123456' }
];

test.describe('成本價和業務成本的查詢權限 (Redmine #47957)', () => {
  for (const user of users) {
    test(`Role: ${user.role} - 驗證商品權限`, async ({ page }) => {
      // 1. 登入
      await b2eLogin(page, user.username, user.password);

      // 2. 處理彈窗
      const laterButton = page.getByRole('button', { name: '彈窗:下次再說', includeHidden: true }).or(page.getByRole('button', { name: '下次再說' }));
      if (await laterButton.isVisible({ timeout: 5000 }).catch(() => false)) {
        await laterButton.click();
      }

      // 3. 進入商品查詢頁面
      await page.getByRole('link', { name: ' 商品管理' }).click();
      await page.getByRole('link', { name: '維護商品資料' }).click();

      const testProducts = [products.sdProduct, products.otherProduct];

      for (const pid of testProducts) {
        // 輸入 ID 並查詢
        const searchInput = page.getByRole('textbox').first();
        await searchInput.clear();
        await searchInput.fill(pid);
        await page.getByRole('button', { name: ' 查詢' }).click();
        
        // 等待查詢結果載入
        await page.waitForTimeout(1000); 

        // 截圖查詢清單
        await page.screenshot({ path: `tests/images/issue_47957_${user.role}_list_${pid}.png` });

        // 檢查清單中的成本顯示 (這裡需要具體的 selector, 先假設有對應的欄位)
        // 點擊進入明細頁
        const link = page.locator('#dataList_table [product-detail-page-link]').first();
        if (await link.isVisible()) {
          console.log('找到的連結文本:', await link.textContent());
          await link.click();
          await page.waitForTimeout(2000); // 等待明細載入

          // 截圖明細頁
          await page.screenshot({ path: `tests/images/issue_47957_${user.role}_detail_${pid}.png` });

          // 返回查詢頁面以進行下一個商品測試
          await page.goBack();
          await page.waitForTimeout(1000);
        }
      }
    });
  }
});
