// NS_REMOVE
using Experiment.Library.Core;
// NS_REMOVE
using Experiment.Triggers.Core;

// NS_DEBATABLE | Isn't UI a bit too far? Exp / Game / Periph is ok
using Helpers.UI.Menus;
// NS_DEBATABLE | Isn't UI a bit too far? Exp / Game / Periph is ok
using Experiment.Task.UI;
// NS_DEBATABLE | Isn't UI a bit too far? Exp / Game / Periph is ok
using Game.Managers.GameplayManager.UI;
// NS_DEBATABLE | Isn't UI a bit too far? Exp / Game / Periph is ok
using Game.Managers.LevelManagers.UI;
// NS_DEBATABLE | Isn't UI a bit too far? Exp / Game / Periph is ok
using Game.Managers.SessionManagers.UI;
// NS_DEBATABLE | Isn't UI a bit too far? Exp / Game / Periph is ok
using Experiment.Subject.UI;

// NS_DEBATABLE | Isn't Helpers a bit too far? Exp / Game / Periph | Helpers maybe should self-manage a bit?
using Helpers.Assets;

// NS_DEBATABLE | Needs segmentation, then maybe it will make sense (ie. separate for EXP / GAME / PERIPH)
using Game.Systems.Misc;
// NS_DEBATABLE | Init, DeInit, Debug, 
using Game.Managers.SessionManagers;
// NS_DEBATABLE
using Game.Managers.LevelManagers;

// NS_DEBATABLE | Needs segmentation, then maybe it will make sense (ie. separate for EXP / GAME / PERIPH)
using Experiment.Triggers;
// NS_DEBATABLE | Needs segmentation, then maybe it will make sense (ie. separate for EXP / GAME / PERIPH)
using Experiment.Stimulus;

// NS_DEBATABLE | Needs segmentation, then maybe it will make sense (ie. separate for EXP / GAME / PERIPH)
using Peripherals.Audio;
// NS_DEBATABLE | Needs segmentation, then maybe it will make sense (ie. separate for EXP / GAME / PERIPH)
using Peripherals.UserInput;
// NS_DEBATABLE
using Peripherals.EyeTracking;

using System;
using System.Collections;
using TGP.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Helpers.Engine;
using Game.Systems.Bridges;
using Experiment.Task;
using Game.Systems.Replay;
using Game.Managers.GameplayManager;
using System.Collections.Generic;
using Game.Systems.Adaptive;
using Game.Systems.Score;
using Game.Entities.Player;
using Game.Entities.Interactables;
using Peripherals.Koreo;
using Game.Entities.Player.Core;
using Game.Core;// NS_SEGMENT | Levels
using Peripherals.Logging;
using ExperimentLibrary;
using Experiment.Managers;
using Experiment.Subject;
using Experiment.Helpers;
using Helpers.Async;
using Peripherals.Logging.Core;
using ExperimentLibrary.Test;
using Peripherals.EyeTracking.EyeLink;
using Peripherals.EyeTracking.Tobii;
using Experiment.Background;

/// <summary>
/// -> Move to Session?
/// </summary>
namespace Experiment
{
    /// <summary>
    /// SEGMENT UI?
    /// <see cref="PostLevelCleanUp(LevelCompleteArgs)"/> and <see cref="ShowLevelCompletePopup(LevelCompleteArgs, Action, float)"/> are a mess
    /// <see cref="module"/> should be moved to Experiment manager ? Or maybe this can become MODULE Manager?
    /// </summary>
    public class ExperimentManagerApplication : Singleton<ExperimentManagerApplication>
    {
        public static readonly string version = "[201130-pilot;v1.0;hf2]";

        public bool EDITOR_ONLY_DO_SINGLE_TRIGGER_SEQUENCE_GENERATOR = false;
        private static SubjectInfo currentSubject;
        private static string overrideFolderPathToLoadFrom;

        // Does not make sesne to have outgoing events
        public static EventHandler onLevelStartLockToggled;
        private static void RaiseOnLevelStartLockToggled(bool on)
        {
            IS_LEVELSTART_LOCKED_BY_EXPERIMENTER = on;
            onLevelStartLockToggled?.Invoke(null, null);
        }

        #region Post Level Clean up

        private static LevelCompleteArgs LAST_LEVEL_COMPLETE_ARGS = null;

        // Called After Player Progression is all done
        public static void ExperimentManagerSession_onLevelComplete(LevelCompleteArgs e)
        {
            LAST_LEVEL_COMPLETE_ARGS = e;
            instance?._PostLevelCleanUp(e);
        }

        // Called After Player Progression is all done
        private void _PostLevelCleanUp(LevelCompleteArgs e)
        {
            bool fmriShowBlackScreen;
            float levelCompleteTimeout = isFMRI ?
                ExperimentLibraryManager.Config.UI.popUpTimeout_LevelComplete_FMRI :
                ExperimentLibraryManager.Config.UI.popUpTimeout_LevelComplete_NONFMRI;

            bool keepMusicPlaying = isFMRI &&
                ExperimentLibraryManager.Config.FMRI.fmriPlayAudioDuringBetweenLevels &&
                e.endReason != EndReason.Exit; // Don't persist for exits

            experimentSessionManager.OnLevelComplete(e, keepMusicPlaying);
            ExperimentManagerSession.onProbesComplete -= ExperimentSessionManager_onProbesComplete;

            if (e.endReason == EndReason.Exit)
                fmriShowBlackScreen = false;
            else
                fmriShowBlackScreen = e.levelConfig.isTutorial ? false : true;

            // SPR_EXPE.StopBackgroundDataWriting();
            // if (isFMRI) onTRReceived -= FMRITrigger_onTRReceived; // Should not be needed 200620

            if (menuManager != null)
                menuManager.DeInitialize();

            Action CleanUp_FinalCB = () =>
            {
                StartCoroutine(CleanUp_FinalIE());
            };

            float cameraOrthoSize = Camera.main.orthographicSize;

            Action CleanUpFirstHalf = () =>
            {
                this.LogWarning("Cleaning up FIRST half of run, PREVENTING LEVEL START");

                // Prevent level-start!
                IS_LEVELSTART_LOCKED_BY_FMRI = true;

                CleanUp_FinalCB();
            };

            Action LoadSecondHalf = () =>
            {
                MenuManager_LevelSelection mmLobby = menuManager as MenuManager_LevelSelection;
                if (mmLobby == null)
                {
                    if (!EXPERIMENTER_AUTO_ANSWER)
                        this.LogError("MenuManager not in LevelSelection mode?? Shouldnt Happen");
                }
                else
                {
                    // Allow level start again
                    IS_LEVELSTART_LOCKED_BY_FMRI = false;

                    this.LogWarning("Auto - forwarding to SECOND(no menu), ALLOWING LEVEL START");
                    mmLobby.StartNextLevel();

                    // This will also start the music
                    PauseExperiment(true, false, keepMusicPlaying);
                }
            };

            if (isFMRI && fmriShowBlackScreen)
            {
                int runID = ExperimentManagerSession.FMRI_GetRunID(PlayerProgression.ActiveLevel);

                // We want to auto forward for the 2nd half of each run!
                // So if we just finished the FIRST half of the run (not the second)
                if (ExperimentManagerSession.FMRI_IsLevelFirstHalfOfRun(PlayerProgression.ActiveLevel))
                {
                    Game_PopUp fmriBetweenReplayLevelPopup = null;
                    StimulusType? localizerTarget = null;
                    if (e.levelConfig.isLocalizer)
                        localizerTarget = StimulusManager.localizerInfos[e.levelConfig.localizerID_0Based + 1].target;
                    bool showInstructions = localizerTarget.HasValue;

                    SoundSystem.TogglePersistentMusic(keepMusicPlaying);

                    AsyncThread.RequestRunOnNewThread_OnNextFrame(() =>
                    {
                        experimentSessionManager.LogFMRIDump("{0};{1};1A;FADE_OUT_BEGIN"._Format(
                            TimeWrapper.GetLastFrameTimestamp_TS().ToString(), runID));
                    });

                    FadeSceneController.FadeInOut(
                        // After we're done fading in, go to level selection (should start writing)
                        () =>
                        {
                            experimentSessionManager.LogFMRIDump("{0};{1};1B;FADE_OUT_DONE"._Format(
                                TimeWrapper.GetLastFrameTimestamp_TS().ToString(), runID));

                            CleanUpFirstHalf();
                        },
                        // Show instructions
                        () =>
                        {
                            if (showInstructions)
                            {
                                fmriBetweenReplayLevelPopup = ShowInstructionsFmriReplayBetweenLevels(localizerTarget.Value);
                                AsyncThread.RequestRunOnNewThread_OnNextFrame(() =>
                                {
                                    experimentSessionManager.LogFMRIDump("{0};{1};1Bi;SHOW_INSTRUCTIONS"._Format(
                                        TimeWrapper.GetLastFrameTimestamp_TS().ToString(), runID));
                                });
                            }
                        },
                        // Hide instructions
                        () =>
                        {
                            if (fmriBetweenReplayLevelPopup)
                            {
                                fmriBetweenReplayLevelPopup.Hide();

                                AsyncThread.RequestRunOnNewThread_OnNextFrame(() =>
                                {
                                    experimentSessionManager.LogFMRIDump("{0};{1};1Bii;HIDE_INSTRUCTIONS"._Format(
                                        TimeWrapper.GetLastFrameTimestamp_TS().ToString(), runID));
                                });
                            }
                        },
                        // Once we're done with the waiting, start the next level
                        () =>
                        {
                        },
                        // Nothing for Post-Waiting
                        () =>
                        {
                            LoadSecondHalf();

                            AsyncThread.RequestRunOnNewThread_OnNextFrame(() =>
                            {
                                experimentSessionManager.LogFMRIDump("{0};{1};1C;FADE_IN_BEGIN"._Format(
                                    TimeWrapper.GetLastFrameTimestamp_TS().ToString(), runID));
                            });
                        },
                        // When we've faded back in, start the game
                        () =>
                        {
                            experimentSessionManager.LogFMRIDump("{0};{1};1D;FADE_IN_DONE"._Format(
                                TimeWrapper.GetLastFrameTimestamp_TS().ToString(), runID));

                            // TODO This should be dependent on instruction showing, not if localizer or not
                            // Showing instructions between Localizers!
                            if (!PlayerProgression.ActiveLevel.isLocalizer)
                                PauseExperiment(false, false);
                        }, 
                        showInstructions);
                    return;
                }
                else
                {
                    AsyncThread.RequestRunOnNewThread_OnNextFrame(() =>
                    {
                        experimentSessionManager.LogFMRIDump("{0};{1};2A;FADE_OUT_BEGIN"._Format(
                            TimeWrapper.GetLastFrameTimestamp_TS().ToString(), runID));
                    });

                    FadeSceneController.FadeInOut(
                        // Fade-in
                        () =>
                        {
                            AsyncThread.RequestRunOnNewThread_OnNextFrame(() =>
                            {
                                experimentSessionManager.LogFMRIDump("{0};{1};2B;FADE_OUT_DONE"._Format(
                                    TimeWrapper.GetLastFrameTimestamp_TS().ToString(), runID));
                            });

                            if (!keepMusicPlaying)
                                SoundSystem.PauseAudioMusic();

                            PauseExperiment(true, false, keepMusicPlaying); // Dont toggle cursor!
                        },
                        // Show instructions
                        () =>
                        {
                        },
                        // Hide instructions
                        () =>
                        {
                        },
                        () =>
                        {
                        },
                        // Post Wait
                        () =>
                        {
                            AsyncThread.RequestRunOnNewThread_OnNextFrame(() =>
                            {
                                experimentSessionManager.LogFMRIDump("{0};{1};2C;FADE_IN_BEGIN"._Format(
                                    TimeWrapper.GetLastFrameTimestamp_TS().ToString(), runID));
                            });

                            if (ShowLevelCompletePopup(e, () =>
                            {
                                SoundSystem.TogglePersistentMusic(false); // Kill the music after this

                                // Get back to LevelSelection
                                CleanUp_FinalCB();
                                AsyncThread.RequestRunOnNewThread_OnNextFrame(() =>
                                {
                                    experimentSessionManager.LogFMRIDump("{0};{1};2C++;LEVEL_COMPLETE_POPUP_HIDDEN"._Format(
                                        TimeWrapper.GetLastFrameTimestamp_TS().ToString(), runID));
                                });
                            }, levelCompleteTimeout))
                            {
                                // Make sure we can click it
                                experimentSessionManager.ToggleCursor(true);
                                AsyncThread.RequestRunOnNewThread_OnNextFrame(() =>
                                {
                                    experimentSessionManager.LogFMRIDump("{0};{1};2C+;LEVEL_COMPLETE_POPUP_SHOWN"._Format(
                                        TimeWrapper.GetLastFrameTimestamp_TS().ToString(), runID));
                                });
                            }
                        },
                        // Fade out
                        () =>
                        {
                            experimentSessionManager.LogFMRIDump("{0};{1};2D;FADE_IN_DONE"._Format(
                                TimeWrapper.GetLastFrameTimestamp_TS().ToString(), runID));
                        },
                        false
                        );
                    return;
                }
            }

            // End of level ::
            if (ShowLevelCompletePopup(e, () =>
            {
                // Get back to LevelSelection
                CleanUp_FinalCB();
            }, levelCompleteTimeout))
            {
                // Make sure we can click it
                experimentSessionManager.ToggleCursor(true);
            }
        }

