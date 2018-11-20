using System.Collections.Generic;

using System.Runtime.InteropServices;
using UnityGLTF;
using UnityEngine;
using DCL.Interface;
using Newtonsoft.Json;
using DCL.Models;
using System.Linq;
using DCL.Controllers;
using System;
using System.Collections;

public class SceneController : MonoBehaviour {
  public bool startDecentralandAutomatically = true;

  public Dictionary<string, DecentralandScene> loadedScenes = new Dictionary<string, DecentralandScene>();

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

  DecentralandScene GetDecentralandSceneOfParcel(int x, int y) {
    var key = new Vector2(x, y);
    foreach (var estate in loadedScenes) {
      if (estate.Value.sceneData.basePosition.Equals(key)) {
        return estate.Value;
      }

      foreach (var parcel in estate.Value.sceneData.parcels) {
        if (parcel.Equals(key)) {
          return estate.Value;
        }
      }
    }
    return null;
  }

  public void LoadDecentralandScenes(string decentralandSceneJSON) {
    var scenesToLoad = JsonConvert.DeserializeObject<LoaderScene[]>(decentralandSceneJSON);
    var completeListOfParcelsThatShouldBeLoaded = new List<string>();

    // LOAD MISSING SCENES
    for (int i = 0; i < scenesToLoad.Length; i++) {
      var sceneToLoad = scenesToLoad[i];

      completeListOfParcelsThatShouldBeLoaded.Add(sceneToLoad.id);

      if (GetDecentralandSceneOfParcel(sceneToLoad.x, sceneToLoad.y) == null) {
        var newScene = new DecentralandScene(sceneToLoad);

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
      loadedScenes[sceneKey].Dispose();
      loadedScenes.Remove(sceneKey);
    }
  }

  public void UnloadAllScenes() {
    var list = loadedScenes.ToArray();
    for (int i = 0; i < list.Length; i++) {
      UnloadScene(list[i].Key);
    }
  }

  public DecentralandScene CreateTestScene(LoaderScene data) {
    if (data.basePosition == null) {
      data.basePosition = new Vector2(data.x, data.y);
    }

    if (data.parcels == null) {
      data.parcels = new Vector2[] { data.basePosition };
    }

    if (string.IsNullOrEmpty(data.id)) {
      data.id = $"{data.x},${data.y}";
    }

    var newScene = new DecentralandScene(data);

    if (!loadedScenes.ContainsKey(data.id)) {
      loadedScenes.Add(data.id, newScene);
    } else {
      throw new Exception($"Scene {data.id} is already loaded.");
    }

    return newScene;
  }
}
