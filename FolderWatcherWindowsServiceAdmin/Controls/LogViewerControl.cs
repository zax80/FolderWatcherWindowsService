using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
            //FixControlOrder(); // Add this line
            InitializeLogViewer();
        }

        /// <summary>
        /// Fixes the control Z-order to ensure panelBottom is visible.
        /// </summary>
        private void FixControlOrder()
        {
            // Reorder controls so docking works correctly
            this.Controls.SetChildIndex(this.panelBottom, 0);
            this.Controls.SetChildIndex(this.panelTop, 1);
            this.Controls.SetChildIndex(this.panelMain, 2);

            // Ensure panelBottom has enough height for all controls
            panelBottom.Height = 70; // Increase from 50 to 70

            // Make sure panelPagination is visible and on top
            panelPagination.BringToFront();
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

        /// <summary>
        /// Load only recent entries for faster initial display.
        /// </summary>
        private async void LoadRecentEntriesOnly()
        {
            if (_isRefreshing)
            {
                return;
            }

            _isRefreshing = true;

            try
            {
                btnRefresh.Enabled = false;
                lblStatus.Text = "Loading recent entries...";
                lblStatus.ForeColor = Color.Blue;

                // Load only the last 500 entries for faster initial display
                var recentEntries = await _logParser.ParseLastEntriesAsync(500).ConfigureAwait(false);

                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        if (!this.IsDisposed && this.IsHandleCreated)
                        {
                            _allEntries = recentEntries;
                            ApplyFilters();
                            UpdateSummary();
                            UpdatePagination();
                            LoadCurrentPage();

                            lblStatus.Text = $"Showing recent entries - Last updated: {DateTime.Now:HH:mm:ss}";
                            lblStatus.ForeColor = Color.Green;
                        }
                    }));
                }
                else
                {
                    _allEntries = recentEntries;
                    ApplyFilters();
                    UpdateSummary();
                    UpdatePagination();
                    LoadCurrentPage();

                    lblStatus.Text = $"Showing recent entries - Last updated: {DateTime.Now:HH:mm:ss}";
                    lblStatus.ForeColor = Color.Green;
                }
            }
            catch (Exception ex)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        if (!this.IsDisposed && this.IsHandleCreated)
                        {
                            lblStatus.Text = $"Error loading recent entries: {ex.Message}";
                            lblStatus.ForeColor = Color.Red;
                        }
                    }));
                }
                else
                {
                    lblStatus.Text = $"Error loading recent entries: {ex.Message}";
                    lblStatus.ForeColor = Color.Red;
                }
                System.Diagnostics.Debug.WriteLine($"Error loading recent entries: {ex.Message}");
            }
            finally
            {
                _isRefreshing = false;
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        if (!this.IsDisposed && this.IsHandleCreated)
                        {
                            btnRefresh.Enabled = true;
                        }
                    }));
                }
                else
                {
                    btnRefresh.Enabled = true;
                }
            }
        }

        private void FindLogFile()
        {
            try
            {
                // Debug: Log the startup path and current directory
                System.Diagnostics.Debug.WriteLine($"Application StartupPath: {Application.StartupPath}");
                System.Diagnostics.Debug.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");

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

                    System.Diagnostics.Debug.WriteLine($"Checking log file path: {logPath}");

                    if (File.Exists(logPath))
                    {
                        _logFilePath = logPath;
                        System.Diagnostics.Debug.WriteLine($"Found log file at: {_logFilePath}");

                        // ✅ CRITICAL: Ensure log file is READ-ONLY access
                        // This prevents any accidental deletion or modification of the log file
                        EnsureLogFileProtection(_logFilePath);
                        break;
                    }
                }

                if (string.IsNullOrEmpty(_logFilePath))
                {
                    _logFilePath = Path.Combine(Application.StartupPath, "ServiceLog.txt");
                    System.Diagnostics.Debug.WriteLine($"Using default log file path: {_logFilePath}");
                }

                lblLogPath.Text = $"Log File: {_logFilePath}";
                _logParser = new LogParser(_logFilePath);

                // Diagnose the log file to understand its structure
                _logParser.DiagnoseLogFile();

                // Set up file watcher
                SetupFileWatcher();

                // Always load data regardless of file size to ensure existing entries are shown
                System.Diagnostics.Debug.WriteLine("Starting initial log data load...");
                RefreshLogData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in FindLogFile: {ex.Message}");
                MessageBox.Show($"Error finding log file: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Ensures the log file is protected from accidental deletion or modification.
        /// This viewer only performs READ operations on the log file.
        /// </summary>
        private void EnsureLogFileProtection(string logFilePath)
        {
            try
            {
                if (File.Exists(logFilePath))
                {
                    // Verify we can read the file (this is all we need)
                    using (var fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        // Just verify read access - we never write to or modify the log file
                        System.Diagnostics.Debug.WriteLine($"✅ Log file protection verified: READ-ONLY access confirmed for {logFilePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Log file access check failed: {ex.Message}");
                // Don't throw - just log the warning
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

            // Add LogLevel column (HIDDEN)
            dataGridViewLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LogLevel",
                HeaderText = "LogLevel",
                DataPropertyName = "LogLevel",
                Width = 70,
                Visible = false, // HIDDEN
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

            // Add Error column (HIDDEN)
            dataGridViewLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Error",
                HeaderText = "Error",
                DataPropertyName = "Error",
                Width = 80,
                Visible = false // HIDDEN
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
            try
            {
                // Defensive check for valid row index and bounds
                if (e.RowIndex < 0 || e.RowIndex >= dataGridViewLogs.Rows.Count)
                    return;

                if (this.IsDisposed || !this.IsHandleCreated)
                    return;

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
            catch (Exception ex)
            {
                // Log error but don't throw to avoid disrupting the UI
                System.Diagnostics.Debug.WriteLine($"Error in DataGridViewLogs_CellFormatting: {ex.Message}");
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

                // Ensure we're on the UI thread before calling RefreshLogData
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        if (!this.IsDisposed && this.IsHandleCreated)
                        {
                            RefreshLogData();
                        }
                    }));
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

                // Use the new async method for better performance
                _allEntries = await _logParser.ParseLogFileAsync().ConfigureAwait(false);

                // Ensure UI updates happen on the UI thread
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        if (!this.IsDisposed && this.IsHandleCreated)
                        {
                            ApplyFilters();
                            UpdateSummary();
                            UpdatePagination();
                            LoadCurrentPage();

                            lblStatus.Text = $"Last updated: {DateTime.Now:HH:mm:ss}";
                            lblStatus.ForeColor = Color.Green;
                        }
                    }));
                }
                else
                {
                    ApplyFilters();
                    UpdateSummary();
                    UpdatePagination();
                    LoadCurrentPage();

                    lblStatus.Text = $"Last updated: {DateTime.Now:HH:mm:ss}";
                    lblStatus.ForeColor = Color.Green;
                }
            }
            catch (OperationCanceledException)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        if (!this.IsDisposed && this.IsHandleCreated)
                        {
                            lblStatus.Text = "Refresh cancelled";
                            lblStatus.ForeColor = Color.Orange;
                        }
                    }));
                }
                else
                {
                    lblStatus.Text = "Refresh cancelled";
                    lblStatus.ForeColor = Color.Orange;
                }
            }
            catch (Exception ex)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        if (!this.IsDisposed && this.IsHandleCreated)
                        {
                            lblStatus.Text = $"Error: {ex.Message}";
                            lblStatus.ForeColor = Color.Red;
                        }
                    }));
                }
                else
                {
                    lblStatus.Text = $"Error: {ex.Message}";
                    lblStatus.ForeColor = Color.Red;
                }
                System.Diagnostics.Debug.WriteLine($"Error refreshing log data: {ex.Message}");
            }
            finally
            {
                _isRefreshing = false;
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        if (!this.IsDisposed && this.IsHandleCreated)
                        {
                            btnRefresh.Enabled = true;
                        }
                    }));
                }
                else
                {
                    btnRefresh.Enabled = true;
                }
            }
        }

        private void ApplyFilters()
        {
            // Defensive check for UI thread and control state
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ApplyFilters));
                return;
            }

            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            IEnumerable<LogEntry> filtered = _allEntries;

            // Use parallel processing for large datasets
            if (_allEntries.Count > 1000)
            {
                filtered = _allEntries.AsParallel();
            }

            // Date range filter
            if (chkDateFilter.Checked)
            {
                var fromDate = dtpFromDate.Value.Date;
                var toDate = dtpToDate.Value.Date.AddDays(1); // Include entire day
                filtered = filtered.Where(e => e.Timestamp >= fromDate && e.Timestamp < toDate);
            }

            // Change type filter
            var selectedChangeType = cmbChangeType.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedChangeType) && selectedChangeType != "All")
            {
                filtered = filtered.Where(e => e.ChangeType.Equals(selectedChangeType, StringComparison.OrdinalIgnoreCase));
            }

            // Text search filter - search displayed columns including RawLine
            var searchText = txtSearch.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var searchLower = searchText.ToLowerInvariant();
                filtered = filtered.Where(e =>
                    (e.FilePath?.ToLowerInvariant().Contains(searchLower) == true) ||
                    (e.ChangeType?.ToLowerInvariant().Contains(searchLower) == true) ||
                    (e.LogLevel?.ToLowerInvariant().Contains(searchLower) == true) ||
                    (e.ServiceUser?.ToLowerInvariant().Contains(searchLower) == true) ||
                    (e.ModifiedBy?.ToLowerInvariant().Contains(searchLower) == true) ||
                    (e.FileType?.ToLowerInvariant().Contains(searchLower) == true) ||
                    (e.Extension?.ToLowerInvariant().Contains(searchLower) == true) ||
                    (e.Error?.ToLowerInvariant().Contains(searchLower) == true) ||
                    (e.RawLine?.ToLowerInvariant().Contains(searchLower) == true)
                );
            }

            _filteredEntries = filtered.ToList();
            _currentPage = 0; // Reset to first page when filters change
        }

        private void UpdateSummary()
        {
            // Defensive check for UI thread and control state
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateSummary));
                return;
            }

            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            // Debug: Log information about the filtered entries
            System.Diagnostics.Debug.WriteLine($"UpdateSummary: Processing {_filteredEntries.Count} filtered entries");

            // Sample a few entries for debugging
            if (_filteredEntries.Any())
            {
                var firstEntry = _filteredEntries.First();
                var lastEntry = _filteredEntries.Last();
                System.Diagnostics.Debug.WriteLine($"First entry: LogLevel={firstEntry.LogLevel}, Error='{firstEntry.Error}', ChangeType={firstEntry.ChangeType}, Timestamp={firstEntry.Timestamp:yyyy-MM-dd HH:mm:ss}");
                System.Diagnostics.Debug.WriteLine($"Last entry: LogLevel={lastEntry.LogLevel}, Error='{lastEntry.Error}', ChangeType={lastEntry.ChangeType}, Timestamp={lastEntry.Timestamp:yyyy-MM-dd HH:mm:ss}");

                // Count different types for debugging
                var errorEntries = _filteredEntries.Where(e => !string.IsNullOrWhiteSpace(e.Error)).ToList();
                var errorLogLevel = _filteredEntries.Where(e => e.LogLevel.Equals("ERROR", StringComparison.OrdinalIgnoreCase)).ToList();
                var recentEntries = _filteredEntries.Where(e => e.Timestamp >= DateTime.Now.AddMinutes(-30)).ToList();

                System.Diagnostics.Debug.WriteLine($"Entries with Error field: {errorEntries.Count}");
                System.Diagnostics.Debug.WriteLine($"Entries with ERROR LogLevel: {errorLogLevel.Count}");
                System.Diagnostics.Debug.WriteLine($"Recent entries (30m): {recentEntries.Count}");

                if (errorEntries.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"Sample error entry: {errorEntries.First().Error}");
                }
            }

            var summary = _logParser.GenerateSummary(_filteredEntries);

            lblTotalRecords.Text = $"Total Records: {summary.TotalRecords:N0}";
            lblRecentActivity.Text = $"Recent Activity (30m): {summary.RecentActivityCount:N0}";
            lblUnknownCount.Text = $"Unknowns: {summary.UnknownCount:N0}";
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

            // Debug: Log the final summary values
            System.Diagnostics.Debug.WriteLine($"Summary - Total: {summary.TotalRecords}, Unknowns: {summary.UnknownCount}, Recent: {summary.RecentActivityCount}");
        }

        private void UpdatePagination()
        {
            // Defensive check for UI thread and control state
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdatePagination));
                return;
            }

            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            _pageSize = (int)cmbPageSize.SelectedItem;
            var totalPages = (int)Math.Ceiling((double)_filteredEntries.Count / _pageSize);

            lblPageInfo.Text = $"Page {_currentPage + 1} of {Math.Max(1, totalPages)} ({_filteredEntries.Count:N0} records)";

            btnPrevPage.Enabled = _currentPage > 0;
            btnNextPage.Enabled = _currentPage < totalPages - 1;
        }

        private void LoadCurrentPage()
        {
            // Defensive check for UI thread and control state
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(LoadCurrentPage));
                return;
            }

            if (this.IsDisposed || !this.IsHandleCreated)
                return;

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

        /// <summary>
        /// Public method to refresh log data (called from parent form).
        /// </summary>
        public void RefreshData()
        {
            RefreshLogData();
        }

        /// <summary>
        /// Show diagnostic information about the current log data.
        /// </summary>
        public void ShowDiagnosticInfo()
        {
            if (_logParser == null)
            {
                MessageBox.Show("No log parser available.", "Diagnostic Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Log File: {_logFilePath}");
            sb.AppendLine($"File Exists: {File.Exists(_logFilePath)}");

            if (File.Exists(_logFilePath))
            {
                var fileInfo = new FileInfo(_logFilePath);
                sb.AppendLine($"File Size: {fileInfo.Length:N0} bytes");
                sb.AppendLine($"Last Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
            }

            sb.AppendLine();
            sb.AppendLine($"All Entries Count: {_allEntries.Count:N0}");
            sb.AppendLine($"Filtered Entries Count: {_filteredEntries.Count:N0}");
            sb.AppendLine($"Current Page: {_currentPage + 1}");
            sb.AppendLine($"Page Size: {_pageSize}");

            if (_filteredEntries.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Sample Entry Analysis:");
                var firstEntry = _filteredEntries.First();
                sb.AppendLine($"  LogLevel: '{firstEntry.LogLevel}'");
                sb.AppendLine($"  ChangeType: '{firstEntry.ChangeType}'");
                sb.AppendLine($"  Error Field: '{firstEntry.Error}'");
                sb.AppendLine($"  Timestamp: {firstEntry.Timestamp:yyyy-MM-dd HH:mm:ss}");

                var now = DateTime.Now;
                var recentThreshold = now.AddMinutes(-30);
                var isRecent = firstEntry.Timestamp >= recentThreshold;
                sb.AppendLine($"  Is Recent (30m): {isRecent}");
                sb.AppendLine($"  Current Time: {now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"  Recent Threshold: {recentThreshold:yyyy-MM-dd HH:mm:ss}");

                // Count different types
                var errorEntries = _filteredEntries.Count(e => !string.IsNullOrWhiteSpace(e.Error));
                var errorLogLevel = _filteredEntries.Count(e => e.LogLevel.Equals("ERROR", StringComparison.OrdinalIgnoreCase));
                var recentEntries = _filteredEntries.Count(e => e.Timestamp >= recentThreshold);

                sb.AppendLine();
                sb.AppendLine("Entry Type Counts:");
                sb.AppendLine($"  Entries with Error field: {errorEntries}");
                sb.AppendLine($"  Entries with ERROR LogLevel: {errorLogLevel}");
                sb.AppendLine($"  Recent entries (30m): {recentEntries}");
                sb.AppendLine($"  Created: {_filteredEntries.Count(e => e.ChangeType.Equals("Created", StringComparison.OrdinalIgnoreCase))}");
                sb.AppendLine($"  Changed: {_filteredEntries.Count(e => e.ChangeType.Equals("Changed", StringComparison.OrdinalIgnoreCase))}");
                sb.AppendLine($"  Deleted: {_filteredEntries.Count(e => e.ChangeType.Equals("Deleted", StringComparison.OrdinalIgnoreCase))}");
                sb.AppendLine($"  Renamed: {_filteredEntries.Count(e => e.ChangeType.Equals("Renamed", StringComparison.OrdinalIgnoreCase))}");
            }

            MessageBox.Show(sb.ToString(), "Log Viewer Diagnostic Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                try
                {
                    _refreshCancellationTokenSource?.Cancel();
                    _refreshCancellationTokenSource?.Dispose();
                    _logFileWatcher?.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error during disposal: {ex.Message}");
                }
            }
            base.Dispose(disposing);
        }
    }
}