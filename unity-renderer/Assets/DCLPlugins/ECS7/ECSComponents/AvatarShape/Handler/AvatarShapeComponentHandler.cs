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
        private Renderer renderer;
        private PBAvatarShape prevModel;

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

            if (renderer)
            {
                renderersInternalComponent.RemoveRenderer(scene, entity, renderer);
            }

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

            // We assume avatar combined renderer is going to change by checking equality
            // between model update
            // we should refactor this avatar shape handler
            // so the assumption is fair enough for the moment
            if (renderer && IsRendererGoingToChange(model, prevModel))
            {
                renderersInternalComponent.RemoveRenderer(scene, entity, renderer);
            }

            prevModel = model;
            avatar.ApplyModel(scene, entity, model);
        }

        private void OnCombinedRendererUpdate(Renderer newRenderer)
        {
            if (renderer)
            {
                renderersInternalComponent.RemoveRenderer(scene, entity, renderer);
            }

            renderersInternalComponent.AddRenderer(scene, entity, newRenderer);
            renderer = newRenderer;
        }

        private static bool IsRendererGoingToChange(PBAvatarShape newModel, PBAvatarShape prevModel)
        {
            if (prevModel == null) return false;

            prevModel.ExpressionTriggerId = newModel.ExpressionTriggerId;
            prevModel.ExpressionTriggerTimestamp = newModel.ExpressionTriggerTimestamp;
            prevModel.Talking = newModel.Talking;

            return !prevModel.Equals(newModel);
        }
    }
}
