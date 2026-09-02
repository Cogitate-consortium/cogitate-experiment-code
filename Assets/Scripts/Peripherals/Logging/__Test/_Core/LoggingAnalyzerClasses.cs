using TGP.Helpers;

namespace Peripherals.Logging.Test.Core
{
    public struct LoggingAnalytics
    {
        public int requested;
        public int requestedShouldLog;
        public int addedToPending;
        public int addedToWrite;
        public int readyToWrite;
        /// <summary>
        /// At allLogs.AppendLine()
        /// </summary>
        public int addedToAllLogs;

        public LoggingAnalytics(int requested, int requestedShouldLog, int addedToPending, int addedToWrite, int addedToLevelData, int addedToAllLogs)
        {
            this.requested = requested;
            this.requestedShouldLog = requestedShouldLog;
            this.addedToPending = addedToPending;
            this.addedToWrite = addedToWrite;
            this.readyToWrite = addedToLevelData;
            this.addedToAllLogs = addedToAllLogs;
        }

        public override string ToString()
        {
            return ("Requested {0}, RequestedShouldLog {1}, AddedToPending {2}, " +
                "AddedToWrite {3}, AddedToLevelData {4}, AddedToAllLogs {5}")._Format(
                requested, requestedShouldLog, addedToPending,
                addedToWrite, readyToWrite, addedToAllLogs);
        }

        public override int GetHashCode()
        {
            return
               requested +
               requestedShouldLog * 3 +
               addedToPending * 5 +
               addedToWrite * 7 +
               readyToWrite * 11 +
               addedToAllLogs * 13;
        }

        public static LoggingAnalytics operator +(LoggingAnalytics a, LoggingAnalytics b)
        {
            return new LoggingAnalytics(
                a.requested + b.requested,
                a.requestedShouldLog + b.requestedShouldLog,
                a.addedToPending + b.addedToPending,
                a.addedToWrite + b.addedToWrite,
                a.readyToWrite + b.readyToWrite,
                a.addedToAllLogs + b.addedToAllLogs);
        }
    }

}