using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.ContentModeration
{
    public class AdultContentSceneWarningComponentView : BaseComponentView, IAdultContentSceneWarningComponentView
    {
        [SerializeField] internal Button backgroundButton;
        [SerializeField] internal ButtonComponentView gotItButton;
        [SerializeField] internal ButtonComponentView cancelButton;
        [SerializeField] internal ButtonComponentView goToSettingsButton;
        [SerializeField] internal TMP_Text subTitle;

        private const string RESTRICTED_MODE_SUBTITLE = "This scene has been restricted by our community as it potentially contains prohibited content for the users.";
        private const string ADULT_MODE_SUBTITLE = @"This scene has been flagged by the community as containing 18+ Adult content that may be inappropriate for some users.\n\nTo see this and other Adult-rated scenes, please confirm your choice under 'Adult Content' in the General Settings.";

        public event Action OnGoToSettingsClicked;

        public override void Awake()
        {
            base.Awake();
            backgroundButton.onClick.AddListener(HideModal);
            cancelButton.onClick.AddListener(HideModal);
            gotItButton.onClick.AddListener(HideModal);
            goToSettingsButton.onClick.AddListener(GoToSettings);
        }

        public override void Dispose()
        {
            backgroundButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            gotItButton.onClick.RemoveAllListeners();
            goToSettingsButton.onClick.RemoveAllListeners();
            base.Dispose();
        }

        public override void RefreshControl() { }

        public void ShowModal() =>
            Show();

        public void HideModal() =>
            Hide();

        public void SetRestrictedMode(bool isRestricted)
        {
            gotItButton.gameObject.SetActive(isRestricted);
            cancelButton.gameObject.SetActive(!isRestricted);
            goToSettingsButton.gameObject.SetActive(!isRestricted);
            subTitle.text = isRestricted ? RESTRICTED_MODE_SUBTITLE : ADULT_MODE_SUBTITLE;
        }

        private void GoToSettings() =>
            OnGoToSettingsClicked?.Invoke();
    }
}
