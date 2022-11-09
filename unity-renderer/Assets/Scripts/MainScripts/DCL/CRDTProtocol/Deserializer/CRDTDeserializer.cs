using System;
using System.Collections.Generic;
using KernelCommunication;

namespace DCL.CRDT
{
    // Deserialize CRDT binary messages (BigEndian)
    public static class CRDTDeserializer
    {
        internal static readonly CRDTComponentMessageHeader componentHeader = new CRDTComponentMessageHeader();

        public static IEnumerator<object> DeserializeBatch(ReadOnlyMemory<byte> memory)
        {
            int position = 0;

            while (position < memory.Length)
            {
                int messageLength = ByteUtils.ReadInt32(memory.Span, position);
                position += 4;

                CrdtMessageType messageType = (CrdtMessageType)ByteUtils.ReadInt32(memory.Span, position);
                position += 4;

                if (messageLength <= CrdtConstants.MESSAGE_HEADER_LENGTH)
                {
                    continue;
                }

                switch (messageType)
                {
                    case CrdtMessageType.PUT_COMPONENT:
                    case CrdtMessageType.DELETE_COMPONENT:
                        yield return DeserializeSingle(memory, messageType, ref position);
                        break;
                    default:
                        position += messageLength - CrdtConstants.MESSAGE_HEADER_LENGTH;
                        break;
                }
            }
        }

        public static CRDTMessage DeserializeSingle(ReadOnlyMemory<byte> memory, CrdtMessageType messageType, ref int memoryPosition)
        {
            ReadOnlySpan<byte> memorySpan = memory.Span;

            componentHeader.entityId = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;
            componentHeader.componentClassId = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;
            componentHeader.timestamp = ByteUtils.ReadInt64(memorySpan, memoryPosition);
            memoryPosition += 8;
            componentHeader.dataLength = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;

            byte[] data = null;
            if (componentHeader.dataLength > 0 && messageType != CrdtMessageType.DELETE_COMPONENT)
            {
                data = memorySpan.Slice(memoryPosition, componentHeader.dataLength).ToArray();
            }
            else if (messageType == CrdtMessageType.PUT_COMPONENT)
            {
                data = new byte[0];
            }
            memoryPosition += componentHeader.dataLength;

            return new CRDTMessage()
            {
                key1 = componentHeader.entityId,
                key2 = componentHeader.componentClassId,
                timestamp = componentHeader.timestamp,
                data = data
            };
        }
    }
}