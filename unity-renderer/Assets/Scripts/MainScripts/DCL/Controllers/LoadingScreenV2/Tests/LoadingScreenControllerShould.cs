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
    public class LoadingScreenControllerShould
    {
        private HintRequestService hintRequestService;
        private List<IHintRequestSource> hintRequestSources;
        private ISceneController sceneController;
        private IHintTextureRequestHandler hintTextureRequestHandler;
        private CancellationToken cancellationToken;


        [SetUp]
        public void Setup()
        {

        }

        [TearDown]
        public void TearDown()
        {
            hintRequestService.Dispose();
        }

        [Test]
        public async Task PlaceholderTest()
        {
            // Arrange

            // Act
            var result = await hintRequestService.RequestHintsFromSources(cancellationToken, 5);

            // Assert
            Assert.IsTrue(true);
        }

        [Test]
        public async Task InitializeHintView()
        {
            //Arrange
            var hint = new Hint("https://example.com/image1.png", "title1", "body1", SourceTag.Event);
            var hintView = new HintView();

            //Act
            hintView.Initialize(hint, new Texture2D(2, 2));

            //Assert
            Assert.IsFalse(hintView.isActiveAndEnabled);
            Assert.AreEqual(hintView.hintText.text, hint.Title);
            Assert.NotNull(hintView.hintImage.sprite);
        }
    }
}
