// NS_REMOVE | Logs
using Experiment.Managers;

using Helpers.Engine;
using Peripherals.LPT;
using System;
using TGP.Helpers;

namespace Experiment.Triggers.Core
{
    public class TriggerManager_LPT : TriggerManager
    {
        private static bool config_debug;
        private Config config;
        private RuntimeConfig runtimeConfig;

        public TriggerManager_LPT(Config config, RuntimeConfig runtimeConfig)
            : base(config, runtimeConfig)
        {
            this.config = config;
            this.runtimeConfig = runtimeConfig;

            LPTManager.Initialize(config.lptManager);
        }

        public override void Dispose()
        {
            LPTManager.Deinitialize();
        }

        public void Send_Now_TS(TriggerMaster.TriggerEventArgs e)
        {
            Send_TS(e, RequestType.Instant);
        }

        public void Send_NextFrame_TS(TriggerMaster.TriggerEventArgs e)
        {
            Send_TS(e, RequestType.NextFrame);
        }

        private void Send_TS(TriggerMaster.TriggerEventArgs e, RequestType requestType)
        {
            Report_AsItHappens_TS(e.type, e.codePrio, "REQUEST_RECEIVED");

            if (!ShouldSend(e.type))
            {
                Report_AsItHappens_TS(e.type, e.codePrio, "REQUEST_DENIED");

                return;
            }

            LPTManager.Send_TS(e.type, e.codePrio.code, e.codePrio.prio, requestType);
        }

        private void Report_AsItHappens_TS(object triggerEventString, object data, string result)
        {
            ExperimentManagerSession.LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(), "TRIGGER_MANAGER_LPT",
                "{0};{1};{2}"._Format(result, triggerEventString, data), config_debug); // [0] type, [1] code
        }

        [Serializable]
        public new class Config : TriggerManager.Config
        {
            public LPTManager.Config lptManager;
        }

        public new class RuntimeConfig : TriggerManager.RuntimeConfig
        {
            public RuntimeConfig(Func<TriggerOutEvent, bool> shouldFire) : base(shouldFire)
            {
            }
        }
    }
}