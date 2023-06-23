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
        [Header("General")]
        [SerializeField] internal GameObject mainContainer;
        [SerializeField] internal GameObject loadingContainer;

        [Header("Names")]
        [SerializeField] internal GameObject nonClaimedNameModeContainer;
        [SerializeField] internal TMP_InputField nonClaimedNameInputField;
        [SerializeField] internal GameObject claimNameBanner;
        [SerializeField] internal Button claimNameButton;
        [SerializeField] internal GameObject claimedNameModeContainer;
        [SerializeField] internal DropdownComponentView claimedNameDropdown;
        [SerializeField] internal Button claimedNameGoToNonClaimedNameButton;
        [SerializeField] internal GameObject claimedNameInputContainer;
        [SerializeField] internal TMP_InputField claimedNameInputField;
        [SerializeField] internal Button claimedNameBackToClaimedNamesListButton;
        [SerializeField] internal Button claimedNameUniqueNameButton;
        [SerializeField] internal GameObject nameValidationsContainer;
        [SerializeField] internal TMP_Text nameCharCounter;
        [SerializeField] internal GameObject nonValidNameWarning;
        [SerializeField] internal MyProfileLinkListComponentView linkListView;

        [Header("About")]
        [SerializeField] internal TMP_InputField aboutInputText;
        [SerializeField] internal TMP_Text aboutCharCounter;

        public event Action<string> OnCurrentNameEdited;
        public event Action<string, bool> OnCurrentNameSubmitted;
        public event Action OnGoFromClaimedToNonClaimNameClicked;
        public event Action OnClaimNameClicked;
        public event Action<string> OnAboutDescriptionSubmitted;
        public event Action<(string title, string url)> OnLinkAdded;
        public event Action<(string title, string url)> OnLinkRemoved;

        public override void Awake()
        {
            base.Awake();

            UpdateNameCharLimit(0, nonClaimedNameInputField.characterLimit);
            UpdateAboutCharLimit(0, aboutInputText.characterLimit);

            nonClaimedNameInputField.onValueChanged.AddListener(newName =>
            {
                UpdateNameCharLimit(newName.Length, nonClaimedNameInputField.characterLimit);
                OnCurrentNameEdited?.Invoke(newName);
            });
            nonClaimedNameInputField.onDeselect.AddListener(newName => OnCurrentNameSubmitted?.Invoke(newName, false));
            nonClaimedNameInputField.onSubmit.AddListener(newName => OnCurrentNameSubmitted?.Invoke(newName, false));
            claimedNameInputField.onValueChanged.AddListener(newName =>
            {
                UpdateNameCharLimit(newName.Length, claimedNameInputField.characterLimit);
                OnCurrentNameEdited?.Invoke(newName);
            });
            claimedNameInputField.onDeselect.AddListener(newName => OnCurrentNameSubmitted?.Invoke(newName, false));
            claimedNameInputField.onSubmit.AddListener(newName => OnCurrentNameSubmitted?.Invoke(newName, false));
            claimedNameDropdown.OnOptionSelectionChanged += (isOn, optionId, _) =>
            {
                if (!isOn) return;
                OnCurrentNameSubmitted?.Invoke(optionId, true);
            };
            claimNameButton.onClick.AddListener(() => OnClaimNameClicked?.Invoke());
            claimedNameGoToNonClaimedNameButton.onClick.AddListener(() => OnGoFromClaimedToNonClaimNameClicked?.Invoke());
            claimedNameBackToClaimedNamesListButton.onClick.AddListener(() => SetClaimedModeAsInput(false));
            claimedNameUniqueNameButton.onClick.AddListener(() => OnClaimNameClicked?.Invoke());

            aboutInputText.onValueChanged.AddListener(newDesc => UpdateAboutCharLimit(newDesc.Length, aboutInputText.characterLimit));
            aboutInputText.onDeselect.AddListener(newDesc => OnAboutDescriptionSubmitted?.Invoke(newDesc));
            aboutInputText.onSubmit.AddListener(newDesc => OnAboutDescriptionSubmitted?.Invoke(newDesc));

            linkListView.OnAddedNew += tuple => OnLinkAdded?.Invoke((tuple.title, tuple.url));
            linkListView.OnRemoved += tuple => OnLinkRemoved?.Invoke((tuple.title, tuple.url));
        }

        public override void Dispose()
        {
            nonClaimedNameInputField.onValueChanged.RemoveAllListeners();
            nonClaimedNameInputField.onDeselect.RemoveAllListeners();
            nonClaimedNameInputField.onSubmit.RemoveAllListeners();
            claimedNameInputField.onDeselect.RemoveAllListeners();
            claimedNameInputField.onSubmit.RemoveAllListeners();
            claimNameButton.onClick.RemoveAllListeners();
            claimedNameGoToNonClaimedNameButton.onClick.RemoveAllListeners();
            claimedNameBackToClaimedNamesListButton.onClick.RemoveAllListeners();
            claimedNameUniqueNameButton.onClick.RemoveAllListeners();

            base.Dispose();
        }

        public override void RefreshControl()
        {
            SetClaimedNameMode(model.IsClaimedMode);
            SetCurrentName(model.MainName, model.NonClaimedHashtag);
            SetClaimBannerActive(model.ShowClaimBanner);
            SetClaimedModeAsInput(model.ShowInputForClaimedMode);
            SetClaimedNameDropdownOptions(model.loadedClaimedNames);
            SetAboutDescription(model.AboutDescription);
        }

        public void SetClaimedNameMode(bool isClaimed)
        {
            model.IsClaimedMode = isClaimed;
            nonClaimedNameModeContainer.SetActive(!isClaimed);
            claimedNameModeContainer.SetActive(isClaimed);
            nameValidationsContainer.SetActive(!isClaimed || model.ShowInputForClaimedMode);
        }

        public void SetCurrentName(string newName, string nonClaimedHashtag)
        {
            model.MainName = newName;
            model.NonClaimedHashtag = nonClaimedHashtag;

            if (model.IsClaimedMode)
            {
                claimedNameDropdown.SelectOption(newName, false);
                claimedNameDropdown.SetTitle(newName);
            }

            claimedNameInputField.text = newName;
            nonClaimedNameInputField.text = newName;
        }

        public void SetClaimBannerActive(bool isActive)
        {
            model.ShowClaimBanner = isActive;
            claimNameBanner.SetActive(!model.IsClaimedMode && isActive);
        }

        public void SetClaimedModeAsInput(bool isInput, bool cleanInputField = false)
        {
            model.ShowInputForClaimedMode = isInput;
            claimedNameInputContainer.SetActive(isInput);
            claimedNameDropdown.gameObject.SetActive(!isInput);
            claimedNameGoToNonClaimedNameButton.gameObject.SetActive(!isInput);
            nameValidationsContainer.SetActive(isInput);
            nameValidationsContainer.SetActive(!model.IsClaimedMode || isInput);

            if (cleanInputField)
                claimedNameInputField.text = string.Empty;

            if (isInput)
                return;

            if (model.loadedClaimedNames.Contains(model.MainName))
            {
                claimedNameDropdown.SelectOption(model.MainName, false);
                claimedNameDropdown.SetTitle(model.MainName);
            }
            else
            {
                claimedNameDropdown.SelectOption(
                    model.loadedClaimedNames.Count == 0 ? string.Empty : model.loadedClaimedNames[0],
                    model.loadedClaimedNames.Count != 0);
                claimedNameDropdown.SetTitle(model.loadedClaimedNames.Count == 0 ? string.Empty : model.loadedClaimedNames[0]);
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

        public void SetAboutDescription(string newDesc)
        {
            model.AboutDescription = newDesc;
            aboutInputText.text = newDesc;
        }

        public void SetLoadingActive(bool isActive)
        {
            loadingContainer.SetActive(isActive);
            mainContainer.SetActive(!isActive);
        }

        public void SetNonValidNameWarningActive(bool isActive) =>
            nonValidNameWarning.SetActive(isActive);

        public void SetLinks(List<(string title, string url)> links)
        {
            linkListView.Clear();

            foreach ((string title, string url) link in links)
                linkListView.Add(link.title, link.url);
        }

        public void ClearLinkInput() =>
            linkListView.ClearInput();

        public void EnableOrDisableAddLinksOption(bool enabled) =>
            linkListView.EnableOrDisableAddNewLinkOption(enabled);

        private void UpdateNameCharLimit(int currentLenght, int maxLength) =>
            nameCharCounter.text = $"{currentLenght}/{maxLength}";

        private void UpdateAboutCharLimit(int currentLenght, int maxLength) =>
            aboutCharCounter.text = $"{currentLenght}/{maxLength}";
    }
}
