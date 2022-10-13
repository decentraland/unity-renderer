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
            containerData.model.parentElement?.Remove(containerData.model.rootElement);
            containerData.model.parentElement = null;
        }
    }

    public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, InternalUiContainer model)
    {
        if (model.rootElement.childCount == 0 && !model.hasTransform)
        {
            thisComponent.RemoveFor(scene, entity);
        }
    }
}