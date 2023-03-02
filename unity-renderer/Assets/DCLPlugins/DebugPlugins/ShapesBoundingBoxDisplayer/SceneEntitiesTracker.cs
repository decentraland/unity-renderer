using System;
using System.Collections.Generic;
using DCL;
using DCL.Models;
using DCL.Shaders;
using DCLPlugins.DebugPlugins.Commons;
using UnityEngine;
using Object = UnityEngine.Object;

internal class SceneEntitiesTracker : ISceneListener
{
    internal const string WIREFRAME_GAMEOBJECT_NAME = "ShapeBoundingBoxWireframe";
    internal const string WIREFRAME_PREFAB_NAME = "Prefabs/WireframeCubeMesh";

    private readonly Dictionary<IDCLEntity, WatchEntityShapeHandler> entityShapeHandler = new Dictionary<IDCLEntity, WatchEntityShapeHandler>();
    private readonly IUpdateEventHandler updateEventHandler;
    
    private GameObject wireframeOriginal;
    private Material wireframeMaterial;

    public SceneEntitiesTracker(IUpdateEventHandler updateEventHandler)
    {
        this.updateEventHandler = updateEventHandler;
    }

    void IDisposable.Dispose()
    {
        foreach (var handler in entityShapeHandler.Values)
        {
            handler.Dispose();
        }

        entityShapeHandler.Clear();
        DestroyWireframeOriginal();
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
        entityShapeHandler.Add(entity, new WatchEntityShapeHandler(entity,
            new EntityWireframe(GetWireframeOriginal(), updateEventHandler)));
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
            DestroyWireframeOriginal();
        }
    }

    private GameObject GetWireframeOriginal()
    {
        if (wireframeOriginal != null)
        {
            return wireframeOriginal;
        }
        wireframeOriginal = Object.Instantiate(Resources.Load<GameObject>(WIREFRAME_PREFAB_NAME));
        wireframeOriginal.name = WIREFRAME_GAMEOBJECT_NAME;
        wireframeMaterial = wireframeOriginal.GetComponent<Renderer>().material;
        wireframeMaterial.SetColor(ShaderUtils.EmissionColor, Color.grey);
        wireframeOriginal.SetActive(false);
        return wireframeOriginal;
    }

    private void DestroyWireframeOriginal()
    {
        if (wireframeOriginal == null)
        {
            return;
        }
        Object.Destroy(wireframeOriginal);
        Object.Destroy(wireframeMaterial);
    }
}