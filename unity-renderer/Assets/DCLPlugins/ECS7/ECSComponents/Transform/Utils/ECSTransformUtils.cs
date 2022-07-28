using DCL.Controllers;
using DCL.Models;

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

    public static bool SetParent(IParcelScene scene, IDCLEntity child, long parentId)
    {
        if (parentId == (long)SpecialEntityId.SCENE_ROOT_ENTITY)
        {
            child.SetParent(null);
            child.gameObject.transform.SetParent(scene.GetSceneTransform(), false);
            return true;
        }

        IDCLEntity parent = scene.GetEntityById(parentId);
        if (parent != null)
        {
            child.SetParent(parent);
            return true;
        }

        return false;
    }
}