using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;

public class InternalECSComponent<T> : IInternalECSComponent<T>
{
    private readonly ECSComponentsFactory componentsFactory;
    private readonly int componentId;
    private readonly ECSComponent<T> component;

    public InternalECSComponent(InternalECSComponentsId id,
        ECSComponentsManager componentsManager,
        ECSComponentsFactory componentsFactory,
        Func<IECSComponentHandler<T>> handlerBuilder)
    {
        this.componentId = (int)id;
        this.componentsFactory = componentsFactory;

        componentsFactory.AddOrReplaceComponent<T>(componentId, x => (T)x, handlerBuilder);
        component = (ECSComponent<T>)componentsManager.GetOrCreateComponent(componentId);
    }

    public void PutFor(IParcelScene scene, IDCLEntity entity, T model)
    {
        scene.crdtExecutor.ExecuteWithoutStoringState(entity.entityId, componentId, model);
    }

    public void PutFor(IParcelScene scene, long entityId, T model)
    {
        scene.crdtExecutor.ExecuteWithoutStoringState(entityId, componentId, model);
    }

    public void RemoveFor(IParcelScene scene, IDCLEntity entity)
    {
        scene.crdtExecutor.ExecuteWithoutStoringState(entity.entityId, componentId, null);
    }

    public IECSReadOnlyComponentData<T> GetFor(IParcelScene scene, IDCLEntity entity)
    {
        return component.Get(scene, entity);
    }

    public IECSReadOnlyComponentData<T> GetFor(IParcelScene scene, long entityId)
    {
        return component.Get(scene, entityId);
    }

    public IReadOnlyList<KeyValueSetTriplet<IParcelScene, long, ECSComponentData<T>>> GetForAll()
    {
        return component.Get();
    }

    public void Dispose()
    {
        componentsFactory.RemoveComponent(componentId);
    }
}