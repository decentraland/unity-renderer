using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Tasks;
using System;
using System.Threading;
using UnityEngine;

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
                RequestManaAsync(TheGraphNetwork.Ethereum, fetchEthereumManaCancellationToken.Token).Forget();
                RequestManaAsync(TheGraphNetwork.Polygon, fetchPolygonManaCancellationToken.Token).Forget();
            }
            else
            {
                fetchEthereumManaCancellationToken.SafeCancelAndDispose();
                fetchPolygonManaCancellationToken.SafeCancelAndDispose();
            }
        }

        private async UniTask RequestManaAsync(TheGraphNetwork network, CancellationToken cancellationToken)
        {
            while (true)
            {
                await UniTask.WaitUntil(() =>
                        ownUserProfile != null &&
                        !string.IsNullOrEmpty(ownUserProfile.userId) &&
                        !dataStore.wallet.isWalletSectionVisible.Get(),
                    cancellationToken: cancellationToken);

                double ethereumManaBalanceResult = dataStore.wallet.currentEthereumManaBalance.Get();
                double polygonManaBalanceResult = dataStore.wallet.currentPolygonManaBalance.Get();

                try
                {
                    if (network == TheGraphNetwork.Ethereum)
                        view.SetEthereumManaLoadingActive(true);
                    else
                        view.SetPolygonManaLoadingActive(true);

                    Promise<double> promise = theGraph.QueryMana(ownUserProfile.userId, network);

                    if (promise != null)
                    {
                        await promise;

                        if (network == TheGraphNetwork.Ethereum)
                            ethereumManaBalanceResult = promise.value;
                        else
                            polygonManaBalanceResult = promise.value;
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception)
                {
                    Debug.LogError(network == TheGraphNetwork.Ethereum ?
                        "Error requesting Ethereum MANA balance from TheGraph!" :
                        "Error requesting Polygon MANA balance from TheGraph!");
                }
                finally
                {
                    if (network == TheGraphNetwork.Ethereum)
                    {
                        dataStore.wallet.currentEthereumManaBalance.Set(ethereumManaBalanceResult);
                        view.SetEthereumManaLoadingActive(false);
                    }
                    else
                    {
                        dataStore.wallet.currentPolygonManaBalance.Set(polygonManaBalanceResult);
                        view.SetPolygonManaLoadingActive(false);
                    }
                }

                await UniTask.Delay(TimeSpan.FromSeconds(WalletUtils.FETCH_MANA_INTERVAL), cancellationToken: cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    break;
            }
        }
    }
}
