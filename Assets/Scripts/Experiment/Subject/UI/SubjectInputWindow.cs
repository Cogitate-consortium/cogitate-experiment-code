/// NS_REMOVE | SEGMENT
using ExperimentLibrary.Test;
// Should be fed to through initialization
using Experiment.Helpers;
using Peripherals.Logging.Core;

// NS_SEGMENT | Model behind UI
using Peripherals.Logging;

using System.Collections.Generic;
using TGP.Helpers;
using Helpers.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using ExperimentLibrary;
using System;

namespace Experiment.Subject.UI
{
    public class SubjectInputWindow : CanvasWindow
    {
        #region UI

        [Header("UI")]
        [SerializeField] private CanvasGroup globalCG = null;
        [SerializeField] private Button StartButton = null;
        [SerializeField] private Button AutoFillSubjectInput = null;
        [SerializeField] private Text SubjectLabCodeText = null;
        [SerializeField] private InputField SubjectIdInputField = null;
        [SerializeField] private InputField RunIdInputField = null;
        [SerializeField] private GameObject SubjectFieldsParent = null;
        [SerializeField] private GameObject SubjectFieldInputPrefab = null;

        [Header("Error messages")]
        [SerializeField] private Text SubjectIdError = null;
        [SerializeField] private Text RunIdError = null;
        [SerializeField] private Text InputFieldsError = null;
        [SerializeField] private Color inactiveErrorColor = Color.gray;
        [SerializeField] private Color activeErrorColor = Color.red;

        [SerializeField] private CanvasGroup inputFlippingCG = null;
        [SerializeField] private Toggle inputFlippingToggle = null;
        [SerializeField] private Dropdown handDD = null;
        
        [SerializeField] private CanvasGroup continueFromCG = null;
        [SerializeField] private Dropdown continueFromDD = null;

        private static SubjectConfig config { get { return ExperimentLibraryManager.Config.Subject; } }

        /// <summary>
        /// Null / Empty -> No choice, <see cref="TEXT_DD_NEW_RUN"/> -> NEW RUN, Otherwise -> Old run
        /// </summary>
        private RunFolder? continueFromRun;

        // Should be empty, problematic unity behavior 
        // https://forum.unity.com/threads/solved-dropdown-always-selects-the-first-option-automatically.518034/
        private string TEXT_DD_NO_OPTION = "CONTINUE ?";
        private string TEXT_DD_NEW_RUN = "NEW RUN";
        private string TEXT_DD_EXISTING = "{0}";

        private readonly Dictionary<string, RunFolder?> ddOptionToRunFolder = new Dictionary<string, RunFolder?>();

        #endregion
        protected void Awake()
        {
            SubjectLabCodeText.text = ExperimentLibraryManager.Config.LabCode;
            TEXT_DD_NO_OPTION = ExperimentLibraryManager.Config.Subject.TEXT_DD_NO_OPTION;
            TEXT_DD_NEW_RUN = ExperimentLibraryManager.Config.Subject.TEXT_DD_NEW_RUN;
            TEXT_DD_EXISTING = ExperimentLibraryManager.Config.Subject.TEXT_DD_EXISTING;

            SubjectIdInputField.text = "";
            RunIdInputField.text = "";
            InputFieldsError.text = "";

            activeFields.Clear();
            if (config.getDemographicFields)
                activeFields.AddRange(config.fieldsToQuery.FindAll(a => a.isEnabled));
            else
                this.LogWarning("Demographics disabled!");

            SubjectIdInputField.contentType = InputField.ContentType.IntegerNumber;
            SubjectIdInputField.characterLimit = SubjectInfo.subjectNumberLength;

            RunIdInputField.contentType = InputField.ContentType.Alphanumeric;
            RunIdInputField.characterLimit = SubjectInfo.subjectRunLength;

            continueFromCG.Toggle(false);
            continueFromRun = null;
            continueFromDD.onValueChanged.AddListener(ContinueFromDD_onValueChanged);

            if (ExperimentLibraryManager.Config.Input.startFlippedStrategy == StartFlipped.ValueFromUI)
            {
                inputFlippingCG.Toggle(true);
                inputFlippingToggle.isOn = ExperimentLibraryManager.Config.Input.startFlippedUI_DefaultValue;
            }
            else
                inputFlippingCG.Toggle(false);

            handDD.options.Clear();
            handDD.AddOptions(Utility_Helper.EnumGetValuesAsStrings<HandType>());
            handDD.value = (int)ExperimentLibraryManager.Config.Input.defaultHandType;

            InitializeSubjectWindow();

            StartButton.onClick.AddListener(StartButton_OnClick);
            AutoFillSubjectInput.onClick.AddListener(AutoFillSubjectInput_OnClick);

            SubjectIdError.color = RunIdError.color = inactiveErrorColor;
            InputFieldsError.color = activeErrorColor;
        }

