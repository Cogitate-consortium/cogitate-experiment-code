using TGP.Helpers;
using Helpers.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using ExperimentLibrary;

namespace Experiment.Subject.UI
{
    /// DEPRECATED?
    public class ReactionSpeedMenu : CanvasWindow
    {
        [SerializeField] private Slider reactionSpeedSlider = null;
        [SerializeField] private Button saveButton = null;
        [SerializeField] private Text speedText = null;

        protected override void Start()
        {
            base.Start();
            reactionSpeedSlider.value = ExperimentLibraryManager.Config.Subject.reactionTimeMS;
            reactionSpeedSlider.minValue = ExperimentLibraryManager.Config.Subject.reactionSpeedMinMax.min;
            reactionSpeedSlider.maxValue = ExperimentLibraryManager.Config.Subject.reactionSpeedMinMax.max;
            reactionSpeedSlider.onValueChanged.AddListener(UI_OnReactionSpeedChange);

            saveButton.onClick.AddListener(UI_OnClickSave);
        }

        private void UI_OnReactionSpeedChange(float arg0)
        {
            SetReactionSpeed((int)arg0);
        }

        private void UI_OnClickSave()
        {
            ExperimentLibraryManager.SaveConfigToFile();
        }

        private void SetReactionSpeed(int reaction_MS)
        {
            ExperimentLibraryManager.Config.Subject.reactionTimeMS = reaction_MS;
            // Debug
            speedText.text = "{0} Reaction (MS)"._Format(reaction_MS);
        }
    }
}