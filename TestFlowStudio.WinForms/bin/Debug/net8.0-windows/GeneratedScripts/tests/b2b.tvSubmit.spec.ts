import { test, expect } from '@playwright/test';
import { b2bLogin } from './b2bLogin';

test('B2B TV Product Submission', async ({ page }) => {
  // Login using the shared login function
  await b2bLogin(page);
  await page.waitForURL('http://b2b.lab.etzone.net/Web/DefaultPage');

  // After login, explicitly navigate to the TV product submission page.
  // NOTE: The URL parameter 'e=3' is an assumption for TV products.
  // In the original script, there was no explicit navigation after login.
  // Adding this makes the test more robust. Please verify 'e=3' is correct for TV products.
  await page.goto('http://b2b.lab.etzone.net/Web/ProductManager?e=2');

  // --- Start Filling Product Details ---

  const productName = 'playwrightTV';
  await page.getByRole('textbox', { name: '商品名稱' }).fill(productName);
  await page.getByRole('textbox', { name: '銷售名稱(供前台使用)' }).fill(productName);
  await page.getByRole('textbox', { name: '銷售副標(供前台使用)' }).fill(productName);

  // Category selection
  await page.getByRole('combobox').first().selectOption('10000');
  await page.getByRole('combobox').nth(1).selectOption('10100');
  // This JSON value is kept from the original script. It might be brittle.
  await page.getByRole('combobox').nth(2).selectOption('{"id":10102,"name":"男休閒服","parentCategoryId":10100,"isDisabled":false,"FormEnabled":false,"hasInstallmentConstraint":false,"CanUseDiscount":true,"isfugoCouponUsable":true,"canBuyByProfit":false,"IsAmigo":false}');

  // More category settings
  await page.locator('#MDSelect').selectOption('103004');
  await page.locator('#fRoot_e').selectOption('31771');
  await page.locator('#fFirst_e').selectOption('34697');
  await page.locator('#fSecond_e').selectOption('34698');
  await page.locator('#fThird_e').selectOption('34703');
  await page.getByRole('button', { name: '主分類設定' }).click();

  // Price and cost
  await page.getByText('一般').nth(1).click();
  await page.locator('#marketPrice').fill('3000');
  await page.locator('#salePrice').fill('2000');
  await page.locator('#costPriceNoTax').fill('1000');

  // Stock and quantity
  await page.getByPlaceholder('必填').nth(5).fill('1885');
  // Date selection from iframe
  await page.getByRole('textbox', { name: '-- 請選擇 --' }).click();

  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  const tomorrowDay = tomorrow.getDate().toString();

  const dateCell = page.locator('iframe').nth(4).contentFrame().getByRole('cell', { name: tomorrowDay }).first();
  await dateCell.scrollIntoViewIfNeeded();
  await dateCell.click();

  // Package & Product dimensions - Consolidated from multiple messy inputs
  await page.getByRole('textbox', { name: '包裹長  (mm)' }).fill('11');
  await page.getByRole('textbox', { name: '包裹寬  (mm)' }).fill('1111');
  await page.getByRole('textbox', { name: '包裹高  (mm)' }).fill('11');
  await page.getByRole('textbox', { name: '包裹總重  (g)' }).fill('11');
  await page.getByRole('textbox', { name: '商品長  (mm)' }).fill('22');
  await page.getByRole('textbox', { name: '商品寬  (mm)' }).fill('2222');
  await page.getByRole('textbox', { name: '商品高  (mm)' }).fill('22');
  await page.getByRole('textbox', { name: '商品總重  (g)' }).fill('22');

  // Brand and attributes
  await page.locator('.form-control.form-inline').first().selectOption('623');
  await page.getByRole('button', { name: '尚未選擇' }).click();
  await page.getByRole('button', { name: 'CHANEL' }).click();
  await page.locator('.col-sm-2 > .form-control').first().selectOption('string:10001380');
  await page.locator('div:nth-child(2) > .col-sm-2.form-inline > .form-control').selectOption('string:10001410');

  await page.locator('#mainImgs').click();
  await page.locator('#picUpload').setInputFiles('tests/images/1.jpg');

  await page.locator('#adsImg').click();
  await page.locator('#adsPicUpload').setInputFiles('tests/images/1.jpg');

  // Rich text editor and descriptions
  await page.locator('iframe[title="RTF 編輯器, editor_kama"]').contentFrame().locator('body').fill('test description for playwright');
  await page.getByRole('textbox', { name: '必填欄位 (請勿斷行超過3行，字數限制100字內。)' }).fill('test summary');
  await page.locator('#productCharacteristics').fill('test characteristics');
  await page.locator('#productSpec').fill('test specifications');
  await page.locator('#attentionItem').fill('test attention items');
  await page.locator('#experienceweb').fill('體驗說明');
  await page.locator('#presentDescription').fill('gift贈品說明');
  await page.locator('#accessory').fill('playwright 配件說明');
  await page.locator('#useMethod').fill('使用方法說明');

  // --- Submission Process ---
  // The original script had a complex sequence of submit clicks, revealing new fields.
  // This sequence is preserved but cleaned up.

  // 1. First submit attempt (likely triggers validation)
  await page.getByRole('button', { name: ' 送審' }).click();

  // 2. Handle options revealed after first submit
  await page.getByText('廠送', { exact: true }).click();
  await page.getByRole('spinbutton', { name: ' 可追加量所需工作日 * 此欄位不可空白 * 只能填整數 日' }).fill('10');

  // 3. Second submit attempt
  await page.getByRole('button', { name: ' 送審' }).click();

  // 4. Fill newly required fields revealed after second submit
  await page.getByRole('textbox', { name: '包裹長  * 此欄位不可空白 * 只能填整數 (mm)' }).fill('10');
  await page.getByRole('textbox', { name: '包裹寬  * 此欄位不可空白 * 只能填整數 (mm)' }).fill('10');
  await page.getByRole('textbox', { name: '包裹高  * 此欄位不可空白 * 只能填整數 (mm)' }).fill('10');
  await page.getByRole('textbox', { name: '包裹總重  * 此欄位不可空白 * 只能填整數 (g)' }).fill('10');

  // 5. Final submit clicks
  await page.getByRole('button', { name: ' 送審' }).click();
  await page.getByRole('button', { name: '送審', exact: true }).click();
});