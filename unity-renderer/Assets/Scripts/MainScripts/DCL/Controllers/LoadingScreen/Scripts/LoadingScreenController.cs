using DCL.Controllers;
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

        public LoadingScreenController(ILoadingScreenView view, ISceneController sceneController)
        {
            this.view = view;
            this.sceneController = sceneController;
            sceneController.OnNewSceneAdded += NewSceneAdded;
            sceneController.OnReadyScene += OnReadyScene;
            CommonScriptableObjects.isLoadingHUDOpen.Set(true);
        }

        private void NewSceneAdded(IParcelScene obj)
        {
            currentLoadingScenes++;
        }

        private void OnReadyScene(int obj)
        {
            currentLoadingScenes--;
            if (obj == 1) return;
            if (currentLoadingScenes.Equals(0))
            {
                CommonScriptableObjects.rendererState.Set(true);
                CommonScriptableObjects.isLoadingHUDOpen.Set(false);
                view.FadeOut();
            }
        }


        private void SceneRemoved() { }

        private void UpdateLoadingMessage()
        {
            view.UpdateLoadingMessage();
        }

        public void Dispose() =>
            view.Dispose();
    }
}
