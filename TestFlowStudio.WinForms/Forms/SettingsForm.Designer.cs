namespace TestFlowStudio.WinForms.Forms;

partial class SettingsForm
{
    private System.ComponentModel.IContainer components = null!;

    // Redmine
    private TextBox txtRedmineUrl = null!, txtRedmineKey = null!, txtProjectId = null!;
    private CheckBox chkIgnoreSsl = null!;
    private NumericUpDown nudTimeout = null!;
    private Button btnTestRedmine = null!;

    // AI
    private ComboBox cmbProvider = null!;
    private TextBox txtClaudeKey = null!, txtOpenAIKey = null!, txtGeminiKey = null!;
    private TextBox txtClaudeModel = null!, txtOpenAIModel = null!, txtGeminiModel = null!;
    private NumericUpDown nudMaxTokens = null!;
    private Button btnTestAI = null!;

    // Ollama 本地端
    private TextBox txtOllamaUrl = null!, txtOllamaModel = null!;

    // 任務 AI 指派
    private ComboBox cmbTaskTestCaseProvider = null!, cmbTaskScriptProvider = null!, cmbTaskResultProvider = null!;
    private TextBox txtTaskTestCaseModel = null!, txtTaskScriptModel = null!, txtTaskResultModel = null!;

    // Playwright
    private TextBox txtNodePath = null!;
    private CheckBox chkWriteBack = null!;

    // Output
    private TextBox txtTestCasesDir = null!, txtScriptsDir = null!;
    private NumericUpDown nudMaxHistory = null!;

    // Footer
    private Button btnOK = null!, btnCancel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        Text          = "設定";
        Size          = new Size(640, 700);
        MinimumSize   = new Size(540, 600);
        StartPosition = FormStartPosition.CenterParent;
        Font          = new Font("Microsoft JhengHei UI", 9.5f);

        var tabs = new TabControl { Dock = DockStyle.Fill };

        tabs.TabPages.Add(BuildRedmineTab());
        tabs.TabPages.Add(BuildAITab());
        tabs.TabPages.Add(BuildAITaskTab());
        tabs.TabPages.Add(BuildPlaywrightTab());
        tabs.TabPages.Add(BuildOutputTab());

        var footer = new FlowLayoutPanel
        {
            Dock          = DockStyle.Bottom,
            Height        = 48,
            FlowDirection = FlowDirection.RightToLeft,
            Padding       = new Padding(8)
        };
        btnCancel = new Button { Text = "取消", Width = 80, DialogResult = DialogResult.Cancel };
        btnOK     = new Button { Text = "儲存", Width = 80, BackColor = Color.FromArgb(37,99,168), ForeColor = Color.White };
        btnOK.Click     += btnOK_Click;
        btnCancel.Click += btnCancel_Click;
        footer.Controls.AddRange(new Control[] { btnCancel, btnOK });

