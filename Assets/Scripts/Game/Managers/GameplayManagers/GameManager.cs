// NS_REMOVE (Known issue) | Precalculator.LevelDuration
using Experiment.Task;

// NS_SEGMENT (Audio Assist)
using Peripherals.Audio;

// NS_DEBATABLE | Interactables
using Helpers.Assets;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TGP.Helpers;
using Peripherals.Koreo;
using Game.Core;
using Helpers.Engine;
using Game.Systems.Adaptive;
using Game.Systems.Score;
using Game.UI;
using Game.Entities.Player;
using Game.Entities.Interactables;
using Game.Systems.Replay;
using Game.Systems.Misc;
using Helpers.Async;

namespace Game.Managers.GameplayManager
{
    public enum GameEvent { Start, Pause, Resume, Restart, LevelExited }

    /// <summary>
    /// [SEGMENT] Scoring (ScoreManager? <see cref="ScoreSystem"/>) | Level-related stuff -> <see cref="LevelMasterManager"/> | Spawning / Player-related stuff (maybe GameplayManager? or even SpawnManager too / helper for GP manager)
    /// [DEPRECATE] Lives still a thing?
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private float GetLevelDuration()
        {
            return (float) Precalculator.GetLevelDuration(levelConfig);
        }

        /// <summary>
        /// Does NOT INCLUDE pauses
        /// </summary>
        public static float elapsedTimeSeconds_Level_NoPauses { get; private set; }

        private const string PLAYER_RESOURCE_NAME_FORMAT = "Player/Player_{0}";
        private const string INTERACTABLE_RESOURCE_NAME_FORMAT = "Interactables/Interactable_{0}_{1}";

        private const int POOL_SIZE = 25;

        private readonly Dictionary<ObjectColorType, float> unnormalizedSpawnTypeProb_01 = new Dictionary<ObjectColorType, float>();
        private readonly Dictionary<ObjectColorType, float> normalizedSpawnTypeProb_01 = new Dictionary<ObjectColorType, float>();
        private readonly Dictionary<ObjectColorType, ObjectPooler<Interactable>> interactablesPool = new Dictionary<ObjectColorType, ObjectPooler<Interactable>>();
        private readonly List<Interactable> interactables = new List<Interactable>();

        public event EventHandler<EventArgs> onGameOver;
        public event EventHandler<EventArgs> onPlayerDamaged;

        public bool isPaused { get; protected set; }
        public bool autoSpawn { get; protected set; }
        // public bool isImmune { get; protected set; }
        public float lives { get; protected set; }
        public bool isReplay { get; private set; }
        public DifficultyManager difficultyManager { get; private set; }
        public bool isGeneratingLife = true;
        public bool isTimerPaused = false;

        [Header("Editor Objects")]
        [SerializeField] private Transform foregroundParent = null;
        // [SerializeField] private Transform cameraParent = null;

        // Inherit
        protected PlayerManager player { get; private set; }
        public ScoreSystem scoreSystem { get; private set; }
        public ReplaySystem replaySystem { get; private set; }

        public float timeRemaining
        {
            get
            {
                if (levelConfig == null) return 0;
                return GetLevelDuration() - elapsedTimeSeconds_Level_NoPauses; // (ApplicationLibrary.Config.Probes.probeShieldBeforeEndOfLevelMS / 1000f)
            }
        }

        public InGameUI gameUI { get; private set; }

        private Config config;

        protected LevelConfig levelConfig { get { return runtimeConfig.level; } }
        protected virtual bool IsAlive() { return lives > 0; }

        private bool isInitialized = false;
        private float timeSinceStart = 0;
        private float timeLastDamage = 0;

        private int skippedBeats = 0;
        private float wantedSkippedBeats_Remainder = 0f;

        private float lastTimeUsePowerUp = 0;
        private float delayBetweenPowerUps = 1;
        private int maxPowerUps = 1;
        private Dictionary<PowerUp, int> availablePowerUps = new Dictionary<PowerUp, int>();

        private bool EDITOR_ONLY_debugReplay = false;
        private float vibratingProbMulti = 1;
        private float timeLastUndamagedLog;

        #region Public Methods
        protected virtual string GetPlayerSuffix()
        {
            throw new NotImplementedException();
        }

        public virtual void Initialize(Config config, RuntimeConfig runtimeConfig)
        {
            this.config = config;
            this.runtimeConfig = runtimeConfig;

            // Create the Player
            GameObject temp = ResourceHelper.InstantiateResource<GameObject>(PLAYER_RESOURCE_NAME_FORMAT._Format(GetPlayerSuffix()));
            player = temp.GetComponent<PlayerManager>();
            player.transform.ReparentAndReset(foregroundParent);

            // Sub to their events
            player.onDamaged += Player_OnDamaged;
            player.onAbsorb += Player_OnAbsorb;
            player.onRequestPowerUp_Thread += Player_onRequestPowerUp_TS;

            player.Initialize(config.playerManager, runtimeConfig.player);
            
            this.Log("Init!");
            isInitialized = true;
            elapsedTimeSeconds_Level_NoPauses = 0;

            this.player = player;

            // ----- SUPPORT SYSTEMS

            scoreSystem = Utility_Helper.GetComponentInScene<ScoreSystem>();
            //scoreSystem.Initialize(level, ApplicationLibrary.WantedFacesPerLevel(level.levelID));
            // Test faster
            scoreSystem.Initialize(runtimeConfig.score);
            scoreSystem.onScoreChanged += ScoreSystem_onScoreChanged;

            AudioSource koreographerSource = KoreographyWrapper.InstantiateKoreographer(runtimeConfig.koreo);
            SoundSystem.SetKoreographySource(koreographerSource, runtimeConfig.audioMusic);

            float levelDuration = GetLevelDuration();
            float trackDuration = (koreographerSource?.clip?.length).Value;

            string msg = "Level Length :: {0}sec || Track Duration :: {1}sec".
                _Format(levelDuration, trackDuration);

            /*
            if (levelDuration > trackDuration)
                this.LogError(msg);
            else
            */
            this.LogWarning(msg);

            if (config.decoupleGameFromMusic)
            {
                StartCoroutine(SpawnEssencesIE());
            }
            else
            {
                KoreographyWrapper.onRhythm += KoreographyEventWrapper_onRhythm;
                KoreographyWrapper.onMelody += KoreographyEventWrapper_onMelody;
            }

            // --- UI
            gameUI = FindObjectOfType<InGameUI>();
            gameUI.Initialize(config.inGameUI);
            gameUI.SetMaxLives(levelConfig.maxLives);

            // ----- PLAYER

            // No more difficulty increment inside level?!
            difficultyManager = new DifficultyManager(runtimeConfig.difficulty);

            replaySystem = this.gameObject.AddComponentIfNotExists<ReplaySystem>();
            replaySystem.Initialize();

            ReplaySystem.onInteractableSpawn += ReplaySystem_onInteractableSpawn;
            ReplaySystem.onPlayerUsePowerUp += ReplaySystem_onPowerUpChange;

            player.AssignReplaySystem(replaySystem);

            // Load all prefabs from disk
            CachePrefabs();

            timeSinceStart = 0;

            // BACKGROUND

            // -- BACKGROUND
            string bgHex = (levelConfig.GetGameType() == GameType.Runner_2D_Blue) ?
                            config.blueWorldBGColorHex :
                            config.orangeWorldBGColorHex;
            float bgBrightness = (levelConfig.GetGameType() == GameType.Runner_2D_Blue) ?
                            config.blueWorldBGBrightness :
                            config.orangeWorldBGBrightness;
            float bgContrast = (levelConfig.GetGameType() == GameType.Runner_2D_Blue) ?
                            config.blueWorldBGContrast :
                            config.orangeWorldBGContrast;
            Color bgColor = bgHex.ToColor();

            SpriteRenderer backgroundImage = GameObject.FindGameObjectWithTag("Background")?.GetComponent<SpriteRenderer>();
            if (backgroundImage == null) this.LogError("No Background Image! Tag something with Background.");
            backgroundImage.material.SetColor("_Color", bgColor);
            backgroundImage.material.SetFloat("_Brightness", bgBrightness);
            backgroundImage.material.SetFloat("_Contrast", bgContrast);
            ClearUnormalizedProbabilities();

            // === START

            PlaySFX(config.audio.GameStart);
            gameUI.SetLives(levelConfig.maxLives);
            SetLives(levelConfig.lives);
            difficultyManager.StartLevel();
            Reset();

            availablePowerUps.Clear();
            foreach (PowerUp pu in Enum.GetValues(typeof(PowerUp)))
            {
                availablePowerUps.Add(pu, 0);
            }
        }
        
