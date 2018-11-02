using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityGLTF;
using UnityEngine;

public class SceneController : MonoBehaviour {
  public GameObject baseEntityPrefab;
  public Dictionary<string, DecentralandEntity> entities = new Dictionary<string, DecentralandEntity>();

  DecentralandEntity entityObject;
  DecentralandEntity auxiliaryEntityObject;
  // GameObject parentGameObject;
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

  public void CreateEntity(string RawJSONParams) {
    entityObject = JsonUtility.FromJson<DecentralandEntity>(RawJSONParams);

    if (entities.ContainsKey(entityObject.id)) {
      Debug.Log("Couldn't create entity with ID: " + entityObject.id + " as it already exists.");
      return;
    }

    entityObject.gameObjectReference = Instantiate(baseEntityPrefab);

    entities.Add(entityObject.id, entityObject);
  }

  public void SetEntityParent(string RawJSONParams) {
    /*jsonParams = JsonUtility.FromJson<JSONParams>(RawJSONParams);

    entities.TryGetValue(jsonParams.parentIdParam, out parentGameObject);
    if (parentGameObject != null) {
        entities.TryGetValue(jsonParams.entityIdParam, out entityGameObject);
        if(entityGameObject != null)
            entityGameObject.transform.SetParent(parentGameObject.transform);
    }*/
  }

  public void UpdateEntity(string RawJSONParams) {
    auxiliaryEntityObject = JsonUtility.FromJson<DecentralandEntity>(RawJSONParams);

    entities.TryGetValue(auxiliaryEntityObject.id, out entityObject);
    if (entityObject != null) {
      auxiliaryVector.Set(auxiliaryEntityObject.components.position.x,
                          auxiliaryEntityObject.components.position.y,
                          auxiliaryEntityObject.components.position.z);

      entityObject.gameObjectReference.transform.position = auxiliaryVector;

      if (auxiliaryEntityObject.components.shape.src != "") {
        gltfComponent = entityObject.gameObjectReference.GetComponent<GLTFComponent>();

        if (gltfComponent.alreadyLoadedAsset) return;

        gltfComponent.GLTFUri = auxiliaryEntityObject.components.shape.src;

        StartCoroutine(gltfComponent.LoadAssetCoroutine());

        entityObject.gameObjectReference.GetComponent<MeshRenderer>().enabled = false;
      }
    }
  }
}
