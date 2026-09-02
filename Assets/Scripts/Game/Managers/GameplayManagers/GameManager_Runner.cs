using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;
using Game.Core;
using Game.Entities.Interactables; // These are practically Runner interactables
using Game.Entities.Player;
using Game.Systems.Replay; // It's only using its own stuff
using Game.Systems.Adaptive;
using Game.Systems.Score;
using Peripherals.Audio;
using Peripherals.Koreo;

namespace Game.Managers.GameplayManager
{
    /// <summary>
    /// [RENAME?] It just overrides the SPAWNING part of <see cref="GameManager"/> (lanes n stuff)
    /// </summary>
    public class GameManager_Runner : GameManager
    {
        [SerializeField] private Transform lanesParent = null;

        private readonly List<Transform> lanes = new List<Transform>();
        private TimeQueue<Transform> laneBlockages;// = new TimeQueue<Transform>(null);

        private ObjectColorType worldColor = ObjectColorType.Gray;
        private bool debug = false;
        private const string playerResourceSuffix = "Runner";

        private Config config;
        private RuntimeConfig runtimeConfig;

        protected override string GetPlayerSuffix()
        {
            return playerResourceSuffix;
        }

        public override void Initialize(GameManager.Config baseConfig, GameManager.RuntimeConfig baseRuntimeConfig)
        {
            base.Initialize(baseConfig, baseRuntimeConfig);

            config = baseConfig as Config;
            runtimeConfig = baseRuntimeConfig as RuntimeConfig;
            
            // [SOS] Get the lanes before initialization (affects PlayerView_Runner's Init())
            laneBlockages = new TimeQueue<Transform>(null);
            SetupLanes();
            
            gameUI.SetColor(worldColor);
            
            ReplaySystem.onInteractableJump += ReplaySystem_onInteractableJump;
        }

        private void SetupLanes()
        {
            laneBlockages.Clear();
            lanes.Clear();

            float wantedScale = config.baseLaneScale;
            wantedScale *= runtimeConfig.scaleMultiplierLanes;
            this.Log("Scaling lanes to :: {0}"._Format(wantedScale));
            lanesParent.SetLossyScale(Vector3.one * wantedScale);

            for (int i = 0; i < lanesParent.childCount; i++)
            {
                Transform lane = lanesParent.GetChild(i);
                lane.name = "Lane {0}"._Format(i);
                lanes.Add(lane);
                // Compensate for the length reduction
                lane.localScale = new Vector3(1, 1 / wantedScale, 1);
            }

            // Bit hacky 
            (player as PlayerManager_Runner).SetupLanes(lanes);
            float playerViewMovementSpeed = Mathf.Abs(lanes[0].position.x - lanes[1].position.x) / config.laneSwapDuration_CODE;
            player.SetMovementSpeed(playerViewMovementSpeed);
        }

        public List<Transform> GetLanes()
        {
            return lanes;
        }

        #region Override Logic

        public override void DeInitialize()
        {
            base.DeInitialize();

            ReplaySystem.onInteractableJump -= ReplaySystem_onInteractableJump;
        }

        public override void Reset()
        {
            // override base
            base.Reset();

            player.Restart();
            // isImmune = false;

            /// Handled in <see cref="GameManager.Restart"/>
            // ApplicationLibrary.PlaySFX(ApplicationLibrary.Config.Audio.GameStart); 

            // [SOS] Only the GameManager's Restart chains its UnPause
            // Fire an Unpause
            Pause(false);
        }

        public Vector3 GetSpawnPosition(Transform lane)
        {
            return lane.transform.position +
                Vector3.up * lane.lossyScale.y * runtimeConfig.interactable.spawnVerticalPositionScaled;
        }

        private void InteractableJumpLane(Interactable interactable, float positionX)
        {
            interactable.ChangeLane(0.5f, positionX);

            if (!isReplay)
                replaySystem.RecordInteractableJump(interactable, positionX);
        }

        public override Interactable SpawnInteractable(Vector3 spawnPosition, ObjectColorType colorType, bool forceSpawn = false)
        {
            if (isReplay || forceSpawn)
            {
                // Shouldn't we return here?
                return base.SpawnInteractable(spawnPosition, colorType);
            }

            Transform laneToSpawnInto = GetValidRandomLane();
            if (laneToSpawnInto == null)
            {
                if (debug) this.LogWarning("There was no valid lane, aborted to avoid DEADLOCK!");
                // interactable.Toggle(false);
                return null;
            }

            spawnPosition = GetSpawnPosition(laneToSpawnInto);

            // If this is a NEGATIVE object, mark this as a potential lane blockage
            if (!IsColorTypeFriendly(colorType))
            {
                if (debug) this.LogWarning("Colortype not friendly! Adding {0} to lane blockages for {1} seconds"._Format(laneToSpawnInto.name, config.blockageTimeToAvoidDeadlocks.ToString("#.00")));
                laneBlockages.AddOrUpdate(laneToSpawnInto, config.blockageTimeToAvoidDeadlocks * GetDeadlockMultiplier()); //  * ApplicationLibrary.Config.Difficulty.fallingSpeedMultiplier
            }
            //Debug.LogWarning("Spawned {0} Into lane :: {1}"._Format(colorType, laneToSpawnInto));

            Interactable interactable = base.SpawnInteractable(spawnPosition, colorType);
            if (!isReplay)
            {
                interactable.onReadyToJumpLane -= Interactable_onReadyToJumpLane;
                interactable.onReadyToJumpLane += Interactable_onReadyToJumpLane;
            }

            return interactable;
        }

