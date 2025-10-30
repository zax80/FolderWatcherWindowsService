using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace FolderWatcherWindowsServiceAdmin
{
    /// <summary>
    /// Form for configuring email alerts and threat detection settings.
    /// </summary>
    public partial class AlertConfigForm : Form
    {
        private AlertConfiguration _config;
        private readonly string _configFilePath;
        private bool _isAutoPopulating = false;

        // Common email provider SMTP settings
        private static readonly Dictionary<string, SmtpProviderSettings> EmailProviders = new Dictionary<string, SmtpProviderSettings>(StringComparer.OrdinalIgnoreCase)
        {
            // Gmail
            { "gmail.com", new SmtpProviderSettings
     {
          Name = "Gmail",
    SmtpServer = "smtp.gmail.com",
        Port = 587,
    UseSsl = true,
         Instructions = "Use an App Password (not your regular Gmail password).\nEnable 2FA and create one at: https://myaccount.google.com/apppasswords"
           }
    },
    { "googlemail.com", new SmtpProviderSettings
       {
     Name = "Gmail",
     SmtpServer = "smtp.gmail.com",
      Port = 587,
        UseSsl = true,
        Instructions = "Use an App Password (not your regular Gmail password).\nEnable 2FA and create one at: https://myaccount.google.com/apppasswords"
                }
},
    
            // Outlook.com / Hotmail
    { "outlook.com", new SmtpProviderSettings
                {
                Name = "Outlook.com",
SmtpServer = "smtp-mail.outlook.com",
         Port = 587,
         UseSsl = true,
         Instructions = "Use your regular Outlook.com password."
     }
            },
      { "hotmail.com", new SmtpProviderSettings
      {
           Name = "Outlook.com",
             SmtpServer = "smtp-mail.outlook.com",
                  Port = 587,
          UseSsl = true,
     Instructions = "Use your regular Hotmail password."
          }
        },
     { "live.com", new SmtpProviderSettings
        {
            Name = "Outlook.com",
     SmtpServer = "smtp-mail.outlook.com",
          Port = 587,
     UseSsl = true,
      Instructions = "Use your regular Live.com password."
                }
},
    
            // Office 365
      { "office365.com", new SmtpProviderSettings
    {
          Name = "Office 365",
           SmtpServer = "smtp.office365.com",
        Port = 587,
     UseSsl = true,
           Instructions = "Use your Office 365 email and password.\nMay require App Password if using Modern Authentication."
                }
            },
            
          // Yahoo Mail
            { "yahoo.com", new SmtpProviderSettings
   {
     Name = "Yahoo Mail",
     SmtpServer = "smtp.mail.yahoo.com",
           Port = 587,
          UseSsl = true,
      Instructions = "Generate an App Password at: https://login.yahoo.com/account/security"
     }
      },
  { "yahoo.co.uk", new SmtpProviderSettings
        {
        Name = "Yahoo Mail",
  SmtpServer = "smtp.mail.yahoo.com",
        Port = 587,
   UseSsl = true,
   Instructions = "Generate an App Password at: https://login.yahoo.com/account/security"
                }
  },
            
        // iCloud
       { "icloud.com", new SmtpProviderSettings
   {
   Name = "iCloud Mail",
     SmtpServer = "smtp.mail.me.com",
        Port = 587,
        UseSsl = true,
        Instructions = "Generate an App-Specific Password at: https://appleid.apple.com"
    }
  },
            { "me.com", new SmtpProviderSettings
       {
    Name = "iCloud Mail",
         SmtpServer = "smtp.mail.me.com",
     Port = 587,
     UseSsl = true,
       Instructions = "Generate an App-Specific Password at: https://appleid.apple.com"
                }
 },

            // AOL
    { "aol.com", new SmtpProviderSettings
                {
   Name = "AOL Mail",
   SmtpServer = "smtp.aol.com",
           Port = 587,
      UseSsl = true,
  Instructions = "Generate an App Password at: https://login.aol.com/account/security"
                }
          },
         
    // Zoho Mail
        { "zoho.com", new SmtpProviderSettings
              {
        Name = "Zoho Mail",
       SmtpServer = "smtp.zoho.com",
        Port = 587,
       UseSsl = true,
     Instructions = "Use your Zoho email and password."
    }
    },
            
         // ProtonMail
            { "protonmail.com", new SmtpProviderSettings
         {
  Name = "ProtonMail",
    SmtpServer = "smtp.protonmail.com",
 Port = 587,
            UseSsl = true,
        Instructions = "Requires ProtonMail Bridge. Download at: https://protonmail.com/bridge"
   }
            },
            { "proton.me", new SmtpProviderSettings
    {
          Name = "ProtonMail",
       SmtpServer = "smtp.protonmail.com",
    Port = 587,
        UseSsl = true,
          Instructions = "Requires ProtonMail Bridge. Download at: https://protonmail.com/bridge"
    }
            },
            
          // SendGrid (common for bulk emails)
            { "sendgrid.net", new SmtpProviderSettings
                {
     Name = "SendGrid",
SmtpServer = "smtp.sendgrid.net",
  Port = 587,
UseSsl = true,
     Instructions = "Use 'apikey' as username and your SendGrid API key as password."
    }
        }
        };

        public AlertConfigForm()
        {
            InitializeComponent();

            // Determine config file path (same location as service would use)
            string serviceDir = Path.Combine(Application.StartupPath);
            _configFilePath = Path.Combine(serviceDir, "AlertConfiguration.xml");

            // Wire up the TextChanged event for auto-detection
            textBoxFromEmail.TextChanged += TextBoxFromEmail_TextChanged;
        }

        private void AlertConfigForm_Load(object sender, EventArgs e)
        {
            LoadConfiguration();
            PopulateUI();
            labelConfigPath.Text = $"Config file: {_configFilePath}";
        }

        /// <summary>
        /// Auto-detect email provider and populate SMTP settings when "From Email" changes.
        /// </summary>
        private void TextBoxFromEmail_TextChanged(object sender, EventArgs e)
        {
            if (_isAutoPopulating) return;

            string email = textBoxFromEmail.Text?.Trim();
            if (string.IsNullOrEmpty(email)) return;

            // Extract domain from email
            int atIndex = email.IndexOf('@');
            if (atIndex < 0 || atIndex == email.Length - 1) return;

            string domain = email.Substring(atIndex + 1).ToLowerInvariant();

            // Check if we have settings for this provider
            if (EmailProviders.TryGetValue(domain, out SmtpProviderSettings settings))
            {
                // Only auto-populate if SMTP fields are empty or default
                if (string.IsNullOrWhiteSpace(textBoxSmtpServer.Text) ||
                         textBoxSmtpServer.Text == "smtp.gmail.com" || // Previous default
                  ShouldAutoPopulate())
                {
                    ApplyProviderSettings(settings);
                    ShowProviderDetectedMessage(settings);
                }
            }
        }

        /// <summary>
        /// Determines if we should auto-populate based on current field values.
        /// </summary>
        private bool ShouldAutoPopulate()
        {
            // Auto-populate if fields are empty or have common defaults
            bool smtpEmpty = string.IsNullOrWhiteSpace(textBoxSmtpServer.Text);
            bool portDefault = numericSmtpPort.Value == 587 || numericSmtpPort.Value == 465 || numericSmtpPort.Value == 25;
            bool usernameEmpty = string.IsNullOrWhiteSpace(textBoxUsername.Text);

            return smtpEmpty || (portDefault && usernameEmpty);
        }

        /// <summary>
        /// Apply provider settings to the form fields.
        /// </summary>
        private void ApplyProviderSettings(SmtpProviderSettings settings)
        {
            _isAutoPopulating = true;
            try
            {
                textBoxSmtpServer.Text = settings.SmtpServer;
                numericSmtpPort.Value = settings.Port;
                checkBoxEnableSsl.Checked = settings.UseSsl;

                // Auto-fill username with the from email if empty
                if (string.IsNullOrWhiteSpace(textBoxUsername.Text))
                {
                    textBoxUsername.Text = textBoxFromEmail.Text;
                }
            }
            finally
            {
                _isAutoPopulating = false;
            }
        }

        /// <summary>
        /// Show a helpful message when provider is detected.
        /// </summary>
        private void ShowProviderDetectedMessage(SmtpProviderSettings settings)
        {
            string message = $"✓ {settings.Name} detected!\n\n" +
            $"SMTP settings have been auto-configured:\n" +
               $"• Server: {settings.SmtpServer}\n" +
                        $"• Port: {settings.Port}\n" +
         $"• SSL: {(settings.UseSsl ? "Enabled" : "Disabled")}\n\n";

            if (!string.IsNullOrEmpty(settings.Instructions))
            {
                message += $"Important:\n{settings.Instructions}";
            }

            MessageBox.Show(message, "Email Provider Detected",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var serializer = new XmlSerializer(typeof(AlertConfiguration));
                    using (var reader = new StreamReader(_configFilePath))
                    {
                        _config = (AlertConfiguration)serializer.Deserialize(reader);
                    }
                }
                else
                {
                    _config = new AlertConfiguration();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}\n\nUsing default settings.",
                      "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _config = new AlertConfiguration();
            }
        }

        private void PopulateUI()
        {
            _isAutoPopulating = true;
            try
            {
                // General settings
                checkBoxEnableAlerts.Checked = _config.AlertingEnabled;
                checkBoxEnableAI.Checked = _config.EnableAIAnalysis;
                numericThreshold.Value = (decimal)_config.MinimumThreatScore;

                // Email settings
                if (_config.EmailRecipients != null && _config.EmailRecipients.Length > 0)
                {
                    textBoxRecipients.Text = string.Join(", ", _config.EmailRecipients);
                }
                textBoxSmtpServer.Text = _config.SmtpServer ?? "";
                numericSmtpPort.Value = _config.SmtpPort;
                checkBoxEnableSsl.Checked = _config.SmtpEnableSsl;
                textBoxUsername.Text = _config.SmtpUsername ?? "";
                textBoxPassword.Text = _config.SmtpPassword ?? "";
                textBoxFromEmail.Text = _config.FromEmail ?? "";
                textBoxFromName.Text = _config.FromDisplayName ?? "";

                // OpenAI settings
                textBoxApiKey.Text = _config.OpenAIApiKey ?? "";

                if (!string.IsNullOrEmpty(_config.OpenAIModel))
                {
                    int index = comboBoxModel.Items.IndexOf(_config.OpenAIModel);
                    comboBoxModel.SelectedIndex = index >= 0 ? index : 0;
                }
                else
                {
                    comboBoxModel.SelectedIndex = 1; // Default to gpt-4o-mini
                }
            }
            finally
            {
                _isAutoPopulating = false;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateConfiguration())
            {
                return;
            }

            UpdateConfigFromUI();

            try
            {
                SaveConfiguration();
                MessageBox.Show("Configuration saved successfully.\n\nNote: You may need to restart the service for changes to take effect.",
                 "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}",
                   "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnTestEmail_Click(object sender, EventArgs e)
        {
            if (!ValidateEmailConfiguration())
            {
                return;
            }

            UpdateConfigFromUI();

            try
            {
                // Temporarily save config for testing
                SaveConfiguration();

                // Get test recipient
                string testRecipient = textBoxRecipients.Text.Split(',')[0].Trim();

                // Create a test email service
                var testService = new EmailAlertService(_config);
                testService.SendTestEmail(testRecipient);

                MessageBox.Show($"Test email sent successfully to {testRecipient}!\n\nPlease check the recipient's inbox.",
              "Test Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send test email:\n\n{ex.Message}\n\nPlease verify your SMTP settings.",
                     "Test Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateConfiguration()
        {
            if (checkBoxEnableAlerts.Checked)
            {
                if (!ValidateEmailConfiguration())
                {
                    return false;
                }
            }

            if (checkBoxEnableAI.Checked)
            {
                if (string.IsNullOrWhiteSpace(textBoxApiKey.Text))
                {
                    MessageBox.Show("Please enter an OpenAI API key to enable AI analysis.",
                "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxApiKey.Focus();
                    return false;
                }
            }

            return true;
        }

        private bool ValidateEmailConfiguration()
        {
            if (string.IsNullOrWhiteSpace(textBoxRecipients.Text))
            {
                MessageBox.Show("Please enter at least one email recipient.",
          "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxRecipients.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxSmtpServer.Text))
            {
                MessageBox.Show("Please enter an SMTP server.",
                     "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxSmtpServer.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxUsername.Text))
            {
                MessageBox.Show("Please enter an SMTP username.",
           "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxUsername.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxPassword.Text))
            {
                MessageBox.Show("Please enter an SMTP password.",
                         "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxPassword.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBoxFromEmail.Text))
            {
                MessageBox.Show("Please enter a 'From' email address.",
                   "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxFromEmail.Focus();
                return false;
            }

            return true;
        }

        private void UpdateConfigFromUI()
        {
            // General settings
            _config.AlertingEnabled = checkBoxEnableAlerts.Checked;
            _config.EnableAIAnalysis = checkBoxEnableAI.Checked;
            _config.MinimumThreatScore = (float)numericThreshold.Value;

            // Email settings
            _config.EmailRecipients = textBoxRecipients.Text
 .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
           .Select(e => e.Trim())
     .Where(e => !string.IsNullOrEmpty(e))
             .ToArray();
            _config.SmtpServer = textBoxSmtpServer.Text.Trim();
            _config.SmtpPort = (int)numericSmtpPort.Value;
            _config.SmtpEnableSsl = checkBoxEnableSsl.Checked;
            _config.SmtpUsername = textBoxUsername.Text.Trim();
            _config.SmtpPassword = textBoxPassword.Text;
            _config.FromEmail = textBoxFromEmail.Text.Trim();
            _config.FromDisplayName = textBoxFromName.Text.Trim();

            // OpenAI settings
            _config.OpenAIApiKey = textBoxApiKey.Text.Trim();
            _config.OpenAIModel = comboBoxModel.SelectedItem?.ToString() ?? "gpt-4o-mini";
        }

        private void SaveConfiguration()
        {
            var serializer = new XmlSerializer(typeof(AlertConfiguration));
            using (var writer = new StreamWriter(_configFilePath))
            {
                serializer.Serialize(writer, _config);
            }
        }
    }

    /// <summary>
    /// Alert configuration class for the admin application (matches service structure).
    /// </summary>
    [Serializable]
    public class AlertConfiguration
    {
        public bool AlertingEnabled { get; set; }
        public string[] EmailRecipients { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public bool SmtpEnableSsl { get; set; }
        public string FromEmail { get; set; }
        public string FromDisplayName { get; set; }
        public float MinimumThreatScore { get; set; }
        public string OpenAIApiKey { get; set; }
        public string OpenAIModel { get; set; }
        public bool EnableAIAnalysis { get; set; }

        public AlertConfiguration()
        {
            AlertingEnabled = false;
            MinimumThreatScore = 0.7f;
            SmtpPort = 587;
            SmtpEnableSsl = true;
            OpenAIModel = "gpt-4o-mini";
            EnableAIAnalysis = true;
            FromDisplayName = "Folder Watcher Alert System";
        }
    }

    /// <summary>
    /// Helper class for sending test emails from admin application.
    /// </summary>
    internal class EmailAlertService
    {
        private readonly AlertConfiguration _config;

        public EmailAlertService(AlertConfiguration config)
        {
            _config = config;
        }

        public void SendTestEmail(string testRecipient)
        {
            using (var message = new System.Net.Mail.MailMessage())
            {
                message.From = new System.Net.Mail.MailAddress(_config.FromEmail, _config.FromDisplayName);
                message.To.Add(testRecipient);
                message.Subject = "Folder Watcher Alert System - Test Email";
                message.Body = BuildTestEmailBody();
                message.IsBodyHtml = true;

                using (var smtpClient = new System.Net.Mail.SmtpClient(_config.SmtpServer, _config.SmtpPort))
                {
                    smtpClient.EnableSsl = _config.SmtpEnableSsl;
                    smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new System.Net.NetworkCredential(_config.SmtpUsername, _config.SmtpPassword);

                    smtpClient.Send(message);
                }
            }
        }

        private string BuildTestEmailBody()
        {
            return $@"<!DOCTYPE html>
<html><body style='font-family: Arial, sans-serif;'>
<h2>Folder Watcher Alert System - Test Email</h2>
<p>This is a test email from the Folder Watcher Alert System.</p>
<p>If you received this email, your alert configuration is working correctly.</p>
<p><small>Test sent at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</small></p>
</body></html>";
        }
    }

    /// <summary>
    /// SMTP provider settings for common email services.
    /// </summary>
    internal class SmtpProviderSettings
    {
        public string Name { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public string Instructions { get; set; }
    }
}
