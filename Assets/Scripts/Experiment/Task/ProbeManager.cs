// NS_REMOVE | Configs, Logs, Analytics
using ExperimentLibrary;

// NS_DEBATABLE
using Helpers.Async;

// NS_DEBATABLE
using Game.Core;
// NS_DEBATABLE 
using Game.Managers.SessionManagers;
// NS_DEBATABLE 
using Game.Systems.Adaptive;
// NS_DEBATABLE
using Game.Systems.Score;

// NS_DEBATABLE | Probe System
using Experiment.Task.UI;

using Experiment.Stimulus;
using Experiment.Background;
using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;
using Helpers.Engine;
using Experiment.Task.Core;
using Experiment.Managers;

namespace Experiment.Task
{
    public class ProbeManager : MonoBehaviour
    {
        public event EventHandler<EventArgs<bool>> onProbeShown_NotTS;
        public event EventHandler<ResultArgs> onProbeResult_TS;

        public bool isTracking { get; private set; }

        private StimulusManager_TaskIrrelevant stimulusManager;

        private TrialType trialType = TrialType.None;
        private StimulusType stimulusType = StimulusType.None;
        private string stimulusName = "";
        private BackgroundObject lastSimulusBackgroundObject;

        protected ProbeSystem.Config config { get { return ExperimentLibraryManager.Config.Probes; } }

        // Needed only for logging details
        private ScoreSystem _scoreSystem;

        /*
        private readonly Dictionary<TrialType, int> numProbesPerType = new Dictionary<TrialType, int>();
        private float currentRatio_Stimulus_vs_Absent
        {
            get
            {
                int stimulusProbes = numProbesPerType.TryGet(TrialType.Stimulus);
                int absentProbes = numProbesPerType.TryGet(TrialType.Absent);
                if (absentProbes == 0) return Mathf.Infinity;
                return stimulusProbes / absentProbes;
            }
        }
        */

        #region Public Methods

        private static int currentCycleIdx = 0;
        public static int cycleIdxAtLevelStart { get; private set; }
        public static int numProbesPerWorld { get { return ExperimentManagerSession.GetWantedNumProbesPerWorld(); } }

        public static void SetCycleIdx(int cycleIdx)
        {
            currentCycleIdx = cycleIdx;
            cycleIdxAtLevelStart = cycleIdx;
        }

        private double config_probeTimeout_MS;

        /// <summary>
        /// Initialize component given StimulusManager.
        /// </summary>
        public void Initialize(StimulusManager_TaskIrrelevant stimulusManager)//, MinMax timerLock_MinMax)
        {
            config_probeTimeout_MS = config.probeTimeoutMS;
            this.gameObject.name = "ProbeManager";

            this.stimulusManager = stimulusManager;
            stimulusManager.onStimulusClear += StimulusManager_onStimulusClear;

            isTracking = true;
            trialType = TrialType.None;

            _scoreSystem = FindObjectOfType<ScoreSystem>();
            /*
            numProbesPerType.Clear();
            foreach (TrialType trialType in Utility_Helper.EnumGetValues<TrialType>())
                numProbesPerType.Add(trialType, 0);
            */
        }

        public static Dictionary<float, float> GetTruncatedExpDistribution(bool isFMRI_Scanner)
        {
            ProbeSystem.Config config = ExperimentLibraryManager.Config.Probes;
            return Math_Helper.TruncExp_GetCumProb(isFMRI_Scanner ? config.expConfig_FMRIScanner : config.expConfig);
        }

        /// <summary>
        /// Set wheteher this component is actively tracking game and player.
        /// </summary>
        public void SetTracking(bool isTracking)
        {
            this.isTracking = isTracking;

            // Clear public values
        }

        #endregion

        private void OnDestroy()
        {
            if (stimulusManager != null)
            {
                stimulusManager.onStimulusClear -= StimulusManager_onStimulusClear;
            }
        }

        #region Event Handlers

