using System;
using System.Collections.Generic;
using TGP.Helpers;

namespace Experiment.Subject
{
    public enum StartFlipped { AlwaysStartNormal = 0, AlwaysStartFlipped = 1, StartFlippedForEvenIDs = 2, StartFlippedForOddIDS = 3, ValueFromUI = 4 }

    [Serializable]
    public class SubjectInfo
    {
        public const int subjectNumberLength = 3;
        public const int subjectRunLength = 1;
        /// <summary>
        /// [SOS] The full ID ; for number use <see cref="subjectNumber"/>
        /// </summary>
        public string id;
        /// <summary>
        /// Will Log to & Load from here
        /// </summary>
        public string folder;
        public string labCode;
        public int subjectNumber;
        public HandType handType;
        public string runNumber;
        public bool? startFlippedUI;
        public readonly List<SubjectField> subjectFields = new List<SubjectField>();

        public string GetOtherRunFolder(string runNumber)
        {
            return GetFolder(labCode, subjectNumber, runNumber);
        }

        internal string GetOtherRunID(string otherRunNumber)
        {
            return GetID(labCode, subjectNumber, otherRunNumber);
        }

        public static string GetFolder(string labCode, int subjectNumber)
        {
            return labCode + subjectNumber.AddLeadingSymbols(subjectNumberLength, '0') + "/";
        }

        public static string GetFolder(string labCode, int subjectNumber, string runNumber)
        {
            return GetFolder(labCode, subjectNumber) + runNumber + "/";
        }

        public static string GetID(string labCode, int subjectNumber, string runNumber)
        {
            return labCode + subjectNumber.AddLeadingSymbols(subjectNumberLength, '0') + runNumber;
        }

        public SubjectInfo(string labCode, int subjectNumber, string runNumber, HandType handType, bool? startFlippedUI, params SubjectField[] subjectFields)
        {
            this.labCode = labCode;
            this.subjectNumber = subjectNumber;
            this.runNumber = runNumber;
            this.handType = handType;
            this.startFlippedUI = startFlippedUI;
            id = GetID(labCode, subjectNumber, runNumber);
            folder = GetFolder(labCode, subjectNumber, runNumber);
            this.subjectFields.Clear();
            this.subjectFields.AddRange(subjectFields);
        }
    }

    [Serializable]
    public class SubjectConfig
    {
        // public int subjectNumberLength = 3;
        // public int subjectRunLength = 1;
        public string TEXT_DD_COMMENT = "Those appear in the drop-down of the subject window";
        public string TEXT_DD_NO_OPTION = "FRESH OR RESUME?";
        public string TEXT_DD_NEW_RUN = "START FRESH";
        public string TEXT_DD_EXISTING_COMMENT = "Run number will be inserted at {0}";
        public string TEXT_DD_EXISTING = "RESUME {0}";

        public int reactionTimeMS = 300;
        public MinMax reactionSpeedMinMax = new MinMax(150, 400);
        public MinMax reactionSpeedInteractableMultiplierMinMan = new MinMax(0.67f, 1.5f);

        public bool getDemographicFields = false;
        public string mandatoryFieldsMarking = " (*)";
        public string noAnswer = "No Answer";
        public string placeHolderString_Comment = "Shown in text fields where the player hasn't typed something yet. {0} will be replaced by the field's name.";
        public string placeHolderString = "Enter Subject's {0}";
        public List<SubjectField> fieldsToQuery = new List<SubjectField>()
        {
            new SubjectField("Age", true, true, "int"),
            new SubjectField("Gender", true, true, "Male", "Female", "Other"),
            new SubjectField("Dominant Hand", true, true, "Right", "Left", "Neither"),
            new SubjectField("Visual Acuity", true, true, "No Glasses", "Glasses", "Contacts"),
            new SubjectField("Dominant Eye", true, true, "Right", "Left", "Neither"),
            new SubjectField("Diopter of Lenses", false, true, "string"),
            new SubjectField("Color Blindness", true, true, "None", "Deuteranomaly", "Protanomaly", "Protanopia", "Deuteranopia", "Tritanomaly", "Tritanopia", "Monochromacy"),
            new SubjectField("Level of Education", true, true, "None", "Primary", "Secondary", "Tertiary"),
            new SubjectField("Ethnicity", true, true, "string"),
            new SubjectField("Primary Language", true, true, "string"),
            new SubjectField("Secondary Language", true, true, "string"),
            new SubjectField("Auditory Sensitivity", true, false, "1", "2", "3", "4", "5"),
            new SubjectField("Known Medical Conditions", false, false, "string"),
            new SubjectField("Current Medications", false, false, "string"),
        };

