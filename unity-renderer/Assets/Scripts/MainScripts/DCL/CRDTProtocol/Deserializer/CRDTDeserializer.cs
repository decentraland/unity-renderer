using System;
using System.Collections.Generic;
using DCL.CRDT.BinaryReader;

namespace DCL.CRDT
{
    // Deserialize CRDT binary messages (BigEndian)
    public static class CRDTDeserializer
    {
        internal static readonly CRDTMessageHeader messageHeader = new CRDTMessageHeader();
        internal static readonly CRDTComponentMessageHeader componentHeader = new CRDTComponentMessageHeader();

        public static IEnumerator<CRDTMessage> Deserialize(IntPtr ptr, int length)
        {
            return Deserialize(new UnmanagedMemoryReader(ptr, length));
        }

        public static IEnumerator<CRDTMessage> Deserialize(byte[] bytes)
        {
            return Deserialize(new ByteArrayReader(bytes));
        }

        public static IEnumerator<CRDTMessage> Deserialize(IBinaryReader dataReader)
        {
            while (dataReader.CanRead())
            {
                messageHeader.length = dataReader.ReadInt32();
                messageHeader.type = dataReader.ReadInt32();

                componentHeader.entityId = dataReader.ReadInt32();
                componentHeader.componentClassId = dataReader.ReadInt32();
                componentHeader.timestamp = dataReader.ReadInt64();
                componentHeader.dataLength = dataReader.ReadInt32();

                byte[] data = null;
                if (componentHeader.dataLength > 0)
                {
                    data = dataReader.ReadBytes(componentHeader.dataLength);
                }

                yield return new CRDTMessage()
                {
                    key = $"{componentHeader.entityId}.{componentHeader.componentClassId}",
                    timestamp = componentHeader.timestamp,
                    data = data
                };
            }
        }
    }
}