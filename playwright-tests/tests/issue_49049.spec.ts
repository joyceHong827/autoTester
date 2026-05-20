import { test, expect, Page } from '@playwright/test';
import { b2eLogin } from './b2eLoginFunction';

test.describe('Issue #49049: Filter Content Tags in Issue Management', () => {
  test.setTimeout(60000); // Increase timeout to 120 seconds
  let page: Page;

  // Before all tests, create a new browser context and log in.
  test.beforeAll(async ({ browser }) => {
    page = await browser.newPage();
    // Perform login. Note: Using lab environment as per the login function.
    await b2eLogin(page, 'admin', 'sensengo168'); 
    
    // Navigate to the issue management page after login.
    await page.goto('http://b2e.lab.etzone.net/Web/IssueManagement');
    // Wait for the main content to be visible, indicating the page has loaded.
    //await page.waitForSelector('#main-content');
   // Fill in the issue title     
    await page.getByRole('searchbox', { name: '輸入使用者名稱' }).nth(1).fill('');
    await page.getByRole('button', { name: '  待處理' }).click();
    await page.getByText('Lingsun_49058_處理人Email為空').click();
    await page.locator('#editor').click(); 
  });

  // After all tests, close the page.
  test.afterAll(async () => {
    await page.close();
  });

  // A helper function to run a single test case.
  const runPasteTest = async (description: string, inputHtml: string, expectedText: string) => {
    await test.step(description, async () => {
      // Click on the first issue in the list to open the details/reply view.
      // This assumes there's at least one issue.
      // Locate the Quill editor container.
      const editorContainer = page.locator('#editor');
      await editorContainer.click(); // Focus the editor.

      // New method: Simulate a paste event
      await editorContainer.evaluate((node, { html }) => {
        const editor = node.querySelector('.ql-editor');
        if (!editor) return;

        // The data needs to be in a DataTransfer object to simulate a real paste event
        const dataTransfer = new DataTransfer();
        dataTransfer.setData('text/html', html);
        // A plain text fallback can be useful
        const plainText = new DOMParser().parseFromString(html, 'text/html').body.textContent || "";
        dataTransfer.setData('text/plain', plainText);

        const pasteEvent = new ClipboardEvent('paste', {
          bubbles: true,
          cancelable: true,
          clipboardData: dataTransfer,

        });

        editor.dispatchEvent(pasteEvent);
      }, { html: inputHtml });

      // Wait a moment for Quill to process the pasted content and update its state.
      await page.waitForTimeout(500);
      // Click the submit button to save the reply.
      await page.locator('button:has-text("回覆廠商")').click();
      await page.locator('button:has-text("送出")').click();
      await page.getByRole('button', { name: 'OK' }).click();
      // Wait for the confirmation or for the page to reflect the new content.
      // A simple delay to wait for the DOM to update. A better way would be to wait for a specific element.
      await page.waitForTimeout(2000); 
      // Wait for the main content to be visible, indicating the page has loaded.
      //await page.waitForSelector('#main-content');
     // Fill in the issue title
      await page.getByText('Lingsun_49058_處理人Email為空',{ exact: true }).first().click({timeout:2000});
      await page.locator('#editor').click();

      await page.waitForTimeout(2000);

      // Verify the saved content. We check the last reply added.
      const lastReplyContent = await page.locator('.reply-content').last().innerText();
      
      const sanitizedContent = lastReplyContent.trim().replace(/\u00A0/g, ' ');
      // Assert that the sanitized content matches the expectation.
      expect(sanitizedContent).toBe(expectedText);

      // Navigate back to the list view for the next test.
      //await page.goto('http://b2e.lab.etzone.net/Web/IssueManagement');      
    });
  };

    // A helper function to run a single test case.
    const runPasteTestErrorDialog = async (description: string, inputHtml: string, expectedText: string) => {
      await test.step(description, async () => {
        // Click on the first issue in the list to open the details/reply view.
        // This assumes there's at least one issue.
        // Locate the Quill editor container.
        const editorContainer = page.locator('#editor');
        await editorContainer.click(); // Focus the editor.
  
        // New method: Simulate a paste event
        await editorContainer.evaluate((node, { html }) => {
          const editor = node.querySelector('.ql-editor');
          if (!editor) return;
  
          // The data needs to be in a DataTransfer object to simulate a real paste event
          const dataTransfer = new DataTransfer();
          dataTransfer.setData('text/html', html);
          // A plain text fallback can be useful
          const plainText = new DOMParser().parseFromString(html, 'text/html').body.textContent || "";
          dataTransfer.setData('text/plain', plainText);
  
          const pasteEvent = new ClipboardEvent('paste', {
            bubbles: true,
            cancelable: true,
            clipboardData: dataTransfer,
  
          });
  
          editor.dispatchEvent(pasteEvent);
        }, { html: inputHtml });
  
        // Wait a moment for Quill to process the pasted content and update its state.
        await page.waitForTimeout(500);
        // Click the submit button to save the reply.
        await page.locator('button:has-text("回覆廠商")').click();
        await page.locator('button:has-text("送出")').click();
        
        await page.waitForTimeout(8000);
        // 定位 swal 的內容區塊
        const swalMessage = page.locator('.swal-text');

        // 等待視窗出現
        await swalMessage.waitFor({ state: 'visible', timeout: 2000 });

        // 獲取文字
        const content = await swalMessage.textContent();
        expect(content?.trim()).toBe(expectedText);
      });
    };

  // --- Test Cases from test_scenarios_49049.md ---

  // B-1: Hyperlink Filtering
  test('B-1: Should filter out hyperlink tags', async () => {
    await runPasteTest(
      'Test Hyperlink Filtering',
      `這是一個 <a href="http://example.com">連結</a>`,
      '這是一個 連結'
    );
  });

  // B-2: List Filtering (UL/OL)
  test('B-2: Should filter out list tags', async () => {
    await runPasteTest(
      'Test List Filtering',
      `<ul><li>項目一</li><li>項目二</li></ul>`,
      '項目一   項目二' // Quill often preserves newlines between list items.
    );
  });

  // B-4: Style Filtering
  test('B-4: Should filter out style attributes', async () => {
    await runPasteTest(
      'Test Style Filtering',
      `<span style="color: red;">紅色文字</span> and <b>bold text</b>`,
      '紅色文字 and <b>bold text</b>'
    );
  });

  // C-1: Basic XSS Filtering
  test('C-1: Should filter out script tags', async () => {
    await runPasteTest(
      'Test Basic XSS Filtering',
      `正常內容<script>alert('XSS')</script>`,
      '正常內容alert(\'XSS\')'
    );
  });

  // C-2: Event Handler XSS Filtering
  test('C-2: Should filter out event handler attributes', async () => {
    await runPasteTestErrorDialog(
      'Test Event Handler XSS',
      `<img src=x onerror=alert('XSS')> Evil Image`,
      '處理圖片資料發生問題' // The image tag should be removed or sanitized to be empty.
    );
  });
});
