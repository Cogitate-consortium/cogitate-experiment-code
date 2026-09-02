// NS_REMOVE
using ExperimentLibrary;
using Experiment.Managers;
// NS_REMOVE
using Game.Managers.GameplayManager;

using System;
using TGP.Helpers;
using UnityEngine;
using System.Collections.Generic;

namespace Helpers.Engine
{
    /// <summary>
    /// [SOS] Make sure to call <see cref="Initialize"/> before using
    /// Wraps and Extends <see cref="Time"/>.
    /// </summary>
    public static class TimeWrapper
    {
        // TODO
        public static double currentFrameCycleBeginMS
        {
            get
            {
                return (ExperimentManagerSession.session_probeSummary.IsNull() ? 0 :
    (currentFrameCycleBeginUTC - ExperimentManagerSession.session_probeSummary.sessionStartTime).TotalMilliseconds) + timestampMSOffset;
            }
        }

        // SOS - keep thread safe!!
        public static double currentTimestampMS
        {
            get
            {
                return (ExperimentManagerSession.session_probeSummary.IsNull() ? 0 :
    (DateTime.UtcNow - ExperimentManagerSession.session_probeSummary.sessionStartTime).TotalMilliseconds) + timestampMSOffset;
            }
        }

        /// <summary>
        /// MAYBE Reposition? ?? It kinda makes more sense in the context of the GAME rather than the helper
        /// </summary>
        public static double currentTimestampMS_NoPauses
        {
            get
            {
                if (ExperimentManagerSession.session_probeSummary == null) return -1;

                float session_playTime_AtLevelStart_NoPauses = 0;

                if (ExperimentManagerSession.session_probeSummary?.replaySum != null)
                    session_playTime_AtLevelStart_NoPauses += ExperimentManagerSession.session_probeSummary.replaySum.Run_ReplayRunTime_NoPauses;

                if (ExperimentManagerSession.journeySum != null)
                    session_playTime_AtLevelStart_NoPauses += ExperimentManagerSession.journeySum.Run_JourneyRunTime_NoPauses;

                float session_playTime_NoPauses = session_playTime_AtLevelStart_NoPauses +
                    // [SPAGHETTI]
                    GameManager.elapsedTimeSeconds_Level_NoPauses;

                return session_playTime_NoPauses * 1000 + timestampMSOffset; // it's in seconds!
            }
        }


        #region Constants

        /// <summary>
        /// Appears in all DEBUG messages
        /// </summary>
        const string NAME = "TIME_WRAPPER";

        #endregion

        #region Public Variables

        // public static int currentFrameCycle { get; private set; }
        // public static DateTime lastAnchorUTC { get; private set; }

        public static DateTime currentFrameCycleBeginUTC { get; private set; }

        public static EventHandler<FrameInfo> onFrameRendered;
        public static EventHandler<DebugInfo> onDebugMessageSent;
        public static int averageFrameDT_Seconds;

        public static double currentTimestamp_DTFrom_CycleBegin
        {
            get
            {
                return currentTimestampMS - currentFrameCycleBeginMS;
            }
        }

        /// <summary>
        /// When the LAST frame was rendered - should match <see cref="currentFrameCycleBeginMS"/>
        /// </summary>
        public static double lastRenderedFrame_TimeOfRenderMS { get; private set; }
        public static double lastRenderedFrame_TimeOfRender_SessionPlayTime_MS_NoPauses { get; private set; }
        public static double lastRenderedFrame_TimeOfRender_LevelPlayTime_NoPauses { get; private set; }

        // public static double deltaTime_SinceLastRenderedFrame { get { return currentTimestampMS - lastRenderedFrame_TimeOfRenderMS; } }


        /// <summary>
        /// The Nth frame cycle ends with the rendering of the Nth frame
        /// </summary>
        public static int numRenderedFrames { get; private set; }
        
        /// <summary>
        /// After rendering the Nth frame, we begin the N+1th frame cycle
        /// </summary>
        public static int currentFrameCycleID { get; private set; }

        #endregion

        #region Public Functions

        private static Timestamp? overrideTimestamp = null;

        public static void Initialize()
        {
            EngineWrapper.onUpdate -= Engine_onUpdate;
            EngineWrapper.onUpdate += Engine_onUpdate;
            EngineWrapper.onFixedUpdate -= Engine_onFixedUpdate;
            EngineWrapper.onFixedUpdate += Engine_onFixedUpdate;
            EngineWrapper.onGUI -= Engine_onGUI;
            EngineWrapper.onGUI += Engine_onGUI;
            EngineWrapper.Initialize();
        }

        // https://www.nimaara.com/high-resolution-clock-in-net/
        public static double DoAccuracyTest(int seconds)
        {
            var duration = TimeSpan.FromSeconds(seconds);
            var distinctValues = new HashSet<DateTime>();
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();

            while (stopWatch.Elapsed < duration)
            {
                distinctValues.Add(DateTime.UtcNow);
            }

            // Debug.Log("Samples: " + distinctValues.Count);
            // Debug.Log($"Accuracy: {stopWatch.Elapsed.TotalMilliseconds / distinctValues.Count:0.000000} ms");

            return stopWatch.Elapsed.TotalMilliseconds / distinctValues.Count;
        }

        public static void ResetOverrideTimestamp()
        {
            SetOverrideTimestamp(null);
        }

        public static void SetOverrideTimestamp(Timestamp? overrideTimestamp)
        {
            TimeWrapper.overrideTimestamp = overrideTimestamp;

            currentFrameCycleID = frameOffset;
            numRenderedFrames = frameOffset;
        }

