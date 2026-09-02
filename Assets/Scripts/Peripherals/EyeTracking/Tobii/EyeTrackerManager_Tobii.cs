using System.Collections.Generic;
using UnityEngine;
using TGP.Helpers;
using Tobii.Research.Unity;
using System;

namespace Peripherals.EyeTracking.Tobii
{
    /// <summary>
    /// [CHECK]
    /// </summary>
    public class EyeTrackerManager_Tobii : EyeTrackerManager_Base
    {
        private Config config;
        [SerializeField] private EyeTracker core = null;
        [SerializeField] private Calibration calibration = null;
        [SerializeField] private ScreenBasedSaveData saveData = null;

        protected override void OnAwake()
        {
            calibration?.Setup();
            core?.Setup();
            saveData.Setup(core);
        }

        protected override void Toggle(bool on)
        {
            if (core)
                core.enabled = on;
            if (calibration)
                calibration.enabled = on;
        }

        // [TODO?] Does tobii have a "shut down" functionality?
        protected override void Core_CloseSystem()
        {

        }

        protected override string GetTrackingFileNameSuffix()
        {
            return "xml";
        }

        protected override void Core_DoRunCalibration(IList<Vector2> calibrationTargets, Action<ProgressReport> progressReportCB)
        {
            if (calibration == null) return;

            if (calibration.CalibrationInProgress)
            {
                this.LogWarning("Could not calibrate - Calibration in Progress");
                return;
            }

            progressReportCB?.Invoke(new ProgressReport(false, 0.0f));

            if (calibration.LatestCalibrationSuccessful)
            {
                this.LogWarning("Latest calibration was successful - recalibrating though");
            }

            Vector2[] calibrationTargetsTobii = calibrationTargets.FromBase();

            // Potential to add callbacks etc
            calibration.StartCalibration(calibrationTargetsTobii, a =>
            {
                if (a)
                {
                    progressReportCB?.Invoke(new ProgressReport(true, 1.0f));
                    this.LogWarning("Calibrated Successfully!");
                }
                else
                {
                    progressReportCB?.Invoke(new ProgressReport(true, 0.0f));
                    this.LogError("Calibration Failed!");
                }
            });
        }

        protected override void Core_CloseFile()
        {
            saveData.SetOverrideFilePath("");
        }

        protected override double Core_GetCurrentTimeMS_Debug()
        {
            if (Core_IsNull()) return -1;

            return core.currentSystemTimestampMS;
        }

        // Tobii's data resides on Unity computer
        protected override void Core_GetFile(string fullPath)
        {
            Debug.Log("Transferring file : " + fullPath);
            saveData.CloseDataFile();
            saveData.TransferFileTo(fullPath);
        }

        protected override List<Sacada> Core_GetSacades()
        {
            return new List<Sacada>() { };
        }

        protected override List<Blink> Core_GetBlinks()
        {
            return base.Core_GetBlinks();
        }

        protected override Sample Core_GetSample()
        {
            if (Core_IsNull()) return null;

            IGazeData gazeData = core.LatestGazeData;

            return gazeData.ToBase(config);
        }

        protected override double Core_GetTrackerTimeMS()
        {
            if (Core_IsNull()) return -1;

            return core.currentDeviceTimestampMS;
        }

        protected override double Core_GetTrackerTimeMS_Fallback()
        {
            return Core_GetTrackerTimeMS();
        }

        protected override bool Core_IsConnected()
        {
            if (Core_IsNull()) return false;

            return core.Connected;
        }

        protected override bool Core_IsNull()
        {
            return core.IsNull();
        }

        // [TODO?]
        protected override void Core_DoCommandRequested(string command)
        {
        }

        protected override void Core_CreateTrackingFile(string filePath)
        {
            saveData.SetOverrideFilePath(filePath);
            saveData.OpenDataFile();
        }

        protected override void Core_StartRecording(string filename)
        {
            if (Core_IsNull()) return;

            // [TODO?] Not sure if tobii has a local file name on the device
            core.SubscribeToGazeData = true;
            saveData.isRecording = true;
        }

        protected override void Core_StopRecording()
        {
            if (Core_IsNull()) return;

            core.SubscribeToGazeData = false;
            saveData.isRecording = false;
        }

        protected override bool Core_TryConnect()
        {
            if (Core_IsNull()) return false;

            return core.TryConnect();
        }

        // [TODO?] Not sure if tobii has a simulated connection for when a device is not present
        protected override bool Core_TryConnect_Simulation()
        {
            return false;
        }

        protected override void Core_WriteMessage_TS(string msg)
        {
            saveData.WriteMessage_TS(msg);
        }

        // [TODO?] Do we do any initialization at the onset of the experiment?
        protected override void ExperimentOnset()
        {

        }

        protected override string GetName()
        {
            return "EYE_TRACKER_TOBII";
        }

        // [TODO?] Not sure if tobii has a simulated connection for when a device is not present
        protected override bool GetUseSimulation()
        {
            return false;
        }

