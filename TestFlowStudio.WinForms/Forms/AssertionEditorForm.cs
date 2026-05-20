using TestFlowStudio.Core.Models;

namespace TestFlowStudio.WinForms.Forms;

/// <summary>
/// 驗證條件編輯器：讓使用者定義要驗證的 UI 元素、欄位值、頁面狀態等 assertion 條件，
/// 這些條件會附加給 AI，讓 AI 在轉換 Playwright 腳本時產生對應的 expect()。
/// </summary>
public partial class AssertionEditorForm : Form
{
    public List<AssertionRule> Rules { get; private set; } = new();

    public AssertionEditorForm(List<AssertionRule>? existingRules = null)
    {
        InitializeComponent();
        if (existingRules != null)
            Rules = new List<AssertionRule>(existingRules);
        RefreshGrid();
    }

    // ── Grid refresh ──────────────────────────────────────────────────────

    private void RefreshGrid()
    {
        dgvRules.Rows.Clear();
        foreach (var r in Rules)
        {
            var row = dgvRules.Rows[dgvRules.Rows.Add()];
            row.Cells["colEnabled"].Value       = r.Enabled;
            row.Cells["colName"].Value          = r.Name;
            row.Cells["colLocatorType"].Value   = r.LocatorType;
            row.Cells["colLocatorValue"].Value  = r.LocatorValue;
            row.Cells["colAssertionType"].Value = r.AssertionType;
            row.Cells["colExpectedValue"].Value = r.ExpectedValue;
            row.Cells["colNote"].Value          = r.Note;
            row.Tag = r;
        }
        UpdatePreview();
    }

    // ── Toolbar handlers ──────────────────────────────────────────────────

    private void btnAddRow_Click(object sender, EventArgs e)
    {
        var rule = new AssertionRule
        {
            Name          = $"條件 {Rules.Count + 1}",
            LocatorType   = "CSS",
            AssertionType = "TextEquals",
            Enabled       = true
        };
        Rules.Add(rule);
        RefreshGrid();
        // Select the new row and start editing Name
        if (dgvRules.Rows.Count > 0)
        {
            dgvRules.CurrentCell = dgvRules.Rows[dgvRules.Rows.Count - 1].Cells["colName"];
            dgvRules.BeginEdit(true);
        }
    }

    private void btnDeleteRow_Click(object sender, EventArgs e)
    {
        if (dgvRules.CurrentRow == null) return;
        var idx = dgvRules.CurrentRow.Index;
        if (idx >= 0 && idx < Rules.Count)
        {
            Rules.RemoveAt(idx);
            RefreshGrid();
        }
    }

    private void btnMoveUp_Click(object sender, EventArgs e)
    {
        var idx = dgvRules.CurrentRow?.Index ?? -1;
        if (idx <= 0) return;
        (Rules[idx], Rules[idx - 1]) = (Rules[idx - 1], Rules[idx]);
        RefreshGrid();
        dgvRules.CurrentCell = dgvRules.Rows[idx - 1].Cells[0];
    }

    private void btnMoveDown_Click(object sender, EventArgs e)
    {
        var idx = dgvRules.CurrentRow?.Index ?? -1;
        if (idx < 0 || idx >= Rules.Count - 1) return;
        (Rules[idx], Rules[idx + 1]) = (Rules[idx + 1], Rules[idx]);
        RefreshGrid();
        dgvRules.CurrentCell = dgvRules.Rows[idx + 1].Cells[0];
    }

    private void btnImportFromClipboard_Click(object sender, EventArgs e)
    {
        // Paste CSS selectors line by line from clipboard
        var text = Clipboard.GetText();
        if (string.IsNullOrWhiteSpace(text)) return;

        foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var sel = line.Trim();
            if (string.IsNullOrEmpty(sel)) continue;
            Rules.Add(new AssertionRule
            {
                Name         = sel,
                LocatorType  = sel.StartsWith('/') ? "XPath" : "CSS",
                LocatorValue = sel,
                AssertionType = "IsVisible",
                Enabled      = true
            });
        }
        RefreshGrid();
    }

    // ── Grid events ───────────────────────────────────────────────────────

    private void dgvRules_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= Rules.Count) return;
        SyncRowToModel(e.RowIndex);
        UpdatePreview();
    }

    private void dgvRules_CurrentCellChanged(object sender, EventArgs e)
    {
        UpdatePreview();
        UpdateExpectedValueVisibility();
    }

    private void SyncRowToModel(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= Rules.Count) return;
        var row = dgvRules.Rows[rowIndex];
        var r   = Rules[rowIndex];

        r.Enabled       = row.Cells["colEnabled"].Value is true;
        r.Name          = row.Cells["colName"].Value?.ToString()          ?? "";
        r.LocatorType   = row.Cells["colLocatorType"].Value?.ToString()   ?? "CSS";
        r.LocatorValue  = row.Cells["colLocatorValue"].Value?.ToString()  ?? "";
        r.AssertionType = row.Cells["colAssertionType"].Value?.ToString() ?? "TextEquals";
        r.ExpectedValue = row.Cells["colExpectedValue"].Value?.ToString() ?? "";
        r.Note          = row.Cells["colNote"].Value?.ToString()          ?? "";
    }

    private void UpdateExpectedValueVisibility()
    {
        if (dgvRules.CurrentRow == null) return;
        var idx = dgvRules.CurrentRow.Index;
        if (idx < 0 || idx >= Rules.Count) return;
        SyncRowToModel(idx);
        // Grey-out ExpectedValue cell if not needed
        var cell = dgvRules.CurrentRow.Cells["colExpectedValue"];
        var needsVal = Rules[idx].NeedsExpectedValue;
        cell.Style.BackColor = needsVal
            ? dgvRules.DefaultCellStyle.BackColor
            : Color.FromArgb(235, 235, 235);
        cell.ReadOnly = !needsVal;
    }

    // ── Preview pane ──────────────────────────────────────────────────────

    private void UpdatePreview()
    {
        // Sync all rows first
        for (int i = 0; i < Math.Min(dgvRules.Rows.Count, Rules.Count); i++)
            SyncRowToModel(i);

        var activeRules = Rules.Where(r => r.Enabled).ToList();

        // Natural-language summary (for AI prompt)
        rtbPromptPreview.Clear();
        rtbPromptPreview.SelectionColor = Color.FromArgb(100, 149, 237);
        rtbPromptPreview.AppendText("── 自然語言描述（AI Prompt 補充）──\n");
        rtbPromptPreview.SelectionColor = rtbPromptPreview.ForeColor;
        for (int i = 0; i < activeRules.Count; i++)
            rtbPromptPreview.AppendText($"{i + 1}. {activeRules[i].ToPromptLine()}\n");

        rtbPromptPreview.AppendText("\n");
        rtbPromptPreview.SelectionColor = Color.FromArgb(100, 149, 237);
        rtbPromptPreview.AppendText("── 對應的 Playwright TypeScript 程式碼 ──\n");
        rtbPromptPreview.SelectionColor = Color.FromArgb(150, 220, 150);
        foreach (var r in activeRules)
            rtbPromptPreview.AppendText(r.ToPlaywrightCode() + "\n");
    }

    // ── OK / Cancel ───────────────────────────────────────────────────────

    private void btnOK_Click(object sender, EventArgs e)
    {
        for (int i = 0; i < Math.Min(dgvRules.Rows.Count, Rules.Count); i++)
            SyncRowToModel(i);
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
