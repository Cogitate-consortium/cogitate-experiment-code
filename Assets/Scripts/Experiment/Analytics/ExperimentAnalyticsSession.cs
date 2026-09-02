using Game.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TGP.Helpers;
using Helpers.Engine;
using Experiment.Stimulus;
using Experiment.Analytics.Core;
using System;

namespace Experiment.Analytics
{
    /// CLEANUP :: Overlaps with <see cref="ProbeSummary"/> / JourneySummary
    /// <summary>
    /// </summary>
    public class ExperimentAnalyticsSession
    {
        private List<ExperimentAnalyticsLevel> levelAnalytics = new List<ExperimentAnalyticsLevel>();
        private string trackingLevelId = null;

        private float pausedTime = 0;
        private bool isPaused;

        private float trackingSessionStartTime = -1f;

        private List<string> ignoreNames = new List<string>() { "2_5", "3_6" };

        public ExperimentAnalyticsSession()
        {
            EngineWrapper.onUpdate += EngineWrapper_onUpdate;
        }

        private void EngineWrapper_onUpdate(object sender, float e)
        {
            if (isPaused)
                pausedTime += TimeWrapper.deltaTime_SinceLastUpdate_NotTS;
        }

        public void StartTrackingSession()
        {
            trackingSessionStartTime = TimeWrapper.realtimeSinceStartup_NotTS;
        }

        public void ResetTrackingSession(Action<string> writingAction)
        {
            Save(writingAction);

            levelAnalytics.Clear();
            trackingLevelId = null;
            pausedTime = 0;
            isPaused = false;
            trackingSessionStartTime = -1f;
        }

        public void StartTrackingLevel(LevelConfig level)
        {
            string levelName = level.levelName;
            levelAnalytics.Add(new ExperimentAnalyticsLevel(levelName));
            trackingLevelId = levelName;
            pausedTime = 0;
            isPaused = false;
        }

        public void StopTrackingLevel(LevelCompleteArgs e)
        {
            if (trackingLevelId == null)
            {
                // Debug_Helper.LogError(typeof(GameAnalytics), "No tracking level to stop tracking");
                return;
            }

            // If it was aborted, delete
            if (e.endReason == EndReason.Exit)
            {
                this.LogWarning("Aborted level -> Removing tracking info");
                levelAnalytics.RemoveAt(levelAnalytics.Count - 1);
            }
            else
            {
                levelAnalytics.Last().totalTime = TimeWrapper.timeSinceLevelLoad_NotTS;
                levelAnalytics.Last().playTime = TimeWrapper.timeSinceLevelLoad_NotTS - pausedTime;
                levelAnalytics.Last().stars = e.stars;
                levelAnalytics.Last().score = e.score;
                levelAnalytics.Last().scorePercentile = e.scorePercentile;
            }

            trackingLevelId = null;
            isPaused = false;
            // Debug.Log("DONE");
        }

        internal void RemoveLevel(string levelName)
        {
            if (levelAnalytics == null) return;
            ExperimentAnalyticsLevel levelToRemove = levelAnalytics.Find(x => x.levelName == levelName);
            if (levelToRemove == null) return;
            levelAnalytics.Remove(levelToRemove);
            this.LogWarning("Removed level {0} from levelAnalytics"._Format(levelName));
        }

        public void IncreaseProbeShown()
        {
            if (trackingLevelId == null)
            {
                Debug_Helper.LogError(typeof(ExperimentAnalyticsSession), "No tracking level to stop tracking");
                return;
            }
            levelAnalytics.Last().probesShown++;
        }

        public void StimulusShown(StimulusType stimulusType)
        {
            switch (stimulusType)
            {
                case StimulusType.None:
                    levelAnalytics.Last().blankShown++;
                    break;
                case StimulusType.Object:
                    levelAnalytics.Last().objectsShown++;
                    break;
                case StimulusType.Face:
                    levelAnalytics.Last().facesShown++;
                    break;
            }
        }

        public void Pause(bool pause)
        {
            isPaused = pause;
        }

        public float GetAverageStarsAll()
        {
            if (levelAnalytics.Last().levelName.Contains("_1") || levelAnalytics.Count < 2) return -1f;
            float stars = 0;
            for (int i = 0; i < levelAnalytics.Count; i++)
            {
                if (levelAnalytics[i].levelName.ContainsInvariant("l_") || levelAnalytics[i].totalTime == 0) continue;

                stars += levelAnalytics[i].stars;
            }
            return stars / levelAnalytics.Count;
        }

