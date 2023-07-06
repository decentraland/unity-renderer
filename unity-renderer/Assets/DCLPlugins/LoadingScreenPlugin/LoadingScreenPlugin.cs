using Cysharp.Threading.Tasks;
using DCL;
using DCL.LoadingScreen.V2;
using DCL.LoadingScreen;
using DCL.Providers;
using MainScripts.DCL.Controllers.ShaderPrewarm;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Environment = DCL.Environment;

namespace DCLPlugins.LoadingScreenPlugin
{
    /// <summary>
    /// Plugin controller for the decoupled Loading Screen
    /// </summary>
    public class LoadingScreenPlugin : IPlugin
    {
        private const string LOADING_SCREEN_ASSET = "_LoadingScreenV2";

        private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly IAddressableResourceProvider addressableProvider;

        private LoadingScreenController loadingScreenController;
        private LoadingScreenHintsController loadingScreenHintsController;
        private LoadingScreenView loadingScreenView;

        public LoadingScreenPlugin()
        {
            this.addressableProvider = Environment.i.serviceLocator.Get<IAddressableResourceProvider>();
            dataStoreLoadingScreen.Ref.decoupledLoadingHUD.visible.Set(true);

            cancellationTokenSource = new CancellationTokenSource();
            CreateLoadingScreen(cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid CreateLoadingScreen(CancellationToken cancellationToken = default)
        {
            loadingScreenView = CreateLoadingScreenView();

            // FD:: keeping this until LoadingScreenV2 is finalized
            if (dataStoreLoadingScreen.Ref.decoupledLoadingHUD.loadingScreenV2Enabled.Get())
            {
                InitializeV2Plugin(true, false);
            }
            else
            {
                dataStoreLoadingScreen.Ref.decoupledLoadingHUD.loadingScreenV2Enabled.OnChange += InitializeV2Plugin;
            }

            loadingScreenController = new LoadingScreenController(
                loadingScreenView, loadingScreenHintsController,
                Environment.i.world.sceneController, Environment.i.world.state, NotificationsController.i,
                DataStore.i.player, DataStore.i.common, dataStoreLoadingScreen.Ref, DataStore.i.realm, new ShaderPrewarm(Environment.i.serviceLocator.Get<IAddressableResourceProvider>()));
        }

        private void InitializeV2Plugin(bool activeState, bool previousActiveState)
        {
            if (!activeState) return;

            loadingScreenView.ToggleLoadingScreenV2(activeState);
            var loadingScreenV2ProxyPlugin = new LoadingScreenV2ProxyPlugin();
            InitializeV2Async(loadingScreenV2ProxyPlugin, cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid InitializeV2Async(LoadingScreenV2ProxyPlugin loadingScreenV2ProxyPlugin, CancellationToken cancellationToken)
        {
            loadingScreenHintsController = await loadingScreenV2ProxyPlugin.InitializeAsync(loadingScreenView, addressableProvider, cancellationToken);
        }

        public static LoadingScreenView CreateLoadingScreenView() =>
            GameObject.Instantiate(Resources.Load<GameObject>(LOADING_SCREEN_ASSET)).GetComponent<LoadingScreenView>();

        public void Dispose()
        {
            dataStoreLoadingScreen.Ref.decoupledLoadingHUD.loadingScreenV2Enabled.OnChange -= InitializeV2Plugin;

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();

            loadingScreenController.Dispose();
            loadingScreenView = null;

            loadingScreenHintsController?.Dispose();
        }
    }
}
