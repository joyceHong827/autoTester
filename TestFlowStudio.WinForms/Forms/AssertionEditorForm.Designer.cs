using TestFlowStudio.Core.Models;

namespace TestFlowStudio.WinForms.Forms;

partial class AssertionEditorForm
{
    private System.ComponentModel.IContainer components = null!;

    private DataGridView dgvRules = null!;
    private RichTextBox  rtbPromptPreview = null!;
    private Button btnAddRow = null!, btnDeleteRow = null!,
                   btnMoveUp = null!, btnMoveDown = null!,
                   btnImportFromClipboard = null!,
                   btnOK = null!, btnCancel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();

        // ── Form ──────────────────────────────────────────────────────────
        Text          = "驗證條件編輯器（Assertion Rules）";
        Size          = new Size(1100, 700);
        MinimumSize   = new Size(900, 560);
        StartPosition = FormStartPosition.CenterParent;
        Font          = new Font("Microsoft JhengHei UI", 9.5f);
        BackColor     = Color.FromArgb(245, 247, 250);

        // ── Main layout: top toolbar | middle split | bottom footer ───────
        var mainLayout = new TableLayoutPanel
        {
            Dock      = DockStyle.Fill,
            RowCount  = 3,
            ColumnCount = 1
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));   // toolbar
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // content
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));   // footer

        // ── Toolbar ───────────────────────────────────────────────────────
        var toolbar = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding       = new Padding(6, 8, 6, 0),
            WrapContents  = false
        };

        btnAddRow = MakeToolBtn("＋ 新增條件", Color.ForestGreen);
        btnAddRow.Click += new EventHandler(btnAddRow_Click);

        btnDeleteRow = MakeToolBtn("✕ 刪除", Color.Crimson);
        btnDeleteRow.Click += new EventHandler(btnDeleteRow_Click);

        btnMoveUp = MakeToolBtn("↑ 上移", null);
        btnMoveUp.Click += new EventHandler(btnMoveUp_Click);

        btnMoveDown = MakeToolBtn("↓ 下移", null);
        btnMoveDown.Click += new EventHandler(btnMoveDown_Click);

        btnImportFromClipboard = MakeToolBtn("📋 從剪貼簿匯入選取器", null);
        btnImportFromClipboard.Click += new EventHandler(btnImportFromClipboard_Click);

        var helpLabel = new Label
        {
            Text      = "提示：定義驗證條件後，AI 轉換腳本時會自動產生對應的 expect() assertion",
            AutoSize  = true,
            ForeColor = Color.Gray,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin    = new Padding(16, 6, 0, 0)
        };

        toolbar.Controls.Add(btnAddRow);
        toolbar.Controls.Add(btnDeleteRow);
        toolbar.Controls.Add(btnMoveUp);
        toolbar.Controls.Add(btnMoveDown);
        toolbar.Controls.Add(new Label { Width = 16, Text = "" });
        toolbar.Controls.Add(btnImportFromClipboard);
        toolbar.Controls.Add(helpLabel);

        // ── Content: left grid | right preview ───────────────────────────
        var splitContent = new SplitContainer
        {
            Dock        = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 380,
            Panel1MinSize    = 200,
            Panel2MinSize    = 120
        };

        // ── DataGridView ──────────────────────────────────────────────────
        dgvRules = new DataGridView
        {
            Dock                  = DockStyle.Fill,
            AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
            AllowUserToAddRows    = false,
            AllowUserToDeleteRows = false,
            MultiSelect           = false,
            RowHeadersWidth       = 32,
            BackgroundColor       = Color.White,
            GridColor             = Color.FromArgb(220, 220, 220),
            BorderStyle           = BorderStyle.None,
            EditMode              = DataGridViewEditMode.EditOnKeystrokeOrF2,
            Font                  = new Font("Microsoft JhengHei UI", 9f)
        };
        dgvRules.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 58, 92);
        dgvRules.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvRules.ColumnHeadersDefaultCellStyle.Font      = new Font("Microsoft JhengHei UI", 9f, FontStyle.Bold);
        dgvRules.EnableHeadersVisualStyles = false;

        // ── Columns ───────────────────────────────────────────────────────

        // 啟用 checkbox
        var colEnabled = new DataGridViewCheckBoxColumn
        {
            Name       = "colEnabled",
            HeaderText = "啟用",
            Width      = 50,
            FillWeight = 5,
            FlatStyle  = FlatStyle.Standard
        };

        // 條件名稱
        var colName = new DataGridViewTextBoxColumn
        {
            Name       = "colName",
            HeaderText = "條件名稱",
            FillWeight = 15
        };

        // 定位方式 (ComboBox)
        var colLocatorType = new DataGridViewComboBoxColumn
        {
            Name       = "colLocatorType",
            HeaderText = "定位方式",
            FillWeight = 12,
            FlatStyle  = FlatStyle.Flat
        };
        foreach (var t in AssertionRule.LocatorTypes)
            colLocatorType.Items.Add(t);
        colLocatorType.DefaultCellStyle.NullValue = "CSS";

        // 定位值
        var colLocatorValue = new DataGridViewTextBoxColumn
        {
            Name       = "colLocatorValue",
            HeaderText = "定位值（Selector）",
            FillWeight = 22
        };

        // 驗證類型 (ComboBox)
        var colAssertionType = new DataGridViewComboBoxColumn
        {
            Name       = "colAssertionType",
            HeaderText = "驗證類型",
            FillWeight = 15,
            FlatStyle  = FlatStyle.Flat
        };
        foreach (var t in AssertionRule.AssertionTypes)
            colAssertionType.Items.Add(t);
        colAssertionType.DefaultCellStyle.NullValue = "TextEquals";

        // 期望值
        var colExpectedValue = new DataGridViewTextBoxColumn
        {
            Name       = "colExpectedValue",
            HeaderText = "期望值 / 內容",
            FillWeight = 20
        };

        // 備註
        var colNote = new DataGridViewTextBoxColumn
        {
            Name       = "colNote",
            HeaderText = "備註（寫入程式碼註解）",
            FillWeight = 16
        };

        dgvRules.Columns.AddRange(
            colEnabled, colName, colLocatorType, colLocatorValue,
            colAssertionType, colExpectedValue, colNote);

        dgvRules.CellValueChanged   += new DataGridViewCellEventHandler(dgvRules_CellValueChanged);
        dgvRules.CurrentCellChanged += new EventHandler(dgvRules_CurrentCellChanged);
        // Immediately commit checkbox changes
        dgvRules.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (dgvRules.CurrentCell is DataGridViewCheckBoxCell)
                dgvRules.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };

        // ── Alternating row colors ────────────────────────────────────────
        dgvRules.RowsDefaultCellStyle.BackColor          = Color.White;
        dgvRules.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 245, 255);

        splitContent.Panel1.Controls.Add(dgvRules);

        // ── Preview pane ──────────────────────────────────────────────────
        var previewHeader = new Label
        {
            Text      = "預覽：AI Prompt 補充 & 對應 TypeScript 程式碼",
            Dock      = DockStyle.Top,
            Height    = 26,
            Font      = new Font("Microsoft JhengHei UI", 9f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(30, 58, 92),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(8, 0, 0, 0)
        };

        rtbPromptPreview = new RichTextBox
        {
            Dock      = DockStyle.Fill,
            ReadOnly  = true,
            Font      = new Font("Consolas", 9.5f),
            BackColor = Color.FromArgb(18, 18, 30),
            ForeColor = Color.FromArgb(200, 200, 200),
            BorderStyle = BorderStyle.None
        };

        splitContent.Panel2.Controls.Add(rtbPromptPreview);
        splitContent.Panel2.Controls.Add(previewHeader);

        // ── Footer ────────────────────────────────────────────────────────
        var footer = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding       = new Padding(8, 8, 8, 0)
        };

        btnCancel = new Button
        {
            Text      = "取消",
            Width     = 88,
            Height    = 32,
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.Click += new EventHandler(btnCancel_Click);

        btnOK = new Button
        {
            Text      = "✔ 套用條件",
            Width     = 110,
            Height    = 32,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(37, 99, 168),
            ForeColor = Color.White
        };
        btnOK.Click += new EventHandler(btnOK_Click);

        footer.Controls.Add(btnCancel);
        footer.Controls.Add(btnOK);

        // ── Assemble ──────────────────────────────────────────────────────
        mainLayout.Controls.Add(toolbar,       0, 0);
        mainLayout.Controls.Add(splitContent,  0, 1);
        mainLayout.Controls.Add(footer,        0, 2);

        Controls.Add(mainLayout);
        AcceptButton = btnOK;
        CancelButton = btnCancel;
        ResumeLayout(false);
    }

    private static Button MakeToolBtn(string text, Color? bg)
    {
        var btn = new Button
        {
            Text      = text,
            AutoSize  = true,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Microsoft JhengHei UI", 9f),
            Margin    = new Padding(3, 0, 3, 0),
            Cursor    = Cursors.Hand,
            Height    = 30
        };
        if (bg.HasValue)
        {
            btn.BackColor = bg.Value;
            btn.ForeColor = Color.White;
        }
        return btn;
    }
}
