using System;
using TGP.Helpers;

namespace Game.Core
{
    public class LevelCompleteArgs : EventArgs
    {
        public EndReason endReason;
        public LevelConfig levelConfig;
        public float score;
        public int stars = 0;
        public float scorePercentile;
        public float elapsedTimeSeconds_Level_NoPauses;

        public static LevelCompleteArgs GetLevelExitArgs(LevelConfig levelConfig)
        {
            return new LevelCompleteArgs(EndReason.Exit, levelConfig, 0, 0, 0, 0);
        }

        public LevelCompleteArgs(EndReason endReason, LevelConfig levelConfig, float score, int stars, float scorePercentile, float elapsedTimeSeconds_Level_NoPauses)
        {
            this.endReason = endReason;
            this.levelConfig = levelConfig;
            this.score = score;
            this.stars = stars;
            this.scorePercentile = scorePercentile;
            this.elapsedTimeSeconds_Level_NoPauses = elapsedTimeSeconds_Level_NoPauses;
        }
    }

    public enum EndReason { Success, Failure, Exit }

    /// <summary>
    /// NS_SPAGHETTI | Config vs Runtime Config
    /// </summary>
    [System.Serializable]
    public class LevelConfig
    {
        /// <summary>
        /// 1-based
        /// </summary>
        public int levelID_1Based = 0;
        public string levelName = "";
        public float skipAverageSpawnBeats_Base = 0;

        /// <summary>
        /// [SOS] Use for READING <see cref="LevelConfig.GetGameType()"/> instead
        /// </summary>
        public GameType gameType = GameType.Duet;

        public int koregraphyIndex = 0;
        public int lives = 3;
        public int maxLives = 5;
        public float musicVolume = 1f;

        [System.NonSerialized]
        public bool isInverted = false;

        /// <summary>
        /// [SOS] Use <see cref="LevelConfig.GetLevelDuration(LevelConfig)"/> instead
        /// </summary>
        public int wantedDuration = 60;

        public bool isBlueWorld { get { return gameType == GameType.Runner_2D_Blue; } }
        public bool isOrangeWorld { get { return gameType == GameType.Runner_2D_Orange; } }

        /// <summary>
        /// 0-based
        /// </summary>
        public int worldID { get { return levelName.Split('_')[0].ToInt() - 1; } }
        /// <summary>
        /// 0-based
        /// </summary>
        public int levelID_WithinWorld { get { return levelName.Split('_')[1].ToInt() - 1; } }

        public bool isFirstOfWorld { get { return levelName.Contains("_1"); } }

        public bool isLocalizer { get { return localizerID_0Based >= 0; } }
        public bool isTutorial_T { get { return levelID_1Based == tutorial_T_ID_1Based; } }
        public bool isTutorial_I { get { return levelID_1Based == tutorial_I_ID_1Based; } }
        public bool isTutorial_P { get { return levelID_1Based == tutorial_P_ID_1Based; } }

        public static readonly int tutorial_T_ID_1Based = -2;
        public static readonly int tutorial_I_ID_1Based = -1;
        public static readonly int tutorial_P_ID_1Based = -0;
        public static readonly int localizerOffset = 100;

        /// <summary>
        /// 0-based
        /// </summary>
        public int localizerID_0Based { get { return levelID_1Based - localizerOffset < 0 ? -1 : levelID_1Based - localizerOffset; } }

        /*
        // [DUPLICATE INTERFACE]
        public string GetLocalizerReplayName(bool continuingGameColors)
        {
            if (!isLocalizer)
            {
                this.LogError("Trying to get replay name from a non-replay level:" + levelName);
                return "";
            }

            string replayLevelName = "1_2";

            if (localizerID_0Based == 0)
                replayLevelName = "1_2";
            else if (localizerID_0Based == 1)
                replayLevelName = continuingGameColors ? "1_4" : "2_2";
            else if (localizerID_0Based == 2)
                replayLevelName = continuingGameColors ? "2_2" : "1_4";
            else if (localizerID_0Based == 3)
                replayLevelName = "2_4";

            else if (localizerID_0Based == 4)
                replayLevelName = "3_2";
            else if (localizerID_0Based == 5)
                replayLevelName = continuingGameColors ? "3_4" : "4_2";
            else if (localizerID_0Based == 6)
                replayLevelName = continuingGameColors ? "4_2" : "3_4";
            else if (localizerID_0Based == 7)
                replayLevelName = "4_4";

            return replayLevelName;
        }
        */

        public bool isTutorial { get { return isTutorial_T || isTutorial_I || isTutorial_P; } }

        public GameType GetGameType()
        {
            if (isInverted)
                return (GameType)(1 - (int)gameType);
            return gameType;
        }

        public WorldType GetWorldType_TS()
        {
            WorldType world = WorldType.Blue;
            if (worldID == 1 || worldID == 3)
                world = WorldType.Orange;
            if (isInverted)
                world.Invert_TS();
            return world;
        }

        /*
        /// <summary>
        /// [SOS] Use <see cref="LevelsLibrary.GetLevelDifficulty(LevelConfig)"/> instead
        /// </summary>
        public float baseDifficulty = 0;
        */
    }
}