        #endregion


        #region UI
        private string WorldTypeToString(WorldType worldType)
        {
            switch (worldType)
            {
                case WorldType.Blue:
                    return ExperimentLibraryManager.Config.Texts.blueString;
                case WorldType.Orange:
                    return ExperimentLibraryManager.Config.Texts.orangeString;
            }
            return " ";
        }

        private Game_PopUp ShowInstructionsFmriReplayBetweenLevels(StimulusType target)
        {
            Game_PopUp.RuntimeConfig popupToShow = new Game_PopUp.RuntimeConfig();
            TextsConfig textsConfig = ExperimentLibraryManager.Config.Texts;

            popupToShow = target == StimulusType.Face ?
                textsConfig.fmriReplayBetweenLevels_Faces :
                textsConfig.fmriReplayBetweenLevels_Objects;

            return Game_PopUp.ShowPopUp(popupToShow, null);
        }

        /// <summary>
        /// Guaranteed callback
        /// </summary>
        protected bool ShowInstructionsIfNeeded(LevelConfig levelConfig, Action onPopupClicked, Action onNoPopupWanted)
        {
            Game_PopUp.RuntimeConfig popupToShow = new Game_PopUp.RuntimeConfig();
            TextsConfig textsConfig = ExperimentLibraryManager.Config.Texts;

            // Debug.LogError((levelConfig.isLocalizer ? "LOCALIZER" : "GAME") + " : " + (TheManager.FMRI_IsLevelFirstHalfOfRun(levelConfig) ? "FIRST" : "SECOND"));
            if (levelConfig.isTutorial)
            {
                if (levelConfig.isTutorial_T)
                {
                    popupToShow = InputManager.useResponseBox ?
                        textsConfig.tutorialWelcome_ResponseBoxInput :
                        textsConfig.tutorialWelcome_KeyboardInput;

                    popupToShow.description = popupToShow.description._Format(ExperimentLibraryManager.Config.moveLeft_String, ExperimentLibraryManager.Config.moveRight_String);
                }
                else if (levelConfig.isTutorial_I)
                {
                    // if not this dont show
                    if (ExperimentLibraryManager.Config.Texts.informationWelcome.title != "" &&
                       ExperimentLibraryManager.Config.Texts.informationWelcome.description != "")
                        popupToShow = textsConfig.informationWelcome;

                    // No popup is wanted
                    else
                    {
                        onNoPopupWanted();
                        return false;
                    }
                }
                else if (levelConfig.isTutorial_P)
                {
                    bool isBehavioral = module.ToPrepVsFull() == PrepVsFull.Screen;
                    bool use3Buttons = ExperimentLibraryManager.Config.Probes.use3Buttons;
                    popupToShow = textsConfig.GetPracticeTutorialMessage(isBehavioral, use3Buttons);
                        
                    if (popupToShow.description.ContainsInvariant("{3}"))  // Add in Movement
                        popupToShow.description = popupToShow.description._Format(ExperimentLibraryManager.Config.moveLeft_String, ExperimentLibraryManager.Config.moveRight_String,
                            ExperimentLibraryManager.Config.probeYes_String, ExperimentLibraryManager.Config.probeNo_String, ExperimentLibraryManager.Config.probeMaybe_String);
                    else
                        popupToShow.description = popupToShow.description._Format(
                            ExperimentLibraryManager.Config.probeYes_String, ExperimentLibraryManager.Config.probeNo_String, ExperimentLibraryManager.Config.probeMaybe_String);
                }
                else
                {
                    this.LogError("Weird tutorial bug - active level is considered tutorial but isnt one of the 3 tutorials. Level ID :: " + levelConfig.levelID_1Based);
                }

                // [SOS] StartGame immediately - instead of CheckStartGame - as we don't need to wait for FMRI during Tutorials
                Game_PopUp.ShowPopUp(popupToShow, onPopupClicked);
                return true;
            }

            WorldType worldType = levelConfig.GetWorldType_TS();

            bool isMidwayPoint =
                (!levelConfig.isLocalizer && levelConfig.levelID_1Based == numLevels / 2 + 1) ||
                (levelConfig.isLocalizer && levelConfig.localizerID_0Based == numLocalizers / 2);
            bool doInvert = ExperimentLibraryManager.Config.Input.doFlipMidWay && isMidwayPoint;

            // For game levels (first of each world)
            if (!levelConfig.isLocalizer &&
                levelConfig.levelName.ContainsInvariant("_1"))
            {

                if (levelConfig.levelName.ContainsInvariant("1_1"))
                {
                    popupToShow = textsConfig.welcomeMessage_1_1;
                    popupToShow.description = popupToShow.description._Format(WorldTypeToString(worldType).ToLower());
                }
                else if (levelConfig.levelName.ContainsInvariant("2_1"))
                {
                    popupToShow = textsConfig.welcomeMessage_2_1;
                    popupToShow.description = popupToShow.description._Format(WorldTypeToString(worldType).ToLower(), WorldTypeToString(worldType.Invert_TS()).ToLower());
                }
                else if (levelConfig.levelName.ContainsInvariant("3_1"))
                {
                    popupToShow = textsConfig.welcomeMessage_3_1;
                    popupToShow.description = popupToShow.description._Format(WorldTypeToString(worldType).ToLower(), WorldTypeToString(worldType.Invert_TS()).ToLower());
                }
                else if (levelConfig.levelName.ContainsInvariant("4_1"))
                {
                    popupToShow = textsConfig.welcomeMessage_4_1;
                    popupToShow.description = popupToShow.description._Format(WorldTypeToString(worldType).ToLower(), WorldTypeToString(worldType.Invert_TS()).ToLower());
                }

                // Did we invert inputs?
                if (doInvert)
                {
                    popupToShow.description += "\n\n" + textsConfig.welcomeMessage_InputsFlippedAppendix
                        ._Format(ExperimentLibraryManager.Config.probeYes_String, ExperimentLibraryManager.Config.probeNo_String);
                }

                if (module == ModuleType.FMRI_Scanner && ExperimentLibraryManager.Config.FMRI.scannerSkipInstructions)
                {
                    onNoPopupWanted();
                    return false;
                }

                // [SOS] Use CheckStartGame - as we may need to wait for FMRI
                Game_PopUp.ShowPopUp(popupToShow, onPopupClicked);

                return true;
            }

            // For All localizers
            if (levelConfig.isLocalizer &&
                /// That are either not FMRI, OR that are the first half of an FMRI run (2nd halves are handled in <see cref="ShowInstructionsFmriReplayBetweenLevels"/>
                (!isFMRI || ExperimentManagerSession.FMRI_IsLevelFirstHalfOfRun(levelConfig)))
            {
                // TODO HACK This should be on the LOCALIZER (not the base!
                bool lookForFaces = StimulusManager.GetLevelStimulusTarget(levelConfig).target == StimulusType.Face;

                if (lookForFaces)
                {
                    popupToShow = doInvert ? textsConfig.instructionTaskRelevant_Faces_Inverted : textsConfig.instructionTaskRelevant_Faces;
                    popupToShow.description = popupToShow.description._Format(ExperimentLibraryManager.Config.reportFace_String);
                }
                else
                {
                    popupToShow = doInvert ? textsConfig.instructionTaskRelevant_Objects_Inverted : textsConfig.instructionTaskRelevant_Objects;
                    popupToShow.description = popupToShow.description._Format(ExperimentLibraryManager.Config.reportObject_String);
                }

                Game_PopUp.ShowPopUp(popupToShow, onPopupClicked);
                return true;
            }

            {
                onNoPopupWanted();
                return false;
            }
        }

        public static bool ShowLevelCompletePopup(LevelCompleteArgs e, Action confirmCallback, float timeout = -1)
        {
            if (e.endReason == EndReason.Exit)
            {
                confirmCallback();
                return false;
            }

            if (!ExperimentLibraryManager.Config.Texts.showCongratulatoryMessages)
            {
                if (EngineWrapper.Debug_IsDebugBuild)
                    Debug.LogWarning("Not showing congratulatory messages");
                confirmCallback();
                return false;
            }

            LevelConfig levelConfig = e.levelConfig;
            int currentLevelID = levelConfig.levelID_1Based;

            bool isLastLocalizer = currentLevelID >= LevelConfig.localizerOffset + numLocalizers - 1;
            bool isLastGameLevel = PlayerProgression.IsLastLevelOfGame(levelConfig);

            // Check for end of Experiment
            if ((numLocalizers >= 1 && isLastLocalizer) ||   // if we completed L8 (id 107) or L4 (id 103)
                (numLocalizers == 0 && isLastGameLevel))     // if we completed the last game level (used in FMRI practice)
            {
                Game_PopUp.ShowPopUp(ExperimentLibraryManager.Config.Texts.levelCompleted_GameCompleted, confirmCallback, timeout);

                // Turn off the editor's auto-answer (after we auto-answer and are back to menu, no need to bloat the logs)
                if (Debug.isDebugBuild)
                    experimentSessionManager.ResetTimeScale(false);

                return true;
            }

            string levelName_Previous = LevelsLibrary.GetLevelName(currentLevelID - 1, true);
            string levelName_Current = LevelsLibrary.GetLevelName(currentLevelID, true);

            // For the last level of the game
            string levelName_Next = PlayerProgression.IsLastLevelOfGame(levelConfig) ?
                levelName_Next = LevelsLibrary.GetLevelName(LevelConfig.localizerOffset, true) :  // Next level is localizer #1
                LevelsLibrary.GetLevelName(currentLevelID + 1, true);     // Otherwise it's just id + 1

            Game_PopUp.RuntimeConfig popupToUse;

            if (levelConfig.isTutorial)
            {
                popupToUse = ExperimentLibraryManager.Config.Texts.levelCompleted_Tutorial_Generic;

                if (levelConfig.isTutorial_P && // If it's the Practice level
                    ExperimentLibraryManager.Config.Texts.levelCompleted_Tutorial_PracticeShowExtendedMessage) // and we treat it differently than the other tutorials
                {
                    popupToUse = e.endReason == EndReason.Success ?
                        ExperimentLibraryManager.Config.Texts.levelCompleted_Tutorial_PracticeExtended_Success :
                        ExperimentLibraryManager.Config.Texts.levelCompleted_Tutorial_PracticeExtended_Failure;

                    popupToUse.description = popupToUse.description._Format(
                        ExperimentManagerSession.journeySum.TotalNumberOfBlanksFalseAlarms);
                }

                Game_PopUp.ShowPopUp(popupToUse, confirmCallback, timeout);
            }
            else
            {
                if (isFMRI)
                {
                    if (ExperimentManagerSession.FMRI_IsLevelFirstHalfOfRun(levelConfig))
                    {
                        Debug.LogWarning("FMRI - skipping congrats");
                        confirmCallback();
                    }
                    else
                    {
                        popupToUse = ExperimentLibraryManager.Config.Texts.levelCompleted_Pair;
                        popupToUse.description = popupToUse.description._Format(levelName_Previous, levelName_Current);
                        Game_PopUp.ShowPopUp(popupToUse, confirmCallback, timeout);
                    }
                }
                else
                {
                    popupToUse = ExperimentLibraryManager.Config.Texts.levelCompleted;
                    popupToUse.description = popupToUse.description._Format(levelName_Next);
                    Game_PopUp.ShowPopUp(popupToUse, confirmCallback, timeout);
                }
            }

            return true;
        }

