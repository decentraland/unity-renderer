using System;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;

public class RemoveOnConditionHandler<T> : IECSComponentHandler<T> where T : InternalComponent
{

    private readonly IInternalECSComponent<T> thisComponent;
    private readonly Func<T, bool> condition;

    public RemoveOnConditionHandler(Func<IInternalECSComponent<T>> componentGet, Func<T, bool> condition)
    {
        this.thisComponent = componentGet();
        this.condition = condition;
    }

    public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

    public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity) { }

    public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, T model)
    {
        if (condition(model))
        {
            thisComponent.RemoveFor(scene, entity);
        }
    }
}