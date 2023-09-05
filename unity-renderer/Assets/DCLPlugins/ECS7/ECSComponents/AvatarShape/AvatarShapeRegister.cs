using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using System;
using DCL.ECSRuntime;
using UnityEngine;
using Object = UnityEngine.Object;

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

        public AvatarShapeRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, IInternalECSComponents internalComponents)
        {
            var poolWrapper = new ECSReferenceTypeIecsComponentPool<PBAvatarShape>(
                new WrappedComponentPool<IWrappedComponent<PBAvatarShape>>(10,
                    () => new ProtobufWrappedComponent<PBAvatarShape>(new PBAvatarShape()))
            );

            AvatarShape avatarShapePrefab = Resources.Load<AvatarShape>(AVATAR_PREFAB_PATH);
            ConfigurePool(avatarShapePrefab.gameObject);

            factory.AddOrReplaceComponent(componentId,
                () => new AvatarShapeComponentHandler(pool, internalComponents.renderersComponent),
                AvatarShapeSerializer.Deserialize, // FD::
                iecsComponentPool: poolWrapper
                );

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
            if (pool != null) return;

            pool = PoolManager.i.AddPool(AVATAR_POOL_NAME, Object.Instantiate(prefab).gameObject, isPersistent: true);

            pool.ForcePrewarm(forceActive: false);
        }
    }
}
