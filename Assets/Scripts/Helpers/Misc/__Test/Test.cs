// NS_REMOVE | This is using background static helpers which will be moved
using Experiment.Background;

using TGP.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace Helpers.Misc.Test
{
    /// <summary>
    /// [SOS] Important when testing! Set to "Free Aspect" or make sure the "zoom" factor of the editor is set to 1.
    /// </summary>
    [ExecuteInEditMode]
    public class Test : MonoBehaviour
    {
        public SpriteRenderer sR;
        public Camera c;
        // public Canvas canv;
        public Image image;
        public Text text;
        public Slider dpiSlider;
        public Text dpiText;

        [Header("For debugging the Full System")]
        public float wantedPhysicalSize = 10;
        public float physicalSizeToScaleCorrector = 1.77f;
        public float dpi = 145;

        // public bool factorInWidth;
        // public bool factorInHeight;
        private Mode mode = Mode.PhysicalToScale;

        [Header("For debugging Visual Angle")]
        public float wantedVisualAngle = 3;
        public float wantedPhysicalCM = 3;
        public float distanceFromScreenCM = 57;

        public bool report = false;

        private void OnEnable()
        {
            dpiSlider.onValueChanged.RemoveAllListeners();
            dpiSlider.onValueChanged.AddListener(UI_dpiValueChange);
        }

        private void UI_dpiValueChange(float arg0)
        {
            dpi = arg0;
        }

        // Update is called once per frame
        void Update()
        {
            // Debug.Log(sR.GetPixelSize());

            if (report)
            {
                report = false;

                Vector2 pixelSize = sR.GetPixelSize();

                Debug.Log("Pixel Size :: " + pixelSize);

                float testPhysicalCM = VisualAngleMath.CalculateSize(wantedVisualAngle, distanceFromScreenCM);
                float testVisualAngle = VisualAngleMath.CalculateVisualAngle(wantedPhysicalCM, distanceFromScreenCM);
                float testDistanceCM = VisualAngleMath.CalculateDistance(wantedVisualAngle, wantedPhysicalCM);

                Debug.Log("Physical Size (CM) :: " + testPhysicalCM);
                Debug.Log("Visual Angle :: " + testVisualAngle);
                Debug.Log("Distance (CM) :: " + testDistanceCM);
                Debug.Log("Screen DPIs :: " + Screen.dpi);
                Debug.Log("Screen Resolution :: " + Screen.width + "x" + Screen.height);

                // float testScreenPercentile = BackgroundConfig.GetPhysicalSizeAsPercentileOfScreen(wantedPhysicalSize, false);

                // Debug.Log("Wanted Screen Percent :: " + testScreenPercentile.PercentileToPercent("#.0000"));

            }

            if (sR == null) return;
            if (c == null) return;
            // if (canv == null) return;
            if (image == null) return;

            dpiText.text = "{0} dpi"._Format(dpi.ToString("#"));
            // if (!Application.isEditor) wantedPhysicalSize = Time.time;

            switch (mode)
            {
                case Mode.PhysicalToScale:
                    float wantedScale = BackgroundConfig.GetScaleFromPhysicalSize(dpi, wantedPhysicalSize, c.orthographicSize, physicalSizeToScaleCorrector, Application.isEditor) / sR.sprite.bounds.size.x;
                    sR.transform.localScale = Vector3.one * wantedScale;

                    // Debug
                    image.rectTransform.sizeDelta = new Vector2(wantedPhysicalSize, wantedPhysicalSize);
                    text.text = "{0}cm ({1})dpi"._Format(wantedPhysicalSize, dpi);
                    break;
                case Mode.ScaleToPhysical:
                    break;
            }
        }

        public enum Mode { PhysicalToScale, ScaleToPhysical }
    }
}