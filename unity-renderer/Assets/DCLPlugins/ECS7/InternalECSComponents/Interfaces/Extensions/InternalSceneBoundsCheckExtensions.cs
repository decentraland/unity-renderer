using DCL.Controllers;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECS7.InternalComponents
{
    public static class InternalSceneBoundsCheckExtensions
    {
        public static void SetPosition(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity, Vector3 newEntityPosition, bool createComponentIfMissing = true)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model;

            if (model == null)
            {
                if (!createComponentIfMissing)
                    return;

                model = new InternalSceneBoundsCheck();
            }

            model.entityPosition = newEntityPosition;

            sbcInternalComponent.PutFor(scene, entity, model);
        }

        public static void SetRenderers(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity, IList<Renderer> newRenderersCollection)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model ?? new InternalSceneBoundsCheck();
            model.renderers = newRenderersCollection;

            sbcInternalComponent.PutFor(scene, entity, model);
        }

        public static void SetPhysicsColliders(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity, IList<Collider> newCollidersCollection)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model ?? new InternalSceneBoundsCheck();
            model.physicsColliders = newCollidersCollection;

            sbcInternalComponent.PutFor(scene, entity, model);
        }

        public static void SetPointerColliders(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity, IList<Collider> newCollidersCollection)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model ?? new InternalSceneBoundsCheck();
            model.pointerColliders = newCollidersCollection;

            sbcInternalComponent.PutFor(scene, entity, model);
        }

        public static void RecalculateEntityMeshBounds(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model;

            if (model == null) return;

            Bounds newBounds = new Bounds();

            if (model.renderers != null)
            {
                for (var i = 0; i < model.renderers.Count; i++)
                {
                    newBounds.Encapsulate(model.renderers[i].bounds);
                }
            }

            if (model.physicsColliders != null)
            {
                for (var i = 0; i < model.physicsColliders.Count; i++)
                {
                    newBounds.Encapsulate(model.physicsColliders[i].bounds);
                }
            }

            if (model.pointerColliders != null)
            {
                for (var i = 0; i < model.pointerColliders.Count; i++)
                {
                    newBounds.Encapsulate(model.pointerColliders[i].bounds);
                }
            }

            model.entityMeshBounds = newBounds;

            sbcInternalComponent.PutFor(scene, entity, model);
        }

        public static bool IsFullyDefaulted(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model;

            return model == null || (
                model.entityPosition == Vector3.zero
                && (model.renderers == null || model.renderers.Count == 0)
                && (model.physicsColliders == null || model.physicsColliders.Count == 0)
                && (model.pointerColliders == null || model.pointerColliders.Count == 0)
            );
        }
    }
}
