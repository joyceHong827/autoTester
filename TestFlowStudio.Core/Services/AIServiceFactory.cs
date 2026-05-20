using TestFlowStudio.Core.Interfaces;
using TestFlowStudio.Core.Models;

namespace TestFlowStudio.Core.Services;

/// <summary>Factory that returns the correct IAIService based on settings.</summary>
public static class AIServiceFactory
{
    public static IAIService Create(AppSettings settings) =>
        settings.AI.Provider switch
        {
            "OpenAI"  => OpenAICompatService.CreateOpenAI(settings),
            "Gemini"  => OpenAICompatService.CreateGemini(settings),
            _         => new ClaudeAIService(settings)   // default: Claude
        };
}
