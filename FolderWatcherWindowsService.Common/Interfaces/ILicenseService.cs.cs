using System;
using FolderWatcherWindowsService.Common.Interfaces;

namespace FolderWatcherWindowsService.Common.Interfaces
{
    /// <summary>
    /// Interface for license management services.
    /// Allows the service to work with or without licensing module.
    /// SOLID: Dependency Inversion Principle
    /// </summary>
    public interface ILicenseService
    {
        /// <summary>
        /// Initializes the license service and validates existing license.
        /// </summary>
        bool Initialize();

        /// <summary>
        /// Gets the current license information.
        /// </summary>
        LicenseInfoResult GetLicenseInfo();

        /// <summary>
        /// Checks if a premium feature is available with current license.
        /// </summary>
        /// <param name="featureName">Name of the feature (e.g., "ThreatDetection")</param>
        bool IsFeatureEnabled(string featureName);

        /// <summary>
        /// Activates a license key.
        /// </summary>
        /// <param name="licenseKey">The license key to activate</param>
        /// <returns>Activation result with message</returns>
        object ActivateLicense(string licenseKey);

        /// <summary>
        /// Starts the trial period.
        /// </summary>
        /// <returns>True if trial started successfully</returns>
        bool StartTrial();

        /// <summary>
        /// Gets remaining trial days.
        /// </summary>
        int GetRemainingTrialDays();

        /// <summary>
        /// Cleanup resources.
        /// </summary>
        void Cleanup();
    }

    /// <summary>
    /// Result returned by GetLicenseInfo with all license details.
    /// </summary>
    public class LicenseInfoResult
    {
        /// <summary>
        /// Indicates whether a license exists (valid or not).
        /// </summary>
        public bool HasLicense { get; set; }

        /// <summary>
        /// Indicates if trial can be started (only valid when HasLicense is false).
        /// </summary>
        public bool? CanStartTrial { get; set; }

        /// <summary>
        /// License type (Trial, Monthly, Yearly).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Indicates if the license is valid and not expired.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// User name associated with the license.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// User email associated with the license.
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// Subscription period (Monthly, Yearly, Trial).
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// License expiration date in yyyy-MM-dd format.
        /// </summary>
        public string ExpirationDate { get; set; }

        /// <summary>
        /// Number of days remaining before expiration.
        /// </summary>
        public int RemainingDays { get; set; }

        /// <summary>
        /// Indicates if the license needs renewal soon (within 7 days).
        /// </summary>
        public bool NeedsRenewal { get; set; }

        /// <summary>
        /// Human-readable message about the license status.
        /// </summary>
        public string Message { get; set; }
    }
}
