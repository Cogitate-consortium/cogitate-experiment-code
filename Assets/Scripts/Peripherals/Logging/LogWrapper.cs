using Peripherals.Logging.Core;
using Helpers.Async;
using Helpers.Engine;
using TGP.Helpers;
using Peripherals.Logging.BackgroundWriting;
using Peripherals.Logging.Timestamping;
using System;
using System.Collections.Generic;

namespace Peripherals.Logging
{
    /// <summary>
    /// [SOS] <see cref="Initialize(string, Config)"/> first | 
    /// Writes to the correct folders, and is highly optimized to run on the background
    /// </summary>
    public static class LogWrapper
    {
        public static bool isWriting { get { return BackgroundWriter.isWriting; } }

        // This is the MASTER directory of logs (on the app level)
        private static string logsDirectory;
        private static Config config;

        private static bool config_doFullLogs;

        #region Public Methods

        public static void Initialize(string logsDirectory, Config config)
        {
            LogWrapper.logsDirectory = logsDirectory;
            LogWrapper.config = config;
            config_doFullLogs = config.doFullLogs;

            BackgroundWriter.onEntriesReadyToWrite -= BW_onEntriesReadyToWrite;
            BackgroundWriter.onEntriesReadyToWrite += BW_onEntriesReadyToWrite;
            BackgroundWriter.Initalize(config.backgroundWriterConfig);

            LogStamper.onLogsTimestamped -= LS_onLogsTimestamped;
            LogStamper.onLogsTimestamped += LS_onLogsTimestamped;
            LogStamper.Initialize();
        }

        public static void StartBackgroundDataWriting()
        {
            BackgroundWriter.StartBackgroundDataWriting();
        }

        /// <summary>
        /// [SOS] Do not forget to also call <see cref="WriteLeftoverBackgroundData(Action)"/> afterwards
        /// </summary>
        public static void StopBackgroundDataWriting()
        {
            BackgroundWriter.StopBackgroundDataWriting();
        }

        public static void WriteLeftoverBackgroundData(Action<ProgressReport> callback)
        {
            BackgroundWriter.WriteLeftoversToFile(progressReport =>
            {
                callback(progressReport);

                /*
                if (ApplicationLibrary.Config.Logging.debugLevel == LogDebuggingLevel.PerLevel)
                {
                    DoFormattedReports();
                    Debug_Helper.LogError(typeof(SubjectPerformanceReport), "Finished Cached Level data writing");
                }
                */
            });
        }

        /// <summary>
        /// [SOS] Always call <see cref="GetCurrentTimestamp_TS"/> to get the timestamp
        /// [SOS] First get timestamp, then invoke events / call other scripts, finally make the call
        /// </summary>
        /// [SOS] Keep timestamp of event as the FIRST argument, so that calling <see cref="TimeWrapper.GetCurrentTimestamp_TS"/> 
        /// gets calculated before any string operations that could delay it by a few ms
        public static void AddToLoggingQueue_AsTheyHappen_TS(TimeWrapper.Timestamp timestampOfEvent, 
            string sender, object data, string filepathInLogs, bool doDebug = false)
        {
            // AsyncThread.RequestRunOnNewThread(() =>
            {
                AddToLoggingQueue_TS(new LogEntry(timestampOfEvent, sender, data, filepathInLogs), doDebug);
            }// );
        }

        /// <summary>
        /// Will timestamp things on the next rendered frame. Use for things that will be rendered on screen.
        /// </summary>
        public static void AddToLoggingQueue_AtNextRenderedFrame_TS(string sender, object data, string filepathInLogs, bool doDebug = false)
        {
            //AsyncThread.RequestRunOnNewThread_OnNextFrame(() =>
            {
                AddToLoggingQueue_TS(new LogEntry(TimestampingType.NextRenderedFrame, sender, data, filepathInLogs), doDebug);
            }//);
        }

        /// <summary>
        /// Log to file, relative to <see cref="logsDirectory"/>
        /// </summary>
        public static void WriteToLogsAsync(string data, string filepathInLogs, bool append = true)
        {
            // Debug.Log("Logging Async to " + path);
            string filePath = logsDirectory + filepathInLogs;
            int maxNumAttempts = config.maxNumAttemptsPerBatch;
            WriteToFileAsync(data, filePath, maxNumAttempts, append);
        }

