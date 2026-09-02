using System;
using System.Collections;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Background
{
    /// <summary>
    /// [CLEANUP]
    /// </summary>
    public class BackgroundObject_Abstract_RotatingSquares : BackgroundObject
    {
        [SerializeField] private Transform pivot = null;
        [SerializeField] private Transform overlayObject = null;

        public bool isSatellite { get; private set; }
        private Vector3 initPos;
        private float scaleMulti = 1;
        // private Vector2 scaleLimits;

        public void SetInitPos(Vector3 initPos)
        {
            this.initPos = initPos;
        }

        public void DetachPivot()
        {
            pivot.parent = null;
        }

        public void ResetToInitPos(bool includeSatellites)
        {
            if (isSatellite && !includeSatellites) return;

            transform.position = initPos;
        }

        public void AssignSatellite(BackgroundObject_Abstract_RotatingSquares satellite, Vector3 localOffset)
        {
            satellite.transform.parent = pivot;
            satellite.transform.localPosition = localOffset;
            satellite.isSatellite = true;
        }

        public void SetType(BackgroundManager_Abstract_StaticRotatingSquares.SquareType squareType)
        {
            overrideIsStimulusCandidate = squareType == BackgroundManager_Abstract_StaticRotatingSquares.SquareType.CriticalSquare;
            isSatellite = squareType == BackgroundManager_Abstract_StaticRotatingSquares.SquareType.Satellite;
        }

        private bool overrideIsStimulusCandidate = false;

        public override bool IsStimulusCandidate()
        {
            return overrideIsStimulusCandidate;
            // return !isSatellite;
        }

        protected override void UpdateMotion(Vector2 motion01)
        {
            base.UpdateMotion(motion01);

            /*
            // Change size
            float newScaleAnimationCoef = motion01.x.Fold(0.50f);
            float newScale01 = newScaleAnimationCoef.RetargetedTo_01(0, 0.25f);

            // NonSats are the opposite
            if (!isSatellite)
                newScale01 = 1 - newScale01;

            float newScale = newScale01.RetargetedFrom_01(scaleLimits);

            GetMainObject().localScale = Vector3.one * newScale;

            Vector3 rotationMultiplier = 360 * Vector3.forward;

            // Satellites rotate inversely
            if (isSatellite)
                rotationMultiplier *= -1;

            // The main motion is PIVOT rotation
            pivot.localRotation = Quaternion.Euler(rotationMultiplier * motion01.x);

            // The off motion is TEXTURE rotation -- 
            if (newScale01 < 0.5f)
                GetMainObject().localRotation = Quaternion.Euler(rotationMultiplier * motion01.y);
            */
        }

        protected override void OnUpdate(float dT)
        {
            base.OnUpdate(dT);
            pivot.transform.position = transform.position;
            //pivot.transform.rotation = transform.rotation;

            //if (isSatellite) transform.rotation = Quaternion.identity;
        }

        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
            LogObject();
        }

        public float GetScaleMultiplier()
        {
            return scaleMulti;
        }

        public void SetScaleMultiplier(float scaleMulti)
        {
            this.scaleMulti = scaleMulti;
            // this.scaleLimits = scaleLimits;
        }

        private void Start()
        {
            senderName = "RS" + this.gameObject.GetInstanceID();
        }

        public void SetOverlayParentScale(float scale)
        {
            overlayObject.localScale = Vector3.one * scale;
        }

        public void MainParentScale(float scale)
        {
            GetMainObject().localScale = Vector3.one * scale;
        }

        public static EventHandler<StatusArgs> onStatusUpdate;

        public class StatusArgs : EventArgs
        {
            public string senderName;
            public Vector2 relativeScreenPosition;
            public int eulerZ;
            public float scaleX;
            public float overlayBrightness;
            public float overlayAlpha;

            public StatusArgs(string senderName, Vector2 relativeScreenPosition, int eulerZ, float scaleX, float overlayBrightness, float overlayAlpha)
            {
                this.senderName = senderName;
                this.relativeScreenPosition = relativeScreenPosition;
                this.eulerZ = eulerZ;
                this.scaleX = scaleX;
                this.overlayBrightness = overlayBrightness;
                this.overlayAlpha = overlayAlpha;
            }

            public override string ToString()
            {
                return string.Format("{0};{1};{2};{3};{4}", // ;position-rotation-scale-brightness-alpha
                    relativeScreenPosition.ToStringPrecise(), eulerZ, scaleX, overlayBrightness, overlayAlpha);
            }
        }

        // Logs
        private string senderName;
        private void LogObject()
        {
            onStatusUpdate?.Invoke(this, new StatusArgs(senderName,
                Utility_Helper.GetRelativeScreenPosition(this.transform.position),
                (int)this.transform.rotation.eulerAngles.z,
                this.transform.localScale.x,
                base.overlayBrightness,
                base.overlayAlpha
                ));
        }
    }
}