using System;
using System.Collections.Generic;
using System.Threading;
using TGP.Helpers;
using UnityEngine;
using Peripherals.LPT.Core;
using Helpers.Async;
using Helpers.Engine;

namespace Peripherals.LPT
{
    // SEGMENT : Part of this should become LPTTriggerManager
    // SHOULD NOT BE STATIC
    public class LPTManager : MonoBehaviour
    {
        public static EventHandler<StatusUpdateArgs> onStatusUpdate;
        public static LPTManager instance;

        private static LPTAccess lptPort;
        private static bool isPortInitialized = false;

        // Prepare it as if it started an infinity ago
        private static short latestCode_Data = 0;
        private static int latestCode_Prio = int.MinValue;
        private static double latestCode_TimestampSentMS = double.MinValue;

        private static int singleFrameMS = 1;
        private static bool clearPortAfterCode = true;
        private static int clearPortAfterCode_Prio = 0;

        private static int clearPortAfterCode_DelayMS = 1;
        private static short maxShortPerSend = 255;
        private static Config config;
        // Useless to resend a missing trigger, timing will be off
        private static int config_CheckEveryMS = 1;
        private static int config_ResendSafetyMS = 1;
        private static int config_InterruptClearForMS = 1;
        private static bool config_DoInterruptClear = true;
        private static bool config_RetrySendingWhenBusy = true;
        private static bool config_InterruptWhenHigherPrio = true;
        private static int config_InterruptResponseMS = 1;

        private static Interrupt interruptHandle = null;
        private static object interruptHandleLock = new object();

