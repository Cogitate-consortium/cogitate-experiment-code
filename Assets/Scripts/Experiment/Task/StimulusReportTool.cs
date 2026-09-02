// NS_REMOVE | ENTANGLED Classes
using Experiment.Library.Core;

// NS_DEBATABLE | Maybe centralize?
using Peripherals.UserInput;

using System;
using UnityEngine;
using TGP.Helpers;
using Experiment.Stimulus;
using Helpers.Engine;
using Helpers.Async; //  Just for running on main thread / next frame
using Experiment.Managers;
using System.Collections.Generic;

namespace Experiment.Task
{
    public static class StimulusReportToolHelper
    {
        public static bool IsFinal(this StimulusReportTool.TaskRelevantResponseEvaluation evaluation)
        {
            return
                evaluation == StimulusReportTool.TaskRelevantResponseEvaluation.FalseNegative ||
                evaluation == StimulusReportTool.TaskRelevantResponseEvaluation.TrueNegative ||
                evaluation == StimulusReportTool.TaskRelevantResponseEvaluation.FalsePositive ||
                evaluation == StimulusReportTool.TaskRelevantResponseEvaluation.TruePositive;
        }
    }

    public class StimulusReportTool : MonoBehaviour
    {
        [Serializable]
        public enum TaskRelevantResponse
        {
            Unknown = -1,
            CorrectStimNoResponse,
            CorrectStimYes,
            CorrectStimAdditionalPress,
            WrongStimNoResponse,
            WrongStimYes,
            WrongStimAdditionalPress,
            BlankNoResponse,
            BlankYes,
            BlankAdditionalPress,
            BeforeWindowPress,
            AfterWindowPress,
        }

        [System.Serializable]
        public enum TaskRelevantResponseEvaluation { Unknown = -1, FalseNegative = 0, TruePositive = 1, FalsePositive = 2, TrueNegative = 3, AdditionalPress = 4, OutsideWindowPress = 5 }

        public static TaskRelevantResponseEvaluation GetResponseEvaluation(TaskRelevantResponse response)
        {
            switch (response)
            {
                // False Negative :: There was a stimulus, but we said there wasn't (or we didn't say anything and the window passed)
                case TaskRelevantResponse.CorrectStimNoResponse:
                    return TaskRelevantResponseEvaluation.FalseNegative;

                // True Positive :: There was a stimulus, and we reported it
                case TaskRelevantResponse.CorrectStimYes:
                    return TaskRelevantResponseEvaluation.TruePositive;

                // True Negative :: There wasn't a stimulus (at least not the one we were looking for) and we didn't report anything
                case TaskRelevantResponse.WrongStimNoResponse:
                case TaskRelevantResponse.BlankNoResponse:
                    return TaskRelevantResponseEvaluation.TrueNegative;

                // False Positive :: There wasn't a stimulus, but we said there was one
                case TaskRelevantResponse.WrongStimYes:
                case TaskRelevantResponse.BlankYes:
                    return TaskRelevantResponseEvaluation.FalsePositive;

                // Additional Press :: All extra presses within the same window of a press 
                case TaskRelevantResponse.CorrectStimAdditionalPress:
                case TaskRelevantResponse.WrongStimAdditionalPress:
                case TaskRelevantResponse.BlankAdditionalPress:
                    return TaskRelevantResponseEvaluation.AdditionalPress;

                // Outside Window Press :: All presses outside a Stim shown - Window over period
                case TaskRelevantResponse.BeforeWindowPress:
                case TaskRelevantResponse.AfterWindowPress:
                    return TaskRelevantResponseEvaluation.OutsideWindowPress;
            }

            return TaskRelevantResponseEvaluation.Unknown;
        }

        /// <summary>
        /// 0 : Stimuli shown, but not player action.
        /// 1 : Stimuli shown and player report it.
        /// 2 : Player report it, but no visible stimuli.
        /// </summary>
        public event EventHandler<ReportChanceEventArgs> onReportChance_TS;

        public event EventHandler<TimeWrapper.Timestamp> onWindowEnd_Thread;

        public bool isTracking { get; private set; }

        /// <summary>
        /// Resets outside the window
        /// </summary>
        private StimulusType? currentWindowStimulusType { get { lock (currentWindowStimulus_Lock) return currentWindowStimulus?.type; } }
        private object currentWindowStimulus_Lock = new object();
        /// <summary>
        /// Resets outside the window
        /// </summary>
        private SpriteLocation? currentWindowStimulus;

