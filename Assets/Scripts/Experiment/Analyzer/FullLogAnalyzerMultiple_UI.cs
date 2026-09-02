/// NS_REMOVE | Use <see cref="Peripherals.Logging.LogWrapper"/> instead
using System.IO;

// NS_DEBATABLE | VICE VERSA??
using Experiment.Analyzer.UI;

using Experiment.Analyzer.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TGP.Helpers;
using UnityEngine;
using UnityEngine.UI;
using Helpers.Engine;
using Helpers.Async;

/// <summary>
/// [RENAME] Should not contain _UI
/// [SEGMENT] Send the UI-related stuff under <see cref="Experiment.Analyzer.UI"/>
/// </summary>
namespace Experiment.Analyzer
{
    public class FullLogAnalyzerMultiple_UI : MonoBehaviour
    {
        private const string REGEX_SUBJECT_ID = "([A-Z]{2}[0-9]{3}[A-Z]{1})";
        private const string REGEX_ANY = @"([A-Z])\w+";
        private const string OUTPUT_FILENAME = "Analysis.csv";

        private string GetOutputFile()
        {
            return EDITOR_ONLY_DEFAULT_PATH + "//" + OUTPUT_FILENAME;
        }

        [Header("UI")]
        public Slider progressSlider;
        public InputField folderPathInput;
        public Text outputFolderPath;
        public Button checkButton;
        public Button analyzeButton;
        public Text infoText;
        public Text allValidSubjectsText;
        public CheckedListBox allValidSubjectsCheckedList;
        public Text allInvalidSubjectsText;
        public CheckedListBox allInvalidSubjectsCheckedList;
        public Dropdown MEEG_DD;
        public Dropdown ECOG_DD;
        public Dropdown FMRI_DD;
        public Toggle doOneEach_Toggle;
        public Toggle doTutorialLevels_Toggle;
        public Toggle doGameLevels_Toggle;
        public Toggle doReplayLevels_Toggle;
        public Toggle runSeparateThread_Toggle;
        public Toggle runParallelFor_Toggle;

        [Header("Log Color")]
        public Color messageColor = Color.yellow;
        public Color successColor = Color.green;
        public Color errorColor = Color.red;

        [Header("Editor Values")]
        public string EDITOR_ONLY_DEFAULT_PATH = "";
        public bool EDITOR_ONLY_AUTO_RUN = true;
        [SerializeField] private bool doOneEach = false;
        [SerializeField] private bool doTutorialLevels = true;
        [SerializeField] private bool doGameLevels = true;
        [SerializeField] private bool doReplayLevels = true;
        [SerializeField] private bool runSeparateThread = true;
        [SerializeField] private bool runParallelFor = true;

        public Dictionary<string, FullLogAnalyzer> folderToSubjectAnalyzer = new Dictionary<string, FullLogAnalyzer>();

        private int completeAnalyzedExperiments = 0;

        //  ie. S2442A
        private const int OLDSCHOOL_LENGTH = 6;

        // Quick access selected check boxes
        private List<string> selectedSubjects
        {
            get
            {
                return allValidSubjectsCheckedList == null ?
                    new List<string>() :
                    allValidSubjectsCheckedList.GetSelectedValues();
            }
        }

