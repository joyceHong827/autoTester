import { test, expect } from '@playwright/test';

test.describe('電視商品提報 - ESV價欄位驗證', () => {

  test('新增電視商品提報並驗證ESV價欄位顯示與儲存', async ({ page }) => {

    await test.step('導航至B2B登入頁面', async () => {
      await page.goto('http://b2b.lab.etzone.net/Web/B2B_B2ELogin');
      await expect(page).toHaveURL(/B2B_B2ELogin/);
    });

    await test.step('輸入帳號密碼並執行登入', async () => {
      await page.getByRole('textbox', { name: '統一編號' }).fill('12718372');
      await page.getByRole('textbox', { name: '密碼' }).fill('123456');
      await page.getByRole('button', { name: ' 登入' }).click();
      await expect(page).toHaveURL(/DefaultPage/);
    });

    await test.step('關閉登入後彈出視窗', async () => {
      await page.getByRole('button', { name: '下次再說' }).click();
      // 驗證彈窗消失
      await expect(page.getByRole('button', { name: '下次再說' })).not.toBeVisible();
    });

    await test.step('進入商品管理 > 商品上架 > 電視商品提報頁面', async () => {
      // 假設此連結直接導向新增提報頁面，或在頁面中點擊新增
      await page.getByRole('link', { name: '電視商品', exact: true }).click();
      await expect(page).toHaveURL(/Web\/ProductManager\?e=2&pid=0/); // 驗證進入新增電視商品頁面
    });

    await test.step('驗證ESV價欄位存在並可輸入', async () => {
      // 預期結果: 提報表單中可見「ESV價」欄位
      const esvPriceLocator = page.locator('span').filter({ hasText: 'ESV價' }).getByRole('spinbutton');
      await expect(esvPriceLocator).toBeVisible();
      // 預期結果: 欄位可輸入數字 (透過後續填寫驗證)
    });

    await test.step('填寫商品基本資訊', async () => {
      await page.getByRole('textbox', { name: '商品名稱' }).fill('測試ESV欄位商品名稱');
      await expect(page.getByRole('textbox', { name: '商品名稱' })).toHaveValue('測試ESV欄位商品名稱');

      await page.getByRole('textbox', { name: '銷售名稱(供前台使用)' }).fill('測試ESV欄位銷售名稱');
      await expect(page.getByRole('textbox', { name: '銷售名稱(供前台使用)' })).toHaveValue('測試ESV欄位銷售名稱');

      await page.getByRole('textbox', { name: '銷售副標(供前台使用)' }).fill('測試ESV欄位銷售副標');
      await expect(page.getByRole('textbox', { name: '銷售副標(供前台使用)' })).toHaveValue('測試ESV欄位銷售副標');

      await page.getByRole('combobox').first().selectOption('10000'); // 商品主分類
      await page.getByRole('combobox').nth(1).selectOption('10100'); // 商品次分類
      await page.getByRole('combobox').nth(2).selectOption('{"id":10102,"name":"男休閒服","parentCategoryId":10100,"isDisabled":false,"FormEnabled":false,"hasInstallmentConstraint":false,"CanUseDiscount":true,"isfugoCouponUsable":true,"canBuyByProfit":false,"IsAmigo":false}'); // 商品明細分類

      await page.locator('#MDSelect').selectOption('103004'); // 商品MD
      await page.locator('#fRoot_e').selectOption('31771'); // 提報分類
      await page.locator('#fFirst_e').selectOption('34697'); // 提報次分類
      await page.locator('#fSecond_e').selectOption('34698'); // 提報細分類
      await page.locator('#fThird_e').selectOption('34703'); // 提報子細分類
      await page.getByRole('button', { name: '主分類設定' }).click(); // 點擊主分類設定 (假設是觸發UI更新)
      await expect(page.getByText('一般').nth(1)).toBeVisible(); // 驗證分類設定成功
    });

    await test.step('填寫商品價格與庫存資訊', async () => {
      await page.locator('#marketPrice').fill('30000');
      await expect(page.locator('#marketPrice')).toHaveValue('30000');
      await page.locator('#salePrice').fill('28800');
      await expect(page.locator('#salePrice')).toHaveValue('28800');
      await page.locator('#costPriceNoTax').fill('18800');
      await expect(page.locator('#costPriceNoTax')).toHaveValue('18800');

      // 預期結果: 欄位可輸入數字，並正確填入ESV價
      await page.locator('span').filter({ hasText: 'ESV價' }).getByRole('spinbutton').fill('29800');
      await expect(page.locator('span').filter({ hasText: 'ESV價' }).getByRole('spinbutton')).toHaveValue('29800');

      await page.getByPlaceholder('必填').nth(5).fill('32'); // 建議售價
      await expect(page.getByPlaceholder('必填').nth(5)).toHaveValue('32');

      await page.getByRole('textbox', { name: '-- 請選擇 --' }).click();
      await page.locator('iframe').nth(4).contentFrame().getByRole('cell', { name: '14' }).click(); // 預計到貨日

      await page.locator('.form-control.validate\\[custom\\[onlyNumberSp\\]\\,min\\[0\\]\\,max\\[999999\\]\\]').fill('10'); // 可供貨量
      await expect(page.locator('.form-control.validate\\[custom\\[onlyNumberSp\\]\\,min\\[0\\]\\,max\\[999999\\]\\]')).toHaveValue('10');

      await page.getByRole('spinbutton', { name: ' 可追加量所需工作日 日' }).fill('10'); // 可追加量所需工作日
      await expect(page.getByRole('spinbutton', { name: ' 可追加量所需工作日 日' })).toHaveValue('10');

      await page.getByText('廠送', { exact: true }).click(); // 配送方式 - 廠送
      await page.locator('div:nth-child(2) > span > .form-control').selectOption('1'); // 運送天數
      await expect(page.locator('div:nth-child(2) > span > .form-control')).toHaveValue('1');
    });

    await test.step('填寫包裝與物流資訊', async () => {
      await page.getByLabel('包裝方式: 無 原箱包裝').selectOption('1005'); // 包裝方式 - 原箱包裝
      await expect(page.getByLabel('包裝方式: 無 原箱包裝')).toHaveValue('1005');

      await page.getByRole('textbox', { name: '包裹長  * 此欄位不可空白 * 只能填整數 (mm)' }).fill('10');
      await expect(page.getByRole('textbox', { name: '包裹長  * 此欄位不可空白 * 只能填整數 (mm)' })).toHaveValue('10');
      await page.getByRole('textbox', { name: '包裹寬  (mm)' }).fill('10');
      await expect(page.getByRole('textbox', { name: '包裹寬  (mm)' })).toHaveValue('10');
      await page.getByRole('textbox', { name: '包裹高  (mm)' }).fill('10');
      await expect(page.getByRole('textbox', { name: '包裹高  (mm)' })).toHaveValue('10');
      await page.getByRole('textbox', { name: '包裹總重  (g)' }).fill('10');
      await expect(page.getByRole('textbox', { name: '包裹總重  (g)' })).toHaveValue('10');
    });

    await test.step('填寫商品屬性與品牌', async () => {
      await page.locator('.form-control.form-inline').first().selectOption('623'); // 商品屬性
      await expect(page.locator('.form-control.form-inline').first()).toHaveValue('623');

      await page.getByRole('row', { name: '電源' }).locator('textarea').fill('testt');
      await expect(page.getByRole('row', { name: '電源' }).locator('textarea')).toHaveValue('testt');
      await page.getByRole('row', { name: '電壓' }).locator('textarea').fill('test');
      await expect(page.getByRole('row', { name: '電壓' }).locator('textarea')).toHaveValue('test');

      await page.getByRole('button', { name: '尚未選擇' }).click(); // 點擊品牌選擇
      await page.getByRole('button', { name: 'CHANEL' }).click(); // 選擇品牌
      await expect(page.getByRole('button', { name: 'CHANEL' })).toBeVisible(); // 驗證品牌被選擇 (或更精確地驗證顯示在欄位中)
    });

    await test.step('上傳商品圖片', async () => {
      await page.locator('#mainImgs').getByTitle('上傳圖片').setInputFiles('codex_大全 1.png'); // 主圖上傳
      await page.locator('#qcImgsBlock').getByTitle('上傳圖片').setInputFiles('codex_大全 1.png'); // QC圖上傳
      // 驗證圖片上傳成功 (通常是看是否有縮圖顯示或上傳狀態提示)
      await expect(page.locator('#mainImgs .uploaded-img')).toBeVisible();
      await expect(page.locator('#qcImgsBlock .uploaded-img')).toBeVisible();
    });

    await test.step('填寫商品詳細描述', async () => {
      await page.locator('iframe[title="RTF 編輯器, editor_kama"]').contentFrame().locator('body').fill('testaaaaa'); // 商品說明
      await expect(page.locator('iframe[title="RTF 編輯器, editor_kama"]').contentFrame().locator('body')).toHaveText('testaaaaa');

      await page.getByRole('textbox', { name: '必填欄位 (請勿斷行超過3行，字數限制100字內。)' }).fill('testaaaaaa'); // 網頁文案
      await expect(page.getByRole('textbox', { name: '必填欄位 (請勿斷行超過3行，字數限制100字內。)' })).toHaveValue('testaaaaaa');

      await page.locator('#productCharacteristics').fill('testaaaaaa'); // 商品特色
      await expect(page.locator('#productCharacteristics')).toHaveValue('testaaaaaa');
      await page.locator('#productSpec').fill('testaaaaaa'); // 商品規格
      await expect(page.locator('#productSpec')).toHaveValue('testaaaaaa');
      await page.locator('#attentionItem').fill('testaaaaaa'); // 注意事項
      await expect(page.locator('#attentionItem')).toHaveValue('testaaaaaa');
      await page.locator('#experienceweb').fill('testaaaaaa'); // 體驗網頁
      await expect(page.locator('#experienceweb')).toHaveValue('testaaaaaa');
      await page.locator('#presentDescription').fill('testaaaaaa'); // 贈品說明
      await expect(page.locator('#presentDescription')).toHaveValue('testaaaaaa');

      await page.locator('#useMethod').fill('testaaaaaa'); // 使用方式
      await expect(page.locator('#useMethod')).toHaveValue('testaaaaaa');
    });

    await test.step('點擊送審按鈕完成提報', async () => {
      // 由於送審按鈕會觸發多個驗證，按照錄製腳本逐一處理
      await page.getByRole('button', { name: ' 送審' }).click();
      await page.locator('#form-validation-field-2').fill('93'); // 處理某個驗證彈窗
      await page.getByRole('button', { name: ' 送審' }).click();
      await page.locator('#form-validation-field-3').fill('3846'); // 處理某個驗證彈窗
      await page.getByRole('button', { name: ' 送審' }).click();
      await page.locator('#form-validation-field-3').fill('39306'); // 處理某個驗證彈窗
      await page.getByRole('button', { name: ' 送審' }).click();

      // 最後點擊送審按鈕
      await page.getByRole('button', { name: '送審', exact: true }).click();

      // 預期結果: 提報成功
      await expect(page.getByText('提報商品送審成功')).toBeVisible();
    });
  });
});