        private StimulusType lookForSpecificStimulusType;
        private bool isLookingForSpecificStimuliType { get { return lookForSpecificStimulusType != StimulusType.None; } }
        private object gotResponseWithinWindow_Lock = new object();
        private bool gotResponseWithinWindow = false;
        private StimulusManager stimulusManager;

        private bool isGenericReporting = true;
        private List<KeyCode> config_Report_Face_keyCodes = new List<KeyCode>();
        private List<KeyCode> config_Report_Object_keyCodes = new List<KeyCode>();
        private int config_reactionTimeMS;
        private int config_reactionWindowMS;

        #region Public Methods
        private SeattleInputKeysConfig_KeyCode config;
        public RuntimeConfig runtimeConfig;

        public class RuntimeConfig
        {
            public StimulusManager stimulusManager;
            public StimulusType lookForSpecificStimulus;
            public int reactionTimeMS;
            public int reactionWindowMS;

            public RuntimeConfig(StimulusManager stimulusManager, StimulusType lookForSpecificStimulus, int reactionTimeMS, int reactionWindowMS)
            {
                this.stimulusManager = stimulusManager;
                this.lookForSpecificStimulus = lookForSpecificStimulus;
                this.reactionTimeMS = reactionTimeMS;
                this.reactionWindowMS = reactionWindowMS;
            }
        }

        /// <summary>
        /// Initialize component given StimulusManager.
        /// </summary>
        public void Initialize(SeattleInputKeysConfig_KeyCode config, RuntimeConfig runtimeConfig)
        {
            config_Report_Face_keyCodes = config.Report_Face.keyCodes;
            config_Report_Object_keyCodes = config.Report_Object.keyCodes;
            config_reactionTimeMS = runtimeConfig.reactionTimeMS;
            config_reactionWindowMS = runtimeConfig.reactionWindowMS;

            isGenericReporting = config.Report_Face == config.Report_Object;

            gameObject.name = "ReportTool";
            stimulusManager = runtimeConfig.stimulusManager;
            lookForSpecificStimulusType = runtimeConfig.lookForSpecificStimulus;
            stimulusManager.onStimulusOnset += StimulusManager_onStimulusOnset;
            InputManager.onKeyDown_TS += InputManager_onKeyDown_TS;
            isTracking = true;
        }

        /// <summary>
        /// Set wheteher this component is actively tracking game and player.
        /// </summary>
        public void SetTracking(bool isTracking)
        {
            this.isTracking = isTracking;

            // Clear public values
            Reset();
        }

        #endregion

        private void OnDestroy()
        {
            if (stimulusManager != null)
            {
                stimulusManager.onStimulusOnset -= StimulusManager_onStimulusOnset;
            }
            InputManager.onKeyDown_TS -= InputManager_onKeyDown_TS;
        }

        private void Reset()
        {
            lock (timestampWindowStartMS_Lock)
                timestampWindowStartMS = Mathf.Infinity;
            lock (timestampWindowEndMS_Lock)
                timestampWindowEndMS = Mathf.NegativeInfinity;
            lock (currentWindowStimulus_Lock)
                currentWindowStimulus = null;
            lock (gotResponseWithinWindow_Lock)
                gotResponseWithinWindow = false;
        }

        #region Event Handlers

        // Check player input
        private void InputManager_onKeyDown_TS(object sender, InputManager.HighAccuracyEventArgs e)
        {
            if (!isTracking) return;

            KeyCode key = e.key;
            double timestampMS = e.timestamp.timestampMS;
            double timestampMS_NoPauses = e.timestamp.timestampMS_NoPauses;

            // Is it a Generic report?
            if (isGenericReporting)
            {
                // Debug.Log(e.value);
                if (config_Report_Face_keyCodes.Contains(key))
                    HandleReportRequest_Generic_TS(timestampMS, timestampMS_NoPauses);
            }
            // Is it a specific Report (and we guessed it correctly?
            else
            {
                if (config_Report_Face_keyCodes.Contains(key))
                    HandleReportRequest_Specific_TS(StimulusType.Face, timestampMS, timestampMS_NoPauses);
                else if (config_Report_Object_keyCodes.Contains(key))
                    HandleReportRequest_Specific_TS(StimulusType.Object, timestampMS, timestampMS_NoPauses);
            }
        }