        // Start is called before the first frame update
        private void Start()
        {
            // Cache all enums for faster parsing
            EnumHelper.Initialize();

            List<string> options = Utility_Helper.EnumGetValuesAsStrings<WritingStrategy>();

            MEEG_DD.ClearOptions();
            MEEG_DD.AddOptions(options);
            MEEG_DD.value = (int)WritingStrategy.PerWorld;

            ECOG_DD.ClearOptions();
            ECOG_DD.AddOptions(options);
            ECOG_DD.value = (int)WritingStrategy.PerWorld;

            FMRI_DD.ClearOptions();
            FMRI_DD.AddOptions(options);
            FMRI_DD.value = (int)WritingStrategy.PerTwoLevels;

#if UNITY_EDITOR
            doOneEach_Toggle.isOn = doOneEach;
            doTutorialLevels_Toggle.isOn = doTutorialLevels;
            doGameLevels_Toggle.isOn = doGameLevels;
            doReplayLevels_Toggle.isOn = doReplayLevels;
            runSeparateThread_Toggle.isOn = runSeparateThread;
            runParallelFor_Toggle.isOn = runParallelFor;
#else
            doOneEach_Toggle.isOn = false;
            doTutorialLevels_Toggle.isOn = true;
            doGameLevels_Toggle.isOn = true;
            doReplayLevels_Toggle.isOn = true;
            runSeparateThread_Toggle.isOn = true;
            runParallelFor_Toggle.isOn = true;
#endif

            allValidSubjectsCheckedList.onSelectedNumberChanged += AllValidSubjectsCheckedList_onSelectedNumberChanged;
            allValidSubjectsCheckedList.Clear();
            allInvalidSubjectsCheckedList.Clear();
            outputFolderPath.text = OUTPUT_FILENAME;

            checkButton.interactable = true;
            analyzeButton.interactable = false;

            checkButton.onClick.AddListener(CheckStatusForPath);
            analyzeButton.onClick.AddListener(AnalyzeButton_OnClick);

#if UNITY_EDITOR
            folderPathInput.text = EDITOR_ONLY_DEFAULT_PATH;
            if (EDITOR_ONLY_AUTO_RUN)
            {
                CheckStatusForPath();
                AnalyzeButton_OnClick();
            }
#endif
        }

#region Core Functions

        // Analyze data for all selected IDs
        public void Run()
        {
            StopAllCoroutines();
            StartCoroutine(RunIE());
        }

        private IEnumerator RunIE()
        {
            // Load the correct values

            // Allow UI to update
            yield return new WaitForSeconds(0.1f);

            // [SOS] Do before running anything else!
            GetUIValues();

            float timeStarted = TimeWrapper.realtimeSinceStartup_NotTS;
            FullLogAnalyzer.RuntimeConfig runtimeConfig_Analyzer = new FullLogAnalyzer.RuntimeConfig(runParallelFor, doOneEach,
                GetWritingStrategy(ModuleType.MEEG), GetWritingStrategy(ModuleType.ECOG), GetWritingStrategy(ModuleType.FMRI_Scanner));

            ShowDebugText(string.Format("\n\n"), MessageType.Success);
            ShowDebugText(string.Format("######################################"), MessageType.Success);
            ShowDebugText(string.Format("Starting new run of multiple analysis!"), MessageType.Success);
            ShowDebugText(string.Format("MEEG :: {0} | ECOG :: {1} | FMRI :: {2}", 
                runtimeConfig_Analyzer.writingStrategy_MEEG, runtimeConfig_Analyzer.writingStrategy_ECOG, runtimeConfig_Analyzer.writingStrategy_FMRI), MessageType.Informational);
            ShowDebugText(string.Format("DoOneEach :: {0} | DoTutorialLevels :: {1} | DoGameLevels :: {2} | DoReplayLevels :: {3} | RunSeparateThread :: {4} | RunParallelFor :: {5}",
                runtimeConfig_Analyzer.onlyDoOneOfEach, doTutorialLevels, doGameLevels, doReplayLevels, runSeparateThread, runtimeConfig_Analyzer.useParallelFor), MessageType.Informational);
            ShowDebugText(string.Format("######################################"), MessageType.Success);

            // Loop through all selected checkbox per SubjectID
            List <FullLogAnalyzer> selectedAnalyzers = GetSelectedSubjectAnalyzer();
            for (int i = 0; i < selectedAnalyzers.Count; i++)
            {
                FullLogAnalyzer subjectAnalyzer = selectedAnalyzers[i];

                bool finished = false;

                float analysisTimeStarted = TimeWrapper.realtimeSinceStartup_NotTS;
                if (runSeparateThread)
                {
                    AsyncThread.RequestRunOnNewThread(() =>
                    {
                        try
                        {
                            subjectAnalyzer.ParseAllFiles(runtimeConfig_Analyzer);
                            finished = true;
                        }
                        catch (Exception ex)
                        {
                            ShowDebugText(string.Format("There was an error during analysis. error:{0}\nstack:{1}", ex.Message, ex.StackTrace), MessageType.Error);
                        }
                    });
                }
                else
                {
                    try
                    {
                        subjectAnalyzer.ParseAllFiles(runtimeConfig_Analyzer);
                        finished = true;
                    }
                    catch (Exception ex)
                    {
                        ShowDebugText(string.Format("There was an error during analysis. error:{0}\nstack:{1}", ex.Message, ex.StackTrace), MessageType.Error);
                    }
                }


                while (!finished)
                {
                    yield return null;
                }

                // Allow UI to update
                yield return new WaitForSeconds(0.1f);

                float elapsedTime = TimeWrapper.realtimeSinceStartup_NotTS - analysisTimeStarted;
                ShowDebugText(string.Format("Finish analysis for experiment {0} in {1}s\n", subjectAnalyzer.subjectID, elapsedTime.ToString("0.0")), MessageType.Success);
            }
            Debug.Log(string.Format("Read all files in {0}s", (TimeWrapper.realtimeSinceStartup_NotTS - timeStarted).ToString("0.0")));

            // Wait for Threads to finish
            yield return new WaitForSeconds(0.1f);

            // Wait until all Analyzer are complete
            while (selectedAnalyzers.FindAll((FullLogAnalyzer analyzer) => { return !analyzer.isCompleted; }).Count > 0)
            {
                ShowDebugText("Waiting for all analyses to complete...", MessageType.Informational);
                yield return new WaitForSeconds(1f);
            }

            // Read and combine all per subject analyses to a single file
            yield return StartCoroutine(ExportCombineAllData());

            ShowDebugText(string.Format("Finished analysis in:{0}s", (TimeWrapper.realtimeSinceStartup_NotTS - timeStarted).ToString("0.00")), MessageType.Success);
        }

