//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using Helpers.Engine;
using System;
using System.Threading;
using TGP.Helpers;
using UnityEngine;

namespace Tobii.Research.Unity
{
    public class EyeTracker : EyeTrackerBase
    {
        #region 200723 Konstantinos Vasileiadis
        public bool TryConnect()
        {
            var eyeTrackers = EyeTrackingOperations.FindAllEyeTrackers();

            foreach (var eyeTrackerEntry in eyeTrackers)
            {
                if (_connectToFirst || eyeTrackerEntry.SerialNumber.StartsWith(_eyeTrackerSerialStart))
                {
                    FoundEyeTracker = eyeTrackerEntry;
                    _eyeTracker = FoundEyeTracker;
                    FoundEyeTracker = null;
                    UpdateSubscriptions();
                    // StopAutoConnectThread();
                    // AutoConnectThreadRunning = false;
                    Debug.Log("Connected to Eye Tracker: " + _eyeTracker.SerialNumber);
                    return true;
                }
            }

            return false;
        }

        public void Initialize(Config config)
        {
            this.config = config;
        }

        private Config config;

        [Serializable]
        public class Config
        {
            public enum SyncMode { Latest = 0, Best = 1 }
            [SerializeField] private string syncMode_Comment = "Latest = 0, Best = 1 | Latest keeps the latest sync value, while Best compares jitter and drift differences to dynamically pick the best sync value. Default is Best.";
            public SyncMode syncMode = SyncMode.Best;
            public double driftPercentile = 0.00001f;
        }

        public static double currentSystemTimestampMS_Backup { get { return EyeTrackingOperations.GetSystemTimeStamp() / shift; } }
        public double currentSystemTimestampMS { get { return currentTimeSync != null ? currentTimeSync.Value.GetCurrentSystemTS(TimeWrapper.currentTimestampMS) : -1; } }
        public double currentDeviceTimestampMS { get { return currentTimeSync != null ? currentTimeSync.Value.GetCurrentDeviceTS(TimeWrapper.currentTimestampMS) : -1; } }

        public TimeSync? currentTimeSync;
        public TimeSync newTimeSyncTemp;

        public static event EventHandler<EventArgs<TimeSync>> onTimeSyncRefRcv;

        const double shift = 1000;

        private void _eyeTracker_TimeSynchronizationReferenceReceived(object sender, TimeSynchronizationReferenceEventArgs e)
        {
            newTimeSyncTemp = new TimeSync(currentTimeSync, e,
                TimeWrapper.currentTimestampMS,
                EyeTrackingOperations.GetSystemTimeStamp() / shift,
                e.SystemRequestTimeStamp / shift,
                e.SystemResponseTimeStamp / shift,
                e.DeviceTimeStamp / shift,
                config.driftPercentile);

            bool shouldUseNew;

            if (currentTimeSync == null)
                shouldUseNew = true;
            else if (config.syncMode == Config.SyncMode.Latest)
                shouldUseNew = true;
            else
                shouldUseNew = newTimeSyncTemp.jitterDiffFromCurrent < newTimeSyncTemp.driftDiffFromCurrent;

            if (shouldUseNew)
            {
                newTimeSyncTemp.SetIsUsed(true); // do this first so it gets copied!
                currentTimeSync = newTimeSyncTemp;
            }
            else
                newTimeSyncTemp.SetIsUsed(false);

            onTimeSyncRefRcv?.Invoke(this, newTimeSyncTemp);
            if (EngineWrapper.Debug_IsDebugBuild) Debug.LogError(newTimeSyncTemp);
        }

        public struct TimeSync
        {
            public double GetCurrentSystemTS(double currentUnityTS)
            {
                double dT_Unity_Now_RefReceived = currentUnityTS - unityTS_RefReceived;

                return systemTS_RefReceived + dT_Unity_Now_RefReceived;
            }

            public double GetCurrentDeviceTS(double currentUnityTS)
            {
                double systemTS_Now = GetCurrentSystemTS(currentUnityTS);
                double systemTS_Ack = systemTS_Sent + (systemTS_Received - systemTS_Sent) / 2;
                double dT_System_Now_Ack = systemTS_Now - systemTS_Ack;

                return deviceTS_Ack + dT_System_Now_Ack;
            }

