using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using TGP.Helpers;
using Experiment.Managers;

namespace Experiment.Analyzer.Core
{
    [System.Serializable]
    public class FullLogAnalyzer
    {
        public const string outputFolder = "AnalyzerOutput";
        public const string outputSuffix = "csv";

        private const string IOS_TMP_FILES = "._";

        public event EventHandler<EventArgs> onReadDataComplete;
        public event EventHandler<EventArgs<float>> onProgress;
        public event EventHandler<EventArgs<string>> onProcessFileBegin;
        public event EventHandler<LogMessageEventArgs> onLogMessage;
        
        public bool isCompleted { get; private set; }
        public string path { get; private set; }
        public string fillerDetailsFile;
        public string stimulusDetailsFile;
        public string probeDetailsFile;
        public string localizerDetailsFile;
        public string versionFile;
        public string sessionFile;
        public string subjectID;
        public List<string> fullDataFiles = new List<string>();
        public Dictionary<string, List<StimulusAnalysis>> stimulusAnalyses = new Dictionary<string, List<StimulusAnalysis>>();
        public Dictionary<string, List<TriggerAnalysis>> triggerAnalyses = new Dictionary<string, List<TriggerAnalysis>>();
        public Dictionary<string, List<CumulativeAnalysis>> cumulativeAnalyses = new Dictionary<string, List<CumulativeAnalysis>>();
        public ParseVersionFile version = new ParseVersionFile();
        public ParseSessionFile session = new ParseSessionFile();
        public ParseDetailsFile fillerDetails = new ParseDetailsFile();
        public ParseDetailsFile stimulusDetails = new ParseDetailsFile();
        public ParseDetailsFile probeDetails = new ParseDetailsFile();
        public ParseDetailsFile localizerDetails = new ParseDetailsFile();

        public string GetOutputFile(string worldLevel, FullLogEntryType analysisType)
        {
            return GetOutputFile(worldLevel, analysisType.ToString());
        }

        public string GetOutputFile(string worldLevel, string analysisType)
        {
            return string.Format("{0}/{1}_{2}_{3}.{4}", GetOutputFolder(), subjectID, worldLevel, analysisType, outputSuffix);
        }

        public string GetOutputFolder()
        {
            return "{0}/{1}"._Format(Path.Combine(Path.GetDirectoryName(probeDetailsFile), ".."), outputFolder);
        }

        public string error { get; private set; }
        public string errorShort { get; private set; }

        public FullLogAnalyzer() { }

        public FullLogAnalyzer(string path, bool includeTutorialLevels)
        {
            SelectDirectory(path, includeTutorialLevels);
        }

        public RuntimeConfig runtimeConfig;