        Controls.Add(tabs);
        Controls.Add(footer);
        AcceptButton = btnOK;
        CancelButton = btnCancel;
    }

    private TabPage BuildRedmineTab()
    {
        var tab = new TabPage("Redmine");
        var tbl = MakeTable(6);

        AddRow(tbl, 0, "Redmine URL：",  txtRedmineUrl  = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "http://192.168.1.100/redmine" });
        AddRow(tbl, 1, "API Key：",      txtRedmineKey  = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true });
        AddRow(tbl, 2, "預設 Project ID：", txtProjectId = new TextBox { Dock = DockStyle.Fill });
        AddRow(tbl, 3, "逾時（秒）：",   nudTimeout     = new NumericUpDown { Minimum = 5, Maximum = 300, Value = 30 });
        chkIgnoreSsl = new CheckBox { Text = "忽略 SSL 憑證錯誤（自簽憑證）", AutoSize = true };
        tbl.Controls.Add(chkIgnoreSsl, 1, 4);
        btnTestRedmine = new Button { Text = "測試連線", AutoSize = true };
        btnTestRedmine.Click += btnTestRedmine_Click;
        tbl.Controls.Add(btnTestRedmine, 1, 5);

        tab.Controls.Add(tbl);
        return tab;
    }

    private TabPage BuildAITab()
    {
        var tab = new TabPage("AI");
        var tbl = MakeTable(11);

        cmbProvider = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
        cmbProvider.Items.AddRange(new[] { "Claude", "OpenAI", "Gemini", "Ollama" });
        cmbProvider.SelectedIndex = 0;

        AddRow(tbl, 0, "Provider：",             cmbProvider);
        AddRow(tbl, 1, "Claude API Key：",        txtClaudeKey   = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true });
        AddRow(tbl, 2, "Claude Model：",          txtClaudeModel = new TextBox { Dock = DockStyle.Fill, Text = "claude-sonnet-4-5" });
        AddRow(tbl, 3, "OpenAI API Key：",        txtOpenAIKey   = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true });
        AddRow(tbl, 4, "OpenAI Model：",          txtOpenAIModel = new TextBox { Dock = DockStyle.Fill, Text = "gpt-4o" });
        AddRow(tbl, 5, "Gemini API Key：",        txtGeminiKey   = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true });
        AddRow(tbl, 6, "Gemini Model：",          txtGeminiModel = new TextBox { Dock = DockStyle.Fill, Text = "gemini-2.5-flash" });
        AddRow(tbl, 7, "Max Tokens：",            nudMaxTokens   = new NumericUpDown { Minimum = 256, Maximum = 32768, Value = 4096, Increment = 256 });
        AddRow(tbl, 8, "Ollama URL：",            txtOllamaUrl   = new TextBox { Dock = DockStyle.Fill, Text = "http://localhost:11434" });
        AddRow(tbl, 9, "Ollama Model：",          txtOllamaModel = new TextBox { Dock = DockStyle.Fill, Text = "qwen2.5-coder:7b" });

        btnTestAI = new Button { Text = "測試 AI 連線", AutoSize = true };
        btnTestAI.Click += btnTestAI_Click;
        tbl.Controls.Add(btnTestAI, 1, 10);

        tab.Controls.Add(tbl);
        return tab;
    }

    private TabPage BuildPlaywrightTab()
    {
        var tab = new TabPage("Playwright");
        var tbl = MakeTable(3);

        AddRow(tbl, 0, "Node.js 路徑：", txtNodePath  = new TextBox { Dock = DockStyle.Fill, Text = "node" });
        chkWriteBack = new CheckBox { Text = "測試完成後自動回寫結果至 Redmine Journal", AutoSize = true };
        tbl.Controls.Add(chkWriteBack, 1, 1);

        tab.Controls.Add(tbl);
        return tab;
    }

    private TabPage BuildAITaskTab()
    {
        var tab = new TabPage("任務 AI 指派");
        var tbl = MakeTable(7);

        // 說明列
        var hint = new Label
        {
            Text      = "每個任務可獨立指定 AI，留空表示沿用上方全域設定。",
            Dock      = DockStyle.Fill,
            ForeColor = Color.FromArgb(180, 200, 255),
            TextAlign = ContentAlignment.MiddleLeft
        };
        tbl.SetColumnSpan(hint, 2);
        tbl.Controls.Add(hint, 0, 0);

        static ComboBox MakeProviderCombo()
        {
            var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            c.Items.AddRange(new[] { "（沿用全域）", "Claude", "OpenAI", "Gemini", "Ollama" });
            c.SelectedIndex = 0;
            return c;
        }

        AddRow(tbl, 1, "① 生成測試案例 Provider：", cmbTaskTestCaseProvider = MakeProviderCombo());
        AddRow(tbl, 2, "① 生成測試案例 Model：",    txtTaskTestCaseModel    = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "留空沿用全域" });
        AddRow(tbl, 3, "② 撰寫腳本 Provider：",     cmbTaskScriptProvider   = MakeProviderCombo());
        AddRow(tbl, 4, "② 撰寫腳本 Model：",        txtTaskScriptModel      = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "留空沿用全域" });
        AddRow(tbl, 5, "③ 寫回結果 Provider：",     cmbTaskResultProvider   = MakeProviderCombo());
        AddRow(tbl, 6, "③ 寫回結果 Model：",        txtTaskResultModel      = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "留空沿用全域" });

        tab.Controls.Add(tbl);
        return tab;
    }

    private TabPage BuildOutputTab()
    {
        var tab = new TabPage("輸出路徑");
        var tbl = MakeTable(4);

        AddRow(tbl, 0, "測試案例目錄：", txtTestCasesDir = new TextBox { Dock = DockStyle.Fill, Text = "./TestCases" });
        AddRow(tbl, 1, "腳本輸出目錄：", txtScriptsDir   = new TextBox { Dock = DockStyle.Fill, Text = "./GeneratedScripts" });
        AddRow(tbl, 2, "保留歷史筆數：", nudMaxHistory   = new NumericUpDown { Minimum = 1, Maximum = 50, Value = 10 });

        tab.Controls.Add(tbl);
        return tab;
    }

    private static TableLayoutPanel MakeTable(int rows)
    {
        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = rows, Padding = new Padding(12)
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < rows; i++)
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        return tbl;
    }

    private static void AddRow(TableLayoutPanel tbl, int row, string label, Control control)
    {
        tbl.Controls.Add(new Label
        {
            Text = label, Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 8, 0)
        }, 0, row);
        tbl.Controls.Add(control, 1, row);
    }
}
