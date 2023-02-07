using System;
using System.Collections.Generic;
using KernelCommunication;

namespace DCL.CRDT
{
    // Deserialize CRDT binary messages (BigEndian)
    public static class CRDTDeserializer
    {
        internal static readonly CRDTMessage tempMessage = new CRDTMessage();

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
                        yield return DeserializePutComponent(memory, messageType, ref position);
                        break;

                    case CrdtMessageType.DELETE_COMPONENT:
                        yield return DeserializeDeleteComponent(memory, messageType, ref position);
                        break;

                    case CrdtMessageType.DELETE_ENTITY:
                        yield return DeserializeDeleteEntity(memory, messageType, ref position);
                        break;

                    default:
                        position += messageLength - CrdtConstants.MESSAGE_HEADER_LENGTH;
                        break;
                }
            }
        }

        public static CRDTMessage DeserializePutComponent(ReadOnlyMemory<byte> memory, CrdtMessageType messageType, ref int memoryPosition)
        {
            ReadOnlySpan<byte> memorySpan = memory.Span;

            if (memoryPosition + CrdtConstants.CRDT_PUT_COMPONENT_HEADER_LENGTH > memorySpan.Length)
            {
                return null;
            }

            tempMessage.entityId = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;
            tempMessage.componentId = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;
            tempMessage.timestamp = ByteUtils.ReadInt32(memorySpan, memoryPosition);
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
            else if (messageType == CrdtMessageType.PUT_COMPONENT)
            {
                data = new byte[0];
            }
            memoryPosition += dataLength;

            return new CRDTMessage()
            {
                type = CrdtMessageType.PUT_COMPONENT,
                entityId = tempMessage.entityId,
                componentId= tempMessage.componentId,
                timestamp = tempMessage.timestamp,
                data = data
            };
        }

        public static CRDTMessage DeserializeDeleteComponent(ReadOnlyMemory<byte> memory, CrdtMessageType messageType, ref int memoryPosition)
        {
            ReadOnlySpan<byte> memorySpan = memory.Span;

            if (memoryPosition + CrdtConstants.CRDT_DELETE_COMPONENT_HEADER_LENGTH > memorySpan.Length)
            {
                return null;
            }

            tempMessage.entityId = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;
            tempMessage.componentId = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;
            tempMessage.timestamp = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;

            return new CRDTMessage()
            {
                type = CrdtMessageType.DELETE_COMPONENT,
                entityId = tempMessage.entityId,
                componentId= tempMessage.componentId,
                timestamp = tempMessage.timestamp
            };
        }


        public static CRDTMessage DeserializeDeleteEntity(ReadOnlyMemory<byte> memory, CrdtMessageType messageType, ref int memoryPosition)
        {
            ReadOnlySpan<byte> memorySpan = memory.Span;

            if (memoryPosition + CrdtConstants.CRDT_DELETE_ENTITY_HEADER_LENGTH > memorySpan.Length)
            {
                return null;
            }

            tempMessage.entityId = ByteUtils.ReadInt32(memorySpan, memoryPosition);
            memoryPosition += 4;

            return new CRDTMessage()
            {
                type = CrdtMessageType.DELETE_ENTITY,
                entityId = tempMessage.entityId
            };
        }
    }
}
