using DCL.CRDT;

namespace KernelCommunication
{
    public static class KernelBinaryMessageSerializer
    {
        public static void Serialize(BinaryWriter binaryWriter, CRDTMessage message)
        {
            //sizeof(messageHeader) + sizeof(componentHeader) + dataLength
            int messageLength = 32 + (((byte[])message.data)?.Length ?? 0);
            var type = message.data != null ? KernelBinaryMessageType.PUT_COMPONENT : KernelBinaryMessageType.DELETE_COMPONENT;

            binaryWriter.Write(messageLength);
            binaryWriter.Write((int)type);

            CRDTSerializer.Serialize(binaryWriter, message);
        }
    }
}