        /// Called after the stimulus has reached its minimum
        private void StimulusManager_onStimulusClear(object sender, StimulusEventArgs e)
        {
            stimulusType = e.stimulusType;
            trialType = stimulusType == StimulusType.None ? TrialType.Absent : TrialType.Stimulus;
            stimulusName = e.stimulusName;
            lastSimulusBackgroundObject = e.backgroundObject;

            int currentStimulusIdx = e.stimulusIdxCycle;
            // Debug.LogError("HIDING STIMULUS " + stimulusName + " FROM " + lastSimulusBackgroundObject.GetDirection());

            // Wait BackgroundManager to place objects
            // Debug.LogError(TimeWrapper.currentTimestampMS);
            float probeDelay = ExperimentLibraryManager.Config.Probes.probeDelayAfterStimulus;
            probeDelay = Mathf.Max(probeDelay - PerformanceObserver.averageFrameDT_Seconds, 0);
            Utility_Helper.StartTimer(probeDelay, (a) =>
            {
                HandleTrialEnd(trialType, currentStimulusIdx, e.toBeProbed);
            });
        }

        // Validade user input and stimulus chance
        private void HandleTrialEnd(TrialType trialType, int currentStimulusIdx, bool e_toBeProbed)
        {
            EventInformation currentCycleInfo = Precalculator.gameTimings.probeOccurences_Actual[currentCycleIdx];
            int wantedStimulusIdx = currentCycleInfo.triggeredByEventID;

            bool shouldBeProbed = wantedStimulusIdx == currentStimulusIdx;

            if (shouldBeProbed != e_toBeProbed)
                this.LogError("SHOULD BE PROBED :: " + shouldBeProbed + "\nE_TO BE PROBED :: " + e_toBeProbed);

            if (!shouldBeProbed) return;

            if (!isTracking) return;

            this.Log("Showing Probe :: {0}"._Format(trialType));

            // numProbesPerType[trialType]++;

            // Get a random Probe and display it
            // Probe probe = new Probe();
            // ApplicationLibrary.Config.Probes.Questions.GetRandom();
            StimulusType actualType = stimulusType;

            if (lastSimulusBackgroundObject == null)
            {
                this.LogException(new Exception("Background Obj Destroyed"));
                return;
            }

            int indexToPointAt = GetIndexOf(lastSimulusBackgroundObject.gameObject);
            //Debug.Log("Object was at " + ((Direction_2D_Diagonal)indexToPointAt).ToString());

            // [SOS] Keep Thread Safe!!
            bool resultCalled = false;

            // NEXT FRAME RENDER
            AsyncThread.RunOnMainThread_NextFrameCycle_TS(() =>
            {
                double probeTimestampMS = TimeWrapper.lastRenderedFrame_TimeOfRenderMS;
                double probeTimestampMS_NoPauses = TimeWrapper.lastRenderedFrame_TimeOfRender_SessionPlayTime_MS_NoPauses;
                // Debug.LogError(probeTimestampMS);

                Action<ProbeSystem.ResponseArgs> result_TS = (e) =>
                {
                    if (resultCalled)
                    {
                        ExperimentManagerSession.LogData_AsTheyHappen_TS(
                            TimeWrapper.GetCurrentTimestamp_TS(),
                            "ProbeManager", "DOUBLE_ANSWER_ABORTED");

#if UNITY_EDITOR
                        Debug.LogError("Double answer - wtf");
#endif

                        return;
                    }

                    resultCalled = true;

                    // Raise a cooldown!
                    bool wasSomething = actualType == StimulusType.Face || actualType == StimulusType.Object;

                    bool isCorrectAnswer = false;
                    // string answerEvaluation = "";
                    if (wasSomething && e.response == TaskIrrelevantResponse.Yes)
                    {
                        // answerEvaluation = "correctly";
                        isCorrectAnswer = true;
                    }
                    else if (!wasSomething && e.response == TaskIrrelevantResponse.Yes)
                    {
                        // answerEvaluation = "incorrectly";
                        isCorrectAnswer = false;
                    }
                    else if (wasSomething && e.response == TaskIrrelevantResponse.No)
                    {
                        // answerEvaluation = "incorrectly";
                        isCorrectAnswer = false;
                    }
                    else if (!wasSomething && e.response == TaskIrrelevantResponse.No)
                    {
                        // answerEvaluation = "correctly";
                        isCorrectAnswer = true;
                    }
                    else if (e.response == TaskIrrelevantResponse.Maybe)
                    {
                        // answerEvaluation = "inconclusive";
                        isCorrectAnswer = false;
                    }
                    else if (e.response == TaskIrrelevantResponse.NoResponse)
                    {
                        // answerEvaluation = "no-response";
                        isCorrectAnswer = false;
                    }

                    /*
                    string log = string.Format("Showed {0} stimulus at index {1} - User responded {2} that they {3}".
                        _Format(actualType, e.crossSelectedIndex, answerEvaluation, e.response));

                    AsyncThread.RunOnMainThread(() => { Debug.Log(log); });
                    SubjectPerformanceReport.LogProbeAnswer(actualType, isCorrectAnswer, answer);
                    */

                    // Because we want thread-safe, better to have them spread out and ensure each of them is TS
                    double stimulusOnsetTS = StimulusManager.lastTimeStimulusShown_MS;
                    double stimulusOnsetTS_NoPauses = StimulusManager.lastTimeStimulusShown_MS_RunElapsed_NoPauses;
                    int currentLevelID = PlayerProgression.ActiveLevel.levelID_1Based;
                    int activeLevelID = PlayerProgression.ActiveLevel.levelID_1Based;
                    int activeWorldID = PlayerProgression.ActiveLevel.worldID;
                    WorldType activeWorldType = PlayerProgression.ActiveLevel.GetWorldType_TS();
                    float difficulty = DifficultyManager.difficulty;
                    float performance = DifficultyManager.dPrime_Final;
                    float scorePercentile = (_scoreSystem != null) ? _scoreSystem.GetScorePercentile_TS() : 0;
                    int animCycleID = BackgroundManager.currentCycleIdx;
                    int stimulusID = StimulusManager.currentCycleIdx;
                    StimulusType stimulusType = actualType;
                    Direction_2D_Diagonal stimulusLocation = (Direction_2D_Diagonal)e.crossSelectedIndex;
                    int probeID = currentCycleIdx;
                    double windowEndTS = config_probeTimeout_MS > 0 ? (probeTimestampMS + config_probeTimeout_MS) : double.MaxValue;
                    // We can only know the exact no-pause window end if we waited til that point (aka we got a no-response response)
                    double windowEndTS_NoPauses = e.response == TaskIrrelevantResponse.NoResponse ? e.responseTS_NoPauses : -1;
                    double responseDT = e.responseTS - probeTimestampMS;

                    // Log actions/answers here or log them directly in ProbeSystem (DoFileLogs)
                    //StimulusType answerType = GetStimulusTypeFromAnswer(result);

                    // [SOS] Do that before the stimulus disappears
                    // This cycle id is before we increase it in code below, but still present on game/screen
                    // Debug.Log(TimeWrapper.currentTimestampMS + " RaiseOnAnswer_TS");
                    RaiseOnAnswer_TS(e.cycleID, e.response, isCorrectAnswer);

                    // NEXT FRAME RENDER
                    // We dont care when the logging happens - just that we have the correct details for it
                    AsyncThread.RunOnMainThread_NextFrameCycle_TS(() =>
                    {
                        RaiseOnProbeShown_NotTS(false);
                        // So anything that isn't that important to be measured on the high-accuracy thread can be moved here

                        float averageDifficulty = DifficultyManager.GetAverageDifficulty_CurrentLevel();
                        float averagePerformance = DifficultyManager.GetAverageDPrimeFinal_CurrentLevel();
                        float averageStarsPerLevel = ExperimentManagerSession.session_analytics.GetAverageStarsAll();
                        float averageStarsInWorld = ExperimentManagerSession.session_analytics.GetAverageStarsThisWorld();

                        ExperimentManagerSession.ReportTaskIrrelevantResponse_NotTS(stimulusOnsetTS, stimulusOnsetTS_NoPauses,
                                            currentLevelID, activeLevelID, activeWorldID, activeWorldType,
                                            difficulty, averageDifficulty, performance, averagePerformance,
                                            scorePercentile, averageStarsPerLevel, averageStarsInWorld,
                                            animCycleID,
                                            stimulusID, stimulusType, stimulusName, stimulusLocation,
                                            probeID, probeTimestampMS, probeTimestampMS_NoPauses, windowEndTS, windowEndTS_NoPauses,
                                            e.responseTS, e.responseTS_NoPauses, responseDT, e.response, !e.isMainThread);

                        //FileLogger.Log("Probe System", string.Format("User answered:{0}", index));
                        // Debug.Log("cycle:" + cycleID);
                    });
                };

                ProbeSystem.AssignResultCallback(result_TS);
            });

            ProbeSystem.ShowProbe(currentCycleIdx, indexToPointAt, lastSimulusBackgroundObject.transform);
            RaiseOnProbeShown_NotTS(true);

            /*
            this.LogWarning("PROBE #{0} ({1}){2}"._Format(currentCycleIdx,
                (((float)(TimeWrapper.totalPlayTime_NoPauses - currentCycleInfo.timestamp * 1000)).Round(0.5f).ToString("#"),
                currentCycleInfo.triggersEventID >= 0 ? " -> {0}"._Format(currentCycleInfo.triggersEventID) : "")));
            */

            this.LogWarning("Showing Probe #" + currentCycleIdx);
            currentCycleIdx++;
        }

