// NS_SEGMENT | Used for testing
using Peripherals.UserInput;

using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;
using Helpers.Engine;

namespace Peripherals.EyeTracking
{
    /// <summary>
    /// [SEGMENT] Public Interface | + public functionality in <CORE> | + Classes
    /// </summary>
    public class EyeTrackerManager_Base : MonoBehaviour, IDisposable
    {
        private Config config;

        private EyeTracker_UI ui = null;

        public static event EventHandler<EventArgs<TimestampedPosition>> onGazeUpdated;
        public static event EventHandler<EventArgs<Sacada>> onSacadaEnd;
        public static event EventHandler<EventArgs<Blink>> onBlinkEnd;
        public static event EventHandler<EventArgs<Blink>> onBlinkInfo;
        public static event EventHandler<EventArgs<ProgressReport>> onCalibrateProgressReport;
        public static event EventHandler<MessageArgs> onMessageFailed_Thread;
        public static event EventHandler<MessageArgs> onMessageWritten_Thread;
        public static event EventHandler<MessageArgs> onCommandFailed;
        public static event EventHandler<MessageArgs> onCommandSent;
        public static event EventHandler<MessageArgs> onDebugInfo;

        public bool isConnected { get; private set; }
        private bool isSimulated = false; // we dont care so much about this
        private bool isRecording = false;
        private bool isCurrentGazeValid { get { return IsGazeValid(currentGaze); } }

        private bool IsGazeValid(TimestampedPosition gaze)
        {
            return IsGazeValid(gaze?.position);
        }

        private bool IsGazeValid(Vector2? gaze)
        {
            return gaze != null;
        }

        private bool isCurrentGazeValidAndWithinScreen { get { return isCurrentGazeValid ? IsGazeWithinScreen(currentGaze.position) : false; } }
        private bool isCurrentGazeValidAndNearFixation { get { return isCurrentGazeValid ? IsGazeWithinBounds(currentGaze.position) : false; } }
        
        private bool bothLostAndOver { get { return
                    leftLostAndOver && rightLostAndOver; } }

        private bool eitherLostAndOver { get { return
                    leftLostAndOver || rightLostAndOver; } }
        
        private bool leftLostAndOver { get {
                return eyeLostLeft != null &&
GetCurrentTimestampMS() - eyeLostLeft.startTimestampMS > config.blinkTimeLimitMS; } }

        private bool rightLostAndOver { get {
                return eyeLostRight != null &&
GetCurrentTimestampMS() - eyeLostRight.startTimestampMS > config.blinkTimeLimitMS; } }
        
        private bool bothLostButUnder { get { return
                    leftLostButUnder && rightLostButUnder; } }

        private bool eitherLostButUnder { get { return
                    leftLostButUnder || rightLostButUnder; } }
        
        private bool leftLostButUnder { get {
                return eyeLostLeft != null &&
GetCurrentTimestampMS() - eyeLostLeft.startTimestampMS <= config.blinkTimeLimitMS; } }

        private bool rightLostButUnder { get {
                return eyeLostRight != null &&
GetCurrentTimestampMS() - eyeLostRight.startTimestampMS <= config.blinkTimeLimitMS; } }

        private float lastTimeValidGaze = Mathf.NegativeInfinity;
        public EyeTrackerState currentState { get; private set; }
        private TimestampedPosition sacadaBegin = null;
        /// <summary>
        /// Joint from any number of tracked eyes
        /// </summary>
        private TimestampedPosition currentGaze;
        private EyeLost eyeLostLeft;
        private EyeLost eyeLostRight;
        private Sample sample = null;
        private readonly List<Sacada> sacades = new List<Sacada>();
        private readonly List<Blink> blinks = new List<Blink>();
        private bool debug = false;
        private bool useMouseForGaze = false;
        List<string[]> pendingMessageBatches = new List<string[]>();
        public string currentFileName_NoSuffix { get; private set; }
        private string logsDirectory;

        protected virtual List<Sacada> Core_GetSacades() { return new List<Sacada>(); }
        protected virtual List<Blink> Core_GetBlinks() { return new List<Blink>(); }
        protected virtual bool Core_IsNull() { return false; }
        protected virtual bool Core_IsConnected() { return false; }
        protected virtual Sample Core_GetSample() { return null; }
        protected virtual void OnUpdate(float dT) { }
        protected virtual double Core_GetTrackerTimeMS() { return -1; }
        protected virtual double Core_GetTrackerTimeMS_Fallback() { return -1; }
        protected virtual double Core_GetCurrentTimeMS_Debug() { return -1; }
        protected virtual void Core_WriteMessage_TS(string msg) { }
        protected virtual string GetName() { return "EYE_TRACKER_BASE"; }
        protected virtual void OnInitialized(ImplementationConfig config) { }
        protected virtual bool Core_TryConnect() { return false; }
        protected virtual bool Core_TryConnect_Simulation() { return false; }
        protected virtual bool GetUseSimulation() { return false; }
        protected virtual void ExperimentOnset() { }
        protected virtual void Core_DoCommandRequested(string command) { }
        protected virtual void Core_DoRunCalibration(IList<Vector2> calibrationTargets, Action<ProgressReport> progressReportCB) { }
        protected virtual void Core_CreateTrackingFile(string filePath) { }
        protected virtual void Core_CloseFile() { }
        protected virtual void Core_GetFile(string fullPath) { }
        protected virtual void Core_StartRecording(string filename) { }
        protected virtual void Core_StopRecording() { }
        protected virtual string GetTrackingFileNameSuffix() { return ""; }
        protected virtual void Core_CloseSystem() { }

        public bool isInitialized { get; private set; }
        
        #region Public Accessors

