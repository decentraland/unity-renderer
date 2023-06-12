﻿using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Tasks;
using System;
using System.Threading;

namespace DCL.Wallet
{
    public class WalletCardHUDController
    {
        private readonly IWalletCardHUDComponentView view;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly ITheGraph theGraph;
        private readonly DataStore dataStore;

        private CancellationTokenSource fetchEthereumManaCancellationToken;
        private CancellationTokenSource fetchPolygonManaCancellationToken;

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

            fetchEthereumManaCancellationToken.SafeCancelAndDispose();
            fetchPolygonManaCancellationToken.SafeCancelAndDispose();
        }

        private void OnCurrentEthereumManaBalanceChanged(double currentBalance, double _) =>
            view.SetEthereumManaBalance(currentBalance);

        private void OnCurrentPolygonManaBalanceChanged(double currentBalance, double _) =>
            view.SetPolygonManaBalance(currentBalance);

        private void OnWalletCardVisible(bool isVisible, bool _)
        {
            if (ownUserProfile.isGuest)
                return;

            if (isVisible)
            {
                fetchEthereumManaCancellationToken = fetchEthereumManaCancellationToken.SafeRestart();
                fetchPolygonManaCancellationToken = fetchPolygonManaCancellationToken.SafeRestart();
                RequestEthereumManaAsync(fetchEthereumManaCancellationToken.Token).Forget();
                RequestPolygonManaAsync(fetchPolygonManaCancellationToken.Token).Forget();
            }
            else
            {
                fetchEthereumManaCancellationToken.SafeCancelAndDispose();
                fetchPolygonManaCancellationToken.SafeCancelAndDispose();
            }
        }

        private async UniTask RequestEthereumManaAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                await UniTask.WaitUntil(() =>
                    ownUserProfile != null &&
                    !string.IsNullOrEmpty(ownUserProfile.userId) &&
                    !dataStore.wallet.isWalletSectionVisible.Get(),
                    cancellationToken: cancellationToken);

                view.SetEthereumManaLoadingActive(true);
                Promise<double> promise = theGraph.QueryEthereumMana(ownUserProfile.userId);
                if (promise != null)
                {
                    await promise;
                    dataStore.wallet.currentEthereumManaBalance.Set(promise.value);
                    view.SetEthereumManaLoadingActive(false);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(WalletUtils.FETCH_MANA_INTERVAL), cancellationToken: cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    break;
            }
        }

        private async UniTask RequestPolygonManaAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                await UniTask.WaitUntil(() =>
                    ownUserProfile != null &&
                    !string.IsNullOrEmpty(ownUserProfile.userId) &&
                    !dataStore.wallet.isWalletSectionVisible.Get(),
                    cancellationToken: cancellationToken);

                view.SetPolygonManaLoadingActive(true);
                Promise<double> promise = theGraph.QueryPolygonMana(ownUserProfile.userId);
                if (promise != null)
                {
                    await promise;
                    dataStore.wallet.currentPolygonManaBalance.Set(promise.value);
                    view.SetPolygonManaLoadingActive(false);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(WalletUtils.FETCH_MANA_INTERVAL), cancellationToken: cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    break;
            }
        }
    }
}
