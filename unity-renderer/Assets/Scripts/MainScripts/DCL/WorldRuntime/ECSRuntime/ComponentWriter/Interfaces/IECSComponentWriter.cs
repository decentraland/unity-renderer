using System;
using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    public interface IECSComponentWriter : IDisposable
    {
        void AddOrReplaceComponentSerializer<T>(int componentId, Func<T, byte[]> serializer);
        void PutComponent<T>(IParcelScene scene, IDCLEntity entity, int componentId, T model);
        void RemoveComponent(IParcelScene scene, IDCLEntity entity, int componentId);
    }
}