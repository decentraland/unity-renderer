using UnityEngine;

namespace DCL.Wallet
{
    public class WalletSectionHUDController
    {
        private readonly IWalletSectionHUDComponentView view;
        private readonly DataStore dataStore;

        public WalletSectionHUDController(
            IWalletSectionHUDComponentView view,
            DataStore dataStore)
        {
            this.view = view;
            this.dataStore = dataStore;

            dataStore.exploreV2.configureWalletSectionInFullscreenMenu.OnChange += ConfigureWalletSectionInFullscreenMenuChanged;
            ConfigureWalletSectionInFullscreenMenuChanged(dataStore.exploreV2.configureWalletSectionInFullscreenMenu.Get(), null);

            dataStore.wallet.isInitialized.Set(true);
        }

        public void Dispose() =>
            dataStore.exploreV2.configureWalletSectionInFullscreenMenu.OnChange -= ConfigureWalletSectionInFullscreenMenuChanged;

        private void ConfigureWalletSectionInFullscreenMenuChanged(Transform currentParentTransform, Transform previousParentTransform) =>
            view.SetAsFullScreenMenuMode(currentParentTransform);
    }
}