            public bool isEmpty;

            public TimeSynchronizationReferenceEventArgs timeSyncArgs { set; private get; }

            private double unityTS_RefReceived;
            private double systemTS_RefReceived;

            private double systemTS_Sent;
            private double systemTS_Received;
            private double deviceTS_Ack;

            public double jitterDiffFromCurrent { get; private set; }
            public double driftDiffFromCurrent { get; private set; }
            private bool isUsed;

            public TimeSync(TimeSync? lastTimeSync, TimeSynchronizationReferenceEventArgs timeSyncArgs, double unityTS_RefReceived, double systemTS_RefReceived, double systemTS_Sent, double systemTS_Received, double deviceTS_Ack, double driftPercentile) : this()
            {
                this.timeSyncArgs = timeSyncArgs;
                this.unityTS_RefReceived = unityTS_RefReceived;
                this.systemTS_RefReceived = systemTS_RefReceived;
                this.systemTS_Sent = systemTS_Sent;
                this.systemTS_Received = systemTS_Received;
                this.deviceTS_Ack = deviceTS_Ack;

                if (lastTimeSync != null)
                {
                    jitterDiffFromCurrent = GetJitter() - lastTimeSync.Value.GetJitter();
                    driftDiffFromCurrent = (systemTS_RefReceived - lastTimeSync.Value.systemTS_RefReceived) * driftPercentile;
                }
                else
                {
                    jitterDiffFromCurrent = double.MinValue;
                    driftDiffFromCurrent = double.MinValue;
                }
            }

            private double GetJitter()
            {
                return systemTS_Received - systemTS_Sent;
            }

            public void SetIsUsed(bool isUsed)
            {
                this.isUsed = isUsed;
            }

            // CHECK 1 -> unityTS_RefReceived vs systemTS_RefReceived -> CONSTANT
            // CHECK 2 -> deviceTS_Ack vs systemTS_Ack -> CONSTANT

            public static string GetHeader()
            {
                return string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14}",
                    "timeSyncArgs_SystemRequestTimeStamp", "timeSyncArgs_SystemResponseTimeStamp", "timeSyncArgs_DeviceTimeStamp",
                    "unityTS_RefReceived", "systemTS_RefReceived", "systemTS_Sent", "systemTS_Received", "deviceTS_Ack",
                    "currentTS", "currentSystemTS", "currentDeviceTS", "currentSystemTS_Backup",
                    "jitterDiffFromCurrent", "driftDiffFromCurrent", "isUsed");
            }

