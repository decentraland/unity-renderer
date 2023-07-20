using DCL.CRDT;
using DCL.ECS7.ComponentWrapper;
using System;

namespace DCL.ECS7
{
    public readonly struct WriteData : IDisposable
    {
        public readonly IPooledWrappedComponent PooledWrappedComponent;
        public readonly CrdtMessageType MessageType;

        public WriteData(IPooledWrappedComponent pooledWrappedComponent, CrdtMessageType messageType)
        {
            PooledWrappedComponent = pooledWrappedComponent;
            MessageType = messageType;
        }

        public WriteData(CrdtMessageType messageType)
        {
            MessageType = messageType;
            PooledWrappedComponent = null;
        }

        public void Dispose()
        {
            PooledWrappedComponent?.Dispose();
        }
    }

    public record ComponentWriter
    {
        private readonly DualKeyValueSet<long, int, WriteData> outgoingMessages;

        public ComponentWriter(DualKeyValueSet<long, int, WriteData> sceneOutgoingMessages)
        {
            outgoingMessages = sceneOutgoingMessages;
        }

        public void Put(long entityId, int componentId, IPooledWrappedComponent component)
        {
            if (outgoingMessages.TryGetValue(entityId, componentId, out var storedData))
                storedData.Dispose();

            outgoingMessages[entityId, componentId] = new WriteData(component, CrdtMessageType.PUT_COMPONENT);
        }

        public void Remove(long entityId, int componentId)
        {
            if (outgoingMessages.TryGetValue(entityId, componentId, out var storedData))
                storedData.Dispose();

            outgoingMessages[entityId, componentId] = new WriteData(CrdtMessageType.DELETE_COMPONENT);
        }

        public void Append(long entityId, int componentId, IPooledWrappedComponent component)
        {
            if (outgoingMessages.TryGetValue(entityId, componentId, out var storedData))
                storedData.Dispose();

            outgoingMessages[entityId, componentId] = new WriteData(component, CrdtMessageType.APPEND_COMPONENT);
        }
    }
}
