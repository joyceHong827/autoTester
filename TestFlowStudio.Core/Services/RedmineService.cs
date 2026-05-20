using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestFlowStudio.Core.Interfaces;
using TestFlowStudio.Core.Models;

namespace TestFlowStudio.Core.Services;

public class RedmineService : IRedmineService
{
    private readonly HttpClient _http;
    private readonly AppSettings _settings;

    public RedmineService(AppSettings settings)
    {
        _settings = settings;
        var handler = new HttpClientHandler();
        if (settings.Redmine.IgnoreSslErrors)
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

        _http = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(settings.Redmine.TimeoutSeconds)
        };

        var key = Helpers.SettingsManager.GetRedmineApiKey(settings);
        _http.DefaultRequestHeaders.Add("X-Redmine-API-Key", key);
        _http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private string Base => _settings.Redmine.BaseUrl.TrimEnd('/');

    public async Task<bool> ValidateConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _http.GetAsync($"{Base}/users/current.json", ct);
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<List<RedmineProject>> GetProjectsAsync(CancellationToken ct = default)
    {
        var resp = await _http.GetStringAsync($"{Base}/projects.json?limit=100", ct);
        var obj = JsonConvert.DeserializeObject<RedmineProjectListResponse>(resp);
        return obj?.Projects ?? new();
    }

    public async Task<List<RedmineIssue>> GetIssuesAsync(
        string projectId, string? keyword = null, CancellationToken ct = default)
    {
        // 如果 keyword 是數字，視為 issue ID 直接查詢
        if (!string.IsNullOrWhiteSpace(keyword) && int.TryParse(keyword.Trim(), out var issueId))
        {
            var url = $"{Base}/issues.json?issue_id={issueId}&limit=100";
            var resp = await _http.GetStringAsync(url, ct);
            var obj = JsonConvert.DeserializeObject<RedmineIssueListResponse>(resp);
            return obj?.Issues ?? new();
        }
        
        // 否則使用原本的專案 + 關鍵字查詢
        var searchUrl = $"{Base}/issues.json?project_id={projectId}&status_id=open&limit=100";
        if (!string.IsNullOrWhiteSpace(keyword))
            searchUrl += $"&subject=~{Uri.EscapeDataString(keyword)}";

        var response = await _http.GetStringAsync(searchUrl, ct);
        var result = JsonConvert.DeserializeObject<RedmineIssueListResponse>(response);
        return result?.Issues ?? new();
    }

    public async Task<RedmineIssue> GetIssueAsync(int issueId, CancellationToken ct = default)
    {
        var url = $"{Base}/issues/{issueId}.json?include=journals,attachments";
        var resp = await _http.GetStringAsync(url, ct);
        var obj = JsonConvert.DeserializeObject<RedmineSingleIssueResponse>(resp);
        return obj?.Issue ?? new();
    }

    public async Task<bool> AddJournalNoteAsync(int issueId, string note, CancellationToken ct = default)
    {
        var body = JsonConvert.SerializeObject(new { issue = new { notes = note } });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        var resp = await _http.PutAsync($"{Base}/issues/{issueId}.json", content, ct);
        return resp.IsSuccessStatusCode;
    }
}
