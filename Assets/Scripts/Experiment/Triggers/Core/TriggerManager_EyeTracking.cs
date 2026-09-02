// NS_REMOVE | Logs
using Experiment.Managers;

using Helpers.Engine;
using System;
using TGP.Helpers;
using Peripherals.EyeTracking;
using Helpers.Async;

namespace Experiment.Triggers.Core
{
    public class TriggerManager_EyeTracking : TriggerManager
    {
        private static bool config_debug;
        private Config config;
        private RuntimeConfig runtimeConfig;

        public TriggerManager_EyeTracking(Config config, RuntimeConfig runtimeConfig)
            : base(config, runtimeConfig)
        {
            this.config = config;
            this.runtimeConfig = runtimeConfig;

            config_debug = config.debug;

            // LPTManager.Initialize(config.lptManager);
        }

        public override void Dispose()
        {
            // LPTManager.Deinitialize();
        }

        public void Send_Now_TS(TriggerMaster.TriggerEventArgs e)
        {
            Send_TS(e, RequestType.Instant);
        }

        public void Send_NextFrame_TS(TriggerMaster.TriggerEventArgs e)
        {
            AsyncThread.RequestRunOnNewThread_OnNextFrame(() =>
            {
                Send_TS(e, RequestType.NextFrame);
            });
        }

        private void Send_TS(TriggerMaster.TriggerEventArgs e, RequestType requestType)
        {
            Report_AsItHappens_TS("REQUEST_RECEIVED", e.type, e.codePrio.code, e.codePrio.prio);

            object codeSTR = GetCodeSTR(e.codePrio.code);

            if (!ShouldSend(e.type))
            {
                Report_AsItHappens_TS("REQUEST_DENIED", e.type, codeSTR, e.codePrio.prio);

                return;
            }

            if (runtimeConfig.eyeTracker == null)
            {
                Report_AsItHappens_TS("REQUEST_FAILED_NO_EYETRACKER", e.type, codeSTR, e.codePrio.prio);

                return;
            }

            string error = "";
            if (runtimeConfig.eyeTracker.RequestWriteToTracker_TS(codeSTR, out error))
                Report_AsItHappens_TS("TRIGGER_SENT", e.type, codeSTR);
            else
                Report_AsItHappens_TS("TRIGGER_FAILED", e.type, codeSTR, error);
        }

        private object GetCodeSTR(int code)
        {
            object codeSTR = null;

            if (code <= config.maxShortPerSend)
                codeSTR = code;
            else
            {
                short codeMSB = 0;
                short codeLSB = 0;

                Math_Helper.BreakIntoShorter(code, config.maxShortPerSend, out codeMSB, out codeLSB);

                codeSTR = "{0}_{1}"._Format(codeMSB, codeLSB);
            }

            if (!config.prependString.IsNullOrEmpty())
                codeSTR = "{0};{1}"._Format(config.prependString, codeSTR);

            return codeSTR;
        }

        private void Report_AsItHappens_TS(string result, object triggerEventString, object data, object info = null)
        {
            ExperimentManagerSession.LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(), "TRIGGER_MANAGER_EYETRACKING",
                "{0};{1};{2};{3}"._Format(result, triggerEventString, data, info), config_debug); // [0] result, [1] event, [2] data, [3] info
        }

        [Serializable]
        public new class Config : TriggerManager.Config
        {
            public short maxShortPerSend = 255;
            public string prependString = "TRIGGER";
        }

        public new class RuntimeConfig : TriggerManager.RuntimeConfig
        {
            public EyeTrackerManager_Base eyeTracker;
            public RuntimeConfig(Func<TriggerOutEvent, bool> shouldFire, EyeTrackerManager_Base eyeTracker) : base(shouldFire)
            {
                this.eyeTracker = eyeTracker;
            }
        }
    }
}