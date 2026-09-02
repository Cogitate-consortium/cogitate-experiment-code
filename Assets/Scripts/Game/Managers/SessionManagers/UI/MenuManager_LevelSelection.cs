// NS_REMOVE | Config, TheManager.Reset / 
using ExperimentLibrary;

// NS_DEBATABLE  | Gets unlock info from this
using Game.Systems.Replay;

// NS_DEBATABLE | Too direct?
using Game.Core;
/// NS_DEBATABLE | Too direct? Maybe ask the <see cref="Game.Managers.LevelManagers.LevelMasterManager"/>
using Game.Systems.Misc;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;
using UnityEngine.UI;
using Helpers.UI.Core;
using Experiment;
using Helpers.UI.Menus;
using Experiment.Managers;

namespace Game.Managers.SessionManagers.UI
{
    public class MenuManager_LevelSelection : MenuManagerUI
    {
        // [200721] TEMP
        bool onlyAllowProceedThroughMouse;

        bool allowReplayOfCompleted;

        public event EventHandler<EventArgs<int>> onGameStartRequested;
        public event EventHandler<EventArgs<int>> onMenuExitRequested;

        // Populate levels dynamically
        public LevelSelectionItem LevelSelectionItemPrefab;
        public List<GameObject> worldGridParents = new List<GameObject>();
        public GameObject localizerGridParent;
        public List<LevelSelectionItem> levelSelectionItems = new List<LevelSelectionItem>();

        // From Editor
        public GameObject tutorialSelectionParent;
        public List<LevelSelectionItem> tutorialSelectionItems = new List<LevelSelectionItem>();

        public LeaderboardUI leaderboard;

        public Button resetButton;
        public Button BackButton;

        public Text applicationTitleText;
        public Text applicationSubtitleText;
        public Text proceedInstructionsText;
        public TMPro.TextMeshProUGUI totalScoreText;
        public TMPro.TextMeshProUGUI totalStarsText;
        public CanvasGroup moneyCG;
        public TMPro.TextMeshProUGUI totalMoneyText;

        private LevelSelectionItem focusedLevel;
        private bool isLeaderboardActive = false;

        private void CreateLevelMenuItems()
        {
            levelSelectionItems.Clear();
            levelSelectionItems.AddRange(tutorialSelectionItems);

            // If we're doing FMRI Scanner skip tutorials
            tutorialSelectionParent.SetActive(true);// TheManager.system != ModuleType.FMRI_Scanner);

            // Assigned from Editor - listen only to event
            for (int i = 0; i < tutorialSelectionItems.Count; i++)
            {
                if ((ExperimentManagerSession.module == ModuleType.FMRI_Scanner && !ExperimentLibraryManager.Config.FMRI.Scanner_GetShowTutorial(i)) ||
                    (ExperimentManagerSession.module == ModuleType.MEEG && !ExperimentLibraryManager.Config.MEEG.FullVersion_GetShowTutorial(i)))
                {
                    tutorialSelectionItems[i].Toggle(false);
                    continue;
                }

                tutorialSelectionItems[i].Toggle(true);
                tutorialSelectionItems[i].onLevelSelected += LevelSelection_onLevelSelected;
            }

            // Create Levels
            for (int i = 0; i < PlayerProgression.numGameWorlds; i++)
            {
                worldGridParents[i * 2].transform.Genocide();
                worldGridParents[i * 2 + 1].transform.Genocide();
                for (int j = 0; j < PlayerProgression.numLevelsPerWorld; j++)
                {
                    // 1 Based
                    int levelID = i * PlayerProgression.numLevelsPerWorld + j + 1;

                    // 0 Based
                    int halfLevelsPerWorld = PlayerProgression.numLevelsPerWorld / 2 + PlayerProgression.numLevelsPerWorld % 2;
                    int subGridIndex = j / halfLevelsPerWorld;
                    int worldGridParentIndex = i * 2 + subGridIndex;

                    LevelSelectionItem levelItem = Instantiate(LevelSelectionItemPrefab, worldGridParents[worldGridParentIndex].transform);
                    levelItem.Initialize(levelID, false, GetPreventUnlock(levelID));
                    levelItem.SetLevelText((levelID).ToString());
                    levelItem.onLevelSelected += LevelSelection_onLevelSelected;

                    levelSelectionItems.Add(levelItem);
                }
            }

            // Create Localizers
            localizerGridParent.transform.Genocide();
            for (int i = 0; i < PlayerProgression.numLocalizers; i++)
            {
                int levelID = LevelConfig.localizerOffset + i;
                LevelSelectionItem levelItem = Instantiate(LevelSelectionItemPrefab, localizerGridParent.transform);
                levelItem.Initialize(levelID, false, GetPreventUnlock(levelID));
                levelItem.SetLevelText(LevelsLibrary.GetLocalizerNameUI(i));
                levelItem.HideArrow();
                levelItem.onLevelSelected += LevelSelection_onLevelSelected;

                levelSelectionItems.Add(levelItem);
            }

            // Hide worlds depending on NumWorlds
            for (int i = 0; i < worldGridParents.Count; i++)
            {
                // Hide parent of Grids
                worldGridParents[i].transform.parent.gameObject.SetActive(i < PlayerProgression.numGameWorlds * 2);
            }
        }
        /*
        private IEnumerator ShowLeaderboardDelay(float delay)
        {
    #if UNITY_EDITOR
            if (TheManager.EDITOR_ONLY_FAST_FORWARD)
            {
                // Jump from 0-20 game levels to Localizer 100+
                if (PlayerProgression.ActiveLevel.worldID == 3 && PlayerProgression.ActiveLevel.isLastOfWorld)
                    RaiseOnLevelSelected(100);
                else
                    RaiseOnLevelSelected(PlayerProgression.ActiveLevel.levelID + 1);

                yield break;
            }
    #endif
            leaderboard.Toggle(true);
            isLeaderboardActive = true;
        }
        */

