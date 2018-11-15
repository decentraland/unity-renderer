using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityGLTF;
using UnityEngine;

public class SceneController : MonoBehaviour {
  public bool startDecentralandAutomatically = true;
  public GameObject baseEntityPrefab;
  public GameObject landParcelPrefab;
  public GameObject[] rendererPrefabs;

  public Dictionary<string, DecentralandScene> decentralandScenes = new Dictionary<string, DecentralandScene>();
  public Dictionary<string, DecentralandEntity> decentralandEntities = new Dictionary<string, DecentralandEntity>();

  [DllImport("__Internal")] static extern void StartDecentraland();

  void Start() {
    if (startDecentralandAutomatically) {
      // We trigger the Decentraland logic once SceneController has been instanced and is ready to act.
      StartDecentraland();
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

  // TODO: Move entities creation, update, etc. functionlaties to the DecentralandScene
  public void CreateEntity(string entityID) {
    if (!decentralandEntities.ContainsKey(entityID)) {
      DecentralandEntity decentralandEntity = new DecentralandEntity();
      decentralandEntity.id = entityID;
      decentralandEntity.gameObjectReference = Instantiate(baseEntityPrefab);
      decentralandEntity.gameObjectReference.name = entityID;

      decentralandEntities.Add(entityID, decentralandEntity);
    } else {
      Debug.Log("Couldn't create entity with ID: " + entityID + " as it already exists.");
    }
  }

  public void RemoveEntity(string entityID) {
    if (decentralandEntities.ContainsKey(entityID)) {
      Destroy(decentralandEntities[entityID].gameObjectReference);
      decentralandEntities.Remove(entityID);
    } else {
      Debug.Log("Couldn't remove entity with ID: " + entityID + " as it doesn't exist.");
    }
  }

  public void SetEntityParent(string RawJSONParams) {
    DecentralandEntity auxiliaryDecentralandEntity = JsonUtility.FromJson<DecentralandEntity>(RawJSONParams);

    if (auxiliaryDecentralandEntity.id != auxiliaryDecentralandEntity.parentId) {
      DecentralandEntity parentDecentralandEntity;

      decentralandEntities.TryGetValue(auxiliaryDecentralandEntity.parentId, out parentDecentralandEntity);

      if (parentDecentralandEntity != null) {
        DecentralandEntity decentralandEntity;

        decentralandEntities.TryGetValue(auxiliaryDecentralandEntity.id, out decentralandEntity);

        if (decentralandEntity != null) {
          decentralandEntity.gameObjectReference.transform.SetParent(parentDecentralandEntity.gameObjectReference.transform);
        } else {
          Debug.Log("Couldn't enparent entity " + auxiliaryDecentralandEntity.id + " because that entity doesn't exist.");
        }
      } else {
        Debug.Log("Couldn't enparent entity " + auxiliaryDecentralandEntity.id + " because the parent (id " + auxiliaryDecentralandEntity.parentId + ") doesn't exist");
      }
    } else {
      Debug.Log("Couldn't enparent entity " + auxiliaryDecentralandEntity.id + " because the configured parent id is its own id.");
    }
  }

  public void UpdateEntity(string RawJSONParams) {
    DecentralandEntity decentralandEntity;

    DecentralandEntity auxiliaryDecentralandEntity = JsonUtility.FromJson<DecentralandEntity>(RawJSONParams);

    decentralandEntities.TryGetValue(auxiliaryDecentralandEntity.id, out decentralandEntity);
    if (decentralandEntity != null) {
      // Update entity transform data
      decentralandEntity.components.transform = auxiliaryDecentralandEntity.components.transform;

      // Update entity shape data
      if (auxiliaryDecentralandEntity.components.shape != null) {
        if (decentralandEntity.components.shape == null) { // First time shape instantiation
          IntializeDecentralandEntityRenderer(decentralandEntity, auxiliaryDecentralandEntity);
        }

        decentralandEntity.components.shape = auxiliaryDecentralandEntity.components.shape;
      }

      decentralandEntity.UpdateGameObjectComponents();
    } else {
      Debug.Log("Couldn't update entity " + auxiliaryDecentralandEntity.id + " because that entity doesn't exist.");
    }
  }

  void IntializeDecentralandEntityRenderer(DecentralandEntity currentEntity, DecentralandEntity loadedEntity) {
    switch (loadedEntity.components.shape.tag) {
      case "box":

        Instantiate(rendererPrefabs[0], currentEntity.gameObjectReference.transform);
        break;
      case "sphere":
        Instantiate(rendererPrefabs[1], currentEntity.gameObjectReference.transform);
        break;
      case "plane":
        Instantiate(rendererPrefabs[2], currentEntity.gameObjectReference.transform);
        break;
      case "cone":
        Instantiate(rendererPrefabs[3], currentEntity.gameObjectReference.transform);
        break;
      case "cylinder":
        Instantiate(rendererPrefabs[4], currentEntity.gameObjectReference.transform);
        break;
      case "gltf-model":
        GameObject auxiliaryGameObject = Instantiate(rendererPrefabs[5], currentEntity.gameObjectReference.transform);

        // Trigger GLTF loading
        if (!string.IsNullOrEmpty(loadedEntity.components.shape.src)) {
          GLTFComponent gltfComponent = auxiliaryGameObject.GetComponent<GLTFComponent>();

          if (!gltfComponent.alreadyLoadedAsset) {
            gltfComponent.GLTFUri = loadedEntity.components.shape.src;

            gltfComponent.LoadAsset();
          }
        }
        break;
      case "obj-model":
        // Trigger OBJ loading
        if (!string.IsNullOrEmpty(loadedEntity.components.shape.src)) {
          auxiliaryGameObject = Instantiate(rendererPrefabs[6], currentEntity.gameObjectReference.transform);

          DynamicOBJLoaderController OBJLoaderController = auxiliaryGameObject.GetComponent<DynamicOBJLoaderController>();

          if (!OBJLoaderController.alreadyLoaded) {
            OBJLoaderController.OBJUrl = loadedEntity.components.shape.src;

            StartCoroutine(OBJLoaderController.LoadRemoteOBJ());
          }
        }
        break;
    }
  }

  public void LoadDecentralandScenes(string decentralandSceneJSON) {
    DecentralandScenesPackage decentralandScenesPackage = JsonUtility.FromJson<DecentralandScenesPackage>(decentralandSceneJSON);

    for (int i = 0; i < decentralandScenesPackage.scenes.Length; i++) {
      GameObject auxiliaryGameObject = new GameObject();
      auxiliaryGameObject.name = "DecentralandScene (" + decentralandScenesPackage.scenes[i].id + ")";

      for (int j = 0; j < decentralandScenesPackage.scenes[i].parcels.Length; j++) {
        Instantiate(landParcelPrefab, auxiliaryGameObject.transform).name += " (" +
            decentralandScenesPackage.scenes[i].parcels[j].x + "," + decentralandScenesPackage.scenes[i].parcels[j].y + ")";
      }

      decentralandScenesPackage.scenes[i].gameObjectReference = auxiliaryGameObject;
      decentralandScenesPackage.scenes[i].Initialize();

      if (!decentralandScenes.ContainsKey(i.ToString())) {
        decentralandScenes.Add(decentralandScenesPackage.scenes[i].id, decentralandScenesPackage.scenes[i]);
      } else {
        decentralandScenes[decentralandScenesPackage.scenes[i].id] = decentralandScenesPackage.scenes[i];
      }
    }
  }

  public void UnloadScene(string sceneKey) {
    if (decentralandScenes.ContainsKey(sceneKey)) {
      decentralandScenes.Remove(sceneKey);
    }
  }
}