        // Called by THE MANAGER
        public void Dispose()
        {
            // If we got interrupted, the current file name will be running
            if (currentFileName_NoSuffix.IsNullOrEmpty())
            {
                this.LogWarning("Exiting | Nothing to do");
                return;
            }

            string fileName_NoSuffix_Interrupted = "{0}_INTERRUPTED"._Format(currentFileName_NoSuffix);
            string fullFilePath = GetFullEyeTrackingFilePath_FromFileName(fileName_NoSuffix_Interrupted);

            try
            {
                StopRecordingCloseAndTransfer(fullFilePath);
            }
            catch (Exception)
            {
                //Debug.LogError(ex.ToString());
            }
            finally
            {               
                // Call last!
                if (config.closeSystemAtQuit)
                    Core_CloseSystem();
            }
        }

        public void StartRecording()
        {
            ToggleRecording(true);
        }

        public void StopRecording()
        {
            ToggleRecording(false);
        }

        public void FinalizeEyeTrackingFile()
        {
            string fullFilePath = GetFullEyeTrackingFilePath_FromFileName(currentFileName_NoSuffix);

            StopRecordingCloseAndTransfer(fullFilePath);
                currentFileName_NoSuffix = "";
        }
        
        #endregion

        public bool IsConnectedDebug()
        {
            return isConnected || useMouseForGaze;
        }

        private double GetCurrentTimestampMS()
        {
            return TimeWrapper.currentTimestampMS;
        }

        #region public Management

        public void TryConnect()
        {
            this.LogWarning("Trying to connect..");

            if (isConnected)
            {
                DisplayDebugInfo(GetName() + " already connected");
            }
            else
            {
                ConnectionResult result = DoConnect();
                OnConnectAttempted(result);
            }
        }

        public enum ConnectionResult { Unknown, Failed, Simulated, AllGood }

        private ConnectionResult DoConnect()
        {
            bool useSimulation = GetUseSimulation();

#if UNITY_EDITOR
            // useSimulation = true;
#endif
            if (!useSimulation && Core_TryConnect()) return ConnectionResult.AllGood;
            if (useSimulation && Core_TryConnect_Simulation()) return ConnectionResult.Simulated;

            return ConnectionResult.Failed;
        }

        private void OnConnectAttempted(ConnectionResult result)
        {
            string msg =
                result == ConnectionResult.Failed ? "connection failed" :
                result == ConnectionResult.Simulated ? "Simulated" :
                result == ConnectionResult.AllGood ? "connected" : "okay?";

            DisplayDebugInfo("{0} {1}"._Format(GetName(), msg));

            // Debug.LogError(core.IsConnected + " : " + core.IsConnectionSimulated());

            isConnected = result == ConnectionResult.AllGood || result == ConnectionResult.Simulated;
            isSimulated = result == ConnectionResult.Simulated;

            // if (isConnected)
            ExperimentOnset();
        }

        protected void RequestSendCommand(string command)
        {
            // Debug.LogWarning(command); return;
            if (!Core_IsConnected())
            {
                // And to the full log
                onCommandFailed?.Invoke(null, new MessageArgs(TimeWrapper.GetCurrentTimestamp_TS(), GetName(), command + ";NOT_CONNECTED"));
                this.LogWarning("Couldn't execute command :: {0}"._Format(command));
                return;
            }

            // And to the full log
            onCommandSent?.Invoke(null, new MessageArgs(TimeWrapper.GetCurrentTimestamp_TS(), GetName(), command));

            Core_DoCommandRequested(command);
            this.Log("Executed command :: {0}"._Format(command));
        }


        protected IList<Vector2> GetCalibrationTargets()
        {
            return config.calibrationTargets_Normalized;
        }

        private void Awake()
        {
            OnAwake();
            Toggle(false);
        }

        protected virtual void OnAwake() { }

        protected virtual void Toggle(bool on) { }

        public void SetLogsDirectory(string logsDirectory)
        {
            this.logsDirectory = logsDirectory;
        }

        public void Initialize(EyeTracker_UI ui, Config config, ImplementationConfig implementationConfig, bool persistThroughScenes)
        {
            this.config = config;

            // [SOS] Do this first as it may be used in checks afterwards
            isInitialized = true;

            if (persistThroughScenes)
                DontDestroyOnLoad(gameObject);

            isConnected = false;

            this.ui = ui;
            if (ui)
            {
                ui.Initialize();
                ui.onEyeClicked += UI_OnEyeClicked;
            }
            else
                this.LogError("Eye Tracker didnt have UI");

            Toggle(true);
            OnInitialized(implementationConfig);
        }

        public void SetFixation(Vector2 fixationPosition, float maxDistance_Pixels)
        {
            this.fixationPosition = fixationPosition;
            this.maxDistance_Pixels = maxDistance_Pixels;
        }

        public override string ToString()
        {
            return GetName();
        }

        public void DeInitialize()
        {
            isInitialized = false;
            ui?.DeInitialize();
        }