        private void ContinueFromDD_onValueChanged(int arg0)
        {
            string optionString = continueFromDD.options[arg0].text;

            continueFromRun = ddOptionToRunFolder[optionString];
        }

        private readonly List<SubjectField> activeFields = new List<SubjectField>();

        private void InitializeSubjectWindow()
        {
            SubjectFieldsParent.transform.Genocide();

            foreach (SubjectField sF in activeFields)
            {
                this.Log("Setting up {0}"._Format(sF));

                GameObject temp = SubjectFieldsParent.PrefabInstantiateAsChild(SubjectFieldInputPrefab, true);
                SubjectFieldSelector sFF = temp.GetComponentInChildren<SubjectFieldSelector>();
                sFF.Initialize(sF);
            }
            /*
            SubjectAgeInputField.text = "";
            SubjectGenderDropDown.ClearOptions();
            SubjectGenderDropDown.AddOptions(Utility_Helper.EnumGetValuesAsStrings<Gender>());
            SubjectDominantHandDropDown.ClearOptions();
            SubjectDominantHandDropDown.AddOptions(Utility_Helper.EnumGetValuesAsStrings<DominantHand>());
            SubjectDominantHandDropDown.value = (int)DominantHand.Right;
            */

            ToggleInteractable(true);
        }

        public override void Toggle(bool show, int sortingOrder)
        {
            base.Toggle(show, sortingOrder);

            if (EditorOnlyUtilities.mainMenu_AutoCompleteDetails == true)
            {
                if (show)
                {
                    CreateDummySubject();
                    ValidateAndCreateSubject();
                    //  CreateDummySubject();
                }
            }
        }

        private void AutoFillSubjectInput_OnClick()
        {
            CreateDummySubject();
        }

        private void StartButton_OnClick()
        {
            ValidateAndCreateSubject();
        }

