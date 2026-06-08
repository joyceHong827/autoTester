using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestFlowStudio.Core.Interfaces;
using TestFlowStudio.Core.Models;

namespace TestFlowStudio.Core.Services;

/// <summary>
/// Calls OpenAI-compatible APIs (OpenAI and Google Gemini via OpenAI-compat endpoint).
/// Switch provider by changing BaseUrl + ApiKey in settings.
/// </summary>
public class OpenAICompatService : IAIService
{
    private readonly HttpClient _http;
    private readonly AppSettings _settings;
    private readonly string _baseUrl;
    private readonly string _model;
    private readonly string _providerName;

    public string ProviderName => _providerName;

    // OpenAI
    public static OpenAICompatService CreateOpenAI(AppSettings s, string? overrideModel = null) =>
        new(s, "https://api.openai.com/v1",
            Helpers.SettingsManager.GetOpenAIApiKey(s),
            overrideModel.OrDefault(s.AI.OpenAIModel), "OpenAI");

    // Gemini via OpenAI-compatible endpoint
    public static OpenAICompatService CreateGemini(AppSettings s, string? overrideModel = null) =>
        new(s, "https://generativelanguage.googleapis.com/v1beta/openai",
            Helpers.SettingsManager.GetGeminiApiKey(s),
            overrideModel.OrDefault(s.AI.GeminiModel), "Gemini");

    // Ollama 本地端（不需要 API Key）
    public static OpenAICompatService CreateOllama(AppSettings s, string? overrideModel = null) =>
        new(s, $"{s.AI.OllamaBaseUrl.TrimEnd('/')}/v1",
            "ollama",   // Ollama 不驗證 Bearer，放任意字串即可
            overrideModel.OrDefault(s.AI.OllamaModel), "Ollama");

    private OpenAICompatService(
        AppSettings settings, string baseUrl, string apiKey, string model, string name)
    {
        _settings = settings;
        _baseUrl = baseUrl.TrimEnd('/');
        _model = model;
        _providerName = name;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
    }

    private string SystemPrompt(string task) => task == "testcase"
        ? """
          你是一位資深 QA 工程師，請根據提供的 Redmine Issue 內容，以繁體中文產生結構化的 Markdown 測試案例。
          嚴格遵循以下格式，不要新增任何其他內容：

          ##測試目的
          ##測試步驟
          ##測試資料
          ##預期結果
          ##實際結果
          ##驗證項目
          ##測試結論

          驗證項目請用表格呈現
          <!-- 由系統自動填入 -->

          規則：只描述 Issue 明確提及的行為；步驟具體可操作；預期結果與步驟一一對應。
          """
        : """
          你是資深 TypeScript Playwright 測試工程師。
          將 Codegen 錄製腳本轉換為含 assertion 的完整 .spec.ts。

          【核心規則】
          1. 保留所有 Codegen 操作，不刪減任何步驟
          2. 根據「測試案例」的「## 預期結果」與「## 測試步驟」加入對應的 expect() 斷言
          3. 使用 test.describe() 組織測試案例
          4. ⭐ **關鍵要求**：將每個有意義的操作區塊用 `await test.step('步驟描述', async () => { ... })` 包裝
          5. test.step() 的描述必須是繁體中文，清楚表達業務意圖（例：「登入系統」、「選擇品牌篩選條件」、「驗證資料正確顯示」）
          6. 只輸出完整可執行的 TypeScript 程式碼，不加任何說明文字

          【test.step() 包裝範例】
          ```typescript
          test('商品查詢測試', async ({ page }) => {
            await test.step('開啟登入頁面', async () => {
              await page.goto('/login');
            });

            await test.step('輸入帳號密碼並登入', async () => {
              await page.fill('#username', 'admin');
              await page.fill('#password', 'pass');
              await page.click('button[type=submit]');
            });

            await test.step('驗證登入成功進入首頁', async () => {
              await expect(page).toHaveURL(/dashboard/);
            });

            await test.step('進入商品查詢頁面', async () => {
              await page.click('text=商品管理');
              await page.click('text=商品查詢');
            });

            await test.step('驗證商品清單載入', async () => {
              await expect(page.locator('table tbody tr')).toHaveCount({ gte: 1 });
            });
          });
          ```

          【輸出格式】
          - 必須是完整的 .spec.ts 檔案（含 import、test.describe、test）
          - 每個操作區塊都用 test.step() 包裝
          - step 描述用繁體中文，對應測試案例的步驟說明
          - 不輸出任何 Markdown 程式碼區塊標記（```），只輸出純 TypeScript 原始碼
          """;

    public async Task<string> GenerateTestCaseAsync(
        string issueTitle, string issueBody, CancellationToken ct = default)
    {
        var sb = new StringBuilder();
        await GenerateTestCaseStreamAsync(issueTitle, issueBody, c => sb.Append(c), ct);
        return sb.ToString();
    }

    public async Task<string> TransformScriptAsync(
        string codegenScript, string testCaseMd, CancellationToken ct = default)
    {
        var sb = new StringBuilder();
        await TransformScriptStreamAsync(codegenScript, testCaseMd, c => sb.Append(c), ct);
        return sb.ToString();
    }

    public async Task GenerateTestCaseStreamAsync(
        string issueTitle, string issueBody, Action<string> onChunk, CancellationToken ct = default)
    {
        var user = $"Issue 標題：{issueTitle}\n\nIssue 描述：\n{issueBody}";
        await StreamAsync(SystemPrompt("testcase"), user, onChunk, ct);
    }

    public async Task TransformScriptStreamAsync(
        string codegenScript, string testCaseMd, Action<string> onChunk, CancellationToken ct = default)
    {
        var user = $"【測試案例】\n{testCaseMd}\n\n【Codegen 腳本】\n{codegenScript}";
        await StreamAsync(SystemPrompt("script"), user, onChunk, ct);
    }

    private async Task StreamAsync(
        string system, string userMsg, Action<string> onChunk, CancellationToken ct)
    {
        var payload = new
        {
            model = _model,
            max_tokens = _settings.AI.MaxTokens,
            stream = true,
            messages = new[]
            {
                new { role = "system", content = system },
                new { role = "user",   content = userMsg }
            }
        };

        var req = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/chat/completions")
        {
            Content = new StringContent(
                JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
        };

        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        resp.EnsureSuccessStatusCode();

        using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct) ?? "";
            if (!line.StartsWith("data: ")) continue;
            var json = line[6..];
            if (json == "[DONE]") break;
            try
            {
                var obj = JObject.Parse(json);
                var text = obj["choices"]?[0]?["delta"]?["content"]?.ToString();
                if (!string.IsNullOrEmpty(text)) onChunk(text);
            }
            catch { }
        }
    }

    public async Task<bool> ValidateConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                model = _model,
                max_tokens = 10,
                messages = new[] { new { role = "user", content = "hi" } }
            };
            var content = new StringContent(
                JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync($"{_baseUrl}/chat/completions", content, ct);
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}

file static class StringExtensions
{
    /// <summary>若字串為 null 或空白則回傳 fallback。</summary>
    internal static string OrDefault(this string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;
}
