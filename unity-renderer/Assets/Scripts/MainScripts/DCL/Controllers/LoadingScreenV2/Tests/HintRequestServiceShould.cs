using Cysharp.Threading.Tasks;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
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
             var expectedHint = new BaseHint("url", "title", "body", SourceTag.Event);
             var mockSceneRensponse = new LoadParcelScenesMessage.UnityParcelScene
             {
                 loadingScreenHints = new List<BaseHint> { expectedHint }
             };
             string mockJsonResponse = JsonUtility.ToJson(mockSceneRensponse);
             mockWebRequestHandler.Get(Arg.Any<string>()).Returns(UniTask.FromResult(mockJsonResponse));

             var remoteHintSource = new RemoteHintRequestSource(sourceUrlJson, SourceTag.Event, mockWebRequestHandler);
             _hintRequestSources.Add(remoteHintSource);

             // Act
             var result = await _hintRequestService.RequestHintsFromSources(_cancellationToken, 5);

             Debug.Log($"FD:: result.Count: {result.Count}");

             // Assert
             Assert.AreEqual(1, result.Count);
             Assert.AreEqual(expectedHint.Title, result.Keys.ToArray()[0].Title);
             Assert.AreEqual(expectedHint.Body, result.Keys.ToArray()[0].Body);
         }




         // [Test]
         // public async UniTask RequestHintsWithOneSource()
         // {
         //     // Arrange
         //     var mockSource = Substitute.For<LocalHintRequestSource>();
         //     var mockHint = Substitute.For<IHint>();
         //     mockHint.TextureUrl.Returns("https://example.com/image.png");
         //     mockHint.SourceTag.Returns(SourceTag.Dcl);
         //     var hintList = new List<IHint> { mockHint };
         //     // mockSource.GetHintsAsync(_cancellationToken).Returns(UniTask.FromResult(hintList));
         //     _hintRequestSources.Add(mockSource);
         //
         //     // Act
         //     var result = await _hintRequestService.RequestHintsFromSources(_cancellationToken, 5);
         //
         //     // Assert
         //     Assert.IsTrue(result.Count == 1);
         // }

         // [Test]
         // public async UniTask RequestHintsFromSources_HintSourceReturnsList_ReturnsHintDictionary()
         // {
         //     // Arrange
         //     var mockSource = Substitute.For<IHintRequestSource>();
         //     var mockHint = Substitute.For<IHint>();
         //     mockHint.TextureUrl.Returns("https://example.com/image.png");
         //     mockHint.SourceTag.Returns(SourceTag.Scene);
         //     var hintList = new List<IHint> { mockHint };
         //     mockSource.GetHintsAsync(_cancellationToken).Returns(UniTask.FromResult(hintList));
         //     _hintRequestSources.Add(mockSource);
         //
         //     // Act
         //     var result = await _hintRequestService.RequestHintsFromSources(_cancellationToken, 5);
         //
         //     // Assert
         //     Assert.AreEqual(5, result.Count);
         //     Assert.IsTrue(result.ContainsKey(mockHint));
         //     Assert.AreEqual(_preMadeTexture, result[mockHint]);
         // }

        // TODO: Add more test cases for texture handling, handling exceptions, ordering of hints, etc.
    }
}
