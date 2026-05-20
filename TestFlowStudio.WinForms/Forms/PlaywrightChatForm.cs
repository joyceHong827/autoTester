using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestFlowStudio.Core.Models;
using TestFlowStudio.Core.Services;
using TestFlowStudio.Core.Interfaces;

namespace TestFlowStudio.WinForms.Forms
{
    public partial class PlaywrightChatForm : Form
    {
        private readonly PlaywrightTestSession _session;
        private readonly PlaywrightAIOptimizer _optimizer;
        private readonly IPlaywrightService _playwrightService;
        private readonly MarkdownTestReportGenerator _reportGenerator;
        private CancellationTokenSource? _cts;

        public PlaywrightChatForm(
            PlaywrightTestSession session,
            PlaywrightAIOptimizer optimizer,
            IPlaywrightService playwrightService)
        {
            InitializeComponent();
            _session = session;
            _optimizer = optimizer;
            _playwrightService = playwrightService;
            _reportGenerator = new MarkdownTestReportGenerator();
        }

        private void PlaywrightChatForm_Load(object sender, EventArgs e)
        {
            lblTestName.Text = $"🤖 {_session.TestName}";

            // Load available test files
            RefreshTestFiles();

            // Load chat history
            foreach (var msg in _session.ChatHistory)
            {
                AppendChatMessage(msg);
            }

            // Welcome message
            if (!_session.ChatHistory.Any())
            {
                var welcomeMsg = new ChatMessage
                {
                    Role = "system",
                    Content = "歡迎！我是 Playwright AI 助手，可以幫你優化測試腳本。\n\n你可以：\n- 詢問如何改善腳本\n- 請我加入新的測試步驟\n- 修正特定的問題\n- 執行測試並查看結果\n- 從下拉選單選擇不同的測試檔案執行",
                    Timestamp = DateTime.Now
                };
                _session.ChatHistory.Add(welcomeMsg);
                AppendChatMessage(welcomeMsg);
            }
        }

        private void RefreshTestFiles()
        {
            try
            {
                var testsDir = @"D:\autoTester\playwright-tests\tests";
                if (!Directory.Exists(testsDir))
                {
                    Directory.CreateDirectory(testsDir);
                    return;
                }

                cmbTestFiles.Items.Clear();

                // Add "Current Script" option
                cmbTestFiles.Items.Add("📝 目前編輯的腳本");

                // Add all .spec.ts files
                var testFiles = Directory.GetFiles(testsDir, "*.spec.ts", SearchOption.TopDirectoryOnly);
                foreach (var file in testFiles.OrderBy(f => f))
                {
                    var fileName = Path.GetFileName(file);
                    cmbTestFiles.Items.Add($"📄 {fileName}");
                }

                // Select current script by default
                if (!string.IsNullOrEmpty(_session.ScriptPath))
                {
                    var currentFileName = Path.GetFileName(_session.ScriptPath);
                    var itemIndex = cmbTestFiles.Items.Cast<string>()
                        .Select((item, index) => new { item, index })
                        .FirstOrDefault(x => x.item.Contains(currentFileName))?.index ?? 0;
                    cmbTestFiles.SelectedIndex = itemIndex;
                }
                else
                {
                    cmbTestFiles.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                AppendTestOutput($"⚠️ 載入測試檔案列表時發生錯誤: {ex.Message}\n", Color.Orange);
            }
        }

        private void btnRefreshTests_Click(object sender, EventArgs e)
        {
            RefreshTestFiles();
            AppendTestOutput("✅ 已重新整理測試檔案列表\n", Color.Green);
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            var userMessage = txtUserInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(userMessage))
                return;

            txtUserInput.Clear();
            txtUserInput.Enabled = false;
            btnSend.Enabled = false;

            try
            {
                _cts = new CancellationTokenSource();

                // Show user message
                var userMsg = new ChatMessage
                {
                    Role = "user",
                    Content = userMessage,
                    Timestamp = DateTime.Now
                };
                AppendChatMessage(userMsg);

                // Prepare for assistant response
                var assistantMsgStart = txtChatHistory.TextLength;
                AppendText("🤖 AI: ", Color.Blue, true);

                // Stream response
                var response = await _optimizer.ChatAsync(
                    userMessage,
                    _session,
                    onChatMessage: null,
                    onChunk: chunk =>
                    {
                        if (txtChatHistory.InvokeRequired)
                        {
                            txtChatHistory.Invoke(() => txtChatHistory.AppendText(chunk));
                        }
                        else
                        {
                            txtChatHistory.AppendText(chunk);
                        }
                    },
                    _cts.Token);

                txtChatHistory.AppendText("\n\n");
                txtChatHistory.ScrollToCaret();

                // Check if response contains code
                if (response.Contains("```"))
                {
                    var extractedCode = ExtractCodeFromResponse(response);
                    if (!string.IsNullOrWhiteSpace(extractedCode))
                    {
                        _session.CurrentScript = extractedCode;
                        AppendText("✅ 已更新腳本內容\n\n", Color.Green, false);
                    }
                }
            }
            catch (Exception ex)
            {
                AppendText($"❌ 錯誤: {ex.Message}\n\n", Color.Red, false);
            }
            finally
            {
                txtUserInput.Enabled = true;
                btnSend.Enabled = true;
                txtUserInput.Focus();
                _cts?.Dispose();
                _cts = null;
            }
        }