        public virtual void DeInitialize()
        {
            this.Log("De-Init!");
            player.DeInitialize();
            replaySystem.DeInitialize();

            // if (SPR_EXPE.isFMRI)
            {
                gameUI.Toggle(false);
                gameUI.scoreSystemUI.Toggle(false);
            }

            KoreographyWrapper.onRhythm -= KoreographyEventWrapper_onRhythm;
            KoreographyWrapper.onMelody -= KoreographyEventWrapper_onMelody;
            ReplaySystem.onInteractableSpawn -= ReplaySystem_onInteractableSpawn;
            ReplaySystem.onPlayerUsePowerUp -= ReplaySystem_onPowerUpChange;

            // [SOS] Breaks the tutorial! this.Log(string.Format("Spawned:{0} | Recorded:{1}", ___TEMP_NUMBEROFSPAWNS, ReplaySystem.recordedLevelsPerWorld[0].GetTotalNumberOfSpawns()));
            ___TEMP_NUMBEROFSPAWNS = 0;
        }

        public void StartGame()
        {
            if (runtimeConfig.replaySystem.shouldRecord)
                replaySystem.TryStartRecording(levelConfig.levelName, levelConfig.levelID_1Based);

            StartCoroutine(FillPowerUpsIE());
            StartCoroutine(UpdateProbabilitiesIE());
        }

        public static void SetOverridePerformance_LocalizerOnly(float p)
        {
            DifficultyManager.SetOverridePerformance_LocalizerOnly(p);
        }

        public static float GetCurrentPerformance_GameOnly()
        {
            return DifficultyManager.dPrime_Final;
        }

        public virtual void Reset()
        {
            this.Log("Reset!");

            difficultyManager.ResetScore();
            player.Restart();
            // isImmune = false;

            ClearInteractables();

            // [SOS] Only the GameManager's Restart chains its UnPause
            // Fire an Unpause
            Pause(false);
        }

        public virtual void Pause(bool doPause, bool keepPlayerFree = false)
        {
            this.Log("Pause -> {0}"._Format(doPause.BoolToOnOff()));
            isPaused = doPause;
            autoSpawn = !doPause;

            // Toggle everyone's pause
            player.Pause(doPause);
            scoreSystem.Pause(doPause);
            replaySystem.Pause(doPause);

            for (int i = 0; i < interactables.Count; i++)
                interactables[i].Pause(doPause);

            if (keepPlayerFree)
                player.Pause(false);
        }

        public virtual void ClearInteractables()
        {
            for (int i = 0; i < interactables.Count; i++)
            {
                FreeInteractable(interactables[i]);
            }
        }

        public virtual void SetLives(float lives)
        {
            this.lives = Mathf.Min(lives, levelConfig.maxLives);
            gameUI.SetLives(lives);
        }

        /*
        public void SetFlatIncreaseDifficulty(float newDifficulty)
        {
            difficultyManager.SetFlatIncreaseDifficulty(newDifficulty);
        }

        public void SetOverrideDifficulty(float newDifficulty)
        {
            difficultyManager.SetOverridePerformance(newDifficulty);
        }

        public void UnsetOverrideDifficulty()
        {
            SetOverrideDifficulty(-1);
        }
        */

        /*
        public void SetIsImmune(bool isImmune)
        {
            this.isImmune = isImmune;
        }
        */
        private RuntimeConfig runtimeConfig;

        public event EventHandler<LevelCompleteArgs> onGameComplete;

        public void CompleteGame()
        {
            int stars = scoreSystem.GetStars();
            difficultyManager.EndLevel(elapsedTimeSeconds_Level_NoPauses);
            float scorePercentile = scoreSystem.GetScorePercentile_TS();

            bool success = runtimeConfig.checkLevelSuccess(levelConfig);

            onGameComplete?.Invoke(this, new LevelCompleteArgs(success ? EndReason.Success : EndReason.Failure,
                levelConfig, scoreSystem.score, stars, scorePercentile, elapsedTimeSeconds_Level_NoPauses));
        }

