using System.Collections.Generic;
using DCL.Components;
using DCL.Models;

namespace DCL.Controllers
{
    public static class SceneUtils
    {
        public static DecentralandEntity DuplicateEntity(ParcelScene scene, DecentralandEntity entity)
        {
            if (!scene.entities.ContainsKey(entity.entityId))
                return null;

            DecentralandEntity newEntity = scene.CreateEntity(System.Guid.NewGuid().ToString());

            if (entity.children.Count > 0)
            {
                using (var iterator = entity.children.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        DecentralandEntity childDuplicate = DuplicateEntity(scene, iterator.Current.Value);
                        childDuplicate.SetParent(newEntity);
                    }
                }
            }

            if (entity.parent != null)
                scene.SetEntityParent(newEntity.entityId, entity.parent.entityId);

            DCLTransform.model.position = WorldStateUtils.ConvertUnityToScenePosition(entity.gameObject.transform.position);
            DCLTransform.model.rotation = entity.gameObject.transform.rotation;
            DCLTransform.model.scale = entity.gameObject.transform.lossyScale;

            foreach (KeyValuePair<CLASS_ID_COMPONENT, IEntityComponent> component in entity.components)
            {
                scene.EntityComponentCreateOrUpdateWithModel(newEntity.entityId, component.Key, component.Value.GetModel());
            }

            foreach (KeyValuePair<System.Type, ISharedComponent> component in entity.GetSharedComponents())
            {
                ISharedComponent sharedComponent = scene.SharedComponentCreate(System.Guid.NewGuid().ToString(), component.Value.GetClassId());
                sharedComponent.UpdateFromModel(component.Value.GetModel());
                scene.SharedComponentAttach(newEntity.entityId, sharedComponent.id);
            }

            return newEntity;
        }
    }
}