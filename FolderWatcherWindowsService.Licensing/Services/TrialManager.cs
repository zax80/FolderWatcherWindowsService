using FolderWatcherWindowsService.Licensing.Models;
using FolderWatcherWindowsService.Licensing.Storage;
using System;

namespace FolderWatcherWindowsService.Licensing.Services
{
    internal class TrialManager
    {
        private const int TRIAL_DAYS = 30;
        private readonly LicenseStorage _storage;

        public TrialManager()
        {
            _storage = new LicenseStorage();
        }

        public bool CanStartTrial()
        {
            var existingLicense = _storage.LoadLicense();
            
            // Trial already used
            if (existingLicense != null && existingLicense.Type == LicenseType.Trial)
                return false;

            return true;
        }

        public LicenseInfo StartTrial()
        {
            var trialLicense = new LicenseInfo
            {
                Type = LicenseType.Trial,
                LicenseKey = "TRIAL-" + Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper(),
                ActivationDate = DateTime.Now,
                ExpirationDate = DateTime.Now.AddDays(TRIAL_DAYS),
                IsValid = true,
                CompanyName = "Trial User"
            };

            _storage.SaveLicense(trialLicense);
            return trialLicense;
        }

        public int GetRemainingDays(LicenseInfo license)
        {
            if (license?.ExpirationDate == null)
                return 0;

            return Math.Max(0, (license.ExpirationDate.Value - DateTime.Now).Days);
        }
    }
}