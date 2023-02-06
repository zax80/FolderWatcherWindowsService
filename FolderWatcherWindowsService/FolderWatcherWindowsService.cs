using log4net;
using log4net.Appender;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;

namespace FolderWatcherWindowsService
{
    public partial class FolderWatcherWindowsService : ServiceBase
    {
        // Log4Net
        private ILog mLogger;

        // Collection for the FileSystemWatcher objects
        private List<FileSystemWatcher> fileSystemWatchers;

        public FolderWatcherWindowsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // write entry in event log
            EventLog.WriteEntry("Folder Watcher Windows Service is starting.", EventLogEntryType.Information);

            // get log Log4 configuration from App.config
            ConfigureLog4Net();

            // start watching of folders
            StartWatching();
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry("Folder Watcher Windows Service is stoping.", EventLogEntryType.Information);

            StopWatching();
        }

        /// <summary>
        /// Get folder paths, create FileSystemWatcher instances & populate fileSystemWatchers collection.
        /// </summary>
        private void StartWatching()
        {
            // get folder paths from App.config
            StringCollection folderPaths = Properties.Settings.Default.FolderPaths;

            // create FileSystemWatcher instances for each path and attach event handlers
            foreach (string path in folderPaths)
            {
                if (fileSystemWatchers == null) { fileSystemWatchers = new List<FileSystemWatcher>(); }
                try
                {
                    // create FileSystemWatcher instance
                    FileSystemWatcher mFSW = new FileSystemWatcher(path);
                    mFSW.EnableRaisingEvents = true;
                    mFSW.IncludeSubdirectories = true;

                    // Log4Net log file filter
                    string logFilePath = LogManager.GetRepository()
                                  .GetAppenders()
                                  .OfType<FileAppender>().First().File;
                    string logFile = Path.GetFileName(logFilePath);
                    string logFileFilter = "!" + logFile;
                    mFSW.Filter = logFileFilter;

                    // events
                    mFSW.Created += SomethingHappenedToTheFolder;
                    mFSW.Changed += SomethingHappenedToTheFolder;
                    mFSW.Deleted += SomethingHappenedToTheFolder;
                    mFSW.Renamed += SomethingHappenedToTheFolder;

                    // add FileSystemWatcher to fileSystemWatchers List collection
                    fileSystemWatchers.Add(mFSW);
                }
                catch (Exception exc)
                {
                    mLogger.Error(exc.ToString());
                }
            }
        }

        /// <summary>
        /// Clean up unmanaged resources.
        /// </summary>
        private void StopWatching()
        {
            if (fileSystemWatchers != null)
            {
                foreach (FileSystemWatcher fileSystemWatcher in fileSystemWatchers)
                {
                    fileSystemWatcher.Dispose();
                }
            }
        }

        /// <summary>
        /// FileSystemWatchers event handler.
        /// </summary>
        /// <param name="sender">FileSystemWatcher</param>
        /// <param name="e">Arguments of type FileSystemEventArgs</param>
        private void SomethingHappenedToTheFolder(object sender, FileSystemEventArgs e)
        {
            mLogger.Debug
                (string.Format("{0} - \r\nFile event for: {1}",
                e.ChangeType, e.FullPath));

            Debug.WriteLine(string.Format("{0} - \r\n File event for: {1}",
                e.ChangeType, e.FullPath));
        }

        /// <summary>
        /// get log Log4 configuration from App.config
        /// </summary>
        private void ConfigureLog4Net()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure();
                mLogger = LogManager.GetLogger("servicelog");
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
            }
        }
    }
}