using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using System;
using System.Collections.Generic;

public interface IInternalECSComponent<T> : IDisposable
{
    int ComponentId { get; }
    void PutFor(IParcelScene scene, IDCLEntity entity, T model);
    void PutFor(IParcelScene scene, long entityId, T model);
    void PutFor(int sceneNumber, long entityId, T model);
    void RemoveFor(IParcelScene scene, IDCLEntity entity, T defaultModel = default);
    void RemoveFor(IParcelScene scene, long entityId, T defaultModel = default);
    void RemoveFor(int sceneNumber, long entityId, T defaultModel = default);
    IECSReadOnlyComponentData<T> GetFor(IParcelScene scene, IDCLEntity entity);
    IECSReadOnlyComponentData<T> GetFor(IParcelScene scene, long entityId);
    IReadOnlyList<KeyValueSetTriplet<IParcelScene, long, ECSComponentData<T>>> GetForAll();
}
