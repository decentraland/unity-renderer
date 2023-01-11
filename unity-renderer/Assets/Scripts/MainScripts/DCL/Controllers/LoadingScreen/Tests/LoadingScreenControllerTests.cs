using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using DCL.Helpers;
using Decentraland.Bff;
using System;
using UnityEngine.Playables;

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
        private DataStore_Realm realmDataStore;



        [SetUp]
        public void SetUp()
        {
            loadingScreenView = Substitute.For<ILoadingScreenView>();
            sceneController = Substitute.For<ISceneController>();
            worldState = Substitute.For<IWorldState>();
            playerDataStore = new DataStore_Player();
            commonDataStore = new DataStore_Common();
            loadingScreenDataStore = new DataStore_LoadingScreen();
            realmDataStore = new DataStore_Realm();
            realmDataStore.playerRealmAboutConfiguration.Set(new AboutResponse.Types.AboutConfiguration());


            LoadingScreenView auxiliaryViews = LoadingScreenView.Create();
            loadingScreenView.GetTipsView().Returns(auxiliaryViews.GetTipsView());
            loadingScreenView.GetPercentageView().Returns(auxiliaryViews.GetPercentageView());

            worldState.GetSceneNumberByCoords(destination).Returns(-1);

            NotificationsController notificationsController = auxiliaryViews.gameObject.AddComponent<NotificationsController>();
            notificationsController.allowNotifications = false;

            loadingScreenController = new LoadingScreenController(loadingScreenView, sceneController, worldState, notificationsController,playerDataStore, commonDataStore, loadingScreenDataStore,realmDataStore );
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
            commonDataStore.isPlayerRendererLoaded.Set(true);
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
