using DCL.Controllers;
using DCL.Models;
using System;
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

            InternalSceneBoundsCheck finalModel;
            if (model == null)
            {
                if (!createComponentIfMissing)
                    return;

                finalModel = new InternalSceneBoundsCheck(new Bounds());
            }
            else
            {
                finalModel = model.Value;
            }

            finalModel.entityPosition = newEntityPosition;

            sbcInternalComponent.PutFor(scene, entity, finalModel);

            // Update children position in their SBCComponent as well
            IList<long> childrenId = entity.childrenId;
            for (int i = 0; i < childrenId.Count; i++)
            {
                if (scene.entities.TryGetValue(childrenId[i], out IDCLEntity childEntity))
                {
                    sbcInternalComponent.SetPosition(scene, childEntity, childEntity.gameObject.transform.position, createComponentIfMissing);
                }
            }
        }

        public static void OnTransformScaleRotationChanged(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model ?? new InternalSceneBoundsCheck(new Bounds());

            // Mesh bounds need to be recalculated
            model.meshesDirty = true;

            sbcInternalComponent.PutFor(scene, entity, model);

            // Update children 'meshesDirty' in their SBCComponent as well
            IList<long> childrenId = entity.childrenId;
            for (int i = 0; i < childrenId.Count; i++)
            {
                if (scene.entities.TryGetValue(childrenId[i], out IDCLEntity childEntity))
                {
                    sbcInternalComponent.OnTransformScaleRotationChanged(scene, childEntity);
                }
            }
        }

        public static void SetRenderers(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity, IList<Renderer> newRenderersCollection)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model ?? new InternalSceneBoundsCheck(new Bounds());
            model.renderers = newRenderersCollection;
            model.meshesDirty = true;

            sbcInternalComponent.PutFor(scene, entity, model);
        }

        public static void SetPhysicsColliders(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity, KeyValueSet<Collider, uint> newCollidersCollection)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model ?? new InternalSceneBoundsCheck(new Bounds());
            model.physicsColliders = newCollidersCollection;
            model.meshesDirty = true;

            sbcInternalComponent.PutFor(scene, entity, model);
        }

        public static void SetPointerColliders(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity, KeyValueSet<Collider, uint> newCollidersCollection)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model ?? new InternalSceneBoundsCheck(new Bounds());
            model.pointerColliders = newCollidersCollection;
            model.meshesDirty = true;

            sbcInternalComponent.PutFor(scene, entity, model);
        }

        public static void RegisterOnSceneBoundsStateChangeCallback(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity, Action<bool> callback)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model ?? new InternalSceneBoundsCheck(new Bounds());
            model.OnSceneBoundsStateChange += callback;

            sbcInternalComponent.PutFor(scene, entity, model);
        }

        public static void RemoveOnSceneBoundsStateChangeCallback(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity, Action<bool> callback)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model;

            if (model == null)
                return;

            InternalSceneBoundsCheck finalModel = model.Value;
            finalModel.OnSceneBoundsStateChange -= callback;

            sbcInternalComponent.PutFor(scene, entity, finalModel);
        }

        public static bool IsFullyDefaulted(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model;

            return model == null || (model.Value.entityPosition == Vector3.zero
                                     && model.Value.entityLocalMeshBounds.size == Vector3.zero
                                     && model.Value.OnSceneBoundsStateChange == null);
        }
    }
}
