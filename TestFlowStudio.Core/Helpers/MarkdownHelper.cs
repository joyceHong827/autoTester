using System.Text;
using TestFlowStudio.Core.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TestFlowStudio.Core.Helpers;

public static class MarkdownHelper
{
    private static readonly ISerializer _serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();

    private static readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>Parse a .md file into a TestCase object.</summary>
    public static TestCase ParseFile(string filePath)
    {
        var raw = File.ReadAllText(filePath, Encoding.UTF8);
        var tc = ParseContent(raw);
        tc.FilePath = filePath;
        return tc;
    }

    public static TestCase ParseContent(string content)
    {
        var tc = new TestCase();
        if (content.StartsWith("---"))
        {
            var end = content.IndexOf("---", 3);
            if (end > 0)
            {
                var yaml = content[3..end].Trim();
                var body = content[(end + 3)..].Trim();
                try { tc = _deserializer.Deserialize<TestCase>(yaml); } catch { }
                tc.MarkdownBody = body;
                return tc;
            }
        }
        tc.MarkdownBody = content;
        return tc;
    }

    /// <summary>Serialise TestCase back to .md file.</summary>
    public static void SaveFile(TestCase tc)
    {
        var yaml = _serializer.Serialize(tc);
        var sb = new StringBuilder();
        sb.AppendLine("---");
        sb.Append(yaml);
        sb.AppendLine("---");
        sb.AppendLine();
        sb.Append(tc.MarkdownBody);
        File.WriteAllText(tc.FilePath, sb.ToString(), Encoding.UTF8);
    }

    /// <summary>
    /// Append a test-run result block under the "## 測試結果" section.
    /// 僅寫入：測試步驟、最終結果（通過/不通過）、最終測試結論建議。
    /// </summary>
    public static void AppendRunResult(TestCase tc, TestRunResult result, int maxHistory)
    {
        const string section = "## 測試結果";
        var sb = new StringBuilder();

        // === 測試執行標題 ===
        sb.AppendLine($"### Run {result.RunTime:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        // === 1. 📋 測試步驟 ===
        sb.AppendLine("#### 📋 測試步驟");
        sb.AppendLine();

        // 優先使用 Playwright JSON 解析出的詳細步驟
        var steps = result.TestSteps.ToList();

        // 若沒有，退回從 RawOutput（list reporter）解析
        if (!steps.Any())
        {
            steps = ExtractStepsFromRawOutput(result.RawOutput, result.Status == "passed");
        }

        if (steps.Any())
        {
            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                var stepIcon = step.Passed ? "✅" : "❌";
                var actionTag = string.IsNullOrEmpty(step.Action) ? "" : $"【{step.Action}】";
                var durTag = step.Duration > TimeSpan.Zero ? $"（{step.Duration.TotalMilliseconds:F0} 毫秒）" : "";
                sb.AppendLine($"{i + 1}. {stepIcon} {actionTag}{step.StepName}{durTag}");

                if (!step.Passed && !string.IsNullOrEmpty(step.ErrorMessage))
                {
                    var msg = step.ErrorMessage.Length > 300 ? step.ErrorMessage[..300] + "..." : step.ErrorMessage;
                    sb.AppendLine($"   - ⚠️ 錯誤訊息：{msg.Replace("\n", " ")}");
                }
            }
            sb.AppendLine();
        }
        else
        {
            // 最後退回測試案例 Markdown 中的步驟段
            var scenarioSteps = ExtractScenarioSteps(tc.MarkdownBody);
            if (scenarioSteps.Any())
            {
                for (int i = 0; i < scenarioSteps.Count; i++)
                {
                    var stepIcon = result.Status == "passed" ? "✅" : "—";
                    sb.AppendLine($"{i + 1}. {stepIcon} {scenarioSteps[i]}");
                }
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("> （此次執行未取得詳細步驟資訊，請建議於測試腳本中以「測試步驟」函式包裝動作，或於測試案例的「## 測試步驟」段落列出步驟。）");
                sb.AppendLine();
            }
        }

        // === 2. ✔️ 驗證項目（含「是否通過」欄位）===
        var verificationItems = ExtractVerificationItems(tc.MarkdownBody, result);
        if (verificationItems.Any())
        {
            sb.AppendLine("#### ✔️ 驗證項目");
            sb.AppendLine();
            sb.AppendLine("| # | 驗證項目 | 是否通過 |");
            sb.AppendLine("|---|---------|---------|");
            for (int i = 0; i < verificationItems.Count; i++)
            {
                var item = verificationItems[i];
                var statusIcon = item.Passed ? "✅ 通過" : "❌ 不通過";
                var name = item.Name.Replace("|", "\\|");
                sb.AppendLine($"| {i + 1} | {name} | {statusIcon} |");
            }
            sb.AppendLine();
        }

        // === 3. 🎯 最終結果（通過/不通過）===
        var resultIcon = result.Status == "passed" ? "✅" : "❌";
        var resultText = result.Status == "passed" ? "通過" : "不通過";
        sb.AppendLine("#### 🎯 最終結果");
        sb.AppendLine();
        sb.AppendLine($"**{resultIcon} {resultText}**");
        sb.AppendLine();

        // === 4. 💡 最終測試結論建議 ===
        sb.AppendLine("#### 💡 最終測試結論建議");
        sb.AppendLine();
        sb.AppendLine(BuildFinalConclusion(result));
        sb.AppendLine();

        sb.AppendLine("---");
        sb.AppendLine();

        // Find or create section
        var body = tc.MarkdownBody;
        var idx = body.IndexOf(section, StringComparison.Ordinal);
        if (idx < 0)
        {
            body += $"\n\n{section}\n\n{sb}";
        }
        else
        {
            var insertAt = idx + section.Length;
            body = body.Insert(insertAt, $"\n\n{sb}");
        }

        // Trim old runs beyond maxHistory
        body = TrimOldRuns(body, section, maxHistory);

        tc.MarkdownBody = body;
        tc.Status = result.Status;
        tc.LastRun = result.RunTime.ToString("o");
    }

