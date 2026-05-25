using System;

namespace FolderWatcherWindowsService.Common.Models
{
    /// <summary>
    /// Represents a complete log entry for a file system event.
    /// Shared across all modules for consistent logging and parsing.
    /// Used by: Service (for logging), Admin (for parsing/display), ThreatDetection (for analysis).
    /// </summary>
    public class FileSystemLogEntry
    {
        // This single line triggers log4net auto-initialization
        static FileSystemLogEntry()
        {
            Logging.Log4NetAutoInit.EnsureInitialized();
        }
        /// <summary>
        /// When the file system event occurred.
        /// Format: yyyy-MM-dd HH:mm:ss,fff (e.g., "2025-01-15 14:30:45,123")
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Type of file system change that occurred.
        /// Valid values: "Created", "Changed", "Deleted", "Renamed"
        /// Maps to FileSystemWatcher.WatcherChangeTypes enumeration.
        /// </summary>
        public string ChangeType { get; set; } = string.Empty;

        /// <summary>
        /// Full path to the file or directory that was affected.
        /// Example: "C:\Users\Documents\report.docx"
        /// Includes both filename and complete directory path.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// The Windows user account under which the service is running.
        /// Example: "NT AUTHORITY\SYSTEM" or "DOMAIN\ServiceAccount"
        /// Retrieved from WindowsIdentity.GetCurrent().Name
        /// </summary>
        public string ServiceUser { get; set; } = string.Empty;

        /// <summary>
        /// The user who actually modified the file (if determinable).
        /// Example: "DOMAIN\JohnDoe" or "COMPUTER\Username"
        /// Retrieved from file owner information via GetAccessControl().GetOwner()
        /// May be empty if user cannot be determined.
        /// </summary>
        public string ModifiedBy { get; set; } = string.Empty;

        /// <summary>
        /// General category/type of the file.
        /// Valid values: "File", "Directory", "Unknown"
        /// Determined by checking FileInfo vs DirectoryInfo attributes.
        /// </summary>
        public string FileType { get; set; } = string.Empty;

        /// <summary>
        /// Size of the file in bytes as a formatted string.
        /// Example: "1024 bytes", "2.5 KB", "10.3 MB"
        /// Empty for directories or if file size cannot be determined.
        /// </summary>
        public string FileSize { get; set; } = string.Empty;

        /// <summary>
        /// File extension including the leading dot.
        /// Example: ".docx", ".pdf", ".txt", ".exe"
        /// Empty string for files without extensions or directories.
        /// Used for file type classification and threat detection.
        /// </summary>
        public string Extension { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of when the file was last accessed.
        /// Format: yyyy-MM-dd HH:mm:ss (e.g., "2025-01-15 14:30:45")
        /// Retrieved from FileInfo.LastAccessTime
        /// May be empty if file doesn't exist or access time unavailable.
        /// </summary>
        public string LastAccessed { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of when the file was last modified.
        /// Format: yyyy-MM-dd HH:mm:ss (e.g., "2025-01-15 14:30:45")
        /// Retrieved from FileInfo.LastWriteTime
        /// May be empty if file doesn't exist or modification time unavailable.
        /// </summary>
        public string LastModified { get; set; } = string.Empty;

        /// <summary>
        /// Error message if an exception occurred while processing this event.
        /// Contains exception details including message and type.
        /// Empty string indicates successful processing with no errors.
        /// Used by Admin UI to display error count and error filtering.
        /// PREMIUM: Used by ThreatDetection to identify suspicious error patterns.
        /// </summary>
        public string Error { get; set; } = string.Empty;
    }
}
