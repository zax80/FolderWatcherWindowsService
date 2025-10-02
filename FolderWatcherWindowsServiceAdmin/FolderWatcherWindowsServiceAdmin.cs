using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FolderWatcherWindowsServiceAdmin
{
    public partial class FolderWatcherWindowsServiceAdmin : Form
    {
        // service name
        private const string serviceName = "FolderWatcherWindowsService";

        // service management
        private ServiceController serviceController;
        private ServiceControllerStatus previousServiceStatus;

        // service status with thread safety
        private volatile ServiceControllerStatus serviceControllerStatus;
        private readonly object statusLock = new object();

        // button text
        public enum BtnStartStopText
        {
            Start,
            Starting,
            Stop,
            Stopping
        }

        private CancellationTokenSource cancellationTokenSource;
        private volatile bool isUpdatingUI;
        private volatile bool isServiceOperationInProgress;

        public FolderWatcherWindowsServiceAdmin()
        {
            // create ServiceController instance for FolderWatcherWindowsService
            serviceController = new ServiceController(serviceName);

            InitializeComponent();
        }

        /// <summary>
        /// On form load.
        /// </summary>
        /// <param name="sender">form</param>
        /// <param name="e">event arguments</param>
        private void FolderWatcherWindowsServiceAdmin_Load(object sender, EventArgs e)
        {
            CheckPermissions();
            InitializeServiceMonitoring();

            // Set the default tab to Service Control, but user can navigate to Log Viewer
            tabControl1.SelectedIndex = 0;
        }

        /// <summary>
        /// Initialize service monitoring with proper error handling
        /// </summary>
        private void InitializeServiceMonitoring()
        {
            try
            {
                RefreshServiceStatus();
                UpdateUI();
                StartServiceMonitoring();
            }
            catch (Exception ex)
            {
                HandleServiceError(ex, "Failed to initialize service monitoring");
            }
        }

        /// <summary>
        /// Refresh service status with proper error handling
        /// </summary>
        private void RefreshServiceStatus()
        {
            try
            {
                if (ServiceExists())
                {
                    serviceController.Refresh();
                    lock (statusLock)
                    {
                        previousServiceStatus = serviceControllerStatus;
                        serviceControllerStatus = serviceController.Status;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing service status: {ex.Message}");
            }
        }

        /// <summary>
        /// Start service monitoring with improved error handling
        /// </summary>
        private void StartServiceMonitoring()
        {
            // Cancel existing monitoring
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            
            cancellationTokenSource = new CancellationTokenSource();
            _ = MonitorServiceStatusAsync(cancellationTokenSource.Token);
        }

        /// <summary>
        /// Check if the current user has administrator privileges.
        /// </summary>
        /// <returns>True if user is administrator, false otherwise</returns>
        private bool IsUserAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check permissions and warn user if needed.
        /// </summary>
        private void CheckPermissions()
        {
            if (!IsUserAdministrator())
            {
                MessageBox.Show(
                    "Warning: You are not running as Administrator.\n\n" +
                    "Please restart this application as Administrator to perform these operations.",
                    "Administrator Privileges Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                Application.Exit();
            }
        }

        /// <summary>
        /// Check if user has permission to control services.
        /// </summary>
        /// <returns>True if user can control services, false otherwise</returns>
        private bool CanControlServices()
        {
            try
            {
                // Try to access the service to check permissions
                ServiceControllerStatus status = serviceController.Status;

                // Try to check if we can start/stop (this will throw if no permission)
                // We're not actually starting/stopping, just checking access
                ServiceControllerPermissionAccess access = ServiceControllerPermissionAccess.Control;
                ServiceControllerPermission permission = new ServiceControllerPermission(access, Environment.MachineName, serviceName);
                permission.Demand();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// On form closing.
        /// </summary>
        /// <param name="sender">form</param>
        /// <param name="e">event arguments</param>
        private void FolderWatcherWindowsServiceAdmin_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cancel monitoring before disposing
            cancellationTokenSource?.Cancel();
            
            // Give monitoring task time to complete
            Task.Delay(100).Wait();
            
            serviceController?.Dispose();
            cancellationTokenSource?.Dispose();
        }

        /// <summary>
        /// Update service status with improved thread safety and error handling.
        /// </summary>
        private void UpdateUI()
        {
            try
            {
                // Prevent concurrent UI updates
                if (isUpdatingUI) return;

                // Check if we're on the UI thread
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(UpdateUI));
                    return;
                }

                // Check if form is disposed or handle not created
                if (this.IsDisposed || !this.IsHandleCreated)
                    return;

                isUpdatingUI = true;

                // get status
                if (ServiceExists())
                {
                    RefreshServiceStatus();
                    
                    ServiceControllerStatus currentStatus;
                    lock (statusLock)
                    {
                        currentStatus = serviceControllerStatus;
                    }

                    UpdateUIForServiceStatus(currentStatus);

                    // Disable buttons if user doesn't have permissions or service operation is in progress
                    bool hasPermissions = IsUserAdministrator() && CanControlServices();
                    bool enableButtons = hasPermissions && !isServiceOperationInProgress;
                    
                    // Only enable button if not in transition state
                    if (currentStatus == ServiceControllerStatus.StartPending || 
                        currentStatus == ServiceControllerStatus.StopPending)
                    {
                        enableButtons = false;
                    }
                    
                    btnChangeStatus.Enabled = enableButtons;
                }
                else
                {
                    lblStatus.Text = "Not Installed";
                    btnChangeStatus.Text = "Install";
                    btnChangeStatus.Enabled = IsUserAdministrator() && !isServiceOperationInProgress;

                    // Stop monitoring if service doesn't exist
                    if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
                    {
                        cancellationTokenSource.Cancel();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleServiceError(ex, "Unable to update UI");
            }
            finally
            {
                isUpdatingUI = false;
            }
        }

        /// <summary>
        /// Update UI elements based on service status
        /// </summary>
        private void UpdateUIForServiceStatus(ServiceControllerStatus status)
        {
            switch (status)
            {
                case ServiceControllerStatus.StartPending:
                    lblStatus.Text = "Starting...";
                    btnChangeStatus.Text = BtnStartStopText.Starting.ToString();
                    break;

                case ServiceControllerStatus.StopPending:
                    lblStatus.Text = "Stopping...";
                    btnChangeStatus.Text = BtnStartStopText.Stopping.ToString();
                    break;

                case ServiceControllerStatus.Running:
                    lblStatus.Text = "Running";
                    btnChangeStatus.Text = BtnStartStopText.Stop.ToString();
                    break;

                case ServiceControllerStatus.Stopped:
                    lblStatus.Text = "Stopped";
                    btnChangeStatus.Text = BtnStartStopText.Start.ToString();
                    break;

                case ServiceControllerStatus.Paused:
                    lblStatus.Text = "Paused";
                    btnChangeStatus.Text = BtnStartStopText.Start.ToString();
                    break;

                case ServiceControllerStatus.PausePending:
                    lblStatus.Text = "Pausing...";
                    btnChangeStatus.Text = BtnStartStopText.Starting.ToString();
                    break;

                case ServiceControllerStatus.ContinuePending:
                    lblStatus.Text = "Resuming...";
                    btnChangeStatus.Text = BtnStartStopText.Starting.ToString();
                    break;

                default:
                    lblStatus.Text = status.ToString();
                    btnChangeStatus.Text = BtnStartStopText.Start.ToString();
                    break;
            }
        }

        /// <summary>
        /// Handle service-related errors consistently
        /// </summary>
        private void HandleServiceError(Exception ex, string context)
        {
            string errorMessage = $"{context}: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(errorMessage);
            
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => {
                    if (!this.IsDisposed && this.IsHandleCreated)
                    {
                        lblStatus.Text = "Error";
                        btnChangeStatus.Enabled = false;
                    }
                }));
            }
            else
            {
                if (!this.IsDisposed && this.IsHandleCreated)
                {
                    lblStatus.Text = "Error";
                    btnChangeStatus.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Check if the service exists.
        /// </summary>
        /// <returns>True if service exists, false otherwise</returns>
        private bool ServiceExists()
        {
            try
            {
                ServiceControllerStatus status = serviceController.Status;
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        /// <summary>
        /// Get the service log file path.
        /// </summary>
        /// <returns>Full path to the service log file</returns>
        private string GetServiceLogPath()
        {
            // The log file is configured as "ServiceLog.txt" relative to the service executable
            // First try the service installation directory
            string serviceExePath = Path.Combine(Application.StartupPath, "FolderWatcherWindowsService.exe");
            string serviceDirectory = Path.GetDirectoryName(serviceExePath);
            string logPath = Path.Combine(serviceDirectory, "ServiceLog.txt");
            
            if (File.Exists(logPath))
            {
                return logPath;
            }
            
            // Fallback to admin application directory
            string adminLogPath = Path.Combine(Application.StartupPath, "ServiceLog.txt");
            if (File.Exists(adminLogPath))
            {
                return adminLogPath;
            }
            
            // Return the expected path even if file doesn't exist yet
            return logPath;
        }

        /// <summary>
        /// Install the Windows service.
        /// </summary>
        /// <returns>True if installation succeeded, false otherwise</returns>
        private bool InstallService()
        {
            if (!IsUserAdministrator())
            {
                MessageBox.Show("Administrator privileges are required to install services.",
                    "Permission Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                string serviceExePath = Path.Combine(Application.StartupPath, "FolderWatcherWindowsService.exe");

                if (!File.Exists(serviceExePath))
                {
                    MessageBox.Show($"Service executable not found at: {serviceExePath}", "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"create \"{serviceName}\" binPath= \"{serviceExePath}\" start= auto",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Verb = "runas"
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (process.ExitCode == 0)
                    {
                        MessageBox.Show($"Service '{serviceName}' installed successfully.", "Installation Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        return true;
                    }
                    else
                    {
                        MessageBox.Show($"Failed to install service. Error: {error}", "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error installing service: {ex.Message}", "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Uninstall the Windows service.
        /// </summary>
        /// <returns>True if uninstallation succeeded, false otherwise</returns>
        private bool UninstallService()
        {
            if (!IsUserAdministrator())
            {
                MessageBox.Show("Administrator privileges are required to uninstall services.",
                    "Permission Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                // Stop service if running
                if (ServiceExists() && serviceController.Status == ServiceControllerStatus.Running)
                {
                    serviceController.Stop();
                    serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"delete \"{serviceName}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Verb = "runas"
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (process.ExitCode == 0)
                    {
                        MessageBox.Show($"Service '{serviceName}' uninstalled successfully.", "Uninstall Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show($"Failed to uninstall service. Error: {error}", "Uninstall Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error uninstalling service: {ex.Message}", "Uninstall Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// btnChangeStatus click with improved error handling and user feedback.
        /// </summary>
        /// <param name="sender">btnChangeStatus</param>
        /// <param name="e">event arguments</param>
        public async void BtnChangeStatus_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                if (!IsUserAdministrator())
                {
                    MessageBox.Show("Administrator privileges are required to start/stop services.",
                        "Permission Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (isServiceOperationInProgress)
                {
                    MessageBox.Show("A service operation is already in progress. Please wait for it to complete.",
                        "Operation In Progress", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!ServiceExists())
                {
                    // Install the service
                    isServiceOperationInProgress = true;
                    UpdateUI();

                    try
                    {
                        if (InstallService())
                        {
                            // Refresh service controller
                            serviceController.Dispose();
                            serviceController = new ServiceController(serviceName);

                            // Wait a moment for service to be registered
                            await Task.Delay(1000).ConfigureAwait(false);

                            RefreshServiceStatus();
                            UpdateUI();
                            StartServiceMonitoring();
                        }
                    }
                    finally
                    {
                        isServiceOperationInProgress = false;
                        UpdateUI();
                    }
                    return;
                }

                await PerformServiceOperationAsync(cancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Perform service start/stop operation asynchronously
        /// </summary>
        private async Task PerformServiceOperationAsync(CancellationToken cancellationToken)
        {
            isServiceOperationInProgress = true;
            UpdateUI();

            try
            {
                ServiceControllerStatus currentStatus;
                lock (statusLock)
                {
                    currentStatus = serviceControllerStatus;
                }

                if (currentStatus == ServiceControllerStatus.Running)
                {
                    await StopServiceAsync(cancellationToken).ConfigureAwait(false);
                }
                else if (currentStatus == ServiceControllerStatus.Stopped)
                {
                    await StartServiceAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    MessageBox.Show("Service status must be Running or Stopped to perform this operation.", 
                        "Invalid Service State", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error controlling service: {ex.Message}\n\nPlease ensure you have administrator privileges.",
                    "Service Control Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isServiceOperationInProgress = false;
                RefreshServiceStatus();
                UpdateUI();
            }
        }

        /// <summary>
        /// Start service asynchronously with timeout and cancellation support
        /// </summary>
        private async Task StartServiceAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                serviceController.Start();
                serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
            }, cancellationToken).ConfigureAwait(false);

            // Give service a moment to initialize
            await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Stop service asynchronously with timeout and cancellation support
        /// </summary>
        private async Task StopServiceAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                serviceController.Stop();
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
            }, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// btnUninstall click event handler.
        /// </summary>
        /// <param name="sender">btnUninstall</param>
        /// <param name="e">event arguments</param>
        public void BtnUninstall_Click(object sender, EventArgs e)
        {
            if (!IsUserAdministrator())
            {
                MessageBox.Show("Administrator privileges are required to uninstall services.",
                    "Permission Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!ServiceExists())
            {
                MessageBox.Show($"Service '{serviceName}' is not installed.", "Service Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show($"Are you sure you want to uninstall the '{serviceName}' service?", "Confirm Uninstall", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (UninstallService())
                {
                    // Stop monitoring and refresh UI
                    cancellationTokenSource?.Cancel();
                    UpdateUI();
                }
            }
        }

        /// <summary>
        /// Monitor service status with improved error handling and recovery
        /// </summary>
        private async Task MonitorServiceStatusAsync(CancellationToken cancellationToken)
        {
            int consecutiveErrors = 0;
            const int maxConsecutiveErrors = 5;
            
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (ServiceExists())
                    {
                        serviceController.Refresh();
                        ServiceControllerStatus currentStatus = serviceController.Status;

                        ServiceControllerStatus previousStatus;
                        lock (statusLock)
                        {
                            previousStatus = serviceControllerStatus;
                            if (currentStatus != serviceControllerStatus)
                            {
                                previousServiceStatus = serviceControllerStatus;
                                serviceControllerStatus = currentStatus;
                            }
                        }

                        if (currentStatus != previousStatus)
                        {
                            // Check if form handle is created and form is not disposed
                            if (this.IsHandleCreated && !this.IsDisposed)
                            {
                                try
                                {
                                    // Use BeginInvoke for better responsiveness
                                    this.BeginInvoke(new Action(() => {
                                        try
                                        {
                                            if (!this.IsDisposed && this.IsHandleCreated)
                                            {
                                                OnServiceStatusChanged(currentStatus);
                                            }
                                        }
                                        catch (Exception uiEx)
                                        {
                                            System.Diagnostics.Debug.WriteLine($"UI update error: {uiEx.Message}");
                                        }
                                    }));
                                }
                                catch (InvalidOperationException invEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Invoke error: {invEx.Message}");
                                    break; // Form might be closing
                                }
                            }
                        }
                        
                        consecutiveErrors = 0; // Reset error count on success
                    }
                    else
                    {
                        // Service doesn't exist, stop monitoring
                        break;
                    }
                    
                    await Task.Delay(750, cancellationToken).ConfigureAwait(false); // Slightly longer delay for better performance
                }
                catch (OperationCanceledException)
                {
                    break; // Expected when cancellation is requested
                }
                catch (Exception ex)
                {
                    consecutiveErrors++;
                    System.Diagnostics.Debug.WriteLine($"Status monitoring error (attempt {consecutiveErrors}): {ex.Message}");
                    
                    if (consecutiveErrors >= maxConsecutiveErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("Too many consecutive monitoring errors, stopping monitoring");
                        break;
                    }
                    
                    // Progressive delay on errors
                    int delayMs = Math.Min(5000, 1000 * consecutiveErrors);
                    await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Called when service status changes with improved handling
        /// </summary>
        private void OnServiceStatusChanged(ServiceControllerStatus newStatus)
        {
            try
            {
                // Update UI elements based on status
                UpdateUIForServiceStatus(newStatus);
                
                // Update button state based on operation progress and permissions
                bool hasPermissions = IsUserAdministrator() && CanControlServices();
                bool enableButtons = hasPermissions && !isServiceOperationInProgress;
                
                // Disable button during transition states
                if (newStatus == ServiceControllerStatus.StartPending || 
                    newStatus == ServiceControllerStatus.StopPending ||
                    newStatus == ServiceControllerStatus.PausePending ||
                    newStatus == ServiceControllerStatus.ContinuePending)
                {
                    enableButtons = false;
                }
                
                btnChangeStatus.Enabled = enableButtons;

                // Log status change for debugging
                System.Diagnostics.Debug.WriteLine($"Service status changed to: {newStatus}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnServiceStatusChanged: {ex.Message}");
            }
        }

        private void logViewerControl_Load(object sender, EventArgs e)
        {

        }
    }
}