        /// <summary>
        /// Log Immediately to file
        /// </summary>
        public static void WriteToLogs_TS(string data, string filepathInLogs, bool append = true)
        {
            // [SOS] KEEP THREAD SAFE!
            /*
            char[] illegal = Path.GetInvalidPathChars();
            foreach (char c in illegal)
                if (path.Contains("" + c))
                    Debug.LogError("ILLEGAL! DE MUY LOCO PATHATES " + c + " IN " + path);
            */

            // if (data.Contains("LevelEnd")) Debug.LogError("YAY!");
            string filePath = logsDirectory + filepathInLogs;
            int maxNumAttempts = config.maxNumAttemptsPerBatch;
            WriteToFile_TS(data, filePath, maxNumAttempts, append);
        }

        /// <summary>
        /// Log Immediately to file, relative to <see cref="logsDirectory"/>
        /// </summary>
        public static void WriteToLogs(string path, byte[] bytes)
        {
            string filePath = logsDirectory + path;
            FileWrapper.WriteToFile(filePath, bytes);
        }

        public static bool LogFolderExists(string folderNameInLogs)
        {
            string path = logsDirectory + folderNameInLogs;
            return FileWrapper.FolderExists(path);
        }

        public static List<string> ReadAllLines(string fileNameInLogs)
        {
            string path = GetPath(fileNameInLogs);
            return FileWrapper.ReadAllLines(path);
        }

        public static string ReadFromFile(string fileNameInLogs)
        {
            string path = GetPath(fileNameInLogs);
            return FileWrapper.ReadFromFile(path);
        }

        public static string GetPath(string pathInLogs)
        {
            return logsDirectory + pathInLogs;
        }

        /// <summary>
        /// Returns null if folder name doesn't exist
        /// </summary>
        public static List<string> GetLogFolderContents(string folderNameInLogs, FilesFolders filesFolders, bool getFullPaths)
        {
            string path = logsDirectory + folderNameInLogs;

            return FileWrapper.GetFolderContents(path, filesFolders, getFullPaths);
        }

        public static void CopyFile(string source, string filepathInLogs, bool overwrite)
        {
            string destination = logsDirectory + filepathInLogs;
            FileWrapper.CopyFile(source, destination, overwrite);
        }

        public static void CopyFolder(string source, string folderPathInLogs, bool overwrite)
        {
            if (source == "")
                return;

            if (folderPathInLogs == "")
                return;

            string destination = logsDirectory + folderPathInLogs;
            FileWrapper.CopyFolder(source, destination, overwrite);
        }
        #endregion

        #region public
        private static void AddToLoggingQueue_TS(LogEntry logEntry, bool doDebug = false)
        {
            if (!config_doFullLogs) return;

            // We'll run it asap, and if it's meant to be stamped next frame, its -1, -1 signature will stamp it when it should
            AddToBackgroundWritingQueue_TS(logEntry);

            // Make sure we close this off for release
            if (EngineWrapper.Debug_IsDebugBuild && doDebug)
                Debug_Helper.LogWarning(typeof(LogWrapper),
                    "[{0} ({1}, {2})] {3}\n[SOS, Debug adds delays] Queued Log for Timestamping of type {4}"._Format(
                    logEntry.sender, TimeWrapper.currentFrameCycleID, TimeWrapper.currentTimestampMS.ToString("#"), logEntry.data, logEntry.timestampingType));

            /*
            // if (timestampingType == TimestampingType.NextRenderedFrame && sender == "TRIGGER_MANAGER_PHOTODIODE") Debug.Log("SPR [A] :: " + currentFrameCycleID);
            bool isMainThread = AsyncThread.isMainThread_TS;
            // We do high-accuracy logging BUT dont care when these are added to the files
            // Go for NEXT FRAME in case we are NextRenderedFrame (it's all the same if we aren't)
            AsyncThread.RunOnMainThread_NextFrameCycle_TS(() =>
            {
                // if (timestampingType == TimestampingType.NextRenderedFrame && sender == "TRIGGER_MANAGER_PHOTODIODE") Debug.LogWarning("SPR [B] :: " + currentFrameCycleID);

                // Respect overrides, but
                if (!overrideTimestamp.HasValue &&
                    // If we want to log on the next rendered frame
                    timestampingType == TimestampingType.NextRenderedFrame &&
                    // In other threads, this will get executed on the cycle AFTER the event that requested it, 
                    // [SOS] Note that in here we are ALWAYS in the main thread, so poll this earlier
                    !isMainThread)

                    // so we need to anchor to the last rendered frame
                    timestampToUse = new Timestamp(-1, -1);

                else
                    timestampToUse = overrideTimestamp;
            });*/
        }

