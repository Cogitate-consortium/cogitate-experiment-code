using Helpers.Engine;
using System.Collections;
using TGP.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Systems.Misc
{
    public class LevelIntroDimmer : MonoBehaviour
    {
        public Image DimmerImage;
        public GameObject parentObject;
        public float fadeOutDuration = 1f;

        public void Initialize()
        {
            DimmerImage.color = Color.black;
            StartCoroutine(FadeOutIE());
        }

        // Start is called before the first frame update
        IEnumerator FadeOutIE()
        {
            float timeStarted = TimeWrapper.time_NotTS;
            while (true)
            {
                float lerp = (TimeWrapper.time_NotTS - timeStarted) / fadeOutDuration;
                DimmerImage.color = Color.black.UpdateAlpha(1f - lerp);

                if (lerp >= 1.0f) break;
                yield return null;
            }

            Destroy();
        }

        public void Destroy()
        {
            Destroy(parentObject.gameObject);
        }
    }
}