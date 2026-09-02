using System;
using System.Collections;
using UnityEngine;
using TGP.Helpers;
using Game.Managers.GameplayManager;
using Game.Core;
using Game.Systems.Cameras; // Manages camera blocking for the level
using Peripherals.Audio; // Level Music Pause-Resume
using Game.Systems.Misc; // Level fade in
using Game.Systems.Bridges;
using System.Collections.Generic;
using Helpers.Async;

namespace Game.Managers.LevelManagers
{
    /// <summary>
    /// Assumes RUNNER
    /// Rename FMRI stuff to Start Signal or something
    /// </summary>
    public class LevelMasterManager : MonoBehaviour
    {
        /// <summary>
        /// Guaranteed callback
        /// </summary>
        public void TEMP_RequestStart(Action<bool> onStarted) { RequestStart(onStarted); }

        // SHOULD NOT BE PUBLIC
        public bool isExited { get; private set; }
        public GameManager gameManager { get; protected set; }

        protected float POST_GAME_DELAY = 1;
        public bool isGameStarted { get; private set; }

        // protected NarrationConfig config { get { return ApplicationLibrary.Config.Narration; } }

        private CameraBlockControl cameraBlockControl = null;

        private int fmriStartCount = 0;

        protected LevelConfig levelConfig { get; private set; }
        protected RuntimeConfig runtimeConfig;

        #region Public Methods
        public void CompleteLevel()
        {
            if (isExited) return;

            ExitLevel(false);
            gameManager.CompleteGame();
        }

        private void ExitLevel(bool earlyOut)
        {
            if (isExited) return;
            isExited = true;

            CancelInvoke("LevelTimeOut");

            gameManager.StopReplay();

            if (earlyOut)
                onLevelComplete?.Invoke(this, LevelCompleteArgs.GetLevelExitArgs(levelConfig));
        }

        public class RuntimeConfig
        {
            public bool doTriggers { get { return triggerManagers.Count > 0; } }

            public GameManager.RuntimeConfig gameManager;
            public float orthoSize; // <- Level manager should handle all of these things, but for now app manager does
            public LevelConfig levelConfig;
            public FadeSceneController.RuntimeConfig fadeScene;
            public bool hasIntroDim;
            public readonly List<ITrigger> triggerManagers = new List<ITrigger>();
            public bool fakeStartTrigger;
            public bool skipEndOfGameMessage;
            public bool useCameraBlock;
            public float viewportSizePercentile;
            public Color viewportBlockingColor;

            public RuntimeConfig(GameManager.RuntimeConfig gameManager, float orthoSize, LevelConfig levelConfig, FadeSceneController.RuntimeConfig fadeScene, bool hasIntroDim,
                IList<ITrigger> triggerManagers, bool fakeStartTrigger, bool skipEndOfGameMessage, 
                bool useCameraBlock, float viewportSizePercentile, Color viewportBlockingColor)
            {
                this.gameManager = gameManager;
                this.orthoSize = orthoSize;
                this.levelConfig = levelConfig;
                this.fadeScene = fadeScene;
                this.hasIntroDim = hasIntroDim;
                this.triggerManagers.Clear();
                if (triggerManagers != null)
                    this.triggerManagers.AddRange(triggerManagers);
                this.fakeStartTrigger = fakeStartTrigger;
                this.skipEndOfGameMessage = skipEndOfGameMessage;
                this.useCameraBlock = useCameraBlock;
                this.viewportSizePercentile = viewportSizePercentile;
                this.viewportBlockingColor = viewportBlockingColor;
            }
        }

        [Serializable]
        public class Config
        {
            public float levelTimeOutDuration = 5;
            public bool pauseMusicWhenPausingGame; // ApplicationLibrary.Config.Audio
            public FadeSceneController.Config fadeScene_FirstHalf;
            public FadeSceneController.Config fadeScene_SecondHalf;
            public int countToStartGame; // ApplicationLibrary.Config.FMRI.countToStartGame
            public GameManager_Runner.Config gameManager;
        }
        private Config config;