        public float GetAverageStarsThisWorld()
        {
            if (levelAnalytics.Last().levelName.Contains("_1") || levelAnalytics.Count < 2) return -1f;

            string worldPrefix = levelAnalytics.Last().levelName.Split('_')[0] + "_";
            float stars = 0;
            int levelCount = 0;
            for (int i = 0; i < levelAnalytics.Count; i++)
            {
                if (levelAnalytics[i].levelName.ContainsInvariant("l_") || levelAnalytics[i].totalTime == 0) continue;
                if (!levelAnalytics[i].levelName.StartsWith(worldPrefix)) continue;

                stars += levelAnalytics[i].stars;
                levelCount++;
            }

            if (levelCount == 0)
                return -1;
            return stars / levelCount;
        }

        public void Save(Action<string> writingFunction)
        {
            if (writingFunction == null)
            {
                this.LogWarning("not saving, writing function null");
                return;
            }

            StringBuilder logs = new StringBuilder();

            logs.AppendLine("\n### Levels - No Cutscenes ###");
            // Analytics per Level - No CutScenes
            foreach (ExperimentAnalyticsLevel level in levelAnalytics)
            {
                if (ignoreNames.Contains(level.levelName)) continue;
                logs.AppendLine(level.ToString());
            }

            // Summary per World
            Dictionary<string, List<ExperimentAnalyticsLevel>> perWorldAnalytics = new Dictionary<string, List<ExperimentAnalyticsLevel>>();
            List<ExperimentAnalyticsLevel> perCutSceneAnalytics = new List<ExperimentAnalyticsLevel>();
            List<ExperimentAnalyticsLevel> orderedResults = levelAnalytics.OrderBy(o => o.levelName).ToList();

            // Sort by World
            for (int i = 0; i < orderedResults.Count; i++)
            {
                // Check for cutscenes
                if (ignoreNames.Contains(orderedResults[i].levelName))
                {
                    perCutSceneAnalytics.Add(orderedResults[i]);
                }
                else
                {
                    // Extract world by pattern '{world}_3' 
                    string worldID = orderedResults[i].levelName.Split('_')[0];
                    // Add level to corresponding world
                    if (!perWorldAnalytics.ContainsKey(worldID))
                        perWorldAnalytics.Add(worldID, new List<ExperimentAnalyticsLevel>());
                    perWorldAnalytics[worldID].Add(orderedResults[i]);
                }
            }

            logs.AppendLine("\n### Cutscenes ###");
            for (int i = 0; i < perCutSceneAnalytics.Count; i++)
            {
                logs.AppendLine(perCutSceneAnalytics[i].ToString());
            }

            logs.AppendLine("\n### Summary - No Cutscenes ###");
            // Log Summary per World
            foreach (string key in perWorldAnalytics.Keys)
            {
                string sumName = string.Format("World {0} Summary", key);
                ExperimentAnalyticsLevel sum = new ExperimentAnalyticsLevel(sumName);
                List<ExperimentAnalyticsLevel> worldAnalytics = perWorldAnalytics[key];
                string levels = "";
                for (int i = 0; i < worldAnalytics.Count; i++)
                {
                    levels += worldAnalytics[i].levelName + ", ";
                    sum += worldAnalytics[i];
                }

                sum.levelName = sumName;

                logs.AppendLine(string.Format("World {0} Summary | Levels {1} | {2}", key, levels, sum.ToString()));
                logs.AppendLine(string.Format("World {0} Average | Levels {1} | {2}", key, levels, (sum / worldAnalytics.Count).ToString()));
            }

            logs.AppendLine(string.Format("Total session time:{0} seconds", (TimeWrapper.realtimeSinceStartup_NotTS - trackingSessionStartTime)));

            string logsToWrite = logs.ToString();

            writingFunction?.Invoke(logsToWrite);
        }

        public ExperimentAnalyticsLevel GetSum()
        {
            ExperimentAnalyticsLevel total = new ExperimentAnalyticsLevel("total");

            foreach (ExperimentAnalyticsLevel lA in levelAnalytics)
                total += lA;

            total.levelName = "Total";

            return total;
        }
    }
}