        // [BUG]  GetDirection return false direction for bottom sides. it reports them as top
        //private static int GetIndexOf(BackgroundObject backgroundObject)
        //{
        //    return (int)backgroundObject.GetDirection();
        //}

        private static int GetIndexOf(GameObject lastSimulus)
        {
            Vector3 objScreenPosition = Camera.main.WorldToViewportPoint(lastSimulus.transform.position);
            Vector3 crossScreenPosition = Camera.main.WorldToViewportPoint(ProbeSystem.instance.crossParent.transform.position);
            Vector3 objectOffset = objScreenPosition - crossScreenPosition;

            // Top Left
            if (objectOffset.x < 0 && objectOffset.y > 0) return 0;
            // Top right
            if (objectOffset.x > 0 && objectOffset.y > 0) return 1;
            // Bottom right
            if (objectOffset.x > 0 && objectOffset.y < 0) return 2;
            // Bottom left
            if (objectOffset.x < 0 && objectOffset.y < 0) return 3;

            return -1;
        }

        private StimulusType GetStimulusTypeFromAnswer(string result)
        {
            // Current Probe :: (f / o / -)
            StimulusType answerType = StimulusType.None;
            if (result.ContainsInvariant("face"))
                answerType = StimulusType.Face;
            else if (result.ContainsInvariant("object"))
                answerType = StimulusType.Object;

            return answerType;
        }

