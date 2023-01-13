using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace ECSSystems.ECSSceneBoundsCheckerSystem
{
    public class ECSSceneBoundsCheckerSystem
    {
        private IInternalECSComponent<InternalSceneBoundsCheck> sceneBoundsCheckComponent;
        private IInternalECSComponent<InternalRenderers> renderersComponent;
        private IInternalECSComponent<InternalColliders> pointerCollidersComponent;
        private IInternalECSComponent<InternalColliders> physicsCollidersComponent;

        public ECSSceneBoundsCheckerSystem(IInternalECSComponent<InternalSceneBoundsCheck> sbcComponent,
            IInternalECSComponent<InternalRenderers> renderersComponent,
            IInternalECSComponent<InternalColliders> pointerColliderComponent,
            IInternalECSComponent<InternalColliders> physicsColliderComponent)
        {
            this.sceneBoundsCheckComponent = sbcComponent;
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

                // TODO: Avoid recalculating when not needed... (based on dirty state of the other 3 internal components ???)
                // TODO: Should we update merged bounds when the transform/parenting is changed as well? should we avoid using Bounds class?
                // TODO: Deal with "safe" merged bounds...
                // TODO: merged bounds should adjust to MeshCollider or MeshRenderer being removed while the other still exists for the entity...
                // TODO: When will merged bounds be cleaned up ???
                sceneBoundsCheckComponent.RecalculateEntityMeshBounds(componentData.scene, componentData.entity);

                //TODO: add cpu time budget managing here

                RunEntityEvaluation(componentData);
            }
        }

        // TODO: Add 'outer bounds check' optimization
        private void RunEntityEvaluation(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData)
        {
            // If it has a mesh we don't evaluate its position due to artists common "pivot point sloppiness", we evaluate its mesh merged bounds
            // TODO: Is it OK to rely on the bounds size for this check? is the bounds object being updated/cleaned correctly?
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

            bool isInsideBounds = sbcComponentData.scene.IsInsideSceneBoundaries(globalBoundsMaxPoint + worldOffset)
                                  && sbcComponentData.scene.IsInsideSceneBoundaries(globalBoundsMinPoint + worldOffset);

            SetInsideBoundsStateForEntity(sbcComponentData.entity, isInsideBounds);
            SetInsideBoundsStateForMeshComponents(sbcComponentData, isInsideBounds);
        }

        private void EvaluateEntityPosition(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData)
        {
            bool isInsideBounds = sbcComponentData.scene.IsInsideSceneBoundaries(sbcComponentData.model.entityPosition + CommonScriptableObjects.worldOffset.Get());
            SetInsideBoundsStateForEntity(sbcComponentData.entity, isInsideBounds);
            SetInsideBoundsStateForNonMeshComponents(sbcComponentData.entity, isInsideBounds);
        }

        private void SetInsideBoundsStateForEntity(IDCLEntity entity, bool isInsideBounds)
        {
            if (entity.isInsideSceneBoundaries == isInsideBounds)
                return;

            entity.isInsideSceneOuterBoundaries = isInsideBounds; // TODO: correct
            entity.isInsideSceneBoundaries = isInsideBounds;

            // for debugging
            if (isInsideBounds)
            {
                entity.gameObject.name = entity.gameObject.name.Replace("/", "");
                entity.gameObject.name += "+";
            }
            else
            {
                entity.gameObject.name = entity.gameObject.name.Replace("+", "");
                entity.gameObject.name += "/";
            }

            // OnEntityBoundsCheckerStatusChanged?.Invoke(entity, isInsideBounds);
        }

        private void SetInsideBoundsStateForMeshComponents(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData, bool isInsideBounds)
        {
            if (sbcComponentData.model.renderers != null)
            {
                for (var i = 0; i < sbcComponentData.model.renderers.Count; i++)
                {
                    sbcComponentData.model.renderers[i].enabled = isInsideBounds;
                }
            }

            if (sbcComponentData.model.physicsColliders != null)
            {
                for (var i = 0; i < sbcComponentData.model.physicsColliders.Count; i++)
                {
                    sbcComponentData.model.physicsColliders[i].enabled = isInsideBounds;
                }
            }

            if (sbcComponentData.model.pointerColliders != null)
            {
                for (var i = 0; i < sbcComponentData.model.pointerColliders.Count; i++)
                {
                    sbcComponentData.model.pointerColliders[i].enabled = isInsideBounds;
                }
            }
        }

        private void SetInsideBoundsStateForNonMeshComponents(IDCLEntity entity, bool isInsideBounds)
        {

            // foreach (IOutOfSceneBoundariesHandler component in DataStore.i.sceneBoundariesChecker.componentsCheckSceneBoundaries[entity.entityId])
            // {
            //     component.UpdateOutOfBoundariesState(isInsideBounds);
            // }

            //...
        }
    }
}
