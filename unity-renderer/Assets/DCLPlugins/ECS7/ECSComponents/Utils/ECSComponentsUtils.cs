using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Configuration;
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
            
        foreach (Collider collider in meshesInfo.colliders)
        {
            collider.enabled = withCollisions;
        }
            
        //TODO: Implement isPointerBlocker when it is defined
    }

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
        
        UpdateRenderer(entity, meshFilter, gameObject, renderers, visible, withCollisions, isPointerBlocker);
        meshesInfo.UpdateRenderersCollection(renderers,entity.meshesInfo.meshFilters);
        return meshesInfo;
    }
    
    public static void UpdateRenderer(IDCLEntity entity, MeshFilter meshFilter, GameObject meshGameObject, Renderer[] renderers,bool visible, bool withCollisions, bool isPointerBlocker)
    {
        ConfigurePrimitiveShapeVisibility(meshGameObject, visible,renderers);
        
        // TODO: For better perfomance we should create the correct collider to each component shape instead of creating a meshCollider
        CollidersManager.i.ConfigureColliders(entity.meshRootGameObject,false, withCollisions, entity,CalculateCollidersLayer(withCollisions,isPointerBlocker));
    }
    
    public static void ConfigurePrimitiveShapeVisibility(GameObject meshGameObject, bool shouldBeVisible, Renderer[] meshRenderers = null)
    {
        if (meshGameObject == null)
            return;

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
        if (!withCollisions && isPointerBlocker)
            return PhysicsLayers.onPointerEventLayer;
        else if (withCollisions && !isPointerBlocker)
            return PhysicsLayers.characterOnlyLayer;

        return PhysicsLayers.defaultLayer;
    }
}
