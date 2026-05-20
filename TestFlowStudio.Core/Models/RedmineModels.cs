using Newtonsoft.Json;

namespace TestFlowStudio.Core.Models;

public class RedmineIssue
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("subject")]
    public string Subject { get; set; } = "";

    [JsonProperty("description")]
    public string Description { get; set; } = "";

    [JsonProperty("status")]
    public RedmineIdName? Status { get; set; }

    [JsonProperty("priority")]
    public RedmineIdName? Priority { get; set; }

    [JsonProperty("assigned_to")]
    public RedmineIdName? AssignedTo { get; set; }

    [JsonProperty("author")]
    public RedmineIdName? Author { get; set; }

    [JsonProperty("project")]
    public RedmineIdName? Project { get; set; }

    [JsonProperty("created_on")]
    public DateTime? CreatedOn { get; set; }

    [JsonProperty("updated_on")]
    public DateTime? UpdatedOn { get; set; }

    [JsonProperty("custom_fields")]
    public List<RedmineCustomField> CustomFields { get; set; } = new();

    [JsonProperty("journals")]
    public List<RedmineJournal> Journals { get; set; } = new();

    public override string ToString() => $"#{Id} {Subject}";
}

public class RedmineIdName
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = "";
}

public class RedmineCustomField
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = "";

    [JsonProperty("value")]
    public string Value { get; set; } = "";
}

public class RedmineJournal
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("notes")]
    public string Notes { get; set; } = "";

    [JsonProperty("created_on")]
    public DateTime? CreatedOn { get; set; }

    [JsonProperty("user")]
    public RedmineIdName? User { get; set; }
}

public class RedmineProject
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = "";

    [JsonProperty("identifier")]
    public string Identifier { get; set; } = "";

    public override string ToString() => Name;
}

public class RedmineIssueListResponse
{
    [JsonProperty("issues")]
    public List<RedmineIssue> Issues { get; set; } = new();

    [JsonProperty("total_count")]
    public int TotalCount { get; set; }
}

public class RedmineProjectListResponse
{
    [JsonProperty("projects")]
    public List<RedmineProject> Projects { get; set; } = new();
}

public class RedmineSingleIssueResponse
{
    [JsonProperty("issue")]
    public RedmineIssue Issue { get; set; } = new();
}
