using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderWatcherWindowsService.Models
{
    /// <summary>
    /// Represents user information for logging.
    /// </summary>
    internal class UserInfoData
    {
        public string ServiceUser { get; set; } = string.Empty;
        public string ModifiedBy { get; set; } = string.Empty;
    }
}
