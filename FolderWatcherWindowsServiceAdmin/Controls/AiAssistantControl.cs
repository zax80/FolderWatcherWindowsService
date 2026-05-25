#if PREMIUM
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FolderWatcherWindowsService.ThreatDetection.Services;
using FolderWatcherWindowsService.Common.Models;
using FolderWatcherWindowsServiceAdmin.Models;

namespace FolderWatcherWindowsServiceAdmin.Controls
{
    public partial class AiAssistantControl : UserControl
    {
        private ThreatDetectionService _threatService;
        private TextBox txtQuestion;
        private RichTextBox txtAnswer;
        private Button btnAsk;
        private Button btnClear;
        private ListBox lstHistory;
        private DataGridView dgvRelevantLogs;
        private Label lblStatus;

        public AiAssistantControl()
        {
            InitializeComponent();
            InitializeAiControls();
        }

        public void SetThreatService(ThreatDetectionService service)
        {
            _threatService = service;
        }

        private void InitializeAiControls()
        {
            // Layout
            this.Dock = DockStyle.Fill;

            // Top Panel - Question Input
            var panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(10)
            };

            var lblPrompt = new Label
            {
                Text = "Ask a question about your file system logs:",
                Dock = DockStyle.Top,
                Height = 20
            };

            txtQuestion = new TextBox
            {
                Dock = DockStyle.Top,
                Multiline = true,
                Height = 40,
                ForeColor = Color.Gray,
                Text = "e.g., 'Show me all suspicious activities today' or 'What files did John modify?'"
            };

            // Implement placeholder behavior for .NET Framework
            txtQuestion.Enter += TxtQuestion_Enter;
            txtQuestion.Leave += TxtQuestion_Leave;

            panelTop.Controls.Add(txtQuestion);
            panelTop.Controls.Add(lblPrompt);

            // Button Panel
            var panelButtons = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(10, 5, 10, 5)
            };

            btnAsk = new Button
            {
                Text = "🤖 Ask AI",
                Width = 100,
                Height = 30,
                Dock = DockStyle.Left
            };
            btnAsk.Click += BtnAsk_Click;

            btnClear = new Button
            {
                Text = "Clear",
                Width = 80,
                Height = 30,
                Dock = DockStyle.Left,
                Margin = new Padding(5, 0, 0, 0)
            };
            btnClear.Click += (s, e) => ClearChat();

            panelButtons.Controls.Add(btnClear);
            panelButtons.Controls.Add(btnAsk);

            // Status Label
            lblStatus = new Label
            {
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Text = "Ready"
            };

