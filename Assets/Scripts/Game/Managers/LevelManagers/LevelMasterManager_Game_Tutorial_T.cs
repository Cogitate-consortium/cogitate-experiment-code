using System.Collections;

namespace Game.Managers.LevelManagers
{
    /// <summary>
    /// </summary>
    public class LevelMasterManager_Game_Tutorial_T : LevelMasterManager_Game
    {
        protected override void DoStart()
        {
            base.DoStart();

            StartCoroutine(CheckForEndOfGame());
        }
        
        private IEnumerator CheckForEndOfGame()
        {
            while (true)
            {
                if (gameManager.timeRemaining < 0) // ApplicationLibrary.Config.Probes.probeShieldBeforeEndOfLevelMS / 1000f)
                {
                    CompleteLevel();
                    yield break;
                }

                yield return null;
            }
        }

    }
}