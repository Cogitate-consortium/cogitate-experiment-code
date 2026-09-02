// NS_REMOVE
using ExperimentLibrary;

using Experiment.Managers;
using Helpers.Engine;
using Peripherals.UserInput;
using System;
using UnityEngine;
using Peripherals.UserInput.Core; // Just for Extended Keycode
using System.Collections.Generic;

namespace Game.Systems.Bridges
{
    public class KeyboardTrigger : ITrigger
    {
        Config config { get { return ExperimentLibraryManager.Config.TriggerInKeyCode; } }
        public event EventHandler<EventArgs> onTRReceived_TS;
        private static List<KeyCode> config_SendTrigger_KeyCodes;

        public void Initialize()
        {
            config_SendTrigger_KeyCodes = config.sendTrigger.keyCodes;
            InputManager.onKeyUp_TS += InputManager_onKeyUp_TS;
        }

        private void InputManager_onKeyUp_TS(object sender, InputManager.HighAccuracyEventArgs e)
        {
            KeyCode key = e.key;

            if (!config_SendTrigger_KeyCodes.Contains(key)) return;

            ExperimentManagerSession.LogData_AsTheyHappen_TS(
                TimeWrapper.GetCurrentTimestamp_TS(),
                "TRIGGER_MANAGER_FMRI", "TRIGGER_RECEIVED;TRCode");

            onTRReceived_TS?.Invoke(this, EventArgs.Empty);
        }

        public void DeInitialize()
        {
            InputManager.onKeyUp_TS -= InputManager_onKeyUp_TS;
        }

        [Serializable]
        public class Config
        {
            public bool debug;
            public ExtendedKeyCode sendTrigger = (KeyCode)116;
        }
    }
}