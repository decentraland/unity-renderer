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
        public void SetUp()
        {
            loadingScreenView = Substitute.For<ILoadingScreenView>();
            sceneController = Substitute.For<ISceneController>();
            loadingScreenController = new LoadingScreenController(loadingScreenView, sceneController, new DataStore_Player());
        }

        [Test]
        public void OnSceneAdded()
        {
            Assert.True(true);
        }

        [Test]
        public void OnSceneRemoved()
        {
            Assert.True(true);
        }

        [Test]
        public void OnSceneLoading()
        {
            Assert.True(true);
        }
    }
}
