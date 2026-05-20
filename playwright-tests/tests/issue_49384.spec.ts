import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

/**
 * Issue 49384: 混搭商品變價檢查權限發生錯誤
 * 核心問題：當設定多筆預約變價，若有筆數未點開「編輯」子商品，會因缺少 KitProductList 導致後端檢查噴錯。
 */

test.describe.configure({ mode: 'serial' });

test.describe('Issue 49384: 混搭商品變價檢查權限發生錯誤', () => {
  const productId = '1214847';

  // 每個案例前重新進入商品變價頁面 (除了獨立的取消變價案例)
  async function enterProductPricePage(page) {
    await setupTest(page, productId);
    const setBookingBtn = page.getByText('設定預約變價').nth(1);
    await setBookingBtn.waitFor({ state: 'visible', timeout: 10000 });
    await setBookingBtn.click();
    
    // 等待變價區塊載入，確保後續操作能找到元素
    await page.locator('#changePriceDateStart_0_0').waitFor({ state: 'visible', timeout: 15000 });
  }

  // 測試案例 1：部分預約未編輯 (第一筆編輯，第二筆不編輯) -> 重現 Bug 情境
  test('issue_49384-1: 混搭商品 - 第一筆編輯，第二筆不編輯 (應成功)', async ({ page }) => {
    // 執行前先確保環境乾淨
    //await cancelAutoAproveChangePrice(page, productId);
    
    await enterProductPricePage(page);
    const { day1, day2 } = getTestDates();

    // 第一筆：設定日期 + 編輯子商品
    await setDate(page, '#changePriceDateStart_0_0', day1);
    await editSubItemPrice(page, 0, '1100');

    // 第二筆：設定日期 + 不編輯 (不動)
    await page.getByRole('button', { name: '+' }).click();
    await setDate(page, '#changePriceDateStart_0_1', day2);

    await submitChangePrice(page);    
  });

   // 測試案例 3：獨立的取消變價案例
   test('issue_49384-2: 取消變價 (應成功)', async ({ page }) => {
    await cancelAutoAproveChangePrice(page, productId);
    await page.waitForTimeout(2000);
    await cancelChangePrice(page, productId);
  });

  // 測試案例 2：第一筆與第二筆皆正常編輯 -> 標準成功情境
  test('issue_49384-3: 混搭商品 - 第一筆與第二筆皆編輯 (應成功)', async ({ page }) => {    
    await enterProductPricePage(page);
    const { day1, day2 } = getTestDates();

    // 第一筆：編輯
    await setDate(page, '#changePriceDateStart_0_0', day1);
    await editSubItemPrice(page, 0, '1001');

    // 第二筆：編輯
    await page.getByRole('button', { name: '+' }).click();
    await setDate(page, '#changePriceDateStart_0_1', day2);
    await editSubItemPrice(page, 1, '1002');

    await submitChangePrice(page);
  });

  // 測試案例 3：獨立的取消變價案例
  test('issue_49384-4: 取消變價 (應成功)', async ({ page }) => {
    await cancelChangePrice(page, productId);
  });

  // 測試案例 4：第一筆與第二筆皆「不編輯」子商品 (僅設定日期) -> 驗證原值回帶邏輯
  test('issue_49384-5: 混搭商品 - 第一筆與第二筆皆不編輯子商品 (應成功)', async ({ page }) => {
    await enterProductPricePage(page);
    const { day1, day2 } = getTestDates();

    // 第一筆
    await setDate(page, '#changePriceDateStart_0_0', day1);

    // 第二筆
    await page.getByRole('button', { name: '+' }).click();
    await setDate(page, '#changePriceDateStart_0_1', day2);

    // 建議：按一下 Esc 鍵確保日期選擇器關閉，避免擋住下方的「確定變價」按鈕
    await page.keyboard.press('Escape');

    // 處理「確認送審」對話框
    await Promise.all([
      page.waitForEvent('dialog').then(d => d.accept()), // 捕捉並自動按確定
      page.getByRole('button', { name: '確定變價' }).click({ force: true }) // 使用 force 確保不被殘留遮罩影響
    ]);

    // 如果預期還有第二個「價格無異動」的彈窗
    const finalDialog = await page.waitForEvent('dialog');
    expect(finalDialog.message()).toContain('價格無異動');
    await finalDialog.accept();
  });
});

