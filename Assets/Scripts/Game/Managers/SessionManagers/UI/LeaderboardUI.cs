// NS_REMOVE | Config, Other Configs (Text, Subject, Global)
using ExperimentLibrary;

using System;
using System.Collections.Generic;
using System.Linq;
using TGP.Helpers;
using Helpers.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using Helpers.UI.Menus;
using Game.Core;
using Helpers.Engine;

namespace Game.Managers.SessionManagers.UI
{
    /// Lots of overlap with <see cref="GameplayManager.UI.Game_PopUp"/>
    public class LeaderboardUI : MenuManager
    {
        #region TEMP TODO Remove
        private static string subjectNumber;
        public static void InitializeSubject(string subjectNumber)
        {
            LeaderboardUI.subjectNumber = subjectNumber;
        }
        #endregion

        private static Config config { get { return ExperimentLibraryManager.Config.Leaderboard; } }
        private static List<Config.LeaderboardOpponent> opponents = new List<Config.LeaderboardOpponent>();

        public event EventHandler<EventArgs> onHide;

        public GameObject itemParent;
        public LeaderboardItem itemPrefab;
        public Button backButton;
        public Text titleText;
        public Text okayText;

        private bool isVisible = false;

        bool onlyAllowProceedThroughMouse;

        private void Start()
        {
            base.Initialize();
            this.gameObject.SetActive(isVisible);
            onlyAllowProceedThroughMouse = ExperimentLibraryManager.Config.Texts.onlyAllowProceedThroughMouse_PopUps;
            backButton.onClick.AddListener(OnBackButtonClicked);
            titleText.text = ExperimentLibraryManager.Config.Texts.leaderBoardTitle;
            okayText.text = onlyAllowProceedThroughMouse ?
                ExperimentLibraryManager.Config.Texts.popupOkayText_mouseOnly :
                ExperimentLibraryManager.Config.Texts.popupOkayText;
        }

        public static void Reset()
        {
            opponents = new List<Config.LeaderboardOpponent>(config.opponents);
        }
        
        private void OnBackButtonClicked()
        {
            Toggle(false);
        }

        public static void SetProgressFromCSV(IList<string> csv)
        {
            Reset();

            if (csv == null || csv.Count < 2)
            {
                // Debug.LogWarning("Could not load progress from CSV");
                return;
            }

            // Ignore header
            for (int i = 1; i < csv.Count; i++)
            {
                string csvLine = csv[i];

                string[] fields = csvLine.Split(';');

                string opponentName = fields[0];
                int levelID_1Based = fields[1].ToInt();
                float points = fields[2].ToFloat();

                Config.LeaderboardOpponent opponent = opponents.Find(x => x.name == opponentName);

                if (opponent == null)
                {
                    Debug.LogError("We got a CSV entry not corresponding to an opponent. Shouldn't happen!");
                    continue;
                }

                // This guarantees the latest score will be used (though we don't expect to find more than one)
                opponent.score.AddScoreFor(levelID_1Based, points);
            }
        }

        public static string GetOpponentScoresCSV()
        {
            string csv = "OpponentName;LevelID_1Based;Points\r\n";

            foreach (Config.LeaderboardOpponent lO in opponents)
                foreach (LeaderboardOpponentScore.LevelScore lS in lO.score.levelScores)
                    csv += "{0};{1};{2}\r\n"._Format(lO.name, lS.levelID, lS.score);

            return csv;
        }


        public void Toggle(bool show)
        {
            this.gameObject.SetActive(show);
            if (show)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(backButton.gameObject);

            if (this.isVisible && !show)
                RaiseOnHide();

            this.isVisible = show;
        }

        private void RaiseOnHide()
        {
            onHide?.Invoke(this, new EventArgs());
        }

        public static void AddCompleteLevel(LevelConfig level)
        {
            if (level == null)
            {
                if (EngineWrapper.Debug_IsDebugBuild)
                    Debug.LogError("Level was null!");
                return;
            }
            float targetLevelScore = PlayerProgression.TargetScorePerLevel(level.levelID_1Based) * config.targetLevelScoreMultiplier;
            ProgressionLevel progressionLevel = PlayerProgression.GetLevel(level.levelID_1Based);

            if (progressionLevel == null)
            {
                if (EngineWrapper.Debug_IsDebugBuild)
                    Debug.LogError("Level was null!");
                return;
            }

            float playerScore = progressionLevel.points;

            // Add score per opponent
            for (int i = 0; i < opponents.Count; i++)
            {
                if (opponents[i].score.ContainsScoreFor(level.levelID_1Based)) continue;

                float diff = UnityEngine.Random.Range(opponents[i].differenceScoreMinMax.x, opponents[i].differenceScoreMinMax.y);
                float threshold = config.thresholdPlayerTried;
                float baseScore = opponents[i].withRespectTo == Config.Type.Player &&
                    playerScore > targetLevelScore * threshold ? // Make sure the player tried (if not, don't drag the bots with the player)
                    playerScore : targetLevelScore;

                float levelScore = baseScore * (1 + diff);
                float scoreMulti = opponents[i].scoreMultiplier * config.opponentScoreMultiplier;
                opponents[i].score.AddScoreFor(level.levelID_1Based, levelScore * scoreMulti);
            }
        }

