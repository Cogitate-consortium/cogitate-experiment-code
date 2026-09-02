// NS_REMOVE
using ExperimentLibrary;

// NS_DEBATABLE
using Game.Entities.Interactables;

using Game.Core;
using System.Collections;
using TGP.Helpers;
using UnityEngine;
using Helpers.Engine;
using Game.Entities.Core;

namespace Game.Entities.Player.Core
{
    /// <summary>
    /// [DEPRECATED?]
    /// </summary>
    public class PlayerView_Duet : PlayerView
    {
#if false
        [SerializeField] private Transform objectA = null;
        [SerializeField] private Transform objectB = null;

        [SerializeField, Range(0.35f, 0.49f)] private float minDistance = 0.42f;
        [SerializeField, Range(1.50f, 2.00f)] private float maxDistance = 1.75f;

        private float currentDistance = 0;
        private GameManagerDuetConfig config { get { return ApplicationLibrary.Config.Duet; } }

        private Vector3 wantedPositionA;
        private Vector3 wantedPositionB;

        private bool isAnimatingSwap = false;
        private float animationSwapDuration = 0.25f;

        protected bool isInvulnerable_Orange = false;

        Color startColorA;
        Color startColorB;

        #region Override Logic

        public override void Initialize()
        {
            base.Initialize();
            startColorA = objectA.GetComponentInChildren<SpriteRenderer>().color;
            startColorB = objectB.GetComponentInChildren<SpriteRenderer>().color;
        }

        public override void Restart()
        {
            base.Restart();

            // float scaleMultiplier = ApplicationLibrary.Config.Experiment.stimulus.background.GetScaleMultiplierDefault(Camera.main);
            // SetContractExpandTo(config.duetDistance * scaleMultiplier);
            // this.LogWarning(scaleMultiplier);
        }

        public override void HandleLeftRight(float normalizedLeftRight)
        {
            if (isAnimatingSwap) return;

            // The closer we are, the faster we go (same linear velocity, lower radius -> higher angular velocity)
            float distanceMultiplier = maxDistance / currentDistance;
            float degrees = -config.rotationCyclesPerSecAtPerimeter * distanceMultiplier * normalizedLeftRight * 360;
            RotateBy(degrees);
        }

        public override void HandleUpDown(float normalizedUpDown)
        {
            //ContractExpandBy(normalizedUpDown);
            //SwapDuet();
        }

        public override void Damage(GameObject collidedObject, ObjectColorType defeatedByType)
        {
            base.Damage(collidedObject, defeatedByType);

            bool isBlue = collidedObject.GetComponent<InteractableObjectColor>().objectColor == ObjectColorType.Blue;
            StartCoroutine(FlashIE(isBlue));

            return;
        }

        public override void Reset()
        {
            base.Reset();

            objectA.gameObject.SetActive(true);
            objectB.gameObject.SetActive(true);
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (!isAnimatingSwap)
            {
                objectA.localPosition = wantedPositionA;
                objectB.localPosition = wantedPositionB;
            }


            SPR_EXPE.LogData_AtNextRenderedFrame_TS("Player_Duet_World_Rotation", base.pivot.transform.eulerAngles.z);
            SPR_EXPE.LogData_AtNextRenderedFrame_TS("Player_Duet_Blue_Screen_Position", Utility_Helper.GetRelativeScreenPosition(objectA.transform.position));
            SPR_EXPE.LogData_AtNextRenderedFrame_TS("Player_Duet_Orange_Screen_Position", Utility_Helper.GetRelativeScreenPosition(objectB.transform.position));
        }

        protected override void HandleCollision(GameObject colliderSelf, Interactable interactable)
        {
            //base.HandleCollision(colliderSelf, interactable);

            // Are they of the same type?
            ObjectColorType typeSelf = colliderSelf.GetComponent<InteractableObjectColor>().objectColor;
            switch (typeSelf)
            {
                case ObjectColorType.Orange:
                    if (isInvulnerable_Orange)
                        return;
                    break;
                case ObjectColorType.Blue:
                    if (isInvulnerable)
                        return;
                    break;
            }

            FireCollisionEvent(colliderSelf, interactable);
        }

        #endregion

        #region Logic

        private void SwapDuet()
        {
            if (isAnimatingSwap) return;

            isAnimatingSwap = true;
            Vector3 originA = objectA.transform.localPosition;
            Vector3 originB = objectB.transform.localPosition;
            LeanTween.moveLocal(objectA.gameObject, originB, animationSwapDuration).setOnComplete(() =>
            {
                wantedPositionA = originB;
                wantedPositionB = originA;
                isAnimatingSwap = false;
            });
            LeanTween.moveLocal(objectB.gameObject, originA, animationSwapDuration);
        }

        private void ContractExpandBy(float normalizedUpDown)
        {
            /*
            float dist01 = normalizedUpDown / config.fullContractDuration;
            float currentDist = Mathf.Abs(objectA.localPosition.x).RetargetedTo_01(minDistance, maxDistance);
            SetContractExpandTo(currentDist + dist01);
            */
        }

        private void SetContractExpandTo(float dist01)
        {
            float dist = dist01.RetargetedFrom_01(minDistance, maxDistance);
            wantedPositionA.x = -dist;
            wantedPositionB.x = dist;
            currentDistance = dist;
        }

        private IEnumerator FlashIE(bool isBlue = false)
        {
            float invulnerableDuration = ApplicationLibrary.Config.PlayerModel.duetInvincibleDuration_MS / 1000f;
            float timeStarted = TimeWrapper.time;

            //Debug.LogWarning("Dying Duet");
            if (isBlue)
                isInvulnerable = true;
            else
                isInvulnerable_Orange = true;

            Transform damagedObject = isBlue ? objectA : objectB;
            SpriteRenderer sr = damagedObject.GetComponentInChildren<SpriteRenderer>();

            Color targetColor = new Color(0.7f, 0.7f, 0.7f, 0.2f);
            while (TimeWrapper.time - timeStarted < invulnerableDuration)
            {
                float lerp = (TimeWrapper.time - timeStarted) / invulnerableDuration;
                float sin = Mathf.Sin(lerp * 8) * 0.5f + 0.5f;

                Color colorLerp = Color.Lerp(isBlue ? startColorA : startColorB, targetColor, sin);
                sr.color = colorLerp;
                yield return null;
            }

            sr.color = isBlue ? startColorA : startColorB;

            if (isBlue)
                isInvulnerable = false;
            else
                isInvulnerable_Orange = false;

            //Debug.LogWarning("Reset Duet");
        }

        #endregion
#endif
    }
}