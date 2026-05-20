namespace TestFlowStudio.WinForms.Forms
{
    partial class PlaywrightChatForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.txtChatHistory = new System.Windows.Forms.RichTextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtUserInput = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnRunTest = new System.Windows.Forms.Button();
            this.btnSaveScript = new System.Windows.Forms.Button();
            this.btnExportReport = new System.Windows.Forms.Button();
            this.lblTestName = new System.Windows.Forms.Label();
            this.txtTestOutput = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 50);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.txtChatHistory);
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtTestOutput);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Size = new System.Drawing.Size(1200, 600);
            this.splitContainer1.SplitterDistance = 600;
            this.splitContainer1.TabIndex = 0;
            // 
            // txtChatHistory
            // 
            this.txtChatHistory.BackColor = System.Drawing.Color.White;
            this.txtChatHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtChatHistory.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtChatHistory.Location = new System.Drawing.Point(0, 0);
            this.txtChatHistory.Name = "txtChatHistory";
            this.txtChatHistory.ReadOnly = true;
            this.txtChatHistory.Size = new System.Drawing.Size(600, 510);
            this.txtChatHistory.TabIndex = 0;
            this.txtChatHistory.Text = "";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnSend);
            this.panel1.Controls.Add(this.txtUserInput);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 510);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(5);
            this.panel1.Size = new System.Drawing.Size(600, 90);
            this.panel1.TabIndex = 1;
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSend.Font = new System.Drawing.Font("Microsoft JhengHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSend.ForeColor = System.Drawing.Color.White;
            this.btnSend.Location = new System.Drawing.Point(490, 8);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(100, 74);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "發送";
            this.btnSend.UseVisualStyleBackColor = false;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtUserInput
            // 
            this.txtUserInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtUserInput.Font = new System.Drawing.Font("Microsoft JhengHei UI", 10F);
            this.txtUserInput.Location = new System.Drawing.Point(8, 8);
            this.txtUserInput.Multiline = true;
            this.txtUserInput.Name = "txtUserInput";
            this.txtUserInput.Size = new System.Drawing.Size(476, 74);
            this.txtUserInput.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.cmbTestFiles);
            this.panel2.Controls.Add(this.btnRefreshTests);
            this.panel2.Controls.Add(this.btnExportReport);
            this.panel2.Controls.Add(this.btnSaveScript);
            this.panel2.Controls.Add(this.btnRunTest);
            this.panel2.Controls.Add(this.lblTestName);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(10);
            this.panel2.Size = new System.Drawing.Size(1200, 50);
            this.panel2.TabIndex = 1;
            // 
            // btnRunTest
            // 
            this.btnRunTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRunTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(136)))));
            this.btnRunTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRunTest.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRunTest.ForeColor = System.Drawing.Color.White;
            this.btnRunTest.Location = new System.Drawing.Point(890, 10);
            this.btnRunTest.Name = "btnRunTest";
            this.btnRunTest.Size = new System.Drawing.Size(90, 30);
            this.btnRunTest.TabIndex = 1;
            this.btnRunTest.Text = "🚀 執行測試";
            this.btnRunTest.UseVisualStyleBackColor = false;
            this.btnRunTest.Click += new System.EventHandler(this.btnRunTest_Click);
            // 
            // cmbTestFiles
            // 
            this.cmbTestFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbTestFiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTestFiles.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.cmbTestFiles.FormattingEnabled = true;
            this.cmbTestFiles.Location = new System.Drawing.Point(550, 12);
            this.cmbTestFiles.Name = "cmbTestFiles";
            this.cmbTestFiles.Size = new System.Drawing.Size(250, 27);
            this.cmbTestFiles.TabIndex = 4;
            // 
            // btnRefreshTests
            // 
            this.btnRefreshTests.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefreshTests.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
            this.btnRefreshTests.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefreshTests.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.btnRefreshTests.ForeColor = System.Drawing.Color.White;
            this.btnRefreshTests.Location = new System.Drawing.Point(810, 10);
            this.btnRefreshTests.Name = "btnRefreshTests";
            this.btnRefreshTests.Size = new System.Drawing.Size(70, 30);
            this.btnRefreshTests.TabIndex = 5;
            this.btnRefreshTests.Text = "🔄 重整";
            this.btnRefreshTests.UseVisualStyleBackColor = false;
            this.btnRefreshTests.Click += new System.EventHandler(this.btnRefreshTests_Click);
            // 
            // btnSaveScript
            // 
            this.btnSaveScript.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveScript.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(125)))), ((int)(((byte)(139)))));
            this.btnSaveScript.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveScript.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.btnSaveScript.ForeColor = System.Drawing.Color.White;
            this.btnSaveScript.Location = new System.Drawing.Point(990, 10);
            this.btnSaveScript.Name = "btnSaveScript";
            this.btnSaveScript.Size = new System.Drawing.Size(90, 30);
            this.btnSaveScript.TabIndex = 2;
            this.btnSaveScript.Text = "💾 儲存腳本";
            this.btnSaveScript.UseVisualStyleBackColor = false;
            this.btnSaveScript.Click += new System.EventHandler(this.btnSaveScript_Click);
            // 
            // btnExportReport
            // 
            this.btnExportReport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportReport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.btnExportReport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportReport.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.btnExportReport.ForeColor = System.Drawing.Color.White;
            this.btnExportReport.Location = new System.Drawing.Point(1090, 10);
            this.btnExportReport.Name = "btnExportReport";
            this.btnExportReport.Size = new System.Drawing.Size(100, 30);
            this.btnExportReport.TabIndex = 3;
            this.btnExportReport.Text = "📄 匯出報告";
            this.btnExportReport.UseVisualStyleBackColor = false;
            this.btnExportReport.Click += new System.EventHandler(this.btnExportReport_Click);
            // 
            // lblTestName
            // 
            this.lblTestName.AutoSize = true;
            this.lblTestName.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTestName.Location = new System.Drawing.Point(10, 15);
            this.lblTestName.Name = "lblTestName";
            this.lblTestName.Size = new System.Drawing.Size(172, 20);
            this.lblTestName.TabIndex = 0;
            this.lblTestName.Text = "🤖 Playwright AI 助手";
            // 
            // txtTestOutput
            // 
            this.txtTestOutput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.txtTestOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTestOutput.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtTestOutput.ForeColor = System.Drawing.Color.Lime;
            this.txtTestOutput.Location = new System.Drawing.Point(0, 25);
            this.txtTestOutput.Name = "txtTestOutput";
            this.txtTestOutput.ReadOnly = true;
            this.txtTestOutput.Size = new System.Drawing.Size(596, 575);
            this.txtTestOutput.TabIndex = 0;
            this.txtTestOutput.Text = "";
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(5);
            this.label1.Size = new System.Drawing.Size(596, 25);
            this.label1.TabIndex = 1;
            this.label1.Text = "📊 測試執行輸出";
            // 
            // PlaywrightChatForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 650);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel2);
            this.Name = "PlaywrightChatForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Playwright AI 測試助手";
            this.Load += new System.EventHandler(this.PlaywrightChatForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.RichTextBox txtChatHistory;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox txtUserInput;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblTestName;
        private System.Windows.Forms.Button btnRunTest;
        private System.Windows.Forms.Button btnSaveScript;
        private System.Windows.Forms.Button btnExportReport;
        private System.Windows.Forms.RichTextBox txtTestOutput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbTestFiles;
        private System.Windows.Forms.Button btnRefreshTests;
    }
}
