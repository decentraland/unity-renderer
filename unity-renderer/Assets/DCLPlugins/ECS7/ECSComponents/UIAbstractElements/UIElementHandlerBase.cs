using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.Models;
using UnityEngine.UIElements;

namespace DCL.ECSComponents.UIAbstractElements
{
    public abstract class UIElementHandlerBase
    {
        protected readonly IInternalECSComponent<InternalUiContainer> internalUiContainer;
        protected readonly int componentId;

        protected UIElementHandlerBase(IInternalECSComponent<InternalUiContainer> internalUiContainer, int componentId)
        {
            this.internalUiContainer = internalUiContainer;
            this.componentId = componentId;
        }

        protected internal InternalUiContainer AddElementToRoot(IParcelScene scene, IDCLEntity entity, VisualElement uiElement)
        {
            var containerModel = internalUiContainer.GetFor(scene, entity)?.model ?? new InternalUiContainer();
            containerModel.rootElement.Add(uiElement);
            containerModel.components.Add(componentId);

            internalUiContainer.PutFor(scene, entity, containerModel);
            return containerModel;
        }

        protected internal void RemoveElementFromRoot(IParcelScene scene, IDCLEntity entity, VisualElement uiElement)
        {
            var containerData = internalUiContainer.GetFor(scene, entity);
            if (containerData != null)
            {
                var containerModel = containerData.model;
                containerModel.rootElement.Remove(uiElement);
                containerModel.components.Remove(componentId);
                internalUiContainer.PutFor(scene, entity, containerModel);
            }
        }
    }
}
