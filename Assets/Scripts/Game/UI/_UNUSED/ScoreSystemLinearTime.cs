using Helpers.Engine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// [DEPRECATE] or subclass better
    /// </summary>
    public class ScoreSystemLinearTime : ScoreSystemUI
    {
        public void Update()
        {
            if (!isPaused)
            {
                SetScore(score += TimeWrapper.deltaTime_SinceLastUpdate_NotTS * config.rewardPerSecond);
            }
        }

    }
}