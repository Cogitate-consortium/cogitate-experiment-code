using Peripherals.UserInput.Core;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Peripherals.UserInput.HighAccu
{
    [Serializable]
    public class HighAccuracyInput_USB : HighAccuracyInput_Base
    {
        protected new const string name = nameBase + "USB";
        protected override string GetName() { return name; }

        private static readonly List<int> allKeys = new List<int>();

        public HighAccuracyInput_USB()
        {
            debug = false;

            if (allKeys.Count == 0)
                for (int i = 0; i < 255; i++)
                    allKeys.Add(i);
        }

        protected override List<int> GetAllKeys_TS()
        {
            return allKeys;
        }

        protected override bool IsKeyPushed_TS(int key)
        {
            return HighAccuracyInput_USB_Helper.IsKeyPushedDown_TS(key);
        }

        /// <summary>
        /// Takes 1ms the first time you call it for a certain key, 0ms otherwise
        /// </summary>
        protected override KeyCode GetUnityKeyCode_TS(int key)
        {
            return HighAccuracyInput_USB_Helper.GetKeyCodeFromWindowsFormKey_TS(key);
        }
    }

    public static class HighAccuracyInput_USB_Helper
    {
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Int32 i);

        public static bool IsKeyPushedDown(WindowsFormKeys vKey)
        {
            return IsKeyPushedDown_TS((int)vKey);
        }

        public static bool IsKeyPushedDown_TS(int vKey)
        {
            return IsKeyPushedDown(GetAsyncKeyState(vKey));
        }

        private static bool IsKeyPushedDown(short keyState)
        {
            return 0 != (keyState & 0x8000);
        }

        private static readonly Dictionary<int, KeyCode> windowsToUnityKeys = new Dictionary<int, KeyCode>();

        /// <summary>
        /// Loses 4ms first time per key, 0ms after
        /// </summary>
        public static KeyCode GetKeyCodeFromWindowsFormKey_TS(int key)
        {
            if (windowsToUnityKeys.ContainsKey(key))
                return windowsToUnityKeys[key];

            WindowsFormKeys windowsFormKey = (WindowsFormKeys)key;

            string windowsKey_STR = windowsFormKey.ToString();

            KeyCode unityKeyCode = KeyCode.None;

            if (windowsFormKey == WindowsFormKeys.D0)
                unityKeyCode = KeyCode.Alpha0;
            else if (windowsFormKey == WindowsFormKeys.D1)
                unityKeyCode = KeyCode.Alpha1;
            else if (windowsFormKey == WindowsFormKeys.D2)
                unityKeyCode = KeyCode.Alpha2;
            else if (windowsFormKey == WindowsFormKeys.D3)
                unityKeyCode = KeyCode.Alpha3;
            else if (windowsFormKey == WindowsFormKeys.D4)
                unityKeyCode = KeyCode.Alpha4;
            else if (windowsFormKey == WindowsFormKeys.D5)
                unityKeyCode = KeyCode.Alpha5;
            else if (windowsFormKey == WindowsFormKeys.D6)
                unityKeyCode = KeyCode.Alpha6;
            else if (windowsFormKey == WindowsFormKeys.D7)
                unityKeyCode = KeyCode.Alpha7;
            else if (windowsFormKey == WindowsFormKeys.D8)
                unityKeyCode = KeyCode.Alpha8;
            else if (windowsFormKey == WindowsFormKeys.D9)
                unityKeyCode = KeyCode.Alpha9;
            else if (TGP.Helpers.Utility_Helper.EnumContains<KeyCode>(windowsKey_STR))
                unityKeyCode = TGP.Helpers.Utility_Helper.ToEnum<KeyCode>(windowsKey_STR);

            windowsToUnityKeys.Add(key, unityKeyCode);

            return unityKeyCode;
        }
    }
}