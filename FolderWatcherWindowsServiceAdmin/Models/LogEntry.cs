using System;

namespace FolderWatcherWindowsServiceAdmin.Models
{
    /// <summary>
    /// Represents a parsed log entry from the service log file.
    /// </summary>
    public class LogEntry
    {
        public string LogLevel { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string ChangeType { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ServiceUser { get; set; } = string.Empty;
        public string ModifiedBy { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string LastAccessed { get; set; } = string.Empty;
        public string LastModified { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public string RawLine { get; set; } = string.Empty;
    }
}