        private static IEnumerator SafeShowProbesCompletePopupIE()
        {
            yield return new WaitForSeconds(ExperimentLibraryManager.Config.PostLevelCleanupSafety);
            ShowProbesCompletePopup();
        }

        private static void ShowProbesCompletePopup()
        {
            int levelID_1Based = CURRENT_LEVEL.levelID_1Based;

            // Are we showing?
            if (ExperimentLibraryManager.Config.Texts.showProbesEarlyOutMessage)
            {
                Game_PopUp.ShowPopUp(ExperimentLibraryManager.Config.Texts.levelCompleted_ProbesEarlyOut, () =>
                {
                    PlayerProgression.CompleteCurrentLevel();
                });
            }
            else
                PlayerProgression.CompleteCurrentLevel();
        }

        public void DisplayEyeTrackerResultsUI(LevelConfig levelConfig)
        {
            // did we have eyelink?
            if (eyeTracker == null || eyeTrackingDuringLevel_HadSomeData == 0)
                return;

            string lastLevelID_Name = levelConfig.isLocalizer ?
                LevelsLibrary.GetLocalizerNameUI(levelConfig.localizerID_0Based) :
                levelConfig.levelID_1Based.ToString();

            Action doReport = () =>
            {
                string report = "";

                // Is it ok now?
                if (eyeTracker.IsConnectedDebug())
                {
                    // Debug.Log(eyeTrackingDuringLevel_HadNoConnection); Debug.Log(eyeTrackingDuringLevel_HadNoData); Debug.Log(eyeTrackingDuringLevel_NumFrames);

                    // Did we lose the data at any point
                    if (eyeTrackingDuringLevel_HadNoConnection >
                            ExperimentLibraryManager.Config.EyeTracking.maxPercentileDisconnectForWarning * eyeTrackingDuringLevel_NumFrames)
                    {
                        // Warning
                        report = "Eye Link lost CONNECTION during level, but is connected now.";
                        Game_PopUp.ShowPopUp(ExperimentLibraryManager.Config.Texts.eyeLink_Connected_ConnLostDuringLastLevel, null);
                        Debug_Helper.LogWarning(typeof(Debug_Helper), report);
                    }
                    else if (eyeTrackingDuringLevel_HadNoData >
                        ExperimentLibraryManager.Config.EyeTracking.maxPercentileNoDataForWarning * eyeTrackingDuringLevel_NumFrames)
                    {
                        // Warning
                        report = "Eye Link lost DATA during level, but is connected now.";
                        Game_PopUp.ShowPopUp(ExperimentLibraryManager.Config.Texts.eyeLink_Connected_DataLostDuringLastLevel, null);
                        Debug_Helper.LogWarning(typeof(Debug_Helper), report);
                    }
                    else
                    {
                        // Log
                        report = "Eye Link didn't lose DATA or CONNECTION during level, and it's connected.";
                        Debug_Helper.Log(typeof(Debug_Helper), report);
                    }
                }
                else
                {
                    // Error
                    report = "Eye Link is NOT connected now.";
                    Game_PopUp.ShowPopUp(ExperimentLibraryManager.Config.Texts.eyeLink_NotConnected, () =>
                    {
                        eyeTracker.TryConnect();
                    });
                    Debug_Helper.LogError(typeof(Debug_Helper), report);
                }

                if (!report.IsNullOrEmpty())
                {
                    string log = "[EYE_LINK] Level-ID {0} (1-based) Report :: {1}"._Format(lastLevelID_Name, report);
                    ExperimentManagerSession.LogSessionInfo(log);
                }
            };

            // Show Heatmap if needed !
            if (ExperimentLibraryManager.Config.EyeTracking.showHeatmapAfterLevel &&
                (!isFMRI || !ExperimentManagerSession.FMRI_IsLevelFirstHalfOfRun(levelConfig)))
            {
                Sprite heatmap = null;

                // Debug.Log("B " + TimeWrapper.currentTimestampMS);
                // If we want gaze and have gaze, return gaze
                if ((ExperimentLibraryManager.Config.EyeTracking.heatmapType == HeatmapType.Gaze && ExperimentManagerSession.latestLevelHeatmap_Gaze_Backgrounded_ForUI != null) ||
                        // Or if we want sacade, but dont have it, try gaze
                        (ExperimentLibraryManager.Config.EyeTracking.heatmapType == HeatmapType.Sacade && ExperimentManagerSession.latestLevelHeatmap_Sacades_Backgrounded_ForUI == null))
                    heatmap = ExperimentManagerSession.latestLevelHeatmap_Gaze_Backgrounded_ForUI;
                // Otherwise try sacade
                else
                    heatmap = ExperimentManagerSession.latestLevelHeatmap_Sacades_Backgrounded_ForUI;

                Game_PopUp.ShowPopUp(ExperimentLibraryManager.Config.Texts.eyeLink_HeatmapTitle._Format(lastLevelID_Name), heatmap, doReport);
            }
            else
                doReport();
        }

        #endregion

