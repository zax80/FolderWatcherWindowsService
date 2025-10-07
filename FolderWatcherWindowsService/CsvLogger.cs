using FolderWatcherWindowsService.Models;
using log4net;
using System;
using System.Diagnostics;

namespace FolderWatcherWindowsService
{
    /// <summary>
    /// Handles CSV formatting and logging.
    /// </summary>
    internal class CsvLogger
    {
        private readonly ILog _logger;

        public CsvLogger(ILog logger)
        {
            _logger = logger;
        }

        public void WriteHeader()
        {
            try
            {
                const string header = "Timestamp,ChangeType,FilePath,ServiceUser,ModifiedBy,FileType,FileSize,Extension,LastAccessed,LastModified,Error";
                _logger?.Info(header);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to write CSV header: {ex.Message}");
            }
        }

        public void LogEntry(FileSystemLogEntry entry)
        {
            string csvLine = FormatAsCsv(entry);
            _logger?.Info(csvLine);
            Debug.WriteLine(csvLine);
        }

        public void LogError(FileSystemLogEntry entry)
        {
            string csvLine = FormatAsCsv(entry);
            _logger?.Error(csvLine);
        }

        private string FormatAsCsv(FileSystemLogEntry entry)
        {
            return string.Join(",",
                EscapeField(entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff")),
                EscapeField(entry.ChangeType),
                EscapeField(entry.FilePath),
                EscapeField(entry.ServiceUser),
                EscapeField(entry.ModifiedBy),
                EscapeField(entry.FileType),
                EscapeField(entry.FileSize),
                EscapeField(entry.Extension),
                EscapeField(entry.LastAccessed),
                EscapeField(entry.LastModified),
                EscapeField(entry.Error)
            );
        }

        private string EscapeField(string field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return string.Empty;
            }

            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }
    }
}