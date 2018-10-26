using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SceneController : MonoBehaviour {
    public GameObject baseEntityPrefab;
    public Dictionary<string, DecentralandEntity> entities = new Dictionary<string, DecentralandEntity>();

    DecentralandEntity entityObject;
    DecentralandEntity auxiliaryEntityObject;
    //GameObject parentGameObject;
    Vector3 auxiliaryVector;

    [DllImport("__Internal")] static extern void InitializeDecentraland();

    void Awake() {
        InitializeDecentraland();
    }

    public void CreateEntity(string RawJSONParams) {
        entityObject = JsonUtility.FromJson<DecentralandEntity>(RawJSONParams);

        if (entities.ContainsKey(entityObject.entityIdParam)) {
            Debug.Log("Couldn't create entity with ID: " + entityObject.entityIdParam  + "as it already exists.");
            return;
        }

        entityObject.sceneObjectReference = Instantiate(baseEntityPrefab);

        entities.Add(entityObject.entityIdParam, entityObject);
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

        entities.TryGetValue(auxiliaryEntityObject.entityIdParam, out entityObject);
        if (entityObject != null) {
            auxiliaryVector.Set(auxiliaryEntityObject.entityComponents.position.x,
                                auxiliaryEntityObject.entityComponents.position.y,
                                auxiliaryEntityObject.entityComponents.position.z);

            entityObject.sceneObjectReference.transform.position = auxiliaryVector;
        }

        /*Debug.Log("C#: json params");
        Debug.Log(jsonParams.entityComponents.position.x + ", " + 
                jsonParams.entityComponents.position.y + ", " + 
                jsonParams.entityComponents.position.z);
        Debug.Log("-----------");*/
    }
}