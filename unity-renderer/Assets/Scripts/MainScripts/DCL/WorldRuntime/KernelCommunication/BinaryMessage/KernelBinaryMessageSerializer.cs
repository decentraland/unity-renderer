using DCL.CRDT;

namespace KernelCommunication
{
    public static class KernelBinaryMessageSerializer
    {
        public static void Serialize(BinaryWriter binaryWriter, CRDTMessage message)
        {
            //sizeof(messageHeader) + sizeof(componentHeader) + dataLength
            int messageLength = BinaryMessageConstants.CRDT_MESSAGE_BASE_HEADER_LENGTH + (((byte[])message.data)?.Length ?? 0);
            var type = GetCRDTMessageType(message);

            binaryWriter.Write(messageLength);
            binaryWriter.Write((int)type);

            CRDTSerializer.Serialize(binaryWriter, message);
        }

        private static KernelBinaryMessageType GetCRDTMessageType(CRDTMessage message)
        {
            if (message.data == null)
                return KernelBinaryMessageType.DELETE_COMPONENT;
            return KernelBinaryMessageType.PUT_COMPONENT;
        }
    }
}