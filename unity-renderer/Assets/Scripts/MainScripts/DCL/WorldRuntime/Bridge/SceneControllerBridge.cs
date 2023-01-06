using DCL.Helpers;
using DCL.Models;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Environment = DCL.Environment;

public class SceneControllerBridge : MonoBehaviour
{
    public void LoadParcelScenes(string payload) { Environment.i.world.sceneController.LoadParcelScenes(payload); }

    public void SendSceneMessage(string payload) { Environment.i.world.sceneController.SendSceneMessage(payload); }
    
    // sceneNumber comes as a string because WebSocketCommunication can only receive strings as kernel message parameters
    public void UnloadSceneV2(string sceneNumber)
    {
        if (!Int32.TryParse(sceneNumber, out int targetSceneNumber))
        {
            Debug.LogError($"UnloadSceneV2() Int32 failed to parse the received scene number...{sceneNumber}.");
            return;
        }
        
        Environment.i.world.sceneController.UnloadScene(targetSceneNumber);
    }

    public void CreateGlobalScene(string payload)
    {
        CreateGlobalSceneMessage globalScene = Utils.SafeFromJson<CreateGlobalSceneMessage>(payload);
        Environment.i.world.sceneController.CreateGlobalScene(globalScene);
    }

    public void UpdateParcelScenes(string payload) { Environment.i.world.sceneController.UpdateParcelScenes(payload); }
    
    // TODO: Move to Builder Bridge
    public void BuilderReady() { UnityEngine.SceneManagement.SceneManager.LoadScene("BuilderScene", UnityEngine.SceneManagement.LoadSceneMode.Additive); }
}