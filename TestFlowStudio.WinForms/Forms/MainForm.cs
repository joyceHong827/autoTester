using TestFlowStudio.Core.Helpers;
using TestFlowStudio.Core.Models;
using TestFlowStudio.Core.Services;

namespace TestFlowStudio.WinForms.Forms;

public partial class MainForm : Form
{
    private AppSettings _settings = new();
    private Core.Interfaces.IAIService? _ai;
    private RedmineService? _redmine;
    private PlaywrightService? _playwright;
    private CancellationTokenSource _cts = new();

    // Assertion rules defined by the user for the current recording session
    private List<AssertionRule> _assertionRules = new();

    public MainForm()
    {
        InitializeComponent();
        LoadSettings();
        SetupStatusBar();
        Shown += (_, _) =>
        {
            try
            {
                splitRunner.Panel1MinSize = 200;
                splitRunner.Panel2MinSize = 200;
                splitRunner.SplitterDistance = splitRunner.Width / 2;
            }
            catch { /* 視窗尺寸不足時忽略 */ }
        };
        _ = CheckEnvironmentAsync();
    }

    // ── Settings ──────────────────────────────────────────────────────────

    private void LoadSettings()
    {
        _settings   = SettingsManager.Load();
        _ai         = AIServiceFactory.Create(_settings);
        _redmine    = new RedmineService(_settings);
        _playwright = new PlaywrightService(_settings);
    }

    // ── Status bar ────────────────────────────────────────────────────────

    private void SetupStatusBar()
    {
        tsProvider.Text = $"AI: {_settings.AI.Provider}";
        tsRedmine.Text  = "Redmine: 未連線";
        tsNode.Text     = "Node.js: 檢查中...";
    }

    private async Task CheckEnvironmentAsync()
    {
        var (nodeOk, nodeVer, pwOk) = await _playwright!.CheckEnvironmentAsync();
        Invoke(() =>
        {
            tsNode.Text = nodeOk ? $"Node.js {nodeVer}" : "Node.js: 未安裝 ⚠";
            if (!pwOk) tsNode.Text += "  Playwright: 未安裝 ⚠";
        });

        if (!string.IsNullOrWhiteSpace(_settings.Redmine.BaseUrl))
        {
            var ok = await _redmine!.ValidateConnectionAsync();
            Invoke(() => tsRedmine.Text = ok ? "Redmine: ✅ 已連線" : "Redmine: ❌ 連線失敗");
        }
    }

    // ── Tab: Redmine Issues ───────────────────────────────────────────────

    private async void btnLoadProjects_Click(object sender, EventArgs e)
    {
        try
        {
            SetBusy(true);
            var projects = await _redmine!.GetProjectsAsync();
            cmbProject.Items.Clear();
            foreach (var p in projects) cmbProject.Items.Add(p);
            if (cmbProject.Items.Count > 0) cmbProject.SelectedIndex = 0;
        }
        catch (Exception ex) { ShowError(ex); }
        finally { SetBusy(false); }
    }

    private async void btnSearchIssues_Click(object sender, EventArgs e)
    {
        if (cmbProject.SelectedItem is not Core.Models.RedmineProject proj) return;
        try
        {
            SetBusy(true);
            var issues = await _redmine!.GetIssuesAsync(
                proj.Id.ToString(), txtIssueSearch.Text.Trim());
            dgvIssues.DataSource = issues.Select(i => new
            {
                i.Id,
                i.Subject,
                Status   = i.Status?.Name   ?? "",
                Assignee = i.AssignedTo?.Name ?? ""
            }).ToList();
        }
        catch (Exception ex) { ShowError(ex); }
        finally { SetBusy(false); }
    }

