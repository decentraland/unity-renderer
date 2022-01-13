using System.Collections.Generic;
using DCL.Helpers;
using DCL.Models;
using DCLPlugins.PreviewModePlugin.Commons;
using UnityEngine;

internal class SceneEntitiesTracker : ISceneListener
{
    private const string WIREFRAME_PREFAB_NAME = "Prefabs/WireframeCubeMesh";

    private readonly Dictionary<IDCLEntity, WatchEntityShapeHandler> entityShapeHandler = new Dictionary<IDCLEntity, WatchEntityShapeHandler>();
    private readonly GameObject wireframeOriginal;
    private readonly Material wireframeMaterial;

    public SceneEntitiesTracker()
    {
        wireframeOriginal = Object.Instantiate(Resources.Load<GameObject>(WIREFRAME_PREFAB_NAME));
        wireframeMaterial = wireframeOriginal.GetComponent<Renderer>().material;
        wireframeMaterial.SetColor(ShaderUtils.EmissionColor, Color.grey);
        wireframeOriginal.SetActive(false);
    }

    public void Dispose()
    {
        foreach (var handler in entityShapeHandler.Values)
        {
            handler.Dispose();
        }

        entityShapeHandler.Clear();

        Object.Destroy(wireframeOriginal);
        Object.Destroy(wireframeMaterial);
    }

    public void OnEntityAdded(IDCLEntity entity)
    {
        WatchEntityShape(entity);
    }

    private void WatchEntityShape(IDCLEntity entity)
    {
        if (entityShapeHandler.ContainsKey(entity))
        {
            return;
        }
        entityShapeHandler.Add(entity, new WatchEntityShapeHandler(entity, new EntityWireframe(wireframeOriginal)));
    }
}