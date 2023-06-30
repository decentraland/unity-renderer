using Cysharp.Threading.Tasks;
using DCL;
using DCL.LoadingScreen.V2;
using DCL.LoadingScreen;
using DCL.Providers;
using MainScripts.DCL.Controllers.ShaderPrewarm;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCLPlugins.LoadingScreenPlugin
{
    /// <summary>
    /// Plugin controller for the decoupled Loading Screen
    /// </summary>
    public class LoadingScreenPlugin : IPlugin
    {
        private const string LOADING_SCREEN_ASSET = "_LoadingScreen";

        private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly IAddressableResourceProvider addressableProvider;

        private LoadingScreenController loadingScreenController;
        private LoadingScreenHintsController loadingScreenHintsController;

        public LoadingScreenPlugin()
        {
            this.addressableProvider = Environment.i.serviceLocator.Get<IAddressableResourceProvider>();
            dataStoreLoadingScreen.Ref.decoupledLoadingHUD.visible.Set(true);

            cancellationTokenSource = new CancellationTokenSource();
            CreateLoadingScreen(cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid CreateLoadingScreen(CancellationToken cancellationToken = default)
        {
            var loadingScreenView = CreateLoadingScreenView();

            // FD:: keeping this until LoadingScreenV2 is finalized
            if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("loading_screen_v2"))
            {
                loadingScreenView.ToggleTeleportLoadingAnimation(false);
                var loadingScreenV2ProxyPlugin = new LoadingScreenV2ProxyPlugin();
                loadingScreenHintsController = await loadingScreenV2ProxyPlugin.InitializeAsync(loadingScreenView, addressableProvider, cancellationToken);
            }

            loadingScreenController = new LoadingScreenController(
                loadingScreenView, loadingScreenHintsController,
                Environment.i.world.sceneController, Environment.i.world.state, NotificationsController.i,
                DataStore.i.player, DataStore.i.common, dataStoreLoadingScreen.Ref, DataStore.i.realm, new ShaderPrewarm(Environment.i.serviceLocator.Get<IAddressableResourceProvider>()));
        }

        public static LoadingScreenView CreateLoadingScreenView() =>
            GameObject.Instantiate(Resources.Load<GameObject>(LOADING_SCREEN_ASSET)).GetComponent<LoadingScreenView>();

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();

            loadingScreenController.Dispose();

            loadingScreenHintsController?.Dispose();
        }
    }
}
