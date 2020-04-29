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
        [Serializable]
        public class MeshesInfo
        {
            public event Action OnUpdated;
            public event Action OnCleanup;

            public GameObject meshRootGameObject
            {
                get
                {
                    return meshRootGameObjectValue;
                }
                set
                {
                    meshRootGameObjectValue = value;

                    UpdateRenderersCollection();
                }
            }

            public BaseShape currentShape;
            public Renderer[] renderers;
            public MeshFilter[] meshFilters;
            public List<Collider> colliders = new List<Collider>();

            Vector3 lastBoundsCalculationPosition;
            Vector3 lastBoundsCalculationScale;
            Quaternion lastBoundsCalculationRotation;
            Bounds mergedBoundsValue;
            public Bounds mergedBounds
            {
                get
                {
                    if (meshRootGameObject.transform.position != lastBoundsCalculationPosition)
                    {
                        mergedBoundsValue.center += meshRootGameObject.transform.position - lastBoundsCalculationPosition;
                        lastBoundsCalculationPosition = meshRootGameObject.transform.position;
                    }

                    if (meshRootGameObject.transform.lossyScale != lastBoundsCalculationScale || meshRootGameObject.transform.rotation != lastBoundsCalculationRotation)
                        RecalculateBounds();

                    return mergedBoundsValue;
                }
                set
                {
                    mergedBoundsValue = value;
                }
            }

            GameObject meshRootGameObjectValue;

            public void UpdateRenderersCollection()
            {
                if (meshRootGameObjectValue != null)
                {
                    renderers = meshRootGameObjectValue.GetComponentsInChildren<Renderer>(true);
                    meshFilters = meshRootGameObjectValue.GetComponentsInChildren<MeshFilter>(true);

                    RecalculateBounds();

                    OnUpdated?.Invoke();
                }
            }

            public void RecalculateBounds()
            {
                if (renderers == null || renderers.Length == 0) return;

                lastBoundsCalculationPosition = meshRootGameObject.transform.position;
                lastBoundsCalculationScale = meshRootGameObject.transform.lossyScale;
                lastBoundsCalculationRotation = meshRootGameObject.transform.rotation;

                mergedBoundsValue = Utils.BuildMergedBounds(renderers);
            }

            public void CleanReferences()
            {
                OnCleanup?.Invoke();
                meshRootGameObjectValue = null;
                currentShape = null;
                renderers = null;
                colliders.Clear();
            }

            public void UpdateExistingMeshAtIndex(Mesh mesh, uint meshFilterIndex = 0)
            {
                if (meshFilters != null && meshFilters.Length > meshFilterIndex)
                {
                    meshFilters[meshFilterIndex].sharedMesh = mesh;
                    OnUpdated?.Invoke();
                }
                else
                {
                    Debug.LogError($"MeshFilter index {meshFilterIndex} out of bounds - MeshesInfo.UpdateExistingMesh failed");
                }
            }
        }

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

        public void Cleanup()
        {
            // Dont't do anything if this object was already released
            if (isReleased) return;

            OnRemoved?.Invoke(this);

            // This will release the poolable objects of the mesh and the entity
            OnCleanupEvent?.Invoke(this);

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
            }

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

        public BaseDisposable GetSharedComponent(System.Type targetType)
        {
            BaseDisposable component;
            sharedComponents.TryGetValue(targetType, out component);

            return component;
        }
    }
}
