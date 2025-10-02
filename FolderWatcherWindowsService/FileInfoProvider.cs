using FolderWatcherWindowsService.Models;
using System;
using System.IO;
using System.Security.Principal;

/// <summary>
/// Provides file and directory information for logging.
/// </summary>
namespace FolderWatcherWindowsService
{
    internal class FileInfoProvider
    {
        public FileInfoData GetFileInfo(FileSystemEventArgs e)
        {
            var data = new FileInfoData();

            try
            {
                if (e.ChangeType == WatcherChangeTypes.Deleted)
                {
                    data.FileType = "Deleted";
                    return data;
                }

                var attributes = File.GetAttributes(e.FullPath);
                bool isDirectory = (attributes & FileAttributes.Directory) == FileAttributes.Directory;

                if (isDirectory)
                {
                    PopulateDirectoryInfo(data, e.FullPath);
                }
                else
                {
                    PopulateFileInfo(data, e.FullPath);
                }
            }
            catch (Exception)
            {
                data.FileType = "Error";
            }

            return data;
        }

        public UserInfoData GetUserInfo(string filePath)
        {
            return new UserInfoData
            {
                ServiceUser = WindowsIdentity.GetCurrent().Name,
                ModifiedBy = GetFileOwner(filePath)
            };
        }

        private void PopulateDirectoryInfo(FileInfoData data, string path)
        {
            data.FileType = "Directory";
            var dirInfo = new DirectoryInfo(path);
            data.LastAccessed = FormatDateTime(dirInfo.LastAccessTime);
            data.LastModified = FormatDateTime(dirInfo.LastWriteTime);
        }

        private void PopulateFileInfo(FileInfoData data, string path)
        {
            data.FileType = "File";
            var fileInfo = new FileInfo(path);
            data.FileSize = fileInfo.Length.ToString();
            data.Extension = fileInfo.Extension;
            data.LastAccessed = FormatDateTime(fileInfo.LastAccessTime);
            data.LastModified = FormatDateTime(fileInfo.LastWriteTime);
        }

        private string GetFileOwner(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                var security = fileInfo.GetAccessControl();
                var owner = security.GetOwner(typeof(SecurityIdentifier));
                return owner?.Translate(typeof(NTAccount))?.Value ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }
}