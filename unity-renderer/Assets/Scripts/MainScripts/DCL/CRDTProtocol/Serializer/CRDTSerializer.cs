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

            binaryWriter.Write(entityId);
            binaryWriter.Write(componentId);
            binaryWriter.Write(message.timestamp);
            binaryWriter.Write(dataLength);

            if (dataLength > 0)
            {
                binaryWriter.Write(data);
            }
        }
    }
}