        public override void Initialize()
        {
            base.Initialize();
            onlyAllowProceedThroughMouse = ExperimentLibraryManager.Config.Texts.onlyAllowProceedThroughMouse_LevelSelection;
            allowReplayOfCompleted = ExperimentLibraryManager.Config.allowReplayOfCompleted;

            ExperimentManagerApplication.onLevelStartLockToggled += TheManager_onLevelStartLockToggled;

            ToggleMoney(ExperimentLibraryManager.Config.Money.showMoney);
            ToggleReturnButton(!FadeSceneController.isActive);
        }

        public override void DeInitialize()
        {
            base.DeInitialize();

            ExperimentManagerApplication.onLevelStartLockToggled -= TheManager_onLevelStartLockToggled;
        }

        private void ToggleMoney(bool useMoney)
        {
            moneyCG.Toggle(useMoney);
        }

        private void TheManager_onLevelStartLockToggled(object sender, EventArgs e)
        {
            UpdateProceedInstructionsValue();
        }

        private IEnumerator ShowLeaderboardDelay(float delay, Action callback)
        {
            yield return new WaitForSeconds(delay);

            leaderboard.Toggle(true);
            isLeaderboardActive = true;

            callback?.Invoke();
        }

        public void StartNextLevel()
        {
            leaderboard.Toggle(false);
            UncheckLeaderboard();

            // Jump from 0-20 game levels to Localizer 100+
            bool isLastWorldOfGame = PlayerProgression.IsLastWorldOfGame(PlayerProgression.ActiveLevel.worldID);

            // Jump from 0-20 game levels to Localizer 100+
            if (isLastWorldOfGame && PlayerProgression.IsLastLevelOfWorld(PlayerProgression.ActiveLevel))
            {
                if (PlayerProgression.numLocalizers > 0)
                    RaiseOnLevelSelected(100);
                else
                    this.LogWarning("No localizers to play. Done.");
            }
            else
                RaiseOnLevelSelected(PlayerProgression.ActiveLevel.levelID_1Based + 1);

        }

