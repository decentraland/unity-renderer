using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;

public interface IInternalECSComponent<T> : IDisposable
{
    void PutFor(IParcelScene scene, IDCLEntity entity, T model);
    void PutFor(IParcelScene scene, long entityId, T model);
    void RemoveFor(IParcelScene scene, IDCLEntity entity);
    void RemoveFor(IParcelScene scene, IDCLEntity entity, T defaultModel);
    IECSReadOnlyComponentData<T> GetFor(IParcelScene scene, IDCLEntity entity);
    IECSReadOnlyComponentData<T> GetFor(IParcelScene scene, long entityId);
    IReadOnlyList<KeyValueSetTriplet<IParcelScene, long, ECSComponentData<T>>> GetForAll();
}