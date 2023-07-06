using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.MyAccount
{
    public class MyProfileAdditionalInfoListComponentView : MonoBehaviour
    {
        private const string DEFAULT_SELECT_OPTION_VALUE = "- Select an option -";
        private const string OTHER_VALUE = "Other";

        [SerializeField] private Button addButton;
        [SerializeField] private DropdownComponentView optionsDropdown;
        [SerializeField] private TMP_InputField freeFormInputField;
        [SerializeField] private TMP_InputField dateInputField;
        [SerializeField] private DropdownComponentView strictValueListDropdown;
        [SerializeField] private MyProfileAdditionalInfoEntryComponentView entryPrefab;
        [SerializeField] private RectTransform entryContainer;
        [SerializeField] private GameObject commonInputContainer;
        [SerializeField] private GameObject otherInputContainer;
        [SerializeField] private Button cancelOtherInputButton;
        [SerializeField] private TMP_InputField otherInputField;

        public Action OnAdditionalFieldAdded;
        public Action OnAdditionalFieldRemoved;

        private readonly Dictionary<string, MyProfileAdditionalInfoEntryComponentView> entries = new ();
        private readonly List<ToggleComponentModel> optionToggles = new ();
        private readonly List<ToggleComponentModel> valueToggles = new ();

        private string currentOptionId;
        private AdditionalInfoOptionsModel optionsModel;
        private bool isOtherInputActivated;

        private void Awake()
        {
            optionsDropdown.OnOptionSelectionChanged += (isOn, optionId, optionName) =>
            {
                if (!isOn) return;

                ChangeCurrentOption(optionId, optionName);
            };

            dateInputField.onValueChanged.AddListener(UpdateAddButtonInteractibilityByDateFormat);
            freeFormInputField.onValueChanged.AddListener(UpdateAddButtonInteractibilityByFreeFormText);
            otherInputField.onValueChanged.AddListener(UpdateAddButtonInteractibilityByFreeFormText);

            strictValueListDropdown.OnOptionSelectionChanged += (isOn, optionId, optionName) =>
            {
                if (!isOn) return;

                if (optionId != OTHER_VALUE)
                    strictValueListDropdown.SetTitle(optionName);
                else
                    SetOtherInputActive(true);

                UpdateAddButtonInteractibilityByFreeFormText(isOtherInputActivated ? otherInputField.text : optionName);
            };

            cancelOtherInputButton.onClick.AddListener(() => SetOtherInputActive(false));

            addButton.onClick.AddListener(() =>
            {
                if (string.IsNullOrEmpty(currentOptionId)) return;
                optionsModel.Options[currentOptionId].OnValueSubmitted.Invoke(GetCurrentValue());
                OnAdditionalFieldAdded?.Invoke();
            });
        }

        public void SetOptions(AdditionalInfoOptionsModel model)
        {
            optionsModel = model;

            optionToggles.Clear();

            foreach ((string optionId, AdditionalInfoOptionsModel.Option option) in model.Options)
            {
                if (!option.IsAvailable) continue;

                optionToggles.Add(new ToggleComponentModel
                {
                    text = option.Name,
                    id = optionId,
                    isOn = false,
                    isTextActive = true,
                    changeTextColorOnSelect = true,
                });
            }

            optionsDropdown.SetOptions(optionToggles);

            if (optionToggles.Count > 0)
            {
                ToggleComponentModel option = optionToggles[0];
                ChangeCurrentOption(option.id, option.text);
            }
        }

        public void SetValues(Dictionary<string, (string title, string value)> values)
        {
            ClearValues();

            foreach (KeyValuePair<string, (string title, string value)> pair in values)
            {
                MyProfileAdditionalInfoEntryComponentView entry = Instantiate(entryPrefab, entryContainer);
                entry.Set(pair.Key, pair.Value.title, pair.Value.value);
                entry.OnRemoved += OnInfoRemoved;
                entries[pair.Key] = entry;
            }
        }

        private void ClearValues()
        {
            foreach (MyProfileAdditionalInfoEntryComponentView entry in entries.Values)
            {
                entry.OnRemoved -= OnInfoRemoved;
                Destroy(entry.gameObject);
            }

            entries.Clear();
        }

        private void ChangeCurrentOption(string optionId, string optionName)
        {
            currentOptionId = optionId;
            SetOtherInputActive(false);

            switch (optionsModel.Options[optionId].InputType)
            {
                case AdditionalInfoOptionsModel.InputType.Date:
                    dateInputField.gameObject.SetActive(true);
                    freeFormInputField.gameObject.SetActive(false);
                    strictValueListDropdown.gameObject.SetActive(false);
                    break;
                case AdditionalInfoOptionsModel.InputType.FreeFormText:
                    dateInputField.gameObject.SetActive(false);
                    freeFormInputField.gameObject.SetActive(true);
                    strictValueListDropdown.gameObject.SetActive(false);
                    break;
                case AdditionalInfoOptionsModel.InputType.StrictValueList:
                    dateInputField.gameObject.SetActive(false);
                    freeFormInputField.gameObject.SetActive(false);
                    strictValueListDropdown.gameObject.SetActive(true);
                    FillStrictValueOptions(optionsModel.Options[optionId].Values);
                    strictValueListDropdown.SetTitle(DEFAULT_SELECT_OPTION_VALUE);
                    break;
            }

            optionsDropdown.SetTitle(optionName);
            dateInputField.text = "";
            freeFormInputField.text = "";
            addButton.interactable = false;
        }

        private void OnInfoRemoved(string optionId)
        {
            optionsModel.Options[optionId].OnRemoved.Invoke();
            OnAdditionalFieldRemoved?.Invoke();
        }

        private string GetCurrentValue()
        {
            switch (optionsModel.Options[currentOptionId].InputType)
            {
                case AdditionalInfoOptionsModel.InputType.Date:
                    return dateInputField.text;
                case AdditionalInfoOptionsModel.InputType.FreeFormText:
                    return freeFormInputField.text;
                case AdditionalInfoOptionsModel.InputType.StrictValueList:
                    return isOtherInputActivated ? otherInputField.text : strictValueListDropdown.Title;
            }

            throw new ArgumentException($"Cannot solve value, invalid input type: {optionsModel.Options[currentOptionId].InputType}");
        }

        private void UpdateAddButtonInteractibilityByDateFormat(string str)
        {
            addButton.interactable = DateTime.TryParseExact(str,
                optionsModel.Options[currentOptionId].DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal, out DateTime _);
        }

        private void UpdateAddButtonInteractibilityByFreeFormText(string str)
        {
            addButton.interactable = !string.IsNullOrEmpty(str);
        }

        private void FillStrictValueOptions(string[] options)
        {
            valueToggles.Clear();

            foreach (string option in options)
            {
                valueToggles.Add(new ToggleComponentModel
                {
                    text = option,
                    id = option,
                    isOn = false,
                    isTextActive = true,
                    changeTextColorOnSelect = true,
                });
            }

            strictValueListDropdown.SetOptions(valueToggles);
        }

        private void SetOtherInputActive(bool isActive)
        {
            isOtherInputActivated = isActive;
            commonInputContainer.SetActive(!isActive);
            otherInputContainer.SetActive(isActive);
            otherInputField.text = string.Empty;

            if (isActive) return;
            strictValueListDropdown.SelectOption(null, false);
            strictValueListDropdown.SetTitle(DEFAULT_SELECT_OPTION_VALUE);
        }
    }
}
