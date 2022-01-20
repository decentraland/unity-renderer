using System;
using System.Collections.Generic;
using DCL.Models;
using DCLPlugins.DebugPlugins.Commons;
using UnityEngine;
using Object = UnityEngine.Object;

internal class SceneEntitiesTracker : ISceneListener
{
    private const string COLLIDERS_MATERIAL_PATH = "Materials/HologramMaterial";
    private const string ENTITIES_MATERIAL_PATH = "Materials/DebugEntitiesWithCollidersMaterial";

    private static readonly int materialColorProperty = Shader.PropertyToID("_Color");
    private static readonly int materialRimColorProperty = Shader.PropertyToID("_RimColor");
    private static readonly int materialCullYPlaneProperty = Shader.PropertyToID("_CullYPlane");

    private static Material collidersMaterialResource;
    private static Material entitiesWithColliderMaterialResource;

    private readonly Dictionary<IDCLEntity, WatchEntityShapeHandler> entityShapeHandler = new Dictionary<IDCLEntity, WatchEntityShapeHandler>();
    private Material collidersMaterial;

    void IDisposable.Dispose()
    {
        foreach (var handler in entityShapeHandler.Values)
        {
            handler.Dispose();
        }

        entityShapeHandler.Clear();
        DestroyCollidersMaterial();
    }

    void ISceneListener.OnEntityAdded(IDCLEntity entity)
    {
        WatchEntityShape(entity);
    }

    void ISceneListener.OnEntityRemoved(IDCLEntity entity)
    {
        KillWatchEntityShape(entity);
    }

    private void WatchEntityShape(IDCLEntity entity)
    {
        if (entityShapeHandler.ContainsKey(entity))
        {
            return;
        }
        entityShapeHandler.Add(entity, new WatchEntityShapeHandler(
            entity,
            new EntityStyle(GetColliderMaterialOriginal(), GetEntitiesWithColliderMaterialResource())
        ));
    }

    private void KillWatchEntityShape(IDCLEntity entity)
    {
        if (entityShapeHandler.TryGetValue(entity, out WatchEntityShapeHandler entityWatchHandler))
        {
            entityWatchHandler.Dispose();
            entityShapeHandler.Remove(entity);
        }

        if (entityShapeHandler.Count == 0)
        {
            DestroyCollidersMaterial();
        }
    }

    private Material GetColliderMaterialOriginal()
    {
        if (collidersMaterial != null)
        {
            return collidersMaterial;
        }

        if (collidersMaterialResource == null)
        {
            collidersMaterialResource = Resources.Load(COLLIDERS_MATERIAL_PATH) as Material;
        }

        collidersMaterial = new Material(collidersMaterialResource);
        collidersMaterial.SetColor(materialColorProperty, new Color(1, 0, 1, 0.3f));
        collidersMaterial.SetColor(materialRimColorProperty, new Color(1, 0, 1, 0));
        collidersMaterial.SetFloat(materialCullYPlaneProperty, 1000);

        return collidersMaterial;
    }

    private Material GetEntitiesWithColliderMaterialResource()
    {
        if (entitiesWithColliderMaterialResource == null)
        {
            entitiesWithColliderMaterialResource = Resources.Load(ENTITIES_MATERIAL_PATH) as Material;
        }
        return entitiesWithColliderMaterialResource;
    }

    private void DestroyCollidersMaterial()
    {
        if (collidersMaterial != null)
        {
            Object.Destroy(collidersMaterial);
        }
        if (collidersMaterialResource != null)
        {
            Resources.UnloadAsset(collidersMaterialResource);
        }
        if (entitiesWithColliderMaterialResource != null)
        {
            Resources.UnloadAsset(entitiesWithColliderMaterialResource);
        }
    }
}