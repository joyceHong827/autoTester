---
redmine_issue_id: 49402
title: '[RedAPI] 壓縮圖檔異常訊息優化'
status: pending
last_run: ''
created_at: 2026-03-27T17:50:20.3933805+08:00
playwright_script: ./GeneratedScripts/TC-49402_[RedAPI] 壓縮圖檔異常訊息優化.spec.ts
---

## 測試目標
*   驗證當壓縮廣告圖檔 (`GetCompressAdsImg`) 時，圖片長寬小於設定值，錯誤訊息會顯示實際長寬且文字已優化。
*   驗證當壓縮圖檔 (`CompressImgs`) 失敗時，錯誤訊息中的圖片類型能正確轉換為中文描述。
*   驗證新品提報 (`SubmitNewProduct`) 或修改提報 (`SubmitModifiedProduct`) 呼叫壓縮圖檔失敗時，錯誤日誌能正確記錄壓縮錯誤訊息、`ImageInfo`、`draftView` 及廠商資訊，且不記錄 `Stream` 內容；同時使用者訊息不包含內部資訊。
*   驗證廣告用圖修改 (`ChangeAdsPicture`) 呼叫壓縮圖檔失敗時，錯誤日誌能正確記錄壓縮錯誤訊息、`ImageInfo`、`req` 及廠商資訊。

## 前置條件
*   系統已部署相關服務並可正常運行。
*   NLog 日誌功能已啟用並可正確記錄錯誤訊息。
*   壓縮服務的最小長寬限制已設定 (例如：500px)。
*   `ImageInfo` 結構中的圖片類型已配置中文描述（例如：透過 `Description` 屬性）。
*   測試環境可模擬壓縮服務回傳失敗。

## 測試步驟

### 測試案例 1：壓縮廣告圖檔 (`GetCompressAdsImg`) 錯誤訊息優化
1.  準備一個圖片串流，其圖片長寬皆小於預設的最小長寬限制 (例如：寬 300px, 高 200px)。
2.  呼叫 `GetCompressAdsImg(Stream)` 方法，並傳入步驟 1 準備的圖片串流。
3.  檢查 NLog 錯誤日誌。

### 測試案例 2：壓縮圖檔 (`CompressImgs`) 錯誤訊息優化 (圖片類型中文化)
1.  準備一個 `Dictionary<ImageInfo, Stream>`，其中包含一個 `ImageInfo` 項目及其對應的圖片串流 (例如：圖片類型為「主圖」)。
2.  模擬壓縮服務回傳錯誤，導致 `CompressImgs` 方法失敗。
3.  呼叫 `CompressImgs(Dictionary<ImageInfo, Stream>)` 方法，並傳入步驟 1 準備的字典。
4.  檢查 NLog 錯誤日誌。

### 測試案例 3：新品提報 (`SubmitNewProduct`) 壓縮失敗日誌優化
1.  準備一份 `DraftView` 資料，其中包含至少一張圖片資訊，該圖片預計會導致壓縮服務失敗。
2.  模擬壓縮服務回傳錯誤，使 `CompressImgs` 方法失敗。
3.  呼叫 `SubmitNewProduct(DraftView)` 方法，並傳入步驟 1 準備的 `DraftView` 資料。
4.  檢查 NLog 錯誤日誌。
5.  檢查 `SubmitNewProduct` 方法回傳給使用者的 `result.msg`。

### 測試案例 4：修改提報 (`SubmitModifiedProduct`) 壓縮失敗日誌優化
1.  準備一份 `DraftView` 資料，其中包含至少一張圖片資訊，該圖片預計會導致壓縮服務失敗。
2.  模擬壓縮服務回傳錯誤，使 `CompressImgs` 方法失敗。
3.  呼叫 `SubmitModifiedProduct(DraftView)` 方法，並傳入步驟 1 準備的 `DraftView` 資料。
4.  檢查 NLog 錯誤日誌。
5.  檢查 `SubmitModifiedProduct` 方法回傳給使用者的 `result.msg`。

### 測試案例 5：廣告用圖修改API (`ChangeAdsPicture`) 壓縮失敗日誌優化
1.  準備一個 `List<ProductAdsPictureMessage.ChangeAdsPicture>` 請求資料，其中包含至少一張圖片資訊，該圖片預計會導致壓縮服務失敗。
2.  模擬壓縮服務回傳錯誤。
3.  呼叫 `ChangeAdsPicture(List<ProductAdsPictureMessage.ChangeAdsPicture>)` 方法，並傳入步驟 1 準備的請求資料。
4.  檢查 NLog 錯誤日誌。

## 預期結果

### 測試案例 1：壓縮廣告圖檔 (`GetCompressAdsImg`) 錯誤訊息優化
*   NLog 錯誤日誌中應包含類似 "圖片任一長寬小於500px, 實際寬度: 300px, 實際高度: 200px" 的訊息 (數字應與傳入圖片的實際長寬相符)。

### 測試案例 2：壓縮圖檔 (`CompressImgs`) 錯誤訊息優化 (圖片類型中文化)
*   NLog 錯誤日誌中應包含壓縮失敗的錯誤訊息，且若訊息內容中提及圖片類型，應顯示其中文描述 (例如："圖片類型: 主圖")。

### 測試案例 3：新品提報 (`SubmitNewProduct`) 壓縮失敗日誌優化
*   NLog