import { test, expect } from '@playwright/test';
import { b2bLogin } from './b2bLogin';

test('b2bLoginTest', async ({ page }) => {
  // Call the modularized login function
  await b2bLogin(page);

  // You can add additional assertions here to verify post-login state if needed
  console.log('Login successful, now on the main page.');
});
// for b2b提報網路商品
test('b2bSubmitWebProduct', async ({ page }) => {
  //b2b提報網路商品
  await b2bLogin(page);
  await page.waitForURL('http://b2b.lab.etzone.net/Web/DefaultPage');
  await page.goto('http://b2b.lab.etzone.net/Web/ProductManager?e=1');  
  await page.locator('#saleName').fill('playwrightTest');
  await page.getByRole('textbox', { name: '銷售副標' }).fill('playwrightTest-1');
  await page.getByRole('textbox', { name: '--請選擇--' }).click();
  
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  const tomorrowDay = tomorrow.getDate().toString();

  const cell = page.locator('iframe').nth(4).contentFrame().getByRole('cell', { name: tomorrowDay }).first();
  await cell.scrollIntoViewIfNeeded(); // 強制捲動到該位置
  await cell.click();

  await page.locator('#fRoot_e').selectOption('16462');
  await page.locator('#fFirst_e').selectOption('16491');
  await page.locator('#fSecond_e').selectOption('39773');
  await page.locator('#fThird_e').selectOption('26570');
  await page.getByRole('button', { name: '主分類設定' }).click();    
  await page.locator('#marketPrice').fill('3000');    
  await page.locator('#salePrice').fill('2000');    
  await page.locator('#costPrice').fill('1000');
  await page.getByRole('textbox', { name: '包裹長  (mm)' }).fill('10');
  await page.getByRole('textbox', { name: '包裹寬  (mm)' }).fill('10');    
  await page.getByRole('textbox', { name: '包裹高  (mm)' }).fill('10');    
  await page.getByRole('textbox', { name: '包裹總重  (g)' }).fill('10');
  await page.locator('.form-control.form-inline').first().selectOption('271');
  await page.getByRole('row', { name: ' 品牌 (最多選1項)' }).getByLabel('Select box').click();
  await page.getByText('Livi優活').click();
  await page.getByRole('row', { name: ' 類型 (最多選1項)' }).getByLabel('Select box').click();
  await page.locator('#ui-select-choices-row-1-3 > .select2-result-label').click();
  await page.getByRole('row', { name: ' 主要材質 (最多選1項)' }).getByLabel('Select box').click();
  await page.getByText('蠶絲-LucyTest2222').click();
  await page.locator('label').filter({ hasText: '包以下' }).click();    
  await page.getByRole('row', { name: ' 實際包數' }).locator('textarea').fill('12');
  await page.getByText('台灣').click();
  
  await page.locator('#mainImgs').click();
  await page.locator('#picUpload').setInputFiles('tests/images/1.jpg');

  await page.locator('#adsImg').click();
  await page.locator('#adsPicUpload').setInputFiles('tests/images/1.jpg');
    
  await page.locator('iframe[title="RTF 編輯器, editor_kama"]').contentFrame().locator('html').click();
  await page.locator('iframe[title="RTF 編輯器, editor_kama"]').contentFrame().locator('body').fill('playwrightTest');    
  await page.locator('#attentionItem').fill('playwrightTest');    
  await page.locator('#attentionItem').fill('playwrightTest');
  await page.locator('#prdDesPlanner').fill('playwrightTest');  
  await page.locator('#useMethod').fill('playwrightTest');
  await page.getByRole('button', { name: ' 送審' }).click({ timeout: 5000 });
  await page.getByRole('button', { name: '送審'}).first().click({ timeout: 5000 });   
  await page.getByRole('button', { name: '送審'}).first().click({ timeout: 5000 }); 
  
});
