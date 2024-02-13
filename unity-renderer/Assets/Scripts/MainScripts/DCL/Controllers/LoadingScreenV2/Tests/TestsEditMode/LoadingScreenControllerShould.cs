using Cysharp.Threading.Tasks;
using DCL.Models;
using DCL.Providers;
using DCLPlugins.LoadingScreenPlugin;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DCL.LoadingScreen.V2.Tests
{
    public class LoadingScreenControllerShould2
    {
        private readonly string HINT_VIEW_PREFAB_ADDRESSABLE = "LoadingScreenV2HintView.prefab";
        private const string LOADING_SCREEN_ASSET = "_LoadingScreenV2";

        private HintRequestService hintRequestService;
        private IAddressableResourceProvider addressableProvider;
        private List<IHintRequestSource> hintRequestSources;
        private CancellationToken cancellationToken;
        private Hint premadeHint1;
        private Hint premadeHint2;
        private CancellationTokenSource cts;

        [SetUp]
        public void Setup()
        {
            // setup sources
            premadeHint1 = new Hint("https://example.com/image1.png", "title1", "body1", SourceTag.Event);
            premadeHint2 = new Hint("https://example.com/image2.png", "title2", "body2", SourceTag.Dcl);
            var sourceUrlJson = "http://remote_source_url";
            addressableProvider = new AddressableResourceProvider();
            cts = new CancellationTokenSource();

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
            ISceneController sceneController = Substitute.For<ISceneController>();
            IHintTextureRequestHandler hintTextureRequestHandler = new HintTextureRequestHandler();
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

            // Check if the carousel was started
            Assert.IsTrue(loadingScreenHintsController.hintViewManager.isIteratingHints);

            // Act
            loadingScreenHintsController.StopHintsCarousel();

            // Check if the carousel was stopped
            Assert.IsFalse(loadingScreenHintsController.hintViewManager.isIteratingHints);
        }

        [Test]
        public async Task InitializeHintsProperlyAsync()
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
            Assert.IsNotNull(loadingScreenHintsController.hintViewPrefab);
            Assert.IsNotNull(loadingScreenHintsController.hintViewPool);
            Assert.AreEqual(15, loadingScreenHintsController.hintViewPool.Count);
        }

        [Test]
        public async Task RequestHintsProperlyAsync()
        {
            // Arrange
            // hintViewPrefab = await addressableProvider.GetAddressable<HintView>(HINT_VIEW_PREFAB_ADDRESSABLE, cts.Token);
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

            // Act
            // Request hints has been already executed on the constructor

            // Assert
            Assert.IsNotNull(loadingScreenHintsController.hintsDictionary);
            Assert.IsTrue(loadingScreenHintsController.hintsDictionary.Count > 0);
            Assert.IsNotNull(loadingScreenHintsController.hintViewManager);
        }

    }
}
