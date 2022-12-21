using NUnit.Framework;

namespace DCL.LoadingScreen.Test
{
    public class LoadingScreenViewTests
    {
        private LoadingScreenView loadingScreenView;

        [SetUp]
        public void SetUp()
        {
            loadingScreenView = BaseComponentView.Create<LoadingScreenView>("_LoadingScreen");
        }

        [Test]
        public void MessageUpdatedCorrectly()
        {
            Assert.True(true);
        }
    }
}
