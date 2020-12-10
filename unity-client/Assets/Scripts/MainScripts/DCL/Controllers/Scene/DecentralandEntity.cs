using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Models
{
    [Serializable]
    public class DecentralandEntity : DCL.ICleanable, DCL.ICleanableEventDispatcher
    {
        public ParcelScene scene;
        public bool markedForCleanup = false;

        public Dictionary<string, DecentralandEntity> children = new Dictionary<string, DecentralandEntity>();
        public DecentralandEntity parent;

        public Dictionary<CLASS_ID_COMPONENT, BaseComponent> components = new Dictionary<CLASS_ID_COMPONENT, BaseComponent>();

        // HACK: (Zak) will be removed when we separate each
        // uuid component as a different class id
        public Dictionary<string, UUIDComponent> uuidComponents = new Dictionary<string, UUIDComponent>();

        public GameObject gameObject;
        public string entityId;
        public MeshesInfo meshesInfo;
        public GameObject meshRootGameObject => meshesInfo.meshRootGameObject;
        public Renderer[] renderers => meshesInfo.renderers;

        public System.Action<MonoBehaviour> OnComponentUpdated;
        public System.Action<DecentralandEntity> OnShapeUpdated;
        public System.Action<DCLName.Model> OnNameChange;
        public System.Action<DecentralandEntity> OnRemoved;
        public System.Action<DCLTransform.Model> OnTransformChange;
        public System.Action<DecentralandEntity> OnMeshesInfoUpdated;
        public System.Action<DecentralandEntity> OnMeshesInfoCleaned;

        public System.Action<ICleanableEventDispatcher> OnCleanupEvent { get; set; }
        Dictionary<System.Type, BaseDisposable> sharedComponents = new Dictionary<System.Type, BaseDisposable>();

        const string MESH_GAMEOBJECT_NAME = "Mesh";

        bool isReleased = false;

        public DecentralandEntity()
        {
            meshesInfo = new MeshesInfo();
            OnShapeUpdated += (entity) => meshesInfo.UpdateRenderersCollection();
            meshesInfo.OnUpdated += () => OnMeshesInfoUpdated?.Invoke(this);
            meshesInfo.OnCleanup += () => OnMeshesInfoCleaned?.Invoke(this);
        }

        public Dictionary<System.Type, BaseDisposable> GetSharedComponents()
        {
            return sharedComponents;
        }

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

                if (entity.gameObject && gameObject)
                    gameObject.transform.SetParent(entity.gameObject.transform, false);
            }
            else if (gameObject)
            {
                gameObject.transform.SetParent(null, false);
            }

            parent = entity;
        }

        public void EnsureMeshGameObject(string gameObjectName = null)
        {
            if (meshesInfo.meshRootGameObject == null)
            {
                meshesInfo.meshRootGameObject = new GameObject();
                meshesInfo.meshRootGameObject.name = gameObjectName == null ? MESH_GAMEOBJECT_NAME : gameObjectName;
                meshesInfo.meshRootGameObject.transform.SetParent(gameObject.transform);
                Utils.ResetLocalTRS(meshesInfo.meshRootGameObject.transform);
            }
        }

        public void ResetRelease()
        {
            isReleased = false;
        }

        public void Cleanup()
        {
            // Don't do anything if this object was already released
            if (isReleased) return;

            OnRemoved?.Invoke(this);

            // This will release the poolable objects of the mesh and the entity
            OnCleanupEvent?.Invoke(this);

            foreach (var kvp in components)
            {
                if (kvp.Value == null || kvp.Value.poolableObject == null)
                    continue;

                kvp.Value.poolableObject.Release();
            }

            components.Clear();

            if (meshesInfo.meshRootGameObject)
            {
                Utils.SafeDestroy(meshesInfo.meshRootGameObject);
                meshesInfo.CleanReferences();
            }

            if (gameObject)
            {
                int childCount = gameObject.transform.childCount;

                // Destroy any other children
                for (int i = 0; i < childCount; i++)
                {
                    Utils.SafeDestroy(gameObject.transform.GetChild(i).gameObject);
                }

                //NOTE(Brian): This will prevent any component from storing/querying invalid gameObject references.
                gameObject = null;
            }

            OnTransformChange = null;
            isReleased = true;
        }

        public void AddSharedComponent(System.Type componentType, BaseDisposable component)
        {
            if (component == null)
            {
                return;
            }

            RemoveSharedComponent(componentType);

            sharedComponents.Add(componentType, component);
        }

        public void RemoveSharedComponent(System.Type targetType, bool triggerDettaching = true)
        {
            if (sharedComponents.TryGetValue(targetType, out BaseDisposable component))
            {
                if (component == null)
                    return;

                sharedComponents.Remove(targetType);

                if (triggerDettaching)
                    component.DetachFrom(this, targetType);
            }
        }

        public BaseDisposable GetSharedComponent(System.Type targetType)
        {
            BaseDisposable component;
            sharedComponents.TryGetValue(targetType, out component);

            return component;
        }
    }
}