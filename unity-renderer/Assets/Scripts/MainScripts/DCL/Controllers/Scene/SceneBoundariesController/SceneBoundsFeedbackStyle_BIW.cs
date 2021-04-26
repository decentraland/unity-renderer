using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using UnityEngine.XR;

public class SceneBoundsFeedbackStyle_BIW : ISceneBoundsFeedbackStyle
{
    class InvalidMeshInfo
    {
        public List<GameObject> wireframeObjects = new List<GameObject>();
        public MeshesInfo meshesInfo;
        public System.Action OnResetMaterials;

        public InvalidMeshInfo(MeshesInfo meshesInfo) { this.meshesInfo = meshesInfo; }
        
        public void ResetMaterials()
        {
            if (meshesInfo.meshRootGameObject == null)
                return;
            
            int wireframeObjectscount = wireframeObjects.Count;
            for (int i = 0; i < wireframeObjectscount; i++)
            {
                Utils.SafeDestroy(wireframeObjects[i]);
            }

            OnResetMaterials?.Invoke();
        }
    }

    const string WIREFRAME_PREFAB_NAME = "Prefabs/WireframeCubeMesh";
    
    Dictionary<GameObject, InvalidMeshInfo> invalidMeshesInfo = new Dictionary<GameObject, InvalidMeshInfo>();
    HashSet<Renderer> invalidSubmeshes = new HashSet<Renderer>();
    private readonly List<MeshesInfo> currentMeshesInvalidated = new List<MeshesInfo>();
    
    public SceneBoundsFeedbackStyle_BIW()
    {
        invalidMeshesInfo = new Dictionary<GameObject, InvalidMeshInfo>();
    }

    public void OnRendererExitBounds(Renderer renderer) { invalidSubmeshes.Add(renderer); }

    public void ApplyFeedback(MeshesInfo meshesInfo, bool isInsideBoundaries)
    {
        if (isInsideBoundaries)
        {
            RemoveInvalidMeshEffect(meshesInfo);
            return;
        }

        AddInvalidMeshEffect(meshesInfo);
    }
    public List<Material> GetOriginalMaterials(MeshesInfo meshesInfo) { return new List<Material>(); }

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
        if(currentMeshesInvalidated.Contains(meshesInfo))
            return;
        if (!WasGameObjectInAValidPosition(meshesInfo.innerGameObject))
            return;

        InvalidMeshInfo invalidMeshInfo = new InvalidMeshInfo(meshesInfo);

        invalidMeshInfo.OnResetMaterials = () => { invalidMeshesInfo.Remove(meshesInfo.innerGameObject); };

        PoolableObject shapePoolableObjectBehaviour = PoolManager.i.GetPoolable(meshesInfo.meshRootGameObject);
        if (shapePoolableObjectBehaviour != null)
        {
            shapePoolableObjectBehaviour.OnRelease -= invalidMeshInfo.ResetMaterials;
            shapePoolableObjectBehaviour.OnRelease += invalidMeshInfo.ResetMaterials;
        }

        // Apply invalid mesh
        Renderer[] entityRenderers = meshesInfo.renderers;
        for (int i = 0; i < entityRenderers.Length; i++)
        {
            // Wireframe that shows the boundaries to the dev (We don't use the GameObject.Instantiate(prefab, parent)
            // overload because we need to set the position and scale before parenting, to deal with scaled objects)
            GameObject wireframeObject = GameObject.Instantiate(Resources.Load<GameObject>(WIREFRAME_PREFAB_NAME));
            wireframeObject.transform.position = entityRenderers[i].bounds.center;
            wireframeObject.transform.localScale = entityRenderers[i].bounds.size * 1.01f;
            wireframeObject.transform.SetParent(meshesInfo.innerGameObject.transform);

            invalidMeshInfo.wireframeObjects.Add(wireframeObject);
        }
        
        currentMeshesInvalidated.Add(meshesInfo);
        invalidMeshesInfo.Add(meshesInfo.innerGameObject, invalidMeshInfo);
    }

    public bool WasGameObjectInAValidPosition(GameObject gameObject) { return !invalidMeshesInfo.ContainsKey(gameObject); }

}
