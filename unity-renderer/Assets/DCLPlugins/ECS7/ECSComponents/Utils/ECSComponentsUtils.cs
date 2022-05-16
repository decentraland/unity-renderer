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
        
        UpdateRenderer(entity, meshFilter, gameObject, renderers, visible, withCollisions, isPointerBlocker);
        meshesInfo.UpdateRenderersCollection();
        return meshesInfo;
    }
    
    public static void UpdateRenderer(IDCLEntity entity, MeshFilter meshFilter, GameObject meshGameObject, Renderer[] renderers,bool visible, bool withCollisions, bool isPointerBlocker)
    {
        ConfigurePrimitiveShapeVisibility(meshGameObject, visible,renderers);
        
        // TODO: For better perfomance we should create the correct collider to each primitive shape instead of creating a meshCollider
        CollidersManager.i.ConfigureCollider(entity, meshFilter, withCollisions, CalculateCollidersLayer(withCollisions,isPointerBlocker));
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

    public static void DisposeMeshInfo(MeshesInfo mesheshInfo)
    {
        foreach (Renderer renderer in mesheshInfo.renderers)
        {
            Utils.CleanMaterials(renderer);
        }
        mesheshInfo.CleanReferences();
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
