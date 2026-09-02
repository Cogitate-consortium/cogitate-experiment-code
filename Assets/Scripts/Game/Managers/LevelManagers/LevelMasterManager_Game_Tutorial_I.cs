using System;
using System.Collections.Generic;
using Game.Core;
using Game.Managers.GameplayManager;
using Game.Systems.Bridges;
using Game.Systems.Misc;
using UnityEngine;

namespace Game.Managers.LevelManagers
{
    /// <summary>
    /// </summary>
    public class LevelMasterManager_Game_Tutorial_I : LevelMasterManager
    {
        public new class RuntimeConfig : LevelMasterManager.RuntimeConfig
        {
            public LevelMasterManager_Game_Tutorial_I_UI.RuntimeConfig ui;

            public RuntimeConfig(GameManager.RuntimeConfig gameManager, float orthoSize, LevelConfig levelConfig,
                FadeSceneController.RuntimeConfig fadeScene, bool hasIntroDim, IList<ITrigger> triggerManagers,
                bool fakeStartTrigger, bool skipEndOfGameMessage, bool useCameraBlock,
                float viewportSizePercentile, Color viewportBlockingColor, LevelMasterManager_Game_Tutorial_I_UI.RuntimeConfig ui) :
                base(gameManager, orthoSize, levelConfig, fadeScene, hasIntroDim, triggerManagers, fakeStartTrigger,
                    skipEndOfGameMessage, useCameraBlock, viewportSizePercentile, viewportBlockingColor)
            {
                this.ui = ui;
            }
        }

        public override void Initialize(Config config, LevelMasterManager.RuntimeConfig baseRuntimeConfig)
        {
            base.Initialize(config, baseRuntimeConfig);

            RuntimeConfig runtimeConfig = baseRuntimeConfig as RuntimeConfig;
            
            LevelMasterManager_Game_Tutorial_I_UI.Initialize(runtimeConfig.ui);
            LevelMasterManager_Game_Tutorial_I_UI.instance.onEndOfGame += Instance_onEndOfGame;
            gameManager.gameUI.Toggle(false);
        }

        private void Instance_onEndOfGame(object sender, EventArgs e)
        {
            CompleteLevel();
        }
    }
}