        private void UpdateProceedInstructionsValue()
        {
            proceedInstructionsText.text =
                ExperimentManagerApplication.IS_LEVELSTART_LOCKED ? ExperimentLibraryManager.Config.Texts.menuProceedInstructions_LOCKED :
                onlyAllowProceedThroughMouse ? ExperimentLibraryManager.Config.Texts.menuProceedInstructions_mouseOnly :
                ExperimentLibraryManager.Config.Texts.menuProceedInstructions._Format(ExperimentLibraryManager.Config.menuOK_String);
        }

        private void UpdateUIValues()
        {
            UpdateProceedInstructionsValue();

            applicationTitleText.text = ExperimentLibraryManager.Config.ApplicationTitle;
            applicationSubtitleText.text = ExperimentLibraryManager.Config.ApplicationSubTitle;
            totalScoreText.text = PlayerProgression.GetTotalPoints().ToString("0");
            totalStarsText.text = PlayerProgression.GetTotalStars().ToString("0");
            totalMoneyText.text = ExperimentManagerSession.GetTotalDollars().ToString(ExperimentLibraryManager.Config.Money.format);
        }

        private void UpdateLevels()
        {
            for (int i = 0; i < levelSelectionItems.Count; i++)
            {
                LevelSelectionItem levelSelectionItem_Current = levelSelectionItems[i];
                int levelId = levelSelectionItem_Current.LevelId;
                ProgressionLevel progressionLevel = PlayerProgression.GetLevel(levelId);
                bool isLocalizer = progressionLevel?.Level?.isLocalizer == true;
                bool isLevelCompleted = (progressionLevel != null);
                bool shouldUnlock = false;

                if (i == 0)
                {
                    isLevelCompleted = true;
                    shouldUnlock = true;
                    focusedLevel = levelSelectionItem_Current;
                }
                else if (i > 0)
                {
                    bool isPreviousLevelCompleted = PlayerProgression.HasCompletedGameLevel(levelSelectionItems[i - 1].LevelId);

                    if (isPreviousLevelCompleted && !isLevelCompleted)
                        focusedLevel = levelSelectionItem_Current;

                    shouldUnlock = (isPreviousLevelCompleted || isLevelCompleted);
                }

                int numStars =
                    levelId == -1 ? -1 :    // For Instructions, dont show stars
                    progressionLevel == null ? -1 : // If we haven't completed yet, don't show stars
                    progressionLevel.GetStars();    // Show stars

                levelSelectionItem_Current.TrySetup(shouldUnlock, isLocalizer ? -1 : numStars);
            }

            focusedLevel.SetFocus();
        }

        private void Start()
        {
            BackButton.gameObject.SetActive(Debug.isDebugBuild ||
                ExperimentManagerSession.module.ToPrepVsFull() != PrepVsFull.Full || // Always close for full versions
                (ExperimentManagerSession.module == ModuleType.FMRI_Scanner && PlayerProgression.CompleteLevels.Count == 1) || // Fmri SCANNER auto-completes the practice level to unlock level 1
                PlayerProgression.CompleteLevels.Count == 0);       // Close the moment we complete one level otherwise

            BackButton.onClick.AddListener(OnBackButtonPressed);

            // 200619 TODO HACK Maybe HasCompletedGame() instead?
            resetButton.gameObject.SetActive(PlayerProgression.HasCompletedGameLevel(LevelsLibrary.config.levels.Last().levelID_1Based));
            resetButton.onClick.AddListener(ResetButton_OnClick);

            CreateLevelMenuItems();

            leaderboard.onHide += Leaderboard_onHide;

            UpdateUIValues();
            UpdateLevels();
            /*
            Action checkStartNextLevel = () =>
            {
                bool shouldFastForwardToNextLevel = ExperimentManagerApplication.autoForwardToNextLevel;

                if (ExperimentManagerSession.EXPERIMENTER_AUTO_ANSWER)
                {
                    Debug.LogError("[{0} ({1})] FAST FORWARD - Starting Level"._Format(
                       TimeWrapper.currentFrameCycleID,
                       TimeWrapper.currentTimestampMS));

                    shouldFastForwardToNextLevel = true;
                }

                if (shouldFastForwardToNextLevel)
                {
                    Debug.LogWarning("Auto-started next level");

                    StartNextLevel();
                }
            };
            */

            // Leave this in for testing it in fast forward
            Utility_Helper.WaitUntil(() => !ExperimentManagerSession.isWriting && !ExperimentManagerApplication.IS_LEVELSTART_LOCKED, a =>
            {
                // Check if we exit the game - in this case we haven't completed the level
                if (PlayerProgression.ActiveLevel != null && PlayerProgression.GetLevel(PlayerProgression.ActiveLevel.levelID_1Based) != null)
                {
                    LevelConfig lastCompleted = PlayerProgression.CompleteLevels.Last().Level;
                    if (!PlayerProgression.ActiveLevel.isLocalizer && !lastCompleted.isTutorial)
                    {
                        leaderboard.DisplayBoard();
                        StartCoroutine(ShowLeaderboardDelay(0.5f, null));// checkStartNextLevel));
                    }
                    else
                    { }// checkStartNextLevel();
                }
            });
        }


