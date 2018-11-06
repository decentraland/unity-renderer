using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityGLTF;
using UnityEngine;

public class SceneController : MonoBehaviour {
  public GameObject baseEntityPrefab;
  public Dictionary<string, DecentralandEntity> entities = new Dictionary<string, DecentralandEntity>();

  DecentralandEntity entityObject;
  DecentralandEntity parentEntity;
  DecentralandEntity auxiliaryEntityObject;
  Vector3 auxiliaryVector;
  GLTFComponent gltfComponent;

  [DllImport("__Internal")] static extern void StartDecentraland();

  void Start() {
    ToggleCursorVisibility();

    // We trigger the Decentraland logic once SceneController has been instanced and is ready to act.
    StartDecentraland();
  }

  void Update() {
    if (Input.GetKeyDown(KeyCode.Escape))
      ToggleCursorVisibility();
  }

  void ToggleCursorVisibility() {
    if (Cursor.visible)
      Cursor.lockState = CursorLockMode.Locked;
    else
      Cursor.lockState = CursorLockMode.Confined;

    Cursor.visible = !Cursor.visible;
  }

  public void CreateEntity(string entityID) {
    if (entities.ContainsKey(entityID)) {
      Debug.Log("Couldn't create entity with ID: " + entityID + " as it already exists.");
      return;
    }

    entityObject = new DecentralandEntity();
    entityObject.id = entityID;
    entityObject.gameObjectReference = Instantiate(baseEntityPrefab);
    entityObject.gameObjectReference.name = entityID;

    entities.Add(entityID, entityObject);
  }

  public void RemoveEntity(string entityID)
  {
    if (!entities.ContainsKey(entityID)) {
      Debug.Log("Couldn't remove entity with ID: " + entityID + " as it doesn't exist.");
      return;
    }

    Destroy(entities[entityID].gameObjectReference);
    entities.Remove(entityID);
  }

  public void SetEntityParent(string RawJSONParams) {
    auxiliaryEntityObject = JsonUtility.FromJson<DecentralandEntity>(RawJSONParams);

    if (auxiliaryEntityObject.id == auxiliaryEntityObject.parentId) {
      Debug.Log("Couldn't enparent entity " + auxiliaryEntityObject.id + " because the configured parent id is its own id.");
      return;
    }

    entities.TryGetValue(auxiliaryEntityObject.parentId, out parentEntity);
    if (parentEntity != null) {
        entities.TryGetValue(auxiliaryEntityObject.id, out entityObject);

        if(entityObject != null)
          entityObject.gameObjectReference.transform.SetParent(parentEntity.gameObjectReference.transform);
        else
          Debug.Log("Couldn't enparent entity " + auxiliaryEntityObject.id + " because that entity is doesn't exist.");
    } else {
      Debug.Log("Couldn't enparent entity " + auxiliaryEntityObject.id + " because the parent (id " + auxiliaryEntityObject.parentId + ") doesn't exist");
    }
  }

  public void UpdateEntity(string RawJSONParams) {
    auxiliaryEntityObject = JsonUtility.FromJson<DecentralandEntity>(RawJSONParams);

    entities.TryGetValue(auxiliaryEntityObject.id, out entityObject);
    if (entityObject != null) {
      auxiliaryVector.Set(auxiliaryEntityObject.components.position.x,
                          auxiliaryEntityObject.components.position.y,
                          auxiliaryEntityObject.components.position.z);

      entityObject.gameObjectReference.transform.position = auxiliaryVector;

      if (!string.IsNullOrEmpty(auxiliaryEntityObject.components.shape.src)) {
        gltfComponent = entityObject.gameObjectReference.GetComponent<GLTFComponent>();

        if (gltfComponent.alreadyLoadedAsset) return;

        gltfComponent.GLTFUri = auxiliaryEntityObject.components.shape.src;

        gltfComponent.LoadAsset();

        entityObject.gameObjectReference.GetComponent<MeshRenderer>().enabled = false;
      }
    }
  }
}
