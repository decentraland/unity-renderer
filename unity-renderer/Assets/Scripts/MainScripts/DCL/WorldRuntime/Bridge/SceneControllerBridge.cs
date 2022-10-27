using System;
using UnityEngine;
using Environment = DCL.Environment;

public class SceneControllerBridge : MonoBehaviour
{
    public void LoadParcelScenes(string payload) { Environment.i.world.sceneController.LoadParcelScenes(payload); }

    public void SendSceneMessage(string payload) { Environment.i.world.sceneController.SendSceneMessage(payload); }

    public void UnloadScene(string sceneId)
    {
        // Environment.i.world.sceneController.UnloadScene(sceneId);
    }
    
    // TODO: Are PEXes unloaded with UnloadScene as well? do they have sceneNumber besides their mandatory scene id? otherwise we will have to crate an UnloadScene(string) overload for those.
    // sceneNumber comes as a string because WebSocketCommunication can only receive strings as kernel message parameters
    public void UnloadSceneV2(string sceneNumber) { Environment.i.world.sceneController.UnloadScene(Int32.Parse(sceneNumber)); }

    public void CreateGlobalScene(string payload) { Environment.i.world.sceneController.CreateGlobalScene(payload); }

    public void UpdateParcelScenes(string payload) { Environment.i.world.sceneController.UpdateParcelScenes(payload); }
    
    // TODO: Move to Builder Bridge
    public void BuilderReady() { UnityEngine.SceneManagement.SceneManager.LoadScene("BuilderScene", UnityEngine.SceneManagement.LoadSceneMode.Additive); }
}