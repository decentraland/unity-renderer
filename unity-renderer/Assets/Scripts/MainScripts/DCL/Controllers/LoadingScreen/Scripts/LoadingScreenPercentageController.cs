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
        private readonly DataStore_Common commonDataStore;

        private Vector2Int currentDestination;
        private IParcelScene currentSceneBeingLoaded;

        public LoadingScreenPercentageController(ISceneController sceneController, LoadingScreenPercentageView loadingScreenPercentageView, DataStore_Common commonDataStore)
        {
            this.loadingScreenPercentageView = loadingScreenPercentageView;
            this.sceneController = sceneController;
            this.commonDataStore = commonDataStore;

            loadingScreenPercentageView.InitLoading();

            sceneController.OnNewSceneAdded += SceneController_OnNewSceneAdded;
        }

        public void Dispose()
        {
            sceneController.OnNewSceneAdded -= SceneController_OnNewSceneAdded;

            if (currentSceneBeingLoaded != null)
                currentSceneBeingLoaded.OnLoadingStateUpdated -= StatusUpdate;
        }

        private void SceneController_OnNewSceneAdded(IParcelScene scene)
        {
            //We will only update the percentage of the current destination scene. It may be the only one we care about
            if (scene != null &&
                Environment.i.world.state.GetSceneNumberByCoords(currentDestination).Equals(scene.sceneData.sceneNumber))
            {
                currentSceneBeingLoaded = scene;
                currentSceneBeingLoaded.OnLoadingStateUpdated += StatusUpdate;
            }
        }

        private void StatusUpdate(float percentage,  int currentLoadedObjects, int totalObjects, float sizeDownloadedInMB, float totalSizeInMB)
        {
            Debug.Log("AAAA " + sizeDownloadedInMB);
            Debug.Log("BBBB " + totalSizeInMB);

            loadingScreenPercentageView.SetSceneLoadingMessage();
            loadingScreenPercentageView.SetLoadingPercentage((int)percentage, currentLoadedObjects, totalObjects, sizeDownloadedInMB, totalSizeInMB);
            if (currentSceneBeingLoaded != null && percentage >= 100)
            {
                currentSceneBeingLoaded.OnLoadingStateUpdated -= StatusUpdate;
                currentSceneBeingLoaded = null;
            }
        }

        public void StartLoading(Vector2Int newDestination)
        {
            loadingScreenPercentageView.gameObject.SetActive(true);
            currentDestination = newDestination;
            loadingScreenPercentageView.InitLoading();
        }

        public void SetAvatarLoadingMessage() =>
            loadingScreenPercentageView.SetPlayerLoadingMessage();
    }
}
