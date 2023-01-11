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
                var rendererModel = rendererComponents[i].value.model;
                if(!rendererModel.dirty) continue;

                var componentData = rendererComponents[i].value;
                var sbcModel = internalSceneBoundsCheckComponent.GetFor(componentData.scene, componentData.entity)?.model ?? new InternalSceneBoundsCheck();
                sbcModel.renderers = rendererModel.renderers;

                // CREATE/UPDATE MERGED BOUNDS
                for (var j = 0; j < sbcModel.renderers.Count; j++)
                {
                    sbcModel.entityMeshBounds.Encapsulate(sbcModel.renderers[j].bounds);
                }

                internalSceneBoundsCheckComponent.PutFor(componentData.scene, componentData.entity, sbcModel);
            }

            // POINTER COLLIDERS
            var pointerColliderComponents = pointerInternalCollidersComponent.GetForAll();
            for (int i = 0; i < pointerColliderComponents.Count; i++)
            {
                var colliderModel = pointerColliderComponents[i].value.model;
                if(!colliderModel.dirty) continue;

                var componentData = pointerColliderComponents[i].value;
                var sbcModel = internalSceneBoundsCheckComponent.GetFor(componentData.scene, componentData.entity)?.model ?? new InternalSceneBoundsCheck();
                sbcModel.colliders = colliderModel.colliders;

                // CREATE/UPDATE MERGED BOUNDS
                for (var j = 0; j < sbcModel.colliders.Count; j++)
                {
                    sbcModel.entityMeshBounds.Encapsulate(sbcModel.colliders[j].bounds);
                }

                internalSceneBoundsCheckComponent.PutFor(componentData.scene, componentData.entity, sbcModel);
            }

            // PHYSICS COLLIDERS
            var physicsColliderComponents = physicsInternalCollidersComponent.GetForAll();
            for (int i = 0; i < physicsColliderComponents.Count; i++)
            {
                var colliderModel = physicsColliderComponents[i].value.model;
                if(!colliderModel.dirty) continue;

                var componentData = physicsColliderComponents[i].value;
                var sbcModel = internalSceneBoundsCheckComponent.GetFor(componentData.scene, componentData.entity)?.model ?? new InternalSceneBoundsCheck();
                sbcModel.colliders = colliderModel.colliders; // add collider instead of overwrite ???

                // CREATE/UPDATE MERGED BOUNDS
                for (var j = 0; j < sbcModel.colliders.Count; j++)
                {
                    sbcModel.entityMeshBounds.Encapsulate(sbcModel.colliders[j].bounds);
                }

                internalSceneBoundsCheckComponent.PutFor(componentData.scene, componentData.entity, sbcModel);
            }

            // TODO: Traverse inversed to check if model is totally default and remove component in that case...
            var sbcComponents = internalSceneBoundsCheckComponent.GetForAll();
            for (int i = 0; i < sbcComponents.Count; i++)
            {
                if(!sbcComponents[i].value.model.dirty) continue;

                // Debug.Log($"entity pos: {model.entityPosition}, dirty? {model.dirty}");
                //TODO: add cpu time budget managing here

                RunEntityEvaluation(sbcComponents[i].value);
            }
        }

        // TODO: Add 'outer bounds check' optimization
        private void RunEntityEvaluation(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData)
        {
            IDCLEntity entity = sbcComponentData.entity;
            if (entity == null || entity.gameObject == null || entity.scene == null || entity.scene.isPersistent)
                return;

            // TODO: Solve recursive children checking... if an entity is checked, their children should be checked as well regardless of their own component...
            // Recursively evaluate entity children as well, we need to check this up front because this entity may not have meshes of its own, but the children may.
            // if (entity.children.Count > 0)
            // {
            //     using (var iterator = entity.children.GetEnumerator())
            //     {
            //         while (iterator.MoveNext())
            //         {
            //             RunEntityEvaluation(iterator.Current.Value, onlyOuterBoundsCheck);
            //         }
            //     }
            // }


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
