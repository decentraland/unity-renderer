using DCL;
using DCL.Controllers;
using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityGLTF;

namespace DCL
{
    /// <summary>
    /// This class recopiles all the needed information to be sent to the kernel and be able to show the feedback along the world loading.
    /// </summary>
    public class LoadingFeedbackController
    {
        private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;
        private  bool isDecoupledLoadingScreenEnabled;

        private class SceneLoadingStatus
        {
            public int sceneInstanceId;
            public int componentsLoading;
        }

        private List<SceneLoadingStatus> loadedScenes;

        private int currentComponentsLoading = 0;
        private int loadingComponentsPercentage = 0;
        private int maxLoadingComponentsRef = 0;
        private int maxLoadingCalculatedPercentage = 0;

        private int totalActiveDownloads = 0;
        private int downloadingAssetsPercentage = 0;
        private int maxDownloadingAssetsRef = 0;
        private int maxDownloadingCalculatedPercentage = 0;

        public LoadingFeedbackController()
        {
            DataStore.i.featureFlags.flags.OnChange += FeatureFlagsSet;
        }

        private void FeatureFlagsSet(FeatureFlag current, FeatureFlag _)
        {
            DataStore.i.featureFlags.flags.OnChange -= FeatureFlagsSet;

            isDecoupledLoadingScreenEnabled = current.IsFeatureEnabled(DataStore.i.featureFlags.DECOUPLED_LOADING_SCREEN_FF);
            if (!isDecoupledLoadingScreenEnabled)
            {
                loadedScenes = new List<SceneLoadingStatus>();

                //Add this null check is quite bad. But we are deleting this class soon, so it does not generate any tech debt
                if(Environment.i.world.sceneController != null) Environment.i.world.sceneController.OnNewSceneAdded += SceneController_OnNewSceneAdded;
                GLTFComponent.OnDownloadingProgressUpdate += GLTFComponent_OnDownloadingProgressUpdate;
                AssetPromise_AB.OnDownloadingProgressUpdate += AssetPromise_AB_OnDownloadingProgressUpdate;
                CommonScriptableObjects.rendererState.OnChange += RendererState_OnChange;
            }
        }

        public void Dispose()
        {
            if (!isDecoupledLoadingScreenEnabled)
            {
                Environment.i.world.sceneController.OnNewSceneAdded -= SceneController_OnNewSceneAdded;
                GLTFComponent.OnDownloadingProgressUpdate -= GLTFComponent_OnDownloadingProgressUpdate;
                AssetPromise_AB.OnDownloadingProgressUpdate -= AssetPromise_AB_OnDownloadingProgressUpdate;
                CommonScriptableObjects.rendererState.OnChange -= RendererState_OnChange;
            }
        }

        private void SceneController_OnNewSceneAdded(IParcelScene scene)
        {
            var parcelScene = scene as ParcelScene;

            if (parcelScene == null)
                return;

            parcelScene.sceneLifecycleHandler.OnStateRefreshed += Scene_OnStateRefreshed;
        }

        private void Scene_OnStateRefreshed(ParcelScene scene)
        {
            SceneLoadingStatus refreshedScene = new SceneLoadingStatus
            {
                sceneInstanceId = scene.GetInstanceID(),
                componentsLoading = scene.sceneLifecycleHandler.sceneResourcesLoadTracker.pendingResourcesCount
            };

            switch (scene.sceneLifecycleHandler.state)
            {
                case SceneLifecycleHandler.State.WAITING_FOR_COMPONENTS:
                    AddOrUpdateLoadedScene(refreshedScene);
                    break;
                case SceneLifecycleHandler.State.READY:
                    scene.sceneLifecycleHandler.OnStateRefreshed -= Scene_OnStateRefreshed;
                    break;
            }

            RefreshFeedbackMessage();
        }

        private void AddOrUpdateLoadedScene(SceneLoadingStatus scene)
        {
            SceneLoadingStatus existingScene = loadedScenes.FirstOrDefault(x => x.sceneInstanceId == scene.sceneInstanceId);
            if (existingScene == null)
                loadedScenes.Add(scene);
            else
            {
                existingScene.componentsLoading = scene.componentsLoading;
            }
        }

        private void GLTFComponent_OnDownloadingProgressUpdate()
        {
            RefreshFeedbackMessage();
        }

        private void AssetPromise_AB_OnDownloadingProgressUpdate()
        {
            RefreshFeedbackMessage();
        }

        private void RefreshFeedbackMessage()
        {
            if (!dataStoreLoadingScreen.Ref.loadingHUD.fadeIn.Get() && !dataStoreLoadingScreen.Ref.loadingHUD.visible.Get())
                return;

            string loadingText = string.Empty;
            string secondLoadingText = string.Empty;
            DCL.Interface.WebInterface.LoadingFeedbackMessage messageToSend = new WebInterface.LoadingFeedbackMessage();
            messageToSend.loadPercentage = 0;

            currentComponentsLoading = loadedScenes.Sum(x => x.componentsLoading);
            if (currentComponentsLoading > 0)
            {
                loadingComponentsPercentage = GetLoadingComponentsPercentage(currentComponentsLoading);
                messageToSend.loadPercentage = loadingComponentsPercentage;
                dataStoreLoadingScreen.Ref.loadingHUD.percentage.Set(loadingComponentsPercentage);
                loadingText = string.Format("Loading scenes {0}%", loadingComponentsPercentage);
            }

            totalActiveDownloads = AssetPromiseKeeper_GLTF.i.waitingPromisesCount +
                                   AssetPromiseKeeper_AB.i.waitingPromisesCount;
            if (totalActiveDownloads > 0)
            {
                downloadingAssetsPercentage = GetDownloadingAssetsPercentage(totalActiveDownloads);
                secondLoadingText = string.Format("Downloading images, 3D models, and sounds {0}%",
                    downloadingAssetsPercentage);

                if (!string.IsNullOrEmpty(loadingText))
                {
                    loadingText += "\n";
                }

                loadingText += secondLoadingText;
            }

            if (!string.IsNullOrEmpty(loadingText))
            {
                dataStoreLoadingScreen.Ref.loadingHUD.message.Set(loadingText);
                messageToSend.message = loadingText;
                WebInterface.ScenesLoadingFeedback(messageToSend);
            }
        }

        private int GetLoadingComponentsPercentage(int currentComponentsLoading)
        {
            if (currentComponentsLoading > maxLoadingComponentsRef)
                maxLoadingComponentsRef = currentComponentsLoading;

            int result = Mathf.FloorToInt(100f - (currentComponentsLoading * 100f / maxLoadingComponentsRef));
            if (result > maxLoadingCalculatedPercentage)
                maxLoadingCalculatedPercentage = result;

            return maxLoadingCalculatedPercentage;
        }

        private int GetDownloadingAssetsPercentage(int totalActiveDownloads)
        {
            if (totalActiveDownloads > maxDownloadingAssetsRef)
                maxDownloadingAssetsRef = totalActiveDownloads;

            int result = Mathf.FloorToInt(100f - (totalActiveDownloads * 100f / maxDownloadingAssetsRef));
            if (result > maxDownloadingCalculatedPercentage)
                maxDownloadingCalculatedPercentage = result;

            return maxDownloadingCalculatedPercentage;
        }

        private void RendererState_OnChange(bool current, bool previous)
        {
            if (!current)
                return;

            loadedScenes.Clear();
            maxLoadingComponentsRef = 0;
            maxLoadingCalculatedPercentage = 0;
            maxDownloadingAssetsRef = 0;
            maxDownloadingCalculatedPercentage = 0;
        }
    }
}
