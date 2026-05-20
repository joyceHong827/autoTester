import { test } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

test('Get All Storage Keys', async ({ page }) => {
  await b2eLogin(page);
  const data = await page.evaluate(() => {
    return {
      localStorage: Object.keys(localStorage),
      sessionStorage: Object.keys(sessionStorage),
      cookies: document.cookie
    };
  });
  console.log('STORAGE_DATA_START');
  console.log(JSON.stringify(data, null, 2));
  console.log('STORAGE_DATA_END');
});