        #region TEMP Player Progression Initialization
        public static void SESSION_GameManagers_PrepLevelManager(LevelConfig levelConfig, float orthoSize, Vector2 offset01)
        {
            // [SOS] set before messing with the Anything else system
            Camera.main.orthographicSize = orthoSize;

            // [201013] [HACK] [KON] No idea why this works, why it needs negative to go up, or why it needs * 2
            Camera.main.transform.position = new Vector3(
                - Camera.main.orthographicSize * offset01.x * 2 / Screen.height * Screen.width,
                - Camera.main.orthographicSize * offset01.y * 2,
                Camera.main.transform.position.z);

            Func<Camera, float> fixationScale = ProbeSystem.GetFixationLossyScale;
            Color fixationColor = ExperimentLibraryManager.Config.Probes.colorMainHex.ToColor();
            bool isFirstHalf = ExperimentManagerSession.FMRI_IsLevelFirstHalfOfRun(levelConfig);
            FadeSceneController.RuntimeConfig fadeScene = new FadeSceneController.RuntimeConfig(
                orthoSize, fixationScale, fixationColor, isFirstHalf);

            bool hasIntroDim = !isFMRI || ExperimentManagerSession.FMRI_IsLevelFirstHalfOfRun(levelConfig);

            List<ITrigger> fmriTriggerManagers = new List<ITrigger>();

            // In FMRI
            if (isFMRI &&
                // First Halves get triggers
                // (levelConfig.isLocalizer || 
                (ExperimentManagerSession.FMRI_IsLevelFirstHalfOfRun(levelConfig) || levelConfig.isTutorial_P))
            {
                fmriTriggerManagers.Add(instance.fmriTrigger_Keyboard);
                fmriTriggerManagers.Add(instance.fmriTrigger_Serial);
            };

            bool fakeStartTrigger =
                module == ModuleType.FMRI_Preparation ||
                ExperimentLibraryManager.Config.FMRI.scanner_startGame_SendFakeSerialTriggers || 
                EXPERIMENTER_AUTO_ANSWER;

            bool skipEndOfGameMessage = isFMRI && !levelConfig.isTutorial; // Skip message for FMRI - TheManager will handle showing it | tutorials are an exception
            string levelName = levelConfig.levelName;

            bool shouldRecord;
            string replayLevelName;

            if (levelConfig.isLocalizer)
            {
                shouldRecord = false;
                replayLevelName = ExperimentManagerSession.GetRecordedReplayLevel(levelConfig).levelName;
                // Debug.LogError(levelConfig.levelID_1Based + " is a REPLAY :: Replaying level :: " + replayLevelName);
            }
            else
            {
                // Whether 
                shouldRecord = ExperimentManagerSession.IsIntendedForReplay(levelConfig.levelID_1Based);
                replayLevelName = levelConfig.levelName;
                // Debug.LogError(levelConfig.levelID_1Based + " is a GAME LEVEL | " + (shouldRecord ? "RECORDING" : "NOT RECORDING") + " LEVEL " + replayLevelName);
            }


            bool useCameraBlock = ExperimentLibraryManager.Config.Experiment.stimulus.background.usingActiveAreaAndCropEnabled;
            float viewportSizePercentile = ExperimentLibraryManager.Config.Experiment.stimulus.background.viewportSizePercentile;
            Color viewportBlockingColor = ExperimentLibraryManager.Config.Experiment.stimulus.background.viewportBlockingColor;

            float evaluationWindow =
                ExperimentLibraryManager.Config.Difficulty.evaluationWindow_NumAnimCycles *
                ExperimentLibraryManager.Config.GetPeriodMinMax_Background().avg;

            int targetScore = PlayerProgression.TargetScorePerLevel(levelConfig.levelID_1Based);

            bool doCalculatePerformance = !levelConfig.isLocalizer;
            // LevelsLibrary.GetLevel(ReplaySystem.activeRecordLevel.levelID_1Based).baseDifficulty;
            // float baseLevelDifficulty = LevelsLibrary.GetLevel(ReplaySystem.activeRecordLevel.levelID_1Based).baseDifficulty;

            float playerVerticalPositionScaled = ExperimentLibraryManager.Config.Experiment.stimulus.background.GetPlayerVerticalPositionScaled(Camera.main);
            float playerViewScaleMulti = ExperimentLibraryManager.Config.Experiment.stimulus.background.GetScaleMultiplierPlayer(Camera.main);
            float playerViewInvulnerableDurationSeconds =
                ExperimentLibraryManager.Config.PlayerProgression.levelMasterManager.gameManager.playerManager.model.invulnerableDuration;

            PlayerView_Runner.RuntimeConfig playerViewRunner = new PlayerView_Runner.RuntimeConfig(
                playerViewScaleMulti, playerViewInvulnerableDurationSeconds);

            List<KeyCode> moveLeftKeyCodes = ExperimentLibraryManager.Config.InputKeyCode.Action_Left;
            List<KeyCode> moveRightKeyCodes = ExperimentLibraryManager.Config.InputKeyCode.Action_Right;

            PlayerController.RuntimeConfig playerController = new
                PlayerController.RuntimeConfig(moveLeftKeyCodes, moveRightKeyCodes);

            bool hasAbsorptionPowerUp = levelConfig.levelID_1Based >= ExperimentLibraryManager.Config.LevelProgression.absorptionPowerUpLevel;
            bool hasThunderPowerUp = levelConfig.levelID_1Based >= ExperimentLibraryManager.Config.LevelProgression.thunderPowerUpLevel;
            bool enableHighEnergyEssences = ExperimentLibraryManager.Config.LevelProgression.highEnergyEssenceLevel <= levelConfig.levelID_1Based;

            Rect interactableActiveArea = ExperimentLibraryManager.Config.Experiment.stimulus.background.GetActiveAreaWorld(Camera.main);
            float interactableSpawnVerticalPositionScaled = ExperimentLibraryManager.Config.Experiment.stimulus.background.GetSpawnVerticalPositionScaled(Camera.main);
            // [200618] This should in theory also affect the player, but keeping it consistent with March pilots it only affects interactables
            float interactableFallingSpeedMulti = ExperimentLibraryManager.Config.Subject.GetFallingSpeedMultiplier();
            bool interactableCanJumpLanes = levelConfig.levelID_1Based >= ExperimentLibraryManager.Config.LevelProgression.jumpingEssenceLevel;
            float interactableScaleMulti = ExperimentLibraryManager.Config.Experiment.stimulus.background.GetScaleMultiplierEssences(Camera.main);

            /// ==== KOREO
            LevelConfig audioLevelOverride = levelConfig;
            if (ExperimentLibraryManager.Config.PlayerProgression.levelsLibrary.invertBlueOrangeWorlds)
            {
                string inverseLevelName = levelConfig.levelName;
                if (inverseLevelName.Contains("1_"))
                    inverseLevelName = inverseLevelName.Replace("1_", "2_");
                else if (inverseLevelName.Contains("2_"))
                    inverseLevelName = inverseLevelName.Replace("2_", "1_");
                else if (inverseLevelName.Contains("3_"))
                    inverseLevelName = inverseLevelName.Replace("3_", "4_");
                else if (inverseLevelName.Contains("4_"))
                    inverseLevelName = inverseLevelName.Replace("4_", "3_");
                audioLevelOverride = LevelsLibrary.GetLevelByName(inverseLevelName);
            }
            int koreoIndex = audioLevelOverride.koregraphyIndex;
            float koreoVolume = audioLevelOverride.musicVolume;
            UnityEngine.Audio.AudioMixerGroup koreMixerGroup = null;

            // Hook up its audio source to the audio mixer
            if (ExperimentLibraryManager.Config.Audio.useAudioMixer)
            {
                UnityEngine.Audio.AudioMixer audioMixer = Resources.Load<UnityEngine.Audio.AudioMixer>("Audio/Main");
                koreMixerGroup = audioMixer.FindMatchingGroups("Master").GetFirst();
            }

            /// ==== KOREO
            float laneScaleMulti = ExperimentLibraryManager.Config.Experiment.stimulus.background.GetScaleMultiplierLanes(Camera.main);

            GameManager_Runner.RuntimeConfig gameRuntimeConfig = new GameManager_Runner.RuntimeConfig(
                levelConfig, CheckLevelSuccess,
                new ReplaySystem.RuntimeConfig(shouldRecord, replayLevelName),
                new DifficultyManager.RuntimeConfig(levelConfig, doCalculatePerformance, evaluationWindow, numLevels),//, baseLevelDifficulty),
                new ScoreSystem.RuntimeConfig(levelConfig, targetScore),
                new PlayerManager_Runner.RuntimeConfig(levelConfig.gameType, playerVerticalPositionScaled, playerViewRunner, playerController),
                hasAbsorptionPowerUp, hasThunderPowerUp, enableHighEnergyEssences,
                new Interactable.RuntimeConfig(interactableActiveArea, interactableSpawnVerticalPositionScaled, interactableScaleMulti, interactableFallingSpeedMulti, interactableCanJumpLanes),
                ExperimentLibraryManager.Config.GetAudioSourceConfig(module, AudioSourceType.SFX),
                ExperimentLibraryManager.Config.GetAudioSourceConfig(module, AudioSourceType.MUSIC),
                new KoreographyWrapper.RuntimeConfig(koreoIndex, koreoVolume, koreMixerGroup), laneScaleMulti);

            // Is it The Info level?
            LevelMasterManager.RuntimeConfig levelRuntimeConfig;

            // MESSY
            if (levelConfig.isTutorial_I)
            {
                string tutorialImagesPath = ExperimentPaths.GetTutorialInfoImagesDirectory(module.ToString());
                bool autoProceed = EXPERIMENTER_AUTO_ANSWER;
                List<KeyCode> keyCodes_Forward = ExperimentLibraryManager.Config.InputKeyCode.Menu_OK;
                List<KeyCode> keyCodes_Backward = ExperimentLibraryManager.Config.InputKeyCode.Menu_Close;
                List<string> ignoreZoomFileNames = new List<string>()
                {
                    // [07-10-20] Nick - Bypassed the zoomignore for both Faces, Objects so they can scale like the other Instruction slides
                    // ExperimentLibraryManager.Config.Experiment.stimulus.preExposureConfig_Faces.fileName,
                    // ExperimentLibraryManager.Config.Experiment.stimulus.preExposureConfig_Objects.fileName
                };

                LevelMasterManager_Game_Tutorial_I_UI.RuntimeConfig uiRuntimeConfig = new LevelMasterManager_Game_Tutorial_I_UI.RuntimeConfig(
                    tutorialImagesPath, autoProceed, keyCodes_Forward, keyCodes_Backward, ignoreZoomFileNames, viewportSizePercentile);

                levelRuntimeConfig = new LevelMasterManager_Game_Tutorial_I.RuntimeConfig(gameRuntimeConfig,
                    orthoSize, levelConfig, fadeScene, hasIntroDim, fmriTriggerManagers, fakeStartTrigger,
                    skipEndOfGameMessage, useCameraBlock, viewportSizePercentile, viewportBlockingColor, uiRuntimeConfig);
            }
            else
            {
                levelRuntimeConfig = new LevelMasterManager.RuntimeConfig(gameRuntimeConfig,
                    orthoSize, levelConfig, fadeScene, hasIntroDim, fmriTriggerManagers, fakeStartTrigger,
                    skipEndOfGameMessage, useCameraBlock, viewportSizePercentile, viewportBlockingColor);
            }

            PlayerProgression.InitializeLevelManager(levelRuntimeConfig);
        }

        private static bool CheckLevelSuccess(LevelConfig e)
        {
            if (!e.isTutorial_P) return true;

            // Before we end the game, make sure to update our success / fail for the popup
            int numFalseAlarms = ExperimentManagerSession.journeySum.TotalNumberOfBlanksFalseAlarms;
            int acceptableFalseAlarms = ExperimentLibraryManager.Config.Probes.GetAcceptableNumberOfFalseAlarmsPractice(module.ToPrepVsFull());

            bool practiceSuccess = numFalseAlarms <= acceptableFalseAlarms;

            if (EngineWrapper.Debug_IsDebugBuild)
                Debug.LogWarning("Subject had " + numFalseAlarms + " out of " + acceptableFalseAlarms + " max - " + (practiceSuccess ? "SUCCESS" : "FAIL"));

            return practiceSuccess;
        }

        #endregion


        #region TEMP EyeTracking

        private static EyeTrackerManager_Base eyeTracker { get { return experimentSessionManager.eyeTracker; } }

        private static int eyeTrackingDuringLevel_HadSomeData;
        private static int eyeTrackingDuringLevel_HadNoConnection;
        private static int eyeTrackingDuringLevel_HadNoData;
        private static int eyeTrackingDuringLevel_NumFrames;

        private void EyeTracker_DeInitialize()
        {
            // Some systems are no longer needed
            eyeTracker?.DeInitialize();
        }

        private void UpdateEyeTracker()
        {
            if (IS_IN_GAME && eyeTracker?.isInitialized == true)
            {
                switch (eyeTracker.currentState)
                {
                    case EyeTrackerState.Unknown:
                        break;
                    case EyeTrackerState.NotConnected:
                        eyeTrackingDuringLevel_HadNoConnection++;
                        break;
                    case EyeTrackerState.InsufficientData:
                        eyeTrackingDuringLevel_HadNoData++;
                        break;
                    // We don't have coordinates but we still have a sample
                    case EyeTrackerState.Blink:
                    case EyeTrackerState.GazeOutOfScreen:
                    case EyeTrackerState.GazeOffFixation:
                    case EyeTrackerState.AllGood:
                        eyeTrackingDuringLevel_HadSomeData++;
                        break;
                }

                eyeTrackingDuringLevel_NumFrames++;
            }
        }

        #endregion


        #region TEMP Quick Access
        public static PrepVsFull prepVsFull { get { return ExperimentManagerSession.prepVsFull; } }
        private static ModuleType module { get { return ExperimentManagerSession.module; } }
        private static ModuleType MODULE_TO_LOAD_CONFIG;
        private static bool isFMRI { get { return ExperimentManagerSession.isFMRI; } }
        private static int numLocalizers { get { return ExperimentManagerSession.numLocalizers; } }
        private static int numGameWorlds { get { return ExperimentManagerSession.numGameWorlds; } }
        private static int numLevelsPerWorld { get { return ExperimentManagerSession.numLevelsPerWorld; } }
        private static int numLevels { get { return ExperimentManagerSession.numLevels; } }
        private static bool EXPERIMENTER_AUTO_ANSWER { get { return ExperimentManagerSession.EXPERIMENTER_AUTO_ANSWER; } }
        #endregion


        #region Vars

        /// <summary>
        /// [SOS] Consumed by <see cref="SceneManager_sceneLoaded(Scene, LoadSceneMode)"/>
        /// </summary>
        private static LevelConfig TEMP_LEVEL_SELECTED = null;
        
        public static bool IS_IN_GAME { get { return LAST_GAME_TYPE != GameType.None; } }
        public static bool IS_IN_MENU { get { return !IS_IN_GAME; } }
        public static GameType LAST_GAME_TYPE { get { return CURRENT_LEVEL != null ? CURRENT_LEVEL.gameType : GameType.None; } }
        public static string LAST_LEVEL_NAME { get { return CURRENT_LEVEL != null ? CURRENT_LEVEL.levelName : ""; } }
        public static LevelConfig CURRENT_LEVEL { get; private set; }

        [SerializeField] private Toggle muteToggle = null;
        private CanvasGroup muteCG = null;
        [SerializeField] private Text configBrokenParent = null;

        // private static LevelConfig TEMP_LEVEL_SELECTED = null;

        private MenuManager menuManager;
        private AsyncOperation loadingLevelOperation;

        public const string GAME_SCENE_NAME_FORMAT = "Game_{0}";

        public const string CUTSCENE_NAME_FORMAT = "Director_{0}";
        public const string LEVELSELECTION_SCENE_NAME = "LevelSelection";
        public const string MAINMENU_SCENE_NAME = "MainMenu";

        public static bool autoForwardToNextLevel { get; private set; }

        public static event EventHandler<EventArgs> onTRReceived;
        private KeyboardTrigger fmriTrigger_Keyboard;
        [SerializeField] private SerialPortTrigger fmriTrigger_Serial = null;

        private static ExperimentManagerSession experimentSessionManager;

