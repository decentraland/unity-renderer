using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Models
{
    [Serializable]
    public class DecentralandEntity : IDCLEntity
    {
        public IParcelScene scene { get; set; }
        public bool markedForCleanup { get; set; } = false;
        public bool isInsideBoundaries { get; set; } = false;

        public Dictionary<long, IDCLEntity> children { get; private set; } = new Dictionary<long, IDCLEntity>();
        public IDCLEntity parent { get; private set; }
        public GameObject gameObject { get; set; }
        public long entityId { get; set; }
        public MeshesInfo meshesInfo { get; set; }
        public GameObject meshRootGameObject => meshesInfo.meshRootGameObject;
        public Renderer[] renderers => meshesInfo.renderers;

        public Action<IDCLEntity> OnShapeUpdated { get; set; }
        public Action<IDCLEntity> OnShapeLoaded { get; set; }
        public Action<object> OnNameChange { get; set; }
        public Action<object> OnTransformChange { get; set; }
        public Action<IDCLEntity> OnRemoved { get; set; }
        public Action<IDCLEntity> OnMeshesInfoUpdated { get; set; }
        public Action<IDCLEntity> OnMeshesInfoCleaned { get; set; }

        public Action<ICleanableEventDispatcher> OnCleanupEvent { get; set; }

        public long parentId { get; set; }

        const string MESH_GAMEOBJECT_NAME = "Mesh";

        bool isReleased = false;

        public DecentralandEntity()
        {
            meshesInfo = new MeshesInfo();
            OnShapeUpdated += (entity) => meshesInfo.UpdateRenderersCollection();
            meshesInfo.OnUpdated += () => OnMeshesInfoUpdated?.Invoke(this);
            meshesInfo.OnCleanup += () => OnMeshesInfoCleaned?.Invoke(this);
        }

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
                parentId = entity.entityId;
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

            scene.componentsManagerLegacy.CleanComponents(this);

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
    }
}