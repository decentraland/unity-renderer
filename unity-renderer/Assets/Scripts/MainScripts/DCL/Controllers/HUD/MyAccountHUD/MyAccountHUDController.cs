using UnityEngine;

namespace DCL.MyAccount
{
    public class MyAccountHUDController
    {
        private readonly IMyAccountHUDComponentView view;
        private readonly DataStore dataStore;

        public MyAccountHUDController(
            IMyAccountHUDComponentView view,
            DataStore dataStore)
        {
            this.view = view;
            this.dataStore = dataStore;

            dataStore.exploreV2.configureMyAccountSectionInFullscreenMenu.OnChange += ConfigureMyAccountSectionInFullscreenMenuChanged;
            ConfigureMyAccountSectionInFullscreenMenuChanged(dataStore.exploreV2.configureMyAccountSectionInFullscreenMenu.Get(), null);
        }

        public void Dispose()
        {
            dataStore.exploreV2.configureMyAccountSectionInFullscreenMenu.OnChange -= ConfigureMyAccountSectionInFullscreenMenuChanged;
        }

        private void ConfigureMyAccountSectionInFullscreenMenuChanged(Transform currentParentTransform, Transform _) =>
            view.SetAsFullScreenMenuMode(currentParentTransform);
    }
}
