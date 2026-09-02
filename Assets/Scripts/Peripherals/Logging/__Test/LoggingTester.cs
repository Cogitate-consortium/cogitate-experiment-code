using Helpers.Engine;
using Peripherals.Logging.BackgroundWriting;
using Peripherals.Logging.Test.Core;
using System;
using System.Collections;
using TGP.Helpers;
using UnityEngine;

namespace Peripherals.Logging.Test
{
    public static class LoggingTester
    {
        private static Config config;

        public static event EventHandler<EventArgs<LoggingAnalyzer.Report>> onReportReady;

        public static void Initialize(Config config)
        {
#if !UNITY_EDITOR
            if (!config.enabled) return;
#endif

            LoggingTester.config = config;

            EngineWrapper.Initialize();

            BackgroundWriter.onStatusUpdate -= BW_onStatusUpdate;
            BackgroundWriter.onStatusUpdate += BW_onStatusUpdate;

            LoggingAnalyzer.Initialize(config.analyzer);

            EngineWrapper.StartCoroutine(DebugLoggerIE());
        }

        private static IEnumerator DebugLoggerIE()
        {
            while (true)
            {
                yield return new WaitUntil(() => config.debugLevel == LogDebuggingLevel.Realtime);

                if (config.debugEverySeconds > 0)
                    yield return new WaitForSeconds(config.debugEverySeconds);
                else
                    yield return new WaitForEndOfFrame();

                LoggingAnalyzer.Report report = LoggingAnalyzer.TryDoFormattedReports();

                onReportReady?.Invoke(null, report);
            }
        }

#region Logger Wrapper
        private static void BW_onStatusUpdate(object sender, BackgroundWriter.StatusUpdate e)
        {
            if (e.transitionalEvent == BackgroundWriter.Event.Started &&
                config.debugLevel == LogDebuggingLevel.OnStartStop)
            {
                Debug_Helper.LogError(typeof(LoggingTester), "Starting background data writing");
                LoggingAnalyzer.DoFormattedReports();
            }
            else if (e.transitionalEvent == BackgroundWriter.Event.Stopped &&
                config.debugLevel == LogDebuggingLevel.OnStartStop)
            {
                Debug_Helper.LogError(typeof(LoggingTester), "Stopping background data writing");
                LoggingAnalyzer.DoFormattedReports();
            }
        }

#endregion

        [Serializable]
        public class Config
        {
            public bool enabled = false;
            public string debugLevel_Comment = "None = 0, PerLevel = 1, Realtime = 2";
            public LogDebuggingLevel debugLevel = LogDebuggingLevel.OnStartStop;
            public float debugEverySeconds = 5;
            public LoggingAnalyzer.Config analyzer;
        }
    }

    public enum LogDebuggingLevel { None = 0, OnStartStop = 1, Realtime = 2 }
}