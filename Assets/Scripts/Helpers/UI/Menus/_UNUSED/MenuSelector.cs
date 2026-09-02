// NS_REMOVE
using Experiment.Library.Core;

// NS_DEBATABLE
using Game.Core;
// NS_DEBATABLE
using Helpers.Assets;

using System;
using TGP.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Helpers.UI.Menus
{
    /// <summary>
    /// [DEPRECATE?]
    /// </summary>
    public class MenuSelector : MonoBehaviour
    {
        [SerializeField] private Image screenshotImage = null;
        [SerializeField] private Text taglineText = null;
        [SerializeField] private Button button = null;

        public EventHandler<EventArgs<int>> onPrototypeSelected;

        private LevelConfig level;

        public void Initialize(LevelConfig level)
        {
            this.Log("Init!");

            this.level = level;

            string filename = ExperimentPaths.screenshotDirectory + level.ToString() + ".png";
            StreamingAssetsManager.GetSprite(filename, (sprite) =>
            {
                screenshotImage.sprite = sprite;
            });

            // Setup the button
            UICheck uiCheck = button.gameObject.AddComponent<UICheck>();
            uiCheck.onEnter += UI_OnPointerEnter;
            uiCheck.onExit += UI_OnPointerExit;
            uiCheck.onClick += UI_OnPointerClick;

            // Setup the texts
            //nameText.text = level.gameType.ToString();
            taglineText.text = level.GetGameType().ToString();

            // Init
            ToggleTagline(false);
        }

        private void UI_OnPointerClick(object sender, EventArgs<PointerEventData> e)
        {
            FireOnClickEvent();
        }

        private void UI_OnPointerEnter(object sender, EventArgs<PointerEventData> e)
        {
            ToggleTagline(true);
        }

        private void UI_OnPointerExit(object sender, EventArgs<PointerEventData> e)
        {
            ToggleTagline(false);
        }

        private void ToggleTagline(bool on)
        {
            taglineText.enabled = on;
        }

        private void FireOnClickEvent()
        {
            if (onPrototypeSelected != null)
                onPrototypeSelected(this, new EventArgs<int>(level.levelID_1Based));
        }
    }
}