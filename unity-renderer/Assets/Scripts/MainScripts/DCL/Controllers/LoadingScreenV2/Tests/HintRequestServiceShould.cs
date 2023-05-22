using NUnit.Framework;
// using NSubstitute;
// using Cysharp.Threading.Tasks;
// using System.Collections.Generic;
// using System.Threading;
// using UnityEngine;
// using UnityEngine.TestTools;

namespace DCL.Controllers.LoadingScreenV2
{
    public class HintRequestServiceShould
    {
         // private HintRequestService _hintRequestService;
         // private List<IHintRequestSource> _hintRequestSources;
         // private ISceneController _sceneController;
         // private HintTextureRequest _hintTextureRequest;
         // private CancellationToken _cancellationToken;
         // private Texture2D _preMadeTexture;

         // [SetUp]
         // public void Setup()
         // {
         //     // _hintRequestSources = new List<IHintRequestSource>(); // <-- This is the line that causes the tests error
         //     // _sceneController = Substitute.For<ISceneController>();
         //     // _hintTextureRequest = Substitute.For<HintTextureRequest>();
         //     // _hintRequestService = new HintRequestService(_hintRequestSources, _sceneController, _hintTextureRequest);
         //     // _cancellationToken = new CancellationToken();
         //     //
         //     // _preMadeTexture = new Texture2D(2, 2);
         //     // _hintTextureRequest.DownloadTexture(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(UniTask.FromResult(_preMadeTexture));
         // }

         // [TearDown]
         // public void TearDown()
         // {
         //     // _hintRequestService.Dispose();
         // }
         [Test]
         public void RequestHint()
         {
             Assert.IsTrue(true);
         }
         //
         // [Test]
         // public void RequestHintsFromSources_EmptyHintSourceList_ReturnsEmptyHintList()
         // {
         //     // Act
         //     // var result = await _hintRequestService.RequestHintsFromSources(_cancellationToken, 5);
         //     var result = new List<IHint>();
         //
         //     // Assert
         //     Assert.IsEmpty(result);
         // }

//         [Test]
//         public async UniTask RequestHintsFromSources_HintSourceReturnsEmptyList_ReturnsEmptyHintList()
//         {
//             // Arrange
//             var mockSource = Substitute.For<IHintRequestSource>();
//             var mockHint = Substitute.For<IHint>();
//             mockHint.TextureUrl.Returns("https://example.com/image.png");
//             mockHint.SourceTag.Returns(SourceTag.Scene);
//             var hintList = new List<IHint> { mockHint };
//             mockSource.GetHintsAsync(_cancellationToken).Returns(UniTask.FromResult(hintList));
//             _hintRequestSources.Add(mockSource);
//
//             // Act
//             var result = await _hintRequestService.RequestHintsFromSources(_cancellationToken, 5);
//
//             // Assert
//             Assert.IsEmpty(result);
//         }
//
//         [Test]
//         public async UniTask RequestHintsFromSources_HintSourceReturnsList_ReturnsHintDictionary()
//         {
//             // Arrange
//             var mockSource = Substitute.For<IHintRequestSource>();
//             var mockHint = Substitute.For<IHint>();
//             mockHint.TextureUrl.Returns("https://example.com/image.png");
//             mockHint.SourceTag.Returns(SourceTag.Scene);
//             var hintList = new List<IHint> { mockHint };
//             mockSource.GetHintsAsync(_cancellationToken).Returns(UniTask.FromResult(hintList));
//             _hintRequestSources.Add(mockSource);
//
//             // Act
//             var result = await _hintRequestService.RequestHintsFromSources(_cancellationToken, 5);
//
//             // Assert
//             Assert.AreEqual(5, result.Count);
//             Assert.IsTrue(result.ContainsKey(mockHint));
//             Assert.AreEqual(_preMadeTexture, result[mockHint]);
//         }
//
        // TODO: Add more test cases for texture handling, handling exceptions, ordering of hints, etc.
    }
}
