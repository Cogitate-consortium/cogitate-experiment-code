using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// [CHECK]
/// </summary>
namespace Experiment.Analyzer.UI
{
    [System.Serializable]
    public enum DataError { None, Information, Warning, Error }

    public class CheckedItemMessage : MonoBehaviour
    {
        public event EventHandler<EventArgs> onValueChanged;

        [Header("Editor")]
        public Sprite informationSprite;
        public Sprite warningSprite;
        public Sprite errorSprite;
        public Button openFolderButton;
        public Button statusButton;
        public Image statusImage;
        public Text dataText;
        public Text statusText;
        public Toggle toggle;

        public bool isOn
        {
            get { return toggle.isOn; }
            set { toggle.isOn = value; }
        }
        public string message { get; private set; }
        public string value { get; private set; }
        public string data { get { return dataText.text; } }

        private void Start()
        {
            statusButton.onClick.AddListener(StatusButton_OnClick);

            toggle.onValueChanged.AddListener((bool selected) => { onValueChanged?.Invoke(this, EventArgs.Empty); });
            if (openFolderButton != null)
            {
                openFolderButton.onClick.AddListener(OpenFolderButton_OnClick);
            }
        }

        private void OpenFolderButton_OnClick()
        {
            ShowInExplorer(value); 
            //CopyToClipboard(data);
            //InformationMessageUI.instance.ShowNotification("Invalid Data folder copied to clipboard!");
        }

        private void StatusButton_OnClick()
        {
            InformationMessageUI.instance.ShowMessage(message);
        }

        public void Set(DataError status, string text, string value, string message = "")
        {
            statusImage.enabled = true;
            switch (status)
            {
                case DataError.None:
                    statusImage.enabled = false;
                    break;
                case DataError.Information:
                    statusImage.sprite = informationSprite;
                    break;
                case DataError.Warning:
                    statusImage.sprite = warningSprite;
                    break;
                case DataError.Error:
                    statusImage.sprite = errorSprite;
                    break;
            }

            dataText.text = text;
            this.message = status + ": " + message;
            this.value = value;
        }

        public void SetStatusText(string text)
        {
            statusText.text = text;
        }

        // https://answers.unity.com/questions/1144378/copy-to-clipboard-with-a-button-unity-53-solution.html
        // [Hack] Copy to cliboard workaround
        public static void CopyToClipboard(string s)
        {
            TextEditor te = new TextEditor();
            te.text = s;
            te.SelectAll();
            te.Copy();
        }

        public static void ShowInExplorer(string directory)
        {
            directory = directory.Replace(@"/", @"\");   // explorer doesn't like front slashes
#if UNITY_STANDALONE_WIN
            System.Diagnostics.Process.Start("explorer.exe", "/select," + directory);
#elif UNITY_STANDALONE_OSX
            System.Diagnostics.Process.Start("open", directory);
#endif
        }
    }
}