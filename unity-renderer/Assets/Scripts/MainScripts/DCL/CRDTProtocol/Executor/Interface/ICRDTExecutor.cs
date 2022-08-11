using System;

namespace DCL.CRDT
{
    public interface ICRDTExecutor : IDisposable
    {
        CRDTProtocol crdtProtocol { get; }
        void Execute(CRDTMessage crdtMessage);
        void ExecuteWithoutStoringState(CRDTMessage crdtMessage);
    }
}