using NSubstitute;
using NUnit.Framework;
using UnityEditor.VersionControl;

namespace DCL.LoadingScreen.Test
{
    public class LoadingScreenControllerTests
    {
        private ILoadingScreenView loadingScreenView;
        private ISceneController sceneController;
        private LoadingScreenController loadingScreenController;

        [SetUp]
        private void SetUp()
        {
            loadingScreenView = Substitute.For<ILoadingScreenView>();
            sceneController = Substitute.For<ISceneController>();
            loadingScreenController = new LoadingScreenController(loadingScreenView, sceneController);
        }

        [Test]
        private void OnSceneAdded()
        {
            Assert.True(true);
        }

        [Test]
        private void OnSceneRemoved()
        {
            Assert.True(true);
        }

        [Test]
        private void OnSceneLoading()
        {
            Assert.True(true);
        }
    }
}
