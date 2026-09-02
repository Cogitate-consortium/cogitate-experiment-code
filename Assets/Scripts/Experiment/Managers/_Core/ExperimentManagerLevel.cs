// NS_REMOVE | Higher level should handle this
using Experiment.Analytics;

// NS_DEBATABLE | Maybe a Session Manager
using Game.Managers.SessionManagers;

// NS_DEBATABLE | Too deep?
using Game.Systems.Adaptive;

// NS_DEBATABLE | Maybe a Task Manager ?
using Experiment.Task.UI;
// NS_DEBATABLE | Maybe a Task Manager ?
using Experiment.Task.Core;

// NS_DEBATABLE | Maybe a Task Manager ?
using Game.Managers.LevelManagers;
// NS_DEBATABLE | Maybe a Task Manager ?
using Game.Managers.GameplayManager;

using ExperimentLibrary;
using Experiment.Stimulus;
using Experiment.Task;
using Experiment.Triggers;
using Game.Core; // LEVELS ONLY
using Helpers.Async;
using Helpers.Engine;
using Peripherals.EyeTracking;
using System;
using TGP.Helpers;
using UnityEngine;
using Peripherals.Audio; // EXPERIMENT-RELATED SOUNDS ONLY
using Game.Systems.Score;
using Experiment.Background; // | Classes only

namespace Experiment.Managers.Core
{
    /// <summary>
    /// NS_SEGMENT (Session and Level, maybe also Task? Or is Task Level)
    /// NS_SUBCLASS (Level part) to Task Rel / Irrel
    /// </summary>
    public class ExperimentManagerLevel : IDisposable
    {
        // HACK
        private LevelConfig GetReplayLevel(LevelConfig level)
        {
            return ExperimentManagerSession.GetRecordedReplayLevel(level);
        }

        private ModuleType module { get { return ExperimentManagerSession.module; } }
        private ExperimentAnalyticsSession session_analytics { get { return ExperimentManagerSession.session_analytics; } }
        private TriggerMaster triggerMaster { get { return ExperimentManagerSession.triggerMaster; } }
        public StimulusManager stimulusManager; // SHOULD BE PRIVATE
        protected StimulusReportTool reportTool;
        protected ProbeManager probeManager;
        private LevelConfig levelConfig { get { return runtimeConfig.levelConfig; } }
        private Config config;
        private RuntimeConfig runtimeConfig;
        private bool replay_hasShownLastStimulus;

        private void Prep(Config config, RuntimeConfig runtimeConfig, StimulusManager stimulusManager)
        {
            this.config = config;
            this.runtimeConfig = runtimeConfig;
            // [HACK], should be handled via subclassing
            this.stimulusManager = stimulusManager;

            SoundSystem.InitializeAudioTone(ExperimentLibraryManager.Config.Audio.FixationBreak, runtimeConfig.audioTone);

            // -- STIMULUS
            if (stimulusManager != null)
            {
                stimulusManager.onStimulusOnset += StimulusManager_onStimulusOnset;
                stimulusManager.onAnimEvent += StimulusManager_onAnimEvent;
            }
            else
            {
                this.LogWarning("No stimulus manager");
            }

            // eyeTrackerManager = Utility_Helper.GetComponentInScene<EyeTrackerManager_Base>();

            if (runtimeConfig.eyeTracker == null)
                this.LogError("Could not find Eye Tracker Manager!");

            session_analytics.StartTrackingLevel(runtimeConfig.levelConfig);

            // Show Cross regardless of whether we use probes or not
            ProbeSystem.Initialize(runtimeConfig.probeSystem);
        }

        [Serializable]
        public class Config
        {
            public StimulusManager.Config stimulus;
            public bool GetAllowProceedWithFailure(PrepVsFull prepVsFull)
            {
                return prepVsFull == PrepVsFull.Full ? allowProceedWithFailure_Full : allowProceedWithFailure_PrepScreening;
            }

