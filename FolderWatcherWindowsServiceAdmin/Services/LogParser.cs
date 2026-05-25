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
    /// SIMPLIFIED: Uses a more flexible regex that works with any log4net pattern.
    /// </summary>
    public class LogParser
    {
        private readonly string _logFilePath;
        private readonly Regex _logLinePattern;
        private const int DefaultBufferSize = 65536; // 64KB buffer for better I/O performance
        private const int LargeFileThreshold = 10 * 1024 * 1024; // 10MB
        private static int _debugEntryCount = 0; // Debug counter for logging first few entries

        // Expected CSV field count (based on FileSystemLogEntry model)
        private const int ExpectedCsvFieldCount = 11;

        public LogParser(string logFilePath)
        {
            _logFilePath = logFilePath;

            // SIMPLIFIED: Match any line that contains a log level and has commas (CSV data)
            // This is more flexible and works with various log4net formats
            _logLinePattern = new Regex(
                @"^.*?(INFO|ERROR|DEBUG|WARN)\s+(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}[,\.]\d{3}).*?(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}[.,]\d{3},.*)$",
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
                System.Diagnostics.Debug.WriteLine($"Log file does not exist: {_logFilePath}");
                return entries;
            }

            try
            {
                var fileInfo = new FileInfo(_logFilePath);
                System.Diagnostics.Debug.WriteLine($"Parsing log file: {_logFilePath} ({fileInfo.Length:N0} bytes)");

                // Use different strategies based on file size
                if (fileInfo.Length > LargeFileThreshold)
                {
                    System.Diagnostics.Debug.WriteLine("Using large file parsing strategy (parallel)");
                    return ParseLargeFile();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Using small/medium file parsing strategy (sequential)");
                    return ParseSmallToMediumFile();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing log file: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return entries;
            }
        }

        /// <summary>
        /// Optimized parsing for small to medium files (under 10MB).
        /// </summary>
        private List<LogEntry> ParseSmallToMediumFile()
        {
            var entries = new List<LogEntry>();
            var lineNumber = 0;
            var skippedLines = 0;
            var parsedLines = 0;

            using (var fileStream = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, DefaultBufferSize))
            using (var reader = new StreamReader(fileStream, Encoding.UTF8, true, DefaultBufferSize))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;

                    var entry = ParseLogLine(line, lineNumber);
                    if (entry != null)
                    {
                        entries.Add(entry);
                        parsedLines++;
                    }
                    else
                    {
                        skippedLines++;
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"Parsing complete: {lineNumber} total lines, {parsedLines} parsed, {skippedLines} skipped");
            return entries.OrderByDescending(e => e.Timestamp).ToList();
        }

        /// <summary>
        /// Optimized parsing for large files (over 10MB) using parallel processing.
        /// </summary>
        private List<LogEntry> ParseLargeFile()
        {
            var allLines = new List<string>();
            var lineNumber = 0;

            // Read all lines efficiently
            using (var fileStream = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, DefaultBufferSize))
            using (var reader = new StreamReader(fileStream, Encoding.UTF8, true, DefaultBufferSize))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    allLines.Add(line);
                    lineNumber++;
                }
            }

            System.Diagnostics.Debug.WriteLine($"Read {lineNumber} lines, starting parallel parsing...");

            // Parse lines in parallel for better performance on large files
            var entries = allLines
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .Select((line, index) => ParseLogLine(line, index + 1))
                .Where(entry => entry != null)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"Parallel parsing complete: {entries.Count} entries parsed");
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
                // Read extra lines to account for non-CSV lines
                var lines = ReadLastLinesOptimized(_logFilePath, count * 3);

                System.Diagnostics.Debug.WriteLine($"Read {lines.Count} lines for last {count} entries");

                var lineNumber = 0;
                foreach (var line in lines)
                {
                    lineNumber++;
                    var entry = ParseLogLine(line, lineNumber);
                    if (entry != null)
                    {
                        entries.Add(entry);
                    }

                    if (entries.Count >= count)
                    {
                        break;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Parsed {entries.Count} entries from last {lines.Count} lines");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing last entries: {ex.Message}");
            }

            return entries.OrderByDescending(e => e.Timestamp).ToList();
        }

        /// <summary>
        /// Parse a single log line into a LogEntry with SIMPLIFIED logic.
        /// FIXED: Uses simple string search instead of complex regex.
        /// </summary>
        private LogEntry ParseLogLine(string line, int lineNumber = 0)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            // Skip CSV header line
            if (line.TrimStart().StartsWith("Timestamp,ChangeType", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Quick check: must contain a log level and commas (CSV data)
            var hasLogLevel = line.Contains("INFO") || line.Contains("ERROR") || line.Contains("DEBUG") || line.Contains("WARN");
            if (!hasLogLevel || !line.Contains(','))
            {
                return null;
            }

            // Extract log level (first occurrence)
            string logLevel = "INFO";
            if (line.Contains("ERROR")) logLevel = "ERROR";
            else if (line.Contains("WARN")) logLevel = "WARN";
            else if (line.Contains("DEBUG")) logLevel = "DEBUG";

            // Find the CSV data - it starts with a timestamp in format YYYY-MM-DD HH:MM:SS
            var csvStartPattern = new Regex(@"(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}[.,]\d{3}),");
            var csvMatch = csvStartPattern.Match(line);

            if (!csvMatch.Success)
            {
                return null;
            }

            // Extract the CSV portion (from the timestamp match to end of line)
            var csvData = line.Substring(csvMatch.Index);

            // Also extract the log4net timestamp (earlier in the line, before CSV data)
            DateTime logTimestamp = DateTime.Now;
            var logTimestampPattern = new Regex(@"(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}[.,]\d{3})");
            var logTimestampMatch = logTimestampPattern.Match(line, 0, csvMatch.Index);
            if (logTimestampMatch.Success)
            {
                TryParseTimestamp(logTimestampMatch.Groups[1].Value, out logTimestamp);
            }

            // Parse CSV data
            List<string> csvFields;
            try
            {
                csvFields = ParseCsvLineOptimized(csvData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Line {lineNumber}: CSV parsing error: {ex.Message}");
                return new LogEntry
                {
                    LogLevel = "ERROR",
                    Timestamp = logTimestamp,
                    ChangeType = "ParseError",
                    FileType = "Error",
                    Error = $"CSV Parse Error: {ex.Message}",
                    RawLine = line
                };
            }

            // Validate field count
            if (csvFields.Count < ExpectedCsvFieldCount)
            {
                var debugCount = System.Threading.Interlocked.Increment(ref _debugEntryCount);
                if (debugCount <= 10)
                {
                    System.Diagnostics.Debug.WriteLine($"Line {lineNumber}: Insufficient CSV fields. Expected {ExpectedCsvFieldCount}, got {csvFields.Count}");
                    System.Diagnostics.Debug.WriteLine($"  CSV Data: {csvData.Substring(0, Math.Min(200, csvData.Length))}");
                }

                return CreatePartialEntry(logLevel, logTimestamp, csvFields, line,
                    $"Incomplete CSV: Expected {ExpectedCsvFieldCount} fields, got {csvFields.Count}");
            }

            // Parse CSV timestamp (first field)
            DateTime csvTimestamp = logTimestamp;
            if (!string.IsNullOrWhiteSpace(csvFields[0]))
            {
                if (!TryParseTimestamp(csvFields[0], out csvTimestamp))
                {
                    csvTimestamp = logTimestamp;
                }
            }

            // Debug: Log first few successful parses
            var debugCounter = System.Threading.Interlocked.Increment(ref _debugEntryCount);
            if (debugCounter <= 3)
            {
                System.Diagnostics.Debug.WriteLine($"✓ ParseLogLine #{debugCounter} (Line {lineNumber}): LogLevel={logLevel}, CSV fields={csvFields.Count}");
                System.Diagnostics.Debug.WriteLine($"  Timestamp: {csvTimestamp:yyyy-MM-dd HH:mm:ss.fff}");
                System.Diagnostics.Debug.WriteLine($"  ChangeType: '{csvFields[1]}'");
                System.Diagnostics.Debug.WriteLine($"  FilePath: '{csvFields[2]}'");
            }

            // Create log entry with all fields
            return new LogEntry
            {
                LogLevel = logLevel,
                Timestamp = csvTimestamp,
                ChangeType = GetField(csvFields, 1),
                FilePath = GetField(csvFields, 2),
                ServiceUser = GetField(csvFields, 3),
                ModifiedBy = GetField(csvFields, 4),
                FileType = GetField(csvFields, 5),
                FileSize = GetField(csvFields, 6),
                Extension = GetField(csvFields, 7),
                LastAccessed = GetField(csvFields, 8),
                LastModified = GetField(csvFields, 9),
                Error = GetField(csvFields, 10).Trim(),
                RawLine = line
            };
        }

        /// <summary>
        /// Safely get a CSV field by index with bounds checking.
        /// </summary>
        private string GetField(List<string> fields, int index)
        {
            return index < fields.Count ? fields[index] : string.Empty;
        }

        /// <summary>
        /// Create a partial log entry when CSV parsing fails or is incomplete.
        /// </summary>
        private LogEntry CreatePartialEntry(string logLevel, DateTime timestamp, List<string> csvFields, string rawLine, string errorMessage)
        {
            return new LogEntry
            {
                LogLevel = logLevel,
                Timestamp = timestamp,
                ChangeType = GetField(csvFields, 1),
                FilePath = GetField(csvFields, 2),
                ServiceUser = GetField(csvFields, 3),
                ModifiedBy = GetField(csvFields, 4),
                FileType = "Error",
                FileSize = GetField(csvFields, 6),
                Extension = GetField(csvFields, 7),
                LastAccessed = GetField(csvFields, 8),
                LastModified = GetField(csvFields, 9),
                Error = errorMessage,
                RawLine = rawLine
            };
        }

        /// <summary>
        /// Optimized timestamp parsing with multiple format support.
        /// </summary>
        private bool TryParseTimestamp(string timestampStr, out DateTime timestamp)
        {
            if (string.IsNullOrWhiteSpace(timestampStr))
            {
                timestamp = DateTime.MinValue;
                return false;
            }

            // Normalize timestamp string
            var normalizedStr = timestampStr.Replace(',', '.');

            // Try common formats
            string[] formats = new[]
            {
                "yyyy-MM-dd HH:mm:ss.fff",
                "yyyy-MM-dd HH:mm:ss,fff",
                "yyyy-MM-dd HH:mm:ss",
                "M/d/yyyy H:mm",
                "M/d/yyyy H:mm:ss",
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(normalizedStr, format,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp))
                {
                    return true;
                }
            }

            // Fallback
            if (DateTime.TryParse(timestampStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp))
            {
                return true;
            }

            timestamp = DateTime.MinValue;
            return false;
        }

        /// <summary>
        /// Optimized CSV line parser using StringBuilder for better memory efficiency.
        /// Handles quoted fields, escaped quotes, and embedded commas correctly.
        /// </summary>
        private List<string> ParseCsvLineOptimized(string line)
        {
            var fields = new List<string>();
            var fieldBuilder = new StringBuilder(256);
            var inQuotes = false;
            var i = 0;

            while (i < line.Length)
            {
                var ch = line[i];

                if (ch == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        fieldBuilder.Append('"');
                        i += 2;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                        i++;
                    }
                }
                else if (ch == ',' && !inQuotes)
                {
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

            fields.Add(fieldBuilder.ToString());
            return fields;
        }

        /// <summary>
        /// Optimized reverse file reading with larger buffers and better memory management.
        /// </summary>
        private List<string> ReadLastLinesOptimized(string filePath, int lineCount)
        {
            const int bufferSize = 16384;
            var lines = new List<string>();

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize))
            {
                if (fileStream.Length == 0)
                {
                    return lines;
                }

                var buffer = new byte[bufferSize];
                var lineBytes = new List<byte>(1024);
                var position = fileStream.Length;
                var foundLines = new List<string>();

                while (position > 0 && foundLines.Count < lineCount)
                {
                    var bytesToRead = (int)Math.Min(buffer.Length, position);
                    position -= bytesToRead;
                    fileStream.Seek(position, SeekOrigin.Begin);
                    var bytesRead = fileStream.Read(buffer, 0, bytesToRead);

                    for (int i = bytesRead - 1; i >= 0; i--)
                    {
                        if (buffer[i] == '\n')
                        {
                            if (lineBytes.Count > 0)
                            {
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

                if (lineBytes.Count > 0 && foundLines.Count < lineCount)
                {
                    lineBytes.Reverse();
                    var lineText = Encoding.UTF8.GetString(lineBytes.ToArray()).TrimEnd('\r');
                    if (!string.IsNullOrWhiteSpace(lineText))
                    {
                        foundLines.Add(lineText);
                    }
                }

                foundLines.Reverse();
                return foundLines;
            }
        }

        public async Task<List<LogEntry>> ParseLogFileAsync()
        {
            return await Task.Run(() => ParseLogFile()).ConfigureAwait(false);
        }

        public async Task<List<LogEntry>> ParseLastEntriesAsync(int count)
        {
            return await Task.Run(() => ParseLastEntries(count)).ConfigureAwait(false);
        }

        public LogSummary GenerateSummary(IEnumerable<LogEntry> entries)
        {
            var entriesList = entries as List<LogEntry> ?? entries.ToList();

            if (!entriesList.Any())
            {
                return new LogSummary();
            }

            var now = DateTime.Now;
            var recentThreshold = now.AddMinutes(-30);

            if (entriesList.Count > 1000)
            {
                return GenerateSummaryParallel(entriesList, recentThreshold, now);
            }

            var unknownCount = entriesList.Count(e => e.FileType.Equals("Unknown", StringComparison.OrdinalIgnoreCase));

            var recentActivityCount = entriesList.Count(e => e.Timestamp >= recentThreshold);
            var lastUpdate = entriesList.Max(e => e.Timestamp);

            return new LogSummary
            {
                TotalRecords = entriesList.Count,
                RecentActivityCount = recentActivityCount,
                UnknownCount = unknownCount,
                CreatedCount = entriesList.Count(e => e.ChangeType.Equals("Created", StringComparison.OrdinalIgnoreCase)),
                ModifiedCount = entriesList.Count(e => e.ChangeType.Equals("Changed", StringComparison.OrdinalIgnoreCase)),
                DeletedCount = entriesList.Count(e => e.ChangeType.Equals("Deleted", StringComparison.OrdinalIgnoreCase)),
                RenamedCount = entriesList.Count(e => e.ChangeType.Equals("Renamed", StringComparison.OrdinalIgnoreCase)),
                LastUpdate = lastUpdate
            };
        }

        private LogSummary GenerateSummaryParallel(List<LogEntry> entries, DateTime recentThreshold, DateTime now)
        {
            var parallelQuery = entries.AsParallel();

            var errorCount = parallelQuery.Count(e => e.FileType.Equals("Unknown", StringComparison.OrdinalIgnoreCase));

            var recentActivityCount = parallelQuery.Count(e => e.Timestamp >= recentThreshold);
            var lastUpdate = parallelQuery.Max(e => e.Timestamp);

            return new LogSummary
            {
                TotalRecords = entries.Count,
                RecentActivityCount = recentActivityCount,
                UnknownCount = errorCount,
                CreatedCount = parallelQuery.Count(e => e.ChangeType.Equals("Created", StringComparison.OrdinalIgnoreCase)),
                ModifiedCount = parallelQuery.Count(e => e.ChangeType.Equals("Changed", StringComparison.OrdinalIgnoreCase)),
                DeletedCount = parallelQuery.Count(e => e.ChangeType.Equals("Deleted", StringComparison.OrdinalIgnoreCase)),
                RenamedCount = parallelQuery.Count(e => e.ChangeType.Equals("Renamed", StringComparison.OrdinalIgnoreCase)),
                LastUpdate = lastUpdate
            };
        }

        public void DiagnoseLogFile()
        {
            if (!File.Exists(_logFilePath))
            {
                System.Diagnostics.Debug.WriteLine($"DiagnoseLogFile: Log file does not exist: {_logFilePath}");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"\n{new string('=', 80)}");
                System.Diagnostics.Debug.WriteLine($"DiagnoseLogFile: {_logFilePath}");
                System.Diagnostics.Debug.WriteLine(new string('=', 80));

                var fileInfo = new FileInfo(_logFilePath);
                System.Diagnostics.Debug.WriteLine($"File size: {fileInfo.Length:N0} bytes");
                System.Diagnostics.Debug.WriteLine($"Last modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
                System.Diagnostics.Debug.WriteLine(new string('-', 80));

                var lines = new List<string>();
                using (var fs = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fs))
                {
                    for (int i = 0; i < 10 && !reader.EndOfStream; i++)
                    {
                        lines.Add(reader.ReadLine());
                    }
                }

                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    System.Diagnostics.Debug.WriteLine($"\n[Line {i + 1}]");
                    System.Diagnostics.Debug.WriteLine($"Length: {line.Length}");
                    System.Diagnostics.Debug.WriteLine($"Content: {line.Substring(0, Math.Min(150, line.Length))}...");

                    var entry = ParseLogLine(line, i + 1);
                    if (entry != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"✓ PARSED: LogLevel={entry.LogLevel}, ChangeType={entry.ChangeType}, FileType={entry.FileType}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"✗ SKIPPED");
                    }
                }

                System.Diagnostics.Debug.WriteLine(new string('=', 80) + "\n");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DiagnoseLogFile error: {ex.Message}");
            }
        }
    }
}