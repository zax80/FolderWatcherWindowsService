using FolderWatcherWindowsService.Models;
using log4net;
using log4net.Appender;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

/// <summary>
/// Manages multiple FileSystemWatcher instances and coordinates file system monitoring.
/// </summary>
namespace FolderWatcherWindowsService
{
    internal class WatcherManager
    {
        private const int DebounceDelayMs = 500;

        private readonly ILog _logger;
        private readonly CsvLogger _csvLogger;
        private readonly List<FileSystemWatcher> _watchers;
        private readonly EventDebouncer _debouncer;
        private readonly FileInfoProvider _fileInfoProvider;
        private string _logFilePath;

        public WatcherManager(ILog logger, CsvLogger csvLogger)
        {
            _logger = logger;
            _csvLogger = csvLogger;
            _watchers = new List<FileSystemWatcher>();
            _debouncer = new EventDebouncer(DebounceDelayMs);
            _fileInfoProvider = new FileInfoProvider();
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
            foreach (var watcher in _watchers)
            {
                DisposeWatcher(watcher);
            }
            _watchers.Clear();
            _debouncer.Dispose();
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
            }
            catch (Exception ex)
            {
                var errorEntry = CreateErrorLogEntry(e, ex);
                _csvLogger.LogError(errorEntry);
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