        public Text debugTimeText;
        public static bool IS_LEVELSTART_LOCKED { get { return IS_LEVELSTART_LOCKED_BY_EXPERIMENTER || IS_LEVELSTART_LOCKED_BY_FMRI; } }
        public static bool IS_LEVELSTART_LOCKED_BY_EXPERIMENTER { get; private set; }
        public static bool IS_LEVELSTART_LOCKED_BY_FMRI { get; private set; }

        private static string CURRENT_SCENE = "";

        private static bool? wasFlipped = null;

        /// <summary>
        /// Use for testing purposes only, before this script awakes
        /// </summary>
        public static bool SOS_PREVENT_AUTO_LOAD_OF_MAIN_MENU = false;

        #endregion


        // Session -> Subject -> Module -> Level
        #region Init (Session)

        protected override void Initialize()
        {
            base.Initialize();

            // [SOS] Init The Library before anything else as the CONFIG depends on it
            ExperimentLibraryManager.Initialize();

            AsyncThread.maxThreads = ExperimentLibraryManager.Config.maxNumThreads;

            // Init all the helpers
            PerformanceObserver.Initialize(ExperimentLibraryManager.Config.PerformanceObserver);
            LogWrapper.Initialize(ExperimentLibraryManager.logsDirectory, ExperimentLibraryManager.Config.Logging.logger.logWrapper);
            TimeWrapper.Initialize();
            EngineWrapper.Initialize();

            DifficultyManager.Initialize(ExperimentLibraryManager.Config.Difficulty);

            muteCG = muteToggle.gameObject.AddComponentIfNotExists<CanvasGroup>();

            CURRENT_LEVEL = null;

            // Debug.Log(Utility_Helper.EnumGetValues<KeyCode>().ToReadableString());

            LeanTween.init(ExperimentLibraryManager.Config.maxNumTweens);

            string error;
            if (!ExperimentLibraryManager.TryLoadFile(out error))
            {
                configBrokenParent.gameObject.SetActive(true);
                configBrokenParent.text += string.Format("\n\nFollowing error appearead:\n<i>{0}</i>", error);

                return;
            }

            if (ExperimentLibraryManager.Config.LabCode.Length != 2)
            {
                error = string.Format("\n\nLabCode in Config->LabCode is invalid. Expected 2 characters, but found {0}\nvalue:'{1}'", ExperimentLibraryManager.Config.LabCode.Length, ExperimentLibraryManager.Config.LabCode);
                configBrokenParent.gameObject.SetActive(true);
                configBrokenParent.text += error;
                this.LogError(error.Replace("\n", ""));
                return;
            }

            if (configBrokenParent == null)
                this.LogWarning("No config broken parent ; can't communicate config state to UI");
            else
                configBrokenParent.gameObject.SetActive(false);

            // We have CONFIG
            MODULE_TO_LOAD_CONFIG = ExperimentLibraryManager.Config.module;
#if UNITY_EDITOR
            if (EditorOnlyUtilities.moduleTypeOverride != null && EditorOnlyUtilities.moduleTypeOverride != ModuleType.None)
            {
                Debug.LogWarning("[EDITOR_ONLY] Overriding Module {0} -> {1}"._Format(module, EditorOnlyUtilities.moduleTypeOverride));
                MODULE_TO_LOAD_CONFIG = EditorOnlyUtilities.moduleTypeOverride.Value;
            }
#endif

            // Sub to events
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            //EyeLinkManager.onSacadaDetected += EyeLinkManager_onSacada;
            
            // [SOS] Call before muteToggle UI.
            SoundSystem.Initialize(ExperimentLibraryManager.Config.Audio, ExperimentPaths.getAudioFolderPath());
            //SerialPortManager.Initialize();

            // NS_segment move to Sound System.
            if (muteToggle == null)
                this.LogWarning("Could not find the Mute Toggle button!");
            else
            {
                muteToggle.isOn = !SoundSystem.isMuted;// AudioListener.volume > 0;
                muteToggle.onValueChanged.AddListener(MuteToggle_OnValueChanged);
            }

            if (debugTimeText == null)
                this.LogWarning("Could not find the Debug Time text!");
            else
                debugTimeText.enabled = Debug.isDebugBuild || ExperimentLibraryManager.Config.showTime;

            Application.runInBackground = ExperimentLibraryManager.Config.runInBackground;

            Action onReadyToProceed = () =>
            {
                // Move on to the next scene!
                if (SOS_PREVENT_AUTO_LOAD_OF_MAIN_MENU)
                    this.LogWarning("Prevented auto loading of main menu");
                else
                    SceneManager.LoadSceneAsync(MAINMENU_SCENE_NAME);
            };

            // Are we doing audio triggers?
            if (ExperimentLibraryManager.Config.TriggerOut.GetEventConfig(MODULE_TO_LOAD_CONFIG).doAudio &&
                ExperimentLibraryManager.Config.TriggerOut.triggerManagerAudio.playerType == TriggerManager_Audio.Config.PlayerType.LowLevel)
            {
                this.LogWarning("Generating Audio Triggers");
                DateTime start = DateTime.UtcNow;

                TriggerManager_Audio.Prepare(
                    ExperimentLibraryManager.Config.TriggerOut.triggerManagerAudio,
                    ExperimentLibraryManager.Config.TriggerOut.triggerCodes_A.numBits,
                    progressReport =>
                    {
                        if (progressReport.isDone)
                        {
                            TimeSpan diff = DateTime.UtcNow - start;
                            onReadyToProceed();
                            this.LogWarning("Audio Trigger Generation took {0} seconds "._Format(diff.TotalSeconds.ToString("#.00")));
                        };
                    });
            }
            else
                onReadyToProceed();

            // [TODO] ENABLE LOGGING TESTING
            // LoggingTester.Initialize(ApplicationLibrary.Config.LoggingTesting);
        }

        public static void SetPrepVsFull(PrepVsFull prepVsFull)
        {
            // [200720] DUMMY (will be changed soon)
            ExperimentManagerSession.module = prepVsFull.ToModule(MODULE_TO_LOAD_CONFIG);

            // [HACK]
            BackgroundConfig.SetUseMachine(prepVsFull == PrepVsFull.Screen ? BackgroundConfig.Machine.Behavioral : BackgroundConfig.Machine.Experimental);

            if (ExperimentManagerSession.session_probeSummary == null)
            {
                MenuManager_MainMenu mmMainMenu = instance.menuManager as MenuManager_MainMenu;

                if (mmMainMenu == null)
                    Debug.LogError("Weird, set prep vs full called without a " + typeof(MenuManager_MainMenu));
                else
                    mmMainMenu.ToggleSubjectWindow(true); // ready for subject info!
            }

            Debug.Log("Set System -> " + module);
        }

        private void SetupEyeTracker(ModuleType module, out EyeTrackerManager_Base eyeTracker, out EyeTracker_UI eyeTrackerUI)
        {
            eyeTracker = null;
            eyeTrackerUI = null;

            // Eye Trackers
            if (ExperimentLibraryManager.Config.EyeTracking.IsEnabled(module))
            {
                EyeTrackerManager_Base.ImplementationConfig implementationConfig = null;

                switch (ExperimentLibraryManager.Config.EyeTracking.trackerType)
                {
                    case EyeTrackerManager_Base.Config.TrackerType.EyeLink:
                        eyeTracker = UnityEngine.Object.FindObjectOfType<EyeTrackerManager_EyeLink>();
                        implementationConfig = ExperimentLibraryManager.Config.EyeLink;
                        break;
                    case EyeTrackerManager_Base.Config.TrackerType.Tobii:
                        eyeTracker = UnityEngine.Object.FindObjectOfType<EyeTrackerManager_Tobii>();
                        implementationConfig = ExperimentLibraryManager.Config.Tobii;
                        break;
                }

                eyeTrackerUI = UnityEngine.Object.FindObjectOfType<EyeTracker_UI>();

                if (eyeTracker == null)
                {
                    this.LogError("Wanted Eye tracker but found none!");
                }
                else
                {
                    if (ExperimentLibraryManager.Config.EyeTracking.IsEnabled(ExperimentManagerSession.module))
                    {
                        EyeTrackerManager_Base.Config baseConfig = ExperimentLibraryManager.Config.EyeTracking;

                        eyeTracker?.Initialize(eyeTrackerUI, baseConfig, implementationConfig, true);

                        BackgroundManager.Config backgroundConfig = ExperimentLibraryManager.Config.Experiment.stimulus.background;

                        // Calculate the visual angle between the center of the screen and the current gaze coordinates
                        Vector2 screenCenterPixels = new Vector2(0.5f * Screen.width, backgroundConfig.fixationVerticalPositionPercentileUnscaled * Screen.height);

                        float maxDistance_VisualAngle = backgroundConfig.maxVisualAngleDeviationFromFixation;
                        float maxDistance_CM = backgroundConfig.GetCMFromVisualAngle(maxDistance_VisualAngle);
                        float maxDistance_Pixels = backgroundConfig.GetPixelsFromCM(maxDistance_CM);

                        eyeTracker?.SetFixation(screenCenterPixels, maxDistance_Pixels);
                    }
                    else
                        EngineWrapper.Destroy(eyeTracker.gameObject);
                }
            }
            else
            {
                eyeTracker = null;
                eyeTrackerUI = null;
            }
        }

