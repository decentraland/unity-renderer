using System.Linq;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

public static class ECSTransformUtils
{
    public readonly struct OrphanEntity
    {
        public readonly IParcelScene scene;
        public readonly IDCLEntity entity;
        public readonly long parentId;

        public OrphanEntity(IParcelScene scene, IDCLEntity entity, long parentId)
        {
            this.scene = scene;
            this.entity = entity;
            this.parentId = parentId;
        }
    }

    public static KeyValueSet<IDCLEntity, OrphanEntity> orphanEntities = null;

    public static bool TrySetParent(IParcelScene scene, IDCLEntity child, long parentId)
    {
        return TrySetParent(scene, child, parentId, out IDCLEntity parent);
    }

    public static bool TrySetParent(IParcelScene scene, IDCLEntity child, long parentId, out IDCLEntity parent)
    {
        if (parentId == SpecialEntityId.SCENE_ROOT_ENTITY)
        {
            child.gameObject.transform.SetParent(GetRootEntityTransform(scene), false);
            parent = null;
            return true;
        }
        if (parentId == SpecialEntityId.PLAYER_ENTITY)
        {
            parentId = SpecialEntityId.INTERNAL_PLAYER_ENTITY_REPRESENTATION;
        }

        if (scene.entities.TryGetValue(parentId, out parent))
        {
            child.gameObject.transform.SetParent(parent.gameObject.transform, false);
            return true;
        }

        return false;
    }

    public static bool IsCircularParenting(IParcelScene scene, IDCLEntity entity, long parentId)
    {
        if (parentId == SpecialEntityId.SCENE_ROOT_ENTITY)
        {
            return false;
        }

        if (parentId == entity.entityId)
        {
            return true;
        }

        do
        {
            if (!scene.entities.TryGetValue(parentId, out IDCLEntity parent))
                break;

            if (entity.entityId == parentId)
            {
                return true;
            }
            parentId = parent.parentId;
        } while (parentId != SpecialEntityId.SCENE_ROOT_ENTITY);

        return false;
    }

    public static Transform GetRootEntityTransform(IParcelScene scene)
    {
        return scene.GetSceneTransform();
    }

    public static bool IsInsideSceneBoundaries(IParcelScene scene, Vector2Int position)
    {
        return scene.sceneData.parcels.Contains(position);
    }
}