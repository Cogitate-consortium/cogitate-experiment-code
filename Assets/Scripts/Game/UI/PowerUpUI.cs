using Game.Entities.Player;
using Helpers.Engine;
using System;
using System.Collections;
using TGP.Helpers;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// [CONFIG] (colors)
    /// Integrate into <see cref="InGameUI"/> then move to <see cref="Game.UI.Core"/>
    /// </summary>
    public class PowerUpUI : MonoBehaviour
    {
        public SpriteRenderer absorbPowerUp;
        public SpriteRenderer thunderPowerUp;

        private SpriteRenderer GetPowerUp(PowerUp powerUp)
        {
            switch (powerUp)
            {
                case PowerUp.Thunder:
                    return thunderPowerUp;
                case PowerUp.Absorption:
                    return absorbPowerUp;
            }

            return null;
        }

        public void Initialize(Config config)
        {
            transform.localScale = Vector3.one * config.scaleTotal;

            absorbPowerUp.transform.localScale = Vector3.one * config.scaleGlobe;
            thunderPowerUp.transform.localScale = Vector3.one * config.scaleGlobe;

            absorbPowerUp.color = new Color(1, 1, 1, 0);
            thunderPowerUp.color = new Color(1, 1, 1, 0);
        }

        public void UpdatePowerUp(PowerUp powerUp, ConsumeOrAcquire effect)
        {
            StartCoroutine(FadeImage(GetPowerUp(powerUp), effect == ConsumeOrAcquire.Consume ? 0 : 1));
        }

        private IEnumerator FadeImage(SpriteRenderer image, float alpha)
        {
            float duration = 0.5f;
            float timeStarted = TimeWrapper.time_NotTS;
            float startAlpha = image.color.a;
            while (true)
            {
                float lerp = (TimeWrapper.time_NotTS - timeStarted) / duration;
                Color targetColor = new Color(image.color.r, image.color.g, image.color.b, Mathf.Lerp(startAlpha, alpha, lerp));
                image.color = targetColor;

                if (lerp >= 1f)
                {
                    break;
                }
                yield return null;
            }
        }

        [Serializable]
        public class Config
        {
            public float scaleTotal = 1.25f;
            public float scaleGlobe = 0.27f;
        }
    }
}