        [SerializeField] private bool debug = false;

        /// <summary>
        /// New code, where you press the same button for all stimuli, but there is a "lookForSpecificStimulus" target (face / object)
        /// </summary>
        private void HandleReportRequest_Generic_TS(double timestampMS, double timestampMS_NoPauses)
        {
            TaskRelevantResponse response = TaskRelevantResponse.Unknown;

            // We're AFTER the window has ended
            if (timestampMS > timestampWindowEndMS)
            {
                response = TaskRelevantResponse.AfterWindowPress;

                if (debug && Debug.isDebugBuild)
                {
                    // In this clause, the stimulus may still be NOT null (since we may not have cleaned up yet)
                    if (currentWindowStimulusType != null)
                        // But we dont care!
                        Debug.LogWarning("AFTER Window end, BEFORE cleanup, not null stimulus - expected edge case");
                    else
                        Debug.Log("Outside Window, null stimulus - normal");
                }
            }
            // We are BEFORE the window has begun
            // But it WILL happen when we fire prematurily!
            else if (timestampMS < timestampWindowStartMS)
            {
                response = TaskRelevantResponse.BeforeWindowPress;

                if (debug && Debug.isDebugBuild)
                {
                    // During initialization the above clause will fire
                    if (currentWindowStimulusType == null)
                        // So this shouldn't happen!
                        Debug.LogError("Before Window without stimulus - weird!");
                    // When we fire prematurily, the stimulus will be up 
                    else
                        // But we don't care
                        Debug.LogWarning("AFTER onset, BEFORE Window start, not null stimulus - expected edge case");
                }
            }
            // We're inside the window
            else
            {
                lock (gotResponseWithinWindow_Lock)
                {
                    // If there isn't a stim, this is problematic!
                    if (currentWindowStimulusType == null)
                    {
                        if (debug && Debug.isDebugBuild)
                            Debug.LogError("Within Window, null STIM, Shouldn't happen!");
                    }
                    // There ISN'T a stim!
                    else if (currentWindowStimulusType == StimulusType.None)
                        response = gotResponseWithinWindow ?
                            TaskRelevantResponse.BlankAdditionalPress :
                            TaskRelevantResponse.BlankYes;

                    // .. or it wasn't the one they were supposed to be looking for
                    else if (isLookingForSpecificStimuliType && lookForSpecificStimulusType != currentWindowStimulusType)
                        response = gotResponseWithinWindow ?
                            TaskRelevantResponse.WrongStimAdditionalPress :
                            TaskRelevantResponse.WrongStimYes;

                    // ..and there IS one!
                    else
                        response = gotResponseWithinWindow ?
                            TaskRelevantResponse.CorrectStimAdditionalPress :
                            TaskRelevantResponse.CorrectStimYes;

                    if (debug && Debug.isDebugBuild)
                        Debug.Log("Got Response within window -> " + response);

                    gotResponseWithinWindow = true;
                }
            }

            RaiseOnReportChance_TS(response, timestampMS, timestampMS_NoPauses);
        }

        /// <summary>
        /// [DEPRECATED] Old Code when there was a different button for each stimulus and you were reporting both faces and objects
        /// </summary>
        private void HandleReportRequest_Specific_TS(StimulusType guessedStimulusType, double timestamp, double timestamp_NoPauses)
        {
            if (guessedStimulusType == StimulusType.None)
            {
                AsyncThread.RunOnMainThread_ASAP_TS(() =>
                {
                    this.LogWarning("Reported StimulusType 'None' - This Shouldn't happen. Aborting.");
                });
                return;
            }

            TaskRelevantResponse response = TaskRelevantResponse.Unknown;

            // We're outside the window
            if (currentWindowStimulusType == null)
                response = TaskRelevantResponse.AfterWindowPress; // Should re-examine if we revive this code! (used to be unified Before and After into Outside Window)
            else
            {
                lock (gotResponseWithinWindow_Lock)
                {
                    // There ISN'T a stim!
                    if (currentWindowStimulusType == StimulusType.None)
                        response = gotResponseWithinWindow ?
                            TaskRelevantResponse.BlankAdditionalPress :
                            TaskRelevantResponse.BlankYes;

                    // .. or it wasn't the one that was guessed
                    else if (guessedStimulusType != currentWindowStimulusType ||
                        // .. or it wasn't the one they were supposed to be looking for
                        isLookingForSpecificStimuliType && lookForSpecificStimulusType != currentWindowStimulusType)
                        response = gotResponseWithinWindow ?
                            TaskRelevantResponse.WrongStimAdditionalPress :
                            TaskRelevantResponse.WrongStimYes;

                    // ..and there IS one!
                    else
                        response = gotResponseWithinWindow ?
                            TaskRelevantResponse.CorrectStimAdditionalPress :
                            TaskRelevantResponse.CorrectStimYes;

                    gotResponseWithinWindow = true;
                }
            }

            RaiseOnReportChance_TS(response, timestamp, timestamp_NoPauses);
        }

