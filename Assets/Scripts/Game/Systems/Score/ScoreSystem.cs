// NS_REMOVE
using ExperimentLibrary;

// NS_DEBATABLE
using Game.Core;
// NS_DEBATABLE
using Game.Entities.Interactables;
using Game.Managers.SessionManagers;
using System;
using TGP.Helpers;
using UnityEngine;

namespace Game.Systems.Score
{
    /// <summary>
    /// [EXPAND] Potentially used when <see cref="GameManager"/> breaks down into ScoreManager
    /// Maybe move into UI_GAME_CORE and let all go through <see cref="InGameUI"/>
    /// </summary>
    public class ScoreSystem : MonoBehaviour
    {
        public event EventHandler<FloatChangedArgs> onScoreChanged;

        public bool isPaused { get; protected set; }
        public float progress { get { return ConvertScoreToProgress_TS(score); } }
        public float score { get; protected set; }
        public float scoreMultiplier { get; protected set; }
        public float highscore { get; private set; }

        [Header("Editor Values")]
        /// <summary>
        /// Score display normalizer. For now, set to target Survival Time. For visualization only (un-normalized score is being logged).
        /// </summary>
        [SerializeField, Range(10, 50)] protected float asymptoteTargetScore = 30;

        protected static ScoreConfig config { get { return ExperimentLibraryManager.Config.PlayerProgression.score; } }
        protected LevelConfig currentLevel { get { return runtimeConfig.level; } }
        protected int scoreTarget { get { return runtimeConfig.scoreTarget; } }
        private RuntimeConfig runtimeConfig;

        #region Public Methods

        public void Pause(bool doPause)
        {
            isPaused = doPause;
        }

        private ScoreConfig.Type config_Type;

        /// <summary>
        /// Clamps to 01
        /// </summary>
        public float ConvertScoreToProgress_TS(float score)
        {
            float score01 = score;

            switch (config_Type)
            {
                case ScoreConfig.Type.Linear:
                    score01 = score / scoreTarget;
                    break;
                case ScoreConfig.Type.Asymptote:
                    score01 = 1 - asymptoteTargetScore / (score + asymptoteTargetScore);
                    break;
            }

            return score01.Clamped01();
        }

        public float GetScorePercentile_TS()
        {
            return ConvertScoreToProgress_TS(score);
        }

        public class RuntimeConfig
        {
            public LevelConfig level;
            public int scoreTarget;

            public RuntimeConfig(LevelConfig level, int scoreTarget)
            {
                this.level = level;
                this.scoreTarget = scoreTarget;
            }
        }

        public virtual void Initialize(RuntimeConfig runtimeConfig)
        {
            this.runtimeConfig = runtimeConfig;
            config_Type = config.type;
            scoreMultiplier = 1f;
            highscore = 0;
            SetScore(0);
            Pause(false);
        }

        public virtual void ScorePlayerDeath()
        {
            // Decrease score by percent
            float scorePercentile = highscore * config.scorePercentileToKeepAfterDefeat;
            float scoreReduction = highscore - config.scoreMaxLossAfterDefeat;

            float score = Mathf.Max(scorePercentile, scoreReduction);

            SetScore(score);
        }

        public float ScorePlayerHealing(ObjectColorType healedByType)
        {
            float healing = healedByType == ObjectColorType.BlueHigh || healedByType == ObjectColorType.OrangeHigh ?
                config.highEssenseScore : config.essenseScore;

            float pointsIncrement = healing * scoreMultiplier;
            SetScore(score + pointsIncrement);
            return pointsIncrement;
        }

        public void ScorePlayerDamage(ObjectColorType defeatedByType)
        {
            float damage =
                defeatedByType == ObjectColorType.BlueHigh || defeatedByType == ObjectColorType.OrangeHigh ?
                config.highEssenseScore : config.essenseScore;

            float oldScoreProgression = ConvertScoreToProgress_TS(score);

            if (score > scoreTarget)
                damage *= 4; // penalize when we are above the limit 

            float newScore = Mathf.Max(score - damage, 0);

            float newScoreProgression = ConvertScoreToProgress_TS(newScore);

            if (oldScoreProgression >= config.thresholdForStars_3 && newScoreProgression < config.thresholdForStars_3)
            {
                SetScore(scoreTarget * config.thresholdForStars_3);
            }
            else if (oldScoreProgression >= config.thresholdForStars_2 && newScoreProgression < config.thresholdForStars_2)
            {
                SetScore(scoreTarget * config.thresholdForStars_2);
            }
            else if (oldScoreProgression >= config.thresholdForStars_1 && newScoreProgression < config.thresholdForStars_1)
            {
                SetScore(scoreTarget * config.thresholdForStars_1);
            }
            else
            {
                SetScore(newScore);
            }
        }

        public int GetStars()
        {
            if (ConvertScoreToProgress_TS(score) >= config.thresholdForStars_3)
                return 3;
            if (ConvertScoreToProgress_TS(score) >= config.thresholdForStars_2)
                return 2;
            if (ConvertScoreToProgress_TS(score) >= config.thresholdForStars_1)
                return 1;
            return 0;
        }

        public virtual void IncreaseScore(Interactable interactable) { }

        public virtual void Reset()
        {
            SetScore(0);
        }

        public virtual void SetScoreMultiplier(float multiplier)
        {
            scoreMultiplier = multiplier;
        }

        #endregion

        protected virtual void SetScore(float newScore)
        {
            float _scoreBeforeChange = score;
            score = newScore;
            highscore = Mathf.Max(highscore, score);
            RaiseOnScoreChanged(new FloatChangedArgs(newScore, newScore - _scoreBeforeChange));
        }

        private void RaiseOnScoreChanged(FloatChangedArgs score)
        {
            onScoreChanged?.Invoke(this, score);
        }
    }
}