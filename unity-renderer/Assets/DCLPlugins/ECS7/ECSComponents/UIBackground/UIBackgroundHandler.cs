using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class UIBackgroundHandler : IECSComponentHandler<PBUiBackground>
    {
        private readonly IInternalECSComponent<InternalUiContainer> internalUiContainer;
        private readonly int componentId;

        public UIBackgroundHandler(IInternalECSComponent<InternalUiContainer> internalUiContainer, int componentId)
        {
            this.internalUiContainer = internalUiContainer;
            this.componentId = componentId;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            RemoveFromContainer(scene, entity);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBUiBackground model)
        {
            if (model.BackgroundColor == null || model.BackgroundColor.A == 0)
            {
                RemoveFromContainer(scene, entity);
            }
            else
            {
                var containerModel = internalUiContainer.GetFor(scene, entity)?.model ?? new InternalUiContainer();
                containerModel.components.Add(componentId);
                containerModel.rootElement.style.backgroundColor = model.BackgroundColor.ToUnityColor();
                internalUiContainer.PutFor(scene, entity, containerModel);
            }
        }

        private void RemoveFromContainer(IParcelScene scene, IDCLEntity entity)
        {
            var containerData = internalUiContainer.GetFor(scene, entity);
            if (containerData != null)
            {
                var containerModel = containerData.model;
                if (containerModel.components.Remove(componentId))
                {
                    containerModel.rootElement.style.backgroundColor = Color.clear;
                    internalUiContainer.PutFor(scene, entity, containerModel);
                }
            }
        }
    }
}