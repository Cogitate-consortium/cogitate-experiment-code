// NS_REMOVE | Config
using ExperimentLibrary;

using Helpers.Assets;
using System;
using TGP.Helpers;
using Helpers.UI.Menus;
using UnityEngine;
using UnityEngine.UI;
using Helpers.Engine;
using Experiment;

namespace Game.Managers.GameplayManager.UI
{
    /// <summary>
    /// [RENAME / SEGMENT] how is game popup a menu manager?
    /// </summary>
    public class Game_PopUp : MenuManager
    {
        #region Static

        private const string POP_UP_RESOURCE_FILENAME = "Menus/Game_PopUp";

        [SerializeField] private Image image = null;

        public static Game_PopUp ShowPopUp(RuntimeConfig popupConfig, Action callback, float timeout = -1)
        {
            return ShowPopUp(popupConfig.title, popupConfig.description, callback, timeout, popupConfig.minimumDurationMS);
        }

        public static Game_PopUp ShowPopUp(string title, Sprite sprite, Action callback, float timeout = -1, float minimumDurationMS = 0)
        {
            Game_PopUp popup = ShowPopUp(POP_UP_RESOURCE_FILENAME, title, "", callback, timeout, minimumDurationMS);
            popup.SetImage(sprite);
            return popup;
        }

        private static Game_PopUp Instantiate(string path)
        {
            return ResourceHelper.InstantiateResource<GameObject>(path).GetComponent<Game_PopUp>();
        }

        /// <summary>
        /// [SOS] Consider using <see cref="ShowPopUp(RuntimeConfig, Action, float)"/> instead, with a reference to the config file
        /// </summary>
        private static Game_PopUp ShowPopUp(string title, string description, Action callback, float timeout, float minimumDuration)
        {
            return ShowPopUp(POP_UP_RESOURCE_FILENAME, title, description, callback, timeout, minimumDuration);
        }

        private void SetImage(Sprite sprite)
        {
            if (!image)
            {
                Debug.Log("Image was null");
                return;
            }

            image.sprite = sprite;
            image.enabled = image.sprite != null;
        }

        private static Game_PopUp ShowPopUp(string prefabPath, string title, string description, Action callback, float timeout, float minimumDurationMS)
        {
            Game_PopUp popUp = Instantiate(prefabPath);
            popUp.Initialize();
            popUp.onProceed += (obj, args) =>
            {
                callback?.Invoke();
                popUp.Hide();
            };
            popUp.Show(title, description, timeout, minimumDurationMS);

            /*
            if (ExperimentManagerSession.EXPERIMENTER_AUTO_ANSWER)
            {
                Debug.LogError("[{0} ({1})] FAST FORWARD - Clicking OK"._Format(
                    TimeWrapper.currentFrameCycleID,
                    TimeWrapper.currentTimestampMS));

                callback?.Invoke();
                popUp.Hide();
            }
            */

            return popUp;
        }

        #endregion

        public event EventHandler<EventArgs> onProceed;
        bool onlyAllowProceedThroughMouse;
        bool allowClsoeViaEscape_PopUps;

        public Button confirmButton;
        public Transform container;
        public Text titleText;
        public Text descriptionText;
        public Text confirmText;

        private float timeout = -1;
        private double minimumTimeForHidingMS = -1;

        public override void Initialize()
        {
            // Scale it up!
            container.localScale = Vector3.one * ExperimentLibraryManager.Config.UI.popupScaleAtZoom1 * ExperimentLibraryManager.Config.Experiment.stimulus.background.zoomFactor;

            // HACK
            if (ExperimentManagerApplication.IS_IN_GAME)
            {
                RectTransform rT = container.GetComponent<RectTransform>();

                Vector2 anchors = Vector2.one / 2 + Utility_Helper.GetScreenOffset();
                rT.anchorMin = anchors;
                rT.anchorMax = anchors;
                rT.anchoredPosition = Vector2.zero;
                rT.sizeDelta = new Vector2(Screen.width, Screen.height);
            }

            allowClsoeViaEscape_PopUps = ExperimentLibraryManager.Config.Texts.allowClsoeViaEscape_PopUps;
            onlyAllowProceedThroughMouse = ExperimentLibraryManager.Config.Texts.onlyAllowProceedThroughMouse_PopUps;
            base.Initialize();
            SetImage(null);
            confirmButton.onClick.AddListener(OnConfirmClick);
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(confirmButton.gameObject);
        }

        public void Show(string title, string description, float timeout, float minimumDurationMS)
        {
            this.LogWarning("Showing Popup :: {0}, {1} ({2} sec)"._Format(title, description, timeout));
            confirmText.text = onlyAllowProceedThroughMouse ?
                ExperimentLibraryManager.Config.Texts.popupOkayText_mouseOnly :
                ExperimentLibraryManager.Config.Texts.popupOkayText;
            titleText.text = title;
            descriptionText.text = description;
            this.gameObject.SetActive(true);
            this.timeout = timeout;
            minimumTimeForHidingMS = TimeWrapper.currentTimestampMS + minimumDurationMS;
        }

        private void OnConfirmClick()
        {
            RaiseOnProceed();
        }

        protected override void OnEscapePressed()
        {
            if (!Check(true)) return;

            base.OnEscapePressed();
            RaiseOnProceed();
        }

        protected override void OnYesPressed()
        {
            if (!Check(false)) return;

            base.OnYesPressed();
            RaiseOnProceed();
        }

        private bool Check(bool isEscape)
        {
            if ((isEscape && !allowClsoeViaEscape_PopUps) ||
                (!isEscape && onlyAllowProceedThroughMouse))
            {
                this.LogWarning("Prevented key - mouses only!");
                return false;
            }

            if (TimeWrapper.currentTimestampMS < minimumTimeForHidingMS)
            {
                this.LogWarning("Not accepting keys yet ; wait until {0}ms"._Format(minimumTimeForHidingMS));
                return false;
            }

            return true;
        }

        public void Hide()
        {
            if (this == null)
                Debug_Helper.LogWarning(typeof(Game_PopUp), "Should not happen - Destroyed twice");
            else
            {
                this.LogWarning("HIDING");
                Destroy(gameObject);
            }
        }


        protected override void OnUpdate(float dT)
        {
            base.OnUpdate(dT);

            if (timeout > 0 && timeAlive > timeout)
                OnYesPressed();
        }

        private void RaiseOnProceed()
        {
            onProceed?.Invoke(this, new EventArgs());
        }

        [Serializable]
        public struct RuntimeConfig
        {
            public string title;
            public string description;
            public int minimumDurationMS;

            public RuntimeConfig(string title, string description, int minimumDurationMS = 0)
            {
                this.title = title;
                this.description = description;
                this.minimumDurationMS = minimumDurationMS;
            }
        }

    }
}