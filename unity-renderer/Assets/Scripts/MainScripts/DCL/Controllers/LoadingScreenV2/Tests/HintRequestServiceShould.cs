using Cysharp.Threading.Tasks;
using DCL.Models;
using DCL.Providers;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    [Category("EditModeCI")]
    [TestFixture]
    public class HintRequestServiceShould
    {
         private HintRequestService _hintRequestService;
         private List<IHintRequestSource> _hintRequestSources;
         private ISceneController _sceneController;
         private HintTextureRequestHandler hintTextureRequestHandler;
         private CancellationToken _cancellationToken;
         private Texture2D _preMadeTexture;
         private BaseHint _premadeHint;

         [SetUp]
         public void Setup()
         {
             _hintRequestSources = new List<IHintRequestSource>(); // <-- This is the line that causes the tests error
             _sceneController = Substitute.For<ISceneController>();
             hintTextureRequestHandler = Substitute.For<HintTextureRequestHandler>();
             _hintRequestService = new HintRequestService(_hintRequestSources, _sceneController, hintTextureRequestHandler);
             _cancellationToken = new CancellationToken();

             _preMadeTexture = new Texture2D(2, 2);
             // _hintTextureRequest.DownloadTexture(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(UniTask.FromResult(_preMadeTexture));

             // mock premade hint for scene response
             _premadeHint = new BaseHint("https://example.com/image.png", "title", "body", SourceTag.Event);
         }

         [TearDown]
         public void TearDown()
         {
             _hintRequestService.Dispose();
         }

         [Test]
         public async Task RequestHintsWithZeroSources()
         {
             // Act
             var result = await _hintRequestService.RequestHintsFromSources(_cancellationToken, 5);

             // Assert
             Assert.IsEmpty(result);
         }

         [Test]
         public async Task RequestHintsFromRemoteSource()
         {
             // Arrange
             var sourceUrlJson = "http://remote_source_url";
             var mockWebRequestHandler = Substitute.For<ISourceWebRequestHandler>();
             var mockSceneRensponse = new LoadParcelScenesMessage.UnityParcelScene
             {
                 loadingScreenHints = new List<BaseHint> { _premadeHint },
             };
             Debug.Log ("FD:: mockSceneRensponse: " + mockSceneRensponse);
             string mockJsonResponse = JsonUtility.ToJson(mockSceneRensponse);
             mockWebRequestHandler.Get(Arg.Any<string>()).Returns(UniTask.FromResult(mockJsonResponse));

             var remoteHintSource = new RemoteHintRequestSource(sourceUrlJson, SourceTag.Event, mockWebRequestHandler);
             _hintRequestSources.Add(remoteHintSource);

             // Act
             var result = await _hintRequestService.RequestHintsFromSources(_cancellationToken, 5);

             // Assert
             Assert.AreEqual(1, result.Count);
             Assert.AreEqual(_premadeHint.Title, result.Keys.ToArray()[0].Title);
             Assert.AreEqual(_premadeHint.Body, result.Keys.ToArray()[0].Body);
         }

         [Test]
         public async Task RequestHintsFromLocalSource()
         {
             // Arrange
             var sourceSceneAddressable = "LoadingScreenV2LocalHintsJsonSource";
             IAddressableResourceProvider  addressableProvider = new AddressableResourceProvider();
             var remoteHintSource = new LocalHintRequestSource(sourceSceneAddressable, SourceTag.Dcl, addressableProvider);
             _hintRequestSources.Add(remoteHintSource);

             // Act
             var result = await _hintRequestService.RequestHintsFromSources(_cancellationToken, 5);

             // Assert
             Assert.AreEqual(1, result.Count);
         }

         [Test] // FD:: TODO: This test is failing because of the Environment requirement in SceneHintRequestSource
         public async Task ShouldRequestHintsFromSceneSource()
         {
             // Arrange
             string sceneJson = "scene.json";
             var mockSceneController = Substitute.For<ISceneController>();
             var mockScene = Substitute.For<IParcelScene>();
             var mockSceneRensponse = new LoadParcelScenesMessage.UnityParcelScene
             {
                 loadingScreenHints = new List<BaseHint> { _premadeHint },
             };
             mockScene.sceneData.Returns(mockSceneRensponse);
             var sourceTag = SourceTag.Event;
             var currentDestination = new Vector2Int(1, 1);

             mockSceneController
                .When(controller => controller.OnNewSceneAdded += Arg.Any<Action<IParcelScene>>())
                .Do(info =>
                 {
                     var handler = info.Arg<Action<IParcelScene>>();
                     handler(mockScene);
                 });

             // var sceneHintSource = new SceneHintRequestSource(sceneJson, sourceTag, mockSceneController, currentDestination);
             var sceneHintSource = Substitute.For<SceneHintRequestSource>(sceneJson, sourceTag, mockSceneController, currentDestination);
             sceneHintSource.CheckTargetSceneWithCoords(Arg.Any<IParcelScene>()).Returns(true);

             // Act
             var result = await sceneHintSource.GetHintsAsync(_cancellationToken);

             // Assert
             Assert.AreEqual(1, result.Count);
             Assert.AreEqual(_premadeHint.Title, result[0].Title);
             Assert.AreEqual(_premadeHint.Body, result[0].Body);
         }

        // TODO: Add more test cases for texture handling, handling exceptions, ordering of hints, etc.
    }
}
