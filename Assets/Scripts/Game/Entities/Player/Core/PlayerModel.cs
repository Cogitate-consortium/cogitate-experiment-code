// NS_DEBATABLE | Someone needs to do that though
using Game.Entities.Interactables;

using System;
using TGP.Helpers;
using UnityEngine;
using Game.Core;
using Helpers.Engine;
using Game.Entities.Core;

namespace Game.Entities.Player
{
    /// <summary>
    /// [CHECK]
    /// </summary>
    public class PlayerModel
    {
        public EventHandler<DamageEventArgs> onDamaged;
        public EventHandler<EventArgs<ObjectColorType>> onAbsorb;
        public EventHandler<EventArgs<float>> onLeftRightHold_TS;
        public EventHandler<EventArgs<float>> onUpDownHold_TS;

        private bool isActive = true;

        private Direction_2D lastDir = Direction_2D.Down;

        private double timeSwitchHorizontal = 0;
        private double timeSwitchVertical = 0;

        float playerModelConfig_timeToFull;
        float playerModelConfig_valueAt0;
        bool playerModelConfig_smoothHorizontal;
        bool playerModelConfig_smoothVertical;

        private double lastRequestMS = -1;
        private double timeSinceLastRequestMS { get { return TimeWrapper.currentTimestampMS - lastRequestMS; } }
        private double requestThreshold { get { return config.movementCooldown; } }
        private double timeLeftForRequest { get { return requestThreshold - timeSinceLastRequestMS / 1000; } }
        private bool canTakeRequests { get { return timeLeftForRequest <= 0; } }
        private float invulnerableDuration { get { return config.invulnerableDuration / 1000f; } }

        #region Public Methods

        private Config config;

        public virtual void Initialize(Config config)
        {
            this.Log("Init!");
            this.config = config;

            playerModelConfig_timeToFull = config.timeToFull;
            playerModelConfig_valueAt0 = config.valueAt0;
            playerModelConfig_smoothHorizontal = config.smoothHorizontal;
            playerModelConfig_smoothVertical = config.smoothVertical;
        }

        [Serializable]
        public class Config
        {
            public float movementCooldown = 0.15f;

            public float timeToFull = 0.05f;
            public float valueAt0 = 0.2f;

            public bool smoothHorizontal = true;
            public bool smoothVertical = false;

            public float invulnerableDuration = 0.5f;
        }

        public void Pause(bool doFreeze)
        {
            isActive = !doFreeze;
        }

        public void Restart()
        {
            timeSwitchHorizontal = 0;

            this.Log("Restart!");
        }

        public virtual void HandleCommand_TS(PlayerController.Command command)
        {
            // [SOS] Careful with Debugs, as this needs to be TS
            if (!canTakeRequests)
            {
#if UNITY_EDITOR
                // Debug.LogWarning("CANT TAKE REQS FOR ANOTHER: " + timeLeftForRequest);
#endif
                return;
            }

            if (!isActive)
            {
#if UNITY_EDITOR
                // Debug.Log("Inactive, command not processed!");
#endif
                return;
            }

            double timestampMS = TimeWrapper.currentFrameCycleBeginMS;

            float magnitude = command.magnitude;

            if (magnitude < 0)
            {
                timeSwitchHorizontal = timestampMS;
                timeSwitchVertical = timestampMS;
                return;
            }

            if ((command.direction == Direction_2D.Left && lastDir == Direction_2D.Right) ||
                (command.direction == Direction_2D.Right && lastDir == Direction_2D.Left))
                timeSwitchHorizontal = timestampMS;
            if ((command.direction == Direction_2D.Up && lastDir == Direction_2D.Down) ||
                (command.direction == Direction_2D.Down && lastDir == Direction_2D.Up))
                timeSwitchHorizontal = timestampMS;

            float lerpHorizontal = ((float)timestampMS - ((float)timeSwitchHorizontal + playerModelConfig_timeToFull)).Retargeted(0, playerModelConfig_timeToFull, playerModelConfig_valueAt0, 1f).Smooth01(SmoothType.Sqrt);
            float lerpVertical = ((float)timestampMS - ((float)timeSwitchVertical + playerModelConfig_timeToFull)).Retargeted(0, playerModelConfig_timeToFull, playerModelConfig_valueAt0, 1f).Smooth01(SmoothType.Sqrt);

            if (!playerModelConfig_smoothHorizontal)
                lerpHorizontal = 1;
            if (!playerModelConfig_smoothVertical)
                lerpVertical = 1;

            switch (command.direction)
            {
                case Direction_2D.Left:
                    RaiseLeftRightEvent_TS(-lerpHorizontal * magnitude);
                    break;
                case Direction_2D.Right:
                    RaiseLeftRightEvent_TS(lerpHorizontal * magnitude);
                    break;
                case Direction_2D.Up:
                    RaiseUpDownEvent_TS(lerpVertical * magnitude);
                    break;
                case Direction_2D.Down:
                    RaiseUpDownEvent_TS(-lerpVertical * magnitude);
                    break;
            }

            lastDir = command.direction;
            lastRequestMS = TimeWrapper.currentTimestampMS;
        }