// --- Helper Functions ---

async function setupTest(page, productId) {
  console.log(`Setting up test for product: ${productId}`);
  // 登入帳號 role.md 密碼 123456
  await b2eLogin(page, 'role.md', '123456');

  // 快速檢查是否有「下次再說」彈窗，最多等 1 秒
  const skipButton = page.getByRole('button', { name: '下次再說' });
  try {
    await skipButton.waitFor({ state: 'visible', timeout: 1000 });
    await skipButton.click();
  } catch (e) {
    // 如果 1 秒內沒出現則繼續執行
  }

  await page.getByRole('link', { name: ' 商品管理' }).click();
  await page.getByRole('link', { name: '審核變價' }).click();
  
  // 監聽搜尋結果對話框
  const dialogHandler = async dialog => {
    console.log(`Setup Dialog: ${dialog.message()}`);
    await dialog.accept().catch(() => {});
  };
  page.on('dialog', dialogHandler);

  const searchInput = page.getByRole('textbox').first();
  await searchInput.fill(productId);
  await page.getByRole('button', { name: '商品變價' }).click();
  
  // 等待並選取第一列商品 (搜尋結果的第一筆)
  const firstCheckbox = page.getByRole('row').nth(1).getByRole('checkbox');
  try {
    await firstCheckbox.waitFor({ state: 'visible', timeout: 5000 });
    await firstCheckbox.check();
  } catch (e) {
    console.warn(`Could not find checkbox for ${productId}. Search might have failed.`);
    await page.screenshot({ path: `test-results/setup_error_${productId}.png` });
    throw e;
  } finally {
    page.off('dialog', dialogHandler);
  }
}

function getTestDates() {
  const today = new Date();
  const d1 = new Date(today); d1.setDate(today.getDate() + 1);
  const d2 = new Date(today); d2.setDate(today.getDate() + 2);
  return { day1: d1.getDate().toString(), day2: d2.getDate().toString() };
}

async function setDate(page, selector, day) {
  const dateInput = page.locator(selector);
  
  // 1. 等待元素出現在 DOM 中並滾動到視口，防止被遮擋
  await dateInput.waitFor({ state: 'attached', timeout: 10000 });
  await dateInput.scrollIntoViewIfNeeded();

  // 2. 如果元素被禁用，嘗試等待它變成可用狀態（最多等 5 秒）
  try {
    await dateInput.waitFor({ state: 'visible', timeout: 5000 });
  } catch (e) {
    console.warn(`Warning: Element ${selector} is not visible yet, attempting to continue...`);
  }

  // 3. 執行點擊動作
  await dateInput.click({ force: true, delay: 500 });

  // 4. 等待日期選擇面板中的日期連結出現並點擊
  const dayLink = page.getByRole('link', { name: day, exact: true });
  await dayLink.waitFor({ state: 'visible', timeout: 5000 });
  await dayLink.click();
}

async function editSubItemPrice(page, index, price) {
  // 點擊「編輯」開啟子商品變價視窗
  await page.getByTitle('編輯').nth(index).click();
  
  // 定位編輯視窗
  const modal = page.locator('#kitEditWindow');
  await modal.waitFor({ state: 'visible', timeout: 10000 });

  // 填寫視窗中所有的變價欄位 (spinbutton)
  const inputs = modal.getByRole('spinbutton');
  const count = await inputs.count();
  for (let i = 0; i < count; i++) {
    await inputs.nth(i).fill(price);
  }
  
  await page.getByRole('button', { name: '確定', exact: true }).click();
}

