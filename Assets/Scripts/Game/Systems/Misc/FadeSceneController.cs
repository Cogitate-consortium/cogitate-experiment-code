using System;
using System.Collections;
using UnityEngine;
using TGP.Helpers;
using Helpers.Engine;

namespace Game.Systems.Misc
{
    /// <summary>
    /// [CHECK]
    /// </summary>
    public class FadeSceneController : MonoBehaviour
    {
        #region Static Access

        private static FadeSceneController instance;

        public static bool isActive = false;

        public static void FadeInOut(Action onFadeIn, Action onWaitPreInstructions, Action onWaitInstructions, Action onWaitPostInstructions, Action onPostWait, Action onFadeOut, bool showInstructions)
        {
            if (instance == null) return;
            instance._FadeInOut(onFadeIn, onWaitPreInstructions, onWaitInstructions, onWaitPostInstructions, onPostWait, onFadeOut, showInstructions);
        }

        #endregion

        public GameObject parent;
        public Camera fixationCamera;
        public SpriteRenderer backgroundImage;
        public SpriteRenderer fixationImage;

        // Start is called before the first frame update
        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            parent.SetActive(false);
        }



        private void _FadeInOut(Action onFadeIn, Action onWaitPreInstructions, Action onWaitInstructions, Action onWaitPostInstructions, Action onPostWait, Action onFadeOut, bool showInstructions)
        {
            StartCoroutine(FadeInOutIE(onFadeIn, onWaitPreInstructions, onWaitInstructions, onWaitPostInstructions, onPostWait, onFadeOut, showInstructions));
        }

        private RuntimeConfig runtimeConfig;

        public class RuntimeConfig
        {
            public float orthoSize;
            public Func<Camera, float> fixationScale;
            public Color fixationColor;
            public bool isFirstHalf;

            public RuntimeConfig(float orthoSize, Func<Camera, float> fixationScale, Color fixationColor, bool isFirstHalf)
            {
                this.orthoSize = orthoSize;
                this.fixationScale = fixationScale;
                this.fixationColor = fixationColor;
                this.isFirstHalf = isFirstHalf;
            }
        }

        private Config config;

        [Serializable]
        public class Config
        {
            public Color bgColor { get { return fadeColorHex.ToColor(); } }

            [SerializeField] private string fadeColorHex_Comment = "Blocker color";
            public string fadeColorHex = "000000";
            [SerializeField] private string fadeInDuration_Comment = "Fade-to-blocker when null event begins";
            public float fadeInDuration = 0.7f;
            [SerializeField] private string waitPreInstructions_Comment = "At the end of this duration, instructions are shown (where applicable) but the duration applies whether they are or not";
            public float waitPreInstructions = 5;
            [SerializeField] private string waitInstructions_Comment = "If showing instructions, this duration is also applied (and the instructions are active)";
            public float waitInstructions = 10;
            [SerializeField] private string waitPostInstructions_Comment = "This duration is applied after the preInstructions wait (if no instructions) or after the instructions wait (if instructions)";
            public float waitPostInstructions = 10;
            [SerializeField] private string postWaitDuration_Comment = "This extra duration is when a few loading operations happen and is always applied after the postInstructions waiting";
            public float postWaitDuration = 1.0f;
            [SerializeField] private string fadeOutDuration_Comment = "Fade-to-scene when null event ends";
            public float fadeOutDuration = 0.7f;
        }

        public static void Initialize(Config config, RuntimeConfig runtimeConfig)
        {
            if (instance == null)
            {
                Debug.LogError("NO INSTANCE");
                return;
            }

            instance._Initialize(config, runtimeConfig);
        }

        private void _Initialize(Config config, RuntimeConfig runtimeConfig)
        {
            this.config = config;
            this.runtimeConfig = runtimeConfig;

            SetCameraOrthoSize(runtimeConfig.orthoSize);
        }

        private void SetCameraOrthoSize(float orthoSize)
        {
            if (fixationCamera)
                fixationCamera.orthographicSize = orthoSize;
            else
                this.LogError("NO FIXATION CAMERA!");

            RefreshFixationSize();
        }

        public void RefreshFixationSize()
        {
            if (fixationImage)
                fixationImage.transform.SetLossyScale(Vector3.one * runtimeConfig.fixationScale(fixationCamera));
            else
                this.LogError("NO FIXATION IMAGE");
        }

        private IEnumerator FadeInOutIE(Action onFadeIn, Action onWaitPreInstructions, Action onWaitInstructions, Action onWaitPostInstructions, Action onPostWait, Action onFadeOut, bool showInstructions)
        {
            backgroundImage.color = config.bgColor.UpdateAlpha(0f);

            // Resize fixation accordinly
            fixationImage.color = runtimeConfig.fixationColor;

            // yield return null;

            isActive = true;
            parent.SetActive(true);

            yield return StartCoroutine(LerpIE(config.fadeInDuration, (float lerp) =>
            {
                backgroundImage.color = config.bgColor.UpdateAlpha(lerp);
            }));

            onFadeIn?.Invoke();

            yield return new WaitForSecondsRealtime(config.waitPreInstructions);

            onWaitPreInstructions?.Invoke();

            if (showInstructions)
                yield return new WaitForSecondsRealtime(config.waitInstructions);

            onWaitInstructions?.Invoke();

            yield return new WaitForSecondsRealtime(config.waitPostInstructions);

            onWaitPostInstructions?.Invoke();

            yield return new WaitForSecondsRealtime(config.postWaitDuration);

            onPostWait?.Invoke();

            yield return StartCoroutine(LerpIE(config.fadeOutDuration, (float lerp) =>
            {
                backgroundImage.color = config.bgColor.UpdateAlpha(1f - lerp);
            }));

            onFadeOut?.Invoke();

            parent.SetActive(false);
            isActive = false;
        }

        private IEnumerator LerpIE(float duration, Action<float> lerpCB)
        {
            float timeStarted = TimeWrapper.time_NotTS;

            while (true)
            {
                float lerp = (TimeWrapper.time_NotTS + TimeWrapper.deltaTime_SinceLastUpdate_NotTS - timeStarted) / duration;
                lerpCB(lerp);

                if (lerp >= 1f)
                    break;

                yield return null;
            }

            lerpCB(1f);
        }
    }
}