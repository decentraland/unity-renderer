using DCL.Models;
using DCLPlugins.PreviewModePlugin.Commons;
using UnityEngine;

internal class EntityWireframe : IShapeListener
{
    private readonly GameObject wireframeOriginal;

    private GameObject entityWireframe;

    public EntityWireframe(GameObject wireframeOriginal)
    {
        this.wireframeOriginal = wireframeOriginal;
    }

    public void Dispose()
    {
        Object.Destroy(entityWireframe);
    }

    public void OnShapeUpdated(IDCLEntity entity)
    {
        entityWireframe ??= Object.Instantiate(wireframeOriginal);

        Transform wireframeT = entityWireframe.transform;

        wireframeT.position = entity.meshesInfo.mergedBounds.center;
        wireframeT.localScale = entity.meshesInfo.mergedBounds.size * 1.01f;

        wireframeT.SetParent(entity.gameObject.transform);
        entityWireframe.SetActive(true);
    }
}