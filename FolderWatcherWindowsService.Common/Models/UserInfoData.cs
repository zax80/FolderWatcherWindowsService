namespace FolderWatcherWindowsService.Common.Models
{
  /// <summary>
    /// User information data collected during monitoring.
    /// </summary>
    public class UserInfoData
    {
        public string ServiceUser { get; set; } = string.Empty;
     public string ModifiedBy { get; set; } = string.Empty;
    }
}
