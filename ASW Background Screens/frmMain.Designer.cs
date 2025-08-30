using System.Drawing;

namespace ASW_Background_Screens
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.txtDuration = new System.Windows.Forms.TextBox();
            this.lblDuration = new System.Windows.Forms.Label();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openConfigFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openPrintListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearFormSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblMyScreenOrder = new System.Windows.Forms.Label();
            this.txtMyScreenOrder = new System.Windows.Forms.TextBox();
            this.tableMonitors = new System.Windows.Forms.TableLayoutPanel();
            this.cbFillStyle = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkIncludeSubfolders = new System.Windows.Forms.CheckBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtDuration
            // 
            this.txtDuration.Location = new System.Drawing.Point(334, 403);
            this.txtDuration.Name = "txtDuration";
            this.txtDuration.Size = new System.Drawing.Size(155, 47);
            this.txtDuration.TabIndex = 8;
            this.txtDuration.Validating += new System.ComponentModel.CancelEventHandler(this.txtDuration_Validating);
            // 
            // lblDuration
            // 
            this.lblDuration.AutoSize = true;
            this.lblDuration.Location = new System.Drawing.Point(12, 403);
            this.lblDuration.Name = "lblDuration";
            this.lblDuration.Size = new System.Drawing.Size(268, 41);
            this.lblDuration.TabIndex = 1;
            this.lblDuration.Text = "Duration (seconds)";
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(1128, 505);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(241, 75);
            this.btnStop.TabIndex = 9;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(1128, 405);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(241, 75);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1413, 49);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(87, 45);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(230, 54);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openConfigFileToolStripMenuItem,
            this.openPrintListToolStripMenuItem,
            this.clearFormSettingsToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(111, 45);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // openConfigFileToolStripMenuItem
            // 
            this.openConfigFileToolStripMenuItem.Name = "openConfigFileToolStripMenuItem";
            this.openConfigFileToolStripMenuItem.Size = new System.Drawing.Size(448, 54);
            this.openConfigFileToolStripMenuItem.Text = "Open Config File";
            this.openConfigFileToolStripMenuItem.Click += new System.EventHandler(this.openConfigFileToolStripMenuItem_Click);
            // 
            // openPrintListToolStripMenuItem
            // 
            this.openPrintListToolStripMenuItem.Name = "openPrintListToolStripMenuItem";
            this.openPrintListToolStripMenuItem.Size = new System.Drawing.Size(448, 54);
            this.openPrintListToolStripMenuItem.Text = "Open Print List";
            this.openPrintListToolStripMenuItem.Click += new System.EventHandler(this.openPrintListToolStripMenuItem_Click);
            // 
            // clearFormSettingsToolStripMenuItem
            // 
            this.clearFormSettingsToolStripMenuItem.Name = "clearFormSettingsToolStripMenuItem";
            this.clearFormSettingsToolStripMenuItem.Size = new System.Drawing.Size(448, 54);
            this.clearFormSettingsToolStripMenuItem.Text = "Reset Form Settings";
            this.clearFormSettingsToolStripMenuItem.Click += new System.EventHandler(this.resetFormSettingsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(104, 45);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(266, 54);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // lblMyScreenOrder
            // 
            this.lblMyScreenOrder.AutoSize = true;
            this.lblMyScreenOrder.Location = new System.Drawing.Point(12, 471);
            this.lblMyScreenOrder.Name = "lblMyScreenOrder";
            this.lblMyScreenOrder.Size = new System.Drawing.Size(234, 41);
            this.lblMyScreenOrder.TabIndex = 1;
            this.lblMyScreenOrder.Text = "My screen order";
            // 
            // txtMyScreenOrder
            // 
            this.txtMyScreenOrder.Location = new System.Drawing.Point(334, 471);
            this.txtMyScreenOrder.Name = "txtMyScreenOrder";
            this.txtMyScreenOrder.Size = new System.Drawing.Size(216, 47);
            this.txtMyScreenOrder.TabIndex = 10;
            this.txtMyScreenOrder.TabStop = false;
            this.txtMyScreenOrder.Validated += new System.EventHandler(this.txtMyScreenOrder_Validated);
            // 
            // tableMonitors
            // 
            this.tableMonitors.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableMonitors.ColumnCount = 3;
            this.tableMonitors.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableMonitors.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableMonitors.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableMonitors.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableMonitors.Location = new System.Drawing.Point(0, 49);
            this.tableMonitors.Name = "tableMonitors";
            this.tableMonitors.RowCount = 4;
            this.tableMonitors.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMonitors.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMonitors.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMonitors.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMonitors.Size = new System.Drawing.Size(1413, 338);
            this.tableMonitors.TabIndex = 11;
            // 
            // cbFillStyle
            // 
            this.cbFillStyle.FormattingEnabled = true;
            this.cbFillStyle.Location = new System.Drawing.Point(766, 463);
            this.cbFillStyle.Name = "cbFillStyle";
            this.cbFillStyle.Size = new System.Drawing.Size(264, 49);
            this.cbFillStyle.TabIndex = 12;
            this.cbFillStyle.SelectedIndexChanged += new System.EventHandler(this.cbFillStyle_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(759, 403);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(121, 41);
            this.label2.TabIndex = 1;
            this.label2.Text = "Fit Style";
            // 
            // chkIncludeSubfolders
            // 
            this.chkIncludeSubfolders.AutoSize = true;
            this.chkIncludeSubfolders.Location = new System.Drawing.Point(19, 535);
            this.chkIncludeSubfolders.Name = "chkIncludeSubfolders";
            this.chkIncludeSubfolders.Size = new System.Drawing.Size(316, 45);
            this.chkIncludeSubfolders.TabIndex = 13;
            this.chkIncludeSubfolders.Text = "Include Subfolders?";
            this.chkIncludeSubfolders.UseVisualStyleBackColor = true;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(17F, 41F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1413, 597);
            this.Controls.Add(this.chkIncludeSubfolders);
            this.Controls.Add(this.cbFillStyle);
            this.Controls.Add(this.tableMonitors);
            this.Controls.Add(this.txtMyScreenOrder);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblMyScreenOrder);
            this.Controls.Add(this.lblDuration);
            this.Controls.Add(this.txtDuration);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmMain";
            this.Text = "ASW Background Scenes";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.DoubleClick += new System.EventHandler(this.frmMain_DoubleClick);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmMain_KeyDown);
            this.Resize += new System.EventHandler(this.frmMain_Resize);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtDuration;
        private System.Windows.Forms.Label lblDuration;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openConfigFileToolStripMenuItem;
        private System.Windows.Forms.Label lblMyScreenOrder;
        private System.Windows.Forms.TextBox txtMyScreenOrder;
        private System.Windows.Forms.ToolStripMenuItem openPrintListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableMonitors;
        private System.Windows.Forms.ToolStripMenuItem clearFormSettingsToolStripMenuItem;
        private System.Windows.Forms.ComboBox cbFillStyle;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkIncludeSubfolders;
    }
}