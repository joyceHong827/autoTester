# TestFlow Studio

> Windows 自動化測試工作台 — 整合 Redmine、AI 與 Playwright 的端對端測試自動化工具

---

## 功能概覽

| 功能 | 說明 |
|------|------|
| Redmine Issue 讀取 | 透過 REST API 讀取 self-hosted Redmine 的 Issue 清單與詳細內容 |
| AI 測試案例生成 | 將 Issue 描述送入 AI，自動產生結構化 Markdown 測試案例（.md） |
| Playwright Codegen 錄製 | 在應用程式內啟動瀏覽器錄製，捕捉操作腳本 |
| AI 腳本轉換 | 將錄製腳本 + 測試案例合併，讓 AI 補全 assertion，產生 TypeScript .spec.ts |
| 測試執行與回寫 | 執行測試腳本，結果自動更新至 .md；可選回寫至 Redmine Journal |

---

## 系統需求

| 項目 | 版本 |
|------|------|
| Windows | 10 / 11（64-bit） |
| .NET SDK | 8.0 以上 |
| Node.js | 18.0 以上 |
| Visual Studio | 2022（含 .NET 桌面開發工作負載） |

---

## 快速開始

### 1. 複製或搬移專案

```
C:\Users\Administrator\Documents\autoTester\
```

將整個 `autoTester` 目錄搬移至你偏好的位置（例如 `D:\autoTester`）。

### 2. 安裝 Playwright

在 `playwright-tests` 目錄下執行：

```bash
cd playwright-tests
npm install
npx playwright install chromium
```

### 3. 開啟 Visual Studio 解決方案

```
TestFlowStudio.sln
```

執行 NuGet 還原（Build → Restore NuGet Packages），然後建置解決方案。

### 4. 設定應用程式

啟動應用程式後，點擊「⚙ 設定」頁籤，填入：

- **Redmine URL**：例如 `http://192.168.1.100/redmine`
- **Redmine API Key**：從 Redmine「我的帳戶」頁面取得
- **AI Provider**：選擇 Claude / OpenAI / Gemini 並填入 API Key
- **輸出目錄**：測試案例與腳本的儲存路徑

---

## 支援的 AI Provider

| Provider | Base URL | 認證方式 |
|----------|----------|---------|
| Anthropic Claude | `https://api.anthropic.com/v1/messages` | `x-api-key` 標頭 |
| OpenAI | `https://api.openai.com/v1/chat/completions` | `Authorization: Bearer` |
| Google Gemini | `https://generativelanguage.googleapis.com/v1beta/openai/` | `Authorization: Bearer`（OpenAI 相容端點） |

> **注意**：Gemini 使用 Google 提供的 OpenAI 相容端點，可直接複用 OpenAI 的請求格式。

---

## 專案結構

```
TestFlowStudio.sln
├── TestFlowStudio.WinForms/        # WinForms 主程式
│   ├── Forms/
│   │   ├── MainForm.cs             # 主視窗（6 個頁籤）
│   │   ├── MainForm.Designer.cs    # UI 佈局
│   │   ├── SettingsForm.cs         # 設定視窗
│   │   └── SettingsForm.Designer.cs
│   ├── Program.cs
│   └── appsettings.json            # 設定檔（自動產生，API Key 以 DPAPI 加密）
│
├── TestFlowStudio.Core/            # 核心邏輯（無 UI 依賴）
│   ├── Interfaces/
│   │   ├── IAIService.cs
│   │   ├── IRedmineService.cs
│   │   └── IPlaywrightService.cs
│   ├── Services/
│   │   ├── AIServiceFactory.cs     # 根據設定建立對應 AI 服務
│   │   ├── ClaudeAIService.cs      # Anthropic Claude（原生格式）
│   │   ├── OpenAICompatService.cs  # OpenAI + Gemini（OpenAI 相容格式）
│   │   ├── RedmineService.cs       # Redmine REST API
│   │   ├── PlaywrightService.cs    # Codegen 錄製 + 測試執行
│   │   ├── TestCaseGenerator.cs    # Issue → .md 測試案例
│   │   ├── ScriptTransformer.cs    # Codegen 腳本 → .spec.ts
│   │   └── ResultWriter.cs         # 測試結果 → .md / Redmine
│   ├── Models/
│   │   ├── AppSettings.cs
│   │   ├── RedmineModels.cs
│   │   └── TestCase.cs
│   └── Helpers/
│       ├── EncryptionHelper.cs     # Windows DPAPI 加解密
│       ├── MarkdownHelper.cs       # .md YAML Front Matter 讀寫
│       └── SettingsManager.cs      # 設定檔載入 / 儲存
│
└── playwright-tests/               # Node.js TypeScript 測試專案
    ├── package.json
    ├── playwright.config.ts
    ├── tsconfig.json
    └── tests/
        └── example.spec.ts         # 範例測試（AI 生成的腳本放置於此）
```

---

## 測試案例 .md 格式

```markdown
---
redmine_issue_id: 1234
title: "使用者登入功能 - 正常流程"
status: pending          # pending / passed / failed / error
last_run: ""
created_at: "2026-03-19T10:00:00+08:00"
playwright_script: "./GeneratedScripts/TC-1234_xxx.spec.ts"
---

## 測試目標
驗證使用者以正確帳號密碼登入後能成功進入首頁。

## 前置條件
- 測試帳號已存在

## 測試步驟
1. 開啟登入頁面
2. 輸入帳號與密碼
3. 點擊登入按鈕

## 預期結果
1. 登入頁正確載入
2. 帳號密碼欄位可輸入
3. 頁面導向至 /dashboard

## 測試結果
### Run 2026-03-19 15:30:00
**結果**：✅ PASSED（通過 3 / 共 3）
```

---

## 開發快捷鍵

| 快捷鍵 | 動作 |
|--------|------|
| `F5` | 執行選取的測試腳本 |
| `Ctrl+R` | 開始 Playwright Codegen 錄製 |
| `Ctrl+G` | 從選取的 Issue 生成測試案例 |

---

## 常見問題

**Q：Playwright Codegen 啟動後沒有出現瀏覽器視窗？**
請確認 Node.js 已安裝且可在 PATH 中找到，並執行 `npx playwright install chromium`。

**Q：AI 生成的腳本 assertion 不正確？**
請確保 Redmine Issue 的「描述」欄位清楚描述操作步驟與預期行為；描述越具體，AI 生成品質越高。

**Q：Redmine API Key 儲存在哪裡？**
API Key 以 Windows DPAPI 加密後儲存於 `appsettings.json`，只有同一 Windows 帳號才能解密。

**Q：Gemini 如何設定？**
在設定頁選擇 Provider 為「Gemini」，填入 Google AI Studio 取得的 API Key（格式：`AIzaSy...`），Model 填寫 `gemini-2.0-flash` 或 `gemini-1.5-pro`。

---

## 版本歷史

| 版本 | 日期 | 說明 |
|------|------|------|
| v1.1 | 2026-03-19 | 初始版本，支援 Claude / OpenAI / Gemini，TypeScript Playwright |

---

*TestFlow Studio · MIT License*