        private async void btnRunTest_Click(object sender, EventArgs e)
        {
            // Determine which test file to run
            string scriptPath;
            string testName;

            var selectedItem = cmbTestFiles.SelectedItem?.ToString() ?? "";

            if (selectedItem.StartsWith("📝"))
            {
                // Use current script
                if (string.IsNullOrWhiteSpace(_session.ScriptPath))
                {
                    MessageBox.Show("請先儲存腳本", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                scriptPath = _session.ScriptPath;
                testName = _session.TestName;
            }
            else if (selectedItem.StartsWith("📄"))
            {
                // Use selected test file
                var fileName = selectedItem.Replace("📄 ", "").Trim();
                scriptPath = Path.Combine(@"D:\autoTester\playwright-tests\tests", fileName);
                testName = Path.GetFileNameWithoutExtension(fileName);

                if (!File.Exists(scriptPath))
                {
                    MessageBox.Show($"找不到測試檔案：{scriptPath}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show("請選擇要執行的測試檔案", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnRunTest.Enabled = false;
            txtTestOutput.Clear();

            try
            {
                _cts = new CancellationTokenSource();

                AppendTestOutput($"📋 執行測試檔案：{Path.GetFileName(scriptPath)}\n");
                AppendTestOutput($"📂 完整路徑：{scriptPath}\n\n");
                AppendTestOutput("開始執行測試...\n\n");

                var result = await _optimizer.RunWithAutoRetryAsync(
                    scriptPath,
                    "", // testCaseMd can be loaded if needed
                    onOutput: output =>
                    {
                        AppendTestOutput(output + "\n");
                    },
                    onChatMessage: msg =>
                    {
                        if (msg.Role == "system" || msg.Role == "assistant")
                        {
                            AppendChatMessage(msg);
                        }
                    },
                    _cts.Token);

                _session.TestResults.Add(result);

                // Show summary
                if (result.Success)
                {
                    AppendTestOutput($"\n✅ 測試通過！ ({result.PassedTests}/{result.TotalTests})\n", Color.Green);
                }
                else
                {
                    AppendTestOutput($"\n❌ 測試失敗 ({result.FailedTests}/{result.TotalTests})\n", Color.Red);
                    AppendTestOutput($"重試次數: {result.RetryAttempt}\n");
                }

                // Show HTML report path
                if (!string.IsNullOrEmpty(result.HtmlReportPath) && File.Exists(result.HtmlReportPath))
                {
                    AppendTestOutput($"\n📊 Playwright HTML 報告已產生：\n", Color.Cyan);
                    AppendTestOutput($"{result.HtmlReportPath}\n", Color.Yellow);
                }

                // 自動產生 Markdown 測試報告
                try
                {
                    var defaultDir = @"D:\autoTester\playwright-tests\test-reports";
                    Directory.CreateDirectory(defaultDir);

                    var reportPath = await _reportGenerator.SaveReportAsync(
                        testName,
                        new List<TestRunResult> { result }, // Only include current result
                        defaultDir,
                        new PlaywrightTestSession 
                        { 
                            TestName = testName,
                            ScriptPath = scriptPath,
                            ChatHistory = _session.ChatHistory,
                            TestResults = new List<TestRunResult> { result }
                        });

                    AppendTestOutput($"\n📄 Markdown 測試報告已自動產生：\n", Color.Cyan);
                    AppendTestOutput($"{reportPath}\n", Color.Yellow);
                    AppendTestOutput("可使用「📄 匯出報告」按鈕重新產生或開啟報告\n", Color.Gray);
                }
                catch (Exception reportEx)
                {
                    AppendTestOutput($"\n⚠️ 報告產生失敗: {reportEx.Message}\n", Color.Orange);
                }
            }
            catch (Exception ex)
            {
                AppendTestOutput($"\n❌ 執行錯誤: {ex.Message}\n", Color.Red);
            }
            finally
            {
                btnRunTest.Enabled = true;
                _cts?.Dispose();
                _cts = null;
            }
        }

        private async void btnSaveScript_Click(object sender, EventArgs e)
        {
            try
            {
                var scriptPath = await _playwrightService.SaveTestScriptAsync(
                    _session.TestName,
                    _session.CurrentScript);

                _session.ScriptPath = scriptPath;

                // Refresh test files list after saving
                RefreshTestFiles();

                MessageBox.Show(
                    $"腳本已儲存至：\n{scriptPath}",
                    "儲存成功",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"儲存失敗：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnExportReport_Click(object sender, EventArgs e)
        {
            if (!_session.TestResults.Any())
            {
                MessageBox.Show("尚無測試結果", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 預設儲存到 D:\autoTester\playwright-tests\test-reports
                var defaultDir = @"D:\autoTester\playwright-tests\test-reports";
                Directory.CreateDirectory(defaultDir);

                // 自動儲存到預設目錄
                var reportPath = await _reportGenerator.SaveReportAsync(
                    _session.TestName,
                    _session.TestResults,
                    defaultDir,
                    _session);

                // 詢問是否開啟報告
                var result = MessageBox.Show(
                    $"✅ 測試報告已產生！\n\n" +
                    $"📄 Markdown 報告：\n{reportPath}\n\n" +
                    $"📊 Playwright HTML 報告：\n{_session.TestResults.LastOrDefault()?.HtmlReportPath ?? "未產生"}\n\n" +
                    $"是否開啟 Markdown 報告？",
                    "報告產生成功",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes && File.Exists(reportPath))
                {
                    // 使用預設程式開啟 Markdown 檔案
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = reportPath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"匯出失敗：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AppendChatMessage(ChatMessage msg)
        {
            var icon = msg.Role == "user" ? "👤" : msg.Role == "assistant" ? "🤖" : "ℹ️";
            var color = msg.Role == "user" ? Color.DarkBlue : msg.Role == "assistant" ? Color.DarkGreen : Color.Gray;
            var timestamp = msg.Timestamp.ToString("HH:mm:ss");

            AppendText($"{icon} {msg.Role.ToUpper()} [{timestamp}]\n", color, true);
            AppendText($"{msg.Content}\n\n", Color.Black, false);

            txtChatHistory.ScrollToCaret();
        }

        private void AppendText(string text, Color color, bool bold)
        {
            if (txtChatHistory.InvokeRequired)
            {
                txtChatHistory.Invoke(() => AppendText(text, color, bold));
                return;
            }

            var start = txtChatHistory.TextLength;
            txtChatHistory.AppendText(text);
            txtChatHistory.Select(start, text.Length);
            txtChatHistory.SelectionColor = color;
            if (bold)
                txtChatHistory.SelectionFont = new Font(txtChatHistory.Font, FontStyle.Bold);
            txtChatHistory.Select(txtChatHistory.TextLength, 0);
            txtChatHistory.ScrollToCaret();
        }

        private void AppendTestOutput(string text, Color? color = null)
        {
            if (txtTestOutput.InvokeRequired)
            {
                txtTestOutput.Invoke(() => AppendTestOutput(text, color));
                return;
            }

            var start = txtTestOutput.TextLength;
            txtTestOutput.AppendText(text);
            if (color.HasValue)
            {
                txtTestOutput.Select(start, text.Length);
                txtTestOutput.SelectionColor = color.Value;
            }
            txtTestOutput.Select(txtTestOutput.TextLength, 0);
            txtTestOutput.ScrollToCaret();
        }

        private static string ExtractCodeFromResponse(string response)
        {
            var lines = response.Split('\n');
            var inCodeBlock = false;
            var sb = new System.Text.StringBuilder();

            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("```"))
                {
                    inCodeBlock = !inCodeBlock;
                    continue;
                }

                if (inCodeBlock)
                {
                    sb.AppendLine(line);
                }
            }

            var extracted = sb.ToString().Trim();
            return string.IsNullOrWhiteSpace(extracted) ? response : extracted;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cts?.Cancel();
            base.OnFormClosing(e);
        }
    }
}
