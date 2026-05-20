using System.Text;
using Newtonsoft.Json.Linq;
using TestFlowStudio.Core.Models;

namespace TestFlowStudio.Core.Services;

/// <summary>
/// Generate Markdown test result reports with detailed steps and Playwright integration
/// </summary>
public class MarkdownTestReportGenerator
{
    private readonly string _defaultOutputDir = @"D:\autoTester\playwright-tests\test-reports";

    public string GenerateReport(
        string testName,
        List<TestRunResult> results,
        PlaywrightTestSession? session = null)
    {
        var sb = new StringBuilder();
        var latestResult = results.LastOrDefault();

        // Header with visual appeal
        sb.AppendLine("# 🎭 Playwright 測試報告");
        sb.AppendLine();
        sb.AppendLine($"## 測試名稱：{testName}");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"**📅 產生時間**：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"**📂 測試檔案**：{session?.ScriptPath ?? "N/A"}");
        sb.AppendLine();

        // Overall Summary with visual indicators
        if (latestResult != null)
        {
            sb.AppendLine("## 📊 測試摘要");
            sb.AppendLine();

            var passRate = latestResult.TotalTests > 0 
                ? (double)latestResult.PassedTests / latestResult.TotalTests * 100 
                : 0;

            sb.AppendLine("```");
            sb.AppendLine($"  狀態: {GetStatusEmoji(latestResult.Status)} {latestResult.Status.ToUpper()}");
            sb.AppendLine($"  通過率: {passRate:F1}%");
            sb.AppendLine($"  執行時間: {latestResult.RunTime:HH:mm:ss}");
            sb.AppendLine("```");
            sb.AppendLine();

            // Visual metrics
            sb.AppendLine("| 指標 | 數量 | 比例 |");
            sb.AppendLine("|------|------|------|");
            sb.AppendLine($"| ✅ 通過測試 | {latestResult.PassedTests} | {(latestResult.TotalTests > 0 ? (double)latestResult.PassedTests / latestResult.TotalTests * 100 : 0):F1}% |");
            sb.AppendLine($"| ❌ 失敗測試 | {latestResult.FailedTests} | {(latestResult.TotalTests > 0 ? (double)latestResult.FailedTests / latestResult.TotalTests * 100 : 0):F1}% |");
            sb.AppendLine($"| 📋 總測試數 | {latestResult.TotalTests} | 100% |");
            sb.AppendLine($"| 🔄 重試次數 | {latestResult.RetryAttempt} | - |");
            sb.AppendLine();
        }

        // === ✔️ 驗證項目結果 (從測試情境 Markdown 擷取並回填通過/不通過) ===
        if (latestResult != null)
        {
            var scenarioMarkdown = session?.CurrentScript ?? string.Empty;
            var verificationItems = ExtractVerificationItems(scenarioMarkdown, latestResult);
            if (verificationItems.Any())
            {
                sb.AppendLine("## ✔️ 驗證項目結果");
                sb.AppendLine();
                sb.AppendLine("> 以下為測試情境中所定義的驗證項目，並依此次執行結果回填「是否通過」狀態。");
                sb.AppendLine();
                sb.AppendLine("| # | 驗證項目 | 預期結果 | 實際結果 | 是否通過 |");
                sb.AppendLine("|---|---------|---------|---------|----------|");

                for (int i = 0; i < verificationItems.Count; i++)
                {
                    var item = verificationItems[i];
                    var statusIcon = item.Passed ? "✅ 通過" : "❌ 不通過";
                    var name = (item.Name ?? "").Replace("|", "\\|");
                    var expected = string.IsNullOrEmpty(item.Expected) ? "—" : item.Expected.Replace("|", "\\|");
                    var actual = string.IsNullOrEmpty(item.Actual) ? "—" : item.Actual.Replace("|", "\\|");
                    sb.AppendLine($"| {i + 1} | {name} | {expected} | {actual} | {statusIcon} |");
                }
                sb.AppendLine();

                var passedCount = verificationItems.Count(v => v.Passed);
                var failedCount = verificationItems.Count - passedCount;
                sb.AppendLine($"**驗證項目統計**：共 {verificationItems.Count} 項 ｜ ✅ 通過 {passedCount} 項 ｜ ❌ 不通過 {failedCount} 項");
                sb.AppendLine();
                sb.AppendLine("---");
                sb.AppendLine();
            }
        }

        // Test Scenarios - Detailed breakdown
        sb.AppendLine("## 🎯 測試情境案例");
        sb.AppendLine();

        if (latestResult != null)
        {
            var scenarios = ParseTestScenarios(latestResult);

            foreach (var scenario in scenarios)
            {
                sb.AppendLine($"### {scenario.Status} {scenario.Name}");
                sb.AppendLine();

                if (!string.IsNullOrEmpty(scenario.Description))
                {
                    sb.AppendLine($"**描述**：{scenario.Description}");
                    sb.AppendLine();
                }

                if (scenario.Steps.Any())
                {
                    sb.AppendLine("**測試步驟**：");
                    sb.AppendLine();
                    for (int i = 0; i < scenario.Steps.Count; i++)
                    {
                        sb.AppendLine($"{i + 1}. {scenario.Steps[i]}");
                    }
                    sb.AppendLine();
                }

                sb.AppendLine($"**結果**：{scenario.Status} {scenario.Result}");
                sb.AppendLine();

                if (!string.IsNullOrEmpty(scenario.ErrorMessage))
                {
                    sb.AppendLine($"**錯誤訊息**：");
                    sb.AppendLine("```");
                    sb.AppendLine(scenario.ErrorMessage);
                    sb.AppendLine("```");
                    sb.AppendLine();
                }

                if (!string.IsNullOrEmpty(scenario.Duration))
                {
                    sb.AppendLine($"**執行時間**：{scenario.Duration}");
                    sb.AppendLine();
                }

                sb.AppendLine("---");
                sb.AppendLine();
            }
        }

        // Test Execution History
        if (results.Count > 1)
        {
            sb.AppendLine("## 📝 測試執行歷史");
            sb.AppendLine();
            sb.AppendLine("| 執行次數 | 狀態 | 總數 | ✅ 通過 | ❌ 失敗 | ⏱️ 時間 |");
            sb.AppendLine("|---------|------|------|---------|---------|----------|");

            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                sb.AppendLine($"| 第 {i + 1} 次 | {GetStatusEmoji(r.Status)} {r.Status} | {r.TotalTests} | {r.PassedTests} | {r.FailedTests} | {r.RunTime:HH:mm:ss} |");
            }
            sb.AppendLine();
        }

