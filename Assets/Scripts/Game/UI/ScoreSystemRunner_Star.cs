using Game.Core;
using Game.Entities.Interactables;
using System.Collections.Generic;
using TGP.Helpers;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// [CLEANUP] This is the one we're using
    /// </summary>
    public class ScoreSystemRunner_Star : ScoreSystemUI
    {
        [Header("Editor")]
        public List<GameObject> emptyStarIcons = new List<GameObject>();
        public TextMeshProUGUI scoreMultiplierText;
        public TextMeshProUGUI scoreText;
        private int maxStars = 3;

        #region Logic

        // DEPRECATE
        [System.Obsolete("Turned into SetScore")]
        public override void IncreaseScore(Interactable interactable)
        {
            float increment = config.essenseScore;
            if (interactable.colorType.ToString().ContainsInvariant("high"))
                increment = config.highEssenseScore;

            SetScore(score + (increment * scoreMultiplier));
        }

        protected override void SetScore(float newScore)
        {
            //newScore = base.ConvertScoreToProgress(newScore).Round(1f / (float)base.scoreTarget) * base.scoreTarget;
            base.SetScore(newScore);

            // Empty stars are OVER the normal, seting them to false makes them show
            int stars = GetStars();

            for (int i = 0; i < maxStars; i++)
                emptyStarIcons[i].SetActive(stars > i);
        }

        public override void ScorePlayerDeath()
        {
            float oldScoreProgression = base.ConvertScoreToProgress_TS(base.score);
            base.ScorePlayerDeath();

            float newScoreProgression = base.ConvertScoreToProgress_TS(base.score);
            if (oldScoreProgression > 0.80f && newScoreProgression < 0.80f)
            {
                SetScore(base.scoreTarget * 0.80f);
            }
            else if (oldScoreProgression > 0.4f && newScoreProgression < 0.4f)
            {
                SetScore(base.scoreTarget * 0.4f);
            }
            else if (oldScoreProgression > 0.15f && newScoreProgression < 0.15f)
            {
                SetScore(base.scoreTarget * 0.15f);
            }
        }

        #endregion

        #region UI

        protected override void UpdateUI()
        {
            base.UpdateUI();
            scoreMultiplierText.text = string.Format("x{0}", base.scoreMultiplier);
            scoreText.text = Mathf.FloorToInt(base.score).ToString();
        }

        public override void SetScoreMultiplier(float multiplier)
        {
            base.SetScoreMultiplier(multiplier);
            UpdateUI();
        }

        #endregion

    }
}