namespace FolderWatcherWindowsServiceAdmin.Controls
{
    partial class LogViewerControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.groupBoxSummary = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelSummary = new System.Windows.Forms.TableLayoutPanel();
            this.lblTotalRecords = new System.Windows.Forms.Label();
            this.lblRecentActivity = new System.Windows.Forms.Label();
            this.lblErrorCount = new System.Windows.Forms.Label();
            this.lblCreated = new System.Windows.Forms.Label();
            this.lblModified = new System.Windows.Forms.Label();
            this.lblDeleted = new System.Windows.Forms.Label();
            this.lblRenamed = new System.Windows.Forms.Label();
            this.lblLastUpdate = new System.Windows.Forms.Label();
            this.groupBoxFilters = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelFilters = new System.Windows.Forms.TableLayoutPanel();
            this.chkDateFilter = new System.Windows.Forms.CheckBox();
            this.dtpFromDate = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.dtpToDate = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbChangeType = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnApplyFilters = new System.Windows.Forms.Button();
            this.btnClearFilters = new System.Windows.Forms.Button();
            this.panelMain = new System.Windows.Forms.Panel();
            this.dataGridViewLogs = new System.Windows.Forms.DataGridView();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblLogPath = new System.Windows.Forms.Label();
            this.panelPagination = new System.Windows.Forms.Panel();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnNextPage = new System.Windows.Forms.Button();
            this.btnPrevPage = new System.Windows.Forms.Button();
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbPageSize = new System.Windows.Forms.ComboBox();
            this.panelTop.SuspendLayout();
            this.groupBoxSummary.SuspendLayout();
            this.tableLayoutPanelSummary.SuspendLayout();
            this.groupBoxFilters.SuspendLayout();
            this.tableLayoutPanelFilters.SuspendLayout();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLogs)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.panelPagination.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.groupBoxSummary);
            this.panelTop.Controls.Add(this.groupBoxFilters);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1200, 140);
            this.panelTop.TabIndex = 0;
            // 
            // groupBoxSummary
            // 
            this.groupBoxSummary.Controls.Add(this.tableLayoutPanelSummary);
            this.groupBoxSummary.Dock = System.Windows.Forms.DockStyle.Right;
            this.groupBoxSummary.Location = new System.Drawing.Point(700, 0);
            this.groupBoxSummary.Name = "groupBoxSummary";
            this.groupBoxSummary.Size = new System.Drawing.Size(500, 140);
            this.groupBoxSummary.TabIndex = 1;
            this.groupBoxSummary.TabStop = false;
            this.groupBoxSummary.Text = "Summary";
            // 
            // tableLayoutPanelSummary
            // 
            this.tableLayoutPanelSummary.ColumnCount = 2;
            this.tableLayoutPanelSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelSummary.Controls.Add(this.lblTotalRecords, 0, 0);
            this.tableLayoutPanelSummary.Controls.Add(this.lblRecentActivity, 1, 0);
            this.tableLayoutPanelSummary.Controls.Add(this.lblErrorCount, 0, 1);
            this.tableLayoutPanelSummary.Controls.Add(this.lblCreated, 1, 1);
            this.tableLayoutPanelSummary.Controls.Add(this.lblModified, 0, 2);
            this.tableLayoutPanelSummary.Controls.Add(this.lblDeleted, 1, 2);
            this.tableLayoutPanelSummary.Controls.Add(this.lblRenamed, 0, 3);
            this.tableLayoutPanelSummary.Controls.Add(this.lblLastUpdate, 1, 3);
            this.tableLayoutPanelSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelSummary.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanelSummary.Name = "tableLayoutPanelSummary";
            this.tableLayoutPanelSummary.RowCount = 4;
            this.tableLayoutPanelSummary.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelSummary.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelSummary.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelSummary.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelSummary.Size = new System.Drawing.Size(494, 121);
            this.tableLayoutPanelSummary.TabIndex = 0;
            // 
            // lblTotalRecords
            // 
            this.lblTotalRecords.AutoSize = true;
            this.lblTotalRecords.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalRecords.Location = new System.Drawing.Point(3, 0);
            this.lblTotalRecords.Name = "lblTotalRecords";
            this.lblTotalRecords.Size = new System.Drawing.Size(241, 30);
            this.lblTotalRecords.TabIndex = 0;
            this.lblTotalRecords.Text = "Total Records: 0";
            this.lblTotalRecords.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRecentActivity
            // 
            this.lblRecentActivity.AutoSize = true;
            this.lblRecentActivity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRecentActivity.Location = new System.Drawing.Point(250, 0);
            this.lblRecentActivity.Name = "lblRecentActivity";
            this.lblRecentActivity.Size = new System.Drawing.Size(241, 30);
            this.lblRecentActivity.TabIndex = 1;
            this.lblRecentActivity.Text = "Recent Activity (30m): 0";
            this.lblRecentActivity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblErrorCount
            // 
            this.lblErrorCount.AutoSize = true;
            this.lblErrorCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblErrorCount.ForeColor = System.Drawing.Color.Red;
            this.lblErrorCount.Location = new System.Drawing.Point(3, 30);
            this.lblErrorCount.Name = "lblErrorCount";
            this.lblErrorCount.Size = new System.Drawing.Size(241, 30);
            this.lblErrorCount.TabIndex = 2;
            this.lblErrorCount.Text = "Errors: 0";
            this.lblErrorCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCreated
            // 
            this.lblCreated.AutoSize = true;
            this.lblCreated.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCreated.ForeColor = System.Drawing.Color.Green;
            this.lblCreated.Location = new System.Drawing.Point(250, 30);
            this.lblCreated.Name = "lblCreated";
            this.lblCreated.Size = new System.Drawing.Size(241, 30);
            this.lblCreated.TabIndex = 3;
            this.lblCreated.Text = "Created: 0";
            this.lblCreated.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblModified
            // 
            this.lblModified.AutoSize = true;
            this.lblModified.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblModified.ForeColor = System.Drawing.Color.Blue;
            this.lblModified.Location = new System.Drawing.Point(3, 60);
            this.lblModified.Name = "lblModified";
            this.lblModified.Size = new System.Drawing.Size(241, 30);
            this.lblModified.TabIndex = 4;
            this.lblModified.Text = "Modified: 0";
            this.lblModified.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDeleted
            // 
            this.lblDeleted.AutoSize = true;
            this.lblDeleted.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDeleted.ForeColor = System.Drawing.Color.OrangeRed;
            this.lblDeleted.Location = new System.Drawing.Point(250, 60);
            this.lblDeleted.Name = "lblDeleted";
            this.lblDeleted.Size = new System.Drawing.Size(241, 30);
            this.lblDeleted.TabIndex = 5;
            this.lblDeleted.Text = "Deleted: 0";
            this.lblDeleted.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRenamed
            // 
            this.lblRenamed.AutoSize = true;
            this.lblRenamed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRenamed.ForeColor = System.Drawing.Color.Purple;
            this.lblRenamed.Location = new System.Drawing.Point(3, 90);
            this.lblRenamed.Name = "lblRenamed";
            this.lblRenamed.Size = new System.Drawing.Size(241, 31);
            this.lblRenamed.TabIndex = 6;
            this.lblRenamed.Text = "Renamed: 0";
            this.lblRenamed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblLastUpdate
            // 
            this.lblLastUpdate.AutoSize = true;
            this.lblLastUpdate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLastUpdate.Location = new System.Drawing.Point(250, 90);
            this.lblLastUpdate.Name = "lblLastUpdate";
            this.lblLastUpdate.Size = new System.Drawing.Size(241, 31);
            this.lblLastUpdate.TabIndex = 7;
            this.lblLastUpdate.Text = "Last Entry: None";
            this.lblLastUpdate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBoxFilters
            // 
            this.groupBoxFilters.Controls.Add(this.tableLayoutPanelFilters);
            this.groupBoxFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxFilters.Location = new System.Drawing.Point(0, 0);
            this.groupBoxFilters.Name = "groupBoxFilters";
            this.groupBoxFilters.Size = new System.Drawing.Size(1200, 140);
            this.groupBoxFilters.TabIndex = 0;
            this.groupBoxFilters.TabStop = false;
            this.groupBoxFilters.Text = "Filters";
            // 
            // tableLayoutPanelFilters
            // 
            this.tableLayoutPanelFilters.ColumnCount = 6;
            this.tableLayoutPanelFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanelFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanelFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanelFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanelFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelFilters.Controls.Add(this.chkDateFilter, 0, 0);
            this.tableLayoutPanelFilters.Controls.Add(this.dtpFromDate, 1, 0);
            this.tableLayoutPanelFilters.Controls.Add(this.label1, 2, 0);
            this.tableLayoutPanelFilters.Controls.Add(this.dtpToDate, 3, 0);
            this.tableLayoutPanelFilters.Controls.Add(this.label2, 4, 0);
            this.tableLayoutPanelFilters.Controls.Add(this.cmbChangeType, 5, 0);
            this.tableLayoutPanelFilters.Controls.Add(this.label4, 0, 1);
            this.tableLayoutPanelFilters.Controls.Add(this.txtSearch, 1, 1);
            this.tableLayoutPanelFilters.Controls.Add(this.btnApplyFilters, 5, 2);
            this.tableLayoutPanelFilters.Controls.Add(this.btnClearFilters, 4, 2);
            this.tableLayoutPanelFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelFilters.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanelFilters.Name = "tableLayoutPanelFilters";
            this.tableLayoutPanelFilters.RowCount = 3;
            this.tableLayoutPanelFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelFilters.Size = new System.Drawing.Size(1194, 121);
            this.tableLayoutPanelFilters.TabIndex = 0;
            // 
            // chkDateFilter
            // 
            this.chkDateFilter.AutoSize = true;
            this.chkDateFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkDateFilter.Location = new System.Drawing.Point(3, 3);
            this.chkDateFilter.Name = "chkDateFilter";
            this.chkDateFilter.Size = new System.Drawing.Size(74, 24);
            this.chkDateFilter.TabIndex = 0;
            this.chkDateFilter.Text = "Date Filter";
            this.chkDateFilter.UseVisualStyleBackColor = true;
            // 
            // dtpFromDate
            // 
            this.dtpFromDate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtpFromDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFromDate.Location = new System.Drawing.Point(83, 3);
            this.dtpFromDate.Name = "dtpFromDate";
            this.dtpFromDate.Size = new System.Drawing.Size(114, 20);
            this.dtpFromDate.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(203, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(24, 30);
            this.label1.TabIndex = 2;
            this.label1.Text = "to";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dtpToDate
            // 
            this.dtpToDate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtpToDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpToDate.Location = new System.Drawing.Point(233, 3);
            this.dtpToDate.Name = "dtpToDate";
            this.dtpToDate.Size = new System.Drawing.Size(114, 20);
            this.dtpToDate.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(353, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 30);
            this.label2.TabIndex = 4;
            this.label2.Text = "Change Type:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbChangeType
            // 
            this.cmbChangeType.Dock = System.Windows.Forms.DockStyle.Left;
            this.cmbChangeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbChangeType.FormattingEnabled = true;
            this.cmbChangeType.Location = new System.Drawing.Point(453, 3);
            this.cmbChangeType.Name = "cmbChangeType";
            this.cmbChangeType.Size = new System.Drawing.Size(200, 21);
            this.cmbChangeType.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(3, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 30);
            this.label4.TabIndex = 8;
            this.label4.Text = "Search:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtSearch
            // 
            this.tableLayoutPanelFilters.SetColumnSpan(this.txtSearch, 3);
            this.txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSearch.Location = new System.Drawing.Point(83, 33);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(264, 20);
            this.txtSearch.TabIndex = 9;
            this.txtSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtSearch_KeyPress);
            // 
            // btnApplyFilters
            // 
            this.btnApplyFilters.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnApplyFilters.Location = new System.Drawing.Point(453, 63);
            this.btnApplyFilters.Name = "btnApplyFilters";
            this.btnApplyFilters.Size = new System.Drawing.Size(94, 55);
            this.btnApplyFilters.TabIndex = 10;
            this.btnApplyFilters.Text = "Apply";
            this.btnApplyFilters.UseVisualStyleBackColor = true;
            this.btnApplyFilters.Click += new System.EventHandler(this.BtnApplyFilters_Click);
            // 
            // btnClearFilters
            // 
            this.btnClearFilters.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnClearFilters.Location = new System.Drawing.Point(353, 63);
            this.btnClearFilters.Name = "btnClearFilters";
            this.btnClearFilters.Size = new System.Drawing.Size(94, 55);
            this.btnClearFilters.TabIndex = 11;
            this.btnClearFilters.Text = "Clear";
            this.btnClearFilters.UseVisualStyleBackColor = true;
            this.btnClearFilters.Click += new System.EventHandler(this.BtnClearFilters_Click);
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.dataGridViewLogs);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 140);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(1200, 460);
            this.panelMain.TabIndex = 1;
            // 
            // dataGridViewLogs
            // 
            this.dataGridViewLogs.AllowUserToAddRows = false;
            this.dataGridViewLogs.AllowUserToDeleteRows = false;
            this.dataGridViewLogs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewLogs.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewLogs.Name = "dataGridViewLogs";
            this.dataGridViewLogs.ReadOnly = true;
            this.dataGridViewLogs.Size = new System.Drawing.Size(1200, 460);
            this.dataGridViewLogs.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.lblStatus);
            this.panelBottom.Controls.Add(this.lblLogPath);
            this.panelBottom.Controls.Add(this.panelPagination);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 600);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1200, 50);
            this.panelBottom.TabIndex = 2;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 30);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(38, 13);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Ready";
            // 
            // lblLogPath
            // 
            this.lblLogPath.AutoSize = true;
            this.lblLogPath.Location = new System.Drawing.Point(12, 10);
            this.lblLogPath.Name = "lblLogPath";
            this.lblLogPath.Size = new System.Drawing.Size(50, 13);
            this.lblLogPath.TabIndex = 1;
            this.lblLogPath.Text = "Log File: ";
            // 
            // panelPagination
            // 
            this.panelPagination.Controls.Add(this.btnRefresh);
            this.panelPagination.Controls.Add(this.btnNextPage);
            this.panelPagination.Controls.Add(this.btnPrevPage);
            this.panelPagination.Controls.Add(this.lblPageInfo);
            this.panelPagination.Controls.Add(this.label5);
            this.panelPagination.Controls.Add(this.cmbPageSize);
            this.panelPagination.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelPagination.Location = new System.Drawing.Point(500, 0);
            this.panelPagination.Name = "panelPagination";
            this.panelPagination.Size = new System.Drawing.Size(700, 50);
            this.panelPagination.TabIndex = 0;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(10, 15);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 5;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // btnNextPage
            // 
            this.btnNextPage.Location = new System.Drawing.Point(620, 15);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(75, 23);
            this.btnNextPage.TabIndex = 4;
            this.btnNextPage.Text = "Next >";
            this.btnNextPage.UseVisualStyleBackColor = true;
            this.btnNextPage.Click += new System.EventHandler(this.BtnNextPage_Click);
            // 
            // btnPrevPage
            // 
            this.btnPrevPage.Location = new System.Drawing.Point(539, 15);
            this.btnPrevPage.Name = "btnPrevPage";
            this.btnPrevPage.Size = new System.Drawing.Size(75, 23);
            this.btnPrevPage.TabIndex = 3;
            this.btnPrevPage.Text = "< Previous";
            this.btnPrevPage.UseVisualStyleBackColor = true;
            this.btnPrevPage.Click += new System.EventHandler(this.BtnPrevPage_Click);
            // 
            // lblPageInfo
            // 
            this.lblPageInfo.AutoSize = true;
            this.lblPageInfo.Location = new System.Drawing.Point(350, 20);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(77, 13);
            this.lblPageInfo.TabIndex = 2;
            this.lblPageInfo.Text = "Page 1 of 1 (0)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(210, 20);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Page Size:";
            // 
            // cmbPageSize
            // 
            this.cmbPageSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPageSize.FormattingEnabled = true;
            this.cmbPageSize.Location = new System.Drawing.Point(274, 17);
            this.cmbPageSize.Name = "cmbPageSize";
            this.cmbPageSize.Size = new System.Drawing.Size(70, 21);
            this.cmbPageSize.TabIndex = 0;
            this.cmbPageSize.SelectedIndexChanged += new System.EventHandler(this.CmbPageSize_SelectedIndexChanged);
            // 
            // LogViewerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.Name = "LogViewerControl";
            this.Size = new System.Drawing.Size(1200, 650);
            this.panelTop.ResumeLayout(false);
            this.groupBoxSummary.ResumeLayout(false);
            this.tableLayoutPanelSummary.ResumeLayout(false);
            this.tableLayoutPanelSummary.PerformLayout();
            this.groupBoxFilters.ResumeLayout(false);
            this.tableLayoutPanelFilters.ResumeLayout(false);
            this.tableLayoutPanelFilters.PerformLayout();
            this.panelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLogs)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.panelPagination.ResumeLayout(false);
            this.panelPagination.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.GroupBox groupBoxFilters;
        private System.Windows.Forms.GroupBox groupBoxSummary;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.DataGridView dataGridViewLogs;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Panel panelPagination;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelFilters;
        private System.Windows.Forms.CheckBox chkDateFilter;
        private System.Windows.Forms.DateTimePicker dtpFromDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpToDate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbChangeType;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnApplyFilters;
        private System.Windows.Forms.Button btnClearFilters;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSummary;
        private System.Windows.Forms.Label lblTotalRecords;
        private System.Windows.Forms.Label lblRecentActivity;
        private System.Windows.Forms.Label lblErrorCount;
        private System.Windows.Forms.Label lblCreated;
        private System.Windows.Forms.Label lblModified;
        private System.Windows.Forms.Label lblDeleted;
        private System.Windows.Forms.Label lblRenamed;
        private System.Windows.Forms.Label lblLastUpdate;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblLogPath;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnNextPage;
        private System.Windows.Forms.Button btnPrevPage;
        private System.Windows.Forms.Label lblPageInfo;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbPageSize;
    }
}