    private static string TrimOldRuns(string body, string section, int max)
    {
        var sectionIdx = body.IndexOf(section, StringComparison.Ordinal);
        if (sectionIdx < 0) return body;
        var afterSection = body[(sectionIdx + section.Length)..];
        var runs = afterSection.Split(new[] { "### Run " }, StringSplitOptions.None).ToList();
        // runs[0] is text before first ### Run
        if (runs.Count - 1 > max)
        {
            // Keep only last `max` runs
            var keep = runs.Take(1).Concat(runs.Skip(runs.Count - max)).ToList();
            afterSection = string.Join("### Run ", keep);
        }
        return body[..(sectionIdx + section.Length)] + afterSection;
    }

    /// <summary>Load all .md files from a directory.</summary>
    public static List<TestCase> LoadAll(string directory)
    {
        if (!Directory.Exists(directory)) return new();
        return Directory.GetFiles(directory, "*.md", SearchOption.TopDirectoryOnly)
            .Select(ParseFile)
            .ToList();
    }

    /// <summary>驗證項目（含通過/不通過狀態）。</summary>
    private class VerificationItem
    {
        public string Name { get; set; } = string.Empty;
        public bool Passed { get; set; }
    }

    /// <summary>
    /// 從測試案例 Markdown 擷取「驗證項目」條目（## 驗證項目 / ## 驗證點 / ## 預期結果 等），
    /// 並依測試結果回填通過/不通過。
    /// 若整體測試通過則所有項目標為通過；否則標為不通過。
    /// 若情境 Markdown 無相關段落，則退回使用 Playwright 步驟結果。
    /// </summary>
    private static List<VerificationItem> ExtractVerificationItems(string markdownBody, TestRunResult result)
    {
        var items = new List<VerificationItem>();
        if (string.IsNullOrWhiteSpace(markdownBody)) return items;

        string[] sectionHeaders = new[]
        {
            "## 驗證項目", "## 驗證點", "## 預期結果", "## 驗證", "## Acceptance Criteria",
            "### 驗證項目", "### 驗證點", "### 預期結果", "### 驗證",
            "##驗證項目", "##驗證點", "##預期結果", "##驗證"
        };

        foreach (var header in sectionHeaders)
        {
            var idx = markdownBody.IndexOf(header, StringComparison.Ordinal);
            if (idx < 0) continue;

            var startOfBody = markdownBody.IndexOf('\n', idx);
            if (startOfBody < 0) continue;

            var rest = markdownBody[(startOfBody + 1)..];
            var endIdx = -1;
            using (var reader = new StringReader(rest))
            {
                int pos = 0;
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (pos > 0 && (line.StartsWith("## ") || (line.StartsWith("##") && line.Length > 2 && line[2] != '#')))
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
                bool? itemPassed = null;

                // 解析 Markdown 表格行（跳過表頭分隔行 |---|）
                if (line.StartsWith("|") && !line.Contains("---"))
                {
                    var cols = line.Split('|', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(c => c.Trim()).ToArray();
                    if (cols.Length >= 2)
                    {
                        // 若第一欄是純數字（項次欄），使用第二欄作為項目名稱
                        var nameCol = System.Text.RegularExpressions.Regex.IsMatch(cols[0], @"^\d+$")
                            ? (cols.Length > 1 ? cols[1] : cols[0])
                            : cols[0];
                        // 跳過表頭行（第一欄通常為「驗證項目」等標題文字）
                        if (!nameCol.Equals("驗證項目", StringComparison.Ordinal)
                            && !nameCol.Equals("項次", StringComparison.Ordinal)
                            && !nameCol.Equals("#", StringComparison.Ordinal)
                            && !string.IsNullOrWhiteSpace(nameCol))
                        {
                            itemText = nameCol;
                            // 從最後一欄判斷通過狀態
                            var lastCol = cols[^1];
                            if (lastCol.Contains("✅") || lastCol.Contains("通過"))
                                itemPassed = true;
                            else if (lastCol.Contains("❌") || lastCol.Contains("不通過") || lastCol.Contains("失敗"))
                                itemPassed = false;
                        }
                    }
                }
                else if (line.StartsWith("- ") || line.StartsWith("* "))
                {
                    itemText = line[2..].Trim();
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(line, @"^\d+\.\s"))
                {
                    itemText = System.Text.RegularExpressions.Regex.Replace(line, @"^\d+\.\s", "").Trim();
                }
                if (!string.IsNullOrEmpty(itemText) && itemPassed == null)
                {
                    itemText = System.Text.RegularExpressions.Regex.Replace(itemText, @"^\[[ xX]\]\s*", "");
                }

                if (!string.IsNullOrEmpty(itemText))
                {
                    items.Add(new VerificationItem
                    {
                        Name = itemText,
                        Passed = itemPassed ?? (result.Status == "passed")
                    });
                }
            }

            if (items.Any()) break;
        }

        // Fallback：用 Playwright 步驟結果作為驗證項目
        if (!items.Any() && result.TestSteps.Any())
        {
            foreach (var step in result.TestSteps)
            {
                items.Add(new VerificationItem
                {
                    Name = step.StepName,
                    Passed = step.Passed
                });
            }
        }

        return items;
    }

    /// <summary>
    /// 從 Playwright list reporter 的純文字輸出中嘗試解析步驟。
    /// 範例輸入行：
    ///   "  ✓  1 [chromium] › tests/foo.spec.ts:5:7 › 登入流程 (1.2s)"
    ///   "  ✘  2 [chromium] › tests/foo.spec.ts:9:7 › 商品查詢 (3.5s)"
    /// </summary>
    private static List<TestStepResult> ExtractStepsFromRawOutput(string rawOutput, bool overallPassed)
    {
        var steps = new List<TestStepResult>();
        if (string.IsNullOrWhiteSpace(rawOutput)) return steps;

        var pattern = new System.Text.RegularExpressions.Regex(
            @"^\s*(?<icon>[✓✘×✗○⊘\-])\s+\d+\s+\[[^\]]+\]\s*[›>]\s*(?<rest>.+?)(?:\s*\((?<dur>[\d.]+m?s)\))?\s*$",
            System.Text.RegularExpressions.RegexOptions.Multiline);

        foreach (System.Text.RegularExpressions.Match m in pattern.Matches(rawOutput))
        {
            var icon = m.Groups["icon"].Value;
            var rest = m.Groups["rest"].Value.Trim();
            var dur = m.Groups["dur"].Value;

            // rest 通常為「{相對路徑.spec.ts:行:列} › 測試標題」，取最後一段為標題
            var parts = rest.Split('›', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();
            var title = parts.LastOrDefault() ?? rest;

            // 把 .spec.ts:line:col 從標題中濾掉
            title = System.Text.RegularExpressions.Regex.Replace(title, @"^.+?\.spec\.ts:\d+:\d+\s*", "");

            var passed = icon == "✓";
            var duration = ParseDuration(dur);

            steps.Add(new TestStepResult
            {
                StepName = title,
                Action = "測試案例",
                Passed = passed,
                Duration = duration,
                Actual = passed ? "符合預期" : "未符合預期"
            });
        }

        return steps;
    }

    private static TimeSpan ParseDuration(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return TimeSpan.Zero;
        try
        {
            if (text.EndsWith("ms"))
            {
                return TimeSpan.FromMilliseconds(double.Parse(text[..^2], System.Globalization.CultureInfo.InvariantCulture));
            }
            if (text.EndsWith("s"))
            {
                return TimeSpan.FromSeconds(double.Parse(text[..^1], System.Globalization.CultureInfo.InvariantCulture));
            }
        }
        catch { }
        return TimeSpan.Zero;
    }

    /// <summary>
    /// 從測試案例 Markdown 中提取「測試步驟」段落（## 測試步驟 / ## 測試流程 等）。
    /// </summary>
    private static List<string> ExtractScenarioSteps(string markdownBody)
    {
        var steps = new List<string>();
        if (string.IsNullOrWhiteSpace(markdownBody)) return steps;

        string[] sectionHeaders = new[]
        {
            "## 測試步驟", "## 測試流程", "## 操作步驟", "## Steps",
            "### 測試步驟", "### 測試流程", "### 操作步驟",
            "##測試步驟", "##測試流程", "##操作步驟"
        };

        foreach (var header in sectionHeaders)
        {
            var idx = markdownBody.IndexOf(header, StringComparison.Ordinal);
            if (idx < 0) continue;

            var startOfBody = markdownBody.IndexOf('\n', idx);
            if (startOfBody < 0) continue;

            var rest = markdownBody[(startOfBody + 1)..];
            var endIdx = -1;
            using (var reader = new StringReader(rest))
            {
                int pos = 0;
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (pos > 0 && (line.StartsWith("## ") || (line.StartsWith("##") && line.Length > 2 && line[2] != '#')))
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
                if (line.StartsWith("- ") || line.StartsWith("* "))
                {
                    steps.Add(line[2..].Trim());
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(line, @"^\d+\.\s"))
                {
                    steps.Add(System.Text.RegularExpressions.Regex.Replace(line, @"^\d+\.\s", "").Trim());
                }
            }

            if (steps.Any()) break;
        }

        return steps;
    }

    /// <summary>
    /// 根據測試結果產生最終測試結論建議文字。
    /// </summary>
    private static string BuildFinalConclusion(TestRunResult result)
    {
        var sb = new StringBuilder();
        var passed = result.Status == "passed";

        if (passed)
        {
            sb.AppendLine("> ✅ 本次測試 **通過**，功能行為符合預期需求。");
            sb.AppendLine(">");
            sb.AppendLine("> 📌 **建議**：可進入下一階段（程式碼審查 / 整合測試 / 部署），並持續以此案例做迴歸驗證。");
        }
        else
        {
            sb.AppendLine("> ❌ 本次測試 **不通過**。");
            if (result.FailureMessages.Any())
            {
                sb.AppendLine(">");
                sb.AppendLine("> **主要失敗原因**：");
                foreach (var msg in result.FailureMessages.Take(3))
                {
                    var trimmed = msg.Length > 200 ? msg[..200] + "..." : msg;
                    sb.AppendLine($"> - {trimmed}");
                }
            }
            sb.AppendLine(">");
            sb.AppendLine("> 📌 **建議**：");
            sb.AppendLine("> 1. 檢視上述失敗訊息與失敗步驟，確認是測試腳本問題或實際缺陷。");
            sb.AppendLine("> 2. 若為測試腳本問題（選擇器/等待時序），修正腳本後重新執行。");
            sb.AppendLine("> 3. 若為產品缺陷，建議於 Redmine 開立 Issue 並指派處理。");
            sb.AppendLine("> 4. 修復後重新執行此測試案例驗證。");
        }

        return sb.ToString().TrimEnd();
    }
}
