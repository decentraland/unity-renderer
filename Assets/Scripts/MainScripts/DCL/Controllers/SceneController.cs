using DCL.Controllers;
using DCL.Interface;
using DCL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public bool startDecentralandAutomatically = true;

    public Dictionary<string, ParcelScene> loadedScenes = new Dictionary<string, ParcelScene>();

    void Awake()
    {

        // We trigger the Decentraland logic once SceneController has been instanced and is ready to act.
        if (startDecentralandAutomatically)
        {
            WebInterface.StartDecentraland();
        }
    }

    void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetMouseButtonDown(0))
            {
                LockCursor();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }
    }

    void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    ParcelScene GetDecentralandSceneOfParcel(Vector2Int parcel)
    {
        foreach (var estate in loadedScenes)
        {
            if (estate.Value.sceneData.basePosition.Equals(parcel))
            {
                return estate.Value;
            }

            foreach (var iteratedParcel in estate.Value.sceneData.parcels)
            {
                if (iteratedParcel.Equals(parcel))
                {
                    return estate.Value;
                }
            }
        }
        return null;
    }

    private LoadParcelScenesMessage loadParcelScenesMessage = new LoadParcelScenesMessage();

    public void LoadParcelScenes(string decentralandSceneJSON)
    {
        JsonUtility.FromJsonOverwrite(decentralandSceneJSON, this.loadParcelScenesMessage);

        var scenesToLoad = loadParcelScenesMessage.parcelsToLoad;
        var completeListOfParcelsThatShouldBeLoaded = new List<string>();

        // LOAD MISSING SCENES
        for (int i = 0; i < scenesToLoad.Count; i++)
        {
            var sceneToLoad = scenesToLoad[i];

            completeListOfParcelsThatShouldBeLoaded.Add(sceneToLoad.id);

            if (GetDecentralandSceneOfParcel(sceneToLoad.basePosition) == null)
            {
                var newGameObject = new GameObject("New Scene");

                var newScene = newGameObject.AddComponent<ParcelScene>();
                newScene.SetData(sceneToLoad);

                if (!loadedScenes.ContainsKey(sceneToLoad.id))
                {
                    loadedScenes.Add(sceneToLoad.id, newScene);
                }
                else
                {
                    loadedScenes[sceneToLoad.id] = newScene;
                }
            }
        }

        // UNLOAD EXTRA SCENES
        var loadedScenesClone = loadedScenes.ToArray();

        for (int i = 0; i < loadedScenesClone.Length; i++)
        {
            var loadedScene = loadedScenesClone[i];
            if (!completeListOfParcelsThatShouldBeLoaded.Contains(loadedScene.Key))
            {
                UnloadScene(loadedScene.Key);
            }
        }
    }

    public void UnloadScene(string sceneKey)
    {
        if (loadedScenes.ContainsKey(sceneKey))
        {
            var scene = loadedScenes[sceneKey];

            loadedScenes.Remove(sceneKey);

            if (scene)
            {
#if UNITY_EDITOR
                DestroyImmediate(scene);
#else
        Destroy(scene);
#endif
            }
        }
    }

    public void UnloadAllScenes()
    {
        var list = loadedScenes.ToArray();
        for (int i = 0; i < list.Length; i++)
        {
            UnloadScene(list[i].Key);
        }
    }

    public void SendSceneMessage(string payload)
    {
        var chunks = payload.Split('\n');

        for (int i = 0; i < chunks.Length; i++)
        {
            try
            {
                if (chunks[i].Length > 0)
                {
                    var separatorPosition = chunks[i].IndexOf('\t');

                    if (separatorPosition == -1)
                    {
                        continue;
                    }

                    var sceneId = chunks[i].Substring(0, separatorPosition);
                    var message = chunks[i].Substring(separatorPosition + 1);
                    ProcessMessage(sceneId, message);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    public void ProcessMessage(string sceneId, string message)
    {
        ParcelScene scene;

        if (loadedScenes.TryGetValue(sceneId, out scene))
        {
            var separatorPosition = message.IndexOf('\t');
            var method = message.Substring(0, separatorPosition);
            var payload = message.Substring(separatorPosition + 1);
            switch (method)
            {
                case "CreateEntity": scene.CreateEntity(payload); break;
                case "SetEntityParent": scene.SetEntityParent(payload); break;

                //NOTE(Brian): EntityComponent messages
                case "UpdateEntityComponent": scene.EntityComponentCreate(payload); break;
                case "ComponentRemoved": scene.EntityComponentRemove(payload); break;

                //NOTE(Brian): SharedComponent messages
                case "AttachEntityComponent": scene.SharedComponentAttach(payload); break;
                case "ComponentCreated": scene.SharedComponentCreate(payload); break;
                case "ComponentDisposed": scene.SharedComponentDispose(payload); break;
                case "ComponentUpdated": scene.SharedComponentUpdate(payload); break;
                default:
                    throw new Exception($"Unkwnown method {method}");
            }
        }
    }

    public ParcelScene CreateTestScene(LoadParcelScenesMessage.UnityParcelScene data)
    {
        if (data.basePosition == null)
        {
            data.basePosition = new Vector2Int(0, 0);
        }

        if (data.parcels == null)
        {
            data.parcels = new Vector2Int[] { data.basePosition };
        }

        if (string.IsNullOrEmpty(data.id))
        {
            data.id = $"(test):{data.basePosition.x},{data.basePosition.y}";
        }

        var go = new GameObject();
        var newScene = go.AddComponent<ParcelScene>();
        newScene.SetData(data);

        if (!loadedScenes.ContainsKey(data.id))
        {
            loadedScenes.Add(data.id, newScene);
        }
        else
        {
            throw new Exception($"Scene {data.id} is already loaded.");
        }

        return newScene;
    }
}
