using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using DCL.Helpers;
using DCL.Providers;
using DCLPlugins.LoadingScreenPlugin;
using Decentraland.Bff;
using System;
using System.Collections;
using UnityEngine.TestTools;

namespace DCL.LoadingScreen.Test
{
    public class LoadingScreenControllerTests
    {
        private ILoadingScreenView loadingScreenView;
        private ISceneController sceneController;
        private IWorldState worldState;
        private ILoadingScreenTimeoutView loadingScreenTimeoutView;

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
            loadingScreenTimeoutView = Substitute.For<ILoadingScreenTimeoutView>();
            sceneController = Substitute.For<ISceneController>();
            worldState = Substitute.For<IWorldState>();
            playerDataStore = new DataStore_Player();
            commonDataStore = new DataStore_Common();
            loadingScreenDataStore = new DataStore_LoadingScreen();
            realmDataStore = new DataStore_Realm();
            realmDataStore.playerRealmAboutConfiguration.Set(new AboutResponse.Types.AboutConfiguration());

            LoadingScreenView auxiliaryViews = LoadingScreenPlugin.CreateLoadingScreenView();
            loadingScreenView.GetTipsView().Returns(auxiliaryViews.GetTipsView());
            loadingScreenView.GetPercentageView().Returns(auxiliaryViews.GetPercentageView());
            loadingScreenView.GetTimeoutView().Returns(loadingScreenTimeoutView);

            worldState.GetSceneNumberByCoords(destination).Returns(-1);

            NotificationsController notificationsController = auxiliaryViews.gameObject.AddComponent<NotificationsController>();
            notificationsController.allowNotifications = false;

            loadingScreenController = new LoadingScreenController(loadingScreenView, sceneController, worldState, notificationsController, playerDataStore, commonDataStore, loadingScreenDataStore, realmDataStore);
        }

        [Category("EditModeCI")]
        [UnityTest]
        public IEnumerator HandleFirstTimeoutCorrectly() =>
            UniTask.ToCoroutine(async () =>
            {
                //Arrange
                int simulatedTimeout = 100;
                loadingScreenController.timeoutController.currentEvaluatedTimeout = simulatedTimeout;

                //Act
                playerDataStore.lastTeleportPosition.Set(Vector3.one);
                await UniTask.Delay(simulatedTimeout + 10);

                //Assert
                loadingScreenTimeoutView.Received().ShowSceneTimeout();
            });

        [Category("EditModeCI")]
        [UnityTest]
        public IEnumerator HandleRandomPositionCorrectly() =>
            UniTask.ToCoroutine(async () =>
            {
                //Arrange
                int simulatedTimeout = 100;
                loadingScreenController.timeoutController.currentEvaluatedTimeout = simulatedTimeout;

                //Act
                playerDataStore.lastTeleportPosition.Set(Vector3.one);
                await UniTask.Delay(simulatedTimeout + 10);

                //Assert
                loadingScreenTimeoutView.Received().ShowSceneTimeout();

                //Act
                loadingScreenController.timeoutController.GoBackHomeClicked();
                playerDataStore.lastTeleportPosition.Set(Vector3.one * 100);

                //Assert
                loadingScreenTimeoutView.Received().HideSceneTimeout();
                Assert.IsTrue(loadingScreenController.timeoutController.goHomeRequested);

                //Act
                playerDataStore.lastTeleportPosition.Set(Vector3.one);
                await UniTask.Delay(simulatedTimeout + 10);

                //Assert
                Assert.IsTrue(loadingScreenController.showRandomPositionNotification);
            });

        [Category("EditModeCI")]
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

        [Category("EditModeCI")]
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
