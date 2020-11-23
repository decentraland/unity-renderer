using DCL.Helpers;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Controllers
{
    public class SceneBoundariesDebugModeChecker : SceneBoundariesChecker
    {
        class InvalidMeshInfo
        {
            public Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();
            public List<GameObject> wireframeObjects = new List<GameObject>();
            public DecentralandEntity.MeshesInfo meshesInfo;
            public System.Action OnResetMaterials;

            public InvalidMeshInfo(DecentralandEntity.MeshesInfo meshesInfo)
            {
                this.meshesInfo = meshesInfo;
            }

            public void ResetMaterials(DecentralandEntity.MeshesInfo meshesInfo)
            {
                this.meshesInfo = meshesInfo;
                ResetMaterials();
            }

            public void ResetMaterials()
            {
                if (meshesInfo.meshRootGameObject == null) return;

                for (int i = 0; i < meshesInfo.renderers.Length; i++)
                {
                    meshesInfo.renderers[i].sharedMaterial = originalMaterials[meshesInfo.renderers[i]];
                }

                int wireframeObjectscount = wireframeObjects.Count;
                for (int i = 0; i < wireframeObjectscount; i++)
                {
                    Utils.SafeDestroy(wireframeObjects[i]);
                }

                OnResetMaterials?.Invoke();
            }
        }

        const string WIREFRAME_PREFAB_NAME = "Prefabs/WireframeCubeMesh";
        const string INVALID_MESH_MATERIAL_NAME = "Materials/InvalidMesh";
        const string INVALID_SUBMESH_MATERIAL_NAME = "Materials/InvalidSubMesh";

        Material invalidMeshMaterial;
        Material invalidSubMeshMaterial;
        Dictionary<GameObject, InvalidMeshInfo> invalidMeshesInfo = new Dictionary<GameObject, InvalidMeshInfo>();
        HashSet<Renderer> invalidSubmeshes = new HashSet<Renderer>();

        public SceneBoundariesDebugModeChecker() : base()
        {
            invalidMeshesInfo = new Dictionary<GameObject, InvalidMeshInfo>();
            invalidMeshMaterial = Resources.Load(INVALID_MESH_MATERIAL_NAME) as Material;
            invalidSubMeshMaterial = Resources.Load(INVALID_SUBMESH_MATERIAL_NAME) as Material;
        }

        protected override bool AreSubmeshesInsideBoundaries(DecentralandEntity entity)
        {
            bool isInsideBoundaries = true;

            for (int i = 0; i < entity.meshesInfo.renderers.Length; i++)
            {
                if (!entity.scene.IsInsideSceneBoundaries(entity.meshesInfo.renderers[i].bounds))
                {
                    isInsideBoundaries = false;

                    invalidSubmeshes.Add(entity.renderers[i]);
                }
            }

            return isInsideBoundaries;
        }

        protected override void UpdateEntityMeshesValidState(DecentralandEntity entity, bool isInsideBoundaries)
        {
            if (isInsideBoundaries)
                RemoveInvalidMeshEffect(entity);
            else
                AddInvalidMeshEffect(entity);
        }

        void RemoveInvalidMeshEffect(DecentralandEntity entity)
        {
            if (entity == null || WasEntityInAValidPosition(entity)) return;

            PoolableObject shapePoolableObjectBehaviour = PoolManager.i.GetPoolable(entity.meshesInfo.meshRootGameObject);

            if (shapePoolableObjectBehaviour != null)
                shapePoolableObjectBehaviour.OnRelease -= invalidMeshesInfo[entity.gameObject].ResetMaterials;

            var renderers = entity.meshesInfo.renderers;

            if (renderers != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (invalidSubmeshes.Contains(renderers[i]))
                        invalidSubmeshes.Remove(renderers[i]);
                }
            }

            invalidMeshesInfo[entity.gameObject].ResetMaterials();
        }

        void AddInvalidMeshEffect(DecentralandEntity entity)
        {
            if (!WasEntityInAValidPosition(entity)) return;

            InvalidMeshInfo invalidMeshInfo = new InvalidMeshInfo(entity.meshesInfo);

            invalidMeshInfo.OnResetMaterials = () => { invalidMeshesInfo.Remove(entity.gameObject); };

            PoolableObject shapePoolableObjectBehaviour = PoolManager.i.GetPoolable(entity.meshesInfo.meshRootGameObject);
            if (shapePoolableObjectBehaviour != null)
            {
                shapePoolableObjectBehaviour.OnRelease -= invalidMeshInfo.ResetMaterials;
                shapePoolableObjectBehaviour.OnRelease += invalidMeshInfo.ResetMaterials;
            }

            // Apply invalid material
            Renderer[] entityRenderers = entity.meshesInfo.renderers;
            for (int i = 0; i < entityRenderers.Length; i++)
            {
                // Save original materials
                invalidMeshInfo.originalMaterials.Add(entityRenderers[i], entityRenderers[i].sharedMaterial);

                if (invalidSubmeshes.Contains(entityRenderers[i]))
                {
                    // Wireframe that shows the boundaries to the dev (We don't use the GameObject.Instantiate(prefab, parent)
                    // overload because we need to set the position and scale before parenting, to deal with scaled objects)
                    GameObject wireframeObject = GameObject.Instantiate(Resources.Load<GameObject>(WIREFRAME_PREFAB_NAME));
                    wireframeObject.transform.position = entityRenderers[i].bounds.center;
                    wireframeObject.transform.localScale = entityRenderers[i].bounds.size * 1.01f;
                    wireframeObject.transform.SetParent(entity.gameObject.transform);

                    entityRenderers[i].sharedMaterial = invalidSubMeshMaterial;

                    invalidMeshInfo.wireframeObjects.Add(wireframeObject);
                }
                else
                {
                    entityRenderers[i].sharedMaterial = invalidMeshMaterial;
                }
            }

            invalidMeshesInfo.Add(entity.gameObject, invalidMeshInfo);
        }

        public bool WasEntityInAValidPosition(DecentralandEntity entity)
        {
            return !invalidMeshesInfo.ContainsKey(entity.gameObject);
        }

        public Dictionary<Renderer, Material> GetOriginalMaterials(DecentralandEntity entity)
        {
            if (invalidMeshesInfo.ContainsKey(entity.gameObject))
            {
                return invalidMeshesInfo[entity.gameObject].originalMaterials;
            }

            return null;
        }

        protected override void OnRemoveEntity(DecentralandEntity entity)
        {
            base.OnRemoveEntity(entity);

            if (entity.gameObject != null)
            {
                RemoveInvalidMeshEffect(entity);
            }
        }
    }
}