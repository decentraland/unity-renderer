using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSTransformHandler : IECSComponentHandler<ECSTransform>
    {
        private IWorldState worldState;

        public ECSTransformHandler(IWorldState worldState)
        {
            ECSTransformUtils.orphanEntities = new KeyValueSet<IDCLEntity, ECSTransformUtils.OrphanEntity>(10);
            this.worldState = worldState;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            if (entity.parentId != SpecialEntityId.SCENE_ROOT_ENTITY)
            {
                entity.parentId = SpecialEntityId.SCENE_ROOT_ENTITY;
                ECSTransformUtils.SetParent(scene, entity, SpecialEntityId.SCENE_ROOT_ENTITY);
            }
            ECSTransformUtils.orphanEntities.Remove(entity);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, ECSTransform model)
        {
            // From SDK `PLAYER_ENTITY` entity's transform can be modified to
            // move character across the scene
            if (entity.entityId == SpecialEntityId.PLAYER_ENTITY)
            {
                // If player is not at the scene that triggered this event
                // we'll ignore it
                if (scene.sceneData.id != worldState.currentSceneId)
                {
                    return;
                }

                Vector2Int targetCoords = scene.sceneData.basePosition
                                          + new Vector2Int(Mathf.CeilToInt(model.position.x), Mathf.CeilToInt(model.position.z));

                // If target coordinates are outside the scene we'll ignore it
                if (!scene.IsInsideSceneBoundaries(targetCoords))
                {
                    return;
                }

                // MOVE CHARACTER
                return;
            }

            Transform transform = entity.gameObject.transform;
            transform.localPosition = model.position;
            transform.localRotation = model.rotation;
            transform.localScale = model.scale;

            if (entity.parentId != model.parentId)
            {
                entity.parentId = model.parentId;
                ECSTransformUtils.orphanEntities.Remove(entity);

                // if `parentId` does not exist yet, we added to `orphanEntities` so system `ECSTransformParentingSystem`
                // can retry parenting later
                long parentId = model.parentId != entity.entityId ? model.parentId : (long)SpecialEntityId.SCENE_ROOT_ENTITY;
                if (!ECSTransformUtils.SetParent(scene, entity, parentId))
                {
                    ECSTransformUtils.orphanEntities[entity] = new ECSTransformUtils.OrphanEntity(scene, entity, parentId);
                }
            }
        }
    }
}