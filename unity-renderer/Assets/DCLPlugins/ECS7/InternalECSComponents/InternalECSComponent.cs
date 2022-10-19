using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;

public readonly struct InternalComponentWriteData
{
    public readonly IParcelScene scene;
    public readonly long entityId;
    public readonly int componentId;
    public readonly InternalComponent data;

    public InternalComponentWriteData(IParcelScene scene, long entityId, int componentId, InternalComponent data)
    {
        this.scene = scene;
        this.entityId = entityId;
        this.componentId = componentId;
        this.data = data;
    }
}

public class InternalECSComponent<T> : IInternalECSComponent<T> where T : InternalComponent
{
    private readonly ECSComponentsFactory componentsFactory;
    private readonly int componentId;
    private readonly ECSComponent<T> component;
    private readonly IList<InternalComponentWriteData> scheduledWrite;

    public InternalECSComponent(InternalECSComponentsId id,
        ECSComponentsManager componentsManager,
        ECSComponentsFactory componentsFactory,
        Func<IECSComponentHandler<T>> handlerBuilder,
        IList<InternalComponentWriteData> scheduledWrite)
    {
        this.componentId = (int)id;
        this.componentsFactory = componentsFactory;
        this.scheduledWrite = scheduledWrite;

        componentsFactory.AddOrReplaceComponent<T>(componentId, x => (T)x, handlerBuilder);
        component = (ECSComponent<T>)componentsManager.GetOrCreateComponent(componentId);
    }

    public void PutFor(IParcelScene scene, IDCLEntity entity, T model)
    {
        PutFor(scene, entity.entityId, model);
    }

    public void PutFor(IParcelScene scene, long entityId, T model)
    {
        model._dirty = true;
        scene.crdtExecutor.ExecuteWithoutStoringState(entityId, componentId, model);
        scheduledWrite.Add(new InternalComponentWriteData(scene, entityId, componentId, model));
    }

    public void RemoveFor(IParcelScene scene, IDCLEntity entity)
    {
        RemoveFor(scene, entity, null);
    }

    public void RemoveFor(IParcelScene scene, IDCLEntity entity, T defaultModel)
    {
        if (defaultModel != null)
        {
            defaultModel._dirty = true;
            scene.crdtExecutor.ExecuteWithoutStoringState(entity.entityId, componentId, defaultModel);
            scheduledWrite.Add(new InternalComponentWriteData(scene, entity.entityId, componentId, null));
        }
        else
        {
            scene.crdtExecutor.ExecuteWithoutStoringState(entity.entityId, componentId, null);
        }
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