        private void ValidateAndCreateSubject()
        {
            SubjectIdError.color = RunIdError.color = inactiveErrorColor;

            int subjectNumberLength = SubjectInfo.subjectNumberLength; // ApplicationLibrary.Config.
            int subjectRunLength = SubjectInfo.subjectRunLength; // ApplicationLibrary.Config.

            if (!SubjectIdInputField.text.Length.IsBetween(1, subjectNumberLength))
            {
                SubjectIdError.color = activeErrorColor;
                this.LogWarning("Subject id is invalid; it must be {0} characters exactly e.g. {1}"._Format(subjectNumberLength, 1.AddLeadingSymbols(subjectNumberLength, '0')));
                return;
            }
            if (!RunIdInputField.text.Length.IsBetween(1, subjectRunLength))
            {
                RunIdError.color = activeErrorColor;
                this.LogWarning("Run id is invalid; it must be {0} characters exactly e.g. {1}"._Format(subjectRunLength, 1.AddLeadingSymbols(subjectRunLength, '0')));
                return;
            }

            string wantedLabCode = SubjectLabCodeText.text;
            int wantedSubjectNumber = SubjectIdInputField.text.ToInt();
            string wantedRunNumber = RunIdInputField.text;
            string wantedID = SubjectInfo.GetID(wantedLabCode, wantedSubjectNumber, wantedRunNumber);

            // First off, get the subject Number folder
            string subjectFolder_AllRuns = SubjectInfo.GetFolder(wantedLabCode, wantedSubjectNumber);

            bool? inputFlipUI = inputFlippingToggle?.isOn;
            HandType handType = (HandType)handDD.value;

            SubjectInfo wantedSubject = new SubjectInfo(wantedLabCode, wantedSubjectNumber, wantedRunNumber, handType, inputFlipUI, activeFields.ToArray());

            // What other runs do we have
            List<RunFolder> otherRunsSameSubjectNumber = new List<RunFolder>();

            List<string> allRunNumbers = LogWrapper.GetLogFolderContents(subjectFolder_AllRuns, FilesFolders.Folders, false);

            if (allRunNumbers != null)
                foreach (string otherRun in allRunNumbers)
                {
                    // Filter out PREP runs
                    RunFolder rF = new RunFolder(otherRun, wantedSubject);

                    string runModuleFilePath = ExperimentLogger.GetModuleFilePath(rF.runFolder, rF.subjectID);
                    string runModuleSTR = FileWrapper.ReadFromFile(runModuleFilePath);

                    if (runModuleSTR.IsNullOrEmpty())
                    {
                        this.LogError("Skipping run " + rF.subjectID + " because it had no MODULE txt");
                        continue;
                    }

                    ModuleType runModule = runModuleSTR.ToEnum<ModuleType>();

                    if (runModule.ToPrepVsFull() != PrepVsFull.Full) // Only full runs can be continued
                    {
                        this.LogWarning("Skipping run " + rF.subjectID + " because of its module " + runModule);
                        continue;
                    }

                    otherRunsSameSubjectNumber.Add(rF);
                }

            // It's our first time here (even if the base folder exists, we have no other runs)
            if (otherRunsSameSubjectNumber == null || otherRunsSameSubjectNumber.Count == 0)
            {
                // Nothing to do, this run is a valid choice!

            }
            // If we've been here before
            else
            {
                // Are we aiming to start a RUN we have already started? Invalid!
                if (otherRunsSameSubjectNumber.FindAll(rF => rF.runNumber == wantedRunNumber).Count > 0)
                {
                    InputFieldsError.text = "Duplicate Subject ID (lab code + subject number + run number)";
                    this.LogWarning("Duplicate Subject ID (lab code + subject number + run number)");
                    return;
                }

                // Ok this run ID is a fresh one, and other runs do exist, do we want to continue one of them?
                if (continueFromRun == null)
                {
                    // Is that even a question?
                    if (ExperimentManagerApplication.prepVsFull != PrepVsFull.Full) // Only full runs can be continued
                    {
                        continueFromRun = new RunFolder(wantedRunNumber, wantedSubject);
                    }
                    else
                    {
                        continueFromCG.Toggle(true);

                        List<string> options_STR = new List<string>();
                        ddOptionToRunFolder.Clear();

                        options_STR.Add(TEXT_DD_NO_OPTION);
                        ddOptionToRunFolder.Add(TEXT_DD_NO_OPTION, null);

                        options_STR.Add(TEXT_DD_NEW_RUN);
                        ddOptionToRunFolder.Add(TEXT_DD_NEW_RUN, new RunFolder(wantedRunNumber, wantedSubject));

                        foreach (RunFolder otherRun in otherRunsSameSubjectNumber)
                        {
                            string key = TEXT_DD_EXISTING._Format(otherRun.subjectID);
                            options_STR.Add(key);
                            ddOptionToRunFolder.Add(key, otherRun);
                        }

                        continueFromDD.ClearOptions();
                        continueFromDD.AddOptions(options_STR);

                        InputFieldsError.text = "Do you want to continue one of these runs or start a new one?";
                        this.LogWarning("Do you want to continue one of these runs or start a new one?");
                        return;
                    }
                }
                
                // At this point we have a fresh ID, and want to continue a run which is fine
            }

            InputFieldsError.text = "";

            // Validate fields
            foreach (SubjectField sF in activeFields)
            {
                // Validate
                string validationResult = "";
                if (sF.isMandatory && !sF.Validate(out validationResult))
                {
                    string message = "{0} required"._Format(sF.name);// // "{0} is invalid ({1})"._Format(sF.name, validationResult);
                    InputFieldsError.text = message;
                    this.LogWarning(message);
                    return;
                }
            }

            Action<bool> callback = success =>
            {
                if (success)
                    base.Toggle(false, 0);
                else // unlock to allow retries
                    ToggleInteractable(true);
            };

            if (continueFromRun == null || continueFromRun.Value.runNumber == wantedRunNumber)
                ExperimentManagerApplication.instance.InitializeSubjectSession(wantedSubject, callback);
            else
            {
                ExperimentManagerApplication.instance.InitializeSubjectSession(wantedSubject, callback, continueFromRun.Value.runFolder);
            }

            ToggleInteractable(false);
        }

        private void ToggleInteractable(bool v)
        {
            globalCG.interactable = v;
        }

        public void CreateDummySubject()
        {
            SubjectIdInputField.text = UnityEngine.Random.Range(100, 999).ToString();
            RunIdInputField.text = "A";

            // Validate fields
            foreach (SubjectField sF in activeFields)
            {
                string randAnswer = sF.Randomize();
                // this.LogWarning("Set {0} to {1}"._Format(sF.name, randAnswer));
            }

            // Update the UI
            foreach (SubjectFieldSelector sFS in SubjectFieldsParent.GetComponentsInChildren<SubjectFieldSelector>())
                sFS.Refresh();
        }

        private struct RunFolder
        {
            public string runNumber;
            public string subjectID;
            public string runFolder;

            public RunFolder(string runNumber, SubjectInfo subject)
            {
                this.runNumber = runNumber;
                subjectID = subject.GetOtherRunID(runNumber);
                string runFolderInLogs = subject.GetOtherRunFolder(runNumber);
                runFolder = LogWrapper.GetPath(runFolderInLogs);

            }
        }
    }
}