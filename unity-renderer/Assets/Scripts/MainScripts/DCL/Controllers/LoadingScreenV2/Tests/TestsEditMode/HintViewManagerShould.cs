using Cysharp.Threading.Tasks;
using DCL.Models;
using DCL.Providers;
using DCLPlugins.LoadingScreenPlugin;
using MainScripts.DCL.Controllers.AssetManager.Addressables.Editor;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DCL.LoadingScreen.V2.Tests
{
    public class HintViewManagerShould : MonoBehaviour
    {
        private readonly string HINT_VIEW_PREFAB_ADDRESSABLE = "LoadingScreenV2HintView.prefab";
        private const string LOADING_SCREEN_ASSET = "_LoadingScreenV2";

        private HintRequestService hintRequestService;
        private List<IHintRequestSource> hintRequestSources;
        private ISceneController sceneController;
        private IHintTextureRequestHandler hintTextureRequestHandler;
        private CancellationToken cancellationToken;
        private Hint premadeHint1;
        private Hint premadeHint2;
        private IAddressableResourceProvider addressableProvider;

        [SetUp]
        public void Setup()
        {
            // setup sources
            addressableProvider = new EditorAddressableResourceProvider();
            premadeHint1 = new Hint("https://example.com/image1.png", "title1", "body1", SourceTag.Event);
            premadeHint2 = new Hint("https://example.com/image2.png", "title2", "body2", SourceTag.Dcl);
            var sourceUrlJson = "http://remote_source_url";
            var mockWebRequestHandler = Substitute.For<ISourceWebRequestHandler>();
            var mockSceneRensponse = new LoadParcelScenesMessage.UnityParcelScene
            {
                loadingScreenHints = new List<Hint> { premadeHint1, premadeHint2 },
            };
            string mockJsonResponse = JsonUtility.ToJson(mockSceneRensponse);
            mockWebRequestHandler.Get(Arg.Any<string>()).Returns(UniTask.FromResult(mockJsonResponse));

            var remoteHintSource = new RemoteHintRequestSource(sourceUrlJson, SourceTag.Event, mockWebRequestHandler);
            hintRequestSources = new List<IHintRequestSource>();
            hintRequestSources.Add(remoteHintSource);

            // setup the rest
            sceneController = Substitute.For<ISceneController>();
            sceneController.OnNewSceneAdded += null;
            hintTextureRequestHandler = new HintTextureRequestHandler();
            hintRequestService = new HintRequestService(hintRequestSources, sceneController, hintTextureRequestHandler);
            cancellationToken = new CancellationToken();
        }

        [TearDown]
        public void TearDown()
        {
            hintRequestService.Dispose();
        }

        [Test]
        public async Task StartAndStopHintsCarousel()
        {
            // Arrange
            LoadingScreenView loadingScreenView = GameObject.Instantiate(Resources.Load<GameObject>(LOADING_SCREEN_ASSET)).GetComponent<LoadingScreenView>();
            var loadingScreenHintsController = new LoadingScreenHintsController(hintRequestService, loadingScreenView, addressableProvider);
            loadingScreenView.ToggleLoadingScreenV2(true);
            var loadingScreenV2ProxyPlugin = new LoadingScreenV2ProxyPlugin();
            loadingScreenHintsController = await loadingScreenV2ProxyPlugin.InitializeAsync(loadingScreenView, addressableProvider, cancellationToken);

            // Create a TaskCompletionSource to wait for RequestHints to complete
            var requestHintsCompletedTaskSource = new TaskCompletionSource<bool>();
            loadingScreenHintsController.OnRequestHintsCompleted += () => requestHintsCompletedTaskSource.SetResult(true);

            // Wait for RequestHints to complete
            await requestHintsCompletedTaskSource.Task;

            // Assert
            // Check if the carousel was started
            Assert.IsTrue(loadingScreenHintsController.hintViewManager.isIteratingHints);

            // Act
            loadingScreenHintsController.StopHintsCarousel();

            // Assert
            // Check if the carousel was stopped
            Assert.IsFalse(loadingScreenHintsController.hintViewManager.isIteratingHints);
        }


        [Test]
        public async Task CarouselNextAndPreviousHint()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            LoadingScreenView loadingScreenView = GameObject.Instantiate(Resources.Load<GameObject>(LOADING_SCREEN_ASSET)).GetComponent<LoadingScreenView>();
            var loadingScreenHintsController = new LoadingScreenHintsController(hintRequestService, loadingScreenView, addressableProvider);
            var loadingScreenV2ProxyPlugin = new LoadingScreenV2ProxyPlugin();
            loadingScreenHintsController = await loadingScreenV2ProxyPlugin.InitializeAsync(loadingScreenView, addressableProvider, cancellationToken);
            loadingScreenView.ToggleLoadingScreenV2(true);

            // Create a TaskCompletionSource to wait for RequestHints to complete
            var requestHintsCompletedTaskSource = new TaskCompletionSource<bool>();
            loadingScreenHintsController.OnRequestHintsCompleted += () => requestHintsCompletedTaskSource.SetResult(true);

            // Wait for RequestHints to complete
            await requestHintsCompletedTaskSource.Task;

            // Act
            loadingScreenHintsController.CarouselNextHint();
            var nextHintIndex = loadingScreenHintsController.hintViewManager.currentHintIndex;

            // Assert
            // Check if the carousel moved to the next hint
            Assert.AreEqual(1, nextHintIndex);

            // Act
            loadingScreenHintsController.CarouselPreviousHint();
            var previousHintIndex = loadingScreenHintsController.hintViewManager.currentHintIndex;

            // Assert
            // Check if the carousel moved to the previous hint
            Assert.AreEqual(0, previousHintIndex);

            // Dispose
            loadingScreenHintsController.cancellationTokenSource.Cancel();
        }
    }
}
