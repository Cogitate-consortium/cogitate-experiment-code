using Experiment.Analyzer.Core;
using Helpers.Async;
using Helpers.Engine;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Experiment.Analyzer
{
    /// <summary>
    /// [DEPRECATED?] Or at least make it share structure with <see cref="FullLogAnalyzerMultiple_UI"/>.
    /// [SEGMENT] Send the UI-related stuff under <see cref="Experiment.Analyzer.UI"/>
    /// Their difference is on the folder structure they expect. Maybe detect that or make a checkbox? Or just differ the UI and use the Multiople one.
    /// </summary>
    public class FullLogAnalyzer_UI : MonoBehaviour
    {
        [Header("UI")]
        public Slider progressSlider;
        public InputField folderPathInput;
        public Text outputFolderPath;
        public Button checkButton;
        public Button analyzeButton;
        public Text infoText;
        public Text probeDetailsText;
        public Text allFilesText;

        [Header("Editor Values")]
        public bool EDITOR_ONLY_AUTO_RUN = true;
        public bool EDITOR_ONLY_RUN_ONE_EACH = true;
        public string folderPath = "";
        public bool useSeparateThread = false;
        public bool includeTutorialLevels = true;
        public bool useParallelFor = false;

        public FullLogAnalyzer analyzer;

        // Turn automations off for the build
#if !UNITY_EDITOR
        private void Awake()
        {
            folderPath = "";
        }
#endif

        // Start is called before the first frame update
        void Start()
        {
            // Cache all enums for faster parsing
            EnumHelper.Initialize();

            allFilesText.text = "";
            outputFolderPath.text = FullLogAnalyzer.outputFolder;

            checkButton.interactable = true;
            analyzeButton.interactable = false;

            checkButton.onClick.AddListener(CheckButton_OnClick);
            analyzeButton.onClick.AddListener(AnalyzeButton_OnClick);

#if UNITY_EDITOR
            if (EDITOR_ONLY_AUTO_RUN)
            {
                CheckButton_OnClick();
                AnalyzeButton_OnClick();
            }
#endif
        }

#region UI

        // Keep Thread-Safe
        private void UpdateAnalyzerProcessBegin(string text)
        {
            infoText.text = "Analyzing file:\n" + text;
        }

        // Keep Thread-Safe
        private void UpdateAnalyzerProgress(float val)
        {
            progressSlider.value = val;
        }

        // Keep Thread-Safe
        private void UpdateReadDataComplete()
        {
            analyzer.ExportData();

            checkButton.interactable = true;
            analyzeButton.interactable = false;
            infoText.text = "Analyzing completed successfully. You can find the analyses in\n" + analyzer.GetOutputFile("WORLD_LEVEL", "<TYPE>");
        }

        private void CheckButton_OnClick()
        {
            infoText.text = "";
            allFilesText.text = "";
            probeDetailsText.text = "";
            string folderUI = folderPathInput.text;

            if (folderUI != "")
            {
                #if UNITY_STANDALONE_OSX
                if (folderUI.Contains("\\ ")) {
                    infoText.text = "Path cannot contain spaces. Please move folder somewhere else (desktop/docs etc);";
                    return;
                }
                #endif

                if (!Directory.Exists(folderUI))
                {
                    infoText.text = "Path doesn't exists";
                    return;
                }

                folderPath = folderUI;
            }

            analyzer = new FullLogAnalyzer(folderPath, includeTutorialLevels);
            analyzer.onReadDataComplete += Analyzer_onReadDataComplete;
            analyzer.onProgress += Analyzer_onProgress;
            analyzer.onProcessFileBegin += Analyzer_onProcessFileBegin;

            if (analyzer.probeDetailsFile == null)
            {
                infoText.text = "Couldn't find {ID}_ProbeDetails.csv";
                return;
            }
            if (analyzer.fullDataFiles.Count == 0)
            {
                infoText.text = "Folder doesn't contain any valid csv files";
                return;
            }

            probeDetailsText.text = analyzer.probeDetailsFile;

            for (int i = 0; i < analyzer.fullDataFiles.Count; i++)
            {
                allFilesText.text += analyzer.fullDataFiles[i] + "\n";
            }

            // outputFolderPath.text = analyzer.GetOutputFile();

            //checkButton.interactable = false;
            analyzeButton.interactable = true;
        }

        private void AnalyzeButton_OnClick()
        {
            checkButton.interactable = false;
            analyzeButton.interactable = false;
            Run();
        }

#endregion

#region Analyzer Events

        private void Analyzer_onProcessFileBegin(object sender, TGP.Helpers.EventArgs<string> e)
        {
            if (useSeparateThread)
                AsyncThread.RunOnMainThread_ASAP_TS(() => { UpdateAnalyzerProcessBegin(e.value); });
            else
                UpdateAnalyzerProcessBegin(e.value);
        }

        private void Analyzer_onProgress(object sender, TGP.Helpers.EventArgs<float> e)
        {
            if (useSeparateThread)
                AsyncThread.RunOnMainThread_ASAP_TS(() => { UpdateAnalyzerProgress(e.value); });
            else
                UpdateAnalyzerProgress(e.value);
        }

        private void Analyzer_onReadDataComplete(object sender, EventArgs e)
        {
            if (useSeparateThread)
                AsyncThread.RunOnMainThread_ASAP_TS(() => { UpdateReadDataComplete(); });
            else
                UpdateReadDataComplete();
        }

#endregion

        [ContextMenu("Run")]
        public void Run()
        {
            StopAllCoroutines();
            StartCoroutine(RunIE());
        }

        private IEnumerator RunIE()
        {
            float timeStarted = TimeWrapper.realtimeSinceStartup_NotTS;
            bool finished = false;

            FullLogAnalyzer.RuntimeConfig runtimeConfig_Analyzer = new FullLogAnalyzer.RuntimeConfig(useParallelFor, EDITOR_ONLY_RUN_ONE_EACH,
                WritingStrategy.PerLevel, WritingStrategy.PerLevel, WritingStrategy.PerLevel);

            if (useSeparateThread)
            {
                AsyncThread.RequestRunOnNewThread(() =>
                {
                    analyzer.ParseAllFiles(runtimeConfig_Analyzer);
                    finished = true;
                });
            }
            else
            {
                analyzer.ParseAllFiles(runtimeConfig_Analyzer);
                finished = true;
            }

            while (!finished)
            {
                yield return null;
            }

            Debug.Log(string.Format("Read all files in {0}s", (TimeWrapper.realtimeSinceStartup_NotTS - timeStarted).ToString("0.0")));
        }

    }
}