            [SerializeField] private bool allowProceedWithFailure_Full = false;
            [SerializeField] private bool allowProceedWithFailure_PrepScreening = false;
            public int numFramesDelayStartCurrentOnNoInstructionLevels = 2;
        }

        public class RuntimeConfig
        {
            public bool useProbes;
            public StimulusManager.RuntimeConfig stimulus;
            public LevelConfig levelConfig;
            public GameManager gameManager;
            public LevelMasterManager levelMasterManager;
            public ProbeSystem.RuntimeConfig probeSystem;
            public SoundSystem.RuntimeAudioSourceConfig audioTone;
            public EyeTrackerManager_Base eyeTracker;

            public RuntimeConfig(bool useProbes, StimulusManager.RuntimeConfig stimulus, LevelConfig levelConfig, GameManager gameManager, LevelMasterManager levelMasterManager, ProbeSystem.RuntimeConfig probeSystem, SoundSystem.RuntimeAudioSourceConfig audioTone, EyeTrackerManager_Base eyeTracker)
            {
                this.useProbes = useProbes;
                this.stimulus = stimulus;
                this.levelConfig = levelConfig;
                this.gameManager = gameManager;
                this.levelMasterManager = levelMasterManager;
                this.probeSystem = probeSystem;
                this.audioTone = audioTone;
                this.eyeTracker = eyeTracker;
            }
        }

        // NS_ MOVE TO SUBCLASS
        StimulusType stimulusTarget;
        public void PrepTaskRelevantGame(Config config, RuntimeConfig runtimeConfig)
        {
            // [SOS] First Prep (solved when subclassing)
            StimulusManager_TaskRelevant stimulusManager_TaskRelevant = new StimulusManager_TaskRelevant(config.stimulus, runtimeConfig.stimulus);
            Prep(config, runtimeConfig, stimulusManager_TaskRelevant);

            stimulusManager_TaskRelevant.onLastStimulusShown += StimulusManager_TaskRelevant_onLastStimulusShown;

            stimulusTarget = StimulusManager.GetLevelStimulusTarget(levelConfig).target;

            ExperimentManagerSession.LogData_AsTheyHappen_TS(
                TimeWrapper.GetCurrentTimestamp_TS(),
                "LevelMasterManager", "INITIALIZE;TASK_RELEVANT;{0}"._Format(stimulusTarget)); // [0] Game Type, [1] StimulusType

            if (reportTool != null) EngineWrapper.Destroy(reportTool);

            reportTool = new GameObject().AddComponent<StimulusReportTool>();
            StimulusReportTool.RuntimeConfig reportToolConfig = new StimulusReportTool.RuntimeConfig(
                stimulusManager_TaskRelevant, stimulusTarget,
                ExperimentLibraryManager.Config.Subject.reactionTimeMS,
                ExperimentLibraryManager.Config.Report.reactionWindowMS
                );

            reportTool.Initialize(ExperimentLibraryManager.Config.InputKeyCode, reportToolConfig);
            reportTool.onReportChance_TS += ReportTool_onReportChance_TS;
            reportTool.onWindowEnd_Thread += ReportTool_onWindowEnd_TS;
            replay_hasShownLastStimulus = false;
        }

        public void PrepSlides(Config config, RuntimeConfig runtimeConfig)
        {
            // [SOS] First Prep (solved when subclassing)
            Prep(config, runtimeConfig, null);

            ExperimentManagerSession.LogData_AsTheyHappen_TS(
                TimeWrapper.GetCurrentTimestamp_TS(),
                "LevelMasterManager", "INITIALIZE;SLIDES;[0] Game Type");
        }

