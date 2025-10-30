namespace FolderWatcherWindowsServiceAdmin
{
    partial class AlertConfigForm
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
 this.groupBoxGeneral = new System.Windows.Forms.GroupBox();
 this.checkBoxEnableAlerts = new System.Windows.Forms.CheckBox();
            this.checkBoxEnableAI = new System.Windows.Forms.CheckBox();
            this.labelThreshold = new System.Windows.Forms.Label();
        this.numericThreshold = new System.Windows.Forms.NumericUpDown();
 this.groupBoxEmail = new System.Windows.Forms.GroupBox();
            this.labelRecipients = new System.Windows.Forms.Label();
 this.textBoxRecipients = new System.Windows.Forms.TextBox();
       this.labelSmtpServer = new System.Windows.Forms.Label();
            this.textBoxSmtpServer = new System.Windows.Forms.TextBox();
      this.labelSmtpPort = new System.Windows.Forms.Label();
          this.numericSmtpPort = new System.Windows.Forms.NumericUpDown();
  this.checkBoxEnableSsl = new System.Windows.Forms.CheckBox();
          this.labelUsername = new System.Windows.Forms.Label();
  this.textBoxUsername = new System.Windows.Forms.TextBox();
 this.labelPassword = new System.Windows.Forms.Label();
      this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.labelFromEmail = new System.Windows.Forms.Label();
            this.textBoxFromEmail = new System.Windows.Forms.TextBox();
   this.labelFromName = new System.Windows.Forms.Label();
     this.textBoxFromName = new System.Windows.Forms.TextBox();
  this.groupBoxOpenAI = new System.Windows.Forms.GroupBox();
     this.labelApiKey = new System.Windows.Forms.Label();
   this.textBoxApiKey = new System.Windows.Forms.TextBox();
       this.labelModel = new System.Windows.Forms.Label();
    this.comboBoxModel = new System.Windows.Forms.ComboBox();
        this.btnSave = new System.Windows.Forms.Button();
     this.btnCancel = new System.Windows.Forms.Button();
            this.btnTestEmail = new System.Windows.Forms.Button();
      this.labelConfigPath = new System.Windows.Forms.Label();
            this.groupBoxGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericThreshold)).BeginInit();
     this.groupBoxEmail.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericSmtpPort)).BeginInit();
            this.groupBoxOpenAI.SuspendLayout();
            this.SuspendLayout();
            // 
          // groupBoxGeneral
   // 
     this.groupBoxGeneral.Controls.Add(this.checkBoxEnableAlerts);
 this.groupBoxGeneral.Controls.Add(this.checkBoxEnableAI);
 this.groupBoxGeneral.Controls.Add(this.labelThreshold);
            this.groupBoxGeneral.Controls.Add(this.numericThreshold);
            this.groupBoxGeneral.Location = new System.Drawing.Point(12, 12);
      this.groupBoxGeneral.Name = "groupBoxGeneral";
   this.groupBoxGeneral.Size = new System.Drawing.Size(560, 100);
      this.groupBoxGeneral.TabIndex = 0;
 this.groupBoxGeneral.TabStop = false;
            this.groupBoxGeneral.Text = "General Settings";
        // 