        private void Update()
        {
            if (!isInitialized) return;

            sample = null;
            sacades.Clear();
            blinks.Clear();

            if (!InputManager.GetKey(config.EXPERIMENTER_USE_MOUSE_FOR_GAZE_MODIFIER) &&
                InputManager.GetKeyUp(config.EXPERIMENTER_DEBUG_GAZE_POSITION))
                debug = !debug;

            if ((Debug.isDebugBuild || config.allowDebugOnRelease) &&
                InputManager.GetKey(config.EXPERIMENTER_USE_MOUSE_FOR_GAZE_MODIFIER) &&
                InputManager.GetKeyUp(config.EXPERIMENTER_USE_MOUSE_FOR_GAZE_TOGGLE_KEY))
            {
                useMouseForGaze = !useMouseForGaze;
                debug = useMouseForGaze;
            }

            // === DEBUG MOUSE
            if (useMouseForGaze)
            {
                // Handle current sample
                sample = new Sample();
                sample.timestampMS = GetCurrentTimestampMS();
                sample.leftEyePosition = new Vector2(
                    Input.mousePosition.x - 50,
                    Input.mousePosition.y); // This is how it comes to us from EyeLink

                // Handle binocular
                sample.rightEyePosition = new Vector2(
                    Input.mousePosition.x + 50,
                    Input.mousePosition.y); // This is how it comes to us from EyeLink

                // Handle sacada begin
                TimestampedPosition sampleCenterPosition = sample.GetCenterPosition();

                if (sampleCenterPosition != null)
                {
                    if (InputManager.GetKeyDown(config.EXPERIMENTER_USE_MOUSE_FOR_GAZE_SACADA_KEY))
                    {
                        // Begin is current sample
                        sacadaBegin = new TimestampedPosition();
                        sacadaBegin.position = sampleCenterPosition.position;

                        sacadaBegin.timestampMS = sample.timestampMS;
                    }
                    // Handle sacada end
                    else if (InputManager.GetKeyUp(config.EXPERIMENTER_USE_MOUSE_FOR_GAZE_SACADA_KEY))
                    {
                        Sacada sacada = new Sacada();

                        // Begin is already logged
                        sacada.start = new TimestampedPosition();
                        sacada.start.position = sacadaBegin.position;
                        sacada.start.timestampMS = sacadaBegin.timestampMS;

                        // End is current sample
                        sacada.end = new TimestampedPosition();
                        sacada.end.position = sampleCenterPosition.position;
                        sacada.end.timestampMS = sample.timestampMS;

                        sacades.Add(sacada);
                    }
                }

                // Do blink
                if (InputManager.GetKey(KeyCode.Mouse0))
                    sample.leftEyePosition = null;// new Vector2(-16000, 17000);
                if (InputManager.GetKey(KeyCode.Mouse1))
                    sample.rightEyePosition = null;// new Vector2(-16000, 17000);
            }
            // === PROPER VIA EYE TRACKER
            else if (!Core_IsNull())
            {
                // Update wether we are connected or not
                isConnected = Core_IsConnected();

                if (isConnected && isRecording)
                {
                    if (config.getSamples)
                        sample = Core_GetSample();

                    if (config.getSacades)
                        sacades.AddRange(Core_GetSacades());

                    if (config.getBlinks)
                        blinks.AddRange(Core_GetBlinks());
                }
            }
            // Not taking manual input and no core to connnect to - nothing to do here!
            else
            {

            }

            // Do we even have DATA ?
            currentGaze = sample?.GetCenterPosition();

            if (sample == null) { }
            else
            {
                // Do we even HAVE left data?
                if (IsGazeValid(sample.leftEyePosition))
                {
                    // Debug.LogError("LEFT :: " + sample.leftEyePosition.Value);

                    // LOST AND AFTER
                    if (leftLostAndOver)
                        this.LogWarning("Left eye was lost and returned AFTER the blink limit. Nothing to do.");
                    // LOST BUT WITHIN
                    else if (leftLostButUnder)
                    {
                        this.LogWarning("Left eye was lost but returned WITHIN the blink limit. Raising blink.");

                        RaiseOnBlink(new Blink(eyeLostLeft, GetCurrentTimestampMS()));
                    }
                    // Not LOST 
                    else { }

                    eyeLostLeft = null;
                }
                // We do NOT have left data
                else
                {
                    // And it's the first frame too!
                    if (eyeLostLeft == null)
                    {
                        this.LogWarning("Left eye was lost.");

                        eyeLostLeft = new EyeLost();
                        eyeLostLeft.eye = Eye.Left;
                        eyeLostLeft.startTimestampMS = GetCurrentTimestampMS();
                    }
                    // We already know that we're blinking
                    else
                    {

                    }
                }

                // Do we even HAVE right data?
                if (IsGazeValid(sample.rightEyePosition))
                {
                    // Debug.LogError("RIGHT :: " + sample.rightEyePosition.Value);

                    // Lost and AFTER
                    if (rightLostAndOver)
                        this.LogWarning("Right eye was lost and returned AFTER the blink limit. Nothing to do.");
                    // Lost but WITHIN
                    else if (rightLostButUnder)
                    {
                        this.LogWarning("Right eye was lost but returned WITHIN the blink limit. Raising Blink.");

                        RaiseOnBlink(new Blink(eyeLostRight, GetCurrentTimestampMS()));
                    }
                    // Not LOST 
                    else { }

                    eyeLostRight = null;
                }
                // We do NOT have right data
                else
                {
                    // And it's the first frame too!
                    if (eyeLostRight == null)
                    {
                        this.LogWarning("Right eye was lost.");

                        eyeLostRight = new EyeLost();
                        eyeLostRight.eye = Eye.Right;
                        eyeLostRight.startTimestampMS = GetCurrentTimestampMS();
                    }
                    // We already know that we're blinking
                    else
                    {

                    }
                }
            }

            if (useMouseForGaze || isRecording)
            {
                RaiseOnGazeUpdated(currentGaze);

                ui?.SetDebugCrossPosition(currentGaze?.position);

                if (debug)
                {
                    ui?.ToggleDebugCross(isCurrentGazeValid);

                    if (isCurrentGazeValid)
                        ui?.DisplayText(currentGaze.position.ToString("#"));
                }
                else
                    ui?.ToggleDebugCross(false);

                foreach (Sacada sacada in sacades)
                    onSacadaEnd?.Invoke(this, sacada);

                sacades.Clear();

                foreach (Blink blink in blinks)
                    onBlinkInfo?.Invoke(this, blink);

                blinks.Clear();
            }
            else
            {
                ui?.ToggleDebugCross(false);
            }

            RefreshTrackingState();

            ProcessAllPendingWriteRequests_NotTS();
            OnUpdate(TimeWrapper.deltaTime_SinceLastUpdate_NotTS);
        }