        public void PrepTaskIrrelevantGame(Config config, RuntimeConfig runtimeConfig)
        {
            // [SOS] First Prep (solved when subclassing)
            StimulusManager_TaskIrrelevant stimulusManager_TaskIrrelevant = new StimulusManager_TaskIrrelevant(config.stimulus, runtimeConfig.stimulus);
            Prep(config, runtimeConfig, stimulusManager_TaskIrrelevant);


            ExperimentManagerSession.LogData_AsTheyHappen_TS(
                TimeWrapper.GetCurrentTimestamp_TS(),
                "LevelMasterManager", "INITIALIZE;TASK_IRRELEVANT;[0] Game Type");

            if (runtimeConfig.useProbes)
            {
                if (probeManager != null) EngineWrapper.Destroy(probeManager);
                probeManager = new GameObject().AddComponent<ProbeManager>();

                // Calculate the min/max
                // MinMax probeMinMax = ApplicationLibrary.Config.GetPeriodMinMax_Probes();

                probeManager.Initialize(stimulusManager_TaskIrrelevant);//, probeMinMax);

                probeManager.onProbeShown_NotTS += ProbeManager_onProbeShown;
                probeManager.onProbeResult_TS += ProbeManager_onProbeResult_TS;
            }
        }

        #region Handlers

        private void StimulusManager_onAnimEvent(object sender, BackgroundManager.AnimEventArgs e)
        {
            // [HACK] Should get that info from the background, maybe?
            bool isFiller = !StimulusManager.isShowingStimulus;// !e.isStimulusPeak; <- this doesnt work for REPLAYS

            if (e.animEvent == AnimEvent.PeakBegin)
            {
                LogBackgroundPeak(e);

                float scorePercentile = EngineWrapper.FindObjectOfType<ScoreSystem>().GetScorePercentile_TS();

                // Stimuli
                LevelConfig baseLevel = levelConfig;
                LevelConfig activeLevel = !baseLevel.isLocalizer ? baseLevel : GetReplayLevel(baseLevel);

                bool isGame = !baseLevel.isLocalizer;

                string trialType = "";
                if (isGame && isFiller)
                {
                    triggerMaster.SendGameFillerEvent();
                    trialType = "GAME_FILLER";
                }
                else if (isGame && !isFiller)
                {
                    trialType = "GAME_STIMULUS/PROBE";
                }
                else if (!isGame && isFiller)
                {
                    triggerMaster.SendLocalizerFillerEvent();
                    trialType = "LOCALIZER_FILLER";
                }
                else if (!isGame && !isFiller)
                {
                    trialType = "LOCALIZER_STIMULUS";
                }

                AsyncThread.RunOnMainThread_NextFrameCycle_TS(() =>
                {
                    ExperimentManagerSession.ReportFillerDetails(
                        TimeWrapper.lastRenderedFrame_TimeOfRenderMS,
                        TimeWrapper.lastRenderedFrame_TimeOfRender_SessionPlayTime_MS_NoPauses,
                        baseLevel.levelID_1Based, activeLevel.levelID_1Based, activeLevel.worldID, activeLevel.GetWorldType_TS(),
                        DifficultyManager.difficulty, DifficultyManager.GetAverageDifficulty_CurrentLevel(), DifficultyManager.dPrime_Final, DifficultyManager.GetAverageDPrimeFinal_CurrentLevel(),
                        scorePercentile, session_analytics.GetAverageStarsAll(), session_analytics.GetAverageStarsThisWorld(),
                        BackgroundManager.currentCycleIdx, trialType);
                });
            }
            else if (e.animEvent == AnimEvent.PeakEnd)
            {
                if (!e.isFirstCycle)
                    triggerMaster.SendAnimationPeakEndEvent(!levelConfig.isLocalizer, !isFiller);
                else
                    ExperimentManagerSession.LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(),
                        "LEVEL_MASTER_MANAGER", "TRIGGER_PREVENTED;ANIM_PEAK_END;FIRST_BACKGROUND_CYCLE");
            }
        }

