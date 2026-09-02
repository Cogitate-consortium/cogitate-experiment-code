// NS_REMOVE
using ExperimentLibrary;

using UnityEngine;
using UnityEngine.UI;

namespace Helpers.Misc
{
    /// <summary>
    /// [DEPRECATED?] Is this used somewherE?
    /// </summary>
    public class VisualAngleScaler : MonoBehaviour
    {
        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void Update()
        {
            if (image == null) return;

            float screenPhysicalXSize = Screen.width / Screen.dpi;
            screenPhysicalXSize *= 2.54f;
            float screenPhysicalYSize = Screen.height / Screen.dpi;
            screenPhysicalYSize *= 2.54f;

            float size = VisualAngleMath.CalculateSize(ExperimentLibraryManager.Config.Experiment.stimulus.background.wantedVisualAngleStimulus, ExperimentLibraryManager.Config.Experiment.stimulus.background.wantedSubjectDistance_CM);
            if (size <= 0) return;

            image.rectTransform.sizeDelta = new Vector2(size, size);
        }
    }
}