        private void ToggleRecording(bool on)
        {
            isRecording = on;

            if (on)
            {
                if (isConnected)
                    Core_StartRecording(currentFileName_NoSuffix);

                RequestWriteToTracker_NotTS("recording -> ON");
            }
            else
            {
                RequestWriteToTracker_NotTS("recording -> OFF");

                if (isConnected)
                    Core_StopRecording();
            }

            if (isConnected)
                ui?.HarelSelect();
        }

        #endregion

        #region Handlers

        private void UI_OnEyeClicked(object sender, EventArgs e)
        {
            // Debug.LogError("Eye Clicked");
            // If we're not connected
            if (!isConnected)
            {
                // Debug.LogError("Trying to connect");
                // Go for connection
                TryConnect();
                // Debug.LogError("Tried connecting");
            }


            // Otherwise..
            if (isConnected)
            {
                // Debug.LogError("Connected, running calibration");
                // Go for callibration
                DoRunCalibration();
            }

#if UNITY_EDITOR
            // DoRunCalibration(); Debug.LogError("Calibrating editor-only");
#endif
        }

        private void DoRunCalibration()
        {
            if (isSimulated)
                DisplayDebugInfo("Simulating Calibration");
            else
                DisplayDebugInfo("Running Callibration");

            Core_DoRunCalibration(config.calibrationTargets_Normalized, progressReport =>
            {
                DisplayDebugInfo("Calibration {0} ({1})"._Format(progressReport.isDone, progressReport.value01.PercentileToPercent()));
                onCalibrateProgressReport?.Invoke(this, progressReport);
            });

            // WriteToEDF("calibrating");
            ui?.HarelSelect();
        }

        #endregion


        #region Writing Out

        /// <summary>
        /// Closes the EDF file. Potentially transfers it (if the full name isn't empty)
        /// </summary>
        /// <param name="fullPath">Where to transfer to local computer</param>
        public void StopRecordingCloseAndTransfer(string fullPath)
        {
            StopRecording();

            // Transfer it!
            CloseAndTransferFile(fullPath);
        }

        public void CloseAndTransferFile(string fullPath)
        {
            CloseFile();
            TransferFile(fullPath);
        }

        /// <summary>
        /// Creates a new EDF if needed, transfers old one if existing.
        /// </summary>
        public bool TryCreateTrackingFile(string newFileName_NoSuffix, bool shouldStartRecording)
        {
            string oldFileName_NoSuffix = currentFileName_NoSuffix;

            if (oldFileName_NoSuffix == newFileName_NoSuffix)
            {
                // Already writing to that edf
                DisplayDebugInfo("Already writing to {0}.{1}"._Format(newFileName_NoSuffix, GetTrackingFileNameSuffix()), true);
                return false;
            }

            // Close the previous EDF if it existed
            if (!oldFileName_NoSuffix.IsNullOrEmpty())
            {
                string oldFullPath = GetFullEyeTrackingFilePath_FromFileName(oldFileName_NoSuffix);
                StopRecordingCloseAndTransfer(oldFullPath);
            }

            // DisplayText("Setting current to {0} (was {1})"._Format(newFileName_NoSuffix, oldFileName_NoSuffix));
            currentFileName_NoSuffix = newFileName_NoSuffix;

            // Start with the new 
            return CreateCurrentTrackingFile(shouldStartRecording);
        }

        private string debugRemoteFileName_NoSuffix = "";

        /// <summary>
        /// [SOS] Set <see cref="currentFileName_NoSuffix"/> before calling!
        /// Create an EDF file at the EyeLink computer and write to it
        /// </summary>
        /// <param name="fileNameNoSuffix">the filename (WITHOUT .edf)</param>
        private bool CreateCurrentTrackingFile(bool shouldStartRecording)
        {
            // Store Fake Remote on Memory
            debugRemoteFileName_NoSuffix = currentFileName_NoSuffix;

            if (!isConnected)
            {
                DisplayDebugInfo("{0} is not connected - would have written to Remote {1}.{2}"._Format(
                    GetName(), currentFileName_NoSuffix, GetTrackingFileNameSuffix()));
                return false;
            }

            /*
            if (core.IsConnectionSimulated())
            {
                DisplayText("EyeLink is simulated - would have written to {0}.edf"._Format(fileName));
                return false;
            }
            */

            DisplayDebugInfo("Recording to Remote {0}.{1}"._Format(currentFileName_NoSuffix, GetTrackingFileNameSuffix()));

            Core_CreateTrackingFile("{0}.{1}"._Format(currentFileName_NoSuffix, GetTrackingFileNameSuffix()));

            /// [SOS] Should be called before any calls to <see cref="RequestWriteToTracker_NotTS(string[])"/>
            if (shouldStartRecording) StartRecording(); 

            // Monitor Refresh Rate
            RequestWriteToTracker_NotTS("SCREEN_REFRESH_RATE " + Screen.currentResolution.refreshRate); // [TODO] :: Get more accurate refresh rate (https://forum.unity.com/threads/acquiring-accurate-refresh-rate-for-display.572995/)

            // Monitor Pixel coords
            RequestWriteToTracker_NotTS("SCREEN_RESOLUTION " + Screen.currentResolution.width + " " + Screen.currentResolution.height);

            // At the top of each EDF file write..
            // Unity Framerate
            // WriteToEDF("UNITY_FRAME_RATE " + config.framerate);  // [TODO] :: Get framerate otherwise

            ui?.HarelSelect();
            return true;
        }

