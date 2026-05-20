using System.Text;
using Newtonsoft.Json;
using TestFlowStudio.Core.Helpers;
using TestFlowStudio.Core.Models;

namespace TestFlowStudio.Core.Helpers;

public static class SettingsManager
{
    private static string _path = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

    public static void SetPath(string path) => _path = path;

    public static AppSettings Load()
    {
        if (!File.Exists(_path)) return new AppSettings();
        var json = File.ReadAllText(_path, Encoding.UTF8);
        return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
    }

    public static void Save(AppSettings settings)
    {
        var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        File.WriteAllText(_path, json, Encoding.UTF8);
    }

    // Convenience wrappers that handle DPAPI transparently
    public static string GetRedmineApiKey(AppSettings s) =>
        EncryptionHelper.Decrypt(s.Redmine.ApiKeyEncrypted);

    public static void SetRedmineApiKey(AppSettings s, string plain) =>
        s.Redmine.ApiKeyEncrypted = EncryptionHelper.Encrypt(plain);

    public static string GetClaudeApiKey(AppSettings s) =>
        EncryptionHelper.Decrypt(s.AI.ClaudeApiKeyEncrypted);

    public static void SetClaudeApiKey(AppSettings s, string plain) =>
        s.AI.ClaudeApiKeyEncrypted = EncryptionHelper.Encrypt(plain);

    public static string GetOpenAIApiKey(AppSettings s) =>
        EncryptionHelper.Decrypt(s.AI.OpenAIApiKeyEncrypted);

    public static void SetOpenAIApiKey(AppSettings s, string plain) =>
        s.AI.OpenAIApiKeyEncrypted = EncryptionHelper.Encrypt(plain);

    public static string GetGeminiApiKey(AppSettings s) =>
        EncryptionHelper.Decrypt(s.AI.GeminiApiKeyEncrypted);

    public static void SetGeminiApiKey(AppSettings s, string plain) =>
        s.AI.GeminiApiKeyEncrypted = EncryptionHelper.Encrypt(plain);
}
