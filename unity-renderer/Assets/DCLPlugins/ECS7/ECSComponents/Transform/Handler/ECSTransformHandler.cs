using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSTransformHandler : IECSComponentHandler<ECSTransform>
    {
        private static readonly ECSTransform TransfomIdentity = new ECSTransform()
        {
            position = Vector3.zero,
            scale = Vector3.one,
            rotation = Quaternion.identity,
            parentId = SpecialEntityId.SCENE_ROOT_ENTITY
        };

        private readonly IWorldState worldState;
        private readonly BaseVariable<Vector3> playerTeleportVariable;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public ECSTransformHandler(int componentId, IWorldState worldState, BaseVariable<Vector3> playerTeleportVariable,
            IECSComponentWriter componentWriter)
        {
            ECSTransformUtils.orphanEntities = new KeyValueSet<IDCLEntity, ECSTransformUtils.OrphanEntity>(10);
            this.worldState = worldState;
            this.playerTeleportVariable = playerTeleportVariable;
            this.componentWriter = componentWriter;
            this.componentId = componentId;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            // if entity has any parent
            if (entity.parentId != SpecialEntityId.SCENE_ROOT_ENTITY)
            {
                // remove as child
                if (scene.entities.TryGetValue(entity.parentId, out IDCLEntity parent))
                {
                    parent.childrenId.Remove(entity.entityId);
                }

                // reset transform and re-parent to the scene
                entity.gameObject.transform.ResetLocalTRS();
                entity.parentId = SpecialEntityId.SCENE_ROOT_ENTITY;
                ECSTransformUtils.TrySetParent(scene, entity, SpecialEntityId.SCENE_ROOT_ENTITY);
            }

            // if entity has any children
            int childrenCount = entity.childrenId.Count;
            if (childrenCount > 0)
            {
                for (int i = childrenCount - 1; i >= 0; i--)
                {
                    long childId = entity.childrenId[i];
                    entity.childrenId.RemoveAt(i);

                    if (!scene.entities.TryGetValue(childId, out IDCLEntity child))
                        continue;

                    // re-parent child to the scene
                    ECSTransformUtils.TrySetParent(scene, child, SpecialEntityId.SCENE_ROOT_ENTITY);

                    // add child as orphan
                    ECSTransformUtils.orphanEntities[child] = new ECSTransformUtils.OrphanEntity(scene, child, child.parentId);
                }

                // create implicit transform component for this entity
                componentWriter.PutComponent(scene, entity, componentId, TransfomIdentity, ECSComponentWriteType.EXECUTE_LOCALLY);

            }
            ECSTransformUtils.orphanEntities.Remove(entity);
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
                ProcessNewParent(componentId, scene, entity, model.parentId, componentWriter);
            }
        }

        private static void ProcessNewParent(int componentId, IParcelScene scene, IDCLEntity entity, long parentId,
            IECSComponentWriter componentWriter)
        {
            IDCLEntity parent = null;

            // remove as child of previous parent
            if (entity.parentId != SpecialEntityId.SCENE_ROOT_ENTITY)
            {
                if (scene.entities.TryGetValue(entity.parentId, out parent))
                {
                    parent.childrenId.Remove(entity.entityId);
                }
            }

            entity.parentId = parentId;
            ECSTransformUtils.orphanEntities.Remove(entity);

            // if `parentId` does not exist yet, we added to `orphanEntities` so system `ECSTransformParentingSystem`
            // can retry parenting later
            if (ECSTransformUtils.TrySetParent(scene, entity, parentId, out parent))
            {
                if (entity.parentId != SpecialEntityId.SCENE_ROOT_ENTITY)
                {
                    parent.childrenId.Add(entity.entityId);
                }
            }
            else
            {
                // set entity as orphan
                ECSTransformUtils.orphanEntities[entity] = new ECSTransformUtils.OrphanEntity(scene, entity, parentId);

                // create implicit transform component for parent entity
                componentWriter.PutComponent(scene.sceneData.id, parentId, componentId, TransfomIdentity, 
                    ECSComponentWriteType.EXECUTE_LOCALLY);
            }
        }

        private static bool TryMoveCharacter(IParcelScene scene, Vector3 localPosition,
            IWorldState worldState, BaseVariable<Vector3> playerTeleportVariable)
        {
            // If player is not at the scene that triggered this event
            // we'll ignore it
            if (scene.sceneData.id != worldState.currentSceneId)
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