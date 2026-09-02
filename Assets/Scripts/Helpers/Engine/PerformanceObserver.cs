using System;
using System.Collections;
using TGP.Helpers;
using TGP.Helpers.Filters;
using UnityEngine;

namespace Helpers.Engine
{
    /// <summary>
    /// [SOS] Make sure to call <see cref="Initialize"/> before using
    /// </summary>
    public static class PerformanceObserver
    {
        private static bool isInitialized = false;

        // Maybe move to TIME ??
        /// <summary>
        /// [SOS] Use this instead of <see cref="TimeWrapper.deltaTime_SinceLastUpdate_NotTS"/> | 
        /// From update to Update Time.deltaTime is accurately representing the time between two frames | 
        /// Used anywhere else in the code, Time.deltaTime represents the time since last 
        /// </summary>
        public static float averageFrameDT_Seconds { get { return averageFrameDT.runningAverage; } }

        // private const int qualityLevel = 0;
        private const float updateEverySeconds = 1f;
        private const bool resetEveryUpdate = false;

        private static double lastFrameTS = -1;
        private static Config config;

        public static void Initialize(Config config)
        {
            if (isInitialized) return;
            PerformanceObserver.config = config;

            averageFrameDT = new RunningAverage(config.runningAverage_newObservationWeight, config.runningAverage_amplifier);

            // Make the game run as fast as possible
            // Application.targetFrameRate = 1000000;
            // QualitySettings.SetQualityLevel(qualityLevel);
            averageFrameDT.Reset();
            averageFrameDT.runningAverage = 1f / Application.targetFrameRate;

            // Observing
            // Engine.onUpdate += Engine_onUpdate;
            TimeWrapper.Initialize();
            TimeWrapper.onFrameRendered += TimeWrapper_onFrameRendered;

            // Showing
            EngineWrapper.Initialize();
            EngineWrapper.StartCoroutine(TrackPerformanceIE());

            isInitialized = true;
        }

        private static void TimeWrapper_onFrameRendered(object sender, TimeWrapper.FrameInfo e)
        {
            if (lastFrameTS > 0)
            {
                float newObservationSeconds = (float)((TimeWrapper.currentTimestampMS - lastFrameTS) / 1000);
                // Debug.LogError(newObservation);

                averageFrameDT.AddAndQuery(newObservationSeconds);
            }

            lastFrameTS = TimeWrapper.currentTimestampMS;
        }

        private static IEnumerator TrackPerformanceIE()
        {
            averageFrameDT.Reset();

            while (true)
            {
                if (resetEveryUpdate)
#pragma warning disable CS0162 // Unreachable code detected
                    averageFrameDT.Reset();
#pragma warning restore CS0162 // Unreachable code detected

                yield return new WaitForSeconds(updateEverySeconds);

                // Report it
                if (config.doReportOnGUI)
                {
                    float fps = 1 / averageFrameDT.runningAverage;
                    string fpsText = fps.Round(1).ToString("#");

                    Debug_MB.instance.OnGUI_AddMessage("{0} fps"._Format(fpsText), updateEverySeconds + TimeWrapper.deltaTime_SinceLastUpdate_NotTS);
                }
            }
        }

        /// <summary>
        /// [SOS] Use <see cref="RunningAverage.AddAndQuery(float, float)"/> of this in UPDATE only, to feed it <see cref="TimeWrapper.deltaTime_SinceLastUpdate_NotTS"/>
        /// </summary>
        private static RunningAverage averageFrameDT;

        [Serializable]
        public class Config
        {
            public bool doReportOnGUI = false;
            public float runningAverage_newObservationWeight = 0.1f;
            public float runningAverage_amplifier = 1.0f;
        }
    }
}