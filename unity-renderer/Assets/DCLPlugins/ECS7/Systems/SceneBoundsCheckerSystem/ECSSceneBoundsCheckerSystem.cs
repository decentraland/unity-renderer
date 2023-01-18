
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace ECSSystems.ECSSceneBoundsCheckerSystem
{
    public class ECSSceneBoundsCheckerSystem
    {
        private IInternalECSComponent<InternalSceneBoundsCheck> sceneBoundsCheckComponent;
        private IInternalECSComponent<InternalVisibility> visibilityComponent;
        private IInternalECSComponent<InternalRenderers> renderersComponent;
        private IInternalECSComponent<InternalColliders> pointerCollidersComponent;
        private IInternalECSComponent<InternalColliders> physicsCollidersComponent;

        // TODO: Toggle which feedback style is instantiated based on being in debug/preview mode or not
        // private IECSOutOfSceneBoundsFeedbackStyle outOfBoundsVisualFeedback = new ECSOutOfSceneBoundsFeedback_EnabledToggle();
        private IECSOutOfSceneBoundsFeedbackStyle outOfBoundsVisualFeedback = new ECSOutOfSceneBoundsFeedback_RedWireframe();

        public ECSSceneBoundsCheckerSystem(IInternalECSComponent<InternalSceneBoundsCheck> sbcComponent,
            IInternalECSComponent<InternalVisibility> visibilityComponent,
            IInternalECSComponent<InternalRenderers> renderersComponent,
            IInternalECSComponent<InternalColliders> pointerColliderComponent,
            IInternalECSComponent<InternalColliders> physicsColliderComponent)
        {
            this.sceneBoundsCheckComponent = sbcComponent;
            this.visibilityComponent = visibilityComponent;
            this.renderersComponent = renderersComponent;
            this.pointerCollidersComponent = pointerColliderComponent;
            this.physicsCollidersComponent = physicsColliderComponent;
        }

        public void Update()
        {
            // Update renderers
            var rendererComponents = renderersComponent.GetForAll();
            for (int i = 0; i < rendererComponents.Count; i++)
            {
                var componentData = rendererComponents[i].value;

                if(!componentData.model.dirty) continue;

                sceneBoundsCheckComponent.SetRenderers(componentData.scene, componentData.entity, componentData.model.renderers);
            }

            // Update physics colliders
            var physicsColliderComponents = physicsCollidersComponent.GetForAll();
            for (int i = 0; i < physicsColliderComponents.Count; i++)
            {
                var componentData = physicsColliderComponents[i].value;

                if(!componentData.model.dirty) continue;

                sceneBoundsCheckComponent.SetPhysicsColliders(componentData.scene, componentData.entity, componentData.model.colliders);
            }

            // Update pointer colliders
            var pointerColliderComponents = pointerCollidersComponent.GetForAll();
            for (int i = 0; i < pointerColliderComponents.Count; i++)
            {
                var componentData = pointerColliderComponents[i].value;

                if(!componentData.model.dirty) continue;

                sceneBoundsCheckComponent.SetPointerColliders(componentData.scene, componentData.entity, componentData.model.colliders);
            }

            var sbcComponents = sceneBoundsCheckComponent.GetForAll();
            for (int i = sbcComponents.Count-1; i >= 0 ; i--)
            {
                var componentData = sbcComponents[i].value;

                if(!componentData.model.dirty) continue;

                if (sceneBoundsCheckComponent.IsFullyDefaulted(componentData.scene, componentData.entity))
                {
                    // Since no other component is using the internal SBC component, we remove it.
                    sceneBoundsCheckComponent.RemoveFor(componentData.scene, componentData.entity);
                    continue;
                }

                if (componentData.model.meshesDirty)
                {
                    // TODO: Deal with "safe" merged bounds...
                    // TODO: When will merged bounds be cleaned up? If entity did have meshes and now it doesn't, does the merged bounds get re-set/cleaned?
                    sceneBoundsCheckComponent.RecalculateEntityMeshBounds(componentData.scene, componentData.entity);
                }

                //TODO: add cpu time budget managing here
                //TODO: deal with height checks ???

                RunEntityEvaluation(componentData);

                // This reset of the meshesDirty will trigger 1 extra check due to the sbcComponent dirty flag
                componentData.model.meshesDirty = false;
                sceneBoundsCheckComponent.PutFor(componentData.scene, componentData.entity, componentData.model);
            }
        }

        // TODO: Add 'outer bounds check' optimization
        private void RunEntityEvaluation(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData)
        {
            // If it has a mesh we don't evaluate its position due to artists common "pivot point sloppiness", we evaluate its mesh merged bounds
            if (sbcComponentData.model.entityLocalMeshBounds.size != Vector3.zero) // has a mesh/collider
            {
                EvaluateMeshBounds(sbcComponentData);
            }
            else
            {
                EvaluateEntityPosition(sbcComponentData);
            }
            // TODO: case for AvatarShape evaluation...
        }

        private void EvaluateMeshBounds(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData)
        {
            Vector3 worldOffset = CommonScriptableObjects.worldOffset.Get();
            Vector3 entityGlobalPosition = sbcComponentData.model.entityPosition;
            Vector3 globalBoundsMaxPoint = entityGlobalPosition + sbcComponentData.model.entityLocalMeshBounds.max;
            Vector3 globalBoundsMinPoint = entityGlobalPosition + sbcComponentData.model.entityLocalMeshBounds.min;

            // 1. cheap outer-bounds check
            sbcComponentData.entity.isInsideSceneOuterBoundaries = sbcComponentData.scene.IsInsideSceneOuterBoundaries(globalBoundsMaxPoint)
                                                                   && sbcComponentData.scene.IsInsideSceneOuterBoundaries(globalBoundsMinPoint);

            // 2. confirm with inner-bounds check only if entity is inside outer bounds
            sbcComponentData.entity.isInsideSceneBoundaries = sbcComponentData.entity.isInsideSceneOuterBoundaries
                                                              && sbcComponentData.scene.IsInsideSceneBoundaries(globalBoundsMaxPoint + worldOffset)
                                                              && sbcComponentData.scene.IsInsideSceneBoundaries(globalBoundsMinPoint + worldOffset);
            SetInsideBoundsStateForMeshComponents(sbcComponentData);
        }

        private void EvaluateEntityPosition(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData)
        {
            // 1. cheap outer-bounds check
            sbcComponentData.entity.isInsideSceneOuterBoundaries = sbcComponentData.scene.IsInsideSceneOuterBoundaries(sbcComponentData.model.entityPosition);

            // 2. confirm with inner-bounds check only if entity is inside outer bounds
            sbcComponentData.entity.isInsideSceneBoundaries = sbcComponentData.entity.isInsideSceneOuterBoundaries
                                                              && sbcComponentData.scene.IsInsideSceneBoundaries(sbcComponentData.model.entityPosition + CommonScriptableObjects.worldOffset.Get());
            SetInsideBoundsStateForNonMeshComponents(sbcComponentData.entity);
        }

        private void SetInsideBoundsStateForMeshComponents(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData)
        {
            outOfBoundsVisualFeedback.ApplyFeedback(sbcComponentData, visibilityComponent.GetFor(sbcComponentData.scene, sbcComponentData.entity), sbcComponentData.entity.isInsideSceneBoundaries);
        }

        private void SetInsideBoundsStateForNonMeshComponents(IDCLEntity entity)
        {
        }
    }
}
