using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FolderWatcherWindowsServiceAdmin.Models;

namespace FolderWatcherWindowsServiceAdmin.Services
{
    /// <summary>
    /// Service for parsing log4net log files and extracting CSV data.
    /// </summary>
    public class LogParser
    {
        private readonly string _logFilePath;
        private readonly Regex _logLinePattern;

        public LogParser(string logFilePath)
        {
            _logFilePath = logFilePath;
            // Pattern to match log4net format: LEVEL DATE TIME MESSAGE
            _logLinePattern = new Regex(@"^(INFO|ERROR|DEBUG|WARN)\s+(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2},\d{3})\s+\d+\s+(.*)$");
        }

        /// <summary>
        /// Parse all entries from the log file.
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
                using (var fileStream = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fileStream))
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing log file: {ex.Message}");
            }

            return entries.OrderByDescending(e => e.Timestamp).ToList();
        }

        /// <summary>
        /// Parse the last N entries from the log file efficiently.
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
                var lines = ReadLastLines(_logFilePath, count * 2); // Read extra to account for non-CSV lines
                
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
        /// Parse a single log line into a LogEntry.
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

            // Skip non-CSV messages (like service startup messages)
            if (!message.Contains(",") || message.StartsWith("Service"))
            {
                return null;
            }

            // Parse timestamp
            if (!DateTime.TryParseExact(timestampStr, "yyyy-MM-dd HH:mm:ss,fff", 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timestamp))
            {
                return null;
            }

            // Parse CSV message
            var csvFields = ParseCsvLine(message);
            if (csvFields.Count < 11) // Should have at least 11 fields
            {
                return null;
            }

            return new LogEntry
            {
                LogLevel = logLevel,
                Timestamp = DateTime.TryParseExact(csvFields[0], "yyyy-MM-dd HH:mm:ss.fff", 
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime csvTimestamp) 
                    ? csvTimestamp : timestamp,
                ChangeType = csvFields.Count > 1 ? csvFields[1] : string.Empty,
                FilePath = csvFields.Count > 2 ? csvFields[2] : string.Empty,
                ServiceUser = csvFields.Count > 3 ? csvFields[3] : string.Empty,
                ModifiedBy = csvFields.Count > 4 ? csvFields[4] : string.Empty,
                FileType = csvFields.Count > 5 ? csvFields[5] : string.Empty,
                FileSize = csvFields.Count > 6 ? csvFields[6] : string.Empty,
                Extension = csvFields.Count > 7 ? csvFields[7] : string.Empty,
                LastAccessed = csvFields.Count > 8 ? csvFields[8] : string.Empty,
                LastModified = csvFields.Count > 9 ? csvFields[9] : string.Empty,
                Error = csvFields.Count > 10 ? csvFields[10] : string.Empty,
                RawLine = line
            };
        }

        /// <summary>
        /// Parse CSV line handling quoted fields properly.
        /// </summary>
        private List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var currentField = string.Empty;
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
                        currentField += '"';
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
                    fields.Add(currentField);
                    currentField = string.Empty;
                    i++;
                }
                else
                {
                    currentField += ch;
                    i++;
                }
            }

            // Add the last field
            fields.Add(currentField);

            return fields;
        }

        /// <summary>
        /// Read the last N lines from a file efficiently.
        /// </summary>
        private List<string> ReadLastLines(string filePath, int lineCount)
        {
            var lines = new List<string>();

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (fileStream.Length == 0)
                {
                    return lines;
                }

                // Start from the end and work backwards
                var buffer = new byte[4096];
                var lineBuffer = new List<byte>();
                var position = fileStream.Length;
                var foundLines = new List<string>();

                while (position > 0 && foundLines.Count < lineCount)
                {
                    var bytesToRead = (int)Math.Min(buffer.Length, position);
                    position -= bytesToRead;
                    fileStream.Seek(position, SeekOrigin.Begin);
                    fileStream.Read(buffer, 0, bytesToRead);

                    // Process bytes in reverse order
                    for (int i = bytesToRead - 1; i >= 0; i--)
                    {
                        if (buffer[i] == '\n')
                        {
                            if (lineBuffer.Count > 0)
                            {
                                // Convert line buffer to string (reverse it first)
                                lineBuffer.Reverse();
                                var lineText = System.Text.Encoding.UTF8.GetString(lineBuffer.ToArray()).TrimEnd('\r');
                                if (!string.IsNullOrWhiteSpace(lineText))
                                {
                                    foundLines.Add(lineText);
                                }
                                lineBuffer.Clear();

                                if (foundLines.Count >= lineCount)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            lineBuffer.Add(buffer[i]);
                        }
                    }
                }

                // Handle any remaining content in the line buffer
                if (lineBuffer.Count > 0 && foundLines.Count < lineCount)
                {
                    lineBuffer.Reverse();
                    var lineText = System.Text.Encoding.UTF8.GetString(lineBuffer.ToArray()).TrimEnd('\r');
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
        /// Generate summary statistics from log entries.
        /// </summary>
        public LogSummary GenerateSummary(IEnumerable<LogEntry> entries)
        {
            var entriesList = entries.ToList();
            var now = DateTime.Now;
            var recentThreshold = now.AddMinutes(-30); // Last 30 minutes

            return new LogSummary
            {
                TotalRecords = entriesList.Count,
                RecentActivityCount = entriesList.Count(e => e.Timestamp >= recentThreshold),
                ErrorCount = entriesList.Count(e => !string.IsNullOrEmpty(e.Error) || e.LogLevel.Equals("ERROR")),
                CreatedCount = entriesList.Count(e => e.ChangeType.Equals("Created")),
                ModifiedCount = entriesList.Count(e => e.ChangeType.Equals("Changed")),
                DeletedCount = entriesList.Count(e => e.ChangeType.Equals("Deleted")),
                RenamedCount = entriesList.Count(e => e.ChangeType.Equals("Renamed")),
                LastUpdate = entriesList.Any() ? entriesList.Max(e => e.Timestamp) : (DateTime?)null
            };
        }
    }
}