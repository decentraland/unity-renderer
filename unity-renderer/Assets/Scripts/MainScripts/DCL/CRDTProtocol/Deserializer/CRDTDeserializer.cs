using KernelCommunication;

namespace DCL.CRDT
{
    // Deserialize CRDT binary messages (BigEndian)
    public static class CRDTDeserializer
    {
        internal static readonly CRDTComponentMessageHeader componentHeader = new CRDTComponentMessageHeader();

        public static CRDTMessage Deserialize(IBinaryReader dataReader)
        {
            componentHeader.entityId = dataReader.ReadInt32();
            componentHeader.componentClassId = dataReader.ReadInt32();
            componentHeader.timestamp = dataReader.ReadInt64();
            componentHeader.dataLength = dataReader.ReadInt32();

            byte[] data = null;
            if (componentHeader.dataLength > 0)
            {
                data = dataReader.ReadBytes(componentHeader.dataLength);
            }

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