using System;
using Helpers.Engine;
using Peripherals.Logging.BackgroundWriting;
using Peripherals.Logging.Core;
using Peripherals.Logging.Timestamping;
using TGP.Helpers;

namespace Peripherals.Logging.Test.Core
{
    public static class LoggingAnalyzer
    {
        private static object analyticsLock = new object();
        private static LoggingAnalytics analytics = new LoggingAnalytics();

        public static int logTotalData_AllLogs = 0;
        public static int logTotalData_ToBeWritten = 0;
        public static int logTotalData_Written = 0;

        public static string config_logData_FromSender = "";
        public static bool config_doAnalytics = false;
        public static string lastBatchToBeWritten = "";
        public static string lastLineWritten = "";

        private static int lastHash = -1;
        private static int lastDifferenceReported_PreWriting = -1;
        private static Config config;

        internal static void Initialize(Config config)
        {
            config_logData_FromSender = config.logData_FromSender;
            config_doAnalytics = config.doAnalytics;

            LoggingAnalyzer.config = config;
            LogStamper.onLogStatus_Thread += LogStamper_onLogStatus_TS;
            LogWrapper.onLogEvent_Thread += LogWrapper_onLogEvent_TS;
            BackgroundWriter.onLogEvent += BackgroundWriter_onLogEvent;
        }

        private static void LogStamper_onLogStatus_TS(object sender, LogEntry.StatusArgs e)
        {
            LogEvent_TS(e);
        }

        private static void LogEvent_TS(LogEntry.StatusArgs e)
        {
            if (!config_doAnalytics) return;

            if (!config_logData_FromSender.IsNullOrEmpty() && config_logData_FromSender != e.entry.sender) return;

            lock (analyticsLock)
            {
                switch (e.state)
                {
                    case LogEntry.State.Requested:
                        analytics.requested++;
                        break;
                    case LogEntry.State.RequestedShouldLog:
                        analytics.requestedShouldLog++;
                        break;
                    case LogEntry.State.AddedToPending:
                        analytics.addedToPending++;
                        break;
                    case LogEntry.State.AddedToWrite:
                        analytics.addedToWrite++;
                        break;
                    case LogEntry.State.ReadyToWrite:
                        analytics.readyToWrite++;
                        break;
                    case LogEntry.State.AddedToAllLogs:
                        analytics.addedToAllLogs++;
                        break;
                }
            }
        }

        private static void BackgroundWriter_onLogEvent(object sender, LogEntry.StatusArgs e)
        {
            LogEvent_TS(e);
        }

        private static void LogWrapper_onLogEvent_TS(object sender, LogEntry.StatusArgs e)
        {
            LogEvent_TS(e);
        }

        private static int lastDifferenceReported_Writing = -1;
        private static bool debug = false;

        public class Report
        {
            public string overview;
            public string preWriting;
            public string writing;

            public override string ToString()
            {
                return "{0}{1}{2}"._Format(
                    overview.IsNullOrEmpty() ? "" : overview + Environment.NewLine,
                    preWriting.IsNullOrEmpty() ? "" : preWriting + Environment.NewLine,
                    writing.IsNullOrEmpty() ? "" : writing + Environment.NewLine);
            }
        }

        public static Report TryDoFormattedReports(bool forceAsError = false)
        {
            lock (analyticsLock)
            {
                int currentHash =
                    analytics.GetHashCode() +
                    logTotalData_ToBeWritten * 3 +
                    logTotalData_Written * 5;

                if (currentHash == lastHash)
                    return null;

                lastHash = currentHash;

                return DoFormattedReports(forceAsError);
            }
        }

        public static Report DoFormattedReports(bool forceAsError = false)
        {
            if (analytics.addedToAllLogs < analytics.requested)
            {
                Debug_Helper.LogError(typeof(LoggingAnalyzer),
                    "Requested {0} | Added To All Logs {1}"._Format(analytics.requested, analytics.addedToAllLogs));
            }
            else
            {
                Debug_Helper.LogWarning(typeof(LoggingAnalyzer),
                    "Requested {0} |  Added To All Logs {1}"._Format(analytics.requested, analytics.addedToAllLogs));
                // Debug.Log("Logs All Good!");
            }

            Report report = new Report();

            report.overview = DoFormattedReport_Overview(forceAsError);
            report.preWriting = DoFormattedReport_PreWriting(forceAsError);
            report.writing = DoFormattedReport_Writing();

            return report;
        }

        public static string DoFormattedReport_Overview(bool doAsError)
        {
            if (analytics.addedToAllLogs < logTotalData_ToBeWritten)
            {
                Debug_Helper.LogError(typeof(LoggingAnalyzer),
                    "WTF :: analytics.addedToAllLogs {0} < logTotalData_ToBeWritten {1}"
                    ._Format(analytics.addedToAllLogs, logTotalData_ToBeWritten));
                // Debug.LogError("WTF :: logTotalData.AddedToAllLogs {0} ({1}) < logTotalData_ToBeWritten {2}"
                //    ._Format(logTotalData.addedToAllLogs, logTotalData_AllLogs, logTotalData_ToBeWritten));
            }

            string debugString = TimeWrapper.currentFrameCycleBeginMS + ": OVERVIEW\n" + GetFormattedLogReport_Overview() +
                        "\n[LAST_LINE_WRITTEN]: " + lastLineWritten;
            if (doAsError)
                Debug_Helper.LogError(typeof(LoggingAnalyzer), debugString);
            else
                Debug_Helper.Log(typeof(LoggingAnalyzer), debugString);

            return debugString;
        }

