using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

/// <summary>
/// Handles debouncing of file system events to prevent duplicate processing.
/// </summary>
namespace FolderWatcherWindowsService
{
    internal class EventDebouncer : IDisposable
    {
        private readonly int _delayMs;
        private readonly Dictionary<string, Timer> _timers;
        private readonly object _lock = new object();

        public EventDebouncer(int delayMs)
        {
            _delayMs = delayMs;
            _timers = new Dictionary<string, Timer>();
        }

        public void Debounce(FileSystemEventArgs e, Action<FileSystemEventArgs> action)
        {
            lock (_lock)
            {
                string key = $"{e.ChangeType}|{e.FullPath}";

                if (_timers.TryGetValue(key, out Timer existingTimer))
                {
                    existingTimer.Change(_delayMs, Timeout.Infinite);
                }
                else
                {
                    var timer = new Timer(
                        callback =>
                        {
                            action(e);
                            RemoveTimer(key);
                        },
                        null,
                        _delayMs,
                        Timeout.Infinite
                    );

                    _timers[key] = timer;
                }
            }
        }

        private void RemoveTimer(string key)
        {
            lock (_lock)
            {
                if (_timers.TryGetValue(key, out Timer timer))
                {
                    timer.Dispose();
                    _timers.Remove(key);
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var timer in _timers.Values)
                {
                    timer?.Dispose();
                }
                _timers.Clear();
            }
        }
    }
}