        public void InitializeSubjectSession(SubjectInfo subject, Action<bool> _callback, string overrideFolderPathToLoadFrom = "", bool preventLoadingLevelSelection = false)
        {
            Action<bool> callback = success => // TheManager.system != ModuleType.FMRI_Preparation
            {
                if (success)
                {
                    if (preventLoadingLevelSelection)
                        this.LogWarning("Prevented loading level selection");

                    else
                        SceneManager.LoadScene(LEVELSELECTION_SCENE_NAME);
                }
                else // If for any reason it fails
                    this.LogError("Systems Initialization Error");

                _callback?.Invoke(success);
            };

            currentSubject = subject;
            ExperimentManagerApplication.overrideFolderPathToLoadFrom = overrideFolderPathToLoadFrom;

            string defaultConfigFilePath = ExperimentLibraryManager.defaultConfigFilePath;

            SoundSystem.ResetAudioSources();

            SoundSystem.RuntimeAudioSourceConfig audioTrigger =
                ExperimentLibraryManager.Config.GetAudioSourceConfig(module, AudioSourceType.TRIGGER);

            SoundSystem.RuntimeAudioSourceConfig audioTone =
                ExperimentLibraryManager.Config.GetAudioSourceConfig(module, AudioSourceType.TONE);

            string subjectSequenceFolderPath = "";
            string subjectConfigFilePath = "";

            if (overrideFolderPathToLoadFrom.IsNullOrEmpty())
            {
                subjectConfigFilePath = defaultConfigFilePath;
                int subjectID = currentSubject.subjectNumber;
                GetSubjectSequenceFolderPath(subjectID, out subjectSequenceFolderPath);
            }
            else
            {
                subjectConfigFilePath = ExperimentLibraryManager.GetConfigFilePath(overrideFolderPathToLoadFrom);

                // Reload the config in this case!
                ExperimentLibraryManager.LoadFromFile(subjectConfigFilePath);

                subjectSequenceFolderPath = overrideFolderPathToLoadFrom + "Sequence/";
            }

            EyeTrackerManager_Base eyeTracker = null;
            EyeTracker_UI eyeTrackerUI = null;
            SetupEyeTracker(module, out eyeTracker, out eyeTrackerUI);

            float zoomFactor = ExperimentLibraryManager.Config.Experiment.stimulus.background.zoomFactor;
            Vector2 screenCenterOffsetFrom_05_05 = (1 - zoomFactor) / 2 * ExperimentLibraryManager.Config.Experiment.stimulus.background.wantedOffset01; // new Vector2(0.5f, -0.5f);
            Vector2 screenScaleWithRespectToFull = Vector2.one * zoomFactor;
            Utility_Helper.SetScreenOffsetAndScale(screenCenterOffsetFrom_05_05, screenScaleWithRespectToFull);

            // [SOS] do before expSessionManager 
            // Initializes level library which is needed for exp session manager
            InitializePlayerProgression(module);

            experimentSessionManager = new ExperimentManagerSession(ExperimentLibraryManager.Config.Logging,
                new ExperimentManagerSession.RuntimeConfig(version, subjectConfigFilePath, subjectSequenceFolderPath, overrideFolderPathToLoadFrom, currentSubject,
                new ExperimentLogger.RuntimeConfig(currentSubject.id, currentSubject.folder, ExperimentLibraryManager.logsDirectory),
                 new TriggerMaster.RuntimeConfig(module, audioTrigger, eyeTracker),
                 audioTone, eyeTracker, eyeTrackerUI, screenCenterOffsetFrom_05_05));

            ExperimentManagerSession.LogSessionInfo("Screen Center Offset From (0.5, 0.5) -> {0} || Screen Scale With Respect To Full -> {1}".
                _Format(screenCenterOffsetFrom_05_05.ToString("#.00000000"), screenScaleWithRespectToFull.ToString("#.00000000")));

            // Do we want to use Serial for this system type?
            bool useSerial = ExperimentLibraryManager.Config.Input_UseSerial(module);
            bool useResponseBox = ExperimentLibraryManager.Config.Input_UseResponseBox(module);

            List<KeyCode> keyCodes = new List<KeyCode>();
            keyCodes.AddRange(ExperimentLibraryManager.Config.Input.GetExperimenterKeyCodes());
            keyCodes.AddRange(ExperimentLibraryManager.Config.EyeTracking.GetExperimenterKeyCodes());
            InputManager.RuntimeConfig inputRuntimeConfig = new InputManager.RuntimeConfig(useResponseBox, keyCodes);

            if (useSerial)
                InputManager.Initialize_Serial(ExperimentLibraryManager.Config.Input, inputRuntimeConfig,
                    ExperimentLibraryManager.Config.GetSerialInputChecks(subject.handType));
            else
                InputManager.Initialize_USB(ExperimentLibraryManager.Config.Input, inputRuntimeConfig);

            // Triggers
            instance.fmriTrigger_Keyboard = new KeyboardTrigger();
            instance.fmriTrigger_Keyboard.Initialize();

            if (instance && ExperimentLibraryManager.Config.TriggerInSerialPort.IsEnabled(module))
            {
                instance.fmriTrigger_Serial = new SerialPortTrigger();
                instance.fmriTrigger_Serial.Initialize(ExperimentLibraryManager.Config.TriggerInSerialPort);
            }

            RaiseOnLevelStartLockToggled(false);

#if UNITY_EDITOR
            bool doMassGenerate = EditorOnlyUtilities.sequences_massGenerationFromTo != null;

            StimulusManager_QueueGenerator.MASS_GENERATION_MODE = doMassGenerate;

            if (doMassGenerate)
            {
                StartCoroutine(GenerateIE(EditorOnlyUtilities.sequences_massGenerationFromTo.Value));
                return;
            }

            /// This triggers the generator (overrides config)
            if (EDITOR_ONLY_DO_SINGLE_TRIGGER_SEQUENCE_GENERATOR)
            {
                if (module.ToPrepVsFull() != PrepVsFull.Full)
                    Debug.LogError("Can't auto generate for prep modules!");
                else
                    subjectSequenceFolderPath = "";
            }
#endif

            if (ExperimentLibraryManager.Config.getSequenceFromFile && subjectSequenceFolderPath != "")
            {
                // Read in Precalculated timings
                string timingsFilePath = subjectSequenceFolderPath + "Timings.csv";
                List<string> timingsContents = FileWrapper.ReadAllLines(timingsFilePath);
                timingsContents.RemoveAt(0); // Remove Header!
                GameTimings gT = Precalculator.ReadInAllTimings(timingsContents);
                ExperimentManagerSession.LogStimulusTimings_Final(gT.GetTotalLogsAsString());

                // Read in Localizer types
                string localizersFilePath = subjectSequenceFolderPath + "Localizers.csv";
                List<string> localizersContents = FileWrapper.ReadAllLines(localizersFilePath);
                localizersContents.RemoveAt(0); // Remove Header!
                StimulusManager.ReadInLocalizers(localizersContents, subject.subjectNumber);
                ExperimentManagerSession.LogLocalizersCSV_Final(LocalizerInfo.ToCSV(StimulusManager.localizerInfos));

                // Read in Stim Sequences
                string stimSequenceFilePath = subjectSequenceFolderPath + "StimSequence.csv";
                List<string> stimSequenceContents = FileWrapper.ReadAllLines(stimSequenceFilePath);
                stimSequenceContents.RemoveAt(0); // Remove Header!
                StimulusManager.ReadInQueues(stimSequenceContents, callback);
            }
            else
            {
                // StimulusManager.GenerateQueues_OLD(callback);
                GenerateAll(subject.subjectNumber, callback);
            }

            // [SOS] Do that AFTER randomizing / reading them
            // experimentSessionManager.LogLocalizers();
        }

#if UNITY_EDITOR
        private IEnumerator GenerateIE(Vector3Int generateFromTo)
        {
            int subjectNumber = generateFromTo.x;

            int wantedSubjects = generateFromTo.y - generateFromTo.x;

            DateTime timeStart = DateTime.Now;

            while (true)
            {
                if (subjectNumber >= generateFromTo.y) break;

                int wantedDigits = Mathf.FloorToInt(Mathf.Log10(generateFromTo.y)) + 1;
                if (generateFromTo.z > 0)
                    wantedDigits = generateFromTo.z;
                ExperimentLogger.overrideAnalysisSubfolder = subjectNumber.AddLeadingSymbols(wantedDigits, '0');
                bool completed = false;

                // completed = true;
                GenerateAll(subjectNumber, success => { completed = true; });

                yield return new WaitUntil(() => completed == true);

                subjectNumber++;

                int subjectsDone = subjectNumber - generateFromTo.x;

                TimeSpan timeElapsed = DateTime.Now - timeStart;
                float progress01 = (float)subjectsDone / wantedSubjects;

                TimeSpan timeRemaining = TimeSpan.FromSeconds(timeElapsed.TotalSeconds / progress01) - timeElapsed;

                DateTime ETA = DateTime.Now + timeRemaining;

                Debug.Log("{0} of {1} ({2} complete) | Time Elapsed :: {3} | Time Remaining :: {4}\nETA :: {5}"._Format(
                    subjectsDone, wantedSubjects, progress01.PercentileToPercent(), timeElapsed, timeRemaining, ETA));
            }
        }
#endif

        private void GenerateAll(int subjectNumber, Action<bool> callback)
        {
#if UNITY_EDITOR
            // subjectNumber = 0;
#endif
            StimulusManager.RandomizeLocalizers(subjectNumber, numLocalizers);
            List<int> replayLevelIDs_0Based = new List<int>();
            foreach (LocalizerInfo lI in StimulusManager.localizerInfos)
                replayLevelIDs_0Based.Add(lI.targetLevelID_1Based - 1);
            GameTimings gameTimings = Precalculator.PrecalculateAllTimings(subjectNumber, replayLevelIDs_0Based);
            StimulusManager.GenerateQueues_NEW(isFMRI, subjectNumber, gameTimings.GenerateTotalLogs(), numGameWorlds, numLevelsPerWorld, callback);
        }

        private static bool GetSubjectSequenceFolderPath(int subjectID, out string subjectSequenceFolderPath)
        {
            subjectSequenceFolderPath = "";

            string baseFolderPath = ExperimentLibraryManager.dataRootDirectory + "/Sequences/" + module + "/";

            // Pick the right sequence
            List<string> subFolders = FileWrapper.GetFolderContents(baseFolderPath, FilesFolders.Folders, true);
            if (subFolders.Count == 0)
            {
                Debug.LogError("CRITICAL FAILURE ; NO FOLDERS IN " + baseFolderPath);
                return false;
            }

            int idx = subjectID % subFolders.Count;

            subjectSequenceFolderPath = subFolders[idx] + "/";

            return true;
        }

        private static void InitializePlayerProgression(ModuleType system)
        {
            // 200619 SPAGHETTI TODO This loading from config then feeding to player progression
            int numGameWorlds =

                system == ModuleType.FMRI_Screening ? // Override
                    ExperimentLibraryManager.Config.FMRI.numWorldsFMRIScreening :
                system == ModuleType.FMRI_Preparation ? // Override
                    ExperimentLibraryManager.Config.FMRI.numWorldsFMRIPrep :

                system == ModuleType.MEEG_Screening ? // Override
                    ExperimentLibraryManager.Config.MEEG.numWorldsScreening :
                system == ModuleType.MEEG_Preparation ?
                    ExperimentLibraryManager.Config.MEEG.numWorldsPrep :

                system == ModuleType.ECOG ? // Do half
                    Mathf.CeilToInt(ExperimentLibraryManager.Config.PlayerProgression.levelsLibrary.numWorldsFullGame / 2f) :
                    ExperimentLibraryManager.Config.PlayerProgression.levelsLibrary.numWorldsFullGame;


            int numLevelsPerWorld =

                system == ModuleType.FMRI_Screening ?
                    ExperimentLibraryManager.Config.FMRI.numLevelsPerWorldFMRIScreening :
                system == ModuleType.FMRI_Preparation ?
                    ExperimentLibraryManager.Config.FMRI.numLevelsPerWorldFMRIPrep :

                system == ModuleType.MEEG_Screening ?
                    ExperimentLibraryManager.Config.MEEG.numLevelsPerWorldScreening :
                system == ModuleType.MEEG_Preparation ?
                    ExperimentLibraryManager.Config.MEEG.numLevelsPerWorldPrep :

                    ExperimentLibraryManager.Config.PlayerProgression.levelsLibrary.numLevelsPerWorld;


            int numLocalizersPerGameWorld =

                system == ModuleType.FMRI_Screening ?
                    ExperimentLibraryManager.Config.FMRI.numLocalizersPerWorldFMRIScreening :
                system == ModuleType.FMRI_Preparation ?
                    ExperimentLibraryManager.Config.FMRI.numLocalizersPerWorldFMRIPrep :

                system == ModuleType.MEEG_Screening ?
                    ExperimentLibraryManager.Config.MEEG.numLocalizersPerWorldScreening :
                system == ModuleType.MEEG_Preparation ?
                    ExperimentLibraryManager.Config.MEEG.numLocalizersPerWorldPrep :

                    ExperimentLibraryManager.Config.PlayerProgression.levelsLibrary.numLocalizersPerWorld;

            float normalToVibratingRatio = ExperimentLibraryManager.Config.PlayerProgression.levelMasterManager.gameManager.normalToVibratingRatio;
            bool allowProceedWithFailure = ExperimentLibraryManager.Config.Experiment.GetAllowProceedWithFailure(module.ToPrepVsFull());
            PlayerProgression.Initialize(ExperimentLibraryManager.Config.PlayerProgression, new PlayerProgression.RuntimeConfig(
                    numGameWorlds, numLevelsPerWorld, numLocalizersPerGameWorld, normalToVibratingRatio, allowProceedWithFailure));

            if ((system == ModuleType.FMRI_Scanner && (!ExperimentLibraryManager.Config.FMRI.scannerShowTutorial_I || !ExperimentLibraryManager.Config.FMRI.scannerShowTutorial_T)) ||
                (system == ModuleType.MEEG && (!ExperimentLibraryManager.Config.MEEG.fullVersion_ShowTutorial_I || !ExperimentLibraryManager.Config.MEEG.fullVersion_ShowTutorial_T)))
                PlayerProgression.Unlock_TutorialP();
        }

