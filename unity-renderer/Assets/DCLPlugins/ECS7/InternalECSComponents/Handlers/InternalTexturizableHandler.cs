using System;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;

public class InternalTexturizableHandler : IECSComponentHandler<InternalTexturizable>
{
    private readonly IInternalECSComponent<InternalTexturizable> thisComponent;

    public InternalTexturizableHandler(Func<IInternalECSComponent<InternalTexturizable>> componentGet)
    {
        this.thisComponent = componentGet();
    }

    public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

    public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity) { }

    public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, InternalTexturizable model)
    {
        if (model.renderers.Count == 0)
        {
            thisComponent.RemoveFor(scene, entity);
        }
    }
}