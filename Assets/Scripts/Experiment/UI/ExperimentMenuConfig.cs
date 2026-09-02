using System;
using UnityEngine;

namespace Experiment.UI
{
    [Serializable]
    public class ExperimentMenuConfig
    {
        public string prepFull_Comment = "These appear to the experimenter before loading the game";
        public string behavioralScreeningTitle = "BEHAVIORAL\nSCREENING";
        public string screeningDesc = "Play a screening round";
        public string inScannerPrepTitle = "IN-SCANNER\nPREPARATION";
        public string prepDesc = "Play a short prep round";
        public string fullGameTitle = "IN-SCANNER\nFULL GAME";
        public string fullDesc = "Play the entire experiment";
        public bool showScreeningButton = true;
        public bool showPreparationButton = false;

        public bool Get_AllPopups_OnlyProceedViaClick(ModuleType module)
        {
            return module.ToPrepVsFull() == PrepVsFull.Screen ?
                allPopups_onlyProceedViaClick_Behavioral :
                allPopups_onlyProceedViaClick_Experimental;
        }

        [SerializeField] private bool allPopups_onlyProceedViaClick_Behavioral = false;
        [SerializeField] private bool allPopups_onlyProceedViaClick_Experimental = true;

        public bool Get_DialogueYes_OnlyProceedViaClick(ModuleType module)
        {
            return module.ToPrepVsFull() == PrepVsFull.Screen ?
                dialogueYes_onlyProceedViaClick_Behavioral :
                dialogueYes_onlyProceedViaClick_Experimental;
        }

        [SerializeField] private bool dialogueYes_onlyProceedViaClick_Behavioral = true;
        [SerializeField] private bool dialogueYes_onlyProceedViaClick_Experimental = true;

        public float popUpTimeout_LevelComplete_FMRI = 5f;
        public float popUpTimeout_LevelComplete_NONFMRI = 300f;

        // Show bottom left tips
        public bool showGeneralTips = true;
        // Show intruction tip botton right
        public string inGameMenuQuitConfirmation = "Would you like to quit level and return to level selection?";
        public string quitConfirmation = "Would you like to exit the game?";
        public string showTimerComment = "Never = 0, Constantly = 1, AtTheStartOnly =2";
        public bool showTimer = false;
        public bool showGameplayTips = false;
        public float gameplayTipsDuration_MS = 5000f;
        public bool inGameShowExitButton = false;
        public float popupScaleAtZoom1 = 0.69f;
        public float confirmationScaleAtZoom1 = 0.50f;
        public bool showPerfAndDIffInRelease = false;
        public bool showMuteDuringLevel = true;
        public bool showEyeTrackingDuringLevel = true;
    }
}