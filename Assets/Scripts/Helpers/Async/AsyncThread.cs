// NS_REMOVE | LOG, CONFIG
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;
using Helpers.Engine;
using Experiment.Managers;

namespace Helpers.Async
{
    // TODO Isolate MB vs Wrapper
    /// <summary>
    /// Use <see cref="Initialize"/> !
    /// </summary>
    public class AsyncThread : MonoBehaviour
    {
        private struct DelayedQueueItem
        {
            public bool IsTime(double TimestampMS, int Frame)
            {
                return TimestampMS >= this.TimestampMS || Frame >= this.Frame;
            }

            public double TimestampMS;
            public int Frame;
            public Action Action;

            public DelayedQueueItem(double time, Action action) : this()
            {
                TimestampMS = time;
                Frame = int.MaxValue;
                Action = action;
            }

            public DelayedQueueItem(int frame, Action action) : this()
            {
                TimestampMS = double.MaxValue;
                Frame = frame;
                Action = action;
            }

            public bool IsTime(TimeWrapper.Timestamp timestamp)
            {
                return IsTime(timestamp.timestampMS, timestamp.frameCycleID);
            }
        }

        #region Static

        // Do this when you start your application
        private static int mainThreadID;
        public static int currentThreadID { get { return Thread.CurrentThread.ManagedThreadId; } }

        private static bool doDebug = false;

        // [SOS] Call this or they won't know!
        private static void OnMainThreadInitialized()
        {
            // In Main method:
            mainThreadID = currentThreadID;

            if (doDebug)
                TestThreadInit();
        }

        // If called in the non main thread, will return false;
        public static bool isMainThread_TS
        {
            get { return currentThreadID == mainThreadID; }
        }

        /// <summary>
        /// Maximum number of active threads.
        /// </summary>
        public static int maxThreads = 100;// { get; private set; }

        public static void RunOnMainThread_NextFrameCycle_TS(Action action)
        {
            RunOnMainThread_AfterFrameCycles_TS(action, 1);
        }

        public static void RunOnMainThread_AfterFrameCycles_TS(Action action, int delayFrames)
        {
            int currentFrameCycle = TimeWrapper.GetCurrentTimestamp_TS().frameCycleID;
            lock (Instance._delayed)
            {
                Instance._delayed.Add(new DelayedQueueItem(currentFrameCycle + delayFrames, action));
            }
        }

        public static void RunOnMainThread_Delayed_TS(Action action, double delayMS)
        {
            double currentTime = TimeWrapper.GetCurrentTimestamp_TS().timestampMS;
            lock (Instance._delayed)
            {
                Instance._delayed.Add(new DelayedQueueItem(currentTime + delayMS, action));
            }
        }

        // [TS]
        /// <summary>
        /// Handle data in MainThread with delay.
        /// </summary>
        public static void RunOnMainThread_ASAP_TS(Action action)
        {
#if UNITY_EDITOR
            // return;
#endif
            // Are we on the main thread?
            // [200505] Confirmed that this shaves off an unnecessarily lost frame
            if (isMainThread_TS)
            {
                // Just Do It (tm)
                action?.Invoke();
            }
            else
            {
                lock (Instance._actions)
                {
                    Instance._actions.Add(action);
                }
            }
        }

        public static void Sleep(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }

        /// <summary>
        /// [SOS] Executes code in separate THREAD iff we are on the main one
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static bool RequestRunOnNewThread_IfOnMain(Action a, float retryDelayMS = 0)
        {
            // If on main
            if (isMainThread_TS)
                // Request new thread
                return RequestRunOnNewThread(a, retryDelayMS);

            // Else just execute now
            a?.Invoke();
            return true;
        }

        public static bool RequestRunOnNewThread_OnNextFrame(Action a, float retryDelayMS = 0)
        {
            return RequestRunOnNewThread(() =>
            {
                int currentFrameCycle = TimeWrapper.currentFrameCycleID;

            // Run this on the next frame
            while (TimeWrapper.currentFrameCycleID == currentFrameCycle)
                    Thread.Sleep(1);

                a?.Invoke();
            }, retryDelayMS);
        }