        // Failure Details with categorization
        if (latestResult?.FailureMessages.Any() == true)
        {
            sb.AppendLine("## ❌ 失敗詳情分析");
            sb.AppendLine();

            var categorized = CategorizeFailures(latestResult.FailureMessages);

            foreach (var category in categorized)
            {
                sb.AppendLine($"### {category.Key}");
                sb.AppendLine();
                foreach (var msg in category.Value)
                {
                    sb.AppendLine($"- {msg}");
                }
                sb.AppendLine();
            }
        }

        // Retry History
        if (latestResult?.RetryHistory.Any() == true)
        {
            sb.AppendLine("## 🔄 自動重試歷史");
            sb.AppendLine();
            sb.AppendLine("系統已自動嘗試修正以下問題：");
            sb.AppendLine();

            for (int i = 0; i < latestResult.RetryHistory.Count; i++)
            {
                sb.AppendLine($"**重試 {i + 1}**：{latestResult.RetryHistory[i]}");
                sb.AppendLine();
            }
        }

        // AI Chat History
        if (session?.ChatHistory.Any() == true)
        {
            sb.AppendLine("## 💬 AI 優化對話記錄");
            sb.AppendLine();

            foreach (var msg in session.ChatHistory)
            {
                var icon = msg.Role == "user" ? "👤 使用者" : msg.Role == "assistant" ? "🤖 AI 助手" : "ℹ️ 系統";
                sb.AppendLine($"### {icon}");
                sb.AppendLine($"*{msg.Timestamp:HH:mm:ss}*");
                sb.AppendLine();
                sb.AppendLine("```");
                sb.AppendLine(msg.Content);
                sb.AppendLine("```");
                sb.AppendLine();
            }
        }

        // Playwright HTML Report Link
        sb.AppendLine("## 📊 Playwright 詳細報告");
        sb.AppendLine();
        sb.AppendLine("使用以下命令查看 Playwright 原生 HTML 報告：");
        sb.AppendLine();
        sb.AppendLine("```powershell");
        sb.AppendLine("cd D:\\autoTester\\playwright-tests");
        sb.AppendLine("npx playwright show-report");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("或開啟檔案：`playwright-tests/playwright-report/index.html`");
        sb.AppendLine();

        // Screenshots and Videos
        sb.AppendLine("## 📸 測試附件");
        sb.AppendLine();
        sb.AppendLine("- **截圖位置**：`playwright-tests/test-results/`");
        sb.AppendLine("- **影片位置**：`playwright-tests/test-results/`");
        sb.AppendLine();