        private WritingStrategy GetWritingStrategy(ModuleType moduleType)
        {
            switch (moduleType)
            {
                default:
                case ModuleType.None:
                    return default;
                case ModuleType.MEEG:
                case ModuleType.MEEG_Preparation:
                    return GetWritingStrategy(MEEG_DD);
                case ModuleType.ECOG:
                    return GetWritingStrategy(ECOG_DD);
                case ModuleType.FMRI_Preparation:
                case ModuleType.FMRI_Scanner:
                    return GetWritingStrategy(FMRI_DD);
            }
        }

        private WritingStrategy GetWritingStrategy(Dropdown dropdown)
        {
            int v = dropdown.value;
            if (v < 0)
                return default;
            if (v > dropdown.options.Count - 1)
                return default;

            string s = dropdown.options[v]?.text;

            return s.ToEnum<WritingStrategy>();
        }

        private void GetUIValues()
        {
            doOneEach = doOneEach_Toggle.isOn;
            doTutorialLevels = doTutorialLevels_Toggle.isOn;
            doGameLevels = doGameLevels_Toggle.isOn;
            doReplayLevels = doReplayLevels_Toggle.isOn;
            runSeparateThread = runSeparateThread_Toggle.isOn;
            runParallelFor = runParallelFor_Toggle.isOn;
        }

