using TestFlowStudio.Core.Helpers;
using TestFlowStudio.Core.Interfaces;
using TestFlowStudio.Core.Models;

namespace TestFlowStudio.Core.Services;

public class ResultWriter
{
    private readonly AppSettings _settings;
    private readonly IRedmineService? _redmine;

    public ResultWriter(AppSettings settings, IRedmineService? redmine = null)
    {
        _settings = settings;
        _redmine  = redmine;
    }

    /// <summary>
    /// Persists a test run result into the .md file and optionally back to Redmine.
    /// </summary>
    public async Task WriteAsync(
        TestCase tc,
        TestRunResult result,
        CancellationToken ct = default)
    {
        // 1. Update .md
        MarkdownHelper.AppendRunResult(tc, result, _settings.Output.MaxHistoryRuns);
        MarkdownHelper.SaveFile(tc);

        // 2. Optionally write back to Redmine
        if (_settings.Playwright.WriteBackToRedmine
            && _redmine != null
            && tc.RedmineIssueId > 0)
        {
            var icon    = result.Status == "passed" ? "✅" : "❌";
            var note    = BuildRedmineNote(tc, result, icon);
            await _redmine.AddJournalNoteAsync(tc.RedmineIssueId, note, ct);
        }
    }

    private static string BuildRedmineNote(TestCase tc, TestRunResult result, string icon)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"*[TestFlow Studio] 自動化測試結果 — {result.RunTime:yyyy-MM-dd HH:mm:ss}*");
        sb.AppendLine();
        sb.AppendLine($"{icon} **{result.Status.ToUpper()}**");
        sb.AppendLine($"- 通過：{result.PassedTests} / 共 {result.TotalTests}");
        if (result.FailureMessages.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("**失敗訊息：**");
            foreach (var m in result.FailureMessages)
                sb.AppendLine($"- {m}");
        }
        return sb.ToString();
    }
}
