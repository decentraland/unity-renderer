using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityGLTF;
using UnityEngine;

public class SceneController : MonoBehaviour {
  public GameObject baseEntityPrefab;
  public GameObject entityRendererPrefab;
  public GameObject landParcelPrefab;

  public Dictionary<string, DecentralandScene> decentralandScenes = new Dictionary<string, DecentralandScene>();
  public Dictionary<string, DecentralandEntity> decentralandEntities = new Dictionary<string, DecentralandEntity>();

  DecentralandEntity decentralandEntity;
  DecentralandEntity parentDecentralandEntity;
  DecentralandEntity auxiliaryDecentralandEntity;
  DecentralandScenesPackage decentralandScenesPackage;
  Vector3 auxiliaryVector;
  GLTFComponent gltfComponent;
  GameObject auxiliaryGameObject;

  [DllImport("__Internal")] static extern void StartDecentraland();

  void Start() {
    // We trigger the Decentraland logic once SceneController has been instanced and is ready to act.
    StartDecentraland();
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

  // TODO: Move entities creation, update, etc. functionlities to the DecentralandScene
  public void CreateEntity(string entityID) {
    if (!decentralandEntities.ContainsKey(entityID)) {
      decentralandEntity = new DecentralandEntity();
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
    auxiliaryDecentralandEntity = JsonUtility.FromJson<DecentralandEntity>(RawJSONParams);

    if (auxiliaryDecentralandEntity.id != auxiliaryDecentralandEntity.parentId) {
      decentralandEntities.TryGetValue(auxiliaryDecentralandEntity.parentId, out parentDecentralandEntity);

      if (parentDecentralandEntity != null) {
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
    auxiliaryDecentralandEntity = JsonUtility.FromJson<DecentralandEntity>(RawJSONParams);

    decentralandEntities.TryGetValue(auxiliaryDecentralandEntity.id, out decentralandEntity);
    if (decentralandEntity != null) {
      // Update entity transform data
      decentralandEntity.components.transform = auxiliaryDecentralandEntity.components.transform;

      // Update entity shape data
      if (auxiliaryDecentralandEntity.components.shape != null) {
        if (decentralandEntity.components.shape == null) { // First time shape instantiation
          auxiliaryGameObject = Instantiate(entityRendererPrefab, decentralandEntity.gameObjectReference.transform);

          // Trigger GLTF loading
          if (!string.IsNullOrEmpty(auxiliaryDecentralandEntity.components.shape.src)) {
            gltfComponent = auxiliaryGameObject.GetComponent<GLTFComponent>();

            if (!gltfComponent.alreadyLoadedAsset) {
              gltfComponent.GLTFUri = auxiliaryDecentralandEntity.components.shape.src;

              gltfComponent.LoadAsset();

              auxiliaryGameObject.GetComponent<MeshRenderer>().enabled = false;
            }
          }
        }

        decentralandEntity.components.shape = auxiliaryDecentralandEntity.components.shape;
      }

      decentralandEntity.UpdateGameObjectComponents();
    }
  }

  public void LoadDecentralandScenes(string decentralandSceneJSON) {
    decentralandScenesPackage = JsonUtility.FromJson<DecentralandScenesPackage>(decentralandSceneJSON);

    for (int i = 0; i < decentralandScenesPackage.scenes.Length; i++) {
      auxiliaryGameObject = new GameObject();
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
