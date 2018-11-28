using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityGLTF;
using UnityEngine;
using DCL.Interface;
using DCL.Models;
using DCL.Controllers;

public class SceneController : MonoBehaviour {
  public bool startDecentralandAutomatically = true;

  public Dictionary<string, ParcelScene> loadedScenes = new Dictionary<string, ParcelScene>();

  void Start() {
    // We trigger the Decentraland logic once SceneController has been instanced and is ready to act.
    if (startDecentralandAutomatically) {
      WebInterface.StartDecentraland();
    }
  }

  void Update() {
    if (Cursor.lockState != CursorLockMode.Locked) {
      if (Input.GetMouseButtonDown(0)) {
        LockCursor();
      }
    } else if (Input.GetKeyDown(KeyCode.Escape)) {
      UnlockCursor();
    }
  }

  void LockCursor() {
    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;
  }

  void UnlockCursor() {
    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
  }

  ParcelScene GetDecentralandSceneOfParcel(Vector2Int parcel) {
    foreach (var estate in loadedScenes) {
      if (estate.Value.sceneData.basePosition.Equals(parcel)) {
        return estate.Value;
      }

      foreach (var iteratedParcel in estate.Value.sceneData.parcels) {
        if (iteratedParcel.Equals(parcel)) {
          return estate.Value;
        }
      }
    }
    return null;
  }


  private LoadParcelScenesMessage loadParcelScenesMessage = new LoadParcelScenesMessage();

  public void LoadParcelScenes(string decentralandSceneJSON) {
    JsonUtility.FromJsonOverwrite(decentralandSceneJSON, this.loadParcelScenesMessage);

    var scenesToLoad = loadParcelScenesMessage.parcelsToLoad;
    var completeListOfParcelsThatShouldBeLoaded = new List<string>();

    // LOAD MISSING SCENES
    for (int i = 0; i < scenesToLoad.Count; i++) {
      var sceneToLoad = scenesToLoad[i];

      completeListOfParcelsThatShouldBeLoaded.Add(sceneToLoad.id);

      if (GetDecentralandSceneOfParcel(sceneToLoad.basePosition) == null) {
        var newGameObject = new GameObject();

        var newScene = newGameObject.AddComponent<ParcelScene>();
        newScene.SetData(sceneToLoad);

        if (!loadedScenes.ContainsKey(sceneToLoad.id)) {
          loadedScenes.Add(sceneToLoad.id, newScene);
        } else {
          loadedScenes[sceneToLoad.id] = newScene;
        }
      }
    }

    // UNLOAD EXTRA SCENES
    var loadedScenesClone = loadedScenes.ToArray();

    for (int i = 0; i < loadedScenesClone.Length; i++) {
      var loadedScene = loadedScenesClone[i];
      if (!completeListOfParcelsThatShouldBeLoaded.Contains(loadedScene.Key)) {
        UnloadScene(loadedScene.Key);
      }
    }
  }

  public void UnloadScene(string sceneKey) {
    if (loadedScenes.ContainsKey(sceneKey)) {
      if (loadedScenes[sceneKey] != null) {
        loadedScenes[sceneKey].Dispose();
      }
      loadedScenes.Remove(sceneKey);
    }
  }

  public void UnloadAllScenes() {
    var list = loadedScenes.ToArray();
    for (int i = 0; i < list.Length; i++) {
      UnloadScene(list[i].Key);
    }
  }

  public ParcelScene CreateTestScene(LoadParcelScenesMessage.UnityParcelScene data) {
    if (data.basePosition == null) {
      data.basePosition = new Vector2Int(0, 0);
    }

    if (data.parcels == null) {
      data.parcels = new Vector2Int[] { data.basePosition };
    }

    if (string.IsNullOrEmpty(data.id)) {
      data.id = $"(test):{data.basePosition.x},{data.basePosition.y}";
    }

    var go = new GameObject();
    var newScene = go.AddComponent<ParcelScene>();
    newScene.SetData(data);

    if (!loadedScenes.ContainsKey(data.id)) {
      loadedScenes.Add(data.id, newScene);
    } else {
      throw new Exception($"Scene {data.id} is already loaded.");
    }

    return newScene;
  }
}
