using Helpers.Engine;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Experiment.Analyzer.UI
{
    /// <summary>
    /// [CHECK]
    /// </summary>
    public class InformationMessageUI : MonoBehaviour
    {
        public static InformationMessageUI instance;

        [Header("Editor")]
        public GameObject notificationPanel;
        public Text notificationText;
        public GameObject errorPanel;
        public InputField errorText;
        public Button errorConfirmButton;

        private void Awake()
        {
            instance = this;
            errorConfirmButton.onClick.AddListener(() =>
            {
                errorPanel.SetActive(false);
            });

            notificationPanel.SetActive(false);
            errorPanel.SetActive(false);
        }

        public void ShowNotification(string message)
        {
            StartCoroutine(showNotificationIE(message));
        }

        private IEnumerator showNotificationIE(string message)
        {
            notificationPanel.SetActive(true);
            notificationText.text = message;

            float duration = 1f;
            float timeStarted = TimeWrapper.time_NotTS;
            while(true)
            {
                float lerp = (TimeWrapper.time_NotTS - timeStarted) / duration;
                if (lerp >= 1)
                    break;

                yield return null;
            }
            notificationPanel.SetActive(false);
        }

        public void ShowMessage(string message)
        {
            errorPanel.SetActive(true);
            errorText.text = message;
        }

    }
}