namespace FolderWatcherWindowsServiceAdmin
{
    partial class FolderWatcherWindowsServiceAdmin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components;

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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageService = new System.Windows.Forms.TabPage();
            this.groupBoxService = new System.Windows.Forms.GroupBox();
            this.btnChangeStatus = new System.Windows.Forms.Button();
            this.btnUninstall = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.tabPageLogs = new System.Windows.Forms.TabPage();
            this.logViewerControl = new global::FolderWatcherWindowsServiceAdmin.Controls.LogViewerControl();
            this.tabControl1.SuspendLayout();
            this.tabPageService.SuspendLayout();
            this.groupBoxService.SuspendLayout();
            this.tabPageLogs.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageService);
            this.tabControl1.Controls.Add(this.tabPageLogs);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1200, 700);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageService
            // 
            this.tabPageService.Controls.Add(this.groupBoxService);
            this.tabPageService.Location = new System.Drawing.Point(4, 22);
            this.tabPageService.Name = "tabPageService";
            this.tabPageService.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageService.Size = new System.Drawing.Size(1192, 674);
            this.tabPageService.TabIndex = 0;
            this.tabPageService.Text = "Service Control";
            this.tabPageService.UseVisualStyleBackColor = true;
            // 
            // groupBoxService
            // 
            this.groupBoxService.Controls.Add(this.btnChangeStatus);
            this.groupBoxService.Controls.Add(this.btnUninstall);
            this.groupBoxService.Controls.Add(this.lblStatus);
            this.groupBoxService.Location = new System.Drawing.Point(9, 10);
            this.groupBoxService.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxService.Name = "groupBoxService";
            this.groupBoxService.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxService.Size = new System.Drawing.Size(570, 73);
            this.groupBoxService.TabIndex = 3;
            this.groupBoxService.TabStop = false;
            this.groupBoxService.Text = "Service Control";
            // 
            // btnChangeStatus
            // 
            this.btnChangeStatus.Location = new System.Drawing.Point(9, 18);
            this.btnChangeStatus.Margin = new System.Windows.Forms.Padding(2);
            this.btnChangeStatus.Name = "btnChangeStatus";
            this.btnChangeStatus.Size = new System.Drawing.Size(76, 19);
            this.btnChangeStatus.TabIndex = 1;
            this.btnChangeStatus.Text = "Start";
            this.btnChangeStatus.UseVisualStyleBackColor = true;
            this.btnChangeStatus.Click += new System.EventHandler(this.BtnChangeStatus_ClickAsync);
            // 
            // btnUninstall
            // 
            this.btnUninstall.Location = new System.Drawing.Point(9, 41);
            this.btnUninstall.Margin = new System.Windows.Forms.Padding(2);
            this.btnUninstall.Name = "btnUninstall";
            this.btnUninstall.Size = new System.Drawing.Size(76, 19);
            this.btnUninstall.TabIndex = 2;
            this.btnUninstall.Text = "Uninstall";
            this.btnUninstall.UseVisualStyleBackColor = true;
            this.btnUninstall.Click += new System.EventHandler(this.BtnUninstall_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(89, 20);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            this.lblStatus.TabIndex = 0;
            // 
            // tabPageLogs
            // 
            this.tabPageLogs.Controls.Add(this.logViewerControl);
            this.tabPageLogs.Location = new System.Drawing.Point(4, 22);
            this.tabPageLogs.Name = "tabPageLogs";
            this.tabPageLogs.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLogs.Size = new System.Drawing.Size(1192, 674);
            this.tabPageLogs.TabIndex = 1;
            this.tabPageLogs.Text = "Log Viewer";
            this.tabPageLogs.UseVisualStyleBackColor = true;
            // 
            // logViewerControl
            // 
            this.logViewerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logViewerControl.Location = new System.Drawing.Point(3, 3);
            this.logViewerControl.Name = "logViewerControl";
            this.logViewerControl.Size = new System.Drawing.Size(1186, 668);
            this.logViewerControl.TabIndex = 0;
            this.logViewerControl.Load += new System.EventHandler(this.logViewerControl_Load);
            // 
            // FolderWatcherWindowsServiceAdmin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "FolderWatcherWindowsServiceAdmin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Folder Watcher Windows Service Admin";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FolderWatcherWindowsServiceAdmin_FormClosing);
            this.Load += new System.EventHandler(this.FolderWatcherWindowsServiceAdmin_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPageService.ResumeLayout(false);
            this.groupBoxService.ResumeLayout(false);
            this.groupBoxService.PerformLayout();
            this.tabPageLogs.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageService;
        private System.Windows.Forms.TabPage tabPageLogs;
        private System.Windows.Forms.GroupBox groupBoxService;
        private System.Windows.Forms.Button btnChangeStatus;
        private System.Windows.Forms.Button btnUninstall;
        private System.Windows.Forms.Label lblStatus;
        private global::FolderWatcherWindowsServiceAdmin.Controls.LogViewerControl logViewerControl;
    }
}

