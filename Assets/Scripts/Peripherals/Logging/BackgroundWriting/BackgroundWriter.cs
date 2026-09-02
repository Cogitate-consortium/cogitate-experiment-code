using Helpers.Async;
using Helpers.Engine;
using Peripherals.Logging.Core;
using Peripherals.Logging.Test.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TGP.Helpers;
using UnityEngine;

namespace Peripherals.Logging.BackgroundWriting
{
    /// <summary>
    /// [SOS] Run <see cref="Initialize"/> first |
    /// This logs data on the background with <see cref="StartBackgroundDataWriting"/> |
    /// When <see cref="StopBackgroundDataWriting"/> is called, call <see cref="WriteLeftoversToFile(Action)"/> to wrap up
    /// </summary>
    public static class BackgroundWriter
    {
        public static EventHandler<EntriesReadyArgs> onEntriesReadyToWrite;
        public static EventHandler<StatusUpdate> onStatusUpdate;

        public static bool isWriting
        {
            get
            {
                return isWritingBackground || isWritingLeftovers;
            }
        }

        public static bool isWritingBackground = false;
        public static bool isWritingLeftovers = false;
        public static Status currentStatus { get; private set; }

        private static List<LogEntry> pendingFormattingEntries = new List<LogEntry>();
        private static List<LogEntry> readyToWriteEntries = new List<LogEntry>();

        private static Coroutine backgroundWritingCR = null;
        private static Coroutine leftoverWritingCR = null;

        private static Config config;

        #region Public Accessors

        public static void Initalize(Config config)
        {
            BackgroundWriter.config = config;
            EngineWrapper.Initialize();
            TryUpdateStatus(Event.Initialized);
        }

        /// <summary>
        /// Will lock briefly
        /// </summary>
        public static void AddToBackgroundWritingQueue(IEnumerable<LogEntry> entries)
        {
            lock (pendingFormattingEntries)
            {
                pendingFormattingEntries.AddRange(entries);
            }
        }

        public static void StartBackgroundDataWriting()
        {
            StopBackgroundDataWriting();
            backgroundWritingCR = EngineWrapper.StartCoroutine(BackgroundWritingIE());
            TryUpdateStatus(Event.Started);
        }

        /// <summary>
        /// [SOS] Should be stopped 1. When starting, 2. At post-level screen, 3. At deinitalization
        /// </summary>
        public static void StopBackgroundDataWriting()
        {
            if (backgroundWritingCR == null)
            {
                return;
            }

            TryUpdateStatus(Event.Stopped);

            EngineWrapper.StopCoroutine(backgroundWritingCR);
            backgroundWritingCR = null;
        }

        /// <summary>
        /// Write any leftovers that were added with <see cref="AddToBackgroundWritingQueue(IEnumerable{LogEntry})"/> 
        /// but were not processed because of <see cref="StopBackgroundDataWriting"/>
        /// </summary>
        public static void WriteLeftoversToFile(Action<ProgressReport> callback)
        {
            if (currentStatus == Status.Unknown) // trackingLevel == null || 
            {
                if (config.doDebug)
                    Debug_Helper.LogError(typeof(BackgroundWriter), "Couldn't start Cached Level data writing. Initialize first!");
                return;
            }

            if (config.doDebug)
            {
                Debug_Helper.LogError(typeof(BackgroundWriter), "Starting Cached Level data writing");
            }

            if (leftoverWritingCR != null)
            {
                if (config.doDebug)
                    Debug_Helper.LogError(typeof(BackgroundWriter), "Had to interrupt Cached Level data writing");

                EngineWrapper.StopCoroutine(leftoverWritingCR);
            }

            leftoverWritingCR = EngineWrapper.StartCoroutine(WriteLeftoverDataIE(callback));
        }
        #endregion

