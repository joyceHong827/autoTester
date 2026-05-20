using TestFlowStudio.Core.Models;

namespace TestFlowStudio.Core.Interfaces;

public interface IRedmineService
{
    Task<bool> ValidateConnectionAsync(CancellationToken ct = default);
    Task<List<RedmineProject>> GetProjectsAsync(CancellationToken ct = default);
    Task<List<RedmineIssue>> GetIssuesAsync(string projectId, string? keyword = null, CancellationToken ct = default);
    Task<RedmineIssue> GetIssueAsync(int issueId, CancellationToken ct = default);
    Task<bool> AddJournalNoteAsync(int issueId, string note, CancellationToken ct = default);
}