        /// <summary>
        /// Exercute code in a separate Thread.
        /// [SOS] We have no way of TERMINATING threads until they finish themselves, so no multi-starting infinite loop threds like <see cref="HighAccuracyInput_Base.Initialize(bool)"/>
        /// [SOS] Errors will fail silently (ie <see cref="string.Format(string, object)"/> with one more {0} than it should have.
        /// [SOS] Using things you shouldn't (ie. <see cref="TempTime.time"/>) will fail silently.
        /// </summary>
        public static bool RequestRunOnNewThread(Action a, float retryDelayMS = 0)
        {
#if UNITY_EDITOR
            // return;
#endif

            Initialize();

            if (numThreads >= maxThreads)
            {
                Action delayedRetry = () =>
                {
                    // Debug.Log("Processing requested action");
                    RequestRunOnNewThread(a);
                };

                if (retryDelayMS > 0)
                    RunOnMainThread_Delayed_TS(delayedRetry, retryDelayMS);
                else
                    RunOnMainThread_NextFrameCycle_TS(delayedRetry);

                return false;
            }

            // if (Debug.isDebugBuild) Debug.Log("Opening new Thread :" + numThreads);

            /*
            // Wait for Threads to complete
            while (numThreads >= maxThreads)
            {
                Thread.Sleep(1); // This is still running ON THE MAIN THREAD
            }
            */

            Interlocked.Increment(ref numThreads);
            ThreadPool.QueueUserWorkItem(RunAction, a);

            return true;
        }

        private static AsyncThread Instance
        {
            get
            {
                Initialize();
                return _instance;
            }
        }

        /// <summary>
        /// Active number of threads.
        /// </summary>
        private static int numThreads;

        /// <summary>
        /// Is Instance Initialized?
        /// </summary>
        private static bool initialized;

        /// <summary>
        /// Static instance.
        /// </summary>
        private static AsyncThread _instance;

        /// <summary>
        /// Initialize instance.
        /// </summary>
        private static void Initialize()
        {
            if (!initialized)
            {
                if (!Application.isPlaying)
                    return;
                initialized = true;
                _instance = new GameObject("AsyncThread").AddComponent<AsyncThread>();
                DontDestroyOnLoad(_instance);
            }
        }

        /// <summary>
        /// Run code in separate Thread.
        /// </summary>
        /// <param name="action"></param>
        private static void RunAction(object action)
        {
            try
            {
                ((Action)action)();
            }
            catch
            { }
            finally
            {
                Interlocked.Decrement(ref numThreads);
                if (Debug.isDebugBuild) Debug.Log("Closing Thread :" + numThreads);
            }
        }

        #endregion

        private List<Action> _actions = new List<Action>();
        private List<Action> _currentActions = new List<Action>();
        private List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();
        private List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();

        // Initialize Instance
        void Awake()
        {
            _instance = this;
            initialized = true;
            OnMainThreadInitialized();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                initialized = false;
                _instance = null;
            }

            StopAllCoroutines();

            lock (_actions)
                _actions.Clear();
            lock (_currentActions)
                _currentActions.Clear();
            lock (_delayed)
                _delayed.Clear();
            lock (_currentDelayed)
                _currentDelayed.Clear();
        }

        // Update is called once per frame
        void LateUpdate()
        {
            TryProcessActions(ProcessingEvent.LateUpdate);
        }

        private void Update()
        {
            TryProcessActions(ProcessingEvent.Update);
            ExperimentManagerSession.LogData_AsTheyHappen_TS(
                TimeWrapper.GetCurrentTimestamp_TS(),
                "ASYNC_THREAD", string.Format("NUM_THREADS;{0};{1}", numThreads, maxThreads));
        }

        private void FixedUpdate()
        {
            TryProcessActions(ProcessingEvent.FixedUpdate);
        }

        private void OnGUI()
        {
            TryProcessActions(ProcessingEvent.OnGUI);
        }

        private enum ProcessingEvent { LateUpdate, Update, FixedUpdate, OnGUI }

