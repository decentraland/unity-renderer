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
    public class HintViewManagerShould : MonoBehaviour
    {
        private HintRequestService hintRequestService;
        private List<IHintRequestSource> hintRequestSources;
        private ISceneController sceneController;
        private IHintTextureRequestHandler hintTextureRequestHandler;
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
            sceneController = Substitute.For<ISceneController>();
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
            var loadingScreenHintsController = new LoadingScreenHintsController(hintRequestService);

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
            var loadingScreenHintsController = new LoadingScreenHintsController(hintRequestService);

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
