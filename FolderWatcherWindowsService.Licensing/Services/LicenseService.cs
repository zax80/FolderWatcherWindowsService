using FolderWatcherWindowsService.Common.Interfaces;
using FolderWatcherWindowsService.Licensing.Models;
using FolderWatcherWindowsService.Licensing.Storage;
using log4net;
using System;

namespace FolderWatcherWindowsService.Licensing.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly ILog _logger;
        private readonly LicenseStorage _storage;
        private readonly LicenseValidator _validator;
        private readonly TrialManager _trialManager;
        private LicenseInfo _currentLicense;

        public LicenseService(ILog logger)
        {
            _logger = logger;
            _storage = new LicenseStorage();
            _validator = new LicenseValidator();
            _trialManager = new TrialManager();
        }

        public bool Initialize()
        {
            try
            {
                _currentLicense = _storage.LoadLicense();

                if (_currentLicense == null)
                {
                    _logger?.Info("No license found - Free version");
                    _currentLicense = new LicenseInfo
                    {
                        Type = LicenseType.Free,
                        IsValid = true
                    };
                }
                else
                {
                    _validator.Validate(_currentLicense);
                    _logger?.Info($"License loaded: {_currentLicense.Type}, Valid: {_currentLicense.IsValid}");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger?.Error($"License initialization failed: {ex.Message}", ex);
                return false;
            }
        }

        public object GetLicenseInfo()
        {
            return new
            {
                Type = _currentLicense.Type.ToString(),
                _currentLicense.IsValid,
                _currentLicense.CompanyName,
                ExpirationDate = _currentLicense.ExpirationDate?.ToString("yyyy-MM-dd"),
                RemainingDays = _currentLicense.RemainingDays,
                Message = _currentLicense.IsExpired ? "License expired" : "Active"
            };
        }

        public bool IsFeatureEnabled(string featureName)
        {
            if (_currentLicense == null || !_currentLicense.IsValid)
                return false;

            // Free version has no premium features
            if (_currentLicense.Type == LicenseType.Free)
                return false;

            // Check expiration for trial/paid licenses
            if (_currentLicense.IsExpired)
                return false;

            // All premium features available for valid licenses
            return _currentLicense.Type == LicenseType.Trial ||
                   _currentLicense.Type == LicenseType.Professional ||
                   _currentLicense.Type == LicenseType.Enterprise;
        }

        public object ActivateLicense(string licenseKey)
        {
            try
            {
                var result = _validator.ValidateLicenseKey(licenseKey);

                if (result.Success)
                {
                    _currentLicense = result.LicenseInfo;
                    _storage.SaveLicense(_currentLicense);
                    _logger?.Info($"License activated: {_currentLicense.Type}");
                }
                else
                {
                    _logger?.Warn($"License activation failed: {result.Message}");
                }

                return new
                {
                    result.Success,
                    result.Message,
                    LicenseType = result.LicenseInfo?.Type.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger?.Error($"License activation error: {ex.Message}", ex);
                return new
                {
                    Success = false,
                    Message = $"Activation error: {ex.Message}"
                };
            }
        }

        public bool StartTrial()
        {
            try
            {
                if (!_trialManager.CanStartTrial())
                {
                    _logger?.Warn("Trial cannot be started - already used");
                    return false;
                }

                _currentLicense = _trialManager.StartTrial();
                _logger?.Info($"Trial started - {_currentLicense.RemainingDays} days remaining");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.Error($"Failed to start trial: {ex.Message}", ex);
                return false;
            }
        }

        public int GetRemainingTrialDays()
        {
            if (_currentLicense?.Type == LicenseType.Trial)
                return _currentLicense.RemainingDays;

            return 0;
        }

        public void Cleanup()
        {
            // No persistent resources
        }
    }
}