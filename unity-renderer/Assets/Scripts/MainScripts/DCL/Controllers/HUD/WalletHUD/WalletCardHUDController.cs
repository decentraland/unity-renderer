using DCL.Helpers;
using System.Collections;
using UnityEngine;

namespace DCL.Wallet
{
    public class WalletCardHUDController
    {
        private readonly IWalletCardHUDComponentView view;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly ITheGraph theGraph;
        private readonly DataStore dataStore;

        private Coroutine fetchEthereumManaIntervalRoutine;
        private Coroutine fetchPolygonManaIntervalRoutine;

        private UserProfile ownUserProfile => userProfileBridge.GetOwn();

        public WalletCardHUDController(
            IWalletCardHUDComponentView view,
            IUserProfileBridge userProfileBridge,
            ITheGraph theGraph,
            DataStore dataStore)
        {
            this.view = view;
            this.userProfileBridge = userProfileBridge;
            this.theGraph = theGraph;
            this.dataStore = dataStore;

            dataStore.wallet.currentEthereumManaBalance.OnChange += OnCurrentEthereumManaBalanceChanged;
            OnCurrentEthereumManaBalanceChanged(dataStore.wallet.currentEthereumManaBalance.Get(), 0f);
            dataStore.wallet.currentPolygonManaBalance.OnChange += OnCurrentPolygonManaBalanceChanged;
            OnCurrentPolygonManaBalanceChanged(dataStore.wallet.currentPolygonManaBalance.Get(), 0f);

            dataStore.wallet.isWalletCardVisible.OnChange += OnWalletCardVisible;
        }

        public void Dispose()
        {
            dataStore.wallet.currentEthereumManaBalance.OnChange -= OnCurrentEthereumManaBalanceChanged;
            dataStore.wallet.currentPolygonManaBalance.OnChange -= OnCurrentPolygonManaBalanceChanged;
            dataStore.wallet.isWalletCardVisible.OnChange -= OnWalletCardVisible;
        }

        private void OnCurrentEthereumManaBalanceChanged(double currentBalance, double _) =>
            view.SetEthereumManaBalance(currentBalance);

        private void OnCurrentPolygonManaBalanceChanged(double currentBalance, double _) =>
            view.SetPolygonManaBalance(currentBalance);

        private void OnWalletCardVisible(bool isVisible, bool _)
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

        private IEnumerator EthereumManaIntervalRoutine()
        {
            while (true)
            {
                yield return new WaitUntil(() =>
                    ownUserProfile != null &&
                    !string.IsNullOrEmpty(ownUserProfile.userId) &&
                    !dataStore.wallet.isWalletSectionVisible.Get());

                view.SetEthereumManaLoadingActive(true);
                Promise<double> promise = theGraph.QueryEthereumMana(ownUserProfile.userId);

                if (promise != null)
                {
                    yield return promise;
                    dataStore.wallet.currentEthereumManaBalance.Set(promise.value);
                    view.SetEthereumManaLoadingActive(false);
                }

                yield return WaitForSecondsCache.Get(WalletUtils.FETCH_MANA_INTERVAL);
            }
        }

        private IEnumerator PolygonManaIntervalRoutine()
        {
            while (true)
            {
                yield return new WaitUntil(() =>
                    ownUserProfile != null &&
                    !string.IsNullOrEmpty(ownUserProfile.userId) &&
                    !dataStore.wallet.isWalletSectionVisible.Get());

                view.SetPolygonManaLoadingActive(true);
                Promise<double> promise = theGraph.QueryPolygonMana(ownUserProfile.userId);

                if (promise != null)
                {
                    yield return promise;
                    dataStore.wallet.currentPolygonManaBalance.Set(promise.value);
                    view.SetPolygonManaLoadingActive(false);
                }

                yield return WaitForSecondsCache.Get(WalletUtils.FETCH_MANA_INTERVAL);
            }
        }
    }
}
