using DCL.Controllers;
using DCL.Helpers;
using System;
using UnityEngine;

namespace DCL.LoadingScreen
{
    /// <summary>
    /// Controls the state of the loading screen. It's responsibility is to update the view depending on the SceneController state
    /// </summary>
    public class LoadingScreenController : IDisposable
    {
        private readonly ILoadingScreenView view;
        private readonly ISceneController sceneController;
        private DataStore_Player playerDataStore;
        private DataStore_Common commonDataStore;
        private IWorldState worldState;

        private Vector2Int currentDestination;
        public LoadingScreenController(ILoadingScreenView view, ISceneController sceneController, DataStore_Player playerDataStore, DataStore_Common commonDataStore, IWorldState worldState)
        {
            this.view = view;
            this.sceneController = sceneController;
            this.playerDataStore = playerDataStore;
            this.commonDataStore = commonDataStore;
            this.worldState = worldState;

            //Needed since CameraController is using it to activate its component
            CommonScriptableObjects.isLoadingHUDOpen.Set(true);

            this.playerDataStore.lastTeleportPosition.OnChange += TeleportRequested;
            this.commonDataStore.isSignUpFlow.OnChange += OnSignupFlow;
            this.sceneController.OnReadyScene += ReadyScene;
            view.OnFadeInFinish += FadeInFinished;
        }

        public void Dispose()
        {
            view.Dispose();
            this.playerDataStore.lastTeleportPosition.OnChange -= TeleportRequested;
            this.commonDataStore.isSignUpFlow.OnChange -= OnSignupFlow;
            this.sceneController.OnReadyScene -= ReadyScene;
            view.OnFadeInFinish -= FadeInFinished;
        }

        private void FadeInFinished(ShowHideAnimator obj)
        {
            SetCommmonScriptableObjectRenderState(false);
        }

        private void ReadyScene(int obj)
        {
            //We have to check that the latest scene loaded is the one from our current destination
            if (worldState.GetSceneNumberByCoords(currentDestination).Equals(obj))
            {
                view.FadeOut();
                SetCommmonScriptableObjectRenderState(true);
            }
        }

        private void OnSignupFlow(bool current, bool previous)
        {
            if (current)
            {
                view.FadeOut();
                SetCommmonScriptableObjectRenderState(true);
            }
            else
            {
                //Blit not necessary since we wont be hiding the Terms&Condition menu until full fade in
                view.FadeIn(false);
            }
        }

        private void TeleportRequested(Vector3 current, Vector3 previous)
        {
            Vector2Int currentDestinationCandidate = Utils.WorldToGridPosition(current);
            //The teleport is request both on the POSITION_SETTLED and POSITION_UNSETTLED events from kernel. So, if the positions are not the same, then it means we are calling to a POSITION_UNSETTLED event and we are indeed teleporting
            //Also, we need to check that the scene that has just been unsettled is not loaded. Only then, we show the loading screen
            if (!current.Equals(previous) && worldState.GetSceneNumberByCoords(currentDestinationCandidate).Equals(-1))
            {
                currentDestination = currentDestinationCandidate;
                //TODO: The blit to avoid the flash of the empty camera/the unloaded scene
                view.FadeIn(true);
            }
        }

        //TODO: Get rid of the usage variables. Now we need them to activate the CameraController
        private void SetCommmonScriptableObjectRenderState(bool newRendererState)
        {
            CommonScriptableObjects.isLoadingHUDOpen.Set(!newRendererState);
        }
    }
}
