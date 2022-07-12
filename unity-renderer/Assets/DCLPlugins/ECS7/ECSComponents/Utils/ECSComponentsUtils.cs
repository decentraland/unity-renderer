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
        for (var i = 0; i < meshesInfo.renderers.Length; i++)
        {
            UpdateRendererVisibility(meshesInfo.renderers[i], isVisible);
        }

        UpdateMeshInfoColliders(withCollisions, isPointerBlocker, meshesInfo);
    }

    public static void UpdateMeshInfoColliders(bool withCollisions, bool isPointerBlocker, MeshesInfo meshesInfo)
    {
        int colliderLayer = isPointerBlocker ? PhysicsLayers.onPointerEventLayer : PhysicsLayers.collisionsLayer;
        
        foreach (Collider collider in meshesInfo.colliders)
        {
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
        ConfigureRenderer(entity, meshFilters, gameObject, renderers, visible, withCollisions, isPointerBlocker);
        
        return meshesInfo;
    }
    
    public static void ConfigureRenderer(IDCLEntity entity, MeshFilter[] meshFilters, GameObject meshGameObject, Renderer[] renderers,bool visible, bool withCollisions, bool isPointerBlocker)
    {
        ConfigurePrimitiveShapeVisibility(meshGameObject, visible,renderers);
        
        // TODO: For better perfomance we should create the correct collider to each component shape instead of creating a meshCollider
        CollidersManager.i.CreatePrimitivesColliders(entity.meshRootGameObject, meshFilters, withCollisions, isPointerBlocker, entity);
    }

    public static void ConfigurePrimitiveShapeVisibility(GameObject meshGameObject, bool isVisible, Renderer[] meshRenderers = null)
    {
        if (meshGameObject == null)
            return;
        
        for (var i = 0; i < meshRenderers.Length; i++)
        {
            UpdateRendererVisibility(meshRenderers[i], isVisible);
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

        meshesInfo.meshRootGameObject.layer = PhysicsLayers.defaultLayer;
        meshesInfo.CleanReferences();
    }
    
    private static void UpdateRendererVisibility(Renderer renderer, bool isVisible)
    {
        renderer.enabled = isVisible;
        if (renderer.gameObject.layer == PhysicsLayers.onPointerEventLayer)
        {
            var collider = renderer.gameObject.GetComponent<Collider>();
            collider.enabled = isVisible;
        }
    }
}
