using FolderWatcherWindowsService.Common.Interfaces;
using System;

namespace FolderWatcherWindowsService.Services
{
    /// <summary>
    /// Null Object implementation for free version without licensing.
    /// SOLID: Null Object Pattern
    /// </summary>
    internal class NullLicenseService : ILicenseService
    {
        public bool Initialize() => true;

        public object GetLicenseInfo()
        {
            return new LicenseInfoResult
            {
                Type = "Free",
                IsValid = true,
                Message = "Free version - Premium features not available"
            };
        }

        public bool IsFeatureEnabled(string featureName) => false;

        public object ActivateLicense(string licenseKey)
        {
            return new
            {
                Success = false,
                Message = "Licensing not available in this build. Please download Premium version."
            };
        }

        public bool StartTrial() => false;

        public int GetRemainingTrialDays() => 0;

        public void Cleanup() { }
    }
}
