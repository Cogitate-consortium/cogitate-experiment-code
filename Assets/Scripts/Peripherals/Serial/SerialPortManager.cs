// NS_SEGMENT | TEST
using Peripherals.UserInput;
// NS_SEGMENT | TEST
using ExperimentLibrary.Test;

using System;
using System.IO.Ports;
using UnityEngine;
using Helpers.Async;
using Helpers.Engine;
using TGP.Helpers;
using System.Collections.Generic;

namespace Peripherals.Serial
{
    /// <summary>
    /// [SEGMENT] Put the wrapper into Core, defer the Tests to the Wrapper
    /// </summary>
    [Serializable]
    public class SerialPortManager
    {
        public static EventHandler<DebugInfo> onDebugInfo;

        protected Config config { get; private set; }
        protected SerialPort serialPort;

        private static readonly List<KeyCode> keyCodes_TS = new List<KeyCode>();

        public string sourceID { get; private set; }
        protected string sourceString;

        private string config_portName;

        /// <summary>
        /// [SOS] Does not guarantee it can actually send things but it will try |
        /// <see cref="isPortOpen"/> on the other hand means we have a connection
        /// </summary>
        public bool isInitialized { get; private set; }
        public bool isPortOpen { get { return serialPort?.IsOpen == true; } }

        public string portName { get { return serialPort?.PortName; } }

        private void RaiseDebugInfo(string message)
        {
            onDebugInfo?.Invoke(this, new DebugInfo(sourceID, message));
        }

        public virtual void Initialize(Config config)
        {
            // debug = false;
            this.config = config;

            lock (keyCodes_TS)
            {
                keyCodes_TS.Clear();
                keyCodes_TS.AddRange(Utility_Helper.EnumGetValues<KeyCode>());
            }

            config_portName = config.portName;
            sourceID = "SERIAL_PORT_" + config_portName;

            string portInfo = "{0} ({1} baud rate, {2} read buffer size)"._Format(
                config.portName, config.baudRate, config.readBufferSize);

            RaiseDebugInfo(string.Format("Attempting to Open port {0}\n", portInfo));

            isInitialized = true;

            try
            {
                serialPort = new SerialPort(config.portName, config.baudRate, Parity.None, 8, StopBits.One);
                serialPort.ReadBufferSize = config.readBufferSize;
                serialPort.Disposed += SerialPort_Disposed;
                serialPort.Open();

                RaiseDebugInfo(string.Format("Opened port {0}\n", portInfo));
            }
            catch (Exception ex)
            {

#if UNITY_EDITOR_OSX
#elif UNITY_STANDALONE_OSXs
#elif PLATFORM_STANDALONE_OSX
#else
                if (config.debug)
                    Debug.LogError(ex.Message);
                RaiseDebugInfo(string.Format("Failed opening port {0}, serial error {1}\n", portInfo, ex.Message));
                return;
#endif
            }
        }

        public void DeInitialize()
        {
            if (isInitialized && serialPort != null)
            {
                serialPort.Close();
                serialPort.Dispose();
            }

            isInitialized = false;
        }

        private void SerialPort_Disposed(object sender, EventArgs e)
        {
            RaiseDebugInfo(string.Format("Serial DISPOSED"));
        }

        private bool processKeyboardInputAsSerial = false;

        public void Update_NotTS()
        {
            if (!isInitialized) return;

            if (config.EXPERIMENTER_ALLOW_SERIAL_FROM_KEYBOARD)
            {
                if (InputManager.GetKeyUp(config.EXPERIMENTER_SERIAL_FROM_KEYBOARD_KEY))
                {
                    processKeyboardInputAsSerial = !processKeyboardInputAsSerial;
                    if (Debug.isDebugBuild) Debug.Log(config_portName + "Toggled Keyboard as Serial -> " + (processKeyboardInputAsSerial ? "ON" : "OFF"));
                }
            }
            else
                processKeyboardInputAsSerial = false;

            if (EditorOnlyUtilities.serialPort_ProcessKeyboardInput == true || processKeyboardInputAsSerial)
            {
                string fakeData = "";
                foreach (KeyCode kC in keyCodes_TS)
                {
                    if (InputManager.GetKeyDown(kC))
                    {
                        string fakeDataIn = InputManager.GetKey(KeyCode.LeftShift) ? kC.ToString().ToUpper() : kC.ToString().ToLower();
                        if (fakeData.Length > 1) continue; // ignore Mouse0 etc
                                                           // Debug.LogWarning(fakeDataIn);
                        fakeData += fakeDataIn;
                    }
                }

                if (fakeData != "")
                {
                    // Debug.LogError(fakeData);
                    if (fakeData.Length > 1)
                        Debug.LogWarning(config_portName + "[EDITOR_ONLY] didn't send fake serial port data : " + fakeData);
                    else
                    {
                        Debug.Log(config_portName +  " [EDITOR_ONLY] Sent fake serial port data : " + fakeData);
                        OnDataReceived_TS(fakeData);
                    }
                    return;
                }
            }

            Update_TS();
        }

        public string UI_TriggerToSend = "zcz";
        public bool UI_SendTrigger;

        public void Update_TS()
        {
            if (UI_SendTrigger)
            {
                // Debug.Log("Sending trigger:: " + UI_TriggerToSend);
                OnDataReceived_TS(UI_TriggerToSend);
                UI_TriggerToSend = "";
                UI_SendTrigger = false;
                return;
            }

            if (!isInitialized) return;
            if (!isPortOpen) return;

            string data = serialPort.ReadExisting();

            if (data.Length == 0) return;

            OnDataReceived_TS(data);
        }

        private void OnDataReceived_TS(string data)
        {
            double timestampMS = TimeWrapper.currentTimestampMS;

            onDataReceived?.Invoke(this, data);

            AsyncThread.RunOnMainThread_ASAP_TS(() =>
            {
                RaiseDebugInfo(string.Format("{0};{1};{2}\n", timestampMS, config_portName, data));
            });
        }

        public EventHandler<string> onDataReceived;

        [System.Serializable]
        public class Config
        {
            public bool debug = false;
            public string portName = "COM3";
            public int baudRate = 115200;
            public int readBufferSize = 4096;
            public bool EXPERIMENTER_ALLOW_SERIAL_FROM_KEYBOARD = true;
            public KeyCode EXPERIMENTER_SERIAL_FROM_KEYBOARD_KEY = KeyCode.F8;
        }
    }
}