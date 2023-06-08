using DCL.Helpers;
using System.Collections;
using UnityEngine;

namespace DCL.Wallet
{
    public class WalletCardHUDController
    {
        private const float FETCH_MANA_INTERVAL = 60;

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
        }

        public void Dispose()
        {
            dataStore.wallet.currentEthereumManaBalance.OnChange -= OnCurrentEthereumManaBalanceChanged;
            dataStore.wallet.currentPolygonManaBalance.OnChange -= OnCurrentPolygonManaBalanceChanged;
        }

        public void RefreshManaBalances()
        {
            if (fetchEthereumManaIntervalRoutine != null)
            {
                CoroutineStarter.Stop(fetchEthereumManaIntervalRoutine);
                fetchEthereumManaIntervalRoutine = null;
            }

            fetchEthereumManaIntervalRoutine = CoroutineStarter.Start(EthereumManaIntervalRoutine());

            if (fetchPolygonManaIntervalRoutine != null)
            {
                CoroutineStarter.Stop(fetchPolygonManaIntervalRoutine);
                fetchPolygonManaIntervalRoutine = null;
            }

            fetchPolygonManaIntervalRoutine = CoroutineStarter.Start(PolygonManaIntervalRoutine());
        }

        private void OnCurrentEthereumManaBalanceChanged(double currentBalance, double _) =>
            view.SetEthereumManaBalance(currentBalance);

        private void OnCurrentPolygonManaBalanceChanged(double currentBalance, double _) =>
            view.SetPolygonManaBalance(currentBalance);

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
                    dataStore.wallet.currentEthereumManaBalance.Set(promise.value);
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
                    dataStore.wallet.currentPolygonManaBalance.Set(promise.value);
                    view.SetPolygonManaBalance(promise.value);
                    view.SetPolygonManaLoadingActive(false);
                }

                yield return WaitForSecondsCache.Get(FETCH_MANA_INTERVAL);
            }
        }
    }
}
