using System;

namespace FolderWatcherWindowsServiceAdmin.Models
{
    /// <summary>
    /// Summary statistics for the log viewer.
    /// </summary>
    public class LogSummary
    {
        public int TotalRecords { get; set; }
        public int RecentActivityCount { get; set; }
        public int ErrorCount { get; set; }
        public int CreatedCount { get; set; }
        public int ModifiedCount { get; set; }
        public int DeletedCount { get; set; }
        public int RenamedCount { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}