        // Update UI for specified folder
        private void CheckStatusForPath()
        {
            GetUIValues();

            infoText.text = "";
            allValidSubjectsCheckedList.Clear();
            allInvalidSubjectsCheckedList.Clear();
            folderToSubjectAnalyzer.Clear();
            string folderUI = folderPathInput.text;

            if (folderUI != "")
            {
#if UNITY_STANDALONE_OSX
                if (folderUI.Contains("// ")) {
                    infoText.text = "Path cannot contain spaces. Please move folder somewhere else (desktop/docs etc);";
                    return;
                }
#endif

                if (!Directory.Exists(folderUI))
                {
                    infoText.text = string.Format("<color={0}>Path doesn't exists</color>", errorColor.ToHex());
                    ShowDebugText("Path doesn't exist: " + folderUI, MessageType.Error);
                    return;
                }

                EDITOR_ONLY_DEFAULT_PATH = folderUI;
            }

            List<string> validExperimentFolderNames = new List<string>();

            // Read all valid experiment folder names
            foreach (string s in GetAllFolders(EDITOR_ONLY_DEFAULT_PATH, REGEX_ANY))
            {
                List<string> validExperimentSubFolderNames = GetAllFolders(s, REGEX_ANY);

                // If it's old school
                if (s.Split('/', '/').GetLast().Length == OLDSCHOOL_LENGTH)
                    // s is the experiment
                    validExperimentFolderNames.Add(s);
                else
                    // Add all the subfolders!
                    validExperimentFolderNames.AddRange(validExperimentSubFolderNames);
            }

            // Find all folders not containing all needed files
            List<string> invalidExperimentFoldersData = new List<string>();
            for (int i = 0; i < validExperimentFolderNames.Count; i++)
            {
                FullLogAnalyzer analyzer = new FullLogAnalyzer();
                analyzer.onReadDataComplete += Analyzer_onReadDataComplete;
                analyzer.onProgress += Analyzer_onProgress;
                analyzer.onProcessFileBegin += Analyzer_onProcessFileBegin;
                analyzer.onLogMessage += Analyzer_onLogMessage;
                if (analyzer.SelectDirectory(validExperimentFolderNames[i], doTutorialLevels, doGameLevels, doReplayLevels))
                {
                    DataError dataError = DataError.Information;
                    string message = string.Format("Found a total of {0} full logs for experiment\n{1}", analyzer.fullDataFiles.Count, validExperimentFolderNames[i]);
                    if (analyzer.fullDataFiles.Count < 12)
                    {
                        message = string.Format("Found a total of {0} full logs, but should expect more for a full run\n{1}", analyzer.fullDataFiles.Count, validExperimentFolderNames[i]);
                        dataError = DataError.Warning;
                    }
                    // Strip folder name, by replacing its root path
                    string stripFolderName = validExperimentFolderNames[i].Replace(EDITOR_ONLY_DEFAULT_PATH, "");
                    CheckedItemMessage item = allValidSubjectsCheckedList.Add(dataError, stripFolderName, validExperimentFolderNames[i], message);
                    // Set sub text
                    item.SetStatusText(string.Format("{0} logs", analyzer.fullDataFiles.Count));

                    folderToSubjectAnalyzer.Add(validExperimentFolderNames[i], analyzer);
                }
                else
                {
                    // Strip folder name, by replacing its root path
                    string stripFolderName = validExperimentFolderNames[i].Replace(EDITOR_ONLY_DEFAULT_PATH, "");
                    CheckedItemMessage item = allInvalidSubjectsCheckedList.Add(DataError.Error, stripFolderName, validExperimentFolderNames[i], analyzer.error);
                    // Set sub text
                    item.SetStatusText(analyzer.errorShort);
                    invalidExperimentFoldersData.Add(validExperimentFolderNames[i]);
                }
            }

            // Remove all invalid data folder from 1st list
            for (int i = 0; i < invalidExperimentFoldersData.Count; i++)
                validExperimentFolderNames.Remove(invalidExperimentFoldersData[i]);

            UpdateSelectedValidFilesLabel();

            List<string> invalidExperimentFolderNamesRelative = GetAllFoldersRelative(invalidExperimentFoldersData, EDITOR_ONLY_DEFAULT_PATH);
            allInvalidSubjectsText.text = string.Format("All invalid subjects in folder ({0})", invalidExperimentFolderNamesRelative.Count);

            // Capture run ID {TA203A}_FullLog_Level_1_1_2019-12-06 15-22-42
            outputFolderPath.text = "//" + OUTPUT_FILENAME;

            //checkButton.interactable = false;
            analyzeButton.interactable = true;
        }

