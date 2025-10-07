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