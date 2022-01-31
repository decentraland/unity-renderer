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
            public List<GameObject> wireframeObjects = new List<GameObject>();
            public MeshesInfo meshesInfo;
            public System.Action OnResetMaterials;

            public InvalidMeshInfo(MeshesInfo meshesInfo) { this.meshesInfo = meshesInfo; }

            public void ResetMaterials(MeshesInfo meshesInfo)
            {
                this.meshesInfo = meshesInfo;
                ResetMaterials();
            }

            public void ResetMaterials()
            {
                if (meshesInfo.meshRootGameObject == null)
                    return;

                for (int i = 0; i < meshesInfo.renderers.Length; i++)
                {
                    meshesInfo.renderers[i].renderingLayerMask = 0;
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

        private const uint INVALID_MESH_RENDERING_LAYER_MASK = 1 << 1;

        Dictionary<GameObject, InvalidMeshInfo> invalidMeshesInfo = new Dictionary<GameObject, InvalidMeshInfo>();
        HashSet<Renderer> invalidSubmeshes = new HashSet<Renderer>();
        private readonly List<MeshesInfo> currentMeshesInvalidated = new List<MeshesInfo>();

        public SceneBoundsFeedbackStyle_RedFlicker()
        {
            invalidMeshesInfo = new Dictionary<GameObject, InvalidMeshInfo>();
        }

        public int GetInvalidMeshesCount() { return invalidMeshesInfo.Count; }

        public void ApplyFeedback(MeshesInfo meshesInfo, bool isInsideBoundaries)
        {
            if (isInsideBoundaries)
            {
                RemoveInvalidMeshEffect(meshesInfo);
                return;
            }

            AddInvalidMeshEffect(meshesInfo);
        }

        public void CleanFeedback()
        {
            for ( int x = 0; x < currentMeshesInvalidated.Count; x++)
            {
                RemoveInvalidMeshEffect(currentMeshesInvalidated[x]);
            }

            currentMeshesInvalidated.Clear();
        }

        void RemoveInvalidMeshEffect(MeshesInfo meshesInfo)
        {
            if (meshesInfo == null || WasGameObjectInAValidPosition(meshesInfo.innerGameObject))
                return;

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
            currentMeshesInvalidated.Remove(meshesInfo);
        }

        void AddInvalidMeshEffect(MeshesInfo meshesInfo)
        {
            if (!WasGameObjectInAValidPosition(meshesInfo.innerGameObject))
                return;

            if (currentMeshesInvalidated.Contains(meshesInfo))
                return;

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
                entityRenderers[i].renderingLayerMask = INVALID_MESH_RENDERING_LAYER_MASK;

                if (invalidSubmeshes.Contains(entityRenderers[i]))
                    continue;

                // Wireframe that shows the boundaries to the dev (We don't use the GameObject.Instantiate(prefab, parent)
                // overload because we need to set the position and scale before parenting, to deal with scaled objects)
                GameObject wireframeObject = Object.Instantiate(Resources.Load<GameObject>(WIREFRAME_PREFAB_NAME));
                wireframeObject.transform.position = entityRenderers[i].bounds.center;
                wireframeObject.transform.localScale = entityRenderers[i].bounds.size * 1.01f;
                wireframeObject.transform.SetParent(meshesInfo.innerGameObject.transform);

                invalidMeshInfo.wireframeObjects.Add(wireframeObject);
                invalidSubmeshes.Add(entityRenderers[i]);
            }

            currentMeshesInvalidated.Add(meshesInfo);
            invalidMeshesInfo.Add(meshesInfo.innerGameObject, invalidMeshInfo);
        }

        public bool WasGameObjectInAValidPosition(GameObject gameObject) { return !invalidMeshesInfo.ContainsKey(gameObject); }

        public List<Material> GetOriginalMaterials(MeshesInfo meshesInfo)
        {
            List<Material> result = new List<Material>();

            for (int i = 0; i < meshesInfo.renderers.Length; i++)
            {
                result.AddRange(meshesInfo.renderers[i].sharedMaterials);
            }

            return result;
        }
    }
}