using FolderWatcherWindowsService.Common.Interfaces;
using FolderWatcherWindowsService.Common.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Timers;
using log4net;

#if PREMIUM
using FolderWatcherWindowsService.ThreatDetection.Services;
using FolderWatcherWindowsService.Licensing.Services;
#endif

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
        private const int CleanupIntervalMs = 300000;
        
        // Files to exclude from monitoring (service's own files)
        private readonly HashSet<string> _excludedFiles;

        public WatcherManager(ILog logger, CsvLogger csvLogger)
        {
            _logger = logger;
            _csvLogger = csvLogger;
            _watchers = new List<FileSystemWatcher>();
            _debouncer = new EventDebouncer(DebounceDelayMs);
            _fileInfoProvider = new FileInfoProvider();

            // Initialize exclusion list with service's own files
            _excludedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ServiceLog.txt",      // Changed from "Service.log"
                "ServiceLog.txt.1",    // Changed from "Service.log.1"
                "ServiceLog.txt.2",
                "ServiceLog.txt.3",
                "ServiceLog.txt.4",
                "ServiceLog.txt.5",
                "ServiceLog.txt.6",
                "ServiceLog.txt.7",
                "ServiceLog.txt.8",
                "ServiceLog.txt.9",
                "ServiceLog.txt.10"
            };

#if PREMIUM
            _licenseService = new LicenseService(logger);
            _licenseService.Initialize();

            var licenseInfo = _licenseService.GetLicenseInfo();
            _logger?.Info($"License: {licenseInfo}");

            if (_licenseService.IsFeatureEnabled("ThreatDetection"))
            {
                _threatDetectionService = new ThreatDetectionService(logger) as IThreatDetectionService;
                _threatDetectionService.Initialize();
                _logger?.Info("Threat Detection enabled");
            }
            else
            {
                _threatDetectionService = new Services.NullThreatDetectionService();
            }
#else
            _licenseService = new Services.NullLicenseService();
            _threatDetectionService = new Services.NullThreatDetectionService();
            _logger?.Info("FREE version");
#endif

            _cleanupTimer = new Timer(CleanupIntervalMs);
            _cleanupTimer.Elapsed += OnCleanupTimerElapsed;
            _cleanupTimer.AutoReset = true;
            _cleanupTimer.Start();
        }

        public void StartWatching(StringCollection folderPaths)
        {
            if (folderPaths == null) return;

            foreach (string path in folderPaths)
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    try
                    {
                        _logger?.Info($"Watching: {path}");

                        var watcher = new FileSystemWatcher(path)
                        {
                            IncludeSubdirectories = true,
                            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName |
                                           NotifyFilters.LastWrite | NotifyFilters.Size,
                            InternalBufferSize = 65536
                        };

                        watcher.Created += OnFileSystemEvent;
                        watcher.Changed += OnFileSystemEvent;
                        watcher.Deleted += OnFileSystemEvent;
                        watcher.Renamed += OnFileRenamed;
                        watcher.Error += OnWatcherError;

                        watcher.EnableRaisingEvents = true;
                        _watchers.Add(watcher);
                    }
                    catch (Exception ex)
                    {
                        _logger?.Error($"Failed: {path}", ex);
                    }
                }
            }
            
            _logger?.Info($"{_watchers.Count} watchers active");
        }

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
                _logger?.Debug("Cleanup");
            }
            catch (Exception ex)
            {
                _logger?.Error("Cleanup error", ex);
            }
        }

        /// <summary>
        /// Checks if a file should be excluded from monitoring.
        /// </summary>
        private bool ShouldExcludeFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return true;

            try
            {
                string fileName = Path.GetFileName(filePath);
                
                // Exclude service's own log files (silently - no logging to avoid loops)
                if (_excludedFiles.Contains(fileName))
                {
                    return true;
                }

                // Exclude files in service's own directory (silently)
                string serviceDir = AppDomain.CurrentDomain.BaseDirectory;
                string fileDir = Path.GetDirectoryName(filePath);
                
                if (fileDir != null && 
                    fileDir.Equals(serviceDir, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Filter out service's own files
                if (ShouldExcludeFile(e.FullPath))
                {
                    return; // Silently ignore
                }

                _logger?.Info($"EVENT: {e.ChangeType} | {e.FullPath}");

                _debouncer.Debounce(e, (debouncedEvent) =>
                {
                    // Double-check exclusion after debounce
                    if (ShouldExcludeFile(debouncedEvent.FullPath))
                    {
                        return;
                    }

                    var entry = CreateLogEntry(debouncedEvent);

#if PREMIUM
                    if (_threatDetectionService != null)
                    {
                        System.Threading.Tasks.Task.Run(async () =>
                        {
                            try
                            {
                                await _threatDetectionService.AnalyzeAsync(entry);
                            }
                            catch (Exception ex)
                            {
                                _logger?.Error($"Threat analysis error", ex);
                            }
                        });
                    }
#endif

                    _csvLogger.LogEntry(entry);
                });
            }
            catch (Exception ex)
            {
                _logger?.Error($"Event error: {e.FullPath}", ex);
            }
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            try
            {
                // Filter out service's own files
                if (ShouldExcludeFile(e.FullPath) || ShouldExcludeFile(e.OldFullPath))
                {
                    return;
                }

                _logger?.Info($"RENAME: {e.OldFullPath} -> {e.FullPath}");
                var entry = CreateLogEntry(e);
                entry.ChangeType = $"Renamed from {e.OldName}";
                _csvLogger.LogEntry(entry);
            }
            catch (Exception ex)
            {
                _logger?.Error("Rename error", ex);
            }
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            _logger?.Error("Watcher error", e.GetException());
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
                ServiceUser = userInfo?.ServiceUser ?? Environment.UserName,
                ModifiedBy = userInfo?.ModifiedBy ?? "Unknown",
                FileType = fileInfo?.FileType ?? "Unknown",
                FileSize = fileInfo?.FileSize ?? "0",
                Extension = fileInfo?.Extension ?? Path.GetExtension(e.FullPath),
                LastAccessed = fileInfo?.LastAccessed ?? "",
                LastModified = fileInfo?.LastModified ?? "",
                Error = ""
            };
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
            }
        }
    }
}