        protected override void OnInitialized(ImplementationConfig config)
        {
            base.OnInitialized(config);

            this.config = config as Config;

            if (this.config == null)
            {
                (this).LogError("Invalid Config provided ; needs to be of type {0} was of type {1}"._Format(
                    typeof(Config), typeof(ImplementationConfig)));
            }

            core?.Initialize(this.config.sdkConfig);
        }

        protected override void OnUpdate(float dT)
        {
            saveData.DoUpdate();
        }

        [Serializable]
        public new class Config : ImplementationConfig
        {
            public EyeTracker.Config sdkConfig = new EyeTracker.Config();
            public bool useRayScreen = false; // ray screen doesnt work (needs diff calibration)
            public Vector2 calibrateHorizontalRay = new Vector2(-0.31f, 0.31f);
            public Vector2 calibrateHorizontalDisplay = new Vector2(0f, 1f);
            public Vector2 calibrateVerticalRay = new Vector2(0.83f, 1.16f);
            public Vector2 calibrateVerticalDisplay = new Vector2(1f, 0f);
        }
    }

    public static class EyeTrackerManager_Tobii_Helper
    {
        public static Sample ToBase(this IGazeData sample_Tobii, EyeTrackerManager_Tobii.Config config)
        {
            if (sample_Tobii == null) return null;

            Sample sample_Base = new Sample();

            sample_Base.leftEyePosition = GetBaseCalibratedScreen(sample_Tobii.Left, config);
            sample_Base.rightEyePosition = GetBaseCalibratedScreen(sample_Tobii.Right, config);
            sample_Base.timestampMS = sample_Tobii.GetTimestampMS();

            // Debug.Log("Tobii Timestamp :: " + sample_Tobii.TimeStamp);

            return sample_Base;
        }

        private static Vector2? GetBaseCalibratedScreen(IGazeDataEye left, EyeTrackerManager_Tobii.Config config)
        {
            Vector2? baseRaw = GetBaseRaw(left, config.useRayScreen);
            if (baseRaw == null) return null;

            Vector2 calibrateHorizontal = config.useRayScreen ? config.calibrateHorizontalRay : config.calibrateHorizontalDisplay;
            Vector2 calibrateVertical = config.useRayScreen ? config.calibrateVerticalRay : config.calibrateVerticalDisplay;
            return new Vector2(
                baseRaw.Value.x.RetargetedTo_01(calibrateHorizontal) * Screen.width,
                baseRaw.Value.y.RetargetedTo_01(calibrateVertical) * Screen.height);
        }

        private static Vector2? GetBaseRaw(IGazeDataEye eye, bool useRayScreen)
        {
            return useRayScreen && eye.GazeOriginValid ? eye.GazeRayScreen.origin :
                !useRayScreen && eye.GazePointValid ? (Vector2?)eye.GazePointOnDisplayArea : null;
        }

        public static Vector2[] FromBase(this IList<Vector2> calibrationData_Base)
        {
            return new List<Vector2>(calibrationData_Base).ToArray();
        }

        /*

        public static Blink ToBase(this Core.Blink blink_EyeLink)
        {
            Blink blink_Base = new Blink();

            blink_Base.eye = blink_EyeLink.eye.ToBase();
            blink_Base.startTimestampMS = blink_EyeLink.startTime;
            blink_Base.endTimestampMS = blink_EyeLink.endTime;
            blink_Base.lengthMS = blink_EyeLink.length;

            return blink_Base;
        }

        public static Sacada ToBase(this Core.Sacada sacada_EyeLink)
        {
            Sacada sacada_Base = new Sacada();

            sacada_Base.start = sacada_EyeLink.Start.ToBase();
            sacada_Base.end = sacada_EyeLink.End.ToBase();

            return sacada_Base;
        }

        public static TimestampedPosition ToBase(this Core.XYT tsPos_EyeLink)
        {
            TimestampedPosition tsPos_Base = new TimestampedPosition();

            tsPos_Base.position.x = tsPos_EyeLink.X;
            tsPos_Base.position.y = tsPos_EyeLink.Y;
            tsPos_Base.timestampMS = tsPos_EyeLink.time;

            return tsPos_Base;
        }

        public static Vector2 ToBase(this Core.XY pos_EyeLink)
        {
            Vector2 pos_Base = Vector2.zero;

            pos_Base.x = pos_EyeLink.X;
            pos_Base.y = pos_EyeLink.Y;

            return pos_Base;
        }

        public static Eye ToBase(this Core.EYE eye_EyeLink)
        {
            switch (eye_EyeLink)
            {
                case Core.EYE.NONE:
                    return Eye.None;
                case Core.EYE.LEFT:
                    return Eye.Left;
                case Core.EYE.RIGHT:
                    return Eye.Right;
                case Core.EYE.BINOCULAR:
                    return Eye.Binocular;
            }

            return Eye.None;
        }
        */
    }
}