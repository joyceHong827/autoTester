using System.Text;
using TestFlowStudio.Core.Interfaces;
using TestFlowStudio.Core.Models;

namespace TestFlowStudio.Core.Services;

/// <summary>
/// AI-powered Playwright script optimizer with chat interface
/// </summary>
public class PlaywrightAIOptimizer
{
    private readonly IAIService _aiService;
    private readonly IPlaywrightService _playwrightService;
    private readonly AppSettings _settings;

    public PlaywrightAIOptimizer(
        IAIService aiService,
        IPlaywrightService playwrightService,
        AppSettings settings)
    {
        _aiService = aiService;
        _playwrightService = playwrightService;
        _settings = settings;
    }

    /// <summary>
    /// Auto-retry and optimize failed test
    /// </summary>
    public async Task<TestRunResult> RunWithAutoRetryAsync(
        string scriptPath,
        string testCaseMd,
        Action<string>? onOutput = null,
        Action<ChatMessage>? onChatMessage = null,
        CancellationToken ct = default)
    {
        var maxRetries = _settings.Playwright.MaxRetryAttempts;
        TestRunResult? lastResult = null;
        var currentScript = await File.ReadAllTextAsync(scriptPath, ct);
        var originalScript = currentScript;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            onOutput?.Invoke($"\n=== 測試執行第 {attempt + 1} 次 ===\n");

            // Run test
            lastResult = await _playwrightService.RunTestAsync(
                scriptPath,
                onOutput,
                ct);

            lastResult.RetryAttempt = attempt;

            // Success!
            if (lastResult.Success)
            {
                onOutput?.Invoke("\n✅ 測試通過！\n");
                break;
            }

            // Failed and no more retries
            if (attempt >= maxRetries)
            {
                onOutput?.Invoke($"\n❌ 已達最大重試次數 ({maxRetries})，測試仍失敗。\n");
                break;
            }

            // Failed - ask AI to fix
            onOutput?.Invoke($"\n🤖 測試失敗，請求 AI 優化腳本 (第 {attempt + 1}/{maxRetries} 次重試)...\n");

            var chatMsg = new ChatMessage
            {
                Role = "system",
                Content = $"測試執行失敗 (第 {attempt + 1} 次)，開始 AI 優化...",
                Timestamp = DateTime.Now
            };
            onChatMessage?.Invoke(chatMsg);

            // Generate fix
            var fixedScript = await OptimizeFailedScriptAsync(
                currentScript,
                testCaseMd,
                lastResult,
                onChatMessage,
                ct);

            if (string.IsNullOrWhiteSpace(fixedScript))
            {
                onOutput?.Invoke("⚠️ AI 無法產生修正腳本，停止重試。\n");
                break;
            }

            // Save optimized script
            currentScript = fixedScript;
            await File.WriteAllTextAsync(scriptPath, fixedScript, ct);

            lastResult.RetryHistory.Add($"Attempt {attempt + 1}: {string.Join("; ", lastResult.FailureMessages)}");

            onOutput?.Invoke("✨ 已套用 AI 優化的腳本，準備重新執行...\n");
        }

