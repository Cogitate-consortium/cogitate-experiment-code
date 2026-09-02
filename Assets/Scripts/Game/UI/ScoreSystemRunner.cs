// NS_DEBATABLE
using Game.Managers.SessionManagers;
// NS_DEBATABLE
using Game.Entities.Interactables;
// NS_DEBATABLE | Can it do without levels?
using Game.Core;

using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;
using Game.Systems.Score;

namespace Game.UI
{
    /// <summary>
    /// [DEPRECATE] absorb into <see cref="ScoreSystemRunner_Star"/> ?
    /// </summary>
    public class ScoreSystemRunner : ScoreSystemUI
    {
        [Header("Editor")]
        [SerializeField] private GameObject stimuliParent = null;
        [SerializeField] private GameObject stimuliPrefab = null;

        private List<GameObject> facesList = new List<GameObject>();

        #region Logic

        public override void Initialize(RuntimeConfig runtimeConfig)
        {
            base.Initialize(runtimeConfig);
            stimuliParent.transform.Genocide();

            int targetScore = PlayerProgression.TargetScorePerLevel(runtimeConfig.level.levelID_1Based);
            for (int i = 0; i < targetScore; i++)
            {
                GameObject stimuliObj = Instantiate(stimuliPrefab, stimuliParent.transform, false);
                facesList.Add(stimuliObj);
            }
        }

        // TODO Is this need to have a reference?
        public override void IncreaseScore(Interactable interactable)
        {
            if (interactable == null)
            {
                throw new System.ArgumentNullException(nameof(interactable));
            }

            SetScore(score + 1);
        }

        protected override void SetScore(float newScore)
        {
            newScore = base.ConvertScoreToProgress_TS(newScore).Round(1f / (float)base.scoreTarget) * base.scoreTarget;
            base.SetScore(newScore);
        }

        #endregion

        #region UI

        #endregion

    }
}