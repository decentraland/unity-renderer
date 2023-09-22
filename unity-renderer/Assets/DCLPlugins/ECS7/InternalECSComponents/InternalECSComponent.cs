using DCL.Controllers;
using DCL.CRDT;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using System;
using System.Collections.Generic;

public class InternalECSComponent<T> : IInternalECSComponent<T> where T: struct, IInternalComponent
{
    internal readonly struct DirtyData
    {
        public readonly T Data;
        public readonly bool IsDelayedRemoval;

        public DirtyData(T data, bool isDelayedRemoval)
        {
            Data = data;
            IsDelayedRemoval = isDelayedRemoval;
        }
    }

    private readonly ECSComponentsFactory componentsFactory;
    private readonly int componentId;
    private readonly ECSComponent<T> component;
    private readonly IReadOnlyDictionary<int, ICRDTExecutor> crdtExecutors;
    private readonly IComponentDirtySystem dirtySystem;
    internal readonly DualKeyValueSet<int, long, DirtyData> markAsDirtyComponents = new DualKeyValueSet<int, long, DirtyData>(100);
    internal readonly DualKeyValueSet<int, long, DirtyData> removeAsDirtyComponents = new DualKeyValueSet<int, long, DirtyData>(100);

    public InternalECSComponent(InternalECSComponentsId id,
        ECSComponentsManager componentsManager,
        ECSComponentsFactory componentsFactory,
        Func<IECSComponentHandler<T>> handlerBuilder,
        IReadOnlyDictionary<int, ICRDTExecutor> crdtExecutors,
        IComponentDirtySystem dirtySystem)
    {
        this.componentId = (int)id;
        this.componentsFactory = componentsFactory;
        this.crdtExecutors = crdtExecutors;
        this.dirtySystem = dirtySystem;

        componentsFactory.AddOrReplaceComponent<T>(componentId, x => (T)x, handlerBuilder);
        component = (ECSComponent<T>)componentsManager.GetOrCreateComponent(componentId);

        dirtySystem.MarkComponentsAsDirty += MarkDirtyComponentData;
        dirtySystem.RemoveComponentsAsDirty += ResetDirtyComponentData;
    }

    public void Dispose()
    {
        componentsFactory.RemoveComponent(componentId);
        dirtySystem.MarkComponentsAsDirty -= MarkDirtyComponentData;
        dirtySystem.RemoveComponentsAsDirty -= ResetDirtyComponentData;
    }

    public int ComponentId => componentId;

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
            markAsDirtyComponents[sceneNumber, entityId] = new DirtyData(model, false);

            crdtExecutor.PutComponent(entityId, component, model);
        }
    }

    public void RemoveFor(IParcelScene scene, IDCLEntity entity, T defaultModel) =>
        RemoveFor(scene, entity.entityId, defaultModel);

    public void RemoveFor(IParcelScene scene, long entityId, T defaultModel) =>
        RemoveFor(scene.sceneData.sceneNumber, entityId, defaultModel);

    public void RemoveFor(int sceneNumber, long entityId, T defaultModel)
    {
        if (!crdtExecutors.TryGetValue(sceneNumber, out ICRDTExecutor crdtExecutor))
            return;

        markAsDirtyComponents[sceneNumber, entityId] = new DirtyData(defaultModel, true);

        crdtExecutor.PutComponent(entityId, component, defaultModel);
    }

    public void RemoveFor(IParcelScene scene, IDCLEntity entity) =>
        RemoveFor(scene, entity.entityId);

    public void RemoveFor(IParcelScene scene, long entityId) =>
        RemoveFor(scene.sceneData.sceneNumber, entityId);

    public void RemoveFor(int sceneNumber, long entityId)
    {
        if (!crdtExecutors.TryGetValue(sceneNumber, out ICRDTExecutor crdtExecutor))
            return;

        markAsDirtyComponents.Remove(sceneNumber, entityId);
        crdtExecutor.RemoveComponent(entityId, componentId);
    }

    public ECSComponentData<T>? GetFor(IParcelScene scene, IDCLEntity entity)
    {
        return GetFor(scene, entity.entityId);
    }

    public ECSComponentData<T>? GetFor(IParcelScene scene, long entityId)
    {
        if (component.TryGet(scene, entityId, out var data))
        {
            if (markAsDirtyComponents.TryGetValue(scene.sceneData.sceneNumber, entityId, out var dirtyData))
            {
                T model = dirtyData.Data;
                model.dirty = true;
                return data.With(model);
            }

            return data;
        }

        return null;
    }

    public IReadOnlyList<KeyValueSetTriplet<IParcelScene, long, ECSComponentData<T>>> GetForAll()
    {
        return component.Get();
    }

    private void MarkDirtyComponentData()
    {
        var markAsDirty = markAsDirtyComponents.Pairs;

        for (int i = 0; i < markAsDirty.Count; i++)
        {
            int sceneNumber = markAsDirty[i].key1;
            long entityId = markAsDirty[i].key2;
            T data = markAsDirty[i].value.Data;
            bool isRemoval = markAsDirty[i].value.IsDelayedRemoval;

            if (!crdtExecutors.TryGetValue(sceneNumber, out ICRDTExecutor crdtExecutor))
            {
                continue;
            }

            data.dirty = true;
            crdtExecutor.PutComponent(entityId, component, data);
            removeAsDirtyComponents[sceneNumber, entityId] = new DirtyData(data, isRemoval);
        }

        markAsDirtyComponents.Clear();
    }

    private void ResetDirtyComponentData()
    {
        var resetDirtyComponents = removeAsDirtyComponents.Pairs;

        for (int i = 0; i < resetDirtyComponents.Count; i++)
        {
            int sceneNumber = resetDirtyComponents[i].key1;
            long entityId = resetDirtyComponents[i].key2;
            T data = resetDirtyComponents[i].value.Data;
            bool isRemoval = resetDirtyComponents[i].value.IsDelayedRemoval;

            if (!crdtExecutors.TryGetValue(sceneNumber, out ICRDTExecutor crdtExecutor))
            {
                continue;
            }

            if (isRemoval)
            {
                crdtExecutor.RemoveComponent(entityId, componentId);
            }
            else
            {
                data.dirty = false;
                crdtExecutor.PutComponent(entityId, component, data);
            }
        }

        removeAsDirtyComponents.Clear();
    }
}
