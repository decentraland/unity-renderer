using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

public static class ECSComponentsUtils
{
    public static MeshesInfo GenerateMeshInfo(IDCLEntity entity, Mesh mesh, GameObject gameObject,bool visible, bool withCollisions, bool isPointerBlocker)
    {
        MeshesInfo meshesInfo = new MeshesInfo();
        meshesInfo.innerGameObject = gameObject;
        meshesInfo.meshRootGameObject = gameObject;
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = Utils.EnsureResourcesMaterial("Materials/Default");
        Renderer[] renderers = new Renderer[] { meshRenderer };
        
        meshFilter.sharedMesh = mesh;

        // We should remove this relation in the future, the entity shouldn't know about the mesh
        entity.meshesInfo = meshesInfo;
        
        UpdateRenderer(entity,gameObject, renderers, visible, withCollisions, isPointerBlocker);
        meshesInfo.UpdateRenderersCollection();
        return meshesInfo;
    }
    
    public static void UpdateRenderer(IDCLEntity entity,GameObject meshGameObject, Renderer[] renderers,bool visible, bool withCollisions, bool isPointerBlocker)
    {
        ConfigureVisibility(meshGameObject, visible,renderers);
        
        CollidersManager.i.ConfigureColliders(meshGameObject, withCollisions, false, entity, CalculateCollidersLayer(withCollisions,isPointerBlocker));
    }
    
    public static void ConfigureVisibility(GameObject meshGameObject, bool shouldBeVisible, Renderer[] meshRenderers = null)
    {
        if (meshGameObject == null)
            return;

        if (!shouldBeVisible)
        {
            MaterialTransitionController[] materialTransitionControllers = meshGameObject.GetComponentsInChildren<MaterialTransitionController>();

            for (var i = 0; i < materialTransitionControllers.Length; i++)
            {
                Object.Destroy(materialTransitionControllers[i]);
            }
        }

        if (meshRenderers == null)
            meshRenderers = meshGameObject.GetComponentsInChildren<Renderer>(true);

        Collider onPointerEventCollider;

        for (var i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].enabled = shouldBeVisible;

            if (meshRenderers[i].transform.childCount > 0)
            {
                onPointerEventCollider = meshRenderers[i].transform.GetChild(0).GetComponent<Collider>();

                if (onPointerEventCollider != null && onPointerEventCollider.gameObject.layer == PhysicsLayers.onPointerEventLayer)
                    onPointerEventCollider.enabled = shouldBeVisible;
            }
        }
    }
    
    public static void RemoveRendereableFromDataStore(string sceneId, Rendereable rendereable)
    {
        DataStore.i.sceneWorldObjects.RemoveRendereable(sceneId, rendereable);
    }

    public static Rendereable AddRendereableToDataStore(string sceneId, long entityId, Mesh mesh, GameObject gameObject, Renderer[] renderers)
    {
        int triangleCount = mesh.triangles.Length;

        var newRendereable =
            new Rendereable()
            {
                container = gameObject,
                totalTriangleCount = triangleCount,
                meshes = new HashSet<Mesh>() { mesh },
                meshToTriangleCount = new Dictionary<Mesh, int>() { { mesh, triangleCount } }
            };

        newRendereable.renderers = new HashSet<Renderer>(renderers); 
        newRendereable.ownerId = entityId;

        DataStore.i.sceneWorldObjects.AddRendereable(sceneId, newRendereable);
        return newRendereable;
    }

    public static void DisposePrimitiveShape(AssetPromise_PrimitiveMesh primitiveMeshPromisePrimitive,MeshesInfo meshesInfo, string sceneId, Rendereable rendereable)
    {
        if (primitiveMeshPromisePrimitive != null)
            AssetPromiseKeeper_PrimitiveMesh.i.Forget(primitiveMeshPromisePrimitive);
        
        foreach (Renderer renderer in meshesInfo.renderers)
        {
            Utils.CleanMaterials(renderer);
        }
        meshesInfo.CleanReferences();
        RemoveRendereableFromDataStore(sceneId,rendereable);
    }

    private static int CalculateCollidersLayer(bool withCollisions, bool isPointerBlocker)
    {
        if (!withCollisions && isPointerBlocker)
            return PhysicsLayers.onPointerEventLayer;
        else if (withCollisions && !isPointerBlocker)
            return PhysicsLayers.characterOnlyLayer;

        return PhysicsLayers.defaultLayer;
    }
}
