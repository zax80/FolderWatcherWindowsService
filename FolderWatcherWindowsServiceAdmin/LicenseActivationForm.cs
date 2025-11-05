using System;
using System.Windows.Forms;
using FolderWatcherWindowsService.Common.Interfaces;

namespace FolderWatcherWindowsServiceAdmin
{
    public partial class LicenseActivationForm : Form
    {
        private readonly ILicenseService _licenseService;

        public LicenseActivationForm(ILicenseService licenseService)
        {
            _licenseService = licenseService;
            InitializeComponent();
            LoadLicenseInfo();
        }

        private void InitializeComponent()
        {
            this.txtLicenseKey = new System.Windows.Forms.TextBox();
            this.btnActivate = new System.Windows.Forms.Button();
            this.btnStartTrial = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblLicenseKey = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtLicenseKey
            // 
            this.txtLicenseKey.Location = new System.Drawing.Point(12, 70);
            this.txtLicenseKey.Name = "txtLicenseKey";
            this.txtLicenseKey.Size = new System.Drawing.Size(460, 20);
            this.txtLicenseKey.TabIndex = 0;
            // 
            // btnActivate
            // 
            this.btnActivate.Location = new System.Drawing.Point(12, 96);
            this.btnActivate.Name = "btnActivate";
            this.btnActivate.Size = new System.Drawing.Size(100, 30);
            this.btnActivate.TabIndex = 1;
            this.btnActivate.Text = "Activate License";
            this.btnActivate.UseVisualStyleBackColor = true;
            this.btnActivate.Click += new System.EventHandler(this.btnActivate_Click);
            // 
            // btnStartTrial
            // 
            this.btnStartTrial.Location = new System.Drawing.Point(118, 96);
            this.btnStartTrial.Name = "btnStartTrial";
            this.btnStartTrial.Size = new System.Drawing.Size(100, 30);
            this.btnStartTrial.TabIndex = 2;
            this.btnStartTrial.Text = "Start Trial";
            this.btnStartTrial.UseVisualStyleBackColor = true;
            this.btnStartTrial.Click += new System.EventHandler(this.btnStartTrial_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(372, 96);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(155, 20);
            this.lblTitle.TabIndex = 4;
            this.lblTitle.Text = "License Activation";
            // 
            // lblLicenseKey
            // 
            this.lblLicenseKey.AutoSize = true;
            this.lblLicenseKey.Location = new System.Drawing.Point(12, 54);
            this.lblLicenseKey.Name = "lblLicenseKey";
            this.lblLicenseKey.Size = new System.Drawing.Size(68, 13);
            this.lblLicenseKey.TabIndex = 5;
            this.lblLicenseKey.Text = "License Key:";
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(12, 32);
            this.lblInfo.MaximumSize = new System.Drawing.Size(460, 0);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(304, 13);
            this.lblInfo.TabIndex = 6;
            this.lblInfo.Text = "Enter your license key or start a 30-day trial to continue.";
            // 
            // LicenseActivationForm
            // 
            this.AcceptButton = this.btnActivate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(484, 141);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lblLicenseKey);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnStartTrial);
            this.Controls.Add(this.btnActivate);
            this.Controls.Add(this.txtLicenseKey);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LicenseActivationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "License Activation";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private TextBox txtLicenseKey;
        private Button btnActivate;
        private Button btnStartTrial;
        private Button btnCancel;
        private Label lblTitle;
        private Label lblLicenseKey;
        private Label lblInfo;

        private void LoadLicenseInfo()
        {
            // Check if trial is available
            var licenseInfo = _licenseService.GetLicenseInfo();
            dynamic info = licenseInfo;

            if (info.HasLicense == false && info.CanStartTrial == false)
            {
                btnStartTrial.Enabled = false;
                lblInfo.Text = "Trial period has been used. Please enter a valid license key.";
            }
        }

        private void btnActivate_Click(object sender, EventArgs e)
        {
            string licenseKey = txtLicenseKey.Text.Trim();

            if (string.IsNullOrEmpty(licenseKey))
            {
                MessageBox.Show("Please enter a license key.", "License Key Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Show current machine name for confirmation
            string currentMachineNameConfirm = Environment.MachineName;

            try
            {
                var result = _licenseService.ActivateLicense(licenseKey);

                // Check if result is null
                if (result == null)
                {
                    MessageBox.Show(
                        "License activation failed: No response from license service.",
                        "Activation Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // Use reflection to safely access properties
                var resultType = result.GetType();
                var successProperty = resultType.GetProperty("Success");
                
                if (successProperty == null)
                {
                    MessageBox.Show(
                        $"License activation failed: Invalid response format.\n\nResponse: {result}",
                        "Activation Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                bool success = (bool)successProperty.GetValue(result);

                if (success)
                {
                    // FIX: Use correct property names from LicenseService.ActivateLicense
                    var durationProperty = resultType.GetProperty("Duration");
                    var expirationProperty = resultType.GetProperty("ExpirationDate");
                    var remainingDaysProperty = resultType.GetProperty("RemainingDays");

                    string duration = durationProperty?.GetValue(result)?.ToString() ?? "Unknown";
                    string expiration = expirationProperty?.GetValue(result)?.ToString() ?? "Unknown";
                    string remainingDays = remainingDaysProperty?.GetValue(result)?.ToString() ?? "Unknown";

                    MessageBox.Show(
                        $"License activated successfully!\n\n" +
                        $"Duration: {duration}\n" +
                        $"Expires: {expiration}\n" +
                        $"Days Remaining: {remainingDays}\n\n" +
                        $"Machine: {currentMachineNameConfirm}",
                        "Activation Successful",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    var messageProperty = resultType.GetProperty("Message");
                    string message = messageProperty?.GetValue(result)?.ToString() ?? "Unknown error";

                    // Enhanced error message with machine name info
                    MessageBox.Show(
                        $"License activation failed:\n\n" +
                        $"{message}\n\n" +
                        $"Current Machine Name: {currentMachineNameConfirm}\n\n" +
                        $"Common causes:\n" +
                        $"• License was generated for a different machine\n" +
                        $"• Machine name mismatch (check capitalization/spaces)\n" +
                        $"• License key is invalid or expired\n\n" +
                        $"TIP: Copy the machine name above and use it EXACTLY\n" +
                        $"when generating a new license key.",
                        "Activation Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error activating license: {ex.Message}\n\n" +
                    $"Machine Name: {Environment.MachineName}",
                    "Activation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnStartTrial_Click(object sender, EventArgs e)
        {
            try
            {
                if (_licenseService.StartTrial())
                {
                    int remainingDays = _licenseService.GetRemainingTrialDays();
                    MessageBox.Show(
                        $"Trial started successfully!\n\n" +
                        $"You have {remainingDays} days to evaluate this software.",
                        "Trial Started",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        "Unable to start trial. Trial may have already been used.",
                        "Trial Start Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error starting trial: {ex.Message}",
                    "Trial Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}