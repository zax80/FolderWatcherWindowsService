using log4net;
using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using FolderWatcherWindowsService.Common.Interfaces;
using FolderWatcherWindowsService.Common.Logging;

#if PREMIUM
using FolderWatcherWindowsService.Licensing.Services;
#endif

namespace FolderWatcherWindowsService
{
    /// <summary>
    /// Windows Service that monitors specified folders for file system changes and logs them in CSV format.
    /// </summary>
    public partial class FolderWatcherWindowsService : ServiceBase
    {
        private readonly ILog _logger;
        private readonly WatcherManager _watcherManager;
        private readonly CsvLogger _csvLogger;
#if PREMIUM
        private readonly ILicenseService _licenseService;
#endif

        public FolderWatcherWindowsService()
        {
            InitializeComponent();

            // Force log4net initialization
            Log4NetAutoInit.EnsureInitialized();
            _logger = LogManager.GetLogger("servicelog");

#if PREMIUM
            _licenseService = new LicenseService(_logger);
#endif
            
            _csvLogger = new CsvLogger(_logger);
            _watcherManager = new WatcherManager(_logger, _csvLogger);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                LogServiceEvent("Service starting.", EventLogEntryType.Information);
                _logger?.Info("Service starting...");

#if PREMIUM
                if (!_licenseService.Initialize())
                {
                    _logger?.Error("Invalid license");
                    throw new InvalidOperationException("Invalid license");
                }
                _logger?.Info($"License: {_licenseService.GetLicenseInfo()}");
#endif

                ConfigurationManager.RefreshSection("appSettings");
                _csvLogger.WriteHeader();
                _watcherManager.StartWatching(Properties.Settings.Default.FolderPaths);
                _logger?.Info("Service started");
            }
            catch (Exception ex)
            {
                LogServiceError("Start failed", ex);
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                _logger?.Info("Service stopping...");
                _watcherManager.StopWatching();
#if PREMIUM
                _licenseService?.Cleanup();
#endif
                _logger?.Info("Service stopped");
            }
            catch (Exception ex)
            {
                LogServiceError("Stop error", ex);
            }
        }

        private void LogServiceEvent(string message, EventLogEntryType type)
        {
            EventLog.WriteEntry(message, type);
        }

        private void LogServiceError(string message, Exception ex)
        {
            EventLog.WriteEntry($"{message}: {ex.Message}", EventLogEntryType.Error);
            _logger?.Error(message, ex);
        }
    }
}