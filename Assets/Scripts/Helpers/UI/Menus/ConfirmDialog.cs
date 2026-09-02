// NS_REMOVE | Config
using ExperimentLibrary;

/// NS_REMOVE | Move this to <see cref="Helpers.UI.Menus.MenuManager"/>
using Helpers.Assets;

using System;
using TGP.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace Helpers.UI.Menus
{
    public class ConfirmDialog : MenuManager
    {
        private static bool EDITOR_ONLY_AUTO_RESPOND_TO_DIALOGUES = false;

        #region Static

        private const string RESOURCE_FILENAME = "Menus/ConfirmDialog";

        public static ConfirmDialog Instantiate()
        {
            return ResourceHelper.InstantiateResource<GameObject>(RESOURCE_FILENAME).GetComponent<ConfirmDialog>();
        }

        public static void ShowDialog(string text, Action<bool> result, bool onlyProceedViaClick)
        {
            ConfirmDialog dialog = GameObject.FindObjectOfType<ConfirmDialog>();
            if (dialog == null)
                dialog = Instantiate();
            dialog.Initialize();
            dialog.Show(text, result, onlyProceedViaClick);

#if UNITY_EDITOR
            if (EDITOR_ONLY_AUTO_RESPOND_TO_DIALOGUES)
            {
                Debug.Log("Auto-responded to dialogue");
                result(true);
                result = null;
            }
#endif
        }

        public static void HideDialog()
        {
            ConfirmDialog dialog = GameObject.FindObjectOfType<ConfirmDialog>();
            if (dialog == null) return;
            dialog.Hide();
        }

        #endregion

        public Transform container;
        public Button yesButton;
        public Button noButton;
        public TMPro.TextMeshProUGUI dialogText;

        private Action<bool> resultCB;
        private bool onlyProceedViaClick = false;

        public override void Initialize()
        {
            container.transform.localScale = Vector3.one * ExperimentLibraryManager.Config.UI.confirmationScaleAtZoom1 * ExperimentLibraryManager.Config.Experiment.stimulus.background.zoomFactor;

            base.Initialize();
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(yesButton.gameObject);
        }

        public void Show(string text, Action<bool> result, bool onlyProceedViaClick)
        {
            resultCB = result;
            dialogText.text = text;
            this.onlyProceedViaClick = onlyProceedViaClick;

            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(() =>
            {
                RaiseOnResult(true);
            });

            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(() =>
            {
                RaiseOnResult(false);
            });

            this.gameObject.SetActive(true);
        }

        protected override void OnEscapePressed()
        {
            base.OnEscapePressed();
            RaiseOnResult(false);
        }

        protected override void OnYesPressed()
        {
            base.OnYesPressed();

            if (onlyProceedViaClick)
            {
                this.LogWarning("Only proceeding via click");
                return;
            }

            RaiseOnResult(true);
        }

        protected override void OnNoPressed()
        {
            base.OnNoPressed();
            RaiseOnResult(false);
        }

        public void Hide()
        {
            Destroy(this.gameObject);
        }

        private void RaiseOnResult(bool result)
        {
            if (resultCB != null)
                resultCB(result);
        }
    }
}