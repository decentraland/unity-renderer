using NUnit.Framework;
using UnityEngine;

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
        public void LoadingPercentageCorrectly()
        {
            //Arrange
            LoadingScreenPercentageView loadingScreenPercentageView = loadingScreenView.GetPercentageView();

            //Act
            int currentLoadingValue = 0;
            loadingScreenPercentageView.SetSceneLoadingMessage();
            loadingScreenPercentageView.SetLoadingPercentage(currentLoadingValue);

            //Assert
            Assert.AreEqual(loadingScreenPercentageView.loadingMessage.text, loadingScreenPercentageView.GetCurrentLoadingMessage(currentLoadingValue));
            Assert.AreEqual(loadingScreenPercentageView.loadingPercentage.fillAmount, currentLoadingValue/100f);

            //Act
            currentLoadingValue = 50;
            loadingScreenPercentageView.SetSceneLoadingMessage();
            loadingScreenPercentageView.SetLoadingPercentage(currentLoadingValue);

            //Assert
            Assert.AreEqual(loadingScreenPercentageView.loadingMessage.text, loadingScreenPercentageView.GetCurrentLoadingMessage(currentLoadingValue));
            Assert.AreEqual(loadingScreenPercentageView.loadingPercentage.fillAmount, currentLoadingValue/100f);
        }

        [Test]
        public void LoadingTipsUpdatedCorrectly()
        {
            //Arrange
            LoadingScreenTipsView loadingScreenTipsView = loadingScreenView.GetTipsView();
            Sprite testSprite = Resources.Load<Sprite>("TipsImgs/BuilderImg");
            LoadingTip newLoadingTip = new LoadingTip("LoadingTest", testSprite);

            //Act
            loadingScreenTipsView.ShowTip(newLoadingTip);

            //Assert
            Assert.AreEqual(loadingScreenTipsView.tipsImage.sprite, testSprite);
            Assert.AreEqual(loadingScreenTipsView.tipsText.text, "LoadingTest");
        }
    }
}