        // Read and combine all per subject analyses to a single file
        private IEnumerator ExportCombineAllData()
        {
            List<FullLogEntryType> analysisTypes = new List<FullLogEntryType>() { FullLogEntryType.Stimulus, FullLogEntryType.Trigger, FullLogEntryType.Cumulative };

            foreach (FullLogEntryType analysisType in analysisTypes)
            {
                int writtenAnalyses = 0;

                List<FullLogAnalyzer> allSubjectAnalyzers = folderToSubjectAnalyzer.Values.ToList();
                StringBuilder combinedAnalysis = new StringBuilder();
                for (int i = 0; i < allSubjectAnalyzers.Count; i++)
                {
                    FullLogAnalyzer fullLogAnalyzer = allSubjectAnalyzers[i];
                    if (!fullLogAnalyzer.isCompleted) continue;

                    List<string> worldLevelCombinations = new List<string>(fullLogAnalyzer.cumulativeAnalyses.Keys);

                    string outputFolder = fullLogAnalyzer.GetOutputFolder();
                    string[] outputFiles = Directory.GetFiles(outputFolder);
                    outputFiles = outputFiles.Where(s => s.ContainsInvariant(analysisType.ToString())).ToArray();

                    foreach (string analysisFilepath in outputFiles)
                    {
                        if (!File.Exists(analysisFilepath))
                        {
                            ShowDebugText(string.Format("Error while trying to combine all file analyses.\nCouldn't locate analysis for subject id:{0} at path: {1}", fullLogAnalyzer.subjectID, analysisFilepath),
                                MessageType.Error);
                            continue;
                        }

                        string[] lines = File.ReadAllLines(analysisFilepath);
                        for (int ln = 0; ln < lines.Length; ln++)
                        {
                            // Skip Header for all next files
                            if (writtenAnalyses > 0 && ln == 0)
                            {
                                // Debug.Log("SKIPPING :: ANALYSIS {0}, LINE {1}\n{2}"._Format(writtenAnalyses, ln, lines[ln]));
                                continue;
                            }

                            combinedAnalysis.Append(lines[ln] + Environment.NewLine);
                        }

                        writtenAnalyses++;
                    }

                    //combinedAnalysis.Append(File.ReadAllText(analysisFilepath));
                    ShowDebugText(string.Format("Combining anaysis for subject id:{0}", fullLogAnalyzer.subjectID), MessageType.Informational);
                }

                yield return null;
                string combineAnalysisFilepath = string.Format("{0}//{1}{2}", EDITOR_ONLY_DEFAULT_PATH, analysisType, OUTPUT_FILENAME);
                while (true)
                {
                    bool fileAlreadyInUse = false;
                    try
                    {
                        File.WriteAllText(combineAnalysisFilepath, combinedAnalysis.ToString());
                        break;
                    }
                    catch (IOException ex)
                    {
                        if (ex.Message.ToLower().Contains("sharing violation"))
                        {
                            ShowDebugText(string.Format("File is locked. Please close all open applications using the file:\n{0}.Waiting 3s\n\n", combineAnalysisFilepath), MessageType.Error);
                            fileAlreadyInUse = true;
                        }
                    }

                    yield return null;
                    if (fileAlreadyInUse)
                        yield return new WaitForSeconds(3f);
                }

                ShowDebugText(string.Format("Merging all analysis completed! You can locate it at: {0}\n\n", combineAnalysisFilepath), MessageType.Success);
            }
        }

#endregion

#region Update UI

        // Keep Thread-Safe
        private void UpdateAnalyzerProcessBegin(string text)
        {
            ShowDebugText(string.Format("Analyzing file: {0}", Path.GetFileName(text)), MessageType.Informational);
        }

        // Keep Thread-Safe
        private void UpdateReadDataComplete(FullLogAnalyzer analyzer)
        {
            analyzer.ExportData();

            checkButton.interactable = true;
            analyzeButton.interactable = false;
            ShowDebugText(string.Format("Analyzing completed successfully. You can find the analysis in: \n{0}\n", analyzer.GetOutputFile("<WORLD_LEVEL>", "<TYPE>")), MessageType.Success);
            completeAnalyzedExperiments++;
            progressSlider.value = (float)completeAnalyzedExperiments / (float)selectedSubjects.Count;
            analyzer.Clear();
            GC.Collect();
        }

        // Keep Thread-Safe
        private void UpdateAnalyzerProgress(float val)
        {
            float perRunVal = 1f / (float)selectedSubjects.Count;
            progressSlider.value = (float)completeAnalyzedExperiments / (float)selectedSubjects.Count + val * perRunVal;
        }

#endregion

#region Event Handlers

