//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using Helpers.Engine;
using TGP.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace Tobii.Research.Unity.Examples
{
    public class ScreenBasedPrefabDemo : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Attach text object here.")]
        private Text _text;

        [SerializeField] private InputField calibrateHorizontalMin;
        [SerializeField] private InputField calibrateHorizontalMax;
        [SerializeField] private InputField calibrateVerticalMin;
        [SerializeField] private InputField calibrateVerticalMax;
        [SerializeField] private InputField numUpdates;

        [SerializeField] private Peripherals.EyeTracking.EyeTracker_UI eyeTracker_UI;
        [SerializeField] private Toggle useGazeRayScreenOriginVsGazePointOnDisplayArea;
        // [SerializeField] private Toggle originalGazeUseDeviceTimestamp;

        [SerializeField] private EyeTracker _eyeTracker;
        [SerializeField] private Calibration _calibration;
        [SerializeField] private TrackBoxGuide _trackBoxGuide;
        [SerializeField] private GazeTrail _gazeTrail;
        [SerializeField] private ScreenBasedSaveData _saveData;

        private void Start()
        {
            // Cache our prefab scripts.
            TimeWrapper.Initialize();
            eyeTracker_UI.Initialize();

            // for useGazeRayScreenOriginVsGazePointOnDisplayArea = false check right value
            calibrateHorizontalMin.text = "-0.31"; // 0
            calibrateHorizontalMax.text = "0.31"; // 1
            calibrateVerticalMin.text = "0.83"; // 1 
            calibrateVerticalMax.text = "1.16"; // 0

            numUpdates.text = "1";

            _eyeTracker.Setup();
            _calibration.Setup();
            _trackBoxGuide.Setup(_eyeTracker);
            _saveData.Setup(_eyeTracker);
            _gazeTrail.Setup(_eyeTracker, _calibration);

            _eyeTracker.TryConnect();
        }

        private double deviceTimestampMS_Begin;
        private double systemTimestampMS_Begin;
        private double timeWrapperTimestampMS_Begin;
        private double shiftTimestamp = 1000000;
        private bool restartedOnce = false;

        private void Update()
        {
            eyeTracker_UI.ToggleDebugCross(false);
            // We really should run this in full screen.
            if (!Screen.fullScreen)
            {
                _text.text = "<color=red>Please run in full screen!</color>";
                // return;
            }

            // Quit if escape is pressed.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!Application.isEditor)
                {
                    Application.Quit();
                }
            }

            // We are expecting to have all objects.
            if (!_eyeTracker || !_gazeTrail || !_calibration || !_saveData || !_trackBoxGuide)
            {
                return;
            }

            // We really should run this in full screen.
            if (!_eyeTracker.Connected)
            {
                _text.text = "<color=red>Eye tracker not connected</color>";
                return;
            }

            // Thin out updates a bit.
            int _numUpdates = numUpdates.text.ToInt();
            if (TimeWrapper.frameCount_NotTS % _numUpdates != 0)
            {
                return;
            }

            if (Input.GetKeyUp(KeyCode.R))
            {
                RestartTimings();
            }

            if (!restartedOnce && _eyeTracker.currentDeviceTimestampMS > 0 && _eyeTracker.currentSystemTimestampMS > 0)
            {
                RestartTimings();
                restartedOnce = true;
            }

            Vector2 calibrateHorizontal = new Vector2(calibrateHorizontalMin.text.ToFloat(), calibrateHorizontalMax.text.ToFloat());
            Vector2 calibrateVertical = new Vector2(calibrateVerticalMin.text.ToFloat(), calibrateVerticalMax.text.ToFloat());

            bool _useRayScreen = useGazeRayScreenOriginVsGazePointOnDisplayArea.isOn;

            Vector2? baseLeft = GetBase(_useRayScreen, _eyeTracker.LatestProcessedGazeData.Left);
            Vector2? baseRight = GetBase(_useRayScreen, _eyeTracker.LatestProcessedGazeData.Right);

            int count = 0;
            Vector2? gaze = Vector2.zero;
            if (baseLeft != null)
            {
                count++;
                gaze += baseLeft.Value;
            }
            if (baseRight != null)
            {
                count++;
                gaze += baseRight.Value;
            }

            if (count > 0)
                gaze /= count;
            else
                gaze = null;

            // Normalize it
            if (gaze != null)
            {
                Vector2 _gaze = gaze.Value;
                _gaze.x = _gaze.x.RetargetedTo_01(calibrateHorizontal);
                _gaze.y = _gaze.y.RetargetedTo_01(calibrateVertical);

                eyeTracker_UI.SetDebugCrossPosition(_gaze.MultiplyBy(new Vector2(Screen.width, Screen.height)));
                eyeTracker_UI.ToggleDebugCross(Input.GetKeyUp(KeyCode.I));

                gaze = _gaze;
            }


            double currentTimestampMS_Device = _eyeTracker.currentDeviceTimestampMS / shiftTimestamp - deviceTimestampMS_Begin;
            double currentTimestampMS_System = _eyeTracker.currentSystemTimestampMS / shiftTimestamp - systemTimestampMS_Begin;
            double currentTimestampMS_TimeWrapper = TimeWrapper.currentTimestampMS - timeWrapperTimestampMS_Begin;

            // Create an informational string.
            var info = string.Format("<color=yellow>Gaze {0}\nTime {1}\nLatest hit object: {2}\nCalibration in progress: {3}\nSaving data: {4}\nPositioning guide visible: {5}</color>",
                string.Format("L: {0}\nR: {1}\nJN: {2}",
                    baseLeft.HasValue ? baseLeft.Value.ToString("#.0000") : "No gaze",
                    baseRight.HasValue ? baseRight.Value.ToString("#.0000") : "No gaze",
                    gaze.HasValue ? gaze.Value.ToString("#.0000") : "No gaze"),
                _gazeTrail.LatestHitObject != null ? _gazeTrail.LatestHitObject.name : "Nothing",
                string.Format("Dev: {0}, Sys: {1}, TW: {2}", currentTimestampMS_Device, currentTimestampMS_System, currentTimestampMS_TimeWrapper),
                _calibration.CalibrationInProgress ? "Yes" : "No",
                _saveData.SaveData ? "Yes" : "No",
                _trackBoxGuide.TrackBoxGuideActive ? "Yes" : "No");

            _text.text = info;

            eyeTracker_UI.DisplayText(gaze.HasValue ? gaze.Value.ToString("#.0000") : "No gaze");
        }

        private Vector2? GetBase(bool useRayScreen, IGazeDataEye eye)
        {
            return useRayScreen && eye.GazeOriginValid ? eye.GazeRayScreen.origin :
                !useRayScreen && eye.GazePointValid ? (Vector2?)eye.GazePointOnDisplayArea : null;
        }

        private void RestartTimings()
        {
            deviceTimestampMS_Begin = _eyeTracker.currentDeviceTimestampMS / shiftTimestamp;
            systemTimestampMS_Begin = _eyeTracker.currentSystemTimestampMS / shiftTimestamp;
            timeWrapperTimestampMS_Begin = TimeWrapper.currentTimestampMS;
        }
    }
}