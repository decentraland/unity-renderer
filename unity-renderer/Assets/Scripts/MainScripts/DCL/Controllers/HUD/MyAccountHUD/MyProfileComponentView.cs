using System;
using System.Collections.Generic;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.MyAccount
{
    public class MyProfileComponentView : BaseComponentView<MyProfileModel>, IMyProfileComponentView
    {
        [Header("Names")]
        [SerializeField] internal GameObject nonClaimedNameModeContainer;
        [SerializeField] internal TMP_InputField nonClaimedNameInputField;
        [SerializeField] internal GameObject claimNameBanner;
        [SerializeField] internal Button claimNameButton;
        [SerializeField] internal GameObject claimedNameModeContainer;
        [SerializeField] internal DropdownComponentView claimedNameDropdown;
        [SerializeField] internal GameObject claimedNameInputContainer;
        [SerializeField] internal TMP_InputField claimedNameInputField;
        [SerializeField] internal Button claimedNameBackToClaimedNamesListButton;
        [SerializeField] internal Button claimedNameUniqueNameButton;
        [SerializeField] internal GameObject mainContainer;
        [SerializeField] internal GameObject loadingContainer;

        public event Action<string> OnCurrentNameEdited;
        public event Action<string, bool> OnCurrentNameSubmitted;
        public event Action OnClaimNameClicked;

        public override void Awake()
        {
            base.Awake();

            nonClaimedNameInputField.onValueChanged.AddListener(newName => OnCurrentNameEdited?.Invoke(newName));
            nonClaimedNameInputField.onDeselect.AddListener(newName => OnCurrentNameSubmitted?.Invoke(newName, false));
            nonClaimedNameInputField.onSubmit.AddListener(newName => OnCurrentNameSubmitted?.Invoke(newName, false));
            claimedNameInputField.onValueChanged.AddListener(newName => OnCurrentNameEdited?.Invoke(newName));
            claimedNameInputField.onDeselect.AddListener(newName => OnCurrentNameSubmitted?.Invoke(newName, false));
            claimedNameInputField.onSubmit.AddListener(newName => OnCurrentNameSubmitted?.Invoke(newName, false));
            claimedNameDropdown.OnOptionSelectionChanged += (isOn, optionId, _) =>
            {
                if (!isOn) return;
                OnCurrentNameSubmitted?.Invoke(optionId, true);
            };
            claimNameButton.onClick.AddListener(() => OnClaimNameClicked?.Invoke());
            claimedNameBackToClaimedNamesListButton.onClick.AddListener(() => SetClaimedModeAsInput(false));
            claimedNameUniqueNameButton.onClick.AddListener(() => OnClaimNameClicked?.Invoke());
        }

        public override void Dispose()
        {
            nonClaimedNameInputField.onValueChanged.RemoveAllListeners();
            nonClaimedNameInputField.onDeselect.RemoveAllListeners();
            nonClaimedNameInputField.onSubmit.RemoveAllListeners();
            claimedNameInputField.onDeselect.RemoveAllListeners();
            claimedNameInputField.onSubmit.RemoveAllListeners();
            claimNameButton.onClick.RemoveAllListeners();
            claimedNameBackToClaimedNamesListButton.onClick.RemoveAllListeners();
            claimedNameUniqueNameButton.onClick.RemoveAllListeners();

            base.Dispose();
        }

        public override void RefreshControl()
        {
            SetClaimedNameMode(model.IsClaimedMode);
            SetCurrentName(model.CurrentName);
            SetClaimBannerActive(model.ShowClaimBanner);
            SetClaimedModeAsInput(model.ShowInputForClaimedMode);
            SetClaimedNameDropdownOptions(model.loadedClaimedNames);
        }

        public void SetClaimedNameMode(bool isClaimed)
        {
            model.IsClaimedMode = isClaimed;
            nonClaimedNameModeContainer.SetActive(!isClaimed);
            claimedNameModeContainer.SetActive(isClaimed);
        }

        public void SetCurrentName(string newName)
        {
            model.CurrentName = newName;

            if (model.IsClaimedMode)
            {
                claimedNameDropdown.SelectOption(newName, false);
                claimedNameDropdown.SetTitle(newName);
            }
            else
            {
                claimedNameInputField.text = newName;
                nonClaimedNameInputField.text = newName;
            }
        }

        public void SetClaimBannerActive(bool isActive)
        {
            model.ShowClaimBanner = isActive;
            claimNameBanner.SetActive(isActive);
        }

        public void SetClaimedModeAsInput(bool isInput)
        {
            claimedNameInputContainer.SetActive(isInput);
            claimedNameDropdown.gameObject.SetActive(!isInput);

            if (isInput)
                return;

            if (model.loadedClaimedNames.Contains(model.CurrentName))
            {
                claimedNameDropdown.SelectOption(model.CurrentName, false);
                claimedNameDropdown.SetTitle(model.CurrentName);
            }
            else
            {
                claimedNameDropdown.SelectOption(string.Empty, false);
                claimedNameDropdown.SetTitle(string.Empty);
            }
        }

        public void SetClaimedNameDropdownOptions(List<string> claimedNamesList)
        {
            model.loadedClaimedNames.Clear();
            model.loadedClaimedNames.AddRange(claimedNamesList);

            List<ToggleComponentModel> collectionsToAdd = new ();

            foreach (string claimedName in claimedNamesList)
            {
                ToggleComponentModel newCollectionModel = new ToggleComponentModel
                {
                    id = claimedName,
                    text = claimedName,
                    isOn = false,
                    isTextActive = true,
                    changeTextColorOnSelect = true,
                };

                collectionsToAdd.Add(newCollectionModel);
            }

            if (collectionsToAdd.Count > 0)
                claimedNameDropdown.SetTitle(collectionsToAdd[0].text);

            claimedNameDropdown.SetOptions(collectionsToAdd);
        }

        public void SetLoadingActive(bool isActive)
        {
            loadingContainer.SetActive(isActive);
            mainContainer.SetActive(!isActive);
        }
    }
}