        public static EventHandler<StatusArgs> onStatusUpdate;

        public class StatusArgs
        {
            public ObjectColorType colorType;

            public StatusArgs(ObjectColorType colorType)
            {
                this.colorType = colorType;
            }
        }

        public virtual void HandleCollision(GameObject colliderSelf, GameObject colliderOther)
        {
            // Grab the interactable
            Interactable interactable = Interactable.TryGetInteractableFromColliderObject(colliderOther);
            if (interactable == null)
            {
                this.Log("Collided with unknown object");
                return;
            }

            onStatusUpdate?.Invoke(this, new StatusArgs(interactable.colorType));

            this.Log("{0} collided with {1}"._Format(colliderSelf.name, interactable.name));

            // Are they of the same type?
            ObjectColorType typeSelf = colliderSelf.GetComponent<InteractableObjectColor>().objectColor;
            ObjectColorType typeOther = interactable.colorType;

            //Debug.Log(string.Format("Player collided with interactable:{0}", interactable.colorType.ToString()), interactable.transform);

            // We are.. Colliding with..
            switch (typeSelf)
            {
                case ObjectColorType.Gray:
                    break;
                case ObjectColorType.Orange:
                    switch (typeOther)
                    {
                        case ObjectColorType.GrayOrange:
                        case ObjectColorType.Orange:
                            interactable.Absorb();
                            RaiseAbsorbEvent(interactable);
                            break;
                        case ObjectColorType.OrangeHigh:
                            interactable.Absorb();
                            RaiseAbsorbEvent(interactable);
                            break;
                        case ObjectColorType.Gray:
                        case ObjectColorType.GrayBlue:
                        case ObjectColorType.Blue:
                        case ObjectColorType.BlueHigh:
                            HandleDamage(colliderSelf, interactable);
                            break;
                    }
                    break;

                case ObjectColorType.Blue:
                    switch (typeOther)
                    {
                        case ObjectColorType.GrayBlue:
                        case ObjectColorType.Blue:
                            interactable.Absorb();
                            RaiseAbsorbEvent(interactable);
                            break;
                        case ObjectColorType.BlueHigh:
                            interactable.Absorb();
                            RaiseAbsorbEvent(interactable);
                            break;
                        case ObjectColorType.Gray:
                        case ObjectColorType.GrayOrange:
                        case ObjectColorType.Orange:
                        case ObjectColorType.OrangeHigh:
                            HandleDamage(colliderSelf, interactable);
                            break;
                    }
                    break;
                case ObjectColorType.OrangeHigh:
                    break;
                case ObjectColorType.BlueHigh:
                    break;
            }
        }

        private void HandleDamage(GameObject colliderSelf, Interactable interactable)
        {
            interactable.Explode();
            interactable.Pause(true);
            interactable.Toggle(false);

            // Substract lives and what not
            RaiseDamageEvent(colliderSelf, interactable.colorType);
        }

        #endregion

        private void RaiseLeftRightEvent_TS(float normalizedLeftRight)
        {
            onLeftRightHold_TS?.Invoke(this, normalizedLeftRight);
        }

        private void RaiseUpDownEvent_TS(float normalizedLeftRight)
        {
            onUpDownHold_TS?.Invoke(this, normalizedLeftRight);
        }

        private void RaiseDamageEvent(GameObject colliderSelf, ObjectColorType colorType)
        {
            onDamaged?.Invoke(this, new DamageEventArgs(colliderSelf, colorType));
        }

        private void RaiseAbsorbEvent(Interactable collidedObject)
        {
            onAbsorb?.Invoke(this, new EventArgs<ObjectColorType>(collidedObject.colorType));
        }

        public class DamageEventArgs : EventArgs
        {
            public GameObject objectDamaged;
            public ObjectColorType defeatedByType;

            public DamageEventArgs(GameObject objectDefeated, ObjectColorType defeatedByType)
            {
                this.objectDamaged = objectDefeated;
                this.defeatedByType = defeatedByType;
            }
        }
    }
}