        private void TryProcessActions(ProcessingEvent processingEvent)
        {
            bool debugASAP = false;
            bool debugDELAYED = false;

            int numTotal = 0;
            int numProcessed = 0;
            TimeWrapper.Timestamp timestamp = TimeWrapper.GetCurrentTimestamp_TS();

            lock (_actions)
            {
                _currentActions.Clear();
                _currentActions.AddRange(_actions);
                numTotal = _actions.Count;
                numProcessed = _currentActions.Count;
                _actions.Clear();
            }

#if UNITY_EDITOR
            if (doDebug && debugASAP && numProcessed > 0)
                Debug.Log(string.Format("[SOS, Delays Added] PROCESSING {0} OF {1} ASAP ITEMS ON CYCLE {2} (CALLED FROM {3} on {4} ms)",
                    numProcessed, numTotal, timestamp.frameCycleID, processingEvent, timestamp.timestampMS));
#endif

            foreach (var a in _currentActions)
            {
                // [HACK] Ready-made strings for all events / current combos
                /*
                SubjectPerformanceReport.LogLevelData_AsTheyHappen_TS(
                    TimeWrapper.GetCurrentTimestamp_TS(), "ASYNC_THREAD",
                    processingEvent == ProcessingEvent.LateUpdate ? "PROCESSED_INSTANT_ACTION;LATE_UPDATE" :
                    processingEvent == ProcessingEvent.Update ? "PROCESSED_INSTANT_ACTION;UPDATE" :
                    processingEvent == ProcessingEvent.FixedUpdate ? "PROCESSED_INSTANT_ACTION;FIXED_UPDATE" :
                    processingEvent == ProcessingEvent.OnGUI ? "PROCESSED_INSTANT_ACTION;ON_GUI" : "PROCESSED_INSTANT_ACTION;UNKNOWN", doDebug);
                */
                a();
            }

            lock (_delayed)
            {
                _currentDelayed.Clear();
                _currentDelayed.AddRange(_delayed.Where(d => d.IsTime(timestamp)));
                numTotal = _delayed.Count;
                numProcessed = _currentDelayed.Count;
                foreach (var item in _currentDelayed)
                    _delayed.Remove(item);
            }

#if UNITY_EDITOR
            if (doDebug && debugDELAYED && numProcessed > 0)
                Debug.Log(string.Format("[SOS, Delays Added] PROCESSING {0} OF {1} DELAYED ITEMS ON CYCLE {2} (CALLED FROM {3} on {4} ms)",
                    numProcessed, numTotal, timestamp.frameCycleID, processingEvent, timestamp.timestampMS));
#endif

            foreach (var delayed in _currentDelayed)
            {
                // [HACK] Ready-made strings for all events / delayed combos
                /*
                SubjectPerformanceReport.LogLevelData_AsTheyHappen_TS(
                    TimeWrapper.GetCurrentTimestamp_TS(), "ASYNC_THREAD",
                    processingEvent == ProcessingEvent.LateUpdate ? "PROCESSED_DELAYED_ACTION;LATE_UPDATE" :
                    processingEvent == ProcessingEvent.Update ? "PROCESSED_DELAYED_ACTION;UPDATE" :
                    processingEvent == ProcessingEvent.FixedUpdate ? "PROCESSED_DELAYED_ACTION;FIXED_UPDATE" :
                    processingEvent == ProcessingEvent.OnGUI ? "PROCESSED_DELAYED_ACTION;ON_GUI" : "PROCESSED_INSTANT_ACTION;UNKNOWN", doDebug);
                */
                delayed.Action();
            }
        }

        void OnDisable()
        {
            if (_instance == this)
                _instance = null;
        }

        private static void TestThreadInit()
        {
            Debug.Log("=== Async Thread Test ===");

            // Base
            Debug.Log("[A] " + TimeWrapper.currentTimestampMS + " Running on thread ID " + currentThreadID + " isMain = " + isMainThread_TS);

            // Should run instantly!
            RunOnMainThread_ASAP_TS(() =>
            {
            // [A] Running on thread ID 1 isMain = True
            Debug.Log("[A2] " + TimeWrapper.currentTimestampMS + " Running on thread ID " + currentThreadID + " isMain = " + isMainThread_TS);
            });

            RequestRunOnNewThread(() =>
            {
            // [B] Running on thread ID X isMain = False
            Debug.Log("[B] " + TimeWrapper.currentTimestampMS + " Running on thread ID " + currentThreadID + " isMain = " + isMainThread_TS);

                RunOnMainThread_ASAP_TS(() =>
                {
                // [C] Running on thread ID 1 isMain = True
                Debug.Log("[C] " + TimeWrapper.currentTimestampMS + " Running on thread ID " + currentThreadID + " isMain = " + isMainThread_TS);

                // Should run instantly!
                RunOnMainThread_ASAP_TS(() =>
                    {
                    // [A] Running on thread ID 1 isMain = True
                    Debug.Log("[C2] " + TimeWrapper.currentTimestampMS + " Running on thread ID " + currentThreadID + " isMain = " + isMainThread_TS);
                    });

                    RequestRunOnNewThread(() =>
                    {
                        Thread.Sleep(1000);
                    // [D] Running on thread ID Y isMain = False
                    Debug.Log("[D] " + TimeWrapper.currentTimestampMS + " Running on thread ID " + currentThreadID + " isMain = " + isMainThread_TS);
                    });

                    RequestRunOnNewThread(() =>
                    {
                        Thread.Sleep(2000);
                    // [E] Running on thread ID Z isMain = False
                    Debug.Log("[E] " + TimeWrapper.currentTimestampMS + " Running on thread ID " + currentThreadID + " isMain = " + isMainThread_TS);
                    });
                });
            });
        }
    }
}