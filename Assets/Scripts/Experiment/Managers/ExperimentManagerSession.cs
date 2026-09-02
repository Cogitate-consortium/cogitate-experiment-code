// NS_REMOVE | Shouldn't connect to the CORE of anything 
using Game.Core;
// NS_REMOVE | Shouldn't connect to the CORE of anything 
using Experiment.Task.Core;
// NS_REMOVE | Config
using ExperimentLibrary;

// NS_DEBATABLE | Isn't UI a bit too far? Exp / Game is ok
using Game.Managers.SessionManagers.UI;

// NS_DEBATABLE | A bit too DEEP
using Peripherals.EyeTracking;

// NS_DEBATABLE | Needs segmentation, then maybe it will make sense (ie. separate for EXP / GAME)
using Experiment.Stimulus;
// NS_DEBATABLE | Needs segmentation, then maybe it will make sense (ie. separate for EXP / GAME)
using Experiment.Task;
// NS_DEBATABLE | Needs segmentation, then maybe it will make sense (ie. separate for EXP / GAME)
using Experiment.Background;

using System;
using System.Text;
using TGP.Helpers;
using UnityEngine;
using Helpers.Engine;
using System.Collections;
using Peripherals.Photodiode;
using Peripherals.Serial;
using Peripherals.LPT;
using Peripherals.UserInput;
using Peripherals.UserInput.HighAccu;
using Peripherals.Audio;
using Game.Entities.Interactables;
using Game.Entities.Player.Core;
using Game.Entities.Player;
using Game.Systems.Replay;
using Game.Managers.LevelManagers;
using Game.Managers.SessionManagers;
using Game.Systems.Adaptive;
using Experiment.Analytics;
using Experiment.Managers.Core;
using Experiment.Subject;
using Experiment.Task.UI;
using Experiment.Helpers.Game;
using Experiment.Triggers;
using Experiment.Helpers;
using Peripherals.Logging.Core;
using System.Collections.Generic;
using Peripherals.Logging;
using Peripherals.Logging.Test;
using Peripherals.Logging.Test.Core;
using Helpers.Async;

namespace Experiment.Managers
{
    /// <summary>
    /// NS_RENAME Tracking
    /// It formats the things and sends them for logging. Logger does not handle formatting.
    /// SEGMENT THIS !!FilePaths, Helpers (Evaluation)
    /// </summary>
    public class ExperimentManagerSession : IDisposable
    {
        #region Weird

        public LevelConfig GetLevelConfig(LevelConfig level)
        {
            // Move on to the next scene!
            if (level.GetTaskType() == TaskType.TaskRelevant)
            {
                LevelConfig recordedLevel = GetRecordedReplayLevel(level);
                if (recordedLevel != null)
                {
                    // Find recorded Level and get KoregraphyIndex
                    level.koregraphyIndex = recordedLevel.koregraphyIndex;
                    level.gameType = recordedLevel.GetGameType();
                }
                else
                {
                    this.LogWarning(string.Format("World {0} is recorded. Loading default values", level.levelName));
                    level.gameType = GameType.Runner_2D_Blue;
                    level.koregraphyIndex = 0;
                }
            }

            return level;
        }
        #endregion


        #region THESE NEED TO BECOME PRIVATE AND HANDLED AS EVENTS ANYONE WHO CALLS IT

        /// <summary>
        /// [SOS] Always call <see cref="GetCurrentTimestamp_TS"/> to get the timestamp
        /// [SOS] First get timestamp, then invoke events / call other scripts, finall make the call
        /// </summary>
        // [SOS] Keep timestamp of event as the FIRST argument, so that calling GetCurrentTimestamp_TS gets calculated before any string operations that could delay it by a few ms
        public static void LogData_AsTheyHappen_TS(TimeWrapper.Timestamp timestampOfEvent, string sender, object data, bool doDebug = false)
        {
            if (!doLogs) return;

            logger.LogData_AsTheyHappen_TS(timestampOfEvent, sender, data, doDebug);
        }

        public static void LogData_AtNextRenderedFrame_TS(string sender, object data, bool doDebug = false)
        {
            if (!doLogs) return;

            logger.LogData_AtNextRenderedFrame_TS(sender, data, doDebug);
        }

        internal static void LogStimulusTimings_Queue(string timings)
        {
            logger.LogIntoAnalysisFolder("Timings.csv", timings);
        }

        internal static void LogStimulusTimings_Final(string timings)
        {
            logger.LogIntoSequenceFolder("Timings_CORRECTED.csv", timings);
        }

        internal static void LogStimulusQueue(string fileName, string report)
        {
            logger.LogStimulusQueue(fileName, report);
        }

        internal static void LogStimulusQueueCSV(string csv)
        {
            logger.LogIntoAnalysisFolder("StimSequence.csv", csv);
        }

        internal static void LogStimulusQueueAnalysisCSV(string csv)
        {
            logger.LogIntoAnalysisFolder("StimSequence_Analysis.csv", csv);
        }

        internal static void LogLocalizersCSV(string csv)
        {
            logger.LogIntoAnalysisFolder("Localizers.csv", csv);
        }

        internal static void LogLocalizersCSV_Final(string csv)
        {
            logger.LogIntoSequenceFolder("Localizers_Corrected.csv", csv);
        }

        #endregion


        #region From Experiment Manager
        /// <summary>
        /// NS_RENAME Exp Manager Level
        /// </summary>

        public static ModuleType module;
        public static HandType handType { get; private set; }
        public static PrepVsFull prepVsFull { get { return module.ToPrepVsFull(); } }
        public static bool isFMRI { get { return module == ModuleType.FMRI_Scanner || module == ModuleType.FMRI_Preparation; } }
        public static int numLocalizers { get { return PlayerProgression.numLocalizers; } }
        public static int numGameWorlds { get { return PlayerProgression.numGameWorlds; } }
        public static int numLevelsPerWorld { get { return PlayerProgression.numLevelsPerWorld; } }
        public static int numLevels { get { return PlayerProgression.numLevels; } }

        // TODO : Should be PRIVATE
        public static ExperimentAnalyticsSession session_analytics { get; private set; }

        public bool session_shownTutorialTip = false;

        private void LogExperimentSessionAnalytics(string analytics)
        {
            logger.LogExperimentSessionAnalytics(analytics);
        }

        /// <summary>
        /// [SPAGHETTI] This goes over to <see cref="ExperimentManagerApplication"/>
        /// from <see cref="PlayerProgression_onLevelComplete(object, LevelCompleteArgs)"/> instead of directly here
        /// </summary>
        /// <param name="e"></param>
        internal void OnLevelComplete(LevelCompleteArgs e, bool keepMusicPlaying = false)
        {
            if (e.endReason == EndReason.Success) // Only add Successfully Completed Levels. FAILED won't do
                LeaderboardUI.AddCompleteLevel(e.levelConfig);

            bool aborted = e.endReason == EndReason.Exit;
            bool isFirstHalf = FMRI_IsLevelFirstHalfOfRun(e.levelConfig.levelID_1Based);

            triggerMaster.SendLevelEndEvent(!e.levelConfig.isLocalizer);

            AsyncThread.RequestRunOnNewThread_OnNextFrame(() =>
            {
                int runID = FMRI_GetRunID(currentLevel);
                int levelWithinRun = FMRI_IsLevelFirstHalfOfRun(currentLevel.levelID_1Based) ? 1 : 2;
                LogFMRIDump("{0};{1};{2};LEVEL_END"._Format(
                    TimeWrapper.GetLastFrameTimestamp_TS().ToString(), runID, levelWithinRun));
            });

            if (e.endReason == EndReason.Success)
                AddTotalLevelTime(e.elapsedTimeSeconds_Level_NoPauses);
            else
                this.LogWarning("Level failed / exited manually, not adding to total level time | " + e.endReason);

            Pause(false, keepMusicPlaying); // Let the cursor where it is. If we go back to the menu, it will be handled by the level selection
            
            session_analytics.StopTrackingLevel(e);

            // If we aborted, and it was the 2nd half of a run, remove the first one too
            if (aborted && isFMRI && !isFirstHalf)
            {
                string secondHalfName = e.levelConfig.levelName;
                string firstHalfName = FMRI_GetNameOtherHalfOfRun(secondHalfName);
                session_analytics.RemoveLevel(firstHalfName);

                // Same for Progression!
                int firstHalfID_1Based = LevelsLibrary.GetLevelByName(firstHalfName).levelID_1Based;
                PlayerProgression.RemoveCompleteLevel(firstHalfID_1Based);
                LeaderboardUI.RemoveCompleteLevel(firstHalfID_1Based);
            }

            eyeTracker?.OnLevelComplete();

            PlayerProgression.StopLevel();

            // Check if time to WRITE completed levels. 
            if (aborted)
                this.LogWarning("Not writing levels ; Aborted!");

            else if (isFMRI && isFirstHalf)
                // Dont do for 1st halves
                this.LogWarning("Not writing levels ; FMRI 1st half");

            else
            {
                string completedLevelsCSV = PlayerProgression.GetCompletedLevelsCSV();
                logger.LogCompletedLevels(completedLevelsCSV);
                this.LogWarning("WROTE levels");

                string opponentScoresCSV = LeaderboardUI.GetOpponentScoresCSV();
                logger.LogOpponentScores(opponentScoresCSV);
                this.LogWarning("WROTE scores");
            }

            if (IsIntendedForReplay(e.levelConfig.levelID_1Based))
            {
                string recordedLevelJSON = ReplaySystem.TryGetRecordedLevelJSON(e.levelConfig.levelName);

                if (recordedLevelJSON.IsNullOrEmpty())
                    this.LogWarning("No recorded info for level :: " + e.levelConfig.levelName);
                else if (aborted)
                {
                    this.LogWarning("Had recorded info for level :: " + e.levelConfig.levelName + " but the level was ABORTED");
                    // Throw it away
                    // ReplaySystem.RemoveRecordedLevel(e.levelConfig.levelName);
                    this.LogWarning("Saved it nonetheless");
                    logger.LogReplayLevel(e.levelConfig.levelName, recordedLevelJSON);
                }
                else
                    logger.LogReplayLevel(e.levelConfig.levelName, recordedLevelJSON);
            }
        }
        
        // [TODO] maybe return false for non fmri ??
        public static bool FMRI_IsLevelFirstHalfOfRun(LevelConfig level)
        {
            return FMRI_IsLevelFirstHalfOfRun(level.levelID_1Based, level.isLocalizer);
        }

        // [TODO] maybe return false for non fmri ??
        public static bool FMRI_IsLevelFirstHalfOfRun(int levelID_1Based)
        {
            return FMRI_IsLevelFirstHalfOfRun(levelID_1Based, levelID_1Based >= LevelConfig.localizerOffset);
        }

        public static int FMRI_GetRunID(LevelConfig level)
        {
            return FMRI_GetRunID(level.levelID_1Based, level.isLocalizer);
        }

        public static int FMRI_GetRunID(int levelID_1Based)
        {
            return FMRI_GetRunID(levelID_1Based, levelID_1Based >= LevelConfig.localizerOffset);
        }

        // [TODO] maybe return false for non fmri ??
        private static bool FMRI_IsLevelFirstHalfOfRun(int levelID_1Based, bool isLocalizer)
        {
            // [SOS] Levels are 1-based - Localizers are 0-based
            int oneBasedID = isLocalizer ? levelID_1Based + 1 : levelID_1Based;

            // First halfs of run are all levels that appear ODD in the UI
            // aka 1, 3, 5, ..
            return oneBasedID % 2 == 1;
        }

        // [TODO] maybe return false for non fmri ??
        private static int FMRI_GetRunID(int levelID_1Based, bool isLocalizer)
        {
            // [SOS] Levels are 1-based - Localizers are 0-based
            int oneBasedID = isLocalizer ? levelID_1Based + 1 : levelID_1Based;

            // First halfs of run are all levels that appear ODD in the UI
            // aka 1, 3, 5, ..
            return Mathf.CeilToInt(oneBasedID / 2f);
        }

        internal static string FMRI_GetNameOtherHalfOfRun(string levelName)
        {
            string[] parts = levelName.Split('_');
            int idxWithinWorld_1Based = parts[1].ToInt();
            int idxOfOther_1Based = -1;

            if (idxWithinWorld_1Based % 2 == 1) // Odd ones are the first half
                idxOfOther_1Based = idxWithinWorld_1Based + 1;
            else
                idxOfOther_1Based = idxWithinWorld_1Based - 1;

            return "{0}_{1}"._Format(parts[0], idxOfOther_1Based);
        }

        public static bool IsLastLevelOfLocalizerBlock(LevelConfig level)
        {
            return GetIDOfLocalizerWithinBlock_0Based(level) == numLocalizersPerBlock - 1;
        }

        public static int GetIDOfLocalizerBlock(LevelConfig level)
        {
            if (!level.isLocalizer) return -1;

            return GetIDOfLocalizerBlock(level.localizerID_0Based);
        }

        public static int GetIDOfLocalizerBlock(int localizerID_0Based)
        {
            return Mathf.FloorToInt(localizerID_0Based / numLocalizersPerBlock);
        }

        public static string GetIDOfLocalizerBlock_String(LevelConfig level)
        {
            if (!level.isLocalizer) return "";

            return GetIDOfLocalizerBlock_String(level.localizerID_0Based);
        }

        public static string GetIDOfLocalizerBlock_String(int localizerID_0Based)
        {
            int idOfLocalizerBlock = GetIDOfLocalizerBlock(localizerID_0Based);

            return idOfLocalizerBlock == 0 ? "A" : "B";
        }

        public static int GetIDOfLocalizerWithinBlock_0Based(LevelConfig level)
        {
            if (!level.isLocalizer) return -1;

            return level.localizerID_0Based % numLocalizersPerBlock;
        }

