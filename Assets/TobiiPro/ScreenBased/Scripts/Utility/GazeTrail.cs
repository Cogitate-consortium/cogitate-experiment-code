//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;

namespace Tobii.Research.Unity
{
    public class GazeTrail : GazeTrailBase
    {
        /// <summary>
        /// Get <see cref="GazeTrail"/> instance. This is assigned
        /// in Awake(), so call earliest in Start().
        /// </summary>
        public static GazeTrail Instance { get; private set; }

        private EyeTracker _eyeTracker;
        private Calibration _calibrationObject;

        public void Setup(EyeTracker eyeTracker, Calibration calibrationObject)
        {
            Instance = this;

            _eyeTracker = eyeTracker;
            base.OnAwake();

            _calibrationObject = calibrationObject;
            base.OnStart();
        }

        protected override void OnAwake() {  }

        protected override void OnStart() { }

        protected override bool GetRay(out Ray ray)
        {
            if (_eyeTracker == null)
            {
                return base.GetRay(out ray);
            }

            var data = _eyeTracker.LatestGazeData;
            ray = data.CombinedGazeRayScreen;
            return data.CombinedGazeRayScreenValid;
        }

        protected override bool HasEyeTracker
        {
            get
            {
                return _eyeTracker != null;
            }
        }

        protected override bool CalibrationInProgress
        {
            get
            {
                return _calibrationObject != null ? _calibrationObject.CalibrationInProgress : false;
            }
        }
    }
}