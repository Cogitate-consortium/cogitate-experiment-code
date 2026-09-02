using System;
using System.Collections.Generic;
using UnityEngine;
using Helpers.Async;
using Helpers.Engine;

namespace Peripherals.UserInput.HighAccu
{
    [Serializable]
    public class HighAccuracyInput_Base
    {
        public static EventHandler<KeyStatusUpdateArgs> onKeyStatusUpdate;
        public static EventHandler<StatusUpdateArgs> onStatusUpdate;

        /// <summary>
        /// [SOS] Keep Handlers to his thread safe
        /// </summary>
        public event EventHandler<HighAccuracyEventArgs> onKeyDown_TS;

        /// <summary>
        /// [SOS] Keep Handlers to his thread safe
        /// </summary>
        public event EventHandler<HighAccuracyEventArgs> onKeyUp_TS;

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

        public bool doBreak = false;

        protected bool debug;

        public enum Status { Unknown, NotInitialized, Asleep, Running, Stuck }

        public Status GetStatus()
        {
            double timeSinceLastPing = TimeWrapper.currentTimestampMS - lastAliveTimestampMS;
            double timeToPronounceDead_Running = refreshMS + deadMS;
            double timeToPronounceDead_Asleep = sleepMS + deadMS;

            if (!initialized)
                return Status.NotInitialized;

            if (!isAsleep && timeSinceLastPing >= timeToPronounceDead_Running)
                return Status.Stuck;
            else if (!isAsleep && timeSinceLastPing < timeToPronounceDead_Running)
                return Status.Running;
            else if (isAsleep && timeSinceLastPing >= timeToPronounceDead_Asleep)
                return Status.Stuck;
            else if (isAsleep && timeSinceLastPing < timeToPronounceDead_Asleep)
                return Status.Asleep;

            return Status.Unknown;
        }

        private int refreshMS = 5;
        private int sleepMS = 1000;
        private int deadMS = 3;
        public double lastAliveTimestampMS = -1;
        private bool isAsleep = false;
        private bool initialized = false;
        private int lastStartedThreadID;
        private object lastStartedThreadID_Lock = new object();

