using System;
using Peripherals.Serial;
using Helpers.Engine;
using TGP.Helpers;
using Experiment.Managers;

namespace Game.Systems.Bridges
{
    [Serializable]
    public class SerialPortTrigger : ITrigger
    {
        private SerialPortManager serialPortManager;
        private Config config;

        public event EventHandler<EventArgs> onTRReceived_TS;

        private string _config_LowToHigh = "-1";

        public void Initialize(Config config)
        {
            serialPortManager = new SerialPortManager();
            serialPortManager.Initialize(config.serialPortConfig);
            serialPortManager.onDataReceived += SPM_onDataReceived;
            this.config = config;
            _config_LowToHigh = this.config.lowToHigh;
        }

        private void SPM_onDataReceived(object sender, string e)
        {
            string data = e;
            if (!data.Contains(_config_LowToHigh)) return;

            onTRReceived_TS?.Invoke(this, EventArgs.Empty);

            ExperimentManagerSession.LogData_AsTheyHappen_TS(TimeWrapper.GetCurrentTimestamp_TS(),
                "TRIGGER_MANAGER_FMRI", string.Format("{0};{1};{2}", "TRIGGER_RECEIVED", "TRCode", data));

            this.LogWarning("Initialized");
        }

        [Serializable]
        public class Config
        {
            public string lowToHigh = "a";

            public SerialPortManager.Config serialPortConfig;
        }

        public void Update_NotTS()
        {
            serialPortManager?.Update_NotTS();
        }

        public void DeInitialize()
        {
            serialPortManager?.DeInitialize();
        }
    }
}