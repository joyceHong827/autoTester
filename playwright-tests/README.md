# Playwright Tests

此資料夾包含由 TestFlow Studio 自動產生的 Playwright 測試腳本。

## 安裝

```bash
npm install
npx playwright install
```

## 執行測試

```bash
# 無頭模式執行
npm test

# 有畫面執行
npm run test:headed

# Debug 模式
npm run test:debug
```

## 產生的測試

所有測試腳本會放在 `tests/` 資料夾中。
