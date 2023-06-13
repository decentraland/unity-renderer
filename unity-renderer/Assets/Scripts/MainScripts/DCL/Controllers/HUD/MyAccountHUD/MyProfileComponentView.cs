using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.MyAccount
{
    public class MyProfileComponentView : BaseComponentView<MyProfileModel>, IMyProfileComponentView
    {
        [Header("MyProfile")]
        [SerializeField] internal GameObject nonClaimedNameModeContainer;
        [SerializeField] internal TMP_InputField nonClaimedNameInputField;
        [SerializeField] internal GameObject claimNameBanner;
        [SerializeField] internal Button claimNameButton;
        [SerializeField] internal GameObject claimedNameModeContainer;
        [SerializeField] internal DropdownComponentView claimedNameDropdown;
        [SerializeField] internal GameObject claimedNameInputContainer;
        [SerializeField] internal TMP_InputField claimedNameInputField;
        [SerializeField] internal Button claimedNameUniqueNameButton;

        public event Action<string, bool> OnCurrentNameChanged;
        public event Action OnClaimNameClicked;

        public override void Awake()
        {
            base.Awake();

            nonClaimedNameInputField.onDeselect.AddListener(newName => OnCurrentNameChanged?.Invoke(newName, false));
            claimedNameInputField.onDeselect.AddListener(newName => OnCurrentNameChanged?.Invoke(newName, true));
            claimedNameDropdown.OnOptionSelectionChanged += (isOn, optionId, _) =>
            {
                if (!isOn) return;
                OnCurrentNameChanged?.Invoke(optionId, true);
            };
            claimNameButton.onClick.AddListener(() => OnClaimNameClicked?.Invoke());
            claimedNameUniqueNameButton.onClick.AddListener(() => OnClaimNameClicked?.Invoke());
        }

        public override void Dispose()
        {
            nonClaimedNameInputField.onDeselect.RemoveAllListeners();
            claimedNameInputField.onDeselect.RemoveAllListeners();
            claimNameButton.onClick.RemoveAllListeners();
            claimedNameUniqueNameButton.onClick.RemoveAllListeners();

            base.Dispose();
        }

        public override void RefreshControl()
        {
            SetClaimedNameMode(model.IsClaimedMode);
            SetCurrentName(model.CurrentName);
            SetClaimBannerActive(model.ShowClaimBanner);
            SetClaimedModeAsInput(model.ShowInputForClaimedMode);
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
        }
    }
}
