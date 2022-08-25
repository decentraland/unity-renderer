using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

public class InternalMaterialHandler : IECSComponentHandler<InternalMaterial>
{
    internal IList<Renderer> renderers;

    public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

    public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Count; i++)
        {
            Renderer r = renderers[i];
            if (r is null)
                continue;
            r.sharedMaterial = null;
        }
    }

    public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, InternalMaterial model)
    {
        if (model.renderers != null)
        {
            renderers = model.renderers;
        }
    }
}