using Experiment.Managers;
using ExperimentLibrary;
using System;
using TGP.Helpers;
using UnityEngine.UI;

namespace Helpers.UI.Menus
{
    public class MenuManagerUI : MenuManager
    {
        public event EventHandler<EventArgs> onApplicationQuitRequested;
        public Button returnButton = null;

        public bool isShowingDialog { get; private set; }
        private bool onlyProceedViaClick;
        public bool preventReturnDialogue = false;

        public override void Initialize()
        {
            base.Initialize();
            returnButton.onClick.AddListener(ReturnButton_OnClick);

            onlyProceedViaClick = ExperimentLibraryManager.Config.UI.Get_DialogueYes_OnlyProceedViaClick(ExperimentManagerSession.module);
        }

        public void ToggleReturnButton(bool show)
        {
            returnButton.gameObject.SetActive(show);
        }

        private void ReturnButton_OnClick()
        {
            ShowReturnDialog();
        }

        protected virtual string GetReturnDialogText() { return ""; }

        public void ShowReturnDialog()
        {
            if (preventReturnDialogue)
            {
                this.LogWarning("Prevented Return Dialogue!");
                return;
            }

            // Cursor.visible = true;
            isShowingDialog = true;
            OnDialogShow();

            ConfirmDialog.ShowDialog(GetReturnDialogText(), (result) =>
            {
                OnDialogResult(result);
            }, onlyProceedViaClick);
        }

        protected override void OnEscapePressed()
        {
            base.OnEscapePressed();
            if (!isShowingDialog)
                ShowReturnDialog();
            else
                OnDialogResult(false);
        }

        protected virtual void OnDialogResult(bool result)
        {
            isShowingDialog = false;
            ConfirmDialog.HideDialog();
        }

        protected void RequestApplicationQuit()
        {
            onApplicationQuitRequested?.Invoke(this, new EventArgs());
        }

        protected virtual void OnDialogShow() { }
    }
}