import { test, expect } from '@playwright/test';
import { b2bLogin } from './b2bLogin';

test('check SPMPaymentRequest page', async ({ page }) => {
  await b2bLogin(page);
  await page.waitForURL('**/Web/DefaultPage');
  await page.goto('http://b2b.lab.etzone.net/Web/SPMPaymentRequest');
  await page.waitForTimeout(5000);
  await page.screenshot({ path: 'check_page.png', fullPage: true });
  
  const content = await page.content();
  console.log('Page content length:', content.length);
  
  const allText = await page.innerText('body');
  console.log('Body text contains 憑證:', allText.includes('憑證'));
  console.log('Body text contains 上傳:', allText.includes('上傳'));

  const elements = await page.locator(':text-matches("憑證|上傳", "i")').all();
  console.log('Found', elements.length, 'elements with matching text');
  for (const el of elements) {
    const tagName = await el.evaluate(e => e.tagName);
    const text = await el.innerText();
    console.log(`[${tagName}] text:`, text);
  }
});
