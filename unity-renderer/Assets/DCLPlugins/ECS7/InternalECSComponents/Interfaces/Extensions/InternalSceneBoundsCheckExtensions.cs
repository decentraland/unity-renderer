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
            var model = sbcInternalComponent.GetFor(scene, entity)?.model ?? new InternalSceneBoundsCheck();

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
            var model = sbcInternalComponent.GetFor(scene, entity)?.model ?? new InternalSceneBoundsCheck();
            model.renderers = newRenderersCollection;
            model.meshesDirty = true;

            sbcInternalComponent.PutFor(scene, entity, model);
        }

        public static void SetPhysicsColliders(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity, KeyValueSet<Collider, uint> newCollidersCollection)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model ?? new InternalSceneBoundsCheck();
            model.physicsColliders = newCollidersCollection;
            model.meshesDirty = true;

            sbcInternalComponent.PutFor(scene, entity, model);
        }

        public static void SetPointerColliders(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity, KeyValueSet<Collider, uint> newCollidersCollection)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model ?? new InternalSceneBoundsCheck();
            model.pointerColliders = newCollidersCollection;
            model.meshesDirty = true;

            sbcInternalComponent.PutFor(scene, entity, model);
        }

        public static void SetAudioSource(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity, AudioSource audioSource)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model ?? new InternalSceneBoundsCheck();
            model.audioSource = audioSource;

            sbcInternalComponent.PutFor(scene, entity, model);
        }

        public static bool IsFullyDefaulted(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model;

            return model == null || (model.entityPosition == Vector3.zero
                                     && model.entityLocalMeshBounds.size == Vector3.zero
                                     && model.audioSource == null);
        }
    }
}
