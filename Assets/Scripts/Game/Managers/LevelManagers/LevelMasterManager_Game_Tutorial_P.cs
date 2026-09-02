namespace Game.Managers.LevelManagers
{
    public class LevelMasterManager_Game_Tutorial_P : LevelMasterManager_Game
    {
        protected override void DoStart()
        {
            base.DoStart();
            gameManager.gameUI.ToggleTimer(false);
        }
    }
}