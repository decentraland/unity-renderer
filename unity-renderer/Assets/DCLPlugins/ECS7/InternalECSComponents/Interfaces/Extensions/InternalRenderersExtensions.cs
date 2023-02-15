using DCL.Controllers;
using DCL.Models;
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

        public static void AddRenderer(this IInternalECSComponent<InternalRenderers> renderersInternalComponent,
            IParcelScene scene, IDCLEntity entity, Renderer renderer)
        {
            if (renderer == null)
                return;

            var model = renderersInternalComponent.GetFor(scene, entity)?.model ?? new InternalRenderers();
            model.renderers.Add(renderer);
            renderersInternalComponent.PutFor(scene, entity, model);
        }
    }
}
