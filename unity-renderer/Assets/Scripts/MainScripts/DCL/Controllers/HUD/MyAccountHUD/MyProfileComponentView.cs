using DCL.Helpers;
using System;
using System.Collections.Generic;
using TMPro;
using UIComponents.CollapsableSortedList;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace DCL.MyAccount
{
    public class MyProfileComponentView : BaseComponentView<MyProfileModel>, IMyProfileComponentView, IPointerClickHandler
    {
        private const float DISABLED_SECTION_ALPHA = 0.7f;
        private const string ABOUT_READ_ONLY_CONTAINER_NAME = "AboutReadOnlyContainer";

        [Header("General")]
        [SerializeField] internal GameObject mainContainer;
        [SerializeField] internal GameObject loadingContainer;
        [SerializeField] internal RectTransform contentTransform;

        [Header("Header")]
        [SerializeField] internal RectTransform headerContainerTransform;
        [SerializeField] internal CollapsableListToggleButton disclaimerButton;

        [Header("Names")]
        [SerializeField] internal RectTransform namesContainerTransform;
        [SerializeField] internal GameObject nameTypeSelectorContainer;
        [SerializeField] internal GameObject nonClaimedNameModeContainer;
        [SerializeField] internal TMP_InputField nonClaimedNameInputField;
        [SerializeField] internal GameObject nonClaimedNameEditionLogo;
        [SerializeField] internal TMP_Text nonClaimedNameAddressHashtag;
        [SerializeField] internal GameObject claimNameBanner;
        [SerializeField] internal Button claimNameButton;
        [SerializeField] internal GameObject claimedNameModeContainer;
        [SerializeField] internal DropdownComponentView claimedNameDropdown;
        [SerializeField] internal Button claimedNameGoToNonClaimedNameButton;
        [SerializeField] internal TMP_Text claimedNameGoToNonClaimedNameButtonText;
        [SerializeField] internal GameObject claimedNameGoToNonClaimedNameSelectionMark;
        [SerializeField] internal GameObject claimedNameInputContainer;
        [SerializeField] internal TMP_InputField claimedNameInputField;
        [SerializeField] internal GameObject claimedNameEditionLogo;
        [SerializeField] internal TMP_Text claimedNameAddressHashtag;
        [SerializeField] internal Button claimedNameBackToClaimedNamesListButton;
        [SerializeField] internal TMP_Text claimedNameBackToClaimedNamesListButtonText;
        [SerializeField] internal GameObject claimedNameBackToClaimedNamesListSelectionMark;
        [SerializeField] internal Button claimedNameUniqueNameButton;
        [SerializeField] internal GameObject nameValidationsContainer;
        [SerializeField] internal TMP_Text nameCharCounter;
        [SerializeField] internal GameObject nonValidNameWarning;

        [Header("About")]
        [SerializeField] internal TMP_InputField aboutInputText;
        [SerializeField] internal TMP_Text aboutCharCounter;
        [SerializeField] internal CanvasGroup aboutCanvasGroup;
        [SerializeField] internal GameObject editableAboutContainer;
        [SerializeField] internal GameObject readOnlyAboutContainer;
        [SerializeField] internal TMP_Text readOnlyAboutInputText;

        [Header("Links")]
        [SerializeField] internal RectTransform linksContainerTransform;
        [SerializeField] internal MyProfileLinkListComponentView linkListView;
        [SerializeField] internal CanvasGroup linksCanvasGroup;

        [Header("Additional Info")]
        [SerializeField] internal RectTransform additionalInfoContainerTransform;
        [SerializeField] internal MyProfileAdditionalInfoListComponentView additionalInfoList;
        [SerializeField] internal CanvasGroup additionalInfoCanvasGroup;

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
            nonClaimedNameInputField.onSelect.AddListener(_ =>
            {
                nonClaimedNameEditionLogo.SetActive(false);
                nonClaimedNameAddressHashtag.gameObject.SetActive(true);
            });
            nonClaimedNameInputField.onDeselect.AddListener(newName =>
            {
                nonClaimedNameEditionLogo.SetActive(true);
                nonClaimedNameAddressHashtag.gameObject.SetActive(false);
                OnCurrentNameSubmitted?.Invoke(newName, false);
            });
            nonClaimedNameInputField.onSubmit.AddListener(newName => OnCurrentNameSubmitted?.Invoke(newName, false));
            claimedNameInputField.onValueChanged.AddListener(newName =>
            {
                UpdateNameCharLimit(newName.Length, claimedNameInputField.characterLimit);
                OnCurrentNameEdited?.Invoke(newName);
            });
            claimedNameInputField.onSelect.AddListener(_ =>
            {
                claimedNameEditionLogo.SetActive(false);
                claimedNameAddressHashtag.gameObject.SetActive(true);
            });
            claimedNameInputField.onDeselect.AddListener(newName =>
            {
                claimedNameEditionLogo.SetActive(true);
                claimedNameAddressHashtag.gameObject.SetActive(false);
                OnCurrentNameSubmitted?.Invoke(newName, false);
            });
            claimedNameInputField.onSubmit.AddListener(newName => OnCurrentNameSubmitted?.Invoke(newName, false));
            claimedNameDropdown.OnOptionSelectionChanged += (isOn, optionId, _) =>
            {
                if (!isOn) return;
                OnCurrentNameSubmitted?.Invoke(optionId, true);
            };
            claimNameButton.onClick.AddListener(() => OnClaimNameClicked?.Invoke());
            claimedNameGoToNonClaimedNameButton.onClick.AddListener(() =>
            {
                Utils.ForceRebuildLayoutImmediate(namesContainerTransform);
                OnGoFromClaimedToNonClaimNameClicked?.Invoke();
            });
            claimedNameBackToClaimedNamesListButton.onClick.AddListener(() =>
            {
                Utils.ForceRebuildLayoutImmediate(namesContainerTransform);
                SetClaimedNameModeAsInput(false);
            });
            claimedNameUniqueNameButton.onClick.AddListener(() => OnClaimNameClicked?.Invoke());

            aboutInputText.onValueChanged.AddListener(newDesc => UpdateAboutCharLimit(newDesc.Length, aboutInputText.characterLimit));
            aboutInputText.onDeselect.AddListener(newDesc =>
            {
                readOnlyAboutInputText.text = newDesc;
                readOnlyAboutContainer.SetActive(true);
                editableAboutContainer.SetActive(false);
                OnAboutDescriptionSubmitted?.Invoke(newDesc);
            });
            aboutInputText.onSubmit.AddListener(newDesc => OnAboutDescriptionSubmitted?.Invoke(newDesc));

            linkListView.OnAddedNew += tuple =>
            {
                OnLinkAdded?.Invoke((tuple.title, tuple.url));
                Utils.ForceRebuildLayoutImmediate(linksContainerTransform);
            };
            linkListView.OnRemoved += tuple =>
            {
                OnLinkRemoved?.Invoke((tuple.title, UnityWebRequest.EscapeURL(tuple.url)));
                Utils.ForceRebuildLayoutImmediate(linksContainerTransform);
            };

            disclaimerButton.OnToggled += _ => Utils.ForceRebuildLayoutImmediate(headerContainerTransform);
            additionalInfoList.OnAdditionalFieldAdded += () => Utils.ForceRebuildLayoutImmediate(additionalInfoContainerTransform);
            additionalInfoList.OnAdditionalFieldRemoved += () => Utils.ForceRebuildLayoutImmediate(additionalInfoContainerTransform);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject.name != ABOUT_READ_ONLY_CONTAINER_NAME)
                return;

            readOnlyAboutContainer.SetActive(false);
            editableAboutContainer.SetActive(true);
            aboutInputText.Select();
        }

        public override void Dispose()
        {
            nonClaimedNameInputField.onValueChanged.RemoveAllListeners();
            nonClaimedNameInputField.onSelect.RemoveAllListeners();
            nonClaimedNameInputField.onDeselect.RemoveAllListeners();
            nonClaimedNameInputField.onSubmit.RemoveAllListeners();
            claimedNameInputField.onSelect.RemoveAllListeners();
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
            SetClaimNameBannerActive(model.ShowClaimBanner);
            SetClaimedNameModeAsInput(model.ShowInputForClaimedMode);
            SetClaimedNameDropdownOptions(model.loadedClaimedNames);
            SetAboutDescription(model.AboutDescription);
        }

        public void SetClaimedNameMode(bool isClaimed)
        {
            model.IsClaimedMode = isClaimed;
            nameTypeSelectorContainer.SetActive(isClaimed);
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
            claimedNameAddressHashtag.text = $"#{nonClaimedHashtag}";
            nonClaimedNameInputField.text = newName;
            nonClaimedNameAddressHashtag.text = $"#{nonClaimedHashtag}";
        }

        public void SetClaimNameBannerActive(bool isActive)
        {
            model.ShowClaimBanner = isActive;
            claimNameBanner.SetActive(!model.IsClaimedMode && isActive);
        }

        public void SetClaimedNameModeAsInput(bool isInput, bool cleanInputField = false)
        {
            model.ShowInputForClaimedMode = isInput;
            claimedNameInputContainer.SetActive(isInput);
            claimedNameDropdown.gameObject.SetActive(!isInput);
            nameValidationsContainer.SetActive(isInput);
            nameValidationsContainer.SetActive(!model.IsClaimedMode || isInput);

            claimedNameGoToNonClaimedNameSelectionMark.SetActive(isInput);
            claimedNameBackToClaimedNamesListSelectionMark.SetActive(!isInput);
            claimedNameGoToNonClaimedNameButtonText.fontStyle = isInput ? FontStyles.Bold : FontStyles.Normal;
            claimedNameBackToClaimedNamesListButtonText.fontStyle = isInput ? FontStyles.Normal : FontStyles.Bold;

            if (cleanInputField)
                claimedNameInputField.text = string.Empty;

            if (isInput)
            {
                if (cleanInputField)
                    claimedNameInputField.Select();

                return;
            }

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
            readOnlyAboutInputText.text = newDesc;
        }

        public void SetAboutEnabled(bool isEnabled)
        {
            aboutCanvasGroup.alpha = isEnabled ? 1f : DISABLED_SECTION_ALPHA;
            aboutCanvasGroup.interactable = isEnabled;
            aboutCanvasGroup.blocksRaycasts = isEnabled;
        }

        public void SetLoadingActive(bool isActive)
        {
            loadingContainer.SetActive(isActive);
            mainContainer.SetActive(!isActive);
        }

        public void SetNonValidNameWarningActive(bool isActive) =>
            nonValidNameWarning.SetActive(isActive);

        public void SetLinks(List<UserProfileModel.Link> links)
        {
            linkListView.Clear();

            foreach (UserProfileModel.Link link in links)
                linkListView.Add(link.title, link.url);
        }

        public void ClearLinkInput() =>
            linkListView.ClearInput();

        public void EnableOrDisableAddLinksOption(bool isEnabled) =>
            linkListView.EnableOrDisableAddNewLinkOption(isEnabled);

        public void SetLinksEnabled(bool isEnabled)
        {
            linksCanvasGroup.alpha = isEnabled ? 1f : DISABLED_SECTION_ALPHA;
            linksCanvasGroup.interactable = isEnabled;
            linksCanvasGroup.blocksRaycasts = isEnabled;
        }

        public void SetAdditionalInfoOptions(AdditionalInfoOptionsModel additionalInfoOptionsModel)
        {
            additionalInfoList.SetOptions(additionalInfoOptionsModel);
        }

        public void SetAdditionalInfoValues(Dictionary<string, (string title, string value)> values)
        {
            additionalInfoList.SetValues(values);
        }

        public void SetAdditionalInfoEnabled(bool isEnabled)
        {
            additionalInfoCanvasGroup.alpha = isEnabled ? 1f : DISABLED_SECTION_ALPHA;
            additionalInfoCanvasGroup.interactable = isEnabled;
            additionalInfoCanvasGroup.blocksRaycasts = isEnabled;
        }

        public void RefreshContentLayout() =>
            Utils.ForceRebuildLayoutImmediate(contentTransform);

        private void UpdateNameCharLimit(int currentLenght, int maxLength) =>
            nameCharCounter.text = $"{currentLenght}/{maxLength}";

        private void UpdateAboutCharLimit(int currentLenght, int maxLength) =>
            aboutCharCounter.text = $"{currentLenght}/{maxLength}";
    }
}
