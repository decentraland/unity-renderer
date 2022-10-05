using System;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;

public class UiContainerHandler : IECSComponentHandler<InternalUiContainer>
{
    private readonly IInternalECSComponent<InternalUiContainer> thisComponent;
    private InternalUiContainer currentModel;

    public UiContainerHandler(Func<IInternalECSComponent<InternalUiContainer>> componentGet)
    {
        this.thisComponent = componentGet();
    }

    public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

    public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
    {
        currentModel.rootElement.Clear();
    }

    public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, InternalUiContainer model)
    {
        currentModel = model;

        if (model.childElements.Count == 0 && !model.hasTransform)
        {
            thisComponent.RemoveFor(scene, entity);
        }
    }
}