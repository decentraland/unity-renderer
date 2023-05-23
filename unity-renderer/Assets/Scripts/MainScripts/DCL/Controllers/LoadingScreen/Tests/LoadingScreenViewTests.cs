using DCL.Providers;
using DCLPlugins.LoadingScreenPlugin;
using NUnit.Framework;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace DCL.LoadingScreen.Test
{

    public class LoadingScreenViewTests
    {
        private LoadingScreenView loadingScreenView;

        [SetUp]
        public void SetUp()
        {
            loadingScreenView = LoadingScreenPlugin.CreateLoadingScreenView();
        }

        [TearDown]
        public void TearDown()
        {
            loadingScreenView.Dispose();
        }

        [Category("EditModeCI")]
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

        [Category("EditModeCI")]
        [Test]
        public void LoadingTipsUpdatedCorrectly()
        {
            //Arrange
            LoadingScreenTipsView loadingScreenTipsView = loadingScreenView.GetTipsView();

            Sprite testSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/Scripts/MainScripts/DCL/Controllers/LoadingScreen/Sprites/BuilderImg.png");
            LoadingTip newLoadingTip = new LoadingTip("LoadingTest", testSprite);

            //Act
            loadingScreenTipsView.ShowTip(newLoadingTip);

            //Assert
            Assert.AreEqual(loadingScreenTipsView.tipsImage.sprite, testSprite);
            Assert.AreEqual(loadingScreenTipsView.tipsText.text, "LoadingTest");
        }
    }
}
