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
#region Loading Screen V1
        private const string LOADING_SCREEN_ASSET = "_LoadingScreen";

        private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;
        private readonly CancellationTokenSource cancellationTokenSource;

        private LoadingScreenController loadingScreenController;
#endregion
#region Loading Screen V2
        // private readonly string HINT_VIEW_PREFAB_ADDRESSABLE = "LoadingScreenV2HintView.prefab";
        private readonly string LOCAL_HINT_RESOURCE_ADDRESSABLE = "LoadingScreenV2LocalHintsJsonSource";
        // TODO: FD:: the following REMOTE_HINT_URL is a placeholder URL
        private readonly string REMOTE_HINT_URL = "http://dcl-catalyst-content-api.decentraland.org/v1/contents/5f6a5b3b-9b0a-4b7a-8a5a-1a8c0e1f1f0e/hints";

        private readonly IAddressableResourceProvider addressableProvider;
        private ISceneController sceneController;
        private List<IHintRequestSource> hintRequestSources;
        private HintTextureRequestHandler hintTextureRequestHandler;
        private ISourceWebRequestHandler sourceWebRequestHandler;
        private HintRequestService hintRequestService;

        private LoadingScreenHintsController loadingScreenHintsController;
#endregion

        public LoadingScreenPlugin()
        {
            dataStoreLoadingScreen.Ref.decoupledLoadingHUD.visible.Set(true);

            cancellationTokenSource = new CancellationTokenSource();
            CreateLoadingScreen(cancellationTokenSource.Token).Forget();

            this.addressableProvider = Environment.i.serviceLocator.Get<IAddressableResourceProvider>();
        }

        private async UniTaskVoid CreateLoadingScreen(CancellationToken cancellationToken = default)
        {
            var loadingScreenView = CreateLoadingScreenView();
            loadingScreenController = new LoadingScreenController(
                loadingScreenView,
                Environment.i.world.sceneController, Environment.i.world.state, NotificationsController.i,
                DataStore.i.player, DataStore.i.common, dataStoreLoadingScreen.Ref, DataStore.i.realm, new ShaderPrewarm(Environment.i.serviceLocator.Get<IAddressableResourceProvider>()));

            Debug.Log("FD:: LoadingScreenPlugin - CreateLoadingScreen");
            // if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("loading_screen_v2"))
                InitializeHintSystem(loadingScreenView, cancellationToken).Forget();
        }

        public static LoadingScreenView CreateLoadingScreenView() =>
            GameObject.Instantiate(Resources.Load<GameObject>(LOADING_SCREEN_ASSET)).GetComponent<LoadingScreenView>();

#region Loading Screen V2 Initialization
        private async UniTaskVoid InitializeHintSystem(LoadingScreenView loadingScreenView, CancellationToken cancellationToken = default)
        {
            Debug.Log("FD:: LoadingScreenPlugin - InitializeHintSystem 1");
            sourceWebRequestHandler = new HintSourceSourceWebRequestHandler();
            sceneController = Environment.i.serviceLocator.Get<ISceneController>();
            hintRequestSources = new List<IHintRequestSource>();
            hintRequestSources.Add(new LocalHintRequestSource(LOCAL_HINT_RESOURCE_ADDRESSABLE, SourceTag.Dcl, addressableProvider));
            hintRequestSources.Add(new RemoteHintRequestSource(REMOTE_HINT_URL, SourceTag.Dcl, sourceWebRequestHandler));
            hintRequestSources.Add(new SceneHintRequestSource(SourceTag.Scene, sceneController));
            Debug.Log("FD:: LoadingScreenPlugin - InitializeHintSystem 2");

            hintTextureRequestHandler = new HintTextureRequestHandler();
            Debug.Log("FD:: LoadingScreenPlugin - InitializeHintSystem 3");
            hintRequestService = new HintRequestService(hintRequestSources, sceneController, hintTextureRequestHandler);
            Debug.Log("FD:: LoadingScreenPlugin - InitializeHintSystem 4");
            // var hintViewPrefab = await addressableProvider.GetAddressable<HintView>(HINT_VIEW_PREFAB_ADDRESSABLE, cancellationToken);
            loadingScreenHintsController = new LoadingScreenHintsController(hintRequestService/*, hintViewPrefab*/, loadingScreenView);
        }
#endregion

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();

            loadingScreenController.Dispose();

            if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("loading_screen_v2"))
                loadingScreenHintsController.Dispose();
        }
    }
}