        #region Mono Behaviour

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
                return;
            }
            instance = this;
            DontDestroyOnLoad(this);
        }

        #endregion

        #region Public Methods
        private static bool isInitialized = false;

        public static void Initialize(Config config)
        {
            LPTManager.config = config;

            config_lptDebug = LPTManager.config.debug;
            minDelayBetweenCodesMS = config.minDelayBetweenCodesMS;
            clearPortAfterCode = config.clearPortAfterCode;
            clearPortAfterCode_Prio = config.clearPortAfterCode_Prio;
            clearPortAfterCode_DelayMS = config.clearPortAfterCode_DelayMS;
            maxShortPerSend = config.maxShortPerSend;
            config_RetrySendingWhenBusy = config.retrySendingWhenBusy;
            config_InterruptWhenHigherPrio = config.interruptWhenHigherPrio;
            config_InterruptResponseMS = config.interruptResponseMS;
            config_InterruptClearForMS = config.interruptClearForMS;
            config_DoInterruptClear = config.doInterruptClear;

            singleFrameMS = (int)((1f / Screen.currentResolution.refreshRate) * 1000f);
            config_CheckEveryMS = config.checkEveryMS;
            config_ResendSafetyMS = config.resendSafetyMS;
            // Start the processing thread
            AsyncThread.RequestRunOnNewThread(RequestHandler_Thread);

            // TriggerHelper.onTrigger_Thread += TriggerHelper_onTrigger_TS;
            // TriggerHelper.onTrigger_Main += TriggerHelper_onTrigger_NotTS;
            isInitialized = true;
            try
            {
                lptPort = new LPTAccess(config.accessPort);
                isPortInitialized = true;
            }
            catch (Exception ex)
            {

#if UNITY_EDITOR_OSX
#elif UNITY_STANDALONE_OSXs
#elif PLATFORM_STANDALONE_OSX
#else
                if (LPTManager.config.debug)
                    if (Debug.isDebugBuild)
                        Debug.LogError("LPTManager Init Exception :: " + ex.Message);
#endif
            }

            GameObject go = new GameObject("LPTManager");
            go.AddComponent<LPTManager>();
        }

        private static readonly List<Request> pendingRequests = new List<Request>();

        private static void RequestHandler_Thread()
        {
            Report_AsItHappens_TS(null, null, null, "REQUEST_HANDLER_INFO", "STARTED");

            lock (pendingRequests)
                pendingRequests.Clear();

            Request req = null;

            int requestID = 0;

            while (true)
            {
                req = null;

                lock (pendingRequests)
                    if (pendingRequests.Count > 0)
                    {
                        req = pendingRequests[0];
                        pendingRequests.RemoveAt(0);
                    }

                if (req == null)
                    Thread.Sleep(config_CheckEveryMS);
                else
                {
                    if (req == Request.KILL)
                    {
                        Report_AsItHappens_TS(null, null, null, "REQUEST_HANDLER_INFO", "STOPPED");

                        return;
                    }

                    if (req.type == RequestType.NextFrame)
                    {
                        int currentFrame = TimeWrapper.currentFrameCycleID;
                        while (TimeWrapper.currentFrameCycleID == currentFrame)
                            Thread.Sleep(1);
                    }

                    SendData_Thread(req.triggerEventString, requestID++, req.data, req.prio, req.isAppendixToPreviousMessage);
                }
            }
        }

        private static void RequestSendData(params Request[] requests)
        {
            lock (pendingRequests)
            {
                foreach (Request req in requests)
                    pendingRequests.Add(req);
            }
        }

        private static void StopRequestHandler()
        {
            lock (pendingRequests)
            {
                pendingRequests.Clear();
                pendingRequests.Add(Request.KILL);
            }
        }

        private static bool config_lptDebug = false;

        public static void Send_TS(object triggerEventString, int data, int prio, RequestType requestType)
        {
            TimeWrapper.Timestamp timestamp = TimeWrapper.GetCurrentTimestamp_TS();

            if (!isInitialized) return;

            // Report_AsItHappens_TS(triggerEventString, data, "REQUEST_RECEIVED", "RCV_FROM_MASTER");

            // So, it's time to send ; can we send in one go?
            int maxAllowedCode = (maxShortPerSend + 1) * (maxShortPerSend + 1) - 1;

            if (data > maxAllowedCode)
            {
#if UNITY_EDITOR
                Debug.LogError("Requested code :: {0}, max Code allowed in current implementation :: {1}"._Format(data, maxAllowedCode));
#endif
                return;
            }
            // handle longer codes
            else if (data > maxShortPerSend)
            {
                short codeMSB = 0;
                short codeLSB = 0;

                Math_Helper.BreakIntoShorter(data, maxShortPerSend, out codeMSB, out codeLSB);

                // Debug.LogWarning("Broke {0} into {1} and {2} "._Format(data, codeMSB, codeLSB));

                double lastSendTS = TimeWrapper.currentTimestampMS;
                string eType_Appended = string.Format("{0}_APPENDIX", triggerEventString);

                // Create the request
                RequestSendData(
                    new Request(triggerEventString, codeMSB, prio, false, requestType),
                    new Request(eType_Appended, codeLSB, prio, true, requestType));
            }
            // Normal sized codes
            else
            {
                RequestSendData(
                    new Request(triggerEventString, (short)data, prio, false, requestType));
            }
        }

        public static void Deinitialize()
        {
            StopRequestHandler();
            isInitialized = false;
            // TriggerHelper.onTrigger_Thread -= TriggerHelper_onTrigger_TS;
            // TriggerHelper.onTrigger_Main -= TriggerHelper_onTrigger_TS;
        }

        private static int minDelayBetweenCodesMS = 1;

        /// TODO Turn <see cref="LogType"/> into enum
        private static void Report_AsItHappens_TS(object triggerEvent, object data, object prio, string logType, object info, bool debug = false)
        {
            onStatusUpdate?.Invoke(null, new StatusUpdateArgs(triggerEvent, data, prio, logType, info, debug));
        }
                
        private static void SendData_Thread(object triggerEventString, int requestID, short data, int prio, bool isAppendixToPreviousMessage = false)
        {
            Report_AsItHappens_TS(triggerEventString, data, prio, "PROCESSING_REQUEST", "TRY_SEND_DATA");

            // Debug.Log("LPT B " + TimeWrapper.currentTimestampMS);
            bool debug = false;

            // string timestamp = TimeWrapper.currentTimestampMS.ToString();

            // If we're clearing port after code, we need to factor in the time it takes to clear the port after sending
            double lastCode_TimestampEndMS = latestCode_TimestampSentMS + (clearPortAfterCode ? clearPortAfterCode_DelayMS : 0);
            double lastCode_TimeSinceEndMS = TimeWrapper.currentTimestampMS - lastCode_TimestampEndMS;

            // We got a CONFLICT - tried to send a new code while an old one was being processed
            if (lastCode_TimeSinceEndMS < minDelayBetweenCodesMS) //  - singleFrameMS (no need, we're on a thread)
            {
                // Are we higher prio than the old code?
                if (prio > latestCode_Prio && config_InterruptWhenHigherPrio)
                {
                    // INTERRUPT
                    lock (interruptHandleLock)
                        interruptHandle = new Interrupt(requestID, prio);

                    Report_AsItHappens_TS(triggerEventString, data, prio, "TRIGGER_INFO", "INTERRUPTING_CURRENT", debug);

                    // Send Data to port
                    if (config_DoInterruptClear)
                    {
                        if (isPortInitialized)
                            lptPort.Write(0);
                        else
                            Thread.Sleep(1);

                        if (config_InterruptClearForMS > 0)
                            Thread.Sleep(config_ResendSafetyMS);

                        Report_AsItHappens_TS(triggerEventString, 0, clearPortAfterCode_Prio, "TRIGGER_INFO", "INTERRUPT_PORT_CLEARED", debug);
                    }
                }

                // Do we requeue?
                // After stim ALWAYS send appendix
                else if (config_RetrySendingWhenBusy || isAppendixToPreviousMessage)
                {
                    Report_AsItHappens_TS(triggerEventString, data, prio, "TRIGGER_INFO", "MIN_DELAY_RETRYING", debug);

                    int waitDelay = Mathf.CeilToInt((float)(minDelayBetweenCodesMS - lastCode_TimeSinceEndMS));

                    Thread.Sleep(waitDelay + config_ResendSafetyMS);

                    SendData_Thread(triggerEventString, requestID, data, prio, false); // We no longer know if it will be appendix next time it retries!

                    return;
                }

                // ABORT
                else
                {
                    Report_AsItHappens_TS(triggerEventString, data, prio, "TRIGGER_FAILED", "MIN_DELAY_ABORTED", debug);
                    return;
                }

            }

            // [HACK] If it's an APPENDIX, slow it down by 1ms to not bump into the game stim on

            /*
            if (clearPortAfterCode && clearPortCR != null)
            {
                if (data == lastCode)
                {
                    if (debug) Debug.LogError("Trying to resend same code before clearing port. Resulting in losing an event. code:" + data);
                    SubjectPerformanceReport.LogLevelData_TS("LPTManager", "Trying to resend same code before clearing port. Resulting in losing an event. code:" + data);
                }

                // No need to clear port, because codes are different
                instance.StopCoroutine(clearPortCR);
            }
            */

            latestCode_TimestampSentMS = TimeWrapper.currentTimestampMS;

            try
            {
                Report_AsItHappens_TS(triggerEventString, data, prio, "TRIGGER_INFO", "DATA_PRE_WRITE", debug);

                latestCode_Data = data;
                latestCode_Prio = prio;

                if (isPortInitialized)
                {
                    lptPort.Write(data);
                    Report_AsItHappens_TS(triggerEventString, data, prio, "TRIGGER_SENT", "SUCCESS", debug);
                }
                else
                {
                    Thread.Sleep(1);
                    Report_AsItHappens_TS(triggerEventString, data, prio, "TRIGGER_FAILED", "NOT_INITIALIZED", debug);
                }

                Report_AsItHappens_TS(triggerEventString, data, prio, "TRIGGER_INFO", "DATA_POST_WRITE", debug);

                // It's good to clear values, in case of a sequence of same code event
                if (clearPortAfterCode)
                {
                    for (int i = 0; i < clearPortAfterCode_DelayMS; i += config_InterruptResponseMS)
                    {
                        lock (interruptHandleLock)
                            if (interruptHandle != null && interruptHandle.requestID != requestID)
                            {
                                Report_AsItHappens_TS(triggerEventString, data, prio, "TRIGGER_INTERRUPTED", interruptHandle, debug);
                                interruptHandle = null; // "Consume" the interrupt
                                return;
                            }

                        Thread.Sleep(config_InterruptResponseMS);
                    }

                    Report_AsItHappens_TS(triggerEventString, 0, clearPortAfterCode_Prio, "TRIGGER_INFO", "CLEAR_PRE_WRITE", debug);

                    // Send Data to port
                    if (isPortInitialized)
                        lptPort.Write(0);
                    else
                        Thread.Sleep(1);

                    Report_AsItHappens_TS(triggerEventString, 0, clearPortAfterCode_Prio, "TRIGGER_INFO", "CLEAR_POST_WRITE", debug);
                }
            }
            catch (Exception ex)
            {
                Report_AsItHappens_TS(triggerEventString, 0, prio, "TRIGGER_FAILED", ex.Message, debug);
                if (Debug.isDebugBuild) Debug.LogError(ex.Message);
            }

            lock (interruptHandleLock)
                if (interruptHandle != null && interruptHandle.requestID == requestID)
                    interruptHandle = null; // "Clear" the interrupt
        }

        #endregion

        #region Private Methods

        private static int GetStimID(string name)
        {
            // Don't parse blank stumulus
            if (name == "NULL")
                return -1;

            int stimID = 0;
            try
            {
                stimID = int.Parse(name);
            }
            catch
            {
                if (Debug.isDebugBuild)
                    Debug.LogError("Couldn't match stimulus id to integer! StimulusName:" + name);
                return -1;
            }

            // Logical errors
            if (stimID < 1 || stimID > 20)
            {
                if (Debug.isDebugBuild)
                    Debug.LogError("Stimulus ID/Name should be between 1 and 20, but found:" + stimID);
                return -1;
            }
            return stimID;
        }

        #endregion

        private class Request
        {
            public object triggerEventString;
            public short data;
            public int prio;
            public bool isAppendixToPreviousMessage;
            public RequestType type;

            public static readonly Request KILL = new Request("KILL", -1, int.MaxValue, false, RequestType.Instant);

            public Request(object triggerEventString, short data, int prio, bool isAppendixToPreviousMessage, RequestType type)
            {
                this.triggerEventString = triggerEventString;
                this.data = data;
                this.prio = prio;
                this.isAppendixToPreviousMessage = isAppendixToPreviousMessage;
                this.type = type;
            }
        }

        [Serializable]
        public class Config
        {
            public bool debug = false;

            public short accessPort = 888;
            public int minDelayBetweenCodesMS = 50;
            public bool clearPortAfterCode = true;
            public string clearPortAfterCode_DelayMS_Comment = "This is how long the Trigger stays up for in the LPT";
            public int clearPortAfterCode_DelayMS = 25;
            public string maxBitsPerSend_Comment = "Will segment codes that are above that into two consecutive sends (max value = 15)";
            public int maxBitsPerSend = 8;
            public int checkEveryMS = 1;
            public string resendSafetyMS_Comment = "Advised to be 1+ to avoid rounding errors";
            public int resendSafetyMS = 1;
            public int clearPortAfterCode_Prio = 0;
            public bool retrySendingWhenBusy = true;
            public bool interruptWhenHigherPrio = true;
            public int interruptResponseMS = 1;
            public int interruptClearForMS = 1;
            public bool doInterruptClear = true;

            public short maxShortPerSend { get { return (short)(Mathf.Pow(2, maxBitsPerSend) - 1); } }
        }

        public class Interrupt
        {
            public int requestID;
            public object reason;

            public Interrupt(int requestID, object reason)
            {
                this.requestID = requestID;
                this.reason = reason;
            }

            public override string ToString()
            {
                return "Request ID {0} (Reason :: {1})"._Format(requestID, reason);
            }
        }
        /// TODO Cleanup should be referencing <see cref="Request"/> fields maybe?>
        public class StatusUpdateArgs : EventArgs
        {
            public object triggerEvent;
            public object data;
            public object prio;
            public object logType;
            public object info;
            public bool debug;

            public StatusUpdateArgs(object triggerEvent, object data, object prio, object logType, object info, bool debug)
            {
                this.triggerEvent = triggerEvent;
                this.data = data;
                this.prio = prio;
                this.logType = logType;
                this.info = info;
                this.debug = debug;
            }
        }
    }
}