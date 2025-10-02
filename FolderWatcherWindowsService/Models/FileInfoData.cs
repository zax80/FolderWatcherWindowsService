using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderWatcherWindowsService.Models
{
    /// <summary>
    /// Represents file or directory information for logging.
    /// </summary>
    internal class FileInfoData
    {
        public string FileType { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string LastAccessed { get; set; } = string.Empty;
        public string LastModified { get; set; } = string.Empty;
    }
}
