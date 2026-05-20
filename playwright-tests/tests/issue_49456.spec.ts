import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

const productId = '1214815';
const users = [
  { role: 'SD', username: 'role.mdmp', password: '123456', description: '商開 (OB 通路)' },
  { role: 'Manager', username: 'william.li', password: '123456', description: '營業主管 (OB 通路)' },
  { role: 'Other', username: 'role.md', password: '123456', description: '一般帳號 (非 OB)' }
];

test.describe('OB KIT 子商品顯示業務成本 (Redmine #49456)', () => {
  test.setTimeout(90000);

  for (const user of users) {
    test(`驗證角色權限: ${user.description}`, async ({ page }) => {
      await b2eLogin(page, user.username, user.password);

      // 強制隱藏 SweetAlert2 遮罩，防止阻擋點擊
      await page.addStyleTag({ content: '.swal2-container { display: none !important; }' });

      const handlePopups = async () => {
        const laterButton = page.getByRole('button', { name: '下次再說' });
        if (await laterButton.isVisible({ timeout: 5000 }).catch(() => false)) {
          await laterButton.click();
        }
      };
      
      await handlePopups();

      await page.getByRole('link', { name: ' 商品管理' }).click();
      await page.getByRole('link', { name: '維護商品資料' }).click();
      
      await handlePopups();

      const searchInput = page.getByRole('textbox').first();
      await searchInput.click();
      await searchInput.fill(productId);
      await page.getByRole('textbox', { name: '輸入使用者姓名搜尋' }).fill(''); // 清空可能的預設值
      await page.getByRole('button', { name: ' 查詢' }).click();

      // 等待表格載入
      await page.waitForTimeout(5000);

      // 截圖查詢結果以便診斷
      await page.screenshot({ path: `tests/images/issue_49456_${user.role}_search_result.png` });

      // 嘗試點擊商品連結
      const productLink = page.locator('#dataList_table [product-detail-page-link]').first();
      
      if (await productLink.isVisible({ timeout: 10000 }).catch(() => false)) {
        console.log('找到的連結文本:', await productLink.textContent());
        await productLink.click();
        await page.waitForTimeout(3000);

        // 針對有權限的角色驗證成本顯示
        if (user.role === 'SD' || user.role === 'Manager') {
          const haveCostColum = await validHasCostColumn(page);
          const businessCost = await getBusinessCost(page);
          console.log(`[${user.role}] 是否有成本欄位:`, haveCostColum);
          console.log(`[${user.role}] 業務成本:`, businessCost);
          
          expect(haveCostColum).toBeTruthy();
          expect(businessCost).toEqual('1,305');
        } else {
          // 一般帳號驗證不應看到成本欄位
          const newCostTh = page.locator('th').filter({ has: page.locator('font'), hasText: '新成本' });
          const isVisible = await newCostTh.isVisible();
          const businessCost = await getBusinessCost(page);
          expect(isVisible).toBeFalsy();
          expect(businessCost).toEqual('1,305');
          console.log(`[${user.role}] 驗證正確：一般帳號無法看到成本欄位`);
        }

        await page.screenshot({ 
          path: `tests/images/issue_49456_${user.role}_detail.png`,
          fullPage: true 
        });
      } else {
        console.error(`Product link ${productId} not found for role ${user.role}`);
      }

      console.log(`Finished testing for ${user.role}`);
    });
  }
});

async function validHasCostColumn(page) {
    // 定位包含「新成本」且裡面有 font 標籤的 th
    const newCostTh = page.locator('th').filter({ has: page.locator('font'), hasText: '新成本' });
    // 關鍵：這行會一直等，直到 UI 真的出現這個欄位為止 (預設等 30 秒)
    await newCostTh.waitFor({ state: 'visible' });    

    const xpathSelectorForNewCost = '//th[contains(., "新成本")]';
    const hasNewCost = await page.locator(xpathSelectorForNewCost);  

    if (await hasNewCost.isVisible()) {
        return true;
    } else {
        return false;
    }  
}

async function getBusinessCost(page) {
    const targetCellLink = page.locator('tbody tr >> span[ng-bind="obKitTotalBusinessCost | intCurrency"]').first();
    return await targetCellLink.textContent();
}
