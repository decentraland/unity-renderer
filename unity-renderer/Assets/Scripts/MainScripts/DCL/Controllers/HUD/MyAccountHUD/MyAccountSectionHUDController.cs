using UnityEngine;

namespace DCL.MyAccount
{
    public class MyAccountSectionHUDController
    {
        private const string NEW_TOS_AND_EMAIL_SUBSCRIPTION_FF = "new_terms_of_service_and_email_subscription";

        private readonly IMyAccountSectionHUDComponentView view;
        private readonly DataStore dataStore;

        public MyAccountSectionHUDController(
            IMyAccountSectionHUDComponentView view,
            DataStore dataStore)
        {
            this.view = view;
            this.dataStore = dataStore;

            dataStore.exploreV2.configureMyAccountSectionInFullscreenMenu.OnChange += ConfigureMyAccountSectionInFullscreenMenuChanged;
            ConfigureMyAccountSectionInFullscreenMenuChanged(dataStore.exploreV2.configureMyAccountSectionInFullscreenMenu.Get(), null);

            dataStore.myAccount.isInitialized.Set(true);
        }

        public void Dispose()
        {
            dataStore.exploreV2.configureMyAccountSectionInFullscreenMenu.OnChange -= ConfigureMyAccountSectionInFullscreenMenuChanged;
        }

        public void ShowAccountSettingsUpdatedToast() =>
            view.ShowAccountSettingsUpdatedToast();

        private void ConfigureMyAccountSectionInFullscreenMenuChanged(Transform currentParentTransform, Transform _)
        {
            view.SetAsFullScreenMenuMode(currentParentTransform);
            view.SetSectionsMenuActive(dataStore.featureFlags.flags.Get().IsFeatureEnabled(NEW_TOS_AND_EMAIL_SUBSCRIPTION_FF));
        }
    }
}
