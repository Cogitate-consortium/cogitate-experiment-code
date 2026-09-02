// NS_REMOVE
using ExperimentLibrary;

/// NS_DEBATABLE | Shouldnt <see cref="Game.Managers.GameplayManager.GameManager"/> handle this?
using Game.Entities.Interactables;
/// NS_DEBATABLE | Shouldnt <see cref="Game.Managers.GameplayManager.GameManager"/> handle this?
using Game.UI;

using Game.Core;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;
using Game.Entities.Core;

namespace Game.Managers.GameplayManager
{
    /// <summary>
    /// [DEPRECATE?]
    /// </summary>
    public class GameManager_Duet : GameManager
    {
#if false
        private Dictionary<LeftRight, TimeQueue<ObjectColorType>> blockagesInLane = new Dictionary<LeftRight, TimeQueue<ObjectColorType>>();
        private new GameManagerDuetConfig config { get { return base.config as GameManagerDuetConfig; } }
        private Coroutine spawnObstaclesCoroutine = null;

        public float livesOrange = 0;

        public override void Initialize(LevelConfig level)
        {
            base.Initialize(level);

            blockagesInLane.Clear();
            foreach (LeftRight lR in Utility_Helper.EnumGetValues<LeftRight>())
                blockagesInLane.Add(lR, new TimeQueue<ObjectColorType>(null));
        }

#region Override Logic

        public override void Restart()
        {
            base.Restart();

            SetLives(currentLevel.lives, currentLevel.lives);
        }

        public override void BeginReplay(bool lookForFaces)
        {
            base.BeginReplay(lookForFaces);
            StopSpawningObstacles();
        }

        public override GameType GetGameType()
        {
            return GameType.Duet;
        }

        public override void SetLives(float lives)
        {
            //base.SetLives(lives);
            // Do nothing - override below
        }

        public void SetLives(float livesBlue, float livesOrange)
        {
            this.lives = Mathf.Min(livesBlue, currentLevel.maxLives);
            this.livesOrange = Mathf.Min(livesOrange, currentLevel.maxLives);

            (gameUI as InGameUI_Duet).SetLives(this.lives, this.livesOrange);
        }

        protected override bool IsAlive()
        {
            return lives > 0 && livesOrange > 0;
        }

        protected override void HandleDamage(GameObject objectDamaged, float damage)
        {
            ObjectColorType playerType = objectDamaged.GetComponent<InteractableObjectColor>().objectColor;

            if (playerType == ObjectColorType.Blue)
            {
                lives -= damage;
            }
            else if (playerType == ObjectColorType.Orange)
            {
                livesOrange -= damage;
            }

            SetLives(lives, livesOrange);
        }

        // Rewrite live logic
        protected override void Player_OnAbsorb(object sender, EventArgs<Interactable> e)
        {
            base.Player_OnAbsorb(sender, e);

            if (isReplay) return;

            float healAmount = GetHealAmount(e.value.colorType);

            if (e.value.colorType.ToString().ContainsInvariant("blue"))
                SetLives(lives + healAmount, livesOrange);
            else if (e.value.colorType.ToString().ContainsInvariant("orange"))
                SetLives(lives, livesOrange + healAmount);
        }

#endregion

#region public Logic

        public override Interactable SpawnInteractable(Vector3 spawnPosition, ObjectColorType colorType, bool forceSpawn = false)
        {
            if (isReplay || forceSpawn)
            {
                base.SpawnInteractable(spawnPosition, colorType);
                // Shouldn't we return here?
                return null;
            }

            LeftRight leftRight = default(LeftRight);

            if (!GetValidRandomLeftRight(out leftRight, colorType))
            {
                //this.LogWarning("There was no valid position, aborted to avoid DEADLOCK!");
                // interactable.Toggle(false);
                return null;
            }

            // Mark this as a potential block to other lanes (for a brief period of time
            blockagesInLane[leftRight].AddOrUpdate(colorType, config.blockageTimeToAvoidDeadlocks);

            // Update interactable position or switch it off
            Vector3 spawnPositionNormalized = GetValidNormalizedInteractablePosition(leftRight);
            spawnPosition = Interactable.NormalizedToAbsolutePosition(spawnPositionNormalized);

            return base.SpawnInteractable(spawnPosition, colorType);
        }

        private void StopSpawningObstacles()
        {
            // Stop Spawning Coroutine
            if (spawnObstaclesCoroutine != null)
                StopCoroutine(spawnObstaclesCoroutine);
        }

        private bool GetValidRandomLeftRight(out LeftRight lR, ObjectColorType type)
        {
            lR = Utility_Helper.EnumGetRandom<LeftRight>();

            LeftRightColor choice = new LeftRightColor(lR, type);

            return IsChoiceValid(choice);
        }

        private bool IsChoiceValid(LeftRightColor choice)
        {
            TimeQueue<ObjectColorType> blockagesOwn = blockagesInLane[choice.lR];

            // What do we have in the lane we are about to spawn?

            // We have both Orange and Blue - abort
            if (blockagesOwn.Count >= 2)
                return false;

            // We have one, but we want to spawn the other - this is too close for comfort
            else if (blockagesOwn.Count >= 1 && !blockagesOwn.Contains(choice.type))
                return false;

            // So we're good from our own lane, but what about compared to the other?
            foreach (LeftRight lR in Utility_Helper.EnumGetValues<LeftRight>())
            {
                TimeQueue<ObjectColorType> blockagesOther = blockagesInLane[lR];

                // If we're about to spawn a same-color block abort
                if (blockagesOther.Contains(choice.type))
                    return false;
            }

            return true;
        }

        private Vector3 GetValidNormalizedInteractablePosition(LeftRight leftRight)
        {
            float sign = leftRight == LeftRight.Left ? -1 : 1;
            float leftRightOffset = 0.5f + sign * config.leftRightSpawningPoints;
            float upDownOffset = config.upDownSpawnPoints;
            Vector3 position = new Vector2(leftRightOffset, upDownOffset);

            return position;
        }

#endregion

        private struct LeftRightColor
        {
            public LeftRight lR;
            public ObjectColorType type;

            public LeftRightColor(LeftRight lR, ObjectColorType type)
            {
                this.lR = lR;
                this.type = type;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is LeftRightColor))
                    return false;

                LeftRightColor other = (LeftRightColor)obj;

                return lR == other.lR && type == other.type;
            }

            public override int GetHashCode()
            {
                int typeValues = Utility_Helper.EnumCount<ObjectColorType>();
                return lR.GetHashCode() * typeValues + type.GetHashCode();
            }

            public override string ToString()
            {
                return "({0}, {1})"._Format(lR, type);
            }
        }
#endif
    }
}