using DCL;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class SceneController : MonoBehaviour
{
    public static SceneController i { get; private set; }

    public bool startDecentralandAutomatically = true;

    [FormerlySerializedAs("factoryManifest")]
    public DCLComponentFactory componentFactory;

    public Dictionary<string, ParcelScene> loadedScenes = new Dictionary<string, ParcelScene>();

    void Awake()
    {
        if (i != null)
        {
            Destroy(this);
            return;
        }

        i = this;

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

            if (!loadedScenes.ContainsKey(sceneToLoad.id))
            {
                var newGameObject = new GameObject("New Scene");

                var newScene = newGameObject.AddComponent<ParcelScene>();
                newScene.SetData(sceneToLoad);
                newScene.ownerController = this;

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
                Utils.SafeDestroy(scene.gameObject);
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
            if (chunks[i].Length > 0)
            {
                var separatorPosition = chunks[i].IndexOf('\t');

                if (separatorPosition == -1) continue;

                var sceneId = chunks[i].Substring(0, separatorPosition);
                var message = chunks[i].Substring(separatorPosition + 1);
                ProcessMessage(sceneId, message);
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
                case "RemoveEntity": scene.RemoveEntity(payload); break;
                default:
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    throw new UnityException($"Unknown method {method}");
#else
          return;
#endif
            }
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        else
        {
            throw new UnityException($"Scene not found {sceneId}");
        }
#endif
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
        newScene.ownerController = this;

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
