using System;

namespace DCL.CRDT
{
    public interface ICRDTExecutor : IDisposable
    {
        void Execute(CRDTMessage crdtMessage);
    }
}