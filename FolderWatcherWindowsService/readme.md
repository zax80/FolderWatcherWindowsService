# Folder Watcher Windows Service

A Windows Service that monitors folders for file changes and logs all activity in CSV format.

## Features

- Monitors multiple folders in real-time
- Logs all file changes (create, modify, delete, rename)
- CSV output for easy analysis
- Automatic error recovery
- Tracks file details (size, timestamps, owner)

## Requirements

- Windows 10/Server 2012 R2 or higher
- .NET Framework 4.7.2+
- Administrator privileges

## Installation

### 1. Build the Project
Build the solution in Visual Studio using Release configuration.

### 2. Install the Service
Open Command Prompt as Administrator:

```cmd
cd C:\Path\To\Service\bin\Release
%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe FolderWatcherWindowsService.exe
```

### 3. Start the Service

```cmd
net start FolderWatcherService
```

## Configuration

Edit `App.config` to set folders to monitor:

```xml
<setting name="FolderPaths" serializeAs="Xml">
  <value>
    <ArrayOfString>
      <string>C:\Monitored\Folder1</string>
      <string>C:\Monitored\Folder2</string>
    </ArrayOfString>
  </value>
</setting>
```

Set log file location:

```xml
<log4net>
  <appender name="RollingFileAppender" type="log4net.Appender.RollingFileAppender">
    <file value="C:\Logs\FolderWatcher\service.log" />
    <maximumFileSize value="10MB" />
    <maxSizeRollBackups value="10" />
  </appender>
</log4net>
```

## Log Format

The service creates CSV logs with these columns:

- **Timestamp** - When the event happened
- **ChangeType** - Created, Changed, Deleted, or Renamed
- **FilePath** - Full path to the file
- **ServiceUser** - User running the service
- **ModifiedBy** - File owner
- **FileType** - File or Directory
- **FileSize** - Size in bytes
- **Extension** - File extension (.txt, .pdf, etc.)
- **LastAccessed** - Last access time
- **LastModified** - Last modified time
- **Error** - Error message (if any)

### Example Log

```csv
Timestamp,ChangeType,FilePath,ServiceUser,ModifiedBy,FileType,FileSize,Extension,LastAccessed,LastModified,Error
2025-10-01 14:30:15.123,Created,C:\Data\report.xlsx,NT AUTHORITY\SYSTEM,DOMAIN\John,File,15360,.xlsx,2025-10-01 14:30:15.000,2025-10-01 14:30:15.000,
2025-10-01 14:31:20.456,Changed,C:\Data\report.xlsx,NT AUTHORITY\SYSTEM,DOMAIN\John,File,18432,.xlsx,2025-10-01 14:31:20.000,2025-10-01 14:31:20.000,
```

## Usage

### Control the Service

```cmd
# Start
net start FolderWatcherService

# Stop
net stop FolderWatcherService

# Check status
sc query FolderWatcherService
```

### View Logs

```powershell
# View recent entries
Get-Content C:\Logs\FolderWatcher\service.log -Tail 50

# Analyze in PowerShell
$logs = Import-Csv "C:\Logs\FolderWatcher\service.log"
$logs | Where-Object {$_.ChangeType -eq "Deleted"}
```

Or open the CSV file directly in Excel.

## Troubleshooting

### Service Won't Start

Check Windows Event Log:
```powershell
Get-EventLog -LogName Application -Source "FolderWatcherService" -Newest 10
```

Common fixes:
- Verify folder paths exist
- Check service account has folder access
- Ensure log directory is writable

### No Logs Generated

1. Verify service is running: `sc query FolderWatcherService`
2. Check App.config has valid folder paths
3. Confirm log file location is writable

### Update Configuration

After changing App.config:

```cmd
net stop FolderWatcherService
# Copy new config to service directory
net start FolderWatcherService
```

## Uninstall

```cmd
net stop FolderWatcherService
%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe /u FolderWatcherWindowsService.exe
```

## Best Practices

- Monitor no more than 10 folders per service
- Avoid monitoring system folders (Windows, Program Files)
- Archive old logs regularly
- Run service with minimum required permissions

## Architecture

The service consists of:
- **WatcherManager** - Manages folder monitoring
- **EventDebouncer** - Prevents duplicate events (500ms delay)
- **FileInfoProvider** - Collects file information
- **CsvLogger** - Formats and writes logs

## Support

For issues:
- Check Windows Event Log for errors
- Review service log file
- Verify configuration settings

---

**Version:** 2.0.0  
**Last Updated:** October 2025