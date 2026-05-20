using YamlDotNet.Serialization;

namespace TestFlowStudio.Core.Models;

public class TestCase
{
    // YAML Front Matter
    [YamlMember(Alias = "redmine_issue_id")]
    public int RedmineIssueId { get; set; }

    [YamlMember(Alias = "title")]
    public string Title { get; set; } = "";

    [YamlMember(Alias = "status")]
    public string Status { get; set; } = "pending"; // pending / passed / failed / error

    [YamlMember(Alias = "last_run")]
    public string LastRun { get; set; } = "";

    [YamlMember(Alias = "created_at")]
    public string CreatedAt { get; set; } = "";

    [YamlMember(Alias = "playwright_script")]
    public string PlaywrightScript { get; set; } = "";

    // Runtime only (not stored in YAML)
    [YamlIgnore]
    public string FilePath { get; set; } = "";

    [YamlIgnore]
    public string MarkdownBody { get; set; } = "";

    [YamlIgnore]
    public string FileName => Path.GetFileName(FilePath);
}

public class TestRunResult
{
    public bool Success { get; set; }
    public string Status { get; set; } = "error"; // passed / failed / error
    public DateTime RunTime { get; set; } = DateTime.Now;
    public int TotalTests { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
    public List<string> FailureMessages { get; set; } = new();
    public string RawOutput { get; set; } = "";
    public int RetryAttempt { get; set; } = 0;
    public List<string> RetryHistory { get; set; } = new();
    public string HtmlReportPath { get; set; } = "";  // Playwright HTML 報告路徑

    // 新增：詳細測試步驟資訊
    public List<TestStepResult> TestSteps { get; set; } = new();

    // 新增：測試截圖和附件
    public List<TestAttachment> Attachments { get; set; } = new();

    // 新增：測試持續時間
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;
}

/// <summary>
/// 測試步驟結果
/// </summary>
public class TestStepResult
{
    public string StepName { get; set; } = "";
    public string Action { get; set; } = "";
    public string Expected { get; set; } = "";
    public string Actual { get; set; } = "";
    public bool Passed { get; set; }
    public string ErrorMessage { get; set; } = "";
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;
}

/// <summary>
/// 測試附件（截圖、影片等）
/// </summary>
public class TestAttachment
{
    public string Type { get; set; } = ""; // screenshot / video / trace
    public string FilePath { get; set; } = "";
    public string FileName { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class ChatMessage
{
    public string Role { get; set; } = "user"; // user / assistant / system
    public string Content { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class PlaywrightTestSession
{
    public string TestName { get; set; } = "";
    public string ScriptPath { get; set; } = "";
    public string CurrentScript { get; set; } = "";
    public List<ChatMessage> ChatHistory { get; set; } = new();
    public List<TestRunResult> TestResults { get; set; } = new();
}