        public static event EventHandler<LogEntry.StatusArgs> onLogEvent_Thread;

        /// <summary>
        /// [SOS] Consider using <see cref="LogWrapper.WriteToLogs_TS(string, string, bool)"/> instead
        /// </summary>
        public static void AddToBackgroundWritingQueue_TS(LogEntry logEntry)
        {
            onLogEvent_Thread?.Invoke(null, new LogEntry.StatusArgs(logEntry, LogEntry.State.Requested));
            
            // if (sender.ContainsInvariant("photodiode")) Debug.LogError((shouldLog ? "LOGGING" : "ABORTING") + data);
            if (!config.doFullLogs)
            {
#if UNITY_EDITOR
                // Debug_Helper.LogWarning(typeof(SubjectPerformanceReport),
                //   "Didn't log message from {0}:\n{1}"._Format(sender, data));
#endif
                return;
            }

            onLogEvent_Thread?.Invoke(null, new LogEntry.StatusArgs(logEntry, LogEntry.State.RequestedShouldLog));
            
            LogStamper.AddToTimpestampingQueue_TS(logEntry);
        }

        // These are ABSOLUTE file paths
        private static void WriteToFileAsync(string data, string filePath, int maxNumAttempts, bool append = true)
        {
            AsyncThread.RequestRunOnNewThread(() =>
            {
                WriteToFile_TS(data, filePath, maxNumAttempts, append);
            });
        }

        // These are ABSOLUTE file paths
        private static void WriteToFile_TS(string data, string filePath, int maxNumAttempts, bool append = true)
        {
            if (append)
                FileWrapper.AppendToFile_TS(filePath, data, maxNumAttempts);
            else
                FileWrapper.WriteToFile_TS(filePath, data);
        }
        #endregion

        #region Handlers
        private static void LS_onLogsTimestamped(object sender, EventArgs<List<LogEntry>> e)
        {
            /// Pass them onto the <see cref="BackgroundWriter"/>
            BackgroundWriter.AddToBackgroundWritingQueue(e.value);
        }

        private static void BW_onEntriesReadyToWrite(object sender, BackgroundWriter.EntriesReadyArgs e)
        {
            WriteToLogs_TS(e.entriesFormattedString, e.entriesFilePath);
        }
        #endregion

        #region Classes
        [Serializable]
        public class Config
        {
            public bool doFullLogs = false;
            public int maxNumAttemptsPerBatch = 30;
            public string FriendlyDateString = "yyyyMMdd-HH.mm.ss";

            public BackgroundWriter.Config backgroundWriterConfig;
        }

        /// <summary>
        /// Case sensitive
        /// </summary>
        public static void ReplaceInFilesOfFolder(string fullLogsPath, string ifContains, string wantedFileEnding, string replaceThis, string replaceWith, bool overwrite)
        {
            List<string> filePaths = GetLogFolderContents(fullLogsPath, FilesFolders.Files, true);

            foreach (string filePath in filePaths)
            {
                string fileEnding = filePath.Split('.').GetLast();

                if (filePath.Contains(ifContains) && filePath.Contains(replaceThis) && fileEnding.ContainsInvariant(wantedFileEnding))
                {
                    string newFilePath = filePath.Replace(replaceThis, replaceWith);

                    FileWrapper.MoveFile(filePath, newFilePath, overwrite);
                }
            }
        }
        #endregion
    }
}