        public void ParseAllFiles(RuntimeConfig runtimeConfig)
        {
            this.runtimeConfig = runtimeConfig;

            // == VERSION
            if (!File.Exists(versionFile))
                onLogMessage?.Invoke(this, new LogMessageEventArgs(string.Format("Version.txt was either missing or empty for subject {0}, resulting in potential malreading of data", subjectID), MessageType.Error));
            else
            {
                version = new ParseVersionFile(versionFile);
                onLogMessage?.Invoke(this, new LogMessageEventArgs(string.Format("Identified version {0} (dated :: {1}). Loaded settings {2}.", version.versionString, version.versionDate, version.versionSettings), MessageType.Informational));
            }

            // == SESSION
            if (!File.Exists(sessionFile))
                onLogMessage?.Invoke(this, new LogMessageEventArgs(string.Format("SessionInfo.txt was either missing or empty for subject {0}, resulting in potential issues", subjectID), MessageType.Error));
            else
            {
                session = new ParseSessionFile(sessionFile);
                onLogMessage?.Invoke(this, new LogMessageEventArgs(string.Format("Identified version {0} (dated :: {1}). Loaded settings {2}.", version.versionString, version.versionDate, version.versionSettings), MessageType.Informational));
            }

            // == FILLERS
            if (!File.Exists(fillerDetailsFile))
                onLogMessage?.Invoke(this, new LogMessageEventArgs(string.Format("FillerDetails.csv was either missing or empty for subject {0}, resulting in null seen/unseen reporting for Localizer Levels", subjectID), MessageType.Error));
            else
                fillerDetails = new ParseDetailsFile(fillerDetailsFile, version, DetailsEntry.Type.Filler);

            // == STIMULI
            if (!File.Exists(stimulusDetailsFile))
                onLogMessage?.Invoke(this, new LogMessageEventArgs(string.Format("StimulusDetails.csv was either missing or empty for subject {0}, resulting in null seen/unseen reporting for Localizer Levels", subjectID), MessageType.Error));
            else
                stimulusDetails = new ParseDetailsFile(stimulusDetailsFile, version, DetailsEntry.Type.Stimulus);

            // == PROBES
            if (!File.Exists(probeDetailsFile))
                onLogMessage?.Invoke(this, new LogMessageEventArgs(string.Format("ProbeDetails.csv was either missing or empty for subject {0}, resulting in null seen/unseen reporting for Localizer Levels", subjectID), MessageType.Error));
            else
                probeDetails = new ParseDetailsFile(probeDetailsFile, version, DetailsEntry.Type.InGameProbe);

            // == LOCALIZERS
            if (!File.Exists(localizerDetailsFile))
                onLogMessage?.Invoke(this, new LogMessageEventArgs(string.Format("LocalizerDetails.csv was either missing or empty for subject {0}, resulting in null seen/unseen reporting for Localizer Levels", subjectID), MessageType.Error));
            else
                localizerDetails = new ParseDetailsFile(localizerDetailsFile, version, DetailsEntry.Type.LocalizerResponse);


            ParseFullFile parseFileLevel;

#if UNITY_EDITOR
            bool levelDone = false;
            bool localDone = false;
            bool tutorDone = false;
#endif

            // Loop over all the files - levels
            for (int i = 0; i < fullDataFiles.Count; i++)
            {
                string filePath = fullDataFiles[i];

#if UNITY_EDITOR

                // if (!filePath.Contains("Level_L")) continue; // only do localizers

                if (runtimeConfig.onlyDoOneOfEach && (
                    (filePath.Contains("Level_0") && tutorDone) || 
                    ((filePath.Contains("Level_1") || filePath.Contains("Level_2") || filePath.Contains("Level_3") || filePath.Contains("Level_4")) && levelDone) ||
                    (filePath.Contains("Level_L") && localDone)))
                {
                    onLogMessage?.Invoke(this, new LogMessageEventArgs(string.Format("One-Of-Each mode enabled (EDITOR ONLY), skipping File"), MessageType.Informational));

                    continue;
                }
#endif
                // UnityEngine.Debug.Log(filePath);

                /*
                C:\Users\mrkon\Documents\WorkInProgress\__LOGS\CURRENT\S2541A\FullLogs\S2541A_FullLogLevel_0_1_7346.4004.csv
                 * [_0]: C:\Users\mrkon\Documents\WorkInProgress\
                    [1]: LOGS\CURRENT\S2541A\FullLogs\S2541A
                    [2]: FullLogLevel
                    [3]: 0                      <--- World
                    [4]: 1                      <--- Level
                    [5]: 7346.4004.csv
                */
                string[] filePath_Parts = filePath.Split(true, "_");
                // UnityEngine.Debug.Log(filePath_Parts.ToReadableString());

                int indexEnd = filePath_Parts.Length - 1;

                int offsetLevelFromEnd =
                    version.versionSettings >= VersionSettings.post200922 ? 2 : // From here on out we added _INTERRUPTED / _ ABORTED etc
                    filePath_Parts[indexEnd].ContainsInvariant("INTERRUPTED") || filePath_Parts[indexEnd].ContainsInvariant("ABORTED") || filePath_Parts[indexEnd].ContainsInvariant("COMPLETED")? 2 : 1;

                int offsetWorldFromLevel = 1;

                int indexLevel =  indexEnd - offsetLevelFromEnd;
                int indexWorld = indexLevel - offsetWorldFromLevel;

                string worldLevel = "{0}_{1}"._Format(filePath_Parts[indexWorld], filePath_Parts[indexLevel]);

                // UnityEngine.Debug.Log("LEVEL :: " + worldLevel);

                onProcessFileBegin?.Invoke(this, new EventArgs<string>(filePath));

                float extraWidth = session.screenWidthToHeight - 1;
                UnityEngine.Vector2 WIDTH_LIMITS = new UnityEngine.Vector2(
                    -extraWidth / 2, 1 + extraWidth / 2);

                onLogMessage.Invoke(this, new LogMessageEventArgs("Screen Size :: {0} | Width-To-Height :: {1}:1 | WIDTH_LIMITS :: {2}"._Format(
                    session.screenSize, session.screenWidthToHeight.ToString("#.00000000"), WIDTH_LIMITS.ToString("#.00000000")), MessageType.Informational));
                parseFileLevel = new ParseFullFile(filePath, subjectID, WIDTH_LIMITS, fillerDetails, stimulusDetails, probeDetails, localizerDetails, version, runtimeConfig.useParallelFor);

                // Get all the analyses of all the stimuli for that level (SORTED)
                List<StimulusAnalysis> stimulusAnalysesLevel = parseFileLevel.GetAllStimuliAnalyses();
                List<TriggerAnalysis> triggerAnalysesLevel = parseFileLevel.GetAllTriggerAnalyses();
                List<CumulativeAnalysis> cumulativeAnalysesLevel = CumulativeAnalysis.MergeAnalyses(stimulusAnalysesLevel, triggerAnalysesLevel);

                /*
                if (stimulusAnalyses.ContainsKey(worldLevel))
                    onLogMessage?.Invoke(this, new LogMessageEventArgs("Already processed a full log for {0} - skipping this one!"._Format(worldLevel), MessageType.Error)); // filePath
                else
                */
                {
                    if (!stimulusAnalyses.ContainsKey(worldLevel))
                        stimulusAnalyses.Add(worldLevel, new List<StimulusAnalysis>());
                    stimulusAnalyses[worldLevel].AddRange(stimulusAnalysesLevel);

                    if (!triggerAnalyses.ContainsKey(worldLevel))
                        triggerAnalyses.Add(worldLevel, new List<TriggerAnalysis>());
                    triggerAnalyses[worldLevel].AddRange(triggerAnalysesLevel);

                    if (!cumulativeAnalyses.ContainsKey(worldLevel))
                        cumulativeAnalyses.Add(worldLevel, new List<CumulativeAnalysis>());
                    cumulativeAnalyses[worldLevel].AddRange(cumulativeAnalysesLevel);
                }

                parseFileLevel.Clear();
                onProgress?.Invoke(this, new EventArgs<float>((float)i / (float)fullDataFiles.Count));

#if UNITY_EDITOR
                if (filePath.Contains("Level_0"))
                    tutorDone = true;
                else if (filePath.Contains("Level_1") || filePath.Contains("Level_2") || filePath.Contains("Level_3") || filePath.Contains("Level_4"))
                    levelDone = true;
                else if (filePath.Contains("Level_L"))
                    localDone = true;
#endif
            }

            foreach (List<StimulusAnalysis> sA in stimulusAnalyses.Values)
                sA.Sort();
            foreach (List<TriggerAnalysis> tA in triggerAnalyses.Values)
                tA.Sort();
            foreach (List<CumulativeAnalysis> cA in cumulativeAnalyses.Values)
                cA.Sort();

            isCompleted = true;
            onProgress?.Invoke(this, new EventArgs<float>(1f));
            onReadDataComplete?.Invoke(this, EventArgs.Empty);
        }

