using KernelCommunication;

namespace DCL.CRDT
{
    public static class CRDTSerializer
    {
        public static void Serialize(BinaryWriter binaryWriter, CrdtMessage message)
        {
            int crdtMessageLength = CrdtMessage.GetMessageDataLength(message);

            binaryWriter.WriteInt32(crdtMessageLength);
            binaryWriter.WriteInt32((int)message.Type);
            binaryWriter.WriteInt32((int)message.EntityId);

            if (message.Type == CrdtMessageType.PUT_COMPONENT ||
                message.Type == CrdtMessageType.APPEND_COMPONENT)
            {
                binaryWriter.WriteInt32(message.ComponentId);
                binaryWriter.WriteInt32(message.Timestamp);

                byte[] data = (byte[])message.Data;
                int dataLength = data?.Length ?? 0;

                binaryWriter.WriteInt32(dataLength);
                if (dataLength > 0)
                {
                    binaryWriter.WriteBytes(data);
                }
            }

            if (message.Type == CrdtMessageType.DELETE_COMPONENT) {
                binaryWriter.WriteInt32(message.ComponentId);
                binaryWriter.WriteInt32(message.Timestamp);
            }
        }
    }
}
