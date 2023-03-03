using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using System;
using System.Collections.Generic;

public readonly struct InternalComponentWriteData
{
    public readonly IParcelScene scene;
    public readonly long entityId;
    public readonly int componentId;
    public readonly InternalComponent data;
    public readonly bool flaggedForRemoval;

    public InternalComponentWriteData(IParcelScene scene, long entityId, int componentId, InternalComponent data, bool flaggedForRemoval)
    {
        this.scene = scene;
        this.entityId = entityId;
        this.componentId = componentId;
        this.data = data;
        this.flaggedForRemoval = flaggedForRemoval;
    }
}

public class InternalECSComponent<T> : IInternalECSComponent<T> where T: InternalComponent
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
        scheduledWrite.Add(new InternalComponentWriteData(scene, entityId, componentId, model, false));
    }

    public void RemoveFor(IParcelScene scene, IDCLEntity entity, T defaultModel = null)
    {
        RemoveFor(scene, entity.entityId, defaultModel);
    }

    public void RemoveFor(IParcelScene scene, long entityId, T defaultModel = null)
    {
        if (defaultModel != null)
        {
            scheduledWrite.Add(new InternalComponentWriteData(scene, entityId, componentId, defaultModel, true));
        }
        else
        {
            scheduledWrite.Add(new InternalComponentWriteData(scene, entityId, componentId, null, false));
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