        private void Analyzer_onLogMessage(object sender, LogMessageEventArgs e)
        {
            if (runSeparateThread)
                AsyncThread.RunOnMainThread_ASAP_TS(() => { ShowDebugText(e.message, e.type); });
            else
                ShowDebugText(e.message, e.type);
        }

        private void Analyzer_onProcessFileBegin(object sender, TGP.Helpers.EventArgs<string> e)
        {
            if (runSeparateThread)
                AsyncThread.RunOnMainThread_ASAP_TS(() => { UpdateAnalyzerProcessBegin(e.value); });
            else
                UpdateAnalyzerProcessBegin(e.value);
        }

        private void Analyzer_onReadDataComplete(object sender, EventArgs e)
        {
            if (runSeparateThread)
                AsyncThread.RunOnMainThread_ASAP_TS(() => { UpdateReadDataComplete(sender as FullLogAnalyzer); });
            else
                UpdateReadDataComplete(sender as FullLogAnalyzer);
        }

        private void Analyzer_onProgress(object sender, TGP.Helpers.EventArgs<float> e)
        {
            if (runSeparateThread)
                AsyncThread.RunOnMainThread_ASAP_TS(() => { UpdateAnalyzerProgress(e.value); });
            else
                UpdateAnalyzerProgress(e.value);
        }

        private void AllValidSubjectsCheckedList_onSelectedNumberChanged(object sender, System.EventArgs e)
        {
            UpdateSelectedValidFilesLabel();
        }

        private void AnalyzeButton_OnClick()
        {
            checkButton.interactable = false;
            analyzeButton.interactable = false;
            Run();
        }

#endregion

#region Helper Methods

        private void UpdateSelectedValidFilesLabel()
        {
            allValidSubjectsText.text = string.Format("All valid subjects in folder ({0}/{1})", allValidSubjectsCheckedList.GetSelectedValues().Count, allValidSubjectsCheckedList.options.Count);
        }

        private void ShowDebugText(string text, MessageType type)
        {
            Color selectedColor = Color.yellow;
            switch (type)
            {
                case MessageType.Informational:
                    selectedColor = messageColor;
                    break;
                case MessageType.Error:
                    selectedColor = errorColor;
                    break;
                case MessageType.Success:
                    selectedColor = successColor;
                    break;
            }
            infoText.text = infoText.text.Insert(0, string.Format("<color={0}>{1}</color>\n", selectedColor.ToHex(), text));
            LayoutRebuilder.ForceRebuildLayoutImmediate(infoText.rectTransform.parent as RectTransform);

            string dumpFilepath = EDITOR_ONLY_DEFAULT_PATH + "//dump.txt";
            File.AppendAllText(dumpFilepath, text + Environment.NewLine);
        }

        // Match and get all selected FullLogAnalyzers based on UI
        private List<FullLogAnalyzer> GetSelectedSubjectAnalyzer()
        {
            List<FullLogAnalyzer> selectedAnalyzers = new List<FullLogAnalyzer>();

            // Get through selected subject id in UI
            for (int i = 0; i < selectedSubjects.Count; i++)
            {
                // Get only the id"//{TA944A}
                string subjectID = selectedSubjects[i];
                FullLogAnalyzer _analyzer = folderToSubjectAnalyzer[subjectID];
                if (_analyzer != null)
                    selectedAnalyzers.Add(_analyzer);
                else
                    ShowDebugText(string.Format("Couldn't find FullLogAnalyzer for subject ID:{0}", subjectID), MessageType.Error);
            }
            return selectedAnalyzers;
        }

        private List<string> GetAllFoldersRelative(List<string> folders, string rootPath)
        {
            for (int i = 0; i < folders.Count; i++)
            {
                folders[i] = folders[i].Replace(rootPath, "");
            }
            return folders;
        }

        private List<string> GetAllFolders(string path, string regexPattern)
        {
            List<string> validFolders = new List<string>();
            List<string> folders = Directory.EnumerateDirectories(path).ToList();

            Regex regex = new Regex(regexPattern);
            Match regexMatch;
            for (int i = 0; i < folders.Count; i++)
            {
                regexMatch = regex.Match(folders[i]);
                if (regexMatch.Success)
                    validFolders.Add(folders[i]);
            }

            return validFolders;
        }

#endregion
    }
}