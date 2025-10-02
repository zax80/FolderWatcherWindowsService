using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FolderWatcherWindowsServiceAdmin.Models;
using FolderWatcherWindowsServiceAdmin.Services;

namespace FolderWatcherWindowsServiceAdmin.Controls
{
    public partial class LogViewerControl : UserControl
    {
        private LogParser _logParser;
        private FileSystemWatcher _logFileWatcher;
        private List<LogEntry> _allEntries;
        private List<LogEntry> _filteredEntries;
        private int _currentPage = 0;
        private int _pageSize = 50;
        private CancellationTokenSource _refreshCancellationTokenSource;
        private volatile bool _isRefreshing = false;
        private string _logFilePath;

        public LogViewerControl()
        {
            _allEntries = new List<LogEntry>();
            _filteredEntries = new List<LogEntry>();
            
            InitializeComponent();
            InitializeLogViewer();
        }

        private void InitializeLogViewer()
        {
            // Initialize page size combo
            cmbPageSize.Items.AddRange(new object[] { 25, 50, 100, 250, 500 });
            cmbPageSize.SelectedItem = 50;

            // Initialize change type filter
            cmbChangeType.Items.AddRange(new object[] { "All", "Created", "Changed", "Deleted", "Renamed" });
            cmbChangeType.SelectedItem = "All";

            // Set up DataGridView
            SetupDataGridView();

            // Set date filters to reasonable defaults
            dtpFromDate.Value = DateTime.Today.AddDays(-7);
            dtpToDate.Value = DateTime.Now;

            // Try to find the log file
            FindLogFile();
        }

        private void FindLogFile()
        {
            try
            {
                // Try multiple possible locations for the log file
                var possiblePaths = new[]
                {
                    Path.Combine(Application.StartupPath, "ServiceLog.txt"),
                    Path.Combine(Application.StartupPath, "..", "FolderWatcherWindowsService", "ServiceLog.txt"),
                    Path.Combine(Application.StartupPath, "FolderWatcherWindowsService.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "FolderWatcher", "ServiceLog.txt")
                };

                foreach (var path in possiblePaths)
                {
                    var logPath = path.EndsWith(".exe") 
                        ? Path.Combine(Path.GetDirectoryName(path), "ServiceLog.txt")
                        : path;

                    if (File.Exists(logPath))
                    {
                        _logFilePath = logPath;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(_logFilePath))
                {
                    _logFilePath = Path.Combine(Application.StartupPath, "ServiceLog.txt");
                }

                lblLogPath.Text = $"Log File: {_logFilePath}";
                _logParser = new LogParser(_logFilePath);

                // Set up file watcher
                SetupFileWatcher();

                // Initial load
                RefreshLogData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error finding log file: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetupDataGridView()
        {
            dataGridViewLogs.AutoGenerateColumns = false;
            dataGridViewLogs.AllowUserToAddRows = false;
            dataGridViewLogs.AllowUserToDeleteRows = false;
            dataGridViewLogs.ReadOnly = true;
            dataGridViewLogs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewLogs.MultiSelect = false;
            dataGridViewLogs.EnableHeadersVisualStyles = false;
            dataGridViewLogs.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;
            dataGridViewLogs.AlternatingRowsDefaultCellStyle.BackColor = Color.LightBlue;

            // Clear existing columns
            dataGridViewLogs.Columns.Clear();

            // Add columns exactly as shown in the screenshot
            dataGridViewLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LogLevel",
                HeaderText = "LogLevel",
                DataPropertyName = "LogLevel",
                Width = 70,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dataGridViewLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Timestamp",
                HeaderText = "Timestamp",
                DataPropertyName = "Timestamp",
                Width = 140,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "MM/d/yyyy H:mm" }
            });

            dataGridViewLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ChangeType",
                HeaderText = "ChangeType",
                DataPropertyName = "ChangeType",
                Width = 90
            });

            dataGridViewLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FilePath",
                HeaderText = "FilePath",
                DataPropertyName = "FilePath",
                Width = 200
            });

            dataGridViewLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ServiceUser",
                HeaderText = "ServiceUser",
                DataPropertyName = "ServiceUser",
                Width = 100
            });

            dataGridViewLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ModifiedBy",
                HeaderText = "ModifiedBy",
                DataPropertyName = "ModifiedBy",
                Width = 100
            });

            dataGridViewLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FileType",
                HeaderText = "FileType",
                DataPropertyName = "FileType",
                Width = 80
            });

            dataGridViewLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FileSize",
                HeaderText = "FileSize",
                DataPropertyName = "FileSize",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dataGridViewLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Extension",
                HeaderText = "Extension",
                DataPropertyName = "Extension",
                Width = 70
            });

            dataGridViewLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LastAccessed",
                HeaderText = "LastAccessed",
                DataPropertyName = "LastAccessed",
                Width = 120
            });

            dataGridViewLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LastModified",
                HeaderText = "LastModified",
                DataPropertyName = "LastModified",
                Width = 120
            });

            dataGridViewLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Error",
                HeaderText = "Error",
                DataPropertyName = "Error",
                Width = 80
            });

            dataGridViewLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RawLine",
                HeaderText = "RawLine",
                DataPropertyName = "RawLine",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 150
            });

            // Handle cell formatting for log level colors only (no red row highlighting)
            dataGridViewLogs.CellFormatting += DataGridViewLogs_CellFormatting;
        }

        private void DataGridViewLogs_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridViewLogs.Rows[e.RowIndex].DataBoundItem is LogEntry entry)
            {
                // Color code log levels in the LogLevel column only (no red row highlighting)
                if (e.ColumnIndex == 0) // LogLevel column
                {
                    switch (entry.LogLevel)
                    {
                        case "ERROR":
                            e.CellStyle.ForeColor = Color.Red;
                            e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                            break;
                        case "WARN":
                            e.CellStyle.ForeColor = Color.Orange;
                            break;
                        case "INFO":
                            e.CellStyle.ForeColor = Color.Blue;
                            break;
                        case "DEBUG":
                            e.CellStyle.ForeColor = Color.Gray;
                            break;
                    }
                }
            }
        }

        private void SetupFileWatcher()
        {
            try
            {
                _logFileWatcher?.Dispose();

                if (!File.Exists(_logFilePath))
                {
                    return;
                }

                var directory = Path.GetDirectoryName(_logFilePath);
                var fileName = Path.GetFileName(_logFilePath);

                _logFileWatcher = new FileSystemWatcher(directory, fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                    EnableRaisingEvents = true
                };

                _logFileWatcher.Changed += LogFileWatcher_Changed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up file watcher: {ex.Message}");
            }
        }

        private async void LogFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (_isRefreshing)
                {
                    return;
                }

                // Debounce file changes
                await Task.Delay(500).ConfigureAwait(false);

                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() => RefreshLogData()));
                }
                else
                {
                    RefreshLogData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LogFileWatcher_Changed: {ex.Message}");
            }
        }

        private async void RefreshLogData()
        {
            if (_isRefreshing)
            {
                return;
            }

            _isRefreshing = true;

            try
            {
                btnRefresh.Enabled = false;
                lblStatus.Text = "Loading...";
                lblStatus.ForeColor = Color.Blue;

                // Cancel any existing refresh operation
                _refreshCancellationTokenSource?.Cancel();
                _refreshCancellationTokenSource = new CancellationTokenSource();

                await Task.Run(() =>
                {
                    if (File.Exists(_logFilePath))
                    {
                        _allEntries = _logParser.ParseLogFile();
                    }
                    else
                    {
                        _allEntries.Clear();
                    }
                }, _refreshCancellationTokenSource.Token).ConfigureAwait(false);

                ApplyFilters();
                UpdateSummary();
                UpdatePagination();
                LoadCurrentPage();

                lblStatus.Text = $"Last updated: {DateTime.Now:HH:mm:ss}";
                lblStatus.ForeColor = Color.Green;
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "Refresh cancelled";
                lblStatus.ForeColor = Color.Orange;
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error: {ex.Message}";
                lblStatus.ForeColor = Color.Red;
                System.Diagnostics.Debug.WriteLine($"Error refreshing log data: {ex.Message}");
            }
            finally
            {
                _isRefreshing = false;
                btnRefresh.Enabled = true;
            }
        }

        private void ApplyFilters()
        {
            var filtered = _allEntries.AsEnumerable();

            // Date range filter
            if (chkDateFilter.Checked)
            {
                var fromDate = dtpFromDate.Value.Date;
                var toDate = dtpToDate.Value.Date.AddDays(1); // Include entire day
                filtered = filtered.Where(e => e.Timestamp >= fromDate && e.Timestamp < toDate);
            }

            // Change type filter
            if (cmbChangeType.SelectedItem?.ToString() != "All")
            {
                var changeType = cmbChangeType.SelectedItem?.ToString();
                filtered = filtered.Where(e => e.ChangeType.Equals(changeType));
            }

            // Text search filter - search displayed columns including RawLine
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchText = txtSearch.Text.ToLowerInvariant();
                filtered = filtered.Where(e =>
                    e.FilePath.ToLowerInvariant().Contains(searchText) ||
                    e.ChangeType.ToLowerInvariant().Contains(searchText) ||
                    e.LogLevel.ToLowerInvariant().Contains(searchText) ||
                    e.ServiceUser.ToLowerInvariant().Contains(searchText) ||
                    e.ModifiedBy.ToLowerInvariant().Contains(searchText) ||
                    e.FileType.ToLowerInvariant().Contains(searchText) ||
                    e.Extension.ToLowerInvariant().Contains(searchText) ||
                    e.Error.ToLowerInvariant().Contains(searchText) ||
                    e.RawLine.ToLowerInvariant().Contains(searchText)
                );
            }

            _filteredEntries = filtered.ToList();
            _currentPage = 0; // Reset to first page when filters change
        }

        private void UpdateSummary()
        {
            var summary = _logParser.GenerateSummary(_filteredEntries);

            lblTotalRecords.Text = $"Total Records: {summary.TotalRecords:N0}";
            lblRecentActivity.Text = $"Recent Activity (30m): {summary.RecentActivityCount:N0}";
            lblErrorCount.Text = $"Errors: {summary.ErrorCount:N0}";
            lblCreated.Text = $"Created: {summary.CreatedCount:N0}";
            lblModified.Text = $"Modified: {summary.ModifiedCount:N0}";
            lblDeleted.Text = $"Deleted: {summary.DeletedCount:N0}";
            lblRenamed.Text = $"Renamed: {summary.RenamedCount:N0}";

            if (summary.LastUpdate.HasValue)
            {
                lblLastUpdate.Text = $"Last Entry: {summary.LastUpdate.Value:yyyy-MM-dd HH:mm:ss}";
            }
            else
            {
                lblLastUpdate.Text = "Last Entry: None";
            }
        }

        private void UpdatePagination()
        {
            _pageSize = (int)cmbPageSize.SelectedItem;
            var totalPages = (int)Math.Ceiling((double)_filteredEntries.Count / _pageSize);

            lblPageInfo.Text = $"Page {_currentPage + 1} of {Math.Max(1, totalPages)} ({_filteredEntries.Count:N0} records)";

            btnPrevPage.Enabled = _currentPage > 0;
            btnNextPage.Enabled = _currentPage < totalPages - 1;
        }

        private void LoadCurrentPage()
        {
            var pageData = _filteredEntries
                .Skip(_currentPage * _pageSize)
                .Take(_pageSize)
                .ToList();

            dataGridViewLogs.DataSource = new BindingList<LogEntry>(pageData);
        }

        // Event Handlers
        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            RefreshLogData();
        }

        private void BtnApplyFilters_Click(object sender, EventArgs e)
        {
            ApplyFilters();
            UpdateSummary();
            UpdatePagination();
            LoadCurrentPage();
        }

        private void BtnClearFilters_Click(object sender, EventArgs e)
        {
            // Reset all filters
            chkDateFilter.Checked = false;
            dtpFromDate.Value = DateTime.Today.AddDays(-7);
            dtpToDate.Value = DateTime.Now;
            cmbChangeType.SelectedItem = "All";
            txtSearch.Clear();

            ApplyFilters();
            UpdateSummary();
            UpdatePagination();
            LoadCurrentPage();
        }

        private void BtnPrevPage_Click(object sender, EventArgs e)
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                UpdatePagination();
                LoadCurrentPage();
            }
        }

        private void BtnNextPage_Click(object sender, EventArgs e)
        {
            var totalPages = (int)Math.Ceiling((double)_filteredEntries.Count / _pageSize);
            if (_currentPage < totalPages - 1)
            {
                _currentPage++;
                UpdatePagination();
                LoadCurrentPage();
            }
        }

        private void CmbPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentPage = 0; // Reset to first page
            UpdatePagination();
            LoadCurrentPage();
        }

        private void TxtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                BtnApplyFilters_Click(sender, e);
                e.Handled = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _refreshCancellationTokenSource?.Cancel();
                _refreshCancellationTokenSource?.Dispose();
                _logFileWatcher?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}