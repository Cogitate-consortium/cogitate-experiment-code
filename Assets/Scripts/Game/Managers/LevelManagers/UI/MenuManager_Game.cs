// NS_REMOVE | Single Variable
using ExperimentLibrary;

using System;
using Helpers.UI.Menus;
using UnityEngine;
using Game.Managers.GameplayManager.UI;
using Experiment.UI;

namespace Game.Managers.LevelManagers.UI
{
    public class MenuManager_Game : MenuManagerUI
    {
        private bool showing = false;

        private static ExperimentMenuConfig config { get { return ExperimentLibraryManager.Config.UI; } }

        private Game_PopUp instructionPopUp = null;

        // public EventHandler onGameRestartRequested;
        public EventHandler onGamePauseRequested;
        public EventHandler onGameResumeRequested;
        public EventHandler onGamePauseToggleRequested;
        public EventHandler onGameExitRequested;

        public override void Initialize()
        {
            base.Initialize();

            ToggleReturnButton(config.inGameShowExitButton);
        }

        protected override string GetReturnDialogText()
        {
            return ExperimentLibraryManager.Config.UI.inGameMenuQuitConfirmation;
        }

        protected override void DebugInput()
        {
            base.DebugInput();

            /*
            // if (Input.GetKeyUp(KeyCode.Alpha1))
            //    RequestRestartGame();

            if (Input.GetKeyUp(KeyCode.Alpha2))
                RequestPauseGame();

            if (Input.GetKeyUp(KeyCode.Alpha3))
                RequestResumeGame();

            if (Input.GetKeyUp(KeyCode.Alpha4))
                RequestExitGame();
            */
        }
        
        protected override void HandleInput(KeyCode keyCode)
        {
            base.HandleInput(keyCode);

            if (keyCode == ExperimentLibraryManager.Config.InputKeyCode.Game_Pause)
                RequestTogglePause();
        }

        protected override void OnYesPressed()
        {
            base.OnYesPressed();

            if (showing)
                OnDialogResult(true);
        }

        protected override void OnNoPressed()
        {
            base.OnNoPressed();

            if (showing)
                OnDialogResult(false);
        }

        protected override void OnDialogResult(bool result)
        {
            // Debug.Log("OnDialogResult " + result, this);

            // Cursor.visible = false;
            base.OnDialogResult(result);
            if (result == true)
            {
                RequestExitGame();
            }
            else
            {
                // Debug.Log("MMG A :: " + TimeWrapper.currentTimestampMS);
                RequestResumeGame();
            }
        }

        protected override void OnDialogShow()
        {
            base.OnDialogShow();
            RequestPauseGame();
        }

        private void InstructionSystem_OnProceed(object sender, EventArgs e)
        {
            instructionPopUp.Hide();
            RequestResumeGame();
        }

        /*
        private void RequestRestartGame()
        {
            if (onGameRestartRequested != null)
                onGameRestartRequested(this, null);
        }
        */

        // For experimenters only
        private void RequestTogglePause()
        {
            onGamePauseToggleRequested?.Invoke(this, null);
        }

        private void RequestPauseGame()
        {
            onGamePauseRequested?.Invoke(this, null);
        }

        private void RequestResumeGame()
        {
            onGameResumeRequested?.Invoke(this, null);
        }

        private void RequestExitGame()
        {
            onGameExitRequested?.Invoke(this, null);
        }
    }
}