using System.Collections.Generic;
using Helpers.UI.Core;
using UnityEngine;

namespace Game.UI
{
    public class InGameUI_Duet : InGameUI
    {
        [SerializeField] protected GameObject LivesParentOrange = null;
        [SerializeField] protected HeartObjectUI LivesPrefabOrange = null;

        private List<HeartObjectUI> livesObjectsOrange = new List<HeartObjectUI>();
        private float lastLivesOrange = 0;

        public override void ToggleProgressUI(bool isVisible)
        {
            base.ToggleProgressUI(isVisible);
            LivesParentOrange.SetActive(isVisible);
        }

        public override void SetMaxLives(float maxLives)
        {
            base.SetMaxLives(maxLives);

            SetupLivesUI(livesObjectsOrange, LivesParentOrange, LivesPrefabOrange, maxLives);

            // [SOS] Color AFTER picking up the lives
            SetColor(livesObjects, COLOR_BLUE);
            SetColor(livesObjectsOrange, COLOR_ORANGE);
        }

        public void SetLives(float lives, float livesOrange)
        {
            SetLives(lives);

            if (livesOrange < 0) return;

            UpdateLives(livesOrange);
        }

        // [TODO] this is repeated
        private void UpdateLives(float remainingLivesOrange)
        {
            int baseLives = Mathf.FloorToInt(remainingLivesOrange);
            int maxFillIdx = baseLives - 1;
            float extraBit = remainingLivesOrange - baseLives;

            for (int i = 0; i < livesObjectsOrange.Count; i++)
            {
                if (i <= maxFillIdx)
                    livesObjectsOrange[i].SetFillAmount(1f);
                else if (i == maxFillIdx + 1)
                    livesObjectsOrange[i].SetFillAmount(extraBit);
                else
                    livesObjectsOrange[i].SetFillAmount(0f);
            }

            if (remainingLivesOrange > 0 && remainingLivesOrange < livesObjectsOrange.Count)
            {
                livesObjectsOrange[(int)remainingLivesOrange].SetFillAmount(extraBit, true);
                if (remainingLivesOrange % 1 == 0 && (remainingLivesOrange - lastLivesOrange) > 0)
                    livesObjectsOrange[(int)remainingLivesOrange - 1].Scale();
            }

            lastLivesOrange = remainingLivesOrange;
        }
    }
}