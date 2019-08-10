using DCL.Components;
using DCL.Controllers;
using System;
using System.Collections.Generic;
using UnityEngine;
using DCL.Helpers;

namespace DCL.Models
{
    [Serializable]
    public class DecentralandEntity : DCL.ICleanable, DCL.ICleanableEventDispatcher
    {
        public ParcelScene scene;

        public Dictionary<CLASS_ID_COMPONENT, BaseComponent> components =
            new Dictionary<CLASS_ID_COMPONENT, BaseComponent>();

        // HACK: (Zak) will be removed when we separate each 
        // uuid component as a different class id
        public Dictionary<string, UUIDComponent> uuidComponents =
            new Dictionary<string, UUIDComponent>();

        public GameObject gameObject;
        public string entityId;

        public System.Action<MonoBehaviour> OnComponentUpdated;
        public System.Action<DecentralandEntity> OnShapeUpdated;
        public System.Action<DecentralandEntity> OnRemoved;

        public System.Action<ICleanableEventDispatcher> OnCleanupEvent { get; set; }

        public GameObject meshGameObject;
        public BaseShape currentShape;

        Dictionary<Type, BaseDisposable> sharedComponents = new Dictionary<Type, BaseDisposable>();

        const string MESH_GAMEOBJECT_NAME = "Mesh";

        bool isReleased = false;

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

        public void Cleanup()
        {
            if (isReleased)
                return;

            OnCleanupEvent?.Invoke(this);

            if (meshGameObject)
            {
                Utils.SafeDestroy(meshGameObject);
                meshGameObject = null;
            }

            isReleased = true;
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
