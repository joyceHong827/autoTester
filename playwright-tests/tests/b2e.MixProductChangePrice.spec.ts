import { test } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

// b2e 混搭商品變價測試腳本
test('b2e 混搭商品變價測試', async ({ page }) => {
  // 計算生效日期：今天+1天 (明天) 與 今天+2天 (後天)
  const today = new Date();
  const datePlus1 = new Date(today);
  datePlus1.setDate(today.getDate() + 1);
  const datePlus2 = new Date(today);
  datePlus2.setDate(today.getDate() + 2);

  const day1 = datePlus1.getDate().toString();
  const day2 = datePlus2.getDate().toString();

  const productId = '1214643';

  // 使用 b2eLoginFunction 進行登入
  await b2eLogin(page, 'role.md', '123456');

  // 處理登入後的彈窗 (如果有的話)
  const skipButton = page.getByRole('button', { name: '下次再說' });
  if (await skipButton.isVisible({ timeout: 5000 }).catch(() => false)) {
    await skipButton.click();
  }

  // 導航至審核變價頁面
  await page.getByRole('link', { name: ' 商品管理' }).click();
  await page.getByRole('link', { name: '審核變價' }).click();

  // 自動搜尋商品編號
  
  await page.getByRole('combobox').nth(2).selectOption('12');
  await page.getByRole('button', { name: '商品變價' }).click();

  // 輸入商品編號並搜尋
  const searchInput = page.getByRole('textbox').first();
  await searchInput.fill(productId);
  await page.getByRole('button', { name: '商品變價' }).click();

  // 選取商品並設定預約變價
  await page.getByRole('row').nth(1).getByRole('checkbox').check();
  await page.getByText('設定預約變價').nth(1).click();

  // 1. 等待表格標籤本身出現
await page.locator('table').first().waitFor();
await page.locator('span[title="編輯"]').first().click();

// 改等這個 ID
const modal = page.locator('#kitEditWindow');
await modal.waitFor({ state: 'visible', timeout: 10000 });

// 進入視窗後再填寫裡面的 input
await modal.getByRole('spinbutton').first().fill('1500');
await page.getByRole('button', { name: '確定', exact: true }).click();

  // 設定第一筆生效日期 (今天+1天)
  await page.locator('#changePriceDateStart_0_0').click();
  await page.getByRole('link', { name: day1, exact: true }).click();

  // 新增第二筆預約日期
  await page.getByRole('button', { name: '+' }).click();

  // 設定第二筆生效日期 (今天+2天)
  await page.locator('#changePriceDateStart_0_1').click();
  await page.getByRole('link', { name: day2, exact: true }).click();
  
  await page.getByRole('button', { name: 'Done' }).click();

  // 監聽對話框並執行確定變價
  page.once('dialog', dialog => {
    console.log(`Dialog message: ${dialog.message()}`);
    dialog.accept().catch(() => {});
  });
  await page.getByRole('button', { name: '確定變價' }).click();
  
  //確認變價成功後，所有測試結束後的清理工作：駁回變價申請
  await cancelChangePrice(page, productId);
});

// 需要審核的變價駁回流程
 async function cancelChangePrice(page,productId) {
   // 不要點登出按鈕了，直接清空狀態
  await page.context().clearCookies();
  await page.evaluate(() => window.localStorage.clear()); // 清空 LocalStorage
    // 直接前往登入頁
  await page.goto('http://b2e.lab.etzone.net/web/B2ELogin', { waitUntil: 'networkidle' });

  // 開始輸入 admin 帳號
  await page.locator('#account').fill('admin');
  await page.locator('#password').fill('sensengo168');
  await page.locator('#domain').first().selectOption('');
   await page.getByRole('button', { name: '  登入' }).click();
   await page.getByRole('link', { name: ' 商品管理' }).click();
   await page.getByRole('link', { name: '審核變價' }).click(); 

     // 輸入商品編號並搜尋
   const searchInput = page.getByRole('textbox').first();
   await searchInput.fill(productId);   
   await page.getByRole('button', { name: '營業主管審核' }).click();
   await page.getByRole('row').nth(1).getByRole('checkbox').check();
   await page.getByRole('button', { name: '駁回' }).click();
  
   await page.getByRole('row', { name: '駁回原因', exact: true }).getByRole('textbox').fill('playwright auto reject');
   page.once('dialog', dialog => {
     console.log(`Dialog message: ${dialog.message()}`);
     dialog.dismiss().catch(() => {});
   });
   await page.getByRole('button', { name: '確定' }).click(); 
 }

 //不需要審核的變價取消流程
 async function cancelAutoAproveChangePrice(page,productId) {
  await page.getByRole('button', { name: '取消變價' }).click();  
  await page.getByRole('row', { name: '變價作業會同步富購，會持續一段時間 請勿重複操作或另開視窗重送，以免造成合約異常' }).getByRole('textbox').fill(productId);
  page.once('dialog', dialog => {
    console.log(`Dialog message: ${dialog.message()}`);
    dialog.dismiss().catch(() => {});
  });
  await page.getByRole('button', { name: '☁ 取消變價' }).click();
}