        public virtual void BeginReplay()
        {
            isReplay = true;

            // L1 (id 100) -> World 1 (id 0);
            // L2 (id 101) -> World 2 (id 1);
            // L3 (id 102) -> World 3 (id 2);
            // L4 (id 103) -> World 4 (id 3);

            // string replayLevelName = string.Format("{0}_1", localizerIndex + 1);
            //int replayWorldID = currentLevel.localizerID; // both are zero based
            
            replaySystem.LoadReplay(runtimeConfig.replaySystem.replayLevelName);
            replaySystem.StartReplay();

            /* Given by REPLAY no need to do here too
            SPR_EXPE.LogData_AtNextRenderedFrame_TS("GAME_MANAGER", "REPLAY_START;{0};{1};{2}"._Format(
                levelConfig.localizerID_0Based, replayLevelName,
                "[0] localizerIndex, [1] replayWorldID"));
            */

            player.ListenToReplaySystem(true);

            // SetOverrideDifficulty(runtimeConfig.baseDiff);

            scoreSystem.Reset();
            gameUI.Toggle(false);
            gameUI.scoreSystemUI.Toggle(false);
        }

        public void StopReplay()
        {
            isReplay = false;
            replaySystem.StopReplay();
            player.ListenToReplaySystem(false);
        }

        public void ClearUnormalizedProbabilities()
        {
            unnormalizedSpawnTypeProb_01.Clear();
            normalizedSpawnTypeProb_01.Clear();
        }

        public virtual Interactable SpawnInteractable(Vector3 position, ObjectColorType colorType, bool forceSpawn = false)
        {
            Interactable interactable = PopInteractable(colorType);
            if (interactable == null) return null;

            // [TODO] Why is this happening? How is possible to Pop an already moving object?
            if (!interactable.isVisible && interactable.canMove)
            {
                this.LogWarning("Did we pull an active object?");

                // Try again
                return null;
            }

            // Initialize the Interactable
            interactable.SetPosition(position);
            interactable.onOutOfVisibility += Interactable_onOutOfVisibility;

            //interactable.SetMotionMultiplierMain(config.fallingSpeedMultiplier * GetDifficultyMultiplier());
            //interactable.SetMotionMultiplierSecondary(config.sideSpeedMultiplier * GetDifficultyMultiplier());
            interactable.SetMotionMultiplierMain(config.fallingSpeedMultiplier);
            interactable.SetMotionMultiplierSecondary(config.sideSpeedMultiplier);
            interactable.Toggle(true);

            replaySystem.AddInteractableSpawn(interactable);

            ___TEMP_NUMBEROFSPAWNS++;
            this.Log(string.Format("Game spawn at:{0} | numberOfSpawn:{1}", elapsedTimeSeconds_Level_NoPauses, ___TEMP_NUMBEROFSPAWNS));
            return interactable;
        }

        public void SetOrUpdateNormalizedProbabilities_Additive01(ObjectColorType type, float prob)
        {
            // [TEMP]
            bool isVibrating = type == ObjectColorType.BlueHigh || type == ObjectColorType.OrangeHigh;
            if (isVibrating)
            {
                // Scale it down
                prob /= config.normalToVibratingRatio;
            }

            prob = prob.Clamped01();

            unnormalizedSpawnTypeProb_01.AddOrUpdate(type, prob);
            normalizedSpawnTypeProb_01.AddOrUpdate(type, prob * (isVibrating ? vibratingProbMulti : 1));
        }

        public virtual GameType GetGameType() { return GameType.None; }

        protected virtual void HandleDamage(GameObject objectDamaged, float damage)
        {
            SetLives(lives - damage);
        }

        #endregion

        #region Power Ups

        protected virtual void UsePowerUp_NotTS(PowerUp powerUp)
        {
            if (!isReplay)
            {
                if (TimeWrapper.time_NotTS - lastTimeUsePowerUp < delayBetweenPowerUps)
                    return;

                // Check if PowerUp is available
                if (availablePowerUps[powerUp] == 0)
                    return;
            }

            switch (powerUp)
            {
                case PowerUp.Thunder:
                    UseThunderPowerUp();
                    break;
                case PowerUp.Absorption:
                    UseAbsorptionPowerUp();
                    break;
            }
        }

        protected virtual void UseThunderPowerUp()
        {
            // Find all enemy interactables
            List<Interactable> enemyInteractables = new List<Interactable>();
            ObjectColorType enemyType = (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ContainsInvariant("blue")) ? ObjectColorType.Orange : ObjectColorType.Blue;
            for (int i = 0; i < interactables.Count; i++)
            {
                if (interactables[i].colorType.ToString().ContainsInvariant(enemyType.ToString()))
                {
                    enemyInteractables.Add(interactables[i]);
                }
            }

            // Explode all interactables
            for (int i = 0; i < enemyInteractables.Count; i++)
            {
                enemyInteractables[i].Explode();
                // FreeInteractable(enemyInteractables[i]);
            }

            lastTimeUsePowerUp = TimeWrapper.time_NotTS;
            UpdatePowerUp(PowerUp.Thunder, ConsumeOrAcquire.Consume);
        }

        protected virtual void UseAbsorptionPowerUp()
        {
            // Find all same interactables
            List<Interactable> sameInteractables = new List<Interactable>();
            ObjectColorType sameType = (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ContainsInvariant("blue")) ? ObjectColorType.Blue : ObjectColorType.Orange;
            for (int i = 0; i < interactables.Count; i++)
            {
                if (interactables[i].colorType.ToString().ContainsInvariant(sameType.ToString()))
                {
                    sameInteractables.Add(interactables[i]);
                }
            }

            // Absorb all interactables
            // 1. Turn off logging for a sec
            difficultyManager.preventLogPositive = true;

            float animDuration = config.powerUpAbsorbAnimationSeconds;
            float failsafeDuration = config.powerUpAbsorbFailSafeSeconds >= 0 ?
                Mathf.Max(config.powerUpAbsorbFailSafeSeconds, animDuration) : -1;

            for (int i = 0; i < sameInteractables.Count; i++)
            {
                sameInteractables[i].MoveTo(animDuration, player.GetPivot());
            }

            Utility_Helper.StartTimer(animDuration, a => // TimeWrapper.deltaTime_SinceLastUpdate_NotTS * 10
            {
                if (difficultyManager != null)
                    difficultyManager.preventLogPositive = false;
            });

            if (failsafeDuration > 0)
                Utility_Helper.StartTimer(failsafeDuration, a => // TimeWrapper.deltaTime_SinceLastUpdate_NotTS * 10
                {
                    if (difficultyManager != null)
                        difficultyManager.preventLogPositive = true;

                    // Cleanup all of them
                    foreach (Interactable i in sameInteractables)
                        if (i != null && i.isVisible) // if we haven't already
                            i.Absorb();

                    if (difficultyManager != null)
                        difficultyManager.preventLogPositive = false;
                });

            lastTimeUsePowerUp = TimeWrapper.time_NotTS;
            UpdatePowerUp(PowerUp.Absorption, ConsumeOrAcquire.Consume);
        }