        private object timestampWindowStartMS_Lock = new object();
        private double timestampWindowStartMS;
        private object timestampWindowEndMS_Lock = new object();
        private double timestampWindowEndMS;

        // Reset stimulus chance
        // [SOS] This is called DURING the frame cycle LEADING to the frame where stim will be rendered
        private void StimulusManager_onStimulusOnset(object sender, StimulusEventArgs e)
        {
            if (!isTracking) return;
            // Debug.LogError("A :: " + TimeWrapper.GetCurrentTimestamp_TS());

            // Mark window start MS at the beginning of the next frame
            AsyncThread.RequestRunOnNewThread_OnNextFrame(() =>
            {
                // From there, setup the time values
                /// [SOS] Using <see cref="StimulusManager.lastTimeStimulusShown_MS"/> here may be pointing to the previous stim, not the one just rendered
                lock (timestampWindowStartMS_Lock)
                {
                    timestampWindowStartMS = TimeWrapper.lastRenderedFrame_TimeOfRenderMS + config_reactionTimeMS;
                    lock (timestampWindowEndMS_Lock)
                        timestampWindowEndMS = timestampWindowStartMS + config_reactionWindowMS;
                }
                double timestampWindowStartMS_NoPauses = TimeWrapper.lastRenderedFrame_TimeOfRender_SessionPlayTime_MS_NoPauses + config_reactionTimeMS;

                // Debug.LogError("B :: " + TimeWrapper.GetCurrentTimestamp_TS());

                // Wait for the window to begin
                int waitTime = 0;

                lock (timestampWindowEndMS_Lock)
                    waitTime = (int)(timestampWindowStartMS - TimeWrapper.currentTimestampMS);
                AsyncThread.Sleep(waitTime);

                lock (currentWindowStimulus_Lock)
                    currentWindowStimulus = e.stimulus;

                lock (gotResponseWithinWindow_Lock)
                    gotResponseWithinWindow = false;

                ExperimentManagerSession.LogData_AsTheyHappen_TS(
                    new TimeWrapper.Timestamp(-1, TimeWrapper.currentTimestampMS, TimeWrapper.currentTimestampMS_NoPauses),
                    "STIMULUS_REPORT_TOOL", "WINDOW_START");

                // Debug.LogError("C :: " + TimeWrapper.GetCurrentTimestamp_TS());

                // Wait for the window to end
                lock (timestampWindowStartMS_Lock)
                    waitTime = (int)(timestampWindowEndMS - TimeWrapper.currentTimestampMS);

                AsyncThread.Sleep(waitTime);

                PostWindowCleanup_TS();
            });
        }

        // Clean-Up on the frame following the true window time end
        private void PostWindowCleanup_TS()
        {
            // Debug.LogError("D :: " + TimeWrapper.GetCurrentTimestamp_TS());

            // [SPAGHETTI] while we know when we want the window to end ahead of time ,
            // we can't know that time in terms of non-pause playtime (because we don't know how much pausing will happen)
            // But, at this point, we know the current time, the intended window end, so we get a small dT indicating distance from NOW to window end
            // Then we apply that dT to the NOW (without pauses) to get the window end (without pauses)

            // Calculate the dT from last rendered frame to our window end ms
            double dT = TimeWrapper.currentTimestampMS - timestampWindowEndMS;
            double timestampWindowEnd_NoPauses = TimeWrapper.currentTimestampMS_NoPauses - dT;

            // We didn't respond and the window ended
            lock (gotResponseWithinWindow_Lock)
                if (!gotResponseWithinWindow)
                {
                    TaskRelevantResponse response = TaskRelevantResponse.Unknown;

                    // There ISN'T a stim!
                    if (currentWindowStimulusType == StimulusType.None)
                        response = TaskRelevantResponse.BlankNoResponse;
                    // .. or it wasn't the one they were supposed to be looking for
                    else if (isLookingForSpecificStimuliType && lookForSpecificStimulusType != currentWindowStimulusType)
                        response = TaskRelevantResponse.WrongStimNoResponse;
                    // ..and there IS one!
                    else
                        response = TaskRelevantResponse.CorrectStimNoResponse;

                    RaiseOnReportChance_TS(response, timestampWindowEndMS, timestampWindowEnd_NoPauses); // This cleanup happens Off-Frame, but we know accurately when we stopped accepting input as "within window"
                }

            onWindowEnd_Thread?.Invoke(this, new TimeWrapper.Timestamp(TimeWrapper.currentFrameCycleID, timestampWindowEndMS, timestampWindowEnd_NoPauses));

            lock (currentWindowStimulus_Lock)
                currentWindowStimulus = null;
        }

