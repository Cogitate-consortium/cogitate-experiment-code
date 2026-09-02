using UnityEngine;
using UnityEngine.UI;

namespace Helpers.UI.Core
{
    [RequireComponent(typeof(Image))]
    public class HeartObjectUI : MonoBehaviour
    {
        [Header("Editor")]
        public Image fullImage = null;
        public Image flashImage = null;
        public Color flashColor;
        public float fillAnimationDuration = 0.3f;
        public float flashAnimationDuration = 0.5f;

        private LTDescr animCR;
        private LTDescr flashCR;
        private LTDescr scaleCR;

        private Color startFlashColor;
        private float lastAnimateAmount;

        public void SetFillAmount(float fillAmount, bool animate = false)
        {
            float lastVal = fullImage.fillAmount;
            if (!animate)
            {
                fullImage.fillAmount = fillAmount;
                flashImage.fillAmount = fillAmount;
            }
            else
            {
                if (animCR != null)
                    LeanTween.cancel(animCR.uniqueId);

                if (fillAmount > lastVal)
                {
                    // Show growing animation
                    animCR = LeanTween.value(lastAnimateAmount, fillAmount, fillAnimationDuration).setOnUpdate((float lerp) =>
                    {
                        SetFillAmount(lerp, false);
                    });
                }

                if (fillAmount > lastVal)
                    Flash();

                lastAnimateAmount = fillAmount;
            }
        }

        public void Flash()
        {
            if (flashCR != null)
                LeanTween.cancel(flashCR.uniqueId);

            flashImage.gameObject.SetActive(true);
            flashCR = LeanTween.alpha(flashImage.rectTransform, 1f, flashAnimationDuration).setEaseInQuad().setLoopPingPong(1).setOnComplete(() =>
            {
                flashImage.color = new Color(startFlashColor.r, startFlashColor.g, startFlashColor.b, 0);
                flashImage.gameObject.SetActive(false);
            });
        }

        public void Scale()
        {
            if (scaleCR != null)
                LeanTween.cancel(scaleCR.uniqueId);

            scaleCR = LeanTween.scale(fullImage.rectTransform, fullImage.rectTransform.localScale * 1.2f, flashAnimationDuration).setEaseInQuad().setLoopPingPong(1).setOnComplete(() =>
            {
                fullImage.rectTransform.localScale = Vector3.one;
            });
        }

        private void Awake()
        {
            startFlashColor = flashImage.color;
            flashImage.color = new Color(startFlashColor.r, startFlashColor.g, startFlashColor.b, 0);
            flashImage.gameObject.SetActive(false);
        }

        public void SetColor(Color color)
        {
            startFlashColor = color;
            fullImage.color = color;
            flashImage.color = color;
        }

        private void OnDestroy()
        {
            if (animCR != null)
                LeanTween.cancel(animCR.uniqueId);

            if (flashCR != null)
                LeanTween.cancel(flashCR.uniqueId);

            if (scaleCR != null)
                LeanTween.cancel(scaleCR.uniqueId);
        }

    }
}