        private void AcquirePowerUp(PowerUp powerUp)
        {
            UpdatePowerUp(powerUp, ConsumeOrAcquire.Acquire);
        }

        private void UpdatePowerUp(PowerUp powerUp, ConsumeOrAcquire effect)
        {
            availablePowerUps[powerUp] += effect == ConsumeOrAcquire.Acquire ? 1 : -1;
            gameUI.powerUpUI.UpdatePowerUp(powerUp, effect);

            if (!isReplay && replaySystem.isRecording)
                replaySystem.RecordPowerUp(powerUp, effect);
        }

        private IEnumerator FillPowerUpsIE()
        {
            float time = 0;
            while (true)
            {
                while (isPaused || isReplay)
                    yield return null;

                time += TimeWrapper.deltaTime_SinceLastUpdate_NotTS;
                // Debug.Log(time);

                if (time > config.powerUpRegenerationSeconds)
                {
                    /*
                    int levelID = levelConfig.levelID_1Based;
                    if (isReplay)
                    {
                        // Get corresponding level id
                        levelID = ReplaySystem.activeRecordLevel.levelID_1Based;
                    }
                    */

                    // Check if power up is unlocked
                    if (runtimeConfig.powerUp_hasAbsorptionPowerUp)
                    {
                        if (availablePowerUps[PowerUp.Absorption] < maxPowerUps)
                        {
                            AcquirePowerUp(PowerUp.Absorption);
                        }
                    }

                    // Check if power up is unlocked
                    if (runtimeConfig.powerUp_hasThunderPowerUp)
                    {
                        if (availablePowerUps[PowerUp.Thunder] < maxPowerUps)
                        {
                            AcquirePowerUp(PowerUp.Thunder);
                        }
                    }

                    time = 0;
                }

                yield return null;
            }
        }

        private IEnumerator SpawnEssencesIE()
        {
            ObjectColorType objectColorType;
            while (true)
            {
                if (isReplay || isPaused || !autoSpawn) //  || ProbeSystem.isProbeShown == true
                {
                    yield return null;
                    continue;
                };

                if (normalizedSpawnTypeProb_01.Count > 0)
                {
                    objectColorType = normalizedSpawnTypeProb_01.GetRandomWeighted();
                    SpawnInteractable(Vector3.zero, objectColorType);
                }

                // Debug.Log(currentLevel.skipAverageSpawnBeatsMultiplier);
                // Debug.Log(1f / hz * currentLevel.skipAverageSpawnBeatsMultiplier);
                yield return new WaitForSeconds(1f / wantedSpawnPerSecond);
            }
        }

        float wantedSpawnPerSecond = 1;

        #endregion

        #region Event Handlers

        private void KoreographyEventWrapper_onMelody(object sender, EventArgs e) { }

        public float skipAverageSpawnBeats { get { return levelConfig.skipAverageSpawnBeats_Base * wantedSpawnPerSecond; } }

        public static int ___TEMP_NUMBEROFSPAWNS = 0;
        private void KoreographyEventWrapper_onRhythm(object sender, EventArgs e)
        {
            if (isReplay || isPaused || !autoSpawn) return;

            // 200620 [TODO] Hack to stop interactable from spawning during a probe - but why they are spawned anyways?
            // if (ProbeSystem.isProbeShown == true) return;

            //Debug.Log("skipAverageSpawnBeats:" + currentLevel.skipAverageSpawnBeats);
            float overallMultiplier = config.skippedBeatsMultiplier_Multi;
            float wantedSkippedBeats_Base = skipAverageSpawnBeats * overallMultiplier;
            float wantedSkippedBeats = wantedSkippedBeats_Base + wantedSkippedBeats_Remainder;
            if (skipAverageSpawnBeats > 0)
            {
                if (skippedBeats < Mathf.RoundToInt(wantedSkippedBeats))
                {
                    skippedBeats++;
                    return;
                }
            }

            skippedBeats = 0;
            wantedSkippedBeats_Remainder = wantedSkippedBeats - Mathf.RoundToInt(wantedSkippedBeats);
            //Debug.Log("Remainder:" + skipBeatRemainder);

            // Vector3 randomPos = GetSpawnPosition(lanes.GetRandom());

            if (normalizedSpawnTypeProb_01.Count > 0)
            {
                ObjectColorType objectColorType = normalizedSpawnTypeProb_01.GetRandomWeighted();
                SpawnInteractable(Vector3.zero, objectColorType);
            }

            // Debug
#if UNITY_EDITOR
        // UnityEditor.EditorApplication.isPaused = true;
#endif
        }

        protected virtual void Player_OnDamaged(object sender, PlayerModel.DamageEventArgs e)
        {
            if (isReplay)
            {
                if (config.audio.doReplaySounds)
                    RaiseOnPlayerDamaged();
                return;
            }

            // We're above it
            if (isPaused) return; // isImmune || 

            float damage = GetDamageAmout(e.defeatedByType);

            // Substract points
            scoreSystem.ScorePlayerDamage(e.defeatedByType);

            // Negate damage
            HandleDamage(e.objectDamaged, 0);

            if (IsAlive())
            {
                // Handle Damage
                difficultyManager.LogBadEssenceHit(damage, timeSinceStart);
                timeLastDamage = timeSinceStart;

                RaiseOnPlayerDamaged();
            }
            else
            {
                difficultyManager.LogDeathIncrement(timeSinceStart);

                scoreSystem.Pause(true);

                difficultyManager.ResetScore();
                scoreSystem.ScorePlayerDeath();

                // Dont raise GameOver - Let game continue
                RaiseOnGameOverEvent();
            }
        }

        public void PlaySFX(SoundSystem.AudioClipConfig audioToPlay, float volume = 1f)
        {
            if (config.audio.playSFX)
                SoundSystem.PlayAudio(audioToPlay, runtimeConfig.audioSFX, volume);
        }

        protected virtual void Player_OnAbsorb(object sender, EventArgs<ObjectColorType> e)
        {
            float healAmount = GetHealAmount(e.value);

            if (!isReplay || config.audio.doReplaySounds)
                PlaySFX(config.audio.GameplayReward, healAmount * 4);

            if (isReplay) return;

            difficultyManager.LogGoodEssenceCollect(healAmount, timeSinceStart);

            // --- HACK
            // Add points
            ScoreSystemRunner_Star sSRS = (scoreSystem as ScoreSystemRunner_Star);
            if (sSRS)
            {
                sSRS.ScorePlayerHealing(e);

                // The faster essences fall, the less we want to reward each essence
                //float speedMulti = GetFallingSpeedMultiplier();
                //float diffOffset = 1 / speedMulti;

                // but dont affect negative ones
            }

            SetLives(lives + healAmount);

            // [HACK] Dont give life when getting a half gray orb
            /*
            if (!e.value.colorType.ToString().ContainsInvariant("gray"))
                ApplicationLibrary.PlaySFX(ApplicationLibrary.Config.Audio.Reward, 0.3f);
            else
                ApplicationLibrary.PlaySFX(ApplicationLibrary.Config.Audio.Reward, 0.05f * healAmount);
            */

        }

