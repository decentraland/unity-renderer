using DCL.Configuration;
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

        // TODO: Deal with 'safe' bounds as in MeshesInfoUtils
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
            Bounds returnedBounds = collider.bounds;

            // Disabled colliders return a size-0 bounds object, so we enable it only to get its bounds
            if (!collider.enabled)
            {
                GameObject colliderGO = collider.gameObject;
                int colliderLayer = colliderGO.layer;

                // Temporarily change the collider GO layer to avoid it colliding with anything.
                colliderGO.layer = PhysicsLayers.gizmosLayer;

                // Enable collider to copy its real bounds
                collider.enabled = true;
                returnedBounds = collider.bounds;

                // Reset modified values
                collider.enabled = false;
                colliderGO.layer = colliderLayer;
            }

            return returnedBounds;
        }

        public static bool IsFullyDefaulted(this IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent,
            IParcelScene scene, IDCLEntity entity)
        {
            var model = sbcInternalComponent.GetFor(scene, entity)?.model;

            // TODO: Just check for bounds size instead of the 3 collections?
            return model == null || (
                model.entityPosition == Vector3.zero
                && (model.renderers == null || model.renderers.Count == 0)
                && (model.physicsColliders == null || model.physicsColliders.Count == 0)
                && (model.pointerColliders == null || model.pointerColliders.Count == 0)
            );
        }
    }
}
