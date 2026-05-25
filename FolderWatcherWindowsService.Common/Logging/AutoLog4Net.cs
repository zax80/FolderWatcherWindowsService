using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

// Assembly-level attribute - triggers when ANY assembly loads Common
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace FolderWatcherWindowsService.Common.Logging
{
    /// <summary>
    /// ZERO-CONFIGURATION log4net setup.
    /// Just reference FolderWatcherWindowsService.Common and logging works everywhere!
    /// Auto-detects: app name, config files, log locations, build mode.
    /// NO CODE NEEDED in your projects - it's all automatic!
    /// </summary>
    public static class AutoLog4Net
    {
        private static volatile bool _initialized = false;
        private static readonly object _lock = new object();

        // Static constructor - runs once when type is first accessed
        static AutoLog4Net()
        {
            Initialize();
        }

        /// <summary>
        /// Auto-initializes log4net. Called automatically - you don't need to call this!
        /// </summary>
        private static void Initialize()
        {
            if (_initialized) return;

            lock (_lock)
            {
                if (_initialized) return;

                try
                {
                    // Detect application details
                    string appName = AutoDetectAppName();
                    string configPath = AutoDetectConfigFile();

                    // Try to use config file first
                    if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath))
                    {
                        ConfigureFromFile(configPath);
                    }
                    else
                    {
                        // Fallback: Auto-configure programmatically
                        ConfigureAuto(appName);
                    }

                    _initialized = true;

                    // Silent test - only log to Event Log
                    WriteEventLog($"log4net auto-initialized for {appName}");
                }
                catch (Exception ex)
                {
                    WriteEventLog($"log4net auto-init error: {ex.Message}", EventLogEntryType.Error);
                }
            }
        }

        /// <summary>
        /// Configure from file.
        /// </summary>
        private static void ConfigureFromFile(string configPath)
        {
            try
            {
                var configFile = new FileInfo(configPath);
                XmlConfigurator.ConfigureAndWatch(configFile);
                WriteEventLog($"Loaded config: {configPath}");
            }
            catch (Exception ex)
            {
                WriteEventLog($"Config file error: {ex.Message}", EventLogEntryType.Warning);
                ConfigureAuto(AutoDetectAppName());
            }
        }

        /// <summary>
        /// Auto-configure programmatically.
        /// </summary>
        private static void ConfigureAuto(string appName)
        {
            try
            {
                var repository = GetOrCreateRepository();

                // Simple pattern
                var layout = new PatternLayout("%date [%thread] %-5level %logger - %message%newline");
                layout.ActivateOptions();

                // Find writable log location
                string logPath = FindLogPath(appName);

                // File appender
                var fileAppender = new RollingFileAppender
                {
                    File = logPath,
                    AppendToFile = true,
                    RollingStyle = RollingFileAppender.RollingMode.Size,
                    MaxSizeRollBackups = 10,
                    MaximumFileSize = "10MB",
                    Layout = layout,
                    ImmediateFlush = true,
                    LockingModel = new FileAppender.MinimalLock()
                };
                fileAppender.ActivateOptions();

                // Console appender
                var consoleAppender = new ConsoleAppender { Layout = layout };
                consoleAppender.ActivateOptions();

                // Apply
                BasicConfigurator.Configure(repository, fileAppender, consoleAppender);

#if DEBUG
                repository.Threshold = Level.All;
#else
                repository.Threshold = Level.Info;
#endif
                repository.Configured = true;

                WriteEventLog($"Auto-configured: {logPath}");
            }
            catch (Exception ex)
            {
                WriteEventLog($"Auto-config error: {ex.Message}", EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// Auto-detect application name from entry assembly or process.
        /// </summary>
        private static string AutoDetectAppName()
        {
            try
            {
                // Try entry assembly first
                var asm = Assembly.GetEntryAssembly();
                if (asm != null)
                {
                    string name = asm.GetName().Name;
                    if (name.Contains("Service") && !name.Contains("Admin")) return "Service";
                    if (name.Contains("Admin")) return "Admin";
                    return name;
                }
            }
            catch { }

            try
            {
                // Fallback to process name
                string proc = Process.GetCurrentProcess().ProcessName;
                if (proc.Contains("Service")) return "Service";
                if (proc.Contains("Admin")) return "Admin";
                return proc;
            }
            catch { }

            return "App";
        }

        /// <summary>
        /// Auto-detect log4net config file in multiple locations.
        /// Supports: log4net.config, app.config, web.config, [AppName].exe.config
        /// </summary>
        private static string AutoDetectConfigFile()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string appName = AutoDetectAppName();

                // Check common config file names
                string[] candidates = new[]
                {
                    Path.Combine(baseDir, "log4net.config"),
                    Path.Combine(baseDir, $"{appName}.log4net.config"),
                    Path.Combine(baseDir, "app.config"),
                    Path.Combine(baseDir, $"{appName}.exe.config"),
                    Path.Combine(baseDir, "web.config"),
                    "log4net.config",
                    "app.config"
                };

                foreach (var path in candidates)
                {
                    if (File.Exists(path))
                    {
                        // Verify it contains log4net configuration
                        if (IsValidLog4NetConfig(path))
                        {
                            return path;
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Check if file contains log4net configuration.
        /// </summary>
        private static bool IsValidLog4NetConfig(string path)
        {
            try
            {
                string content = File.ReadAllText(path);
                return content.Contains("log4net") || content.Contains("<appender");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Find writable log file location.
        /// </summary>
        private static string FindLogPath(string appName)
        {
            string fileName = $"{appName}.log";

            string[] locations = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "FolderWatcher", fileName),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FolderWatcher", fileName),
                Path.Combine(Path.GetTempPath(), "FolderWatcher", fileName)
            };

            foreach (var path in locations)
            {
                if (TestWrite(path))
                    return path;
            }

            return Path.Combine(Path.GetTempPath(), fileName);
        }

        /// <summary>
        /// Test if we can write to a path.
        /// </summary>
        private static bool TestWrite(string path)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.AppendAllText(path, string.Empty);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get or create log4net repository.
        /// </summary>
        private static ILoggerRepository GetOrCreateRepository()
        {
            try
            {
                return LogManager.GetRepository(Assembly.GetEntryAssembly());
            }
            catch
            {
                try
                {
                    return LogManager.CreateRepository(
                        Assembly.GetEntryAssembly(),
                        typeof(log4net.Repository.Hierarchy.Hierarchy));
                }
                catch
                {
                    return LogManager.GetRepository();
                }
            }
        }

        /// <summary>
        /// Write to Windows Event Log for diagnostics.
        /// </summary>
        private static void WriteEventLog(string message, EventLogEntryType type = EventLogEntryType.Information)
        {
            try
            {
                const string source = "FolderWatcher";
                if (!EventLog.SourceExists(source))
                    EventLog.CreateEventSource(source, "Application");
                EventLog.WriteEntry(source, message, type);
            }
            catch { }
        }
    }

    /// <summary>
    /// Ensures AutoLog4Net initializes when Common assembly loads.
    /// </summary>
    internal static class Log4NetBootstrap
    {
        static Log4NetBootstrap()
        {
            // Force AutoLog4Net static constructor to run
            var _ = typeof(AutoLog4Net);
        }

        internal static void EnsureLoaded()
        {
            // Trigger static constructor
        }
    }
}
