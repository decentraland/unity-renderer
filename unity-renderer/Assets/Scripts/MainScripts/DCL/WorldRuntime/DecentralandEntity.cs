using System;
using System.Collections.Generic;
using DCL.Components;
using DCL.Configuration;
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
        public GameObject gameObject { get; private set; }
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

        const string MESH_GAMEOBJECT_NAME = "Mesh";
        const string BOUNDS_CHECK_COLLIDER_GAMEOBJECT_NAME = "BoundsCheckCollider";

        BoxCollider boundsCheckCollider = null;
        EntityBoundsCollisionChecker boundsCollisionChecker = null;
        int trespassingScenesCounter = 0;
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

        public void SetGameObject(GameObject gameObject)
        {   
            this.gameObject = gameObject;

            if (scene.isPersistent) return;

            // Create bounds trigger collider
            // TODO: every entity will always need this, we should avoid destroying it (cleaning its EntityBoundsCollisionChecker on clean)
            GameObject boundsCheckColliderGO = new GameObject(BOUNDS_CHECK_COLLIDER_GAMEOBJECT_NAME);
            boundsCheckColliderGO.layer = PhysicsLayers.entityBoundsCheckColliderLayer;

            boundsCollisionChecker = boundsCheckColliderGO.AddComponent<EntityBoundsCollisionChecker>();
            boundsCollisionChecker.OnEnteredParcel += OnEnteredParcel;
            boundsCollisionChecker.OnExitedParcel += OnExitedParcel;
            boundsCheckCollider = boundsCheckColliderGO.AddComponent<BoxCollider>();
            boundsCheckCollider.isTrigger = true;
            
            Transform boundsCheckColliderTransform = boundsCheckCollider.transform;
            boundsCheckColliderTransform.SetParent(gameObject.transform);
            // boundsCollisionChecker.SetEntityTransform(gameObject.transform);
            boundsCheckColliderTransform.localScale = Vector3.one * 0.009f;
            
            UpdateBoundsCheckColliderBasedOnMesh(this);

            OnShapeLoaded += UpdateBoundsCheckColliderBasedOnMesh;
            OnShapeUpdated += UpdateBoundsCheckColliderBasedOnMesh;
            // OnTransformChange += OnTransformUpdate; // ...
        }

        public void UpdateBoundsCheckColliderBasedOnMesh(IDCLEntity entity)
        {
            if (scene.isPersistent) return;
            
            if (meshesInfo != null && meshesInfo.meshRootGameObject != null && meshesInfo.mergedBounds != null)
            {                
                // meshesInfo.RecalculateBounds();
                
                meshesInfo.OnUpdated -= OnMeshesInfoUpdate;
                meshesInfo.OnUpdated += OnMeshesInfoUpdate;
                
                Transform boundsCheckColliderTransform = boundsCheckCollider.transform;
                
                SetTransformGlobalValues(
                        boundsCheckColliderTransform, 
                        true,
                        meshesInfo.mergedBounds.center,
                        Quaternion.Inverse(entity.gameObject.transform.rotation),
                        Vector3.one
                    );
                boundsCheckColliderTransform.localRotation = Quaternion.Inverse(entity.gameObject.transform.rotation);
                boundsCheckCollider.size = meshesInfo.mergedBounds.size;
            }
            
            EvaluateOutOfBoundsState();
        }
        
        public void SetTransformGlobalValues(Transform transform, bool setScale, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            transform.position = pos;
            transform.rotation = rot;
            
            if (setScale)
            {
                transform.localScale = Vector3.one;
                var m = transform.worldToLocalMatrix;
                
                m.SetColumn(0, new Vector4(m.GetColumn(0).magnitude, 0f));
                m.SetColumn(1, new Vector4(0f, m.GetColumn(1).magnitude));
                m.SetColumn(2, new Vector4(0f, 0f, m.GetColumn(2).magnitude));
                m.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));
                
                transform.localScale = m.MultiplyPoint(scale);
            }
        }

        void OnMeshesInfoUpdate()
        {
            UpdateBoundsCheckColliderBasedOnMesh(this);
        }

        /*void OnTransformUpdate(object newModel)
        {   
            // EvaluateOutOfBoundsState();
            
            UpdateBoundsCheckColliderBasedOnMesh(this);
        }*/

        void OnEnteredParcel(string parcelSceneId)
        {
            if (parcelSceneId != scene.sceneData.id)
                trespassingScenesCounter++;
            
            EvaluateOutOfBoundsState();
        }
        
        void OnExitedParcel(string parcelSceneId)
        {
            if (parcelSceneId != scene.sceneData.id)
                trespassingScenesCounter--;

            EvaluateOutOfBoundsState();
        }

        void EvaluateOutOfBoundsState()
        {
            // This better approach has problems (some meshes are re-enabled when they should be off).
            /*
            // Should we skip this if the shape hasn't finished loading ???
            
            if(isDebug) Debug.Log($"Pravs - DecentralandEntity.EvaluateOutOfBoundsState() - 1 - isInsideBoundaries? {isInsideBoundaries}; trespassingCounter? {trespassingScenesCounter}");
            
            if (isInsideBoundaries)
            {
                if (isDebug) Debug.Log($"Pravs - DecentralandEntity.EvaluateOutOfBoundsState() - 2A - Set as OUTSIDE BOUNDARIES");
                if (trespassingScenesCounter > 0 || boundsCheckCollider.bounds.max.y > scene.metricsCounter.maxCount.sceneHeight)
                    Environment.i.world.sceneBoundsChecker.ForceEntityInsideBoundariesState(this, false);
            }
            else if (trespassingScenesCounter <= 0 && boundsCheckCollider.bounds.max.y <= scene.metricsCounter.maxCount.sceneHeight)
            {
                if (isDebug) Debug.Log($"Pravs - DecentralandEntity.EvaluateOutOfBoundsState() - 2B - Set as INSIDE BOUNDARIES");
                Environment.i.world.sceneBoundsChecker.ForceEntityInsideBoundariesState(this, true);
            }
            
            if(isDebug) Debug.Log($"Pravs - DecentralandEntity.EvaluateOutOfBoundsState() - 3 - isInsideBoundaries? {isInsideBoundaries}; trespassingCounter? {trespassingScenesCounter}");*/

            Environment.i.world.sceneBoundsChecker.ForceEntityInsideBoundariesState(this, trespassingScenesCounter <= 0 && boundsCheckCollider.bounds.max.y <= scene.metricsCounter.maxCount.sceneHeight);
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

                if (!scene.isPersistent)
                {
                    OnShapeLoaded -= UpdateBoundsCheckColliderBasedOnMesh;
                    OnShapeUpdated -= UpdateBoundsCheckColliderBasedOnMesh;
                    boundsCollisionChecker.OnEnteredParcel -= OnEnteredParcel;
                    boundsCollisionChecker.OnExitedParcel -= OnExitedParcel;
                    Utils.SafeDestroy(boundsCheckCollider.gameObject);
                }
            }

            OnTransformChange = null;
            isReleased = true;
        }
    }
}