        protected virtual void Player_onRequestPowerUp_TS(object sender, EventArgs<PowerUp> e)
        {
            if (isReplay) return;

            AsyncThread.RunOnMainThread_ASAP_TS(() =>
            {
                UsePowerUp_NotTS(e);
            });
        }

        private void ScoreSystem_onScoreChanged(object sender, FloatChangedArgs e)
        {
            // If it was a reset, do NOT log it
            if (e.newValue == 0 && e.diff < 0) return;

            float progressDiff = scoreSystem.ConvertScoreToProgress_TS(e.diff);

            float scoreProgress = scoreSystem.ConvertScoreToProgress_TS(scoreSystem.score);
            // Increase score multiplier when full scored
            scoreSystem.SetScoreMultiplier(scoreProgress >= 1f ? 2 : 1);

            // Did we reach a checkpoint? Did we complete the game?
            if (scoreProgress >= 1)
            {
                // New level!
                //ApplicationLibrary.PlaySFX(ApplicationLibrary.Config.Audio.Progress);
                //RaiseOnLevelCompleted();
            }
        }

        private void ReplaySystem_onInteractableSpawn(object sender, EventArgs<ReplayRecord> e)
        {
            if (!isReplay) return;

            SpawnRecord record = e.value as SpawnRecord;
            //Debug.LogError(string.Format("Creating record TimesinceLevel:{4} | time:{0} | type:{1} | id:{2} | pos:{3}", record.elapsedTime, record.interactableType, record.instanceID, record.position, Time.timeSinceLevelLoad));
            Interactable interactable = SpawnInteractable(record.position, record.interactableType);

            // Name object as instance id so we can find it later for jumping essenses
            interactable.name = record.instanceID.ToString();
        }

        private void ReplaySystem_onPowerUpChange(object sender, EventArgs<ReplayRecord> e)
        {
            if (!isReplay) return;

            PowerUpRecord record = e.value as PowerUpRecord;

            if (record.effect == ConsumeOrAcquire.Consume)
                UsePowerUp_NotTS(record.powerUp);
            else if (record.effect == ConsumeOrAcquire.Acquire)
                AcquirePowerUp(record.powerUp);
        }

        #endregion

        #region Raise Events

        //protected void RaiseOnLevelCompleted()
        //{
        //    if (OnLevelCompleted != null)
        //        OnLevelCompleted(this, currentLevel);
        //}

        protected void RaiseOnGameOverEvent()
        {
            PlaySFX(config.audio.GameOver);
            onGameOver?.Invoke(this, null);
        }

        protected void RaiseOnPlayerDamaged()
        {
            PlaySFX(config.audio.GameplayPenalty);
            onPlayerDamaged?.Invoke(this, null);
        }

        #endregion

        private static Dictionary<ObjectColorType, GameObject> interactablePrefabs = new Dictionary<ObjectColorType, GameObject>();

        private static float fallingSpeedMultiplier;
        public float badGoodEssenceRatio_Multi { get; private set; }

        private void CachePrefabs()
        {
            // Load Prefabs
            interactablePrefabs.Clear();

            foreach (ObjectColorType colorType in Utility_Helper.EnumGetValues<ObjectColorType>())
            {
                string baseName = GetGameType() == GameType.Duet ? "Duet" : "Runner_2D";
                string interactableResourceName = INTERACTABLE_RESOURCE_NAME_FORMAT._Format(baseName, colorType.ToString());
                GameObject temp = ResourceHelper.LoadResource<GameObject>(interactableResourceName);
                if (temp != null)
                    interactablePrefabs.Add(colorType, temp);
            }

            // Create Pool objects
            interactablesPool.Clear();
            foreach (ObjectColorType colorType in interactablePrefabs.Keys)
            {
                interactablesPool.Add(colorType, new ObjectPooler<Interactable>());
                for (int i = 0; i < 50; i++)
                {
                    Interactable freeObject = PopInteractable(colorType);
                }
            }

            List<Interactable> interactablesClone = new List<Interactable>(interactables);
            for (int i = 0; i < interactablesClone.Count; i++)
            {
                FreeInteractable(interactablesClone[i]);
            }
        }

        private void Update()
        {
            if (!isPaused && !isTimerPaused)
                elapsedTimeSeconds_Level_NoPauses += TimeWrapper.deltaTime_SinceLastUpdate_NotTS;

            if (gameUI != null)
                gameUI.SetTime(timeRemaining);

            if (!isPaused && isInitialized)
            {
                float dT = TimeWrapper.deltaTime_SinceLastUpdate_NotTS;

                timeSinceStart += dT;

                if (timeSinceStart - timeLastDamage > 5)
                {
                    if (timeSinceStart - timeLastUndamagedLog > 1)
                    {
                        timeLastUndamagedLog = timeSinceStart;
                        difficultyManager.LogUndamagedIncrement(timeSinceStart);
                    }
                }

                // Calculate performance only if we're not in replay
                difficultyManager.UpdateDifficulty(timeSinceStart, dT, isReplay, elapsedTimeSeconds_Level_NoPauses);

                // --- UPDATE DIFFICULTY PARAMETERS

                // Good-bad ratio
                badGoodEssenceRatio_Multi = GetBadGoodEssenceRatio();
                fallingSpeedMultiplier = GetFallingSpeedMultiplier();

                Interactable.SetGlobalSpeedMultiplier(fallingSpeedMultiplier);
                PlayerManager.SetGlobalSpeedMulti(fallingSpeedMultiplier);

                // The faster they fall, the more we need of them to keep the proper objects on screen at any given time number
                wantedSpawnPerSecond = GetEssencesPerSecond() / fallingSpeedMultiplier;

                // UpdateVibratingProbabilities();
                // Update the interactables

                // Update the UI
                gameUI.SetDifficulty(DifficultyManager.difficulty);
                gameUI.SetPerformance(DifficultyManager.dPrime_Final);

                // ------

                if (isGeneratingLife && Mathf.Abs(config.regenerationSpeedPerSecond) > 0)
                {
                    SetLives(lives + dT * config.regenerationSpeedPerSecond);
                }
            }

            // Don't end with time in Localizers
            /*
            if (timeRemaining <= 0 && !isPaused && currentLevel?.taskType == TaskType.TaskIrrelevant)
            {
                // New level!
                ApplicationLibrary.PlaySFX(ApplicationLibrary.Config.Audio.Progress);
                RaiseOnLevelCompleted();
            }
            */

            /*
            if (Debug.isDebugBuild)
            {
                if (Input.GetKeyUp(KeyCode.Z))
                {
                    UseThunderPowerUp();
                }
                if (Input.GetKeyUp(KeyCode.X))
                {
                    UseAbsorptionPowerUp();
                }
            }

            // [TEST]
            if (!Application.isEditor) return;

            if (Input.GetKeyUp(KeyCode.F8))
            {
                Debug.Log(unnormalizedSpawnTypeProb_01.ToReadableString("Probabilities Unnormalized 0..1"));
            }

            if (EDITOR_ONLY_debugReplay)
            {
                if (Input.GetKeyDown(KeyCode.Q))
                    replaySystem.TryStartRecording();
                if (Input.GetKeyDown(KeyCode.W))
                    replaySystem.StopRecording();

                if (Input.GetKeyDown(KeyCode.E))
                {
                    BeginReplay(true);
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    replaySystem.StopReplay();
                    player.ListenToReplaySystem(false);
                }
            }
            */
        }