        // Raw Output (collapsed for readability)
        if (!string.IsNullOrWhiteSpace(latestResult?.RawOutput))
        {
            sb.AppendLine("<details>");
            sb.AppendLine("<summary>📄 點擊查看完整輸出日誌</summary>");
            sb.AppendLine();
            sb.AppendLine("```");
            sb.AppendLine(latestResult.RawOutput);
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("</details>");
            sb.AppendLine();
        }

        // Footer
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"*本報告由 TestFlow Studio 自動產生於 {DateTime.Now:yyyy-MM-dd HH:mm:ss}*");

        return sb.ToString();
    }

    public async Task<string> SaveReportAsync(
        string testName,
        List<TestRunResult> results,
        string? outputDirectory = null,
        PlaywrightTestSession? session = null,
        CancellationToken ct = default)
    {
        outputDirectory ??= _defaultOutputDir;
        Directory.CreateDirectory(outputDirectory);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"test-report_{SanitizeFileName(testName)}_{timestamp}.md";
        var filePath = Path.Combine(outputDirectory, fileName);

        var report = GenerateReport(testName, results, session);
        await File.WriteAllTextAsync(filePath, report, System.Text.Encoding.UTF8, ct);

        return filePath;
    }

    private static List<TestScenario> ParseTestScenarios(TestRunResult result)
    {
        var scenarios = new List<TestScenario>();

        try
        {
            // Try to parse JSON output for detailed test info
            var jsonStart = result.RawOutput.IndexOf('{');
            if (jsonStart >= 0)
            {
                var json = result.RawOutput.Substring(jsonStart);
                var jobj = JObject.Parse(json);
                var suites = jobj["suites"] as JArray;

                if (suites != null)
                {
                    foreach (var suite in suites)
                    {
                        var specs = suite["specs"] as JArray;
                        if (specs == null) continue;

                        foreach (var spec in specs)
                        {
                            var tests = spec["tests"] as JArray;
                            if (tests == null) continue;

                            foreach (var test in tests)
                            {
                                var scenario = new TestScenario
                                {
                                    Name = spec["title"]?.ToString() ?? "未命名測試",
                                    Description = suite["title"]?.ToString() ?? "",
                                    Result = test["status"]?.ToString() ?? "unknown",
                                    Status = GetStatusEmoji(test["status"]?.ToString() ?? "unknown")
                                };

                                // Extract duration
                                var results = test["results"] as JArray;
                                if (results?.Count > 0)
                                {
                                    var duration = results[0]?["duration"]?.Value<int>() ?? 0;
                                    scenario.Duration = $"{duration / 1000.0:F2} 秒";

                                    // Extract error
                                    var error = results[0]?["error"];
                                    if (error != null)
                                    {
                                        scenario.ErrorMessage = error["message"]?.ToString() ?? "";

                                        // Extract steps from error stack
                                        var stack = error["stack"]?.ToString();
                                        if (!string.IsNullOrEmpty(stack))
                                        {
                                            scenario.Steps = ExtractStepsFromStack(stack);
                                        }
                                    }
                                }

                                scenarios.Add(scenario);
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // Fallback: create basic scenario
            scenarios.Add(new TestScenario
            {
                Name = "測試案例",
                Result = result.Status,
                Status = GetStatusEmoji(result.Status),
                ErrorMessage = string.Join("\n", result.FailureMessages)
            });
        }

        return scenarios;
    }

    private static List<string> ExtractStepsFromStack(string stack)
    {
        var steps = new List<string>();
        var lines = stack.Split('\n');

        foreach (var line in lines.Take(10))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("at ") && trimmed.Contains(".spec.ts:"))
            {
                // Extract meaningful step info
                var match = System.Text.RegularExpressions.Regex.Match(
                    trimmed, @"at.*\.spec\.ts:(\d+):(\d+)");

                if (match.Success)
                {
                    steps.Add($"第 {match.Groups[1].Value} 行：{trimmed}");
                }
            }
        }

        return steps;
    }

    private static Dictionary<string, List<string>> CategorizeFailures(List<string> failures)
    {
        var categorized = new Dictionary<string, List<string>>
        {
            ["⏱️ 超時錯誤"] = new(),
            ["🔍 元素找不到"] = new(),
            ["🌐 網路錯誤"] = new(),
            ["✔️ 驗證失敗"] = new(),
            ["❓ 其他錯誤"] = new()
        };

        foreach (var failure in failures)
        {
            var lower = failure.ToLower();

            if (lower.Contains("timeout") || lower.Contains("超時"))
                categorized["⏱️ 超時錯誤"].Add(failure);
            else if (lower.Contains("not found") || lower.Contains("找不到") || lower.Contains("not visible"))
                categorized["🔍 元素找不到"].Add(failure);
            else if (lower.Contains("network") || lower.Contains("網路") || lower.Contains("connection"))
                categorized["🌐 網路錯誤"].Add(failure);
            else if (lower.Contains("expect") || lower.Contains("assertion") || lower.Contains("驗證"))
                categorized["✔️ 驗證失敗"].Add(failure);
            else
                categorized["❓ 其他錯誤"].Add(failure);
        }

        // Remove empty categories
        return categorized.Where(c => c.Value.Any()).ToDictionary(c => c.Key, c => c.Value);
    }

    private static string GetStatusEmoji(string status) => status.ToLower() switch
    {
        "passed" => "✅",
        "failed" => "❌",
        "error" => "⚠️",
        "skipped" => "⏭️",
        _ => "❓"
    };

    private static string SanitizeFileName(string fileName)
    {
        var invalids = Path.GetInvalidFileNameChars();
        var invalidCharsAndSpace = new List<char>(invalids) { ' ' };
        return string.Join("_", fileName.Split(invalidCharsAndSpace.ToArray(), 
            StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }

    private class TestScenario
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Result { get; set; } = "";
        public string Status { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
        public string Duration { get; set; } = "";
        public List<string> Steps { get; set; } = new();
    }

    private class VerificationItem
    {
        public string Name { get; set; } = "";
        public string Expected { get; set; } = "";
        public string Actual { get; set; } = "";
        public bool Passed { get; set; }
    }

    /// <summary>
    /// 從測試情境 Markdown 內容擷取「驗證項目」並依測試結果回填通過/不通過。
    /// 優先尋找 "## 驗證項目" / "## 驗證點" / "## 預期結果" 區段。
    /// 若情境 Markdown 無相關段落，則退回使用 Playwright 步驟結果。
    /// </summary>
    private static List<VerificationItem> ExtractVerificationItems(string scenarioMarkdown, TestRunResult result)
    {
        var items = new List<VerificationItem>();

        if (!string.IsNullOrWhiteSpace(scenarioMarkdown))
        {
            string[] sectionHeaders = new[]
            {
                "## 驗證項目", "## 驗證點", "## 預期結果", "## 驗證", "## Acceptance Criteria",
                "### 驗證項目", "### 驗證點", "### 預期結果", "### 驗證"
            };

            foreach (var header in sectionHeaders)
            {
                var idx = scenarioMarkdown.IndexOf(header, StringComparison.Ordinal);
                if (idx < 0) continue;

                var startOfBody = scenarioMarkdown.IndexOf('\n', idx);
                if (startOfBody < 0) continue;

                var rest = scenarioMarkdown[(startOfBody + 1)..];
                var endIdx = -1;
                using (var reader = new StringReader(rest))
                {
                    int pos = 0;
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (pos > 0 && line.StartsWith("## "))
                        {
                            endIdx = pos;
                            break;
                        }
                        pos += line.Length + 1;
                    }
                }
                var sectionContent = endIdx > 0 ? rest[..endIdx] : rest;

                foreach (var rawLine in sectionContent.Split('\n'))
                {
                    var line = rawLine.TrimEnd('\r').Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    string? itemText = null;
                    if (line.StartsWith("- ") || line.StartsWith("* "))
                    {
                        itemText = line[2..].Trim();
                    }
                    else if (System.Text.RegularExpressions.Regex.IsMatch(line, @"^\d+\.\s"))
                    {
                        itemText = System.Text.RegularExpressions.Regex.Replace(line, @"^\d+\.\s", "").Trim();
                    }
                    // 跳過 markdown 任務清單前綴 [ ] / [x]
                    if (!string.IsNullOrEmpty(itemText))
                    {
                        itemText = System.Text.RegularExpressions.Regex.Replace(itemText, @"^\[[ xX]\]\s*", "");
                    }

                    if (!string.IsNullOrEmpty(itemText))
                    {
                        items.Add(new VerificationItem
                        {
                            Name = itemText,
                            Expected = itemText,
                            Actual = result.Status == "passed"
                                ? "與預期一致"
                                : "請參考失敗訊息與步驟詳情",
                            Passed = result.Status == "passed"
                        });
                    }
                }

                if (items.Any()) break;
            }
        }

        // Fallback: 使用 Playwright 步驟結果
        if (!items.Any() && result.TestSteps.Any())
        {
            foreach (var step in result.TestSteps)
            {
                items.Add(new VerificationItem
                {
                    Name = step.StepName,
                    Expected = string.IsNullOrEmpty(step.Expected) ? step.StepName : step.Expected,
                    Actual = !string.IsNullOrEmpty(step.Actual)
                        ? step.Actual
                        : (step.Passed ? "符合預期" : (step.ErrorMessage ?? "與預期不符")),
                    Passed = step.Passed
                });
            }
        }

        return items;
    }
}
