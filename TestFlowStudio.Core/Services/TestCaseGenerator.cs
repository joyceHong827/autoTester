using TestFlowStudio.Core.Helpers;
using TestFlowStudio.Core.Interfaces;
using TestFlowStudio.Core.Models;

namespace TestFlowStudio.Core.Services;

public class TestCaseGenerator
{
    private readonly IAIService _ai;
    private readonly AppSettings _settings;

    public TestCaseGenerator(IAIService ai, AppSettings settings)
    {
        _ai = ai;
        _settings = settings;
    }

    /// <summary>
    /// Generate a .md TestCase from a Redmine Issue and save it to disk.
    /// Returns the saved TestCase (with FilePath populated).
    /// </summary>
    public async Task<TestCase> GenerateAndSaveAsync(
        RedmineIssue issue,
        Action<string>? onChunk = null,
        CancellationToken ct = default)
    {
        var dir = _settings.Output.TestCasesDirectory;
        Directory.CreateDirectory(dir);

        // Build markdown body via streaming
        var body = new System.Text.StringBuilder();
        Action<string> handler = chunk =>
        {
            body.Append(chunk);
            onChunk?.Invoke(chunk);
        };

        await _ai.GenerateTestCaseStreamAsync(issue.Subject, issue.Description, handler, ct);

        // Compose the full TestCase object
        var now = DateTimeOffset.Now;
        var tc = new TestCase
        {
            RedmineIssueId = issue.Id,
            Title          = issue.Subject,
            Status         = "pending",
            CreatedAt      = now.ToString("o"),
            LastRun        = "",
            MarkdownBody   = body.ToString().Trim()
        };

        // Build a safe filename:  TC-1234_subject.md
        var safeName = SanitizeFileName($"TC-{issue.Id}_{issue.Subject}");
        tc.FilePath = Path.Combine(dir, safeName + ".md");
        tc.PlaywrightScript = Path.Combine(_settings.Playwright.TestsOutputDirectory, safeName + ".spec.ts");

        MarkdownHelper.SaveFile(tc);
        return tc;
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name.Length > 80 ? name[..80] : name;
    }
}
