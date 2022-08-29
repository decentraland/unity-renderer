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

    public static MeshesInfo GeneratePrimitive(IDCLEntity entity, Mesh mesh, GameObject gameObject,bool visible, bool withCollisions, bool isPointerBlocker)
    {
        // We create the unity components needed to generate the primitive
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = Utils.EnsureResourcesMaterial("Materials/Default");
        Renderer[] renderers = new Renderer[] { meshRenderer };
        MeshFilter[] meshFilters = new MeshFilter[] { meshFilter };

        // We generate the mesh info on the entity, so we can use on other systems 
        MeshesInfo meshesInfo = new MeshesInfo();
        meshesInfo.innerGameObject = gameObject;
        meshesInfo.meshRootGameObject = gameObject;
        meshesInfo.UpdateRenderersCollection(renderers,meshFilters);
        
        // We generate the representation of the primitive and assign it to the meshInfo
        ShapeRepresentation shape = new ShapeRepresentation();
        shape.UpdateModel(visible, withCollisions);
        meshesInfo.currentShape = shape;
        
        // We should remove this relation in the future, the entity shouldn't know about the mesh
        entity.meshesInfo = meshesInfo;
        
        // We update the rendering
        UpdateRenderer(entity, gameObject, renderers, visible, withCollisions, isPointerBlocker);
        
        return meshesInfo;
    }
    
    public static void UpdateRenderer(IDCLEntity entity, GameObject meshGameObject, Renderer[] renderers,bool visible, bool withCollisions, bool isPointerBlocker)
    {
        ConfigurePrimitiveShapeVisibility(meshGameObject, visible,renderers);
        
        // TODO: For better perfomance we should create the correct collider to each component shape instead of creating a meshCollider
        CollidersManager.i.ConfigureColliders(entity.meshRootGameObject,withCollisions, false, entity,CalculateCollidersLayer(withCollisions,isPointerBlocker));
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