        private void ResetSession()
        {
            PlayerProgression.Clear();
            DifficultyManager.SubjectReset();

            if (experimentSessionManager != null)
            {
                experimentSessionManager?.Dispose();
                experimentSessionManager = null;
            }
        }

        protected override void DeInitialize()
        {
            base.DeInitialize();

            ResetSession();

            WavePlayerManager.Dispose();

            InputManager.DeInitializeHighAccuInput();
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;

            if (fmriTrigger_Keyboard != null)
            {
                fmriTrigger_Keyboard.DeInitialize();
            }

            if (fmriTrigger_Serial != null)
            {
                fmriTrigger_Serial.DeInitialize();
            }

            //EyeLinkManager.onSacadaDetected -= EyeLinkManager_onSacada;

            this.LogWarning("EXPERIMENT MANAGER APPLICATION DISPOSED!");
        }

        private void ApplicationQuit()
        {
            this.Log("Ba-Bai!");
#if UNITY_EDITOR_OSX
#elif UNITY_STANDALONE_OSXs
#elif PLATFORM_STANDALONE_OSX
#else
            Application.Quit();
#endif
        }

        private IEnumerator CleanUp_FinalIE()
        {
            // Give it a second (we may have triggers running)
            yield return new WaitForSeconds(ExperimentLibraryManager.Config.PostLevelCleanupSafety);

            yield return StartCoroutine(experimentSessionManager.StopTracking());

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            // Get back to LevelSelection
            SceneManager.LoadSceneAsync(LEVELSELECTION_SCENE_NAME);
        }
        
        /*ExperimentManagerApplication.OnApplicationQuit vs Singleton.OnDestroy (dont double-dispose things)
#if UNITY_EDITOR_OSX
#elif UNITY_STANDALONE_OSXs
#elif PLATFORM_STANDALONE_OSX
#else
        private void OnApplicationQuit()
        {
            // Can't log outside levels
            //SPR_EXPE.LogLevelData("TheManager", "APPLICATION_QUIT");

            LevelConfig level = PlayerProgression.ActiveLevel;
            if (level == null) return;
        }
#endif
        */
        #endregion


        #region Init (Scene)

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            loadingLevelOperation = null;
            //Debug.LogError("Loaded scene:" + arg0.name);
            LeanTween.cancelAll();

            string sceneName = arg0.name;

            // [HACK] There are multiple MenuManager handlers attached to 'MenuManagerGame_OnGameExitRequested'
            InitializeManagers(sceneName);

            // [SOS] Do this step last
            if (arg1 == LoadSceneMode.Single)
                CURRENT_SCENE = arg0.name;
        }

        void Update()
        {
            if (IS_IN_GAME &&
                (ExperimentLibraryManager.Config.Input.EXPERIMENTER_ALLOW_COMPLETE_LEVELS || Debug.isDebugBuild) &&
                InputManager.GetKeyUp(ExperimentLibraryManager.Config.Input.EXPERIMENTER_COMPLETE_LEVEL_KEY))
                PlayerProgression.CompleteCurrentLevel();

            // For debugging shift & scale operations
            // if (Debug.isDebugBuild) Debug.Log(Utility_Helper.GetRelativeScreenPosition(
            //  Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono), Camera.main).ToString("#.00"));

            UpdateEyeTracker();

            // if (StimulusManager.isShowingStimulus) Debug.LogError("SHOWING");

            if (debugTimeText)
                debugTimeText.text = experimentSessionManager?.subject == null ? "" :
                    "{0} ({1}{2}) THIS is frame #{3} | LAST frame (#{4}) got rendered @{5}{6}{7}"._Format(
                        experimentSessionManager?.subjectID, module,
                        PlayerProgression.ActiveLevel != null ? " level {0}"._Format(PlayerProgression.ActiveLevel.levelID_1Based) : "",
                        TimeWrapper.currentFrameCycleID,
                        TimeWrapper.numRenderedFrames,
                        TimeWrapper.lastRenderedFrame_TimeOfRenderMS.ToString("#"),
                        TimeWrapper.timeScale_NotTS != 1 ? " | {0}x SPEED"._Format(TimeWrapper.timeScale_NotTS.ToString("#.00")) : "",
                        EXPERIMENTER_AUTO_ANSWER ? " | AUTO-PROCEED" : "");

            if (InputManager.GetKeyUp(ExperimentLibraryManager.Config.Input.EXPERIMENTER_LEVELSELECTION_LOCK))
                RaiseOnLevelStartLockToggled(true);
            else if (InputManager.GetKeyUp(ExperimentLibraryManager.Config.Input.EXPERIMENTER_LEVELSELECTION_UNLOCK))
                RaiseOnLevelStartLockToggled(false);

            if (Debug.isDebugBuild || ExperimentLibraryManager.Config.allowTimeScaleOutsideDebug)
            {
                if (InputManager.GetKeyUp(ExperimentLibraryManager.Config.Input.EXPERIMENTER_TIMESCALE_FAST))
                    experimentSessionManager.SetTimeScaleFast(InputManager.GetKey(ExperimentLibraryManager.Config.Input.EXPERIMENTER_TIMESCALE_MODIFIER));
                else if (InputManager.GetKeyUp(ExperimentLibraryManager.Config.Input.EXPERIMENTER_TIMESCALE_SLOW))
                    experimentSessionManager.SetTimeScaleSlow(InputManager.GetKey(ExperimentLibraryManager.Config.Input.EXPERIMENTER_TIMESCALE_MODIFIER));
                else if (InputManager.GetKeyUp(ExperimentLibraryManager.Config.Input.EXPERIMENTER_TIMESCALE_NORMAL))
                    experimentSessionManager.ResetTimeScale(!InputManager.GetKey(ExperimentLibraryManager.Config.Input.EXPERIMENTER_TIMESCALE_MODIFIER));
            }

            if (InputManager.GetKeyUp(ExperimentLibraryManager.Config.Input.EXPERIMENTER_DEBUG_TIME))
                debugTimeText.enabled = !debugTimeText.enabled;

            if (fmriTrigger_Serial != null)
                fmriTrigger_Serial.Update_NotTS();
        }

        private void InitializeMenuManagerFromScene()
        {
            menuManager = Utility_Helper.GetComponentInScene<MenuManager>();
            if (menuManager == null) return;

            // Sub to type-specific events!
            MenuManager_MainMenu mmMainMenu = menuManager as MenuManager_MainMenu;
            if (mmMainMenu != null)
            {
                mmMainMenu.onApplicationQuitRequested += MmMainMenu_onApplicationQuitRequested;
                
                // If we already know which module we'll pick, go now
                if (MODULE_TO_LOAD_CONFIG == ModuleType.ECOG) // Those do not have a practice mode
                    SetPrepVsFull(PrepVsFull.Full);
                // Otherwise let the UI take care of it (hide subject window)
                else
                    mmMainMenu.ToggleSubjectWindow(false);

                menuManager.Initialize();
            }

            MenuManager_LevelSelection mmLobby = menuManager as MenuManager_LevelSelection;
            if (mmLobby != null)
            {
                mmLobby.preventReturnDialogue = isFMRI && CURRENT_LEVEL != null &&
                    ExperimentManagerSession.FMRI_IsLevelFirstHalfOfRun(CURRENT_LEVEL) &&
                    PlayerProgression.HasCompletedGameLevel(CURRENT_LEVEL.levelID_1Based) &&
                    ExperimentLibraryManager.Config.FMRI.preventEscapeDuringNullEvent;

                mmLobby.onGameStartRequested += MenuManagerLobby_OnGameStartRequested;
                mmLobby.onMenuExitRequested += MenuManagerLobby_OnMenuExitRequested;
                mmLobby.onApplicationQuitRequested += MenuManagerLobby_OnApplicationQuitRequested;
                mmLobby.onDestroy += MmLobby_onDestroy;

                menuManager.Initialize();
            }

            MenuManager_Game mmGame = menuManager as MenuManager_Game;
            if (mmGame != null)
            {
                // mmGame.onGameRestartRequested += MenuManagerGame_OnGameRestartRequested;
                mmGame.onGamePauseToggleRequested += MenuManagerGame_OnGamePauseToggleRequested;
                mmGame.onGamePauseRequested += MenuManagerGame_OnGamePauseRequested;
                mmGame.onGameResumeRequested += MenuManagerGame_OnGameResumeRequested;
                mmGame.onGameExitRequested += MenuManagerGame_OnGameExitRequested;
                mmGame.onDestroy += MmGame_onDestroy;

                menuManager.Initialize();
            }

            /// SOS  using <see cref="MenuManager.Initialize"/> outside these ifs may initialize the POPUP !
        }

        private void MenuManagerLobby_OnMenuExitRequested(object sender, EventArgs<int> e)
        {
            SceneManager.LoadScene(MAINMENU_SCENE_NAME);
        }

        private void InitializeManagers(string sceneName)
        {
            InputManager.ToggleForceNormalInput(true);

            if (sceneName.ContainsInvariant("loadingscreen"))
            {
                CURRENT_LEVEL = null;
            }
            else if (sceneName.ContainsInvariant("preloader"))
            {
                CURRENT_LEVEL = null;
            }
            else if (sceneName.ContainsInvariant("mainmenu"))
            {
                ResetSession();
                InitializeMenuManagerFromScene();
                CURRENT_LEVEL = null;
                // 200623 EyeTracker_DeInitialize();
            }
            else if (sceneName.ContainsInvariant("levelselection"))
            {
                // In the middle of an FMRI run
                if (!FadeSceneController.isActive)
                    experimentSessionManager.ToggleCursor(true);

                WavePlayerManager.Stop(); // Kill any remaining audio

                // Were we in a level before?
                if (CURRENT_SCENE.ContainsInvariant("game")) // SPR_EXPE.probeSummary.journeySum.TotalJourneyRunTime )
                {
                    StartCoroutine(PostLevelLoggingIE(CURRENT_LEVEL));
                }

                ToggleUI_LevelSelection(CURRENT_LEVEL, LAST_LEVEL_COMPLETE_ARGS?.endReason == EndReason.Exit);
                InitializeManagers_LevelSelection();
                CURRENT_LEVEL = null;
            }
            // First load the game
            else if (sceneName.ContainsInvariant("game"))
            {
                // [SOS] Set this before anything else is initialized
                CURRENT_LEVEL = TEMP_LEVEL_SELECTED;
                TEMP_LEVEL_SELECTED = null;
                InitializeManagers_Game(CURRENT_LEVEL);
            }
        }

        private void InitializeManagers_LevelSelection()
        {
            SoundSystem.SetAudioToneVolume(0);

            StreamingAssetsManager.ClearCache();

            if (ExperimentLibraryManager.Config.deInitializeHighAccuInputInMenu)
                InputManager.DeInitializeHighAccuInput();
            
            InitializeMenuManagerFromScene();

            // Close input for a sec
            InputManager.ToggleLock(true);
            Invoke("EnableInput", ExperimentLibraryManager.Config.levelSelectionInputSafetyLockSeconds);
        }

        private void EnableInput()
        {
            InputManager.ToggleLock(false);
        }

