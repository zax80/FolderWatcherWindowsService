using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FolderWatcherWindowsServiceAdmin.Models;

namespace FolderWatcherWindowsServiceAdmin.Services
{
    /// <summary>
    /// Service for parsing log4net log files and extracting CSV data with optimized performance.
    /// </summary>
    public class LogParser
    {
        private readonly string _logFilePath;
        private readonly Regex _logLinePattern;
        private const int DefaultBufferSize = 65536; // 64KB buffer for better I/O performance
        private const int LargeFileThreshold = 10 * 1024 * 1024; // 10MB
        private static int _debugEntryCount = 0; // Debug counter for logging first few entries

        public LogParser(string logFilePath)
        {
            _logFilePath = logFilePath;
            // Pre-compiled pattern to match log4net format: LEVEL DATE TIME MESSAGE
            _logLinePattern = new Regex(@"^(INFO|ERROR|DEBUG|WARN)\s+(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2},\d{3})\s+\d+\s+(.*)$", 
                RegexOptions.Compiled);
            
            // Reset debug counter for this parser instance
            _debugEntryCount = 0;
        }

        /// <summary>
        /// Parse all entries from the log file with optimized performance.
        /// </summary>
        public List<LogEntry> ParseLogFile()
        {
            var entries = new List<LogEntry>();

            if (!File.Exists(_logFilePath))
            {
                return entries;
            }

            try
            {
                var fileInfo = new FileInfo(_logFilePath);
                
                // Use different strategies based on file size
                if (fileInfo.Length > LargeFileThreshold)
                {
                    return ParseLargeFile();
                }
                else
                {
                    return ParseSmallToMediumFile();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing log file: {ex.Message}");
                return entries;
            }
        }

        /// <summary>
        /// Optimized parsing for small to medium files (under 10MB).
        /// </summary>
        private List<LogEntry> ParseSmallToMediumFile()
        {
            var entries = new List<LogEntry>();

            using (var fileStream = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, DefaultBufferSize))
            using (var reader = new StreamReader(fileStream, Encoding.UTF8, true, DefaultBufferSize))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var entry = ParseLogLine(line);
                    if (entry != null)
                    {
                        entries.Add(entry);
                    }
                }
            }

