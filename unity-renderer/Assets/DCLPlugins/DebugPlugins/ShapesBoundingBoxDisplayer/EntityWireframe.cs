using System;
using System.Collections.Generic;
using DCL;
using DCL.Models;
using DCLPlugins.DebugPlugins.Commons;
using UnityEngine;
using Object = UnityEngine.Object;

internal class EntityWireframe : IShapeListener
{
    internal const float WIREFRAME_SIZE_MULTIPLIER = 1.01f;

    private readonly GameObject wireframeOriginal;
    private readonly IUpdateEventHandler updateEventHandler;

    internal readonly List<GameObject> entityWireframes = new List<GameObject>();
    private MeshesInfo meshesInfo;

    public EntityWireframe(GameObject wireframeOriginal, IUpdateEventHandler updateEventHandler)
    {
        this.wireframeOriginal = wireframeOriginal;
        this.updateEventHandler = updateEventHandler;
    }

    void IDisposable.Dispose()
    {
        CleanWireframe();
    }

    void IShapeListener.OnShapeUpdated(IDCLEntity entity)
    {
        updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);

        meshesInfo = entity.meshesInfo;
        int wireframesCount = entityWireframes.Count;
        int renderersCount = meshesInfo.renderers.Length;

        for (int i = wireframesCount; i < renderersCount; i++)
        {
            var wireframe = Object.Instantiate(wireframeOriginal);
            wireframe.transform.localScale = Vector3.zero;
            wireframe.SetActive(true);
            entityWireframes.Add(wireframe);
        }

        for (int i = wireframesCount; i > renderersCount; i--)
        {
            int index = i - 1;
            DestroyWireframe(entityWireframes[index]);
            entityWireframes.RemoveAt(index);
        }

        updateEventHandler.AddListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);
    }

    void IShapeListener.OnShapeCleaned(IDCLEntity entity)
    {
        CleanWireframe();
    }

    internal void LateUpdate()
    {
        int renderersCount = meshesInfo.renderers.Length;

        for (int i = 0; i < renderersCount; i++)
        {
            Transform wireframeT = entityWireframes[i].transform;
            Renderer renderer = meshesInfo.renderers[i];
            Bounds rendererBounds = renderer.bounds;

            wireframeT.position = rendererBounds.center;
            wireframeT.localScale = rendererBounds.size * WIREFRAME_SIZE_MULTIPLIER;

            wireframeT.gameObject.SetActive(true);
        }
    }

    private void CleanWireframe()
    {
        for (int i = 0; i < entityWireframes.Count; i++)
        {
            DestroyWireframe(entityWireframes[i]);
        }
        entityWireframes.Clear();
        updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.LateUpdate, LateUpdate);
    }

    private void DestroyWireframe(GameObject wireframe)
    {
        if (wireframe == null)
            return;

        wireframe.SetActive(false);
        Object.Destroy(wireframe);
    }
}