        public static string DoFormattedReport_PreWriting(bool doAsError)
        {
            int currentDifference = analytics.requested - logTotalData_ToBeWritten;
            // if (currentDifference == lastDifferenceReported_PreWriting) return;

            string debugString = "";

            if (currentDifference == 0)
            {
                debugString = TimeWrapper.currentFrameCycleBeginMS + ": PREWRITING_CORRECTED\nRequested == ToBeWritten ({0})"._Format(logTotalData_ToBeWritten) +
                    "\n[LAST_LINE_WRITTEN]: " + lastLineWritten;
            }
            else if (currentDifference < lastDifferenceReported_PreWriting)
            {
                debugString = TimeWrapper.currentFrameCycleBeginMS + ": PREWRITING_IMPROVED ({0} -> {1})\n{2}"._Format(
                    lastDifferenceReported_PreWriting, currentDifference, GetFormattedLogReport_PreWriting()) +
                    "\n[LAST_LINE_WRITTEN]: " + lastLineWritten;
            }
            else
            {
                debugString = TimeWrapper.currentFrameCycleBeginMS + ": PREWRITING_BROKE ({0} -> {1})\n{2}"._Format(
                    lastDifferenceReported_PreWriting, currentDifference, GetFormattedLogReport_PreWriting()) +
                    "\n[LAST_LINE_WRITTEN]: " + lastLineWritten;
            }

            if (doAsError)
                Debug_Helper.LogError(typeof(LoggingAnalyzer), debugString);
            else
                Debug_Helper.Log(typeof(LoggingAnalyzer), debugString);

            lastDifferenceReported_PreWriting = currentDifference;

            return debugString;
        }

        public static string DoFormattedReport_Writing()
        {
            int currentDifference = logTotalData_ToBeWritten - logTotalData_Written;
            // if (currentDifference == lastDifferenceReported_Writing) return;

            string debugString = "";

            if (currentDifference == 0)
            {
                debugString = TimeWrapper.currentFrameCycleBeginMS +
                    ": WRITING_CORRECTED\nToBeWritten == Written ({0})"._Format(logTotalData_Written) +
                    "\n[LAST_LINE_WRITTEN]: " + lastLineWritten;
            }
            else if (currentDifference < lastDifferenceReported_Writing)
            {
                debugString = TimeWrapper.currentFrameCycleBeginMS + ": WRITING_IMPROVED ({0} -> {1}) \n".
                    _Format(lastDifferenceReported_Writing, currentDifference) + GetFormattedLogReport_Writing() +
                    "\n[LAST_LINE_WRITTEN]: " + lastLineWritten;
            }
            else
            {
                debugString = TimeWrapper.currentFrameCycleBeginMS + ": WRITING_BROKE ({0} -> {1}) \n{2}\n{3}"._Format(
                    lastDifferenceReported_Writing, currentDifference, GetFormattedLogReport_Writing(), lastBatchToBeWritten) +
                    "\n[LAST_LINE_WRITTEN]: " + lastLineWritten;
            }

            Debug_Helper.Log(typeof(LoggingAnalyzer), debugString);

            lastDifferenceReported_Writing = currentDifference;

            return debugString;
        }

        public static string GetFormattedLogReport_Overview()
        {
            return (config_logData_FromSender == "" ? "ALL_LOGS" : config_logData_FromSender) + ":\n" +
                "TOTAL_DATA]: " + analytics.ToString() + "\n" +
                "[WRTNG_DATA]: ToBeWritten {0}, Written {1}"._Format(
                logTotalData_ToBeWritten, logTotalData_Written);
            // "[WRTNG_DATA]: AllLogs {0}, ToBeWritten {1}, Written {2}"._Format(
            //    logTotalData_AllLogs, logTotalData_ToBeWritten, logTotalData_Written);
        }

        public static string GetFormattedLogReport_PreWriting()
        {
            return (config_logData_FromSender == "" ? "ALL_LOGS" : config_logData_FromSender) + ":\n" +
                "[TOTAL_DATA]: " + analytics.ToString() + "\n" +
                "[WRTNG_DATA]: ToBeWritten {0} "._Format(logTotalData_ToBeWritten);
            // "[WRTNG_DATA]: AllLogs {0}, ToBeWritten {1} "._Format(logTotalData_AllLogs, logTotalData_ToBeWritten);
        }

        public static string GetFormattedLogReport_Writing()
        {
            return (config_logData_FromSender == "" ? "ALL_LOGS" : config_logData_FromSender) +
                ": ToBeWritten {0}, Written {1}\n"._Format(
                logTotalData_ToBeWritten, logTotalData_Written);
            // ": AllLogs {0}, ToBeWritten {1}, Written {2}\n"._Format(
            // logTotalData_AllLogs, logTotalData_ToBeWritten, logTotalData_Written);
        }

        [Serializable]
        public class Config
        {
            public string logData_FromSender = "TRIGGER_MANAGER_PHOTODIODE";
            public bool doAnalytics = true;
        }
    }
}