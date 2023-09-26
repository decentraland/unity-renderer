using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.ContentModeration
{
    public class AdultContentSceneWarningComponentView : BaseComponentView, IAdultContentSceneWarningComponentView
    {
        [SerializeField] internal Button backgroundButton;
        [SerializeField] internal ButtonComponentView closeButton;
        [SerializeField] internal ButtonComponentView cancelButton;
        [SerializeField] internal ButtonComponentView goToSettingsButton;

        public event Action OnGoToSettingsClicked;

        public override void Awake()
        {
            base.Awake();
            backgroundButton.onClick.AddListener(HideModal);
            closeButton.onClick.AddListener(HideModal);
            cancelButton.onClick.AddListener(HideModal);
            goToSettingsButton.onClick.AddListener(GoToSettings);
        }

        public override void Dispose()
        {
            backgroundButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            goToSettingsButton.onClick.RemoveAllListeners();
            base.Dispose();
        }

        public override void RefreshControl() { }

        public void ShowModal() =>
            Show();

        public void HideModal() =>
            Hide();

        private void GoToSettings() =>
            OnGoToSettingsClicked?.Invoke();
    }
}