        private void UpdateVibratingProbabilities()
        {
            // Calculate probabilities for Vibrating essences
            float percentileOfIntendedDuration = timeSinceStart / GetLevelDuration();
            float percentileOfCompletion = scoreSystem.progress;

            // Are we as complete as we should be?
            float normalizedCompleteness = percentileOfCompletion / percentileOfIntendedDuration;

            if (normalizedCompleteness >= 1 || percentileOfIntendedDuration < 0.05f || timeSinceStart < 10)
                vibratingProbMulti = 1;
            else
            {
                // Make sure we don't overdo it
                float multi = 1f / Mathf.Max(normalizedCompleteness, 0.2f);

                vibratingProbMulti = Mathf.Pow(multi, config.targetDurationCatchupPow);
            }

            // this.LogWarning("We are at {0} completion, but we should be at at least {1}. Due to normalized completeness of {2}, we get a multi of {3}".
            //    _Format(percentileOfCompletion.PercentileToPercent(), percentileOfIntendedDuration.PercentileToPercent(), normalizedCompleteness.PercentileToPercent(), vibratingProbMulti.PercentileToPercent()));

            normalizedSpawnTypeProb_01.TrySet(ObjectColorType.BlueHigh, unnormalizedSpawnTypeProb_01.TryGet(ObjectColorType.BlueHigh) * vibratingProbMulti);
            normalizedSpawnTypeProb_01.TrySet(ObjectColorType.OrangeHigh, unnormalizedSpawnTypeProb_01.TryGet(ObjectColorType.OrangeHigh) * vibratingProbMulti);
        }

        private Interactable PopInteractable(ObjectColorType colorType)
        {
            Interactable interactable = interactablesPool[colorType].GetObject();
            if (interactable == null)
                interactable = InstantiateInteractable(colorType);
            interactables.Add(interactable);
            InitializeInteractable(interactable);
            return interactable;
        }

        private Interactable InstantiateInteractable(ObjectColorType colorType)
        {
            if (!interactablePrefabs.ContainsKey(colorType))
                return null;

            this.Log("Instantiating prefab");
            GameObject prefab = interactablePrefabs[colorType];
            GameObject temp = Instantiate(prefab);

            Interactable interactable = temp.GetComponent<Interactable>();
            interactable.transform.ReparentAndReset(foregroundParent);
            InitializeInteractable(interactable);
            interactable.SetPosition(Vector3.one * 1000f);
            interactable.SetType(colorType);

            return interactable;
        }

        private void InitializeInteractable(Interactable interactable)
        {
            interactable.Initialize(config.interactable, runtimeConfig.interactable);
        }

        private void FreeInteractable(Interactable interactable)
        {
            //Debug.Log("Freeing:" + interactable.name, interactable.transform);

            interactable.Pause(true);
            interactable.Toggle(false);
            interactable.SetPosition(Vector3.one * 1000f);
            interactable.onOutOfVisibility -= Interactable_onOutOfVisibility;

            interactablesPool[interactable.colorType].FreeObject(interactable);
            interactables.Remove(interactable);
        }

        private void Interactable_onOutOfVisibility(object sender, EventArgs e)
        {
            Interactable i = sender as Interactable;
            FreeInteractable(i);

            if (isReplay) return;

            // Debug.Log(i, i);

            // Same colored? Missed opportunity
            bool wasBlue = i.colorType == ObjectColorType.Blue || i.colorType == ObjectColorType.BlueHigh || i.colorType == ObjectColorType.GrayBlue;
            bool wasOrange = i.colorType == ObjectColorType.Orange || i.colorType == ObjectColorType.OrangeHigh || i.colorType == ObjectColorType.GrayOrange;

            bool wasPositive = wasBlue && levelConfig.GetGameType() == GameType.Runner_2D_Blue ||
                wasOrange && levelConfig.GetGameType() == GameType.Runner_2D_Orange;

            float value = GetDamageAmout(i.colorType);

            if (wasPositive)
                difficultyManager.LogGoodEssenceMissed(value, timeSinceStart);
            else
                difficultyManager.LogBadEssenceAvoided(value, timeSinceStart);
        }

        /*
        protected float GetSkippedBeatsMultiplierFromDifficulty()
        {
            if (!config.tieDifficultyToSkippedBeats) return 1;

            float difficulty_01 = DifficultyManager.difficulty;

            float skippedBeatsMultiplier_05_2 = difficulty_01.RetargetedFrom0_1To05_2();
            float skippedBeatsMultiplier_Base = Mathf.Pow(skippedBeatsMultiplier_05_2,
                difficulty_01 <= 0.5f ? config.skippedBeatsMultiplier_Pow_Under_05 :
                config.skippedBeatsMultiplier_Pow_Over_05);
            float skippedBeatsMultiplier_Final = skippedBeatsMultiplier_Base * config.skippedBeatsMultiplier_Multi;

            return 1f / skippedBeatsMultiplier_Final;
        }
        */

        protected float GetEssencesPerSecond()
        {
            DifficultyKeyFrame frame = config.GetKeyFrameAt(DifficultyManager.difficulty);
            return frame.essencesPerSecond;
        }

