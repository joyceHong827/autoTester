import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

test.describe('Redmine 49299: IB Staff Purchase Settings', () => {

  test.beforeEach(async ({ page }) => {
    await test.step('登入 B2E 系統', async () => {
      await b2eLogin(page);
    });
  });

  async function getProductSaleId(page, etype: string) {
    return await test.step(`獲取商品類型為 ${etype} 的銷售編號`, async () => {
      await page.getByRole('link', { name: ' 商品管理' }).click();
      await page.getByRole('link', { name: '維護商品資料', exact: true }).click();
      
      // Select product type
      await page.locator('span').filter({ hasText: '--未選擇--網路商品電視商品型錄商品OB商品直播商品' }).getByRole('combobox').selectOption(etype);
      await page.getByRole('button', { name: ' 查詢' }).click();

      // Get the first sale ID from the table
      const row = page.locator('#dataList_table tbody tr').first();
      const saleId = await row.locator('td').nth(1).locator('span').textContent();
      return saleId?.trim();
    });
  }

  async function getMultipleProductSaleIds(page, etype: string, count: number) {
    return await test.step(`獲取 ${count} 筆商品類型為 ${etype} 的銷售編號`, async () => {
      await page.getByRole('link', { name: ' 商品管理' }).click();
      await page.getByRole('link', { name: '維護商品資料', exact: true }).click();
      
      await page.locator('span').filter({ hasText: '--未選擇--網路商品電視商品型錄商品OB商品直播商品' }).getByRole('combobox').selectOption(etype);
      await page.getByRole('button', { name: ' 查詢' }).click();

      const saleIds: string[] = [];
      for (let i = 0; i < count; i++) {
        const row = page.locator('#dataList_table tbody tr').nth(i);
        const saleId = await row.locator('td').nth(1).locator('span').textContent();
        if (saleId) saleIds.push(saleId.trim());
      }
      return saleIds;
    });
  }

  test('Scenario 1: Single IB Product - Add Staff Purchase', async ({ page }) => {
    const saleId = await getProductSaleId(page, '8'); // 8: IB商品
    if (!saleId) throw new Error('Could not find IB product');

    await test.step('導航至維護大批商品頁面', async () => {
      await page.getByRole('link', { name: ' 商品管理' }).click();
      await page.getByRole('link', { name: '維護大批商品' }).click();
    });
    
    await test.step('設定員購屬性為「是」並輸入商品編號', async () => {
      await page.getByRole('radio', { name: '員購' }).check();
      await page.locator('div:nth-child(20) > .status').selectOption('1'); // 1:是
      await page.locator('#txtareaCodeList').fill(saleId);
    });

    await test.step('點擊修改並處理確認對話框', async () => {
      page.once('dialog', dialog => dialog.accept());
      await page.getByRole('button', { name: ' 開始修改' }).click();
    });

    await test.step('驗證資料庫：IB 商品 ChannelCode 應為 86', async () => {
      await expect.poll(async () => {
        const dbResponse = await page.request.get(`http://localhost:3000/readEtmallStaffPurchase/${saleId}`);
        return await dbResponse.json();
      }, {
        message: '等待資料庫寫入員購記錄',
        timeout: 10000,
      }).toEqual(expect.arrayContaining([expect.objectContaining({ ChannelCode: 86 })]));
    });

    await test.step('驗證旗標 API：hasStaffPurchase 應為 true', async () => {
      await expect.poll(async () => {
        const response = await page.request.get(`http://localhost:3000/readSalesMixProductFlag/${saleId}`);
        const data = await response.json();
        return data.hasStaffPurchase;
      }, {
        message: '檢查 hasStaffPurchase 旗標狀態',
        timeout: 10000,
      }).toBe(true);
    });
  });

  test('Scenario 2: Single IB Product - Delete Staff Purchase', async ({ page }) => {
    const saleId = await getProductSaleId(page, '8');
    if (!saleId) throw new Error('Could find IB product');

    await test.step('導航至維護大批商品頁面', async () => {
      await page.getByRole('link', { name: ' 商品管理' }).click();
      await page.getByRole('link', { name: '維護大批商品' }).click();
    });
    
    await test.step('設定員購屬性為「否」並輸入商品編號', async () => {
      await page.getByRole('radio', { name: '員購' }).check();
      await page.locator('div:nth-child(20) > .status').selectOption('0'); // 0:否
      await page.locator('#txtareaCodeList').fill(saleId);
    });

    await test.step('點擊修改並處理確認對話框', async () => {
      page.once('dialog', dialog => dialog.accept());
      await page.getByRole('button', { name: ' 開始修改' }).click();
    });
    
    await test.step('驗證資料庫：員購記錄應被刪除', async () => {
      await expect.poll(async () => {
        const dbResponse = await page.request.get(`http://localhost:3000/readEtmallStaffPurchase/${saleId}`);
        return await dbResponse.json();
      }, {
        message: '等待資料庫刪除員購記錄',
        timeout: 15000,
      }).toEqual([]);
    });

    await test.step('驗證旗標 API：hasStaffPurchase 應為 false', async () => {
      await expect.poll(async () => {
        const response = await page.request.get(`http://localhost:3000/readSalesMixProductFlag/${saleId}`);
        const data = await response.json();
        return data.hasStaffPurchase;
      }, {
        message: '檢查 hasStaffPurchase 旗標狀態',
        timeout: 10000,
      }).toBe(false);
    });
  });

  test('Scenario 3: Batch IB Product Maintenance', async ({ page }) => {
    const saleIds = await getMultipleProductSaleIds(page, '8', 2);
    if (saleIds.length < 2) throw new Error('Could not find enough IB products');

    await test.step('導航至維護大批商品頁面', async () => {
      await page.getByRole('link', { name: ' 商品管理' }).click();
      await page.getByRole('link', { name: '維護大批商品' }).click();
    });
    
    await test.step('批次設定員購屬性為「是」並輸入多筆商品編號', async () => {
      await page.getByRole('radio', { name: '員購' }).check();
      await page.locator('div:nth-child(20) > .status').selectOption('1');
      await page.locator('#txtareaCodeList').fill(saleIds.join('\n'));
    });

    await test.step('點擊修改並處理確認對話框', async () => {
      page.once('dialog', dialog => dialog.accept());
      await page.getByRole('button', { name: ' 開始修改' }).click();
    });
    
    await test.step('驗證資料庫與旗標 API：所有 IB 商品 ChannelCode 應為 86 且 hasStaffPurchase 為 true', async () => {
      for (const id of saleIds) {
        // Verify Database
        await expect.poll(async () => {
          const dbResponse = await page.request.get(`http://localhost:3000/readEtmallStaffPurchase/${id}`);
          return await dbResponse.json();
        }, {
          message: `檢查商品 ${id} 的員購記錄`,
          timeout: 10000,
        }).toEqual(expect.arrayContaining([expect.objectContaining({ ChannelCode: 86 })]));

        // Verify Flag API
        await expect.poll(async () => {
          const response = await page.request.get(`http://localhost:3000/readSalesMixProductFlag/${id}`);
          const data = await response.json();
          return data.hasStaffPurchase;
        }, {
          message: `檢查商品 ${id} 的 hasStaffPurchase 旗標`,
          timeout: 10000,
        }).toBe(true);
      }
    });
  });

  test('Scenario 4: Regression Test (OB Product)', async ({ page }) => {
    const saleId = await getProductSaleId(page, '5'); // 5: OB商品
    if (!saleId) throw new Error('Could not find OB product');

    await test.step('導航至維護大批商品頁面', async () => {
      await page.getByRole('link', { name: ' 商品管理' }).click();
      await page.getByRole('link', { name: '維護大批商品' }).click();
    });
    
    await test.step('設定 OB 商品員購屬性為「是」', async () => {
      await page.getByRole('radio', { name: '員購' }).check();
      await page.locator('div:nth-child(20) > .status').selectOption('1');
      await page.locator('#txtareaCodeList').fill(saleId);
    });

    await test.step('點擊修改並處理確認對話框', async () => {
      page.once('dialog', dialog => dialog.accept());
      await page.getByRole('button', { name: ' 開始修改' }).click();
    });

    await test.step('驗證資料庫：OB 商品 ChannelCode 應維持 84', async () => {
      await expect.poll(async () => {
        const dbResponse = await page.request.get(`http://localhost:3000/readEtmallStaffPurchase/${saleId}`);
        return await dbResponse.json();
      }, {
        message: '檢查 OB 商品的員購記錄',
        timeout: 10000,
      }).toEqual(expect.arrayContaining([expect.objectContaining({ ChannelCode: 84 })]));
    });

    await test.step('驗證旗標 API：OB 商品 hasStaffPurchase 應為 true', async () => {
      await expect.poll(async () => {
        const response = await page.request.get(`http://localhost:3000/readSalesMixProductFlag/${saleId}`);
        const data = await response.json();
        return data.hasStaffPurchase;
      }, {
        message: '檢查 OB 商品 hasStaffPurchase 旗標',
        timeout: 10000,
      }).toBe(true);
    });
  });

});
