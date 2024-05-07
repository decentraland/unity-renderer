using System;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;

public class UiContainerHandler : IECSComponentHandler<InternalUiContainer>
{
    private readonly IInternalECSComponent<InternalUiContainer> thisComponent;

    public UiContainerHandler(Func<IInternalECSComponent<InternalUiContainer>> componentGet)
    {
        this.thisComponent = componentGet();
    }

    public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

    public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
    {
        var containerData = thisComponent.GetFor(scene, entity);
        if (containerData != null)
        {
            containerData.Value.model.parentElement?.Remove(containerData.Value.model.rootElement);
        }
    }

    public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, InternalUiContainer model)
    {
        if (model.rootElement.childCount == 0 && model.components.Count == 0 && entity.entityId != SpecialEntityId.SCENE_ROOT_ENTITY)
        {
            thisComponent.RemoveFor(scene, entity);
        }
    }
}