        private void OnBackButtonPressed()
        {
            onMenuExitRequested?.Invoke(this, null);
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



        protected override void OnYesPressed()
        {
            if (isShowingDialog)
            {
                this.LogWarning("Showing dialogue ; aborting");
                return;
            }

            if (onlyAllowProceedThroughMouse)
            {
                this.LogWarning("Only allowing proceed through mouse ; aborting");
                return;
            }

#if UNITY_EDITOR
            /*
            if (TheManager.EDITOR_ONLY_FAST_FORWARD)
            {
                Debug.LogWarning("Will start the level from elsewhere");
                return;
            }
            */
#endif
            base.OnYesPressed();
            if (focusedLevel != null)
                RaiseOnLevelSelected(focusedLevel.LevelId);
        }

        #region Events

        private void ResetButton_OnClick()
        {
            resetButton.gameObject.SetActive(false);

            OnBackButtonPressed();
        }

        private void LevelSelection_onLevelSelected(object sender, EventArgs<int> e)
        {
            RaiseOnLevelSelected(e);
        }

        private void RaiseOnLevelSelected(int gamelevelId_1Based)
        {
            if (ExperimentManagerSession.isWriting)
            {
                this.LogWarning("Subject Performance Report is Writing. Wait!");
                return;
            }
            if (isLeaderboardActive)
            {
                this.LogWarning("Leaderboard is Active. Wait!");
                return;
            }
            if (ExperimentManagerApplication.IS_LEVELSTART_LOCKED)
            {
                this.LogWarning("LEVELSTART IS LOCKED. Wait!");
                return;
            }

            if (gamelevelId_1Based != LevelConfig.tutorial_I_ID_1Based &&
                PlayerProgression.HasCompletedGameLevel(gamelevelId_1Based))
            {
                if (!allowReplayOfCompleted)
                {
                    this.LogWarning("Not allowing replay of completed levels!");
                    return;
                }
                else
                    this.LogWarning("Replaying completed level");
            }

            onGameStartRequested?.Invoke(this, new EventArgs<int>(gamelevelId_1Based));
        }

        private void Leaderboard_onHide(object sender, EventArgs e)
        {
            Invoke("UncheckLeaderboard", 1f);
        }

        // Don't immediately uncheck leaderboard, because will disable it and start the level immediately
        private void UncheckLeaderboard()
        {
            isLeaderboardActive = false;
        }

        #endregion

        #region Helper

        public bool GetPreventUnlock(int levelID)
        {
            LevelConfig level = LevelsLibrary.GetLevel(levelID);

            // For tutorials, we're all good
            if (level.isTutorial) return false;

            // For localizers, prevent unlock if we dont have a replay
            if (level.isLocalizer && !ReplaySystem.HasReplay()) return true;

            // for FMRI prevent unlock for 2nd halves of runs
            if (ExperimentManagerSession.isFMRI && !ExperimentManagerSession.FMRI_IsLevelFirstHalfOfRun(levelID) &&
                // Unless thess the first half is unlocked!
                !PlayerProgression.HasCompletedGameLevel(levelID - 1)) return true;

            return false;
        }
        #endregion
    }
}