using System;

namespace DCL.CRDT
{
    public interface ICRDTExecutor : IDisposable
    {
        CRDTProtocol crdtProtocol { get; }
        void Execute(CrdtMessage crdtMessage);
        void ExecuteWithoutStoringState(long entityId, int componentId, object data);
        void PutComponent<T>(long entityId, int componentId, T model);
        void RemoveComponent(long entityId, int componentId);
    }
}
