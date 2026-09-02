using Helpers.Engine;
using System;

namespace Peripherals.Logging.Core
{
    public enum TimestampingType { AsTheyHappen = 0, NextRenderedFrame = 1 }

    public class LogEntry
    {
        // public bool preserveFrameCycle { get { return frameCycleID >= 0; } }
        public bool preserveTimestamp { get { return timestampMS >= 0; } }

        /// <summary>
        /// Relevant to <see cref="LogWrapper.logsDirectory"/>
        /// </summary>
        public string filepathInLogs { get; private set; }
        public int frameCycleID;
        public double timestampMS;
        public double timestampMS_NoPauses;
        public string sender;
        public object data;
        public TimestampingType timestampingType;
        public double timestampDT_FromCycleBegin;

        public enum State
        {
            Requested, RequestedShouldLog,
            AddedToPending,
            AddedToWrite, ReadyToWrite,
            AddedToAllLogs
        }

        public class StatusArgs : EventArgs
        {
            public LogEntry entry;
            public State state;

            public StatusArgs(LogEntry entry, State state)
            {
                this.entry = entry;
                this.state = state;
            }
        }

        // For creating Overrides
        public LogEntry(TimeWrapper.Timestamp overrideTimestamp, string sender, object data, string filepathInLogs) :
            this(overrideTimestamp, TimestampingType.AsTheyHappen, sender, data, filepathInLogs)
        { }

        // For creating Others
        public LogEntry(TimestampingType timestampingType, string sender, object data, string filepathInLogs)
            : this(GetDefaultTimestamp(timestampingType), timestampingType, sender, data, filepathInLogs)
        { }

        private LogEntry(TimeWrapper.Timestamp timestamp, TimestampingType timestampingType, string sender, object data, string filepathInLogs)
        {
            this.filepathInLogs = filepathInLogs;

            frameCycleID = timestamp.frameCycleID;
            timestampMS = timestamp.timestampMS;
            timestampMS_NoPauses = timestamp.timestampMS_NoPauses;

            this.sender = sender;
            this.data = data;
            this.timestampingType = timestampingType;
            timestampDT_FromCycleBegin = timestampMS - TimeWrapper.currentFrameCycleBeginMS;
        }

        public override string ToString()
        {
            return string.Format("{0};{1};{2};{3};{4};{5}",
                frameCycleID, timestampMS, timestampMS_NoPauses, (int)timestampingType, sender, data, timestampDT_FromCycleBegin);
        }

        private static TimeWrapper.Timestamp GetDefaultTimestamp(TimestampingType timestampingType)
        {
            switch (timestampingType)
            {
                case TimestampingType.AsTheyHappen:
                    return TimeWrapper.GetCurrentTimestamp_TS();
                case TimestampingType.NextRenderedFrame:
                default:
                    return new TimeWrapper.Timestamp(TimeWrapper.currentFrameCycleID + 1, -1, -1);
            }
        }
    }
}