        private void CloseFile()
        {
            if (!isConnected)
            {
                DisplayDebugInfo("Not connected would have closed Remote", true);
                return;
            }

            DisplayDebugInfo("Closing Remote");
            Core_CloseFile();
        }

        /// <summary>
        /// Transfers the latest .edf file to the specified location
        /// </summary>
        /// <param name="fullPath">Local computer file path</param>
        private void TransferFile(string fullPath)
        {
            DisplayDebugInfo("Requested transfer Remote to {0}"._Format(fullPath));

            if (!isConnected)
            {
                // Fake transfer remote
                if (config.createFakeTrackingFileWhenNotConnectedRelease || EngineWrapper.Debug_IsDebugBuild)
                    Logging.Core.FileWrapper.WriteToFile_TS(fullPath, debugRemoteFileName_NoSuffix);

                DisplayDebugInfo("Not Connected | Would have transferred Remote");
                return;
            }

            DisplayDebugInfo("Transferring Remote");

            try
            {
                Core_GetFile(fullPath);
            }
            catch { }

            ui?.HarelSelect();
        }

        public bool RequestWriteToTracker_TS(object msg, out string error)
        {
            EyeTrackerTimestampData timestamps = GetCurrentTimestamps();

            // Log the time of sending ! // This may also be the timestamp of an Update, but we care for SENDING
            long unityTimeMS = (long)Math.Round(timestamps.unityMS);                        // Unity Timestamp
            long estimatedEyeTrackerMS = (long)Math.Round(timestamps.estimatedTrackerMS);   // Tracker estimation (usec)

            string msgSTR = "{0};{1};{2}"._Format(msg, unityTimeMS, estimatedEyeTrackerMS);

            error = "";

            if (!isConnected)
            {
                error = "NOT_CONNECTED";
                onMessageFailed_Thread?.Invoke(null, new MessageArgs(TimeWrapper.GetCurrentTimestamp_TS(), "{0}_{1}"._Format(GetName(), error), msgSTR));
                return false;
            }

            if (!isRecording)
            {
                error = "NOT_RECORDING";
                onMessageFailed_Thread?.Invoke(null, new MessageArgs(TimeWrapper.GetCurrentTimestamp_TS(), "{0}_{1}"._Format(GetName(), error), msgSTR));
                return false;
            }

            // Send the msg to the Tracker
            Core_WriteMessage_TS(msgSTR);

            // And to the full log
            onMessageWritten_Thread?.Invoke(null, new MessageArgs(TimeWrapper.GetCurrentTimestamp_TS(), GetName(), msgSTR));

            return true;
        }

        /// <summary>
        /// [200921] DEPRECATED | use <see cref="RequestWriteToTracker_TS"/> for events
        /// </summary>
        /// <param name="msgs"></param>
        public void RequestWriteToTracker_NotTS_Event(params string[] msgs)
        {
            if (config.disableVerboseEventMessages)
            {
                this.LogWarning("Verbose disabled from config!");
                return;
            }
            RequestWriteToTracker_NotTS(msgs);
        }

        protected void RequestWriteToTracker_NotTS(params string[] msgs)
        {
            RequestWriteToTracker_NotTS(null, msgs);
        }

        /// <summary>
        /// [200921] DEPRECATED | use <see cref="RequestWriteToTracker_TS"/> for events
        /// </summary>
        /// <param name="msgs"></param>
        public void RequestWriteToTracker_NotTS_Event(EyeTrackerTimestampData overrideTimestamps, params string[] msgs)
        {
            if (config.disableVerboseEventMessages)
            {
                this.LogWarning("Verbose disabled from config!");
                return;
            }

            RequestWriteToTracker_NotTS(overrideTimestamps, msgs);
        }

        public void RequestWriteToTracker_NotTS(EyeTrackerTimestampData overrideTimestamps, params string[] msgs)
        {
            // figure how this works and if the buffer is actually needed
            pendingMessageBatches.Add(msgs);
            ProcessAllPendingWriteRequests_NotTS(overrideTimestamps); // Force it to write instantly
        }

        private void ProcessAllPendingWriteRequests_NotTS(EyeTrackerTimestampData overrideTimestamps = null)
        {
            foreach (string[] batch in pendingMessageBatches)
                WriteToTracker_NotTS(overrideTimestamps, batch);

            pendingMessageBatches.Clear();
        }

