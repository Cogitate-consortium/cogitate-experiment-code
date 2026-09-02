// NS_REMOVE
using ExperimentLibrary;
// NS_REMOVE | Needed just because Input is wired wrongly
using Helpers.Async;

// NS_DEBATABLE | Input to UI Directly?
using Peripherals.UserInput;

using Game.Core;
using System.Collections;
using System.Collections.Generic;
using TGP.Helpers;
using Helpers.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using Helpers.Engine;
using System;

namespace Game.UI
{
    public class InGameUI : MonoBehaviour
    {
        public PowerUpUI powerUpUI { get; private set; }
        public ScoreSystemUI scoreSystemUI { get; private set; }

        protected static Color COLOR_BLUE = "1CDCE2FF".ToColor();
        protected static Color COLOR_ORANGE = "FFD260FF".ToColor();

        [SerializeField] protected TMPro.TextMeshProUGUI performanceText = null;
        [SerializeField] protected TMPro.TextMeshProUGUI difficultyText = null;
        [SerializeField] protected CanvasGroup tipPanelContainer = null;
        [SerializeField] protected TMPro.TextMeshProUGUI tipPanelText = null;

        [SerializeField] protected CanvasGroup instructionsContainer = null;
        [SerializeField] protected TMPro.TextMeshProUGUI instructionsText = null;

        [Header("Lives")]
        [SerializeField] protected GameObject LivesParent = null;
        // DEPRECATE?
        [SerializeField] protected HeartObjectUI LivesPrefab = null;

        [Header("Timer")]
        [SerializeField] protected GameObject timerParent = null;
        [SerializeField] protected TMPro.TextMeshProUGUI timerText = null;

        [Header("SerialFMRI")]
        [SerializeField] private GameObject fmriPanel = null;
        [SerializeField] private List<Image> fmriTickReadyImages = null;

        protected CanvasGroup cg;
        protected List<HeartObjectUI> livesObjects = new List<HeartObjectUI>();

        [SerializeField] private CanvasGroup debugCG = null;

        private const float tipFadeInOutDuration = 0.5f;

        private float lastLives = 0;

        public bool isVisible { get; private set; }

        private Config config;

        public virtual void Initialize(Config config)
        {
            this.config = config;

            if (ExperimentLibraryManager.Config.UI.showGameplayTips)
                ShowTips(ExperimentLibraryManager.Config.UI.gameplayTipsDuration_MS / 1000f);

            ToggleTimer(ExperimentLibraryManager.Config.UI.showTimer);

            cg = this.gameObject.AddComponentIfNotExists<CanvasGroup>();
            InputManager.onKeyUp_TS += InputManager_OnKeyUp_TS;

            tipPanelContainer.alpha = 0f;

            debugCG.Toggle(ExperimentLibraryManager.Config.UI.showPerfAndDIffInRelease || Debug.isDebugBuild);// || Debug.isDebugBuild;

            instructionsContainer.Toggle(false);
            instructionsText.text = string.Format(
                "Use the right and left keys to move.\nUse '{0}' to report Yes and '{1}' to report No",
                ExperimentLibraryManager.Config.probeYes_String, ExperimentLibraryManager.Config.probeNo_String);
            ToggleTriggersPanel(false);
            powerUpUI = FindObjectOfType<PowerUpUI>();
            powerUpUI.Initialize(config.powerUp);
            scoreSystemUI = FindObjectOfType<ScoreSystemUI>();
            scoreSystemUI.Toggle(config.showProgressBar);
        }

        private void OnDestroy()
        {
            InputManager.onKeyUp_TS -= InputManager_OnKeyUp_TS;
        }

        private void InputManager_OnKeyUp_TS(object sender, InputManager.HighAccuracyEventArgs e)
        {
            // We don't need high accuracy on this
            AsyncThread.RunOnMainThread_ASAP_TS(() =>
            {
                if (e.key == ExperimentLibraryManager.Config.InputKeyCode.Debug_ToggleStats)
                    Toggle(!isVisible);
            });
        }

        private IEnumerator AnimateFadeInOut(float showDuration)
        {
            // Animate in - delay - out
            LeanTween.cancel(tipPanelContainer.gameObject);

            LeanTween.alphaCanvas(tipPanelContainer, 1.0f, tipFadeInOutDuration);
            yield return new WaitForSeconds(showDuration * 1.1f);
            HideTip();
        }

        private IEnumerator ShowHideTips(float showDuration)
        {
            instructionsContainer.Toggle(true);
            instructionsContainer.alpha = 0;

            // Fade in fast
            LeanTween.alphaCanvas(instructionsContainer, 1.0f, 0.5f);

            // Wait for duration
            yield return new WaitForSeconds(showDuration - 0.5f);

            // Fade out fast
            LeanTween.alphaCanvas(instructionsContainer, 0f, 0.5f);

            // Clear
            yield return new WaitForSeconds(showDuration + TimeWrapper.deltaTime_SinceLastUpdate_NotTS);
            instructionsContainer.Toggle(false);
        }

        #region Public Methods

        public void Toggle(bool isVisible)
        {
            this.isVisible = isVisible;
            cg.Toggle(isVisible);
        }