        public static void RemoveCompleteLevel(int levelID_1Based)
        {
            // Add score per opponent
            for (int i = 0; i < opponents.Count; i++)
            {
                if (opponents[i].score.ContainsScoreFor(levelID_1Based)) continue;

                opponents[i].score.RemoveScoreFor(levelID_1Based);
            }
        }

        public void DisplayBoard()
        {
            Config.LeaderboardOpponent self = new Config.LeaderboardOpponent();
            
            foreach (ProgressionLevel pL in PlayerProgression.CompleteLevels)
                self.score.AddScoreFor(pL.levelID_1Based, pL.points);

            List<Config.LeaderboardOpponent> orderedOpponents = new List<Config.LeaderboardOpponent>(opponents);
            
            // Add self
            orderedOpponents.Add(self);

            // Order list
            orderedOpponents = orderedOpponents.OrderByDescending(i => i.score.GetTotalScore()).ToList();

            // Clear previous entries
            itemParent.transform.Genocide();
            // Create entry per opponent; including slef
            for (int i = 0; i < orderedOpponents.Count; i++)
            {
                LeaderboardItem item = GameObject.Instantiate(itemPrefab, itemParent.transform).GetComponent<LeaderboardItem>();
                string _name = orderedOpponents[i].name;
                // Does the name match the player's number ?
                if (_name == subjectNumber.ToString()) // This can't be true twice
                    _name = config.backupName; // So one backup name is enough

                bool prependLabCode = config.prependLabCodeToOpponentNames;
                string name =
                    orderedOpponents[i] == self ? ExperimentLibraryManager.Config.Texts.leaderBoardSubjectName :
                    ((prependLabCode ? ExperimentLibraryManager.Config.LabCode : "") + _name);

                item.SetEntry(i + 1, name, orderedOpponents[i].score.GetTotalScore());

                if (orderedOpponents[i] == self)
                    item.SetActive();
            }
        }

        protected override void OnEscapePressed()
        {
            base.OnEscapePressed();
            Toggle(false);
        }

        protected override void OnYesPressed()
        {
            if (onlyAllowProceedThroughMouse)
            {
                this.LogWarning("Prevented key - mouses only!");
                return;
            }

            base.OnYesPressed();
            Toggle(false);
        }

        [Serializable]
        public class Config
        {
            public bool prependLabCodeToOpponentNames = true;
            public float thresholdPlayerTried = 0.25f;
            public List<LeaderboardOpponent> opponents = new List<LeaderboardOpponent>() { new LeaderboardOpponent() };
            public string backupName = "128";
            public float targetLevelScoreMultiplier = 1.0f;
            public float opponentScoreMultiplier = 1.0f;

            [Serializable]
            public class LeaderboardOpponent
            {
                public string name;
                public Vector2 differenceScoreMinMax = new Vector2(-2f, 2f);
                public Type withRespectTo = Type.Player;
                public float scoreMultiplier = 1;

                [NonSerialized] public LeaderboardOpponentScore score = new LeaderboardOpponentScore();
            }

            public enum Type { Player, Level }
        }

        [Serializable]
        public class LeaderboardOpponentScore
        {
            public List<LevelScore> levelScores = new List<LevelScore>();

            internal void AddScoreFor(int levelID_1Based, float v)
            {
                levelScores.Add(new LevelScore(levelID_1Based, v));
            }

            internal bool ContainsScoreFor(int levelID_1Based)
            {
                return levelScores.Find(a => a.levelID == levelID_1Based) != null;
            }

            internal float GetTotalScore()
            {
                return levelScores.Sum(x => Mathf.Max(0, x.score));
            }

            internal void RemoveScoreFor(int levelID_1Based)
            {
                levelScores.RemoveAll(x => x.levelID == levelID_1Based);
            }

            [Serializable]
            public class LevelScore
            {
                public int levelID = -1;
                public float score = -1;

                public LevelScore(int levelID, float score)
                {
                    this.levelID = levelID;
                    this.score = score;
                }
            }
        }
    }
}