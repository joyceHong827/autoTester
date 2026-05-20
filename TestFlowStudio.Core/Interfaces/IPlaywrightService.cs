using TestFlowStudio.Core.Models;

namespace TestFlowStudio.Core.Interfaces;

public interface IPlaywrightService
{
    Task<(bool nodeOk, string nodeVersion, bool playwrightOk)> CheckEnvironmentAsync();
    Task InstallPlaywrightAsync(Action<string> onOutput);
    Task<string> RecordAsync(string url, CancellationToken ct = default);
    void StopRecording();
    Task<TestRunResult> RunTestAsync(
        string specFilePath,
        Action<string>? onOutput = null,
        CancellationToken ct = default);
    Task<string> SaveTestScriptAsync(
        string testName,
        string script,
        CancellationToken ct = default);
}