            // Split Container for Answer and Logs
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 250
            };

            // Answer Panel
            var grpAnswer = new GroupBox
            {
                Text = "AI Answer",
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            txtAnswer = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10F)
            };

            grpAnswer.Controls.Add(txtAnswer);
            splitContainer.Panel1.Controls.Add(grpAnswer);

            // Relevant Logs Panel
            var grpLogs = new GroupBox
            {
                Text = "Relevant Log Entries",
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            dgvRelevantLogs = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            SetupRelevantLogsGrid();
            grpLogs.Controls.Add(dgvRelevantLogs);
            splitContainer.Panel2.Controls.Add(grpLogs);

            // Side Panel - History
            var panelRight = new Panel
            {
                Dock = DockStyle.Right,
                Width = 200,
                Padding = new Padding(5)
            };

            var grpHistory = new GroupBox
            {
                Text = "Recent Questions",
                Dock = DockStyle.Fill
            };

            lstHistory = new ListBox
            {
                Dock = DockStyle.Fill
            };
            lstHistory.DoubleClick += LstHistory_DoubleClick;

            grpHistory.Controls.Add(lstHistory);
            panelRight.Controls.Add(grpHistory);

            // Add all controls
            this.Controls.Add(splitContainer);
            this.Controls.Add(lblStatus);
            this.Controls.Add(panelButtons);
            this.Controls.Add(panelTop);
            this.Controls.Add(panelRight);
        }

        private void TxtQuestion_Enter(object sender, EventArgs e)
        {
            if (txtQuestion.ForeColor == Color.Gray)
            {
                txtQuestion.Text = "";
                txtQuestion.ForeColor = Color.Black;
            }
        }

        private void TxtQuestion_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtQuestion.Text))
            {
                txtQuestion.ForeColor = Color.Gray;
                txtQuestion.Text = "e.g., 'Show me all suspicious activities today' or 'What files did John modify?'";
            }
        }

        private void LstHistory_DoubleClick(object sender, EventArgs e)
        {
            if (lstHistory.SelectedItem != null)
            {
                var selectedText = lstHistory.SelectedItem.ToString();
                if (selectedText.Length > 8)
                {
                    txtQuestion.Text = selectedText.Substring(8); // Remove timestamp
                    txtQuestion.ForeColor = Color.Black;
                }
            }
        }

        private void SetupRelevantLogsGrid()
        {
            dgvRelevantLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Timestamp",
                HeaderText = "Timestamp",
                DataPropertyName = "Timestamp",
                Width = 140
            });

            dgvRelevantLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ChangeType",
                HeaderText = "Change Type",
                DataPropertyName = "ChangeType",
                Width = 90
            });

            dgvRelevantLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FilePath",
                HeaderText = "File Path",
                DataPropertyName = "FilePath",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvRelevantLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ModifiedBy",
                HeaderText = "Modified By",
                DataPropertyName = "ModifiedBy",
                Width = 120
            });
        }

        private async void BtnAsk_Click(object sender, EventArgs e)
        {
            string question = txtQuestion.Text.Trim();

            // Check if it's still the placeholder text (gray color)
            if (string.IsNullOrWhiteSpace(question) || txtQuestion.ForeColor == Color.Gray)
            {
                MessageBox.Show("Please enter a question.", "Input Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_threatService == null)
            {
                MessageBox.Show("AI service not initialized.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            await AskQuestionAsync(question);
        }

        private async Task AskQuestionAsync(string question)
        {
            try
            {
                btnAsk.Enabled = false;
                lblStatus.Text = "🔄 Analyzing logs...";
                lblStatus.ForeColor = Color.Blue;
                txtAnswer.Clear();
                dgvRelevantLogs.DataSource = null;

                var result = await _threatService.QueryLogsAsync(question, maxResults: 10);

                if (result.Success)
                {
                    // Display answer
                    txtAnswer.Text = result.Answer;

                    // Show relevant logs
                    if (result.RelevantLogs != null && result.RelevantLogs.Any())
                    {
                        dgvRelevantLogs.DataSource = result.RelevantLogs;
                    }

                    // Add to history
                    lstHistory.Items.Insert(0, $"{DateTime.Now:HH:mm} - {question}");
                    if (lstHistory.Items.Count > 20)
                    {
                        lstHistory.Items.RemoveAt(lstHistory.Items.Count - 1);
                    }

                    lblStatus.Text = $"✅ Found {result.RelevantLogs.Count} relevant entries (Confidence: {result.ConfidenceScore:P0})";
                    lblStatus.ForeColor = Color.Green;
                }
                else
                {
                    txtAnswer.Text = result.Answer;
                    lblStatus.Text = "⚠️ Query completed with limited results";
                    lblStatus.ForeColor = Color.Orange;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error querying AI: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "❌ Error processing question";
                lblStatus.ForeColor = Color.Red;
            }
            finally
            {
                btnAsk.Enabled = true;
            }
        }

        private void ClearChat()
        {
            txtQuestion.ForeColor = Color.Gray;
            txtQuestion.Text = "e.g., 'Show me all suspicious activities today' or 'What files did John modify?'";
            txtAnswer.Clear();
            dgvRelevantLogs.DataSource = null;
            lblStatus.Text = "Ready";
            lblStatus.ForeColor = Color.Black;
        }
    }
}
#endif
