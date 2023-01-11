using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;

namespace ECSSystems.ECSSceneBoundsCheckerSystem
{
    public class ECSSceneBoundsCheckerSystem
    {
        private IInternalECSComponent<InternalSceneBoundsCheck> internalSceneBoundsCheckComponent;
        private IInternalECSComponent<InternalRenderers> internalRenderersComponent;
        private IInternalECSComponent<InternalColliders> pointerInternalCollidersComponent;
        private IInternalECSComponent<InternalColliders> physicsInternalCollidersComponent;

        public ECSSceneBoundsCheckerSystem(IInternalECSComponent<InternalSceneBoundsCheck> sbcComponent,
            IInternalECSComponent<InternalRenderers> renderersComponent,
            IInternalECSComponent<InternalColliders> pointerColliderComponent,
            IInternalECSComponent<InternalColliders> physicsColliderComponent)
        {
            this.internalSceneBoundsCheckComponent = sbcComponent;
            this.internalRenderersComponent = renderersComponent;
            this.pointerInternalCollidersComponent = pointerColliderComponent;
            this.physicsInternalCollidersComponent = physicsColliderComponent;
        }

        public void Update()
        {
            // TODO: Should we update merged bounds when the transform/parenting is changed as well? should we avoid using Bounds class?
            // TODO: Deal with "safe" merged bounds...
            // TODO: merged bounds should adjust to MeshCollider or MeshRenderer being removed while the other still exists for the entity...
            // TODO: When will merged bounds be cleaned up ???

            // RENDERERS
            var rendererComponents = internalRenderersComponent.GetForAll();
            for (int i = 0; i < rendererComponents.Count; i++)
            {
                var componentData = rendererComponents[i].value;

                if(!componentData.model.dirty) continue;

                internalSceneBoundsCheckComponent.SetRenderers(componentData.scene, componentData.entity, componentData.model.renderers);
            }

            // PHYSICS COLLIDERS
            var physicsColliderComponents = physicsInternalCollidersComponent.GetForAll();
            for (int i = 0; i < physicsColliderComponents.Count; i++)
            {
                var componentData = physicsColliderComponents[i].value;

                if(!componentData.model.dirty) continue;

                // TODO: Should overwrite colliders or not??? when is that collection cleared?
                internalSceneBoundsCheckComponent.SetPhysicsColliders(componentData.scene, componentData.entity, componentData.model.colliders);
            }

            // POINTER COLLIDERS
            var pointerColliderComponents = pointerInternalCollidersComponent.GetForAll();
            for (int i = 0; i < pointerColliderComponents.Count; i++)
            {
                var componentData = pointerColliderComponents[i].value;

                if(!componentData.model.dirty) continue;

                // TODO: Should overwrite colliders or not??? when is that collection cleared?
                internalSceneBoundsCheckComponent.SetPointerColliders(componentData.scene, componentData.entity, componentData.model.colliders);
            }

            // TODO: Traverse inversed to check if model is totally default and remove component in that case...
            var sbcComponents = internalSceneBoundsCheckComponent.GetForAll();
            for (int i = 0; i < sbcComponents.Count; i++)
            {
                var componentData = sbcComponents[i].value;

                // TODO: Avoid recalculating when not needed... (based on dirty state of the other 3 internal components ???)
                internalSceneBoundsCheckComponent.RecalculateEntityMeshBounds(componentData.scene, componentData.entity);

                if(!componentData.model.dirty) continue;

                // Debug.Log($"entity pos: {model.entityPosition}, dirty? {model.dirty}");
                //TODO: add cpu time budget managing here

                RunEntityEvaluation(componentData);
            }
        }

        // TODO: Add 'outer bounds check' optimization
        private void RunEntityEvaluation(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData)
        {
            IDCLEntity entity = sbcComponentData.entity;
            if (entity == null || entity.gameObject == null || entity.scene == null || entity.scene.isPersistent)
                return;

            // TODO: To solve recursive entity evaluation of children: every time a transform is affected (parenting, position), the entity children should have its sbc component entity position updated.

            // If it has a mesh we don't evaluate its position due to artists "pivot point sloppiness", we evaluate its mesh merged bounds
            if (sbcComponentData.model.entityMeshBounds.size.sqrMagnitude > 0) // has a mesh/collider
            {
                // EvaluateMeshBounds();
            }
            else
            {
                EvaluateEntityPosition(sbcComponentData);
            }
            // TODO: case for AvatarShape evaluation...
        }

        private void EvaluateEntityPosition(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData)
        {
            bool isInsideBounds = sbcComponentData.scene.IsInsideSceneBoundaries(sbcComponentData.model.entityPosition + CommonScriptableObjects.worldOffset.Get());
            SetInsideBoundsStateForNonMeshComponents(sbcComponentData.entity, isInsideBounds);
            SetInsideBoundsStateForEntity(sbcComponentData.entity, isInsideBounds);
        }

        private void SetInsideBoundsStateForNonMeshComponents(IDCLEntity entity, bool isInsideBounds)
        {
            // if(entity.isInsideSceneBoundaries == isInsideBounds || !DataStore.i.sceneBoundariesChecker.componentsCheckSceneBoundaries.ContainsKey(entity.entityId))
            if(entity.isInsideSceneBoundaries == isInsideBounds)
                return;

            // foreach (IOutOfSceneBoundariesHandler component in DataStore.i.sceneBoundariesChecker.componentsCheckSceneBoundaries[entity.entityId])
            // {
            //     component.UpdateOutOfBoundariesState(isInsideBounds);
            // }

            //...
        }

        private void SetInsideBoundsStateForEntity(IDCLEntity entity, bool isInsideBounds)
        {
            if (entity.isInsideSceneBoundaries == isInsideBounds)
                return;

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
    }
}
