using DCL.Controllers;
using DCL.CRDT;
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

public readonly struct ComponentIdentifier
{
    public readonly int SceneNumber;
    public readonly long EntityId;
    public readonly int ComponentId;

    public ComponentIdentifier(int sceneNumber, long entityId, int componentId)
    {
        this.SceneNumber = sceneNumber;
        this.EntityId = entityId;
        this.ComponentId = componentId;
    }
}

public readonly struct ComponentWriteData
{
    public readonly InternalComponent Data;
    public readonly bool IsDelayedRemoval;

    public ComponentWriteData(InternalComponent data, bool isDelayedRemoval)
    {
        this.Data = data;
        this.IsDelayedRemoval = isDelayedRemoval;
    }
}

public class InternalECSComponent<T> : IInternalECSComponent<T> where T: InternalComponent
{
    private readonly ECSComponentsFactory componentsFactory;
    private readonly int componentId;
    private readonly ECSComponent<T> component;
    private readonly IList<InternalComponentWriteData> scheduledWrite;
    private readonly KeyValueSet<ComponentIdentifier, ComponentWriteData> markAsDirtyComponents;
    private readonly IReadOnlyDictionary<int, ICRDTExecutor> crdtExecutors;

    public InternalECSComponent(InternalECSComponentsId id,
        ECSComponentsManager componentsManager,
        ECSComponentsFactory componentsFactory,
        Func<IECSComponentHandler<T>> handlerBuilder,
        IList<InternalComponentWriteData> scheduledWrite)
        : this(id, componentsManager, componentsFactory, handlerBuilder, scheduledWrite, null, null) { }

    public InternalECSComponent(InternalECSComponentsId id,
        ECSComponentsManager componentsManager,
        ECSComponentsFactory componentsFactory,
        Func<IECSComponentHandler<T>> handlerBuilder,
        IList<InternalComponentWriteData> scheduledWrite,
        KeyValueSet<ComponentIdentifier, ComponentWriteData> markAsDirtyComponents,
        IReadOnlyDictionary<int, ICRDTExecutor> crdtExecutors)
    {
        this.componentId = (int)id;
        this.componentsFactory = componentsFactory;
        this.scheduledWrite = scheduledWrite;
        this.markAsDirtyComponents = markAsDirtyComponents;
        this.crdtExecutors = crdtExecutors;

        componentsFactory.AddOrReplaceComponent<T>(componentId, x => (T)x, handlerBuilder);
        component = (ECSComponent<T>)componentsManager.GetOrCreateComponent(componentId);
    }

    public void PutFor(IParcelScene scene, IDCLEntity entity, T model)
    {
        PutFor(scene, entity.entityId, model);
    }

    public void PutFor(IParcelScene scene, long entityId, T model)
    {
        PutFor(scene.sceneData.sceneNumber, entityId, model);
    }

    public void PutFor(int sceneNumber, long entityId, T model)
    {
        if (crdtExecutors.TryGetValue(sceneNumber, out ICRDTExecutor crdtExecutor))
        {
            crdtExecutor.ExecuteWithoutStoringState(entityId, componentId, model);

            markAsDirtyComponents[new ComponentIdentifier(sceneNumber, entityId, componentId)] =
                new ComponentWriteData(model, false);
        }
    }

    public void RemoveFor(IParcelScene scene, IDCLEntity entity, T defaultModel = null)
    {
        RemoveFor(scene, entity.entityId, defaultModel);
    }

    public void RemoveFor(IParcelScene scene, long entityId, T defaultModel = null)
    {
        RemoveFor(scene.sceneData.sceneNumber, entityId, defaultModel);
    }

    public void RemoveFor(int sceneNumber, long entityId, T defaultModel = null)
    {
        if (!crdtExecutors.TryGetValue(sceneNumber, out ICRDTExecutor crdtExecutor))
        {
            return;
        }

        if (defaultModel != null)
        {
            crdtExecutor.ExecuteWithoutStoringState(entityId, componentId, defaultModel);

            markAsDirtyComponents[new ComponentIdentifier(sceneNumber, entityId, componentId)] =
                new ComponentWriteData(defaultModel, true);

            //scheduledWrite.Add(new InternalComponentWriteData(scene, entityId, componentId, defaultModel, true));
        }
        else
        {
            //scheduledWrite.Add(new InternalComponentWriteData(scene, entityId, componentId, null, false));
            crdtExecutor.ExecuteWithoutStoringState(entityId, componentId, null);
            markAsDirtyComponents.Remove(new ComponentIdentifier(sceneNumber, entityId, componentId));
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