        public void ExportData()
        {
            WritingStrategy writingStrategy = GetWritingStrategy();

            List<string> worldLevels = new List<string>(cumulativeAnalyses.Keys);
            worldLevels.Sort();

            Dictionary<string, List<string>> segmentations = new Dictionary<string, List<string>>();

            foreach (string key in worldLevels)
            {
                string[] keyParts = key.Split(true, "_");
                int levelID_1Based = keyParts[1].ToInt();
                string worldID_1Based = keyParts[0] != "L" ? keyParts[0] :
                    ExperimentManagerSession.GetIDOfLocalizerBlock_String_ANALYZER(levelID_1Based - 1);

                // And reposition levelID based on whether we're in localizer or not
                levelID_1Based = keyParts[0] != "L" ? levelID_1Based :
                    (ExperimentManagerSession.GetIDOfLocalizerWithinBlock_0Based_ANALYZER(levelID_1Based - 1) + 1);

                string anchorKey = "";
#if UNITY_EDITOR
                // writingStrategy = WritingStrategy.PerLevel;
#endif
                switch (writingStrategy)
                {
                    case WritingStrategy.AllInOne:
                        break;
                    case WritingStrategy.PerLevel:
                        anchorKey = key;
                        break;
                    case WritingStrategy.PerTwoLevels:
                        // 2 entries per key, anchored to the 1-based odd level (ie. levels 1, 3 , ..)
                        anchorKey = "{0}_{1}"._Format(worldID_1Based, 
                            levelID_1Based % 2 == 1 ? levelID_1Based : (levelID_1Based - 1));
                        break;
                    case WritingStrategy.PerWorld:
                        // All entries anchored to their world
                        // But not localizers!
                        // UnityEngine.Debug.Log(key + " : " + worldID);
                        anchorKey = "{0}_X"._Format(worldID_1Based);
                        break;
                }

                if (!segmentations.ContainsKey(anchorKey))
                    segmentations.Add(anchorKey, new List<string>());

                segmentations[anchorKey].Add(key);
            }

            string stripPath = GetOutputFolder();

            if (Directory.Exists(stripPath))
            {
                DirectoryInfo di = new DirectoryInfo(stripPath);

                foreach (FileInfo file in di.GetFiles())
                    file.Delete();

                foreach (DirectoryInfo dir in di.GetDirectories())
                    dir.Delete(true);

                onLogMessage?.Invoke(this, new LogMessageEventArgs("Flushed " + stripPath, MessageType.Informational));
            }
            else
            {
                Directory.CreateDirectory(stripPath);
                onLogMessage?.Invoke(this, new LogMessageEventArgs("Created " + stripPath, MessageType.Informational));
            }

            foreach (KeyValuePair<string, List<string>> keys in segmentations)
            {
                ExportData(keys, FullLogEntryType.Stimulus);
                ExportData(keys, FullLogEntryType.Trigger);
                ExportData(keys, FullLogEntryType.Cumulative);
            }
        }