        private void InitializeManagers_Game(LevelConfig levelConfig)
        {
            // PlayerProgression.levelManager.Pause(true);
            InitializeManagers_Peripherals(levelConfig);

            ExperimentManagerSession.onProbesComplete += ExperimentSessionManager_onProbesComplete;

            eyeTrackingDuringLevel_HadSomeData = 0;
            eyeTrackingDuringLevel_HadNoData = 0;
            eyeTrackingDuringLevel_HadNoConnection = 0;
            eyeTrackingDuringLevel_NumFrames = 0;

            InitializeMenuManagerFromScene();
            InputManager.ToggleForceNormalInput(false);
            InputManager.InitializeHighAccuInput();

            // If we did not show instructions

            // Debug.LogError(typeof(ExperimentManagerApplication) + " REQUEST POPUP");

            Vector2 offset01 = Utility_Helper.GetScreenOffset();

            // FIRST PREP
            experimentSessionManager.SetAndPrepareLevel(levelConfig,
                ExperimentLibraryManager.Config.Experiment.stimulus.background.cameraSize, offset01);

            ShowInstructionsIfNeeded(levelConfig,
            
                // When we've already shown a popup
                () => 
                {
                    experimentSessionManager.RequestStartCurrentLevel(); // Probably always start paused
                },

                // When we didn't have to show a popup
                () =>
                {
                    // Let it cool for a sec
                    // Debug.Log(TimeWrapper.GetCurrentTimestamp_TS());
                    AsyncThread.RunOnMainThread_AfterFrameCycles_TS(() =>
                    {
                        // Debug.Log(TimeWrapper.GetCurrentTimestamp_TS());
                        experimentSessionManager.RequestStartCurrentLevel(); // Probably always start paused
                    }, ExperimentLibraryManager.Config.Experiment.numFramesDelayStartCurrentOnNoInstructionLevels);
                });

            ToggleUI_LevelBegin();
        }

        private void ToggleUI_LevelBegin()
        {
            experimentSessionManager?.ToggleEyeTrackingUI(ExperimentLibraryManager.Config.UI.showEyeTrackingDuringLevel);
            muteCG?.Toggle(ExperimentLibraryManager.Config.UI.showMuteDuringLevel);
        }

        private void ToggleUI_LevelSelection(LevelConfig lastLevel, bool wasAborted)
        {
            if (isFMRI && !wasAborted && lastLevel != null && ExperimentManagerSession.FMRI_IsLevelFirstHalfOfRun(lastLevel.levelID_1Based))
            {
                this.LogWarning("No changes to UI during null event");
                return;
            }

            experimentSessionManager?.ToggleEyeTrackingUI(true);
            muteCG?.Toggle(true);
        }

        private static void InitializeManagers_Peripherals(LevelConfig levelConfig)
        {
            // Setup inputs
            bool isSecondHalfOfGame = PlayerProgression.IsLevelInSecondHalf(levelConfig);
            int subjectID = experimentSessionManager.subject.subjectNumber;
            bool? uiStartFlipped = experimentSessionManager.subject.startFlippedUI;
            bool isFlipped = ExperimentLibraryManager.Config.Input.GetFlipped(isSecondHalfOfGame, subjectID, uiStartFlipped);

            if (isFlipped != wasFlipped)
            {
                ExperimentManagerSession.LogSessionInfo("Inputs From {0} to {1}"._Format(
                    wasFlipped == true ? "FLIPPED" : wasFlipped == false ? "NORMAL" : "-",
                    isFlipped ? "FLIPPED" : "NORMAL"));
                wasFlipped = isFlipped;
            }
            HandType handType = experimentSessionManager.subject.handType;
            ExperimentLibraryManager.Config.Get_InputKeyCode_Keyboard(handType).SetFlipped(isFlipped);
            ExperimentLibraryManager.Config.Get_InputKeyCode_ResponseBox(handType).SetFlipped(isFlipped);
            ExperimentLibraryManager.Config.Get_InputSerialPort_ResponseBox(handType).SetFlipped(isFlipped);

            string msg = "INPUTS {0}"._Format(isFlipped ? "FLIPPED" : "NORMAL");
            if (ExperimentLibraryManager.Config.Input.debug)
                ExperimentLibraryManager.Config.InputKeyCode.LogError(msg);
            ExperimentManagerSession.LogData_AsTheyHappen_TS(
                TimeWrapper.GetCurrentTimestamp_TS(),
                "LevelMasterManager", msg);
        }

        private void HandleGameStartRequest(LevelConfig level)
        {
            if (loadingLevelOperation != null) return;

            Action levelLoad = () =>
            {
                menuManager.DeInitialize();

                TEMP_LEVEL_SELECTED = level;

                level = experimentSessionManager.GetLevelConfig(level);

                string sceneName = GAME_SCENE_NAME_FORMAT._Format(level.GetGameType());
                // loadingLevelOperation = SceneManager.LoadSceneAsync(sceneName);
                SceneManager.LoadScene(sceneName);

                this.LogWarning("Load game:" + sceneName);

                // Stop auto forward once we are in next level
                autoForwardToNextLevel = false;
            };

            if (eyeTracker != null && 
                experimentSessionManager.GetEyeTrackingFileExists(level) && // The file for the level we're about to start exists already
                experimentSessionManager.GetEyeTrackingFileName_NoSuffix(level) != eyeTracker.currentFileName_NoSuffix // And so far we've been writing to a different one
                )
            {
                this.LogWarning("Re-starting a world we have eye data for!");

                if (ExperimentLibraryManager.Config.warnWhenReplayingOldWorlds)
                    ConfirmDialog.ShowDialog("WARNING!!!\n\nRestarting an old world may result in overwriting that world's eye tracking data.\n\nAre you sure you want to proceed?", proceed =>
                    {
                        if (proceed)
                            levelLoad();
                        else
                            this.LogWarning("Aborted");

                        ConfirmDialog.HideDialog();

                    }, ExperimentLibraryManager.Config.UI.Get_DialogueYes_OnlyProceedViaClick(module));
                else
                    levelLoad();
            }
            else
                levelLoad();

        }

        private void PauseExperiment(bool pause, bool toggleCursor = true, bool keepMusicPlaying = false, ExperimentManagerSession.PauseReason pauseReason = ExperimentManagerSession.PauseReason.Other)
        {
            experimentSessionManager.PauseExperiment(pause, toggleCursor, keepMusicPlaying, pauseReason);
        }

        private IEnumerator PostLevelLoggingIE(LevelConfig levelConfig)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LoadingScreen", LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
                yield return null;

            // Wait program to unfreeze
            yield return null;

            Slider progressBar = GameObject.Find("LoadingScreenProgressBar")?.GetComponent<Slider>();

            // And rename!
            bool aborted = LAST_LEVEL_COMPLETE_ARGS.endReason == EndReason.Exit;

            experimentSessionManager.PostLevel_WriteCacheToFile(levelConfig, aborted, progressReport =>
            {
                if (progressBar)
                    progressBar.value = progressReport.value01;
                else
                    this.LogWarning("No Progress Bar!");

                if (progressReport.isDone)
                {
                    // Debug.LogError("UNLOADING LOADING SCREEN");
                    SceneManager.UnloadSceneAsync("LoadingScreen");

                    // Not for FMRI first halves
                    if (!isFMRI || !ExperimentManagerSession.FMRI_IsLevelFirstHalfOfRun(levelConfig.levelID_1Based))
                        DisplayEyeTrackerResultsUI(levelConfig);

                    // If it was aborted, or we're outside FMRI, or we're the second run - rename
                    bool isSecondHalf = !ExperimentManagerSession.FMRI_IsLevelFirstHalfOfRun(levelConfig.levelID_1Based);
                    if (aborted || !isFMRI || isSecondHalf)
                    {
                        experimentSessionManager.RenameFullLogs(levelConfig.levelName, aborted);

                        if (isFMRI && isSecondHalf) // For FMRI 2nd runs rename the first run as well!
                        {
                            string levelName_FirstHalf = ExperimentManagerSession.FMRI_GetNameOtherHalfOfRun(levelConfig.levelName);
                            experimentSessionManager.RenameFullLogs(levelName_FirstHalf, aborted);
                        }
                    }
                }
            });
        }

        private void DeInitializeLobbyManager()
        {
            // Sub to type-specific events!
            MenuManager_LevelSelection mmLobby = menuManager as MenuManager_LevelSelection;
            if (mmLobby != null)
            {
                mmLobby.onGameStartRequested -= MenuManagerLobby_OnGameStartRequested;
                mmLobby.onApplicationQuitRequested -= MenuManagerLobby_OnApplicationQuitRequested;
                mmLobby.onDestroy -= MmLobby_onDestroy;
            }
        }

        private void DeInitializeGameManager()
        {
            MenuManager_Game mmGame = menuManager as MenuManager_Game;
            if (mmGame != null)
            {
                // mmGame.onGameRestartRequested -= MenuManagerGame_OnGameRestartRequested;
                mmGame.onGamePauseToggleRequested -= MenuManagerGame_OnGamePauseToggleRequested;
                mmGame.onGamePauseRequested -= MenuManagerGame_OnGamePauseRequested;
                mmGame.onGameResumeRequested -= MenuManagerGame_OnGameResumeRequested;
                mmGame.onGameExitRequested -= MenuManagerGame_OnGameExitRequested;
                mmGame.onDestroy -= MmGame_onDestroy;
                mmGame.DeInitialize();
            }
        }

        #endregion

        #region Handlers

        private void MmLobby_onDestroy(object sender, EventArgs e)
        {
            DeInitializeLobbyManager();
        }

        private void MmGame_onDestroy(object sender, EventArgs e)
        {
            DeInitializeGameManager();
        }

        private void MmMainMenu_onApplicationQuitRequested(object sender, EventArgs e)
        {
            ApplicationQuit();
        }

        private void MenuManagerLobby_OnApplicationQuitRequested(object sender, EventArgs e)
        {
            ApplicationQuit();
        }

        private void MenuManagerLobby_OnGameStartRequested(object sender, EventArgs<int> e)
        {
            LevelConfig level = LevelsLibrary.GetLevel(e);
            HandleGameStartRequest(level);
        }

        private static void ExperimentSessionManager_onProbesComplete(object sender, EventArgs<int> e)
        {
            // ShowProbesCompletePopup();
            if (!ExperimentLibraryManager.Config.Probes.earlyOutWorldWhenProbesComplete) return;

            EngineWrapper.StartCoroutine(SafeShowProbesCompletePopupIE());
        }

        /// <summary>
        /// Menu Manager does not know if we are paused or not and sometimes it wants to toggle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuManagerGame_OnGamePauseToggleRequested(object sender, EventArgs e)
        {
            if (LevelMasterManager.isPaused)
                MenuManagerGame_OnGameResumeRequested(sender, e);
            else
                MenuManagerGame_OnGamePauseRequested(sender, e);
        }

        private void MenuManagerGame_OnGamePauseRequested(object sender, EventArgs e)
        {
            PauseExperiment(true, true, false, ExperimentManagerSession.PauseReason.ManualPausing);
        }

        private void MenuManagerGame_OnGameResumeRequested(object sender, EventArgs e)
        {
            PauseExperiment(false, true, false, ExperimentManagerSession.PauseReason.ManualPausing);
        }

        private void MenuManagerGame_OnGameExitRequested(object sender, EventArgs e)
        {
            ExperimentManagerSession_onLevelComplete(LevelCompleteArgs.GetLevelExitArgs(CURRENT_LEVEL));
        }

        private void MuteToggle_OnValueChanged(bool arg0)
        {
            experimentSessionManager?.Mute(!arg0);
        }

        /*
        private void QuitButton_OnClick()
        {
            ApplicationQuit();
        }
        */
        #endregion
    }
}