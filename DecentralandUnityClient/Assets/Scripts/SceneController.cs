using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SceneController : MonoBehaviour{
    public GameObject baseEntityPrefab;

    Dictionary<string, GameObject> entities = new Dictionary<string, GameObject>();
    JSONParams jsonParams;
    GameObject parentGameObject;
    GameObject entityGameObject;
    Vector3 auxiliaryVector;

    [DllImport("__Internal")] static extern void InitializeDecentraland();

    void Awake(){
        InitializeDecentraland();
    }

    public void CreateEntity(string RawJSONParams)
    {
        jsonParams = JsonUtility.FromJson<JSONParams>(RawJSONParams);

        entities.Add(jsonParams.entityIdParam, Instantiate(baseEntityPrefab));
    }

    public void SetEntityParent(string RawJSONParams)
    {
        /*jsonParams = JsonUtility.FromJson<JSONParams>(RawJSONParams);
        
        entities.TryGetValue(jsonParams.parentIdParam, out parentGameObject);
        if (parentGameObject != null)
        {
            entities.TryGetValue(jsonParams.entityIdParam, out entityGameObject);
            if(entityGameObject != null)
                entityGameObject.transform.SetParent(parentGameObject.transform);
        }*/
    }

    public void UpdateEntity(string RawJSONParams)
    {
        jsonParams = JsonUtility.FromJson<JSONParams>(RawJSONParams);
        
        entities.TryGetValue(jsonParams.entityIdParam, out entityGameObject);
        if (entityGameObject != null)
        {
            auxiliaryVector.Set(jsonParams.entityComponents.position.x,
                                jsonParams.entityComponents.position.y,
                                jsonParams.entityComponents.position.z);

            entityGameObject.transform.position = auxiliaryVector;
        }

        Debug.Log("C#: json params");
        Debug.Log(jsonParams.entityComponents.position.x + ", " + 
                jsonParams.entityComponents.position.y + ", " + 
                jsonParams.entityComponents.position.z);
        Debug.Log("-----------");
    }

    [System.Serializable]
    struct JSONParams{
        public string entityIdParam;
        public string parentIdParam;
        public EntityComponents entityComponents;

        [System.Serializable]
        public struct EntityComponents{
            public EntityPosition position;
            public EntityScale scale;
            public EntityShape shape;
            public EntityPhysics physics;

            [System.Serializable]
            public struct EntityPosition{
                public float x;
                public float y;
                public float z;
            }

            [System.Serializable]
            public struct EntityScale{
                public float x;
                public float y;
                public float z;
            }

            [System.Serializable]
            public struct EntityShape{
                public string tag;
            }

            [System.Serializable]
            public struct EntityPhysics{
                public float accelerationX;
                public float accelerationY;
                public float accelerationZ;
                public float mass;
                public bool rigid;
                public float velocityX;
                public float velocityY;
                public float velocityZ;
            }
        }
    }
}