        private WritingStrategy GetWritingStrategy()
        {
            if (session != null)
                switch (session.system)
                {
                    case ModuleType.MEEG:
                    case ModuleType.MEEG_Preparation:
                        return runtimeConfig.writingStrategy_MEEG;
                    case ModuleType.ECOG:
                        return runtimeConfig.writingStrategy_ECOG;
                    case ModuleType.FMRI_Scanner:
                    case ModuleType.FMRI_Preparation:
                        return runtimeConfig.writingStrategy_FMRI;
                }

            return WritingStrategy.PerWorld;
        }

        private void ExportData(KeyValuePair<string, List<string>> segmentation, FullLogEntryType analysisType)
        {
            string segmentationName = segmentation.Key;
            List<string> worldLevels = segmentation.Value;

            string filePath = GetOutputFile(segmentationName, analysisType);
            string output = "";

            string separator = ";";
            string nullFieldValue = "_";

            for (int i = 0; i < worldLevels.Count; i++)
            {
                string worldLevel = worldLevels[i];
                string _output = "";

                // Skip Header for all next files
                bool shouldDoHeaders = output == "";

                if (analysisType == FullLogEntryType.Stimulus)
                    _output = stimulusAnalyses[worldLevel].ListToCsv(separator, nullFieldValue, shouldDoHeaders);
                else if (analysisType == FullLogEntryType.Trigger)
                    _output = triggerAnalyses[worldLevel].ListToCsv(separator, nullFieldValue, shouldDoHeaders);
                else if (analysisType == FullLogEntryType.Cumulative)
                    _output = cumulativeAnalyses[worldLevel].ListToCsv(separator, nullFieldValue, shouldDoHeaders);

                output += _output;
            }

            if (File.Exists(filePath))
                File.Delete(filePath);

            File.WriteAllText(filePath, output);
        }

