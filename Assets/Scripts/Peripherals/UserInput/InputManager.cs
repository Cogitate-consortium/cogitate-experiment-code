using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;
using Peripherals.UserInput.Normal;
using Peripherals.UserInput.HighAccu;
using Helpers.Engine;

namespace Peripherals.UserInput
{
    /// <summary>
    /// Make NOT singleton
    /// NS_SEGMENT USB Keyboard and Serial need to become two separate things
    /// </summary>
    public class InputManager : Singleton<InputManager>
    {
        public static EventHandler<KeyStatusArgs> onKeyStatusUpdate;
        public static EventHandler<ConnectionStatusArgs> onConnectionStatusUpdate;
        public static EventHandler<ActivityStatusArgs> onActivityStatusUpdate;

        // TODO Provide NonTS Alternative to these RENAME _TS to Thread and the other to Main
        // UI Should follow the Main for example not the Thread one
        /// <summary>
        /// [SOS] Keep Handlers to his thread safe | This may be coming from a thread
        /// </summary>
        public static event EventHandler<HighAccuracyEventArgs> onKeyUp_TS;
        /// <summary>
        /// [SOS] Keep Handlers to his thread safe | This may be coming from a thread
        /// </summary>
        public static event EventHandler<HighAccuracyEventArgs> onKeyDown_TS;
        /// <summary>
        /// [SOS] Keep Handlers to his thread safe | This may be coming from a thread
        /// </summary>
        public static event EventHandler<EventArgs<KeyCode>> onKey_TS;

        private static Config config;
        private static RuntimeConfig runtimeConfig;
        private bool FORCE_NORMAL_INPUT = false;

        private static bool allowExperimenterKeyCodes;

        private NormalInput_USB.Config usbConfig;

        /// <summary>
        /// USB KEY CONFIG
        /// </summary>
        //public static InputKeyCodeConfig keyCodeConfig { get { return ApplicationLibrary.Config.InputKeyCode; } }

        /// <summary>
        /// [DECOUPLE] Should not reference <see cref="NormalInput_Serial"/>
        /// </summary>
        private static List<KeyCode> keyCodes = null;

        private readonly Dictionary<string, List<KeyCode>> heldKeyCodes = new Dictionary<string, List<KeyCode>>();

        private static readonly bool debug = false;

        [SerializeField]
        private NormalInput_USB usbInput = null;

        [SerializeField]
        private NormalInput_Serial normalInput_Serial = null;

        [SerializeField]
        private HighAccuracyInput_USB highAccuracyInput_USB = null;
        [SerializeField]
        private HighAccuracyInput_Serial highAccuracyInput_Serial = null;

        ConnectionType lastConnectionType = ConnectionType.Unknown;
        private bool? lastUseResponseBox = null;

        // NS_DEBATABLE
        public static bool useSerial { get; private set; }
        // NS_DEBATABLE
        public static bool useResponseBox { get { return runtimeConfig.useResponseBox; } }

        private bool? isActive { get { return
            isFocused == null || isLocked == null ? null : 
            isFocused.Value && !isLocked.Value ? (bool?) true : false; } }

        private bool? isFocused = null;
        private bool? isLocked = null;

        private bool config_doLock;
        
        public static void ToggleForceNormalInput(bool on)
        {
            if (!instance) return;
            instance.FORCE_NORMAL_INPUT = on;
        }

        public static void Initialize_USB(Config config, RuntimeConfig runtimeConfig)
        {
            TryInitialize(config, runtimeConfig);

            instance.LogWarning("Input manager initialized (USB)");
        }

        public static void Initialize_Serial(Config config, RuntimeConfig runtimeConfig, List<SerialInputCheck> serialInputChecks)
        {
            if (!TryInitialize(config, runtimeConfig)) return;

            useSerial = 
                instance.TryEnableSerial(config.serialConfig, serialInputChecks);

            instance.LogWarning("Input manager initialized (SERIAL) " + (useSerial ? "SUCCESS" : "FAIL"));
        }

        private static bool TryInitialize(Config config, RuntimeConfig runtimeConfig)
        {
            if (!instance)
            {
                Debug.LogError("Could not initialize Input Manager instance!");
                return false;
            }

            InputManager.config = config;
            InputManager.runtimeConfig = runtimeConfig;

            instance.config_doLock = config.doLock;

            allowExperimenterKeyCodes = EngineWrapper.Debug_IsDebugBuild || config.allowExperimenterKeyCodesInRelease;

            instance.ResetSerial();
            instance.ConfigureUSB(config.usbConfig);

            instance.isLocked = false;
            instance.isFocused = true;

            return true;
        }