        public void SetPerformance(float performance01)
        {
            // Because of Updating constantly - "PercentileToPercent()" cause GC spike
            /*
            performance01 = Mathf.Clamp01(performance01);

            // [TODO] ?? I don't get why the float *100, then to string, then parse as int
            // (int) (difficulty01 * 100) + "%" would have been better no?
            // also I think + "%" is less efficient than string.Format("{0}%", )
            string perf = (performance01 * 100f).ToString("#");
            perf = (perf == "") ? "0" : perf;
            performanceText.text = int.Parse(perf) + "%";
            */

            performanceText.text = performance01.ToString("#.00");
        }

        public void SetDifficulty(float difficulty01)
        {
            // Because of Updating constantly - "PercentileToPercent()" cause GC spike
            difficulty01 = Mathf.Clamp01(difficulty01);

            // [TODO] ?? I don't get why the float *100, then to string, then parse as int
            // (int) (difficulty01 * 100) + "%" would have been better no?
            // also I think + "%" is less efficient than string.Format("{0}%", )
            string diff = (difficulty01 * 100f).ToString("#");
            diff = (diff == "") ? "0" : diff;
            difficultyText.text = int.Parse(diff) + "%";
        }

        public void ShowTips(float duration)
        {
            StartCoroutine(ShowHideTips(duration));
        }

        public void SetTime(float seconds)
        {
            // Make them better readable (>0)
            seconds++;

            if (seconds < 0)
                seconds = 0;
            timerText.text = string.Format("{0}:{1}", ((int)seconds / 60).ToString("00"), ((int)seconds % 60).ToString("00"));
        }

        /// <summary>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="duration">Set to -1 to auto-calculate</param>
        public void ShowTip(string text, float duration = -1)
        {
            if (this == null)
            {
                Debug_Helper.LogWarning(typeof(InGameUI), "InGame UI was null");
                return;
            }

            if (!ExperimentLibraryManager.Config.UI.showGeneralTips)
                return;

            tipPanelText.text = text;

            if (duration <= 0)
                duration = text.GetAverageReadingTime();

            StartCoroutine(AnimateFadeInOut(duration));
        }

        public void HideTip()
        {
            LeanTween.alphaCanvas(tipPanelContainer, 0f, tipFadeInOutDuration);
        }

        public virtual void ToggleProgressUI(bool isVisible)
        {
            ToggleLives(isVisible);
            ToggleTimer(isVisible);
        }

        public virtual void ToggleLives(bool isVisible)
        {
            LivesParent.SetActive(isVisible);
        }

        public virtual void ToggleTimer(bool isVisible)
        {
            timerParent.SetActive(isVisible);
        }

        public void SetColor(ObjectColorType colorType)
        {
            Color color = colorType == ObjectColorType.Blue ? COLOR_BLUE : COLOR_ORANGE;
            SetColor(livesObjects, color);
        }

        public virtual void SetMaxLives(float maxLives)
        {
            SetupLivesUI(livesObjects, LivesParent, LivesPrefab, maxLives);
        }

        protected static void SetupLivesUI(List<HeartObjectUI> livesObjects, GameObject livesParent, HeartObjectUI livesPrefab, float maxLives)
        {
            livesObjects.Clear();
            livesParent.transform.Genocide();

            for (int i = 0; i < maxLives; i++)
            {
                HeartObjectUI newLive = Instantiate(livesPrefab, livesParent.transform);
                livesObjects.Add(newLive);
            }
        }

        public void SetLives(float lives)
        {
            if (lives < 0)
            {
                //Debug.LogError("Trying to set lives < 0");
                lives = 0;
            }

            lastLives = lives;

            UpdateLives(lives);
        }

        // [TODO] this is repeated
        private void UpdateLives(float remainingLives)
        {
            int baseLives = Mathf.FloorToInt(remainingLives);
            int maxFillIdx = baseLives - 1;
            float extraBit = remainingLives - baseLives;

            for (int i = 0; i < livesObjects.Count; i++)
            {
                if (i <= maxFillIdx)
                    livesObjects[i].SetFillAmount(1f);
                else if (i == maxFillIdx + 1)
                    livesObjects[i].SetFillAmount(extraBit);
                else
                    livesObjects[i].SetFillAmount(0f);
            }

            if (remainingLives > 0 && remainingLives < livesObjects.Count)
            {
                livesObjects[(int)remainingLives].SetFillAmount(extraBit, true);
                if (remainingLives % 1 == 0 && (remainingLives - lastLives) > 0)
                    livesObjects[(int)remainingLives - 1].Scale();
            }

            lastLives = remainingLives;
        }

        protected static void SetColor(List<HeartObjectUI> livesObjects, Color color)
        {
            foreach (HeartObjectUI hOUI in livesObjects)
                hOUI.SetColor(color);
        }

        public void ToggleTriggersPanel(bool show)
        {
            // Debug.LogError("TOGGLE FMRI PANEL -> " + show);
            fmriPanel.SetActive(show);
        }

        public void ToggleFMRITicks(int ticks)
        {
            for (int i = 0; i < ticks; i++)
            {
                // Hide children image as gray overlay
                fmriTickReadyImages[i].transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        [Serializable]
        public class Config
        {
            public bool showProgressBar = false;
            public PowerUpUI.Config powerUp;
        }

        #endregion
    }
}