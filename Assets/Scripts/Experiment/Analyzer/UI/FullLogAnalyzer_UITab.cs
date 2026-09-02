using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// [CHECK]
/// </summary>
public class FullLogAnalyzer_UITab : MonoBehaviour
{
    public Button multipleFilesButton;
    public Button singleFileButton;

    public GameObject multipleFilesPanel;
    public GameObject singleFilePanel;

    // Start is called before the first frame update
    void Start()
    {
        multipleFilesButton.onClick.AddListener(() => { ShowMultipleFilesTab(true); });
        singleFileButton.onClick.AddListener(() => { ShowMultipleFilesTab(false); });
        ShowMultipleFilesTab(true);
    }

    public void ShowMultipleFilesTab(bool isMultiple)
    {
        multipleFilesPanel.gameObject.SetActive(isMultiple);
        singleFilePanel.gameObject.SetActive(!isMultiple);

        ColorBlock multipleColorBlock = multipleFilesButton.colors;
        multipleColorBlock.normalColor = (isMultiple) ? Color.white : Color.gray;
        multipleFilesButton.colors = multipleColorBlock;

        ColorBlock singleColorBlock = singleFileButton.colors;
        singleColorBlock.normalColor = (!isMultiple) ? Color.white : Color.gray;
        singleFileButton.colors = singleColorBlock;
    }
}
