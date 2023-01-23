using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
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
        private readonly IECSOutOfSceneBoundsFeedbackStyle outOfBoundsVisualFeedback;

        public ECSSceneBoundsCheckerSystem(IInternalECSComponent<InternalSceneBoundsCheck> sbcComponent,
            IInternalECSComponent<InternalVisibility> visibilityComponent,
            IInternalECSComponent<InternalRenderers> renderersComponent,
            IInternalECSComponent<InternalColliders> pointerColliderComponent,
            IInternalECSComponent<InternalColliders> physicsColliderComponent,
            bool previewMode = false)
        {
            this.sceneBoundsCheckComponent = sbcComponent;
            this.visibilityComponent = visibilityComponent;
            this.renderersComponent = renderersComponent;
            this.pointerCollidersComponent = pointerColliderComponent;
            this.physicsCollidersComponent = physicsColliderComponent;

            outOfBoundsVisualFeedback = previewMode ? new ECSOutOfSceneBoundsFeedback_RedWireframe() : new ECSOutOfSceneBoundsFeedback_EnabledToggle();
        }

        public void Update()
        {
            // Update renderers
            var rendererComponents = renderersComponent.GetForAll();
            for (int i = 0; i < rendererComponents.Count; i++)
            {
                var componentData = rendererComponents[i].value;

                if(!componentData.model.dirty || componentData.scene.isPersistent) continue;

                sceneBoundsCheckComponent.SetRenderers(componentData.scene, componentData.entity, componentData.model.renderers);
            }

            // Update physics colliders
            var physicsColliderComponents = physicsCollidersComponent.GetForAll();
            for (int i = 0; i < physicsColliderComponents.Count; i++)
            {
                var componentData = physicsColliderComponents[i].value;

                if(!componentData.model.dirty || componentData.scene.isPersistent) continue;

                sceneBoundsCheckComponent.SetPhysicsColliders(componentData.scene, componentData.entity, componentData.model.colliders);
            }

            // Update pointer colliders
            var pointerColliderComponents = pointerCollidersComponent.GetForAll();
            for (int i = 0; i < pointerColliderComponents.Count; i++)
            {
                var componentData = pointerColliderComponents[i].value;

                if(!componentData.model.dirty || componentData.scene.isPersistent) continue;

                sceneBoundsCheckComponent.SetPointerColliders(componentData.scene, componentData.entity, componentData.model.colliders);
            }

            // Note: the components are traversed backwards as we may free the 'fully defaulted' entities from the component
            var sbcComponentGroup = sceneBoundsCheckComponent.GetForAll();
            for (int i = sbcComponentGroup.Count-1; i >= 0 ; i--)
            {
                var componentData = sbcComponentGroup[i].value;

                if(!componentData.model.dirty || componentData.scene.isPersistent) continue;

                componentData.model.meshesDirty |= AnyRendererOrColliderComponentRemoved(componentData);
                if (componentData.model.meshesDirty)
                {
                    sceneBoundsCheckComponent.RecalculateEntityMeshBounds(componentData.scene, componentData.entity);

                    // If all meshes were removed we need to reset the feedback.
                    if (componentData.model.entityLocalMeshBounds.size == Vector3.zero)
                        SetInsideBoundsStateForMeshComponents(componentData, true);
                }

                if (sceneBoundsCheckComponent.IsFullyDefaulted(componentData.scene, componentData.entity))
                {
                    // Since no other component is using the internal SBC component, we remove it.
                    sceneBoundsCheckComponent.RemoveFor(componentData.scene, componentData.entity);
                    continue;
                }

                RunEntityEvaluation(componentData);

                // This reset of the meshesDirty will trigger 1 extra check due to the sbcComponent dirty flag being affected
                componentData.model.meshesDirty = false;
                sceneBoundsCheckComponent.PutFor(componentData.scene, componentData.entity, componentData.model);
            }
        }

        private bool AnyRendererOrColliderComponentRemoved(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData)
        {
            bool returnValue = false;
            if (sbcComponentData.model.renderers != null && renderersComponent.GetFor(sbcComponentData.scene, sbcComponentData.entity) == null)
            {
                sbcComponentData.model.renderers = null;
                returnValue = true;
            }

            if (sbcComponentData.model.physicsColliders != null && physicsCollidersComponent.GetFor(sbcComponentData.scene, sbcComponentData.entity) == null)
            {
                sbcComponentData.model.physicsColliders = null;
                returnValue = true;
            }

            if (sbcComponentData.model.pointerColliders != null && pointerCollidersComponent.GetFor(sbcComponentData.scene, sbcComponentData.entity) == null)
            {
                sbcComponentData.model.pointerColliders = null;
                returnValue = true;
            }

            return returnValue;
        }
        private void RunEntityEvaluation(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData)
        {
            // If it has a mesh we don't evaluate its position due to artists common "pivot point sloppiness", we evaluate its mesh merged bounds
            if (sbcComponentData.model.entityLocalMeshBounds.size != Vector3.zero) // has a mesh/collider
                EvaluateMeshBounds(sbcComponentData);
            else
                EvaluateEntityPosition(sbcComponentData);

            SetInsideBoundsStateForNonMeshComponents(sbcComponentData.entity);
        }

        private void EvaluateEntityPosition(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData)
        {
            // 1. Cheap outer-bounds check
            sbcComponentData.entity.isInsideSceneOuterBoundaries = sbcComponentData.scene.IsInsideSceneOuterBoundaries(sbcComponentData.model.entityPosition);

            // 2. Confirm with inner-bounds check only if entity is inside outer bounds
            Vector3 entityWorldPosition = sbcComponentData.model.entityPosition + CommonScriptableObjects.worldOffset.Get();
            sbcComponentData.entity.isInsideSceneBoundaries = sbcComponentData.entity.isInsideSceneOuterBoundaries
                                                              && sbcComponentData.scene.IsInsideSceneBoundaries(entityWorldPosition, entityWorldPosition.y);
        }

        private void EvaluateMeshBounds(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData)
        {
            Vector3 worldOffset = CommonScriptableObjects.worldOffset.Get();
            Vector3 entityGlobalPosition = sbcComponentData.model.entityPosition;
            Vector3 globalBoundsMaxPoint = entityGlobalPosition + sbcComponentData.model.entityLocalMeshBounds.max;
            Vector3 globalBoundsMinPoint = entityGlobalPosition + sbcComponentData.model.entityLocalMeshBounds.min;

            // 1. Cheap outer-bounds check
            sbcComponentData.entity.isInsideSceneOuterBoundaries = sbcComponentData.scene.IsInsideSceneOuterBoundaries(globalBoundsMaxPoint)
                                                                   && sbcComponentData.scene.IsInsideSceneOuterBoundaries(globalBoundsMinPoint);

            if (sbcComponentData.entity.isInsideSceneOuterBoundaries)
            {
                // 2. If entity is inside outer bounds then check full merged bounds AABB
                sbcComponentData.entity.isInsideSceneBoundaries = sbcComponentData.scene.IsInsideSceneBoundaries(globalBoundsMaxPoint + worldOffset, globalBoundsMaxPoint.y)
                                                                  && sbcComponentData.scene.IsInsideSceneBoundaries(globalBoundsMinPoint + worldOffset);

                // 3. If merged bounds is detected as outside bounds we need a final check on submeshes (for L-Shaped subdivided meshes)
                if (!sbcComponentData.entity.isInsideSceneBoundaries)
                    sbcComponentData.entity.isInsideSceneBoundaries = AreSubMeshesAndCollidersInsideBounds(sbcComponentData);
            }
            else
            {
                sbcComponentData.entity.isInsideSceneBoundaries = false;
            }

            SetInsideBoundsStateForMeshComponents(sbcComponentData);
        }

        private bool AreSubMeshesAndCollidersInsideBounds(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData)
        {
            var renderers = sbcComponentData.model.renderers;
            var physicsColliders = sbcComponentData.model.physicsColliders;
            var pointerColliders = sbcComponentData.model.pointerColliders;
            int renderersCount = renderers?.Count ?? 0;
            int collidersCount = physicsColliders?.Count ?? 0 + pointerColliders?.Count ?? 0;

            // For entities with 1 mesh/collider the already-checked merged bounds already represent its bounds
            // So we avoid all these unneeded submesh checks for those
            if (renderersCount + collidersCount <= 1) return sbcComponentData.entity.isInsideSceneBoundaries;

            if (renderers != null)
            {
                for (int i = 0; i < renderersCount; i++)
                {
                    if (!sbcComponentData.scene.IsInsideSceneBoundaries(renderers[i].bounds))
                        return false;
                }
            }

            if (physicsColliders != null)
            {
                int physicsCollidersCount = physicsColliders.Count;
                for (int i = 0; i < physicsCollidersCount; i++)
                {
                    if (!sbcComponentData.scene.IsInsideSceneBoundaries(physicsColliders[i].bounds))
                        return false;
                }
            }

            if (pointerColliders != null)
            {
                int pointerCollidersCount = pointerColliders.Count;
                for (int i = 0; i < pointerCollidersCount; i++)
                {
                    if (!sbcComponentData.scene.IsInsideSceneBoundaries(pointerColliders[i].bounds))
                        return false;
                }
            }

            return true;
        }

        private void SetInsideBoundsStateForMeshComponents(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData)
        {
            SetInsideBoundsStateForMeshComponents(sbcComponentData, sbcComponentData.entity.isInsideSceneBoundaries);
        }

        private void SetInsideBoundsStateForMeshComponents(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData, bool isInsideBounds)
        {
            outOfBoundsVisualFeedback.ApplyFeedback(sbcComponentData, visibilityComponent.GetFor(sbcComponentData.scene, sbcComponentData.entity), isInsideBounds);
        }

        private void SetInsideBoundsStateForNonMeshComponents(IDCLEntity entity)
        {
        }
    }
}
