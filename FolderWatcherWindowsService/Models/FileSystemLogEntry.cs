using System;

namespace FolderWatcherWindowsService.Models
{
    /// <summary>
    /// Represents a complete log entry for a file system event.
    /// </summary>
    internal class FileSystemLogEntry
    {
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
    }
}