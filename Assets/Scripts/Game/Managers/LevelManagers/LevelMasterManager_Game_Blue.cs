namespace Game.Managers.LevelManagers
{
    /// <summary>
    /// NS_MERGE with <see cref="LevelMasterManager_Game"/> ??
    /// </summary>
    public class LevelMasterManager_Game_Blue : LevelMasterManager_Game
    {
        /*
        protected override bool isBlueWorld { get { return true; } }

        /// Deprecated related to <see cref="Narrative"/>
        public override void Initialize(LevelConfig levelConfig)
        {
            base.Initialize(levelConfig);
            gameManager.gameUI.ShowTip(config.t_1_1_2);
        }

        protected override void StartGame()
        {
            base.StartGame();

            if (levelConfig.levelID_1Based < 3)
                StartCoroutine(CheckForLowHealthIE());
        }

        private IEnumerator CheckForLowHealthIE()
        {
            while (true)
            {
                if (gameManager.lives < 2)
                {
                    gameManager.gameUI.ShowTip(config.t_1_2_1);
                    yield return new WaitForSeconds(10);
                }
                yield return new WaitForSeconds(1);
            }
        }
        */

    }
}