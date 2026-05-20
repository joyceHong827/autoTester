using TestFlowStudio.Core.Helpers;
using TestFlowStudio.Core.Models;

namespace TestFlowStudio.WinForms.Forms;

public partial class SettingsForm : Form
{
    private readonly AppSettings _settings;

    public SettingsForm(AppSettings settings)
    {
        _settings = settings;
        InitializeComponent();
        LoadValues();
    }

    private void LoadValues()
    {
        // Redmine
        txtRedmineUrl.Text     = _settings.Redmine.BaseUrl;
        txtRedmineKey.Text     = SettingsManager.GetRedmineApiKey(_settings);
        txtProjectId.Text      = _settings.Redmine.DefaultProjectId;
        chkIgnoreSsl.Checked   = _settings.Redmine.IgnoreSslErrors;
        nudTimeout.Value       = _settings.Redmine.TimeoutSeconds;

        // AI
        cmbProvider.SelectedItem = _settings.AI.Provider;
        txtClaudeKey.Text        = SettingsManager.GetClaudeApiKey(_settings);
        txtOpenAIKey.Text        = SettingsManager.GetOpenAIApiKey(_settings);
        txtGeminiKey.Text        = SettingsManager.GetGeminiApiKey(_settings);
        txtClaudeModel.Text      = _settings.AI.ClaudeModel;
        txtOpenAIModel.Text      = _settings.AI.OpenAIModel;
        txtGeminiModel.Text      = _settings.AI.GeminiModel;
        nudMaxTokens.Value       = _settings.AI.MaxTokens;

        // Playwright
        txtNodePath.Text            = _settings.Playwright.NodePath;
        chkWriteBack.Checked        = _settings.Playwright.WriteBackToRedmine;

        // Output
        txtTestCasesDir.Text  = _settings.Output.TestCasesDirectory;
        txtScriptsDir.Text    = _settings.Output.ScriptsDirectory;
        nudMaxHistory.Value   = _settings.Output.MaxHistoryRuns;
    }

    private void SaveValues()
    {
        _settings.Redmine.BaseUrl          = txtRedmineUrl.Text.Trim();
        SettingsManager.SetRedmineApiKey(_settings, txtRedmineKey.Text.Trim());
        _settings.Redmine.DefaultProjectId = txtProjectId.Text.Trim();
        _settings.Redmine.IgnoreSslErrors  = chkIgnoreSsl.Checked;
        _settings.Redmine.TimeoutSeconds   = (int)nudTimeout.Value;

        _settings.AI.Provider   = cmbProvider.SelectedItem?.ToString() ?? "Claude";
        SettingsManager.SetClaudeApiKey(_settings, txtClaudeKey.Text.Trim());
        SettingsManager.SetOpenAIApiKey(_settings, txtOpenAIKey.Text.Trim());
        SettingsManager.SetGeminiApiKey(_settings, txtGeminiKey.Text.Trim());
        _settings.AI.ClaudeModel = txtClaudeModel.Text.Trim();
        _settings.AI.OpenAIModel = txtOpenAIModel.Text.Trim();
        _settings.AI.GeminiModel = txtGeminiModel.Text.Trim();
        _settings.AI.MaxTokens   = (int)nudMaxTokens.Value;

        _settings.Playwright.NodePath            = txtNodePath.Text.Trim();
        _settings.Playwright.WriteBackToRedmine  = chkWriteBack.Checked;

        _settings.Output.TestCasesDirectory = txtTestCasesDir.Text.Trim();
        _settings.Output.ScriptsDirectory   = txtScriptsDir.Text.Trim();
        _settings.Output.MaxHistoryRuns     = (int)nudMaxHistory.Value;
    }

    private async void btnTestRedmine_Click(object sender, EventArgs e)
    {
        btnTestRedmine.Enabled = false;
        var tmp = new AppSettings();
        tmp.Redmine.BaseUrl          = txtRedmineUrl.Text.Trim();
        tmp.Redmine.IgnoreSslErrors  = chkIgnoreSsl.Checked;
        SettingsManager.SetRedmineApiKey(tmp, txtRedmineKey.Text.Trim());
        var svc = new Core.Services.RedmineService(tmp);
        var ok  = await svc.ValidateConnectionAsync();
        MessageBox.Show(ok ? "✅ 連線成功！" : "❌ 連線失敗，請確認 URL 與 API Key。",
            "Redmine 連線測試", MessageBoxButtons.OK,
            ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        btnTestRedmine.Enabled = true;
    }

    private async void btnTestAI_Click(object sender, EventArgs e)
    {
        btnTestAI.Enabled = false;
        var tmp = new AppSettings { AI = new AISettings
        {
            Provider      = cmbProvider.SelectedItem?.ToString() ?? "Claude",
            ClaudeModel   = txtClaudeModel.Text.Trim(),
            OpenAIModel   = txtOpenAIModel.Text.Trim(),
            GeminiModel   = txtGeminiModel.Text.Trim(),
        }};
        SettingsManager.SetClaudeApiKey(tmp, txtClaudeKey.Text.Trim());
        SettingsManager.SetOpenAIApiKey(tmp, txtOpenAIKey.Text.Trim());
        SettingsManager.SetGeminiApiKey(tmp, txtGeminiKey.Text.Trim());
        var svc = Core.Services.AIServiceFactory.Create(tmp);
        var ok  = await svc.ValidateConnectionAsync();
        MessageBox.Show(ok ? $"✅ {svc.ProviderName} 連線成功！" : $"❌ {svc.ProviderName} 連線失敗，請確認 API Key 與模型名稱。",
            "AI 連線測試", MessageBoxButtons.OK,
            ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        btnTestAI.Enabled = true;
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        SaveValues();
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
