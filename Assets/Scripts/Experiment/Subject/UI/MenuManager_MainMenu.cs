using TGP.Helpers;
using Helpers.UI.Menus;
using UnityEngine.UI;
using ExperimentLibrary;
using UnityEngine;

namespace Experiment.Subject.UI
{
    public class MenuManager_MainMenu : MenuManagerUI
    {
        public Callibration CallibrationCanvas;
        public ReactionSpeedMenu ReactionSPeedCanvas;
        public SubjectInputWindow SubjectWindow;

        public Button PlayScreen;
        public Text ScreenTitle;
        public Text ScreenDesc;

        public Button PlayPrep;
        public Text PrepTitle;
        public Text PrepDesc;

        public Button PlayFull;
        public Text FullTitle;
        public Text FullDesc;

        public Canvas buttonsCanvas;
        public Button screenCalibrationButton;
        public Button playerReactionButton;
        public Text versionText;
        
        public void ToggleSubjectWindow(bool on)
        {
            SubjectWindow.Toggle(on, on ? 10 : 0);
        }

        private void Start()
        {
            SetButtonsAvailable(true);

            CallibrationCanvas.Toggle(false, 0);
            ReactionSPeedCanvas.Toggle(false, 0);

            bool showScreening = ExperimentLibraryManager.Config.UI.showScreeningButton;
            bool showPreparation = ExperimentLibraryManager.Config.UI.showPreparationButton;

            PlayScreen.onClick.AddListener(() => { SetLevelSelection(PrepVsFull.Screen); });
            PlayPrep.onClick.AddListener(() => { SetLevelSelection(PrepVsFull.Prep); });
            PlayFull.onClick.AddListener(() => { SetLevelSelection(PrepVsFull.Full); });

            PlayScreen.gameObject.SetActive(showScreening);
            PlayPrep.gameObject.SetActive(showPreparation);

            ScreenTitle.text = ExperimentLibraryManager.Config.UI.behavioralScreeningTitle;
            ScreenDesc.text = ExperimentLibraryManager.Config.UI.screeningDesc;
            PrepTitle.text = ExperimentLibraryManager.Config.UI.inScannerPrepTitle;
            PrepDesc.text = ExperimentLibraryManager.Config.UI.prepDesc;
            FullTitle.text = ExperimentLibraryManager.Config.UI.fullGameTitle;
            FullDesc.text = ExperimentLibraryManager.Config.UI.fullDesc;

            screenCalibrationButton.interactable = ExperimentLibraryManager.Config.showCalibrationButton || Debug.isDebugBuild;
            screenCalibrationButton.onClick.AddListener(ScreenCalibrationButton_OnClick);

            playerReactionButton.onClick.AddListener(PlayerReactionButton_OnClick);
            buttonsCanvas.sortingOrder = 15;

            versionText.text = string.Format("v{0}", ExperimentManagerApplication.version);

            if (!showScreening && !showPreparation)
            {
                this.LogWarning("Screening & Preparation disabled - invoking Full Game");
                PlayFull.onClick?.Invoke();
            }
        }

        private void SetButtonsAvailable(bool interactable)
        {
            PlayPrep.interactable = interactable;
            PlayFull.interactable = interactable;
            screenCalibrationButton.interactable = interactable;
            playerReactionButton.interactable = interactable;
        }

        private void PlayerReactionButton_OnClick()
        {
            ReactionSPeedCanvas.Toggle(true, 20);
        }

        private void ScreenCalibrationButton_OnClick()
        {
            CallibrationCanvas.Toggle(true, 20); // over the subject!
        }

        protected override string GetReturnDialogText()
        {
            return ExperimentLibraryManager.Config.UI.quitConfirmation;
        }

        protected override void OnDialogResult(bool result)
        {
            base.OnDialogResult(result);
            if (result)
                RequestApplicationQuit();
        }

        public void SetLevelSelection(PrepVsFull prepVsFull)
        {
            Debug_Helper.Log(typeof(MenuManager_MainMenu), "LEVEL SELECTION :: {0}"._Format(prepVsFull));

            // SetButtonsAvailable(false);

            // Toggle the correct systems
            ExperimentManagerApplication.SetPrepVsFull(prepVsFull);
        }
    }
}