using System;
using UnityEngine;
using UnityEngine.UI;

namespace Helpers.UI.Core
{
    public class CanvasWindow : MonoBehaviour
    {
        public event EventHandler<EventArgs> onWindowClose;

        [SerializeField] protected Canvas canvas = null;
        [SerializeField] private Button backButton = null;

        public bool autoClose = true;

        protected virtual void Start()
        {
            backButton.onClick.AddListener(BackButton_OnClick);
        }

        public virtual void Toggle(bool show, int sortingOrder)
        {
            gameObject.SetActive(show);
            canvas.sortingOrder = sortingOrder;
        }

        private void BackButton_OnClick()
        {
            onWindowClose?.Invoke(this, new EventArgs());
            if (autoClose)
                Toggle(false, 0);
        }
    }
}