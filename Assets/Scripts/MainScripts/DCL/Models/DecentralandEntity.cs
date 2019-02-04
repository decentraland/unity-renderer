using System;
using System.Collections.Generic;
using DCL.Components;
using DCL.Controllers;
using UnityEngine;

namespace DCL.Models
{
    [Serializable]
    public class DecentralandEntity
    {
        public ParcelScene scene;
        public Dictionary<CLASS_ID_COMPONENT, BaseComponent> components = new Dictionary<CLASS_ID_COMPONENT, BaseComponent>();
        public GameObject gameObject;
        public string entityId;

        public System.Action<MonoBehaviour> OnComponentUpdated;
        public System.Action<DecentralandEntity> OnShapeUpdated;

        public GameObject meshGameObject;
        public BaseShape currentShape;

        const string MESH_GAMEOBJECT_NAME = "Mesh";

        public void EnsureMeshGameObject()
        {
            if (meshGameObject == null)
            {
                meshGameObject = new GameObject();
                meshGameObject.name = MESH_GAMEOBJECT_NAME;
                meshGameObject.transform.SetParent(gameObject.transform);
                meshGameObject.transform.localPosition = Vector3.zero;
            }
        }
    }
}
