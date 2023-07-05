using DCL.Browser;
using DCL.SettingsCommon;
using System;

namespace DCL.MyAccount
{
    public class MyAccountCardController
    {
        private const string OPEN_PASSPORT_SOURCE = "ProfileHUD";
        private const string URL_TERMS_OF_USE = "https://decentraland.org/terms";
        private const string URL_PRIVACY_POLICY = "https://decentraland.org/privacy";

        private readonly IMyAccountCardComponentView view;
        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly Settings settings;
        private readonly IBrowserBridge browserBridge;

        public MyAccountCardController(
            IMyAccountCardComponentView view,
            DataStore dataStore,
            IUserProfileBridge userProfileBridge,
            Settings settings,
            IBrowserBridge browserBridge)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.settings = settings;
            this.browserBridge = browserBridge;

            view.OnPreviewProfileClicked += OnPreviewProfileClicked;
            view.OnAccountSettingsClicked += OnAccountSettingsClicked;
            view.OnSignOutClicked += OnSignOutClicked;
            view.OnTermsOfServiceClicked += OnTermsOfServiceClicked;
            view.OnPrivacyPolicyClicked += OnPrivacyPolicyClicked;

            userProfileBridge.GetOwn().OnUpdate += OnOwnUserProfileUpdate;
            bool isProfileInitialized = !string.IsNullOrEmpty(userProfileBridge.GetOwn().userId);
            if (isProfileInitialized)
                OnOwnUserProfileUpdate(userProfileBridge.GetOwn());
        }

        public void Dispose()
        {
            view.OnPreviewProfileClicked -= OnPreviewProfileClicked;
            view.OnAccountSettingsClicked -= OnAccountSettingsClicked;
            view.OnSignOutClicked -= OnSignOutClicked;
            view.OnTermsOfServiceClicked -= OnTermsOfServiceClicked;
            view.OnPrivacyPolicyClicked -= OnPrivacyPolicyClicked;
            userProfileBridge.GetOwn().OnUpdate -= OnOwnUserProfileUpdate;
        }

        private void OnOwnUserProfileUpdate(UserProfile userProfile)
        {
            if (userProfile == null)
                return;

            view.SetSignOutButtonActive(!userProfile.isGuest);
        }

        private void OnPreviewProfileClicked()
        {
            dataStore.HUDs.currentPlayerId.Set((userProfileBridge.GetOwn().userId, OPEN_PASSPORT_SOURCE));
            dataStore.exploreV2.profileCardIsOpen.Set(false);
            view.Hide();
        }

        private void OnAccountSettingsClicked() =>
            dataStore.myAccount.myAccountSectionOpenFromProfileHUD.Set(true, true);

        private void OnSignOutClicked()
        {
            settings?.SaveSettings();
            userProfileBridge.LogOut();
        }

        private void OnTermsOfServiceClicked() =>
            browserBridge.OpenUrl(URL_TERMS_OF_USE);

        private void OnPrivacyPolicyClicked() =>
            browserBridge.OpenUrl(URL_PRIVACY_POLICY);
    }
}
