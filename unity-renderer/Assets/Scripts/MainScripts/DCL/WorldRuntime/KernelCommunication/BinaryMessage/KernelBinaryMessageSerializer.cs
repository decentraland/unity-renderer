using DCL.CRDT;

namespace KernelCommunication
{
    public static class KernelBinaryMessageSerializer
    {
        public static void Serialize(BinaryWriter binaryWriter, CRDTMessage message)
        {
            //sizeof(messageHeader) + sizeof(componentHeader) + dataLength
            int dataLength = (message.data as byte[])?.Length ?? 0;
            int messageLength = BinaryMessageConstants.CRDT_MESSAGE_BASE_HEADER_LENGTH + dataLength;
            int type = GetCRDTMessageType(message);

            binaryWriter.WriteInt32(messageLength);
            binaryWriter.WriteInt32(type);

            CRDTSerializer.Serialize(binaryWriter, message);
        }

        public static void Serialize(BinaryWriter binaryWriter, CRDTProtocol crdt)
        {
            var crdtStateIterator = crdt.GetState();
            while (crdtStateIterator.MoveNext())
            {
                Serialize(binaryWriter, crdtStateIterator.Current);
            }
        }

        private static int GetCRDTMessageType(CRDTMessage message)
        {
            if (message.data == null)
                return (int)KernelBinaryMessageType.DELETE_COMPONENT;
            return (int)KernelBinaryMessageType.PUT_COMPONENT;
        }
    }
}