        internal static void LogStimulusQueueAnalysis(string fileName, string analysis)
        {
            logger.LogStimulusQueueAnalysis(fileName, analysis);
        }

        // [HACK 200526]
        /// Analyzer does not have access to config so we can't use <see cref="numLocalizersPerBlock"/>
        public static string GetIDOfLocalizerBlock_String_ANALYZER(int localizerID_0Based)
        {
            int idOfLocalizerBlock = GetIDOfLocalizerBlock_ANALYZER(localizerID_0Based);

            return idOfLocalizerBlock == 0 ? "A" : "B";
        }

        public static int GetIDOfLocalizerBlock_ANALYZER(int localizerID_0Based)
        {
            return Mathf.FloorToInt(localizerID_0Based / 4);
        }

        public static int GetIDOfLocalizerWithinBlock_0Based_ANALYZER(int localizerID_0Based)
        {
            return localizerID_0Based % 4;
        }

        public static float GetTotalDollars()
        {
            float money = Mathf.Max(0, ExperimentLibraryManager.Config.Money.wantedMoneyStart);

            // 1 per Star
            float wantedMoneyPerStar = ExperimentLibraryManager.Config.Money.wantedMoneyPerStar;
            float moneyFromStars = PlayerProgression.GetTotalStars() * wantedMoneyPerStar;

            money += Mathf.Max(0, moneyFromStars);

            // We want 
            float wantedMoneyPerPoint = ExperimentLibraryManager.Config.Money.wantedMoneyPerPoint;
            float moneyFromPoints = PlayerProgression.GetTotalPoints() * wantedMoneyPerPoint;

            // 1 per 50 Points
            money += Mathf.Max(0, moneyFromPoints);

            // 5 per world
            float wantedMoneyPerCompletedWorld = ExperimentLibraryManager.Config.Money.wantedMoneyPerCompletedWorld;
            float moneyFromWorlds = PlayerProgression.GetNumCompletedWorlds() * wantedMoneyPerCompletedWorld;

            money += Mathf.Max(0, moneyFromWorlds);

            // Finished game?
            if (PlayerProgression.HasCompletedGame())
                money += Mathf.Max(0, ExperimentLibraryManager.Config.Money.wantedMoneyEnd);

            return money;
        }

        public static int numLocalizersPerBlock { get { return PlayerProgression.numLevelsPerWorld; } }

        public static int wantedNumStimuli_AllLocalizers
        {
            get
            {
                return Mathf.CeilToInt(ExperimentLibraryManager.Config.Experiment.stimulus.
                    wantedNumStimulus_Localizer_FullGame * (PlayerProgression.numGameWorlds / (float)LevelsLibrary.config.numWorldsFullGame));
            }
        }

        public static int wantedNumStimuli_PerLocalizer
        {
            get
            {
                int numLocalizers = PlayerProgression.numLocalizers;
                return numLocalizers == 0 ? 0 : Mathf.CeilToInt(
                    wantedNumStimuli_AllLocalizers / numLocalizers * 1f);
            }
        }

        public static int GetWantedNumProbesPerWorld()
        {
            int probesPerWorld_Base = ExperimentLibraryManager.Config.Probes.GetWantedNumProbesPerWorld(
                PlayerProgression.numGameWorlds);

            if (module.ToPrepVsFull() == PrepVsFull.Full)
                return probesPerWorld_Base;

            // For Prep, we want proportionate to how many levels there are
            int referenceNumLevelsPerWorld = 4;
            return Mathf.CeilToInt((float)probesPerWorld_Base / referenceNumLevelsPerWorld *
                PlayerProgression.numLevelsPerWorld);
        }

        public static int GetWantedNumProbesTotal()
        {
            return GetWantedNumProbesPerWorld() * PlayerProgression.numGameWorlds;
        }
        #endregion

        public static event EventHandler<EventArgs<int>> onProbesComplete = null;

        public static ProbeSummary session_probeSummary;

        // Don't add time for Tutorial
        /// TODO : Deprecate
        /// <summary>
        /// [SOS] is Null when not tracking a level! Consider using <see cref="GetJourneySummary(bool)"/> instead.
        /// </summary>
        public static ProbeSummary.JourneySummary journeySum
        {
            get
            {
                if (currentLevel == null) return null;

                return GetJourneySummary(currentLevel.isTutorial);
            }
        }

        public static ProbeSummary.JourneySummary GetJourneySummary(bool isTutorial)
        {
            return session_probeSummary.GetJourneySummary(isTutorial);
        }

        public static ExperimentLogger logger = new ExperimentLogger();

        #region MonoBehaviour

        private static ExperimentManagerLevel experimentManagerLevel_Persistent;

        public void Pause(bool toggleCursor = true, bool keepMusicPlaying = false)
        {
            SetPause(true, toggleCursor, keepMusicPlaying);
        }

        public void UnPause(bool toggleCursor = true)
        {
            SetPause(false, toggleCursor);
        }

        private void SetPause(bool pause, bool toggleCursor = true, bool keepMusicPlaying = false)
        {
            if (toggleCursor)
                ToggleCursor(pause);

            // Are we in a level?
            if (currentLevel != null)
            {
                if (pause)
                    experimentManagerLevel_Persistent.Pause(keepMusicPlaying);
                else
                    experimentManagerLevel_Persistent.UnPause();
            }

            string msg = pause ? "PAUSE" : "RESUME";
            // PauseEyeTracking(pause);

            LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(), "LevelMasterManager", msg);
        }

        public void PauseEyeTracking(bool pause)
        {
            if (pause)
                eyeTracker?.StopRecording();
            else
                eyeTracker?.StartRecording();

            this.LogWarning("Eye Tracking " + (pause ? "PAUSE" : "RESUME"));
        }

        public void ToggleCursor(bool on)
        {
            this.LogWarning("Cursor -> " + on);
            Cursor.visible = on;
        }

        public void ClearTutorialSum()
        {
            session_probeSummary.ClearTutorialSum();
        }

        public static void SetAllIndices(LevelConfig level)
        {
            AllIndices allIndices = Precalculator.GetAllIndicesAtStartOfLevel(level);

            BackgroundManager.SetCycleIdx(allIndices.bckgIdx);
            StimulusManager.SetCycleIdx(allIndices.stimIdx);
            ProbeManager.SetCycleIdx(allIndices.probeIdx);

            Debug_Helper.LogWarning(typeof(ExperimentManagerSession), "All indices set {0}"._Format(allIndices));
        }

        [Serializable]
        public class Config
        {
            public bool doFullLogs { get { return logger.doLogs; } }

            public bool obscureDateInGeneralInfo = false;
            public bool obscureTimeInGeneralInfo = false;

            public bool overrideFixedUpdateInterval = true;
            public float fixedUpdateIntervalS { get { return fixedUpdateIntervalMS / 1000f; } }
            /// <summary>
            /// [SOS] Use <see cref="fixedUpdateIntervalS"/> instead
            /// </summary>
            public int fixedUpdateIntervalMS = 5;
            public ExperimentLogger.Config logger;
            public TimescaleConfig timescale;
            public bool doEndOfLevelLoggingReport = true;
        }

        public class RuntimeConfig
        {
            public string version;
            public string originalConfigFilepath;
            /// <summary>
            /// For NEW runs, this is the Streaming assets folder, for CONTINUATIONS it's the last run's sequence folder
            /// </summary>
            public string originalSequenceFolderpath;
            public string overrideLoadFromFolder;
            public SubjectInfo subject;
            public ExperimentLogger.RuntimeConfig logger;
            public TriggerMaster.RuntimeConfig triggerMaster;
            public SoundSystem.RuntimeAudioSourceConfig audioTone;
            public EyeTrackerManager_Base eyeTracker;
            public EyeTracker_UI eyeTrackerUI;
            public Vector2 cameraOffset;

            public RuntimeConfig(string version, string originalConfigFilepath, string originalSequenceFolderpath, string overrideLoadFromFolder, SubjectInfo subject, ExperimentLogger.RuntimeConfig logger, TriggerMaster.RuntimeConfig triggerMaster, SoundSystem.RuntimeAudioSourceConfig audioTone, EyeTrackerManager_Base eyeTracker, EyeTracker_UI eyeTrackerUI, Vector2 cameraOffset)
            {
                this.version = version;
                this.originalConfigFilepath = originalConfigFilepath;
                this.originalSequenceFolderpath = originalSequenceFolderpath;
                this.overrideLoadFromFolder = overrideLoadFromFolder;
                this.subject = subject;
                this.logger = logger;
                this.triggerMaster = triggerMaster;
                this.audioTone = audioTone;
                this.eyeTracker = eyeTracker;
                this.eyeTrackerUI = eyeTrackerUI;
                this.cameraOffset = cameraOffset;

                handType = subject.handType;
            }
        }

        public string subjectID { get { return subject?.id; } }
        public SubjectInfo subject { get { return runtimeConfig?.subject; } }

        private RuntimeConfig runtimeConfig;

        private Config config;

        private void TryInitializeEyeTracker()
        {
            if (eyeTracker)
            {
                string logsPath = ExperimentLibraryManager.logsDirectory + "/" + logger.GetEyeTrackerLogsFolderPathInLogs();
                eyeTracker.SetLogsDirectory(logsPath);

                BackgroundManager.Config backgroundConfig = ExperimentLibraryManager.Config.Experiment.stimulus.background;

                // Calculate the visual angle between the center of the screen and the current gaze coordinates
                Vector2 screenCenterPixels = new Vector2(0.5f * Screen.width, backgroundConfig.fixationVerticalPositionPercentileUnscaled * Screen.height);

                float maxDistance_VisualAngle = backgroundConfig.maxVisualAngleDeviationFromFixation;
                float maxDistance_CM = backgroundConfig.GetCMFromVisualAngle(maxDistance_VisualAngle);
                float maxDistance_Pixels = backgroundConfig.GetPixelsFromCM(maxDistance_CM);

                eyeTracker.SetFixation(screenCenterPixels, maxDistance_Pixels);

                eyeTracker.TryConnect();

                if (eyeTracker as Peripherals.EyeTracking.Tobii.EyeTrackerManager_Tobii != null)
                    logger.LogEyeTrackingSyncDump(Tobii.Research.Unity.EyeTracker.TimeSync.GetHeader());
            }
        }