        public float GetFallingSpeedMultiplier()
        {
            return 1f / ((float)reactionTimeMS).Retargeted(reactionSpeedMinMax.min, reactionSpeedMinMax.max, reactionSpeedInteractableMultiplierMinMan.min, reactionSpeedInteractableMultiplierMinMan.max);
        }
    }

    [System.Serializable]
    public class SubjectField
    {
        public string name = "";
        public string answer { get; set; }
        public bool isMandatory = false;
        public bool isEnabled = true;
        public List<string> possibleAnswers = new List<string>();

        public SubjectField(string name, bool isMandatory, bool isEnabled, params string[] possibleAnswers)
        {
            this.name = name;
            this.isMandatory = isMandatory;
            this.isEnabled = isEnabled;
            this.possibleAnswers.Clear();
            this.possibleAnswers.AddRange(possibleAnswers);

            if (possibleAnswers.Length == 0)
                this.possibleAnswers.Add("string");
        }

        public string Randomize()
        {
            if (possibleAnswers.Count == 0)
                answer = "";
            else if (possibleAnswers.Count == 1)
            {
                if (possibleAnswers[0] == "string")
                    answer = "random-string";
                else if (possibleAnswers[0] == "int")
                    answer = Utility_Helper.RandomRange(int.MinValue, int.MaxValue).ToString();
                else if (possibleAnswers[0] == "float")
                    answer = Utility_Helper.RandomRange(float.MinValue, float.MaxValue).ToString();
            }
            else
                answer = possibleAnswers.GetRandom();

            return answer;
        }

        public bool Validate()
        {
            string result = "";
            return Validate(out result);
        }

        public bool Validate(out string result)
        {
#if UNITY_EDITOR
            // result = "EDITOR"; return true;
#endif
            // We have nothing to compare against
            if (possibleAnswers.Count == 0)
            {
                result = "Wrong Configuration";
                this.LogWarning("Something went wrong");
                return true;
            }

            // We are not contained in the answers
            // Check for floats / ints
            if (possibleAnswers.Count == 1)
            {
                // For strings we just do
                if (possibleAnswers[0] == "string")
                {
                    if (answer.IsNullOrEmpty())
                    {
                        result = "Open answer left empty";
                        return false;
                    }
                    else
                    {
                        result = "Open answer";
                        return true;
                    }
                }
                if (possibleAnswers[0] == "int")
                {
                    int answerInt = 0;
                    if (int.TryParse(answer, out answerInt))
                    {
                        result = "Integer";
                        return true;
                    }
                    else
                    {
                        result = "Expected Integer";
                        return false;
                    }
                }
                if (possibleAnswers[0] == "float")
                {
                    float answerFloat = 0;
                    if (float.TryParse(answer, out answerFloat))
                    {
                        result = "Float";
                        return true;
                    }
                    else
                    {
                        result = "Expected Float";
                        return false;
                    }
                }
            }


            // Check answers
            if (possibleAnswers.Contains(answer))
            {
                result = "Contained within possible answers";
                return true;
            }

            result = "Not contained within possible answers";
            return false;
        }

        public override string ToString()
        {
            string baseString = "{0} ({1}, {2})"._Format(name, isMandatory ? "Mandatory" : "Optional", isEnabled ? "Enabled" : "Disabled");
            string answerString = " -> {0} ({1})"._Format(answer, Validate() ? "Valid" : "Invalid");

            return baseString + (answer.IsNullOrEmpty() ? "" : answerString);
        }
    }

    public enum Gender { Male, Female, Other }
    public enum DominantHand { Left, Right, Both }
}