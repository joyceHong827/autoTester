import { Page, expect } from '@playwright/test';

export async function b2bLogin(page: Page) {
  await page.goto('http://b2b.lab.etzone.net/Web/B2B_B2ELogin');  
  await page.getByRole('textbox', { name: '統一編號' }).fill('12718372');  
  await page.getByRole('textbox', { name: '密碼' }).fill('123456');
  await page.getByRole('button', { name: ' 登入' }).click();
  
  // To make the script more robust, handle the case where the button might not appear.
  const remindLaterButton = page.getByRole('button', { name: '下次再說' });
  if (await remindLaterButton.isVisible({ timeout: 5000 }).catch(() => false)) {
    await remindLaterButton.click();
  }

  // It's a good practice to add an assertion to confirm that the login was successful.
  // For example, you can check if the URL has changed to the post-login page.
  //await expect(page).toHaveURL(/.*\/Web\/B2B_Index/, { timeout: 10000 });
}
