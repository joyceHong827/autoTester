using TestFlowStudio.Core.Models;

namespace TestFlowStudio.Core.Interfaces;

public interface IAIService
{
    string ProviderName { get; }

    Task<string> GenerateTestCaseAsync(
        string issueTitle,
        string issueBody,
        CancellationToken ct = default);

    Task<string> TransformScriptAsync(
        string codegenScript,
        string testCaseMd,
        CancellationToken ct = default);

    // Streaming variant — calls onChunk for each token received
    Task GenerateTestCaseStreamAsync(
        string issueTitle,
        string issueBody,
        Action<string> onChunk,
        CancellationToken ct = default);

    Task TransformScriptStreamAsync(
        string codegenScript,
        string testCaseMd,
        Action<string> onChunk,
        CancellationToken ct = default);

    Task<bool> ValidateConnectionAsync(CancellationToken ct = default);
}
