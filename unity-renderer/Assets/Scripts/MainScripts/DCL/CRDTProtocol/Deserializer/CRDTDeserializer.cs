using KernelCommunication;
using System;
using System.Collections.Generic;

namespace DCL.CRDT
{
    // Deserialize CRDT binary messages (BigEndian)
    public static class CRDTDeserializer
    {
        public static IEnumerator<object> DeserializeBatch(ReadOnlyMemory<byte> memory)
        {
            int position = 0;
            int remainingBytesToRead = 0;

            // While we have a header to read
            while (position + CrdtConstants.MESSAGE_HEADER_LENGTH <= memory.Length)
            {
                int messageLength = ByteUtils.ReadInt32(memory.Span, position);
                position += 4;

                CrdtMessageType messageType = (CrdtMessageType)ByteUtils.ReadInt32(memory.Span, position);
                position += 4;

                // Message length lower than minimal, it's an invalid message
                if (messageLength <= CrdtConstants.MESSAGE_HEADER_LENGTH)
                {
                    break;
                }

                // Do we have the bytes computed in the header?
                remainingBytesToRead = messageLength - CrdtConstants.MESSAGE_HEADER_LENGTH;

                if (position + remainingBytesToRead > memory.Length)
                {
                    break;
                }

                switch (messageType)
                {
                    case CrdtMessageType.PUT_COMPONENT:
                        yield return DeserializePutComponent(memory, ref position);
                        break;

                    case CrdtMessageType.DELETE_COMPONENT:
                        yield return DeserializeDeleteComponent(memory, ref position);
                        break;

                    case CrdtMessageType.DELETE_ENTITY:
                        yield return DeserializeDeleteEntity(memory, ref position);
                        break;

                    case CrdtMessageType.APPEND_COMPONENT:
                        yield return DeserializeAppendComponent(memory, ref position);
                        break;

                    default:
                        position += messageLength - CrdtConstants.MESSAGE_HEADER_LENGTH;
                        break;
                }
            }
        }

        public static CrdtMessage? DeserializePutComponent(ReadOnlyMemory<byte> memory, ref int memoryPosition)
        {
            ReadOnlySpan<byte> memorySpan = memory.Span;

            if (memoryPosition + CrdtConstants.CRDT_PUT_COMPONENT_HEADER_LENGTH > memorySpan.Length)
            {
                return null;
            }

            long entityId = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;
            int componentId = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;
            int timestamp = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;
            int dataLength = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;

            if (memoryPosition + dataLength > memorySpan.Length)
            {
                return null;
            }

            byte[] data = null;

            if (dataLength > 0)
            {
                data = memorySpan.Slice(memoryPosition, dataLength).ToArray();
            }
            else
            {
                data = new byte[0];
            }

            memoryPosition += dataLength;

            return new CrdtMessage
            (
                entityId: entityId,
                componentId: componentId,
                timestamp: timestamp,
                data: data,
                type: CrdtMessageType.PUT_COMPONENT
            );
        }

        private static CrdtMessage? DeserializeAppendComponent(ReadOnlyMemory<byte> memory, ref int memoryPosition)
        {
            ReadOnlySpan<byte> memorySpan = memory.Span;

            if (memoryPosition + CrdtConstants.CRDT_APPEND_COMPONENT_HEADER_LENGTH > memorySpan.Length)
            {
                return null;
            }

            long entityId = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;
            int componentId = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;
            int timestamp = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;
            int dataLength = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;

            if (memoryPosition + dataLength > memorySpan.Length)
            {
                return null;
            }

            byte[] data = null;

            if (dataLength > 0)
            {
                data = memorySpan.Slice(memoryPosition, dataLength).ToArray();
            }
            else
            {
                data = new byte[0];
            }

            memoryPosition += dataLength;

            return new CrdtMessage
            (
                entityId: entityId,
                componentId: componentId,
                timestamp: timestamp,
                data: data,
                type: CrdtMessageType.APPEND_COMPONENT
            );
        }

        private static CrdtMessage? DeserializeDeleteComponent(ReadOnlyMemory<byte> memory, ref int memoryPosition)
        {
            ReadOnlySpan<byte> memorySpan = memory.Span;

            if (memoryPosition + CrdtConstants.CRDT_DELETE_COMPONENT_HEADER_LENGTH > memorySpan.Length)
            {
                return null;
            }

            long entityId = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;
            int componentId = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;
            int timestamp = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;

            return new CrdtMessage
            (
                entityId: entityId,
                componentId: componentId,
                timestamp: timestamp,
                data: null,
                type: CrdtMessageType.DELETE_COMPONENT
            );
        }

        private static CrdtMessage? DeserializeDeleteEntity(ReadOnlyMemory<byte> memory, ref int memoryPosition)
        {
            ReadOnlySpan<byte> memorySpan = memory.Span;

            if (memoryPosition + CrdtConstants.CRDT_DELETE_ENTITY_HEADER_LENGTH > memorySpan.Length)
            {
                return null;
            }

            long entityId = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;

            return new CrdtMessage
            (
                entityId: entityId,
                componentId: 0,
                timestamp: 0,
                data: null,
                type: CrdtMessageType.DELETE_ENTITY
            );
        }
    }
}
