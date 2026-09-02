// NS_REMOVE - Game Manager should talk to Player, and get stuff from either Input or Replay
using Game.Systems.Replay;

using Helpers.Assets;
using System;
using TGP.Helpers;
using UnityEngine;
using Game.Entities.Player.Core;
using Game.Core;
using Helpers.Async; // Needed because of VIEW (run on MAIN) - maybe segment to DE-threading and Threading?

namespace Game.Entities.Player
{
    /// <summary>
    /// 
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        public event EventHandler<PlayerModel.DamageEventArgs> onDamaged;
        public event EventHandler<EventArgs<ObjectColorType>> onAbsorb;
        public event EventHandler<EventArgs<PowerUp>> onRequestPowerUp_Thread;

        public Transform cameraAnchor { get { return view?.pivot; } }

        private PlayerModel model;
        protected PlayerView view;
        private PlayerController controller;

        public static float globalSpeedMulti;

        private const string VIEW_RESOURCE_NAME = "Player/PlayerView_{0}";

        #region Public Methods

        public static void SetGlobalSpeedMulti(float value)
        {
            globalSpeedMulti = value;
        }

        /// <summary>
        /// This ensures the correct SUBCLASS of <see cref="PlayerView"/> is returned
        /// </summary>
        /// <param name="gameType"></param>
        /// <returns></returns>
        protected PlayerView CreateView(GameType gameType)
        {
            // Setup the view
            string resourceName = VIEW_RESOURCE_NAME._Format(gameType);
            GameObject gO = ResourceHelper.InstantiateResource<GameObject>(resourceName);
            gO.transform.ReparentAndReset(transform);

            return gO.GetComponent<PlayerView>();
        }

        public virtual void Initialize(Config config, RuntimeConfig runtimeConfig)
        {
            this.Log("Init!");

            transform.localPosition = Vector2.up * runtimeConfig.verticalPositionScaled;

            model = new PlayerModel();
            view = CreateView(runtimeConfig.gameType);
            controller = new PlayerController();

            // Sub to their events
            controller.onCommand_TS += Controller_OnCommand_TS;

            model.onDamaged += Model_OnDamaged;
            model.onAbsorb += Model_OnAbsorb;
            model.onLeftRightHold_TS += Model_OnLeftRight_TS;
            model.onUpDownHold_TS += Model_OnUpDown_TS;

            view.onCollision += View_OnCollision;
            view.onRequestPowerUp_Thread += View_onRequestPowerUp_TS;

            model.Initialize(config.model);
            view.Initialize(config.view, runtimeConfig.view);
            controller.Initialize(config.controller, runtimeConfig.controller);
        }

        public void SetMovementSpeed(float playerViewMovementSpeed)
        {
            view.SetMovementSpeed(playerViewMovementSpeed);
        }

        public void SetControllable(bool canControl)
        {
            controller.Pause(!canControl);
        }

        public void Pause(bool doPause)
        {
            model.Pause(doPause);
            view.Pause(doPause);
            controller.Pause(doPause);
        }

        public void DeInitialize()
        {
            this.LogWarning("Player Deinitialized");

            if (controller != null)
            {
                controller.DeInitialize();
                controller.onCommand_TS -= Controller_OnCommand_TS;
            }

            if (model != null)
            {
                model.onDamaged -= Model_OnDamaged;
                model.onAbsorb -= Model_OnAbsorb;
                model.onLeftRightHold_TS -= Model_OnLeftRight_TS;
                model.onUpDownHold_TS -= Model_OnUpDown_TS;
            }

            if (view != null)
            {
                view.onCollision -= View_OnCollision;
                // Destroy(view.gameObject);
            }
        }

        public void Restart()
        {
            // Restart everything
            model.Restart();
            view.Restart();
            controller.Restart();
        }

        public void ListenToReplaySystem(bool listen)
        {
            controller.listenToReplay = listen;
        }

        public void Toggle(bool show)
        {
            view.Toggle(show);
        }

        public Transform GetPivot()
        {
            return view.pivot;
        }

        #endregion

        private void RaiseOnDamaged(PlayerModel.DamageEventArgs e)
        {
            onDamaged?.Invoke(this, e);
        }

        private void RaiseOnAbsorb(ObjectColorType colorType)
        {
            onAbsorb?.Invoke(this, new EventArgs<ObjectColorType>(colorType));
        }

        #region Events

        private void Controller_OnCommand_TS(object sender, EventArgs<PlayerController.Command> e)
        {
            model.HandleCommand_TS(e);
        }

        private void Model_OnDamaged(object sender, PlayerModel.DamageEventArgs e)
        {
            view.Damage(e.objectDamaged.gameObject, e.defeatedByType);
            // Handle Defeat
            RaiseOnDamaged(e);
        }

        private void Model_OnAbsorb(object sender, EventArgs<ObjectColorType> e)
        {
            RaiseOnAbsorb(e);
        }

        private void Model_OnLeftRight_TS(object sender, EventArgs<float> e)
        {
            // this.LogError(Helpers.Engine.TimeWrapper.GetCurrentTimestamp_TS() + " : Request Received");
            view.HandleLeftRight_TS(e);
        }

        private void Model_OnUpDown_TS(object sender, EventArgs<float> e)
        {
            AsyncThread.RunOnMainThread_ASAP_TS(() =>
            {
                view.HandleUpDown(e);
            });
        }

        private void View_OnCollision(object sender, PlayerView.CollisionEventArgs e)
        {
            model.HandleCollision(e.colliderSelfGO, e.colliderOtherGO);
        }

        private void View_onRequestPowerUp_TS(object sender, EventArgs<PowerUp> e)
        {
            onRequestPowerUp_Thread?.Invoke(this, e);
        }

        public void AssignReplaySystem(ReplaySystem replaySystem)
        {
            controller.replaySystem = replaySystem;
        }

        public class RuntimeConfig
        {
            public GameType gameType;
            public float verticalPositionScaled;
            public PlayerView.RuntimeConfig view;
            public PlayerController.RuntimeConfig controller;

            public RuntimeConfig(GameType gameType, float verticalPositionScaled, PlayerView.RuntimeConfig view, PlayerController.RuntimeConfig controller)
            {
                this.gameType = gameType;
                this.verticalPositionScaled = verticalPositionScaled;
                this.view = view;
                this.controller = controller;
            }
        }

        [Serializable]
        public class Config
        {
            public PlayerModel.Config model;
            public PlayerView.Config view;
            public PlayerController.Config controller;
        }

        #endregion
    }

    public enum PowerUp { Thunder = 0, Absorption = 1 }
}