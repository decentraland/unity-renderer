using System;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;

public interface IInternalECSComponent<T> : IDisposable
{
    void PutFor(IParcelScene scene, IDCLEntity entity, T model);
    void RemoveFor(IParcelScene scene, IDCLEntity entity);
    IECSReadOnlyComponentData<T> GetFor(IParcelScene scene, IDCLEntity entity);
}