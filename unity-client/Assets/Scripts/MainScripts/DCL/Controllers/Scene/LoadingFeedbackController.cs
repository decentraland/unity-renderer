using DCL;
using DCL.Controllers;
using DCL.Interface;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityGLTF;

/// <summary>
/// This class recopiles all the needed information to be sent to the kernel and be able to show the feedback along the world loading.
/// </summary>
public class LoadingFeedbackController : MonoBehaviour
{
    private class SceneLoadingStatus
    {
        public int sceneId;
        public int componentsLoading;
    }

    private List<SceneLoadingStatus> loadedScenes;

    private void Start()
    {
        loadedScenes = new List<SceneLoadingStatus>();

        SceneController.i.OnNewSceneAdded += SceneController_OnNewSceneAdded;
        GLTFComponent.OnDownloadingProgressUpdate += GLTFComponent_OnDownloadingProgressUpdate;
        AssetPromise_AB.OnDownloadingProgressUpdate += AssetPromise_AB_OnDownloadingProgressUpdate;
        CommonScriptableObjects.rendererState.OnChange += RendererState_OnChange;
    }

    private void OnDestroy()
    {
        SceneController.i.OnNewSceneAdded -= SceneController_OnNewSceneAdded;
        GLTFComponent.OnDownloadingProgressUpdate -= GLTFComponent_OnDownloadingProgressUpdate;
        AssetPromise_AB.OnDownloadingProgressUpdate -= AssetPromise_AB_OnDownloadingProgressUpdate;
        CommonScriptableObjects.rendererState.OnChange -= RendererState_OnChange;
    }

    private void SceneController_OnNewSceneAdded(ParcelScene scene)
    {
        scene.OnStateRefreshed += Scene_OnStateRefreshed;
    }

    private void Scene_OnStateRefreshed(ParcelScene scene)
    {
        SceneLoadingStatus refreshedScene = new SceneLoadingStatus
        {
            sceneId = scene.GetInstanceID(),
            componentsLoading = scene.disposableNotReadyCount
        };

        switch (scene.state)
        {
            case ParcelScene.State.WAITING_FOR_COMPONENTS:
                AddOrUpdateLoadedScene(refreshedScene);
                break;
            case ParcelScene.State.READY:
                scene.OnStateRefreshed -= Scene_OnStateRefreshed;
                break;
        }

        RefreshFeedbackMessage();
    }

    private void AddOrUpdateLoadedScene(SceneLoadingStatus scene)
    {
        SceneLoadingStatus existingScene = loadedScenes.FirstOrDefault(x => x.sceneId == scene.sceneId);
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
        if (CommonScriptableObjects.rendererState.Get())
            return;

        string loadingText = string.Empty;
        string secondLoadingText = string.Empty;

        int currentComponentsLoading = loadedScenes.Sum(x => x.componentsLoading);
        if (currentComponentsLoading > 0)
        {
            loadingText = string.Format(
                "Loading scenes ({0} component{1} left...)",
                currentComponentsLoading,
                currentComponentsLoading > 1 ? "s" : string.Empty);
        }

        int totalActiveDownloads = AssetPromiseKeeper_GLTF.i.waitingPromisesCount + AssetPromiseKeeper_AB.i.waitingPromisesCount;
        if (totalActiveDownloads > 0)
        {
            secondLoadingText = string.Format(
                "Downloading assets ({0} asset{1} left...)",
                totalActiveDownloads,
                totalActiveDownloads > 1 ? "s" : string.Empty);

            if (!string.IsNullOrEmpty(loadingText))
            {
                loadingText += "\\n";
            }

            loadingText += secondLoadingText;
        }

        if (!string.IsNullOrEmpty(loadingText))
            WebInterface.ScenesLoadingFeedback(loadingText);
    }

    private void RendererState_OnChange(bool current, bool previous)
    {
        if (!current)
            return;

        loadedScenes.Clear();
    }
}
