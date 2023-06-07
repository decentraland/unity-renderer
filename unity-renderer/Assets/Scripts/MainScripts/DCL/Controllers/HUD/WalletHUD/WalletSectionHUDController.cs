using DCL.Browser;
using DCL.Helpers;
using System.Collections;
using UnityEngine;

namespace DCL.Wallet
{
    public class WalletSectionHUDController
    {
        private const string URL_MANA_INFO = "https://docs.decentraland.org/examples/get-a-wallet";
        private const string URL_MANA_PURCHASE = "https://account.decentraland.org";
        private const float FETCH_MANA_INTERVAL = 60;

        private readonly IWalletSectionHUDComponentView view;
        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IClipboard clipboard;
        private readonly IBrowserBridge browserBridge;
        private readonly ITheGraph theGraph;

        private Coroutine fetchEthereumManaIntervalRoutine;
        private Coroutine fetchPolygonManaIntervalRoutine;

        private UserProfile ownUserProfile => userProfileBridge.GetOwn();

        public WalletSectionHUDController(
            IWalletSectionHUDComponentView view,
            DataStore dataStore,
            IUserProfileBridge userProfileBridge,
            IClipboard clipboard,
            IBrowserBridge browserBridge,
            ITheGraph theGraph)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.clipboard = clipboard;
            this.browserBridge = browserBridge;
            this.theGraph = theGraph;

            dataStore.exploreV2.configureWalletSectionInFullscreenMenu.OnChange += ConfigureWalletSectionInFullscreenMenuChanged;
            ConfigureWalletSectionInFullscreenMenuChanged(dataStore.exploreV2.configureWalletSectionInFullscreenMenu.Get(), null);

            dataStore.wallet.isInitialized.Set(true);
            dataStore.wallet.isWalletSectionVisible.OnChange += OnWalletSectionVisible;

            ownUserProfile.OnUpdate += OnProfileUpdated;
            OnProfileUpdated(ownUserProfile);

            view.OnCopyWalletAddress += CopyWalletAddress;
            view.OnBuyManaClicked += GoToManaPurchaseUrl;
            view.OnLearnMoreClicked += GoToLearnMoreUrl;
        }

        public void Dispose()
        {
            dataStore.exploreV2.configureWalletSectionInFullscreenMenu.OnChange -= ConfigureWalletSectionInFullscreenMenuChanged;
            dataStore.wallet.isWalletSectionVisible.OnChange -= OnWalletSectionVisible;
            ownUserProfile.OnUpdate -= OnProfileUpdated;
            view.OnBuyManaClicked -= GoToManaPurchaseUrl;
            view.OnLearnMoreClicked -= GoToLearnMoreUrl;
        }

        private void ConfigureWalletSectionInFullscreenMenuChanged(Transform currentParentTransform, Transform previousParentTransform) =>
            view.SetAsFullScreenMenuMode(currentParentTransform);

        private void OnWalletSectionVisible(bool isVisible, bool _)
        {
            switch (isVisible)
            {
                case true when fetchEthereumManaIntervalRoutine == null:
                    fetchEthereumManaIntervalRoutine = CoroutineStarter.Start(EthereumManaIntervalRoutine());
                    break;
                case false when fetchEthereumManaIntervalRoutine != null:
                    CoroutineStarter.Stop(fetchEthereumManaIntervalRoutine);
                    fetchEthereumManaIntervalRoutine = null;
                    break;
            }

            switch (isVisible)
            {
                case true when fetchPolygonManaIntervalRoutine == null:
                    fetchPolygonManaIntervalRoutine = CoroutineStarter.Start(PolygonManaIntervalRoutine());
                    break;
                case false when fetchPolygonManaIntervalRoutine != null:
                    CoroutineStarter.Stop(fetchPolygonManaIntervalRoutine);
                    fetchPolygonManaIntervalRoutine = null;
                    break;
            }
        }

        private void OnProfileUpdated(UserProfile userProfile)
        {
            if (string.IsNullOrEmpty(userProfile.userId))
                return;

            view.SetWalletAddress(userProfile.userId);
        }

        private void CopyWalletAddress()
        {
            if (string.IsNullOrEmpty(ownUserProfile.userId))
                return;

            clipboard.WriteText(ownUserProfile.userId);
        }

        private void GoToManaPurchaseUrl() =>
            browserBridge.OpenUrl(URL_MANA_PURCHASE);

        private void GoToLearnMoreUrl() =>
            browserBridge.OpenUrl(URL_MANA_INFO);

        private IEnumerator EthereumManaIntervalRoutine()
        {
            while (true)
            {
                yield return new WaitUntil(() => ownUserProfile != null && !string.IsNullOrEmpty(ownUserProfile.userId));

                view.SetEthereumManaLoadingActive(true);
                Promise<double> promise = theGraph.QueryEthereumMana(ownUserProfile.userId);

                if (promise != null)
                {
                    yield return promise;
                    view.SetEthereumManaBalance(promise.value);
                    view.SetEthereumManaLoadingActive(false);
                }

                yield return WaitForSecondsCache.Get(FETCH_MANA_INTERVAL);
            }
        }

        private IEnumerator PolygonManaIntervalRoutine()
        {
            while (true)
            {
                yield return new WaitUntil(() => ownUserProfile != null && !string.IsNullOrEmpty(ownUserProfile.userId));

                view.SetPolygonManaLoadingActive(true);
                Promise<double> promise = theGraph.QueryPolygonMana(ownUserProfile.userId);

                if (promise != null)
                {
                    yield return promise;
                    view.SetPolygonManaBalance(promise.value);
                    view.SetPolygonManaLoadingActive(false);
                }

                yield return WaitForSecondsCache.Get(FETCH_MANA_INTERVAL);
            }
        }
    }
}