        public virtual void Initialize(Config config, RuntimeConfig runtimeConfig)
        {
            // Debug.LogError("INIT");
            this.config = config;
            this.runtimeConfig = runtimeConfig;

            levelConfig = runtimeConfig.levelConfig;
            isPaused = false;

            // Debug.LogError("YAY");

            FadeSceneController.Config fadeSceneConfig = runtimeConfig.fadeScene.isFirstHalf ?
                config.fadeScene_FirstHalf : config.fadeScene_SecondHalf;

            FadeSceneController.Initialize(fadeSceneConfig, runtimeConfig.fadeScene);

            // [SOS] level master DEPENDS on game manager existence.
            gameManager = Utility_Helper.GetComponentInScene<GameManager>();
            gameManager.onGameComplete += GameManager_onGameComplete;
            
            // -- GAME
            if (gameManager != null)
            {
                // Initialize each type differently
                gameManager.Initialize(config.gameManager, runtimeConfig.gameManager);
            }
            else
            {
                this.LogWarning("No game manager");
            }

            // -- DIMMER
            LevelIntroDimmer lID = FindObjectOfType<LevelIntroDimmer>();

            if (lID != null)
            {
                if (runtimeConfig.hasIntroDim)
                    lID.Initialize();
                else
                    lID.Destroy();
            }
            else
                this.LogWarning("No level intro dimmer");

            if (runtimeConfig.useCameraBlock)
            {
                cameraBlockControl = Utility_Helper.GetComponentInScene<CameraBlockControl>();
                cameraBlockControl.SetViewport(runtimeConfig.viewportSizePercentile);
                cameraBlockControl.SetBlockColor(runtimeConfig.viewportBlockingColor);
            }

            CancelInvoke("LevelTimeOut");
            Invoke("LevelTimeOut", config.levelTimeOutDuration);
            // Cursor.visible = false;

            /*
            // If we do NOT show instructions, check the game already
            // If we do show instructions, it will handle starting the game afterwards
            if (runtimeConfig.startPaused)
            {
                Pause(true, false);
            }
            else
            {
                // This is used to prevent the initial frame spike from messing with things.
                // If we show instructions, this isn't needed though, as the spike will be soaked up before we click "OK"
                IEnumerator CheckStartGame_NextFrame()
                {
                    yield return null;
                    CheckStart();
                };

                StartCoroutine(CheckStartGame_NextFrame());
            }
            */
            // Debug.LogError(typeof(LevelMasterManager) + " Initialize");
        }

        private void LevelTimeOut()
        {
            this.LogWarning("Level Timed Out - Exiting");
            ExitLevel(true);
        }

        public event EventHandler<LevelCompleteArgs> onLevelComplete;

        private void GameManager_onGameComplete(object sender, LevelCompleteArgs e)
        {
            onLevelComplete?.Invoke(this, e);
        }

        public virtual void DeInitialize()
        {
            if (gameManager != null)
                gameManager.DeInitialize();

            StopAllCoroutines();

            if (onTriggersReceived_TS != null)
                foreach (ITrigger triggerManager in runtimeConfig.triggerManagers)
                {
                    this.LogWarning("Unsubscribed from " + triggerManager);
                    triggerManager.onTRReceived_TS -= onTriggersReceived_TS;
                }

            Destroy(this);
        }

        /// <summary>
        /// Guaranteed callback
        /// </summary>
        private void RequestStart(Action<bool> onStarted)
        {
            // Debug.LogError(typeof(LevelMasterManager) + " REQUEST START LEVEL ");
            StartCoroutine(HandleStartRequest(onStarted));
        }

        EventHandler<EventArgs> onTriggersReceived_TS;

        /// <summary>
        /// Guaranteed callback
        /// </summary>
        private IEnumerator HandleStartRequest(Action<bool> onStarted)
        {
            if (isGameStarted)
            {
                this.LogError("Game already started - why double-start?");
                onStarted(false);
                yield break;
            }

            Action finalDoStartSuccess = () =>
            {
                gameManager.gameUI.ToggleTriggersPanel(false);
                DoStart();
                onStarted(true);
            };

            // Only the scanner needs triggers!
            if (runtimeConfig.doTriggers)
            {
                // Is it the kind of level that requires waiting? (FIRST Half of run)
                this.LogWarning("Waiting for Trigger Signals");
                Pause(); // do not allow player movement
                gameManager.gameUI.ToggleTriggersPanel(true);

                onTriggersReceived_TS = (sender, e) =>
                {
                    AsyncThread.RunOnMainThread_ASAP_TS(() =>
                    {
                        if (!this)
                        {
                            this.LogWarning("Ghost Level Manager - Ignoring trigger received.");
                            return;
                        }

                        // If we've already started, too late to discuss anything
                        if (isGameStarted)
                        {
                            this.LogWarning("Game already started! Ignoring trigger received.");
                            return;
                        }

                        fmriStartCount++;

                        if (fmriStartCount >= config.countToStartGame)
                        {
                            this.Log("SerialPortTrigger_FMRI;GameStartCount:" + fmriStartCount);

                            finalDoStartSuccess();
                        }
                        else
                        {
                            gameManager.gameUI.ToggleFMRITicks(fmriStartCount);
                        }
                    });
                };

                foreach (ITrigger triggerManager in runtimeConfig.triggerManagers)
                    triggerManager.onTRReceived_TS += onTriggersReceived_TS;

                // Fake start
                if (runtimeConfig.fakeStartTrigger)
                {
                    // this.LogWarning("Sending Fake Trigger Signals");
                    for (int i = 0; i < config.countToStartGame; i++)
                    {
                        yield return new WaitForSeconds(Utility_Helper.RandomRange(0.2f, 0.5f));
                        onTriggersReceived_TS(this, null);
                    }
                }
            }
            else
            {
                finalDoStartSuccess();
                // WEIRD 200620 FMRI Pause(true, true)
            }
        }
        
