using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FolderWatcherWindowsService.Common.Interfaces;

#if PREMIUM
using FolderWatcherWindowsService.Licensing.Services;
using log4net;
#endif

namespace FolderWatcherWindowsServiceAdmin
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if PREMIUM
            // Initialize licensing for admin application
            log4net.Config.XmlConfigurator.Configure();
            ILog logger = LogManager.GetLogger("adminlog");
            ILicenseService licenseService = new LicenseService(logger);

            try
            {
                // Check license before showing main form
                if (!licenseService.Initialize())
                {
                    // Show appropriate dialog based on license status
                    var navigationHelper = new LicenseNavigationHelper((LicenseService)licenseService);
                    var navigationAction = navigationHelper.CheckLicenseAndGetAction();

                    switch (navigationAction.NavigationType)
                    {
                        case NavigationType.TrialOrActivation:
                        case NavigationType.ActivationOnly:
                            using (var activationForm = new LicenseActivationForm(licenseService))
                            {
                                if (activationForm.ShowDialog() == DialogResult.OK)
                                {
                                    // License activated, proceed to main form - PASS licenseService
                                    Application.Run(new FolderWatcherWindowsServiceAdmin(licenseService));
                                }
                                else
                                {
                                    MessageBox.Show(
                                        "A valid license is required to use this application.",
                                        "License Required",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                                }
                            }
                            break;

                        case NavigationType.None:
                            // Valid license, proceed normally - PASS licenseService
                            Application.Run(new FolderWatcherWindowsServiceAdmin(licenseService));
                            break;

                        case NavigationType.Renewal:
                            MessageBox.Show(
                                "Your license has expired. Please renew your license to continue using this application.",
                                "License Expired",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                            break;

                        case NavigationType.Warning:
                        default:
                            MessageBox.Show(
                                "A valid license is required to use this application.\n\n" +
                                "Trial period has been used and no valid license is activated.",
                                "License Required",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            break;
                    }
                }
                else
                {
                    // Valid license, proceed normally - PASS licenseService
                    Application.Run(new FolderWatcherWindowsServiceAdmin(licenseService));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error initializing license system: {ex.Message}\n\n" +
                    "The application will now close.",
                    "Initialization Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                logger?.Error("License initialization failed", ex);
            }
            finally
            {
                licenseService?.Cleanup();
            }
#else
            // FREE version - no licensing required
            Application.Run(new FolderWatcherWindowsServiceAdmin());
#endif
        }
    }
}
