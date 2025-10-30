using FolderWatcherWindowsService.Common.Interfaces;
using FolderWatcherWindowsService.Common.Models;
#if PREMIUM
using FolderWatcherWindowsService.ThreatDetection.Services;
#endif
using log4net;
using log4net.Appender;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Timers;

namespace FolderWatcherWindowsService
{
    /// <summary>
    /// Manages multiple FileSystemWatcher instances and coordinates file system monitoring.
    /// SOLID: Single Responsibility - only manages file watching, delegates threat detection.
    /// </summary>
    internal class WatcherManager
    {
        private const int DebounceDelayMs = 500;
        private const int CleanupIntervalMinutes = 30;

        private readonly ILog _logger;
        private readonly CsvLogger _csvLogger;
        private readonly List<FileSystemWatcher> _watchers;
        private readonly EventDebouncer _debouncer;
        private readonly FileInfoProvider _fileInfoProvider;
        private string _logFilePath;

        // Threat detection via interface
        private readonly IThreatDetectionService _threatDetectionService;
        private Timer _cleanupTimer;

        public WatcherManager(ILog logger, CsvLogger csvLogger)
        {
            _logger = logger;
            _csvLogger = csvLogger;
            _watchers = new List<FileSystemWatcher>();
            _debouncer = new EventDebouncer(DebounceDelayMs);
            _fileInfoProvider = new FileInfoProvider();

            // Initialize threat detection based on build configuration
#if PREMIUM
            _threatDetectionService = new ThreatDetectionService(logger);
            _threatDetectionService.Initialize();
            _logger?.Info("WatcherManager initialized with PREMIUM threat detection");
#else
            _threatDetectionService = new Services.NullThreatDetectionService();
            _logger?.Info("WatcherManager initialized in FREE mode (no threat detection)");
#endif

            // Setup periodic cleanup
            _cleanupTimer = new Timer(CleanupIntervalMinutes * 60 * 1000);
            _cleanupTimer.Elapsed += OnCleanupTimer;
            _cleanupTimer.Start();
        }

        public void StartWatching(StringCollection folderPaths)
        {
            if (folderPaths == null || folderPaths.Count == 0)
            {
                _logger?.Warn("No folder paths configured in App.config");
                return;
            }

            _logFilePath = GetLogFilePath();

            foreach (string path in folderPaths)
            {
                CreateWatcher(path);
            }

            _logger?.Info($"Total watchers initialized: {_watchers.Count}");
        }

        public void StopWatching()
        {
            _cleanupTimer?.Stop();
            _cleanupTimer?.Dispose();

            foreach (var watcher in _watchers)
            {
                DisposeWatcher(watcher);
            }
            _watchers.Clear();
            _debouncer.Dispose();

            // Cleanup threat detection
            _threatDetectionService?.Cleanup();
        }

        private void CreateWatcher(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                _logger?.Warn("Skipping empty folder path");
                return;
            }

            if (!Directory.Exists(path))
            {
                _logger?.Warn($"Folder path does not exist: {path}");
                return;
            }

            try
            {
                var watcher = new FileSystemWatcher(path)
                {
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName |
                          NotifyFilters.DirectoryName |
                 NotifyFilters.LastWrite |
                 NotifyFilters.Size
                };

                watcher.Created += OnFileSystemEvent;
                watcher.Changed += OnFileSystemEvent;
                watcher.Deleted += OnFileSystemEvent;
                watcher.Renamed += OnFileSystemEvent;
                watcher.Error += OnError;

                _watchers.Add(watcher);
                _logger?.Info($"Now watching folder: {path}");
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error setting up watcher for path '{path}': {ex.Message}", ex);
            }
        }

        private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
        {
            if (IsLogFile(e.FullPath))
            {
                return;
            }

            _debouncer.Debounce(e, ProcessFileEvent);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Exception ex = e.GetException();
            _logger?.Error($"FileSystemWatcher error: {ex?.Message ?? "Unknown error"}", ex);

            if (sender is FileSystemWatcher faultyWatcher)
            {
                RecoverWatcher(faultyWatcher);
            }
        }

        private void ProcessFileEvent(FileSystemEventArgs e)
        {
            try
            {
                var logEntry = CreateLogEntry(e);
                _csvLogger.LogEntry(logEntry);

                // Perform threat analysis via interface
                _ = _threatDetectionService.AnalyzeAsync(logEntry);
            }
            catch (Exception ex)
            {
                var errorEntry = CreateErrorLogEntry(e, ex);
                _csvLogger.LogError(errorEntry);
            }
        }

        /// <summary>
        /// Periodic cleanup of old tracking data.
        /// </summary>
        private void OnCleanupTimer(object sender, ElapsedEventArgs e)
        {
            try
            {
                // ThreatDetectionService handles its own config reload during cleanup
                _threatDetectionService?.Cleanup();

                _logger?.Debug("Periodic cleanup completed");
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error during cleanup: {ex.Message}", ex);
            }
        }

        private FileSystemLogEntry CreateLogEntry(FileSystemEventArgs e)
        {
            var fileInfo = _fileInfoProvider.GetFileInfo(e);
            var userInfo = _fileInfoProvider.GetUserInfo(e.FullPath);

            return new FileSystemLogEntry
            {
                Timestamp = DateTime.Now,
                ChangeType = e.ChangeType.ToString(),
                FilePath = e.FullPath,
                ServiceUser = userInfo.ServiceUser,
                ModifiedBy = userInfo.ModifiedBy,
                FileType = fileInfo.FileType,
                FileSize = fileInfo.FileSize,
                Extension = fileInfo.Extension,
                LastAccessed = fileInfo.LastAccessed,
                LastModified = fileInfo.LastModified,
                Error = string.Empty
            };
        }

        private FileSystemLogEntry CreateErrorLogEntry(FileSystemEventArgs e, Exception ex)
        {
            return new FileSystemLogEntry
            {
                Timestamp = DateTime.Now,
                ChangeType = e.ChangeType.ToString(),
                FilePath = e.FullPath,
                Error = ex.Message
            };
        }

        private bool IsLogFile(string filePath)
        {
            return !string.IsNullOrEmpty(_logFilePath) &&
             string.Equals(filePath, _logFilePath, StringComparison.OrdinalIgnoreCase);
        }

        private void RecoverWatcher(FileSystemWatcher faultyWatcher)
        {
            try
            {
                string path = faultyWatcher.Path;
                _logger?.Info($"Attempting to recreate watcher for: {path}");

                _watchers.Remove(faultyWatcher);
                DisposeWatcher(faultyWatcher);
                CreateWatcher(path);

                _logger?.Info($"Successfully recreated watcher for: {path}");
            }
            catch (Exception ex)
            {
                _logger?.Error($"Failed to recover watcher: {ex.Message}", ex);
            }
        }

        private void DisposeWatcher(FileSystemWatcher watcher)
        {
            try
            {
                watcher.EnableRaisingEvents = false;
                watcher.Created -= OnFileSystemEvent;
                watcher.Changed -= OnFileSystemEvent;
                watcher.Deleted -= OnFileSystemEvent;
                watcher.Renamed -= OnFileSystemEvent;
                watcher.Error -= OnError;
                watcher.Dispose();
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error disposing watcher: {ex.Message}");
            }
        }

        private string GetLogFilePath()
        {
            try
            {
                return LogManager.GetRepository()
                      .GetAppenders()
                 .OfType<FileAppender>()
                 .FirstOrDefault()?.File;
            }
            catch (Exception ex)
            {
                _logger?.Warn($"Could not determine log file path: {ex.Message}");
                return null;
            }
        }
    }
}
