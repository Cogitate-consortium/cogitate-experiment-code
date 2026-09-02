using System;
using System.Collections.Generic;
using TGP.Helpers;
using Peripherals.Serial;
using UnityEngine;
using Peripherals.UserInput.Core;

namespace Peripherals.UserInput.Normal
{
    /// <summary>
    ///  DECOUPLE DATA !! Data Gets fed into it from (A) Experiment (B) Game (C) UI
    /// </summary>
    [Serializable]
    public class NormalInput_Serial
    {
        private SerialPortManager serialPortManager;

        public event EventHandler<SerialKeyEventArgs> onKeyDown_TS;
        public event EventHandler<SerialKeyEventArgs> onKeyUp_TS;
        // public event EventHandler<SerialKeyEventArgs> onKey;

        // private SeattleInputKeyCodeConfig keyConfig { get { return ApplicationLibrary.Config.InputKeyCode; } }

        List<SerialInputCheck> serialInputChecks = new List<SerialInputCheck>();

        private bool doDebug_TS = false;

        public void Initialize(Config config, List<SerialInputCheck> serialInputChecks)
        {
            serialPortManager = new SerialPortManager();
            serialPortManager.Initialize(config.serialPortConfig);

            serialPortManager.onDataReceived += SPM_onDataReceived;

            doDebug_TS = config.debug;

            this.serialInputChecks = new List<SerialInputCheck>(serialInputChecks);

            this.LogWarning("Initialized");
        }

        [Serializable]
        public class Config
        {
            public SerialPortManager.Config serialPortConfig;
            public bool debug;
        }

        private List<SerialInputCheck> keyDowns = new List<SerialInputCheck>();
        private List<SerialInputCheck> keyUps = new List<SerialInputCheck>();
        
        private void SPM_onDataReceived(object sender, string e)
        {
            string data = e;

            // double dT1 = TimeWrapper.currentTimestampMS - ts0 - dT0;

            keyDowns.Clear();
            keyUps.Clear();

            //double dT2 = TimeWrapper.currentTimestampMS - ts0 - dT1;

            foreach (SerialInputCheck sIC in serialInputChecks)
            {
                // Filter out empty serial codes (unassigned)
                if (sIC.serialCode.IsNullOrEmpty() ||
                    // And anything not contained in the data stream
                    !data.Contains(sIC.serialCode)) continue;

                if (sIC.serialCodeOn)
                {
                    bool shouldAdd = keyDowns.Find(a => a.keyCode == sIC.keyCode) == null;
                    if (shouldAdd)
                    {
                        keyDowns.Add(sIC);
                        // Debug.Log(sIC.serialCode + " : " + sIC.keyCode);
                    }
                    continue;
                }
                else
                {
                    bool shouldAdd = keyUps.Find(a => a.keyCode == sIC.keyCode) == null;
                    if (shouldAdd)
                    {
                        keyUps.Add(sIC);
                        // Debug.Log(sIC.serialCode + " : " + sIC.keyCode);
                    }

                    continue;
                }
            }

            //double dT3 = TimeWrapper.currentTimestampMS - ts0 - dT2;

            // 0-1 MS WHEN NEW DOWN
            foreach (SerialInputCheck sIC in keyDowns)
                onKeyDown_TS?.Invoke(this, new SerialKeyEventArgs(serialPortManager.sourceID, serialPortManager.portName, sIC.serialCode, sIC.keyCode.keyCodes.GetFirst(), doDebug_TS));

            // double dT4 = TimeWrapper.currentTimestampMS - ts0 - dT3;

            // 0-1 MS WHEN NEW UP
            foreach (SerialInputCheck sIC in keyUps)
                onKeyUp_TS?.Invoke(this, new SerialKeyEventArgs(serialPortManager.sourceID, serialPortManager.portName, sIC.serialCode, sIC.keyCode.keyCodes.GetFirst(), doDebug_TS));

            // double dT5 = TimeWrapper.currentTimestampMS - ts0 - dT4;

            /*
            AsyncThread.RunOnMainThread(() =>
            {
                if (dT0 > 0.3f) Debug.LogError(ts0 + " SERIAL PORT MANAGER RBI A " + dT0);
                if (dT1 > 0.3f) Debug.LogError(ts0 + " SERIAL PORT MANAGER RBI B " + dT1);
                if (dT2 > 0.3f) Debug.LogError(ts0 + " SERIAL PORT MANAGER RBI C " + dT2);
                if (dT3 > 0.3f) Debug.LogError(ts0 + " SERIAL PORT MANAGER RBI D " + dT3);
                if (dT4 > 0.3f) Debug.LogError(ts0 + " SERIAL PORT MANAGER RBI E " + dT4);
                if (dT5 > 0.3f) Debug.LogError(ts0 + " SERIAL PORT MANAGER RBI F " + dT5);
            });
            */
        }

        public void DeInitialize()
        {
            serialPortManager?.DeInitialize();
        }

        public void Update_NotTS()
        {
            serialPortManager?.Update_NotTS();
        }

        public void Update_TS()
        {
            serialPortManager?.Update_TS();
        }
    }

    public class SerialInputCheck
    {
        public string serialCode;
        public bool serialCodeOn;
        public ExtendedKeyCode keyCode;

        public SerialInputCheck(string serialCode, bool serialCodeOn, ExtendedKeyCode keyCode)
        {
            this.serialCode = serialCode;
            this.serialCodeOn = serialCodeOn;
            this.keyCode = keyCode;
        }
    }

    [System.Serializable]
    public class SerialConfigKey
    {
        public string label;
        public string lowToHigh;
        public string highToLow;

        public SerialConfigKey() { }

        public SerialConfigKey(string label, string lowToHigh, string highToLow)
        {
            this.label = label;
            this.lowToHigh = lowToHigh;
            this.highToLow = highToLow;
        }
    }

    public class SerialKeyEventArgs : EventArgs
    {
        // DOnt want to have to create this while in a Thread!
        public string sourceID;

        public string port;
        public string serialKey;
        public KeyCode keyboardEquivalent;

        public bool debug;

        public SerialKeyEventArgs(string sourceID, string port, string serialKey, KeyCode keyboardEquivalent, bool debug)
        {
            this.sourceID = sourceID;
            this.port = port;
            this.serialKey = serialKey;
            this.keyboardEquivalent = keyboardEquivalent;
            this.debug = debug;
        }
    }
}