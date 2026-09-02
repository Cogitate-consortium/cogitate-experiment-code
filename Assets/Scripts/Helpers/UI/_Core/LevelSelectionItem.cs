using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Helpers.UI.Core
{
    public class LevelSelectionItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public event EventHandler<EventArgs<int>> onLevelSelected;
        /// <summary>
        /// 1-based
        /// </summary>
        public int LevelId;
        public bool isEnabled = true;

        [Header("Editor Values")]
        [SerializeField] private CanvasGroup CanvasGroup = null;
        [SerializeField] private TMPro.TextMeshProUGUI LevelText = null;
        [SerializeField] private Image ImageContainer = null;
        [SerializeField] private Image LockImage = null;
        [SerializeField] private Color InactiveColor = Color.gray;
        [SerializeField] private Color ActiveColor = Color.white;
        [SerializeField] private Color HoverColor = Color.white * 0.2f;

        [Header("Stars")]
        [SerializeField] private List<Image> StarImages = null;
        [SerializeField] private Sprite EmptyStarSprite = null;
        [SerializeField] private Sprite FullStarSprite = null;
        [SerializeField] private GameObject ArrowImage = null;

        [Header("Highlight")]
        [SerializeField] private GameObject highlightObject = null;

        private bool preventUnlock = false;

        #region Public Methods

        // [TODO] DEBUg
        void Awake()
        {
            if (CanvasGroup == null)
                CanvasGroup = gameObject.AddComponentIfNotExists<CanvasGroup>();
            //Initialize(isEnabled);
        }

        public void Toggle(bool on)
        {
            CanvasGroup.Toggle(on);
        }

        public void Initialize(int levelID, bool isEnabled, bool preventUnlock)
        {
            this.LevelId = levelID;
            this.preventUnlock = preventUnlock;
            TrySetup(isEnabled, 0);
        }

        public void HideArrow()
        {
            ArrowImage.SetActive(false);
        }

        public void SetLevelText(string text)
        {
            LevelText.text = text.ToString();
        }

        public void SetFocus()
        {
            highlightObject.SetActive(true);
            StarImages[0].transform.parent.gameObject.SetActive(false);
        }

        public void TrySetup(bool isEnabled, int stars)
        {
            if (isEnabled && preventUnlock)
            {
                this.LogWarning("Prevented untimely unlock of level ID " + LevelId);
                isEnabled = false;
            }

            this.isEnabled = isEnabled;

            ImageContainer.color = (isEnabled) ? ActiveColor : InactiveColor;
            LockImage.gameObject.SetActive(!isEnabled);
            LevelText.gameObject.SetActive(isEnabled);
            highlightObject.SetActive(false);

            for (int i = 0; i < StarImages.Count; i++)
            {
                bool isUncloked = (stars > i);
                StarImages[i].sprite = (isUncloked) ? FullStarSprite : EmptyStarSprite;
            }

            StarImages[0].transform.parent.gameObject.SetActive(isEnabled && stars >= 0);
        }

        private bool IsLocked()
        {
            if (LockImage.gameObject.activeSelf && !LevelText.gameObject.activeSelf) return true;
            if (!LockImage.gameObject.activeSelf && LevelText.gameObject.activeSelf) return false;

            this.LogWarning("Weird state with locked / unlocked");
            return true;
        }

        #endregion

        private void RaiseOnLevelSelected()
        {
            if (onLevelSelected != null)
                onLevelSelected(this, new EventArgs<int>(LevelId));
        }

        #region Mouse Events

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (IsLocked())
                    TrySetup(true, -1);
                else
                    this.LogWarning("Already unlocked");
            }
            else if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (isEnabled)
                    RaiseOnLevelSelected();
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isEnabled) return;
            ImageContainer.color = ActiveColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isEnabled) return;
            ImageContainer.color = HoverColor;
        }

        #endregion
    }
}