using System.Threading.Tasks;

namespace FolderWatcherWindowsService.Common.Interfaces
{
    /// <summary>
    /// Interface for threat detection services.
    /// Allows the core service to work with or without threat detection module.
    /// SOLID: Dependency Inversion Principle - high-level modules depend on abstraction.
    /// </summary>
    public interface IThreatDetectionService
 {
        /// <summary>
        /// Initializes the threat detection service.
        /// Loads configuration from default location.
        /// </summary>
        /// <returns>True if initialization succeeded, false otherwise</returns>
        bool Initialize();

        /// <summary>
        /// Initializes the threat detection service with provided configuration.
        /// </summary>
        /// <param name="config">Configuration object (use AlertConfiguration)</param>
        /// <returns>True if initialization succeeded, false otherwise</returns>
        bool Initialize(object config);
        
        /// <summary>
        /// Analyzes a file system event for potential security threats.
        /// </summary>
        /// <param name="entry">The file system log entry to analyze</param>
   /// <returns>Task representing the async operation</returns>
        Task AnalyzeAsync(Models.FileSystemLogEntry entry);
    
        /// <summary>
        /// Performs cleanup of resources used by the threat detection service.
        /// </summary>
        void Cleanup();
 }
}
