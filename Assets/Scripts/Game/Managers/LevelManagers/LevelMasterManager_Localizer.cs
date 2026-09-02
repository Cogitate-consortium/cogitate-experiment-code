namespace Game.Managers.LevelManagers
{
    /// <summary>
    /// </summary>
    public class LevelMasterManager_Localizer : LevelMasterManager
    {
        protected override void DoStart()
        {
            base.DoStart();

            gameManager.BeginReplay();
        }
    }
}