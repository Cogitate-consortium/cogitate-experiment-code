using Helpers.Engine;
using System;
using System.Collections;
using TGP.Helpers;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// NS_CHECK
    /// </summary>
    public class BaseObject : MonoBehaviour
    {
        [SerializeField] private bool debugPositionTrigger = false;

        public event EventHandler<EventArgs> onOutOfVisibility;

        [SerializeField] private Transform mainObject = null;
        [SerializeField] private FlashingParameters flashingParameters;

        private bool doUpdateBrightness = true;
        private Quaternion lastRot;
        private Vector3 lastPos;
        private bool isSteady = false;
        private Renderer myRenderer;

        private RuntimeConfig config;

        private float randomizationOffset_01 = 0;
        private float randomizationOffset_HL = 0;
        private float motionMultiplierMain = 1;
        private float motionMultiplierSecondary = 1;

        public bool canMove { get; private set; }
        public float brightness { get; private set; }
        public Color rendererColor { get { return myRenderer.material.color; } }

        private Coroutine checkOutOfVisibiltyCR;

        public bool isVisible { get; private set; }

        public float GetRandomizationOffset_01()
        {
            return randomizationOffset_01;
        }

        public float GetRandomizationOffset_HL()
        {
            return randomizationOffset_01;
        }

        protected Transform GetMainObject()
        {
            return mainObject;
        }

        public bool IsSteady()
        {
            return isSteady;
        }

        protected void Initialize(RuntimeConfig config)
        {
            this.config = config;
            Toggle(false);
            StopAllCoroutines();
        }

        public void ResetRandomizationOffset()
        {
            randomizationOffset_01 = Utility_Helper.RandomRange(0, 1f);
            randomizationOffset_HL = Utility_Helper.RandomRange(0, 1f);
        }

        public virtual void Toggle(bool on, bool toggleChildren = false)
        {
            isVisible = on;
            canMove = on;
            //myRenderer.enabled = on;
            if (toggleChildren)
                ToggleChildren(on);

            if (checkOutOfVisibiltyCR != null)
                StopCoroutine(checkOutOfVisibiltyCR);
            if (on)
            {
                checkOutOfVisibiltyCR = StartCoroutine(CheckOutOfVisibility());
            }
        }

        public virtual void ToggleChildren(bool on)
        {
            // We now turn off the whole game object instead of each compoment separately - this helps also optimization
            return;
            /*
            foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>())
                r.enabled = on;
            foreach (ParticleSystem pS in GetComponentsInChildren<ParticleSystem>())
                pS.SetVisibility(on ? 1 : 0);
            */
        }

        private void Awake()
        {
            myRenderer = mainObject.GetComponentInChildren<Renderer>();

            // Turn off
            Toggle(false);

            ResetRandomizationOffset();

            OnAwake();
        }

        protected void SetUpdateBrightness(bool on)
        {
            doUpdateBrightness = on;
        }

        protected Renderer GetRenderer()
        {
            return myRenderer;
        }

        private IEnumerator CheckOutOfVisibility()
        {
            // Give a random time delay, so they dont check all within the same frame
            float waitDelay = UnityEngine.Random.Range(0.5f, 1.5f);
            WaitForSeconds yieldDelay = new WaitForSeconds(waitDelay);

            while (true)
            {
                yield return yieldDelay;
                //yield return null;
                isVisible = Utility_Helper.IsObjectInsideOfCameraVisibility(this.gameObject, 0.048f);
                //isVisible = (this.gameObject.activeInHierarchy && myRenderer.isVisible);

                // Is inside active area?
                if (!Utility_Helper.IsInsideArea(transform, config.activeArea))
                    isVisible = false;

                if (!isVisible && canMove)
                {
                    RaiseOnOutOfVisibility();
                    yield break;
                }
            }
        }

        protected virtual void OnAwake() { }

        float brightness01;
        Vector2 motion01;
        float lerp01;

        float maxT = -1;
        float t = 0;

        // [SOS] 200624 This intuitively makes more sense to be / maxT and not / .flashing period
        // That being said, it's correct as it is because flashing period is the actual period and max T really is the currentTarget
        private void Flash()
        {
            // Time to pick new parameters
            if (t > maxT)
            {
                maxT = flashingParameters.flashingPeriod * randomizationOffset_01.RetargetedFrom0_1To05_2();
                t = 0;
            }

            if (!isVisible || !canMove) return;

            lerp01 = t / flashingParameters.flashingPeriod;
            // Add offset
            lerp01 = (lerp01 + randomizationOffset_01).NegMod(1f);

            brightness01 = flashingParameters.GetBrightness(lerp01);
            motion01.x = flashingParameters.GetMotionSide_X(lerp01);
            motion01.y = flashingParameters.GetMotionMain_Y(lerp01);

            if (doUpdateBrightness)
                UpdateVisibility(brightness01);
            UpdateMotion(motion01);
        }

        public void SetMotionMultiplierMain(float motionMultiplierMain)
        {
            this.motionMultiplierMain = motionMultiplierMain;
        }

        public void SetMotionMultiplierSecondary(float motionMultiplierSecondary)
        {
            this.motionMultiplierSecondary = motionMultiplierSecondary;
        }

        public float GetMotionMultiplierMain()
        {
            return motionMultiplierMain;
        }

        public float GetMotionMultiplierSecondary()
        {
            return motionMultiplierSecondary;
        }

        public void SetRandomizationOffset(float randomizationOffset_01)
        {
            this.randomizationOffset_01 = randomizationOffset_01;
            randomizationOffset_HL = UnityEngine.Random.Range(0f, 1f);
        }

        public void SetLocalRotation(Quaternion rot)
        {
            transform.localRotation = rot;
        }

        public void SetLocalRotation(float rot)
        {
            transform.localRotation = Quaternion.Euler(Vector3.forward * rot);
        }

        public void SetLocalScale(float localScale)
        {
            transform.localScale = Vector3.one * localScale;
        }

        public void SetPosition(Vector3 wantedPosition)
        {
            transform.position = wantedPosition;
        }

        protected virtual void UpdateMotion(Vector2 speed01) { }

        public void Pause(bool doFreeze)
        {
            canMove = !doFreeze;
        }

        public Vector2 GetPixelCoords(Camera camera = null)
        {
            return transform.GetPixelCoordinates(camera);
        }

        public Vector2 GetBackgroundPixelSize()
        {
            return (GetRenderer() as SpriteRenderer).GetPixelSize();
        }

        protected virtual string GetPixelsReport()
        {
            return "Pixels :: Position {0}, Background Size {1}"._Format(GetPixelCoords(), GetBackgroundPixelSize());
        }

        private void Update()
        {
            if (debugPositionTrigger)
            {
                string pixelReport = GetPixelsReport();
                Debug.LogError(pixelReport);
                debugPositionTrigger = false;
            }

            if (!isVisible) return;

            isSteady = lastRot == transform.rotation && lastPos == transform.position;
            lastRot = transform.rotation;
            lastPos = transform.position;

            if (!canMove) return;

            OnUpdate(TimeWrapper.deltaTime_SinceLastUpdate_NotTS);
        }

        private void LateUpdate()
        {
            if (!isVisible) return;
            if (!canMove) return;

            OnLateUpdate();
        }

        /// <summary>
        /// Called only when <see cref="isVisible"/> and <see cref="canMove"/>
        /// </summary>
        /// <param name="dT"></param>
        protected virtual void OnUpdate(float dT)
        {
            Flash();
        }

        /// <summary>
        /// Called only when <see cref="isVisible"/> and <see cref="canMove"/>
        /// </summary>
        protected virtual void OnLateUpdate() { }

        protected virtual void UpdateVisibility(float alpha01)
        {
            brightness = alpha01;
            myRenderer.material.color = myRenderer.material.color.AdjustBrightness(alpha01);
            // myRenderer.material.color = myRenderer.material.color.Change(ColorProperty.a, alpha01);
        }

        protected void RaiseOnOutOfVisibility()
        {
            // Debug.Log("Out of visibility:" + this.transform.name, this.transform);
            onOutOfVisibility?.Invoke(this, null);
        }

        public class RuntimeConfig
        {
            public Rect activeArea { get; set; }

            public RuntimeConfig(Rect activeArea)
            {
                this.activeArea = activeArea;
            }
        }
    }

    [Serializable]
    public struct FlashingParameters
    {
        [Range(0.1f, 15.0f)] public float flashingPeriod;

        public AnimationCurve brightness;

        /// <summary>
        /// Percentile of Screen Height per second
        /// </summary>
        public AnimationCurve mainSpeed;

        /// <summary>
        /// Percentile of Screen Width per second
        /// </summary>
        public AnimationCurve sideSpeed;

        public float GetBrightness(float lerp01)
        {
            return brightness.Evaluate(lerp01);
        }

        public Vector2 GetMotion(float lerp01)
        {
            return new Vector2(GetMotionSide_X(lerp01), GetMotionMain_Y(lerp01));
        }

        public float GetMotionSide_X(float lerp01)
        {
            return sideSpeed.Evaluate(lerp01);
        }

        public float GetMotionMain_Y(float lerp01)
        {
            return mainSpeed.Evaluate(lerp01);
        }
    }
}