using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.Models;
using UnityEngine.UIElements;

namespace DCL.ECSComponents.UIAbstractElements
{
    /// <summary>
    /// Base class to handle common routines such as
    /// adding and removing UI Components and Visual Elements
    /// </summary>
    public abstract class UIElementHandlerBase
    {
        protected readonly IInternalECSComponent<InternalUiContainer> internalUiContainer;
        protected readonly int componentId;

        protected UIElementHandlerBase(
            IInternalECSComponent<InternalUiContainer> internalUiContainer, int componentId)
        {
            this.internalUiContainer = internalUiContainer;
            this.componentId = componentId;
        }

        protected internal InternalUiContainer AddElementToRoot(IParcelScene scene, IDCLEntity entity, VisualElement uiElement)
        {
            var internalContainer = AddComponentToEntity(scene, entity);
            internalContainer.rootElement.Add(uiElement);
            return internalContainer;
        }

        protected internal InternalUiContainer AddComponentToEntity(IParcelScene scene, IDCLEntity entity)
        {
            var containerModel = internalUiContainer.GetFor(scene, entity)?.model ?? new InternalUiContainer(entity.entityId);
            containerModel.components.Add(componentId);

            internalUiContainer.PutFor(scene, entity, containerModel);
            return containerModel;
        }

        protected internal void RemoveElementFromRoot(IParcelScene scene, IDCLEntity entity, VisualElement uiElement)
        {
            RemoveComponentFromEntity(scene, entity)?.rootElement?.Remove(uiElement);
        }

        protected internal InternalUiContainer? RemoveComponentFromEntity(IParcelScene scene, IDCLEntity entity)
        {
            var containerData = internalUiContainer.GetFor(scene, entity);
            if (containerData != null)
            {
                var containerModel = containerData.Value.model;
                containerModel.components.Remove(componentId);
                internalUiContainer.PutFor(scene, entity, containerModel);
                return containerModel;
            }

            return null;
        }
    }
}
