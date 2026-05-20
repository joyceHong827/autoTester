namespace TestFlowStudio.WinForms.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;

    // Shared controls
    private TabControl tabMain = null!;
    private ProgressBar progressBar = null!;
    private StatusStrip statusStrip = null!;
    private ToolStripStatusLabel tsRedmine = null!, tsProvider = null!, tsNode = null!;

    // Tab pages
    private TabPage tabDashboard = null!;
    private TabPage tabIssues = null!;
    private TabPage tabTestCases = null!;
    private TabPage tabRecording = null!;
    private TabPage tabRunner = null!;
    private TabPage tabSettings = null!;

    // Dashboard
    private Label lblTotalCases = null!, lblPassed = null!, lblFailed = null!, lblPending = null!;

    // Issues tab
    private ComboBox cmbProject = null!;
    private Button btnLoadProjects = null!, btnSearchIssues = null!, btnGenerateTestCase = null!;
    private TextBox txtIssueSearch = null!;
    private DataGridView dgvIssues = null!;
    private RichTextBox rtbIssuePreview = null!;

    // TestCases tab
    private ListView lvTestCases = null!;
    private ComboBox cmbStatusFilter = null!;

    // Recording tab
    private ComboBox cmbRecordUrl = null!;      // ← 改為 ComboBox 支援預設選項
    private Button btnStartRecording = null!, btnStopRecording = null!;
    private Button btnEditAssertions = null!;   // ← 驗證條件按鈕
    private Button btnTransformScript = null!;
    private Button btnOpenAIChat = null!;       // ← AI 對話按鈕
    private RichTextBox rtbScript = null!, rtbGeneratedScript = null!;
    private Label lblScriptStatus = null!, lblGeneratedPath = null!;

    // Runner tab
    private Label lblSpecPath = null!, lblRunResult = null!;
    private Button btnPickSpec = null!, btnRunTest = null!;
    private ComboBox cmbTestScenario = null!;
    private Button btnRefreshScenarios = null!;
    private RichTextBox rtbRunLog = null!;
    private ListView lvTestSteps = null!;
    private RichTextBox rtbStepError = null!;
    private SplitContainer splitRunner = null!;

    // Settings tab
    private Button btnOpenSettings = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();

        this.Text          = "TestFlow Studio";
        this.Size          = new Size(1280, 820);
        this.MinimumSize   = new Size(1024, 680);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font          = new Font("Microsoft JhengHei UI", 9.5f);
        this.BackColor     = Color.FromArgb(245, 247, 250);

        // StatusStrip
        statusStrip           = new StatusStrip();
        statusStrip.BackColor = Color.FromArgb(30, 58, 92);
        tsRedmine  = new ToolStripStatusLabel("Redmine: 未連線") { ForeColor = Color.White };
        tsProvider = new ToolStripStatusLabel("AI: —")          { ForeColor = Color.LightSkyBlue };
        tsNode     = new ToolStripStatusLabel("Node.js: 檢查中") { ForeColor = Color.PaleGreen };
        statusStrip.Items.Add(tsRedmine);
        statusStrip.Items.Add(new ToolStripSeparator());
        statusStrip.Items.Add(tsProvider);
        statusStrip.Items.Add(new ToolStripSeparator());
        statusStrip.Items.Add(tsNode);

        // ProgressBar
        progressBar         = new ProgressBar();
        progressBar.Dock    = DockStyle.Top;
        progressBar.Height  = 4;
        progressBar.Style   = ProgressBarStyle.Marquee;
        progressBar.Visible = false;

        // TabControl
        tabMain         = new TabControl();
        tabMain.Dock    = DockStyle.Fill;
        tabMain.Font    = new Font("Microsoft JhengHei UI", 10f);
        tabMain.Padding = new Point(16, 6);
        tabMain.SelectedIndexChanged += new EventHandler(this.tabMain_SelectedIndexChanged);

        BuildDashboardTab();
        BuildIssuesTab();
        BuildTestCasesTab();
        BuildRecordingTab();
        BuildRunnerTab();
        BuildSettingsTab();

        tabMain.TabPages.Add(tabDashboard);
        tabMain.TabPages.Add(tabIssues);
        tabMain.TabPages.Add(tabTestCases);
        tabMain.TabPages.Add(tabRecording);
        tabMain.TabPages.Add(tabRunner);
        tabMain.TabPages.Add(tabSettings);

        this.Controls.Add(tabMain);
        this.Controls.Add(progressBar);
        this.Controls.Add(statusStrip);

        ResumeLayout(false);
        PerformLayout();
    }

    // ── Dashboard ─────────────────────────────────────────────────────────
    private void BuildDashboardTab()
    {
        tabDashboard = new TabPage("📊 Dashboard");
        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(20),
            FlowDirection = FlowDirection.LeftToRight
        };
        lblTotalCases = CreateStatCard("總案例", "0", Color.FromArgb(37, 99, 168));
        lblPassed     = CreateStatCard("已通過", "0", Color.Green);
        lblFailed     = CreateStatCard("失敗",   "0", Color.Crimson);
        lblPending    = CreateStatCard("待執行", "0", Color.DarkOrange);
        flow.Controls.Add(lblTotalCases);
        flow.Controls.Add(lblPassed);
        flow.Controls.Add(lblFailed);
        flow.Controls.Add(lblPending);
        tabDashboard.Controls.Add(flow);
    }

    // ── Redmine Issues ────────────────────────────────────────────────────
    private void BuildIssuesTab()
    {
        tabIssues = new TabPage("🐞 Redmine Issues");
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(8)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        cmbProject = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        btnLoadProjects = CreateButton("載入專案", new EventHandler(this.btnLoadProjects_Click));
        txtIssueSearch  = new TextBox { Width = 180, PlaceholderText = "搜尋 Issue…" };
        btnSearchIssues = CreateButton("搜尋", new EventHandler(this.btnSearchIssues_Click));
        btnGenerateTestCase = CreateButton("✨ 生成測試案例", new EventHandler(this.btnGenerateTestCase_Click), Color.FromArgb(37, 99, 168));
        toolbar.Controls.Add(cmbProject);
        toolbar.Controls.Add(btnLoadProjects);
        toolbar.Controls.Add(new Label { Width = 12, Text = "" });
        toolbar.Controls.Add(txtIssueSearch);
        toolbar.Controls.Add(btnSearchIssues);
        toolbar.Controls.Add(new Label { Width = 24, Text = "" });
        toolbar.Controls.Add(btnGenerateTestCase);

        dgvIssues = new DataGridView
        {
            Dock = DockStyle.Fill, ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AllowUserToAddRows = false, BackgroundColor = Color.White, MultiSelect = false
        };
        rtbIssuePreview = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(252, 252, 255) };

        panel.Controls.Add(toolbar, 0, 0);
        panel.Controls.Add(dgvIssues, 0, 1);
        panel.Controls.Add(rtbIssuePreview, 0, 2);
        tabIssues.Controls.Add(panel);
    }

    // ── Test Cases ────────────────────────────────────────────────────────
    private void BuildTestCasesTab()
    {
        tabTestCases = new TabPage("📋 測試案例");
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(8)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        var filterLabel = new Label { Text = "篩選狀態：", Width = 80, TextAlign = ContentAlignment.MiddleLeft };
        cmbStatusFilter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
        cmbStatusFilter.Items.AddRange(new object[] { "全部", "pending", "passed", "failed", "error" });
        cmbStatusFilter.SelectedIndex = 0;
        cmbStatusFilter.SelectedIndexChanged += new EventHandler(this.cmbStatusFilter_SelectedIndexChanged);
        toolbar.Controls.Add(filterLabel);
        toolbar.Controls.Add(cmbStatusFilter);

        lvTestCases = new ListView
        {
            Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true,
            GridLines = true, MultiSelect = false, BackColor = Color.White
        };
        lvTestCases.Columns.Add("檔案名稱", 240);
        lvTestCases.Columns.Add("標題",     300);
        lvTestCases.Columns.Add("狀態",      90);
        lvTestCases.Columns.Add("最後執行", 160);

        panel.Controls.Add(toolbar, 0, 0);
        panel.Controls.Add(lvTestCases, 0, 1);
        tabTestCases.Controls.Add(panel);
    }

    // ── Recording Studio ──────────────────────────────────────────────────
    private void BuildRecordingTab()
    {
        tabRecording = new TabPage("🎬 錄製工作台");
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1, Padding = new Padding(8)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 60));

        // Row 0 — URL bar
        var urlBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false
        };
        var urlLabel = new Label { Text = "URL：", Width = 45, TextAlign = ContentAlignment.MiddleLeft };

        // 改為 ComboBox 並預設 b2b/b2e 選項
        cmbRecordUrl = new ComboBox 
        { 
            Width = 420,
            DropDownStyle = ComboBoxStyle.DropDown
        };
        cmbRecordUrl.Items.Add("b2b: http://b2b.lab.etzone.net/Web/B2B_B2ELogin");
        cmbRecordUrl.Items.Add("b2e: http://b2e.lab.etzone.net/web/B2ELogin");
        cmbRecordUrl.SelectedIndex = 0; // 預設選擇 b2b

        btnStartRecording = CreateButton("▶ 開始錄製", new EventHandler(this.btnStartRecording_Click), Color.ForestGreen);
        btnStopRecording  = CreateButton("■ 停止",     new EventHandler(this.btnStopRecording_Click),  Color.Crimson);
        btnStopRecording.Enabled = false;
        lblScriptStatus = new Label { AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Text = "" };

        urlBar.Controls.Add(urlLabel);
        urlBar.Controls.Add(cmbRecordUrl);
        urlBar.Controls.Add(btnStartRecording);
        urlBar.Controls.Add(btnStopRecording);
        urlBar.Controls.Add(lblScriptStatus);

        // Row 1 — Codegen script preview
        rtbScript = new RichTextBox
        {
            Dock = DockStyle.Fill, Font = new Font("Consolas", 9.5f),
            BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.LightGreen
        };

        // Row 2 — Action bar (assertion editor + AI transform)
        var xformBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false, Padding = new Padding(0, 4, 0, 0)
        };

        // ▼ Assertion editor button
        btnEditAssertions = CreateButton("⚙ 驗證條件", new EventHandler(this.btnEditAssertions_Click), Color.FromArgb(80, 80, 80));
        btnEditAssertions.Width = 130;

        var separator = new Label { Width = 16, Text = "" };

        btnTransformScript = CreateButton(
            "🤖 AI 轉換為 TypeScript",
            new EventHandler(this.btnTransformScript_Click),
            Color.FromArgb(37, 99, 168));

        btnOpenAIChat = CreateButton(
            "💬 AI 對話優化",
            new EventHandler(this.btnOpenAIChat_Click),
            Color.FromArgb(139, 69, 19));
        btnOpenAIChat.Width = 120;

        lblGeneratedPath = new Label { AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Text = "" };

        xformBar.Controls.Add(btnEditAssertions);
        xformBar.Controls.Add(separator);
        xformBar.Controls.Add(btnTransformScript);
        xformBar.Controls.Add(btnOpenAIChat);
        xformBar.Controls.Add(lblGeneratedPath);

        // Row 3 — Generated script preview
        rtbGeneratedScript = new RichTextBox
        {
            Dock = DockStyle.Fill, Font = new Font("Consolas", 9.5f),
            BackColor = Color.FromArgb(20, 20, 40), ForeColor = Color.Cyan
        };

        panel.Controls.Add(urlBar,             0, 0);
        panel.Controls.Add(rtbScript,          0, 1);
        panel.Controls.Add(xformBar,           0, 2);
        panel.Controls.Add(rtbGeneratedScript, 0, 3);
        tabRecording.Controls.Add(panel);
    }

    // ── Test Runner ───────────────────────────────────────────────────────
    private void BuildRunnerTab()
    {
        tabRunner = new TabPage("▶ 測試執行");
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(8)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Row 0 - Test spec file selection
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        btnPickSpec = CreateButton("選擇 .spec.ts", new EventHandler(this.btnPickSpec_Click));
        lblSpecPath = new Label { Text = "（尚未選取）", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
        btnRunTest  = CreateButton("▶ 執行測試", new EventHandler(this.btnRunTest_Click), Color.ForestGreen);
        var resultLabel = new Label { Text = "結果：", Width = 56, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(32, 0, 0, 0) };
        lblRunResult = new Label
        {
            Text = "—", AutoSize = true,
            Font = new Font("Microsoft JhengHei UI", 13f, FontStyle.Bold)
        };
        toolbar.Controls.Add(btnPickSpec);
        toolbar.Controls.Add(lblSpecPath);
        toolbar.Controls.Add(new Label { Width = 32, Text = "" });
        toolbar.Controls.Add(btnRunTest);
        toolbar.Controls.Add(resultLabel);
        toolbar.Controls.Add(lblRunResult);

        // Row 1 - Markdown test scenario selection
        var scenarioBar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var scenarioLabel = new Label { Text = "測試情境：", Width = 80, TextAlign = ContentAlignment.MiddleLeft };
        cmbTestScenario = new ComboBox 
        { 
            Width = 400, 
            DropDownStyle = ComboBoxStyle.DropDownList 
        };
        btnRefreshScenarios = CreateButton("🔄 重整", new EventHandler(this.btnRefreshScenarios_Click));
        scenarioBar.Controls.Add(scenarioLabel);
        scenarioBar.Controls.Add(cmbTestScenario);
        scenarioBar.Controls.Add(btnRefreshScenarios);

        // Row 2 - SplitContainer: 左邊執行 log，右邊測試步驟清單
        splitRunner = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical
        };

        rtbRunLog = new RichTextBox
        {
            Dock = DockStyle.Fill, Font = new Font("Consolas", 9.5f),
            ReadOnly = true, BackColor = Color.Black, ForeColor = Color.White
        };
        splitRunner.Panel1.Controls.Add(rtbRunLog);

        // 右邊：測試步驟清單
        var stepsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1
        };
        stepsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        stepsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        stepsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));

        var stepsHeader = new Label
        {
            Text = "📋 Test Steps",
            Dock = DockStyle.Fill,
            Font = new Font("Microsoft JhengHei UI", 10f, FontStyle.Bold),
            BackColor = Color.FromArgb(37, 99, 168),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(6, 0, 0, 0)
        };

        lvTestSteps = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            ShowItemToolTips = true,
            Font = new Font("Microsoft JhengHei UI", 9f),
            BackColor = Color.FromArgb(250, 252, 255)
        };
        lvTestSteps.Columns.Add("",       24,  HorizontalAlignment.Center);
        lvTestSteps.Columns.Add("步驟名稱", 340, HorizontalAlignment.Left);
        lvTestSteps.Columns.Add("類型",     80,  HorizontalAlignment.Center);
        lvTestSteps.Columns.Add("耗時",     64,  HorizontalAlignment.Right);
        lvTestSteps.SelectedIndexChanged += new EventHandler(this.lvTestSteps_SelectedIndexChanged);

        rtbStepError = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            Font = new Font("Consolas", 8.5f),
            BackColor = Color.FromArgb(40, 10, 10),
            ForeColor = Color.FromArgb(255, 100, 100),
            BorderStyle = BorderStyle.None,
            Text = "（點擊失敗步驟查看錯誤詳情）",
            Visible = true
        };

        stepsPanel.Controls.Add(stepsHeader,  0, 0);
        stepsPanel.Controls.Add(lvTestSteps,  0, 1);
        stepsPanel.Controls.Add(rtbStepError, 0, 2);
        splitRunner.Panel2.Controls.Add(stepsPanel);

        panel.Controls.Add(toolbar,     0, 0);
        panel.Controls.Add(scenarioBar, 0, 1);
        panel.Controls.Add(splitRunner, 0, 2);
        tabRunner.Controls.Add(panel);
    }

    // ── Settings ──────────────────────────────────────────────────────────
    private void BuildSettingsTab()
    {
        tabSettings = new TabPage("⚙ 設定");
        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(20), FlowDirection = FlowDirection.TopDown
        };
        var lbl = new Label { Text = "點擊下方按鈕開啟設定視窗，設定 Redmine、AI Provider 及輸出路徑。", AutoSize = true, Margin = new Padding(0, 0, 0, 12) };
        btnOpenSettings = CreateButton("⚙ 開啟設定", new EventHandler(this.btnOpenSettings_Click), Color.FromArgb(37, 99, 168));
        flow.Controls.Add(lbl);
        flow.Controls.Add(btnOpenSettings);
        tabSettings.Controls.Add(flow);
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private static Label CreateStatCard(string title, string value, Color accent)
    {
        return new Label
        {
            Text = title + "\n" + value, Size = new Size(160, 100), Margin = new Padding(12),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Microsoft JhengHei UI", 14f, FontStyle.Bold),
            ForeColor = Color.White, BackColor = accent
        };
    }

    private static Button CreateButton(string text, EventHandler handler, Color? bgColor = null)
    {
        var btn = new Button
        {
            Text = text, AutoSize = true, FlatStyle = FlatStyle.Flat,
            Font = new Font("Microsoft JhengHei UI", 9.5f),
            Margin = new Padding(4), Cursor = Cursors.Hand
        };
        if (bgColor.HasValue) { btn.BackColor = bgColor.Value; btn.ForeColor = Color.White; }
        btn.Click += handler;
        return btn;
    }
}
