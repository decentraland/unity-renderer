using System;
using System.Collections.Generic;
using DCL.CRDT;

namespace KernelCommunication
{
    // Deserialize kernel binary messages (BigEndian)
    public static class KernelBinaryMessageDeserializer
    {
        public static IEnumerator<object> Deserialize(IntPtr intPtr, int messageLength)
        {
            return Deserialize(new UnmanagedMemoryReader(intPtr, messageLength));
        }

        public static IEnumerator<object> Deserialize(byte[] bytes)
        {
            return Deserialize(new ByteArrayReader(bytes));
        }

        public static IEnumerator<object> Deserialize(IBinaryReader reader)
        {
            const int headerLength = 8;

            while (reader.CanRead())
            {
                int messageLength = reader.ReadInt32();
                CrdtMessageType messageType = (CrdtMessageType)reader.ReadInt32();

                if (messageLength <= BinaryMessageConstants.MESSAGE_HEADER_LENGTH)
                {
                    continue;
                }

                switch (messageType)
                {
                    case CrdtMessageType.PUT_COMPONENT:
                    case CrdtMessageType.DELETE_COMPONENT:
                        yield return CRDTDeserializer.Deserialize(reader, messageType);
                        break;
                    default:
                        reader.Skip(messageLength - headerLength);
                        break;
                }
            }
        }
    }
}