        public void TryInitialize(Config config)
        {
            debug = false;

            doBreak = false;

            // Let it update variables, but not restart unless needed
            refreshMS = config.refreshMS;
            sleepMS = config.sleepMS;
            deadMS = config.deadMS;

            if (initialized && GetStatus() != Status.Stuck)
            {
                if (EngineWrapper.Debug_IsDebugBuild)
                    Debug.LogWarning("Was Initialized and wasn't Stuck - so not re-initializing!");
                return;
            }

            if (initialized)
                onKeyStatusUpdate?.Invoke(null, new KeyStatusUpdateArgs(TimeWrapper.GetCurrentTimestamp_TS(), GetName(), KeyState.KEY_DOWN, TimeWrapper.GetCurrentTimestamp_TS(), KeyCode.CapsLock));

            List<int> allKeys = new List<int>(GetAllKeys_TS());

            AsyncThread.RequestRunOnNewThread(() =>
            {
                string currentName = "";

                lock (lastStartedThreadID_Lock)
                {
                    lastStartedThreadID = AsyncThread.currentThreadID;
                    currentName = GetName() + "_" + lastStartedThreadID;
                }


                List<int> heldKeys = new List<int>();

                while (true)
                {
                    // Sleeps up-top to avoid forgetting to call them
                    AsyncThread.Sleep(isAsleep ? sleepMS : refreshMS);

                    // Trigger for breaking the thread
                    lock (lastStartedThreadID_Lock)
                        if (doBreak || AsyncThread.currentThreadID != lastStartedThreadID) { doBreak = false; Debug.Log(TimeWrapper.time_NotTS); return; }

                    lastAliveTimestampMS = TimeWrapper.currentTimestampMS;

                    // Making sure there are no added delays!!
                    TimeWrapper.Timestamp currentTimestamp = TimeWrapper.GetCurrentTimestamp_TS();

                    if (isAsleep) continue;

                    // AsyncThread.RunOnMainThread(() => { Debug.Log("HIGH_ACCU_BASE" + lastAliveTimestampMS); });
                    // double ts0 = lastAliveTimestampMS;
                    // double dT0 = TimeWrapper.currentTimestampMS - ts0;

                    OnUpdate_TS();

                    // double dT1 = TimeWrapper.currentTimestampMS - ts0 - dT0;

                    // Occasional 1-5 ms spikes

                    // double dTI = dT1;
                    // Create it every frame!
                    // List<double> dTIs = new List<double>();

                    for (int index = 0; index < allKeys.Count; index++)
                        {
                        //  dTI = TimeWrapper.currentTimestampMS - ts0 - dTI;
                        //  dTIs.Add(dTI);

                        // if (TimeWrapper.currentTimestampMS != lastAliveTimestampMS)
                        int key = allKeys[index];

                        // Is that key pushed?
                        bool keyDown = IsKeyPushed_TS(key);

                            if (keyDown && !heldKeys.Contains(key))
                            {
                                KeyCode keyCode = GetUnityKeyCode_TS(key);
                                RaiseKeyEvent_TS(currentTimestamp, KeyState.KEY_DOWN, keyCode, currentName);
                                heldKeys.Add(key);
                            }
                            else if (!keyDown && heldKeys.Contains(key))
                            {
                                KeyCode keyCode = GetUnityKeyCode_TS(key);
                                RaiseKeyEvent_TS(currentTimestamp, KeyState.KEY_UP, keyCode, currentName);
                                heldKeys.Remove(key);
                            }
                        }

                    //  double dT2 = TimeWrapper.currentTimestampMS - ts0 - dTI;
                    /*
                      AsyncThread.RunOnMainThread(() =>
                      {
                          if (dT0 > 0.3f) Debug.LogError(ts0 + " HIGH ACCU INPUT BASE A " + dT0);
                          if (dT1 > 0.3f) Debug.LogError(ts0 + " HIGH ACCU INPUT BASE B " + dT1);
                          for (int i = 0; i < dTIs.Count; i++) if (dTIs[i] > 0.3f) Debug.LogError(ts0 + " HIGH ACCU INPUT BASE I" + i + " ->" + dTIs[i]);
                          if (dT2 > 0.3f) Debug.LogError(ts0 + " HIGH ACCU INPUT BASE C " + dT2);
                      });
                      */
                }
            });

            initialized = true;
        }

        protected virtual void OnUpdate_TS() { }

        protected virtual bool IsKeyPushed_TS(int key)
        {
            throw new NotImplementedException();
        }

        protected virtual KeyCode GetUnityKeyCode_TS(int key)
        {
            throw new NotImplementedException();
        }

        protected virtual List<int> GetAllKeys_TS()
        {
            throw new NotImplementedException();
        }

        public enum KeyState { KEY_DOWN, KEY_UP }

        protected void RaiseKeyEvent_TS(TimeWrapper.Timestamp stateChangeTimestamp, KeyState keyState, KeyCode keyCode, string name)
        {

#if UNITY_EDITOR
            // if (keyCode == KeyCode.I) { Debug.Log("IGNORING " + keyCode); return; }
#endif

            if (debug) Report(GetName(), TimeWrapper.currentTimestampMS, stateChangeTimestamp.timestampMS, keyCode, keyState);

            TimeWrapper.Timestamp timestamp = TimeWrapper.GetCurrentTimestamp_TS();

            (keyState == KeyState.KEY_DOWN ? onKeyDown_TS : onKeyUp_TS)?.
                Invoke(name, new HighAccuracyEventArgs(keyCode, stateChangeTimestamp));

            // Do use the override timestamp, since this is a high accuracy measurement
            onKeyStatusUpdate?.Invoke(null, new KeyStatusUpdateArgs(timestamp, name, keyState, stateChangeTimestamp, keyCode));
        }