        public ExperimentManagerSession(Config config, RuntimeConfig runtimeConfig)
        {
            this.config = config;
            this.runtimeConfig = runtimeConfig;
            wantToLog = config.doFullLogs;

            triggerMaster = new TriggerMaster(ExperimentLibraryManager.Config.TriggerOut, runtimeConfig.triggerMaster);
            triggerMaster.SetCheckSend(TriggerMaster_CheckSend);

            logger = new ExperimentLogger();
            logger.Initialize(config.logger, runtimeConfig.logger);

            // 1. PERIPHERALS SUBSCRIPTIONS
            {
                PhotoDiodeDebugger.onStatusUpdate += PDB_onStatusUpdate;

                LPTManager.onStatusUpdate += LPTM_onStatusUpdate;

                InputManager.onKeyStatusUpdate += IM_onKeyStatusUpdate;
                InputManager.onConnectionStatusUpdate += IM_onConnectionStatusUpdate;
                InputManager.onActivityStatusUpdate += IM_onActivityStatusUpdate;

                LoggingTester.onReportReady += LoggingTester_onReportReady;

                HighAccuracyInput_Base.onKeyStatusUpdate += HAIB_onKeyStatusUpdate;
                HighAccuracyInput_Base.onStatusUpdate += HAIB_onStatusUpdate;

                SoundSystem.onStatusUpdate_AudioSource += SS_onStatusUpdate_AudioSource;
                SoundSystem.onStatusUpdate_Mute += SS_onStatusUpdate_Mute;

                EyeTrackerManager_Base.onGazeUpdated += EyeTracker_onGazeUpdated;
                EyeTrackerManager_Base.onSacadaEnd += EyeTracker_onSacadaEnd;
                EyeTrackerManager_Base.onBlinkEnd += EyeTracker_onBlinkEnd;
                EyeTrackerManager_Base.onBlinkInfo += EyeTracker_onBlinkInfo;
                EyeTrackerManager_Base.onTrackingStateChanged += EyeTracker_onTrackerStateChanged;
                EyeTrackerManager_Base.onMessageFailed_Thread += EyeTracker_onMessageFailed_TS;
                EyeTrackerManager_Base.onMessageWritten_Thread += EyeTracker_onMessageWritten_TS;
                EyeTrackerManager_Base.onCommandFailed += EyeTracker_onCommandFailed;
                EyeTrackerManager_Base.onCommandSent += EyeTracker_onCommandSent;
                EyeTrackerManager_Base.onDebugInfo += EyeTracker_onDebugInfo;
                EyeTrackerManager_Base.onCalibrateProgressReport += EyeTracker_onCalibrateProgressReport;
                EyeTrackerManager_Base.onTrackingStateChanged += EyeTracker_onTrackingStateChanged;

                SerialPortManager.onDebugInfo += SPM_onDebugInfo;

                Tobii.Research.Unity.EyeTracker.onTimeSyncRefRcv += EyeTracker_onTimeSyncRefRcv;
            }


            // 2. GAME SUBSCRIPTIONS
            {
                Interactable.onStatusUpdate += Interactable_onStatusUpdate;
                PlayerView_Runner.onStatusUpdate += PlayerView_Runner_onStatusUpdate;
                PlayerModel.onStatusUpdate += PlayerModel_onStatusUpdate;

                BackgroundObject_Abstract_RotatingSquares.onStatusUpdate += BOAR_onStatusUpdate;
                BackgroundManager.onStatusUpdate += BackgroundManager_onStatusUpdate;

                ReplaySystem.onErrorMessage += ReplaySystem_onErrorMessage;
                ReplaySystem.onStatusUpdate += ReplaySystem_onStatusUpdate;

                PlayerProgression.onLevelComplete += PlayerProgression_onLevelComplete;
                TimeWrapper.onDebugMessageSent += TimeWrapper_onDebugMessageSent;
            }

            // [201113] Would maybe make more sense AFTER session resume (though they don't seem to have an effect
            // GAME INITIALIZATIONS
            {
                if (config.overrideFixedUpdateInterval)
                    TimeWrapper.fixedDeltaTime_NotTS = config.fixedUpdateIntervalS;

                EngineWrapper.StartCoroutine(TrackDifficultyInBackground());

                this.runtimeConfig = runtimeConfig;

                LeaderboardUI.InitializeSubject(subject.subjectNumber.ToString());

                experimentManagerLevel_Persistent = new ExperimentManagerLevel();
                experimentManagerLevel_Persistent.onLevelReadyToEnd += ExperimentManagerLevel_onLevelReadyToEnd;
                experimentManagerLevel_Persistent.onReplayWindowOver_Thread += ExperimentManagerLevel_Persistent_onReplayWindowOver_TS;
                experimentManagerLevel_Persistent.ui_onProbeShown += ExperimentManagerLevel_onProbeShown;

                session_analytics = new ExperimentAnalyticsSession();

                // At this point we can Initialize the File Logger
                logger.CopyConfigFile(runtimeConfig.originalConfigFilepath);
                logger.CopySequenceFolder(runtimeConfig.originalSequenceFolderpath);
                if (!runtimeConfig.originalConfigFilepath.IsNullOrEmpty())
                    LogSessionInfo("Copied Config from :: " + runtimeConfig.originalConfigFilepath);
                if (!runtimeConfig.originalSequenceFolderpath.IsNullOrEmpty())
                    LogSessionInfo("Copied Sequence from :: " + runtimeConfig.originalSequenceFolderpath);

                LogHeaders();
                session_probeSummary = new ProbeSummary(runtimeConfig.subject);
            }

            // 3. SESSION RESUME (IF APPLICABLE)
            {
                if (runtimeConfig.overrideLoadFromFolder.IsNullOrEmpty())
                {
                }
                else
                {
                    string subjectProgressFolderPath = runtimeConfig.overrideLoadFromFolder + "Progress/";
                    string subjectFullLogsFolderPath = runtimeConfig.overrideLoadFromFolder + "FullLogs/";
                    string subjectExtraLogsFolderPath = runtimeConfig.overrideLoadFromFolder + "ExtraLogs/";
                    string subjectSummariesFolderPath = runtimeConfig.overrideLoadFromFolder + "Summaries/";

                    logger.CopyProgressFolder(subjectProgressFolderPath);
                    logger.CopyFullLogsFolder(subjectFullLogsFolderPath);
                    logger.CopyExtraLogsFolder(subjectExtraLogsFolderPath);
                    // logger.CopySummariesFolder(subjectSummariesFolderPath);

                    string summaryGame = "";
                    string summaryReplay = "";
                    foreach (string summaryFilePath in FileWrapper.GetFolderContents(subjectSummariesFolderPath, FilesFolders.Files, true))
                        if (summaryFilePath.ContainsInvariant("_GAME"))
                            summaryGame = FileWrapper.ReadFromFile(summaryFilePath);
                        else if (summaryFilePath.ContainsInvariant("_REPLAY"))
                            summaryReplay = FileWrapper.ReadFromFile(summaryFilePath);
                        else
                            this.LogWarning("Unexpected file in Summaries :: {0}"._Format(subjectSummariesFolderPath, summaryFilePath));

                    session_probeSummary.SetSummaries(summaryGame, summaryReplay);

                    LogSessionInfo("Copied Progress from :: " + subjectProgressFolderPath);
                    LogSessionInfo("Copied COMPLETED FullLogs from :: " + subjectFullLogsFolderPath);
                    LogSessionInfo("Copied Extra Logs from :: " + subjectExtraLogsFolderPath);
                    LogSessionInfo("Copied Summaries from :: " + subjectSummariesFolderPath);

                    List<string> allFullLogs = FileWrapper.GetFolderContents(
                        LogWrapper.GetPath(logger.GetFullLogsFolderPath()), FilesFolders.Files, true).FindAll(x => x.Contains("FullLogLevel"));

                    allFullLogs.Sort();
                    string last_Completed = "";
                    foreach (string s in allFullLogs)
                        if (!s.Contains("COMPLETED"))
                            FileWrapper.DeleteFile(s);
                        else
                            last_Completed = s;

                    // Get the timestamp
                    if (last_Completed != "")
                    {
                        string lastLine = FileWrapper.ReadAllLines(last_Completed).GetLast();
                        string[] parts = lastLine.Split(';');
                        int frameID = parts[0].ToInt();
                        double timestamp = parts[1].ToDouble();
                        double timestampNoPauses = parts[2].ToDouble();

                        LogSessionInfo("Last run's last known valid FrameID was {0} and timestamp {1} (w/o pauses {2})"._Format(frameID, timestamp, timestampNoPauses));
                        TimeWrapper.SetOverrideTimestamp(new TimeWrapper.Timestamp(frameID, timestamp, timestampNoPauses));
                        // Override the wrapper
                        // TimeWrapper.OverrideCurrentTimestamp(frameID, timestamp);
                    }

                    Func<string, bool> check = s =>
                    {
                    // 11580.9943;2403.39875221252;1033.57803821564;1;1;0;Blue;0.1999937;0.1999968;0;0;0.02150538;-1;-1;2;GAME_FILLER;False
                    int levelID_Idx = 3;
                        int timestamp_Idx = 0;

                        string[] parts = s.Split(false, ";");

                        if (parts.Length < levelID_Idx)
                        {
                            this.LogError("Entry had less than {0} fields :: {1}"._Format(levelID_Idx, s));
                            return false;
                        }

                        int levelID = parts[levelID_Idx].ToInt();

                        if (levelID < 0)
                        {
                            this.LogWarning("Quering about invalid level ID 1-based :: {0} ({1})"._Format(levelID, s));
                            return false;
                        }

                        if (parts.Length < timestamp_Idx)
                        {
                            this.LogError("Entry had less than {0} fields :: {1}"._Format(timestamp_Idx, s));
                            return false;
                        }

                        double timestamp = parts[timestamp_Idx].ToDouble();

                        if (timestamp < 0)
                        {
                            this.LogWarning("Quering about invalid timestamp :: {0} ({1})"._Format(timestamp, s));
                            return false;
                        }

                        LevelConfig levelConfig = LevelsLibrary.GetLevel(levelID);

                        if (levelConfig == null)
                        {
                            this.LogError("Null level config for ID 1-based :: {0} ({1})"._Format(levelID, s));
                            return false;
                        }

                        string levelName = levelConfig.levelName;

                    // Was the level completed? 
                    // First, filter out
                    Dictionary<double, string> fullLogNames = new Dictionary<double, string>();
                        foreach (string fullLogName in allFullLogs.FindAll(x => x.Contains(levelName))) // Filter out irrelevant logs
                    {
                        // S0100A_FullLogLevel_1_1_6080.8806_COMPLETED
                        string[] fullLogParts = fullLogName.Split('_');

                            int maxIndex = fullLogParts.Length - 1;
                            double _timestamp = fullLogParts[maxIndex - 1].ToDouble(); // 6080.8806

                        fullLogNames.Add(_timestamp, fullLogName);
                        }

                    // Start at the highest timestamp
                    List<double> keys = new List<double>();
                        keys.AddRange(fullLogNames.Keys);
                        foreach (double logTS in keys.CustomOrderBy(x => x, Order.Descending))
                        {
                            if (timestamp < logTS) continue; // Not this log (go down the list)

                        // Is this log completed?
                        string logName = fullLogNames[logTS];
                            if (logName.ContainsInvariant("COMPLETED"))
                            {
                                return true;
                            }
                            else
                            {
                                this.LogWarning("Level {0}'s full log was not COMPLETED\n{1} | {2}"._Format(levelName, logName, s));
                                return false;
                            }
                        }

                        this.LogWarning("Level {0}'s had no full logs"._Format(levelName, s));

                        return false;
                    };

                    string detailsFolder = runtimeConfig.overrideLoadFromFolder + "Details/";
                    this.LogWarning("Loading details from :: " + detailsFolder);
                    List<string> originalRunContent = FileWrapper.GetFolderContents(detailsFolder, FilesFolders.Files, true);

                    string fillerDetailsFilePath = originalRunContent.Find(x => x.Contains("FillerDetails.csv"));
                    foreach (string s in FileWrapper.ReadAllLines(fillerDetailsFilePath))
                        if (check(s))
                            logger.LogFillerDetails(s, true);

                    string stimulusDetailsFilePath = originalRunContent.Find(x => x.Contains("StimulusDetails.csv"));
                    foreach (string s in FileWrapper.ReadAllLines(stimulusDetailsFilePath))
                        if (check(s))
                            logger.LogStimulusDetails(s, true);

                    string probeDetailsFilePath = originalRunContent.Find(x => x.Contains("ProbeDetails.csv"));
                    foreach (string s in FileWrapper.ReadAllLines(probeDetailsFilePath))
                        if (check(s))
                            logger.LogProbeDetail(s, true);

                    string localizerDetailsFilePath = originalRunContent.Find(x => x.Contains("LocalizerDetails.csv"));
                    foreach (string s in FileWrapper.ReadAllLines(localizerDetailsFilePath))
                        if (check(s))
                            logger.LogLocalizerDetails(s, true);

                    LogSessionInfo("Copied FillerDetails from :: " + fillerDetailsFilePath);
                    LogSessionInfo("Copied StimulusDetails from :: " + stimulusDetailsFilePath);
                    LogSessionInfo("Copied ProbeDetails from :: " + probeDetailsFilePath);
                    LogSessionInfo("Copied LocalizerDetails from :: " + localizerDetailsFilePath);
                }
            }

            // MORE INITIALIZATIONS 
            {
                // [SOS] Do after time initialization (as time may get offset due to resume)
                TryInitializeEyeTracker();

                // [SOS] Do after assignments above
                LogProbeSummaries();

                logger.LogApplicationVersion(runtimeConfig.version);
                logger.LogModule(module.ToString());

                LoggingTester.Initialize(ExperimentLibraryManager.Config.LoggingTesting);
                // HighAccuracyInput.SetReferenceTime(probeSummary.sessionStartTime);

                LeaderboardUI.Reset();
                DifficultyManager.SubjectReset();
                PlayerProgression.SetProgressFromCSV(logger.LoadCompletedLevels());
                LeaderboardUI.SetProgressFromCSV(logger.LoadOpponentScores());
                ReplaySystem.SetRecordedLevelsJSON(logger.LoadReplayLevels());

                taskRelevantResponseID = 0;
                taskIrrelevantResponseID = 0;
                ReportSubjectDetails();

                // Log session Start
                LogSessionStart();
            }

            // ACCURACY TEST
            {
                int accuracyTestSeconds = ExperimentLibraryManager.Config.timingAccuracyTestSeconds;

                Action<string> accuracyTest = type =>
                {
                    double accuracy = TimeWrapper.DoAccuracyTest(accuracyTestSeconds);

                    if (accuracy > 2)
                        this.LogError(accuracy);
                    else if (accuracy > 1)
                        this.LogWarning(accuracy);
                    else
                        this.Log(accuracy);

                    LogSessionInfo("Timing Accuracy ({0}):: +-{1}ms"._Format(type, accuracy.ToString("#.00000000")));
                };

                if (ExperimentLibraryManager.Config.doTimingAccuracyTest_Normal)
                    accuracyTest("Normal");
                else
                    LogSessionInfo("Skipped Timing Accuracy test (Normal)");

                if (ExperimentLibraryManager.Config.doTimingAccuracyTest_Thread)
                    AsyncThread.RequestRunOnNewThread(() => { accuracyTest("Thread"); });
                else
                    LogSessionInfo("Skipped Timing Accuracy test (Thread)");
            }
        }


        #endregion

        #region Logging Handlers

        private static IEnumerator TrackDifficultyInBackground()
        {
            while (true)
            {
                yield return new WaitWhile(() => !ExperimentManagerApplication.IS_IN_GAME || LevelMasterManager.isPaused);
                LogData_AtNextRenderedFrame_TS("DifficultyManager", "{0};{1}"._Format( // [0]difficulty,[1]performance
                    DifficultyManager.difficulty, DifficultyManager.dPrime_Final));
                yield return new WaitForSeconds(1f);
            }
        }

        private void PDB_onStatusUpdate(object sender, PhotoDiodeDebugger.StatusUpdateArgs e)
        {
            LogData_AtNextRenderedFrame_TS("TRIGGER_MANAGER_PHOTODIODE",
                string.Format("{0};{1};{2};{3}", "TRIGGER_INFO", e.reason, "TOGGLE", e.isOn ? "ON" : "OFF"));

            // Mark as sent the first time we go WHITE
            if (e.isFirst)
                LogData_AtNextRenderedFrame_TS("TRIGGER_MANAGER_PHOTODIODE",
                    string.Format("{0};{1}", "TRIGGER_SENT", e.reason));
        }

        private void LPTM_onStatusUpdate(object sender, LPTManager.StatusUpdateArgs e)
        {
            TimeWrapper.Timestamp timestampOfReport = TimeWrapper.GetCurrentTimestamp_TS();

            /// TODO ASYNC (to not hold up the thread!) 
            /// Maybe a <see cref="Func{T, TResult}"/> <see cref="string"/> that calculates at the time of writing?
            LogData_AsTheyHappen_TS(timestampOfReport,
                "TRIGGER_MANAGER_LPT", "{0};{1};{2};{3}". // [0] event, [1] data, [2] extraInfo
                _Format(e.logType, e.triggerEvent, e.data, e.info), e.debug);
        }

