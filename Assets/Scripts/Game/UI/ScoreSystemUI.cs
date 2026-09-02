using Game.Systems.Score;
using TGP.Helpers;
using UnityEngine;

namespace Game.UI
{
    public class ScoreSystemUI : ScoreSystem
    {
        protected CanvasGroup cg;

        [Header("Editor")]
        public ThemeSlider scoreProgressSlider = null;

        public override void Initialize(RuntimeConfig runtimeConfig)
        {
            base.Initialize(runtimeConfig);

            cg = gameObject.AddComponentIfNotExists<CanvasGroup>();
            scoreProgressSlider.SetValue(0);
        }

        #region UI

        protected override void SetScore(float newScore)
        {
            base.SetScore(newScore);
            UpdateUI();
        }

        protected virtual void UpdateUI()
        {
            scoreProgressSlider.SetValue(base.ConvertScoreToProgress_TS(base.score), true);
        }

        public virtual void Toggle(bool show)
        {
            cg.Toggle(show);
        }


        #endregion


    }
}