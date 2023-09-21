using System;
using UnityEngine;

namespace DCL.ContentModeration
{
    public class AdultContentSceneWarningComponentView : BaseComponentView, IAdultContentSceneWarningComponentView
    {
        [SerializeField] internal ButtonComponentView closeButton;
        [SerializeField] internal ButtonComponentView learnMoreButton;
        [SerializeField] internal ButtonComponentView goToSettingsButton;

        public event Action OnLearnMoreClicked;
        public event Action OnGoToSettingsClicked;

        public override void Awake()
        {
            base.Awake();
            closeButton.onClick.AddListener(() => Hide());
            learnMoreButton.onClick.AddListener(() => OnLearnMoreClicked?.Invoke());
            goToSettingsButton.onClick.AddListener(GoToSettings);
        }

        public override void Dispose()
        {
            closeButton.onClick.RemoveAllListeners();
            learnMoreButton.onClick.RemoveAllListeners();
            goToSettingsButton.onClick.RemoveAllListeners();
            base.Dispose();
        }

        public override void RefreshControl() { }

        public void ShowModal() =>
            Show();

        public void HideModal() =>
            Hide();

        private void GoToSettings()
        {
            Hide();
            OnGoToSettingsClicked?.Invoke();
        }
    }
}
