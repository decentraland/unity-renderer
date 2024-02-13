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
    public class LoadingScreenV2ProxyPlugin
    {

        private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;
        private CancellationTokenSource cancellationTokenSource;

        private LoadingScreenController loadingScreenController;

        private readonly string LOCAL_HINT_RESOURCE_ADDRESSABLE = "LoadingScreenV2LocalHintsJsonSource";
        // TODO: the following REMOTE_HINT_URL is a placeholder URL
        private readonly string REMOTE_HINT_URL = "http://dcl-catalyst-content-api.decentraland.org/v1/contents/5f6a5b3b-9b0a-4b7a-8a5a-1a8c0e1f1f0e/hints";

        private  IAddressableResourceProvider addressableProvider;
        private ISceneController sceneController;
        private List<IHintRequestSource> hintRequestSources;
        private HintTextureRequestHandler hintTextureRequestHandler;
        private ISourceWebRequestHandler sourceWebRequestHandler;
        private HintRequestService hintRequestService;

        private LoadingScreenHintsController loadingScreenHintsController;

        public LoadingScreenV2ProxyPlugin() { }

        public async UniTask<LoadingScreenHintsController> InitializeAsync(LoadingScreenView loadingScreenView, IAddressableResourceProvider addressableProvider, CancellationToken cancellationToken = default)
        {
            this.addressableProvider = addressableProvider;
            this.cancellationTokenSource = new CancellationTokenSource();

            sourceWebRequestHandler = new HintSourceSourceWebRequestHandler();
            sceneController = Environment.i.serviceLocator.Get<ISceneController>();
            hintRequestSources = new List<IHintRequestSource>();
            hintRequestSources.Add(new LocalHintRequestSource(LOCAL_HINT_RESOURCE_ADDRESSABLE, SourceTag.Dcl, addressableProvider));
            hintRequestSources.Add(new RemoteHintRequestSource(REMOTE_HINT_URL, SourceTag.Dcl, sourceWebRequestHandler));
            hintRequestSources.Add(new SceneHintRequestSource(SourceTag.Scene, sceneController));

            hintTextureRequestHandler = new HintTextureRequestHandler();
            hintRequestService = new HintRequestService(hintRequestSources, sceneController, hintTextureRequestHandler);

            loadingScreenHintsController = new LoadingScreenHintsController(hintRequestService, loadingScreenView, addressableProvider);

            return loadingScreenHintsController;
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();

            loadingScreenController.Dispose();

            loadingScreenHintsController.Dispose();
        }
    }
}
