using Helpers.Engine;
using Helpers.Misc;
using Peripherals.Logging;
using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Helpers
{
    /// <summary>
    /// NS_RENAME EXPERIMENT LOGGING
    /// </summary>
    public class ExperimentLogger : IDisposable
    {
        private static string subjectID;
        private static string subjectFolder;
        private Config config;

        [Serializable]
        public class Config
        {
            public bool doLogs { get { return logWrapper?.doFullLogs == true; } }
            public bool logHighAccuracyInputDump = true;
            public bool logSerialDump = true;
            public LogWrapper.Config logWrapper;
            public HeatmapGenerator.Config heatmap;
        }

        public class RuntimeConfig
        {
            public string subjectID;
            public string subjectFolder;
            public string logsDirectory;

            public RuntimeConfig(string subjectID, string subjectFolder, string logsDirectory)
            {
                this.subjectID = subjectID;
                this.subjectFolder = subjectFolder;
                this.logsDirectory = logsDirectory;
            }
        }

        public void Initialize(Config config, RuntimeConfig runtimeConfig)
        {
            this.config = config;
            subjectID = runtimeConfig.subjectID;
            subjectFolder = runtimeConfig.subjectFolder;
            LogWrapper.Initialize(runtimeConfig.logsDirectory, config.logWrapper);
        }


        #region Logging Bridge
        private static string filePathInLogs;

        public void StartBackgroundDataWriting(string lastTracking_levelName, string lastTracking_levelTimestamp)
        {
            filePathInLogs = GetFullLogLevelFilePath(lastTracking_levelName, lastTracking_levelTimestamp);
            LogWrapper.StartBackgroundDataWriting();
        }

        public void StopBackgroundDataWriting()
        {
            LogWrapper.StopBackgroundDataWriting();
        }

        public void WriteLeftoverBackgroundData(Action<ProgressReport> callback)
        {
            LogWrapper.WriteLeftoverBackgroundData(callback);
        }
        
        /// <summary>
        /// [SOS] Always call <see cref="GetCurrentTimestamp_TS"/> to get the timestamp
        /// [SOS] First get timestamp, then invoke events / call other scripts, finall make the call
        /// </summary>
        // [SOS] Keep timestamp of event as the FIRST argument, so that calling GetCurrentTimestamp_TS gets calculated before any string operations that could delay it by a few ms
        public void LogData_AsTheyHappen_TS(TimeWrapper.Timestamp timestampOfEvent, string sender, object data, bool doDebug = false)
        {
            LogWrapper.AddToLoggingQueue_AsTheyHappen_TS(timestampOfEvent, sender, data, filePathInLogs, doDebug);
        }

        public void LogData_AtNextRenderedFrame_TS(string sender, object data, bool doDebug = false)
        {
            LogWrapper.AddToLoggingQueue_AtNextRenderedFrame_TS(sender, data, filePathInLogs, doDebug);
        }
        
        private void LogEyeTrackingDetails<T>(JsonArray<T> eyeData, string filePathInLogs) where T : struct //, Action<bool> callback)
        {
            if (eyeData.array.Count == 0)
            {
                Debug_Helper.LogWarning(typeof(ExperimentLogger),
                    "No eye data to write! ({0})"._Format(filePathInLogs));
                return;
            }

            LogWrapper.WriteToLogs_TS(JsonUtility.ToJson(eyeData, true), filePathInLogs);
        }


        public void LogProbeDetail(string text, bool instant = false)
        {
            if (instant)
                LogWrapper.WriteToLogs_TS(text + Environment.NewLine, GetProbeDetailsFilePath());
            else
                LogWrapper.WriteToLogsAsync(text + Environment.NewLine, GetProbeDetailsFilePath());

            Debug_Helper.Log(typeof(ExperimentLogger), text);
        }

        public void LogLocalizerDetails(string text, bool instant = false)
        {
            if (instant)
                LogWrapper.WriteToLogs_TS(text + Environment.NewLine, GetLocalizerDetailsFilePath());
            else
                LogWrapper.WriteToLogsAsync(text + Environment.NewLine, GetLocalizerDetailsFilePath());

            Debug_Helper.Log(typeof(ExperimentLogger), text);
        }

        public void LogStimulusDetails(string text, bool instant = false)
        {
            if (instant)
                LogWrapper.WriteToLogs_TS(text + Environment.NewLine, GetStimulusDetailsFilePath());
            else
                LogWrapper.WriteToLogsAsync(text + Environment.NewLine, GetStimulusDetailsFilePath());

            Debug_Helper.Log(typeof(ExperimentLogger), text);
        }

        public void LogSerialDump(string dump)
        {
            if (!config.logSerialDump) return;
            LogWrapper.WriteToLogs_TS(dump + Environment.NewLine, GetSerialDumpFilePath());
        }

        public void LogHighAccuracyInputDump(string dump)
        {
            if (!config.logHighAccuracyInputDump) return;
            LogWrapper.WriteToLogs_TS(dump + Environment.NewLine, GetHighAccuracyInputDump());
        }

        public void LogFullLogDump(string dump)
        {
            LogWrapper.WriteToLogs_TS(dump + Environment.NewLine, GetFullLogDump());
        }

        public void LogFMRIDump(string dump)
        {
            LogWrapper.WriteToLogs_TS(dump + Environment.NewLine, GetFMRIDump());
        }

        public void LogEyeTrackingSyncDump(string dump)
        {
            LogWrapper.WriteToLogs_TS(dump + Environment.NewLine, GetEyeTrackingSyncDump());
        }

        public void LogEyeTrackingCommandDump(string dump)
        {
            LogWrapper.WriteToLogs_TS(dump + Environment.NewLine, GetEyeTrackingCommandDump());
        }

        public void LogEyeTrackingDebugDump(string dump)
        {
            LogWrapper.WriteToLogs_TS(dump + Environment.NewLine, GetEyeTrackingDebugDump());
        }

        public void LogLoggerDump(string dump)
        {
            LogWrapper.WriteToLogs_TS(dump + Environment.NewLine, GetLoggerDump());
        }

        public void LogFillerDetails(string text, bool instant = false)
        {
            if (instant)
                LogWrapper.WriteToLogs_TS(text + Environment.NewLine, GetFillerDetailsFilePath());
            else
                LogWrapper.WriteToLogsAsync(text + Environment.NewLine, GetFillerDetailsFilePath());

            Debug_Helper.Log(typeof(ExperimentLogger), text);
        }

        public void LogSubjectDetails(string text)
        {
            LogWrapper.WriteToLogsAsync(text + Environment.NewLine, GetSubjectDetailsFilePath());
            Debug_Helper.Log(typeof(ExperimentLogger), text);
        }

        public void LogApplicationVersion(string version)
        {
            string filePath = GetApplicationVersionFilePath();
            LogWrapper.WriteToLogsAsync(version, filePath, false);
        }

        public void LogModule(string module)
        {
            string filePath = GetModuleFilePath();
            LogWrapper.WriteToLogsAsync(module, filePath, false);
        }

        public void CopyConfigFile(string originalConfigFilepath)
        {
            LogWrapper.CopyFile(originalConfigFilepath, GetConfigFilePath(), true);
            Debug_Helper.Log(typeof(ExperimentLogger), "Copied config file");
        }

        public void CopyProbeSummaryFile_Game(string originalProbeSummaryFilepath_Game)
        {
            LogWrapper.CopyFile(originalProbeSummaryFilepath_Game, GetProbeSummaryFilePath_Game(), true);
            Debug_Helper.Log(typeof(ExperimentLogger), "Copied probe summary file (Game)");
        }

        public void CopyProbeSummaryFile_Replay(string originalProbeSummaryFilepath_Replay)
        {
            LogWrapper.CopyFile(originalProbeSummaryFilepath_Replay, GetProbeSummaryFilePath_Replay(), true);
            Debug_Helper.Log(typeof(ExperimentLogger), "Copied probe summary file (Replay)");
        }

        public void CopySequenceFolder(string originalSequenceFolderpath)
        {
            LogWrapper.CopyFolder(originalSequenceFolderpath, GetSequenceFolderPath(), true);
            Debug_Helper.Log(typeof(ExperimentLogger), "Copied Sequence folder");
        }

        public void LogIntoSequenceFolder(string fileName, string content)
        {
            LogWrapper.WriteToLogs_TS(content, "{0}{1}"._Format(GetSequenceFolderPath(), fileName));
        }

        public void CopyProgressFolder(string originalProgressFolderpath)
        {
            LogWrapper.CopyFolder(originalProgressFolderpath, GetProgressFolderPath(), true);
            Debug_Helper.Log(typeof(ExperimentLogger), "Copied Progress folder");
        }

        public void CopyFullLogsFolder(string originalFullLogsFolderpath)
        {
            LogWrapper.CopyFolder(originalFullLogsFolderpath, GetFullLogsFolderPath(), true);
            Debug_Helper.Log(typeof(ExperimentLogger), "Copied Full Logs folder");
        }

        public void CopyExtraLogsFolder(string originalExtraLogsFolderpath)
        {
            LogWrapper.CopyFolder(originalExtraLogsFolderpath, GetExtraLogsFolderPath(), true);
            Debug_Helper.Log(typeof(ExperimentLogger), "Copied Extra Logs folder");
        }

        public void CopySummariesFolder(string originalSummariesFolderpath)
        {
            LogWrapper.CopyFolder(originalSummariesFolderpath, GetProbeSummaryFilePath_Replay(), true);
            Debug_Helper.Log(typeof(ExperimentLogger), "Copied Extra Logs folder");
        }

        public void LogStimulusQueue(string fileName, string queue)
        {
            string fileNameInLogs = "{0}{1}"._Format(GetAnalysisFolderPath(), fileName);
            LogWrapper.WriteToLogs_TS(queue, fileNameInLogs, false);
        }

        public void LogStimulusQueueAnalysis(string fileName, string analysis)
        {
            string fileNameInLogs = "{0}{1}"._Format(GetAnalysisFolderPath(), fileName);
            LogWrapper.WriteToLogs_TS(analysis, fileNameInLogs, false);
        }

        public void LogIntoAnalysisFolder(string fileName, string content)
        {
            string fileNameInLogs = "{0}{1}"._Format(GetAnalysisFolderPath(), fileName);
            LogWrapper.WriteToLogs_TS(content, fileNameInLogs);
        }

        public void LogExperimentSessionAnalytics(string logsToWrite)
        {
            string path = string.Format("{0}/{1}_analytics.txt", subjectFolder, subjectID);
            LogWrapper.WriteToLogsAsync(logsToWrite, path, false);
        }

        public void LogOpponentScores(string completedLevelsCSV)
        {
            string fileNameInLogs = "{0}{1}"._Format(GetProgressFolderPath(), "OpponentScores.csv");
            LogWrapper.WriteToLogs_TS(completedLevelsCSV, fileNameInLogs, false);
        }

        public List<string> LoadOpponentScores()
        {
            string fileNameInLogs = "{0}{1}"._Format(GetProgressFolderPath(), "OpponentScores.csv");
            return LogWrapper.ReadAllLines(fileNameInLogs);
        }

        public void LogCompletedLevels(string completedLevelsCSV)
        {
            string fileNameInLogs = "{0}{1}"._Format(GetProgressFolderPath(), "CompletedLevels.csv");
            LogWrapper.WriteToLogs_TS(completedLevelsCSV, fileNameInLogs, false);
        }

        public List<string> LoadCompletedLevels()
        {
            string fileNameInLogs = "{0}{1}"._Format(GetProgressFolderPath(), "CompletedLevels.csv");
            return LogWrapper.ReadAllLines(fileNameInLogs);
        }

        public void LogReplayLevel(string levelName, string replayJSON)
        {
            string fileNameInLogs = "{0}{1}"._Format(GetProgressFolderPath(), "Replays/{0}.json"._Format(levelName));
            LogWrapper.WriteToLogs_TS(replayJSON, fileNameInLogs, false);
        }

        public Dictionary<string, string> LoadReplayLevels()
        {
            string folderNameInLogs = "{0}{1}"._Format(GetProgressFolderPath(), "Replays/");
            List<string> replayLevelNames = LogWrapper.GetLogFolderContents(folderNameInLogs, FilesFolders.Files, false);

            Dictionary<string, string> replayLevelsJSON = new Dictionary<string, string>();

            if (replayLevelNames != null)
                for (int i = 0; i < replayLevelNames.Count; i++)
                {
                    string levelName_withSuffix = replayLevelNames[i]; // this includes the .json suffix
                    string levelName_noSuffix = levelName_withSuffix.Split('.').GetFirst();
                    string levelJson = LogWrapper.ReadFromFile("{0}/{1}"._Format(folderNameInLogs, levelName_withSuffix));

                    replayLevelsJSON.Add(levelName_noSuffix, levelJson);
                }

            return replayLevelsJSON;
        }

        #endregion


        #region Logging Eye Tracking

        public static bool isWriting { get { return LogWrapper.isWriting; } }

        public void LogEyeTrackingDetails<T>(JsonArray<T> eyeData, string levelName, string type, string timestampStarted) where T : struct //, Action<bool> callback)
        {
            LogEyeTrackingDetails<T>(eyeData, GetFullLogEyeTrackerFilePath(levelName, type, timestampStarted));
        }
        
        /// <summary>
        /// Returns NULL if failed
        /// </summary>
        public void LogHeatmap<T>(JsonArray<T> eyeData, bool isGaze, string levelName, string timestampStarted, Action<Sprite> callback) where T : struct
        {
            // Nothing to do!
            if (eyeData.array.Count == 0)
            {
                this.LogWarning("No heatmap data to write! ({0})"._Format(isGaze ? "GAZE" : "SACADA"));
                callback?.Invoke(null);
                return;
            }

            string heatmapFileName = GetFullLogHeatmapFilePath(levelName, isGaze ? "Gaze" : "Saccade", timestampStarted);
            // Debug.Log(heatmapFileName);

            Vector2Int size = new Vector2Int(Screen.width, Screen.height);
            Color colorBackgroundFile = config.heatmap.colorBackgroundFile.ToColor();
            Color colorBackgroundUI = config.heatmap.colorBackgroundUI.ToColor();
            Color colorGazesUI = config.heatmap.colorGazesUI.ToColor();
            Color colorSacadasUI = config.heatmap.colorSacadasUI.ToColor();
            Color colorGazesFile = config.heatmap.colorGazesFile.ToColor();
            Color colorSacadasFile = config.heatmap.colorSacadasFile.ToColor();

            if (isGaze)
            {
                // Create background-less texture (for writting to file)

                HeatmapGenerator.GenerateAndLogHeatmap(size, (eyeData as JsonArray<Vector2>).array, colorBackgroundFile, colorGazesFile, 50, heatmapFileName, true, (texture_File) =>
                {
                    // Leave filename blank to NOT write to file
                    HeatmapGenerator.GenerateAndLogHeatmap(size, (eyeData as JsonArray<Vector2>).array, colorBackgroundUI, colorGazesUI, 50, "", true, (texture_UI) =>
                    {
                        // Debug.Log("GAZE :: " + latestLevelHeatmap_Gaze_Backgrounded_ForUI.texture.width);
                        callback?.Invoke(Sprite.Create(texture_UI, new Rect(0, 0, size.x, size.y), Vector2.one / 2, 100));
                    });
                });
            }
            // Sacada
            else
            {
                // Create background-less texture (for writting to file)
                HeatmapGenerator.GenerateAndLogHeatmap(size, (eyeData as JsonArray<Vector4>).array, colorBackgroundFile, colorSacadasFile, 50, heatmapFileName, true, (texture_File) =>
                {
                    // Leave filename blank to NOT write to file
                    HeatmapGenerator.GenerateAndLogHeatmap(size, (eyeData as JsonArray<Vector4>).array, colorBackgroundUI, colorSacadasUI, 50, "", true, (texture_UI) =>
                    {
                        callback?.Invoke(Sprite.Create(texture_UI, new Rect(0, 0, size.x, size.y), Vector2.one / 2, 100));
                    });
                });
            }
        }

        #endregion


        #region PATHS

        public string GetEyeTrackerLogsFolderPathInLogs()
        {
            return GetFullLogsFolderPath();
        }

        public string GetFullLogsFolderPath()
        {
            return string.Format("{0}/FullLogs/", subjectFolder);
        }

#if UNITY_EDITOR
        public static string overrideAnalysisSubfolder = "";
#endif

        private string GetAnalysisFolderPath()
        {
            string folderPath = string.Format("{0}/Queues/", subjectFolder);

#if UNITY_EDITOR
            if (!overrideAnalysisSubfolder.IsNullOrEmpty())
                folderPath += overrideAnalysisSubfolder + "/";
#endif

            return folderPath;
        }

        public void RenameFullLogs(string levelName, bool aborted)
        {
            string fullLogsPath = GetFullLogsFolderPath();
            string patternToContain = "_{0}_"._Format(levelName);
            LogWrapper.ReplaceInFilesOfFolder(fullLogsPath, patternToContain, "",
                FullLogState.INTERRUPTED.ToString(),
                (aborted ? FullLogState.ABORTED : FullLogState.COMPLETED).ToString(), false);
        }

        private enum FullLogState { COMPLETED = 0, INTERRUPTED = -1, ABORTED = -2 }

        // ALL of these are w.r.t. the log folder
        private string GetFullLogFilePath(string type, string name, string timestamp, string fileEnding)
        {
            return string.Format("{0}{1}_{2}_{3}_{4}_{5}.{6}", /// <see cref="FullLogState.INTERRUPTED"/> is the default state of the logs
                GetFullLogsFolderPath(), subjectID, type, name, timestamp, FullLogState.INTERRUPTED, fileEnding);
        }

        private string GetFullLogLevelFilePath(string levelName, string timestampStarted)
        {
            if (levelName.IsNullOrEmpty()) Debug.LogError("WTF");
            if (timestampStarted.IsNullOrEmpty()) Debug.LogError("WTF");
            return GetFullLogFilePath("FullLogLevel", levelName, timestampStarted, "csv");
        }

        private string GetFullLogEyeTrackerFilePath(string levelName, string type, string timestampStarted)
        {
            if (levelName.IsNullOrEmpty()) Debug.LogError("WTF");
            if (timestampStarted.IsNullOrEmpty()) Debug.LogError("WTF");
            return GetFullLogFilePath("EyeTracker", "{0}_{1}"._Format(levelName, type), timestampStarted, "json");
        }

        private string GetFullLogHeatmapFilePath(string levelName, string type, string timestampStarted)
        {
            if (levelName.IsNullOrEmpty()) Debug.LogError("WTF");
            if (timestampStarted.IsNullOrEmpty()) Debug.LogError("WTF");

            return GetFullLogFilePath("Heatmap", "{0}_{1}"._Format(levelName, type), timestampStarted, "png");
        }

        private string GetProbeDetailsFilePath()
        {
            return string.Format("{0}/Details/{1}_ProbeDetails.csv", subjectFolder, subjectID);
        }

        private string GetLocalizerDetailsFilePath()
        {
            return string.Format("{0}/Details/{1}_LocalizerDetails.csv", subjectFolder, subjectID);
        }

        private string GetStimulusDetailsFilePath()
        {
            return string.Format("{0}/Details/{1}_StimulusDetails.csv", subjectFolder, subjectID);
        }

        private string GetFillerDetailsFilePath()
        {
            return string.Format("{0}/Details/{1}_FillerDetails.csv", subjectFolder, subjectID);
        }

        private string GetSubjectDetailsFilePath()
        {
            return string.Format("{0}/Details/{1}_SubjectDetails.csv", subjectFolder, subjectID);
        }

        private string GetSerialDumpFilePath()
        {
            return string.Format("{0}/ExtraLogs/{1}_Serial.txt", subjectFolder, subjectID);
        }

        private string GetHighAccuracyInputDump()
        {
            return string.Format("{0}/ExtraLogs/{1}_HighAccuInput.txt", subjectFolder, subjectID);
        }

        private string GetFullLogDump()
        {
            return string.Format("{0}/ExtraLogs/{1}_FullLog.txt", subjectFolder, subjectID);
        }

        private string GetFMRIDump()
        {
            return string.Format("{0}/ExtraLogs/{1}_FMRI.txt", subjectFolder, subjectID);
        }

        private string GetEyeTrackingSyncDump()
        {
            return string.Format("{0}/ExtraLogs/{1}_EyeTrackingSync.csv", subjectFolder, subjectID);
        }

        private string GetEyeTrackingCommandDump()
        {
            return string.Format("{0}/ExtraLogs/{1}_EyeTrackingCommand.txt", subjectFolder, subjectID);
        }

        private string GetEyeTrackingDebugDump()
        {
            return string.Format("{0}/ExtraLogs/{1}_EyeTrackingDebug.txt", subjectFolder, subjectID);
        }

        private string GetLoggerDump()
        {
            return string.Format("{0}/ExtraLogs/{1}_Logger.txt", subjectFolder, subjectID);
        }

        private string GetSessionInfoFilePath()
        {
            return string.Format("{0}/{1}_SessionInfo.txt", subjectFolder, subjectID);
        }

        private string GetApplicationVersionFilePath()
        {
            return string.Format("{0}/{1}_version.txt", subjectFolder, subjectID);
        }

        private string GetModuleFilePath()
        {
            return GetModuleFilePath(subjectFolder, subjectID);
        }

        public static string GetModuleFilePath(string subjectFolder, string subjectID)
        {
            return string.Format("{0}/{1}_module.txt", subjectFolder, subjectID);
        }

        private string GetConfigFilePath()
        {
            // return string.Format("{0}/{1}_config.json", subjectFolder, subjectID);
            return string.Format("{0}/config.json", subjectFolder);
        }

        private string GetSequenceFolderPath()
        {
            return string.Format("{0}/Sequence/", subjectFolder);
        }

        private string GetAudioTriggerFolderPath()
        {
            return string.Format("{0}/AudioTriggers/", subjectFolder);
        }

        private string GetProgressFolderPath()
        {
            return string.Format("{0}/Progress/", subjectFolder);
        }

        private string GetExtraLogsFolderPath()
        {
            return string.Format("{0}/ExtraLogs/", subjectFolder);
        }

        private string GetSummariesFolderPath()
        {
            return string.Format("{0}/Summaries/", subjectFolder);
        }

        private string GetProbeSummaryFilePath_Game()
        {
            return string.Format("{0}/{1}_ProbeSummary_Game.json", GetSummariesFolderPath(), subjectID);
        }

        private string GetProbeSummaryFilePath_Replay()
        {
            return string.Format("{0}/{1}_ProbeSummary_Replay.json", GetSummariesFolderPath(), subjectID);
        }

        internal void LogSessionInfo(string info, bool debug = false)
        {
            LogWrapper.WriteToLogsAsync(info + Environment.NewLine, GetSessionInfoFilePath());
            if (EngineWrapper.Debug_IsDebugBuild && debug)
                Debug.Log(info);
        }

        internal void LogProbeSummary_Game(string summary)
        {
            LogWrapper.WriteToLogs_TS(summary, GetProbeSummaryFilePath_Game(), false);
        }

        internal void LogProbeSummary_Replay(string summary)
        {
            LogWrapper.WriteToLogs_TS(summary, GetProbeSummaryFilePath_Replay(), false);
        }

        public string GetProbeSummary_Game()
        {
            return LogWrapper.ReadFromFile(GetProbeSummaryFilePath_Game());
        }

        public string GetProbeSummary_Replay()
        {
            return LogWrapper.ReadFromFile(GetProbeSummaryFilePath_Replay());
        }

        public void Dispose()
        {

        }

        /*
        public static string FormatTimestampDT(DateTime timestamp)
        {
            TimeSpan dt = timestamp - probeSummary.sessionStartTime;
            return dt.TotalMilliseconds.ToString();
            // return string.Format("{0}:{1}.{2}", dt.Minutes, dt.Seconds, dt.Milliseconds);
        }
        */

        #endregion

    }
}