        protected float GetBadGoodEssenceRatio()
        {
            if (!config.tieDifficultyToBadGoodEssenceRatio) return 1;

            float difficulty_01 = DifficultyManager.difficulty;

            float badGoodRatio_05_2 = difficulty_01.RetargetedFrom0_1To05_2();
            float badGoodRatio_Base = Mathf.Pow(badGoodRatio_05_2,
                difficulty_01 <= 0.5f ? config.badGoodEssenceRatio_Pow_Under_05 :
                config.badGoodEssenceRatio_Pow_Over_05);
            float badGoodRatio_Final = badGoodRatio_Base * config.badGoodEssenceRatio_Multi;

            return badGoodRatio_Final;
        }

        protected float GetFallingSpeedMultiplierFromDifficulty()
        {
            if (!config.tieDifficultyToFallingSpeed) return 1;

            float difficulty_01 = DifficultyManager.difficulty;

            float fallingSpeed_05_2 = difficulty_01.RetargetedFrom0_1To05_2();
            float fallingSpeed_Base = Mathf.Pow(fallingSpeed_05_2,
                difficulty_01 <= 0.5f ? config.fallingSpeed_Pow_Under_05 :
                config.fallingSpeed_Pow_Over_05);
            float fallingSpeed_Final = fallingSpeed_Base * config.fallingSpeed_Multi;

            return fallingSpeed_Final;
        }

        protected float GetFallingSpeedMultiplier()
        {
            DifficultyKeyFrame frame = config.GetKeyFrameAt(DifficultyManager.difficulty);
            return frame.fallingSpeed;
        }

        protected float GetDeadlockMultiplier()
        {
            DifficultyKeyFrame frame = config.GetKeyFrameAt(DifficultyManager.difficulty);
            return frame.deadlockMultiplier;
        }

        #region Bad-Good Ratios
        protected float badGoodSqrtRatio { get { return Mathf.Sqrt(badGoodEssenceRatio); } }
        private float badGoodEssenceRatio { get { return config.badGoodEssenceRatio_Base * badGoodEssenceRatio_Multi; } }
        private float goodProbabilitySqrtRation { get { return 1f * badGoodSqrtRatio; } }
        private float badProbabilitySqrtRation { get { return 1f / badGoodSqrtRatio; } }

        // [HACK] Setting probabilities in Initialize doesn't work
        private IEnumerator UpdateProbabilitiesIE()
        {
            while (true)
            {
                ClearUnormalizedProbabilities();
                SetOrUpdateNormalizedProbabilities_Additive01(ObjectColorType.Blue, (levelConfig.isBlueWorld) ? goodProbabilitySqrtRation : badGoodSqrtRatio);
                SetOrUpdateNormalizedProbabilities_Additive01(ObjectColorType.Orange, (levelConfig.isBlueWorld) ? badGoodSqrtRatio : goodProbabilitySqrtRation);

                // Enable vibrating essence
                if (runtimeConfig.enableHighEnergyEssences)
                {
                    SetOrUpdateNormalizedProbabilities_Additive01(ObjectColorType.BlueHigh, (levelConfig.isBlueWorld) ? goodProbabilitySqrtRation : badGoodSqrtRatio);
                    SetOrUpdateNormalizedProbabilities_Additive01(ObjectColorType.OrangeHigh, (levelConfig.isBlueWorld) ? badGoodSqrtRatio : goodProbabilitySqrtRation);
                }
                yield return null;
            }
        }
        #endregion

        protected float GetHealAmount(ObjectColorType interactableType)
        {
            switch (interactableType)
            {
                // Same color absorbs them (and heals) -1/+1 -> 0
                case ObjectColorType.Orange:
                case ObjectColorType.Blue:
                    return config.lifePerAbsorb_Normal;
                // Same color absorbs them (and heals) -2/+2 -> 0
                case ObjectColorType.OrangeHigh:
                case ObjectColorType.BlueHigh:
                    return config.lifePerAbsorb_Vibrating;
                // Same color absorbs them (but doesn't heal) -1/0 -> -1
                case ObjectColorType.GrayOrange:
                case ObjectColorType.GrayBlue:
                    return config.lifePerAbsorb_HalfGray;
                // No one can absorb those -1/-1 -> -2
                case ObjectColorType.Gray:
                    return 0;
            }

            return 0;
        }

        protected float GetDamageAmout(ObjectColorType interactableType)
        {
            switch (interactableType)
            {
                // Same color absorbs them (and heals) -1/+1 -> 0
                case ObjectColorType.Orange:
                case ObjectColorType.Blue:
                    return config.damagePerAbsorb_Normal;
                // Same color absorbs them (and heals) -2/+2 -> 0
                case ObjectColorType.OrangeHigh:
                case ObjectColorType.BlueHigh:
                    return config.damagePerAbsorb_Vibrating;
                // Same color absorbs them (but doesn't heal) -1/0 -> -1
                case ObjectColorType.GrayOrange:
                case ObjectColorType.GrayBlue:
                    return config.damagePerAbsorb_HalfGray;
                // No one can absorb those -1/-1 -> -2
                case ObjectColorType.Gray:
                    return config.damagePerAbsorb_Gray;
            }

            return 0;
        }

        [Serializable]
        public class Config
        {
            public float targetDurationCatchupPow = 1;
            public float fallingSpeedMultiplier = 2;

            public bool decoupleGameFromMusic = true;
            // public float spawnEssenceHz = 2;

            public bool tieDifficultyToSkippedBeats = true;
            public float skippedBeatsMultiplier_Pow_Under_05 = 1.0f;
            public float skippedBeatsMultiplier_Pow_Over_05 = 1.0f;
            public float skippedBeatsMultiplier_Multi = 1.0f;

            public bool tieDifficultyToBadGoodEssenceRatio = true;
            public float badGoodEssenceRatio_Pow_Under_05 = 1.0f;
            public float badGoodEssenceRatio_Pow_Over_05 = 1.0f;
            public float badGoodEssenceRatio_Multi = 1.0f;

            public bool tieDifficultyToFallingSpeed = true;
            public float fallingSpeed_Pow_Under_05 = 0.0f;
            public float fallingSpeed_Pow_Over_05 = 1.0f;
            public float fallingSpeed_Multi = 1.00f;

            public float sideSpeedMultiplier = .3f;
            public float objectsPerSecond = 1;
            public float postRestartSpawnDelay = 1.0f;
            public float normalToVibratingRatio = 2f;
            public float regenerationSpeedPerSecond = 0.0f;
            public float lifePerAbsorb_Normal = 0.25f;
            public float lifePerAbsorb_HalfGray = 0.125f;
            public float lifePerAbsorb_Vibrating = 0.50f;
            public float damagePerAbsorb_Normal = 1.00f;
            public float damagePerAbsorb_HalfGray = 0.50f;
            public float damagePerAbsorb_Gray = 1.00f;
            public float damagePerAbsorb_Vibrating = 2.00f;

