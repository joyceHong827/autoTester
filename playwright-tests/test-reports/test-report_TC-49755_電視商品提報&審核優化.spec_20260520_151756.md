# 🎭 Playwright 測試報告

## 測試名稱：TC-49755_電視商品提報&審核優化.spec

---

**📅 產生時間**：2026-05-20 15:17:56
**📂 測試檔案**：D:\autoTester\playwright-tests\tests\TC-49755_電視商品提報&審核優化.spec.ts

## 📊 測試摘要

```
  狀態: ❌ FAILED
  通過率: 0.0%
  執行時間: 15:17:55
```

| 指標 | 數量 | 比例 |
|------|------|------|
| ✅ 通過測試 | -1 | 0.0% |
| ❌ 失敗測試 | 1 | 0.0% |
| 📋 總測試數 | 0 | 100% |
| 🔄 重試次數 | 0 | - |

## ✔️ 驗證項目結果

> 以下為測試情境中所定義的驗證項目，並依此次執行結果回填「是否通過」狀態。

| # | 驗證項目 | 預期結果 | 實際結果 | 是否通過 |
|---|---------|---------|---------|----------|
| 1 | 導航至登入頁面並執行登入 | 導航至登入頁面並執行登入 | 符合預期 | ✅ 通過 |
| 2 | 導航至「電視商品管理 ＞ 商品開發&審核」頁面 | 導航至「電視商品管理 ＞ 商品開發&審核」頁面 | 符合預期 | ✅ 通過 |
| 3 | 驗證「商品開發&審核」列表欄位名稱已更新 | 驗證「商品開發&審核」列表欄位名稱已更新 | 符合預期 | ✅ 通過 |
| 4 | 執行商品開發&審核頁面上的「商開審核」操作 | 執行商品開發&審核頁面上的「商開審核」操作 | 符合預期 | ✅ 通過 |
| 5 | 導航至「商審作業」頁面並驗證相關欄位顯示 | 導航至「商審作業」頁面並驗證相關欄位顯示 | 符合預期 | ✅ 通過 |
| 6 | 返回「商品開發&審核」頁面並驗證「送審類型」查詢選項 | 返回「商品開發&審核」頁面並驗證「送審類型」查詢選項 | 符合預期 | ✅ 通過 |
| 7 | 導航至「銷售商品查詢」頁面並驗證新增的查詢條件 | 導航至「銷售商品查詢」頁面並驗證新增的查詢條件 | 符合預期 | ✅ 通過 |
| 8 | 設定「送審類型」為「書審」、日期範圍並執行查詢 | 設定「送審類型」為「書審」、日期範圍並執行查詢 | Error: locator.click: Test timeout of 30000ms exceeded.
Call log:
[2m  - waiting for getByText('查詢 匯出')[22m
[2m    - locator resolved to <div class="col-md-3">…</div>[22m
[2m  - attempting click ... | ❌ 不通過 |

**驗證項目統計**：共 8 項 ｜ ✅ 通過 7 項 ｜ ❌ 不通過 1 項

---

## 🎯 測試情境案例

## ❌ 失敗詳情分析

### ⏱️ 超時錯誤

- 設定「送審類型」為「書審」、日期範圍並執行查詢: Error: locator.click: Test timeout of 30000ms exceeded.
Call log:
[2m  - waiting for getByText('查詢 匯出')[22m
[2m    - locator resolved to <div class="col-md-3">…</div>[22m
[2m  - attempting click action[22m
[2m    2 × waiting for element to be visible, enabled and stable[22m
[2m      - element is visible, enabled and stable[22m
[2m      - scrolling into view if needed[22m
[2m      - done scrolling[22m
[2m      - <div>…</div> intercepts pointer events[22m
[2m    - retrying click action[22m
[2m    - waiting 20ms[22m
[2m    2 × waiting for element to be visible, enabled and stable[22m
[2m      - element is visible, enabled and stable[22m
[2m      - scrolling into view if needed[22m
[2m      - done scrolling[22m
[2m      - <div>…</div> intercepts pointer events[22m
[2m    - retrying click action[22m
[2m      - waiting 100ms[22m
[2m    19 × waiting for element to be visible, enabled and stable[22m
[2m       - element is visible, enabled and stable[22m
[2m       - scrolling into view if needed[22m
[2m       - done scrolling[22m
[2m       - <div>…</div> intercepts pointer events[22m
[2m     - retrying click action[22m
[2m       - waiting 500ms[22m
[2m    2 × waiting for element to be visible, enabled and stable[22m
[2m      - element is visible, enabled and stable[22m
[2m      - scrolling into view if needed[22m
[2m      - done scrolling[22m
[2m      - <img src="/Web/Images/logo.gif"/> from <header class="container-fluid b2e-header">…</header> subtree intercepts pointer events[22m
[2m    - retrying click action[22m
[2m      - waiting 500ms[22m


## 📊 Playwright 詳細報告

使用以下命令查看 Playwright 原生 HTML 報告：

```powershell
cd D:\autoTester\playwright-tests
npx playwright show-report
```

或開啟檔案：`playwright-tests/playwright-report/index.html`

## 📸 測試附件

- **截圖位置**：`playwright-tests/test-results/`
- **影片位置**：`playwright-tests/test-results/`

<details>
<summary>📄 點擊查看完整輸出日誌</summary>

```

Running 1 test using 1 worker

  x  1 [chromium] › tests\TC-49755_電視商品提報&審核優化.spec.ts:7:7 › 電視商品管理模組優化驗證 › 商品開發&審核與銷售商品查詢功能驗證 (31.0s)


  1) [chromium] › tests\TC-49755_電視商品提報&審核優化.spec.ts:7:7 › 電視商品管理模組優化驗證 › 商品開發&審核與銷售商品查詢功能驗證 › 設定「送審類型」為「書審」、日期範圍並執行查詢 

    Test timeout of 30000ms exceeded.

    Error: locator.click: Test timeout of 30000ms exceeded.
    Call log:
      - waiting for getByText('查詢 匯出')
        - locator resolved to <div class="col-md-3">…</div>
      - attempting click action
        2 × waiting for element to be visible, enabled and stable
          - element is visible, enabled and stable
          - scrolling into view if needed
          - done scrolling
          - <div>…</div> intercepts pointer events
        - retrying click action
        - waiting 20ms
        2 × waiting for element to be visible, enabled and stable
          - element is visible, enabled and stable
          - scrolling into view if needed
          - done scrolling
          - <div>…</div> intercepts pointer events
        - retrying click action
          - waiting 100ms
        19 × waiting for element to be visible, enabled and stable
           - element is visible, enabled and stable
           - scrolling into view if needed
           - done scrolling
           - <div>…</div> intercepts pointer events
         - retrying click action
           - waiting 500ms
        2 × waiting for element to be visible, enabled and stable
          - element is visible, enabled and stable
          - scrolling into view if needed
          - done scrolling
          - <img src="/Web/Images/logo.gif"/> from <header class="container-fluid b2e-header">…</header> subtree intercepts pointer events
        - retrying click action
          - waiting 500ms


      120 |
      121 |       // Codegen 在日期選擇前，點擊了「查詢 匯出」文本區域。此操作的具體意圖不明，但依規保留。
    > 122 |       await page.getByText('查詢 匯出').click();
          |                                     ^
      123 |
      124 |       // 設定日期範圍（商開審核時間）
      125 |       await page.getByRole('textbox').nth(4).click(); // 點擊開始日期輸入框
        at D:\autoTester\playwright-tests\tests\TC-49755_電視商品提報&審核優化.spec.ts:122:37
        at D:\autoTester\playwright-tests\tests\TC-49755_電視商品提報&審核優化.spec.ts:116:5

    attachment #1: screenshot (image/png) ──────────────────────────────────────────────────────────
    test-results\TC-49755_電視商品提報&審核優化-電視商品管理模組優化驗證-商品開發-審核與銷售商品查詢功能驗證-chromium\test-failed-1.png
    ────────────────────────────────────────────────────────────────────────────────────────────────

    attachment #2: video (video/webm) ──────────────────────────────────────────────────────────────
    test-results\TC-49755_電視商品提報&審核優化-電視商品管理模組優化驗證-商品開發-審核與銷售商品查詢功能驗證-chromium\video.webm
    ────────────────────────────────────────────────────────────────────────────────────────────────

    Error Context: test-results\TC-49755_電視商品提報&審核優化-電視商品管理模組優化驗證-商品開發-審核與銷售商品查詢功能驗證-chromium\error-context.md

  1 failed
    [chromium] › tests\TC-49755_電視商品提報&審核優化.spec.ts:7:7 › 電視商品管理模組優化驗證 › 商品開發&審核與銷售商品查詢功能驗證 ────────

```

</details>

---

*本報告由 TestFlow Studio 自動產生於 2026-05-20 15:17:56*
