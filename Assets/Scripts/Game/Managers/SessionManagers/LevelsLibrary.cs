using Game.Core;
using System;
using System.Collections.Generic;
using TGP.Helpers;

namespace Game.Managers.SessionManagers
{
    public static class LevelsLibrary
    {
        public static Config config { get; private set; }

        public static void Initialize(Config config, RuntimeConfig runtimeConfig)
        {
            config.Initialize(runtimeConfig.numLevelsPerWorld);

            LevelsLibrary.config = config;

            // [DEBUG] Print all run halves
            // foreach (LevelConfig lC in LevelsLibrary.config.levels) UnityEngine.Debug.Log(lC.levelName + " + " + Experiment.Managers.ExperimentManagerSession.FMRI_GetNameOtherHalfOfRun(lC.levelName));
        }

        #region Public Accessors
        public static LevelConfig GetLevel(int levelIdx_1based)
        {
            foreach (LevelConfig l in config.levels)
                if (l.levelID_1Based == levelIdx_1based)
                    return l;
            Debug_Helper.LogWarning(typeof(LevelsLibrary), "Couldn't find level ID {0}. Returning random."._Format(levelIdx_1based));
            return null;
        }

        public static string GetLevelName(int lvlIdx_1based, bool isUI)
        {
            LevelConfig level = GetLevel(lvlIdx_1based);

            if (level == null) return "";

            if (isUI)
            {
                // In game = ID
                if (level.isLocalizer)
                    return Config.GetLocalizerPrefixUI() + (level.localizerID_0Based + 1);
                else
                    return "" + lvlIdx_1based;
            }
            else
                return level.levelName;
        }

        public static LevelConfig GetLevelByName(string levelName)
        {
            foreach (LevelConfig l in config.levels)
                if (l.levelName == levelName)
                    return l;
            Debug_Helper.LogWarning(typeof(LevelsLibrary), "Couldn't find level ID {0}. Returning random."._Format(levelName));
            return config.levels.GetRandom();
        }

        /*
        public static float GetLevelDifficulty(LevelConfig level)
        {
            if (level == null)
            {
                Debug_Helper.LogError(typeof(LevelsLibrary), "Level was null");
                return 0;
            }

            if (config.overrideDifficulty >= 0) return config.overrideDifficulty;
            return level.baseDifficulty * config.difficultyMultiplier;
        }
        */

        public static string GetLocalizerNameUI(int localizerID_0Based)
        {
            return string.Format("{0}{1}", Config.GetLocalizerPrefixUI(), localizerID_0Based + 1);
        }

        public static float GetLevelDuration(LevelConfig level)
        {
            if (level == null)
            {
                Debug_Helper.LogError(typeof(LevelsLibrary), "Level was null");
                return 0;
            }

            return level.wantedDuration * config.wantedDurationMultiplier;
        }
        #endregion

        [System.Serializable]
        public class Config
        {
            public static string GetLocalizerPrefixUI() { return localizerPrefixUI; }
            private const string localizerPrefixUI = "R";

            public int numWorldsFullGame = 4;
            public int numLevelsPerWorld = 4;
            public int numLocalizersPerWorld = 2;

            public float overrideDifficulty = -1;
            public float difficultyMultiplier = 1;
            public float wantedDurationMultiplier = 2.2f;
            public float skipAverageSpawnBeats_Multiplier = 0.75f;
            public bool invertBlueOrangeWorlds = true;
            [NonSerialized]
            public static List<LevelConfig> LEVELS;
            public List<LevelConfig> levels { get { return LEVELS; } }

            /// <summary>
            /// [SOS] Do not use (json only)
            /// </summary>
            public LevelConfig[] _levels;

            public void Initialize(int wantedNumLevelsPerWorld)
            {
                if (levels != null)
                {
                    for (int i = 0; i < levels.Count; i++)
                    {
                        // For Localizers we don't care about their own game type, but for their recorded game type
                        if (levels[i].isLocalizer)
                            continue;
                        levels[i].isInverted = invertBlueOrangeWorlds;
                    }
                }

                PreProcessLevels(wantedNumLevelsPerWorld);
            }

            private void PreProcessLevels(int wantedNumLevelsPerWorld)
            {
                LEVELS = new List<LevelConfig>();

                // Add all tutorials (world = 0)
                int actualLevels = 0;

                foreach (LevelConfig level in _levels)
                {
                    if (level.worldID < 0) // tutorial
                        LEVELS.Add(level);
                    else if (level.isLocalizer) // localizers
                        LEVELS.Add(level);
                    else if (level.levelID_WithinWorld < wantedNumLevelsPerWorld) // acceptable levels
                    {
                        actualLevels++;
                        level.levelID_1Based = actualLevels;
                        LEVELS.Add(level);

                        // this.Log("Added {0} as level #{1} (1-based)"._Format(level.levelName, level.levelID));
                    }
                }
            }
        }

        public class RuntimeConfig
        {
            public int numLevelsPerWorld;

            public RuntimeConfig(int numLevelsPerWorld)
            {
                this.numLevelsPerWorld = numLevelsPerWorld;
            }
        }
    }
}