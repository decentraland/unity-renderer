using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSTransformHandler : IECSComponentHandler<ECSTransform>
    {
        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            if (entity.parent != null)
            {
                scene.SetEntityParent(entity.entityId, (long)SpecialEntityId.SCENE_ROOT_ENTITY);
            }
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, ECSTransform model)
        {
            Transform transform = entity.gameObject.transform;
            transform.localPosition = model.position;
            transform.localRotation = model.rotation;
            transform.localScale = model.scale;

            long currentParent = entity.parent?.entityId ?? 0;
            if (currentParent != model.parentId)
            {
                scene.SetEntityParent(entity.entityId, model.parentId);
            }
        }
    }
}