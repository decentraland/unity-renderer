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

        private int currentLoadingScenes;

        private Vector3 lastTeleportRequested;

        public LoadingScreenController(ILoadingScreenView view, ISceneController sceneController)
        {
            this.view = view;
            this.sceneController = sceneController;

            lastTeleportRequested = Vector3.one * float.MaxValue;
            DataStore.i.player.lastTeleportPosition.OnChange += TeleportRequested;

            //Needed since CameraController is using it to activate its component
            CommonScriptableObjects.isLoadingHUDOpen.Set(true);
            view.OnFadeInFinish += LoadingScreenVisible;
        }

        private void LoadingScreenVisible(ShowHideAnimator obj)
        {
            SetCommmonScriptableObjectRenderState(false);
        }

        public void Dispose()
        {
            view.Dispose();
            view.OnFadeInFinish -= LoadingScreenVisible;
        }

        private void TeleportRequested(Vector3 current, Vector3 previous)
        {
            if (lastTeleportRequested.Equals(current))
            {
                SetCommmonScriptableObjectRenderState(true);
                view.FadeOut();
            }
            else
                view.FadeIn();

            lastTeleportRequested = current;
        }

        private void UpdateLoadingMessage()
        {
            view.UpdateLoadingMessage();
        }

        //TODO: Get rid of the usage variables. Now we need them to activate the CameraController
        private void SetCommmonScriptableObjectRenderState(bool newRendererState)
        {
            CommonScriptableObjects.rendererState.Set(newRendererState);
            CommonScriptableObjects.isLoadingHUDOpen.Set(!newRendererState);
        }
    }
}