// checkBoxEnableAlerts
     // 
            this.checkBoxEnableAlerts.AutoSize = true;
 this.checkBoxEnableAlerts.Location = new System.Drawing.Point(15, 25);
            this.checkBoxEnableAlerts.Name = "checkBoxEnableAlerts";
    this.checkBoxEnableAlerts.Size = new System.Drawing.Size(150, 17);
            this.checkBoxEnableAlerts.TabIndex = 0;
 this.checkBoxEnableAlerts.Text = "Enable Email Alerts";
       this.checkBoxEnableAlerts.UseVisualStyleBackColor = true;
            // 
            // checkBoxEnableAI
            // 
       this.checkBoxEnableAI.AutoSize = true;
  this.checkBoxEnableAI.Location = new System.Drawing.Point(15, 48);
            this.checkBoxEnableAI.Name = "checkBoxEnableAI";
            this.checkBoxEnableAI.Size = new System.Drawing.Size(180, 17);
          this.checkBoxEnableAI.TabIndex = 1;
            this.checkBoxEnableAI.Text = "Enable AI Threat Analysis";
          this.checkBoxEnableAI.UseVisualStyleBackColor = true;
 // 
            // labelThreshold
        // 
 this.labelThreshold.AutoSize = true;
            this.labelThreshold.Location = new System.Drawing.Point(15, 74);
            this.labelThreshold.Name = "labelThreshold";
            this.labelThreshold.Size = new System.Drawing.Size(150, 13);
        this.labelThreshold.TabIndex = 2;
    this.labelThreshold.Text = "Alert Threshold (0.0 - 1.0):";
  // 
 // numericThreshold
    // 
            this.numericThreshold.DecimalPlaces = 2;
   this.numericThreshold.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
       this.numericThreshold.Location = new System.Drawing.Point(180, 72);
       this.numericThreshold.Maximum = new decimal(new int[] { 10, 0, 0, 65536 });
            this.numericThreshold.Name = "numericThreshold";
        this.numericThreshold.Size = new System.Drawing.Size(100, 20);
  this.numericThreshold.TabIndex = 3;
        this.numericThreshold.Value = new decimal(new int[] { 7, 0, 0, 65536 });
      // 
      // groupBoxEmail
            // 
  this.groupBoxEmail.Controls.Add(this.labelRecipients);
   this.groupBoxEmail.Controls.Add(this.textBoxRecipients);
            this.groupBoxEmail.Controls.Add(this.labelSmtpServer);
       this.groupBoxEmail.Controls.Add(this.textBoxSmtpServer);
            this.groupBoxEmail.Controls.Add(this.labelSmtpPort);
    this.groupBoxEmail.Controls.Add(this.numericSmtpPort);
      this.groupBoxEmail.Controls.Add(this.checkBoxEnableSsl);
            this.groupBoxEmail.Controls.Add(this.labelUsername);
      this.groupBoxEmail.Controls.Add(this.textBoxUsername);
      this.groupBoxEmail.Controls.Add(this.labelPassword);
            this.groupBoxEmail.Controls.Add(this.textBoxPassword);
            this.groupBoxEmail.Controls.Add(this.labelFromEmail);
            this.groupBoxEmail.Controls.Add(this.textBoxFromEmail);
 this.groupBoxEmail.Controls.Add(this.labelFromName);
            this.groupBoxEmail.Controls.Add(this.textBoxFromName);
 this.groupBoxEmail.Location = new System.Drawing.Point(12, 118);
 this.groupBoxEmail.Name = "groupBoxEmail";
            this.groupBoxEmail.Size = new System.Drawing.Size(560, 260);
            this.groupBoxEmail.TabIndex = 1;
  this.groupBoxEmail.TabStop = false;
  this.groupBoxEmail.Text = "Email Configuration";
     // 
      // labelRecipients
      // 
    this.labelRecipients.AutoSize = true;
    this.labelRecipients.Location = new System.Drawing.Point(15, 25);
   this.labelRecipients.Name = "labelRecipients";
     this.labelRecipients.Size = new System.Drawing.Size(170, 13);
   this.labelRecipients.TabIndex = 0;
  this.labelRecipients.Text = "Recipients (comma-separated):";
   // 
         // textBoxRecipients
            // 
            this.textBoxRecipients.Location = new System.Drawing.Point(15, 41);
            this.textBoxRecipients.Multiline = true;
            this.textBoxRecipients.Name = "textBoxRecipients";
         this.textBoxRecipients.Size = new System.Drawing.Size(530, 40);
            this.textBoxRecipients.TabIndex = 1;
            // 
      // labelSmtpServer
            // 
            this.labelSmtpServer.AutoSize = true;
   this.labelSmtpServer.Location = new System.Drawing.Point(15, 90);
         this.labelSmtpServer.Name = "labelSmtpServer";
 this.labelSmtpServer.Size = new System.Drawing.Size(80, 13);
            this.labelSmtpServer.TabIndex = 2;
   this.labelSmtpServer.Text = "SMTP Server:";
     // 
   // textBoxSmtpServer
     // 
        this.textBoxSmtpServer.Location = new System.Drawing.Point(120, 87);
    this.textBoxSmtpServer.Name = "textBoxSmtpServer";
         this.textBoxSmtpServer.Size = new System.Drawing.Size(200, 20);
            this.textBoxSmtpServer.TabIndex = 3;
    // 
        // labelSmtpPort
         // 
  this.labelSmtpPort.AutoSize = true;
            this.labelSmtpPort.Location = new System.Drawing.Point(340, 90);
            this.labelSmtpPort.Name = "labelSmtpPort";
            this.labelSmtpPort.Size = new System.Drawing.Size(32, 13);
    this.labelSmtpPort.TabIndex = 4;
  this.labelSmtpPort.Text = "Port:";
      // 
            // numericSmtpPort
            // 
   this.numericSmtpPort.Location = new System.Drawing.Point(380, 88);
            this.numericSmtpPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
    this.numericSmtpPort.Name = "numericSmtpPort";
        this.numericSmtpPort.Size = new System.Drawing.Size(80, 20);
    this.numericSmtpPort.TabIndex = 5;
            this.numericSmtpPort.Value = new decimal(new int[] { 587, 0, 0, 0 });
         // 
            // checkBoxEnableSsl
        // 
       this.checkBoxEnableSsl.AutoSize = true;
      this.checkBoxEnableSsl.Checked = true;
  this.checkBoxEnableSsl.CheckState = System.Windows.Forms.CheckState.Checked;
          this.checkBoxEnableSsl.Location = new System.Drawing.Point(470, 89);
   this.checkBoxEnableSsl.Name = "checkBoxEnableSsl";
 this.checkBoxEnableSsl.Size = new System.Drawing.Size(75, 17);
            this.checkBoxEnableSsl.TabIndex = 6;
            this.checkBoxEnableSsl.Text = "Use SSL";
     this.checkBoxEnableSsl.UseVisualStyleBackColor = true;
  // 
 // labelUsername
            // 
            this.labelUsername.AutoSize = true;
            this.labelUsername.Location = new System.Drawing.Point(15, 120);
          this.labelUsername.Name = "labelUsername";
       this.labelUsername.Size = new System.Drawing.Size(63, 13);
     this.labelUsername.TabIndex = 7;
     this.labelUsername.Text = "Username:";
      // 
        // textBoxUsername
  // 
