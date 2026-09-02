using System;
using System.Collections.Generic;
using UnityEngine;
using Peripherals.UserInput.Normal;

namespace Peripherals.UserInput.HighAccu
{
    [Serializable]
    public class HighAccuracyInput_Serial : HighAccuracyInput_Base
    {
        protected new const string name = nameBase + "SERIAL";
        protected override string GetName() { return name; }

        private NormalInput_Serial serialInputManager;

        private static readonly List<int> allKeys = new List<int>();

        public HighAccuracyInput_Serial(NormalInput_Serial serialInputManager)
        {
            // debug = false;

            this.serialInputManager = serialInputManager;

            if (serialInputManager != null)
            {
                serialInputManager.onKeyDown_TS += SerialPortManager_ResponseBoxInput_onKeyDown_TS;
                serialInputManager.onKeyUp_TS += SerialPortManager_ResponseBoxInput_onKeyUp_TS;
            }

            if (allKeys.Count == 0)
                foreach (KeyCode kC in TGP.Helpers.Utility_Helper.EnumGetValues<KeyCode>())
                    if (kC != KeyCode.None)
                        allKeys.Add((int)kC);
        }

        [SerializeField]
        private List<KeyCode> heldKeys = new List<KeyCode>();

        private void SerialPortManager_ResponseBoxInput_onKeyDown_TS(object sender, SerialKeyEventArgs e)
        {
            // Debug.Log("Down : " + e.keyboardEquivalent);
            KeyCode kC = e.keyboardEquivalent;
            if (heldKeys.Contains(kC)) return; // already down
            heldKeys.Add(kC);
        }

        private void SerialPortManager_ResponseBoxInput_onKeyUp_TS(object sender, SerialKeyEventArgs e)
        {
            // Debug.Log("Up : " + e.keyboardEquivalent);
            KeyCode kC = e.keyboardEquivalent;
            if (!heldKeys.Contains(kC)) return; // already released
            heldKeys.Remove(kC);
        }

        protected override List<int> GetAllKeys_TS()
        {
            return allKeys;
        }

        protected override void OnUpdate_TS()
        {
            // This will fire off the events we want
            serialInputManager?.Update_TS();
        }

        protected override bool IsKeyPushed_TS(int key)
        {
            KeyCode kC = GetUnityKeyCode_TS(key);

            return heldKeys.Contains(kC);
        }

        protected override KeyCode GetUnityKeyCode_TS(int key)
        {
            return (KeyCode)key;
        }
    }
}