        return lastResult ?? new TestRunResult { Status = "error" };
    }

    /// <summary>
    /// Optimize failed script using AI
    /// </summary>
    public async Task<string> OptimizeFailedScriptAsync(
        string currentScript,
        string testCaseMd,
        TestRunResult failedResult,
        Action<ChatMessage>? onChatMessage = null,
        CancellationToken ct = default)
    {
        var systemPrompt = @"你是資深 Playwright 測試工程師。
用戶的測試腳本執行失敗了，請分析錯誤訊息並修正腳本。

【核心規則】
1. 保留原有測試邏輯和結構
2. 只修正導致失敗的部分
3. 常見問題：選擇器錯誤、時序問題、assertion 錯誤
4. 建議使用更穩定的選擇器（data-testid, role, 或更具體的 CSS/XPath）
5. 必要時增加 page.waitForSelector() 或 page.waitForLoadState('networkidle')
6. ⭐ **保持 test.step() 結構**：若原腳本有用 test.step() 包裝，修正後必須保留相同結構與步驟描述
7. 若原腳本沒有 test.step()，建議加上（參考測試案例的步驟說明，用繁體中文描述）
8. 只輸出完整的修正後 TypeScript 程式碼
9. 不要添加任何解釋或註解

【test.step() 範例】
若測試失敗在「點擊按鈕」步驟，修正時保持結構：
```typescript
await test.step('點擊送出按鈕', async () => {
  // 修正：加上 waitFor 確保按鈕可見
  await page.waitForSelector('button[type=submit]', { state: 'visible' });
  await page.click('button[type=submit]');
});
```";

        var userPrompt = $@"【測試案例】
{testCaseMd}

【當前腳本】
```typescript
{currentScript}
```

【執行錯誤】
總測試數：{failedResult.TotalTests}
失敗數：{failedResult.FailedTests}
錯誤訊息：
{string.Join("\n", failedResult.FailureMessages)}

【原始輸出】
{failedResult.RawOutput}

請修正腳本並輸出完整的 TypeScript 程式碼。";

        var requestMsg = new ChatMessage
        {
            Role = "user",
            Content = userPrompt,
            Timestamp = DateTime.Now
        };
        onChatMessage?.Invoke(requestMsg);

        var sb = new StringBuilder();
        await _aiService.TransformScriptStreamAsync(
            currentScript,
            $"{testCaseMd}\n\n## 錯誤修正需求\n{string.Join("\n", failedResult.FailureMessages)}",
            chunk =>
            {
                sb.Append(chunk);
            },
            ct);

        var response = sb.ToString();

        var responseMsg = new ChatMessage
        {
            Role = "assistant",
            Content = response,
            Timestamp = DateTime.Now
        };
        onChatMessage?.Invoke(responseMsg);

        return ExtractCodeFromResponse(response);
    }

    /// <summary>
    /// Chat with AI about the script
    /// </summary>
    public async Task<string> ChatAsync(
        string userMessage,
        PlaywrightTestSession session,
        Action<ChatMessage>? onChatMessage = null,
        Action<string>? onChunk = null,
        CancellationToken ct = default)
    {
        var systemPrompt = $@"你是資深 Playwright 測試工程師，協助用戶改善測試腳本。

當前測試：{session.TestName}
測試腳本路徑：{session.ScriptPath}

請根據用戶的需求提供建議或修改腳本。如果需要修改程式碼，請輸出完整的 TypeScript 程式碼。";

        var userMsg = new ChatMessage
        {
            Role = "user",
            Content = userMessage,
            Timestamp = DateTime.Now
        };
        session.ChatHistory.Add(userMsg);
        onChatMessage?.Invoke(userMsg);

        // Build conversation context
        var conversationContext = new StringBuilder();
        conversationContext.AppendLine("【對話歷史】");
        foreach (var msg in session.ChatHistory.TakeLast(5))
        {
            conversationContext.AppendLine($"{msg.Role}: {msg.Content}");
        }
        conversationContext.AppendLine($"\n【當前腳本】\n```typescript\n{session.CurrentScript}\n```");
        conversationContext.AppendLine($"\n【用戶訊息】\n{userMessage}");

        var sb = new StringBuilder();
        await _aiService.TransformScriptStreamAsync(
            session.CurrentScript,
            conversationContext.ToString(),
            chunk =>
            {
                sb.Append(chunk);
                onChunk?.Invoke(chunk);
            },
            ct);

        var response = sb.ToString();

        var assistantMsg = new ChatMessage
        {
            Role = "assistant",
            Content = response,
            Timestamp = DateTime.Now
        };
        session.ChatHistory.Add(assistantMsg);
        onChatMessage?.Invoke(assistantMsg);

        return response;
    }

    private static string ExtractCodeFromResponse(string response)
    {
        // Extract code from markdown code blocks
        var lines = response.Split('\n');
        var inCodeBlock = false;
        var sb = new StringBuilder();
        var codeBlockLang = "";

        foreach (var line in lines)
        {
            if (line.TrimStart().StartsWith("```"))
            {
                if (!inCodeBlock)
                {
                    // Start of code block
                    inCodeBlock = true;
                    codeBlockLang = line.Trim().Substring(3).Trim().ToLower();
                }
                else
                {
                    // End of code block
                    inCodeBlock = false;
                }
                continue;
            }

            if (inCodeBlock)
            {
                sb.AppendLine(line);
            }
        }

        var extracted = sb.ToString().Trim();
        return string.IsNullOrWhiteSpace(extracted) ? response : extracted;
    }
}
