using System.Threading.Tasks;
using FolderWatcherWindowsService.Common.Interfaces;
using FolderWatcherWindowsService.Common.Models;

namespace FolderWatcherWindowsService.Services
{
    /// <summary>
    /// Null Object implementation of IThreatDetectionService.
    /// Used in free version when threat detection module is not available.
    /// SOLID: Null Object Pattern - provides do-nothing implementation.
    /// </summary>
    public class NullThreatDetectionService : IThreatDetectionService
    {
        /// <summary>
        /// Initialize - no-op for null implementation.
        /// </summary>
        public bool Initialize()
        {
            return true;
        }

        /// <summary>
        /// Initialize with config - no-op for null implementation.
        /// </summary>
        public bool Initialize(object config)
        {
            return true;
        }

        /// <summary>
        /// Analyze - no-op for null implementation.
        /// </summary>
        public Task AnalyzeAsync(FileSystemLogEntry entry)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Cleanup - no-op for null implementation.
        /// </summary>
        public void Cleanup()
        {
            // No resources to clean up
        }
    }
}