        private void IM_onKeyStatusUpdate(object sender, InputManager.KeyStatusArgs e)
        {
            if (e.debug) Debug.LogError("{0};{1};{2}"._Format(e.source, e.state, e.keyCode));

            LogData_AsTheyHappen_TS(e.timestamp,
                "INPUT_MANAGER", string.Format("{0};{1};{2}", //  "[0] keyState, [1] source, [2] keyCode, [3] keyState TS"
                e.source, e.state, e.keyCode, e.keyStateTS));
        }

        private void IM_onActivityStatusUpdate(object sender, InputManager.ActivityStatusArgs e)
        {
            LogSessionInfo("Input Manager :: Toggling Input {0} ({1})"._Format(e.isActive ? "ON" : "OFF", e.reason));
        }

        private void IM_onConnectionStatusUpdate(object sender, InputManager.ConnectionStatusArgs e)
        {
            LogSessionInfo(
                "Input Connection / Device Changed from {0} ({1}) to {2} ({3})"._Format(
                e.lastConnectionType, e.lastResponseBoxString,
                e.currentConnectionType, e.currentResponseBoxString));

            // TODO : Check, this WAS BEING logged in every frame
            LogData_AsTheyHappen_TS(e.timestamp,
                "INPUT_MANAGER", "CURRENT_INPUT_TYPE;{0};{1}".
                    _Format(e.currentConnectionType, e.currentResponseBoxString)); // "[0] Connection Type, [1] Device Type"
        }

