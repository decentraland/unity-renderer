using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSTransformHandler : IECSComponentHandler<ECSTransform>
    {
        public ECSTransformHandler()
        {
            ECSTransformUtils.orphanEntities = new KeyValueSet<IDCLEntity, ECSTransformUtils.OrphanEntity>(10);
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            if (entity.parentId != (long)SpecialEntityId.SCENE_ROOT_ENTITY)
            {
                entity.parentId = (long)SpecialEntityId.SCENE_ROOT_ENTITY;
                ECSTransformUtils.SetParent(scene, entity, (long)SpecialEntityId.SCENE_ROOT_ENTITY);
            }
            ECSTransformUtils.orphanEntities.Remove(entity);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, ECSTransform model)
        {
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
                long parentId = model.parentId != entity.entityId? model.parentId : (long)SpecialEntityId.SCENE_ROOT_ENTITY;
                if (!ECSTransformUtils.SetParent(scene, entity, parentId))
                {
                    ECSTransformUtils.orphanEntities[entity] = new ECSTransformUtils.OrphanEntity(scene, entity, parentId);
                }
            }
        }
    }
}