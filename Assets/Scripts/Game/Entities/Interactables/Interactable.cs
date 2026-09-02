// NS_DEBATABLE
using Game.Core;

using Helpers.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Game.Entities.Interactables
{
    /// <summary>
    /// [SEGMENT] Parameters, everything ielse seems fine
    /// </summary>
    public class Interactable : BaseObject
    {
        private Config config;
        private RuntimeConfig runtimeConfig;

        [Serializable]
        public class Config
        {
            public float jumpingEssenceChance = 0.1f;
            public float chargedBlueFallingMultiplier = 0.5f;
            public float chargedOrangeFallingMultiplier = 2.0f;
        }

        public void Initialize(Config config, RuntimeConfig runtimeConfig)
        {
            base.Initialize(runtimeConfig);
            this.config = config;
            this.runtimeConfig = runtimeConfig;
        }

        public new class RuntimeConfig : BaseObject.RuntimeConfig
        {
            public float spawnVerticalPositionScaled;
            public float scaleMultiplier;
            public float fallingSpeedMultiplier;
            public bool canJumpLanes;

            public RuntimeConfig(Rect activeArea, float spawnVerticalPositionScaled, float scaleMultiplier, float fallingSpeedMultiplier, bool canJumpLanes) : base(activeArea)
            {
                this.spawnVerticalPositionScaled = spawnVerticalPositionScaled;
                this.scaleMultiplier = scaleMultiplier;
                this.fallingSpeedMultiplier = fallingSpeedMultiplier;
                this.canJumpLanes = canJumpLanes;
            }
        }

        public event EventHandler<EventArgs> onReadyToJumpLane;

        private static Color whiteColor = "FFFFFFFF".ToColor();
        private static Color orangeColor = "FFD260FF".ToColor();
        private static Color blueColor = "1CDCE2FF".ToColor();

        public static readonly Dictionary<GameObject, Interactable> collidersLookUpTable = new Dictionary<GameObject, Interactable>();

        public static Interactable TryGetInteractableFromColliderObject(GameObject colliderGO)
        {
            return collidersLookUpTable.TryGet(colliderGO);
        }

        [Header("Editor Values")]
        [SerializeField] private SpeedType speedType = SpeedType.YX;
        [SerializeField] private ParticleSystem absorbParticles = null;
        [SerializeField] private ParticleSystem explodeParticles = null;

        public ObjectColorType colorType { get; private set; }

        private static Vector2 screenSize = new Vector2(18, 10);
        private Vector3 currentSpeed = Vector3.zero;
        private float speedMultiplier_Down = 1;

        // [TEMP, SPAGHETTI] - this avoids two subclasses
        private Collider2D myCollider2D;
        private Collider myCollider;

        // Cache reusable values
        private Vector3 screenPos;
        private Vector2 normalizedScreenPos;
        float mainSpeed;
        float sideSpeed;
        private string selfSenderType = "";
        private bool isAnimating = false;
        public bool canJumpLanes { get { return runtimeConfig.canJumpLanes; } }

        private Coroutine delayToggleOffCR;
        private Coroutine moveToCR;
        private Coroutine changeLaneCR;

        private Vector2 lastScreenPos;

        public override void Toggle(bool on, bool toggleChildren = true)
        {
            base.Toggle(on, toggleChildren);

            isAnimating = false;

            if (on && canJumpLanes)
            {
                // Roll a dice
                bool willJump = UnityEngine.Random.Range(0f, 1f) < config.jumpingEssenceChance;
                if (willJump)
                    StartCoroutine(WaitToJump());
            }

            if (!on)
            {
                if (moveToCR != null)
                    StopCoroutine(moveToCR);
                if (changeLaneCR != null)
                    StopCoroutine(changeLaneCR);
            }

            if (on)
            {
                transform.localScale = Vector3.one * runtimeConfig.scaleMultiplier;

                if (delayToggleOffCR != null)
                    StopCoroutine(delayToggleOffCR);
                lastScreenPos = Utility_Helper.GetRelativeScreenPosition(this.transform.position);
            }

            // [TEMP, SPAGHETTI] - this avoids two subclasses
            if (myCollider)
            {
                myCollider.gameObject.SetActive(on);
                myCollider.enabled = on;
            }
            if (myCollider2D)
            {
                myCollider2D.gameObject.SetActive(on);
                myCollider2D.enabled = on;
            }
        }

        public void Absorb()
        {
            Toggle(false);

            if (absorbParticles == null)
                return;

            // Turn on particles system
            absorbParticles.SetVisibility(1.0f);
            absorbParticles.GetComponent<Renderer>().enabled = true;
            absorbParticles.Play();

            ParticleSystem.MainModule main = absorbParticles.main;
            delayToggleOffCR = StartCoroutine(DelayToggleOff(main.duration));

            RaiseOnOutOfVisibility();
        }

        public void Explode()
        {
            ParticleSystem.MainModule main = explodeParticles.main;
            switch (colorType)
            {
                case ObjectColorType.Gray:
                    main.startColor = whiteColor;
                    break;
                case ObjectColorType.Orange:
                case ObjectColorType.OrangeHigh:
                    main.startColor = orangeColor;
                    break;
                case ObjectColorType.Blue:
                case ObjectColorType.BlueHigh:
                    main.startColor = blueColor;
                    break;
                case ObjectColorType.GrayOrange:
                case ObjectColorType.GrayBlue:
                    main.startColor = whiteColor;
                    break;
            }
            explodeParticles.Play();

            RaiseOnOutOfVisibility();
        }

        private IEnumerator WaitToJump()
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.0f, 1.5f));
            Vector3 normalizedScreenPos = Utility_Helper.GetRelativeScreenPosition(this.transform.position);

            // Don't jump while we are below half of screen
            if (normalizedScreenPos.y < 0.6f)
                yield break;

            if (onReadyToJumpLane != null)
                onReadyToJumpLane(this, new EventArgs());
        }

        public void ChangeLane(float duration, float xPosition)
        {
            changeLaneCR = StartCoroutine(ChangeLaneIE(duration, xPosition));
        }

        private IEnumerator ChangeLaneIE(float duration, float xPosition)
        {
            float timeStarted = 0;
            float initPosX = this.transform.position.x;
            while (true)
            {
                while (!base.canMove)
                    yield return null;

                float lerp = timeStarted / duration;
                this.transform.position = new Vector3(Mathf.Lerp(initPosX, xPosition, lerp), this.transform.position.y, this.transform.position.z);
                if (lerp >= 1)
                {
                    break;
                }
                timeStarted += TimeWrapper.deltaTime_SinceLastUpdate_NotTS;
                yield return null;
            }
        }

        public void MoveTo(float duration, Transform target)
        {
            moveToCR = StartCoroutine(MoveToIE(duration, target));
        }

        // CLEANUP should be for : duration with any smoothing within there but making sure it arrives at duration
        private IEnumerator MoveToIE(float duration, Transform target)
        {
            isAnimating = true;
            float timeStarted = 0;
            Vector3 initPos = this.transform.position;
            while (true)
            {
                while (!base.canMove)
                    yield return null;

                float lerp = timeStarted / duration;
                this.transform.position = Vector3.Lerp(initPos, target.position, lerp);
                if (lerp >= 1)
                {
                    yield break;
                }

                timeStarted += TimeWrapper.deltaTime_SinceLastUpdate_NotTS;
                yield return null;
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();

            // [TEMP, SPAGHETTI] - this avoids two subclasses
            myCollider = GetMainObject().GetComponentInChildren<Collider>();
            myCollider2D = GetMainObject().GetComponentInChildren<Collider2D>();

            if (myCollider)
                collidersLookUpTable.Add(myCollider.gameObject, this);
            if (myCollider2D)
                collidersLookUpTable.Add(myCollider2D.gameObject, this);
        }

        public void SetType(ObjectColorType colorType)
        {
            switch (colorType)
            {
                case ObjectColorType.Gray:
                case ObjectColorType.Orange:
                case ObjectColorType.Blue:
                    speedMultiplier_Down = 1.0f;
                    break;
                case ObjectColorType.OrangeHigh:
                    speedMultiplier_Down = config.chargedOrangeFallingMultiplier;
                    break;
                case ObjectColorType.BlueHigh:
                    speedMultiplier_Down = config.chargedBlueFallingMultiplier;
                    break;
            }

            this.colorType = colorType;
        }

        public static Vector3 NormalizedToAbsolutePosition(Vector2 position01)
        {
            Vector2 wantedPosition = new Vector2();
            wantedPosition.x = position01.x.RetargetedFrom_01(-screenSize.x, screenSize.x);
            wantedPosition.y = position01.y.RetargetedFrom_01(0, screenSize.y);
            return wantedPosition;
        }

        public void SetPositionNormalized(Vector2 position01)
        {
            Vector2 wantedPosition = NormalizedToAbsolutePosition(position01);
            SetPosition(wantedPosition);
        }

        private static float speedMultiplier = 1;
        public static void SetGlobalSpeedMultiplier(float speedMultiplier)
        {
            Interactable.speedMultiplier = speedMultiplier;
        }

        protected override void OnUpdate(float dT)
        {
            if (isAnimating) return;

            base.OnUpdate(dT);

            Vector3 scaledSpeed = currentSpeed * speedMultiplier * runtimeConfig.fallingSpeedMultiplier;
            transform.Translate(scaledSpeed * dT);
        }

        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();

            if (isVisible)
            {
                normalizedScreenPos = Utility_Helper.GetRelativeScreenPosition(this.transform.position);
                Vector2 speed = (normalizedScreenPos - lastScreenPos) / TimeWrapper.deltaTime_SinceLastUpdate_NotTS;
                lastScreenPos = normalizedScreenPos;

                onStatusUpdate?.Invoke(this, new StatusUpdateArgs(colorType, normalizedScreenPos, speed));
            }
        }

        public static EventHandler<StatusUpdateArgs> onStatusUpdate;

        [Serializable]
        public class StatusUpdateArgs : EventArgs
        {
            public ObjectColorType colorType;
            public Vector2 normalizedScreenPos;
            public Vector2 speed;

            public StatusUpdateArgs(ObjectColorType colorType, Vector2 normalizedScreenPos, Vector2 speed)
            {
                this.colorType = colorType;
                this.normalizedScreenPos = normalizedScreenPos;
                this.speed = speed;
            }

            public override string ToString()
            {
                return string.Format("{0};{1}", new Vector2PreciseString(normalizedScreenPos), new Vector2PreciseString(speed));
            }
        }

        protected override void UpdateMotion(Vector2 speed01)
        {
            base.UpdateMotion(speed01);

            mainSpeed = speed01.y.Retargeted(-1, 1, -screenSize.y, screenSize.y) * GetMotionMultiplierMain();
            sideSpeed = speed01.x.Retargeted(-1, 1, -screenSize.x, screenSize.x) * GetMotionMultiplierSecondary();

            currentSpeed.x = sideSpeed;
            currentSpeed.y = speedType == SpeedType.YX ? mainSpeed : 0;
            currentSpeed.z = speedType == SpeedType.ZX ? mainSpeed : 0;

            currentSpeed.y *= speedMultiplier_Down;
        }

        public bool IsOutOfVisibility()
        {
            if (Camera.main == null) return true;

            Vector2 objectScreenPosition = Camera.main.WorldToScreenPoint(this.transform.position);
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
#if UNITY_EDITOR
        screenSize = Utility_Helper.GetEditorWindowSize();
#endif
            bool isObjectOutOfScreen = ((objectScreenPosition.x > screenSize.x * 1.2f || objectScreenPosition.x < -screenSize.x * 0.2f) ||
                                    (objectScreenPosition.y > screenSize.y * 1.2f || objectScreenPosition.y < -screenSize.y * 0.2f));

            return isObjectOutOfScreen;
        }

        private IEnumerator DelayToggleOff(float delay)
        {
            yield return new WaitForSeconds(delay);
            ToggleChildren(false);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        public enum SpeedType { YX, ZX }
    }
}