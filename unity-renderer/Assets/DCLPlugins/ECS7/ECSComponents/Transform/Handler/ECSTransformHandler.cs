using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSTransformHandler : IECSComponentHandler<ECSTransform>
    {
        private readonly IWorldState worldState;
        private readonly BaseVariable<Vector3> playerTeleportVariable;

        public ECSTransformHandler(IWorldState worldState, BaseVariable<Vector3> playerTeleportVariable)
        {
            ECSTransformUtils.orphanEntities = new KeyValueSet<IDCLEntity, ECSTransformUtils.OrphanEntity>(100);
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
            transform.localPosition = model.position;
            transform.localRotation = model.rotation;
            transform.localScale = model.scale;

            if (entity.parentId != model.parentId)
            {
                ProcessNewParent(scene, entity, model.parentId);
            }
        }

        private static void ProcessNewParent(IParcelScene scene, IDCLEntity entity, long parentId)
        {
            //check for cyclic parenting
            if (ECSTransformUtils.IsCircularParenting(scene, entity, parentId))
            {
                Debug.LogError($"cyclic parenting found for entity {entity.entityId} " +
                               $"parenting to {parentId} at scene {scene.sceneData.id} ({scene.sceneData.basePosition})");
                return;
            }

            // remove as child of previous parent
            if (entity.parentId != SpecialEntityId.SCENE_ROOT_ENTITY)
            {
                if (scene.entities.TryGetValue(entity.parentId, out IDCLEntity parent))
                {
                    parent.childrenId.Remove(entity.entityId);
                }
            }

            entity.parentId = parentId;

            // add as orphan so system can parent it
            ECSTransformUtils.orphanEntities[entity] = new ECSTransformUtils.OrphanEntity(scene, entity, parentId);
        }

        private static bool TryMoveCharacter(IParcelScene scene, Vector3 localPosition,
            IWorldState worldState, BaseVariable<Vector3> playerTeleportVariable)
        {
            // If player is not at the scene that triggered this event
            // we'll ignore it
            if (scene.sceneData.id != worldState.GetCurrentSceneId())
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
                                       + localPosition);
            return true;
        }
    }
}