        private void EyeTracker_onGazeUpdated(object sender, EventArgs<TimestampedPosition> e)
        {
            if (currentLevel == null) return;

            if (e.value == null) return;

            eyeData_Gazes.array.Add(e.value.position);

            LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(),
                "EyeTracker", "GAZE;{0}"._Format(e.value.ToCSVString(true)));
        }

        private void EyeTracker_onSacadaEnd(object sender, EventArgs<Sacada> e)
        {
            if (currentLevel == null) return;
            Sacada s = e.value;
            Vector4 v4 = new Vector4(s.start.position.x, s.start.position.y, s.end.position.x, s.end.position.y);

            LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(),
                "EyeTracker", "SACADA;{0}"._Format(s.ToCSVString(true)));
            eyeData_Sacades.array.Add(v4);
        }

        private void EyeTracker_onBlinkEnd(object sender, EventArgs<Blink> e)
        {
            if (currentLevel == null) return;
            Blink b = e.value;

            LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(),
                "EyeTracker", "BLINK;{0}"._Format(b.ToCSVString()));
        }

        private void EyeTracker_onBlinkInfo(object sender, EventArgs<Blink> e)
        {
            if (currentLevel == null) return;
            Blink b = e.value;

            LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(),
                "EyeTracker", "BL_TRACKER;{0}"._Format(b.ToCSVString())); // Avoid naming it BLINK due to how analyzer works (checks if contains BLINK)
        }

        private void HAIB_onKeyStatusUpdate(object sender, HighAccuracyInput_Base.KeyStatusUpdateArgs e)
        {
            string e_Str = e.ToString();

            AsyncThread.RequestRunOnNewThread(() =>
            {
                logger.LogHighAccuracyInputDump(e_Str);
            });

            LogData_AsTheyHappen_TS(e.timestamp, e.name, e_Str);
        }

        public void OnCurrentLevelStart()
        {
            if (currentLevel.isLocalizer)
                experimentManagerLevel_Persistent.stimulusManager.OnReplayLevelStart();

            triggerMaster.SendLevelBeginEvent(!currentLevel.isLocalizer);

            AsyncThread.RequestRunOnNewThread_OnNextFrame(() =>
            {
                int runID = FMRI_GetRunID(currentLevel);
                int levelWithinRun = FMRI_IsLevelFirstHalfOfRun(currentLevel.levelID_1Based) ? 1 : 2;
                LogFMRIDump("{0};{1};{2};LEVEL_BEGIN"._Format(
                    TimeWrapper.GetLastFrameTimestamp_TS().ToString(), runID, levelWithinRun));
            });

            // AFTER start recording!
            eyeTracker?.RequestWriteToTracker_NotTS_Event("LEVEL_START " + currentLevel.levelName);
            LogData_AtNextRenderedFrame_TS(
                "LevelMasterManager", "LEVEL_START;{0};[0]levelName". // Level Name
                _Format(currentLevel.levelName));

            UnPause();
        }

        private void ExperimentManagerLevel_onLevelReadyToEnd(object sender, EventArgs<StimulusReportTool.TaskRelevantResponseEvaluation> e)
        {
            SafeEndOfGame(e);
        }

        private void ExperimentManagerLevel_Persistent_onReplayWindowOver_TS(object sender, TimeWrapper.Timestamp e)
        {
            LogData_AsTheyHappen_TS(e, "STIMULUS_REPORT_TOOL", "WINDOW_END");
        }

        private void ExperimentManagerLevel_onProbeShown(object sender, bool isShowing)
        {
            if (module == ModuleType.FMRI_Scanner || session_shownTutorialTip) { }
            // Only show tutorials for systems other than fmri scanner
            else
            {
                session_shownTutorialTip = true;
                ProbeSystem.ToggleTutorialTip(isShowing);
            }

            if (isShowing)
                Pause(false, ExperimentLibraryManager.Config.Probes.keepMusicOnDuringProbes);
            else
                UnPause();
        }

        private void HAIB_onStatusUpdate(object sender, HighAccuracyInput_Base.StatusUpdateArgs e)
        {
            string e_Str = e.ToString();

            AsyncThread.RequestRunOnNewThread(() =>
            {
                logger.LogHighAccuracyInputDump(e_Str);
            });

            LogData_AsTheyHappen_TS(e.timestamp, "INPUT_MANAGER", e_Str);
        }

        private void LoggingTester_onReportReady(object sender, EventArgs<LoggingAnalyzer.Report> e)
        {
            if (e.value == null) return;

            string e_Str = e.value.ToString();

            if (e_Str.IsNullOrEmpty()) return;

            logger.LogFullLogDump(e_Str);
        }

        private void TimeWrapper_onDebugMessageSent(object sender, DebugInfo e)
        {
            LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(), e.sender, e.message);
        }

        private void SS_onStatusUpdate_AudioSource(object sender, SoundSystem.AudioSourceStatusArgs e)
        {
            LogData_AtNextRenderedFrame_TS("SOUND_SYSTEM", "{0};{1};{2};{3}".
                   _Format(e.sourceName + "_" + e.eventType, e.clipName, e.volume, e.panStereo)); // "[0] Event Type, [1] Clip Name, [2] Playback Volume (Normalized), [3] Stereo Panning"
        }

        private void SS_onStatusUpdate_Mute(object sender, SoundSystem.MuteStatusArgs e)
        {
            string msg = "MUTE_SOUNDS;{0};{1}"._Format(e.isMuted ? 1 : 0, e.reason); // "[0] Is Muted, [1] Reason"
            LogData_AtNextRenderedFrame_TS("SOUND_SYSTEM", msg);
        }

        private void EyeTracker_onTrackerStateChanged(object sender, EyeTrackerManager_Base.TrackingStateArgs e)
        {
            LogData_AsTheyHappen_TS(e.timestamp, e.name,
                "STATE_CHANGED;{0};{1}"._Format(e.oldState, e.newState)); // [0] Old, [1] New
        }

        private void EyeTracker_onMessageFailed_TS(object sender, EyeTrackerManager_Base.MessageArgs e)
        {
            LogData_AsTheyHappen_TS(e.timestamp, "MESSAGE_FAILED_" + e.name, e.msg);
        }

        private void EyeTracker_onMessageWritten_TS(object sender, EyeTrackerManager_Base.MessageArgs e)
        {
            LogData_AsTheyHappen_TS(e.timestamp, "MESSAGE_SENT_" + e.name, e.msg);
        }

        private void EyeTracker_onCommandSent(object sender, EyeTrackerManager_Base.MessageArgs e)
        {
            AsyncThread.RequestRunOnNewThread(() =>
            {
                logger.LogEyeTrackingCommandDump("COMMAND_SENT;{0}"._Format(e));
            });
        }

        private void EyeTracker_onDebugInfo(object sender, EyeTrackerManager_Base.MessageArgs e)
        {
            AsyncThread.RequestRunOnNewThread(() =>
            {
                logger.LogEyeTrackingDebugDump(e.ToString());
            });
        }

        private void EyeTracker_onCommandFailed(object sender, EyeTrackerManager_Base.MessageArgs e)
        {
            AsyncThread.RequestRunOnNewThread(() =>
            {
                logger.LogEyeTrackingCommandDump("COMMAND_FAILED;{0}"._Format(e));
            });
        }

        private void EyeTracker_onCalibrateProgressReport(object sender, EventArgs<ProgressReport> e)
        {
            if (!e.value.isDone)
            {
                LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(), "EYE_TRACKER", "CALIBRATION_IN_PROGRESS;{0}"._Format(e.value.value01));
                if (ExperimentLibraryManager.Config.EyeTracking.pauseDuringEyeTrackingCalibration)
                    Pause(false); // dont let the eyetracking interfere with the cursor (no need as we overlay)
            }
            else
            {
                LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(), "EYE_TRACKER", "CALIBRATION_COMPLETE");
                if (ExperimentLibraryManager.Config.EyeTracking.unPauseAfterEyeTrackingCalibration)
                    UnPause(false); // dont let the eyetracking interfere with the cursor (no need as we overlay)
            }
        }

        private void SPM_onDebugInfo(object sender, DebugInfo e)
        {
            AsyncThread.RequestRunOnNewThread(() =>
            {
                logger.LogSerialDump(e.ToString());
            });
        }

        private void EyeTracker_onTimeSyncRefRcv(object sender, EventArgs<Tobii.Research.Unity.EyeTracker.TimeSync> e)
        {
            AsyncThread.RequestRunOnNewThread(() =>
            {
                logger.LogEyeTrackingSyncDump(e.value.ToString());
            });
        }

        private void Interactable_onStatusUpdate(object sender, Interactable.StatusUpdateArgs e)
        {
            string selfSenderType = string.Format("Interactable:{0} at Screen_Position", e.colorType);
            LogData_AtNextRenderedFrame_TS(selfSenderType, e);
        }

        private void PlayerView_Runner_onStatusUpdate(object sender, PlayerView_Runner.StatusArgs e)
        {
            LogData_AtNextRenderedFrame_TS("Player_Runner_Screen_Position", e);
        }

        private void PlayerModel_onStatusUpdate(object sender, PlayerModel.StatusArgs e)
        {
            LogData_AtNextRenderedFrame_TS("Player", string.Format(
                "Player collided with interactable:{0}", e.colorType.ToString()));
        }

        private void BOAR_onStatusUpdate(object sender, BackgroundObject_Abstract_RotatingSquares.StatusArgs e)
        {
            LogData_AtNextRenderedFrame_TS(e.senderName, e, false);
        }

        private void BackgroundManager_onStatusUpdate(object sender, BackgroundManager.StatusArgs e)
        {
            LogData_AtNextRenderedFrame_TS("BACKGROUND_MANAGER",
                "ANIMATION_PART;{0};OF_{1};{2};{3}"._Format(
                    e.currentAnimEvent_1Based, e.totalNumAnimEventsPerCycle, e.animEventType, e.animEventExplanation,
                    e.doDebug));
        }

        private void ReplaySystem_onErrorMessage(object sender, string e)
        {
            LogSessionInfo(e);
        }

        private void ReplaySystem_onStatusUpdate(object sender, ReplaySystem.StatusArgs e)
        {
            LogData_AtNextRenderedFrame_TS("REPLAY_SYSTEM", "REPLAY_{0};{1};{2};{3}"._Format(
                e.state.ToString().AddSubstringBeforeCapitals("_").ToUpper(),
                e.levelID_1Based,
                e.levelName,
                e.numFrames)); // "[0] state, [1] recordedLevelId, [2] recordedLevelName, [3] recordedFrameCount"
        }
        
        private void PlayerProgression_onLevelComplete(object sender, LevelCompleteArgs e)
        {
            LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(), "LevelMasterManager", "LEVEL_COMPLETE");
            ExperimentManagerApplication.ExperimentManagerSession_onLevelComplete(e);
        }
        #endregion

        #region Tracking

        private void EyeTracker_onTrackingStateChanged(object sender, EyeTrackerManager_Base.TrackingStateArgs e)
        {
            // Ignore this in the menus
            if (ExperimentManagerApplication.IS_IN_MENU)
            {
                // this.LogWarning("Not caring in menu, state " + e);
                return;
            }

            switch (e.newState)
            {
                case EyeTrackerState.Unknown:
                    SoundSystem.SetAudioToneVolume(0);
                    break;
                case EyeTrackerState.NotConnected:
                    SoundSystem.SetAudioToneVolume(ExperimentLibraryManager.Config.Audio.eyeTrackerNoConnection);
                    break;
                case EyeTrackerState.InsufficientData:
                    SoundSystem.SetAudioToneVolume(ExperimentLibraryManager.Config.Audio.eyeTrackerNoDataWarningVolume);
                    break;
                // We don't have coordinates but we still have a sample
                case EyeTrackerState.Blink:
                    SoundSystem.SetAudioToneVolume(ExperimentLibraryManager.Config.Audio.eyeTrackerBlinkVolume);
                    break;
                case EyeTrackerState.GazeOutOfScreen:
                    SoundSystem.SetAudioToneVolume(ExperimentLibraryManager.Config.Audio.eyeTrackerOutOfScreenVolume);
                    break;
                // We have coordinates but 
                case EyeTrackerState.GazeOffFixation:
                    SoundSystem.SetAudioToneVolume(ExperimentLibraryManager.Config.Audio.eyeTrackerOffFixationVolume);
                    break;
                case EyeTrackerState.AllGood:
                    SoundSystem.SetAudioToneVolume(0);
                    break;
            }
        }

        private static LevelConfig currentLevel;
        public static string lastTracking_levelName = "";
        public static string lastTracking_levelTimestamp = "";
                
        public void WriteEyeTrackingInfo(Action<ProgressReport> callback)
        {
            EngineWrapper.StartCoroutine(WriteEyeTrackingInfoIE(callback));
        }

        public void RequestStartCurrentLevel()
        {
            PlayerProgression.TEMP_RequestStart(success =>
            {
                if (success)
                    OnCurrentLevelStart();
                else
                    LogData_AtNextRenderedFrame_TS(
                        "LevelMasterManager", "LEVEL_START_DENIED;{0};[0]levelName". // Level Name
                        _Format(currentLevel.levelName));
            });
        }

        public void SetAndPrepareLevel(LevelConfig levelToPrepare, float orthoSize, Vector2 offset01)
        {
            safeEndOFGameCR = null;
            doingSafeEndOfGame = false;
            currentLevel = levelToPrepare;

            // Reload probe summary !
            if (!isFMRI || FMRI_IsLevelFirstHalfOfRun(levelToPrepare))
            {
                this.LogWarning("Re-loading Summaries");
                session_probeSummary.SetSummaries(logger.GetProbeSummary_Game(), logger.GetProbeSummary_Replay());
            }

            // [SOS] Pause when this is over

            // Debug.LogError(typeof(ExperimentManagerSession) + " PrepBeforeStartingLevel ");

            // [SOS] Set these first
            lastTracking_levelName = levelToPrepare.levelName;
            lastTracking_levelTimestamp = TimeWrapper.currentTimestampMS.ToString();

            // [SOS] Then setup background writer
            logger.StartBackgroundDataWriting(lastTracking_levelName, lastTracking_levelTimestamp);

            Debug_Helper.LogError(typeof(ExperimentManagerSession), " Starting in-game tracking");

            if (!ExperimentLogger.isWriting)
                Debug_Helper.LogError(typeof(ExperimentManagerSession), "[ERROR] Should have already started in-game background writing!");

            /*
            if (ApplicationLibrary.Config.Logging.debugLevel == LogDebuggingLevel.PerLevel)
            {
                DoFormattedReports();
            }
            */


            latestLevelHeatmap_Gaze_Backgrounded_ForUI = null;
            latestLevelHeatmap_Sacades_Backgrounded_ForUI = null;

            lastFillerOnsetTS_NoPauses = -1;
            lastStimulusOnsetTS_NoPauses = -1;
            lastTaskRelevantQuestionTS_NoPauses = -1;
            lastTaskIrrelevantQuestionTS_NoPauses = -1;
            
            LogData_AtNextRenderedFrame_TS("SUBJECT_PERFORMANCE_REPORT", "Game_Start;{0}"._Format(levelToPrepare.GetWorldType_TS())); // [0] World Type]

            InitializeIndicies(levelToPrepare);

            // [SOS] First initialize LevelManager
            ExperimentManagerApplication.SESSION_GameManagers_PrepLevelManager(levelToPrepare, orthoSize, offset01);

            bool useProbes = ExperimentLibraryManager.Config.Probes.showProbes && !levelToPrepare.isTutorial_T && !levelToPrepare.isTutorial_I;
            BackgroundManager.RuntimeConfig background = new BackgroundManager.RuntimeConfig(Precalculator.gameTimings.backgroundPeaks);
            ProbeSystem.RuntimeConfig probeSystem = new ProbeSystem.RuntimeConfig(isFMRI ? ExperimentLibraryManager.Config.Probes.probeTimeout_Seconds : -1);
            
            // NS_TODO SUBCLASS
            if (levelToPrepare.isLocalizer)
            {
                StimulusManager_TaskRelevant.RuntimeConfig stimulus =
                    new StimulusManager_TaskRelevant.RuntimeConfig(background, levelToPrepare, Precalculator.gameTimings.stimulusOccurences_Actual);

                experimentManagerLevel_Persistent.PrepTaskRelevantGame(ExperimentLibraryManager.Config.Experiment,
                    new ExperimentManagerLevel.RuntimeConfig(false, stimulus, levelToPrepare,
                    PlayerProgression.levelManager.gameManager, PlayerProgression.levelManager, probeSystem, runtimeConfig.audioTone, eyeTracker));
            }
            else if (!levelToPrepare.isTutorial_I)
            {
                StimulusManager_TaskIrrelevant.RuntimeConfig stimulus =
                    new StimulusManager_TaskIrrelevant.RuntimeConfig(
                        background, levelToPrepare, useProbes, Precalculator.gameTimings.stimulusOccurences_Actual);

                experimentManagerLevel_Persistent.PrepTaskIrrelevantGame(ExperimentLibraryManager.Config.Experiment,
                    new ExperimentManagerLevel.RuntimeConfig(useProbes, stimulus, levelToPrepare,
                    PlayerProgression.levelManager.gameManager, PlayerProgression.levelManager, probeSystem, runtimeConfig.audioTone, eyeTracker));
            }
            else
            {
                experimentManagerLevel_Persistent.PrepSlides(ExperimentLibraryManager.Config.Experiment,
                    new ExperimentManagerLevel.RuntimeConfig(useProbes, null, levelToPrepare,
                    PlayerProgression.levelManager.gameManager, PlayerProgression.levelManager, probeSystem, runtimeConfig.audioTone, eyeTracker));
            }
            
            // Check if we need to do anything with EDFs
            string newFileName_NoSuffix = GetEyeTrackingFileName_NoSuffix(levelToPrepare);
            eyeTracker?.TryCreateTrackingFile(newFileName_NoSuffix, true);

            Pause(!isFMRI || FMRI_IsLevelFirstHalfOfRun(levelToPrepare)); // Toggle cursor for non-fmris and first halves

            // Start the eye tracking
            PauseEyeTracking(false);

            if (levelToPrepare.isTutorial_P)
                ClearTutorialSum();

            // Debug.Log(Camera.main.name, Camera.main);
        }

        private string TriggerMaster_CheckSend(TriggerOutEvent triggerOutEvent)
        {
            if (currentLevel?.isTutorial_I == true)
                return "TUTORIAL_I";

            if (doingSafeEndOfGame && triggerOutEvent != TriggerOutEvent.LevelEnd)
                return "SAFE_END_OF_GAME_AND_NOT_LEVEL_END_TRIGGER";

            return null;
        }

        private static void InitializeIndicies(LevelConfig levelConfig)
        {
            LevelConfig levelToLoadIndicesFrom;

            // Only practice needs to be randomized
            if (levelConfig.isTutorial_P)
            {
                /*
                // If we don't have any worlds, just set it to -1 to trigger default behavior
                int numWorlds = numGameWorlds;
                int levelIDToLoad_1Based = -1;

                if (numWorlds == 0)
                    levelIDToLoad_1Based = -1;
                else
                {
                    int randWorld = Utility_Helper.RandomRange(0, numWorlds);

                    int randLevelWithinWorld = Utility_Helper.RandomRange(0, numLevelsPerWorld);

                    levelIDToLoad_1Based = randWorld * numLevelsPerWorld + randLevelWithinWorld + 1;
                }

                levelToLoadIndicesFrom = LevelsLibrary.GetLevel(levelIDToLoad_1Based);
                LogData_AtNextRenderedFrame_TS("LEVEL_MASTER_MANAGER", "PRACTICE_LOADED;{0};[0] levelID_1Based"._Format(levelToLoadIndicesFrom.levelID_1Based));
                */
                levelToLoadIndicesFrom = LevelsLibrary.GetLevel(1);
                LogData_AtNextRenderedFrame_TS("LEVEL_MASTER_MANAGER", "PRACTICE_LOADED;{0}"._Format(levelToLoadIndicesFrom.levelID_1Based)); // [0] levelID_1Based
            }
            else
                // Set all indices to be at the current level
                levelToLoadIndicesFrom = levelConfig;

            SetAllIndices(levelToLoadIndicesFrom);
        }

        #endregion

        #region EyeTracking

        private static JsonArray<Vector2> eyeData_Gazes = new JsonArray<Vector2>();
        private static JsonArray<Vector4> eyeData_Sacades = new JsonArray<Vector4>();
        public static Sprite latestLevelHeatmap_Gaze_Backgrounded_ForUI { get; private set; }
        public static Sprite latestLevelHeatmap_Sacades_Backgrounded_ForUI { get; private set; }

        private IEnumerator WriteEyeTrackingInfoIE(Action<ProgressReport> callback)
        {
            callback?.Invoke(new ProgressReport(false, 0, "Writing Eye Data"));

            // ClearLevelTracking();
            bool gazeHeatmapComplete = false;
            bool sacadesHeatmapComplete = false;
            latestLevelHeatmap_Gaze_Backgrounded_ForUI = null;
            latestLevelHeatmap_Sacades_Backgrounded_ForUI = null;

            logger.LogEyeTrackingDetails(eyeData_Gazes, lastTracking_levelName, "GAZE", lastTracking_levelTimestamp);
            logger.LogEyeTrackingDetails(eyeData_Sacades, lastTracking_levelName, "SACADA", lastTracking_levelTimestamp);

            logger.LogHeatmap(eyeData_Gazes, true, lastTracking_levelName, lastTracking_levelTimestamp, heatmapSprite => { latestLevelHeatmap_Gaze_Backgrounded_ForUI = heatmapSprite; gazeHeatmapComplete = true; });
            logger.LogHeatmap(eyeData_Sacades, false, lastTracking_levelName, lastTracking_levelTimestamp, heatmapSprite => { latestLevelHeatmap_Sacades_Backgrounded_ForUI = heatmapSprite; sacadesHeatmapComplete = true; });

            while (!gazeHeatmapComplete || !sacadesHeatmapComplete)
                yield return null;

            eyeData_Gazes.array.Clear();
            eyeData_Sacades.array.Clear();

            callback?.Invoke(new ProgressReport(true, 1, "Finished Writing Eye Data"));
        }

        /// <summary>
        /// Cleanup - is it needed to be IE?
        /// [SOS] once, at post-level cleanup
        /// </summary>
        public IEnumerator StopTracking()
        {
            /*
            if (ApplicationLibrary.Config.Logging.debugLevel == LogDebuggingLevel.PerLevel)
            {
                Debug_Helper.LogError(typeof(SubjectPerformanceReport), "Stopping in-game tracking");
                DoFormattedReports();
            }
            */

            ClearLevelTracking();

            yield return null;
            yield return null;

            logger.StopBackgroundDataWriting();
        }

        private void ClearLevelTracking()
        {
            currentLevel = null;
        }

        public void AddTotalLevelTime(float elapsedTimeSeconds_Level_NoPauses)
        {
            if (currentLevel == null)
            {
                Debug_Helper.LogWarning(typeof(ExperimentManagerSession), "There is no tracking level to add to analytics");
                return;
            }

            switch (currentLevel.GetTaskType())
            {
                case TaskType.TaskIrrelevant:
                    journeySum.Run_JourneyRunTime_NoPauses += elapsedTimeSeconds_Level_NoPauses;
                    break;
                case TaskType.TaskRelevant:
                    session_probeSummary.replaySum.Run_ReplayRunTime_NoPauses += elapsedTimeSeconds_Level_NoPauses;
                    break;
            }
        }
        #endregion

        #region Details

        public static TaskIrrelevantResponseEvaluation EvaluateResponse(bool wasSomething, TaskIrrelevantResponse probeAnswer)
        {
            TaskIrrelevantResponseEvaluation responseEvaluation = TaskIrrelevantResponseEvaluation.FalseNegative;

            // There was something
            if (wasSomething)
            {
                // And we correctly (true) said correctly that there was (positive)
                if (probeAnswer == TaskIrrelevantResponse.Yes)
                    responseEvaluation = TaskIrrelevantResponseEvaluation.TruePositive;
                // But we incorrectly (false) said there wasn't (negative)
                else if (probeAnswer == TaskIrrelevantResponse.No)
                    responseEvaluation = TaskIrrelevantResponseEvaluation.FalseNegative;
                // Or we never responsed
                else if (probeAnswer == TaskIrrelevantResponse.NoResponse)
                    responseEvaluation = TaskIrrelevantResponseEvaluation.StimNoResponse;
                // Or we weren't certain
                else if (probeAnswer == TaskIrrelevantResponse.Maybe)
                    responseEvaluation = TaskIrrelevantResponseEvaluation.StimMaybe;
            }
            // There wasn't something
            else
            {
                // And we correctly (true) said there wasn't (negative)
                if (probeAnswer == TaskIrrelevantResponse.No)
                    responseEvaluation = TaskIrrelevantResponseEvaluation.TrueNegative;
                // But we incorrectly (false) said there was (positive)
                else if (probeAnswer == TaskIrrelevantResponse.Yes)
                    responseEvaluation = TaskIrrelevantResponseEvaluation.FalsePositive;
                // Or we never responsed
                else if (probeAnswer == TaskIrrelevantResponse.NoResponse)
                    responseEvaluation = TaskIrrelevantResponseEvaluation.BlankNoResponse;
                // Or we weren't certain
                else if (probeAnswer == TaskIrrelevantResponse.Maybe)
                    responseEvaluation = TaskIrrelevantResponseEvaluation.BlankMaybe;
            }

            return responseEvaluation;
        }

        public static void LogHeaders()
        {
            logger.LogFillerDetails(fillerDetailsHeader);
            logger.LogStimulusDetails(stimulusDetailsHeader);
            logger.LogLocalizerDetails(taskRelevantResponseHeader);
            logger.LogProbeDetail(taskIrrelevantResponseHeader);
        }

        private static int taskIrrelevantResponseID = 0;


        // Fillers
        private static string fillerDetailsHeader =
            "fillerOnsetTS;fillerOnsetTS_NoPauses;dTSinceLast_NoPauses;" +
            "currentLevelID;activeLevelID;activeWorldID;activeWorldType;" +
            "difficulty;averageDifficulty;performance;averagePerformance;" +
            "scorePercentile;averageStarsPerLevel;averageStarsInWorld;" +
            "animCycleID;type;duringSafeEndOfGame";

        private static double lastFillerOnsetTS_NoPauses = -1;

        public static void ReportFillerDetails(double fillerOnsetTS, double fillerOnsetTS_NoPauses,
                                            int currentLevelID, int activeLevelID, int activeWorldID, WorldType activeWorldType,
                                            float difficulty, float averageDifficulty, float performance, float averagePerformance,
                                            float scorePercentile, float averageStarsPerLevel, float averageStarsInWorld,
                                            int animCycleID, string type)
        {
            if (doingSafeEndOfGame)
            {
                LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(),
                    "FILLER", "EXTRA_FILLER", true);
                // return;
            }

            double dTSinceLast_NoPauses = lastFillerOnsetTS_NoPauses >= 0 ? fillerOnsetTS_NoPauses - lastFillerOnsetTS_NoPauses : 0;
            lastFillerOnsetTS_NoPauses = fillerOnsetTS_NoPauses;

            // Log ProbeDetails
            string fillerDetails = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16}",
                fillerOnsetTS, fillerOnsetTS_NoPauses, dTSinceLast_NoPauses > 0 ? ("" + dTSinceLast_NoPauses) : "",
                currentLevelID, activeLevelID, activeWorldID, activeWorldType,
                difficulty, averageDifficulty, performance, averagePerformance,
                scorePercentile, averageStarsPerLevel, averageStarsInWorld,
                animCycleID, type, doingSafeEndOfGame);

            if (currentLevel != null)
                logger.LogFillerDetails(fillerDetails);
        }

        // Stimuli
        private static string stimulusDetailsHeader =
            "stimulusOnsetTS;stimulusOnsetTS_NoPauses;dTSinceLast_NoPauses;" +
            "currentLevelID;activeLevelID;activeWorldID;activeWorldType;" +
            "difficulty;averageDifficulty;performance;averagePerformance;" +
            "scorePercentile;averageStarsPerLevel;averageStarsInWorld;" +
            "animCycleID;type;" +
            "stimulusID;stimulusType;stimulusName;stimulusLocation";

        private static double lastStimulusOnsetTS_NoPauses = -1;

        public static void ReportStimulusDetails(double stimulusOnsetTS, double stimulusOnsetTS_NoPauses,
                                            int currentLevelID, int activeLevelID, int activeWorldID, WorldType activeWorldType,
                                            float difficulty, float averageDifficulty, float performance, float averagePerformance,
                                            float scorePercentile, float averageStarsPerLevel, float averageStarsInWorld,
                                            int animCycleID, string type,
                                            int stimulusID, StimulusType stimulusType, string stimulusName, Direction_2D_Diagonal stimulusLocation)
        {
            if (doingSafeEndOfGame)
            {
                LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(),
                    "STIMULUS_DETAILS", "IGNORING_EXTRA_STIMULUS");
                return;
            }

            double dTSinceLast_NoPauses = lastStimulusOnsetTS_NoPauses >= 0 ? stimulusOnsetTS_NoPauses - lastStimulusOnsetTS_NoPauses : 0;
            lastStimulusOnsetTS_NoPauses = stimulusOnsetTS_NoPauses;

            // Log ProbeDetails
            string stimulusDetails = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16};{17};{18};{19}",
                stimulusOnsetTS, stimulusOnsetTS_NoPauses, dTSinceLast_NoPauses > 0 ? ("" + dTSinceLast_NoPauses) : "",
                currentLevelID, activeLevelID, activeWorldID, activeWorldType,
                difficulty, averageDifficulty, performance, averagePerformance,
                scorePercentile, averageStarsPerLevel, averageStarsInWorld,
                animCycleID, type,
                stimulusID, stimulusType, stimulusName, stimulusLocation);
            EDITOR_ONLY_CHECK_NAME_TYPE(stimulusName, stimulusType);

            if (currentLevel != null)
                logger.LogStimulusDetails(stimulusDetails);
        }

        private static string taskRelevantResponseHeader =
            "stimulusOnsetTS;stimulusOnsetTS_NoPauses;dTSinceLast_NoPauses;" +
            "currentLevelID;activeLevelID;activeWorldID;activeWorldType;" +
            "difficulty;averageDifficulty;performance;averagePerformance;" +
            "scorePercentile;averageStarsPerLevel;averageStarsInWorld;" +
            "animCycleID;type;" +
            "stimulusID;stimulusType;stimulusName;stimulusLocation;" +
            "questionID;questionTS;questionTS_NoPauses;windowEndTS;windowEndTS_NoPauses;" +
            "responseID;responseTS;responseTS_NoPauses;responseDT;response;responseEvaluation;repliedUsingHighAccu";

        private static double lastTaskRelevantQuestionTS_NoPauses = -1;

        public static void ReportTaskRelevantResponse(double stimulusOnsetTS, double stimulusOnsetTS_NoPauses,
                                            int currentLevelID, int activeLevelID, int activeWorldID, WorldType activeWorldType,
                                            float difficulty, float averageDifficulty, float performance, float averagePerformance,
                                            float scorePercentile, float averageStarsPerLevel, float averageStarsInWorld,
                                            int animCycleID,
                                            int stimulusID, StimulusType? stimulusType, string stimulusName, Direction_2D_Diagonal? stimulusLocation,
                                            double windowEndTS, double windowEndTS_NoPauses,
                                            double responseTS, double responseTS_NoPauses, double responseDT, StimulusReportTool.TaskRelevantResponse response, StimulusReportTool.TaskRelevantResponseEvaluation responseEvaluation, bool repliedUsingHighAccu)
        {
            if (doingSafeEndOfGame)
            {
                LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(), "LOCALIZER_DETAILS", 
                    string.Format("IGNORING_EXTRA_LOCALIZER;{0};{1}", responseTS, responseEvaluation), true); // [0] responseTS, [1] responseEvaluation
                return;
            }

            double questionTS = stimulusOnsetTS;
            double questionTS_NoPauses = stimulusOnsetTS_NoPauses;

            double dTSinceLast_NoPauses = lastTaskRelevantQuestionTS_NoPauses >= 0 ? questionTS_NoPauses - lastTaskRelevantQuestionTS_NoPauses : 0;
            lastTaskRelevantQuestionTS_NoPauses = questionTS_NoPauses;

            // Log ProbeDetails
            string localizerString = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16};{17};{18};{19};{20};{21};{22};{23};{24};{25};{26};{27};{28};{29};{30};{31}",
                stimulusOnsetTS, stimulusOnsetTS_NoPauses, dTSinceLast_NoPauses > 0 ? ("" + dTSinceLast_NoPauses) : "",
                currentLevelID, activeLevelID, activeWorldID, activeWorldType,
                difficulty, averageDifficulty, performance, averagePerformance,
                scorePercentile, averageStarsPerLevel, averageStarsInWorld,
                animCycleID, stimulusType == null ? "LOCALIZER_FILLER" : "LOCALIZER_STIMULUS",
                stimulusID, stimulusType == null ? "NULL" : stimulusType.ToString(), stimulusName, stimulusLocation.HasValue ? stimulusLocation.Value.ToString() : "-",
                stimulusID, questionTS, questionTS_NoPauses, windowEndTS, windowEndTS_NoPauses,
                taskRelevantResponseID, responseTS, responseTS_NoPauses, responseDT, response, responseEvaluation, repliedUsingHighAccu ? "HIGH_ACCU" : "NORMAL");

            // Debug.LogError(localizerString);
            EDITOR_ONLY_CHECK_NAME_TYPE(stimulusName, stimulusType);

            if (currentLevel != null)
                logger.LogLocalizerDetails(localizerString);

            // Log ProbeDetails
            LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(),
                "LOCALIZER_DETAILS", string.Format("RESPONSE_TO_STIMULUS;{0};{1}", responseTS, responseEvaluation)); // [0] responseTS, [1] responseEvaluation

            StimulusType levelTrackingStimulusType = StimulusManager.GetLevelStimulusTarget(currentLevel).target;

            // Log Summary
            if (levelTrackingStimulusType == StimulusType.Face)
            {
                switch (responseEvaluation)
                {
                    // [SOS] No player action doesnt mean a missed action, but depend on what missed
                    case StimulusReportTool.TaskRelevantResponseEvaluation.FalseNegative:
                        if (stimulusType == StimulusType.Face)
                            session_probeSummary.replaySum.TotalNumberOfMissesInFaceTarget++;
                        break;
                    case StimulusReportTool.TaskRelevantResponseEvaluation.TruePositive:
                        session_probeSummary.replaySum.TotalNumberOfHitsInFaceTarget++;
                        break;
                    case StimulusReportTool.TaskRelevantResponseEvaluation.FalsePositive:
                        session_probeSummary.replaySum.TotalNumberOfFalseAlarmsInFaceTarget++;
                        break;
                }
            }
            else if (levelTrackingStimulusType == StimulusType.Object)
            {
                switch (responseEvaluation)
                {
                    // [SOS] No player action doesnt mean a missed action, but depend on what missed
                    case StimulusReportTool.TaskRelevantResponseEvaluation.FalseNegative:
                        if (stimulusType == StimulusType.Object)
                            session_probeSummary.replaySum.TotalNumberOfMissesInObjectTarget++;
                        break;
                    case StimulusReportTool.TaskRelevantResponseEvaluation.TruePositive:
                        session_probeSummary.replaySum.TotalNumberOfHitsInObjectTarget++;
                        break;
                    case StimulusReportTool.TaskRelevantResponseEvaluation.FalsePositive:
                        session_probeSummary.replaySum.TotalNumberOfFalseAlarmsInObjectTarget++;
                        break;
                }
            }

            taskRelevantResponseID++;
        }

        private static string taskIrrelevantResponseHeader =
            "stimulusOnsetTS;stimulusOnsetTS_NoPauses;dTSinceLast_NoPauses;" +
            "currentLevelID;activeLevelID;activeWorldID;activeWorldType;" +
            "difficulty;averageDifficulty;performance;averagePerformance;" +
            "scorePercentile;averageStarsPerLevel;averageStarsInWorld;" +
            "animCycleID;type;" +
            "stimulusID;stimulusType;stimulusName;stimulusLocation;" +
            "questionID;questionTS;questionTS_NoPauses;windowEndTS;windowEndTS_NoPauses;" +
            "responseID;responseTS;responseTS_NoPauses;responseDT;response;responseEvaluation;repliedUsingHighAccu";

        private static double lastTaskIrrelevantQuestionTS_NoPauses = -1;

        public static void ReportTaskIrrelevantResponse_NotTS(double stimulusOnsetTS, double stimulusOnsetTS_NoPauses,
                                            int currentLevelID, int activeLevelID, int activeWorldID, WorldType activeWorldType,
                                            float difficulty, float averageDifficulty, float performance, float averagePerformance,
                                            float scorePercentile, float averageStarsPerLevel, float averageStarsInWorld,
                                            int animCycleID,
                                            int stimulusID, StimulusType stimulusType, string stimulusName, Direction_2D_Diagonal stimulusLocation,
                                            int probeID, double probeTS, double probeTS_NoPauses, double windowEndTS, double windowEndTS_NoPauses,
                                            double responseTS, double responseTS_NoPauses, double responseDT, TaskIrrelevantResponse response, bool repliedUsingHighAccu)
        {
            if (doingSafeEndOfGame)
            {
                LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(), "PROBE_DETAILS",
                    string.Format("IGNORING_EXTRA_PROBE;{0}", responseTS), true); // [0] responseTS
                return;
            }

            bool wasSomething = stimulusType != StimulusType.None;
            TaskIrrelevantResponseEvaluation responseEvaluation = EvaluateResponse(wasSomething, response);

            double questionTS = probeTS;
            double questionTS_NoPauses = probeTS_NoPauses;

            double dTSinceLast_NoPauses = lastTaskIrrelevantQuestionTS_NoPauses >= 0 ? questionTS_NoPauses - lastTaskIrrelevantQuestionTS_NoPauses : 0;
            lastTaskIrrelevantQuestionTS_NoPauses = questionTS_NoPauses;

            // Log ProbeDetails
            string probeDetailsLogs = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16};{17};{18};{19};{20};{21};{22};{23};{24};{25};{26};{27};{28};{29};{30};{31}",
                stimulusOnsetTS, stimulusOnsetTS_NoPauses, dTSinceLast_NoPauses > 0 ? ("" + dTSinceLast_NoPauses) : "",
                currentLevelID, activeLevelID, activeWorldID, activeWorldType,
                difficulty, averageDifficulty, performance, averagePerformance,
                scorePercentile, averageStarsPerLevel, averageStarsInWorld,
                animCycleID, "GAME_PROBE",
                stimulusID, stimulusType, stimulusName, stimulusLocation,
                probeID, questionTS, questionTS_NoPauses, windowEndTS, windowEndTS_NoPauses,
                taskIrrelevantResponseID, responseTS, responseTS_NoPauses, responseDT, response, responseEvaluation, repliedUsingHighAccu ? "HIGH_ACCU" : "NORMAL");

            EDITOR_ONLY_CHECK_NAME_TYPE(stimulusName, stimulusType);

            if (currentLevel != null)
                logger.LogProbeDetail(probeDetailsLogs);

            // Which world are we in?
            if (activeWorldID >= 0)
                session_probeSummary.worldSum.LogNewProbe(activeWorldID);

            // Log Summary
            journeySum.TotalNumberOfProbes++;
            switch (stimulusType)
            {
                case StimulusType.None:

                    journeySum.TotalNumberOfBlanksProbed++;

                    switch (response)
                    {
                        case TaskIrrelevantResponse.Yes:
                            journeySum.TotalNumberOfBlanksFalseAlarms++;
                            break;
                        case TaskIrrelevantResponse.No:
                            journeySum.TotalNumberOfBlanksCorrectlyUnseen++;
                            break;
                        case TaskIrrelevantResponse.Maybe:
                            journeySum.TotalNumberOfBlanksMaybe++;
                            break;
                        case TaskIrrelevantResponse.NoResponse:
                            journeySum.TotalNumberOfBlanksNoResponse++;
                            break;
                    }

                    break;
                case StimulusType.Object:
                    journeySum.TotalNumberOfObjectsProbed++;

                    switch (response)
                    {
                        case TaskIrrelevantResponse.Yes:
                            journeySum.TotalNumberObjectsSeen++;
                            journeySum.TotalNumberOfAllStimuliSeen++;
                            break;
                        case TaskIrrelevantResponse.No:
                            journeySum.TotalNumberObjectsUnseen++;
                            journeySum.TotalNumberOfAllStimuliUnseen++;
                            break;
                        case TaskIrrelevantResponse.Maybe:
                            journeySum.TotalNumberObjectsMaybe++;
                            journeySum.TotalNumberOfAllStimuliMaybe++;
                            break;
                        case TaskIrrelevantResponse.NoResponse:
                            journeySum.TotalNumberObjectsNoResponse++;
                            journeySum.TotalNumberOfAllStimuliNoResponse++;
                            break;
                    }

                    break;
                case StimulusType.Face:
                    journeySum.TotalNumberOfFacesProbed++;

                    switch (response)
                    {
                        case TaskIrrelevantResponse.Yes:
                            journeySum.TotalNumberFacesSeen++;
                            journeySum.TotalNumberOfAllStimuliSeen++;
                            break;
                        case TaskIrrelevantResponse.No:
                            journeySum.TotalNumberFacesUnseen++;
                            journeySum.TotalNumberOfAllStimuliUnseen++;
                            break;
                        case TaskIrrelevantResponse.Maybe:
                            journeySum.TotalNumberFacesMaybe++;
                            journeySum.TotalNumberOfAllStimuliMaybe++;
                            break;
                        case TaskIrrelevantResponse.NoResponse:
                            journeySum.TotalNumberFacesNoResponse++;
                            journeySum.TotalNumberOfAllStimuliNoResponse++;
                            break;
                    }

                    break;
            }

            Debug_Helper.Log(typeof(ExperimentManagerSession), journeySum);

            // Exclude tutorial from summary
            if (activeWorldID < 0)
                return;

            // Are we done?
            if (session_probeSummary.worldSum.GetNumberProbes(activeWorldID) >=
                GetWantedNumProbesPerWorld())
            {
                Debug_Helper.LogWarning(typeof(ExperimentManagerSession), "Collected enough probes for this world!");
                onProbesComplete?.Invoke(null, activeWorldID);
            }

            taskIrrelevantResponseID++;
        }

        // Replay
        public static void LogStimulusShown(StimulusType stimulusType)
        {
            if (currentLevel == null) return;
            // Only for replays!
            if (currentLevel.GetTaskType() != TaskType.TaskRelevant) return;

            StimulusType levelTrackingStimulusType = StimulusManager.GetLevelStimulusTarget(currentLevel).target;

            // Log Summary
            bool done = false;

            if (levelTrackingStimulusType == StimulusType.Face)
            {
                switch (stimulusType)
                {
                    case StimulusType.None:
                        session_probeSummary.replaySum.TotalNumberOfBlanksPresentedInFaceTarget++;
                        break;
                    case StimulusType.Object:
                        session_probeSummary.replaySum.TotalNumberOfObjectsPresentedInFaceTarget++;
                        break;
                    case StimulusType.Face:
                        session_probeSummary.replaySum.TotalNumberOfFacesPresentedInFaceTarget++;
                        break;
                }

                done = session_probeSummary.replaySum.TotalNumberOfStimuliPresentedInFaceTarget >= wantedNumStimuli_AllLocalizers;
            }
            else if (levelTrackingStimulusType == StimulusType.Object)
            {
                switch (stimulusType)
                {
                    case StimulusType.None:
                        session_probeSummary.replaySum.TotalNumberOfBlanksPresentedInObjectTarget++;
                        break;
                    case StimulusType.Object:
                        session_probeSummary.replaySum.TotalNumberOfObjectsPresentedInObjectTarget++;
                        break;
                    case StimulusType.Face:
                        session_probeSummary.replaySum.TotalNumberOfFacesPresentedInObjectTarget++;
                        break;
                }

                done = session_probeSummary.replaySum.TotalNumberOfStimuliPresentedInObjectTarget >= wantedNumStimuli_AllLocalizers;
            }

            // Are we done?
            if (done)
            {
                Debug_Helper.LogWarning(typeof(ExperimentManagerSession), "Collected enough probes for this world!");
                onProbesComplete?.Invoke(null, -1);
            }
        }

        internal void Mute(bool arg0)
        {
            SoundSystem.Mute(arg0, "MUTE_TOGGLE_UI");
        }

        private static int taskRelevantResponseID = 0;

        #endregion

        public bool wantToLog { get { return _wantToLog; } set { _wantToLog = value; } }
        private static bool _wantToLog;

        private static bool doLogs
        {
            get
            {
                return _wantToLog && currentLevel != null;
            }
        }

        public static bool isWriting { get { return ExperimentLogger.isWriting; } }
        public static bool EXPERIMENTER_AUTO_ANSWER { get; private set; }

        #region Eye Tracking Helpers

        public EyeTrackerManager_Base eyeTracker { get { return runtimeConfig.eyeTracker; } }
        private EyeTracker_UI eyeTrackerUI { get { return runtimeConfig.eyeTrackerUI; } }

        public bool GetEyeTrackingFileExists(LevelConfig level)
        {
            string eyeTrackingFileName_NoSuffix = GetEyeTrackingFileName_NoSuffix(level);
            string eyeTrackingFullPath = eyeTracker.GetFullEyeTrackingFilePath_FromFileName(eyeTrackingFileName_NoSuffix);
            // Debug.Log(eyeTrackingFullPath);
            return FileWrapper.FileExists(eyeTrackingFullPath);
        }

        public enum PauseReason { ManualPausing, Other }

        internal void PauseExperiment(bool pause, bool toggleCursor = true, bool keepMusicPlaying = false, PauseReason pauseReason = PauseReason.Other)
        {
            // Is it running?
            if (PlayerProgression.isLevelStarted != true)
            {
                this.LogWarning("Tried pausing / unpausing before the game even started ; aborting");
                return;
            }

            // Is it running?
            if (PlayerProgression.hasLevelEnded == true)
            {
                this.LogWarning("Tried pausing / unpausing while the game had ended ; aborting");
                return;
            }

            if (pause)
                Pause(toggleCursor, keepMusicPlaying);
            else
                UnPause();

            // [200721] TEMP, HACKY (should be inside session manager)
            if (pauseReason == PauseReason.ManualPausing)
            {
                this.LogWarning("Manual pausing");
                PauseEyeTracking(pause);
            }
        }

        public string GetEyeTrackingFileName_NoSuffix(LevelConfig level)
        {
            if (level == null) return "";

            // Up to 8 characters for EYELINK at least!
            // We'll use 2 of those for the world / level info
            string shortID = subjectID.Length > 6 ? subjectID.Substring(0, 6) : subjectID;

            string worldID_1Based = level.isLocalizer ?
                GetIDOfLocalizerBlock_String(level) :// "A" or "B"
                (level.worldID + 1).ToString();                 // "0" (practice) or "1" / "2" / "3" / "4"

            int levelID_1Based =
                (level.isLocalizer ?
                GetIDOfLocalizerWithinBlock_0Based(level) :
                level.levelID_WithinWorld) + 1;

            string fileName = "";

            switch (ExperimentLibraryManager.Config.EyeTracking.GetWritingStrategy(module))
            {
                case WritingStrategy.AllInOne:
                    fileName = "XXXXXXXX";
                    break;
                case WritingStrategy.PerLevel:
                    fileName = "{0}{1}{2}"._Format(shortID, worldID_1Based, levelID_1Based);
                    break;
                case WritingStrategy.PerTwoLevels:

                    int baseLevelID = levelID_1Based;

                    // If it's an even level
                    if (levelID_1Based % 2 == 0)
                        // we want the one before it!
                        baseLevelID = levelID_1Based - 1;

                    fileName = "{0}{1}{2}"._Format(shortID, worldID_1Based, baseLevelID);
                    break;
                case WritingStrategy.PerWorld:
                    fileName = "{0}{1}X"._Format(shortID, worldID_1Based);
                    break;
                default:
                    break;
            }

            if (fileName.Length > 8)
                fileName = fileName.Substring(0, 8);

            return fileName;
        }

        internal void RenameFullLogs(string levelName, bool aborted)
        {
            logger.RenameFullLogs(levelName, aborted);
        }

        private static void EDITOR_ONLY_CHECK_NAME_TYPE(string stimulusName, StimulusType? stimulusType)
        {
            if (stimulusType == null)
            {
#if UNITY_EDITOR
                // Debug.Log("LOCALIZER-OK (NO TYPE, OUT OF WINDOW) :: " + stimulusName);
#endif
            }
            else if (stimulusName == null)
            {
#if UNITY_EDITOR
                Debug.Log("Weird?");
#endif
            }
            else if (stimulusName.Contains("SF") && stimulusType == StimulusType.Face)
            {
#if UNITY_EDITOR
                // Debug.Log("FACE-OK :: " + stimulusName);
#endif
            }
            else if (stimulusName.Contains("SO") && stimulusType == StimulusType.Object)
            {
#if UNITY_EDITOR
                // Debug.Log("OBJECT-OK :: " + stimulusName);
#endif
            }
            else if (stimulusName == StimulusManager.EMPTY_STIM_NAME && stimulusType == StimulusType.None)
            {
#if UNITY_EDITOR
                // Debug.Log("NULL OK");
#endif
            }
            else
            {
                Debug.LogError(stimulusName + " -> " + stimulusType);
                LogSessionInfo("[Error With Stimuli Name Mismatch] :: " + stimulusName + ", " + stimulusType);
            }
        }

        #endregion


        #region Logging Bridge

        public void ToggleEyeTrackingUI(bool on)
        {
            eyeTrackerUI?.Toggle(on);
        }

        /// Called when we are back at level selection
        public void PostLevel_WriteCacheToFile(LevelConfig levelToWrite, bool aborted, Action<ProgressReport> progressReport)
        {
            EngineWrapper.StartCoroutine(PostLevel_WriteCacheToFileIE(levelToWrite, aborted, progressReport));
        }

        private IEnumerator PostLevel_WriteCacheToFileIE(LevelConfig levelToWrite, bool aborted, Action<ProgressReport> callback)
        {
            Action<ProgressReport> Report = report =>
            {
                if (config.doEndOfLevelLoggingReport)
                    logger.LogLoggerDump("{0};{1};{2};{3}"._Format(TimeWrapper.GetCurrentTimestamp_TS(), report.value01.PercentileToPercent(), report.isDone, report.message));

                callback(report);
            };

            Report(new ProgressReport(false, 0, "Begun Writing Leftovers"));

            // Transfer EDF
            {
                WritingStrategy writingStrategy =
                    ExperimentLibraryManager.Config.EyeTracking.GetWritingStrategy(module);

                bool shouldFinalize =
                    writingStrategy == WritingStrategy.PerLevel ||
                    (writingStrategy == WritingStrategy.PerTwoLevels &&
                        (!FMRI_IsLevelFirstHalfOfRun(levelToWrite) || levelToWrite.isTutorial_P)) ||
                    (writingStrategy == WritingStrategy.PerWorld &&
                        (IsLastLevelOfLocalizerBlock(levelToWrite) || PlayerProgression.IsLastLevelOfWorld(levelToWrite)));

                bool shouldStopWriting = 
                    shouldFinalize ||   // Always stop writing when we want to finalize EDF
                    !isFMRI ||          // Always stop if we're not in FMRI mode
                    !FMRI_IsLevelFirstHalfOfRun(levelToWrite);  // Also stop if we're in FMRI 2nd half (but not first)

                // Stop the eye tracking (only for non-fmri or 2nd halves of runs)
                if (shouldStopWriting)
                {
                    PauseEyeTracking(true);
                    Report(new ProgressReport(false, 1 / 6f, "Stopped Eye Tracking"));
                    yield return null;
                }


                if (shouldFinalize)
                {
                    // Finalize
                    eyeTracker?.FinalizeEyeTrackingFile();
                    Report(new ProgressReport(false, 1 / 5f, "Finalized Eye Tracking File"));
                    yield return null;
                }

                Report(new ProgressReport(false, 1 / 3f, "Done with Eye Tracking Device"));
            }

            /// Write cached text to file
            {
                bool isDone = false;

                logger.WriteLeftoverBackgroundData(loggerProgressReport =>
                {
                    Report(new ProgressReport(false, loggerProgressReport.value01 / 3 + 1 / 3f - 0.001f, "Leftover Background Data"));
                    isDone = loggerProgressReport.isDone;

                    // Debug.LogWarning(progressReport.message);
                });

                yield return new WaitUntil(() => isDone);

                Report(new ProgressReport(false, 2 / 3f, "Done with Cached Full Logs"));
            }

            // Experiment stuff
            if (aborted)
                this.LogWarning("Didn't write analytics ; was aborted");

            else if (isFMRI && FMRI_IsLevelFirstHalfOfRun(levelToWrite.levelID_1Based))
                this.LogWarning("Didn't write analytics ; FMRI 1st half of run");

            else
            {
                session_analytics.Save(LogExperimentSessionAnalytics);
                LogProbeSummaries();
            }

            // Time for Eye Data
            {
                bool isDone = false;

                WriteEyeTrackingInfo(eyeProgressReport =>
                {
                    Report(new ProgressReport(false, eyeProgressReport.value01 / 3 + 2 / 3f - 0.001f, "Eye Tracking Data"));
                    isDone = eyeProgressReport.isDone;

                    // Debug.LogWarning(progressReport.message);
                });

                yield return new WaitUntil(() => isDone);

                Report(new ProgressReport(false, 1f, "Done with Eye Tracking Data"));
            }

            Report(new ProgressReport(true, 1, "All Good"));
        }

        public static void ReportSubjectDetails()
        {
            string details = "Field Name; Field Value";
            details += Environment.NewLine + string.Format("ID;{0}", session_probeSummary.subject.id);
            foreach (SubjectField sF in session_probeSummary.subject.subjectFields)
                details += Environment.NewLine + string.Format("{0};{1}", sF.name, sF.answer);

            // Dont use this in a foreach
            logger.LogSubjectDetails(details);
        }

        /// <summary>
        /// [SOS] First do: Subject info, System set, Localizer randomization
        /// </summary>
        public void LogSessionStart()
        {
            // === USER / SUBJECT
            string msg = "----\n\nSession ({0}) Started for user: {1}"._Format(module, subjectID);

            //LogProbeDetail("Localizer 1 Set to {0}, Localizer 2 Set to {1}, Localizer 3 Set to {2}, Localizer 4 Set to {3}"._Format(
            //    LevelMasterManager_Localizer.Games_Types[0], LevelMasterManager_Localizer.Games_Types[1],
            //    LevelMasterManager_Localizer.Games_Types[2], LevelMasterManager_Localizer.Games_Types[3]
            //    ), false);

            // Nothing to do if we obscure both date and time!
            if (config.obscureDateInGeneralInfo && config.obscureTimeInGeneralInfo) { }
            else
            {
                string dateTimeSTR = "";

                if (!config.obscureDateInGeneralInfo)
                    dateTimeSTR += " " + DateTime.UtcNow.ToString("yyyy-MM-dd");
                if (!config.obscureTimeInGeneralInfo)
                    dateTimeSTR += " " + DateTime.UtcNow.ToString("HH:mm:ss");

                msg += " at{0} UTC"._Format(dateTimeSTR);
            }

            // === SUBJECT FIELDS
            msg += "\n\n{0}"._Format(session_probeSummary.subject.subjectFields.ToReadableString("User Description"));

            // === SEQUENCES
            msg += "\n\nSequence loaded :: {0}"._Format(runtimeConfig.originalSequenceFolderpath.Split(true, "/").GetLast());

            msg += "\n\nScreen Pixel dimensions :: {0}x{1}"._Format(Screen.width, Screen.height);

            LogSessionInfo(msg);
        }

        /*
        public void LogLocalizers()
        {
            string msg = "";
            LogSessionInfo("Localizer 1 Set to {0}s, Localizer 2 Set to {1}s, Localizer 3 Set to {2}s, Localizer 4 Set to {3}s, Localizer 5 Set to {4}s, Localizer 6 Set to {5}s, Localizer 7 Set to {6}s, Localizer 8 Set to {7}s"._Format(
                StimulusManager.StimulusTypes[0], StimulusManager.StimulusTypes[1],
                StimulusManager.StimulusTypes[2], StimulusManager.StimulusTypes[3],
                StimulusManager.StimulusTypes[4], StimulusManager.StimulusTypes[5],
                StimulusManager.StimulusTypes[6], StimulusManager.StimulusTypes[7]));
        }
        */

        public void LogProbeSummaries()
        {
            lock (session_probeSummary)
            {
                string probeSummaryJSON_Game = JsonUtility.ToJson(GetJourneySummary(false), true);
                string probeSummaryJSON_Replay = JsonUtility.ToJson(session_probeSummary.replaySum, true);

                logger.LogProbeSummary_Game(probeSummaryJSON_Game);
                logger.LogProbeSummary_Replay(probeSummaryJSON_Replay);
            }

            GC.Collect();
        }

        public static void LogSessionInfo(string text, bool debug = false)
        {
            // If probe is null here, it will throw errors - but no one should call it before that
            string timePrefix = "{0} :: "._Format(TimeWrapper.currentTimestampMS.ToString("#"));

            logger.LogSessionInfo(timePrefix + text, debug);
        }

        public void LogFMRIDump(string info)
        {
            // Debug.LogError(info);

            logger.LogFMRIDump(info);
        }

        public void Dispose()
        {
            // StopTracking();
            ClearLevelTracking();
            UnsubscribeStatic();

            session_probeSummary = null;
            session_analytics.ResetTrackingSession(LogExperimentSessionAnalytics);
            session_shownTutorialTip = false;

            experimentManagerLevel_Persistent.Dispose();
            eyeTracker?.Dispose();
            logger.Dispose();

            config = null;
            runtimeConfig = null;

            experimentManagerLevel_Persistent = null;
            logger = null;
        }

        private void UnsubscribeStatic()
        {
            // 1. PERIPHERALS
            PhotoDiodeDebugger.onStatusUpdate -= PDB_onStatusUpdate;

            LPTManager.onStatusUpdate -= LPTM_onStatusUpdate;

            InputManager.onKeyStatusUpdate -= IM_onKeyStatusUpdate;
            InputManager.onConnectionStatusUpdate -= IM_onConnectionStatusUpdate;
            InputManager.onActivityStatusUpdate -= IM_onActivityStatusUpdate;

            HighAccuracyInput_Base.onKeyStatusUpdate -= HAIB_onKeyStatusUpdate;
            HighAccuracyInput_Base.onStatusUpdate -= HAIB_onStatusUpdate;

            LoggingTester.onReportReady -= LoggingTester_onReportReady;

            SoundSystem.onStatusUpdate_AudioSource -= SS_onStatusUpdate_AudioSource;
            SoundSystem.onStatusUpdate_Mute -= SS_onStatusUpdate_Mute;

            EyeTrackerManager_Base.onGazeUpdated -= EyeTracker_onGazeUpdated;
            EyeTrackerManager_Base.onSacadaEnd -= EyeTracker_onSacadaEnd;
            EyeTrackerManager_Base.onBlinkEnd -= EyeTracker_onBlinkEnd;
            EyeTrackerManager_Base.onBlinkInfo -= EyeTracker_onBlinkInfo;
            EyeTrackerManager_Base.onTrackingStateChanged -= EyeTracker_onTrackerStateChanged;
            EyeTrackerManager_Base.onMessageFailed_Thread -= EyeTracker_onMessageFailed_TS;
            EyeTrackerManager_Base.onMessageWritten_Thread -= EyeTracker_onMessageWritten_TS;
            EyeTrackerManager_Base.onCommandFailed -= EyeTracker_onCommandFailed;
            EyeTrackerManager_Base.onCommandSent -= EyeTracker_onCommandSent;
            EyeTrackerManager_Base.onDebugInfo -= EyeTracker_onDebugInfo;
            EyeTrackerManager_Base.onCalibrateProgressReport -= EyeTracker_onCalibrateProgressReport;

            SerialPortManager.onDebugInfo -= SPM_onDebugInfo;

            Tobii.Research.Unity.EyeTracker.onTimeSyncRefRcv -= EyeTracker_onTimeSyncRefRcv;

            // 2. GAME
            Interactable.onStatusUpdate -= Interactable_onStatusUpdate;
            PlayerView_Runner.onStatusUpdate -= PlayerView_Runner_onStatusUpdate;
            PlayerModel.onStatusUpdate -= PlayerModel_onStatusUpdate;

            BackgroundObject_Abstract_RotatingSquares.onStatusUpdate -= BOAR_onStatusUpdate;
            BackgroundManager.onStatusUpdate -= BackgroundManager_onStatusUpdate;

            ReplaySystem.onErrorMessage -= ReplaySystem_onErrorMessage;
            ReplaySystem.onStatusUpdate -= ReplaySystem_onStatusUpdate;

            PlayerProgression.onLevelComplete -= PlayerProgression_onLevelComplete;
            TimeWrapper.onDebugMessageSent -= TimeWrapper_onDebugMessageSent;
        }

        #endregion

        #region Helper

        public static LevelConfig GetRecordedReplayLevel(LevelConfig localizerLevel)
        {
            int replayLevelID_1Based = GetRecordedReplayID(localizerLevel);//, isFMRI);

            if (replayLevelID_1Based < 0)
            {
                // Debug.LogError("No replay could be loaded. Crashing now.");
                return null;
            }

            return LevelsLibrary.GetLevel(replayLevelID_1Based);
        }

        public static int GetRecordedReplayID(LevelConfig localizerLevel)
        {
            int wantedLevelID_1Based = StimulusManager.GetWantedReplayInfo(localizerLevel).targetLevelID_1Based; // , ExperimentManagerSession.isFMRI
            LevelConfig wantedLevelConfig = LevelsLibrary.GetLevel(wantedLevelID_1Based);
            return ReplaySystem.GetRecordedReplayID(wantedLevelConfig.levelName);
        }

        public static bool IsIntendedForReplay(int gameLevelID_1Based)
        {
            // Is our ID part of any replay targets?
            return GetIntendedLocalizerInfoForReplaying(gameLevelID_1Based).targetLevelID_1Based >= 0;
        }

        public static LocalizerInfo GetIntendedLocalizerInfoForReplaying(int gameLevelID_1Based)
        {
            // Is our ID part of any replay targets?
            foreach (LocalizerInfo lI in StimulusManager.localizerInfos)
                if (lI.targetLevelID_1Based == gameLevelID_1Based)
                    return lI;
            return LocalizerInfo.EMPTY;
        }

        #endregion

        // CLEANUP Should not be static
        private static bool doingSafeEndOfGame = false;

        private Coroutine safeEndOFGameCR;

        private void SafeEndOfGame(StimulusReportTool.TaskRelevantResponseEvaluation e)
        {
            if (safeEndOFGameCR != null)
            {
                this.LogWarning("Already doing safe end of game!");
                LogData_AtNextRenderedFrame_TS("ExperimentManagerSession", "SAFE_END_OF_GAME_PREVENTED;{0};ALREADY_DOING"._Format(e));
                return;
            }

            safeEndOFGameCR = EngineWrapper.StartCoroutine(SafeEndOfGameIE(e));
        }

        private IEnumerator SafeEndOfGameIE(StimulusReportTool.TaskRelevantResponseEvaluation e)
        {
            doingSafeEndOfGame = true;
            LogData_AtNextRenderedFrame_TS("ExperimentManagerSession", "SAFE_END_OF_GAME_STARTED;{0}"._Format(e));

            this.LogWarning("Started Safe End of Game!");

            if (PlayerProgression.IsLastLevelOfGame(currentLevel))
                ResetTimeScale(false);

            float extraWaitSeconds = ExperimentLibraryManager.Config.Experiment.stimulus.extraDelayMS_LocalizerEnd / 1000f;

            yield return new WaitForSeconds(extraWaitSeconds);

            this.LogWarning("Finished Safe End of Game!");

            LogData_AtNextRenderedFrame_TS("ExperimentManagerSession", "SAFE_END_OF_GAME_FINISHED;{0}"._Format(e));

            PlayerProgression.CompleteCurrentLevel();

            // Free the resource
            safeEndOFGameCR = null;
        }

        #region Time Scale

        private static bool? oldWantToLog = null;
        internal static TriggerMaster triggerMaster;

        public void SetTimeScaleFast(bool superFast)
        {
            EXPERIMENTER_AUTO_ANSWER = true;
            TimeWrapper.timeScale_NotTS = superFast ? config.timescale.timeScaleSuperFast : config.timescale.timeScaleFast;

            if (superFast || !config.timescale.allowLogsFast)
            {
                oldWantToLog = wantToLog;
                wantToLog = false;
            }
            else if (oldWantToLog.HasValue)
                wantToLog = oldWantToLog.Value;

            LogSessionInfo("Time Scale -> {0}x (Auto-Answer :: {1})"._Format(TimeWrapper.timeScale_NotTS, EXPERIMENTER_AUTO_ANSWER));
        }

        public void ResetTimeScale(bool doAutoAnswer)
        {
            EXPERIMENTER_AUTO_ANSWER = doAutoAnswer;
            TimeWrapper.timeScale_NotTS = 1;
            if (oldWantToLog.HasValue)
                wantToLog = oldWantToLog.Value;
            LogSessionInfo("Time Scale -> {0}x (Auto-Answer :: {1})"._Format(TimeWrapper.timeScale_NotTS, EXPERIMENTER_AUTO_ANSWER));
        }

        public void SetTimeScaleSlow(bool superSlow)
        {
            EXPERIMENTER_AUTO_ANSWER = false;
            TimeWrapper.timeScale_NotTS = superSlow ? config.timescale.timeScaleSuperSlow : config.timescale.timeScaleSlow;
            if (oldWantToLog.HasValue)
                wantToLog = oldWantToLog.Value;
            LogSessionInfo("Time Scale -> {0}x (Auto-Answer :: {1})"._Format(TimeWrapper.timeScale_NotTS, EXPERIMENTER_AUTO_ANSWER));
        }

        #endregion
    }

    [Serializable]
    public class TimescaleConfig
    {
        public float timeScaleSuperFast = 25;
        public float timeScaleFast = 3;
        public bool allowLogsFast = true;
        public float timeScaleSlow = 0.25f;
        public float timeScaleSuperSlow = 0.025f;
    }

    public enum TaskIrrelevantResponseEvaluation
    {
        Unknown = 0,
        FalsePositive = 1,
        FalseNegative = 2,
        TruePositive = 3,
        TrueNegative = 4,
        StimNoResponse = 5,
        BlankNoResponse = 6,
        StimMaybe = 7,
        BlankMaybe = 8,
    }
}