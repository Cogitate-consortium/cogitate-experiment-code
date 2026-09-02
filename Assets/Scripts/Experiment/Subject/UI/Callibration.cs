using Experiment.Background;
using TGP.Helpers;
using Helpers.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using ExperimentLibrary;

namespace Experiment.Subject.UI
{
    public class Callibration : CanvasWindow
    {
        private static BackgroundManager.Config config { get { return ExperimentLibraryManager.Config.Experiment.stimulus.background; } }

        [SerializeField] private Camera cam = null;
        [SerializeField] private Slider scaleBaseSlider = null;
        [SerializeField] private Text scaleBaseText = null;
        [SerializeField] private Slider scaleLabSlider = null;
        [SerializeField] private Text scaleLabText = null;
        [SerializeField] private Button saveButton = null;
        [SerializeField] private SpriteRenderer stimulusSR = null;
        [SerializeField] private SpriteRenderer viewportSR = null;
        [SerializeField] private SpriteRenderer physicalSR = null;

        private Vector2 defaultSize = new Vector2(16, 9) * 0.23f;

        float screenDPI_Original;
        float zoomIndex_Original;

        private void Awake()
        {
            physicalSR.size = defaultSize;

            scaleBaseSlider.wholeNumbers = false;
            scaleBaseSlider.minValue = 0.0f;
            scaleBaseSlider.maxValue = 1.0f;

            scaleLabSlider.wholeNumbers = false;
            scaleLabSlider.minValue = 0.0f;
            scaleLabSlider.maxValue = 1.0f;

            scaleBaseSlider.onValueChanged.AddListener(UI_OnScaleBaseChange);
            scaleLabSlider.onValueChanged.AddListener(UI_OnScaleLabChange);
            saveButton.onClick.AddListener(UI_OnClickSave);
        }

        public override void Toggle(bool show, int sortingOrder)
        {
            base.Toggle(show, sortingOrder);

            stimulusSR.sortingOrder = sortingOrder;
            viewportSR.sortingOrder = sortingOrder;
            physicalSR.sortingOrder = sortingOrder;

            this.LogWarning("Canvas & Sprite Sorting Orders updated -> " + sortingOrder);

            // If we are in full screen mode
            if (!config.useActiveAreaInsteadOfFullScreen ||
                // Or our active area is essentially full screen
                config._activeAreaVerticalOffsetFromScreenEdges == 0)
                // We do not want to be able to "zoom in"
                scaleLabSlider.maxValue = 0.5f;

            // Force to refresh
            if (show)
            {
                ExperimentLibraryManager.LoadFromFile();

                scaleBaseSlider.value = config.zoomIndexBase;
                scaleLabSlider.value = config.zoomIndexLab;

                UI_OnScaleBaseChange(config.zoomIndexBase);
                UI_OnScaleLabChange(config.zoomIndexLab);
            }
        }

        private void UI_OnScaleBaseChange(float arg0)
        {
            config.zoomIndexBase = arg0;
            // Debug.Log("Base -> " + arg0);
            scaleBaseText.text = "base: {0}x"._Format(config.zoomFactorBase.PercentileToPercent("#.00"));

            UpdateZoom();
        }

        private void UI_OnScaleLabChange(float arg0)
        {
            config.zoomIndexLab = arg0;
            // Debug.Log("Lab -> " + arg0);
            scaleLabText.text = "lab: {0}x"._Format(config.zoomFactorLab.PercentileToPercent("#.00"));

            UpdateZoom();
        }

        private void UI_OnClickSave()
        {
            ExperimentLibraryManager.SaveConfigToFile();
        }

        private void UpdateZoom()
        {
            float zoomFactor = config.zoomFactor;
            viewportSR.size = defaultSize * zoomFactor;
            UpdateStimulusSize();
        }

        private void UpdateStimulusSize()
        {
            float dpi = BackgroundConfig.screenDPI;

            float wantedVisualAngle = config.wantedVisualAngleStimulus;
            float wantedSubjectDist = config.wantedSubjectDistance_CM;
            float wantedScale = BackgroundConfig.GetScaleFromVisualAngle(dpi, wantedVisualAngle, wantedSubjectDist, cam.orthographicSize, 1) / stimulusSR.sprite.bounds.size.x;
            stimulusSR.transform.localScale = Vector3.one * wantedScale * config.zoomFactor; // * viewportScale;
        }
        /*
        private void UI_OnViewportChange(float viewportPercentile)
        {
            config.viewportSizePercentile = viewportPercentile;

            // Debug
            scaleText.text = "view {0}"._Format(viewportPercentile.PercentileToPercent());

            // UpdateStimulusSize();
            UpdateViewportSize();
        }
        */

        /*
        private void UpdateViewportSize()
        {
            float viewportPercentile = config.viewportSizePercentile;

            viewportSR.size = defaultSize * viewportPercentile;
        }
        */
    }
}