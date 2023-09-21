using System;
using UnityEngine;

namespace DCL.ContentModeration
{
    public class AdultContentAgeConfirmationComponentView : BaseComponentView, IAdultContentAgeConfirmationComponentView
    {
        [SerializeField] internal ToggleComponentView confirmAgeCheckbox;
        [SerializeField] internal ButtonComponentView closeButton;
        [SerializeField] internal ButtonComponentView cancelButton;
        [SerializeField] internal ButtonComponentView confirmButton;

        public event Action OnCancelClicked;
        public event Action OnConfirmClicked;

        public override void Awake()
        {
            base.Awake();
            confirmAgeCheckbox.OnSelectedChanged += OnConfirmAgeCheckboxChanged;
            closeButton.onClick.AddListener(Cancel);
            cancelButton.onClick.AddListener(Cancel);
            confirmButton.onClick.AddListener(Confirm);
            OnConfirmAgeCheckboxChanged(confirmAgeCheckbox.isOn, null, null);
        }

        public override void Dispose()
        {
            confirmAgeCheckbox.OnSelectedChanged -= OnConfirmAgeCheckboxChanged;
            closeButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            confirmButton.onClick.RemoveAllListeners();
            base.Dispose();
        }

        public override void RefreshControl() { }

        public void ShowModal()
        {
            confirmAgeCheckbox.isOn = false;
            Show();
        }

        private void OnConfirmAgeCheckboxChanged(bool isOn, string id, string text) =>
            confirmButton.SetInteractable(isOn);

        private void Cancel()
        {
            Hide();
            OnCancelClicked?.Invoke();
        }

        private void Confirm()
        {
            Hide();
            OnConfirmClicked?.Invoke();
        }
    }
}
