namespace FolderWatcherWindowsServiceAdmin
{
    partial class FolderWatcherWindowsServiceAdmin
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageService = new System.Windows.Forms.TabPage();
            this.groupBoxFolders = new System.Windows.Forms.GroupBox();
            this.listBoxFolders = new System.Windows.Forms.ListBox();
            this.btnAddFolder = new System.Windows.Forms.Button();
            this.btnRemoveFolder = new System.Windows.Forms.Button();
            this.btnEditFolder = new System.Windows.Forms.Button();
            this.lblConfigPath = new System.Windows.Forms.Label();
            this.lblConfigPathValue = new System.Windows.Forms.Label();
            this.groupBoxService = new System.Windows.Forms.GroupBox();
            this.btnChangeStatus = new System.Windows.Forms.Button();
            this.btnUninstall = new System.Windows.Forms.Button();
            this.btnAlertConfig = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.tabPageLogs = new System.Windows.Forms.TabPage();
            this.logViewerControl = new global::FolderWatcherWindowsServiceAdmin.Controls.LogViewerControl();
            this.tabControl1.SuspendLayout();
            this.tabPageService.SuspendLayout();
            this.groupBoxFolders.SuspendLayout();
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
            this.tabPageService.Controls.Add(this.groupBoxFolders);
            this.tabPageService.Controls.Add(this.groupBoxService);
            this.tabPageService.Location = new System.Drawing.Point(4, 22);
            this.tabPageService.Name = "tabPageService";
            this.tabPageService.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageService.Size = new System.Drawing.Size(1192, 674);
            this.tabPageService.TabIndex = 0;
            this.tabPageService.Text = "Service Control";
            this.tabPageService.UseVisualStyleBackColor = true;
            // 
            // groupBoxFolders
            // 
            this.groupBoxFolders.Controls.Add(this.listBoxFolders);
            this.groupBoxFolders.Controls.Add(this.btnAddFolder);
            this.groupBoxFolders.Controls.Add(this.btnRemoveFolder);
            this.groupBoxFolders.Controls.Add(this.btnEditFolder);
            this.groupBoxFolders.Controls.Add(this.lblConfigPath);
            this.groupBoxFolders.Controls.Add(this.lblConfigPathValue);
            this.groupBoxFolders.Location = new System.Drawing.Point(9, 88);
            this.groupBoxFolders.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxFolders.Name = "groupBoxFolders";
            this.groupBoxFolders.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxFolders.Size = new System.Drawing.Size(1175, 578);
            this.groupBoxFolders.TabIndex = 4;
            this.groupBoxFolders.TabStop = false;
            this.groupBoxFolders.Text = "Watched Folders Configuration";
            // 
            // listBoxFolders
            // 
            this.listBoxFolders.FormattingEnabled = true;
            this.listBoxFolders.Location = new System.Drawing.Point(9, 45);
            this.listBoxFolders.Margin = new System.Windows.Forms.Padding(2);
            this.listBoxFolders.Name = "listBoxFolders";
            this.listBoxFolders.SelectionMode = System.Windows.Forms.SelectionMode.One;
            this.listBoxFolders.Size = new System.Drawing.Size(1158, 472);
            this.listBoxFolders.TabIndex = 0;
            this.listBoxFolders.SelectedIndexChanged += new System.EventHandler(this.listBoxFolders_SelectedIndexChanged);
            // 
            // btnAddFolder
            // 
            this.btnAddFolder.Location = new System.Drawing.Point(9, 522);
            this.btnAddFolder.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddFolder.Name = "btnAddFolder";
            this.btnAddFolder.Size = new System.Drawing.Size(100, 25);
            this.btnAddFolder.TabIndex = 1;
            this.btnAddFolder.Text = "Add Folder";
            this.btnAddFolder.UseVisualStyleBackColor = true;
            this.btnAddFolder.Click += new System.EventHandler(this.btnAddFolder_Click);
            // 
            // btnRemoveFolder
            // 
            this.btnRemoveFolder.Enabled = false;
            this.btnRemoveFolder.Location = new System.Drawing.Point(114, 522);
            this.btnRemoveFolder.Margin = new System.Windows.Forms.Padding(2);
            this.btnRemoveFolder.Name = "btnRemoveFolder";
            this.btnRemoveFolder.Size = new System.Drawing.Size(100, 25);
            this.btnRemoveFolder.TabIndex = 2;
            this.btnRemoveFolder.Text = "Remove Folder";
            this.btnRemoveFolder.UseVisualStyleBackColor = true;
            this.btnRemoveFolder.Click += new System.EventHandler(this.btnRemoveFolder_Click);
            // 
            // btnEditFolder
            // 
            this.btnEditFolder.Enabled = false;
            this.btnEditFolder.Location = new System.Drawing.Point(219, 522);
            this.btnEditFolder.Margin = new System.Windows.Forms.Padding(2);
            this.btnEditFolder.Name = "btnEditFolder";
            this.btnEditFolder.Size = new System.Drawing.Size(100, 25);
            this.btnEditFolder.TabIndex = 3;
            this.btnEditFolder.Text = "Edit Folder";
            this.btnEditFolder.UseVisualStyleBackColor = true;
            this.btnEditFolder.Click += new System.EventHandler(this.btnEditFolder_Click);
            // 
            // lblConfigPath
            // 
            this.lblConfigPath.AutoSize = true;
            this.lblConfigPath.Location = new System.Drawing.Point(9, 18);
            this.lblConfigPath.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblConfigPath.Name = "lblConfigPath";
            this.lblConfigPath.Size = new System.Drawing.Size(94, 13);
            this.lblConfigPath.TabIndex = 4;
            this.lblConfigPath.Text = "Configuration File:";
            // 
            // lblConfigPathValue
            // 
            this.lblConfigPathValue.AutoSize = true;
            this.lblConfigPathValue.Location = new System.Drawing.Point(107, 18);
            this.lblConfigPathValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblConfigPathValue.Name = "lblConfigPathValue";
            this.lblConfigPathValue.Size = new System.Drawing.Size(0, 13);
            this.lblConfigPathValue.TabIndex = 5;
            // 
            // groupBoxService
            // 
            this.groupBoxService.Controls.Add(this.btnChangeStatus);
            this.groupBoxService.Controls.Add(this.btnUninstall);
            this.groupBoxService.Controls.Add(this.btnAlertConfig);
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
            // btnAlertConfig
            // 
            this.btnAlertConfig.Location = new System.Drawing.Point(89, 41);
            this.btnAlertConfig.Margin = new System.Windows.Forms.Padding(2);
            this.btnAlertConfig.Name = "btnAlertConfig";
            this.btnAlertConfig.Size = new System.Drawing.Size(100, 19);
            this.btnAlertConfig.TabIndex = 3;
            this.btnAlertConfig.Text = "Alert Settings...";
            this.btnAlertConfig.UseVisualStyleBackColor = true;
#if PREMIUM
            this.btnAlertConfig.Click += new System.EventHandler(this.btnAlertConfig_Click);
#else
            this.btnAlertConfig.Enabled = false;
            this.btnAlertConfig.Visible = false;
#endif
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
            this.groupBoxFolders.ResumeLayout(false);
            this.groupBoxFolders.PerformLayout();
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
        private System.Windows.Forms.Button btnAlertConfig;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.GroupBox groupBoxFolders;
        private System.Windows.Forms.ListBox listBoxFolders;
        private System.Windows.Forms.Button btnAddFolder;
        private System.Windows.Forms.Button btnRemoveFolder;
        private System.Windows.Forms.Button btnEditFolder;
        private System.Windows.Forms.Label lblConfigPath;
        private System.Windows.Forms.Label lblConfigPathValue;
        private global::FolderWatcherWindowsServiceAdmin.Controls.LogViewerControl logViewerControl;
    }
}