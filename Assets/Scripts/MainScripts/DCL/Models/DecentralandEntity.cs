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

        public Dictionary<string, DecentralandEntity> children = new Dictionary<string, DecentralandEntity>();
        public DecentralandEntity parent;

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
        public System.Action<DCLTransform.Model> OnTransformChange;

        public System.Action<ICleanableEventDispatcher> OnCleanupEvent { get; set; }

        public GameObject meshGameObject;
        public BaseShape currentShape;

        Dictionary<Type, BaseDisposable> sharedComponents = new Dictionary<Type, BaseDisposable>();

        const string MESH_GAMEOBJECT_NAME = "Mesh";

        bool isReleased = false;

        private void AddChild(DecentralandEntity entity)
        {
            if (!children.ContainsKey(entity.entityId))
            {
                children.Add(entity.entityId, entity);
            }
        }

        private void RemoveChild(DecentralandEntity entity)
        {
            if (children.ContainsKey(entity.entityId))
            {
                children.Remove(entity.entityId);
            }
        }

        public void SetParent(DecentralandEntity entity)
        {
            if (parent != null)
            {
                parent.RemoveChild(this);
            }

            if (entity != null)
            {
                entity.AddChild(this);

                if (entity.gameObject && this.gameObject)
                    this.gameObject.transform.SetParent(entity.gameObject.transform, false);
            }
            else if (this.gameObject)
            {
                this.gameObject.transform.SetParent(null, false);
            }

            parent = entity;
        }

        public void EnsureMeshGameObject(string gameObjectName = null)
        {
            if (meshGameObject == null)
            {
                meshGameObject = new GameObject();
                meshGameObject.name = gameObjectName == null ? MESH_GAMEOBJECT_NAME : gameObjectName;
                meshGameObject.transform.SetParent(gameObject.transform);
                Utils.ResetLocalTRS(meshGameObject.transform);
            }
        }

        public void Cleanup()
        {
            // Dont't do anything if this object was already released
            if (isReleased)
                return;

            OnRemoved?.Invoke(this);

            // This will release the poolable objects of the mesh and the entity
            OnCleanupEvent?.Invoke(this);

            if (meshGameObject)
            {
                Utils.SafeDestroy(meshGameObject);
                meshGameObject = null;
            }

            if (gameObject)
            {
                int childCount = gameObject.transform.childCount;

                // Destroy any other children 
                for (int i = 0; i < childCount; i++)
                {
                    Utils.SafeDestroy(gameObject.transform.GetChild(i).gameObject);
                }
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
