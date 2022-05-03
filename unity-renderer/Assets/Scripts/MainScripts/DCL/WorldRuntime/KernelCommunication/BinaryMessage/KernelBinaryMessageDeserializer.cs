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
                int messageType = reader.ReadInt32();

                if (messageLength == 0)
                {
                    continue;
                }

                switch ((KernelBinaryMessageType)messageType)
                {
                    case KernelBinaryMessageType.PUT_COMPONENT:
                    case KernelBinaryMessageType.DELETE_COMPONENT:
                        yield return CRDTDeserializer.Deserialize(reader);
                        break;
                    default:
                        reader.Skip(messageLength - headerLength);
                        break;
                }
            }
        }
    }
}