using ExperimentLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Experiment.Subject.UI
{
    public class SubjectFieldSelector : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText = null;
        [SerializeField] private InputField inputField = null;
        [SerializeField] private Dropdown inputDropdown = null;

        private SubjectField subjectField = null;

        private void Awake()
        {
            inputDropdown.onValueChanged.AddListener(Dropdown_ValueChange);
            inputField.onValueChanged.AddListener(InputField_ValueChange);
        }

        private void Dropdown_ValueChange(int arg0)
        {
            if (arg0 < 0 || arg0 >= inputDropdown.options.Count) return;
            subjectField.answer = inputDropdown.options[arg0].text;
        }

        private void InputField_ValueChange(string arg0)
        {
            subjectField.answer = arg0;
        }
        public void Initialize(SubjectField subjectField)
        {
            this.subjectField = subjectField;
            name = "SubjectFieldSelector - " + subjectField.name;
            Refresh();
        }

        public void Refresh()
        {
            string fieldsMarking = ExperimentLibraryManager.Config.Subject.mandatoryFieldsMarking;
            labelText.text = subjectField.name;
            if (subjectField.isMandatory)
                labelText.text += fieldsMarking;

            // Are we talking Open answer?
            if (subjectField.possibleAnswers.Count == 0 ||
                (subjectField.possibleAnswers.Count == 1 && (
                    subjectField.possibleAnswers[0] == "string" ||
                    subjectField.possibleAnswers[0] == "int" ||
                    subjectField.possibleAnswers[0] == "float")))
            {
                if (subjectField.possibleAnswers[0] == "int")
                    inputField.contentType = InputField.ContentType.IntegerNumber;
                else if (subjectField.possibleAnswers[0] == "float")
                    inputField.contentType = InputField.ContentType.DecimalNumber;
                else
                    inputField.contentType = InputField.ContentType.Standard;

                inputDropdown.gameObject.SetActive(false);
                inputField.gameObject.SetActive(true);
                Text placeHolderText = inputField.placeholder.GetComponent<Text>();
                if (placeHolderText) placeHolderText.text = string.Format(ExperimentLibraryManager.Config.Subject.placeHolderString, subjectField.name); // Assume string may contain {0}
                inputField.text = subjectField.answer;
            }
            else
            {
                inputDropdown.ClearOptions();

                inputDropdown.options.Add(new Dropdown.OptionData(ExperimentLibraryManager.Config.Subject.noAnswer));

                int sF_OptionIndex = -1; // Doesn't include "empty" answer
                for (int i = 0; i < subjectField.possibleAnswers.Count; i++)
                {
                    string possAnswer = subjectField.possibleAnswers[i];
                    if (subjectField.answer == possAnswer)
                        sF_OptionIndex = i;
                    inputDropdown.options.Add(new Dropdown.OptionData(possAnswer));
                }

                inputDropdown.gameObject.SetActive(true);
                inputField.gameObject.SetActive(false);

                if (sF_OptionIndex >= 0)
                    inputDropdown.value = sF_OptionIndex + 1;
                inputDropdown.RefreshShownValue();
            }
        }
    }
}