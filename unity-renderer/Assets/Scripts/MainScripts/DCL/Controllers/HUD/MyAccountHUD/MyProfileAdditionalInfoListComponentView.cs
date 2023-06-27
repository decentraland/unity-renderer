using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.MyAccount
{
    public class MyProfileAdditionalInfoListComponentView : MonoBehaviour
    {
        [SerializeField] private Button addButton;
        [SerializeField] private DropdownComponentView optionsDropdown;
        [SerializeField] private TMP_InputField freeFormInputField;
        [SerializeField] private TMP_InputField dateInputField;
        [SerializeField] private DropdownComponentView strictValueListDropdown;
        [SerializeField] private MyProfileAdditionalInfoEntryComponentView entryPrefab;
        [SerializeField] private RectTransform entryContainer;

        private readonly Dictionary<string, MyProfileAdditionalInfoEntryComponentView> entries = new ();

        private string currentOptionId;
        private AdditionalInfoOptionsModel optionsModel;

        private void Awake()
        {
            optionsDropdown.OnOptionSelectionChanged += (isOn, optionId, optionName) =>
            {
                if (!isOn) return;

                ChangeCurrentOption(optionId, optionName);
            };

            dateInputField.onValueChanged.AddListener(UpdateAddButtonInteractabilityByDateFormat);
            freeFormInputField.onValueChanged.AddListener(UpdateAddButtonInteractabilityByFreeFormText);

            strictValueListDropdown.OnOptionSelectionChanged += (isOn, optionId, optionName) =>
            {
                if (!isOn) return;
                strictValueListDropdown.SetTitle(optionName);
                UpdateAddButtonInteractabilityByFreeFormText(optionName);
            };

            addButton.onClick.AddListener(() =>
            {
                if (string.IsNullOrEmpty(currentOptionId)) return;
                optionsModel.Options[currentOptionId].OnValueSubmitted.Invoke(GetCurrentValue());
            });
        }

        public void SetOptions(AdditionalInfoOptionsModel model)
        {
            optionsModel = model;

            optionsDropdown.SetOptions(model.Options.Select(pair => new ToggleComponentModel
                                             {
                                                 text = pair.Value.Name,
                                                 id = pair.Key,
                                                 isOn = false,
                                                 isTextActive = true,
                                                 changeTextColorOnSelect = true,
                                             })
                                            .ToList());

            if (model.Options.Count > 0)
            {
                KeyValuePair<string, AdditionalInfoOptionsModel.Option> firstOption = model.Options.First();
                ChangeCurrentOption(firstOption.Key, firstOption.Value.Name);
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
                    strictValueListDropdown.SetTitle("");
                    break;
            }

            optionsDropdown.SetTitle(optionName);
            dateInputField.text = "";
            freeFormInputField.text = "";
            addButton.interactable = false;
        }

        private void OnInfoRemoved(string optionId) =>
            optionsModel.Options[optionId].OnRemoved.Invoke();

        private string GetCurrentValue()
        {
            switch (optionsModel.Options[currentOptionId].InputType)
            {
                case AdditionalInfoOptionsModel.InputType.Date:
                    return dateInputField.text;
                case AdditionalInfoOptionsModel.InputType.FreeFormText:
                    return freeFormInputField.text;
                case AdditionalInfoOptionsModel.InputType.StrictValueList:
                    return strictValueListDropdown.Title;
            }

            throw new ArgumentException($"Cannot solve value, invalid input type: {optionsModel.Options[currentOptionId].InputType}");
        }

        private void UpdateAddButtonInteractabilityByDateFormat(string str)
        {
            addButton.interactable = DateTime.TryParseExact(str,
                optionsModel.Options[currentOptionId].DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime result);
        }

        private void UpdateAddButtonInteractabilityByFreeFormText(string str)
        {
            addButton.interactable = !string.IsNullOrEmpty(str);
        }

        private void FillStrictValueOptions(string[] options)
        {
            strictValueListDropdown.SetOptions(options
                                              .Select(option => new ToggleComponentModel
                                               {
                                                   text = option,
                                                   id = option,
                                                   isOn = false,
                                                   isTextActive = true,
                                                   changeTextColorOnSelect = true,
                                               })
                                              .ToList());
        }
    }
}
