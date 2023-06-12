using Cysharp.Threading.Tasks;
using DCL.Models;
using DCL.Providers;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2.Tests
{
    public class LoadingScreenControllerShould2
    {
        private readonly string sourceHintViewAddressable = "LoadingScreenV2HintView.prefab";

        private HintRequestService hintRequestService;
        private IAddressableResourceProvider addressableProvider;
        private List<IHintRequestSource> hintRequestSources;
        private CancellationToken cancellationToken;
        private Hint premadeHint1;
        private Hint premadeHint2;

        [SetUp]
        public void Setup()
        {
            // setup sources
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
            var loadingScreenHintsController = new LoadingScreenHintsController(hintRequestService);

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
            var loadingScreenHintsController = new LoadingScreenHintsController(hintRequestService);

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
            var loadingScreenHintsController = new LoadingScreenHintsController(hintRequestService);

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
