using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.Models
{
    public interface IDCLEntity : ICleanable, ICleanableEventDispatcher
    {
        GameObject gameObject { get; set; }
        string entityId { get; set; }
        MeshesInfo meshesInfo { get; set; }
        GameObject meshRootGameObject { get; }
        Renderer[] renderers { get; }
        void SetParent(IDCLEntity entity);
        void AddChild(IDCLEntity entity);
        void RemoveChild(IDCLEntity entity);
        void EnsureMeshGameObject(string gameObjectName = null);
        void ResetRelease();
        void AddSharedComponent(System.Type componentType, ISharedComponent component);
        void RemoveSharedComponent(System.Type targetType, bool triggerDetaching = true);

        /// <summary>
        /// This function is designed to get interfaces implemented by diverse components.
        ///
        /// If you want to get the component itself please use TryGetBaseComponent or TryGetSharedComponent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T TryGetComponent<T>() where T : class;

        bool TryGetBaseComponent(CLASS_ID_COMPONENT componentId, out IEntityComponent component);
        bool TryGetSharedComponent(CLASS_ID componentId, out ISharedComponent component);
        ISharedComponent GetSharedComponent(System.Type targetType);
        IParcelScene scene { get; set; }
        bool markedForCleanup { get; set; }
        Dictionary<string, IDCLEntity> children { get; }
        IDCLEntity parent { get; }
        Dictionary<CLASS_ID_COMPONENT, IEntityComponent> components { get; }
        Dictionary<System.Type, ISharedComponent> sharedComponents { get; }
        Action<IDCLEntity> OnShapeUpdated { get; set; }
        Action<DCLName.Model> OnNameChange { get; set; }
        Action<IDCLEntity> OnRemoved { get; set; }
        Action<DCLTransform.Model> OnTransformChange { get; set; }
        Action<IDCLEntity> OnMeshesInfoUpdated { get; set; }
        Action<IDCLEntity> OnMeshesInfoCleaned { get; set; }
    }

    [Serializable]
    public class DecentralandEntity : IDCLEntity
    {
        public IParcelScene scene { get; set; }
        public bool markedForCleanup { get; set; } = false;

        public Dictionary<string, IDCLEntity> children { get; private set; } = new Dictionary<string, IDCLEntity>();
        public IDCLEntity parent { get; private set; }

        public Dictionary<CLASS_ID_COMPONENT, IEntityComponent> components { get; private set; } = new Dictionary<CLASS_ID_COMPONENT, IEntityComponent>();
        public Dictionary<System.Type, ISharedComponent> sharedComponents { get; private set; } = new Dictionary<System.Type, ISharedComponent>();

        public GameObject gameObject { get; set; }
        public string entityId { get; set; }
        public MeshesInfo meshesInfo { get; set; }
        public GameObject meshRootGameObject => meshesInfo.meshRootGameObject;
        public Renderer[] renderers => meshesInfo.renderers;

        public System.Action<IDCLEntity> OnShapeUpdated { get; set; }
        public System.Action<DCLName.Model> OnNameChange { get; set; }
        public System.Action<IDCLEntity> OnRemoved { get; set; }
        public System.Action<DCLTransform.Model> OnTransformChange { get; set; }
        public System.Action<IDCLEntity> OnMeshesInfoUpdated { get; set; }
        public System.Action<IDCLEntity> OnMeshesInfoCleaned { get; set; }

        public System.Action<ICleanableEventDispatcher> OnCleanupEvent { get; set; }

        const string MESH_GAMEOBJECT_NAME = "Mesh";

        bool isReleased = false;

        public DecentralandEntity()
        {
            meshesInfo = new MeshesInfo();
            OnShapeUpdated += (entity) => meshesInfo.UpdateRenderersCollection();
            meshesInfo.OnUpdated += () => OnMeshesInfoUpdated?.Invoke(this);
            meshesInfo.OnCleanup += () => OnMeshesInfoCleaned?.Invoke(this);
        }

        public Dictionary<System.Type, ISharedComponent> GetSharedComponents() { return sharedComponents; }

        public void AddChild(IDCLEntity entity)
        {
            if (!children.ContainsKey(entity.entityId))
            {
                children.Add(entity.entityId, entity);
            }
        }

        public void RemoveChild(IDCLEntity entity)
        {
            if (children.ContainsKey(entity.entityId))
            {
                children.Remove(entity.entityId);
            }
        }

        public void SetParent(IDCLEntity entity)
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

        public void ResetRelease() { isReleased = false; }

        public void Cleanup()
        {
            // Don't do anything if this object was already released
            if (isReleased)
                return;

            OnRemoved?.Invoke(this);

            // This will release the poolable objects of the mesh and the entity
            OnCleanupEvent?.Invoke(this);

            foreach (var kvp in components)
            {
                if (kvp.Value == null)
                    continue;

                if (!(kvp.Value is IPoolableObjectContainer poolableContainer))
                    continue;

                if (poolableContainer.poolableObject == null)
                    continue;

                poolableContainer.poolableObject.Release();
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

        public void AddSharedComponent(System.Type componentType, ISharedComponent component)
        {
            if (component == null)
            {
                return;
            }

            RemoveSharedComponent(componentType);

            sharedComponents.Add(componentType, component);
        }

        public void RemoveSharedComponent(Type targetType, bool triggerDetaching = true)
        {
            if (sharedComponents.TryGetValue(targetType, out ISharedComponent component))
            {
                if (component == null)
                    return;

                sharedComponents.Remove(targetType);

                if (triggerDetaching)
                    component.DetachFrom(this, targetType);
            }
        }

        /// <summary>
        /// This function is designed to get interfaces implemented by diverse components.
        ///
        /// If you want to get the component itself please use TryGetBaseComponent or TryGetSharedComponent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T TryGetComponent<T>() where T : class
        {
            //Note (Adrian): If you are going to call this function frequently, please refactor it to avoid using LinQ for perfomance reasons.
            T component = components.Values.FirstOrDefault(x => x is T) as T;

            if (component != null)
                return component;

            component = sharedComponents.Values.FirstOrDefault(x => x is T) as T;

            if (component != null)
                return component;

            return null;
        }

        public bool TryGetBaseComponent(CLASS_ID_COMPONENT componentId, out IEntityComponent component) { return components.TryGetValue(componentId, out component); }

        public bool TryGetSharedComponent(CLASS_ID componentId, out ISharedComponent component)
        {
            foreach (KeyValuePair<Type, ISharedComponent> keyValuePairBaseDisposable in sharedComponents)
            {
                if (keyValuePairBaseDisposable.Value.GetClassId() == (int) componentId)
                {
                    component = keyValuePairBaseDisposable.Value;
                    return true;
                }
            }

            component = null;
            return false;
        }

        public ISharedComponent GetSharedComponent(System.Type targetType)
        {
            if (sharedComponents.TryGetValue(targetType, out ISharedComponent component))
            {
                return component;
            }

            return null;
        }
    }
}