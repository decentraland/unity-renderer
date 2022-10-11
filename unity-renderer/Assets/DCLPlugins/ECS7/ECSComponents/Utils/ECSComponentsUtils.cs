using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Configuration;
using DCL.ECSComponents;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

public static class ECSComponentsUtils
{
    public static void RemoveMaterialTransition(GameObject go)
    {
        MaterialTransitionController[] materialTransitionControllers = go.GetComponentsInChildren<MaterialTransitionController>();

        for (var i = 0; i < materialTransitionControllers.Length; i++)
        {
            GameObject.Destroy(materialTransitionControllers[i]);
        }
    }
    
    public static void UpdateMeshInfo(bool isVisible, bool withCollisions, bool isPointerBlocker, MeshesInfo meshesInfo)
    {
        foreach (Renderer renderer in meshesInfo.renderers)
        {
            renderer.enabled = isVisible;
        }

        UpdateMeshInfoColliders(withCollisions, isPointerBlocker, meshesInfo);
    }

    public static void UpdateMeshInfoColliders(bool withCollisions, bool isPointerBlocker, MeshesInfo meshesInfo)
    {
        int colliderLayer = isPointerBlocker ? PhysicsLayers.onPointerEventLayer : PhysicsLayers.defaultLayer;
        
        foreach (Collider collider in meshesInfo.colliders)
        {
            if (collider == null) continue;
            
            collider.enabled = withCollisions;
            collider.gameObject.layer = colliderLayer;
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
    
    public static void DisposeMeshInfo(MeshesInfo meshesInfo)
    {
        // Dispose renderer
        foreach (Renderer renderer in meshesInfo.renderers)
        {
            Utils.CleanMaterials(renderer);
            GameObject.Destroy(renderer);
        }
        
        // Dispose Mesh filter
        foreach (MeshFilter meshFilter in meshesInfo.meshFilters)
        {
            GameObject.Destroy(meshFilter);
        }
        
        // Dispose collider
        foreach (Collider collider in meshesInfo.colliders)
        {
            if (collider == null) continue;
            
            GameObject.Destroy(collider);
        }
        
        meshesInfo.CleanReferences();
    }
    
    public static int CalculateNFTCollidersLayer(bool withCollisions, bool isPointerBlocker)
    {
        // We can't enable this layer changer logic until we redeploy all the builder and street scenes with the corrected 'withCollision' default in true...
        /* if (!model.withCollisions && model.isPointerBlocker)
            return PhysicsLayers.onPointerEventLayer;
        else */
        if (withCollisions && !isPointerBlocker)
            return PhysicsLayers.characterOnlyLayer;

        return PhysicsLayers.defaultLayer;
    }
    
    private static int CalculateCollidersLayer(bool withCollisions, bool isPointerBlocker)
    {
        if (isPointerBlocker)
            return PhysicsLayers.onPointerEventLayer;
        else if (withCollisions)
            return PhysicsLayers.characterOnlyLayer;

        return PhysicsLayers.defaultLayer;
    }
}
