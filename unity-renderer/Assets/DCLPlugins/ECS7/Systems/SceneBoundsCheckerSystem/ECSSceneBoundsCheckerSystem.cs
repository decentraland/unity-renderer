using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSystems.ECSSceneBoundsCheckerSystem
{
    public class ECSSceneBoundsCheckerSystem
    {
        private readonly IInternalECSComponent<InternalSceneBoundsCheck> sceneBoundsCheckComponent;
        private readonly IInternalECSComponent<InternalVisibility> visibilityComponent;
        private readonly IInternalECSComponent<InternalRenderers> renderersComponent;
        private readonly IInternalECSComponent<InternalColliders> pointerCollidersComponent;
        private readonly IInternalECSComponent<InternalColliders> physicsCollidersComponent;
        private readonly IInternalECSComponent<InternalAudioSource> audioSourceComponent;
        private readonly IECSOutOfSceneBoundsFeedbackStyle outOfBoundsVisualFeedback;

        public ECSSceneBoundsCheckerSystem(IInternalECSComponent<InternalSceneBoundsCheck> sbcComponent,
            IInternalECSComponent<InternalVisibility> visibilityComponent,
            IInternalECSComponent<InternalRenderers> renderersComponent,
            IInternalECSComponent<InternalColliders> pointerColliderComponent,
            IInternalECSComponent<InternalColliders> physicsColliderComponent,
            IInternalECSComponent<InternalAudioSource> audioSourceComponent,
            bool previewMode = false)
        {
            this.sceneBoundsCheckComponent = sbcComponent;
            this.visibilityComponent = visibilityComponent;
            this.renderersComponent = renderersComponent;
            this.pointerCollidersComponent = pointerColliderComponent;
            this.physicsCollidersComponent = physicsColliderComponent;
            this.audioSourceComponent = audioSourceComponent;

            outOfBoundsVisualFeedback = previewMode ? new ECSOutOfSceneBoundsFeedback_RedWireframe() : new ECSOutOfSceneBoundsFeedback_EnabledToggle();
        }

        public void Update()
        {
            ProcessRendererComponentChanges(sceneBoundsCheckComponent, renderersComponent.GetForAll());
            ProcessPhysicColliderComponentChanges(sceneBoundsCheckComponent, physicsCollidersComponent.GetForAll());
            ProcessPointerColliderComponentChanges(sceneBoundsCheckComponent, pointerCollidersComponent.GetForAll());
            ProcessAudioSourceComponentChanges(sceneBoundsCheckComponent, audioSourceComponent.GetForAll());

            // Note: the components are traversed backwards as we may free the 'fully defaulted' entities from the component
            var sbcComponentGroup = sceneBoundsCheckComponent.GetForAll();

            for (int i = sbcComponentGroup.Count - 1; i >= 0; i--)
            {
                var componentData = sbcComponentGroup[i].value;
                IParcelScene scene = componentData.scene;
                IDCLEntity entity = componentData.entity;
                InternalSceneBoundsCheck model = componentData.model;

                if (!model.dirty || scene.isPersistent) continue;

                bool physicsColliderRemoved = WereColliderComponentRemoved(scene, entity,
                    model.physicsColliders, physicsCollidersComponent);

                bool pointerColliderRemoved = WereColliderComponentRemoved(scene, entity,
                    model.pointerColliders, pointerCollidersComponent);

                bool renderersRemoved = WereRendererComponentRemoved(scene, entity,
                    model.renderers, renderersComponent);

                bool isMeshDirty = model.meshesDirty | physicsColliderRemoved | pointerColliderRemoved | renderersRemoved;

                if (isMeshDirty)
                {
                    if (physicsColliderRemoved)
                        model.physicsColliders = null;

                    if (pointerColliderRemoved)
                        model.pointerColliders = null;

                    if (renderersRemoved)
                        model.renderers = null;

                    sceneBoundsCheckComponent.RecalculateEntityMeshBounds(scene, entity);

                    // If all meshes were removed we need to reset the feedback.
                    if (model.entityLocalMeshBounds.size == Vector3.zero)
                    {
                        SetInsideBoundsStateForMeshComponents(outOfBoundsVisualFeedback, entity, model,
                            IsEntityVisible(scene, entity, visibilityComponent), true);
                    }

                    model.meshesDirty = false;
                    sceneBoundsCheckComponent.PutFor(scene, entity, model);
                }

                if (sceneBoundsCheckComponent.IsFullyDefaulted(scene, entity))
                {
                    // Since no other component is using the internal SBC component, we remove it.
                    sceneBoundsCheckComponent.RemoveFor(scene, entity);
                    continue;
                }

                RunEntityEvaluation(scene, entity, model, outOfBoundsVisualFeedback, visibilityComponent);
            }
        }

        private static void RunEntityEvaluation(IParcelScene scene, IDCLEntity entity, InternalSceneBoundsCheck sbcComponentModel,
            IECSOutOfSceneBoundsFeedbackStyle outOfBoundsVisualFeedback, IInternalECSComponent<InternalVisibility> visibilityComponent)
        {
            // If it has a mesh we don't evaluate its position due to artists common "pivot point sloppiness", we evaluate its mesh merged bounds
            if (sbcComponentModel.entityLocalMeshBounds.size != Vector3.zero) // has a mesh/collider
                EvaluateMeshBounds(scene, entity, sbcComponentModel, outOfBoundsVisualFeedback, visibilityComponent);
            else
                EvaluateEntityPosition(scene, entity, sbcComponentModel);

            SetInsideBoundsStateForNonMeshComponents(entity, sbcComponentModel);
        }

        private static void EvaluateEntityPosition(IParcelScene scene, IDCLEntity entity, InternalSceneBoundsCheck sbcComponentModel)
        {
            // 1. Cheap outer-bounds check
            entity.isInsideSceneOuterBoundaries = scene.IsInsideSceneOuterBoundaries(sbcComponentModel.entityPosition);

            // 2. Confirm with inner-bounds check only if entity is inside outer bounds
            Vector3 entityWorldPosition = sbcComponentModel.entityPosition + CommonScriptableObjects.worldOffset.Get();

            entity.isInsideSceneBoundaries = entity.isInsideSceneOuterBoundaries
                                             && scene.IsInsideSceneBoundaries(entityWorldPosition, entityWorldPosition.y);
        }

        private static void EvaluateMeshBounds(IParcelScene scene, IDCLEntity entity, InternalSceneBoundsCheck sbcComponentModel,
            IECSOutOfSceneBoundsFeedbackStyle outOfBoundsVisualFeedback, IInternalECSComponent<InternalVisibility> visibilityComponent)
        {
            Vector3 worldOffset = CommonScriptableObjects.worldOffset.Get();
            Vector3 entityGlobalPosition = sbcComponentModel.entityPosition;
            Vector3 globalBoundsMaxPoint = entityGlobalPosition + sbcComponentModel.entityLocalMeshBounds.max;
            Vector3 globalBoundsMinPoint = entityGlobalPosition + sbcComponentModel.entityLocalMeshBounds.min;

            // 1. Cheap outer-bounds check
            entity.isInsideSceneOuterBoundaries = scene.IsInsideSceneOuterBoundaries(globalBoundsMaxPoint)
                                                  && scene.IsInsideSceneOuterBoundaries(globalBoundsMinPoint);

            if (entity.isInsideSceneOuterBoundaries)
            {
                // 2. If entity is inside outer bounds then check full merged bounds AABB
                entity.isInsideSceneBoundaries = scene.IsInsideSceneBoundaries(globalBoundsMaxPoint + worldOffset, globalBoundsMaxPoint.y)
                                                 && scene.IsInsideSceneBoundaries(globalBoundsMinPoint + worldOffset);

                // 3. If merged bounds is detected as outside bounds we need a final check on submeshes (for L-Shaped subdivided meshes)
                if (!entity.isInsideSceneBoundaries)
                    entity.isInsideSceneBoundaries = AreSubMeshesAndCollidersInsideBounds(scene, entity, sbcComponentModel);
            }
            else
            {
                entity.isInsideSceneBoundaries = false;
            }

            SetInsideBoundsStateForMeshComponents(outOfBoundsVisualFeedback, entity, sbcComponentModel,
                IsEntityVisible(scene, entity, visibilityComponent), entity.isInsideSceneBoundaries);
        }

        private static bool AreSubMeshesAndCollidersInsideBounds(IParcelScene scene, IDCLEntity entity,
            InternalSceneBoundsCheck sbcComponentModel)
        {
            var renderers = sbcComponentModel.renderers;
            var physicsColliders = sbcComponentModel.physicsColliders;
            var pointerColliders = sbcComponentModel.pointerColliders;
            int renderersCount = renderers?.Count ?? 0;
            int collidersCount = physicsColliders?.Count ?? 0 + pointerColliders?.Count ?? 0;

            // For entities with 1 mesh/collider the already-checked merged bounds already represent its bounds
            // So we avoid all these unneeded submesh checks for those
            if (renderersCount + collidersCount <= 1) return entity.isInsideSceneBoundaries;

            if (renderers != null)
            {
                for (int i = 0; i < renderersCount; i++)
                {
                    if (!scene.IsInsideSceneBoundaries(renderers[i].bounds))
                        return false;
                }
            }

            if (physicsColliders != null)
            {
                int physicsCollidersCount = physicsColliders.Count;

                for (int i = 0; i < physicsCollidersCount; i++)
                {
                    if (!scene.IsInsideSceneBoundaries(physicsColliders[i].bounds))
                        return false;
                }
            }

            if (pointerColliders != null)
            {
                int pointerCollidersCount = pointerColliders.Count;

                for (int i = 0; i < pointerCollidersCount; i++)
                {
                    if (!scene.IsInsideSceneBoundaries(pointerColliders[i].bounds))
                        return false;
                }
            }

            return true;
        }

        private static bool WereColliderComponentRemoved(IParcelScene scene, IDCLEntity entity,
            IList<Collider> sbcComponentColliders, IInternalECSComponent<InternalColliders> colliderComponent)
        {
            return sbcComponentColliders != null && colliderComponent.GetFor(scene, entity) == null;
        }

        private static bool WereRendererComponentRemoved(IParcelScene scene, IDCLEntity entity,
            IList<Renderer> sbcComponentRenderers, IInternalECSComponent<InternalRenderers> rendererComponent)
        {
            return sbcComponentRenderers != null && rendererComponent.GetFor(scene, entity) == null;
        }

        private static void SetInsideBoundsStateForMeshComponents(IECSOutOfSceneBoundsFeedbackStyle outOfBoundsVisualFeedback, IDCLEntity entity,
            InternalSceneBoundsCheck sbcComponentModel, bool isVisible, bool isInsideBounds)
        {
            outOfBoundsVisualFeedback.ApplyFeedback(entity, sbcComponentModel, isVisible, isInsideBounds);
        }

        private static void SetInsideBoundsStateForNonMeshComponents(IDCLEntity entity, InternalSceneBoundsCheck componentModel)
        {
            if (componentModel.audioSource != null)
                componentModel.audioSource.enabled = entity.isInsideSceneBoundaries;
        }

        private static void ProcessRendererComponentChanges(IInternalECSComponent<InternalSceneBoundsCheck> sceneBoundsCheckComponent,
            IReadOnlyList<KeyValueSetTriplet<IParcelScene, long, ECSComponentData<InternalRenderers>>> rendererComponents)
        {
            for (int i = 0; i < rendererComponents.Count; i++)
            {
                var componentData = rendererComponents[i].value;
                IParcelScene scene = componentData.scene;
                IDCLEntity entity = componentData.entity;
                InternalRenderers model = componentData.model;

                if (!model.dirty || scene.isPersistent) continue;

                sceneBoundsCheckComponent.SetRenderers(scene, entity, model.renderers);
            }
        }

        private static void ProcessPointerColliderComponentChanges(IInternalECSComponent<InternalSceneBoundsCheck> sceneBoundsCheckComponent,
            IReadOnlyList<KeyValueSetTriplet<IParcelScene, long, ECSComponentData<InternalColliders>>> colliderComponents)
        {
            for (int i = 0; i < colliderComponents.Count; i++)
            {
                var componentData = colliderComponents[i].value;
                IParcelScene scene = componentData.scene;
                IDCLEntity entity = componentData.entity;
                InternalColliders model = componentData.model;

                if (!model.dirty || scene.isPersistent) continue;

                sceneBoundsCheckComponent.SetPointerColliders(scene, entity, model.colliders);
            }
        }

        private static void ProcessPhysicColliderComponentChanges(IInternalECSComponent<InternalSceneBoundsCheck> sceneBoundsCheckComponent,
            IReadOnlyList<KeyValueSetTriplet<IParcelScene, long, ECSComponentData<InternalColliders>>> colliderComponents)
        {
            for (int i = 0; i < colliderComponents.Count; i++)
            {
                var componentData = colliderComponents[i].value;
                IParcelScene scene = componentData.scene;
                IDCLEntity entity = componentData.entity;
                InternalColliders model = componentData.model;

                if (!model.dirty || scene.isPersistent) continue;

                sceneBoundsCheckComponent.SetPhysicsColliders(scene, entity, model.colliders);
            }
        }

        private static void ProcessAudioSourceComponentChanges(IInternalECSComponent<InternalSceneBoundsCheck> sceneBoundsCheckComponent,
            IReadOnlyList<KeyValueSetTriplet<IParcelScene, long, ECSComponentData<InternalAudioSource>>> audioSourceComponents)
        {
            for (int i = 0; i < audioSourceComponents.Count; i++)
            {
                var componentData = audioSourceComponents[i].value;
                IParcelScene scene = componentData.scene;
                IDCLEntity entity = componentData.entity;
                InternalAudioSource model = componentData.model;

                if (!model.dirty || scene.isPersistent) continue;

                sceneBoundsCheckComponent.SetAudioSource(scene, entity, model.audioSource);
            }
        }

        private static bool IsEntityVisible(IParcelScene scene, IDCLEntity entity, IInternalECSComponent<InternalVisibility> visibilityComponent)
        {
            return visibilityComponent.GetFor(scene, entity)?.model.visible ?? true;
        }
    }
}
