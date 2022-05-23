using KernelCommunication;

namespace DCL.CRDT
{
    public static class CRDTSerializer
    {
        public static void Serialize(BinaryWriter binaryWriter, CRDTMessage message)
        {
            byte[] data = (byte[])message.data;
            int entityId = CRDTUtils.EntityIdFromKey(message.key);
            int componentId = CRDTUtils.ComponentIdFromKey(message.key);
            int dataLength = data?.Length ?? 0;

            binaryWriter.WriteInt32(entityId);
            binaryWriter.WriteInt32(componentId);
            binaryWriter.WriteInt64(message.timestamp);
            binaryWriter.WriteInt32(dataLength);

            if (dataLength > 0)
            {
                binaryWriter.WriteBytes(data);
            }
        }
    }
}