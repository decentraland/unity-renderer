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
}