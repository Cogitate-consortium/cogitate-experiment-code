using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// [CHECK]
/// </summary>
namespace Experiment.Analyzer.UI
{
    public class CheckedListBox : MonoBehaviour
    {
        private const string TOGGLE_PREFAB_LABEL_NAME = "Label";

        public event EventHandler<EventArgs> onSelectedNumberChanged;

        [Header("Editor")]
        public CheckedItemMessage togglePrefab;
        public Transform containerTransform;
        public Toggle checkAllToggle;

        public List<CheckedItemMessage> options = new List<CheckedItemMessage>();

        private void Start()
        {
            togglePrefab.gameObject.SetActive(false);

            if (checkAllToggle != null)
                checkAllToggle.onValueChanged.AddListener(ToggleSelectAll);
        }

        private void ToggleSelectAll(bool selected)
        {
            for (int i = 0; i < options.Count; i++)
            {
                options[i].isOn = selected;
            }
        }

        #region Public Methods

        public CheckedItemMessage Add(DataError dataStatus, string text, string value, string message = "")
        {
            CheckedItemMessage option = InstantiateCheckedItem(dataStatus, text, value, message);
            options.Add(option);
            option.onValueChanged += Option_onValueChanged;
            return option;
        }

        private void Option_onValueChanged(object sender, EventArgs e)
        {
            onSelectedNumberChanged?.Invoke(this, EventArgs.Empty);
        }

        public List<CheckedItemMessage> GetSelectedToggles()
        {
            List<CheckedItemMessage> selected = new List<CheckedItemMessage>();
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i].isOn)
                    selected.Add(options[i]);
            }
            return selected;
        }

        public List<string> GetSelectedValues()
        {
            List<CheckedItemMessage> selectedToggles = GetSelectedToggles();
            List<string> selectedValues = new List<string>();
            for (int i = 0; i < selectedToggles.Count; i++)
            {
                selectedValues.Add(selectedToggles[i].value);
            }
            return selectedValues;
        }

        public void Clear()
        {
            options.Clear();
            containerTransform.Genocide();
            onSelectedNumberChanged?.Invoke(this, EventArgs.Empty);
            if (checkAllToggle != null)
                checkAllToggle.isOn = true;
        }

        #endregion

        private CheckedItemMessage InstantiateCheckedItem(DataError dataStatus, string text, string value, string message = "")
        {
            CheckedItemMessage option = GameObject.Instantiate<CheckedItemMessage>(togglePrefab, containerTransform);
            option.name = text;
            option.Set(dataStatus, text, value, message);
            option.gameObject.SetActive(true);
            return option;
        }

    }
}