            public string blueWorldBGColorHex = "#0000ff";
            public float blueWorldBGBrightness = 1.0f;
            public float blueWorldBGContrast = 1.0f;

            public string orangeWorldBGColorHex = "#ff3333";
            public float orangeWorldBGBrightness = 1.0f;
            public float orangeWorldBGContrast = 1.0f;

            public string masterSettings_Comment = "Multiplies the resulting Key Frame";
            public DifficultyKeyFrame masterFrame = new DifficultyKeyFrame(-1, 1, 1, 1, 1);

            public List<DifficultyKeyFrame> difficultyKeyFrames = new List<DifficultyKeyFrame>()
            {
                new DifficultyKeyFrame(0.1f, 1f, 1f, 1f, 1f),
                new DifficultyKeyFrame(0.2f, 2f, 2f, 1f, 1f),
                new DifficultyKeyFrame(1.0f, 3f, 5f, 1f, 1f)
            };
            public float powerUpRegenerationSeconds = 30;
            public GameAudioConfig audio;
            public Interactable.Config interactable;
            public float badGoodEssenceRatio_Base = 1;
            public InGameUI.Config inGameUI;
            public PlayerManager.Config playerManager;
            public string powerUpAbsorbAnimationSeconds_Comment = "Average time it takes for essences to reach the player when absorbing";
            public float powerUpAbsorbAnimationSeconds = 0.5f;
            public string powerUpAbsorbFailSafeSeconds_Comment = "Failsafe for auto-absorbing essences to prevent them from being stuck. Set to -1 to disable.";
            public float powerUpAbsorbFailSafeSeconds = 1.0f;

            public DifficultyKeyFrame GetKeyFrameAt(float difficulty)
            {
                // Check for empty entries
                if (difficultyKeyFrames.Count == 0)
                {
                    Debug.LogError("There are not any difficulty key frames to interpolate between");
                    return new DifficultyKeyFrame();
                }

                // Check for single entry
                if (difficultyKeyFrames.Count == 1)
                {
                    Debug.LogError("Not enough key frames to interpolate between");
                    return difficultyKeyFrames[0];
                }

                // Check for less than minimum
                if (difficulty <= difficultyKeyFrames[0].atDifficulty)
                {
                    return difficultyKeyFrames[0];
                }

                //// Check for greater than maximum
                if (difficulty >= difficultyKeyFrames.GetLast().atDifficulty)
                {
                    return difficultyKeyFrames.GetLast();
                }

                // Find inbetween frame index
                int betweenIndex = 0;
                for (int i = 0; i < difficultyKeyFrames.Count - 1; i++)
                {
                    if (difficulty >= difficultyKeyFrames[i].atDifficulty && difficulty < difficultyKeyFrames[i + 1].atDifficulty)
                    {
                        betweenIndex = i;
                        break;
                    }
                }

                DifficultyKeyFrame frame1 = difficultyKeyFrames[betweenIndex];
                DifficultyKeyFrame frame2 = difficultyKeyFrames[betweenIndex + 1];

                float betweenVal = (difficulty - frame1.atDifficulty) / (frame2.atDifficulty - frame1.atDifficulty);
                DifficultyKeyFrame lerpedFrame = DifficultyKeyFrame.Lerp(frame1, frame2, betweenVal);
                DifficultyKeyFrame finalFrame = DifficultyKeyFrame.Multiply(lerpedFrame, masterFrame);
                /*
                Debug.LogError("Difficulty {0} is at {1} between {2} (frame {3}) and {4} (frame {5})"._Format(
                    difficulty.PercentileToPercent(), betweenVal.PercentileToPercent(), 
                    frame1.atDifficulty.PercentileToPercent(), betweenIndex, 
                    frame2.atDifficulty.PercentileToPercent(), betweenIndex + 1));
                */
                /*
                Debug.LogError("{0} :: {1}, {2}, {3}"._Format(
                    lerpedFrame.atDifficulty.PercentileToPercent(), lerpedFrame.essencesPerSecond.ToString("#.00"), 
                    lerpedFrame.fallingSpeed.ToString("#.00"), lerpedFrame.deadlockMultiplier.ToString("#.00")));
                */

                return finalFrame;
            }
        }

        public class RuntimeConfig
        {
            public LevelConfig level;

            public Func<LevelConfig, bool> checkLevelSuccess;
            public ReplaySystem.RuntimeConfig replaySystem;
            public DifficultyManager.RuntimeConfig difficulty;
            public ScoreSystem.RuntimeConfig score;
            public PlayerManager.RuntimeConfig player;
            public bool powerUp_hasAbsorptionPowerUp;
            public bool powerUp_hasThunderPowerUp;
            public bool enableHighEnergyEssences;
            public Interactable.RuntimeConfig interactable;
            public SoundSystem.RuntimeAudioSourceConfig audioSFX;
            public SoundSystem.RuntimeAudioSourceConfig audioMusic;
            public KoreographyWrapper.RuntimeConfig koreo;

            public RuntimeConfig(LevelConfig level, Func<LevelConfig, bool> checkLevelSuccess,
                ReplaySystem.RuntimeConfig replaySystem, DifficultyManager.RuntimeConfig difficulty, 
                ScoreSystem.RuntimeConfig score, PlayerManager.RuntimeConfig player,
                bool powerUp_hasAbsorptionPowerUp, bool powerUp_hasThunderPowerUp, bool enableHighEnergyEssences, 
                Interactable.RuntimeConfig interactable, SoundSystem.RuntimeAudioSourceConfig audioSFX, 
                SoundSystem.RuntimeAudioSourceConfig audioMusic, KoreographyWrapper.RuntimeConfig koreo)
            {
                this.level = level;
                this.checkLevelSuccess = checkLevelSuccess;
                this.replaySystem = replaySystem;
                this.difficulty = difficulty;
                this.score = score;
                this.player = player;
                this.powerUp_hasAbsorptionPowerUp = powerUp_hasAbsorptionPowerUp;
                this.powerUp_hasThunderPowerUp = powerUp_hasThunderPowerUp;
                this.enableHighEnergyEssences = enableHighEnergyEssences;
                this.interactable = interactable;
                this.audioSFX = audioSFX;
                this.audioMusic = audioMusic;
                this.koreo = koreo;
            }
        }
    }
}