        private void Interactable_onReadyToJumpLane(object sender, EventArgs e)
        {
            Interactable interactable = sender as Interactable;
            Vector3 adjacentLane = GetRandomAdjacentLane(interactable.transform.position);
            InteractableJumpLane(interactable, adjacentLane.x);
        }

        private void ReplaySystem_onInteractableJump(object sender, EventArgs<ReplayRecord> e)
        {
            InteractableJumpRecord record = e.value as InteractableJumpRecord;
            GameObject go = GameObject.Find(record.instanceID.ToString());
            if (go == null)
            {
                this.LogWarning("Couldn't find interactable to activate jump power");
                return;
            }

            Interactable interactable = go.GetComponent<Interactable>();
            InteractableJumpLane(interactable, record.positionX);
        }

        private bool IsColorTypeFriendly(ObjectColorType colorType)
        {
            // World Color: Blue / Orange
            return colorType.ToString().ContainsInvariant(worldColor.ToString());
        }

        public override GameType GetGameType()
        {
            return GameType.Runner_2D_Blue;
        }

        #endregion

        #region public Logic

        private Vector3 GetRandomAdjacentLane(Vector3 position)
        {
            int index = 0;
            float minDistance = float.MaxValue;
            for (int i = 0; i < lanes.Count; i++)
            {
                float distance = Mathf.Abs(position.x - lanes[i].position.x);
                if (distance < minDistance)
                {
                    index = i;
                    minDistance = distance;
                }
            }

            int adjacentIndex = -1;
            if (index == 0 || index == 2)
                adjacentIndex = 1;
            else
                adjacentIndex = UnityEngine.Random.Range(0, 1) * 2;
            return lanes[adjacentIndex].transform.position;
        }

        private Transform GetValidRandomLane()
        {
            // If it's already locked, then skip it
            List<Transform> potentiallyValidLanes = new List<Transform>(lanes);

            foreach (Transform lane in lanes)
                if (!IsLaneValid(lane))
                    potentiallyValidLanes.Remove(lane);

            // if (debug) this.LogWarning(potentiallyValidLanes.Count);
            return potentiallyValidLanes.GetRandom();
        }

        private bool IsLaneValid(Transform lane)
        {
            // If it's already blocked, then no
            if (laneBlockages.Contains(lane))
            {
                if (debug) this.LogWarning("Not considering {0} to avoid DEADLOCK with self!"._Format(lane.name));
                return false;
            }

            // If the other two are, then no
            if (laneBlockages.Count >= lanes.Count - 1)
            {
                if (debug) this.LogWarning("Not considering {0} to avoid DEADLOCK with others!"._Format(lane.name));
                return false;
            }

            return true;
        }

        #endregion

        public new class RuntimeConfig : GameManager.RuntimeConfig
        {
            public float scaleMultiplierLanes; //  ApplicationLibrary.Config.Experiment.stimulus.background.GetScaleMultiplierLanes(Camera.main);

            public RuntimeConfig(LevelConfig level, Func<LevelConfig, bool> checkLevelSuccess, 
                ReplaySystem.RuntimeConfig replaySystem, DifficultyManager.RuntimeConfig difficulty,
                ScoreSystem.RuntimeConfig score, PlayerManager.RuntimeConfig player, bool powerUp_hasAbsorptionPowerUp,
                bool powerUp_hasThunderPowerUp, bool enableHighEnergyEssences, Interactable.RuntimeConfig interactable, 
                SoundSystem.RuntimeAudioSourceConfig audioSFX, SoundSystem.RuntimeAudioSourceConfig audioMusic, KoreographyWrapper.RuntimeConfig koreo, float scaleMultiplierLanes) : 
                base(level, checkLevelSuccess, replaySystem, difficulty, score, player, powerUp_hasAbsorptionPowerUp, powerUp_hasThunderPowerUp, enableHighEnergyEssences, interactable, audioSFX, audioMusic, koreo)
            {
                this.scaleMultiplierLanes = scaleMultiplierLanes;
            }
        }

        [Serializable]
        public new class Config : GameManager.Config
        {
            public float laneSwapDuration_CODE { get { return laneSwapDuration * baseLaneScale; } }
            public float blockageTimeToAvoidDeadlocks = 0.5f;
            public float laneSwapDuration = 0.15f;
            public float baseLaneScale = 0.85f;
            // public string resetMovementCooldownOnRelease_Comment = "Allows a double lane swap if players release";
            // public bool resetMovementCooldownOnRelease = false;
        }
    }
}