using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datadog.Trace.Logging;

namespace Datadog.Trace
{
    internal static class DatadogSpanStagingArea
    {
        private static readonly ILog Log = LogProvider.GetLogger(typeof(DatadogSpanStagingArea));
        private static readonly ConcurrentDictionary<Guid, Action> WakeUpTasks = new ConcurrentDictionary<Guid, Action>();
        private static readonly ConcurrentQueue<FlushTask> FlushTaskQueue = new ConcurrentQueue<FlushTask>();

        public static int FlushTaskCount { get; private set; }

        public static DateTime LastFlushRequest { get; private set; }

        public static TimeSpan TimeSinceLastFlush => Now.Subtract(LastFlushRequest);

        private static DateTime Now => DateTime.UtcNow;

        public static void RegisterForWakeup(Action wakeupCall)
        {
            WakeUpTasks.TryAdd(Guid.NewGuid(), wakeupCall);
        }

        public static void QueueSpanForFlush(Span span)
        {
            long approximateByteCount = 10_000;
            FlushTaskQueue.Enqueue(new FlushTask
            {
                AttemptsRemaining = 2,
                QueuedAt = DateTime.UtcNow,
                ByteCountApproximation = approximateByteCount,
                Span = span
            });

            IncrementTaskCount();
        }

        public static async Task Flush(int maxTasks, Func<IEnumerable<FlushTask>, Task<IEnumerable<FlushTask>>> toilet)
        {
            LastFlushRequest = Now;

            var readyToFlush = new List<FlushTask>();
            var tasksLeftBeforeLimit = maxTasks;

            while (tasksLeftBeforeLimit-- > 0 && FlushTaskQueue.TryDequeue(out FlushTask task))
            {
                readyToFlush.Add(task);
                FlushTaskCount--;
            }

            if (!readyToFlush.Any())
            {
                return;
            }

            var backup = await toilet(readyToFlush);

            foreach (var flushTask in backup)
            {
                if (--flushTask.AttemptsRemaining == 0)
                {
                    // TODO: log
                    continue;
                }

                flushTask.LastAttemptAt = LastFlushRequest;

                Retry(flushTask);
            }

            if (FlushTaskCount > maxTasks)
            {
                // We should go again, can't have the queue piling up
                await Flush(maxTasks, toilet).ConfigureAwait(false);
            }
        }

        public static void Retry(FlushTask task)
        {
            FlushTaskQueue.Enqueue(task);
            IncrementTaskCount();
        }

        private static void IncrementTaskCount()
        {
            if (FlushTaskCount++ == 0)
            {
                var keysToRemove = new HashSet<Guid>();
                // This is the first item in the queue
                // Let's notify any subscribers that it's time to wake up
                foreach (var wakeUpTaskKey in WakeUpTasks.Keys)
                {
                    try
                    {
                        WakeUpTasks[wakeUpTaskKey]();
                    }
                    catch (Exception ex)
                    {
                        // Don't wake the neighbors
                        Log.Error(ex, "Wake up call threw an exception.");
                        keysToRemove.Add(wakeUpTaskKey);
                    }
                }

                foreach (var guid in keysToRemove)
                {
                    WakeUpTasks.TryRemove(guid, out _);
                }
            }
        }

        public class FlushTask
        {
            public DateTime QueuedAt { get; set; }

            public DateTime? LastAttemptAt { get; set; }

            public int AttemptsRemaining { get; set; }

            public long ByteCountApproximation { get; set; }

            public Span Span { get; set; }
        }
    }
}
