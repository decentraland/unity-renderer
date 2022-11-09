namespace DCL.CRDT
{
    internal class CRDTComponentMessageHeader
    {
        public int entityId;
        public int componentClassId;
        public long timestamp;
        public int dataLength;
    }

    public enum CrdtMessageType
    {
        PUT_COMPONENT = 1,
        DELETE_COMPONENT = 2
    }

    public static class CrdtConstants
    {
        public const int MESSAGE_HEADER_LENGTH = 8;
        public const int CRDT_HEADER_LENGTH = 20;

        public const int CRDT_MESSAGE_BASE_HEADER_LENGTH = MESSAGE_HEADER_LENGTH + CRDT_HEADER_LENGTH;
    }
}