        public static Timestamp GetLastFrameTimestamp_TS()
        {
            // Returns the current timestamp, not the frame cycle timestamp
            // Logs are aligned to their anchor at a later stage
            return new Timestamp(currentFrameCycleID - 1, lastRenderedFrame_TimeOfRenderMS, lastRenderedFrame_TimeOfRender_SessionPlayTime_MS_NoPauses);
        }

        public static Timestamp GetCurrentTimestamp_TS()
        {
            // Returns the current timestamp, not the frame cycle timestamp
            // Logs are aligned to their anchor at a later stage
            return new Timestamp(currentFrameCycleID, currentTimestampMS, currentTimestampMS_NoPauses);
        }

        #endregion

        #region Unity Time Wrapper
        public static float timeScale_NotTS
        {
            get { return Time.timeScale; }
            set { Time.timeScale = value; }
        }

        /// <summary>
        /// In Seconds
        /// </summary>
        public static float time_NotTS
        {
            get { return Time.time; }
        }

        /// <summary>
        /// </summary>
        public static float deltaTime_SinceLastUpdate_NotTS
        {
            get { return Time.deltaTime; }
        }

        public static float timeSinceLevelLoad_NotTS
        {
            get { return Time.timeSinceLevelLoad; }
        }

        public static float realtimeSinceStartup_NotTS
        {
            get { return Time.realtimeSinceStartup; }
        }

        public static float fixedDeltaTime_NotTS
        {
            get { return Time.fixedDeltaTime; }
            set { Time.fixedDeltaTime = value; }
        }

        public static int frameCount_NotTS
        {
            get { return Time.frameCount + frameOffset; }
        }

        public static int renderedFrameCount_NotTS
        {
            get { return Time.renderedFrameCount + frameOffset; }
        }

        private static int frameOffset { get { return overrideTimestamp.HasValue ? overrideTimestamp.Value.frameCycleID : 0; } }
        private static double timestampMSOffset { get { return overrideTimestamp.HasValue ? overrideTimestamp.Value.timestampMS : 0; } }
        #endregion

        #region public Management
        private static void CheckForFrameBegin(string loggingEvent)
        {
            // Entered a new frame cycle!
            if (renderedFrameCount_NotTS > currentFrameCycleID)
            {
                // Log the anchor of the last frame (before updating current frame cycle num / its timestamp)
                {
                    // [SOS] first get dT then udpate
                    double dT = currentTimestampMS - lastRenderedFrame_TimeOfRenderMS; // currentTimestampMS - currentFrameCycleTimestampMS;

                    // [SOS] first update then log
                    lastRenderedFrame_TimeOfRenderMS = currentTimestampMS;
                    lastRenderedFrame_TimeOfRender_SessionPlayTime_MS_NoPauses = currentTimestampMS_NoPauses;
                    // Debug.LogError(totalPlayTime_NoPauses);

                    // [SPAGHETTI]
                    lastRenderedFrame_TimeOfRender_LevelPlayTime_NoPauses = GameManager.elapsedTimeSeconds_Level_NoPauses;

                    numRenderedFrames = numRenderedFrames + 1;

                    // [SOS] first update then log
                    SendDebugMessage("FRAME_JUST_RENDERED;{0};{1};{2};{3}". // "[0] numRenderedFrames (incl this one), [1] anchorTimestamp, [2] dTFromLastRenderedFrame, [3] loggingEvent"
                            _Format(numRenderedFrames, currentTimestampMS, dT, loggingEvent));
                }

                // Log the frame start of this frame (this updates current frame cycle num and its timestamp)
                {
                    // Update the frame counter
                    currentFrameCycleBeginUTC = DateTime.UtcNow;
                    currentFrameCycleID = numRenderedFrames + 1;

                    // Log the frame (AFTER the above update)

                    double dT = currentTimestampMS - currentFrameCycleBeginMS; // currentTimestampMS - currentFrameCycleTimestampMS;

                    SendDebugMessage("FRAME_CYCLE_BEGIN;{0};{1};{2};{3}". // "[0] frameCycleNum, [1] anchorTimestamp, [2] dTFromLastFrameBegin, [3] loggingEvent"
                        _Format(currentFrameCycleID, currentTimestampMS, dT, loggingEvent));
                }

                // Do this at the very end
                {
                    onFrameRendered?.Invoke(null, new FrameInfo());
                }
            }
        }
        #endregion

        #region Helper Variables

        #endregion

        #region Handlers
        private static void Engine_onUpdate(object sender, float e)
        {
            CheckForFrameBegin("Update");
        }

        private static void Engine_onFixedUpdate(object sender, float e)
        {
            CheckForFrameBegin("FixedUpdate");
        }

        private static void Engine_onGUI(object sender, EventArgs e)
        {
            CheckForFrameBegin("OnGUI");
        }
        #endregion

        #region Helper Functions
        private static void SendDebugMessage(string message)
        {
            onDebugMessageSent?.Invoke(null, new DebugInfo(NAME, message));
        }

        #endregion

        #region Helper Classes
        public class FrameInfo : EventArgs
        {
            // contains all the static vars
        }

        public struct Timestamp
        {
            public int frameCycleID;
            public double timestampMS;
            public double timestampMS_NoPauses;

            public Timestamp(int frameCycleID, double timestampMS, double timestampMS_NoPauses)
            {
                this.frameCycleID = frameCycleID;
                this.timestampMS = timestampMS;
                this.timestampMS_NoPauses = timestampMS_NoPauses;
            }

            public override string ToString()
            {
                return "{0}_{1}_{2}"._Format(frameCycleID, timestampMS, timestampMS_NoPauses);
            }
        }
        /*
        internal static void OverrideCurrentTimestamp(int frameID, double timestampMS)
        {
            currentFrameCycleID = frameID;
            ExperimentManagerSession.session_probeSummary.OverrideStartTime(timestampMS);
        }*/
        #endregion
    }
}