using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSTransformHandler : IECSComponentHandler<ECSTransform>
    {
        private readonly IWorldState worldState;
        private readonly IBaseVariable<Vector3> playerTeleportVariable;

        private readonly IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent;

        public ECSTransformHandler(IWorldState worldState, IBaseVariable<Vector3> playerTeleportVariable, IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent)
        {
            ECSTransformUtils.orphanEntities = new KeyValueSet<IDCLEntity, ECSTransformUtils.OrphanEntity>(100);
            this.sbcInternalComponent = sbcInternalComponent;

            this.worldState = worldState;
            this.playerTeleportVariable = playerTeleportVariable;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            ECSTransformUtils.orphanEntities.Remove(entity);

            // reset transform and re-parent to the scene
            entity.gameObject.transform.ResetLocalTRS();
            ECSTransformUtils.TrySetParent(scene, entity, SpecialEntityId.SCENE_ROOT_ENTITY);

            // if entity has any parent
            if (entity.parentId != SpecialEntityId.SCENE_ROOT_ENTITY)
            {
                // remove as child
                if (scene.entities.TryGetValue(entity.parentId, out IDCLEntity parent))
                {
                    parent.childrenId.Remove(entity.entityId);
                }

                entity.parentId = SpecialEntityId.SCENE_ROOT_ENTITY;
            }

            // if entity has any children
            int childrenCount = entity.childrenId.Count;

            if (childrenCount > 0)
            {
                for (int i = childrenCount - 1; i >= 0; i--)
                {
                    long childId = entity.childrenId[i];

                    if (!scene.entities.TryGetValue(childId, out IDCLEntity child))
                        continue;

                    // re-parent child to the scene
                    ECSTransformUtils.TrySetParent(scene, child, SpecialEntityId.SCENE_ROOT_ENTITY);

                    // add child as orphan
                    ECSTransformUtils.orphanEntities[child] = new ECSTransformUtils.OrphanEntity(scene, child, child.parentId);
                }

                entity.childrenId.Clear();
            }

            // Reset value in SBC internal component
            sbcInternalComponent.SetPosition(scene, entity, Vector3.zero, false);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, ECSTransform model)
        {
            // From SDK `PLAYER_ENTITY` entity's transform can be modified to
            // move character across the scene
            if (entity.entityId == SpecialEntityId.PLAYER_ENTITY)
            {
                TryMoveCharacter(scene, model.position, worldState, playerTeleportVariable);
                return;
            }

            Transform transform = entity.gameObject.transform;
            bool positionChange = transform.localPosition != model.position;
            bool scaleChange = transform.localScale != model.scale;
            bool rotationChange = transform.localRotation != model.rotation;

            transform.localPosition = model.position;
            transform.localRotation = model.rotation;
            transform.localScale = model.scale;

            if (entity.parentId != model.parentId)
                ProcessNewParent(scene, entity, model.parentId);

            if(positionChange)
                sbcInternalComponent.SetPosition(scene, entity, transform.position);

            if(scaleChange || rotationChange)
                sbcInternalComponent.OnTransformScaleRotationChanged(scene, entity);
        }

        private static void ProcessNewParent(IParcelScene scene, IDCLEntity entity, long parentId)
        {
            //check for cyclic parenting
            if (ECSTransformUtils.IsCircularParenting(scene, entity, parentId))
            {
                Debug.LogError($"cyclic parenting found for entity {entity.entityId} " +
                               $"parenting to {parentId} at scene {scene.sceneData.sceneNumber} ({scene.sceneData.basePosition})");

                return;
            }

            // remove as child of previous parent
            if (entity.parentId != SpecialEntityId.SCENE_ROOT_ENTITY)
            {
                if (scene.entities.TryGetValue(entity.parentId, out IDCLEntity parent))
                {
                    parent.childrenId.Remove(entity.entityId);
                }

                ECSTransformUtils.TrySetParent(scene, entity, SpecialEntityId.SCENE_ROOT_ENTITY);
            }

            entity.parentId = parentId;

            // add as orphan so system can parent it
            ECSTransformUtils.orphanEntities[entity] = new ECSTransformUtils.OrphanEntity(scene, entity, parentId);
        }

        private static bool TryMoveCharacter(IParcelScene scene, Vector3 localPosition,
            IWorldState worldState, IBaseVariable<Vector3> playerTeleportVariable)
        {
            // If player is not at the scene that triggered this event
            // we'll ignore it
            if (scene.sceneData.sceneNumber != worldState.GetCurrentSceneNumber() && !scene.isPersistent)
            {
                return false;
            }

            Vector2Int targetCoords = scene.sceneData.basePosition + Utils.WorldToGridPosition(localPosition);

            // If target coordinates are outside the scene we'll ignore it
            if (!ECSTransformUtils.IsInsideSceneBoundaries(scene, targetCoords))
            {
                return false;
            }

            playerTeleportVariable.Set(Utils.GridToWorldPosition(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y)
                                       + localPosition, true);

            return true;
        }
    }
}
