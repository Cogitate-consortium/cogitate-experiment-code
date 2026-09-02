// NS_REMOVE | Level Duration
using Experiment.Task;

using Game.Core;
using Game.Managers.LevelManagers;
using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Game.Managers.SessionManagers
{
    public static class PlayerProgression
    {
        public static int numLocalizers { get { return runtimeConfig.numLocalizers; } }
        public static int numGameWorlds { get { return runtimeConfig.numGameWorlds; } }
        public static int numLevelsPerWorld { get { return runtimeConfig.numLevelsPerWorld; } }
        public static int numLevels { get { return runtimeConfig.numLevels; } }

        // TODO :: Those should not have public gets
        public static bool? isLevelStarted { get { return levelManager?.isGameStarted; } }
        public static bool? hasLevelEnded { get { return levelManager?.isExited; } }

        public static LevelMasterManager levelManager { get; private set; }
        public static LevelConfig ActiveLevel { get; private set; }

        private static RuntimeConfig runtimeConfig;

        private static Config config;

        public static void Initialize(Config config, RuntimeConfig runtimeConfig)
        {
            LevelsLibrary.Initialize(config.levelsLibrary,
                new LevelsLibrary.RuntimeConfig(runtimeConfig.numLevelsPerWorld));

            PlayerProgression.config = config;
            PlayerProgression.runtimeConfig = runtimeConfig;
            ProgressionLevel.Initialize(config.score);
        }

        public static void InitializeLevelManager(LevelMasterManager.RuntimeConfig levelRuntimeConfig)
        {
            // Debug.LogError(typeof(PlayerProgression) + " InitializeLevelManager");

            LevelConfig levelConfig = levelRuntimeConfig.levelConfig;

            if (levelConfig.levelID_1Based < 1)
            {
                if (levelConfig.levelID_1Based == -2)
                    levelManager = new GameObject().AddComponent<LevelMasterManager_Game_Tutorial_T>();
                else if (levelConfig.levelID_1Based == -1)
                    levelManager = new GameObject().AddComponent<LevelMasterManager_Game_Tutorial_I>();
                else if (levelConfig.levelID_1Based == 0)
                    levelManager = new GameObject().AddComponent<LevelMasterManager_Game_Tutorial_P>();
            }
            else if (levelConfig.isLocalizer)
            {
                levelManager = new GameObject().AddComponent<LevelMasterManager_Localizer>();
            }
            else
            {
                if (levelConfig.GetGameType() == GameType.Runner_2D_Blue)
                {
                    levelManager = new GameObject().AddComponent<LevelMasterManager_Game_Blue>();
                }
                else if (levelConfig.GetGameType() == GameType.Runner_2D_Orange)
                {
                    levelManager = new GameObject().AddComponent<LevelMasterManager_Game_Orange>();
                }
            }

            levelManager.Initialize(config.levelMasterManager, levelRuntimeConfig);

            levelManager.onLevelComplete += LM_onLevelComplete;
            levelManager.gameObject.name = levelManager.GetType().ToString();

            ActiveLevel = levelConfig;
        }

        public static void CompleteCurrentLevel()
        {
            levelManager?.CompleteLevel();
            /// This will then callback <see cref="LM_onLevelComplete(object, LevelMasterManager.LevelCompleteArgs)"/>
        }

        private static void LM_onLevelComplete(object sender, LevelCompleteArgs e)
        {
            if (e.endReason == EndReason.Success)
            {
                AddCompleteLevel(e.levelConfig.levelID_1Based, e.score);
                Debug_Helper.LogWarning(typeof(PlayerProgression), "Level Completed Successfully!");
            }
            else if (e.endReason == EndReason.Failure && runtimeConfig.allowProceedWithFailure)
            {
                AddCompleteLevel(e.levelConfig.levelID_1Based, e.score);
                Debug_Helper.LogWarning(typeof(PlayerProgression), "Level Completed Unsuccessfully - but we allow that");
            }
            else
            {
                Debug_Helper.LogWarning(typeof(PlayerProgression), "Level Completed Usuccessfully / Exited | " + e.endReason);
            }

            onLevelComplete?.Invoke(null, e);
        }

        public static void StopLevel()
        {
            // TODO 200619 probably needed
            // ActiveLevel = null;
            levelManager?.DeInitialize();
        }

        public static event EventHandler<LevelCompleteArgs> onLevelComplete;


        public static readonly List<ProgressionLevel> CompleteLevels = new List<ProgressionLevel>();
        public static int GetCurrentLocalizerID()
        {
            return ActiveLevel != null ? ActiveLevel.localizerID_0Based : -1;
        }

        public static void SetProgressFromCSV(IList<string> csv)
        {
            if (csv == null || csv.Count < 2)
            {
                // Debug.LogWarning("Could not load progress from CSV");
                return;
            }

            CompleteLevels.Clear();
            // Ignore first line (headers)
            for (int i = 1; i < csv.Count; i++)
            {
                string csvLine = csv[i];
                string[] fields = csvLine.Split(';');

                int levelID_1Based = fields[0].ToInt();
                float points = fields[1].ToFloat();

                CompleteLevels.Add(new ProgressionLevel(levelID_1Based, points));
            }
        }

        public static string GetCompletedLevelsCSV()
        {
            string csv = "LevelID_1Based;Points\r\n";

            foreach (ProgressionLevel pL in CompleteLevels)
                csv += "{0};{1}\r\n"._Format(pL.levelID_1Based, pL.points);

            return csv;
        }

        /*
        public static int GetActiveWorldID()
        {
            return GetActiveWorldID(ActiveLevel);
        }

        /// <summary>
        /// 0-based world ID
        /// </summary>
        public static int GetActiveWorldID(LevelConfig localizerLevel)
        {
            if (ActiveLevel == null)
                return -1;

            if (!ActiveLevel.isLocalizer)
                return ActiveLevel.worldID;
            else
                return GetReplayLevel(ActiveLevel).worldID;
        }
        */

        public static bool HasCompletedGame()
        {
            bool hasCompletedGameLevels =
                GetNumCompletedWorlds() >= runtimeConfig.numGameWorlds;

            bool hasCompletedLocalizers =
                GetNumCompletedLocalizers() >= runtimeConfig.numLocalizers;

            return hasCompletedGameLevels && hasCompletedLocalizers;
        }

        public static int GetNumCompletedLocalizers()
        {
            int numCompletedLocalizers = 0;

            for (int i = 0; i < runtimeConfig.numLocalizers; i++)
            {
                int localizerID_0Based = LevelConfig.localizerOffset + i;

                if (HasCompletedLocalizer(localizerID_0Based))
                    numCompletedLocalizers++;
            }

            return numCompletedLocalizers;
        }

        public static int GetNumCompletedWorlds()
        {
            int numCompletedWorlds = 0;

            for (int i = 0; i < runtimeConfig.numGameWorlds; i++)
                if (HasCompletedWorld(i))
                    numCompletedWorlds++;

            return numCompletedWorlds;
        }

        public static bool HasCompletedWorld(int worldID)
        {
            int numLevelsPerWorld = runtimeConfig.numLevelsPerWorld;
            for (int j = 0; j < numLevelsPerWorld; j++)
            {
                int levelID_0Based = worldID * numLevelsPerWorld + j;
                int levelID_1Based = levelID_0Based + 1;

                if (!HasCompletedGameLevel(levelID_1Based)) return false;
            }

            return true;
        }

        // [TODO] [Spaghetti, IDs]
        public static bool HasCompletedLocalizer(int localizerId_0Based)
        {
            return HasCompletedLevel(localizerId_0Based);
        }

        public static bool HasCompletedGameLevel(int gamelevelId_1Based)
        {
            return HasCompletedLevel(gamelevelId_1Based);
        }

        /// <summary>
        /// [SOS] Should only be used by <see cref="HasCompletedGameLevel(int)"/> and <see cref="HasCompletedLocalizer(int)"/>
        /// </summary>
        private static bool HasCompletedLevel(int levelID)
        {
            for (int i = 0; i < CompleteLevels.Count; i++)
            {
                if (CompleteLevels[i].Level.levelID_1Based == levelID)
                    return true;
            }
            return false;
        }

        public static ProgressionLevel GetLevel(int levelId)
        {
            for (int i = 0; i < CompleteLevels.Count; i++)
            {
                if (CompleteLevels[i].Level.levelID_1Based == levelId)
                    return CompleteLevels[i];
            }
            return null;
        }

        public static float GetTotalPoints()
        {
            float totalPoints = 0;
            for (int i = 0; i < CompleteLevels.Count; i++)
                totalPoints += Mathf.Max(0, CompleteLevels[i].points);
            return totalPoints;
        }

        public static float GetTotalStars()
        {
            float totalStars = 0;
            for (int i = 0; i < CompleteLevels.Count; i++)
                totalStars += Mathf.Max(0, CompleteLevels[i].GetStars());
            return totalStars;
        }
        
        #region Checks
        public static bool IsLastLevelOfGame(LevelConfig currentLevel)
        {
            return currentLevel.worldID == runtimeConfig.numGameWorlds - 1 && 
                currentLevel.levelID_WithinWorld == runtimeConfig.numLevelsPerWorld - 1;
        }

        /// <summary>
        /// Returns true for >= ceil(max/2)
        /// </summary>
        public static bool IsLevelInSecondHalf(LevelConfig level)
        {
            if (level.isLocalizer)
            {
                int localizerID_1Based = level.localizerID_0Based + 1;
                return localizerID_1Based > Mathf.CeilToInt(runtimeConfig.numLocalizers / 2f);
            }
            else
            {
                int worldID_1Based = level.worldID + 1;
                return worldID_1Based > Mathf.CeilToInt(runtimeConfig.numGameWorlds / 2f);
            }
        }

        public static bool IsLastWorldOfGame(int worldID_ZeroBased)
        {
            return worldID_ZeroBased + 1 == runtimeConfig.numGameWorlds;
        }

        public static bool IsLastLevelOfWorld(LevelConfig level)
        {
            if (level == null) return false;

            string levelName = level.levelName;

            // If it's the tutorial practice
            if (level.isTutorial_P) return true;

            // If it's a localizer
            if (levelName.ContainsInvariant("L_{0}"._Format(runtimeConfig.numLocalizers))) return true;

            // If it's a level
            for (int w = 0; w < runtimeConfig.numGameWorlds; w++)
                if (levelName.ContainsInvariant("{0}_{1}"._Format(w + 1, runtimeConfig.numLevelsPerWorld)))
                    return true;

            return false;
        }
        #endregion

        /*
        public static void AddDummyLevel(int levelIDToComplete)
        {
            CompleteLevels.Add(new ProgressionLevel(LevelsLibrary.GetLevel(levelIDToComplete), 0));
        }

        public static void AddDummyLevel(int startIndex, int endIndexIncluded)
        {
            for (int i = startIndex; i < endIndexIncluded + 1; i++)
                AddDummyLevel(i);
        }
        */

        public static void RemoveCompleteLevel(int levelID_1Based)
        {
            if (!HasCompletedGameLevel(levelID_1Based))
                return;

            int idx = CompleteLevels.FindIndex(a => a.levelID_1Based == levelID_1Based);
            CompleteLevels.RemoveAt(idx);
        }

        public static void AddCompleteLevel(int levelID_1Based, float score)
        {
            if (!HasCompletedGameLevel(levelID_1Based))
                CompleteLevels.Add(new ProgressionLevel(levelID_1Based, score));
            else
            {
                // Find previous entry
                for (int i = 0; i < CompleteLevels.Count; i++)
                {
                    if (CompleteLevels[i].Level.levelID_1Based == levelID_1Based)
                    {
                        // Override if we have higher score
                        if (CompleteLevels[i].points < score)
                            CompleteLevels[i].points = score;
                    }
                }
            }
        }

        public static void Clear()
        {
            CompleteLevels.Clear();
        }

        public static void Unlock_TutorialP()
        {
            AddCompleteLevel(LevelConfig.tutorial_I_ID_1Based, -1);
        }

        public static void UnlockLevel1()
        {
            AddCompleteLevel(LevelConfig.tutorial_P_ID_1Based, -1);
        }
        
        public class RuntimeConfig
        {
            public bool allowProceedWithFailure;

            public float normalToVibratingRatio { get; private set; }

            public RuntimeConfig(int numGameWorlds, int numLevelsPerWorld, int numLocalizersPerGameWorld, float normalToVibratingRatio, bool allowProceedWithFailure)
            {
                this.numGameWorlds = numGameWorlds;
                this.numLevelsPerWorld = numLevelsPerWorld;
                this.numLocalizersPerGameWorld = numLocalizersPerGameWorld;
                this.normalToVibratingRatio = normalToVibratingRatio;
                this.allowProceedWithFailure = allowProceedWithFailure;
            }

            public int numLevels { get { return numGameWorlds * numLevelsPerWorld; } }
            public int numLocalizers { get { return numLocalizersPerGameWorld * numGameWorlds; } }

            public int numGameWorlds { get; private set; }
            public int numLevelsPerWorld { get; private set; }
            public int numLocalizersPerGameWorld { get; private set; }
        }

        [Serializable]
        public class Config
        {
            public ScoreConfig score;
            public LevelsLibrary.Config levelsLibrary;
            public LevelMasterManager.Config levelMasterManager;
        }

        public static int TargetScoreTotal()
        {
            int t = 0;
            for (int i = 0; i < runtimeConfig.numGameWorlds; i++)
            {
                for (int j = 0; j < runtimeConfig.numLevelsPerWorld; j++)
                {
                    t += TargetScorePerLevel(i * runtimeConfig.numLevelsPerWorld + j);
                }
            }

            return t;
        }

        public static int TargetScorePerLevel(int levelID_1Based)
        {
            LevelConfig level = LevelsLibrary.GetLevel(levelID_1Based);

            if (level == null) return 0;
            //float wantedDuration = Config.Level.GetLevelDuration(level);
            double wantedDuration = Precalculator.GetLevelDuration(level);

            /*
             *  Task Relevant
            float accuracy = Config.pilotGoals.taskIrrelevantGoal.accuracy;
            float stimulusDuration = Config.Stimulus.stimulusDurationSeconds;
            float wantedFaces = wantedDuration / (Config.Stimulus.backgroundToStimulusRatio_Gameplay / accuracy);
            return Mathf.CeilToInt(wantedFaces);
            */

            // Objects per second
            float objectsPerSecond = 2f;
            float beneficialToDetrimental = 0.5f;
            float beneficialObjectsPerSecond = objectsPerSecond * beneficialToDetrimental;
            float detrimentalObjectsPerSecond = objectsPerSecond * (1 - beneficialToDetrimental);

            float percentileVibrating = 1 / (1 + runtimeConfig.normalToVibratingRatio);
            float beneficialVibratingPerSecond = beneficialObjectsPerSecond * percentileVibrating;
            float beneficialNormalPerSecond = beneficialObjectsPerSecond * (1 - percentileVibrating);
            float detrimentalVibratingPerSecond = detrimentalObjectsPerSecond * percentileVibrating;
            float detrimentalNormalPerSecond = detrimentalObjectsPerSecond * percentileVibrating;

            // Expected Values
            float expectedCatchRate = 0.75f;
            float expectedDodgeRate = 0.75f;
            float beneficialExpectedValue = (beneficialNormalPerSecond * config.score.essenseScore + beneficialVibratingPerSecond * config.score.highEssenseScore) * expectedCatchRate;
            float detrimentalExpectedValue = (detrimentalNormalPerSecond * config.score.essenseScore + detrimentalVibratingPerSecond * config.score.highEssenseScore) * (1 - expectedDodgeRate);

            float expectedValue = (float)((beneficialExpectedValue - detrimentalExpectedValue) * wantedDuration);

            expectedValue *= config.score.scoreMultiplier;

            return (int)(Mathf.CeilToInt(expectedValue) * config.score.neededScorePerLevelMultiplier);
        }

        internal static void TEMP_RequestStart(Action<bool> onStarted)
        {
            levelManager.TEMP_RequestStart(onStarted);
        }
    }

    [System.Serializable]
    public class ProgressionLevel
    {
        private static ScoreConfig config;

        public static void Initialize(ScoreConfig config)
        {
            ProgressionLevel.config = config;
        }

        public LevelConfig Level { get { return LevelsLibrary.GetLevel(levelID_1Based);} }
        public int levelID_1Based;
        public float points;

        public ProgressionLevel(int levelID_1Based, float points)
        {
            this.levelID_1Based = levelID_1Based;
            this.points = points;
        }

        public int GetStars()
        {
            if (Level == null) return -1;
            float progress = points / (float)PlayerProgression.TargetScorePerLevel(Level.levelID_1Based);

            if (progress >= config.thresholdForStars_3)
                return 3;
            if (progress >= config.thresholdForStars_2)
                return 2;
            if (progress >= config.thresholdForStars_1)
                return 1;
            if (progress >= 0)
                return 0;
            return -1;
        }
    }
}