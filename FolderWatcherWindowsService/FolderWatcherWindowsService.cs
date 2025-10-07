using log4net;
using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;

namespace FolderWatcherWindowsService
{
    /// <summary>
    /// Windows Service that monitors specified folders for file system changes and logs them in CSV format.
    /// </summary>
    public partial class FolderWatcherWindowsService : ServiceBase
    {
        private readonly ILog _logger;
        private readonly WatcherManager _watcherManager;
        private readonly CsvLogger _csvLogger;

        public FolderWatcherWindowsService()
        {
            InitializeComponent();

            ConfigureLog4Net();
            _logger = LogManager.GetLogger("servicelog");
            _csvLogger = new CsvLogger(_logger);
            _watcherManager = new WatcherManager(_logger, _csvLogger);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                LogServiceEvent("Folder Watcher Windows Service is starting.", EventLogEntryType.Information);
                _logger?.Info("Service starting - initializing folder watchers");

                ConfigurationManager.RefreshSection("appSettings");

                _csvLogger.WriteHeader();
                _watcherManager.StartWatching(Properties.Settings.Default.FolderPaths);

                _logger?.Info("Service started successfully");
            }
            catch (Exception ex)
            {
                LogServiceError("Failed to start service", ex);
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                LogServiceEvent("Folder Watcher Windows Service is stopping.", EventLogEntryType.Information);
                _logger?.Info("Service stopping - cleaning up resources");

                _watcherManager.StopWatching();

                _logger?.Info("Service stopped successfully");
            }
            catch (Exception ex)
            {
                LogServiceError("Error during service stop", ex);
            }
        }

        private void ConfigureLog4Net()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Failed to configure Log4Net: {ex.Message}", EventLogEntryType.Error);
            }
        }

        private void LogServiceEvent(string message, EventLogEntryType type)
        {
            EventLog.WriteEntry(message, type);
        }

        private void LogServiceError(string message, Exception ex)
        {
            string errorMsg = $"{message}: {ex.Message}";
            EventLog.WriteEntry(errorMsg, EventLogEntryType.Error);
            _logger?.Error(errorMsg, ex);
        }
    }
}