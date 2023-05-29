using DCL.Controllers;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECS7.InternalComponents
{
    public static class InternalRenderersExtensions
    {
        public static void RemoveRenderer(this IInternalECSComponent<InternalRenderers> renderersInternalComponent,
            IParcelScene scene, IDCLEntity entity, Renderer renderer)
        {
            if (renderer == null)
                return;

            var model = renderersInternalComponent.GetFor(scene, entity)?.model;

            if (model == null)
                return;

            model.renderers.Remove(renderer);

            if (model.renderers.Count == 0)
            {
                renderersInternalComponent.RemoveFor(scene, entity, new InternalRenderers());
                return;
            }

            renderersInternalComponent.PutFor(scene, entity, model);
        }

        public static void RemoveRenderers(this IInternalECSComponent<InternalRenderers> renderersInternalComponent,
            IParcelScene scene, IDCLEntity entity, IReadOnlyCollection<Renderer> renderers)
        {
            if (renderers == null)
                return;

            var model = renderersInternalComponent.GetFor(scene, entity)?.model;

            if (model == null)
                return;

            foreach (Renderer renderer in renderers)
            {
                model.renderers.Remove(renderer);
            }

            if (model.renderers.Count == 0)
            {
                renderersInternalComponent.RemoveFor(scene, entity, new InternalRenderers());
                return;
            }

            renderersInternalComponent.PutFor(scene, entity, model);
        }

        public static void AddRenderer(this IInternalECSComponent<InternalRenderers> renderersInternalComponent,
            IParcelScene scene, IDCLEntity entity, Renderer renderer)
        {
            if (!renderer)
                return;

            var model = renderersInternalComponent.GetFor(scene, entity)?.model ?? new InternalRenderers();
            model.renderers.Add(renderer);
            renderersInternalComponent.PutFor(scene, entity, model);
        }
    }
}