            public override string ToString()
            {
                double currentTS = TimeWrapper.currentTimestampMS;
                double currentSystemTS_Backup = currentSystemTimestampMS_Backup;
                
                return string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14}",
                    timeSyncArgs.SystemRequestTimeStamp, timeSyncArgs.SystemResponseTimeStamp, timeSyncArgs.DeviceTimeStamp,
                    unityTS_RefReceived, systemTS_RefReceived, systemTS_Sent, systemTS_Received, deviceTS_Ack,
                    currentTS, GetCurrentSystemTS(currentTS), GetCurrentDeviceTS(currentTS), currentSystemTS_Backup,
                    jitterDiffFromCurrent, driftDiffFromCurrent, isUsed);
            }
        }

        private static string TimeSyncReferenceEventArgsToString(TimeSynchronizationReferenceEventArgs e)
        {
            return "{0};{1};{2}"._Format(e.SystemRequestTimeStamp, e.SystemResponseTimeStamp, e.DeviceTimeStamp);
        }

        private void GazeDataReceivedCallback(object sender, GazeDataEventArgs eventArgs)
        {
            _originalGazeData.Next = eventArgs;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Get <see cref="EyeTracker"/> instance. This is assigned
        /// in Awake(), so call earliest in Start().
        /// </summary>
        public static EyeTracker Instance { get; private set; }

        /// <summary>
        /// Get the latest gaze data. If there are new arrivals,
        /// they will be processed before returning.
        /// </summary>
        public IGazeData LatestGazeData
        {
            get
            {
                if (UnprocessedGazeDataCount > 0)
                {
                    // We have more data.
                    ProcessGazeEvents();
                }

                return _latestGazeData;
            }
        }

        /// <summary>
        /// Get the latest processed processed gaze data.
        /// Don't care if there a newer one has arrived.
        /// </summary>
        public IGazeData LatestProcessedGazeData { get { return _latestGazeData; } }

        /// <summary>
        /// Pop and get the next gaze data object from the queue.
        /// </summary>
        public IGazeData NextData
        {
            get
            {
                if (_gazeDataQueue.Count < 1)
                {
                    return default(IGazeData);
                }

                return _gazeDataQueue.Next;
            }
        }

        public override bool SubscribeToGazeData
        {
            get
            {
                return _subscribeToGaze;
            }

            set
            {
                _subscribeToGaze = value;
                base.SubscribeToGazeData = value;
            }
        }

        public override int GazeDataCount { get { return _gazeDataQueue.Count; } }

        public override int UnprocessedGazeDataCount { get { return _originalGazeData.Count; } }

        #endregion Public Properties

        #region Inspector Properties

        [SerializeField]
        [Tooltip("Connect to the first found eye tracker. Otherwise use provided serial number.")]
        private bool _connectToFirst;

        [SerializeField]
        [Tooltip("Check for this specific eyetracker serial number. Matches start of string so a partial start of a serial number can be used.")]
        private string _eyeTrackerSerialStart = "IS";

        #endregion Inspector Properties

        #region Private Fields

        /// <summary>
        /// Locked access and size management.
        /// </summary>
        private LockedQueue<GazeDataEventArgs> _originalGazeData = new LockedQueue<GazeDataEventArgs>(maxCount: _maxGazeDataQueueSize);

        /// <summary>
        /// Size managed queue.
        /// </summary>
        private SizedQueue<IGazeData> _gazeDataQueue = new SizedQueue<IGazeData>(maxCount: _maxGazeDataQueueSize);

        /// <summary>
        /// Hold the latest processed gaze data. Initialized to an invalid object.
        /// </summary>
        private IGazeData _latestGazeData = new GazeData();

        #endregion Private Fields

        #region Unity Methods

        public void Setup()
        {
            Instance = this;
            // DO IT HERE
            base.OnAwake();
        }

        // DO NOTHING HERE
        protected override void OnAwake() { }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        #endregion Unity Methods

        #region Private Eye Tracking Methods

        protected override void ProcessGazeEvents()
        {
            const int maxIterations = 20;

            var gazeData = _latestGazeData;

            for (int i = 0; i < maxIterations; i++)
            {
                var originalGaze = _originalGazeData.Next;

                // Queue empty
                if (originalGaze == null)
                {
                    break;
                }

                gazeData = new GazeData(originalGaze);
                _gazeDataQueue.Next = gazeData;
            }

            var queueCount = UnprocessedGazeDataCount;
            if (queueCount > 0)
            {
                Debug.LogWarning("We didn't manage to empty the queue: " + queueCount + " items left...");
            }

            _latestGazeData = gazeData;
        }

        protected override void StartAutoConnectThread()
        {
            if (_autoConnectThread != null)
            {
                return;
            }

            _autoConnectThread = new Thread(() =>
            {
                AutoConnectThreadRunning = true;

                while (AutoConnectThreadRunning)
                {
                    if (TryConnect())
                    {
                        // Success
                        return;
                    }

                    Thread.Sleep(200);
                }
            });

            _autoConnectThread.IsBackground = true;
            _autoConnectThread.Start();
        }

        protected override void UpdateSubscriptions()
        {
            if (_eyeTracker == null)
            {
                return;
            }

            if (_subscribeToGaze && !_subscribingToGazeData)
            {
                // [KV]
                _eyeTracker.TimeSynchronizationReferenceReceived += _eyeTracker_TimeSynchronizationReferenceReceived;

                _eyeTracker.GazeDataReceived += GazeDataReceivedCallback;
                _subscribingToGazeData = true;
            }
            else if (!_subscribeToGaze && _subscribingToGazeData)
            {
                // [KV]
                _eyeTracker.TimeSynchronizationReferenceReceived -= _eyeTracker_TimeSynchronizationReferenceReceived;

                _eyeTracker.GazeDataReceived -= GazeDataReceivedCallback;
                _subscribingToGazeData = false;
            }
        }

        #endregion Private Eye Tracking Methods
    }
}