this.textBoxUsername.Location = new System.Drawing.Point(120, 117);
    this.textBoxUsername.Name = "textBoxUsername";
    this.textBoxUsername.Size = new System.Drawing.Size(200, 20);
            this.textBoxUsername.TabIndex = 8;
       // 
            // labelPassword
            // 
          this.labelPassword.AutoSize = true;
this.labelPassword.Location = new System.Drawing.Point(15, 150);
   this.labelPassword.Name = "labelPassword";
          this.labelPassword.Size = new System.Drawing.Size(59, 13);
        this.labelPassword.TabIndex = 9;
          this.labelPassword.Text = "Password:";
       // 
            // textBoxPassword
       // 
    this.textBoxPassword.Location = new System.Drawing.Point(120, 147);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '•';
       this.textBoxPassword.Size = new System.Drawing.Size(200, 20);
       this.textBoxPassword.TabIndex = 10;
   // 
            // labelFromEmail
// 
    this.labelFromEmail.AutoSize = true;
     this.labelFromEmail.Location = new System.Drawing.Point(15, 180);
          this.labelFromEmail.Name = "labelFromEmail";
          this.labelFromEmail.Size = new System.Drawing.Size(68, 13);
  this.labelFromEmail.TabIndex = 11;
      this.labelFromEmail.Text = "From Email:";
        // 
  // textBoxFromEmail
            // 
     this.textBoxFromEmail.Location = new System.Drawing.Point(120, 177);
            this.textBoxFromEmail.Name = "textBoxFromEmail";
          this.textBoxFromEmail.Size = new System.Drawing.Size(200, 20);
            this.textBoxFromEmail.TabIndex = 12;
          // 
 // labelFromName
            // 
            this.labelFromName.AutoSize = true;
  this.labelFromName.Location = new System.Drawing.Point(15, 210);
          this.labelFromName.Name = "labelFromName";
            this.labelFromName.Size = new System.Drawing.Size(98, 13);
    this.labelFromName.TabIndex = 13;
         this.labelFromName.Text = "From Display Name:";
            // 
         // textBoxFromName
     // 
            this.textBoxFromName.Location = new System.Drawing.Point(120, 207);
   this.textBoxFromName.Name = "textBoxFromName";
       this.textBoxFromName.Size = new System.Drawing.Size(200, 20);
         this.textBoxFromName.TabIndex = 14;
   // 
            // groupBoxOpenAI
          // 
            this.groupBoxOpenAI.Controls.Add(this.labelApiKey);
         this.groupBoxOpenAI.Controls.Add(this.textBoxApiKey);
    this.groupBoxOpenAI.Controls.Add(this.labelModel);
   this.groupBoxOpenAI.Controls.Add(this.comboBoxModel);
            this.groupBoxOpenAI.Location = new System.Drawing.Point(12, 384);
         this.groupBoxOpenAI.Name = "groupBoxOpenAI";
            this.groupBoxOpenAI.Size = new System.Drawing.Size(560, 100);
     this.groupBoxOpenAI.TabIndex = 2;
            this.groupBoxOpenAI.TabStop = false;
  this.groupBoxOpenAI.Text = "OpenAI Configuration";
            // 
 // labelApiKey
        // 
            this.labelApiKey.AutoSize = true;
       this.labelApiKey.Location = new System.Drawing.Point(15, 25);
     this.labelApiKey.Name = "labelApiKey";
          this.labelApiKey.Size = new System.Drawing.Size(51, 13);
  this.labelApiKey.TabIndex = 0;
