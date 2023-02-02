using Cysharp.Threading.Tasks;
using DCL;
using DCL.LoadingScreen;
using DCL.Providers;
using System.Threading;

namespace DCLPlugins.LoadingScreenPlugin
{
    /// <summary>
    /// Plugin controller for the decoupled Loading Screen
    /// </summary>
    public class LoadingScreenPlugin : IPlugin
    {
        private const string LOADING_SCREEN_ASSET = "_LoadingScreen";

        private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;
        private readonly IAddressableResourceProvider assetsProvider;
        private readonly CancellationTokenSource cancellationTokenSource;

        private LoadingScreenController loadingScreenController;

        public LoadingScreenPlugin(IAddressableResourceProvider assetsProvider)
        {
            this.assetsProvider = assetsProvider;

            dataStoreLoadingScreen.Ref.decoupledLoadingHUD.visible.Set(true);

            cancellationTokenSource = new CancellationTokenSource();
            CreateLoadingScreen().Forget();
        }

        private async UniTask CreateLoadingScreen()
        {
            loadingScreenController = new LoadingScreenController(
                await CreateLoadingScreenView(assetsProvider, cancellationTokenSource.Token),
                Environment.i.world.sceneController, Environment.i.world.state, NotificationsController.i,
                DataStore.i.player, DataStore.i.common, dataStoreLoadingScreen.Ref, DataStore.i.realm);
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();

            loadingScreenController.Dispose();
        }

        public static async UniTask<LoadingScreenView> CreateLoadingScreenView(IAddressableResourceProvider assetsProvider, CancellationToken cancellationToken) =>
            await assetsProvider.Instantiate<LoadingScreenView>(LOADING_SCREEN_ASSET, cancellationToken: cancellationToken);
    }
}
