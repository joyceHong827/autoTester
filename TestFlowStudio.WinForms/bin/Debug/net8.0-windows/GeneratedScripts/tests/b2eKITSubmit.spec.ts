import { test, expect } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

test('B2E Kit Submission Test', async ({ page }) => {
  const timestamp = new Date().getTime();
  const productName = `playwrightKit提報_${timestamp}`;

  // Use the common login function
  await b2eLogin(page);


  // Navigate to Product Management -> Create Kit Product
  // Use more robust navigation if needed, or rely on clicking the menu
  await page.getByRole('link', { name: ' 商品管理' }).click();
  await page.getByRole('link', { name: '建立套組商品' }).click();


  
  // Select Supplier (assuming first combobox is supplier selection)
  await page.getByRole('combobox').first().selectOption('5'); //選擇開發來源

  // Fill Product Names
  await page.locator('#saleName').fill(productName);
  await page.locator('#SalesSubtitle').fill(productName);

  // Select Dates using frameLocator for elements inside iframes
  // Shelf Date textbox
  await page.getByRole('textbox').nth(2).click();
  const datePickerFrame = page.frameLocator('iframe').nth(1);
  // Clicking day '2' in the current month
  await datePickerFrame.getByRole('cell', { name: '2', exact: true }).first().click();

  // Actual Shelf Date
  await page.locator('[id="SMA.actualShelf"]').first().click();
  // Clicking day '3' in the current month
  await datePickerFrame.getByRole('cell', { name: '3', exact: true }).first().click();

  // Select Categories (assuming specific indices for Large/Medium/Small/Class)
  await page.getByRole('combobox').nth(1).selectOption('10000');
  await page.getByRole('combobox').nth(2).selectOption('10100');
  await page.getByRole('combobox').nth(3).selectOption('10102');
  await page.getByRole('combobox').nth(4).selectOption('103004');

  // Open Product Selection Modal
  await page.getByRole('button', { name: '選取商品' }).click();
  
  // Search and select items
  await page.getByRole('button', { name: '搜尋' }).click();
  
  // Wait for results to load (optional but recommended for dynamic content)
  //await page.locator('.pure-checkbox > label').first().waitFor({ state: 'visible' });
  
  // Select first item
  //await page.locator('.pure-checkbox > label').first().click();
  // Select second item (based on nth row or specific selector)
  await page.locator('tbody:nth-child(4) > tr > td > .pure-checkbox > label').click();
  
  await page.getByRole('button', { name: '加入勾選商品' }).first().click();

  // Fill Price and Quantity information
  // These indices are from the recorder; in a real project, using data-test-id or placeholder is better.
  await page.getByRole('textbox').nth(4).fill('101');
  await page.getByRole('textbox').nth(5).fill('10');
  await page.locator('.row > div:nth-child(3) > .form-control').fill('10');
  await page.locator('.row > div:nth-child(4) > .form-control').fill('10');

  // Finalize Kit Details
  await page.getByRole('button', { name: '置入組合商品資料' }).click();
  await page.getByRole('button', { name: ' 確定' }).click();

  // Fill Short Description and Final Submit
  await page.locator('#form-validation-field-6').fill('playwrightOBKit短敘述');
  await page.getByRole('button', { name: ' 確定' }).click();
  
  // Basic validation that we haven't stayed on the same form (or check for a success message)
  // await expect(page).not.toHaveURL(/.*建立套組商品/);
  console.log(`Kit submission completed for: ${productName}`);
});