        private void ConfigureUSB(NormalInput_USB.Config usbConfig)
        {
            this.usbConfig = usbConfig;
        }

        private void ResetSerial()
        {
            normalInput_Serial?.DeInitialize();
            highAccuracyInput_Serial?.DeInitialize();

            normalInput_Serial = null;
            highAccuracyInput_Serial = null;
            useSerial = false;
        }

        public bool TryEnableSerial(NormalInput_Serial.Config inputSerialPortConfig, List<SerialInputCheck> serialInputChecks)
        {
            if (inputSerialPortConfig == null)
                return false;

            normalInput_Serial = new NormalInput_Serial();
            normalInput_Serial.Initialize(inputSerialPortConfig, serialInputChecks);
            normalInput_Serial.onKeyUp_TS += SerialPortInput_onKeyUp_TS;
            normalInput_Serial.onKeyDown_TS += SerialPortInput_onKeyDown_TS;

            highAccuracyInput_Serial = new HighAccuracyInput_Serial(normalInput_Serial);
            highAccuracyInput_Serial.onKeyDown_TS += HighAccuracyInput_Serial_onKeyDown_TS;
            highAccuracyInput_Serial.onKeyUp_TS += HighAccuracyInput_Serial_onKeyUp_TS;

            return true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            lastConnectionType = ConnectionType.Unknown;
            lastUseResponseBox = null;

            heldKeyCodes.Clear();
            heldKeyCodes.Add("KEYBOARD", new List<KeyCode>());
            // Serial will come based on ports

            // Create it
            highAccuracyInput_USB = new HighAccuracyInput_USB();
            highAccuracyInput_USB.onKeyDown_TS += HighAccuracyInput_USB_onKeyDown_TS;
            highAccuracyInput_USB.onKeyUp_TS += HighAccuracyInput_USB_onKeyUp_TS;
        }