        private void RaiseOnAnswer_TS(int id, TaskIrrelevantResponse answer, bool isCorrect)
        {
            onProbeResult_TS?.Invoke(this, new ResultArgs(id, answer, isCorrect));
        }

        /*
        private void RaiseOnAnswer(StimulusType actualType, StimulusType answerType)
        {
            if (onProbeResult != null)
                onProbeResult(this, new ResultArgs(actualType, answerType));
        }
        */

        private void RaiseOnProbeShown_NotTS(bool isShown)
        {
            onProbeShown_NotTS?.Invoke(this, new EventArgs<bool>(isShown));
        }

        private enum TrialType { None, Stimulus, Absent }

        public class ResultArgs : EventArgs
        {
            public int probeId;
            public TaskIrrelevantResponse answer;
            public bool isCorrect;

            public ResultArgs(int probeId, TaskIrrelevantResponse answer, bool isCorrect)
            {
                this.probeId = probeId;
                this.answer = answer;
                this.isCorrect = isCorrect;
            }
        }

        /*
        public class ResultArgs : EventArgs
        {
            public StimulusType actualType;
            public StimulusType answerType;

            public bool isCorrect { get { return actualType == answerType; } }

            public ResultArgs(StimulusType actualType, StimulusType answerType)
            {
                this.actualType = actualType;
                this.answerType = answerType;
            }
        }
        */

        #endregion
    }
}