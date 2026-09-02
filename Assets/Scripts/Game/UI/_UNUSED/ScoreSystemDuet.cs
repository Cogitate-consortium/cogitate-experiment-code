// NS_REMOVE
using Game.Core;

// NS_DEBATABLE
using Game.Entities.Interactables;
// NS_DEBATABLE
using Game.Managers.SessionManagers;

using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// [DEPRECATE] if we remove duet
    /// </summary>
    public class ScoreSystemDuet : ScoreSystemUI
    {
#if false
        [Header("Editor")]
        [SerializeField] private GameObject stimuliParent = null;
        [SerializeField] private GameObject stimuliPrefab = null;

        private List<ScoreItemDuetPrefab> scoreItems = new List<ScoreItemDuetPrefab>();

#region Logic

        public bool NeedsColorType(ObjectColorType objectColorType)
        {
            return !scoreItems[(int)score].HasColorType(objectColorType);
        }

        public override void Initialize(LevelConfig level, int scoreTarget)
        {
            base.Initialize(level, scoreTarget);
            stimuliParent.transform.Genocide();

            int targetScore = PlayerProgression.TargetScorePerLevel(level.levelID_1Based);
            for (int i = 0; i < targetScore; i++)
            {
                ScoreItemDuetPrefab scoreItem = Instantiate(stimuliPrefab, stimuliParent.transform, false).GetComponent<ScoreItemDuetPrefab>();
                scoreItem.SetEmpty();
                scoreItems.Add(scoreItem);
            }
        }

        public override void IncreaseScore(Interactable interactable)
        {
            if (interactable != null)
            {
                scoreItems[(int)score].Unlock(interactable.colorType);
                if (scoreItems[(int)score].IsComplete())
                    SetScore(score + 1);
            }
            else
            {
                SetScore(score + 1);
            }
        }

        protected override void SetScore(float newScore)
        {
            newScore = base.ConvertScoreToProgress_TS(newScore).Round(1f / (float)base.scoreTarget) * base.scoreTarget;
            base.SetScore(newScore);
        }

#endregion

#region UI

#endregion
#endif
    }
}