namespace TestFlowStudio.Core.Models;

public class AppSettings
{
    public RedmineSettings Redmine { get; set; } = new();
    public AISettings AI { get; set; } = new();
    public AITaskSettings AITask { get; set; } = new();
    public PlaywrightSettings Playwright { get; set; } = new();
    public OutputSettings Output { get; set; } = new();
}

public class RedmineSettings
{
    public string BaseUrl { get; set; } = "";
    public string ApiKeyEncrypted { get; set; } = "";
    public string DefaultProjectId { get; set; } = "";
    public bool IgnoreSslErrors { get; set; } = false;
    public int TimeoutSeconds { get; set; } = 30;
}

public class AISettings
{
    // Claude / OpenAI / Gemini / Ollama
    public string Provider { get; set; } = "Claude";
    public string ClaudeApiKeyEncrypted { get; set; } = "";
    public string OpenAIApiKeyEncrypted { get; set; } = "";
    public string GeminiApiKeyEncrypted { get; set; } = "";
    public string ClaudeModel { get; set; } = "claude-sonnet-4-5";
    public string OpenAIModel { get; set; } = "gpt-4o";
    public string GeminiModel { get; set; } = "gemini-2.0-flash";
    public int MaxTokens { get; set; } = 4096;
    public string CustomSystemPrompt { get; set; } = "";

    // Ollama 本地端 AI
    public string OllamaBaseUrl { get; set; } = "http://localhost:11434";
    public string OllamaModel { get; set; } = "qwen2.5-coder:7b";
}

public class PlaywrightSettings
{
    public string NodePath { get; set; } = "node";
    public bool WriteBackToRedmine { get; set; } = false;
    public bool UsePOM { get; set; } = false;
    public int MaxRetryAttempts { get; set; } = 3;
    public bool HeadedMode { get; set; } = true;
    public string TestsOutputDirectory { get; set; } = @"D:\autoTester\playwright-tests\tests";
    public string PlaywrightTestsDirectory { get; set; } = @"D:\autoTester\playwright-tests";
}

public class OutputSettings
{
    public string TestCasesDirectory { get; set; } = @"D:\autoTester\playwright-tests\TestCase";
    public string ScriptsDirectory { get; set; } = @"D:\autoTester\playwright-tests\tests";
    public int MaxHistoryRuns { get; set; } = 10;
}

/// <summary>
/// 每個任務可獨立指定 AI Provider 與 Model，
/// 空白表示沿用全域 AI 設定。
/// </summary>
public class AITaskSettings
{
    // 任務一：生成測試案例
    public string TestCaseProvider { get; set; } = "";
    public string TestCaseModel    { get; set; } = "";

    // 任務二：撰寫 Playwright 腳本
    public string ScriptProvider { get; set; } = "";
    public string ScriptModel    { get; set; } = "";

    // 任務三：執行結果寫回
    public string ResultProvider { get; set; } = "";
    public string ResultModel    { get; set; } = "";
}
