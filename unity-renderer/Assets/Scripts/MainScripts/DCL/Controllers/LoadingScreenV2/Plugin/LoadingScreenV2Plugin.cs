using Cysharp.Threading.Tasks;
using DCL;
using DCL.LoadingScreen;
using DCL.LoadingScreen.V2;
using DCL.Providers;
using System.Collections.Generic;
using System.Threading;

namespace DCLPlugins.LoadingScreenPlugin
{
    public class LoadingScreenV2Plugin : IPlugin
    {
        private readonly string HINT_VIEW_PREFAB_ADDRESSABLE = "LoadingScreenV2HintView.prefab";
        private readonly string LOCAL_HINT_RESOURCE_ADDRESSABLE = "LoadingScreenV2LocalHintsJsonSource";
        // TODO: FD:: the following REMOTE_HINT_URL is a placeholder URL
        private readonly string REMOTE_HINT_URL = "http://dcl-catalyst-content-api.decentraland.org/v1/contents/5f6a5b3b-9b0a-4b7a-8a5a-1a8c0e1f1f0e/hints";

        private readonly IAddressableResourceProvider addressableProvider;
        private ISceneController sceneController;
        private List<IHintRequestSource> hintRequestSources;
        private HintTextureRequestHandler hintTextureRequestHandler;
        private ISourceWebRequestHandler sourceWebRequestHandler;
        private HintRequestService hintRequestService;

        private LoadingScreenHintsController controller;

        private CancellationTokenSource cts;

        public LoadingScreenV2Plugin(IAddressableResourceProvider addressableProvider)
        {
            // Loading Screen V2
            this.addressableProvider = addressableProvider;
            cts = new CancellationTokenSource();

            InitializeHintSystem(cts).Forget();
        }


        private async UniTaskVoid InitializeHintSystem(CancellationTokenSource cts)
        {
            // sourceWebRequestHandler = new HintSourceSourceWebRequestHandler();
            // sceneController = Environment.i.serviceLocator.Get<ISceneController>();
            //
            // hintRequestSources = new List<IHintRequestSource>();
            // hintRequestSources.Add(new LocalHintRequestSource(LOCAL_HINT_RESOURCE_ADDRESSABLE, SourceTag.Dcl, addressableProvider));
            // hintRequestSources.Add(new RemoteHintRequestSource(REMOTE_HINT_URL, SourceTag.Dcl, sourceWebRequestHandler));
            // hintRequestSources.Add(new SceneHintRequestSource(SourceTag.Scene, sceneController));
            //
            // hintTextureRequestHandler = new HintTextureRequestHandler();
            // hintRequestService = new HintRequestService(hintRequestSources, sceneController, hintTextureRequestHandler);
            // var hintViewPrefab = await addressableProvider.GetAddressable<HintView>(HINT_VIEW_PREFAB_ADDRESSABLE, cts.Token);
            //
            // controller = new LoadingScreenHintsController(hintRequestService, hintViewPrefab;
        }


        public void Dispose()
        {
            controller.Dispose();
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }
    }

}

