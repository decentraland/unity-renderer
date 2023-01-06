using DCL.Helpers;
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
        private readonly IWorldState worldState;

        private Vector2Int currentDestination;
        private string currentRealm;
        private readonly LoadingScreenTipsController tipsController;
        private readonly LoadingScreenPercentageController percentageController;

        public LoadingScreenController(ILoadingScreenView view, ISceneController sceneController, DataStore_Player playerDataStore, DataStore_Common commonDataStore, DataStore_LoadingScreen loadingScreenDataStore,
            IWorldState worldState)
        {
            this.view = view;
            this.sceneController = sceneController;
            this.playerDataStore = playerDataStore;
            this.commonDataStore = commonDataStore;
            this.worldState = worldState;
            this.loadingScreenDataStore = loadingScreenDataStore;

            tipsController = new LoadingScreenTipsController(view.GetTipsView());
            percentageController = new LoadingScreenPercentageController(sceneController, view.GetPercentageView());

            this.playerDataStore.lastTeleportPosition.OnChange += TeleportRequested;
            this.commonDataStore.isSignUpFlow.OnChange += OnSignupFlow;
            this.sceneController.OnReadyScene += ReadyScene;
            view.OnFadeInFinish += FadeInFinished;

            loadingScreenDataStore.decoupledLoadingHUD.visible.Set(true);

            tipsController.StartTips();
        }

        public void Dispose()
        {
            view.Dispose();
            percentageController.Dispose();
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
                FadeOutView();
        }

        private void OnSignupFlow(bool current, bool previous)
        {
            if (current)
                FadeOutView();
            else
                //Blit not necessary since we wont be hiding the Terms&Condition menu until full fade in
                view.FadeIn(false, false);
        }

        private void TeleportRequested(Vector3 current, Vector3 previous)
        {
            Vector2Int currentDestinationCandidate = Utils.WorldToGridPosition(current);

            //If the destination scene is not loaded, we show the teleport screen
            //This is going to be called also on the POSITION_SETTLED event; but since the scene will already be loaded, the loading screen wont be shown
            if (worldState.GetSceneNumberByCoords(currentDestinationCandidate).Equals(-1))
            {
                currentDestination = currentDestinationCandidate;

                view.FadeIn(false, true);

                //On a teleport, to copy previos behaviour, we disable tips entirely and show the teleporting screen
                //This is probably going to change with the integration of WORLDS loading screen
                tipsController.StopTips();
                percentageController.StartLoading(currentDestination);
            }
        }

        private void FadeOutView()
        {
            view.FadeOut();
            loadingScreenDataStore.decoupledLoadingHUD.visible.Set(false);
        }
    }
}