        private double? lastReportedTimestamp;

        private void RaiseOnReportChance_TS(TaskRelevantResponse response, double timestamp, double timestampNoPauses)
        {
            // Debug.LogError("E :: " + TimeWrapper.GetCurrentTimestamp_TS());
            if (timestamp == lastReportedTimestamp)
            {
                if (EngineWrapper.Debug_IsDebugBuild)
                    Debug.LogError("Already sent a report on that exact timestamp! Aborting");
                return;
            }

            lastReportedTimestamp = timestamp;

            if (debug && EngineWrapper.Debug_IsDebugBuild)
                Debug.LogError("{0} -> {1}"._Format(response, timestamp));

            // Iff it's a no-response "response" then we know the window's exact no-pausing timestamp
            // Otherwise, we don't (because we answered before it ended, and we can't a-priori know how much pausing we'll have in-between)
            double timestampWindowEndMS_NoPauses =
                response == TaskRelevantResponse.BlankNoResponse ||
                response == TaskRelevantResponse.CorrectStimNoResponse ||
                response == TaskRelevantResponse.WrongStimNoResponse ? timestampNoPauses : -1;

            lock (currentWindowStimulus_Lock)
                lock (timestampWindowEndMS_Lock)
                    onReportChance_TS?.Invoke(this, new ReportChanceEventArgs(response, timestamp, timestampNoPauses,
                        StimulusManager.lastCyclceIdx,
                        StimulusManager.lastTimeStimulusShown_MS,
                        StimulusManager.lastTimeStimulusShown_MS_RunElapsed_NoPauses,
                        currentWindowStimulus,
                        timestampWindowEndMS, timestampWindowEndMS_NoPauses, !AsyncThread.isMainThread_TS));
        }

        #endregion

    }

    public class ReportChanceEventArgs : EventArgs
    {
        public StimulusReportTool.TaskRelevantResponse response;
        public double responseTS;
        public double responseTS_NoPauses;
        public int stimulusID;
        public double stimulusHiddenTS;
        public double stimulusHiddenTS_NoPauses;
        public SpriteLocation? stimulus;
        public double windowEndTS;
        public double windowEndTS_NoPauses;
        public bool repliedUsingHighAccu;

        public ReportChanceEventArgs(StimulusReportTool.TaskRelevantResponse response, double responseTS, double responseTS_NoPauses, int stimulusID, double stimulusHiddenTS, double stimulusHiddenTS_NoPauses, SpriteLocation? stimulus, double windowEndTS, double windowEndTS_NoPauses, bool repliedUsingHighAccu)
        {
            this.response = response;
            this.responseTS = responseTS;
            this.responseTS_NoPauses = responseTS_NoPauses;
            this.stimulusID = stimulusID;
            this.stimulusHiddenTS = stimulusHiddenTS;
            this.stimulusHiddenTS_NoPauses = stimulusHiddenTS_NoPauses;
            this.stimulus = stimulus;
            this.windowEndTS = windowEndTS;
            this.windowEndTS_NoPauses = windowEndTS_NoPauses;
            this.repliedUsingHighAccu = repliedUsingHighAccu;
        }

        internal string stimulusName { get { return stimulus?.spriteName; } }
        internal StimulusType? stimulusType { get { return stimulus?.type; } }
        internal Direction_2D_Diagonal? stimulusLocation { get { return stimulus?.direction; } }

    }
}