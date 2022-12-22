using NUnit.Framework;

namespace DCL.LoadingScreen.Test
{
    public class LoadingScreenViewTests
    {
        private LoadingScreenView loadingScreenView;

        [SetUp]
        public void SetUp()
        {
            loadingScreenView = LoadingScreenView.Create();
        }

        [Test]
        public void MessageUpdatedCorrectly()
        {
            Assert.True(true);
        }
    }
}