            return entries.OrderByDescending(e => e.Timestamp).ToList();
        }

        /// <summary>
        /// Optimized parsing for large files (over 10MB) using parallel processing.
        /// </summary>
        private List<LogEntry> ParseLargeFile()
        {
            var allLines = new List<string>();

            // Read all lines efficiently
            using (var fileStream = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, DefaultBufferSize))
            using (var reader = new StreamReader(fileStream, Encoding.UTF8, true, DefaultBufferSize))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    allLines.Add(line);
                }
            }

            // Parse lines in parallel for better performance on large files
            var entries = allLines
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .Select(ParseLogLine)
                .Where(entry => entry != null)
                .ToList();

            return entries.OrderByDescending(e => e.Timestamp).ToList();
        }

        /// <summary>
        /// Parse the last N entries from the log file efficiently using reverse reading.
        /// </summary>
        public List<LogEntry> ParseLastEntries(int count)
        {
            var entries = new List<LogEntry>();

            if (!File.Exists(_logFilePath))
            {
                return entries;
            }

            try
            {
                var lines = ReadLastLinesOptimized(_logFilePath, count * 2); // Read extra to account for non-CSV lines
                
                foreach (var line in lines)
                {
                    var entry = ParseLogLine(line);
                    if (entry != null)
                    {
                        entries.Add(entry);
                    }

                    if (entries.Count >= count)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing last entries: {ex.Message}");
            }

            return entries.OrderByDescending(e => e.Timestamp).ToList();
        }

        /// <summary>
        /// Parse a single log line into a LogEntry with optimized string handling.
        /// </summary>
        private LogEntry ParseLogLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            var match = _logLinePattern.Match(line);
            if (!match.Success)
            {
                return null;
            }

            var logLevel = match.Groups[1].Value;
            var timestampStr = match.Groups[2].Value;
            var message = match.Groups[3].Value;

            // Quick checks to skip non-CSV messages
            if (message.Length < 20 || !message.Contains(',') || message.StartsWith("Service", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Parse timestamp with optimized parsing
            if (!TryParseTimestamp(timestampStr, out DateTime timestamp))
            {
                return null;
            }

            // Parse CSV message with optimized CSV parser
            var csvFields = ParseCsvLineOptimized(message);
            if (csvFields.Count < 11) // Should have at least 11 fields
            {
                return null;
            }

            // Debug: Log some parsing details for first few entries
            var debugCount = System.Threading.Interlocked.Increment(ref _debugEntryCount);
            if (debugCount <= 5)
            {
                System.Diagnostics.Debug.WriteLine($"ParseLogLine #{debugCount}: LogLevel={logLevel}, CSV fields={csvFields.Count}");
                System.Diagnostics.Debug.WriteLine($"  Error field (index 10): '{(csvFields.Count > 10 ? csvFields[10] : "N/A")}'");
                System.Diagnostics.Debug.WriteLine($"  ChangeType field (index 1): '{(csvFields.Count > 1 ? csvFields[1] : "N/A")}'");
            }

            var entry = new LogEntry
            {
                LogLevel = logLevel,
                Timestamp = TryParseTimestamp(csvFields[0], out DateTime csvTimestamp) ? csvTimestamp : timestamp,
                ChangeType = csvFields.Count > 1 ? csvFields[1] : string.Empty,
                FilePath = csvFields.Count > 2 ? csvFields[2] : string.Empty,
                ServiceUser = csvFields.Count > 3 ? csvFields[3] : string.Empty,
                ModifiedBy = csvFields.Count > 4 ? csvFields[4] : string.Empty,
                FileType = csvFields.Count > 5 ? csvFields[5] : string.Empty,
                FileSize = csvFields.Count > 6 ? csvFields[6] : string.Empty,
                Extension = csvFields.Count > 7 ? csvFields[7] : string.Empty,
                LastAccessed = csvFields.Count > 8 ? csvFields[8] : string.Empty,
                LastModified = csvFields.Count > 9 ? csvFields[9] : string.Empty,
                Error = csvFields.Count > 10 ? csvFields[10].Trim() : string.Empty, // Trim whitespace from error field
                RawLine = line
            };

            return entry;
        }

        /// <summary>
        /// Optimized timestamp parsing with multiple format support.
        /// </summary>
        private bool TryParseTimestamp(string timestampStr, out DateTime timestamp)
        {
            // Try the most common format first
            if (DateTime.TryParseExact(timestampStr, "yyyy-MM-dd HH:mm:ss,fff", 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp))
            {
                return true;
            }

            // Try alternative format
            if (DateTime.TryParseExact(timestampStr, "yyyy-MM-dd HH:mm:ss.fff", 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp))
            {
                return true;
            }

            // Fallback to general parsing
            return DateTime.TryParse(timestampStr, out timestamp);
        }

        /// <summary>
        /// Optimized CSV line parser using StringBuilder for better memory efficiency.
        /// </summary>
        private List<string> ParseCsvLineOptimized(string line)
        {
            var fields = new List<string>();
            var fieldBuilder = new StringBuilder();
            var inQuotes = false;
            var i = 0;

            while (i < line.Length)
            {
                var ch = line[i];

                if (ch == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Escaped quote
                        fieldBuilder.Append('"');
                        i += 2;
                    }
                    else
                    {
                        // Toggle quote state
                        inQuotes = !inQuotes;
                        i++;
                    }
                }
                else if (ch == ',' && !inQuotes)
                {
                    // Field separator
                    fields.Add(fieldBuilder.ToString());
                    fieldBuilder.Clear();
                    i++;
                }
                else
                {
                    fieldBuilder.Append(ch);
                    i++;
                }
            }

            // Add the last field
            fields.Add(fieldBuilder.ToString());

            return fields;
        }

        /// <summary>
        /// Optimized reverse file reading with larger buffers and better memory management.
        /// </summary>
        private List<string> ReadLastLinesOptimized(string filePath, int lineCount)
        {
            const int bufferSize = 16384; // 16KB buffer for better performance
            var lines = new List<string>();

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize))
            {
                if (fileStream.Length == 0)
                {
                    return lines;
                }

                var buffer = new byte[bufferSize];
                var lineBytes = new List<byte>();
                var position = fileStream.Length;
                var foundLines = new List<string>();

                while (position > 0 && foundLines.Count < lineCount)
                {
                    var bytesToRead = (int)Math.Min(buffer.Length, position);
                    position -= bytesToRead;
                    fileStream.Seek(position, SeekOrigin.Begin);
                    var bytesRead = fileStream.Read(buffer, 0, bytesToRead);

                    // Process bytes in reverse order
                    for (int i = bytesRead - 1; i >= 0; i--)
                    {
                        if (buffer[i] == '\n')
                        {
                            if (lineBytes.Count > 0)
                            {
                                // Convert line buffer to string (reverse it first)
                                lineBytes.Reverse();
                                var lineText = Encoding.UTF8.GetString(lineBytes.ToArray()).TrimEnd('\r');
                                if (!string.IsNullOrWhiteSpace(lineText))
                                {
                                    foundLines.Add(lineText);
                                }
                                lineBytes.Clear();

                                if (foundLines.Count >= lineCount)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            lineBytes.Add(buffer[i]);
                        }
                    }
                }

                // Handle any remaining content in the line buffer
                if (lineBytes.Count > 0 && foundLines.Count < lineCount)
                {
                    lineBytes.Reverse();
                    var lineText = Encoding.UTF8.GetString(lineBytes.ToArray()).TrimEnd('\r');
                    if (!string.IsNullOrWhiteSpace(lineText))
                    {
                        foundLines.Add(lineText);
                    }
                }

                // Reverse to get chronological order (oldest first)
                foundLines.Reverse();
                return foundLines;
            }
        }

        /// <summary>
        /// Asynchronous version for non-blocking UI operations.
        /// </summary>
        public async Task<List<LogEntry>> ParseLogFileAsync()
        {
            return await Task.Run(() => ParseLogFile()).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronous version for parsing last entries.
        /// </summary>
        public async Task<List<LogEntry>> ParseLastEntriesAsync(int count)
        {
            return await Task.Run(() => ParseLastEntries(count)).ConfigureAwait(false);
        }

        /// <summary>
        /// Generate summary statistics from log entries with optimized LINQ.
        /// </summary>
        public LogSummary GenerateSummary(IEnumerable<LogEntry> entries)
        {
            var entriesList = entries as List<LogEntry> ?? entries.ToList();
            
            if (!entriesList.Any())
            {
                return new LogSummary
                {
                    TotalRecords = 0,
                    RecentActivityCount = 0,
                    ErrorCount = 0,
                    CreatedCount = 0,
                    ModifiedCount = 0,
                    DeletedCount = 0,
                    RenamedCount = 0,
                    LastUpdate = null
                };
            }

            var now = DateTime.Now;
            var recentThreshold = now.AddMinutes(-30); // Last 30 minutes

            // Use parallel processing for large datasets
            if (entriesList.Count > 1000)
            {
                return GenerateSummaryParallel(entriesList, recentThreshold, now);
            }

            // Debug: Log some info about the calculation
            System.Diagnostics.Debug.WriteLine($"GenerateSummary: Total entries = {entriesList.Count}");
            System.Diagnostics.Debug.WriteLine($"GenerateSummary: Current time = {now:yyyy-MM-dd HH:mm:ss}");
            System.Diagnostics.Debug.WriteLine($"GenerateSummary: Recent threshold = {recentThreshold:yyyy-MM-dd HH:mm:ss}");
            
            // Count entries with actual errors - be more specific about what constitutes an error
            // An error is either:
            // 1. Has non-empty Error field content (actual error message)
            // 2. Has LogLevel of ERROR (but only if Error field is empty to avoid double counting)
            var errorCount = 0;
            var entriesWithErrorField = 0;
            var entriesWithErrorLogLevel = 0;
            
            foreach (var entry in entriesList)
            {
                var hasErrorField = !string.IsNullOrWhiteSpace(entry.Error);
                var hasErrorLogLevel = entry.LogLevel.Equals("ERROR", StringComparison.OrdinalIgnoreCase);
                
                if (hasErrorField)
                {
                    entriesWithErrorField++;
                    errorCount++;
                }
                else if (hasErrorLogLevel)
                {
                    entriesWithErrorLogLevel++;
                    errorCount++;
                }
            }

            // Count recent activity based on entry timestamps
            var recentActivityCount = entriesList.Count(e => e.Timestamp >= recentThreshold);
            
            // Get last update time
            var lastUpdate = entriesList.Max(e => e.Timestamp);

            System.Diagnostics.Debug.WriteLine($"GenerateSummary: Entries with Error field = {entriesWithErrorField}");
            System.Diagnostics.Debug.WriteLine($"GenerateSummary: Entries with ERROR LogLevel (no Error field) = {entriesWithErrorLogLevel}");
            System.Diagnostics.Debug.WriteLine($"GenerateSummary: Total error count = {errorCount}");
            System.Diagnostics.Debug.WriteLine($"GenerateSummary: Recent activity count = {recentActivityCount}");
            System.Diagnostics.Debug.WriteLine($"GenerateSummary: Last update = {lastUpdate:yyyy-MM-dd HH:mm:ss}");

            return new LogSummary
            {
                TotalRecords = entriesList.Count,
                RecentActivityCount = recentActivityCount,
                ErrorCount = errorCount,
                CreatedCount = entriesList.Count(e => e.ChangeType.Equals("Created", StringComparison.OrdinalIgnoreCase)),
                ModifiedCount = entriesList.Count(e => e.ChangeType.Equals("Changed", StringComparison.OrdinalIgnoreCase)),
                DeletedCount = entriesList.Count(e => e.ChangeType.Equals("Deleted", StringComparison.OrdinalIgnoreCase)),
                RenamedCount = entriesList.Count(e => e.ChangeType.Equals("Renamed", StringComparison.OrdinalIgnoreCase)),
                LastUpdate = lastUpdate
            };
        }

        /// <summary>
        /// Generate summary using parallel processing for large datasets.
        /// </summary>
        private LogSummary GenerateSummaryParallel(List<LogEntry> entries, DateTime recentThreshold, DateTime now)
        {
            var parallelQuery = entries.AsParallel();

            // Debug: Log some info about the calculation
            System.Diagnostics.Debug.WriteLine($"GenerateSummaryParallel: Total entries = {entries.Count}");
            System.Diagnostics.Debug.WriteLine($"GenerateSummaryParallel: Current time = {now:yyyy-MM-dd HH:mm:ss}");
            System.Diagnostics.Debug.WriteLine($"GenerateSummaryParallel: Recent threshold = {recentThreshold:yyyy-MM-dd HH:mm:ss}");

            // Count entries with actual errors - be more specific about what constitutes an error
            var errorEntries = parallelQuery.Where(e => 
                !string.IsNullOrWhiteSpace(e.Error) || 
                (string.IsNullOrWhiteSpace(e.Error) && e.LogLevel.Equals("ERROR", StringComparison.OrdinalIgnoreCase))
            ).ToList();

            // Count recent activity based on entry timestamps
            var recentActivityCount = parallelQuery.Count(e => e.Timestamp >= recentThreshold);
            
            // Get last update time
            var lastUpdate = parallelQuery.Max(e => e.Timestamp);

            System.Diagnostics.Debug.WriteLine($"GenerateSummaryParallel: Error count = {errorEntries.Count}");
            System.Diagnostics.Debug.WriteLine($"GenerateSummaryParallel: Recent activity count = {recentActivityCount}");
            System.Diagnostics.Debug.WriteLine($"GenerateSummaryParallel: Last update = {lastUpdate:yyyy-MM-dd HH:mm:ss}");

            return new LogSummary
            {
                TotalRecords = entries.Count,
                RecentActivityCount = recentActivityCount,
                ErrorCount = errorEntries.Count,
                CreatedCount = parallelQuery.Count(e => e.ChangeType.Equals("Created", StringComparison.OrdinalIgnoreCase)),
                ModifiedCount = parallelQuery.Count(e => e.ChangeType.Equals("Changed", StringComparison.OrdinalIgnoreCase)),
                DeletedCount = parallelQuery.Count(e => e.ChangeType.Equals("Deleted", StringComparison.OrdinalIgnoreCase)),
                RenamedCount = parallelQuery.Count(e => e.ChangeType.Equals("Renamed", StringComparison.OrdinalIgnoreCase)),
                LastUpdate = lastUpdate
            };
        }

        /// <summary>
        /// Diagnostic method to analyze the first few lines of the log file.
        /// This helps identify parsing issues and data format problems.
        /// </summary>
        public void DiagnoseLogFile()
        {
            if (!File.Exists(_logFilePath))
            {
                System.Diagnostics.Debug.WriteLine($"DiagnoseLogFile: Log file does not exist: {_logFilePath}");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"DiagnoseLogFile: Analyzing {_logFilePath}");
                
                var fileInfo = new FileInfo(_logFilePath);
                System.Diagnostics.Debug.WriteLine($"File size: {fileInfo.Length:N0} bytes");
                System.Diagnostics.Debug.WriteLine($"Last modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");

                // Read first 10 lines
                var lines = new List<string>();
                using (var fileStream = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fileStream))
                {
                    for (int i = 0; i < 10 && !reader.EndOfStream; i++)
                    {
                        lines.Add(reader.ReadLine());
                    }
                }

                System.Diagnostics.Debug.WriteLine($"First {lines.Count} lines:");
                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    System.Diagnostics.Debug.WriteLine($"Line {i + 1}: {line}");
                    
                    // Try to parse this line
                    var match = _logLinePattern.Match(line);
                    if (match.Success)
                    {
                        var logLevel = match.Groups[1].Value;
                        var timestampStr = match.Groups[2].Value;
                        var message = match.Groups[3].Value;
                        
                        System.Diagnostics.Debug.WriteLine($"  -> Parsed: LogLevel={logLevel}, Timestamp={timestampStr}");
                        System.Diagnostics.Debug.WriteLine($"  -> Message length: {message.Length}, Contains comma: {message.Contains(',')}");
                        
                        if (message.Contains(','))
                        {
                            var csvFields = ParseCsvLineOptimized(message);
                            System.Diagnostics.Debug.WriteLine($"  -> CSV fields count: {csvFields.Count}");
                            if (csvFields.Count > 10)
                            {
                                System.Diagnostics.Debug.WriteLine($"  -> Error field (index 10): '{csvFields[10]}'");
                            }
                            if (csvFields.Count > 1)
                            {
                                System.Diagnostics.Debug.WriteLine($"  -> ChangeType field (index 1): '{csvFields[1]}'");
                            }
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"  -> Does not match log pattern");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DiagnoseLogFile error: {ex.Message}");
            }
        }
    }
}