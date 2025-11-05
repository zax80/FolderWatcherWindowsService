using FolderWatcherWindowsService.Licensing.Models;
using System;
using System.Security.Cryptography;
using System.Text;

namespace FolderWatcherWindowsService.Licensing.Services
{
    internal class LicenseValidator
    {
        // Simple validation - replace with your own algorithm
        private const string SECRET_SALT = "FolderWatcher2025SecretSalt";

        public ActivationResult ValidateLicenseKey(string licenseKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(licenseKey))
                {
                    return new ActivationResult
                    {
                        Success = false,
                        Message = "License key cannot be empty"
                    };
                }

                // Parse license key format: TYPE-COMPANY-HASH-EXPIRY
                var parts = licenseKey.Split('-');
                if (parts.Length != 4)
                {
                    return new ActivationResult
                    {
                        Success = false,
                        Message = "Invalid license key format"
                    };
                }

                var licenseType = ParseLicenseType(parts[0]);
                var companyHash = parts[1];
                var validationHash = parts[2];
                var expiryDate = ParseExpiryDate(parts[3]);

                // Validate hash
                var expectedHash = GenerateHash($"{parts[0]}-{parts[1]}-{parts[3]}");
                if (validationHash != expectedHash)
                {
                    return new ActivationResult
                    {
                        Success = false,
                        Message = "Invalid license key"
                    };
                }

                var license = new LicenseInfo
                {
                    Type = licenseType,
                    LicenseKey = licenseKey,
                    CompanyName = DecodeCompanyName(companyHash),
                    ActivationDate = DateTime.Now,
                    ExpirationDate = expiryDate,
                    IsValid = !expiryDate.HasValue || DateTime.Now <= expiryDate.Value
                };

                return new ActivationResult
                {
                    Success = true,
                    Message = "License activated successfully",
                    LicenseInfo = license
                };
            }
            catch (Exception ex)
            {
                return new ActivationResult
                {
                    Success = false,
                    Message = $"Validation error: {ex.Message}"
                };
            }
        }

        public bool Validate(LicenseInfo license)
        {
            if (license == null)
                return false;

            if (license.IsExpired)
            {
                license.IsValid = false;
                license.ErrorMessage = "License has expired";
                return false;
            }

            license.IsValid = true;
            return true;
        }

        private string GenerateHash(string data)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(data + SECRET_SALT);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").Substring(0, 8);
            }
        }

        private LicenseType ParseLicenseType(string code)
        {
            return code switch
            {
                "PRO" => LicenseType.Professional,
                "ENT" => LicenseType.Enterprise,
                _ => LicenseType.Free
            };
        }

        private DateTime? ParseExpiryDate(string dateCode)
        {
            // Format: YYYYMMDD or "PERM" for permanent
            if (dateCode == "PERM")
                return null;

            if (DateTime.TryParseExact(dateCode, "yyyyMMdd", null, 
                System.Globalization.DateTimeStyles.None, out var date))
            {
                return date;
            }

            return DateTime.Now.AddYears(1);
        }

        private string DecodeCompanyName(string hash)
        {
            // Simple decoding - implement proper encoding/decoding
            return "Licensed User";
        }

        // License key generator (use this in your admin tool)
        public static string GenerateLicenseKey(string licenseTypeCode, string company, DateTime? expiry)
        {
            var companyHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(company))
                .Replace("=", "").Replace("+", "").Replace("/", "").Substring(0, 8);
            
            var expiryCode = expiry.HasValue ? expiry.Value.ToString("yyyyMMdd") : "PERM";
            var dataToHash = $"{licenseTypeCode}-{companyHash}-{expiryCode}";
            
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(dataToHash + SECRET_SALT);
                var hash = sha256.ComputeHash(bytes);
                var hashCode = BitConverter.ToString(hash).Replace("-", "").Substring(0, 8);
                
                return $"{licenseTypeCode}-{companyHash}-{hashCode}-{expiryCode}";
            }
        }
    }
}