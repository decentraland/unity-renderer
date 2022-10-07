using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL.ECS7.InternalComponents
{
    public static class InternalTexturizableExtensions
    {
        public static void RemoveRenderers(this IInternalECSComponent<InternalTexturizable> texurizableInternalComponent,
            IParcelScene scene, IDCLEntity entity, IList<Renderer> renderers)
        {
            if (renderers == null)
                return;

            var model = texurizableInternalComponent.GetFor(scene, entity)?.model;

            if (model == null)
                return;

            for (int i = 0; i < renderers.Count; i++)
            {
                model.renderers.Remove(renderers[i]);
            }
            texurizableInternalComponent.PutFor(scene, entity, model);
        }

        public static void AddRenderers(this IInternalECSComponent<InternalTexturizable> texurizableInternalComponent,
            IParcelScene scene, IDCLEntity entity, IList<Renderer> renderers)
        {
            if (renderers == null)
                return;

            var model = texurizableInternalComponent.GetFor(scene, entity)?.model ?? new InternalTexturizable();
            for (int i = 0; i < renderers.Count; i++)
            {
                model.renderers.Add(renderers[i]);
            }
            texurizableInternalComponent.PutFor(scene, entity, model);
        }

        public static void RemoveRenderer(this IInternalECSComponent<InternalTexturizable> texurizableInternalComponent,
            IParcelScene scene, IDCLEntity entity, Renderer renderer)
        {
            if (renderer == null)
                return;

            var model = texurizableInternalComponent.GetFor(scene, entity)?.model;

            if (model == null)
                return;

            model.renderers.Remove(renderer);
            texurizableInternalComponent.PutFor(scene, entity, model);
        }

        public static void AddRenderer(this IInternalECSComponent<InternalTexturizable> texurizableInternalComponent,
            IParcelScene scene, IDCLEntity entity, Renderer renderer)
        {
            if (renderer == null)
                return;

            var model = texurizableInternalComponent.GetFor(scene, entity)?.model ?? new InternalTexturizable();
            model.renderers.Add(renderer);
            texurizableInternalComponent.PutFor(scene, entity, model);
        }
    }
}