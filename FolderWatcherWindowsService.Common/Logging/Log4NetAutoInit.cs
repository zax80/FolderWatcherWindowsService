using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.IO;

namespace FolderWatcherWindowsService.Common.Logging
{
    /// <summary>
    /// Automatically configures log4net for the application.
    /// This eliminates the need for App.config or log4net.config files.
    /// </summary>
    public static class Log4NetAutoInit
    {
        private static bool _isInitialized = false;
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Ensures log4net is initialized with default configuration.
        /// Safe to call multiple times - will only initialize once.
        /// </summary>
        public static void EnsureInitialized()
        {
            if (_isInitialized)
                return;

            lock (_lockObject)
            {
                if (_isInitialized)
                    return;

                ConfigureLog4Net();
                _isInitialized = true;
            }
        }

        /// <summary>
        /// Configures log4net programmatically with optimized settings.
        /// </summary>
        private static void ConfigureLog4Net()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();

            // Create pattern layout that matches LogParser expectations
            var patternLayout = new PatternLayout
            {
                // CRITICAL: This pattern must match the LogParser regex
                // Pattern: LEVEL DATE TIME THREADID MESSAGE
                ConversionPattern = "%level %date{yyyy-MM-dd HH:mm:ss,fff} %thread %message%newline"
            };
            patternLayout.ActivateOptions();

            // Create rolling file appender
            var roller = new RollingFileAppender
            {
                AppendToFile = true,
                // CRITICAL: Must be "ServiceLog.txt" to match LogParser search paths
                File = "ServiceLog.txt",
                Layout = patternLayout,
                MaxSizeRollBackups = 10,
                MaximumFileSize = "10MB",
                RollingStyle = RollingFileAppender.RollingMode.Size,
                StaticLogFileName = true
            };
            roller.ActivateOptions();

            // Add appender to hierarchy
            hierarchy.Root.AddAppender(roller);

            // Set logging level
            hierarchy.Root.Level = Level.Info;
            hierarchy.Configured = true;
        }

        /// <summary>
        /// Gets a logger for the specified type.
        /// Ensures initialization before returning the logger.
        /// </summary>
        public static ILog GetLogger(Type type)
        {
            EnsureInitialized();
            return LogManager.GetLogger(type);
        }

        /// <summary>
        /// Gets a logger with the specified name.
        /// Ensures initialization before returning the logger.
        /// </summary>
        public static ILog GetLogger(string name)
        {
            EnsureInitialized();
            return LogManager.GetLogger(name);
        }
    }
}