    private async void btnGenerateTestCase_Click(object sender, EventArgs e)
    {
        if (dgvIssues.CurrentRow == null) return;
        var id = (int)dgvIssues.CurrentRow.Cells["Id"].Value;
        try
        {
            SetBusy(true);
            _cts = new CancellationTokenSource();
            rtbIssuePreview.Clear();

            var issue = await _redmine!.GetIssueAsync(id, _cts.Token);
            var gen   = new TestCaseGenerator(_ai!, _settings);
            var tc    = await gen.GenerateAndSaveAsync(
                issue,
                chunk => SafeAppend(rtbIssuePreview, chunk),
                _cts.Token);

            MessageBox.Show($"測試案例已儲存：\n{tc.FilePath}",
                "生成完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadTestCaseList();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { ShowError(ex); }
        finally { SetBusy(false); }
    }

    // ── Tab: Test Cases ───────────────────────────────────────────────────

    private void LoadTestCaseList()
    {
        var cases  = MarkdownHelper.LoadAll(_settings.Output.TestCasesDirectory);
        var filter = cmbStatusFilter.SelectedItem?.ToString() ?? "全部";
        if (filter != "全部") cases = cases.Where(c => c.Status == filter).ToList();

        lvTestCases.Items.Clear();
        foreach (var tc in cases)
        {
            var item = new ListViewItem(tc.FileName);
            item.SubItems.Add(tc.Title);
            item.SubItems.Add(tc.Status);
            item.SubItems.Add(tc.LastRun);
            item.Tag      = tc;
            item.ForeColor = tc.Status switch
            {
                "passed" => Color.Green,
                "failed" => Color.Red,
                "error"  => Color.OrangeRed,
                _        => SystemColors.WindowText
            };
            lvTestCases.Items.Add(item);
        }

        // 同步更新 Dashboard 統計
        var all = MarkdownHelper.LoadAll(_settings.Output.TestCasesDirectory);
        lblTotalCases.Text = $"總案例\n{all.Count}";
        lblPassed.Text     = $"已通過\n{all.Count(c => c.Status == "passed")}";
        lblFailed.Text     = $"失敗\n{all.Count(c => c.Status is "failed" or "error")}";
        lblPending.Text    = $"待執行\n{all.Count(c => c.Status == "pending")}";
    }

    private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (tabMain.SelectedTab == tabTestCases) LoadTestCaseList();
        if (tabMain.SelectedTab == tabRunner) RefreshTestScenarios();
    }

    private void cmbStatusFilter_SelectedIndexChanged(object sender, EventArgs e) =>
        LoadTestCaseList();

    // ── Tab: Recording Studio ─────────────────────────────────────────────

    private void btnEditAssertions_Click(object sender, EventArgs e)
    {
        using var form = new AssertionEditorForm(_assertionRules);
        if (form.ShowDialog() == DialogResult.OK)
        {
            _assertionRules = form.Rules;
            UpdateAssertionBadge();
        }
    }

    private void UpdateAssertionBadge()
    {
        var active = _assertionRules.Count(r => r.Enabled);
        btnEditAssertions.Text = active > 0
            ? $"✅ 驗證條件 ({active})"
            : "⚙ 驗證條件";
        btnEditAssertions.BackColor = active > 0
            ? Color.FromArgb(30, 120, 60)
            : Color.FromArgb(80, 80, 80);
    }

    private async void btnStartRecording_Click(object sender, EventArgs e)
    {
        var url = cmbRecordUrl.Text.Trim();

        // 如果使用者選擇預設選項，提取實際 URL
        if (url.Contains(": http"))
        {
            url = url.Substring(url.IndexOf("http"));
        }

        if (string.IsNullOrWhiteSpace(url)) { ShowError("請輸入或選擇目標 URL"); return; }
        try
        {
            SetBusy(true);
            btnStartRecording.Enabled = false;
            btnStopRecording.Enabled  = true;
            rtbScript.Clear();
            rtbScript.AppendText("錄製中… 請在瀏覽器操作，完成後關閉 Codegen 視窗。\n");

            _cts = new CancellationTokenSource();
            var script = await _playwright!.RecordAsync(url, _cts.Token);

            rtbScript.Clear();
            rtbScript.AppendText(script);
            lblScriptStatus.Text = script.Length > 0 ? "✅ 錄製完成" : "⚠ 未取得腳本";
        }
        catch (OperationCanceledException) { rtbScript.AppendText("\n[錄製已取消]"); }
        catch (Exception ex) { ShowError(ex); }
        finally
        {
            SetBusy(false);
            btnStartRecording.Enabled = true;
            btnStopRecording.Enabled  = false;
        }
    }

    private void btnStopRecording_Click(object sender, EventArgs e)
    {
        _cts.Cancel();
        _playwright?.StopRecording();
    }

    private async void btnTransformScript_Click(object sender, EventArgs e)
    {
        var script = rtbScript.Text.Trim();
        if (string.IsNullOrWhiteSpace(script)) { ShowError("沒有腳本可以轉換"); return; }
        if (lvTestCases.SelectedItems.Count == 0)
        {
            ShowError("請先在「測試案例」頁選取對應的測試案例");
            return;
        }

        var tc = (TestCase)lvTestCases.SelectedItems[0].Tag!;

        try
        {
            SetBusy(true);
            _cts = new CancellationTokenSource();
            rtbGeneratedScript.Clear();

            // Inject assertion rules into the test-case markdown body
            var enrichedMd = EnrichMarkdownWithAssertions(tc);

            // Create a copy of the test case with enriched markdown
            var enrichedTc = new TestCase
            {
                RedmineIssueId = tc.RedmineIssueId,
                Title = tc.Title,
                Status = tc.Status,
                LastRun = tc.LastRun,
                CreatedAt = tc.CreatedAt,
                PlaywrightScript = tc.PlaywrightScript,
                FilePath = tc.FilePath,
                MarkdownBody = enrichedMd
            };

            var transformer = new ScriptTransformer(_ai!, _settings);
            var outPath = await transformer.TransformAndSaveAsync(
                script,
                enrichedTc,   // pass enriched copy
                chunk => SafeAppend(rtbGeneratedScript, chunk),
                _cts.Token);

            lblGeneratedPath.Text = outPath;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { ShowError(ex); }
        finally { SetBusy(false); }
    }

    /// <summary>
    /// Open AI Chat window for Playwright script optimization
    /// </summary>
    private void btnOpenAIChat_Click(object sender, EventArgs e)
    {
        var script = rtbGeneratedScript.Text.Trim();
        if (string.IsNullOrWhiteSpace(script))
        {
            script = rtbScript.Text.Trim();
            if (string.IsNullOrWhiteSpace(script))
            {
                ShowError("請先產生或錄製腳本");
                return;
            }
        }

        var testName = lvTestCases.SelectedItems.Count > 0
            ? ((TestCase)lvTestCases.SelectedItems[0].Tag!).Title
            : "New Test";

        var scriptPath = lblGeneratedPath.Text;
        if (string.IsNullOrWhiteSpace(scriptPath) || !File.Exists(scriptPath))
        {
            scriptPath = "";
        }

        var session = new PlaywrightTestSession
        {
            TestName = testName,
            ScriptPath = scriptPath,
            CurrentScript = script
        };

        var optimizer = new PlaywrightAIOptimizer(_ai!, _playwright!, _settings);

        using var chatForm = new PlaywrightChatForm(session, optimizer, _playwright!);
        chatForm.ShowDialog();

        // Update script after chat
        if (!string.IsNullOrWhiteSpace(session.CurrentScript))
        {
            rtbGeneratedScript.Text = session.CurrentScript;
            if (!string.IsNullOrWhiteSpace(session.ScriptPath))
            {
                lblGeneratedPath.Text = session.ScriptPath;
            }
        }
    }

    /// <summary>
    /// Appends user-defined assertion rules to the test-case markdown
    /// so the AI can generate matching expect() calls.
    /// </summary>
    private string EnrichMarkdownWithAssertions(TestCase tc)
    {
        var active = _assertionRules.Where(r => r.Enabled).ToList();
        if (active.Count == 0) return tc.MarkdownBody;

        var sb = new System.Text.StringBuilder(tc.MarkdownBody);
        sb.AppendLine();
        sb.AppendLine("## 補充驗證條件（使用者定義）");
        sb.AppendLine("請依照以下條件，在對應的操作步驟後加入 expect() assertion：");
        sb.AppendLine();
        for (int i = 0; i < active.Count; i++)
            sb.AppendLine($"{i + 1}. {active[i].ToPromptLine()}");

        sb.AppendLine();
        sb.AppendLine("對應的 Playwright 程式碼參考（定位方式與期望值已確認）：");
        sb.AppendLine("```typescript");
        foreach (var r in active)
            sb.AppendLine(r.ToPlaywrightCode());
        sb.AppendLine("```");

        return sb.ToString();
    }

    // ── Tab: Test Runner ──────────────────────────────────────────────────

    private void RefreshTestScenarios()
    {
        try
        {
            cmbTestScenario.Items.Clear();
            cmbTestScenario.Items.Add("（不使用測試情境）");

            var testCasesDir = _settings.Output.TestCasesDirectory;
            if (Directory.Exists(testCasesDir))
            {
                var markdownFiles = Directory.GetFiles(testCasesDir, "*.md", SearchOption.TopDirectoryOnly);
                foreach (var file in markdownFiles.OrderBy(f => f))
                {
                    var fileName = Path.GetFileName(file);
                    cmbTestScenario.Items.Add($"📄 {fileName}");
                }
            }

            cmbTestScenario.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            ShowError($"載入測試情境列表失敗：{ex.Message}");
        }
    }

    private void btnRefreshScenarios_Click(object sender, EventArgs e)
    {
        RefreshTestScenarios();
        SafeAppend(rtbRunLog, "✅ 已重新整理測試情境列表\n");
    }

    private async void btnRunTest_Click(object sender, EventArgs e)
    {
        var specPath = lblSpecPath.Text.Trim();
        if (!File.Exists(specPath)) { ShowError("找不到測試檔案，請先選取"); return; }

        string? testCaseMarkdown = null;
        string? markdownFilePath = null;
        var selectedScenario = cmbTestScenario.SelectedItem?.ToString() ?? "";

        // 如果選擇了 Markdown 測試情境檔案，讀取內容
        if (selectedScenario.StartsWith("📄"))
        {
            try
            {
                var fileName = selectedScenario.Replace("📄 ", "").Trim();
                markdownFilePath = Path.Combine(_settings.Output.TestCasesDirectory, fileName);
                if (File.Exists(markdownFilePath))
                {
                    testCaseMarkdown = await File.ReadAllTextAsync(markdownFilePath);
                    SafeAppend(rtbRunLog, $"📋 使用測試情境：{fileName}\n");
                }
            }
            catch (Exception ex)
            {
                ShowError($"讀取測試情境檔案失敗：{ex.Message}");
                return;
            }
        }

        try
        {
            SetBusy(true);
            _cts = new CancellationTokenSource();
            rtbRunLog.Clear();
            lvTestSteps.Items.Clear();

            SafeAppend(rtbRunLog, $"🚀 開始執行測試：{Path.GetFileName(specPath)}\n");
            if (!string.IsNullOrEmpty(testCaseMarkdown))
            {
                SafeAppend(rtbRunLog, "📝 包含測試情境步驟驗證\n");
            }
            SafeAppend(rtbRunLog, new string('─', 60) + "\n\n");

            var result = await _playwright!.RunTestAsync(
                specPath,
                line => SafeAppend(rtbRunLog, line + "\n"),
                _cts.Token);

            lblRunResult.Text      = result.Status.ToUpper();
            lblRunResult.ForeColor = result.Status == "passed" ? Color.Green : Color.Red;

            PopulateTestSteps(result);

            // 如果使用了 Markdown 測試情境，將結果寫回該檔案
            if (!string.IsNullOrEmpty(markdownFilePath) && File.Exists(markdownFilePath))
            {
                try
                {
                    var tc = MarkdownHelper.ParseFile(markdownFilePath);
                    var writer = new ResultWriter(_settings, _redmine);
                    await writer.WriteAsync(tc, result, _cts.Token);
                    SafeAppend(rtbRunLog, $"\n✅ 測試結果已寫回：{Path.GetFileName(markdownFilePath)}\n");
                }
                catch (Exception ex)
                {
                    SafeAppend(rtbRunLog, $"\n⚠️ 寫回測試結果失敗：{ex.Message}\n");
                }
            }

            // 自動產生包含測試情境的 Markdown 報告
            if (!string.IsNullOrEmpty(testCaseMarkdown))
            {
                await GenerateTestReportWithScenarioAsync(
                    Path.GetFileNameWithoutExtension(specPath),
                    result,
                    testCaseMarkdown);
            }

            // 如果在測試案例列表中有選取項目，也寫回該項目
            if (lvTestCases.SelectedItems.Count > 0)
            {
                var tc     = (TestCase)lvTestCases.SelectedItems[0].Tag!;
                var writer = new ResultWriter(_settings, _redmine);
                await writer.WriteAsync(tc, result, _cts.Token);
                LoadTestCaseList();
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { ShowError(ex); }
        finally { SetBusy(false); }
    }

    private async Task GenerateTestReportWithScenarioAsync(
        string testName,
        TestRunResult result,
        string scenarioMarkdown)
    {
        try
        {
            var reportGenerator = new MarkdownTestReportGenerator();
            var reportDir = @"D:\autoTester\playwright-tests\test-reports";
            Directory.CreateDirectory(reportDir);

            // 建立包含測試情境的 session
            var session = new PlaywrightTestSession
            {
                TestName = testName,
                ScriptPath = lblSpecPath.Text,
                CurrentScript = scenarioMarkdown,  // 包含測試情境內容
                ChatHistory = new List<ChatMessage>(),
                TestResults = new List<TestRunResult> { result }
            };

            var reportPath = await reportGenerator.SaveReportAsync(
                testName,
                new List<TestRunResult> { result },
                reportDir,
                session);

            SafeAppend(rtbRunLog, $"\n📄 測試報告已產生：\n{reportPath}\n");
        }
        catch (Exception ex)
        {
            SafeAppend(rtbRunLog, $"\n⚠️ 報告產生失敗：{ex.Message}\n");
        }
    }

    private void btnPickSpec_Click(object sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Filter           = "TypeScript 測試檔|*.spec.ts|所有檔案|*.*",
            InitialDirectory = _settings.Output.ScriptsDirectory
        };
        if (dlg.ShowDialog() == DialogResult.OK)
            lblSpecPath.Text = dlg.FileName;
    }

    // ── Settings ──────────────────────────────────────────────────────────

    private void btnOpenSettings_Click(object sender, EventArgs e)
    {
        using var form = new SettingsForm(_settings);
        if (form.ShowDialog() == DialogResult.OK)
        {
            SettingsManager.Save(_settings);
            LoadSettings();
            SetupStatusBar();
            _ = CheckEnvironmentAsync();
        }
    }

    // ── Utilities ─────────────────────────────────────────────────────────

    private void PopulateTestSteps(TestRunResult result)
    {
        lvTestSteps.Items.Clear();
        rtbStepError.Text = "（點擊失敗步驟查看錯誤詳情）";
        if (result.TestSteps.Count == 0) return;

        lvTestSteps.BeginUpdate();
        foreach (var step in result.TestSteps)
        {
            var icon = step.Passed ? "✅" : "❌";
            var duration = step.Duration.TotalSeconds >= 1
                ? $"{step.Duration.TotalSeconds:F1}s"
                : $"{step.Duration.TotalMilliseconds:F0}ms";

            var item = new ListViewItem(icon);
            item.SubItems.Add(step.StepName);
            item.SubItems.Add(step.Action);
            item.SubItems.Add(duration);
            item.Tag      = step;
            item.ForeColor = step.Passed ? Color.FromArgb(0, 120, 50) : Color.Crimson;
            item.BackColor = step.Passed ? Color.FromArgb(240, 255, 245) : Color.FromArgb(255, 242, 242);

            if (!step.Passed && !string.IsNullOrEmpty(step.ErrorMessage))
                item.ToolTipText = step.ErrorMessage;

            lvTestSteps.Items.Add(item);
        }
        lvTestSteps.EndUpdate();

        // 加入統計摘要列
        var passed = result.TestSteps.Count(s => s.Passed);
        var failed = result.TestSteps.Count(s => !s.Passed);
        SafeAppend(rtbRunLog, $"\n📋 步驟統計：共 {result.TestSteps.Count} 步，✅ {passed} 通過，❌ {failed} 失敗\n");
    }

    private void lvTestSteps_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lvTestSteps.SelectedItems.Count == 0) return;
        var step = lvTestSteps.SelectedItems[0].Tag as TestStepResult;
        if (step == null) return;

        if (!step.Passed && !string.IsNullOrEmpty(step.ErrorMessage))
        {
            rtbStepError.ForeColor = Color.FromArgb(255, 100, 100);
            rtbStepError.Text = step.ErrorMessage;
        }
        else
        {
            rtbStepError.ForeColor = Color.FromArgb(100, 220, 120);
            rtbStepError.Text = $"✅ 步驟通過  耗時：{(step.Duration.TotalSeconds >= 1 ? $"{step.Duration.TotalSeconds:F1}s" : $"{step.Duration.TotalMilliseconds:F0}ms")}";
        }
    }

    private void SetBusy(bool busy)
    {
        Invoke(() =>
        {
            progressBar.Visible = busy;
            progressBar.Style   = busy ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
        });
    }

    private void SafeAppend(RichTextBox rtb, string text)
    {
        if (rtb.InvokeRequired)
            rtb.Invoke(() => { rtb.AppendText(text); rtb.ScrollToCaret(); });
        else { rtb.AppendText(text); rtb.ScrollToCaret(); }
    }

    private static void ShowError(Exception ex) =>
        MessageBox.Show(ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);

    private static void ShowError(string msg) =>
        MessageBox.Show(msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
