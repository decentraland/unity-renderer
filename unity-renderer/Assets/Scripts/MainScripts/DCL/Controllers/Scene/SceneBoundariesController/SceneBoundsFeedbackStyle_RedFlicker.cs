using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Controllers
{
    public class SceneBoundsFeedbackStyle_RedFlicker : ISceneBoundsFeedbackStyle
    {
        class InvalidMeshInfo
        {
            public Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();
            public List<GameObject> wireframeObjects = new List<GameObject>();
            public MeshesInfo meshesInfo;
            public System.Action OnResetMaterials;

            public InvalidMeshInfo(MeshesInfo meshesInfo)
            {
                this.meshesInfo = meshesInfo;
            }

            public void ResetMaterials(MeshesInfo meshesInfo)
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

        public SceneBoundsFeedbackStyle_RedFlicker()
        {
            invalidMeshesInfo = new Dictionary<GameObject, InvalidMeshInfo>();
            invalidMeshMaterial = Resources.Load(INVALID_MESH_MATERIAL_NAME) as Material;
            invalidSubMeshMaterial = Resources.Load(INVALID_SUBMESH_MATERIAL_NAME) as Material;
        }

        public void OnRendererExitBounds(Renderer renderer)
        {
            invalidSubmeshes.Add(renderer);
        }

        public void ApplyFeedback(MeshesInfo meshesInfo, bool isInsideBoundaries)
        {
            if (isInsideBoundaries)
            {
                RemoveInvalidMeshEffect(meshesInfo);
                return;
            }

            AddInvalidMeshEffect(meshesInfo);
        }

        void RemoveInvalidMeshEffect(MeshesInfo meshesInfo)
        {
            if (meshesInfo == null || WasGameObjectInAValidPosition(meshesInfo.innerGameObject)) return;

            PoolableObject shapePoolableObjectBehaviour = PoolManager.i.GetPoolable(meshesInfo.meshRootGameObject);

            if (shapePoolableObjectBehaviour != null)
                shapePoolableObjectBehaviour.OnRelease -= invalidMeshesInfo[meshesInfo.innerGameObject].ResetMaterials;

            var renderers = meshesInfo.renderers;

            if (renderers != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (invalidSubmeshes.Contains(renderers[i]))
                        invalidSubmeshes.Remove(renderers[i]);
                }
            }

            invalidMeshesInfo[meshesInfo.innerGameObject].ResetMaterials();
        }

        void AddInvalidMeshEffect(MeshesInfo meshesInfo)
        {
            if (!WasGameObjectInAValidPosition(meshesInfo.innerGameObject)) return;

            InvalidMeshInfo invalidMeshInfo = new InvalidMeshInfo(meshesInfo);

            invalidMeshInfo.OnResetMaterials = () => { invalidMeshesInfo.Remove(meshesInfo.innerGameObject); };

            PoolableObject shapePoolableObjectBehaviour = PoolManager.i.GetPoolable(meshesInfo.meshRootGameObject);
            if (shapePoolableObjectBehaviour != null)
            {
                shapePoolableObjectBehaviour.OnRelease -= invalidMeshInfo.ResetMaterials;
                shapePoolableObjectBehaviour.OnRelease += invalidMeshInfo.ResetMaterials;
            }

            // Apply invalid material
            Renderer[] entityRenderers = meshesInfo.renderers;
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
                    wireframeObject.transform.SetParent(meshesInfo.innerGameObject.transform);

                    entityRenderers[i].sharedMaterial = invalidSubMeshMaterial;

                    invalidMeshInfo.wireframeObjects.Add(wireframeObject);
                }
                else
                {
                    entityRenderers[i].sharedMaterial = invalidMeshMaterial;
                }
            }

            invalidMeshesInfo.Add(meshesInfo.innerGameObject, invalidMeshInfo);
        }

        public bool WasGameObjectInAValidPosition(GameObject gameObject)
        {
            return !invalidMeshesInfo.ContainsKey(gameObject);
        }

        public List<Material> GetOriginalMaterials(MeshesInfo meshesInfo)
        {
            if (invalidMeshesInfo.ContainsKey(meshesInfo.innerGameObject))
            {
                return invalidMeshesInfo[meshesInfo.innerGameObject].originalMaterials.Values.ToList();
            }

            List<Material> result = new List<Material>();

            for (int i = 0; i < meshesInfo.renderers.Length; i++)
            {
                result.AddRange(meshesInfo.renderers[i].sharedMaterials);
            }

            return result;
        }
    }
}