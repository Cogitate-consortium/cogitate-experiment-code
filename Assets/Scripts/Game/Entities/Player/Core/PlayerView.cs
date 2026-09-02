using System;
using TGP.Helpers;
using UnityEngine;
using Game.Core;
using Helpers.Engine;

namespace Game.Entities.Player.Core
{
    /// <summary>
    /// If DUET gets deprecated, simplify
    /// </summary>
    public class PlayerView : MonoBehaviour
    {
        public EventHandler<CollisionEventArgs> onCollision;
        public event EventHandler<EventArgs<PowerUp>> onRequestPowerUp_Thread;
        public Transform pivot;
        protected bool isInvulnerable = false;
        protected Vector3 currentPosition;

        protected bool isActive = true;

        private Config config;
        private RuntimeConfig runtimeConfig;

        [Serializable]
        public class Config { }

        public class RuntimeConfig
        {
            public float scaleMultiplier;
            public float invulnerableDurationSeconds;

            public RuntimeConfig(float scaleMultiplier, float invulnerableDurationSeconds)
            {
                this.scaleMultiplier = scaleMultiplier;
                this.invulnerableDurationSeconds = invulnerableDurationSeconds;
            }
        }
        
        #region Public & Inherit Methods

        public virtual void Initialize(Config config, RuntimeConfig runtimeConfig)
        {
            this.config = config;
            this.runtimeConfig = runtimeConfig;

            Reset();

            foreach (Collider c in transform.GetComponentsInChildren<Collider>())
            {
                TriggerCheck tC = c.gameObject.AddComponentIfNotExists<TriggerCheck>();
                tC.TC_OnTriggerEnter += TC_OnTriggerEnter;
            }

            foreach (Collider2D c2D in transform.GetComponentsInChildren<Collider2D>())
            {
                TriggerCheck2D tC2D = c2D.gameObject.AddComponentIfNotExists<TriggerCheck2D>();
                tC2D.TC_OnTriggerEnter += TC_OnTriggerEnter2D;
            }
        }

        protected float movementSpeed { get; private set; }

        public void SetMovementSpeed(float movementSpeed)
        {
            this.movementSpeed = movementSpeed;
        }

        public virtual void Pause(bool doFreeze)
        {
            isActive = !doFreeze;
        }

        public virtual void Restart()
        {
            this.Log("Restart!");

            pivot.localScale = Vector3.one * runtimeConfig.scaleMultiplier;
            RotateTo(0);
            MoveX_To(0);

            Reset();
        }

        public virtual void HandleLeftRight_TS(float normalizedLeftRight) { }

        public virtual void HandleUpDown(float normalizedUpDown) { }

        public virtual void Damage(GameObject collidedObject, ObjectColorType defeatedByType)
        {
            isInvulnerable = true;
            Utility_Helper.StartTimer(runtimeConfig.invulnerableDurationSeconds, a =>
            {
                isInvulnerable = false;
                // Pause(false);
            });
        }

        public virtual void Reset()
        {
            isInvulnerable = false;
        }

        public virtual void Toggle(bool show)
        {
            pivot.gameObject.SetActive(show);
        }

        protected virtual void OnUpdate(float deltaTime) { }

        protected virtual void OnLateUpdate() { }

        protected virtual void HandleCollision(GameObject colliderSelf, GameObject colliderOther)
        {
            if (!isActive || isInvulnerable) return;
            FireCollisionEvent(colliderSelf, colliderOther);
        }

        #endregion

        #region Protected Methods

        protected void RaiseRequestPowerUp_TS(PowerUp powerUp)
        {
            onRequestPowerUp_Thread?.Invoke(this, powerUp);
        }

        protected void FireCollisionEvent(GameObject colliderSelfGO, GameObject colliderOtherGO)
        {
            onCollision?.Invoke(this, new CollisionEventArgs(colliderSelfGO, colliderOtherGO));
        }

        protected void RotateBy(float degreesZ)
        {
            pivot.Rotate(Vector3.forward * degreesZ);
        }

        protected void RotateTo(float degreeZ)
        {
            pivot.rotation = Quaternion.Euler(Vector3.forward * degreeZ);
        }

        protected void MoveX_By(float worldUnits)
        {
            MoveX_To(pivot.position.x + worldUnits);
        }

        protected void MoveX_To(float absWorld_X)
        {
            // this.LogError(TimeWrapper.GetCurrentTimestamp_TS() + " : " + absWorld_X);
            currentPosition = pivot.position;
            currentPosition.x = absWorld_X;
            pivot.position = currentPosition;
        }

        protected void MoveBy(Vector2 worldUnits)
        {
            pivot.transform.Translate(worldUnits);
        }

        protected Vector3 GetCurrentPos()
        {
            return pivot.position;
        }

        #endregion

        #region Private Logic

        private void Update()
        {
            if (!isActive) return;
            OnUpdate(TimeWrapper.deltaTime_SinceLastUpdate_NotTS);
        }

        private void LateUpdate()
        {
            if (!isActive) return;
            OnLateUpdate();
        }

        private void TC_OnTriggerEnter(TriggerCheck self, Collider other)
        {
            HandleCollision(self.gameObject, other.gameObject);
        }

        private void TC_OnTriggerEnter2D(TriggerCheck2D self, Collider2D other)
        {
            HandleCollision(self.gameObject, other.gameObject);
        }

        #endregion

        public class CollisionEventArgs : EventArgs
        {
            public GameObject colliderSelfGO;
            public GameObject colliderOtherGO;

            public CollisionEventArgs(GameObject colliderSelfGO, GameObject colliderOtherGO)
            {
                this.colliderSelfGO = colliderSelfGO;
                this.colliderOtherGO = colliderOtherGO;
            }
        }
    }
}