async function submitChangePrice(page) {
  page.once('dialog', async dialog => {
    console.log(`Dialog: ${dialog.message()}`);
    await dialog.accept().catch(() => {});
  });
  await page.getByRole('button', { name: '確定變價' }).click();
  await page.waitForTimeout(2000);
}

async function cancelChangePrice(page, productId) {

  await page.waitForTimeout(2000);
  // 不要點登出按鈕了，直接清空狀態
  await page.context().clearCookies();
  // 直接前往登入頁
  await page.goto('http://b2e.lab.etzone.net/web/B2ELogin');
  
  // 確保在該網域下清空 LocalStorage，若失敗則忽略 (例如在 about:blank 時)
  await page.evaluate(() => {
    try {
      window.localStorage.clear();
      window.sessionStorage.clear();
    } catch (e) {
      console.log('Storage access denied, skipping clear.');
    }
  });

  // 開始輸入 admin 帳號
  await page.locator('#account').fill('admin');
  await page.locator('#password').fill('sensengo168');
  await page.locator('#domain').first().selectOption('');
  await page.getByRole('button', { name: '  登入' }).click();
  await page.waitForLoadState('networkidle');

  await page.getByRole('link', { name: ' 商品管理' }).click();
  await page.getByRole('link', { name: '審核變價' }).click();
  await page.waitForLoadState('networkidle');

  // 輸入商品編號並搜尋
  const searchInput = page.getByRole('textbox').first();
  await searchInput.fill(productId);
  
  const auditButton = page.getByRole('button', { name: '營業主管審核' });
  await auditButton.waitFor({ state: 'visible', timeout: 10000 });
  // 額外等待按鈕變為可用 (如果有 ng-disabled)
  await page.waitForFunction((btn) => !btn.disabled, await auditButton.elementHandle(), { timeout: 10000 }).catch(() => {});
  await auditButton.click();

  await page.getByRole('row').nth(1).getByRole('checkbox').check();
  await page.getByRole('button', { name: '駁回' }).click();

  await page.getByRole('row', { name: '駁回原因', exact: true }).getByRole('textbox').fill('playwright auto reject');
  page.once('dialog', dialog => {
    console.log(`Dialog message: ${dialog.message()}`);
    dialog.accept().catch(() => {});
  });
  await page.getByRole('button', { name: '確定' }).click();
  await page.waitForTimeout(1000);
}

//不需要審核的變價取消流程
async function cancelAutoAproveChangePrice(page,productId) {
  // 不要點登出按鈕了，直接清空狀態
  await page.context().clearCookies();
  // 直接前往登入頁
  await page.goto('http://b2e.lab.etzone.net/web/B2ELogin');
  
  // 確保在該網域下清空 LocalStorage，若失敗則忽略 (例如在 about:blank 時)
  await page.evaluate(() => {
    try {
      window.localStorage.clear();
      window.sessionStorage.clear();
    } catch (e) {
      console.log('Storage access denied, skipping clear.');
    }
  });

  // 開始輸入 admin 帳號
  await page.locator('#account').fill('admin');
  await page.locator('#password').fill('sensengo168');
  await page.locator('#domain').first().selectOption('');
  await page.getByRole('button', { name: '  登入' }).click();
  await page.waitForLoadState('networkidle');

  await page.getByRole('link', { name: ' 商品管理' }).click();
  await page.getByRole('link', { name: '審核變價' }).click();
  await page.waitForLoadState('networkidle');
  await page.getByRole('button', { name: '取消變價' }).click();  
  await page.getByRole('row', { name: '變價作業會同步富購，會持續一段時間 請勿重複操作或另開視窗重送，以免造成合約異常' }).getByRole('textbox').fill(productId);
  page.once('dialog', dialog => {
    console.log(`Dialog message: ${dialog.message()}`);
    dialog.dismiss().catch(() => {});
  });
  await page.getByRole('button', { name: '☁ 取消變價' }).click();
}