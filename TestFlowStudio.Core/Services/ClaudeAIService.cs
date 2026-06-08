using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestFlowStudio.Core.Interfaces;
using TestFlowStudio.Core.Models;

namespace TestFlowStudio.Core.Services;

/// <summary>
/// Calls the Anthropic Claude Messages API (native format).
/// </summary>
public class ClaudeAIService : IAIService
{
    private readonly HttpClient _http;
    private readonly AppSettings _settings;
    private readonly string? _overrideModel;
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";

    public string ProviderName => "Claude";

    public ClaudeAIService(AppSettings settings, string? overrideModel = null)
    {
        _settings = settings;
        _overrideModel = overrideModel;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
        var key = Helpers.SettingsManager.GetClaudeApiKey(settings);
        _http.DefaultRequestHeaders.Add("x-api-key", key);
        _http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    private string Model =>
        string.IsNullOrWhiteSpace(_overrideModel) ? _settings.AI.ClaudeModel : _overrideModel;

    private string SystemPrompt(string task) => task == "testcase"
        ? """
          你是一位資深 QA 工程師，請根據提供的 Redmine Issue 內容，以繁體中文產生結構化的 Markdown 測試案例。
          嚴格遵循以下格式，不要新增格式以外的任何內容：

          ## 測試目標
          （一句話描述）

          ## 前置條件
          - 條件一
          - 條件二

          ## 測試步驟
          1. 步驟一
          2. 步驟二

          ## 預期結果
          1. 對應步驟一的預期
          2. 對應步驟二的預期

          ## 測試結果
          <!-- 由系統自動填入 -->

          規則：
          - 只描述 Issue 中明確提及的行為，禁止推測未說明的功能
          - 步驟必須具體可操作
          - 預期結果須與測試步驟一一對應
          """
        : """
          你是一位資深 TypeScript Playwright 測試工程師。
          請將提供的 Playwright Codegen 錄製腳本，轉換為符合測試案例的完整 .spec.ts 測試檔。

          規則：
          1. 保留所有錄製的操作步驟
          2. 根據「預期結果」中的每一條，加入對應的 expect() assertion
          3. 使用 async/await
          4. 使用 @playwright/test 的 test()、expect() API
          5. 以 test.describe() 包裝整個測試
          6. 不要推測或新增錄製中未出現的操作
          7. 只輸出程式碼，不要任何說明文字或 markdown 區塊標記
          """;

    public async Task<string> GenerateTestCaseAsync(
        string issueTitle, string issueBody, CancellationToken ct = default)
    {
        var result = new StringBuilder();
        await GenerateTestCaseStreamAsync(issueTitle, issueBody, chunk => result.Append(chunk), ct);
        return result.ToString();
    }

    public async Task<string> TransformScriptAsync(
        string codegenScript, string testCaseMd, CancellationToken ct = default)
    {
        var result = new StringBuilder();
        await TransformScriptStreamAsync(codegenScript, testCaseMd, chunk => result.Append(chunk), ct);
        return result.ToString();
    }

    public async Task GenerateTestCaseStreamAsync(
        string issueTitle, string issueBody, Action<string> onChunk, CancellationToken ct = default)
    {
        var userMsg = $"Issue 標題：{issueTitle}\n\nIssue 描述：\n{issueBody}";
        await StreamAsync(SystemPrompt("testcase"), userMsg, onChunk, ct);
    }

    public async Task TransformScriptStreamAsync(
        string codegenScript, string testCaseMd, Action<string> onChunk, CancellationToken ct = default)
    {
        var userMsg = $"【測試案例】\n{testCaseMd}\n\n【Codegen 腳本】\n{codegenScript}";
        await StreamAsync(SystemPrompt("script"), userMsg, onChunk, ct);
    }

    private async Task StreamAsync(
        string system, string userMsg, Action<string> onChunk, CancellationToken ct)
    {
        var payload = new
        {
            model = Model,
            max_tokens = _settings.AI.MaxTokens,
            stream = true,
            system,
            messages = new[] { new { role = "user", content = userMsg } }
        };

        var req = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
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
                var text = obj["delta"]?["text"]?.ToString();
                if (!string.IsNullOrEmpty(text)) onChunk(text);
            }
            catch { /* skip malformed lines */ }
        }
    }

    public async Task<bool> ValidateConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                model = Model,
                max_tokens = 10,
                messages = new[] { new { role = "user", content = "hi" } }
            };
            var req = new StringContent(
                JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync(ApiUrl, req, ct);
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
