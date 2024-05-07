using Cysharp.Threading.Tasks;
using DCL.Models;
using DCL.Providers;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2.Tests
{
    public class HintRequestServiceShould
    {
         private HintRequestService hintRequestService;
         private List<IHintRequestSource> hintRequestSources;
         private ISceneController sceneController;
         private IHintTextureRequestHandler hintTextureRequestHandler;
         private CancellationToken cancellationToken;
         private Texture2D preMadeTexture;
         private Hint premadeHint1;
         private Hint premadeHint2;

         [SetUp]
         public void Setup()
         {
             hintRequestSources = new List<IHintRequestSource>();
             sceneController = Substitute.For<ISceneController>();
             hintTextureRequestHandler = Substitute.For<IHintTextureRequestHandler>();
             hintRequestService = new HintRequestService(hintRequestSources, sceneController, hintTextureRequestHandler);
             cancellationToken = new CancellationToken();

             preMadeTexture = new Texture2D(2, 2);

             // mock premade hint for scene response
             premadeHint1 = new Hint("https://example.com/image1.png", "title1", "body1", SourceTag.Event);
             premadeHint2 = new Hint("https://example.com/image2.png", "title2", "body2", SourceTag.Dcl);
         }

         [TearDown]
         public void TearDown()
         {
             hintRequestService.Dispose();
         }

         [Test]
         public async Task RequestHintsWithZeroSources()
         {
             // Act
             var result = await hintRequestService.RequestHintsFromSources(cancellationToken, 5);

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
                 loadingScreenHints = new List<Hint> { premadeHint1 },
             };
             string mockJsonResponse = JsonUtility.ToJson(mockSceneRensponse);
             mockWebRequestHandler.Get(Arg.Any<string>()).Returns(UniTask.FromResult(mockJsonResponse));

             var remoteHintSource = new RemoteHintRequestSource(sourceUrlJson, SourceTag.Event, mockWebRequestHandler);
             hintRequestSources.Add(remoteHintSource);

             // Act
             var result = await hintRequestService.RequestHintsFromSources(cancellationToken, 5);

             // Assert
             Assert.AreEqual(1, result.Count);
             Assert.AreEqual(premadeHint1.Title, result.Keys.ToArray()[0].Title);
             Assert.AreEqual(premadeHint1.Body, result.Keys.ToArray()[0].Body);
         }

         [Test]
         public async Task RequestHintsFromLocalSource()
         {
             // Arrange
             var sourceSceneAddressable = "LoadingScreenV2LocalHintsJsonSource";
             IAddressableResourceProvider  addressableProvider = new AddressableResourceProvider();
             var remoteHintSource = new LocalHintRequestSource(sourceSceneAddressable, SourceTag.Dcl, addressableProvider);
             hintRequestSources.Add(remoteHintSource);

             // Act
             var result = await hintRequestService.RequestHintsFromSources(cancellationToken, 5);

             // Assert
             Assert.AreEqual(1, result.Count);
         }

         [Test]
         public async Task HandleCancellationDuringHintDownload()
         {
             // Arrange
             var cts = new CancellationTokenSource();
             cts.Cancel();

             UniTask<Texture2D> ReturnTx2D(CallInfo x)
             {
                 return UniTask.FromResult(new Texture2D(2, 2));
             }

             hintTextureRequestHandler.DownloadTexture(Arg.Any<string>(), Arg.Any<CancellationToken>())
                                      .Returns(ReturnTx2D);

             // Act
             var result = await hintRequestService.RequestHintsFromSources(cts.Token, 5);

             // Assert
             Assert.IsEmpty(result);
         }

    }
}
