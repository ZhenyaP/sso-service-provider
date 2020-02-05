using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;

namespace ServiceProvider.API.Entities
{
    public class Timer : IDisposable
    {
        private static readonly object Lock = new object();
        private static readonly AsyncLock AsyncLock = new AsyncLock();
        private static readonly AsyncLocal<Timer> CurrentTimer = new AsyncLocal<Timer>();

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly Lazy<ConcurrentQueue<Timer>> _attachedTimers = new Lazy<ConcurrentQueue<Timer>>(() => new ConcurrentQueue<Timer>());
        private readonly Lazy<ConcurrentQueue<string>> _attachedMessages = new Lazy<ConcurrentQueue<string>>(() => new ConcurrentQueue<string>());
        private readonly string _description;
        private readonly TimeSpan? _threshold;
        private readonly Timer _previousCurrent;
        private static readonly ILogger Logger = Serilog.Log.ForContext<Timer>();

        private bool _isDisposed;
        private bool _suspendLogging;

        private Timer(Timer previousCurrent, string description = null, TimeSpan? threshold = null)
        {
            _previousCurrent = previousCurrent;
            _description = description;
            _threshold = threshold;
            _stopwatch.Start();
        }

        #region Get Current Timer

        private static Timer GetCurrent()
        {
            var timer = CurrentTimer.Value;
            if (timer == null)
            {
                CurrentTimer.Value = timer = new Timer(null);
            }
            return timer;
        }

        public static Timer Current
        {
            get
            {
                lock (Lock)
                {
                    return GetCurrent();
                }
            }
        }

        public static async Task<Timer> GetCurrentAsync()
        {
            using (await AsyncLock.Lock())
            {
                return GetCurrent();
            }
        }

        #endregion Get Current Timer

        #region Set Current Timer

        private static Timer SetCurrentTimer(string description, TimeSpan? threshold = null)
        {
            var currentTimer = CurrentTimer.Value;

            var timer = new Timer(currentTimer, description, threshold);

            CurrentTimer.Value = timer;

            currentTimer?._attachedTimers.Value.Enqueue(timer);

            return timer;
        }

        public static Timer SetCurrentTimerSync(string description, TimeSpan? threshold = null)
        {
            lock (Lock)
            {
                return SetCurrentTimer(description, threshold);
            }
        }

        public static async Task<Timer> SetCurrentTimerAsync(string description, TimeSpan? threshold = null)
        {
            using (await AsyncLock.Lock())
            {
                return SetCurrentTimer(description, threshold);
            }
        }

        #endregion Set Current Timer

        public void AttachMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _attachedMessages.Value.Enqueue(message);
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                _stopwatch.Stop();

                if (_attachedTimers.IsValueCreated)
                {
                    foreach (var attachedTimer in _attachedTimers.Value)
                    {
                        attachedTimer.Dispose();
                    }
                }

                if (!_suspendLogging && _threshold.HasValue && _stopwatch.Elapsed > _threshold.Value)
                {
                    Log();
                }

                if (_previousCurrent != null)
                {
                    CurrentTimer.Value = _previousCurrent;
                }
            }
        }

        #region Logging

        private JObject Message
        {
            get
            {
                Dispose();

                var message = new StringBuilder($"It took {_stopwatch.ElapsedMilliseconds} ms to execute {_description}.");

                if (_threshold.HasValue)
                {
                    message.Append($" Duration threshold is {_threshold.Value.TotalMilliseconds} ms.");
                }

                var messageObj = new JObject
                {
                    ["message"] = message.ToString(),
                };

                if (_attachedTimers.IsValueCreated && _attachedTimers.Value.Any())
                {
                    messageObj["attachedTimers"] = new JArray(_attachedTimers.Value.Select(t => t.Message));
                }

                if (_attachedMessages.IsValueCreated && _attachedMessages.Value.Any())
                {
                    messageObj["attachedMessages"] = new JArray(_attachedMessages.Value);
                }

                return messageObj;
            }
        }

        public void Log()
        {
            try
            {
                _suspendLogging = true;

                Dispose();

                if (_stopwatch.Elapsed < _threshold)
                {
                    Logger.Debug(Message.ToString());
                }
                else
                {
                    Logger.Warning(Message.ToString());
                }
            }
            finally
            {
                _suspendLogging = false;
            }
        }

        #endregion Logging
    }
}
