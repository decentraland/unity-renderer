using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    /// <summary>
    /// This class will handle the avatar shape for the global scenes and the scenes that use it
    /// Take into account that this component uses a pool to manage the prefabs
    /// </summary>
    public class AvatarShapeComponentHandler : IECSComponentHandler<PBAvatarShape>
    {
        internal IAvatarShape avatar;
        private readonly Pool pool;
        private readonly PoolableObject poolableObject;
        private readonly IInternalECSComponent<InternalRenderers> renderersInternalComponent;
        private IParcelScene scene;
        private IDCLEntity entity;

        private bool isAvatarInitialized = false;

        public AvatarShapeComponentHandler(Pool pool, IInternalECSComponent<InternalRenderers> renderersComponent)
        {
            this.pool = pool;
            poolableObject = pool.Get();
            renderersInternalComponent = renderersComponent;
            avatar = poolableObject.gameObject.GetComponent<AvatarShape>();
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            this.scene = scene;
            this.entity = entity;

            avatar.transform.SetParent(entity.gameObject.transform, false);

            avatar.internalAvatar.OnCombinedRendererUpdate += OnCombinedRendererUpdate;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            if (avatar == null)
                return;

            avatar.internalAvatar.OnCombinedRendererUpdate -= OnCombinedRendererUpdate;
            renderersInternalComponent.RemoveFor(scene, entity);

            avatar.Cleanup();
            pool.Release(poolableObject);
            avatar = null;
            isAvatarInitialized = false;
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBAvatarShape model)
        {
            if (!isAvatarInitialized)
            {
                avatar.Init();
                isAvatarInitialized = true;
            }
            avatar.ApplyModel(scene, entity, model);
        }

        private void OnCombinedRendererUpdate(Renderer newRenderer)
        {
            renderersInternalComponent.RemoveFor(scene, entity);
            renderersInternalComponent.AddRenderer(scene, entity, newRenderer);
        }
    }
}
