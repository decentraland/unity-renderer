using DCL.Helpers;
using DCL.Interface;
using DCL.NotificationModel;
using System;
using UnityEngine;

namespace DCL.LoadingScreen
{
    /// <summary>
    /// Controls the state of the loading screen. It's responsibility is to update the view depending on the SceneController state
    /// Creates and provides the controllers associated to the LoadingScreen: TipsController and PercentageController
    /// </summary>
    public class LoadingScreenController : IDisposable
    {
        private readonly ILoadingScreenView view;
        private readonly ISceneController sceneController;
        private readonly DataStore_Player playerDataStore;
        private readonly DataStore_Common commonDataStore;
        private readonly DataStore_LoadingScreen loadingScreenDataStore;
        private readonly DataStore_Realm realmDataStore;
        private readonly IWorldState worldState;

        private Vector2Int currentDestination;
        private string currentRealm;
        private bool currentRealmIsWorld;
        private readonly LoadingScreenTipsController tipsController;
        private readonly LoadingScreenPercentageController percentageController;
        internal readonly LoadingScreenTimeoutController timeoutController;
        private readonly NotificationsController notificationsController;
        private bool onSignUpFlow;
        internal bool showRandomPositionNotification;

        public LoadingScreenController(ILoadingScreenView view, ISceneController sceneController, IWorldState worldState, NotificationsController notificationsController,
            DataStore_Player playerDataStore, DataStore_Common commonDataStore, DataStore_LoadingScreen loadingScreenDataStore, DataStore_Realm realmDataStore)
        {
            this.view = view;
            this.sceneController = sceneController;
            this.playerDataStore = playerDataStore;
            this.commonDataStore = commonDataStore;
            this.worldState = worldState;
            this.loadingScreenDataStore = loadingScreenDataStore;
            this.realmDataStore = realmDataStore;
            this.notificationsController = notificationsController;

            tipsController = new LoadingScreenTipsController(view.GetTipsView());
            percentageController = new LoadingScreenPercentageController(sceneController, view.GetPercentageView(), commonDataStore);
            timeoutController = new LoadingScreenTimeoutController(view.GetTimeoutView(), worldState, this);

            this.playerDataStore.lastTeleportPosition.OnChange += TeleportRequested;
            this.commonDataStore.isSignUpFlow.OnChange += OnSignupFlow;
            this.sceneController.OnReadyScene += ReadyScene;
            view.OnFadeInFinish += FadeInFinished;

            // The initial loading has still a destination to set. We are starting the timeout for the
            // websocket initialization
            timeoutController.StartTimeout(new Vector2Int(-1, -1));
        }

        public void Dispose()
        {
            view.Dispose();
            percentageController.Dispose();
            timeoutController.Dispose();

            playerDataStore.lastTeleportPosition.OnChange -= TeleportRequested;
            commonDataStore.isSignUpFlow.OnChange -= OnSignupFlow;
            sceneController.OnReadyScene -= ReadyScene;
            view.OnFadeInFinish -= FadeInFinished;
        }

        private void FadeInFinished(ShowHideAnimator obj)
        {
            loadingScreenDataStore.decoupledLoadingHUD.visible.Set(true);
        }

        private void ReadyScene(int obj)
        {
            //We have to check that the latest scene loaded is the one from our current destination
            if (worldState.GetSceneNumberByCoords(currentDestination).Equals(obj))
                HandlePlayerLoading();
        }

        //We have to add one more check not to show the loadingScreen unless the player is loaded
        private void PlayerLoaded(bool loaded, bool _)
        {
            if (loaded)
                FadeOutView();

            commonDataStore.isPlayerRendererLoaded.OnChange -= PlayerLoaded;
        }

        private void OnSignupFlow(bool current, bool previous)
        {
            onSignUpFlow = current;

            if (current)
                FadeOutView();
            else
                view.FadeIn(false, false);
        }

        private void TeleportRequested(Vector3 current, Vector3 previous)
        {
            if (onSignUpFlow) return;

            Vector2Int currentDestinationCandidate = Utils.WorldToGridPosition(current);

            if (IsNewRealm() || IsNewScene(currentDestinationCandidate))
            {
                currentDestination = currentDestinationCandidate;

                //On a teleport, to copy previos behaviour, we disable tips entirely and show the teleporting screen
                //This is probably going to change with the integration of WORLDS loading screen
                //Temporarily removing tips until V2
                //tipsController.StopTips();
                percentageController.StartLoading(currentDestination);
                timeoutController.StartTimeout(currentDestination);
                view.FadeIn(false, true);
            }
            else if (IsSceneLoaded(currentDestinationCandidate))
                HandlePlayerLoading();
        }

        //The realm gets changed before the scenes starts to unload. So, if we try to teleport to a world scene in which the destination coordinates are loaded,
        //we wont see the loading screen. Same happens when leaving a world. Thats why we need to keep track of the latest realmName as well as if it is a world.
        private bool IsNewRealm()
        {
            //Realm has not been set yet, so we are not in a new realm
            if (realmDataStore.playerRealmAboutConfiguration.Get() == null)
                return false;

            bool realmChangeRequiresLoadingScreen;

            if (commonDataStore.isWorld.Get())
                realmChangeRequiresLoadingScreen = string.IsNullOrEmpty(currentRealm) || !currentRealm.Equals(realmDataStore.playerRealmAboutConfiguration.Get().RealmName);
            else
                realmChangeRequiresLoadingScreen = currentRealmIsWorld;

            currentRealm = realmDataStore.playerRealmAboutConfiguration.Get().RealmName;
            currentRealmIsWorld = commonDataStore.isWorld.Get();
            return realmChangeRequiresLoadingScreen;
        }

        //If the destination scene is not loaded, we show the teleport screen. THis is called in the POSITION_UNSETTLED
        //On the other hand, the POSITION_SETTLED event is called; but since the scene will already be loaded, the loading screen wont be shown
        private bool IsNewScene(Vector2Int currentDestinationCandidate) =>
            worldState.GetSceneNumberByCoords(currentDestinationCandidate).Equals(-1);

        private bool IsSceneLoaded(Vector2Int candidate) =>
            worldState.GetScene(worldState.GetSceneNumberByCoords(candidate))?.loadingProgress >= 100;

        private void HandlePlayerLoading()
        {
            //We have to check if the player is loaded
            if (commonDataStore.isPlayerRendererLoaded.Get())
                FadeOutView();
            else
            {
                percentageController.SetAvatarLoadingMessage();
                commonDataStore.isPlayerRendererLoaded.OnChange += PlayerLoaded;
            }
        }

        private void FadeOutView()
        {
            timeoutController.StopTimeout();
            view.FadeOut();
            loadingScreenDataStore.decoupledLoadingHUD.visible.Set(false);
            if (showRandomPositionNotification)
                ShowRandomPositionNotification();
        }

        private void ShowRandomPositionNotification()
        {
            notificationsController.ShowNotification(new Model
            {
                message = "There was an error while trying to load your home scene. If the problem persists contact support.",
                type = NotificationModel.Type.ERROR,
                timer = 10f,
                destroyOnFinish = true
            });
            showRandomPositionNotification = false;
        }

        public void RandomPositionRequested()
        {
            WebInterface.SendChatMessage(new ChatMessage
            {
                messageType = ChatMessage.Type.NONE,
                recipient = string.Empty,
                body = "/goto random",
            });
            showRandomPositionNotification = true;
        }
    }
}
