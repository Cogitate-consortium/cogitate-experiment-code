using System;
using TGP.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace Peripherals.EyeTracking
{
    /// <summary>
    /// [CHECK]
    /// </summary>
    public class EyeTracker_UI : MonoBehaviour
    {
        public EventHandler onEyeClicked = null;

        [Header("UI Examples")]
        [SerializeField] private Toggle ConnectionStatus = null;
        [SerializeField] private Button EyeButton = null;
        [SerializeField] private Image warningImage = null;
        [SerializeField] private Image simulatedImage = null;
        [SerializeField] private Text debugText = null;
        [SerializeField] private GameObject debugCross = null;
        /// <summary>
        /// [HACK] A button that does nothing, but is needed to avoid glitches with the callibration
        /// </summary>
        [SerializeField] private Button Harel = null;

        [SerializeField] private CanvasGroup cG_Master = null;

        public void Toggle(bool on)
        {
            cG_Master.Toggle(on);
        }

        private void Awake()
        {
            DeInitialize();
            EyeButton.onClick.RemoveAllListeners();
            EyeButton.onClick.AddListener(UI_OnEyeClicked);
        }

        public void Initialize()
        {
            Toggle(true);
        }

        public void DeInitialize()
        {
            Toggle(false);
        }

        private void UI_OnEyeClicked()
        {
            onEyeClicked?.Invoke(this, null);
        }

        /// <summary>
        /// [SOS, Since First Integration] Black magic, does nothing but is needed
        /// </summary>
        public void HarelSelect()
        {
            Harel?.Select();
        }

        public void SetDebugCrossPosition(Vector2? pos)
        {
            if (debugCross == null) return;
            if (!pos.HasValue) return;

            // Debug.Log(pos);
            debugCross.transform.position = pos.Value;
        }

        public void DisplayText(string msg, float time = 5)
        {
            CancelInvoke("ClearText");

            this.Log(msg);
            debugText.text = msg;

            Invoke("ClearText", time);
        }

        // [SOS] Not sure if invoke and privates play well
        void ClearText()
        {
            debugText.text = "";
        }

        public void ToggleDebugCross(bool on)
        {
            if (debugCross == null) return;

            debugCross.SetActive(on);
        }

        public void ToggleImage_Simulated(bool on)
        {
            simulatedImage.enabled = on;
        }
        public void ToggleImage_Connection(bool on)
        {
            ConnectionStatus.isOn = on;
        }
        public void ToggleImage_Warning(bool on)
        {
            warningImage.enabled = on;
        }
    }
}