        public class KeyStatusUpdateArgs : EventArgs
        {
            public TimeWrapper.Timestamp timestamp;
            public string name;
            public KeyState keyState;
            public TimeWrapper.Timestamp stateChangeTimestamp;
            public KeyCode keyCode;

            public KeyStatusUpdateArgs(TimeWrapper.Timestamp timestamp, string name, KeyState keyState, TimeWrapper.Timestamp stateChangeTimestamp, KeyCode keyCode)
            {
                this.timestamp = timestamp;
                this.name = name;
                this.keyState = keyState;
                this.stateChangeTimestamp = stateChangeTimestamp;
                this.keyCode = keyCode;
            }

            public override string ToString()
            {
                return string.Format("{0};{1};{2};{3};{4}", timestamp, name, keyState, stateChangeTimestamp, keyCode);
            }
        }

        public class StatusUpdateArgs : EventArgs
        {
            public TimeWrapper.Timestamp timestamp;
            public StatusChange statusChange;

            public StatusUpdateArgs(TimeWrapper.Timestamp timestamp, StatusChange statusChange)
            {
                this.timestamp = timestamp;
                this.statusChange = statusChange;
            }

            public override string ToString()
            {
                return string.Format("{0};{1}",
                    timestamp, statusChange);
            }
        }

        protected const string nameBase = "HIGH_ACCU_INPUT_";
        protected const string name = nameBase + "BASE";

        protected virtual string GetName() { return name; }

        public static void Report(object sender, double reportTS, double keyStateTS, object keyCode, object state)
        {
            // if (keyCode.ToString() != "E" && keyCode.ToString() != "e") return;
            AsyncThread.RunOnMainThread_ASAP_TS(() =>
            {
                Debug.Log(string.Format("{0}: Key {4} -> {5} (debug TS {1}, report TS {2}, keyState TS {3})",
sender, TimeWrapper.currentTimestampMS, reportTS, keyStateTS, keyCode, state));
            });
        }

        public void ToggleSleep(bool shouldPause)
        {
            isAsleep = shouldPause;
        }

        public void DeInitialize()
        {
            initialized = false;
            doBreak = true;
            isAsleep = true;
        }

        bool wasHighAccuThreadWorking = true;

        // Also fires a warning any time you call it and it notices a change from the last time you called it
        public bool CheckStatus()
        {
            bool isHighAccuThreadWorking = false;

            Status status = GetStatus();
            isHighAccuThreadWorking = status != Status.Stuck;

            TimeWrapper.Timestamp timestamp = TimeWrapper.GetCurrentTimestamp_TS();

            if (!isHighAccuThreadWorking && wasHighAccuThreadWorking)
            {
                if (Debug.isDebugBuild) Debug.LogWarning(timestamp.timestampMS + " : " + GetName() + " High Accu Thread stopped working! Will be restarted next level (maybe earlier).");
                onStatusUpdate?.Invoke(null, new StatusUpdateArgs(timestamp, StatusChange.HIGH_ACCU_INPUT_BROKE));
                wasHighAccuThreadWorking = false;
            }
            else if (isHighAccuThreadWorking && !wasHighAccuThreadWorking)
            {
                if (Debug.isDebugBuild) Debug.Log(timestamp.timestampMS + " : " + GetName() + " High Accu Thread has started working again.");
                onStatusUpdate?.Invoke(null, new StatusUpdateArgs(timestamp, StatusChange.HIGH_ACCU_INPUT_FIXED));
                wasHighAccuThreadWorking = true;
            }

            return isHighAccuThreadWorking;
        }

        public enum StatusChange { HIGH_ACCU_INPUT_BROKE, HIGH_ACCU_INPUT_FIXED }

        [Serializable]
        public class Config
        {
            public int refreshMS = 5;
            public int sleepMS = 1000;
            public int deadMS = 3;
        }
    }
}