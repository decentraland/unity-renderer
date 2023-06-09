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
    public class LoadingScreenControllerShould
    {
        private readonly string sourceHintViewAddressable = "LoadingScreenV2HintView.prefab";

        private HintRequestService hintRequestService;
        private List<IHintRequestSource> hintRequestSources;
        private ISceneController sceneController;
        private IHintTextureRequestHandler hintTextureRequestHandler;
        private CancellationToken cancellationToken;
        // private HintView hintViewPref;
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
        public async Task InitializeHintView()
        {
            //Arrange
            var hint = new Hint("https://example.com/image1.png", "title1", "body1", SourceTag.Event);

            IAddressableResourceProvider  addressableProvider = new AddressableResourceProvider();
            HintView hintViewPrefab = await addressableProvider.GetAddressable<HintView>(sourceHintViewAddressable, cancellationToken);

            //Act
            hintViewPrefab.Initialize(hint, new Texture2D(2, 2));

            //Assert
            Assert.IsFalse(hintViewPrefab.isActiveAndEnabled);
            Assert.AreEqual(hintViewPrefab.hintText.text, hint.Title);
            Assert.NotNull(hintViewPrefab.hintImage.sprite);
        }

        [Test]
        public async Task RequestAndDisplayHints()
        {
            // Arrange
            var loadingScreenHintsController = new LoadingScreenHintsController(hintRequestService);

            // Act
            // Carousel started automatically on RequestHints

            // Assert
            // Check if the carousel was started
            Assert.IsNotNull(loadingScreenHintsController.cancellationTokenSource);
        }
    }
}
