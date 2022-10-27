using System.Collections;
using DCL;
using UnityEngine;

public class SceneControllerBridge : MonoBehaviour
{
    public void LoadParcelScenes(string payload) { Environment.i.world.sceneController.LoadParcelScenes(payload); }

    public void SendSceneMessage(string payload) { Environment.i.world.sceneController.SendSceneMessage(payload); }

    public void UnloadScene(string sceneId)
    {
        StartCoroutine(AnalyzeLoadingHUDState(sceneId));
    }

    IEnumerator AnalyzeLoadingHUDState(string sceneId)
    {
        if(DataStore.i.HUDs.loadingHUD.fadeIn.Get())
        {
            yield return new UnityEngine.WaitUntil(() => CommonScriptableObjects.isLoadingHUDOpen.Get());

        }
        Environment.i.world.sceneController.UnloadScene(sceneId);
    }

    public void CreateGlobalScene(string payload) { Environment.i.world.sceneController.CreateGlobalScene(payload); }

    public void UpdateParcelScenes(string payload) { Environment.i.world.sceneController.UpdateParcelScenes(payload); }
    
    // TODO: Move to Builder Bridge
    public void BuilderReady() { UnityEngine.SceneManagement.SceneManager.LoadScene("BuilderScene", UnityEngine.SceneManagement.LoadSceneMode.Additive); }
}
