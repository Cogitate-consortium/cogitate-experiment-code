using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;
using Game.Core;
using System;
using System.Collections;

namespace Game.Entities.Player.Core
{
    public class PlayerView_Runner : PlayerView
    {
        [SerializeField] private ParticleSystem deathParticles = null;
        [SerializeField] private GameObject sprite = null;

        private readonly List<Transform> lanes = new List<Transform>();
        private readonly List<Vector3> lanes_Positions_TS = new List<Vector3>();

        private int currentLaneIdx = -1;
        private int wantedLaneIdx = -1;

        private bool doFlash;
        private Color startColor;
        private Color flashColor = new Color(0.7f, 0.7f, 0.7f, 0.0f);

        #region Override Logic

        public override void Initialize(PlayerView.Config baseConfig, PlayerView.RuntimeConfig baseRuntimeConfig)
        {
            base.Initialize(baseConfig, baseRuntimeConfig);

            config = baseConfig as Config;
            runtimeConfig = baseRuntimeConfig as RuntimeConfig;

            // Calculate moving speed between two lanes
            startColor = sprite.GetComponent<SpriteRenderer>().color;

            StartCoroutine(HandleDamage());
        }

        public void SetupLanes(List<Transform> lanes)
        {
            this.lanes.Clear();
            this.lanes.AddRange(lanes);

            lock (lanes_Positions_TS)
            {
                lanes_Positions_TS.Clear();
                foreach (Transform lane in lanes)
                    lanes_Positions_TS.Add(lane.position);
            }

            ForceMoveToLane(1);
        }

        public override void HandleLeftRight_TS(float normalizedLeftRight)
        {
            int direction = Mathf.RoundToInt(Mathf.Sign(normalizedLeftRight));

            RequestMoveToLane_TS(currentLaneIdx + direction);
        }

        private Config config;
        private RuntimeConfig runtimeConfig;

        [Serializable]
        public new class Config : PlayerView.Config { }

        public new class RuntimeConfig : PlayerView.RuntimeConfig
        {
            public RuntimeConfig(float scaleMultiplier, float invulnerableDurationSeconds) : base(scaleMultiplier, invulnerableDurationSeconds)
            { }
        }

        public override void Damage(GameObject collidedObject, ObjectColorType defeatedByType)
        {
            base.Damage(collidedObject, defeatedByType);

            doFlash = true;
        }

        private IEnumerator HandleDamage()
        {
            SpriteRenderer sR = sprite.GetComponent<SpriteRenderer>();
            sR.color = startColor;
            int numFlashes = 2;
            float flashDuration = runtimeConfig.invulnerableDurationSeconds / numFlashes;

            while (true)
            {
                while (!doFlash)
                    yield return null;

                doFlash = false;

                for (int i = 0; i < numFlashes; i++)
                    for (float t = 0; t < flashDuration; t += Time.deltaTime)
                    {
                        // Within the flash it goes and comes back
                        float lerp = ((t / flashDuration).Fold(1.0f) * 2).Clamped01();
                        // Debug.Log(t + " :: " + lerp);
                        sR.color = Color.Lerp(startColor, flashColor, lerp);
                        yield return null;
                    }

                sR.color = startColor;
            }
        }

        public override void Reset()
        {
            base.Reset();
            sprite.SetActive(true);
            deathParticles.Stop();
            deathParticles.Clear();
        }

        public static EventHandler<StatusArgs> onStatusUpdate;

        public class StatusArgs : EventArgs
        {
            public Vector2 relativeScreenPosition;

            public StatusArgs(Vector2 relativeScreenPosition)
            {
                this.relativeScreenPosition = relativeScreenPosition;
            }

            public override string ToString()
            {
                return Vector2PreciseString.ToString(relativeScreenPosition);
            }
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (wantedLaneIdx < 0 || currentLaneIdx < 0) return;
            if (currentLaneIdx == wantedLaneIdx) return;

            Transform wantedLane = lanes[wantedLaneIdx];
            Transform currentLane = lanes[currentLaneIdx];

            // Where do we want to end up?
            float wantedX = wantedLane.position.x;
            float currentLaneX = currentLane.position.x;
            float expectedDirection = Mathf.Sign(wantedX - currentLaneX);

            float currentX = GetCurrentPos().x;
            float actualDirection = Mathf.Sign(wantedX - currentX);
            float step = actualDirection * movementSpeed * PlayerManager.globalSpeedMulti * deltaTime;

            bool overshoot = false;
            // Have we already overshot?
            if (expectedDirection * actualDirection <= 0)
                overshoot = true;
            // Are we about to overshoot?
            else if (Mathf.Abs(currentX - wantedX) <= Mathf.Abs(step))
                overshoot = true;

            if (overshoot)
            {
                ForceMoveToLane(wantedLaneIdx);
                return;
            }

            // Just move a bit
            MoveX_By(step);

        }

        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();

            onStatusUpdate?.Invoke(this, new StatusArgs(Utility_Helper.GetRelativeScreenPosition(pivot.position)));
        }


        // [TODO] Incorporate into the model
        /*
        private void HandleInputReset()
        {
            if (Input.GetKeyUp(ApplicationLibrary.Config.InputKeyCode.Action_Left) || Input.GetKeyUp(ApplicationLibrary.Config.InputKeyCode.Action_Right))
            {
                if (config.resetMovementCooldownOnRelease)
                {
                    this.LogWarning("Resetting movement cooldown");
                    lastRequest = -1;
                }
            }
        }
        */

        #endregion

        #region Logic

        private void ForceMoveToLane(int laneIdx)
        {
            if (!laneIdx.IsBetween(0, lanes.Count - 1))
            {
                // this.Log("Invalid force lane!");
                return;
            }

            this.Log("Made it from Lane {0} to Lane {1}"._Format(currentLaneIdx, laneIdx));

            currentLaneIdx = laneIdx;
            wantedLaneIdx = -1;

            MoveX_To(lanes[laneIdx].position.x);
        }

        private void RequestMoveToLane_TS(int wantedLaneIdx)
        {
            // Are we already moving?
            if (currentPosition.x != lanes_Positions_TS[currentLaneIdx].x)
            {
                wantedLaneIdx += (int)Mathf.Sign(wantedLaneIdx - currentLaneIdx);
                this.Log("Resetting request lock");
            }

            if (!wantedLaneIdx.IsBetween(0, lanes_Positions_TS.Count - 1))
            {
                RaiseRequestPowerUp_TS(wantedLaneIdx < 0 ? PowerUp.Absorption : PowerUp.Thunder);
                return;
            }

            this.wantedLaneIdx = wantedLaneIdx;
        }

        #endregion
    }
}