        #region public Management
        public static event EventHandler<LogEntry.StatusArgs> onLogEvent;
        private static IEnumerator BackgroundWritingIE()
        {
            // bool showDebug = false;
            int dataPerChunk = config.backgroundDataWriteChunk;

            // [SOS] Do not trust the CURRENT active level (as we may be playing another level while writing a previous one)
            string filepathInLogs = "";
            StringBuilder allLogs = new StringBuilder();
            string DUMMY_STRING = "";

            while (true)
            {
                /*
                 *  [SOS] Temporary state added after null entry bug appeared. Probably items tried to be added while arrays were locked, and new entries appeared null
                 */

                MovePendingFormattingToReadyToWrite();

                int dataToProccess = dataPerChunk;
                if (readyToWriteEntries.Count < dataPerChunk)
                    dataToProccess = readyToWriteEntries.Count;

                //Debug.Log(string.Format("Cached bg_data:{0} | level_data:{1} | Processing:{2}", backgroundLevelData.Count, levelData.Count, dataToProccess));
                allLogs.Length = 0;
                allLogs.Capacity = 0;
                // Debug.LogError("Writing Background");
                isWritingBackground = true;

                AsyncThread.RequestRunOnNewThread(() =>
                {
                    try
                    {
                        lock (readyToWriteEntries)
                        {
                            readyToWriteEntries = readyToWriteEntries.CustomOrderBy(a => a.timestampMS);

                            // Sum up n chunks of logs
                            for (int i = 0; i < dataToProccess; i++)
                            {
                                if (readyToWriteEntries.Count > 0 && readyToWriteEntries[0] == null)
                                {
                                    AsyncThread.RunOnMainThread_ASAP_TS(() =>
                                    {
                                        Debug_Helper.LogException(
                                            typeof(BackgroundWriter),
                                            new Exception("Shouldn't happen - levelData had a null item!"));
                                    });
                                }
                                else
                                {
                                    ProcessReadyToWriteEntryToAllLogs(readyToWriteEntries[0], allLogs, DUMMY_STRING, out filepathInLogs);
                                }

                                readyToWriteEntries.RemoveAt(0);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Debug_Helper.LogError(typeof(BackgroundWriter), ex);
                    }
                    finally
                    {
                        ProcessAllLogs(allLogs, filepathInLogs, dataToProccess);

                        #region UNITY_EDITOR
                        // Debug.LogError("Sleeping write-thread"); System.Threading.Thread.Sleep(15000);
                        #endregion

                        // Debug.Log("Finished Writing Data");
                        AsyncThread.RunOnMainThread_ASAP_TS(() =>
                        {
                            isWritingBackground = false;
                            // Debug.LogError("NOT writing Background");
                        });
                    }
                });
                while (isWritingBackground)
                {
                    // Debug.Log("Writing Background");
                    // if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.F8)) showDebug = !showDebug;
                    yield return null;
                }
                yield return null;
            }
        }

        /// <summary>
        /// Save cached text to file - Called once after each level (when returning to level selection)
        /// </summary>
        private static IEnumerator WriteLeftoverDataIE(Action<ProgressReport> callback)
        {
            isWritingLeftovers = true;
            // Debug.LogError("REquested Leftover");

            while (isWritingBackground)
            {
                // Debug.Log("Still Writing Background");
                yield return null;
            }

            // Debug.LogError("Writing Leftover");

            int bytesPerLog = 64;   // Check it in DeepProfiling (GC.Alloc)
            int fileChunkSize = 1000000; // 1MB
            float timeStarted = TimeWrapper.realtimeSinceStartup_NotTS;

            // Cache everything
            StringBuilder allLogs = new StringBuilder();
            string DUMMY_STRING = "";

            // [SOS] Do before ready-to-write
            MovePendingFormattingToReadyToWrite();

            int totalLogs = readyToWriteEntries.Count;
            int chunksPerFile = (fileChunkSize / bytesPerLog);
            int totalProccessed = 0;

            int remainingChunksPerFile = chunksPerFile;

            string filepathInLogs = "";

            // Write all Data
            while (readyToWriteEntries.Count > 0)
            {
                // Check for remaining entries
                remainingChunksPerFile = chunksPerFile;
                if (readyToWriteEntries.Count < chunksPerFile)
                    remainingChunksPerFile = readyToWriteEntries.Count;

                // Sum up n chunks of logs
                List<int> missingIDs = new List<int>();

                for (int i = 0; i < remainingChunksPerFile; i++)
                {
                    if (readyToWriteEntries[i] == null)
                    {
                        missingIDs.Add(i);
                        continue;
                    }

                    ProcessReadyToWriteEntryToAllLogs(readyToWriteEntries[i], allLogs, DUMMY_STRING, out filepathInLogs);
                    totalProccessed++;
                }

                readyToWriteEntries.RemoveRange(0, remainingChunksPerFile);

                // Write logs without timestamp - so catch up with background (in game) logging
                LogWrapper.WriteToLogs_TS("APPENDING_MISSING_LOGS - {0} NULLS_PREVENTED\n{1}"._Format(missingIDs.Count, allLogs.ToString()), filepathInLogs);

                // Remove logs from List to decrease Memory overhead
                ProcessAllLogs(allLogs, filepathInLogs, remainingChunksPerFile);

                // Clear StringBuilder
                allLogs.Length = 0;
                allLogs.Capacity = 0;

                GC.Collect();

                // Update Progress
                callback?.Invoke(new ProgressReport(false, 
                    (float)totalProccessed / totalLogs, "Writing leftover data"));

                yield return new WaitForFixedUpdate();
            }

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            GC.Collect();
            
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Debug_Helper.LogWarning(typeof(BackgroundWriter), "Writing data to file took:" + 
                (TimeWrapper.realtimeSinceStartup_NotTS - timeStarted).ToString("0.0") + "s");
           
            leftoverWritingCR = null;
            // Update Progress
            callback?.Invoke(new ProgressReport(true, 1f, "Done"));

            isWritingLeftovers = false;
        }

        private static void MovePendingFormattingToReadyToWrite()
        {
            foreach (LogEntry logEntry in pendingFormattingEntries)
                onLogEvent?.Invoke(null, new LogEntry.StatusArgs(logEntry, LogEntry.State.ReadyToWrite));

            // Move temp data to cached array

            readyToWriteEntries.AddRange(pendingFormattingEntries);
            pendingFormattingEntries.Clear();
        }

        /// <summary>
        /// </summary>
        /// <param name="DUMMY_String">Just needed to avoid creating garbage</param>
        /// <returns></returns>
        private static void ProcessReadyToWriteEntryToAllLogs(LogEntry readyToWriteEntry, StringBuilder allLogs, string DUMMY_String, out string filePathInLogs)
        {
            onLogEvent?.Invoke(null, new LogEntry.StatusArgs(readyToWriteEntry, LogEntry.State.AddedToAllLogs));
            DUMMY_String = readyToWriteEntry.ToString();
            allLogs.AppendLine(DUMMY_String);

            filePathInLogs = readyToWriteEntry.filepathInLogs;
        }

        private static void ProcessAllLogs(StringBuilder allLogs, string filepathInLogs, int numLogs)
        {
            if (allLogs.Length > 0)
            {
                string allLogs_STR = allLogs.ToString();
                
                /*
                if (LoggingAnalyzer.config_doAnalytics)
                {
                    LoggingAnalyzer.logTotalData_AllLogs += String_Helper.CountOccurences(allLogs_STR, Environment.NewLine);
                }
                */

                onEntriesReadyToWrite?.Invoke(typeof(BackgroundWriter),
                    new EntriesReadyArgs(allLogs_STR, filepathInLogs));
            }
        }

        #endregion


        #region Status Update
        private static void TryUpdateStatus(Event transitionalEvent)
        {
            Status oldStatus = currentStatus;
            currentStatus = GetNewStatus(oldStatus, transitionalEvent);

            // We want to know if Unknown stuff are happening
            if (currentStatus == oldStatus && currentStatus != Status.Unknown) return;

            onStatusUpdate?.Invoke(typeof(BackgroundWriter),
                new StatusUpdate(oldStatus, currentStatus, transitionalEvent));
        }

        private static Status GetNewStatus(Status currentStatus, Event transitionalEvent)
        {
            if (currentStatus == Status.Unknown && transitionalEvent == Event.Initialized)
                return Status.Idle;

            if (currentStatus == Status.Idle && transitionalEvent == Event.Started)
                return Status.Writing;

            if (currentStatus == Status.Writing && transitionalEvent == Event.Stopped)
                return Status.Idle;

            return currentStatus;
        }

        #endregion

        #region Classes
        public enum Status { Unknown = 0, Idle = 1, Writing = 2 }
        public enum Event { Initialized = 0, Started = 1, Stopped = 2 }

        public class StatusUpdate : EventArgs
        {
            public Status oldStatus;
            public Status currentStatus;
            public Event transitionalEvent;

            public StatusUpdate(Status oldStatus, Status currentStatus, Event transitionalEvent)
            {
                this.oldStatus = oldStatus;
                this.currentStatus = currentStatus;
                this.transitionalEvent = transitionalEvent;
            }
        }

        [Serializable]
        public class Config
        {
            public bool doDebug = false;
            public int backgroundDataWriteChunk = 200;
        }

        public class EntriesReadyArgs : EventArgs
        {
            public string entriesFormattedString;
            public string entriesFilePath;

            public EntriesReadyArgs(string entriesFormattedString, string entriesFilePath)
            {
                this.entriesFormattedString = entriesFormattedString;
                this.entriesFilePath = entriesFilePath;
            }
        }
        #endregion
    }
}