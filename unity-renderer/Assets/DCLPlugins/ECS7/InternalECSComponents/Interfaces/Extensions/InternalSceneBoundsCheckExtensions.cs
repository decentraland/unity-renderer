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
            foreach (long entityId in entity.childrenId)
            {
                if (scene.entities.TryGetValue(entityId, out IDCLEntity childEntity))
                {
                    sbcInternalComponent.SetPosition(scene, childEntity, childEntity.gameObject.transform.position, createComponentIfMissing);
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
            IParcelScene scene, IDCLEntity entity, IList<Collider> newCollidersCollection)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model ?? new InternalSceneBoundsCheck();
            model.physicsColliders = newCollidersCollection;
            model.meshesDirty = true;

            sbcInternalComponent.PutFor(scene, entity, model);
        }

        public static void SetPointerColliders(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity, IList<Collider> newCollidersCollection)
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

        public static void RecalculateEntityMeshBounds(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model;

            if (model == null) return;

            // Clean existing bounds object
            model.entityLocalMeshBounds.size = Vector3.zero;

            // Note: the center shouldn't be modified beyond this point as it affects the bounds relative values
            model.entityLocalMeshBounds.center = entity.gameObject.transform.position;

            // Encapsulate with global bounds
            if (model.renderers != null)
            {
                for (var i = 0; i < model.renderers.Count; i++)
                {
                    model.entityLocalMeshBounds.Encapsulate(model.renderers[i].bounds);
                }
            }

            if (model.physicsColliders != null)
            {
                for (var i = 0; i < model.physicsColliders.Count; i++)
                {
                    model.entityLocalMeshBounds.Encapsulate(GetColliderBounds(model.physicsColliders[i]));
                }
            }

            if (model.pointerColliders != null)
            {
                for (var i = 0; i < model.pointerColliders.Count; i++)
                {
                    model.entityLocalMeshBounds.Encapsulate(GetColliderBounds(model.pointerColliders[i]));
                }
            }

            // Turn min-max values to local/relative to be usable when the entity has moved
            Vector3 entityPosition = entity.gameObject.transform.position;

            model.entityLocalMeshBounds.SetMinMax(model.entityLocalMeshBounds.min - entityPosition,
                model.entityLocalMeshBounds.max - entityPosition);

            sbcInternalComponent.PutFor(scene, entity, model);
        }

        private static Bounds GetColliderBounds(Collider collider)
        {
            // Disabled colliders return a size-0 bounds object, so we enable it only to get its bounds
            if (!collider.enabled)
            {
                // Enable collider to copy its real bounds
                collider.enabled = true;
                Bounds returnedBounds = collider.bounds;

                // Reset modified values
                collider.enabled = false;

                return returnedBounds;
            }

            return collider.bounds;
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
