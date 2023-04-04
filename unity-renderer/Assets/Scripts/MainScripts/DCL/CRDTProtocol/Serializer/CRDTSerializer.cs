using KernelCommunication;

namespace DCL.CRDT
{
    public static class CRDTSerializer
    {
        public static void Serialize(BinaryWriter binaryWriter, CRDTMessage message)
        {
            int crdtMessageLength = CRDTMessage.GetMessageDataLength(message);

            binaryWriter.WriteInt32(crdtMessageLength);
            binaryWriter.WriteInt32((int)message.type);
            binaryWriter.WriteInt32((int)message.entityId);

            if (message.type == CrdtMessageType.PUT_COMPONENT ||
                message.type == CrdtMessageType.APPEND_COMPONENT)
            {
                binaryWriter.WriteInt32(message.componentId);
                binaryWriter.WriteInt32(message.timestamp);

                byte[] data = (byte[])message.data;
                int dataLength = data?.Length ?? 0;

                binaryWriter.WriteInt32(dataLength);
                if (dataLength > 0)
                {
                    binaryWriter.WriteBytes(data);
                }
            }

            if (message.type == CrdtMessageType.DELETE_COMPONENT) {
                binaryWriter.WriteInt32(message.componentId);
                binaryWriter.WriteInt32(message.timestamp);
            }
        }
    }
}