        private void WriteToTracker_NotTS(EyeTrackerTimestampData overrideTimestamps = null, params string[] msgs)
        {
            if (msgs.Length == 0) return;

            if (!isConnected)
            {
                this.LogWarning("Couldn't write {0}"._Format(msgs.ToReadableString()));

                foreach (string msg in msgs)
                    onMessageFailed_Thread?.Invoke(null, new MessageArgs(TimeWrapper.GetCurrentTimestamp_TS(), GetName() + "_NOT_CONNECTED", msg));

                return;
            }

            if (!isRecording)
            {
                this.LogWarning("Not recording, won't be able to write to EDF {0}"._Format(msgs.ToReadableString()));

                foreach (string msg in msgs)
                    onMessageFailed_Thread?.Invoke(null, new MessageArgs(TimeWrapper.GetCurrentTimestamp_TS(), GetName() + "_NOT_RECORDING", msg));

                return;
            }

            EyeTrackerTimestampData timestamps = overrideTimestamps != null ?
                overrideTimestamps : GetCurrentTimestamps();

            // Log the time of sending ! // This may also be the timestamp of an Update, but we care for SENDING
            double unityTimeMS = timestamps.unityMS;
            double estimatedEyeTrackerMS = timestamps.estimatedTrackerMS;
            msgs = msgs.Add(unityTimeMS.ToString());                // Unity Timestamp
            msgs = msgs.Add(estimatedEyeTrackerMS.ToString());         // Tracker estimation (usec)
            msgs = msgs.Add((estimatedEyeTrackerMS - unityTimeMS).ToString());  // Marker DT

            // - DEBUG
            double estimatedEyeTrackerMS_LowRes = timestamps.estimatedTrackerMS_Fallback;
            msgs = msgs.Add("--DEBUG BEGIN--");
            msgs = msgs.Add(TimeWrapper.currentFrameCycleID.ToString());     // Frame Count
            msgs = msgs.Add(Core_GetCurrentTimeMS_Debug().ToString());                          // SDK
            msgs = msgs.Add(estimatedEyeTrackerMS_LowRes.ToString());                      // Tracker estimation (msec)
            msgs = msgs.Add((estimatedEyeTrackerMS_LowRes - unityTimeMS).ToString());      // Marker DT
            msgs = msgs.Add("--DEBUG END--");

            /*
            msgs = msgs.Add("UnityTimeMS {0}"._Format(GetCurrentTime()));
            msgs = msgs.Add("TrackerTimeMS {0}"._Format(core.GetTrackerTime()));
            msgs = msgs.Add("TrackerTimeMSOffset {0}"._Format(core.GetTrackerTimeOffset()));
            msgs = msgs.Add("TrackerTimeUS {0}"._Format(core.GetTrackerTimeUsec()));
            msgs = msgs.Add("TrackerTimeUSOffset {0}"._Format(core.GetTrackerTimeUsecOffset()));
            */

            foreach (string msg in msgs)
            {
                // Send the msg to EyeLink
                Core_WriteMessage_TS(msg);

                // And to the full log
                onMessageWritten_Thread?.Invoke(null, new MessageArgs(TimeWrapper.GetCurrentTimestamp_TS(), GetName(), msg));

                // Debug
                this.Log("Wrote :: " + msg);
            }

            this.Log("Writing message(s) to core :: {0}"._Format(msgs.ToReadableString()));
        }

        public class MessageArgs : EventArgs
        {
            public TimeWrapper.Timestamp timestamp;
            public string name;
            public string msg;
            private int _idx = 0;
            private static int INDEX_ASSIGNMENT = 0;

            public MessageArgs(TimeWrapper.Timestamp timestamp, string name, string msg)
            {
                this.timestamp = timestamp;
                this.name = name;
                this.msg = msg;
                _idx = INDEX_ASSIGNMENT++;
            }

            public override string ToString()
            {
                return "{0};{1};{2};{3}"._Format(_idx, timestamp, name, msg);
            }
        }

        public EyeTrackerTimestampData GetCurrentTimestamps()
        {
            double unityTimeMS = TimeWrapper.currentTimestampMS;
            double estimatedTrackerTimeMS = Core_IsNull() ? -1 : Core_GetTrackerTimeMS();
            double estimatedTrackerTimeMS_Fallback = Core_IsNull() ? -1 : Core_GetTrackerTimeMS_Fallback();

            return new EyeTrackerTimestampData(unityTimeMS, estimatedTrackerTimeMS, estimatedTrackerTimeMS_Fallback);
        }
        
        #endregion

        private void RaiseOnBlink(Blink blink)
        {
            onBlinkEnd?.Invoke(this, blink);
        }

        private void RaiseOnGazeUpdated(TimestampedPosition gaze)
        {
            onGazeUpdated?.Invoke(this, gaze);
        }

        #region UI

        #endregion


        private bool IsGazeWithinScreen(Vector2 gaze)
        {
            return gaze.x.IsBetween(0, Screen.width) && gaze.y.IsBetween(0, Screen.height);
        }

        private Vector2 fixationPosition;
        private float maxDistance_Pixels;

        private bool IsGazeWithinBounds(Vector2 gaze)
        {
            float distance_Pixels = Vector2.Distance(fixationPosition, gaze);
            bool isWithinBounds = distance_Pixels <= maxDistance_Pixels;

            // Debug.Log("Gaze at {0} , {1} pixels from {2} (max is {3})"._Format(gaze, distance_Pixels, fixationPosition, maxDistance_Pixels));

            return isWithinBounds;
        }

        private bool acceptOneEyedData { get { return config.acceptOneEyedData; } }

        private EyeTrackerState GetTrackingState()
        {
            /*
            Debug.Log("=====");
            Debug.Log("Left Eye Lost And Over Blink Limit -> " + leftLostAndOver);
            Debug.Log("Left Eye Lost But Under Blink Limit -> " + leftLostButUnder);
            Debug.Log("Right Eye Lost And Over Blink Limit -> " + rightLostAndOver);
            Debug.Log("Right Eye Lost But Under Blink Limit -> " + rightLostButUnder);
            Debug.Log("Either Eye Lost And Over Blink Limit -> " + eitherLostAndOver);
            Debug.Log("Either Eye Lost But Under Blink Limit -> " + eitherLostButUnder);
            Debug.Log("Both Eyes Lost And Over Blink Limit -> " + bothLostAndOver);
            Debug.Log("Both Eyes Lost But Under Blink Limit -> " + bothLostButUnder);
            */

            if (!IsConnectedDebug()) return EyeTrackerState.NotConnected;

            // If we didn't have SAMPLE data, we're done
            if (sample == null) return EyeTrackerState.InsufficientData;

            // We do accept one-eyed data, but both are missing!
            if (acceptOneEyedData && bothLostAndOver) return EyeTrackerState.InsufficientData;
            // We do not accept one-eyed data, and at least one is missing!
            if (!acceptOneEyedData && eitherLostAndOver) return EyeTrackerState.InsufficientData;

            // We do accept one-eyed data, but both are blinking
            if (acceptOneEyedData && bothLostButUnder) return EyeTrackerState.Blink;
            // We do not accept one-eyed data, and at least one is blinking
            if (!acceptOneEyedData && eitherLostButUnder) return EyeTrackerState.Blink;
            
            if (!isCurrentGazeValidAndWithinScreen) return EyeTrackerState.GazeOutOfScreen;

            if (!isCurrentGazeValidAndNearFixation) return EyeTrackerState.GazeOffFixation;

            // if (!isRecording) return State.NotRecording;
            return EyeTrackerState.AllGood;
        }