        protected override void DeInitialize()
        {
            base.DeInitialize();

            if (highAccuracyInput_USB != null)
            {
                highAccuracyInput_USB.onKeyDown_TS -= HighAccuracyInput_USB_onKeyDown_TS;
                highAccuracyInput_USB.onKeyUp_TS -= HighAccuracyInput_USB_onKeyUp_TS;
            }


            if (highAccuracyInput_Serial != null)
            {
                highAccuracyInput_Serial.onKeyDown_TS -= HighAccuracyInput_Serial_onKeyDown_TS;
                highAccuracyInput_Serial.onKeyUp_TS -= HighAccuracyInput_Serial_onKeyUp_TS;
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            // we have initialized and want to do this
            if (isFocused == null || !config.preventInputWhenAltTabbed) return;

            isFocused = focus;

            onActivityStatusUpdate?.Invoke(null, new ActivityStatusArgs(isActive.Value,
                focus == true ? ActivityStatusArgs.Reason.APPLICATION_FOCUSED : 
                ActivityStatusArgs.Reason.APPLICATION_UNFOCUSED));
        }

        private void FireKeyHoldEvent(string sourceID, KeyCode keyCode, bool debug)
        {
            if (isActive != true)
            {
                UpdateKeyStatus_TS(TimeWrapper.GetCurrentTimestamp_TS(), "UN-FOCUSED HOLD", sourceID, keyCode, debug, -1);
                return;
            }

            if (!allowExperimenterKeyCodes && runtimeConfig.experimenterKeyCodes.Contains(keyCode))
            {
                UpdateKeyStatus_TS(TimeWrapper.GetCurrentTimestamp_TS(), "PREVENTED EXPERIMENT KEY HOLD", sourceID, keyCode, debug, -1);
                return;
            }

            onKey_TS?.Invoke(this, keyCode);

            UpdateKeyStatus_TS(TimeWrapper.GetCurrentTimestamp_TS(), "KEY_HOLD", sourceID, keyCode, debug, -1);
        }

        private void FireKeyDownEvent_TS(string sourceID, KeyCode keyCode, bool debug)
        {
            FireKeyDownEvent_TS(sourceID, keyCode, TimeWrapper.GetCurrentTimestamp_TS(), debug);
        }

        private void FireKeyDownEvent_TS(string sourceID, KeyCode keyCode, TimeWrapper.Timestamp keyStateTS, bool debug)
        {
            if (isActive != true)
            {
                UpdateKeyStatus_TS(TimeWrapper.GetCurrentTimestamp_TS(), "UN-FOCUSED DOWN", sourceID, keyCode, debug, -1);
                return;
            }

            if (!allowExperimenterKeyCodes && runtimeConfig.experimenterKeyCodes.Contains(keyCode))
            {
                UpdateKeyStatus_TS(TimeWrapper.GetCurrentTimestamp_TS(), "PREVENTED EXPERIMENT KEY DOWN", sourceID, keyCode, debug, -1);
                return;
            }

            TimeWrapper.Timestamp currentTS = TimeWrapper.GetCurrentTimestamp_TS();

            if (InputManager.debug)
                HighAccuracyInput_Base.Report("INPUT_MANAGER_2", currentTS.timestampMS, keyStateTS.timestampMS, keyCode, "DOWN");

            onKeyDown_TS?.Invoke(this, new HighAccuracyEventArgs(keyCode, keyStateTS));

            // Is it a known source?
            ConditionalLock_TS(heldKeyCodes, () =>
            {
                if (!heldKeyCodes.ContainsKey(sourceID))
                    heldKeyCodes.Add(sourceID, new List<KeyCode>());
            });

            // Is that code held?
            ConditionalLock_TS(heldKeyCodes[sourceID], () =>
            {
                if (!heldKeyCodes[sourceID].Contains(keyCode))
                    heldKeyCodes[sourceID].Add(keyCode);
            });

            UpdateKeyStatus_TS(currentTS, "KEY_PRESS", sourceID, keyCode, debug, keyStateTS.timestampMS);
        }

        private void FireKeyUpEvent_TS(string sourceID, KeyCode keyCode, bool debug)
        {
            FireKeyUpEvent_TS(sourceID, keyCode, TimeWrapper.GetCurrentTimestamp_TS(), debug);
        }

        private void FireKeyUpEvent_TS(string sourceID, KeyCode keyCode, TimeWrapper.Timestamp keyStateTS, bool debug)
        {
            if (isActive != true)
            {
                UpdateKeyStatus_TS(TimeWrapper.GetCurrentTimestamp_TS(), "UN-FOCUSED UP", sourceID, keyCode, debug, -1);
                return;
            }

            if (!allowExperimenterKeyCodes && runtimeConfig.experimenterKeyCodes.Contains(keyCode))
            {
                UpdateKeyStatus_TS(TimeWrapper.GetCurrentTimestamp_TS(), "PREVENTED EXPERIMENT KEY UP", sourceID, keyCode, debug, -1);
                return;
            }

            TimeWrapper.Timestamp timestamp = TimeWrapper.GetCurrentTimestamp_TS();

            if (InputManager.debug)
                HighAccuracyInput_Base.Report("INPUT_MANAGER_2", timestamp.timestampMS, keyStateTS.timestampMS, keyCode, "UP");
            // Debug.Log("IM B :: " + TimeWrapper.currentTimestampMS); // [200505] confirmed matches IM A (+0ms)
            onKeyUp_TS?.Invoke(this, new HighAccuracyEventArgs(keyCode, keyStateTS));

            // Is it a known source?
            ConditionalLock_TS(heldKeyCodes, () =>
            {
                if (!heldKeyCodes.ContainsKey(sourceID))
                    heldKeyCodes.Add(sourceID, new List<KeyCode>());
            });

            // Is that code held?
            ConditionalLock_TS(heldKeyCodes[sourceID], () =>
            {
                if (heldKeyCodes[sourceID].Contains(keyCode))
                    heldKeyCodes[sourceID].Remove(keyCode);
            });

            UpdateKeyStatus_TS(timestamp, "KEY_RELEASE", sourceID, keyCode, debug, keyStateTS.timestampMS);
        }

        /// <summary>
        /// [SOS] Run <see cref="Initialize_USB(Config, bool)"/> or <see cref="Initialize_Serial(Config, List{SerialInputCheck}, bool)"/> first
        /// </summary>
        public static void InitializeHighAccuInput()
        {
            instance?._InitializeHighAccuInput();
        }

        /// <summary>
        /// [SOS] Use in <see cref="Update"/> only
        /// </summary>
        public static bool GetKeyUp(KeyCode keyCode)
        {
            if (instance?.isActive != true) return false;

            return Input.GetKeyUp(keyCode);
        }

        /// <summary>
        /// [SOS] Use in <see cref="Update"/> only
        /// </summary>
        public static bool GetKey(KeyCode keyCode)
        {
            if (instance?.isActive != true) return false;

            return Input.GetKey(keyCode);
        }

        /// <summary>
        /// [SOS] Use in <see cref="Update"/> only
        /// </summary>
        public static bool GetKeyDown(KeyCode keyCode)
        {
            if (instance?.isActive != true) return false;

            return Input.GetKeyDown(keyCode);
        }

        private void _InitializeHighAccuInput()
        {
            ConnectionType currentInputType = GetCurrentConnectionType_NotTS();

            highAccuracyInput_USB?.TryInitialize(config.highAccuUSBConfig);
            highAccuracyInput_USB?.ToggleSleep(currentInputType != ConnectionType.HighAccuUSB);

            highAccuracyInput_Serial?.TryInitialize(config.highAccuSerialConfig);
            highAccuracyInput_Serial?.ToggleSleep(currentInputType != ConnectionType.HighAccuSerial);
        }

        public static void DeInitializeHighAccuInput()
        {
            instance?.highAccuracyInput_USB?.DeInitialize();
            instance?.highAccuracyInput_Serial?.DeInitialize();
        }

        private static void UpdateKeyStatus_TS(TimeWrapper.Timestamp timestamp, string state, string source, KeyCode keyCode, bool debug, double keyStateTS)
        {
            onKeyStatusUpdate?.Invoke(null, new KeyStatusArgs(timestamp, state, source, keyCode, debug, keyStateTS));
        }

        private void UpdateConnectionStatus_TS(ConnectionType currentConnectionType)
        {
            TimeWrapper.Timestamp timestamp = TimeWrapper.GetCurrentTimestamp_TS();

            string currentResponseBoxString = useResponseBox == true ? "Response Box" : useResponseBox == false ? "Keyboard" : "-";

            if (lastConnectionType != currentConnectionType || lastUseResponseBox != useResponseBox)
            {
                string lastResponseBoxString = lastUseResponseBox == true ? "Response Box" : lastUseResponseBox == false ? "Keyboard" : "-";

                onConnectionStatusUpdate?.Invoke(null, new ConnectionStatusArgs(timestamp,
                    lastConnectionType, lastResponseBoxString, currentConnectionType, currentResponseBoxString));

                lastConnectionType = currentConnectionType;
                lastUseResponseBox = useResponseBox;
            }
        }

        private void HighAccuracyInput_USB_onKeyDown_TS(object sender, HighAccuracyInput_Base.HighAccuracyEventArgs e)
        {
            HighAccuracyInput_onKeyDown_TS("HIGH_ACCURACY_USB", e);
        }

        private void HighAccuracyInput_Serial_onKeyDown_TS(object sender, HighAccuracyInput_Base.HighAccuracyEventArgs e)
        {
            HighAccuracyInput_onKeyDown_TS("HIGH_ACCURACY_SERIAL", e);
        }

        private void HighAccuracyInput_onKeyDown_TS(string sender, HighAccuracyInput_Base.HighAccuracyEventArgs e)
        {
            TimeWrapper.Timestamp keyStateTS = e.timestamp;
            KeyCode kC = e.key;
            if (debug)
                HighAccuracyInput_Base.Report("INPUT_MANAGER_0", TimeWrapper.currentTimestampMS, keyStateTS.timestampMS, kC, "DOWN");

            if (kC == KeyCode.None) return;

            FireKeyDownEvent_TS(sender, kC, keyStateTS, false);
        }

        private void HighAccuracyInput_USB_onKeyUp_TS(object sender, HighAccuracyInput_Base.HighAccuracyEventArgs e)
        {
            HighAccuracyInput_onKeyUp_TS("HIGH_ACCURACY_USB", e);
        }

        private void HighAccuracyInput_Serial_onKeyUp_TS(object sender, HighAccuracyInput_Base.HighAccuracyEventArgs e)
        {
            HighAccuracyInput_onKeyUp_TS("HIGH_ACCURACY_SERIAL", e);
        }

        // [SOS] Keep thread safe!!
        private void HighAccuracyInput_onKeyUp_TS(string sender, HighAccuracyInput_Base.HighAccuracyEventArgs e)
        {
            // Debug.Log("IM A :: " + TimeWrapper.currentTimestampMS);
            TimeWrapper.Timestamp keyStateTS = e.timestamp;
            KeyCode kC = e.key;
            if (debug)
                HighAccuracyInput_Base.Report("INPUT_MANAGER_0", TimeWrapper.currentTimestampMS, keyStateTS.timestampMS, kC, "UP");

            if (kC == KeyCode.None) return;

            FireKeyUpEvent_TS(sender, kC, keyStateTS, false);
        }

        private void SerialPortInput_onKeyDown_TS(object sender, SerialKeyEventArgs e)
        {
            // Key down and key up should share the same source ID
            FireKeyDownEvent_TS(e.sourceID, e.keyboardEquivalent, debug && e.debug);
        }

        private void SerialPortInput_onKeyUp_TS(object sender, SerialKeyEventArgs e)
        {
            // Key down and key up should share the same source ID
            FireKeyUpEvent_TS(e.sourceID, e.keyboardEquivalent, debug && e.debug);
        }

        public enum ConnectionType
        {
            Unknown = -1,
            NormalUSB = 0,
            NormalSerial = 1,
            HighAccuUSB = 2,
            HighAccuSerial = 3
        }

        /// <summary>
        /// Wrapper for <see cref="_GetCurrentConnectionType_NotTS"/>. Use this.
        /// </summary>
        public ConnectionType GetCurrentConnectionType_NotTS()
        {
            ConnectionType currentConnectionType = _GetCurrentConnectionType_NotTS();

            UpdateConnectionStatus_TS(currentConnectionType);

            return currentConnectionType;
        }

        void ConditionalLock_TS(object objToLock, Action actionToExecute)
        {
            if (config_doLock)
                lock (objToLock)
                    actionToExecute();
            else
                actionToExecute();
        }

        void Update()
        {
            if (isActive == null) return; // Not initalized yet!

            if (keyCodes == null)
                keyCodes = Utility_Helper.EnumGetValues<KeyCode>();

            ConnectionType currentConnectionType = GetCurrentConnectionType_NotTS();

            // If we are using serial port, make sure it's working
            if (currentConnectionType == ConnectionType.NormalSerial ||
                (currentConnectionType == ConnectionType.HighAccuSerial))// && ApplicationLibrary.Config.Input.EXPERIMENTER_ALLOW_SERIAL_FROM_KEYBOARD))
                normalInput_Serial.Update_NotTS();

            // Debug.Log(shouldTakeNormalInput);
            // 1. Remove
            if (currentConnectionType == ConnectionType.NormalUSB)// || Input.GetKeyUp(KeyCode.I))
                foreach (KeyCode kC in keyCodes)
                    // Anything released? 
                    if (Input.GetKeyUp(kC) ||
                        // or no longer being pressed?
                        (heldKeyCodes["KEYBOARD"].Contains(kC) && !Input.GetKey(kC)))
                        FireKeyUpEvent_TS("KEYBOARD", kC, usbConfig.debug);

            // 2. Anything still held ? 
            // [SOS] APPLIES TO NON-KEYBOARD INPUTS TOO
            ConditionalLock_TS(heldKeyCodes, () =>
            {
                foreach (KeyValuePair<string, List<KeyCode>> kVP in heldKeyCodes)
                    ConditionalLock_TS(kVP.Value, () =>
                    {
                        foreach (KeyCode kC in kVP.Value)
                            FireKeyHoldEvent(kVP.Key, kC, debug);
                    });
            });

            // 3. Add new
            if (currentConnectionType == ConnectionType.NormalUSB) //|| Input.GetKeyDown(KeyCode.I))
                                                                   // Anything new pressed?
                foreach (KeyCode kC in keyCodes)
                    if (Input.GetKeyDown(kC))
                        FireKeyDownEvent_TS("KEYBOARD", kC, usbConfig.debug);
        }

        /// <summary>
        /// [SOS] Only call from <see cref="GetCurrentConnectionType_NotTS"/>
        /// </summary>
        private ConnectionType _GetCurrentConnectionType_NotTS()
        {
#if UNITY_EDITOR
            // Set to default input to debug other systems
            // return InputType.NormalUSB;
#endif
            // Handle Normal
            if (FORCE_NORMAL_INPUT || config.preferredInputType == Config.Type.Normal)
                return useSerial ? ConnectionType.NormalSerial : ConnectionType.NormalUSB;

            // Ok, so we want High Accu USB and are in-game
            if (useSerial)
                return highAccuracyInput_Serial.CheckStatus() ? ConnectionType.HighAccuSerial : ConnectionType.NormalSerial;

            // We want 
            return highAccuracyInput_USB.CheckStatus() ? ConnectionType.HighAccuUSB : ConnectionType.NormalUSB;
        }

        [Serializable]
        public class Config
        {
            public bool debug = false;

            public enum Type { Normal = 0, HighAccuracy = 1}

            public string preferredInputType_Comment = "Normal = 0, HighAccuracy = 1";
            public Type preferredInputType = Type.HighAccuracy;

            public bool preventInputWhenAltTabbed = true;
            public bool allowExperimenterKeyCodesInRelease = false;

            public NormalInput_USB.Config usbConfig;
            public NormalInput_Serial.Config serialConfig;

            public HighAccuracyInput_Base.Config highAccuUSBConfig;
            public HighAccuracyInput_Base.Config highAccuSerialConfig;
            public string doLock_Comment = "May help prevent a rare input crash. May also cause timing issues (untested as of 18/09/20)";
            public bool doLock = true;
        }

        public class HighAccuracyEventArgs : EventArgs
        {
            public KeyCode key;
            public TimeWrapper.Timestamp timestamp;

            public HighAccuracyEventArgs(KeyCode key, TimeWrapper.Timestamp timestamp)
            {
                this.key = key;
                this.timestamp = timestamp;
            }
        }

        public class KeyStatusArgs : EventArgs
        {
            public TimeWrapper.Timestamp timestamp;
            public string state;
            public string source;
            public KeyCode keyCode;
            public bool debug;
            public double keyStateTS;

            public KeyStatusArgs(TimeWrapper.Timestamp timestamp, string state, string source, KeyCode keyCode, bool debug, double keyStateTS)
            {
                this.timestamp = timestamp;
                this.state = state;
                this.source = source;
                this.keyCode = keyCode;
                this.debug = debug;
                this.keyStateTS = keyStateTS;
            }
        }

        public class ConnectionStatusArgs : EventArgs
        {
            public TimeWrapper.Timestamp timestamp;

            public ConnectionType lastConnectionType;
            public string lastResponseBoxString;
            public ConnectionType currentConnectionType;
            public string currentResponseBoxString;

            public ConnectionStatusArgs(TimeWrapper.Timestamp timestamp, ConnectionType lastConnectionType, string lastResponseBoxString, ConnectionType currentConnectionType, string currentResponseBoxString)
            {
                this.timestamp = timestamp;
                this.lastConnectionType = lastConnectionType;
                this.lastResponseBoxString = lastResponseBoxString;
                this.currentConnectionType = currentConnectionType;
                this.currentResponseBoxString = currentResponseBoxString;
            }
        }

        public class ActivityStatusArgs
        {
            public enum Reason { APPLICATION_FOCUSED, APPLICATION_UNFOCUSED, MANUAL_LOCK, MANUAL_UNLOCK }
            public bool isActive;
            public Reason reason;

            public ActivityStatusArgs(bool isActive, Reason reason)
            {
                this.isActive = isActive;
                this.reason = reason;
            }
        }

        public class RuntimeConfig
        {
            public bool useResponseBox;
            public List<KeyCode> experimenterKeyCodes;

            public RuntimeConfig(bool useResponseBox, List<KeyCode> experimenterKeyCodes)
            {
                this.useResponseBox = useResponseBox;
                this.experimenterKeyCodes = experimenterKeyCodes;
            }
        }

        public static void ToggleLock(bool doLock)
        {
            // we have initialized and want to do this
            if (!instance) return;
            if (instance.isLocked == null) return;

            instance.isLocked = doLock;

            onActivityStatusUpdate?.Invoke(null, new ActivityStatusArgs(instance.isActive.Value,
                doLock == true ? ActivityStatusArgs.Reason.MANUAL_LOCK :
                ActivityStatusArgs.Reason.MANUAL_UNLOCK));
        }
    }
}