        protected virtual void StimulusManager_onStimulusOnset(object sender, StimulusEventArgs e)
        {
            session_analytics.StimulusShown(e.stimulusType);

            LogStimulusOnset(e);

            float scorePercentile = GameObject.FindObjectOfType<ScoreSystem>().GetScorePercentile_TS();

            // Stimuli
            LevelConfig baseLevel = levelConfig;
            LevelConfig activeLevel = !baseLevel.isLocalizer ? baseLevel : ExperimentManagerSession.GetRecordedReplayLevel(baseLevel);

            AsyncThread.RunOnMainThread_NextFrameCycle_TS(() =>
            {
                ExperimentManagerSession.ReportStimulusDetails(
                    TimeWrapper.lastRenderedFrame_TimeOfRenderMS,
                    TimeWrapper.lastRenderedFrame_TimeOfRender_SessionPlayTime_MS_NoPauses,
                    baseLevel.levelID_1Based, activeLevel.levelID_1Based, activeLevel.worldID, activeLevel.GetWorldType_TS(),
                    DifficultyManager.difficulty, DifficultyManager.GetAverageDifficulty_CurrentLevel(), DifficultyManager.dPrime_Final, DifficultyManager.GetAverageDPrimeFinal_CurrentLevel(),
                    scorePercentile, session_analytics.GetAverageStarsAll(), session_analytics.GetAverageStarsThisWorld(),
                    BackgroundManager.currentCycleIdx, e.toBeProbed ? "GAME_PROBE" : baseLevel.isLocalizer ? "LOCALIZER_STIMULUS" : "GAME_STIMULUS",
                    StimulusManager.currentCycleIdx, e.stimulusType, e.stimulusName, e.backgroundObject.GetDirection());
            });

            if (levelConfig.isLocalizer)
            {
                if (e.shouldFireTriggers)
                    triggerMaster.SendStimulusEventReplay(e.stimulusType, e.stimulusName, stimulusTarget, e.backgroundObject.GetDirection());
                else
                    ExperimentManagerSession.LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(),
                        "LEVEL_MASTER_MANAGER", "TRIGGER_PREVENTED;REPLAY_STIMULUS_ON;FIRST_BACKGROUND_CYCLE");
            }
            else
            {
                bool isProbeTrial = probeManager != null && e.toBeProbed;
                Direction_2D_Diagonal direction = e.backgroundObject.GetDirection();

                if (e.shouldFireTriggers)
                    triggerMaster.SendStimulusEventGame(e.stimulusType, e.stimulusName, direction, isProbeTrial);
                else
                    ExperimentManagerSession.LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(),
                        "LEVEL_MASTER_MANAGER", "TRIGGER_PREVENTED;GAME_STIMULUS_ON;FIRST_BACKGROUND_CYCLE");
            }
        }

        private void StimulusManager_TaskRelevant_onLastStimulusShown(object sender, EventArgs e)
        {
            replay_hasShownLastStimulus = true;
        }

        private void ReportTool_onWindowEnd_TS(object sender, TimeWrapper.Timestamp e)
        {
            onReplayWindowOver_Thread?.Invoke(this, e);
        }

        private void ReportTool_onReportChance_TS(object sender, ReportChanceEventArgs e)
        {
            StimulusReportTool.TaskRelevantResponseEvaluation responseEvaluation = StimulusReportTool.GetResponseEvaluation(e.response);

            switch (responseEvaluation)
            {
                // Target stimulus came, player responded
                case StimulusReportTool.TaskRelevantResponseEvaluation.TruePositive:
                    triggerMaster.SendResponseEventReplay_TS(true);
                    break;

                // Non-target / blank came, player responded
                case StimulusReportTool.TaskRelevantResponseEvaluation.FalsePositive:
                    triggerMaster.SendResponseEventReplay_TS(true);
                    break;

                // Non-target / blank came, window ended (no response)
                case StimulusReportTool.TaskRelevantResponseEvaluation.TrueNegative:
                    triggerMaster.SendResponseEventReplay_TS(false);
                    break;

                // Target stimulus came, window ended (no response)
                case StimulusReportTool.TaskRelevantResponseEvaluation.FalseNegative:
                    triggerMaster.SendResponseEventReplay_TS(false);
                    break;
            }

            // At this stage we have all our high-accuracy data, we can return to the main thread to log
            AsyncThread.RunOnMainThread_ASAP_TS(() =>
            {
                ReportTool_onReportChance_NotTS(sender, e, responseEvaluation);
            });
        }

        protected virtual void ReportTool_onReportChance_NotTS(object sender, ReportChanceEventArgs e, StimulusReportTool.TaskRelevantResponseEvaluation responseEvaluation)
        {
            // Don't report anything while game is paused
            if (runtimeConfig.gameManager.isPaused) return; // || runtimeConfig.gameManager.isImmune

            HandleResponseEvaluation(responseEvaluation);

            // This event comes from HighAccuracy input and not unity input
            double responseTS = e.responseTS; // SubjectPerformanceReport.currentFrameCycleUTC;
            double responseDT = responseTS - e.stimulusHiddenTS;
            float scorePercentile = GameObject.FindObjectOfType<ScoreSystem>().GetScorePercentile_TS();

            int stimulusID = e.stimulusID;

            LevelConfig replayLevel = levelConfig;
            LevelConfig replayedLevel = GetReplayLevel(replayLevel);

            ExperimentManagerSession.ReportTaskRelevantResponse(
                StimulusManager.lastTimeStimulusShown_MS, StimulusManager.lastTimeStimulusShown_MS_RunElapsed_NoPauses,
                replayLevel.levelID_1Based, replayedLevel.levelID_1Based, replayedLevel.worldID, replayedLevel.GetWorldType_TS(),
                DifficultyManager.difficulty, DifficultyManager.GetAverageDifficulty_CurrentLevel(), DifficultyManager.dPrime_Final, DifficultyManager.GetAverageDPrimeFinal_CurrentLevel(),
                scorePercentile, session_analytics.GetAverageStarsAll(), session_analytics.GetAverageStarsThisWorld(),
                BackgroundManager.currentCycleIdx,
                StimulusManager.currentCycleIdx, e.stimulusType, e.stimulusName, e.stimulusLocation,
                e.windowEndTS, e.windowEndTS_NoPauses,
                responseTS, e.responseTS_NoPauses, responseDT, e.response, responseEvaluation, e.repliedUsingHighAccu);

            if (replay_hasShownLastStimulus && responseEvaluation.IsFinal())
                onLevelReadyToEnd?.Invoke(this, responseEvaluation);
            //bool correctGuess = e == StimulusReportTool.ReportUserAction.PlayerFoundStimulus;
            //gameManager.difficultyManager.LogGuessIncrement(correctGuess);
            // feedbackManager.IncrementGuesses(correctGuess);
        }
        
        public void HandleResponseEvaluation(StimulusReportTool.TaskRelevantResponseEvaluation responseEvaluation)
        {
            if (responseEvaluation == StimulusReportTool.TaskRelevantResponseEvaluation.TruePositive)
            {
                if (ExperimentLibraryManager.Config.Audio.localizerSounds_Response)
                    runtimeConfig.gameManager.PlaySFX(ExperimentLibraryManager.Config.Audio.ResponseReward);
                //scoreSystem.IncreaseScore(null);
            }
            else if (responseEvaluation == StimulusReportTool.TaskRelevantResponseEvaluation.FalsePositive)
            {
                if (ExperimentLibraryManager.Config.Audio.localizerSounds_Response)
                    runtimeConfig.gameManager.PlaySFX(ExperimentLibraryManager.Config.Audio.ResponsePenalty);
            }

            // Reduce difficulty when user misses stimuli
            /*
            if (responseEvaluation == StimulusReportTool.TaskRelevantResponseEvaluation.FalseNegative)
                difficultyManager.LogGuessIncrement(false, timeSinceStart);
            */
        }

        public void Pause(bool keepMusicPlaying = false)
        {
            SetPause(true, keepMusicPlaying);
        }

        public void UnPause()
        {
            SetPause(false);
        }

        private void SetPause(bool pause, bool keepMusicPlaying = false)
        {
            // If we try to unpause while a probe is active
            if (!pause && ProbeSystem.isProbeShown == true)
            {
                Debug_Helper.LogError(typeof(ExperimentManagerApplication), "Tried unpausing while probe being shown! BAD!");
                return;
            }

            if (stimulusManager != null)
                stimulusManager.Pause(pause);

            if (reportTool != null)
                reportTool.SetTracking(!pause);

            if (pause)
                PlayerProgression.levelManager.Pause(false, keepMusicPlaying);
            else
                PlayerProgression.levelManager.UnPause();

            session_analytics.Pause(pause);
        }


        protected virtual void ProbeManager_onProbeShown(object sender, EventArgs<bool> e)
        {
            if (runtimeConfig.levelMasterManager.isExited) return;

            if (e)
            {
                // Do that instantly, the frame delay occurs naturally within trigger master
                triggerMaster.SendProbeEvent();

                AsyncThread.RunOnMainThread_NextFrameCycle_TS(() =>
                {
                    LogProbeOnset();
                    session_analytics.IncreaseProbeShown();
                });
            }

            // Same frame !
            ui_onProbeShown?.Invoke(this, e);
        }

        public event EventHandler<bool> ui_onProbeShown;

        private void ProbeManager_onProbeResult_TS(object sender, ProbeManager.ResultArgs e)
        {
            // Disable feedback for Irrelevant tasks
            //feedbackManager.IncrementGuesses(e.isCorrect);

            // When done with the probe
            if (e == null) return;

            LogProbeResult_TS(e);
            triggerMaster.SendResponseEventGame_TS(e.answer);

            int wantedProbeID = GetWantedProbeID();
            int probeIdx = e.probeId;

            // Debug.LogError("Wanted Probe #{0}, Got probe {1}"._Format(wantedProbeID, probeIdx));
            if (probeIdx < wantedProbeID) return;

            // We can afford for this to run on the main thread (gameplay flow related things)
            AsyncThread.RunOnMainThread_ASAP_TS(() =>
            {
                if (probeIdx > wantedProbeID)
                {
                    ExperimentManagerSession.LogData_AsTheyHappen_TS(
                        TimeWrapper.GetCurrentTimestamp_TS(),
                        "LevelMasterManager", "FAIL_SAFE {0} > {1}"._Format(probeIdx, wantedProbeID));
                    this.LogWarning("Shouldn't happen, we got probe {0} when max was {1}"._Format(probeIdx, wantedProbeID));
                }

                triggerMaster.LockNonLevelEndTriggers(true);

                Utility_Helper.StartTimer(ExperimentLibraryManager.Config.Probes.probeShieldBeforeEndOfLevelMS / 1000f, a =>
                {
                    // Level end
                    triggerMaster.LockNonLevelEndTriggers(false);
                    PlayerProgression.CompleteCurrentLevel(); //  OnLevelComplete(); // send the 1-based ID here
                    this.Log(TimeWrapper.currentTimestampMS_NoPauses + " :: " + TimeWrapper.currentFrameCycleBeginMS);
                });
            });
        }

        /// <summary>
        /// </summary>
        /// <returns>0-based indexing</returns>
        public int GetWantedProbeID()
        {
            // For Tutorial PRACTICE
            if (runtimeConfig.levelConfig.isTutorial_P)
                return ProbeManager.cycleIdxAtLevelStart + ExperimentLibraryManager.Config.Probes.GetNumProbesInPractice(module.ToPrepVsFull()) - 1;

            // Foreverything else
            return Precalculator.gameTimings.cumulativeLevelDurations_Actual[runtimeConfig.levelConfig.levelID_1Based - 1].triggeredByEventID;
        }

        #endregion

        #region Eye Tracking
        // protected EyeTrackerManager_Base eyeTrackerManager;
        public event EventHandler<EventArgs<StimulusReportTool.TaskRelevantResponseEvaluation>> onLevelReadyToEnd;
        public event EventHandler<TimeWrapper.Timestamp> onReplayWindowOver_Thread;

        private void LogBackgroundPeak(BackgroundManager.AnimEventArgs e)
        {
            runtimeConfig.eyeTracker?.RequestWriteToTracker_NotTS_Event(
                "BACKGROUND",
                e.backgroundCycleIdx.ToString()
            );
        }

        private void LogStimulusOnset(StimulusEventArgs e)
        {
            runtimeConfig.eyeTracker?.RequestWriteToTracker_NotTS_Event(
                "STIMULUS",
                e.worldLevelTrial,
                e.stimulusType.ToString(),
                e.stimulusName,
                e.backgroundObject.GetDirection().ToString(),
                e.backgroundObject.GetPixelCoords().ToString(),
                e.backgroundObject.GetBackgroundPixelSize().ToString(),
                e.backgroundObject.GetOverlayPixelSize().ToString(),
                e.toBeProbed ? "to-be-probed" : "no-probe"
            );
        }

        private void LogProbeOnset()
        {
            runtimeConfig.eyeTracker?.RequestWriteToTracker_NotTS_Event("PROBE");
        }

        private void LogProbeResult_TS(ProbeManager.ResultArgs e)
        {
            EyeTrackerTimestampData timestamps = runtimeConfig.eyeTracker?.GetCurrentTimestamps();

            AsyncThread.RunOnMainThread_ASAP_TS(() =>
            {
                string answer = (e.answer == TaskIrrelevantResponse.Yes) ? "seen" :
                                (e.answer == TaskIrrelevantResponse.No) ? "unseen" : "maybe";
                runtimeConfig.eyeTracker?.RequestWriteToTracker_NotTS_Event(
                    timestamps,
                    "RESPONSE",
                    answer,
                    e.isCorrect ? "correct" : "incorrect"
                );
            });
        }

        public void Dispose()
        {
            // Unsub from events
            triggerMaster.DeInitialize();
        }
        #endregion


        /*
        private void StimulusManager_onStimulusHidden(object sender, StimulusManager.StimulusEventArgs e)
        {
            // See StimulusQueueFinish
            return;
            StimulusType trialType = GetActiveLevelStimulusTarget();

            int threshold = Precalculator.wantedNumPeaks_Localizer * (PlayerProgression.ActiveLevel.localizerID >= 2 ? 2 : 1) / 2;

            bool isComplete = false;
            if (trialType == StimulusType.Face)
            {
                // [TODO] Factor in previous localizer! 
                // First should end when TotalNumberOfStimuliPresentedInFaceTarget is Precalculator.wantedNumPeaks_Localizer (80)
                // second when it is 2 * Precalculator.wantedNumPeaks_Localizer (160)
                // Otherwise, reset TotalNumberOfStimuliPresentedInFaceTarget
                if (SPR_EXPE.probeSummary.replaySum.
                    TotalNumberOfStimuliPresentedInFaceTarget >= threshold)
                {
                    isComplete = true;
                }
            }
            if (trialType == StimulusType.Object)
            {
                // [TODO] (see above)
                if (SPR_EXPE.probeSummary.replaySum.
                    TotalNumberOfStimuliPresentedInObjectTarget >= threshold)
                {
                    isComplete = true;
                }
            }

            if (isComplete)
            {
                // Add some delay befre ending the level to prevent losing last event logs
                SafeEndOfGame(PlayerProgression.ActiveLevel.levelID_1Based);
            }
        }
        */
    }
}