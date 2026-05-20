import { test } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

test('Get OB Product FugoSaleNo and Call API', async ({ page }) => {
  // 1. Login
  await b2eLogin(page);

  // 2. Navigate to OB Product Management
  await page.getByRole('link', { name: 'OB商品管理' }).click();
  await page.getByRole('link', { name: 'OB商品審核&維護' }).click();
  await page.getByRole('button', { name: '商開審核' }).click();

  // 3. Find the product link and extract FugoSaleNo
  const productLink = page.getByRole('link', { name: '作價測試商品-不勾選作價品-OB' });
  const href = await productLink.getAttribute('href');
  console.log('Product Href:', href);

  // Example Href might be: /Web/ProductDetail?f=1214841
  const match = href?.match(/[?&]f=(\d+)/);
  const fugoSaleNo = match ? match[1] : '1214841'; // Fallback to a known one if not found
  console.log('Extracted FugoSaleNo:', fugoSaleNo);

  // 4. Try to call the API from the page context to reuse the session
  // Usually the token is in localStorage or cookies
  const result = await page.evaluate(async (fugo) => {
    // Try to find the token
    const token = localStorage.getItem('token') || ''; 
    const response = await fetch('https://redapi.etzone.net/o/api/Product/AccessedNum', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify([{ FugoSaleNo: fugo, AccessedNum: [1] }])
    });
    return {
      status: response.status,
      body: await response.text()
    };
  }, fugoSaleNo);

  console.log('API Response:', result);
});
