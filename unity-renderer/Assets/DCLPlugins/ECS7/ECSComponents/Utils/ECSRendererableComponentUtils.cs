using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.Models;
using UnityEngine;

public static class ECSRendererableComponentUtils
{
    public static void RemoveFromTexturizableComponent(IParcelScene scene, IDCLEntity entity, IList<Renderer> renderers,
        IInternalECSComponent<InternalTexturizable> texurizableInternalComponent)
    {
        if (renderers == null)
            return;

        var model = texurizableInternalComponent.GetFor(scene, entity).model;
        for (int i = 0; i < renderers.Count; i++)
        {
            model.renderers.Remove(renderers[i]);
        }
        texurizableInternalComponent.PutFor(scene, entity, model);
    }

    public static void AddToTexturizableComponent(IParcelScene scene, IDCLEntity entity, IList<Renderer> renderers,
        IInternalECSComponent<InternalTexturizable> texurizableInternalComponent)
    {
        if (renderers == null)
            return;

        var model = texurizableInternalComponent.GetFor(scene, entity).model;
        for (int i = 0; i < renderers.Count; i++)
        {
            model.renderers.Add(renderers[i]);
        }
        model.dirty = true;
        texurizableInternalComponent.PutFor(scene, entity, model);
    }
}