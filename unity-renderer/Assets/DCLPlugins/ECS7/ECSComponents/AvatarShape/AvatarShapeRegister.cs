using System;
using DCL.ECSRuntime;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class AvatarShapeRegister : IDisposable
    {
        private const string AVATAR_POOL_NAME = "AvatarShapeECS7";
        private const string AVATAR_PREFAB_PATH = "NewAvatarShape";
        
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        private Pool pool;
        
        public AvatarShapeRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            AvatarShape avatarShapePrefab = Resources.Load<AvatarShape>(AVATAR_PREFAB_PATH);
            ConfigurePool(avatarShapePrefab.gameObject);
            factory.AddOrReplaceComponent(componentId, AvatarShapeSerializer.Deserialize, () => new AvatarShapeComponentHandler(pool));
            componentWriter.AddOrReplaceComponentSerializer<PBAvatarShape>(componentId, AvatarShapeSerializer.Serialize);

            this.factory = factory;
            this.componentWriter = componentWriter;
            this.componentId = componentId;
        }

        public void Dispose()
        {
            factory.RemoveComponent(componentId);
            componentWriter.RemoveComponentSerializer(componentId);
            
            PoolManager.i.RemovePool(AVATAR_POOL_NAME);
            pool.ReleaseAll();
        }

        internal void ConfigurePool(GameObject prefab)
        {
            pool = PoolManager.i.GetPool(AVATAR_POOL_NAME);
            if (pool != null)
                return;

            pool = PoolManager.i.AddPool(
                AVATAR_POOL_NAME,
                GameObject.Instantiate(prefab).gameObject,
                isPersistent: true);

            pool.ForcePrewarm();
        }
    }
}