using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using DCL.Helpers;
using System;

namespace DCL.LoadingScreen.Test
{
    public class LoadingScreenControllerTests
    {
        private ILoadingScreenView loadingScreenView;
        private ISceneController sceneController;
        private IWorldState worldState;

        private Vector2Int destination = Utils.WorldToGridPosition(Vector3.zero);

        private LoadingScreenController loadingScreenController;
        private DataStore_Player playerDataStore;
        private DataStore_Common commonDataStore;
        private DataStore_LoadingScreen loadingScreenDataStore;



        [SetUp]
        public void SetUp()
        {
            loadingScreenView = Substitute.For<ILoadingScreenView>();
            sceneController = Substitute.For<ISceneController>();
            worldState = Substitute.For<IWorldState>();
            playerDataStore = new DataStore_Player();
            commonDataStore = new DataStore_Common();
            loadingScreenDataStore = new DataStore_LoadingScreen();

            LoadingScreenView auxiliaryViews = LoadingScreenView.Create();
            loadingScreenView.GetTipsView().Returns(auxiliaryViews.GetTipsView());
            loadingScreenView.GetPercentageView().Returns(auxiliaryViews.GetPercentageView());

            worldState.GetSceneNumberByCoords(destination).Returns(-1);

            loadingScreenController = new LoadingScreenController(loadingScreenView, sceneController, playerDataStore, commonDataStore, loadingScreenDataStore, worldState);
        }

        [Test]
        public void TeleportRequestedCorrectly()
        {
            //Act
            playerDataStore.lastTeleportPosition.Set(Vector3.one);
            loadingScreenView.OnFadeInFinish += Raise.Event<Action<ShowHideAnimator>>(Arg.Any<ShowHideAnimator>());

            //Assert
            loadingScreenView.Received().FadeIn(Arg.Any<bool>(), Arg.Any<bool>());
            Assert.True(loadingScreenDataStore.decoupledLoadingHUD.visible.Get());

            //Act
            worldState.GetSceneNumberByCoords(destination).Returns(5);
            sceneController.OnReadyScene += Raise.Event<Action<int>>(5);

            //Assert
            loadingScreenView.Received().FadeOut();
            Assert.False(loadingScreenDataStore.decoupledLoadingHUD.visible.Get());
        }



        [Test]
        public void SignUpDissapearedCorrectly()
        {
            //Act
            commonDataStore.isSignUpFlow.Set(true);

            //Assert
            loadingScreenView.Received().FadeOut();
            Assert.False(loadingScreenDataStore.decoupledLoadingHUD.visible.Get());

            //Act
            commonDataStore.isSignUpFlow.Set(false);
            loadingScreenView.OnFadeInFinish += Raise.Event<Action<ShowHideAnimator>>(Arg.Any<ShowHideAnimator>());

            //Assert
            loadingScreenView.Received().FadeIn(Arg.Any<bool>(), Arg.Any<bool>());
            Assert.True(loadingScreenDataStore.decoupledLoadingHUD.visible.Get());

        }
    }
}
