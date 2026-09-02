using Game.Core;
using Game.Entities.Player.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Entities.Player
{
    public class PlayerManager_Runner : PlayerManager
    {
        private new PlayerView_Runner view;

        public void SetupLanes(List<Transform> lanes)
        {
            view.SetupLanes(lanes);
        }

        public override void Initialize(PlayerManager.Config baseConfig, PlayerManager.RuntimeConfig baseRuntimeConfig)
        {
            base.Initialize(baseConfig, baseRuntimeConfig);

            view = base.view as PlayerView_Runner;
        }

        [Serializable]
        public new class Config : PlayerManager.Config
        {
            public new PlayerView_Runner.Config view;
        }

        public new class RuntimeConfig : PlayerManager.RuntimeConfig
        {
            public new PlayerView_Runner.RuntimeConfig view;

            public RuntimeConfig(GameType gameType, float verticalPositionScaled, PlayerView_Runner.RuntimeConfig view, PlayerController.RuntimeConfig controller) : 
                base(gameType, verticalPositionScaled, view, controller)
            {
                this.view = view;
            }
        }
    }
}