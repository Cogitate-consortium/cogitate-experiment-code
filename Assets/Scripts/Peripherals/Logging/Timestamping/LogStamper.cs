using Helpers.Engine;
using Peripherals.Logging.Core;
using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Peripherals.Logging.Timestamping
{
    /// <summary>
    /// [SOS] <see cref="Initialize"/> first
    /// </summary>
    public static class LogStamper
    {
        public static EventHandler<EventArgs<List<LogEntry>>> onLogsTimestamped;

        /// TODO This is a bit too many buffers
        private static readonly List<LogEntry> pendingTimestampEntries = new List<LogEntry>();
        private static readonly List<LogEntry> timestampedEntries = new List<LogEntry>();

        private static bool isUpdatingTimestamps = false;

        public static void Initialize()
        {
            TimeWrapper.onFrameRendered -= TW_onFrameRendered;
            TimeWrapper.onFrameRendered += TW_onFrameRendered;
            TimeWrapper.Initialize();
        }

        public static event EventHandler<LogEntry.StatusArgs> onLogStatus_Thread;

        /// <summary>
        /// Will lock briefly
        /// </summary>
        public static void AddToTimpestampingQueue_TS(params LogEntry[] entries)
        {
            lock (pendingTimestampEntries)
            {
                pendingTimestampEntries.AddRange(entries);

                foreach (LogEntry lE in entries)
                    onLogStatus_Thread?.Invoke(null, new LogEntry.StatusArgs(lE, LogEntry.State.AddedToPending));
            }
        }

        /// <summary>
        /// [SOS] Will briefly lock <see cref="pendingTimestampEntries"/>
        /// </summary>
        public static void OnFrameRendered_UpdateTimestamps(TimeWrapper.FrameInfo e)
        {
            if (isUpdatingTimestamps)
            {
                Debug.LogError("Already writting pending timestamps");
            }
            else
            {
                UnityEngine.Profiling.Profiler.BeginSample("Anchoring Timestamps");

                isUpdatingTimestamps = true;

                /*
                    *  1 - Level Data
                    */

                // Mark all pending LevelData
                // Copy to PendingWritting
                lock (pendingTimestampEntries)
                {
                    // Cache all pending data
                    timestampedEntries.Clear();
                    timestampedEntries.AddRange(pendingTimestampEntries);

                    // Clear list in use
                    pendingTimestampEntries.Clear();
                }

                // Update cached pending Level Data
                for (int i = 0; i < timestampedEntries.Count; i++)
                {
                    if (!timestampedEntries[i].preserveTimestamp) // This is our cue to stamp it
                    {
                        if (timestampedEntries[i].frameCycleID != TimeWrapper.currentFrameCycleID) // That was the frame cycle th
                        {
                            // This is very problematic!(will appear with timestamp MS as -1, which is a good way to catch it)
                            // Debug.LogError("A");
                        }

                        timestampedEntries[i].frameCycleID = TimeWrapper.currentFrameCycleID - 1; // That's the cycle that led to the last rendered frame
                        timestampedEntries[i].timestampMS = TimeWrapper.lastRenderedFrame_TimeOfRenderMS; // That's the last rendered frame's timestamp
                        timestampedEntries[i].timestampMS_NoPauses = TimeWrapper.lastRenderedFrame_TimeOfRender_SessionPlayTime_MS_NoPauses; // That's the last rendered frame's timestamp
                    }

                    onLogStatus_Thread?.Invoke(null, new LogEntry.StatusArgs(timestampedEntries[i], LogEntry.State.AddedToWrite));
                }

                // Prevent erasure
                onLogsTimestamped(null, new List<LogEntry>(timestampedEntries));

                timestampedEntries.Clear();

                isUpdatingTimestamps = false;

                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        private static void TW_onFrameRendered(object sender, TimeWrapper.FrameInfo e)
        {
            OnFrameRendered_UpdateTimestamps(e);
        }
    }
}