        public bool SelectDirectory(string directory, bool includeTutorialLevels, bool includeGameLevels, bool includeReplayLevels)
        {
            error = "";

            if (!Directory.Exists(directory))
            {
                error = string.Format("Director doesn't exist: {0}", directory);
                errorShort = error;
                return false;
            }

            path = directory;

            // Find ProbeDetails
            fillerDetailsFile = null;
            stimulusDetailsFile = null;
            probeDetailsFile = null;
            localizerDetailsFile = null;
            versionFile = null;
            sessionFile = null;

            List<string> allFilesInFolder = GetAllFilesRecursively(path);
            for (int i = 0; i < allFilesInFolder.Count; i++)
            {
                if (allFilesInFolder[i].ContainsInvariant("FillerDetails"))
                {
                    fillerDetailsFile = allFilesInFolder[i];
                }
                else if (allFilesInFolder[i].ContainsInvariant("StimulusDetails"))
                {
                    stimulusDetailsFile = allFilesInFolder[i];
                }
                else if (allFilesInFolder[i].ContainsInvariant("ProbeDetails"))
                {
                    probeDetailsFile = allFilesInFolder[i];
                }
                else if (allFilesInFolder[i].ContainsInvariant("LocalizerDetails"))
                {
                    localizerDetailsFile = allFilesInFolder[i];
                }
                else if (allFilesInFolder[i].ContainsInvariant("version"))
                {
                    versionFile = allFilesInFolder[i];
                }
                else if (allFilesInFolder[i].ContainsInvariant("session"))
                {
                    sessionFile = allFilesInFolder[i];
                }
            }

            if (probeDetailsFile == null)
            {
                error = string.Format("Couldn't Locate ProbeDetails file within Folder: {0}", directory);
                errorShort = "ProbeDetails.csv";
                return false;
            }

            try
            {
                // Capture subject ID {TA203A}_ProbeDetails.csv
                this.subjectID = Path.GetFileName(probeDetailsFile).Split('_')[0];
            }
            catch
            {
                error = string.Format("Couldn't Get SubjectID from ProbeDetails file. Invalid pattern [SubjectID]_ProbeDetails.csv for file:\n{0}", probeDetailsFile);
                errorShort = "ProbeDetails ID";
                return false;
            }

            Dictionary<double, string> fullDataFiles_TS = new Dictionary<double, string>();

            for (int i = 0; i < allFilesInFolder.Count; i++)
            {
                string stripName = Path.GetFileName(allFilesInFolder[i]);
                if (!stripName.EndsWith(".csv") || !stripName.Contains("FullLog"))
                    continue;

                // Tutorial
                if (stripName.Contains("0_"))
                {
                    if (!includeTutorialLevels)
                        continue;
                }
                // Replay
                else if (stripName.Contains("R_"))
                {
                    if (!includeReplayLevels)
                        continue;
                }
                // Game
                else
                {
                    if (!includeGameLevels)
                        continue;
                }

                fullDataFiles.Add(allFilesInFolder[i]);
            }

            if (fullDataFiles.Count == 0)
            {
                error = string.Format("Couldn't find any FullLogs recursively for directory:\n{0}", path);
                errorShort = "0 FullLogs";
                return false;
            }

            isCompleted = false;
            return true;
        }

        public bool SelectDirectory(string v, object includeTutorialLevels)
        {
            throw new NotImplementedException();
        }

        static List<string> GetAllFilesRecursively(string directory, bool skipIOSTempFiles = true)
        {
            List<string> files = Directory.GetFiles(directory).ToList();
            if(skipIOSTempFiles)
            {
                List<string> iosTmpFiles = new List<string>();
                for (int i = 0; i < files.Count; i++)
                {
                    if (files[i].Contains(IOS_TMP_FILES))
                        iosTmpFiles.Add(files[i]);
                }
                files.RemoveRange(iosTmpFiles);
            }

            foreach (string d in Directory.GetDirectories(directory))
            {
                files.AddRange(GetAllFilesRecursively(d));
            }
            return files;
        }

        public void Clear()
        {
            stimulusAnalyses.Clear();
            triggerAnalyses.Clear();
            cumulativeAnalyses.Clear();
            probeDetails.Clear();
            fillerDetails.Clear();
            stimulusDetails.Clear();
        }

        public class RuntimeConfig
        {
            public bool useParallelFor;
            public bool onlyDoOneOfEach;

            public WritingStrategy writingStrategy_MEEG;
            public WritingStrategy writingStrategy_ECOG;
            public WritingStrategy writingStrategy_FMRI;

            public RuntimeConfig(bool useParallelFor, bool onlyDoOneOfEach, WritingStrategy writingStrategy_MEEG, WritingStrategy writingStrategy_ECOG, WritingStrategy writingStrategy_FMRI)
            {
                this.useParallelFor = useParallelFor;
                this.onlyDoOneOfEach = onlyDoOneOfEach;
                this.writingStrategy_MEEG = writingStrategy_MEEG;
                this.writingStrategy_ECOG = writingStrategy_ECOG;
                this.writingStrategy_FMRI = writingStrategy_FMRI;
            }
        }
    }

    public class LogMessageEventArgs : EventArgs
    {
        public string message;
        public MessageType type;

        public LogMessageEventArgs(string message, MessageType type)
        {
            this.message = message;
            this.type = type;
        }
    }

    public enum MessageType { Informational, Error, Success }
}