        private void RefreshTrackingState()
        {
            EyeTrackerState newState = GetTrackingState();
            
            SetCurrentState(newState);
        }

        public static EventHandler<TrackingStateArgs> onTrackingStateChanged;

        private void SetCurrentState(EyeTrackerState newState)
        {
            if (newState != currentState)
                onTrackingStateChanged?.Invoke(null, new TrackingStateArgs(
                    TimeWrapper.GetCurrentTimestamp_TS(), GetName(), currentState, newState));

            currentState = newState;

            ui?.ToggleImage_Simulated(newState != EyeTrackerState.NotConnected && isSimulated);
            ui?.ToggleImage_Connection(newState == EyeTrackerState.NotConnected);
            ui?.ToggleImage_Warning(newState == EyeTrackerState.InsufficientData);
        }

        private void DisplayDebugInfo(string text, bool debug = false)
        {
            if (config.logDisplayedDebugInfo)
                onDebugInfo?.Invoke(this, new MessageArgs(TimeWrapper.GetCurrentTimestamp_TS(), name, text));

            ui?.DisplayText(text);

            if (debug)
                this.LogWarning(text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName_NoSuffix">without .edf</param>
        /// <returns></returns>
        public string GetFullEyeTrackingFilePath_FromFileName(string fileName_NoSuffix)
        {
            return logsDirectory + fileName_NoSuffix + "." + GetTrackingFileNameSuffix();
        }

        public void OnLevelComplete()
        {
            // BEFORE stop recording
            RequestWriteToTracker_NotTS_Event("LEVEL_COMPLETE");
        }

        public class Config
        {
            [HideInInspector]
            public string trackerType_Comment = "EyeLink = 0 , Tobii = 1";
            public TrackerType trackerType = TrackerType.EyeLink;
            public enum TrackerType { EyeLink = 0, Tobii = 1 }
            public string disableVerboseEventMessages_Comment = "Prevents the old multi-line messages about events (that since 21/09/20 we send triggers for)";
            public bool disableVerboseEventMessages = true;

            public float gazeValidThreshold = 0.2f;
            public int blinkTimeLimitMS = 1250;

            public bool closeSystemAtQuit = true; // screenPixelCoords = "0 0 1920 1080";

            public bool acceptOneEyedData = true;
            public bool allowDebugOnRelease = true;

            public Vector2[] calibrationTargets_Normalized = new Vector2[]
            {
                new Vector2(0.5f, 0.56f),
                new Vector2(0.5f, 0.37f),
                new Vector2(0.5f, 0.74f),

                new Vector2(0.33f, 0.56f),

                new Vector2(0.66f, 0.56f),
                new Vector2(0.33f, 0.37f),

                new Vector2(0.66f, 0.37f),
                new Vector2(0.33f, 0.74f),

                new Vector2(0.66f, 0.74f)
            };


            public List<KeyCode> GetExperimenterKeyCodes()
            {
                return new List<KeyCode>()
                {
                    EXPERIMENTER_DEBUG_GAZE_POSITION,
                };
            }

            // TEST
            public KeyCode EXPERIMENTER_USE_MOUSE_FOR_GAZE_TOGGLE_KEY { get { return EXPERIMENTER_DEBUG_GAZE_POSITION; } }
            public string EXPERIMENTER_DEBUG_GAZE_POSITION_COMMENT = "PRESS TO SHOW WHERE THE SUBJECT IS LOOKING";
            public KeyCode EXPERIMENTER_DEBUG_GAZE_POSITION = KeyCode.F7;
            public string EXPERIMENTER_USE_MOUSE_FOR_GAZE_MODIFIER_COMMENT = "IF HELD WHILE PRESSING THE BUTTON ABOVE, MAPS THE MOUSE TO THE GAZE";
            public KeyCode EXPERIMENTER_USE_MOUSE_FOR_GAZE_MODIFIER = KeyCode.LeftShift;
            public string EXPERIMENTER_USE_MOUSE_FOR_GAZE_SACADA_KEY_COMMENT = "HOLD DOWN TO SIMULATE STARTING A SACADA, RELEASE TO SIMULATE ENDING IT. ONLY AVAILABLE WHILE IN MOUSE-FOR-GAZE MODE.";
            public KeyCode EXPERIMENTER_USE_MOUSE_FOR_GAZE_SACADA_KEY = KeyCode.Space;
            public string get_Comment = "GetSamples / Sacades / Blinks disables the requesting of that type of sample / event from the tracker";
            public bool getSamples = true;
            public bool getSacades = true;
            public bool getBlinks = true;
            public bool createFakeTrackingFileWhenNotConnectedRelease = false;
            public bool logDisplayedDebugInfo = true;
        }

        public class ImplementationConfig
        { }

        public class TrackingStateArgs : EventArgs
        {
            public TimeWrapper.Timestamp timestamp;
            public string name;
            public EyeTrackerState oldState;
            public EyeTrackerState newState;

            public TrackingStateArgs(TimeWrapper.Timestamp timestamp, string name, EyeTrackerState oldState, EyeTrackerState newState)
            {
                this.timestamp = timestamp;
                this.name = name;
                this.oldState = oldState;
                this.newState = newState;
            }
        }
    }

    public enum EyeTrackerState
    {
        Unknown,
        NotConnected,
        // NotRecording,
        /// <summary>
        /// No Data for more than BLINK threshold
        /// </summary>
        InsufficientData,
        /// <summary>
        /// Includes blinks within threshold!
        /// </summary>
        Blink,
        GazeOutOfScreen,
        GazeOffFixation,
        AllGood
    }

    public class EyeTrackerTimestampData
    {
        public double unityMS;
        public double estimatedTrackerMS;
        public double estimatedTrackerMS_Fallback;

        public EyeTrackerTimestampData(double unityMS, double estimatedTrackerMS, double estimatedTrackerMS_Fallback)
        {
            this.unityMS = unityMS;
            this.estimatedTrackerMS = estimatedTrackerMS;
            this.estimatedTrackerMS_Fallback = estimatedTrackerMS_Fallback;
        }
    }

    public class TimestampedPosition
    {
        public Vector2 position;
        public double timestampMS;

        public TimestampedPosition() { }

        public TimestampedPosition(Vector2 position, double timestampMS)
        {
            this.position = position;
            this.timestampMS = timestampMS;
        }

        internal string ToCSVString(bool useRelative)
        {
            Vector2 position = this.position;
            if (useRelative)
                position = position.DivideBy(new Vector2(Screen.width, Screen.height));

            return "{0};{1};{2}"._Format(position.x, position.y, timestampMS);
        }

        public static TimestampedPosition FromCSVString(string csvString)
        {
            string[] fields = csvString.Split(';');
            return FromCSVString(fields[0], fields[1], fields[2]);
        }

        public static TimestampedPosition FromCSVString(string posX, string posY, string timeMS)
        {
            TimestampedPosition tP = new TimestampedPosition();
            tP.position.x = Utility_Helper.ToFloat_FromCSV(posX);
            tP.position.y = Utility_Helper.ToFloat_FromCSV(posY);
            tP.timestampMS = Utility_Helper.ToDouble_FromCSV(timeMS);
            return tP;
        }
    }

    public class Sample
    {
        public TimestampedPosition GetCenterPosition()
        {
            Vector2 output = Vector2.zero;

            int count = 0;

            if (leftEyePosition.HasValue)
            {
                output += leftEyePosition.Value;
                count++;
            }

            if (rightEyePosition.HasValue)
            {
                output += rightEyePosition.Value;
                count++;
            }

            if (count == 0) return null;

            return new TimestampedPosition(output / count, timestampMS);
        }

        public Vector2? leftEyePosition;
        public Vector2? rightEyePosition;

        public Sample() { }

        public Sample(Vector2 leftEyePosition, Vector2 rightEyePosition, double timestampMS)
        {
            this.leftEyePosition = leftEyePosition;
            this.rightEyePosition = rightEyePosition;
            this.timestampMS = timestampMS;
        }

        public double timestampMS { get; set; }
    }

    public class Sacada
    {
        public Sacada() { }

        public Sacada(TimestampedPosition start, TimestampedPosition end)
        {
            this.start = start;
            this.end = end;
        }

        public TimestampedPosition start { get; set; }
        public TimestampedPosition end { get; set; }

        internal string ToCSVString(bool useRelative)
        {
            return "{0};{1}"._Format(start.ToCSVString(useRelative), end.ToCSVString(useRelative));
        }

        public static Sacada FromCSVString(string csvString)
        {
            string[] fields = csvString.Split(';');
            return FromCSVString(fields[0], fields[1], fields[2], fields[3], fields[4], fields[5]);
        }

        public static Sacada FromCSVString(string startX, string startY, string startTime, string endX, string endY, string endTime)
        {
            Sacada s = new Sacada();
            s.start = TimestampedPosition.FromCSVString(startX, startY, startTime);
            s.end = TimestampedPosition.FromCSVString(endX, endY, endTime);

            return s;
        }
    }

    public class EyeLost
    {
        public EyeLost() { }

        public Eye eye;
        public double startTimestampMS;

        public EyeLost(Eye eye, double startTimestampMS)
        {
            this.eye = eye;
            this.startTimestampMS = startTimestampMS;
        }
    }

    public class Blink : EyeLost
    {
        public Blink() { }

        public double endTimestampMS;
        public double lengthMS { get { return endTimestampMS - startTimestampMS; } }

        public Blink(EyeLost eyeLost, double endTimestampMS) :
            this(eyeLost.eye, eyeLost.startTimestampMS, endTimestampMS)
        { }

        public Blink(Eye eye, double startTimestampMS, double endTimestampMS) :
            base(eye, startTimestampMS)
        {
            this.endTimestampMS = endTimestampMS;
        }

        internal string ToCSVString()
        {
            return "{0};{1};{2}"._Format(eye, startTimestampMS, endTimestampMS);
        }

        public static Blink FromCSVString(string csvString)
        {
            string[] fields = csvString.Split(';');
            return FromCSVString(fields[0], fields[1], fields[2]);
        }

        public static Blink FromCSVString(string eye, string timeStart, string timeEnd)
        {
            Blink b = new Blink();
            b.eye = eye.ToEnum<Eye>();
            b.startTimestampMS = Utility_Helper.ToDouble_FromCSV(timeStart);
            b.endTimestampMS = Utility_Helper.ToDouble_FromCSV(timeEnd);

            return b;
        }
    }

    public enum Eye
    {
        None = -1,
        Left = 0,
        Right = 1,
        Binocular = 2
    }
}