// NS_REMOVE
using ExperimentLibrary;
using Helpers.Engine;
using System;
using System.Collections;
using TGP.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Systems.Narration
{
    public class SimpleNarrator : MonoBehaviour
    {
#if false
        public Text narrationText;

        protected NarrationConfig config { get { return ApplicationLibrary.Config.Narration; } }
        private CanvasGroup cg;

        private void Awake()
        {
            //narrationText.color = new Color(narrationText.color.r, narrationText.color.g, narrationText.color.b, 1.0f);
            this.gameObject.SetActive(false);
            cg = this.gameObject.AddComponentIfNotExists<CanvasGroup>();
            cg.alpha = 0f;
        }

        public void ShowNarration(string text, Action finishCallback)
        {
            if (text.IsNullOrEmpty() || text[0] == '_' || !ApplicationLibrary.Config.Narration.showNarration)
            {
                finishCallback();
                return;
            }

            this.gameObject.SetActive(true);
            narrationText.text = text;

            StopAllCoroutines();
            StartCoroutine(FadeInOut(text, finishCallback));
        }

        private IEnumerator FadeInOut(string text, Action finishCallback)
        {
            float narrationTime = text.GetAverageReadingTime() * config.inGameTextDurationMultiplier;
            float fadeDuration = config.fadeDuration;

            // Fade in
            float fadeInStart = TimeWrapper.time;
            bool userSkipped = false;
            while (true)
            {
                float lerp = (TimeWrapper.time - fadeInStart) / fadeDuration;
                //narrationText.color = UpdateAlpha(narrationText.color, lerp);
                cg.alpha = lerp;
                if (lerp >= 1)
                    break;

                if (Input.GetKeyDown(ApplicationLibrary.Config.InputKeyCode.Skip_Narration))
                {
                    userSkipped = true;
                    break;
                }
                yield return null;
            }

            // Wait to read text or skip
            float waitTime = narrationTime;
            while (true)
            {
                waitTime -= TimeWrapper.deltaTime;
                if (waitTime <= 0)
                    break;

                if (Input.GetKeyDown(ApplicationLibrary.Config.InputKeyCode.Skip_Narration))
                {
                    userSkipped = true;
                    break;
                }

                yield return null;
            }


            // Convert from ms
            if (userSkipped)
                fadeDuration = (float)ApplicationLibrary.Config.Narration.userSkipNarrationTime / 1000f;

            // Fade out
            float fadeOutStart = TimeWrapper.time;
            while (true)
            {
                float lerp = (TimeWrapper.time - fadeOutStart) / fadeDuration;
                //narrationText.color = UpdateAlpha(narrationText.color, 1f - lerp);
                cg.alpha = 1f - lerp;
                if (lerp >= 1)
                    break;
                yield return null;
            }

            if (finishCallback != null)
                finishCallback();
        }
#endif
    }
}