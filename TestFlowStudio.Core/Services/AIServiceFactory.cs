using TestFlowStudio.Core.Interfaces;
using TestFlowStudio.Core.Models;

namespace TestFlowStudio.Core.Services;

/// <summary>Factory that returns the correct IAIService based on settings.</summary>
public static class AIServiceFactory
{
    /// <summary>全域 AI（由 AI.Provider 決定）</summary>
    public static IAIService Create(AppSettings settings) =>
        CreateByProvider(settings, settings.AI.Provider, null);

    /// <summary>
    /// 依任務指定的 Provider / Model 建立 AI。
    /// 若任務未指定（空字串），則退回全域設定。
    /// </summary>
    public static IAIService CreateForTask(AppSettings settings, AITask task)
    {
        var (provider, model) = task switch
        {
            AITask.TestCase => (settings.AITask.TestCaseProvider, settings.AITask.TestCaseModel),
            AITask.Script   => (settings.AITask.ScriptProvider,   settings.AITask.ScriptModel),
            AITask.Result   => (settings.AITask.ResultProvider,   settings.AITask.ResultModel),
            _               => ("", "")
        };

        // 空白代表沿用全域設定
        if (string.IsNullOrWhiteSpace(provider))
            return Create(settings);

        return CreateByProvider(settings, provider, model);
    }

    private static IAIService CreateByProvider(AppSettings settings, string provider, string? overrideModel)
    {
        return provider switch
        {
            "OpenAI"  => OpenAICompatService.CreateOpenAI(settings, overrideModel),
            "Gemini"  => OpenAICompatService.CreateGemini(settings, overrideModel),
            "Ollama"  => OpenAICompatService.CreateOllama(settings, overrideModel),
            _         => new ClaudeAIService(settings, overrideModel)   // Claude / default
        };
    }
}

/// <summary>任務種類列舉</summary>
public enum AITask
{
    TestCase,   // 生成測試案例
    Script,     // 撰寫 Playwright 腳本
    Result      // 執行結果寫回
}
