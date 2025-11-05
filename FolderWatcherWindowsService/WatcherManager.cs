// Update using statements
using FolderWatcherWindowsService.Common.Interfaces;
using FolderWatcherWindowsService;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using log4net;
using System;
using System.Collections.Specialized;



// Add missing using for IThreatDetectionService and ILicenseService
#if PREMIUM
using FolderWatcherWindowsService.ThreatDetection.Services;
using FolderWatcherWindowsService.Licensing.Services;
#endif

// Ensure the class declaration is present and correct
namespace FolderWatcherWindowsService
{
    public class WatcherManager : IDisposable
    {
        private readonly ILicenseService _licenseService;
        private readonly IThreatDetectionService _threatDetectionService;
        private readonly ILog _logger;
        private readonly CsvLogger _csvLogger;
        private readonly List<FileSystemWatcher> _watchers;
        private readonly EventDebouncer _debouncer;
        private readonly FileInfoProvider _fileInfoProvider;
        private readonly Timer _cleanupTimer;
        private const int DebounceDelayMs = 500;
        private const int CleanupIntervalMs = 300000; // 5 minutes

        public WatcherManager(ILog logger, CsvLogger csvLogger)
        {
            _logger = logger;
            _csvLogger = csvLogger;
            _watchers = new List<FileSystemWatcher>();
            _debouncer = new EventDebouncer(DebounceDelayMs);
            _fileInfoProvider = new FileInfoProvider();

            // Initialize licensing first
#if PREMIUM
            _licenseService = new LicenseService(logger);
            _licenseService.Initialize();

            var licenseInfo = _licenseService.GetLicenseInfo();
            _logger?.Info($"License Status: {licenseInfo}");

            // Initialize threat detection only if licensed
            if (_licenseService.IsFeatureEnabled("ThreatDetection"))
            {
                _threatDetectionService = new ThreatDetectionService(logger) as IThreatDetectionService;
                _threatDetectionService.Initialize();
                _logger?.Info("Threat Detection enabled");
            }
            else
            {
                _threatDetectionService = new Services.NullThreatDetectionService();
                _logger?.Warn("Threat Detection disabled - Premium license required");
            }
#else
            _licenseService = new Services.NullLicenseService();
            _threatDetectionService = new Services.NullThreatDetectionService();
            _logger?.Info("FREE version - Premium features not available");
#endif

            // Setup cleanup timer
            _cleanupTimer = new Timer(CleanupIntervalMs);
            _cleanupTimer.Elapsed += OnCleanupTimerElapsed;
            _cleanupTimer.AutoReset = true;
            _cleanupTimer.Start();
            _logger?.Info($"Cleanup timer started with interval: {CleanupIntervalMs}ms");
        }

        /// <summary>
        /// Starts watching the specified folder paths.
        /// </summary>
        public void StartWatching(StringCollection folderPaths)
        {
            if (folderPaths == null) return;
            foreach (string path in folderPaths)
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    var watcher = new FileSystemWatcher(path)
                    {
                        EnableRaisingEvents = true,
                        IncludeSubdirectories = true
                    };
                    _watchers.Add(watcher);
                    // Optionally, hook up event handlers here if needed
                }
            }
        }

        /// <summary>
        /// Stops all active folder watchers and releases resources.
        /// </summary>
        public void StopWatching()
        {
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            _watchers.Clear();

            _cleanupTimer?.Stop();
            _cleanupTimer?.Dispose();
        }

        private void OnCleanupTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _logger?.Debug("Running periodic cleanup...");
                // The debouncer implements IDisposable and manages its own timer cleanup
                // Additional cleanup operations can be added here if needed
            }
            catch (Exception ex)
            {
                _logger?.Error("Error during periodic cleanup", ex);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cleanupTimer?.Stop();
                _cleanupTimer?.Dispose();

                foreach (var watcher in _watchers)
                {
                    watcher?.Dispose();
                }
                _watchers.Clear();

                _debouncer?.Dispose();

#if PREMIUM
                _threatDetectionService?.Cleanup();
                _licenseService?.Cleanup();
#endif

                _logger?.Info("WatcherManager disposed");
            }
        }
    }
}