        /// <summary>
        /// The single point where the game starts
        /// </summary>
        protected virtual void DoStart()
        {
            // Cursor.visible = false;

            gameManager.StartGame();

            isGameStarted = true;
        }

        protected virtual void Update()
        {
            DebugUpdate();
        }

        public static bool isPaused { get; private set; }

        public void Pause(bool keepPlayerFree = false, bool keepMusicPlaying = false)
        {
            SetPause(true, keepPlayerFree, keepMusicPlaying);
        }

        public void UnPause()
        {
            SetPause(false);
        }

        /// <summary>
        /// [SOS] Use <see cref="Pause(bool, bool)"/> / <see cref="UnPause"/>
        /// </summary>
        private void SetPause(bool pause, bool keepPlayerFree = false, bool keepMusicPlaying = false)
        {
            // [HACK]
            if (levelConfig?.isTutorial_I == true && pause == false) // Don't let the Instructions UNpause
            {
                this.LogWarning("Tutorial Instructions prevented UNpause");
                return;
            }

            if (gameManager != null)
                gameManager.Pause(pause, keepPlayerFree);

            // Unfocus UI
            if (!pause)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

            // Begin music after menu instructions resume
            if (pause)
            {
                if (config.pauseMusicWhenPausingGame && !keepMusicPlaying)
                    SoundSystem.PauseAudioMusic();
            }
            else
                SoundSystem.ResumeAudioMusic();

            isPaused = pause;
        }

        #endregion

        /*
        protected void StopTaskIrrelevantGame()
        {
            if (probeManager == null) return;
            probeManager.onProbeShown -= ProbeManager_onProbeShown;
            probeManager.onProbeResult_TS -= ProbeManager_onProbeResult_TS;
            Destroy(probeManager);
        }

        protected void ResetGame()
        {
            if (gameManager != null)
                gameManager.Reset();
            // if (backgroundManager != null)
            //  backgroundManager.Restart();
            if (stimulusManager != null)
                stimulusManager.Restart();

            Pause(false);
        }
        */

        protected void ShowLanes(bool show)
        {
            foreach (Transform lane in (gameManager as GameManager_Runner).GetLanes())
                lane.gameObject.SetActive(show);
        }

        /*
        private string GetPowerUpInstructions()
        {
            if (levelConfig.levelID == ApplicationLibrary.Config.LevelProgression.absorptionPowerUpLevel)
            {
                return "\n\n" + ApplicationLibrary.Config.LevelProgression.absorptionPowerUpDescription;
            }
            else if (levelConfig.levelID == ApplicationLibrary.Config.LevelProgression.thunderPowerUpLevel)
            {
                return "\n\n" + ApplicationLibrary.Config.LevelProgression.thunderPowerUpDescription;
            }
            return "";
        }
        */

        #region Game Manager Event 

        //protected virtual void GameManager_OnLevelCompleted(object sender, EventArgs<Level> e)
        //{
        //    EndOfGame(e.value.levelID);
        //}

        #endregion

        #region Debug

        private static bool isDebugOrEditor { get { return Application.isEditor || Debug.isDebugBuild; } }

        private void DebugUpdate()
        {
            if (!isDebugOrEditor) return;

            //int numChoice = Input_Helper.GetNumericalChoiceUp();
            // if (numChoice >= 0)
            //    FastForwardToAct(numChoice);

            // if (Input.GetKeyDown(KeyCode.J)) Debug_FMRIFakeSignal();
        }

        #endregion
    }
}