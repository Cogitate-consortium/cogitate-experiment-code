// NS_REMOVE | Logs
using ExperimentLibrary;
using Experiment.Managers;

// NS_DEBATABLE
using Helpers.Async; // Needed to run on next frame (?)

using Helpers.Engine;
using Peripherals.Photodiode;
using System;
using TGP.Helpers;

namespace Experiment.Triggers.Core
{
    public class TriggerManager_Photodiode : TriggerManager
    {
        private Config config;
        private RuntimeConfig runtimeConfig;

        private TriggerOutEvent lastTrigger = default;

        public TriggerManager_Photodiode(Config config, RuntimeConfig runtimeConfig)
            : base(config, runtimeConfig)
        {
            this.config = config;
            this.runtimeConfig = runtimeConfig;

            PhotoDiodeDebugger.Initialize(config.photodiodeManager);
        }

        public override void Dispose()
        {
            base.Dispose();
            PhotoDiodeDebugger.DeInitialize();
        }

        // Thread safe variant, to handle calls made from a thread
        public void Send_NextFrame_TS(TriggerMaster.TriggerEventArgs e)
        {
            if (!isInitialized) return;
            // Debug.Log("TMP A " + TimeWrapper.currentTimestampMS);
            // Debug.Log(AsyncThread.isMainThread_TS);

            AsyncThread.RunOnMainThread_ASAP_TS(() =>
            {
                // LOSES A FRAME
                // Debug.Log("TMP B " + TimeWrapper.currentTimestampMS);
                Send_NextFrame_NotTS(e);
            });
        }

        public void Send_NextFrame_NotTS(TriggerMaster.TriggerEventArgs e)
        {
            TimeWrapper.Timestamp timestamp = TimeWrapper.GetCurrentTimestamp_TS();

            if (!isInitialized) return;

            ExperimentManagerSession.LogData_AsTheyHappen_TS(timestamp, "TRIGGER_MANAGER_PHOTODIODE",
                "REQUEST_RECEIVED;{0};{1}"._Format(e.type, e.codePrio)); // [0] type, [1] code

            // Only log things if we are meant to
            if (!ShouldSend(e.type))
            {
                ExperimentManagerSession.LogData_AsTheyHappen_TS(timestamp, "TRIGGER_MANAGER_PHOTODIODE",
                    "REQUEST_DENIED;{0};{1}"._Format(e.type, e.codePrio)); // [0] type, [1] code

                if (config.debug)
                    Debug_Helper.LogWarning(typeof(TriggerManager_Photodiode), "Config either doesn't contain " + e.type + ", or its doPhotodiode is set to false. Or the MASTER_" + ExperimentManagerSession.module + "'s doPhotodiode is set to false. Aborting!");
                return;
            }

            int numBangs = 0;

            string code = Convert.ToString(e.codePrio, 2).PadLeft(8, '0');

            foreach (char c in code)
                if (c != '0')
                    numBangs++;

            // Ready to play!
            TrySendPhotoDiodeTrigger(numBangs, e.type);

            ExperimentManagerSession.LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(), "TRIGGER_MANAGER_PHOTODIODE",
                "REQUEST_PROCESSED;{0};{1}"._Format(e.type, e.codePrio)); // [0] type, [1] code
        }

        private bool TrySendPhotoDiodeTrigger(int numBangs, TriggerOutEvent triggerEvent)
        {
            TimeWrapper.Timestamp timestamp = TimeWrapper.GetCurrentTimestamp_TS();

            if (!PhotoDiodeDebugger.TryFire(numBangs, triggerEvent))
            {
                ExperimentManagerSession.LogData_AsTheyHappen_TS(timestamp, "TRIGGER_MANAGER_PHOTODIODE",
                    string.Format("TRIGGER_FAILED;{0};{1};{2};BUSY_WITH_LAST", triggerEvent, numBangs, lastTrigger)); // [0] Trigger Event, [1] num bangs, [2] last trigger

                return false;
            }

            lastTrigger = triggerEvent;

            return true;
        }

        [Serializable]
        public new class Config : TriggerManager.Config
        {
            public PhotoDiodeDebugger.Config photodiodeManager;
        }

        public new class RuntimeConfig : TriggerManager.RuntimeConfig
        {
            public RuntimeConfig(Func<TriggerOutEvent, bool> shouldFire) : base(shouldFire)
            {
            }
        }
    }
}