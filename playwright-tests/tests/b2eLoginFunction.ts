import { Page, expect } from '@playwright/test';

export async function b2eLogin(page: Page, username?: string, password?: string) {
  // Navigate to the login page
  await page.goto('http://b2e.lab.etzone.net/web/B2ELogin');

  // Fill in the username
  await page.locator('#account').fill(username || 'admin');

  // Fill in the password
  await page.locator('#password').fill(password || 'sensengo168');

  // Select the empty value for the domain dropdown by clicking
  await page.locator('select[ng-model="userInfo.domain"]').selectOption({ value: '' });

  // Click the login button
  await page.locator('button[id="login"]').click();

  await page.waitForURL(
    'http://b2e.lab.etzone.net/web/DefaultNewPage',
    { timeout: 15000 }
  );
  
  await expect(page).toHaveURL(
    'http://b2e.lab.etzone.net/web/DefaultNewPage'
  );  
}
