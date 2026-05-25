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
        /// Gets the current license information as a dynamic object.
        /// For LicenseService: Returns anonymous object with properties: Type, IsValid, CompanyName, ExpirationDate, RemainingDays, Message
        /// For NullLicenseService: Returns LicenseInfoResult with properties: HasLicense, CanStartTrial, Type, IsValid, CompanyName, ExpirationDate, RemainingDays, NeedsRenewal, Message
        /// </summary>
        /// <returns>Dynamic object with license details (use reflection or dynamic to access properties)</returns>
        object GetLicenseInfo();

        /// <summary>
        /// Checks if a premium feature is available with current license.
        /// </summary>
        /// <param name="featureName">Name of the feature (e.g., "ThreatDetection")</param>
        bool IsFeatureEnabled(string featureName);

        /// <summary>
        /// Activates a license key.
        /// Returns an anonymous object with properties: Success, Message, LicenseType
        /// </summary>
        /// <param name="licenseKey">The license key to activate</param>
        /// <returns>Anonymous object with activation result</returns>
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
    /// Result returned by NullLicenseService.GetLicenseInfo() with all license details.
    /// Note: LicenseService returns an anonymous object instead.
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
        /// License type (Trial, Professional, Enterprise, Free).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Indicates if the license is valid and not expired.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Company or user name associated with the license.
        /// </summary>
        public string CompanyName { get; set; }

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