this.labelApiKey.Text = "API Key:";
       // 
 // textBoxApiKey
            // 
            this.textBoxApiKey.Location = new System.Drawing.Point(120, 22);
   this.textBoxApiKey.Name = "textBoxApiKey";
            this.textBoxApiKey.PasswordChar = '•';
       this.textBoxApiKey.Size = new System.Drawing.Size(425, 20);
      this.textBoxApiKey.TabIndex = 1;
  // 
            // labelModel
     // 
            this.labelModel.AutoSize = true;
            this.labelModel.Location = new System.Drawing.Point(15, 60);
     this.labelModel.Name = "labelModel";
       this.labelModel.Size = new System.Drawing.Size(42, 13);
         this.labelModel.TabIndex = 2;
    this.labelModel.Text = "Model:";
   // 
     // comboBoxModel
  // 
            this.comboBoxModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
    this.comboBoxModel.FormattingEnabled = true;
       this.comboBoxModel.Items.AddRange(new object[] {
        "gpt-4o",
   "gpt-4o-mini",
     "gpt-4-turbo",
            "gpt-4",
  "gpt-3.5-turbo"});
 this.comboBoxModel.Location = new System.Drawing.Point(120, 57);
            this.comboBoxModel.Name = "comboBoxModel";
this.comboBoxModel.Size = new System.Drawing.Size(200, 21);
          this.comboBoxModel.TabIndex = 3;
            // 
            // btnSave
      // 
        this.btnSave.Location = new System.Drawing.Point(336, 530);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 30);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
      // 
            // btnCancel
     // 
 this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
          this.btnCancel.Location = new System.Drawing.Point(442, 530);
            this.btnCancel.Name = "btnCancel";
         this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.TabIndex = 4;
         this.btnCancel.Text = "Cancel";
  this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
   // btnTestEmail
         // 
            this.btnTestEmail.Location = new System.Drawing.Point(230, 530);
   this.btnTestEmail.Name = "btnTestEmail";
            this.btnTestEmail.Size = new System.Drawing.Size(100, 30);
            this.btnTestEmail.TabIndex = 5;
       this.btnTestEmail.Text = "Test Email";
  this.btnTestEmail.UseVisualStyleBackColor = true;
 this.btnTestEmail.Click += new System.EventHandler(this.btnTestEmail_Click);
        // 
            // labelConfigPath
            // 
    this.labelConfigPath.AutoSize = true;
    this.labelConfigPath.Location = new System.Drawing.Point(12, 497);
          this.labelConfigPath.Name = "labelConfigPath";
            this.labelConfigPath.Size = new System.Drawing.Size(100, 13);
            this.labelConfigPath.TabIndex = 6;
       this.labelConfigPath.Text = "Config file path: ";
   // 
          // AlertConfigForm
      // 
          this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
  this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
       this.ClientSize = new System.Drawing.Size(584, 572);
          this.Controls.Add(this.labelConfigPath);
            this.Controls.Add(this.btnTestEmail);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
  this.Controls.Add(this.groupBoxOpenAI);
   this.Controls.Add(this.groupBoxEmail);
            this.Controls.Add(this.groupBoxGeneral);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
 this.MaximizeBox = false;
          this.MinimizeBox = false;
  this.Name = "AlertConfigForm";
    this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
    this.Text = "Alert Configuration";
         this.Load += new System.EventHandler(this.AlertConfigForm_Load);
            this.groupBoxGeneral.ResumeLayout(false);
            this.groupBoxGeneral.PerformLayout();
  ((System.ComponentModel.ISupportInitialize)(this.numericThreshold)).EndInit();
          this.groupBoxEmail.ResumeLayout(false);
          this.groupBoxEmail.PerformLayout();
     ((System.ComponentModel.ISupportInitialize)(this.numericSmtpPort)).EndInit();
      this.groupBoxOpenAI.ResumeLayout(false);
   this.groupBoxOpenAI.PerformLayout();
       this.ResumeLayout(false);
   this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxGeneral;
      private System.Windows.Forms.CheckBox checkBoxEnableAlerts;
        private System.Windows.Forms.CheckBox checkBoxEnableAI;
        private System.Windows.Forms.Label labelThreshold;
        private System.Windows.Forms.NumericUpDown numericThreshold;
        private System.Windows.Forms.GroupBox groupBoxEmail;
        private System.Windows.Forms.Label labelRecipients;
        private System.Windows.Forms.TextBox textBoxRecipients;
        private System.Windows.Forms.Label labelSmtpServer;
        private System.Windows.Forms.TextBox textBoxSmtpServer;
   private System.Windows.Forms.Label labelSmtpPort;
        private System.Windows.Forms.NumericUpDown numericSmtpPort;
        private System.Windows.Forms.CheckBox checkBoxEnableSsl;
        private System.Windows.Forms.Label labelUsername;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.Label labelPassword;
private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelFromEmail;
        private System.Windows.Forms.TextBox textBoxFromEmail;
      private System.Windows.Forms.Label labelFromName;
    private System.Windows.Forms.TextBox textBoxFromName;
      private System.Windows.Forms.GroupBox groupBoxOpenAI;
        private System.Windows.Forms.Label labelApiKey;
  private System.Windows.Forms.TextBox textBoxApiKey;
      private System.Windows.Forms.Label labelModel;
        private System.Windows.Forms.ComboBox comboBoxModel;
        private System.Windows.Forms.Button btnSave;
   private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnTestEmail;
     private System.Windows.Forms.Label labelConfigPath;
    }
}
