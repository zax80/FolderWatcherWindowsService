using FolderWatcherWindowsService.Common.Interfaces;

using FolderWatcherWindowsServiceAdmin.Controls;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

#if PREMIUM
using FolderWatcherWindowsService.ThreatDetection.Services;
#endif

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

        // folder management
        private FolderManager folderManager;
        private List<string> watchedFolders;

#if PREMIUM
        // License management
        private ILicenseService _licenseService;

        // AI Control
        private AiAssistantControl aiAssistantControl;

        // Threat Detection Service
        private ThreatDetectionService _threatDetectionService;

        // Logger
        private ILog _logger;
#endif

        public FolderWatcherWindowsServiceAdmin()
        {
            // create ServiceController instance for FolderWatcherWindowsService
            serviceController = new ServiceController(serviceName);

            InitializeComponent();
            
            // Initialize folder management
            InitializeFolderManagement();

            // CRITICAL FIX: Initialize layout for both FREE and PREMIUM
            InitializeFormLayout();


        }

#if PREMIUM
        /// <summary>
        /// Constructor with license service injection.
        /// </summary>
        public FolderWatcherWindowsServiceAdmin(ILicenseService licenseService) : this()
        {
            _licenseService = licenseService;
            
            // CRITICAL: Initialize license UI AFTER base constructor completes
            InitializeLicenseUI();
        }

        /// <summary>
        /// Initialize license-related UI elements.
        /// </summary>
        private void InitializeLicenseUI()
        {
            // Update form title with license info
            UpdateFormTitle();
            
            bool showLicenseBar = true;
            
#if !DEBUG
            // In Release builds, check if we should hide the license bar for valid paid licenses
            showLicenseBar = ShouldShowLicenseBar();
            
            if (!showLicenseBar)
            {
                System.Diagnostics.Debug.WriteLine("InitializeLicenseUI: Hiding license bar (valid paid license)");
            }
#endif
    
            if (showLicenseBar)
            {
                System.Diagnostics.Debug.WriteLine("InitializeLicenseUI: Creating license bar");
                
                // Create a new panel for license controls at the top of the form
                var licensePanel = new Panel
                {
                    Name = "licensePanel",
                    Height = 35,
                    Dock = DockStyle.Top,
                    BackColor = System.Drawing.Color.LightYellow,
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Add Buy License link
                var lnkBuyLicense = new LinkLabel
                {
                    Text = "🛒 Buy License",
                    AutoSize = true,
                    Location = new System.Drawing.Point(10, 10),
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold)
                };
                lnkBuyLicense.Click += LnkBuyLicense_Click;

                // Add Enter License button
                var btnEnterLicense = new Button
                {
                    Text = "🔑 Enter License",
                    AutoSize = true,
                    Location = new System.Drawing.Point(120, 6),
                    Height = 25
                };
                btnEnterLicense.Click += BtnEnterLicense_Click;

                // Add license status label
                var lblLicenseStatus = new Label
                {
                    Name = "lblLicenseStatus",
                    AutoSize = true,
                    Location = new System.Drawing.Point(240, 10),
                    ForeColor = System.Drawing.Color.DarkBlue
                };

#if DEBUG  // Only in debug builds
                var btnClearLicense = new Button
                {
                    Text = "🧪 Clear License (Test)",
                    AutoSize = true,
                    Location = new System.Drawing.Point(420, 6),
                    Height = 25,
                    BackColor = System.Drawing.Color.LightCoral
                };
                btnClearLicense.Click += (s, ev) => {
                    try
                    {
                        using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\FolderWatcher", true))
                        {
                            key?.DeleteSubKey("License", false);
                        }
                        
                        string licensePath = @"C:\ProgramData\FolderWatcher\license.dat";
                        if (File.Exists(licensePath))
                            File.Delete(licensePath);
                        
                        MessageBox.Show("License cleared! Restart the app to test.", "Test Helper");
                        Application.Exit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}", "Error");
                    }
                };
                licensePanel.Controls.Add(btnClearLicense);
#endif
                licensePanel.Controls.Add(lnkBuyLicense);
                licensePanel.Controls.Add(btnEnterLicense);
                licensePanel.Controls.Add(lblLicenseStatus);

                // Add panel to form
                this.Controls.Add(licensePanel);
                licensePanel.BringToFront();
                
                // Update license status
                UpdateLicenseStatus();
                
                // CRITICAL: Re-adjust layout after adding license panel
                AdjustFormLayout(licensePanel);
            }
            else
            {
                // No license panel needed - layout was already initialized
                System.Diagnostics.Debug.WriteLine("InitializeLicenseUI: No license panel needed");
            }

            // Show trial reminder on startup if in trial mode
            ShowTrialReminderOnStartup();
        }

        private void InitializeAITab()
        {
            // Create AI Assistant tab
            var tabAI = new TabPage("AI Assistant 🤖");

            aiAssistantControl = new AiAssistantControl
            {
                Dock = DockStyle.Fill
            };

            tabAI.Controls.Add(aiAssistantControl);
            tabControl1.TabPages.Add(tabAI);

            // Initialize threat service
            InitializeThreatService();
        }

        private void InitializeThreatService()
        {
            try
            {
                var logger = LogManager.GetLogger(typeof(FolderWatcherWindowsServiceAdmin));
                //_threatDetectionService = new ThreatDetectionService(logger);

                if (_threatDetectionService.Initialize())
                {
                    //aiAssistantControl.SetThreatService(_threatDetectionService);
                    //logViewerControl.SetThreatService(_threatDetectionService);

                    _logger?.Info("Threat detection service initialized for Admin UI");
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Failed to initialize threat service: {ex.Message}", ex);
                MessageBox.Show("AI features may not be available. Check configuration.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
#endif

        /// <summary>
        /// Initialize form layout (works for both FREE and PREMIUM versions).
        /// This ensures proper control sizing and anchoring.
        /// </summary>
        private void InitializeFormLayout()
        {
            System.Diagnostics.Debug.WriteLine("InitializeFormLayout: Starting layout initialization");
            
            AdjustFormLayout(null);

            CheckPermissions();
        }

        /// <summary>
        /// Adjust form layout based on whether a license panel exists.
        /// </summary>
        /// <param name="licensePanel">The license panel if it exists, null otherwise</param>
        private void AdjustFormLayout(Panel licensePanel)
        {
            // Find the TabControl
            var tabControl = this.Controls.Find("tabControl1", true).FirstOrDefault() as TabControl;
            if (tabControl == null)
            {
                System.Diagnostics.Debug.WriteLine("AdjustFormLayout: WARNING - TabControl not found!");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"AdjustFormLayout: Found TabControl, current Top={tabControl.Top}, Height={tabControl.Height}");
            
            // Adjust TabControl position based on whether license panel exists
            if (licensePanel != null)
            {
                // Move TabControl down to make room for license panel
                tabControl.Top = licensePanel.Bottom;
                System.Diagnostics.Debug.WriteLine($"AdjustFormLayout: License panel exists - TabControl.Top set to {tabControl.Top}");
            }
            else
            {
                // No license panel - TabControl should start at top
                tabControl.Top = 0;
                System.Diagnostics.Debug.WriteLine("AdjustFormLayout: No license panel - TabControl.Top set to 0");
            }
            
            // CRITICAL: Calculate height BEFORE setting it
            int tabControlHeight = this.ClientSize.Height - tabControl.Top;
            tabControl.Height = tabControlHeight;
            
            // Ensure proper anchoring for resize
            tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            
            System.Diagnostics.Debug.WriteLine($"AdjustFormLayout: TabControl adjusted - Top={tabControl.Top}, Height={tabControl.Height}");
            
            // Fix the layout of the Service Control tab (first tab)
            if (tabControl.TabPages.Count > 0)
            {
                var serviceControlTab = tabControl.TabPages[0];
                System.Diagnostics.Debug.WriteLine($"AdjustFormLayout: Fixing layout for tab '{serviceControlTab.Text}'");
                
                // CRITICAL: Calculate available height for controls
                int availableHeight = tabControl.Height - tabControl.ItemSize.Height - 8; // Subtract tab header height and padding
                
                GroupBox grpServiceControl = null;
                GroupBox grpFolderConfig = null;
                
                // Find and fix the groupboxes in Service Control tab
                foreach (Control control in serviceControlTab.Controls)
                {
                    if (control is GroupBox groupBox)
                    {
                        System.Diagnostics.Debug.WriteLine($"AdjustFormLayout: Found GroupBox '{groupBox.Name}' with Anchor={groupBox.Anchor}");
                        
                        // Service Control groupbox should stay at top
                        if (groupBox.Name == "groupBoxService" || groupBox.Text.Contains("Service Control"))
                        {
                            grpServiceControl = groupBox;
                            groupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                            System.Diagnostics.Debug.WriteLine($"AdjustFormLayout: Set Service Control anchor to Top|Left|Right, Height={groupBox.Height}");
                        }
                        // Watched Folders Configuration should fill remaining space
                        else if (groupBox.Name == "groupBoxFolders" || groupBox.Text.Contains("Watched Folders"))
                        {
                            grpFolderConfig = groupBox;
                            groupBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                            System.Diagnostics.Debug.WriteLine($"AdjustFormLayout: Set Watched Folders anchor to Top|Bottom|Left|Right");
                            
                            // CRITICAL: Ensure ALL child controls have proper anchoring
                            foreach (Control innerControl in groupBox.Controls)
                            {
                                if (innerControl is ListBox listBox)
                                {
                                    listBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                                    System.Diagnostics.Debug.WriteLine($"AdjustFormLayout: Set ListBox anchor to Top|Bottom|Left|Right");
                                }
                                else if (innerControl is Button btn)
                                {
                                    // CRITICAL FIX: Buttons should anchor to the bottom
                                    if (btn.Name == "btnAddFolder" || btn.Name == "btnRemoveFolder" || btn.Name == "btnEditFolder" ||
                                        btn.Text.Contains("Add") || btn.Text.Contains("Remove") || btn.Text.Contains("Edit"))
                                    {
                                        btn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                                        System.Diagnostics.Debug.WriteLine($"AdjustFormLayout: Set Button '{btn.Name}' anchor to Bottom|Left");
                                    }
                                }
                            }
                        }
                    }
                }
                
                // CRITICAL FIX: Adjust folder config groupbox height based on service control groupbox
                if (grpServiceControl != null && grpFolderConfig != null)
                {
                    // Calculate proper position and height for folder config
                    int serviceControlBottom = grpServiceControl.Bottom;
                    int spacing = Math.Max(6, grpFolderConfig.Top - serviceControlBottom); // Maintain at least 6px spacing
                    
                    grpFolderConfig.Top = serviceControlBottom + spacing;
                    grpFolderConfig.Height = availableHeight - grpFolderConfig.Top - 10; // 10px bottom margin
                    
                    System.Diagnostics.Debug.WriteLine($"AdjustFormLayout: Adjusted grpFolderConfig - Top={grpFolderConfig.Top}, Height={grpFolderConfig.Height}");
                    System.Diagnostics.Debug.WriteLine($"AdjustFormLayout: Available height={availableHeight}, Service control height={grpServiceControl.Height}");
                }
                
                // ELEGANT FIX: Force complete layout recalculation
                serviceControlTab.PerformLayout();
            }
            
            // Force TabControl to recalculate layout
            tabControl.PerformLayout();
            
            System.Diagnostics.Debug.WriteLine("AdjustFormLayout: Layout adjustment complete");
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
                if (IsServiceInstalled(serviceName))
                {
                    serviceController.Refresh();
                    lock (statusLock)
                    {
                        previousServiceStatus = serviceControllerStatus;
                        serviceControllerStatus = serviceController.Status;
                    }
                }
                else
                {
                    // Service doesn't exist, set default status
                    lock (statusLock)
                    {
                        serviceControllerStatus = ServiceControllerStatus.Stopped;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing service status: {ex.Message}");
                // Set default status on error
                lock (statusLock)
                {
                    serviceControllerStatus = ServiceControllerStatus.Stopped;
                }
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
        /// Check if a Windows service is installed on the system.
        /// </summary>
        /// <param name="serviceName">The name of the service to check</param>
        /// <returns>True if service is installed, false otherwise</returns>
        public static bool IsServiceInstalled(string serviceName)
        {
            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    // Check if the service exists by accessing its status
                    ServiceControllerStatus status = sc.Status;
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
                // Service does not exist
                return false;
            }
            catch (ArgumentException)
            {
                // Invalid service name or computer name
                return false;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // Windows API error (service not found, access denied, etc.)
                return false;
            }
            catch (Exception ex)
            {
                // Any other exception means we can't access the service
                System.Diagnostics.Debug.WriteLine($"Unexpected error checking service existence: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if a Windows service is stopped.
        /// </summary>
        /// <param name="serviceName">The name of the service to check</param>
        /// <returns>True if service is stopped, false if running or not installed</returns>
        public static bool IsServiceStopped(string serviceName)
        {
            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    // Check if the service exists by accessing its status
                    ServiceControllerStatus status = sc.Status;
                    return status == ServiceControllerStatus.Stopped;
                }
            }
            catch (InvalidOperationException)
            {
                // Service does not exist
                return false;
            }
            catch (ArgumentException)
            {
                // Invalid service name or computer name
                return false;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // Windows API error (service not found, access denied, etc.)
                return false;
            }
            catch (Exception ex)
            {
                // Any other exception means we can't access the service
                System.Diagnostics.Debug.WriteLine($"Unexpected error checking service status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if a Windows service is running.
        /// </summary>
        /// <param name="serviceName">The name of the service to check</param>
        /// <returns>True if service is running, false if stopped or not installed</returns>
        public static bool IsServiceRunning(string serviceName)
        {
            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    // Check if the service exists by accessing its status
                    ServiceControllerStatus status = sc.Status;
                    return status == ServiceControllerStatus.Running;
                }
            }
            catch (InvalidOperationException)
            {
                // Service does not exist
                return false;
            }
            catch (ArgumentException)
            {
                // Invalid service name or computer name
                return false;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // Windows API error (service not found, access denied, etc.)
                return false;
            }
            catch (Exception ex)
            {
                // Any other exception means we can't access the service
                System.Diagnostics.Debug.WriteLine($"Unexpected error checking service status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get the current status of a Windows service.
        /// </summary>
        /// <param name="serviceName">The name of the service to check</param>
        /// <returns>Service status if accessible, null if service doesn't exist or can't be accessed</returns>
        public static ServiceControllerStatus? GetServiceStatus(string serviceName)
        {
            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    return sc.Status;
                }
            }
            catch (InvalidOperationException)
            {
                // Service does not exist
                return null;
            }
            catch (ArgumentException)
            {
                // Invalid service name or computer name
                return null;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // Windows API error (service not found, access denied, etc.)
                return null;
            }
            catch (Exception ex)
            {
                // Any other exception means we can't access the service
                System.Diagnostics.Debug.WriteLine($"Unexpected error getting service status: {ex.Message}");
                return null;
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
                // Only check permissions if service exists
                if (!IsServiceInstalled(serviceName))
                {
                    return IsUserAdministrator(); // Can install if admin
                }

                // Try to access the service to check permissions
                ServiceControllerStatus status = serviceController.Status;

                // Try to check if we can start/stop (this will throw if no permission)
                // We're not actually starting/stopping, just checking access
                ServiceControllerPermissionAccess access = ServiceControllerPermissionAccess.Control;
                ServiceControllerPermission permission = new ServiceControllerPermission(access, Environment.MachineName, serviceName);
                permission.Demand();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Permission check failed: {ex.Message}");
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
                if (IsServiceInstalled(serviceName))
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
                if (IsServiceInstalled(serviceName))
                {
                    try
                    {
                        if (serviceController.Status == ServiceControllerStatus.Running)
                        {
                            serviceController.Stop();
                            serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error stopping service before uninstall: {ex.Message}");
                        // Continue with uninstall even if stop fails
                    }
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

                if (!IsServiceInstalled(serviceName))
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
                serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(120));
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
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(120));
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

            if (!IsServiceInstalled(serviceName))
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
        /// Open the Alert Configuration dialog.
        /// This feature is only available in PREMIUM builds.
        /// </summary>
        private void btnAlertConfig_Click(object sender, EventArgs e)
        {
#if PREMIUM
            try
            {
                using (var alertConfigForm = new AlertConfigForm())
                {
                    var result = alertConfigForm.ShowDialog(this);
        
                    if (result == DialogResult.OK)
                    {
                        MessageBox.Show(
                            "Alert configuration saved successfully.\n\n" +
                            "Note: If the service is currently running, you may need to restart it for the new settings to take effect.",
                            "Configuration Saved",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening alert configuration: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#else
            MessageBox.Show(
                "Alert Settings is a PREMIUM feature.\n\n" +
                "Upgrade to the PREMIUM version to access threat detection and alert configuration.",
                "PREMIUM Feature",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
#endif
        }

        /// <summary>
        /// Add new folder to watch list with enhanced conflict detection
        /// </summary>
        private void btnAddFolder_Click(object sender, EventArgs e)
        {
            try
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Select folder to watch for file system changes";
                    dialog.ShowNewFolderButton = false;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string selectedPath = dialog.SelectedPath;

                        // Debug: Log the selected path and current watched folders
                        System.Diagnostics.Debug.WriteLine($"Adding folder: '{selectedPath}'");
                        System.Diagnostics.Debug.WriteLine($"Current watched folders: {string.Join(", ", watchedFolders)}");

                        // Validate the folder
                        if (!folderManager.ValidateFolder(selectedPath))
                        {
                            MessageBox.Show("The selected folder does not exist or is not accessible.",
                                "Invalid Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Check if folder is already being watched
                        if (watchedFolders.Contains(selectedPath, StringComparer.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("This folder is already being watched.",
                                "Duplicate Folder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        // Enhanced check for parent-child relationships
                        var conflictResult = CheckForFolderConflicts(selectedPath);
                        System.Diagnostics.Debug.WriteLine($"Conflict check result: CanAdd={conflictResult.CanAdd}, FoldersToRemove={conflictResult.FoldersToRemove.Count}");
                        
                        if (!conflictResult.CanAdd)
                        {
                            return; // User chose not to proceed
                        }

                        // Remove any folders that the user chose to replace
                        foreach (string folderToRemove in conflictResult.FoldersToRemove)
                        {
                            System.Diagnostics.Debug.WriteLine($"Removing conflicting folder: '{folderToRemove}'");
                            watchedFolders.Remove(folderToRemove);
                        }

                        // Add the folder
                        watchedFolders.Add(selectedPath);
                        SaveFoldersConfiguration();
                        UpdateFoldersListBox();

                        string message = $"Folder '{selectedPath}' has been added to the watch list.";
                        if (conflictResult.FoldersToRemove.Count > 0)
                        {
                            message += $"\n\nRemoved {conflictResult.FoldersToRemove.Count} overlapping folder(s).";
                        }
                        message += "\n\nNote: You may need to restart the service for changes to take effect.";

                        MessageBox.Show(message, "Folder Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding folder: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Remove selected folder from watch list
        /// </summary>
        private void btnRemoveFolder_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBoxFolders.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select a folder to remove.", "No Selection",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int selectedIndex = listBoxFolders.SelectedIndex;
                string selectedFolder = watchedFolders[selectedIndex]; // Use actual path, not display text

                DialogResult result = MessageBox.Show(
                    $"Are you sure you want to remove '{selectedFolder}' from the watch list?",
                    "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    watchedFolders.RemoveAt(selectedIndex);
                    SaveFoldersConfiguration();
                    UpdateFoldersListBox();

                    MessageBox.Show($"Folder '{selectedFolder}' has been removed from the watch list.\n\n" +
                        "Note: You may need to restart the service for changes to take effect.",
                        "Folder Removed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing folder: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Edit selected folder path
        /// </summary>
        private void btnEditFolder_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBoxFolders.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select a folder to edit.", "No Selection",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int selectedIndex = listBoxFolders.SelectedIndex;
                string currentFolder = watchedFolders[selectedIndex]; // Use actual path, not display text

                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Select new folder path";
                    dialog.SelectedPath = currentFolder;
                    dialog.ShowNewFolderButton = false;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string newPath = dialog.SelectedPath;

                        // Don't validate if path hasn't changed
                        if (string.Equals(currentFolder, newPath, StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("The folder path is unchanged.", "No Changes",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        // Validate the folder
                        if (!folderManager.ValidateFolder(newPath))
                        {
                            MessageBox.Show("The selected folder does not exist or is not accessible.",
                                "Invalid Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Check if folder is already being watched (excluding current item)
                        if (watchedFolders.Where((f, i) => i != selectedIndex)
                            .Contains(newPath, StringComparer.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("This folder is already being watched.",
                                "Duplicate Folder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        // Temporarily set new path for conflict checking
                        string originalFolder = watchedFolders[selectedIndex];
                        watchedFolders[selectedIndex] = newPath;

                        // Check for conflicts with the new path
                        var conflictResult = CheckForEditConflicts(newPath, selectedIndex);
                        
                        if (!conflictResult.CanEdit)
                        {
                            // Restore original folder if user cancelled
                            watchedFolders[selectedIndex] = originalFolder;
                            return;
                        }

                        // Remove any folders that conflict
                        foreach (string folderToRemove in conflictResult.FoldersToRemove)
                        {
                            int indexToRemove = watchedFolders.IndexOf(folderToRemove);
                            if (indexToRemove >= 0 && indexToRemove != selectedIndex)
                            {
                                watchedFolders.RemoveAt(indexToRemove);
                                // Adjust selected index if necessary
                                if (indexToRemove < selectedIndex)
                                {
                                    selectedIndex--;
                                }
                            }
                        }

                        // Save configuration and update UI
                        SaveFoldersConfiguration();
                        UpdateFoldersListBox();

                        // Restore selection
                        if (selectedIndex < listBoxFolders.Items.Count)
                        {
                            listBoxFolders.SelectedIndex = selectedIndex;
                        }

                        string message = $"Folder path has been updated to '{newPath}'.";
                        if (conflictResult.FoldersToRemove.Count > 0)
                        {
                            message += $"\n\nRemoved {conflictResult.FoldersToRemove.Count} overlapping folder(s).";
                        }
                        message += "\n\nNote: You may need to restart the service for changes to take effect.";

                        MessageBox.Show(message, "Folder Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing folder: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handle folder selection change
        /// </summary>
        private void listBoxFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool hasSelection = listBoxFolders.SelectedIndex >= 0;
            btnRemoveFolder.Enabled = hasSelection;
            btnEditFolder.Enabled = hasSelection;
        }

        /// <summary>
        /// Check for folder conflicts and handle user decisions
        /// </summary>
        /// <param name="newFolderPath">The new folder path to check</param>
        /// <returns>Result indicating if the folder can be added and which folders to remove</returns>
        private FolderConflictResult CheckForFolderConflicts(string newFolderPath)
        {
            var result = new FolderConflictResult { CanAdd = true, FoldersToRemove = new List<string>() };
            
            var parentFolders = new List<string>();
            var childFolders = new List<string>();

            System.Diagnostics.Debug.WriteLine($"Checking conflicts for: '{newFolderPath}'");

            // Categorize existing folders in relation to the new folder
            foreach (string existingFolder in watchedFolders)
            {
                System.Diagnostics.Debug.WriteLine($"  Checking against existing: '{existingFolder}'");
                
                // Check if newFolderPath is a child of existingFolder
                bool isNewFolderChild = FolderManager.IsSubfolder(existingFolder, newFolderPath);
                System.Diagnostics.Debug.WriteLine($"    Is '{newFolderPath}' child of '{existingFolder}': {isNewFolderChild}");
                
                // Check if existingFolder is a child of newFolderPath  
                bool isExistingChild = FolderManager.IsSubfolder(newFolderPath, existingFolder);
                System.Diagnostics.Debug.WriteLine($"    Is '{existingFolder}' child of '{newFolderPath}': {isExistingChild}");

                if (isNewFolderChild)
                {
                    // New folder is a child of existing folder
                    parentFolders.Add(existingFolder);
                    System.Diagnostics.Debug.WriteLine($"    Added '{existingFolder}' as parent folder");
                }
                else if (isExistingChild)
                {
                    // New folder is a parent of existing folder
                    childFolders.Add(existingFolder);
                    System.Diagnostics.Debug.WriteLine($"    Added '{existingFolder}' as child folder");
                }
            }

            System.Diagnostics.Debug.WriteLine($"Found {parentFolders.Count} parent folders, {childFolders.Count} child folders");

            // Handle case where new folder has parent folders being watched
            if (parentFolders.Count > 0)
            {
                string parentList = string.Join("\n• ", parentFolders);
                string message;
                
                if (IsDriveRootScenario(parentFolders, newFolderPath))
                {
                    message = $"WARNING: Drive-level monitoring detected!\n\n" +
                             $"The selected folder:\n'{newFolderPath}'\n\n" +
                             $"is located on a drive that is already being monitored:\n• {parentList}\n\n" +
                             $"This will cause DUPLICATE LOGGING for all files in '{newFolderPath}'.\n\n" +
                             $"Monitoring an entire drive is very resource-intensive and may impact system performance.\n\n" +
                             $"Recommendation: Remove drive monitoring and monitor specific folders instead.\n\n" +
                             $"Do you want to continue anyway?";
                }
                else
                {
                    message = $"The selected folder:\n'{newFolderPath}'\n\n" +
                             $"is a subfolder of these already monitored folders:\n• {parentList}\n\n" +
                             $"This will cause duplicate logging for files in the selected folder.\n\n" +
                             $"Do you want to continue anyway?";
                }

                DialogResult dialogResult = MessageBox.Show(message, "Folder Overlap Warning", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dialogResult == DialogResult.No)
                {
                    result.CanAdd = false;
                    return result;
                }
            }

            // Handle case where new folder would be parent of existing folders
            if (childFolders.Count > 0)
            {
                string childList = string.Join("\n• ", childFolders);
                string message;

                if (IsDriveRootScenario(new List<string> { newFolderPath }, childFolders.ToArray()))
                {
                    message = $"WARNING: You are about to monitor an entire drive!\n\n" +
                             $"The selected folder:\n'{newFolderPath}'\n\n" +
                             $"is a drive root that contains these currently monitored folders:\n• {childList}\n\n" +
                             $"Drive-level monitoring is very resource-intensive and may impact system performance.\n\n" +
                             $"Options:\n" +
                             $"• YES: Monitor entire drive (will remove specific folder monitoring)\n" +
                             $"• NO: Keep monitoring specific folders only\n\n" +
                             $"Do you want to monitor the entire drive?";
                }
                else
                {
                    message = $"The selected folder:\n'{newFolderPath}'\n\n" +
                             $"is a parent folder of these currently monitored folders:\n• {childList}\n\n" +
                             $"Do you want to:\n" +
                             $"• YES: Remove the specific folders and monitor the parent folder\n" +
                             $"• NO: Keep the current specific folder monitoring";
                }

                DialogResult dialogResult = MessageBox.Show(message, "Parent Folder Selection", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    result.FoldersToRemove.AddRange(childFolders);
                }
                else
                {
                    result.CanAdd = false;
                    return result;
                }
            }

            return result;
        }

        /// <summary>
        /// Check for conflicts when editing a folder path
        /// </summary>
        /// <param name="newFolderPath">The new folder path</param>
        /// <param name="excludeIndex">Index to exclude from conflict checking</param>
        /// <returns>Result indicating if the edit can proceed</returns>
        private FolderConflictResult CheckForEditConflicts(string newFolderPath, int excludeIndex)
        {
            var result = new FolderConflictResult { CanAdd = true, FoldersToRemove = new List<string>() };
            
            var parentFolders = new List<string>();
            var childFolders = new List<string>();

            // Check against all other folders (excluding the one being edited)
            for (int i = 0; i < watchedFolders.Count; i++)
            {
                if (i == excludeIndex) continue;

                string existingFolder = watchedFolders[i];
                
                if (FolderManager.IsSubfolder(existingFolder, newFolderPath))
                {
                    parentFolders.Add(existingFolder);
                }
                else if (FolderManager.IsSubfolder(newFolderPath, existingFolder))
                {
                    childFolders.Add(existingFolder);
                }
            }

            // Handle parent folder conflicts
            if (parentFolders.Count > 0)
            {
                string parentList = string.Join("\n• ", parentFolders);
                string message;
                
                if (IsDriveRootScenario(parentFolders, newFolderPath))
                {
                    message = $"WARNING: Drive-level monitoring detected!\n\n" +
                             $"The new folder path:\n'{newFolderPath}'\n\n" +
                             $"is located on a drive that is already being monitored:\n• {parentList}\n\n" +
                             $"This will cause DUPLICATE LOGGING for all files in '{newFolderPath}'.\n\n" +
                             $"Do you want to continue with this change?";
                }
                else
                {
                    message = $"The new folder path:\n'{newFolderPath}'\n\n" +
                             $"is a subfolder of these monitored folders:\n• {parentList}\n\n" +
                             $"This will cause duplicate logging. Do you want to continue?";
                }

                if (MessageBox.Show(message, "Folder Overlap Warning", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    result.CanAdd = false;
                    return result;
                }
            }

            // Handle child folder conflicts
            if (childFolders.Count > 0)
            {
                string childList = string.Join("\n• ", childFolders);
                string message;

                if (IsDriveRootScenario(new List<string> { newFolderPath }, childFolders.ToArray()))
                {
                    message = $"WARNING: You are changing to monitor an entire drive!\n\n" +
                             $"The new folder path:\n'{newFolderPath}'\n\n" +
                             $"contains these currently monitored folders:\n• {childList}\n\n" +
                             $"Do you want to remove the specific folders and monitor the entire drive?";
                }
                else
                {
                    message = $"The new folder path:\n'{newFolderPath}'\n\n" +
                             $"is a parent of these monitored folders:\n• {childList}\n\n" +
                             $"Do you want to remove them and monitor the parent folder instead?";
                }

                if (MessageBox.Show(message, "Parent Folder Selection", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    result.FoldersToRemove.AddRange(childFolders);
                }
                else
                {
                    result.CanAdd = false;
                    return result;
                }
            }

            return result;
        }

        /// <summary>
        /// Determine if this is a drive root scenario (high impact)
        /// </summary>
        private bool IsDriveRootScenario(List<string> parentPaths, params string[] childPaths)
        {
            // Check if any parent is a drive root
            foreach (string parent in parentPaths)
            {
                if (IsDriveRoot(parent))
                    return true;
            }

            // Check if the new path being added is a drive root
            foreach (string child in childPaths)
            {
                if (IsDriveRoot(child))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check if a path is a drive root (e.g., C:\, D:\)
        /// </summary>
        private bool IsDriveRoot(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return false;

                string normalizedPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
                return normalizedPath.Length == 2 && normalizedPath.EndsWith(":");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Save the folders configuration to file
        /// </summary>
        private void SaveFoldersConfiguration()
        {
            try
            {
                folderManager.SaveWatchedFolders(watchedFolders);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // Re-throw to prevent UI from being out of sync
            }
        }

        /// <summary>
        /// Refresh the folders list from configuration
        /// </summary>
        private void RefreshFoldersList()
        {
            try
            {
                watchedFolders = folderManager.GetWatchedFolders();
                UpdateFoldersListBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading watched folders: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Update the folders list box display
        /// </summary>
        private void UpdateFoldersListBox()
        {
            listBoxFolders.Items.Clear();
            
            foreach (string folder in watchedFolders)
            {
                string displayText = folder;
                
                // Add warning indicator for drive roots
                if (IsDriveRoot(folder))
                {
                    displayText += " ⚠️ [DRIVE ROOT - HIGH RESOURCE USAGE]";
                }
                
                listBoxFolders.Items.Add(displayText);
            }
            
            // Update button states
            bool hasSelection = listBoxFolders.SelectedIndex >= 0;
            btnRemoveFolder.Enabled = hasSelection;
            btnEditFolder.Enabled = hasSelection;
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
                    if (IsServiceInstalled(serviceName))
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
                        System.Diagnostics.Debug.WriteLine("Service doesn't exist, stopping monitoring");
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
            // Ensure the log viewer loads data when the tab is first accessed
            if (logViewerControl != null)
            {
                // Force a refresh of the log data to ensure existing entries are displayed
                logViewerControl.RefreshData();
            }
        }

        /// <summary>
        /// Result of folder conflict checking
        /// </summary>
        private class FolderConflictResult
        {
            public bool CanAdd { get; set; }
            public List<string> FoldersToRemove { get; set; } = new List<string>();
            
            // Alias property for edit operations
            public bool CanEdit 
            { 
                get { return CanAdd; } 
                set { CanAdd = value; } 
            }
        }

#if PREMIUM
        /// <summary>
        /// Update the form title with license information.
        /// </summary>
        private void UpdateFormTitle()
        {
            if (_licenseService == null)
            {
                this.Text = "Folder Watcher Service Admin - PREMIUM";
                return;
            }

            try
            {
                var licenseInfo = _licenseService.GetLicenseInfo();
                var hasLicenseProperty = licenseInfo.GetType().GetProperty("HasLicense");
                
                if (hasLicenseProperty != null)
                {
                    bool hasLicense = (bool)hasLicenseProperty.GetValue(licenseInfo);
                    
                    if (hasLicense)
                    {
                        this.Text = "Folder Watcher Service Admin - PREMIUM (Licensed)";
                    }
                    else
                    {
                        int remainingDays = _licenseService.GetRemainingTrialDays();
                        this.Text = $"Folder Watcher Service Admin - PREMIUM (Trial: {remainingDays} days)";
                    }
                }
                else
                {
                    this.Text = "Folder Watcher Service Admin - PREMIUM";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateFormTitle ERROR: {ex.Message}");
                this.Text = "Folder Watcher Service Admin - PREMIUM";
            }
        }

        /// <summary>
        /// Show trial reminder dialog on application startup.
        /// </summary>
        private void ShowTrialReminderOnStartup()
        {
            if (_licenseService == null)
                return;

            try
            {
                var licenseInfo = _licenseService.GetLicenseInfo();
                var hasLicenseProperty = licenseInfo.GetType().GetProperty("HasLicense");
                
                if (hasLicenseProperty != null)
                {
                    bool hasLicense = (bool)hasLicenseProperty.GetValue(licenseInfo);
                    
                    if (!hasLicense)
                    {
                        int remainingDays = _licenseService.GetRemainingTrialDays();
                        
                        if (remainingDays <= 0)
                        {
                            MessageBox.Show(
                                "Your trial period has expired.\n\n" +
                                "Please purchase a license to continue using PREMIUM features.\n\n" +
                                "Click 'Buy License' to visit our website.",
                                "Trial Expired",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                        else if (remainingDays <= 7)
                        {
                            MessageBox.Show(
                                $"Your trial period expires in {remainingDays} day(s).\n\n" +
                                "Please consider purchasing a license to continue using PREMIUM features.",
                                "Trial Reminder",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowTrialReminderOnStartup ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle Buy License link click.
        /// </summary>
        private void LnkBuyLicense_Click(object sender, EventArgs e)
        {
            try
            {
                // Open purchase URL in default browser
                string purchaseUrl = "https://yourcompany.com/purchase"; // Replace with actual URL
                System.Diagnostics.Process.Start(purchaseUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to open purchase page: {ex.Message}\n\n" +
                    "Please visit our website manually to purchase a license.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Handle Enter License button click.
        /// </summary>
        private void BtnEnterLicense_Click(object sender, EventArgs e)
        {
            try
            {
                using (var licenseKeyForm = new Form())
                {
                    licenseKeyForm.Text = "Enter License Key";
                    licenseKeyForm.Size = new System.Drawing.Size(500, 200);
                    licenseKeyForm.StartPosition = FormStartPosition.CenterParent;
                    licenseKeyForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    licenseKeyForm.MaximizeBox = false;
                    licenseKeyForm.MinimizeBox = false;

                    var lblPrompt = new Label
                    {
                        Text = "Please enter your license key:",
                        Location = new System.Drawing.Point(20, 20),
                        AutoSize = true
                    };

                    var txtLicenseKey = new TextBox
                    {
                        Location = new System.Drawing.Point(20, 50),
                        Width = 440,
                        Font = new System.Drawing.Font("Courier New", 10F)
                    };

                    var btnOk = new Button
                    {
                        Text = "Activate",
                        Location = new System.Drawing.Point(280, 100),
                        DialogResult = DialogResult.OK
                    };

                    var btnCancel = new Button
                    {
                        Text = "Cancel",
                        Location = new System.Drawing.Point(370, 100),
                        DialogResult = DialogResult.Cancel
                    };

                    licenseKeyForm.Controls.AddRange(new Control[] { lblPrompt, txtLicenseKey, btnOk, btnCancel });
                    licenseKeyForm.AcceptButton = btnOk;
                    licenseKeyForm.CancelButton = btnCancel;

                    if (licenseKeyForm.ShowDialog(this) == DialogResult.OK)
                    {
                        string licenseKey = txtLicenseKey.Text.Trim();
                        
                        if (string.IsNullOrWhiteSpace(licenseKey))
                        {
                            MessageBox.Show("Please enter a valid license key.", "Invalid Input",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Attempt to activate the license
                        var result = _licenseService.ActivateLicense(licenseKey);
                        
                        // Use reflection to get the Success property
                        var successProperty = result.GetType().GetProperty("Success");
                        var messageProperty = result.GetType().GetProperty("Message");
                        
                        if (successProperty != null && messageProperty != null)
                        {
                            bool success = (bool)successProperty.GetValue(result);
                            string message = messageProperty.GetValue(result)?.ToString() ?? "Unknown result";
                            
                            if (success)
                            {
                                MessageBox.Show(message, "Activation Successful",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                
                                // Update UI to reflect new license status
                                UpdateFormTitle();
                                UpdateLicenseStatus();
                            }
                            else
                            {
                                MessageBox.Show(message, "Activation Failed",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("License activation result format error.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error activating license: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Determine if the license bar should be shown.
        /// In Debug builds: always show for testing.
        /// In Release builds: only show for Trial, No License, Expired, or Expiring Soon.
        /// </summary>
        /// <returns>True if license bar should be visible, false otherwise</returns>
        private bool ShouldShowLicenseBar()
        {
            if (_licenseService == null)
            {
                System.Diagnostics.Debug.WriteLine("ShouldShowLicenseBar: _licenseService is null - showing bar");
                return true; // Show by default if service unavailable
            }
            
            try
            {
                var licenseInfo = _licenseService.GetLicenseInfo();
                
                // Get properties using reflection (same pattern as UpdateLicenseStatus)
                var hasLicenseProperty = licenseInfo.GetType().GetProperty("HasLicense");
                var typeProperty = licenseInfo.GetType().GetProperty("Type");
                var isValidProperty = licenseInfo.GetType().GetProperty("IsValid");
                var needsRenewalProperty = licenseInfo.GetType().GetProperty("NeedsRenewal");
                
                if (hasLicenseProperty == null)
                {
                    System.Diagnostics.Debug.WriteLine("ShouldShowLicenseBar: HasLicense property not found - showing bar");
                    return true;
                }
                
                bool hasLicense = (bool)hasLicenseProperty.GetValue(licenseInfo);
                
                // No license at all - SHOW bar
                if (!hasLicense)
                {
                    System.Diagnostics.Debug.WriteLine("ShouldShowLicenseBar: No license found - SHOW bar");
                    return true;
                }
                
                // Check if license is valid
                if (isValidProperty != null)
                {
                    bool isValid = (bool)isValidProperty.GetValue(licenseInfo);
                    if (!isValid)
                    {
                        System.Diagnostics.Debug.WriteLine("ShouldShowLicenseBar: License is invalid - SHOW bar");
                        return true; // Show bar for invalid licenses
                    }
                }
                
                // Check license type
                if (typeProperty != null)
                {
                    string licenseType = typeProperty.GetValue(licenseInfo)?.ToString() ?? "";
                    System.Diagnostics.Debug.WriteLine($"ShouldShowLicenseBar: License type = '{licenseType}'");
                    
                    // TRIAL license - ALWAYS SHOW bar
                    if (licenseType.Equals("Trial", StringComparison.OrdinalIgnoreCase))
                    {
                        System.Diagnostics.Debug.WriteLine("ShouldShowLicenseBar: Trial license - SHOW bar");
                        return true;
                    }
                    
                    // PAID license
                    if (licenseType.Equals("Paid", StringComparison.OrdinalIgnoreCase))
                    {
                        // Check if expiring soon (within 7 days as defined in LicenseInfo.NeedsRenewal)
                        if (needsRenewalProperty != null)
                        {
                            bool needsRenewal = (bool)needsRenewalProperty.GetValue(licenseInfo);
                            if (needsRenewal)
                            {
                                System.Diagnostics.Debug.WriteLine("ShouldShowLicenseBar: Paid license expiring soon - SHOW bar");
                                return true;
                            }
                        }
                        
                        // Valid paid license, not expiring soon - HIDE bar (in Release only)
                        System.Diagnostics.Debug.WriteLine("ShouldShowLicenseBar: Valid Paid license with no renewal needed - HIDE bar");
                        return false;
                    }
                }
                
                // Unknown state or couldn't determine type - show bar to be safe
                System.Diagnostics.Debug.WriteLine("ShouldShowLicenseBar: Unknown license state - SHOW bar (safe default)");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShouldShowLicenseBar ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"ShouldShowLicenseBar Stack: {ex.StackTrace}");
                return true; // Show bar on error (safe default)
            }
        }
#endif

        /// <summary>
        /// Initialize folder management system and load watched folders.
        /// </summary>
        private void InitializeFolderManagement()
        {
            try
            {
                // Initialize folder manager
                folderManager = new FolderManager();
                
                // Load watched folders from configuration
                watchedFolders = folderManager.GetWatchedFolders();
                
                // Update the UI with loaded folders (only if controls are initialized)
                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    UpdateFoldersListBox();
                }
                
                System.Diagnostics.Debug.WriteLine($"Folder management initialized. Loaded {watchedFolders.Count} watched folders.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing folder management: {ex.Message}");
                MessageBox.Show(
                    $"Error initializing folder management: {ex.Message}\n\n" +
                    "The application will continue, but folder configuration may not be available.",
                    "Initialization Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                // Initialize with empty list as fallback
                watchedFolders = new List<string>();
            }
        }

#if PREMIUM
        /// <summary>
        /// Update the license status label in the license bar.
        /// </summary>
        private void UpdateLicenseStatus()
        {
            if (_licenseService == null)
            {
                System.Diagnostics.Debug.WriteLine("UpdateLicenseStatus: _licenseService is null");
                return;
            }

            try
            {
                // Find the license status label
                var licensePanel = this.Controls.Find("licensePanel", false).FirstOrDefault() as Panel;
                if (licensePanel == null)
                {
                    System.Diagnostics.Debug.WriteLine("UpdateLicenseStatus: License panel not found");
                    return;
                }

                var lblLicenseStatus = licensePanel.Controls.Find("lblLicenseStatus", false).FirstOrDefault() as Label;
                if (lblLicenseStatus == null)
                {
                    System.Diagnostics.Debug.WriteLine("UpdateLicenseStatus: License status label not found");
                    return;
                }

                // Get license information
                var licenseInfo = _licenseService.GetLicenseInfo();
                var hasLicenseProperty = licenseInfo.GetType().GetProperty("HasLicense");
                var typeProperty = licenseInfo.GetType().GetProperty("Type");
                var isValidProperty = licenseInfo.GetType().GetProperty("IsValid");
                
                if (hasLicenseProperty == null)
                {
                    lblLicenseStatus.Text = "License status unavailable";
                    lblLicenseStatus.ForeColor = System.Drawing.Color.Gray;
                    return;
                }

                bool hasLicense = (bool)hasLicenseProperty.GetValue(licenseInfo);
                
                if (!hasLicense)
                {
                    int remainingDays = _licenseService.GetRemainingTrialDays();
                    if (remainingDays <= 0)
                    {
                        lblLicenseStatus.Text = "⚠️ Trial Expired";
                        lblLicenseStatus.ForeColor = System.Drawing.Color.Red;
                    }
                    else
                    {
                        lblLicenseStatus.Text = $"📅 Trial: {remainingDays} day(s) remaining";
                        lblLicenseStatus.ForeColor = remainingDays <= 7 ? System.Drawing.Color.OrangeRed : System.Drawing.Color.DarkBlue;
                    }
                    return;
                }

                // Check if license is valid
                if (isValidProperty != null)
                {
                    bool isValid = (bool)isValidProperty.GetValue(licenseInfo);
                    if (!isValid)
                    {
                        lblLicenseStatus.Text = "❌ Invalid License";
                        lblLicenseStatus.ForeColor = System.Drawing.Color.Red;
                        return;
                    }
                }

                // Check license type
                if (typeProperty != null)
                {
                    string licenseType = typeProperty.GetValue(licenseInfo)?.ToString() ?? "";
                    
                    if (licenseType.Equals("Trial", StringComparison.OrdinalIgnoreCase))
                    {
                        int remainingDays = _licenseService.GetRemainingTrialDays();
                        lblLicenseStatus.Text = $"📅 Trial: {remainingDays} day(s) remaining";
                        lblLicenseStatus.ForeColor = remainingDays <= 7 ? System.Drawing.Color.OrangeRed : System.Drawing.Color.DarkBlue;
                    }
                    else if (licenseType.Equals("Paid", StringComparison.OrdinalIgnoreCase))
                    {
                        // Check if needs renewal
                        var needsRenewalProperty = licenseInfo.GetType().GetProperty("NeedsRenewal");
                        if (needsRenewalProperty != null)
                        {
                            bool needsRenewal = (bool)needsRenewalProperty.GetValue(licenseInfo);
                            if (needsRenewal)
                            {
                                lblLicenseStatus.Text = "⚠️ License expiring soon";
                                lblLicenseStatus.ForeColor = System.Drawing.Color.OrangeRed;
                            }
                            else
                            {
                                lblLicenseStatus.Text = "✅ Licensed";
                                lblLicenseStatus.ForeColor = System.Drawing.Color.Green;
                            }
                        }
                        else
                        {
                            lblLicenseStatus.Text = "✅ Licensed";
                            lblLicenseStatus.ForeColor = System.Drawing.Color.Green;
                        }
                    }
                    else
                    {
                        lblLicenseStatus.Text = $"License Type: {licenseType}";
                        lblLicenseStatus.ForeColor = System.Drawing.Color.DarkBlue;
                    }
                }
                else
                {
                    lblLicenseStatus.Text = "✅ Licensed";
                    lblLicenseStatus.ForeColor = System.Drawing.Color.Green;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateLicenseStatus ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"UpdateLicenseStatus Stack: {ex.StackTrace}");
            }
        }
#endif

        private void FolderWatcherWindowsServiceAdmin_Load(object sender, EventArgs e)
        {
            // Initialize form layout, service monitoring, and folder management
            InitializeFormLayout();
            InitializeServiceMonitoring();
            InitializeFolderManagement();
            RefreshFoldersList();
            UpdateUI();

            // Set the configuration file path
            lblConfigPathValue.Text = folderManager.ConfigFilePath;
#if PREMIUM
            InitializeAITab();
#endif
        }
    }
}