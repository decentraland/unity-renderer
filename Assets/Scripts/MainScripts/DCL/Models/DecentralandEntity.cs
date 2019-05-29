using DCL.Components;
using DCL.Controllers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Models
{
    [Serializable]
    public class DecentralandEntity
    {
        public ParcelScene scene;

        public Dictionary<CLASS_ID_COMPONENT, BaseComponent> components =
            new Dictionary<CLASS_ID_COMPONENT, BaseComponent>();

        public GameObject gameObject;
        public string entityId;

        public System.Action<MonoBehaviour> OnComponentUpdated;
        public System.Action<DecentralandEntity> OnShapeUpdated;
        public System.Action<DecentralandEntity> OnRemoved;

        public GameObject meshGameObject;
        public BaseShape currentShape;

        Dictionary<Type, BaseDisposable> sharedComponents = new Dictionary<Type, BaseDisposable>();

        const string MESH_GAMEOBJECT_NAME = "Mesh";

        public void EnsureMeshGameObject(string gameObjectName = null)
        {
            if (meshGameObject == null)
            {
                meshGameObject = new GameObject();
                meshGameObject.name = gameObjectName == null ? MESH_GAMEOBJECT_NAME : gameObjectName;
                meshGameObject.transform.SetParent(gameObject.transform);
                meshGameObject.transform.localPosition = Vector3.zero;
                meshGameObject.transform.localScale = Vector3.one;
                meshGameObject.transform.localRotation = Quaternion.identity;
            }
        }

        public void AddSharedComponent(Type componentType, BaseDisposable component)
        {
            if (component == null)
            {
                return;
            }

            RemoveSharedComponent(componentType);

            sharedComponents.Add(componentType, component);
        }

        public void RemoveSharedComponent(Type targetType, bool triggerDettaching = true)
        {
            BaseDisposable component;
            if (sharedComponents.TryGetValue(targetType, out component) && component != null)
            {
                sharedComponents.Remove(targetType);

                if (triggerDettaching)
                {
                    component.DetachFrom(this, targetType);
                }
            }
        }

        public BaseDisposable GetSharedComponent(Type targetType)
        {
            BaseDisposable component;
            sharedComponents.TryGetValue(targetType, out component);

            return component;
        }
    }
}