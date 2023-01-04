using DCL.Controllers;
using System;
using UnityEngine;

namespace DCL.LoadingScreen
{
    /// <summary>
    /// Loading screen percentage updater. The responsibility of this class is to update the loading bar only
    /// listening to the destination scene
    /// </summary>
    public class LoadingScreenPercentageController : IDisposable
    {

        private readonly LoadingScreenPercentageView loadingScreenPercentageView;
        private readonly ISceneController sceneController;

        private Vector2Int currentDestination;
        private ParcelScene currentScene;


        public LoadingScreenPercentageController(ISceneController sceneController, LoadingScreenPercentageView loadingScreenPercentageView)
        {
            this.loadingScreenPercentageView = loadingScreenPercentageView;
            this.sceneController = sceneController;

            loadingScreenPercentageView.gameObject.SetActive(false);

            sceneController.OnNewSceneAdded += SceneController_OnNewSceneAdded;
        }

        public void Dispose()
        {
            sceneController.OnNewSceneAdded -= SceneController_OnNewSceneAdded;
            if (currentScene)
            {
                currentScene.sceneLifecycleHandler.sceneResourcesLoadTracker.OnResourcesStatusUpdate -= ResourcesStatusUpdate;
                currentScene.sceneLifecycleHandler.OnStateRefreshed -= Scene_OnStateRefreshed;
            }
        }

        private void SceneController_OnNewSceneAdded(IParcelScene scene)
        {
            currentScene = scene as ParcelScene;
            //We will only update the percentage of the current destination scene. It may be the only one we care about
            if (currentScene != null &&
                Environment.i.world.state.GetSceneNumberByCoords(currentDestination).Equals(currentScene.sceneData.sceneNumber))
            {
                loadingScreenPercentageView.SetLoadingPercentage(0);
                loadingScreenPercentageView.SetLoadingMessage($"Downloading Assets...0%");
                currentScene.sceneLifecycleHandler.sceneResourcesLoadTracker.OnResourcesStatusUpdate += ResourcesStatusUpdate;
                currentScene.sceneLifecycleHandler.OnStateRefreshed += Scene_OnStateRefreshed;
            }
        }

        private void Scene_OnStateRefreshed(ParcelScene scene)
        {
            switch (scene.sceneLifecycleHandler.state)
            {
                case SceneLifecycleHandler.State.READY:
                    currentScene.sceneLifecycleHandler.sceneResourcesLoadTracker.OnResourcesStatusUpdate -= ResourcesStatusUpdate;
                    currentScene.sceneLifecycleHandler.OnStateRefreshed -= Scene_OnStateRefreshed;
                    currentScene = null;
                    break;
            }
        }

        private void ResourcesStatusUpdate()
        {
            int downloadPercentage = (int)currentScene.sceneLifecycleHandler.sceneResourcesLoadTracker.loadingProgress;
            loadingScreenPercentageView.SetLoadingPercentage(downloadPercentage/100f);
            loadingScreenPercentageView.SetLoadingMessage($"Downloading Assets...{downloadPercentage}%");
        }

        public void StartLoading(Vector2Int newDestination)
        {
            loadingScreenPercentageView.gameObject.SetActive(true);
            currentDestination = newDestination;
            loadingScreenPercentageView.SetLoadingPercentage(0);
            loadingScreenPercentageView.SetLoadingMessage("Fetching scene...");
        }

    }
}
